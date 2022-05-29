using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
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
    /// Classe gérant l'extension PowerBI dans l'EudoStore
    /// </summary>
    internal class eAdminExtensionPowerBI : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> _dicoConfigAdv;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionPowerBI(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionPowerBI(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        /// <summary>
        /// Chargement des clés de configuration utilisées pour l'extension Sirene
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try
            {
                _dicoConfigAdv = eLibTools.GetConfigAdvValues(pref,
                    new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.POWERBI_ENABLED,
                                    eLibConst.CONFIGADV.POWERBI_IPRESTRICTION
                        }
                    );
            }
            catch (Exception e)
            {
                _dicoConfigAdv = null;
                sError = String.Concat("eAdminExtensionPowerBI.InitConfig error : ", Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace);
            }

            base.InitConfig(pref, out sError);

            return _dicoConfigAdv != null;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();

            // Restrictions d'IP
            HtmlGenericControl spanInfo = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoLabel = new HtmlGenericControl("span");
            HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
            spanInfoLabel.Attributes.Add("class", "extensionInfoLabel");
            spanInfoValue.Attributes.Add("class", "extensionInfoValue");
            spanInfoLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 1903), " : "); // Restrictions d'IP
            spanInfoValue.InnerHtml = _dicoConfigAdv[eLibConst.CONFIGADV.POWERBI_IPRESTRICTION];
            spanInfo.Controls.Add(spanInfoLabel);
            spanInfo.Controls.Add(spanInfoValue);
            infoList.Add(spanInfo);

            return infoList;
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bPwrBiEnabled = false;
            sError = String.Empty;

            try
            {
                bPwrBiEnabled = _dicoConfigAdv[eLibConst.CONFIGADV.POWERBI_ENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bPwrBiEnabled;
        }

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
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.POWERBI_ENABLED, sEnable, eLibConst.CONFIGADV_CATEGORY.MAIN);

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Active ou désactive l'extension sur la base
        /// </summary>
        /// <param name="bEnable">true pour activer l'extension, false pour la désactiver</param>
        /// <param name="sError">erreur</param>
        /// <returns>Objet eAdminResult indiquant si la mise à jour s'est correctement déroulée, ou non</returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";

                _dicoConfigAdv[eLibConst.CONFIGADV.POWERBI_ENABLED] = sEnable;

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }
    }
}