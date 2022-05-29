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
    /// Classe gérant l'extension native Sirene dans l'EudoStore
    /// </summary>
    public class eAdminExtensionSirene : eAdminExtension
    {
        IDictionary<eLibConst.CONFIGADV, string> dicoConfig;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionSirene(eAdminExtensionInfo infos) : base()
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionSirene(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE)
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
                dicoConfig = eLibTools.GetConfigAdvValues(pref,
                    new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.SIRENE_API_URL
                        }
                    );
            }
            catch (Exception e)
            {
                dicoConfig = null;
                sError = String.Concat("L'URL d'accès au référentiel Sirene n'est pas définie. L'extension a t-elle été activée ?", Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace);
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
        /// <param name="sError">Message d'erreur éventuellement remonté</param>
        /// <returns></returns>
        public override bool IsEnabledProcess(out string sError)
        {
            bool bReturnValue = false;
            sError = String.Empty;

            try
            {
                bReturnValue = dicoConfig[eLibConst.CONFIGADV.SIRENE_API_URL].Length > 0;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Méthode appelée lors de l'activation/désactivation de l'extension Sirene
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
                // Pour l'activation de Sirene, on utilise en priorité l'URL éventuellement définie dans le Web.config, puis, à défaut, celle définie dans eConst
                string sireneAPIURL = ConfigurationManager.AppSettings["EudoSireneURL"];
                if (String.IsNullOrEmpty(sireneAPIURL))
                    sireneAPIURL = eModelConst.SIRENE_API_DEFAULTURL;

                eLibTools.AddOrUpdateConfigAdv(_pref, eLibConst.CONFIGADV.SIRENE_API_URL, (bEnable ? sireneAPIURL : String.Empty), eLibConst.CONFIGADV_CATEGORY.MAIN); // TOCHECK

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Méthode appelée APRES l'activation/désactivation de l'extension Sirene
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
                dicoConfig[eLibConst.CONFIGADV.SIRENE_API_URL] = (bEnable ? eModelConst.SIRENE_API_DEFAULTURL : String.Empty);

                bReturnValue = true;
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, e.StackTrace);
            }

            return bReturnValue;
        }

        /// <summary>
        /// Renvoie les informations à afficher dans l'encart d'informations spécifiques à l'extension Sirene
        /// </summary>
        /// <returns>Liste de contrôles à afficher dans l'encart</returns>
        public override List<HtmlGenericControl> GetModuleInfo()
        {
            List<HtmlGenericControl> infoList = new List<HtmlGenericControl>();
            List<Tuple<string, string>> infoData = new List<Tuple<string, string>>();

            infoData.Add(new Tuple<string, string>(eResApp.GetRes(Pref, 8559), eResApp.GetRes(Pref, 1206))); // Le référentiel Sirene est / activé/désactivé

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