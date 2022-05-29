using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using static Com.Eudonet.Internal.eLibConst;
using Com.Eudonet.Core.Model;
using System.Linq;
using Com.Eudonet.Engine.ORM;
using EudoEnum = Com.Eudonet.Common.Enumerations;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParametersLocalizationRenderer : eAdminModuleRenderer
    {
        Dictionary<eLibConst.CONFIG_DEFAULT, String> _configs;
        IDictionary<EudoEnum.CONFIGADV, String> _configsADV;
        private eAdminParametersLocalizationRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs, IDictionary<EudoEnum.CONFIGADV, String> configsADV) : base(pref)
        {
            _configs = configs;
            _configsADV = configsADV;
        }

        public static eAdminParametersLocalizationRenderer CreateAdminParametersLocalizationRenderer(ePref pref, Dictionary<eLibConst.CONFIG_DEFAULT, String> configs, IDictionary<EudoEnum.CONFIGADV, String> configsADV)
        {
            eAdminParametersLocalizationRenderer rdr = new eAdminParametersLocalizationRenderer(pref, configs, configsADV);
            rdr.Generate();
            return rdr;
        }

        protected override bool Build()
        {
            Panel targetPanel = null;

            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL_LOCALIZATION.ToString(), eResApp.GetRes(Pref, 7763));
            PgContainer.Controls.Add(section);

            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            String value = String.Empty;
            Dictionary<String, String> items;
            List<ListItem> listItems;

            #region pays de référence de la base

            listItems = new List<ListItem>();
            List<eISOCountry> countries = eISOCountry.GetCountries(Pref).OrderBy(c1 => c1.Label).ToList();

            foreach (eISOCountry country in countries)
            {
                ListItem item = new ListItem(country.Label, country.Id.ToString());
                listItems.Add(item);
                eAdminCapsule<eAdminUpdateProperty> caps = new Internal.eda.eAdminCapsule<eAdminUpdateProperty>();
                caps.ListProperties.Add(new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.CONFIGADV,
                        Property = (int)EudoEnum.CONFIGADV.DEFAULT_CURRENCY,
                        Value = country.CurrencyCode
                    }
                );

                item.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
            }

            value = _configsADV[EudoEnum.CONFIGADV.COUNTRY];
            listItems.Insert(0, new ListItem(eResApp.GetRes(Pref, 2474), ""));
            AddDropdownOptionField(targetPanel, "ddlCountry", eResApp.GetRes(Pref, 6104), "", eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.COUNTRY.GetHashCode(),
                     typeof(EudoEnum.CONFIGADV), listItems, value, EudoQuery.FieldFormat.TYP_NUMERIC, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customLabelCSSClasses: "info",
                     sortItemsByLabel: false);




            #endregion

            // Référentiel d'adresses postales
            // Le serveur doit pouvoir se connecter à internet
            // L'extension "Cartographie" doit être activée
            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Maps_InternationalPredictiveAddr)
                && eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "0"
                && eExtension.IsReady(Pref, ExtensionCode.CARTOGRAPHY))
            {
                listItems = new List<ListItem>();
                listItems.Add(new ListItem("Data.gouv.fr", eConst.PredictiveAddressesRef.OpenDataGouvFr.GetHashCode().ToString()));
                listItems.Add(new ListItem("Microsoft Bing Maps V8", eConst.PredictiveAddressesRef.BingMapsV8.GetHashCode().ToString()));
                value = String.IsNullOrEmpty(_configsADV[EudoEnum.CONFIGADV.PREDICTIVEADDRESSESREF]) ? "0" : _configsADV[EudoEnum.CONFIGADV.PREDICTIVEADDRESSESREF];
                AddDropdownOptionField(targetPanel, "ddlRefPostalAddresses", eResApp.GetRes(Pref, 8285), "", eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.PREDICTIVEADDRESSESREF.GetHashCode(),
                    typeof(EudoEnum.CONFIGADV), listItems, value, EudoQuery.FieldFormat.TYP_NUMERIC, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, customLabelCSSClasses: "info");
            }





            //Modification comportement internationnal
            if (_ePref.User.UserLevel > 99)
            {

                //Check version ORM
                int nVersionORM = 0;
                try
                {
                    //OrmMappingInfo oi = eLibTools.OrmLoadAndGetMapAdv(_ePref, new OrmGetParams() { });
                    OrmMappingInfo oi = eLibTools.OrmLoadAndGetMapWeb(_ePref);
                    nVersionORM = oi.MinVersion;
                }
                catch
                {
                    //En cas d'erreur, on considère que l'ORM n'est pas compatible
                }


                //ORM >= V3 : Mode FULL UNICODE
                if (nVersionORM >= 3)
                {
                    if (_configsADV.ContainsKey(EudoEnum.CONFIGADV.FULL_UNICODE))
                        value = String.IsNullOrEmpty(_configsADV[EudoEnum.CONFIGADV.FULL_UNICODE]) ? "0" : _configsADV[EudoEnum.CONFIGADV.FULL_UNICODE];
                    else
                        value = "0";

                    // Fullunicode sur tous les champs
                    items = new Dictionary<string, string>();
                    items.Add("1", eResApp.GetRes(Pref, 7731));
                    items.Add("0", eResApp.GetRes(Pref, 59));



                    var ctrG = AddRadioButtonOptionField(targetPanel,
                        "rbFullUnicode",
                        "rbFullUnicode",
                         eResApp.GetRes(_ePref, 2481),
                        "",
                        eAdminUpdateProperty.CATEGORY.CONFIGADV,
                        (int)EudoEnum.CONFIGADV.FULL_UNICODE,
                        typeof(EudoEnum.CONFIGADV), items, value, EudoQuery.FieldFormat.TYP_BIT);

                    //Mise en conformité ORM : relance la mise à jour des champ text => (n)varchar et varchar<=>nvarchar
                    HyperLink crl = (HyperLink)AddButtonOptionField(LastPanel ?? targetPanel,
                          "btnOrmUpgrade",
                        eResApp.GetRes(_ePref, 2944),
                          "",
                          "nsAdmin.ormUpgrade()");


                }
                else
                {

                    // Finalement, Gardé pour les version ORM< 3
                    // OBSOLETE suite à US 1131 
                    // Caractères internationaux dans les rubriques texte
                    items = new Dictionary<string, string>();
                    items.Add("1", eResApp.GetRes(Pref, 7731));
                    items.Add("0", eResApp.GetRes(Pref, 59));
                    value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.UNICODE]) ? "0" : _configs[eLibConst.CONFIG_DEFAULT.UNICODE];
                    AddRadioButtonOptionField(targetPanel, "rbUnicode", "rbUnicode", eResApp.GetRes(Pref, 7732), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.UNICODE.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

                }

                // Recherche sur caractères internationaux
                items = new Dictionary<string, string>();
                items.Add("0", eResApp.GetRes(Pref, 58));
                items.Add("1", eResApp.GetRes(Pref, 59));
                value = String.IsNullOrEmpty(_configs[eLibConst.CONFIG_DEFAULT.SEARCHUNICODEDISABLED]) ? "1" : _configs[eLibConst.CONFIG_DEFAULT.SEARCHUNICODEDISABLED];


                //  value = sSearchIsDisabled == "1" ? "0" : "1";


                AddRadioButtonOptionField(targetPanel, "rbSearchUnicodeDisabled", "rbSearchUnicodeDisabled",


                    eResApp.GetRes(Pref, 7733), "", eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT,
                    eLibConst.CONFIG_DEFAULT.SEARCHUNICODEDISABLED.GetHashCode(), typeof(eLibConst.CONFIG_DEFAULT), items, value, EudoQuery.FieldFormat.TYP_BIT);

            }

            // Formats de date
            items = new Dictionary<string, string>();
            items.Add(CONFIGADV_CULTUREINFO.LITTLE_ENDIAN.GetHashCode().ToString(), "dd/MM/yyyy");
            items.Add(CONFIGADV_CULTUREINFO.BIG_ENDIAN.GetHashCode().ToString(), "yyyy/MM/dd");
            items.Add(CONFIGADV_CULTUREINFO.MIDDLE_ENDIAN.GetHashCode().ToString(), "MM/dd/yyyy");
            items.Add(CONFIGADV_CULTUREINFO.BIG_ENDIAN_TIRET.GetHashCode().ToString(), "yyyy-MM-dd");
            value = String.IsNullOrEmpty(_configsADV[EudoEnum.CONFIGADV.CULTUREINFO]) ? "0" : _configsADV[EudoEnum.CONFIGADV.CULTUREINFO];
            AddRadioButtonOptionField(targetPanel, "rbCultureInfo", "rbCultureInfo", eResApp.GetRes(Pref, 7270), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.CULTUREINFO.GetHashCode(), typeof(EudoEnum.CONFIGADV), items, value, EudoQuery.FieldFormat.TYP_NUMERIC);

            #region Caractères séparateur

            Panel panelSep = new Panel();
            panelSep.ID = "blockThousandsSep";
            targetPanel.Controls.Add(panelSep);

            value = _configsADV[EudoEnum.CONFIGADV.THOUSANDS_SEP_DISABLED];
            bool noSep = value == "1" ? true : false;

            eAdminField txtField = new eAdminTextboxField(0, eResApp.GetRes(Pref, 7265), eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.NUMBER_SECTIONS_DELIMITER.GetHashCode(),
                value: _configsADV[EudoEnum.CONFIGADV.NUMBER_SECTIONS_DELIMITER], labelType: eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField");
            txtField.SetFieldControlID("textSectionsDelimiter");
            txtField.Generate(panelSep);
            ((TextBox)(txtField.FieldControl)).Enabled = !noSep;
            ((TextBox)txtField.FieldControl).Attributes.Add("tabfld", "cfgadv");
            ((TextBox)txtField.FieldControl).Attributes.Add("onchange", DEFAULT_OPTION_ONCHANGE);
            // Case à cocher "Pas de séparateur"
            AddCheckboxOptionField(panelSep, "chkNoSep", eResApp.GetRes(_ePref, 1871), "", eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.THOUSANDS_SEP_DISABLED.GetHashCode(), typeof(EudoEnum.CONFIGADV), noSep,
                onClick: "nsAdmin.checkNoSep(this)");

            AddTextboxOptionField(targetPanel
                , id: "textDecimalDelimiter"
                , label: eResApp.GetRes(Pref, 7266)
                , tooltip: ""
                , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                , propKeyCode: EudoEnum.CONFIGADV.NUMBER_DECIMAL_DELIMITER.GetHashCode()
                , propKeyType: typeof(EudoEnum.CONFIGADV)
                , currentValue: _configsADV[EudoEnum.CONFIGADV.NUMBER_DECIMAL_DELIMITER]
                , adminFieldType: EudoQuery.AdminFieldType.ADM_TYPE_CHAR
                , labelType: eAdminTextboxField.LabelType.INLINE
                , customTextboxCSSClasses: "optionField");

            Panel pnCurrency = new Panel();
            targetPanel.Controls.Add(pnCurrency);

            pnCurrency.CssClass = "pnCurrency";

            AddTextboxOptionField(pnCurrency
                , id: "textDefaultCurrency"
                , label: eResApp.GetRes(Pref, 3130)
                , tooltip: ""
                , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                , propKeyCode: EudoEnum.CONFIGADV.DEFAULT_CURRENCY.GetHashCode()
                , propKeyType: typeof(EudoEnum.CONFIGADV)
                , currentValue: _configsADV[EudoEnum.CONFIGADV.DEFAULT_CURRENCY]
                , adminFieldType: EudoQuery.AdminFieldType.ADM_TYPE_CHAR
                , labelType: eAdminTextboxField.LabelType.INLINE
                , customTextboxCSSClasses: "optionField"
                );

            Dictionary<string, string> dicPositionsValue = new Dictionary<string, string>();
            dicPositionsValue.Add(((int)UNIT_POSITION.LEFT).ToString(), eResApp.GetRes(Pref, 3125));
            dicPositionsValue.Add(((int)UNIT_POSITION.RIGHT).ToString(), eResApp.GetRes(Pref, 3126));

            AddRadioButtonOptionField(pnCurrency
                , id: "rbCurrencyPosition"
                , groupName: "rbCurrencyPosition"
                , label: ""
                , tooltip: ""
                , propCat: eAdminUpdateProperty.CATEGORY.CONFIGADV
                , propKeyCode: (int)EudoEnum.CONFIGADV.DEFAULT_CURRENCY_POSITION
                , propKeyType: typeof(EudoEnum.CONFIGADV)
                , items: dicPositionsValue
                , selectedValue: String.IsNullOrEmpty(_configsADV[EudoEnum.CONFIGADV.DEFAULT_CURRENCY_POSITION]) ? ((int)(UNIT_POSITION.RIGHT)).ToString() : _configsADV[EudoEnum.CONFIGADV.DEFAULT_CURRENCY_POSITION]
                , valueFormat: FieldFormat.TYP_NUMERIC
                , customRadioButtonCSSClasses: "cbCurrencyPosition"
                , customPanelCSSClasses: "pnCurrencyPosition"
                );

            //Boutons pour appliquer les devises par défaut aux rubriques monétaires existentes
            AddButtonOptionField(pnCurrency, "btnApplyCurrency", eResApp.GetRes(Pref, 3133), "", "nsAdmin.applyCurrency();");


            #endregion

            //Boutons des traductions
            AddButtonOptionField(targetPanel, "btnTranslations", eResApp.GetRes(Pref, 7716), eResApp.GetRes(Pref, 7717), "nsAdmin.openTranslations(0);");

            //Boutons du mappage des langues
            AddButtonOptionField(targetPanel, "btnLangs", eResApp.GetRes(Pref, 7718), eResApp.GetRes(Pref, 7719), "nsAdmin.confLanguages()");

            return true;
        }
    }
}