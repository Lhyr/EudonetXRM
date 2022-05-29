using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EudoExtendedClasses;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eSelectionWizardRenderer : eBaseWizardRenderer
    {
        private Int32 _iWidth;
        private Int32 _iHeight;
        protected Int32 _iTab;
        protected Int32 _iTabSource;

        private eSelectionWizardRenderer(ePref ePref, Int32 width, Int32 height, Int32 tab, Int32 tabSource)
        {
            _nbStep = 2;
            _iWidth = width;
            _iHeight = height;
            Pref = ePref;
            _iTab = tab;
            _iTabSource = tabSource;
            _pgContainer.ID = "wizard";
            lstStep.Add(new WizStep(1, "Sélection des fiches", BuildSelectionPanel));
            lstStep.Add(new WizStep(2, "Création des fiches", BuildCreationPanel));
        }


        public static eSelectionWizardRenderer GetSelectionWizardRenderer(ePref ePref, Int32 width, Int32 height, Int32 tab, Int32 tabSource)
        {
            eSelectionWizardRenderer myRenderer = new eSelectionWizardRenderer(ePref, width, height, tab, tabSource);
            return myRenderer;
        }

        /// <summary>Construction étape 1</summary>
        /// <returns></returns>
        private Panel BuildSelectionPanel()
        {
            Panel p = new Panel();
            p.ID = "selectionWizard";

            HiddenField hidTabSource = new HiddenField();
            hidTabSource.ID = "hidTabSource";
            hidTabSource.Value = _iTabSource.ToString();

            p.Controls.Add(hidTabSource);

            HiddenField hidFilters = new HiddenField();
            hidFilters.ID = "hidFilters";

            p.Controls.Add(hidFilters);

            #region Bloc critères
            eRenderer criteria = eRendererFactory.CreateSelectionCriteriaRenderer(Pref, _iTab);
            p.Controls.Add(criteria.PgContainer);
            #endregion

            #region Bloc carte
            //Boolean hasGeoField = true; // TODO


            //if (hasGeoField)
            //{
            //    Panel panelMap = new Panel();
            //    panelMap.ID = "blockMap";
            //    p.Controls.Add(panelMap);
            //}
            #endregion

            #region Bloc liste
            Panel panelList = new Panel();
            panelList.ID = "blockSelectionList";
            

            Panel panelMap = BuildMapPanel();
            panelList.Controls.Add(panelMap);

            p.Controls.Add(panelList);

            Panel panelTable = new Panel();
            panelTable.ID = "blockList";
            panelList.Controls.Add(panelTable);

            #endregion

            return p;
        }

        /// <summary>Construction étape 2</summary>
        /// <returns></returns>
        private Panel BuildCreationPanel()
        {
            Panel p = new Panel();
            return p;
        }

        public static Panel BuildMapPanel()
        {

            Panel panelBlockMap= new Panel();
            panelBlockMap.ID = "blockMap";

            HiddenField hidMapView = new HiddenField();
            hidMapView.ID = "hidMapView";
            panelBlockMap.Controls.Add(hidMapView);

            Panel toolbar = new Panel();
            toolbar.ID = "toolbarContainer";
            panelBlockMap.Controls.Add(toolbar);

            Panel pnMessage = new Panel();
            pnMessage.ID = "mapMessage";
            pnMessage.Style.Add("display", "none");
            pnMessage.Controls.Add(new LiteralControl("Veuillez affiner votre recherche pour afficher les points"));
            panelBlockMap.Controls.Add(pnMessage);

            Panel panelMap = new Panel();
            panelMap.ID = "map";
            panelBlockMap.Controls.Add(panelMap);

            return panelBlockMap;
        }


        public static eRenderer BuildListPart(ePref pref, ExtendedDictionary<String, String> dicParam, out String error)
        {
            error = String.Empty;

            if (dicParam == null)
            {
                error = "Params manquants";
                return null;
            }

            int nTab;
            int nTabSource;
            int nRows;
            int nPage;
            int nWidth;
            int nHeight;
            String filters;
            Boolean bReloadMap;

            dicParam.TryGetValueConvert<Int32>("tab", out nTab);
            dicParam.TryGetValueConvert<Int32>("tabsource", out nTabSource);
            dicParam.TryGetValueConvert<Int32>("rows", out nRows);
            dicParam.TryGetValueConvert<Int32>("page", out nPage);
            dicParam.TryGetValueConvert<Int32>("width", out nWidth);
            dicParam.TryGetValueConvert<Int32>("height", out nHeight);
            dicParam.TryGetValue("filters", out filters);
            dicParam.TryGetValueConvert<Boolean>("reloadmap", out bReloadMap);

            List<WhereCustom> listwc;
            if (!String.IsNullOrEmpty(filters)) 
                listwc = GetFiltersWhereCustom(filters);
            else
                listwc = new List<WhereCustom>();

            eRenderer eGlobalRender = eRenderer.CreateRenderer();

            eList list = eListFactory.CreateListFilteredSelection(pref, nTabSource, nTab, listwc, nRows, nPage);

            if (list.ErrorMsg.Length > 0 || list.InnerException != null)
            {
                eGlobalRender.SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, list.ErrorMsg, list.InnerException);
                return eGlobalRender;
            }

            eListFilteredSelection eFilteredSel = (eListFilteredSelection)list;

            //Panel pnMap = BuildMapPanel();

            // Ajout du champ de type hidden qui contient toutes les coordonnées et infos pour les pushpins/infoboxes
            if (eFilteredSel.NbTotalRows <= eModelConst.NB_MAX_MAP_PUSHPINS)
            {
                eList listAll = eListFactory.CreateListFilteredSelection(pref, nTabSource, nTab, listwc, 0, 0);

                eListFilteredSelection eFilteredSelAll = (eListFilteredSelection)listAll;

                HiddenField hidLocations = new HiddenField();
                hidLocations.ID = "hidLocations";
                hidLocations.Value = eFilteredSelAll.GeoLocations;

                eGlobalRender.PgContainer.Controls.Add(hidLocations);
            }

            // Ajout du champ de type hidden qui contient le champ de type geo
            HiddenField hidGeoField = new HiddenField();
            hidGeoField.ID = "hidGeoField";
            hidGeoField.Value = eFilteredSel.GeoField;

            eGlobalRender.PgContainer.Controls.Add(hidGeoField);

            //Panel panelWrapper = new Panel();
            //panelWrapper.ID = "blockList";
            //if (eFilteredSel.HasGeoField)
            //{
            //    panelWrapper.Width = new Unit(580, UnitType.Pixel);
            //}
            //else
            //{
            //    panelWrapper.Width = new Unit(100, UnitType.Percentage);
            //}
            //eGlobalRender.PgContainer.Controls.Add(panelWrapper);

            #region Nb de fiches sélectionnées
            Panel pnCfm = new Panel();
            pnCfm.ID = "CfmDest";
            pnCfm.CssClass = "CfmDest";
            String sLibCfm = String.Concat("Aucune fiche sélectionnée parmi les ", list.NbTotalRows, " fiches proposées");
            pnCfm.Controls.Add(new LiteralControl(sLibCfm));

            eGlobalRender.PgContainer.Controls.Add(pnCfm);
            #endregion

            #region Pagination + icône Rubrique
            Panel pnPaggingBtnBar = eSelectionWizardRenderer.CreateListSelPagingBar(nPage, list);
            eGlobalRender.PgContainer.Controls.Add(pnPaggingBtnBar);
            #endregion

            eRenderer listRenderer = eRendererFactory.CreateSelectionRenderer(pref, nTab, list, nWidth, nHeight, nRows, nPage);
            if (listRenderer.ErrorMsg.Length > 0 || listRenderer.InnerException != null)
            {
                eGlobalRender.SetError(QueryErrorType.ERROR_NUM_FIELD_NOT_FOUND, listRenderer.ErrorMsg, listRenderer.InnerException);
                return eGlobalRender;
            }
            eGlobalRender.PgContainer.Controls.Add(listRenderer.PgContainer);

            return eGlobalRender;
        }





        /// <summary>Retourne une liste de WhereCustom à partir du param filtres</summary>
        /// <param name="filters">The filters.</param>
        /// <returns></returns>
        public static List<WhereCustom> GetFiltersWhereCustom(String filters)
        {
            List<WhereCustom> listWC = new List<WhereCustom>();
            String[] arrExp;
            String[] arrFilters = filters.Split("&&");
            foreach (String filter in arrFilters)
            {
                if (!String.IsNullOrEmpty(filter))
                {
                    arrExp = filter.Split(";|;");
                    listWC.Add(new WhereCustom(arrExp[0], (Operator)eLibTools.GetNum(arrExp[1]), arrExp[2]));
                }
                
            }

            return listWC;
        }


        /// <summary>
        /// Création des boutons pour le paging dans la liste des invitations
        /// 
        /// </summary>
        /// <returns></returns>
        private static Panel CreateListSelPagingBar(Int32 nPage, eList list)
        {
            Panel pnTitle = new Panel();
            pnTitle.CssClass = "paggingFilterWizardList";
            pnTitle.ID = "GetLstSelPagging";

            // Paging activé
            HtmlGenericControl divFirst = new HtmlGenericControl("div");
            divFirst.Attributes.Add("Class", "icon-edn-first disable");

            Int32 nbPage = list.NbPage;

            int pageDprt = nPage;

            if (nPage > 1)
            {
                divFirst.Attributes.Add("onclick", String.Concat("updateSelectionList(1);"));
                divFirst.Attributes.Add("class", "icon-edn-first fRight");
            }
            else
            {
                divFirst.Attributes.Add("class", "icon-edn-first fRight disable");
            }


            HtmlGenericControl divPrev = new HtmlGenericControl("div");

            divPrev.Attributes.Add("Class", "icon-edn-prev fLeft disable");

            if (nPage > 1)
            {
                divPrev.Attributes.Add("onclick", String.Concat("updateSelectionList(", nPage - 1, ", null, null, true);"));
                divPrev.Attributes.Add("class", "icon-edn-prev fRight");

            }
            else
            {
                divPrev.Attributes.Add("class", "icon-edn-prev fRight disable");
            }


            HtmlGenericControl divNumPage = new HtmlGenericControl("div");
            divNumPage.Attributes.Add("class", "numpagePlsPls");
            divNumPage.Attributes.Add("align", "center");


            HtmlInputText inptNumPage = new HtmlInputText();
            divNumPage.Controls.Add(inptNumPage);
            inptNumPage.Attributes.Add("onchange", String.Concat("updateSelectionList(this.value);"));
            inptNumPage.Attributes.Add("onkeydown", String.Concat("if(event.ctrlKey){updateSelectionList(this.value)};"));
            // ="" 
            inptNumPage.Attributes.Add("class", "pagInput");
            inptNumPage.Size = 2;
            inptNumPage.Value = pageDprt.ToString();


            HtmlGenericControl divTotalPage = new HtmlGenericControl("div");
            divTotalPage.Attributes.Add("class", "NbPagePlsPls");


            Panel TotalPage = new Panel();
            //   divTotalPage.Controls.Add(TotalPage);
            //divTotalPage.ID = String.Concat("tNP_", _tab);

            Literal liNbPage = new Literal();
            liNbPage.Text = String.Concat("&nbsp;/&nbsp;", nbPage);
            divTotalPage.Controls.Add(liNbPage);


            HtmlGenericControl divNext = new HtmlGenericControl("div");
            if (pageDprt < nbPage)
            {
                //divNext.Attributes.Add("onclick", String.Concat("NextPage(", _list.ViewMainTable.DescId.ToString(), ",", inptNumPage.ID, ");"));
                divNext.Attributes.Add("onclick", String.Concat("updateSelectionList(", pageDprt + 1, ", null, null, true);"));
                divNext.Attributes.Add("class", "icon-edn-next fRight");
            }
            else
            {
                divNext.Attributes.Add("class", "icon-edn-next fRight disable");
            }

            HtmlGenericControl divLast = new HtmlGenericControl("div");
            if (pageDprt < nbPage)
            {
                divLast.Attributes.Add("onclick", String.Concat("updateSelectionList(", nbPage, ", null, null, true);"));
                divLast.Attributes.Add("class", "icon-edn-last fRight");
            }
            else
            {
                divLast.Attributes.Add("class", "icon-edn-last fRight disable");
            }

            // rajouter pour savoir qu elle est la derniere page caché
            HtmlInputText inptNbPage = new HtmlInputText();
            inptNbPage.Attributes.Add("type", "hidden");
            //inptNbPage.ID = "nbP" + _tab; //Ne pas modifier tNP Utilisé dans un script eMAin.js
            inptNbPage.Value = nbPage.ToString();
            divLast.Controls.Add(inptNbPage);

            try
            {
                Panel pnSelFields = new Panel();
                pnSelFields.CssClass = "ivtFieldsSel icon-rubrique";

                pnSelFields.Attributes.Add("onclick", String.Concat("setSelectionCol(", ((eListFilteredSelection)list).CalledTabDescId.ToString(), ")"));
                pnTitle.Controls.Add(pnSelFields);
            }
            catch
            {
                throw;
            }

            // comme on est en float:right il faut ajouter les élément de droite à gauche
            pnTitle.Controls.Add(divLast);
            pnTitle.Controls.Add(divNext);
            pnTitle.Controls.Add(divTotalPage);
            pnTitle.Controls.Add(divNumPage);
            pnTitle.Controls.Add(divPrev);
            pnTitle.Controls.Add(divFirst);

            return pnTitle;
        }




        /// <summary>
        /// /// Javascript pour le wizard des selections d'invité
        /// </summary>
        /// <returns></returns>
        public override string GetInitJS()
        {
            String js = String.Concat(
            "   var oSelectionWizard;", Environment.NewLine,
            "   var iCurrentStep = 1;", Environment.NewLine,
            "   var htmlTemplate = null;", Environment.NewLine,
            "   var htmlHeader = null;", Environment.NewLine,
            "   var htmlFooter = null;", Environment.NewLine,
            "   var _activeFilter = 0;", Environment.NewLine,
            "   var iTotalSteps =", _nbStep, " ;", Environment.NewLine,

            "   var _ePopupVNEditor;", Environment.NewLine,
            "   var _eFilterNameEditor;", Environment.NewLine,
            "   var _eReportNameEditor;", Environment.NewLine,
            "   var _activeFilter;", Environment.NewLine,
            "   var _activeFilterTab;", Environment.NewLine,
            "   var _nSelectedFilter;", Environment.NewLine,
            
              "   var _eCurentSelectedFilter = null;", Environment.NewLine,
              "   var _aFilterWizarBtns ; ", Environment.NewLine,

              " var oIframe;", Environment.NewLine,
            "   function OnDocLoad() { ", Environment.NewLine,
            "       oSelectionWizard =  new eSelectionWizard(", _iTab, ",", _iTabSource, ");", Environment.NewLine,
            "       nGlobalActiveTab = ", EudoQuery.TableType.FILTER.GetHashCode(), ";", Environment.NewLine,
            // Initialisation de la carte
            //"       nsSelectionWizard.initMap('blockMap', 'Aia9V-TFKUb44CNZsVp_oxYGgszFUgksJal8-_IW1SSbodepQ4didGSMVp4UiSwR', '48.901865', '2.261006', '580', '570', 15); ", Environment.NewLine,
            "       Init('selection'); ", Environment.NewLine,
            "       oIframe = wizardIframe; ", Environment.NewLine,
            "}"

            );
            return js;
        }
    }
}