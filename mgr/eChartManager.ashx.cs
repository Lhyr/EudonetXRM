using Com.Eudonet.Internal;
using System;
using System.Threading;
using System.Web;
using System.Diagnostics;
using EudoExtendedClasses;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eChartManager
    /// </summary>
    public class eChartManager : eEudoManagerReadOnly
    {

        private Int32 _nReportId = 0;
        private string _sReportId = string.Empty;
        private Int32 _nFileId = 0;
        private Int32 _nTab;
        private Int32 _nTabFrom;
        private Int32 _nIdFrom;
        private Int32 _nDescId;
        private Int32 _nField;
        private eCommunChart.TypeAgregatFonction _agregat;
        private string _sExpressFilterParam;
        /// <summary>Gestion des actions asynchrones du champ de liaison (principalement : recherche MRU, affichage fenetre, recherche depuis fenêtre)</summary>
        protected override void ProcessManager()
        {
            Stopwatch watch = Stopwatch.StartNew();

            #region Récupération des infos postées


            _sReportId = _requestTools.GetRequestQSKeyS("reportid").Split("_")[0] ?? string.Empty;
            _nFileId = _requestTools.GetRequestQSKeyI("fileid") ?? 0;

            _nTabFrom = _requestTools.GetRequestFormKeyI("tabFrom") ?? 0;
            _nIdFrom = _requestTools.GetRequestFormKeyI("idFrom") ?? 0;

            Int32.TryParse(_sReportId, out _nReportId);


            eCommunChart.TypeChart chartType = (eCommunChart.TypeChart)(_requestTools.GetRequestFormKeyI("action") ?? 0);
            eChartRenderer rd = null;
            try
            {
                //
                rd = new eChartRenderer(_pref);

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine, "Reportid à 0.");

                switch (chartType)
                {
                    case eCommunChart.TypeChart.UNDEFINED:
                        ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));
                        LaunchError();
                        break;
                    case eCommunChart.TypeChart.GLOBALCHART:


                        if (_nReportId == 0)
                        {

                            ErrorContainer = eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                    eResApp.GetRes(_pref, 72),  //   titre
                                    String.Concat(sDevMsg)

                                    );
                            LaunchError();
                        }

                        // On transmet le contexte grille 
                        eXrmWidgetContext context = new eXrmWidgetContext(0, _nTabFrom, _nIdFrom);
                        if (context.GridLocation == eXrmWidgetContext.eGridLocation.Bkm)
                            _nFileId = 0;

                        _sExpressFilterParam = HttpUtility.UrlDecode(_requestTools.GetRequestFormKeyS("expressFilterParam")) ?? string.Empty;

                        rd = (eChartRenderer)eRendererFactory.CreateChartXML(_pref, _nReportId, _nFileId, _sExpressFilterParam, context);

                        break;
                    case eCommunChart.TypeChart.STATCHART:
                        _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;

                        _nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
                        _nField = _requestTools.GetRequestFormKeyI("field") ?? 0;
                        _agregat = (eCommunChart.TypeAgregatFonction)(_requestTools.GetRequestFormKeyI("agregatFonction") ?? 0);

                        rd = (eChartRenderer)eRendererFactory.CreateStatisticalChartXML(_pref, _nTab, _nTabFrom, _nIdFrom, _nDescId, _nField, _agregat, _nFileId);
                        break;
                }

                if (rd.ErrorMsg.Length != 0)
                    throw new Exception(eResApp.GetRes(_pref, 72), rd.InnerException);

                if (rd.CombinedChart != null && rd.CombinedChart.ErrorMsg.Length != 0)
                    throw new Exception(eResApp.GetRes(_pref, 72), rd.CombinedChart.InnerException);
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "InnerException Message : ", ex.InnerException?.Message, Environment.NewLine, "Exception Message : ", rd.ErrorMsg, Environment.NewLine, "Exception StackTrace :", ex.StackTrace, Environment.NewLine, "InnerException StackTrace : ", ex.InnerException?.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError();
            }
            finally
            {
                try
                {
                    watch.Stop();
                    if(rd.XMLData.OuterXml != string.Empty)
                    {
                        RenderResult(RequestContentType.XML, delegate () {

                            eRequestReportManager.CallInfo cf = new eRequestReportManager.CallInfo();
                            cf.ElapsedTime = (int)watch.ElapsedMilliseconds;
                            cf.BaseName = _pref.GetBaseName;
                            cf.User = _pref.User.UserLogin;
                            cf.Action = ((eCommunChart.TypeChart)chartType).ToString();
                            cf.Report = _nReportId;

                            eRequestReportManager.Instance.Add(cf);

                            watch = null;

                            return rd.XMLData.OuterXml;
                        });
                    }      

                }
                catch (eEndResponseException) { }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {

                    String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);


                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                    ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       String.Concat(sDevMsg));


                    LaunchError();

                }

            }

            #endregion
        }



        /// <summary>
        /// Pour les charts, on log :
        ///  id du chart
        ///  id du demandeur
        ///  temps de réponse
        ///  résultat opération (et en cas d'erreur le message d'erreur)
        /// </summary>
        /// <param name="err">Conteneur d'erreur éventeulle</param>
        protected override void LogResult(eErrorContainer err = null)
        {


            try
            {

                //var obj = new { ReportId = _nReportId, FileId = _nFileId, User = _pref.UserId , Time = (DateTime.Now - dtStart).TotalMilliseconds.ToString() };

                //eModelTools.EudoTraceLog(JsonConvert.SerializeObject(obj), _pref, "CHART");
            }
            finally
            {

            }
        }

    }
}