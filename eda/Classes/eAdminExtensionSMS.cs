using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe gérant l'extension native SMS dans l'EudoStore
    /// </summary>
    public class eAdminExtensionSMS : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        private IDictionary<eLibConst.EXTENSION, string> dicExtension;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionSMS(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur avec Pref et indication explicite de l'extension fille concernée
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module"></param>
        public eAdminExtensionSMS(ePref pref, eUserOptionsModules.USROPT_MODULE module) : base(pref, module)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionSMS(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        /// <summary>
        /// Chargement des clés de configuration utilisées pour l'extension SMS
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try
            {
	            dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
	                new HashSet<eLibConst.CONFIGADV> {
	                        eLibConst.CONFIGADV.SMS_CLIENT_ID,
	                        eLibConst.CONFIGADV.SMS_MAIL_FROM,
	                        eLibConst.CONFIGADV.SMS_MAIL_TO,
	                        eLibConst.CONFIGADV.SMS_SERVER_ENABLED,
                            //SHA
                            eLibConst.CONFIGADV.SMS_SENDTYPE
                    });

                dicExtension = eLibTools.GetExtensionValues(Pref,
                  new HashSet<eLibConst.EXTENSION> { eLibConst.EXTENSION.SMS_NETMESSAGE});

                if (dicExtension.Count != 0)
                    dicConfigAdv.Add(eLibConst.CONFIGADV.SMS_NETMESSAGE, dicExtension.First().Value);
                else
                    dicConfigAdv.Add(
                        eLibConst.CONFIGADV.SMS_NETMESSAGE, eLibTools.GetConfigAdvValues(Pref,
                        new HashSet<eLibConst.CONFIGADV> {
                            eLibConst.CONFIGADV.SMS_SETTINGS }).ToString()
                        );

            }
            catch (Exception e)
            {
                dicConfigAdv = null;
                sError = String.Concat("Les informations de configuration de l'extension SMS ne sont pas inscrites en base. L'extension a t-elle été activée ?", Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace);
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
                return true;
            }
        }

        /// <summary>
        /// Vérifie si l'extension est activée en base
        /// </summary>
        /// <param name="sError">Message d'erreur éventuellement remonté</param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                //SHA
                bReturnValue = (dicConfigAdv[eLibConst.CONFIGADV.SMS_SERVER_ENABLED] == "1" && dicConfigAdv[eLibConst.CONFIGADV.SMS_SENDTYPE] == "0");
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Méthode appelée lors de l'activation/désactivation de l'extension SMS
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Eventuelle erreur remontée lors de l'exécution du processus</param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";
                eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.SMS_SERVER_ENABLED, sEnable, eLibConst.CONFIGADV_CATEGORY.MAIN);
                //SHA
                eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.SMS_SENDTYPE, "0", eLibConst.CONFIGADV_CATEGORY.MAIN);

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Méthode appelée APRES l'activation/désactivation de l'extension SMS
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Eventuelle erreur remontée lors de l'exécution du processus</param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                dicConfigAdv[eLibConst.CONFIGADV.SMS_SERVER_ENABLED] = sEnable;

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Renvoie les informations à afficher dans l'encart d'informations spécifiques à l'extension SMS
        /// </summary>
        /// <returns>Liste de contrôles à afficher dans l'encart</returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();

            #region SMS_MAIL_FROM
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7760), " : ");
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_FROM];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
            #endregion

            #region SMS_MAIL_TO
            spanInfo = new HtmlGenericControl("span");
            spanInfoLabel = new HtmlGenericControl("span");
            spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7761), " : ");
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_TO];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
            #endregion

            #region SMS_CLIENT_ID
            spanInfo = new HtmlGenericControl("span");
            spanInfoLabel = new HtmlGenericControl("span");
            spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 7762), " : ");
            spanInfoValue.InnerHtml = dicConfigAdv[eLibConst.CONFIGADV.SMS_CLIENT_ID];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);
            #endregion

            return infoList;
        }

  
    }
}