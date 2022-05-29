using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Configuration de l'autocompletion d'une adresse
    /// </summary>
    public class eAdminAutocompleteAddressDialogRenderer : eAdminRenderer
    {
        #region propriétés
        private enum COLUMN
        {
            LABEL = 1,
            LIST = 2,
            ADDCATALOGVALUE = 3
        }

        private Int32 _nTab;
        private String _tabName;
        private eAdminTableInfos _tabInfos;
        private IDictionary<eLibConst.CONFIGADV, String> _configADV;
        private eConst.PredictiveAddressesRef _autoCompleteRef = eConst.PredictiveAddressesRef.OpenDataGouvFr;

        string _ppTabName = String.Empty;
        string _pmTabName = String.Empty;
        string _adrTabName = String.Empty;

        private IEnumerable<eFieldLiteWithLib> _listTabAvailableSearchFields;
        private IEnumerable<eFieldLiteWithLib> _listTabFieldsChar;
        private IEnumerable<eFieldLiteWithLib> _listTabFieldsGeo;

        int _autoCompletionDescid = 0;
        bool _autoCompletionMidFormulaEnabled = false;
        bool _autoCompletionPmAutomationEnabled = false;

        private List<eFilemapPartner> _listMappings;
        private List<int> _listMappingDescid;
        private eFilemapPartner _mappingHouseNumber;
        private eFilemapPartner _mappingStreetName;
        private eFilemapPartner _mappingCity;
        private eFilemapPartner _mappingPostalCode;
        private eFilemapPartner _mappingCityCode;
        private eFilemapPartner _mappingDepartmentNumber;
        private eFilemapPartner _mappingDepartment;
        private eFilemapPartner _mappingRegion;
        private eFilemapPartner _mappingCountry;
        private eFilemapPartner _mappingGeography;
        private eFilemapPartner _mappingLabel;

        private Panel _panelMainDiv;
        private Panel _panelSectionDiv;
        #endregion

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminAutocompleteAddressDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
        }

        public static eAdminAutocompleteAddressDialogRenderer CreateAdminAutocompleteAddressDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminAutocompleteAddressDialogRenderer(pref, nTab);
        }

        protected override bool Init()
        {

            if (base.Init())
            {
                _configADV = eLibTools.GetConfigAdvValues(Pref, new HashSet<eLibConst.CONFIGADV> { eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF });
                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Maps_InternationalPredictiveAddr))
                {
                    _autoCompleteRef = (eConst.PredictiveAddressesRef)eLibTools.GetNum(_configADV[eLibConst.CONFIGADV.PREDICTIVEADDRESSESREF]);
                }

                // Chargement des valeurs des dropDownLists
                LoadDropDownListValues();

                // Chargement des Mappings
                LoadMappings();

                if (_tabInfos.DescId == EudoQuery.TableType.PM.GetHashCode())
                {
                    // Chargement des RES de PP/PM/ADDRESS
                    LoadTabsNames();
                }

                return true;

            }

            return false;
        }

        public static IEnumerable<eFieldLiteWithLib> GetTabFields(ePref pref, int tab,
            IEnumerable<FieldFormat> onlyThisFormats, IEnumerable<PopupType> onlyThisPopupType = null)
        {
            IEnumerable<eFieldLiteWithLib> list = RetrieveFields.GetDefault(pref)
                .AddOnlyThisTabs(new int[] { tab })
                .AddOnlyThisFormats(onlyThisFormats)
                .AddOnlyThisPopupType(onlyThisPopupType)
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(pref));

            // Exclus les rubriques avec libelle vide (cibles etendues)
            list = list?.Where(fld => !String.IsNullOrEmpty(fld.Libelle));

            // Tri par libelle
            list = list?.OrderBy(fld => fld.Libelle);

            if (list != null)
                return list;

            return new List<eFieldLiteWithLib>();
        }

        private void LoadDropDownListValues()
        {
            string error = String.Empty;

            try
            {
                // Liste des rubriques proposées comme champs déclencheurs : Caractère
                _listTabAvailableSearchFields = GetTabFields(Pref, _nTab, new FieldFormat[] { FieldFormat.TYP_CHAR }, new PopupType[] { PopupType.NONE });
        
                // Rub char hors catalogue
                _listTabFieldsChar = GetTabFields(Pref, _nTab, new FieldFormat[] { FieldFormat.TYP_CHAR }, new PopupType[] { PopupType.NONE, PopupType.FREE, PopupType.ONLY, PopupType.DATA });
                // Rub geo
                _listTabFieldsGeo = GetTabFields(Pref, _nTab, new FieldFormat[] { FieldFormat.TYP_GEOGRAPHY_V2 });
            }
            catch (Exception ex)
            {
                throw new Exception(String.Concat(Environment.NewLine, ex.Message));
            }
        }

        private void LoadMappings()
        {
            string sError = String.Empty;

            #region Chargement des champs déclencheurs actuellement définis
            // Récupération du premier descid défini avec autocomplete activé, mais uniquement parmi ceux qui ne sont PAS utilisés par Sirene
            Dictionary<Int32, Int32> dicoAutocompleteAddress = _tabInfos.GetAutocompleteAddressFields(Pref);
            string[] sireneEnabledFields = eSireneMapping.GetSireneEnabledFields(Pref, _tabInfos.DescId);
            KeyValuePair<Int32, Int32> mappingAutoCompletion = dicoAutocompleteAddress.FirstOrDefault(
                mp => (
                    EudoQuery.Field.AutoCompletionEnabledStatic((EudoQuery.AutoCompletion)mp.Value) == true &&
                    !sireneEnabledFields.Contains(mp.Key.ToString())
                )
            );

            if (mappingAutoCompletion.Key != 0)
            {
                _autoCompletionDescid = mappingAutoCompletion.Key;
                _autoCompletionMidFormulaEnabled = EudoQuery.Field.AutoCompletionMidFormulaEnabledStatic((EudoQuery.AutoCompletion)mappingAutoCompletion.Value);
                _autoCompletionPmAutomationEnabled = EudoQuery.Field.AutoCompletionPmAutomationEnabledStatic((EudoQuery.AutoCompletion)mappingAutoCompletion.Value);
            }
            #endregion

            #region Chargement des rubriques mappées avec les champs proposés par le fournisseur d'adresses prédictives
            _listMappings = eAutoCompletionTools.Load(Pref, _nTab, EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE, EudoQuery.AutoCompletionType.AUTO_COMPLETION_ADDRESS, false, out sError);
            if (!String.IsNullOrEmpty(sError))
            {
                throw new Exception("Erreur lors du chargement du mapping : " + sError);
            }

            _mappingHouseNumber = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.NoVOIE);
            _mappingStreetName = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.RUE);
            _mappingCity = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.VILLE);
            _mappingPostalCode = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.CODEPOSTAL);
            _mappingCityCode = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.INSEE);
            _mappingDepartmentNumber = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.NoDEPARTEMENT);
            _mappingDepartment = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.DEPARTEMENT);
            _mappingRegion = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.REGION);
            _mappingCountry = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.PAYS);
            _mappingGeography = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.GEOGRAPHY);
            _mappingLabel = _listMappings.FirstOrDefault(mp => mp.Source == eModelConst.AutocompleteAddressMappings.LABEL);

            // Recensement des champs déjà mappés
            List<eFilemapPartner> listTemp = new List<eFilemapPartner>() {
                _mappingHouseNumber,
                _mappingStreetName,
                _mappingCity,
                _mappingPostalCode,
                _mappingCityCode,
                _mappingDepartmentNumber,
                _mappingDepartment,
                _mappingRegion,
                _mappingCountry,
                _mappingGeography,
                _mappingLabel
            };

            _listMappingDescid = new List<int>();

            foreach (eFilemapPartner mapping in listTemp)
            {
                if (mapping != null && !_listMappingDescid.Contains(mapping.DescId))
                    _listMappingDescid.Add(mapping.DescId);
            }
            #endregion
        }

        private void LoadTabsNames()
        {
            List<string> listDescIds = new List<string>()
            {
                EudoQuery.TableType.PP.GetHashCode().ToString(),
                EudoQuery.TableType.PM.GetHashCode().ToString(),
                EudoQuery.TableType.ADR.GetHashCode().ToString()
            };

            if (_tabInfos.InterEVT)
                listDescIds.Add(_tabInfos.InterEVTDescid.ToString());

            eRes res = new eRes(Pref, String.Join(",", listDescIds.ToArray()));

            bool bResFound = false;
            _ppTabName = res.GetRes(EudoQuery.TableType.PP.GetHashCode(), out bResFound);
            if (!bResFound)
                _ppTabName = "Contacts";

            bResFound = false;
            _pmTabName = res.GetRes(EudoQuery.TableType.PM.GetHashCode(), out bResFound);
            if (!bResFound)
                _ppTabName = "Sociétés";

            bResFound = false;
            _adrTabName = res.GetRes(EudoQuery.TableType.ADR.GetHashCode(), out bResFound);
            if (!bResFound)
                _ppTabName = "Adresses";
        }


        protected override bool Build()
        {
            _pgContainer.ID = "autocompleteAddressAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            try
            {
                CreateMainDiv();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }

            return base.Build();
        }

        private void CreateMainDiv()
        {
            _panelMainDiv = new Panel();
            _panelMainDiv.ID = "edaAcaMainDiv";

            // Champ déclencheur recherche
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 7031));

            CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 7032)), "searchField", _listTabAvailableSearchFields, selectedVal: _autoCompletionDescid, isSearchField: true);


            // Champs des mappings
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 7033));

            CreateMappingFields();


            /*
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 7034));
            CreateSubtitle(eResApp.GetRes(Pref, 7035));

            if (_autoCompletionDescid != 0)
                CreateRadioBox("midFormula", selectedValue: _autoCompletionMidFormulaEnabled, radioboxEnabled: true);
            else
                CreateRadioBox("midFormula", radioboxEnabled: false);

            if (_tabInfos.DescId == EudoQuery.TableType.PM.GetHashCode())
            {
                CreateSubtitle(eResApp.GetRes(Pref, 7036).Replace("<TABPM>", _pmTabName).Replace("<TABADR>", _adrTabName).Replace("<TABPP>", _ppTabName));
                if (_autoCompletionDescid != 0)
                    CreateRadioBox("pmAutomation", _autoCompletionPmAutomationEnabled, radioboxEnabled: true);
                else
                    CreateRadioBox("pmAutomation", radioboxEnabled: false);
            }*/

            _pgContainer.Controls.Add(_panelMainDiv);
        }

        /// <summary>
        /// Crée le mapping 
        /// </summary>
        private void CreateMappingFields()
        {
            bool fromDataGouv = _autoCompleteRef == eConst.PredictiveAddressesRef.OpenDataGouvFr;

            // Résultat complet de la recherche (adresse complète) - US #1224
            CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 2484)), eModelConst.AutocompleteAddressMappings.LABEL, _listTabFieldsChar, _mappingLabel);
            // Numéro de voie
            if (fromDataGouv)
            {
                CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 7026)), eModelConst.AutocompleteAddressMappings.NoVOIE, _listTabFieldsChar, _mappingHouseNumber);
            }
            // Rue
            CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 6107)), eModelConst.AutocompleteAddressMappings.RUE, _listTabFieldsChar, _mappingStreetName);
            // Code Postal
            CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 6106)), eModelConst.AutocompleteAddressMappings.CODEPOSTAL, _listTabFieldsChar, _mappingPostalCode);
            // Ville
            CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 6105)), eModelConst.AutocompleteAddressMappings.VILLE, _listTabFieldsChar, _mappingCity);

            if (fromDataGouv)
            {
                // Code INSEE
                CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 7030)), eModelConst.AutocompleteAddressMappings.INSEE, _listTabFieldsChar, _mappingCityCode);
                // N° Département
                CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 7027)), eModelConst.AutocompleteAddressMappings.NoDEPARTEMENT, _listTabFieldsChar, _mappingDepartmentNumber);
                // Département
                CreateRubriques(String.Concat(eResApp.GetResWithColon(Pref, 7028)), eModelConst.AutocompleteAddressMappings.DEPARTEMENT, _listTabFieldsChar, _mappingDepartment);
            }
            // Région (DataGouv.fr) ou Etat/Province (Bing Maps)
            CreateRubriques(String.Concat(eResApp.GetRes(Pref, fromDataGouv ? 7029 : 8329), " :"), eModelConst.AutocompleteAddressMappings.REGION, _listTabFieldsChar, _mappingRegion);
            // Pays
            CreateRubriques(String.Concat(eResApp.GetRes(Pref, 6104), " :"), eModelConst.AutocompleteAddressMappings.PAYS, _listTabFieldsChar, _mappingCountry);
            // Géolocalisation
            CreateRubriques(String.Concat(eResApp.GetRes(Pref, 7108), " :"), eModelConst.AutocompleteAddressMappings.GEOGRAPHY, _listTabFieldsGeo, _mappingGeography);
        }

        /// <summary>
        /// Crée le mapping pour BingMaps Search
        /// </summary>
        private void CreateBingMapsFields()
        {

        }

        private void CreateSection()
        {
            _panelSectionDiv = new Panel();
            _panelSectionDiv.CssClass = "edaAcaSection";
            _panelMainDiv.Controls.Add(_panelSectionDiv);
        }

        private HtmlGenericControl GetSpan(COLUMN columm)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");

            switch (columm)
            {
                case COLUMN.LABEL:
                    span.Attributes.Add("class", "edaAcaLabel");
                    break;
                case COLUMN.LIST:
                    span.Attributes.Add("class", "edaAcaList");
                    break;
                case COLUMN.ADDCATALOGVALUE:
                    span.Attributes.Add("class", "edaAcaAddCatalogValue");
                    break;
                default:
                    break;
            }

            return span;
        }

        private void CreateTitle(string text)
        {
            Panel field = new Panel();
            field.CssClass = "title";
            _panelSectionDiv.Controls.Add(field);

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = text;
            field.Controls.Add(span);
        }

        private void CreateSubtitle(string text)
        {
            Panel field = new Panel();
            field.CssClass = "subtitle";
            _panelSectionDiv.Controls.Add(field);

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = text;
            field.Controls.Add(span);
        }

        private void CreateRubriques(string label, string name, IEnumerable<eFieldLiteWithLib> datasource, eFilemapPartner mapping = null, int selectedVal = 0, bool isSearchField = false)
        {
            int mappingId = 0;
            int selectedValue = (selectedVal > 0) ? selectedVal : 0;
            bool addCatValue = false;
            eFieldLiteWithLib selectedField = null;

            if (mapping != null)
            {
                mappingId = mapping.Id;
                selectedValue = mapping.DescId;
                addCatValue = mapping.CreateCatalogValue;
            }

            Panel field = new Panel();
            field.ID = String.Concat("field", name);
            field.CssClass = "field";

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanList = GetSpan(COLUMN.LIST);
            HtmlGenericControl spanCatValue = GetSpan(COLUMN.ADDCATALOGVALUE);
            field.Controls.Add(spanLabel);
            field.Controls.Add(spanList);
            field.Controls.Add(spanCatValue);

            string selectName = String.Concat("ddl", name);

            HtmlGenericControl lbl = new HtmlGenericControl("label");
            lbl.Attributes.Add("id", String.Concat("lbl", name));
            lbl.Attributes.Add("for", selectName);
            lbl.InnerText = label;
            spanLabel.Controls.Add(lbl);

            #region Liste des champs
            HtmlGenericControl ddl = new HtmlGenericControl("select");
            ddl.Attributes.Add("id", selectName);
            ddl.Attributes.Add("name", selectName);
            ddl.Attributes.Add("edaAcaMpid", mappingId.ToString());
            ddl.Attributes.Add("edaAcaOldvalue", selectedValue.ToString());
            if (isSearchField)
                ddl.Attributes.Add("onchange", "nsAdmin.adminAutocompletionToggleSearch(this);");
            else
                ddl.Attributes.Add("onchange", "nsAdmin.adminAutocompletionToggleField(this);");

            spanList.Controls.Add(ddl);

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "0");
            option.InnerText = eResApp.GetRes(Pref, 6211);
            if (selectedValue == 0)
                option.Attributes.Add("selected", "selected");
            ddl.Controls.Add(option);

            foreach (eFieldLiteWithLib fld in datasource)
            {
                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", fld.Descid.ToString());
                option.InnerText = fld.Libelle;

                if (selectedValue == fld.Descid)
                {
                    selectedField = fld;
                    option.Attributes.Add("selected", "selected");
                }

                option.Attributes.Add("data-advcat", (fld.Popup == PopupType.DATA) ? "1" : "0");

                if (!isSearchField && selectedValue != fld.Descid && _listMappingDescid.Contains(fld.Descid))
                    option.Attributes.Add("disabled", "disabled");

                ddl.Controls.Add(option);
            }
            #endregion


            #region Case à cocher pour indiquer si on ajoute la valeur dans le catalogue

            if (!isSearchField)
            {
                bool disabled = true;
                if (selectedField != null)
                    disabled = selectedField.Popup != PopupType.DATA;
                eCheckBoxCtrl checkbox = new eCheckBoxCtrl(addCatValue, disabled);
                checkbox.AddClass("chkAddCatValue");
                checkbox.AddText(eResApp.GetRes(Pref, 8330));
                checkbox.AddClick();
                spanCatValue.Controls.Add(checkbox);
            }

            #endregion

            _panelSectionDiv.Controls.Add(field);
        }

        private void CreateRadioBox(string name, bool selectedValue = true, bool radioboxEnabled = true)
        {
            Panel field = new Panel();
            field.ID = String.Concat("field", name);
            field.CssClass = "field";

            RadioButtonList rbl = new RadioButtonList();
            rbl.ID = String.Concat("rbl", name);
            rbl.RepeatDirection = RepeatDirection.Horizontal;
            rbl.Items.Add(new ListItem(eResApp.GetRes(Pref, 58), "true"));
            rbl.Items.Add(new ListItem(eResApp.GetRes(Pref, 59), "false"));
            rbl.Enabled = radioboxEnabled;
            if (selectedValue)
                rbl.SelectedValue = "true";
            else
                rbl.SelectedValue = "false";
            field.Controls.Add(rbl);

            _panelSectionDiv.Controls.Add(field);
        }
    }
}
