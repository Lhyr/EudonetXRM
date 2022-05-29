using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// administration des extensions : paramétrage propre à l'onglet
    /// </summary>
    public class eAdminExtensionTabParamRenderer : eAdminBlockRenderer
    {
        private eAdminExtensionTabParamRenderer(ePref pref, eAdminTableInfos tabInfos, string title, string titleInfo)
                 : base(pref, tabInfos, title, titleInfo, idBlock: "ExtensionsPart")
        {

        }

        public static eAdminExtensionTabParamRenderer CreateAdminExtensionRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            return new eAdminExtensionTabParamRenderer(pref, tabInfos, title, titleInfo);
        }

        protected override bool Init()
        {
            return base.Init();
        }

        protected override bool Build()
        {
            if (!base.Build())
                return false;

            _panelContent.CssClass = "paramPartContent";

            eAdminField button = eAdminButtonField.GetEAdminButtonField(
                param: new eAdminButtonParams()
                {
                    OnClick = $"javascript:nsAdmin.confTeamsMapping({_tabInfos.DescId});",
                    Label = eResApp.GetRes(Pref, 2993),
                    ID = "buttonExtensionParam",
                    ToolTip = ""
                }
                );

            button.Generate(_panelContent);

            return true;
        }

    }
}