using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration d'une extension issue du Store (avec paramètres gérés par une spécif)
    /// </summary>
    public class eAdminExtensionFromStoreRenderer : eAdminExtensionFileRenderer
    {

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionFromStoreRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionFromStoreRenderer CreateAdminExtensionFromStoreRenderer(ePref pref, int extensionFileIdFromStore, eUserOptionsModules.USROPT_MODULE module, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(module, pref, extensionFileIdFromStore) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE, pref, extensionFileIdFromStore);

            return new eAdminExtensionFromStoreRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            string error = String.Empty;

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                bool extensionEnabled = Extension.Infos.IsEnabled;

                // TODO: charger les paramètres à partir d'une spécif/URL extraite de l'API

                return true;
            }
            return false;
        }
    }
}