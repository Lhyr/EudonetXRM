using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// eAdminKanbanSettingsRenderer : rendu des paramètres "lignes de couloir et agrégats" Kanban
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eAbstractRenderer" />
    public class eAdminKanbanSettingsRenderer : eAbstractRenderer
    {
        eudoDAL _dal = null;
        private eAdminKanbanSettings _settings;
        private int _iMainTab = 0;
        private List<ParentTab> _listParentTab = new List<ParentTab>();
        private eXrmWidgetParam _widgetParam;
        private eXrmWidgetContext _widgetContext;

        /// <summary>Valeurs de catalogues sur lesquelles s'appuient les colonnes </summary>
        private Dictionary<int, string> _diCatValues = new Dictionary<int, string>();
        /// <summary>Liste des rubriques numériques surlesquelles il est possible de réaliser des opérations (somme, moyenne, etc.) </summary>
        private ExtendedDictionary<int, string> _diOperationFields = new ExtendedDictionary<int, string>();
        /// <summary>
        /// The resource code manager
        /// </summary>
        private eResCodeTranslationManager _resCodeMgr = null;

        private class ParentTab
        {
            public int RelationFileDescId = 0;
            public string RelationFile = "";
            public int LinkFieldDescId = 0;
            public string LinkField = "";

        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="sets">The setting.</param>
        /// <param name="param">eXrmWidgetParam</param>
        /// <param name="context">eXrmWidgetContext</param>
        /// <exception cref="NullReferenceException"></exception>
        public eAdminKanbanSettingsRenderer(ePref pref, eAdminKanbanSettings sets, eXrmWidgetParam param, eXrmWidgetContext context)
        {
            if (sets == null)
                throw new NullReferenceException();
            Error = "";
            Pref = pref;
            _settings = sets;
            _iMainTab = param.GetParamValueInt("tab");
            _widgetParam = param;
            _widgetContext = context;
        }

        /// <summary>
        /// charge le renderer.
        /// </summary>
        public override Boolean Generate()
        {
            try
            {
                _dal = eLibTools.GetEudoDAL(this.Pref);

                _dal.OpenDatabase();

                GetRendererInfos();
                PgContainer.CssClass = "PgContainer";

                RenderHiddenInfos();

                PgContainer.Controls.Add(RenderSortOptions());

                PgContainer.Controls.Add(RenderSwimlanes());

                PgContainer.Controls.Add(RenderAggregates());



            }
            catch (Exception e)
            {
                Error = String.Format("eAdminKanbanSettingsRenderer : Une Erreur est survenue lors de la Génération du rendu : {0}{1}{2} ", e.Message, Environment.NewLine, e.StackTrace);
                return false;
            }
            finally
            {
                _dal.CloseDatabase();
            }

            return true;
        }

        /// <summary>
        /// Rendu du bloc pour les options de tri
        /// </summary>
        /// <returns></returns>
        private Control RenderSortOptions()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.ID = "SortBlock";
            div.Attributes.Add("class", "Level1");

            HtmlGenericControl p = new HtmlGenericControl("P");
            div.Controls.Add(p);
            p.Attributes.Add("class", "BlockTitle");
            p.InnerHtml = eResApp.GetRes(_ePref, 1919);

            int fieldDescid = _settings.Sort?.Field ?? 0;
            int sortOrder = _settings.Sort?.Order ?? 0;

            #region Liste des rubriques
            IEnumerable<eFieldLiteWithLib> fieldsList = RetrieveFields.GetDefault(this.Pref).SetExternalDal(_dal)
                .AddOnlyThisTabs(new List<int>() { _iMainTab })
                .AddExcludeFormats(new List<FieldFormat> { FieldFormat.TYP_IMAGE, FieldFormat.TYP_FILE })
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(this.Pref))
                .OrderBy(f => f.Libelle);
            ListItem item;

            Panel pField = new Panel();
            pField.CssClass = "fieldBlock";

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 222);
            pField.Controls.Add(label);

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlSortField";

            ddl.Items.Add(new ListItem(eResApp.GetRes(_ePref, 8166), "0"));
            foreach (var f in fieldsList)
            {
                item = new ListItem(f.Libelle, f.Descid.ToString());
                if (f.Descid % 100 == 1)
                {
                    item.Attributes.Add("data-mainfield", "1");
                }
                ddl.Items.Add(item);
            }
            ddl.SelectedValue = fieldDescid.ToString();

            pField.Controls.Add(ddl);
            div.Controls.Add(pField);
            #endregion

            #region Ordre de tri 
            pField = new Panel();
            pField.CssClass = "fieldBlock";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 1920);
            pField.Controls.Add(label);

            ddl = new DropDownList();
            ddl.ID = "ddlSortOrder";
            ddl.Items.Add(new ListItem(eResApp.GetRes(_ePref, 158), "0"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(_ePref, 159), "1"));
            ddl.SelectedValue = sortOrder.ToString();
            pField.Controls.Add(ddl);
            div.Controls.Add(pField);
            #endregion

            return div;
        }

        /// <summary>
        /// Renders the hidden infos.
        /// </summary>
        private void RenderHiddenInfos()
        {
            HtmlInputHidden input = new HtmlInputHidden();
            input.ID = "CatValues";
            input.Value = JsonConvert.SerializeObject(_diCatValues);
            PgContainer.Controls.Add(input);

            input = new HtmlInputHidden();
            input.ID = "OpFields";
            input.Value = JsonConvert.SerializeObject(_diOperationFields);
            PgContainer.Controls.Add(input);


            input = new HtmlInputHidden();
            input.ID = "wid";
            input.Value = _widgetParam.WidgetId.ToString();
            PgContainer.Controls.Add(input);

            input = new HtmlInputHidden();
            input.ID = "widgetContext";
            input.Value = JsonConvert.SerializeObject(_widgetContext);
            PgContainer.Controls.Add(input);
        }

        /// <summary>
        /// on récupère toutes les infos nécessaires en BDD
        /// </summary>
        private void GetRendererInfos()
        {
            string sError = "";
            try
            {
                //_tab.ExternalLoadInfo(dal, out sError);
                RqParam rq = eSqlDesc.GetRqLiaison(_iMainTab, Pref.User.UserLangId);
                DataTableReaderTuned dtr = _dal.Execute(rq, out sError);
                while (dtr.Read())
                {
                    _listParentTab.Add(new ParentTab()
                    {
                        RelationFile = dtr.GetString("RelationFile"),
                        RelationFileDescId = dtr.GetInt32("RelationFileDescId"),
                        LinkFieldDescId = dtr.GetInt32("LinkFieldDescId"),
                        LinkField = dtr.GetString("LinkField")
                    });
                }

                _listParentTab.Add(new ParentTab()
                {
                    RelationFileDescId = _iMainTab
                });

                #region Valeurs en ligne de couloir
                eCatalog slCatalog;
                List<string> listSLValues, listSLLabels;
                StringBuilder sbError = new StringBuilder();
                List<eCatalog.CatalogValue> valuesList;
                foreach (eAdminKanbanSettings.Swimlane sl in _settings.Swimlanes)
                {
                    if (sl.DescId > 0 && !String.IsNullOrEmpty(sl.SelectedValues))
                    {
                        listSLValues = sl.SelectedValues.Split(';').ToList<string>();
                        listSLLabels = new List<string>();

                        if (sl.FieldFormat == (int)FieldFormat.TYP_CHAR)
                        {
                            valuesList = new List<eCatalog.CatalogValue>();
                            sl.SelectedValues.Split(';').ToList<string>().ForEach(
                                s => valuesList.Add(new eCatalog.CatalogValue() { Id = Int32.Parse(s) }));

                            slCatalog = new eCatalog(_dal, Pref, PopupType.DATA, Pref.User, sl.DescId, false, valuesList);
                            slCatalog.Values.ForEach(cv => listSLLabels.Add(cv.DisplayValue));

                            sl.SelectedLabels = String.Join(";", listSLLabels.OrderBy(l => l));
                        }
                        else if (sl.FieldFormat == (int)FieldFormat.TYP_USER)
                        {

                            sl.SelectedLabels = eSqlUser.GetUserDisplay(_dal, sl.SelectedValues);
                        }

                    }

                }
                #endregion

                #region colonnes disponibles 
                eKanban.AdvCatalogField field = JsonConvert.DeserializeObject<eKanban.AdvCatalogField>(_widgetParam.GetParamValue("catalog"));

                int iDescId = field.PopupDescID;
                string[] sColumnsId = _widgetParam.GetParamValue("catvalues").Split(';');
                List<eCatalog.CatalogValue> liVal = new List<eCatalog.CatalogValue>();
                sColumnsId.ToList<string>().ForEach(s => liVal.Add(new eCatalog.CatalogValue() { Id = Int32.Parse(s) }));
                eCatalog catalog = new eCatalog(_dal, Pref, PopupType.DATA, Pref.User, iDescId, false, liVal);
                catalog.Values.ForEach(cv => _diCatValues.Add(cv.Id, cv.DisplayValue));

                #endregion

                #region liste des champs sur lesquels on peut procéder à des opérations
                bool bInterPP = _listParentTab.Exists(pt => pt.RelationFileDescId == (int)TableType.PP);
                bool bInterPM = _listParentTab.Exists(pt => pt.RelationFileDescId == (int)TableType.PM);
                bool bAdrJoin = _listParentTab.Exists(pt => pt.RelationFileDescId == (int)TableType.ADR);
                int iInterEvtDescId = _listParentTab.Find(pt => pt.RelationFileDescId == 100 || pt.RelationFileDescId >= 1000)?.RelationFileDescId ?? 0;

                //TODO : Lorsqu'on prendra en charge les liaisons custom il faudra changer de méthode.
                _diOperationFields = eSqlDesc.LoadLinkedTabsFields(Pref,
                                                                    tab: _iMainTab,
                                                                    bIncludeCurrentTable: true,
                                                                    interEVTDescid: iInterEvtDescId,
                                                                    bInterPP: bInterPP,
                                                                    bInterPM: bInterPM,
                                                                    bAdrJoin: bAdrJoin,
                                                                    fieldFormats: new List<int>() { (int)FieldFormat.TYP_NUMERIC, (int)FieldFormat.TYP_MONEY },
                                                                    edal: _dal);
                #endregion

                #region Traductions éventuelles
                _resCodeMgr = new eResCodeTranslationManager(_ePref, _dal, new eResLocation(eModelConst.ResCodeNature.WidgetParam, eResLocation.GetPathFromWidgetContext(_widgetContext)));
                #endregion
            }
            catch (Exception)
            {
                throw;
            }


        }



        /// <summary>
        /// Représente le premier paragraphe de l'écran de paramétrage du KanBan : Les lignes de couloirs.
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl RenderSwimlanes()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.ID = "swimlanes";
            div.Attributes.Add("class", "Level1");

            HtmlGenericControl p = new HtmlGenericControl("P");
            div.Controls.Add(p);
            p.Attributes.Add("class", "BlockTitle");
            //8472 - Rubriques pouvant être utilisées comme lignes de couloir
            p.InnerHtml = eResApp.GetRes(Pref, 8472);

            IEnumerable<int> tabs = _listParentTab.Select(t => t.RelationFileDescId);

            DataTableReaderTuned dtrFields = eSqlDesc.LoadKanbanSwimlaneFields(_dal, this.Pref.User.UserLang, tabs);

            eAdminKanbanSettings.Swimlane slTmp;
            Dictionary<string, string> diFields = new Dictionary<string, string>();
            while (dtrFields.Read())
            {
                slTmp = new eAdminKanbanSettings.Swimlane()
                {
                    DescId = dtrFields.GetEudoNumeric("DescId"),
                    Tab = dtrFields.GetEudoNumeric("Tab"),
                    IsGroup = false,
                    FieldFormat = dtrFields.GetInt32("Format"),
                    LinkField = 0 //TODO
                };
                diFields.Add(JsonConvert.SerializeObject(slTmp), dtrFields.GetString("LABEL"));

                // Si c'est un champ de type utilisateur, on permet la répartition dans les couloirs par Groupe.
                //if ((FieldFormat)dtrFields.GetInt32("Format") == FieldFormat.TYP_USER)
                //{
                //    slTmp = new eAdminKanbanSettings.Swimlane()
                //    {
                //        DescId = dtrFields.GetEudoNumeric("DescId"),
                //        Tab = dtrFields.GetEudoNumeric("Tab"),
                //        IsGroup = true,
                //        LinkField = 0 //TODO                       
                //    };
                //    diFields.Add(JsonConvert.SerializeObject(slTmp), String.Format("{0} ({1})", dtrFields.GetString("LABEL"), eResApp.GetRes(Pref, 7575)));

                //}

            }


            for (int index = 0; index < 3; index++)
            {
                if (index < _settings.Swimlanes.Count)
                    slTmp = _settings.Swimlanes[index];
                else
                    slTmp = null;

                div.Controls.Add(RenderSwimlane(slTmp, index, diFields));
            }

            return div;
        }



        /// <summary>
        /// Représente une ligne de couloir à paramétrer
        /// </summary>
        /// <param name="swimlane">Ligne de Couloir (Peut être null)</param>
        /// <param name="index"></param>
        /// <param name="diFields">Liste des champs à afficher dans la ddl</param>
        /// <returns></returns>
        private HtmlGenericControl RenderSwimlane(eAdminKanbanSettings.Swimlane swimlane, int index, Dictionary<string, string> diFields)
        {

            HtmlGenericControl blockSL = new HtmlGenericControl("div");
            blockSL.ID = String.Format("swimlane{0}", index);
            blockSL.Attributes.Add("class", "swimlaneBlock");

            HtmlGenericControl labelSL = new HtmlGenericControl();
            labelSL.Attributes.Add("class", "labelSL");
            blockSL.Controls.Add(labelSL);
            labelSL.InnerHtml = String.Format("{0} {1}", eResApp.GetRes(Pref, 8471), index + 1); //Ligne de couloir

            #region Sélection de la ligne de couloir + option
            HtmlGenericControl blockSLField = new HtmlGenericControl("div");
            blockSL.Controls.Add(blockSLField);

            DropDownList ddl = new DropDownList();
            ddl.ID = String.Format("swimlaneDescid{0}", index);
            ddl.Attributes.Add("onchange", $"nsAdminKanban.onSLFieldChange(this, {index})");
            blockSLField.Controls.Add(ddl);

            ddl.DataSource = diFields;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();

            ddl.Items.Insert(0, new ListItem("", ""));
            //ListItem it;
            //foreach (KeyValuePair<eAdminKanbanSettings.Swimlane, string> f in diFields)
            //{
            //    it = new ListItem(f.Value, JsonConvert.SerializeObject(f.Key));
            //    it.Attributes.Add("data-format", ((eAdminKanbanSettings.Swimlane)f.Key).FieldFormat.GetHashCode().ToString());
            //    ddl.Items.Add(it);
            //}

            eCheckBoxCtrl cbEmpty = new eCheckBoxCtrl(swimlane?.DisplayEmptyLane ?? false, false);

            //Afficher les lignes de couloir sans carte.
            cbEmpty.AddText(eResApp.GetRes(Pref, 8477));
            cbEmpty.AddClick();

            blockSLField.Controls.Add(cbEmpty);

            if (swimlane != null)
            {
                foreach (ListItem item in ddl.Items)
                {
                    if (item.Value.Length == 0)
                        continue;
                    eAdminKanbanSettings.Swimlane slItem = null;
                    try
                    {
                        slItem = JsonConvert.DeserializeObject<eAdminKanbanSettings.Swimlane>(item.Value);
                    }
                    catch (JsonSerializationException)
                    {
                        continue;
                    }

                    if (slItem?.Equals(swimlane) ?? false)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }
            #endregion

            #region Sélection des valeurs de la ligne de couloir
            HtmlGenericControl blockSLValues = new HtmlGenericControl("div");
            blockSLValues.Attributes.Add("class", "fieldBlock");
            blockSL.Controls.Add(blockSLValues);

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 8669);
            blockSLValues.Controls.Add(label);

            TextBox input = new TextBox();
            input.ID = String.Concat("slValues", index);
            input.ReadOnly = true;
            input.Attributes.Add("data-slindex", index.ToString());
            input.Attributes.Add("onclick", "nsAdminKanban.selectSLValues(this)");
            input.CssClass = "txtSLValues";
            if (swimlane != null)
            {
                string values = swimlane.SelectedValues ?? string.Empty;
                if (!String.IsNullOrEmpty(values))
                {
                    input.Attributes.Add("dbv", values);
                    input.Text = swimlane.SelectedLabels;
                }
            }


            blockSLValues.Controls.Add(input);

            HtmlGenericControl btn = new HtmlGenericControl();
            btn.Attributes.Add("class", "icon-catalog");
            btn.Attributes.Add("data-for", input.ID);
            btn.Attributes.Add("onclick", "nsAdminKanban.selectSLBtnOnClick(this)");
            blockSLValues.Controls.Add(btn);
            #endregion

            return blockSL;

        }

        /// <summary>
        /// Gère le rendu des blocs d'agrégats par colonne
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl RenderAggregates()
        {
            HtmlGenericControl divAggregatesBlock = new HtmlGenericControl("div");
            divAggregatesBlock.ID = "AggregatesBlock";
            divAggregatesBlock.Attributes.Add("class", "Level1");


            HtmlGenericControl p = new HtmlGenericControl("P");
            divAggregatesBlock.Controls.Add(p);
            p.Attributes.Add("class", "BlockTitle");
            //8473 - Agrégats à afficher dans les entêtes de colonne
            p.InnerHtml = eResApp.GetRes(Pref, 8473);

            HtmlGenericControl divColsAggregates = new HtmlGenericControl("div");
            divColsAggregates.Attributes.Add("class", "ColsAggregates");
            divColsAggregates.ID = "ColsAggregates";
            divAggregatesBlock.Controls.Add(divColsAggregates);

            //if (_settings.Aggregates.Count == 0)
            //    divColsAggregates.Controls.Add(AggregateRenderer.GetAggregateRenderer(Pref, _diCatValues, _diOperationFields).PgContainer);
            //else
            foreach (eAdminKanbanSettings.Aggregate aggregate in _settings.Aggregates)
            {
                divColsAggregates.Controls.Add(AggregateRenderer.GetAggregateRenderer(Pref, _diCatValues, _diOperationFields, aggregate, _resCodeMgr).PgContainer);
            }

            p = new HtmlGenericControl("P");
            divAggregatesBlock.Controls.Add(p);
            p.ID = "AddAgg";
            p.Attributes.Add("class", "BlockTitle AddAgg");
            p.Attributes.Add("onclick", "nsAdminKanban.AddAggregate();");
            //HtmlGenericControl span = new HtmlGenericControl("span");
            //span.Attributes.Add("class", "icon-add active");
            //p.Controls.Add(span);
            //8488 - Ajouter un agrégat.
            LiteralControl l = new LiteralControl(eResApp.GetRes(Pref, 8488));
            p.Controls.Add(l);
            return divAggregatesBlock;
        }



        /// <summary>
        /// crée un rendu sans données des agrégats pour une colonne
        /// </summary>
        internal class AggregateRenderer : eAbstractRenderer
        {

            private DropDownList _ddlColumn = new DropDownList();
            private List<SettingRenderer> _settings = new List<SettingRenderer>(nbAggregates);
            private Dictionary<int, string> _diCatValues = new Dictionary<int, string>();
            private Dictionary<int, string> _diOperationFields = new Dictionary<int, string>();
            private eResCodeTranslationManager _resCodeMgr = null;

            private const int nbAggregates = 2; //2 agrégats par colonne
            /// <summary>
            /// constructive
            /// </summary>
            /// <param name="pref">ePref</param>
            /// <param name="diCatValues">Dictionnaire des valeurs de catalogue sur lesquelles s'appuient les colonnes</param>
            /// <param name="diOperationFields">Dictionnaire des champs</param>
            /// <param name="resCodeMgr">Gestion des ressources</param>
            private AggregateRenderer(ePref pref, Dictionary<int, string> diCatValues, Dictionary<int, string> diOperationFields, eResCodeTranslationManager resCodeMgr)
            {
                Pref = pref;
                _diCatValues = diCatValues;
                _diOperationFields = diOperationFields;
                _resCodeMgr = resCodeMgr;
                Error = "";
            }


            /// <summary>
            /// génération
            /// </summary>
            /// <returns></returns>
            public override bool Generate()
            {
                PgContainer.Attributes.Add("class", "ColAggregate");

                HtmlGenericControl icondel = new HtmlGenericControl("span");
                PgContainer.Controls.Add(icondel);
                icondel.Attributes.Add("class", "icon-delete");
                icondel.Attributes.Add("onclick", "nsAdminKanban.DeleteAggregate(this);");
                HtmlGenericControl lit = new HtmlGenericControl("span");
                lit.InnerText = eResApp.GetRes(Pref, 8486); // Pour la colonne : 
                PgContainer.Controls.Add(lit);
                PgContainer.Controls.Add(_ddlColumn);
                _ddlColumn.Attributes.Add("role", "colvalue");
                _ddlColumn.Attributes.Add("onchange", "nsAdminKanban.PreventColDuplicate();");
                _ddlColumn.DataSource = _diCatValues;
                _ddlColumn.DataTextField = "Value";
                _ddlColumn.DataValueField = "Key";
                _ddlColumn.DataBind();

                _ddlColumn.Items.Insert(0, new ListItem());

                for (int i = 0; i < nbAggregates; i++)
                {
                    SettingRenderer set = SettingRenderer.GetSettingRenderer(Pref, _diOperationFields, _resCodeMgr);
                    _settings.Add(set);
                    PgContainer.Controls.Add(set.PgContainer);
                }



                return true;
            }

            /// <summary>
            /// complète le renderer avec le paramétrage pré-renseigné
            /// </summary>
            /// <param name="Agg">The aggregate.</param>
            public void SetAggregate(eAdminKanbanSettings.Aggregate Agg)
            {
                _ddlColumn.Select(Agg.Value);
                for (int i = 0; i < nbAggregates; i++)
                {
                    if (Agg.Settings.Count <= i)
                        break;

                    _settings[i].SetSetting(Agg.Settings[i]);
                }
            }



            /// <summary>
            /// retourne un renderer pour les aggrégats d'une colonne
            /// </summary>
            /// <param name="pref">ePref</param>
            /// <param name="diCatValue">dictionnaire contenant les dataid (key) et valeurs affichées (value)</param>
            /// <param name="diOperationFields">Dictionnaire des champs</param>
            /// <param name="resCodeMgr">The resource code MGR.</param>
            /// <returns></returns>
            public static AggregateRenderer GetAggregateRenderer(ePref pref, Dictionary<int, string> diCatValue, Dictionary<int, string> diOperationFields, eResCodeTranslationManager resCodeMgr)
            {
                AggregateRenderer aggRdr = new AggregateRenderer(pref, diCatValue, diOperationFields, resCodeMgr);
                aggRdr.Generate();
                return aggRdr;
            }

            /// <summary>
            /// retourne un renderer pour les aggrégats d'une colonne
            /// </summary>
            /// <param name="pref">ePref</param>
            /// <param name="diCatValue">dictionnaire contenant les dataid (key) et valeurs affichées (value)</param>
            /// <param name="diOperationFields">Dictionnaire des champs</param>
            /// <param name="agg">Paramétrage des agrégat pour la colonne</param>
            /// <param name="resCodeMgr">The resource code MGR.</param>
            /// <returns></returns>
            public static AggregateRenderer GetAggregateRenderer(ePref pref, Dictionary<int, string> diCatValue, Dictionary<int, string> diOperationFields, eAdminKanbanSettings.Aggregate agg, eResCodeTranslationManager resCodeMgr)
            {
                AggregateRenderer aggRdr = AggregateRenderer.GetAggregateRenderer(pref, diCatValue, diOperationFields, resCodeMgr);
                if (agg != null)
                    aggRdr.SetAggregate(agg);
                return aggRdr;
            }

            /// <summary>
            /// crée un rendu sans données des agrégats pour une colonne
            /// </summary>
            internal class SettingRenderer : eAbstractRenderer
            {
                private DropDownList _ddlOperation = new DropDownList();
                HtmlGenericControl _liOperationFields = new HtmlGenericControl("li");
                private DropDownList _ddlOperationFields = new DropDownList();
                private HtmlInputText _inptUnit = new HtmlInputText();
                private DropDownList _ddlUnitPosition = new DropDownList();
                private HtmlInputText _inptLabel = new HtmlInputText();
                private Dictionary<int, string> _diOperationFields;
                private eResCodeTranslationManager _resCodeMgr = new eResCodeTranslationManager(null, null);


                /// <summary>
                /// constructive
                /// </summary>
                /// <param name="pref">The preference.</param>
                /// <param name="resCodeMgr">The resource code MGR.</param>
                private SettingRenderer(ePref pref, eResCodeTranslationManager resCodeMgr)
                {
                    Pref = pref;
                    _resCodeMgr = resCodeMgr;
                    Error = "";
                }

                public override bool Generate()
                {
                    string jsonResloc = string.Empty;
                    eResLocation resLoc = _resCodeMgr.ResLoc;

                    PgContainer.Attributes.Add("class", "AggSetting");

                    _ddlOperation.Attributes.Add("role", "op");
                    _ddlOperation.Attributes.Add("onclick", "nsAdminKanban.onChangeOperation(this);");
                    _ddlOperationFields.Attributes.Add("role", "opfield");
                    _ddlUnitPosition.Attributes.Add("role", "unitposition");

                    _inptUnit.Attributes.Add("role", "unit");
                    _inptUnit.Attributes.Add("data-hasrcode", "1");
                    resLoc.Identifier = "kanban-aggr-unit";
                    _inptUnit.Attributes.Add("data-rloc", JsonConvert.SerializeObject(resLoc));

                    _inptLabel.Attributes.Add("role", "label");
                    _inptLabel.Attributes.Add("data-hasrcode", "1");
                    resLoc.Identifier = "kanban-aggr-label";
                    _inptLabel.Attributes.Add("data-rloc", JsonConvert.SerializeObject(resLoc));

                    #region 1ere ligne

                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    this.PgContainer.Controls.Add(ul);
                    ul.Attributes.Add("class", "setline1");
                    HtmlGenericControl li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);
                    HtmlGenericControl span = new HtmlGenericControl("span");
                    li.Controls.Add(span);
                    span.InnerText = eResApp.GetRes(Pref, 8487); // afficher en entete
                    li.Controls.Add(_ddlOperation);
                    foreach (eAdminKanbanSettings.EnumOperation op in Enum.GetValues(typeof(eAdminKanbanSettings.EnumOperation)))
                    {
                        _ddlOperation.Items.Add(new ListItem(eAdminKanbanSettings.GetOperationLabel(op, Pref), ((int)op).ToString()));
                    }

                    _liOperationFields = new HtmlGenericControl("li");
                    ul.Controls.Add(_liOperationFields);
                    span = new HtmlGenericControl("span");
                    _liOperationFields.Controls.Add(span);
                    span.InnerText = eResApp.GetRes(Pref, 5085); // De
                    _liOperationFields.Controls.Add(_ddlOperationFields);
                    _ddlOperationFields.DataSource = _diOperationFields;
                    _ddlOperationFields.DataTextField = "Value";
                    _ddlOperationFields.DataValueField = "Key";
                    _ddlOperationFields.DataBind();

                    _liOperationFields.Attributes.Add("role", "opfield");
                    _liOperationFields.Style.Add(HtmlTextWriterStyle.Display, "none");
                    #endregion

                    #region 2eme ligne

                    ul = new HtmlGenericControl("ul");
                    this.PgContainer.Controls.Add(ul);
                    ul.Attributes.Add("class", "setline2");
                    li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);
                    span = new HtmlGenericControl("span");
                    li.Controls.Add(span);
                    span.InnerText = eResApp.GetRes(Pref, 1353); // Unité
                    li.Controls.Add(_inptUnit);

                    li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);
                    span = new HtmlGenericControl("span");
                    li.Controls.Add(span);
                    span.InnerText = eResApp.GetRes(Pref, 8407); // Position de l'unité
                    li.Controls.Add(_ddlUnitPosition);
                    foreach (eAdminKanbanSettings.EnumUnitPosition position in Enum.GetValues(typeof(eAdminKanbanSettings.EnumUnitPosition)))
                    {
                        _ddlUnitPosition.Items.Add(new ListItem(eAdminKanbanSettings.GetPositionLabel(position, Pref), ((int)position).ToString()));
                    }

                    li = new HtmlGenericControl("li");
                    ul.Controls.Add(li);
                    span = new HtmlGenericControl("span");
                    li.Controls.Add(span);
                    span.InnerText = eResApp.GetRes(Pref, 223); // Libellé
                    li.Controls.Add(_inptLabel);

                    #endregion

                    return true;

                }

                /// <summary>
                /// complète le renderer avec le paramétrage pré-renseigné
                /// </summary>
                /// <param name="set"></param>
                public void SetSetting(eAdminKanbanSettings.Aggregate.Setting set)
                {
                    _ddlOperation.Select(set.Operation);
                    if (set.Operation > (int)eAdminKanbanSettings.EnumOperation.FILES_NUMBER)
                    {
                        _ddlOperationFields.Select(set.OperationField);
                        _liOperationFields.Style.Remove(HtmlTextWriterStyle.Display);
                    }

                    int unitResCode = 0, labelResCode = 0;
                    string unit = set.Unit, label = set.Label;

                    if (_resCodeMgr != null)
                    {
                        unit = _resCodeMgr.Translate(unit, out unitResCode);
                        label = _resCodeMgr.Translate(label, out labelResCode);

                    }
                    _inptUnit.Attributes.Add("data-rcode", unitResCode.ToString());
                    _inptLabel.Attributes.Add("data-rcode", labelResCode.ToString());

                    _inptUnit.Value = unit;
                    _ddlUnitPosition.Select(set.UnitPosition);
                    _inptLabel.Value = label;
                }

                public static SettingRenderer GetSettingRenderer(ePref pref, Dictionary<int, string> diOperationFields, eResCodeTranslationManager resCodeMgr)
                {
                    SettingRenderer setting = new SettingRenderer(pref, resCodeMgr);
                    setting._diOperationFields = diOperationFields;
                    setting.Generate();

                    return setting;

                }

            }

        }
    }
}