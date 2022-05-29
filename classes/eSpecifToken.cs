using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;
using static Com.Eudonet.Internal.eDataFillerGeneric;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// DEVELOPPEUR	: SPH
    /// DATE			: 10/06/2013
    /// DESCRIPTION 			: 
    /// Classe permettant de générer un token
    /// contenant les informations nécessaire pour 
    /// les spécif (v7 et xrm)
    /// </summary>
    public class eSpecifToken
    {

        #region propriétés

        private String _sError = String.Empty;        //Erreur
        private ePref _ePref;               //préférence utilisateur
        private HttpContext _Context;       //Context Http de la demande
        private Exception _eInnerException = null;          // Exception rencontrée
        private Int32 _nTab = 0;                            //table de la spéicf

        private Int32 _nFileId = 0;                         //Fileid de la spécif

        private Int32 _nParentTab = 0;                            //lancement depuis une formule

        private Int32 _nParentFileId = 0;                         //lancement depuis une formule

        private Int32 _nDescId = 0;                         //descid du champ depuis lequel la spécif a été lancée (Champ de type Page Web)

        private Int32 _nURLID = 0;                          //Id de la spécif - la table varie selon le type de spécif
        private String _sBaseSiteV7URL = String.Empty;      //Url de base la v7 de la base

        private String _sURL = String.Empty;                //URL de la spécif
        private String _sURLName = String.Empty;            // Nom de la spécif
        private String _sURLParam = String.Empty;           // Paramètres additionnels

        private eLibConst.SPECIF_OPENMODE _openMode;        //mode d'ouverture de la specif (cachée , modaldialog, nouvelle fenêtre)

        private eLibConst.SPECIF_TYPE _cSpecifType;            // Type de spécif

        private eLibConst.SPECIF_SOURCE _cSourceSpecif;        // XRM/EXTENSION..
        private bool _isStatic;
        private String _sToken = String.Empty;              // Token 
        private string _sShortToken = string.Empty;

        private Boolean _bIsInit = false;               //flag indiquant l'initialisation correcte
        private Boolean _bIsGenerated = false;          // flag indiquant la génération correcte

        #endregion


        #region accesseurs
        /// <summary>
        /// table active lors du lancement de la spécif
        /// </summary>
        public Int32 Tab
        {
            get { return _nTab; }
            set { _nTab = value; }
        }

        /// <summary>
        /// fiche active lors du lancement de la spécif
        /// </summary>
        public Int32 FileId
        {
            get { return _nFileId; }
            set { _nFileId = value; }
        }

        /// <summary>
        /// Lors du lancement de la spécif depuis une formule dans un signet, tabid du signet
        /// </summary>
        public Int32 ParentTab
        {
            get { return _nParentTab; }
            set { _nParentTab = value; }
        }

        /// <summary>
        /// Lors du lancement de la spécif depuis une formule dans un signet, fileid de la fiche surlaquelle on éxécute la formule.
        /// </summary>
        public Int32 ParentFileId
        {
            get { return _nParentFileId; }
            set { _nParentFileId = value; }
        }

        /// <summary>
        /// dans le cas d'une sspecif depuis une champ/signet de type page web, on transmet le descid
        /// </summary>
        public Int32 DescId
        {
            get { return _nDescId; }
            set { _nDescId = value; }
        }

        /// <summary>
        /// Token d'information permettant les appels aux spécifs asp/xrm
        /// </summary>
        public String Token
        {
            get { return _sToken; }
        }




        /// <summary>
        /// Token court pour les url accessible via get
        /// </summary>
        public string ShortToken
        {
            get
            {
                return _sShortToken;
            }
        }
        /// <summary>
        /// URL de base de la v7
        /// </summary>
        public String BaseSiteV7URL
        {
            get { return _sBaseSiteV7URL; }
        }


        /// <summary>
        /// Indique si une erreur est survenue
        /// </summary>
        public Boolean IsError
        {
            get
            {
                return _sError.Length > 0;
            }
        }

        /// <summary>
        /// Message d'erreur utilisateur
        /// </summary>
        public String ErrorMsg
        {
            get
            {
                return _sError;
            }

            set
            {

                _sError = value;

                //Vide les valeurs - Un token invalide / en erreur ne doit pas fournir d'information pour des questions de sécurité
                if (!string.IsNullOrEmpty(value))
                {
                    _nFileId = 0;
                    _nTab = 0;
                    _nParentFileId = 0;
                    _nParentTab = 0;
                    _ePref = null; //S'il y a une erreur, aucun epref valide ne doit être fourni
                    _sToken = "";
                    _nDescId = 0;
                }



            }
        }


        /// <summary>
        /// Retourne l'exception ayant arrêté le traitement
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return _eInnerException;
            }
        }

        /// <summary>
        /// Url de la spécif
        /// </summary>
        public String URL
        {
            get { return _sURL; }
        }

        /// <summary>
        /// Nom du lien
        /// </summary>
        public String URLName
        {
            get { return _sURLName; }
        }


        /// <summary>
        /// Parametres suplémentaires
        /// </summary>
        public String URLParam
        {
            get { return _sURLParam; }
        }

        #endregion

        #region Génération instance eSpecifToken


        /// <summary>
        /// Constructeur privé pour la classe de  génération de token
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="context">Context http de la demande - nécessiaire pour récupérer des variables du client</param>
        private eSpecifToken(ePref pref, HttpContext context = null)
        {

            //Si htttpcontextcurrent inaccessible et pas passé en param, erreur
            if (HttpContext.Current == null && context == null)
            {
                ErrorMsg = "Contexte d'appel invalide";
            }

            if (HttpContext.Current == null)
                _Context = context;
            else
                _Context = HttpContext.Current;

            _ePref = pref;

        }

        /// <summary>
        /// génère un token permettant aux specifs XRM d'atteindre la base
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="spec">Objet eSpec de la spécif</param>
        /// <param name="nTab">Table de la spécif</param>
        /// <param name="nFileId">Id de la fiche de la spécif</param>
        /// <param name="nParentTab">DescId de la table parente (mode signet)</param>
        /// <param name="nParentFileId">Id de la fiche parente (mode signet)</param>
        /// <param name="nDescId">Descid du champ déclencheur (mode champ déclencheur xrm=)</param>
        /// <returns></returns>
        public static eSpecifToken GetSpecifTokenXRM(ePref pref, eSpecif spec, Int32 nTab = 0, Int32 nFileId = 0, Int32 nParentTab = 0, Int32 nParentFileId = 0, Int32 nDescId = 0)
        {

            String sToken = String.Empty;

            eSpecifToken estToken = new eSpecifToken(pref);
            if (estToken.IsError)
                return estToken;




            //Information sur l'urlid
            Boolean bIsOk = estToken.InitTokenXRM(spec, nTab, nFileId, nParentTab, nParentFileId, nDescId)
                    && estToken.GenerateToken();



            return estToken;
        }

        /// <summary>
        /// génère un token permettant aux specifs XRM d'atteindre la base
        /// </summary>
        /// <param name="prefBase">Préférence utilisateur</param>
        /// <param name="nUserId">Utilisateur ouvrant la spécif</param>
        /// <param name="nSpecId">Id de la spécif</param>
        /// <param name="nTab">Table de la spécif</param>
        /// <param name="nFileId">Id de la fiche de la spécif</param>
        /// <param name="nParentTab">DescId de la table parente (mode signet)</param>
        /// <param name="nParentFileId">Id de la fiche parente (mode signet)</param>
        /// <param name="nDescId">Descid du champ déclencheur (mode champ déclencheur xrm=)</param>
        /// <returns></returns>
        public static eSpecifToken GetSpecifTokenXRM(ePrefSQL prefBase, Int32 nUserId, Int32 nSpecId, Int32 nTab = 0, Int32 nFileId = 0, Int32 nParentTab = 0, Int32 nParentFileId = 0, Int32 nDescId = 0)
        {
            eudoDAL eudoDal = null;
            String sLang = "LANG_00";
            try
            {
                eudoDal = eLibTools.GetEudoDAL(prefBase);

                eUserInfo userInfo;

                eudoDal.OpenDatabase();

                userInfo = new eUserInfo(nUserId, eudoDal);
                sLang = userInfo.UserLang;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (eudoDal != null)
                    eudoDal.CloseDatabase();
            }

            ePref pref = new ePref(prefBase.GetSqlInstance, prefBase.GetBaseName, prefBase.GetSqlUser, prefBase.GetSqlPassword, nUserId, sLang);
            if (!pref.LoadConfig())
                throw new Exception("Erreur de chargement");

            String sToken = String.Empty;

            eSpecif spec = eSpecif.GetSpecif(pref, nSpecId);

            eSpecifToken estToken = new eSpecifToken(pref);
            if (estToken.IsError)
                return estToken;

            //Information sur l'urlid
            Boolean bIsOk = estToken.InitTokenXRM(spec, nTab, nFileId, nParentTab, nParentFileId, nDescId)
                    && estToken.GenerateToken();

            return estToken;
        }

        /// <summary>
        /// Retourne le token d'information
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="cType">Type de spécif</param>
        /// <param name="sURL">id ou url de la spécif</param>
        /// <param name="nTab">Table depuis laquelle l'appel a été fait</param>
        /// <param name="nFileId">Id de la fiche depuis l'appel a éta fait</param>
        /// <param name="nDescId">descid du champ depuis lequel la spécif a été appelée (chamsp de type Page Web)</param>
        /// <returns>Token encodé</returns>
        public static eSpecifToken GetSpecifTokenV7(ePref pref, eLibConst.SPECIF_TYPE cType, String sURL, Int32 nTab, Int32 nFileId, Int32 nDescId = 0)
        {

            String sToken = String.Empty;

            eSpecifToken estToken = new eSpecifToken(pref);
            if (estToken.IsError)
                return estToken;

            //Information sur l'urlid
            Boolean bIsOk = estToken.InitTokenV7(cType, sURL, nTab, nFileId, nDescId)
                    && estToken.GenerateToken();
            return estToken;
        }


        #endregion

        #region méthodes internes

        /// <summary>
        /// Génère la chaine de token
        /// </summary>
        /// <returns></returns>
        private Boolean GenerateToken()
        {
            if (!_bIsInit)
            {
                ErrorMsg = "Le token n'a pas été initialisé";
                return false;
            }

            try
            {
                Dictionary<eLibConst.CONFIG_DEFAULT, String> config = _ePref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[] {
                    eLibConst.CONFIG_DEFAULT.LOGSIMULT, eLibConst.CONFIG_DEFAULT.LOGSIMULTENABLED, eLibConst.CONFIG_DEFAULT.MARKEDFILEENABLED, eLibConst.CONFIG_DEFAULT.KEYCOMMON, eLibConst.CONFIG_DEFAULT.EUDONETWORKENABLED, eLibConst.CONFIG_DEFAULT.SYNCHROENABLED, eLibConst.CONFIG_DEFAULT.DATASPATH, eLibConst.CONFIG_DEFAULT.BANNERNAME, eLibConst.CONFIG_DEFAULT.HTMLENABLED, eLibConst.CONFIG_DEFAULT.APPNAME, eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED, eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME, eLibConst.CONFIG_DEFAULT.HELPDESKCOPY, eLibConst.CONFIG_DEFAULT.ASPEMAILENABLED, eLibConst.CONFIG_DEFAULT.ASPPDFENABLED, eLibConst.CONFIG_DEFAULT.CONDITIONALSENDENABLED, eLibConst.CONFIG_DEFAULT.DISPLAYVERMSG, eLibConst.CONFIG_DEFAULT.THEME
                    ,eLibConst.CONFIG_DEFAULT.NOMINATIONLOGIN,eLibConst.CONFIG_DEFAULT.NOMINATIONPASSWORD,eLibConst.CONFIG_DEFAULT.NOMINATIONENABLED
                });

                bool bCheckLogSimult = eLibTools.GetNum(config[eLibConst.CONFIG_DEFAULT.LOGSIMULTENABLED]) == 0;
                bCheckLogSimult = bCheckLogSimult && eLibTools.GetNum(config[eLibConst.CONFIG_DEFAULT.LOGSIMULT]) == 0;

                String sLocalIp = _Context.Request.ServerVariables["LOCAL_ADDR"];

                //Liste des 
                Dictionary<String, String> dicoSession = new Dictionary<String, String>();

                Random random = new Random();
                int randomNumber = random.Next(0, 100);
                dicoSession.Add(String.Concat("xx", randomNumber), String.Concat(DateTime.Now.Ticks.ToString(), randomNumber.ToString()));

                DateTime ts = new DateTime(1970, 1, 1, 0, 0, 0);

                dicoSession.Add("XTS", (DateTime.Now - ts).TotalSeconds.ToString());

                dicoSession.Add("urlid", _nURLID.ToString());

                // Construction des variables de sessions lié à "LogDatabaseTreatment"
                dicoSession.Add("Base", _ePref.GetBaseName);
                dicoSession.Add("BaseName", _ePref.EudoBaseName);
                dicoSession.Add("baseuid", _ePref.DatabaseUid);
                dicoSession.Add("CheckLogSimult", bCheckLogSimult ? "1" : "0");
                dicoSession.Add("Client", _ePref.ClientId.ToString());
                dicoSession.Add("GroupId", _ePref.User.UserGroupId.ToString());
                dicoSession.Add("GroupLevel", _ePref.User.UserGroupLevel);
                dicoSession.Add("Platform", "WIN");
                dicoSession.Add("ReadOnly", _ePref.ReadOnly ? "1" : "0");
                dicoSession.Add("SubscriberId", _ePref.LoginId.ToString());
                dicoSession.Add("UserDisplayName", _ePref.User.UserDisplayName);
                dicoSession.Add("UserId", _ePref.User.UserId.ToString());
                dicoSession.Add("UserLevel", _ePref.User.UserLevel.ToString());
                dicoSession.Add("UserLogin", _ePref.User.UserLogin);
                dicoSession.Add("UserMail", _ePref.User.UserMail);
                dicoSession.Add("UserMailOther", _ePref.User.UserMailOther);
                dicoSession.Add("UserName", _ePref.User.UserName);
                dicoSession.Add("UserStatId", _ePref.User.UserStatLogId.ToString());
                dicoSession.Add("langue", _ePref.User.UserLang);

                //problématique. Il faudrait ne pas laisser ces informations dans le token
                // nécessaire actuellement pour le bon fonctionnement des spécifs existantes...
                dicoSession.Add("sqlinstance", _ePref.GetSqlInstance);
                dicoSession.Add("sqllogin", _ePref.GetSqlUser);
                dicoSession.Add("sqlpwd", _ePref.GetSqlPassword);

                randomNumber = random.Next(0, 500);
                dicoSession.Add(String.Concat("xx", randomNumber), String.Concat(randomNumber, _Context.Timestamp.Ticks.ToString()));

                // Construction des variables de sessions lié à "Config.asp"
                dicoSession.Add("AlertMode", _ePref.GetConfig(eLibConst.PREF_CONFIG.ALERTMODE));
                dicoSession.Add("AppName", config[eLibConst.CONFIG_DEFAULT.APPNAME]);
                dicoSession.Add("ApplicationLoaded", "1");
                dicoSession.Add("AspEmail", config[eLibConst.CONFIG_DEFAULT.ASPEMAILENABLED]);
                dicoSession.Add("AspPDF", config[eLibConst.CONFIG_DEFAULT.ASPPDFENABLED]);
                dicoSession.Add("BannerName", config[eLibConst.CONFIG_DEFAULT.BANNERNAME]);
                dicoSession.Add("BgColor", "#B0C4DE");  //Par défaut. Le chargement effectif des variable de theme se fait coté asp
                dicoSession.Add("ConditionalSend", config[eLibConst.CONFIG_DEFAULT.CONDITIONALSENDENABLED]);

                dicoSession.Add("DatasPath", eLibTools.GetDatasDir(_ePref.GetBaseName));
                // MCR 39438 : ajout d une nouvelle propriété FullPhysicalDatasPath contenant ce chemin complet
                // et retourner le chemin physique complet du dossier, incluant le lecteur et les dossiers parents  (D:\eudonet\datas\XXXXA28......).
                dicoSession.Add("FullDatasPath", eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT, _ePref.GetBaseName));


                dicoSession.Add("DisplayVerMsg", config[eLibConst.CONFIG_DEFAULT.DISPLAYVERMSG]);
                dicoSession.Add("EudoNetworkEnabled", config[eLibConst.CONFIG_DEFAULT.EUDONETWORKENABLED]);
                dicoSession.Add("ExportMode", _ePref.GetConfig(eLibConst.PREF_CONFIG.EXPORTMODE));
                dicoSession.Add("FeedBack", config[eLibConst.CONFIG_DEFAULT.FEEDBACKENABLED]);
                dicoSession.Add("FusionChart", "1");
                dicoSession.Add("Group", _ePref.GroupMode.GetHashCode().ToString());
                dicoSession.Add("HelpDeskCopy", config[eLibConst.CONFIG_DEFAULT.HELPDESKCOPY]);
                dicoSession.Add("HtmlEnabled", config[eLibConst.CONFIG_DEFAULT.HTMLENABLED]);
                dicoSession.Add("Intranet", eLibConst.ADR_IP_EUDOSRV.ContainsKey(sLocalIp) ? "0" : "1");
                dicoSession.Add("KeyCommon", config[eLibConst.CONFIG_DEFAULT.KEYCOMMON]);
                dicoSession.Add("MarkedFileEnabled", config[eLibConst.CONFIG_DEFAULT.MARKEDFILEENABLED]);

                dicoSession.Add("NomiAuth", String.Concat(config[eLibConst.CONFIG_DEFAULT.NOMINATIONLOGIN], "||", config[eLibConst.CONFIG_DEFAULT.NOMINATIONPASSWORD]));
                dicoSession.Add("NomiEnabled", config[eLibConst.CONFIG_DEFAULT.NOMINATIONENABLED]);
                dicoSession.Add("SmtpFeedBack", config[eLibConst.CONFIG_DEFAULT.SMTPEMAILINGSERVERNAME]);   //GCH smtp feedback devient smtp servermailing pour l'unification des smtps
                dicoSession.Add("SqlVersion", "10"); // version min pour faire fonctionner xrm
                dicoSession.Add("SynchroEnabled", config[eLibConst.CONFIG_DEFAULT.SYNCHROENABLED] == "1" ? "1" : "0");
                dicoSession.Add("ToolTipTextEnabled", _ePref.GetConfig(eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED));
                // Theme V7
                dicoSession.Add("Theme", config[eLibConst.CONFIG_DEFAULT.THEME]);
                // Theme XRM
                dicoSession.Add("ThemeXrmColor", _ePref.ThemeXRM.Color);
                dicoSession.Add("ThemeXrmName", _ePref.ThemeXRM.Name);
                dicoSession.Add("ThemeXrmFolder", _ePref.ThemeXRM.Folder);

                randomNumber = random.Next(0, 500);
                dicoSession.Add(String.Concat("xx", randomNumber), String.Concat(randomNumber, _Context.Timestamp.Ticks.ToString()));

                //Dans Main.asp
                dicoSession.Add("DisplayVerMsgSession", "1");
                dicoSession.Add("ScreenWidth", _ePref.Context.ScreenWidth.ToString());
                dicoSession.Add("ScreenHeight", _ePref.Context.ScreenHeight.ToString());

                //Requête SQL des ID du mode liste courrant
                dicoSession.Add(String.Concat("idfrom_", Tab), _ePref.Context.Paging.SqlId);

                //Informations de contexte de liste
                if (_ePref.Context.Filters.ContainsKey(Tab))
                {
                    //Id du filtre
                    dicoSession.Add(String.Concat("queryid_", Tab), _ePref.Context.Filters[Tab].FilterSelId.ToString());

                    //Nom du filtre
                    dicoSession.Add(String.Concat("queryname_", Tab), _ePref.Context.Filters[Tab].FilterName);

                    dicoSession.Add(String.Concat("QueryActiveClick_", Tab), "1");
                }

                MarkedFilesSelection markedSel = null;
                _ePref.Context.MarkedFiles.TryGetValue(Tab, out markedSel);
                if (markedSel != null)
                    dicoSession["markedfileseactived"] = markedSel.Enabled ? "1" : "0";
                markedSel = null;

                //Valeurs à passé en QS a ExportToLink.asp
                dicoSession.Add("QS_tab", Tab.ToString());
                dicoSession.Add("QS_fileid", FileId.ToString());
                dicoSession.Add("QS_descid", _nDescId.ToString());
                dicoSession.Add("QS_url", _sURL.ToString());
                dicoSession.Add("QS_urlname", _sURLName.ToString());
                dicoSession.Add("QS_urlparam", _sURLParam.ToString());

                //Param pour XRM
                dicoSession.Add("XS_openmode", _openMode.GetHashCode().ToString());
                dicoSession.Add("XS_speciftype", _cSpecifType.GetHashCode().ToString());
                dicoSession.Add("XS_specifsource", _cSourceSpecif.GetHashCode().ToString());

                dicoSession.Add("XS_parenttab", ParentTab.ToString()); //execution depuis une formule
                dicoSession.Add("XS_parentfileid", ParentFileId.ToString());     //execution depuis une formule

                dicoSession.Add("XS_BaseURL", _ePref.AppExternalUrl);


                //Création du token
                String sToken = String.Empty;

                foreach (KeyValuePair<String, String> kv in dicoSession)
                {
                    String sKey = kv.Key;
                    String sValue = kv.Value;
                    String sKV = String.Concat(sKey, "#=#", kv.Value);

                    if (sToken.Length > 0)
                        sToken = String.Concat(sToken, "#&#");

                    sToken = String.Concat(sToken, sKV);
                }

                _sToken = CryptoTripleDES.Encrypt(sToken, CryptographyConst.KEY_CRYPT_LINK3);
                _bIsGenerated = true;


                if (_isStatic)
                {


                    string sAppName = string.Concat("SPECIF_", this._nURLID, "_", DateTime.Now.Ticks.ToString(), "_", eLibTools.GetToken(10) );

                    APPKEY curr = APPKEY.GetToken(_ePref,
                        _ePref.User,
                        CryptographyConst.TokenType.SPECIF, sAppName,
                            notExpired: true,
                            notDisabled: true);

                    if (curr == null)
                    {
                        _sShortToken = APPKEY.CreateAppKeyToken(_ePref, _ePref.User, CryptographyConst.TokenType.SPECIF, sAppName, _sToken, expirationDate: DateTime.Now.AddHours(24));
                    }
                    else
                    {
                        APPKEY.UpdateExpiration(_ePref, curr.Id, DateTime.Now.AddHours(24));

                        _sShortToken = eLoginTools.GetCnxTokenKey(new CnxToken()
                        {
                            Key = curr.Token,
                            DBUID = _ePref.DatabaseUid,
                            tokentype = CryptographyConst.TokenType.SPECIF

                        });


                    }
                   
                }
                return true;
            }
            catch (Exception e)
            {
                _eInnerException = e;
                return false;
            }
        }

        /// <summary>
        /// initie les paramètre tu token à partir d'un objet eSpecif
        /// </summary>
        /// <param name="spec">Ide la spécif</param>
        /// <param name="nTab"> descid de l'onglet actif</param>
        /// <param name="nFileId">fileid de la fiche en cours</param>
        /// <param name="nParentTab">Dans le cas d'un lancement via signet, Table parente</param>
        /// <param name="nParentFileId">Dans le cas d'un lancement via signet,  Id de la fiche parente</param>
        /// <param name="nDescId">lancement depuis un champ/signet de typ page web : descid du champ/signet</param>
        /// <returns></returns>
        private Boolean InitTokenXRM(eSpecif spec, Int32 nTab = 0, Int32 nFileId = 0, Int32 nParentTab = 0, Int32 nParentFileId = 0, Int32 nDescId = 0)
        {
            _cSpecifType = spec.Type;
            _nURLID = spec.SpecifId;
            _sURL = spec.Url;
            _sURLParam = spec.UrlParam;
            _sURLName = spec.Label;
            _openMode = spec.OpenMode;
            _cSourceSpecif = spec.Source;

            _isStatic = spec.IsStatic;

            _nFileId = nFileId;
            _nTab = nTab;
            _nParentTab = nParentTab;
            _nParentFileId = nParentFileId;
            _nDescId = nDescId;

            _bIsInit = true;



            #region sécurité/confidentialié
            if (nFileId > 0 || _nParentFileId > 0 || _nTab > 0 || _nParentTab > 0)
            {

                eudoDAL d = eLibTools.GetEudoDAL(_ePref);
                try
                {

                    d.OpenDatabase();

                    //Droit sur la fiche
                    if (nFileId > 0 && nTab > 0)
                    {
                        var val = eDataFillerGeneric.GetFieldsValue(_ePref, new HashSet<int>() { nTab + 95 }, nTab, nFileId);
                        if (val.Count == 0)
                        {
                            ErrorMsg = eResApp.GetRes(_ePref, 6696);
                            return false;
                        }
                    }
                    else if (nTab > 0) //Droit sur la table (mode liste)
                    {
                        var tt = eDataFillerGeneric.GetInfo(_ePref, nTab);
                        if (tt == null || tt.ViewMainTable == null || !tt.ViewMainTable.PermViewAll)
                        {
                            ErrorMsg = eResApp.GetRes(_ePref, 6696);
                            return false;
                        }
                    }

                    if (nParentFileId > 0 && nParentFileId > 0)
                    {
                        var valpra = eDataFillerGeneric.GetFieldsValue(_ePref, new HashSet<int>() { nParentTab + 95 }, nParentTab, nParentFileId);
                        if (valpra.Count == 0)
                        {
                            ErrorMsg = eResApp.GetRes(_ePref, 6696);
                            return false;
                        }
                    }
                    else if (nParentTab > 0)
                    {
                        var tt = eDataFillerGeneric.GetInfo(_ePref, nParentTab);
                        if (tt == null || tt.ViewMainTable == null || !tt.ViewMainTable.PermViewAll)
                        {
                            ErrorMsg = eResApp.GetRes(_ePref, 6696);
                            return false;
                        }
                    }
                }
                catch (DataFillerRowNotFound)
                {
                    ErrorMsg = eResApp.GetRes(_ePref, 6696);

                    return false;
                }
                finally
                {
                    d.CloseDatabase();
                }
            }
            #endregion
            return true;
        }



        /// <summary>
        /// Initialise le token avec les informations fournies et celle en bdd
        /// </summary>
        /// <param name="ctype">Type de spécif</param>
        /// <param name="sUrl">url de la spécif</param>
        /// <param name="nTab">Table depuis laquelle l'appel a été fait</param>
        /// <param name="nFileId">Id de la fiche depuis l'appel a éta fait</param>
        /// <param name="nDescId">descid du champ depuis laquelle la specif a été appelée (Champ de type Page Web)</param>
        private Boolean InitTokenV7(eLibConst.SPECIF_TYPE ctype, String sUrl, Int32 nTab, Int32 nFileId, Int32 nDescId = 0)
        {

            FileId = nFileId;
            Tab = nTab;
            _nDescId = nDescId;
            _cSpecifType = ctype;

            _sBaseSiteV7URL = String.Concat(_Context.Request.Url.Scheme, "://", _Context.Request.Url.Authority, "/", eLibTools.GetServerConfig("v7dir").TrimEnd('/'), "/");


            if (ctype == eLibConst.SPECIF_TYPE.TYP_ADMIN)
            {
                _bIsInit = true;

                _sURL = "main.asp";
                return true;
            }

            eudoDAL edal = eLibTools.GetEudoDAL(_ePref);


            try
            {

                Int32 nUrlId = 0;

                RqParam rqURL = new RqParam();
                String sSQL = String.Empty;



                switch (ctype)
                {
                    case eLibConst.SPECIF_TYPE.TYP_FAVORITE:

                        if (!Int32.TryParse(sUrl, out nUrlId))
                        {
                            ErrorMsg = String.Concat(" URL INVALIDE : [", sUrl, "]");
                            return false;
                        }



                        //Lien page accueil
                        sSQL = String.Concat("SELECT  [libelle] as [URLNAME], [value] as [URL], '' as URLPARAM FROM [homepage] ",
                                              " inner join [config] on ( [config].[userid]=@USERID  or [config].[userid] = 0 ) ",
                                              " and charindex(';' + cast([HpgId] as varchar(10)) + ';',';'+favorites +';')>0 ",
                                              "   where [type] = @HPG_FAV_EDN  AND [HPGID] = @URLID  ");
                        rqURL.SetQuery(sSQL);
                        rqURL.AddInputParameter("@USERID", SqlDbType.Int, _ePref.User.UserId.ToString());
                        rqURL.AddInputParameter("@URLID", SqlDbType.Int, nUrlId);
                        rqURL.AddInputParameter("@HPG_FAV_EDN", SqlDbType.Int, eConst.HOMEPAGE_TYPE.HPG_FAV_EDN.GetHashCode());

                        break;
                    case eLibConst.SPECIF_TYPE.TYP_SPECIF:
                        if (!Int32.TryParse(sUrl, out nUrlId))
                        {
                            ErrorMsg = String.Concat(" URL INVALIDE : [", sUrl, "]");
                            return false;
                        }
                        //Lien spécif
                        sSQL = "SELECT PREFID, URL,URLNAME,URLPARAM FROM [PREF] WHERE USERID=0 AND TAB=@TAB AND URL<>'' AND PREFID=@URLID";

                        rqURL.SetQuery(sSQL);
                        rqURL.AddInputParameter("@TAB", SqlDbType.Int, nTab);
                        rqURL.AddInputParameter("@URLID", SqlDbType.Int, nUrlId);

                        break;
                    case eLibConst.SPECIF_TYPE.TYP_DECL:
                        //Lien spécif
                        sSQL = "SELECT @URL URL, '' URLNAME, '' URLPARAM";

                        rqURL.SetQuery(sSQL);
                        rqURL.AddInputParameter("@URL", SqlDbType.VarChar, sUrl);
                        break;
                    case eLibConst.SPECIF_TYPE.TYP_EUDOPART:
                        if (!Int32.TryParse(sUrl, out nUrlId))
                        {
                            ErrorMsg = String.Concat(" URL INVALIDE : [", sUrl, "]");
                            return false;
                        }
                        //HomePage User
                        sSQL = String.Concat("SELECT EudoPartTitle as URLNAME, EudoPartContent as URL, '' as URLPARAM FROM [HomePageAdvanced] ",
                                              " inner join [eudopart] on [eudopart].[EUDOPARTID]=@URLID ",
                                              " where [HomePageAdvanced].[HomePageId]  in ( ",
                                              " select top 1 coalesce(u.ADVANCEDHOMEPAGEID,d.ADVANCEDHOMEPAGEID,gr.ADVANCEDHOMEPAGEID,0 ) ",
                                              " from config u   ",
                                              " left join config d on d.userid=0    ",
                                              " left join (  ",
                                              " select [AdvancedHomepageId], userid ",
                                              " from [user]  ",
                                              " left join [Group] on [user].[groupid] = [group].[groupid]  where userid = @USERID ) gr on gr.userid=u.userid ",
                                              "   where u.userid= @USERID )");

                        rqURL.SetQuery(sSQL);
                        rqURL.AddInputParameter("@USERID", SqlDbType.Int, _ePref.User.UserId.ToString());
                        rqURL.AddInputParameter("@URLID", SqlDbType.Int, nUrlId);

                        break;
                    case eLibConst.SPECIF_TYPE.TYP_WEBFIELD:

                        //Lien spécif
                        sSQL = "SELECT @URL URL, '' URLNAME, '' URLPARAM";

                        rqURL.SetQuery(sSQL);
                        rqURL.AddInputParameter("@URL", SqlDbType.VarChar, sUrl);
                        break;

                    case eLibConst.SPECIF_TYPE.TYP_REPORT:

                        if (!Int32.TryParse(sUrl, out nUrlId))
                        {
                            ErrorMsg = String.Concat(" URL INVALIDE : [", sUrl, "]");
                            return false;
                        }


                        eReport er = new eReport(_ePref, nUrlId);
                        if (er.LoadFromDB())
                        {

                            String sParam = String.Concat("tab=", nTab, "&frombrkm=0&tabfrom=", nTab, "&typereport=0&parentfileid=0&reportid=", nUrlId);

                            sSQL = "SELECT @URL URL, @URLNAME URLNAME, @URLPARAM URLPARAM";
                            rqURL.SetQuery(sSQL);

                            rqURL.AddInputParameter("@URL", SqlDbType.VarChar, er.GetParamValue("URL"));
                            rqURL.AddInputParameter("@URLNAME", SqlDbType.VarChar, er.Name);
                            rqURL.AddInputParameter("@URLPARAM", SqlDbType.VarChar, sParam);
                            break;
                        }
                        else
                            return false;

                        break;
                    default:
                        return false;
                        break;
                }



                edal.OpenDatabase();
                string eErrExec = "";
                DataTableReaderTuned dtr = edal.Execute(rqURL, out eErrExec);

                try
                {
                    if (eErrExec.Length > 0)
                    {
                        ErrorMsg = "Informations sur la spécif non disponible";
                        if (edal.InnerException != null)
                            _eInnerException = edal.InnerException;

                        return false;
                    }
                    else if (dtr == null || !dtr.HasRows || !dtr.Read())
                    {
                        ErrorMsg = String.Concat("Spécif non trouvée (ctype : ", ctype.ToString(), " - sUrl : ", sUrl, " - nTab : ", nTab, " - nFileId : ", nFileId);
                        return false;
                    }
                    else
                    {
                        _sURL = dtr.GetString("URL");
                        _sURLName = dtr.GetString("URLNAME");
                        _sURLParam = dtr.GetString("URLPARAM");

                        if (_sURL.Length == 0)
                        {
                            ErrorMsg = String.Concat("URL Non valide (ctype : ", ctype.ToString(), " - sUrl : ", sUrl, " - nTab : ", nTab, " - nFileId : ", nFileId);
                            return false;
                        }
                    }

                    _bIsInit = true;
                }
                finally
                {
                    if (dtr != null)
                        dtr.Dispose();
                }
                return true;


            }
            catch (Exception e)
            {
                _eInnerException = e;
                return false;
            }
            finally
            {
                edal.CloseDatabase();
            }






        }

        #endregion
    }
}