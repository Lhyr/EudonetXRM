using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParametersLogoNameRenderer : eAdminModuleRenderer
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, String> _configs;

        private eAdminParametersLogoNameRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs) : base(pref)
        {
            _configs = configs;        }

        public static eAdminParametersLogoNameRenderer CreateAdminParametersLogoNameRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs)
        {
            eAdminParametersLogoNameRenderer rdr = new eAdminParametersLogoNameRenderer(pref, configs);
            rdr.Generate();
            return rdr;
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL_LOGO.ToString(), eResApp.GetRes(Pref, 7734));
                PgContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

                String logopath = (String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.LOGONAME])) ? "themes/default/images/emain_logo.png" : String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.CUSTOM, Pref.GetBaseName), @"/", _configs[eLibConst.CONFIG_DEFAULT.LOGONAME]);

                Panel logoField = new Panel();
                logoField.ID = "fieldLogo";

                HtmlImage logoImage = new HtmlImage();
                logoImage.ID = "imgLogo";
                logoImage.Src = logopath;
                logoImage.Attributes.Add("onclick", "doGetImage(this, 'LOGO');");
  
                HtmlGenericControl info = new HtmlGenericControl();
                info.InnerText = eResApp.GetRes(Pref, 7735);
                logoField.Controls.Add(logoImage);
                logoField.Controls.Add(info);
                targetPanel.Controls.Add(GetLabelField(eResApp.GetRes(Pref, 333), logoField));

                return true;
            }

            return false;

        }
    }
}