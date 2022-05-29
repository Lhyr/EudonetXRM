using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;


using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eFileMgr
    /// </summary>
    public class eFileManager : eEudoManagerReadOnly
    {
        private Int32 _tab = 0;


        //Traitemenrt de masse
        Boolean _bGlobalAffect = false;
        Boolean _bWithoutAdr = false;
        Boolean _bGlobalInvit = false;

        //Sur la création d'une adresse, autorise l'écrasement des rubrique d'adress postale
        Boolean _bOwerWritePmAdr = false;

        Int32 _nParentTab = 0;

        Int32 statut = EmailStatus.MAIL_NOT_SENT.GetHashCode();

        Boolean _bModeResume = false;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 nFileId = 0;
            Int32 nType = 0;

            Int32 nTriggerDescid = 0;
            Int32 nPg = 1;


            bool bFromChgAdr = _requestTools.GetRequestFormKeyB("fromchgtype") ?? false;
            Boolean bCompletePage = false;
            Boolean bCroix = false;
            eConst.ParentInfoFormat parentInfoRdrFormat = eConst.ParentInfoFormat.NONE;

            #region param de la page

            if (_requestTools.AllKeys.Contains("fileid") && !String.IsNullOrEmpty(_context.Request.Form["fileid"]))
                Int32.TryParse(_context.Request.Form["fileid"].ToString(), out nFileId);


            //Traitement de masse!
            if (_requestTools.AllKeys.Contains("globalaffect") && !String.IsNullOrEmpty(_context.Request.Form["globalaffect"]))
            {
                _bGlobalAffect = _context.Request.Form["globalaffect"].ToString() == "1";
            }

            if (_requestTools.AllKeys.Contains("parenttab") && !String.IsNullOrEmpty(_context.Request.Form["parenttab"]))
            {
                Int32.TryParse(_context.Request.Form["parenttab"].ToString(), out _nParentTab);
            }

            if (_requestTools.AllKeys.Contains("globalinvit") && !String.IsNullOrEmpty(_context.Request.Form["globalinvit"]))
            {
                _bGlobalInvit = _context.Request.Form["globalinvit"].ToString() == "1";
            }

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(_context.Request.Form["tab"]))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out _tab);

            if (_requestTools.AllKeys.Contains("width") && !String.IsNullOrEmpty(_context.Request.Form["width"]))
                _pref.Context.FileWidth = eLibTools.GetNum(_context.Request.Form["width"].ToString());

            //cas particulier, création d'un PP sans adresse
            if (_requestTools.AllKeys.Contains("withoutadr") && !String.IsNullOrEmpty(_context.Request.Form["withoutadr"]))
                _bWithoutAdr = _context.Request.Form["withoutadr"].ToString() == "1";

            //#37334  - écrasement des adresse postal par l'adr pro
            if (_requestTools.AllKeys.Contains("adrprooverwrite") && !String.IsNullOrEmpty(_context.Request.Form["adrprooverwrite"]))
                _bOwerWritePmAdr = _context.Request.Form["adrprooverwrite"].ToString() == "1";


            //ApplyRulesOnBlank - Champs autorisé a la modif
            String sLstAllowedField = _requestTools.GetRequestFormKeyS("crtldescid") ?? String.Empty;
            if (sLstAllowedField.Length > 0)
            {
                try
                {
                    sLstAllowedField = CryptoAESRijndael
                        .Decrypt(sLstAllowedField, CryptographyConst.KEY_CRYPT_LINK6, CryptographyConst.KEY_CRYPT_LINK1.Substring(0, 16))
                        .TrimEnd('\0');
                }
                catch
                {
                    //Si echec de décryptage, la liste des champs valide est laissée vide
                }
            }

            if (_requestTools.AllKeys.Contains("modeResume") && !String.IsNullOrEmpty(_context.Request.Form["modeResume"]))
                _bModeResume = _context.Request.Form["modeResume"].ToString() == "1";

            // type : eConst.eFileType
            eConst.eFileType efType = eConst.eFileType.FILE_CONSULT;
            if (_requestTools.AllKeys.Contains("type") && !String.IsNullOrEmpty(_context.Request.Form["type"]))
            {
                Int32.TryParse(_context.Request.Form["type"].ToString(), out nType);
                try
                {
                    efType = (eConst.eFileType)nType;
                }
                catch (Exception exp)
                {
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", string.Empty), string.Concat("Type de fichier invalide : ", nType), "", exp.ToString());
                    LaunchError();
                }
            }
            else if (_requestTools.AllKeys.Contains("pformat") && !String.IsNullOrEmpty(_context.Request.Form["pformat"]))
            {
                switch (_context.Request.Form["pformat"].ToString())
                {
                    case "head":
                        parentInfoRdrFormat = eConst.ParentInfoFormat.HEADER;
                        break;
                    case "foot":
                        parentInfoRdrFormat = eConst.ParentInfoFormat.FOOTER;
                        break;
                }
            }
            else
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""));
                LaunchError();
            }

            if (_requestTools.AllKeys.Contains("trgdid") && !String.IsNullOrEmpty(_context.Request.Form["trgdid"]))
                Int32.TryParse(_context.Request.Form["trgdid"].ToString(), out nTriggerDescid);


            if (_requestTools.AllKeys.Contains("pg") && !String.IsNullOrEmpty(_context.Request.Form["pg"]))
                Int32.TryParse(_context.Request.Form["pg"].ToString(), out nPg);

            if (nPg == 0)
                nPg = 1;

            if (_requestTools.AllKeys.Contains("bModal") && !String.IsNullOrEmpty(_context.Request.Form["bModal"]) && _context.Request.Form["bModal"].ToString() == "1")
                bCompletePage = true;

            // indique si la fiche est affichée en popup
            Boolean bPopupDisplay = false;
            //Affichage en popup
            if (_requestTools.AllKeys.Contains("popup")
                && !String.IsNullOrEmpty(_context.Request.Form["popup"])
                && _context.Request.Form["popup"].ToString().Equals("1")
                )
            {
                bPopupDisplay = true;
            }


            bool isBkmFile = _requestTools.GetRequestFormKeyB("bkmfile") ?? false;

            // Afficher ou non la croix
            if (_requestTools.AllKeys.Contains("bCroix") && _context.Request.Form["bCroix"].ToString() == "true")
                bCroix = true;

            #region pour planning

            String date = String.Empty;
            Int32 concernedUser = 0,
                nWidth = 0,
                nHeight = 0;
            if (_requestTools.AllKeys.Contains("date") && !String.IsNullOrEmpty(_context.Request.Form["date"]))
                date = _context.Request.Form["date"].ToString();

            if (_requestTools.AllKeys.Contains("concerneduser") && !String.IsNullOrEmpty(_context.Request.Form["concerneduser"]))
                concernedUser = eLibTools.GetNum(_context.Request.Form["concerneduser"].ToString());

            if (_requestTools.AllKeys.Contains("width") && !String.IsNullOrEmpty(_context.Request.Form["width"]))
                nWidth = eLibTools.GetNum(_context.Request.Form["width"].ToString());
            //SHA : correction bug #72 765
            if (_requestTools.AllKeys.Contains("height") && !String.IsNullOrEmpty(_context.Request.Form["height"]))
                nHeight = eLibTools.GetNum(_context.Request.Form["height"].ToString());
            if (_requestTools.AllKeys.Contains("mainheight") && !String.IsNullOrEmpty(_context.Request.Form["mainheight"]))
                nHeight = eLibTools.GetNum(_context.Request.Form["mainheight"].ToString());

            #endregion

            #endregion

            // Ouverture de connexion
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            eDal.OpenDatabase();

            try
            {
                #region gestion des valeurs temporaires (mode creation et popup)

                ExtendedDictionary<Int32, String> dicValues = new ExtendedDictionary<Int32, String>();
                ExtendedList<eUpdateField> liFldUpdate = new ExtendedList<eUpdateField>();

                ISet<Int32> dicValuesChanged = new HashSet<Int32>();

                //liste les catalogues parents à replacer devant les enfants que la mise à jour en cascade soit effectuée correctement 
                ExtendedDictionary<Int32, Int32> dicChildBeforeParent = new ExtendedDictionary<Int32, Int32>();

                //liste des descids des champs que l'on va modifier dans le cadre du controle d'intégrité parent/enfant sur les catalogues liés
                List<Int32> modifiedFieldsDescid = new List<Int32>();
                if (nTriggerDescid > 0)
                    modifiedFieldsDescid.Add(nTriggerDescid);

                foreach (string s in _allKeys)
                {
                    // parse la chaine transmise pour obtenir les informations du champ
                    // pour plus d'informations cf eEngine.js fldUpdEngine.GetSerialize()
                    if (!s.StartsWith("field"))
                        continue;

                    String sFieldParam = _context.Request.Form[s];
                    eUpdateField updField = eUpdateField.GetUpdateFieldFromDesc(sFieldParam);

                    // Ajout au dico
                    if (updField != null)
                    {
                        dicValues.AddContainsKey(updField.Descid, updField.NewValue);

                        // si le parent du catalogue ne se trouve pas encore dans le dictionnaire recensant les champs 
                        // c'est qu'il faudra le remonter dans la liste lorsqu'il se présentera.
                        if (updField.BoundDescid > 0 && updField.BoundPopup != PopupType.NONE)
                        {
                            eUpdateField parentUpdField = liFldUpdate.Find(
                                    delegate (eUpdateField tmpUpdField)
                                    {
                                        return tmpUpdField.Descid == updField.BoundDescid;
                                    }
                                );

                            if (parentUpdField == null)
                                dicChildBeforeParent.AddContainsKey(updField.BoundDescid, updField.Descid);
                        }

                        // on teste si le catalogue fait partie des parents à remonter
                        if (dicChildBeforeParent.ContainsKey(updField.Descid))
                        {
                            Int32 childDescid = 0;
                            dicChildBeforeParent.TryGetValue(updField.Descid, out childDescid);

                            eUpdateField childUpdField = liFldUpdate.Find(
                                   delegate (eUpdateField tmpUpdField)
                                   {
                                       return tmpUpdField.Descid == childDescid;
                                   });

                            Int32 childPosition = liFldUpdate.IndexOf(childUpdField);

                            liFldUpdate.InsertContains(childPosition, updField);
                        }
                        else
                        {
                            liFldUpdate.Add(updField);
                        }
                    }
                }

                //on vérifie que les valeurs sélectionnées sont compatibles avec les valeurs parentes des catalogues liés
                foreach (eUpdateField updField in liFldUpdate)
                {

                    if (updField.ChangedValue == null || updField.ChangedValue.Value)
                        dicValuesChanged.Add(updField.Descid);

                    if (updField.BoundDescid == 0 || updField.BoundPopup == PopupType.NONE)
                        continue;

                    // si le catalogue parent a été modifié
                    if (!modifiedFieldsDescid.Contains(updField.BoundDescid))
                        continue;

                    eUpdateField parentUpdField = liFldUpdate.Find(
                            delegate (eUpdateField tmpUpdField)
                            {
                                return tmpUpdField.Descid == updField.BoundDescid;
                            }
                        );

                    if (parentUpdField == null)
                        continue;

                    Int32 catParentId = eCatalog.GetCatalogValueID(_pref, parentUpdField.Popup, parentUpdField.PopupDescId, parentUpdField.NewValue, eDal, _pref.User);

                    eCatalog.CatalogValue catVal = new eCatalog.CatalogValue();
                    catVal.ParentId = catParentId;

                    bool isTreeView = updField.PopupDataRend == PopupDataRender.TREE;
                    eCatalog catalog = new eCatalog(eDal, _pref, updField.Popup, _pref.User, updField.PopupDescId, isTreeView, catVal, isSnapshot: _pref.IsSnapshot);

                    // on verifie si le parent n'a pas qu'une seule valeur enfant auquel cas il faut la sélectionner automatiquement
                    if (catalog.Values.Count == 1)
                    {
                        dicValues[updField.Descid] = catalog.Values[0].DbValue;
                        updField.NewValue = catalog.Values[0].DbValue;
                        updField.NewDisplay = catalog.Values[0].Label;
                        modifiedFieldsDescid.Add(updField.Descid);
                    }
                    else if (updField.NewValue.Length > 0)
                    {
                        // on verifie que la valeur actuelle est  compatible avec le parent sélectionné
                        catVal.Label = updField.NewDisplay;
                        catVal.DbValue = updField.NewValue;

                        catVal = catalog.Find(catVal);

                        // si ce n'est pas le cas on vide le champ
                        if (catVal == null)
                        {
                            dicValues[updField.Descid] = String.Empty;
                            updField.NewValue = String.Empty;
                            updField.NewDisplay = String.Empty;
                            modifiedFieldsDescid.Add(updField.Descid);
                        }
                    }
                }

                #endregion

                //
                if (_tab <= 0)
                {
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", string.Empty), string.Concat("Table invalide : ", _tab));
                    LaunchError();
                }

                //Mode d'affichage
                String tableLiteErr = String.Empty;
                TableLite tableLite = new TableLite(_tab);
                if (!tableLite.ExternalLoadInfo(eDal, out tableLiteErr))
                {
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "tableLite.ExternalLoadInfo", tableLiteErr);
                    LaunchError();
                }

                //Si le mode consultation n'est pas de type VCARD et qu'il est de type PP, PM, ou EVENT, la mise à jour des MRU se fait bien.
                if (efType != eConst.eFileType.FILE_VCARD && (tableLite.TabType == TableType.PP || tableLite.TabType == TableType.PM || tableLite.TabType == TableType.EVENT))
                {
                    UpdateMRU(eDal, _tab, nFileId);
                }

                //Statut de mail
                if (dicValues.ContainsKey(_tab + MailField.DESCID_MAIL_STATUS.GetHashCode()))
                    statut = eLibTools.GetNum(dicValues[_tab + MailField.DESCID_MAIL_STATUS.GetHashCode()]);

                // TOCHECK SMS
                //(tpl85 de Label fiche nfiledId )
                if ((tableLite.EdnType == EdnType.FILE_MAIL || tableLite.EdnType == EdnType.FILE_SMS) && nFileId > 0 && (EmailStatus)statut != EmailStatus.MAIL_DRAFT)
                    efType = eConst.eFileType.FILE_CONSULT;

                ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();
                param.Add("fileid", nFileId);
                param.Add("dicvalues", dicValues);
                param.Add("dicvalueschanged", dicValuesChanged);

                param.Add("blankallowedfield", sLstAllowedField);

                param.Add("modeResume", _bModeResume);

                // VCard
                param.Add("npg", nPg);
                param.Add("bCroix", bCroix); // Indique si on affiche ou non la croix pour fermer la VCard

                // Planning
                param.Add("date", date);
                param.Add("ispostback", true);
                param.Add("concernedUser", concernedUser);
                param.Add("width", nWidth);
                //SHA : correction bug #72 765
                param.Add("height", nHeight);
                param.Add("mainheight", nHeight);
                param.Add("globalaffect", _bGlobalAffect);
                param.Add("globalinvit", _bGlobalInvit);
                param.Add("parenttab", _nParentTab);
                param.Add("popup", bPopupDisplay);
                param.Add("bkmfile", isBkmFile);
                param.Add("withoutadr", _bWithoutAdr);
                param.Add("fromchgadr", bFromChgAdr);
                if (nTriggerDescid > 0)
                    param.Add("trgdid", nTriggerDescid);

                eRes res = new eRes(_pref, _nParentTab.ToString());
                param.Add("parenttablabel", res.GetRes(_nParentTab));

                //adresse
                //#37334  - écrasement des adresse postal par l'adr pro
                if (_requestTools.AllKeys.Contains("adrprooverwrite") && !String.IsNullOrEmpty(_context.Request.Form["adrprooverwrite"]))
                    param.Add("adrprooverwrite", _context.Request.Form["adrprooverwrite"].ToString() == "1");

                eRenderer efRend = null;
                if (parentInfoRdrFormat == eConst.ParentInfoFormat.NONE)
                {
                    eFileTools.eParentFileId efPrtId = new eFileTools.eParentFileId();
                    eFileTools.eFileContext ef = new eFileTools.eFileContext(efPrtId, _pref.User, _tab, _nParentTab);

                    //SPH : a priori, ce cas se produit lorsqu'une page est rechargé
                    // on passe le ispostback a true du filecontext pour pouvoir le prendre
                    // en compte lors de la reprise des valeur par défaut
                    ef.IsPostBack = true;
                    param.Add("filecontext", ef);
                    param.Add("maildraft", this.statut);
                    efRend = eRendererFactory.CreateFileRenderer(efType, tableLite, _pref, param);
                }
                else
                {
                    eFileHeader fileParentInfo = eFileHeader.CreateFileHeader(_pref, _tab, nFileId, dicValues);

                    switch (parentInfoRdrFormat)
                    {

                        case eConst.ParentInfoFormat.HEADER:
                            efRend = eRendererFactory.CreateParenttInHeadRenderer(_pref, fileParentInfo);
                            break;
                        case eConst.ParentInfoFormat.FOOTER:
                            efRend = eRendererFactory.CreateParenttInFootRenderer(_pref, fileParentInfo);
                            break;
                    }
                }

                if (efRend != null && efRend.ErrorMsg.Length == 0)
                    RenderResult(efRend.PgContainer, bCompletePage);
                else
                {
                    if (efRend.ErrorNumber == QueryErrorType.ERROR_NUM_FILE_NOT_FOUND)
                    {
                        this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6695), eResApp.GetRes(_pref, 6696), eResApp.GetRes(_pref, 6695));
                    }
                    else
                    {
                        string strErr = string.Empty;
                        if (efRend != null)
                            strErr = efRend.ErrorMsg;
                        else
                            strErr = "efRend a la valeur null";

                        String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, strErr);

                        if (efRend.InnerException != null)
                            sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", efRend.InnerException.Message, Environment.NewLine, "Exception StackTrace :", efRend.InnerException.StackTrace);


                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""), "", "", sDevMsg);
                    }
                    LaunchError();
                }
            }
            finally
            {
                eDal.CloseDatabase();
            }
        }


        /// <summary>
        /// Met à jour les MRU de dernières fiches consultées.
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="nTab">Table du mru</param>
        /// <param name="nFileId"></param>
        protected void UpdateMRU(eudoDAL eDal, Int32 nTab, Int32 nFileId)
        {
            // Maj MRU
            eParam eParam = new eParam(_pref);
            String sMajMRU = String.Empty;
            if (!eParam.SetTableMru(eDal, nTab, nFileId, out sMajMRU))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "Un problème est survenu lors de la mise à jour des MRU", sMajMRU);
                LaunchError();
            }
        }



        /// <summary>
        /// Rendu HTML
        /// </summary>
        /// <param name="monPanel">Div à afficher dans la page</param>
        /// <param name="bCompletePage">indique si on rend un contenu html complet ou juste le contenu d'une balise</param>
        private void RenderResult(Panel monPanel, Boolean bCompletePage)
        {
            string sHeaderScript = "<script type = 'text/javascript' language = 'javascript' src='{0}'></script>";
            string sHeaderCSS = "<link type='text/css' rel='stylesheet' href='{0}'></link>";
            string sVer = $"?ver={eConst.VERSION}.{eConst.REVISION}";

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);

            monPanel.RenderControl(tw);


            if (bCompletePage)
            {
                //Dans le cas de la vcard, il faut construire l'enveloppe html
                // la vcard étant affichée dans une eModalDialog
                // MAB/ELAIZ - 2019-10-15 - US #1074 - Backlog #1660 - VCard : On précise le TabID de l'onglet en cours pour différencier en CSS
                /** Remplacement String.Concat par StringBuilder, avec formatage et chaines d'interpolations. G.L */

                StringBuilder sbHeader = new StringBuilder("<html><head>");
                sbHeader.AppendFormat(sHeaderCSS, $"themes/default/css/eMain.css{sVer}");
                sbHeader.AppendFormat(sHeaderCSS, $"themes/default/css/eFile.css{sVer}");
                sbHeader.AppendFormat(sHeaderCSS, $"themes/default/css/eVcard.css{sVer}");
                sbHeader.AppendFormat(sHeaderCSS, $"themes/default/css/eList.css{sVer}");

                /** Si le thème est en version 2 ou + on met une feuille de style générique. G.L */
                if (_pref.ThemeXRM.Version > 1)
                {
                    sbHeader.AppendFormat(sHeaderCSS, $"{eTools.WebPathCombine("themes", "Theme2019", "css", "theme.css")}{sVer}");
                }

                sbHeader.AppendFormat(sHeaderCSS, $"{eTools.WebPathCombine("themes", _pref.ThemeXRM.Folder, "css", "theme.css")}{sVer}");
                sbHeader.AppendFormat(sHeaderCSS, $"themes/default/css/eudoFont.css{sVer}");

                sbHeader.AppendFormat(sHeaderScript, $"scripts/eFile.js{sVer}");
                sbHeader.AppendFormat(sHeaderScript, $"scripts/eAutoCompletion.js{sVer}");
                sbHeader.AppendFormat(sHeaderScript, $"scripts/eUpdater.js{sVer}");
                sbHeader.AppendFormat(sHeaderScript, $"scripts/eTools.js{sVer}");
                sbHeader.AppendFormat(sHeaderScript, $"scripts/eMain.js{sVer}");

                sbHeader.AppendFormat("</head><body class='VCardTT' id='VCardTT' tab='{0}'>", _tab);
                sbHeader.Append(sb);
                sbHeader.Append("</body></html>");

                _context.Response.Clear();
                _context.Response.Write(sbHeader.ToString());
            }
            else
            {
                _context.Response.Clear();
                _context.Response.Write(sb.ToString());
            }
        }
    }
}