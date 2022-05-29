using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de PowerBI
    /// </summary>
    public class eAdminExtensionPowerBIRenderer : eAdminExtensionFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> _dicoConfigAdv;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionPowerBIRenderer(ePref pref, eAdminExtension extension, string initialTab) : base(pref, extension, initialTab)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension PowerBI
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore"></param>
        /// <param name="bNoInternet"></param>
        /// <param name="initialTab"></param>
        /// <returns></returns>
        public static eAdminExtensionPowerBIRenderer CreateAdminExtensionPowerBIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI, pref) :
                eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI, pref, extensionFileIdFromStore);

            return new eAdminExtensionPowerBIRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            string error = String.Empty;

            if (base.Init())
            {
                try
                {
                    _dicoConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                        new HashSet<eLibConst.CONFIGADV> {
                                    eLibConst.CONFIGADV.POWERBI_IPRESTRICTION
                            }
                        );

                    return true;
                }
                catch (Exception e)
                {
                    error = String.Concat("eAdminExtensionPowerBI.Init error : ", Environment.NewLine, e.Message, Environment.NewLine, e.StackTrace);
                }
            }

            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI.ToString(), eResApp.GetRes(Pref, 206));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;
                

                AddTextboxOptionField(targetPanel, "txt_pwrbi_ip_restrictions", eResApp.GetRes(Pref, 1915), eResApp.GetRes(Pref, 1904),                 
                                        eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.POWERBI_IPRESTRICTION, typeof(eLibConst.CONFIGADV),
                                        _dicoConfigAdv[eLibConst.CONFIGADV.POWERBI_IPRESTRICTION], AdminFieldType.ADM_TYPE_CHAR
                                        );

                return true;
            }
            else
                return false;
        }
    }
}