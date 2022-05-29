using System;
using System.Collections.Generic;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe gérant l'extension "IP Dédié" dans l'EudoStore
    /// </summary>
    /// <authors>SHA</authors>
    /// <date>2020-12-09</date>
    public class eAdminExtensionDedicatedIp : eAdminExtension
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionDedicatedIp(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Paramètres de l'extension IP Dédiée
        /// </summary>
        public eDedicatedIpSetting diSetting = new eDedicatedIpSetting();

        /// <summary>
        /// Cet extension a un bloc paramètre
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionDedicatedIp(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP)
        {

        }

        /// <summary>
        /// Renvoie les informations à afficher dans l'encart supérieur de la fiche Extension
        /// </summary>
        /// <returns></returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            return infoList;
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
                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bReturnValue;
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = "";
            if (this.Infos == null)
                return false;

            //status global
            if (Infos.Status == EXTENSION_STATUS.STATUS_DISABLED
                || Infos.Status == EXTENSION_STATUS.STATUS_NON_INSTALLED
                || Infos.Status == EXTENSION_STATUS.STATUS_UNDEFINED
                || Infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED
                )
                Infos.IsEnabled = false;
            else
                Infos.IsEnabled = true;

            return Infos.IsEnabled;
        }

        /// <summary>
        /// Processus exécuté après activation
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                string sEnable = bEnable ? "1" : "0";
                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bReturnValue;
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
    }
}