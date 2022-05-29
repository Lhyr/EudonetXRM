using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.common;
using Com.Eudonet.Internal.wcfs.data.mailing;
using Com.Eudonet.Internal.wcfs.data.scheduledjob;
using Com.Eudonet.Internal.wcfs.data.scheduledjob.jobs;
using Com.Eudonet.Internal.wcfs.data.treatment;
using Com.Eudonet.Internal.xrm;
using EudoProcessInterfaces;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe eEventStep pour la gestion des event "Étapes" en XRM avec appels vers les WCF du plannificateur
    /// </summary>
    public class eEventStepXRM : eEventStep
    {
        /// <summary>Paramètres utilisateur</summary>
        private ePref _ePref;

        /// <summary>URL pour les appels WCF au plannificateur</summary>
        private string _WCFSchedulerUrl;

        /// <summary>URL pour les appels WCF à l'emailing</summary>
        private string _WCFMailingUrl;

        /// <summary>URL pour les appels WCF à l'ajout des destinataires</summary>
        private string _WCFInvitRecipUrl;

        /// <summary>
        /// 
        /// </summary>
        public eTreatmentCall TreatmentCall { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="nTab">DescId de la table</param>
        /// <param name="nFileId">Id de la fiche</param>
        public eEventStepXRM(ePref pref, int nTab, Int32 nFileId = 0)
            : base(pref, pref.User, eModelTools.GetRootPhysicalDatasPath(), nTab, nFileId)
        {
            this._ePref = pref;
            this._WCFSchedulerUrl = ConfigurationManager.AppSettings.Get("EudoScheduledJobsURL");
            this._WCFMailingUrl = ConfigurationManager.AppSettings.Get("EudoMailingURL");
            this._WCFInvitRecipUrl = ConfigurationManager.AppSettings.Get("EudoTreatmentURL");
        }

        #region Plannificateur

        /// <summary>Convertit les données interne de la plannification en objet eScheduledJobData</summary>
        /// <returns></returns>
        private eScheduledJobData GetScheduledJobData()
        {
            eScheduledJobData schedule = new eScheduledJobData();

            if (this.ScheduleId != 0)
            {
                //Chargement des infos de la plannification depuis la BDD
                eScheduleObject scheduleXRM = new eScheduleObject(this.ScheduleId);
                scheduleXRM.LoadFromDB(_ePref, false);

                //Copie des infos du schedule de XRM dans le eScheduledJobData
                schedule.Active = true;
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

                //Transformation des infos du job en JSON
                eScheduledJob job = new eScheduledJob();
                job.Type = GetScheduledJobType();
                //SHA
                if (job.Type == TaskJobType.MAILING)
                    job.Param = JsonConvert.SerializeObject(GetJobMailCall());
                else if (job.Type == TaskJobType.ADD_RECIPIENT)
                    job.Param = JsonConvert.SerializeObject(GetJobInvitRecipCall());

                if (schedule.FrequencyType == Internal.eLibConst.SCHEDULE_FREQUENCY.SCHEDULE_DAILY_WORKING_DAY)
                {
                    if (schedule.WeekDays.Count() == 0)
                    {
                        // TODO - Recuperer les préférences du planning de l'utilisateur pour obtenir les jours de travail de la personne

                        // Par defaut, les jours de travail sont du lundi au vendredi
                        schedule.WeekDays = new HashSet<int>()
                        {
                            (int)DayOfWeek.Monday +1,
                            (int)DayOfWeek.Tuesday +1,
                            (int)DayOfWeek.Wednesday+1,
                            (int)DayOfWeek.Thursday+1,
                            (int)DayOfWeek.Friday+1,
                        };
                    }
                }

                //Ajout du JSON
                schedule.Jobs = new List<eScheduledJob>() { job };
            }

            return schedule;
        }

        /// <summary>
        /// Récupère un JobMailCall d'après les paramètres internes de la fiche
        /// </summary>
        /// <returns></returns>
        private JobMailCall GetJobMailCall()
        {
            eMailingCall mailingCall = new eMailingCall();
            //eMailingCall est utilisé pour faire l'envoi des campagnes "enfants" qui n'existent donc pas encore à ce moment là,
            //donc on laisse le paramètre vide
            //mailingCall.CampaignId = 0;
            mailingCall.UserId = _ePref.User.UserId;
            mailingCall.Lang = _ePref.Lang;
            //mailingCall.PrefSQL = null;//_ePref.GetNewPrefSql() //Inutile de sérialiser ça ici, le PrefSQL est reconstitué dans le JobMail
            mailingCall.AppExternalUrl = _ePref.AppExternalUrl;
            mailingCall.SecurityGroup = (int)_ePref.GroupMode;
            mailingCall.UID = _ePref.DatabaseUid;
            //GCH 20140211 : on ne doit passer au WCFque le chemin des datas commun à chaques bases (ex:d:\datas et surtout pas d:\datas\DEVTEST_C2...)
            mailingCall.DatasPath = this.GlobalDatasPath;

            JobMailCall param = new JobMailCall();
            param.InfoSql = GetInfoSql();
            param.InfoDb = GetInfoDatabase();
            param.CampaignId = this.CampaignId;
            param.WCFMailingUrl = this._WCFMailingUrl;
            mailingCall.MarketStepInfos = new eMarketStepInfos()
            {
                Tab = this.Tab,
                FileId = this.FileId
            };

            param.MailingCall = mailingCall;

            return param;
        }

        /// <summary>
        /// Récupère un JobInvitRecipCall après les paramètres internes de la fiche
        /// </summary>
        /// <returns></returns>
        private JobInvitRecipCall GetJobInvitRecipCall()
        {
            if (TreatmentCall == null)
                throw new ArgumentNullException("TreatmentCall doit être renseigné dans le cas d'un JobInvitRecipCall");

            JobInvitRecipCall param = new JobInvitRecipCall();
            param.InfoSql = GetInfoSql();
            param.InfoDb = GetInfoDatabase();
            param.EventStepDescId = this.Tab;
            param.EventStepFileId = this.FileId;
            param.DatasPath = this.GlobalDatasPath;
            param.WCFInvitRecipUrl = this._WCFInvitRecipUrl;
            param.TreatmentCall = TreatmentCall;

            return param;
        }

        /// <summary>
        /// Récupère un InfoSql d'après les paramètres internes de la fiche
        /// </summary>
        /// <returns></returns>
        private InfoSql GetInfoSql()
        {
            InfoSql sqlInfos = new InfoSql()
            {
                ApplicationName = _ePref.GetSqlApplicationName,
                Instance = _ePref.GetSqlInstance,
                User = _ePref.GetSqlUser,
                Password = _ePref.GetSqlPassword,
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
                Name = _ePref.GetBaseName,
                Uid = _ePref.DatabaseUid
            };

            return dbInfos;
        }

        /// <summary>
        /// Récupère un ScheduledJobType en fonction du EventStepType
        /// </summary>
        /// <returns></returns>
        private TaskJobType GetScheduledJobType()
        {
            switch (this._type)
            {
                case EventStepType.SMSING_SENDING:
                case EventStepType.EMAILING_SENDING:
                    return TaskJobType.MAILING;
                case EventStepType.SOURCE_ADD:
                    return TaskJobType.ADD_RECIPIENT;
                default:
                    return TaskJobType.UNDEFINED;
            }
        }

        /// <summary>
        /// Crée ou Met à jour la plannification au niveau du plannificateur EudoProcess
        /// </summary>
        public void AddOrUpdateSchedule()
        {
            if (this._schedulerJobId == 0)
                AddSchedule();
            else
                UpdateSchedule();
        }

        /// <summary>
        /// Crée la plannification au niveau du plannificateur EudoProcess
        /// </summary>
        private void AddSchedule()
        {
            eScheduledJobIdentifier identifier = new eScheduledJobIdentifier()
            {
                BaseName = this._ePref.GetBaseName,
                UserId = this._ePref.UserId,
                Lang = this._ePref.Lang
            };
            eScheduledJobData scheduleData = GetScheduledJobData();
            eScheduledJobRun result = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.AddSchedule(identifier, scheduleData));

            if (!result.Success)
                throw new SchedulerEventStepException(string.Format("{0}.{1} : Une erreur est survenue.", nameof(eEventStepXRM), nameof(AddSchedule)));
            else
            {
                this._schedulerJobId = result.ServerScheduleId;
                UpdateSchedulerId();
            }
        }

        /// <summary>
        /// Mets à jour la plannification au niveau du plannificateur EudoProcess
        /// </summary>
        private void UpdateSchedule()
        {
            throw new NotImplementedException();
            // TODO - Une fois que la méthode ChangeSchedule sera implémentée dans le plannificateur
            /*
            if (this._schedulerJobId != 0)
            {
                eScheduledJobData scheduleData = GetScheduledJobData();
                bool success = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.ChangeSchedule(this._schedulerJobId, scheduleData));

                if (!success)
                    throw new SchedulerEventStepException(String.Format("{0}.{1} : Une erreur est survenue.", nameof(eEventStepXRM), nameof(UpdateSchedule)));
            }
            */
        }

        /// <summary>
        /// Suppression de la plannification au niveau du plannificateur EudoProcess
        /// </summary>
        public void DeleteSchedule()
        {
            if (this._schedulerJobId != 0)
            {
                try
                {
                    bool success = this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.DeleteSchedule(this._schedulerJobId));

                    // Si success a false = indique que l'item n'est pas présent dans le moteur de planification Ce qui, par exemple, peut arriver si la planification est terminée
                }
                catch (Exception)
                {
                    throw new SchedulerEventStepException(string.Format("{0}.{1} : Une erreur est survenue.", nameof(eEventStepXRM), nameof(DeleteSchedule)));
                }
            }
        }



        /// <summary>
        /// Change le status actif/inactif d'un schedule
        /// </summary>
        /// <param name="bActivate"></param>
        /// <returns></returns>
        public bool SwitchActive(bool bActivate)
        {
            if (this._schedulerJobId != 0)
            {
                try
                {
                    if (bActivate)
                        return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.EnableSchedule(this._schedulerJobId));
                    else
                        return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.DisableSchedule(this._schedulerJobId));

                    // Si success a false = indique que l'item n'est pas présent dans le moteur de planification Ce qui, par exemple, peut arriver si la planification est terminée
                }
                catch (Exception)
                {
                    throw new SchedulerEventStepException(string.Format("{0}.{1} : Une erreur est survenue.", nameof(eEventStepXRM), nameof(DeleteSchedule)));
                }
            }

            return false;
        }

        public bool EnableSchedule()
        {
            if (this._schedulerJobId != 0)
            {
                try
                {
                    return this.CallWCF(this._WCFSchedulerUrl, SchedulerJob => SchedulerJob.EnableSchedule(this._schedulerJobId));

                    // Si success a false = indique que l'item n'est pas présent dans le moteur de planification Ce qui, par exemple, peut arriver si la planification est terminée
                }
                catch (Exception)
                {
                    throw new SchedulerEventStepException(string.Format("{0}.{1} : Une erreur est survenue.", nameof(eEventStepXRM), nameof(DeleteSchedule)));
                }
            }

            return false;
        }

        #endregion

        #region WCF

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
                throw new SchedulerEventStepException(err);
            }
            catch (Exception ex)
            {
                err = string.Concat("Une erreur est survenue ", ex.ToString());
                throw new SchedulerEventStepException(err);
            }
            finally
            {
                wcfAccess?.Dispose();
            }
        }

        #endregion
    }
}