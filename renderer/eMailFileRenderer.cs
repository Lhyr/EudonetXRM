using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EudoExtendedClasses;
using Com.Eudonet.Core.Model;
using System.Text.RegularExpressions;
using System.Net.Mail;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu des modes fiches
    /// </summary>
    public class eMailFileRenderer : eEditFileRenderer
    {
        #region PROPRIETES

        /// <summary>
        /// Table d'ou on vient
        /// </summary>
        protected Int32 _nTabFrom = 0;
        /// <summary>
        /// Largeur de la fenêtre E-mail
        /// </summary>
        protected Int32 _nWidth = 0;

        /// <summary>
        /// Hauteur de la fenêtre E-mail
        /// </summary>
        protected Int32 _nHeight = 0;

        /// <summary>
        /// Adresse du destinataire à préremplir par défaut (contact associé)
        /// </summary>
        protected String _strMailTo = String.Empty;

        /// <summary>
        /// Indique si la fenêtre a été déclenchée par le bouton Transférer
        /// </summary>
        protected Boolean _bMailForward = false;

        /// <summary>
        /// Indique si l'on édite un brouillon 
        /// </summary>
        protected Boolean _bMailDraft = false;

        /// <summary>
        /// Champ Date envoyé le ...(Correspond au DateSent dans campaigne)
        /// </summary>
        protected eFieldRecord _fldDate = null;

        /// <summary>
        /// Champ HTML ou TEXT
        /// </summary>
        protected eFieldRecord _fldIsHTML = null;

        /// <summary>
        /// Expediteur
        /// </summary>
        protected eFieldRecord _fldFrom = null;




        /// <summary>
        /// Destinataire(s)
        /// </summary>
        protected eFieldRecord _fldTo = null;

        /// <summary>
        /// Copie Conforme
        /// </summary>
        protected eFieldRecord _fldCc = null;

        /// <summary>
        /// Copie Conforme Invisible
        /// </summary>
        protected eFieldRecord _fldBcc = null;

        /// <summary>
        /// Nom apparent
        /// </summary>
        protected eFieldRecord _fldDisplayName = null;

        /// <summary>
        /// Répondre à
        /// </summary>
        protected eFieldRecord _fldReplyTo = null;

        /// <summary>
        /// Objet de mail
        /// </summary>
        protected eFieldRecord _fldSubject = null;

        //SHA : tâche #1 939
        /// <summary>
        /// Preheader du mail
        /// </summary>
        protected eFieldRecord _fldPreheader = null;

        /// <summary>
        /// Corps de mail
        /// </summary>
        protected eFieldRecord _fldBody = null;

        /// <summary>
        /// Historisé
        /// </summary>
        protected eFieldRecord _fldHisto = null;

        /// <summary>
        /// Confirmation lecture
        /// </summary>
        protected eFieldRecord _fldTracking = null;

        /// <summary>
        /// Lu
        /// </summary>
        protected eFieldRecord _fldRead = null;

        /// <summary>
        /// Statut
        /// </summary>
        protected eFieldRecord _fldStatus = null;

        /// <summary>
        /// SMS/MAIL
        /// </summary>
        protected eFieldRecord _fldMailSendType = null;

        /// <summary>
        /// Ajout de fonctions/variable js 
        /// </summary>
        protected StringBuilder _strScriptBuilder = null;

        protected Int32 _defaultTemplateID;

        protected eMailingTemplate _mailingTemplate;

        /// <summary>
        /// ALISTER => Demande #79357
        /// Permet la récupération des données
        /// </summary>
        protected List<eFieldRecord> headerFields;

        #endregion

        #region ACCESSEURS

        /// <summary>
        /// Indique si la fenêtre a été déclenchée par le bouton Transférer
        /// </summary>
        public Boolean MailForward
        {
            get { return _bMailForward; }
            set { _bMailForward = value; }
        }

        /// <summary>
        /// Indique si l'édition porte sur un brouillon de mail
        /// </summary>
        public Boolean MailDraft
        {
            get { return _bMailDraft; }
            set { _bMailDraft = value; }
        }

        #endregion

        #region CONSTRUCTEUR

        /// <summary>
        /// Crée un objet pour le rendu de la fiche E-mail
        /// </summary>
        /// <param name="pref">Preference user</param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <param name="nHeight">Hauteur du renderer</param>
        /// <param name="nWidth">Largeur du renderer</param>
        /// <param name="bIsSMS">Indique si on fait le rendu d'un fichier de type SMS ou non</param>
        public eMailFileRenderer(ePref pref, int nTab, Int32 nFileId, Int32 nWidth, Int32 nHeight, String strMailTo = "", bool bIsSMS = false)
            : base(pref, nTab, nFileId)
        {
            _nWidth = nWidth;
            _nHeight = nHeight;
            _rType = (bIsSMS ? RENDERERTYPE.SMSFile : RENDERERTYPE.MailFile);

            _strMailTo = strMailTo;

            this._strScriptBuilder = new StringBuilder();
        }

        #endregion

        #region REDIFINITION DES METHODES (DE RENDRER)

        /// <summary>
        /// Création et initialisation de l'objet eFile
        /// Surchage de la classe héritée
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            try
            {
                //Génération d'un objet "métier" de type file
                if (_rType == RENDERERTYPE.EditSMS || _rType == RENDERERTYPE.EditSMSMailing) // EditSMSMailing ne devrait toutefois pas utiliser ce renderer
                    _myFile = eFileSMS.CreateEditSMSFile(Pref, _tab, _nFileId, _dicParams);
                else
                    _myFile = eFileMail.CreateEditMailFile(Pref, _tab, _nFileId, _dicParams);

                _dicParams.TryGetValueConvert("globalaffect", out GlobalAffect);
                _dicParams.TryGetValueConvert("globalinvit", out GlobalInvit);

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eMailFileRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
                    if (_myFile.InnerException.GetType() == typeof(EudoFileNotFoundException))
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_FILE_NOT_FOUND;
                    }
                    else
                    {
                        _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                    }

                    return false;
                }

                // Récupération de l'ID du modèle par défaut
                // #58 412 : filtrage sur le UserID et le TabID, afin de ne récupérer que les modèles concernant l'utilisateur et l'onglet en cours
                _defaultTemplateID = 0;

                if (_myFile.FileId == 0)
                {
                    IDictionary<eLibConst.PREFADV, String> dicPrefAdv = eLibTools.GetPrefAdvValues(Pref,
                        new HashSet<eLibConst.PREFADV>() {
                            eLibConst.PREFADV.DEFAULT_EMAILTEMPLATE
                        },
                        Pref.UserId,
                        _tab);
                    String valueDefaultTplID = String.Empty;
                    dicPrefAdv.TryGetValue(eLibConst.PREFADV.DEFAULT_EMAILTEMPLATE, out valueDefaultTplID);
                    _defaultTemplateID = eLibTools.GetNum(valueDefaultTplID);

                    //KJE le 13/01/2020, numéro de demande: 71 547
                    //Dans le cas d'un transfert de mail, on ne charge pas le mail template (on teste sur _bMailForward)
                    if (_defaultTemplateID != 0 && !_bMailForward)
                    {
                        _mailingTemplate = new eMailingTemplate(Pref);
                        _mailingTemplate.Load(_defaultTemplateID);
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eMailFileRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// renseigne le contenu de la fiche mail/mailing (campaign en mode edition de mail dans l'assistant)
        /// </summary>
        /// <param name="sortedFields">Liste des champs triés</param>
        protected override void FillContent(List<eFieldRecord> sortedFields)
        {
            if (this.DicParams != null)
                this.DicParams.TryGetValueConvert("ntabfrom", out _nTabFrom, 0);
            // TOCHECK SMS - Cas lors de la MAJ - Passage de nTabFrom sous une autre variable utilisée par certains contexte
            if (_nTabFrom == 0 && this.DicParams != null)
                this.DicParams.TryGetValueConvert("parenttab", out _nTabFrom, 0);

            #region Analyse des champs

            //Tri la liste par disporder
            sortedFields.Sort(eFieldRecord.CompareByDisporder);

            // Les champs en disporder 0 sont des champs système à ne pas afficher
            // la modification de disporder à la source peut avoir des effets de bords,
            // il a donc été décidé de le retirer après construction du tableau
            List<eFieldRecord> systemFields = new List<eFieldRecord>();
            foreach (eFieldRecord field in sortedFields)
            {
                if (field.FldInfo.PosDisporder == 0)
                    systemFields.Add(field);
                else
                    break;
            }
            sortedFields.RemoveRange(0, systemFields.Count);

            Int32 nbColByLine = _myFile.ViewMainTable.ColByLine;

            //ALISTER => Demande #79357
            headerFields = GetHeaderFields(sortedFields, systemFields);

            // Suppression de tous les champs système de la liste globale des champs de la table
            // Ne resteront donc dans ce tableau que les champs utilisateur
            foreach (eFieldRecord fld in headerFields)
            {
                sortedFields.Remove(fld);
            }

            #endregion

            #region Fiche E-MAIL

            // Cas particulier pour les fiches E-mail dont le rendu est spécifique : on ajoute une barre de défilement sur le conteneur
            // Sur les autres modes popup, le contenu est encapsulé dans un "fileDivDetailsBkms" qui possède son propre design avec barre de défilement,
            // que l'on utilise pas ici
            // HDJ jai changé OverflowX : none en Hidden. Il apparassait quand meme avec none.
            PgContainer.Style.Add(HtmlTextWriterStyle.OverflowX, "hidden");
            PgContainer.Style.Add(HtmlTextWriterStyle.OverflowY, "auto");
            //PgContainer.Style.Add(HtmlTextWriterStyle.Height, "98%");

            #region Entête "Créez votre e-mail" / "Créez votre SMS"
            if (!ReadonlyRenderer && (
                RendererType == RENDERERTYPE.EditMail || RendererType == RENDERERTYPE.EditSMS ||
                RendererType == RENDERERTYPE.EditMailing || RendererType == RENDERERTYPE.EditSMSMailing
            ))
            {
                HtmlGenericControl lblCreateEmail = CreateHeaderLabel();
                PgContainer.Controls.Add(lblCreateEmail);


                if (RendererType == RENDERERTYPE.EditMailing

                    && eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.HTMLTemplateEditor)
                    && _ePref.ClientInfos.ClientOffer > 0
                    && eTools.IsMSBrowser
                    )
                {
                    HtmlGenericControl lblWarning = new HtmlGenericControl("span");
                    lblWarning.InnerText = eResApp.GetRes(Pref, 2225); // Une nouvelle version de l’éditeur d’email est disponible pour les navigateurs Chrome, Firefox et Safari
                    lblWarning.Attributes.Add("class", "mail-grapswarning-text");
                    lblCreateEmail.Controls.Add(lblWarning);
                }

            }

            #endregion

            #region Partie haute

            HtmlGenericControl fileTabMain = new HtmlGenericControl("div");
            fileTabMain.ID = "ftm_" + _myFile.ViewMainTable.DescId;
            PgContainer.Controls.Add(fileTabMain);

            Panel pnlMailMain = new Panel();
            pnlMailMain.ID = "mailDiv";
            pnlMailMain.CssClass = "md_mail-base";

            //Panel pnlMain = new Panel();
            Panel pnlLeft = new Panel();
            //pnlMain.Controls.Add(pnlLeft);

            //Table contenant les champ mail/mailing 
            System.Web.UI.WebControls.Table tab = new System.Web.UI.WebControls.Table();
            pnlLeft.Controls.Add(tab);

            tab.Style.Add(HtmlTextWriterStyle.Width, "99%");

            //On recuperer les champs de headers depuis la liste pour pouvoir les positionner correctement
            RetrieveFields(headerFields);

            //on fait le rendu des champs cachés
            RenderHiddenFields(pnlLeft);

            //on fait un rendu de c Tab.
            RenderFields(tab);

            //On ajoute du javascript 
            RenderJavaScript(pnlLeft);

            //Partie principale/haute
            pnlMailMain.Controls.Add(pnlLeft);

            fileTabMain.Controls.Add(pnlMailMain);

            #endregion

            #region Vérification des droits d'accès aux champs obligatoires
            /*
            MAB - #49 201 - Suppression de ce test qui pose problème dans les cas où on fait des envois via campagnes d'e-mailing, ainsi que pour la consultation des
            e-mails envoyés (un test ReadonlyRenderer ne suffit pas). Le champ "A" étant toujours à !RightIsUpdatable dans le cas des campagnes pour des raisons système,
            on ne conserve que le test côté JS pour vérifier la validité des e-mails saisis dans ce champ (et donc, le fait qu'il doive être rempli, par l'utilisateur
            ou le système)
            */
            /*
            if (!ReadonlyRenderer)
            {
                if (!_fldTo.RightIsUpdatable)
                {
                    this._sErrorMsg = _fldTo.FldInfo.Libelle;
                    this._nErrorNumber = QueryErrorType.ERROR_NUM_MAIL_FILE_NOT_FOUND; // ce code d'erreur sera utilisé pour remonter une erreur Utilisateur sur eFileDisplayer
                }
            }
            */
            #endregion

            #region Entête "Associez votre e-mail" (mail unitaire uniquement) ou "Associez votre SMS" (SMS unitaire uniquement)
            if (_rType != RENDERERTYPE.EditMailing && _rType != RENDERERTYPE.EditSMSMailing
                //SHA : Backlog 940 : masquage de l'entête si pas de champs de liaison
                && (_myFile.ViewMainTable.InterPP || _myFile.ViewMainTable.InterPM || _myFile.ViewMainTable.InterEVT))
            {
                HtmlGenericControl lblLinkEmail = new HtmlGenericControl("div");
                lblLinkEmail.InnerText = _myFile.ViewMainTable.EdnType != EdnType.FILE_SMS ? eResApp.GetRes(Pref, 6310) : " ";
                lblLinkEmail.Attributes.Add("class", "mail-head-text");
                PgContainer.Controls.Add(lblLinkEmail);
            }
            #endregion

            #region Champs de liaison (partie basse)
            if (PnlDetailsBkms != null)
            {
                if (_rType != RENDERERTYPE.EditMailing && _rType != RENDERERTYPE.EditSMSMailing)
                {
                    PnlDetailsBkms.CssClass = "divDetailsBkmsMail";
                    PgContainer.Controls.Add(PnlDetailsBkms);
                }
                else
                {
                    PnlDetailsBkms.CssClass = "";
                }
            }


            #endregion

            #endregion

        }

        /// <summary>
        /// Créée un label
        /// </summary>
        protected virtual HtmlGenericControl CreateHeaderLabel()
        {
            
            HtmlGenericControl lblCreateEmail = new HtmlGenericControl("div");
            //lblCreateEmail.InnerText = eResApp.GetRes(Pref, 6306);
            //lblCreateEmail.Attributes.Add("class", "mail-head-text");
            return lblCreateEmail;
        }

        /// <summary>
        /// REDIFINITION DE LA METHODE DE 'RenderBitFieldFormat' DE RENDERER, PAR DEFAUT ELLE AFFICHE UNE CHECKBOX: 
        /// Fait un rendu du champ de type Binaire en Label si l email en consultation
        /// en bouton radios si l email/emailing en modification pour le champ IsHtml
        /// </summary>
        /// <param name="rowRecord">Ligne de la liste a afficher</param>
        /// <param name="fieldRecord">Le champ binaire</param>
        /// <param name="sClassAction">classe CSS choisi pour l'element</param>
        /// <returns>Retourne le control généré pour la rubrique de type BIT (retourne un eCheckBoxCtrl ou un Panel avec des HtmlInputRadioButton)</returns>
        protected override WebControl RenderBitFieldFormat(eRecord rowRecord, eFieldRecord fieldRecord, ref String sClassAction)
        {
            //Si le champ n'est pas 'IsHTML' on fait un rendu de checkbox de base
            if (fieldRecord != _fldIsHTML)
                return base.RenderBitFieldFormat(rowRecord, fieldRecord, ref sClassAction);

            //N'affiche la case que si elle est liée à une fiche
            if (fieldRecord.FileId <= 0 && fieldRecord.FldInfo.Table.Alias != rowRecord.ViewTab.ToString())
                return null;

            Panel divIsHTML = new Panel();
            divIsHTML.Attributes.Add("class", "mailIsHTML");
            divIsHTML.Attributes.Add("rval", "1"); // la valeur du bouton radio est conservée en attribut de la div parente

            //Fait un rendu en label
            if (this._rType == RENDERERTYPE.MailFile || this._rType == RENDERERTYPE.SMSFile || ReadonlyRenderer)
            {
                HtmlGenericControl labelFormat = new HtmlGenericControl("label");
                labelFormat.InnerText = eResApp.GetRes(Pref, (fieldRecord.DisplayValue == "1") ? 1004 : 1001); // 1004 : HTML, 1001 : Texte brut
                divIsHTML.Controls.Add(labelFormat);
            }
            else
            {   //fait un rendu  en bouton radio : html ou text
                if (this._rType == RENDERERTYPE.EditMail || this._rType == RENDERERTYPE.EditSMS ||
                    this._rType == RENDERERTYPE.EditMailing || this._rType == RENDERERTYPE.EditSMSMailing
                )
                {
                    HtmlInputRadioButton inputRadioButtonHTML = new HtmlInputRadioButton();
                    inputRadioButtonHTML.Name = String.Concat(eTools.GetFieldValueCellName(rowRecord, fieldRecord), "_R");
                    inputRadioButtonHTML.ID = String.Concat(inputRadioButtonHTML.Name, "_RH");
                    inputRadioButtonHTML.Value = "1";
                    inputRadioButtonHTML.Checked = (fieldRecord.DisplayValue == "1");
                    inputRadioButtonHTML.Attributes.Add("onclick", "this.parentNode.setAttribute('rval', this.value);");
                    HtmlGenericControl labelHTML = new HtmlGenericControl("label");
                    labelHTML.Attributes.Add("for", inputRadioButtonHTML.ID);
                    labelHTML.InnerText = eResApp.GetRes(Pref, 1004); // HTML

                    HtmlInputRadioButton inputRadioButtonText = new HtmlInputRadioButton();
                    inputRadioButtonText.Name = String.Concat(eTools.GetFieldValueCellName(rowRecord, fieldRecord), "_R");
                    inputRadioButtonText.ID = String.Concat(inputRadioButtonText.Name, "_RT");
                    inputRadioButtonText.Value = "0";
                    inputRadioButtonText.Checked = (fieldRecord.DisplayValue != "1");
                    inputRadioButtonText.Attributes.Add("onclick", "this.parentNode.setAttribute('rval', this.value);");
                    HtmlGenericControl labelText = new HtmlGenericControl("label");
                    labelText.Attributes.Add("for", inputRadioButtonText.ID);
                    labelText.InnerText = eResApp.GetRes(Pref, 1001); // Texte

                    divIsHTML.Controls.Add(inputRadioButtonHTML);
                    divIsHTML.Controls.Add(labelHTML);
                    divIsHTML.Controls.Add(inputRadioButtonText);
                    divIsHTML.Controls.Add(labelText);
                }
            }

            return divIsHTML;
        }

        /// <summary>
        /// ajoute les liaisons parentes en pied de page
        /// </summary>
        protected override void AddParentInFoot()
        {
            // ajout du pied de page contenant les informations parentes en popup
            if (_bPopupDisplay)
            {
                eRenderer footRenderer = eRendererFactory.CreateParenttInFootRenderer(Pref, this);
                Panel pgC = null;
                if (footRenderer.ErrorMsg.Length > 0)
                {
                    this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
                }
                if (footRenderer != null)
                    pgC = footRenderer.PgContainer;
                _backBoneRdr.PnlDetailsBkms.Controls.Add(footRenderer.PgContainer);
            }

        }

        #endregion

        #region METHODE DE LA CLASSE

        /// <summary>
        /// Récupère la liste des champs de la fiche E-Mail / 
        /// </summary>
        /// <param name="sortedFields"></param>
        /// <param name="systemFields"></param>
        /// <returns></returns>
        protected virtual List<eFieldRecord> GetHeaderFields(List<eFieldRecord> sortedFields, List<eFieldRecord> systemFields)
        {

            String[] aHeaderFields = eConst.MAIL_HEADER_FIELDS.Split(';');
            List<String> lHeaderFields = new List<string>();
            lHeaderFields.AddRange(aHeaderFields);
            List<eFieldRecord> headerFields = new List<eFieldRecord>();

            // Séparation champs système / champs utilisateur
            foreach (eFieldRecord fld in sortedFields)
            {
                if (
                    fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DATE_SENT.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_ISHTML.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_FROM.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_CC.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_BCC.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DISPLAY_NAME.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_REPLY_TO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_OBJECT.GetHashCode()
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + (int)MailField.DESCID_MAIL_PREHEADER
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HTML.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HISTO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TRACK.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_READ.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode()
                    )
                {
                    headerFields.Add(fld);
                }
            }

            foreach (eFieldRecord fld in systemFields)
            {
                if (
                     fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId
                     && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DATE_SENT.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_ISHTML.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_FROM.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_CC.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_BCC.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DISPLAY_NAME.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_REPLY_TO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_OBJECT.GetHashCode()
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + (int)MailField.DESCID_MAIL_PREHEADER
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HTML.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HISTO.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TRACK.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_READ.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode()
                    || fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode()
                    )
                    headerFields.Add(fld);
            }

            return headerFields;
        }

        /// <summary>
        /// Génère le champ Nom apparent et renvoie ses composantes à l'appelant, pour qu'il les ajoute selon son contexte.
        /// Renvoie null si la génération est ignorée pour cause de droits
        /// </summary>
        /// <param name="displayValue">Valeur initiale à positionner dans le champ de saisie</param>
        /// <param name="disabled">Indique si le champ de saisie doit être désactivé</param>
        /// <param name="updatedLabelID">Nouvel ID à utiliser pour le libellé, si nécessaire</param>
        /// <param name="updatedLabelAttributes">Liste d'attributs HTML à ajouter ou mettre à jour sur le libellé, si nécessaire</param>
        /// <param name="updatedInputID">Nouvel ID à utiliser pour le champ de saisie, si nécessaire</param>
        /// <param name="updatedInputAttributes">Liste d'attributs HTML à ajouter ou mettre à jour sur le libellé, si nécessaire</param>
        /// <returns>Le champ généré, à ajouter au conteneur source par l'appelant</returns>
        protected List<HtmlControl> GetDisplayNameControls(string displayValue, bool disabled,
            string updatedLabelID, Dictionary<string, string> updatedLabelAttributes, string updatedInputID, Dictionary<string, string> updatedInputAttributes
        )
        {
            // Nom apparent
            // TOCHECK: #68 131 - le rendu dépendait de RenderFldFrom avant la refonte et l'intégration de grapesjs. Voir si on conserve cette vérification de droits
            // dépendante des droits de visu sur _fldFrom, si on la remplace par une seule vérification de droits de visu sur _fldDisplayName, ou si on conserve les 2
            if (_fldDisplayName != null && _fldFrom.RightIsVisible && _fldDisplayName.RightIsVisible)
            {
                // Remplissage du champ avec la valeur envoyée par l'appelant
                _fldDisplayName.DisplayValue = displayValue;

                // Génération du champ via les méthodes standard
                List<TableCell> displayNameField = RenderField(_fldDisplayName, String.Empty);
                TableCell labelDisplayNameCell = displayNameField[0];
                TableCell inputDisplayNameCell = displayNameField[1];
                HtmlGenericControl labelDisplayName = new HtmlGenericControl("span");
                HtmlInputText inputDisplayName = new HtmlInputText();
                if (labelDisplayNameCell != null && labelDisplayNameCell.HasControls())
                {
                    foreach (Control control in labelDisplayNameCell.Controls)
                        if (control != null && control is LiteralControl)
                        {
                            labelDisplayName.ID = control.ID;
                            labelDisplayName.InnerText = ((LiteralControl)control).Text;
                        }
                }
                if (inputDisplayNameCell != null && inputDisplayNameCell.HasControls())
                {
                    foreach (Control control in inputDisplayNameCell.Controls)
                        if (control != null && control is TextBox)
                        {
                            inputDisplayName.ID = control.ID;
                            inputDisplayName.Value = ((TextBox)control).Text;
                        }
                }

                // Modification du champ généré pour utiliser des IDs, des câblages JS ou des mises en forme spécifiques pour conserver le même fonctionnement antérieur à la demande #68 131,
                // qui effectuait ce rendu dans RenderCharFieldFormat à partir de _fldFrom
                if (labelDisplayName != null)
                {
                    if (!String.IsNullOrEmpty(updatedLabelID))
                        labelDisplayName.ID = updatedLabelID;
                    // Attributs par défaut positionnés dans tous les cas pour tous les renderers
                    if (updatedLabelAttributes == null)
                        updatedLabelAttributes = new Dictionary<string, string>();
                    if (!updatedLabelAttributes.ContainsKey("class"))
                        updatedLabelAttributes.Add("class", String.Empty);
                    updatedLabelAttributes["class"] = "table_labels spanYAdjust";
                    // Application
                    foreach (KeyValuePair<string, string> attr in updatedLabelAttributes)
                    {
                        if (labelDisplayName.Attributes[attr.Key] == null)
                            labelDisplayName.Attributes.Add(attr.Key, attr.Value);
                        else
                            labelDisplayName.Attributes[attr.Key] = attr.Value;
                    }
                }
                if (inputDisplayName != null)
                {
                    if (!String.IsNullOrEmpty(updatedInputID))
                        inputDisplayName.ID = updatedInputID;
                    if (disabled)
                        inputDisplayName.Disabled = disabled;
                    // Attributs par défaut positionnés dans tous les cas pour tous les renderers
                    if (updatedInputAttributes == null)
                        updatedInputAttributes = new Dictionary<string, string>();
                    if (!updatedInputAttributes.ContainsKey("class"))
                        updatedInputAttributes.Add("class", String.Empty);
                    updatedInputAttributes["class"] = "edit mailDn";

                    //SHA : backlog #1 104
                    if (_rType == RENDERERTYPE.SMSFile)
                        updatedInputAttributes["value"] = _fldDisplayName.DisplayValue;

                    // Application
                    foreach (KeyValuePair<string, string> attr in updatedInputAttributes)
                    {
                        if (inputDisplayName.Attributes[attr.Key] == null)
                            inputDisplayName.Attributes.Add(attr.Key, attr.Value);
                        else
                            inputDisplayName.Attributes[attr.Key] = attr.Value;
                    }
                }

                return new List<HtmlControl>() { labelDisplayName, inputDisplayName };
            }

            return null;
        }

        /// <summary>
        /// Comme son nom l'indique, ajoute les cellules générées par un appel à RenderFld* à la TableRow source, en ayant pris soin de renseigner leur ID à partir
        /// du préfixe donné si l'appel à RenderFld* n'a pas déjà fourni un ID
        /// </summary>
        /// <param name="tr">Ligne de tableau source</param>
        /// <param name="cells">Cellules générées</param>
        /// <param name="cellIdPrefix">Préfixe</param>
        /// <param name="cellStyles">Styles additionnels à ajouter ou modifier sur toutes les cellules</param>
        protected void AddCellsToTableAndFillIDsIfNotEmpty(TableRow tr, List<TableCell> cells, string cellIdPrefix, IDictionary<HtmlTextWriterStyle, string> cellStyles = null)
        {
            int cellIndex = 0;
            foreach (TableCell cell in cells)
            {
                if (cell.CssClass.Contains("table_labels"))
                    cell.CssClass = String.Concat(cell.CssClass, " table_labels_mail");
                if (String.IsNullOrEmpty(cell.ID))
                {
                    //if (cell.HasAttributes && cell.Attributes["eltvalid"] != null)
                    //    cell.ID = cell.Attributes["eltvalid"];
                    //else
                    cell.ID = String.Concat(cellIdPrefix, cellIndex);
                }
                if (cellStyles != null)
                {
                    foreach (KeyValuePair<HtmlTextWriterStyle, string> kvp in cellStyles)
                        if (cell.Style[kvp.Key] == null)
                            cell.Style.Add(kvp.Key, kvp.Value);
                        else
                            cell.Style[kvp.Key] = kvp.Value;
                }
                tr.Cells.Add(cell);
                cellIndex++;
            }
        }

        /// <summary>
        /// Génere des champs cachés
        /// </summary>
        /// <param name="pnlLeft"></param>
        protected virtual void RenderHiddenFields(Panel pnlLeft)
        {
            // est redifini dans eEditMailRenderer 
        }

        /// <summary>
        /// Génere du javascript nécessaire pour la fiche mail : obj merge field, ...
        /// </summary>
        /// <param name="pnlLeft"></param>
        private void RenderJavaScript(Panel pnlLeft)
        {
            // On crée un bloc JavaScript contenant les champs disponibles pour la fusion
            HtmlGenericControl javaScript = new HtmlGenericControl("script");
            javaScript.Attributes.Add("type", "text/javascript");
            javaScript.Attributes.Add("language", "javascript");



            AppendScript(this._strScriptBuilder);

            if (this._strScriptBuilder.Length > 0)
                javaScript.InnerHtml = this._strScriptBuilder.ToString();

            pnlLeft.Controls.Add(javaScript);
        }

        /// <summary>
        /// Ajout des fonctions ou des variables js (comme la liste des champs du fusion)
        /// </summary>
        /// <param name="strScriptBuilder"></param>
        protected virtual void AppendScript (StringBuilder strScriptBuilder)
        {
            string strJavaScript = String.Empty;
            string strErr = String.Empty;
            string strWebsiteFieldsJavaScript = String.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            try
            {
                dal.OpenDatabase();

                eTableLiteMailing tabEmail = eLibTools.GetTableInfo(dal, _tab, eTableLiteMailing.Factory(Pref));

                //tous les champs de fusion
                List<int> AllMergeFields = eLibTools.GetMergeFieldsList(dal, Pref, 0, 0, 0,
                    tabEmail.InterPP, tabEmail.InterPM, tabEmail.AdrJoin, tabEmail.InterEVT, tabEmail.InterEVTDescid, 0);

                //On filtre la  liste par rapport aux droits de visu
                List<int> AllowedMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(this.Pref, Pref.User, String.Join(";", AllMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs
                eLibTools.GetMergeFieldsData(dal, Pref, Pref.User, AllowedMergeFields, null, null, null, null, null, null, out strJavaScript);

                //Tous les champs de fusion de type site web
                List<int> AllWebsiteMergeFields = eLibTools.GetaMailMergeFieldsList(dal, Pref, new MergeFieldMailInfoParams()
                {
                    AdrJoin = tabEmail.AdrJoin,
                    FieldFormat = FieldFormat.TYP_WEB,
                    DescIdTargetLink = 0,
                    EventDescId = tabEmail.InterEVTDescid,
                    EventDescIdMail = 0,
                    InterEvent = tabEmail.InterEVT,
                    InterPm = tabEmail.InterPM,
                    InterPp = tabEmail.InterPP,
                    ParentEvtId = 0,
                    ParentTplTab = 0
                });
                //on construit la liste des champs de type site web
                eLibTools.GetMergeFieldsData(dal, Pref, Pref.User, AllWebsiteMergeFields, null, null, null, null, null, null, out strWebsiteFieldsJavaScript);
            }
            catch (Exception ex)
            {
                throw new Exception("eMailFileRenderer::AppendScript:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            strScriptBuilder.Append(" var mailMergeFields = ").Append(String.IsNullOrEmpty(strJavaScript) ? "{}" : strJavaScript).Append(";").AppendLine();

            //Chargement des champs de fusion de type web
            strScriptBuilder.AppendLine()
                .Append(" var oMergeHyperLinkFields = { link :{href:'', ednc:'lnk', ednt:'on', ednd:'0', ednn:'', ednl:'0', title:'', target:'_blank'}, fields : [ ")
                .Append("['<").Append(eResApp.GetRes(Pref, 141)).Append(">', '0', '']");     // option vide

            if (!String.IsNullOrEmpty(strWebsiteFieldsJavaScript))
            {
                string[] websiteFields = strWebsiteFieldsJavaScript.Split(',');
                foreach (String wbF in websiteFields)
                {
                    string[] strlist1 = wbF.Split(':');
                    string fldLibelle = Regex.Replace(strlist1[0], "(\\r|\\n|\\|\u0022|{)*", String.Empty);
                    string[] strlist2 = strlist1[1].Split(';');
                    string fldDescId = String.Concat("'", strlist2[0].Replace("\"", String.Empty), "'").Replace(" ", String.Empty);
                    string fldFormat = String.Concat("'", strlist2[3], "'").Replace(" ", String.Empty);
                    string wsField = String.Concat(", [", fldLibelle, ",", fldDescId, ",", fldFormat, "]");
                    strScriptBuilder.Append(wsField);
                }
            }

            strScriptBuilder.Append(" ]}; ");

        }

        /// <summary>
        /// Affiche les liens : afficher les cc bcc,joindre un modèle, ajout de pj
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual Boolean RenderLinks(System.Web.UI.WebControls.TableRow tr) { return true; }

        /// <summary>
        /// Faire un rendu de chaque champ dans la table en parametre
        /// </summary>
        /// <param name="tab">Tableau HTML sur lequel ajouter les champs</param>
        protected virtual void RenderFields(System.Web.UI.WebControls.Table tab)
        {
            //statut + date
            RenderStatusHeader(tab);

            TableRow tr = new TableRow();
            tr.Attributes.Add("class", "fields-row");
            tab.Controls.Add(tr);

            

            tr = new TableRow();
            tr.Attributes.Add("class", "fields-row");
            tab.Controls.Add(tr);

            //From
            RenderFldFrom(tr);         

            //Repondre a 
            RenderFldReplyTo(tr);

            //Destinataire
            RenderFldTo(tr);

            //Affiche les liens pour ajouter un template/une pj ou afficher les champ cc bcc
            RenderLinks(tr);

            tr = new TableRow();
            tr.Attributes.Add("class", "fields-row");
            tab.Controls.Add(tr);

            // Sujet (taille x2)
            RenderFldSubject(tr);

            //SHA : tâche #1 939
            // Preheader (texte d'aperçu)
            RenderFldPreheader(tr);

            //CC
            RenderFldCc(tr);
            //BCC
            RenderFldBcc(tr);

            //is html
            RenderFldIsHTML(tr);

            // Ajout d'une cellule vide
            tr.Cells.Add(new TableCell());

            tr = new TableRow();
            tab.Controls.Add(tr);

            //corps
            RenderFldBody(tab);


            // Ajout d'une cellule vide
            tr.Cells.Add(new TableCell());

            if (_rType == RENDERERTYPE.EditMailing
                && !ReadonlyRenderer
                &&
                 (_ePref.ClientInfos.ClientOffer > 0 & !eTools.IsMSBrowser)
                )
            {
                //2é corps avec Cked - uniquement pour wizard et editeur de modèle et si offre et nav le permettent
                tr = new TableRow();
                tab.Controls.Add(tr);
                RenderFldBody(tab, true);
            }
            tr = new TableRow();
            tab.Controls.Add(tr);

            //Historisation et confirmation lecture 
            RenderFldHistoTrack(tr);
        }

        /// <summary>
        /// Recupere les champs de la fiche Email/Emailing (Campaign) et les stocke dans des variables de class
        /// Cela permet des les ordonner correctement
        /// </summary>
        /// <param name="fields">Les champs de la fiche</param>
        protected virtual void RetrieveFields(List<eFieldRecord> fields)
        {
            foreach (eFieldRecord fld in fields)
            {
                if (MailForward)
                    fld.FileId = 0;


                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DATE_SENT.GetHashCode())
                {
                    _fldDate = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_ISHTML.GetHashCode())
                {
                    _fldIsHTML = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_FROM.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldFrom = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TO.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    fld.RightIsVisible = true;
                    _fldTo = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_CC.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldCc = fld;
                    continue;
                }


                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_BCC.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldBcc = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_DISPLAY_NAME.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldDisplayName = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_REPLY_TO.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldReplyTo = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_OBJECT.GetHashCode())
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldSubject = fld;
                    continue;
                }

                //SHA: tâche #1 939
                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + (int)MailField.DESCID_MAIL_PREHEADER)
                {
                    fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                    _fldPreheader = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HTML.GetHashCode())
                {
                    _fldBody = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_HISTO.GetHashCode())
                {
                    _fldHisto = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_TRACK.GetHashCode())
                {
                    _fldTracking = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_READ.GetHashCode())
                {
                    _fldRead = fld;
                    continue;
                }


                if (this._rType != RENDERERTYPE.SMSFile)
                {
                    if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode())
                    {
                        _fldStatus = fld;
                        continue;
                    }
                }
                else if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_SMS_STATUS.GetHashCode())
                {
                    _fldStatus = fld;
                    continue;
                }

                if (fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode())
                {
                    _fldMailSendType = fld;
                    continue;
                }
            }

            //Avant de faire les transformation aux champs, on construit l'entête de transfert de message.
            if (_bMailForward)
                AppendParentMailInfosToBody();
        }

        /// <summary>
        /// Rendu du champ Historique/ et Confirmation de lecture 
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldHistoTrack(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldHisto == null && _fldTracking == null)
                return false;

            // Historisé et confirmation de lecture
            // Ces deux champs devant être affichés côte-à-côte, on utilise les méthodes usuelles pour générer le rendu, puis on récupère les contrôles case à cocher
            // pour les ajouter dans une ligne séparée
            if (_fldHisto.RightIsVisible || (_fldTracking != null && _fldTracking.RightIsVisible))
            {
                List<TableCell> trHisto = RenderField(_fldHisto, String.Empty);
                List<TableCell> trTracking = new List<TableCell>();
                if (_fldTracking != null)
                    trTracking = RenderField(_fldTracking, String.Empty);

                List<TableCell> trRead = new List<TableCell>();
                if (_fldRead != null)
                    trRead = RenderField(_fldRead, String.Empty);

                TableCell tcHistoTrackingValue = new TableCell();
                tcHistoTrackingValue.ColumnSpan = 2;
                if (_fldHisto.RightIsVisible)
                {
                    //Création du conteneur de contrôle (équivalent de la cellule de tableau attendue par l'Engine)
                    HtmlGenericControl valueHisto = new HtmlGenericControl("span");
                    valueHisto.ID = trHisto[trHisto.Count - 1].ID;
                    valueHisto.Attributes.Add("class", trHisto[trHisto.Count - 1].CssClass);
                    foreach (string attrKey in trHisto[trHisto.Count - 1].Attributes.Keys)
                        valueHisto.Attributes.Add(attrKey, trHisto[trHisto.Count - 1].Attributes[attrKey]);
                    foreach (Control ctrl in trHisto[trHisto.Count - 1].Controls)
                        valueHisto.Controls.Add(ctrl);
                    tcHistoTrackingValue.Controls.Add(valueHisto);
                    //Création du libellé
                    HtmlGenericControl labelHisto = new HtmlGenericControl("span");
                    labelHisto.ID = trHisto[0].ID;
                    labelHisto.Attributes.Add("class", trHisto[0].CssClass);
                    foreach (string attrKey in trHisto[0].Attributes.Keys)
                        labelHisto.Attributes.Add(attrKey, trHisto[0].Attributes[attrKey]);
                    tcHistoTrackingValue.Controls.Add(labelHisto);
                }
                // Le champ Confirmation de lecture (mail unitaire uniquement) ne doit pas être affiché en mode Consultation
                if (_fldTracking != null && _fldTracking.RightIsVisible)//&& _fldTracking.RightIsUpdatable : KHA demande n°30300
                {
                    //Création du conteneur de contrôle (équivalent de la cellule de tableau attendue par l'Engine)
                    HtmlGenericControl valueTracking = new HtmlGenericControl("span");
                    valueTracking.ID = trTracking[trTracking.Count - 1].ID;
                    valueTracking.Attributes.Add("class", trTracking[trTracking.Count - 1].CssClass);
                    foreach (string attrKey in trTracking[trTracking.Count - 1].Attributes.Keys)
                        valueTracking.Attributes.Add(attrKey, trTracking[trTracking.Count - 1].Attributes[attrKey]);
                    foreach (Control ctrl in trTracking[trTracking.Count - 1].Controls)
                        valueTracking.Controls.Add(ctrl);
                    tcHistoTrackingValue.Controls.Add(valueTracking);
                    //Création du libellé
                    HtmlGenericControl labelTracking = new HtmlGenericControl("span");
                    labelTracking.ID = trTracking[0].ID;
                    labelTracking.Attributes.Add("class", trTracking[0].CssClass);
                    foreach (string attrKey in trTracking[0].Attributes.Keys)
                        labelTracking.Attributes.Add(attrKey, trTracking[0].Attributes[attrKey]);
                    tcHistoTrackingValue.Controls.Add(labelTracking);
                }

                //Lu - KHA demande n°30300
                String mailStatusAlias = String.Concat(_myFile.ViewMainTable.DescId, "_", _myFile.ViewMainTable.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode());
                Int32 iStatus;
                Int32.TryParse(_myFile.Record.GetFieldByAlias(mailStatusAlias).Value, out iStatus);

                if (iStatus == (int)EmailStatus.MAIL_SENT && _fldRead != null && _fldRead.RightIsVisible)
                {
                    //Création du conteneur de contrôle (équivalent de la cellule de tableau attendue par l'Engine)
                    HtmlGenericControl valueRead = new HtmlGenericControl("span");
                    valueRead.ID = trRead[trRead.Count - 1].ID;
                    valueRead.Attributes.Add("class", trRead[trRead.Count - 1].CssClass);
                    foreach (string attrKey in trRead[trRead.Count - 1].Attributes.Keys)
                        valueRead.Attributes.Add(attrKey, trRead[trRead.Count - 1].Attributes[attrKey]);
                    foreach (Control ctrl in trRead[trRead.Count - 1].Controls)
                        valueRead.Controls.Add(ctrl);
                    tcHistoTrackingValue.Controls.Add(valueRead);
                    //Création du libellé
                    HtmlGenericControl labelRead = new HtmlGenericControl("span");
                    labelRead.ID = trRead[0].ID;
                    labelRead.Attributes.Add("class", trRead[0].CssClass);
                    foreach (string attrKey in trRead[0].Attributes.Keys)
                        labelRead.Attributes.Add(attrKey, trRead[0].Attributes[attrKey]);
                    tcHistoTrackingValue.Controls.Add(labelRead);
                }

                tr.Controls.Add(tcHistoTrackingValue);
                return false;
            }

            return true;
        }

       
        /// <summary>
        /// Rendu du mode HTML/Texte
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        protected override bool RenderMemoFieldIsHtml(eFieldRecord fieldRow)
        {
            if (!fieldRow.FldInfo.Alias.Equals(_fldBody.FldInfo.Alias))
                return base.RenderMemoFieldIsHtml(fieldRow);

            //GCH - #32338 : L'envoi des mails unitaires est désormais forcé en mode HTML.
            if (this.RendererType == RENDERERTYPE.EditMail)
                return true;
            // MAB - #59 789 : SMS toujours envoyés en format Texte - TOCHECK SMS : vérifier par rapport aux "commandes" ActivMail, à envoyer en HTML ?
            if (this.RendererType == RENDERERTYPE.EditSMS)
                return false;

            return _fldIsHTML.DisplayValue == "1";
        }

        /// <summary>
        /// Rendu du corps de mail 
        /// </summary>
        /// <param name="tab">table html</param>
        /// <param name="bCkeditor">Force l'utilisation de ckeditor pour le wizar "multi"</param>
        /// <returns></returns>
        protected virtual bool RenderFldBody(System.Web.UI.WebControls.Table tab, bool bCkeditor = false)
        {
            // Corps
            if (_fldBody == null || !_fldBody.RightIsVisible)
                return false;

            //Création du cartouche d'entête
            TableRow myTr = new TableRow();
            /*Boolean bHtml = _fldIsHTML.DisplayValue == "1";

            if (this.RendererType == RENDERERTYPE.EditMail)
            {
                //bHtml = _fldIsHTML.DisplayValue == "1"; //Dans le cas de l'édition de mail ce n'est pas le HTML de desc qui est pris en compte mais la valeur du champs Format HTML
                bHtml = true;   //GCH - #32338 : L'envoi des mails unitaires est désormais forcé en mode HTML.
            }*/
            //Cas spécifique E-mail
            //Certains champs spéciaux ont un type et une taille déclaré(e)s en base qui n'est pas ce qui est souhaité en affichage (Corps de mail, De, A, Cc, Bcc, Objet)
            //Pour ces champs-là, on change la taille d'affichage et le format
            _fldBody.FldInfo.PosRowSpan = 1;

            //Création de la cellule
            TableCell myLabel = new TableCell();
            TableCell myValue = new TableCell();

            //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
            GetFieldLabelCell(myLabel, _myFile.Record, _fldBody);

            myLabel.Attributes.Add("fmt", _fldBody.FldInfo.Format.GetHashCode().ToString());
            myLabel.Style.Add("visibility", "hidden");

            //Génération du rendu HTML pour chaque champ à afficher
            //C'est dans cette méthode du eRenderer générique que sont gérées les exceptions visuelles à appliquer sur certains champs d'E-mail
            //ex : combobox pour le champ De (From)
            TableCell myValueCell = new TableCell();

            if (_fldBody.DisplayValue.Trim().Length == 0 && RenderMemoFieldIsHtml(_fldBody) && _mailingTemplate != null)
            {
                _fldBody.DisplayValue = _mailingTemplate.Body;
            }

            #region ajout de signature utilisateur si EMAILAUTOADDSIGN =  1 dans config
            // #59 789 : pas pour les SMS, la signature pouvant être en HTML
            if (this.RendererType == RENDERERTYPE.EditMailing || this.RendererType == RENDERERTYPE.EditMail)
            {
                Boolean _bAutoSignInsert = Pref.GetConfig(eLibConst.PREF_CONFIG.EMAILAUTOADDSIGN).Equals("1");
                String sUserSign;
                if (_bAutoSignInsert)
                {
                    if (!eUser.GetFieldValue<String>(Pref, Pref.User.UserId, "UserSignature", out sUserSign))
                        sUserSign = String.Empty;

                    // On insere la signature que si le corps de mail est vide
                    if (RenderMemoFieldIsHtml(_fldBody))
                    {
                        if (_fldBody.DisplayValue?.Trim().Length == 0 || _mailingTemplate != null)
                            _fldBody.DisplayValue = String.Concat(_fldBody.DisplayValue, sUserSign);
                    }

                }
            }


            #endregion

            int nIdx = 0;
            if (_rType == RENDERERTYPE.EditMailing && bCkeditor)
            {
                nIdx = 1;
                ((eEditMailingRenderer)this).UseCkeditor = true;
            }
            myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, _fldBody, nIdx, Pref);


            //Spécifique mail : si on traite le champ "Corps"

            // Si on affiche un mail en consultation, on met une bordure autour du champ Corps
            // TOCHECK SMS
            if (this.RendererType == RENDERERTYPE.MailFile || this.RendererType == RENDERERTYPE.SMSFile)
                myValueCell.CssClass += "mailBody";

            myValueCell.Attributes["html"] = RenderMemoFieldIsHtml(_fldBody) ? "1" : "0";

            //TODOMOU - 
            myValueCell.Attributes.Add("efld", "1");

            //On l'ajoute dans la variable référençant les champs Mémo
            //afin qu'un eMemoEditor puisse être instancié dessus via un appel à initMemoFields() (JS)
            //Puis on le redimensionne en fonction de la taille indiquée en paramètre
            this.MemoIds.Add(myValueCell.ID);

            // Définition d'une variable correspondant à l'espace à réserver autour du champ Mémo pour afficher les autres champs
            // Plus cette valeur est basse, plus grand sera le champ Mémo (et non pas l'inverse !)
            // Backlog #43 - Avec grapesjs, proposition est faite de maximiser l'espace du champ Mémo, et d'afficher la partie Associez votre e-mail en-dehors du
            // champ de vision avec barre de défilement
            int nOuterBodySpace = 150;
            // #30 143 - Décision a été prise de ne pas réserver d'espace supplémentaire pour afficher les champs Cc/Cci
            // afin de maximiser l'espace alloué au champ Corps
            // Une barre de défilement apparaîtra donc si on affiche ces champs optionnels
            int nOptionalFieldsSpace = 0;
            /* nOptionalFieldsSpace = 55; // espace réservé aux champs Cc/Cci */

            // #29 879 - Si on affiche un mail en consultation, il y a moins de champs affichés, donc on réduit
            // l'espace qui doit leur être réservé pour agrandir davantage le champ Corps
            // #68 13x : ça n'est désormais plus le cas depuis la refonte de l'affichage sur plusieurs colonnes. L'espace à allouer en Edition et Consultation est
            // désormais sensiblement le même
            // Backlog #43 - Dans ce mode, on affiche toutefois tout sans barre de défilement
            if (this.RendererType == RENDERERTYPE.MailFile || this.RendererType == RENDERERTYPE.SMSFile)
                nOuterBodySpace += 150;

            // Taille du champ Mémo ajustée après génération de la cellule
            // Calcul : taille de la fenêtre - espace occupé par les autres éléments
            int nBodyHeight = _nHeight - nOuterBodySpace - nOptionalFieldsSpace;

            // Si la taille est trop réduite, on impose une taille minimale
            if (nBodyHeight < 50)
                nBodyHeight = 50;
            // Ajout
            if (nBodyHeight > 0)
                myValueCell.Style.Add(HtmlTextWriterStyle.Height, String.Concat(nBodyHeight, "px"));

            myValueCell.RowSpan = 1;
            myValueCell.ColumnSpan = 1;

            // Demande #28 631 : on n'affiche pas de cellule libellé à côté de l'éditeur de corps de mail, pour maximiser l'espace qui lui est réservé
            // Il faut toutefois conserver la cellule Libellé dans la structure de tableau pour le bon fonctionnement des JavaScripts utilisant ce champ
            // pour en connaître les attributs (ex : champ obligatoire)
            // On ajoute donc une ligne avec le libellé à gauche + une cellule vide à droite
            // Et une autre ligne avec le corps de mail sur une cellule fusionnée (colspan multiple)
            // Puis on masque la ligne comportant le libellé

            myTr.Cells.Add(myLabel);
            myTr.Cells.Add(new TableCell());
            myTr.Style.Add("display", "none");

            TableRow myTr2 = new TableRow();
            //SHA : tâche #2 015
            myValueCell.ColumnSpan = (RendererType == RENDERERTYPE.EditMailing ? 12 : 8); //6 : 8
            myTr2.Cells.Add(myValueCell);
            /*
            if (_rType == RENDERERTYPE.EditMailing)
            {
                ((eEditMailingRenderer)this).UseCkeditor = true;
                TableCell myValueCell2 = (TableCell)GetFieldValueCell(_myFile.Record, _fldBody, 1, Pref
                    );
             //   myValueCell2.ID += "_CK";
                this.MemoIds.Add(myValueCell2.ID);

                myTr2.Cells.Add(myValueCell2);
            }
            */

            tab.Controls.Add(myTr);
            tab.Controls.Add(myTr2);

            return true;
        }

        /// <summary>
        /// Dans le cas d'un transfert de mail, ajout de l'entête du mail au début corps avec un "Message transféré"
        /// </summary>       
        private void AppendParentMailInfosToBody()
        {
            if (_fldBody != null)
            {
                StringBuilder sb = new StringBuilder();

                Boolean isHtml = HasDisplayValue(_fldIsHTML) && _fldIsHTML.Value == "1";
                String NewLine = "<br />";

                sb.Append("&nbsp;").Append(NewLine).Append(NewLine).Append(NewLine).Append(NewLine);
                // ISO V7 mais c'est moche...
                // sb.Append("<hr align='center' size='1' tabindex='-1' width='100%' />").Append(NewLine);
                sb.Append("------------ ").Append(eResApp.GetRes(Pref.LangId, 6866)).Append(" ------------").Append(NewLine);

                if (HasDisplayValue(_fldFrom))
                    sb.Append(_fldFrom.FldInfo.Libelle).Append(" : ").Append(HttpUtility.HtmlEncode(_fldFrom.DisplayValue)).Append(NewLine);

                if (HasDisplayValue(_fldDate))
                    sb.Append(HasDisplayValue(_fldStatus) ? _fldStatus.DisplayValue : _fldDate.FldInfo.Libelle).Append(" : ").Append(_fldDate.DisplayValue).Append(NewLine);

                if (HasDisplayValue(_fldTo))
                    sb.Append(_fldTo.FldInfo.Libelle).Append(" : ").Append(HttpUtility.HtmlEncode(_fldTo.DisplayValue)).Append(NewLine);

                if (HasDisplayValue(_fldCc))
                    sb.Append(_fldCc.FldInfo.Libelle).Append(" : ").Append(HttpUtility.HtmlEncode(_fldCc.DisplayValue)).Append(NewLine);

                if (HasDisplayValue(_fldSubject))
                    sb.Append(_fldSubject.FldInfo.Libelle).Append(" : ").Append(HttpUtility.HtmlEncode(_fldSubject.DisplayValue)).Append(NewLine);

                //SHA : tâche #1 939 
                //if (HasDisplayValue(_fldPreheader))
                //    sb.Append(_fldPreheader.FldInfo.Libelle).Append(" : ").Append(HttpUtility.HtmlEncode(_fldPreheader.DisplayValue)).Append(NewLine);

                sb.Append(NewLine);

                _fldBody.Value = sb.Append(_fldBody.DisplayValue).ToString();
                _fldBody.DisplayValue = _fldBody.Value;
            }
        }

        /// <summary>
        /// Savoir s'il y a une valeur à afficher
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private bool HasDisplayValue(eFieldRecord fld)
        {
            return fld != null && !String.IsNullOrEmpty(fld.DisplayValue);
        }

        /// <summary>
        /// Sujet du mail
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldSubject(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldSubject != null )
            {
                //SHA KJE #2 476 : rubrique Objet obligatoire pour l'assistant mailing
                _fldSubject.IsMandatory = true;
                if (_bMailForward)
                {
                    if (_fldSubject.Value.ToUpper().StartsWith("RE:") || _fldSubject.Value.ToUpper().StartsWith("TR:"))
                        _fldSubject.Value = _fldSubject.Value.Substring(3).TrimStart();
                    _fldSubject.Value = String.Concat("TR: ", _fldSubject.Value);
                    _fldSubject.DisplayValue = _fldSubject.Value;
                }

                // Objet
                if (_fldSubject.RightIsVisible)
                {
                    if (_defaultTemplateID != 0 && _mailingTemplate != null)
                        _fldSubject.DisplayValue = _mailingTemplate.Subject;
                    foreach (TableCell cell in RenderField(_fldSubject, "mailSubject"))
                        tr.Controls.Add(cell);
                    return true;
                }
            }

            return false;
        }

        //SHA : tâche #1 939
        /// <summary>
        /// Preheader (texte d'aperçu) du mail
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldPreheader(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldPreheader != null && _fldPreheader.RightIsVisible)
            {
                if (_defaultTemplateID != 0 && _mailingTemplate != null)
                    _fldPreheader.DisplayValue = _mailingTemplate.Preheader;
                foreach (TableCell cell in RenderField(_fldPreheader, "mailPreheader"))
                    tr.Controls.Add(cell);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Génère le champ "Répondre à"
        /// A appeler A LA PLACE de eRenderer.GetTableCell
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldReplyTo(System.Web.UI.WebControls.TableRow tr)
        {
            // Répondre à
            if (_fldReplyTo != null && _fldReplyTo.RightIsVisible)
            {

                if ((_fldReplyTo.FldInfo.Table.EdnType == EdnType.FILE_MAIL || _fldReplyTo.FldInfo.Table.EdnType == EdnType.FILE_SMS) && !_bIsEditRenderer)
                {
                    // De + Nom Apparent (sur la même ligne)
                    List<TableCell> cells = RenderField(_fldReplyTo, "mailTo");

                    string cellIdPrefix = "mailTo_";
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);
                }
                else
                {
                    //Cas spécifique E-mail
                    //Certains champs spéciaux ont un type et une taille déclaré(e)s en base qui n'est pas ce qui est souhaité en affichage (Corps de mail, De, A, Cc, Bcc, Objet)
                    //Pour ces champs-là, on change la taille d'affichage et le format
                    int nFieldDescId = 0;

                    //Création de la cellule
                    TableCell myLabel = new TableCell();
                    myLabel.Text = String.Empty;

                    //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
                    this.GetFieldLabelCell(myLabel, _myFile.Record, _fldReplyTo);
                    //Modification des attributs de base

                    myLabel.Text = _fldReplyTo.FldInfo.Libelle;
                    myLabel.ID = eTools.GetFieldValueCellName(_myFile.Record, _fldReplyTo);


                    myLabel.Attributes.Add("did", nFieldDescId.ToString());
                    myLabel.Attributes.Add("lib", myLabel.Text);
                    myLabel.Attributes.Add("fmt", _fldReplyTo.FldInfo.Format.GetHashCode().ToString());

                    //Création de la cellule
                    TableCell tcReplyTo = new TableCell();

                    HtmlInputText inputReplyTo = new HtmlInputText();
                    inputReplyTo.Value = Pref.User.UserMail; // TODO: adresse e-mail par défaut
                    inputReplyTo.Attributes.Add("class", "edit mail_rt");
                    inputReplyTo.ID = String.Concat("COL_", _myFile.Record.CalledTab, "_RT_0_0_0");
                    tcReplyTo.Controls.Add(inputReplyTo);

                    tcReplyTo.RowSpan = 1;
                    tcReplyTo.ColumnSpan = 1;
                    tcReplyTo.CssClass = String.Concat(tcReplyTo.CssClass, " table_values ", "mailCc");

                    tr.Controls.Add(myLabel);
                    tr.Controls.Add(tcReplyTo);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// Champ BCC
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldBcc(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldBcc != null)
            {
                if (_bMailForward)
                {
                    // En mode Transfert de mail, on vide la liste des destinataires et on modifie l'objet pour y inclure "TR:"
                    _fldBcc.Value = String.Empty;
                    _fldBcc.DisplayValue = String.Empty;
                }

                // Copie cachée carbone (Bcc)
                if (_fldBcc.RightIsVisible)
                {
                    List<TableCell> cells = RenderField(_fldBcc, "mailBcc");

                    string cellIdPrefix = "mailBcc_";
                    // l'option est cachée par défaut - TODO MAB #68 13x
                    Dictionary<HtmlTextWriterStyle, string> cellStyles = new Dictionary<HtmlTextWriterStyle, string>();
                    //cellStyles.Add(HtmlTextWriterStyle.Display, "none");
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Champ CC
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldCc(System.Web.UI.WebControls.TableRow tr)
        {
            // Copie cachée (Cc) - Toujours visible en emailing, à la demande en mail unitaire
            if (_fldCc != null && _fldCc.RightIsVisible)
            {

                if (_bMailForward)
                {    // En mode Transfert de mail, on vide la liste des destinataires et on modifie l'objet pour y inclure "TR:"
                    _fldCc.Value = String.Empty;
                    _fldCc.DisplayValue = String.Empty;
                }

                // Copie cachée (Cc) - Toujours visible en emailing, à la demande en mail unitaire
                if (_fldCc.RightIsUpdatable)
                {
                    List<TableCell> cells = RenderField(_fldCc, "mailCc");

                    string cellIdPrefix = "mailCc_";
                    // l'option est cachée par défaut - TODO MAB #68 13x
                    Dictionary<HtmlTextWriterStyle, string> cellStyles = new Dictionary<HtmlTextWriterStyle, string>();
                    //cellStyles.Add(HtmlTextWriterStyle.Display, "none");
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix, cellStyles);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Rendu de Destinataire
        /// </summary>
        /// <param name="tr">table html</param>
        /// <returns></returns>
        protected virtual bool RenderFldTo(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldTo != null)
            {
                if (!String.IsNullOrEmpty(_strMailTo))
                {
                    // Si la fenêtre a été ouverte depuis une adresse mail de l'application
                    _fldTo.Value = _strMailTo;
                    _fldTo.DisplayValue = _strMailTo;
                }
                else
                {
                    // Si la fenêtre a été ouverte depuis un signet E-mail d'une fiche Contact (contact associé) ou autre,
                    // on préremplit le champ "A :" avec son adresse
                    // #59 789 : même chose pour SMS, mais avec un signet SMS et des champs de type Téléphone
                    if (File.ViewMainTable.EdnType == EdnType.FILE_SMS)
                    {
                        eFileSMS fileSMS = File as eFileSMS;
                        if (fileSMS != null)
                        {
                            if (fileSMS.FirstTargetFld != null)
                            {
                                _fldTo.Value = fileSMS.FirstTargetFld;
                                _fldTo.DisplayValue = fileSMS.FirstTargetFld;
                            }
                        }
                    }
                    else
                    {
                        eFileMail fileMail = File as eFileMail;
                        if (fileMail != null)
                        {
                            if (fileMail.FirstTargetFld != null)
                            {
                                _fldTo.Value = fileMail.FirstTargetFld;
                                _fldTo.DisplayValue = fileMail.FirstTargetFld;
                            }
                        }
                    }
                }

                if (_bMailForward)
                {
                    // En mode Transfert de mail, on vide la liste des destinataires et on modifie l'objet pour y inclure "TR:"
                    _fldTo.Value = String.Empty;
                    _fldTo.DisplayValue = String.Empty;
                }

                // A (To)
                if (_fldTo.RightIsVisible)
                {
                    List<TableCell> cells = RenderField(_fldTo, "mailTo");

                    string cellIdPrefix = "mailTo_";
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Rendu de l'expediteur
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldFrom(System.Web.UI.WebControls.TableRow tr)
        {
            // Expéditeur (From - De), Nom apparent si Email
            if (_fldFrom != null && _fldFrom.RightIsVisible)
            {
                // De + Nom Apparent (sur la même ligne)
                List<TableCell> cells = RenderField(_fldFrom, "mailFrom");

                string cellIdPrefix = "mailFrom_";
                AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Rendu de format de text
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected virtual bool RenderFldIsHTML(System.Web.UI.WebControls.TableRow tr)
        {
            // Format du mail (HTML ou texte)
            if (_fldIsHTML != null && _fldIsHTML.RightIsVisible)
            {
                List<TableCell> cells = RenderField(_fldIsHTML, String.Empty);

                string cellIdPrefix = "mailIsHTML_";
                AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Rendu de status + date envoyée
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected virtual bool RenderStatusHeader(System.Web.UI.WebControls.Table tab)
        {
            // Statut du mail/Envoyé le (Date) - Mail unitaire uniquement

            TableRow trDateSent = new TableRow();
            TableCell tcDateSentLabel = new TableCell();
            trDateSent.Controls.Add(tcDateSentLabel);
            TableCell tcDateSent = new TableCell();

            if (_fldDate != null)
            {
                //TODO: déterminer statut brouillon/envoyé/non envoyé selon info stockée en base
                if (_fldDate.DisplayValue.Length > 0 && !_bMailForward)
                {

                    if (_fldMailSendType != null && _fldMailSendType.Value == MailSendType.SMS.GetHashCode().ToString())
                        //SHA
                        tcDateSent.Text = String.Concat(eResApp.GetRes(Pref.LangId, 6668), "  ", eResApp.GetRes(Pref.LangId, 2337), ", ", eResApp.GetRes(Pref.LangId, 1058), " ", _fldDate.DisplayValue);//String.Concat(eResApp.GetRes(Pref, 659), " ", _fldDate.DisplayValue);

                    else
                        tcDateSent.Text = String.Concat(eResApp.GetRes(Pref, 559), " ", _fldDate.DisplayValue);

                    tcDateSent.CssClass = "mail-head-text-sent";
                    trDateSent.Controls.Add(tcDateSent);
                    tab.Controls.Add(trDateSent);
                }
                /*
                else
                {
                    tcDateSent.Text = eResApp.GetRes(Pref.Lang, 558);
                    tcDateSent.CssClass = "mail-head-text-not-sent";
                    trDateSent.Controls.Add(tcDateSent);
                    tab.Controls.Add(trDateSent);
                }
                */

                return true;
            }
            return false;
        }

        /// <summary>
        /// Crée et insère un champ de la fiche E-mail
        /// </summary>
        /// <param name="myField"></param>
        /// <param name="customCssClass"></param>
        /// <returns></returns>
        protected List<TableCell> RenderField(eFieldRecord myField, String customCssClass)
        {
            List<TableCell> fieldCells = new List<TableCell>();

            //Cas spécifique E-mail
            //Certains champs spéciaux ont un type et une taille déclaré(e)s en base qui n'est pas ce qui est souhaité en affichage (Corps de mail, De, A, Cc, Bcc, Objet)
            //Pour ces champs-là, on change la taille d'affichage et le format
            myField.FldInfo.PosRowSpan = 1;

            //Création de la cellule
            TableCell myLabel = new TableCell();

            //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
            this.GetFieldLabelCell(myLabel, _myFile.Record, myField);

            //Pour faire des vérifs coté js
            if (myField == _fldReplyTo || myField == _fldFrom || myField == _fldBcc)
                myLabel.Attributes.Add("fmt", FieldFormat.TYP_EMAIL.GetHashCode().ToString());
            else
                myLabel.Attributes.Add("fmt", myField.FldInfo.Format.GetHashCode().ToString());


            // Spécifique Objet : ajout de l'attribut "mailtemplate" pour indiquer à la fenêtre de choix de modèle de mail (lien) qu'il faut afficher le catalogue avec quelques
            // spécificités : icône pour éditer le modèle et désactivation de certaines fonctionnalités (synchro)
            if (myField == _fldSubject)
            {
                myLabel.Attributes.Add("mailtemplate", "1");
                myLabel.Attributes.Add("email", myLabel.Text);
            }

            //Génération du rendu HTML pour chaque champ à afficher
            //C'est dans cette méthode du eRenderer générique que sont gérées les exceptions visuelles à appliquer sur certains champs d'E-mail
            //ex : combobox pour le champ De (From)

            int nIdx = 0;
            if (_rType == RENDERERTYPE.EditMailing && ((eEditMailingRenderer)this).UseCkeditor)
            {
                nIdx = 1;
            }

            fieldCells.Add(myLabel);
            //SHA tâche #2 598
            if (myField.FldInfo.Descid != CampaignField.SUBJECT.GetHashCode()) //KJE tâche 2 334
            {
                TableCell myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, myField, nIdx, Pref);

                //TODOMOU - Gérer cet attribut de façon globale
                myValueCell.Attributes.Add("efld", "1");

                myValueCell.RowSpan = 1;
                int colSpan = 1;
                if (myField == _fldIsHTML)
                    colSpan = 5;
                myValueCell.ColumnSpan = colSpan;
                myValueCell.CssClass = String.Concat(myValueCell.CssClass, " table_values ", customCssClass);


                //KHA ajout d'un bouton permettant l'ajout de mails de contacts.
                AddMailAddrBtn(myValueCell, myField);
                fieldCells.Add(myValueCell);

            }
            else
            {
                //tâche #2 477
                FieldsDescId.Add(myField.FldInfo.Descid.ToString());
                fieldCells.Add(GetCustomObjectTableCell(myLabel.ClientID, eTools.GetFieldValueCellId(_myFile.Record, myField, nIdx), myField.DisplayValue));
            }

            

            return fieldCells;
        }

        /// <summary>
        /// REDIFINITION DE LA METHODE DE 'RenderCharFieldFormat' DE RENDERER: 
        /// Fait un rendu du champ de type char
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">Classe css à appliquer</param>
        /// <param name="sClassAction">la type d action</param>
        protected override bool RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;

            // Si ce n'est pas un champ 'Email.de' on fait un rendu par défaut
            if (fieldRow != _fldReplyTo)
                return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            if (fieldRow.RightIsUpdatable)
            {
                #region Repondre a

                HtmlGenericControl selectReplyTo = new HtmlGenericControl("select");
                webControl.Controls.Add(selectReplyTo);
                selectReplyTo.ID = "replyto-opt";
                selectReplyTo.Attributes.Add("name", selectReplyTo.ID);
                selectReplyTo.Attributes.Add("class", "mailReply select-theme");


                AddMailOption(selectReplyTo, fieldRow, true);



                #endregion
            }
            else
            {
                GetValueContentControl(ednWebControl, fieldRow.DisplayValue);
            }

            return true;
        }

        /// <summary>
        /// Ajoute au <paramref name="selectCtrl"/> les emails de l'utilisateur
        /// </summary>
        /// <param name="selectCtrl">Select HTML a enrichir</param>
        /// <param name="fld">Champ destinataire</param>
        /// <param name="bCreateMainAliais">Indique si l'email principal doit être aliasser</param>
        /// <param name="bChangeDomainSender">Indique si le nom de domaine doit être changé (pour mapp par ex)</param>
        /// <param name="mailing">Objet mailing, nécessaire pour <paramref name="bChangeDomainSender"/></param>
        public void AddMailOption(HtmlGenericControl selectCtrl, eFieldRecord fld, bool bCreateMainAliais = false, bool bChangeDomainSender = false, eMailing mailing = null)
        {
            bool itemSelected = false;
            if (Pref.User.UserMailOther.Trim().Length > 0)
            {
                eMailAddressConteneur mConteneur = new eMailAddressConteneur(Pref.User.UserMailOther);
                foreach (eMailAddressInfos mInfo in mConteneur.LstAddress)
                {
                    string sFullAdr = mInfo.Mail;
                    MailAddress mail = new MailAddress(sFullAdr);

                    if (bCreateMainAliais && string.IsNullOrEmpty(mail.DisplayName) && mail.Address == Pref.User.UserMail)
                    {
                        //ALISTER => Demande #82 165
                        //Si l'email expediteur n'a pas d'alias, on prend le userDisplayValue par défaut
                        if (string.IsNullOrEmpty(mInfo.Name))
                            mail = new MailAddress(mInfo.Mail, Pref.User.UserDisplayName + " <" + Pref.User.UserMail + ">");
                        else
                            mail = new MailAddress(mInfo.Mail, mInfo.Name + " <" + Pref.User.UserMail + ">");

                        if (!string.IsNullOrEmpty(mail.DisplayName))
                            sFullAdr = mail.DisplayName;
                    }

                    HtmlGenericControl optionMailOther = new HtmlGenericControl("option");
                    //ALISTER => Demande #82 165
                    if (string.IsNullOrEmpty(mInfo.Name))
                    {
                        optionMailOther.Attributes.Add("dn", Pref.User.UserDisplayName);
                        mInfo.Name = Pref.User.UserDisplayName;
                        
                    }                        
                    else
                    {
                        optionMailOther.Attributes.Add("dn", mInfo.Name);
                       
                    }

                    optionMailOther.InnerText = mInfo.ToString();

                    //optionMailOther.Attributes.Add("value", mInfo.ToString());
                    //optionMailOther.InnerText = DecorateWithAliasDomain(mInfo.Mail);
                    
                    

                    if (MailIsSelected(fld, mail.Address))
                    {
                        optionMailOther.Attributes.Add("selected", "selected");
                        itemSelected = true;
                    }
                    selectCtrl.Controls.Add(optionMailOther);
                }
            }
            else
            {
                HtmlGenericControl optionMailMain = new HtmlGenericControl("option");
                optionMailMain.Attributes.Add("dn", Pref.User.UserDisplayName);
                optionMailMain.Attributes.Add("value", Pref.User.UserMail);
                //optionMailMain.InnerText = DecorateWithAliasDomain(Pref.User.UserMail);
                optionMailMain.InnerText = Pref.User.UserMail ;


                if (MailIsSelected(fld, Pref.User.UserMail) || fld.FileId == 0)
                {
                    optionMailMain.Attributes.Add("selected", "selected");
                    itemSelected = true;
                }

                selectCtrl.Controls.Add(optionMailMain);
            }

            //Cas d'un mail stocké dans la fiche existante, mais l'administrateur l'a supprimé dans la liste des mails de l'utilisateur en cours                
            if (!itemSelected && !string.IsNullOrEmpty(fld.Value) && fld.FileId > 0)
            {
                HtmlGenericControl optionMailOther = new HtmlGenericControl("option");
                if (_fldDisplayName != null)
                    optionMailOther.Attributes.Add("dn", _fldDisplayName.DisplayValue);

                optionMailOther.Attributes.Add("value", fld.Value);

                //TODO : vérifier la nécessité de DecorateWithAliasDomain.
                // pourquoi modifier le from/reply to sur une fiche existante ?
                optionMailOther.InnerText = bChangeDomainSender ? DecorateWithAliasDomain(fld.Value, mailing) : fld.Value;

                optionMailOther.Attributes.Add("selected", "selected");

                selectCtrl.Controls.Add(optionMailOther);
            }
        }



        /// <summary>
        /// Remplace le domaine de mail d'origine par l'alias de partenaire
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        protected string DecorateWithAliasDomain(string mail, eMailing mailing)
        {
            if (mailing.SendeType != MAILINGSENDTYPE.ECIRCLE || string.IsNullOrEmpty(mailing.SenderDomainAlias))
                return mail;

            return eLibTools.ReplaceOriginMailDomainWithAlias(mail, mailing.SenderDomainAlias) + " (" + mail + ")";
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fldFrom"></param>
        /// <param name="mail"></param>
        /// <returns></returns>
        protected virtual bool MailIsSelected(eFieldRecord fld, string mail)
        {
            return fld.Value.Trim().Equals(mail.Trim());
        }

        /// <summary>
        /// ajout d'un bouton permettant l'ajout de mails de contacts. non effectif dan sle cas de l'emailing
        /// </summary>
        /// <param name="myValueCell">Cellule de tableau dans laquelle le bouton doit être inséré</param>
        /// <param name="myField">Champ correspondant à la valeur à saisir (To, CC, BCC)</param>
        protected virtual void AddMailAddrBtn(TableCell myValueCell, eFieldRecord myField)
        {

            if (_myFile.ViewMainTable.EdnType != EdnType.FILE_MAIL && _myFile.ViewMainTable.EdnType != EdnType.FILE_SMS)
                return;

            if (myField.FldInfo.Descid != _myFile.CalledTabDescId + MailField.DESCID_MAIL_TO.GetHashCode()
                && myField.FldInfo.Descid != _myFile.CalledTabDescId + MailField.DESCID_MAIL_CC.GetHashCode()
                && myField.FldInfo.Descid != _myFile.CalledTabDescId + MailField.DESCID_MAIL_BCC.GetHashCode())
                return;

            if (_fldStatus == null)
                return;

            if (_fldStatus.Value != ((int)EmailStatus.MAIL_DRAFT).ToString()
                && _fldStatus.Value != ((int)EmailStatus.MAIL_NOT_SENT).ToString()
                && _fldStatus.Value != ""
                && !_bMailForward  // le bouton d'ajout de contacts s'affche aussi pour le mail transféré
                )
                return;

            // #48 903 - Le bouton ne doit pas être affiché si l'utilisateur ne dispose pas des droits adéquats
            if (!myField.RightIsUpdatable)
                return;

            //KHA ajout d'un bouton permettant l'ajout de mails de contacts.
            HtmlGenericControl spBtn = new HtmlGenericControl("span");
            myValueCell.Controls.Add(spBtn);

            int nIdx = 0;
            if (_rType == RENDERERTYPE.EditMailing && ((eEditMailingRenderer)this).UseCkeditor)
            {
                nIdx = 1;
            }

            spBtn.Attributes.Add("class", "icon-add_filter icnBkm");
            spBtn.Attributes.Add("onclick", String.Concat("addMailAddr('", eTools.GetFieldValueCellId(_myFile.Record, myField, nIdx), "', ", _myFile.ViewMainTable.EdnType.GetHashCode(), ");"));
        }

        protected TableCell GetCustomObjectTableCell(string labelClientId, string valueClientId, string valueField)
        {
            TableCell objectValueCell = new TableCell();
            if (!ReadonlyRenderer)
            {
                HtmlGenericControl containerDiv = new HtmlGenericControl("div");
                containerDiv.Attributes.Add("class", "value-container");
                containerDiv.Attributes.Add("onclick", "openMemo_TplObj('" + valueClientId + "')");
                //containerDiv.Style.Add("max-width", "155px");
                containerDiv.Style.Add("display", "block");

                HtmlGenericControl objDiv = new HtmlGenericControl("div");
                objDiv.Attributes.Add("id", valueClientId);
                objDiv.Attributes.Add("name", valueClientId);
                objDiv.Attributes.Add("ename", labelClientId);
                objDiv.Attributes.Add("class", "field-value");
                objDiv.Attributes.Add("efld", "1");
                objDiv.InnerHtml = valueField;
                //SHA KJE #2 476 : rubrique Objet obligatoire pour l'assistant mailing
                objDiv.Attributes.Add("obg", "1");
                containerDiv.Controls.Add(objDiv);


                HtmlGenericControl penDiv = new HtmlGenericControl("div");
                penDiv.Attributes.Add("class", "icon-edn-pen field-btn");
                containerDiv.Controls.Add(penDiv);


                objectValueCell.Controls.Add(containerDiv);

            }
            else
            {
                HtmlGenericControl objDiv = new HtmlGenericControl("div");
                objDiv.InnerHtml = valueField.Replace("\"", "'");
                objDiv.Attributes.Add("class", "icon-edn-pen field-btn");
                HtmlInputText inputDisplayName = new HtmlInputText();
                inputDisplayName.ID = valueClientId;
                inputDisplayName.Attributes.Add("class", "value-container readonly");
                inputDisplayName.Attributes.Add("readonly", "readonly");
                Regex regex = new Regex("(<.*?>\\s*)+", RegexOptions.Singleline);
                inputDisplayName.Value = regex.Replace(objDiv.InnerText.Trim(), " ").Trim();
                objectValueCell.Controls.Add(inputDisplayName);
            }
            objectValueCell.Attributes.Add("efld", "1");
            objectValueCell.RowSpan = 1;
            objectValueCell.ColumnSpan = 1;
            //objectValueCell.CssClass = String.Concat(objectValueCell.CssClass, " table_values ", customCssClass);
            return objectValueCell;
        }

        #endregion
    }
}