using System;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe permettant d'afficher une fiche en popup
    /// </summary>
    public partial class eFileDisplayer : eEudoPage
    {
        public int Tab = 0;

        /// <summary>
        /// Javascript de onLoad de la page
        /// </summary>
        public String onLoad = String.Empty;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Au chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region CSS par défaut

            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("eModalDialog");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eEditFile");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("grapesjs/grapes.min");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-newsletter");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-webpage");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eFile");
            PageRegisters.AddCss("ePopupFile");
            //PageRegisters.AddCss("ePJ"); //Demande #49063 - CNA - ePJ.css cause des problèmes et n'as pas l'air d'être nécessaire sur cette page
            PageRegisters.AddCss("eTitle");

            if (Request.Browser.Browser == "IE" && Request.Browser.Version == "8.0")
                PageRegisters.AddCss("ie8-styles");

            if (Request.Form.AllKeys.Contains("campaignreadonly"))
                PageRegisters.AddCss("eMailing");

            #endregion

            #region add js

            PageRegisters.AddScript("eColorPicker");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eFile");
            PageRegisters.AddScript("eAutoCompletion");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eLastValuesManager");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eEngine");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eNavBar");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("grapesjs/grapes.min");
            PageRegisters.AddScript("grapesjs/grapesjs-plugin-ckeditor.min");
            PageRegisters.AddScript("grapesjs/grapesjs-blocks-basic.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-newsletter.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-webpage.min");
            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("ePlanning");

            // SMS
            //SHA : correction bug #73 068
            //if (_requestTools.GetRequestFormKeyS("bSms") == "1")
            PageRegisters.AddScript("eSmsing");
            PageRegisters.AddScript("ePerm");
            PageRegisters.AddScript("eMailingTpl");
            PageRegisters.AddScript("eMailingWizard");
            PageRegisters.AddScript("eMailing");
            PageRegisters.AddScript("eWizard");
            
            #endregion

            int? reqIntVal;
            Boolean? reqBoolVal;

            //Int32 Tab = 0;
            Int32 nFileId = 0;

            Boolean bGlobalAffect = false;
            Boolean bClone = false;

            // Paramètres supplémentaires transmis au constructeur de rendu
            ExtendedDictionary<String, Object> param = new ExtendedDictionary<String, Object>();

            if (_requestTools.AllKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                Int32.TryParse(Request.Form["tab"].ToString(), out Tab);

            // L'information de table est obligatoire
            if (Tab == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "tab, fileid"), " (tab = ", Tab, ")")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            String sError = String.Empty;

            // Récupération des informations sur la table (côté EudoQuery)
            eTableLiteFileDisplayer myTable = null;
            try
            {
                myTable = eTableLiteFileDisplayer.GetTableLiteFileDisplayer(Tab, _pref);

            }
            catch (Exception err)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6236),
                        eResApp.GetRes(_pref, 72),
                        String.Concat("Erreur chargement informations de la table ", Tab, " dans eFileDisplayer (ExternalLoadInfo) : ", err.Message)
                    );

                //Arrete le traitement et envoi l'erreur
                try
                {
                    LaunchError();
                }
                catch (eEndResponseException) { Response.End(); }
            }

            if (Request.Form.AllKeys.Contains("mailingType"))
            {
                if (Request.Form["mailingType"] == ((int)EdnType.FILE_SMS).ToString())
                    PageRegisters.AddCss("eSMS");
            }

            //Fiche à ouvrir (0 pour nouvelle)
            if (_requestTools.AllKeys.Contains("fileid") && !String.IsNullOrEmpty(Request.Form["fileid"]))
            {
                Int32.TryParse(Request.Form["fileid"].ToString(), out nFileId);
                param.Add("fileid", nFileId);
            }

            if (_requestTools.AllKeys.Contains("clone"))
            {
                bClone = Request.Form["clone"] == "1";
                param.Add("clone", bClone);
            }

            //Si la valeur du champ principale doit déjà être affectée
            ExtendedDictionary<Int32, String> dicvalues = new ExtendedDictionary<Int32, String>();
            string sDefaultValues = _requestTools.GetRequestFormKeyS("defvalues") ?? "";

            if (!string.IsNullOrEmpty(sDefaultValues))
            {
                try
                {
                    dicvalues = JsonConvert.DeserializeObject<ExtendedDictionary<Int32, String>>(sDefaultValues);
                }
                catch (Exception)
                {

                }
            }

            if ((_requestTools.AllKeys.Contains("mainfieldvalue")) && !string.IsNullOrEmpty(Request.Form["mainfieldvalue"]))
                dicvalues.AddContainsKey(Tab + 1, Request.Form["mainfieldvalue"]);

            #region Récupération des informations sur les fiches parentes à la fiche ouverte

            int? parentPpId = null;
            int? parentPmId = null;
            int? parentEvtId = null;
            int? parentAdrId = null;
            int parentEvtDid = 0;

            // Correctif #50029 commenté 
            //  String parentfilefdlvalue = String.Empty;

            // HLA - On a besoin du tabfrom pour les formules du haut et du milieu
            Int32? nParentTab = _requestTools.GetRequestFormKeyI("parenttab");
            Int32? nParentFileid = _requestTools.GetRequestFormKeyI("parentfileid");
            // BBA - On a besoin du libelle du champ principal de la table parente
            // Correctif #50029 commenté 
            // String nParentfilefldvalue = _requestTools.GetRequestFormKeyS("parentfilefldvalue");
            if (nParentFileid != null && nParentTab != null)
            {
                Int32? nRelationFieldDescId = _requestTools.GetRequestFormKeyI("spclnk");
                if (nRelationFieldDescId != null)
                {
                    parentEvtDid = nRelationFieldDescId.Value;
                    parentEvtId = nParentFileid.Value;
                    dicvalues.AddContainsKey(nRelationFieldDescId.Value, nParentFileid.Value.ToString());
                    //  // Correctif #50029 commenté 
                    /*
                    if (!String.IsNullOrEmpty(nParentfilefldvalue))
                    {
                        parentfilefdlvalue = nParentfilefldvalue;
                        dicvalues.AddContainsKey(nParentFileid.Value, parentfilefdlvalue);
                    }
                    */
                }
                else
                {
                    if (nParentTab.Value == TableType.PP.GetHashCode())
                        parentPpId = nParentFileid.Value;
                    else if (nParentTab.Value == TableType.PM.GetHashCode())
                        parentPmId = nParentFileid.Value;
                    else if (nParentTab.Value == TableType.ADR.GetHashCode())
                        parentAdrId = nParentFileid.Value;
                    else
                    {
                        parentEvtDid = nParentTab.Value;
                        parentEvtId = nParentFileid.Value;
                    }

                    dicvalues.AddContainsKey(nParentTab.Value, nParentFileid.Value.ToString());
                }
            }

            /*
             * CNA - #46844 - La première fois qu'on charge la creation d'une fiche en mode traitement de masse
             * Il ne récupère pas le paramètre globalaffect
             * retrait de la condition else pour récupèrer ce paramètre tout le temps
            */
            //else
            //{
            //Affectation globale
            reqBoolVal = _requestTools.GetRequestFormKeyB("globalaffect");
            if (reqBoolVal != null && reqBoolVal.Value)
            {
                if (nParentTab != 0)
                {
                    bGlobalAffect = Request.Form["globalaffect"].ToString() == "1";
                    param.Add("globalaffect", bGlobalAffect);
                }
            }
            //}

            // Gestion des signets de type invitations
            reqBoolVal = _requestTools.GetRequestFormKeyB("globalinvit");
            if (reqBoolVal != null)
                param.Add("globalinvit", reqBoolVal);
            if (_requestTools.AllKeys.Contains("globalaffect") || _requestTools.AllKeys.Contains("globalinvit"))
            {
                param.AddContainsKey("parenttab", nParentTab);
                eRes res = new eRes(_pref, nParentTab.ToString());
                param.Add("parenttablabel", res.GetRes(nParentTab ?? 0));

            }


            //Ouverture de la fiche en lecture seule(utilisé par les spécifs
            param.Add("readonly", _requestTools.GetRequestFormKeyS("readonly") == "1");

            // Recup des informations des liaisons parentes
            eFileTools.eParentFileId efPrtIdParent = new eFileTools.eParentFileId(parentPpId, parentPmId, parentEvtId, parentAdrId, parentEvtDid);

            // Recup des informations des liaisons parentes par la clé lnkid
            eFileTools.eParentFileId efPrtIdLnk = _requestTools.GetRequestFormLnkIds();

            // On ne donne pas la priorité au param parenttab
            eFileTools.eParentFileId efPrtId = eFileTools.GetParentFileIdMerge(efPrtIdParent, efPrtIdLnk);

            // on récupère les liaison pmid et/ou adrid si elle ne sont pas deja reprises
            if (!efPrtId.IsEmpty)
            {
                if (Tab != TableType.ADR.GetHashCode() && nParentTab == TableType.PP.GetHashCode())
                    efPrtId.LoadPmIdAndAdrIdFromPpId(_pref);

                else if (Tab != TableType.ADR.GetHashCode() && nParentTab == TableType.PM.GetHashCode())
                {
                    // #51320 + 41629 : On ne rattache le PP que s'il existe une seule adresse active rattachée à la PM et si la table est de type TEMPLATE
                    if (myTable.TabType == TableType.TEMPLATE) // TODO voir les test pour faire le ratachement, normalement, que pour les template
                                                               // sauf si certaines options activées
                        efPrtId.LoadPpIdAndAdrIdFromPmId(_pref);
                }
                else if (Tab != TableType.ADR.GetHashCode() && nParentTab == TableType.ADR.GetHashCode())
                    efPrtId.LoadPpIdAndPmIdFromAdrId(_pref);

                efPrtId.LoadAdrIdFromPpIdAndPmId(_pref);
            }

            #endregion

            //Création de l'objet contexte
            eFileTools.eFileContext ef = new eFileTools.eFileContext(efPrtId, _pref.User, Tab, nParentTab ?? 0);
            param.Add("filecontext", ef);

            //Dictionnaires de valeurs de la fiche
            if (dicvalues.Count > 0)
                param.Add("dicvalues", dicvalues);

            reqIntVal = _requestTools.GetRequestFormKeyI("width");
            if (reqIntVal != null)
                param.Add("width", reqIntVal);
            reqIntVal = _requestTools.GetRequestFormKeyI("height");
            if (reqIntVal != null)
                param.Add("height", reqIntVal);

            #region Paramètres spécifiques template E-mail
            bool bMailForward = false;
            if (_requestTools.AllKeys.Contains("mailforward") && Request.Form["mailforward"].Equals("1"))
            {
                bMailForward = true;
                param.Add("mailforward", true);
            }
            else
            {
                param.Add("mailforward", false);
            }
            bool bMailDraft = false;
            if (Request.Form.AllKeys.Contains("maildraft") && Request.Form["maildraft"].Equals("1"))
            {
                bMailDraft = true;
                param.Add("maildraft", true);
            }
            else
            {
                param.Add("maildraft", false);
            }
            string strMailTo = String.Empty;
            if (Request.Form.AllKeys.Contains("mailto"))
            {
                strMailTo = Request.Form["mailto"];
            }
            param.Add("mailto", strMailTo);

            // SMS
            param.Add("ntabfrom", nParentTab);

            #endregion

            #region campagne mail

            //Ouvrir une campagne SMS en lecture seule
            if (Tab == TableType.CAMPAIGN.GetHashCode())
            {
                //Affectation globale
                Boolean bSms = _requestTools.GetRequestFormKeyS("bSms") == "1";
                param.Add("bSms", bSms ? "1" : "0");

                if (bSms)
                {
                    PageRegisters.AddCss("eWizard");
                    ////SHA : correction bug 73 068
                    //PageRegisters.RawScrip.Append(String.Concat(" var textareaevents = {'keyup' : \"oSmsing.UpdateSmsContent\" };", Environment.NewLine, "}"));
                    //PageRegisters.AddScript(String.Concat(" var textareaevents = {'keyup' : \"oSmsing.UpdateSmsContent\" };", Environment.NewLine, "}"));
                }
            }

            /****************************************   MOU  23/04/2014 *************************************************
             * Pour afficher une fiche campaigne en lecture seule et en popup on a deux cas :
             * -- "campaignreadonly = 1" : correspond au mode standard  (une fiche en lecture seule)  
             * -- "campaignreadonly = 2" : correspond à l'e-mail de la campaigne (une partie de la fiche)
             * 
             ************************************************************************************************************/
            if (Request.Form.AllKeys.Contains("campaignreadonly") && !String.IsNullOrEmpty(Request.Form["campaignreadonly"]))
            {
                CAMPAIGN_READONLY_TYPE nReadOnlyType = (CAMPAIGN_READONLY_TYPE)eLibTools.GetNum(Request.Form["campaignreadonly"].ToString());
                //On ajoute le clé que si on veux faire un rendu en lecture seule de la campaigne
                if (nReadOnlyType == CAMPAIGN_READONLY_TYPE.STANDARD || nReadOnlyType == CAMPAIGN_READONLY_TYPE.ONLY_MAIL_FIELDS)
                    param.Add("campaignreadonly", nReadOnlyType.GetHashCode().ToString());
            }


            #endregion


            #region Param du planning


            //Type du planning à ouvrir si depuis histo&create
            if (nFileId == 0 && _requestTools.AllKeys.Contains("ptype") && !String.IsNullOrEmpty(Request.Form["ptype"]))
            {

                int nTypePlanning = eLibTools.GetNum(Request.Form["ptype"].ToString());
                if (nTypePlanning >= 0 && nTypePlanning <= 2)
                    param.Add("ptype", eLibTools.GetNum(Request.Form["ptype"].ToString()));
            }

            if (_requestTools.AllKeys.Contains("openseries") && !String.IsNullOrEmpty(Request.Form["openseries"]))
            {
                param.Add("openseries", eLibTools.GetNum(Request.Form["openseries"].ToString()));
            }

            if (Request.Form.AllKeys.Contains("concerneduser") && !String.IsNullOrEmpty(Request.Form["concerneduser"]))
            {
                int concernedUser = eLibTools.GetNum(Request.Form["concerneduser"].ToString());
                param.Add("concernedUser", concernedUser);
            }

            if (Request.Form.AllKeys.Contains("_parentiframeid") && !String.IsNullOrEmpty(Request.Form["_parentiframeid"]))
            {
                PageRegisters.RawScrip.AppendLine().Append("_parentIframeId = '").Append(Request.Form["_parentiframeid"].ToString()).Append("';");
            }

            if (Request.Form.AllKeys.Contains("planningviewmode") && !String.IsNullOrEmpty(Request.Form["planningviewmode"]))
            {
                int calendarViewMode = eLibTools.GetNum(Request.Form["planningviewmode"].ToString());
                param.Add("planningviewmode", calendarViewMode);
            }

            if (Request.Form.AllKeys.Contains("workhourbegin") && !String.IsNullOrEmpty(Request.Form["workhourbegin"]))
            {
                param.Add("workhourbegin", Request.Form["workhourbegin"].ToString());
            }





            if (Request.Form.AllKeys.Contains("date") && !String.IsNullOrEmpty(Request.Form["date"]))
            {
                String date = string.Empty;

                try
                {
                    date = Request.Form["date"].ToString();
                    string[] aDate = date.Split('-');
                    int nDay = Int32.Parse(aDate[0]);
                    int nMonth = Int32.Parse(aDate[1]);
                    int nYear = Int32.Parse(aDate[2]);
                    int nHour = Int32.Parse(aDate[3]);
                    int nMn = Int32.Parse(aDate[4]);

                    date = string.Concat(nDay, "/", nMonth, "/", nYear, " ", nHour, ":", nMn);
                }
                catch
                {
                    date = string.Empty;
                }

                param.Add("date", date);
            }

            if (Request.Form.AllKeys.Contains("enddate") && !String.IsNullOrEmpty(Request.Form["enddate"]))
            {
                String enddate = string.Empty;

                try
                {
                    enddate = Request.Form["enddate"].ToString();
                    string[] aDate = enddate.Split('-');
                    int nDay = Int32.Parse(aDate[0]);
                    int nMonth = Int32.Parse(aDate[1]);
                    int nYear = Int32.Parse(aDate[2]);
                    int nHour = Int32.Parse(aDate[3]);
                    int nMn = Int32.Parse(aDate[4]);

                    enddate = string.Concat(nDay, "/", nMonth, "/", nYear, " ", nHour, ":", nMn);
                }
                catch
                {
                    enddate = string.Empty;
                }

                param.Add("enddate", enddate);
            }

            #endregion

            //eFileDisplayer est appelé pour l'affichage en popup
            param.Add("popup", true);


            // Type MODIFICATION, CREATION ou CONSULTATION
            // Mode Modification par défaut...
            Int32 nType = eConst.eFileType.FILE_MODIF.GetHashCode();

            // Pour l'affichage d'un template E-mail existant, on utilise le flag "Consultation", inutilisé pour tous les autres types de fichiers
            // Ce n'est qu'une question de sémantique. Pour un template E-mail, on utilise de toute façon deux renderers dédiés (un pour l'affichage
            // d'un mail existant, et un autre pour la création d'un mail) qui ne subiront pas les conséquences liées à l'abandon du mode Consultation
            if ((myTable.EdnType == EdnType.FILE_MAIL || myTable.EdnType == EdnType.FILE_SMS) && !bMailForward && !bMailDraft)
                nType = eConst.eFileType.FILE_CONSULT.GetHashCode();

            if (param.ContainsKey("campaignreadonly"))
                nType = eConst.eFileType.FILE_CONSULT.GetHashCode();

            if (myTable.EdnType == EdnType.FILE_MAIL)
                PageRegisters.AddCss("eMailing");

            if (myTable.EdnType == EdnType.FILE_SMS)
                PageRegisters.AddCss("eSMS"); // TODO SMS

            // Sauf si le FileId n'est pas renseigné, auquel cas on passe en mode CREATION
            if (nFileId == 0)
                nType = eConst.eFileType.FILE_CREA.GetHashCode();

            // KHA Chargement des onload des composants javascript
            // KHA/MCR 40575 Ajout impossible sur IE8. 
            // ajout du set de la variable : nGlobalActiveTab qui utilisé par la function js : getCurrentView() dans eMain.js

            #region Annulation de la dernière saisie
            //CreateLastValueIcon();
            #endregion


            onLoad = String.Concat("try{nGlobalActiveTab = ", Tab, ";updateFile(null, ", Tab, ", ", bMailForward ? 0 : nFileId, ", ", nType, ", false);}catch(exp){alert('onload=>updateFile '+exp.message);}");

            eRenderer efRend = null;
            try
            {
                try
                {
                    efRend = eRendererFactory.CreateFileRenderer((eConst.eFileType)nType, myTable, _pref, param);
                    if (!String.IsNullOrEmpty(efRend.ErrorMsg) || efRend.InnerException != null)
                    {
                        if (efRend.ErrorNumber == QueryErrorType.ERROR_NUM_FILE_NOT_FOUND)
                        {
                            this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6695), eResApp.GetRes(_pref, 6696), eResApp.GetRes(_pref, 6695));
                        }
                        // #48 903 - Si une des rubriques du fichier E-mail n'est pas accessible en écriture, on le remonte à l'utilisateur
                        else if (efRend.ErrorNumber == QueryErrorType.ERROR_NUM_MAIL_FILE_NOT_FOUND)
                        {
                            this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                                eResApp.GetRes(_pref, 2020).Replace("<FIELD>", efRend.ErrorMsg), //Vous n'avez pas les droits suffisants pour effectuer la mise à jour de la rubrique <FIELD>
                                eResApp.GetRes(_pref, 6342), //Veuillez contacter votre administrateur
                                eResApp.GetRes(_pref, 5080)); //Information
                        }
                        else
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 6236),
                                eResApp.GetRes(_pref, 72),
                                String.Concat("Erreur Renderer de FileDisplayer : ", efRend.ErrorMsg, (efRend.InnerException != null ? efRend.InnerException.StackTrace : String.Empty))
                            );
                        }
                        //Arrete le traitement et envoi l'erreur
                        LaunchError();

                        return;
                    }
                }
                catch (EudoException ee)
                {
                    eLibConst.MSG_TYPE level = eLibConst.MSG_TYPE.CRITICAL;
                    switch (ee.ErrorCriticity)
                    {
                        case ERROR_CRITICITY.NOERROR:
                            level = eLibConst.MSG_TYPE.SUCCESS;
                            break;
                        case ERROR_CRITICITY.CRITICAL:
                            level = eLibConst.MSG_TYPE.CRITICAL;
                            break;
                        case ERROR_CRITICITY.WARNING:
                            level = eLibConst.MSG_TYPE.EXCLAMATION;
                            break;
                        case ERROR_CRITICITY.INFOS:
                            level = eLibConst.MSG_TYPE.INFOS;
                            break;
                        default:
                            level = eLibConst.MSG_TYPE.CRITICAL;
                            break;
                    }

                    ErrorContainer = eErrorContainer.GetErrorContainerFromEudoException(level, ee);
                    LaunchError();
                }
                catch (eFileLayout.eFileLayoutException flex)
                {
                    ErrorContainer = flex.ErrorContainer;
                    LaunchError();
                }


            }
            catch (eEndResponseException)
            {
                Response.End();
            }
            catch (ThreadAbortException)
            {

            }


            mainDiv.InnerHtml = efRend.RenderMainPanel().ToString();

            if (!bGlobalAffect && !bClone && myTable != null)
                mainDiv.Attributes.Add("autosv", myTable.AutoSave && nFileId > 0 ? "1" : "0");
        }

        /// <summary>
        /// Affichage de l'icône permettant d'afficher les dernières valeurs saisies 
        /// </summary>
        private void CreateLastValueIcon()
        {
            if (eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.File_CancelLastEntries))
            {
                Panel icon = new Panel();
                icon.CssClass = "btnCancelLastModif icon-undo";
                icon.ID = String.Concat("btnCancelLastModif_", Tab);
                icon.Attributes.Add("onmouseover", "LastValuesManager.openContextMenu(this, " + Tab + ")");
                icon.ToolTip = eResApp.GetRes(_pref, 8223);
                container.Controls.Add(icon);
            }
        }

        /// <summary>
        /// Peut-on activer l'autosuggestion BingMaps ?
        /// </summary>
        public Boolean CanRunBingAutoSuggest()
        {
            return eTools.CanRunBingAutoSuggest(_pref, Request);
        }
    }
}