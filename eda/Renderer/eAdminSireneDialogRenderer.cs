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
    /// Renderer gérant l'affichage de la fenêtre de mapping pour l'extension Sirene
    /// </summary>
    public class eAdminSireneDialogRenderer : eAdminRenderer
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

        string _ppTabName = String.Empty;
        string _pmTabName = String.Empty;
        string _adrTabName = String.Empty;

        private IEnumerable<eFieldLiteWithLib> _listAvailableFields;
        private IEnumerable<eFieldLiteWithLib> _listAvailableSearchFields;

        Dictionary<SireneModel.Entreprise, eSireneMapping> _sireneEntrepriseMapping;
        Dictionary<SireneModel.Etablissement, eSireneMapping> _sireneEtablissementMapping;
        List<SireneResultMetaDataField> _sireneFields;
        List<SireneResultMetaDataCategory> _sireneCategories;

        /// <summary>
        /// Rubrique déclencheuse n°1
        /// </summary>
        int _sireneDescId1 = 0;
        /// <summary>
        /// Rubrique déclencheuse n°2
        /// </summary>
        int _sireneDescId2 = 0;
        /// <summary>
        /// Rubrique accueillant le résultat complet de recherche sélectionné (adresse complète) - US #1224
        /// </summary>
        int _sireneResultLabelDescId = 0;

        //bool _sireneMidFormulaEnabled = false;
        //bool _sirenePmAutomationEnabled = false;

        private List<eFilemapPartner> _listMappings;
        private List<int> _listMappingDescid;
        private List<int> _listSelectedDescId = new List<int>();

        private Panel _panelMainDiv;
        private Panel _panelSectionDiv;
        private List<string> _internalErrors = new List<string>();
        #endregion

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminSireneDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
        }

        /// <summary>
        /// Création du renderer de la fenêtre d'administration du mapping Sirene
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="nTab">TabID du champ pour lequel le mapping doit être paramétré</param>
        /// <returns>Le renderer généré</returns>
        public static eAdminSireneDialogRenderer CreateAdminSireneDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminSireneDialogRenderer(pref, nTab);
        }

        /// <summary>
        /// Initialisation du renderer
        /// </summary>
        /// <returns>true si initialisation effectuée, false si erreur</returns>
        protected override bool Init()
        {

            if (base.Init())
            {
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

        /// <summary>
        /// Renvoie la liste des champs disponibles
        /// </summary>
        /// <param name="pref">Objet ePref</param>
        /// <param name="tab">TabID du fichier concerné</param>
        /// <param name="onlyThisFormats">Si précisée, liste des formats acceptés (les autres seront filtrés)</param>
        /// <param name="onlyThisPopupType">Si précisée, liste des types de catalogue acceptés (les autres seront filtrés)</param>
        /// <returns>liste des champs disponibles</returns>
        public static IEnumerable<eFieldLiteWithLib> GetTabFields(ePref pref, int tab,
            IEnumerable<FieldFormat> onlyThisFormats, IEnumerable<PopupType> onlyThisPopupType = null)
        {
            IEnumerable<eFieldLiteWithLib> list = RetrieveFields.GetDefault(pref)
                .AddOnlyThisTabs(new int[] { tab })
                .AddOnlyThisFormats(onlyThisFormats)
                .AddOnlyThisPopupType(onlyThisPopupType)
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(pref));

            // On exclut les rubriques avec libellé vide (notamment les cibles étendues)
            list = list?.Where(fld => !String.IsNullOrEmpty(fld.Libelle));

            // D'autres filtrages seront ensuite effectués selon contexte par les méthodes appelantes
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
                // Récupération de la liste des champs mappables
                error = eSireneMapping.GetSireneFieldsAndCategories(Pref, out _sireneFields, out _sireneCategories);
                if (error.Length > 0)
                {
                    _internalErrors.Add(error);
                    //throw new Exception(error);
                }

                // Liste des rubriques proposées comme champs déclencheurs : Caractère
                // TOCHECK Fonctionnel : non catalogue ?
                _listAvailableSearchFields = GetTabFields(Pref, _nTab, new FieldFormat[] { FieldFormat.TYP_CHAR, FieldFormat.TYP_NUMERIC }, new PopupType[] { PopupType.NONE });

                // Liste des rubriques proposées pour le mapping
                // D'autres filtrages seront ensuite effectués selon contexte par les méthodes appelantes
                _listAvailableFields = GetTabFields(Pref, _nTab, null, new PopupType[] { PopupType.NONE, PopupType.FREE, PopupType.ONLY, PopupType.DATA });
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
            string[] sireneEnabledFields = eSireneMapping.GetSireneEnabledFields(Pref, _nTab);
            if (sireneEnabledFields.Length > 0)
                int.TryParse(sireneEnabledFields[0], out _sireneDescId1);
            if (sireneEnabledFields.Length > 1)
                int.TryParse(sireneEnabledFields[1], out _sireneDescId2);
            #endregion

            #region Chargement des rubriques mappées avec les champs proposés par le référentiel Sirene
            _sireneEntrepriseMapping = eSireneMapping.GetEntrepriseMapping(_nTab, Pref, out sError);
            if (sError.Length > 0)
                _internalErrors.Add(sError);

            _sireneEtablissementMapping = eSireneMapping.GetEtablissementMapping(_nTab, Pref, out sError);
            if (sError.Length > 0)
                _internalErrors.Add(sError);

            _listMappings = eAutoCompletionTools.Load(Pref, _nTab, EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE, EudoQuery.AutoCompletionType.AUTO_COMPLETION_ADDRESS, true, _sireneFields, out sError);
            if (!String.IsNullOrEmpty(sError))
                _internalErrors.Add(sError);

            #region Chargement de la rubrique "Adresse complète" - US #1224 - Récupération depuis le mapping chargé via eAutoCompletionTools.Load (évite de recharger 2 fois depuis DESCADV)
            eFilemapPartner sireneResultLabelMapping = _listMappings.Find(m => m.Source == eModelConst.AutocompleteAddressMappings.LABEL);
            if (sireneResultLabelMapping != null)
                _sireneResultLabelDescId = sireneResultLabelMapping.DescId;
            #endregion

            // Recensement des champs déjà mappés
            if (_listMappings != null)
            {
                List<eFilemapPartner> listTemp = _listMappings.FindAll(m => m.DescId > 0);
                _listMappingDescid = new List<int>();
                foreach (eFilemapPartner mapping in listTemp)
                {
                    if (mapping != null && !_listMappingDescid.Contains(mapping.DescId))
                        _listMappingDescid.Add(mapping.DescId);
                }
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

        /// <summary>
        /// Construction du contenu de la fenêtre
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.ID = "sireneAdminModalContent";
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

        /// <summary>
        /// Finalisation du rendu de la fenêtre
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            if (_internalErrors != null && _internalErrors.Count > 0)
                throw new Exception(String.Join(String.Empty, _internalErrors));

            return base.End();
        }

        private void CreateMainDiv()
        {
            _panelMainDiv = new Panel();
            _panelMainDiv.ID = "edaSireneMainDiv";

            #region Champ déclencheur recherche
            // Section et titre
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 8560)); // Rubriques depuis lesquelles est déclenchée la recherche pour suggérer des établissements

            // Ajout d'un sous-groupe sans titre visible pour l'alignement des champs déclencheurs sur les champs du mapping
            Panel sectionPanel = GetSubtitle(String.Empty);
            _panelSectionDiv.Controls.Add(sectionPanel);

            // Ajout des listes
            sectionPanel.Controls.Add(GetFieldDropDownList(String.Concat(eResApp.GetRes(Pref, 7032), " 1"), String.Empty, null, "searchField1", _listAvailableSearchFields, selectedVal: _sireneDescId1, isSearchField: true, bCatalogAllowed: false));
            sectionPanel.Controls.Add(GetFieldDropDownList(String.Concat(eResApp.GetRes(Pref, 7032), " 2"), String.Empty, null, "searchField2", _listAvailableSearchFields, selectedVal: _sireneDescId2, isSearchField: true, bCatalogAllowed: false));
            #endregion

            #region Champs des mappings
            // Section et titre
            CreateSection();
            CreateTitle(eResApp.GetRes(Pref, 8561)); // Rubriques mises à jour après sélection de l'un des établissements suggérés

            // Ajout des listes
            CreateMappingFields();
            #endregion

            _pgContainer.Controls.Add(_panelMainDiv);
        }

        /// <summary>
        /// Crée le mapping 
        /// </summary>
        private void CreateMappingFields()
        {
            // US #1224 - Champ "Adresse complète" recueillant l'ensemble des champs affichés dans la fenêtre de résultats
            // Il est proposé en premier dans la liste des champs à mapper, hors catégorie, dans une section dite "Système"
            List<Panel> systemFieldsList = new List<Panel>();
            systemFieldsList.Add(GetFieldDropDownList(eResApp.GetRes(Pref, 2484), String.Empty, null, "resultLabelField", _listAvailableSearchFields, selectedVal: _sireneResultLabelDescId, isSearchField: false, bCatalogAllowed: false));
            _panelSectionDiv.Controls.Add(GetFieldsSubSection("SYSTEM", eResApp.GetRes(Pref, 2484), systemFieldsList));

            if (_sireneCategories != null && _sireneFields != null)
            {
                // Pour chaque catégorie disponible, on affiche les champs disponibles
                foreach (SireneResultMetaDataCategory category in _sireneCategories)
                {
                    // Pas de mapping pour les champs faisant partie de la catégorie "Mise à jour"
                    if (category.ID.StartsWith("MAJ_"))
                        continue;

                    // Liste des contrôles à afficher
                    List<Panel> fieldsList = GetFieldControlsList(category.ID, _sireneFields.FindAll(f => f.Category.Equals(category.ID)), _listAvailableFields);

                    // Regroupement des contrôles en sections et ajout des contrôles sur la page
                    _panelSectionDiv.Controls.Add(GetFieldsSubSection(category.ID, category.Label, fieldsList));
                }
            }
        }

        private void CreateSection()
        {
            _panelSectionDiv = new Panel();
            _panelSectionDiv.CssClass = "edaSireneSection";
            _panelMainDiv.Controls.Add(_panelSectionDiv);
        }

        private HtmlGenericControl GetSpan(COLUMN columm)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");

            switch (columm)
            {
                case COLUMN.LABEL:
                    span.Attributes.Add("class", "edaSireneLabel");
                    break;
                case COLUMN.LIST:
                    span.Attributes.Add("class", "edaSireneList");
                    break;
                case COLUMN.ADDCATALOGVALUE:
                    span.Attributes.Add("class", "edaSireneAddCatalogValue");
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

        private Panel GetSubtitle(string text)
        {
            Panel field = new Panel();
            field.CssClass = "subtitle";

            if (!String.IsNullOrEmpty(text))
            {
                HtmlGenericControl span = new HtmlGenericControl("span");
                span.Attributes.Add("class", "edaSireneSubtitle");
                span.InnerText = text;
                field.Controls.Add(span);
            }

            return field;
        }

        private Panel GetFieldDropDownList(string label, string tooltip, SireneResultMetaDataField sireneField, string name, IEnumerable<eFieldLiteWithLib> datasource, eSireneMapping existingMapping = null, int selectedVal = 0, bool isSearchField = false, bool bCatalogAllowed = false)
        {
            string mappingField = String.Empty;
            int mappingDescId = (selectedVal > 0) ? selectedVal : 0;
            bool mappingAddNewValue = false;
            eFieldLiteWithLib selectedField = null;

            if (existingMapping != null)
            {
                mappingField = existingMapping.Field;
                mappingDescId = existingMapping.DescId;
                mappingAddNewValue = existingMapping.AddNewValue;
            }

            string formattedTooltip = String.Concat(sireneField?.Field, Environment.NewLine, tooltip); // TOCHECK FONCTIONNEL - ajout du nom de la colonne Sirene dans le tooltip
            if (formattedTooltip.Trim().Length == 0)
                formattedTooltip = label;

            string documentationUrl = eSireneMapping.GetSireneFieldDocumentationURL(sireneField?.Field);

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
            lbl.Attributes.Add("title", formattedTooltip);
            if (documentationUrl.Length > 0)
            {
                lbl.Attributes.Add("onclick", String.Concat("window.open('", documentationUrl, "');"));
                lbl.Attributes.Add("class", "edaSireneClickableLabel");
            }
            lbl.InnerText = label;
            spanLabel.Controls.Add(lbl);

            #region Liste des champs
            HtmlGenericControl ddl = new HtmlGenericControl("select");
            ddl.Attributes.Add("id", selectName);
            ddl.Attributes.Add("name", selectName);
            ddl.Attributes.Add("edaSireneMpid", mappingField);
            ddl.Attributes.Add("edaSireneOldvalue", mappingDescId.ToString());
            if (isSearchField)
                ddl.Attributes.Add("onchange", "nsAdmin.adminSireneToggleSearch(this);");
            else
                ddl.Attributes.Add("onchange", "nsAdmin.adminSireneToggleField(this);");
            ddl.Attributes.Add("title", formattedTooltip);

            spanList.Controls.Add(ddl);

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "0");
            option.InnerText = eResApp.GetRes(Pref, 6211);
            if (mappingDescId == 0)
                option.Attributes.Add("selected", "selected");
            ddl.Controls.Add(option);

            foreach (eFieldLiteWithLib fld in datasource)
            {
                // On détermine si le champ est Système, afin de ne pas le proposer au mapping
                // On n'utilise pas IsSystem() ici, car cette fonction exclut également les DescID principaux (PM01, PP01...) qui sont effectivement système, mais ce sont ceux-là qu'on souhaitera
                // avant tout mapper avec les systèmes d'adresses prédictives.
                // TODO: méthode plus adaptée ?
                //bool bIsSystemField = eAdminFieldInfos.IsSystem(fld.Descid, fld.Table.EdnType);
                int iTab = eLibTools.GetTabFromDescId(fld.Descid);
                int iShortDescId = fld.Descid - iTab;
                bool bIsSystemField = iShortDescId >= eLibConst.MAX_NBRE_FIELD && iShortDescId != AllField.GEOGRAPHY.GetHashCode();

                bool bCanAddField = true;

                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", fld.Descid.ToString());
                option.InnerText = fld.Libelle;

                if (mappingDescId == fld.Descid)
                {
                    selectedField = fld;
                    option.Attributes.Add("selected", "selected");
                    if (!isSearchField)
                    {
                        if (!_listSelectedDescId.Contains(fld.Descid))
                            _listSelectedDescId.Add(fld.Descid);
                        else
                            field.CssClass = String.Concat(field.CssClass, " mappingConflict");
                    }
                    // Si le champ est à choix multiple, il ne devrait pas être mappé, car l'ajout dans ce type de catalogue n'est pas implémenté à l'heure actuelle
                    // Si un tel mapping existe, on le signale en rouge pour correction (le référentiel ne le renseignera pas)
                    // Même chose s'il s'agit d'un champ système
                    if (fld.Multiple || bIsSystemField)
                        field.CssClass = String.Concat(field.CssClass, " mappingConflict");
                }
                // Si le champ est à choix multiple, ou s'il s'agit d'un champ système, on ne le propose pas au mapping, car l'ajout dans ce type de catalogue ou de champ n'est pas implémenté à l'heure actuelle
                // Il doit toutefois figurer dans la datasource (non filtré en amont) pour pouvoir signaler un mapping erroné (cf. condition ci-dessus)
                else if (fld.Multiple || bIsSystemField)
                    bCanAddField = false;

                option.Attributes.Add("data-advcat", (fld.Popup == PopupType.DATA) ? "1" : "0");

                if (!isSearchField && mappingDescId != fld.Descid && _listMappingDescid.Contains(fld.Descid))
                    option.Attributes.Add("disabled", "disabled");

                if (bCanAddField)
                    ddl.Controls.Add(option);
            }
            #endregion


            #region Case à cocher pour indiquer si on ajoute la valeur dans le catalogue

            if (!isSearchField && bCatalogAllowed)
            {
                bool unavailable = true; // l'option n'est pas disponible (masquée)
                bool disabled = true; // l'option est désactivée, mais visible
                bool multiple = false;
                if (selectedField != null)
                {
                    unavailable = selectedField.Popup != PopupType.DATA;
                    multiple = selectedField.Multiple;
                    disabled = unavailable || multiple;
                }
                // L'ajout de valeurs dans les catalogues multiples n'est actuellement pas implémenté
                if (multiple)
                    mappingAddNewValue = false;

                eCheckBoxCtrl checkbox = new eCheckBoxCtrl(mappingAddNewValue, disabled);
                checkbox.ID = String.Concat(name, "_chkAddCatValue");
                checkbox.AddClass("chkAddCatValue");
                checkbox.AddText(eResApp.GetRes(Pref, 8330));
                checkbox.AddClick();
                if (unavailable)
                    checkbox.Style.Add("visibility", "hidden");
                spanCatValue.Controls.Add(checkbox);
            }

            #endregion

            return field;
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

        private List<Panel> GetFieldControlsList(string idPrefix, List<SireneResultMetaDataField> sireneFields, IEnumerable<eFieldLiteWithLib> fieldList)
        {
            string noFieldSelectionValueLabel = String.Concat("<", eResApp.GetRes(Pref, 7862), ">"); // Sélectionner une rubrique
            List<Panel> fieldControlsList = new List<Panel>();

            foreach (SireneResultMetaDataField sireneField in sireneFields)
            {
                // Filtrage de la liste des Field de la table pour ne retenir que ceux dont le type correspond à ceux acceptés par le champ mappé
                List<eFieldLiteWithLib> filteredFieldList = new List<eFieldLiteWithLib>();
                if (fieldList != null)
                {
                    foreach (eFieldLiteWithLib field in fieldList)
                    {
                        // #63 593 - Filtrages additionnels
                        if (eSireneMapping.GetAllowedFieldFormats(sireneField.Field, sireneFields).Contains(field.Format) &&
                            eSireneMapping.IsFieldMappable(field, sireneField.Field)
                        )
                            filteredFieldList.Add(field);
                    }
                }

                // Conversion de la liste de Field en dictionnaire <int, string> affichable en HTML, où int = DescId, et string = Libellé affiché (Table.champ)
                Dictionary<int, string> fieldListDescidLabel = new Dictionary<int, string>();
                foreach (eFieldLiteWithLib field in filteredFieldList)
                {
                    fieldListDescidLabel[field.Descid] = String.Concat(/*field.TableLibelle, ".", */field.Libelle);
                }

                int selectedValue = -1;
                string displayName = _sireneFields?.Find(d => d.Equals(sireneField))?.Label;
                bool bCatalogAllowed = _sireneFields?.Find(d => d.Equals(sireneField))?.Catalog ?? false;
                if (String.IsNullOrEmpty(displayName))
                    displayName = sireneField.Field; // en cas d'absence de donnée sur le libellé : utilisation du nom de la colonne
                string tooltip = _sireneFields?.Find(d => d.Equals(sireneField))?.Tooltip;
                eSireneMapping currentSireneMapping = null;
                SireneModel.Etablissement etablissementField;
                SireneModel.Entreprise entrepriseField;
                if (Enum.TryParse<SireneModel.Etablissement>(sireneField.Field, out etablissementField))
                    _sireneEtablissementMapping.TryGetValue(etablissementField, out currentSireneMapping);
                if (Enum.TryParse<SireneModel.Entreprise>(sireneField.Field, out entrepriseField))
                    _sireneEntrepriseMapping.TryGetValue(entrepriseField, out currentSireneMapping);

                // Si le champ a été retrouvé dans le mapping existant : paramétrage de son DescId
                if (currentSireneMapping != null)
                    selectedValue = currentSireneMapping.DescId;
                // Sinon, création d'un nouvel objet pour MAJ en base par la suite
                else
                    currentSireneMapping = new eSireneMapping(sireneField.Field, 0, false);

                Panel fieldPanel = GetFieldDropDownList(displayName, tooltip, sireneField, String.Concat(idPrefix, "_", sireneField.Field), filteredFieldList, currentSireneMapping, selectedValue, false, bCatalogAllowed);

                fieldControlsList.Add(fieldPanel);
            }

            return fieldControlsList;
        }

        private Panel GetFieldsSubSection(string id, string sectionLabel, List<Panel> fieldControlsList)
        {
            Panel sectionPanel = GetSubtitle(sectionLabel);
            Panel sectionPanelContainer = sectionPanel;

            // La présélection de la valeur dans la liste des tables se fait en récupérant le TabID du premier champ mappé pour cette section
            // Ex : pour Planning, si le premier champ mappé est Date de début, on prend son DescId et on sélectionne la table lui correspondant dans la liste
            int selectedValue = 0;
            eSireneMapping selectedSireneMapping = null;
            bool foundValue = false;
            switch (id)
            {
                case "etablissementIdentification":
                    foundValue =
                        _sireneEtablissementMapping.TryGetValue(SireneModel.Etablissement.SIREN, out selectedSireneMapping) ||
                        _sireneEtablissementMapping.TryGetValue(SireneModel.Etablissement.NIC, out selectedSireneMapping) ||
                        _sireneEtablissementMapping.TryGetValue(SireneModel.Etablissement.SIRET, out selectedSireneMapping);
                    break;
            }
            if (foundValue)
                selectedValue = selectedSireneMapping.DescId - selectedSireneMapping.DescId % 100;

            foreach (Panel fieldPanel in fieldControlsList)
            {
                sectionPanelContainer.Controls.Add(fieldPanel);
            }

            return sectionPanel;
        }

        /// <summary>
        /// Renvoie une liste déroulante avec la liste des champs appropriés au contexte donné
        /// </summary>
        /// <param name="field">Champ concerné</param>
        /// <param name="id">ID DOM/HTML à donner à la liste</param>
        /// <param name="section">Section concernée par la liste</param>
        /// <param name="label">Libellé à afficher</param>
        /// <param name="selectedValue">Valeur initialement sélectionnée dans la liste</param>
        /// <param name="allItemsValueLabel">Valeur à utiliser pour matérialiser le choix <TOUS></TOUS></param>
        /// <param name="noSelectionValueLabel">Valeur à utiliser pour matérialiser le choix "Non renseigné"</param>
        /// <param name="fieldList">Liste des propositions à afficher dans la liste</param>
        /// <returns>Un objet Panel contenant le contrôle libellé + le contrôle liste</returns>
        public Panel GetFieldDropDownList(
    string field, string id, string section, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> fieldList
)
        {
            return GetDropDownList(field, section, id, label, selectedValue, allItemsValueLabel, noSelectionValueLabel, fieldList, "sireneField", "nsAdminSirene.changeField(this, true);");
        }

        private Panel GetDropDownList(
    string field, string section, string id, string label, int selectedValue, string allItemsValueLabel, string noSelectionValueLabel, Dictionary<int, string> itemList,
    string prefix, string jsOnChange
)
        {
            Panel fieldPanel = new Panel();
            fieldPanel.CssClass = "field";
            fieldPanel.ID = String.Concat(prefix, "List_", id);
            HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
            fieldLabel.InnerText = label;
            DropDownList ddl = new DropDownList();
            ddl.ID = String.Concat(prefix, "ListDdl_", id);
            if (itemList.ContainsKey(selectedValue))
                ddl.SelectedValue = selectedValue.ToString();
            ddl.Attributes.Add("onchange", jsOnChange);

            ddl.DataSource = itemList;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();

            string sSep = "--------------------";
            //Séparateur
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            if (noSelectionValueLabel.Trim().Length > 0)
                ddl.Items.Insert(0, new ListItem(noSelectionValueLabel, "-1"));

            if (allItemsValueLabel.Trim().Length > 0)
                ddl.Items.Insert(0, new ListItem(allItemsValueLabel, "0"));

            // Attributs concernant le champ dans la table de mapping
            string fieldValue = String.Empty;
            eSireneMapping currentSireneMapping = null;
            SireneModel.Etablissement etablissementField;
            Enum.TryParse<SireneModel.Etablissement>(field, out etablissementField);
            SireneModel.Entreprise entrepriseField;
            Enum.TryParse<SireneModel.Entreprise>(field, out entrepriseField);
            if (_sireneEntrepriseMapping.TryGetValue(entrepriseField, out currentSireneMapping) || _sireneEtablissementMapping.TryGetValue(etablissementField, out currentSireneMapping))
            {
                fieldValue = currentSireneMapping.Field;
            }
            fieldPanel.Attributes.Add("field", field);
            fieldPanel.Attributes.Add("section", section);
            List<string> availableTabIds = new List<string>();
            foreach (int descId in itemList.Keys)
            {
                int tabId = descId - descId % 100;
                if (!availableTabIds.Contains(tabId.ToString()))
                    availableTabIds.Add(tabId.ToString());
            }
            fieldPanel.Attributes.Add("tab", String.Join(";", availableTabIds.ToArray())); // TODO / TOCHECK ?
            fieldPanel.Attributes.Add("label", fieldValue.ToString()); // TODO / TOCHECK ?

            fieldPanel.Controls.Add(fieldLabel);
            fieldPanel.Controls.Add(ddl);

            return fieldPanel;
        }

    }
}
