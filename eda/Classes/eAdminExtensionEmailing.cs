using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionEmailing : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIG_DEFAULT, string> dicConfig;

        [JsonConstructor]
        public eAdminExtensionEmailing(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionEmailing(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING)
        {
            dicConfig = pref.GetConfigDefault(
                new HashSet<eLibConst.CONFIG_DEFAULT> {
                        eLibConst.CONFIG_DEFAULT.HIDEMAILREPORT,
                        eLibConst.CONFIG_DEFAULT.EMBEDDEDIMAGESENABLED,
                        eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGAUTHENTICATION,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGUSERNAME,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPASSWORD,
                        eLibConst.CONFIG_DEFAULT.ANTISPOOFINGENABLED,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPORT
                });
            _IsUninstallable = false;
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = String.Empty;
            return true;
        }

        /// <summary>
        /// Extension Email a des paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Active l'extension
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            sError = String.Empty;

            return true;
        }

        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            sError = String.Empty;

            return true;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            HtmlGenericControl idDiv, idLabel, idValue;

            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();

            #region Paramètres par défaut ou personnalisés
            idDiv = new HtmlGenericControl("div");

            idLabel = new HtmlGenericControl("span");
            idLabel.Attributes.Add("class", "extensionInfoLabel");
            idLabel.InnerHtml = String.Concat(eResApp.GetRes(_pref, 7911), " : ");
            idDiv.Controls.Add(idLabel);

            idValue = new HtmlGenericControl("span");
            idValue.Attributes.Add("class", "extensionInfoValue");
            idValue.InnerHtml = (dicConfig[eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM] == "1") ? eResApp.GetRes(_pref, 1417) : eResApp.GetRes(_pref, 6722);
            idDiv.Controls.Add(idValue);

            infoList.Add(idDiv);
            #endregion

            #region Serveur SMTP

            if (dicConfig[eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM] != "1")
            {
                idDiv = new HtmlGenericControl("div");

                idLabel = new HtmlGenericControl("span");
                idLabel.Attributes.Add("class", "extensionInfoLabel");
                idLabel.InnerHtml = String.Concat(eResApp.GetRes(_pref, 574), " : ");
                idDiv.Controls.Add(idLabel);

                idValue = new HtmlGenericControl("span");
                idValue.Attributes.Add("class", "extensionInfoValue");
                idValue.InnerHtml = (dicConfig[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME]);
                idDiv.Controls.Add(idValue);

                infoList.Add(idDiv);
            }

            #endregion

            return infoList;
        }

    }
}