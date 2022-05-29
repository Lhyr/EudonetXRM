using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// rendu de l'écran de paramétrage de l'emailing
    /// </summary>
    public class eAdminExtensionExternalMailingRenderer : eAdminExtensionFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;
        private Dictionary<string, string> dicServerAliasDomain;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionExternalMailingRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref, extension, initialTab)
        {

        }

        public static eAdminExtensionExternalMailingRenderer CreateAdminExtensionExternalMailingRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminExtension ext = bNoInternet ? eAdminExtension.initExtensionFromJson(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING, pref) :
              eAdminExtension.InitFromModule(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING, pref, extensionFileIdFromStore);

            return new eAdminExtensionExternalMailingRenderer(pref, ext, initialTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.MAILINGSENDTYPE,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPLOGIN,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPPASSWORD,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPPORT,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPSSL,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPURL,
                        eLibConst.CONFIGADV.EUDOMAILING_FTPUSEPASSIVEMODE,
                        eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN,
                        eLibConst.CONFIGADV.EUDOMAILING_SENDOUTSPEED
                    });

                dicServerAliasDomain = eLibTools.GetConfigAdvPrefAdvValuesFromPrefix(Pref, eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString());

                return true;
            }
            return false;
        }


        private const string customTextboxCSSClasses = "optionField";
        private const string customPanelCSSClasses = "fieldInline";
        private const string customLabelCSSClasses = "labelField optionField";

        protected override bool Build()
        {
            if (base.Build())
            {
                Panel targetPanel = null;

                Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING.ToString(), eResApp.GetRes(Pref, 7790));
                ExtensionParametersContainer.Controls.Add(section);

                if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                    targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
                if (targetPanel == null)
                    return false;

                AddTextboxOptionField(targetPanel, "txtFTPUrl", eResApp.GetRes(Pref, 7793), eResApp.GetRes(Pref, 7803),
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_FTPURL.GetHashCode(), typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPURL], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtFTPPort", eResApp.GetRes(Pref, 7794), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_FTPPORT.GetHashCode(), typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPPORT], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtFTPLogin", eResApp.GetRes(Pref, 7795), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_FTPLOGIN.GetHashCode(), typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPLOGIN], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                var pwd = AddTextboxOptionField(targetPanel, "txtFTPPassword", eResApp.GetRes(Pref, 7796), "",
                        eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_FTPPASSWORD.GetHashCode(), typeof(eLibConst.CONFIGADV),
                        dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPPASSWORD], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                        customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);


                ((TextBox)pwd).TextMode = TextBoxMode.Password;
                ((TextBox)pwd).Attributes.Add("placeholder", "**********");

                AddCheckboxOptionField(targetPanel, "chkPassiveMode", eResApp.GetRes(Pref, 7797), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.EUDOMAILING_FTPUSEPASSIVEMODE, typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPUSEPASSIVEMODE] == "1", customCheckboxCSSClasses: "optionField");

                AddCheckboxOptionField(targetPanel, "chkSSL", eResApp.GetRes(Pref, 7798), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.EUDOMAILING_FTPSSL, typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_FTPSSL] == "1", customCheckboxCSSClasses: "optionField");

                AddTextboxOptionField(targetPanel, "txtSendoutSpeed", eResApp.GetRes(Pref, 7799), eResApp.GetRes(Pref, 7800),
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_SENDOUTSPEED.GetHashCode(), typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_SENDOUTSPEED], EudoQuery.AdminFieldType.ADM_TYPE_NUM, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);


                Panel SenderAliasDomainPanel = new Panel();
                SenderAliasDomainPanel.CssClass = "senderAliasDomainContainer";
                targetPanel.Controls.Add(SenderAliasDomainPanel);

                Panel SenderAliasDomainLinePanel = new Panel();
                SenderAliasDomainLinePanel.CssClass = "senderAliasDomainLineContainer";
                SenderAliasDomainPanel.Controls.Add(SenderAliasDomainLinePanel);

                AddTextboxOptionField(SenderAliasDomainLinePanel, "txtSenderAliasDomain", eResApp.GetRes(Pref, 7801), eResApp.GetRes(Pref, 7802),
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.GetHashCode(), typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                Panel SenderAliasDomainSubPanel = new Panel();
                SenderAliasDomainSubPanel.ID = "senderAliasDomainSubContainer";
                SenderAliasDomainPanel.Controls.Add(SenderAliasDomainSubPanel);

                //CNA - demande #52891 - Nom de domaine partenaire supplémentaire
                foreach (KeyValuePair<string, string> kvp in dicServerAliasDomain)
                {
                    if (kvp.Key == eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString())
                        continue;

                    SenderAliasDomainLinePanel = BuildSenderAliasDomain(Pref, kvp.Key, kvp.Value);
                    SenderAliasDomainSubPanel.Controls.Add(SenderAliasDomainLinePanel);
                }

                SenderAliasDomainLinePanel = new Panel();
                SenderAliasDomainLinePanel.ID = "senderAliasDomainAddSubContainer";
                SenderAliasDomainLinePanel.CssClass = "senderAliasDomainLineContainer";
                SenderAliasDomainPanel.Controls.Add(SenderAliasDomainLinePanel);

                AddButtonOptionField(SenderAliasDomainLinePanel, "btnSenderAliasDomainAdd", eResApp.GetRes(Pref, 18), eResApp.GetRes(Pref, 8220), "nsAdminExternalEmailing.AddServerAliasDomain(this);");

                return true;
            }
            return false;
        }

        public static Panel BuildSenderAliasDomain(ePrefLite pref, string parameter, string value = "")
        {
            string suffix = parameter.Replace(eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString(), "");

            Panel SenderAliasDomainSubPanel = new Panel();
            SenderAliasDomainSubPanel.ID = String.Concat(parameter, "_LineContainer");
            SenderAliasDomainSubPanel.CssClass = "senderAliasDomainLineContainer";

            AddTextboxOptionField(SenderAliasDomainSubPanel, String.Concat("txtSenderAliasDomain", suffix), eResApp.GetRes(pref, 8219), eResApp.GetRes(pref, 8218),
                eAdminUpdateProperty.CATEGORY.CONFIGADV, -1, typeof(eLibConst.CONFIGADV),
                value, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses,
                onChange: String.Concat("nsAdminExternalEmailing.UpdateServerAliasDomain(this, '", parameter, "');"));

            AddButtonOptionField(SenderAliasDomainSubPanel, String.Concat("btnSenderAliasDomain", suffix, "_Delete"), eResApp.GetRes(pref, 19), eResApp.GetRes(pref, 8221), String.Concat("nsAdminExternalEmailing.DeleteServerAliasDomain(this, '", parameter, "');"));

            return SenderAliasDomainSubPanel;
        }
    }
}