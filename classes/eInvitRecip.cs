using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System.Collections;
using System.Data;
using System.Text;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.invitRecip;
using System.Configuration;
using Com.Eudonet.Internal.tools.WCF;
using EudoProcessInterfaces;
using System.ServiceModel;
using Com.Eudonet.Internal.wcfs.data.common;
//SHA - TODOS - A SUP
namespace Com.Eudonet.Xrm
{
    public class eInvitRecip
    {
        //Paramètres utilisateur
        protected ePref _ePref { get; set; }
        //Id de l'ajout des destinataires à partir d'un filtre
        protected Int32 _iInvitRecipId = 0;
        //table from (pp, pm , ++ , invitation)
        protected Int32 _iTabFrom = 0;
        //Table cible
        protected Int32 _iTab = 0;
        //fiche parente
        protected Int32 _iParentEvtId = 0; //_iParentTabFileId
        //Id fiche
        protected Int32 _iFileId = 0; 

        protected Dictionary<string, string> _params;
        private InvitRecipientMode _invitRecipMode;
        /// <summary>DescId Table Étape</summary>
        private int _eventStepDescId = 0;
        /// <summary>Id fiche Étape</summary>
        private int _eventStepFileId = 0;
        private eEventStepXRM _eventStep = null;
        //Parametres de base de connexion 
        protected eudoDAL _dal;
        protected bool _bIsLocalDal = true;
        protected bool _bIsLocalDalOpened = false;
        //Gestion d'erreurs
        protected string _strErrorMessage = string.Empty;

        //public enum InvitRecipAction
        //{
        //    /// <summary>pas d'action (ne doit pas se produire)</summary>
        //    NONE = 0,
        //    /// <summary>Enregistrement de l'ajout de destinataires</summary>
        //    INSERT = 1,
        //    /// <summary>Mise à jour de l'ajout de destinataires</summary>
        //    UPDATE = 2,
        //    /// <summary>Annulation de l'ajout de destinataires</summary>
        //    CANCEL = 3
        //}

        #region accesseurs
        public int IdInvitRecip
        {
            get { return this._iInvitRecipId; }
        }

        /// <summary>
        /// DescId de la table parente (de départ)
        /// </summary>
        public Int32 TabFrom
        {
            get { return this._iTabFrom; }
            set { this._iTabFrom = value; }
        }

        /// <summary>
        /// DescId de la table en cours
        /// </summary>
        public Int32 Tab
        {
            get { return this._iTab; }
            set { this._iTab = value; }
        }

        /// <summary>Fiche parente</summary>
        public int ParentEvtId
        {
            set { this._iParentEvtId = value; }
            get { return this._iParentEvtId; }

        }

        /// <summary>Mode d'invitation de destinataires à partir d'un filtre</summary>
        public InvitRecipientMode InvitRecipientMode
        {
            get { return this._invitRecipMode; }
            set { this._invitRecipMode = value; }
        }

        #endregion

        #region Constructeurs
        /// <summary>
        /// Constructeur de l'object ajout destinataires
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        public eInvitRecip(ePref pref)
        {
            this._ePref = pref;
            if (this._iInvitRecipId == 0)
                InitParams();
        }

        /// <summary>
        /// Constructeur de l'object ajout destinataires
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="invitRecipId">Id de l'ajout de destinataires depuis un filtre</param>
        public eInvitRecip(ePref pref, int invitRecipId)
            : this(pref)
        {
            this._iInvitRecipId = invitRecipId;
            if (this._iInvitRecipId > 0)
            {
                //LoadFromDB();
            }
        }

        /// <summary>
        /// Constructeur de l'object ajout destinataires
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="invitRecipId">Id de l'ajout de destinataires depuis un filtre</param>
        /// <param name="invitRecipMode">Mode d'invitation de destinataires à partir d'un filtre</param>
        /// <param name="dal">EudoDal Externe</param>
        public eInvitRecip(ePref pref, int invitRecipId, int nTab, eudoDAL dal, InvitRecipientMode invitRecipMode = InvitRecipientMode.INVIT_RECIPIENT_UNDEFINED)
            : this(pref)
        {
            this._invitRecipMode = invitRecipMode;
            this._iInvitRecipId = invitRecipId;
            //this._iTabFrom = nTabFrom;
            this._iTab = nTab;
            if (dal != null)
            {
                this._bIsLocalDal = false;
                this._dal = dal;
            }

            if (this._iInvitRecipId > 0)
            {
                //LoadFromDB();
            }
        }
        #endregion

        /// <summary>
        /// Initialise les paramètres js
        /// </summary>
        private void InitParams()
        {
            this._params = new Dictionary<string, string>();
            this._params.Add("delete", "0");          // (++ ou xx) [ 0 = Ajout de destinataires] ou [1 = Suppression de destinataires]
            this._params.Add("invitRecipMode", "0");         //['0' : ajout immédiat par défaut] ou ['1' : ajout récurrent]
            this._params.Add("immediateAdd", "1");         //['1' : ajout immédiat par défaut] ou ['0' : ajout récurrent]
            this._params.Add("recurrentAdd", "0");         //['1' : ajout récurrent] ou ['0' : ajout immédiat par défaut]
            this._params.Add("fid", "0");           //[Id filtre destinataires]
            this._params.Add("scheduleId", "0");           //[Id de la planification (table Schedule)]
            this._params.Add("scheduleUpdated", "0");           //[pour savoir si l'utilisateur a modifié la planification]
            this._params.Add("libelle", string.Concat(eResApp.GetRes(this._ePref, 6407), FormatDisplayDate(DateTime.Now, true)));// libellé de l'ajout destinataire (à vérifier)
            
            this._params.Add("fltact", "0");   // [0 : ne pas utiliser adresses actives] ou [1 : utiliser adresses active] 
            this._params.Add("fltprinc", "0");      // [0 : ne pas utiliser adresses principales] ou [1 : utiliser adresses principales] 
            this._params.Add("donotdbladr", "0");   //[0 : Ajouter les fiches déjà associées] ou [1 : Ne pas ajouter les fiches déjà associées]
            this._params.Add("typdbl", "1"); //[0 : griser/omettre déjà associées à la fiche de l'événement] ou [1 : ne pas griser/omettre déjà associées à la fiche de l'événement]
            this._params.Add("selecttypdbl", ""); //[Type de fiches à griser/omettre déjà associées à la fiche de l'événement]
            this._params.Add("mediatype", "");      // [id du type de média]
            this._params.Add("fltcampaigntype", ""); // [Type de campagne]
            this._params.Add("flttypeconsent", "");  // [id du consentement]
            this._params.Add("fltoptin", "0");    //[0 : ne pas retirer les abonnés] ou [1 : retire les abonnés]
            this._params.Add("fltoptout", "0");    //[0 : ne pas retirer les désabonnés] ou [1 : retire les désabonnés]
            this._params.Add("fltnoopt", "0");   //[1 : aucun consentement enregistré]
            //this._params.Add("removeDoubles", "0"); // Par défaut on ne dédoublonne pas

            this._params.Add("ownerUserId", _ePref.UserId.ToString());
            this._params.Add("eventStepDescId", "0");   // [DescId de la table EventStep]
            this._params.Add("eventStepFileId", "0");   // [Id de la fiche EventStep]
        }

        private string FormatDisplayDate(DateTime? dateTime, bool forceHourMinSec = false)
        {
            if (dateTime.HasValue)
            {
                if (forceHourMinSec)
                    return eDate.ConvertBddToDisplay(_ePref.CultureInfo, dateTime.Value, forceHourMinSec);

                return eDate.ConvertBddToDisplay(_ePref.CultureInfo, dateTime.Value.ToShortDateString(), forceHourMinSec);
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Recupere la valeur connaissant la clé
        /// </summary>
        /// <param name="ParamKey">La clé du parametre</param>
        /// <returns>La valeur correspondant a la clé</returns>
        public string GetParamValue(string ParamKey)
        {
            if (this._params.Count == 0)
                return "";

            string value;
            this._params.TryGetValue(ParamKey, out value);

            return value;
        }

        /// <summary>
        /// Ajoute une clé/valeur, mise a jour si existe, 
        /// </summary>
        /// <param name="ParamKey">Clé</param>
        /// <param name="Value">La valeur</param>
        public void SetParamValue(string ParamKey, string Value)
        {
            if(!this._params.ContainsKey(ParamKey))
                this._params.Add(ParamKey, Value);
            else
                this._params[ParamKey] = Value;
        }

        /// <summary>
        /// return l objet param 
        /// </summary>
        public Dictionary<string, string> InvitRecipParams
        {
            get { return _params; }
        }

        /// <summary>
        /// Initialiser le DescId de la fiche Étape pour l'ajout de destinataires
        /// </summary>
        private void InitEventStepDescIdFromParams()
        {
            //Lors du 1er enregistrement ce paramètre est à 0, donc on l'initialise depuis les paramètres
            if (this._eventStepDescId == 0)
                int.TryParse(this._params["eventStepDescId"], out this._eventStepDescId);
        }

        /// <summary>
        /// Crée ou met à jour la fiche Étape pour l'ajout de destinataires
        /// </summary>
        private void InsertOrUpdateEventStepInvitRecip()
        {
            InitEventStepDescIdFromParams();

            if (this._eventStepDescId != 0)
            {
                if (this._eventStep == null)
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId);
                
                //this._eventStep.ParentStepId = 0;
                this._eventStep.Type = EventStepType.SOURCE_ADD;

                if (this._params["immediateAdd"] == "1" && this._params["recurrentAdd"] == "0")
                {
                    this._eventStep.ExecutionMode = EventStepExecutionMode.INSTANT;
                }
                else if (this._params["immediateAdd"] == "0" && this._params["recurrentAdd"] == "1")
                {
                    this._eventStep.ExecutionMode = EventStepExecutionMode.RECURRENT;
                }
                else
                    this._eventStep.ExecutionMode = EventStepExecutionMode.UNDEFINED;

                this._eventStep.RecipientsTabDescId = this._iTabFrom;
                this._eventStep.ParentTabFileId = this._iParentEvtId;

                int scheduleId = 0;
                int.TryParse(this._params["scheduleId"], out scheduleId);
                this._eventStep.ScheduleId = scheduleId;

                int recipientsFilterId = 0;
                int.TryParse(this._params["fid"], out recipientsFilterId);
                this._eventStep.RecipientsFilterId = recipientsFilterId;

                if (!String.IsNullOrEmpty(this._params["libelle"]))
                    this._eventStep.Label = this._params["libelle"];

                try
                {
                    this._eventStep.InsertOrUpdateEventStep();
                }
                catch (Exception ex)
                {
                    throw new eInvitRecipException(eErrorCodeInvitRecip.ERROR_ADD_EVENTSTEP, string.Concat("eInvitRecip::InsertOrUpdateEventStepInvitRecip() :: ", ex.Message));
                }

                if (this._eventStep.FileId != 0)
                {
                    this._eventStepFileId = this._eventStep.FileId;
                    this._params["eventStepFileId"] = this._eventStepFileId.ToString();
                    //UpdateCampaignEventStepLink();
                }
            }
        }

        /// <summary>
        /// Supprime la ligne dans campaign si elle est inserée
        /// </summary>
        private void RollBack()
        {
            if (this._eventStepFileId > 0)
                DeleteEventStep();
        }

        /// <summary>
        /// Supprime la fiche Étape
        /// </summary>
        private void DeleteEventStep()
        {
            InitEventStepDescIdFromParams();
            if (this._eventStepDescId != 0)
            {
                if (this._eventStep == null)
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
                this._eventStep.Delete();
            }
        }

        /// <summary>
        /// Ouvre le dal
        /// </summary>
        private void OpenDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                _dal = eLibTools.GetEudoDAL(_ePref);
                this._dal.OpenDatabase();
                this._bIsLocalDalOpened = true;
            }
        }

        /// <summary>
        /// Ferme le dal
        /// </summary>
        private void CloseDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                this._dal.CloseDatabase();
                this._bIsLocalDalOpened = false;
            }
        }

        /// <summary>
        /// Exécute l'action demandée
        /// </summary>
        /// <param name="action">Action à executer : insert, update,...</param>
        public void Run(TraitementOperation operation)
        {
            switch (operation)
            {
                case TraitementOperation.INVIT_CREA_BULK:
                    RunInvitRecip();
                    break;

                //SHA TODO : ajouter d'autres opérations (suppression bulk, ....)

                default:
                    break;
            }
        }

        /// <summary>
        /// Lance l'ajout de destinataires 
        /// </summary>
        private void RunInvitRecip()
        {
            //si c'est un ajout de destinataires récurrent, on ajoute la plannification au Scheduler d'EudoProcess
            if (this._invitRecipMode == InvitRecipientMode.INVIT_RECIPIENT_RECURRENT)
                ProcessSchedulingInvitRecip();
            else if (this._invitRecipMode == InvitRecipientMode.INVIT_RECIPIENT_IMMEDIATE)
                //Ajout immédiat           
                ProcessAddingRecip();
        }

        /// <summary>
        /// Création de la plannification dans le scheduler d'EudoProcess
        /// </summary>
        private void ProcessSchedulingInvitRecip()
        {
            if (this._eventStepDescId != 0 && this._eventStepFileId != 0)
            {
                if (this._eventStep == null)
                {
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
                    string error;
                    this._eventStep.LoadEventStepFile(out error);
                    if (error.Length > 0)
                        throw new eInvitRecipException(eErrorCodeInvitRecip.ERROR_LOAD_EVENTSTEP, error);
                }

                try
                {
                    this._eventStep.AddOrUpdateScheduleInvitRecip();
                }
                catch (Exception ex)
                {
                    throw new eInvitRecipException(eErrorCodeInvitRecip.ERROR_UPDATE_EVENTSTEP, ex.Message);
                }
            }
        }

        /// <summary>
        /// Process d'ajout immédiat de destinataires
        /// </summary>
        private void ProcessAddingRecip()
        {
            eInvitRecipCall invitRecipCall = new eInvitRecipCall();
            invitRecipCall.UserId = _ePref.User.UserId;
            invitRecipCall.Lang = _ePref.Lang;
            invitRecipCall.PrefSQL = _ePref.GetNewPrefSql(); // à vérifier
            invitRecipCall.TabFromFileId = _iInvitRecipId; //à vérifier
            invitRecipCall.AppExternalUrl = this._ePref.AppExternalUrl;
            invitRecipCall.SecurityGroup = this._ePref.GroupMode.GetHashCode();
            invitRecipCall.UID = this._ePref.DatabaseUid;
            //GCH 20140211 : on ne doit passer au WCF que le chemin des datas commun à chaques bases (ex:d:\datas et surtout pas d:\datas\DEVTEST_C2...)
            invitRecipCall.PhysicalDatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT);

            // GMA 20140121 : récupération du chemin physique pour les DATAS si renseigné dans le web.config afin de pouvoir gérer le multi serveur. 
            // Si le web.config est dépourvu de cette information, on conserve la valeur de sDatasPath affectée plus haut.
            try
            {
                string sDatasPathAppSettings = ConfigurationManager.AppSettings["DatasRootFolder"];
                if (!string.IsNullOrWhiteSpace(sDatasPathAppSettings))
                {
                    invitRecipCall.PhysicalDatasPath = string.Concat(sDatasPathAppSettings);
                }
            }
            catch { }

            //Appel de WCF avec l'objet contenant toutes les infos de la campagne 
            CallWCFInvitRecip(invitRecipCall);

            if (_strErrorMessage.Length > 0)
                throw new eInvitRecipException(eErrorCodeInvitRecip.ERROR_SENDING_WCF, _strErrorMessage);
        }

        /// <summary>
        /// Envoi les infos de l'ajout de destinataires au WCF 
        /// </summary>
        /// <param name="invitRecipCall">L'objet à envoyer</param>
        private void CallWCFInvitRecip(eInvitRecipCall invitRecipCall)
        {
            try
            {
                eWCFTools.WCFEudoProcessCaller<IEudoInvitRecipWCF, int>(
                    ConfigurationManager.AppSettings.Get("EudoInvitRecipURL"), obj => obj.RunInvitRecip(invitRecipCall, out _strErrorMessage));
            }
            catch (EndpointNotFoundException ExWS)
            {
                _strErrorMessage = string.Concat(eResApp.GetRes(_ePref, 2195), ", ", eResApp.GetRes(_ePref, 6565), " : ", Environment.NewLine, ExWS.ToString());
            }
            catch (Exception ex)
            {
                _strErrorMessage = ex.ToString();
            }
        }

        /// <summary>
        /// Charge l'ensemble des informations relative à l'ajout des destinataires à partir d'un filtre.
        /// </summary>
        /// <returns>true si le traitement s'est bien passé, sinon false</returns>
        //public bool LoadFromDB()
        //{
        //    eInvitRecipients _eInvitRecipients = new eInvitRecipients(_iInvitRecipId);
        //    string sError = string.Empty;

        //    eudoDAL dal = null;
        //    try
        //    {
        //        dal = eLibTools.GetEudoDAL(_ePref);
        //        dal.OpenDatabase();
        //        // Chargement de l'ajout de destinataires
        //        _eInvitRecipients.Load(dal, _ePref, out sError);
        //        if (sError.Length > 0)
        //        {
        //            _strErrorMessage = string.Concat("Erreur de chargement de l'ajout de destinataire ID=", _iInvitRecipId);
        //            return false;
        //        }

        //        this._params["libelle"] = _eInvitRecipients.Label;// libellé de l'ajout destinataires

        //        if (this._params["immediateAdd"] == "1")
        //            this._params["recurrentAdd"] = "0";
        //        else
        //            this._params["recurrentAdd"] = "1";

        //        if (this._params["recurrentAdd"] == "1")
        //            this._params["immediateAdd"] = "0";
        //        else
        //            this._params["immediateAdd"] = "1";

        //        this._params["scheduleUpdated"] = "0";           //[pour savoir si l'utilisateur a modifié la planification]

        //        this._params["mediaType"] = _eInvitRecipients.MediaTypeId.ToString();     // [id du type de média] 

        //        this._params["activeAdress"] = _eInvitRecipients.AddressActive ? "1" : "0";   // [0 : ne pas utiliser adresses actives] ou [1 : utiliser adresses active] 
        //        this._params["mainAdress"] = _eInvitRecipients.AddressMain ? "1" : "0";      // [0 : ne pas utiliser adresses principales] ou [1 : utiliser adresses principales] 
        //        //this._params["removeDoubles"] = _eInvitRecipients.RemoveDoubles ? "1" : "0";  // [0 : dédoublonner] ou [1 : ne pas dédoublonner] 


        //        this._iParentTabFileId = _eInvitRecipients.ParentFileId;

        //        if (_eInvitRecipients.BkmTabId > 0)
        //            this._iTabFrom = _eInvitRecipients.BkmTabId;
        //        else
        //            this._iTabFrom = _eInvitRecipients.ParentTabId;

        //        #region Fiche Étape
        //        if ((_eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_ALL || _eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_FILTER) && _eCampaign.ParentTabId != 0)
        //        {
        //            //Chargement du DescId de la table Étapes
        //            int eventStepDescId = 0;
        //            int.TryParse(DescAdvDataSet.LoadAndGetAdvParam(dal, _eInvitRecipients.ParentTabId, DESCADV_PARAMETER.EVENT_STEP_DESCID, "0"), out eventStepDescId);

        //            if (eventStepDescId != 0)
        //            {
        //                this._eventStepDescId = eventStepDescId;

        //                //Chargement de l'id de la fiche Étapes
        //                int eventStepFileId = eEventStepXRM.FindEventStepByCampaignId(dal, this._eventStepDescId, _iMailingId);

        //                if (eventStepFileId != 0)
        //                {
        //                    this._eventStepFileId = eventStepFileId;

        //                    eEventStepXRM eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
        //                    string eventStepError;
        //                    if (!eventStep.LoadEventStepFile(out eventStepError, dal))
        //                        throw new Exception(eventStepError);

        //                    this._eventStep = eventStep;
        //                    this._params["eventStepDescId"] = this._eventStepDescId.ToString(); //[DescId table EventStep]
        //                    this._params["eventStepFileId"] = this._eventStepFileId.ToString(); // [Id de la fiche EventStep]
        //                    this._params["scheduleId"] = this._eventStep.ScheduleId.ToString(); //[Id de la plannification (table Schedule)]
        //                    this._params["fid"] = this._eventStep.RecipientsFilterId.ToString(); //[Id filtre destinataires]
        //                }
        //            }
        //        }
        //        #endregion

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        _strErrorMessage = string.Concat("eInvitRecip.LoadFromDB() : ", ex);
        //        return false;
        //    }
        //    finally
        //    {
        //        dal.CloseDatabase();
        //    }
        //}

    }
}
