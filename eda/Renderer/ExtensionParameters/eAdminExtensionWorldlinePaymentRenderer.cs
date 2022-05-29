using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Renderer.ExtensionParameters
{
    /// <summary>
    /// Renderer du module d'administration de Paiement Worldline
    /// </summary>
    public class eAdminExtensionWorldlinePaymentRenderer : eAdminExtensionFileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionWorldlinePaymentRenderer(ePref pref, eAdminExtension extension, string initialTab) : base(pref, extension, initialTab)
        {
        }

        /// <summary>
        /// Création du rendu mode Fiche de l'extension Paiement Worldline
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore"></param>
        /// <param name="bNoInternet"></param>
        /// <param name="initialTab"></param>
        /// <returns></returns>
        public static eAdminExtensionWorldlinePaymentRenderer CreateAdminExtensionWorldlinePaymentRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT, pref) :
                eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT, pref, extensionFileIdFromStore);

            return new eAdminExtensionWorldlinePaymentRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            return base.Build();
        }
    }
}