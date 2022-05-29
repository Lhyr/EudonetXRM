using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension parente de toutes les extensions liées aux demandes entrantes (Ubiflow, HBS)
    /// Backlogs #344 et #345
    /// </summary>
    public class eAdminExtensionIn : eAdminExtension
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;
        private eLibConst.CONFIGADV configKeyEnabled = eLibConst.CONFIGADV.UNDEFINED;
        private eLibConst.CONFIGADV_CATEGORY configCategory = eLibConst.CONFIGADV_CATEGORY.UNDEFINED;
        private eUserOptionsModules.USROPT_MODULE childExtensionModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;

        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionIn(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur pour l'initialisation avec Pref
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="module">Module enfant correspondant à l'extension héritant de cette extension</param>
        public eAdminExtensionIn(ePref pref, eUserOptionsModules.USROPT_MODULE module) : base(pref, module)
        {
            string strError = String.Empty;
            InitConfig(pref, out strError);
        }

        /// <summary>
        /// Initialisation de la configuration de l'extension
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        protected override bool InitConfig(ePref pref, out string sError)
        {
            sError = String.Empty;

            try
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(pref,
                    new HashSet<eLibConst.CONFIGADV> { ConfigKeyEnabled }
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
        /// Fournit la clé de CONFIGADV indiquant si l'extension est activée
        /// </summary>
        public eLibConst.CONFIGADV ConfigKeyEnabled
        {
            get
            {
                return configKeyEnabled;
            }

            set
            {
                configKeyEnabled = value;
            }
        }

        /// <summary>
        /// Indique la catégorie de clés de configuration rattachée à l'extension
        /// </summary>
        public eLibConst.CONFIGADV_CATEGORY ConfigCategory
        {
            get
            {
                return configCategory;
            }

            set
            {
                configCategory = value;
            }
        }

        /// <summary>
        /// Indique à quel module correspond l'extension héritant de cette extension parente
        /// </summary>
        public eUserOptionsModules.USROPT_MODULE ChildExtensionModule
        {
            get
            {
                return childExtensionModule;
            }

            set
            {
                childExtensionModule = value;
            }
        }

        /// <summary>
        /// Effectue le traitement pour vérifier si l'extension est activée
        /// </summary>
        /// <param name="sError"></param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                if (ConfigKeyEnabled != eLibConst.CONFIGADV.UNDEFINED)
                    bReturnValue = dicConfigAdv[ConfigKeyEnabled] == "1";
                else
                    bReturnValue = true; // à défaut de clé valide, l'extension est considérée comme toujours activée
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }
            return bReturnValue;
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

                if (ConfigKeyEnabled != eLibConst.CONFIGADV.UNDEFINED)
                    eLibTools.AddOrUpdateConfigAdv(Pref, ConfigKeyEnabled, sEnable, ConfigCategory);
                // Si pas de clé de config valide définie, pas de MAJ en base, le traitement renverra toujours true

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
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
                // Si pas de clé de config valide définie, pas de MAJ, le traitement renverra toujours true
                if (ConfigKeyEnabled != eLibConst.CONFIGADV.UNDEFINED)
                {
                    string sEnable = bEnable ? "1" : "0";
                    dicConfigAdv[ConfigKeyEnabled] = sEnable;
                }

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
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
    }
}