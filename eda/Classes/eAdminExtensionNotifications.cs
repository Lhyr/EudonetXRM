using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    internal class eAdminExtensionNotifications : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        [JsonConstructor]
        public eAdminExtensionNotifications(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionNotifications(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS)
        {
            dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.NOTIFICATION_ENABLED,
                        eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED
                });
        }

        protected override void Init()
        {
            this._feature = eConst.XrmExtension.Notifications;
            base.Init();
        }

        /// <summary>
        /// Indique si l'intégralité du mode Fiche de l'extension (notamment l'onglet Paramètres) doit être rafraîchi après activation/désactivation
        /// </summary>
        public override bool NeedsFullRefreshAfterEnable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                // Pour le moment, seul le paramètre "Debug" est proposé dans les paramètres. On affiche donc uniquement l'onglet si on est en compilation Debug
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bEnabled = false;
            sError = String.Empty;

            try
            {
                bEnabled = dicConfigAdv[eLibConst.CONFIGADV.NOTIFICATION_ENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bEnabled;
        }

        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";
                eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.NOTIFICATION_ENABLED, sEnable, eLibConst.CONFIGADV_CATEGORY.UNDEFINED);
                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                dicConfigAdv[eLibConst.CONFIGADV.NOTIFICATION_ENABLED] = sEnable;

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();

            // Notifications activées
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetResWithColon(Pref, 5163), " "); // Statut - TOCHECKRES
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.NOTIFICATION_ENABLED] == "1" ? eResApp.GetRes(Pref, 1460) : eResApp.GetRes(Pref, 1459);
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);


            // Mode Debug
#if DEBUG
            spanInfo = new HtmlGenericControl("span");
            spanInfoLabel = new HtmlGenericControl("span");
            spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = "Debug : "; // TOCHECKRES - TODORES
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED] == "1" ? eResApp.GetRes(Pref, 1460) : eResApp.GetRes(Pref, 1459);
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
#endif

            return infoList;
        }
    }
}