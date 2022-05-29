using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminMapTooltipDialogRenderer : eAdminRenderer
    {
        #region constantes
        private const int _nbMainMappings = 5;
        #endregion

        #region propriétés
        private Int32 _nTab;
        private String _tabName;
        private eAdminTableInfos _tabInfos;

        Dictionary<int, string> _dicoTabs;
        List<eFieldLiteWithLib> _dicoFields;
        List<eFieldLiteWithLib> _dicoFieldsGeo;
        List<eFieldLiteWithLib> _dicoFieldsImage;

        private List<eFilemapPartner> _listMappings;
        private List<int> _listUsedMappingDescid;
        private IEnumerable<eFilemapPartner> _listMappingsGeo;
        private IEnumerable<eFilemapPartner> _listMappingsTitles;
        private IEnumerable<eFilemapPartner> _listMappingsSubtitles;
        private IEnumerable<eFilemapPartner> _listMappingsImages;
        private IEnumerable<eFilemapPartner> _listMappingsFields;

        private Panel _panelLeftDiv;
        private Panel _panelRightDiv;
        private Panel _panelSectionDiv;
        #endregion

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminMapTooltipDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
        }

        public static eAdminMapTooltipDialogRenderer CreateAdminMapTooltipDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminMapTooltipDialogRenderer(pref, nTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                // Chargement des valeurs des dropDownLists
                LoadDropDownListValues();

                // Chargement des Mappings
                LoadMappings();

                return true;
            }
            else
                return false;
        }

        private static IEnumerable<eFieldLiteWithLib> GetTabFields(ePref pref, int tab, IEnumerable<FieldFormat> onlyThisFormats = null)
        {
            IEnumerable<eFieldLiteWithLib> list = RetrieveFields.GetDefault(pref)
                .AddOnlyThisTabs(new int[] { tab })
                .AddOnlyThisFormats(onlyThisFormats)
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
            ISet<FieldFormat> fieldFormats = new HashSet<FieldFormat>() {
                FieldFormat.TYP_CHAR,
                FieldFormat.TYP_DATE,
                FieldFormat.TYP_EMAIL,
                FieldFormat.TYP_PHONE,
                FieldFormat.TYP_WEB,
                FieldFormat.TYP_NUMERIC,
                FieldFormat.TYP_AUTOINC,
                FieldFormat.TYP_BIT
            };

            _dicoTabs = eAdminTools.GetListParentTabs(Pref, _tabInfos.DescId);
            if(!_dicoTabs.ContainsKey(_tabInfos.DescId))
                _dicoTabs.Add(_tabInfos.DescId, _tabInfos.TableLabel);

            _dicoFields = new List<eFieldLiteWithLib>();
            _dicoFieldsImage = new List<eFieldLiteWithLib>();
            _dicoFieldsGeo = new List<eFieldLiteWithLib>();

            foreach (KeyValuePair<int, string> tab in _dicoTabs)
            {
                _dicoFields.AddRange(GetTabFields(Pref, tab.Key, fieldFormats));
                _dicoFieldsImage.AddRange(GetTabFields(Pref, tab.Key, new FieldFormat[] { FieldFormat.TYP_IMAGE }));
                _dicoFieldsGeo.AddRange(GetTabFields(Pref, tab.Key, new FieldFormat[] { FieldFormat.TYP_GEOGRAPHY_V2 }));
            }
        }

        private void LoadMappings()
        {
            string sError;
            _listMappings = eFilemapPartner.LoadCartoMapping(Pref, _nTab, out sError);

            _listMappingsGeo = _listMappings.Where(mp => mp.SsType == CartographySsType.GEOGRAPHY.GetHashCode());
            _listMappingsTitles = _listMappings.Where(mp => mp.SsType == CartographySsType.TITLE.GetHashCode());
            _listMappingsSubtitles = _listMappings.Where(mp => mp.SsType == CartographySsType.SUBTITLE.GetHashCode());
            _listMappingsImages = _listMappings.Where(mp => mp.SsType == CartographySsType.IMAGE.GetHashCode());
            _listMappingsFields = _listMappings.Where(mp => mp.SsType == CartographySsType.FIELD.GetHashCode());


            _listUsedMappingDescid = new List<int>();
            eFilemapPartner mappingTemp = _listMappingsGeo.FirstOrDefault();
            if (mappingTemp != null && !_listUsedMappingDescid.Contains(mappingTemp.DescId))
                _listUsedMappingDescid.Add(mappingTemp.DescId);

            mappingTemp = _listMappingsTitles.FirstOrDefault();
            if (mappingTemp != null && !_listUsedMappingDescid.Contains(mappingTemp.DescId))
                _listUsedMappingDescid.Add(mappingTemp.DescId);

            mappingTemp = _listMappingsSubtitles.FirstOrDefault();
            if (mappingTemp != null && !_listUsedMappingDescid.Contains(mappingTemp.DescId))
                _listUsedMappingDescid.Add(mappingTemp.DescId);

            mappingTemp = _listMappingsImages.FirstOrDefault();
            if (mappingTemp != null && !_listUsedMappingDescid.Contains(mappingTemp.DescId))
                _listUsedMappingDescid.Add(mappingTemp.DescId);

            for (int i = 1; i <= _nbMainMappings; ++i)
            {
                if (_listMappingsFields.Count() >= i)
                {
                    mappingTemp = _listMappingsFields.ElementAt(i - 1);
                    if (mappingTemp != null && !_listUsedMappingDescid.Contains(mappingTemp.DescId))
                        _listUsedMappingDescid.Add(mappingTemp.DescId);
                }
            }
        }

        protected override bool Build()
        {
            _pgContainer.ID = "mapTooltipAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            try
            {
                CreateLeftSide();
                CreateRightSide();
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

        private bool GetMappingDispLib(eFilemapPartner mapping)
        {
            if (mapping != null && mapping.SourceType == EudoQuery.CartographySourceType.LABEL_DISPLAY.GetHashCode())
                return true;
            else
                return false;
        }

        private void CreateLeftSide()
        {
            _panelLeftDiv = new Panel();
            _panelLeftDiv.ID = "edaMpTtLeftDiv";
            _pgContainer.Controls.Add(_panelLeftDiv);

            CreateHeaderRubriques();

            //Géocodage
            CreateSection();
            eFilemapPartner mappingTemp = _listMappingsGeo.FirstOrDefault();
            if (mappingTemp != null)
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 7108), " :"), "Geography", _dicoTabs, _dicoFieldsGeo, mappingId: mappingTemp.Id, selectedValue: mappingTemp.DescId, chkbxLibChecked: GetMappingDispLib(mappingTemp), chkbxLibDisplayed: false);
            else
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 7108), " :"), "Geography", _dicoTabs, _dicoFieldsGeo, chkbxLibDisplayed: false);

            //Titre
            CreateSection("edaMpTtRightDivTitle");
            mappingTemp = _listMappingsTitles.FirstOrDefault();
            if (mappingTemp != null)
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 6167), " :"), "Title", _dicoTabs, _dicoFields, mappingId: mappingTemp.Id, selectedValue: mappingTemp.DescId, chkbxLibChecked: GetMappingDispLib(mappingTemp));
            else
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 6167), " :"), "Title", _dicoTabs, _dicoFields);

            //Sous-titre
            CreateSection("edaMpTtRightDivSubtitle");
            mappingTemp = _listMappingsSubtitles.FirstOrDefault();
            if (mappingTemp != null)
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 7109), " :"), "Subtitle", _dicoTabs, _dicoFields, mappingId: mappingTemp.Id, selectedValue: mappingTemp.DescId, chkbxLibChecked: GetMappingDispLib(mappingTemp));
            else
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 7109), " :"), "Subtitle", _dicoTabs, _dicoFields);

            //Rubriques
            CreateSection("edaMpTtRightDivRubrique");
            for (int i = 1; i <= _nbMainMappings; ++i)
            {
                int id = 0;
                int value = 0;
                bool libChecked = false;

                if (_listMappingsFields.Count() >= i)
                {
                    mappingTemp = _listMappingsFields.ElementAt(i - 1);

                    id = mappingTemp.Id;
                    value = mappingTemp.DescId;
                    libChecked = GetMappingDispLib(mappingTemp);
                }

                string label = String.Concat(eResApp.GetRes(Pref, 222), " ", i, " :");
                string name = String.Concat("Rubrique", i);

                CreateRubriques(label, name, _dicoTabs, _dicoFields, mappingId: id, selectedValue: value, chkbxLibChecked: libChecked);
            }

            //Image
            CreateSection("edaMpTtRightDivImage");
            mappingTemp = _listMappingsImages.FirstOrDefault();
            if (mappingTemp != null)
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 1216), " :"), "Image", _dicoTabs, _dicoFieldsImage, mappingId: mappingTemp.Id, selectedValue: mappingTemp.DescId, chkbxLibDisplayed: false);
            else
                CreateRubriques(String.Concat(eResApp.GetRes(Pref, 1216), " :"), "Image", _dicoTabs, _dicoFieldsImage, chkbxLibDisplayed: false);
        }

        private enum COLUMN
        {
            LABEL = 1,
            LISTTAB = 2,
            LISTFIELD = 3,
            LIBELLE = 4
        }
        private HtmlGenericControl GetSpan(COLUMN columm)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");

            switch (columm)
            {
                case COLUMN.LABEL:
                    span.Attributes.Add("class", "edaMpTtLabel");
                    break;
                case COLUMN.LISTTAB:
                    span.Attributes.Add("class", "edaMpTtListTab");
                    break;
                case COLUMN.LISTFIELD:
                    span.Attributes.Add("class", "edaMpTtListField");
                    break;
                case COLUMN.LIBELLE:
                    span.Attributes.Add("class", "edaMpTtLibelle");
                    break;
                default:
                    break;
            }

            return span;
        }

        private void CreateHeaderRubriques()
        {
            Panel headerSection = new Panel();
            headerSection.CssClass = "headerSection";

            Panel field = new Panel();
            field.CssClass = "field";
            headerSection.Controls.Add(field);

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanList = GetSpan(COLUMN.LISTTAB);
            HtmlGenericControl spanLibelle = GetSpan(COLUMN.LIBELLE);

            spanList.InnerText = eResApp.GetRes(Pref, 7022);
            spanLibelle.InnerText = eResApp.GetRes(Pref, 223);
            spanLibelle.Attributes.Add("title", eResApp.GetRes(Pref, 7275));

            //field.Controls.Add(spanLabel);
            field.Controls.Add(spanList);
            field.Controls.Add(spanLibelle);

            _panelLeftDiv.Controls.Add(headerSection);
        }

        private void CreateSection(string hoverDivId = "")
        {
            _panelSectionDiv = new Panel();
            _panelSectionDiv.CssClass = "edaMpTtSection";
            _panelLeftDiv.Controls.Add(_panelSectionDiv);

            if (!String.IsNullOrEmpty(hoverDivId))
            {
                _panelSectionDiv.Attributes.Add("onmouseover", String.Concat("nsAdmin.adminMapTooltipHoverField('", hoverDivId, "', true);"));
                _panelSectionDiv.Attributes.Add("onmouseout", String.Concat("nsAdmin.adminMapTooltipHoverField('", hoverDivId, "', false);"));
            }
        }


        private void CreateRubriques(string label, string name, Dictionary<int, string> datasourceTab, List<eFieldLiteWithLib> datasourceField, int mappingId = 0, int selectedValue = 0, bool chkbxLibDisplayed = true, bool chkbxLibChecked = false)
        {
            Panel field = new Panel();
            field.ID = String.Concat("field", name);
            field.CssClass = "field";

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanListTab = GetSpan(COLUMN.LISTTAB);
            HtmlGenericControl spanListField = GetSpan(COLUMN.LISTFIELD);
            HtmlGenericControl spanLibelle = GetSpan(COLUMN.LIBELLE);
            field.Controls.Add(spanLabel);
            field.Controls.Add(spanListTab);
            field.Controls.Add(spanListField);
            field.Controls.Add(spanLibelle);

            string selectTabName = String.Concat("ddlTabs", name);
            string selectFieldName = String.Concat("ddlFields", name);

            HtmlGenericControl lbl = new HtmlGenericControl("label");
            lbl.Attributes.Add("id", String.Concat("lbl", name));
            lbl.Attributes.Add("for", selectFieldName);
            lbl.InnerText = label;
            spanLabel.Controls.Add(lbl);

            HtmlGenericControl ddlTabs = new HtmlGenericControl("select");
            ddlTabs.Attributes.Add("id", selectTabName);
            ddlTabs.Attributes.Add("name", selectTabName);
            ddlTabs.Attributes.Add("onchange", String.Concat("nsAdmin.adminMapTooltipToggleTab(this, '", selectFieldName, "');"));

            spanListTab.Controls.Add(ddlTabs);

            int tabValue = selectedValue - (selectedValue % 100);
            PopulateDropDownList(ddlTabs, datasourceTab, tabValue, false);

            HtmlGenericControl ddlFields = new HtmlGenericControl("select");
            ddlFields.Attributes.Add("id", selectFieldName);
            ddlFields.Attributes.Add("name", selectFieldName);
            ddlFields.Attributes.Add("edaMpTtMpId", mappingId.ToString());
            ddlFields.Attributes.Add("edaMpTtOldvalue", selectedValue.ToString());
            ddlFields.Attributes.Add("onchange", "nsAdmin.adminMapTooltipToggleField(this);");

            spanListField.Controls.Add(ddlFields);

            PopulateDropDownList(ddlFields, datasourceField.ToDictionary(d => d.Descid, d => d.Libelle), selectedValue, true);

            if (chkbxLibDisplayed)
            {
                eCheckBoxCtrl chkbx = new eCheckBoxCtrl(chkbxLibChecked, false);
                chkbx.ID = String.Concat("chkbxLib", name);
                chkbx.AddClick();
                spanLibelle.Controls.Add(chkbx);
            }

            _panelSectionDiv.Controls.Add(field);
        }

        private void PopulateDropDownList(HtmlGenericControl ddl, Dictionary<int, string> datasource, int selectedValue = 0, bool isFields = false)
        {
            int tabValue = selectedValue - (selectedValue % 100);

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "0");
            option.InnerText = eResApp.GetRes(Pref, 6211);
            if (selectedValue == 0)
                option.Attributes.Add("selected", "selected");
            ddl.Controls.Add(option);

            foreach (KeyValuePair<int, string> kvp in datasource)
            {
                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", kvp.Key.ToString());
                option.InnerText = kvp.Value;

                if (isFields)
                {
                    int optionTab = kvp.Key - (kvp.Key % 100);
                    option.Attributes.Add("edaOptTab", optionTab.ToString());

                    if (tabValue != optionTab)
                        option.Style.Add("display", "none");
                }

                if (selectedValue == kvp.Key)
                    option.Attributes.Add("selected", "selected");

                if (selectedValue != kvp.Key && _listUsedMappingDescid.Contains(kvp.Key))
                    option.Attributes.Add("disabled", "disabled");

                ddl.Controls.Add(option);
            }
        }

        private void CreateRightSide()
        {
            _panelRightDiv = new Panel();
            _panelRightDiv.ID = "edaMpTtRightDiv";
            _pgContainer.Controls.Add(_panelRightDiv);

            Panel divCentered = new Panel();
            divCentered.ID = "edaMpTtImgContainer";
            _panelRightDiv.Controls.Add(divCentered);

            HtmlGenericControl image = new HtmlGenericControl("img");
            image.Attributes.Add("src", "themes/default/images/eda/eAdmin-infobullePosition-Preview.png");
            image.Attributes.Add("alt", "eAdmin-infobullePosition-Preview.png");
            divCentered.Controls.Add(image);

            foreach (string name in new List<string>() { "Title", "Subtitle", "Image", "Rubrique" })
            {
                Panel div = new Panel();
                div.ID = String.Concat("edaMpTtRightDiv", name);
                div.CssClass = "edaMpTtRightDivs";
                divCentered.Controls.Add(div);
            }
        }
    }
}