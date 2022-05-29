using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.renderer;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Com.Eudonet.Core.Model;
using System.IO;
using Com.Eudonet.Xrm.classes;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eMailingTemplateManager
    /// </summary>
    public class eMailingTemplateManager : eEudoManager
    {
        private enum Operation
        {
            /// <summary>
            /// Aucune action, comportement par défaut si aucune action n'est transmise au Manager
            /// Ce cas ne devrait jamais arriver sans provoquer une erreur de paramètre incorrect.
            /// </summary>
            NONE = 0,
            /// <summary>
            /// Sauvegarder un nouveau Modele(Template)
            /// </summary>
            ADD = 1,
            /// <summary>
            /// Renommer le Modele(Template)
            /// </summary>
            RENAME = 2,
            /// <summary>
            /// Mettre le Modele(Template) à jour
            /// </summary>
            UPDATE = 3,
            /// <summary>
            /// Dupliquer le Modele(Template)
            /// </summary>
            CLONE = 4,
            /// <summary>
            /// Supprimer le Modele(Template)
            /// </summary>
            DELETE = 5,
            /// <summary>
            /// Afficher le Modele(Template)
            /// </summary>
            DISPLAY = 6,
            /// <summary>
            /// Charge un modèle
            /// </summary>
            LOAD = 7,
            /// <summary>Affichage de la fenêtre enregistrer sous</summary>
            DIALOG_SAVEAS = 8,
            /// <summary>Mettre le modèle par défaut pour l'utilisateur</summary>
            SET_DEFAULT = 9,
            /// <summary>Afficher l'infobulle du Modele(Template)</summary>
            TOOLTIP = 10
        }
        private enum ErroCode
        {
            NO_ERROR = 0,
            NAME_EXIST = 1,
            EMPTY_NAME = 2,
            EXCEPTION_ERROR = 3
        }

        //Valeurs du modèle 
        int _iMailTemplateId = 0;
        int _iTypeTemplate = 2;
        string _sLabel = string.Empty;
        string _sSubject = string.Empty;
        // AABBA tache #1 940
        string _sPreheader = string.Empty;
        string _sBody = string.Empty;
        string _sBodyCss = string.Empty;
        int _iTab = 106000; //TODO _iTab doit conteneir la table d'ou vient    
        int _iModifiedBy = 0;
        TypeMailTemplate _mtType = TypeMailTemplate.MAILTEMPLATE_UNDEFINED;
        //bool _bSetDefault = false; // Met-on le modèle en modèle par défaut ou le contraire ?

        //Mise a jour, renommage ...
        Operation _operation = Operation.NONE;

        //Gestion des permissions
        ePermission.PermissionMode viewPermMode = ePermission.PermissionMode.MODE_NONE;
        ePermission.PermissionMode updatePermMode = ePermission.PermissionMode.MODE_NONE;
        int _viewPermId = 0;
        int _updatePermId = 0;
        string _viewPermUsersId = string.Empty;
        string _updatePermUsersId = string.Empty;
        int _viewPermLevel = 0;
        int _updatePermLevel = 0;
        bool _bViewPerm = false;
        bool _bUpdatePerm = false;

        //Modèle public
        bool _bPublic = true; // #41134 CRU : Par défaut en public pour l'instant

        //Gestion des erreurs
        string _sErr = string.Empty;
        ErroCode _errCode = ErroCode.NO_ERROR;

        //Modèle a utiliser pour effectuer l'operation demandée
        eMailingTemplate _oMailTemplate;

        // Liste des PJ        
        List<int> _pjIds = new List<int>();

        // Savoir si on copie les annexes depuis le modèle vers la campagne mail
        bool _bClonePjCampaign = false;

        // Identifiant de la camapgne mail pour pour la duplication des pjs
        int _nCampaignId = 0;

        /// <summary>
        /// Gestion des traitements asynchrones de l'envoi d'e-mails ou SMS
        /// </summary>
        protected override void ProcessManager()
        {
            RetrieveParams();
            RunOperation();
            RenderXmlResponse();
        }

        /// <summary>
        /// Récuperer les params de Querystring
        /// </summary>
        private void RetrieveParams()
        {
            #region Récuperation des paramètres du modèle

            if (_context.Request.Form.AllKeys.Contains("lbl") && _context.Request.Form["lbl"] != null)
                _sLabel = _context.Request.Form["lbl"].ToString();

            if (_context.Request.Form.AllKeys.Contains("operation") && _context.Request.Form["operation"] != null)
                _operation = (Operation)eLibTools.GetNum(_context.Request.Form["operation"].ToString());

            if (_context.Request.Form.AllKeys.Contains("MailTemplateId") && _context.Request.Form["MailTemplateId"] != null)
                _iMailTemplateId = eLibTools.GetNum(_context.Request.Form["MailTemplateId"].ToString());

            if (_context.Request.Form.AllKeys.Contains("tab") && _context.Request.Form["tab"] != null)
                _iTab = eLibTools.GetNum(_context.Request.Form["tab"].ToString());

            if (_context.Request.Form.AllKeys.Contains("obj") && _context.Request.Form["obj"] != null)
                _sSubject = _context.Request.Form["obj"].ToString();

            //AABBA tache #1 940
            if (_context.Request.Form.AllKeys.Contains("preheader") && _context.Request.Form["preheader"] != null)
                _sPreheader = _context.Request.Form["preheader"].ToString();

            if (_context.Request.Form.AllKeys.Contains("body") && _context.Request.Form["body"] != null)
                _sBody = _context.Request.Form["body"].ToString();

            if (_context.Request.Form.AllKeys.Contains("bodyCss") && _context.Request.Form["bodyCss"] != null)
                _sBodyCss = _context.Request.Form["bodyCss"].ToString();

            if (_context.Request.Form.AllKeys.Contains("tplType") && _context.Request.Form["tplType"] != null)
                _iTypeTemplate = eLibTools.GetNum(_context.Request.Form["tplType"].ToString());

            if (_context.Request.Form.AllKeys.Contains("tplTypeDb") && _context.Request.Form["tplTypeDb"] != null)
            {
                int nMailTemplateType = eLibTools.GetNum(_context.Request.Form["tplTypeDb"].ToString());

                _mtType = eLibTools.GetEnumFromCode<TypeMailTemplate>(nMailTemplateType);

                    /*
                if (nMailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAIL.GetHashCode())
                    _mtType = TypeMailTemplate.MAILTEMPLATE_EMAIL;
                if (nMailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAILING.GetHashCode())
                    _mtType = TypeMailTemplate.MAILTEMPLATE_EMAILING;
                    */
            }

            // En création, les annexes du modèle de mail ont un fileId=0, on les récupère
            // // pour les attacher au modèle en cours de création
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL && _iMailTemplateId == 0)
            {
                eMailTemplatePjList pjList = new eMailTemplatePjList(_pref, TableType.MAIL_TEMPLATE.GetHashCode(), 0);
                if (pjList.Generate())
                    _pjIds = pjList.DicoPj.Keys.ToList();
            }
            else
            {
                // Liste des pj à ajouter ou à cloner dans le templateMail
                _pjIds = _requestTools.GetRequestIntListFormKeyS(";", "pjids");
            }

            // Si on sauvegarde la compagne mail comme modèle, on duplique les entrées dans pj, car les pjids sont liés à la campagne mail
            //_bClonePjCampaign = _requestTools.GetRequestFormKeyB("clonePjIds") ?? false;

            // identifiant de la campagne mail dans laquelle ce modèle est utilisé
            _nCampaignId = _requestTools.GetRequestFormKeyI("campaignId") ?? 0;



            //if (_context.Request.Form.AllKeys.Contains("bSetDefault") && _context.Request.Form["bSetDefault"] != null)
            //    _bSetDefault = eLibTools.Getbool(_context.Request.Form["bSetDefault"].ToString());

            #endregion

            #region Récuperation des permissions

            #region View
            if (_context.Request.Form.AllKeys.Contains("viewpermid") && _context.Request.Form["viewpermid"] != null)
                _viewPermId = eLibTools.GetNum(_context.Request.Form["viewpermid"].ToString());

            if (_context.Request.Form.AllKeys.Contains("viewpermmode") && _context.Request.Form["viewpermmode"] != null)
                viewPermMode = (ePermission.PermissionMode)eLibTools.GetNum(_context.Request.Form["viewpermmode"].ToString());

            if (_context.Request.Form.AllKeys.Contains("viewpermusersid") && _context.Request.Form["viewpermusersid"] != null)
                _viewPermUsersId = _context.Request.Form["viewpermusersid"].ToString();

            if (_context.Request.Form.AllKeys.Contains("viewpermlevel") && _context.Request.Form["viewpermlevel"] != null)
                _viewPermLevel = eLibTools.GetNum(_context.Request.Form["viewpermlevel"].ToString());

            if (_context.Request.Form.AllKeys.Contains("viewperm") && _context.Request.Form["viewperm"] != null)
                _bViewPerm = _context.Request.Form["viewperm"].ToString().Equals("1");
            #endregion

            #region update

            if (_context.Request.Form.AllKeys.Contains("updatepermid") && _context.Request.Form["updatepermid"] != null)
                _updatePermId = eLibTools.GetNum(_context.Request.Form["updatepermid"].ToString());

            if (_context.Request.Form.AllKeys.Contains("updatepermmode") && _context.Request.Form["updatepermmode"] != null)
                updatePermMode = (ePermission.PermissionMode)eLibTools.GetNum(_context.Request.Form["updatepermmode"].ToString());

            if (_context.Request.Form.AllKeys.Contains("updatepermusersid") && _context.Request.Form["updatepermusersid"] != null)
                _updatePermUsersId = _context.Request.Form["updatepermusersid"].ToString();

            if (_context.Request.Form.AllKeys.Contains("updatepermlevel") && _context.Request.Form["updatepermlevel"] != null)
                _updatePermLevel = eLibTools.GetNum(_context.Request.Form["updatepermlevel"].ToString());

            if (_context.Request.Form.AllKeys.Contains("updateperm") && _context.Request.Form["updateperm"] != null)
                _bUpdatePerm = _context.Request.Form["updateperm"].ToString().Equals("1");

            #endregion

            #region public
            if (_context.Request.Form.AllKeys.Contains("public") && _context.Request.Form["public"] != null)
                _bPublic = eLibTools.GetNum(_context.Request.Form["public"].ToString()) == 1;

            #endregion

            #endregion
        }

        /// <summary>
        /// Execute l'action demandée 
        /// </summary>
        private void RunOperation()
        {
            _oMailTemplate = new eMailingTemplate(_pref);

            switch (_operation)
            {
                case Operation.ADD:
                    AddNewTemplate();
                    break;
                case Operation.RENAME:
                    RenameTemplate();
                    break;
                case Operation.DELETE:
                    DeleteTemplate();
                    break;
                case Operation.UPDATE:
                    UpdateTemplate();
                    break;
                case Operation.LOAD:
                    LoadTemplate();
                    break;
                case Operation.DIALOG_SAVEAS:
                    LoadTemplate();
                    ShowDialogSaveAs();
                    break;
                case Operation.SET_DEFAULT:
                    SetDefaultTemplate();
                    break;
                case Operation.TOOLTIP:
                    GetTemplateDescription();
                    break;
                default:
                    break;
            }
        }



        /// <summary>
        /// Ajout d'un nouveau modèle
        /// </summary>
        private void AddNewTemplate()
        {
            if (_sLabel.Length == 0)
            {
                _errCode = ErroCode.EMPTY_NAME;
                return;
            }

            _oMailTemplate.Id = 0;

            _oMailTemplate.Label = this._sLabel;
            _oMailTemplate.Body = this._sBody;
            _oMailTemplate.BodyCss = this._sBodyCss;
            _oMailTemplate.Subject = this._sSubject;
            ///AABBA tache #1 940
            _oMailTemplate.Preheader = this._sPreheader;
            _oMailTemplate.Tab = _iTab;
            _oMailTemplate.CreatedBy = _pref.User.UserId;
            _oMailTemplate.ModifiedBy = this._iModifiedBy;
            _oMailTemplate.MailTemplateType = this._mtType;
            // #41134 CRU 
            if (_bPublic)
                _oMailTemplate.Owner_User = 0;
            else if (_oMailTemplate.Owner_User <= 0)
                _oMailTemplate.Owner_User = _pref.User.UserId;

            if (_oMailTemplate.Exists(_sLabel))
            {
                _errCode = ErroCode.NAME_EXIST;
                return;
            }
            _viewPermId = 0;
            _updatePermId = 0;
            HandlePermission();

            eRightMailTemplate traitRight = new eRightMailTemplate(_pref);
            if (traitRight.CanAddNewItem())
            {
                if (!_oMailTemplate.Save())
                    SendError("AddNewTemplate > _oMailTemplate.Save", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);
                else
                {
                    //KHA le 8 mars 2018 : lorsqu'on rajoute des PJ au moment de la création du modèle, ceux ci ne sont pas pris en compte.
                    //CNA le 6 Aout 2018 : Je remet la condition, car maintenant pour les modèles d'emailing, les PJ sont créées par le parseur de liens
                    if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL && _pjIds?.Count > 0)
                        AddPJListToTemplate(_oMailTemplate.Id);
                }
            }
            else
            {
                SendError("AddNewTemplate > eRightMailTemplate.CanAddNewItem", "Pas les droits d'ajout");
            }

        }

        /// <summary>
        /// Renomme un modele d'emailing
        /// </summary>
        private void RenameTemplate()
        {
            if (_sLabel.Length == 0)
            {
                _errCode = ErroCode.EMPTY_NAME;
                return;
            }
            //On recharge le modèle on le renomme puis on le sauvegarde
            if (!_oMailTemplate.Load(_iMailTemplateId))
                SendError("RenameTemplate > Load", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);

            if (_oMailTemplate.Exists(_sLabel))
                _errCode = ErroCode.NAME_EXIST;
            else
                _oMailTemplate.Rename(_sLabel);
            if (!string.IsNullOrEmpty(_oMailTemplate.ErrorMessage) || !string.IsNullOrEmpty(_oMailTemplate.ErrorDev))
                SendError("RenameTemplate", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);
        }

        /// <summary>
        /// supprime un modèle d emailing
        /// </summary>
        private void DeleteTemplate()
        {
            _oMailTemplate.Id = _iMailTemplateId;
            if (!_oMailTemplate.Delete())
                SendError("DeleteTemplate > Delete", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);
        }

        /// <summary>
        /// Mise a jour d'un modèle
        /// </summary>
        private void UpdateTemplate()
        {
            if (_sLabel.Length == 0)
            {
                _errCode = ErroCode.EMPTY_NAME;
                return;
            }

            //On recharge le modèle on le renomme puis on le sauvegarde
            if (!_oMailTemplate.Load(_iMailTemplateId))
                SendError("UpdateTemplate > Load", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);

            if (_oMailTemplate.Exists(this._sLabel))
            {
                _errCode = ErroCode.NAME_EXIST;
                return;
            }

            //Si le nom est différent de l'ancien alors on crée un nouveau modèle en dupliquant celui-ci   
            if (_oMailTemplate.IsLabelModified(this._sLabel))
            {
                _oMailTemplate.Id = 0;
                _oMailTemplate.Owner_User = 0;
                _viewPermId = 0;
                _updatePermId = 0;
            }

            //Correction NHA : Bug #72317
            //Droit sur les modèles email : en cas des droits insuffisants, le mail ne sera pas sauvegardé
            bool bAllowed;
            // Niveau uniquement
            bool bLevelAllowed = (_pref.User.UserLevel >= _oMailTemplate.UpdatePerm.PermLevel);
            // Utilisateur uniquement
            bool bUserAllowed = string.Concat(";", _oMailTemplate.UpdatePerm.PermUser, ";").Contains(string.Concat(";", _pref.User.UserId, ";")); //_oMailTemplate.UpdatePerm.PermUser or _updatePermUsersId
            bUserAllowed = bUserAllowed || string.Concat(";", _oMailTemplate.UpdatePerm.PermUser, ";").Contains(string.Concat(";G", _pref.User.UserGroupId, ";"));

            switch ((PermissionMode)_oMailTemplate.UpdatePerm.PermMode)
            {
                case PermissionMode.MODE_LEVEL_ONLY:
                    bAllowed = bLevelAllowed;
                    break;
                case PermissionMode.MODE_USER_ONLY:
                    bAllowed = bUserAllowed;
                    break;
                case PermissionMode.MODE_USER_AND_LEVEL:
                    bAllowed = (bLevelAllowed || bUserAllowed);
                    break;
                case PermissionMode.MODE_USER_OR_LEVEL:
                    bAllowed = (bLevelAllowed || bUserAllowed);
                    break;
                default:
                    bAllowed = false;
                    break;
            }
            //L'utilisateur n'a pas les droits suffisants pour modifier le modèle          
                if (_oMailTemplate.Id != 0 && _oMailTemplate.UpdatePerm.HasPerm && !bAllowed)
                {
                _errCode = ErroCode.EXCEPTION_ERROR;
                _sErr = eResApp.GetRes(_pref, 8325);               
                LaunchError();
            }
            else
            {
                HandlePermission();
                _oMailTemplate.Label = this._sLabel;
                _oMailTemplate.Body = this._sBody;
                _oMailTemplate.BodyCss = this._sBodyCss;
                _oMailTemplate.Subject = this._sSubject;
                _oMailTemplate.Tab = _iTab;
                _oMailTemplate.CreatedBy = _pref.User.UserId;
                _oMailTemplate.ModifiedBy = this._iModifiedBy;
                _oMailTemplate.MailTemplateType = this._mtType;
                _oMailTemplate.Preheader = this._sPreheader;
                //SHA : tâche #2 047
                if (this._sPreheader.Length > eLibConst.MAX_PREHEADER_LENGTH)
                    throw new eMailingException(eErrorCode.ERROR_MAX_LENGTH_PREHEADER);

                if (this._bPublic)
                    _oMailTemplate.Owner_User = 0;
                else if (_oMailTemplate.Owner_User <= 0)
                    _oMailTemplate.Owner_User = _pref.User.UserId;

                if (!_oMailTemplate.Save())
                    SendError("UpdateTemplate > _oMailTemplate.Save", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);
            }
        }

        /// <summary>
        /// Ajoute la liste des PJ à un modèle de mail
        /// </summary>
        /// <param name="templateId">identifiant du template</param>
        private void AddPJListToTemplate(int templateId)
        {
            string error = string.Empty;
            string devError = string.Empty;

            foreach (int pjId in _pjIds)
            {
                devError = ePJTraitementsLite.UpdatePjFileId(_pref, _pref.User, (int)TableType.MAIL_TEMPLATE, templateId, pjId, out error);
                if (!string.IsNullOrEmpty(devError))
                {
                    SendError("AddPJListToTemplate", devError, error);
                    break;
                }
            }
        }


        /// <summary>
        /// Fait une copie de l'entrée pj de campaign pour l'associer au modèle si elle n'existe pas
        /// </summary>
        /// <param name="id">MailTemplateId</param>
        /// <param name="pjIds"></param>
        private void saveTemplatePJ(int templateId)
        {
            if (templateId == 0)
                return;

            string usrMsg, devError;
            if (!ePJTraitementsLite.ClonePjCampaignList(_pref, _pref.User, _nCampaignId, templateId, _pjIds.Where(e => e > 0), out usrMsg, out devError))
            {
                if (!string.IsNullOrEmpty(devError))
                {
                    SendError("clonePJCampaignToTemplate", devError, usrMsg);
                }
            }
        }

        /// <summary>
        /// Affiche un message d'erreur stabndard à l'utilisateur et envoi le feedback avec les informations passées en paramètre
        /// </summary>
        /// <param name="sOrig">Zone ou survient l'erreur</param>
        /// <param name="sDevError">erreur détaillée à envoyer uniquement en feedback</param>
        /// <param name="sUserError">Erreur à afficher à l'utilisateur</param>
        private void SendError(string sOrig, string sDevError, string sUserError = "")
        {
            ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                (sUserError != "") ? sUserError : string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  //   titre
                string.Concat(sOrig, sDevError));
            LaunchError();
        }


        /// <summary>
        /// Charge un modèle utilisateur/personalisable
        /// </summary>
        private void LoadTemplate()
        {
            //user template
            if (_iTypeTemplate == 0)
                if (!_oMailTemplate.Load(_iMailTemplateId))
                    SendError("LoadTemplate > Load", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);



            //custom template
            if (_iTypeTemplate == 1)
                _oMailTemplate.LoadCustom(_iMailTemplateId);


            _mtType = _oMailTemplate.MailTemplateType;
        }

        /// <summary>
        /// Gere les permissions sur les modèles d'emailing actuels
        /// </summary>
        private void HandlePermission()
        {
            ePermission viewPerm = new ePermission(this._viewPermId, this.viewPermMode, this._viewPermLevel, this._viewPermUsersId);
            ePermission updatePerm = new ePermission(this._updatePermId, this.updatePermMode, this._updatePermLevel, this._updatePermUsersId);

            // TODO - Pas bien d'avoir un eudodal ici, faire le job de la méthode dans l'objet métier _oMailTemplate ?
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();
                //si les visus ne sont pas activées, on les supprime de la base
                if (_viewPermId > 0 && !_bViewPerm)
                        viewPerm.Delete(dal);
                    else if (_bViewPerm)
                        viewPerm.Save(dal);
                    else
                        viewPerm.Reset();
                    this._viewPermId = viewPerm.PermId;

                    //pareil pour les modifs
                    if (_updatePermId > 0 && !_bUpdatePerm)
                        updatePerm.Delete(dal);
                    else if (_bUpdatePerm)
                        updatePerm.Save(dal);
                    else
                        updatePerm.Reset();
                    this._updatePermId = updatePerm.PermId;           
            }
            finally
            {
                dal.CloseDatabase();
            }

            _oMailTemplate.ViewPerm = viewPerm;
            _oMailTemplate.UpdatePerm = updatePerm;
        }

        /// <summary>
        /// Construit et renvoit le xml de réponse
        /// </summary>
        private void RenderXmlResponse()
        {
            XmlDocument _xmlDocReturn = new XmlDocument();

            #region XML Declartion, UTF8 ..etc

            XmlNode baseResultNode;
            _xmlDocReturn.AppendChild(_xmlDocReturn.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = NewNode("result", _xmlDocReturn, _xmlDocReturn);

            #endregion

            #region initialisation du retour XML (structure)

            XmlNode xmlNodeSuccessNode = NewNode("success", baseResultNode, _xmlDocReturn);

            //Gestion des erreurs
            XmlNode xmlNodeErrorCodeNode = NewNode("ErrorCode", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeErrorDescription = NewNode("ErrorDescription", baseResultNode, _xmlDocReturn);

            //Noeuds du modèle
            XmlNode xmlNodeOperation = NewNode("operation", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeTable = NewNode("table", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeMailTemplateId = NewNode("iMailTemplateId", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeSuBject = NewNode("subject", baseResultNode, _xmlDocReturn);
            //SHA : tâche #1 939
            XmlNode xmlNodePreheader = NewNode("preheader", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody = NewNode("body", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody_css = NewNode("body_css", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeBody_html = NewNode("body_html", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeName = NewNode("mailTplname", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeType = NewNode("mailTplType", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeMailTemplateType = NewNode("mailTplTypeDb", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodePj = NewNode("templatePjIds", baseResultNode, _xmlDocReturn);

            //Permissions
            XmlNode xmlNodeViewPermId = NewNode("ViewPermId", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodeUpdatePermId = NewNode("UpdatePermId", baseResultNode, _xmlDocReturn);

           
            #endregion

            #region remplit le xml et fait le rendu

            bool bError = CheckError();

            xmlNodeSuccessNode.InnerText = bError ? "0" : "1";

            if (bError)
            {
                //erreur
                xmlNodeName.InnerText = _sLabel;
                xmlNodeErrorCodeNode.InnerText = _errCode.GetHashCode().ToString();
                xmlNodeErrorDescription.InnerText = this._sErr;
            }
            else
            {
                //Succes
                xmlNodeMailTemplateId.InnerText = this._oMailTemplate.Id.ToString();
                xmlNodeSuBject.InnerText = this._oMailTemplate.Subject;
                //SHA : tâche #1 939
                xmlNodePreheader.InnerText = this._oMailTemplate.Preheader;
                xmlNodeBody.InnerText = this._oMailTemplate.Body;

                // Backlog #43 - Traitements selon si le modèle est compatible grapesjs ou non
                bool bIsHTMLTemplateEditorCompatible =
                    xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_SIGNATURE_PLACEHOLDER) ||
                    xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER) ||
                    xmlNodeBody.InnerText.Contains("data-highlightable") ||
                    xmlNodeBody.InnerText.Contains("gjs");

                //Inser la signature si besoin
                string sUserSign;
                if (_pref.GetConfig(eLibConst.PREF_CONFIG.EMAILAUTOADDSIGN).Equals("1"))
                {
                    if (!eUser.GetFieldValue<string>(_pref, _pref.User.UserId, "UserSignature", out sUserSign))
                        sUserSign = string.Empty;
                    else if (sUserSign.Length > 0)
                    {
                        // Backlog #268 - Insertion dans un placeholder des modèles de mail, si défini, et non plus systématiquement à la fin
                        if (xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_SIGNATURE_PLACEHOLDER))
                            xmlNodeBody.InnerText = xmlNodeBody.InnerText.Replace(eLibConst.EMAILTEMPLATE_SIGNATURE_PLACEHOLDER, sUserSign);
                        // Sinon, ajout en fin de mail, comme sur les versions précédentes
                        else
                        {
                            /*
                            if (this._oMailTemplate.Body_HTML)
                                xmlNodeBody.InnerText += "<br/>";
                            else
                                xmlNodeBody.InnerText += Environment.NewLine;
                            */

                            // Backlog #431 - la signature est, dans ce cas de figure (modèle sans placeholder), insérée à la toute fin du corps du mail,
                            // sans balise englobante, ce qui empêche son édition via grapesjs. On l'encapsule donc dans des balises <div> si le corps est en HTML
                            // US 1 552 > autres temps, autres moeurs, on désactive les signatures quand il n'y a pas de placeholder pour la signature. GLA
                            //if (this._oMailTemplate.Body_HTML)
                            //    xmlNodeBody.InnerText += "<div data-gjs-type=\"text\">" + sUserSign + "</div>";
                            //else
                            //    xmlNodeBody.InnerText += sUserSign;
                        }
                    }
                }
                // Backlog #268 - Si le placeholder n'a pas été utilisé (ex : pas de signature, ou auto-ajout désactivé), on le supprime du code HTML inséré
                if (xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_SIGNATURE_PLACEHOLDER))
                    xmlNodeBody.InnerText = xmlNodeBody.InnerText.Replace(eLibConst.EMAILTEMPLATE_SIGNATURE_PLACEHOLDER, string.Empty);

                // Backlog #43 - On rajoute une div englobante avec attribut forçant son édition en mode Texte, sur le modèle, si on suspecte qu'il provient d'un autre
                // éditeur que grapesjs, dont les éléments texte ne pourront pas être édités s'ils ne sont pas dans un conteneur (idem backlog #431 plus haut)
                // Backlog #446 - cela semble au contraire poser problème pour éditer les mails après validation. Mis en commentaire en attendant une meilleure
                // solution
                /*
                if (!bIsHTMLTemplateEditorCompatible && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor))
                    xmlNodeBody.InnerText = "<div data-gjs-type=\"text\">" + xmlNodeBody.InnerText + "</div>";
                */

                // #47151 : Nettoyage des caractères XML invalides
                xmlNodeBody.InnerText = eLibTools.CleanXMLChar(xmlNodeBody.InnerText);

                xmlNodeName.InnerText = this._oMailTemplate.Label;

                xmlNodeBody_css.InnerText = this._oMailTemplate.BodyCss;

                // Backlog #267 - Injection de CSS spécifique à l'éditeur de mails en tant que style inline, pour qu'elle soit transmiss dans le mail final
                // Cette CSS peut notamment être utilisée pour gérer le responsive design des blocs de composants ajoutés par l'utilisateur via l'éditeur
                // Backlog #304 - On l'insère désormais systématiquement, que ce soit dans les modèles de mail utilisateur, ou les modèles prédéfinis
                string sEditorCss = string.Empty;
                try
                {

                    if (this._oMailTemplate.Id == 0 || _iTypeTemplate == 1)
                    {
                        string cssFile = _context.Server.MapPath(string.Concat(_context.Request.ApplicationPath, "/", string.Concat(eTools.WebPathCombine("themes", "default", "css", "grapesjs", "grapesjs-eudonet"), ".css")));
                        TextReader tr = new StreamReader(cssFile);
                        sEditorCss = tr.ReadToEnd();
                        tr.Close();
                        tr.Dispose();
                    }


                }
                catch { }

                // Si le modèle prédéfini a un "emplacement" réservé à l'injection de cette CSS, on l'utilise
                if (
                    xmlNodeBody_css.InnerText.Contains(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER)// ||
                                                                                                     //xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER)
                )
                {
                    // On insère dans la CSS du modèle si définie
                    // Pas dans le code HTML du mail, car ça n'est pas supporté par la plupart des clients mail
                    //if (xmlNodeBody_css.InnerText.Contains(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER))
                    xmlNodeBody_css.InnerText = xmlNodeBody_css.InnerText.Replace(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER, sEditorCss);
                    //else if (xmlNodeBody.InnerText.Contains(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER))
                    //xmlNodeBody.InnerText = xmlNodeBody.InnerText.Replace(eLibConst.EMAILTEMPLATE_EDITORCSS_PLACEHOLDER, string.Concat("<style>", sEditorCss, "</style>"));
                }
                // S'il n'y a pas d'emplacement réservé (cas d'un modèle édité par l'utilisateur via l'éditeur de modèles, ou ancien modèle prédéfini non mis à jour),
                // on l'injecte à la suite de la CSS existante
                else
                {

                    xmlNodeBody_css.InnerText = string.Concat(xmlNodeBody_css.InnerText, Environment.NewLine, sEditorCss);
                }



                xmlNodeBody_html.InnerText = (this._oMailTemplate.Body_HTML ? "1" : "0");

                xmlNodeType.InnerText = _iTypeTemplate.ToString();
                xmlNodeMailTemplateType.InnerText = _mtType.GetHashCode().ToString();
                xmlNodePj.InnerText = string.Join(";", this._oMailTemplate.ListTemplatePJ);


                // Champs à mettre à jour sur l'interface
                int nParentTab = TableType.CAMPAIGN.GetHashCode();
                int nParentFieldIsHTML = CampaignField.ISHTML.GetHashCode();
                int nParentFieldSubject = CampaignField.SUBJECT.GetHashCode();
                //SHA : tâche #1 939
                int nParentFieldPreheader = CampaignField.PREHEADER.GetHashCode();
                int nParentFieldBody = CampaignField.BODY.GetHashCode();
                // Sur la fenêtre de mails unitaires, le nom des champs est de la forme COL_DescIDFichier_DescIDChampv7_0_0_0
                // Objet). On ne peut donc pas, comme sur l'emailing (campagne) utiliser les DescIDs de la nouvelle table XRM
                if (this._oMailTemplate.MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAIL)
                {
                    nParentTab = this._oMailTemplate.Tab;
                    nParentFieldIsHTML = nParentTab + MailField.DESCID_MAIL_ISHTML.GetHashCode();
                    nParentFieldSubject = nParentTab + MailField.DESCID_MAIL_OBJECT.GetHashCode();
                    //SHA : tâche #1 939
                    nParentFieldPreheader = nParentTab + MailField.DESCID_MAIL_PREHEADER.GetHashCode();
                    nParentFieldBody = nParentTab + MailField.DESCID_MAIL_HTML.GetHashCode();
                }

                xmlNodeTable.InnerText = nParentTab.ToString();

                //retourne les ids des permissions
                xmlNodeViewPermId.InnerText = this._oMailTemplate.ViewPerm.PermId.ToString();
                xmlNodeUpdatePermId.InnerText = this._oMailTemplate.UpdatePerm.PermId.ToString();

                //ajout l'attribut descid pour body_html
                XmlAttribute xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = string.Concat("COL_", nParentTab, "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", nParentFieldIsHTML.ToString());
                xmlNodeBody_html.Attributes.Append(xmlDescId);

                //ajout l'attribut descid pour subject
                xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = string.Concat("COL_", nParentTab, "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", nParentFieldSubject.ToString());
                xmlNodeSuBject.Attributes.Append(xmlDescId);

                //SHA : tâche #1 939
                //ajout l'attribut descid pour preheader
                xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = string.Concat("COL_", nParentTab, "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", nParentFieldPreheader.ToString());
                xmlNodePreheader.Attributes.Append(xmlDescId);

                //ajout l'attribut descid pour body
                xmlDescId = _xmlDocReturn.CreateAttribute("id");
                xmlDescId.Value = string.Concat("edtCOL_", nParentTab, "_@descid_0_0_0");
                xmlDescId.Value = xmlDescId.Value.Replace("@descid", nParentFieldBody.GetHashCode().ToString());
                xmlNodeBody.Attributes.Append(xmlDescId);

                // Ajout attribut nbpj
                XmlAttribute xmlNbPj = _xmlDocReturn.CreateAttribute("nbpj");
                xmlNbPj.Value = this._oMailTemplate.ListTemplatePJ.Count.ToString();
                xmlNodePj.Attributes.Append(xmlNbPj);
            }


            RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });

            #endregion

        }

        /// <summary>
        /// Créer un nouveau noeud xml et l'ajoute au noeud parent
        /// </summary>
        /// <param name="Name">nom du noeud</param>
        /// <param name="ParentNode">Noeud parent</param>
        /// <returns></returns>
        private static XmlNode NewNode(string Name, XmlNode ParentNode, XmlDocument Creator)
        {
            XmlNode child = Creator.CreateElement(Name);
            ParentNode.AppendChild(child);
            return child;
        }

        /// <summary>
        /// Vérife s'il y a des erreurs
        /// </summary>
        /// <returns></returns>
        private bool CheckError()
        {
            return _errCode != ErroCode.NO_ERROR || _sErr.Length > 0;
        }


        public void ShowDialogSaveAs()
        {
            ePermissionRenderer rend = new ePermissionRenderer(_pref
                , sLabel: eResApp.GetRes(_pref, 6544), sName: _oMailTemplate.Label, bPublic: _oMailTemplate.Owner_User <= 0
                , viewPermId: _oMailTemplate.ViewPerm.PermId, updatePermId: _oMailTemplate.UpdatePerm.PermId
            );
            RenderResultHTML(rend.GetSaveAsBlock());
        }

        private void SetDefaultTemplate()
        {
            eLibConst.PREFADV prefParameter = eLibConst.PREFADV.UNDEFINED;
            if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL)
                prefParameter = eLibConst.PREFADV.DEFAULT_EMAILTEMPLATE;
            else if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAILING)
                prefParameter = eLibConst.PREFADV.DEFAULT_EMAILINGTEMPLATE;

            eLibTools.AddOrUpdatePrefAdv(_pref, prefParameter, _iMailTemplateId.ToString(), eLibConst.PREFADV_CATEGORY.UNDEFINED, _pref.UserId, _iTab);
        }

        private void GetTemplateDescription()
        {
            //user template
            if (!_oMailTemplate.Load(_iMailTemplateId))
                SendError("LoadTemplate > Load", _oMailTemplate.ErrorDev, _oMailTemplate.ErrorMessage);

            if(_oMailTemplate.Body.Length > 200)
                _oMailTemplate.Body = _oMailTemplate.Body.Substring(0,200);
        }
    }
}