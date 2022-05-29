using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBlockSecurityRenderer : eAdminBlockRenderer
    {
        /// <summary>
        /// Descid de l'élément concerné (tab/field)
        /// </summary>
        protected Int32 _descid;

        protected eAdminBlockSecurityRenderer(ePref pref, eAdminTableInfos tabInfos)
            : base(pref, tabInfos, title: eResApp.GetRes(pref, 206), titleInfo: "", idBlock: "SecurityPart")
        {
            _descid = tabInfos.DescId;
        }

        protected eAdminBlockSecurityRenderer(ePref pref, eAdminFieldInfos fieldInfos)
             : base(pref, fieldInfos.Table, title: eResApp.GetRes(pref, 206), titleInfo: "", idBlock: "SecurityPart")
        {
            _descid = fieldInfos.DescId;
        }

        public static eAdminBlockSecurityRenderer CreateAdminBlockSecurityRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            eAdminBlockSecurityRenderer features = new eAdminBlockSecurityRenderer(pref, tabInfos);
            return features;
        }
        public static eAdminBlockSecurityRenderer CreateAdminBlockSecurityRenderer(ePref pref, eAdminFieldInfos fieldInfos)
        {
            eAdminBlockSecurityRenderer features = new eAdminBlockSecurityRenderer(pref, fieldInfos);
            return features;
        }

        protected override bool Build()
        {
            base.Build();

            //Recupere la case a cocher et rend la textbox inactive si nécessaire
            bool bUnlimitedLifeTime = _tabInfos.DefaultLinkLifeTime == -1;

            List<ListItem> items = new List<ListItem>();
            items.Add(new ListItem(eResApp.GetRes(Pref, 8249), "-1"));
            items.Add(new ListItem(eResApp.GetRes(Pref, 8242), "0"));

            string valueDDL = String.Empty;
            if (_tabInfos.DefaultLinkLifeTime >= 0)
                valueDDL = "0";
            else
                valueDDL = "-1";

            eAdminDropdownField ddlPjLifeTime = new eAdminDropdownField(descid: 0,
              label: eResApp.GetRes(Pref, 8235),
              customLabelCSSClasses: "info",
              propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
              propCode: DESCADV_PARAMETER.DEFAULT_PJ_LIFETIME.GetHashCode(),
              items: items.ToArray(),
              renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
              onChange: "top.nsAdmin.disableOnSelectedValue(this, '-1','tbxPjDuration');",
              valueFormat: FieldFormat.TYP_NUMERIC,
              value: valueDDL,
              bNoUpdate: true);

            ddlPjLifeTime.SetFieldControlID("ddlPjLifeTime");
            ddlPjLifeTime.Generate(_panelContent);

            Dictionary<string, string> customPanelAttributes = new Dictionary<string, string>();
            if (bUnlimitedLifeTime)
                customPanelAttributes.Add("display", "none");

            eAdminField tbxPjDuration = new eAdminTextboxField(_tabInfos.DescId, eResApp.GetResWithColon(Pref, 8229), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.DEFAULT_PJ_LIFETIME.GetHashCode()
               , AdminFieldType.ADM_TYPE_NUM, eResApp.GetRes(Pref, 8228), value: _tabInfos.DefaultLinkLifeTime.ToString(), readOnly: bUnlimitedLifeTime, customPanelStyleAttributes : customPanelAttributes);
            tbxPjDuration.SetFieldControlID("tbxPjDuration");
            tbxPjDuration.Generate(_panelContent);

            return true;
        }


    }
}