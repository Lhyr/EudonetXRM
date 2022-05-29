using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu de l'écran de paramétrage du CTI
    /// </summary>
    public class eAdminStoreCTIRenderer : eAdminStoreFileRenderer
    {
        private IDictionary<eLibConst.CONFIG_DEFAULT, string> dicConfig;



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreCTIRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Crée un renderer pour le paramétrage du CTI
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreCTIRenderer CreateAdminStoreCTIRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreCTIRenderer rdr = new eAdminStoreCTIRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// initialisation du composant
        /// Vérification du paramétrage en base
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfig = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.CTIENABLED,
                    eLibConst.CONFIG_DEFAULT.CTIDevice
                });

                return true;
            }
            return false;
        }


        /// <summary>
        /// Ajoute le panel de rendu des paramètres
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI.ToString(), eResApp.GetRes(Pref, 7786));

            Panel targetPanel = null;
            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);

            List<ListItem> listItems = new List<ListItem>();
            foreach (eLibConst.CTI_DEVICE_TYPE typ in Enum.GetValues(typeof(eLibConst.CTI_DEVICE_TYPE)))
            {
                listItems.Add(new ListItem(eAdminTools.GetCTITypeLabel(Pref, typ), ((int)typ).ToString()));
            }

            AddDropdownOptionField(targetPanel, "ddlCTIDevice", eResApp.GetRes(Pref, 7788), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, (int)eLibConst.CONFIG_DEFAULT.CTIDevice, typeof(eLibConst.CONFIG_DEFAULT),
                listItems, dicConfig[eLibConst.CONFIG_DEFAULT.CTIDevice], EudoQuery.FieldFormat.TYP_NUMERIC,
                eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customLabelCSSClasses: "checkboxField optionField");
        }
    }
}