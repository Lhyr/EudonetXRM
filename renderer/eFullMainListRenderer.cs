using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Retourne le bloc complet d'une main liste (cad, la barre d'info, de pagination, d'action et la mainlist généré via un eListMainRenderer)
    ///  > peut retourner les blocs de façons indépendante
    /// 
    /// </summary>
    public class eFullMainListRenderer : eRenderer
    {

        eListMainRenderer _eListMainrdr = null;


        /// <summary>
        /// Descid de la table
        /// </summary>
        protected int _nTab;

        /// <summary>
        /// Numéro de page
        /// </summary>
        protected int _nPage;


        /// <summary>
        /// Nombre de ligne par page
        /// </summary>
        protected int _nRow;


        /// <summary>
        /// masque le bouton d'action sur les liste de sélections
        /// </summary>
        protected bool _bHideActionSelection = false;


        /// <summary>
        /// Panel contenant le rendu du menu haut
        /// </summary>
        protected Panel _pnlTopMenuList;


        /// <summary>
        /// Panel contennant le rendu de la liste
        /// </summary>
        protected Panel _pnlMainList;

        /// <summary>
        /// Table lite de la table a rendre
        /// </summary>
        protected TableLite _ednTab;


        /// <summary>
        /// Retourne un renderer eListRendererMain
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nPage">Page</param>
        /// <param name="nRow">Nombdre de ligne par page</param>
        /// <param name="tab">TableLite préchargée</param>
        internal static eFullMainListRenderer GetFullMainListRenderer(ePref pref, Int32 height, Int32 width, Int32 nPage, Int32 nRow, TableLite tab)
        {
            eFullMainListRenderer rdr = new eFullMainListRenderer(pref, tab.DescId, height, width, nPage, nRow);
            rdr._ednTab = tab;
            return rdr;
        }


        #region override des fonctions de génération

        /// <summary>
        /// Création de la main list
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _rType = RENDERERTYPE.FullMainList;
            Pref.Context.Paging.Tab = _nTab;


            _bHideActionSelection = _bHideActionSelection || (_ednTab.TabType == TableType.PJ);

            //Construction du renderer de la liste
            _eListMainrdr = eListMainRenderer.GetMainListRenderer(Pref, _nTab, _height, _width, _nPage, _nRow);

            //Génération du rendu
            if (_eListMainrdr.Generate())
                return true;
            else
            {
                _sErrorMsg = _eListMainrdr.ErrorMsg;
                _nErrorNumber = _eListMainrdr.ErrorNumber;
                _eException = _eListMainrdr.InnerException;

                return false;

            }
        }



        /// <summary>
        /// construction du conteneur et intégration des bloc
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (!base.Build())
                return false;


            /********        BLOC  MAIN MENU TOP   *************/
            //Div mainListContent
            _pnlTopMenuList = new Panel();
            PgContainer.Controls.Add(_pnlTopMenuList);
            _pnlTopMenuList.ID = "mainListContent";
            BuildTopMenu(_pnlTopMenuList);


            /********     BLOC  LIST         *************/
            _pnlMainList = new Panel();
            PgContainer.Controls.Add(_pnlMainList);
            _pnlMainList.ID = "listheader";
            _pnlMainList.CssClass = "listheader";

            //Ajout de la fontsize 
            String sUserFontSize = eTools.GetUserFontSize(Pref);
            _pnlMainList.CssClass += " fs_" + sUserFontSize + "pt";

            BuildListContent(_pnlMainList);


            return true;
        }





        /// <summary>
        /// Construction des sous-blocs de contenu
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {


            DicContent = new Dictionary<string, Content>();
            DicContent["MenuPanel"] = new Content() { Ctrl = GetMenuPanel, CallBackScript = GetCallBackScript };
            DicContent["UserPanel"] = new Content() { Ctrl = GetListPanel, CallBackScript = "" };

            return true;
        }

        #endregion


        #region méthodes redefinissable de génération de contenu

        /// <summary>
        /// Bouton historique
        /// </summary>
        /// <param name="tc2"></param>
        protected virtual void AddBtnHisto(HtmlControl tc2)
        {
            HtmlGenericControl pHistoFilter = new HtmlGenericControl("div");
            tc2.Controls.Add(pHistoFilter);
            pHistoFilter.ID = "histoFilter";
            _eListMainrdr?.BuildHistoButton(pHistoFilter);
        }


        /// <summary>
        /// Filtres rapides
        /// </summary>
        /// <param name="tbQuickFilter"></param>
        protected virtual void AddQuickFilter(HtmlControl tbQuickFilter)
        {
            _eListMainrdr?.AddQuickFilter((HtmlTable)tbQuickFilter);
        }

        /// <summary>
        /// Construction du bloc MainList qui contient le menu du haut de liste
        ///  (nombre de fiche, filtres avancés...)
        /// </summary>
        /// <param name="pMainListContent"></param>
        protected virtual void BuildTopMenu(Panel pMainListContent)
        {
            Panel pInfos = new Panel();

            //Div Infos
            pMainListContent.Controls.Add(pInfos);
            pInfos.ID = "infos";

            BuildDivInfos(pInfos);
            BuildListNameDiv(pInfos);


            //Div listfiltres
            Panel pListFilter = new Panel();
            pMainListContent.Controls.Add(pListFilter);
            pListFilter.ID = "listfiltres";
            pListFilter.CssClass = "listfiltres";

            HtmlTable tblFiltre = new HtmlTable();
            pListFilter.Controls.Add(tblFiltre);
            tblFiltre.Attributes.Add("class", "listfiltrestab");

            HtmlTableRow tr = new HtmlTableRow();
            tblFiltre.Rows.Add(tr);
            HtmlTableCell tc1 = new HtmlTableCell();
            tc1.Attributes.Add("nowrap", "nowrap");
            tr.Cells.Add(tc1);


            HtmlTable tbQuickFilter = new HtmlTable();
            tc1.Controls.Add(tbQuickFilter);
            tbQuickFilter.ID = "listQuickFilters";
            tbQuickFilter.Attributes.Add("class", "listQuickFilters");

            AddQuickFilter(tbQuickFilter);


            HtmlTableCell tc2 = new HtmlTableCell();
            //tc2.Attributes.Add("width", "280");
            tc2.Attributes.Add("nowrap", "nowrap");
            tc2.Attributes.Add("align", "right");
            tr.Cells.Add(tc2);


            AddBtnHisto(tc2);


            Panel padvFltMenu = new Panel();
            tc2.Controls.Add(padvFltMenu);
            padvFltMenu.CssClass = "advFltMenu";
            padvFltMenu.ID = "AdvFltMenu";
            padvFltMenu.Attributes.Add("onclick", "dispFltMenu(this,false);");
            padvFltMenu.Attributes.Add("onmouseover", "dispFltMenu(this,false);");
            padvFltMenu.Attributes.Add("onmouseover", "hideFltMenu(this);");

            eRightFilter eRF = new eRightFilter(Pref);
            if (eRF.HasRight(eLibConst.TREATID.FILTER))
            {
                bool canEditFilter = true;
                if (Pref.Context.Filters.ContainsKey(_nTab))
                {
                    FilterSel advFilterSel = Pref.Context.Filters[_nTab];
                    int filterSelId = advFilterSel.FilterSelId;

                    AdvFilter advFilter = new AdvFilter(Pref, filterSelId);
                    string sError = String.Empty;
                    if (AdvFilter.Load(Pref, advFilter, out sError))
                        canEditFilter = advFilter.FilterType != TypeFilter.DBL;
                    else
                        canEditFilter = false;

                    padvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,true,", filterSelId, ",'", advFilter.FilterName.Replace("'", @"\'"), "');");
                    padvFltMenu.Attributes["class"] += " advFltActiv ";
                }
                else
                {
                    padvFltMenu.Attributes["onclick"] = String.Concat("dispFltMenu(this,false);");
                }

                padvFltMenu.Attributes["onmouseover"] = padvFltMenu.Attributes["onclick"];
                padvFltMenu.Attributes.Add("ca", eRF.CanAddNewItem() ? "1" : "0");
                padvFltMenu.Attributes.Add("ce", eRF.CanEditItem() && canEditFilter ? "1" : "0");
            }
            else
            {
                padvFltMenu.Attributes["onclick"] = String.Concat("eAlert(3, top._res_6834, top._res_6837);");
            }

            HtmlGenericControl span = new HtmlGenericControl("span");
            padvFltMenu.Controls.Add(span);
            span.InnerHtml = eResApp.GetRes(Pref, 6191);


        }

        /// <summary>
        /// Construction du bloc infos. Il s'agit du bloc contenant le nom/nombre d'élement de la liste, les boutons de notif, impression et autres
        /// </summary>
        /// <param name="p"></param>     
        protected virtual void BuildDivInfos(Panel p)
        {
            // Si on un filtre sur un champs qui a une règle d'affichage, on affiche pas le compteur
            //  Commenté voir #62537 - Attente de validation 
            // if (_eListMainrdr._list.HasFiltredFieldWithViewRule)
            //     return;


            #region Onglets web
            //pour les "Non calendrier", affichage des onglets web

            HtmlGenericControl pSubTabMenuCtnr = new HtmlGenericControl("div");
            pSubTabMenuCtnr.ID = "SubTabMenuCtnr";
            pSubTabMenuCtnr.Attributes.Add("tab", _nTab.ToString());
            pSubTabMenuCtnr.Attributes.Add("class", "subTabDiv");

            DoWebTab(pSubTabMenuCtnr);

            p.Controls.Add(pSubTabMenuCtnr);

            #endregion

            #region  Refresh grille - par defaut masqué, show=0     
            Panel toolbarCtn = new Panel();
            toolbarCtn.CssClass = "grid-toolbar-ctn";
            toolbarCtn.Attributes.Add("toolbar", "0");
            toolbarCtn.Controls.Add(eWebTabRenderer.BuildGridToolBar());
            p.Controls.Add(toolbarCtn);
            #endregion


        }


        /// <summary>
        /// Label de la liste
        /// </summary>
        /// <param name="element"></param>
        protected virtual void ContentLabel(HtmlGenericControl element)
        {

            if (_eListMainrdr?.ErrorNumber == QueryErrorType.ERROR_NUM_FILTER_NOT_AVAILABLE)
            {
                // #35140 : Si filtre non disponible, on affiche le message suivant :                
                element.InnerHtml = String.Concat(eResApp.GetRes(Pref, 815), ". <a href='#' onclick=\"cancelAdvFlt(", _nTab, ")\">", eResApp.GetRes(Pref, 1179), "</a>.");
            }
            else
            {
                String filterName = String.Empty;
                if (Pref.Context.Filters.ContainsKey(_nTab) && Pref.Context.Filters[_nTab] != null)
                {
                    // Affichage de la description du filtre
                    string error = string.Empty;
                    string filterDescription = AdvFilter.GetDescription(Pref, Pref.Context.Filters[_nTab].FilterSelId, out filterName, out error);
                    if (String.IsNullOrEmpty(error))
                    {
                        eTools.DisplayFilterTooltip(element, filterDescription);
                    }
                    filterName = String.Concat(" - ", filterName);
                }


                element.InnerText = string.Concat(_eListMainrdr?.GetMainTableLibelle() ?? "", filterName);
            }
        }

        /// <summary>
        ///Construit le div avec le nom de la table/filter/nb file
        /// </summary>
        /// <param name="p"></param>
        protected virtual void BuildListNameDiv(Panel p)
        {
            Panel panel = new Panel();
            panel.ID = "tabInfos";

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.ID = "iconeFilter";
            icon.Attributes.Add("class", "icon-list_filter");
            icon.Attributes.Add("onmouseover", "stfilter(event);");
            panel.Controls.Add(icon);

            Panel label = new Panel();
            label.CssClass = "lib";
            HtmlGenericControl element = new HtmlGenericControl();
            element.ID = "SpanNbElem";
            label.Controls.Add(element);

            element = new HtmlGenericControl();
            element.ID = "SpanLibElem";


            ContentLabel(element);

            label.Controls.Add(element);
            panel.Controls.Add(label);
            p.Controls.Add(panel);

        }

        /// <summary>
        /// Permet de changer la taille de la table pagination en fonction du renderer
        /// </summary>
        /// <param name="tbListAction"></param>
        protected virtual void SetPagingTableWith(System.Web.UI.WebControls.Table tbListAction)
        {


        }

        /// <summary>
        /// Création des action de la liste
        /// </summary>
        protected virtual void BuildAction(TableRow tr1)
        {
            TableCell tcFirst = new TableCell();
            tr1.Cells.Add(tcFirst);
            tcFirst.Style.Add("width", "20%");

            #region actionmenu
            if (!_bHideActionSelection)
            {
                Panel pnl = new Panel();
                tcFirst.Controls.Add(pnl);
                pnl.CssClass = "actions";
                pnl.Attributes.Add("eudoaction", "1");
                pnl.ID = "actionsH";

                Panel pnlBtn = new Panel();
                pnl.Controls.Add(pnlBtn);
                pnlBtn.ID = "btnActionsTop";

                Panel pnlActionLeft = new Panel();
                pnlBtn.Controls.Add(pnlActionLeft);
                pnlActionLeft.ID = "actionLeft";

                Panel pnlActionLeftAction = new Panel();
                pnlActionLeft.Controls.Add(pnlActionLeftAction);
                pnlActionLeftAction.ID = "aGH";
                pnlActionLeftAction.CssClass = "icon-bt-actions-left aGH";




                Panel pnlActionMiddle = new Panel();
                pnlBtn.Controls.Add(pnlActionMiddle);
                pnlActionMiddle.ID = "actionMiddle";

                Panel pnlActionMiddleAction = new Panel();
                pnlActionMiddle.Controls.Add(pnlActionMiddleAction);
                pnlActionMiddleAction.ID = "aMH";
                pnlActionMiddleAction.CssClass = "aM";
                pnlActionMiddleAction.Attributes.Add("onmouseover", "displayActions('H')");
                pnlActionMiddleAction.Attributes.Add("onmouseout", "omouA('H');hideActions('H');");
                pnlActionMiddleAction.Attributes.Add("onclick", "displayActions('H')");
                HtmlGenericControl span = new HtmlGenericControl("span");
                pnlActionMiddleAction.Controls.Add(span);
                span.InnerHtml = eResApp.GetRes(Pref, 296);



                Panel pnlActionRight = new Panel();
                pnlBtn.Controls.Add(pnlActionRight);
                pnlActionRight.ID = "actionRight";

                Panel pnlActionRightAction = new Panel();
                pnlActionRight.Controls.Add(pnlActionRightAction);
                pnlActionRightAction.ID = "aDH";
                pnlActionRightAction.CssClass = "icon-bt-actions-right aDH";
                pnlActionRightAction.Attributes.Add("onmouseover", "displayActions('H')");
                pnlActionRightAction.Attributes.Add("onmouseout", "omouA('H');hideActions('H');");
                pnlActionRightAction.Attributes.Add("onclick", "displayActions('H')");


            }
            #endregion


            TableCell tcSecond = new TableCell();
            tr1.Cells.Add(tcSecond);
            tcSecond.Style.Add("width", "22%");

            #region action menu
            if (!_bHideActionSelection)
            {

                MarkedFilesSelection ms = null;
                Pref.Context.MarkedFiles.TryGetValue(this._nTab, out ms);

                HtmlGenericControl span = new HtmlGenericControl("span");
                tcSecond.Controls.Add(span);

                span.InnerHtml = String.Concat(eResApp.GetRes(Pref, 187), "&nbsp;:&nbsp;");
                span.Style.Add("font-style", "italic");
                HtmlGenericControl spanMarked = new HtmlGenericControl("span");
                span.Controls.Add(spanMarked);
                spanMarked.ID = "nbCheckedHead";
                spanMarked.InnerText = ms?.NbFiles.ToString() ?? "0";

            }

            #endregion
        }

        /// <summary>
        /// Pagging
        /// </summary>
        /// <param name="tr1"></param>
        /// <param name="withForceReload">Force le reload (js)</param>
        protected virtual void AddPagging(TableRow tr1, bool withForceReload = false)
        {
            String naviFctParam = _nTab.ToString();

            //first Page
            TableCell tcFirst = new TableCell();
            tcFirst.Attributes.Add("width", "20px");
            tr1.Cells.Add(tcFirst);


            Panel pFirst = new Panel();
            tcFirst.Controls.Add(pFirst);
            pFirst.ID = "idFirst";
            pFirst.CssClass = "icon-edn-first icnListAct";
            pFirst.Attributes.Add("onclick", String.Concat("firstpage(", naviFctParam, ", ", withForceReload ? "true" : "false", ")"));


            // précédente
            TableCell tcPrev = new TableCell();
            tcPrev.Attributes.Add("width", "15px");
            tr1.Cells.Add(tcPrev);
            Panel pPrev = new Panel();
            tcPrev.Controls.Add(pPrev);
            pPrev.ID = "idPrev";
            pPrev.CssClass = "icon-edn-prev fLeft icnListAct";
            pPrev.Attributes.Add("onclick", String.Concat("prevpage(", naviFctParam, ", ", withForceReload ? "true" : "false", ")"));


            //Num PAge
            TableCell tcNumPage = new TableCell();
            tcNumPage.Attributes.Add("width", "8%");
            tcNumPage.Attributes.Add("class", "numpage");
            tcNumPage.Attributes.Add("align", "center");
            tr1.Cells.Add(tcNumPage);

            System.Web.UI.WebControls.Table tbNum = new System.Web.UI.WebControls.Table();
            tcNumPage.Controls.Add(tbNum);

            tbNum.Attributes.Add("width", "100%");
            tbNum.Attributes.Add("cellpadding", "0");
            tbNum.Attributes.Add("cellspacing", "0");
            tbNum.Attributes.Add("border", "0");

            TableRow trNP = new TableRow();
            tbNum.Rows.Add(trNP);
            TableCell tcNp = new TableCell();
            trNP.Cells.Add(tcNp);
            tcNp.Style.Add("text-align", "right");
            tcNp.Style.Add("width", "50%");

            HtmlGenericControl span = new HtmlGenericControl("span");
            tcNp.Controls.Add(span);
            HtmlInputText inpt = new HtmlInputText("text");
            span.Controls.Add(inpt);
            inpt.Attributes.Add("class", "pagInput");
            inpt.ID = "inputNumpage";
            inpt.Name = "inputNumpage";
            inpt.Size = 1;
            inpt.Style.Add("display", "none");
            inpt.Attributes.Add("onblur", String.Concat("selectpage(", naviFctParam, ",this)"));
            inpt.Attributes.Add("onkeypress", String.Concat("if(isValidationKey(event))selectpage(", naviFctParam, ",this);"));

            TableCell tcNpD = new TableCell();
            trNP.Cells.Add(tcNpD);
            tcNpD.Style.Add("text-align", "left");
            tcNpD.Style.Add("width", "50%");
            tcNpD.ID = "tdNumpage";

            Panel pDivNumPage = new Panel();
            tcNpD.Controls.Add(pDivNumPage);
            pDivNumPage.ID = "divNumpage";

            //Num Next
            TableCell tcNext = new TableCell();
            tcNext.Attributes.Add("width", "15px");
            tr1.Cells.Add(tcNext);

            Panel pNext = new Panel();
            tcNext.Controls.Add(pNext);
            pNext.ID = "idNext";
            pNext.CssClass = "icon-edn-next fRight icnListAct";
            pNext.Attributes.Add("onclick", String.Concat("nextpage(", naviFctParam, ", ", withForceReload ? "true" : "false", ")"));

            // #39926 CRU : Si on n'est pas sur la dernière page, l'icône next ne doit pas être "actif"
            //if (_nPage < Pref.Context.Paging.NbPage)
            //   pNext.Style.Add("background-color", Pref.ThemeXRM.Color);


            //Last Page
            TableCell tcLast = new TableCell();
            tcLast.Attributes.Add("width", "20px");
            tr1.Cells.Add(tcLast);
            Panel pLast = new Panel();
            tcLast.Controls.Add(pLast);
            pLast.ID = "idLast";
            pLast.CssClass = "icnListAct icon-edn-last";
            pLast.Attributes.Add("onclick", String.Concat("lastpage(", naviFctParam, ", ", withForceReload ? "true" : "false", ")"));
        }

        /// <summary>
        /// Bouton d
        /// </summary>
        /// <param name="tr1"></param>
        protected virtual void AddSearchBtn(TableRow tr1)
        {

            TableCell tcSearch = new TableCell();
            tr1.Cells.Add(tcSearch);

            tcSearch.CssClass = "eFSTD";
            tcSearch.Style.Add("width", "42%");

            Panel pSearch = new Panel();
            tcSearch.Controls.Add(pSearch);
            pSearch.ID = "eFS";
            pSearch.CssClass = "eFSContainer";

            HtmlInputText inpt = new HtmlInputText("text");
            pSearch.Controls.Add(inpt);

            inpt.Attributes.Add("onfocus", " searchFocus();");
            inpt.Attributes.Add("onblur", " searchBlur();");
            inpt.Attributes.Add("title", "");
            inpt.Attributes.Add("class", "eFSInput");
            inpt.Attributes.Add("onkeyup", "launchSearch(this.value, event);");
            //inpt.Attributes.Add("autocomplete", "off"); // US #2 224 - Demande #82 074 - Désactivation de l'autocomplétion système/navigateur sur les champs affichant des résultats de recherche - A décommenter si on câble un comportement spécifique
            inpt.Attributes.Add("maxlength", "100");
            inpt.Name = "q";
            inpt.ID = "eFSInput";

            Panel pStatus = new Panel();
            pSearch.Controls.Add(pStatus);
            pStatus.ID = "eFSStatus";


            //Affichage du champ de recherche
            if (!_eListMainrdr?.DrawSearchField ?? true)
                pSearch.Style.Add("display", "none");
        }

        /// <summary>
        /// Construction de la liste proprement dite
        /// Méthode d'assemblage
        /// </summary>
        /// <param name="pListHeader"></param>
        protected virtual void BuildListContent(Panel pListHeader)
        {

            //BOuton d'action
            Panel pListactions = new Panel();

            //Div listactions 1
            pListHeader.Controls.Add(pListactions);
            pListactions.ID = "actions";
            pListactions.CssClass = "listactions";
            System.Web.UI.WebControls.Table tbListAction = new System.Web.UI.WebControls.Table();
            pListactions.Controls.Add(tbListAction);

            tbListAction.Attributes.Add("cellpadding", "0");
            tbListAction.Attributes.Add("cellspacing", "0");
            tbListAction.Attributes.Add("border", "0");
            tbListAction.CssClass = "listactionstab";
            SetPagingTableWith(tbListAction);


            TableRow tr1 = new TableRow();
            tr1.TableSection = TableRowSection.TableBody;
            tbListAction.Rows.Add(tr1);


            BuildAction(tr1);
            AddPagging(tr1);
            AddSearchBtn(tr1);


            //Div Content
            Panel pListContent = new Panel();
            pListHeader.Controls.Add(pListContent);
            pListContent.ID = "listContent";

            //Carthographie, cela permet d'envoyer un evenment de mouseover pour les abonnés. 
            pListContent.Attributes.Add("onmouseover", "onListMouseOver(event); return false;");
            pListContent.Attributes.Add("onmouseout", "onListMouseOut(event); return false;");
            pListContent.Width = new Unit("100%"); // ? pkoi pas en css ?

            if (AddSpecialListContent(pListContent) && _eListMainrdr != null)
            {

                // Panel de filtre index            
                Panel pFlxIdx = new Panel();

                //Div Filter Index
                pListHeader.Controls.Add(pFlxIdx);
                pFlxIdx.ID = "fltindex";
                pFlxIdx.CssClass = "idxFilter";
                pFlxIdx.Style.Add("visibility", "hidden");


                if (_eListMainrdr.ErrorNumber != QueryErrorType.ERROR_NUM_FILTER_NOT_AVAILABLE)
                {
                    //Ajout du contenu de la liste
                    if (_eListMainrdr.ErrorMsg.Length == 0)
                        pListContent.Controls.Add(_eListMainrdr.PgContainer);

                    Control ctrlCharIndex = _eListMainrdr.GetCharIndex();
                    if (ctrlCharIndex == null)
                        pFlxIdx.Style["visibility"] = "hidden";
                    else
                    {
                        pFlxIdx.Controls.Add(_eListMainrdr.GetCharIndex());
                        pFlxIdx.Style["visibility"] = "";
                    }
                }
                else
                {
                    pListHeader.Visible = false;
                }
            }

            // #75 981 - ELAIZ/MABBE - Ajout d'un attribut pour cibler en CSS le planning graphique depuis des éléments parents de CalDivMain
            if (pListContent.Attributes["contenttype"] != null)
                pListHeader.Attributes.Add("contenttype", pListContent.Attributes["contenttype"]);
        }


        /// <summary>
        /// Conteneur "spécial" remplacement le renderer standard.
        /// 
        /// retourne true si le traitement doit continuer après appel
        /// </summary>
        /// <param name="pnl"></param>
        protected virtual bool AddSpecialListContent(Panel pnl)
        {
            return true;
        }

        #endregion


        /// <summary>
        /// Retourne le panel du menu du haut
        /// </summary>
        public Panel GetMenuPanel
        {
            get
            {
                return _pnlTopMenuList;

            }

        }

        /// <summary>
        /// Retourne le panel de la liste
        /// </summary>
        public Panel GetListPanel
        {
            get
            {
                return _pnlMainList;

            }

        }

        /// <summary>
        /// constructeur standard
        /// </summary>
        /// <param name="pref">Pref utilisateur</param>
        /// <param name="nTab">DescId de la table à rendrer</param>
        /// <param name="height">dimension de la fen^tre de rendu</param>
        /// <param name="width">dimension de la fen^tre de rendu</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="nRow">Ligne par page</param>       
        protected eFullMainListRenderer(ePref pref, Int32 nTab, Int32 height, Int32 width, Int32 nPage, Int32 nRow)
        {
            Pref = pref;
            _nTab = nTab;
            _height = height;
            _width = width;
            _nPage = nPage;
            _nRow = nRow;

        }

        /// <summary>
        /// Fait un rendu de l'onglet web/ des sous-onglet
        /// </summary>
        /// <param name="cpnl"></param>
        protected virtual void DoWebTab(HtmlGenericControl cpnl)
        {
            // Dev 38096 demandes parante: onglet web
            var subMenuRenderer = new eWebTabSubMenuRenderer(Pref, _nTab, (int)_ednTab.EdnType, true);
            if (subMenuRenderer.Init())
            {
                if (subMenuRenderer.HasItems())
                    subMenuRenderer.Build(cpnl);
            }
            else
            {
                subMenuRenderer.sError += "Impossible d'initialiser le renderer eWebTabSubMenuRenderer";
            }

            if (subMenuRenderer.sError.Length > 0)
            {
                //Avec exception
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", subMenuRenderer.sError, Environment.NewLine, "Exception StackTrace :", subMenuRenderer.innerException.StackTrace);

                throw subMenuRenderer.innerException ?? new Exception(sDevMsg);
            }
        }




    }
}