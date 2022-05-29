using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using EudoEnum = Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParametersSupervisionRenderer : eAdminModuleRenderer
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, String> _configs;
        IDictionary<EudoEnum.CONFIGADV, String> _advConfigs;

        private eAdminParametersSupervisionRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs, IDictionary<EudoEnum.CONFIGADV, String> advConfigs) : base(pref)
        {
            _configs = configs;
            _advConfigs = advConfigs;
        }

        /// <summary>
        /// génère le paragraphe de paramétrage de la supervision
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="configs"></param>
        /// <param name="advConfigs"></param>
        /// <returns></returns>
        public static eAdminParametersSupervisionRenderer CreateAdminParametersSupervisionRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs, IDictionary<EudoEnum.CONFIGADV, String> advConfigs)
        {
            eAdminParametersSupervisionRenderer rdr = new eAdminParametersSupervisionRenderer(pref, configs, advConfigs);
            rdr.Generate();
            return rdr;
        }

        protected override bool Build()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL_SUPERVISION.ToString(), "Supervision");
            PgContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            String value = String.Empty;

            Dictionary<String, String> items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 58));
            items.Add("0", eResApp.GetRes(Pref, 59));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED];

            AddRadioButtonOptionField(targetPanel, "rbFeedbackEnabled", "rbFeedbackEnabled", eResApp.GetRes(Pref, 7752), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            AddTextboxOptionField(targetPanel, "txtHelpDeskCopy", eResApp.GetRes(Pref, 7753), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HELPDESKCOPY.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), _configs[eLibConst.CONFIG_DEFAULT.HELPDESKCOPY], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField");

            var t = AddTextboxOptionField(targetPanel
                , id: "txtFeedbackMailAddress"
                , label: eResApp.GetRes(Pref, 7754)
                , tooltip: ""
                , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                , propKeyCode: EudoEnum.CONFIGADV.FEEDBACK_MAIL_ADDRESS.GetHashCode()
                , propKeyType: typeof(EudoEnum.CONFIGADV)
                , currentValue: String.IsNullOrEmpty(_advConfigs[EudoEnum.CONFIGADV.FEEDBACK_MAIL_ADDRESS]) ? eLibConst.DEFAULT_FEEDBACK_MAIL_ADDRESS : _advConfigs[EudoEnum.CONFIGADV.FEEDBACK_MAIL_ADDRESS]
                , adminFieldType: EudoQuery.AdminFieldType.ADM_TYPE_MAIL
                , labelType: eAdminTextboxField.LabelType.INLINE
                , customTextboxCSSClasses: "optionField"
                , onChange: "nsAdmin.onChangeGenericAction(this,false,true);");
            if (t != null)
                ((TextBox)t).Attributes["lastvalid"] = ((TextBox)t).Text;

            //Demande #65623 - Le smtp de feedback n'est plus utilisé en XRM, on masque cette configuration
            //AddTextboxOptionField(targetPanel, "txtSMTPFeedback", eResApp.GetRes(Pref, 7755), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SMTPFEEDBACK.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), _configs[eLibConst.CONFIG_DEFAULT.SMTPFEEDBACK], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField");

            return true;

        }
    }
}