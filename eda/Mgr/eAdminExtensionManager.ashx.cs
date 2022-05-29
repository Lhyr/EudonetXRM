using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.classes;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminExtensions
    /// </summary>
    public class eAdminExtensionManager : eAdminManager
    {
        /// <summary>
        /// Action
        /// </summary>
        enum Action
        {
            EnableExtension,
            DisableExtension,
            RequestInfo
        }

        Action _action = Action.EnableExtension;

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {
            string strError = String.Empty;

            Boolean bExtensionExists = false;
            eAdminExtension targetExtension = null;
            Boolean bSuccess = false;
            JSONReturnExtensions res = new Mgr.JSONReturnExtensions();
            res.Success = false;
            eAdminResult result = null;
            bool bEnable = false;
            String labelRequestFile = string.Empty;
            String requestStatus = string.Empty;
            String requestType = string.Empty;


            //Initialisation
            String strAction = _requestTools.GetRequestFormKeyS("action") ?? "enable";
            switch (strAction)
            {
                case "enable": _action = Action.EnableExtension; break;
                case "disable": _action = Action.DisableExtension; break;
                case "request": _action = Action.RequestInfo; break;
            }

            bool bListMode = _requestTools.GetRequestFormKeyB("listmode") ?? false;

            /// <summary>
            /// Indique si l'accès aux informations d'extensions peut se faire en effectuant une connexion API vers HotCom, ou si on doit passer par un cache
            /// d'extensions local (ExtensionList.json dans /eudonetXRM/Res/). Cette variable est mise à true si la connexion vers HotCom échoue,
            /// ou si la clé ServerWithoutInternet est valorisée à 1 dans le fichier server.config de /eudonetXRM (cf. ci-dessous)
            /// </summary>
            bool bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";

            try
            {
                eUserOptionsModules.USROPT_MODULE targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
                if (_context.Request.Form["module"] != null)
                {
                    int nTargetModule = 0;
                    Int32.TryParse(_context.Request.Form["module"].ToString(), out nTargetModule);
                    targetModule = (eUserOptionsModules.USROPT_MODULE)nTargetModule;
                }

                int nExtensionInternalId = _requestTools.GetRequestFormKeyI("extensionFileId") ?? 0;
                bExtensionExists = (nExtensionInternalId > 0);

                bool bStoreAccessOk = false;
                eAPIExtensionStoreAccess storeAccess = null;

                #region Récupération des infos de l'extension à partir de la liste référencée sur HotCom
                if (!bNoInternet)
                {
                    storeAccess = new eAPIExtensionStoreAccess(_pref);
                    targetExtension = storeAccess.GetExtensionFile(nExtensionInternalId);

                    if (storeAccess.ApiErrors.Trim().Length == 0)
                        bStoreAccessOk = true;
                    
                }
                #endregion

                #region #64 948 - En cas d'échec : on passe par initExtensionFromJson() pour activation en local sans interaction avec HotCom
                if (!bStoreAccessOk)
                {
                    targetExtension = eAdminExtension.initExtensionFromJson(targetModule, _pref);
                }
                #endregion

                //ALISTER Concerne la demande #83 104
                if (targetExtension != null)
                    targetExtension.SetAdditionalParameters(_requestTools);


                if (_action == Action.EnableExtension || _action == Action.DisableExtension)
                {
                    bEnable = _action == Action.EnableExtension;

                    labelRequestFile = bEnable ? "Demande d'activation" : "Demande désactivation"; // Pas de RES car c'est sur Hotcom
                    requestStatus = bEnable ? "1538" : "1539";
                    requestType = "1594";

                    result = targetExtension.SetEnabled(bEnable);

                    if (!result.Success)
                    {
                        res.ErrorMsg = bEnable ?
                                eResApp.GetRes(_pref, 7855) : // "L'activation de l'extension a échoué"
                                eResApp.GetRes(_pref, 7856); // "La désactivation de l'extension a échoué"

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 6237),
                            res.ErrorMsg,
                            result.UserErrorMessage,
                            result.DebugErrorMessage
                        );
                        LaunchError();


                    }
                    else
                        bSuccess = true;
                }

                if (bSuccess || _action == Action.RequestInfo)
                {
                    if (_action == Action.RequestInfo)
                    {
                        labelRequestFile = "Demande d'information";
                        requestStatus = "1589";
                        requestType = "1593";
                    }


                    #region Création d'une demande d'activation sur HotCom

                    // On supprime ce qu'il y a entre crochets
                    String email = Regex.Replace(_pref.User.UserMail, @" ?\[.*?\]", string.Empty);

                    //Appel API pour incrémente nb install
                    eAPIExtensionStoreParam param = new eAPIExtensionStoreParam(_pref);
                    eAPI api = new eAPI(param.ApiBaseUrl, param.ApiDebug);

                    // Par défaut, les demandes d'activation/désactivation ne sont pas envoyées si on travaille en debug/sur des bases en local
                    if (!bNoInternet && param.ApiSendExtensionStoreRequests)
                    {
                        const bool useNewMethod = true;
                        if (useNewMethod)
                        {
                            #region Nouvelle methode
                            CreateDemandeHotcom(storeAccess, targetExtension, res);
                            #endregion
                        }
                        else
                        {
                            #region Ancienne methode
                            //Flux de création de demande
                            var CreaDemande = new
                            {
                                Fields = new[]
                                {
                            new { DescId = 14801, Value =  labelRequestFile},   // texte de la base
                            new { DescId = 14802, Value = _pref.GetBaseName},   // Nom de la base
                            new { DescId = 14803, Value =  _pref.User.UserId.ToString()}, // Id du user
                            new { DescId = 14807, Value =  _pref.User.UserName   }, // Nom du user
                            new { DescId = 14804, Value =  _pref.User.UserTel   }, // Tel du user
                            new { DescId = 14805, Value =  email   }, // Mail du user
                            new { DescId = 14806, Value =  requestStatus},   // Statut de la demande
                            new { DescId = 14810, Value =  requestType},   // Type de la demande
                            new { DescId = 14808, Value =  eConst.VERSION},   // Version de la base client
                            new { DescId = 4900, Value = nExtensionInternalId.ToString()} // Extension
                                }

                            };

                            api.AuthenticateToken(param.ApiSubscriberLogin, param.ApiSubscriberPassword, param.ApiBaseName,
                                param.ApiUserLogin, param.ApiUserPassword, param.ApiUserLang, param.ApiProductName,

                                // sur auth ok, on créé la demande
                                delegate (ApiResponseAuthenticateToken result2)
                                {
                                    api.Create(14800, CreaDemande,
                                            delegate (ApiResponseCUD result3)
                                            {
                                                // Succes
                                                res.Success = true;
                                            },

                                            delegate (string error)
                                            {
                                                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, error);
                                                LaunchError();
                                                res.ErrorMsg = error;
                                            });
                                },


                                // sur echec
                                delegate (string error)
                                {
                                    ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, error);
                                    LaunchError();
                                    res.ErrorMsg = error;
                                }

                                );
                            #endregion
                        }
                    }
                    else
                        res.Success = true;

                    #region Affectation des infos pour le résultat
                    // Pour éviter de recharger toute la page après activation/désactivation, on récupére uniquement le code HTML de la tuile de l'extension
                    // que l'on vient de mettre à jour, que l'on renvoie dans le JSON à destination du JavaScript appelant
                    // Sauf si l'extension nécessite un refresh complet de la page pour actualiser (par exemple) les paramètres, auquel cas on affecte une
                    // propriété spécifique pour que le JavaScript appelant fasse le nécessaire
                    // Il faut ici passer le FileId et le titre de l'extension en retour afin que le JS cible (updateExtensionPanel) puisse les repasser à
                    // la fonction loadAdminModule() qui en aura besoin pour recharger correctement la page de l'extension

                    // MAB/SPH le 14/04/2017 : on force pour le moment le refresh complet lorsqu'on est en mode Fiche, car il faut de toute façon faire un appel
                    // API pour rafraîchir les infos de l'extension dans l'encart. Or, l'appel à InitFromModule plus haut avec un fileID provoque une
                    // boucle InitFromModule <-> SetInfos, entraînant un chargement partiel des infos de l'extension (bug à élucider)
                    // Comme il faut faire un appel API dans tous les cas, le refresh partiel n'est plus pertinent car il ne fait pas économiser beaucoup.
                    // Ce mode de refresh partiel sera donc certainement supprimé à terme car il n'est plus utilisé en mode Liste non plus
                    // (vu que l'activation se fait uniquement en mode Fiche).
                    // A moins de décider de le conserver pour, par exemple, uniquement changer le statut du bouton d'activation sans mettre à jour les autres
                    // infos de l'encart.
                    res.Action = strAction;
                    res.ExtensionFileId = nExtensionInternalId;

                    if (targetExtension != null)
                    {
                        res.Module = string.Concat(targetModule.ToString());
                        res.ExtensionLabel = (targetExtension != null) ? targetExtension.Infos.Title : string.Empty;
                        res.Result = targetExtension.IsEnabled() ? "1" : "0";
                        res.ShowParametersTab = (targetExtension != null) ? targetExtension.ShowParametersTab : false;
                        res.NeedsFullRefresh = /*targetExtension.NeedsFullRefreshAfterEnable &&*/ !bListMode;
                        if (!res.NeedsFullRefresh)
#if DEBUG
                            if (bListMode)
                                res.Html = GetResultHTML(eAdminExtensionRenderer.GetExtensionPanel(_pref, targetExtension, bListMode));
                            else
                            {
                                eAdminRenderer rdr = eAdminRendererFactory.CreateAdminStoreFileRenderer(_pref, targetExtension);
                                res.Html = GetResultHTML(rdr.PgContainer);
                            }

#else
                            res.Html = GetResultHTML(eAdminExtensionRenderer.GetExtensionPanel(_pref, targetExtension, bListMode));
#endif
                    }

                    #endregion
                    //Si on a pu activer l'extension Synchro E2017, on créer le hashdatabase dans EudoLog si non existant
                    if (res.Success && _action == Action.EnableExtension && targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365)
                    {
                        //On recupere la valeur du hashdatabase
                        IDictionary<eLibConst.EUDOLOG_HASH_TO_DB, string> hashToDbValue = eTools.GetEudologHashToDbValues(_pref, new eLibConst.EUDOLOG_HASH_TO_DB[] { eLibConst.EUDOLOG_HASH_TO_DB.HashDatabase });
                        string hashDatabase = hashToDbValue[eLibConst.EUDOLOG_HASH_TO_DB.HashDatabase];
                        //Si nul on le créer
                        if (String.IsNullOrEmpty(hashDatabase))
                        {
                            //On calcul le hash a partir du numero de base et de l uid 
                            string databaseNumber = _pref.GetBaseName + _pref.DatabaseUid;
                            string newHashDatabase = eArgon2Hash.GetArgon2Hash(databaseNumber);
                            //On insere la valeur dans Databases.HashDatabase de Eudolog
                            eTools.InsertHashDatabase(_pref, newHashDatabase);
                        }
                    }
                    #endregion
                }




                RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });


            }
            catch (eEndResponseException) { }
            catch (Exception e)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, e.Message + " : " + e.StackTrace);
                LaunchError();
            }
        }

        private void CreateDemandeHotcom(eAPIExtensionStoreAccess storeAccess, eAdminExtension targetExtension, JSONReturnExtensions res)
        {
            APIExtensionStoreLinkedBase linkedBase = storeAccess.SearchBaseLiee();

            if (storeAccess.ApiErrors.Length != 0)
            {
                res.Success = false;
                res.ErrorMsg = storeAccess.ApiErrors;
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, storeAccess.ApiErrors);
                LaunchError();
            }
            else
            {
                if (linkedBase.Id == 0)
                {
                    linkedBase = new APIExtensionStoreLinkedBase();

                    linkedBase.Title = _pref.ClientInfos.ClientName;
                    linkedBase.SQLName = _pref.GetBaseName;

                    //int offerId = 1702; //Standard - TODO
                    //int productId = 1687; //Entreprises - TODO
                    int offerId = 0; // TODO
                    int productId = 0; // TODO

                    int newId = storeAccess.CreateBaseLiee(linkedBase.Title, linkedBase.SQLName, offerId, productId,
                        nbPaidSubscriptions: _pref.ClientInfos.NbPaidSubscriptions,
                        nbFreeSubscriptions: _pref.ClientInfos.NbFreeSubscriptions,
                        licenseKey: _pref.ClientInfos.LicenseKey);

                    if (storeAccess.ApiErrors.Length != 0)
                    {
                        res.Success = false;
                        res.ErrorMsg = storeAccess.ApiErrors;
                        ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, storeAccess.ApiErrors);
                        LaunchError();
                    }
                    else
                    {
                        linkedBase.Id = newId;
                    }
                }

                if (linkedBase.Id != 0)
                {
                    APIExtensionStoreLinkedExtension linkedExtension = storeAccess.SearchExtensionLiee(linkedBase.Id, targetExtension.Infos.ExtensionFileId);

                    if (storeAccess.ApiErrors.Length != 0)
                    {
                        res.Success = false;
                        res.ErrorMsg = storeAccess.ApiErrors;
                        ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, storeAccess.ApiErrors);
                        LaunchError();
                    }
                    else
                    {
                        List<APIExtensionStoreCatalogValue> listStatus = storeAccess.GetExtensionLieeStatus();
                        APIExtensionStoreCatalogValue newStatus = new APIExtensionStoreCatalogValue();

                        //string statusRequestTemp = String.Empty;
                        //string statusTemp = String.Empty;
                        string statusToSearch = String.Empty;

                        APIExtensionStoreCatalogValue oldStatus = (linkedExtension.Id > 0) ? listStatus.Find(s => s.Id == linkedExtension.StatutId) : null;

                        if (_action == Action.EnableExtension)
                        {
                            if (!targetExtension.Infos.DelayedActivation || _pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                            {
                                statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.Enabled;
                            }
                            else
                            {
                                if (oldStatus != null && oldStatus.Data == eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.DisableRequest)
                                    statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.Enabled;
                                else
                                    statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequest;
                            }
                        }
                        else if (_action == Action.DisableExtension)
                        {
                            if (_pref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN)
                            {
                                statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.Disabled;
                            }
                            else
                            {
                                if (oldStatus != null
                                    && (
                                    oldStatus.Data == eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequest
                                    || oldStatus.Data == eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequestTransmittedToIC
                                    || oldStatus.Data == eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.CommercialProposalSent
                                    )
                                )
                                    statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.EnableRequestCancellation;
                                else
                                    statusToSearch = eAPIExtensionStoreParam.LinkedExtensionStatutsCodes.DisableRequest;
                            }
                        }

                        newStatus = listStatus.Find(s => !String.IsNullOrEmpty(s.Data) && s.Data == statusToSearch);

                        if (newStatus != null && newStatus.Id != 0)
                        {
                            if (linkedExtension.Id == 0)
                            {
                                string title = String.Concat(targetExtension.Title, ".", linkedBase.Title);
                                int newId = storeAccess.CreateExtensionLiee(title, newStatus.Id, linkedBase.CompanyId, linkedBase.ContractId, targetExtension.Infos.ExtensionFileId, linkedBase.Id);
                            }
                            else
                            {
                                //APIExtensionStoreLinkedExtensionStatus oldStatus = listStatus.Find(s => s.Id == linkedExtension.StatutId);

                                if (storeAccess.IsNeededUpdateExtensionLieeStatus(oldStatus, newStatus))
                                    storeAccess.UpdateExtensionLiee(linkedExtension.Id, newStatus.Id);
                            }

                            if (storeAccess.ApiErrors.Length != 0)
                            {
                                res.Success = false;
                                res.ErrorMsg = storeAccess.ApiErrors;
                                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.INFOS, storeAccess.ApiErrors);
                                LaunchError();
                            }
                            else
                                res.Success = true;
                        }
                    }
                }
            }
        }
    }

    public class JSONReturnExtensions : JSONReturnHTMLContent
    {
        public string Action = String.Empty;

        public string Module = eUserOptionsModules.USROPT_MODULE.UNDEFINED.ToString();

        public string Result = String.Empty;

        public bool ShowParametersTab = true;

        public bool NeedsFullRefresh = false;

        public int ExtensionFileId = 0;

        public string ExtensionLabel = String.Empty;
    }
}