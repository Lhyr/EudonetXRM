using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.wcfs.data.mailing;
using Com.Eudonet.Merge;
using Com.Eudonet.Merge.Eudonet;
using Com.Eudonet.Xrm.mgr;
using EudoProcessInterfaces;
using EudoQuery;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Internal.wcfs.data.common;
using Newtonsoft.Json;
using Com.Eudonet.Internal.tools.filler;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe eMailing représentant un objet mailing
    /// </summary>
    public class eMailing
    {
        /// <summary>
        /// Action d'e-mailing possible
        /// Meme enum disponible en js
        /// </summary>
        public enum MailingAction
        {
            /// <summary>pas d'action (ne doit pas se produire)</summary>
            NONE = 0,
            /// <summary>Enregistrement de la campagne</summary>
            INSERT = 1,
            /// <summary>Mise à jours de la campagne</summary>
            UPDATE = 2,
            /// <summary>Envoyer la campagne</summary>
            SEND = 3,
            /// <summary>Annuler la campagne</summary>
            CANCEL = 4,
            /// <summary>Tester l'envoi</summary>
            SEND_TEST = 5,
            /// <summary>Vérification des liens</summary>
            CHECK_LINKS = 6,

            /// <summary>Reset Mail Tester</summary>
            RESET_MAIL_TESTER = 7,

        }


        /// <summary>
        /// Préférence utilisateur
        /// </summary>
        protected ePref _ePref;

        /// <summary>
        /// id du mailing
        /// </summary>
        protected int _iMailingId = 0;

        /// <summary>
        /// table parente
        /// </summary>
        protected int _iParentTab = 0;

        /// <summary>
        /// id fiche parente
        /// </summary>
        protected int _iParentTabFileId = 0;


        /// <summary>
        /// descid table de départ du mailing (pp, pm, invitation...)
        /// </summary> 
        protected int _iTab = 0;

        /// <summary>
        /// descid de la table destinataire
        /// </summary>
        protected int _iRecipientTab = 0;

        /// <summary>
        /// Paramètres génériques
        /// </summary>
        protected ExtendedDictionary<string, string> _params;


        /// <summary>
        /// dictionnaire des adresses e-mail (descid -> email@email.email)
        /// </summary>
        protected Dictionary<int, string> _emailFields;

        /// <summary>
        /// type de mailing/smsing (origine de lancement : liste/fiche/campagne)
        /// </summary>
        protected TypeMailing _mailingType;

        //objet serialisés
        eAnalyzerInfos _bodyAnalyse;

        /// <summary>
        /// Objet du mail - version sérialisé
        /// </summary>
        protected byte[] _serializedSubject;

        /// <summary>
        /// Objet du mail - en "human readable"
        /// </summary>
        protected string _subjectInClear = string.Empty;

        /// <summary>
        /// Texte d'aperçu
        /// </summary>
        protected string _preheader = string.Empty;

        //types d'emailing TODO vérifie la table configadv

        /// <summary>Status de la capagne</summary>
        protected CampaignStatus _eStatus = CampaignStatus.MAIL_IN_PREPARATION;

        /// <summary> Type d'envoi (Eudon, Mapp...)</summary>
        protected MAILINGSENDTYPE _eSendType = MAILINGSENDTYPE.EUDONET;

        /// <summary>Domain alias de partenaire (pour mapp)</summary>
        protected string _eSenderDomainAlias = string.Empty;

        //Parametres de base de connexion 
        /// <summary>Dal pour les requête</summary>
        protected eudoDAL _dal;

        /// <summary> Indique s'il s'agit d'un dal local (a fermé a près usage) ou un global (a garder ouvert) </summary>
        protected bool _bIsLocalDal = true;

        /// <summary>
        /// Indique si un dal local a été ouvert (et doit donc être fermé)
        /// </summary>
        protected bool _bIsLocalDalOpened = false;

        /// <summary>Message d'erreur</summary>
        protected string _strErrorMessage = string.Empty;

        /// <summary>Dernière requête de récupération des ids créées</summary>
        protected string _sBuildRecipientQuery = string.Empty;

        /// <summary>les ids des pj a mettre a jour après création de la fiche</summary>
        protected string _strPjIds = string.Empty;

        /// <summary>
        /// ID du modèle de mail choisi
        /// </summary>
        private int _mailTemplate = 0;

        /// <summary>DescId Table Étape</summary>
        private int _eventStepDescId = 0;

        /// <summary>Id fiche Étape</summary>
        private int _eventStepFileId = 0;

        private eEventStepXRM _eventStep = null;

        private bool _MailBodyNcharPerLine = false;

        private bool _EnbleSendingToMailTester = false;

        //gérer les erreurs mailTester
        private bool _ErrorOnMailTester = false;
        private string _ErrorMailTester = string.Empty;

        #region Enumérations

        #endregion

        #region Accesseurs
        /// <summary>Identifiant du rapport dans la table MailCampaign.</summary>
        public int Id
        {
            get { return this._iMailingId; }
        }

        //En cas d'erreur dans mailTester
        public bool ErrorOnMailTester
        {
            get { return _ErrorOnMailTester; }
        }

        public string ErrorMailTester
        {
            get { return _ErrorMailTester; }
        }

        /// <summary>les ids des pj a mettre a jour après création de la fiche</summary>
        public string PjIds
        {
            get { return this._strPjIds; }
            set { this._strPjIds = value; }
        }

        /// <summary>
        /// Liste des rubriques de type adresse mail
        /// </summary>
        public Dictionary<int, string> EmailFields
        {
            get { return this._emailFields; }
        }

        /// <summary>Utilisateur créateur de la campaigne.</summary>
        public int Owner
        {
            get { return this._ePref.User.UserId; }
        }

        /// <summary>Type du mailing</summary>
        public TypeMailing MailingType
        {
            get { return this._mailingType; }
            set { this._mailingType = value; }
        }

        /// <summary>Type du mailing</summary>
        public string SenderDomainAlias
        {
            get { return this._eSenderDomainAlias; }

        }
        /// <summary>Type d'envoi</summary>
        public MAILINGSENDTYPE SendeType
        {
            get { return this._eSendType; }

        }

        /// <summary>Onglet/Signet sur lequel la campagne à été créé</summary>
        public int Tab
        {
            get { return this._iTab; }
            set { this._iTab = value; }
        }
        /// <summary>Onglet sur laquelle la campagne à été créé</summary>
        public int ParentTab
        {
            set { this._iParentTab = value; }
            get { return this._iParentTab; }
        }

        /// <summary>Fiche parente</summary>
        public int ParentFileId
        {
            set { this._iParentTabFileId = value; }
            get { return this._iParentTabFileId; }

        }


        /// <summary>
        /// return l objet param 
        /// </summary>
        public ExtendedDictionary<string, string> MailingParams
        {
            get { return _params; }
        }

        /// <summary>
        /// Modèle de mail
        /// </summary>
        public int MailTemplate
        {
            get { return _mailTemplate; }
            set { _mailTemplate = value; }
        }



        #endregion

        /// <summary>
        /// Constructeur de l'object Emailing
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        public eMailing(ePref pref)
        {

            this._ePref = pref;
            this._emailFields = new ExtendedDictionary<int, string>();

            if (this._iMailingId == 0)
                InitParams();

            LoadConfigAdv();

        }

        /// <summary>
        /// Charge les paramètres de configuration propre au mailing
        /// </summary>
        protected virtual void LoadConfigAdv()
        {
            //Depuis eCircle ou eudonet
            IDictionary<eLibConst.CONFIGADV, string> dic = eLibTools.GetConfigAdvValues(this._ePref,
                new HashSet<eLibConst.CONFIGADV>
                {
                    eLibConst.CONFIGADV.MAILINGSENDTYPE,
                    eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN,
                    eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE
                });

            string sValue = string.Empty;
            _eSendType = MAILINGSENDTYPE.EUDONET;
            if (dic.TryGetValue(eLibConst.CONFIGADV.MAILINGSENDTYPE, out sValue))
                Enum.TryParse<MAILINGSENDTYPE>(sValue, out _eSendType);

            _eSenderDomainAlias = dic[eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN];

            if (dic.ContainsKey(eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE) && !string.IsNullOrEmpty(dic[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE]))
                _MailBodyNcharPerLine = dic[eLibConst.CONFIGADV.SPLITTING_MAIL_BODY_NCHAR_PER_LINE] == "1";
        }


        /// <summary>
        /// Constructeur de l'object Emailing
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="mailingId">Id du rapport</param>
        public eMailing(ePref pref, int mailingId)
            : this(pref)
        {
            this._iMailingId = mailingId;
            if (this._iMailingId > 0)
            {
                LoadFromDB();
            }
        }

        /// <summary>
        /// Constructeur de l'object Emailing
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="mailingId">Id du rapport</param>
        /// <param name="mailingType">Type de mailling</param>
        public eMailing(ePref pref, int mailingId, TypeMailing mailingType = TypeMailing.MAILING_FROM_LIST)
            : this(pref)
        {
            this._iMailingId = mailingId;
            this._mailingType = mailingType;


            if (this._iMailingId > 0)
            {
                LoadFromDB();
            }

            // TODO : LoadEmailFields utilise la variable this._iTab qui a ce niveau n'est pas encore
            // définie. (Elle est fournie après l'appel au constructeur cf eMaillingManager.ProcessAction)
            //Cela ne lèvait pas d'erreur mais suite au correctif #39216 le fait que la valeur ne soit pas passée
            // levait l'erreur. Dans le cas de ntab = 0, j'ai ignoré cette levée d'erreur puisqu'il semble que cela soit "normal"
            if (mailingType != TypeMailing.MAILING_FROM_CAMPAIGN && mailingType != TypeMailing.SMS_MAILING_FROM_BKM && _iTab != 0)
                LoadEmailFields();
        }

        /// <summary>
        /// Constructeur de l'object Emailing
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="mailingId">Id du rapport</param>
        /// <param name="nTab">Table de lancement du mailing</param>
        /// <param name="mailingType">Origine du mailing</param>
        /// <param name="dal">EudoDal Externe</param>
        public eMailing(ePref pref, int mailingId, int nTab, eudoDAL dal, TypeMailing mailingType = TypeMailing.MAILING_FROM_LIST)
            : this(pref)
        {
            this._mailingType = mailingType;
            this._iMailingId = mailingId;
            this._iTab = nTab;
            if (dal != null)
            {
                this._bIsLocalDal = false;
                this._dal = dal;
            }

            if (this._iMailingId > 0)
                LoadFromDB();

            if (mailingType != TypeMailing.MAILING_FROM_CAMPAIGN && mailingType != TypeMailing.SMS_MAILING_FROM_BKM)
                LoadEmailFields();
        }

        /// <summary>
        /// Initialise les parametre js
        /// </summary>
        private void InitParams()
        {
            this._params = new ExtendedDictionary<string, string>();
            this._params.Add("libelle", string.Concat(eResApp.GetRes(this._ePref, 6407).Replace(":", ""), " ", FormatDisplayDate(DateTime.Now, true)));// libelle de la campaigne
            this._params.Add("markedFiles", "0");     // [1 : fiches cochées] ou [0 : liste en cours]
            this._params.Add("nMailCount", "0");     // nombre d adresse mail
            this._params.Add("mailFieldDescId", "0"); // [desc id du champs de type e-mail] ou [0 : invalide]
            this._params.Add("activeAdress", "0");   // [0 : ne pas utiliser adresses actives] ou [1 : utiliser adresses active] 
            this._params.Add("mainAdress", "0");      // [0 : ne pas utiliser adresses principales] ou [1 : utiliser adresses principales] 
            this._params.Add("templateType", "0");    // [0 : template utilisateur] ou [1 : template personnalisé] ou [2 : no template] 
            this._params.Add("templateId", "0");      // [0 : no template] ou [id : id template]
            this._params.Add("HtmlFormat", "1");   // [0 : text] ou [1 : html]
            this._params.Add("sender", "");        // ['exemple@Mail.Adr']
            this._params.Add("senderAliasDomain", "");        // ['Mail.Adr.Com']
            this._params.Add("ccrecipient", "0");        // ['exemple@Mail.Adr']
            this._params.Add("displayName", "");   // ['Nom apparent']
            this._params.Add("replyTo", "");       // ['exemple@Mail.Adr; ...' ou vide] 
            this._params.Add("subject", "");        // ['text' ou vide]
            //SHA : tâche #1 939
            this._params.Add("preheader", "");        // ['text' ou vide]
            this._params.Add("body", "");          // ['text' ou vide] 
            this._params.Add("bodyCss", "");          //style du mail ['text' ou vide] 
            this._params.Add("histo", "0");         // [0 : ne pas historiser] ou [1 : historiser]
            this._params.Add("description", "");   // ['text' ou vide] 
            this._params.Add("mediaType", "");      // [id du type de média]
            this._params.Add("category", "");     // [id de la catégorie ou 0 pour toutes la catégorie] 
            this._params.Add("excludeUnsub", "1");  // [0 : included] ou [1 : excluded]
            this._params.Add("mailTabDescId", "0");      // [0 :  pas de fichier] ou [descid de la table mail]
            this._params.Add("trackLnkPurgedDate", DateTime.Now.AddMonths(6).ToShortDateString());  //[ 'date' du purge des liens tracking]
            this._params.Add("trackLnkLifeTime", DateTime.Now.AddMonths(1).ToShortDateString()); //[ 'date' durée de vie des liens tracking]
            this._params.Add("immediateSending", "1");         //['1' : immédiate par defaut] ou ['0' : envoi différé]
            this._params.Add("recurrentSending", "0");         //['1' : envoi récurrent] ou ['0' : envoi immédiat ou différé par defaut]
            this._params.Add("RequestMode", "0");          //0 : Envoi avec la photo des id - 1 : Envoi en enregistrant la requête
            this._params.Add("eventStepDescId", "0");          // [DescId de la table EventStep]
            this._params.Add("eventStepFileId", "0");          // [Id de la fiche EventStep]
            this._params.Add("sendingDate", "");         //['vide' : envoi immediat ] ou ['date' : evoi différé]
            this._params.Add("scheduleId", "0");           //[Id de la plannification (table Schedule)]
            this._params.Add("scheduleUpdated", "0");           //[pour savoir si l'utilisateur a modifier la plannification]
            this._params.Add("recipientsFilterId", "0");           //[Id filtre destinataires]
            this._params.Add("recipientstest", "");         //les destinataires a qui on envoi des mail de test
            this._params.Add("operation", "1");          //[ eMailing.MailingOperation.ADD = 1 : new ] ou [eMailing.MailingOperation.UPDATE = 3 : mise ajour]
            this._params.Add("ownerUserId", _ePref.UserId.ToString());
            this._params.Add("removeDoubles", "1"); // Par défaut on dédoublonne
            this._params.Add("sendingTimeZone", ""); //TimeZone utilisateur
            this._params.Add("bccrecipients", ""); // liste des cci
            this._params.Add("OptInEnabled", "");//Consentement
            this._params.Add("OptOutEnabled", "");
            this._params.Add("NoConsentEnabled", "");
            this._params.Add("AdressEmailSwValideEnabled", ""); //quality of adress email
            this._params.Add("AdressEmailSwNotVerifiedEnabled", "");
            this._params.Add("AdressEmailSwInvalideEnabled", "");
            this._params.Add("scoring", "{}");
            this._params.Add("MailTestType", "");//Type du test
            this._params.Add("MailTesterReportId", "");//Mail tester adresse d'envoie
            this._params.Add("MailTesterToken", "");//Mail tester security token
        }

        /// <summary>
        /// On charge la liste des rubriques de type adresse mail
        /// </summary>
        private void LoadEmailFields()
        {
            string sErr = string.Empty;
            this._emailFields = eDataTools.GetAllowedDescIdByFormat(_ePref, _iTab, FieldFormat.TYP_EMAIL);
        }

        /// <summary>
        /// Charge l'ensemble des informations relative à la campagne mail.
        /// </summary>
        /// <returns>true si le traitement s'est bien passé, sinon false.</returns>
        public bool LoadFromDB()
        {
            eCampaign _eCampaign = new eCampaign(_iMailingId);
            string sError = string.Empty;

            eudoDAL dal = null;
            try
            {
                dal = eLibTools.GetEudoDAL(_ePref);
                dal.OpenDatabase();
                // Chargement de la campagne
                _eCampaign.Load(dal, _ePref, out sError);
                if (sError.Length > 0)
                {
                    _strErrorMessage = string.Concat("Erreur de chargement de la campagne ID=", _iMailingId);
                    return false;
                }

                this._params["libelle"] = _eCampaign.Label;// libelle de la campaigne
                this._params["description"] = _eCampaign.Description;   // ['text' ou vide] 
                this._params["displayName"] = _eCampaign.DisplayName;   // ['exemple@Mail.Adr']
                if (_eCampaign.MailTemplateSysDescId.ToString() != null && _eCampaign.MailTemplateSysDescId.ToString() != "0" && _eCampaign.MailTemplateSysDescId.ToString() != "-1")
                {
                    this._params["templateType"] = "0"; // [0 : custom template]
                    this._params["templateId"] = _eCampaign.MailTemplateSysDescId.ToString(); // [id : id template]
                }
                else if (_eCampaign.MailTemplateDescId.ToString() != null && _eCampaign.MailTemplateDescId.ToString() != "0" && _eCampaign.MailTemplateDescId.ToString() != "-1")
                {

                    this._params["templateType"] = "1"; // [1 : template ]   //GCH par défaut au chargement pas de tempalte car on va directement sur l'étape 2/3
                    this._params["templateId"] = _eCampaign.MailTemplateDescId.ToString();  // [id : id template]
                }
                else
                {
                    this._params["templateType"] = "2"; // [2 : no template ] 
                    this._params["templateId"] = "0"; // [0 : no template] // Backlog #83 : on ne change pas la sélection "Pas de modèle" d'une campagne existant < 10.412, même si "Pas de modèle" a été remplacé par le chargement du modèle "Courriel simple"
                }
                this._params["HtmlFormat"] = _eCampaign.Body_HTML ? "1" : "0";   // [0 : text] ou [1 : html]
                this._params["sender"] = _eCampaign.Sender;        // ['exemple@Mail.Adr']
                this._params["senderAliasDomain"] = _eCampaign.SenderAliasDomain;        // ['Mail.Adr.Com']
                this._params["replyTo"] = _eCampaign.ReplyTo;       // ['exemple@Mail.Adr; ...' ou vide] 
                this._params["histo"] = _eCampaign.Historise ? "1" : "0";         // [0 : ne pas historiser] ou [1 : historiser]
                this._params["excludeUnsub"] = "1"; //cf. 29277     _eCampaign.ExcludeUnsub ? "1" : "0";  // [0 : included] ou [1 : excluded]
                this._params["trackLnkPurgedDate"] = _eCampaign.DateLogExpirate.HasValue ? _eCampaign.DateLogExpirate.Value.ToShortDateString() : string.Empty;  //[ 'date' du purge des liens tracking]
                this._params["trackLnkLifeTime"] = _eCampaign.DateTrackExpirate.HasValue ? _eCampaign.DateTrackExpirate.Value.ToShortDateString() : string.Empty; //[ 'date' durée de vie des liens tracking]
                this._params["sendingDate"] = _eCampaign.DateSent.HasValue ? _eCampaign.DateSent.Value.ToString() : string.Empty;         //['vide' : ] ou ['date' : envoi différé]

                this._params["RequestMode"] = ((int)_eCampaign.QueryMode).ToString();
                if ((_eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_ALL || _eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_FILTER))
                    this._params["recurrentSending"] = "1";
                else
                    this._params["recurrentSending"] = "0";

                if (this._params["recurrentSending"] == "1" || this._params["sendingDate"].Length > 0)
                    this._params["immediateSending"] = "0";          //['1' : immédiate par defaut] ou ['0' : envoi différé]
                else
                    this._params["immediateSending"] = "1";

                this._params["scheduleUpdated"] = "0";           //[pour savoir si l'utilisateur a modifier la plannification]

                this._params["subject"] = eMergeTools.GetObjectMerge_Orig(_eCampaign.Subject);        // ['text' ou vide] 
                //SHA : tâche #1 939
                this._params["preheader"] = _eCampaign.PreHeader;     // ['text' ou vide] 
                if (_eCampaign.Body.content != null)
                    this._params["body"] = eMergeTools.GetBodyMerge_Orig(_eCampaign.Body);          // ['text' ou vide] 

                this._params["bodyCss"] = _eCampaign.Body_Css;

                this._params["operation"] = "3";          //[ eMailing.MailingOperation.ADD = 1 : new ] ou [eMailing.MailingOperation.UPDATE = 3 : mise ajour]

                this._params["mediaType"] = _eCampaign.MediaTypeId.ToString();     // [id du type de média] 
                this._params["category"] = _eCampaign.CategoryId.ToString();     // [id de la catégorie ou 0 pour toutes la catégorie] 
                this._params["categoryLabel"] = _eCampaign.CategoryLabel;
                this._params["mailFieldDescId"] = _eCampaign.MailAddressDescId.ToString(); // [desc id du champs de type e-mail] ou [-1 : erreur]

                this._params["ccrecipient"] = _eCampaign.MailccDescId.ToString();        // ['exemple@Mail.Adr']

                this._params["mailTabDescId"] = _eCampaign.RecipientTabId.ToString();      // [0 :  pas de fichier] ou [descid de la table mail]
                // this._params["markedFiles"] = "0";     // [0 : fiches cochées] ou [1 : liste en cours]

                this._params["activeAdress"] = _eCampaign.AddressActive ? "1" : "0";   // [0 : ne pas utiliser adresses actives] ou [1 : utiliser adresses active] 
                this._params["mainAdress"] = _eCampaign.AddressMain ? "1" : "0";      // [0 : ne pas utiliser adresses principales] ou [1 : utiliser adresses principales] 
                //this._params["ownerMainEmail"] = (!string.IsNullOrEmpty(_eCampaign.OwnerMainEmail)) ? _eCampaign.OwnerMainEmail : _eCampaign.Sender;
                this._params["removeDoubles"] = _eCampaign.RemoveDoubles ? "1" : "0";      // [0 : ne pas utiliser adresses principales] ou [1 : utiliser adresses principales] 


                this._iParentTabFileId = _eCampaign.ParentFileId;

                if (_eCampaign.BkmTabId > 0)
                    this._iTab = _eCampaign.BkmTabId;
                else
                    this._iTab = _eCampaign.ParentTabId;

                //     this._params["nMailCount"] = eListFactory.GetCountRecipientsCampaign(this._ePref, this._iMailingId).ToString();     // nombre d adresse mail


                #region Fiche Étape
                if ((_eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_ALL || _eCampaign.QueryMode == MAILINGQUERYMODE.RECURRENT_FILTER) && _eCampaign.ParentTabId != 0)
                {
                    //Chargement du DescId de la table Étapes
                    int eventStepDescId = 0;
                    int.TryParse(DescAdvDataSet.LoadAndGetAdvParam(dal, _eCampaign.ParentTabId, DESCADV_PARAMETER.EVENT_STEP_DESCID, "0"), out eventStepDescId);

                    if (eventStepDescId != 0)
                    {
                        this._eventStepDescId = eventStepDescId;

                        //Chargement de l'id de la fiche Étapes
                        int eventStepFileId = eEventStepXRM.FindEventStepByCampaignId(dal, this._eventStepDescId, _iMailingId);

                        if (eventStepFileId != 0)
                        {
                            this._eventStepFileId = eventStepFileId;

                            eEventStepXRM eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
                            string eventStepError;
                            if (!eventStep.LoadEventStepFile(out eventStepError, dal))
                                throw new Exception(eventStepError);

                            this._eventStep = eventStep;
                            this._params["eventStepDescId"] = this._eventStepDescId.ToString(); //[DescId table EventStep]
                            this._params["eventStepFileId"] = this._eventStepFileId.ToString(); // [Id de la fiche EventStep]
                            this._params["scheduleId"] = this._eventStep.ScheduleId.ToString(); //[Id de la plannification (table Schedule)]
                            this._params["recipientsFilterId"] = this._eventStep.RecipientsFilterId.ToString(); //[Id filtre destinataires]
                        }
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                _strErrorMessage = string.Concat("eMailing.LoadFromDB() : ", ex);
                return false;
            }
            finally
            {
                dal.CloseDatabase();
            }
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
        /// Chargement et mise a jour des parametres depuis un dictionnaire
        /// </summary>
        /// <param name="dicParam"></param>
        public void LoadFrom(ExtendedDictionary<string, string> dicParam)
        {
            foreach (KeyValuePair<string, string> kv in dicParam)
                _params.UpdateContainsValue(kv.Key, kv.Value);

            int descid;
            int.TryParse(this._params["mailFieldDescId"], out descid);
            this._iRecipientTab = eLibTools.GetTabFromDescId(descid);
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
            this._params.AddOrUpdateValue(ParamKey, Value, true);
        }

        /// <summary>
        /// Sauvegarde la campagne d'emailing en cours
        /// </summary>
        public void Save()
        {
            if (this._iMailingId == 0)
                Insert();
            else
                Update();
        }

        /// <summary>
        /// Exécute l'action demandée
        /// </summary>
        /// <param name="action">Action à executer : insert, update,...</param>
        public void Run(MailingAction action)
        {
            UpdateStatus(action);

            switch (action)
            {
                case MailingAction.CANCEL:
                    Cancel();
                    break;

                case MailingAction.INSERT:
                    Insert();
                    break;

                case MailingAction.UPDATE:
                    Update();
                    break;

                case MailingAction.SEND:
                    SaveAndSend();
                    break;

                case MailingAction.SEND_TEST:
                    SaveAndSendTest();
                    break;

                case MailingAction.RESET_MAIL_TESTER:
                    eCampaign.UpdateMailTesterInfo(_ePref, "", _iMailingId);
                    break;

                default:
                    break;
            }
        }

        private void UpdateStatus(MailingAction action)
        {
            //emailing en préparation
            _eStatus = CampaignStatus.MAIL_IN_PREPARATION;

            if (_mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                _eStatus = CampaignStatus.MAIL_WRKFLW_IN_PREPARATION;
            //envoi en cours ou différé
            if (action == MailingAction.SEND)
            {
                if (this._params["immediateSending"] == "1" && this._params["recurrentSending"] == "0")
                {
                    _eStatus = CampaignStatus.MAIL_SENDING;
                    this._params["sendingDate"] = DateTime.Now.ToString();

                }
                else if (this._params["immediateSending"] == "0" && this._params["recurrentSending"] == "0")
                {
                    _eStatus = CampaignStatus.MAIL_DELAYED;
                }
                else if (this._params["immediateSending"] == "0" && this._params["recurrentSending"] == "1")
                {
                    _eStatus = CampaignStatus.MAIL_RECURRENT;
                }

                if (_mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                    _eStatus = CampaignStatus.MAIL_WRKFLW_READY;
            }

            //annulation de l'envoi
            if (action == MailingAction.CANCEL)
                _eStatus = CampaignStatus.MAIL_CANCELED;
        }

        /// <summary>
        /// Annule l'envoi de la campagne depuis WCF
        /// </summary>
        private void Cancel()
        {
            string sError = string.Empty;

            #region MAJ statut de la Campagne à annulé
            try
            {
                OpenDal();

                RqParam rq = new RqParam("update [CAMPAIGN] set [status] = @status where [CampaignId] = @CampaignId ");
                rq.AddInputParameter("@CampaignId", SqlDbType.Int, _iMailingId);
                rq.AddInputParameter("@status", SqlDbType.Int, CampaignStatus.MAIL_CANCELED.GetHashCode());
                _dal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0)
                    _strErrorMessage = string.Concat(_strErrorMessage, Environment.NewLine, "Erreur sur Cancel.ExecuteNonQuery (revert status) :", sError);
            }
            finally
            {
                CloseDal();
            }
            #endregion

            #region Appel du WCF
            eMailingCall mailingCall = new eMailingCall();
            mailingCall.UserId = _ePref.User.UserId;
            mailingCall.Lang = _ePref.Lang;
            mailingCall.PrefSQL = _ePref.GetNewPrefSql();
            mailingCall.CampaignId = _iMailingId;

            try
            {
                eWCFTools.WCFEudoProcessCaller<IEudoMailingWCF, bool>(
                    ConfigurationManager.AppSettings.Get("EudoMailingURL"), obj => obj.StopMailing(mailingCall, out sError));

                if (sError.Length > 0)
                    _strErrorMessage = sError;
            }
            catch (EndpointNotFoundException ExWS)
            {
                _strErrorMessage = string.Concat(eResApp.GetRes(_ePref, 6660), ", ", eResApp.GetRes(_ePref, 6565), " : ", Environment.NewLine, ExWS.ToString());
            }
            catch (Exception ex)
            {
                _strErrorMessage = ex.ToString();
            }

            #endregion
            #region Si erreur sur appel WCF on annule la MAJ du statut de la Campagne à Différé
            if (_strErrorMessage.Length > 0)
            {
                OpenDal();
                try
                {
                    RqParam rq = new RqParam("update [CAMPAIGN] set [status] = @status where [CampaignId] = @CampaignId ");
                    rq.AddInputParameter("@CampaignId", SqlDbType.Int, _iMailingId);
                    rq.AddInputParameter("@status", SqlDbType.Int, CampaignStatus.MAIL_DELAYED.GetHashCode());
                    _dal.ExecuteNonQuery(rq, out sError);
                    if (sError.Length > 0)
                        _strErrorMessage = string.Concat(_strErrorMessage, Environment.NewLine, "Erreur sur Cancel.ExecuteNonQuery (revert status) :", sError);
                }
                finally
                {
                    CloseDal();
                }
            }
            #endregion
        }

        /// <summary>
        /// Ajoute une nouvelle campagne d'emailing
        /// </summary>
        private void Insert()
        {
            try
            {
                InsertOrUpdateCampaign();
                InsertIntoCampaignSelection();
                UpdatePJLinks();
                UpdateCampaignBody();
            }
            catch (eMailingException)
            {
                // Un emailing doit avoir un enregistrement dans campaign ET des liaisons dans CampaignSelection
                // Sinon il n'est pas valide
                RollBack();
                throw;
            }
            catch (Exception ex)
            {
                RollBack();
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }

        /// <summary>
        /// Analyse et serialise les rubriques comme body, subject, preheader...
        /// </summary>
        protected virtual void AnalyseAndSerializeFields()
        {
            this._subjectInClear = this._params["subject"];

            eAnalyzerInfos subjectAnalyse = eMergeTools.AnalyzeObject(this._subjectInClear);

            this._serializedSubject = SerializerTools.ByteSerialize(subjectAnalyse);

            bool invalidSubject = subjectAnalyse.content == null || subjectAnalyse.mergeData.ExistInvalidMerge;
            if (invalidSubject)
                throw new eMailingException(eErrorCode.INVALID_SUBJECT_LINKS);

            //SHA : tâche #1 941
            this._preheader = this._params["preheader"];
            eAnalyzerInfos preheaderAnalyse = eMergeTools.AnalyzePreheader(this._preheader);
            bool invalidPreheader = preheaderAnalyse.content == null || preheaderAnalyse.mergeData.ExistInvalidMerge;
            if (invalidPreheader)
                throw new eMailingException(eErrorCode.INVALID_PREHEADER_LINKS);

            this._bodyAnalyse = eMergeTools.AnalyzeBody(this._params["body"]);

            bool invalidBody = _bodyAnalyse.content == null || _bodyAnalyse.linksData.ExistInvalidLink || _bodyAnalyse.mergeData.ExistInvalidMerge;
            if (invalidBody)
                throw new eMailingException(eErrorCode.INVALID_BODY_LINKS);
        }

        /// <summary>
        /// Insere une ligne dans la table Campaign
        /// </summary>
        private void InsertOrUpdateCampaign(bool isTestMail = false)
        {
            //Analyse du body et du sujet avant la MAJ ou INSERT pour bien initialiser les variables
            AnalyseAndSerializeFields();

            Engine.Engine eng = eModelTools.GetEngine(_ePref, (int)TableType.CAMPAIGN, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            if (this._iMailingId > 0)
                eng.FileId = this._iMailingId;

            #region eng.AddNewValue : Rubriques / Valeurs

            //Valeurs recupérées depuis params
            eng.AddNewValue(CampaignField.CAMPAIGN.GetHashCode(), this._params["libelle"]);
            eng.AddNewValue(CampaignField.MEDIATYPE.GetHashCode(), this._params["mediaType"]);
            eng.AddNewValue(CampaignField.CATEGORY.GetHashCode(), this._params["category"]);
            eng.AddNewValue(CampaignField.CCRECIPIENTS.GetHashCode(), this._params["ccrecipient"]);
            eng.AddNewValue(CampaignField.DATELOGEXPIRATE.GetHashCode(), this._params["trackLnkPurgedDate"]);
            eng.AddNewValue(CampaignField.DATESENT.GetHashCode(), this._params["sendingDate"]);
            if (_params.ContainsKey("RequestMode"))
                eng.AddNewValue(CampaignField.REQUESTMODE.GetHashCode(), this._params["RequestMode"]);
            if (_iMailingId <= 0 || _eStatus == CampaignStatus.MAIL_RECURRENT) //Si mail récurrent, on régénère la requête au moment de l'enregistrement "définitif"
                eng.AddNewValue(CampaignField.REQUEST.GetHashCode(), BuildRecipientQuery());    //La requête ne s'enregistre qu'à la création



            if (this._params.ContainsKey("bccrecipients"))
            {
                eMailAddressConteneur bcc = new eMailAddressConteneur(this._params["bccrecipients"]);
                if (!string.IsNullOrEmpty(bcc.CleanedAndFormattedOrigin))
                {
                    eng.AddNewValue((int)CampaignField.BCCRECIPIENTS, bcc.CleanedAndFormattedOrigin);
                }
            }

            ExtendedScoreDetail extendedScore = new ExtendedScoreDetail();
            if (this._params.ContainsKey("scoring") && !string.IsNullOrEmpty(this._params["scoring"]))
            {


                var def = new { mailtester = 0.0, other = 0.0, qualityEmailAdresses = 0.0, campaignType = 0.0, recepientCount = 0.0 };

                var t = JsonConvert.DeserializeAnonymousType(this._params["scoring"], def);
                eng.AddNewValue(CampaignField.RATING.GetHashCode(), (t.qualityEmailAdresses + t.mailtester + t.recepientCount + t.campaignType).ToString(), null, readOnly: true, ignoreTestRights: true); ;

                extendedScore = new ExtendedScoreDetail()
                {
                    Consent = t.campaignType,
                    EmailTested = t.mailtester,
                    Duplicate = t.recepientCount,
                    Quality = t.qualityEmailAdresses

                };
            }

            eng.AddNewValue(CampaignField.DATETRACKEXPIRATE.GetHashCode(), this._params["trackLnkLifeTime"]);

            eng.AddNewValue(CampaignField.DISPLAYNAME.GetHashCode(), this._params["displayName"]);
            eng.AddNewValue(CampaignField.EXCLUDEUNSUB.GetHashCode(), this._params["excludeUnsub"]);



            eng.AddNewValue(CampaignField.HISTO.GetHashCode(), this._params["histo"]);
            eng.AddNewValue(CampaignField.DESCRIPTION.GetHashCode(), this._params["description"]);
            eng.AddNewValue(CampaignField.STATUS.GetHashCode(), this._eStatus.GetHashCode().ToString());



            eng.AddNewValue(CampaignField.ISHTML.GetHashCode(), this._params["HtmlFormat"]);
            eng.AddNewValue(CampaignField.MAILADDRESSDESCID.GetHashCode(), this._params["mailFieldDescId"]);
            eng.AddNewValue(CampaignField.RECIPIENTTABID.GetHashCode(), this._params["mailTabDescId"]);
            eng.AddNewValue(CampaignField.REPLYTO.GetHashCode(), this._params["replyTo"]);
            eng.AddNewValue(CampaignField.SENDER.GetHashCode(), this._params["sender"]);
            string ownerUserId = string.Empty;
            if (this._params.TryGetValue("ownerUserId", out ownerUserId))
                eng.AddNewValue(CampaignField.OWNERUSER.GetHashCode(), ownerUserId); // Appartient à

            if (this._params.ContainsKey("bodyCss"))
                eng.AddNewValue(CampaignField.BODYCSS.GetHashCode(), this._params["bodyCss"]);
            // Passé la valeur 0 pour eviter qu'Engine calcul la valeur par defaut des rubriques ADDRESSACTIVE et ADDRESSMAIN - 42 639
            eng.AddNewValue(CampaignField.ADDRESSACTIVE.GetHashCode(), this._params["activeAdress"] == "1" ? "1" : "0");
            eng.AddNewValue(CampaignField.ADDRESSMAIN.GetHashCode(), this._params["mainAdress"] == "1" ? "1" : "0");
            //dédoublonné les adresses mail
            eng.AddNewValue(CampaignField.REMOVEDOUBLES.GetHashCode(), this._params["removeDoubles"] == "1" ? "1" : "0");
            // Modèles utilisateur depuis la table mail template
            if (this._params["templateType"].Equals("0"))
                eng.AddNewValue(CampaignField.MAILTEMPLATEID.GetHashCode(), this._params["templateId"]);
            // Modèles personalisables depuis le fichier xml dans le dossier emailing
            else if (this._params["templateType"].Equals("1"))
                eng.AddNewValue(CampaignField.MAILTEMPLATESYSID.GetHashCode(), this._params["templateId"]);

            // Cible etendu , ++
            if (_mailingType == TypeMailing.MAILING_FROM_BKM || _mailingType == TypeMailing.SMS_MAILING_FROM_BKM || _mailingType == TypeMailing.MAILING_FROM_CAMPAIGN || _mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
            {
                eng.AddNewValue(CampaignField.BKMTABID.GetHashCode(), this._iTab.ToString());
                eng.AddNewValue(CampaignField.PARENTTABID.GetHashCode(), this._iParentTab.ToString());

                if (this._iMailingId == 0)
                {
                    //Demande #31168 - [XRM] - Rattachement Campagne - Evènement - A la création d'une campagne depuis un ++ si un champ de liaison est présent sur la campagne, on le pré-remplit avec l'id de l'événement
                    foreach (int nDescId in GetCustomLinkDescIdListFromTab(_iParentTab))
                    {
                        eng.AddNewValue(nDescId, this._iParentTabFileId.ToString());
                    }
                }
            }
            else
                eng.AddNewValue(CampaignField.PARENTTABID.GetHashCode(), this._iTab.ToString());


            eng.AddNewValue(CampaignField.PARENTFILEID.GetHashCode(), this._iParentTabFileId.ToString());

            eng.AddNewValue(CampaignField.SENDTYPE.GetHashCode(), this._eSendType.GetHashCode().ToString());


            // Objet de la campagne mail en clair
            eng.AddNewValue((int)CampaignField.SUBJECTINCLEAR, this._subjectInClear);
            // Objet de la campagne mail serialisé
            eng.AddBinaryValue(CampaignField.SUBJECT.GetHashCode(), this._serializedSubject);
            //SHA : tâche #1 939
            if (this._params.ContainsKey("preheader"))
                eng.AddNewValue((int)CampaignField.PREHEADER, this._preheader);
            //eng.AddNewValue((int)CampaignField.PREHEADER, this._params["preheader"]);

            #endregion


            eng.EngineProcess(new StrategyCruSimpleUpdateCampaign());

            EngineResult engResult = eng.Result;

            if (engResult == null)
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_UPDATE_BODY, "eMailing::InsertOrUpdateCampaign() :: engResult == null");

            if (!engResult.Success)
            {


                if (engResult.Error != null)
                {
                    eMailingException ex = new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, engResult.Error.DebugMsg, engResult.Error.Msg);
                    ex.UserMessageTitle = engResult.Error.Title;
                    ex.UserMessageDetails = engResult.Error.Detail;
                    throw ex;

                }
                else
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, "eMailing::InsertOrUpdateCampaign()::engResult.Error = null");
            }

            if (this._iMailingId == 0)
            {


                this._iMailingId = engResult.NewRecord.FilesId[0];
            }

            if (this._iMailingId != 0)
            {



                Dictionary<string, string> dicoValues = new Dictionary<string, string>() { { "SenderAliasDomain", this._params["senderAliasDomain"] } };
                //Ajout des consentements
                if (this._params.ContainsKey("OptInEnabled"))
                    dicoValues.Add("optInEnabled", this._params["OptInEnabled"]);
                if (this._params.ContainsKey("NoConsentEnabled"))
                    dicoValues.Add("NoConsentEnabled", this._params["NoConsentEnabled"]);
                if (this._params.ContainsKey("OptOutEnabled"))
                    dicoValues.Add("OptOutEnabled", this._params["OptOutEnabled"]);
                //qualité adresse email
                AdressStatusParam adrEmailStatus = new AdressStatusParam();

                if (this._params.ContainsKey("AdressEmailSwValideEnabled") && !string.IsNullOrEmpty(this._params["AdressEmailSwValideEnabled"]))
                    adrEmailStatus.ValidAdress = this._params["AdressEmailSwValideEnabled"].ToString() == "1" ? true : false;
                if (this._params.ContainsKey("AdressEmailSwNotVerifiedEnabled") && !string.IsNullOrEmpty(this._params["AdressEmailSwNotVerifiedEnabled"]))
                    adrEmailStatus.NotVerifiedAdress = this._params["AdressEmailSwNotVerifiedEnabled"].ToString() == "1" ? true : false;
                if (this._params.ContainsKey("AdressEmailSwInvalideEnabled") && !string.IsNullOrEmpty(this._params["AdressEmailSwInvalideEnabled"]))
                    adrEmailStatus.InvalidAdress = this._params["AdressEmailSwInvalideEnabled"].ToString() == "1" ? true : false;

                dicoValues.Add("ADRESSSTATUSPARAM", SerializerTools.JsonSerialize(adrEmailStatus));
                dicoValues.Add("ratingdetail", SerializerTools.JsonSerialize(extendedScore));



                UpdateCampaignOtherFields(dicoValues);

                //Envois récurrents
                //Création/MaJ fiche Etapes
                if (!isTestMail)
                    InsertOrUpdateEventStep();
            }
        }

        /// <summary>
        /// Met à jour les champs de la campagne qui ne sont pas adressable par EudoQuery/Engine 
        /// </summary>
        private void UpdateCampaignOtherFields(Dictionary<string, string> dicoValues)
        {
            bool dalWasOpened = false;
            try
            {
                TableLite campaignInfo = eLibTools.GetTableInfo(_ePref, (int)TableType.CAMPAIGN, TableLite.Factory());

                StringBuilder sb = new StringBuilder().AppendLine(string.Concat("UPDATE [", campaignInfo.TabName, "] SET"));
                RqParam rq = new RqParam();
                bool firstParam = true;
                foreach (KeyValuePair<string, string> kvp in dicoValues)
                {
                    if (!firstParam)
                        sb.Append(",");
                    else
                        firstParam = false;

                    sb.AppendLine(string.Concat("[", kvp.Key, "] = @", kvp.Key, "Value"));
                    rq.AddInputParameter(string.Concat("@", kvp.Key, "Value"), SqlDbType.VarChar, kvp.Value);
                }

                sb.AppendLine(string.Concat("WHERE [", campaignInfo.FieldId, "] = @campaingId"));
                rq.AddInputParameter("@campaingId", SqlDbType.Int, _iMailingId);

                string strSql = sb.ToString();
                rq.SetQuery(strSql);


                if (_dal.IsOpen)
                    dalWasOpened = true;
                else
                    _dal.OpenDatabase();

                string error;
                _dal.ExecuteNonQuery(rq, out error);

                if (error.Length > 0)
                    throw new Exception(error);
            }
            catch (Exception ex)
            {
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, string.Concat("eMailing::UpdateCampaignOtherFields() :: ", ex.Message));
            }
            finally
            {
                if (!dalWasOpened)
                    _dal.CloseDatabase();
            }
        }

        /// <summary>
        /// Crée ou met à jour la fiche Étape
        /// </summary>
        private void InsertOrUpdateEventStep()
        {
            InitEventStepDescIdFromParams();

            if (this._eventStepDescId != 0)
            {
                if (this._eventStep == null)
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId);

                this._eventStep.CampaignId = this._iMailingId;
                this._eventStep.Type = this.SendeType == MAILINGSENDTYPE.EUDONET_SMS ? EventStepType.SMSING_SENDING : EventStepType.EMAILING_SENDING;

                if (this._params["immediateSending"] == "1" && this._params["recurrentSending"] == "0")
                {
                    this._eventStep.Date = DateTime.Now;
                    this._eventStep.ExecutionMode = EventStepExecutionMode.INSTANT;
                }
                else if (this._params["immediateSending"] == "0" && this._params["recurrentSending"] == "0")
                {
                    this._eventStep.ExecutionMode = EventStepExecutionMode.DELAYED;
                }
                else if (this._params["immediateSending"] == "0" && this._params["recurrentSending"] == "1")
                {
                    this._eventStep.ExecutionMode = EventStepExecutionMode.RECURRENT;
                }
                else
                    this._eventStep.ExecutionMode = EventStepExecutionMode.UNDEFINED;

                this._eventStep.RecipientsTabDescId = this._iTab;
                this._eventStep.ParentTabFileId = this._iParentTabFileId;

                int scheduleId = 0;
                int.TryParse(this._params["scheduleId"], out scheduleId);
                this._eventStep.ScheduleId = scheduleId;

                int recipientsFilterId = 0;
                int.TryParse(this._params["recipientsFilterId"], out recipientsFilterId);
                this._eventStep.RecipientsFilterId = recipientsFilterId;

                if (!String.IsNullOrEmpty(this._params["description"]))
                    this._eventStep.Label = this._params["description"];
                else if (!String.IsNullOrEmpty(this._params["libelle"]))
                    this._eventStep.Label = this._params["libelle"];
                else
                    this._eventStep.Label = eResApp.GetRes(_ePref, 8743);

                try
                {
                    this._eventStep.InsertOrUpdateEventStep();
                }
                catch (Exception ex)
                {
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, string.Concat("eMailing::InsertOrUpdateEventStep() :: ", ex.Message));
                }

                if (this._eventStep.FileId != 0)
                {
                    this._eventStepFileId = this._eventStep.FileId;
                    this._params["eventStepFileId"] = this._eventStepFileId.ToString();
                    UpdateCampaignEventStepLink();
                    //on met à jour le status par défaut
                    this._eventStep.SetStatus(EventStepStatus.DEFAULT);
                }
            }
        }

        /// <summary>
        /// Mets à jour les champs de la campagne pointant vers l'Étapes
        /// </summary>
        private void UpdateCampaignEventStepLink()
        {
            List<int> listDescId = GetCustomLinkDescIdListFromTab(this._eventStepDescId);

            if (listDescId.Count > 0)
            {
                Engine.Engine eng = eModelTools.GetEngine(_ePref, (int)TableType.CAMPAIGN, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = this._iMailingId;

                foreach (int nDescId in listDescId)
                {
                    eng.AddNewValue(nDescId, this._eventStepFileId.ToString());
                }


                eng.EngineProcess(new StrategyCruSimpleUpdateCampaign());

                EngineResult engResult = eng.Result;

                if (engResult == null)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, "eMailing::UpdateCampaignEventStepLink()::engResult == null");

                if (!engResult.Success)
                {
                    if (engResult.Error != null)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, engResult.Error.DebugMsg, engResult.Error.Msg);
                    else
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_INSERT, "eMailing::UpdateCampaignEventStepLink()::engResult.Error == null");
                }
            }
        }

        /// <summary>
        /// Supprime la fiche Étape
        /// </summary>
        private void DeleteEventStep()
        {
            InitEventStepDescIdFromParams();
            if (this._eventStepDescId != 0 && this._eventStepFileId != 0)
            {
                if (this._eventStep == null)
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
                this._eventStep.Delete();
            }
        }

        private void InitEventStepDescIdFromParams()
        {
            //Lors du 1er enregistrement ce paramètre est à 0, donc on l'initialise depuis les paramètres
            if (this._eventStepDescId == 0)
                int.TryParse(this._params["eventStepDescId"], out this._eventStepDescId);
        }


        /// <summary>
        /// Rattachement Campagne - Evènement - A la création d'une campagne depuis un ++ si un champ de liaison est présent sur la campagne, on le pré-remplit avec l'id de l'événement
        /// Demande #31168 - [XRM] - Rattachement Campagne - Evènement - A la création d'une campagne depuis un ++ si un champ de liaison est présent sur la campagne, on le pré-remplit avec l'id de l'événement
        /// </summary>
        /// <param name="nTabIdLink">TabId de liaison recherché</param>
        /// <returns>Liste des DescId dont le champ de liaison est celui de la table passée en paramètre.</returns>
        private List<int> GetCustomLinkDescIdListFromTab(int nTabIdLink)
        {
            List<int> listTabId = new List<int>();
            try
            {
                string sError = string.Empty;
                OpenDal();
                listTabId = eCampaign.GetCustomLinkDescIdListFromTab(_dal, nTabIdLink);
            }
            catch (Exception ex)
            {
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_GET_LINK_FIELD, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }
            finally
            {
                CloseDal();
            }

            return listTabId;
        }


        /// <summary>
        /// Insere des liaisons entre les destinataires et  la campagne en cours dans la table CampaignSelection
        /// </summary>
        private void InsertIntoCampaignSelection()
        {
            string err = string.Empty;
            try
            {
                int tab = this._iTab;
                // #61893 - Plus nécessaire / sph : réactivé pour l'instant - provoque des bugs sur l'emailing mode liste pp #63449/63474
                // #59955
                if (_mailingType == TypeMailing.MAILING_FROM_LIST)
                {
                    if (_iRecipientTab == TableType.ADR.GetHashCode())
                        tab = _iRecipientTab;
                }

                if (!eCampaignSelection.InsertIntoCampaignSelection(this._ePref, tab, this._iRecipientTab, this._iMailingId, BuildRecipientQuery(), out err))
                    throw new Exception(err);
            }
            catch (Exception ex)
            {
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SELECTION_INSERT, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
            }
        }

        /// <summary>
        /// Parse le contenu de corps de mail et enregistre les annexes dans la table PJ 
        /// </summary>
        /// <returns></returns>
        private void UpdatePJLinks()
        {
            ePjLinkSaver pjLinkSaver = new ePjLinkSaver(_ePref,
              (datasPath, pjName) => new ePJToAddLite()
              {
                  FileId = Id,
                  FileType = ePJTraitementsLite.GetUserFriendlyFileType(pjName),
                  Description = string.Empty,
                  Size = (int)new FileInfo(string.Concat(datasPath, "\\", pjName)).Length,
                  OverWrite = false,
                  DayExpire = null,
                  Tab = (int)TableType.CAMPAIGN,
                  TypePj = (int)PjType.CAMPAIGN,
                  SaveAs = pjName,
                  Label = pjName
              });

            if (_bodyAnalyse.content != null && _bodyAnalyse.linksData != null && !pjLinkSaver.Save(_bodyAnalyse, (int)TableType.CAMPAIGN, this._iMailingId, PjType.CAMPAIGN))
                throw new eMailingException(pjLinkSaver.ErrorCode, userMessage: pjLinkSaver.UserMessage);
        }

        /// <summary>
        /// Met à jour le corps de la campagne avec les liens sécurisés
        /// </summary>
        private void UpdateCampaignBody()
        {
            if (this._iMailingId <= 0)
                return;

            Engine.Engine eng = eModelTools.GetEngine(_ePref, (int)TableType.CAMPAIGN, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = this._iMailingId;

            // Corps de la campagne mail serialisé
            eng.AddBinaryValue(CampaignField.BODY.GetHashCode(), SerializerTools.ByteSerialize(_bodyAnalyse));


            eng.EngineProcess(new StrategyCruSimple());

            EngineResult engResult = eng.Result;

            if (this._iMailingId == 0)
            {
                if (engResult == null)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_UPDATE_BODY, "eMailing::UpdateCampaignObjetBody() :: engResult == null");

                if (!engResult.Success)
                {
                    if (engResult.Error != null)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_UPDATE_BODY, engResult.Error.DebugMsg, engResult.Error.Msg);
                    else
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_UPDATE_BODY, "eMailing::UpdateCampaignObjetBody() :: engResult.Error = null");
                }
            }
        }

        /// <summary>
        /// Construit la requete de sélection des destinataires en tenant compte des filtres, fiches marquées, ....
        /// </summary>
        /// <returns></returns>
        private string BuildRecipientQuery()
        {
            if (_sBuildRecipientQuery.Length > 0)
                return _sBuildRecipientQuery;

            bool bFilterActive = this._params["activeAdress"].Equals("1");
            bool bFilterMain = this._params["mainAdress"].Equals("1");
            int nMailDescId;
            int.TryParse(this._params["mailFieldDescId"], out nMailDescId);
            int nMailTabFrom = eLibTools.GetTabFromDescId(nMailDescId);
            //Fiches marquées
            bool bMarkedFiles = false;
            MarkedFilesSelection ms = null;
            _ePref.Context.MarkedFiles.TryGetValue(this._iTab, out ms);

            //Si on a des fiches marquées affichées avec le reste 
            //Ou on affiche que les fiche marquées via le menu action 
            if (this._params["markedFiles"].Equals("1") || (ms != null && ms.Enabled))
                bMarkedFiles = ms != null && ms.NbFiles > 0;

            int nFilterId = 0;
            if (_eStatus == CampaignStatus.MAIL_RECURRENT)
            {
                //Filtre choisie pour la récurence
                MAILINGQUERYMODE queryMode = MAILINGQUERYMODE.NORMAL;
                if (_params.ContainsKey("RequestMode")
                    && Enum.TryParse<MAILINGQUERYMODE>(_params["RequestMode"], out queryMode)
                    && queryMode == MAILINGQUERYMODE.RECURRENT_FILTER)
                    int.TryParse(this._params["recipientsFilterId"], out nFilterId);
            }
            else
            {
                //Filtre en cours
                FilterSel FilterSel = null;
                _ePref.Context.Filters.TryGetValue(_iTab, out FilterSel);
                nFilterId = (FilterSel != null && FilterSel.FilterSelId > 0) ? FilterSel.FilterSelId : 0;
            }

            //Requete du nombre de fiche en cours
            string selectQuery = string.Empty;

            List<WhereCustom> whereCustoms = new List<WhereCustom>();
            if (_eStatus == CampaignStatus.MAIL_WRKFLW_READY)
            {
                WhereCustom wcTrackingStep = new WhereCustom("#SUBQSTEP", Operator.OP_EXISTS,
                    $@" ( SELECT 1 FROM WORKFLOWTRACKING WT WHERE  WT.WFTrFileId = [{_iTab}].tplid AND WT.WFTrDescId = {_iTab}  AND  WFTRTag = @HASH  ) ", InterOperator.OP_AND);
                wcTrackingStep.IsSubQuery = true;
                whereCustoms.Add(wcTrackingStep);

            }

            EudoQuery.EudoQuery query;

            if (_mailingType == TypeMailing.MAILING_FROM_BKM 
                || _mailingType == TypeMailing.SMS_MAILING_FROM_BKM 
                || _mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION
                || _eStatus == CampaignStatus.MAIL_RECURRENT)
            {
                query = eLibTools.GetEudoQuery(this._ePref, _iTab, ViewQuery.TREATMENT_TPL);   //TABLE ++/cible étendue
                query.SetParentDescid = this._iParentTab;
                query.SetParentFileId = this._iParentTabFileId;

             
                //Pas de filtre pour la requête de sélection des destinataires des campagnes récurrentes
                if (_eStatus == CampaignStatus.MAIL_RECURRENT)
                    query.AddParam("nocurrentfilter", "1");

                if (whereCustoms.Count > 0)
                    query.AddCustomFilter(new WhereCustom(whereCustoms));

            }
            else
            {
                query = eLibTools.GetEudoQuery(this._ePref, _iTab, ViewQuery.TREATMENT);

                if (nMailTabFrom == EudoQuery.TableType.ADR.GetHashCode())
                    query.SetCountTabDescId = EudoQuery.TableType.ADR.GetHashCode();

            }

            if (query.GetError.Length > 0)
                throw (new Exception(string.Concat("EudoQueryTool.init => ", Environment.NewLine, query.GetError)));//TODO

            try
            {
                query.SetFilterId = nFilterId;
                if (_eStatus != CampaignStatus.MAIL_RECURRENT)
                    query.SetDisplayMarkedFile = bMarkedFiles;

                //Filtre suplémentaire Active/Principale - Si le champ de mailing est sur adresse 
                if (nMailTabFrom == EudoQuery.TableType.ADR.GetHashCode())
                {

                    List<WhereCustom> list = new List<WhereCustom>();

                    if (bFilterMain)
                        list.Add(new WhereCustom(AdrField.PRINCIPALE.GetHashCode().ToString(), Operator.OP_IS_TRUE, ""));

                    if (bFilterActive)
                        list.Add(new WhereCustom(AdrField.ACTIVE.GetHashCode().ToString(), Operator.OP_IS_TRUE, ""));

                    if (list.Count > 0)
                        query.AddCustomFilter(new WhereCustom(list));

                }

                if (query.GetError.Length > 0)
                    throw (new Exception(string.Concat("QueryManager.AfterInit => ", Environment.NewLine, query.GetError)));//TODO

                //CHARGEMENT
                query.LoadRequest();
                if (query.GetError.Length > 0)
                    throw (new Exception(string.Concat("QueryManager.LoadRequest => ", Environment.NewLine, query.GetError)));//TODO

                //GENERATION
                query.BuildRequest();
                if (query.GetError.Length > 0)
                    throw (new Exception(string.Concat("QueryManager.BuildRequest => ", Environment.NewLine, query.GetError)));//TODO

                selectQuery = query.EqListIdQuery;

                if (query.GetError.Length > 0)
                    throw (new Exception(string.Concat("QueryManager.REQUETES => ", Environment.NewLine, query.GetError)));//TODO


            }
            finally
            {
                _sBuildRecipientQuery = selectQuery;
                query.CloseQuery();
                query = null;
            }
            return selectQuery;
        }

        /// <summary>
        /// Supprime la ligne dans campaign si elle est inserée
        /// </summary>
        private void RollBack()
        {
            if (this._iMailingId > 0)
                Delete();
            if (this._eventStepFileId > 0)
                DeleteEventStep();
        }

        /// <summary>
        /// Mise a jour la campagne en cours
        /// </summary>
        private void Update()
        {
            InsertOrUpdateCampaign();
            UpdatePJLinks();
            UpdateCampaignBody();
        }

        /// <summary>
        /// Lance l'envoi d'emailing 
        /// </summary>
        private void SaveAndSend()
        {
            //on sauvegrade avant l'envoi
            Save();

            //si le mail type est le workflow, pas d'envoie
            if (this.MailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
                return;

            //si depuis eCircle on vérifie le crédit
            if (this._eSendType == MAILINGSENDTYPE.ECIRCLE)
                CheckCredit();

            try
            {
                //si c'est un envoi récurrent, on ajoute la plannification au Scheduler d'EudoProcess
                if (this._eStatus == CampaignStatus.MAIL_RECURRENT)
                    ProcessScheduling();
                else
                    //Envoi est en cours            
                    ProcessSending();
            }
            catch (Exception e)
            {
                //En cas d'erreur, on passe la camapgne en status "Erreur"
                Engine.Engine eng = eModelTools.GetEngine(_ePref, (int)TableType.CAMPAIGN, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = this._iMailingId;
                eng.AddNewValue(CampaignField.STATUS.GetHashCode(), ((int)CampaignStatus.MAIL_ERROR).ToString());
                eng.EngineProcess(new StrategyCruSimpleUpdateCampaign());


                throw;
            }
        }

        /// <summary>
        /// Vérifie si on a de crédit
        /// </summary>
        private void CheckCredit()
        {
            //TODO à elever -- Pour les tests
            //GCH : Tant que l'ADV n'a pas été réalisé on force le fait d'avoir du crédit.
            if (true)
            {

            }
            else
                throw new eMailingException(eErrorCode.OUT_OF_CREDIT);
        }

        /// <summary>
        /// Envoi immediat de l'emailing
        /// </summary>
        private void ProcessSending()
        {
            eMailingCall mailingCall = new eMailingCall();
            mailingCall.UserId = _ePref.User.UserId;
            mailingCall.Lang = _ePref.Lang;
            mailingCall.PrefSQL = _ePref.GetNewPrefSql();
            mailingCall.CampaignId = _iMailingId;

            mailingCall.AppExternalUrl = this._ePref.AppExternalUrl;

            mailingCall.SecurityGroup = this._ePref.GroupMode.GetHashCode();

            mailingCall.UID = this._ePref.DatabaseUid;
            //GCH 20140211 : on ne doit passer au WCFque le chemin des datas commun à chaques bases (ex:d:\datas et surtout pas d:\datas\DEVTEST_C2...)
            mailingCall.DatasPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT);


            if (_eventStep != null)
            {
                mailingCall.MarketStepInfos = new eMarketStepInfos()
                {
                    Tab = _eventStep.Tab,
                    FileId = _eventStep.FileId

                };
            }

            // GMA 20140121 : récupération du chemin physique pour les DATAS si renseigné dans le web.config afin de pouvoir gérer le multi serveur. 
            // Si le web.config est dépourvu de cette information, on conserve la valeur de sDatasPath affectée plus haut.
            try
            {
                string sDatasPathAppSettings = ConfigurationManager.AppSettings["DatasRootFolder"];
                if (!string.IsNullOrWhiteSpace(sDatasPathAppSettings))
                {
                    mailingCall.DatasPath = string.Concat(sDatasPathAppSettings);
                }
            }
            catch { }



            //Appel de WCF avec l'objet contenant toutes les infos de la campagne 
            CallWCF(mailingCall);

            if (_strErrorMessage.Length > 0)
                throw new eMailingException(eErrorCode.ERROR_SENDING_WCF, _strErrorMessage);
        }

        Exception _innerException;

        /// <summary>
        /// Envoi les infos de la campagne au WCF 
        /// </summary>
        /// <param name="mailingCall">L'objet à envoyer</param>
        private void CallWCF(eMailingCall mailingCall)
        {
            try
            {
                eWCFTools.WCFEudoProcessCaller<IEudoMailingWCF, int>(
                    ConfigurationManager.AppSettings.Get("EudoMailingURL"), obj => obj.RunMailing(mailingCall, out _strErrorMessage));
            }
            catch (EndpointNotFoundException ExWS)
            {
                _innerException = new EudoException(ExWS.Message, "Le service Eudoprocess est indisponible, merci de contacter le support.", ExWS);
                _strErrorMessage = string.Concat(eResApp.GetRes(_ePref, 6660), ", ", eResApp.GetRes(_ePref, 6565), " : ", Environment.NewLine, ExWS.ToString());
            }
            catch (Exception ex)
            {
                _innerException = new EudoException(ex.Message, "Une erreur est survenue lors de l'appel à Eudoprocess, merci de contacter le support.", ex);
                _strErrorMessage = ex.ToString();
            }
        }

        /// <summary>
        /// Création de la plannification dans le scheduler d'EudoProcess
        /// </summary>
        private void ProcessScheduling()
        {
            if (this._eventStepDescId != 0 && this._eventStepFileId != 0)
            {
                if (this._eventStep == null)
                {
                    this._eventStep = new eEventStepXRM(_ePref, this._eventStepDescId, this._eventStepFileId);
                    string error;
                    this._eventStep.LoadEventStepFile(out error);
                    if (error.Length > 0)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_LOAD_EVENTSTEP, error);
                }

                try
                {
                    this._eventStep.AddOrUpdateSchedule();
                }
                catch (Exception ex)
                {
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_UPDATE_EVENTSTEP, ex.Message);
                }
            }
        }

        /// <summary>
        /// Lance l'envoi d'emailing de test depuis WCF
        /// </summary>
        private void SaveAndSendTest()
        {
            InsertOrUpdateCampaign(true);
            InsertIntoCampaignSelection();
            UpdatePJLinks();
            UpdateCampaignBody();
            SendTest();
        }
        /// <summary>
        /// Envoi du mail de test
        /// </summary>
        private void SendTest()
        {
            //Création de l'enregistrement sans l'envoyer
            int nEmailId = AddNewMailTest();
            //MAJ de l'enregistrement en l'envoyant
            UpdateToSendMailTest(nEmailId);

            //on lance un test Pour mail tester
            if (eExtension.IsReadyStrict(_ePref, "QUALITYMAIL", true) && this._params["MailTestType"] != null && this._params["MailTestType"] == "1")
            {
                _EnbleSendingToMailTester = true;
                string mailTesterAdress = string.Concat(this._params["MailTesterReportId"], "@srv1.mail-tester.com");
                //Check id mail-tester @ is available

                SendToMailTester(mailTesterAdress);

                GetMailTesterStatus(this._params["MailTesterReportId"], out this._ErrorOnMailTester, out _ErrorMailTester);

                eCampaign.UpdateMailTesterInfo(_ePref, this._ErrorOnMailTester ? "error" : this._params["MailTesterReportId"], this._iMailingId);

            }
        }

        /// <summary>
        /// On lance un mail tester sans bloquer le process
        /// </summary>
        /// <param name="mailTesterAdress"></param>
        /// <returns></returns>
        public void SendToMailTester(string mailTesterAdress)
        {
            this._params["recipientstest"] = mailTesterAdress;
            //Création de l'enregistrement sans l'envoyer
            int nEmailId = AddNewMailTest();
            //MAJ de l'enregistrement en l'envoyant
            if (UpdateToSendMailTest(nEmailId))
            {

                //Incrémente le nb de mail testé si mail ok
                try
                {
                    OpenDal();
                    int nTabMailTabId = eLibTools.GetNum(this._params["mailTabDescId"]);

                    if (nTabMailTabId > 0)
                    {
                        string sMailTabName = "TEMPLATE_" + (nTabMailTabId / 100 - 10).ToString();

                        StatsUpdate st = new StatsUpdate(_dal, this._iMailingId, _ePref.UserId, sMailTabName);
                        st.UpdateMailTesterNB();
                    }

                }
                finally
                {
                    CloseDal();
                }
            }
        }
        /// <summary>
        /// Ajout de l'email avec les information d'e-mail de test basique sans l'objet fusionné
        /// </summary>
        /// <returns></returns>
        private int AddNewMailTest()
        {
            try
            {
                int nTabMailTabId = eLibTools.GetNum(this._params["mailTabDescId"]);
                if (nTabMailTabId == 0)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "AddNewMail => Id de la table de mail manquant.");

                OpenDal();

                Engine.Engine eng = eModelTools.GetEngine(_ePref, nTabMailTabId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.AddParam("mailNotSend", "1");   //On n'envoit pas ! 

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_TO.GetHashCode(), this._params["recipientstest"], true);   //destinataires 
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_REPLY_TO.GetHashCode(), this._params["replyTo"], true);  //Reply to
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_DISPLAY_NAME.GetHashCode(), this._params["displayName"], true); //From Name

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_FROM.GetHashCode(), this._params["sender"], true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_ISHTML.GetHashCode(), this._params["HtmlFormat"], true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode(), MailSendType.MAIL.GetHashCode().ToString(), true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_STATUS.GetHashCode(), EmailStatus.MAIL_SENT.GetHashCode().ToString(), true);


                eng.EngineProcess(new StrategyCruSimple());

                EngineResult engResult = eng.Result;
                int nEmailId = -1;
                if (engResult == null || engResult.NewRecord == null || engResult.Error != null)
                {
                    if (engResult == null)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "Création de la fiche email - engResult vide");

                    if (engResult.NewRecord == null)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "Création de la fiche email - engResult.NewRecord vide");

                    string errorInfos = "";
                    if (engResult.Error == null)
                        errorInfos = "Erreur inconnue";
                    else
                    {
                        if (engResult.Error.DebugMsg.Length != 0)
                            errorInfos = engResult.Error.DebugMsg;
                        else
                            errorInfos = string.Concat(engResult.Error.Msg, Environment.NewLine, engResult.Error.Detail);
                    }

                    if (nEmailId > 0)
                    {
                        //Fiche Créée mais avec des erreurs
                        //Donc on peut continuer le traitement mais on envoi un feedback quand même
                        eFeedbackXrm.LaunchFeedbackXrm(new eErrorContainer() { AppendDebug = string.Concat("AddNewMail - mail de test créé mais avec des erreurs : ", errorInfos) }, _ePref);
                    }
                    else
                    {
                        //NON OK : Fiche non créée
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("AddNewMail => ", errorInfos));
                    }
                }

                if (engResult.NewRecord.FilesId == null || engResult.NewRecord.FilesId.Count == 0)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "Création de la fiche email sans id de fiche en retour");

                nEmailId = engResult.NewRecord.FilesId[0];

                return nEmailId;
            }
            catch (Exception ex)
            {
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("AddNewMail => ", eLibTools.GetExceptionMsg(ex)));
            }
            finally
            {
                CloseDal();
            }
        }

        /// <summary>
        /// Retourne la liste des champs de fusions du 1er contact
        /// </summary>
        /// <returns>Liste des champs de fusions (descid+Valeurs)</returns>
        private Dictionary<int, string> GetMergeFieldFromFirst()
        {
            Dictionary<int, string> dicMergeFields = new Dictionary<int, string>();

            eCampaign campaign = new eCampaign(_iMailingId);
            string sError = string.Empty;
            if (!campaign.Load(_dal, _ePref, out sError))
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst - campaign.Load => ", sError));
            eCampaignSelection campSelection = null;
            try
            {
                campSelection = new eCampaignSelection(_ePref, campaign, true);
                if (!campSelection.InitQuery(out sError))
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst - campSelection.InitQuery => ", sError));
                if (!campSelection.InitSelection(out sError))
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst - campSelection.InitSelection => ", sError));
                if (!campSelection.LoadQuery(out sError))
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst - campSelection.LoadQuery => ", sError));
                DataTableReaderTuned dtr = null;
                try
                {
                    dtr = _dal.Execute(new RqParam(campSelection.SQLEudoQuery), out sError);
                    if (sError.Length > 0)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst - campSelection.SQLEudoQuery => ", sError));
                    if (dtr.Read())
                    {
                        // TODOHLA - Faire un datafillergeneric car pas toujours un eRecord !!

                        try
                        {
                            eRecordLite row = new eRecord();
                            row.FillComplemantaryInfos(campSelection.EQ, dtr, _ePref);

                            eFieldRecord eFldRow = null;

                            foreach (Field currentFld in campSelection.ListFldMailing)
                            {
                                eFldRow = eDataFillerTools.GetFieldRecord(_ePref, campSelection.EQ, dtr, currentFld, row, _ePref.User);
                                // 41422 : Si le descid est déjà présent dans dicMergeFields, on ne le rajoute pas
                                if (!dicMergeFields.ContainsKey(currentFld.Descid))
                                {
                                    dicMergeFields.Add(currentFld.Descid, eFldRow.DisplayValue);
                                }

                            }
                        }
                        catch (Exception e)
                        {
                            throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("GetMergeFieldFromFirst => ", eLibTools.GetExceptionMsg(e)));
                        }


                    }

                }

                finally
                {
                    if (dtr != null)
                        dtr.Dispose();
                }
            }
            finally
            {
                if (campSelection != null)
                    campSelection.CloseQuery();
            }
            return dicMergeFields;
        }
        /// <summary>
        /// On met à jour l'e-mail de test avec l'objet fusionné et les informations déjà entrée pour générer l'envoi de test.
        /// </summary>
        /// <param name="nEmailId">Id de l'e-mail précédemment créé</param>
        private bool UpdateToSendMailTest(int nEmailId)
        {
            string sBody = string.Empty, sObject = string.Empty, sPreheader = string.Empty;

            try
            {
                OpenDal();
                if (nEmailId <= 0)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "UpdateToSendMail - id d'email manquant.");
                int nTabMailTabId = eLibTools.GetNum(this._params["mailTabDescId"]);

                if (nTabMailTabId == 0)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, "UpdateToSendMail - id de table d'email manquante.");

                Engine.Engine eng = eModelTools.GetEngine(_ePref, nTabMailTabId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = nEmailId;
                eng.AddParam("mailNotSend", "0");   //On envoie ! 

                //On ajoute le test mail type pour mail tester si l'extension est valide
                if (_EnbleSendingToMailTester && eExtension.IsReadyStrict(_ePref, "QUALITYMAIL", true) && this._params["MailTestType"] != null && this._params["MailTestType"] == "1")
                    eng.AddParam("mailTesterToken", this._params["MailTesterToken"]);

                // Nom apparent
                if (this._params.ContainsKey("displayName"))
                    eng.AddParam("mailDN", this._params["displayName"]);

                eng.ModeDebug = false;

                #region SUJET, TEXTE D'APERÇU et CORPS
                eAnalyzerInfos subject = eMergeTools.AnalyzeObject(this._params["subject"]);
                //SHA : tâche #1 941
                eAnalyzerInfos preheader = eMergeTools.AnalyzePreheader(this._params["preheader"]);
                eAnalyzerInfos body = eMergeTools.AnalyzeBody(this._params["body"]);

                EudonetMailingBuildParam mbp = new EudonetMailingBuildParam();
                mbp.Uid = _ePref.DatabaseUid;
                mbp.AppExternalUrl = _ePref.AppExternalUrl;
                mbp.MailId = nEmailId;

                mbp.MergeValues = GetMergeFieldFromFirst();

                mbp.MailTabDescId = nTabMailTabId;
                //mbp.EmbeddedImage = _bEmbeddedImage;    //Doit-être géré au niveau de l'envoi des mails unitaires dans engine
                //if (mbp.MergeValues.Count != 0)
                //{
                //KJE, Tâche #2 552: on construit l'objet du mail à partir des champs de fusions, ensuite on tronque si la taille dépasse la taille max                
                sObject = eLibTools.GetTextFromHtmlString(eMergeTools.GetObjectMerge(subject, mbp));
                if (!string.IsNullOrEmpty(sObject))
                    sObject = eLibTools.VerifyAndTruncateMailSubject(sObject);

                //SHA : tâche #1 941
                string Preheader = eMergeTools.GetPreheaderMerge(preheader);
                sBody = eMergeTools.GetBodyMerge_MailTest(body, mbp);
                if (_MailBodyNcharPerLine)
                    sBody = eLibTools.GetMailBodyAbout70CharPerLine(sBody);
                //}
                //else
                //{
                //    foreach (Int32 currentNB in body.mergeData.ListFields)
                //        sBody =string.Concat(sBody, currentNB);

                //    sObject = subject.mergeData.ToString();
                //    sPreheader = preheader.mergeData.ToString();
                //}
                string sStyleBody = "";
                if (this._params.ContainsKey("bodyCss"))
                {
                    string sBodyCss = this._params["bodyCss"];
                    if (!string.IsNullOrEmpty(sBodyCss))
                        sStyleBody = string.Concat("<style>", sBodyCss, "</style>");
                }

                string sFullBody = eMailTools.AddHtmlTag(
                  sStyleBody
                , string.Empty
                , sBody);


                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_OBJECT.GetHashCode(), sObject);
                //SHA : tâche #1 941
                eng.AddNewValue(nTabMailTabId + (int)MailField.DESCID_MAIL_PREHEADER, sPreheader);

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_HTML.GetHashCode(), sFullBody);
                #endregion

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_SIZE.GetHashCode(), string.Empty, true);    //TODO MAIL SIZE ?

                // 41422 : on détache le mail de test de la campagne, pour éviter les problèmes de champs de fusion et pour ne pas fausser les stats de la campagne
                // eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_CAMPAIGNID.GetHashCode(), this._iMailingId.ToString(), true);

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_TO.GetHashCode(), this._params["recipientstest"], true);   //From
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_REPLY_TO.GetHashCode(), this._params["replyTo"], true);  //Reply to
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_DISPLAY_NAME.GetHashCode(), this._params["displayName"], true); //From Name

                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_FROM.GetHashCode(), this._params["sender"], true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_ISHTML.GetHashCode(), this._params["HtmlFormat"], true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode(), MailSendType.MAIL.GetHashCode().ToString(), true);
                eng.AddNewValue(nTabMailTabId + MailField.DESCID_MAIL_STATUS.GetHashCode(), EmailStatus.MAIL_SENT.GetHashCode().ToString(), true);



                // 41422 : on utilise StrategyCRUSimpleTestMail afin d'outrepasser les droits de modif sur la table Email    
                eng.EngineProcess(new StrategyCRUSimpleTestMail());


                EngineResult engResult = eng.Result;
                if ((!engResult.Success) && engResult.Error != null && engResult.Error.DebugMsg.Length > 0)
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("UpdateToSendMail => ", engResult.Error.DebugMsg));

                return engResult.Success;
            }
            catch (eMailingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_SENDMAILTEST_FATALERROR, string.Concat("UpdateToSendMail => ", eLibTools.GetExceptionMsg(ex)));
            }
            finally
            {
                CloseDal();
            }


        }

        /// <summary>
        /// Supprime l'emailing en cours (CAMPAIGN + CAMPAIGNSELECTION)
        /// </summary>
        private void Delete()
        {
            if (this._iMailingId > 0)
            {
                // HLA - [RÉGRESSION 10.408] -  LES LIENS SÉCURISÉS DANS LES E-MAILINGS - #68831
                string deleteQueries = new StringBuilder()
                    .Append(" DELETE FROM [PJ] WHERE [FILE] = (SELECT [FILE] FROM [DESC] WHERE DescId = @CampaignTabId) AND [FileId] = @MailingId; ")
                    .Append(" DELETE FROM [CampaignSelection] WHERE [CampaignSelection].[CampaignId] = @MailingId; ")
                    .Append(" DELETE FROM [CAMPAIGN] WHERE [CAMPAIGN].[CampaignId] = @MailingId ")
                    .ToString();
                string deleteError = string.Empty;

                try
                {
                    OpenDal();

                    RqParam deleteRequest = new RqParam(deleteQueries);
                    deleteRequest.AddInputParameter("@CampaignTabId", SqlDbType.Int, (int)TableType.CAMPAIGN);
                    deleteRequest.AddInputParameter("@MailingId", SqlDbType.Int, this._iMailingId);

                    _dal.ExecuteNonQuery(deleteRequest, out deleteError);

                    if (deleteError.Length > 0)
                        throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_DELETE, deleteError);

                    //Ré-initilisation de l'id
                    this._iMailingId = 0;
                }
                catch (Exception ex)
                {
                    throw new eMailingException(eErrorCode.ERROR_CAMPAIGN_DELETE, string.Concat(ex.Message, Environment.NewLine, ex.StackTrace));
                }
                finally
                {
                    CloseDal();
                }
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
        /// retourne le statut d'un mail de tester après call ApiTester
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="isErrorMailTester"></param>
        /// <param name="mailTesterOrrorDetails"></param>
        private void GetMailTesterStatus(string reportId, out bool isErrorMailTester, out string mailTesterOrrorDetails)
        {
            isErrorMailTester = false;
            mailTesterOrrorDetails = string.Empty;
            HttpClient Client = new HttpClient();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                string campaignUrl = string.Concat("https://www.mail-tester.com/", reportId, "&format=json");
                using (var response = Client.GetAsync(campaignUrl).Result)
                {
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        var def = new { status = false, title = string.Empty, access = false };
                        var t = JsonConvert.DeserializeAnonymousType(response.Content.ReadAsStringAsync().Result, def);
                        //si le title contient Mail not found ça va peux dire que c'est une erreur pourtant le statut est à false
                        //l'analyse de mail-tester prend un certain temps
                        isErrorMailTester = !t.access || (!t.status && t.title != "Mail not found. Please wait a few seconds and try again.");
                        mailTesterOrrorDetails = isErrorMailTester ? t.title : string.Empty;
                    }
                    else
                    {
                        isErrorMailTester = false;
                        mailTesterOrrorDetails = "Error when calling mail tester API. Mail tester is not available";
                    }
                }
            }
            catch (Exception e)
            {
                isErrorMailTester = false;
                mailTesterOrrorDetails = "Error when calling mail tester API. Mail tester is not available";
            }
        }
    }




    /// <summary>
    /// L'envoi de sms via activmail : mode mailToSms
    /// </summary>
    public class eSmsing : eMailing
    {
        /// <summary>Liste des paramètres d'envoi de sms via la plateforme activmail</summary>
        private string _phoneDisplayName = string.Empty;
        private string _mailFrom = string.Empty;
        private string _mailTo = string.Empty;



        /// <summary> Constructeurs de sms</summary>       
        public eSmsing(ePref pref, int nMailingId, TypeMailing mailingType) : base(pref, nMailingId, mailingType) { }


        public eSmsing(ePref pref, int mailingId, int nTab, eudoDAL dal, TypeMailing mailingType = TypeMailing.MAILING_FROM_LIST) : base(pref, mailingId, nTab, dal, mailingType) { }

        public eSmsing(ePref pref, int nMailingId) : base(pref, nMailingId) { }

        public eSmsing(ePref pref) : base(pref) { }

        /// <summary>
        /// Charge la config
        /// </summary>
        protected override void LoadConfigAdv()
        {
            try
            {

                IDictionary<eLibConst.CONFIGADV, string> dicConfigAdv = eLibTools.GetConfigAdvValues(this._ePref,
                    new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.SMS_CLIENT_ID,
                        eLibConst.CONFIGADV.SMS_MAIL_FROM,
                        eLibConst.CONFIGADV.SMS_MAIL_TO,
                        eLibConst.CONFIGADV.SMS_SERVER_ENABLED,
                        eLibConst.CONFIGADV.SMS_SETTINGS,
                        eLibConst.CONFIGADV.SMS_SENDTYPE
                    });


                if (dicConfigAdv[eLibConst.CONFIGADV.SMS_SERVER_ENABLED] != "1")
                    throw new eMailingException(eErrorCode.SMS_DISABLED);


                _phoneDisplayName = dicConfigAdv[eLibConst.CONFIGADV.SMS_CLIENT_ID];

                string smsSendType = dicConfigAdv[eLibConst.CONFIGADV.SMS_SENDTYPE];
                int sendTypeCode;
                if (!int.TryParse(smsSendType, out sendTypeCode) || sendTypeCode > 1 || sendTypeCode < 0)
                    throw new Exception("Type d'envoi SMS non valide");

                // Pour forcer EudoProcess à executer la strategy du sms
                // Cette valeur ne devera pas etre sauvegardée dans configAdv
                _eSendType = MAILINGSENDTYPE.EUDONET_SMS;

                // L'envoi de sms passe par eudonet
                SmsingSendType smsType = eLibTools.GetEnumFromCode<SmsingSendType>(sendTypeCode);

                // Activmail limite la longueur a 8 charctères          

                _mailFrom = dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_FROM];
                _mailTo = dicConfigAdv[eLibConst.CONFIGADV.SMS_MAIL_TO];

                if (smsType == SmsingSendType.ACTIVMAIL)
                    checkParams();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Vérifie les paramètres de sms
        /// </summary>
        private void checkParams()
        {
            if (string.IsNullOrEmpty(_phoneDisplayName) || !IsAlphanumeric(_phoneDisplayName))
                throw new eMailingException(eErrorCode.SMS_INVALID_CONFIG, "Le Client ID (Sender ID) contient des caractères non alphanumériques (CONFIGADV.SMS_CLIENT_ID) :  " + _phoneDisplayName);

            if (string.IsNullOrEmpty(_mailFrom) || !eLibTools.IsEmailAddressValid(_mailFrom))
                throw new eMailingException(eErrorCode.SMS_INVALID_CONFIG, "L'adresse e-mail de l'émetteur doit être correctement définie dans CONFIGADV.SMS_MAIL_FROM : " + _mailFrom);

            // Dans mailTo :  $phone$ sera remplacée par le numéro de tel. Pour la vérification on se contente de l'adresse sans "$"
            if (string.IsNullOrEmpty(_mailTo) || !_mailTo.Contains("$phone$") || !eLibTools.IsEmailAddressValid(_mailTo.Replace("$phone$", "phone")))
                throw new eMailingException(eErrorCode.SMS_INVALID_CONFIG, "L'adresse e-mail type du destinataire doit être correctement définie dans CONFIGADV et elle doit contenir la variable $phone$, qui sera remplacée par le numéro de téléphone réel du destinataire final, dans CONFIGADV.SMS_MAIL_TO : " + _mailTo);

        }

        private bool IsAlphanumeric(string _phoneDisplayName)
        {
            Regex regEx = new Regex("[^a-zA-Z0-9]");

            return !regEx.Match(_phoneDisplayName).Success;

        }

        /// <summary>
        /// Analyse et serialise les rubriques comme body, subject,...
        /// </summary>
        protected override void AnalyseAndSerializeFields()
        {
            _params["body"] = RemoveSMSCommandes(this._params["body"]);

            base.AnalyseAndSerializeFields();
        }

        /// <summary>
        /// Retire les commande sms
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string RemoveSMSCommandes(string text)
        {
            // Supprime les commande sms de forme : @@cmd arg1@@
            return Regex.Replace(text, @"@@[^@]+@@", string.Empty);
        }
    }
}

