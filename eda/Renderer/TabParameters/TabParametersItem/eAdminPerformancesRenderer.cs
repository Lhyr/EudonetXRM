using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPerformancesRenderer : eAdminBlockRenderer
    {

        public eAdminPerformancesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, "PerfPart")
        {

        }

        public static eAdminPerformancesRenderer CreateAdminPerfRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminPerformancesRenderer features = new eAdminPerformancesRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        /// <summary>Construction du bloc Performances</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            #region SLIDER
            // Valeur
            int sliderMax = 1;
            int sliderValue = 0;
            if (_tabInfos.CountOnDemand)
                sliderValue++;
            if (_tabInfos.BkmHideWhenEmpty)
                sliderValue++;
            if (_tabInfos.HistoByPassEnabled)
                sliderValue++;

            HtmlGenericControl slider = eAdminFieldBuilder.BuildRangeSlider(_panelContent, "perfSlider", 0, sliderMax, 1, sliderValue);
            #endregion

            #region Construction des cases à cocher
            eAdminFieldBuilder.BuildSeparator(_panelContent, eResApp.GetRes(Pref, 597));



            Panel subPart = new Panel();
            subPart.CssClass = "subPart";
            subPart.ID = "advOptions";
            subPart.Style.Add("display", "block");

            _panelContent.Controls.Add(subPart);

            HtmlGenericControl info = new HtmlGenericControl("p");
            info.Attributes.Add("class", "info");
            info.InnerText = eResApp.GetResWithColon(Pref, 7376);
            subPart.Controls.Add(info);


            eAdminField field = new eAdminCheckboxField(_tabInfos.DescId, eResApp.GetRes(Pref, 6839), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.COUNTONDEMAND.GetHashCode(), eResApp.GetRes(Pref, 6843),
                _tabInfos.CountOnDemand, onclick: "top.nsAdmin.onCheckboxClick(this);top.nsAdmin.updateSliderValue()");
            field.Generate(subPart);

            if (_tabInfos.TabType != TableType.PM && _tabInfos.TabType != TableType.PP)
            {
                // Ne doit s'appliquer que si on n'est ni sur PP ni sur PM ni sur ADR
                if (_tabInfos.TabType != TableType.ADR)
                {
                    sliderMax++;

                    field = new eAdminCheckboxField(_tabInfos.DescId, eResApp.GetRes(Pref, 6840),
                    eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.BKMHIDEWHENEMPTY.GetHashCode(), eResApp.GetRes(Pref, 6844), _tabInfos.BkmHideWhenEmpty,
                    onclick: "top.nsAdmin.onCheckboxClick(this);top.nsAdmin.updateSliderValue()");
                    field.Generate(subPart);

                }

                // Ne doit s'appliquer que si Historique est activé et si on n'est ni sur PP ni sur PM
                //if (_tabInfos.HistoDescId != 0)
                //{
                //    sliderMax++;

                //    field = new eAdminCheckboxField(_tabInfos.DescId, eResApp.GetRes(_ePref, 6841),
                //    eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.HISTOBYPASSENABLED.GetHashCode(), eResApp.GetRes(_ePref, 6845),
                //    (_tabInfos.HistoByPassEnabled),
                //   onclick: "top.nsAdmin.onCheckboxClick(this);top.nsAdmin.updateSliderValue()");
                //    field.Generate(subPart);
                //}

            }



            #endregion

            // Met à jour la valeur max du slider par rapport au nombre d'options affichées
            slider.Attributes["data-max"] = sliderMax.ToString();
            return true;
        }
    }
}