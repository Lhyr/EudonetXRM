using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionNotificationsRenderer : eAdminExtensionFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionNotificationsRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionNotificationsRenderer CreateAdminExtensionNotificationsRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS, pref) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS, pref, extensionFileIdFromStore);

            return new eAdminExtensionNotificationsRenderer(pref, ext, initialTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.NOTIFICATION_ENABLED,
                        eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED,
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

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS.ToString(), eResApp.GetRes(Pref, 7179));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

#if DEBUG
                bool debugNotificationsEnabled = dicConfigAdv[eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED] == "1";

                AddCheckboxOptionField(
                    targetPanel, "chkbxDebugNotificationsEnabled", "Debug", "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED, typeof(eLibConst.CONFIGADV), debugNotificationsEnabled
                    ); // TOCHECKRES - TODORES
#endif

                return true;
            }
            return false;
        }
    }
}