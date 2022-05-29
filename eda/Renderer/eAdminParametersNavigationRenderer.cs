using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.report;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParametersNavigationRenderer : eAdminModuleRenderer
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, String> _configs;

        private eAdminParametersNavigationRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs) : base(pref)
        {
            _configs = configs;

        }

        public static eAdminParametersNavigationRenderer CreateAdminParametersNavigationRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs)
        {
            eAdminParametersNavigationRenderer rdr = new eAdminParametersNavigationRenderer(pref, configs);
            rdr.Generate();
            return rdr;
        }

        protected override bool Build()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL_NAVIGATION.ToString(), eResApp.GetRes(Pref, 7986));
            PgContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            String value = String.Empty;
            Dictionary<String, String> items;

            // ParentFileBrowserDisabled
            // Masqué pour l'instant
            //items = new Dictionary<string, string>();
            //items.Add("1", "La fiche du parent cliqué s'ouvre automatiquement");
            //items.Add("0", "La liste de toutes les fiches de l'onglet parent s'ouvre");
            //value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.PARENTFILEBROWSERDISABLED]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.PARENTFILEBROWSERDISABLED];
            //AddRadioButtonOptionField(targetPanel, "rbParentFileBrowser", "rbParentFileBrowser", "Souhaitez-vous que, lorsqu'un utilisateur est sur une fiche et qu'il clique sur l'onglet de l'un des parents de la fiche,", "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.PARENTFILEBROWSERDISABLED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT); 

            // Masqué pour l'instant
            //items = new Dictionary<string, string>();
            //items.Add("0", "Affiché pour tous les utilisateurs");
            //items.Add("1", "Masqué pour tous les utilisateurs");
            //value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.HIDELEFTMENUOPTIONSICON]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.HIDELEFTMENUOPTIONSICON];
            //AddRadioButtonOptionField(targetPanel, "rbHideLeftMenuOptionsIcon", "rbHideLeftMenuOptionsIcon", "Souhaitez-vous que \"mon Eudonet\", situé dans le bandeau droit, soit ", "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDELEFTMENUOPTIONSICON.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("0", eResApp.GetRes(Pref, 7736));
            items.Add("1", eResApp.GetRes(Pref, 7737));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.HideLinkEmailing]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.HideLinkEmailing];
            AddRadioButtonOptionField(targetPanel, "rbHideLinkEmailing", "rbHideLinkEmailing", eResApp.GetRes(Pref, 7738), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HideLinkEmailing.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("0", eResApp.GetRes(Pref, 7736));
            items.Add("1", eResApp.GetRes(Pref, 7737));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.HideLinkExport]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.HideLinkExport];
            AddRadioButtonOptionField(targetPanel, "rbHideLinkExport", "rbHideLinkExport", eResApp.GetRes(Pref, 7739), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HideLinkExport.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 58));
            items.Add("0", eResApp.GetRes(Pref, 59));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.DISPLAYCURRENTLIST]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.DISPLAYCURRENTLIST];
            AddRadioButtonOptionField(targetPanel, "rbDisplayCurrentList", "rbDisplayCurrentList", eResApp.GetRes(Pref, 7740), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.DISPLAYCURRENTLIST.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);


            //Demande #66805 - On masque cette option car elle gérée par utilisateur dans Mon Eudonet
            /*
            items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 58));
            items.Add("0", eResApp.GetRes(Pref, 59));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.MRUMODE]) ? "1" : _configs[eLibConst.CONFIG_DEFAULT.MRUMODE];
            AddRadioButtonOptionField(targetPanel, "rbMruMode", "rbMruMode", eResApp.GetRes(Pref, 7985), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.MRUMODE.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);
            */

            List<ListItem> itemsList = new List<ListItem>();
            itemsList.Add(new ListItem(eResApp.GetRes(Pref, 6256), ExportMode.EXPORT_STANDARD.GetHashCode().ToString()));
            itemsList.Add(new ListItem(eResApp.GetRes(Pref, 6257), ExportMode.EXPORT_MAIL_ONLY.GetHashCode().ToString()));
            itemsList.Add(new ListItem(eResApp.GetRes(Pref, 6258), ExportMode.EXPORT_CHOICE.GetHashCode().ToString()));
            //itemsList.Add(new ListItem(eResApp.GetRes(Pref, 7741), ExportMode.EXPORT_SCHEDULE.GetHashCode().ToString()));
            AddDropdownOptionField(targetPanel, "ddlExportMode", eResApp.GetRes(Pref, 6255), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.EXPORTMODE.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), itemsList, _configs[eLibConst.CONFIG_DEFAULT.EXPORTMODE], EudoQuery.FieldFormat.TYP_NUMERIC, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customLabelCSSClasses: "info", sortItemsByLabel: false);

            items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 7742));
            items.Add("0", eResApp.GetRes(Pref, 7743));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.SEARCHEXTENDED]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.SEARCHEXTENDED];
            AddRadioButtonOptionField(targetPanel, "rbSearchExtended", "rbSearchExtended", eResApp.GetRes(Pref, 7744), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SEARCHEXTENDED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("0", eResApp.GetRes(Pref, 7746));
            items.Add("1", eResApp.GetRes(Pref, 7747));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.HIDEPHONETICSEARCHCHECKBOX]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.HIDEPHONETICSEARCHCHECKBOX];
            // #66 396 - Infobulle complémentaire explicitant le comportement de l'optioon
            string complementaryTip = "";
            // 1916 : La recherche phonétique permet d'obtenir des résultats phonétiquement proches de la valeur recherchée. Exemple : "Joli" et "Joly"
            System.Web.UI.HtmlControls.HtmlGenericControl c = new System.Web.UI.HtmlControls.HtmlGenericControl();
            c.Attributes.Add("class", "icon-question-circle-o");
            c.Attributes.Add("title", eResApp.GetRes(Pref, 1916));
            complementaryTip = String.Concat("&nbsp;", eRenderer.RenderControl(c));
            AddRadioButtonOptionField(targetPanel, "rbHidePhoneticSearchCheckbox", "rbHidePhoneticSearchCheckbox", String.Concat(eResApp.GetRes(Pref, 7745), complementaryTip), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.HIDEPHONETICSEARCHCHECKBOX.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("0", eResApp.GetRes(Pref, 7749));
            items.Add("1", eResApp.GetRes(Pref, 7750));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.SEARCHVIEWDISABLED]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.SEARCHVIEWDISABLED];
            AddRadioButtonOptionField(targetPanel, "rbSearchViewDisabled", "rbSearchViewDisabled", eResApp.GetRes(Pref, 7748), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.SEARCHVIEWDISABLED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            items = new Dictionary<string, string>();
            items.Add("1", eResApp.GetRes(Pref, 58));
            items.Add("0", eResApp.GetRes(Pref, 59));
            value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED];
            AddRadioButtonOptionField(targetPanel, "rbTooltiptextenabled", "rbTooltiptextenabled", eResApp.GetRes(Pref, 7751), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.TOOLTIPTEXTENABLED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            PgContainer.Controls.Add(section);

            return true;
        }
    }
}