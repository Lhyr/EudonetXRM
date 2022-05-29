using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionZapier : eAdminExtension
    {

        [JsonConstructor]
        public eAdminExtensionZapier(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        public eAdminExtensionZapier(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER) 
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
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



        public override bool AfterEnableProcess(bool bEnable, out string sError)
            
        {
            sError = "";
            return true;
        }



        /// <summary>
        /// pas d'activation spécifique
        /// </summary>
        /// <param name="bEnable"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool EnableProcess(bool bEnable, out string sError)
        {
            sError = "";
            return true;
        }

        public override List<HtmlGenericControl> GetModuleInfo()
        {
            return new List<HtmlGenericControl>();
        }

        /// <summary>
        /// Vérification de l'activation de l'extensions
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            sError = "";
            if (this.Infos == null)
                return false;

           

            return this.Infos.Status == EXTENSION_STATUS.STATUS_READY;
        }
    }
}