using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration de l'add-in Outlook (stockage du mapping dans la table MOBILE)
    /// </summary>
    public class eAdminExtensionOutlookAddinRenderer : eAdminExtensionMobileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionOutlookAddinRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {
        }

        /// <summary>
        /// Génération du renderer de paramètres de l'extension Add-in Outlook
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="extensionFileIdFromStore">FileID de la fiche Extension sur l'EudoStore (HotCom)</param>
        /// <param name="bNoInternet">Indique si le server.config est paramétré en mode Sans internet</param>
        /// <param name="initialTab">TabID du contexte actuel</param>
        /// <returns>Le renderer</returns>
        public static eAdminExtensionOutlookAddinRenderer CreateAdminExtensionOutlookAddinRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN, pref) :
                eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN, pref, extensionFileIdFromStore);

            return new eAdminExtensionOutlookAddinRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            // On indique au JavaScript (nsAdminMobile) que le module à recharger après modification est l'extension Add-in Outlook, et non l'extension Mobile
            AddCallBackScript("nsAdminMobile.currentExtensionModule = USROPT_MODULE_ADMIN_EXTENSIONS_OUTLOOKADDIN;");

            // Puis on appelle le builder parent
            return base.Build();
        }
    }
}