using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.eda;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu de la liste des rubriques d'un onglet
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRenderer" />
    public class eAdminFieldsListRenderer : eAdminRenderer
    {
        /// <summary>
        /// Liste des rubriques par table
        /// </summary>
        protected List<eAdminFieldInfos> _fields = new List<eAdminFieldInfos>();
        /// <summary>
        /// Liste des colonnes à afficher
        /// </summary>
        protected List<ListCol> listColumns = new List<ListCol>();
        protected Dictionary<int, List<eAdminTriggerField>> _dicTriggers = new Dictionary<int, List<eAdminTriggerField>>();
        protected eResInternal _res;

        /// <summary>
        /// Instancier un objet eAdminFieldsListRenderer
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="tab">Table main</param>
        protected eAdminFieldsListRenderer(ePref pref, int tab)
        {
            Pref = pref;
            _tab = tab;
            listColumns = new List<ListCol>()
            {
                new ListCol(eResApp.GetRes(Pref, 222), 70),
                new ListCol("DescId", 60),
                new ListCol(eResApp.GetRes(Pref, 223), 150),
                new ListCol(eResApp.GetRes(Pref, 105), 100),
                new ListCol(eResApp.GetRes(Pref , 7697), 98, FieldFormat.TYP_BIT),
                new ListCol(eResApp.GetRes(Pref, 17), 85, hideForPrint: true),
                new ListCol(eResApp.GetRes(Pref, 7557), 100, hideForPrint: true),
                new ListCol(eResApp.GetRes(Pref, 7698), 100, hideForPrint: true),
                new ListCol(eResApp.GetRes(Pref, 8238), 100, FieldFormat.TYP_BIT, hideForPrint: true), // Annulation de la saisie autorisée
                new ListCol(eResApp.GetRes(Pref, 528), 100),
                new ListCol(eResApp.GetRes(Pref, 7595), 75, FieldFormat.TYP_NUMERIC, true),
                new ListCol(eResApp.GetRes(Pref, 7699), 70, FieldFormat.TYP_NUMERIC, true),
                new ListCol(eResApp.GetRes(Pref, 7700), 75, FieldFormat.TYP_NUMERIC, true),
                new ListCol(eResApp.GetRes(Pref, 7701), 60, hideForPrint: true),
                new ListCol(eResApp.GetRes(Pref, 7702), 100),
                new ListCol(eResApp.GetRes(Pref, 7373), 75),
                new ListCol(eResApp.GetRes(Pref, 7703), 75),
                new ListCol(eResApp.GetRes(Pref, 7704), 100),
                new ListCol(eResApp.GetRes(Pref, 805), 100),
                new ListCol(eResApp.GetRes(Pref, 7548), 100),
                new ListCol(eResApp.GetRes(Pref, 7651), 100),
                new ListCol(eResApp.GetRes(Pref, 7705), 105),
                new ListCol(eResApp.GetRes(Pref, 7706), 85)

            };
        }

        /// <summary>
        /// Creates the admin fields list renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        public static eAdminFieldsListRenderer CreateAdminFieldsListRenderer(ePref pref, int tab)
        {
            return new eAdminFieldsListRenderer(pref, tab);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {

                eudoDAL eDal = eLibTools.GetEudoDAL(_ePref);

                try
                {
                    eDal.OpenDatabase();

                    _fields = eAdminFieldInfos.GetFieldsList(Pref, eDal, _tab, out _sErrorMsg);
                    if (!String.IsNullOrEmpty(_sErrorMsg))
                        return false;

                    LoadFormulasTriggers(eDal);
                    if (!String.IsNullOrEmpty(_sErrorMsg))
                        return false;

                    // RES
                    StringBuilder resSb = new StringBuilder();
                    foreach (eAdminFieldInfos fi in _fields)
                    {
                        if (fi.DescId != fi.PopupDescId && fi.PopupDescId != 0 && fi.PopupDescId % 100 == 1)
                        {
                            if (resSb.Length > 0)
                                resSb.Append(",");
                            resSb.Append(fi.PopupDescId - fi.PopupDescId % 100);
                        }


                    }
                    if (resSb.Length > 0)
                        _res = new eResInternal(Pref, resSb.ToString());

                }
                finally
                {
                    eDal.CloseDatabase();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                _pgContainer.Controls.Add(CreateTable(_fields, out _sErrorMsg));
                if (!String.IsNullOrEmpty(_sErrorMsg))
                    return false;

                return true;
            }
            return false;

        }

        /// <summary>
        /// Loads the formulas triggers.
        /// </summary>
        /// <param name="eDal">The e dal.</param>
        private void LoadFormulasTriggers(eudoDAL eDal)
        {
            int descid = 0;
            eAdminTriggerField field;

            RqParam rq = eSqlDesc.GetFormulasTriggersRqParam(_tab, this.Pref.Lang);

            DataTableReaderTuned dtr = eDal.Execute(rq, out _sErrorMsg);
            if (!String.IsNullOrEmpty(_sErrorMsg))
                return;

            while (dtr.Read())
            {
                descid = dtr.GetEudoNumeric("Descid");
                field = new eAdminTriggerField(dtr.GetEudoNumeric("TriggerDescid"), dtr.GetString("TriggerFile"), dtr.GetString("TriggerField"), dtr.GetString("TriggerFormula"));
                if (!_dicTriggers.ContainsKey(descid))
                {
                    _dicTriggers.Add(descid, new List<eAdminTriggerField> { field });
                }
                else
                {
                    _dicTriggers[descid].Add(field);
                }

            }
            dtr.Dispose();

        }

        /// <summary>
        /// Construction du tableau HTML
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        private HtmlGenericControl CreateTable(List<eAdminFieldInfos> list, out String sError)
        {
            sError = String.Empty;
            bool bResFound = false;

            int lineNum = 1;

            HtmlGenericControl htmlTable = new HtmlGenericControl("table");
            HtmlGenericControl sectionThead = new HtmlGenericControl("thead");
            HtmlGenericControl sectionTbody = new HtmlGenericControl("tbody");
            //sectionTbody.Style.Add("height", "500px");
            HtmlGenericControl tableTr = new HtmlGenericControl("tr");
            HtmlGenericControl tableTh = new HtmlGenericControl("th");
            HtmlGenericControl tableTd = new HtmlGenericControl("td");

            htmlTable.ID = "tableFieldsList";
            htmlTable.Attributes.Add("class", "mTab");

            #region HEADER
            htmlTable.Controls.Add(sectionThead);

            sectionThead.Controls.Add(tableTr);

            int colNum = 0;
            String thClass;
            foreach (ListCol col in listColumns)
            {
                thClass = "hdBgCol";

                tableTh = new HtmlGenericControl("th");
                if (col.Format == FieldFormat.TYP_BIT)
                {
                    thClass += " chkCol";
                }
                else if (col.Format == FieldFormat.TYP_NUMERIC)
                {
                    thClass += " numCol";
                }
                if (col.HideForPrint)
                {
                    thClass += " hiddenForPrint";
                }

                tableTh.Attributes.Add("class", thClass);
                tableTh.Style.Add("min-width", col.Width + "px");
                tableTh.InnerHtml = String.Concat(
                    "<div>",
                    col.Label,
                    "<div class='buttonsSort'>",
                    "<span class='icon-caret-up btnSort' onclick=\"nsAdminFieldsList.sortTable('tableFieldsList', ", colNum, ", 1, ", (col.Format == FieldFormat.TYP_NUMERIC) ? "true" : "false", ")\"></span>",
                    "<span class='icon-caret-down btnSort' onclick=\"nsAdminFieldsList.sortTable('tableFieldsList', ", colNum, ", -1, ", (col.Format == FieldFormat.TYP_NUMERIC) ? "true" : "false", ")\"></span>",
                    (col.Label == eResApp.GetRes(Pref, 7699)) ? "<span class='icon-exchange' onclick='nsAdminFieldsList.updateTabIndex();' title='" + eResApp.GetRes(Pref, 7925).Replace("'", "''") + "'></span>" : "",
                    "</div>",
                    "</div>"
                    );
                tableTh.Attributes.Add("title", col.Label);
                tableTr.Controls.Add(tableTh);

                colNum++;
            }
            #endregion

            #region BODY

            htmlTable.Controls.Add(sectionTbody);

            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
            eDal.OpenDatabase();

            try
            {
                String ruleDef = String.Empty;
                int descid = 0;

                //sectionTbody.Style.Add("height", _nTableH + "px");
                foreach (eAdminFieldInfos fi in list)
                {
                    descid = fi.DescId;

                    tableTr = new HtmlGenericControl("tr");
                    tableTr.Attributes.Add("class", String.Concat("line", lineNum));
                    tableTr.Attributes.Add("data-did", fi.DescId.ToString());

                    lineNum = (lineNum == 1) ? 2 : 1;

                    // Nom SQL du champ
                    CreateCell(tableTr, descid, fi.FieldName);

                    //DescId
                    CreateCell(tableTr, descid, descid.ToString());

                    // Libellé
                    CreateCell(tableTr, descid, fi.Labels[Pref.LangId],
                        isEditable: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.RES, Pref.LangId));

                    // Type
                    CreateCell(tableTr, descid,
                        (fi.Format != FieldFormat.TYP_CHAR) ? eAdminTools.GetFieldTypeLabel(Pref, fi.Format) : eAdminTools.GetCharTypeLabel(Pref, descid, fi.PopupType, fi.PopupDescId));

                    // Administration restreinte
                    CreateCell(tableTr, descid, fi.SuperAdminOnly ? "1" : "0", bShowTooltip: false,
                        isEditable: true, isCheckbox: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.SUPERADMINONLY.GetHashCode()));

                    //historique
                    CreateCell(tableTr, descid, (fi.Historic) ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59), hideForPrint: true);
                    //CreateCell(tableTr, descid, fi.Historic ? "1" : "0", bShowTooltip: true, tooltip: eResApp.GetRes(Pref, 816),
                    //    isEditable: true, isCheckbox: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.HISTORIC.GetHashCode()));

                    // Infobulle
                    CreateCell(tableTr, descid, fi.ToolTipText,
                            isEditable: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.RESADV, eLibConst.RESADV_TYPE.TOOLTIP.GetHashCode()), hideForPrint: true);

                    // Filigrane
                    CreateCell(tableTr, descid, fi.WaterMark,
                        isEditable: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.RESADV, eLibConst.RESADV_TYPE.WATERMARK.GetHashCode()), hideForPrint: true);

                    // Annulation de la saisie autorisée
                    CreateCell(tableTr, descid, value: fi.CancelLastValueAllowed ? "1" : "0", isEditable: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.CANCELLASTVALUE.GetHashCode()),
                        isCheckbox: true, hideForPrint: true);

                    // Valeur par défaut
                    CreateCell(tableTr, descid, (fi.Default.ToLower().Contains("select")) ? String.Concat("<", eResApp.GetRes(Pref, 7707), ">") : fi.Default, true, fi.Default);

                    // Ordre d'affichage
                    CreateCell(tableTr, descid, fi.Disporder.ToString(), hideForPrint: true);

                    // Ordre de saisie
                    CreateCell(tableTr, descid, fi.TabIndex.ToString(),
                        isEditable: true, attrUpd: eAdminTools.GetUpdateAttribute(eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.TABINDEX.GetHashCode()), hideForPrint: true);

                    // Longueur/décimales
                    CreateCell(tableTr, descid, (fi.Format == FieldFormat.TYP_CHAR || fi.Format == FieldFormat.TYP_NUMERIC) ? fi.Length.ToString() : "", hideForPrint: true);

                    // Somme dans les entêtes
                    CreateCell(tableTr, descid, (fi.ComputedFieldEnabled) ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59), hideForPrint: true);

                    // Nature de catalogue
                    if (fi.PopupType != EudoQuery.PopupType.NONE)
                        CreateCell(tableTr, descid, eAdminTools.GetCatalogNature(Pref, fi.PopupType, fi.Multiple, fi.GetFileDataParam(Pref).TreeView));
                    else
                        CreateCell(tableTr, descid);

                    // Source
                    if (fi.DescId != fi.PopupDescId && fi.PopupDescId != 0 && fi.PopupDescId % 100 > 1)
                        CreateCell(tableTr, descid, eLibTools.GetFullNameByDescId(eDal, fi.PopupDescId, Pref.Lang));
                    else
                        CreateCell(tableTr, descid);

                    // Onglet lié
                    if (fi.DescId != fi.PopupDescId && fi.PopupDescId != 0 && fi.PopupDescId % 100 == 1)
                        CreateCell(tableTr, descid, _res.GetRes(fi.PopupDescId - fi.PopupDescId % 100, out bResFound));
                    else
                        CreateCell(tableTr, descid);

                    // Visibilité
                    CreateConditionsCell(tableTr, TypeTraitConditionnal.FieldView, fi.DescId);

                    // Modification
                    CreateConditionsCell(tableTr, TypeTraitConditionnal.FieldUpdate, fi.DescId);

                    // Saisie obligatoire
                    CreateCell(tableTr, fi.DescId, fi.Obligat ? "1" : "0");

                    // Après enregistrement - Formule du haut
                    if (!String.IsNullOrEmpty(fi.Formula))
                        CreateCell(tableTr, descid, eResApp.GetRes(Pref, 1700), true, fi.Formula);
                    else
                        CreateCell(tableTr, descid);

                    // Dépendances
                    CreateDependancesCell(eDal, tableTr, fi.DescId);

                    // Déclencheurs de
                    CreateAutomatismsCell(eDal, tableTr, fi);



                    sectionTbody.Controls.Add(tableTr);
                }
                #endregion
            }
            catch (Exception exc)
            {
                sError = exc.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }


            return htmlTable;
        }

        /// <summary>
        /// Création d'une cellule avec sa valeur
        /// </summary>
        /// <param name="tr">Ligne "tr"</param>
        /// <param name="descid">The descid.</param>
        /// <param name="value">Valeur</param>
        /// <param name="bShowTooltip">Affichage d'une infobulle</param>
        /// <param name="tooltip">Infobulle</param>
        /// <param name="isEditable">Affichage d'une icône si la cellule est éditable</param>
        /// <param name="attrUpd">The attribute upd.</param>
        /// <param name="isCheckbox">if set to <c>true</c> [is checkbox].</param>
        /// <param name="hideForPrint">if set to <c>true</c> [hide for print].</param>
        void CreateCell(HtmlGenericControl tr, int descid,
            String value = "", Boolean bShowTooltip = true, String tooltip = "", Boolean isEditable = false, String attrUpd = "", Boolean isCheckbox = false, Boolean hideForPrint = false)
        {
            String cssClass = "";

            HtmlGenericControl td = new HtmlGenericControl("td");
            if (isCheckbox)
                cssClass += " center";
            if (hideForPrint)
                cssClass += " hiddenForPrint";
            td.Attributes.Add("class", cssClass);

            String htmlIcon = String.Empty;
            if (isEditable)
            {
                htmlIcon = String.Concat(
                    "<span class='icon-edn-pen btnEdit'></span>"
                    );
                td.Attributes.Add("data-editable", "1");
            }

            td.Attributes.Add("value", value);

            if (!isCheckbox)
            {
                td.InnerHtml = String.Concat("<span class='cellContent'>", "<span class='cellValue'>", value, "</span>", htmlIcon, "</span>");
                if (isEditable)
                {
                    td.InnerHtml = String.Concat(td.InnerHtml, "<input type='text' class='txtValueEdit' value='", value, "' dsc='", attrUpd, "' did='", descid, "' />");
                }
            }
            else
            {
                eCheckBoxCtrl check = new eCheckBoxCtrl(value == "1", false);
                check.AddClick("nsAdmin.sendJson(this)");
                check.Attributes.Add("dsc", attrUpd);
                check.Attributes.Add("did", descid.ToString());
                td.Controls.Add(check);

            }


            if (bShowTooltip)
                td.Attributes.Add("title", (tooltip == "") ? value : tooltip);

            tr.Controls.Add(td);
        }

        /// <summary>
        /// Création d'une cellule avec la liste des conditions de la règle
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="type"></param>
        /// <param name="descid"></param>
        void CreateConditionsCell(HtmlGenericControl tr, TypeTraitConditionnal type, int descid)
        {
            String ruleDef = "<ul>";
            eRules listRules = eRules.GetRules(type, descid, Pref);
            if (listRules.AllRules.Count > 0)
            {
                foreach (Tuple<AdvFilter, InterOperator> f in listRules.AllRules[0].ListFilter)
                {
                    ruleDef = String.Concat(ruleDef, "<li> ", f.Item1.FilterName, "</li>");
                }
            }
            ruleDef = ruleDef + "</ul>";
            CreateCell(tr, descid, ruleDef, false);
        }

        /// <summary>
        /// Création d'une cellule avec la liste des dépendances
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="tr"></param>
        /// <param name="descid"></param>
        void CreateDependancesCell(eudoDAL eDal, HtmlGenericControl tr, int descid)
        {
            String list = "<ul>";

            foreach (String d in eSqlDesc.GetSQLDependencies(eDal, descid))
            {
                list = String.Concat(list, "<li> ", d, "</li>");
            }

            list = list + "</ul>";
            CreateCell(tr, descid, list, false);
        }

        /// <summary>
        /// Création d'une cellule "Déclencheur de"
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="tr"></param>
        /// <param name="fi"></param>
        void CreateAutomatismsCell(eudoDAL eDal, HtmlGenericControl tr, eAdminFieldInfos fi)
        {
            String list = "<ul>";

            if (_dicTriggers.ContainsKey(fi.DescId))
            {
                foreach (eAdminTriggerField trigger in _dicTriggers[fi.DescId])
                {
                    list = String.Concat(list, "<li title='", HttpUtility.HtmlEncode(trigger.Formula), "'> ", trigger.ToString(), "</li>");
                }
            }



            list = list + "</ul>";
            CreateCell(tr, fi.DescId, list);
        }



        /// <summary>
        /// Objet représentant une colonne 
        /// </summary>
        public class ListCol
        {
            /// <summary>
            /// Libellé
            /// </summary>
            public String Label { get; private set; }
            /// <summary>
            /// Largeur
            /// </summary>
            public int Width { get; private set; }
            /// <summary>
            /// Format du champ
            /// </summary>
            public FieldFormat Format { get; private set; }
            /// <summary>
            /// Caché lors de l'impression
            /// </summary>
            public Boolean HideForPrint { get; private set; }
            /// <summary>
            /// Initializes a new instance of the <see cref="ListCol"/> class.
            /// </summary>
            /// <param name="label">The label.</param>
            /// <param name="width">The width.</param>
            /// <param name="format">The format.</param>
            /// <param name="hideForPrint">if set to <c>true</c> [hide for print].</param>
            public ListCol(String label, int width, FieldFormat format = FieldFormat.TYP_CHAR, Boolean hideForPrint = false)
            {
                this.Label = label;
                this.Width = width;
                this.Format = format;
                this.HideForPrint = hideForPrint;
            }

        }
    }
}