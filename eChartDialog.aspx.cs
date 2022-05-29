using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Threading;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page affichée dans une boitre de dialogue pour afficher un chart
    /// </summary>
    public partial class eChartDialog : eEudoPage
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// ID du report
        /// </summary>
        public int ReportId { get; set; }

        /// <summary>
        /// Charge la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eStats", "all");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eStatsPrint", "print");
            PageRegisters.AddCss("eFilterWizard");
            PageRegisters.AddCss("eButtons");
            #region syncfusion css
            PageRegisters.AddCss("syncFusion/ej.web.all.min");
            PageRegisters.AddCss("syncFusion/ej.responsive");
            PageRegisters.AddCss("syncFusion/ejgrid.responsive");
            PageRegisters.AddCss("syncFusion/default-responsive");
            #endregion
            #endregion


            #region ajout des js

            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFilterWizardLight");

            PageRegisters.AddScript("jquery.min");
            PageRegisters.AddScript("syncFusion/jsrender.min");
            PageRegisters.AddScript("syncFusion/ej.web.all.min");
            PageRegisters.AddScript("eCharts");
            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedZ = '", eLibConst.COMBINED_Z, "';"));
            PageRegisters.RawScrip.AppendLine("").Append(string.Concat("top._CombinedY = '", eLibConst.COMBINED_Y, "';"));
            switch (_pref.LangServId)
            {
                case 0:
                    PageRegisters.AddScript("syncFusion/ej.culture.fr-FR.min");
                    PageRegisters.AddScript("syncFusion/ej.localetexts.fr-FR");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'fr-FR';");

                    break;
                case 1:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 2:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.de-DE");
                    PageRegisters.AddScript("syncFusion/ej.culture.de-DE.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'de-DE';");
                    break;
                case 3:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                case 4:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.es-ES");
                    PageRegisters.AddScript("syncFusion/ej.culture.es-ES.min");
                    PageRegisters.RawScrip.AppendLine("").Append("top._resChart = 'es-ES';");
                    break;
                case 5:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;
                default:
                    PageRegisters.AddScript("syncFusion/ej.localetexts.en-US");
                    PageRegisters.AddScript("syncFusion/ej.culture.en-US.min");
                    break;

            }


            #endregion

            //Récupération des paramètres

            ReportId = 0;
            int nReportId = 0;

            //Id du rapport
            if (_requestTools.AllKeys.Contains("reportid") && !String.IsNullOrEmpty(Request.Form["reportid"]))
                Int32.TryParse(Request.Form["reportid"].ToString(), out nReportId);

            ReportId = nReportId;

            //Fiche (pour rapport mode fiche)
            Int32 nFileId = 0;
            if (_requestTools.AllKeys.Contains("fileid") && !String.IsNullOrEmpty(Request.Form["fileid"]))
                Int32.TryParse(Request.Form["fileid"].ToString(), out nFileId);


            // reportid invalide
            if (ReportId == 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, "pas de reprortid");



                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "")), // Paramètres invalides
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg, Environment.NewLine, "reportid invalide"));


                LaunchError();

            }

            eChartRenderer er = null;
            try
            {
                BodyChart.Attributes.Add("onload", "loadSyncFusionChart(" + ReportId + ");");

                bool bCircularGauge = false;
                eReport erChartInfo = new eReport(_pref, ReportId);
                //Chargement des informations du rapport                                                   
                erChartInfo.LoadFromDB();

                string[] chartType = erChartInfo.GetParamValue("typechart").Split("|");
                if (chartType.Length != 2)
                {
                    String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, "type graphique invalide");



                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "")), // Paramètres invalides
                       eResApp.GetRes(_pref, 72),  //   titre
                       String.Concat(sDevMsg, Environment.NewLine, "type graphique invalide"));


                    LaunchError();
                }


                bCircularGauge = chartType[0] == ((int)eModelConst.CHART_TYPE.SPECIAL).ToString() && chartType[1] == ((int)CHART_SPECIAL_TYPE.CIRCULAR_GAUGE_CHART).ToString();
                if (!bCircularGauge)
                {
                    er = (eChartRenderer)eRendererFactory.CreateChartRenderer(_pref, erChartInfo, ReportId);

                    if (er.ErrorMsg.Length == 0)
                    {
                        while (er.PgContainer.Controls.Count > 0)
                        {
                            DivChart.Controls.Add(er.PgContainer.Controls[0]);
                        }
                    }
                    else
                        throw new Exception("Impossible de charger la liste des graphiques depuis le fichier XML", er.InnerException);
                }
                else
                {
                    exPortChart.Attributes.Add("cg", "1");
                    //BSE:#68 559 
                    DivHidden.Attributes.Add("ednchartparam", ReportId.ToString());
                    eChartRenderer.GetImputHiddenParams(_pref, DivHidden, erChartInfo, ReportId.ToString());
                }

                btnShowFilter.Attributes.Add("onmouseover", "st(event, getAttributeValue(document.getElementById('divChartParams_' + " + this.ReportId + "), 'data-filterdescription'))");
                btnShowFilter.Attributes.Add("onmouseout", "ht()");

                //BSE:#66 161
                if (chartType[0] != ((int)eModelConst.CHART_TYPE.SINGLE).ToString())
                    exPortChart.Attributes.Add("Excel", "1");

                ////titre du rapport
                ////charttitle.InnerText = er.chartReport.GetParamValue("Title");

                if (eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.GraphExpressFilter))
                {
                    er = (eChartRenderer)eRendererFactory.CreateFiltreEXpressChartRenderer(_pref, ReportId);

                    if (er.ErrorMsg.Length == 0)
                    {
                        while (er.PgContainer.Controls.Count > 0)
                        {
                            filterExpress.Controls.Add(er.PgContainer.Controls[0]);
                        }
                    }
                    else
                        throw new Exception("Impossible de charger la liste des filtres express", er.InnerException);
                }

            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "InnerException Message : ", ex.InnerException?.Message, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace, Environment.NewLine, "InnerException StackTrace : ", ex.InnerException?.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));


                LaunchError();


            }

        }
    }
}