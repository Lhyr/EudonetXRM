using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoProcessInterfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.common;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page permettant d'exécuter un rapport Power BI, tout en vérifiant les contraintes de sécurité (adresse IP autorisée)
    /// </summary>
    public partial class ePowerBI : eExternalPage<LoadQueryStringPowerBI>, System.Web.SessionState.IRequiresSessionState
    {
        private int _timeOutReport = (1000 * 60 * 5); //TimeOut de 5 minutes avant arrêt forcé de l'attente de l'export (variables en millisecondes)
        private int _checkTimeOutReport = (1000 * 5); //TimeOut de 5 secondes entre chaque appel avant la vérification de l'état du rapport

        /// <summary>
        /// Script exécuté au chargement de la page
        /// </summary>
        protected StringBuilder _onLoadScript = new StringBuilder();

        private eLibConst.MSG_TYPE _currentErrorCriticity = eLibConst.MSG_TYPE.SUCCESS;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessPage()
        {
            try
            {
                errorContainer.Visible = false;
                errorTitle1.InnerText = String.Empty;
                errorTitle2.InnerText = String.Empty;

                ePowerBIReport powerBIReport = LoadPowerBIReport();
                if (powerBIReport != null)
                    this.PageTitle = powerBIReport.Name;

                if (!powerBIReport.CheckIPAddress(Context))
                {
                    RaiseError(eResApp.GetRes(_pref, 515).Replace("<IP>", eLoginOL.GetCurrentIPAddress(Context)), null, true, false);
                    return;
                }

                // Date limite de validité : rubrique de type date, saisie facultative.
                // Si elle est renseignée, l’URL ne retourne aucune valeur si celle-ci est dépassée strictement.
                // TOCHECK FONCTIONNEL : renvoyer l'erreur ? Effectuer la vérification uniquement sur EudoProcess ?
                if (powerBIReport.ExpirationDate != null && powerBIReport.ExpirationDate != DateTime.MinValue && DateTime.Now > powerBIReport.ExpirationDate)
                    RaiseError(String.Format(eResApp.GetRes(_pref, 8729), eResApp.GetRes(_pref, 2386)), null, true, false); // La date limite de validité est dépassée

                /*
                METHODE D'APPEL UTILISEE EN JAVASCRIPT - NE FONCTIONNE QUE SI L'UTILISATEUR A OUVERT UNE SESSION

                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("eButtons");
                PageRegisters.AddCss("eModalDialog");
                PageRegisters.AddCss("eControl");
                PageRegisters.AddCss("eTitle");
                PageRegisters.AddCss("eIcon");

                PageRegisters.AddScript("eMain");
                PageRegisters.AddScript("eTools");
                PageRegisters.AddScript("eModalDialog");
                PageRegisters.AddScript("eUpdater");
                PageRegisters.AddScript("ePerm");
                PageRegisters.AddScript("eWizard");
                PageRegisters.AddScript("eReport");
                PageRegisters.AddScript("eReportCommon");

                _onLoadScript.Append("var reportId = ").Append(powerBIReport.Id).Append(";");
                _onLoadScript.Append("var reportType = ").Append((int)powerBIReport.ReportType).Append(";");
                _onLoadScript.Append("var nTab = ").Append(powerBIReport.Tab).Append(";");
                _onLoadScript.Append("var fid = 0;");
                _onLoadScript.Append("var nTabBkm = 0;");
                _onLoadScript.Append("var bFile = true;");
                _onLoadScript.Append("runReportFromGlobal(reportId, reportType, nTab, fid, nTabBkm, bFile);");
                */

                DateTime dtStart = DateTime.Now;

                // Variables de stockage des résultats XML
                string createNewReportSuccess = String.Empty;
                string createNewReportErrorDescription = String.Empty;
                string checkStatusSuccess = String.Empty;
                string checkStatusErrorDescription = String.Empty;
                string errorCode = String.Empty;
                string content = String.Empty;
                string serverReportId = String.Empty;
                string exportMode = String.Empty;
                string webPath = String.Empty;
                string msgDetail = String.Empty;
                string msgRapport = String.Empty;
                string msgTitle = String.Empty;

                // Lancement du rapport
                List<eErrorContainer> createNewReportErrors = null;
                XmlDocument resultDocumentExecution = new XmlDocument();
                resultDocumentExecution = eReport.CreateNewReport(_pref, Context, resultDocumentExecution, powerBIReport.Id, powerBIReport.Tab, powerBIReport.ReportType, 0, 0, powerBIReport.Name, out createNewReportErrors);

                // Puis vérification du statut de l'exécution toutes les 5 secondes, comme lorsque l'exécution est lancée en JS via eReportManager
                // jusqu'à ce que la durée maximale d'exécution définie par le timeout soit atteinte
                string status = eProcessStatus.WAIT.ToString();
                List<eErrorContainer> checkReportStatusAllErrors = new List<eErrorContainer>();
                XmlDocument resultDocumentCheckStatus = new XmlDocument();
                while (DateTime.Now < dtStart.AddMilliseconds(_timeOutReport) && (status == eProcessStatus.WAIT.ToString() || status == eProcessStatus.RUNNING.ToString()))
                {
                    // Récupération des résultats de l'appel initial pour exécution du rapport
                    createNewReportSuccess = resultDocumentExecution.SelectSingleNode("result//success")?.InnerText;
                    createNewReportErrorDescription = resultDocumentExecution.SelectSingleNode("result//ErrorDescription")?.InnerText;
                    errorCode = resultDocumentExecution.SelectSingleNode("result//ErrorCode")?.InnerText;
                    content = resultDocumentExecution.SelectSingleNode("result//Content")?.InnerText;
                    serverReportId = resultDocumentExecution.SelectSingleNode("result//serverreportid")?.InnerText;
                    exportMode = resultDocumentExecution.SelectSingleNode("result//exportMode")?.InnerText;

                    // Si les résultats ci-dessus n'ont pas pu être récupérés, on considèrera que le premier appel WCF de lancement du rapport n'a pas encore abouti.
                    // On ne peut pas obtenir l'ID à vérifier dans SERVERREPORTS (renvoyé en XML et non fourni par eReport).
                    // Il est donc inutile d'appeler CheckReportStatus(), et on boucle donc jusqu'à l'obtention du résultat ou atteinte du timeout ci-dessus
                    if (!String.IsNullOrEmpty(serverReportId))
                    {
                        int nServerReportId = 0;
                        int.TryParse(serverReportId, out nServerReportId);
                        resultDocumentCheckStatus = new XmlDocument();
                        List<eErrorContainer> checkReportStatusErrors = null;
                        resultDocumentCheckStatus = eReport.CheckReportStatus(_pref, resultDocumentCheckStatus, nServerReportId, out checkReportStatusErrors);
                        checkReportStatusAllErrors.AddRange(checkReportStatusErrors);
                        status = resultDocumentCheckStatus.SelectSingleNode("result//Statut")?.InnerText;
                    }

                    Thread.Sleep(_checkTimeOutReport); // attente de X millisecondes avant nouvelle vérification, comme en JS
                }

                // Récupération du contenu de chaque appel WCF (XML)
                try
                {
                    // Résultats du (dernier) appel pour statut du rapport
                    checkStatusSuccess = resultDocumentCheckStatus.SelectSingleNode("result//success")?.InnerText;
                    checkStatusErrorDescription = resultDocumentCheckStatus.SelectSingleNode("result//ErrorDescription")?.InnerText;
                    webPath = resultDocumentCheckStatus.SelectSingleNode("result//WebPath")?.InnerText;
                    msgTitle = resultDocumentCheckStatus.SelectSingleNode("result//MsgTitle")?.InnerText;
                    msgDetail = resultDocumentCheckStatus.SelectSingleNode("result//MsgDetail")?.InnerText;
                    status = resultDocumentCheckStatus.SelectSingleNode("result//Statut")?.InnerText;
                }
                catch { }

                if (createNewReportSuccess == "1" && checkStatusSuccess == "1" && !String.IsNullOrEmpty(webPath))
                {
                    Response.Redirect(webPath);
                }
                else
                {
                    RaiseError(createNewReportErrorDescription, createNewReportErrors, true, true);
                    RaiseError(checkStatusErrorDescription, checkReportStatusAllErrors, true, true);
                }
            }

            catch (Exception ex)
            {
                // Utile pour le message d'erreur à l'utilisateur
                RaiseError(ex.Message, null, true, true);
            }
        }

        /// <summary>
        /// Charge les tokens du tracking de la queryString
        /// </summary>
        protected override void LoadQueryString()
        {
            DataParam = new LoadQueryStringPowerBI(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);
        }

        /// <summary>
        /// Type d'external page
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp { get { return eExternal.ExternalPageType.POWERBI; } }

        /// <summary>
        /// Retourne le type (nom) de la page pour reconstruire l'UR
        /// </summary>
        /// <returns></returns>
        protected override ExternalUrlTools.PageName GetRedirectPageName()
        {
            return ExternalUrlTools.PageName.PBI;
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Récupère un objet eReport/ePowerBIReport à partir des informations récuperées de la Query String
        /// </summary>
        private ePowerBIReport LoadPowerBIReport()
        {
            int reportId = DataParam.ParamData.ReportId;

            // Si pas d'id fourni, on retourne un message d'erreur
            if (reportId == 0)
            {
                throw new Exception(String.Format(eResApp.GetRes(_pref, 8729), eResApp.GetRes(_pref, 8730))); // Erreur de récupération du rapport Power BI : Identifiant non trouvé en base
            }
            else
            {
                ePowerBIReport powerBIReport = new ePowerBIReport(_pref, reportId);

                if (!powerBIReport.LoadFromDB())
                {
                    throw new Exception(String.Concat("ePowerBI.LoadPowerBIReport() :", powerBIReport.ErrorMessage), powerBIReport.InnerException);
                }

                return powerBIReport;
            }
        }

        private void RaiseError(string error, List<eErrorContainer> detailedErrorContainers, bool displayToUser, bool traceInLogs)
        {
            // Aucune erreur à remonter : on ignore
            if (String.IsNullOrEmpty(error) && (detailedErrorContainers == null || detailedErrorContainers.Count == 0))
                return;

            // On indique à la page externe mère qu'une erreur s'est produite (_anError = true et RendType = ERROR) pour qu'elle puisse prendre les mesures nécessaires (traces
            // dans les logs), puis on affiche le message d'erreur à l'utilisateur
            _anError = true;
            RendType = eExternalPage<LoadQueryStringPowerBI>.ExternalPageRendType.ERROR;

            if (displayToUser) {
                errorContainer.Visible = true;

                if (errorTitle2.InnerText.Trim().Length > 0)
                    errorTitle2.InnerHtml = String.Concat(errorTitle2.InnerHtml, "<br>");

                if (_panelErrorMsg.Trim().Length > 0)
                    _panelErrorMsg = String.Concat(_panelErrorMsg, "<br>");

                _panelErrorMsg = String.Concat(_panelErrorMsg, error);

                if (detailedErrorContainers != null) {
                    // #73 972 - On indique à l'utilisateur que d'autres erreurs non visibles ont été remontées
                    // Permet de ne pas confondre un plantage interne derrière un message type "Date de validité dépassée"
                    if (_panelErrorMsg.Trim().Length > 0)
                        _panelErrorMsg = String.Concat(_panelErrorMsg, "<br>");
                    _panelErrorMsg = String.Concat(_panelErrorMsg, eResApp.GetRes(_pref, 2387)); // Des erreurs sous-jacentes se sont produites, et ont été transmises à nos équipes techniques

                    foreach (eErrorContainer container in detailedErrorContainers)
                    {
                        if (container != null)
                        {
                            // Si le niveau de criticité de l'erreur est plus important que le dernier affiché, on l'indique
                            // MSG_TYPE étant décroissant (plus le HashCode est bas, plus l'erreur est importante), d'où l'action si le niveau observé est inférieur au dernier mémorisé,
                            // et non l'inverse
                            if ((int)container.TypeCriticity < (int)_currentErrorCriticity)
                            {
                                _currentErrorCriticity = container.TypeCriticity;
                                switch (container.TypeCriticity)
                                {
                                    case eLibConst.MSG_TYPE.INFOS: errorTitle1.InnerText = eResApp.GetRes(_pref, 6733); break; // Information
                                    case eLibConst.MSG_TYPE.EXCLAMATION: errorTitle1.InnerText = eResApp.GetRes(_pref, 6536); break; // Avertissement
                                    case eLibConst.MSG_TYPE.CRITICAL: errorTitle1.InnerText = eResApp.GetRes(_pref, 416); break; // Erreur 
                                }
                            }

                            // On ne renvoie pas les mêmes messages plusieurs fois à l'utilisateur
                            if (!errorTitle2.InnerHtml.Contains(container.Msg))
                                errorTitle2.InnerHtml = String.Concat(errorTitle2.InnerHtml, container.Msg);
                            if (!_panelErrorMsg.Contains(container.Detail))
                                _panelErrorMsg = String.Concat(_panelErrorMsg, container.Detail);
                        }
                    }
                }
            }

            if (traceInLogs && detailedErrorContainers != null)
            {
                foreach (eErrorContainer container in detailedErrorContainers)
                    eFeedbackContext.LaunchFeedbackContext(errCont: container, prefSql: _pref, context: Context, userInfo: _pref.User);
            }
        }

        /// <summary>
        /// Gestion de l'affichage du message d'erreur à l'utilisateur si une erreur s'est produite
        /// </summary>
        protected override void RendTitleAndErrorMsg()
        {
            // Si _panelErrorMsg est déjà défini alors c'est un message bien identifié pour l'utilisateur
            if (string.IsNullOrEmpty(_panelErrorMsg))
            {
                base.RendTitleAndErrorMsg();
            }
            else
            {
                // On active le panneau d'erreur
                RendType = ExternalPageRendType.ERROR;

                // On affiche "Erreur" et "Une erreur est survenue" en titre s'ils ne sont pas renseignés
                if (errorTitle1.InnerText.Trim().Length == 0)
                    errorTitle1.InnerHtml = eResApp.GetRes(_pref, 416); // Erreur
                if (errorTitle2.InnerText.Trim().Length == 0)
                    errorTitle2.InnerHtml = eResApp.GetRes(_pref, 72); // Une erreur est survenue
            }
        }
    }
}