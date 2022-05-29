using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu du widget de type Liste, avec entête
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eRenderer" />
    public class eListWidgetRenderer : eWidgetRenderer
    {
        int _filterID = 0;
        int _nbRows = 0;
        string _listCol = string.Empty;
        bool _showHeader = true;
        List<eListWidget.ExpressFilterInfo> _filterInfo = null;
        bool _histo = true;

        private eListWidgetRenderer(ePref pref, int widgetID, int tab, List<eListWidget.ExpressFilterInfo> filterInfo, bool histo, eXrmWidgetContext widgetContext)
            : base(pref, widgetID, widgetContext)
        {
            _tab = tab;
            _filterInfo = filterInfo;
            _histo = histo;
        }

        /// <summary>
        /// Creates the list widget renderer.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="widgetID">The widget identifier.</param>
        /// <param name="tab">The list tab.</param>
        /// <param name="filterInfo">The filter information.</param>
        /// <param name="histo">if set to <c>true</c> [histo].</param>
        /// <param name="widgetContext">The widget context.</param>
        /// <returns></returns>
        public static eListWidgetRenderer CreateListWidgetRenderer(ePref pref, int widgetID, int tab, List<eListWidget.ExpressFilterInfo> filterInfo = null, bool histo = true, eXrmWidgetContext widgetContext = null)
        {
            eListWidgetRenderer rdr = new eListWidgetRenderer(pref, widgetID, tab, filterInfo, histo, widgetContext);
            return rdr;
        }

        /// <summary>
        /// Appel l'objet métier
        /// eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected override bool Init()
        {
            if (base.Init())
            {
                try
                {
                    _filterID = _widgetParams.GetParamValueInt("filterid");
                    _nbRows = _widgetParams.GetParamValueInt("nbrows");
                    _listCol = _widgetParams.GetParamValue("listcol");
                    _showHeader = !(_widgetParams.GetParamValue("showHeader") == "0");

                    return true;
                }
                catch (Exception exc)
                {
                    _eException = exc;
                    _sErrorMsg = exc.Message;
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            string tableLabel = string.Empty;
            string filterLabel = string.Empty;
            string filterName = string.Empty;
            string filterDescription = string.Empty;

            // Champs cachés
            _divHidden = new HtmlGenericControl("div");
            HtmlInputHidden hidden = new HtmlInputHidden();
            hidden.ID = "hidWid";
            hidden.Value = _widgetID.ToString();
            _divHidden.Controls.Add(hidden);
            _pgContainer.Controls.Add(_divHidden);

            _widgetContext.WidgetId = _widgetID;

            if (this._tab != 0)
            {
                // demande 90086 : réinitialisation de pref
                Pref.Context.Paging = new PagingInfo();
                eListFilesXrmWidgetRenderer rdr = eListFilesXrmWidgetRenderer.GetListFilesXrmWidgetRenderer(this.Pref, 500, 500, _tab, 1, _nbRows, _listCol, _filterID, _filterInfo, _histo, _widgetContext);
                rdr.Generate();

                HtmlGenericControl wrapper = new HtmlGenericControl("div");
                wrapper.ID = "listContent";
                wrapper.Attributes.Add("class", "listWidgetContainer");
                _pgContainer.Controls.Add(wrapper);

                if (_showHeader)
                {
                    tableLabel = eLibTools.GetResSingle(this.Pref, _tab, this.Pref.Lang, out _sErrorMsg);

                    #region Header
                    Panel header = new Panel();
                    header.CssClass = "listWidgetHeader";


                    if (_filterID > 0)
                    {
                        filterDescription = AdvFilter.GetDescription(this.Pref, _filterID, out filterName, out _sErrorMsg);

                        filterLabel = (_filterID > 0) ? String.Concat(" - ", filterName) : filterLabel;
                    }


                    // Nb de fiches
                    Panel counter = new Panel();
                    counter.CssClass = "listNbRows";

                    if (_filterInfo != null && _filterInfo.Count > 0)
                    {
                        HtmlGenericControl iconFilter = new HtmlGenericControl();
                        iconFilter.ID = "iconeFilter";
                        iconFilter.Attributes.Add("class", "icon-list_filter");
                        iconFilter.Attributes.Add("onmouseover", $"stfilter(event, {_tab});");
                        iconFilter.Attributes.Add("data-wfilter", "1");
                        counter.Controls.Add(iconFilter);
                    }

                    bool bHasMoreRows = rdr.FileTotalRows > rdr.DisplayedTotalRows;

                    HtmlGenericControl spanLabel = new HtmlGenericControl();
                    spanLabel.InnerText = String.Concat(rdr.DisplayedTotalRows, (bHasMoreRows ? "+" : ""), " ", tableLabel, filterLabel);
                    counter.Controls.Add(spanLabel);

                    // Affichage de la description du filtre s'il y a un filtre
                    if (_filterID > 0)
                    {
                        eTools.DisplayFilterTooltip(spanLabel, filterDescription);
                    }


                    header.Controls.Add(counter);
                    wrapper.Controls.Add(header);

                    Panel buttonLine = new Panel();
                    buttonLine.CssClass = "listWidgetHeader";

                    // Accéder à la table, avec le filtre s'il y en a
                    Panel button = new Panel();
                    button.CssClass = "advFltMenu";
                    button.Style.Add("float", "left");
                    button.Attributes.Add("onclick", "top.goTabListWithFilter(" + _tab + ", " + _filterID + ")");
                    button.ToolTip = eResApp.GetRes(this.Pref, 7391);
                    HtmlGenericControl spanButton = new HtmlGenericControl();
                    spanButton.InnerText = tableLabel;
                    if (bHasMoreRows) {
                        int nResourceId = rdr.TotalRowsAvailable ? 3018 : 3024; // "Voir les <TOTAL> résultats de l'onglet <NOM DE L'ONGLET>" / "Voir tous les résultats de l'onglet <NOM DE L'ONGLET>"
                        spanButton.InnerText = eResApp.GetRes(this.Pref, nResourceId).Replace("<TABLABEL>", tableLabel).Replace("<TOTALROWS>", rdr.FileTotalRows.ToString());
                    }
                    button.Controls.Add(spanButton);
                    buttonLine.Controls.Add(button);

                    wrapper.Controls.Add(buttonLine);
                    #endregion
                }




                //rdr.CreatePagingBar(wrapper);
                wrapper.Controls.Add(rdr.PgContainer);
            }

            return true;
        }
    }
}