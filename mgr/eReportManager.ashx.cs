using Com.Eudonet.Internal;
using EudoProcessInterfaces;
using EudoQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.report;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.scheduledjob.jobs;
using Com.Eudonet.Internal.wcfs.data.scheduledjob;

namespace Com.Eudonet.Xrm
{
    /// <className>eReportManager</className>
    /// <summary>
    /// Manager qui permet :
    ///     - les actions de Report tel que l'appel du WCF pour exporter ou connaitre l'état d'un export.
    ///     - le rennomage, la création, la suppression d'un modèle d'export
    /// </summary>
    /// <authors>GCH</authors>
    /// <date>2013-06-01</date>
    public class eReportManager : eEudoManager
    {
        private eReport _report;

        private ePermission _viewPerm = null;
        private ePermission _updatePerm = null;

        private eRightReport _oRightManager = null;
        private XmlDocument _resultDocument = new XmlDocument();
        eScheduledReportData _eScheduleParam = null;

        bool _bMajSchedule = false;
        /// <summary>Delai d'attente maximale au lancement d'un rapport</summary>
        private TimeSpan _maxTimeOut = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Constructeur du manager, constructeur ASHX standard.
        /// </summary>
        protected override void ProcessManager()
        {
            int reportId = 0;
            int nTabFrom = 0;
            bool modeFiche = false;
            bool isReportWait = false;
            int fid = 0;
            TypeReport typReport = TypeReport.NONE;
            int nTabBkm = 0;
            string DynamiqueTitle = string.Empty;
            int serverReportId = 0;

            string sScheduleDatas = "";


            int selectLstReportype = -1;

            eReport.ReportOperation operation = eReport.ReportOperation.NONE;
            string strReportLabel = string.Empty;
            Dictionary<string, string> reportInfos = new Dictionary<string, string>();

            #region QueryString

            if (_allKeys.Contains("operation") && !string.IsNullOrEmpty(_context.Request.Form["operation"]))
                operation = (eReport.ReportOperation)int.Parse(_context.Request.Form["operation"]);

            if (_allKeys.Contains("reportid") && !string.IsNullOrEmpty(_context.Request.Form["reportid"]))
                int.TryParse(_context.Request.Form["reportid"], out reportId);

            if (_allKeys.Contains("reportname") && !string.IsNullOrEmpty(_context.Request.Form["reportname"]))
                strReportLabel = _context.Request.Form["reportname"];

            if (_allKeys.Contains("reporttype") && !string.IsNullOrEmpty(_context.Request.Form["reporttype"]))
            {
                int n;
                int.TryParse(_context.Request.Form["reporttype"], out n);
                typReport = (TypeReport)n;

                // Réuperer les droits
                _oRightManager = new eRightReport(_pref, typReport);
            }


            if (
                        (reportId == 0 &&
                            (!operation.Equals(eReport.ReportOperation.ADD)
                                & !operation.Equals(eReport.ReportOperation.LIST)
                                 & !operation.Equals(eReport.ReportOperation.SCHEDULE)
                            )
                     || operation.Equals(eReport.ReportOperation.NONE)
                     )
                )
                LaunchError(eErrorContainer.GetUserError(
                    eLibConst.MSG_TYPE.EXCLAMATION,
                    eResApp.GetRes(_pref, 6563),
                    "",
                    eResApp.GetRes(_pref, 72)));

            if (operation.Equals(eReport.ReportOperation.EXECUTE))
            {
                if (_allKeys.Contains("TabFrom") && !string.IsNullOrEmpty(_context.Request.Form["TabFrom"]))
                    int.TryParse(_context.Request.Form["TabFrom"], out nTabFrom);

                if (_allKeys.Contains("bFile") && !string.IsNullOrEmpty(_context.Request.Form["bFile"]))
                    bool.TryParse(_context.Request.Form["bFile"], out modeFiche);

                if (_allKeys.Contains("tabBKM") && !string.IsNullOrEmpty(_context.Request.Form["tabBKM"]))
                    int.TryParse(_context.Request.Form["tabBKM"], out nTabBkm);

                if (_allKeys.Contains("fid") && !string.IsNullOrEmpty(_context.Request.Form["fid"]))
                    int.TryParse(_context.Request.Form["fid"], out fid);

                if (_allKeys.Contains("DynamiqueTitle") && !string.IsNullOrEmpty(_context.Request.Form["DynamiqueTitle"]))
                    DynamiqueTitle = _context.Request.Form["DynamiqueTitle"];

                if (_allKeys.Contains("isReportWait") && !string.IsNullOrEmpty(_context.Request.Form["isReportWait"]))
                    isReportWait = _context.Request.Form["isReportWait"] == "1";
            }
            //cas de l'ajout d'un nouveau rapport
            else if (operation.Equals(eReport.ReportOperation.ADD) || operation.Equals(eReport.ReportOperation.UPDATE))
            {
                if (_allKeys.Contains("reporttype") && !string.IsNullOrEmpty(_context.Request.Form["reporttype"]))
                    reportInfos.Add("reporttype", _context.Request.Form["reporttype"]);
                if (_allKeys.Contains("tab") && !string.IsNullOrEmpty(_context.Request.Form["tab"]))
                    reportInfos.Add("tab", _context.Request.Form["tab"]);
                if (_allKeys.Contains("endproc"))
                    reportInfos.Add("endproc", _context.Request.Form["endproc"]);
                if (_allKeys.Contains("params") && !string.IsNullOrEmpty(_context.Request.Form["params"]))
                    reportInfos.Add("params", HttpUtility.UrlDecode(_context.Request.Form["params"]));
                // #64 326 - Fermer la fenêtre de l'assistant, ou non
                if (_allKeys.Contains("close") && !string.IsNullOrEmpty(_context.Request.Form["close"]))
                    reportInfos.Add("close", _context.Request.Form["close"]);

                // les schedules sont désormais liés au rapport et donc créé/maj/supprimé avec le rapport
                if (_allKeys.Contains("majschedule") && !string.IsNullOrEmpty(_context.Request.Form["majschedule"]))
                    _bMajSchedule = _context.Request.Form["majschedule"] == "1";

                if (_bMajSchedule && _allKeys.Contains("scheduledata") && !string.IsNullOrEmpty(_context.Request.Form["scheduledata"]))
                    sScheduleDatas = _context.Request.Form["scheduledata"];

                if (sScheduleDatas.Length > 0)
                    _eScheduleParam = JsonConvert.DeserializeObject<eScheduledReportData>(sScheduleDatas, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                #region permissions

                int permId;
                ePermission.PermissionMode permMode;
                int permLevel;
                string permUser;

                bool bViewPermissionChecked = _requestTools.GetRequestFormKeyB("viewpermchecked") ?? false;
                bool bUpdatePermissionChecked = _requestTools.GetRequestFormKeyB("updatepermchecked") ?? false;

                if (bViewPermissionChecked)
                {
                    reportInfos.Add("viewpermchecked", bViewPermissionChecked ? "1" : "0");

                    permId = _requestTools.GetRequestFormKeyI("viewpermid") ?? 0;
                    permMode = (ePermission.PermissionMode)(_requestTools.GetRequestFormKeyI("viewpermmode") ?? 0);
                    permLevel = _requestTools.GetRequestFormKeyI("viewpermlevel") ?? 0;
                    permUser = _requestTools.GetRequestFormKeyS("viewpermuser") ?? "";

                    _viewPerm = new ePermission(permId, permMode, permLevel, permUser);
                }
                else
                    _viewPerm = null;


                if (bUpdatePermissionChecked)
                {
                    reportInfos.Add("updatepermchecked", bUpdatePermissionChecked ? "1" : "0");

                    permId = _requestTools.GetRequestFormKeyI("updatepermid") ?? 0;
                    permMode = (ePermission.PermissionMode)(_requestTools.GetRequestFormKeyI("updatepermmode") ?? 0);
                    permLevel = _requestTools.GetRequestFormKeyI("updatepermlevel") ?? 0;
                    permUser = _requestTools.GetRequestFormKeyS("updatepermuser") ?? "";

                    _updatePerm = new ePermission(permId, permMode, permLevel, permUser);
                }
                else
                    _updatePerm = null;

                if (_allKeys.Contains("viewruleid"))
                    reportInfos.Add("viewruleid", _context.Request.Form["viewruleid"]);

                #endregion
            }
            else if (operation.Equals(eReport.ReportOperation.SCHEDULE))
            {
                if (!_requestTools.AllKeys.Contains("scheduledata"))
                {
                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6563), "", eResApp.GetRes(_pref, 72)));
                    return;
                }
                try
                {
                    string sSchedule = _context.Request.Form["scheduledata"];
                    _eScheduleParam = JsonConvert.DeserializeObject<eScheduledReportData>(sSchedule, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
                }
                catch (Exception e)
                {
                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6563), "", eResApp.GetRes(_pref, 72)));
                    return;
                }

            }
            // cas de l affichage du tooltip
            // ASY : #31582 - Ajouter Tous dans l'assistant Rapport -Suite : Ajouter le type dans le tooltip lorsqu'on selectionne Tous - Recuperer la selection du tous
            else if (operation.Equals(eReport.ReportOperation.DISPLAY))
            {
                if (_allKeys.Contains("AllWiz") && !string.IsNullOrEmpty(_context.Request.Form["AllWiz"]))
                    int.TryParse(_context.Request.Form["AllWiz"], out selectLstReportype);

            }

            #endregion
            _report = null;

            switch (operation)
            {
                case eReport.ReportOperation.SCHEDULE:
                    //On ne peut plus planifier un rapport indépendament du wizard, cette entrée est donc deprecated
                    Schedule(_eScheduleParam);
                    break;
                case eReport.ReportOperation.ADD:
                    if (typReport == TypeReport.CHARTS)
                    {
                        if (!_oRightManager.CanAddNewItem())
                        {
                            LaunchError(
                                eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 149), // vous n'avez pas les droits
                                string.Empty,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 149)));
                            return;
                        }
                    }
                    Add(reportInfos);

                    break;
                case eReport.ReportOperation.RENAME:
                    _report = new eReport(_pref, reportId);
                    Rename(_resultDocument, strReportLabel);
                    break;
                case eReport.ReportOperation.UPDATE:
                    if (typReport == TypeReport.CHARTS)
                    {
                        if (!_oRightManager.CanEditItem())
                        {
                            LaunchError(
                                eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 149), // vous n'avez pas les droits
                                string.Empty,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 149)));
                            return;
                        }
                    }
                    Update(reportInfos, reportId, strReportLabel);
                    break;
                case eReport.ReportOperation.CLONE:
                    break;
                case eReport.ReportOperation.DELETE:
                    _report = new eReport(_pref, reportId);
                    Delete(_resultDocument);
                    break;
                case eReport.ReportOperation.DISPLAY:
                    _report = new eReport(_pref, reportId);
                    DisplayDescription(_resultDocument, selectLstReportype);
                    break;
                case eReport.ReportOperation.EXECUTE:
                    if (typReport == TypeReport.PRINT || typReport == TypeReport.PRINT_FILE)
                    {
                        if (!_oRightManager.HasRight(eLibConst.TREATID.PRINT))
                        {
                            LaunchError(
                                eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 149), // vous n'avez pas les droits
                                string.Empty,
                                eResApp.GetRes(_pref, 72),
                                eResApp.GetRes(_pref, 149)));
                            return;
                        }
                    }

                    List<eErrorContainer> createNewReportErrors = null;
                    _resultDocument = eReport.CreateNewReport(_pref, _context, _resultDocument, reportId, nTabFrom, typReport, fid, nTabBkm, DynamiqueTitle, out createNewReportErrors);
                    if (createNewReportErrors != null && createNewReportErrors.Count > 0)
                        foreach (eErrorContainer container in createNewReportErrors)
                            LaunchError(container);

                    break;
                case eReport.ReportOperation.CHECKMODE:
                    List<eErrorContainer> checkReportStatusErrors = null;
                    if (_requestTools.AllKeys.Contains("reportid") && !string.IsNullOrEmpty(_context.Request.Form["reportid"]))
                        int.TryParse(_context.Request.Form["reportid"], out serverReportId);
                    eReport.CheckReportStatus(_pref, _resultDocument, serverReportId, out checkReportStatusErrors);
                    break;
                case eReport.ReportOperation.ARCHIVE:
                    if (_requestTools.AllKeys.Contains("reportid") && !string.IsNullOrEmpty(_context.Request.Form["reportid"]))
                        int.TryParse(_context.Request.Form["reportid"], out serverReportId);
                    if (serverReportId > 0)
                    {
                        ArchiveReport(serverReportId);
                    }

                    break;
                case eReport.ReportOperation.LIST:
                    bool bOnlyArchived = false;
                    if (_requestTools.AllKeys.Contains("archiveonly") && !string.IsNullOrEmpty(_context.Request.Form["archiveonly"]))
                        bOnlyArchived = _context.Request.Form["archiveonly"] == "1";

                    GetReportList(bOnlyArchived);


                    break;
                case eReport.ReportOperation.GETPARAMETERS:
                    _report = new eReport(_pref, reportId);
                    GetParameters(_resultDocument);
                    break;
            }
            if (reportInfos != null)
            {
                reportInfos.Clear();
                reportInfos = null;
            }
            _report = null;
            RenderResult(RequestContentType.XML, delegate () { return _resultDocument.OuterXml; });
        }

        /// <summary>
        /// Appel WCF pour maj/créé/supprimer le schedule sur EUDOTRAIT
        /// </summary>
        /// <param name="eschData"></param>
        private bool Schedule(eScheduledReportData eschData)
        {
            string error = "";

            DateTime dt = DateTime.MinValue;

            #region eschData vide => on supprime la plannification du rapport
            if (eschData == null)
            {
                try
                {

                    //recherche du schedule id 


                    return eWCFTools.WCFEudoProcessCaller<IEudoScheduledJobsWCF, bool>(
                                ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL"),
                                obj => obj.DeleteSchedule(1));


                }
                catch (EndpointNotFoundException ExWS)
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        string.Empty,
                        eResApp.GetRes(_pref, 72),
                        string.Concat(eResApp.GetRes(_pref, 6571), ", ", eResApp.GetRes(_pref, 6565), " : ",
                        Environment.NewLine, ExWS.ToString())));
                }

                return false;
            }
            #endregion

            ReportUrlPaths urlPaths = ReportUrlPaths.GetNew(_pref, _context);

            eschData.ReportParam.DatasPath = urlPaths.DatasPath;
            eschData.ReportParam.AppPath = string.Concat(urlPaths.UrlFullPath, "/");
            eschData.ReportParam.StylePath = urlPaths.StylePath;
            eschData.ReportParam.ImagePath = urlPaths.ImgPath;

            #region init param

            eScheduledReportCall mysch = new eScheduledReportCall();
            mysch.Lang = _pref.Lang;
            mysch.SecurityGroup = _pref.GroupMode.GetHashCode();
            mysch.UserId = _pref.UserId;
            mysch.BaseUID = _pref.DatabaseUid;
            mysch.PrefSQL = _pref.GetNewPrefSql();

            mysch.ScheduleData = eschData;



            #endregion

            eScheduledJobData schedule = new eScheduledJobData();
            schedule.Active = true;
            schedule.FrequencyType = eschData.FrequencyType;
            schedule.Frequency = eschData.Frequency;
            schedule.Day = eschData.Day;
            schedule.Order = eschData.Order;
            schedule.WeekDays = eschData.WeekDays;
            schedule.Month = eschData.Month;
            schedule.StartDate = eschData.StartDate;

            if (!string.IsNullOrEmpty(eschData.EndDate))
                schedule.EndDate = eschData.EndDate;

            schedule.Repeat = eschData.Repeat;
            if (!string.IsNullOrEmpty(eschData.Hour))
                schedule.Hour = eschData.Hour;

            #region Création du job

            eScheduledJob job = new eScheduledJob();
            job.Type = Internal.wcfs.data.common.TaskJobType.REPORT;

            JobReportCall param = new JobReportCall();
            param.InfoSql = new InfoSql()
            {
                ApplicationName = _pref.GetSqlApplicationName,
                Instance = _pref.GetSqlInstance,
                User = _pref.GetSqlUser,
                Password = _pref.GetSqlPassword,
            };

            param.InfoDb = new InfoDatabase()
            {
                Name = _pref.GetBaseName,
                Uid = _pref.DatabaseUid
            };

            param.Lang = _pref.Lang;
            param.SecurityGroup = _pref.GroupMode.GetHashCode();
            param.UserId = _pref.UserId;
            param.ScheduleData = eschData;

            

            job.Param = JsonConvert.SerializeObject(param);

            schedule.Jobs = new List<eScheduledJob>() { job };
            #endregion

            eScheduledJobIdentifier identifier = new eScheduledJobIdentifier()
            {
                BaseName = this._pref.GetBaseName,
                UserId = this._pref.UserId,
                Lang = this._pref.Lang
            };

            eScheduledJobRun rrr = eWCFTools.WCFEudoProcessCaller<IEudoReportWCF, eScheduledJobRun>(
                     ConfigurationManager.AppSettings.Get("EudoReportURL"), obj => obj.ScheduleReportJob(identifier, schedule));

            try
            {
                if (!rrr.Success)
                    throw new EudoException("Erreur WCF", "Une erreur est survenue lors de la planification.");
                else
                {
                    //ok
                    eschData.ScheduleId = rrr.ServerScheduleId;


                    return true;
                }
            }
            catch (Exception e)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                  eLibConst.MSG_TYPE.CRITICAL,
                  eResApp.GetRes(_pref, 72),
                  string.Empty,
                  eResApp.GetRes(_pref, 72),
                  string.Concat(eResApp.GetRes(_pref, 6571), ", ", eResApp.GetRes(_pref, 6565), " : ",
                  Environment.NewLine, e.ToString())));
            }



            if (error.Length > 0)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), error));

            return false;
        }

        /// <summary>
        /// Gère l'action de renommage du rapport
        /// </summary>
        /// <param name="resultDocument">Document XML contenant le résultat de l'opération</param>
        /// <param name="newLabel">Nouveau libellé</param>
        private void Rename(XmlDocument resultDocument, string newLabel)
        {
            #region INIT XML

            /*
             * Code d'erreur :
             *  0 : pas d'erreur
             *  1 : Rapport avec le même nom et le même Type existe déjà
             *  2 : autre Erreur
             * */

            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            resultDocument.AppendChild(resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = resultDocument.CreateElement("result");
            resultDocument.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // Id du rapport
            XmlNode reportIdNode = resultDocument.CreateElement("reportid");
            reportIdNode.InnerText = _report.Id.ToString();
            contentNode.AppendChild(reportIdNode);

            //Nouveau libellé
            XmlNode reportNameNode = resultDocument.CreateElement("reportname");
            reportNameNode.InnerText = newLabel;
            contentNode.AppendChild(reportNameNode);
            #endregion

            string sError = string.Empty;
            bool bError = false;
            bool bExists = false;

            bError = !_report.LoadFromDB();

            if (!bError)
            {
                try
                {
                    bExists = _report.Exists(newLabel);
                }
                catch
                {
                    bError = true;
                    bExists = false;
                }
            }
            if (!bError && !bExists)
                bError = !_report.Rename(newLabel);

            if (bError)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), _report.ErrorMessage));
            else if (bExists)
            {
                bError = true;
                _report.ErrorMessage = eResApp.GetRes(_pref, 783).Replace("<ITEM>", newLabel);
            }

            successNode.InnerText = bError ? "0" : "1";
            errorCodeNode.InnerText = bError ? "1" : "0";
            errorMsgNode.InnerText = _report.ErrorMessage;
        }

        /// <summary>
        /// Gère l'action de suppression du rapport
        /// </summary>
        /// <param name="resultDocument">Document XML contenant le résultat de l'opération</param>
        private void Delete(XmlDocument resultDocument)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            resultDocument.AppendChild(resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = resultDocument.CreateElement("result");
            resultDocument.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // Id du rapport
            XmlNode reportIdNode = resultDocument.CreateElement("reportid");
            reportIdNode.InnerText = _report.Id.ToString();
            contentNode.AppendChild(reportIdNode);

            #endregion

            bool bError = false;

            bError = !_report.LoadFromDB();
            if (!bError)
            {
                bError = !_report.Delete();

                if (!bError)
                {
                    try
                    {
                        //Suppression du schedule sur eudotrait
                        Schedule(null);
                    }
                    catch (Exception)
                    {
                        //l'echec de cette suppression n'est pas bloquante
                    }
                }
            }

            if (bError)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), _report.ErrorMessage));

            //Erreur de chargement du rapport
            successNode.InnerText = bError ? "0" : "1";
            errorCodeNode.InnerText = bError ? "1" : "0";
            errorMsgNode.InnerText = bError ? _report.ErrorMessage : "";
        }

        /// <summary>
        /// génère et affiche la description "en toutes lettres" du rapport
        /// à partir de ses paramètres.
        /// </summary>
        /// <param name="resultDocument">Document xml de retour</param>
        /// <param name="selListRapports">Type de rapport de la liste ( peut etre : tout type)</param>
        private void DisplayDescription(XmlDocument resultDocument, int selListRapports)
        {
            #region INIT XML

            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            resultDocument.AppendChild(resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = resultDocument.CreateElement("result");
            resultDocument.AppendChild(baseResultNode);

            // Succès
            XmlNode successNode = resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // Id du rapport
            XmlNode reportIdNode = resultDocument.CreateElement("reportid");
            reportIdNode.InnerText = _report.Id.ToString();
            contentNode.AppendChild(reportIdNode);

            //Nouveau libellé
            XmlNode reportDescriptionNode = resultDocument.CreateElement("reportdescription");
            contentNode.AppendChild(reportDescriptionNode);
            #endregion

            string strDescription = string.Empty;
            bool bError = false;

            bError = !_report.LoadFromDB();

            if (!bError)
            {
                // ASY : #31582 - Ajouter Tous dans l'assistant Rapport -Suite : Ajouter le type dans le tooltip lorsqu'on selectionne Tous 
                _report.ReportListType = selListRapports;

                strDescription = _report.GetDescription();
                bError = !string.IsNullOrEmpty(_report.ErrorMessage);
            }
            if (bError)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), strDescription, eResApp.GetRes(_pref, 72), _report.ErrorMessage));
            //Erreur de chargement du rapport
            successNode.InnerText = bError ? "0" : "1";
            errorCodeNode.InnerText = bError ? "1" : "0";
            reportDescriptionNode.InnerText = bError ? "" : strDescription;
            errorMsgNode.InnerText = !bError ? "" : _report.ErrorMessage;
        }

        /// <summary>
        /// Récupére tous les paramètres d'un rapport pour utilisation en JavaScript, par exemple
        /// Préferer toutefois récupérer ces informations côté .NET, et les ajouter dans le DOM lors de la génération du contenu pour des questions de performances
        /// </summary>
        /// <param name="resultDocument">Document xml de retour</param>
        private void GetParameters(XmlDocument resultDocument)
        {
            #region INIT XML

            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            resultDocument.AppendChild(resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = resultDocument.CreateElement("result");
            resultDocument.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);
            #endregion

            bool bError = false;

            bError = !_report.LoadFromDB();

            if (!bError)
                bError = !string.IsNullOrEmpty(_report.ErrorMessage);

            if (bError)
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), _report.ErrorMessage, eResApp.GetRes(_pref, 72), _report.ErrorMessage));
            else
            {
                // Content
                XmlNode contentNode = resultDocument.CreateElement("Parameters");
                baseResultNode.AppendChild(contentNode);

                foreach (KeyValuePair<string, string> kvp in eReport.GetReportParamValues(_report))
                {
                    XmlNode reportParameter = resultDocument.CreateElement(kvp.Key);
                    reportParameter.InnerText = kvp.Value;
                    contentNode.AppendChild(reportParameter);
                }
            }

            //Erreur de chargement du rapport
            successNode.InnerText = bError ? "0" : "1";
            errorCodeNode.InnerText = bError ? "1" : "0";

            errorMsgNode.InnerText = !bError ? "" : _report.ErrorMessage;
        }

        /// <summary>
        /// Ajout d'un nouveau rapport
        /// </summary>
        /// <param name="reportInfos">Liste des paramètres du rapport transmis pour mise à jour</param>
        private void Add(Dictionary<string, string> reportInfos)
        {
            string error = string.Empty;

            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            _resultDocument.AppendChild(_resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = _resultDocument.CreateElement("result");
            _resultDocument.AppendChild(baseResultNode);

            // Succès
            XmlNode successNode = _resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = _resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = _resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);

            // Content
            XmlNode contentNode = _resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // Id du rapport
            XmlNode reportIdNode = _resultDocument.CreateElement("reportid");
            contentNode.AppendChild(reportIdNode);

            // Libellé
            XmlNode reportNameNode = _resultDocument.CreateElement("reportname");
            contentNode.AppendChild(reportNameNode);

            // #64 326 - URL pour Power BI
            XmlNode powerBIURLNode = _resultDocument.CreateElement("powerbiurl");
            contentNode.AppendChild(powerBIURLNode);

            // #64 326 - Fermer la fenêtre de l'assistant, ou non
            XmlNode closeNode = _resultDocument.CreateElement("close");
            baseResultNode.AppendChild(closeNode);

            #endregion

            try
            {
                if (!reportInfos.ContainsKey("reporttype"))
                    throw new Exception("Type de rapport Indéfinit");

                _report = new eReport(_pref, int.Parse(reportInfos["tab"]),
                    _viewPerm, _updatePerm, "",
                    (TypeReport)int.Parse(reportInfos["reporttype"]), "", reportInfos["endproc"], reportInfos["params"],
                    0);

                _report.CleanParams();

                //BSE :#48 291
                //BSE :64 194 Il faut pouvoir enregistrer un rapport public si on a les droits de création de graphique
                bool allowedCreate = false;
                if (_report.ReportType == TypeReport.CHARTS)
                    allowedCreate = _oRightManager.CanAddNewItem();
                else
                {
                    bool isPublicReport = _report.GetParamValue("public") == "1";
                    allowedCreate = !isPublicReport || (isPublicReport && _oRightManager.HasRight(eLibConst.TREATID.PUBLIC_EXPORT_REPORT));
                }

                if (!allowedCreate)
                    throw new Exception("Impossible de créer le rapport, droits insuffisants sur la propriété \"rapport public\"");

                if (!_report.Insert())
                    throw new Exception(_report.ErrorMessage);

                // #64 326 - Renvoi de l'ID au JS appelant pour transformer la fenêtre en fenêtre de MAJ si elle n'est pas fermée
                // après opération
                reportIdNode.InnerText = _report.Id.ToString();
                reportNameNode.InnerText = _report.Name;

                if (_eScheduleParam != null)
                    _eScheduleParam.ReportParam.ReportId = _report.Id;

                if (_bMajSchedule && _eScheduleParam != null && _report.Owner == _pref.UserId)
                    Schedule(_eScheduleParam);

                // #64 326 - Renvoi de l'URL Power BI si concerné
                if (_report.Format == ReportFormat.POWERBI)
                {
                    ePowerBIReport powerBIReport = new ePowerBIReport(_pref, _report.Id);

                    if (!powerBIReport.LoadFromDB())
                        throw new Exception(powerBIReport.ErrorMessage, powerBIReport.InnerException);

                    powerBIURLNode.InnerText = powerBIReport.GetRewrittenURL();
                }

                if (!string.IsNullOrEmpty(error))
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty,
                        eResApp.GetRes(_pref, 72), error));

                //Erreur de chargement du rapport
                successNode.InnerText = "1";
                errorCodeNode.InnerText = "0";
                errorMsgNode.InnerText = "";

                // #64 326 - Fermeture de la fenêtre ou non - Si erreur, pas de fermeture
                bool bClose = reportInfos.ContainsKey("close") && reportInfos["close"] == "1";
                closeNode.InnerText = bClose ? "1" : "0";
            }
            catch (Exception ex)
            {
                string devMsg = string.Concat("eReportManager.Add() : ", ex.Message);

                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), devMsg));
            }
        }

        /// <summary>
        /// Mise à jour du rapport
        /// </summary>
        /// <param name="reportInfos">Liste des paramètres du rapport transmis pour mise à jour</param>
        /// <param name="reportId">identifiant du rapport</param>
        /// <param name="reportLabel">Nom du rapport</param>
        private void Update(Dictionary<string, string> reportInfos, int reportId, string reportLabel)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            _resultDocument.AppendChild(_resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = _resultDocument.CreateElement("result");
            _resultDocument.AppendChild(baseResultNode);

            // Succès
            XmlNode successNode = _resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = _resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = _resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);

            // Content
            XmlNode contentNode = _resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // ID du rapport
            XmlNode reportIdNode = _resultDocument.CreateElement("reportid");
            contentNode.AppendChild(reportIdNode);

            // Libellé
            XmlNode reportNameNode = _resultDocument.CreateElement("reportname");
            contentNode.AppendChild(reportNameNode);

            // #64 326 - URL pour Power BI
            XmlNode powerBIURLNode = _resultDocument.CreateElement("powerbiurl");
            contentNode.AppendChild(powerBIURLNode);

            // #64 326 - Fermer la fenêtre de l'assistant, ou non
            XmlNode closeNode = _resultDocument.CreateElement("close");
            baseResultNode.AppendChild(closeNode);

            #endregion

            try
            {
                if (!reportInfos.ContainsKey("reporttype"))
                    throw new Exception("Type de rapport Indéfinit");

                if (reportId == 0)
                    throw new Exception("Mise à jour impossible, rapport non indentifié");

                _report = new eReport(_pref, int.Parse(reportInfos["tab"]),
                    _viewPerm, _updatePerm, "",
                    (TypeReport)int.Parse(reportInfos["reporttype"]), "", reportInfos["endproc"], reportInfos["params"],
                    reportId);
                _report.MajSchedule = _bMajSchedule;

                //BSE :#48 291 
                bool allowedCreate = false;
                if (_report.ReportType == TypeReport.CHARTS)
                    allowedCreate = _oRightManager.CanAddNewItem();
                else
                {
                    bool isPublicReport = _report.GetParamValue("public") == "1";
                    allowedCreate = !isPublicReport || (isPublicReport && _oRightManager.HasRight(eLibConst.TREATID.PUBLIC_EXPORT_REPORT));
                }

                if (!allowedCreate)
                    throw new Exception("Mise à jour impossible, droits insuffisants sur la propriété \"rapport public\"");

                _report.CleanParams();

                if (_eScheduleParam != null)
                    _report.ScheduleParam = _eScheduleParam.ToString();

                if (_report.Name.ToLower().Trim().Equals(reportLabel.ToLower().Trim())
                    || HttpUtility.HtmlEncode(_report.Name).Trim().ToLower().Equals(reportLabel.ToLower().Trim()))
                {
                    if (!_report.Update())
                        throw new Exception(_report.ErrorMessage);
                }
                else
                {
                    //Le saveas est modifié, on crée un nouveau rapport avec une duplication des mêmes permissions
                    if (!_report.Duplicate())
                        throw new Exception(_report.ErrorMessage);
                }

                // #64 326 - Renvoi de l'ID et du nom du rapport au JS appelant pour transformer la fenêtre en fenêtre de MAJ si elle n'est pas fermée
                // après opération
                reportIdNode.InnerText = _report.Id.ToString();
                reportNameNode.InnerText = _report.Name;

                if (_bMajSchedule)
                {
                    if (_eScheduleParam != null)
                        _eScheduleParam.ReportParam.ReportId = _report.Id;

                    Schedule(_eScheduleParam);

                    //on reenregistre avec le scheduleid
                    if(_eScheduleParam != null)
                        _report.ScheduleParam = _eScheduleParam.ToString();
                    else
                        _report.ScheduleParam = "";

                    _report.Update();

                }

                // #64 326 - Renvoi de l'URL Power BI si concerné
                if (_report.Format == ReportFormat.POWERBI)
                {
                    ePowerBIReport powerBIReport = new ePowerBIReport(_pref, _report.Id);

                    if (!powerBIReport.LoadFromDB())
                        throw new Exception(powerBIReport.ErrorMessage, powerBIReport.InnerException);

                    powerBIURLNode.InnerText = powerBIReport.GetRewrittenURL();
                }

                //Erreur de chargement du rapport
                successNode.InnerText = "1";
                errorCodeNode.InnerText = "0";
                errorMsgNode.InnerText = "";

                // #64 326 - Fermeture de la fenêtre ou non - Si erreur, pas de fermeture
                bool bClose = reportInfos.ContainsKey("close") && reportInfos["close"] == "1";
                closeNode.InnerText = bClose ? "1" : "0";
            }
            catch (Exception ex)
            {
                string devMsg = string.Concat("eReportManager.Update() : ", ex.Message);

                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72), devMsg));
            }
        }

        /// <summary>
        /// Retourne la liste des exports de l'utilisateur
        /// </summary>
        /// <param name="bOnlyArchived">Indique si seuls les exports archivés doivent être retournés</param>
        public void GetReportList(bool bOnlyArchived)
        {

            eReportUserListRenderer er = eReportUserListRenderer.GetUserListRenderer(_pref, bOnlyArchived);
            if (er.InnerException == null)
                RenderResultHTML(er.PgContainer);
            else
            {
                string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", er.InnerException.Message, Environment.NewLine, "Exception StackTrace :", er.InnerException.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   string.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   string.Concat(sDevMsg));

            }
        }

        /// <summary>
        /// Archive un rapport sur serverreport
        /// </summary>
        /// <param name="serverReportId">Id serveur a archiver</param>
        public void ArchiveReport(int serverReportId)
        {
            eReportResponse ers = null;

            #region code d'accès à un webservice de façon dynamique
            try
            {
                ers = eWCFTools.WCFEudoProcessCaller<IEudoReportWCF, eReportResponse>(
                    ConfigurationManager.AppSettings.Get("EudoReportURL"), obj => obj.ArchiveReport(_pref.GetBaseName, _pref.User.UserId, serverReportId));
            }
            catch (EndpointNotFoundException ExWS)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6571),
                    string.Empty,
                    eResApp.GetRes(_pref, 72),
                    string.Concat(eResApp.GetRes(_pref, 6571), ", ", eResApp.GetRes(_pref, 6565), " : ", Environment.NewLine, ExWS.ToString())));
            }
            #endregion

            //Créer le XML de retour!
            _resultDocument.AppendChild(_resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = _resultDocument.CreateElement("result");
            _resultDocument.AppendChild(baseResultNode);

            // Success ou pas!
            XmlNode successNode = _resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);
            successNode.InnerText = string.IsNullOrEmpty(ers.Erreur) ? "1" : "0";
            successNode = null;

            // Msg Erreur
            XmlNode errorMsgNode = _resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);
            errorMsgNode.InnerText = ers.Erreur;
            errorMsgNode = null;

            ers = null;

        }
    }
}