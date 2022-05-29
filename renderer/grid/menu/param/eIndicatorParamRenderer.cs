using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Xrm.eConst;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu des paramètres du widget Indicateur
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eWidgetSpecificParamRenderer" />
    public class eIndicatorParamRenderer : eWidgetSpecificParamRenderer
    {
        Boolean _isRatio = false;
        eFieldRecord _fieldContentType;
        eudoDAL _dal = null;
        eResCodeTranslationManager _resCodeMgr = null;


        #region Structs        
        /// <summary>
        /// Champs de la table
        /// </summary>
        public struct TabFields
        {
            /// <summary>
            /// DescId de la table
            /// </summary>
            public int Tab;
            /// <summary>
            /// Dictionnaire des champs
            /// </summary>
            public Dictionary<int, string> Fields;
        }
        #endregion

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>    
        public eIndicatorParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context) : base(pref, isVisible, file, param, context: context)
        {
        }




        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _dal = eLibTools.GetEudoDAL(Pref);
            _dal.OpenDatabase();
            try
            {
                _resCodeMgr = new eResCodeTranslationManager(this.Pref, _dal);

                _fieldContentType = _file.GetField((int)XrmWidgetField.ContentType);

                List<TabFields> availableFieldsFromTabsList = new List<TabFields>();
                List<Tuple<string, string>> listTab = new List<Tuple<string, string>>();
                List<Tuple<string, string>> listOperator = new List<Tuple<string, string>>();

                //Récupération des tabs actives            
                if (_context.GridLocation == eXrmWidgetContext.eGridLocation.Default)
                    listTab = eAdminTools.GetListTabs(Pref).Select(x => new Tuple<string, string>(x.Item1.ToString(), x.Item2)).ToList();
                else
                    listTab = eSqlDesc.GetBkm(this.Pref, _context.ParentTab).Select(x => new Tuple<string, string>(x.Key.ToString(), x.Value)).ToList();

                //Si pas de tab sélectionné, on charge les rubriques de la premiere table
                //Par défaut, pas de tab sélectionné, pas de rubriques dispo
                /*
                if (_widgetParams.GetParamValueInt("tabnumid") == 0)
                    _widgetParams.SetParam("tabnumid", listTab.FirstOrDefault().Item1);
                if (_widgetParams.GetParamValueInt("tabdenid") == 0)
                    _widgetParams.SetParam("tabdenid", listTab.FirstOrDefault().Item1);

    */

                //Recuperation des rubriques de type numeric associés aux tables
                availableFieldsFromTabsList = getAllAvalaibleFieldsFromListTab(listTab);

                //Recuperation des operateurs
                listOperator = getOperatorList();

                _isRatio = _fieldContentType.Value == "1";

                getCommonFields(_pgContainer);

                //Si nature de l indicateur = ratio on affiche la partie numerateur et denominateur

                HtmlGenericControl containerRatio = new HtmlGenericControl("div");
                containerRatio.ID = "containerRatio";
                if (_isRatio)
                    containerRatio.Attributes.Add("style", "display:inline;");
                else
                    containerRatio.Attributes.Add("style", "display:none;");

                //Numerateur
                getNumeratorFields(_pgContainer, _isRatio, listTab, availableFieldsFromTabsList, listOperator);

                //Denominateur
                getDenominatorFields(containerRatio, _isRatio, listTab, availableFieldsFromTabsList, listOperator);

                _pgContainer.Controls.Add(containerRatio);
            }

            finally
            {
                _dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Retourne les champs commun (Nature, Unité, Libellé)
        /// </summary>
        /// <param name="container">Container des controls</param>
        private void getCommonFields(Control container)
        {
            int rCode = 0;

            //Nature de l'indicateur
            container.Controls.Add(BuildYesNoOptionField(eResApp.GetRes(Pref, 8029), _fieldContentType, "oAdminGridMenu.onChangeIndicator(this, event);", eResApp.GetRes(Pref, 8030), eResApp.GetRes(Pref, 164), null, null, true));

            //Unité
            string unit = _resCodeMgr.Translate(_widgetParams.GetParamValue("unit"), out rCode);
            container.Controls.Add(BuildWidgetParamTextbox(_widgetParams.WidgetId, eResApp.GetRes(_ePref, 1353), "unit", unit, true, rCode, _resLoc));

            // Position de l'unité
            List<Tuple<string, string>> positionsList = new List<Tuple<string, string>>();
            positionsList.Add(new Tuple<string, string>(
                ((int)IndicatorWidgetUnitPosition.RightWithSpace).ToString(),
                String.Concat(eResApp.GetRes(Pref, 8408), ", ", eResApp.GetRes(Pref, 8410))));
            positionsList.Add(new Tuple<string, string>(
                ((int)IndicatorWidgetUnitPosition.RightWithoutSpace).ToString(),
                String.Concat(eResApp.GetRes(Pref, 8408), ", ", eResApp.GetRes(Pref, 8411))));
            positionsList.Add(new Tuple<string, string>(
                ((int)IndicatorWidgetUnitPosition.LeftWithSpace).ToString(),
                String.Concat(eResApp.GetRes(Pref, 8409), ", ", eResApp.GetRes(Pref, 8410))));
            positionsList.Add(new Tuple<string, string>(
                ((int)IndicatorWidgetUnitPosition.LeftWithoutSpace).ToString(),
                String.Concat(eResApp.GetRes(Pref, 8409), ", ", eResApp.GetRes(Pref, 8411))));
            string value = String.IsNullOrEmpty(_widgetParams.GetParamValue("unitPosition")) ? "0" : _widgetParams.GetParamValue("unitPosition");
            container.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 8407), string.Empty, _contentParamField, positionsList, "", "unitPosition", value, disableNoValue: true));

            // En pourcentage
            HtmlGenericControl div = (HtmlGenericControl)BuildYesNoOptionField(eResApp.GetRes(Pref, 6060), _contentParamField, "oAdminGridMenu.onPercentOptionChange(this, event);", paramName: "unitinpercent", value: _widgetParams.GetParamValue("unitinpercent"));
            div.ID = "wrapperUnitInPercent";
            div.Style.Add("display", (_isRatio) ? "block" : "none");
            container.Controls.Add(div);

            //Libellé
            string label = _resCodeMgr.Translate(_widgetParams.GetParamValue("libelle"), out rCode);
            container.Controls.Add(BuildWidgetParamTextbox(_widgetParams.WidgetId, eResApp.GetRes(_ePref, 223), "libelle", label, true, rCode, _resLoc));
        }

        /// <summary>
        /// Retourne les controles du numérateur
        /// </summary>
        /// <param name="container">Container des champs</param>
        /// <param name="isRatio">Vrai si ratio sélecionné</param>
        /// <param name="listTab">Liste des tables disponibles</param>
        /// <param name="avalaibleFieldsFromListTab">The avalaible fields from list tab.</param>
        /// <param name="listOperator">Liste des opérateurs</param>
        private void getNumeratorFields(Control container, bool isRatio, List<Tuple<string, string>> listTab, List<TabFields> avalaibleFieldsFromListTab, List<Tuple<string, string>> listOperator)
        {
            string operatorNum = _widgetParams.GetParamValue("operatorNum");
            if (String.IsNullOrEmpty(operatorNum))
                operatorNum = "NB";

            //Onglet
            container.Controls.Add(
                BuildSelectOptionField(isRatio ? eResApp.GetRes(Pref, 8031) : eResApp.GetRes(Pref, 264), "",
                _contentParamField,
                listTab,
                "oAdminGridMenu.onTabIndicatorChange(this);",
                "tabNumId",
                 _widgetParams.GetParamValue("tabnumid")));
            //Opérateur
            container.Controls.Add(BuildSelectOptionField(isRatio ? eResApp.GetRes(Pref, 8032) : eResApp.GetRes(Pref, 8040), "", _contentParamField, listOperator, "oAdminGridMenu.onOpIndicatorChange(this); ", "operatorNum", operatorNum));


            //On construit toutes les DDL pour chaque table 
            HtmlGenericControl ddlRubrique;
            int fieldId = 0;
            foreach (TabFields pairTabList in avalaibleFieldsFromListTab)
            {
                //Si pas de field defini (creation widget) ou si DDL différente de la ddl courante (chargement de tt les DDL)
                if (_widgetParams.GetParamValueInt("fieldnumid") == 0 || pairTabList.Tab.ToString() != _widgetParams.GetParamValue("tabnumid"))
                    fieldId = pairTabList.Fields.Count > 0 ? pairTabList.Fields.FirstOrDefault().Key : 0;
                else
                    fieldId = _widgetParams.GetParamValueInt("fieldnumid");

                //Rubrique
                ddlRubrique = (HtmlGenericControl)BuildSelectOptionField(isRatio ? eResApp.GetRes(Pref, 8033) : eResApp.GetRes(Pref, 222), "", _contentParamField,
                    pairTabList.Fields.Select(f => Tuple.Create(f.Key.ToString(), f.Value)).ToList(), "", "fieldNumId", fieldId.ToString());
                ddlRubrique.ID = "ddlFieldsNum_" + pairTabList.Tab;
                //On masque toutes les DDL qui ne sont pas concernés ou toutes les DDL si opérateur = Nombre de fiche
                if (pairTabList.Tab.ToString() != _widgetParams.GetParamValue("tabnumid") || operatorNum == "NB")
                {
                    ddlRubrique.Attributes.Add("style", "display:none;");
                    ((HtmlGenericControl)ddlRubrique.Controls[1]).Attributes.Add("style", "display:none;");
                }
                container.Controls.Add(ddlRubrique);
            }

            //Filtre
            string btnLabel = isRatio ? eResApp.GetRes(Pref, 8034) : eResApp.GetRes(Pref, 8016);
            if (_widgetParams.GetParamValueInt("filternumid") > 0)
            {
                btnLabel = String.Concat(btnLabel, " (1)");
            }
            container.Controls.Add(BuildBtnField(btnLabel, "", _contentParamField, "oAdminGridMenu.showSelectFilter(this, 'tabNumId');", "filterNumId", _widgetParams.GetParamValue("filternumid")));
        }

        /// <summary>
        /// Retourne les controles du denominateur
        /// </summary>
        /// <param name="container">Container des champs</param>
        /// <param name="isRatio">Vrai si ratio sélecionné</param>
        /// <param name="listTab">Liste des tables disponibles</param>
        /// <param name="avalaibleFieldsFromListTab">The avalaible fields from list tab.</param>
        /// <param name="listOperator">Liste des opérateurs</param>
        private void getDenominatorFields(HtmlGenericControl container, bool isRatio, List<Tuple<string, string>> listTab, List<TabFields> avalaibleFieldsFromListTab, List<Tuple<string, string>> listOperator)
        {
            string operatorNum = _widgetParams.GetParamValue("operatorNum");
            string operatorDen = _widgetParams.GetParamValue("operatorDen");

            if (String.IsNullOrEmpty(operatorNum))
                operatorNum = "NB";
            if (String.IsNullOrEmpty(operatorDen))
                operatorDen = "NB";

            //Onglet
            container.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 8035), "", _contentParamField, listTab, "oAdminGridMenu.onTabIndicatorChange(this);", "tabDenId", _widgetParams.GetParamValue("tabdenid")));
            //Opérateur
            container.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 8036), "", _contentParamField, listOperator, "oAdminGridMenu.onOpIndicatorChange(this);", "operatorDen", operatorDen));

            //On construit toutes les DDL pour chaque table
            HtmlGenericControl ddlRubrique;
            int fieldId = 0;
            foreach (TabFields pairTabList in avalaibleFieldsFromListTab)
            {
                //Si pas de field defini ou si DDL différente de la ddl courante
                if (_widgetParams.GetParamValueInt("fielddenid") == 0 || pairTabList.Tab.ToString() != _widgetParams.GetParamValue("tabdenid"))
                    fieldId = pairTabList.Fields.Count > 0 ? pairTabList.Fields.FirstOrDefault().Key : 0;
                else
                    fieldId = _widgetParams.GetParamValueInt("fielddenid");

                //Rubrique
                ddlRubrique = (HtmlGenericControl)BuildSelectOptionField(eResApp.GetRes(Pref, 8037), "", _contentParamField,
                    pairTabList.Fields.Select(f => Tuple.Create(f.Key.ToString(), f.Value)).ToList(),
                    "", "fieldDenId", fieldId.ToString());
                ddlRubrique.ID = "ddlFieldsDen_" + pairTabList.Tab;
                //On masque toutes les DDL qui ne sont pas concernés ou toutes les DDL si opérateur = Nombre de fiche
                if (pairTabList.Tab.ToString() != _widgetParams.GetParamValue("tabdenid") || operatorNum == "NB")
                {
                    ddlRubrique.Attributes.Add("style", "display:none;");
                    ((HtmlGenericControl)ddlRubrique.Controls[1]).Attributes.Add("style", "display:none;");
                }
                container.Controls.Add(ddlRubrique);
            }

            //Filtre
            string btnLabel = eResApp.GetRes(Pref, 8038);
            if (_widgetParams.GetParamValueInt("filterDenId") > 0)
            {
                btnLabel = String.Concat(btnLabel, " (1)");
            }
            container.Controls.Add(BuildBtnField(btnLabel, "", _contentParamField, "oAdminGridMenu.showSelectFilter(this,'tabDenId');", "filterDenId", _widgetParams.GetParamValue("filterDenId")));
        }

        /// <summary>
        /// Retourne la liste des opérateurs disponibles
        /// </summary>
        /// <returns>Liste de tuple opérateur</returns>
        private List<Tuple<string, string>> getOperatorList()
        {
            List<Tuple<string, string>> listOperator = new List<Tuple<string, string>>();
            listOperator.Add(new Tuple<string, string>("NB", eResApp.GetRes(Pref, 437)));
            listOperator.Add(new Tuple<string, string>("SUM", eResApp.GetRes(Pref, 633)));
            listOperator.Add(new Tuple<string, string>("AVG", eResApp.GetRes(Pref, 634)));
            listOperator.Add(new Tuple<string, string>("MED", eResApp.GetRes(Pref, 8039)));
            listOperator.Add(new Tuple<string, string>("MIN", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(eResApp.GetRes(Pref, 629).ToLower())));
            listOperator.Add(new Tuple<string, string>("MAX", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(eResApp.GetRes(Pref, 630).ToLower())));
            return listOperator;
        }

        /// <summary>
        /// Retourne une liste des champs disponible par table de tuple (ID de latable, list des champs)
        /// </summary>
        /// <param name="listTab">La liste des tables a récuperer</param>
        /// <returns>Liste  de tuple (ID de latable, list des champs)</returns>
        private List<TabFields> getAllAvalaibleFieldsFromListTab(List<Tuple<string, string>> listTab)
        {
            List<TabFields> listAvailableFieldsByTab = new List<TabFields>();
            foreach (Tuple<string, string> tab in listTab)
            {
                listAvailableFieldsByTab.Add(new TabFields
                {
                    Tab = eLibTools.GetNum(tab.Item1),
                    Fields = getAvailableFieldsFromParentTables(Int32.Parse(tab.Item1))
                });
            }
            return listAvailableFieldsByTab;
        }

        /// <summary>
        /// Retourne la liste des champs disponibles pour l id de la table passé en param de type numeric
        /// </summary>
        /// <param name="tableId">Id de la table sélectionnée</param>
        /// <returns>Une liste de tuple id libellé</returns>
        private Dictionary<int, string> getAvailableFieldsFromParentTables(int tableId)
        {
            eAdminTableInfos tabInfos = new eAdminTableInfos(Pref, tableId);
            return tabInfos.GetAvailableFieldsFromParentTables(Pref, true, true, 0, false, false, false, new List<int> { (int)FieldFormat.TYP_NUMERIC, (int)FieldFormat.TYP_MONEY });
        }
    }
}