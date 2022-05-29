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
    /// administration de l'extenison des notifications
    /// </summary>
    public class eAdminStoreNotificationsRenderer : eAdminStoreFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

   
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreNotificationsRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// administration des notifications
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreNotificationsRenderer CreateAdminStoreNotificationsRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreNotificationsRenderer rdr = new eAdminStoreNotificationsRenderer(pref, ext);

            rdr.Generate();
            return rdr;
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



        /// <summary>
        /// Ajoute le panel de rendu des paramètres
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS.ToString(), eResApp.GetRes(Pref, 7179));

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
            {
                ExtensionParametersContainer.Controls.Add(new LiteralControl("Une erreur est survenue durant l'écran de paramétrage"));
                return;
            }
            ExtensionParametersContainer.Controls.Add(section);

#if DEBUG
            bool debugNotificationsEnabled = dicConfigAdv[eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED] == "1";

            AddCheckboxOptionField(
                targetPanel, "chkbxDebugNotificationsEnabled", "Debug", "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.DEBUG_NOTIF_ENABLED, typeof(eLibConst.CONFIGADV), debugNotificationsEnabled
                ); // TOCHECKRES - TODORES
#endif


        }
    }
}