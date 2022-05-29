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
    public class eAdminExtensionAPI : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionAPI(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// L'API a un bloc paramètre
        /// </summary>
        public override bool ShowParametersTab
        {
            get
            {
                return true;
            }
        }


        public eAdminExtensionAPI(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(pref,
                    new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.API_ENABLED
                        }
                    );
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
                return false;
            }
        }



        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="sError">erreur a l'activation</param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bApiEnabled = false;
            sError = String.Empty;

            try
            {
                bApiEnabled = dicConfigAdv[eLibConst.CONFIGADV.API_ENABLED] == "1";
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bApiEnabled;
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

                eLibTools.AddOrUpdateConfigAdv(Pref, eLibConst.CONFIGADV.API_ENABLED, sEnable, eLibConst.CONFIGADV_CATEGORY.MAIN);

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

                dicConfigAdv[eLibConst.CONFIGADV.API_ENABLED] = sEnable;

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

            return infoList;
        }
    }
}