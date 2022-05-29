using System;
using System.Configuration;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.common;
using Com.Eudonet.Internal.wcfs.data.treatment;
using EudoProcessInterfaces;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>eTreatmentManager</className>
    /// <summary>Gestion des traitements de masses</summary>
    /// <authors>GCH/HLA pour le squelette
    /// GCH : UPDATE, DELETE, DUPLICATE
    /// MOU : AFFECT_FILE
    /// SPH : INVIT_CREA, INVIT_CREA_BULK, INVIT_SUP, INVIT_SUP_BULK
    /// HLA : RecurrentMode
    /// </authors>
    /// <date>2013-07-20</date>
    public class eTreatmentManager : eEudoManager
    {
        /// <summary>Delai d'attente maximale au lancement </summary>
        private TimeSpan _maxTimeOut = new TimeSpan(0, 10, 0);

        private TraitementOperation _operation = TraitementOperation.NONE;
        private int _nTargetTab, _nTabFrom, _nCount;
        private bool _bAllFile = false;
        private bool _bMarkedFile = false;
        private bool _bRecurrentMode = false;

        private int _scheduleId = 0;
        private eEventStepXRM _eventStep = null;

        /// <summary>
        /// Constructeur du manager, constructeur ASHX standard.
        /// </summary>
        protected override void ProcessManager()
        {
            _xmlResult = new XmlDocument();

            #region Paramètres transmis

            //Opération à effectuer
            try
            {
                _operation = (TraitementOperation)_requestTools.GetRequestFormKeyI("operation");
            }
            catch (Exception)
            {
                //opération invalide
                throw new Exception(String.Concat("eTreatmentManager - Opération demandée invalide : [", _context.Request.Form["operation"].ToString(), "]"));
            }

            //Table d'origine
            _nTabFrom = _requestTools.GetRequestFormKeyI("tabfrom") ?? 0;

            //Table cible
            _nTargetTab = _requestTools.GetRequestFormKeyI("targettab") ?? 0;

            //Spécifique traitement de masse : traitement sur toutes les fiches
            _bAllFile = _requestTools.GetRequestFormKeyB("allfile") ?? false;

            //Spécifique traitement de masse : traitement sur toutes les fiches
            if (_operation == TraitementOperation.DELETE)
                _bMarkedFile = _requestTools.GetRequestFormKeyB("markedfile") ?? false;

            if (_operation == TraitementOperation.INVIT_CREA_BULK || _operation == TraitementOperation.INVIT_SUP_BULK)
                _bRecurrentMode = _requestTools.GetRequestFormKeyB("recurrentMode") ?? false;

            _nCount = _requestTools.GetRequestFormKeyI("cnt") ?? 0;

            #endregion

            // Cas particulier pour la gestion du Check
            if (_operation == TraitementOperation.CHECK)
            {
                // Check (appel WCF)
                int srvTreatIdCheck = _requestTools.GetRequestFormKeyI("traitementid") ?? 0;
                eTreatmentResponse ets = CheckTreatmentStatus(srvTreatIdCheck);

                // Retour XML
                DoResponseCheck(ets);
            }
            else
            {
                eTreatmentCall etc = new eTreatmentCall();

                #region Chargement des propriétés de eTreatmentCall

                InitializeTreatmentCall(etc);
                switch (_operation)
                {
                    case TraitementOperation.AFFECT_FILE:
                        AffectFile(etc);
                        break;
                    case TraitementOperation.UPDATE:
                        Update(etc);
                        break;
                    case TraitementOperation.DELETE:
                        Delete(etc);
                        break;
                    case TraitementOperation.DUPLICATE:
                        Duplicate(etc);
                        break;
                    case TraitementOperation.INVIT_CREA:
                        CreateInvit(etc);
                        break;
                    case TraitementOperation.INVIT_CREA_BULK:
                        CreateInvitBulk(etc);
                        break;
                    case TraitementOperation.INVIT_SUP:
                        SuppInvit(etc);
                        break;
                    case TraitementOperation.INVIT_SUP_BULK:
                        SuppInvitBulk(etc);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                #endregion

                // Création de la fiche Etape parente
                InsertEventStep(etc);

                bool runSuccess = false;
                try
                {
                    // Appel au WCF
                    //    _eventStep?.SetStatus(EventStepStatus.IN_PROGRESS);



                    Run(etc);

                    runSuccess = true;
                }
                finally
                {
                    // Si nous ne parvenons pas à effectuer la demande auprès du WCF, on supprime la fiche étape
                    if (!runSuccess)
                        _eventStep?.Delete();
                }
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        private void Run(eTreatmentCall etc)
        {
            if (_bRecurrentMode)
            {
                // Appel du WCF
                CreateNewTreatmentRecurrent(etc);

                // Retour XML
                DoResponseRecurrent();
            }
            else
            {
                // Appel du WCF
                int serverTreatmentId = CreateNewTreatmentImmediat(etc);

                // Retour XML
                DoResponseImmediat(serverTreatmentId);
            }
        }

        /// <summary>
        /// Initialise eTreatmentCall avec les paramètres globaux
        /// </summary>
        /// <param name="etc"></param>
        private void InitializeTreatmentCall(eTreatmentCall etc)
        {
            etc.UserId = _pref.User.UserId;
            etc.Lang = _pref.Lang;
            etc.PrefSQL = _pref.GetNewPrefSql();
            etc.SecurityGroup = _pref.GroupMode.GetHashCode();

            etc.Multi = _bAllFile ? 2 : 1;
            etc.TabFrom = _nTabFrom;
            etc.Tab = _nTargetTab;

            etc.PhysicalDatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT, _pref);
            etc.Count = _nCount;

            // GMA 20140121 : récupération du chemin physique pour les DATAS si renseigné dans le web.config afin de pouvoir gérer le multi serveur. 
            // Si le web.config est dépourvu de cette information, on conserve la valeur de sDatasPath affectée plus haut.
            try
            {
                string sDatasPathAppSettings = ConfigurationManager.AppSettings["DatasRootFolder"];
                if (!String.IsNullOrWhiteSpace(sDatasPathAppSettings))
                {
                    sDatasPathAppSettings = String.Concat(sDatasPathAppSettings.TrimEnd('\\'), "\\");
                    etc.PhysicalDatasPath = String.Concat(sDatasPathAppSettings, eLibTools.GetDatasDir(_pref.GetBaseName), "\\");
                }
            }
            catch { }

            // Filtre choisi
            FilterSel nFilterSel = null;
            _pref.Context.Filters.TryGetValue(_nTabFrom, out nFilterSel);
            if (nFilterSel != null && nFilterSel.FilterSelId > 0)
                etc.FilterSelId = nFilterSel.FilterSelId;

            // Fiche marquées
            MarkedFilesSelection mks = null;
            _pref.Context.MarkedFiles.TryGetValue(etc.TabFrom, out mks);
            //Fiche marquées sont seulement affichées si : des fiches sont sélectionnées ET que le filtre n'affiché que les fiche marqués est actif ET que l'on a pas sélectionné Toute les fiches
            //demande #26 554 : bMarkedFile : traitement de masse depuis le menu action, sur une selection de fiches
            //      seulement si l'on passe depuis la suppression des fiches sélectionnées du menu action                
            etc.MarkedFileOnly = (((mks != null) && (mks.NbFiles > 0) && mks.Enabled && etc.Multi == 1) || _bMarkedFile);
        }

        #region Chargement des informations en fonction du type d'operation sur notre objet eTreatmentCall

        /// <summary>
        /// Gère l'affectation globale
        /// </summary>
        /// <param name="etc">Paramètre à fournir au WCF</param>
        private void AffectFile(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.AFFECT_FILE;

            // pour le cas de la mise a jour avec repartition aleatoire des fiches ?
            etc.UseRandomFiles = _requestTools.GetRequestFormKeyB("random") ?? false;
            if (etc.UseRandomFiles)
                etc.NbRandomFiles = _requestTools.GetRequestFormKeyI("nbRandom") ?? 0;

            LoadCommonFileValues(etc);
        }

        /// <summary>
        /// Gère l'action de mise à jour de masse
        /// </summary>
        /// <param name="etc">Paramètre à fournir au WCF</param>
        private void Update(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.UPDATE;

            #region Recuperation des parametres

            // pour le cas de la mise a jour avec repartition aleatoire des fiches ?
            etc.UseRandomFiles = _requestTools.GetRequestFormKeyB("random") ?? false;
            if (etc.UseRandomFiles)
                etc.NbRandomFiles = _requestTools.GetRequestFormKeyI("nbRandom") ?? 0;

            // permet de savoir le type de mise ajour a effectuer

            string sType = _requestTools.GetRequestFormKeyS("typeUpdate");
            TraitementTypeUpd typeUpd = TraitementTypeUpd.NONE;
            if (sType != null)
            {
                switch (sType)
                {
                    case "fromexisting":
                        typeUpd = TraitementTypeUpd.FROM_FIELD;
                        break;
                    case "withnew":
                        typeUpd = TraitementTypeUpd.FROM_VALUE;
                        break;
                    case "withnewdate":
                        typeUpd = TraitementTypeUpd.FROM_DATE;
                        break;
                    case "removevalue":
                        typeUpd = TraitementTypeUpd.REMOVE_VALUE;
                        break;

                    default:
                        throw new Exception(String.Concat("eTreatmentManager - Opération demandée invalide : [", sType, "]"));
                }
            }
            etc.UpdateType = typeUpd;

            // descid du champs de la table principale a mettre a jour
            etc.UpdateFieldDescId = _requestTools.GetRequestFormKeyI("fldDescId") ?? 0;

            // dans le cas de la mise a jour depuis une rubrique : descid de cette rubrique
            etc.UpdateFromFieldDescId = _requestTools.GetRequestFormKeyI("existingFldDescId") ?? 0;

            // nouvelle valeur a remplacer
            etc.UpdateFromValue = _requestTools.GetRequestFormKeyS("updWithnewVal") ?? string.Empty;

            // valeur de catalogue a supprimer
            etc.RemoveValue = _requestTools.GetRequestFormKeyS("removevalue") ?? string.Empty;

            // boolean indiquant si on doit ecraser les valeurs existantes
            etc.OverwriteValue = _requestTools.GetRequestFormKeyB("updWithnewErase") ?? false;

            #endregion

            #region Vérification des données

            if (etc.UpdateType == TraitementTypeUpd.REMOVE_VALUE && string.IsNullOrEmpty(etc.RemoveValue))
            {
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception(String.Concat("eTreatmentManager - Opération demandée invalide - Paramètre manquant"));
            }

            // on vérifie que le champ à mettre à jour est renseigné
            if (etc.UpdateFieldDescId <= 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6449), eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6449)));
            }

            // mettre à jour à partir des valeurs de la rubrique
            if (etc.UpdateType == TraitementTypeUpd.FROM_FIELD && etc.UpdateFromFieldDescId <= 0)
            {
                String sMsg = eResApp.GetRes(_pref, 6450).Replace("<UPD_FROM_FIELD>", eResApp.GetRes(_pref, 302));
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), sMsg, eResApp.GetRes(_pref, 72), sMsg));

            }

            // Date : décaler de ...
            if (etc.UpdateType == TraitementTypeUpd.FROM_DATE)
            {
                Regex regex = new Regex(@"^-?[0-9]+#\|#(Y|M|D|W|H|N)$");
                if (!regex.IsMatch(etc.UpdateFromValue))
                {
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6448), eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6448)));
                }
            }

            #endregion
        }

        /// <summary>
        /// Charge l'objet de paramétrage avec les valeurs commune de fiches
        /// pour les traitment d'affectation (affectfile / création invitation...)
        /// </summary>
        /// <param name="etc"></param>
        private void LoadCommonFileValues(eTreatmentCall etc)
        {
            etc.TabFromFileId = _requestTools.GetRequestFormKeyI("filefromid") ?? 0;

            // Liste des descId qui ne seront pas dupliqué et prendront la valeur saisie par l'utilisateur
            etc.FldNewValue = _requestTools.GetRequestFormKeyS("FldNewValue") ?? string.Empty;

            // tab de la fiche a creer
            etc.FileAffectTabId = _requestTools.GetRequestFormKeyI("FileAffectTabId") ?? 0;
        }

        /// <summary>
        /// Gère l'action de suppression de masse
        /// </summary>
        /// <param name="etc">Paramètre à fournir au WCF</param>
        private void Delete(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.DELETE;

            etc.DeletePp = _requestTools.GetRequestFormKeyB("deletePp") ?? false;
            etc.DeleteAdr = _requestTools.GetRequestFormKeyB("deleteAdr") ?? false;
        }

        /// <summary>
        /// Gère l'action de mise à jour de masse
        /// </summary>
        /// <param name="etc">Paramètre à fournir au WCF</param>
        private void Duplicate(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.DUPLICATE;

            // Liste des descId qui ne seront pas dupliqué et prendront la valeur saisie par l'utilisateur
            etc.FldNewValue = _requestTools.GetRequestFormKeyS("FldNewValue") ?? string.Empty;
            // Liste des signets à dupliquer lors de la duplication
            etc.BkmListId = _requestTools.GetRequestFormKeyS("BkmListId") ?? string.Empty;
        }

        /// <summary>
        /// Création de fiche invitation
        /// Mode fiche par fiche
        /// </summary>
        /// <param name="etc">Objet de paramétrage du traitement</param>
        private void CreateInvit(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.INVIT_CREA;

            //Sélection individuelle de fiches
            eInvitSelection ev = eInvitSelection.GetInvitSelection(_pref, etc.Tab);
            etc.FilterSelId = 0;
            etc.SelectionInvitId = ev.InvitSelectionId;

            //Si pas de selection de fiche, erreur
            if (etc.SelectionInvitId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.Affectfile - Paramètres invalides - Pas de sélection");

            LoadCommonFileValues(etc);

            //Si pas de fiche parente, erreur
            if (etc.TabFromFileId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.CreateInvitBulk - Paramètres invalides - Pas de fiche parente");
        }

        /// <summary>
        /// Création de fiche invitation
        /// Mode Bulk
        /// </summary>
        /// <param name="etc">Objet de paramétrage du traitement</param>
        private void CreateInvitBulk(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.INVIT_CREA_BULK;
            etc.SelectionSelectAll = true;

            //Sélection de toutes les fiches d'un filtre
            etc.FilterSelId = _requestTools.GetRequestFormKeyI("fid") ?? 0;

            etc.SelectionActive = _requestTools.GetRequestFormKeyB("fltact") ?? false;
            etc.SelectionPrincipale = _requestTools.GetRequestFormKeyB("fltprinc") ?? false;
            etc.SelectionNoDoublon = _requestTools.GetRequestFormKeyB("donotdbladr") ?? false;

            //Si pas de filtre de sélection, erreur
            if (etc.FilterSelId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.CreateInvitBulk - Paramètres invalides - Pas de filtre ");

            LoadCommonFileValues(etc);

            //Si pas de fiche parente, erreur
            if (etc.TabFromFileId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.CreateInvitBulk - Paramètres invalides - Pas de fiche parente");

            LoadRecurrentModeParams(etc);
        }

        /// <summary>
        /// Suppression de fiche invitation
        /// Mode fiche par fiche
        /// </summary>
        /// <param name="etc">Objet de paramétrage du traitement</param>
        private void SuppInvit(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.INVIT_SUP;
            etc.SelectionSelectAll = false;

            //Sélection individuelle de fiches
            eInvitSelection ev = eInvitSelection.GetInvitSelection(_pref, etc.Tab);
            etc.FilterSelId = 0;
            etc.SelectionInvitId = ev.InvitSelectionId;

            //Si pas de selection de fiche, erreur
            if (etc.SelectionInvitId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.Affectfile - Paramètres invalides - Pas de selection ");

            LoadCommonFileValues(etc);

            //Si pas de fiche parente, erreur
            if (etc.TabFromFileId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.SuppInvit - Paramètres invalides - Pas de fiche parente");
        }

        /// <summary>
        /// Suppression de fiche invitation
        /// Mode Bulk
        /// </summary>
        /// <param name="etc">Objet de paramétrage du traitement</param>
        private void SuppInvitBulk(eTreatmentCall etc)
        {
            etc.Action = TraitementOperation.INVIT_SUP_BULK;
            etc.SelectionSelectAll = true;

            //Sélection de toutes les fiches d'un filtre
            etc.FilterSelId = _requestTools.GetRequestFormKeyI("fid") ?? 0;

            //Si pas de filtre de sélection, erreur
            if (etc.FilterSelId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.SuppInvitBulk - Paramètres invalides - Pas de filtre ");

            LoadCommonFileValues(etc);

            //Si pas de fiche parente, erreur
            if (etc.TabFromFileId == 0)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new Exception("Ajout Invitation - eTreatmentManagerbyWCH.SuppInvitBulk - Paramètres invalides - Pas de fiche parente");

            LoadRecurrentModeParams(etc);
        }

        /// <summary>
        /// Chargement les informations utiles pour le traitement recurrent
        /// </summary>
        /// <param name="etc">Objet de paramétrage du traitement</param>
        private void LoadRecurrentModeParams(eTreatmentCall etc)
        {
            if (_operation != TraitementOperation.INVIT_CREA_BULK && _operation != TraitementOperation.INVIT_SUP_BULK)
                // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                throw new NotImplementedException($"Le type d'operation {_operation} ne supporte pas le mode recurrent");

            //ScheduleId
            _scheduleId = _requestTools.GetRequestFormKeyI("scheduleId") ?? 0;

            //Filtres de campagne/optin/optout
            //flttypeconsent, fltcampaigntype, fltoptin, fltoptout, fltnoopt
            etc.SelectionFilterCampaignType = _requestTools.GetRequestFormKeyI("fltcampaigntype") ?? 0;
            etc.SelectionFilterTypeConsent = _requestTools.GetRequestFormKeyI("flttypeconsent") ?? 0;
            etc.SelectionFilterOptIn = _requestTools.GetRequestFormKeyI("fltoptin") ?? 0;
            etc.SelectionFilterOptOut = _requestTools.GetRequestFormKeyI("fltoptout") ?? 0;
            etc.SelectionFilterNoOpt = _requestTools.GetRequestFormKeyB("fltnoopt") ?? false;
        }

        #endregion

        /// <summary>
        /// Création de la fiche étape
        /// </summary>
        private void InsertEventStep(eTreatmentCall etc)
        {
            bool eventStepEnabled;
            int eventStepDescId;

            // Uniquement pour les operations d'ajout ou de sup
            if (_operation != TraitementOperation.INVIT_CREA
                && _operation != TraitementOperation.INVIT_CREA_BULK
                && _operation != TraitementOperation.INVIT_SUP
                && _operation != TraitementOperation.INVIT_SUP_BULK)
                return;

            /*
             * Fonction d'envoi récurrent disponible uniquement si depuis un signet ++ (onglet secondaire avec une liaison vers la table Address)
             * lié à un onglet qui a « Mode opération » coché */
            if (!eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.AdminEventStep))
            {
                // Impossible d'être en mode recurrent si l'extension n'est pas disponible
                if (_bRecurrentMode)
                    // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                    throw new Exception("Extension non disponible !");
                return;
            }

            // Recup du descid de la table etape
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                //_nTabFrom = descId de l'onglet parent du signet
                DescAdvDataSet descAdv = new DescAdvDataSet();
                descAdv.LoadAdvParams(dal, new int[] { _nTabFrom }, new DESCADV_PARAMETER[] { DESCADV_PARAMETER.EVENT_STEP_DESCID, DESCADV_PARAMETER.EVENT_STEP_ENABLED });
                eventStepEnabled = descAdv.GetAdvInfoValue(_nTabFrom, DESCADV_PARAMETER.EVENT_STEP_ENABLED, "0") == "1";
                eventStepDescId = eLibTools.GetNum(descAdv.GetAdvInfoValue(_nTabFrom, DESCADV_PARAMETER.EVENT_STEP_DESCID, "0"));
            }
            finally
            {
                dal?.CloseDatabase();
            }

            // Si nous ne sommes pas sur un event mode opération, on ignore la suite de la fonction
            if (!eventStepEnabled || eventStepDescId == 0)
            {
                // Impossible d'être en mode recurrent si il n'y a pas de table étape
                if (_bRecurrentMode)
                    // TODO - Met faut faire un vrai retour utilisateur ! Avec une res de préférence
                    throw new Exception("Table étape non disponible !");
                return;
            }

            // Informations de la fiche étape
            _eventStep = new eEventStepXRM(_pref, eventStepDescId);
            _eventStep.Type = EventStepType.SOURCE_ADD;
            _eventStep.ExecutionMode = _bRecurrentMode ? EventStepExecutionMode.RECURRENT : EventStepExecutionMode.INSTANT;

            if (!_bRecurrentMode)
                _eventStep.Date = DateTime.Now;

            _eventStep.RecipientsTabDescId = etc.Tab;
            _eventStep.ParentTabFileId = etc.TabFromFileId;
            //Planification
            _eventStep.ScheduleId = _scheduleId;
            //Filtre
            _eventStep.FilterId = etc.FilterSelId;

            switch (_operation)
            {
                case TraitementOperation.INVIT_CREA:
                case TraitementOperation.INVIT_CREA_BULK:
                    _eventStep.Label = eResApp.GetRes(_pref, 1971);
                    break;
                case TraitementOperation.INVIT_SUP:
                case TraitementOperation.INVIT_SUP_BULK:
                    _eventStep.Label = eResApp.GetRes(_pref, 2609);
                    break;
                default:
                    _eventStep.Label = eResApp.GetRes(_pref, 2608);
                    break;
            }




            // Ajout de la fiche
            _eventStep.InsertOrUpdateEventStep();


            if (_eventStep != null)
            {
                etc.MarketStepInfos = new eMarketStepInfos()
                {
                    Tab = _eventStep.Tab,
                    FileId = _eventStep.FileId

                };
            }
        }

        /// <summary>
        /// Appel du WCF en mode execution immédiate
        /// </summary>
        /// <param name="etc"></param>
        /// <returns></returns>
        private int CreateNewTreatmentImmediat(eTreatmentCall etc)
        {
            string error = String.Empty;
            eTreatmentRun treatRunInfos = null;

            #region code d'accès à un webservice de façon dynamique
            try
            {
                treatRunInfos = eWCFTools.WCFEudoProcessCaller<IEudoTreatmentWCF, eTreatmentRun>(
                    ConfigurationManager.AppSettings.Get("EudoTreatmentURL"), obj => obj.RunTreatment(etc, out error));
            }
            catch (EndpointNotFoundException ExWS)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), string.Empty, eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 6614), ", ", eResApp.GetRes(_pref, 6656), " : ", Environment.NewLine, ExWS.ToString())));
            }
            catch (Exception ex)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), String.Empty, eResApp.GetRes(_pref, 72), ex.ToString()));
            }
            #endregion

            if (error.Length > 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), String.Empty, eResApp.GetRes(_pref, 72), error));
            }

            //Si cette donnee est renseigné c'est qu un traitement etait déja en cours d execution
            if (treatRunInfos?.AnTraitInProgress ?? false)
            {
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 8197), eResApp.GetRes(_pref, 8198), eResApp.GetRes(_pref, 8197)));
            }

            if (treatRunInfos == null || treatRunInfos.ServerTreatmentId == 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), String.Empty, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6615)));
            }

            etc = null;
            return treatRunInfos?.ServerTreatmentId ?? 0;
        }

        /// <summary>
        /// Appel du WCF en mode execution recurrente
        /// </summary>
        /// <param name="etc"></param>
        private void CreateNewTreatmentRecurrent(eTreatmentCall etc)
        {
            if (this._eventStep == null)
                throw new Exception("EventStep is empty !");

            try
            {
                // Les params
                _eventStep.TreatmentCall = etc;
                // On effectue la demande au WCF d'ajout de la récurrence
                _eventStep.AddOrUpdateSchedule();
            }
            catch (Exception)
            {
                // Si nous ne parvenons pas à effectuer la demande auprès du WCF, on supprime la fiche étape
                _eventStep.Delete();
            }
        }

        /// <summary>
        /// Méthode permettant de récupérer le status d'un traitement auprès d'eudoProcess
        /// </summary>
        /// <param name="serverTreatmentId">Id du rapport dans SERVERTREATMENTS dont on veut connaitre le statut</param>
        /// <returns></returns>
        public eTreatmentResponse CheckTreatmentStatus(int serverTreatmentId)
        {
            eTreatmentResponse ets = null;

            #region code d'accès à un webservice de façon dynamique
            try
            {
                ets = eWCFTools.WCFEudoProcessCaller<IEudoTreatmentWCF, eTreatmentResponse>(
                    ConfigurationManager.AppSettings.Get("EudoTreatmentURL"), obj => obj.GetTreatmentResponse(_pref.GetBaseName, _pref.User.UserId, serverTreatmentId));
            }
            catch (EndpointNotFoundException ExWS)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6614), string.Empty, eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 6614), ", ", eResApp.GetRes(_pref, 6656), " : ", Environment.NewLine, ExWS.ToString())));
            }
            catch (Exception ex)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), String.Empty, eResApp.GetRes(_pref, 72), ex.ToString()));
            }
            #endregion

            if (ets != null && ets.Erreur != null && ets.Erreur.Length > 0)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), String.Empty, eResApp.GetRes(_pref, 72), ets.Erreur), _pref);
            }

            //Pour les suppression le compteur de liste doit-être réinitialisé à la fin du traitement
            // HLA - Fiche toujours présente dans les dernières fiches consultées (survol onglet) après suppression - #55168
            if (ets.Action == TraitementOperation.DELETE && (ets.Status == eProcessStatus.SUCCESS || ets.Status == eProcessStatus.ERROR_USER))
                _pref.Context.Paging.resetInfo();   //RAZ du compteur

            return ets;
        }

        #region Retour XML

        private void DoResponseCheck(eTreatmentResponse ets)
        {
            //Créer le XML de retour!
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(baseResultNode);

            // Success ou pas!
            XmlNode successNode = _xmlResult.CreateElement("success");
            baseResultNode.AppendChild(successNode);
            successNode.InnerText = ets.Status == eProcessStatus.SUCCESS ? "1" : "0";
            successNode = null;

            // Type de traitement
            XmlNode actionNode = _xmlResult.CreateElement("action");
            baseResultNode.AppendChild(actionNode);
            actionNode.InnerText = ets.Action.GetHashCode().ToString();
            actionNode = null;

            // Msg Erreur
            XmlNode errorMsgNode = _xmlResult.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);
            errorMsgNode.InnerText = ets.Erreur;
            errorMsgNode = null;

            // Statut
            XmlNode rapportStatut = _xmlResult.CreateElement("Statut");
            baseResultNode.AppendChild(rapportStatut);
            rapportStatut.InnerText = ets.Status.ToString();
            rapportStatut = null;

            // Percent
            XmlNode rapportPercentProgress = _xmlResult.CreateElement("PercentProgress");
            baseResultNode.AppendChild(rapportPercentProgress);
            rapportPercentProgress.InnerText = ets.PercentProgress.ToString();
            rapportPercentProgress = null;

            // DisplayMsg -------------------------------------------------------------------------
            if (ets.DisplayMsg != null)
            {
                XmlNode rapportDisplayMsg = _xmlResult.CreateElement("DisplayMsg");
                baseResultNode.AppendChild(rapportDisplayMsg);

                // DisplayMsg Title
                XmlNode rapportDisplayMsgTitle = _xmlResult.CreateElement("Title");
                rapportDisplayMsg.AppendChild(rapportDisplayMsgTitle);
                rapportDisplayMsgTitle.InnerText = ets.DisplayMsg.Title;
                rapportDisplayMsgTitle = null;
                // DisplayMsg Msg
                XmlNode rapportDisplayMsgMsg = _xmlResult.CreateElement("Msg");
                rapportDisplayMsg.AppendChild(rapportDisplayMsgMsg);
                rapportDisplayMsgMsg.InnerText = ets.DisplayMsg.Msg;
                rapportDisplayMsgMsg = null;
                // DisplayMsg Detail
                XmlNode rapportDisplayMsgDetail = _xmlResult.CreateElement("Detail");
                rapportDisplayMsg.AppendChild(rapportDisplayMsgDetail);
                rapportDisplayMsgDetail.InnerText = ets.DisplayMsg.Detail;
                rapportDisplayMsgDetail = null;
                // DisplayMsg TypeCriticity
                XmlNode rapportDisplayMsgTypeCriticity = _xmlResult.CreateElement("TypeCriticity");
                rapportDisplayMsg.AppendChild(rapportDisplayMsgTypeCriticity);
                rapportDisplayMsgTypeCriticity.InnerText = ets.DisplayMsg.TypeCriticity.GetHashCode().ToString();
                rapportDisplayMsgTypeCriticity = null;
                rapportDisplayMsg = null;
            }
        }

        /// <summary>
        /// Permet de centraliser la generation du XML de retour
        /// Sauf en cas d exception
        /// </summary>
        /// <param name="serverTreatmentId"> ID du job</param>
        private void DoResponseImmediat(int serverTreatmentId)
        {
            XmlNode contentNode = DoResponse();

            // Id du treatment demandé
            XmlNode reportIdNode = _xmlResult.CreateElement("servertreatmentid");
            reportIdNode.InnerText = serverTreatmentId.ToString();
            contentNode.AppendChild(reportIdNode);
        }

        /// <summary>
        /// Permet de centraliser la generation du XML de retour
        /// Sauf en cas d exception
        /// </summary>
        private void DoResponseRecurrent()
        {
            XmlNode contentNode = DoResponse();

            // Id du treatment demandé
            XmlNode recModeNode = _xmlResult.CreateElement("recurrentmode");
            contentNode.AppendChild(recModeNode);
            recModeNode.InnerText = "1";
        }

        /// <summary>
        /// Permet de centraliser la generation du XML de retour
        /// Sauf en cas d exception
        /// </summary>
        /// <returns>content node</returns>
        private XmlNode DoResponse()
        {
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = _xmlResult.CreateElement("success");
            baseResultNode.AppendChild(successNode);
            successNode.InnerText = "1";

            // Num erreur
            XmlNode errorCodeNode = _xmlResult.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = _xmlResult.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);

            // Content
            XmlNode contentNode = _xmlResult.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            return contentNode;
        }

        #endregion
    }
}