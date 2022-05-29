using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de PowerBI
    /// </summary>
    public class eAdminStorePowerBIRenderer : eAdminStoreFileRenderer
    {

        private IDictionary<eLibConst.CONFIGADV, string> _dicoConfigAdv;




        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStorePowerBIRenderer(ePref pref, eAdminExtension extension) : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension PowerBI
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStorePowerBIRenderer CreateAdminStorePowerBIRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStorePowerBIRenderer rdr = new eAdminStorePowerBIRenderer(pref, ext);

            rdr.Generate();
            return rdr;
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
        /// écran de paramétrage
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {

            base.CreateSettingsPanel();

            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI.ToString(), eResApp.GetRes(Pref, 206));

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);

            AddTextboxOptionField(targetPanel, "txt_pwrbi_ip_restrictions", eResApp.GetRes(Pref, 1915), eResApp.GetRes(Pref, 1904),
                                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.POWERBI_IPRESTRICTION, typeof(eLibConst.CONFIGADV),
                                    _dicoConfigAdv[eLibConst.CONFIGADV.POWERBI_IPRESTRICTION], AdminFieldType.ADM_TYPE_CHAR
                                    );
        }
    }
}