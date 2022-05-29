using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour les charts
    /// </summary>
    public class eChartRenderer : eReportWizardRenderer
    {

        /// <summary>
        /// Object Chart
        /// </summary>
        protected eCommunChart _eC;

        /// <summary>
        /// Object Chart
        /// </summary>
        protected eCommunChart _eCCombined;

        private XmlDocument _xmlRoot = new XmlDocument();

        private XmlElement _xmlBase;
        private Boolean _bIsFileMode = false;

        /// <summary>
        /// Objet XML des data du chart
        /// </summary>
        public XmlDocument XMLData
        {
            get { return _xmlRoot; }
            protected set { _xmlRoot = value; }
        }

        /// <summary>
        /// Object eChart - conteneur de données
        /// </summary>
        public eCommunChart CombinedChart
        {
            get
            {
                return _eCCombined;
            }

            set { _eCCombined = value; }
        }


        /// <summary>
        /// Object eChart - conteneur de données
        /// </summary>
        public eCommunChart Chart
        {
            get
            {
                if (_eCCombined != null)
                    return _eCCombined;
                else
                    return _eC;
            }
        }

        /// <summary>
        /// Accès au paramètre du rapport
        /// </summary>
        public eReport ChartReport
        {
            get { return _eC.ChartReport; }

        }

        /// <summary>
        /// Accès au paramètre des filtres express du chart
        /// </summary>
        public List<eChartExpressFilter> ChartExpressFilter
        {
            get { return _eC.ChartExpressFilter; }

        }


        /// <summary>
        /// Champ de l'axe des X 
        /// </summary>
        Field _fldX;

        /// <summary>
        /// Champ de l'axe des Y 
        /// </summary>
        Field _fldY;

        /// <summary>
        /// Champ(s) de l'axe Y - Valeurs
        /// Contient plusieurs champs en cas de séries multiple avec champ de valeurs multiple
        /// 1 à n champ
        /// </summary>
        private List<Field> _lstFldY = new List<Field>();
        private List<eModelConst.CHART_SERIES_OPERATION> _lstFldOp = new List<eModelConst.CHART_SERIES_OPERATION>();
        private List<eModelConst.CHART_SERIES_OPERATION> _lstExpressFilter = new List<eModelConst.CHART_SERIES_OPERATION>();
        private List<Int32> _lstFileY = new List<Int32>();
        /// <summary>
        /// Champ de séries pour le cas de séries multiple par regroupement
        /// </summary>
        Field _fldGroup = null;


        /// <summary>
        /// Retourne un renderer pour le flux XML d'un chart
        /// </summary>
        /// <param name="pref">Préférence</param>
        /// <param name="nReportId">Id du rapport</param>
        /// <param name="nFileId">Id de la fiche pour les charts en mode fiche</param>
        /// <param name="sExpressFilterParam">Param de filtre express</param>
        /// <param name="context">Contexte du widget</param>
        /// <returns></returns>
        public static eChartRenderer getChartRendererXML(ePref pref, Int32 nReportId, Int32 nFileId = 0, string sExpressFilterParam = "", eXrmWidgetContext context = null)
        {

            //Construction et initialisation

            eChartRenderer ecRenderer = new eChartRenderer(pref);
            ecRenderer._eC = eCharts.BuildChart(pref, nReportId, nFileId, sExpressFilterParam, context);
            ecRenderer._bIsFileMode = nFileId > 0;


            //Erreur sur le chart
            if (ecRenderer._eC != null && ecRenderer._eC.ErrorMsg.Length > 0)
            {

                ecRenderer._sErrorMsg = ecRenderer._eC.ErrorMsg;
                if (ecRenderer._eC.InnerException != null)
                    ecRenderer._eException = ecRenderer._eC.InnerException;
                return ecRenderer;
            }

            return ecRenderer;
        }


        /// <summary>
        /// Retourne le conteneur d'un report graphique
        /// Si aucune largeur/hauteur n'est définie en param, on prend les paramètres du report, sinon 800x600
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="sReportId"> entier The n report identifier.</param>
        /// <param name="bFullSize">if set to <c>true</c> [b full size].</param>
        /// <param name="nWidth">Forcing de la largeur</param>
        /// <param name="nHeight">Forcing de la hauteur</param>
        /// <param name="bFromHomepage">if set to <c>true</c> [b from homepage].</param>
        /// <returns></returns>

        public static eChartRenderer getChartRenderer(ePref pref, Int32 sReportId, Boolean bFullSize = false, int nWidth = 0, int nHeight = 0, Boolean bFromHomepage = false)
        {
            return GetChartRenderer(pref, sReportId.ToString(), bFullSize, nWidth, nHeight, bFromHomepage);
        }


        /// <summary>
        /// Retourne le conteneur d'un report graphique
        /// Si aucune largeur/hauteur n'est définie en param, on prend les paramètres du report, sinon 800x600
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="erChartInfo"> rapport</param>
        /// <param name="sReportId">The n report identifier.</param>
        /// <param name="bFullSize">if set to <c>true</c> [b full size].</param>
        /// <param name="nWidth">Forcing de la largeur</param>
        /// <param name="nHeight">Forcing de la hauteur</param>
        /// <param name="bFromHomepage">Pour un widget sur une grille</param>
        /// <returns></returns>
        public static eChartRenderer GetChartRenderer(ePref pref, eReport erChartInfo, string sReportId, Boolean bFullSize = false, int nWidth = 0, int nHeight = 0, Boolean bFromHomepage = false)
        {
            eChartRenderer ecRenderer = new eChartRenderer(pref);

            if (erChartInfo == null)
                ecRenderer.SetError(QueryErrorType.ERROR_NUM_DEFAULT, "Erreur de récupération du rapport: rapport null.");

            try
            {
                String datas = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.CUSTOM, pref.GetBaseName));
                String sLogo = erChartInfo.GetParamValue("logo").ToLower();
                Boolean bLogo = sLogo.Length > 0 && sLogo != "nologo";

                //Div Conteneur
                HtmlGenericControl divChart = new HtmlGenericControl("div");
                divChart.ID = String.Concat("chart_", erChartInfo.Id);
                divChart.Attributes.Add("class", "divChart");
                ecRenderer._pgContainer.Controls.Add(divChart);

                //Paramètre du chart à générer
                HtmlGenericControl divHidden = new HtmlGenericControl("div");
                divHidden.Style.Add("visibility", "hidden");
                divHidden.Style.Add("display", "none");
                divHidden.Attributes.Add("ednChartParam", sReportId);
                ecRenderer._pgContainer.Controls.Add(divHidden);

                HtmlInputHidden inptChartParam = GetImputHiddenParams(pref, divHidden, erChartInfo, sReportId, bFullSize, nWidth, nHeight, bFromHomepage);

                if (bLogo)
                    inptChartParam.Attributes.Add("logo", String.Concat(datas, "/", sLogo));
            }
            catch (Exception e)
            {
                ecRenderer.SetError(QueryErrorType.ERROR_NUM_DEFAULT, e.Message, e);
            }

            return ecRenderer;
        }

        /// <summary>
        /// Retourne le conteneur d'un report graphique
        /// Si aucune largeur/hauteur n'est définie en param, on prend les paramètres du report, sinon 800x600
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="divHidden">container</param>
        /// <param name="erChartInfo">rapport</param>
        /// <param name="sReportId">The n report identifier.</param>
        /// <param name="bFullSize">if set to <c>true</c> [b full size].</param>
        /// <param name="nWidth">Forcing de la largeur</param>
        /// <param name="nHeight">Forcing de la hauteur</param>
        /// <param name="bFromHomepage">Pour un widget sur une grille</param>
        /// <returns></returns>
        public static HtmlInputHidden GetImputHiddenParams(ePref pref, HtmlGenericControl divHidden, eReport erChartInfo, string sReportId, Boolean bFullSize = false, int nWidth = 0, int nHeight = 0, Boolean bFromHomepage = false)
        {
            String chartName;
            Int32 nSerieType = 0;
            string filterDescription = string.Empty;
            string filterName = string.Empty;
            String error = string.Empty;
            Exception _innerException = new Exception();

            if (!GetChartName(erChartInfo.GetParamValue("typechart").Split("|"), out chartName, out error, out _innerException))
                throw new Exception(error, _innerException);

            //if (bFromHomepage)
            //{
            if (erChartInfo.Filter != null)
                filterDescription = GetChartFilterDescription(pref, erChartInfo);
            else
                filterDescription = eResApp.GetRes(pref, 1925);
            //}

            HtmlInputHidden inptChartParam = new HtmlInputHidden();
            inptChartParam.ID = "divChartParams_" + sReportId;
            inptChartParam.Style.Add("visibility", "hidden");
            inptChartParam.Style.Add("display", "none");
            inptChartParam.Attributes.Add("ednchartparam", sReportId);
            inptChartParam.Attributes.Add("ednreportparam", erChartInfo.Id.ToString());

            divHidden.Controls.Add(inptChartParam);

            #region Largeur/hauteur
            if (nWidth > -1 && nHeight > -1)
            {
                String s = erChartInfo.GetParamValue("w");
                int chartWidth = (nWidth > 0) ? nWidth : ((s.Length > 0) ? eLibTools.GetNum(s) : 800);
                s = erChartInfo.GetParamValue("h");
                int chartHeight = (nHeight > 0) ? nHeight : ((s.Length > 0) ? eLibTools.GetNum(s) : 600);
                inptChartParam.Attributes.Add("w", chartWidth.ToString());
                inptChartParam.Attributes.Add("h", chartHeight.ToString());
            }

            #endregion

            inptChartParam.Attributes.Add("fs", bFullSize ? "1" : "0");
            inptChartParam.Attributes.Add("id", sReportId);
            inptChartParam.Attributes.Add("tab", erChartInfo.Tab.ToString());
            inptChartParam.Attributes.Add("chart", chartName);
            inptChartParam.Attributes.Add("displaygrid", (!bFromHomepage) ? erChartInfo.GetParamValue("DisplayGrid") : "0");
            inptChartParam.Attributes.Add("displaygridnb", erChartInfo.GetParamValue("DisplayGridnb"));
            inptChartParam.Attributes.Add("filterid", erChartInfo.GetParamValue("filterid"));
            inptChartParam.Attributes.Add("addcurrentfilter", erChartInfo.GetParamValue("addcurrentfilter"));
            inptChartParam.Attributes.Add("data-filterdescription", filterDescription);

            if (Int32.TryParse(erChartInfo.GetParamValue("seriestype"), out nSerieType))
            {
                inptChartParam.Attributes.Add("hexcel", nSerieType == (int)eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE || nSerieType == (int)eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_COMBINED ? "0" : "1");

                if (nSerieType == (int)eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_COMBINED)
                {
                    inptChartParam.Attributes.Add("combinedyfilterid", erChartInfo.GetParamValue("combinedyfilterid"));
                    inptChartParam.Attributes.Add("combinedzfilterid", erChartInfo.GetParamValue("combinedzfilterid"));
                    inptChartParam.Attributes.Add("combinedztab", erChartInfo.GetParamValue("combinedzetiquettesfile"));
                }

            }

            return inptChartParam;
        }

        /// <summary>
        /// Gets the chart filter description.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="chartInfos">Données du graphique</param>
        /// <returns></returns>
        public static string GetChartFilterDescription(ePref pref, eReport chartInfos)
        {
            string filterName = string.Empty;
            string filterDescription = string.Empty;
            string error = string.Empty;

            filterDescription = AdvFilter.GetDescription(pref, eLibTools.GetNum(chartInfos.GetParamValue("filterid")), out filterName, out error);
            if (chartInfos.GetParamValue("ADDCURRENTFILTER") == "1")
            {
                filterDescription += $"<b>{eResApp.GetRes(pref, 1922)}</b>";
            }

            return filterDescription;

        }

        /// <summary>
        /// Retourne le conteneur d'un report graphique
        /// Si aucune largeur/hauteur n'est définie en param, on prend les paramètres du report, sinon 800x600
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="sReportId">The n report identifier.</param>
        /// <param name="bFullSize">if set to <c>true</c> [b full size].</param>
        /// <param name="nWidth">Forcing de la largeur</param>
        /// <param name="nHeight">Forcing de la hauteur</param>
        /// <param name="bFromHomepage">Pour un widget sur une grille</param>
        /// <returns></returns>
        public static eChartRenderer GetChartRenderer(ePref pref, string sReportId, Boolean bFullSize = false, int nWidth = 0, int nHeight = 0, Boolean bFromHomepage = false)
        {
            String error = string.Empty;
            eReport erChartInfo = null;
            eChartRenderer ecRenderer = new eChartRenderer(pref);

            try
            {
                Int32 nReportId = 0;
                string[] reportIdTab = sReportId.Split("_");
                Int32.TryParse(reportIdTab[0], out nReportId);

                erChartInfo = new eReport(pref, nReportId);
                erChartInfo.LoadFromDB();
            }
            catch (Exception e)
            {
                ecRenderer.SetError(QueryErrorType.ERROR_NUM_DEFAULT, e.Message, e);
            }

            return GetChartRenderer(pref, erChartInfo, sReportId, bFullSize, nWidth, nHeight, bFromHomepage);
        }


        /// <summary>
        /// Retourne le conteneur d'un report graphique
        /// Si aucune largeur/hauteur n'est définie en param, on prend les paramètres du report, sinon 800x600
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nReportId">The n report identifier.</param>
        /// <param name="bFullSize">if set to <c>true</c> [b full size].</param>
        /// <param name="nWidth">Forcing de la largeur</param>
        /// <param name="nHeight">Forcing de la hauteur</param>
        /// <param name="bFromHomepage">Est-on dans une grille ?</param>
        /// <returns></returns>
        public static eChartRenderer GetExpressFilterChartRenderer(ePref pref, Int32 nReportId, Boolean bFullSize = false, int nWidth = 0, int nHeight = 0, Boolean bFromHomepage = false)
        {
            #region paramètres internes à la méthode
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            eChartRenderer ecRenderer = new eChartRenderer(pref);
            Int32 nbHideValidateButton = 0;
            Int32 nbSpecialOperator = 0;
            Int32 nbValideFilter = 0;
            #endregion
            try
            {
                #region open Dal
                dal?.OpenDatabase();
                #endregion

                #region Construction rapport type chart
                ecRenderer._eC = eChartsEmpty.CreateChart(pref, nReportId);
                eReport erChartInfo = ecRenderer.ChartReport;
                List<eChartExpressFilter> eFilter = ecRenderer.ChartExpressFilter;
                #endregion

                #region filtres Express
                if (eFilter.Count > 0)
                {
                    HtmlGenericControl divGlobalFiltreExpress = new HtmlGenericControl("div");
                    divGlobalFiltreExpress.Attributes.Add("class", "divGlobalFiltreExpress");
                    HtmlGenericControl divFiltreExpress;
                    for (int index = 0; index < eFilter.Count; index++)
                    {
                        if (eFilter[index].EfField != null && eFilter[index].Op != Operator.OP_UNDEFINED)
                        {
                            divFiltreExpress = getExpressFilterLine(pref, dal, nReportId, index, eFilter[index]);
                            if (divFiltreExpress != null)
                                divGlobalFiltreExpress.Controls.Add(divFiltreExpress);
                            else
                                nbHideValidateButton++;
                        }
                        else
                            nbHideValidateButton++;

                        if (eFilter[index].Op != Operator.OP_UNDEFINED)
                            nbValideFilter++;

                    }

                    nbSpecialOperator = eFilter.Where(p => p.EfIsSpecialOperator).Count();

                    if (nbHideValidateButton != eFilter.Count)
                    {
                        ecRenderer._pgContainer.Controls.Add(divGlobalFiltreExpress);

                        if (nbSpecialOperator != nbValideFilter)
                        {
                            HtmlGenericControl divButton = new HtmlGenericControl("div");
                            divButton.Attributes.Add("class", "expressFilterButton");
                            divButton.Controls.Add(getButtonvalidationFilter(pref, nReportId));
                            ecRenderer._pgContainer.Controls.Add(divButton);
                        }


                    }
                }


                #endregion
                return ecRenderer;
            }
            catch (Exception ex)
            {

                ecRenderer._eException = ex;
                ecRenderer._sErrorMsg = "Impossible de charger la liste des filtres express";
                return null;
            }
            finally
            {
                dal?.CloseDatabase();

            }


        }


        /// <summary>
        /// Retourne le nom du graphique
        /// </summary>
        /// <param name="lTypeChart">Type du graphique/Série </param>
        /// <param name="chartName">Nom du graphique </param>
        /// <param name="error">Erreur à retourner </param>
        /// <param name="_innerException">Exception de retour</param>
        /// <returns></returns>
        public static Boolean GetChartName(string[] lTypeChart, out string chartName, out string error, out Exception _innerException)
        {
            chartName = string.Empty;
            error = string.Empty;
            string xmlChartFile = "Charts.xml";
            _innerException = new Exception();
            try
            {

                //chargement de la liste des charts disponible
                XmlDocument xmlListChart = new XmlDocument();
                xmlListChart.Load(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Charts", xmlChartFile));
                XmlNode xChart = xmlListChart.SelectSingleNode("//series/serie[@id=" + lTypeChart[0] + "]/charts/chart[@id=" + lTypeChart[1] + "]");
                chartName = xChart.Attributes["name"].Value;

            }
            catch (System.IO.FileNotFoundException ex)
            {
                _innerException = ex;
                error = string.Concat("Le ficher ", xmlChartFile, " est introuvable ou inaccessible à l'ouverture.");
                return false;

            }
            catch (XmlException ex)
            {
                _innerException = ex;
                error = string.Concat("Il existe une erreur de chargement ou d’analyse dans le fichier XML ", xmlChartFile);
                return false;

            }

            catch (Exception e)
            {
                _innerException = e;
                error = string.Concat("Erreur lors de la récupération du type de graphique, le type", lTypeChart[1], " n'existe pas la série ", lTypeChart[0]);
                return false;
            }


            return true;
        }


        /// <summary>
        /// Constructeur privée
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">DescId de la table</param>
        public eChartRenderer(ePref pref, Int32 tab) : base(pref, tab)
        {

        }


        /// <summary>
        /// Constructeur privée
        /// </summary>
        /// <param name="pref"></param>
        public eChartRenderer(ePref pref) : base(pref)
        {

        }

        /// <summary>
        /// Construit la structure HTML de l'élément
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            return true;
        }

        /// <summary>
        /// Initialisation des objets nécessaire
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {

            try
            {

                //Préfix propre au graphique type combiné
                string prefix = string.Empty;
                string chartPrefix = string.Empty;
                string sLangConnexion = GetCultureLogin();
                bool bEmptyGraph = true;
                string sBk = "bk";
                bool bBk = false;
                string sEmptyGraph = "emptyGraph";
                Int32 dec = 0;
                _fldY = null;

                string sTypeChart = Chart.ChartReport.GetParamValue("TypeChart");
                List<string> lTypeChart = sTypeChart.Split("|").ToList<string>();

                //recherche du type de chart
                if (!sTypeChart.Contains("|"))
                {
                    _sErrorMsg = String.Concat("Type de chart invalide : ", sTypeChart);
                    return false;
                }

                if (lTypeChart.Count != 2)
                {
                    _sErrorMsg = String.Concat("Type de chart invalide : ", sTypeChart);
                    return false;
                }

                //Si graphique type jauge et qu'on a définie une valeur fixe, on arrete
                if (Chart.BFixedGaugeValueType && Chart.BcombinedChartReport && Chart.FldFieldsInfos == null)
                    return true;

                /*  Champs impliqué dans le graphique */



                SYNCFUSIONCHART typeChart = SYNCFUSIONCHART.NONE;
                Enum.TryParse(lTypeChart[1], out typeChart);
                //Séries
                eModelConst.CHART_SERIES_TYPE ecSeriesType = (eModelConst.CHART_SERIES_TYPE)eLibTools.GetNum(ChartReport.GetParamValue("SeriesType"));

                //Dispartité entre le type de série choisie et le chart.swf choisie
                if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SINGLE).ToString()
                    || lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString())
                    ecSeriesType = eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE;
                else if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.MULTI).ToString() || lTypeChart[0] == ((int)eModelConst.CHART_TYPE.STACKED).ToString())
                {
                    if (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE)
                    {
                        _sErrorMsg = String.Concat("Type de chart invalide 'eCOnst.CHART_TYPE' : ", sTypeChart, " -  'eModelConst.CHART_SERIES_TYPE' : ", ecSeriesType);
                        return false;
                    }
                }

                if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString())
                {
                    if (CombinedChart != null)
                    {
                        _lstFldY = new List<Field>();
                        _lstFileY = new List<int>();
                        _lstFldOp = new List<eModelConst.CHART_SERIES_OPERATION>();
                        prefix = eLibConst.COMBINED_Z;
                        chartPrefix = eLibConst.COMBINED_Z;
                    }

                    else
                        prefix = eLibConst.COMBINED_Y;
                }

                //Liste des champs pour les valeurs du graphiques (AXE Y )
                String sValueFile = ChartReport.GetParamValue(string.Concat(prefix, "ValuesFile"));
                String sValueFields = ChartReport.GetParamValue(string.Concat(prefix, "ValuesField"));
                String sValueOperations = ChartReport.GetParamValue(string.Concat(prefix, "ValuesOperation"));

                //Liste des champs de label (AXE X)

                //Descid du champ des X
                String sFieldX = ChartReport.GetParamValue(string.Concat(prefix, "EtiquettesField"));

                if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString()
                    && lTypeChart[1] == ((int)CHART_SPECIAL_TYPE.CIRCULAR_GAUGE_CHART).ToString()
                    && prefix == eLibConst.COMBINED_Y)
                    sFieldX = ChartReport.GetParamValue(string.Concat(eLibConst.COMBINED_Y, "ValuesField"));

                //Regroupement (option des ) de regroupement pour le champ des X (par exemple par trimestre, par mois ect...)
                String sGroupX = ChartReport.GetParamValue(string.Concat(prefix, "EtiquettesGroup"));


                if (ecSeriesType != eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE && ecSeriesType != eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_COMBINED)
                    ChartReport.SetParamValue("DisplayGrid", "0");

                //Champ de série par regroupement
                String sSeriesField = ChartReport.GetParamValue("SeriesField");

                //Pour les séries par regroupement, indique si le champ de regroupement est lié a celui de série
                Boolean bIsBoundFIeld = false;

                /*  Récupération des champs impliqués dans le charts */
                if (Chart.FldFieldsInfos != null)
                {
                    _fldX = Chart.FldFieldsInfos.Find(delegate (Field f) { return f.Descid.ToString() == sFieldX; });

                    if (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE)
                        _fldY = Chart.FldFieldsInfos.Find(delegate (Field f) { return f.Descid.ToString() == sValueFields; });

                    switch (ecSeriesType)
                    {
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE:

                            // Valeur et operation sont unique
                            if (!AddValueFieldValue(sValueFile, sValueFields, sValueOperations))
                                return false;

                            break;
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP:
                            // regroupement des valeurs par un autre champs

                            // Valeur et operation sont unique - Le multi série est géré par un 3é champ
                            if (!AddValueFieldValue(sValueFile, sValueFields, sValueOperations))
                                return false;

                            _fldGroup = Chart.FldFieldsInfos.Find(delegate (Field f) { return f.Descid.ToString() == sSeriesField; });

                            if (_fldGroup == null)
                            {
                                _sErrorMsg = "Champ de regroupement non trouvé";
                                return false;
                            }

                            //Si le champ de regroupement est lié au champ de séries
                            bIsBoundFIeld = (_fldX.Descid == _fldGroup.BoundDescid);

                            break;
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_VALUE:
                            // Plusieurs champ de valeurs
                            List<String> lstValue = sValueFields.Split(";").ToList<String>();
                            List<String> lstOperation = sValueOperations.Split(";").ToList<String>();

                            if (lstValue.Count != lstOperation.Count)
                            {
                                _sErrorMsg = "Nombre de champ != nombre opération";
                                return false;
                            }

                            for (Int32 nCmpt = 0; nCmpt < lstValue.Count; nCmpt++)
                            {
                                if (!AddValueFieldValue(sValueFile, lstValue[nCmpt], lstOperation[nCmpt]))
                                    return false;
                            }

                            break;
                        default:
                            _sErrorMsg = "TYPE DE SERIES NON SUPPORTEE";
                            return false;

                    }
                }



                //Parcours de enregistrement


                //Liste des séries dans le graphiques
                IDictionary<String, SeriesElement> lstS = GetSeriesElement(Chart.ListRecords, ecSeriesType, sGroupX);
                // Dictionary<String, SeriesElement> lstCombinedS = GetSeriesElement(CombinedChart.ListRecords, ecSeriesType, sGroupX);


                //Paramètre du chart

                /*  Option du graphique  */

                // Affichage du libellé des label (diagonal, vertical,...)
                Int32 nDisplayXType = eLibTools.GetNum(ChartReport.GetParamValue("lstdisplayx") == "" ? "1" : ChartReport.GetParamValue("lstdisplayx"));

                // Masque la légende
                Boolean bHideLegend = (ChartReport.GetParamValue("displaylegend") == "0");

                // Positionner la légende
                string legendPosition = ChartReport.GetParamValue("lstdisplaylegend");

                // Masque l'etiquette
                Boolean bHideEtiquette = (ChartReport.GetParamValue("displayx") == "0");

                // Masque le titre
                Boolean bHideTitle = (ChartReport.GetParamValue("HideTitle") == "1");

                // Title
                String sTitle = ChartReport.GetParamValue("Title").Replace("##AMPINTITLE##", "&");

                // Sort 'seulement pour série simple
                Boolean bSort = (ChartReport.GetParamValue("SortEnabled") == "1" && ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE && typeChart != SYNCFUSIONCHART.FUNNEL && typeChart != SYNCFUSIONCHART.PYRAMID);

                Boolean bCatXSortAcending = (ChartReport.GetParamValue("EtiquettesTri").ToLower() == "asc");
                //&& ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE && typeChart == SYNCFUSIONCHART.FUNNEL);

                // Sort Order
                String sSortOrder = ChartReport.GetParamValue("SortOrder");

                //Valeurs affichées
                Boolean bDisplayValues = (ChartReport.GetParamValue("DisplayValues") == "1");

                //Valeurs affichées en pourcent
                Boolean bDisplayValuesPercent = (ChartReport.GetParamValue("DisplayValuesPercent") == "1");

                //Serie empilées en pourcentage
                Boolean bDisplayStackPercent = (ChartReport.GetParamValue("DisplayStackedPercent") == "1");

                //chargement de la liste des charts disponible
                XmlDocument xmlListChart = new XmlDocument();
                try
                {
                    xmlListChart.Load(HttpContext.Current.Server.MapPath((@"..\charts\charts.xml")));
                }
                catch (Exception e)
                {
                    _eException = e;
                    _sErrorMsg = "Impossible de charger la liste des charts";
                    return false;
                }

                XmlNode xChart = xmlListChart.SelectSingleNode("//series/serie[@id=" + lTypeChart[0] + "]/charts/chart[@id=" + lTypeChart[1] + "]");
                if (xChart == null)
                {
                    _sErrorMsg = "Type de chart introuvable";
                    return false;
                }

                String sSerieName = xChart.Attributes["name"].Value;

                if (
                    (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE && !Chart.GetCombinedChart)
                    || (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE && Chart.BFixedGaugeValueType)
                    || ecSeriesType != eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE

                    )
                {
                    // génération du XML
                    XmlNode mainNodeDeclaration = _xmlRoot.CreateXmlDeclaration("1.0", "UTF-8", null);
                    _xmlRoot.AppendChild(mainNodeDeclaration);

                    //Racine du xml
                    this._xmlBase = _xmlRoot.CreateElement("chart");
                    _xmlRoot.AppendChild(_xmlBase);

                }
                //Vérifi si on vient des stats ou pas
                if (Chart.typeChart == eCommunChart.TypeChart.STATCHART)
                    AddAttribute(_xmlBase, "fromStat", "1");

                //Type graphique
                if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString() && lTypeChart[1] == ((int)CHART_SPECIAL_TYPE.CIRCULAR_GAUGE_CHART).ToString())
                    AddAttribute(_xmlBase, "typeGraph", ((int)(eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_COMBINED)).ToString());
                else
                    AddAttribute(_xmlBase, "typeGraph", ((int)(ecSeriesType)).ToString());

                //serie name
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "sSerieName"), sSerieName);

                if (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE)
                {
                    if (_fldX != null)
                        AddAttribute(_xmlBase, string.Concat(chartPrefix, "fldX"), _fldX.Descid.ToString());
                    if (_fldY != null)
                    {
                        AddAttribute(_xmlBase, string.Concat(chartPrefix, "fldY"), _fldY != null ? _fldY.Descid.ToString() : _fldX.Descid.ToString());

                        AddAttribute(_xmlBase, string.Concat(chartPrefix, "tabfldY"), _fldY != null ? _fldY.Table.Libelle : _fldX.Table.Libelle);

                    }

                    AddAttribute(_xmlBase, string.Concat(chartPrefix, "sValueOperations"), sValueOperations?.ToUpper());
                }

                //Récuperer le libéllé pour l'ajouter dans le tableau
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "libelleRubriqueStat"), _fldX.Libelle);

                //Récuperer le descid de la table pour l'ajouter dans le tableau
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "idTableStat"), _fldX.Table.DescId.ToString());

                //Couluer des titres et valeurs
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "xrmThemeColor"), Pref.ThemeXRM.Color);

                //Utiliser la couleur du theme
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "useThemeColor"), ChartReport.GetParamValue("usethemecolor"));

                //Etiquettes
                AddAttribute(_xmlBase, "showEtiquette", bHideEtiquette ? "0" : "1");

                //Légende
                AddAttribute(_xmlBase, "showLegend", bHideLegend ? "0" : "1");

                //Légende position
                AddAttribute(_xmlBase, "legendPosition", legendPosition);

                //Paramètres spécifiques aux graphiques type jauge circulaire
                if (lTypeChart[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString() && lTypeChart[1] == ((int)CHART_SPECIAL_TYPE.CIRCULAR_GAUGE_CHART).ToString())
                {
                    string cgvfixedvalue = ChartReport.GetParamValue("cgvfixedvalue");
                    int fixedValue = 1;
                    if (int.TryParse(cgvfixedvalue, out fixedValue) && fixedValue == 0)
                        fixedValue = 1;

                    AddAttribute(_xmlBase, "cgintervals", ChartReport.GetParamValue("cgintervals"));
                    AddAttribute(_xmlBase, "cgvtype", ChartReport.GetParamValue("cgvtype"));
                    AddAttribute(_xmlBase, "cgvfixedvalue", fixedValue.ToString());
                }


                #region Orientation du texte

                switch (nDisplayXType)
                {
                    case 0:
                        AddAttribute(_xmlBase, "labelRotation", "30");
                        AddAttribute(_xmlBase, "Rotate", "Stagger");
                        AddAttribute(_xmlBase, "slantLabels", "1");
                        break;
                    case 1:
                        AddAttribute(_xmlBase, "labelRotation", "0");
                        break;
                    case 2:
                        AddAttribute(_xmlBase, "labelRotation", "-90");
                        AddAttribute(_xmlBase, "labelDisplay", "Rotate");
                        break;
                    case 3:
                        AddAttribute(_xmlBase, "labelDisplay", "Stagger");
                        break;
                    default:
                        AddAttribute(_xmlBase, "Rotate", "Stagger");
                        AddAttribute(_xmlBase, "slantLabels", "1");
                        AddAttribute(_xmlBase, "labelRotation", "0");
                        break;
                }

                #endregion

                if (bSort)
                    AddAttribute(_xmlBase, "typeSort", sSortOrder.ToLower().Equals("asc") ? "1" : "0");
                else
                    AddAttribute(_xmlBase, "typeSort", "0");

                //Titre
                if (!bHideTitle && sTitle.Length > 0)
                    AddAttribute(_xmlBase, "caption", sTitle);

                //Label X
                AddAttribute(_xmlBase, string.Concat(chartPrefix, "xAxisName"), _fldX.Libelle);

                if (ecSeriesType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE)
                {
                    //Label Y
                    AddAttribute(_xmlBase, string.Concat(chartPrefix, "yAxisName"), SeriesElement.GetValueLabel(ChartReport.Tab, ecSeriesType, _lstFldOp[0], _lstFileY[0], _lstFldY[0], Pref));
                    // Gestion décimale

                    if (!sValueOperations.ToLower().Equals(eModelConst.CHART_SERIES_OPERATION.COUNT.ToString().ToLower()))
                        dec = _fldY == null ? _fldX.Length : _fldY.Length;

                    AddAttribute(_xmlBase, string.Concat(chartPrefix, "decimals"), dec.ToString());
                }

                AddAttribute(_xmlBase, "showValues", bDisplayValues ? "1" : "0");
                //AddAttribute(xmlBase, "showPercentageValues", bDisplayValuesPercent ? "1" : "0");
                AddAttribute(_xmlBase, "showPercentValues", bDisplayValuesPercent ? "1" : "0");
                AddAttribute(_xmlBase, "stack100Percent", bDisplayStackPercent ? "1" : "0");

                //Export
                Boolean bExport = true;
                String sExportId = String.Empty;
                if (bExport && sExportId.Length > 0)
                {
                    AddAttribute(_xmlBase, "exportHandler", sExportId);      //Id du div html qui contiendra le lien d'export
                    AddAttribute(_xmlBase, "exportEnabled", "1");
                    AddAttribute(_xmlBase, "showExportDataMenuItem", "1");
                    AddAttribute(_xmlBase, "exportDialogMessage", eResApp.GetRes(Pref, 1120)); // message d'attente
                }


                //GCH - Internationalisation des numériques - #36869, #37686 - Graphiques
                AddAttribute(_xmlBase, "decimalSeparator", Pref.NumberDecimalDelimiter);
                AddAttribute(_xmlBase, "thousandSeparator", Pref.NumberSectionsDelimiter);



                String sDrillDownLink = String.Empty;
                String sAddedDrill = String.Empty;
                String sSepRaport = "$##$";
                String sSepAliasValue = "$#$";
                String sSepFldFilter = "$||$";
                String sSepFldOperator = "$|#|$";
                String sSepCombinedTab = "$**$";

                if (sGroupX.Length > 0 && _fldX.Format == FieldFormat.TYP_DATE)
                {
                    if (sGroupX == "month")
                        sAddedDrill = "<NODAY>";
                    else if (sGroupX == "year")
                        sAddedDrill = "<YEARGRAPH>";
                    else
                        sAddedDrill = String.Concat("<", sGroupX.ToUpper(), ">");
                }

                //Gestion des séries 

                switch (ecSeriesType)
                {
                    #region SERIES_TYPE_SIMPLE

                    case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE:

                        ChartParams paramsChart = new ChartParams()
                        {
                            bSort = bSort,
                            nSortOrder = sSortOrder.ToLower().Equals("asc"),
                            bEtiquettesTriCatXSortAcending = bCatXSortAcending,
                            sAddedDrill = sAddedDrill,
                            sDrillDownLink = sDrillDownLink,
                            sSepAliasValue = sSepAliasValue,
                            sSepFldFilter = sSepFldFilter,
                            sSepFldOperator = sSepFldOperator,
                            sSepCombinedTab = sSepCombinedTab,
                            sSepRaport = sSepRaport,
                            sTypeChart = typeChart,
                            sValueFields = sValueFields,
                            sValueFile = sValueFile,
                            sValueOperations = sValueOperations,
                            sPrefix = prefix,
                            bDisplayValuesPercent = bDisplayValuesPercent,
                            bDisplayStackPercent = bDisplayStackPercent,
                            nDecimal = dec > 2 ? dec : 2,
                            sLangConnexion = sLangConnexion
                        };



                        if (paramsChart.sPrefix == eLibConst.COMBINED_Z || paramsChart.sPrefix == eLibConst.COMBINED_Y)
                        {
                            IDictionary<String, SeriesElement> lstCombined = lstS.OrderBy(w => w.Value.GetCategorieDisplayLabel).ToDictionary(x => x.Key, x => x.Value);
                            lstS = lstCombined;
                            AddAttribute(_xmlBase, "displayzaxe", ChartReport.GetParamValue("displayzaxe"));
                        }
                        //Création des séries
                        SerieGestion(lstS, paramsChart, _xmlBase);

                        break;

                    #endregion


                    #region SERIES_TYPE_GROUP

                    case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP:

                        //Etiquette X
                        XmlElement xCategories = _xmlRoot.CreateElement("categories");
                        _xmlBase.AppendChild(xCategories);


                        foreach (KeyValuePair<String, SeriesElement> kv in lstS)
                        {
                            SeriesElement se = kv.Value;

                            // X
                            if (xCategories.SelectSingleNode(String.Concat("//category[@eudovaluelabelselect='", se.GetCategorieLabelSanitized, "']")) == null)
                            {

                                XmlElement xCat = _xmlRoot.CreateElement("category");
                                xCategories.AppendChild(xCat);
                                AddAttribute(xCat, "label", String.IsNullOrEmpty(se.GetCategorieDisplayLabel) ? eResApp.GetRes(Pref, 141) : se.GetCategorieDisplayLabel);
                                AddAttribute(xCat, "valuelabel", se.GetCategorieLabel);
                                AddAttribute(xCat, "eudovaluelabelselect", se.GetCategorieLabelSanitized);
                            }

                            //Séries
                            XmlElement xDataset = (XmlElement)xCategories.SelectSingleNode(String.Concat("//dataset[@eudoseriesIdselect='", se.GetDataSetIDSanitized, "']"));
                            if (xDataset == null)
                            {

                                xDataset = _xmlRoot.CreateElement("dataset");
                                _xmlBase.AppendChild(xDataset);
                                AddAttribute(xDataset, "seriesName", String.IsNullOrEmpty(se.FldGValue.sDisplayValue) ? eResApp.GetRes(Pref, 141) : se.FldGValue.sDisplayValue);
                                AddAttribute(xDataset, "seriesId", se.GetDataSetID);
                                AddAttribute(xDataset, "eudoseriesIdselect", se.GetDataSetIDSanitized);
                            }
                        }


                        XmlNodeList xnlCat = xCategories.SelectNodes("//category");
                        XmlNodeList xnlDs = _xmlBase.SelectNodes("//dataset");
                        Int32 categorie = 0;
                        Int32 dSet = 0;

                        foreach (XmlElement xds in xnlDs)
                        {
                            String IdDS = xds.Attributes["seriesId"].Value;
                            foreach (XmlElement xC in xnlCat)
                            {

                                String IdCat = xC.Attributes["valuelabel"].Value;
                                String sIdSeries = String.Concat(IdCat, "_", IdDS);

                                XmlElement xSet = _xmlRoot.CreateElement("set");
                                xds.AppendChild(xSet);

                                AddAttribute(xSet, "serieAlias", sIdSeries);
                                if (lstS.ContainsKey(sIdSeries))
                                {
                                    string sValue = lstS[sIdSeries].GetCategorieLabel;
                                    if (_fldX.Popup == PopupType.ONLY || _fldX.Popup == PopupType.FREE)
                                        sValue = String.Concat(_fldX.Alias, "_", lstS[sIdSeries].GetCategorieDisplayLabel); //49285 sur les catalogues simple, il faut utiliser la displayvalue


                                    //Drilldown
                                    sDrillDownLink = String.Concat(
                                            ChartReport.Id, sSepRaport,
                                            _fldX.Alias,
                                            sSepAliasValue,
                                            sValue, lstS[sIdSeries].GetCategorieLabel != String.Concat(_fldX.Alias, "_") ? sAddedDrill : String.Empty,
                                            sSepFldFilter,
                                            _fldGroup.Alias,
                                            sSepAliasValue,
                                             lstS[sIdSeries].FldGValue.sSQLValue
                                        );

                                    if (_lstFldOp[0] == eModelConst.CHART_SERIES_OPERATION.SUM && _lstFldY[0].Format == FieldFormat.TYP_BIT)
                                    {
                                        sDrillDownLink = String.Concat(sDrillDownLink, sSepFldFilter,
                                            _lstFldY[0].Alias,
                                            sSepAliasValue,
                                            1);
                                    }

                                    if (!_bIsFileMode)
                                        AddAttribute(xSet, "link", String.Concat("j-goNavF-", sDrillDownLink));

                                    AddAttribute(xSet, "value", lstS[sIdSeries].NumValue.ToString(CultureInfo.CreateSpecificCulture(sLangConnexion)).Replace(",", "."));
                                    if (!bBk)
                                        bBk = Math.Abs(lstS[sIdSeries].NumValue) >= 1000;

                                    if (bEmptyGraph)
                                        bEmptyGraph = lstS[sIdSeries].NumValue == 0;
                                }


                            }
                        }

                        categorie = 0;

                        Dictionary<int, float> dSomme = new Dictionary<int, float>();

                        foreach (XmlElement xC in xnlCat)
                        {

                            dSomme[categorie] = 0;

                            foreach (XmlElement xds in xnlDs)
                            {
                                dSet = 0;
                                XmlNodeList xnlSet = xds.ChildNodes;

                                foreach (XmlElement xset in xnlSet)
                                {
                                    if (categorie == dSet)
                                        dSomme[categorie] += String.IsNullOrEmpty(xset.GetAttribute("value")) ? 0 :
                                            Math.Abs(float.Parse(xset.GetAttribute("value").Replace(".", ",")));

                                    dSet++;

                                }

                            }

                            AddAttribute(xC, "total", dSomme[categorie].ToString().Replace(",", "."));
                            categorie++;
                        }
                        AddAttribute(_xmlBase, sBk, bBk ? "1" : "0");
                        AddAttribute(_xmlBase, sEmptyGraph, bEmptyGraph ? "1" : "0");
                        break;

                    #endregion


                    #region SERIES_TYPE_VALUE
                    case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_VALUE:


                        //Etiquette X
                        xCategories = _xmlRoot.CreateElement("categories");
                        _xmlBase.AppendChild(xCategories);


                        //Séries
                        for (Int32 nCmpt = 0; nCmpt < _lstFldY.Count; nCmpt++)
                        {
                            //BSE: bug #72 730 // Attente validation PO
                            // Gestion décimale
                            //if (!sValueOperations.ToLower().Equals(eModelConst.CHART_SERIES_OPERATION.COUNT.ToString().ToLower())
                            //    && dec < _lstFldY[nCmpt].Length)
                            //    dec = _lstFldY[nCmpt].Length;

                            XmlElement xDataset = _xmlRoot.CreateElement("dataset");
                            _xmlBase.AppendChild(xDataset);
                            AddAttribute(xDataset, "seriesName", SeriesElement.GetValueLabel(ChartReport.Tab, ecSeriesType, _lstFldOp[nCmpt], _lstFileY[nCmpt], _lstFldY[nCmpt], Pref, _fldGroup));
                            AddAttribute(xDataset, "seriesId", String.Concat(_lstFldY[nCmpt] != null ? _lstFldY[nCmpt].Alias : _lstFileY[nCmpt].ToString(), "_", _lstFldOp[nCmpt].GetHashCode()));
                        }

                        //BSE: bug #72 730 // Attente validation PO
                        // Gestion décimale
                        //AddAttribute(_xmlBase, string.Concat(chartPrefix, "decimals"), dec.ToString());

                        foreach (KeyValuePair<String, SeriesElement> kv in lstS)
                        {

                            SeriesElement se = kv.Value;
                            if (xCategories.SelectSingleNode(String.Concat("//category[@eudovaluelabelselect='", se.GetCategorieLabelSanitized, "']")) == null)
                            {
                                XmlElement xCat = _xmlRoot.CreateElement("category");
                                xCategories.AppendChild(xCat);
                                AddAttribute(xCat, "label", se.GetCategorieDisplayLabel);
                                AddAttribute(xCat, "valuelabel", se.GetCategorieLabel);
                                AddAttribute(xCat, "eudovaluelabelselect", se.GetCategorieLabelSanitized);
                            }


                            XmlNode xDataset = _xmlBase.SelectSingleNode(String.Concat("//dataset[@eudoseriesIdselect='", se.GetDataSetIDSanitized, "']"));
                            if (xDataset == null)
                                xDataset = _xmlBase.SelectSingleNode(String.Concat("//dataset[@seriesId='", se.GetDataSetIDSanitized, "']"));

                            if (xDataset != null)
                            {
                                XmlElement xSet = _xmlRoot.CreateElement("set");
                                xDataset.AppendChild(xSet);
                                AddAttribute(xSet, "value", se.NumValue.ToString(CultureInfo.CreateSpecificCulture(sLangConnexion)).Replace(",", "."));
                                AddAttribute(xSet, "serieAlias", se.GetSerieAlias);


                                string sValue = se.GetCategorieLabel;
                                if (_fldX.Popup == PopupType.ONLY || _fldX.Popup == PopupType.FREE)
                                    sValue = String.Concat(_fldX.Alias, "_", se.GetCategorieDisplayLabel);

                                //Drilldown
                                sDrillDownLink = String.Concat(
                                        ChartReport.Id,
                                        sSepRaport,

                                        _fldX.Alias,
                                        sSepAliasValue,
                                        sValue, se.GetCategorieLabel != String.Concat(_fldX.Alias, "_") ? sAddedDrill : String.Empty
                                    );


                                if (se.FldY.Format == FieldFormat.TYP_BIT && se.Op == eModelConst.CHART_SERIES_OPERATION.SUM)
                                {
                                    sDrillDownLink = String.Concat(sDrillDownLink, sSepFldFilter,
                                       se.FldY.Alias,
                                        sSepAliasValue,
                                        1);
                                }

                                if (!_bIsFileMode)
                                    AddAttribute(xSet, "link", String.Concat("j-goNavF-", sDrillDownLink));

                                if (bEmptyGraph)
                                    bEmptyGraph = se.NumValue == 0;

                            }
                        }


                        xnlCat = xCategories.SelectNodes("//category");
                        xnlDs = _xmlBase.SelectNodes("//dataset");

                        categorie = 0;

                        dSomme = new Dictionary<int, float>();

                        foreach (XmlElement xC in xnlCat)
                        {

                            dSomme[categorie] = 0;

                            foreach (XmlElement xds in xnlDs)
                            {
                                dSet = 0;
                                XmlNodeList xnlSet = xds.ChildNodes;

                                foreach (XmlElement xset in xnlSet)
                                {
                                    if (categorie == dSet)
                                        dSomme[categorie] += String.IsNullOrEmpty(xset.GetAttribute("value")) ? 0 : float.Parse(xset.GetAttribute("value").Replace(".", ","));

                                    dSet++;

                                }
                            }

                            AddAttribute(xC, "total", dSomme[categorie].ToString().Replace(",", "."));
                            if (!bBk)
                                bBk = Math.Abs(dSomme[categorie]) >= 1000;

                            categorie++;
                        }
                        AddAttribute(_xmlBase, sBk, bBk ? "1" : "0");
                        AddAttribute(_xmlBase, sEmptyGraph, bEmptyGraph ? "1" : "0");
                        break;
                    #endregion

                    default:
                        break;
                }

                //
                //_xmlRoot.AppendChild(xmlBase);

                #region Tri du XML
                /*
                if (bSort)
                {
                    String sSortXSL = "chartA.xsl";
                    if (sSortOrder != "ASC")
                        sSortXSL = "chartD.xsl";


                    sSortXSL = AppDomain.CurrentDomain.BaseDirectory + @"Charts\" + sSortXSL;

                    XslCompiledTransform xslt = new XslCompiledTransform();
                    xslt.Load(sSortXSL);



                    StringBuilder sbNavBar = new StringBuilder();

                    XmlWriter xwriter = XmlWriter.Create(sbNavBar, xslt.OutputSettings);
                    xslt.Transform(_xmlRoot, null, xwriter);

                    _xmlRoot = new XmlDocument();
                    _xmlRoot.LoadXml(sbNavBar.ToString());


                }
                */
                #endregion

                return true;
            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = e.Message;

                return false;
            }
        }

        /// <summary>
        /// Gets the series element.
        /// </summary>
        /// <param name="chartRecord">Liste des enregistrements</param>
        /// <param name="ecSeriesType">Type de série</param>
        /// <param name="sGroupX">Groupe X</param>
        /// <returns></returns>
        private Dictionary<String, SeriesElement> GetSeriesElement(List<eRecord> chartRecord, eModelConst.CHART_SERIES_TYPE ecSeriesType, string sGroupX)
        {
            Dictionary<String, SeriesElement> lstS = new Dictionary<String, SeriesElement>();

            try
            {

                // Parcours des record
                foreach (eRecord er in Chart.ListRecords)
                {


                    /****************************************************************/
                    // Champ X
                    eFieldRecord efX = er.GetFieldByAlias(_fldX.Alias);
                    if (!efX.RightIsVisible)
                        continue;
                    /****************************************************************/



                    /*****************************************************************************************/
                    /* parcours des champs de valeur - 1 seul valeur sauf pour le type .SERIES_TYPE_VALUE   **/
                    /*****************************************************************************************/
                    for (Int32 nCmpt = 0; nCmpt < _lstFldY.Count; nCmpt++)
                    {

                        Field fldY = _lstFldY[nCmpt];
                        eModelConst.CHART_SERIES_OPERATION ecOP = _lstFldOp[nCmpt];

                        SeriesElement se;
                        try
                        {
                            se = SeriesElement.GetSerieElement(Pref, ecSeriesType, _fldX, _lstFileY[nCmpt], fldY, _fldGroup, ecOP, er, sGroupX);
                        }
                        catch (Exception e)
                        {
                            _eException = e;
                            return null;
                        }

                        //Ajoute la série si elle n'existe pas
                        if (!lstS.ContainsKey(se.GetSerieAlias))
                            lstS.Add(se.GetSerieAlias, se);

                        // met à jour la série
                        lstS[se.GetSerieAlias].UpdateSerie(er);

                    }


                }

                foreach (KeyValuePair<String, SeriesElement> kvp in lstS)
                {
                    kvp.Value.ApplyAverage();
                }
            }
            catch (Exception e)
            {

                _eException = e;
                return null;
            }



            return lstS;
        }

        /// <summary>
        /// Générer une série simple 
        /// </summary>
        /// <param name="lstS"></param>
        /// <param name="paramsChart"></param>
        /// <param name="xmlBase"></param>
        private void SerieGestion(IDictionary<String, SeriesElement> lstS, ChartParams paramsChart, XmlElement xmlBase)
        {
            float fTotalValue = 0;
            float fMinVal = 0;
            float fMaxVal = 0;
            float fPercentage = 0;
            string sNode = "set";
            string sTotalValues = "totalValues";
            string sFormatedTotalValues = "formatedTotalValues";
            string sMaxVal = "maxVal";
            string sMinVal = "minVal";
            string sBk = "bk";
            bool bBk = false;
            bool bEmptyGraph = true;
            string sEmptyGraph = "emptyGraph";
            CultureInfo cInfo = CultureInfo.GetCultureInfo(paramsChart.sLangConnexion);

            if (paramsChart.sPrefix == eLibConst.COMBINED_Z)
            {
                sNode = string.Concat(paramsChart.sPrefix, "set");
                sTotalValues = string.Concat(paramsChart.sPrefix, sTotalValues);
                sFormatedTotalValues = string.Concat(paramsChart.sPrefix, sFormatedTotalValues);
                sMaxVal = string.Concat(paramsChart.sPrefix, sMaxVal);
                sMinVal = string.Concat(paramsChart.sPrefix, sMinVal);
                sBk = string.Concat(paramsChart.sPrefix, sBk);
                sEmptyGraph = string.Concat(paramsChart.sPrefix, sEmptyGraph);

            }

            if (lstS.Count > 0)
            {
                //Le total calculer sur des valeurs absolues
                //Si on prend les valeurs exactes: refaire le calule pour le pourcentage
                fTotalValue = lstS.Sum(s => Math.Abs(s.Value.NumValue));
                fMinVal = lstS.Min(s => s.Value.NumValue);
                fMaxVal = lstS.Max(s => s.Value.NumValue);
                bBk = (Math.Abs(fMaxVal) > 1000 || Math.Abs(fMinVal) > 1000);

                List<KeyValuePair<String, SeriesElement>> list = lstS.OrderByDescending(s => s.Value.NumValue).ToList();

                for (int i = 0; i < list.Count; i++)
                {
                    if (i < 30)
                        list[i].Value.bLabelValueVisible = true;
                }

                //Si le type graphique est funnel / pyramid => catalogue , le tri se fait sur le SortBy de la table filedataparam
                if (paramsChart.sTypeChart == SYNCFUSIONCHART.FUNNEL || paramsChart.sTypeChart == SYNCFUSIONCHART.PYRAMID)
                {
                    IList<Int32> lstSorted = Chart.GetListOrderBySortBy(list.Select(x =>
                 Int32.Parse(String.IsNullOrEmpty(x.Value.GetCategorieSqlValue) ? (0).ToString() : x.Value.GetCategorieSqlValue)).ToList(), paramsChart.bEtiquettesTriCatXSortAcending);


                    lstS = lstS.OrderBy(x => lstSorted.IndexOf(Int32.Parse(String.IsNullOrEmpty(x.Value.GetCategorieSqlValue) ? (0).ToString() : x.Value.GetCategorieSqlValue))).ToDictionary(x => x.Key, x => x.Value);
                }
                else if (paramsChart.bSort)
                {
                    if (paramsChart.nSortOrder)
                        lstS = lstS.OrderBy(s => s.Value.NumValue).ToDictionary(x => x.Key, x => x.Value);
                    else
                        lstS = lstS.OrderByDescending(s => s.Value.NumValue).ToDictionary(x => x.Key, x => x.Value);
                }

                //Parcours de la liste des séries
                int nbS = 0;
                foreach (KeyValuePair<String, SeriesElement> kv in lstS)
                {

                    SeriesElement se = kv.Value;
                    fPercentage = Math.Abs(fTotalValue > 0 ? se.NumValue / fTotalValue : 0);
                    //sph voir demande 56 355, j'ai déplace ici le test pour ne pas parcourrir 2 x la liste
                    //if (nbS < 30)
                    //    se.bLabelValueVisible = true;

                    nbS++;


                    XmlElement xSet = _xmlRoot.CreateElement(sNode);
                    xmlBase.AppendChild(xSet);

                    //Drilldown                        

                    string sValue = se.GetCategorieLabel;
                    //BSE #52 206 :utilisation de PopupTypeAdv pour distinction du catalogue avancé et champ de liaison
                    if (_fldX.Popup == PopupType.ONLY || _fldX.Popup == PopupType.FREE)
                        sValue = String.Concat(_fldX.Alias, "_", se.GetCategorieDisplayLabel);//49285 sur les catalogues simple, il faut utiliser la displayvalue

                    paramsChart.sDrillDownLink = String.Concat(ChartReport.Id, paramsChart.sSepRaport, _fldX.Alias, paramsChart.sSepAliasValue,
                        sValue, se.GetCategorieLabel != String.Concat(_fldX.Alias, "_") ? paramsChart.sAddedDrill : String.Empty
                       );


                    if (_lstFldOp[0] == eModelConst.CHART_SERIES_OPERATION.SUM && _lstFldY[0].Format == FieldFormat.TYP_BIT)
                    {
                        paramsChart.sDrillDownLink = String.Concat(paramsChart.sDrillDownLink, paramsChart.sSepFldFilter,

                            _lstFldY[0].Alias,
                            paramsChart.sSepAliasValue,
                            1);
                    }

                    if (Chart != null && Chart.ChartExpressFilter != null && Chart.ChartExpressFilter.Count > 0)
                    {
                        foreach (eChartExpressFilter filtre in Chart.ChartExpressFilter)
                        {
                            if (filtre.EfField != null && filtre.Op != Operator.OP_UNDEFINED && !string.IsNullOrEmpty(filtre.EfValue))
                            {
                                paramsChart.sDrillDownLink = String.Concat(paramsChart.sDrillDownLink, paramsChart.sSepFldFilter,

                            filtre.EfTabe.DescId, "_", filtre.EfField.Descid,
                            paramsChart.sSepAliasValue,
                            String.IsNullOrEmpty(filtre.EfValue) ? "<>" : filtre.EfValue, paramsChart.sSepFldOperator, (int)filtre.Op);
                            }

                        }

                    }

                    if (!_bIsFileMode)
                    {
                        if (!string.IsNullOrEmpty(paramsChart.sPrefix))
                        {
                            if (paramsChart.sPrefix == eLibConst.COMBINED_Z)
                                AddAttribute(xSet, "link", String.Concat("j-goNavF-Z", _fldX.Table.DescId + "$**$", paramsChart.sDrillDownLink));
                            else if (paramsChart.sPrefix == eLibConst.COMBINED_Y)
                                AddAttribute(xSet, "link", String.Concat("j-goNavF-Y", _fldY.Table.DescId + "$**$", paramsChart.sDrillDownLink));
                            else
                                AddAttribute(xSet, "link", String.Concat("j-goNavF-", paramsChart.sDrillDownLink));
                        }
                        else
                            AddAttribute(xSet, "link", String.Concat("j-goNavF-", paramsChart.sDrillDownLink));

                    }

                    if (_fldX.Format == FieldFormat.TYP_DATE)
                    {
                        DateTime date = DateTime.Now;
                        if (DateTime.TryParse(se.GetCategorieDisplayLabel, out date))
                            AddAttribute(xSet, "dateFr", String.IsNullOrEmpty(se.GetCategorieDisplayLabel) ? string.Concat(eResApp.GetRes(Pref, 141), paramsChart.sSepAliasValue, se.GetCategorieSqlValue) : date.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern));

                        AddAttribute(xmlBase, "bDateFr", "1");
                    }


                    if (paramsChart.bDisplayValuesPercent)
                    {
                        if (fPercentage != 0)
                            AddAttribute(xSet, "text", string.Concat((fPercentage * 100).ToString("N" + paramsChart.nDecimal, cInfo), "%"));
                    }
                    else
                        AddAttribute(xSet, "text", se.NumValue.ToString("N" + (paramsChart.nDecimal > 2 ? paramsChart.nDecimal : 0), cInfo));

                    // Etiquette X
                    AddAttribute(xSet, "label", String.IsNullOrEmpty(se.GetCategorieDisplayLabel) ? string.Concat(eResApp.GetRes(Pref, 141), paramsChart.sSepAliasValue, se.GetCategorieSqlValue) : se.GetCategorieDisplayLabel);

                    // Valeur Y      
                    AddAttribute(xSet, "value", se.NumValue.ToString(cInfo));
                    AddAttribute(xSet, "labelValueVisible", list.First(r => r.Key == kv.Key).Value.bLabelValueVisible ? "1" : "0");
                    AddAttribute(xSet, "bSort", paramsChart.bSort ? "1" : "0");
                    AddAttribute(xSet, "percent", fPercentage.ToString(cInfo));

                    if (bEmptyGraph)
                        bEmptyGraph = se.NumValue == 0;

                }
            }

            AddAttribute(xmlBase, sEmptyGraph, bEmptyGraph ? "1" : "0");
            AddAttribute(xmlBase, sBk, bBk ? "1" : "0");
            AddAttribute(xmlBase, sFormatedTotalValues, fTotalValue.ToString("N" + paramsChart.nDecimal, cInfo));
            AddAttribute(xmlBase, "configurated" + sFormatedTotalValues, eNumber.FormatNumber(fTotalValue, new eNumber.DecimalParam(Pref) { HasDigits = paramsChart.nDecimal > 0, NumberDigitMin = paramsChart.nDecimal }, new eNumber.SectionParam(Pref)));
            AddAttribute(xmlBase, sTotalValues, fTotalValue.ToString(cInfo));
            AddAttribute(xmlBase, sMaxVal, bBk ? (fMaxVal / 1000).ToString() : fMaxVal.ToString());
            AddAttribute(xmlBase, sMinVal, bBk ? (fMinVal / 1000).ToString() : fMinVal.ToString());
        }

        /// <summary>
        /// Retourne la langue de connexion
        /// </summary>
        /// <returns></returns>
        private string GetCultureLogin()
        {
            switch (_ePref.LangServId)
            {
                case 0:
                    return "fr-FR";
                case 1:
                case 3:
                case 5:
                    return "en-US";
                case 2:
                    return "de-DE";
                case 4:
                    return "es-ES";
                default:
                    return "fr-FR";
            }
        }


        /// <summary>
        /// Ajoute un attribut à un Element XML
        /// </summary>
        /// <param name="xNode">Node XML</param>
        /// <param name="sNameAtt">Nom de l'attribut</param>
        /// <param name="sValue">Valeur de l'attribut</param>
        private void AddAttribute(XmlElement xNode, String sNameAtt, String sValue)
        {
            XmlAttribute xAtt = _xmlRoot.CreateAttribute(sNameAtt);
            xAtt.InnerText = sValue;
            xNode.Attributes.Append(xAtt);
        }


        /// <summary>
        /// Ajoute au collection de liste de fieldvalue/operation value les champs opération
        /// à partir de leur valeur de paramètre
        /// </summary>
        /// <param name="sValueFile">Descid De la table à ajouter</param>
        /// <param name="sField">Descid Du champ à ajouter</param>
        /// <param name="sOperation">Opération sur le champ</param>
        /// <returns>Résultat de l'opération </returns>
        private Boolean AddValueFieldValue(String sValueFile, String sField, String sOperation)
        {
            try
            {
                eModelConst.CHART_SERIES_OPERATION op;
                if (!Enum.TryParse<eModelConst.CHART_SERIES_OPERATION>(sOperation, true, out op))
                {
                    _sErrorMsg = String.Concat("OPERATION INVALIDE >", sOperation, "<");
                    return false;
                }
                else
                    _lstFldOp.Add(op);

                Field fldValue = new Field();

                fldValue = Chart.FldFieldsInfos.Find(delegate (Field f) { return f.Descid.ToString() == sField; });


                if (fldValue == null && op != eModelConst.CHART_SERIES_OPERATION.COUNT)
                {
                    _sErrorMsg = String.Concat("CHAMP INTROUVABLE >", sField, "<");
                    return false;
                }
                else
                {
                    _lstFldY.Add(fldValue);
                }

                Int32 iTab;
                if (Int32.TryParse(sValueFile, out iTab))
                {
                    _lstFileY.Add(iTab);
                }
                else
                {
                    _sErrorMsg = String.Concat("TABLE INTROUVABLE >", sValueFile, "<");
                    throw new Exception(_sErrorMsg);
                }
            }
            catch (Exception e)
            {
                //Interception des erreur
                _eException = e;
                _sErrorMsg = e.ToString();
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;

                return false;
            }



            return true;
        }

        /// <summary>
        /// Element d'une série
        /// Est définie par une valeur d'étiquette (axe X principal) et une valeur de série (division sur cette étiquette)
        /// </summary>
        private class SeriesElement
        {




            #region Properties

            /// <summary>
            /// Objet Pref, notamment pour l'internationalisation
            /// </summary>
            private ePref _pref;

            /// <summary>
            /// Type de graphique
            /// </summary>
            private eModelConst.CHART_SERIES_TYPE _ecSerieType = eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE;

            /// <summary>
            /// L'objet est-il initialisé ?
            /// </summary>
            private Boolean _bIsInit = false;

            /// <summary>
            /// Liste des ID déjà pris en compte dans la série
            /// </summary>
            private HashSet<string> hsLstElem = new HashSet<string>();

            /// <summary>
            /// Information générique sur le champ X
            /// </summary>
            private Field _fldX;

            private String _sFldxGroup = String.Empty;

            /// <summary>
            /// Information générique sur le champ de valeur (Y)
            /// </summary>
            private Field _fldY;

            private Int32 _iFileY = 0;

            /// <summary>
            /// Information générique sur le champ de regroupement
            /// </summary>
            private Field _fldG;

            /// <summary>
            /// Opération sur la valeur
            /// </summary>
            private eModelConst.CHART_SERIES_OPERATION _ecOp;


            /// <summary>
            /// Valeur du champ X
            /// </summary>
            private SEValue _seFldXValue;

            /// <summary>
            /// Valeur du champ de regroupement
            /// </summary>
            private SEValue _seFldGValue;


            /// <summary>
            /// Descid de la table principale
            /// </summary>
            private Int32 _nMainTabDescid;

            /// <summary>
            /// Valeur de la série
            /// </summary>
            private float _fNumValue = 0;

            /// <summary>
            /// Afficher le label
            /// </summary>
            private Boolean _bLabelValueVisible;

            #endregion


            #region Accesseurs

            /// <summary>
            /// opérateur du champ de valeur sur la série
            /// </summary>
            public eModelConst.CHART_SERIES_OPERATION Op
            {
                get { return _ecOp; }

            }

            /// <summary>
            /// Indique une valeur a été passé a la série
            /// nécessaire pour les opérateur min/max
            /// </summary>
            public Boolean IsInit
            {
                get { return _bIsInit; }

            }

            /// <summary>
            /// Valeur du champ de regroupement
            /// </summary>
            public SEValue FldGValue
            {
                get { return _seFldGValue; }
            }

            /// <summary>
            /// Valeur de la série
            /// </summary>
            public float NumValue
            {
                get
                { return _fNumValue; }

                private set
                {
                    _fNumValue = value;
                    if (!_bIsInit)
                        _bIsInit = true;
                }
            }

            /// <summary>
            /// Hashset permettant de vérifier qu'une valeur n'a pas été déjà comtpé dans la série
            /// </summary>
            public HashSet<string> LstElem
            {
                get { return hsLstElem; }
                set
                {

                    hsLstElem = value;

                }
            }


            /// <summary>
            /// Hashset permettant de vérifier qu'une valeur n'a pas été déjà comtpé dans la série
            /// </summary>
            public Boolean bLabelValueVisible
            {
                get { return _bLabelValueVisible; }
                set
                {

                    _bLabelValueVisible = value;

                }
            }


            public Field FldY
            {
                get { return _fldY; }
            }



            public Field FldX
            {
                get { return _fldX; }
            }


            #endregion



            #region Method



            /// <summary>
            /// Retourne le label affiché de la valeur de la série (Y)
            /// ex : Nombre de fiche (Marketting)
            /// Somme (Affaire.CA)
            /// </summary>
            /// <param name="nMainTabDescId">Table de départ du chart</param>
            /// <param name="ecType">Type de série</param>
            /// <param name="ecOP">Opérateur de la série</param>
            /// <param name="iValueFile">The i value file.</param>
            /// <param name="fldY">Champ de valeur</param>
            /// <param name="pref">The preference.</param>
            /// <param name="fldg">Champ de regroupement</param>
            /// <returns></returns>
            public static string GetValueLabel(Int32 nMainTabDescId, eModelConst.CHART_SERIES_TYPE ecType, eModelConst.CHART_SERIES_OPERATION ecOP, Int32 iValueFile, Field fldY, ePrefLite pref, Field fldg = null)
            {
                String sLabel = String.Concat(eResApp.GetRes(pref.LangServId, ecOP.GetHashCode()));

                if (ecOP == eModelConst.CHART_SERIES_OPERATION.COUNT)
                {
                    eRes res = new eRes(pref, iValueFile.ToString());
                    String sTableLabel = res.GetRes(iValueFile);
                    return String.Concat(sLabel, " (", sTableLabel, ")");
                }
                //Ajout d'information de contexte  (table et champ sur lequel porte la série) si nécessaire
                if (nMainTabDescId != fldY.Table.DescId || ecOP != eModelConst.CHART_SERIES_OPERATION.COUNT)
                {
                    switch (ecType)
                    {
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_SIMPLE:
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_VALUE:

                            sLabel = String.Concat(sLabel, " (");

                            if (nMainTabDescId != fldY.Table.DescId)
                                sLabel = String.Concat(sLabel, fldY.Table.Libelle, ".");

                            //Sauf pour compte indication du champ
                            if (ecOP != eModelConst.CHART_SERIES_OPERATION.COUNT)
                                sLabel = String.Concat(sLabel, fldY.Libelle);

                            sLabel = string.Concat(sLabel, ")");

                            break;

                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP:

                            sLabel = String.Concat(sLabel, " (");

                            if (nMainTabDescId != fldY.Table.DescId)
                                sLabel = String.Concat(sLabel, fldY.Table.Libelle, ".");

                            //Sauf pour compte indication du champ
                            if (ecOP != eModelConst.CHART_SERIES_OPERATION.COUNT)
                                sLabel = String.Concat(sLabel, ".", fldg.Libelle);

                            sLabel = string.Concat(sLabel, ")");

                            break;



                    }
                }
                return sLabel;
            }


            /// <summary>
            /// Retourne la valeur affichable de l'étiquette (X)
            /// </summary>
            public String GetCategorieDisplayLabel
            {
                get
                {
                    if (_fldX.Format == FieldFormat.TYP_BIT)
                    {
                        return (_seFldXValue.sDisplayValue == "1") ? eResApp.GetRes(_pref, 308) : eResApp.GetRes(_pref, 309);
                    }
                    else if (_fldX.Format == FieldFormat.TYP_BITBUTTON)
                    {
                        return (_seFldXValue.sDisplayValue == "1") ? eResApp.GetRes(_pref, 7219) : eResApp.GetRes(_pref, 7135);
                    }
                    else
                        return _seFldXValue.sDisplayValue;
                }
            }



            /// <summary>
            /// Retourne la valeur SQL de l'étiquette (X) avec le prefixe du field
            /// </summary>
            public String GetCategorieLabel
            {
                get { return String.Concat(_fldX.Alias, "_", _seFldXValue.sSQLValue); }
            }

            /// <summary>
            /// Retourne la valeur SQL de l'étiquette (X) avec le prefixe du field, sans single quote
            /// </summary>
            public String GetCategorieLabelSanitized
            {
                get { return GetCategorieLabel.Replace("'", ""); }
            }

            /// <summary>
            /// Retourne la valeur SQL de l'étiquette (X)
            /// </summary>
            public string GetCategorieSqlValue
            {
                get { return _seFldXValue.sSQLValue; }
            }

            /// <summary>
            /// Retourne l'alias de la série (par valeur X )
            ///     CATEGORIELABEL_DATASETID
            /// </summary>
            public String GetSerieAlias
            {
                get { return String.Concat(GetCategorieLabel, "_", GetDataSetID); }
            }

            /// <summary>
            /// Retourne l'alias de la série (par valeur X ), sans single quote
            ///     CATEGORIELABEL_DATASETID
            /// </summary>
            public String GetSerieAliasSanitized
            {
                get { return GetSerieAlias.Replace("'", ""); }
            }


            /// <summary>
            /// Retourne un identifiant pour les valeurs composant un même série
            /// (dataset dans fusion chart)
            /// </summary>
            public String GetDataSetID
            {
                get
                {
                    String sValue = String.Empty;
                    String sAlias = "";
                    if (_fldY != null)
                    {
                        sAlias = _fldY.Alias;
                    }
                    else
                    {
                        sAlias = _iFileY.ToString();
                    }

                    switch (_ecSerieType)
                    {
                        case eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP:
                            sValue = String.Concat(sAlias, "_", _seFldGValue.sSQLValue);
                            break;

                        default:
                            sValue = String.Concat(sAlias, "_", _ecOp.GetHashCode());
                            break;
                    }
                    return sValue;
                }
            }

            /// <summary>
            /// Retourne un identifiant pour les valeurs composant un même série, sans single quote
            /// (dataset dans fusion chart)
            /// </summary>
            public String GetDataSetIDSanitized
            {
                get { return GetDataSetID.Replace("'", ""); }
            }


            /// <summary>
            /// Met à jour la série avec les valeurs de la ligne "e"r
            /// </summary>
            /// <param name="er">Ligne d'enregistrement contenant les valeurs des séries</param>
            public void UpdateSerie(eRecord er)
            {
                eFieldRecord efY = null;

                if (_fldY != null)
                    //Récupèration du champ de valeur
                    efY = er.GetFieldByAlias(_fldY.Alias);

                if (efY == null && _ecOp != eModelConst.CHART_SERIES_OPERATION.COUNT)
                    return;

                Int32 iFileId = 0;

                if (efY != null)
                {
                    iFileId = efY.FileId;

                    // On passe le champ si 
                    //  - Champ non visible pour les opérateur != "COUNT"
                    //  - table non visible pour l'opérateur "COUNT"
                    if ((!(efY.RightIsVisible || (_ecOp == eModelConst.CHART_SERIES_OPERATION.COUNT && efY.RightIsTableVisible))))
                        return;
                }
                else
                {
                    String sTableAlias = er.ViewTab.ToString();

                    if (er.ViewTab != _iFileY)
                        sTableAlias += "_" + _iFileY.ToString();

                    iFileId = er.TablesFileId[sTableAlias];

                    if (!er.TablesIsViewable[sTableAlias])
                        return;

                }


                //Ne pas compter un champ sans fileid
                if (iFileId == 0 && _ecOp != eModelConst.CHART_SERIES_OPERATION.COUNT)
                    return;




                //Chaque champ ne doit être compté qu'une fois pour sa série
                if (hsLstElem.Contains(iFileId.ToString()))
                    return;

                hsLstElem.Add(iFileId.ToString());



                //
                String sFldYValue = efY?.Value ?? "0";
                float fValue = 0;

                if (_ecOp == eModelConst.CHART_SERIES_OPERATION.COUNT || float.TryParse(sFldYValue, out fValue))
                {
                    switch (_ecOp)
                    {
                        case eModelConst.CHART_SERIES_OPERATION.MIN:

                            if (!_bIsInit)
                                NumValue = fValue;

                            if (NumValue > fValue)
                                NumValue = fValue;

                            break;
                        case eModelConst.CHART_SERIES_OPERATION.AVG:
                            NumValue += fValue;
                            break;
                        case eModelConst.CHART_SERIES_OPERATION.MAX:
                            if (!_bIsInit)
                                NumValue = fValue;

                            if (NumValue < fValue)
                                NumValue = fValue;

                            break;
                        case eModelConst.CHART_SERIES_OPERATION.SUM:
                            NumValue += fValue;
                            break;
                        case eModelConst.CHART_SERIES_OPERATION.COUNT:
                            if (iFileId > 0)
                                NumValue++;
                            break;
                        default:
                            break;
                    }
                }

            }

            public void ApplyAverage()
            {
                if (_ecOp != eModelConst.CHART_SERIES_OPERATION.AVG)
                    return;

                if (hsLstElem.Count <= 0)
                    return;

                _fNumValue = _fNumValue / hsLstElem.Count;

            }

            #endregion




            #region Constructeurs



            /// <summary>
            /// Retourne un élement de série
            /// </summary>
            /// <param name="pref">ePref</param>
            /// <param name="ecType">Type de série</param>
            /// <param name="fldX">Champ X</param>
            /// <param name="iFileY">The i file y.</param>
            /// <param name="fldY">Champ Y</param>
            /// <param name="fldG">Champ de regroupement</param>
            /// <param name="ecOP">Opérateur de la série</param>
            /// <param name="er">Ligne d'enregistrement en cours</param>
            /// <param name="sFldXGroup">Regroupement par X (date)</param>
            /// <returns></returns>
            public static SeriesElement GetSerieElement(ePref pref, eModelConst.CHART_SERIES_TYPE ecType, Field fldX, Int32 iFileY, Field fldY, Field fldG, eModelConst.CHART_SERIES_OPERATION ecOP, eRecord er, String sFldXGroup)
            {


                SeriesElement sE = new SeriesElement(pref, ecType, fldX, iFileY, fldY, fldG, ecOP, er, sFldXGroup);
                return sE;


            }



            /// <summary>
            /// private générique d'un élement de série
            /// </summary>
            /// <param name="pref">Objet ePref, notamment utilisé pour les paramètres d'internationalisation</param>
            /// <param name="ecType">Type de série</param>
            /// <param name="fldX">Champ X</param>
            /// <param name="iFileY">File Y</param>
            /// <param name="fldY">Champ Y</param>
            /// <param name="fldG">Champ de regroupement</param>
            /// <param name="ecOP">Opérateur de la série</param>
            /// <param name="er">Ligne d'enregistrement en cours</param>
            /// <param name="sSerieXGroup">Regroupement sur les valeurs axe X : pour regroupement par date (semestre,trimestre...)</param>
            /// <exception cref="Exception">
            /// Champ de regroupement non fourni
            /// or
            /// Champ de regroupement non fourni
            /// or
            /// Champ de série invalide
            /// </exception>
            private SeriesElement(ePref pref, eModelConst.CHART_SERIES_TYPE ecType, Field fldX, Int32 iFileY, Field fldY, Field fldG, eModelConst.CHART_SERIES_OPERATION ecOP, eRecord er, String sSerieXGroup)
            {
                _pref = pref;


                _nMainTabDescid = er.ViewTab;
                _ecSerieType = ecType;
                _ecOp = ecOP;

                _fldX = fldX;
                _sFldxGroup = sSerieXGroup;

                _fldY = fldY;
                _iFileY = iFileY;

                if (_ecSerieType == eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP)
                {
                    if (fldG != null)
                    {
                        _fldG = fldG;
                        eFieldRecord efG = er.GetFieldByAlias(fldG.Alias);
                        if (efG == null)
                            throw new Exception("Champ de regroupement non fourni");


                        _seFldGValue.sDisplayValue = efG.DisplayValue;
                        _seFldGValue.sSQLValue = efG.Value;

                    }
                    else
                    {
                        throw new Exception("Champ de regroupement non fourni");
                    }
                }


                /*  CHAMP DE VALEUR */
                eFieldRecord efX = er.GetFieldByAlias(fldX.Alias);


                if (efX == null)
                {
                    throw new Exception("Champ de série invalide");
                }

                _seFldXValue.sSQLValue = efX.Value;
                if (_fldX.Format == FieldFormat.TYP_DATE && _sFldxGroup.Length > 0)
                {
                    switch (_sFldxGroup)
                    {
                        case "year":
                            // format : YYYY
                            if (efX.Value.Length >= 4)
                                _seFldXValue.sDisplayValue = eDate.ConvertBddToDisplay(pref.CultureInfo, efX.Value.Substring(0, 4), false, false, false, true);
                            break;
                        case "month":
                            // format : MM/YYYY
                            if (efX.Value.Length >= 6)
                                _seFldXValue.sDisplayValue = eDate.ConvertBddToDisplay(pref.CultureInfo, String.Concat(efX.Value.Substring(4, 2), "/", efX.Value.Substring(0, 4)), false, false, true, true);
                            break;
                        case "day":
                            // format : DD/MM/YYYY
                            if (efX.Value.Length >= 8)
                                _seFldXValue.sDisplayValue = eDate.ConvertBddToDisplay(pref.CultureInfo, String.Concat(efX.Value.Substring(efX.Value.Length - 2, 2), "/",
                                            efX.Value.Substring(4, 2), "/",
                                            efX.Value.Substring(0, 4)), false, true, true, true);
                            break;
                        case "weekday":
                            Int32 nWD = 0;
                            if (Int32.TryParse(efX.Value, out nWD) && nWD >= 1 && nWD <= 7)
                            {
                                nWD = nWD % 7;
                                _seFldXValue.sDisplayValue = eResApp.GetRes(0, 44 + nWD);
                            }

                            break;
                        case "trim":
                            if (efX.Value.Length > 0)
                            {
                                String[] aTrim = efX.Value.Split("_");
                                if (aTrim.Length == 2)
                                    _seFldXValue.sDisplayValue = String.Concat(eResApp.GetRes(0, 1703), " ", aTrim[0], " ", aTrim[1]);
                            }

                            break;
                        case "sem":
                            if (efX.Value.Length > 0)
                            {
                                String[] aTrim = efX.Value.Split("_");
                                if (aTrim.Length == 2)
                                    _seFldXValue.sDisplayValue = String.Concat(eResApp.GetRes(0, 1704), " ", aTrim[0], " ", aTrim[1]);
                            }
                            break;

                    }

                    if (_seFldXValue.sDisplayValue != null && _seFldXValue.sDisplayValue.Length == 0 && efX.DisplayValue.Length > 0)
                        _seFldXValue.sDisplayValue = efX.DisplayValue;
                    else if (_seFldXValue.sDisplayValue == null || _seFldXValue.sDisplayValue.Length == 0)
                        _seFldXValue.sDisplayValue = efX.Value;
                }
                else
                    _seFldXValue.sDisplayValue = efX.DisplayValue;

            }

            #endregion


            public struct SEValue
            {
                public String sDisplayValue;
                public String sSQLValue;
            }

        }


        private class ChartParams
        {
            #region Properties
            /// <summary>
            /// Table
            /// </summary>
            public string sValueFile;
            /// <summary>
            /// Rubrique
            /// </summary>
            public string sValueFields;
            /// <summary>
            /// Opérateur 
            /// </summary>
            public string sValueOperations;

            /// <summary>
            /// Préfix du grapique, utiliser dans le graphique combiné
            /// </summary>
            public string sPrefix;

            /// <summary>
            /// Tri par les valeurs 
            /// </summary>
            public bool bSort;
            /// <summary>
            /// Type de tri ASC/DESC
            /// </summary>
            public bool nSortOrder;
            /// <summary>
            /// Type du graphique
            /// </summary>
            public SYNCFUSIONCHART sTypeChart;
            /// <summary>
            /// Tri sur le libéllé des catalogues
            /// </summary>
            public bool bEtiquettesTriCatXSortAcending;
            /// <summary>
            /// Afficher les valeur en pourcentage
            /// </summary>
            public bool bDisplayValuesPercent;
            /// <summary>
            /// Afficher l'echelle en pourcentage
            /// </summary>
            public bool bDisplayStackPercent;

            /// <summary>
            /// 
            /// </summary>
            public string sDrillDownLink;
            /// <summary>
            /// 
            /// </summary>
            public string sAddedDrill;
            /// <summary>
            /// Nombre de décimal à afficher pour un chiffre type Float / decimal
            /// </summary>
            public Int32 nDecimal;
            /// <summary>
            /// Langue de connexion
            /// </summary>
            public string sLangConnexion;

            #region Séparateur
            public string sSepRaport;
            public string sSepAliasValue;
            public string sSepFldFilter;
            public string sSepFldOperator;
            public string sSepCombinedTab;
            #endregion



            #endregion
        }

        /// <summary>
        /// Génerer une erreur en retour
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        protected static XmlDocument GetErrorLuncher(string error)
        {
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = "0";
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = error;
            baseResultNode.AppendChild(errorResultNode);

            return xmlResult;
        }

    }

}