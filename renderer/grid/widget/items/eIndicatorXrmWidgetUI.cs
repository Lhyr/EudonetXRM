using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Classe de rendu d'un widget Indicateur
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eAbstractXrmWidgetUI" />
    public class eIndicatorXrmWidgetUI : eAbstractXrmWidgetUI
    {
        private ePref _pref;

        /// <summary>
        /// Initializes a new instance of the <see cref="eIndicatorXrmWidgetUI"/> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        public eIndicatorXrmWidgetUI(ePref pref)
        {
            this._pref = pref;
        }


        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            eResCodeTranslationManager resMgr;
            string unit = _widgetParam.GetParamValue("unit");
            string label = _widgetParam.GetParamValue("libelle");
            int unitResCode = 0, labelResCode = 0;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {
                resMgr = new eResCodeTranslationManager(_pref, dal);
                unit = resMgr.Translate(unit, out unitResCode);
                label = resMgr.Translate(label, out labelResCode);
            }
            finally
            {
                dal.CloseDatabase();
            }



            string error = string.Empty;
            decimal indicateur = 0;
            int ratio = 0;
            string sIndicateur = string.Empty;

            //Calcul le booleen ratio
            int.TryParse(_widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentType)).Value, out ratio);
            bool isRatio = ratio == 1;

            string countQuery = _widgetParam.GetParamValue("countQuery");

            if (!String.IsNullOrEmpty(countQuery))
            {
                #region Cas d'une requête personnalisée
                if (countQuery.StartsWith("SELECT COUNT("))
                {
                    indicateur = ExecuteCountQuery(countQuery);
                    sIndicateur = eNumber.FormatNumber(_pref, indicateur, 0, true);
                }
                #endregion
            }
            else
            {
                #region Cas classique de calcul de l'indicateur

                // Refacto suite à la regression #64805 
                // Calcul du numérateur
                decimal resNum = ComputeValue("tabNumId", "fieldNumId", "filterNumId", "operatorNum");

                //Dans le cas du ratio on calcul le ratio num / den
                if (isRatio)
                {
                    // calcul du dénominateur
                    decimal resDen = ComputeValue("tabDenId", "fieldDenId", "filterDenId", "operatorDen");

                    indicateur = resDen > 0 ? resNum / resDen : 0; //???

                    if (_widgetParam.GetParamValue("unitInPercent") == "1")
                        indicateur *= 100;

                    int nbDecimal = 2; //nbre de chiffres après la virgule affichés
                    sIndicateur = eNumber.FormatNumber(_pref, Decimal.Round(indicateur, nbDecimal), nbDecimal, true);
                }
                else
                {
                    indicateur = Decimal.Round(resNum);
                    sIndicateur = eNumber.FormatNumber(_pref, indicateur, 0, true);
                }

                #endregion
            }

            HtmlGenericControl indicatorContainer = new HtmlGenericControl("div");
            indicatorContainer.Attributes.Add("class", "indicatorWrapper");

            // CRUCRU ʕ´•ᴥ•`ʔ : Dans le cas d'un indicateur de type regroupement, il est possible de cliquer sur l'indicateur pour être redirigé vers la liste filtrée ou non (#64108)
            if (!isRatio)
            {
                indicatorContainer.Attributes.Add("data-tab", _widgetParam.GetParamValue("tabNumId"));
                indicatorContainer.Attributes.Add("data-filterid", _widgetParam.GetParamValue("filterNumId"));
                indicatorContainer.Attributes.Add("data-clickable", "1");
                indicatorContainer.Attributes.Add("title", eResApp.GetRes(_pref, 8417));
            }

            #region Affichage de l'indicateur
            StringBuilder sb = new StringBuilder();
            eConst.IndicatorWidgetUnitPosition unitPos = (eConst.IndicatorWidgetUnitPosition)_widgetParam.GetParamValueInt("unitPosition");
            if (unitPos == eConst.IndicatorWidgetUnitPosition.LeftWithoutSpace || unitPos == eConst.IndicatorWidgetUnitPosition.LeftWithSpace)
            {
                sb.Append(unit);
                if (unitPos == eConst.IndicatorWidgetUnitPosition.LeftWithSpace)
                    sb.Append(" ");
            }
            sb.Append(sIndicateur);
            if (unitPos == eConst.IndicatorWidgetUnitPosition.RightWithoutSpace || unitPos == eConst.IndicatorWidgetUnitPosition.RightWithSpace)
            {
                if (unitPos == eConst.IndicatorWidgetUnitPosition.RightWithSpace)
                    sb.Append(" ");
                sb.Append(unit);
            }
            HtmlGenericControl numberContainer = new HtmlGenericControl("div");
            numberContainer.Attributes.Add("class", "widgetIndicatorNumber");
            numberContainer.InnerHtml = sb.ToString();
            #endregion

            HtmlGenericControl libelleContainer = new HtmlGenericControl("div");
            libelleContainer.Attributes.Add("class", "widgetIndicatorLibelle");
            libelleContainer.InnerHtml = label;

            indicatorContainer.Controls.Add(numberContainer);
            indicatorContainer.Controls.Add(libelleContainer);
            widgetContainer.Controls.Add(indicatorContainer);



            base.Build(widgetContainer);
        }


        /// <summary>
        /// Applique l'operateur sur les valeurs des champs sur des enregistrements filtrée
        /// </summary>
        /// <param name="tabParam">Table de calcul</param>
        /// <param name="fieldParam">champs cible</param>
        /// <param name="filterIdParam">filtrage de la liste</param>
        /// <param name="operatorParam">opertaeur a appliquer</param>
        /// <returns>La valeur calculée</returns>
        private decimal ComputeValue(string tabParam, string fieldParam, string filterIdParam, string operatorParam)
        {
            decimal result = 0;
            string _operator = _widgetParam.GetParamValue(operatorParam);

            if (String.IsNullOrEmpty(_operator))
                _operator = "NB";

            //A minima il faut selectionner tab et field pour effectuer le calcul
            if (_widgetParam.GetParamValueInt(tabParam) > 0 && (_widgetParam.GetParamValueInt(fieldParam) > 0 || _operator == "NB"))
            {
                if (_operator != "NB")
                {
                    string listCol = _widgetParam.GetParamValue(fieldParam);
                    int fldDescid = _widgetParam.GetParamValueInt(fieldParam);

                    int parentTab = (_widgetContext.GridLocation == eXrmWidgetContext.eGridLocation.Default) ? 0 : _widgetContext.ParentTab;
                    int parentFid = (_widgetContext.GridLocation == eXrmWidgetContext.eGridLocation.Default) ? 0 : _widgetContext.ParentFileId;

                    //On recupere la liste des enregsirement et on calcul la valeur du numerateur
                    eList numList = eListFactory.CreateIndicatorList(_pref, _widgetParam.GetParamValueInt(tabParam),
                         listCol, 0, 0, null, nFilterId: _widgetParam.GetParamValueInt(filterIdParam), bPagingEnabled: false,
                         ParentTab: parentTab, ParentFileId: parentFid);

                    // La liste est vide par défaut
                    var listValueNum = new List<decimal>();
                    if (numList.ListRecords != null)
                    {
                        numList.ListRecords.ForEach(record =>
                        {
                            var flds = record.GetFields.Where(field => field.FldInfo.Descid == fldDescid);
                            if (flds.Count() == 1)
                            {
                                listValueNum.Add(getDecimalFromStringValue(flds.First().Value));
                            }
                        });
                    }

                    result = indicatorCalculation.CalculateIndicator(listValueNum, _operator);
                }
                else
                {
                    result = GetCount(_widgetParam.GetParamValueInt(tabParam), _widgetParam.GetParamValueInt(filterIdParam));
                }
            }

            return result;
        }


        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            base.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="styleBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }

        /// <summary>
        /// Retourne la valeur décimale associée a la chaine passé en parametre independamenent du format
        /// </summary>
        /// <param name="value">Chaine de caractere d'une valeure numerique</param>
        /// <returns>Valeur décimale de la chaine de caractere</returns>
        private decimal getDecimalFromStringValue(string value)
        {
            decimal res = 0;
            if (String.IsNullOrEmpty(value))
                return 0;

            //On uniformise les string en FR pour la conversion => Cas des formats US etc
            // CRU : Pas besoin car la Value est déjà formatée en FR
            //if (_pref.NumberSectionsDelimiter != " ")
            //    value = value.Replace(_pref.NumberSectionsDelimiter, " ");
            if (_pref.NumberDecimalDelimiter != ",")
                value = value.Replace(_pref.NumberDecimalDelimiter, ",");

            if (Decimal.TryParse(value, NumberStyles.Any, new CultureInfo("fr-FR"), out res))
                return res;

            return 0;
        }


        /// <summary>
        /// Exécute la requête de comptage du nombre de fiches pour une table
        /// </summary>
        /// <param name="tab">Descid de la table</param>
        /// <param name="filterId">ID du filtre</param>
        /// <returns></returns>
        private int GetCount(int tab, int filterId = 0)
        {
            EudoQuery.EudoQuery eq = null;
            string query = string.Empty;
            int result = 0;
            string listCol = (tab + 1).ToString();
            WhereCustom wc = null;
            string error = string.Empty;

            try
            {
                // Filtre sur la fiche en cours (signet grille)
                int parentTab = _widgetContext.ParentTab;
                int parentFid = _widgetContext.ParentFileId;

                eq = eLibTools.GetEudoQuery(_pref, tab, ViewQuery.CUSTOM);

                if (_widgetContext.GridLocation == eXrmWidgetContext.eGridLocation.Bkm && parentFid > 0)
                {
                    if (tab == (int)TableType.PJ)
                    {
                        eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
                        eDal.OpenDatabase();

                        try
                        {
                            listCol = String.Concat(PJField.FILE.GetHashCode(), ";", PJField.FILEID.GetHashCode());

                            string file = eLibTools.GetTabNameFromDescId(eDal, parentTab, out error);
                            if (!String.IsNullOrEmpty(error))
                                return 0;

                            wc = new WhereCustom(new List<WhereCustom>()
                                {
                                    new WhereCustom(PJField.FILE.GetHashCode().ToString(), Operator.OP_EQUAL, file),
                                    new WhereCustom(PJField.FILEID.GetHashCode().ToString(), Operator.OP_EQUAL, parentFid.ToString())

                                });
                        }
                        finally
                        {
                            eDal.CloseDatabase();
                        }
                    }
                    else
                    {
                        if (parentTab == (int)TableType.PP || parentTab == (int)TableType.PM || parentTab == (int)TableType.ADR)
                        {
                            listCol = String.Concat(TableType.PP.GetHashCode() + 1, ";", TableType.ADR.GetHashCode() + 1, ";", TableType.PM.GetHashCode() + 1);
                            //tab = TableType.ADR.GetHashCode();
                        }

                        eq.SetParentDescid = parentTab;
                        eq.SetParentFileId = parentFid;
                    }
                }




                if (filterId > 0)
                    eq.SetFilterId = filterId;

                if (eq.GetError.Length != 0)
                    throw new Exception("eIndicatorXrmWidgetUI.GetCount - EudoQuery.Init pour " + eq.GetError);


                eq.SetListCol = listCol;

                if (wc != null)
                    eq.AddCustomFilter(wc);

                eq.LoadRequest();
                if (eq.GetError.Length != 0)
                    throw new Exception("eIndicatorXrmWidgetUI.GetCount  - EudoQuery.LoadRequest pour " + eq.GetError);

                eq.BuildRequest();
                if (eq.GetError.Length != 0)
                    throw new Exception("eIndicatorXrmWidgetUI.GetCount  - EudoQuery.BuildRequest pour " + eq.GetError);

                result = ExecuteCountQuery(eq.EqCountQuery);
            }
            finally
            {
                if (eq != null)
                    eq.CloseQuery();
            }

            return result;
        }


        /// <summary>
        /// Exécute la requête pour retourner un "int"
        /// </summary>
        /// <param name="countQuery">Requête de comptage</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private int ExecuteCountQuery(string countQuery)
        {
            int indicateur = 0;
            string error = string.Empty;

            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);
            try
            {
                eDal.OpenDatabase();
                RqParam rq = new RqParam(countQuery);
                indicateur = eDal.ExecuteScalar<int>(rq, out error);

                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                eDal.CloseDatabase();
            }

            return indicateur;
        }
    }

    static class indicatorCalculation
    {
        public static decimal CalculateIndicator(List<decimal> source, string operateur)
        {
            decimal res = 0;

            if (source.Count > 0)
            {
                switch (operateur)
                {
                    case "NB":
                        res = source.Count;
                        break;
                    case "SUM":
                        res = source.Sum();
                        break;
                    case "AVG":
                        res = (decimal)source.Average();
                        break;
                    case "MED":
                        res = getMedian(source);
                        break;
                    case "MIN":
                        res = source.Min();
                        break;
                    case "MAX":
                        res = source.Max();
                        break;
                    default:
                        res = 0;
                        break;
                }
            }

            return res;
        }

        /// <summary>
        /// Gets the median.
        /// </summary>
        /// <param name="source">Liste des valeurs décimales</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Empty collection</exception>
        private static decimal getMedian(List<decimal> source)
        {
            // Create a copy of the input, and sort the copy
            decimal[] temp = source.ToArray();
            Array.Sort(temp);

            int count = temp.Length;
            if (count == 0)
            {
                throw new InvalidOperationException("Empty collection");
            }
            else if (count % 2 == 0)
            {
                // count is even, average two middle elements
                decimal a = temp[count / 2 - 1];
                decimal b = temp[count / 2];
                return (a + b) / 2m;
            }
            else
            {
                // count is odd, return the middle element
                return temp[count / 2];
            }
        }



    }
}
