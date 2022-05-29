using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using EudoEnum = Com.Eudonet.Common.Enumerations;


namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParameters : eAdminModuleRenderer
    {
        Panel _pnlContents;


        /// <summary>
        /// Type du bloc : orm / avancé
        /// </summary>
        public eUserOptionsModules.USROPT_MODULE OptType { get; private set; }

        private eAdminParameters(ePref pref, int width, int height) : base(pref)
        {
            _width = width;
            _height = height;
        }


        /// <summary>
        /// génère la tuille paramètre
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static eAdminParameters CreateAdminParameters(ePref pref, int width, int height, eUserOptionsModules.USROPT_MODULE type = eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
        {
            eAdminParameters rdr = new eAdminParameters(pref, width, height);
            rdr.OptType = type;
            rdr.Generate();
            return rdr;
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters);
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            InitInnerContainer();

            GetRendererContents();

            return true;
        }

        /// <summary>
        /// On instancie le container 
        /// </summary>
        private void InitInnerContainer()
        {
            _pnlContents = new Panel();
            _pnlContents.CssClass = "adminCntnt adminCntntParameters";
            _pnlContents.Style.Add(HtmlTextWriterStyle.Width, String.Concat(_width, "px"));
            _pnlContents.Style.Add(HtmlTextWriterStyle.Height, String.Concat(_height.ToString(), "px"));
            _pgContainer.Controls.Add(_pnlContents);
        }

        private void GetRendererContents()
        {
            eAdminRenderer rdr = null;
            Control contents = null;

            Dictionary<eLibConst.CONFIG_DEFAULT, String> defaultConfigs = Pref.GetConfigDefault(new List<eLibConst.CONFIG_DEFAULT> {
                eLibConst.CONFIG_DEFAULT.PARENTFILEBROWSERDISABLED,
                eLibConst.CONFIG_DEFAULT.HIDELEFTMENUOPTIONSICON,
                eLibConst.CONFIG_DEFAULT.HideLinkEmailing,
                eLibConst.CONFIG_DEFAULT.HideLinkExport,
                eLibConst.CONFIG_DEFAULT.DISPLAYCURRENTLIST,
                eLibConst.CONFIG_DEFAULT.SEARCHEXTENDED,
                eLibConst.CONFIG_DEFAULT.HIDEPHONETICSEARCHCHECKBOX,
                eLibConst.CONFIG_DEFAULT.SEARCHVIEWDISABLED,
                eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED,
                eLibConst.CONFIG_DEFAULT.UNICODE,
                eLibConst.CONFIG_DEFAULT.SEARCHUNICODEDISABLED,
                eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED,
                eLibConst.CONFIG_DEFAULT.HELPDESKCOPY,
                eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME,
                eLibConst.CONFIG_DEFAULT.EMAILSEPARATOR,
                eLibConst.CONFIG_DEFAULT.EXPORTMODE,
                eLibConst.CONFIG_DEFAULT.LOGONAME,
                eLibConst.CONFIG_DEFAULT.SMTPFEEDBACK,
                eLibConst.CONFIG_DEFAULT.MRUMODE
            });
            IDictionary<EudoEnum.CONFIGADV, String> advConfigs = eLibTools.GetConfigAdvValues(Pref,
                EudoEnum.CONFIGADV.CULTUREINFO,
                EudoEnum.CONFIGADV.NUMBER_SECTIONS_DELIMITER,
                EudoEnum.CONFIGADV.NUMBER_DECIMAL_DELIMITER,
                EudoEnum.CONFIGADV.CALENDAR_DAY_ITEM_WIDTH,
                EudoEnum.CONFIGADV.CREATE_PP_WITHOUT_ADR,
                EudoEnum.CONFIGADV.FEEDBACK_MAIL_ADDRESS,
                EudoEnum.CONFIGADV.THUMBNAIL_ENABLED,
                EudoEnum.CONFIGADV.ORM_URL,
                EudoEnum.CONFIGADV.ORM_EUDO_SPECIF,
                EudoEnum.CONFIGADV.ORM_EUDO_EXT,
                EudoEnum.CONFIGADV.AUTO_COMPLETION_ADR_PROVIDER,
                EudoEnum.CONFIGADV.PREDICTIVEADDRESSESREF,
                EudoEnum.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD,
                EudoEnum.CONFIGADV.THOUSANDS_SEP_DISABLED,
                EudoEnum.CONFIGADV.FULL_UNICODE,
                EudoEnum.CONFIGADV.COUNTRY,
                EudoEnum.CONFIGADV.DEFAULT_CURRENCY,
                EudoEnum.CONFIGADV.DEFAULT_CURRENCY_POSITION,
                EudoEnum.CONFIGADV.ORM_ADMIN_EXT
            );

            if (OptType != eUserOptionsModules.USROPT_MODULE.ADMIN_ORM)
            {

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters_Logo))
                {
                    rdr = eAdminParametersLogoNameRenderer.CreateAdminParametersLogoNameRenderer(Pref, defaultConfigs);
                    contents = (rdr != null ? ((eAdminParametersLogoNameRenderer)rdr).GetContents() : null);
                    _pnlContents.Controls.Add(contents);
                }

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters_Navigation))
                {
                    rdr = eAdminParametersNavigationRenderer.CreateAdminParametersNavigationRenderer(Pref, defaultConfigs);
                    contents = (rdr != null ? ((eAdminParametersNavigationRenderer)rdr).GetContents() : null);
                    _pnlContents.Controls.Add(contents);
                }

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters_Localization))
                {
                    rdr = eAdminParametersLocalizationRenderer.CreateAdminParametersLocalizationRenderer(Pref, defaultConfigs, advConfigs);
                    contents = (rdr != null ? rdr.GetContents() : null);
                    _pnlContents.Controls.Add(contents);
                }

                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters_Supervision))
                {
                    rdr = eAdminParametersSupervisionRenderer.CreateAdminParametersSupervisionRenderer(Pref, defaultConfigs, advConfigs);
                    contents = (rdr != null ? ((eAdminParametersSupervisionRenderer)rdr).GetContents() : null);
                    _pnlContents.Controls.Add(contents);
                }
            }

            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminParameters_Advanced))
            {
                rdr = eAdminParametersAdvRenderer.CreateAdminParametersAdvRenderer(Pref, advConfigs, OptType);
                contents = (rdr != null ? ((eAdminParametersAdvRenderer)rdr).GetContents() : null);
                _pnlContents.Controls.Add(contents);
            }

            if (rdr == null || contents == null)
            {
                _sErrorMsg = "Renderer de module admin non implémenté";
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT; // TODO/TOCHECK
                _eException = null;
            }

            else if (rdr.ErrorMsg.Length > 0)
            {
                _sErrorMsg = rdr.ErrorMsg;
                _nErrorNumber = rdr.ErrorNumber;
                _eException = rdr.InnerException;
            }
            else
            {
                AddCallBackScript(rdr.GetCallBackScript);

                _pnlContents.Controls.Add(contents);
                DicContent["MainContent"] = new Xrm.eRenderer.Content()
                {
                    Ctrl = contents,
                    CallBackScript = GetCallBackScript
                };
            }
        }
    }
}