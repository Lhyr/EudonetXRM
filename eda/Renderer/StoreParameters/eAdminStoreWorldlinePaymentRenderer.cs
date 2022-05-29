using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.Payment;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm.eda.Renderer.StoreParameters
{
    /// <summary>
    /// Renderer du module d'administration de l'extension Paiement Worldline
    /// </summary>
    public class eAdminStoreWorldlinePaymentRenderer : eAdminStoreFileRenderer
    {
        const string sCurrencyCodesList = "https://documentation.sips.worldline.com/fr/WLSIPS.001-GD-Dictionnaire-des-donnees.html#Sips.001_DD_fr-Value-currencyCode";
        private IDictionary<eLibConst.EXTENSION, string> dicExtension;
        private IDictionary<eLibConst.SERVERCONFIG, string> dicServerConfig;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        private eAdminStoreWorldlinePaymentRenderer(ePref pref, eAdminExtension ext)
            : base(pref, ext)
        {

        }

        /// <summary>
        /// Génération du renderer de paramètres de l'extension Paiement Worldline
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="ext">Extension sur l'EudoStore (HotCom)</param>
        /// <returns>Le renderer</returns>
        public static eAdminStoreWorldlinePaymentRenderer CreateAdminStoreWordlinePaymentRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreWorldlinePaymentRenderer rdr = new eAdminStoreWorldlinePaymentRenderer(pref, ext);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                dicServerConfig = eLibTools.GetServerConfigValues(Pref, eLibConst.SERVERCONFIG.WORLDLINE_PAYMENT_SETTINGS);

                //récupérer la config de WORLDLINE à partir de la table Extension
                dicExtension = eLibTools.GetExtensionValues(Pref, new HashSet<eLibConst.EXTENSION> { eLibConst.EXTENSION.WORLDLINECORE });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Build des params
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
               // AddCallBackScript("nsAdmin.addScript('eWorldlinePaymentAdmin', 'ADMIN_STORE');", true);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Création du panneau de Paramètres
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            Panel sectionTitleContainer = new Panel();
            sectionTitleContainer.Attributes.Add("class", "mandatoryParamWarning");
            HtmlGenericControl sectionTitle = new HtmlGenericControl();
            sectionTitle.Attributes.Add("class", "mandatoryParamTitle");
            sectionTitle.InnerText = eResApp.GetRes(_ePref, 2329);
            sectionTitleContainer.Controls.Add(sectionTitle);
            ExtensionParametersContainer.Controls.Add(sectionTitleContainer);

            Panel targetPanel = null;
            Panel section = GetModuleSection(Extension.Module.ToString(), eResApp.GetRes(_ePref, 8786));

            ExtensionParametersContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return;

            #region Paramètres de l'extension

            string customTextboxCSSClasses = "optionField";
            string customLabelCSSClasses = "labelField optionField";

            if (Extension is eAdminExtensionWorldlinePayment)
            {
                eWorldlinePaymentSetting _wlSettings = new eWorldlinePaymentSetting(Pref);

                if (dicExtension.ContainsKey(eLibConst.EXTENSION.WORLDLINECORE) && !string.IsNullOrEmpty(dicExtension[eLibConst.EXTENSION.WORLDLINECORE]))
                {
                    try
                    {
                        string infoConfig = dicExtension[eLibConst.EXTENSION.WORLDLINECORE];
                        _wlSettings = SerializerTools.JsonDeserialize<eWorldlinePaymentSetting>(infoConfig);
                        _wlSettings.DatabaseName = Pref.GetBaseName;

                        var t = new { CryptedSecretKey = "" };
                        var r = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(infoConfig, t);
                        _wlSettings.CryptedSecretKey = r.CryptedSecretKey;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(eResApp.GetRes(Pref, 8790), ex);
                    }
                }

                //Merchant ID / PSPID
                AddTextboxOptionField(targetPanel, "txtPSPID", eResApp.GetRes(_ePref, 8778) + '/' + eResApp.GetRes(_ePref, 8814), "PSPId",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.PSPID, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.PSPId, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses, mandatory: true);

                //Nom du marchand / société
                AddTextboxOptionField(targetPanel, "txtMerchantName", eResApp.GetRes(Pref, 8779), "MerchantName",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.MERCHANTNAME, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.MerchantName, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);

                //Clé API
                AddTextboxOptionField(targetPanel, "txtApiKey", eResApp.GetRes(Pref, 8813), "ApiKey",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.APIKEY, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.ApiKey, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses, mandatory: true);

                //Clé secrète
                AddTextboxOptionField(targetPanel, "txtSecretKey", eResApp.GetRes(Pref, 8780), "SecretKey",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.SECRETKEY, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.SecretKey, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, passwordField: true,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses, mandatory: true);

                //API EndPoint
                AddTextboxOptionField(targetPanel, "txtApiEndPoint", eResApp.GetRes(Pref, 8815), "ApiEndPoint",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.APIENDPOINT, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.ApiEndPoint, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses, mandatory: true);

                //Currency Code
                AddTextboxOptionField(targetPanel, "txtCurrencyCode", eResApp.GetRes(Pref, 8781), "CurrencyCode",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.CURRENCYCODE, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.CurrencyCode, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses);


                //Email a contacter
                AddTextboxOptionField(targetPanel, "txtCurrencyCode", "Adresses mail à contacter", "EmailToContact",
                    eAdminUpdateProperty.CATEGORY.WORLDLINE_PAYMENT_SETTINGS, (int)eWorldlinePaymentSetting.WORLDLINE_PARAMS.LST_MAILSALERT, typeof(eLibConst.CONFIG_DEFAULT),
                    _wlSettings.LstMailAlert, EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                    customLabelCSSClasses: customLabelCSSClasses, customTextboxCSSClasses: customTextboxCSSClasses, mandatory: true);
            }
            #endregion
        }
    }
}