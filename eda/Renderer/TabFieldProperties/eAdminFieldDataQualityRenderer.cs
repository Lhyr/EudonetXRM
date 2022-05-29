using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu du block "Qualité des données et RGPD" sur l'administration d'une rubrique
    /// </summary>
    public class eAdminFieldDataQualityRenderer : eAdminBlockRenderer
    {
        private bool _isFeatureAvailable = false;

        private Int32 _descid;
        private eAdminFieldInfos _field;
        DescAdvDataSet _descAdv;
        List<eUser.UserListItem> _listUsersAndGroups;

        private bool _isRGPDEnabled = false;
        private string _natureValue = String.Empty;
        private string _personalCategoryValue = String.Empty;
        private string _sensitiveCategoryValue = String.Empty;
        private string _categoryPrecisionValue = String.Empty;
        private string _dataPurposeValue = String.Empty;
        private bool _isPseudoEnabled = false;
        private string _pseudoRulesValue = String.Empty;
        private string _pseudoReplaceValue = String.Empty;
        private string _responsible1Value = String.Empty;
        private string _responsible2Value = String.Empty;
        private string _responsible3Value = String.Empty;
        private string _responsibleOtherValue = String.Empty;

        private const string ResponsibleOtherItemValue = DESCADV_RGPD_OTHER_VALUES.RESPONSIBLE;
        private const string jsOnChangeFct = "nsAdmin.onChangeRGPDGenericAction(this, event);";

        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="field">Rubrique</param>
        private eAdminFieldDataQualityRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 8284), idBlock: "blockDataQuality")
        {
            _isFeatureAvailable = eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD);

            _descid = field.DescId;
            _field = field;
            _listUsersAndGroups = new List<eUser.UserListItem>();
        }

        /// <summary>
        /// Méthode d'instanciation externe
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="field">Rubrique</param>
        /// <returns></returns>
        public static eAdminFieldDataQualityRenderer CreateAdminFieldDataQualityRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldDataQualityRenderer features = new eAdminFieldDataQualityRenderer(pref, field);
            return features;
        }

        /// <summary>
        /// Retourne un paramètre DESCADV dans la liste de paramètres
        /// </summary>
        /// <param name="param">Paramètre</param>
        /// <param name="defaultValue">valeur par défaut</param>
        /// <returns></returns>
        private string GetAdvInfoValue(DESCADV_PARAMETER param, string defaultValue = "")
        {
            return _descAdv.GetAdvInfoValue(_field.DescId, param, defaultValue);
        }

        /// <summary>
        /// Charge les infos nécéssaires au rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (_isFeatureAvailable)
                {
                    eudoDAL dal = eLibTools.GetEudoDAL(Pref);

                    bool bDalOpen = dal.IsOpen;
                    try
                    {
                        if (!bDalOpen)
                            dal.OpenDatabase();

                        _descAdv = new DescAdvDataSet();
                        _descAdv.LoadAdvParams(dal, new int[] { _field.DescId }, eDataQualityTools.GetListRGPDDescAdvParameter());

                        eUser usrObj = new eUser(dal, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, new List<string>());
                        StringBuilder sbError = new StringBuilder();
                        _listUsersAndGroups = usrObj.GetUserList(true, false, "", sbError);
                    }
                    finally
                    {
                        if (!bDalOpen)
                            dal.CloseDatabase();
                    }


                    _isRGPDEnabled = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_ENABLED, "0") == "1";
                    _natureValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_NATURE, ((int)DESCADV_RGPD_DEFAULT_VALUES.NATURE).ToString());
                    _personalCategoryValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.PERSONAL_CATEGORY).ToString());
                    _sensitiveCategoryValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_SENSIBLE_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.SENSITIVE_CATEGORY).ToString());
                    _categoryPrecisionValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION, String.Empty);
                    _dataPurposeValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_DATA_PURPOSE, String.Empty);
                    _isPseudoEnabled = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_ENABLED, "0") == "1";
                    _pseudoRulesValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_RULES, String.Empty);
                    _pseudoReplaceValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_REPLACE_VALUE, String.Empty);
                    _responsible1Value = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_RESPONSIBLE_1, String.Empty);
                    _responsible2Value = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_RESPONSIBLE_2, String.Empty);
                    _responsible3Value = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_RESPONSIBLE_3, String.Empty);
                    _responsibleOtherValue = this.GetAdvInfoValue(DESCADV_PARAMETER.RGPD_RESPONSIBLE_OTHER, String.Empty);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fait le rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                if (_isFeatureAvailable)
                {
                    eAdminRadioButtonField adminRadioField;
                    eAdminDropdownField adminDropdownField;
                    eAdminTextboxField adminTextboxField;

                    Dictionary<string, string> dicoRadioYesNo = new Dictionary<string, string>() { { "1", eResApp.GetRes(Pref, 58) }, { "0", eResApp.GetRes(Pref, 59) } };

                    //RGPD activé                               
                    adminRadioField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 8306), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_ENABLED, "rgpdEnabled",
                        dicoRadioYesNo, eResApp.GetRes(Pref, 8307), _isRGPDEnabled ? "1" : "0", FieldFormat.TYP_CHAR,
                        onChange: String.Concat(jsOnChangeFct,
                                    "nsAdmin.toggleRGPDPropertiesPanel(this, '", (int)DESCADV_RGPD_DEFAULT_VALUES.NATURE, "', '", (int)DESCADV_RGPD_DEFAULT_VALUES.PERSONAL_CATEGORY, "');")
                        );
                    adminRadioField.IsLabelBefore = true;
                    adminRadioField.Generate(_panelContent);

                    Panel panelRGPDProperties = new Panel();
                    panelRGPDProperties.ID = "panelRGPDProperties";
                    if (!_isRGPDEnabled)
                        panelRGPDProperties.Style.Add("display", "none");
                    _panelContent.Controls.Add(panelRGPDProperties);


                    //Nature
                    Panel panelRGPDNature = new Panel();
                    panelRGPDNature.ID = "panelRGPDNature";
                    panelRGPDProperties.Controls.Add(panelRGPDNature);
                    Dictionary<string, string> dicoRadioNature = new Dictionary<string, string>()
                    {
                        { ((int)DESCADV_RGPD_NATURE.PERSONAL).ToString(), eDataQualityTools.GetNatureLabel(Pref, DESCADV_RGPD_NATURE.PERSONAL) },
                        { ((int)DESCADV_RGPD_NATURE.SENSITIVE).ToString(), eDataQualityTools.GetNatureLabel(Pref, DESCADV_RGPD_NATURE.SENSITIVE) }
                    };
                    adminRadioField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 8308), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_NATURE, "rgpdNature",
                        dicoRadioNature, eResApp.GetRes(Pref, 8309), _natureValue, FieldFormat.TYP_CHAR,
                        onChange: String.Concat(jsOnChangeFct,
                            "nsAdmin.toggleRGPDNature(this, '", (int)DESCADV_RGPD_NATURE.PERSONAL, "', '", (int)DESCADV_RGPD_NATURE.SENSITIVE, "', '", (int)DESCADV_RGPD_PERSONNAL_CATEGORY.UNSPECIFIED, "', '", (int)DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER, "', '", (int)DESCADV_RGPD_SENSITIVE_CATEGORY.UNSPECIFIED, "', '", (int)DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER, "');")
                        );
                    adminRadioField.IsLabelBefore = true;
                    adminRadioField.Generate(panelRGPDNature);


                    //Categorie personnelle
                    Panel panelRGPDPersonalCategory = new Panel();
                    panelRGPDPersonalCategory.ID = "panelRGPDPersonalCategory";
                    if (_natureValue != ((int)DESCADV_RGPD_NATURE.PERSONAL).ToString())
                        panelRGPDPersonalCategory.Style.Add("display", "none");
                    panelRGPDProperties.Controls.Add(panelRGPDPersonalCategory);

                    List<ListItem> listItemCategoryPersonal = new List<ListItem>();
                    listItemCategoryPersonal.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, DESCADV_RGPD_PERSONNAL_CATEGORY.UNSPECIFIED), ((int)DESCADV_RGPD_PERSONNAL_CATEGORY.UNSPECIFIED).ToString()));
                    foreach (DESCADV_RGPD_PERSONNAL_CATEGORY category in Enum.GetValues(typeof(DESCADV_RGPD_PERSONNAL_CATEGORY)))
                    {
                        if (category != DESCADV_RGPD_PERSONNAL_CATEGORY.UNSPECIFIED && category != DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER)
                            listItemCategoryPersonal.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, category), ((int)category).ToString()));
                    }
                    listItemCategoryPersonal.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER), ((int)DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER).ToString()));

                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8310), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY,
                            listItemCategoryPersonal.ToArray(), eResApp.GetRes(Pref, 8312), _personalCategoryValue, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDCategory(this, '", (int)DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER, "');")
                            );
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDPersonalCategory);

                    //Categorie sensible
                    Panel panelRGPDSensitiveCategory = new Panel();
                    panelRGPDSensitiveCategory.ID = "panelRGPDSensitiveCategory";
                    if (_natureValue != ((int)DESCADV_RGPD_NATURE.SENSITIVE).ToString())
                        panelRGPDSensitiveCategory.Style.Add("display", "none");
                    panelRGPDProperties.Controls.Add(panelRGPDSensitiveCategory);

                    List<ListItem> listItemCategorySensible = new List<ListItem>();
                    listItemCategorySensible.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, DESCADV_RGPD_SENSITIVE_CATEGORY.UNSPECIFIED), ((int)DESCADV_RGPD_SENSITIVE_CATEGORY.UNSPECIFIED).ToString()));
                    foreach (DESCADV_RGPD_SENSITIVE_CATEGORY category in Enum.GetValues(typeof(DESCADV_RGPD_SENSITIVE_CATEGORY)))
                    {
                        if (category != DESCADV_RGPD_SENSITIVE_CATEGORY.UNSPECIFIED && category != DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER)
                            listItemCategorySensible.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, category), ((int)category).ToString()));
                    }
                    listItemCategorySensible.Add(new ListItem(eDataQualityTools.GetCategoryLabel(Pref, DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER), ((int)DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER).ToString()));

                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8311), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_SENSIBLE_CATEGORY,
                            listItemCategorySensible.ToArray(), eResApp.GetRes(Pref, 8312), _sensitiveCategoryValue, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDCategory(this, '", (int)DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER, "');")
                            );
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDSensitiveCategory);

                    //Categorie autre
                    Panel panelRGPDCategoryOther = new Panel();
                    panelRGPDCategoryOther.ID = "panelRGPDCategoryOther";
                    if ((_natureValue == ((int)DESCADV_RGPD_NATURE.PERSONAL).ToString() && _personalCategoryValue != ((int)DESCADV_RGPD_PERSONNAL_CATEGORY.OTHER).ToString())
                        || (_natureValue == ((int)DESCADV_RGPD_NATURE.SENSITIVE).ToString() && _sensitiveCategoryValue != ((int)DESCADV_RGPD_SENSITIVE_CATEGORY.OTHER).ToString())
                        )
                        panelRGPDCategoryOther.Style.Add("display", "none");
                    panelRGPDProperties.Controls.Add(panelRGPDCategoryOther);

                    adminTextboxField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 8313), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION,
                        AdminFieldType.ADM_TYPE_CHAR, eResApp.GetRes(Pref, 8314), _categoryPrecisionValue, optional: true,
                        onChange: String.Concat(jsOnChangeFct)
                        );
                    adminTextboxField.Generate(panelRGPDCategoryOther);


                    //Finalité donnée
                    Panel panelRGPDDataPurpose = new Panel();
                    panelRGPDDataPurpose.ID = "panelRGPDDataPurpose";
                    panelRGPDProperties.Controls.Add(panelRGPDDataPurpose);
                    adminTextboxField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 8315), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_DATA_PURPOSE,
                        AdminFieldType.ADM_TYPE_CHAR, eResApp.GetRes(Pref, 8316), _dataPurposeValue, optional: true,
                        onChange: String.Concat(jsOnChangeFct));
                    adminTextboxField.Generate(panelRGPDDataPurpose);


                    //Pseudonymisation activé
                    Panel panelRGPDPseudoEnabled = new Panel();
                    panelRGPDPseudoEnabled.ID = "panelRGPDPseudoEnabled";
                    panelRGPDProperties.Controls.Add(panelRGPDPseudoEnabled);
                    adminRadioField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 8317), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_ENABLED, "rgpdPseudoEnabled",
                        dicoRadioYesNo, String.Empty, _isPseudoEnabled ? "1" : "0", FieldFormat.TYP_CHAR,
                        onChange: String.Concat(jsOnChangeFct, "nsAdmin.toggleRGPDPseudoRulesPanel(this);"));
                    adminRadioField.IsLabelBefore = true;
                    adminRadioField.Generate(panelRGPDPseudoEnabled);

                    //Pseudonymisation règles
                    Panel panelRGPDPseudoRules = new Panel();
                    panelRGPDPseudoRules.ID = "panelRGPDPseudoRules";
                    if (!_isPseudoEnabled)
                        panelRGPDPseudoRules.Style.Add("display", "none");
                    panelRGPDProperties.Controls.Add(panelRGPDPseudoRules);

                    List<ListItem> listItemPseudoRules = new List<ListItem>();
                    listItemPseudoRules.Add(new ListItem(eDataQualityTools.GetPseudonymisationRuleTypeLabel(Pref, DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.UNSPECIFIED), ((int)DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.UNSPECIFIED).ToString()));
                    foreach (DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE type in Enum.GetValues(typeof(DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE)))
                    {
                        if (type == DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.UNSPECIFIED)
                            continue;

                        if (type == DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.REPLACE && (
                                (_field.PopupType != PopupType.NONE) //Catalogue
                                || (_field.Format == FieldFormat.TYP_CHAR && _field.PopupDescId % 100 == 1) //Liaison
                                || (_field.Format == FieldFormat.TYP_MONEY || _field.Format == FieldFormat.TYP_NUMERIC) //Numérique
                                || (_field.Format == FieldFormat.TYP_AUTOINC || _field.Format == FieldFormat.TYP_COUNT) //Compteur
                                || (_field.Format == FieldFormat.TYP_DATE) //Date
                                || (_field.Format == FieldFormat.TYP_BIT || _field.Format == FieldFormat.TYP_BITBUTTON) //Booléen
                                || (_field.Format == FieldFormat.TYP_GEOGRAPHY_OLD || _field.Format == FieldFormat.TYP_GEOGRAPHY_V2) //Géolocalisation
                                || (_field.Format == FieldFormat.TYP_IMAGE || _field.Format == FieldFormat.TYP_FILE) //Images/Fichiers
                                || (_field.Format == FieldFormat.TYP_CHART) //Graphiques
                                || (_field.Format == FieldFormat.TYP_IFRAME) //Pages internet
                                || (_field.Format == FieldFormat.TYP_USER || _field.Format == FieldFormat.TYP_GROUP) //Users et groupes
                                )
                            )
                            continue;

                        if (type == DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.DELETE && (
                                (_field.Format == FieldFormat.TYP_AUTOINC || _field.Format == FieldFormat.TYP_COUNT) //Compteur
                                || (_field.Format == FieldFormat.TYP_BIT || _field.Format == FieldFormat.TYP_BITBUTTON) //Booléen
                                || (_field.Format == FieldFormat.TYP_GEOGRAPHY_OLD || _field.Format == FieldFormat.TYP_GEOGRAPHY_V2) //Géolocalisation
                                || (_field.Format == FieldFormat.TYP_CHART) //Graphiques
                                || (_field.Format == FieldFormat.TYP_IFRAME) //Pages internet
                                )
                            )
                            continue;

                        listItemPseudoRules.Add(new ListItem(eDataQualityTools.GetPseudonymisationRuleTypeLabel(Pref, type), ((int)type).ToString()));
                    }

                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8318), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_RULES,
                            listItemPseudoRules.ToArray(), String.Empty, _pseudoRulesValue, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDPseudoReplaceValuePanel(this, '", (int)DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.REPLACE, "');"));
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDPseudoRules);

                    //Valeur de remplacement
                    Panel panelRGPDPseudoReplaceValue = new Panel();
                    panelRGPDPseudoReplaceValue.ID = "panelRGPDPseudoReplaceValue";
                    if (_pseudoRulesValue != ((int)DESCADV_RGPD_PSEUDONYMISATION_RULE_TYPE.REPLACE).ToString())
                        panelRGPDPseudoReplaceValue.Style.Add("display", "none");
                    panelRGPDPseudoRules.Controls.Add(panelRGPDPseudoReplaceValue);
                    adminTextboxField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 8532), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_REPLACE_VALUE, //TODORES
                        AdminFieldType.ADM_TYPE_CHAR, String.Empty, _pseudoReplaceValue, optional: true,
                        onChange: String.Concat(jsOnChangeFct));
                    adminTextboxField.Generate(panelRGPDPseudoReplaceValue);


                    //Responsables
                    Panel panelRGPDResponsibles = new Panel();
                    panelRGPDResponsibles.ID = "panelRGPDResponsibles";
                    panelRGPDProperties.Controls.Add(panelRGPDResponsibles);

                    //Responsable 1
                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8319), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_RESPONSIBLE_1,
                            GetListItemUsers().ToArray(), eResApp.GetRes(Pref, 8320),
                            _responsible1Value, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDResponsibleOtherPanel(this, '", ResponsibleOtherItemValue, "');")
                            );
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDResponsibles);

                    //Responsable 2
                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8333), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_RESPONSIBLE_2,
                            GetListItemUsers().ToArray(), eResApp.GetRes(Pref, 8320),
                            _responsible2Value, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDResponsibleOtherPanel(this, '", ResponsibleOtherItemValue, "');")
                            );
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDResponsibles);

                    //Responsable 3
                    adminDropdownField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 8334), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_RESPONSIBLE_3,
                            GetListItemUsers().ToArray(), eResApp.GetRes(Pref, 8320),
                            _responsible3Value, FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                            onChange: String.Concat(jsOnChangeFct,
                                                    "nsAdmin.toggleRGPDResponsibleOtherPanel(this, '", ResponsibleOtherItemValue, "');")
                            );
                    adminDropdownField.SortItemsByLabel = false;
                    adminDropdownField.IsOptional = true;
                    adminDropdownField.Generate(panelRGPDResponsibles);

                    //Responsable Autre
                    Panel panelRGPDResponsibleOther = new Panel();
                    panelRGPDResponsibleOther.ID = "panelRGPDResponsibleOther";
                    if (_responsible1Value != ResponsibleOtherItemValue && _responsible2Value != ResponsibleOtherItemValue && _responsible3Value != ResponsibleOtherItemValue)
                        panelRGPDResponsibleOther.Style.Add("display", "none");
                    panelRGPDProperties.Controls.Add(panelRGPDResponsibleOther);

                    adminTextboxField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 8321), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.RGPD_RESPONSIBLE_OTHER,
                        AdminFieldType.ADM_TYPE_CHAR, eResApp.GetRes(Pref, 8322), _responsibleOtherValue, optional: true,
                        onChange: String.Concat(jsOnChangeFct));
                    adminTextboxField.Generate(panelRGPDResponsibleOther);


                    //Bouton ouverture rapport
                    eAdminField btnReport = new eAdminButtonField(eResApp.GetRes(Pref, 8323), "btnRGPDReport", String.Empty, "nsAdmin.openRGPDReport();");
                    btnReport.Generate(_panelContent);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Construit une liste de users/groupes pour un select html
        /// </summary>
        /// <returns></returns>
        private List<ListItem> GetListItemUsers()
        {
            List<ListItem> listItems = new List<ListItem>();
            listItems.Add(new ListItem(String.Empty, String.Empty));
            foreach (eUser.UserListItem userItem in _listUsersAndGroups)
            {
                ListItem li = new ListItem(userItem.Libelle, userItem.ItemCode);
                if (userItem.Type == eUser.UserListItem.ItemType.GROUP)
                    li.Attributes.Add("class", "grp");
                listItems.Add(li);
            }
            listItems.Add(new ListItem(eResApp.GetRes(Pref, 75), ResponsibleOtherItemValue));

            return listItems;
        }

    }
}