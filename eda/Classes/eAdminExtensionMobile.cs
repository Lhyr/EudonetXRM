using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension Eudonet Mobile alias eudo touch
    /// </summary>
    public class eAdminExtensionMobile : eAdminExtension
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, string> dicoConfig;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionMobile(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionMobile(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }


        /// <summary>
        /// Initialisation de la configuration
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try {
                dicoConfig = _pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                        { eLibConst.CONFIG_DEFAULT.MOBILEENABLED });
            }
            catch (Exception e)
            {
                dicoConfig = null;
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            base.InitConfig(pref, out sError);

            return dicoConfig != null;
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
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                bReturnValue = dicoConfig[eLibConst.CONFIG_DEFAULT.MOBILEENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Processus d'activation/désactivation de l'extension
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                Pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.MOBILEENABLED.ToString(), sEnable) });

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Processus exécuté après (dés)activation
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                dicoConfig[eLibConst.CONFIG_DEFAULT.MOBILEENABLED] = sEnable;

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Renvoie les infos à afficher en entête de l'administration de l'extension
        /// </summary>
        /// <returns></returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            List<Tuple<string, string>> infoData = new List<Tuple<string, string>>();

            infoData.Add(new Tuple<string, string>("Eudonet XRM pour iOS est", "activée"));
            infoData.Add(new Tuple<string, string>("Eudonet XRM pour Android est", "activée"));

            foreach (Tuple<string, string> info in infoData)
            {
                HtmlGenericControl spanInfo = new HtmlGenericControl("span");
                HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
                HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
                spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanInfoLabel.InnerHtml = String.Concat(info.Item1, "&nbsp;");
                spanInfoValue.InnerHtml = info.Item2;
                spanInfo.Controls.Add(spanInfoLabel);
                spanInfo.Controls.Add(spanInfoValue);
                infoList.Add(spanInfo);
            }

            return infoList;
        }
    }
}