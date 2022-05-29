using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionCTIRenderer : eAdminExtensionFileRenderer
    {
        private IDictionary<eLibConst.CONFIG_DEFAULT, string> dicConfig;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionCTIRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionCTIRenderer CreateAdminExtensionCTIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI, pref) :
               eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI, pref, extensionFileIdFromStore);

            return new eAdminExtensionCTIRenderer(pref, ext, initialTab);
        }

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

        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI.ToString(), eResApp.GetRes(Pref, 7786));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

                List<ListItem> listItems = new List<ListItem>();
                foreach (eLibConst.CTI_DEVICE_TYPE typ in Enum.GetValues(typeof(eLibConst.CTI_DEVICE_TYPE)))
                {
                    listItems.Add(new ListItem(eAdminTools.GetCTITypeLabel(Pref, typ), ((int)typ).ToString()));
                }

                AddDropdownOptionField(targetPanel, "ddlCTIDevice", eResApp.GetRes(Pref, 7788), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, (int)eLibConst.CONFIG_DEFAULT.CTIDevice, typeof(eLibConst.CONFIG_DEFAULT),
                    listItems, dicConfig[eLibConst.CONFIG_DEFAULT.CTIDevice], EudoQuery.FieldFormat.TYP_NUMERIC,
                    eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customLabelCSSClasses: "checkboxField optionField");

                return true;
            }
            return false;
        }
    }
}