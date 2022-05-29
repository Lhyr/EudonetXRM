using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    /// <className>eMailingTemplate</className>
    /// <summary>Classe permettant de gérer certains aspects des fichiers de type E-mail dans Assistant de l'Emailing 
    /// Gère, entre autres, la récupération et la mise à jour des modèles de mail (eMailingTemplate)</summary>
    /// <purpose>Permet d'accéder aux informations contenues dans la table MAILTEMPLATE </purpose>
    /// <authors>MZA</authors>
    /// <date>2013-12-05</date>
    public class eMailingTemplate
    {
        private String VIEW_PERMID_ALIAS = String.Concat(TableType.MAIL_TEMPLATE.GetHashCode(), "_", MailTemplateField.VIEW_PERMID.GetHashCode());
        private String UPADTE_PERMID_ALIAS = String.Concat(TableType.MAIL_TEMPLATE.GetHashCode(), "_", MailTemplateField.UPDATE_PERMID.GetHashCode());

        #region Propriétés

        private TemplateType _eTplType;

        private ePref _pref;
        private eudoDAL _dal;
        private Boolean _bIsLocalDal = true;
        private Boolean _bIsLocalDalOpened = false;

        private Exception _innerException;

        /// <summary>Identifiant de la Template dans la table MAILTEMPLATE.</summary>
        private Int32 _nId;
        /// <summary>Onglet sur leque le Modele à été créé</summary>
        private Int32 _nTab;
        /// <summary>Libellé de la Template dans la table MAILTEMPLATE.</summary>
        private String _sLabel = String.Empty;
        /// <summary>L'utilisateur qui a cree la Template </summary>
        private Int32 _nCreatedBy;
        /// <summary> la Date de Modificatn de la Template  </summary>
        private DateTime _dModifiedOn;
        /// <summary>L'utilisateur qui a modifié la Template </summary>
        private Int32 _nModifiedBy;
        /// <summary> la Date de Creation de la Template</summary>
        private DateTime _dCreatedOn;
        /// <summary>
        /// Type de modèle de mail dans la base (e-mailing ou mail unitaire)
        /// </summary>
        private TypeMailTemplate _mtType;
        /// <summary>
        /// Liste des PJ rattachés au modèle
        /// </summary>
        private List<int> _listPJ;

        /// <summary>Objet récupéré par l'analyzer de body</summary>
        eAnalyzerInfos _bodyAnalyse;
        #endregion

        #region Enumérations
        /// <summary>Type de modèle de mail choisi sur l'interface d'emailing (personnalisable, utilisateur, pas de modèle)</summary>
        public enum TemplateType
        {
            /// <summary>Modèles utilisateur</summary>
            USER_TEMPLATE = 0,
            /// <summary>Modèles personnalisable</summary>
            CUSTOM_TEMPLATES = 1,
            /// <summary>Pas de modèle</summary>
            NO_TEMPLATE = 2
        }

        #endregion

        #region Accesseurs

        /// <summary>Exception levée par les méthodes internes à la classe</summary>
        public Exception InnerException
        {
            get { return _innerException; }
        }

        /// <summary>Droits de Visualisation</summary>
        public ePermission ViewPerm { get; set; }

        /// <summary>Droits de modification</summary>
        public ePermission UpdatePerm { get; set; }

        /// <summary>Message d'erreur sur la dernière opération effectuée par le Mailingtemplate s'il y a lieu.</summary>
        public String ErrorMessage { get; private set; }

        /// <summary>Erreur sur la dernière opération effectuée par le Mailingtemplate s'il y a lieu.</summary>
        public String ErrorDev { get; private set; }

        /// <summary>Identifiant de la Template dans la table MAILTEMPLATE.</summary>
        public Int32 Id
        {
            get { return _nId; }
            set { _nId = value; }
        }

        /// <summary>Libellé de la Template dans la table MAILTEMPLATE.</summary>
        public String Label
        {
            get { return _sLabel; }
            set { _sLabel = value; }
        }

        /// <summary>Sujet de la Template dans la table MAILTEMPLATE.</summary>
        public String Subject { get; set; }

        /// <summary> Preheader de la template dans la table MAILTEMPLATE </summary>
        ///AABBA tache #1 940
        public string Preheader { get; set; }

        /// <summary>Body de la Template dans la table MAILTEMPLATE.</summary>
        public String Body { get; set; }

        /// <summary>Style css du corp de modèle.</summary>
        public String BodyCss { get; set; }

        /// <summary>Body_HTML de la Template dans la table MAILTEMPLATE.</summary>
        public Boolean Body_HTML { get; set; }

        /// <summary>Histo de la Template dans la table MAILTEMPLATE.</summary>
        public Boolean Histo { get; set; }

        /// <summary>Onglet sur leque le Modele à été créé</summary>
        public Int32 Tab
        {
            get { return _nTab; }
            set { _nTab = value; }
        }

        /// <summary> la Date de Creation de la Template</summary>
        public DateTime CreatedOn
        {
            get { return _dCreatedOn; }
            set { _dCreatedOn = value; }
        }


        /// <summary>L'utilisateur qui a cree la Template </summary>
        public Int32 CreatedBy
        {
            get { return _nCreatedBy; }
            set { _nCreatedBy = value; }
        }

        /// <summary> la Date de Modificatn de la Template  </summary>
        public DateTime ModifiedOn
        {
            get { return _dModifiedOn; }
            set { _dModifiedOn = value; }
        }

        /// <summary>L'utilisateur qui a modifié la Template </summary>
        public Int32 ModifiedBy
        {
            get { return _nModifiedBy; }
            set { _nModifiedBy = value; }
        }

        /// <summary>Propriétaire</summary>
        public Int32 Owner_User { get; set; }

        /// <summary>
        /// Type de modèle de mail stocké en base (e-mailing, mail unitaire, ...)
        /// </summary>
        public TypeMailTemplate MailTemplateType
        {
            get { return _mtType; }
            set { _mtType = value; }
        }

        /// <summary>
        /// Liste des PJ rattachés au modèle de mail
        /// </summary>
        public List<int> ListTemplatePJ
        {
            get { return _listPJ; }
            set { _listPJ = value; }
        }
        #endregion

        /// <summary>
        /// Constructeur de modèle d'emailing
        /// </summary>
        /// <param name="id">id du modèle</param>
        /// <param name="pref">preferences de l'utilisateur</param>
        public eMailingTemplate(Int32 id, ePref pref)
            : this(pref)
        {
            _listPJ = new List<int>();

            if (id > 0)
                this.Load(id);
        }

        /// <summary>
        /// Constructeur initialisant les prefs
        /// </summary>
        /// <param name="pref">preferences de l'utilisateur</param>
        public eMailingTemplate(ePref pref)
        {
            _pref = pref;
            this.ViewPerm = new ePermission(0, _pref);
            this.UpdatePerm = new ePermission(0, _pref);

            // #41134 CRU : Initialisation de Owner_User
            this.Owner_User = _pref.UserId;

            _listPJ = new List<int>();
        }

        /// <summary>
        /// Ouvre la connexion locale si elle existe
        /// </summary>
        private void OpenDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                _dal = eLibTools.GetEudoDAL(_pref);
                this._dal.OpenDatabase();
                this._bIsLocalDalOpened = true;
            }
        }

        /// <summary>
        /// Ferme la connexion locale si elle existe
        /// </summary>
        private void CloseDal()
        {
            if (_bIsLocalDal && !_bIsLocalDalOpened)
            {
                this._dal?.CloseDatabase();
                this._bIsLocalDalOpened = false;
            }
        }

        #region Gestion d'erreur
        /// <summary>
        /// La gestion d'erreur
        /// </summary>
        private void ClearError()
        {
            ErrorMessage = String.Empty;
            ErrorDev = String.Empty;
        }
        internal void AddError(String errorMessage, Exception ex)
        {
            if (String.IsNullOrEmpty(errorMessage))
                return;
            if (ex == null)
            {
                ErrorMessage = errorMessage;
                ErrorDev = ErrorMessage;
            }
            else
            {
                ErrorMessage = String.Concat(errorMessage, "<br/>", ex.Message);
                ErrorDev = String.Concat(ErrorMessage, "<br/>", "[stacktrace]", "<br />", ex.StackTrace);
            }
        }

        internal void AddError(String errorMessage)
        {
            AddError(errorMessage, null);
        }
        #endregion

        /// <summary>
        /// Charge l'ensemble des information relative au modèle.
        /// </summary>
        /// <returns>true si le traitement s'est bien passé, sinon false.</returns>
        public Boolean Load(Int32 IdMailingTemplate)
        {
            this._nId = IdMailingTemplate;

            EudoQuery.EudoQuery dq = null;
            DataTableReaderTuned dtr = null;
            try
            {
                ClearError();
                dq = eLibTools.GetEudoQuery(_pref, TableType.MAIL_TEMPLATE.GetHashCode(), ViewQuery.FILE);
                //dq = new EudoQuery.EudoQuery(_ePref.GetSqlInstance, _ePref.GetBaseName, TableType.MAIL_TEMPLATE.GetHashCode(), _ePref.User.UserId, _ePref.Lang,);
                dq.SetFileId = IdMailingTemplate;
                dq.LoadRequest();
                if (dq.GetError.Length > 0)
                    return false;
                dq.BuildRequest();
                if (dq.GetError.Length > 0)
                    return false;
                string req = dq.EqQuery;
                List<Field> listFld = dq.GetFieldHeaderList;
                Field fLbl = null;
                Field fObj = null;
                ///AABBA tache #1 940
                Field fPreheader = null;
                Field fBody = null;
                Field fBodyCss = null;
                Field fBodyHtml = null;
                Field fCreatedOn = null;
                Field fCreatedBy = null;
                Field fModifiedOn = null;
                Field fModifiedBy = null;
                Field fOwner = null;
                Field fTab = null;
                Field fMailTemplateType = null;

                foreach (Field fld in listFld)
                {
                    if (fld.Descid == MailTemplateField.LABEL.GetHashCode())
                        fLbl = fld;
                    else if (fld.Descid == MailTemplateField.SUBJECT.GetHashCode())
                        fObj = fld;
                    ///AABBA tache #1 940
                    else if (fld.Descid == MailTemplateField.PREHEADER.GetHashCode())
                        fPreheader = fld;
                    else if (fld.Descid == MailTemplateField.BODY.GetHashCode())
                        fBody = fld;
                    else if (fld.Descid == MailTemplateField.BODY_CSS.GetHashCode())
                        fBodyCss = fld;
                    else if (fld.Descid == MailTemplateField.ISHTML.GetHashCode())
                        fBodyHtml = fld;
                    else if (fld.Descid == MailTemplateField.TAB.GetHashCode())
                        fTab = fld;
                    else if (fld.Descid == MailTemplateField.TYPE.GetHashCode())
                        fMailTemplateType = fld;
                    else if (fld.Descid == TableType.MAIL_TEMPLATE.GetHashCode() + AllField.DATE_CREATE.GetHashCode())
                        fCreatedOn = fld;
                    else if (fld.Descid == TableType.MAIL_TEMPLATE.GetHashCode() + AllField.USER_CREATE.GetHashCode())
                        fCreatedBy = fld;
                    else if (fld.Descid == TableType.MAIL_TEMPLATE.GetHashCode() + AllField.DATE_MODIFY.GetHashCode())
                        fModifiedOn = fld;
                    else if (fld.Descid == TableType.MAIL_TEMPLATE.GetHashCode() + AllField.USER_MODIFY.GetHashCode())
                        fModifiedBy = fld;
                    else if (fld.Descid == TableType.MAIL_TEMPLATE.GetHashCode() + AllField.OWNER_USER.GetHashCode())
                        fOwner = fld;

                }

                if (IdMailingTemplate > 0)
                {
                    RqParam LoadRq = new RqParam(req.ToString());
                    OpenDal();
                    try
                    {
                        String sErrorMessage;
                        dtr = _dal.Execute(LoadRq, out sErrorMessage);
                        if (sErrorMessage.Length > 0)
                        {
                            AddError(String.Concat("Load execute", sErrorMessage));
                            return false;
                        }
                        if (dtr != null && dtr.HasRows && dtr.Read())
                        {

                            this.Label = dtr.GetString(fLbl.Alias);
                            this.Subject = dtr.GetString(fObj.Alias).ToString();
                            ///AABBA tache #1 940
                            this.Preheader = dtr.GetString(fPreheader.Alias).ToString();
                            this.Body = dtr.GetString(fBody.Alias);
                            this.BodyCss = dtr.GetString(fBodyCss.Alias);

                            this.Body_HTML = dtr.GetString(fBodyHtml.Alias) == "1" ? true : false;
                            _nTab = dtr.GetEudoNumeric(fTab.Alias);

                            this.MailTemplateType = TypeMailTemplate.MAILTEMPLATE_UNDEFINED;

                            int nType;
                            if (int.TryParse(dtr.GetString(fMailTemplateType.Alias), out nType))
                            {

                                this.MailTemplateType = eLibTools.GetEnumFromCode<TypeMailTemplate>(nType);
                            }


                            Int32 outViewPermId = dtr.GetEudoNumeric(VIEW_PERMID_ALIAS);
                            Int32 outUpdatePermId = dtr.GetEudoNumeric(UPADTE_PERMID_ALIAS);

                            if (!dtr.IsDBNull(fCreatedOn.Alias))
                                DateTime.TryParse(dtr.GetString(fCreatedOn.Alias), out _dCreatedOn);
                            _nCreatedBy = dtr.GetEudoNumeric(fCreatedBy.Alias);
                            if (!dtr.IsDBNull(fModifiedOn.Alias))
                                DateTime.TryParse(dtr.GetString(fModifiedOn.Alias), out _dModifiedOn);

                            _nModifiedBy = dtr.GetEudoNumeric(fModifiedBy.Alias);


                            this.ViewPerm = new ePermission(outViewPermId, _dal, _pref);
                            this.UpdatePerm = new ePermission(outUpdatePermId, _dal, _pref);
                            this.Owner_User = dtr.GetEudoNumeric(fOwner.Alias);


                        }
                        // PJ
                        LoadAttachments(out sErrorMessage);
                    }
                    catch (Exception ex)
                    {
                        AddError("Load dtr", ex);
                        return false;
                    }
                    finally
                    {
                        if (dtr != null)
                            dtr.Dispose();
                        CloseDal();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.AddError("eMailingTemplate.Load() : ", ex);
                _innerException = ex;
                return false;
            }
            finally
            {
                CloseDal();
            }
        }

        /// <summary>
        /// Charge un modèle de mail depuis la liste prédefinie en fonction d'id de template
        /// </summary>
        /// <param name="_iMailTemplateId">id template</param>
        public void LoadCustom(int _iMailTemplateId)
        {
            this._nId = _iMailTemplateId;
            this._eTplType = TemplateType.CUSTOM_TEMPLATES;

            XmlDocument TemplatesList = new XmlDocument();

            try
            {
                TemplatesList.Load(String.Concat(AppDomain.CurrentDomain.BaseDirectory, "\\Emailing\\templates.xml"));
            }
            catch (Exception ex)
            {
                string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                    , Environment.NewLine);
                sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Exception : ", ex.ToString());
                string sUsrMsg = string.Concat("<br>", eResApp.GetRes(this._pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   sUsrMsg,  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   sDevMsg);
                eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);

                return;
            }

            Int32 nTemplateId;
            Int32 nResId;
            String sHtmlFile = string.Empty;

            foreach (XmlNode xmlTemplate in TemplatesList.SelectNodes("//templates//template"))
            {
                try
                {
                    Int32.TryParse(xmlTemplate.Attributes["id"].Value, out nTemplateId);
                    Int32.TryParse(xmlTemplate.Attributes["res"].Value, out nResId);

                    if (nTemplateId == _iMailTemplateId)
                    {
                        String fileName = xmlTemplate.Attributes["folder"].Value;
                        sHtmlFile = String.Concat("\\Emailing\\Templates\\", fileName, "\\", fileName, ".html");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    string sDevMsg = string.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                       , Environment.NewLine);
                    sDevMsg = string.Concat(sDevMsg, Environment.NewLine, "Attributs n'existent pas dans le document xml: ", ex.ToString());
                    string sUsrMsg = string.Concat("<br>", eResApp.GetRes(this._pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                    eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                       sUsrMsg,  //  Détail : pour améliorer...
                       eResApp.GetRes(_pref, 72),  //   titre
                       sDevMsg);
                    eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);

                    break;
                }
            }

            //lecture du fichier modèle
            StreamReader sr = null;
            try
            {
                //StringBuilder body = new StringBuilder();
                sr = new StreamReader(String.Concat(AppDomain.CurrentDomain.BaseDirectory, sHtmlFile));

                String html = sr.ReadToEnd();

                //a injecter dans pour ckeditor
                this.BodyCss = this.GetInnerTag("style", html);

                //Iso v7 , on sauvegarde les style dans un commantaire
                //this._sBody = body.Append("<style>").Append(_sBodyCss).Append("</style>").Append(this.GetContentOf("body", html)).ToString();

                this.Body = this.GetInnerTag("body", html);
            }
            catch (Exception ex)
            {
                string sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine
                          , Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Error fichier html: ", sHtmlFile, ex.ToString());
                string sUsrMsg = String.Concat("<br>", eResApp.GetRes(this._pref, 422), "<br>", eResApp.GetRes(_pref, 544));

                eErrorContainer eErrCont = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   sUsrMsg,  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   sDevMsg);
                eFeedbackXrm.LaunchFeedbackXrm(eErrCont, _pref);
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }

        }

        /// <summary>
        /// Retourne le contenu dans le tag <tag>html</tag> 
        /// </summary>
        /// <param name="tag">tag html : </param>
        /// <param name="document">Le document html ou chercher</param>
        /// <returns>le contenu dans le tag</returns>
        private String GetInnerTag(String tag, String document)
        {

            RegexOptions opts = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            Regex regX = new Regex(@"<\s*" + tag + @"[^>]*>(?<content>(.*))<[\s/]*" + tag + "[^>]*>", opts);
            Match match = regX.Match(document);

            if (match.Success)
                return match.Groups["content"].Value;


            return String.Empty;
        }

        /// <summary>
        /// Retourne l'objet de modification de la BDD préparamétré pour la CrUD)
        /// </summary>
        /// <returns>objet de modification de la BDD</returns>
        private Engine.Engine GetEngine()
        {
            Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.MAIL_TEMPLATE, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = Id;
            return eng;
        }

        /// <summary>
        /// Supprime un modèle
        /// </summary>
        /// <param name="IdMailingTemplate"></param>
        /// <returns></returns>
        public Boolean Delete()
        {
            String sErrorMessage;

            if (UpdatePerm != null && !UpdatePerm.IsAllowed(_pref.User))
            {
                AddError("Dev: On devrait pas tomber sur cas sauf si on change l'id de formulaire depuis javascript");
                return false;
            }
            if (Id <= 0)
            {
                AddError("eMailingTemplate.Delete() : pas de modèle chargé ou chargement du modèle en erreur");
                return false;
            }

            // Suppression des PJ
            DeleteAttachments(out sErrorMessage);
            AddError(sErrorMessage);

            Engine.Engine eng = GetEngine();
            eng.EngineProcess(new StrategyDelSimple());

            EngineResult engResult = eng.Result;
            if (!engResult.Success)
            {

                if (engResult.Error != null && (engResult.Error.DebugMsg.Length > 0 || engResult.Error.Msg.Length > 0))
                    sErrorMessage = String.Format("user : {0}\n\rdev : {1}", engResult.Error.Msg, engResult.Error.DebugMsg);
                else
                    sErrorMessage = "Erreur inconnue.";
                sErrorMessage = String.Concat("Un problème est survenu lors de la suppression : ", Environment.NewLine, sErrorMessage);

                AddError(sErrorMessage);

                return false;
            }
            return true;
        }

        /// <summary>
        /// On insert le modèle s'il exite pas sinon on le mets a jour
        /// </summary>
        /// <returns></returns>
        public Boolean Save()
        {
            // On enregistre le contenu du mail
            if (!UpdateOrInsert())
                return false;

            // On sauvegarde les liens annexes présent dans le corps de modèle de mailing
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING)
            {
                if (!UpdateOrInsertPjLinks())
                    return false;
            }

            if (!UpdateMailingTemplateBody())
                return false;

            return true;
        }

        /// <summary>
        /// Met à jour ou modifie une fiche
        /// </summary>
        /// <returns>Faux si une erreur s'est produite</returns>
        private Boolean UpdateOrInsert()
        {
            //Analyse du body et du sujet avant la MAJ ou INSERT pour bien initialiser les variables
            if (!AnalyseBody())
                return false;

            Engine.Engine eng = GetEngine();

            eng.AddNewValue(MailTemplateField.LABEL.GetHashCode(), HttpUtility.HtmlDecode(Label), true);
            eng.AddNewValue(MailTemplateField.SUBJECT.GetHashCode(), HttpUtility.HtmlDecode(Subject), true);
            // AABBA tache #1 940
            eng.AddNewValue(MailTemplateField.PREHEADER.GetHashCode(), HttpUtility.HtmlDecode(Preheader), true);
            //Le body est maintenant enregistré dans une méthode à part
            //eng.AddNewValue(MailTemplateField.BODY.GetHashCode(), HttpUtility.HtmlDecode(Body), true);
            eng.AddNewValue(MailTemplateField.BODY_CSS.GetHashCode(), HttpUtility.HtmlDecode(BodyCss), true);
            eng.AddNewValue(MailTemplateField.TYPE.GetHashCode(), MailTemplateType.GetHashCode().ToString(), true);

            eng.AddNewValue(MailTemplateField.VIEW_PERMID.GetHashCode(), (ViewPerm.PermId > 0) ? ViewPerm.PermId.ToString() : String.Empty, true);
            //BSE #48965 les droits de modif n'etaient pas enregistrées dans la base 
            eng.AddNewValue(MailTemplateField.UPDATE_PERMID.GetHashCode(), (UpdatePerm.PermId > 0) ? UpdatePerm.PermId.ToString() : String.Empty, true);
            eng.AddNewValue(MailTemplateField.TAB.GetHashCode(), Tab.ToString(), true);
            // MAB - 2015-09-01 - Demande non déclarée (échanges par mail)
            // On ne modifie pas l'appartenance d'un modèle tant qu'on ne propose pas à l'utilisateur d'indiquer à qui doit appartenir 
            // le modèle ("Filtre public" ou non). Cela évite de réaffecter à l'utilisateur courant un modèle initialement créé en filtre
            // public lorsqu'on le modifie, ce qui provoque sa "disparition" aux yeux des autres utilisateurs.
            // A réactiver lorsqu'on proposera une interface utilisateur permettant de choisir l'appartenance et les droits sur le modèle,
            // depuis la fenêtre d'édition de modèle.
            // #41134 CRU : On réactive la mise à jour de "Appartient à". Lorsqu'on choisit "Aucun modèle" et qu'on tente d'enregistrer le modèle, la popup permet
            // de choisir si c'est un modèle public ou non. Par contre, si l'ajout se fait depuis "Vos modèles", on laisse le modèle créé en public (géré dans le manager).
            eng.AddNewValue(TableType.MAIL_TEMPLATE.GetHashCode() + AllField.OWNER_USER.GetHashCode(), (Owner_User > 0) ? Owner_User.ToString() : String.Empty, true);
            eng.AddNewValue(MailTemplateField.HISTO.GetHashCode(), (Histo ? 1 : 0).ToString(), true);

            eng.EngineProcess(new StrategyCruSimple());

            EngineResult engResult = eng.Result;
            if ((engResult != null) && (engResult.NewRecord != null) && (engResult.NewRecord.FilesId != null) && (engResult.NewRecord.FilesId.Count > 0))
                Id = engResult.NewRecord.FilesId[0];
            if (!engResult.Success)
            {
                String sErrorMessage;
                if (engResult.Error != null && (engResult.Error.DebugMsg.Length > 0 || engResult.Error.Msg.Length > 0))
                    sErrorMessage = String.Format("user : {0}\n\rdev : {1}", engResult.Error.Msg, engResult.Error.DebugMsg);
                else
                    sErrorMessage = "Erreur inconnue.";
                if (Id > 0)
                    sErrorMessage = String.Concat("Fiche créée mais avec des erreurs : ", Environment.NewLine, sErrorMessage);
                else
                    sErrorMessage = String.Concat("Fiche non créée : ", Environment.NewLine, sErrorMessage);
                AddError(sErrorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Analyse le body pour isoler les liens, champs de fusion, etc
        /// </summary>
        private bool AnalyseBody()
        {
            // On teste la validité du corps du mail 
            this._bodyAnalyse = eMergeTools.AnalyzeBody(HttpUtility.HtmlDecode(Body));
            bool invalidBody = _bodyAnalyse.content == null || _bodyAnalyse.linksData.ExistInvalidLink || _bodyAnalyse.mergeData.ExistInvalidMerge;

            if (invalidBody)
            {
                ErrorMessage = eResApp.GetRes(_pref, 6644);
                ErrorDev = ErrorMessage;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse le contenu de coprs de mail et enregistre les annexes dans la table PJ 
        /// </summary>
        /// <returns></returns>
        private bool UpdateOrInsertPjLinks()
        {

            ePjLinkSaver pjLinkSaver = new ePjLinkSaver(_pref,
                (datasPath, pjName) => new ePJToAddLite()
                {
                    FileId = Id,
                    FileType = ePJTraitementsLite.GetUserFriendlyFileType(pjName),
                    Description = string.Empty,
                    Size = (int)new FileInfo(string.Concat(datasPath, "\\", pjName)).Length,
                    OverWrite = false,
                    DayExpire = null,
                    Tab = (int)TableType.MAIL_TEMPLATE,
                    TypePj = (int)PjType.FILE,
                    SaveAs = pjName,
                    Label = pjName
                });

            if (!pjLinkSaver.Save(this._bodyAnalyse, (int)TableType.MAIL_TEMPLATE, Id, (int)PjType.FILE))
            {
                ErrorMessage = pjLinkSaver.UserMessage;
                ErrorDev = ErrorMessage;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Met à jour le corps du template avec les liens sécurisé
        /// </summary>
        private bool UpdateMailingTemplateBody()
        {
            if (this.Id <= 0)
                return true;

            Body = eMergeTools.GetBodyMerge_Orig(this._bodyAnalyse);

            Engine.Engine eng = GetEngine();
            eng.AddNewValue((int)MailTemplateField.BODY, Body, true);

            eng.EngineProcess(new StrategyCruSimple());

            EngineResult engResult = eng.Result;

            string sErrorMessage = String.Empty;
            if (engResult == null)
            {
                sErrorMessage = "eMailingTemplate::UpdateMailingTemplateBody() :: engResult == null";
            }
            else if (!engResult.Success)
            {
                if (engResult.Error != null)
                    sErrorMessage = String.Concat("eMailingTemplate::UpdateMailingTemplateBody() :: ", engResult.Error.Msg, Environment.NewLine, engResult.Error.DebugMsg);
                else
                    sErrorMessage = "eMailingTemplate::UpdateMailingTemplateBody() :: engResult.Error = null";
            }

            if ((engResult != null) && (engResult.NewRecord != null) && (engResult.NewRecord.FilesId != null) && (engResult.NewRecord.FilesId.Count > 0))
                Id = engResult.NewRecord.FilesId[0];

            if (!String.IsNullOrEmpty(sErrorMessage))
            {
                AddError(sErrorMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Renommer le modèle de mail actuel
        /// </summary>
        /// <param name="newLabel">Le nouveau libellé</param>
        public void Rename(string newLabel)
        {
            this.Label = newLabel;
            UpdateOrInsert();
        }

        /// <summary>
        /// Savoir s'il existe des modèles de mails dans la base portant le même libellé que celui passé en paramètre
        /// </summary>
        /// <param name="libelle">le libellé</param>
        /// <returns>libellé disponible ou pas ?</returns>  
        public Boolean Exists(String libelle)
        {
            try
            {
                OpenDal();

                // #38823 MCR GCH ajout 
                // recuperation dans la table DESC pour  File=MAILTEMPLATE et Field=Label
                // et DESCID=la valeur de l'enumeration MailTemplateField.LABEL (descid=107001) 
                // de : la valeur de la colonne : Unicode par la propriete : FieldLite fl.Unicode de type Boolean

                FieldLite fl = new FieldLite(MailTemplateField.LABEL.GetHashCode());

                string sErr = string.Empty;
                fl.ExternalLoadInfo(_dal, out sErr);
                if (sErr.Length > 0)
                {
                    AddError("erreur recuperation du FielLite du champ LABEL " + sErr);
                    return false;
                }

                //L'existance d'un modèle ne prends pas en compte le createur, on se refere seulement au libellé (à décommenter si changement)
                StringBuilder sbQuery = new StringBuilder("SELECT COUNT([MAILTEMPLATE].[MailTemplateId]) FROM [MAILTEMPLATE]")
                    .Append(" WHERE  [MAILTEMPLATE].[LABEL] LIKE @LIBELLE ");
                if (this._nId > 0)
                    sbQuery.Append(" AND [MAILTEMPLATE].[MailTemplateId] <> @ID ");
                if (this._nTab > 0)
                    sbQuery.Append(" AND [MAILTEMPLATE].[TAB] = @TAB ");
                if (this._mtType != TypeMailTemplate.MAILTEMPLATE_UNDEFINED)
                    sbQuery.Append(" AND [MAILTEMPLATE].[TYPE] = @MTTYPE ");

                RqParam ExistsRq = new RqParam(sbQuery.ToString());

                ExistsRq.AddInputParameter("@LIBELLE", (fl.Unicode) ? System.Data.SqlDbType.NVarChar : System.Data.SqlDbType.VarChar, libelle);

                if (this._nId > 0)
                    ExistsRq.AddInputParameter("@ID", System.Data.SqlDbType.Int, this._nId);
                if (this._nTab > 0)
                    ExistsRq.AddInputParameter("@TAB", System.Data.SqlDbType.Int, _nTab);
                if (this._mtType != TypeMailTemplate.MAILTEMPLATE_UNDEFINED)
                    ExistsRq.AddInputParameter("@MTTYPE", System.Data.SqlDbType.Int, _mtType.GetHashCode());

                String sErrorMessage;

                int count = _dal.ExecuteScalar<Int32>(ExistsRq, out sErrorMessage);
                if (sErrorMessage.Length > 0)
                    throw new Exception(sErrorMessage);


                return count > 0;

            }
            catch
            {
                throw;
            }
            finally
            {
                CloseDal();
            }
        }

        /// <summary>
        /// On duplique l'objet en cours en dupliquant les perm
        /// </summary>
        /// <returns></returns>
        public eMailingTemplate Clone()
        {
            eMailingTemplate clonedTpl = new eMailingTemplate(this._pref);

            clonedTpl.Label = this.Label;
            clonedTpl.Body = this.Body;
            clonedTpl.Subject = this.Subject;
            // AABBA tache #1 940
            clonedTpl.Preheader = this.Preheader;

            clonedTpl.Tab = this.Tab;

            clonedTpl.ModifiedOn = this.ModifiedOn;
            clonedTpl.ModifiedBy = this.ModifiedBy;

            try
            {
                OpenDal();

                clonedTpl.ViewPerm = this.ViewPerm.Clone(_dal);
                clonedTpl.UpdatePerm = this.UpdatePerm.Clone(_dal);
            }
            finally { CloseDal(); }

            return clonedTpl;
        }

        /// <summary>
        /// Savoir si on a changé le nom de modèle
        /// </summary>
        /// <param name="_sLabel">Nouveau label</param>
        /// <returns>Vrai/Faux</returns>
        public bool IsLabelModified(string _sLabel)
        {
            return !Label.ToLower().Equals(_sLabel.ToLower());
        }

        /// <summary>
        /// Charge les PJ liées au modèle
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public Boolean LoadAttachments(out String error)
        {
            error = String.Empty;
            _listPJ = new List<int>();

            Boolean bOk = false;

            String query = "SELECT PjId FROM PJ WHERE FileId = @id AND [FILE] = 'MAILTEMPLATE'";

            RqParam rqParam = new RqParam(query);
            rqParam.AddInputParameter("@id", SqlDbType.Int, this._nId);

            try
            {
                OpenDal();

                DataTableReaderTuned dtr = _dal.Execute(rqParam, out error);

                if (String.IsNullOrEmpty(error))
                {
                    while (dtr.Read())
                    {
                        _listPJ.Add(dtr.GetEudoNumeric("PjId"));
                    }

                    bOk = true;
                }

            }
            catch (Exception exc)
            {
                AddError(exc.Message, exc);
            }
            finally
            {
                CloseDal();
            }

            return bOk;
        }

        /// <summary>
        /// Suppression des annexes liées
        /// </summary>
        /// <returns></returns>
        public bool DeleteAttachments(out String error)
        {
            string devErr = string.Empty;

            if (LoadAttachments(out error))
            {
                String fileName = "";
                int fileType;
                eErrorContainer errorContainer;

                foreach (int pj in _listPJ)
                {
                    ePJTraitements.DeletefromPj(_pref, pj, this._nId, TableType.MAIL_TEMPLATE.GetHashCode(), out fileName, out fileType, out errorContainer);

                    if (errorContainer != null)
                    {
                        if (errorContainer.DebugMsg.Length > 0)
                            AddError(errorContainer.DebugMsg);

                        if (errorContainer.Msg.Length > 0)
                            AddError(errorContainer.Msg);
                    }

                    // Suppression du fichier sur le disque
                    if (fileName.Length > 0 && ePJ.IsPhysicalFile(fileType))
                    {
                        error = ePJTraitements.DeletePjFromDisk(_pref.GetBaseName, fileName);
                        AddError(error);
                    }
                }

                return true;

            }

            return false;

        }
    }
}