using Com.Eudonet.Internal;
using EudoQuery;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Grid;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using static Com.Eudonet.Core.Model.eCommunChart;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eChartManagerExport
    /// </summary>
    public class eChartManagerExport : eEudoManager
    {
        PdfPen _transparentPen;
        Int32? _nTab = 0;
        float _margin = 40f;
        string _chartExpressFilter = string.Empty;
        PdfFont _smallFont;
        PdfFont _bigFont;
        PdfPaddings _padding = new PdfPaddings(3, 3, 5, 5);
        private CHART_TYPE _typeOfCurrentChart;
        private Color _themColor;
        private float _chartWidth;
        private float _chartHeight;
        private const string THEMCOLOR = "#C62D42";
        private const string WHITECOLORNAME = "ffffffff";
        private const string EXPORTCHARTFILENAME = "ChartExport";
        private const float PAGEHEIGHT = 842;
        private const float PAGEWIDTH = 595;
        private const float PDFPAGEX = 10;
        private const float PDFPAGEY = 10;
        private const Int32 TRIMGRAPH = 0;
        private float _chartX = PDFPAGEX;
        private float _chartY = PDFPAGEY;
        private float _pageY = PDFPAGEY;
        PdfStringFormat _strformat;
        PdfBrush _titleBrush;
        private bool _bMerging = false;
        private bool _b3dChart = false;
        private string _tableLibelle = string.Empty;
        private string _tableFirstColumnName = string.Empty;
        private string _tableFirstColumnValue = string.Empty;
        private string _tableFirstColumnPercent = string.Empty;
        private string _tableFirstColumnCombinedValue = string.Empty;
        private string _tableFirstColumnCombinedPercent = string.Empty;
        private string[] _chartCategoryLabels;
        private object[] _chartCategoryValues;
        private Field _field;
        private Field _combinedField;
        private Int32 _nbDecimal = 0;
        private Int32 _nbCombinedFieldDecimal = 0;
        TypeAgregatFonction _operationType = TypeAgregatFonction.COUNT;
        ExpressFilter[] _dataExpressFilter = null;
        string _fileName = string.Empty;

        /// <summary>
        /// Retourne le modèle d'excel par rapport à la version d'office parametrè par l'utilisateur 
        /// </summary>
        private string GetExcelChartTemplate
        {
            get
            {
                switch (_typeOfCurrentChart)
                {
                    case CHART_TYPE.UNKNOWN:
                        return "Column";
                    case CHART_TYPE.PIE:
                        return string.Concat("Pie", !_b3dChart ? "" : "3D");
                    case CHART_TYPE.STACKINGBAR:
                    case CHART_TYPE.BAR:
                        return string.Concat("Bar", !_b3dChart ? "" : "3D");
                    case CHART_TYPE.PYRAMID:
                    case CHART_TYPE.COLUMN:
                    case CHART_TYPE.FUNNEL:
                        return string.Concat("Column", !_b3dChart ? "" : "3D");
                    case CHART_TYPE.AREA:
                    case CHART_TYPE.STACKINGAREA:
                        return string.Concat("Area", !_b3dChart ? "" : "3D");
                    case CHART_TYPE.LINE:
                    case CHART_TYPE.SPLINE:
                        return string.Concat("Line", !_b3dChart ? "" : "3D");
                    case CHART_TYPE.STACKINGCOLUMN:
                        return "ColumnStacked";
                    case CHART_TYPE.DOUGHNUT:
                        return "Dougnout";
                    default:
                        return string.Concat("Column", !_b3dChart ? "" : "3D");
                }
            }

        }

        /// <summary>
        /// Retourne la version d'office du client
        /// </summary>
        private ExcelVersion GetOfficeVersion
        {
            get
            {
                switch (_pref.GetConfig(eLibConst.PREF_CONFIG.OFFICERELEASE))
                {
                    case "10":
                        return ExcelVersion.Excel2010;
                    case "12":
                    case "13":
                    case "14":
                        return ExcelVersion.Excel2013;
                    case "15":
                    case "16":
                    case "17":
                        return ExcelVersion.Excel2016;
                    case "8":
                    case "9":
                        return ExcelVersion.Excel97to2003;
                    default:
                        return ExcelVersion.Excel2013;
                }
            }

        }
        /// <summary>
        /// Manager d'export de graphique
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 iLang = 0;
            if (_pref != null)
                iLang = _pref.LangId;
            try
            {
                _nTab = _requestTools.GetRequestFormKeyI("nTab");
                _chartExpressFilter = _requestTools.GetRequestFormKeyS("expressFilterElements");


                if (!string.IsNullOrEmpty(_chartExpressFilter))
                    _dataExpressFilter = GetExpressFilter(_chartExpressFilter);

                if (_nTab.HasValue)
                {
                    _tableLibelle = renderer.eSyncFusionChartRenderer.GetTableNameForExportChart(_pref, _nTab.Value);
                    _fileName = string.Concat(EXPORTCHARTFILENAME, "_", Regex.Replace(_tableLibelle, @"\s+", ""), "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"));
                }
                else
                    throw new Exception("Erreur sur l'export : Id de la table est inconnu");


                this._themColor = ColorTranslator.FromHtml(_pref.ThemeXRM.Color);
                if (this._themColor.Name == WHITECOLORNAME)
                {
                    this._themColor = ColorTranslator.FromHtml(THEMCOLOR);
                }

                _titleBrush = new PdfSolidBrush(new PdfColor(this._themColor.R, this._themColor.G, this._themColor.B));
                _strformat = new PdfStringFormat() { WordWrap = PdfWordWrapType.Word, LineLimit = true, Alignment = PdfTextAlignment.Center };

                if (_requestTools.GetRequestFormKeyS("chartType") != "0")
                    SaveChart();
                else
                    SaveCircularGauge();
            }
            catch (Exception ex)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", HttpContext.Current.Request.Url.Segments[HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(iLang, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(iLang, 422), "<br>", eResApp.GetRes(iLang, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(iLang, 72),  //   titre
                       String.Concat(sDevMsg));

                LaunchErrorHTML(true, ErrorContainer, "this.close()");

            }


        }
        /// <summary>
        /// Exporter
        /// </summary>
        private void SaveChart()
        {
            double _dataSomme = 0;
            double _dataRowValue = 0;
            Bitmap _merge = null;
            GridLine[] _data = null;
            GridLine[] _dataTemp = null;
            GridLine[] _dataColumn = null;

            GridSortingDirection _sort = new GridSortingDirection();
            PdfFont _font = new PdfTrueTypeFont(new Font("Segoe UI", 8f), true);
            //PdfStandardFont _font = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);


            PdfBrush _grayBrush = new PdfSolidBrush(new PdfColor(170, 171, 171));
            PdfStringFormat _centredText = new PdfStringFormat()
            {
                LineAlignment = PdfVerticalAlignment.Middle,
                Alignment = PdfTextAlignment.Center
            };

            PdfStringFormat textMiddleVerticalAlignement = new PdfStringFormat();
            textMiddleVerticalAlignement.LineAlignment = PdfVerticalAlignment.Middle;

            //_bigFont = new PdfStandardFont(_font, 12f);
            //_smallFont = new PdfStandardFont(_font, 9f);
            _bigFont = new PdfTrueTypeFont(new Font("Segoe UI", 12f), true);
            _smallFont = new PdfTrueTypeFont(new Font("Segoe UI", 9f), true);
            string _dataChart = _requestTools.GetRequestFormKeyS("dataChart");
            string _dataLegend = _requestTools.GetRequestFormKeyS("dataLegend");
            string _type = _requestTools.GetRequestFormKeyS("format");
            string _gridDataSource = _requestTools.GetRequestFormKeyS("gridDataSource");
            string _gridDataColumns = _requestTools.GetRequestFormKeyS("gridDataColumns");
            string _gridDataSorting = _requestTools.GetRequestFormKeyS("gridDataSorting");
            string _dataChartType = _requestTools.GetRequestFormKeyS("chartType");
            string _chartHeight = _requestTools.GetRequestFormKeyS("chartHeight");
            string _chartWidth = _requestTools.GetRequestFormKeyS("chartWidth");
            string _chartTitle = _requestTools.GetRequestFormKeyS("chartTitle");


            Int32? _nField = _requestTools.GetRequestFormKeyI("nField");
            Int32? _nCombinedField = _requestTools.GetRequestFormKeyI("nCombinedField");
            Int32? _nOperationType = _requestTools.GetRequestFormKeyI("nOperationType");
            Boolean? _bStat = _requestTools.GetRequestFormKeyB("bStat");
            _b3dChart = _requestTools.GetRequestFormKeyB("chart3D").HasValue ? _requestTools.GetRequestFormKeyB("chart3D").Value : false;


            string _primarySerieAxis = _requestTools.GetRequestFormKeyS("primarySerieAxis");
            string _primaryValueAxis = _requestTools.GetRequestFormKeyS("primaryValueAxis");



            CHART_EXPORT_TYPE _format;
            Enum.TryParse(_type.ToUpper(), out _format);


            if (_dataChart.Length == 0 || _dataLegend.Length == 0)
                throw new Exception(eResApp.GetRes(_pref.LangId, 8176));


            if (!String.IsNullOrEmpty(_dataChartType))
                Enum.TryParse(_dataChartType.ToUpper(), out _typeOfCurrentChart);

            if (_typeOfCurrentChart == CHART_TYPE.UNKNOWN)
                _typeOfCurrentChart = CHART_TYPE.COLUMN;

            _dataChart = _dataChart?.Remove(0, _dataChart.IndexOf(',') + 1);
            _dataLegend = _dataLegend?.Remove(0, _dataLegend.IndexOf(',') + 1);

            if (!String.IsNullOrEmpty(_gridDataSorting))
            {
                _sort = GetGridSorting(_gridDataSorting);
            }




            if (_nOperationType.HasValue)
                Enum.TryParse(_nOperationType.Value.ToString(), out _operationType);

            if (_nField.HasValue && _nTab.HasValue)
            {
                // si différent de nombre de fiche
                if (_operationType != TypeAgregatFonction.COUNT)
                    _field = renderer.eSyncFusionChartRenderer.GetFieldInfoFromDescid(_pref, _nTab.Value, _nField.Value);
                _nbDecimal = GetFieldLEnght(_field);
            }

            if (_nCombinedField.HasValue)
            {
                // si différent de nombre de fiche
                if (_operationType != TypeAgregatFonction.COUNT)
                    _combinedField = renderer.eSyncFusionChartRenderer.GetFieldInfoFromDescid(_pref, _nCombinedField.Value - (_nCombinedField.Value % 100), _nCombinedField.Value);
                _nbCombinedFieldDecimal = GetFieldLEnght(_combinedField);
            }

            // récupération des données
            if (!string.IsNullOrEmpty(_gridDataSource))
            {
                _dataColumn = SetDataGridFirstLine(_gridDataColumns);
                _dataTemp = GetAllLines(_gridDataSource, _sort);
                _data = new GridLine[_dataColumn.Length + _dataTemp.Length];
                Array.Copy(_dataColumn, _data, _dataColumn.Length);
                Array.Copy(_dataTemp, 0, _data, _dataColumn.Length, _dataTemp.Length);
            }


            MemoryStream streamChart = new MemoryStream(Convert.FromBase64String(_dataChart));
            MemoryStream streamLegend = new MemoryStream(Convert.FromBase64String(_dataLegend));

            Bitmap mapDatachart = (Bitmap)Bitmap.FromStream(streamChart);
            Bitmap mapDataLegend = (Bitmap)Bitmap.FromStream(streamLegend);

            switch (_format)
            {
                #region PDF

                case CHART_EXPORT_TYPE.PDF:
                    CultureInfo c = eLibTools.GetCultureFromLang(_pref);

                    _transparentPen = new PdfPen(new PdfColor(Color.FromArgb(Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B)), .3f);
                    _transparentPen.LineCap = PdfLineCap.Square;
                    PdfDocument pdfDoc = new PdfDocument();

                    pdfDoc.PageSettings.Orientation = PdfPageOrientation.Landscape;
                    pdfDoc.PageSettings.Margins.All = 5;

                    PdfPage page = pdfDoc.Pages.Add();

                    if (_bStat.HasValue && _bStat.Value)
                    {
                        if (!String.IsNullOrEmpty(_tableFirstColumnValue) && !String.IsNullOrEmpty(_tableFirstColumnName))
                            SetTableTitle(page, eResApp.GetRes(_pref, 8251).Replace("<OPERATOR>", _tableFirstColumnValue).Replace("<FIELD>", _tableFirstColumnName), 20);
                        if (!String.IsNullOrEmpty(_tableLibelle))
                            SetTableTitle(page, eResApp.GetRes(_pref, 8252).Replace("<TABLE>", _tableLibelle), 35);

                    }



                    if (eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.GraphExpressFilter))
                    {
                        if (_dataExpressFilter != null && _dataExpressFilter.Length > 0)
                        {
                            DrawExpressFilter(page, _dataExpressFilter, (int)this._pageY);
                            this._pageY = this._pageY + (_dataExpressFilter.Length * PDFPAGEY);
                        }
                    }


                    if (mapDataLegend.Height < PAGEHEIGHT * 0.33)
                        _merge = MergeTwoImages((Bitmap)Bitmap.FromStream(streamChart), (Bitmap)Bitmap.FromStream(streamLegend), this._typeOfCurrentChart);

                    if (_merge == null)
                    {
                        //Récuperer les dimensions 
                        SetPdfChartSize(mapDatachart);

                        // PdfPage page = pdfDoc.Pages.Add();

                        PdfGraphics graphics = page.Graphics;



                        graphics.DrawImage(PdfImage.FromImage(TrimIMage(mapDatachart, 0, 0, mapDatachart.Width - TRIMGRAPH, mapDatachart.Height - TRIMGRAPH)),
                            new RectangleF(this._chartX, this._chartY, this._chartWidth, this._chartHeight));


                        page = pdfDoc.Pages.Add();
                        //Récuperer les dimensions 
                        SetPdfChartSize(mapDataLegend);

                        graphics = page.Graphics;
                        graphics.DrawImage(PdfImage.FromImage(mapDataLegend),
                            new RectangleF(this._chartX, this._chartY, this._chartWidth, this._chartHeight));
                    }
                    else
                    {
                        _bMerging = true;
                        //Récuperer les dimensions 
                        SetPdfChartSize(_merge);


                        // PdfPage page = pdfDoc.Pages.Add();

                        PdfGraphics graphics = page.Graphics;
                        graphics.DrawImage(PdfImage.FromImage(TrimIMage(_merge, 0, 0, _merge.Width - TRIMGRAPH, _merge.Height - TRIMGRAPH)),
                            new RectangleF(this._chartX, this._chartY, this._chartWidth, this._chartHeight));
                    }



                    if (_data != null && _data.Length > 0)
                    {
                        pdfDoc.PageSettings.Orientation = PdfPageOrientation.Portrait;
                        page = pdfDoc.Pages.Add();

                        #region Titre du tableau

                        if (!String.IsNullOrEmpty(_tableFirstColumnValue) && !String.IsNullOrEmpty(_tableFirstColumnName))
                            SetTableTitle(page, eResApp.GetRes(_pref, 8251).Replace("<OPERATOR>", _tableFirstColumnValue).Replace("<FIELD>", _tableFirstColumnName), 20);
                        if (!String.IsNullOrEmpty(_tableLibelle))
                            SetTableTitle(page, eResApp.GetRes(_pref, 8252).Replace("<TABLE>", _tableLibelle), 35);

                        #endregion

                        #region PdfGrid

                        PdfGrid pdfGrid = new PdfGrid();
                        #region Ajout du header
                        //Affichage du header 
                        GridColumn[] columns = GetGridColumns(_gridDataColumns);
                        if (columns.Length > 0)
                        {
                            pdfGrid.Columns.Add(columns.Length);
                            pdfGrid.Headers.Add(1);
                            PdfGridRow pdfGridHeader = pdfGrid.Headers[0];
                            pdfGridHeader.Style = GetPdfGridRowStyle(PdfBrushes.Gray, PdfBrushes.White, new PdfStandardFont(PdfFontFamily.Courier, 14));

                            for (int j = 0; j < columns.Length; j++)
                            {
                                if (columns[j].visible)
                                {
                                    SetCellStyle(pdfGridHeader.Cells[j], _bigFont, columns[j].headerText, j != 0 ? _centredText : textMiddleVerticalAlignement);
                                    if (j > 0)
                                        pdfGrid.Columns[j].Width = columns[j].width;
                                }
                            }
                        }

                        #endregion

                        #region Ajout des lignes
                        //Ajout des lignes.
                        PdfGridRow pdfGridRow;

                        //On ajoute autant de ligne que d'elements dans le tableau -1
                        for (int i = 1; i < _data.Length; i++)
                        {
                            pdfGridRow = pdfGrid.Rows.Add();
                        }

                        bool bCombined = false;
                        double _combinedDataSomme = 0;
                        //on enlève la premiere ligne qui est ajoutée dans le header de la grid
                        for (int i = 1; i < _data.Length; i++)
                        {

                            //if ((i - 1) % 2 == 0)
                            //    pdfGrid.Rows[i - 1].Style = GetPdfGridRowStyle(PdfBrushes.WhiteSmoke, titleBrush, new PdfStandardFont(PdfFontFamily.Courier, 10));
                            if (double.TryParse(_data[i].value.Replace(".", ","), out _dataRowValue))
                                _dataSomme += _dataRowValue;

                            SetCellStyle(pdfGrid.Rows[i - 1].Cells[0], _smallFont, _data[i].name);
                            SetCellStyle(pdfGrid.Rows[i - 1].Cells[1], _smallFont, eNumber.FormatNumber(_dataRowValue, new eNumber.DecimalParam(_pref) { NumberDigitMin = _nbDecimal }, new eNumber.SectionParam(_pref)));

                            SetCellStyle(pdfGrid.Rows[i - 1].Cells[2], _smallFont, string.Concat(eNumber.FormatNumber(double.Parse((_data[i].GetPercentValue * 100).ToString()), new eNumber.DecimalParam(_pref) { NumberDigitMin = 2 }, new eNumber.SectionParam(_pref)), "%"));

                            if (!string.IsNullOrEmpty(_data[i].combinedpercent))
                            {
                                bCombined = true;
                                double combinedValue = 0;
                                if (double.TryParse(_data[i].combinedvalue.Replace(".", ","), out combinedValue))
                                    _combinedDataSomme += combinedValue;

                                SetCellStyle(pdfGrid.Rows[i - 1].Cells[3], _smallFont, eNumber.FormatNumber(combinedValue, new eNumber.DecimalParam(_pref) { NumberDigitMin = _nbCombinedFieldDecimal }, new eNumber.SectionParam(_pref)));

                                SetCellStyle(pdfGrid.Rows[i - 1].Cells[4], _smallFont, string.Concat(eNumber.FormatNumber(double.Parse((_data[i].GetCombinedPercentValue * 100).ToString()), new eNumber.DecimalParam(_pref) { NumberDigitMin = 2 }, new eNumber.SectionParam(_pref)), "%"));
                            }



                        }

                        #region La ligne TOTAL
                        pdfGridRow = pdfGrid.Rows.Add();
                        pdfGridRow.Style = GetPdfGridRowStyle(PdfBrushes.Gray, PdfBrushes.White, new PdfStandardFont(PdfFontFamily.Courier, 14));
                        SetCellStyle(pdfGrid.Rows[_data.Length - 1].Cells[0], _bigFont, eResApp.GetRes(_pref, 631));
                        SetCellStyle(pdfGrid.Rows[_data.Length - 1].Cells[1], _bigFont, eNumber.FormatNumber(_dataSomme, new eNumber.DecimalParam(_pref) { NumberDigitMin = _nbDecimal }, new eNumber.SectionParam(_pref)));

                        pdfGrid.Rows[_data.Length - 1].Cells[1].ColumnSpan = 2;

                        if (bCombined)
                        {
                            SetCellStyle(pdfGrid.Rows[_data.Length - 1].Cells[3], _bigFont, eNumber.FormatNumber(_combinedDataSomme, new eNumber.DecimalParam(_pref) { NumberDigitMin = _nbCombinedFieldDecimal }, new eNumber.SectionParam(_pref)));

                            pdfGrid.Rows[_data.Length - 1].Cells[3].ColumnSpan = 2;
                        }

                        #endregion

                        #endregion

                        PdfGridLayoutFormat gridLayoutFormat = new PdfGridLayoutFormat();
                        gridLayoutFormat.Layout = PdfLayoutType.Paginate;
                        gridLayoutFormat.Break = PdfLayoutBreakType.FitPage;

                        pdfGrid.Draw(page, new RectangleF(new PointF(_margin, 70), new SizeF(page.Graphics.ClientSize.Width - _margin, page.Graphics.ClientSize.Height - _margin)), gridLayoutFormat);


                        #endregion
                    }


                    pdfDoc.Save(_context.Response.OutputStream);
                    pdfDoc.Close(true);

                    streamChart.Close();
                    streamChart.Dispose();
                    streamLegend.Close();
                    streamLegend.Dispose();

                    mapDatachart.Dispose();
                    mapDataLegend.Dispose();

                    if (_merge != null)
                        _merge.Dispose();

                    _context.Response.ContentType = "application/octet-stream";
                    _context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename=" + _fileName + "." + _format.ToString()));
                    _context.Response.Flush();
                    break;

                #endregion

                #region EXCEL
                case CHART_EXPORT_TYPE.XLS:
                    string directoryPath = string.Empty;
                    string templateName = GetExcelChartTemplate + "Chart.xlsx";
                    string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Charts\template\" + templateName);

                    //On utilise un modèle temporaire
                    string template = CreateTemplateWork(templateName, templatePath, out directoryPath);
                    if (string.IsNullOrEmpty(template))
                        template = templatePath;
                    ExcelEngine excelEngine = new ExcelEngine();
                    IApplication application = excelEngine.Excel;
                    IWorkbook workbook = application.Workbooks.Open(template, ExcelOpenType.Automatic);
                    string nbFormat = GetExcelNbFormat();

                    if (_data != null && _data.Length > 0)
                    {
                        bool bCombined = false;
                        application.DefaultVersion = this.GetOfficeVersion;
                        IWorksheet worksheet = workbook.Worksheets[0];
                        worksheet.DeleteRow(_data.Length, 256);
                        //bool b = worksheet.AutoFilters[1].IsFiltered;

                        for (int i = 0; i < _data.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(_data[i].combinedpercent))
                                bCombined = true;


                            worksheet.Range["A" + (i + 1).ToString()].Text = _data[i].name;
                            if (i > 0)
                            {
                                worksheet.Range["B" + (i + 1).ToString()].Number = double.Parse(_data[i].value.Replace(".", ","));
                                worksheet.Range["C" + (i + 1).ToString()].Number = double.Parse((_data[i].GetPercentValue * 100).ToString());

                                if (bCombined)
                                {
                                    worksheet.Range["D" + (i + 1).ToString()].Number = double.Parse(_data[i].combinedvalue.Replace(".", ","));
                                    worksheet.Range["E" + (i + 1).ToString()].Number = double.Parse((_data[i].GetCombinedPercentValue * 100).ToString());
                                }
                            }

                            else
                            {
                                worksheet.Range["B" + (i + 1).ToString()].Text = _data[i].value;
                                worksheet.Range["C" + (i + 1).ToString()].Text = _data[i].percent;

                                if (bCombined)
                                {
                                    worksheet.Range["D" + (i + 1).ToString()].Text = _data[i].combinedvalue;
                                    worksheet.Range["E" + (i + 1).ToString()].Text = _data[i].combinedpercent;
                                }
                            }



                        }

                        worksheet.Range["B2:B" + _data.Length.ToString()].NumberFormat = nbFormat;
                        worksheet.Range["C2:C" + _data.Length.ToString()].NumberFormat = "0.00";
                        if (bCombined)
                        {
                            worksheet.Range["D2:D" + _data.Length.ToString()].NumberFormat = nbFormat;
                            worksheet.Range["E2:E" + _data.Length.ToString()].NumberFormat = "0.00";
                        }

                        worksheet.Name = eResApp.GetRes(_pref, 1395);
                        worksheet.UsedRange.AutofitColumns();
                        worksheet.Range["B2"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

                        #region Ajout du graphique
                        IChart chart = workbook.Charts[0];
                        chart.DataRange = worksheet.Range["A1:B" + _data.Length.ToString()];
                        worksheet.AutoFilters.FilterRange = worksheet.Range["B1:B" + (_data.Length).ToString()];
                        if (worksheet.AutoFilters.Count > 0)
                        {
                            IAutoFilter filter = worksheet.AutoFilters[0];
                            filter.FirstCondition.DataType = ExcelFilterDataType.String;
                            filter.FirstCondition.ConditionOperator = ExcelFilterCondition.NotEqual;
                            filter.FirstCondition.String = "0,00";
                            filter.IsAnd = true;
                        }

                        chart.Name = eResApp.GetRes(_pref, 1005);
                        #endregion
                    }

                    workbook.SaveAs(_fileName + ".xls", ExcelSaveType.SaveAsXLS, _context.Response, ExcelDownloadType.PromptDialog);
                    workbook.Close();
                    excelEngine.Dispose();
                    //Suppression du dossier/modèle temporaire
                    Directory.Delete(directoryPath, true);
                    break;

                #endregion

                #region PNG

                case CHART_EXPORT_TYPE.PNG:
                    MemoryStream memoryStream = new MemoryStream();
                    _merge = MergeTwoImages((Bitmap)Bitmap.FromStream(streamChart), (Bitmap)Bitmap.FromStream(streamLegend));
                    _merge.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    memoryStream.WriteTo(_context.Response.OutputStream);
                    memoryStream.Close();
                    _merge.Dispose();
                    streamChart.Close();
                    streamChart.Dispose();
                    streamLegend.Close();
                    streamLegend.Dispose();

                    _context.Response.ContentType = "application/octet-stream";
                    _context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename=" + _fileName + "." + _format.ToString()));

                    _context.Response.Flush();
                    break;
                #endregion

                default:
                    throw new Exception(eResApp.GetRes(_pref.LangId, 8176));

            }


        }


        /// <summary>
        /// Export jauge circulaire
        /// </summary>
        private void SaveCircularGauge()
        {

            PdfFont _font = new PdfTrueTypeFont(new Font("Segoe UI", 8f), true);
            //PdfStandardFont _font = new PdfStandardFont(PdfFontFamily.Helvetica, 8f);
            this._themColor = ColorTranslator.FromHtml(_pref.ThemeXRM.Color);

            PdfBrush _grayBrush = new PdfSolidBrush(new PdfColor(170, 171, 171));
            PdfStringFormat _centredText = new PdfStringFormat()
            {
                LineAlignment = PdfVerticalAlignment.Middle,
                Alignment = PdfTextAlignment.Center
            };

            PdfStringFormat textMiddleVerticalAlignement = new PdfStringFormat();
            textMiddleVerticalAlignement.LineAlignment = PdfVerticalAlignment.Middle;

            //_bigFont = new PdfStandardFont(_font, 12f);
            //_smallFont = new PdfStandardFont(_font, 9f);

            _bigFont = new PdfTrueTypeFont(new Font("Segoe UI", 12f), true);
            _smallFont = new PdfTrueTypeFont(new Font("Segoe UI", 9f), true);

            string _dataChart = _requestTools.GetRequestFormKeyS("dataChart");
            string _type = _requestTools.GetRequestFormKeyS("format");
            string _title = _requestTools.GetRequestFormKeyS("title");
            CHART_EXPORT_TYPE _format;
            Enum.TryParse(_type.ToUpper(), out _format);
            _dataChart = _dataChart?.Remove(0, _dataChart.IndexOf(',') + 1);
            MemoryStream streamChart = new MemoryStream(Convert.FromBase64String(_dataChart));
            switch (_format)
            {
                #region PDF

                case CHART_EXPORT_TYPE.PDF:
                    Bitmap img = (Bitmap)Bitmap.FromStream(streamChart);
                    _typeOfCurrentChart = CHART_TYPE.CIRCULARGAUJE;
                    SetPdfChartSize(img);
                    PdfDocument pdfDoc = new PdfDocument();
                    pdfDoc.PageSettings.Orientation = PdfPageOrientation.Landscape;
                    pdfDoc.PageSettings.Margins.All = 5;
                    PdfPage page = pdfDoc.Pages.Add();

                    if (_dataExpressFilter != null && _dataExpressFilter.Length > 0)
                    {
                        DrawExpressFilter(page, _dataExpressFilter, (int)this._pageY);
                        this._pageY = this._pageY + (_dataExpressFilter.Length * PDFPAGEY);
                    }


                    if (!String.IsNullOrEmpty(_title))
                    {
                        SetTableTitle(page, _title, (int)this._pageY);
                    }


                    PdfGraphics graphics = page.Graphics;
                    graphics.DrawImage(PdfImage.FromImage(img), new RectangleF(this._chartX, this._chartY, this._chartWidth, this._chartHeight));

                    pdfDoc.Save(_context.Response.OutputStream);
                    pdfDoc.Close(true);

                    streamChart.Close();
                    streamChart.Dispose();

                    _context.Response.ContentType = "application/octet-stream";
                    _context.Response.AddHeader("Content-Disposition", String.Format("attachment;filename=" + _fileName + "." + _format.ToString()));
                    _context.Response.Flush();
                    break;

                #endregion
                default:
                    break;
            }

        }

        #region Méthodes private
        /// <summary>
        /// definir la taille de graphique exporté
        /// </summary>
        /// <param name="image"></param>
        private void SetPdfChartSize(Bitmap image)
        {
            switch (_typeOfCurrentChart)
            {
                case CHART_TYPE.DOUGHNUT:
                case CHART_TYPE.PIE:
                    if (PAGEHEIGHT > image.Height)
                    {
                        if (image.Width > PAGEWIDTH)
                        {
                            this._chartHeight = float.Parse((image.Height * (PAGEWIDTH / (image.Width * 1.1))).ToString());
                            this._chartWidth = float.Parse((image.Width * (PAGEWIDTH / (image.Width * 1.1))).ToString());
                        }
                        else
                        {
                            this._chartHeight = float.Parse((image.Height * 0.95).ToString());
                            this._chartWidth = float.Parse((image.Width * 0.95).ToString());
                        }

                    }
                    else
                    {
                        if (image.Width > PAGEWIDTH)
                        {
                            this._chartHeight = float.Parse((image.Height * ((PAGEWIDTH * 0.95) / image.Height)).ToString());
                            this._chartWidth = float.Parse((PAGEWIDTH * 0.95).ToString());
                        }
                        else
                        {
                            this._chartHeight = float.Parse((PAGEHEIGHT * 0.95).ToString());
                            this._chartWidth = float.Parse((PAGEWIDTH * 0.95).ToString());
                        }

                    }

                    break;
                case CHART_TYPE.CIRCULARGAUJE:
                    if (image.Width > PAGEHEIGHT)
                    {
                        this._chartWidth = float.Parse((PAGEHEIGHT * 0.95).ToString());
                        this._chartHeight = float.Parse((image.Height * (this._chartWidth / image.Width)).ToString());
                    }
                    else
                    {
                        if (image.Height > PAGEWIDTH)
                        {
                            this._chartHeight = float.Parse((PAGEWIDTH * 0.95).ToString());
                            this._chartWidth = float.Parse((image.Width * (this._chartHeight / image.Height)).ToString());

                        }
                        else
                        {
                            this._chartHeight = float.Parse((image.Height * 0.95).ToString());
                            this._chartWidth = float.Parse((image.Width * 0.95).ToString());
                        }
                    }
                    break;
                default:
                    if (PAGEHEIGHT > image.Width)
                    {
                        this._chartHeight = float.Parse((PAGEWIDTH * 0.95).ToString());
                        this._chartWidth = float.Parse(image.Width.ToString());
                    }
                    else
                    {
                        this._chartHeight = float.Parse((PAGEWIDTH * 0.95).ToString());
                        this._chartWidth = float.Parse((PAGEHEIGHT * 0.95).ToString());
                    }
                    break;
            }


            if ((PAGEHEIGHT - this._chartWidth) / 2 < 0)
                this._chartX = PDFPAGEX;
            else
                this._chartX = (PAGEHEIGHT - this._chartWidth) / 2;


            if ((PAGEWIDTH - this._chartHeight) / 2 < 0)
                this._chartY = PDFPAGEY;
            else
            {
                this._chartY = (PAGEWIDTH - this._chartHeight) / 2;

                if (_typeOfCurrentChart == CHART_TYPE.DOUGHNUT || _typeOfCurrentChart == CHART_TYPE.PIE && this._chartY < 60f && _bMerging)
                    this._chartY = 60f;

            }

            this._chartY = this._chartY + this._pageY;
            this._chartHeight -= this._pageY;

        }

        /// <summary>
        /// Titres du tableau
        /// </summary>
        /// <param name="page">Page en cours</param>
        /// <param name="res">Titre</param>
        /// <param name="y">Position en Y</param>
        /// <param name="x">Position en X</param>
        private void SetTableTitle(PdfPage page, string res, float y, float x = 0)
        {
            page.Graphics.DrawString(res, _bigFont, _titleBrush, new RectangleF(x, y, page.GetClientSize().Width, page.GetClientSize().Height), _strformat);
        }


        /// <summary>
        /// Ecriture des filtres Express
        /// </summary>
        /// <param name="page">Page en cours</param>
        /// <param name="filters">Les filtres expresses à afficher</param>
        /// <param name="y">Position à partir de laquelle on affiche les filres</param>
        private void DrawExpressFilter(PdfPage page, ExpressFilter[] filters, Int32 y)
        {

            if (filters.Length > 0)
            {
                foreach (ExpressFilter filter in filters)
                {
                    Int32 op = 0;
                    if (Int32.TryParse(filter.FilterOperator, out op))
                    {
                        if (string.IsNullOrEmpty(filter.FilterValue) && (Operator)op != Operator.OP_IS_TRUE && (Operator)op != Operator.OP_IS_FALSE && (Operator)op != Operator.OP_IS_EMPTY && (Operator)op != Operator.OP_IS_NOT_EMPTY && (Operator)op != Operator.OP_NOT_0_EMPTY && (Operator)op != Operator.OP_0_EMPTY)
                            continue;

                        string value = filter.ValueEncoded ? HttpUtility.HtmlDecode(filter.FilterValue) : filter.FilterValue;
                        page.Graphics.DrawString(String.Concat(filter.FilterTxt, value), _smallFont, _titleBrush, new RectangleF(PDFPAGEX, y, page.GetClientSize().Width, page.GetClientSize().Height), new PdfStringFormat() { WordWrap = PdfWordWrapType.Word, LineLimit = true, Alignment = PdfTextAlignment.Left });
                        y = y + 10;
                    }

                }
            }

        }

        /// <summary>
        /// Récupération du style d'une céllule
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="font">Font à appliquer à la cellule</param>
        /// <param name="cellValue">valeur de la cellule</param>
        /// <param name="centredText">Centrer le texte dans la cellule</param>
        /// <returns></returns>
        private PdfGridCell SetCellStyle(PdfGridCell cell, PdfFont font, string cellValue, PdfStringFormat centredText = null)
        {

            cell.Style.Font = font;
            cell.Style.CellPadding = _padding;

            cell.Style.Borders.Bottom.Color = this._themColor;
            cell.Style.Borders.Top.Color = this._themColor;
            cell.Style.Borders.Left.Color = this._themColor;
            cell.Style.Borders.Right.Color = this._themColor;
            cell.Value = cellValue;

            if (centredText != null)
                cell.StringFormat = centredText;

            return cell;
        }

        /// <summary>
        /// Récupération du style d'une ligne
        /// </summary>
        /// <param name="backgroundBrush">Background color de la ligne</param>
        /// <param name="textBrush">couleur du texte</param>
        /// <param name="pdfStandardFont">font du text</param>
        /// <returns></returns>
        private PdfGridRowStyle GetPdfGridRowStyle(PdfBrush backgroundBrush, PdfBrush textBrush, PdfStandardFont pdfStandardFont)
        {
            PdfGridRowStyle pdfGridRowStyle = new PdfGridRowStyle();
            pdfGridRowStyle.BackgroundBrush = backgroundBrush;
            pdfGridRowStyle.Font = pdfStandardFont;
            pdfGridRowStyle.TextBrush = textBrush;

            return pdfGridRowStyle;
        }

        /// <summary>
        /// Merge deux images
        /// </summary>
        /// <param name="firstImage"></param>
        /// <param name="secondImage"></param>
        /// <returns></returns>
        private static Bitmap MergeTwoImages(Image firstImage, Image secondImage, CHART_TYPE typeOfCurrentChart = CHART_TYPE.UNKNOWN)
        {
            bool bSpecialChart = (typeOfCurrentChart == CHART_TYPE.PIE || typeOfCurrentChart == CHART_TYPE.DOUGHNUT);

            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }

            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }

            float outputImageWidth = firstImage.Width > secondImage.Width ? firstImage.Width : secondImage.Width;
            float outputImageHeight = firstImage.Height + secondImage.Height + 1;

            if (bSpecialChart)
            {
                //if (outputImageWidth > PAGEWIDTH)
                //    outputImageWidth = PAGEWIDTH - 20;

                outputImageHeight = outputImageWidth;
            }




            Bitmap outputImage = new Bitmap(Int32.Parse(outputImageWidth.ToString()), Int32.Parse(outputImageHeight.ToString()));

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.Clear(Color.White);

                graphics.DrawImage(firstImage, new Rectangle(new Point(), new Size(outputImage.Width, firstImage.Height)),
                    new Rectangle(new Point(), new Size(outputImage.Width, firstImage.Height)), GraphicsUnit.Pixel);


                graphics.DrawImage(secondImage, new Rectangle(new Point(0, (firstImage.Height) + 1), new Size(outputImage.Width, secondImage.Height)),
                    new Rectangle(new Point(), new Size(outputImage.Width, secondImage.Height)), GraphicsUnit.Pixel);
            }

            return outputImage;
        }

        /// <summary>
        /// Trimer une image avec nombre de pixel en X et en Y
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x">position x du contenneur de l'image</param>
        /// <param name="y">position y du contenneur de l'image</param>
        /// <param name="nbPixelTrimX">trim sur l'axe des X en pixel</param>
        /// <param name="nbPixelTrimY">trim sur l'axe des Y en pixel</param>
        /// <returns></returns>
        private Bitmap TrimIMage(Bitmap image, int x, int y, int nbPixelTrimX, int nbPixelTrimY)
        {

            Rectangle rect = new Rectangle(x, y, nbPixelTrimX, nbPixelTrimY);
            Bitmap trimImage;
            //trimImage = new Bitmap(original_image.Size.Width - 10, original_image.Size.Height - 10);
            trimImage = image.Clone(rect, image.PixelFormat);

            using (Graphics graph = Graphics.FromImage(trimImage))
            {
                graph.Clear(Color.White);
                graph.CompositingMode = CompositingMode.SourceOver;
                graph.DrawImage(image, -2, -2);
            }

            return trimImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridDataColumns"></param>
        /// <returns></returns>
        private GridLine[] SetDataGridFirstLine(string gridDataColumns)
        {
            List<GridLine> myList = new List<GridLine>();
            if (!String.IsNullOrEmpty(gridDataColumns))
            {
                GridColumn[] columns = GetGridColumns(gridDataColumns);
                if (columns.Length > 0)
                {
                    _tableFirstColumnName = columns[0].headerText;
                    _tableFirstColumnValue = columns[1].headerText;
                    _tableFirstColumnPercent = columns[2].headerText;
                    if (columns.Length > 3 && !string.IsNullOrEmpty(columns[3].headerText))
                        _tableFirstColumnCombinedValue = columns[3].headerText;
                    if (columns.Length > 3 && !string.IsNullOrEmpty(columns[4].headerText))
                        _tableFirstColumnCombinedPercent = columns[4].headerText;

                    myList.Add(new GridLine() { name = _tableFirstColumnName, value = _tableFirstColumnValue, percent = _tableFirstColumnPercent, combinedpercent = _tableFirstColumnCombinedPercent, combinedvalue = _tableFirstColumnCombinedValue });
                }
            }
            return myList.ToArray();
        }


        /// <summary>
        /// Récupérer les lignes à exporter
        /// </summary>
        /// <param name="gridDataSource">Source de données</param>
        /// <param name="sort">le tri à apliquer</param>
        /// <returns></returns>
        private GridLine[] GetAllLines(string gridDataSource, GridSortingDirection sort)
        {
            var ser = new JavaScriptSerializer();
            //GridLine[] arrayGridDataTemp = ser.Deserialize<GridLine[]>(gridDataSource);
            //GridLine[] arrayGridData = new GridLine[arrayGridDataTemp.Length +1];

            //GridLine[] arrayGridDataTemp = ser.Deserialize<GridLine[]>(gridDataSource);
            GridLine[] arrayGridData = ser.Deserialize<GridLine[]>(gridDataSource);
            //GridColumn[] columns = GetGridColumns(gridDataColumns);
            _chartCategoryLabels = new string[arrayGridData.Length];
            _chartCategoryValues = new object[arrayGridData.Length];
            Int32 i = 0;
            foreach (var item in arrayGridData)
            {
                _chartCategoryLabels[i] = item.name;
                _chartCategoryValues[i] = double.Parse(item.value.Replace(".", ","));
                i++;
            }
            //GridLine firstLine = new GridLine() { name="",percent="",value=""};

            if (!String.IsNullOrEmpty(sort.direction))
            {
                SORTING_TYPE t = (SORTING_TYPE)Enum.Parse(typeof(SORTING_TYPE), sort.direction, false);

                if (t == SORTING_TYPE.ascending)
                {
                    switch ((SORTING_COLUMN)Enum.Parse(typeof(SORTING_COLUMN), sort.field, false))
                    {
                        case SORTING_COLUMN.value:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => double.Parse(x.value.Replace(".", ",")).CompareTo(double.Parse(y.value.Replace(".", ","))));
                            break;
                        case SORTING_COLUMN.combinedvalue:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => double.Parse(x.combinedvalue.Replace(".", ",")).CompareTo(double.Parse(y.combinedvalue.Replace(".", ","))));
                            break;
                        case SORTING_COLUMN.name:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => x.name.CompareTo(y.name));
                            break;
                        case SORTING_COLUMN.percent:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => x.GetPercentValue.CompareTo(y.GetPercentValue));
                            break;
                        case SORTING_COLUMN.combinedpercent:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => x.GetCombinedPercentValue.CompareTo(y.GetCombinedPercentValue));
                            break;
                        default:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => x.name.CompareTo(y.name));
                            break;
                    }
                }
                else
                {
                    switch ((SORTING_COLUMN)Enum.Parse(typeof(SORTING_COLUMN), sort.field, false))
                    {
                        case SORTING_COLUMN.value:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => double.Parse(y.value.Replace(".", ",")).CompareTo(double.Parse(x.value.Replace(".", ","))));
                            break;
                        case SORTING_COLUMN.combinedvalue:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => double.Parse(y.combinedvalue.Replace(".", ",")).CompareTo(double.Parse(x.combinedvalue.Replace(".", ","))));
                            break;
                        case SORTING_COLUMN.name:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => y.name.CompareTo(x.name));
                            break;
                        case SORTING_COLUMN.percent:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => y.GetPercentValue.CompareTo(x.GetPercentValue));
                            break;
                        case SORTING_COLUMN.combinedpercent:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => y.GetCombinedPercentValue.CompareTo(x.GetCombinedPercentValue));
                            break;
                        default:
                            Array.Sort<GridLine>(arrayGridData, (x, y) => y.name.CompareTo(x.name));
                            break;
                    }
                }

            }

            return arrayGridData;
        }

        /// <summary>
        /// Les colinnes à exporter
        /// </summary>
        /// <param name="gridDataColumns">La source de données</param>
        /// <returns></returns>
        private GridColumn[] GetGridColumns(string gridDataColumns)
        {
            var ser = new JavaScriptSerializer();
            GridColumn[] arreyGridColumns = ser.Deserialize<GridColumn[]>(gridDataColumns);
            return arreyGridColumns;
        }


        /// <summary>
        /// Récuperer les filtres expresse appliqués sur le graphique à exporter
        /// </summary>
        /// <param name="expressFilterElements"></param>
        /// <returns></returns>
        private ExpressFilter[] GetExpressFilter(string expressFilterElements)
        {
            var ser = new JavaScriptSerializer();
            ExpressFilter[] array = ser.Deserialize<ExpressFilter[]>(expressFilterElements);
            return array;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridDataSorting"></param>
        /// <returns></returns>
        private GridSortingDirection GetGridSorting(string gridDataSorting)
        {
            var ser = new JavaScriptSerializer();
            GridSortingDirection[] gridSorting = ser.Deserialize<GridSortingDirection[]>(gridDataSorting);
            return gridSorting[0];
        }

        /// <summary>
        /// Retourne NB décimale
        /// </summary>
        /// <returns></returns>
        private Int32 GetFieldLEnght(Field field)
        {
            if (field != null)
            {
                if (_operationType != TypeAgregatFonction.COUNT)
                {
                    switch (field.Format)
                    {

                        case EudoQuery.FieldFormat.TYP_MONEY:
                        case EudoQuery.FieldFormat.TYP_NUMERIC:
                            return field.Length;
                        default:
                            return 0;
                    }
                }
                else
                    return 0;
            }

            else
                return 0;
        }


        /// <summary>
        /// Retourner le formatage du décimale pour excel
        /// </summary>
        /// <returns></returns>
        private string GetExcelNbFormat()
        {
            if (_operationType == TypeAgregatFonction.COUNT)
                return "0";
            string nbFormat = "0.";
            if (_nbDecimal == 0)
                return "0.00";

            for (int i = 0; i < _nbDecimal; i++)
            {
                nbFormat = string.Concat(nbFormat, "0");
            }

            return nbFormat;
        }



        /// <summary>
        /// Recopie du modele word8/word9 dans le dossier "EudoReportWork" à partir du flux binaire de la ressource
        /// </summary>
        /// <returns>Chemin du fichier</returns>
        private string CreateTemplateWork(string sFileTemplateName, string templatePath, out string sNewDirectoryTemplatePath)
        {
            sNewDirectoryTemplatePath = string.Empty;
            // dossier des datas 
            string sDatasPath = Path.Combine(
                eLibTools.GetRootPhysicalDatasPath(HttpContext.Current),
                eLibTools.GetDatasDir(_pref.GetBaseName));

            string s_TemplateFilePath = string.Empty;
            try
            {
                // Création du dossier temporaire pour le traitement
                string sGenerrateNew = string.Empty;
                string sGenerrateNewPath = Path.Combine(sDatasPath, eLibTools.GetFolderType(eLibConst.FOLDER_TYPE.MODELES));

                do
                {
                    sGenerrateNew = Path.GetRandomFileName().Replace(".", "") + @"\";
                } while (Directory.Exists(sGenerrateNewPath + sGenerrateNew));

                sNewDirectoryTemplatePath = Path.Combine(sGenerrateNewPath, sGenerrateNew);

                Directory.CreateDirectory(sNewDirectoryTemplatePath);
                s_TemplateFilePath = sNewDirectoryTemplatePath + sFileTemplateName;
                File.Copy(templatePath, s_TemplateFilePath, false);


            }
            catch (Exception err)
            {
                throw;
            }

            return s_TemplateFilePath;
        }



        private static string CodeConverter(string src, Encoding src_encoding, Encoding dest_encoding)
        {
            byte[] array = new byte[src.Length];
            array = src_encoding.GetBytes(src);
            string s = dest_encoding.GetString(array);
            return s;
        }


        public static string UTF7Encoder(string src)
        {
            return CodeConverter(src, Encoding.Default, Encoding.UTF7);
        }

        public static string UTF7Decoder(string src)
        {
            return CodeConverter(src, Encoding.Default, Encoding.Unicode);
        }

        #endregion

        #region Région des ENUM
        /// <summary>
        /// Le type D'export
        /// </summary>
        enum CHART_EXPORT_TYPE
        {
            PDF = 0,
            PNG = 1,
            XLS = 2
        }

        /// <summary>
        /// Le type du tri
        /// </summary>
        enum SORTING_TYPE
        {
            descending = 0,
            ascending = 1
        }

        /// <summary>
        /// Les différente colonne sur les quelles on applique un tri
        /// </summary>
        enum SORTING_COLUMN
        {
            name = 0,
            percent = 1,
            value = 2,
            combinedvalue = 3,
            combinedpercent = 4
        }

        /// <summary>
        /// Type du graphique disponible dans syncfusion Excel
        /// </summary>
        enum CHART_TYPE
        {
            UNKNOWN = 0,
            PIE = 1,
            BAR = 2,
            COLUMN = 3,
            AREA = 4,
            LINE = 5,
            SPLINE = 6,
            FUNNEL = 7,
            PYRAMID = 8,
            STACKINGAREA = 9,
            STACKINGCOLUMN = 10,
            STACKINGBAR = 11,
            DOUGHNUT = 12,
            CIRCULARGAUJE = 13

        }

        #endregion

    }

    #region Region des class custum
    /// <summary>
    /// Classe pour représenter une lige d'une grille du graphique
    /// </summary>
    public class GridLine
    {
        /// <summary>
        /// Nom de la colonne
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// valeur de la colonne
        /// </summary>
        public string value { get; set; }
        /// <summary>
        /// pourcentage de la colonne
        /// </summary>
        public string percent { get; set; }
        /// <summary>
        /// Transformation du pourcentage de la colonne
        /// </summary>
        /// <returns></returns>
        public float GetPercentValue
        {

            get { return float.Parse(this.percent.Replace(".", ",")); }
        }

        /// <summary>
        /// valeur de la colonne
        /// </summary>
        public string combinedvalue { get; set; }
        /// <summary>
        /// pourcentage de la colonne
        /// </summary>
        public string combinedpercent { get; set; }
        /// <summary>
        /// Transformation du pourcentage de la colonne
        /// </summary>
        /// <returns></returns>
        public float GetCombinedPercentValue
        {

            get { return float.Parse(this.combinedpercent.Replace(".", ",")); }
        }
    }

    /// <summary>
    /// Représente la grille issue de eGrid de syncfusion
    /// </summary>
    public class GridColumn
    {
        /// <summary>
        /// Defines the cell content's overflow mode The available modes are  Clip,Ellipsis,EllipsisWithTooltip
        /// </summary>
        public string clipMode { get; set; }
        /// <summary>
        /// La rubrique affichée en stat
        /// </summary>
        public string field { get; set; }
        /// <summary>
        /// Libellé de la rubrique
        /// </summary>
        public string headerText { get; set; }
        /// <summary>
        /// Indique si la colonne est clé primaire 
        /// </summary>
        public string isPrimaryKey { get; set; }
        /// <summary>
        /// Indique l'alignement du text
        /// </summary>
        public string textAlign { get; set; }
        /// <summary>
        /// Indique le type de contenu de la colonne(string,numeric, date, boleen)
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Indique si la colonne est visible
        /// </summary>
        public Boolean visible { get; set; }
        /// <summary>
        /// Taille de la colonne
        /// </summary>
        public float width { get; set; }

    }

    /// <summary>
    /// Représente les paramètres appliquées à la grille issue de eGrid de syncfusion
    /// </summary>
    public class GridSortingDirection
    {
        /// <summary>
        /// Représente le sorting appliquées à une colonne de la grille (asc,desc)
        /// </summary>
        public string direction { get; set; }
        /// <summary>
        /// La rubrique sur laquelle on applique le sorting
        /// </summary>
        public string field { get; set; }
    }

    /// <summary>
    /// Représente les filtre expresse à afficher dans l'export PDF 
    /// </summary>
    public class ExpressFilter
    {
        /// <summary>
        /// Le nom du filtre expresse (sur quelle table/rubrique on applique un filtre)
        /// </summary>
        public string FilterTxt { get; set; }
        /// <summary>
        /// La valeur du filtre 
        /// </summary>
        public string FilterValue { get; set; }
        /// <summary>
        /// L'opérateur applique sur la valeur du filtre
        /// </summary>
        public string FilterOperator { get; set; }

        /// <summary>
        /// indique si la valeur est encodée
        /// </summary>
        public bool ValueEncoded { get; set; }
    }


    #endregion
}