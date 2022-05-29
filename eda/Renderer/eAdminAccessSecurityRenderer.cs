using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de l'administration des onglets
    /// </summary>
    public class eAdminAccessSecurityRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;

        Panel _pnlGlobalRights;
        Panel _pnlGroupPolicy;
        Panel _pnlPasswordPolicy;
        Panel _pnlSessionProperties;
        Panel _pnlIPRestrictions;
        Panel _pnlAuthenticationMethods;

        Dictionary<eLibConst.CONFIG_DEFAULT, String> _defaultConfigs;
        IDictionary<eLibConst.CONFIGADV, String> _advConfigs;

        eUserOptionsModules.USROPT_MODULE _childModule;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminAccessSecurityRenderer(ePref pref, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _width = nWidth - 30;
            _height = nHeight - 30;
        }


        public static eAdminAccessSecurityRenderer CreateAdminAccessSecurityRenderer(ePref pref, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminAccessSecurityRenderer rdr = new eAdminAccessSecurityRenderer(pref, nWidth, nHeight);

            rdr.Generate();

            return rdr;
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {

                if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminAccess_Security))
                    return false;

                // On récupère toutes les options affichées sur cette page en un seul accès à la base

                _defaultConfigs = Pref.GetConfigDefault(new HashSet<eLibConst.CONFIG_DEFAULT>()
                {
                    eLibConst.CONFIG_DEFAULT.GROUP,
                    eLibConst.CONFIG_DEFAULT.HIDEGROUPTREE,
                    eLibConst.CONFIG_DEFAULT.PASSWORDMINLENGTH,
                    eLibConst.CONFIG_DEFAULT.PASSWORDMAXAGE,
                    eLibConst.CONFIG_DEFAULT.PASSWORDHISTORY,
                    eLibConst.CONFIG_DEFAULT.SESSIONTIMEOUT,
                    eLibConst.CONFIG_DEFAULT.IPAccessEnabled,
                    eLibConst.CONFIG_DEFAULT.LDAPEnabled,
                    eLibConst.CONFIG_DEFAULT.LDAPAutoCreate,
                    eLibConst.CONFIG_DEFAULT.LDAPBrowseUser,
                    eLibConst.CONFIG_DEFAULT.LDAPBrowsePwd,
                    eLibConst.CONFIG_DEFAULT.LDAPIPAddressServer,
                    eLibConst.CONFIG_DEFAULT.LDAPDomainName,
                    eLibConst.CONFIG_DEFAULT.LDAPUSERDN,
                    eLibConst.CONFIG_DEFAULT.LDAPGroupUser,
                    eLibConst.CONFIG_DEFAULT.LDAPLOGINFIELD,
                    eLibConst.CONFIG_DEFAULT.LDAPUserFullNamePropertiyName,
                    eLibConst.CONFIG_DEFAULT.LDAPEmailPropertiyName,
                });

                _advConfigs = eLibTools.GetConfigAdvValues(Pref, new HashSet<eLibConst.CONFIGADV>()
                {
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_CASE_SENSITIVE,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_FAILED_CONNEXION_THRESHOLD_WARNING,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_FAILED_CONNEXION_WARNING_MAIL,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_MAX_CONNEXION_ATTEMPT,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_MIN_NUM,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_MIN_SPEC,
                    eLibConst.CONFIGADV.PASSWORD_POLICIES_MIN_SPEC,
                        eLibConst.CONFIGADV.PASSWORD_POLICIES_ALGO,
                    eLibConst.CONFIGADV.SSO_CAS,
                    eLibConst.CONFIGADV.LDAP_PROTOCOLVERSION,
                    eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS,
                     eLibConst.CONFIGADV.AUTHENTICATION_MODE,
                     eLibConst.CONFIGADV.LDAP_CONFIG,
                });

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {

            PgContainer.ID = "adminAccessSecurity";

            _pnlGlobalRights = GetModuleSection("globalRights", eResApp.GetRes(Pref, 8692)); // Administration des droits globaux
            _pnlGroupPolicy = GetModuleSection("groupPolicy", eResApp.GetRes(Pref, 7184)); // Restrictions d'affichage liées aux groupes d'utilisateurs
            _pnlPasswordPolicy = GetModuleSection("passwordPolicy", eResApp.GetRes(Pref, 7185)); // Stratégie de mot de passe
            _pnlSessionProperties = GetModuleSection("sessionProperties", eResApp.GetRes(Pref, 7186)); // Durée de session
            _pnlIPRestrictions = GetModuleSection("ipRestrictions", eResApp.GetRes(Pref, 7187)); // Adresses IP autorisées
            _pnlAuthenticationMethods = GetModuleSection("authenticationMethods", eResApp.GetRes(Pref, 7188)); // Méthodes d'authentification
            _pnlPasswordPolicy.ID = "pnlPasswordPolicy"; //Permet de faire la surcharge du css pour ce panel en particulier

            GetGroupGlobalRightsContents();
            GetGroupPolicyContents();
            GetPasswordPolicyContents();
            GetSessionPropertiesContents();
            GetIPRestrictionsContents();
            GetAuthenticationMethodsContents();

            _pgContainer.Controls.Add(_pnlGlobalRights);
            _pgContainer.Controls.Add(_pnlGroupPolicy);
            _pgContainer.Controls.Add(_pnlPasswordPolicy);
            _pgContainer.Controls.Add(_pnlSessionProperties);
            _pgContainer.Controls.Add(_pnlIPRestrictions);
            _pgContainer.Controls.Add(_pnlAuthenticationMethods);

            return true;
        }

        private void GetGroupGlobalRightsContents()
        {
            Panel targetContainer = null;
            if (_pnlGlobalRights.Controls.Count > 0 && _pnlGlobalRights.Controls[_pnlGlobalRights.Controls.Count - 1] is Panel)
                targetContainer = (Panel)_pnlGlobalRights.Controls[_pnlGlobalRights.Controls.Count - 1];

            eAdminButtonField btn = new eAdminButtonField(eResApp.GetRes(Pref, 7611), "btnGlobalRigths", onclick: "nsAdmin.confRights(-1);");
            btn.Generate(targetContainer);

        }

        /// <summary>
        /// Génère le contenu de la section "Restrictions d'affichage liées aux groupes d'utilisateurs"
        /// </summary>
        private void GetGroupPolicyContents()
        {
            Panel targetContainer = null;
            if (_pnlGroupPolicy.Controls.Count > 0 && _pnlGroupPolicy.Controls[_pnlGroupPolicy.Controls.Count - 1] is Panel)
                targetContainer = (Panel)_pnlGroupPolicy.Controls[_pnlGroupPolicy.Controls.Count - 1];
            if (targetContainer == null)
                return;

            /*
            /// <summary>Pas de gestion des groupes</summary>
            GROUP_NONE = 0,
            /// <summary>Gestion des groupes en exclusion</summary>
            GROUP_EXCLUDING = 1,
            /// <summary>Gestion des groupes en lecture seule</summary>
            GROUP_READONLY = 2,
            /// <summary>Affiche les groupes</summary>
            GROUP_DISPLAY = 3,
            /// <summary>Gestion des groupes en exclusion avec lecture seule</summary>
            GROUP_EXCLUDING_READONLY = 4
            */

            string groupName = "rbGroupPolicy";
            string rbOnChange = "nsAdmin.accessSecurityGroupChange(this);";

            string id = String.Concat(groupName, "None");
            string label = String.Empty;
            string tooltip = eResApp.GetRes(Pref, 7234); // "Gérer les utilisateurs sans regroupement"
            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.GROUP;
            int propKeyCode = (int)propKeyConfig;
            Type propKeyType = propKeyConfig.GetType();
            Dictionary<string, string> items = new Dictionary<string, string>();
            items.Add(SECURITY_GROUP.GROUP_NONE.GetHashCode().ToString(), eResApp.GetRes(Pref, 7234)); // "Gérer les utilisateurs sans regroupement"
            FieldFormat valueFormat = FieldFormat.TYP_NUMERIC;
            int currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            int currentGroupValue = currentValue;
            Control rbGroupPolicyNone = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);

            id = String.Concat(groupName, "Enabled");
            tooltip = eResApp.GetRes(Pref, 7235); // "Permettre le regroupement hiérarchique des utilisateurs"
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.GROUP;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            items = new Dictionary<string, string>();
            items.Add(SECURITY_GROUP.GROUP_DISPLAY.GetHashCode().ToString(), eResApp.GetRes(Pref, 7235)); // "Permettre le regroupement hiérarchique des utilisateurs"
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            Control rbGroupPolicyEnabled = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbGroupPolicyEnabled.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    if (grandChildControl.Attributes["type"] == "radio")
                        if (currentGroupValue != (int)SECURITY_GROUP.GROUP_NONE)
                            grandChildControl.Attributes["checked"] = "checked";
                        else
                            grandChildControl.Attributes.Remove("checked");

            Panel groupPolicySubContainer = new Panel();
            groupPolicySubContainer.ID = "groupPolicyOptions";
            groupPolicySubContainer.CssClass = "adminGroupPolicyOptions";
            targetContainer.Controls.Add(groupPolicySubContainer);
            targetContainer = groupPolicySubContainer;

            id = "ddlGroupPolicyRestrictions";
            label = eResApp.GetRes(Pref, 7236); // "Restrictions d'accès aux fiches"
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.GROUP;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            List<ListItem> listItems = new List<ListItem>();
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 7237), SECURITY_GROUP.GROUP_DISPLAY.GetHashCode().ToString())); // "Aucune restriction"
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 7238), SECURITY_GROUP.GROUP_READONLY.GetHashCode().ToString())); // "Visibles sans restriction et modifiables uniquement par le groupe"
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 7239), SECURITY_GROUP.GROUP_EXCLUDING.GetHashCode().ToString())); // "Visibles et modifiables uniquement par le groupe"
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 7240), SECURITY_GROUP.GROUP_EXCLUDING_READONLY.GetHashCode().ToString())); // "Visibles uniquement par le groupe et modifiables uniquement par l'utilisateur"
            listItems[0].Attributes.Add("title", eResApp.GetRes(Pref, 7241)); // "Choisir cette option lorsque, par défaut, les utilisateurs peuvent voir et modifier toutes les fiches"
            listItems[1].Attributes.Add("title", eResApp.GetRes(Pref, 7242)); // "Choisir cette option lorsque seuls les utilisateurs d’un même groupe et des groupes hiérarchiquement inférieurs peuvent modifier une fiche appartenant à l’un d’entre eux"
            listItems[2].Attributes.Add("title", eResApp.GetRes(Pref, 7243)); // "Choisir cette option lorsque seuls les utilisateurs d’un même groupe et des groupes hiérarchiquement inférieurs peuvent voir et modifier une fiche appartenant à l’un d’entre eux"
            listItems[3].Attributes.Add("title", eResApp.GetRes(Pref, 7244)); // "Choisir cette option lorsque seuls les utilisateurs d’un même groupe et des groupes hiérarchiquement inférieurs peuvent voir une fiche appartenant à l’un d’entre eux et que seul l’utilisateur peut modifier une fiche qui lui appartient"
            valueFormat = FieldFormat.TYP_NUMERIC;
            string customLabelCSSClasses = "labelField optionField";
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            Control ddlGroupPolicyRestrictions = AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, listItems, currentValue.ToString(), valueFormat, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, onChange: rbOnChange, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            ((DropDownList)ddlGroupPolicyRestrictions).Enabled = (currentGroupValue != (int)SECURITY_GROUP.GROUP_NONE);

            groupName = String.Concat(groupName, "GroupTreeDisplay");
            id = groupName;
            label = eResApp.GetRes(Pref, 7285);
            tooltip = String.Empty;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.HIDEGROUPTREE;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 7245)); // "Afficher uniquement le nom de l'utilisateur"
            items.Add("0", eResApp.GetRes(Pref, 7246)); // "Afficher le nom de l'utilisateur précédé de l'ensemble de ses groupes d'appartenance"
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            Control rbGroupPolicyDisplay = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbGroupPolicyDisplay.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    grandChildControl.Disabled = (currentGroupValue == (int)SECURITY_GROUP.GROUP_NONE);
        }

        /// <summary>
        /// Génère le contenu de la section "Stratégie de mot de passe" et l'ajoute à la section en question
        /// </summary>
        private void GetPasswordPolicyContents()
        {
            Panel targetContainer = null;
            if (_pnlPasswordPolicy.Controls.Count > 0 && _pnlPasswordPolicy.Controls[_pnlPasswordPolicy.Controls.Count - 1] is Panel)
                targetContainer = (Panel)_pnlPasswordPolicy.Controls[_pnlPasswordPolicy.Controls.Count - 1];
            if (targetContainer == null)
                return;

            #region ALGO
            string id = "ddlPasswordalgo";
            string label = eResApp.GetRes(Pref, 2620);
            string tooltip = "";

            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            eLibConst.CONFIGADV propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_ALGO;
            int propKeyCode = (int)propKeyAdvConfig;
            Type propKeyType = propKeyAdvConfig.GetType();
            FieldFormat valueFormat = FieldFormat.TYP_BIT;

            string customCheckboxLabelCSSClasses = "checkboxField optionField";

            Dictionary<string, string> dicPanelStyle = new Dictionary<string, string>();
            dicPanelStyle.Add("margin-left", "15%");


            int currentValue = (int)eLibTools.GetEnumFromCode<PASSWORD_ALGO>(_advConfigs[propKeyAdvConfig]);

            Control ctr = AddCheckboxOptionField(
                targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType,
                currentValue == (int)PASSWORD_ALGO.ARGON2,
                customPanelStyleAttributes: dicPanelStyle,

                customLabelCSSClasses: customCheckboxLabelCSSClasses,
                swap: false);

            Control a = ctr.Controls[1];
            HtmlGenericControl c = new System.Web.UI.HtmlControls.HtmlGenericControl();
            c.Attributes.Add("class", "icon-exclamation-circle");
            c.Attributes.Add("title", eResApp.GetRes(Pref, 2621));
            c.Attributes.Add("style", "display:inline;margi-right:0px;margin-left:5px");
            a.Controls.Add(c);

            #endregion

            #region Sensibilité à la casse
            id = "ddlPasswordCaseSensitive";
            label = eResApp.GetRes(Pref, 6974);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_CASE_SENSITIVE;
            propKeyCode = (int)propKeyAdvConfig;
            propKeyType = propKeyAdvConfig.GetType();
            valueFormat = FieldFormat.TYP_BIT;
            customCheckboxLabelCSSClasses = "checkboxField optionField";
            currentValue = eLibTools.GetNum(_advConfigs[propKeyAdvConfig]);

            ctr = AddCheckboxOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode,
                propKeyType, currentValue == 1,
                customLabelCSSClasses: customCheckboxLabelCSSClasses,
                 customPanelStyleAttributes: dicPanelStyle, swap: false);

            #endregion

            #region Longueur du mot de passe
            id = "ddlPasswordMinLength";
            label = eResApp.GetRes(Pref, 1453);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.PASSWORDMINLENGTH;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            List<ListItem> items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            eAdminDropdownField.eAdminDropdownFieldRenderType ddlRenderType = eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE;
            string customLabelCSSClasses = "labelField optionField";
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            // Propositions de la v7 (1 à 10 caractères) + propositions de http://www.ssi.gouv.fr/administration/precautions-elementaires/calculer-la-force-dun-mot-de-passe/
            int[] availableValues = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 16, 20, 25, 30 };
            foreach (int availableValue in availableValues)
            {
                string resource = eResApp.GetRes(Pref, 7337); // caractères
                if (availableValue < 2)
                    resource = eResApp.GetRes(Pref, 229); // Caractère
                items.Add(new ListItem(String.Concat(availableValue.ToString(), " ", resource.ToLower()), availableValue.ToString())); // caractères
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion

            #region Nombre minimal de caractères numériques
            id = "ddlPasswordMinNum";
            label = eResApp.GetRes(Pref, 6970);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_MIN_NUM;
            propKeyCode = (int)propKeyAdvConfig;
            propKeyType = propKeyAdvConfig.GetType();
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_advConfigs[propKeyAdvConfig]);
            // Propositions fixes. TODO: champ de saisie et vérification de cohérence vis-à-vis de PasswordMinLength ?
            availableValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int availableValue in availableValues)
            {
                string resource = eResApp.GetRes(Pref, 7337); // caractères
                if (availableValue < 2)
                    resource = eResApp.GetRes(Pref, 229); // Caractère
                items.Add(new ListItem(String.Concat(availableValue.ToString(), " ", resource.ToLower()), availableValue.ToString())); // caractères
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion

            #region Nombre minimal de caractères spéciaux
            id = "ddlPasswordMinSpec";
            label = eResApp.GetRes(Pref, 6973);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_MIN_SPEC;
            propKeyCode = (int)propKeyAdvConfig;
            propKeyType = propKeyAdvConfig.GetType();
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_advConfigs[propKeyAdvConfig]);
            // Propositions fixes. TODO: champ de saisie et vérification de cohérence vis-à-vis de PasswordMinLength ?
            availableValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int availableValue in availableValues)
            {
                string resource = eResApp.GetRes(Pref, 7337); // caractères
                if (availableValue < 2)
                    resource = eResApp.GetRes(Pref, 229); // Caractère
                items.Add(new ListItem(String.Concat(availableValue.ToString(), " ", resource.ToLower()), availableValue.ToString())); // caractères
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion


            #region Expiration du mot de passe
            id = "ddlPasswordMaxAge";
            label = eResApp.GetRes(Pref, 1463);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.PASSWORDMAXAGE;
            propKeyCode = (int)propKeyConfig;
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            // Propositions de la v7 + "Le mot de passe n'expire jamais" (0)
            availableValues = new int[] { 0, 10, 15, 20, 25, 30, 45, 60, 75, 90 };
            foreach (int availableValue in availableValues)
            {
                if (availableValue == 0)
                    items.Add(new ListItem(eResApp.GetRes(Pref, 7189), availableValue.ToString())); // Jamais
                else
                    items.Add(new ListItem(String.Concat(availableValue.ToString(), " ", eResApp.GetRes(Pref, 884)), availableValue.ToString())); // jours
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyConfig.GetType(), items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion

            #region Nombre de mots de passe réutilisables
            id = "ddlPasswordHistory";
            label = eResApp.GetRes(Pref, 7190); // Lors du changement de mot de passe, l'utilisateur
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.PASSWORDHISTORY;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            // Propositions de la v7 et des specs
            availableValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int availableValue in availableValues)
            {
                if (availableValue == 0)
                    items.Add(new ListItem(eResApp.GetRes(Pref, 7191), availableValue.ToString())); // peut réutiliser n'importe quel ancien mot de passe
                else if (availableValue == 1)
                    items.Add(new ListItem(eResApp.GetRes(Pref, 7192), availableValue.ToString())); // ne peut pas réutiliser le mot de passe précédent
                else
                    items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 7193), " ", availableValue, " ", eResApp.GetRes(Pref, 7194)), availableValue.ToString())); // ne peut pas réutiliser les X mots de passe précédents
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion


            #region Nombre d'essaie avant verrouillage
            id = "ddlMaxAttempt";
            label = eResApp.GetRes(Pref, 7777);
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_MAX_CONNEXION_ATTEMPT;
            propKeyCode = (int)propKeyAdvConfig;
            propKeyType = propKeyAdvConfig.GetType();
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_advConfigs[propKeyAdvConfig]);

            availableValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int availableValue in availableValues)
            {
                items.Add(new ListItem(String.Concat(availableValue.ToString()), availableValue.ToString())); // caractères
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion


            #region Nombre d'essaie avant envoie d'alerte (0 pour jamais)
            id = "ddlMaxAttemptWarning";
            label = eResApp.GetRes(Pref, 7778);
            tooltip = eResApp.GetRes(Pref, 8288);
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_FAILED_CONNEXION_THRESHOLD_WARNING;
            propKeyCode = (int)propKeyAdvConfig;
            propKeyType = propKeyAdvConfig.GetType();
            items = new List<ListItem>();
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_advConfigs[propKeyAdvConfig]);

            availableValues = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            foreach (int availableValue in availableValues)
            {
                items.Add(new ListItem(String.Concat(availableValue.ToString()), availableValue.ToString())); // caractères
            }

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
            #endregion





            #region email
            propKeyAdvConfig = eLibConst.CONFIGADV.PASSWORD_POLICIES_FAILED_CONNEXION_WARNING_MAIL;
            AddTextboxOptionField(targetContainer
                  , id: "ddlEmailWarning"
                  , label: eResApp.GetRes(Pref, 7779)
                  , tooltip: eResApp.GetRes(Pref, 7779)
                  , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                  , propKeyCode: (int)propKeyAdvConfig
                  , propKeyType: propKeyAdvConfig.GetType()
                  , currentValue: _advConfigs[propKeyAdvConfig]
                  , adminFieldType: AdminFieldType.ADM_TYPE_CHAR
                  , labelType: eAdminTextboxField.LabelType.INLINE
                  , customTextboxCSSClasses: "optionField optionFieldAdminInputText"
                  , customLabelCSSClasses: "labelField optionField labelAdminNoBold");



            #endregion


        }

        /// <summary>
        /// Génère le contenu de la section "Durée de session" et l'ajoute à la section en question
        /// </summary>
        private void GetSessionPropertiesContents()
        {
            Panel targetContainer = null;
            if (_pnlSessionProperties.Controls.Count > 0 && _pnlSessionProperties.Controls[_pnlSessionProperties.Controls.Count - 1] is Panel)
                targetContainer = (Panel)_pnlSessionProperties.Controls[_pnlSessionProperties.Controls.Count - 1];
            if (targetContainer == null)
                return;

            string id = "ddlSessionDuration";
            string label = eResApp.GetRes(Pref, 572);
            string tooltip = label;
            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.SESSIONTIMEOUT;
            int propKeyCode = (int)propKeyConfig;
            Type propKeyType = propKeyConfig.GetType();
            List<ListItem> items = new List<ListItem>();
            FieldFormat valueFormat = FieldFormat.TYP_NUMERIC;
            eAdminDropdownField.eAdminDropdownFieldRenderType ddlRenderType = eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE;
            string customLabelCSSClasses = "labelField optionField";

            int currentValue = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            int[] availableValues = { 20, 30, 60, 120, 180, 240, 300, 360, 420, 480 }; // durées proposées, en minutes : 20 min / 30 min / 1 h / 2 h / 3 h / 4 h / 5 h / 6 h / 7 h / 8 h
            foreach (int availableValue in availableValues)
                items.Add(new ListItem(eLibTools.GetFormattedTimeStamp(Pref, new TimeSpan(0, availableValue, 0), false), availableValue.ToString()));

            AddDropdownOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, ddlRenderType, customLabelCSSClasses: customLabelCSSClasses, sortItemsByLabel: false);
        }

        /// <summary>
        /// Génère le contenu de la section "Adresses IP autorisées" et l'ajoute à la section en question
        /// </summary>
        private void GetIPRestrictionsContents()
        {
            Panel targetContainer = null;
            if (_pnlIPRestrictions.Controls.Count > 0)
                targetContainer = (Panel)_pnlIPRestrictions.Controls[_pnlIPRestrictions.Controls.Count - 1];
            if (targetContainer == null)
                return;

            // Case à cocher "Activer la vérification"
            string id = "chkIPAccessEnabled";
            string label = eResApp.GetRes(Pref, 508);
            string tooltip = label;
            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.IPAccessEnabled;
            int propKeyCode = (int)propKeyConfig;
            Type propKeyType = propKeyConfig.GetType();
            string customCheckboxLabelCSSClasses = "checkboxField optionField";
            int currentValueInt = eLibTools.GetNum(_defaultConfigs[propKeyConfig]);
            AddCheckboxOptionField(targetContainer, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValueInt == 1, customLabelCSSClasses: customCheckboxLabelCSSClasses);

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.Controls.Add(GetLink("newIPRestriction", "nsAdmin.accessSecurityIPAdd();", eResApp.GetRes(Pref, 7630), eResApp.GetRes(Pref, 510), "plus")); // Nouvelle adresse IP autorisée / Cliquer ici pour ajouter une nouvelle adresse IP
            targetContainer.Controls.Add(p);

            // Génération des données du tableau
            List<string> ipTableColumnHeaders = new List<string>()
            {
                eResApp.GetRes(Pref, 223), // Libellé
                eResApp.GetRes(Pref, 509), // Adresse IP
                eResApp.GetRes(Pref, 1261), // Masque de sous-réseau
                eResApp.GetRes(Pref, 199), // Niveau
                eResApp.GetRes(Pref, 7176) // Groupes et utilisateurs
            };
            List<List<string>> ipTableRowCellLabels = new List<List<string>>();
            List<List<string>> ipTableRowCellLinks = new List<List<string>>();
            List<List<string>> ipTableRowCellTooltips = new List<List<string>>();
            List<List<AttributeCollection>> ipTableRowCellAttributes = new List<List<AttributeCollection>>();
            List<string> ipTableRowLinks = new List<string>();
            List<string> ipTableRowTooltips = new List<string>();
            List<AttributeCollection> ipTableRowAttributes = new List<AttributeCollection>();

            string strError = String.Empty;
            List<eAdminAccessSecurityIPData> ipList = eAdminAccessSecurityIP.GetIPAddresses(Pref, out strError);

            foreach (eAdminAccessSecurityIPData ip in ipList)
            {
                List<string> ipCellLabels = new List<string> {
                    ip.Label,
                    ip.IpAddress,
                    ip.Mask,
                    ip.GetLevelLabel(Pref),
                    ip.GetUserLabel(Pref)
                };
                List<string> ipCellLinks = new List<string> {
                    String.Concat("nsAdmin.accessSecurityIPEditLabel(", ip.IpId, ");"),
                    String.Concat("nsAdmin.accessSecurityIPEditAddress(", ip.IpId, ");"),
                    String.Concat("nsAdmin.accessSecurityIPEditMask(", ip.IpId, ");"),
                    String.Concat("nsAdmin.accessSecurityIPEditLevel(", ip.IpId, ");"),
                    String.Concat("nsAdmin.accessSecurityIPEditUserGroups(", ip.IpId, ");")
                };
                List<string> ipCellTooltips = ipCellLabels; // TODO ?
                List<AttributeCollection> ipCellAttributes = null; // TODO ?

                ipTableRowCellLabels.Add(ipCellLabels);
                ipTableRowCellLinks.Add(ipCellLinks);
                ipTableRowCellTooltips.Add(ipCellTooltips);
                ipTableRowCellAttributes.Add(ipCellAttributes);
            }


            HtmlTable ipTable = GetTable(
                "ipRestrictionsTable",
                ipTableColumnHeaders,
                ipTableRowCellLabels, ipTableRowCellLinks, ipTableRowCellTooltips, ipTableRowCellAttributes,
                ipTableRowLinks, ipTableRowTooltips, ipTableRowAttributes,
                "adminIPTable"
            );
            targetContainer.Controls.Add(ipTable);
        }

        /// <summary>
        /// Génère le contenu de la section "Méthodes d'authentification"
        /// </summary>
        private void GetAuthenticationMethodsContents()
        {
            Panel targetContainer = null;
            if (_pnlAuthenticationMethods.Controls.Count > 0 && _pnlAuthenticationMethods.Controls[_pnlAuthenticationMethods.Controls.Count - 1] is Panel)
                targetContainer = (Panel)_pnlAuthenticationMethods.Controls[_pnlAuthenticationMethods.Controls.Count - 1];
            if (targetContainer == null)
                return;

            // SSO uniquement en intranet et Application configurée en SSO - TODO: A vérifier ?
            // bSsoEnabled = bSsoEnabled && isSSOApplication && isIntranet;
            // cDbToken.IsSSOEnabled = bSsoEnabled || bIsADFSApplication;
            bool saml2 = false;
            int valueLDAP = 0;
            int valueCAS = 0;
            int valueADFS = 0;
            string valueSSO = "False";

            valueSSO = eTools.GetEudologClientsValues(Pref, new eLibConst.EUDOLOG_CLIENTS[] { eLibConst.EUDOLOG_CLIENTS.SSOEnabled })[eLibConst.EUDOLOG_CLIENTS.SSOEnabled];


            LdapLib ldapTools = new LdapLib(Pref);
            ldapTools.loadLdapConfiguration(Pref);
            valueLDAP = ldapTools.Enabled ? 1 : 0;

            valueCAS = eLibTools.GetNum(_advConfigs[eLibConst.CONFIGADV.SSO_CAS]);
            valueADFS = eLibTools.GetNum(eLibTools.GetServerConfig("ADFSApplication", "0"));
            saml2 = eLibTools.GetNum(_advConfigs[eLibConst.CONFIGADV.AUTHENTICATION_MODE]) == (int)eLibConst.AuthenticationMode.SAML2;

            string groupName = "rbAuthenticationMethod";
            string rbOnChange = "nsAdmin.accessSecurityAuthChange(this);";

            string id = String.Concat(groupName, "Eudonet");
            string label = String.Empty;
            string tooltip = eResApp.GetRes(Pref, 7195); // Identification Eudonet
            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            eLibConst.CONFIGADV propKeyConfigadv = eLibConst.CONFIGADV.AUTHENTICATION_MODE;
            int propKeyCode = (int)propKeyConfigadv;
            Type propKeyType = propKeyConfigadv.GetType();
            Dictionary<string, string> items = new Dictionary<string, string>();
            items.Add(((int)eLibConst.AuthenticationMode.EUDONET).ToString(), eResApp.GetRes(Pref, 7195)); // Identification Eudonet
            FieldFormat valueFormat = FieldFormat.TYP_NUMERIC;
            int currentValue = eLibTools.GetNum(_advConfigs[eLibConst.CONFIGADV.AUTHENTICATION_MODE]);
            Control rbAuthenticationMethodEudonet = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodEudonet.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    grandChildControl.Disabled = (valueSSO == "True" || valueADFS == 1);// ce paramètre n'est pas modifiable si on est en authentification SSO ou ADFS ou Saml2



            id = String.Concat(groupName, "SSO");
            label = String.Empty;
            tooltip = eResApp.GetRes(Pref, 7196); // Identification SSO Eudonet
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT; // N/A
            eLibConst.EUDOLOG_CLIENTS propKeyEudoLog = eLibConst.EUDOLOG_CLIENTS.SSOEnabled;
            propKeyCode = (int)propKeyEudoLog;
            propKeyType = propKeyEudoLog.GetType();
            items = new Dictionary<string, string>();
            items.Add("True", eResApp.GetRes(Pref, 7196)); // Identification SSO Eudonet
            valueFormat = FieldFormat.TYP_CHAR;
            string currentValueString = valueSSO;
            Control rbAuthenticationMethodSSO = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValueString, valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodSSO.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    grandChildControl.Disabled = true; // ce paramètre ne peut pas être modifié depuis l'application

            eExtension LdapExtension = eExtension.GetExtensionByCode(Pref, ExtensionCode.LDAP);
            #region LDAP
            id = String.Concat(groupName, "LDAP");
            label = String.Empty;
            if (LdapExtension != null)
                tooltip = eResApp.GetRes(Pref, 7197); // Identification via LDAP
            else
                tooltip = string.Concat(eResApp.GetRes(Pref, 7197), eResApp.GetRes(Pref, 8919));
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPEnabled;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            items = new Dictionary<string, string>();
            if (LdapExtension != null && LdapExtension.Status == EXTENSION_STATUS.STATUS_READY)
                items.Add("1", eResApp.GetRes(Pref, 7197)); // Identification via LDAP
            else
                items.Add("1", string.Concat(eResApp.GetRes(Pref, 7197), eResApp.GetRes(Pref, 8919)));

            valueFormat = FieldFormat.TYP_NUMERIC;
            if (LdapExtension == null)
                valueLDAP = 0;

            currentValue = valueLDAP;

            Control rbAuthenticationMethodLDAP = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodLDAP.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    if (LdapExtension != null && LdapExtension.Status == EXTENSION_STATUS.STATUS_READY)
                        grandChildControl.Disabled = (valueSSO == "True" || valueADFS == 1); // ce paramètre n'est pas modifiable si on est en authentification SSO ou ADFS ou Saml2
                    else
                        grandChildControl.Disabled = true;
            if(LdapExtension != null && LdapExtension.Status == EXTENSION_STATUS.STATUS_READY)
            {
                bool enabled = valueLDAP == 1 && valueCAS != 1 && valueADFS != 1;
                targetContainer.Controls.Add(GetAuthenticationMethodsLDAPContents(enabled, ldapTools));
            }            
            #endregion LDAP

            eExtension CasExtension = eExtension.GetExtensionByCode(Pref, ExtensionCode.CAS);
            
            #region CAS
            id = String.Concat(groupName, "CAS");
            label = String.Empty;
            if (CasExtension != null)
                tooltip = eResApp.GetRes(Pref, 7198); // Identification CAS
            else
                tooltip = string.Concat(eResApp.GetRes(Pref, 7198), eResApp.GetRes(Pref, 8919));

            propCat = eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            eLibConst.CONFIGADV propKeyConfigAdv = eLibConst.CONFIGADV.SSO_CAS;
            propKeyCode = (int)propKeyConfigAdv;
            propKeyType = propKeyConfigAdv.GetType();
            items = new Dictionary<string, string>();
            if (CasExtension != null && CasExtension.Status == EXTENSION_STATUS.STATUS_READY)
                items.Add("1", eResApp.GetRes(Pref, 7198)); // Identification CAS
            else
                items.Add("1", string.Concat(eResApp.GetRes(Pref, 7198), eResApp.GetRes(Pref, 8919)));
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = valueCAS;
            Control rbAuthenticationMethodCAS = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodCAS.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    if (CasExtension != null && CasExtension.Status == EXTENSION_STATUS.STATUS_READY)
                        grandChildControl.Disabled = (valueSSO == "True" || valueADFS == 1);// ce paramètre n'est pas modifiable si on est en authentification SSO ou ADFS ou Saml2
                    else
                        grandChildControl.Disabled = true;
            #endregion CAs

            eExtension AdfsExtension = eExtension.GetExtensionByCode(Pref, ExtensionCode.EUDOADFS);
            bool adfs = eLibTools.GetNum(_advConfigs[eLibConst.CONFIGADV.AUTHENTICATION_MODE]) == (int)eLibConst.AuthenticationMode.ADFS;
            #region ADFS
            id = String.Concat(groupName, "ADFS");
            label = String.Empty;
            if (AdfsExtension != null && AdfsExtension.Status == EXTENSION_STATUS.STATUS_READY)
                tooltip = eResApp.GetRes(Pref, 7199); // Identification ADFS
            else
                tooltip = string.Concat(eResApp.GetRes(Pref, 7199), eResApp.GetRes(Pref, 8919));

            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS; // N/A
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPEnabled; // N/A
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            items = new Dictionary<string, string>();
            if (AdfsExtension != null && AdfsExtension.Status == EXTENSION_STATUS.STATUS_READY)
                items.Add("2", eResApp.GetRes(Pref, 7199)); // Identification ADFS - Valeur factice, non modifiable
            else
                items.Add("2", string.Concat(eResApp.GetRes(Pref, 7199), eResApp.GetRes(Pref, 8919)));
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = valueADFS;
            Control rbAuthenticationMethodADFS = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodADFS.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    if (AdfsExtension != null && AdfsExtension.Status == EXTENSION_STATUS.STATUS_READY)
                        grandChildControl.Disabled = false; 
                    else
                        grandChildControl.Disabled = true;   
            if (AdfsExtension != null && AdfsExtension.Status == EXTENSION_STATUS.STATUS_READY)
               rbAuthenticationMethodADFS.Controls.Add(GetADFSMessage(adfs));
            else if(adfs)
                rbAuthenticationMethodADFS.Controls.Add(GetADFSMessage(adfs));
            #endregion ADFS

            eExtension Saml2Extension = eExtension.GetExtensionByCode(Pref, ExtensionCode.SAML2);
            #region SAML2
            id = String.Concat(groupName, "SAML2");
            label = String.Empty;
            if (Saml2Extension != null && Saml2Extension.Status == EXTENSION_STATUS.STATUS_READY)
                tooltip = eResApp.GetRes(Pref, 1927); //Identification SAML2
            else
                tooltip = string.Concat(eResApp.GetRes(Pref, 1927), eResApp.GetRes(Pref, 8919));
            propCat = eAdminUpdateProperty.CATEGORY.CONFIGADV;
            propKeyConfigadv = eLibConst.CONFIGADV.AUTHENTICATION_MODE;
            propKeyCode = (int)propKeyConfigadv;
            propKeyType = propKeyConfigAdv.GetType();
            items = new Dictionary<string, string>();
            if (Saml2Extension != null && Saml2Extension.Status == EXTENSION_STATUS.STATUS_READY)
                items.Add(((int)eLibConst.AuthenticationMode.SAML2).ToString(), eResApp.GetRes(Pref, 1927));
            else
                items.Add(((int)eLibConst.AuthenticationMode.SAML2).ToString(), string.Concat(eResApp.GetRes(Pref, 1927), eResApp.GetRes(Pref, 8919)));
            valueFormat = FieldFormat.TYP_NUMERIC;
            currentValue = eLibTools.GetNum(_advConfigs[eLibConst.CONFIGADV.AUTHENTICATION_MODE]);
            Control rbAuthenticationMethodSAML2 = AddRadioButtonOptionField(targetContainer, id, groupName, label, tooltip, propCat, propKeyCode, propKeyType, items, currentValue.ToString(), valueFormat, onClick: rbOnChange);
            foreach (HtmlGenericControl childControl in rbAuthenticationMethodSAML2.Controls)
                foreach (HtmlGenericControl grandChildControl in childControl.Controls)
                    if (Saml2Extension != null && Saml2Extension.Status == EXTENSION_STATUS.STATUS_READY)
                        grandChildControl.Disabled = false; // ce paramètre ne peut pas être modifié depuis l'application
                    else
                        grandChildControl.Disabled = true;

            if (Saml2Extension != null && Saml2Extension.Status == EXTENSION_STATUS.STATUS_READY)
                rbAuthenticationMethodSAML2.Controls.Add(GetSAML2Settings(saml2));
            else if (saml2 == true )
                rbAuthenticationMethodSAML2.Controls.Add(GetSAML2Settings(saml2));
            
            #endregion SAML2
        }

        private Panel GetAuthenticationMethodsLDAPContents(bool enabled, LdapLib ldapTools)
        {
            Panel ldapOptions = new Panel();
            ldapOptions.CssClass = "adminLDAPOptions";







            /* AutoCreate */
            string id = "chkAuthenticationMethodLDAPAutoCreate";
            string label = eResApp.GetRes(Pref, 1683); // Création automatique des utilisateurs
            string tooltip = label;
            eAdminUpdateProperty.CATEGORY propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            eLibConst.CONFIG_DEFAULT propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPAutoCreate;
            int propKeyCode = (int)propKeyConfig;
            Type propKeyType = propKeyConfig.GetType();
            string customCheckboxLabelCSSClasses = "checkboxField optionField";
            string checkboxOnClick = String.Concat("if (getAttributeValue(this, 'dis') != '1') { ", DEFAULT_OPTION_ONCHANGE, "};");
            Control control = AddCheckboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, ldapTools.AutoCreate, customLabelCSSClasses: customCheckboxLabelCSSClasses, customPanelCSSClasses: "optionFieldLDAPAutoCreate", onClick: checkboxOnClick);
            ((eCheckBoxCtrl)control).SetDisabled(!enabled);

            /**/
            string groupName = "txtAuthenticationMethodLDAP";
            int nbRows = 0;
            eAdminTextboxField.LabelType labelType = eAdminTextboxField.LabelType.INLINE;
            eAdminDropdownField.eAdminDropdownFieldRenderType dropDownFieldType = eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE;
            AdminFieldType adminFieldType = AdminFieldType.ADM_TYPE_CHAR;
            string prefixText = String.Empty;
            string suffixText = String.Empty;
            Dictionary<string, string> customTextboxStyleAttributes = null;
            Dictionary<string, string> customPanelStyleAttributes = null;
            Dictionary<string, string> customLabelStyleAttributes = null;
            string customTextboxCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";
            string customDropdownLabelCSSClasses = String.Concat("info ", customLabelCSSClasses);
            bool passwordField = false;

            id = "ddlAuthenticationMethodLDAPVersion";
            label = eResApp.GetRes(Pref, 1267).Replace(" <VERSION>", String.Empty); // Version
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAP_PROTOCOL_VERSION;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            List<ListItem> listItems = new List<ListItem>();
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 1267).Replace("<VERSION>", "2"), "2"));
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 1267).Replace("<VERSION>", "3"), "3"));
            listItems[0].Attributes.Add("title", label);
            listItems[1].Attributes.Add("title", label);
            string currentValue = ((int)ldapTools.ProtocolVersion).ToString();
            FieldFormat valueFormat = FieldFormat.TYP_NUMERIC;
            control = AddDropdownOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, listItems, currentValue, valueFormat, dropDownFieldType, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customDropdownLabelCSSClasses, sortItemsByLabel: false);
            ((DropDownList)control).Enabled = enabled;

            id = "chkAuthenticationMethodLDAPS";
            label = "SSL";
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAP_USESSL;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            customCheckboxLabelCSSClasses = "checkboxField optionField";
            checkboxOnClick = String.Concat("if (getAttributeValue(this, 'dis') != '1') { ", DEFAULT_OPTION_ONCHANGE, "};");
            control = AddCheckboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, ldapTools.UseSSL, customLabelCSSClasses: customCheckboxLabelCSSClasses, customPanelCSSClasses: "fieldInline optionFieldLDAPAutoCreate ", onClick: checkboxOnClick);
            ((eCheckBoxCtrl)control).SetDisabled(!enabled);



            id = String.Concat(groupName, "User");
            label = eResApp.GetRes(Pref, 1655); // Utilisateur LDAP
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPBrowseUser;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.BrowserUserLogin;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;


            id = String.Concat(groupName, "Password");
            label = eResApp.GetRes(Pref, 2); // Mot de passe
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPBrowsePwd;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.BrowserUserPassword;
            passwordField = true;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "ServerIP");
            label = eResApp.GetRes(Pref, 1650); // IP du serveur LDAP
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPIPAddressServer;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.LdapServer;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "ServerDN");
            label = eResApp.GetRes(Pref, 1651); // DN du serveur
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPDomainName;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.LdapDN;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "UsersDN");
            label = eResApp.GetRes(Pref, 1656); // DN des utilisateurs
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPUSERDN;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.UserSearchDN;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "AllowedGroup");
            label = eResApp.GetRes(Pref, 1652); // Groupe LDAP autorisé
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPGroupUser; // TODO: type de champ ?
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.UserAllowedGroup;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "LoginField");
            label = eResApp.GetRes(Pref, 7200); // Propriété "Login" dans le LDAP
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPLOGINFIELD;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.LoginField;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "FullNameField");
            label = eResApp.GetRes(Pref, 1684); // Propriété "Nom Complet" dans le LDAP
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPUserFullNamePropertiyName;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.UserFullNamePropertiy;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            id = String.Concat(groupName, "EmailField");
            label = eResApp.GetRes(Pref, 1685); // Propriété "Email" dans le LDAP
            tooltip = label;
            propCat = eAdminUpdateProperty.CATEGORY.LDAPPARAMS;
            propKeyConfig = eLibConst.CONFIG_DEFAULT.LDAPEmailPropertiyName;
            propKeyCode = (int)propKeyConfig;
            propKeyType = propKeyConfig.GetType();
            currentValue = ldapTools.EmailPropertiy;
            passwordField = false;
            control = AddTextboxOptionField(ldapOptions, id, label, tooltip, propCat, propKeyCode, propKeyType, currentValue, adminFieldType, labelType, nbRows, prefixText, suffixText, passwordField, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            ((TextBox)control).Enabled = enabled;

            return ldapOptions;
        }

        /// <summary>
        /// Faire le rendu du paramétrage
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetSAML2Settings(bool samlActive)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");

            string jsonValue;
            eSaml2DatabaseConfig config = GetSamlSettings(li, out jsonValue);

            eAdminSaml2PartRenderer render = new eAdminSaml2PartRenderer(_ePref, config, jsonValue);
            if (render.Generate())
            {
                render.PgContainer.Attributes.Add("data-active", samlActive ? "1" : "0");
                li.Controls.Add(render.PgContainer);
            }
            else
            {
                Panel err = new Panel();
                err.ID = "samlcontainer";
                err.Controls.Add(new LiteralControl(render.ErrorMsg));
                li.Controls.Add(err);
            }

            return li;
        }
        
        private Panel GetADFSMessage(bool active)
        {
            
            Panel ctn = new Panel();
            ctn.CssClass = "field";
            ctn.ID = "AdfsContainer";
            if (active)
                ctn.Attributes.Add("data-active", "1");
            else
                ctn.Attributes.Add("data-active", "0");

            HtmlGenericControl lab = new HtmlGenericControl("label");
            lab.InnerHtml =  eResApp.GetRes(Pref, 8920);
            lab.Attributes.Add("style", "width:400px;font-style: italic;user-select:all;");
            

            ctn.Controls.Add(lab);

            return ctn;

        }

        /// <summary>
        /// Récupération du paramétrage
        /// </summary>
        /// <param name="container"></param>
        /// <param name="jsonValue"></param>
        /// <returns></returns>
        private eSaml2DatabaseConfig GetSamlSettings(HtmlGenericControl container, out string jsonValue)
        {
            jsonValue = _advConfigs[eLibConst.CONFIGADV.AUTHENTICATION_SETTINGS];
            eSaml2DatabaseConfig config = new eSaml2DatabaseConfig();
            try
            {
                config = eSaml2DatabaseConfig.GetConfigOrDefault(_ePref, jsonValue);
            }
            catch (Exception ex)
            {


                if (!string.IsNullOrWhiteSpace(jsonValue))
                {
                    HtmlGenericControl div = new HtmlGenericControl("div");
#if !DEBUG
                    div.Attributes.Add("style", "color:red;display:none;");
#endif
                    div.InnerHtml = "saml2 : Json invalide <br/>" + ex.Message + "<br/>" + ex.StackTrace;
                    container.Controls.Add(div);
                }

                //Config par defaut
                config = eSaml2DatabaseConfig.GetDefault(_ePref);
            }

            // Valeurs par défaut si pas de config
            if (string.IsNullOrWhiteSpace(jsonValue))
                jsonValue = SerializerTools.JsonSerialize(config);

            return config;
        }



    }
}
