using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu du Kanban
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eKanbanRenderer : eWidgetRenderer
    {
        #region Propriétés privées
        eKanban _kb;
        /// <summary>
        /// Rubrique de ligne de couloir sélectionnée
        /// </summary>
        int _selectedSLDescid = 0;
        /// <summary>
        /// La rubrique de ligne de couloir sélectionnée est-elle un groupe ?
        /// </summary>
        bool _selectedSLGroup = false;
        /// <summary>
        /// Infos sur la rubrique de ligne de couloir sélectionnée
        /// </summary>
        eKanbanSwimlane _selectedSL = null;
        /// <summary>
        /// Nombre de cartes affichées
        /// </summary>
        int _nbCards = 0;
        /// <summary>
        /// The span nb elements
        /// </summary>
        HtmlGenericControl _spanNbElements;
        #endregion

        #region Accesseurs        
        /// <summary>
        /// Descid de la table associée au Kanban
        /// </summary>
        public int Tab
        {
            get { return _tab; }
        }

        /// <summary>
        /// Ligne de couloir sélectionnée
        /// </summary>
        public eKanbanSwimlane SelectedSL
        {
            get
            {
                return _selectedSL;
            }

            set
            {
                _selectedSL = value;
            }
        }
        #endregion

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="widgetID">ID du widget</param>
        /// <param name="context">The widget context.</param>
        /// <param name="selectedSL">Descid de la rubrique de ligne de couloir sélectionnée</param>
        /// <param name="slIsGroup">if set to <c>true</c> [sl is group].</param>
        private eKanbanRenderer(ePref pref, int widgetID, eXrmWidgetContext context, int selectedSL = 0, bool slIsGroup = false) : base(pref, widgetID, context)
        {
            _widgetID = widgetID;
            _selectedSLDescid = selectedSL;
            _selectedSLGroup = slIsGroup;
        }

        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override bool Init()
        {
            try
            {

                _kb = new eKanban(this.Pref, _widgetID, _widgetContext, _selectedSLDescid, _selectedSLGroup);
                if (!_kb.Load())
                {
                    if (_kb.ErrorCode == eKanban.ErrorType.LimitExceeded)
                    {
                        CreateLimitExceededErrorPage();
                    }
                    else
                    {
                        _pgContainer.Controls.Add(new LiteralControl(_kb.ErrorMessage));
                    }

                    return false;
                }
                _tab = _kb.Tab;
                _selectedSL = _kb.SelectedSL;

                return true;
            }
            catch (Exception exc)
            {
                _eException = exc;
                _sErrorMsg = exc.Message;
                return false;
            }

        }

        private void CreateLimitExceededErrorPage()
        {
            Panel wrapper = new Panel();
            wrapper.ID = "kbContent";
            wrapper.CssClass = "panelError";

            HtmlImage img = new HtmlImage();
            img.Src = "themes/default/images/oops-kanban.png";
            wrapper.Controls.Add(img);

            HtmlGenericControl error = new HtmlGenericControl("p");
            error.InnerHtml = _kb.ErrorMessage;
            wrapper.Controls.Add(error);


            _pgContainer.Controls.Add(wrapper);
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {


            _pgContainer.Controls.Add(CreateHeader());

            Panel pBody = new Panel();
            pBody.ID = "kbBody";
            _pgContainer.Controls.Add(pBody);

            //if (_kb.Records.Count > 0)
            //{
            pBody.Controls.Add(CreateColumnsHeader());
            pBody.Controls.Add(CreateContent());

            _spanNbElements.InnerText = _nbCards.ToString();
            //}


            return true;
        }


        /// <summary>
        /// Création des zones permettant de scroller 
        /// </summary>
        private void CreateScrollZones(Panel container)
        {
            Panel pArrow = new Panel();
            pArrow.ID = "scrollLeft";
            pArrow.CssClass = "scrollZoneH";
            container.Controls.Add(pArrow);

            pArrow = new Panel();
            pArrow.ID = "scrollRight";
            pArrow.CssClass = "scrollZoneH";
            container.Controls.Add(pArrow);

            //pArrow = new Panel();
            //pArrow.ID = "scrollUp";
            //pArrow.CssClass = "scrollZone icon-caret-up";
            //_pgContainer.Controls.Add(pArrow);

            //pArrow = new Panel();
            //pArrow.ID = "scrollDown";
            //pArrow.CssClass = "scrollZone icon-caret-down";
            //_pgContainer.Controls.Add(pArrow);
        }


        /// <summary>
        /// Création du bloc d'entête
        /// </summary>
        /// <returns></returns>
        private Panel CreateHeader()
        {
            HtmlGenericControl icon;
            string selectedSLLabel = eResApp.GetRes(this.Pref, 8471);

            Panel pHeader = new Panel();
            pHeader.ID = "kbHeader";

            #region Choix des lignes de couloir
            if (_kb.SlList.Count > 0)
            {
                Panel pButton = new Panel();
                pButton.ID = "kbButtonSL";
                pButton.CssClass = "kbButtonSL";


                #region Options
                HtmlGenericControl ulMenu = new HtmlGenericControl("ul");
                ulMenu.Attributes.Add("class", "kbListSL");
                ulMenu.ID = "kbListSL";

                HtmlGenericControl liMenu;
                foreach (eKanbanSwimlane sl in _kb.SlList)
                {
                    if (_selectedSL != null
                        && (_selectedSL.Descid == sl.Descid && _selectedSL.Settings.IsGroup == sl.Settings.IsGroup))
                    {
                        selectedSLLabel = sl.SlFieldLabel;
                    }
                    else
                    {
                        liMenu = new HtmlGenericControl("li");
                        liMenu.Attributes.Add("data-sldid", sl.Descid.ToString());
                        liMenu.Attributes.Add("data-usergroup", sl.Settings.IsGroup ? "1" : "0");
                        liMenu.InnerText = sl.SlFieldLabel;
                        ulMenu.Controls.Add(liMenu);
                    }
                }
                // Option "Désactivé"
                liMenu = new HtmlGenericControl("li");
                liMenu.ID = "optDisable";
                liMenu.Attributes.Add("data-sldid", "-1");
                icon = new HtmlGenericControl();
                icon.Attributes.Add("class", "icon-undo");
                liMenu.Controls.Add(icon);
                HtmlGenericControl opt = new HtmlGenericControl();

                opt.InnerText = eResApp.GetRes(this.Pref, 690);
                liMenu.Controls.Add(opt);
                ulMenu.Controls.Add(liMenu);

                pButton.Controls.Add(ulMenu);
                #endregion

                // Icône
                icon = new HtmlGenericControl();
                icon.Attributes.Add("class", "icon-bars");
                pButton.Controls.Add(icon);

                // Libellé "Ligne de couloir"
                HtmlGenericControl slLabel = new HtmlGenericControl();
                slLabel.ID = "slLabel";
                slLabel.InnerText = selectedSLLabel;
                if (_selectedSL != null)
                {
                    slLabel.Attributes.Add("data-sldid", _selectedSL.Descid.ToString());
                    slLabel.Attributes.Add("data-usergroup", _selectedSL.Settings.IsGroup ? "1" : "0");
                }
                pButton.Controls.Add(slLabel);
                pHeader.Controls.Add(pButton);
            }

            #endregion

            #region Filtre
            Panel pFilter = new Panel();
            pFilter.ID = "tabInfos";

            // Pas d'icône si pas de filtre
            if (_kb.FilterID > 0)
            {
                icon = new HtmlGenericControl();
                icon.ID = "iconeFilter";
                icon.Attributes.Add("class", "icon-list_filter");
                icon.Attributes.Add("onmouseover", "stfilter(event);");
                eTools.DisplayFilterTooltip(icon, _kb.FilterDescription);
                pFilter.Controls.Add(icon);
            }


            Panel label = new Panel();
            label.CssClass = "lib";
            _spanNbElements = new HtmlGenericControl();
            _spanNbElements.ID = "SpanNbElem";
            label.Controls.Add(_spanNbElements);

            HtmlGenericControl element = new HtmlGenericControl();
            element.ID = "SpanLibElem";
            element.InnerText = _kb.FilterName;
            eTools.DisplayFilterTooltip(element, _kb.FilterDescription);
            label.Controls.Add(element);

            pFilter.Controls.Add(label);

            pHeader.Controls.Add(pFilter);
            #endregion

            return pHeader;
        }

        /// <summary>
        /// Création de l'entête des colonnes
        /// </summary>
        /// <returns></returns>
        private Panel CreateColumnsHeader()
        {
            Panel pCol;
            HtmlGenericControl ulAggr, liAggr;
            HtmlGenericControl spanLabel;
            Panel pHeader = new Panel();
            pHeader.ID = "kbColHeader";

            foreach (eKanbanColumn cv in _kb.Columns)
            {
                pCol = new Panel();
                pCol.CssClass = "kbHeaderCol";
                pCol.Attributes.Add("data-id", cv.Id.ToString());

                spanLabel = new HtmlGenericControl();
                spanLabel.InnerText = cv.Value;
                pCol.Controls.Add(spanLabel);

                // Aggrégats
                ulAggr = new HtmlGenericControl("ul");
                ulAggr.Attributes.Add("class", "kbAggr");
                pCol.Controls.Add(ulAggr);
                foreach (eKanbanColumnAggregate aggr in cv.AggrData)
                {
                    liAggr = new HtmlGenericControl("li");
                    liAggr.InnerText = aggr.ToString();
                    ulAggr.Controls.Add(liAggr);
                }

                pHeader.Controls.Add(pCol);

            }

            return pHeader;
        }

        /// <summary>
        /// Création de l'ensemble des colonnes (lignes de couloir incluses)
        /// </summary>
        /// <returns></returns>
        private Panel CreateContent()
        {
            Panel pColumns, pSLBlock;
            Panel pContent = new Panel();
            pContent.ID = "kbContent";

            //if (_kb.Records.Count > 0)
            //{
            bool blockOpened = true; // TODO

            CreateScrollZones(pContent);

            pColumns = new Panel();
            pColumns.ID = "kbColumns";

            if (_kb.SlGroups.Count > 0)
            {
                foreach (eKanbanSwimlaneGroup group in _kb.SlGroups)
                {
                    pSLBlock = new Panel();
                    pSLBlock.CssClass = "kbSLBlock";

                    blockOpened = group.Records.Count > 0;

                    pSLBlock.Controls.Add(CreateSwimlane(group.IdValue, group.Label, blockOpened));

                    pSLBlock.Controls.Add(CreateColumns(group.IdValue, group.Records, blockOpened));

                    pColumns.Controls.Add(pSLBlock);
                }
            }
            else
            {
                pColumns.Controls.Add(CreateColumns(records: _kb.Records, blockOpened: blockOpened));
            }

            pContent.Controls.Add(pColumns);
            //}
            //else
            //{
            //    pContent.Controls.Add(new LiteralControl(eResApp.GetRes(this.Pref, 7612)));
            //}


            //pContent.Controls.Add(CreateScrollDown());

            return pContent;
        }

        /// <summary>
        /// Création d'un bloc de colonnes
        /// </summary>
        /// <param name="idSL">ID de la valeur de la ligne de couloir</param>
        /// <param name="records">The records.</param>
        /// <param name="blockOpened">if set to <c>true</c> [block opened].</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        private Panel CreateColumns(string idSL = "-1", List<eRecord> records = null, bool blockOpened = true)
        {
            Panel pCol, pColumnsContainer;
            List<eRecord> filteredRecords = null;
            eKanbanCardRenderer rdr = null;
            eFieldRecord columnField, slField;

            pColumnsContainer = new Panel();
            pColumnsContainer.CssClass = "kbColumnsContainer";
            pColumnsContainer.Attributes.Add("data-slid", idSL);
            pColumnsContainer.Attributes.Add("data-active", (blockOpened) ? "1" : "0");

            string colAlias = String.Concat(_kb.Tab, "_", _kb.ColFieldDescid);

            //bool bUpdatable = (_kb.ColField?.RightIsUpdatable ?? true) && (_kb.SlField?.RightIsUpdatable ?? true);

            foreach (eKanbanColumn cv in _kb.Columns)
            {
                pCol = new Panel();
                pCol.CssClass = "kbCol";

                // Attributs indiquant la colonne et la ligne de couloir
                pCol.Attributes.Add("data-col", cv.Id.ToString());
                pCol.Attributes.Add("data-setcol", String.Concat(_kb.ColFieldDescid, "|", cv.Id));
                pCol.Attributes.Add("data-sl", idSL);
                pCol.Attributes.Add("data-setsl", String.Concat(_kb.SlFieldDescid, "|", idSL));

                filteredRecords = records.FindAll(r => eLibTools.GetNum(r.GetFieldByAlias(colAlias)?.Value) == cv.Id);

                if (filteredRecords != null)
                {
                    foreach (eRecord r in filteredRecords)
                    {
                        rdr = eKanbanCardRenderer.CreateKanbanCardRenderer(this.Pref, _kb.Tab, _kb.CardParams, r, draggable: true);
                        rdr.Generate();
                        if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                        {
                            throw new Exception(rdr.ErrorMsg);
                        }

                        _nbCards++;

                        #region Ajout des attributs sur les droits de la rubrique en colonne
                        columnField = r.GetFieldByAlias(String.Concat(_tab, "_", _kb.ColFieldDescid));
                        if (columnField != null)
                        {
                            if (columnField.IsMandatory)
                                rdr.PgContainer.Attributes.Add("data-colrequired", "1");

                            if (!columnField.RightIsUpdatable)
                                rdr.PgContainer.Attributes.Add("data-colro", "1");
                        }
                        #endregion

                        #region Ajout des attributs sur les droits de la rubrique en ligne de couloir
                        if (_kb.SlFieldDescid > 0)
                        {
                            slField = r.GetFieldByAlias(String.Concat(_tab, "_", _kb.SlFieldDescid));
                            if (slField != null)
                            {
                                if (slField.IsMandatory)
                                    rdr.PgContainer.Attributes.Add("data-slrequired", "1");

                                if (!slField.RightIsUpdatable)
                                    rdr.PgContainer.Attributes.Add("data-slro", "1");
                            }
                        }

                        #endregion

                        pCol.Controls.Add(rdr.PgContainer);
                    }
                }


                pColumnsContainer.Controls.Add(pCol);
            }
            return pColumnsContainer;
        }

        /// <summary>
        /// Création de la ligne de couloir
        /// </summary>
        /// <param name="id">ID de la valeur</param>
        /// <param name="value">Valeur</param>
        /// <param name="blockOpened">Bloc ouvert</param>
        /// <returns></returns>
        private Panel CreateSwimlane(string id, string value, bool blockOpened = true)
        {
            Panel pLane = new Panel();
            pLane.CssClass = "kbSL";

            pLane.Attributes.Add("data-slid", id);
            pLane.Attributes.Add("data-active", (blockOpened ? "1" : "0"));

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", blockOpened ? "icon-caret-down" : "icon-caret-right");
            pLane.Controls.Add(icon);

            HtmlGenericControl text = new HtmlGenericControl();
            text.InnerText = value;
            pLane.Controls.Add(text);

            return pLane;
        }



        #region Méthodes statiques        
        /// <summary>
        /// Creates the kanban renderer.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="widgetID">The widget identifier.</param>
        /// <param name="context">The context.</param>
        /// <param name="selectedSL">The selected swimlane</param>
        /// <param name="slIsGroup">if set to <c>true</c> [swimlane is group].</param>
        /// <returns></returns>
        public static eKanbanRenderer CreateKanbanRenderer(ePref pref, int widgetID, eXrmWidgetContext context, int selectedSL = 0, bool slIsGroup = false)
        {
            eKanbanRenderer rdr = new eKanbanRenderer(pref, widgetID, context, selectedSL, slIsGroup);
            return rdr;
        }
        #endregion
    }
}