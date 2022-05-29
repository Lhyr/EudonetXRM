using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Génère un rendu du filtre dans une liste déroulante affichable dans l'assistant graphique pour une table donnée en paramètre.
    /// </summary>
    public class eReportChartFilterWizardRenderer
    {

        eReport _report;
        ePref _pref;
        string _prefix;
        int _mainTabId;
        private int _filterId
        {
            get
            {
                int filterId = 0;
                if (_report == null)
                    return filterId;

                int.TryParse(_report.GetParamValue(string.Concat(_prefix, "filterId"), false), out filterId);

                return filterId;
            }
        }
        private IEnumerable<eRecord> ListeRecord;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="tabId">Id de la table main des filtres à charger</param>
        /// <param name="report">Rapport graphique concerné</param>
        /// <param name="prefix">Préfix de l'identifiant de HtmlGenericControl</param>
        public eReportChartFilterWizardRenderer(ePref pref, eReport report, int tabId , string prefix = "")
        {
            _report = report;
            _pref = pref;
            _mainTabId = tabId;
            _prefix = prefix;
        }


        private bool GenerateFilterListe()
        {
            #region récupération de la liste des filtres
            eDataFillerGeneric filler = new eDataFillerGeneric(_pref, 104000, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eq)
                {


                    //Type
                    List<WhereCustom> list = new List<WhereCustom>();

                    list.Add(new WhereCustom(FilterField.TYPE.GetHashCode().ToString(), Operator.OP_0_EMPTY, TypeFilter.USER.GetHashCode().ToString(), InterOperator.OP_AND));


                    WhereCustom wType = new WhereCustom(list);


                    //Libelle non vide
                    list = new List<WhereCustom>();
                    list.Add(new WhereCustom(FilterField.LIBELLE.GetHashCode().ToString(), Operator.OP_IS_NOT_EMPTY, ""));
                    WhereCustom wLibelle = new WhereCustom(list, InterOperator.OP_AND);


                    //Utilisateur en cours ou filtre sélectioner
                    list = new List<WhereCustom>();
                    list.Add(new WhereCustom(FilterField.USERID.GetHashCode().ToString(), Operator.OP_EQUAL, _pref.UserId.ToString(), InterOperator.OP_OR));
                    list.Add(new WhereCustom(FilterField.USERID.GetHashCode().ToString(), Operator.OP_0_EMPTY, "", InterOperator.OP_OR));
                    //list.Add(new WhereCustom(FilterField.ID.GetHashCode().ToString(), Operator.OP_EQUAL, nSelfilterId.ToString(), InterOperator.OP_OR));

                    WhereCustom wCuser = new WhereCustom(list, InterOperator.OP_AND);

                    //Table
                    list = new List<WhereCustom>();
                    //if (this._listOfTabId != null && this._listOfTabId.Count > 0)
                    //    list.Add(new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_IN_LIST, eLibTools.Join<int>(";", this._listOfTabId)));
                    //else
                    list.Add(new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, _mainTabId.ToString()));
                    WhereCustom wcTab = new WhereCustom(list, InterOperator.OP_AND);

                    //
                    list = new List<WhereCustom>();
                    list.Add(wLibelle);
                    list.Add(wType);
                    list.Add(wCuser);
                    list.Add(wcTab);


                    eq.AddCustomFilter(new WhereCustom(list));

                    eq.SetListCol = string.Concat(FilterField.LIBELLE.GetHashCode().ToString(), ";", FilterField.TAB.GetHashCode().ToString());

                };

            filler.Generate();

            if (filler.ErrorMsg.Length > 0)
                throw new Exception(filler.ErrorMsg, filler.InnerException);
            
            else if (filler.ListRecords == null)
                throw new Exception("eChartWizardRendererFillFilter : erreur de la génération de la liste des filtres disponible :ListRecords ==null ");
            
            ListeRecord = filler.ListRecords;

            return true;
            #endregion
        }

        /// <summary>
        /// Générer uen dropdownliste avec la liste des filtres d'une table
        /// </summary>
        /// <param name="li">HtmlGenericControl à peupler</param>
        /// <returns></returns>
        public Control FillFilter(Control li = null)
        {
            DropDownList ddlFilters = new DropDownList();
            if (this.GenerateFilterListe())
            {
                ListItem filterItem;
                String sFilterId;
                String sLibelle;

                
                ddlFilters.ID = string.Concat(_prefix, "ddlfilter");
                
                
                ListItem defaultItem = new ListItem();
                ddlFilters.Items.Add(defaultItem);
                ddlFilters.CssClass = string.Concat(_prefix, "filterSelect");
                //Aucun filtre sélectionné
                defaultItem.Text = eResApp.GetRes(_pref, 430);
                defaultItem.Value = "0";

                foreach (eRecord er in ListeRecord)
                {
                    //     sFilterId = dtrFilters.GetSafeValue("FilterId").ToString();
                    sFilterId = er.MainFileid.ToString();

                    eFieldRecord efLibelle = er.GetFields.Find(delegate (eFieldRecord ef)
                    {

                        return ef.FldInfo.Alias == string.Concat("104000_", FilterField.LIBELLE.GetHashCode().ToString());
                    });

                    if (efLibelle == null || efLibelle.Value.Length == 0)
                        continue;

                    eFieldRecord efTab = er.GetFields.Find(delegate (eFieldRecord ef)
                    {

                        return ef.FldInfo.Alias == string.Concat("104000_", FilterField.TAB.GetHashCode().ToString());
                    });

                    if (efTab == null || efTab.Value.Length == 0)
                        continue;

                    //     sLibelle = dtrFilters.GetSafeValue("Libelle").ToString();
                    sLibelle = efLibelle.DisplayValue;

                    filterItem = new ListItem(sLibelle, sFilterId);
                    filterItem.Selected = sFilterId.Equals(_filterId.ToString()) ? true : false;
                    filterItem.Attributes.Add("tab", efTab.Value);
                    ddlFilters.Items.Add(filterItem);
                }

                ddlFilters.Attributes.Add("onchange", String.Concat("oReport.SetParam('", _prefix.ToLower(), "filterid',this.value);"));
                if (li != null)
                {
                    li.Controls.Add(ddlFilters);
                    li.ID = string.Concat(_prefix, "liAddFilter");
                    li.Controls.Add(ddlFilters);
                }
                
            }

            return li?? ddlFilters;

        }


    }
}