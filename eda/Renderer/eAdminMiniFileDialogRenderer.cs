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

    /// <summary>
    /// Classe de rendu de la modale d'admin des MiniFiches
    /// </summary>
    public class eAdminMiniFileDialogRenderer : eAdminRenderer
    {
        #region constantes
        private const int _nbMainMappings = 6;
        private const int _nbPmMappings = 4;
        private const int _nbPpMappings = 4;
        private const int _nbPrtMappings = 4;
        #endregion

        #region Enums
        private enum COLUMN
        {
            LABEL = 1,
            LIST = 2,
            LIBELLE = 3
        }
        #endregion

        #region Classes
        /// <summary>
        /// Infos d'une table nécessaires à l'admin des minifiche
        /// </summary>
        public class MyTabInfo
        {
            /// <summary>DescId de la table</summary>
            public int DescId { get; private set; }
            /// <summary>Libelle de la table</summary>
            public String Libelle { get; private set; }
            /// <summary>Liste des champs de la table</summary>
            public IEnumerable<eFieldLiteMiniFileAdmin> Fields { get; private set; }
            /// <summary>Liste des champs de la table de type image</summary>
            public IEnumerable<eFieldLiteMiniFileAdmin> FieldsImage
            {
                get
                {
                    return Fields.Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE);
                }
            }

            /// <summary>
            /// Constructeur par defaut
            /// </summary>
            /// <param name="pref">pref</param>
            /// <param name="descId">descid de la table</param>
            /// <param name="libelle">libelle de la table</param>
            public MyTabInfo(ePref pref, int descId, String libelle)
            {
                this.DescId = descId;
                this.Libelle = libelle;
                this.Fields = GetTabFields(pref, descId);
            }
        }
        #endregion

        #region Propriétés privées
        private Int32 _nTab;
        private String _tabName;
        private eAdminTableInfos _tabInfos;
        private int _widgetId = 0;

        private string _pmTabName = String.Empty;
        private string _ppTabName = String.Empty;
        private string _parentTabName = String.Empty;
        private Dictionary<int, String> _dicResTabs;

        private IEnumerable<eFieldLiteMiniFileAdmin> _listTabFields;
        private IEnumerable<eFieldLiteMiniFileAdmin> _listTabFieldsImage;
        private IEnumerable<eFieldLiteMiniFileAdmin> _listPMFields;
        private IEnumerable<eFieldLiteMiniFileAdmin> _listPPFields;
        private IEnumerable<eFieldLiteMiniFileAdmin> _listParentTabFields;


        private Panel _panelLeftDiv;
        private Panel _panelRightDiv;
        private Panel _panelSectionDiv;

        private MiniFileType _minifileType;

        private List<eMiniFileParam> _minifileParams = new List<eMiniFileParam>();
        #endregion


        #region Constructeur(s)
        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nTab">Table de la mini-fiche</param>
        /// <param name="type">Type de mini-fiche</param>
        private eAdminMiniFileDialogRenderer(ePref pref, MiniFileType type, int nTab, int widgetId)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
            _minifileType = type;
            _widgetId = widgetId;
        }
        #endregion


        #region Méthodes statiques
        /// <summary>
        /// Appel le constructeur de la modale
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="type">Type de mini-fiche</param>
        /// <param name="widgetId">ID du widget, facultatif</param>
        /// <returns></returns>
        public static eAdminMiniFileDialogRenderer CreateAdminMiniFileDialogRenderer(ePref pref, MiniFileType type, int nTab, int widgetId = 0)
        {
            return new eAdminMiniFileDialogRenderer(pref, type, nTab, widgetId);
        }

        /// <summary>
        /// Retourne tous les champs de la table passée en paramètre, trié par libellé
        /// </summary>
        /// <param name="pref">pref</param>
        /// <param name="tab">descid de la table</param>
        /// <returns></returns>
        public static IEnumerable<eFieldLiteMiniFileAdmin> GetTabFields(ePref pref, int tab)
        {
            IEnumerable<eFieldLiteMiniFileAdmin> list = RetrieveFields.GetDefault(pref)
                .AddExcludeFormats(new List<FieldFormat>() { FieldFormat.TYP_PASSWORD })
                .AddOnlyThisTabs(new int[] { tab })
                .ResultFieldsInfo(eFieldLiteMiniFileAdmin.Factory(pref));

            // Exclus les rubriques avec libelle vide (cibles etendues)
            list = list?.Where(fld => !String.IsNullOrEmpty(fld.Libelle));

            // Tri par libelle
            list = list?.OrderBy(fld => fld.Libelle);

            if (list != null)
                return list;

            return new List<eFieldLiteMiniFileAdmin>();
        }

        #endregion


        #region Méthodes override
        /// <summary>
        /// Initialisation des paramètres
        /// </summary>
        /// <returns>true si les paramètres ont été initialisés avec succès</returns>
        protected override bool Init()
        {
            // Chargement des RES des tables
            LoadResTab();

            // Chargement des valeurs des dropDownLists
            LoadDropDownListValues();

            // Chargement des Mappings
            _minifileParams = eMiniFileParam.GetParams(this.Pref, _minifileType, this._nTab, this._widgetId);

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            _pgContainer.ID = "miniFileAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            try
            {
                CreateLeftSide();
                CreateRightSide();
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = "Erreur lors du rendu de la page";
                return false;
            }

            return base.Build();
        }
        #endregion


        #region Méthodes privées

        /// <summary>
        /// Chargement des RES
        /// </summary>
        private void LoadResTab()
        {
            List<string> listDescIds = new List<string>();
            _dicResTabs = new Dictionary<int, string>();
            _dicResTabs.Add(_tabInfos.DescId, _tabName);

            if (_tabInfos.InterPM)
                listDescIds.Add(EudoQuery.TableType.PM.GetHashCode().ToString());

            if (_tabInfos.InterPP)
                listDescIds.Add(EudoQuery.TableType.PP.GetHashCode().ToString());

            if (_tabInfos.InterEVT)
                listDescIds.Add(_tabInfos.InterEVTDescid.ToString());

            if (listDescIds.Count() > 0)
            {
                eRes res = new eRes(Pref, String.Join(",", listDescIds.ToArray()));

                bool bResFound = false;

                if (_tabInfos.InterPM)
                {
                    bResFound = false;
                    int descidTab = EudoQuery.TableType.PM.GetHashCode();
                    _pmTabName = res.GetRes(descidTab, out bResFound);
                    if (!bResFound)
                        _pmTabName = "Sociétés";

                    if (!_dicResTabs.ContainsKey(descidTab))
                        _dicResTabs.Add(descidTab, _pmTabName);
                }

                if (_tabInfos.InterPP)
                {
                    bResFound = false;
                    int descidTab = EudoQuery.TableType.PP.GetHashCode();
                    _ppTabName = res.GetRes(descidTab, out bResFound);
                    if (!bResFound)
                        _ppTabName = "Contacts";

                    if (!_dicResTabs.ContainsKey(descidTab))
                        _dicResTabs.Add(descidTab, _ppTabName);
                }

                if (_tabInfos.InterEVT)
                {
                    bResFound = false;
                    int descidTab = _tabInfos.InterEVTDescid;
                    _parentTabName = res.GetRes(descidTab, out bResFound);
                    if (!bResFound)
                        _parentTabName = eResApp.GetRes(this.Pref, 7251);

                    if (!_dicResTabs.ContainsKey(descidTab))
                        _dicResTabs.Add(_tabInfos.InterEVTDescid, _parentTabName);
                }
            }
        }

        /// <summary>
        /// Chargement des listes déroulantes
        /// </summary>
        private void LoadDropDownListValues()
        {
            _listTabFields = GetTabFields(Pref, _nTab);
            _listTabFieldsImage = _listTabFields.Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE);

            if (_tabInfos.InterPM)
            {
                _listPMFields = GetTabFields(Pref, TableType.PM.GetHashCode());
                _listTabFieldsImage = _listTabFieldsImage.Concat(_listPMFields.Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE));
            }

            if (_tabInfos.InterPP)
            {
                _listPPFields = GetTabFields(Pref, TableType.PP.GetHashCode());
                _listTabFieldsImage = _listTabFieldsImage.Concat(_listPPFields.Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE));
            }

            if (_tabInfos.InterEVT)
            {
                _listParentTabFields = GetTabFields(Pref, _tabInfos.InterEVTDescid);
                _listTabFieldsImage = _listTabFieldsImage.Concat(_listParentTabFields.Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE));
            }

            _listTabFieldsImage = _listTabFieldsImage.OrderBy(f => _dicResTabs[f.Table.DescId]).ThenBy(f => f.Libelle);
        }

        /// <summary>
        /// Génération du bloc de gauche (mappings)
        /// </summary>
        private void CreateLeftSide()
        {
            int mappingId = 0;
            string sectionName = string.Empty;
            int titleSelectedValue = 0;
            int mapId = 0;
            int sepId = 0;
            eMiniFileParam mfParam, mfSep;
            bool generateSection = false;
            int parentTabDescid = 0;
            string parentTabName = String.Empty;
            IEnumerable<eFieldLiteMiniFileAdmin> sourceFields = new List<eFieldLiteMiniFileAdmin>();
            int nbMappings = 0;

            string titleBlockID = "edamfRightDivTitle";
            string fieldsBlockID = "edamfRightDivRubrique";
            string avatarBlockID = "edamfRightDivImage";
            string parentFieldsBlockID = "edamfRightDivPrtRubrique";

            if (_minifileType == MiniFileType.Kanban)
            {
                titleBlockID = "cardBlockTitle";
                fieldsBlockID = "cardBlockFields";
                avatarBlockID = "cardBlockAvatar";
                parentFieldsBlockID = "cardBlockParentFields";
            }

            List<eMiniFileParam> listFields =
                _minifileParams.FindAll(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.FIELD && p.Value / 100 * 100 == _nTab)
                .OrderBy(p => p.Order)
                .ToList();



            _panelLeftDiv = new Panel();
            _panelLeftDiv.ID = "edamfLeftDiv";


            CreateHeaderRubriques();

            //rubrique Titre
            CreateSection(titleBlockID);

            mappingId = _minifileParams.FirstOrDefault(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.FIELD_TITLE)?.MappingId ?? 0;
            CreateRubriques(eResApp.GetRes(Pref, 6167), "title", _listTabFields, FILEMAP_MINIFILE_TYPE.FIELD_TITLE, mappingId: mappingId);

            #region Rubriques de l'onglet en cours

            CreateSection(fieldsBlockID);


            int i = 1;

            while (i <= _nbMainMappings)
            {
                string label = String.Concat(eResApp.GetRes(Pref, 222), " ", (i));
                string name = String.Concat("rub", i);
                mappingId = 0;

                if (listFields.Count > 0)
                {
                    mappingId = listFields.First()?.MappingId ?? 0;
                    listFields.RemoveAt(0);
                }

                CreateRubriques(label, name, _listTabFields, FILEMAP_MINIFILE_TYPE.FIELD, mappingId: mappingId, order: i);

                i++;
            }


            #endregion


            #region Rubrique Image
            CreateSection(avatarBlockID);
            mappingId = _minifileParams.FirstOrDefault(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.IMAGE)?.MappingId ?? 0;
            CreateRubriques(eResApp.GetRes(Pref, 1216), "image", _listTabFieldsImage, FILEMAP_MINIFILE_TYPE.IMAGE, mappingId);
            #endregion


            #region Rubriques société, contacts, onglet parent
            foreach (string section in new List<string>() { "pm", "pp", "prt" })
            {
                generateSection = false;
                parentTabDescid = 0;
                parentTabName = String.Empty;
                sourceFields = new List<eFieldLiteMiniFileAdmin>();
                nbMappings = 0;

                switch (section)
                {
                    case "pm": //rubriques société parent
                        generateSection = _tabInfos.InterPM;
                        parentTabDescid = EudoQuery.TableType.PM.GetHashCode();
                        parentTabName = _pmTabName;
                        sourceFields = _listPMFields;
                        nbMappings = _nbPmMappings;
                        break;
                    case "pp": //rubriques contact parent
                        generateSection = _tabInfos.InterPP;
                        parentTabDescid = EudoQuery.TableType.PP.GetHashCode();
                        parentTabName = _ppTabName;
                        sourceFields = _listPPFields;
                        nbMappings = _nbPpMappings;
                        break;
                    case "prt": //rubriques onglet parent
                        generateSection = _tabInfos.InterEVT;
                        parentTabDescid = _tabInfos.InterEVTDescid;
                        parentTabName = _parentTabName;
                        sourceFields = _listParentTabFields;
                        nbMappings = _nbPrtMappings;
                        break;
                    default:
                        continue;
                }

                if (generateSection)
                {
                    //if (_minifileType == MiniFileType.Kanban)
                    //    parentFieldsBlockID = "cardBlockFields" + section.ToUpper();

                    CreateSection(parentFieldsBlockID);

                    sectionName = String.Concat(section, "rub");

                    bool mappingExist = false;
                    titleSelectedValue = parentTabDescid + 1;
                    mapId = 0;
                    sepId = 0;

                    // Recherche d'un mapping existant pour le titre
                    mfParam = _minifileParams.FirstOrDefault(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.FIELD_TITLE && p.Value / 100 * 100 == parentTabDescid);
                    if (mfParam != null)
                    {
                        mapId = mfParam.MappingId;

                        mfSep = _minifileParams.FirstOrDefault(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.SEPARATOR && p.Value / 100 * 100 == parentTabDescid);
                        if (mfSep != null)
                        {
                            sepId = mfSep.MappingId;
                            mappingExist = (mfParam.Value > 0) ? true : false;
                        }
                    }

                    CreateParentRubriquesHeader(eResApp.GetRes(Pref, 7023).Replace("<TAB>", parentTabName), sectionName, parentTabDescid, mappingSepId: sepId, chkbxChecked: mappingExist);
                    CreateRubriques(eResApp.GetRes(Pref, 6167), String.Concat(sectionName, "title"), sourceFields, FILEMAP_MINIFILE_TYPE.FIELD_TITLE,
                        mappingId: mapId, displayField: mappingExist, disableSelect: true, selectedValue: titleSelectedValue, parentTab: parentTabDescid);

                    // Liste des champs mappés
                    listFields =
                        _minifileParams.FindAll(p => p.DisplayType == FILEMAP_MINIFILE_TYPE.FIELD && p.Value / 100 * 100 == parentTabDescid)
                        .OrderBy(p => p.Order)
                        .ToList();

                    i = 1;
                    while (i <= nbMappings)
                    {
                        string label = String.Concat(eResApp.GetRes(Pref, 222), " ", (i));
                        string name = String.Concat(sectionName, i);
                        mappingId = 0;

                        if (listFields.Count > 0)
                        {
                            mappingId = listFields.First()?.MappingId ?? 0;
                            listFields.RemoveAt(0);
                        }

                        CreateRubriques(label, name, sourceFields, FILEMAP_MINIFILE_TYPE.FIELD, mappingId: mappingId, displayField: mappingExist, order: i, parentTab: parentTabDescid);

                        i++;
                    }
                }
            }
            #endregion

            _pgContainer.Controls.Add(_panelLeftDiv);
        }

        /// <summary>
        /// Génération de la section
        /// </summary>
        /// <param name="hoverDivId">The hover div identifier.</param>
        private void CreateSection(string hoverDivId = "")
        {
            _panelSectionDiv = new Panel();
            _panelSectionDiv.CssClass = "edamfSection";
            _panelSectionDiv.Attributes.Add("data-mftype", ((int)_minifileType).ToString());
            _panelLeftDiv.Controls.Add(_panelSectionDiv);

            if (!String.IsNullOrEmpty(hoverDivId))
            {
                _panelSectionDiv.Attributes.Add("onmouseover", String.Concat("nsAdminMiniFile.adminMinifileHoverField(this, '", hoverDivId, "', true);"));
                _panelSectionDiv.Attributes.Add("onmouseout", String.Concat("nsAdminMiniFile.adminMinifileHoverField(this, '", hoverDivId, "', false);"));
            }
        }

        /// <summary>
        /// Retourne un élément "span" correspondant à la colonne
        /// </summary>
        /// <param name="columm">The columm.</param>
        /// <returns></returns>
        private HtmlGenericControl GetSpan(COLUMN columm)
        {
            HtmlGenericControl span = new HtmlGenericControl("span");

            switch (columm)
            {
                case COLUMN.LABEL:
                    span.Attributes.Add("class", "edamflabel");
                    break;
                case COLUMN.LIST:
                    span.Attributes.Add("class", "edamflist");
                    break;
                case COLUMN.LIBELLE:
                    span.Attributes.Add("class", "edamflibelle");
                    break;
                default:
                    break;
            }

            return span;
        }

        /// <summary>
        /// Création de l'entête du mapping
        /// </summary>
        private void CreateHeaderRubriques()
        {
            Panel headerSection = new Panel();
            headerSection.CssClass = "headerSection";

            Panel field = new Panel();
            field.CssClass = "field";
            headerSection.Controls.Add(field);

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanList = GetSpan(COLUMN.LIST);
            HtmlGenericControl spanLibelle = GetSpan(COLUMN.LIBELLE);

            spanList.InnerText = eResApp.GetRes(Pref, 7022);
            spanLibelle.InnerText = eResApp.GetRes(Pref, 223);
            spanLibelle.Attributes.Add("title", eResApp.GetRes(Pref, 7275));

            //field.Controls.Add(spanLabel);
            field.Controls.Add(spanList);
            field.Controls.Add(spanLibelle);

            _panelLeftDiv.Controls.Add(headerSection);
        }

        /// <summary>
        /// Création des rubriques à mapper
        /// </summary>
        /// <param name="label">Libellé</param>
        /// <param name="name">Nom</param>
        /// <param name="datasource">Liste des champs disponibles</param>
        /// <param name="mappingType">Type de mapping : titre, champ, image...</param>
        /// <param name="mappingId">ID du mapping dans FILEMAP_PARTNER. 0 s'il n'existe pas</param>
        /// <param name="displayField">Affichage de la ligne</param>
        /// <param name="disableSelect">Liste déroulante en lecture seule</param>
        /// <param name="selectedValue">Valeur sélectionnée : prise en compte si &gt; 0</param>
        /// <param name="order">Ordre d'affichage</param>
        private void CreateRubriques(string label, string name, IEnumerable<eFieldLiteMiniFileAdmin> datasource, FILEMAP_MINIFILE_TYPE mappingType,
            int mappingId = 0, bool displayField = true, bool disableSelect = false, int selectedValue = 0, int order = 0, int parentTab = 0)
        {
            int selectedDescid = 0;
            bool displayLabel = false;
            eMiniFileParam param = null;

            // Dans les Kanban, on doit prendre l'ordre strict défini. Dans le cas standard, on prend les champs dans l'ordre
            if (_minifileType == MiniFileType.File && mappingId > 0)
                param = _minifileParams.FirstOrDefault(p => p.MappingId == mappingId);

            if (_minifileType == MiniFileType.Kanban)
                param = _minifileParams.FirstOrDefault(p => p.DisplayType == mappingType && p.Order == order && p.ParentTab == parentTab);

            if (param != null)
            {
                selectedDescid = param.Value;
                displayLabel = param.DisplayLabel;
                mappingId = param.MappingId;
            }

            if (selectedValue > 0)
                selectedDescid = selectedValue;

            Panel field = new Panel();
            field.ID = String.Concat("field", name);
            field.CssClass = "field";
            if (!displayField)
                field.Style.Add("display", "none");

            // Ajout des attributs du conteneur "field"
            field.Attributes.Add("edamftype", ((int)mappingType).ToString());
            field.Attributes.Add("edamforder", order.ToString());
            field.Attributes.Add("edamfparenttab", parentTab.ToString());

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanList = GetSpan(COLUMN.LIST);
            HtmlGenericControl spanLibelle = GetSpan(COLUMN.LIBELLE);
            field.Controls.Add(spanLabel);
            field.Controls.Add(spanList);
            field.Controls.Add(spanLibelle);

            #region Affichage de l'intitulé

            string selectName = String.Concat("ddl", name);

            HtmlGenericControl lbl = new HtmlGenericControl("label");
            lbl.Attributes.Add("id", String.Concat("lbl", name));
            lbl.Attributes.Add("for", selectName);
            lbl.InnerText = String.Concat(label, " :");
            spanLabel.Controls.Add(lbl);
            #endregion

            #region Affichage de la liste déroulante des rubriques disponibles

            HtmlGenericControl ddl = new HtmlGenericControl("select");
            ddl.Attributes.Add("id", selectName);
            ddl.Attributes.Add("name", selectName);
            ddl.Attributes.Add("edamfmpid", mappingId.ToString());
            ddl.Attributes.Add("edamfOldvalue", selectedDescid.ToString());
            ddl.Attributes.Add("onchange", "nsAdminMiniFile.adminMinifileToggleField(this);");
            if (disableSelect)
                ddl.Attributes.Add("disabled", "disabled");
            spanList.Controls.Add(ddl);

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "0");
            option.InnerText = eResApp.GetRes(Pref, 6211);
            if (selectedDescid == 0)
                option.Attributes.Add("selected", "selected");
            ddl.Controls.Add(option);

            foreach (eFieldLiteMiniFileAdmin fld in datasource)
            {
                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", fld.Descid.ToString());

                if (name == "image" && _dicResTabs.ContainsKey(fld.Table.DescId))
                    option.InnerText = String.Concat(_dicResTabs[fld.Table.DescId], ".", fld.Libelle);
                else
                    option.InnerText = fld.Libelle;


                if (selectedDescid == fld.Descid)
                    option.Attributes.Add("selected", "selected");

                if (selectedDescid != fld.Descid && _minifileParams.Exists(p => p.Value == fld.Descid))
                    option.Attributes.Add("disabled", "disabled");

                if (fld.Descid % 100 == 1)
                    option.Attributes.Add("data-mainfield", "1");

                ddl.Controls.Add(option);
            }
            #endregion

            #region Affichage du checkbox d'affichage du libellé
            if (name != "image")
            {
                eCheckBoxCtrl chkbx = new eCheckBoxCtrl(displayLabel, false);
                chkbx.ID = String.Concat("chkbxLib", name);
                chkbx.AddClick();
                spanLibelle.Controls.Add(chkbx);
            }
            #endregion

            _panelSectionDiv.Controls.Add(field);
        }

        /// <summary>
        /// Création de l'entête pour une table de liaison
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="name">The name.</param>
        /// <param name="mappingSepId">The mapping sep identifier.</param>
        /// <param name="chkbxChecked">if set to <c>true</c> [CHKBX checked].</param>
        private void CreateParentRubriquesHeader(string label, string name, int parentDescid, int mappingSepId = 0, bool chkbxChecked = false)
        {
            Panel field = new Panel();
            field.CssClass = "prtField field";
            field.Attributes.Add("edamftype", ((int)FILEMAP_MINIFILE_TYPE.SEPARATOR).ToString());
            field.Attributes.Add("edamforder", "0");
            field.Attributes.Add("edamfvalue", parentDescid.ToString());

            HtmlGenericControl spanLabel = GetSpan(COLUMN.LABEL);
            HtmlGenericControl spanLibelle = GetSpan(COLUMN.LIBELLE);
            field.Controls.Add(spanLabel);
            field.Controls.Add(spanLibelle);

            eCheckBoxCtrl chkbxRub = new eCheckBoxCtrl(chkbxChecked, false);
            chkbxRub.ID = String.Concat("chkbx", name);
            chkbxRub.AddText(label);
            chkbxRub.Attributes.Add("edamfmpspid", mappingSepId.ToString());
            chkbxRub.AddClick(String.Concat("nsAdminMiniFile.adminMinifileToggleSection(this, '", name, "');"));
            spanLabel.Controls.Add(chkbxRub);

            _panelSectionDiv.Controls.Add(field);
        }

        /// <summary>
        /// Création de la partie droite correspondant à l'aperçu
        /// </summary>
        private void CreateRightSide()
        {
            _panelRightDiv = new Panel();
            _panelRightDiv.ID = "edamfRightDiv";
            _pgContainer.Controls.Add(_panelRightDiv);

            Panel divCentered = new Panel();
            divCentered.ID = "edamfImgContainer";
            _panelRightDiv.Controls.Add(divCentered);

            if (_minifileType == MiniFileType.Kanban)
            {
                //eKanbanCardRenderer rdr = eKanbanCardRenderer.CreateKanbanCardRenderer(this.Pref, _widgetId, _nTab);
                //rdr.Generate();
                //if (!String.IsNullOrEmpty(rdr.ErrorMsg))
                //{
                //    _sErrorMsg = rdr.ErrorMsg;
                //    _eException = rdr.InnerException;
                //}


                //divCentered.Controls.Add(rdr.PgContainer);
                HtmlGenericControl panel = new HtmlGenericControl("div");
                panel.Attributes.Add("class", "kanbanCard");
                divCentered.Controls.Add(panel);

                //Panel block = new Panel();
                //block.CssClass = "cardBlock";
                //block.ID = "cardBlockAvatar";
                //Panel icon = new Panel();
                //icon.CssClass = "cardAvatar icon-photo";
                //block.Controls.Add(icon);
                //panel.Controls.Add(block);

                //Panel content = new Panel();
                //content.ID = "cardContentWrapper";
                //panel.Controls.Add(content);

                //block = new Panel();
                //block.ID = "cardBlockTitle";
                //block.CssClass = "cardBlock";

                panel.InnerHtml = @"
                <div id = 'cardBlockAvatar' class='cardBlock'>
					<div class='cardAvatar icon-photo'>
						
					</div>
				</div><div id = 'cardContentWrapper' >

                    <div id='cardBlockTitle' class='cardBlock'>
						<div class='cardTitle cardField'>
							<span>Affaire :</span>Gala
                        </div>
					</div><div id = 'cardBlockFields' class='cardBlock'>
						<div class='cardField'>
							<span>But de l'affaire :</span>Appel aux dons
						</div><div class='cardField'>
							<span>Date :</span>2016/09/07
						</div><div class='cardField'>
							<span>Date fin :</span>2016/09/10
						</div><div class='cardField'>
							<span>Montant minimum à atteindre :</span>1 000
						</div><div class='cardField'>
							<span>Réussite de l'objectif :</span>NON
						</div>
					</div><div id = 'cardBlockParentFields' class='cardBlock'>
						<div>
							<div id = 'cardBlockFieldsPM' class='cardBlock'>
								<div class='cardTitle cardField'>
									EUDOWEB
                                </div>
							</div><div id = 'cardBlockFieldsPM' class='cardBlock'>
								<div class='cardField'>
									<span>Tél :</span>01 02 03 04 05
								</div><div class='cardField'>
									<span>Email :</span>dev@eudonet.com
                                </div>
							</div>
						</div>
					</div>
				</div>";

            }
            else
            {
                HtmlGenericControl image = new HtmlGenericControl("img");
                image.Attributes.Add("src", "themes/default/images/eda/eAdmin-miniFile-Preview.png");
                image.Attributes.Add("alt", "eAdmin-miniFile-Preview.png");
                divCentered.Controls.Add(image);

                foreach (string name in new List<string>() { "Image", "Title", "Rubrique", "PrtRubrique" })
                {
                    Panel div = new Panel();
                    div.ID = String.Concat("edamfRightDiv", name);
                    div.CssClass = "edamfRightDivs";
                    divCentered.Controls.Add(div);
                }
            }

        }
        #endregion

    }
}