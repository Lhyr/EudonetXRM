using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension Add-in Outlook
    /// </summary>
    public class eAdminExtensionOutlookAddin : eAdminExtension
    {


        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionOutlookAddin(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionOutlookAddin(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN)
        {
            string strError = "";
            InitConfig(pref, out strError);
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

            sError = "";

            if (this.Infos == null)
                return false;


            return this.Infos.Status == EXTENSION_STATUS.STATUS_READY;
        }

        /// <summary>
        /// Processus d'activation/désactivation de l'extension
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            sError = "";
            return true;

        }

        /// <summary>
        /// Processus exécuté après (dés)activation
        /// </summary>
        /// <param name="bEnable">Doit-on activer ou désactiver l'extension ?</param>
        /// <param name="sError">Erreur éventuellement survenue</param>
        /// <returns></returns>
        public override bool AfterEnableProcess(bool bEnable, out string sError)
        {

            sError = String.Empty;
            return true;

        }

        /// <summary>
        /// Renvoie les infos à afficher en entête de l'administration de l'extension
        /// </summary>
        /// <returns></returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();


            return infoList;
        }


    }
}