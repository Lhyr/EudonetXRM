using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoExtendedClasses;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using static Com.Eudonet.Core.Model.eKanban;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu des paramètres pour le widget Kanban
    /// </summary>
    public class eKanbanParamRenderer : eWidgetSpecificParamRenderer
    {


        /// <summary>
        /// Initializes a new instance of the <see cref="eKanbanParamRenderer" /> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">eFile</param>
        /// <param name="param">eXrmWidgetParam</param>
        /// <param name="context">The context.</param>
        public eKanbanParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context) : base(pref, isVisible, file, param, true, context)
        {
        }

        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            List<Tuple<string, string>> listTab;

            if (_context.GridLocation == eXrmWidgetContext.eGridLocation.Default)
                listTab = eAdminTools.GetListTabs(Pref).Select(x => new Tuple<string, string>(x.Item1.ToString(), x.Item2)).ToList();
            else
                listTab = eSqlDesc.GetBkm(this.Pref, _context.ParentTab).OrderBy(t => t.Value).Select(x => new Tuple<string, string>(x.Key.ToString(), x.Value)).ToList();

            int tab = _widgetParams.GetParamValueInt("tab");
            string selectedField = _widgetParams.GetParamValue("catalog");
            int fieldDescid = 0;
            string catValues = _widgetParams.GetParamValue("catvalues");
            string[] arrCatValues;
            String catDisplayValue = string.Empty;
            List<AdvCatalogField> listFields = new List<AdvCatalogField>();
            AdvCatalogField field = new AdvCatalogField { DescID = 0, PopupDescID = 0 };

            if (!String.IsNullOrEmpty(selectedField))
            {
                try
                {
                    field = JsonConvert.DeserializeObject<AdvCatalogField>(selectedField);
                    fieldDescid = field.DescID;
                }
                catch (Exception)
                {

                }
            }

            // Onglet
            _pgContainer.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 7992), "", _contentParamField, listTab, "oAdminGridMenu.updateParam(this, true);", "tab", tab.ToString()));

            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
            eDal.OpenDatabase();
            try
            {
                listFields = GetAdvCatalogFieldsList(eDal, tab);
                // Recherche de la rubrique dans la liste des rubriques diponibles
                // Si elle n'y est pas, descid = 0
                //field = listFields.FirstOrDefault(f => f.DescID == fieldDescid);
                fieldDescid = field.DescID;

                #region Rubrique en colonne

                string value = string.Empty;
                if (field.DescID > 0)
                    value = JsonConvert.SerializeObject(field);

                HtmlGenericControl select = new HtmlGenericControl("select");
                // Options
                HtmlGenericControl option;
                option = new HtmlGenericControl("option");
                option.InnerText = eResApp.GetRes(_ePref, 8166);
                select.Controls.Add(option);
                foreach (AdvCatalogField f in listFields)
                {
                    option = new HtmlGenericControl("option");
                    option.InnerText = f.Label;
                    option.Attributes.Add("value", f.DescID.ToString());
                    option.Attributes.Add("data-popid", f.PopupDescID.ToString());
                    if (fieldDescid == f.DescID)
                        option.Attributes.Add("selected", "true");
                    select.Controls.Add(option);
                }

                HtmlGenericControl divSelect = (HtmlGenericControl)BuildSelectOptionField(eResApp.GetRes(Pref, 8438), "", _contentParamField, new List<Tuple<string, string>>(),
                    $"oAdminGridMenu.kanbanFieldOnChange(this);", "catalog", value, htmlSelect: select);

                _pgContainer.Controls.Add(divSelect);
                #endregion

                #region Colonnes à afficher

                if (field.PopupDescID != 0)
                    fieldDescid = field.PopupDescID;

                // On n'affiche la sélection des valeurs que si une rubrique est sélectionnée
                if (fieldDescid > 0)
                {
                    // On ne garde que les 10 premières valeurs sélectionnées 
                    if (!String.IsNullOrEmpty(catValues))
                    {
                        arrCatValues = catValues.Split(';');
                        arrCatValues = arrCatValues.Take(10).ToArray();
                        catValues = String.Join(";", arrCatValues);

                        eCatalog cat = new eCatalog(eDal, this.Pref, PopupType.DATA, this.Pref.User, fieldDescid);
                        catDisplayValue = String.Join(", ", cat.Values.Where(v => arrCatValues.Contains(v.DbValue)).Select(v => v.DisplayValue));
                    }


                    _pgContainer.Controls.Add(BuildMultiCatalog(eResApp.GetRes(Pref, 8439), _contentParamField, fieldDescid, "catvalues", catValues, catDisplayValue));

                    // Afficher la colonne "Non affectées"
                    _pgContainer.Controls.Add(BuildCheckboxField(eResApp.GetRes(_ePref, 8543), _contentParamField, "displayemptycol", _widgetParams.GetParamValue("displayemptycol"), "oAdminGridMenu.updateParam(this, true);"));
                }

                #endregion

                #region Boutons

                // Administrer la carte
                _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 8443),
                    clientClick: $"nsAdminKanban.configureKanbanMinifile({tab}, {_widgetParams.WidgetId})",
                    paramName: "mapping",
                    value: _widgetParams.GetParamValue("mapping"),
                    btnId: "btnAdminKanbanMap")
                    );

                // Filtre par défaut
                // TODO US #2147 - Tâche #3127 - Demande #70 070 - Le filtre par défaut est dénommé Filtre permanent s'il n'est pas désactivable/remplaçable par un autre filtre avancé
                String btnLabel = String.Concat(eResApp.GetRes(Pref, 1102), ((_widgetParams.GetParamValueInt("filterid") > 0) ? " (1)" : ""));
                _pgContainer.Controls.Add(BuildBtnField(btnLabel, "", _contentParamField, "oAdminGridMenu.showFilterEditor(this)", "filterid", _widgetParams.GetParamValue("filterid"),
                    containerId: "kanbanFilterIdContainer", isVisible: _isVisible));


                // Lignes de couloir
                _resLoc.Identifier = "swimlanes";
                _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 8444), clientClick: $"nsAdminKanban.configureSwimlanes({_widgetParams.WidgetId}, '{JsonConvert.SerializeObject(_resLoc)}');"));

                #endregion
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                eDal.CloseDatabase();
            }

        }

        /// <summary>
        /// Retourne la liste des rubriques de type catalogue multiple disponibles
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="tab"></param>
        /// <returns></returns>
        private List<AdvCatalogField> GetAdvCatalogFieldsList(eudoDAL eDal, int tab)
        {
            List<AdvCatalogField> fieldsList = new List<AdvCatalogField>();
            AdvCatalogField field;
            DataTableReaderTuned dtr = eSqlCatalog.GetAdvCatalogFields(Pref, eDal, tab, false, false, out _sErrorMsg);
            if (!String.IsNullOrEmpty(_sErrorMsg))
                throw new Exception(_sErrorMsg);
            while (dtr.Read())
            {
                field = new AdvCatalogField
                {
                    DescID = dtr.GetEudoNumeric("DescID"),
                    PopupDescID = dtr.GetEudoNumeric("PopupDescID"),
                    Label = dtr.GetString("Label")
                };
                fieldsList.Add(field);
            }
            dtr.Dispose();
            return fieldsList;
        }
    }
}