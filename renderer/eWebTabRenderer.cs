using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Merge;
using System.Linq;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu de l'onglet Web
    /// </summary>
    public class eWebTabRenderer : eRenderer
    {
        Boolean _httpsEnabled;
        Boolean _mixedContent; // iframe en http dans une appli https WTF
        Int32 _specifId;
        eSpecif _specif;

        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="eWebBkm"></param>
        public eWebTabRenderer(ePref pref, Int32 specifId, Int32 width, Int32 height, Boolean httpsEnabled)
        {
            Pref = pref;
            _rType = RENDERERTYPE.WebTab;
            _specifId = specifId;
            _width = width;
            _height = height;
            _httpsEnabled = httpsEnabled;
            _mixedContent = false;
        }

        /// <summary>
        /// Initialise un objet specif
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                _specif = eSpecif.GetSpecif(Pref, _specifId);
                if (_specif.Type != eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL && _specif.Type != eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL)
                    throw new Exception(string.Concat("La specif attendue doit être de type '", eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL.ToString(), " ou ", eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL.ToString(),
                        "'. Type de specif fournie '", _specif.Type.ToString(), "'"));


                if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL)
                {
                    if (_specif.Url.ToLower().StartsWith("http://") && _httpsEnabled)
                        _mixedContent = true;
                }
            }
            catch (Exception ex)
            {
                _eException = ex;
                _sErrorMsg = String.Concat("eWebTabRenderer::Init :", ex.Message);

                return false;
            }

            return base.Init();
        }

        /// <summary>
        /// construit l'iframe de la spécif
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            try
            {

                String sUrl = _specif.Url;

                if (!_mixedContent)
                {
                    HtmlGenericControl iFrame = new HtmlGenericControl("iframe");
                    if (_specif.Type == eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL)
                    {
                        string sEncode = ExternalUrlTools.GetCryptEncode(string.Concat("sid=", _specif.SpecifId, "&tab=", _specif.Tab, "&fid=0&descid=0"));
                        sUrl = String.Concat("eSubmitTokenXRM.aspx?t=", sEncode);
                    }

                    iFrame.Attributes.Add("src", sUrl);
                    iFrame.Attributes.Add("class", "webtabfrm");

                    _pgContainer.Attributes.Add("class", "webtabdiv");
                    _pgContainer.Style.Add("height", (_height - 135) + "px"); // 135 correspond a la hauteur fixe des partie de logo + onglet + sous-onglet 


                    _pgContainer.Controls.Add(iFrame);
                }
                else
                {
                    HtmlGenericControl divInfos = new HtmlGenericControl("div");
                    divInfos.Attributes.Add("class", "divInfos");
                    HtmlGenericControl span = new HtmlGenericControl("span");

                    span.InnerHtml = eResApp.GetRes(Pref, 6788);
                    divInfos.Controls.Add(span);

                    HtmlGenericControl link = new HtmlGenericControl("a");
                    link.InnerHtml = eResApp.GetRes(Pref, 6789);
                    link.Attributes.Add("href", sUrl);
                    link.Attributes.Add("title", sUrl);
                    link.Attributes.Add("target", "_blank");

                    divInfos.Controls.Add(link);
                    _pgContainer.Controls.Add(divInfos);

                    _pgContainer.Attributes.Add("class", "webtabdiv");
                    _pgContainer.Style.Add("height", (_height - 135) + "px");

                }

            }
            catch (Exception ex)
            {
                _eException = ex;
                _sErrorMsg = String.Concat("eWebTabRenderer::Build :", ex.Message);

                return false;
            }

            return base.Build();
        }


        /// <summary>
        /// Construit une toolbar pour la grille
        /// </summary>
        /// <returns></returns>
        public static Panel BuildGridToolBar()
        {
            Panel toolbar = new Panel();
            toolbar.CssClass = "xrm-grid-options";

            toolbar.Attributes.Add("onclick", "oGridToolbar.click(event);");

            //<span>Dernière mise à jour le 01/01/2000 à 12:00:00</span>
            HtmlGenericControl refreshDate = new HtmlGenericControl("span");
            refreshDate.Attributes.Add("id", "xrmGridRefreshDate");
            toolbar.Controls.Add(refreshDate);

            // Si actu manu
            HtmlGenericControl refresh = new HtmlGenericControl("span");
            refresh.Attributes.Add("class", "icon-refresh");
            refresh.Attributes.Add("action", "refresh");
            toolbar.Controls.Add(refresh);

            HtmlGenericControl option = new HtmlGenericControl("span");
            option.Attributes.Add("class", "icon-cog");
            option.Attributes.Add("action", "options");
            toolbar.Controls.Add(option);


            return toolbar;
        }

        /// <summary>
        /// construit l'iframe de la spécif
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }
    }


    /// <summary>
    /// Classe qui génére les sous-menu de l'onglet web
    /// </summary>
    public class eWebTabSubMenuRenderer
    {
        protected ePref _pref;

        protected Int32 _nTab;

        protected Int32 _ednType;

        protected List<eSpecif> _specifs;

        protected List<eRecord> _lstXrmGrids;

        protected Boolean _bFirstItem;

        protected Boolean _isListSelected;

        public String sError = String.Empty;

        public Exception innerException;

        /// <summary>
        /// LA première specifs
        /// </summary>
        /// <returns></returns>
        public Int32 FirstSpecifItemId()
        {
            if (hasSpecifItems())
                return _specifs[0].SpecifId;

            return 0;
        }

        /// <summary>
        /// LA première specifs
        /// </summary>
        /// <returns></returns>
        public Int32 FirstGridItemId()
        {
            if (hasGridItems())
                return _lstXrmGrids[0].MainFileid;

            return 0;
        }

        /// <summary>
        /// Initialise un onglet web lié ou pas a l'event/template
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="_nTab"></param>
        public eWebTabSubMenuRenderer(ePref _pref, int _nTab, Int32 ednType)
        {
            this._pref = _pref;
            this._nTab = _nTab;
            this._ednType = ednType;
            this._isListSelected = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_pref">Preference utilisateur</param>
        /// <param name="_nTab">Onglet selectionné</param>
        /// <param name="ednType">Type d onglet</param>
        /// <param name="isListSelected">Vrai si on vient du chargement d une fiche</param>
        public eWebTabSubMenuRenderer(ePref _pref, int _nTab, Int32 ednType, bool isListSelected)
        {
            this._pref = _pref;
            this._nTab = _nTab;
            this._ednType = ednType;
            this._isListSelected = isListSelected;
        }

        /// <summary>
        /// Récupère toutes les specifs de type page web
        /// </summary>
        public virtual Boolean Init()
        {
            try
            {
                _bFirstItem = true;
                _specifs = eSpecif.GetSpecifList(_pref, new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL, eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL }, _nTab);
                _lstXrmGrids = eWebTabSubMenuRenderer.GetGridsList(_pref, _nTab, _nTab == (int)TableType.XRMHOMEPAGE ? _pref.XrmHomePageId : 0);
            }
            catch (Exception ex)
            {
                sError = "eWebTab::Init : " + ex.Message;
                innerException = ex;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Récupération des grilles liées à la page d'accueil(avec fileId) OU ratachées à un onglet (sans fileId)
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="iTab">Tables parente des grilles</param>
        /// <param name="iFileId"> Id de la page d'accueil</param>
        /// <returns></returns>
        public static List<eRecord> GetGridsList(ePrefUser pref, int iTab, int iFileId = 0)
        {
            return new List<eRecord>(eGridList.GetRecordList(pref, iTab, iFileId));
        }

        /// <summary>
        /// Savoir s'il y a des éléments a afficher
        /// </summary>
        /// <returns></returns>
        public bool HasItems()
        {
            return hasSpecifItems() || hasGridItems();

        }

        protected bool hasSpecifItems()
        {
            return (_specifs?.Count ?? 0) > 0;
        }

        /// <summary>
        /// Savoir s'il y a des éléments a afficher
        /// </summary>
        /// <returns></returns>
        protected bool hasGridItems()
        {
            return (_lstXrmGrids?.Count ?? 0) > 0;
        }


        /// <summary>
        /// Construit le sous menu
        /// </summary>
        /// <param name="htmlGenericControl"></param>
        public virtual Boolean Build(HtmlGenericControl subMenu)
        {
            if (!HasItems())
                return false;

            //Booleen qui passe a vrai si on doit ajouter le sous menu "Liste des fiches"
            //bool bTodoAddListe = false;
            //Sous-onglet lié a un fichier ou sur la page d'accueil.
            if (_ednType != (int)EdnType.FILE_WEBTAB && _ednType != (int)EdnType.FILE_GRID)
            {
                if (_nTab == 0)
                {   //Accueil
                    subMenu.Controls.Add(newMenuItem(0, eResApp.GetRes(_pref, 551)));
                }
                else
                {
                    // bTodoAddListe = true;
                }
            }

            String sDisplayOrder = String.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.DisplayOrder);
            String sTitleAlias = String.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.Title);
            int displayOrder, oldDisplayOrder = 0;

            //Si la table possede des grilles et que la premiere grille possede un displayOrder a 0
            //alors affichage de la grille en premier, puis des autres grilles si displayOrder se suit, sinon "liste des fiches"
            if (hasGridItems() && Int32.TryParse(_lstXrmGrids[0].GetFieldByAlias(sDisplayOrder).DisplayValue, out displayOrder) && displayOrder == 0)
            {
                bool bAddListItem = false;
                foreach (eRecord rec in _lstXrmGrids)
                {
                    if (Int32.TryParse(rec.GetFieldByAlias(sDisplayOrder).DisplayValue, out displayOrder))
                    {
                        if (displayOrder == 0 || displayOrder == oldDisplayOrder + 1)
                        {
                            subMenu.Controls.Add(newMenuItem(rec.MainFileid, rec.GetFieldByAlias(sTitleAlias)?.DisplayValue ?? "", ItemType.GRID));
                        }
                        else
                        {
                            if (!bAddListItem) //Si on a pas deja ajouter l item liste des fiches on l ajoute une seule fois
                            {
                                subMenu.Controls.Add(newMenuItem(0, eResApp.GetRes(_pref, 179), ItemType.LIST)); // Liste
                                bAddListItem = true;
                            }

                            subMenu.Controls.Add(newMenuItem(rec.MainFileid, rec.GetFieldByAlias(sTitleAlias)?.DisplayValue ?? "", ItemType.GRID)); //On affiche la grille
                        }

                        oldDisplayOrder = displayOrder;
                    }
                }

                if (!bAddListItem) //Si toutes les grilles etait avant l item liste des fiches on l affiche en fin de liste
                    subMenu.Controls.Add(newMenuItem(0, eResApp.GetRes(_pref, 179), ItemType.LIST)); // Liste
            }
            else //Si pas de grilles ou grille.displayOrder > 0, on commence par l affichage de liste des fiches
            {
                if (_nTab != 0 && _nTab != (int)TableType.XRMHOMEPAGE && _ednType != (int)EdnType.FILE_WEBTAB && _ednType != (int)EdnType.FILE_GRID)
                {
                    subMenu.Controls.Add(newMenuItem(0, eResApp.GetRes(_pref, 179), ItemType.LIST)); // Liste
                }
                else
                    _isListSelected = false;

                foreach (eRecord rec in _lstXrmGrids)
                {
                    subMenu.Controls.Add(newMenuItem(rec.MainFileid, rec.GetFieldByAlias(sTitleAlias)?.DisplayValue ?? "", ItemType.GRID));
                }
            }

            //On affiche les specifs
            foreach (eSpecif spec in _specifs)
            {
                subMenu.Controls.Add(newMenuItem(spec.SpecifId, spec.Label, ItemType.SPECIF));
                // on met fin a la boucle si pas d'element la prochaine fois du coup pas de séparateur ajouté a la fin.
            }


            return true;
        }

        /// <summary>
        /// End
        /// </summary>
        /// <param name="subMenu"></param>
        /// <returns></returns>
        public Boolean End(HtmlGenericControl subMenu)
        {
            return true;
        }

        /// <summary>
        /// Ajoute un element au sous menu web
        /// </summary>
        /// <returns></returns>
        protected virtual HtmlContainerControl newMenuItem(Int32 id, String lib, ItemType itemType = ItemType.SPECIF)
        {
            HtmlContainerControl span = new HtmlGenericControl("span");
            HtmlContainerControl anchr = new HtmlGenericControl("a");
            Boolean isSelected = false;
            Boolean isFirstItem = _bFirstItem;

            if (IsSelected(id, itemType))
            {
                anchr.ID = "firstSubTabItem";
                isSelected = true;
                _bFirstItem = false;
            }

            anchr.InnerHtml = lib;
            anchr.Attributes.Add("class", "subTab" + (isSelected ? " selected" : ""));
            anchr.Attributes.Add("list", id == 0 ? "1" : "0"); // le premier element n'est pas une specif
            anchr.Attributes.Add("gid", id.ToString());
            anchr.Attributes.Add("selected", isSelected ? "1" : "0");
            anchr.Attributes.Add("itemtype", ((int)itemType).ToString());

            anchr.Attributes.Add("onclick", "oGridToolbar.load(event);");

            span.Controls.Add(anchr);
            return span;
        }

        /// <summary>
        /// Savoir si l'id de la grille est sélectionnée
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsSelected(Int32 id, ItemType itemType)
        {
            return (_isListSelected && itemType == ItemType.LIST) || (!_isListSelected && _bFirstItem);
        }

        /// <summary>
        /// Le premier sous-onglet est en gras
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Boolean isFirstSpecifItem(Int32 id)
        {
            return id == 0 || (FirstSpecifItemId() == id && _ednType == (int)EdnType.FILE_WEBTAB);
        }

        /// <summary>
        /// Le premier sous-onglet est en gras
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private Boolean isFirstGridItem(Int32 id)
        {
            return id == 0 || (FirstGridItemId() == id && _ednType == (int)EdnType.FILE_GRID);
        }

        /// <summary>
        /// Ajoute un séparteur vertical
        /// </summary>
        /// <returns></returns>
        private HtmlContainerControl newMenuSeparator()
        {
            HtmlContainerControl span = new HtmlGenericControl("span");
            span.InnerHtml = "|";
            return span;
        }
    }

    /// <summary>
    /// Classe qui génére les sous-menu de l'onglet web
    /// </summary>
    public class eXrmHomePageSubMenuRenderer : eWebTabSubMenuRenderer
    {
        int _fileId;

        /// <summary>
        /// Initialise un onglet web lié ou pas a l'event/template
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="id"> Id de la page d'accueil</param>
        /// <param name="_nTab"></param>
        public eXrmHomePageSubMenuRenderer(ePref _pref, int id, Int32 ednType) : base(_pref, (int)TableType.XRMHOMEPAGE, ednType)
        {
            _fileId = id;
        }

        /// <summary>
        /// Récupère toutes les 
        /// </summary>
        public override Boolean Init()
        {
            try
            {
                _bFirstItem = true;
                _isListSelected = false;
                _specifs = eSpecif.GetSpecifList(_pref, new List<eLibConst.SPECIF_TYPE>() { eLibConst.SPECIF_TYPE.TYP_WEBTAB_INTERNAL, eLibConst.SPECIF_TYPE.TYP_WEBTAB_EXTERNAL }, _nTab);
                _lstXrmGrids = GetGridsList(_pref, _nTab, _fileId);
            }
            catch (Exception ex)
            {
                sError = "eXrmHomePageSubMenuRenderer::Init : " + ex.Message;
                innerException = ex;

                return false;
            }

            return true;
        }


        public Control BuildSubMenu()
        {

            Panel mainListContent = new Panel();
            mainListContent.ID = "mainListContent";

            Panel infos = new Panel();
            infos.ID = "infos";
            mainListContent.Controls.Add(infos);

            if (!HasItems())
                return mainListContent;

            HtmlTable table = new HtmlTable();
            table.Attributes.Add("class", "hp-submenu");
            infos.Controls.Add(table);

            HtmlTableRow row = new HtmlTableRow();
            table.Rows.Add(row);

            HtmlTableCell cell = new HtmlTableCell();
            row.Cells.Add(cell);

            HtmlGenericControl SubTabMenuCtnr = new HtmlGenericControl("div");
            SubTabMenuCtnr.ID = "SubTabMenuCtnr";
            SubTabMenuCtnr.Attributes.Add("class", "subTabDiv");
            SubTabMenuCtnr.Attributes.Add("tab", ((int)TableType.XRMHOMEPAGE).ToString());
            SubTabMenuCtnr.Attributes.Add("fid", _fileId.ToString());

            cell.Controls.Add(SubTabMenuCtnr);


            Build(SubTabMenuCtnr);


            cell = new HtmlTableCell();
            cell.Controls.Add(eWebTabRenderer.BuildGridToolBar());
            row.Cells.Add(cell);


            return mainListContent;
        }


    }

    /// <summary>
    /// Renderer du signet grille
    /// </summary>
    public class eBookmarkGridSubMenu : eRenderer
    {
        private eXrmGridSubMenuRenderer _subMenuRenderer;

        /// <summary>
        /// Inistialisation d'une instance de signet
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="bkm"></param>
        /// <param name="gridId"></param>
        public eBookmarkGridSubMenu(ePref pref, eBookmark bkm, int gridId = 0)
        {
            _ePref = pref;
            _tab = bkm.ViewTabDescId;
            _subMenuRenderer = new eXrmGridSubMenuRenderer(_ePref, bkm.ViewTabDescId, bkm.BkmEdnType, gridId);
        }

        /// <summary>
        /// Init de sous-menu des grilles
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            return _subMenuRenderer.Init();
        }

        /// <summary>
        /// Construction du sous-menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            Control ctl = _subMenuRenderer.BuildSubMenu();
            if (!string.IsNullOrEmpty(_subMenuRenderer.sError))
            {
                _sErrorMsg = string.Concat(eResApp.GetRes(_ePref, 8149), " ", eResApp.GetRes(_ePref, 72));

                if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                {

                    ctl = new LiteralControl(_subMenuRenderer.sError);
                }
                else
                {
                    ctl = new LiteralControl(_sErrorMsg);
                }

                return false;
            }

            _pgContainer.Controls.Add(ctl);



            HtmlGenericControl listHeader = new HtmlGenericControl("div");
            listHeader.ID = "listheader";
            listHeader.Attributes.Add("class", "listheader");


            Panel panel = new Panel();
            if (_subMenuRenderer.HasItems())
            {

                panel.ID = "emptyGridPanel";

                int iGridId = _subMenuRenderer.FirstGridItemId();

                WhereCustom wc = new WhereCustom(
                    ((int)XrmWidgetField.Type).ToString()
                    , Operator.OP_NOT_IN_LIST
                    , eLibTools.Join(";", eXrmWidgetTools.AutoRefreshCompliantWidgetType.Select(wt => (int)wt))
                    );

                eList lstWidgetNotAutoRefreshCompliant = eListFactory.GetWidgetList(Pref, iGridId, whereCustom: wc);

                if (lstWidgetNotAutoRefreshCompliant.InnerException != null)
                    throw lstWidgetNotAutoRefreshCompliant.InnerException;

                if (!String.IsNullOrEmpty(lstWidgetNotAutoRefreshCompliant.ErrorMsg))
                    throw new EudoException(String.Concat("Une erreur est survenue lors de la vérification de la liste des Widgets : ", lstWidgetNotAutoRefreshCompliant.ErrorMsg), innerExcp: lstWidgetNotAutoRefreshCompliant.InnerException);

                if (lstWidgetNotAutoRefreshCompliant.ListRecords?.Count == 0)
                    panel.Attributes.Add("are", "1"); //are = AutoRefreshEnabled

                HtmlGenericControl text = new HtmlGenericControl();
                text.ID = "info";
                text.InnerHtml = $"{eResApp.GetRes(_ePref, 8573)} <span class='icon-forward' id='infoIcon'></span>";
                panel.Controls.Add(text);

            }
            else
            {
                panel.ID = "incompletGridPanel";
                ctl = new LiteralControl($"{eResApp.GetRes(_ePref, 1901)}<br/>{eResApp.GetRes(_ePref, 6342)}");
                panel.Controls.Add(ctl);

            }

            listHeader.Controls.Add(panel);
            _pgContainer.Controls.Add(listHeader);
            return true;
        }

        /// <summary>
        /// Rien à ajouter
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return true;
        }
    }


    /// <summary>
    /// Classe qui génére les sous-menu de l'onglet web
    /// </summary>
    public class eXrmGridSubMenuRenderer : eWebTabSubMenuRenderer
    {
        int _gridId;

        /// <summary>
        /// Initialise un onglet web lié ou pas a l'event/template
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfo">type de table</param>        
        /// <param name="gridId"> id de la grille</param>
        public eXrmGridSubMenuRenderer(ePref pref, TableLite tabInfo, int gridId) : base(pref, tabInfo.DescId, (int)tabInfo.EdnType)
        {
            _gridId = gridId;
        }

        /// <summary>
        /// Initialise un onglet web lié ou pas a l'event/template
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="descid">type de table</param>
        /// <param name="ednType">type de table</param>
        /// <param name="gridId"> id de la grille</param>
        public eXrmGridSubMenuRenderer(ePref pref, int descid, EdnType ednType, int gridId) : base(pref, descid, (int)ednType)
        {

            _gridId = gridId;
        }

        /// <summary>
        /// Initialise l'id de la grille à  afficher en premier
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            if (!base.Init())
                return false;

            // Si on connait pas l'id de la grille au clique sur un onglet web ou page d'accueil
            // on charge la première dans la liste
            if (_gridId == 0 && !HasListItemSubMenu())
            {
                _gridId = FirstGridItemId();
                if (_gridId == 0)
                    _gridId = FirstSpecifItemId();
            }

            return true;
        }

        /// <summary>
        /// Construit le sous-menu
        /// </summary>
        /// <returns></returns>
        public Control BuildSubMenu()
        {

            Panel mainListContent = new Panel();
            mainListContent.ID = "mainListContent";


            if (!HasItems())
                return mainListContent;

            Panel infos = new Panel();
            infos.ID = "infos";
            mainListContent.Controls.Add(infos);

            HtmlTable table = new HtmlTable();
            table.Attributes.Add("class", "hp-submenu");
            infos.Controls.Add(table);

            HtmlTableRow row = new HtmlTableRow();
            table.Rows.Add(row);

            HtmlTableCell cell = new HtmlTableCell();
            row.Cells.Add(cell);

            HtmlGenericControl SubTabMenuCtnr = new HtmlGenericControl("div");
            SubTabMenuCtnr.ID = "SubTabMenuCtnr";
            SubTabMenuCtnr.Attributes.Add("class", "subTabDiv");
            SubTabMenuCtnr.Attributes.Add("tab", _nTab.ToString());
            SubTabMenuCtnr.Attributes.Add("fid", "0");

            cell.Controls.Add(SubTabMenuCtnr);


            Build(SubTabMenuCtnr);


            cell = new HtmlTableCell();
            cell.Controls.Add(eWebTabRenderer.BuildGridToolBar());
            row.Cells.Add(cell);


            return mainListContent;
        }

        /// <summary>
        /// Construit le sous menu
        /// </summary>
        /// <param name="subMenuContainer"></param>
        public override bool Build(HtmlGenericControl subMenuContainer)
        {
            if (!HasItems())
                return false;

            RenderGridSubMenu(subMenuContainer);
            RenderSpecifSubMenu(subMenuContainer);

            return true;
        }

        /// <summary>
        /// Fait un rendu de sous-menu des grille en tenant compte de la position de l'entrée "Liste"
        /// </summary>
        /// <param name="subMenu"></param>
        private void RenderGridSubMenu(HtmlGenericControl subMenu)
        {
            eRecord record;
            string title;
            int displayOrder, freeIndex = _lstXrmGrids.Count;   // par défaut, pas de position libre, on se place à la fin       

            for (int index = 0; index < _lstXrmGrids.Count; index++)
            {
                record = _lstXrmGrids[index];

                RetrieveTitleAndDisplayOrder(record, out title, out displayOrder);

                // Si on trouve une position libre plus petite on la garde
                if (index < freeIndex && displayOrder != index)
                    freeIndex = index;

                subMenu.Controls.Add(newMenuItem(record.MainFileid, title, ItemType.GRID));
            }

            // On insere la liste à la position disponible      
            if (HasListItemSubMenu())
                subMenu.Controls.AddAt(freeIndex, newMenuItem(0, eResApp.GetRes(_pref, 179), ItemType.LIST));
        }

        /// <summary>
        /// Savoir si la table peut afficher l'entrée "List" dans le sous-menu des grilles
        /// Les types de table qui n'ont pas l'entrée : grid, tabweb, homepage 
        /// </summary>
        /// <returns></returns>
        private bool HasListItemSubMenu()
        {
            return _ednType != (int)EdnType.FILE_HOMEPAGE && _ednType != (int)EdnType.FILE_GRID && _ednType != (int)EdnType.FILE_WEBTAB;
        }

        /// <summary>
        /// Savoir si l'id de la grille est sélectionnée
        /// </summary>
        /// <returns></returns>
        protected override bool IsSelected(Int32 id, ItemType itemType)
        {
            return id == _gridId; /*&& itemType == ItemType.GRID;*/ // les specifs sont vouées a disparaitre
        }

        /// <summary>
        /// Fait un rendu de sous-menu des specifs en derniere position
        /// </summary>
        /// <param name="subMenu"></param>
        private void RenderSpecifSubMenu(HtmlGenericControl subMenu)
        {
            //On affiche les specifs
            foreach (eSpecif spec in _specifs)
                subMenu.Controls.Add(newMenuItem(spec.SpecifId, spec.Label, ItemType.SPECIF));
        }

        /// <summary>
        /// Récupère le titre et le displayorder de la grille
        /// </summary>
        /// <param name="record">enregistrement de la grille</param>
        /// <param name="title">titre de la grille</param>
        /// <param name="displayOrder">display order</param>
        private void RetrieveTitleAndDisplayOrder(eRecord record, out string title, out int displayOrder)
        {
            title = record.GetFieldByAlias(string.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.Title)).DisplayValue ?? "";
            displayOrder = eLibTools.GetNum(record.GetFieldByAlias(string.Format("{0}_{1}", (int)TableType.XRMGRID, (int)XrmGridField.DisplayOrder)).Value);
        }
    }
}