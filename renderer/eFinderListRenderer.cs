using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eFinderRenderer</className>
    /// <summary>classe générant le rendu du champ de liaison</summary>
    /// <authors>GCH</authors>
    /// <date>2012-09-07</date>
    public class eFinderListRenderer : eListMainRenderer
    {
        private const Int32 HeightList = 385;

        private Boolean _bMulti = false;

        #region VARIABLES
        /// <summary>Récupération de recherche étendu dans les pref</summary>
        private Boolean bGefPrefVals = false;
        /// <summary>
        /// Type de champ de liaison
        ///     0 => Champ de liaison classique (ligne sélectionnable avant validation)
        ///     1 => Recherche avancée (cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée)
        ///     3 => MRU (permet de sélectionner au clique)
        /// </summary>
        protected eFinderList.Mode _fileMode = eFinderList.Mode.ADVANCED_SEARCH;

        /// <summary>
        /// Valeur de recherche initiale
        /// </summary>
        private String _sInitSearch = String.Empty;

        private eConst.ShFileCallFrom _callFrom = eConst.ShFileCallFrom.CallFromNavBar;

        #endregion

        #region ACCESSEURS
        /// <summary>Objet Champ de liaison métier</summary>
        public eFinderList Finder
        {
            get { return (eFinderList)_list; }
        }
        /// <summary>Rattachements des fiches principales</summary>
        public eFileTools.eParentFileId ParentFileId
        {
            get { return (Finder != null) ? Finder.ParentFileId : null; }
        }
        /// <summary>Liste des colonnes affichée</summary>
        public HashSet<Int32> ListCol
        {
            get { return (Finder != null) ? Finder.DisplayCol : null; }
        }
        /// <summary>Nombre de caractères minimum lors de la création d'une fiche principale</summary>
        public Int32 SearchLimit
        {
            get { return (Finder != null) ? Finder.SearchLimit : 0; }
        }
        /// <summary>Champ principale construit automatiquement</summary>
        public Boolean AutobuildName
        {
            get { return (Finder != null) ? Finder.BAutobuildName : false; }
        }
        /// <summary>
        /// Permet d'ajouter l'attribut de nameonly dans la cellule si à vrai
        /// </summary>
        public Boolean AddNameOnly
        {
            get;
            set;
        }
        /// <summary>Indique si l'Option Rechercher sur toutes les fiches TABLE est affichée dépend de UserValue</summary>
        public Boolean EnabledSearchAll
        {
            get { return (Finder != null) ? Finder.BEnabledSearchAll : true; }
            set { if (Finder != null) Finder.BEnabledSearchAll = value; }
        }
        /// <summary>Liste des descid Système (imposés à l'utilisateur)</summary>
        public HashSet<Int32> ListColSpec
        {
            get { return (Finder != null) ? Finder.ListColSpec : null; }
        }
        /// <summary>
        /// Pas de redirection vers la fiche lors de la validation
        /// </summary>
        public bool NoLoadFileAfterValidation { get; set; }
        /// <summary>
        /// Point d'appel
        /// </summary>
        public eConst.ShFileCallFrom CallFrom
        {
            get
            {
                return _callFrom;
            }

            set
            {
                _callFrom = value;
            }
        }


        #endregion

        /// <summary>
        /// Constructeur de l'extrème
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nTargetTab">Table sur laquelle on recherche</param>
        /// <param name="nTabFrom">Table appelante</param>
        /// <param name="nFileId">Id de la fiche appelante</param>
        /// <param name="sSearch">The s search.</param>
        /// <param name="bHisto">if set to <c>true</c> [b histo].</param>
        /// <param name="currentSearchMode">Spécifie le mode de recherche choisi (standard, étendue, étendue sur toutes les rubriques affichées)</param>
        /// <param name="bPhoneticSearch">if set to <c>true</c> [b phonetic search].</param>
        /// <param name="bAllRecord">if set to <c>true</c> [b all record].</param>
        /// <param name="nDescid">DescId du champ</param>
        /// <param name="nDispValue">The n disp value.</param>
        /// <param name="listDisplayedUserValueField">Liste des infos des champs affichés pour Uservalue (IsFound$|$Parameter$|$Value$|$Label)</param>
        /// <param name="listCol">Liste des colonnes demandées à l'affichage (vide pour le choix par défaut)</param>
        /// <param name="listColSpec">The list col spec.</param>
        /// <param name="fileMode">Type de champ de liaison
        /// 0 =&gt; Champ de liaison classique (ligne sélectionnable avant validation)
        /// 1 =&gt; Recherche avancée (cliquable et permet de rediriger vers la fiche correspondant à la ligne sélectionnée)
        /// 3 =&gt; MRU (permet de sélectionner au clique)</param>
        /// <param name="bMulti">if set to <c>true</c> [b multi].</param>
        public eFinderListRenderer(ePref pref, Int32 height, Int32 width, Int32 nTargetTab, Int32 nTabFrom, Int32 nFileId, string sSearch, Boolean bHisto, eFinderList.SearchMode currentSearchMode, Boolean bPhoneticSearch, Boolean bAllRecord, Int32 nDescid, List<Int32> nDispValue, List<UserValueField> listDisplayedUserValueField, List<Int32> listCol, List<Int32> listColSpec, eFinderList.Mode fileMode, Boolean bMulti = false)
            : base(pref)
        {
            _height = HeightList;  //Hauteur de la liste : fixée en css
            _width = width - (width * (2 + 2) / 100); //Largeur de la liste : taille de la fenêtre - 2% et 2% marge à droite et à gauche de la liste

            _rType = RENDERERTYPE.Finder;
            _tab = nTargetTab;
            _bMulti = bMulti;


            if (bGefPrefVals)
            {
                #region Récupération de recherche étendu dans les pref
                // #62 380 - et/ou étendue sur toutes les rubriques
                // la recherche sur toutes les rubriques n'est, pour l'instant, jamais activée par défaut pour des questions de performances
                // S'il est décidé, un jour, de mémoriser cette option, récupérer la valeur mémorisée ici
                bool bExtended = (Pref.GetConfig(eLibConst.PREF_CONFIG.SEARCHEXTENDED) == "1");
                bool bExtendedAllFields = false;

                currentSearchMode = eFinderList.SearchMode.STANDARD;
                if (bExtended)
                {
                    if (bExtendedAllFields)
                        currentSearchMode = eFinderList.SearchMode.EXTENDED_ALLFIELDS;
                    else
                        currentSearchMode = eFinderList.SearchMode.EXTENDED;
                }
                #endregion
            }

            //#33286 : valeur de recherche initiale
            // #42934 CRU : Pour que la recherche se fasse, il ne doit pas y avoir de valeurs multiples
            if (!(bMulti && sSearch.IndexOf(';') != -1))
                _sInitSearch = sSearch;

            _fileMode = fileMode;
            //_nSearchType = nSearchType;

            //à faire après l'initialisation des accesseurs
            initList(nTabFrom, nFileId, sSearch, bHisto, currentSearchMode, bPhoneticSearch, bAllRecord, nDescid, nDispValue, listDisplayedUserValueField, listCol, listColSpec);
        }

        /// <summary>
        ///  MCR/GCH Initialisaton de l object liste
        ///  
        /// </summary>
        /// <param name="nTabFrom"></param>
        /// <param name="nFileId"></param>
        /// <param name="sSearch"></param>
        /// <param name="bHisto"></param>
        /// <param name="currentSearchMode">Spécifie le mode de recherche choisi (standard, étendue, étendue sur toutes les rubriques affichées)</param>
        /// <param name="bPhoneticSearch"></param>
        /// <param name="bAllRecord"></param>
        /// <param name="nDescid"></param>
        /// <param name="nDispValue"></param>
        /// <param name="listDisplayedUserValueField"></param>
        /// <param name="listCol"></param>
        /// <param name="listColSpec"></param>
        /// <param name="bMulti"></param>
        protected virtual void initList(Int32 nTabFrom, Int32 nFileId, string sSearch, Boolean bHisto, eFinderList.SearchMode currentSearchMode, Boolean bPhoneticSearch, Boolean bAllRecord, Int32 nDescid, List<Int32> nDispValue, List<UserValueField> listDisplayedUserValueField, List<Int32> listCol, List<Int32> listColSpec, Boolean bMulti = false)
        {

            _list = eFinderList.CreateFinderList(Pref, _tab, nTabFrom, nFileId, nDescid,
                _sInitSearch, bHisto, currentSearchMode, bPhoneticSearch, bAllRecord, nDispValue, listDisplayedUserValueField, listCol, listColSpec, _fileMode);


            if (!string.IsNullOrEmpty(_list.ErrorMsg))
            {
                _eException = _list.InnerException;
                _sErrorMsg = _list.ErrorMsg;
                return;
            }
        }

        /// <summary>
        /// Dans ce cas, la liste est généré directement dans le construceur
        /// </summary>
        protected override void GenerateList()
        {
            //
        }

        /// <summary>
        /// Entête de choix d'onglet visible seulement pour la recherche avancée
        /// </summary>
        /// <returns></returns>
        public HtmlGenericControl GetFinderSearchTableHeader()
        {
            HtmlGenericControl ulTabHeader = new HtmlGenericControl("ul");

            //Pas de code vu que non maquetté et qu'il était un non sens en v7.

            return ulTabHeader;
        }

        /// <summary>Rendu du haut du champ de liaison</summary>
        /// <returns>Rendu généré</returns>
        public List<eUlCtrl> GetFinderTop()
        {
            bGefPrefVals = true;

            List<eUlCtrl> listUL = new List<eUlCtrl>();

            //Si recherche avancée on affiche les entête de choix d'onglet
            //GCH Commenté : Pas de code vu que non maquetté et qu'il était un non sens en v7.
            //if (_nSearchType == 0)
            //    listUL.Add(GetFinderSearchTableHeader());

            eUlCtrl ulSearch = new eUlCtrl();
            listUL.Add(ulSearch);
            ulSearch.Attributes.Add("class", "LnkTop");

            eLiCtrl li = null;

            #region Block de recherche
            li = new eLiCtrl();
            ulSearch.Controls.Add(li);
            li.Attributes.Add("class", "srchFld");

            Panel panSrch = new Panel();
            li.Controls.Add(panSrch);
            panSrch.CssClass = "Lnk_srch-text";
            panSrch.Controls.Add(new LiteralControl(string.Concat(eResApp.GetRes(_list.Pref, 1040), " : ")));  //1040 - "Rechercher"

            Panel fieldWrapper = new Panel();
            fieldWrapper.CssClass = "searchFieldWrapper";
            li.Controls.Add(fieldWrapper);

            HtmlInputText srchArea = new HtmlInputText();
            fieldWrapper.Controls.Add(srchArea);
            srchArea.Attributes.Add("class", "Lnk_srch-inpt");
            srchArea.ID = "eTxtSrch";
            srchArea.Attributes.Add("onKeyUp", "FindValues(event, this.value);");
            srchArea.Attributes.Add("maxlength", "100");
            srchArea.Value = _sInitSearch;

            panSrch = new Panel();
            fieldWrapper.Controls.Add(panSrch);

            string cssIcon = (!String.IsNullOrEmpty(_sInitSearch)) ? "icon-edn-cross" : "icon-magnifier";
            string srchState = (!String.IsNullOrEmpty(_sInitSearch)) ? "on" : "off";
            panSrch.ID = "lnkBtnSrch";
            panSrch.CssClass = $"{cssIcon} srchFldImg";
            panSrch.Attributes.Add("onclick", "BtnSrch();");
            panSrch.Attributes.Add("srchState", srchState);
            #endregion

            #region Recherche étendue
            // #62 380 - Remplacement de "recherche étendue" par 3 boutons radio "Recherche standard", "Recherche étendue", "Recherche étendue sur toutes les rubriques"

            //li = new eLiCtrl();
            //ulSearch.Controls.Add(li);
            //GetChkLib(li, "chkValue_Extended", eResApp.GetRes(_list.Pref, 74), "StartSearch(true);", Finder.BSearchExtended); //74   :   Recherche étendue

            IDictionary<eFinderList.SearchMode, string> searchModeLabels = new Dictionary<eFinderList.SearchMode, string>
            {
                { eFinderList.SearchMode.STANDARD, eResApp.GetRes(Pref, 984) }, // Recherche standard
                { eFinderList.SearchMode.EXTENDED, eResApp.GetRes(Pref, 74) }, // Recherche étendue
                { eFinderList.SearchMode.EXTENDED_ALLFIELDS, eResApp.GetRes(Pref, 8679) } // Recherche étendue sur toutes les rubriques affichées
            };
            IDictionary<eFinderList.SearchMode, string> searchModeTooltips = new Dictionary<eFinderList.SearchMode, string>
            {
                { eFinderList.SearchMode.STANDARD, eResApp.GetRes(Pref, 8680) }, // Recherche standard : le titre de la fiche débute par le texte saisi
                { eFinderList.SearchMode.EXTENDED, eResApp.GetRes(Pref, 8681) }, // Recherche étendue : le titre de la fiche contient le texte saisi
                { eFinderList.SearchMode.EXTENDED_ALLFIELDS, eResApp.GetRes(Pref, 8682) } // Recherche étendue sur toutes les rubriques affichées : l'une des rubriques affichées contient le texte saisi
            };
            foreach (eFinderList.SearchMode searchMode in Enum.GetValues(typeof(eFinderList.SearchMode)))
            {
                li = new eLiCtrl();
                ulSearch.Controls.Add(li);
                li.Controls.Add(eTools.GetRadioButton(String.Concat("searchMode_", (int)searchMode), "searchMode", Finder.CurrentSearchMode == searchMode, searchModeLabels[searchMode], true, "StartSearch(true)", ((int)searchMode).ToString(), searchModeTooltips[searchMode], true));
            }
            #endregion

            // Création d'une nouvelle liste pour séparation des options ci-dessous vis-à-vis du champ de recherche et des radio buttons du mode de recherche
            ulSearch = new eUlCtrl();
            listUL.Add(ulSearch);
            ulSearch.Attributes.Add("class", "LnkTopSecondSearchOptions");
            // Et d'une ligne vierge sous le contrôle de recherche pour positionner la seconde ligne d'options sous la première
            li = new eLiCtrl();
            ulSearch.Controls.Add(li);

            #region Recherche phonétique
            if (!Finder.BHidePhoneticSearchCheckbox)
            {
                li = new eLiCtrl();
                ulSearch.Controls.Add(li);
                GetChkLib(li, "chkValue_PhoneticSearch", eResApp.GetRes(_list.Pref, 1239), "StartSearch(true);"); //1239  :   Recherche phonétique
            }
            #endregion

            #region Rechercher sur toutes les fiches <<TABLE>>
            li = new eLiCtrl();
            ulSearch.Controls.Add(li);



            GetChkLib(li,
                    "chkValue_AllRecord",
                    string.Concat(eResApp.GetRes(_list.Pref, 93),
                    " ",
                    eLibTools.GetPrefName(_list.Pref, Finder.TargetTab)),
                    "StartSearch(true);",
                    Finder.BAllRecordChk,
                    !Finder.BEnabledSearchAll); //93 :   Rechercher sur toutes les fiches + TABLE
            #endregion

            eUlCtrl ulOptions = new eUlCtrl();

            ulOptions.Attributes.Add("class", "LnkTopOpt");
            HyperLink link;
            HtmlGenericControl btnLink;

            #region AJOUTER
            //Ajout autorisé seulement si Droits de traitement sur nouvelle fiche pour l'auser en cours et que pas un champ de recherche vers une source externe
            if (Finder.IsTreatAllowed && Finder.DataSourceId <= 0 && this.CallFrom != eConst.ShFileCallFrom.CallFromPurpleFile)
            {
                string strToolTipText = string.Concat(eResApp.GetRes(_list.Pref, 76), " ", eLibTools.GetPrefName(_list.Pref, Finder.TargetTab));    //76    :   Nouvelle fiche       Nom de la table

                li = new eLiCtrl();
                ulOptions.Controls.Add(li);
                li.Attributes.Add("class", "lnkFieldsAdd");

                string sClick = string.Concat("AddFileFromPopup('", strToolTipText.Replace("'", "\\'"), "',",
                    (_fileMode == eFinderList.Mode.ADVANCED_SEARCH) ? "'1'" : "null", ", ",
                    (int)this.CallFrom,
                    ", ", (this.NoLoadFileAfterValidation) ? "true" : "false"
                    , ")");

                li.Attributes.Add("onclick", sClick);


                btnLink = new HtmlGenericControl("span");
                li.Controls.Add(btnLink);
                btnLink.Attributes.Add("id", "spanAddFileFromPopup");
                btnLink.Attributes.Add("class", "icon-add");


                link = new HyperLink();
                li.Controls.Add(link);
                link.Attributes.Add("title", strToolTipText);
                link.Controls.Add(new LiteralControl(eResApp.GetRes(_list.Pref, 18))); //18 - Ajouter

            }
            #endregion

            #region RUBRIQUES
            li = new eLiCtrl();
            ulOptions.Controls.Add(li);
            li.Attributes.Add("class", "lnkFieldsSel");

            btnLink = new HtmlGenericControl("span");
            li.Controls.Add(btnLink);
            btnLink.Attributes.Add("class", "icon-rubrique");
            btnLink.Attributes.Add("onclick", string.Concat("selCol(", Finder.TargetTab, ",", Finder.TabFrom, ");"));

            link = new HyperLink();
            li.Controls.Add(link);
            link.Attributes.Add("href", string.Concat("javascript:selCol(", Finder.TargetTab, ",", Finder.TabFrom, ");"));
            link.Attributes.Add("title", eResApp.GetRes(_list.Pref, 652));   //652 - Cliquez ici pour sélectionner des rubriques            
            link.Controls.Add(new LiteralControl(eResApp.GetRes(_list.Pref, 20))); //20 - Rubriques            
            #endregion

            #region Historique
            if (_list.HistoInfo.Has)
            {
                li = new eLiCtrl();
                ulOptions.Controls.Add(li);

                HtmlGenericControl btnContainer = new HtmlGenericControl("div");
                eTools.BuildHistoBtn(this.Pref, btnContainer, _list.HistoInfo.Has, false, "finder");

                li.Controls.Add(btnContainer);
            }
            #endregion

            if (ulOptions.Controls.Count > 0)
                listUL.Add(ulOptions);





            btnLink = null;
            link = null;
            ulOptions = null;
            srchArea = null;
            panSrch = null;
            li = null;
            ulSearch = null;

            return listUL;
        }

        /// <summary>
        /// Génération du rendu d'une case à coché Eudo avec son libellé
        /// </summary>
        /// <param name="hgcDest">Control qui intégrera la CheckBox et son libellé</param>
        /// <param name="sId">Id de la checkbox</param>
        /// <param name="sLib">Text à afficher</param>
        /// <param name="sOnClick">Methode JS appelé au click sur l'élément</param>
        /// <param name="bChecked">Indique si la case doit être coché ou pas</param>
        /// <param name="bDisabled">Indique si la case doit être disabled en javascript</param>
        private void GetChkLib(WebControl hgcDest, string sId, string sLib, string sOnClick, Boolean bChecked = false, Boolean bDisabled = false)
        {
            eCheckBoxCtrl cb = new eCheckBoxCtrl(bChecked, bDisabled);
            hgcDest.Controls.Add(cb);
            if (!bDisabled)
                cb.AddClick(sOnClick);
            cb.AddClass("chkAction");
            cb.ID = sId;
            cb.Attributes.Add("name", "chkValue");
            cb.AddText(sLib);
            cb = null;
        }

        /// <summary>
        /// Récupération du rendu de la liste du finder
        /// </summary>
        /// <returns></returns>
        public Panel GetFinderList(out string strError)
        {
            strError = string.Empty;
            Panel listContent = new Panel();
            listContent.ID = "listContent";
            listContent.CssClass = String.Concat("tabeul", _bMulti ? "multi" : "");

            eRenderer myRend = (eRenderer)eRendererFactory.CreateFinderRenderer(this);

            if (myRend.ErrorMsg.Length == 0)
            {
                //déplace les éléments du conteneur généré (myMainList) vers le conteneur final (listcontent)
                // On ne peut pas ajouter directement myMainList dans listcontent : il ne faut 
                // pas ajouter le div englobant de myMainList (listContent.Controls.resp(myRend.PgContainer);)
                // , cela perturbe js et css               
                while (myRend.PgContainer.Controls.Count > 0)
                {
                    listContent.Controls.Add(myRend.PgContainer.Controls[0]);
                }
            }
            else
            {
                strError = myRend.ErrorMsg;
            }
            myRend = null;

            return listContent;
        }

        bool _bNoCascadeFromPP = false;
        bool _bNoCascadeFromPM = false;

        /// <summary>
        /// (redéfinit pour avoir le nombre de ligne totale à afficher car l'on affiche entre 0 et 200 ligne max)
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            // Pas de paging sur ces mode de liste
            _rows = _list.ListRecords.Count;

            _bNoCascadeFromPP = _list.ViewMainTable.NoCascadePPPM;
            _bNoCascadeFromPM = _list.ViewMainTable.NoCascadePMPP;

            return true;
        }

        /// <summary>
        /// Traitement de fin de génération
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            base.End();

            #region Droit d'ajout
            //Ajout autorisé seulement si Droits de traitement sur nouvelle fiche pour l'auser en cours et que pas un champ de recherche vers une source externe
            _tblMainList.Attributes.Add("addpermission", (Finder.IsTreatAllowed && (Finder.DataSourceId <= 0)) ? "1" : "0");
            #endregion

            #region Nombre de ligne
            if (Finder != null && Finder.ListRecords != null)
                _tblMainList.Attributes.Add("eNbResult", Finder.ListRecords.Count.ToString());
            else
                _tblMainList.Attributes.Add("eNbResult", "0");
            #endregion

            #region Nombre de caractère minimum avant recherche
            if (Finder != null && Finder.ListRecords != null)
                _tblMainList.Attributes.Add("eSearchLimit", Finder.SearchLimit.ToString());
            else
                _tblMainList.Attributes.Add("eSearchLimit", "0");
            #endregion

            if (AddNameOnly)
                _tblMainList.Attributes.Add("eno", "1");

            //CNA - #46232 - On met l'ednmode en finderlist et on rajoute le userid pour pouvoir faire des modifications dans prefadv
            _tblMainList.Attributes["ednmode"] = "finderlist";

            _tblMainList.Attributes.Add("eccpm", _bNoCascadeFromPM ? "1" : "0");
            _tblMainList.Attributes.Add("eccpp", _bNoCascadeFromPP ? "1" : "0");


            #region Message indiquant que les résultats sont limités
            if (((eFinderList)_list).LimitedResults)
            {

                HtmlGenericControl p = new HtmlGenericControl("p");
                p.Attributes.Add("class", "pInfo");
                p.InnerText = String.Format(eResApp.GetRes(_ePref, 1885), eModelConst.MAX_ROWS_FINDER);
                _pgContainer.Controls.AddAt(0, p);
            }
            #endregion



            return true;
        }

        /// <summary>
        /// rajoute dans la ligne d'en-tête la colonne contenant les icones pour les annexes, taches periodiques...
        /// </summary>
        /// <param name="headerRow"></param>
        /// <returns>la taille prise par les icones</returns>
        protected override void HeaderListIcon(TableRow headerRow)
        {
            if (_bMulti)
                base.HeaderListIcon(headerRow);
        }

        /// <summary>
        /// Pas d'icone pour ce mode liste
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="idxLine"></param>
        /// <param name="sLstRulesCss"></param>
        /// <param name="lIcon"></param>
        protected override void BodyListIcon(eRecord row, TableRow trDataRow, Int32 idxLine, ref string sLstRulesCss, List<string> lIcon)
        {
            if (_bMulti)
                base.BodyListIcon(row, trDataRow, idxLine, ref sLstRulesCss, lIcon);
        }

        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, int idxLine)
        {
            trRow.ID = String.Concat("row", idxLine);
        }

        /// <summary>
        /// Ajoute les specifités sur la cell en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="fieldRow">field record</param>
        /// <param name="trCell">Objet cellule courant</param>
        /// <param name="idxCell">index de la colonne</param>
        protected override void CustomTableCell(eRecord row, TableRow trRow, eFieldRecord fieldRow, TableCell trCell, Int32 idxCell)
        {
            if (fieldRow.FldInfo.Descid == 201 && fieldRow.FldInfo.PopupDescId <= 0 && AddNameOnly)
                trCell.Attributes.Add("eNO", fieldRow.Value); //NameOnly
            if (!fieldRow.RightIsVisible)
                trCell.Attributes.Add("eNotV", "1"); //NotVisible

        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sValue"></param>
        protected override void GetHTMLMemoControl(EdnWebControl ednWebCtrl, string sValue)
        {
            WebControl webCtrl = ednWebCtrl.WebCtrl;

            HtmlInputHidden memoValue = new HtmlInputHidden();
            memoValue.ID = string.Concat(webCtrl.ID, "hid");
            memoValue.Value = sValue;
            webCtrl.Controls.Add(memoValue);
            memoValue = null;
            HtmlGenericControl iFrame = new HtmlGenericControl("iFrame");

            iFrame.Attributes.Add("class", "eME");
            iFrame.Attributes.Add("src", "blank.htm");
            iFrame.ID = string.Concat(webCtrl.ID, "ifr");
            iFrame.Style.Add("width", "99%"); // largeur légèrement inférieure à 100% pour éviter que l'iframe dépasse de la cellule et afficher complètement la bordure verte après édition
            //Dans le cas des MRU on ajoute ce paramètre pour que les lignes ne soient pas surdimensionnées
            if (_fileMode == eFinderList.Mode.MRU)
                iFrame.Style.Add("height", "100%");

            iFrame.Attributes.Add("src", "about:blank");
            webCtrl.Controls.Add(iFrame);
            iFrame = null;
        }

        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row">Objet eRecord de la ligne en cours</param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            if (!_bMulti)
                return;
            TableCell cellSelect = new TableCell();
            cellSelect.CssClass = String.Concat(sAltLineCss, " icon");

            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(row.IsMarked, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");

            chkSelect.AddClick("return;");
            chkSelect.Attributes.Add("sf", "1");

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);
        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {
            if (!_bMulti)
                return;

            // Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            TableHeaderCell cellSelect = new TableHeaderCell();
            cellSelect.CssClass = "head icon";
            cellSelect.Attributes.Add("nomove", "1");
            cellSelect.Attributes.Add("width", String.Concat(_sizeTdCheckBox, "px"));

            eCheckBoxCtrl chkSelectAll = new eCheckBoxCtrl(false, false);
            chkSelectAll.ID = String.Concat("chkAll_", VirtualMainTableDescId);

            chkSelectAll.ToolTipChkBox = eResApp.GetRes(Pref, 6302);
            chkSelectAll.AddClass("chkAction");
            chkSelectAll.AddClick("selAllValues(this);");
            chkSelectAll.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cellSelect.Controls.Add(chkSelectAll);

            headerRow.Cells.Add(cellSelect);
        }
    }
}