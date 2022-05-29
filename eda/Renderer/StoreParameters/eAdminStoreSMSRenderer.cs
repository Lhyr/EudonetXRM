using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Web.UI;
using Com.Eudonet.Internal.Sms;
using System;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// écran de paramétrage
    /// </summary>
    public class eAdminStoreSMSRenderer : eAdminStoreFileRenderer
    {
        private IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv;
        private IDictionary<eLibConst.EXTENSION, string> dicExtension;


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreSMSRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Création du module d'administration du module d'envoi de SMS
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="ext">extension (pour récupération informations)</param>
        public static eAdminStoreSMSRenderer CreateAdminStoreSMSRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreSMSRenderer rdr = new eAdminStoreSMSRenderer(pref, ext);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                dicConfigAdv = eLibTools.GetConfigAdvValues(Pref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.SMS_CLIENT_ID,
                        eLibConst.CONFIGADV.SMS_MAIL_FROM,
                        eLibConst.CONFIGADV.SMS_MAIL_TO,
                        eLibConst.CONFIGADV.SMS_SERVER_ENABLED
                    });
                //AABBA recuperer les config de netmessage  à partir de la table extension
                dicExtension = eLibTools.GetExtensionValues(Pref,
                   new HashSet<eLibConst.EXTENSION> { eLibConst.EXTENSION.SMS_NETMESSAGE });

                if (dicExtension.Count != 0)
                    dicConfigAdv.Add(eLibConst.CONFIGADV.SMS_NETMESSAGE, dicExtension.First().Value);
                else
                    dicConfigAdv.Add(
                         eLibConst.CONFIGADV.SMS_NETMESSAGE, eLibTools.GetConfigAdvValues(Pref,
                         new HashSet<eLibConst.CONFIGADV> {
                            eLibConst.CONFIGADV.SMS_SETTINGS }).ToString()
                         );

                return true;
            }
            return false;
        }

        /// <summary>
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            Panel targetPanel = null;
            Panel section = GetModuleSection(Extension.Module.ToString(), eResApp.GetRes(Pref, 7757));

            ExtensionParametersContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return;

            string customTextboxCSSClasses = "optionField";
            string customPanelCSSClasses = "fieldInline";
            string customLabelCSSClasses = "labelField optionField";

            if (Extension is eAdminExtensionSMSNetMessage)
            {
                SmsNetMessageSettingsClient netMessageSettings = new SmsNetMessageSettingsClient(Pref);

                if (dicConfigAdv.ContainsKey(eLibConst.CONFIGADV.SMS_NETMESSAGE) && !string.IsNullOrEmpty(dicConfigAdv[eLibConst.CONFIGADV.SMS_NETMESSAGE]))
                {
                    try
                    {
                        string infoConfig = dicConfigAdv[eLibConst.CONFIGADV.SMS_NETMESSAGE];
                        netMessageSettings = SerializerTools.JsonDeserialize<SmsNetMessageSettingsClient>(infoConfig);
                        netMessageSettings.DatabaseName = Pref.GetBaseName;
                        //#82 152
                        var t = new { CryptedApiKeyRest = "" };
                        var r = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(infoConfig, t);
                        netMessageSettings.CryptedApiKeyRest = r.CryptedApiKeyRest;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Echec de récupération des paramètres de l'extension SMS LinkMobility. ", ex);
                    }
                }

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgSenderID", eResApp.GetRes(Pref, 2232), "SenderID",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.SENDERID, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.SenderId, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgUsername", eResApp.GetRes(Pref, 2233), "Username",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.USERNAME, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.Username, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgUserPassword", eResApp.GetRes(Pref, 2), "Password",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.PASSWORD, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.Password, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    passwordField: true,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgServer", eResApp.GetRes(Pref, 2234), "Server",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.SERVER, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.Server, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgAccountName", eResApp.GetRes(Pref, 2235), "AccountName",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.ACCOUNTNAME, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.AccountName, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgAccountKey", eResApp.GetRes(Pref, 2236), "AccountKey",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.ACCOUNTKEY, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.AccountKey, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgApiKeyRest", eResApp.GetRes(Pref, 2499), "ApiKeyRest",
                    eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.APIKEYREST, typeof(eLibConst.CONFIG_DEFAULT),
                    netMessageSettings.ApiKeyRest, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    passwordField: true,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgServerApiRest", eResApp.GetRes(Pref, 2500), "ServerApiRest",
                  eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.SERVERAPIREST, typeof(eLibConst.CONFIG_DEFAULT),
                  netMessageSettings.ServerApiRest, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                  customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSNetMsgIdAccountApiRest", eResApp.GetRes(Pref, 2501), "IdAccountApiRest",
                  eAdminUpdateProperty.CATEGORY.SMSNETMESSAGE_SETTINGS, (int)SmsNetMessageSettingsClient.SMS_PARAMS.IDACCAOUNTAPIREST, typeof(eLibConst.CONFIG_DEFAULT),
                  netMessageSettings.IdAccountApiRest, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                  customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);
            }
            else if (Extension is eAdminExtensionSMS)
            {
                AddTextboxOptionField(targetPanel, "txtSMSMailFrom", eResApp.GetRes(Pref, 7760), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SMS_MAIL_FROM, typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_FROM], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSMailTo", eResApp.GetRes(Pref, 7761), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SMS_MAIL_TO, typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_TO], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                AddTextboxOptionField(targetPanel, "txtSMSClientId", eResApp.GetRes(Pref, 7762), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SMS_CLIENT_ID, typeof(eLibConst.CONFIGADV),
                    dicConfigAdv[eLibConst.CONFIGADV.SMS_CLIENT_ID], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customPanelCSSClasses: customPanelCSSClasses, customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);
            }

        }
    }
}
