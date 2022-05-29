using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu des paramètres de l'extension "Emailings"
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminExtensionFileRenderer" />
    public class eAdminStoreEmailingRenderer : eAdminStoreFileRenderer
    {
        private IDictionary<eLibConst.CONFIG_DEFAULT, string> _dicConfigDefault;
        private IDictionary<eLibConst.CONFIGADV, string> _dicConfigAdv;



        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreEmailingRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Génère un objet de rendu des paramètres de l'extension Emailing
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="ext">The extension file identifier from store.</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public static eAdminStoreEmailingRenderer CreateAdminStoreEmailingRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminStoreEmailingRenderer rdr = new eAdminStoreEmailingRenderer(pref, ext);

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
                _dicConfigDefault = Pref.GetConfigDefault(
                    new HashSet<eLibConst.CONFIG_DEFAULT> {
                        eLibConst.CONFIG_DEFAULT.HIDEMAILREPORT,
                        eLibConst.CONFIG_DEFAULT.EMBEDDEDIMAGESENABLED,
                        eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGAUTHENTICATION,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGUSERNAME,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPASSWORD,
                        eLibConst.CONFIG_DEFAULT.ANTISPOOFINGENABLED,
                        eLibConst.CONFIG_DEFAULT.NOTIFICATIONADDRESS,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPORT,
                        eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSSL,
                        eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME,
                        eLibConst.CONFIG_DEFAULT.EMAILSEPARATOR
                    });

                _dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.DKIM_SIGN_ENABLED, eLibConst.CONFIGADV.DKIM_DOMAIN, eLibConst.CONFIGADV.DKIM_PRIVATE_RSA_KEY, eLibConst.CONFIGADV.DKIM_SELECTOR, eLibConst.CONFIGADV.SPOOFING_EXTENDED_DOMAIN, eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE
                    });

                return true;
            }
            return false;
        }

        

        /// <summary>
        /// Ajoute l'écran de paramétrage de l'extension
        /// </summary>
        /// <returns></returns>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            AddSectionEmailing();

            AddSectionDKIM();

        }

        /// <summary>
        /// Section Emailing
        /// </summary>
        /// <returns></returns>
        Boolean AddSectionEmailing()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING.ToString(), eResApp.GetRes(Pref, 5045));
            ExtensionParametersContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            string customTextboxCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";

            // Ne pas afficher les rapports d'envoi d'emailings
            AddCheckboxOptionField(targetPanel, "chkHideMailReport", eResApp.GetRes(Pref, 5013), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEMAILREPORT.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.HIDEMAILREPORT] == "1");

            // Embarquer les images
            AddCheckboxOptionField(targetPanel, "chkEmbeddedImagesEnabled", eResApp.GetRes(Pref, 6065), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.EMBEDDEDIMAGESENABLED.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.EMBEDDEDIMAGESENABLED] == "1");
            
            // Limiter les lignes à 70 caractères dans le corps HTML des emailings.
            AddCheckboxOptionField(targetPanel, "chkSplittingMailBodyNcharPerLine", eResApp.GetRes(Pref, 2986), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE.GetHashCode(),
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE] == "1");

            // Paramètres du serveur
            Dictionary<string, string> items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 5046));
            items.Add("0", eResApp.GetRes(Pref, 5047));
            AddRadioButtonOptionField(targetPanel, "rbSmtpDefaultParam", "rbSmtpDefaultParam", eResApp.GetRes(Pref, 7911), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), items, _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM] == "1" ? "1" : "0", EudoQuery.FieldFormat.TYP_NUMERIC,
                onClick: "nsAdmin.sendJson(this, false, true);nsAdmin.toggleSMTPServerParams(this);");

            #region Param serveur par défaut

            Panel pnlServerDefault = new Panel();
            pnlServerDefault.ID = "blockServerDefaultParams";
            if (_dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM] != "1")
                pnlServerDefault.Style.Add("display", "none");


            AddTextboxOptionField(pnlServerDefault, "txtSMTPServerName", eResApp.GetRes(Pref, 7755), "",
        eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT),
        _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
        customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            targetPanel.Controls.Add(pnlServerDefault);
            #endregion

            #region Parametres du serveur d'envoi

            Panel pnlServer = new Panel();
            pnlServer.ID = "blockServerParams";
            if (_dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPUSEDEFAULTPARAM] == "1")
                pnlServer.Style.Add("display", "none");

            // Nom du serveur
            AddTextboxOptionField(pnlServer, "txtServername", eResApp.GetRes(Pref, 574), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // Port
            AddTextboxOptionField(pnlServer, "txtPort", eResApp.GetRes(Pref, 1556), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPORT.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPORT], EudoQuery.AdminFieldType.ADM_TYPE_NUM, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            targetPanel.Controls.Add(pnlServer);
            #endregion

            //  Serveur qui nécessite une authentification
            AddCheckboxOptionField(pnlServer, "chkAuthentification", eResApp.GetRes(Pref, 1557), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGAUTHENTICATION.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGAUTHENTICATION] == "1",
                onClick: "nsAdmin.sendJson(this, false, true);eTools.toggleElement('blockAuth');");

            #region Authentification du serveur

            Panel pnlAuth = new Panel();
            pnlAuth.ID = "blockAuth";
            if (_dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGAUTHENTICATION] != "1")
                pnlAuth.Style.Add("display", "none");

            // username
            AddTextboxOptionField(pnlAuth, "txtEmailingUsername", eResApp.GetRes(Pref, 1393), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGUSERNAME.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGUSERNAME], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // password
            int passLen = _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPASSWORD]?.Length ?? 0;
            string fictiveValue = passLen > 0 ? "".PadLeft(passLen, 'X') : "";
            AddTextboxOptionField(pnlAuth, "txtEmailingPassword", eResApp.GetRes(Pref, 2), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGPASSWORD.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), fictiveValue, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                passwordField: true,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            pnlServer.Controls.Add(pnlAuth);
            #endregion


            // SSL
            AddCheckboxOptionField(pnlServer, "chkSSL", "SSL", "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSSL.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSSL] == "1");

            #region Anti spoofing
            // Anti spoofing
            bool antiSpoofingEnabled = _dicConfigDefault[eLibConst.CONFIG_DEFAULT.ANTISPOOFINGENABLED] == "1";

            AddCheckboxOptionField(targetPanel, "chkAntiSpoofing", eResApp.GetRes(Pref, 7910), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.ANTISPOOFINGENABLED.GetHashCode(),
                typeof(eLibConst.CONFIG_DEFAULT), antiSpoofingEnabled,
                onClick: "nsAdmin.sendJson(this, false, true);eTools.toggleElement('blockAntiSpoofing');");

            Panel pnlAntiSpoofing = new Panel();
            pnlAntiSpoofing.ID = "blockAntiSpoofing";
            if (!antiSpoofingEnabled)
                pnlAntiSpoofing.Style.Add("display", "none");

            // Adresse de remplacement
            AddTextboxOptionField(pnlAntiSpoofing, "txtNotificationAddress", eResApp.GetRes(Pref, 8180), eResApp.GetRes(Pref, 8182),
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, (int)eLibConst.CONFIG_DEFAULT.NOTIFICATIONADDRESS,
                typeof(eLibConst.CONFIG_DEFAULT), _dicConfigDefault[eLibConst.CONFIG_DEFAULT.NOTIFICATIONADDRESS], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // Domaine
            AddTextboxOptionField(pnlAntiSpoofing, "txtSpoofingExtendedDomain", eResApp.GetRes(Pref, 8181), eResApp.GetRes(Pref, 8183),
                eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SPOOFING_EXTENDED_DOMAIN,
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.SPOOFING_EXTENDED_DOMAIN], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            targetPanel.Controls.Add(pnlAntiSpoofing);
            #endregion

            return true;
        }

        /// <summary>
        /// Section Mails unitaires
        /// </summary>
        /// <returns></returns>
        Boolean AddSectionEmail()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING.ToString(), eResApp.GetRes(Pref, 7917));
            ExtensionParametersContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            string customTextboxCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";

            AddTextboxOptionField(targetPanel, "txtSMTPServerName", eResApp.GetRes(Pref, 7755), "",
                eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT),
                _dicConfigDefault[eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // Obsolète, n'est plus utilisé
            //AddTextboxOptionField(targetPanel, "txtEmailSeparator", eResApp.GetRes(Pref, 7756), "",
            //    eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.EMAILSEPARATOR.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT),
            //    _dicConfigDefault[eLibConst.CONFIG_DEFAULT.EMAILSEPARATOR], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
            //    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            return true;
        }

        /// <summary>
        /// Section DKIM
        /// </summary>
        /// <returns></returns>
        Boolean AddSectionDKIM()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING.ToString(), eResApp.GetRes(Pref, 7912));
            ExtensionParametersContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            string customTextboxCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";

            // Signature DKIM
            AddCheckboxOptionField(targetPanel, "chkDKIM", eResApp.GetRes(Pref, 7913), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.DKIM_SIGN_ENABLED.GetHashCode(),
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.DKIM_SIGN_ENABLED] == "1", onClick: "nsAdmin.sendJson(this, false, true); eTools.toggleElement('blockDKIMParams');");


            #region Parametres du DKIM

            Panel pnlParams = new Panel();
            pnlParams.ID = "blockDKIMParams";
            if (_dicConfigAdv[eLibConst.CONFIGADV.DKIM_SIGN_ENABLED] != "1")
                pnlParams.Style.Add("display", "none");

            // Sélecteur
            AddTextboxOptionField(pnlParams, "txtSelector", eResApp.GetRes(Pref, 7914), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.DKIM_SELECTOR.GetHashCode(),
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.DKIM_SELECTOR], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // Domaine
            AddTextboxOptionField(pnlParams, "txtDomain", eResApp.GetRes(Pref, 7915), "",
              eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.DKIM_DOMAIN.GetHashCode(),
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.DKIM_DOMAIN], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            // Clé privée
            AddTextboxOptionField(pnlParams, "txtPrivateKey", eResApp.GetRes(Pref, 7916), "",
              eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.DKIM_PRIVATE_RSA_KEY.GetHashCode(),
                typeof(eLibConst.CONFIGADV), _dicConfigAdv[eLibConst.CONFIGADV.DKIM_PRIVATE_RSA_KEY], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, nbRows: 5,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

            targetPanel.Controls.Add(pnlParams);
            #endregion

            return true;
        }
    }
}