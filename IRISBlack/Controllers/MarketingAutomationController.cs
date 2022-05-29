using System;
using System.Web.Http;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System.Text;
using System.Collections.Generic;
using EudoQuery;
using System.Text.RegularExpressions;
using System.Web;
using System.Configuration;
using EudoProcessInterfaces;
using Com.Eudonet.Internal.wcfs.data.TreatmentCampaign;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.scheduledjob;
using Com.Eudonet.Internal.wcfs.data.scheduledjob.jobs;
using System.ServiceModel;
using EudoPref;
using System.Linq;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// Contrôleur permettant de récupérer la structure du formulaire avancé
    /// </summary>
    public class MarketingAutomationController : BaseController
    {
        private enum Operation
        {
            /// <summary>
            /// Aucune action, comportement par défaut si aucune action n'est transmise au Manager
            /// Ce cas ne devrait jamais arriver sans provoquer une erreur de paramètre incorrect.
            /// </summary>
            NONE = 0,

            /// <summary>Sauvegarder le formulaire</summary>
            SAVE = 1,

            /// <summary>
            /// Mise à jour des donnés flowy dans la base
            /// </summary>
            UpdateFlowyData = 2,

            /// <summary>
            /// check scenario before activation
            /// </summary>
            CHECK_WORKFLOW = 3,
            /// <summary>
            /// UNcheck scenario before activation
            /// </summary>
            UNCHECKED_WORKFLOW = 4,
            /// <summary>
            /// Removed reciempient from Scenario
            /// </summary>
            END_TRACKING_STEP = 5

        }

        //l'opération demandée
        Operation _operation = Operation.NONE;

        //Objet eWorkflowScenario à traiter
        eWorkflowScenario workflowScenario = null;

        //Gestion des erreurs
        eWorkflowException workflowException = null;

        /// <summary>URL pour les appels WCF au plannificateur</summary>
        private string _WCFSchedulerUrl;

        /// <summary>
        /// Récupère les infos d'une campagne
        /// </summary>
        /// <param name="nParentTab"></param>
        /// <param name="nParentFileId"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{nParentTab:int=0}/{nParentFileId:int=0}/{nTab:int=0}")]
        public IHttpActionResult Get(int nParentTab, int nParentFileId, int nTab)
        {
            try
            {
                ePref pref = _pref;
                eWorkflowScenario oWorkflowScenario = new eWorkflowScenario(0, pref, nTab, nParentFileId, nParentTab);
                //oWorkflowScenario.Init();
                if(string.IsNullOrEmpty(oWorkflowScenario.Label))
                {
                    eFile objFile = eFileLite.CreateFileLite(pref, nParentTab, nParentFileId);
                    oWorkflowScenario.Label = string.Concat(objFile.Record.MainFileLabel, eResApp.GetRes(_pref, 8951));
                }
                
                string workflowTriggerFilterLabel = string.Empty;
                if (oWorkflowScenario.TriggerFilterId != 0)
                    workflowTriggerFilterLabel = getFilterLabel(oWorkflowScenario.TriggerFilterId);

                if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                    this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                //récupérer la fréquence du delai dans eudoprocess
                int delayFrequency = 10;//
                try
                {
                    delayFrequency = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.GetDelayFrequency());
                }
                catch
                {
                    //TODO: add log - eudoprocess is not running
                }
                    

                var result = new WorkflowResponseModel
                {
                    ScenarioId = oWorkflowScenario.WFSCId,
                    Label = oWorkflowScenario.Label,
                    Datas = oWorkflowScenario.Datas,
                    WorkflowTriggerStepId = oWorkflowScenario.WorkflowTriggerStepId,
                    WorkflowTriggerId = oWorkflowScenario.TriggerId,
                    WorkflowTriggerFilterId = oWorkflowScenario.TriggerFilterId,
                    WorkflowTriggerFilterLabel = workflowTriggerFilterLabel,
                    ActionInfos = oWorkflowScenario.ActionInfos,
                    IsActivated = oWorkflowScenario.IsActivated,
                    TriggerType = oWorkflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT ? 3 : 0,
                    DelayFrequency = delayFrequency
                };
                //récuperation des infos d'une action récurrente
                if (oWorkflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                {
                    result.SchedulerJobId = oWorkflowScenario.SchedulerJobId;
                    if (result.SchedulerJobId != -1)
                    {
                        eudoDAL dalTrait = ePrefBaseTools.GetEudoDAL(new ePrefSQL(pref.GetSqlInstance, "EUDOTRAIT", pref.GetSqlUser, pref.GetSqlPassword, pref.GetSqlApplicationName));
                        List<eScheduledJobData> jobs = eLibTools.GetSchedules(dalTrait, result.SchedulerJobId);
                        if (jobs != null && jobs.Count != 0)
                        {
                            eScheduledJobData jobData = jobs[0];

                            eScheduledJob job = jobData.Jobs.FirstOrDefault();
                            if (job != null)
                            {
                                var _config = SerializerTools.JsonDeserialize<JobRecurrentActionCall>(job.Param);
                                result.ScheduleId = _config.ScheduleId;
                                try
                                {
                                    result.ScheduleDescription = eScheduleInfos.GetScheduleLabelFromBase(0, _pref, result.ScheduleId);
                                }
                                catch (Exception ex)
                                {
                                    //TODO: add log
                                }
                            }
                        }

                    }
                }

                //TODO: add a factory if we need details
                return Ok(JsonConvert.SerializeObject(
                    result
                ));
            }
            catch (EudoException ee)
            {
                var resulterrorEudo = new WorkflowResponseModel() {

                    Success = false 
                    
                };

                resulterrorEudo.ListErrorCheckValidScenario = new Dictionary<int, string>();
                resulterrorEudo.ListErrorCheckValidScenario.Add(ee.ErrorCode, ee.UserMessage);

                //todo : handle error js side
                throw;

                return Ok(JsonConvert.SerializeObject(
                   resulterrorEudo
               ));

            }
            catch (Exception ex)
            {
                var resulterrorOther = new WorkflowResponseModel()
                {

                    Success = false

                };

                resulterrorOther.ListErrorCheckValidScenario = new Dictionary<int, string>();
                resulterrorOther.ListErrorCheckValidScenario.Add(0, "Impossible de charger le scénario");

                //todo : handle error js side
                throw;

                return Ok(JsonConvert.SerializeObject(
                   resulterrorOther
               ));

               
            }
        }

        /// <summary>
        /// Récupère les infos d'un workflow
        /// </summary>
        /// <param name="nScenarioId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{nScenarioId:int=0}")]
        public IHttpActionResult Get (int nScenarioId)
        {
            try
            {
                ePref pref = _pref;
                eWorkflowScenario oWorkflowScenario = new eWorkflowScenario(nScenarioId, pref, 0, 0, 0);
                oWorkflowScenario.Init();
                if (string.IsNullOrEmpty(oWorkflowScenario.Label))
                {
                    eFile objFile = eFileLite.CreateFileLite(pref, oWorkflowScenario.EvtTabId, oWorkflowScenario.EvtFileId);
                    oWorkflowScenario.Label = string.Concat(objFile.Record.MainFileLabel, eResApp.GetRes(_pref, 8951));
                }

                string workflowTriggerFilterLabel = string.Empty;
                if (oWorkflowScenario.TriggerFilterId != 0)
                    workflowTriggerFilterLabel = getFilterLabel(oWorkflowScenario.TriggerFilterId);

                if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                    this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                //récupérer la fréquence du delai dans eudoprocess
                int delayFrequency = 10;//
                try
                {
                    delayFrequency = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.GetDelayFrequency());
                }
                catch
                {
                    //TODO: add log - eudoprocess is not running
                }


                var result = new WorkflowResponseModel
                {
                    Tab = oWorkflowScenario.Tab,
                    EvtFileId = oWorkflowScenario.EvtFileId,
                    EvtTabId = oWorkflowScenario.EvtTabId,
                    ScenarioId = oWorkflowScenario.WFSCId,
                    Label = oWorkflowScenario.Label,
                    Datas = oWorkflowScenario.Datas,
                    WorkflowTriggerStepId = oWorkflowScenario.WorkflowTriggerStepId,
                    WorkflowTriggerId = oWorkflowScenario.TriggerId,
                    WorkflowTriggerFilterId = oWorkflowScenario.TriggerFilterId,
                    WorkflowTriggerFilterLabel = workflowTriggerFilterLabel,
                    ActionInfos = oWorkflowScenario.ActionInfos,
                    IsActivated = oWorkflowScenario.IsActivated,
                    TriggerType = oWorkflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT ? 3 : 0,
                    DelayFrequency = delayFrequency
                };
                //récuperation des infos d'une action récurrente
                if (oWorkflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                {
                    result.SchedulerJobId = oWorkflowScenario.SchedulerJobId;
                    if (result.SchedulerJobId != -1)
                    {
                        eudoDAL dalTrait = ePrefBaseTools.GetEudoDAL(new ePrefSQL(pref.GetSqlInstance, "EUDOTRAIT", pref.GetSqlUser, pref.GetSqlPassword, pref.GetSqlApplicationName));
                        List<eScheduledJobData> jobs = eLibTools.GetSchedules(dalTrait, result.SchedulerJobId);
                        if (jobs != null && jobs.Count != 0)
                        {
                            eScheduledJobData jobData = jobs[0];

                            eScheduledJob job = jobData.Jobs.FirstOrDefault();
                            if (job != null)
                            {
                                var _config = SerializerTools.JsonDeserialize<JobRecurrentActionCall>(job.Param);
                                result.ScheduleId = _config.ScheduleId;
                                try
                                {
                                    result.ScheduleDescription = eScheduleInfos.GetScheduleLabelFromBase(0, _pref, result.ScheduleId);
                                }
                                catch (Exception ex)
                                {
                                    //TODO: add log
                                }
                            }
                        }

                    }
                }

                //TODO: add a factory if we need details
                return Ok(JsonConvert.SerializeObject(
                    result
                ));
            }
            catch (EudoException ee)
            {
                var resulterrorEudo = new WorkflowResponseModel()
                {

                    Success = false

                };

                resulterrorEudo.ListErrorCheckValidScenario = new Dictionary<int, string>();
                resulterrorEudo.ListErrorCheckValidScenario.Add(ee.ErrorCode, ee.UserMessage);

                //todo : handle error js side
                throw;

                return Ok(JsonConvert.SerializeObject(
                   resulterrorEudo
               ));

            }
            catch (Exception ex)
            {
                var resulterrorOther = new WorkflowResponseModel()
                {

                    Success = false

                };

                resulterrorOther.ListErrorCheckValidScenario = new Dictionary<int, string>();
                resulterrorOther.ListErrorCheckValidScenario.Add(0, "Impossible de charger le scénario");

                //todo : handle error js side
                throw;

                return Ok(JsonConvert.SerializeObject(
                   resulterrorOther
               ));


            }
        }

        private string getFilterLabel(int nSelfilterId)
        {
            #region récupération de la liste des filtres
            eDataFillerGeneric filler = new eDataFillerGeneric(_pref, 104000, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eq)
                {
                    List<WhereCustom> list = new List<WhereCustom>();
                    //filtre sélectionné
                    list.Add(new WhereCustom(FilterField.ID.GetHashCode().ToString(), Operator.OP_EQUAL, nSelfilterId.ToString(), InterOperator.OP_OR));

                    eq.AddCustomFilter(new WhereCustom(list));

                    eq.SetListCol = string.Concat(FilterField.LIBELLE.GetHashCode().ToString(), ";", FilterField.TAB.GetHashCode().ToString());

                };

            filler.Generate();


            if (filler.ErrorMsg.Length > 0)
            {
                return string.Empty;
            }
            else if (filler.ListRecords == null)
            {
                return string.Empty;
            }
            #endregion


            String sLibelle = string.Empty;

            foreach (eRecord er in filler.ListRecords)
            {
                eFieldRecord efLibelle = er.GetFields.Find(delegate (eFieldRecord ef)
                {
                    return ef.FldInfo.Alias == string.Concat("104000_", FilterField.LIBELLE.GetHashCode().ToString());
                });

                if (efLibelle == null || efLibelle.Value.Length == 0)
                    continue;

                sLibelle = efLibelle.DisplayValue;
            }

            return sLibelle;
        }

        [HttpPost]
        public IHttpActionResult Post(WorkflowModel workflowModel)
        {
            bool success = false;


            try
            {
                if (RetrieveParams(workflowModel))
                {
                    //on fait le test sur le label du workflow coté back aussi
                    if(string.IsNullOrEmpty(workflowScenario.Label.Trim()))
                    {
                        workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, eResApp.GetRes(_pref, 8950));
                        workflowException.ListErrorCheckValidScenario.Add(-1, eResApp.GetRes(_pref, 8950));
                        throw workflowException;
                    }
                    success = RunOperation();
                }
            }
            catch (eWorkflowException ex)
            {
                workflowException = ex;
            }

            return Ok(JsonConvert.SerializeObject(RenderResponse(success)));
        }

        private bool RetrieveParams(WorkflowModel workflowModel)
        {
            try
            {
                #region Type d'operation
                _operation = (Operation)workflowModel.Operation;
                #endregion

                #region Paramètres du workflow

                Int32 id = 0;
                id = workflowModel.WorkflowId;

                workflowScenario = eWorkflowScenario.RetrieveParams(id, _pref, workflowModel);

                return true;
                #endregion
            }
            catch
            {
                throw new eWorkflowException(eWorkflowException.ErrorCode.RETRIEVING_PARAM_ERROR, "an error occurred while retrieving paramters for the workflow");
            }
        }

        private bool RunOperation()
        {
            bool oprationSuccess = false;
            try
            {
                switch (_operation)
                {
                    case Operation.SAVE:

                        //workflowScenario.CheckValid();
                        //Add validate method
                        if (workflowScenario.Save())
                        {
                            Dictionary<int, string> errorlist = new Dictionary<int, string>();
                            if (workflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                                AddOrUpdateSchedule(out errorlist);
                            oprationSuccess = true;
                        }
                        break;
                    case Operation.UpdateFlowyData:

                        //workflowScenario.CheckValid();
                        //Add validate method
                        if (workflowScenario.UpdateFlowyData())
                        {
                            oprationSuccess = true;
                        }
                        break;
                    case Operation.CHECK_WORKFLOW:
                        if (workflowScenario.Save())
                        {
                            Dictionary<int, string> errorlist = new Dictionary<int, string>();
                            if (workflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                                AddOrUpdateSchedule(out errorlist);
                            if (errorlist.Count == 0)
                                errorlist = workflowScenario.CheckBeforeActivationScenario(_pref, workflowScenario);

                            //on vérifie la validité du schedule
                            if (workflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                            {
                                if (workflowScenario.SchedulerJobId > 0)
                                {
                                    if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                                        this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                                    var schedule = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.GetSchedule(workflowScenario.SchedulerJobId));
                                    if ((schedule == null || schedule.NextRun == null || schedule.NextRun < DateTime.Now) && !errorlist.ContainsKey(0))
                                        errorlist.Add(0, eResApp.GetRes(_pref, 8924));
                                }
                            }

                            if (errorlist.Count != 0)
                            {
                                oprationSuccess = false;
                                workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, "an error occurred while retrieving paramters for the workflow");
                                workflowException.ListErrorCheckValidScenario = errorlist;
                            }
                            else
                            {
                                oprationSuccess = workflowScenario.DasctiveOrActivateWorkflow(true);
                                EnableSchedule();
                            }

                        }
                        break;
                    case Operation.UNCHECKED_WORKFLOW:
                        // dacive scenario
                        if (workflowScenario.Save())
                        {
                            string sError = string.Empty;
                            // check if scenario with recipients who are already in the waiting stage
                            int nbrRecipients = workflowScenario.CountNumberOfCurrentTracking(out sError);
                            if (!string.IsNullOrEmpty(sError))
                            {
                                oprationSuccess = false;
                                workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, sError);

                            }
                            else
                            {
                                if (nbrRecipients > 0)
                                {
                                    oprationSuccess = false;
                                    workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, "");
                                    workflowException.Warning = string.Format(nbrRecipients > 1 ? eResApp.GetRes(_pref, 8946) : eResApp.GetRes(_pref, 8952), nbrRecipients.ToString());
                                }
                                else
                                {
                                    // update scenario status and disable job
                                    oprationSuccess = workflowScenario.DasctiveOrActivateWorkflow(false);
                                    if (workflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                                        oprationSuccess = DisabelSchedule();
                                }
                            }
                        }
                        break;
                    case Operation.END_TRACKING_STEP:
                        string error = string.Empty;
                        Boolean isUpdated = workflowScenario.UpdateTracking(_pref,out error);
                        if(!string.IsNullOrEmpty(error))
                        {
                            oprationSuccess = false;
                            workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, error);
                        }
                        else
                        {
                            if (isUpdated)
                            {
                                oprationSuccess = workflowScenario.DasctiveOrActivateWorkflow(false);
                            }
                        }
                      
                        break;
                    default:
                        break;
                }
                return oprationSuccess;
            }
            catch (Exception ex)
            {
                oprationSuccess = false;
                workflowException = new eWorkflowException(eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS, "an error occurred while retrieving paramters for the workflow");
                return false;
            }

        }


        // DELETE  
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }

        private WorkflowResponseModel RenderResponse(bool success)
        {
            #region Initialisation du retour JSON, remplissage et rendu

            WorkflowResponseModel response = new WorkflowResponseModel();

            response.Success = success;

            bool bError = workflowException != null || !success;
            response.Success = !bError;

            if (bError)
            {
                if (workflowException != null)
                {
                    response.Message = workflowException.UserMessage;
                    response.Detail = workflowException.UserMessageDetails ?? "";// eResApp.GetRes(_pref, 6721);
                    response.ListErrorCheckValidScenario = workflowException.ListErrorCheckValidScenario;
                    response.Warning = workflowException.Warning;
                }
            }
            else
            {
                response.Message = eResApp.GetRes(_pref, 2772);
                response.Detail = eResApp.GetRes(_pref, 2773);
            }

            if (success || (workflowException != null && workflowException.Code == eWorkflowException.ErrorCode.CHECK_VAlID_PARAMS))
            {
                response.WorkflowTriggerId = workflowScenario.TriggerId;
                response.WorkflowTriggerStepId = workflowScenario.WorkflowTriggerStepId;
                response.ScenarioId = workflowScenario.WFSCId;
                response.ActionResult = workflowScenario.ActionResult;
                if (workflowScenario.TriggerType == Common.Enumerations.WorkflowTriggerType.ONADDRECURENT)
                    response.SchedulerJobId = workflowScenario.SchedulerJobId;
            }

            #endregion
            return response;
        }


        /// <summary>Convertit les données interne de la plannification en objet eScheduledJobData</summary>
        /// <returns></returns>
        private eScheduledJobData GetScheduledJobData()
        {
            eScheduledJobData schedule = new eScheduledJobData();

            if (workflowScenario.ScheduleId != 0)
            {
                //Chargement des infos de la plannification depuis la BDD
                eScheduleObject scheduleXRM = new eScheduleObject(workflowScenario.ScheduleId);
                scheduleXRM.LoadFromDB(_pref, false);

                //Copie des infos du schedule de XRM dans le eScheduledJobData
                schedule.Active = false;
                schedule.FrequencyType = scheduleXRM.Type;
                schedule.Frequency = scheduleXRM.Frequency;
                schedule.Day = scheduleXRM.Day;
                schedule.Order = scheduleXRM.Order;
                schedule.WeekDays = scheduleXRM.WeekDays;
                schedule.Month = scheduleXRM.Month;
                schedule.SetStartDate(scheduleXRM.BeginDate);
                if (scheduleXRM.EndDate.HasValue)
                    schedule.SetEndDate(scheduleXRM.EndDate.Value);
                schedule.Repeat = scheduleXRM.RangeCount;
                if (scheduleXRM.Time.HasValue)
                    schedule.SetHour(scheduleXRM.Time.Value);
                schedule.ScheduleId = workflowScenario.SchedulerJobId;

                //Transformation des infos du job en JSON
                eScheduledJob job = new eScheduledJob();
                job.Type = Internal.wcfs.data.common.TaskJobType.RECURRENT_ACTION;
                JobRecurrentActionCall _JobRecurrentActionCall = new JobRecurrentActionCall();
                _JobRecurrentActionCall.InfoSql = GetInfoSql();
                _JobRecurrentActionCall.InfoDb = GetInfoDatabase();
                _JobRecurrentActionCall.WorkflowId = workflowScenario.WFSCId;
                _JobRecurrentActionCall.ScheduleId = workflowScenario.ScheduleId;
                _JobRecurrentActionCall.StepId = workflowScenario.WorkflowTriggerStepId;

                job.Param = JsonConvert.SerializeObject(_JobRecurrentActionCall);

                //Ajout du JSON
                schedule.Jobs = new List<eScheduledJob>() { job };
            }

            return schedule;
        }

        /// <summary>
        /// Récupère un InfoSql d'après les paramètres internes de la fiche
        /// </summary>
        /// <returns></returns>
        private InfoSql GetInfoSql()
        {
            InfoSql sqlInfos = new InfoSql()
            {
                ApplicationName = _pref.GetSqlApplicationName,
                Instance = _pref.GetSqlInstance,
                User = _pref.GetSqlUser,
                Password = _pref.GetSqlPassword,
            };

            return sqlInfos;
        }

        /// <summary>
        /// Récupère un JobCallCommon.InfoDatabase d'après les paramètres internes de la fiche
        /// </summary>
        /// <returns></returns>
        private InfoDatabase GetInfoDatabase()
        {
            InfoDatabase dbInfos = new InfoDatabase()
            {
                Name = _pref.GetBaseName,
                Uid = _pref.DatabaseUid
            };

            return dbInfos;
        }

        /// <summary>
        /// Crée la plannification au niveau du plannificateur EudoProcess
        /// </summary>
        private void AddOrUpdateSchedule(out Dictionary<int, string> errorlist)
        {
            errorlist = new Dictionary<int, string>();
            if (workflowScenario.ScheduleId == 0)
                return;

            eScheduledJobRun result = null;
            eScheduledJobIdentifier identifier = new eScheduledJobIdentifier()
            {
                BaseName = _pref.GetBaseName,
                UserId = _pref.UserId,
                Lang = _pref.Lang
            };

            if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");
            eScheduledJobData scheduleData = GetScheduledJobData();
            if (scheduleData.ScheduleId <= 0)//ajout
                result = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.AddSchedule(identifier, scheduleData));
            else
            {
                this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.DeleteSchedule(scheduleData.ScheduleId));
                result = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.AddSchedule(identifier, scheduleData));
            }

            if (result == null)
                return;
            if (!result.Success)
            {
                eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                try
                {
                    dal.OpenDatabase();
                    List<DateTime> dates = eScheduleInfos.GetScheduleDate(dal, workflowScenario.ScheduleId);
                    if (dates.Max() < DateTime.Now)
                    {
                        if (!errorlist.ContainsKey(0))
                            errorlist.Add(0, eResApp.GetRes(_pref, 8924));
                    }
                }
                finally
                {
                    dal.CloseDatabase();
                }
            }
            else if (result.ServerScheduleId > 0)
            {
                workflowScenario.SchedulerJobId = result.ServerScheduleId;
                workflowScenario.UpdateReccurentTrigger(workflowScenario.SchedulerJobId);
            }
        }

        /// <summary>
        /// Desactiver le job
        /// </summary>
        private bool DisabelSchedule()
        {
            if (workflowScenario.SchedulerJobId == 0)
                return false;
            try
            {
                if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                    this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.DisableSchedule(workflowScenario.SchedulerJobId));
            }
            catch
            {
                throw;
            }
        }



        /// <summary>
        /// Desactiver le job
        /// </summary>
        private bool EnableSchedule()
        {
            if (workflowScenario.SchedulerJobId == 0)
                return false;
            try
            {
                if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                    this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.EnableSchedule(workflowScenario.SchedulerJobId));
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Desactiver le job
        /// </summary>
        private bool DeleteSchedule()
        {
            if (workflowScenario.SchedulerJobId == 0)
                return false;
            try
            {
                if (string.IsNullOrEmpty(this._WCFSchedulerUrl))
                    this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");

                return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.DeleteSchedule(workflowScenario.SchedulerJobId));
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Crée un objet de communication en WCF et execute la fonction en param
        /// </summary>
        /// <param name="url">URL pour appler le WCF</param>
        /// <param name="work">L'objet à envoyer</param>
        private T CallWCF<T>(string url, Func<IEudoScheduledJobsWCF, T> work)
        {
            string err;
            eWCFBasic<IEudoScheduledJobsWCF> wcfAccess = null;

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("URL du WCF a appeler vide !");

            try
            {
                return eWCFTools.WCFEudoProcessCaller(url, work);
            }
            catch (EndpointNotFoundException ExWS)
            {
                err = string.Concat("Module de plannification injoignable : ", Environment.NewLine, ExWS.ToString());
                throw new EudoException(err, "Module de plannification injoignable", ExWS);
            }
            catch
            {
                throw;
            }
            finally
            {
                wcfAccess?.Dispose();
            }
        }

    }
}
