using System;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Common.Import;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.wcfs.data.import;
using Com.Eudonet.Xrm.import;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>eImportProcessManager</className>
    /// <summary>Gestionnaire de l'assistant d'import en arrière plan</summary>
    /// <authors>MAD</authors>
    /// <date>17/08/2017</date>
    public class eImportProcessManager : eEudoManager
    {
        /// <summary>
        /// Execution d'une action demandée par le client
        /// </summary>
        protected override void ProcessManager()
        {
            // Retour Json  
            eImportWizardJsonResult result = new eImportWizardJsonResult();

            try
            {
                // Paramètres de l'assistant d'import
                eImportWizardParam wizardParam = new eImportWizardParam(_requestTools);
                result.Step = wizardParam.CurrentWizardStep;
                string error = string.Empty;
                switch (wizardParam.Action)
                {
                    case eImportWizardAction.LOAD_STEP:
                        Panel stepContainer = eImportWizardStepFactory.GetStep(_pref, wizardParam.CurrentWizardStep, wizardParam).Init().Render();
                        result.Html = GetResultHTML(stepContainer, true);
                        result.Success = true;
                        if (result.Params == null)
                        {
                            if (wizardParam.ImportTemplateParams.ImportTemplateId != 0)
                                result.Params = wizardParam.ImportTemplateParams;
                            else
                            {
                                result.Params = new ImportTemplate();
                                result.Params.UserId = 0;
                                result.Params.ImportParams = wizardParam.ImportParams;
                            }

                        }
                        break;
                    case eImportWizardAction.UPLOAD_FILE:
                        string fileName = string.Empty;

                        if (_context.Request.Files["file"] != null)
                            result = eImportWizardStepFactory.SaveFile(_pref, _context.Request.Files["file"], wizardParam, out fileName);
                        else
                        {
                            eRequestTools request = new eRequestTools(_context);

                            result = eImportWizardStepFactory.SaveFile(_pref, HttpUtility.UrlDecode(request.GetRequestFormKeyS("file")), wizardParam, out fileName);
                        }

                        if (result.Success)
                            _context.Session["ImportFile"] = fileName;
                        break;
                    case eImportWizardAction.DELETE_FILE:
                        string file = string.Empty;
                        if (_context.Session["ImportFile"] != null && !string.IsNullOrEmpty(_context.Session["ImportFile"].ToString()))
                        {
                            file = _context.Session["ImportFile"].ToString();
                            result.Success = eImportWizardStepFactory.DeleteFile(_pref, file, out error);
                            result.ErrorMsg = error;
                        }

                        if (!result.Success)
                        {
                            result.ErrorTitle = eResApp.GetRes(_pref, 72);
                            if (!string.IsNullOrEmpty(file))
                                result.ErrorMsg = string.Format(eResApp.GetRes(_pref, 8464), file);
                            else
                                result.ErrorMsg = eResApp.GetRes(_pref, 8465);
                        }

                        result.Html = GetResultHTML(eImportWizardStepFactory.GetStep(_pref, wizardParam.CurrentWizardStep, wizardParam).Init().Render(), true);
                        break;
                    case eImportWizardAction.CHECK_PARAMS:
                        result.Success = true;
                        // TODO Check params                 
                        result.Progress = 50;
                        break;
                    case eImportWizardAction.EXEC:
                        result.Success = true;
                        result.Progress = 50;
                        break;
                    case eImportWizardAction.EXEC_PROGRESS:
                        result.Success = true;
                        // TODO
                        result.Progress = 50;
                        break;
                    case eImportWizardAction.CHECK_RUNNING:
                        eImportSourceInfosCallReturn resultWcfCall = eImportWizardStepFactory.ImportServiceFactory(
                            ImportService => ImportService.CheckServer(
                                new eImportCall()
                                {
                                    FileName = wizardParam.FileName,
                                    PrefSQL = _pref.GetNewPrefSql(),
                                    DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT),
                                    WebApp = _pref.AppExternalUrl,
                                    Lang = _pref.Lang,
                                    SecurityGroup = (int)_pref.GroupMode,
                                    UserId = _pref.UserId,
                                    Params = wizardParam.ImportParams
                                }));

                        if (resultWcfCall.ResultCode == ImportResultCode.InstanceAlreadyRunning)
                        {
                            result.Success = false;
                            result.ErrorTitle = eResApp.GetRes(_pref, 1675);
                            result.ErrorMsg = ImportException.GetCodeRes(_pref, resultWcfCall.ResultCode);
                            TimeSpan span = resultWcfCall.EstimatedEndDate.Subtract(DateTime.Now);

                            if (span.TotalHours > 24)
                                result.ErrorDetail = string.Format(eResApp.GetRes(_pref, 8643), resultWcfCall.ImportProgress, "--", "--", "--");
                            else if (resultWcfCall.EstimatedEndDate == DateTime.MinValue || span.TotalMilliseconds <= 0)
                                result.ErrorDetail = string.Format(eResApp.GetRes(_pref, 8643), resultWcfCall.ImportProgress, "00", "00", "00");
                            else
                                result.ErrorDetail = string.Format(eResApp.GetRes(_pref, 8643), resultWcfCall.ImportProgress, span.Hours.ToString("00"), span.Minutes.ToString("00"), span.Seconds.ToString("00"));


                            break;
                        }

                        if (resultWcfCall.ResultCode == ImportResultCode.ServerOverload)
                        {
                            result.Success = false;
                            result.ErrorTitle = eResApp.GetRes(_pref, 1675);
                            result.ErrorMsg = eResApp.GetRes(_pref, 8644);
                            result.ErrorDetail = eResApp.GetRes(_pref, 8645);
                            break;
                        }

                        if (resultWcfCall.ResultCode != ImportResultCode.Success)
                            throw new ImportException(resultWcfCall.ResultCode, resultWcfCall.Error);

                        result.Success = true;
                        break;
                    case eImportWizardAction.NO_ACTION:
                    default:
                        result.Success = true;
                        break;
                }
            }
            // Exception se produit dans l'import
            catch (ImportException ex)
            {
                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(_pref, 416);
                result.ErrorMsg = ImportException.GetCodeRes(_pref, ex.Code);

#if DEBUG
                result.ErrorDetail = ex.Message;
#endif
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorTitle = eResApp.GetRes(_pref, 416);
                result.ErrorMsg = eResApp.GetRes(_pref, 72);
#if DEBUG
                result.ErrorDetail = string.Concat(ex.Message, "<br/>", ex.StackTrace);
#else
                result.ErrorDetail = eResApp.GetRes(_pref, 6574);
#endif
            }

            //Serialisation du retour en json
            RenderResult(RequestContentType.TEXT, delegate () { return SerializerTools.JsonSerialize(result); });
        }
    }
}