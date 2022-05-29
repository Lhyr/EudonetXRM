using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration des synchros (EudoSync Exchange Agendas / EudoSync Exchange Contacts)
    /// </summary>
    public class eAdminExtensionSynchroRenderer : eAdminExtensionFileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionSynchroRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        
        public static eAdminExtensionSynchroRenderer CreateAdminExtensionSynchroRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, pref) :
                eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO, pref, extensionFileIdFromStore);

            return new eAdminExtensionSynchroRenderer(pref, ext, initialTab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
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
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO.ToString(), eResApp.GetRes(Pref, 7179));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

                bool eudoSyncExchangeAgendasEnabled = ((eAdminExtensionSynchro)Extension).IsEudoSyncExchangeAgendasEnabled();
                bool eudoSyncExchangeContactsEnabled = ((eAdminExtensionSynchro)Extension).IsEudoSyncExchangeContactsEnabled();

                // On prépare le tableau JS de paramètres additionnels à passer à la fonction d'activation
                string enableAgendasJSParams = "[{ key: 'additionalparam_synchro_synctype', value: 'eudosyncexchangeagendas' }]";
                string enableContactsJSParams = "[{ key: 'additionalparam_synchro_synctype', value: 'eudosyncexchangecontacts' }]";

                // L'ajout de la case à cocher se fait via les méthodes standardisées Add*OptionField, mais l'activation de l'extension se fait en utilisant la fonction JS
                // du manager dédié aux extensions, pour centraliser les opérations effectuées au sein de la méthode EnableProcess() de l'extension.
                // L'utilisation de la méthode permet de générer un champ charté admin XRM, mais ne tire donc pas profit du mécanisme intégré par défaut
                // (sendJson) pour réaliser la mise à jour en base (le champ SynchroEnabled mentionné dans CONFIG étant inutilisé par les nouvelles synchros).
                // Les paramètres propCat, propKeyType, propKeyCode sont donc envoyés ici, mais non utilisés
                AddCheckboxOptionField(
                    targetPanel, "chkbxEudoSyncExchangeAgendasEnabled", eResApp.GetRes(Pref, 7881), "",
                    eAdminUpdateProperty.CATEGORY.CONFIG, (int)eLibConst.CONFIG_DEFAULT.SYNCHROENABLED, typeof(eLibConst.CONFIG_DEFAULT), eudoSyncExchangeAgendasEnabled,
                    onClick: String.Concat("nsAdmin.enableExtension(USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO, ", eudoSyncExchangeAgendasEnabled ? "false" : "true", ", false, ", enableAgendasJSParams, ")")); // TOCHECKRES

                if (eudoSyncExchangeAgendasEnabled)
                {
                    List<eEudoSyncExchangeAgendasUser> fullyEnabledUserList = ((eAdminExtensionSynchro)Extension).FullyEnabledAgendasUserList;
                    string fullyEnabledUserListLabel = fullyEnabledUserList.Count < 0 ? eResApp.GetRes(Pref, 7848) : fullyEnabledUserList.Count < 2 ? String.Concat(fullyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7849)) : String.Concat(fullyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7850)); // Nombre inconnu d'utilisateurs actifs / utilisateur actif / utilisateurs actifs
                    GetEudoSyncExchangeAgendasUsersList(targetPanel, fullyEnabledUserListLabel, fullyEnabledUserList);
                    List<eEudoSyncExchangeAgendasUser> partiallyEnabledUserList = ((eAdminExtensionSynchro)Extension).PartiallyEnabledAgendasUserList;
                    string partiallyEnabledUserListLabel = partiallyEnabledUserList.Count < 0 ? eResApp.GetRes(Pref, 7851) : partiallyEnabledUserList.Count < 2 ? String.Concat(partiallyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7852)) : String.Concat(partiallyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7853)); // Nombre inconnu d'utilisateurs partiellement paramétrés / utilisateur partiellement paramétré / utilisateurs partiellement paramétrés
                    GetEudoSyncExchangeAgendasUsersList(targetPanel, partiallyEnabledUserListLabel, partiallyEnabledUserList);
                }

                AddCheckboxOptionField(
                    targetPanel, "chkbxEudoSyncExchangeContactsEnabled", eResApp.GetRes(Pref, 7882), "",
                    eAdminUpdateProperty.CATEGORY.CONFIG, (int)eLibConst.CONFIG_DEFAULT.SYNCHROENABLED, typeof(eLibConst.CONFIG_DEFAULT), eudoSyncExchangeContactsEnabled,
                    onClick: String.Concat("nsAdmin.enableExtension(USROPT_MODULE_ADMIN_EXTENSIONS_SYNCHRO, ", eudoSyncExchangeContactsEnabled ? "false" : "true", ", false, ", enableContactsJSParams, ")")); // TOCHECKRES

                if (eudoSyncExchangeContactsEnabled)
                {
                    List<eEudoSyncExchangeContactsUser> fullyEnabledUserList = ((eAdminExtensionSynchro)Extension).FullyEnabledContactsUserList;
                    string fullyEnabledUserListLabel = fullyEnabledUserList.Count < 0 ? eResApp.GetRes(Pref, 7848) : fullyEnabledUserList.Count < 2 ? String.Concat(fullyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7849)) : String.Concat(fullyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7850)); // Nombre inconnu d'utilisateurs actifs / utilisateur actif / utilisateurs actifs
                    GetEudoSyncExchangeContactsUsersList(targetPanel, fullyEnabledUserListLabel, fullyEnabledUserList);
                    List<eEudoSyncExchangeContactsUser> partiallyEnabledUserList = ((eAdminExtensionSynchro)Extension).PartiallyEnabledContactsUserList;
                    string partiallyEnabledUserListLabel = partiallyEnabledUserList.Count < 0 ? eResApp.GetRes(Pref, 7851) : partiallyEnabledUserList.Count < 2 ? String.Concat(partiallyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7852)) : String.Concat(partiallyEnabledUserList.Count, "&nbsp;", eResApp.GetRes(Pref, 7853)); // Nombre inconnu d'utilisateurs partiellement paramétrés / utilisateur partiellement paramétré / utilisateurs partiellement paramétrés
                    GetEudoSyncExchangeContactsUsersList(targetPanel, partiallyEnabledUserListLabel, partiallyEnabledUserList);
                }

                return true;
            }
            return false;
        }

        private void GetEudoSyncExchangeAgendasUsersList(Panel targetPanel, string listLabel, List<eEudoSyncExchangeAgendasUser> userList) {
            if (userList == null || userList.Count == 0)
                return;

            HtmlGenericControl label = new HtmlGenericControl("span");
            label.Attributes.Add("class", "extensionInfoSectionTitle");
            label.InnerHtml = listLabel;

            HtmlGenericControl userListControl = new HtmlGenericControl("ul");
            userListControl.Attributes.Add("class", "extensionInfoList extensionInfoListSynchro");

            foreach (eEudoSyncExchangeUser user in userList)
            {
                HtmlGenericControl userListElt = new HtmlGenericControl("li");
                HtmlGenericControl spanSubInfoLabel = new HtmlGenericControl("span");
                HtmlGenericControl spanSubInfoValue = new HtmlGenericControl("span");
                spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanSubInfoValue.InnerText = String.Concat(user.UserLogin, " (", user.UserDisplayName, ")");
                spanSubInfoLabel.InnerText = String.Concat(
                    " - ",
                    user.UserMail.Trim().Length > 0 ? user.UserMail : eResApp.GetRes(Pref, 7897) // Pas d'adresse e-mail
                );
                if (user is eEudoSyncExchangeContactsUser)
                    spanSubInfoLabel.InnerText = String.Concat(
                        spanSubInfoLabel.InnerText,
                        " - ",
                        ((eEudoSyncExchangeContactsUser)user).UserExchangeServer.Trim().Length > 0 ? ((eEudoSyncExchangeContactsUser)user).UserExchangeServer : eResApp.GetRes(Pref, 7898) // Alias du serveur Exchange non renseigné
                    );

                userListElt.Controls.Add(spanSubInfoValue);
                userListElt.Controls.Add(spanSubInfoLabel);
                userListControl.Controls.Add(userListElt);
            }

            targetPanel.Controls.Add(label);
            targetPanel.Controls.Add(userListControl);
        }

        private void GetEudoSyncExchangeContactsUsersList(Panel targetPanel, string listLabel, List<eEudoSyncExchangeContactsUser> userList)
        {
            if (userList == null || userList.Count == 0)
                return;

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("class", "extensionInfoSectionTitle");
            label.InnerHtml = listLabel;

            HtmlGenericControl userListControl = new HtmlGenericControl("ul");
            userListControl.Attributes.Add("class", "extensionInfoList extensionInfoListSynchro");

            foreach (eEudoSyncExchangeUser user in userList)
            {
                HtmlGenericControl userListElt = new HtmlGenericControl("li");
                HtmlGenericControl spanSubInfoLabel = new HtmlGenericControl("span");
                HtmlGenericControl spanSubInfoValue = new HtmlGenericControl("span");
                spanSubInfoLabel.Attributes.Add("class", "extensionInfoLabel");
                spanSubInfoValue.Attributes.Add("class", "extensionInfoValue");
                spanSubInfoValue.InnerText = String.Concat(user.UserLogin, " (", user.UserDisplayName, ")");
                spanSubInfoLabel.InnerText = String.Concat(
                    " - ",
                    user.UserMail.Trim().Length > 0 ? user.UserMail : eResApp.GetRes(Pref, 7897), // Pas d'adresse e-mail
                    " - ",
                    ((eEudoSyncExchangeContactsUser)user).UserExchangeServer.Trim().Length > 0 ? ((eEudoSyncExchangeContactsUser)user).UserExchangeServer : eResApp.GetRes(Pref, 7898) // Alias du serveur Exchange non renseigné
                );

                userListElt.Controls.Add(spanSubInfoValue);
                userListElt.Controls.Add(spanSubInfoLabel);
                userListControl.Controls.Add(userListElt);
            }

            targetPanel.Controls.Add(label);
            targetPanel.Controls.Add(userListControl);
        }
    }
}