using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.synchroExchange;
using EudoQuery;
using EudoSynchroExchangeInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.Notif;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Cms;
using System.Diagnostics.Eventing.Reader;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.handlers
{
    /// <summary>
    /// Handler appelé à la réception d'une notification Exchange (ExchangeOnNotificationArrival) pour répliquer les modifications effectuées depuis Exchange sur Eudonet
    /// </summary>
    public class eSynchroHandler : IHttpHandler
    {
        /// <summary>
        /// Processus
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            EudoCallContract requestParam = null;
            eudoDAL cnEudoLog = null;
            eudoDAL cnBddClient = null;
            ePrefSQL prefSqlEudolog = null;
            ePrefSQL prefSqlClient = null;
            ePrefLite prefClient = null;
            string clientBaseName = string.Empty;
            string clientBaseUid = string.Empty;
            string appExternalUrl = string.Empty;
            string sError = string.Empty;
            int userId = 0;
            eSynchroEventNotificationController synchroController = new eSynchroEventNotificationController();

            EventContract pendingEvent = new EventContract();
            try
            {
                //Deserialisation parametre JSON 
                string jsonParam;
                jsonParam = new System.IO.StreamReader(context.Request.InputStream).ReadToEnd();
                requestParam = JsonConvert.DeserializeObject<EudoCallContract>(jsonParam);

                if (!IsProcessableEvent(requestParam.NotifiedEvent))
                {
                    return;
                }
                if (requestParam == null)
                    throw new Exception("Impossible de récuperer les paramètres en provenance du service de synchronisation");
                else if (requestParam.authentification == null)
                    throw new Exception("Les paramètres d'authentification ne sont pas renseignés");
                else if (string.IsNullOrEmpty(requestParam.authentification.HashDatabase)
                    || string.IsNullOrEmpty(requestParam.authentification.HashUser))
                {
                    throw new Exception("Les paramètres d'authentification en provenance du module de synchronisation ne sont pas correctement renseignés"
                        + Environment.NewLine
                        + "HashDatabase : " + requestParam.authentification.HashDatabase
                        + "HashUser : " + requestParam.authentification.HashUser);
                }

                //Recuperation instance Eudolog
                prefSqlEudolog = ePrefTools.GetDefaultPrefSql(eLibConst.DB_LOG);
                cnEudoLog = eLibTools.GetEudoDAL(prefSqlEudolog, eLibConst.DB_LOG);
                cnEudoLog.OpenDatabase();

                //Recuperation nom base de données client a partir du hash database
                string sSQL = string.Concat("SELECT d.* ",
                    " FROM [DATABASES] d INNER JOIN [HASH_TO_DB] h on d.[UID] = h.[BaseUID]",
                    " WHERE h.HashDatabase = @hashDatabase");
                RqParam rqClientBase = new RqParam(sSQL);
                rqClientBase.AddInputParameter("@hashDatabase", SqlDbType.Char, requestParam.authentification.HashDatabase);

                //Execution de la rqt
                DataTableReaderTuned dtrClientBase = cnEudoLog.Execute(rqClientBase, out sError);
                if (sError.Length > 0)
                    throw cnEudoLog.InnerException ?? new Exception(sError);
                if (!dtrClientBase.Read())
                    throw new Exception(string.Concat("Base de donnée ['", requestParam.authentification.HashDatabase, "'] non trouvée"));

                //Récuperation du nom de la BDD client et UID
                clientBaseName = dtrClientBase.GetString(eLibConst.EUDOLOG_DATABASES.Directory.ToString());
                clientBaseUid = dtrClientBase.GetString(eLibConst.EUDOLOG_DATABASES.uid.ToString());
                appExternalUrl = dtrClientBase.GetString(eLibConst.EUDOLOG_DATABASES.AppExternalUrl.ToString());
                appExternalUrl = eModelTools.GetBaseUrlXRM();
               
                //chargement objet DAL base client
                prefSqlClient = ePrefTools.GetDefaultPrefSql(clientBaseName);
                cnBddClient = eLibTools.GetEudoDAL(prefSqlClient, clientBaseName);
                cnBddClient.OpenDatabase();

                //Récuperation du user a partir du hashUser
                sSQL = string.Concat("SELECT * ",
                    " FROM [USER] ",
                    " WHERE [USER].HashUser = @hashUser");
                RqParam rqUser = new RqParam(sSQL);
                rqUser.AddInputParameter("@hashUser", SqlDbType.NVarChar, requestParam.authentification.HashUser);

                DataTableReaderTuned dtrUser = cnBddClient.Execute(rqUser, out sError);
                if (sError.Length > 0)
                    throw cnBddClient.InnerException ?? new Exception(sError);
                if (!dtrUser.Read())
                    throw new Exception(string.Concat("Utilisateur ['", requestParam.authentification.HashUser, "'] non trouvé"));

                //Recuperation userId et chargement eUserInfos
                userId = dtrUser.GetEudoNumeric("UserId");
                eUserInfo userInfos = new eUserInfo(userId, cnBddClient);

                //Recuperation d'un ePrefLite 
                prefClient = new ePrefLite(
                        prefSqlClient.GetSqlInstance, prefSqlClient.GetBaseName, prefSqlClient.GetSqlUser, prefSqlClient.GetSqlPassword, prefSqlClient.GetSqlApplicationName,
                        userInfos.UserId, userInfos.UserLang, SECURITY_GROUP.GROUP_READONLY);

                //Recuperation du datapath
                string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();




                eSynchroEventNotification notif = synchroController.GetEventNotification(cnBddClient, prefClient, requestParam);

                /* JAS : Si la notification ne contient pas d'évènement on annule le traitement (ce qui ne devrait jamais arriver en théorie
             * Mais comme nous n'avons aucune confiance en Microsoft et bien nous le géront ici.
             * */
                if (notif.NotifiedEvent == null)
                {
                    return;
                }


                //On verifie l'activation de la synchro           
                if (eLibTools.IsSynchroOffice365Activated(prefClient) && eLibTools.IsSynchroOffice365IsActivatedForUser(prefClient, userId.ToString()))
                {

                    ProcessNotification(notif, cnBddClient, prefClient, appExternalUrl, rootPhysicalDatasPath);

                    #region ANCIENNE GESTION CONSERVEE POUR ARCHIVE LORS DE LA REALEASE DE LA V2, A SUPPRIMER UNE FOIS TOUT VALIDE
                    /*
                    if (requestParam.NotifiedEvent != null)
                    {
                        int returnValue = 0;
                        string action = string.Empty;
                        string error = string.Empty;
                        int notifiedUserId = SynchroExchangeTools.GetUserIdFromEmailAddress(requestParam.NotifiedEvent.UserMail, cnBddClient, out error);

                        EventContract evt = requestParam.NotifiedEvent;   //Master

                        List<EventContract> recurringInstances = requestParam.listEventExchange != null ? requestParam.listEventExchange : new List<EventContract>(); // Instances
                        if (evt.IsRecurrent)
                        {
                            returnValue = TraiterRdvRecurrent(evt, notifiedUserId, recurringInstances, prefClient, userInfos, cnBddClient, rootPhysicalDatasPath,
                                appExternalUrl, clientBaseUid, out action);
                        }
                        else
                        {
                            returnValue = TraiterRdvSimple(evt, notifiedUserId, requestParam.authentification, prefClient, userInfos, cnBddClient, rootPhysicalDatasPath,
                                appExternalUrl, clientBaseUid, out action);

                            if (returnValue > 0)
                            {
                                string actionStr = "synchronisé";
                                switch (action)
                                {
                                    case "CRU": actionStr = "créé ou mis à jour"; break;
                                    case "CREA": actionStr = "créé"; break;
                                    case "UPD": actionStr = "mis à jour"; break;
                                    case "DEL": actionStr = "supprimé"; break;
                                }

                                string msg = string.Concat("Le rendez-vous a bien été ", actionStr, " sur Eudonet.");

                                IDictionary<string, string> additionalDataToLog = new Dictionary<string, string>();
                                additionalDataToLog.Add("EudoIdOrDeletionResult", returnValue.ToString());

                                SynchroExchangeTools.AddLogMessage(
                                    SynchroExchangeTools.LoggerType.Info.ToString(),
                                    msg,
                                    null,
                                    null,
                                    null,
                                    "ExchangeToEudonet",
                                    "eSynchroHandler_ProcessRequest",
                                    "Done",
                                    additionalDataToLog
                                 );
                            }
                        }
                    }
                    */
                    #endregion
                }
            }
            catch (Exception e)
            {
                string error = string.Concat(
                    "Erreur lors de la réception d'une notification en provenance du module de synchronisation");

                IDictionary<string, string> additionalDataToLog = new Dictionary<string, string>();

                SynchroExchangeTools.AddLogMessage(
                    SynchroExchangeTools.LoggerType.Error.ToString(),
                    error,
                    e,
                    null,
                    null,
                    "ExchangeToEudonet",
                    "eSynchroHandler_ProcessRequest",
                    "Failed",
                    additionalDataToLog
                 );

                string msg = string.Concat("Synchronisation Office 365 : ", error, "."
                    , Environment.NewLine, "HashDatabase : ", requestParam?.authentification?.HashDatabase
                    , Environment.NewLine, "HashUser : ", requestParam?.authentification?.HashUser
                    , Environment.NewLine, "Une erreur est survenue lors du traitement d'une notification Exchange à destination d'Eudonet. Voir les logs du module de synchronisation pour plus de détails."
                    , Environment.NewLine, e.ToString());

                eErrorContainer eErrCont = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, msg);
                eFeedback.LaunchFeedback(eErrCont, prefSqlClient, "");
            }
            finally
            {

                if (cnEudoLog.IsOpen)
                    cnEudoLog.CloseDatabase();

                if (cnBddClient.IsOpen)
                    cnBddClient.CloseDatabase();
            }
            context.Response.ContentType = "texte/brut";
            context.Response.Write("End of notification treatment");
        }


        #region Méthodes V1 destinées à être remplacées

        /// <summary>
        /// Gestion des rdvs récurrent(Creation, mise a jour de serie et occurence + suppression d'occurence)
        /// Exchange n'envoyant pas l'occurence dans le cas des maj et suppression on traite toute la liste
        /// </summary>
        /// <param name="NotifiedEvent">Event Exchange Source de la  notification</param>
        /// <param name="listEventExchange">Instances d'évènements dans le cas ou Notified Event est un RDV Récurrent</param>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="userInfo">Paramètres utilisateur</param>
        /// <param name="dal">Couche d'accès aux données</param>
        /// <param name="globalDatasPath">Chemins physique du dossier "Datas" de la base</param>
        /// <param name="appExternalUrl">Url d'accès aux CRM depuis l'internet</param>
        /// <param name="databaseUid">UID de la base de données dans EUDOLOG</param>
        /// <param name="action">Action demandée par Exchange (Modification/Suppression/Création)</param>
        /// <returns>0 si Erreur, 1 si Succès</returns>
        private int TraiterRdvRecurrent(EventContract NotifiedEvent, int notifiedUserId, List<EventContract> listEventExchange, ePrefLite pref, eUserInfo userInfo,
            eudoDAL dal, string globalDatasPath, string appExternalUrl, string databaseUid, out string action)
        {
            action = string.Empty;
            int returnValue = 0;
            string error = string.Empty;

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }
            error = string.Empty;

            int OrganiserId = userInfo.UserId;

            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            try
            {
                //List des id des rdv pour maj du schedule id
                List<int> listEvtId = new List<int>();

                /*On ne traite que les suppressions d occurence dans cette methode
                Cela est du au fait que la suppression d une serie ne permet pas de connaitre les infos de recurrences */
                EventContract eventToSuppres = null;

                //On recupere le premier element de la liste.
                //Comme notifiedEvent est le master et listEventExchange l'ensemble des instances on doit tout garder et traiter ainsi
                /*EventContract evtMasterExchange = listEventExchange.First();
                listEventExchange.RemoveAt(0);      */

                //Si c'est un recurrent sans master, cela veut dire qu'il est master lui meme
                if (string.IsNullOrEmpty(NotifiedEvent.ExchangeMasterRecordId))
                {
                    int masterEudoId = 0;
                    int scheduleId = 0;

                    //En modification de serie, on ne modifie pas l'event master mais seulement les occurences et le schedule
                    eFileMapResult result = SynchroExchangeTools.GetFileMapRecordInfo(dal, NotifiedEvent, OrganiserId, notifiedUserId);
                    eFileMapRecord master = result.Matches
                        .Where(m => m.PrimarySourceFileId == NotifiedEvent.ExchangeRecordCalUId && m.SecondarySourceFileId == NotifiedEvent.ExchangeRecordId).SingleOrDefault();

                    masterEudoId = master == null ? 0 : master.EudonetFileId;

                    if (masterEudoId == 0)
                    {
                        //On recupere l'id du master dans le cas de la création
                        masterEudoId = CreateUpdateDeleteEventOnEudonet(NotifiedEvent, notifiedUserId, null, pref, userInfo, dal, globalDatasPath,
                            appExternalUrl, databaseUid, out action);
                    }

                    //Si on ne trouve pas de master eudo => On tente de supprimer une fiche qui n'existe plus sur Eudo ou alors erreur a la création du master
                    if (masterEudoId > 0)
                    {
                        listEvtId.Add(masterEudoId);

                        //On creer ou on met a jour le schedule
                        scheduleId = SynchroExchangeTools.MajSchedule(dal, NotifiedEvent.RecurrenceInfo, masterEudoId);

                        //On recupere l'occurence a supprimer si existante
                        eventToSuppres = SynchroExchangeTools.GetOccurenceToDelete(dal, pref, listEventExchange, masterEudoId);

                        //Si on n'est pas dans le cas de suppression d'une occurence on créer ou met a jour toute la liste
                        if (eventToSuppres == null && NotifiedEvent.ExchangeAction != (int)SynchroExchangeMapping.ExchangeNotificationAction.Delete)
                        {
                            /*
                             * Dans le cadre d'un récurrent, on doit créer le master + une occurence de planning classique lui correspondant
                             * Ainsi pour un récurrent de 5 jours on a 5 fiches planning + 1 de "métadonnées" correspondant au master qui ne sera pas affichée.
                             */

                            CreateUpdateDeleteEventOnEudonet(NotifiedEvent, notifiedUserId, null, pref, userInfo, dal, globalDatasPath,
                                    appExternalUrl, databaseUid, out action, masterEudoId);
                            //On traite le reste des occurences (Creation et mise a jour)
                            foreach (EventContract eventExchange in listEventExchange)
                            {
                                eventExchange.EudoMasterRecordId = masterEudoId.ToString();
                                listEvtId.Add(CreateUpdateDeleteEventOnEudonet(eventExchange, notifiedUserId, null, pref, userInfo, dal, globalDatasPath,
                                    appExternalUrl, databaseUid, out action, masterEudoId));
                            }

                            //Mise a jour scheduleId sur les fiches template
                            SynchroExchangeTools.MajScheduleId(dal, pref, listEvtId, scheduleId);
                        }
                        else if (eventToSuppres != null)
                        {
                            //Suppression d'une occurence de la serie
                            CreateUpdateDeleteEventOnEudonet(eventToSuppres, notifiedUserId, null, pref, userInfo, dal, globalDatasPath,
                                    appExternalUrl, databaseUid, out action);
                        }
                    }

                }

                returnValue = 1;
            }
            catch (Exception e)
            {
                throw e;
            }

            return returnValue;
        }

        /// <summary>
        /// Cas particulier de la suppression de serie
        /// </summary>
        /// <param name="eventExchange"></param>
        /// <param name="pref"></param>
        /// <param name="userInfo"></param>
        /// <param name="dal"></param>
        /// <param name="globalDatasPath"></param>
        /// <param name="appExternalUrl"></param>
        /// <param name="databaseUid"></param>
        private int SupprimerRdvRecurrent(EventContract eventExchange, int notifiedUserId, ePrefLite pref, eUserInfo userInfo,
            eudoDAL dal, string globalDatasPath, string appExternalUrl, string databaseUid)
        {
            int returnValue = 0;

            try
            {

                string deletePp = "0";
                string openSerie = "1";
                string sError = string.Empty;

                EngineResult engResult = null;

                //On recupere les informations necessaires
                //int masterEudoId = SynchroExchangeTools.GetEudoIdFromExchangeId(dal, eventExchange.ExchangeRecordId);
                eFileMapResult result = SynchroExchangeTools.GetFileMapRecordInfo(dal, eventExchange, userInfo.UserId, 0);
                eFileMapRecord master = result.GetMasterRecord();
                int masterEudoId = master.EudonetFileId;

                if (masterEudoId > 0)
                {
                    int scheduleId = SynchroExchangeTools.GetScheduleIdFromMasterFileId(dal, masterEudoId);
                    if (scheduleId > 0)
                    {
                        int eudoTabId = SynchroExchangeMapping.GetMappingTableDescId(pref);
                        string tabName = eLibTools.GetTabNameFromDescId(dal, eudoTabId, out sError);

                        //Pour la suppression d'une serie, il faut récuperer la premiere occurence de la serie pour la fournir a engine
                        string cmdSql = string.Concat("SELECT TOP 1 [TPLID] FROM ", tabName, " WHERE isnull([TPL", (PlanningField.DESCID_SCHEDULE_ID.GetHashCode()), "], 0) = @scheduleId AND [TplId] <> @masterfileid");
                        RqParam rqTplId = new RqParam(cmdSql);
                        rqTplId.AddInputParameter("@scheduleId", SqlDbType.Int, scheduleId);
                        rqTplId.AddInputParameter("@masterfileid", SqlDbType.Int, masterEudoId);

                        int eudoId = dal.ExecuteScalar<int>(rqTplId, out sError);

                        if (!string.IsNullOrEmpty(sError) || eudoId > 0)
                        {
                            Engine.Engine eng = new Engine.Engine(pref, userInfo, globalDatasPath);
                            eng.CallContext = eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.SYNCHRO_EXCH_REALTIME);
                            eng.FileId = eudoId;
                            eng.Tab = eudoTabId;
                            // Indique qu'il est également demandé de supprimer les PP en cascade
                            eng.AddParam("deletePp", deletePp);
                            // Indique une suppression depuis une serie de planning
                            eng.AddParam("openSerie", openSerie);
                            // Retour de la confirmation de suppression
                            //eng.AddParam("validDeletion", validDel);
                            // Informationsur la base
                            eng.AddParam("uid", databaseUid);
                            eng.AddParam("FromExchangeNotification", "1");

                            eng.EngineProcess(new StrategyDelSimple());
                            engResult = eng.Result;

                            if (engResult != null && engResult.Error != null)
                            {
                                throw new Exception("Une erreur est survenu lors de la suppression de la serie." + engResult.Error.ToString());
                            }
                            else
                            {
                                //En cas de suppression de la serie, on vide le filemap exchange (des occurences et du master)
                                SynchroExchangeTools.DeleteFileMapSeries(dal, master.EudonetFileId);
                                /*
                                SynchroExchangeTools.ExchangeDeleteFilemap(dal, pref, 0, 0, 0, masterEudoId, string.Empty, string.Empty, string.Empty, string.Empty, out sError);
                                SynchroExchangeTools.ExchangeDeleteFilemap(dal, pref, 0, 0, masterEudoId, 0, string.Empty, string.Empty, string.Empty, string.Empty, out sError);
                                */

                                returnValue = 1;
                            }
                        }
                        else
                            throw new Exception("Impossible de retrouver la première occurence pour la série à supprimer.");
                    }
                    else
                        throw new Exception("Impossible de récuperer les informations de schedule pour la série à supprimer.");
                }
                else
                    throw new Exception("Une notification Exchange a été reçue pour supprimer une série qui n'a pas été retrouvée sur Eudonet.");

            }
            catch (Exception e)
            {
                throw e;
            }

            return returnValue;
        }

        /// <summary>
        /// CRUD des rendez vous dans Eudonet. Fait appel aux méthodes de la classes SynchroExchangeCRUD pour réaliser les opérations en base
        /// </summary>
        /// <param name="eventExchange"></param>
        /// <param name="authentification"></param>
        /// <param name="pref"></param>
        /// <param name="userInfo"></param>
        /// <param name="dal"></param>
        /// <param name="globalDatasPath"></param>
        /// <param name="appExternalUrl"></param>
        /// <param name="databaseUid"></param>
        /// <param name="action">Action finalement effectuée (CREA, UPD, DEL...)</param>
        /// <param name="masterEudoId"></param>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        private int CreateUpdateDeleteEventOnEudonet(EventContract eventExchange, int notifiedUserId, AuthentificationContract authentification, ePrefLite pref, eUserInfo userInfo,
            eudoDAL dal, string globalDatasPath, string appExternalUrl, string databaseUid, out string action, int masterEudoId = 0, int scheduleId = 0)
        {
            EngineResult result = new EngineResult();
            result.Success = true;
            int nEudoId = 0;
            action = string.Empty;
            string error = string.Empty;


            if (!string.IsNullOrEmpty(error))
            {
                throw new Exception(error);
            }

            try
            {
                if (eventExchange.ExchangeAction == (int)SynchroExchangeMapping.ExchangeNotificationAction.CreateUpdate)
                {

                    result = SynchroExchangeCRUD.CreateUpdateInEudonet(eventExchange, pref,
                        userInfo, dal, globalDatasPath, appExternalUrl, scheduleId, out nEudoId, out action);
                }
                else
                {
                    //Si la suppression concerne l'organisateur on supprime le rdv en entier, si c'est un participant on le retire juste du rdv

                    eFileMapResult filemapResult = SynchroExchangeTools.GetFileMapRecordInfo(dal, eventExchange, userInfo.UserId, notifiedUserId);

                    if (filemapResult.EudoId != 0)
                    {
                        int organizerDescId = SynchroExchangeMapping.GetMappingFieldDescId(SynchroExchangeMapping.ExchangeField.ORGANIZER, pref, true);
                        int attendeesDescId = SynchroExchangeMapping.GetMappingFieldDescId(SynchroExchangeMapping.ExchangeField.ATTENDEES, pref, false);
                        bool bDeleteFromEudonet = false;
                        Dictionary<int, eFieldRecord> planningField = null;

                        try
                        {
                            planningField = eDataFillerGeneric.GetFieldsValue(pref, new HashSet<int>() { organizerDescId, attendeesDescId }, SynchroExchangeMapping.GetMappingTableDescId(pref), filemapResult.EudoId);
                            if (planningField[organizerDescId].Value == userInfo.UserId.ToString())
                                bDeleteFromEudonet = true;
                        }
                        catch
                        {
                            //Si on ne trouve pas les informations du planning c'est que la fiche est surement déja en cours de suppression
                            bDeleteFromEudonet = true;
                        }


                        if (bDeleteFromEudonet)
                        {
                            action = "DEL";


                            result = SynchroExchangeCRUD.DeleteInEudonet(eventExchange, pref,
                                                    userInfo, dal, globalDatasPath, appExternalUrl, databaseUid);
                        }
                        else
                        {
                            action = "UPD";

                            //On met a jour le planning en retirant l'utilisateur des attendees
                            string attendees = planningField[attendeesDescId].Value;
                            List<string> listAttendees = attendees.Split(';').ToList();
                            listAttendees.Remove(userInfo.UserId.ToString());
                            attendees = listAttendees.Count > 0 ? string.Join(";", listAttendees.ToArray()) : string.Empty;

                            Engine.Engine eng = new Engine.Engine(pref, userInfo, globalDatasPath);
                            eng.CallContext = eEngineCallContext.GetCallContext(Common.Enumerations.EngineContext.SYNCHRO_EXCH_REALTIME);
                            eng.ModeDebug = false;
                            eng.Tab = SynchroExchangeMapping.GetMappingTableDescId(pref);
                            eng.FileId = filemapResult.EudoId;
                            eng.AddParam("FromExchangeNotification", "1");
                            eng.AddNewValue(attendeesDescId, attendees);
                            eng.EngineProcess(new StrategyCruSimple());
                            result = eng.Result;
                        }
                    }
                }

                if (result == null)
                {
                    throw new Exception("Pas de retour de l'Engine.");
                }
                else if (!result.Success)
                {
                    if (result.Error == null)
                        throw new Exception("Erreur sur Engine inconnue.");
                    else
                        throw new Exception(result.Error.ToString());
                }
            }
            catch (Exception e)
            {
                error = string.Concat(
                    "Erreur lors du traitement des rendez vous en provenance d'Exchange avec l'ID Exchange ", eventExchange?.ExchangeRecordId, Environment.NewLine, e.Message);

                IDictionary<string, string> additionalDataToLog = new Dictionary<string, string>();
                additionalDataToLog.Add("ExchangeID", eventExchange?.ExchangeRecordId);
                additionalDataToLog.Add("ExchangeICalUID", eventExchange?.ExchangeRecordCalUId);
                additionalDataToLog.Add("ExchangeMasterRecordId", eventExchange?.ExchangeMasterRecordId);
                additionalDataToLog.Add("ExchangeAction", eventExchange?.ExchangeAction == 0 ? "CreateUpdate" : "Delete");

                SynchroExchangeTools.AddLogMessage(
                    SynchroExchangeTools.LoggerType.Error.ToString(),
                    error,
                    e,
                    eventExchange,
                    authentification,
                    "ExchangeToEudonet",
                    "eSynchroHandler_CreateUpdateDeleteEventOnEudonet",
                    "Failed",
                    additionalDataToLog
                 );

                string msg = string.Concat("Synchronisation Office 365 : ", error
                    , Environment.NewLine, "Une erreur est survenue lors de la synchronisation du rendez-vous Exchange dans Eudonet. Voir les logs du module de synchronisation pour plus de détails."
                    , Environment.NewLine, e.ToString());

                eErrorContainer eErrCont = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, msg);
                eFeedback.LaunchFeedback(eErrCont, pref, "");
            }
            return nEudoId;
        }

        #endregion


        #region Méthodes V2 ajoutées pour remplacer les anciennes
        /// <summary>
        /// Traitement de la notification Exchange formatée à partir du flux de RDV simple reçu d'office 365
        /// </summary>
        /// <param name="notification"> Notification en provenance d'exchange</param>
        /// <param name="dal">Couche d'accès aux données</param>
        /// <param name="pref">Preferences de l'utilisateur notifié</param>
        /// <param name="appExternalUrl">Url d'accès à l'eudonet de destination</param>
        /// <param name="rootDatasPath">CHemin racine du dossier Datas de l'eudonet de destination</param>
        private void ProcessNotification(eSynchroEventNotification notification, eudoDAL dal, ePrefLite pref, string appExternalUrl, string rootDatasPath)
        {

            if (notification.RequiredAction == SynchroExchangeMapping.ExchangeNotificationAction.Delete)
            {
                if (notification.IsSerie)
                {
                    // V1 : Pas de gestion de recurrent dans le sens Exchange => Eudonet
                    return;
                    SynchroExchangeCRUD.DeleteRecurringEvent(dal, pref, notification, appExternalUrl, rootDatasPath);
                }
                else
                {
                    SynchroExchangeCRUD.DeleteSingleEvent(dal, pref, notification, appExternalUrl, rootDatasPath);
                }
            }
            else
            {
                if (notification.NotifiedEvent.IsRecurrent)
                {

                    // V1 : Pas de gestion de recurrent dans le sens Exchange => Eudonet
                    return;
                    //Process Recurrent
                    //List des id des rdv pour maj du schedule id

                }
                else
                {
                    //Process Simple
                    //On creer / maj / supprime le rdv simple ou le master d'une serie
                    CreateUpdateSingle(notification, pref, dal, appExternalUrl, rootDatasPath);
                }
            }
        }

        /// <summary>
        /// Traitement de la création/modification d'un RDV simple en provenance d'Exchange
        /// </summary>
        /// <param name="notification">Notification en provenant d'exchange</param>
        /// <param name="dal">Couche d'accès aux données</param>
        /// <param name="pref">Preferences de l'utilisateur notifié</param>
        /// <param name="appExternalUrl">Url d'accès à l'eudonet de destination</param>
        /// <param name="rootDatasPath">CHemin racine du dossier Datas de l'eudonet de destination</param>
        private void CreateUpdateSingle(eSynchroEventNotification notification, ePrefLite pref, eudoDAL dal, string appExternalUrl, string rootDatasPath)
        {
            EngineResult result = new EngineResult();
            result.Success = true;
            //action = string.Empty;
            string error = string.Empty;


            try
            {
                result = SynchroExchangeCRUD.CreateUpdateSingle(dal, pref, notification, appExternalUrl, rootDatasPath);

                if (result == null)
                {
                    throw new Exception("Pas de retour de l'Engine.");
                }
                else if (!result.Success)
                {
                    if (result.Error == null)
                        throw new Exception("Erreur sur Engine inconnue.");
                    else
                        throw new Exception(result.Error.ToString());
                }
            }
            catch (Exception e)
            {
                error = string.Concat(
                    "Erreur lors du traitement des rendez vous en provenance d'Exchange avec l'ID Exchange ", notification.NotifiedEvent?.ExchangeRecordId, Environment.NewLine, e.Message);

                IDictionary<string, string> additionalDataToLog = new Dictionary<string, string>();
                additionalDataToLog.Add("ExchangeID", notification.NotifiedEvent?.ExchangeRecordId);
                additionalDataToLog.Add("ExchangeICalUID", notification.NotifiedEvent?.ExchangeRecordCalUId);
                additionalDataToLog.Add("ExchangeMasterRecordId", notification.NotifiedEvent?.ExchangeMasterRecordId);
                additionalDataToLog.Add("ExchangeAction", notification.NotifiedEvent?.ExchangeAction == 0 ? "CreateUpdate" : "Delete");

                SynchroExchangeTools.AddLogMessage(
                    SynchroExchangeTools.LoggerType.Error.ToString(),
                    error,
                    e,
                    notification.NotifiedEvent,
                    null,
                    "ExchangeToEudonet",
                    "eSynchroHandler_CreateUpdateDeleteEventOnEudonet",
                    "Failed",
                    additionalDataToLog
                 );

                string msg = string.Concat("Synchronisation Office 365 : ", error
                    , Environment.NewLine, "Une erreur est survenue lors de la synchronisation du rendez-vous Exchange dans Eudonet. Voir les logs du module de synchronisation pour plus de détails."
                    , Environment.NewLine, e.ToString());

                eErrorContainer eErrCont = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, msg);
                eFeedback.LaunchFeedback(eErrCont, pref, "");
            }
        }

        /// <summary>
        /// Traitement de la notification Exchange formatée à partir du flux de RDV reccurent reçu d'office 365
        /// </summary>
        /// <param name="notification"> Notification en provenance d'exchange</param>
        /// <param name="dal">Couche d'accès aux données</param>
        /// <param name="pref">Preferences de l'utilisateur notifié</param>
        /// <param name="appExternalUrl">Url d'accès à l'eudonet de destination</param>
        /// <param name="rootDatasPath">CHemin racine du dossier Datas de l'eudonet de destination</param>
        private void CreateUpdateRecurringEvent(eSynchroEventNotification notification, ePrefLite pref, eudoDAL dal, string appExternalUrl, string rootDatasPath)
        {
            EngineResult result = new EngineResult();
            result.Success = true;

            string error = string.Empty;


            try
            {
                result = SynchroExchangeCRUD.CreateUpdateRecurringEvent(dal, pref, notification, appExternalUrl, rootDatasPath);

                if (result == null)
                {
                    throw new Exception("Pas de retour de l'Engine.");
                }
                else if (!result.Success)
                {
                    if (result.Error == null)
                        throw new Exception("Erreur sur Engine inconnue.");
                    else
                        throw new Exception(result.Error.ToString());
                }
            }
            catch (Exception e)
            {
                error = string.Concat(
                    "Erreur lors du traitement des rendez vous en provenance d'Exchange avec l'ID Exchange ", notification.NotifiedEvent?.ExchangeRecordId, Environment.NewLine, e.Message);

                IDictionary<string, string> additionalDataToLog = new Dictionary<string, string>();
                additionalDataToLog.Add("ExchangeID", notification.NotifiedEvent?.ExchangeRecordId);
                additionalDataToLog.Add("ExchangeICalUID", notification.NotifiedEvent?.ExchangeRecordCalUId);
                additionalDataToLog.Add("ExchangeMasterRecordId", notification.NotifiedEvent?.ExchangeMasterRecordId);
                additionalDataToLog.Add("ExchangeAction", notification.NotifiedEvent?.ExchangeAction == 0 ? "CreateUpdate" : "Delete");

                SynchroExchangeTools.AddLogMessage(
                    SynchroExchangeTools.LoggerType.Error.ToString(),
                    error,
                    e,
                    notification.NotifiedEvent,
                    null,
                    "ExchangeToEudonet",
                    "eSynchroHandler_CreateUpdateDeleteEventOnEudonet",
                    "Failed",
                    additionalDataToLog
                 );

                string msg = string.Concat("Synchronisation Office 365 : ", error
                    , Environment.NewLine, "Une erreur est survenue lors de la synchronisation du rendez-vous Exchange dans Eudonet. Voir les logs du module de synchronisation pour plus de détails."
                    , Environment.NewLine, e.ToString());

                eErrorContainer eErrCont = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, msg);
                eFeedback.LaunchFeedback(eErrCont, pref, "");
            }
        }

        /// <summary>
        /// Vérifie la conformité d'un RDV pour être traité correctement ou ignoré directement
        /// Le RDV, pour être valide, doit respecter les condition suivantes : 
        /// Avoir un email organisateur vide, un id CAL vide ainsi qu'un id exchange renseigné (cas d'une notification en suppression)
        /// OU
        /// Avoir un email organisateur renseigné, un id CAL, un id exchange ainsi qu'une changekey tous renseigné
        /// 
        /// Dans le cas contraire, le RDV n'est pas en capacité d'être traité
        /// </summary>
        /// <param name="exchangeEvent">RDV en provenant d'exchange</param>
        /// <returns></returns>
        private bool IsProcessableEvent(EventContract exchangeEvent)
        {

            return
                (
                    String.IsNullOrEmpty(exchangeEvent.OrganizerEmail)
                    && string.IsNullOrEmpty(exchangeEvent.ExchangeRecordCalUId)
                    && !string.IsNullOrEmpty(exchangeEvent.ExchangeRecordId)
                 )
                 ||
                 (
                    !string.IsNullOrEmpty(exchangeEvent.OrganizerEmail)
                    && !string.IsNullOrEmpty(exchangeEvent.UserMail)
                    && !string.IsNullOrEmpty(exchangeEvent.ExchangeRecordCalUId)
                    && !string.IsNullOrEmpty(exchangeEvent.ExchangeRecordId)
                    && !string.IsNullOrEmpty(exchangeEvent.ExchangeChangeKey)
                 );
        }

        #endregion

        /// <summary>
        /// Méthode système - Indique si le composant est réutilisable
        /// </summary>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }



    }
}