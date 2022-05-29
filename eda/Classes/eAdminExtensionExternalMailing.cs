using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    internal class eAdminExtensionExternalMailing : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        [JsonConstructor]
        public eAdminExtensionExternalMailing(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionExternalMailing(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.MAILINGSENDTYPE,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPLOGIN,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPPASSWORD,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPPORT,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPSSL,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPURL,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPUSEPASSIVEMODE,
                        eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN,
                        eLibConst.CONFIGADV.EUDOMAILING_SENDOUTSPEED
                    });
            }
            catch (Exception e)
            {
                dicConfigAdv = null;
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            base.InitConfig(pref, out sError);

            return dicConfigAdv != null;
        }

        /// <summary>
        /// Indique si l'intégralité du mode Fiche de l'extension (notamment l'onglet Paramètres) doit être rafraîchi après activation/désactivation
        /// </summary>
        public override bool NeedsFullRefreshAfterEnable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Indique si l'extension comporte des paramètres à afficher dans l'onglet Paramètres
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
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


            bool bTeradataEnabled = false;
            sError = String.Empty;

            try
            {
                bTeradataEnabled = dicConfigAdv[eLibConst.CONFIGADV.MAILINGSENDTYPE] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bTeradataEnabled;

        }

        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";
               // eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.MAILINGSENDTYPE, sEnable, eLibConst.CONFIGADV_CATEGORY.UNDEFINED);
                 eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.MAILINGSENDTYPE, sEnable, eLibConst.CONFIGADV_CATEGORY.UNDEFINED);
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

                dicConfigAdv[eLibConst.CONFIGADV.MAILINGSENDTYPE] = sEnable;

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

            // URL FTP
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7793), " : ");
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPURL];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);

            // Login FTP
            spanInfo = new HtmlGenericControl("span");
            spanInfoLabel = new HtmlGenericControl("span");
            spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7795), " : ");
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPLOGIN];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
            return infoList;
        }
    }
}