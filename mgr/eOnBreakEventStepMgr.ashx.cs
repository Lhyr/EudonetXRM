using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.engine.job.cru;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.scheduledjob;
using Com.Eudonet.Internal.wcfs.data.TreatmentCampaign;
using EudoProcessInterfaces;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eOnBreakEventStepMgr
    /// </summary>
    public class eOnBreakEventStepMgr : eEudoManager
    {
        private string _sWCFUrl = "";



        /// <summary>
        /// Active ou désactive une étape marketting récurrente passée en parm
        /// </summary>
        /// <param name="nScheduleId"></param>
        /// <param name="active"></param>
        /// <param name="dal"></param>
        /// <param name="nEventStepDescId"></param>
        /// <param name="nEventStepFileId"></param>
        /// <returns></returns>
        private bool SwitchActiveSchedule(int nScheduleId, bool active, eudoDAL dal, int nEventStepDescId, int nEventStepFileId)
        {


            eEventStepXRM eventStep = new eEventStepXRM(_pref, nEventStepDescId, nEventStepFileId);
            string eventStepError;
            eventStep.LoadEventStepFile(out eventStepError, dal);

            //impossile de loader l'étape
            if (!string.IsNullOrEmpty(eventStepError))
                throw new EudoException("Erreur lors de la demande de mise en pause du schedule " + nScheduleId + " -  Erreur 1 ", "Erreur lors de la demande de mise en pause du schedule " + nScheduleId);

            //id d'étape différent
            if (eventStep.ScheduleJobId != nScheduleId)
                throw new EudoException("Erreur lors de la demande de mise en pause du schedule " + nScheduleId + " -  Erreur 2 ", "Erreur lors de la demande de mise en pause du schedule " + nScheduleId);

            //etape non récurrente
            if (eventStep.ExecutionMode != EventStepExecutionMode.RECURRENT)
                throw new EudoException("Erreur lors de la demande de mise en pause du schedule " + nScheduleId + " -  Erreur 3 ", "Erreur lors de la demande de mise en pause du schedule " + nScheduleId);

            if (eventStep.SwitchActive(active))
            {
                // Met l'étape campagne en pause/enregistré
                Engine.Engine eng = eModelTools.GetEngine(_pref, nEventStepDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = nEventStepFileId;
                eng.AddNewValue(nEventStepDescId + (int)EventStepField.DESCID_STEP_DISABLED, active ? "0" : "1");
                eng.AddNewValue(nEventStepDescId + (int)EventStepField.DESCID_STEP_STATUS, (active ? (int)EventStepStatus.DEFAULT : (int)EventStepStatus.PAUSED).ToString()); ;

                eng.EngineProcess(new StrategyCRUSimpeUpdateStep());
            }

            return true;
        }


        /// <summary>
        /// lancement des actions
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnOnBreakEvent res = new JSONReturnOnBreakEvent();

            try
            {

                int nParentFileId = 0;
                int nParentTabId = 0;
                bool bOnBreak = false;

                try
                {
                    //mode en request.form (xrm)
                    if (_requestTools.AllKeys.Count > 0)
                    {
                        nParentFileId = (int)_requestTools.GetRequestFormKeyI("ParentFileId");
                        nParentTabId = (int)_requestTools.GetRequestFormKeyI("ParentTabId");
                        bOnBreak = (int)_requestTools.GetRequestFormKeyI("Status") == 1;
                    }
                    else
                    {
                        //mode flux json dans le body (iris)
                        var def = new { Status = 0, ParentTabId = 0, ParentFileId = 0 };
                        var jsonAPIParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);

                        nParentFileId = jsonAPIParam.ParentFileId;
                        nParentTabId = jsonAPIParam.ParentTabId;
                        bOnBreak = jsonAPIParam.Status == 1;
                    }
                }
                catch
                {
                    throw new EudoException("Paramètres invalide/non fournis pour la mise en pause", "Paramètres invalide/non fournis pour la mise en pause");
                }

                if (nParentFileId == 0 || nParentTabId == 0)
                    throw new EudoException("Paramètres invalide/non fournis pour la mise en pause", "Paramètres invalide/non fournis pour la mise en pause");

                eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                string sBaseName = dal.dbname;
                string err = string.Empty;
                string sError = string.Empty;


                //liste des étapes
                List<int> stepId = new List<int>();

                //listes des schedules récurrents
                List<int> lstRecurrentScheduleId = new List<int>();


                //liste des campagne recurrente
                List<int> campaignIdReccurent = new List<int>();

                //liste des campagne différées
                List<int> campaignsDelayedId = new List<int>();

                eOnBreakCampaignsResponse result = new eOnBreakCampaignsResponse();

                try
                {


                    if (nParentTabId != 0 && nParentFileId != 0)
                    {
                        dal.OpenDatabase();


                        //informations sur les étapes
                        DescAdvDataSet descAdv = new DescAdvDataSet();
                        descAdv.LoadAdvParams(dal, new int[] { nParentTabId }, new DESCADV_PARAMETER[] {
                            DESCADV_PARAMETER.EVENT_STEP_DESCID,
                            DESCADV_PARAMETER.EVENT_STEP_ENABLED });


                        bool eventStepEnabled = eLibTools.GetNum(descAdv.GetAdvInfoValue(nParentTabId, DESCADV_PARAMETER.EVENT_STEP_ENABLED, "0")) == 1;

                        int eventStepDescId = eLibTools.GetNum(descAdv.GetAdvInfoValue(nParentTabId, DESCADV_PARAMETER.EVENT_STEP_DESCID, "0"));
                        _sWCFUrl = ConfigurationManager.AppSettings.Get("EudoPauseCampaign");

                        if (!eventStepEnabled || eventStepDescId == 0)
                            throw new EudoException("Cette table n'a pas d'étapes marketting", "Cette table n'a pas d'étapes marketting");



                        //Chargement des valeurs de catalogue

                        // type d'étape
                        // Enum EventStepType
                        eCatalog catalogType = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, eventStepDescId + (int)EventStepField.DESCID_STEP_TYPE);
                        var listCatType = catalogType.Values;

                        int stepMail = eLibTools.GetCatValueIdByData((int)EventStepType.EMAILING_SENDING, listCatType);
                        int stepSMS = eLibTools.GetCatValueIdByData((int)EventStepType.SMSING_SENDING, listCatType);
                        int stepAdd = eLibTools.GetCatValueIdByData((int)EventStepType.SOURCE_ADD, listCatType);

                        //Mode execution
                        // Enum : EventStepExecutionMode
                        eCatalog catalogExecutionMode = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, eventStepDescId + (int)EventStepField.DESCID_STEP_EXECUTION_MODE);
                        var listCatExecutionMode = catalogExecutionMode.Values;


                        int executeModeRecurr = eLibTools.GetCatValueIdByData((int)EventStepExecutionMode.RECURRENT, listCatExecutionMode);
                        if (executeModeRecurr == 0)
                            executeModeRecurr = catalogExecutionMode.InsertCatValue(1975, ((int)EventStepExecutionMode.RECURRENT).ToString()); //Récurent


                        int executeModeDelayed = eLibTools.GetCatValueIdByData((int)EventStepExecutionMode.DELAYED, listCatExecutionMode);
                        if (executeModeDelayed == 0)
                            executeModeDelayed = catalogExecutionMode.InsertCatValue(1974, ((int)EventStepExecutionMode.DELAYED).ToString()); //Récurent



                        string sEvtName = eLibTools.GetEventNameFromDescId(eventStepDescId);
                        //liste de status a ne pas changer
                        List<int> lstStep = new List<int>()
                        {
                            (int)EventStepStatus.CANCELED,
                            (int)EventStepStatus.CANCELLATION_ATTEMPT,
                            (int)EventStepStatus.COMPLETED,
                            (int)EventStepStatus.FAILED,
                            (int)EventStepStatus.IN_PROGRESS,

                        };

                        Dictionary<int, string> dicError = new Dictionary<int, string>();

                        #region Campagne Récurrente
                        string sSQLRecurringCampaign = @"SELECT [CAMPAIGNID], [EVTID], [EVT19], [EVT20]
                                FROM [" + sEvtName + @"] STEP
                                INNER JOIN CAMPAIGN ON [CAMPAIGNID] = [STEP].[EVT20] AND [CAMPAIGN].[STATUS] = @CAMPAIGNReccuring
                                WHERE [STEP].[ParentEvtId] = @PARENTFILEID AND isnull([STEP].[EVT20],0) > 0  
                                AND ISNULL(STEP.[EVT19],0) > 0
                                AND ISNULL(STEP.[EVT24],0) NOT IN (  " + String.Join(",", lstStep) + @")
                                AND ( ISNULL(STEP.[EVT02],0) = @TYPE OR ISNULL(STEP.[EVT02],0) = @TYPESMS)
                                AND ISNULL([STEP].[EVT06],0) = @EXECUTEMODE";




                        RqParam rqReccuringCampaign = new RqParam(sSQLRecurringCampaign);
                        rqReccuringCampaign.AddInputParameter("@PARENTFILEID", SqlDbType.Int, nParentFileId);
                        rqReccuringCampaign.AddInputParameter("@CAMPAIGNReccuring", SqlDbType.Int, (int)CampaignStatus.MAIL_RECURRENT);
                        rqReccuringCampaign.AddInputParameter("@EXECUTEMODE", SqlDbType.Int, executeModeRecurr);
                        rqReccuringCampaign.AddInputParameter("@TYPE", SqlDbType.Int, stepMail);
                        rqReccuringCampaign.AddInputParameter("@TYPESMS", SqlDbType.Int, stepSMS);


                        DataTableReaderTuned dtrRecurringd = dal.Execute(rqReccuringCampaign);

                        campaignIdReccurent = new List<int>();

                        while (dtrRecurringd.Read())
                        {

                            //Id Step
                            int idStep = dtrRecurringd.GetEudoNumeric("EVTID");

                            //Id de campagne
                            int valueCampaignId = (int)dtrRecurringd.GetDecimal("CAMPAIGNID");

                            //Id de schedule
                            int recSchedule = dtrRecurringd.GetEudoNumeric("EVT19");

                            try
                            {

                                //Appel du wcf d'arret de schedule de l 'étape
                                if (SwitchActiveSchedule(recSchedule, !bOnBreak, dal, eventStepDescId, idStep))
                                {
                                    //Maj  campagne
                                    RqParam rqCp = new RqParam("UPDATE [CAMPAIGN] SET [ONBREAK] = @ONBREAK WHERE [CAMPAIGNID] = @ID");
                                    rqCp.AddInputParameter("@ONBREAK", SqlDbType.Int, bOnBreak ? 1 : 0);
                                    rqCp.AddInputParameter("@ID", SqlDbType.Int, valueCampaignId);
                                    dal.Execute(rqCp);
                                }
                                else
                                {
                                    //ajout step et campagne à la liste d'erreur
                                    dicError.Add(idStep, "Erreur lors de la mise en pause de la campagne [" + valueCampaignId + "] - Erreur sur l'appel WCF");

                                }
                            }
                            catch (Exception)
                            {
                                dicError.Add(idStep, "Erreur lors de la mise en pause de la campagne [" + valueCampaignId + "]");
                            }

                        }
                        dtrRecurringd = null;
                        #endregion

                        #region Ajout récurrent


                        string sSQLRecurringADD = @"SELECT [EVTID], [EVT19], [EVT20]
                                FROM [" + sEvtName + @"] STEP                             
                                WHERE [STEP].[ParentEvtId] = @PARENTFILEID AND isnull([STEP].[EVT20],0) = 0  
                                AND ISNULL(STEP.[EVT19],0) > 0
                                AND ISNULL(STEP.[EVT02],0) = @TYPE
                                AND ISNULL(STEP.[EVT24],0) NOT IN (  " + String.Join(",", lstStep) + @")
                                AND ISNULL([STEP].[EVT06],0) = @EXECUTEMODE
";





                        RqParam rqReccuringADD = new RqParam(sSQLRecurringADD);
                        rqReccuringADD.AddInputParameter("@PARENTFILEID", SqlDbType.Int, nParentFileId);
                        rqReccuringADD.AddInputParameter("@EXECUTEMODE", SqlDbType.Int, executeModeRecurr);
                        rqReccuringADD.AddInputParameter("@TYPE", SqlDbType.Int, stepAdd);

                        DataTableReaderTuned dtrRecurringADD = dal.Execute(rqReccuringADD);

                        campaignIdReccurent = new List<int>();

                        while (dtrRecurringADD.Read())
                        {
                            int recSchedule = dtrRecurringADD.GetEudoNumeric("EVT19");
                            int idStep = dtrRecurringADD.GetEudoNumeric("EVTID");
                            //Appel du wcf d'arret de schedule de l 'étape
                            if (!SwitchActiveSchedule(recSchedule, !bOnBreak, dal, eventStepDescId, idStep))
                            {
                                //ajout step et campagne à la liste d'erreur
                                dicError.Add(idStep, "Erreur lors de la mise en pause de l'ajout réccurrent [" + recSchedule + "] - Erreur sur l'appel WCF");

                            }
                        }

                        dtrRecurringADD = null;
                        #endregion

                        #region campagne différé
                        //on ne peut qu'annulé les campagnes différées
                        if (bOnBreak)
                        {
                            string sSQLDelay = @"SELECT [CAMPAIGNID], [EVTID]
                                FROM [" + sEvtName + @"]
                                INNER JOIN CAMPAIGN ON [CampaignId] = [evt20]
                                WHERE ParentEvtId = @PARENTFILEID and isnull(EVT20,0) > 0
                                AND ISNULL([EVT06],0) = @EXECUTEMODE
                                AND campaign.Status = @CAMPAIGNDELAYED";

                            RqParam rqDelayed = new RqParam(sSQLDelay);
                            rqDelayed.AddInputParameter("@PARENTFILEID", SqlDbType.Int, nParentFileId);
                            rqDelayed.AddInputParameter("@EXECUTEMODE", SqlDbType.Int, executeModeDelayed);
                            rqDelayed.AddInputParameter("@CAMPAIGNDELAYED", SqlDbType.Int, (int)CampaignStatus.MAIL_DELAYED);

                            DataTableReaderTuned dtrDelayed = dal.Execute(rqDelayed);

                            campaignsDelayedId = new List<int>();

                            while (string.IsNullOrEmpty(sError) && dtrDelayed.Read())
                            {
                                int valueCampaignId = (int)dtrDelayed.GetDecimal("CAMPAIGNID");
                                int idStep = dtrDelayed.GetEudoNumeric("EVTID");

                                try
                                {

                                    eMailing oMailing = new eMailing(_pref, valueCampaignId, 0, dal, TypeMailing.MAILING_FROM_LIST);

                                    oMailing.Run(eMailing.MailingAction.CANCEL);

                                    Engine.Engine eng = eModelTools.GetEngine(_pref, eventStepDescId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                                    eng.FileId = idStep;
                                    eng.AddNewValue(eventStepDescId + (int)EventStepField.DESCID_STEP_DISABLED, "1");
                                    eng.AddNewValue(eventStepDescId + (int)EventStepField.DESCID_STEP_STATUS, ((int)EventStepStatus.CANCELED).ToString()); ;
                                    eng.EngineProcess(new StrategyCRUSimpeUpdateStep());


                                }
                                catch (Exception)
                                {
                                    dicError.Add(idStep, "Erreur lors de l'annulation de la campagne différée [" + valueCampaignId + "]");
                                }


                            }

                        }

                        #endregion

                        if (dicError.Count > 0)
                        {

                            res.Success = false;
                            foreach (var kvp in dicError)
                            {
                                res.ErrorDetailMsg += "Erreur sur l'étate " + kvp.Key.ToString() + " : " + kvp.Value;

                            }
                        }
                        else
                            res.Success = true;


                        // passage de l'event en actif/inactif - change le status même s'il y a eu des erreurs
                        string s = "UPDATE [" + eLibTools.GetEventNameFromDescId(nParentTabId) + "] SET [ONBREAK] = @ONBREAK WHERE [EVTID] = @EVTID";
                        RqParam rq = new RqParam(s);
                        rq.AddInputParameter("@EVTID", SqlDbType.Int, nParentFileId);
                        rq.AddInputParameter("@ONBREAK", SqlDbType.Int, bOnBreak ? 1 : 0);
                        dal.Execute(rq);

                        if (dal.InnerException != null)
                            throw dal.InnerException;

                        //nouveau status
                        res.Status = bOnBreak ? 1 : 0;

                    }
                }
                catch (Exception e)
                {
                    res.Success = false;
                    res.ErrorMsg = string.Concat("Une erreur est survenue: ", e.ToString());
                    res.ErrorDetailMsg = e.ToString();
                }
            }
            catch (EudoException ee)
            {
                res.Success = false;
                res.ErrorTitle = ee.UserMessageTitle;
                res.ErrorMsg = ee.UserMessage;
                res.ErrorDetailMsg = ee.UserMessageDetails;
                if (_pref.User.UserLevel > 99)
                {
                    res.ErrorDebugMsg = ee.DebugMessage;
                }
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorMsg = string.Concat("Une erreur est survenue");
                if (_pref.User.UserLevel > 99)
                {
                    res.ErrorDebugMsg = e.Message;
                }

            }

            RenderResult(RequestContentType.TEXT, delegate ()
        {
            return JsonConvert.SerializeObject(res);
        });
        }
    }

    /// <summary>
    /// classe de retour du manager de mise en pause
    /// </summary>
    public class JSONReturnOnBreakEvent : JSONReturnGeneric
    {
        /// <summary>
        /// status de pause (1)/actif (0)
        /// </summary>
        public int Status = 0;
    }
}