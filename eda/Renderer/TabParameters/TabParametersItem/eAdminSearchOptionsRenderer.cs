using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminSearchOptionsRenderer : eAdminBlockRenderer
    {
        #region propriétés
         Dictionary<int, string> _advancedSearchFields;
        #endregion

        private eAdminSearchOptionsRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "SearchOptionsPart")
        {

        }

        public static eAdminSearchOptionsRenderer CreateAdminSearchOptionsRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminSearchOptionsRenderer features = new eAdminSearchOptionsRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        protected override bool Init()
        {
            base.Init();

            _advancedSearchFields = _tabInfos.GetAdvancedSearchFields(Pref);

            return true;
        }

        protected override bool Build()
        {
            base.Build();

            if (_tabInfos.TabType == TableType.PP || _tabInfos.TabType == TableType.PM || _tabInfos.TabType == TableType.EVENT)
            {
                #region Construction des Liens
                // administrer le nombre de caractères
                string searchLimitValue = _tabInfos.SearchLimit >= 0 ? _tabInfos.SearchLimit.ToString() : String.Empty;
                Dictionary<string, string> customPanelStyleAttributes = new Dictionary<string, string>();
                customPanelStyleAttributes.Add("margin-bottom", "0px");
                Dictionary<string, string> customTextboxStyleAttributes = new Dictionary<string, string>();
                customTextboxStyleAttributes.Add("width", "40px");
                eAdminTextboxField txtSearchLimit = new eAdminTextboxField(
                    _tabInfos.DescId, "",
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.SEARCHLIMIT.GetHashCode(), AdminFieldType.ADM_TYPE_NUM,
                    tooltiptext: eResApp.GetRes(Pref, 7112), value: searchLimitValue,
                    customPanelStyleAttributes: customPanelStyleAttributes,
                    customTextboxStyleAttributes: customTextboxStyleAttributes, customPanelCSSClasses: "fieldInline",
                    prefixText: eResApp.GetRes(Pref, 7113), suffixText: eResApp.GetRes(Pref, 1461));
                txtSearchLimit.SetFieldControlID("txtSearchLimit");
                txtSearchLimit.IsOptional = true;
                txtSearchLimit.Generate(_panelContent);


                // administrer le Champs de recherche Complémentaires               
                List<ListItem> lisItems = new List<ListItem>();
                lisItems.Add(new ListItem(eResApp.GetRes(Pref, 248), "0"));
                foreach (KeyValuePair<int, string> kvp in _advancedSearchFields)
                {
                    lisItems.Add(new ListItem(kvp.Value, kvp.Key.ToString()));
                }

                eAdminDropdownField ddlAdvancedSearch = new eAdminDropdownField(_tabInfos.DescId, eResApp.GetResWithColon(Pref, 7102), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.ADVANCEDSEARCHDESCID.GetHashCode(), lisItems.ToArray(), tooltiptext: eResApp.GetRes(Pref, 7103).Replace("<MAINFIELD>", _tabInfos.MainFieldLabel), value: _tabInfos.AdvancedSearchDescId, renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
                ddlAdvancedSearch.IsOptional = true;
                ddlAdvancedSearch.Generate(_panelContent);

                // Recherche avancée
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 983), "buttonAdvSearch", tooltiptext: eResApp.GetRes(Pref, 7111), onclick: "nsAdmin.showAdvSearchPopup()");
                button.Generate(_panelContent);
            }
            #endregion

            return true;
        }
    }
}