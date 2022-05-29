using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// GCH : Page de rendu du catalogue utilisateur
    /// </summary>
    public partial class eCatalogDialogUser : eEudoPage
    {
        /// <summary>Objet connexion</summary>
        eudoDAL _dal;
        /// <summary>Nom de la modal</summary>
        public String _parentModalVarName = String.Empty;

        /// <summary>id de l'élément iFrame présentant le contenu de la modalDialog</summary>
        public String _iFrameId = String.Empty;
        /// <summary>Indique s'il s'agit d'un catalogue Utilisateur multiple ou simple</summary>
        public Boolean _multiple = false;
        /// <summary>
        /// Hauteur de la modale
        /// </summary>
        public int _height = 0;
        /// <summary>Descid du catalogue Utilisateur</summary>
        public Int32 _nDescId = 0;
        /// <summary>Indique si l'on doit afficher le nom de l'utilisateur en cours en en-tête ou non</summary>
        public Boolean _showCurrentUser = false;
        /// <summary>Indique si l'on doit afficher "utilisateur en cours" en en-tête ou non</summary>
        public Boolean _showCurrentUserFilter = false;
        /// <summary>Indique si l'on doit afficher "groupe en cours" en en-tête ou non</summary>
        public Boolean _showCurrentGroupFilter = false;
        /// <summary>Indique si l'on doit afficher tous les utilisateurs en passant outre la gestion des droits de la base</summary>
        public Boolean _fullUserList = false;
        /// <summary>Indique si l'on doit afficher les Groupe sans utilisateurs</summary>
        public Boolean _showEmptyGroup = false;
        /// <summary>Indique si l'on doit afficher les utilisateurs seulement (sans les groupes)</summary>
        public Boolean _showUserOnly = false;
        /// <summary>Permet d'autoriser la sélection d'un gorupe en catalogue à choix simple</summary>
        public Boolean _useGroup = false;
        /// <summary>Indique si l'on doit afficher "Valeurs Publics" (correspondant à vide) en en-tête ou non (seulement pour le simple)</summary>
        public Boolean _showValuePublicRecord = false;
        /// <summary>Indique si l'on doit afficher "VIDE" en en-tête ou non (seulement pour le simple)</summary>
        public Boolean _showValueEmpty = false;
        /// <summary>Indique si on doit sélectionner tous les utilisateurs à l'ouverture de la liste</summary>
        public Boolean _selectAllUsers = false;
        /// <summary>Affiche-t-on l'option "Tous les utilisateurs" ?</summary>
        public Boolean _showAllUsersOption = false;
        /// <summary>Indique le mode d'affichage ("Tous les utilisateurs sélectionnés", "non sélectionnés", etc...) ?</summary>
        public String _selectDisplayMode = "0";

        //classe css à rajouter sur le corps
        public String BodyCSS = "";

        /// <summary>
        /// Indique si on doit ajouter l'entrer "valeur par défaut (pour les préférences)
        /// </summary>
        public bool _showDefaultValue;

        public bool _showAllValue;

        /// <summary>
        /// Indique s'il faut n'afficher que les utilisateurs de type profil
        /// </summary>
        public bool _bOnlyProfil;


        /// <summary>
        /// Indique s'il faut afficher les utilisateurs de type profil
        /// </summary>
        public bool _bDisplayProfil;

        /// <summary>
        /// Indique si l'utilisateur configure dans ses préférences My Eudonet
        /// </summary>
        public bool _bIsMyEudonet;

        /// <summary>Indique le type d'action réalisée : si on ouvre le catalogue pour la première fois ou si l'on rafraichi le catalogue (pour une recherche par exemple) (voir DialogAction)</summary>
        private String action = String.Empty;

        /// <summary>Objet Utilisateur contenant les informations de l'utilisateur en cours</summary>
        private eUser _eUser;


        protected Boolean _bFromTreat = false;

        /// <summary>
        /// Type d'action pouvant être réalisée réalisée : Catalogue classique ou rafraichissement pour une recherche.
        /// </summary>
        private enum DialogAction
        {
            SHOW_DIALOG,
            REFRESH_DIALOG,
            MRU,
            NONE
        }


        /// <summary>Chaine de recherche de effectué par l'utilisateur</summary>
        private String _catSearch = String.Empty;

        /// <summary>Javascript à executer sur la page côté client.</summary>
        private StringBuilder _sbInitJSOutput = new StringBuilder();

        /// <summary>Indique le type d'action réalisée : si on ouvre le catalogue pour la première fois ou si l'on rafraichi le catalogue (pour une recherche par exemple) (voir DialogAction)</summary>
        private DialogAction DlgAction
        {
            get
            {
                switch (action.ToUpper())
                {
                    case "REFRESH_DIALOG":
                        return DialogAction.REFRESH_DIALOG;
                    case "MRU":
                        return DialogAction.MRU;
                    case "SHOW_DIALOG":
                    default:
                        return DialogAction.SHOW_DIALOG;
                };
            }
        }

        /// <summary>Javascript à executer sur la page côté client.</summary>        
        public String InitJSOutput
        {
            get { return _sbInitJSOutput.ToString(); }
        }


        /// <summary>Pour un renvoi AJAX, retour de sortie</summary>
        XmlDocument _xmlResult = new XmlDocument();

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Dialogue de choix pour les champs de type User
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eCatalog", "all");
            PageRegisters.AddCss("eControl", "all");

            #endregion

            mainDiv.Attributes["class"] += string.Concat(" ", eTools.GetClassNameFontSize(_pref));

            #region ajout des scripts

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTreeView");
            PageRegisters.AddScript("eCatalogUser");

            #endregion

            StringBuilder sbError = new StringBuilder();

            // Liste des id et GroupeId sélectionnée
            List<String> _selectedList = new List<string>();

            XmlNode detailsNode = null;

            _dal = eLibTools.GetEudoDAL(_pref);

            //TODO : gérer la connexion différement + gestion erreur
            try
            {
                #region Initialisation

                _dal.OpenDatabase();

                String selectedIds = string.Empty;

                if (Request.Form["selected"] != null)
                    selectedIds = Request.Form["selected"].ToString();

                //if (selectedIds == "0")
                //{
                //    _selectAllUsers = true;
                //    _sbInitJSOutput.Append("eCU.OnChkSelectAll(true);");
                //}

                else
                {
                    foreach (String s in eLibTools.CleanIdList(selectedIds).Split(';').Distinct())
                        _selectedList.Add(s);
                }



                if (Request.Form["multi"] != null)
                    _multiple = Request.Form["multi"].ToString().Equals("1");

                if (Request.Form["height"] != null)
                    int.TryParse(Request.Form["height"].ToString(), out _height);

                if (Request.Form["descid"] != null)
                {
                    String sDescId = Request.Form["descid"].ToString();
                    _nDescId = eLibTools.GetNum(sDescId);
                }


                if (Request.Form["showcurrentuser"] != null)
                    _showCurrentUser = Request.Form["showcurrentuser"].ToString().Equals("1");

                if (Request.Form["showcurrentuserfilter"] != null)
                    _showCurrentUserFilter = Request.Form["showcurrentuserfilter"].ToString().Equals("1");

                if (Request.Form["showcurrentgroupfilter"] != null)
                    _showCurrentGroupFilter = Request.Form["showcurrentgroupfilter"].ToString().Equals("1");


                _showDefaultValue = _requestTools.GetRequestFormKeyB("showdefaultvalue") ?? false;
                _showAllValue = _requestTools.GetRequestFormKeyB("showdall") ?? false;

                _bIsMyEudonet = _requestTools.GetRequestFormKeyB("ismyeudonet") ?? false;

                if (_pref.AdminMode || _bIsMyEudonet)
                {
                    _bOnlyProfil = _requestTools.GetRequestFormKeyB("onlyprofil") ?? false;
                    _bDisplayProfil = _requestTools.GetRequestFormKeyB("profil") ?? false;
                }


                //Affichage de tous les utilisateurs et groupes dans les catalogues utilisateur (passe au-delà du Mode de sécurité de la base)
                if (Request.Form["fulluserlist"] != null)
                    _fullUserList = Request.Form["fulluserlist"].ToString().Equals("1");

                if (Request.Form["showemptygroup"] != null)
                    _showEmptyGroup = Request.Form["showemptygroup"].ToString().Equals("1");

                if (Request.Form["showuseronly"] != null)
                    _showUserOnly = Request.Form["showuseronly"].ToString().Equals("1");

                if (Request.Form["usegroup"] != null)
                    _useGroup = Request.Form["useGroup"].ToString().Equals("1");


                _bFromTreat = _requestTools.AllKeys.Contains("fromtreat") ? (Request.Form["fromtreat"].ToString() == "1" ? true : false) : false;


                if (Request.Form["modalvarname"] != null)
                    _parentModalVarName = Request.Form["modalvarname"].ToString();

                if (Request.Form["action"] != null)
                    action = Request.Form["action"].ToString();

                if (Request.Form["CatSearch"] != null)
                    _catSearch = Request.Form["CatSearch"].ToString();

                if (Request.Form["showvalueempty"] != null)
                    _showValueEmpty = Request.Form["showvalueempty"].ToString().Equals("1");


                bool bShowGroupOnly = _requestTools.GetRequestFormKeyB("showgrouponly") ?? false;

                _showAllUsersOption = _requestTools.GetRequestFormKeyB("showallusersoption") ?? false;

                bool bAdminOnly = _requestTools.GetRequestFormKeyB("onlyadmin") ?? false;

                //ALISTER => Demande 67 080
                if (Request.Form["displaymode"] != null)
                    _selectDisplayMode = Request.Form["displaymode"].ToString();

                if (bAdminOnly)
                    _sbInitJSOutput.Append("eCU.onlyAdmin = true;");

                if (Request.Form["showvaluepublicrecord"] != null)
                    _showValuePublicRecord = Request.Form["showvaluepublicrecord"].ToString().Equals("1");

                if (Request.Form["iframeId"] != null)
                    _iFrameId = Request.Form["iframeId"].ToString();

                // eUser.ListMode lstType = eUser.ListMode.USERS_AND_GROUPS;


                //KHA le 13/06/2014 : iso V7 mais est ce que ce n'est pas obsolète? je n'ai pas trouvé comment ça pouvait être utilisé en V7.
                Int32 nEVTID = 0;
                Int32 nPmId = 0;
                Int32 nPpId = 0;

                if (_requestTools.AllKeys.Contains("evtid"))
                    Int32.TryParse("evtid", out nEVTID);

                if (_requestTools.AllKeys.Contains("pmid"))
                    Int32.TryParse("pmid", out nPmId);

                if (_requestTools.AllKeys.Contains("ppid"))
                    Int32.TryParse("ppid", out nPpId);

                #endregion


                List<eUser.UserListItem> uli;
                List<eUser.UserListItem> uliCustom = null;

                // 
                //gestion du mode admin, 
                SECURITY_GROUP groupMode = _pref.GroupMode;
                if (_pref.AdminMode && _pref.User.UserLevel >= 99 && (groupMode == SECURITY_GROUP.GROUP_EXCLUDING || groupMode == SECURITY_GROUP.GROUP_EXCLUDING_READONLY))
                {
                    groupMode = SECURITY_GROUP.GROUP_DISPLAY;
                }


                _eUser = new eUser(_dal, _nDescId, _pref.User, eUser.ListMode.USERS_AND_GROUPS, groupMode, _selectedList);

                _eUser.EVTID = nEVTID;
                _eUser.PMID = nPmId;
                _eUser.PPID = nPpId;
                _eUser.ShowOnlyGroup = bShowGroupOnly;
                _eUser.bAdminOnly = bAdminOnly;

                if (_bDisplayProfil)
                {
                    _eUser.ShowOnlyProfil = _bOnlyProfil;
                    uli = _eUser.GetUserArbo(true, false, _catSearch, sbError);
                    if (sbError.Length > 0)
                        throw new Exception(sbError.ToString());


                }
                else if (DlgAction != DialogAction.MRU)
                    uli = _eUser.GetUserArbo(_fullUserList, _showUserOnly, _catSearch, sbError);
                else
                    uli = _eUser.GetUserList(false, true, _catSearch, sbError);



                List<eUser.UserListItem> uliCustomTemp = null;

                if (!_bOnlyProfil)
                {
                #region récupération de l'utilisateurs en cours si cela a été demandé.
                if (String.IsNullOrEmpty(_catSearch) && _showCurrentUser && !_bFromTreat && !_eUser.ShowOnlyGroup)
                        uliCustomTemp = _eUser.GetPrefUserList(ref sbError);
                    #endregion

                    #region Si cela est demandé afficher <Groupe de l'utilisateur en cours> dans la partie custom (par exemple pour le filtre)

                    if (String.IsNullOrEmpty(_catSearch) && (_showCurrentGroupFilter) && !_bFromTreat)
                    {
                        if (uliCustomTemp == null)
                            uliCustomTemp = new List<eUser.UserListItem>();

                        bool bIsCurrentUserGroupSelected = selectedIds != null && System.Web.HttpUtility.HtmlDecode(selectedIds).Contains("<GROUP>");

                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "<GROUP>", String.Concat("<", eResApp.GetRes(_pref, 963), ">"), false, false, "0000", bIsCurrentUserGroupSelected));
                    }

                    // Valeur par défaut
                    if (_showDefaultValue)
                    {
                        uliCustomTemp = new List<eUser.UserListItem>();
                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "0", eResApp.GetRes(_pref, 528), false, false, "0000", false));
                    }


                    #endregion

                    #region Si cela est demandé afficher <utilisateur en cours> dans la partie custom (par exemple pour le filtre)
                    if (String.IsNullOrEmpty(_catSearch) && (_showCurrentUserFilter || (_pref.AdminMode && !_eUser.ShowOnlyGroup)) && !_bFromTreat)
                    {
                        if (uliCustomTemp == null)
                            uliCustomTemp = new List<eUser.UserListItem>();


                        bool bIsCurrentUserSelected = selectedIds != null && System.Web.HttpUtility.HtmlDecode(selectedIds).Contains("<USER>");
                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "<USER>", String.Concat("<", eResApp.GetRes(_pref, 370), ">"), false, false, "0000", bIsCurrentUserSelected));
                    }
                    #endregion
                
                }

                #region Si cela est demandé d'afficher <Tous les utilisateurs> dans la partie custom (admin des droits)
                if (_showAllUsersOption && _multiple)
                {
                    uliCustomTemp = new List<eUser.UserListItem>();
                    uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "0", eResApp.GetRes(_pref, 6869), false, false, "0000", (selectedIds == "0")));
                }
                #endregion

                if (!_multiple)
                {
                    if (!_bOnlyProfil)
                    {
                    #region Si cela est demandé afficher <PUBLIC> dans la partie custom (par exemple pour les champs de saisie classiques) (seulement si non multiple)
                    if (_showValuePublicRecord && !_bFromTreat)
                        {
                            if (uliCustomTemp == null)
                                uliCustomTemp = new List<eUser.UserListItem>();
                            uliCustomTemp.Insert(0, new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "0", String.Concat("<", eResApp.GetRes(_pref, 53), ">"), false, false, string.Empty, false));
                        }
                        #endregion

                        #region Si cela est demandé afficher <VIDE> dans la partie custom (par exemple pour les champs de saisie classiques) (seulement si non multiple)
                        if (_showValueEmpty)
                        {
                            if (uliCustomTemp == null)
                                uliCustomTemp = new List<eUser.UserListItem>();
                            uliCustomTemp.Insert(0, new eUser.UserListItem(eUser.UserListItem.ItemType.USER, String.Empty, String.Concat("<", eResApp.GetRes(_pref, 141), ">"), false, false, string.Empty, false));
                        }
                        #endregion
                    }
                }

                uliCustom = uliCustomTemp;
                System.Text.StringBuilder sbHtmlContent = new System.Text.StringBuilder();
                XmlNode xmlReturn = null;
                //Envoi du rendu selon le type d'appel
                switch (DlgAction)
                {
                    case DialogAction.SHOW_DIALOG:

                        if (_multiple)
                            BodyCSS = "multi";
                        else
                            BodyCSS = "unit";

                        if (_eUser.ShowOnlyGroup)
                            BodyCSS += " grp";
                        else
                            BodyCSS += " usr";



                        FillTopDiv();
                        FillListTitle();
                        FillBottomDiv();

                        //Generation de l'affichage des utilisateurs custom
                        if (uliCustom != null)
                            GenerateTreeViewCustomVal(ref ResultDivCustomValues, uliCustom, _multiple, selectedIds, ref _sbInitJSOutput);

                        //Generation de l'affichage des utilisateurs
                        ResultDiv.Controls.Add(GenerateTreeViewVal(uli, _multiple, selectedIds, ref _sbInitJSOutput));
                        if (!_multiple)
                        {
                            ResultDivCustomValues.Attributes.Add("ondblclick", "eCU.SetSelDblClick(event);return false;");
                            ResultDiv.Attributes.Add("ondblclick", "eCU.SetSelDblClick(event);return false;");
                        }
                        //Bug #71 186: Ajuster la taille de la fenêtre(350 pour avoir minimum une hauteur à 50)
                        if (_height >= 350 && _multiple)
                            ResultDiv.Style.Add(HtmlTextWriterStyle.Height, (_height - 300).ToString() + "px");
                        break;
                    case DialogAction.REFRESH_DIALOG:
                    case DialogAction.NONE:
                        //*****  On force le rendu HTML avant qu'il s'affiche dans la page pour le XML  //
                        System.IO.StringWriter sw = new System.IO.StringWriter(sbHtmlContent);
                        HtmlTextWriter hw = new HtmlTextWriter(sw);
                        //**********************************************//
                        Response.Clear();
                        Response.ClearContent();
                        HtmlGenericControl hgc = new HtmlGenericControl();

                        //Generation de l'affichage des utilisateurs
                        hgc = GenerateTreeViewVal(uli, _multiple, selectedIds, ref _sbInitJSOutput);

                        _sbInitJSOutput.Append("eTV.init();");
                        //ALISTER => Demande 67 080, rafraîchir la liste après l'initialisation de la recherche
                        _sbInitJSOutput.Append("eCU.DisplayChange(true, " + _selectDisplayMode + ", true);");

                        #region paramétrage du retour détaillé
                        // Pour appel Ajax on recupere la table contenant seulement les valeurs
                        try
                        {
                            hgc.RenderControl(hw);
                        }
                        catch (Exception)
                        {
                            sbError.AppendLine("Erreur lors du renvoi du rendu.");  //TODO RES
                        }


                        #endregion

                        break;
                    case DialogAction.MRU:

                        xmlReturn = GenerateMruContent(uli);

                        break;
                    default:
                        break;
                }

                if (DlgAction != DialogAction.SHOW_DIALOG)
                {
                    try
                    {
                        detailsNode = _xmlResult.CreateElement("contents");

                        XmlNode searchValue = _xmlResult.CreateElement("searchValue");
                        searchValue.InnerText = _catSearch;
                        detailsNode.AppendChild(searchValue);

                        XmlNode nbResultNode = _xmlResult.CreateElement("nbResults");
                        nbResultNode.InnerText = uli.Count.ToString();
                        detailsNode.AppendChild(nbResultNode);

                        if (sbHtmlContent.Length > 0)
                        {
                            XmlNode htmlNode = _xmlResult.CreateElement("html");
                            htmlNode.InnerText = sbHtmlContent.ToString();
                            detailsNode.AppendChild(htmlNode);
                        }

                        if (DlgAction == DialogAction.MRU)
                        {
                            detailsNode.AppendChild(xmlReturn);
                        }



                        XmlNode jsNode = _xmlResult.CreateElement("js");
                        jsNode.InnerText = _sbInitJSOutput.ToString();
                        detailsNode.AppendChild(jsNode);
                    }
                    catch (Exception)
                    {
                        sbError.AppendLine("Erreur lors de la génération du xml à renvoyer.");  //TODO RES
                    }
                }

            }
            finally
            {
                _dal.CloseDatabase();
            }
            #region Retour en XML dans les cas des recherche (action <> ShowDialog)

            if (detailsNode != null)
            {
                XmlNode _maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                _xmlResult.AppendChild(_maintNode);

                XmlNode _resultNode = _xmlResult.CreateElement("result");
                _resultNode.InnerText = (sbError.Length <= 0) ? "SUCCESS" : "ERROR";
                detailsNode.AppendChild(_resultNode);

                XmlNode _errDesc = _xmlResult.CreateElement("errordescription");
                _errDesc.InnerText = sbError.ToString();
                detailsNode.AppendChild(_errDesc);

                _xmlResult.AppendChild(detailsNode);

                Response.Clear();
                Response.ClearContent();
                Response.AppendHeader("Access-Control-Allow-Origin", "*");
                Response.ContentType = "text/xml";
                Response.Write(_xmlResult.OuterXml);
                Response.End();
            }

            #endregion
        }

        private XmlNode GenerateMruContent(List<eUser.UserListItem> uli)
        {
            XmlNode xmlReturn = _xmlResult.CreateElement("elements");


            foreach (eUser.UserListItem ui in uli)
            {

                XmlNode xmlUser = _xmlResult.CreateElement("element");
                xmlReturn.AppendChild(xmlUser);


                XmlNode xmlId = _xmlResult.CreateElement("id");
                xmlUser.AppendChild(xmlId);
                xmlId.InnerText = ui.ItemCode;


                XmlNode xmlUserLabel = _xmlResult.CreateElement("label");
                xmlUser.AppendChild(xmlUserLabel);
                xmlUserLabel.InnerText = ui.Libelle;

            }
            return xmlReturn;
        }


        /// <summary>
        /// Remplis la div du haut nommé divTop avec Ne pas afficher les utilisateurs masqués
        /// et les btn d'impressions
        /// </summary>
        private void FillTopDiv()
        {
            return;


        }



        /// <summary>
        /// Remplis la div du bas nommé divBottom avec tous (dé)sélectionner
        /// et avec les btn radio Afficher tous les utilisateurs/N'afficher que les utilisateurs sélectionné
        /// </summary>
        private void FillBottomDiv()
        {
            Panel divBt;    //div contenant 2 colonnes

            //Label LblLibelle;   //span de libellé // jamais utilisé ?

            eCheckBoxCtrl cbCatalogValue;   //CheckBoxEUDO
            #region Ne pas afficher les utilisateurs masqués

            divBt = new Panel();
            divBt.Attributes.Add("class", "catMskUsr");

            cbCatalogValue = new eCheckBoxCtrl(_bDisplayProfil, false);
            cbCatalogValue.AddClass("chkAction");
            cbCatalogValue.AddClick("eCU.DisplayUserMasked(this);");
            cbCatalogValue.ID = "chkUnmsk";
            cbCatalogValue.Attributes.Add("name", "chkUnmsk");
            cbCatalogValue.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cbCatalogValue.AddText(eResApp.GetRes(_pref, 6251));

            divBt.Controls.Add(cbCatalogValue);

            if (_bOnlyProfil)
                divBt.Style.Add("display", "none");

            DivBottom.Controls.Add(divBt);
            #endregion

            if (_multiple)
            {
                Panel divSubBt; //div de gauche puis de droite contenant un div de libellé et un btn d'action
                Panel divSubSubBt;  //div de libellé
                Label lblLibelle;   //span de libellé
                HtmlGenericControl lblLibRadio; //libellé btn radio pour le lier au btn radio
                HtmlInputRadioButton radioBtn;  //btn radio
                Panel chkWrapper;

                divBt = new Panel();
                divBt.Attributes.Add("class", "usrbt-select");

                #region Tout sélectionner
                divSubBt = new Panel();
                divSubBt.Attributes.Add("class", "usrbt-all");
                divSubBt.Attributes.Add("onclick", "eCU.OnChkSelectAll(true);");

                divSubSubBt = new Panel();
                divSubSubBt.Attributes.Add("class", "usrbt-all_logo icon-all_select");
                divSubBt.Controls.Add(divSubSubBt);

                lblLibelle = new Label();
                lblLibelle.Attributes.Add("class", "usrtext-bt-all");
                lblLibelle.Text = eResApp.GetRes(_pref, 431);
                divSubBt.Controls.Add(lblLibelle);
                divBt.Controls.Add(divSubBt);
                #endregion

                #region Tout désélectionner
                divSubBt = new Panel();
                divSubBt.Attributes.Add("class", "usrbt-none");
                divSubBt.Attributes.Add("onclick", "eCU.OnChkSelectAll(false);");

                divSubSubBt = new Panel();
                divSubSubBt.Attributes.Add("class", "usrbt-none_logo icon-deselect_all");
                divSubBt.Controls.Add(divSubSubBt);

                lblLibelle = new Label();
                lblLibelle.Attributes.Add("class", "usrtext-bt-none");
                lblLibelle.Text = eResApp.GetRes(_pref, 432);
                divSubBt.Controls.Add(lblLibelle);

                divBt.Controls.Add(divSubBt);

                #endregion

                DivBottom.Controls.Add(divBt);

                divBt = new Panel();
                divBt.Attributes.Add("class", "usrbt-radio");

                #region Afficher tous les utilisateurs

                divSubBt = new Panel();
                divSubBt.Attributes.Add("class", "bt-left");

                chkWrapper = new Panel();

                radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "DisplayAllOrSelUser";
                radioBtn.Value = "All";
                radioBtn.ID = "rbAll";
                radioBtn.Checked = true;
                radioBtn.Attributes.Add("onclick", String.Concat("eCU.DisplayChange(this.checked,0);"));

                lblLibRadio = new HtmlGenericControl("label");
                lblLibRadio.InnerHtml = eResApp.GetRes(_pref, 6250);
                lblLibRadio.Attributes.Add("for", "rbAll");

                chkWrapper.Controls.Add(radioBtn);
                chkWrapper.Controls.Add(lblLibRadio);

                divSubBt.Controls.Add(chkWrapper);
                #endregion


                #region Afficher les utilisateurs sélectionnés

                chkWrapper = new Panel();

                radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "DisplayAllOrSelUser";
                radioBtn.Value = "Sel";
                radioBtn.ID = "rbSel";
                radioBtn.Attributes.Add("onclick", String.Concat("eCU.DisplayChange(this.checked,1);"));

                lblLibRadio = new HtmlGenericControl("label");
                lblLibRadio.InnerHtml = eResApp.GetRes(_pref, 6249);
                lblLibRadio.Attributes.Add("for", "rbSel");

                chkWrapper.Controls.Add(radioBtn);
                chkWrapper.Controls.Add(lblLibRadio);

                divSubBt.Controls.Add(chkWrapper);
                #endregion

                divBt.Controls.Add(divSubBt);

                divSubBt = new Panel();
                divSubBt.Attributes.Add("class", "bt-right");

                #region Bouton "afficher les utilisateurs non sélectionnés"

                chkWrapper = new Panel();
                chkWrapper.ID = "chkUnsel";

                radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "DisplayAllOrSelUser";
                radioBtn.Value = "Sel";
                radioBtn.ID = "rbUnsel";
                radioBtn.Attributes.Add("onclick", String.Concat("eCU.DisplayChange(this.checked,2);"));

                lblLibRadio = new HtmlGenericControl("label");
                lblLibRadio.InnerHtml = eResApp.GetRes(_pref, 6870);
                lblLibRadio.Attributes.Add("for", "rbUnsel");

                chkWrapper.Controls.Add(radioBtn);
                chkWrapper.Controls.Add(lblLibRadio);

                divSubBt.Controls.Add(chkWrapper);
                #endregion

                divBt.Controls.Add(divSubBt);

                DivBottom.Controls.Add(divBt);
            }
        }

        /// <summary>
        /// Entete du tableau avec les libellés en catalogue avancé ("libellé", "code", "désactivé", "ID")
        /// </summary>
        private void FillListTitle()
        {
            // Table avec les valeures du catalogue
            System.Web.UI.WebControls.Table tbCatValues = new System.Web.UI.WebControls.Table();
            tbCatValues.ID = "tbCatVal";
            tbCatValues.CssClass = "catEditVal";
            tbCatValues.Attributes.Add("cellpadding", "0");
            tbCatValues.Attributes.Add("cellspacing", "0");
            tbCatValues.CssClass = String.Concat(tbCatValues.CssClass, " withHead");

            // Entete du tableau
            TableHeaderRow row = new TableHeaderRow();
            TableHeaderCell hdCell = new TableHeaderCell();

            row.TableSection = TableRowSection.TableHeader;

            hdCell.Text = eResApp.GetRes(_pref, 6274);

            row.Cells.Add(hdCell);
            tbCatValues.Rows.Add(row);

            usrTreeTitle.Controls.Add(tbCatValues);
        }


        /// <summary>
        /// Création de la structure du catalogue arbo de base
        /// </summary>
        /// <param name="listValue">Liste des utilisateurs (en arborescence) à afficher</param>
        /// <param name="bMultiple">Catalogue mulitple ou pas</param>
        /// <param name="sSelectedIds">Liste des UserId et GroupId sélectionnés</param>
        /// <param name="sbInitJSOutput">Javascript de retour à afficher</param>
        /// <returns>Retour Graphique d'élément HTML de type UL</returns>
        private HtmlGenericControl GenerateTreeViewVal(List<eUser.UserListItem> listValue, Boolean bMultiple, string sSelectedIds, ref StringBuilder sbInitJSOutput)
        {
            HtmlGenericControl ulCatalogValue = new HtmlGenericControl("ul");

            ulCatalogValue.Attributes.Add("class", "eTVRoot");

            //Création du rendu graphique pour chaques branches et sous-branche de l'arborescence
            foreach (eUser.UserListItem itm in listValue)
            {
                HtmlGenericControl currentLi = CreateTreeViewChildValueControl(itm, bMultiple, sSelectedIds, ref sbInitJSOutput);




                if (currentLi != null)
                    ulCatalogValue.Controls.Add(currentLi);
            }
            sbInitJSOutput.Append("eCU.DisplayUserMasked();");

            return ulCatalogValue;
        }

        /// <summary>
        /// Création de la structure du catalogue arbo pour les valeurs "Custom" à afficher au dessus de la liste des utilisateurs.
        /// </summary>
        /// <param name="div">Div à laquel la liste des valeurs doit-être rattachées</param>
        /// <param name="listValue">Liste des Valeurs Custom à afficher</param>
        /// <param name="bMultiple">Catalogue mulitple ou pas</param>
        /// <param name="sSelectedIds">Liste des UserId et GroupId sélectionnés</param>
        /// <param name="sbInitJSOutput">Javascript de retour à afficher</param>
        private void GenerateTreeViewCustomVal(ref HtmlGenericControl div, List<eUser.UserListItem> listValue, Boolean bMultiple, string sSelectedIds, ref StringBuilder sbInitJSOutput)
        {
            //Pour chaque élément création du rendu de la ligne
            foreach (eUser.UserListItem uliValue in listValue)
            {
                HtmlGenericControl spanCustom = GetSpanTreeViewValue(uliValue, bMultiple, sSelectedIds, true, sbInitJSOutput);
                spanCustom.Attributes["class"] += " eTVPCustom";
                div.Controls.Add(spanCustom);
            }
        }

        /// <summary>
        /// Génère le contrôle HTML LI correspondant à une branche de catalogue arborescent, avec ses enfants - Fonction récursive
        /// </summary>
        /// <param name="uliValue">Valeur de catalogue pour laquelle générer le code</param>
        /// <param name="bMultiple">Catalogue multiple</param>
        /// <param name="sSelectedIds">Liste des userId et GroupeId sélectionnés séparé par des points virgules</param>
        /// <param name="sbInitJSOutput">Javascript à appliquer en retour.</param>
        /// <returns>Un contrôle HTML LI correspondant à la valeur de catalogue passée en paramètre, incluant ses propres enfants</returns>
        private HtmlGenericControl CreateTreeViewChildValueControl(eUser.UserListItem uliValue, Boolean bMultiple, string sSelectedIds, ref StringBuilder sbInitJSOutput)
        {
            if (
                (uliValue.Type == eUser.UserListItem.ItemType.GROUP)
                && (uliValue.ChildrensUserListItem.Count <= 0 && !_showEmptyGroup && !_eUser.ShowOnlyGroup) //s'il n'a pas d'utilisateurs de rattaché et que l'on ne doit pas afficher les gorupes sans enfants ne pas l'ajouter.
                )
                return null;

            HtmlGenericControl liCatalogValue = new HtmlGenericControl("li");

            HtmlGenericControl spanCatalogValue = GetSpanTreeViewValue(uliValue, bMultiple, sSelectedIds, false, sbInitJSOutput);

            liCatalogValue.ID = "eTVB_" + uliValue.ItemCode;
            liCatalogValue.Controls.Add(spanCatalogValue);

            // Et des enfants, récursivement
            if (uliValue.ChildrensUserListItem.Count > 0)
            {
                HtmlGenericControl ulCatalogValues = new HtmlGenericControl("ul");
                ulCatalogValues.ID = "eTVBC_" + uliValue.ItemCode;
                bool bCollapse = true;
                foreach (eUser.UserListItem _childValue in uliValue.ChildrensUserListItem)
                {
                    // si la branche a au moins un enfant coché, on l'affichera dépliée
                    List<eUser.UserListItem> childrenValues = new List<eUser.UserListItem>();
                    _eUser.GetAllChildren(_childValue, ref childrenValues);
                    childrenValues.Add(_childValue);
                    foreach (eUser.UserListItem current_childValue in childrenValues)
                    {
                        if (
                            sSelectedIds.Split(";").Contains(current_childValue.ItemCode.ToString()) ||
                            (_catSearch.Trim().Length > 0 && current_childValue.Libelle.ToLower().Contains(_catSearch.Trim().ToLower()))
                        )
                        {
                            bCollapse = false;
                            break;
                        }
                    }
                    HtmlGenericControl currentLi = CreateTreeViewChildValueControl(_childValue, bMultiple, sSelectedIds, ref sbInitJSOutput);
                    if (currentLi != null)
                    {
                        if (_childValue.Disabled)
                            currentLi.Attributes["class"] = String.Concat(currentLi.Attributes["class"], " userDis");


                        ulCatalogValues.Controls.Add(currentLi);
                    }
                }
                if (!bCollapse)
                    ulCatalogValues.Attributes["class"] = String.Concat(ulCatalogValues.Attributes["class"], " eTVcol");



                liCatalogValue.Controls.Add(ulCatalogValues);
            }

            return liCatalogValue;
        }

        /// <summary>
        /// Génération du contenu d'une branche standard pour un utilisateur ou groupe
        /// </summary>
        /// <param name="value">Objet Utilisateur</param>
        /// <param name="bMultiple">Catalogue mmultiple ou pas</param>
        /// <param name="sSelectedIds">Liste des UserId et GroupId sélectionnés</param>
        /// <param name="bCustomMode">Si custome mode, les ids seront préfixé de CM</param>
        /// <param name="_sbInitJSOutput">Javascript à appliquer en retour.</param>
        /// <returns></returns>
        private HtmlGenericControl GetSpanTreeViewValue(eUser.UserListItem value, Boolean bMultiple, string sSelectedIds, Boolean bCustomMode, StringBuilder _sbInitJSOutput)
        {
            HtmlGenericControl spanCatalogValue = new HtmlGenericControl("span");
            HtmlGenericControl spanCatalogValueText = new HtmlGenericControl("span");
            String CustomPrefix = (bCustomMode) ? "CM" : "";
            String jsCheckCustomOption = value.ItemCode == _pref.User.UserId.ToString() || value.ItemCode == "0" ? ",false,true" : "";

            if (!string.IsNullOrEmpty(sSelectedIds))
            {
                //metatag handling
                sSelectedIds = sSelectedIds.Replace("&lt;USER&gt;", "<USER>");
                sSelectedIds = sSelectedIds.Replace("&lt;GROUP&gt;", "<GROUP>");
            }

            //Affichage pour le multiple : case à coché en plus
            if (bMultiple)
            {
                eCheckBoxCtrl cbCatalogValue = new eCheckBoxCtrl(false, false);
                cbCatalogValue.AddClass("chkAction");
                cbCatalogValue.AddClass("TVChk");
                cbCatalogValue.AddClick(String.Concat("eCU.ClickValM(this", jsCheckCustomOption, ");"));
                cbCatalogValue.ID = String.Concat("chkValue", CustomPrefix, "_", value.ItemCode);
                cbCatalogValue.Attributes.Add("name", "chkValue");

                // Sélection (cochage) des valeurs actuellement sélectionnées (ou initialement présentes dans le champ)
                if (string.Concat(";", sSelectedIds, ";").Contains(string.Concat(";", value.ItemCode, ";")))
                {
                    cbCatalogValue.SetChecked(true);

                    //la checkbox custom n'a pas besoin d'etre chargée (pas la peine de la stocké dans la base)
                    //elle sera automatiquement mise à jour dès que charge sa soeur dans l'arbre en js
                    if (!bCustomMode || value.ItemCode == "0")
                        _sbInitJSOutput.Append("eCU.Load(document.getElementById('chkValue" + CustomPrefix + "_").Append(value.ItemCode).Append("'));");
                }
                spanCatalogValue.Controls.Add(cbCatalogValue);
                //


                spanCatalogValue.Attributes.Add("onclick", "eCU.SetSel(this);");
            }
            else
            {   //Pour le simple : pas de case à cocher et sauf paramètre contraire, il n'est pas possible de sélectionner un groupe.
                if (sSelectedIds == value.ItemCode.ToString())
                    _sbInitJSOutput
                        .Append("var oSel=document.getElementById('eTVBLV" + CustomPrefix + "_").Append(value.ItemCode).Append("');")
                        .Append("eCU.ClickValS(oSel);")
                        /* #61 208 - Positionnement du catalogue utilisateur
                        
                        Pour une raison complètement obscure, effectuer, au chargement de la page avec IE, un scrollIntoView sur la sélection (en vue de positionner la barre
                        de scroll du catalogue arborescent pour que l'utilisateur sélectionné soit visible)  échoue complètement, et bien qu'il semble cibler le bon objet dans le DOM
                        (la branche sélectionnée du treeview), il provoque en effet un scrollIntoView sur la page parente (!), qui, si elle comporte des éléments en-dehors
                        de la surface d'affichage visible du navigateur (le viewport), est alors "déplacée" d'autant de pixels que d'éléments situés hors du viewport.

                        Ainsi, sur IE, il suffisait de déplacer une fenêtre parente hors de la zone visible, puis d'afficher un catalogue utilisateur en popup par la suite
                        (exemple : depuis un graphique avec paramètres/filtres express, proposant de sélectionner une valeur issue d'un catalogue utilisateur)
                        pour que l'affichage se retrouve complètement perturbé. Et de manière totalement indétectable avec des outils de debug :
                        - passer en pas-à-pas sur scrollIntoView() ne reflète pas immédiatement la modification (on croit alors que l'appel à cette fonction n'est pas la cause du bug)
                        - aucune trace CSS ou JavaScript n'est visible, puisqu'un scrollIntoView() n'agit que sur la position de l'ascenseur et n'effectue aucune modif dans le DOM

                        Or, sur IE, différer l'appel à scrollIntoView(), même de plusieurs secondes, ne change rien (testé).
                        Et pourtant, l'élément ciblé est bien le bon, puisque l'ascenseur du catalogue utilisateur est bien repositionné sur tous les navigateurs (IE compris).

                        Ce comportement semble être dû à une différence d'interprétation d'IE quant à la manière de procéder :
                        https://social.technet.microsoft.com/Forums/ie/en-US/b8458796-8360-4512-b88a-62190f421ef7/issues-with-scrollintoview-javascript-method-in-ie10?forum=ieitprocurrentver

                        Cette fonction visant uniquement à améliorer le confort de l'utilisateur, on n'y fera donc pas appel sur IE dans ce cas.
                        */
                        .Append("var browser = new getBrowser();")
                        .Append("if (!browser.isIE) {")
                        .Append("oSel.scrollIntoView(false);")
                        .Append("}");

                if (value.IsChild)
                    spanCatalogValue.Attributes.Add("ednprofil", "1");

                if (value.Type == eUser.UserListItem.ItemType.USER || (value.Type == eUser.UserListItem.ItemType.GROUP && _useGroup))
                {
                    spanCatalogValue.Attributes.Add("onclick", "eCU.ClickValS(this);");
                }
                spanCatalogValue.InnerText = "  ";
            }


            spanCatalogValue.Attributes.Add("ednid", value.ItemCode);
            spanCatalogValue.Attributes.Add("ednmsk", (value.Hidden && !_bOnlyProfil) ? "1" : "0");
            spanCatalogValue.Attributes.Add("edndsbld", (value.Disabled && !_bOnlyProfil) ? "1" : "0");

            // Ajout de la valeur passée en paramètre
            spanCatalogValue.ID = "eTVBLV" + CustomPrefix + "_" + value.ItemCode;

            spanCatalogValueText.InnerHtml = Server.HtmlEncode(value.Libelle);
            if (value.Type == eUser.UserListItem.ItemType.GROUP)  //Ajout d'un espacement plus grand est d'une classe particulière pour le type Groupe
            {
                if (!_multiple)
                    spanCatalogValueText.InnerHtml = String.Concat("&nbsp;&nbsp;", spanCatalogValueText.InnerHtml);
                spanCatalogValueText.Attributes.Add("class", "usr_Gval");
            }
            spanCatalogValueText.ID = "eTVBLVT" + CustomPrefix + "_" + value.ItemCode;
            spanCatalogValueText.Attributes.Add("onclick", String.Concat("eCU.ClickLabel(this ", jsCheckCustomOption, ");"));

            spanCatalogValue.Controls.Add(spanCatalogValueText);
            return spanCatalogValue;
        }

    }





}