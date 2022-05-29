using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu du mode liste
    /// </summary>
    public class eListMainRenderer : eRenderer
    {

        /// <summary>Objet d'accès aux données</summary>
        public eList _list;

        #region propriété protected

        /// <summary>Somme de la taille des colonnes défini par l'utilisateur</summary>
        protected Int64 _userColWidth = 0;

        /// <summary>Nombre de colonnes affichées</summary>
        protected Int32 _nbDisplayCol = 0;

        /// <summary>Nombre de colonnes affichées en width automatique</summary>
        protected Int32 _nbDisplayColAutoWidth = 0;


        /// <summary>Numéro de la page affichée de la liste dans le cadre de la pagination</summary>
        protected Int32 _page;

        /// <summary>Nombre de rang par page</summary>
        protected Int32 _rows;
        /// <summary>Nombre de rang par page demandée par l'utilisateur</summary>
        protected Int32 _rowsCalculated;


        protected String _sFirstColAlias = String.Empty;

        protected Double _nTotalSize = 0;

        /// <summary>Indique si une colonne est calculé (somme)</summary>
        protected Boolean _existComputedCol = false;
        /// <summary>Div principale de la liste</summary>
        protected Panel _divmt = null;

        /// <summary>Tableau de la liste</summary>
        protected System.Web.UI.WebControls.Table _tblMainList = null;

        /// <summary>Tableau des valeurs max de chaque colonnes</summary>
        protected System.Web.UI.WebControls.Table _lenList = null;

        /// <summary>Tableau des valeurs text de l'en-tête</summary>
        protected System.Web.UI.WebControls.Table _lenHeadList = null;


        // CSS fixe
        /// <summary>Taille de la cellule de la checkbox</summary>
        protected Int32 _sizeTdCheckBox = 24;

        /// <summary>Taille des cellules des icones</summary>
        protected Int32 _sizeTdIcon = 22;

        /// <summary>Index de la colonne de l'icone PJ</summary>
        protected Int32 _idxCellPjCount = -1;

        /// <summary> Index de la colonne de l'icone schedule</summary>
        protected Int32 _idxCellSched = -1;

        /// <summary> Index de la colonne de l'icone fusion</summary>
        protected Int32 _idxCellMerge = -1;

        /// <summary>
        /// CSS de la liste
        /// </summary>
        protected String _sMainTabCss = "mTab mTabLst";

        /// <summary>
        /// Les colonnes sont-elles filtrables ?
        /// </summary>
        protected Boolean _isTableFilterable = true;

        protected string _jsSortAsc = "sla(event);";
        protected string _jsSortDesc = "sld(event);";
        protected string _jsDoFilter = "dof(this);";
        #endregion

        #region accesseurs



        /// <summary>
        /// Correspond au descid de la table demandé pour la liste
        /// Dans certains cas, il ne s'agit pas d'un "vrai" table, par exemple pour
        /// les doublons...
        /// </summary>
        public virtual Int32 VirtualMainTableDescId
        {

            get
            {
                //Dans le cas des listes standard, le descid de la
                // table demandée peut-être directement récupérer sur l'objet liste
                // Ce n'est pas le cas dans certains bookmark (doublon, affaire depuis affaire....)
                return _list.ViewMainTable.DescId;
            }
        }

        /// <summary>
        /// Indique si le champ de recherche doit être affiché
        /// </summary>
        public virtual Boolean DrawSearchField
        {
            get { return (_list != null && _list.ViewMainTable.EdnType == EdnType.FILE_MAIN && _list.MainField.Format == FieldFormat.TYP_CHAR); }
        }

        /// <summary>
        /// Tableau principal
        /// </summary>
        public System.Web.UI.WebControls.Table MainHtmlTable { get { return _tblMainList; } }

        #endregion

        /// <summary>
        /// Retourne un renderer eListRendererMain
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="nTab">Descid de la table à obtenir</param>        
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nPage">Page</param>
        /// <param name="nRow">Nombdre de ligne par page</param>
        internal static eListMainRenderer GetMainListRenderer(ePref pref, Int32 nTab, Int32 height, Int32 width, Int32 nPage, Int32 nRow, Boolean bFullList = true)
        {
            // Instanciation
            eListMainRenderer elRenderer;


            switch (nTab)
            {

                case (int)TableType.PJ:
                    elRenderer = ePjMainListRenderer.GetPjMainListRenderer(pref);
                    break;
                case (int)TableType.USER:
                    elRenderer = eListUserRenderer.GetListUserRenderer(pref);
                    break;
                case (int)TableType.RGPDTREATMENTSLOGS:
                    elRenderer = eListRGPDTreatmentLogRenderer.GetListRGPDTreatmentLogRenderer(pref);
                    break;
                case (int)TableType.XRMHOMEPAGE:
                    elRenderer = eListMainXrmHomPageRenderer.CreateXrmHomPageListRenderer(pref, nPage, nRow, width, height);
                    break;
                default:
                    elRenderer = new eListMainRenderer(pref);
                    break;
            }


            // Initialistion
            elRenderer._height = height;
            elRenderer._width = width;
            elRenderer._page = nPage;
            elRenderer._rows = nRow;
            elRenderer._rowsCalculated = nRow;
            elRenderer._tab = nTab;
            elRenderer._bFullList = bFullList;

            return elRenderer;

        }





        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        protected eListMainRenderer(ePref pref)
        {
            Pref = pref;
            _rType = RENDERERTYPE.ListRendererMain;
            // TODO : dernier paramètre DOCOUNT a récupérer du manager
            //On récupère la liste
        }



        #region méthode protected

        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected virtual void GenerateList()
        {

            if (_tab == TableType.USER.GetHashCode() && Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
            {
                _list = eListMainUser.GetListMainUser(Pref, _tab, _rows, _page);
                _list.Generate();
            }
            else
            {
                _list = eListFactory.CreateMainList(Pref, _tab, _page, _rows, true, _bFullList);
            }


        }

        /// <summary>
        /// Init de eListRendererMain
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {

                //Génération de l'objet _list
                GenerateList();
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eListMainRenderer.Init : Une erreur s'est produite lors de la génération de la liste : ", Environment.NewLine,
                                            e.Message, Environment.NewLine,
                                            e.StackTrace);
                this._eException = e;
                return false;
            }

            //Si la création de eList est  en erreur, faire remonter l'erreur
            if (_list == null)
            {
                _sErrorMsg = String.Concat("eListMainRenderer.Init : _list est null");
                return false;
            }
            else if (_list.ErrorMsg.Length > 0 || _list.InnerException != null)
            {
                _sErrorMsg = String.Concat("ERR eList : ", _list.ErrorMsg);
                _eException = _list.InnerException;
                _nErrorNumber = _list.ErrorType; // CRU

                return false;
            }

            //On récupère le nombre de ligne par page effectif
            _rows = _list.RowsByPage;
            _nbDisplayCol = _list.GetParam<Int32>("nbDisplayCol");
            _nbDisplayColAutoWidth = _list.GetParam<Int32>("nbDisplayColAutoWidth");
            _userColWidth = _list.GetParam<Int32>("UserColWidth");


            return true;

        }

        /// <summary>
        /// Construit la structure HTML de l'élément
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            FillContainer();


            Head();


            Body();

            return true;
        }


        /// <summary>
        /// Ajustement de la taille des colonnes
        /// </summary>
        protected virtual void AdjustCol()
        {
            #region resize auto de colonnes

            _lenList.Attributes.Add("border", "1px");
            _lenList.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            _lenList.Style.Add(HtmlTextWriterStyle.Left, "-50000px");
            _lenList.CssClass = "mTab mTabLen";

            _lenHeadList.Attributes.Add("border", "1px");
            _lenHeadList.Style.Add(HtmlTextWriterStyle.Position, "absolute");
            _lenHeadList.Style.Add(HtmlTextWriterStyle.Left, "-50000px");
            _lenHeadList.CssClass = "mTab mTabHeadLen";

            TableRow rowLen = new TableRow();
            _lenList.Rows.Add(rowLen);
            TableRow rowHead = new TableRow();
            _lenHeadList.Rows.Add(rowHead);

            TableCell cellLen = null;
            TableCell cellHead = null;

            foreach (KeyValuePair<String, ListColMaxValues> keyValue in _colMaxValues)
            {
                cellLen = new TableCell();
                cellLen.ID = String.Concat("LEN_" + keyValue.Key);
                cellLen.Text = System.Web.HttpUtility.HtmlEncode(keyValue.Value.ColMaxValue);
                cellLen.Attributes.Add("nowrap", String.Empty);
                cellLen.CssClass = keyValue.Value.AdditionalCss;
                rowLen.Cells.Add(cellLen);

                cellHead = new TableCell();
                cellHead.ID = String.Concat("LENH_" + keyValue.Key);
                cellHead.Text = System.Web.HttpUtility.HtmlEncode(keyValue.Value.HeadValue);
                cellHead.Attributes.Add("nowrap", String.Empty);
                rowHead.Cells.Add(cellHead);
            }

            #endregion
        }

        /// <summary>
        /// Traitement de fin de génération
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            AdjustCol();

            Type listType = _list.GetType();

            // Informations des filtres
            if (listType == typeof(eListMain) || listType == typeof(eListMainUser) || listType == typeof(eListMainRGPDTreatmentLog) || listType == typeof(ePjMainList))
                AddFilterTip();

            _tblMainList.Attributes.Add("ednmode", "list");
            _tblMainList.Attributes.Add("eCalendarEnabled", _list.ViewMainTable.CalendarEnabled ? "1" : "0");

            return true;
        }


        #endregion

        #region Méthodes interne à la génération de liste
        /// <summary>
        /// remplit le tableau html avec les données de la liste
        /// </summary>
        protected void FillContainer()
        {
            _divmt = new Panel();
            _divmt.ID = String.Concat("div", VirtualMainTableDescId);
            _divmt.CssClass = "divmTab";

            _divmt.Attributes.Add("onscroll", "oListScrollManager.clear(event);");

            _pgContainer.Controls.Add(_divmt);

            // Création du guide de déplacement des céllules.
            Panel cellGuide = new Panel();
            cellGuide.CssClass = "colGuide icon-title_sep";
            cellGuide.ID = "colGuide";
            _divmt.Controls.Add(cellGuide);


            // Tableau de la liste
            _tblMainList = new System.Web.UI.WebControls.Table();
            _tblMainList.ID = String.Concat("mt_", VirtualMainTableDescId);
            _tblMainList.CssClass = _sMainTabCss;

            SetPagingInfo();

            // Init des attributes de la table
            _tblMainList.Attributes.Add("CellPadding", "0");
            _tblMainList.Attributes.Add("CellSpacing", "0");
            _tblMainList.Attributes.Add("w", _width.ToString());
            _tblMainList.Attributes.Add("h", _height.ToString());
            _tblMainList.Attributes.Add("edntyp", _list.ViewMainTable.EdnType.GetHashCode().ToString());

            //MCR 38712 : la liste doit occuper tout l ecran pour toutes les colonnes, sur un nouveau filtre
            // MOU commenté suite au regression avant sortie de version 10.108
            // _tblMainList.Attributes.CssStyle.Add("width", "100%");


            _divmt.Controls.Add(_tblMainList);

            // Tableau des valeurs max de chaque colonnes
            _lenList = new System.Web.UI.WebControls.Table();
            _lenList.ID = String.Concat("lenCol_", VirtualMainTableDescId);
            _divmt.Controls.Add(_lenList);

            // Tableau des valeurs text de l'en-tête
            _lenHeadList = new System.Web.UI.WebControls.Table();
            _lenHeadList.ID = String.Concat("lenHeadCol_", VirtualMainTableDescId);
            _divmt.Controls.Add(_lenHeadList);

            // Div de champ caché
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", VirtualMainTableDescId);

            #region fiches marquées
            Int32 nFileId = 0;
            String sName = String.Empty;
            Int32 nNbFiles = 0;

            if (_list.GetParam<Boolean>("hasMarkedFileSel"))
            {
                nFileId = _list.GetParam<Int32>("MarkedFileId");
                sName = _list.GetParam<String>("MarkedFileName");
                nNbFiles = _list.GetParam<Int32>("MarkedFileNbFiles");
            }

            //TODO : regrouper les 3 input en 1 seul pour gagner de la place
            //Id de la sélection des fiches marquées
            HtmlInputHidden inptMarkedId = new HtmlInputHidden();
            inptMarkedId.ID = String.Concat("markedFileId_", VirtualMainTableDescId);
            inptMarkedId.Value = nFileId.ToString();
            _divHidden.Controls.Add(inptMarkedId);

            //Label de la fiche marquée
            HtmlInputHidden inptMarkedLabel = new HtmlInputHidden();
            inptMarkedLabel.ID = String.Concat("markedLabel_", VirtualMainTableDescId);
            inptMarkedLabel.Value = sName;
            _divHidden.Controls.Add(inptMarkedLabel);

            //Nombre de fiche marquée de la sélection
            HtmlInputHidden inptMarkedNb = new HtmlInputHidden();
            inptMarkedNb.ID = String.Concat("markedNb_", VirtualMainTableDescId);
            inptMarkedNb.Value = nNbFiles.ToString();
            _divHidden.Controls.Add(inptMarkedNb);

            using (eudoDAL dal = eLibTools.GetEudoDAL(Pref))
            {
                dal.OpenDatabase();

                if (VirtualMainTableDescId == TableType.PP.GetHashCode()
                || VirtualMainTableDescId == TableType.PM.GetHashCode())
                {
                    //[MOU 03/09/2013 cf. 22318] En mode liste sur PP ou PM,
                    //champs utilisés dans la fenetre intermediaire pour le traitement de masse

                    //id et libellé de Adresse
                    HtmlInputHidden inptAdress = new HtmlInputHidden();
                    inptAdress.ID = String.Concat("hvAdr_", TableType.ADR.GetHashCode());
                    inptAdress.Value = eLibTools.GetPrefName(dal, Pref.Lang, TableType.ADR.GetHashCode());
                    _divHidden.Controls.Add(inptAdress);

                    //id et libellé de Contact
                    HtmlInputHidden inptPP = new HtmlInputHidden();
                    inptPP.ID = String.Concat("hvPp_", TableType.PP.GetHashCode());
                    inptPP.Value = eLibTools.GetPrefName(dal, Pref.Lang, TableType.PP.GetHashCode());
                    _divHidden.Controls.Add(inptPP);
                }


                //id et libellé de la table d'ou vient
                HtmlInputHidden inptFrom = new HtmlInputHidden();
                inptFrom.ID = String.Concat("hvFromTab_", VirtualMainTableDescId);
                inptFrom.Value = eLibTools.GetPrefName(dal, Pref.Lang, VirtualMainTableDescId);
                _divHidden.Controls.Add(inptFrom);
            }

            #endregion

            #region Historique - CSS
            if (_list.HistoInfo != null && _list.HistoInfo.Has)
            {
                //Histo descid
                HtmlInputHidden inptHistoDescId = new HtmlInputHidden();
                inptHistoDescId.ID = String.Concat("histoDescId_", VirtualMainTableDescId);
                inptHistoDescId.Value = _list.HistoInfo.Descid.ToString();
                _divHidden.Controls.Add(inptHistoDescId);

                // HISTO CSS
                if (!String.IsNullOrEmpty(_list.HistoInfo.BgColor) || !String.IsNullOrEmpty(_list.HistoInfo.Icon))
                {
                    //Histo CSS
                    //HtmlInputHidden inptHisto = new HtmlInputHidden();
                    //inptHisto.ID = String.Concat("HISTO_CSS_", VirtualMainTableDescId);
                    //inptHisto.Attributes.Add("etype", "css");
                    //inptHisto.Attributes.Add("ecssname", String.Concat("iconHisto", VirtualMainTableDescId));

                    //String histoCSS = "background :";

                    //if (!String.IsNullOrEmpty(_list.HistoInfo.BgColor)) // BG
                    //    histoCSS = String.Concat(histoCSS, _list.HistoInfo.BgColor, " ");

                    //if (!String.IsNullOrEmpty(_list.HistoInfo.Icon))  //ICON
                    //    histoCSS = string.Concat(histoCSS, "url(themes/",
                    //        Pref.ThemePaths.GetImageWebPath("/images/iFileIcon/" + _list.HistoInfo.Icon)
                    //        , ") center center no-repeat "); // TODO - gérer les thèmes

                    //histoCSS = String.Concat(histoCSS, "  !important ;");

                    //inptHisto.Attributes.Add("ecssclass", histoCSS);
                    //_divHidden.Controls.Add(inptHisto);
                    //inptHisto.Dispose();
                }
            }
            #endregion

            //CSS ICON STANDARD
            // MAB - Utilisation de l'extension .png pour toutes les icônes
            //String iconWebPath = String.Concat("themes/", Pref.ThemePaths.GetImageWebPath("/images/iFileIcon/" + _list.ViewMainTable.GetIcon.Replace(".jpg", ".png")));
            //HtmlInputHidden inptDefIconCss = new HtmlInputHidden();
            //inptDefIconCss.ID = "ICON_DEF_" + VirtualMainTableDescId;
            //inptDefIconCss.Attributes.Add("etype", "css");
            //inptDefIconCss.Attributes.Add("ecssname", String.Concat("iconDef_", VirtualMainTableDescId));
            //inptDefIconCss.Attributes.Add("ecssclass", String.Concat("background:url(", iconWebPath, ") center center no-repeat  !important "));
            //_divHidden.Controls.Add(inptDefIconCss);

            // Main field
            if (_list.MainField != null)
            {
                HtmlInputHidden inputMainFld = new HtmlInputHidden();
                inputMainFld.ID = String.Concat("mainFld_", VirtualMainTableDescId);
                inputMainFld.Value = _list.MainField.Descid.ToString();
                _divHidden.Controls.Add(inputMainFld);

                // Recherche sur le main search
                String _sMainSearchValue = _list.GetParam<String>("MainSearchValue");
                if (!String.IsNullOrEmpty(_sMainSearchValue))
                {
                    HtmlInputHidden inputSearchMainFld = new HtmlInputHidden();
                    inputSearchMainFld.ID = String.Concat("searchmainFld_", VirtualMainTableDescId);
                    inputSearchMainFld.Value = _sMainSearchValue;
                    _divHidden.Controls.Add(inputSearchMainFld);
                }
            }

            // MIN_COL_WIDTH
            HtmlInputHidden inputMinColWidth = new HtmlInputHidden();
            inputMinColWidth.ID = "minColWidth";
            inputMinColWidth.Value = eConst.MIN_COL_WIDTH.ToString();
            _divHidden.Controls.Add(inputMinColWidth);

            //ASY [BUG # 29409] - Taille max des colonnes
            HtmlInputHidden inputMaxColWidth = new HtmlInputHidden();
            inputMaxColWidth.ID = "maxColWidth";
            inputMaxColWidth.Value = ePrefConst.MAX_COL_WIDTH.ToString();
            _divHidden.Controls.Add(inputMaxColWidth);


            //Ajout du div caché
            _pgContainer.Controls.Add(_divHidden);

            RenameControls();
        }

        /// <summary>
        /// dans le cas de listes secondaires il peut être nécessaire de modifier les id pour ne pas avoir deux fois le meme dans la meme page.
        /// </summary>
        protected virtual void RenameControls()
        {

        }

        /// <summary>
        /// construit les en-tête de colonnes
        /// </summary>
        protected virtual void Head()
        {
            // Taille des colonnes non définie par utilisateur calculées automatiquement en fonction de la taille restante dans la fenêtre
            //Int32 nDefaultColWidth = 0;
            StringBuilder sb = new StringBuilder();

            // Taille de la table restant
            //Int32 tableRemainingWidth = _width - (Int32)_userColWidth;

            TableHeaderRow headerRow = new TableHeaderRow();
            headerRow.TableSection = TableRowSection.TableHeader;
            headerRow.CssClass = "hdBgCol";
            headerRow.VerticalAlign = VerticalAlign.Top;

            HeaderListIcon(headerRow);

            #region Colonnes personnalisées
            String libelleMaxLen = String.Empty;

            StringBuilder sbMailFields = new StringBuilder();

            //Liste des champ computable
            String sListComputedValue = String.Empty;

            Dictionary<Field, WebControl> drawFields = new Dictionary<Field, WebControl>();

            // Création du guide de déplacement des céllules.
            Panel cellGuide = new Panel();
            cellGuide.CssClass = "colGuide";
            _divHidden.Controls.Add(cellGuide);

            #region Colonnes personnalisées

            bool bSpecialField = false;
            for (int fieldIndex = 0; fieldIndex < _list.FldFieldsInfos.Count; fieldIndex++)
            {
                Field field = _list.FldFieldsInfos[fieldIndex];

                BeforeRenderFieldHeaderIcon(field);


                if (!field.DrawField || !field.PermViewAll || field.Format == FieldFormat.TYP_PASSWORD)
                    continue;

                Field fldRef = field.Format == FieldFormat.TYP_ALIASRELATION ? field.RelationSourceField : field;
                String colName = eTools.GetFieldValueCellName(_list.CalledTabDescId, fldRef.Alias);
                String shortColname = colName.Replace("COL_", "");

                libelleMaxLen = String.Concat(field.Libelle, "_");

                if (field.IsSortable)
                    libelleMaxLen = String.Concat(libelleMaxLen, @"/\_\/");

                if (field.IsFiltrable)
                {
                    if (field.IsSortable)
                        libelleMaxLen = String.Concat(libelleMaxLen, @"|Y|");
                    else
                        libelleMaxLen = String.Concat(libelleMaxLen, @"|Y|_");
                }

                if (field.IsComputable)
                {
                    if (sListComputedValue.Length > 0)
                        sListComputedValue = String.Concat(sListComputedValue, ";");

                    //sListComputedValue = String.Concat(sListComputedValue, colName);
                    sListComputedValue = String.Concat(sListComputedValue, field.Descid);

                    _existComputedCol = true;
                    libelleMaxLen = String.Concat(libelleMaxLen, @"SUM");
                }

                if (_sFirstColAlias.Length == 0)
                    _sFirstColAlias = colName;

                try
                {
                    _colMaxValues.Add(colName, new ListColMaxValues(libelleMaxLen));
                }
                catch (ArgumentException)
                {

                }

                // Taille du Champ
                InitFieldWidth(libelleMaxLen, field);

                TableCell cell = new TableCell();
                cell.CssClass = "hdName";
                cell.Attributes.Add("ondblclick", "rdc(event);");

                StringBuilder fullTitleHtml = new StringBuilder();
                fullTitleHtml.Append("<B>").Append(System.Web.HttpUtility.HtmlEncode(field.Libelle)).Append("</B>");
                if (!String.IsNullOrEmpty(field.ToolTipText) && !field.ToolTipText.Equals(field.Libelle))
                    fullTitleHtml.Append(" - ").Append(System.Web.HttpUtility.HtmlEncode(field.ToolTipText));

                String tilteMouseOver = String.Concat("st(event, '", fullTitleHtml.Replace("'", "\\'").Replace("[[BR]]", "<br />"), "');");
                String tilteMouseOut = "ht();";

                if ((field.IsSortable || field.IsFiltrable))
                {
                    HyperLink linkCol = new HyperLink();
                    linkCol.Text = System.Web.HttpUtility.HtmlEncode(field.Libelle);
                    //  linkCol.NavigateUrl = "javascript:;";       // Deplacer sur le onclick pour obtenir l'event sur IE
                    linkCol.Attributes.Add("onclick", "sl(event);");
                    linkCol.Attributes.Add("onmouseover", tilteMouseOver);
                    linkCol.Attributes.Add("onmouseout", tilteMouseOut);
                    // Utile ?
                    //linkCol.Attributes["onmousemove"] = "refreshTitle(event, document);";
                    cell.Controls.Add(linkCol);
                }
                else
                {
                    Label lblCol = new Label();
                    lblCol.Text = System.Web.HttpUtility.HtmlEncode(field.Libelle);
                    lblCol.Attributes.Add("onmouseover", tilteMouseOver);
                    lblCol.Attributes.Add("onmouseout", tilteMouseOut);
                    lblCol.Attributes.Add("oncontextmenu", "return false;");
                    // Utile ?
                    //linkCol.Attributes["onmousemove"] = "refreshTitle(event, document);";
                    cell.Controls.Add(lblCol);
                }

                cell.Controls.Add(new LiteralControl("&nbsp;"));

                #region icon Tri
                if (field.IsSortable)
                {
                    eIconCtrl imgSortAsc;
                    eIconCtrl imgSortDesc;

                    /* sph - 24/10/204 - retour sur les demande 27877/29360 - retour a l'ergonomie de tri v7 */
                    imgSortAsc = new eIconCtrl("SortAsc");
                    imgSortDesc = new eIconCtrl("SortDesc");

                    /*****************/
                    /* Icone de tri  */
                    /*****************/
                    imgSortAsc.ID = String.Concat("IMG_SORT_ASC_", shortColname);

                    SetSortIconActive(field, imgSortAsc, SortOrder.ASC);

                    // Fonction javascript : sortListAsc
                    imgSortAsc.Attributes["onclick"] = _jsSortAsc;


                    // Tri décroissant
                    //imgSortDesc = new eIconCtrl("SortDesc");
                    imgSortDesc.ID = String.Concat("IMG_SORT_DESC_", shortColname);

                    SetSortIconActive(field, imgSortDesc, SortOrder.DESC);

                    // Fonction javascript : sortListDesc
                    imgSortDesc.Attributes["onclick"] = _jsSortDesc;



                    cell.Controls.Add(imgSortAsc);
                    cell.Controls.Add(new LiteralControl("&nbsp;"));
                    cell.Controls.Add(imgSortDesc);




                }
                #endregion

                #region Icon Filtre
                if (field.IsFiltrable && _isTableFilterable)
                {
                    cell.Controls.Add(new LiteralControl("&nbsp;"));

                    // Filtre
                    eIconCtrl imgFilterCol = new eIconCtrl();
                    imgFilterCol.ID = String.Concat("IMG_FILTER_", shortColname);
                    imgFilterCol.Attributes.Add("ednTyp", field.Format.GetHashCode().ToString());
                    // Fonction javascript : doFilter
                    imgFilterCol.Attributes.Add("onclick", _jsDoFilter);

                    // La classe CSS change lorsque il existe un filtreexpress sur le field
                    SetFilterIconActive(field, imgFilterCol);

                    cell.Controls.Add(imgFilterCol);
                }
                #endregion

                #region computed colonne
                // Cellule de l'icone de la colonne calculé (Somme)
                if (field.IsComputable)
                {
                    cell.Controls.Add(new LiteralControl("&nbsp;"));

                    eIconCtrl imgComputed = new eIconCtrl("SumCols");
                    imgComputed.ID = String.Concat("IMG_SUM_COLS_", shortColname);


                    int nFilterId = 0;

                    //ALISTER => Demande / Request 91554 Je ne sais pas si il y a une meilleure solution pour récupérer l'information du filtre du widget 
                    //mais, si il en existe une autre, ça sera forcément avec l'id du widget et eXrmWidgetParam
                    //I wonder if a better solution exist to retrieve widget's filter information but if another one
                    //can exist, it should probably with widget's id and eXrmWidgetParam
                    eListWidget widgetList = _list as eListWidget;
                    if (widgetList != null)
                    {
                        if (widgetList.FilterId > 0)
                            nFilterId = widgetList.FilterId;
                    }

                    // A l'affichage, le calcul est lancé, et donc, l'icone est active
                    imgComputed.Attributes.Add("actif", "0");
                    imgComputed.Attributes["onclick"] = "docc(this, false," + nFilterId + ");";
                    imgComputed.Style.Add("visibility", "hidden");
                    cell.Controls.Add(imgComputed);
                }
                #endregion

                // Cellule de resize de la colonne
                TableCell cellResize = new TableCell();
                cellResize.CssClass = "hdResize";
                cellResize.ID = String.Concat("RESIZE_", shortColname);
                cellResize.RowSpan = 2;
                cellResize.Attributes.Add("ondblclick", "rdc(event);");
                cellResize.Attributes.Add("onmousedown", "rd(event);");
                cellResize.Text = "&nbsp;";

                TableRow row = new TableRow();
                row.Cells.Add(cell);
                row.Cells.Add(cellResize);

                // Ligne des valeurs des colonnes calculées
                TableRow rowComputedValue = null;
                rowComputedValue = new TableRow();
                rowComputedValue.CssClass = "sumColRow";
                rowComputedValue.Style.Add("display", "none");
                TableCell cellComputedValue = new TableCell();
                cellComputedValue.CssClass = "sumColRow";

                rowComputedValue.Cells.Add(cellComputedValue);
                //cellComputedValue.ColumnSpan = 2;
                if (field.IsComputable)
                {
                    cellComputedValue.ID = String.Concat("SUM_VAL_", shortColname);

                    cellComputedValue.Text = "&nbsp;";
                }
                else
                    cellComputedValue.Text = "&nbsp;";

                // IMPORTANT : LES INFORMATIONS DANS LES ATTRIBUTS DOIVENT ETRE REPORTE SUR EMAINFILERENDERER

                System.Web.UI.WebControls.Table cellTable = new System.Web.UI.WebControls.Table();
                cellTable.CssClass = "hdTable";
                cellTable.Attributes.Add("cellspacing", "0");
                cellTable.Attributes.Add("cellpadding", " 0");

                cellTable.Rows.Add(row);

                cellTable.Rows.Add(rowComputedValue);

                TableHeaderCell cellCol = new TableHeaderCell();
                cellCol.CssClass = "head";
                if (!field.IsMovable)
                {
                    cellCol.CssClass = String.Concat(cellCol.CssClass + " firstCol");
                    cellCol.Attributes.Add("nomove", "1");
                }

                if (!field.IsMovableAndResizableUpdate)
                    cellCol.Attributes.Add("nomoveupdt", "1");

                //ELAIZ  - régression 76444 - rajout d'un attribut filtre pour distinguer les colonnes filtrables ou triable des autres ( avatar, notes etc.)
                if (field.IsSortable || field.IsFiltrable)
                    cellCol.Attributes.Add("filter", "1");
                else
                    cellCol.Attributes.Add("filter", "0");

                //ASY [BUG # 29409] - Taille max des colonnes
                cellCol.Attributes.Add("width", String.Concat(field.Width.ToString(), "px"));
                cellCol.Width = new Unit(field.Width, UnitType.Pixel);

                cellCol.ID = colName;
                // Fonction javascript : colHeadOver
                cellCol.Attributes.Add("onmouseover", "chov(this);");


                // Fonction javascript : colHeadOut
                cellCol.Attributes.Add("onmouseout", "chou(this);");
                // Fonction javascript : moveColDown
                if (field.IsMovable)
                    cellCol.Attributes.Add("onmousedown", String.Concat("mcd(event);"));
                cellCol.Controls.Add(cellTable);

                if (field.Format == FieldFormat.TYP_ALIASRELATION)
                    cellCol.Attributes.Add("did", field.RelationSource.ToString());
                else
                    cellCol.Attributes.Add("did", field.Descid.ToString());

                //cellCol.Attributes.Add("tab", field.Table.TabName.ToString());
                //cellCol.Attributes.Add("fld", field.Name.ToString());
                cellCol.Attributes.Add("tab", "tab");
                cellCol.Attributes.Add("fld", "fld");
                //cellCol.Attributes.Add("tabTyp", field.Table.EdnType.GetHashCode().ToString());


                cellCol.Attributes.Add("dat", field.Table.EdnType.GetHashCode().ToString());

                // Le titre est ajouté sur tous les types de champs, y compris les non-catalogues, pour, par ex., affecter un titre aux fenêtres popup les concernant (ex : champ Mémo en mode Zoom)
                cellCol.Attributes.Add("lib", (field.Libelle.ToString()));

                // HLA - Informations necessaires pour la gestion du editorCatalog
                // MAB - US #1566 - Tâche #3265 - Demande #75 895 - Minifiche sur les champs Alias
                Field refFld = field;
                if (field.AliasSourceField?.Popup == PopupType.SPECIAL)
                    refFld = field.AliasSourceField;
                if (refFld.Popup != PopupType.NONE || refFld.Format == FieldFormat.TYP_USER || refFld.Format == FieldFormat.TYP_FILE)
                {
                    //BSE:#60 251 // ne pas appliquer les stats dans les filtres avancés
                    //BSE: pas de stats dans les modèles d'import 
                    bSpecialField = refFld.Popup == PopupType.SPECIAL
                        || refFld.Descid == (int)ImportTemplateField.USERID
                        || refFld.Descid == (int)FilterField.USERID
                        || refFld.Descid == (int)ReportField.USERID;
                    cellCol.Attributes.Add("pop", refFld.Popup.GetHashCode().ToString());
                    cellCol.Attributes.Add("popId", refFld.PopupDescId.ToString());
                    cellCol.Attributes.Add("special", bSpecialField ? "1" : "0");
                    cellCol.Attributes.Add("bndId", refFld.BoundDescid.ToString());
                    cellCol.Attributes.Add("bndPop", refFld.BoundField == null ? String.Empty : ((int)refFld.BoundField.Popup).ToString());
                    cellCol.Attributes.Add("mult", refFld.Multiple ? "1" : "0");
                    cellCol.Attributes.Add("tree", refFld.PopupDataRend == PopupDataRender.TREE ? "1" : "0");
                    cellCol.Attributes.Add("treeolc", refFld.IsTreeViewOnlyLastChildren ? "1" : "0");
                    cellCol.Attributes.Add("treeusr", refFld.IsTreeViewUserList ? "1" : "0");
                    if (refFld.ExpressFilterActived.Length > 0)
                        cellCol.Attributes.Add("aef", refFld.ExpressFilterActived);

                    if (refFld.Popup == PopupType.ENUM)
                    {
                        cellCol.Attributes.Add("eaction", "LNKCATENUM");

                        cellCol.Attributes.Add("data-enumt", ((int)eCatalogEnum.GetCatalogEnumTypeFromField(refFld)).ToString());



                    }

                    if (refFld.Popup == PopupType.DESC)
                    {



                        if (
                            refFld.Descid == (int)RGPDTreatmentsLogsField.TabsID
                        || refFld.Descid == (int)WorkflowScenarioField.WFTEVENTDESCID
                        || refFld.Descid == (int)WorkflowScenarioField.WFTTARGETDESCID
                        )
                        {
                            cellCol.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Table).ToString());
                        }
                        else if (refFld.Descid == (int)RGPDTreatmentsLogsField.FieldsID)
                        {
                            cellCol.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Field).ToString());
                        }

                    }
                }

                //POUR PRISE EN COMPTE DES SPECIFICITE DE MAJ DES CHAMPS DE LIAISONS EN MODE LISTE
                if ((field.Descid % 100 == 1 && field.Table != _list.ViewMainTable)
                    || field.Format == FieldFormat.TYP_ALIASRELATION)
                {
                    cellCol.Attributes.Add("prt", "1");
                }

                //Champ de liaison : précise le descid du champ de liaison 
                if (field.Table.FieldLink != 0 && field.Table.EdnType == EdnType.FILE_MAIN && field.Descid.ToString().EndsWith("01"))
                {
                    cellCol.Attributes.Add("fldlnk", field.Table.FieldLink.ToString());
                }

                headerRow.Controls.Add(cellCol);
                drawFields.Add(field, cellCol);

                //KHA le 06/02/2013 : pour les champs de type email, on rajoute une classe customisee qui va permettre d'agir sur le champ mail
                //par ex pour retailler la div lors du resize de la colonne puisque le champ mail intègr une icone permettant d'envoyer un email directement.
                // #59 789 - idem pour les fichiers de type SMS. TOCHECK: se baser sur la présence du webComplementControl avec le pictogramme de eRenderer ? 
                if (field.Format == FieldFormat.TYP_EMAIL || (field.Format == FieldFormat.TYP_PHONE && Pref.SmsEnabled))
                {
                    HtmlInputHidden inptEmailCss = new HtmlInputHidden();
                    inptEmailCss.ID = String.Concat("divct_", shortColname);
                    inptEmailCss.Attributes.Add("etype", "css");
                    inptEmailCss.Attributes.Add("ecssname", String.Concat("divct_", shortColname));
                    inptEmailCss.Attributes.Add("ecssclass", "width:80%;");
                    _divHidden.Controls.Add(inptEmailCss);
                    if (sbMailFields.Length > 0)
                        sbMailFields.Append(";");

                    sbMailFields.Append(colName);
                }
                //GCH - #35859 - Internationnalisation - on permet l'identification des champs au format date pour les convertir au format de la Base de données
                //GCH - #36869 - Internationalisation - Type numérique
                else if (field.Format == FieldFormat.TYP_DATE
                     || field.Format == FieldFormat.TYP_AUTOINC
                     || field.Format == FieldFormat.TYP_COUNT
                     || field.Format == FieldFormat.TYP_ID
                     || field.Format == FieldFormat.TYP_NUMERIC
                     || field.Format == FieldFormat.TYP_MONEY)
                    cellCol.Attributes.Add("frm", field.Format.GetHashCode().ToString());
            }

            #endregion

            //Liste des champs calculés
            HtmlInputHidden inputComptedFields = new HtmlInputHidden();
            _divHidden.Controls.Add(inputComptedFields);
            inputComptedFields.Value = sListComputedValue;
            inputComptedFields.ID = String.Concat("CMPFLD_", _tab);

            //Liste des champs de type mails
            HtmlInputHidden inputMailFields = new HtmlInputHidden();
            _divHidden.Controls.Add(inputMailFields);
            inputMailFields.Value = sbMailFields.ToString();
            inputMailFields.ID = String.Concat("MLFld_", _tab);

            #endregion



            headerRow.ID = String.Concat("HEAD_", _list.ViewMainTable.Alias);

            _tblMainList.Rows.Add(headerRow);
        }

        protected virtual void SetSortIconActive(Field field, eIconCtrl imgSort, SortOrder order)
        {
            if (field.SortInfo != order)
                imgSort.Style.Add("visibility", "hidden");

            imgSort.Attributes.Add("actif", (field.SortInfo == order) ? "1" : "0");
        }

        /// <summary>
        /// Ajoute les attributes/classes sur l'icône "filtre" permettant de déterminer si le filtre est actif ou non
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="imgFilterCol">The img filter col.</param>
        protected virtual void SetFilterIconActive(Field field, eIconCtrl imgFilterCol)
        {

            if (String.IsNullOrEmpty(field.ExpressFilterActived))
            {
                imgFilterCol.AddClass("Filter");
                imgFilterCol.Style.Add("visibility", "hidden");
                imgFilterCol.Attributes.Add("actif", "0");
            }
            else
            {
                imgFilterCol.AddClass("FilterEnabled");
                imgFilterCol.Attributes.Add("actif", "1");
            }
        }

        protected virtual void BeforeRenderFieldHeaderIcon(Field field)
        {
            // Pas de traitement a faire ici
        }

        /// <summary>
        /// Définit la taille par défaut d'une colonne
        /// </summary>
        /// <param name="libelleMaxLen">taillé max du libellé (avec filtre et tri)</param>
        /// <param name="field">champ à redimensionner</param>
        protected virtual void InitFieldWidth(String libelleMaxLen, Field field)
        {
            if (field.Width <= 0)
            {
                System.Drawing.Font font = new System.Drawing.Font("Verdana", 8, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
                field.Width = eTools.MesureString(libelleMaxLen, font);

                if (field.Width < eConst.MIN_COL_WIDTH)
                    field.Width = eConst.MIN_COL_WIDTH;
            }
        }

        /// <summary>
        /// Construction du contennu de la table de la liste
        /// </summary>
        protected virtual void Body()
        {
            Int32 idxLine = 1;

            _tblMainList.Attributes.Add("cPage", _page.ToString());
            _tblMainList.Attributes.Add("top", ((_page - 1) * _rows).ToString());

            // Information de début de liste pour l'affichage des boutons précédents.
            _tblMainList.Attributes.Add("bof", _page <= 1 ? "1" : "0");

            if (this._rType == RENDERERTYPE.FilterReportList || this._rType == RENDERERTYPE.UserMailTemplateList || this._rType == RENDERERTYPE.AutomationList || this._rType == RENDERERTYPE.ImportTemplate)
                _tblMainList.Attributes.Add("cnton", "0");

            String sLstRulesCss = String.Empty;
            TableRow trDataRow = null;

            List<String> lIcon = new List<String>();

            #region Construction du tableau

            if (this._rType == RENDERERTYPE.ListRendererMain)
                Pref.Context.Paging.LstIdPage.Clear();



            while (idxLine <= _rows && idxLine <= _list.ListRecords.Count)
            {
                eRecord row = _list.ListRecords[idxLine - 1];

                // Vérifie si on a les droits de traitement pour la ligne 
                if (!HasRight(row))
                {
                    idxLine++;
                    continue;
                }

                if (row is eEmptyRecord)
                {
                    Int32 nbCell = _tblMainList.Rows[0].Cells.Count;
                    //SHA : correction bug #70 000
                    //trDataRow = EmptyLine(nbCell, "Les données de cette fiche sont masquées par une condition de visualisation.");
                    trDataRow = EmptyLine(nbCell, eResApp.GetRes(Pref, 2221));

                    cssLine(trDataRow, idxLine);

                    idxLine++;
                    continue;
                }

                trDataRow = new TableRow();
                _tblMainList.Rows.Add(trDataRow);

                trDataRow.TableSection = TableRowSection.TableBody;

                // Css alterné par row (ligne)
                cssLine(trDataRow, idxLine);

                if (this._rType == RENDERERTYPE.ListRendererMain)
                    if (!Pref.Context.Paging.LstIdPage.Contains(row.MainFileid))
                        Pref.Context.Paging.LstIdPage.Add(row.MainFileid);

                //TODO : 
                if (_rType == RENDERERTYPE.ListInvit)
                {
                    trDataRow.Attributes.Add("eSelAdrid", row.AdrId.ToString());
                    trDataRow.Attributes.Add("eSelPPid", row.PpId.ToString());
                }

                //Colonnes fixes
                BodyListIcon(row, trDataRow, idxLine, ref sLstRulesCss, lIcon);

                //Specifités sur la row en fonction du rendu
                CustomTableRow(row, trDataRow, idxLine);

                #region Colonnes personnalisées

                // Valeurs à afficher 
                TableCell cell = null;

                int idxCell = 0;
                foreach (eFieldRecord fieldRow in row.GetFields)
                {
                    idxCell++;

                    //ne pas afficher les champs interdits de visu
                    if (!fieldRow.FldInfo.DrawField || !fieldRow.FldInfo.PermViewAll)
                        continue;

                    cell = (TableCell)GetFieldValueCell(row, fieldRow, idxLine, Pref, _colMaxValues);

                    //Couleur de la ligne si la date est inferieur à la date en cours
                    if (_list.ViewMainTable.EdnType == EdnType.FILE_PLANNING && fieldRow.FldInfo.Descid == _list.DateDescId)
                    {
                        DateTime dTask = DateTime.MinValue;
                        DateTime.TryParse(fieldRow.Value, out dTask);
                        string rowColor = eTools.GetDateColor(dTask);
                        trDataRow.Style.Add(HtmlTextWriterStyle.Color, eTools.GetDateColor(dTask));
                        //BSE:#54 655
                        if (rowColor == eConst.COL_DATE_PAST)
                            cell.Style.Remove(HtmlTextWriterStyle.Color);
                    }


                    cell.CssClass = String.Concat(cell.CssClass, " cell");

                    // TODO CRU: Couleur valeur
                    if (!String.IsNullOrEmpty(fieldRow.FldInfo.StyleForeColor))
                        cell.Style.Add("color", fieldRow.FldInfo.StyleForeColor);


                    trDataRow.Cells.Add(cell);

                    //Specifités sur la row en fonction du rendu
                    CustomTableCell(row, trDataRow, fieldRow, cell, idxCell);
                }

                AddCustomCells(row, trDataRow);

                trDataRow.Attributes.Add("eid", String.Concat(VirtualMainTableDescId, "_", row.MainFileid.ToString()));



                #endregion

                idxLine++;

            }


            #endregion



            // Complète les lignes
            CompleteList(idxLine);

            // Information de fin de liste pour l'affichage des boutons suivants.


            Int32 a = _list.RowsByPage;
            Boolean eof;
            // "eof" obsolète ??
            if (idxLine == 1 || idxLine < _rows)       // Aucune ligne ou toutes les lignes du datatablereader ont été affiché
                eof = true;
            else
                eof = (idxLine - 1) >= _list.ListRecords.Count;

            _tblMainList.Attributes.Add("eof", eof ? "1" : "0");


            #region Traitement post-génération

            // Masque les colonnes de pj/shedule si besoin
            if ((_list.PjIconActive && !lIcon.Contains("pj")) || (_list.ScheduleIconActive && !lIcon.Contains("schedule")))
            {
                foreach (TableRow tr in _tblMainList.Rows)
                {



                    if (tr.ID != null && tr.ID.Equals("empty"))
                        continue;

                    if (_idxCellPjCount != -1 && !lIcon.Contains("pj"))
                    {
                        tr.Cells[_idxCellPjCount].Style.Add("visibility", "hidden");
                        tr.Cells[_idxCellPjCount].Style.Add("display", "none");
                    }

                    if (_idxCellSched != -1 && !lIcon.Contains("schedule"))
                    {
                        tr.Cells[_idxCellSched].Style.Add("visibility", "hidden");
                        tr.Cells[_idxCellSched].Style.Add("display", "none");
                    }

                    if (_idxCellMerge != -1 && !lIcon.Contains("merge"))
                    {
                        tr.Cells[_idxCellMerge].Style.Add("visibility", "hidden");
                        tr.Cells[_idxCellMerge].Style.Add("display", "none");
                    }
                }


            }

            #endregion

        }

        /// <summary>
        /// Ajout de colonnes personnalisées en fin de ligne
        /// </summary>
        /// <param name="record">Objet eRecord</param>
        /// <param name="tr">Objet TableRow</param>
        protected virtual void AddCustomCells(eRecord record, TableRow tr)
        {
            return;
        }

        /// <summary>
        /// Par defaut on a les droits
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected virtual Boolean HasRight(eRecord row)
        {
            return true;
        }

        #endregion

        #region Compléments

        /// <summary>
        /// Classe ajoutée lors de la gestion d'une ligne sur 2
        /// </summary>
        /// <param name="trDataRow">Ligneà laquel on affecte le style</param>
        /// <param name="idxLine">Numéro de ligne</param>
        protected virtual void cssLine(TableRow trDataRow, Int32 idxLine)
        {
            trDataRow.CssClass = (idxLine % 2 == 0) ? "line2" : "line1";
        }

        /// <summary>
        /// présente les icones pour les annexes, taches periodiques...
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="idxLine"></param>
        /// <param name="sLstRulesCss"></param>
        /// <param name="lIcon">Liste enregistrant les types d'icone ajouté à la liste</param>
        protected virtual void BodyListIcon(eRecord row, TableRow trDataRow, Int32 idxLine, ref String sLstRulesCss, List<string> lIcon)
        {
            String sAltLineCss = "cell";

            // Traitment sur la ligne (couleur conditionnelle, historique....)
            #region Colonnes fixes

            #region Couleur conditionnelle
            // TODO : gérer le tooltip
            //Création de la css Couleur Conditionnelle
            //if (row.RuleColor.HasRuleColor)
            //{

            //    //Construction de la liste

            //    String sRulesId = String.Concat(row.RuleColor.Idendity, "_", ((idxLine % 2) == 0 ? "2" : "1"));

            //    //background Info
            //    if (!sLstRulesCss.Contains(String.Concat(";", sRulesId, ";")) && (!String.IsNullOrEmpty(row.RuleColor.BgColor) || !string.IsNullOrEmpty(row.RuleColor.Icon)))
            //    {
            //        AddPochoirInputCss(row, idxLine, ref  sLstRulesCss, sRulesId);
            //    }

            //}

            #endregion

            //  Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            // KHA demande n° 19160 on ne met pas les cases à cocher de selection dans le cas des signet
            if (DisplayCheckBox())
            {
                AddSelectCheckBox(row, trDataRow, sAltLineCss);
            }

            //Icone
            AddIconBody(row, trDataRow, sAltLineCss, idxLine);

            // Annexes
            if (_list.PjIconActive)
            {
                TableCell pjIcon = new TableCell();
                pjIcon.CssClass = String.Concat(sAltLineCss, " icon iPj ");
                if (row.IsPJViewable && row.PjCnt != 0)
                {
                    if (!lIcon.Contains("pj")) { lIcon.Add("pj"); }

                    //Image _imgPj = new Image();
                    //_imgPj.ToolTip = String.Concat(row.PjCnt, " ", _list.ViewMainTable.PjLibelle);
                    //_imgPj.ImageUrl = eConst.GHOST_IMG;
                    //pjIcon.Controls.Add(_imgPj);

                    HtmlGenericControl spanPjIcon = new HtmlGenericControl("span");
                    pjIcon.Controls.Add(spanPjIcon);
                    spanPjIcon.Attributes.Add("Title", String.Concat(row.PjCnt, " ", _list.ViewMainTable.PjLibelle));
                    spanPjIcon.Attributes.Add("class", "icon-annex");
                    pjIcon.Attributes.Add("eaction", "LNKOPENPJDIALOG");
                    pjIcon.Attributes.Add("ref", JsonConvert.SerializeObject(new { t = _list.ViewMainTable.DescId, i = row.MainFileid }));
                    pjIcon.Attributes.Add("efld", "1");

                }

                _idxCellPjCount = trDataRow.Cells.Count;
                trDataRow.Cells.Add(pjIcon);
            }

            // Schedule
            if (_list.ScheduleIconActive)
            {
                TableCell schIcon = new TableCell();
                schIcon.CssClass = String.Concat(sAltLineCss, " icon");
                eRecordWithSchedule rowScheduled = null;
                if (_list.ViewMainTable.EdnType == EdnType.FILE_PLANNING)
                {
                    rowScheduled = (eRecordPlanningCalendar)row;
                }
                else if (_list.ViewMainTable.IsEventStep && row is eRecordEventStep)
                {
                    rowScheduled = (eRecordEventStep)row;
                }

                if (rowScheduled != null && rowScheduled.ScheduleId > 0)
                {
                    if (!lIcon.Contains("schedule")) { lIcon.Add("schedule"); }
                    // CRU 30/05/2018 : Remplacement de l'image par une icône font
                    HtmlGenericControl iconSch = new HtmlGenericControl();
                    iconSch.Attributes.Add("class", "logo-recur icon-repeat");
                    iconSch.Attributes.Add("title", rowScheduled.ScheduleLbl);
                    schIcon.Controls.Add(iconSch);
                }

                _idxCellSched = trDataRow.Cells.Count;
                trDataRow.Cells.Add(schIcon);
            }
            // fusion de doublon
            if (_list.TypeQuery == ViewQuery.LIST_BKM_DBL)
            {
                TableCell tcMrgIcon = new TableCell();
                tcMrgIcon.CssClass = String.Concat(sAltLineCss, " icon icon-edn-cross");
                tcMrgIcon.Attributes.Add("eaction", "LNKOPENMRG");
                tcMrgIcon.Attributes.Add("efld", "1");
                tcMrgIcon.ToolTip = eResApp.GetRes(Pref, 812);

                if (!lIcon.Contains("merge"))
                {
                    lIcon.Add("merge");
                }
                //Image imgSch = new Image();
                //imgSch.ToolTip = eResApp.GetRes(Pref, 812);
                //imgSch.ImageUrl = eConst.GHOST_IMG;
                //imgSch.CssClass = "logo-mrg";
                //tcMrgIcon.Controls.Add(imgSch);

                _idxCellMerge = trDataRow.Cells.Count;
                trDataRow.Cells.Add(tcMrgIcon);
            }

            #endregion
        }


        /// <summary>
        /// Ajoute les specifités sur la row en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="idxLine">index de la ligne</param>
        protected virtual void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {

        }


        private String _sConfigVCard = "X";
        private Dictionary<int, bool> _dicoConfigMiniFile = new Dictionary<int, bool>();

        /// <summary>
        /// Ajoute les specifités sur la cell en fonction du rendu
        /// Ajoute les specifités sur la cell en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="fieldRow">field record</param>
        /// <param name="trCell">Objet cellule courant</param>
        /// <param name="idxCell">index de la colonne</param>
        protected virtual void CustomTableCell(eRecord row, TableRow trRow, eFieldRecord fieldRow, TableCell trCell, Int32 idxCell)
        {
            //Affichage de la VCARD au survol de PP01 si champ de liaison
            // il y avait un appel a getconfigdefault a chaque fois qu'on affiche le descid 201 et donc un appel en bdd.
            // cette config ne changeant pas d'une ligne à l'autre, il faudrait mettre en cache cette valeur

            // MAB - US #1586 - Tâche #3265 - Demande #75 895 - Minifiche sur les champs Alias
            // #84 255 - Si le champ Alias source (AliasSourceField) est défini, mais qu'aucun enregistrement (AliasSourceFieldRecord) n'y est renseigné,
            // on utilise fieldRow. Dans ce cas de figure, on est alors face au même comportement qu'en l'absence d'alias :
            // - soit fieldRow.(Popup)DescId = 201, auquel cas on affiche la VCard du champ principal de "Contacts" (201)
            // - soit le DescId est différent, auquel cas, les traitements ci-dessous ne seront pas exécutés
            eFieldRecord referenceFieldRow = fieldRow.AliasSourceFieldRecord ?? fieldRow;

            if (referenceFieldRow.FldInfo.PopupDescId == 201 || referenceFieldRow.FldInfo.Descid == 201)
            {
                if (_sConfigVCard == "X")
                    _sConfigVCard = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] { eLibConst.CONFIG_DEFAULT.VCARDMAPPING })[eLibConst.CONFIG_DEFAULT.VCARDMAPPING];

                if (_sConfigVCard.Length > 1)
                    VCMouseActionAttributes(trCell);
            }
            //CNA - Affichage de la MiniFiche au survol de xx01 si champ de liaison
            else if (referenceFieldRow.FldInfo.PopupDescId % 100 == 1 || referenceFieldRow.FldInfo.Descid % 100 == 1)
            {
                int tab = (referenceFieldRow.FldInfo.PopupDescId % 100 == 1 ? referenceFieldRow.FldInfo.PopupDescId : referenceFieldRow.FldInfo.Descid) - 1;

                if (VCMiniFileCheckMappingEnabled(tab))
                    VCMiniFileMouseActionAttributes(trCell, tab);
            }
        }

        /// <summary>
        /// Peuple un panel avec les boutons pour le paging dans la liste des filtres/report
        /// </summary>
        /// <returns></returns>
        public virtual void CreatePagingBar(HtmlGenericControl pnTitle)
        {
            // Panel pnTitle = new Panel();


            int pageDprt = _page;

            /* Comme on ne peut pas connaitre le nombre d'enregistrement, les page first& last sont désactivées */
            HtmlGenericControl divFirst = new HtmlGenericControl("div");
            divFirst.Attributes.Add("style", "display:none");
            divFirst.ID = "idFirst";

            HtmlGenericControl divLast = new HtmlGenericControl("div");
            divLast.Attributes.Add("style", "display:none");
            divLast.ID = "idLast";

            HtmlGenericControl divPrev = new HtmlGenericControl("div");
            divPrev.Attributes.Add("Class", "icon-edn-prev fLeft disable icnListAct");
            divPrev.ID = "idPrev";
            divPrev.Attributes.Add("onclick", String.Concat("prevpage(", _list.CalledTabDescId.ToString(), ");"));

            if (_page > 1)
            {

                divPrev.Attributes.Add("class", "icon-edn-prev fLeft icnListAct");
            }
            else
            {
                divPrev.Attributes.Add("class", "icon-edn-prev fLeft disable icnListAct");
            }



            HtmlGenericControl divPage = new HtmlGenericControl("div");
            divPage.Attributes.Add("class", "NbPagePlsPls");
            divPage.ID = "divNumpage";


            Literal liNbPage = new Literal();
            liNbPage.Text = _list.Page.ToString();
            divPage.Controls.Add(liNbPage);


            HtmlGenericControl divNext = new HtmlGenericControl("div");
            divNext.ID = "idNext";
            divNext.Attributes.Add("onclick", String.Concat("nextpage(", _list.CalledTabDescId.ToString(), ");"));
            if (_list.ListRecords.Count > _list.RowsByPage)
            {
                divNext.Attributes.Add("class", "icon-edn-next fRight icnListAct");
            }
            else
            {
                divNext.Attributes.Add("class", "icon-edn-next fRight icnListAct disable");
            }


            // comme on est en float:right il faut ajouter les élément de droite à gauche
            pnTitle.Controls.Add(divLast);
            pnTitle.Controls.Add(divNext);
            pnTitle.Controls.Add(divPage);
            pnTitle.Controls.Add(divPrev);
            pnTitle.Controls.Add(divFirst);


        }

        /// <summary>
        /// identifie les paramètres de pagination
        /// </summary>
        protected virtual void SetPagingInfo()
        {
            //Information de paging
            _tblMainList.Attributes.Add("eNbCnt", eNumber.FormatNumber(Pref, Pref.Context.Paging.NbResult, 0, true));
            _tblMainList.Attributes.Add("eNbTotal", eNumber.FormatNumber(Pref, Pref.Context.Paging.NbTotalResult, 0, true));
            _tblMainList.Attributes.Add("eHasCount", Pref.Context.Paging.HasCount ? "1" : "0");
            _tblMainList.Attributes.Add("nbPage", Pref.Context.Paging.NbPage.ToString());

            //Compteur appararent
            //demande #84 555
            if (_list.GetParam<Boolean>("CountOnDemand"))
                _tblMainList.Attributes.Add("cnton", "0");
            else if (_list.GetParam<Boolean>("PagingEnabled") || !_list.GetParam<Boolean>("CountOnDemand"))
                _tblMainList.Attributes.Add("cnton", "1");
        }


        private TableRow EmptyLine(int nbCell, string sContent = "&nbsp;")
        {
            TableRow trDataRow = new TableRow();
            trDataRow.ID = "empty";
            trDataRow.TableSection = TableRowSection.TableBody;

            TableCell cell = new TableCell();
            cell.ColumnSpan = nbCell;
            cell.CssClass = "cellempty";
            cell.Text = sContent;
            trDataRow.Cells.Add(cell);

            _tblMainList.Rows.Add(trDataRow);

            return trDataRow;

        }

        /// <summary>
        /// Complète la liste principale en rajoutant autant de ligne que nécessaire 
        /// afin que les boutons d'action du bas de page reste toujours à la même position
        /// </summary>
        /// <param name="idxLine"></param>
        protected virtual void CompleteList(Int32 idxLine)
        {
            // Complète les lignes (avec le nombre de lignes demandées)
            if (idxLine < _rowsCalculated)
            {
                Int32 nbCell = _tblMainList.Rows[0].Cells.Count;

                while (idxLine <= _rowsCalculated)
                {
                    EmptyLine(nbCell, "");
                    idxLine++;
                }
            }

        }

        /// <summary>
        /// Affiche-t-on la case à cocher en début de ligne ?
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean DisplayCheckBox()
        {
            return _rType == RENDERERTYPE.ListRendererMain
                    || _rType == RENDERERTYPE.ListInvit
                    || _rType == RENDERERTYPE.ListSelection
                    || _rType == RENDERERTYPE.ListPjFromTpl
                    || _rType == RENDERERTYPE.Finder
                    || _rType == RENDERERTYPE.FinderSelection
                ;
        }


        /// <summary>
        /// rajoute dans la ligne d'en-tête la colonne contenant les icones pour les annexes, taches periodiques...
        /// </summary>
        /// <param name="headerRow"></param>
        /// <returns>la taille prise par les icones</returns>
        protected virtual void HeaderListIcon(TableRow headerRow)
        {
            if (DisplayCheckBox())
            {
                // KHA demande n° 19160 on ne met pas les cases à cocher de selection dans le cas des signets
                AddSelectCheckBoxHead(headerRow);
            }

            // Icon
            AddIconHead(headerRow);

            // Icon Annexes
            if (_list.PjIconActive)
            {
                TableHeaderCell pjIcon = new TableHeaderCell();
                pjIcon.CssClass = "head icon";
                pjIcon.Attributes.Add("nomove", "1");
                pjIcon.Attributes.Add("width", String.Concat(_sizeTdIcon, "px"));
                headerRow.Cells.Add(pjIcon);
            }

            // Icon schedule
            if (_list.ScheduleIconActive)
            {
                TableHeaderCell schIcon = new TableHeaderCell();
                schIcon.CssClass = "head icon";
                schIcon.Attributes.Add("nomove", "1");
                schIcon.Attributes.Add("width", String.Concat(_sizeTdIcon, "px"));
                headerRow.Cells.Add(schIcon);
            }

            // fusion de doublon
            if (_list.TypeQuery == ViewQuery.LIST_BKM_DBL)
            {
                TableHeaderCell mgrIcon = new TableHeaderCell();
                mgrIcon.CssClass = "head icon";
                mgrIcon.Attributes.Add("nomove", "1");
                mgrIcon.Attributes.Add("width", String.Concat(_sizeTdIcon, "px"));
                headerRow.Cells.Add(mgrIcon);

            }
        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected virtual void AddSelectCheckBoxHead(TableRow headerRow)
        {
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
            chkSelectAll.AddClick("selectAllList(this);");
            chkSelectAll.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cellSelect.Controls.Add(chkSelectAll);

            headerRow.Cells.Add(cellSelect);
        }

        /// <summary>
        /// Ajoute l'entête pour l'icone d'ouverture de la fiche
        /// </summary>
        /// <param name="headerRowicon">Ligne à modifier</param>
        protected virtual void AddIconHead(TableRow headerRowicon)
        {
            // Icon
            TableHeaderCell selIcon = new TableHeaderCell();
            selIcon.CssClass = "head icon";
            selIcon.ID = String.Concat("HEAD_ICON_COL_", this._tab);
            selIcon.Attributes.Add("nomove", "1");
            selIcon.Attributes.Add("width", String.Concat(_sizeTdIcon, "px"));
            headerRowicon.Cells.Add(selIcon);

            // Icône additionnelle pour PJ
            if (_list.ViewMainTable.EdnType == EdnType.FILE_PJ)
            {
                TableHeaderCell selIcon2 = new TableHeaderCell();
                selIcon2.CssClass = selIcon.CssClass;
                selIcon2.ID = String.Concat("HEAD_ICON_COL2_", this._tab);
                selIcon2.Attributes.Add("nomove", "1");
                selIcon2.Attributes.Add("width", String.Concat(_sizeTdIcon, "px"));
                headerRowicon.Cells.Add(selIcon2);
            }
        }

        /// <summary>
        /// Ajoute les Icones d'ouverture en début de ligne
        /// </summary>
        /// <param name="rowIcon">Enregistrement</param>
        /// <param name="bodyRowicon">Ligne à modifier</param>
        /// <param name="cssIcon">Classe CSS d'origine</param>
        /// <param name="idxLine">index de la ligne</param>
        protected virtual void AddIconBody(eRecord rowIcon, TableRow bodyRowicon, String cssIcon, Int32 idxLine)
        {
            // Icon IMG en TD
            TableCell selIcon = new TableCell();
            TableCell selIcon2 = new TableCell();

            // Attributs
            selIcon.Attributes.Add("ename", String.Concat("HEAD_ICON_COL_", this._tab.ToString()));
            selIcon.Attributes.Add("lnkid", rowIcon.MainFileid.ToString());
            selIcon.Attributes.Add("efld", "1");
            selIcon2.Attributes.Add("ename", String.Concat("HEAD_ICON_COL_", this._tab.ToString()));
            selIcon2.Attributes.Add("lnkid", rowIcon.MainFileid.ToString());
            selIcon2.Attributes.Add("efld", "1");

            // Type d'icone
            selIcon.CssClass = String.Concat(cssIcon, " icon lnkIcon");
            selIcon.ToolTip = _list.ViewMainTable.Libelle;

            String iconClass = String.Empty;
            HtmlGenericControl spanIcon = new HtmlGenericControl();

            // Priorité : Historique -> Conditionnel -> Standard
            if (rowIcon.IsHisto && !String.IsNullOrEmpty(_list.HistoInfo.Icon))
            {
                //selIcon.CssClass = String.Concat(selIcon.CssClass, " iconHisto", VirtualMainTableDescId);
                selIcon.ToolTip = eResApp.GetRes(Pref, 5086);
                selIcon.Style.Add("background-color", _list.HistoInfo.BgColor);
                iconClass = eFontIcons.GetFontClassName(_list.HistoInfo.Icon);
                spanIcon.Style.Add("color", _list.HistoInfo.Color);
            }
            else if (rowIcon.RuleColor.HasRuleColor && !String.IsNullOrEmpty(rowIcon.RuleColor.Icon))
            {
                //selIcon.CssClass = String.Concat(selIcon.CssClass, " ", rowIcon.RuleColor.Idendity, "_", (idxLine % 2) == 0 ? "2" : "1");
                selIcon.ToolTip = rowIcon.RuleColor.Label;
                selIcon.Style.Add("background-color", rowIcon.RuleColor.BgColor);
                iconClass = eFontIcons.GetFontClassName(rowIcon.RuleColor.Icon);
                spanIcon.Style.Add("color", rowIcon.RuleColor.Color);
            }
            else
            {
                //selIcon.CssClass = String.Concat(selIcon.CssClass, "  iconDef_", VirtualMainTableDescId);
                iconClass = eFontIcons.GetFontClassName(_list.ViewMainTable.GetIcon);
                // HLA - Ne pas mettre de couleur par defaut, mais laisser le theme prendre le relais sur la couleur des icones de tables
                if (!String.IsNullOrEmpty(_list.ViewMainTable.GetIconColor))
                    spanIcon.Style.Add("color", _list.ViewMainTable.GetIconColor);
            }

            spanIcon.Attributes.Add("class", iconClass);
            selIcon.Controls.Add(spanIcon);

            #region Gestion de l'action

            if (_list.ViewMainTable.EdnType == EdnType.FILE_USER || _list.ViewMainTable.TabType == TableType.RGPDTREATMENTSLOGS)
            {


                //selIcon.Attributes.Add("eaction", "LNKOPENPOPUP");
                //Finalement, page incrustée.
                //selIcon.Attributes.Add("eaction", "LNKGOUSERFILE");
                //Finalement, l'icon ouvre la popup
                selIcon.Attributes.Add("eaction", "LNKOPENPOPUP");
            }
            else if (_list.ViewMainTable.EdnType == EdnType.FILE_PJ)
            {
                selIcon.Attributes.Add("eaction", "LNKOPENPJPTIES");

                selIcon2.Attributes.Add("eaction", "LNKVIEWPJ");
                selIcon2.CssClass = "cell icon icon-edn-eye";
                selIcon2.ToolTip = eResApp.GetRes(Pref, 1229);
                eRecordPJ rowPj = (eRecordPJ)rowIcon;
                selIcon2.Attributes.Add("eTyp", rowPj.PjType.GetHashCode().ToString());
                selIcon2.Attributes.Add("lnkdid", rowPj.PJTabDescID.ToString());
                selIcon2.Attributes.Add("title", rowPj.ToolTip); //ID de la fiche
                selIcon2.ID = String.Concat("HEAD_ICON_COL2_", rowPj.MainFileid, "_", VirtualMainTableDescId);
                SetSecuredPJLinkAttribute(rowPj, selIcon2);
            }
            else if (RendererType == RENDERERTYPE.Bookmark || _list.ViewMainTable.EdnType != EdnType.FILE_MAIN /*|| _list.ViewMainTable.DescId == (int)TableType.ADR*/)
            {
                selIcon.CssClass = String.Concat(selIcon.CssClass, " OpnPup");
                selIcon.Attributes.Add("efld", "1");

                if (_list.ViewMainTable.EdnType == EdnType.FILE_PLANNING)
                    selIcon.Attributes.Add("eaction", "LNKOPENCALPUP");
                else
                    selIcon.Attributes.Add("eaction", "LNKOPENPOPUP");
            }
            else
            {
                //JAS Ouverture de la fiche depuis l'icone
                selIcon.Attributes.Add("eaction", "LNKGOFILE");
            }

            #endregion

            if (_list.ViewMainTable.EdnType == EdnType.FILE_MAIL || _list.ViewMainTable.EdnType == EdnType.FILE_SMS)
            {
                eRecordMail rowIconMail = (eRecordMail)rowIcon;

                // On ajoute un attribut sur les enregistrements correspondant à un mail en brouillon
                // Afin de guider le JS d'ouverture de popup, et lui indiquer d'ouvrir un mail existant en tant que brouillon
                if (rowIconMail.MailStatus == EmailStatus.MAIL_DRAFT.GetHashCode())
                    selIcon.Attributes.Add("draft", "1");
                else if (rowIconMail.MailStatus == EmailStatus.MAIL_NOT_SENT.GetHashCode())
                    selIcon.Attributes.Add("notsent", "1");
            }
            else if (_list.ViewMainTable.EdnType == EdnType.FILE_PLANNING && _list.ViewMainTable.CalendarEnabled)
            {
                eRecordPlanningCalendar rowCalendar = (eRecordPlanningCalendar)rowIcon;
                if (rowCalendar.ScheduleId > 0)
                {
                    selIcon.Attributes.Add("data-sid", rowCalendar.ScheduleId.ToString());
                }
            }

            //CNA - Affichage Minifiche
            if (VCMiniFileCheckMappingEnabled(this._tab))
                VCMiniFileMouseActionAttributes(selIcon, _tab);

            // selIcon.Controls.Add(img);
            bodyRowicon.Cells.Add(selIcon);

            if (_list.ViewMainTable.EdnType == EdnType.FILE_PJ)
                bodyRowicon.Cells.Add(selIcon2);
        }

        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row">Objet eRecord de la ligne en cours</param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected virtual void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            TableCell cellSelect = new TableCell();
            cellSelect.CssClass = String.Concat(sAltLineCss, " icon");

            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(row.IsMarked, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");


            chkSelect.AddClick("chkMarkedFile(this);");


            chkSelect.Attributes.Add("name", "chkMF");

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);
        }

        /// <summary>
        /// Retourne le nom de la table
        /// </summary>
        /// <returns></returns>
        public string GetMainTableLibelle()
        {
            return _list.ViewMainTable.Libelle;
        }


        /// <summary>
        /// Ajoute des proprité au bouton Afficher l'historique
        /// </summary>
        /// <param name="btnContainer">Le container </param>
        public void BuildHistoButton(HtmlGenericControl btnContainer)
        {
            eTools.BuildHistoBtn(Pref, btnContainer, _list.HistoInfo.Has, _list.HistoInfo.Actived, "mainlist");
        }

        /// <summary>
        /// Ajoute les liens opour les filtres ABC
        /// </summary>
        public Control GetCharIndex()
        {
            if (_list.ViewMainTable.EdnType != EdnType.FILE_MAIN)
                return null;

            System.Web.UI.WebControls.Table tblIdx = new System.Web.UI.WebControls.Table();
            tblIdx.ID = String.Concat("tblIDX_", VirtualMainTableDescId);
            tblIdx.CssClass = "idxFilter";

            tblIdx.Attributes.Add("CellPadding", "3");
            tblIdx.Attributes.Add("CellSpacing", "3");

            TableHeaderRow headerRow = new TableHeaderRow();
            headerRow.TableSection = TableRowSection.TableHeader;
            TableCell cellSelect;

            eConst.CHARINDEX_MODE chMode = eConst.CHARINDEX_MODE.ALL;
            Int32 nCharIndex = 0;
            String sCharIndex = Pref.GetPref(VirtualMainTableDescId, ePrefConst.PREF_PREF.CHARINDEX);

            if (!String.IsNullOrEmpty(sCharIndex))
                chMode = Int32.TryParse(sCharIndex, out nCharIndex) ? eConst.CHARINDEX_MODE.NUMERIC : eConst.CHARINDEX_MODE.ALPHABET;

            tblIdx.Attributes.Add("eVisuMode", (chMode == eConst.CHARINDEX_MODE.NUMERIC) ? "num" : "alp");
            tblIdx.Attributes.Add("eLastIdx", sCharIndex);

            cellSelect = new TableCell();
            cellSelect.Attributes.Add("id", "fidx_*");
            cellSelect.Text = eResApp.GetRes(Pref, 2216); /*cellSelect.Text = "Tout";*/
            // Demande ergonomique du 20/12/2011, la valeur "tout" n'est jamais selectionné
            //cellSelect.CssClass = (chMode == eConst.CHARINDEX_MODE.ALL) ? "fIOn" : "fIOff";
            cellSelect.CssClass = "fIAll";

            headerRow.Controls.Add(cellSelect);

            // Filtre Index A - Z
            for (char nCmpt = 'A'; nCmpt <= 'Z'; nCmpt++)
            {
                cellSelect = new TableCell();
                cellSelect.Attributes.Add("id", String.Concat("fidx_", nCmpt.ToString()));
                cellSelect.Attributes.Add("typ", "alp");
                cellSelect.Text = nCmpt.ToString();
                cellSelect.CssClass = (nCmpt.ToString() == sCharIndex) ? "fIOn" : "fIOff";

                if (chMode == eConst.CHARINDEX_MODE.NUMERIC)
                    cellSelect.CssClass += " fIHide";

                headerRow.Controls.Add(cellSelect);
            }

            // Filtre Index 0 - 9
            for (char nCmpt = '0'; nCmpt <= '9'; nCmpt++)
            {
                cellSelect = new TableCell();
                cellSelect.Attributes.Add("id", String.Concat("fidx_", nCmpt.ToString()));
                cellSelect.Attributes.Add("typ", "num");
                cellSelect.Text = nCmpt.ToString();
                cellSelect.CssClass = (nCmpt.ToString() == sCharIndex) ? "fIOn" : "fIOff";

                if (chMode != eConst.CHARINDEX_MODE.NUMERIC)
                    cellSelect.CssClass += " fIHide";

                headerRow.Controls.Add(cellSelect);
            }

            cellSelect = new TableCell();
            cellSelect.Attributes.Add("id", "fidx_alp");
            cellSelect.Text = " ABC ";
            cellSelect.CssClass = "fIOff";
            if (chMode != eConst.CHARINDEX_MODE.NUMERIC)
                cellSelect.CssClass += " fIHide";
            headerRow.Controls.Add(cellSelect);

            cellSelect = new TableCell();
            cellSelect.Attributes.Add("id", "fidx_num");
            cellSelect.Text = " 123 ";
            cellSelect.CssClass = "fIOff";
            if (chMode == eConst.CHARINDEX_MODE.NUMERIC)
                cellSelect.CssClass += " fIHide";
            headerRow.Controls.Add(cellSelect);

            tblIdx.Controls.Add(headerRow);

            return tblIdx;
        }

        /// <summary>
        /// Peuple de le conteneur donnée en paramètre des filtre rapides défini en admin
        /// </summary>
        /// <param name="conteneur"></param>
        public void AddQuickFilter(HtmlTable conteneur)
        {
            Int32 idx = 0;

            // Liste trié par la clé (index de la uservalue)
            SortedList<Int32, Control> controls = new SortedList<Int32, Control>();
            // Construteur du filtre
            eQuickFilter htmlField = new eQuickFilter(null, Pref, null);
            htmlField.MainTable = _list.ViewMainTable;

            try
            {
                List<Field> lstQuick = ((eListMain)_list).LstQuickFilter;

                if (lstQuick == null)
                    return;

                htmlField.AllQuickFieldFilter = lstQuick;

                // Construit les listBox
                foreach (Field fld in lstQuick)
                {
                    htmlField.SetField = fld;

                    Control ctrl = htmlField.RendQuickFilterList(out idx);
                    if (ctrl != null)
                    {
                        while (controls.ContainsKey(idx))
                            idx++;
                        controls.Add(idx, ctrl);
                    }
                }
            }
            finally
            {
                htmlField.Close();
            }

            HtmlTableRow rowContent = new HtmlTableRow();
            conteneur.Rows.Add(rowContent);
            // Ajoute les listBox dans l'ordre
            foreach (KeyValuePair<Int32, Control> keyValue in controls)
            {
                HtmlTableCell cellContent = new HtmlTableCell();
                rowContent.Cells.Add(cellContent);

                cellContent.Controls.Add(keyValue.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected void AddFilterTip()
        {
            FilterTipType lastInfTyp = FilterTipType.NONE;

            List<FilterTipInfo> lstInfo = ((eListMain)_list).FilterTipInfo;

            if (lstInfo.Count <= 0)
                return;

            HtmlGenericControl divTip = new HtmlGenericControl("div");
            divTip.ID = String.Concat("filterTip_", VirtualMainTableDescId);
            divTip.Attributes.Add("class", "filterTip");
            _divHidden.Controls.Add(divTip);

            foreach (FilterTipInfo inf in lstInfo)
            {
                if (lastInfTyp != inf.Type)
                {
                    // Libellé de la catégorie
                    switch (inf.Type)
                    {
                        case FilterTipType.MARKEDFILE:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 5061), 0, inf.Type);
                            break;
                        case FilterTipType.CHARINDEX:
                        case FilterTipType.HISTO:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 397), 0, inf.Type);
                            break;
                        case FilterTipType.RANDOM:
                        case FilterTipType.DEFAULT:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 397), 0, inf.Type);
                            break;
                        case FilterTipType.QUICK:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 727), 0, inf.Type);
                            break;
                        case FilterTipType.EXPRESS:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 5038), 0, inf.Type);
                            break;
                        case FilterTipType.ADVANCED:
                            AddFilterTipDesc(divTip, eResApp.GetRes(Pref, 6191), 0, inf.Type);
                            break;
                        default:
                            break;
                    }

                    lastInfTyp = inf.Type;
                }

                Boolean activeJs = inf.Type == FilterTipType.MARKEDFILE
                    || inf.Type == FilterTipType.CHARINDEX
                    || inf.Type == FilterTipType.HISTO
                    || inf.Type == FilterTipType.EXPRESS
                    || inf.Type == FilterTipType.ADVANCED;

                // Valeurs
                AddFilterTipDesc(divTip, inf.Label, 1, inf.Type, activeJs, inf.Value, inf.DivId, bHtml: inf.BHtml);
            }
        }

        /// <summary>
        /// Ajout une description de filter au parent passé en paramétre avec une simulation de puce
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="innerHtml"></param>
        /// <param name="level"></param>
        /// <param name="filter"></param>
        /// <param name="act"></param>
        /// <param name="value"></param>
        /// <param name="divId"></param>
        private void AddFilterTipDesc(Control parent, String innerHtml, Int32 level, FilterTipType filter, Boolean act = false, String value = "", String divId = "", Boolean bHtml = false)
        {
            if (filter == FilterTipType.ADVANCED)
            {
                // Si filtre avancé et libellé = numérique, alors ce doit être un filtre widget
                int wid = eLibTools.GetNum(innerHtml);
                if (wid > 0)
                {
                    innerHtml = eResApp.GetRes(Pref, 8268);
                }
            }

            innerHtml = HtmlTools.SanitizeHtml(innerHtml);


            HtmlGenericControl ctrl = eTools.GetHtmlGenericControl("div", String.Concat("&nbsp;", innerHtml));
            ctrl.Attributes.Add("tab", VirtualMainTableDescId.ToString());
            ctrl.Attributes.Add("typ", filter.GetHashCode().ToString());
            ctrl.Attributes.Add("lvl", level.ToString());
            ctrl.Attributes.Add("val", value.ToString());
            ctrl.Attributes.Add("html", bHtml ? "1" : "0");
            if (!String.IsNullOrEmpty(divId))
                ctrl.ID = divId;
            ctrl.Attributes.Add("act", act ? "1" : "0");

            if (value.Length > 0)
            {
                try
                {
                    //information complémentaire pour les filtres express
                    Field f = ((eListMain)_list).FldFieldsInfos.Find(f1 => f1.Descid.ToString() == value);
                    if (f?.ExpressFilterActived?.Length > 0)
                    {
                        string[] s = f.ExpressFilterActived.Split("$%$");
                        string sOp = s.Length > 0 ? s[0] : "";
                        if (s.Length > 1)
                            s = s[1].Split("#$|#$");
                        else
                            s = f.ExpressFilterActived.Split("#$|#$");
                        string sEdnVal = s.Length >= 2 ? s[1] : s[0];


                        ctrl.Attributes.Add("ednval", sEdnVal);
                        ctrl.Attributes.Add("ednop", sOp);
                    }
                }
                catch (Exception e)
                {

                }
            }

            parent.Controls.Add(ctrl);
        }

        #endregion


    }
}
