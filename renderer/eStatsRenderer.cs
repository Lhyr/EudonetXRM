using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu des statistiques : 
    /// pour la fiche campagne
    /// pour les statistiques de catalogues accessible depuis les entetes de colonnes (TODO)
    /// </summary>
    public class eStatsRenderer
    {
        const String DIV_ID_BASE = "StatsRend";

        private ePref _ePref;
        private Dictionary<String, Int32> _diData;
        private Int32 _nbTotal = 0;
        private XmlDocument _xmlChart = new XmlDocument();
        private XmlNode _xmlRoot;
        private XmlElement _xmlCategories;
        private XmlElement _xmlDataSet;

        private Table _tbDetails;
        private String _head1 = "";
        private String _head2 = "";
        private Panel _pnChart;
        private Panel _pnDetails;
        private String _sChartFormat;
        private Boolean _bDisplayDetail = true;
        private Boolean _bDisplayChart = true;
        private Boolean _bMulti = false;
        private String _sValToDispOnChart = "pct";

        /// <summary>suffix permettant d'identifier les élément html du renderer</summary>
        public String DivSuffix { get; set; }

        /// <summary>Message d'erreur rencontré par le Renderer</summary>
        public String Error { get; set; }

        /// <summary>
        /// Div Final contenant le rendu à afficher
        /// </summary>
        public Panel PgContainer { get; set; }

        /// <summary>Remonte dans une balise input toutes les informations nécéssaire à la génération du Chart</summary>
        public HtmlInputText InptChartData { get; set; }

        /// <summary>Booleen indiquant s'il n'y a pas de données</summary>
        public Boolean NoData { get; private set; }


        /// <summary>Constructeur privé</summary>
        /// <param name="sDivSuffix">Suffixe permettant de différencier les différents charts dans une même page</param>
        /// <param name="diData">dictionnaire contenant les data à représenter</param>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        private eStatsRenderer(String sDivSuffix, Dictionary<String, Int32> diData, ePref ePref)
        {
            _diData = diData;
            DivSuffix = sDivSuffix;
            _ePref = ePref;
            PgContainer = new Panel();
            PgContainer.ID = String.Concat(DIV_ID_BASE, DivSuffix);
            PgContainer.CssClass = "StatsRendBlock";
            HtmlGenericControl input = new HtmlGenericControl("input");
            input.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
            input.Style.Add(HtmlTextWriterStyle.Display, "none");
            input.ID = String.Concat(DIV_ID_BASE, DivSuffix, "Hidden");
            input.Attributes.Add("w", "0");
            input.Attributes.Add("h", "0");
            PgContainer.Controls.Add(input);
            Error = String.Empty;

        }

        /// <summary>
        /// méthode statique de rendu de statistiques
        /// </summary>
        /// <param name="sDivSuffix"></param>
        /// <param name="diData"></param>
        /// <param name="ePref"></param>
        /// <param name="aColsLabel">Tableau de chaine de caractères affichant les entete pour le tableau détails</param>
        /// <param name="sChartFormat">Format du graphique. Attention pour certains format tel que MSCombi3D, il est nécéssaire de passer bMulti à 1.</param>
        /// <param name="bDisplayDetail">Valeur à passer à false pour masquer le tableau de détails</param>
        /// <param name="bDisplayChart">Valeur à passer à false pour masquer le graphique</param>
        /// <param name="bMulti">valeur à passer à true si le graphique prends des séries multiples</param>
        /// <param name="sValToDispOnChart">"value" si c'est la valeur qui doit être prise en compte sur le graphique,  "pct" si c'est le pourcentage (valeur par défaut) </param>
        /// <returns></returns>
        public static eStatsRenderer CreateStatRenderer(String sDivSuffix, Dictionary<String, Int32> diData, ePref ePref, String[] aColsLabel = null, String sChartFormat = "Pie2D", Boolean bDisplayDetail = true, Boolean bDisplayChart = true, Boolean bMulti = false, String sValToDispOnChart = "pct")
        {
            eStatsRenderer statsRdr = new eStatsRenderer(sDivSuffix, diData, ePref);
            if (aColsLabel != null && aColsLabel.Length > 0)
            {
                if (aColsLabel.Length >= 0)
                    statsRdr._head1 = aColsLabel[0];
                if (aColsLabel.Length >= 1)
                    statsRdr._head2 = aColsLabel[1];
            }
            statsRdr._sChartFormat = sChartFormat;
            statsRdr._bDisplayDetail = bDisplayDetail;
            statsRdr._bDisplayChart = bDisplayChart;
            statsRdr._bMulti = bMulti;
            statsRdr._sValToDispOnChart = sValToDispOnChart;


            if (!statsRdr.Generate())
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, statsRdr.Error), ePref);


            return statsRdr;

        }

        /// <summary>Constructeur privé pour créer un statrenderer vide</summary>
        private eStatsRenderer()
        {
            Error = String.Empty;
        }

        /// <summary>
        /// méthode statique pour créer un statrenderer vide
        /// </summary>
        /// <returns></returns>
        public static eStatsRenderer CreateEmptyStatRenderer()
        {
            eStatsRenderer statsRdr = new eStatsRenderer();
            return statsRdr;
        }


        /// <summary>
        /// Génère le graphique et le tableau de données
        /// </summary>
        /// <returns></returns>
        private Boolean Generate()
        {
            if (!InitData())
            {
                return false;
            }


            InitBackBone();

            if (NoData)
                return true;

            InitChartXml();
            InitDetailsTable();
            FillContent();

            InitChartDataForJs();
            EndDetailsTable();

            return true;
        }

        /// <summary>
        /// crée le bloc js qui va lancer la génération fusion chart
        /// </summary>
        private void InitChartDataForJs()
        {
            if (!_bDisplayChart)
                return;

            String sObjName = String.Concat("StatsChart", DivSuffix);

            InptChartData = new HtmlInputText();
            InptChartData.Value = _xmlChart.InnerXml;
            InptChartData.Attributes.Add("ChartFmt", _sChartFormat);  //format du graphique 
            InptChartData.Attributes.Add("DivChartId", _pnChart.ID);
            InptChartData.Attributes.Add("Suffix", DivSuffix);


        }

        /// <summary>
        /// Vérifie et Prépare les données avant traitement
        /// </summary>
        /// <returns></returns>
        private bool InitData()
        {
            NoData = false;
            Error = String.Empty;
            if (_diData == null)
            {
                Error = "eStatsRenderer.InitData() : _diData Non renseigné";
                NoData = true;
                return false;
            }

            if (_diData.Count == 0)
            {
                _nbTotal = 0;
                NoData = true;
                return true;
            }



            // si le Total n'a pas été fourni, il faut le calculer
            if (_nbTotal > 0)
                return true;

            _nbTotal = 0;
            foreach (KeyValuePair<String, Int32> kvp in _diData)
            {
                _nbTotal += kvp.Value;
            }
            if (_nbTotal > 0)
                return true;
            else
            {
                NoData = true;
                return true;
            }
        }

        /// <summary>
        /// initialise la structure HTML
        /// </summary>
        /// <returns></returns>
        private Boolean InitBackBone()
        {
            _pnChart = new Panel();
            PgContainer.Controls.Add(_pnChart);
            _pnChart.ID = String.Concat(DIV_ID_BASE, DivSuffix, "Chart");
            _pnChart.Attributes.Add("SyncFusionChart", "1");
            if (NoData)
            {
                HtmlGenericControl htmlNoData = new HtmlGenericControl("center");
                LiteralControl liNoData = new LiteralControl(eResApp.GetRes(_ePref, 6470));    //Pas de données pour ce graphique
                htmlNoData.Controls.Add(liNoData);
                _pnChart.Controls.Add(htmlNoData);
                return true;
            }

            if (_bDisplayDetail && _bDisplayChart)
            {
                Panel pnOpenDetail = new Panel();

                pnOpenDetail.ID = String.Concat(DIV_ID_BASE, DivSuffix, "dispDtl");
                pnOpenDetail.CssClass = "statsRenderSep";
                pnOpenDetail.Attributes.Add("onclick", String.Concat("displayDetails('", DivSuffix, "')"));
                pnOpenDetail.Attributes.Add("ednisdisplayed", "1");

                Label labelIcon = new Label();
                labelIcon.CssClass = "icon-develop";
                pnOpenDetail.Controls.Add(labelIcon);

                Label labelSep = new Label();
                labelSep.CssClass = "sepTitle";
                pnOpenDetail.Controls.Add(labelSep);

                PgContainer.Controls.Add(pnOpenDetail);
            }

            if (_bDisplayDetail)
            {
                _pnDetails = new Panel();
                PgContainer.Controls.Add(_pnDetails);
                _pnDetails.ID = String.Concat(DIV_ID_BASE, DivSuffix, "Dtl");
                _pnDetails.CssClass = "data";

                _tbDetails = new Table();
                _pnDetails.Controls.Add(_tbDetails);
            }

            return true;
        }

        /// <summary>
        /// Initialisation du FusionChart
        /// </summary>
        /// <returns></returns>
        private Boolean InitChartXml()
        {
            if (!_bDisplayChart)
                return true;

            _xmlRoot = _xmlChart.CreateElement("chart");
            _xmlChart.AppendChild(_xmlRoot);

            //Label tronqué remplacé par des ellipse
            XmlAttribute xmlAttribute = _xmlChart.CreateAttribute("manageLabelOverflow");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "1";

            xmlAttribute = _xmlChart.CreateAttribute("baseFontSize");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "9";

            xmlAttribute = _xmlChart.CreateAttribute("bgColor");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "FFFFFF";

            //xmlAttribute = _xmlChart.CreateAttribute("outCnvBaseFontColor");
            //_xmlRoot.Attributes.Append(xmlAttribute);
            //xmlAttribute.InnerText = "FFFFFF";

            xmlAttribute = _xmlChart.CreateAttribute("showBorder");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "0";

            xmlAttribute = _xmlChart.CreateAttribute("animation");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "1";


            xmlAttribute = _xmlChart.CreateAttribute("showValues");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = "1";


            xmlAttribute = _xmlChart.CreateAttribute("totalValues");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = _nbTotal.ToString();


            xmlAttribute = _xmlChart.CreateAttribute("xrmThemeColor");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = _ePref.ThemeXRM.Color;


            xmlAttribute = _xmlChart.CreateAttribute("showPercentValues");
            _xmlRoot.Attributes.Append(xmlAttribute);
            xmlAttribute.InnerText = _sChartFormat.Substring(0, 3).ToLower() == "pie" ? "1" : "0";


            if (_sChartFormat.Substring(0, 3).ToLower() == "pie")
            {
                xmlAttribute = _xmlChart.CreateAttribute("numberSuffix");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "%";

                xmlAttribute = _xmlChart.CreateAttribute("forceDecimals");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "2";

                xmlAttribute = _xmlChart.CreateAttribute("decimals");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "2";


                xmlAttribute = _xmlChart.CreateAttribute("decimalSeparator");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = ",";

                xmlAttribute = _xmlChart.CreateAttribute("enableRotation");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "1";

                xmlAttribute = _xmlChart.CreateAttribute("pieYScale");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "30";

                xmlAttribute = _xmlChart.CreateAttribute("pieSliceDepth");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "8";

                xmlAttribute = _xmlChart.CreateAttribute("startingAngle");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "295";

                //Margin
                xmlAttribute = _xmlChart.CreateAttribute("chartLeftMargin");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("chartRightMargin");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("chartTopMargin");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("chartBottomMargin");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("captionPadding");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";


                xmlAttribute = _xmlChart.CreateAttribute("xAxisNamePadding");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";


                xmlAttribute = _xmlChart.CreateAttribute("yAxisNamePadding");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("labelPadding");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";

                xmlAttribute = _xmlChart.CreateAttribute("canvasPadding");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";
            }

            if (_sChartFormat.ToLower().IndexOf("column") >= 0)
            {
                xmlAttribute = _xmlChart.CreateAttribute("useRoundEdges");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "1";

                xmlAttribute = _xmlChart.CreateAttribute("showValues");
                _xmlRoot.Attributes.Append(xmlAttribute);
                xmlAttribute.InnerText = "0";
            }


            if (_bMulti)
            {
                _xmlCategories = _xmlChart.CreateElement("categories");
                _xmlRoot.AppendChild(_xmlCategories);

                _xmlDataSet = _xmlChart.CreateElement("dataset");
                _xmlRoot.AppendChild(_xmlDataSet);

                if (_sChartFormat.ToLower() == "mscombi3d")
                {
                    xmlAttribute = _xmlChart.CreateAttribute("ID");
                    xmlAttribute.Value = "dataset";
                    _xmlDataSet.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("renderAs");
                    xmlAttribute.Value = "line";
                    _xmlDataSet.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("color");
                    xmlAttribute.Value = "BB1515";
                    _xmlDataSet.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("drawAnchors");
                    xmlAttribute.Value = "0";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("XYWallDepth");
                    xmlAttribute.Value = "5";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("YZWallDepth");
                    xmlAttribute.Value = "5";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("ZXWallDepth");
                    xmlAttribute.Value = "5";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("zdepth");
                    xmlAttribute.Value = "120";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("divLineEffect");
                    xmlAttribute.Value = "bevel";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("showPlotBorder");
                    xmlAttribute.Value = "0";
                    _xmlRoot.Attributes.Append(xmlAttribute);


                    xmlAttribute = _xmlChart.CreateAttribute("dynamicShading");
                    xmlAttribute.Value = "1";
                    _xmlRoot.Attributes.Append(xmlAttribute);


                    xmlAttribute = _xmlChart.CreateAttribute("exeTime");
                    xmlAttribute.Value = "1.2";
                    _xmlRoot.Attributes.Append(xmlAttribute);


                    xmlAttribute = _xmlChart.CreateAttribute("startAngX");
                    xmlAttribute.Value = "0";
                    _xmlRoot.Attributes.Append(xmlAttribute);


                    xmlAttribute = _xmlChart.CreateAttribute("startAngY");
                    xmlAttribute.Value = "0";
                    _xmlRoot.Attributes.Append(xmlAttribute);


                    xmlAttribute = _xmlChart.CreateAttribute("endAngX");
                    xmlAttribute.Value = "5";
                    _xmlRoot.Attributes.Append(xmlAttribute);

                    xmlAttribute = _xmlChart.CreateAttribute("endAngY");
                    xmlAttribute.Value = "-25";
                    _xmlRoot.Attributes.Append(xmlAttribute);



                }
            }

            return true;
        }


        /// <summary>
        /// initialise le tableau contenant le détail des données
        /// </summary>
        /// <returns></returns>
        private Boolean InitDetailsTable()
        {
            if (!_bDisplayDetail)
                return true;

            #region Initialisation du Tableau de détails


            //_tb.ID = "TbDetails";
            _tbDetails.Attributes.Add("class", "data");

            TableRow trHead = new TableRow();
            trHead.Attributes.Add("class", "data-head");
            _tbDetails.Rows.Add(trHead);

            // libellé du champ
            TableCell tdHead = new TableCell();
            tdHead.Controls.Add(new LiteralControl(_head1));
            tdHead.Attributes.Add("class", "dispVal");
            trHead.Cells.Add(tdHead);

            // nb de fiche
            tdHead = new TableCell();
            tdHead.Controls.Add(new LiteralControl(_head2));
            tdHead.Attributes.Add("class", "count");
            trHead.Cells.Add(tdHead);

            //tdHead.Controls.Add(new LiteralControl("   "));

            //HtmlImage imgSortCtrl = new HtmlImage();
            //imgSortCtrl.Attributes.Add("class", "rIco SortAsc picto");
            //imgSortCtrl.Attributes.Add("src", eConst.GHOST_IMG);
            //imgSortCtrl.Attributes.Add("onclick", "doSort('1')");
            //imgSortCtrl.Attributes.Add("id", "IMG_ASC");
            //tdHead.Controls.Add(imgSortCtrl);

            //imgSortCtrl = new HtmlImage();
            //imgSortCtrl.Attributes.Add("class", "rIco SortDesc picto");
            //imgSortCtrl.Attributes.Add("src", eConst.GHOST_IMG);
            //imgSortCtrl.Attributes.Add("onclick", "doSort('0')");
            //imgSortCtrl.Attributes.Add("id", "IMG_DESC");
            //imgSortCtrl.Attributes.Add("style", "visibility:hidden");
            //tdHead.Controls.Add(imgSortCtrl);


            // pourcentage
            tdHead = new TableCell();
            tdHead.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 6228)));
            tdHead.Attributes.Add("class", "pct");
            trHead.Cells.Add(tdHead);
            #endregion

            return true;

        }

        /// <summary>
        /// affiche le total au pied du tableau
        /// </summary>
        /// <returns></returns>
        private Boolean EndDetailsTable()
        {
            if (!_bDisplayDetail)
                return true;

            #region detail : pied du tableau

            TableRow trTotal = new TableRow();
            trTotal.Attributes.Add("class", "total");
            _tbDetails.Rows.Add(trTotal);

            TableCell _tdTotal = new TableCell();
            _tdTotal.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 1278)));
            _tdTotal.Attributes.Add("class", "dispVal");
            trTotal.Cells.Add(_tdTotal);

            _tdTotal = new TableCell();
            _tdTotal.Controls.Add(new LiteralControl(_nbTotal.ToString()));
            _tdTotal.Attributes.Add("class", "count");
            trTotal.Cells.Add(_tdTotal);

            _tdTotal = new TableCell();
            trTotal.Cells.Add(_tdTotal);

            #endregion

            return true;

        }

        /// <summary>
        /// Génère le contenu
        /// </summary>
        /// <returns></returns>
        private Boolean FillContent()
        {
            int i = 0;

            XmlAttribute xmlAttribute;
            float fSum = _diData.Count > 0 ? _diData.Sum(k => k.Value) : 0;

            foreach (KeyValuePair<string, int> kvp in _diData)
            {
                Double dPercent = (Double)kvp.Value / (Double)_nbTotal;
                Int32 iKvpValue = kvp.Value;

                String sLabel = kvp.Key;
                String sToolTip = String.Empty;

                String[] tabLabel = sLabel.Split(EudoQuery.SEPARATOR.LVL1);
                if (tabLabel.Length > 1)    //Gestion d'un tooltip
                {
                    sLabel = tabLabel[0];
                    sToolTip = tabLabel[1];
                }
                if (_bDisplayChart)
                {
                    #region Chart
                    String sDisplayValue = String.Empty;
                    if (_sValToDispOnChart.ToLower() == "pct")
                        sDisplayValue = (dPercent * 100).ToString("F").Replace(',', '.');
                    else
                        sDisplayValue = kvp.Value.ToString("F").Replace(',', '.');

                    XmlNode xmlSet = _xmlChart.CreateElement("set");
                    if (_bMulti)
                    {
                        XmlNode xmlCategory = _xmlChart.CreateElement("category");
                        _xmlCategories.AppendChild(xmlCategory);

                        _xmlDataSet.AppendChild(xmlSet);


                        if (sToolTip.Length > 0)    //Gestion d'un tooltip
                        {
                            xmlAttribute = _xmlChart.CreateAttribute("toolText");
                            xmlCategory.Attributes.Append(xmlAttribute);
                            xmlAttribute.InnerText = String.Concat(sToolTip, ", ", sDisplayValue);
                        }

                        xmlAttribute = _xmlChart.CreateAttribute("label");
                        xmlCategory.Attributes.Append(xmlAttribute);
                        xmlAttribute.InnerText = (sLabel == "") ? eResApp.GetRes(_ePref, 141) : sLabel;

                        xmlAttribute = _xmlChart.CreateAttribute("total");
                        xmlCategory.Attributes.Append(xmlAttribute);
                        xmlAttribute.InnerText = _nbTotal.ToString("F").Replace(',', '.');
                    }
                    else
                    {
                        _xmlRoot.AppendChild(xmlSet);

                        if (sToolTip.Length > 0)    //Gestion d'un tooltip
                        {
                            xmlAttribute = _xmlChart.CreateAttribute("toolText");
                            xmlSet.Attributes.Append(xmlAttribute);
                            xmlAttribute.InnerText = String.Concat(sToolTip, ", ", sDisplayValue);
                        }
                        xmlAttribute = _xmlChart.CreateAttribute("label");
                        xmlSet.Attributes.Append(xmlAttribute);
                        xmlAttribute.InnerText = (sLabel == "") ? eResApp.GetRes(_ePref, 141) : sLabel;
                    }

                    xmlAttribute = _xmlChart.CreateAttribute("value");
                    xmlSet.Attributes.Append(xmlAttribute);
                    xmlAttribute.InnerText = iKvpValue.ToString();

                    xmlAttribute = _xmlChart.CreateAttribute("text");
                    xmlSet.Attributes.Append(xmlAttribute);
                    xmlAttribute.InnerText = ((iKvpValue / fSum) * 100).ToString("00") + "%";

                    if (_sChartFormat.ToLower() == "mscombi3d")
                    {
                        //_xmlDataSet = _xmlChart.GetElementById("dataset");
                        xmlAttribute = _xmlChart.CreateAttribute("seriesName");

                        xmlAttribute.InnerText = (sLabel == "") ? eResApp.GetRes(_ePref, 141) : sLabel;
                        _xmlDataSet.Attributes.Append(xmlAttribute);
                    }



                    #endregion
                }


                if (_bDisplayDetail)
                {
                    #region Details

                    TableRow tr = new TableRow();

                    if (i % 2 == 0)
                        tr.Attributes.Add("class", "dataLine");
                    else
                        tr.Attributes.Add("class", "dataLine2");

                    _tbDetails.Rows.Add(tr);

                    // valeur du champ
                    TableCell td = new TableCell();
                    td.Controls.Add(new LiteralControl(sLabel));
                    td.Attributes.Add("class", "dispVal");
                    if (sToolTip.Length > 0)    //Gestion d'un tooltip
                    {
                        td.Attributes.Add("title", sToolTip);
                    }
                    tr.Cells.Add(td);

                    // nb de fiche
                    td = new TableCell();
                    td.Controls.Add(new LiteralControl(kvp.Value.ToString("# ###")));
                    td.Attributes.Add("class", "count");
                    tr.Cells.Add(td);

                    // pourcentage
                    td = new TableCell();
                    td.Controls.Add(new LiteralControl(dPercent.ToString("P")));
                    td.Attributes.Add("class", "pct");
                    tr.Cells.Add(td);
                    #endregion

                }

                i++;
            }

            return true;
        }




    }
}