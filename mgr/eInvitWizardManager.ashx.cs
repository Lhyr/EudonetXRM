using System;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Xml;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// met à jour les diférentes parties de l'assistant de sélection ++
    /// </summary>
    public class eInvitWizardManager : eEudoManager
    {
        XmlNode _baseResultNode;

        /// <summary>ACTION_LOADINVIT = 0</summary>
        const int ACTION_LOADINVIT = 0;
        /// <summary>ACTION_SELECTINVIT = 1</summary>
        const int ACTION_SELECTINVIT = 1;
        /// <summary>ACTION_SELECTALLINVIT = 2</summary>
        const int ACTION_SELECTALLINVIT = 2;
        /// <summary> ACTION_UNSELECTINVIT = 3</summary>
        const int ACTION_UNSELECTINVIT = 3;
        /// <summary>ACTION_UNSELECTALLINVIT = 4</summary>
        const int ACTION_UNSELECTALLINVIT = 4;
        /// <summary>ACTION_SELECTALLINVIT_NODBL = 5</summary>
        const int ACTION_SELECTALLINVIT_NODBL = 5;
        /// <summary>ACTION_CONFIRM_AUTO = 6</summary>
        const int ACTION_CONFIRM_AUTO = 6;
        /// <summary>Pour Rafraichir la liste des type de campagne</summary>
        const int ACTION_RELOAD_CAMPAIGN_TYPE = 7;

        private Int32 _nNbInvit = 0;
        private Int32 _nNbAdr = 0;
        private Int32 _nNbPP = 0;

        // Table des invitations
        Int32 _nTabInvit = 0;

        // Table de l'event parent
        Int32 _nTabFrom = 0;

        protected override void ProcessManager()
        {
            
            String sError = "";



            // BASE DU XML DE RETOUR            
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            _baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(_baseResultNode);

            //Dictionnaire de paramètre
            ExtendedDictionary<String, String> dicParam = new ExtendedDictionary<String, String>();
            LoadCommonParam(dicParam);



            // Paramètres obligatoires


            //Type d'action pour le manager
            Int32 iAction = 0;
            if (!(_allKeys.Contains("action") && Int32.TryParse(_context.Request.Form["action"].ToString(), out iAction)))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "Action"));
                LaunchError();
            }

            if (!dicParam.ContainsKey("bkm"))
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "Bkm"));
                LaunchError();
            }

            Boolean bDeleteMode = (_allKeys.Contains("delete") && _context.Request.Form["delete"].ToString() == "1");

            Int32 nPPId = 0;
            Int32 nAdrId = 0;
            Int32 nTPlId = 0;
            //Actions suivant le type
            switch (iAction)
            {
                case ACTION_LOADINVIT:
                    #region Chargement de la liste des invitations ++/xx


                    LoadFromFilterParam(dicParam);


                    if (!dicParam.ContainsKey("parentevtid"))
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "parentevtid"));
                        LaunchError();
                    }


                    /* PARAMETRES SPECIFIQUES */

                    //Le chargement initiale passe le paramètre init
                    Int32 nInit = 0;
                    if (_allKeys.Contains("init") && Int32.TryParse(_context.Request.Form["init"].ToString(), out nInit))
                    {
                        dicParam.Add("init", nInit.ToString());
                        if (nInit == 1 && _pref.Context.InvitSelectId.ContainsKey(_nTabInvit))
                            _pref.Context.InvitSelectId.Remove(_nTabInvit);

                    }

                    //On nettoie la sélection précédente [MOU #37207]
                    CleanInvitSelection(dicParam);

                    //Retourne le contenu du renderer
                    GetListPP(dicParam);


                    break;
                #endregion
                case ACTION_SELECTINVIT:
                    #region Selection d'un invité

                    if (bDeleteMode)
                    {
                        if ((_allKeys.Contains("tplid") && Int32.TryParse(_context.Request.Form["tplid"].ToString(), out nTPlId)))
                            AddToSelection(nPPId, nAdrId, nTPlId, true, true);             
                        else
                        {
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "PPID et ADRID"));
                            LaunchError();
                        }
                    }
                    else if
                        ((_allKeys.Contains("ppid") && Int32.TryParse(_context.Request.Form["ppid"].ToString(), out nPPId))
                           && (_allKeys.Contains("adrid") && Int32.TryParse(_context.Request.Form["adrid"].ToString(), out nAdrId)))
                    {
                        AddToSelection(nPPId, nAdrId, nTPlId, true, false);

                    }
                    else if (_requestTools.GetRequestFormKeyS("lstInvit").Length > 0)
                    {

                        var def = new[] { new { ppid = 0, adrid = 0 } };
                        var mySelection = JsonConvert.DeserializeAnonymousType(_requestTools.GetRequestFormKeyS("lstInvit"), def);
                        foreach(var t in mySelection)
                        {

                            AddToSelection(t.ppid, t.adrid, nTPlId, true, false);
                        }
                    }
                    else
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "PPID et ADRID"));
                        LaunchError();

                    }


                    break;
                #endregion
                case ACTION_SELECTALLINVIT:
                    #region SELECTION DE TOUS LES INVITES


                    //Vérification des paramètres obligatoire
                    if (!dicParam.ContainsKey("parentevtid"))
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "parentevtid"));
                        LaunchError();
                    }



                    /* PARAMETRES SPECIFIQUES */
                    dicParam.Add("addall", "1");
                    dicParam.Add("deletemode", bDeleteMode ? "1" : "0");

                    LoadFromFilterParam(dicParam);


  

                    AddAllToSelection(dicParam);
                    break;

                #endregion
                case ACTION_UNSELECTINVIT:
                    #region DESELECTION D'UN INVITE

                    if (bDeleteMode)
                    {

                        if ((_allKeys.Contains("tplid") && Int32.TryParse(_context.Request.Form["tplid"].ToString(), out nTPlId)))
                            AddToSelection(nPPId, nAdrId, nTPlId, false, true);
                        else
                        {
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "PPID et ADRID"));
                            LaunchError();
                        }
                    }
                    else if
                        ((_allKeys.Contains("ppid") && Int32.TryParse(_context.Request.Form["ppid"].ToString(), out nPPId))
                           && (_allKeys.Contains("adrid") && Int32.TryParse(_context.Request.Form["adrid"].ToString(), out nAdrId)))
                    {

                        AddToSelection(nPPId, nAdrId, 0, false, false);
                    }
                    else
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "PPID et ADRID"));
                        LaunchError();

                    }
                    break;
                #endregion
                case ACTION_UNSELECTALLINVIT:
                    #region DESELECTION DE TOUS INVITES
                    eInvitSelection ev = eInvitSelection.GetInvitSelection(_pref, _nTabInvit);


                    dicParam.Add("addall", "0");
                    LoadFromFilterParam(dicParam);

                    ev.SelectAllInvit(dicParam);
                    break;
                #endregion
                case ACTION_SELECTALLINVIT_NODBL:
                    #region SELECTION DE TOUS SAUF DOUBLON

                    //Vérification des paramètres obligatoire
                    if (!dicParam.ContainsKey("parentevtid"))
                    {
                        this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "parentevtid"));
                        LaunchError();
                    }

                    #region On déselectionne d'abord toutes les invitations existantes (dont les grisées)
                    eInvitSelection ev2 = eInvitSelection.GetInvitSelection(_pref, _nTabInvit);
                    dicParam.Add("addall", "0");
                    LoadFromFilterParam(dicParam);
                    ev2.SelectAllInvit(dicParam);
                    #endregion

                    #region Puis on resélectionne toutes les invitations, sauf les grisées
                    //Plus besoin de refaire appel à LoadFromFilterParam() ici, cet appel étant fait lors de la désélection ci-dessus
                    //De même, le paramètre addall ayant été ajouté plus haut, il faut modifier l'existant, et non le rajouter en doublon
                    /* PARAMETRES SPECIFIQUES */
                    dicParam["addall"] = "1";
                    //Type de dédoublonnage
                    if (_allKeys.Contains("typdbl") && !String.IsNullOrEmpty(_context.Request.Form["typdbl"]))
                    {
                        Int32 nInvitSelectTypDbl;
                        if (Int32.TryParse(_context.Request.Form["typdbl"].ToString(), out nInvitSelectTypDbl) && (nInvitSelectTypDbl == 200 || nInvitSelectTypDbl == 400))
                            dicParam.Add("invitselecttypdbl", nInvitSelectTypDbl.ToString());
                    }



                    AddAllToSelection(dicParam);
                    #endregion
                    break;
                #endregion
                case ACTION_CONFIRM_AUTO:
                    #region DEMANDE DE CONFIRMATION POUR LA GÉNÉRATION AUTOMATIQUE

                    break;
                #endregion
                case ACTION_RELOAD_CAMPAIGN_TYPE:
                    #region RAFRAÎCHIR LA LISTE DES TYPES DE CAMPAGNES

                    int mediaType = 0;
                    if (_allKeys.Contains("mediaType"))
                        Int32.TryParse(_context.Request.Form["mediaType"].ToString(), out mediaType);


                    XmlNode _campaignTypesNode = xmlResult.CreateElement("campaignTypes");
                    _baseResultNode.AppendChild(_campaignTypesNode);

                    XmlNode _campaignTypeNode = xmlResult.CreateElement("campaignType");
                    //_campaignTypesNode.AppendChild(_campaignTypeNode);

                    XmlNode _campaignTypeValueNode = xmlResult.CreateElement("value");
                    _campaignTypeValueNode.InnerText = "0";
                    _campaignTypeNode.AppendChild(_campaignTypeValueNode);

                    XmlNode _campaignTypeLabelNode = xmlResult.CreateElement("label");
                    _campaignTypeLabelNode.InnerText = eResApp.GetRes(_pref, 8166); //Aucune valeur
                    _campaignTypeNode.AppendChild(_campaignTypeLabelNode);

                    if (mediaType != 0)
                    {
                        foreach (eCatalog.CatalogValue catValue in GetCampaignTypeCatalogValues(mediaType))
                        {
                            _campaignTypeNode = xmlResult.CreateElement("campaignType");
                            _campaignTypesNode.AppendChild(_campaignTypeNode);

                            _campaignTypeValueNode = xmlResult.CreateElement("value");
                            _campaignTypeValueNode.InnerText = catValue.Id.ToString();
                            _campaignTypeNode.AppendChild(_campaignTypeValueNode);

                            _campaignTypeLabelNode = xmlResult.CreateElement("label");
                            _campaignTypeLabelNode.InnerText = catValue.Label;
                            _campaignTypeNode.AppendChild(_campaignTypeLabelNode);
                        }
                    }

                    break;
                #endregion
                default:

                    sError = String.Concat(" Action non implémentée (", iAction, ")");
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), sError);
                    LaunchError();
                    break;
            }

            XmlNode _successNode = xmlResult.CreateElement("success");
            _baseResultNode.AppendChild(_successNode);
            _successNode.InnerText = "1";

            XmlNode _nbElementNode = xmlResult.CreateElement("nbinvit");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = _nNbInvit.ToString();

            _nbElementNode = xmlResult.CreateElement("nbadr");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = _nNbAdr.ToString();

            _nbElementNode = xmlResult.CreateElement("nbpp");
            _baseResultNode.AppendChild(_nbElementNode);
            _nbElementNode.InnerText = _nNbPP.ToString();


            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });


        }


        #region Action du manager

        /// <summary>
        /// Ajoute une adresse à sélection
        /// </summary>
        /// <param name="nPPid">Id du pp</param>
        /// <param name="nAdrid">Id de l'adresse</param>
        /// <param name="bAdd">Ajout/Suppression</param>
        /// <param name="nTplId">Id de l'invitation (cas du mode XX)</param>
        /// <param name="bDeleteMode">Mode suppression</param>
        private void AddToSelection(Int32 nPPid, Int32 nAdrid, Int32 nTplId, Boolean bAdd, Boolean bDeleteMode)
        {


            eInvitSelection ev = eInvitSelection.GetInvitSelection(_pref, _nTabInvit);
            ev.SelectInvit(nPPid, nAdrid, nTplId, bAdd, bDeleteMode);

            _nNbInvit = ev.NbAll;
            _nNbAdr = ev.NbAddress;
            _nNbPP = ev.NbContact;

        }


        /// <summary>
        /// Ajoute une adresse à sélection
        /// </summary>
        /// <param name="dicParam">Dictionnaire des parametre pour l'ajout/suppression en masse</param>
        private void AddAllToSelection(ExtendedDictionary<String, String> dicParam)
        {


            eInvitSelection ev = eInvitSelection.GetInvitSelection(_pref, _nTabInvit);

            if (!ev.SelectAllInvit(dicParam)) // remonte l'exception
                throw new Exception(String.Concat("erreur d'ajout des invitations : ", ev.ErrorMsg), ev.InnerException);

            _nNbInvit = ev.NbAll;
            _nNbAdr = ev.NbAddress;
            _nNbPP = ev.NbContact;
        }


        /// <summary>
        /// Génère une renderer pour une liste de ++/xx a partir d'un filtre/table d'invit (cf construction de dicParam)
        /// </summary>
        /// <param name="dicParam">Dictionnaire des paramètres nécessaire à la constuction de la liste</param>
        private void GetListPP(ExtendedDictionary<String, String> dicParam)
        {
            //Retourne le flux HTML
            String sError = "";


            eRenderer eRet = eInvitWizardRenderer.BuildPPList(_pref, dicParam, out sError);

            if (eRet.ErrorMsg.Length == 0 && eRet.InnerException == null)
                RenderResultHTML(eRet.PgContainer, true);
            else
            {
                StringBuilder sDevMsg = new StringBuilder();
                StringBuilder sUserMsg = new StringBuilder();

                sDevMsg.Append("Erreur sur la page : ").Append(System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1]).Append(Environment.NewLine).Append(eRet.ErrorMsg);

                if (eRet.InnerException != null)
                    sDevMsg.AppendLine(eRet.InnerException.Message).AppendLine(eRet.InnerException.StackTrace);

                sUserMsg.Append(eResApp.GetRes(_pref, 422)).Append("<br>").Append(eResApp.GetRes(_pref, 544));


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    sUserMsg.ToString(),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg.ToString()

                    );

                LaunchError();
            }
        }


        /// <summary>
        /// Si l'utilisateur a choisi l'option :  Ne retenir que les fiches Adresses Active ou/et principale
        /// On supprime les selections ne repondant pas à ce choix.
        /// </summary>
        /// <param name="dicParam"></param>
        private void CleanInvitSelection(ExtendedDictionary<String, String> dicParam)
        {
            Boolean bFilterActive = false, bFilterMain = false;

            if (dicParam.ContainsKey("fltact"))
                bFilterActive = dicParam["fltact"] == "1";

            if (dicParam.ContainsKey("fltprinc"))
                bFilterMain = dicParam["fltprinc"] == "1";

            eInvitSelection.GetInvitSelection(_pref, _nTabInvit).CleanSelection(bFilterActive, bFilterMain);
        }

        /// <summary>
        /// Récupère les valeurs du catalogue Interaction.Type de campagne
        /// </summary>
        /// <returns></returns>
        private List<eCatalog.CatalogValue> GetCampaignTypeCatalogValues(int parentId)
        {
            List<eCatalog.CatalogValue> catalogValues = new List<eCatalog.CatalogValue>();

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                FieldLite typeCampaignField = null;

                IEnumerable<FieldLite> listFields = RetrieveFields.GetEmpty(null)
                                                .AddOnlyThisTabs(new int[] { (int)TableType.INTERACTION })
                                                .AddOnlyThisDescIds(new int[] { (int)InteractionField.TypeCampaign })                                                
                                                .SetExternalDal(dal)
                                                .ResultFieldsInfo(FieldLite.Factory());

                if (listFields.Any())
                    typeCampaignField = listFields.First();

                if (typeCampaignField == null)
                    throw new Exception("GetCampaignTypeCatalogValues error : impossible de récupérer les infos de la rubrique Type de Campagne");

                eCatalog catalog = new eCatalog(dal, _pref, typeCampaignField.Popup, _pref.User, typeCampaignField.PopupDescId);
                catalogValues = catalog.Values.Where(cv => cv.ParentId == parentId).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dal.CloseDatabase();
            }

            return catalogValues;
        }

        #endregion



        #region Chargement des paramètres

        /// <summary>
        /// Charge les paramètres communs aux différents appels
        /// </summary>
        /// <param name="dicParam"></param>
        private void LoadCommonParam(ExtendedDictionary<String, String> dicParam)
        {

            Boolean bDeleteMode = false;


            //Table des fiche a ajouter (toujours 200 pour les ++)
            if (_allKeys.Contains("delete"))
            {
                bDeleteMode = _context.Request.Form["delete"].ToString() == "1";
            }

            dicParam.Add("delete", bDeleteMode ? "1" : "0");

            if (!bDeleteMode)
                dicParam.Add("tab", TableType.PP.GetHashCode().ToString());
            else
            {
                if (_allKeys.Contains("bkm"))
                {
                    dicParam.Add("tab", _context.Request.Form["bkm"].ToString());
                }
                else
                {

                }
            }

            //Signet Invitation
            if (_allKeys.Contains("bkm"))
            {
                Int32.TryParse(_context.Request.Form["bkm"].ToString(), out _nTabInvit);
                dicParam.Add("bkm", _context.Request.Form["bkm"].ToString());
            }


            //descid Event
            if (_allKeys.Contains("tabfrom"))
            {
                Int32.TryParse(_context.Request.Form["tabfrom"].ToString(), out _nTabFrom);
                dicParam.Add("tabfrom", _context.Request.Form["tabfrom"].ToString());
            }



            //Largeur disponible pour la liste
            if (_allKeys.Contains("width"))
                dicParam.Add("width", _context.Request.Form["width"].ToString());

            //Hauteur disponible pour la liste
            if (_allKeys.Contains("height"))
                dicParam.Add("height", _context.Request.Form["height"].ToString());

            //Nombre de ligne
            if (_allKeys.Contains("rows"))
                dicParam.Add("rows", _context.Request.Form["rows"].ToString());

            //Page demandée
            if (_allKeys.Contains("page"))
                dicParam.Add("page", _context.Request.Form["page"].ToString());


            //Parent de l'invitation
            Int32 nParentEvtId;
            if (_allKeys.Contains("filefromid") && !String.IsNullOrEmpty(_context.Request.Form["filefromid"]))
            {
                Int32.TryParse(_context.Request.Form["filefromid"], out nParentEvtId);
                dicParam.Add("parentevtid", nParentEvtId.ToString());
            }
        }


        /// <summary>
        /// Charge les paramètres communs des actions nécessitant un filtre
        /// </summary>
        /// <param name="dicParam"></param>
        private void LoadFromFilterParam(ExtendedDictionary<String, String> dicParam)
        {
            // Filter Id 
            Int32 iFilterId;
            if (_allKeys.Contains("fid") && Int32.TryParse(_context.Request.Form["fid"].ToString(), out iFilterId))
                dicParam.Add("filterid", iFilterId.ToString());
            else
            {
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "FilterId"));
                LaunchError();
            }



            /* PARAMETRES OPTIONNELLLE */
            //Adresse active et principale
            if (_allKeys.Contains("fltprinc") && !String.IsNullOrEmpty(_context.Request.Form["fltprinc"]) && _context.Request.Form["fltprinc"] == "1")
                dicParam.Add("fltprinc", "1");

            if (_allKeys.Contains("fltact") && !String.IsNullOrEmpty(_context.Request.Form["fltact"]) && _context.Request.Form["fltact"] == "1")
                dicParam.Add("fltact", "1");

            if (_allKeys.Contains("donotdbladr") && !String.IsNullOrEmpty(_context.Request.Form["donotdbladr"]) && _context.Request.Form["donotdbladr"] == "1")
                dicParam.Add("donotdbladr", "1");


            //Filtres de campagne/optin/optout
            //flttypeconsent, fltcampaigntype, fltoptin, fltoptout, fltnoopt
            Int32 nTypeConsent = 0;
            if (_allKeys.Contains("flttypeconsent"))
                Int32.TryParse(_context.Request.Form["flttypeconsent"].ToString(), out nTypeConsent);
            dicParam.Add("flttypeconsent", nTypeConsent.ToString());

            Int32 nCampaignType = 0;
            if (_allKeys.Contains("fltcampaigntype"))
                Int32.TryParse(_context.Request.Form["fltcampaigntype"].ToString(), out nCampaignType);
            dicParam.Add("fltcampaigntype", nCampaignType.ToString());

            Int32 nOptIn = 0;
            if (_allKeys.Contains("fltoptin"))
                Int32.TryParse(_context.Request.Form["fltoptin"].ToString(), out nOptIn);
            dicParam.Add("fltoptin", nOptIn.ToString());

            Int32 nOptOut = 0;
            if (_allKeys.Contains("fltoptout"))
                Int32.TryParse(_context.Request.Form["fltoptout"].ToString(), out nOptOut);
            dicParam.Add("fltoptout", nOptOut.ToString());

            if (_allKeys.Contains("fltnoopt") && _context.Request.Form["fltnoopt"].ToString() == "1")
                dicParam.Add("fltnoopt", "1");
        }

        #endregion
    }
}
 