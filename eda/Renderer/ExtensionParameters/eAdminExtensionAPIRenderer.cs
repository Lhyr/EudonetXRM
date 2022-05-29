using System.Collections.Generic;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension API
    /// </summary>
    public class eAdminExtensionAPIRenderer : eAdminExtensionFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionAPIRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        /// <summary>
        /// Génération du render des paramètres de l'extension API
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore"></param>
        /// <param name="bNoInternet"></param>
        /// <param name="initialTab"></param>
        /// <returns></returns>
        public static eAdminExtensionAPIRenderer CreateAdminExtensionAPIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API, pref) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API, pref, extensionFileIdFromStore);

            return new eAdminExtensionAPIRenderer(pref, ext, initialTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.API_ENABLED
                    });

                return true;
            }
            return false;
        }

        protected override bool Build()
        {
            return base.Build();
        }
    }
}