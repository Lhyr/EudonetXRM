using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.classes;
using EudoQuery;
using EudoSynchroExchange;
using EudoSynchroExchangeInterface;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager pour l'extension de synchro E2017
    /// </summary>
    public class eAdminExtensionSynchroExchangeManager : eAdminManager
    {
        /// <summary>
        /// Indique si, en cas d'échec d'activation/désactivation, on doit demander à Exchange de rétablir la situation inverse
        /// (réactiver un utilisateur qui n'a pas pu être désactivé, ou désactiver un utilisateur qui n'a pas pu être activé)
        /// </summary>
        private bool _fixUserSubscriptionsOnFailure = true;

        /// <summary>
        /// Execution lors de l'appel au manager
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnGeneric res = new JSONReturnGeneric() { Success = true };

            //Recuperation des utilisateurs a activer et désactiver
            List<string> listUserFailed = new List<string>(); //Liste des user ayant échoué
            string usersToActivate = _requestTools.GetRequestFormKeyS("userListToActivate") ?? string.Empty;
            string userToDeactivate = _requestTools.GetRequestFormKeyS("userListToDeactivate") ?? string.Empty;
            List<string> listUserToActivate = string.IsNullOrEmpty(usersToActivate) ? new List<string>() : new List<string>(usersToActivate.Split(','));
            List<string> listUserToDeactivate = string.IsNullOrEmpty(userToDeactivate) ? new List<string>() : new List<string>(userToDeactivate.Split(','));
            _fixUserSubscriptionsOnFailure = _requestTools.GetRequestFormKeyB("fixUserSubscriptionsOnFailure") ?? true;

            //Récuperation de la configuration synchro (Activation, TenantId, SecretId, ClientId)
            IDictionary<eLibConst.CONFIGADV, String> configs = eLibTools.GetConfigAdvValues(_pref,
                   new HashSet<eLibConst.CONFIGADV> {
                            eLibConst.CONFIGADV.SYNC365_CLIENTID, eLibConst.CONFIGADV.SYNC365_SECRETID, eLibConst.CONFIGADV.SYNC365_TENANTID,
                            eLibConst.CONFIGADV.SYNC365_USERS
            });

            // #67 550 - Liste de messages relatifs à chaque opération pour chaque utilisateur
            Dictionary<string, string> userManagementMessages = new Dictionary<string, string>();

            try
            {
                Dictionary<string, string> hashUserToUserId = new Dictionary<string, string>(); //Contient une correspondance hashUser UserId pour gérer les retours d'erreur

                //Test si la synchronisation est activé
                if (eLibTools.IsSynchroOffice365Activated(_pref))
                {
                    //On recupere les authentifications pour tout les user a activer / desactiver
                    List<AuthentificationContract> listUserToManage = CreateListUserToManage(listUserToActivate, listUserToDeactivate, configs, out hashUserToUserId);

                    if (listUserToManage.Count > 0)
                    {
                        //On appel le WS de Synchro pour activation en passant paramJson, hashdatabase et hashUser
                        Task<List<ResponseContract>> task = SynchroExchangeCaller.WCFEudoSynchroCaller<ISynchroWCF, Task<List<ResponseContract>>>(obj => obj.ManageUsers(listUserToManage));
                        task.Wait();
                        List<ResponseContract> listReponse = task.Result;

                        //On traite la réponse d'activation pour chaque utilisateur
                        foreach (ResponseContract reponse in listReponse)
                        {
                            string currentUserId = hashUserToUserId[reponse.HashUser];

                            //Si OK on insere une ligne dans EUDOTRAIT, si KO on doit décocher l'utilisateur
                            if (reponse.ReturnCode == (int)HttpStatusCode.OK)
                            {
                                ManageSubscription(reponse);
                            }
                            else
                            {
                                //On recupere l'erreur et on ajoute l'utilisateur a la liste des utilisateurs a ne pas activer/desactiver
                                //L'erreur sera consignée dans un seul feedback pour tout le monde
                                if (!String.IsNullOrEmpty(currentUserId))
                                    userManagementMessages[currentUserId] = reponse.ErrorDetail;
                                listUserFailed.Add(reponse.HashUser);
                            }
                        }

                        //On traite les utilisateurs pour lesquels l'action a échouché
                        if (listUserFailed.Count > 0)
                        {
                            userManagementMessages = RollbackUserList(listUserFailed, listUserToManage, userManagementMessages, true, hashUserToUserId, configs);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //On réinitialise la liste des user
                listUserFailed.AddRange(listUserToActivate);
                listUserFailed.AddRange(listUserToDeactivate);
                userManagementMessages = RollbackUserList(listUserFailed, null, userManagementMessages, false, null, configs);
                userManagementMessages["GeneralException"] = ex.Message;
            }

            if (listUserFailed.Count > 0) {
                res.Success = false;
                res.ErrorMsg = eResApp.GetRes(_pref, 8535); //ex.Message; =>Que fait t on de l'exception
                res.ErrorTitle = eResApp.GetRes(_pref, 8536);

                //Envoi de feedback avec les erreurs collectées
                string detailedOperationMsg = String.Empty;
                foreach (KeyValuePair<string, string> kvp in userManagementMessages)
                {
                    string currentMsg = kvp.Value;
                    int currentUserId = -1;
                    int.TryParse(kvp.Key, out currentUserId);
                    if (currentUserId > -1)
                        currentMsg = String.Concat("- Utilisateur ", currentUserId, " : ", currentMsg);
                    else
                    {
                        if (kvp.Key == "UpdateOnDbException")
                            currentMsg = String.Concat("- Une erreur s'est produite lors de la mise à jour de la liste des utilisateurs synchronisés en base : ", currentMsg);
                        else if (kvp.Key == "UpdateOnExchangeException")
                            currentMsg = String.Concat("- Une erreur s'est produite lors de la tentative de restauration des souscriptions d'utilisateur sur Exchange : ", currentMsg);
                        else if (kvp.Key == "GeneralException")
                            currentMsg = String.Concat("- Une erreur générale s'est produite lors du processus de restauration de la liste des utilisateurs synchronisés : ", currentMsg);
                    }

                    detailedOperationMsg = String.Concat(
                        detailedOperationMsg,
                        Environment.NewLine, Environment.NewLine,
                        currentMsg
                    );
                }

                eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   res.ErrorTitle,
                   res.ErrorMsg + Environment.NewLine + detailedOperationMsg,
                   res.ErrorTitle,
                   res.ErrorMsg + Environment.NewLine + detailedOperationMsg);
                eFeedback.LaunchFeedback(eErrCont, _pref, "");
            }

            RenderResult(RequestContentType.SCRIPT, delegate () { return EudoQuery.SerializerTools.JsonSerialize(res); });
        }

        /// <summary>
        /// Permet de gérer la liste des utilisateurs en fonction des erreurs survenues, en rétablissant au mieux la configuration initiale
        /// </summary>
        /// <param name="listUserFailed">Liste des utilisateurs échoués</param>
        /// <param name="listUserToManage">Liste d'authentifications concernant les utilisateurs traités, si récupérée. Dans ce cas de figure, on tentera également de rétablir la situation côté Exchange</param>
        /// <param name="userManagementMessages">Dictionnaire d'éventuels retours d'erreurs déjà collectés pour les utilisateurs, qui sera enrichie et renvoyée par cette fonction</param>
        /// <param name="isHashedValue">Vrai si la liste des utilisateurs échoués contient les hashuser, faux si la liste contient les id user</param>
        /// <param name="hashUserToUserId">Dictionnaire de correspondance hashuser / userid</param>
        /// <param name="configs">Configadv</param>
        /// <returns>Une liste de messages de retour concernant les opérations effectuées, utilisateur par utilisateur</returns>
        private Dictionary<string, string> RollbackUserList(List<string> listUserFailed, List<AuthentificationContract> listUserToManage, Dictionary<string, string> userManagementMessages, bool isHashedValue, Dictionary<string, string> hashUserToUserId, IDictionary<eLibConst.CONFIGADV, String> configs)
        {
            string currentUsers = configs[eLibConst.CONFIGADV.SYNC365_USERS];
            string userId = string.Empty;
            string userHash = string.Empty;
            List<string> listCurrentUsers = string.IsNullOrEmpty(currentUsers) ? new List<string>() : new List<string>(currentUsers.Split(';'));
            List<AuthentificationContract> listUsersToFixOnExchange = new List<AuthentificationContract>();
            Dictionary<string, string> userManagementMessagesForDbUpdate = new Dictionary<string, string>();

            #region Préparation des opérations à effectuer
            foreach (string userFailed in listUserFailed)
            {
                // Conversion hash <> userId
                if (isHashedValue) {
                    userId = hashUserToUserId[userFailed]; //On recupere le userId associé au hashUser
                    userHash = userFailed;
                }
                else {
                    userId = userFailed;
                    userHash = null;
                }

                // Si la liste contient le user en échec, on l'enlève de CONFIGADV, sinon, on le restaure
                bool removeUser = (listCurrentUsers.Contains(userId));
                if (removeUser)
                {
                    listCurrentUsers.Remove(userId);
                    userManagementMessagesForDbUpdate[userId] = "L'utilisateur a été retiré de la synchronisation côté Eudonet";
                }
                else {
                    listCurrentUsers.Add(userId);
                    userManagementMessagesForDbUpdate[userId] = "L'utilisateur a été remis dans la synchronisation côté Eudonet";
                }

                // On prépare l'opération de restauration côté Exchange si le contexte l'autorise (cf. ci-dessous)
                try
                {
                    if (listUserToManage != null && userHash != null)
                    {
                        int userIndexInList = listUserToManage.FindIndex(u => u.HashUser == userHash);
                        if (userIndexInList > -1)
                        {
                            listUsersToFixOnExchange.Add(listUserToManage[userIndexInList]);
                            // Puis on inverse l'action à effectuer
                            listUsersToFixOnExchange[listUsersToFixOnExchange.Count - 1].UserToActivate = !listUsersToFixOnExchange[listUsersToFixOnExchange.Count - 1].UserToActivate;
                        }
                    }
                }
                catch
                {
                    listUsersToFixOnExchange = new List<AuthentificationContract>();
                }
            }
            #endregion

            #region Mise à jour de la liste dans CONFIGADV
            try {
                currentUsers = listCurrentUsers.Count > 0 ? string.Join(";", listCurrentUsers.ToArray()) : string.Empty;
                eLibTools.AddOrUpdateConfigAdv(_pref, eLibConst.CONFIGADV.SYNC365_USERS, currentUsers, eLibConst.CONFIGADV_CATEGORY.SYNCHRO_OFFICE365);

                // Mise à jour des messages renvoyés par la fonction
                foreach (KeyValuePair<string, string> userUpdateMsg in userManagementMessagesForDbUpdate)
                {
                    string userStatusMsg = userManagementMessages[userUpdateMsg.Key];
                    if (!String.IsNullOrEmpty(userStatusMsg))
                        userStatusMsg = String.Concat(userStatusMsg, Environment.NewLine, "==> ");
                    userStatusMsg = String.Concat(userStatusMsg, userUpdateMsg.Value);
                    userManagementMessages[userUpdateMsg.Key] = userStatusMsg;
                }
            }
            catch (Exception ex)
            {
                userManagementMessages["UpdateOnDbException"] = ex.Message;
                userManagementMessagesForDbUpdate.Clear(); // La MAJ en base ayant échoué, ces messages ne sont plus valables
            }
            #endregion

            #region Tentative de rétablir la situation côté Exchange (#67 550)
            // MAB - #67 550 - Cas particulier de fonctionnement en boucle dont il est difficile de se débarrasser :

            // Si on a demandé à désactiver un utilisateur qui n'a pas pu être authentifié sur la base commune (ex : aucun résultat dans USERDATA),
            // le RollbackUserList() ci-dessous va le remettre dans la liste des utilisateurs actifs, alors que sa souscription Exchange sera en
            // réalité inutilisable (puisque qu'il est impossible à authentifier).
            // Or, le remettre dans la liste sous-entend que toute demande de modification le concernant (appel à ManageUsers() plus haut) sera forcément
            // une suppression, et on retombera de nouveau dans ce cas => il restera impossible à désactiver, et considéré toujours actif côté E2017,
            // alors qu'il sera considéré comme inutilisable/inactif côté Exchange. Et il sera impossible de le réactiver.

            // Pour éviter ce cas de figure, et un éventuel cas inverse, on considèrera qu'en plus de faire un rollback côté UI et CONFIGADV, il faudra
            // également tenter de rétablir la situation côté Exchange, en envoyant une demande inverse de celle initialement demandée (et correspondant au
            // rollback effectué côté UI). Ainsi, pour se conformer à ce qui est mémorisé côté E2017 :
            // - si la désactivation d'un utilisateur a échoué, on envoie une demande pour le réactiver côté Exchange
            // - si l'activation d'un utilisateur a échoué, on envoie une demande pour supprimer toute souscription le concernant sur Exchange pour le désactiver
            try {
                if (_fixUserSubscriptionsOnFailure && listUsersToFixOnExchange.Count > 0)
                {
                    //On appel le WS de Synchro pour (dés)activation en passant paramJson, hashdatabase et hashUser
                    Task<List<ResponseContract>> task = SynchroExchangeCaller.WCFEudoSynchroCaller<ISynchroWCF, Task<List<ResponseContract>>>(obj => obj.ManageUsers(listUsersToFixOnExchange));
                    task.Wait();
                    List<ResponseContract> listReponse = task.Result;

                    //On traite la réponse d'activation pour chaque utilisateur
                    foreach (ResponseContract reponse in listReponse)
                    {
                        string operationStatusMsg = "n'a pas pu être";

                        //Si OK on insere une ligne dans EUDOTRAIT, si KO on doit décocher l'utilisateur
                        if (reponse.ReturnCode == (int)HttpStatusCode.OK)
                        {
                            ManageSubscription(reponse);
                            operationStatusMsg = "a été";
                        }

                        // On complète ou alimente la liste de messages concernant l'utilisateur avec l'opération effectuée
                        AuthentificationContract concernedUser = listUsersToFixOnExchange.Find(u => u.HashUser == reponse.HashUser);
                        string currentUserId = hashUserToUserId[reponse.HashUser];
                        if (concernedUser != null && !String.IsNullOrEmpty(currentUserId))
                        {
                            string userOperationMsg = String.Concat("L'utilisateur ", operationStatusMsg, " ", (concernedUser.UserToActivate ? "réactivé" : "désactivé"), " sur Exchange");
                            string userStatusMsg = userManagementMessages[currentUserId];
                            if (!String.IsNullOrEmpty(userStatusMsg))
                                userStatusMsg = String.Concat(userStatusMsg, Environment.NewLine, "==> ");
                            userStatusMsg = String.Concat(userStatusMsg, userOperationMsg);
                            userManagementMessages[currentUserId] = userStatusMsg;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                userManagementMessages["UpdateOnExchangeException"] = ex.Message;
            }
            #endregion

            return userManagementMessages;
        }

        private List<AuthentificationContract> CreateListUserToManage(List<string> listUserToActivate, List<string> listUserToDeactivate, IDictionary<eLibConst.CONFIGADV, String> configs, out Dictionary<string, string> hashUserToUserId)
        {
            List<AuthentificationContract> listAuthentificationSynchro = new List<AuthentificationContract>();
            eudoDAL dal = null;
            hashUserToUserId = new Dictionary<string, string>();
            try
            {
                //Objets d'authentification
                AuthentificationContract authentificationSynchro = null;

                //On récupère les données de connexion (client id, secretid , tenantid, url WCF eudo)
                string clientId = configs[eLibConst.CONFIGADV.SYNC365_CLIENTID];
                string secretId = configs[eLibConst.CONFIGADV.SYNC365_SECRETID];
                string tenantId = configs[eLibConst.CONFIGADV.SYNC365_TENANTID];

                //Information handler
                string urlWcfEudo = _pref.AppExternalUrl.EndsWith("/") ? _pref.AppExternalUrl + "handlers/eSynchroHandler.ashx" : _pref.AppExternalUrl + "/handlers/eSynchroHandler.ashx";

                //Récuperation du hashDatabse
                IDictionary<eLibConst.EUDOLOG_HASH_TO_DB, string> hashToDbValue = eTools.GetEudologHashToDbValues(_pref, new eLibConst.EUDOLOG_HASH_TO_DB[] { eLibConst.EUDOLOG_HASH_TO_DB.HashDatabase });
                string hashDatabase = hashToDbValue[eLibConst.EUDOLOG_HASH_TO_DB.HashDatabase];

                dal = eLibTools.GetEudoDAL(_pref);
                if (!dal.IsOpen)
                    dal.OpenDatabase();

                foreach (string userId in listUserToActivate)
                {
                    authentificationSynchro = GetAuthentificationForUser(userId, hashDatabase, urlWcfEudo, clientId, secretId, tenantId, dal);
                    hashUserToUserId.Add(authentificationSynchro.HashUser, userId);
                    authentificationSynchro.UserToActivate = true;
                    listAuthentificationSynchro.Add(authentificationSynchro);
                }
                foreach (string userId in listUserToDeactivate)
                {
                    authentificationSynchro = GetAuthentificationForUser(userId, hashDatabase, urlWcfEudo, clientId, secretId, tenantId, dal);
                    hashUserToUserId.Add(authentificationSynchro.HashUser, userId);
                    authentificationSynchro.UserToActivate = false;
                    listAuthentificationSynchro.Add(authentificationSynchro);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (dal.IsOpen)
                    dal.CloseDatabase();
            }


            return listAuthentificationSynchro;
        }

        /// <summary>
        /// Renvoi un objet authentification renseigné
        /// </summary>
        /// <param name="userId">Le userId</param>
        /// <param name="hashDatabase">Le hash database</param>
        /// <param name="urlHandlerXrm">L'url de callback xrm</param>
        /// <param name="clientId">Client id de connexion graph</param>
        /// <param name="secretId">Secret id de connexion graph</param>
        /// <param name="tenantId">Tenant id de connexion graph</param>
        /// <returns></returns>
        private AuthentificationContract GetAuthentificationForUser(string userId, string hashDatabase, string urlHandlerXrm, string clientId, string secretId, string tenantId, eudoDAL dal)
        {
            AuthentificationContract authentificationSynchro = new AuthentificationContract();
            try
            {
                //Information user
                string hashUser = string.Empty;
                eUserInfo userInfos = null;
                string paramSynchroJson = string.Empty;

                //Recuperation infos utilisateur
                userInfos = new eUserInfo(Int32.Parse(userId), dal);

                //Instanciation du hashUser 
                InstanciateHashUser(userInfos, out hashUser);

                //Construction du param JSON 
                paramSynchroJson = JsonConvert.SerializeObject(new ParametrageSynchro(clientID: clientId,
                    secretID: secretId,
                    tenantID: tenantId,
                    urlXrm: urlHandlerXrm,
                    userMail: userInfos.UserMail));

                //Création de l'objet d'authentification
                authentificationSynchro.HashDatabase = hashDatabase;
                authentificationSynchro.HashUser = hashUser;
                authentificationSynchro.ParamJSON = paramSynchroJson;
            }
            catch (Exception e)
            {
                throw e;
            }

            return authentificationSynchro;
        }


        /// <summary>
        /// Gère la création ou la suppression de souscription dans EUDOTRAIT
        /// </summary>
        /// <param name="reponse">La réponse de l'appel synchro</param>
        private void ManageSubscription(ResponseContract reponse)
        {
            String sCurrentSQLInstance = ePrefTools.GetAppDefaultInstance();
            eudoDAL eDal = ePrefTools.GetDefaultEudoDal("EUDOTRAIT", sCurrentSQLInstance);
            string sError = string.Empty;
            try
            {
                eDal.OpenDatabase();

                RqParam rqSubscriptions = new RqParam();

                // Si on demande un ajout de souscription (par défaut)
                if (reponse.IsReponseActivation)
                {
                    rqSubscriptions.SetQuery("INSERT INTO SUBSCRIPTIONS VALUES (@HASHDATABASE,@HASHUSER,@EXPIRATION)");
                    rqSubscriptions.AddInputParameter("@EXPIRATION", System.Data.SqlDbType.DateTime, reponse.ExpirationDate);
                }
                // Si on demande une suppression de souscription
                else
                {
                    rqSubscriptions.SetQuery("DELETE FROM SUBSCRIPTIONS WHERE HashDatabase = @HASHDATABASE AND HashUser = @HASHUSER");
                }
                rqSubscriptions.AddInputParameter("@HASHDATABASE", System.Data.SqlDbType.VarChar, reponse.HashDatabase);
                rqSubscriptions.AddInputParameter("@HASHUSER", System.Data.SqlDbType.VarChar, reponse.HashUser);
                eDal.ExecuteNonQuery(rqSubscriptions, out sError);

                if (!string.IsNullOrEmpty(sError))
                {
                    throw new Exception("Erreur lors de " + (reponse.IsReponseActivation ? "l'activation" : "la suppression") + "de la souscription dans EUDOTRAIT");
                }
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }
        }

        /// <summary>
        /// Instancie le hash user si il n'exsite pas et le retourne
        /// </summary>
        /// <param name="userInfos">Les informations utilisateur</param>
        /// <param name="hashUser">Le hash utilisateur a instancié</param>
        private void InstanciateHashUser(eUserInfo userInfos, out string hashUser)
        {
            try
            {
                //Récuperation du hashUser
                eUser.GetFieldValue(_pref, userInfos.UserId, "HashUser", out hashUser);

                //En cas d'activation, on créer son hashUser si il n'existe pas (cas de la premiere activation)
                if (String.IsNullOrEmpty(hashUser))
                {
                    //On calcul le hash a partir de la concatenation du userId Name et email
                    hashUser = eArgon2Hash.GetArgon2Hash(String.Concat(userInfos.UserId, userInfos.UserName, userInfos.UserMail));

                    //On sauvegarde le hashUser en BDD
                    eUser.SetFieldValue(_pref, userInfos.UserId, "HashUser", hashUser, System.Data.SqlDbType.VarChar);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erreur lors de la récuperation du hash utilisateur", e);
            }
        }
    }
}