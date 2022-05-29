using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminExtensionSnapshotRenderer : eAdminExtensionFileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionSnapshotRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionSnapshotRenderer CreateAdminExtensionSnapshotRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT, pref) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT, pref, extensionFileIdFromStore);

            return new eAdminExtensionSnapshotRenderer(pref, ext, initialTab);
        }

        protected override bool Init()
        {
            return base.Init();
        }

        protected override bool Build()
        {
            return base.Build();
        }
    }
}