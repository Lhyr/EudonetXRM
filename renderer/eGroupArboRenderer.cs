using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{


    /// <summary>
    /// Paramètres de rendu d'un group
    /// -> provient du manager et des entrées user
    /// </summary>
    public class CatalogParamUserGroup
    {

        /// <summary>
        /// Catalogue pour l'adminstration des users
        /// TOUJOURS VERIFIER LE NIVEAU DU USER AVANT DE FAIRE CONFIANCE
        /// CE PARAM EST SET DEPUIS UN MANAGER ET DES DONNEES USERS
        /// 
        /// </summary>
        public bool Admin { get; set; }

        /// <summary>
        /// Afficher l'entrée 'Utilisateur en cours'
        /// </summary>
        public bool ShowCurrentUser { get; set; }

        public bool ShowCurrentUserFilter { get; set; }


        /// <summary>
        /// N'afficher que les group
        /// </summary>
        public bool ShowGroupOnly { get; set; }

        /// <summary>Indique s'il faut   </summary>
        public bool ShowCurrentGroupFilter { get; set; }

        /// <summary>
        /// Afficher <Racine/> dans la liste des groupes
        /// </summary>
        public bool ShowRootGroup { get; set; }


        /// <summary>
        /// indique s'il faut afficher la liste complète des utilisateurs
        /// </summary>
        public bool FullUserList { get; set; }


        /// <summary>
        /// Afficher les groupes vides
        /// </summary>
        public bool ShowEmptyGroup { get; set; }


        /// <summary>
        /// N'afficher que les utilisateurs
        /// </summary>
        public bool ShowUserOnly { get; set; }

        public bool UseGroup { get; set; }

        public bool ShowValueEmpty { get; set; }

        public bool ShowValuePublicRecord { get; set; }


        /// <summary>
        /// Catalogue à choix multiple
        /// </summary>
        public bool Multiple { get; set; }


        private int _nWitdh = 76;

        /// <summary>
        /// Taille en % de la boite de recherche
        /// </summary>
        public int WidthSearchBox { get { return _nWitdh; } set { _nWitdh = value; } }

        /// <summary>
        /// CSS additionnel
        /// </summary>
        public string Css
        {
            get; set;

        }

    }


    /// <summary>
    /// Code repris de  eCatalogDialogUser.aspx
    ///  -> passage en un rendrer pour pouvoir être réutilisé à d'autres endroits
    ///  TODO : refactoriser 
    /// </summary>
    public class eUserGroupCatalogRenderer : eRenderer
    {


        /// <summary>
        /// Paramètre du catalogue
        /// </summary>
        CatalogParamUserGroup _paramGroup;


        /// <summary>
        /// Information fiche parente
        /// </summary>
        eFileTools.eParentFileId _parentFile;


        DialogAction _dlg;

        /// <summary>
        /// Type d'action
        /// </summary>
        private DialogAction DlgAction { get { return _dlg; } }



        /// <summary>
        /// Objet utilisateur
        /// </summary>
        eUser _eUser;

        /// <summary>
        /// Champ de recherche saisie libre
        /// </summary>
        string _catSearch;


        /// <summary>
        /// Liste des utilisateurs/groupe sélectionné
        /// </summary>
        List<string> _lSelectedId;


        /// <summary>
        /// Descid du catalogue
        /// </summary>
        Int32 _nDescId;


        /// <summary>
        /// Appellé depuis les traitments
        /// </summary>
        bool _bFromTreat;




        XmlNode detailsNode = null;



        //  HtmlGenericControl DivBottom = new HtmlGenericControl();
        //        HtmlGenericControl ResultDiv = new HtmlGenericControl();
        //HtmlGenericControl ResultDivCustomValues = new HtmlGenericControl();


        public static eUserGroupCatalogRenderer GetGroupArboRenderer(ePref pref,
            Int32 nAction,
            Int32 nDescId,
            CatalogParamUserGroup param,
            string SearchValue,
            List<string> lSelectedId,
            eFileTools.eParentFileId parentFileId, bool bFromTreat)
        {



            eUserGroupCatalogRenderer a = new eUserGroupCatalogRenderer(pref, nAction, nDescId, param, SearchValue, lSelectedId, parentFileId, bFromTreat);


            //todo : le generate est censé être dans une factory
            a.Generate();

            return a;
        }




        /// <summary>
        /// Constructeur du catalogue user/groupe
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nAction"></param>
        /// <param name="nDescId"></param>
        /// <param name="param"></param>
        /// <param name="SearchValue"></param>
        /// <param name="lSelectedId"></param>
        /// <param name="parentFileId"></param>
        /// <param name="bFromTreat"></param>
        private eUserGroupCatalogRenderer(ePref pref,
            Int32 nAction,
            Int32 nDescId,

            CatalogParamUserGroup param,
            string SearchValue,
            List<string> lSelectedId,
            eFileTools.eParentFileId parentFileId,
            bool bFromTreat)
        {


            _dlg = eLibTools.GetEnumFromCode<DialogAction>(nAction);


            param.Admin = param.Admin && pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN;

            Pref = pref;
            _paramGroup = param;
            _lSelectedId = lSelectedId ?? new List<string>();
            _nDescId = nDescId;
            _catSearch = SearchValue;
            _parentFile = parentFileId;
            _bFromTreat = bFromTreat;
        }



        /// <summary>
        /// Initialisation du renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            //Alister #Demande 64862
            if (!(_ePref.GroupMode == SECURITY_GROUP.GROUP_NONE && _paramGroup.Admin))
            {

                StringBuilder sbError = new StringBuilder();
                eudoDAL dal = eLibTools.GetEudoDAL(Pref);

                dal.OpenDatabase();
                try
                {
                    _eUser = new eUser(dal, _nDescId, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, _lSelectedId);
                    _eUser.EVTID = _parentFile?.ParentEvtId ?? 0;
                    _eUser.PMID = _parentFile?.ParentPmId ?? 0;
                    _eUser.PPID = _parentFile?.ParentPpId ?? 0;


                    _eUser.ShowOnlyGroup = _paramGroup.ShowGroupOnly;

                    #region Construction HTML
                    #region Script JS

                    //variable
                    AddCallBackScript(String.Concat(" var isMultipleCatalog = ", _paramGroup.Multiple ? "true" : "fasel", ";     "));
                    AddCallBackScript(String.Concat(" var eTV;"));
                    AddCallBackScript(String.Concat(" var eCU;"));


                    AddCallBackScript(String.Concat("var pParamCat = ", SerializerTools.JsonSerialize(_paramGroup), ";"));

                    //fonction init
                    //  AddCallBackScript(String.Concat(" function init() {"));
                    AddCallBackScript(" eCU = new eCatalogUser(");
                    AddCallBackScript("'eCU',");
                    AddCallBackScript("'',"); // parentmodalvarname
                    AddCallBackScript("'eCatalogDialogUser.aspx',");
                    AddCallBackScript("pParamCat.Multiple, ");
                    AddCallBackScript("'" + _nDescId.ToString() + "',");
                    AddCallBackScript("pParamCat.FullUserList ,");
                    AddCallBackScript("pParamCat.ShowUserOnly ,");
                    AddCallBackScript("pParamCat.ShowEmptyGroup ,");
                    AddCallBackScript("pParamCat.UseGroup ,");
                    AddCallBackScript("pParamCat.ShowValuePublicRecord ,");
                    AddCallBackScript("pParamCat.ShowValueEmpty ");

                    AddCallBackScript(");");

                    AddCallBackScript("eCU.paramValues = pParamCat ;");
                    AddCallBackScript("eCU.iFrameId = '';"); // iframeId



                    AddCallBackScript("eTV = new eTreeView('eTV', 'eTVOusr icon-contact', 'eTVCusr icon-contact', 'eTVSBusr icon-avatar', 'eTVS', 'eTVRoot', 'eTVF', 'eTVI', 'eTVSolo', 'eTVP eTVPGroup')");


                    #endregion

                    #region Champ Recherche
                    Panel DivSearch = new Panel();
                    PgContainer.Controls.Add(DivSearch);
                    PgContainer.CssClass = "usr_cat_body";
                    // PgContainer.Attributes["class"] += string.Concat(" toto ", eTools.GetClassNameFontSize(Pref));

                    if (_paramGroup.Admin && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                        PgContainer.Attributes.Add("ednadmin", "1");

                    DivSearch.CssClass = "usr_top_srch";

                    //
                    Panel DivSearchLabel = new Panel();
                    DivSearch.Controls.Add(DivSearchLabel);
                    DivSearchLabel.CssClass = "usr_search-text";
                    DivSearchLabel.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 595), " : ")));

                    HtmlInputText inpt = new HtmlInputText();
                    DivSearch.Controls.Add(inpt);
                    inpt.ID = "eTxtSrch";
                    inpt.Attributes.Add("class", "usr_search-inpt");
                    inpt.Attributes.Add("type", "text");
                    //inpt.Attributes.Add("style", "width:" + _paramGroup.WidthSearchBox.ToString() + "%");


                    if (_paramGroup.Admin && _ePref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                    {
                        inpt.Attributes.Add("onkeyup", "nsAdminUsers.FilterGroupCatalog(event, this.value);");
                    }
                    else
                        inpt.Attributes.Add("onkeyup", "eCU.FindValues(event, this.value);");

                    if (_ePref.GroupMode == SECURITY_GROUP.GROUP_NONE)
                        inpt.Attributes.Add("disabled", "true");

                    Panel DivSearchBtn = new Panel();
                    DivSearch.Controls.Add(DivSearchBtn);

                    if (_ePref.GroupMode != SECURITY_GROUP.GROUP_NONE)
                    {
                        if (_paramGroup.Admin && _ePref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                            DivSearchBtn.Attributes.Add("onclick", "nsAdminUsers.FilterGroupCatalogBtn(event, this.value);");
                        else
                            DivSearchBtn.Attributes.Add("onclick", "eCU.BtnSrch();");
                    }

                    DivSearchBtn.ID = "eBtnSrch";
                    DivSearchBtn.Attributes.Add("title", eResApp.GetRes(Pref, 111));
                    DivSearchBtn.Attributes.Add("class", "icon-magnifier srchFldImg");

                    #endregion


                    #region DivTop
                    Panel DivTop = new Panel();
                    PgContainer.Controls.Add(DivTop);
                    DivTop.ID = "DivTop";

                    #endregion

                    #region Ajout, modification, suppression
                    /*
                    Panel DivAddEditDelete = new Panel();
                    HtmlGenericControl ulToolbarAdd = new HtmlGenericControl("ul");
                    ulToolbarAdd.Attributes.Add("class", "grpTVToolAdd");
                    // TODO: droits d'ajout/édition/suppression de groupes ?
                    ulToolbarAdd.Controls.Add(BtnAdd(true));
                    ulToolbarAdd.Controls.Add(BtnEdit(true));
                    ulToolbarAdd.Controls.Add(BtnDel(true));
                    DivAddEditDelete.Controls.Add(ulToolbarAdd);
                    PgContainer.Controls.Add(DivAddEditDelete);
                    */
                    #endregion


                    #region Corps de catalogue

                    Panel DivUserCat = new Panel();
                    PgContainer.Controls.Add(DivUserCat);
                    DivUserCat.CssClass = "usr_cat";

                    Panel DivUserTreeTitle = new Panel();
                    DivUserCat.Controls.Add(DivUserTreeTitle);
                    DivUserTreeTitle.ID = "usrTreeTitle";

                    Panel DivUserTree = new Panel();
                    DivUserCat.Controls.Add(DivUserTree);
                    DivUserTree.CssClass = "userTree";


                    Panel ResultDivCustomValues = new Panel();
                    DivUserTree.Controls.Add(ResultDivCustomValues);
                    ResultDivCustomValues.ID = "ResultDivCustomValues";
                    ResultDivCustomValues.CssClass = "userTreeCustomValues";


                    Panel ResultDiv = new Panel();
                    DivUserTree.Controls.Add(ResultDiv);
                    ResultDiv.ID = "ResultDiv";
                    ResultDiv.CssClass = "userTreeValues";





                    #endregion



                    #region Bas du catalogue

                    Panel DivBottom = new Panel();
                    PgContainer.Controls.Add(DivBottom);
                    DivBottom.ID = "DivBottom";
                    DivBottom.CssClass = "usr_btm";


                    #endregion




                    #endregion

                    List<eUser.UserListItem> uli;
                    List<eUser.UserListItem> uliCustom = null;


                    if (_dlg != DialogAction.MRU)
                        uli = _eUser.GetUserArbo(_paramGroup.FullUserList, _paramGroup.ShowUserOnly, _catSearch, sbError);
                    else
                        uli = _eUser.GetUserList(false, true, _catSearch, sbError);


                    List<eUser.UserListItem> uliCustomTemp = null;

                    #region récupération de l'utilisateurs en cours si cela a été demandé.
                    if (String.IsNullOrEmpty(_catSearch) && _paramGroup.ShowCurrentUser && !_bFromTreat)
                        uliCustomTemp = _eUser.GetPrefUserList(ref sbError);
                    #endregion


                    #region Si cela est demandé afficher <Groupe de l'utilisateur en cours> dans la partie custom (par exemple pour le filtre)

                    if (String.IsNullOrEmpty(_catSearch) && _paramGroup.ShowCurrentGroupFilter && !_bFromTreat)
                    {
                        if (uliCustomTemp == null)
                            uliCustomTemp = new List<eUser.UserListItem>();
                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "<GROUP>", String.Concat("<", eResApp.GetRes(Pref, 963), ">"), false, false, "0000", false));
                    }

                    #endregion


                    #region Si cela est demandé afficher <utilisateur en cours> dans la partie custom (par exemple pour le filtre)
                    if (String.IsNullOrEmpty(_catSearch) && _paramGroup.ShowCurrentUserFilter && !_bFromTreat)
                    {
                        if (uliCustomTemp == null)
                            uliCustomTemp = new List<eUser.UserListItem>();
                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "<USER>", String.Concat("<", eResApp.GetRes(Pref, 370), ">"), false, false, "0000", false));
                    }
                    #endregion

                    #region Si cela est demandé afficher <Racine> dans la partie custom (cas de l'administration des groupes, où on doit pouvoir insérer un nouveau groupe à la racine)
                    if (String.IsNullOrEmpty(_catSearch) && _paramGroup.ShowRootGroup && !_bFromTreat)
                    {
                        if (uliCustomTemp == null)
                            uliCustomTemp = new List<eUser.UserListItem>();
                        uliCustomTemp.Add(new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "ROOT", eResApp.GetRes(Pref, 7576), false, false, "0000", false)); // TODORES
                    }
                    #endregion

                    if (!_paramGroup.Multiple)
                    {
                        #region Si cela est demandé afficher <PUBLIC> dans la partie custom (par exemple pour les champs de saisie classiques) (seulement si non multiple)
                        if (_paramGroup.ShowValuePublicRecord && !_bFromTreat)
                        {
                            if (uliCustomTemp == null)
                                uliCustomTemp = new List<eUser.UserListItem>();
                            uliCustomTemp.Insert(0, new eUser.UserListItem(eUser.UserListItem.ItemType.USER, "0", String.Concat("<", eResApp.GetRes(Pref, 53), ">"), false, false, string.Empty, false));
                        }
                        #endregion

                        #region Si cela est demandé afficher <VIDE> dans la partie custom (par exemple pour les champs de saisie classiques) (seulement si non multiple)
                        if (_paramGroup.ShowValueEmpty)
                        {
                            if (uliCustomTemp == null)
                                uliCustomTemp = new List<eUser.UserListItem>();
                            uliCustomTemp.Insert(0, new eUser.UserListItem(eUser.UserListItem.ItemType.USER, String.Empty, String.Concat("<", eResApp.GetRes(Pref, 141), ">"), false, false, string.Empty, false));
                        }
                        #endregion
                    }


                    uliCustom = uliCustomTemp;
                    System.Text.StringBuilder sbHtmlContent = new System.Text.StringBuilder();
                    XmlNode xmlReturn = null;
                    //Envoi du rendu selon le type d'appel


                    switch (DlgAction)
                    {
                        case DialogAction.SHOW_DIALOG:

                            FillTopDiv(DivTop);
                            FillListTitle(DivUserTreeTitle);
                            FillBottomDiv(DivBottom);

                            //Generation de l'affichage des utilisateurs custom
                            if (_ePref.GroupMode == SECURITY_GROUP.GROUP_NONE && _paramGroup.Admin)
                            {
                                Panel msg = new Panel();
                                msg.Style.Add("margin", "5px");
                                msg.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 1970)));
                                ResultDiv.Controls.Add(msg);
                            }
                            else
                            {
                                if (uliCustom != null)
                                    GenerateTreeViewCustomVal(ResultDivCustomValues, uliCustom);

                                //Generation de l'affichage des utilisateurs
                                ResultDiv.Controls.Add(GenerateTreeViewVal(uli));
                                if (!_paramGroup.Multiple)
                                {
                                    ResultDivCustomValues.Attributes.Add("ondblclick", "eCU.SetSelDblClick(event);return false;");
                                    ResultDiv.Attributes.Add("ondblclick", "eCU.SetSelDblClick(event);return false;");
                                }
                            }

                            break;
                        case DialogAction.REFRESH_DIALOG:
                        case DialogAction.NONE:
                            //*****  On force le rendu HTML avant qu'il s'affiche dans la page pour le XML  //
                            System.IO.StringWriter sw = new System.IO.StringWriter(sbHtmlContent);
                            HtmlTextWriter hw = new HtmlTextWriter(sw);
                            //**********************************************//


                            HtmlGenericControl hgc = new HtmlGenericControl();


                            //Generation de l'affichage des utilisateurs
                            hgc = GenerateTreeViewVal(uli);

                            AddCallBackScript("eTV.init();");

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
                            jsNode.InnerText = GetCallBackScript;
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

                    dal?.CloseDatabase();

                }

            }


            return true;
        }


        protected override bool Build()
        {
            return true;
        }


        protected override bool End()
        {
            return true;
        }


        /// <summary>Pour un renvoi AJAX, retour de sortie</summary>
        XmlDocument _xmlResult = new XmlDocument();

        /// <summary>
        /// Création du bouton d'edition pour renommer une valeur 
        /// </summary>
        /// <param name="_UpdateAllowed">Boolean correspondant au droit de modification sur ce catalogue</param>
        /// <returns>Rend un LI</returns>
        private HtmlGenericControl BtnEdit(bool _UpdateAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");

            if (_UpdateAllowed)
            {
                liToolbar.Attributes.Add("onclick", "nsAdminUsers.editGroup();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(Pref, 151)); // Modifier
                liToolbar.Attributes.Add("class", "icon-edn-pen");

            }
            else
            {
                // TODO: title ?
                liToolbar.Attributes.Add("class", "icon-edn-pen disable");

            }

            return liToolbar;
        }

        /// <summary>
        /// Création du bouton de suppression pour un groupe
        /// </summary>
        /// <param name="_DeleteAllowed">Boolean correspondant au droit de suppression de groupe</param>
        /// <returns>rend un LI</returns>
        private HtmlGenericControl BtnDel(bool _DeleteAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");

            if (_DeleteAllowed)
            {
                liToolbar.Attributes.Add("onclick", "nsAdminUsers.delGroup();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(Pref, 19));
                liToolbar.Attributes.Add("class", "icon-delete");
            }
            else
            {
                // TODO: title ?
                liToolbar.Attributes.Add("class", "icon-delete disable");
            }

            return liToolbar;
        }

        /// <summary>
        /// Création d'un bouton d'ajout pour les groupes
        /// </summary>
        /// <param name="_AddAllowed">Droit d'ajout</param>
        /// <returns>Rend un LI avec le bouton</returns>
        private HtmlGenericControl BtnAdd(bool _AddAllowed)
        {
            HtmlGenericControl liToolbar = new HtmlGenericControl("li");

            if (_AddAllowed)
            {
                liToolbar.Attributes.Add("onclick", "nsAdminUsers.addGroup();");
                liToolbar.Attributes.Add("title", eResApp.GetRes(Pref, 18)); // Ajouter
                liToolbar.Attributes.Add("class", "icon-add");
            }
            else
            {
                liToolbar.Attributes.Add("title", eResApp.GetRes(Pref, 6279)); // Les droits d'ajout sont désactivés
                liToolbar.Attributes.Add("class", "catTVToolAddBtnDis");
            }

            return liToolbar;
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
        private void FillTopDiv(Panel pnlTop)
        {
            return;


        }

        /// <summary>
        /// Remplis la div du bas nommé divBottom avec tous (dé)sélectionner
        /// et avec les btn radio Afficher tous les utilisateurs/N'afficher que les utilisateurs sélectionné
        /// </summary>
        private void FillBottomDiv(Panel DivBottom)
        {
            Panel divBt;    //div contenant 2 colonnes

            //Label LblLibelle;   //span de libellé // jamais utilisé ?


            #region Ne pas afficher les utilisateurs masqués

            if (!_paramGroup.ShowGroupOnly)
            {
                eCheckBoxCtrl cbCatalogValue;   //CheckBoxEUDO
                divBt = new Panel();
                divBt.Attributes.Add("class", "catMskUsr");

                cbCatalogValue = new eCheckBoxCtrl(false, false);
                cbCatalogValue.AddClass("chkAction");
                cbCatalogValue.AddClick("eCU.DisplayUserMasked(this);");
                cbCatalogValue.ID = "chkUnmsk";
                cbCatalogValue.Attributes.Add("name", "chkUnmsk");
                cbCatalogValue.Style.Add(HtmlTextWriterStyle.Height, "18px");
                cbCatalogValue.AddText(eResApp.GetRes(Pref, 6251));

                divBt.Controls.Add(cbCatalogValue);

                DivBottom.Controls.Add(divBt);

            }
            #endregion

            if (!_paramGroup.ShowGroupOnly && _paramGroup.Multiple)
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
                lblLibelle.Text = eResApp.GetRes(Pref, 431);
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
                lblLibelle.Text = eResApp.GetRes(Pref, 432);
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
                lblLibRadio.InnerHtml = eResApp.GetRes(Pref, 6250);
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
                lblLibRadio.InnerHtml = eResApp.GetRes(Pref, 6249);
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
                lblLibRadio.InnerHtml = eResApp.GetRes(Pref, 6870);
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
        private void FillListTitle(Panel usrTreeTitle)
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

            if (_paramGroup.ShowGroupOnly)
                hdCell.Text = eResApp.GetRes(Pref, 208);
            else
                hdCell.Text = eResApp.GetRes(Pref, 6274);

            row.Cells.Add(hdCell);
            tbCatValues.Rows.Add(row);
            usrTreeTitle.Controls.Add(tbCatValues);

        }


        /// <summary>
        /// Création de la structure du catalogue arbo de base
        /// </summary>
        /// <param name="listValue">Liste des utilisateurs (en arborescence) à afficher</param>
        /// <returns>Retour Graphique d'élément HTML de type UL</returns>
        private HtmlGenericControl GenerateTreeViewVal(List<eUser.UserListItem> listValue)
        {
            HtmlGenericControl ulCatalogValue = new HtmlGenericControl("ul");

            ulCatalogValue.Attributes.Add("class", "eTVRoot");

            //Création du rendu graphique pour chaques branches et sous-branche de l'arborescence
            foreach (eUser.UserListItem itm in listValue)
            {
                HtmlGenericControl currentLi = CreateTreeViewChildValueControl(itm);
                if (currentLi != null)
                    ulCatalogValue.Controls.Add(currentLi);
            }

            AddCallBackScript("eCU.DisplayUserMasked();");

            return ulCatalogValue;
        }

        /// <summary>
        /// Création de la structure du catalogue arbo pour les valeurs "Custom" à afficher au dessus de la liste des utilisateurs.
        /// </summary>
        /// <param name="div">Div à laquel la liste des valeurs doit-être rattachées</param>
        /// <param name="listValue">Liste des Valeurs Custom à afficher</param>
        private void GenerateTreeViewCustomVal(Panel div, List<eUser.UserListItem> listValue)
        {
            //Pour chaque élément création du rendu de la ligne
            foreach (eUser.UserListItem uliValue in listValue)
            {
                HtmlGenericControl spanCustom = GetSpanTreeViewValue(uliValue, true);
                spanCustom.Attributes["class"] += " eTVPCustom";
                div.Controls.Add(spanCustom);
            }
        }

        /// <summary>
        /// Génère le contrôle HTML LI correspondant à une branche de catalogue arborescent, avec ses enfants - Fonction récursive
        /// </summary>
        /// <param name="uliValue">Valeur de catalogue pour laquelle générer le code</param>
        /// <returns>Un contrôle HTML LI correspondant à la valeur de catalogue passée en paramètre, incluant ses propres enfants</returns>
        private HtmlGenericControl CreateTreeViewChildValueControl(eUser.UserListItem uliValue)
        {
            if (
                (uliValue.Type == eUser.UserListItem.ItemType.GROUP)
                && (uliValue.ChildrensUserListItem.Count <= 0 && !_paramGroup.ShowEmptyGroup && !_eUser.ShowOnlyGroup) //s'il n'a pas d'utilisateurs de rattaché et que l'on ne doit pas afficher les gorupes sans enfants ne pas l'ajouter.
                )
                return null;


            HtmlGenericControl liCatalogValue = new HtmlGenericControl("li");
            HtmlGenericControl spanCatalogValue = GetSpanTreeViewValue(uliValue, false);

            liCatalogValue.ID = "eTVB_" + uliValue.ItemCode;
            liCatalogValue.Controls.Add(spanCatalogValue);
            liCatalogValue.Attributes.Add("edndisplay", "1");

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

                            _lSelectedId.Contains(current_childValue.ItemCode)

                            ||

                            (_catSearch.Trim().Length > 0 && current_childValue.Libelle.ToLower().Contains(_catSearch.Trim().ToLower()))
                        )
                        {
                            bCollapse = false;
                            break;
                        }
                    }
                    HtmlGenericControl currentLi = CreateTreeViewChildValueControl(_childValue);
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
        /// <param name="bCustomMode">Si custome mode, les ids seront préfixé de CM</param>
        /// <returns></returns>
        private HtmlGenericControl GetSpanTreeViewValue(eUser.UserListItem value, Boolean bCustomMode)
        {
            HtmlGenericControl spanCatalogValue = new HtmlGenericControl("span");
            HtmlGenericControl spanCatalogValueText = new HtmlGenericControl("span");
            String CustomPrefix = (bCustomMode) ? "CM" : "";
            String jsCheckCustomOption = value.ItemCode == Pref.User.UserId.ToString() ? ",false,true" : "";

            //Barre d'outils
            HtmlGenericControl spanCatalogToolbar = GetSpanTreeViewToolbar(value, true, true, value.ItemCode != "ROOT", value.ItemCode != "ROOT");

            //Affichage pour le multiple : case à coché en plus
            if (_paramGroup.Multiple)
            {
                eCheckBoxCtrl cbCatalogValue = new eCheckBoxCtrl(false, false);
                cbCatalogValue.AddClass("chkAction");
                cbCatalogValue.AddClass("TVChk");




                if (_paramGroup.Admin && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                {
                    cbCatalogValue.AddClass("chkRootGroupAdm");

                    cbCatalogValue.AddClick(String.Concat("nsAdminUsers.FilterByGroup(this, eCU);"));
                }
                else
                    cbCatalogValue.AddClick(String.Concat("eCU.ClickValM(this", jsCheckCustomOption, ");"));


                cbCatalogValue.ID = String.Concat("chkValue", CustomPrefix, "_", value.ItemCode);
                cbCatalogValue.Attributes.Add("name", "chkValue");

                // Sélection (cochage) des valeurs actuellement sélectionnées (ou initialement présentes dans le champ)
                if (_lSelectedId.Contains(value.ItemCode))
                {
                    cbCatalogValue.SetChecked(true);
                    //la checkbox custom n'a pas besoin d'etre chargée (pas la peine de la stocké dans la base)
                    //elle sera automatiquement mise à jour dès que charge sa soeur dans l'arbre en js
                    if (!bCustomMode)
                    {
                        AddCallBackScript(

                            String.Concat("eCU.Load(document.getElementById('chkValue", CustomPrefix, "_", value.ItemCode, "'));")

                            );

                    }
                }
                spanCatalogValue.Controls.Add(cbCatalogValue);
                //


                spanCatalogValue.Attributes.Add("onclick", "eCU.SetSel(this);");


            }
            else
            {   //Pour le simple : pas de case à cocher et sauf paramètre contraire, il n'est pas possible de sélectionner un groupe.
                if (_lSelectedId.Count == 1 && _lSelectedId[0] == value.ItemCode.ToString())
                {
                    AddCallBackScript(String.Concat("var oSel=document.getElementById('eTVBLV", CustomPrefix, "_", value.ItemCode, "');"));
                    AddCallBackScript("eCU.ClickValS(oSel);");
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
                    AddCallBackScript(
                        String.Concat(
                            "var browser = new getBrowser();",
                            "if (!browser.isIE) {",
                                "oSel.scrollIntoView(false);",
                            "}"
                        )
                   );
                }

                if (value.Type == eUser.UserListItem.ItemType.USER || (value.Type == eUser.UserListItem.ItemType.GROUP && _paramGroup.UseGroup))
                {
                    if (value.IsChild)
                        spanCatalogValue.Attributes.Add("ednprofil", "1");

                    spanCatalogValue.Attributes.Add("onclick", "eCU.ClickValS(this);");
                }
                spanCatalogValue.InnerText = "  ";
            }
            spanCatalogValue.Attributes.Add("ednid", value.ItemCode);
            spanCatalogValue.Attributes.Add("ednmsk", (value.Hidden) ? "1" : "0");
            spanCatalogValue.Attributes.Add("edndsbld", (value.Disabled) ? "1" : "0");

            // Ajout de la valeur passée en paramètre
            spanCatalogValue.ID = "eTVBLV" + CustomPrefix + "_" + value.ItemCode;

            spanCatalogValueText.InnerHtml = WebUtility.HtmlEncode(value.Libelle);

            if (value.Type == eUser.UserListItem.ItemType.GROUP)  //Ajout d'un espacement plus grand est d'une classe particulière pour le type Groupe
            {
                if (!_paramGroup.Multiple)
                    spanCatalogValueText.InnerHtml = String.Concat("&nbsp;&nbsp;", spanCatalogValueText.InnerHtml);
                spanCatalogValueText.Attributes.Add("class", "usr_Gval");
            }
            spanCatalogValueText.ID = "eTVBLVT" + CustomPrefix + "_" + value.ItemCode;

            if (_paramGroup.Admin && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                spanCatalogValueText.Attributes.Add("onclick", String.Concat("eCU.SetSel(this);"));
            else
                spanCatalogValueText.Attributes.Add("onclick", String.Concat("eCU.ClickLabel(this ", jsCheckCustomOption, ");"));

            spanCatalogValue.Controls.Add(spanCatalogValueText);

            spanCatalogValue.Controls.Add(spanCatalogToolbar);

            return spanCatalogValue;
        }

        private HtmlGenericControl GetSpanTreeViewToolbar(eUser.UserListItem value, bool bAddUserInGroupAllowed, bool bAddAllowed, bool bUpdateAllowed, bool bDeleteAllowed)
        {
            eUlCtrl ulBtnValues = new eUlCtrl(); ;
            ulBtnValues.CssClass = "catBtnUl";

            #region Bouton Ajouter un utilisateur
            eLiCtrl liBtn = new eLiCtrl();
            liBtn.ToolTip = eResApp.GetRes(Pref, 7585); // "Ajouter un nouvel utilisateur dans ce groupe"

            if (bAddUserInGroupAllowed)
            {
                liBtn.Attributes.Add("onclick", String.Concat("nsAdminUsers.addUserInGroup('", value.ItemCode, "');"));
                liBtn.CssClass = "icon-user";
            }
            else
            {
                liBtn.CssClass = "icon-user disable";
            }
            ulBtnValues.Controls.Add(liBtn);
            #endregion

            #region Bouton Ajouter
            liBtn = new eLiCtrl();
            liBtn.ToolTip = eResApp.GetRes(Pref, 18);

            if (bAddAllowed)
            {
                liBtn.Attributes.Add("onclick", String.Concat("nsAdminUsers.addGroup('", value.ItemCode, "');"));
                liBtn.CssClass = "icon-add";
            }
            else
            {
                liBtn.CssClass = "icon-add disable";
            }
            ulBtnValues.Controls.Add(liBtn);
            #endregion

            #region Bouton Modifier
            liBtn = new eLiCtrl();
            liBtn.ToolTip = eResApp.GetRes(Pref, 151);

            if (bUpdateAllowed)
            {
                liBtn.Attributes.Add("onclick", String.Concat("nsAdminUsers.editGroup('", value.ItemCode, "');"));
                liBtn.CssClass = "icon-edn-pen";
                ulBtnValues.Controls.Add(liBtn);
            }
            else
            {
                liBtn.CssClass = "icon-edn-pen disable";
            }
            //ulBtnValues.Controls.Add(liBtn); // contrairement aux autres cas similaires de l'application, sur le TreeView des groupes, une icône inactive est masquée
            #endregion

            #region Bouton Supprimer
            liBtn = new eLiCtrl();
            liBtn.ToolTip = eResApp.GetRes(Pref, 19);

            if (bDeleteAllowed)
            {
                liBtn.Attributes.Add("onclick", String.Concat("nsAdminUsers.delGroup('", value.ItemCode, "');"));
                liBtn.CssClass = "icon-delete";
                ulBtnValues.Controls.Add(liBtn);
            }
            else
            {
                liBtn.CssClass = "icon-delete disable";
            }
            //ulBtnValues.Controls.Add(liBtn); // contrairement aux autres cas similaires de l'application, sur le TreeView des groupes, une icône inactive est masquée
            #endregion

            HtmlGenericControl divbtnValues = new HtmlGenericControl("span");
            divbtnValues.Attributes.Add("class", "eTVTB");
            divbtnValues.Controls.Add(ulBtnValues);

            return divbtnValues;
        }


        protected enum DialogAction
        {
            NONE = 0,
            SHOW_DIALOG = 1,
            REFRESH_DIALOG = 2,
            MRU = 3,

        }


    }
}