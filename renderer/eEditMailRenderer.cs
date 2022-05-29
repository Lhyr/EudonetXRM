using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Net.Mail;
using System.Linq;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu de fiche en modification et création
    /// </summary>
    public class eEditMailRenderer : eMailFileRenderer
    {
        /// <summary>
        /// Dictionnaire contenant les pjIds ainsi que les libellé des Pjs
        /// </summary>
        protected Dictionary<int, string> _dicoLstPj = new Dictionary<int, string>();

        #region CONSTRUCTEUR
        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nFileId"></param>
        /// <param name="nWidth">Largeur du renderer à créer</param>
        /// <param name="nHeight">hauteur du rencerer à créer</param>
        public eEditMailRenderer(ePref pref, Int32 nTab, Int32 nFileId, Int32 nWidth, Int32 nHeight, String strMailTo, Boolean bMailForward, Boolean bMailDraft, Boolean bEmailing)
            : base(pref, nTab, nFileId, nWidth, nHeight, strMailTo, false)
        {
            _bMailForward = bMailForward;
            _bMailDraft = bMailDraft;
            _rType = RENDERERTYPE.EditMail;
        }

        #endregion


        protected override bool Init()
        {
            // Charge le mail
            if (!base.Init())
                return false;

            String error = string.Empty;
            if (_dicoLstPj.Count == 0)
            {
                // Si fiche mail transféré, on remet à zéro le fileId
                if (MailForward)
                {
                    error = ePJTraitements.PjListCloneAndSelect(Pref, _myFile.ViewMainTable.DescId, _nFileId, out _dicoLstPj);
                    _nFileId = 0;
                }
                else
                    error = ePJTraitements.PjListSelect(Pref, _myFile.ViewMainTable.DescId, _nFileId, null, out _dicoLstPj);
            }

            if (error.Length > 0)
            {
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), Pref);
                return false;
            }

            return true;
        }

        /// <summary>
        /// REDIFINITION DE LA METHODE DE 'RenderCharFieldFormat' DE RENDERER: 
        /// Fait un rendu du champ de type char
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">Class CSS</param>
        /// <param name="sClassAction">la type d action</param>
        protected override Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;

            //Si ce n'est pas un champ 'Email.de' on fait un rendu par défaut
            if (fieldRow != _fldFrom)
                return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            if (fieldRow.RightIsUpdatable)
            {
                #region De

                HtmlGenericControl selectFrom = new HtmlGenericControl("select");
                webControl.Controls.Add(selectFrom);
                selectFrom.ID = String.Concat(webControl.ID, "_0");
                selectFrom.Attributes.Add("name", selectFrom.ID);
                selectFrom.Attributes.Add("class", "mailFrom");
                selectFrom.Attributes.Add(
                    "onchange",
                    String.Concat(
                        "document.getElementById('", webControl.ID.Replace(String.Concat("_", fieldRow.FldInfo.Descid, "_"), "_DN_"), "').value = this.options[this.selectedIndex].getAttribute('dn');",
                        "if (document.getElementById('", webControl.ID.Replace(String.Concat("_", fieldRow.FldInfo.Descid, "_"), "_RT_"), "')) { document.getElementById('",
                        webControl.ID.Replace(String.Concat("_", fieldRow.FldInfo.Descid, "_"), "_RT_"), "').value = this.options[this.selectedIndex].value; }"
                    )
                );

                HtmlGenericControl optionMailMain = new HtmlGenericControl("option");

                //Amna => Demande #85 245
                eMailAddressConteneur mMainConteneur = new eMailAddressConteneur(Pref.User.UserMailOther);

                if (mMainConteneur.LstAddress.Count() == 0)
                {
                    EudoException ee=  new EudoException("", eResApp.GetRes(Pref, 8533), launchFeedback:false);
                    ee.UserMessageTitle = eResApp.GetRes(Pref, 72);
                    ee.UserMessageDetails = eResApp.GetRes(Pref, 2943);
                    ee.SetCriticity(ERROR_CRITICITY.WARNING);
                    throw ee;
                   
                }

               // eMailAddressInfos mMainInfo = mMainConteneur.LstAddress.ElementAt<eMailAddressInfos>(0);
                if (!Pref.User.UserMailOther.Contains(Pref.User.UserMail))
                {
                    optionMailMain.Attributes.Add("value", Pref.User.UserMail);
                    string value = String.Concat(Pref.User.UserDisplayName, " <", Pref.User.UserMail, ">");
                    optionMailMain.InnerText = value;
                    optionMailMain.Attributes.Add("selected", "selected"); // TODO: demande de dév pour sélectionner le mail alternatif par défaut
                    selectFrom.Controls.Add(optionMailMain);
                }

                bool optionSelected = false;
                string inputDisplayNameValue = String.Empty;

                if (Pref.User.UserMailOther.Trim().Length > 0)
                {
                    eMailAddressConteneur mConteneur = new eMailAddressConteneur(Pref.User.UserMailOther);

                   

                    foreach (eMailAddressInfos mInfo in mConteneur.LstAddress)
                    {

                        MailAddress mail = new MailAddress(mInfo.Mail);

                        HtmlGenericControl optionMailOther = new HtmlGenericControl("option");
                        //ALISTER => Demande #82 165
                        //Si l'email expediteur n'a pas d'alias, on prend le userDisplayValue par défaut
                        if (string.IsNullOrEmpty(mInfo.Name))
                        {
                            optionMailOther.Attributes.Add("dn", Pref.User.UserDisplayName);
                            mInfo.Name = Pref.User.UserDisplayName;
                        }
                        else
                        {
                            optionMailOther.Attributes.Add("dn", mInfo.Name);
                        }

                        optionMailOther.Attributes.Add("value", mInfo.Mail);
                        optionMailOther.InnerText = mInfo.ToString();

                        if (!optionSelected && fieldRow.Value == mInfo.Mail)
                        {
                            optionMailOther.Attributes.Add("selected", "selected");
                            inputDisplayNameValue = mInfo.Name;
                            optionSelected = true;
                        }
                        selectFrom.Controls.Add(optionMailOther);
                    }
                }

                if (!optionSelected)
                {
                    optionMailMain.Attributes.Add("selected", "selected");
                    //inputDisplayNameValue = Pref.User.UserDisplayName;
                    inputDisplayNameValue = eLibTools.GetEmailSenderDisplayName(Pref, Pref.User);
                    optionSelected = true;
                }

                #endregion

                #region Nom apparent
                List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, false, String.Concat(webControl.Attributes["ename"], "_DN"), null, webControl.ID.Replace(String.Concat("_", fieldRow.FldInfo.Descid, "_"), "_DN_"), null);

                // On ajoute tous les champs générés
                foreach (Control ctrl in displayName)
                    if (ctrl != null)
                        webControl.Controls.Add(ctrl);

                #endregion
            }
            else
                GetValueContentControl(ednWebControl, fieldRow.DisplayValue);

            return true;
        }


        /// <summary>
        /// Génere des champs cachés
        /// </summary>
        /// <param name="pnlLeft"></param>
        protected override void RenderHiddenFields(Panel pnlLeft)
        {
            // On crée un champ caché pour permettre aux JS appelés par validateFile() de savoir que l'on est sur une fenêtre de transfert de mail
            HtmlInputHidden hiddenMailForward = new HtmlInputHidden();
            hiddenMailForward.ID = "mailForward";
            hiddenMailForward.Name = "mailForward";
            hiddenMailForward.Value = _bMailForward ? "1" : "0";
            pnlLeft.Controls.Add(hiddenMailForward);

            // On crée un champ caché pour permettre aux JS appelés par validateFile() de savoir que l'on a cliqué sur le bouton "Brouillon"
            HtmlInputHidden mailSaveAsDraft = new HtmlInputHidden();
            mailSaveAsDraft.ID = "mailSaveAsDraft";
            mailSaveAsDraft.Name = "mailSaveAsDraft";
            mailSaveAsDraft.Value = "0"; // La valeur de ce champ sera mise à 1 lorsqu'on cliquera sur le bouton "Brouillon"
            pnlLeft.Controls.Add(mailSaveAsDraft);

            // On crée un champ caché pour permettre aux JS appelés par validateFile() de savoir que l'on est en train d'éditer un mail
            HtmlInputHidden mailIsDraft = new HtmlInputHidden();
            mailIsDraft.ID = "mailIsDraft";
            mailIsDraft.Name = "mailIsDraft";
            mailIsDraft.Value = _bMailDraft ? "1" : "0";
            pnlLeft.Controls.Add(mailIsDraft);

            // On crée un champ caché pour permettre aux JS appelés par validateFile() de savoir que l'on a cliqué sur le bouton "Envoyer un e-mail de test"
            HtmlInputHidden mailIsTest = new HtmlInputHidden();
            mailIsTest.ID = "mailIsTest";
            mailIsTest.Name = "mailIsTest";
            mailIsTest.Value = "0"; // La valeur de ce champ sera mise à 1 lorsqu'on cliquera sur le bouton "Envoyer un e-mail de test"
            pnlLeft.Controls.Add(mailIsTest);

            // On crée un champ caché pour permettre aux JS appelés par validateFile() de connaître la liste des destinataires renseignés lors de l'utilisation d'"Envoyer un e-mail de test"
            HtmlInputHidden mailTestRecipients = new HtmlInputHidden();
            mailTestRecipients.ID = "mailTestRecipients";
            mailTestRecipients.Name = "mailTestRecipients";
            mailTestRecipients.Value = ""; // La valeur de ce champ sera indiquée par l'utilisateur dans une popup lorsqu'il cliquera sur le bouton "Envoyer un e-mail de test"
            pnlLeft.Controls.Add(mailTestRecipients);

            String id = string.Concat("COL_", this._tab, "_", _tab + MailField.DESCID_MAIL_STATUS);
            HtmlInputHidden mailStatus = new HtmlInputHidden();
            mailStatus.ID = string.Concat(id, "_", this._nFileId, "_", this._nFileId, "_0");

            mailStatus.Attributes.Add("did", (_tab + MailField.DESCID_MAIL_STATUS).ToString());
            mailStatus.Attributes.Add("ename", id);
            //if(_nFileId > 0)
            //    mailStatus.Attributes.Add("ero", "1");

            //ajout du champs 85 l'input cachée pour l engine.js pour pouvoir le reecupére avec getFieldsInfos()
            FieldsDescId.AddContains((this._tab + MailField.DESCID_MAIL_STATUS).ToString());

            // Ajout des champs cachés ou en lecture seule devant être autorisés en écriture lors de la mise à jour
            AllowedFieldsDescId.AddContains((this._tab + MailField.DESCID_MAIL_STATUS).ToString());


            mailStatus.Value = _bMailDraft ? EmailStatus.MAIL_DRAFT.GetHashCode().ToString() : "0";

            pnlLeft.Controls.Add(mailStatus);
        }

        /// <summary>
        /// Génère les liens "Choisir un modèle de mail" et "Joindre un fichier" et l'ajout à la table
        /// </summary>
        /// <returns></returns>       
        protected override Boolean RenderLinks(System.Web.UI.WebControls.TableRow tr)
        {
            // On prépare les contrôles qui vont être utilisés pour l'affichage de la liste des PJ
            Int32 pjDescId = _myFile.ViewMainTable.DescId + AllField.ATTACHMENT.GetHashCode();
            HtmlGenericControl spanPjCnt = new HtmlGenericControl("span");
            String cellName = eTools.GetFieldValueCellName(_myFile.ViewMainTable.DescId,
                String.Concat(_myFile.ViewMainTable.DescId, "_", pjDescId));
            spanPjCnt.Attributes.Add("ename", cellName);
            spanPjCnt.Attributes.Add("did", pjDescId.ToString());
            spanPjCnt.ID = eTools.GetFieldValueCellId(cellName, _myFile.Record.MainFileid, _myFile.Record.MainFileid);//"PjSpan";
            spanPjCnt.Attributes.Add("class", "pjCnt");

            TableCell tcLinksValue = new TableCell();
            tcLinksValue.RowSpan = 2;
            HtmlGenericControl ulLinks = new HtmlGenericControl("ul");
            ulLinks.Attributes.Add("class", "mailLinks");

            #region Choisir un modèle de mail
            //GCH #33692 : Ne pas afficher les modèles dans le cas d'un transfert
            if (!_bMailForward)
            {
                eRes res = new eRes(Pref, _myFile.Record.CalledTab.ToString());
                string tabLabel = res.GetRes(_myFile.Record.CalledTab);
                HtmlGenericControl liSelectMailTemplate = new HtmlGenericControl("li");
                liSelectMailTemplate.Attributes.Add("class", "mailSelTpl");

                // #41437 CRU : Remplacement image par EudoFont
                HtmlGenericControl imgSelectMailTemplate = new HtmlGenericControl("span");
                imgSelectMailTemplate.Attributes.Add("class", "icon-empty_mail");

                HtmlGenericControl lnkSelectMailTemplate = new HtmlGenericControl("a");
                lnkSelectMailTemplate.InnerText = eResApp.GetRes(Pref, 6318);
                liSelectMailTemplate.Attributes.Add("title", lnkSelectMailTemplate.InnerText);
                liSelectMailTemplate.Attributes.Add(
                    "onclick",
                    String.Concat("ShowMailTemplateList(",
                        "1, ",
                        "'", tabLabel.Replace("'", "\\'"), "', ",
                        _myFile.Record.CalledTab, ", ",
                        (_myFile.ParentFileId.HasParentLnk(TableType.EVENT)) ? _myFile.ParentFileId.ParentEvtId.ToString() : "0", ", ", // #42319 CRU : Argument = 0 si aucune liaison parente
                        "document.getElementById('", eTools.GetFieldValueCellId(_myFile.Record, _myFile.Record.GetFieldByAlias(String.Concat(_myFile.Record.CalledTab, "_", _myFile.Record.CalledTab + MailField.DESCID_MAIL_OBJECT))), "'), ",
                        //SHA : tâche #1 941
                        //"document.getElementById('", eTools.GetFieldValueCellId(_myFile.Record, _myFile.Record.GetFieldByAlias(String.Concat(_myFile.Record.CalledTab, "_", _myFile.Record.CalledTab + MailField.DESCID_MAIL_PREHEADER))), "'), ",
                        "nsMain.getMemoEditor('edt", eTools.GetFieldValueCellId(_myFile.Record, _myFile.Record.GetFieldByAlias(String.Concat(_myFile.Record.CalledTab, "_", _myFile.Record.CalledTab + MailField.DESCID_MAIL_HTML))), "'), ",
                        // #68 13x et Backlog #43, #375 + #409 - Editeur de templates HTML avancé (grapesjs) - Uniquement accessible en E2017 - Ajout du paramètre enableTemplateEditor - Non proposé pour les mails unitaires en v1
                        // grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer la condition _rType == RENDERERTYPE.EditMailing ci-dessous
                        (_rType == RENDERERTYPE.EditMailing && eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor)) ? "true" : "false",
                        ");"
                    )
                ); // les noms des variables JS représentant les champs Objet et Mémo à mettre à jour seront injectés au JS par la page ASPX
                liSelectMailTemplate.Controls.Add(imgSelectMailTemplate);
                liSelectMailTemplate.Controls.Add(lnkSelectMailTemplate);
                ulLinks.Controls.Add(liSelectMailTemplate);
            }
            #endregion

            #region Lien Joindre un fichier
            if (_myFile.FileId == 0 || _bMailForward || _bMailDraft)
            {
                // Récupération du signet Annexes afin de vérifier les droits d'ajout ainsi que le nombre de PJ
                eBookmark bkmAttachments = null;
                foreach (eBookmark bkm in _myFile.LstBookmark)
                {
                    if (bkm != null && bkm.ViewMainTable != null && bkm.ViewMainTable.DescId == TableType.PJ.GetHashCode())
                    {
                        bkmAttachments = bkm;
                        break;
                    }
                }
                // Affichage du lien
                // On va vérifier si on est en mode transfert pour donner l'accès à l'ajout de PJ
                if (bkmAttachments != null && bkmAttachments.IsAddAllowed && bkmAttachments.IsUpdateAllowed)
                {
                    HtmlGenericControl liPj = new HtmlGenericControl("li");

                    if (_myFile.Record.IsPJViewable)
                    {
                        #region Récupération des informations
                        // Récupération des PjIds 
                        string error = string.Empty;
                        string strPjIds = string.Empty;
                        try
                        {

                            if (_dicoLstPj != null && _dicoLstPj.Count > 0)
                                strPjIds = eLibTools.Join(";", _dicoLstPj, kv => { return kv.Key.ToString(); });

                        }
                        catch (Exception ex)
                        {
                            eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.ToString()), Pref);
                            return false;
                        }

                        spanPjCnt.Attributes.Add("PjIds", strPjIds);

                        // Indication du nombre de PJ
                        spanPjCnt.InnerText = eResApp.GetRes(Pref, 6349).Replace("<NUMBER>", _myFile.Record.PjCnt.ToString());



                        this.FieldsDescId.AddContains(pjDescId.ToString());
                        this.AllowedFieldsDescId.AddContains(pjDescId.ToString());

                        #endregion

                        // #41437 CRU : Remplacement image par EudoFont
                        HtmlGenericControl imgPjCnt = new HtmlGenericControl("span");
                        imgPjCnt.Attributes.Add("class", "icon-annex");

                        liPj.Attributes.Add("class", "liPjMail");
                        liPj.Attributes.Add("OnClick", string.Concat("showPJFromTpl('tplmail','", spanPjCnt.ID, "');"));
                        liPj.Attributes.Add("title", eResApp.GetRes(Pref, 6317));

                        HtmlGenericControl lnkPJ = new HtmlGenericControl();
                        lnkPJ.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6317)));

                        HtmlGenericControl dspCntPj = new HtmlGenericControl();
                        dspCntPj.InnerText = String.Concat("(", _dicoLstPj.Count, ")");
                        dspCntPj.ID = String.Concat("dspCntPj_", _myFile.ViewMainTable.DescId);

                        liPj.Controls.Add(imgPjCnt);
                        liPj.Controls.Add(lnkPJ);
                        liPj.Controls.Add(dspCntPj);

                        ulLinks.Controls.Add(liPj);
                    }
                }
            }

            tcLinksValue.Controls.Add(ulLinks);

            tr.Controls.Add(tcLinksValue);
            #endregion

            #region NBA - Liste des PJ // Annulé par KHA dans le cadre de la refonte des pièces jointes depuis template 12/06/2015

            //TableRow trPJList = new TableRow();
            //TableCell tbCell = new TableCell();
            //tbCell.ID = "tdVide";
            //trPJList.Cells.Add(tbCell);
            //tbCell = new TableCell();
            //tbCell.ID = "tdLstPj";
            //tbCell.CssClass = "tdLstPj";

            //Panel divLstPj = new Panel();
            //divLstPj.ID = "divlstPJMail";
            //divLstPj.CssClass = "divlstPJMail";
            ////divLstPj.Controls.Add(spanPjCnt);

            //HtmlGenericControl spanPj = new HtmlGenericControl("span");
            //spanPj.ID = "spanlstPj";
            //spanPj.Attributes.Add("class", "spanlstPj");
            //divLstPj.Controls.Add(spanPj);
            //tbCell.Controls.Add(divLstPj);


            //if (_myFile.ViewMainTable.PermViewPjAll && _myFile.FileId != 0)
            //{
            //    // Chargement de la liste des annexes à l'ouverture du template
            //    string linkdelete = string.Empty;
            //    if (_dicoLstPj.Count > 0)
            //    {
            //        foreach (var item in _dicoLstPj)
            //        {
            //            if (_bMailForward)
            //                linkdelete = string.Concat("<a onclick=\"DeletePJ('", _myFile.ViewMainTable.DescId, "', '", _myFile.FileId, "', '", item.Key, "', true, 'mail');\"><img  class=\"imgdelpj\"  src=\"", eConst.GHOST_IMG, "\"></a>");
            //            spanPj.InnerHtml = string.Concat(spanPj.InnerHtml, item.Value, linkdelete, " ");
            //        }

            //    }
            //    else
            //    {
            //        divLstPj.Style.Add("display", "none");
            //    }
            //}
            //else
            //    divLstPj.Style.Add("display", "none");

            //trPJList.Cells.Add(tbCell);
            //tab.Controls.Add(trPJList);

            #endregion

            return true;
        }


        /// <summary>
        /// Ajoute les ids et le nombre de pj appelé depuis End()
        /// </summary>
        /// <param name="pjDescId"></param>
        /// <param name="hidPj"></param>
        protected override void AppendPJAttributes(Int32 pjDescId, HtmlGenericControl hidPj)
        {
            if (!MailForward)
            {
                base.AppendPJAttributes(pjDescId, hidPj);

                // Ajout des IDs des PJ liés au modèle
                if (_mailingTemplate != null)
                {
                    if (_mailingTemplate.ListTemplatePJ != null)
                    {

                        String pjIds = hidPj.Attributes["PjIds"];
                        int nbPj = eLibTools.GetNum(hidPj.Attributes["nbpj"]) + _mailingTemplate.ListTemplatePJ.Count;

                        foreach (int pj in _mailingTemplate.ListTemplatePJ)
                        {
                            if (!String.IsNullOrEmpty(pjIds))
                                pjIds = String.Concat(pjIds, ";");

                            pjIds = String.Concat(pjIds, pj.ToString());
                        }

                        hidPj.Attributes.Add("PjIds", pjIds);
                        hidPj.Attributes.Add("nbpj", _dicoLstPj.Count.ToString());

                    }
                }
            }
            else
            {
                hidPj.Attributes.Add("PjIds", eLibTools.Join(";", _dicoLstPj, kv => { return kv.Key.ToString(); }));
                hidPj.Attributes.Add("nbpj", _dicoLstPj.Count.ToString());
            }
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
                    this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
                if (footRenderer != null)
                    pgC = footRenderer.PgContainer;
                _backBoneRdr.PnlDetailsBkms.Controls.Add(footRenderer.PgContainer);
            }
        }

        /// <summary>
        /// Recupere le fieldRecord de descid dans le record
        /// </summary>
        /// <param name="fldDescId"></param>
        /// <returns></returns>
        protected override eFieldRecord GetFldRecord(Int32 fldDescId)
        {
            eFieldRecord fldRcd = _myFile.Record.GetFieldByAlias(String.Concat(_myFile.ViewMainTable.DescId, "_", fldDescId));

            // Pour le mail transféré le fileId = 0
            if (MailForward && !fldRcd.IsLink)
                fldRcd.FileId = 0;

            return fldRcd;
        }

        /// <summary>
        /// Rendu de format de text
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldIsHTML(System.Web.UI.WebControls.TableRow tr)
        {
            // Format du mail (HTML ou texte)
            if (_fldIsHTML != null && _fldIsHTML.RightIsVisible)
            {
                List<TableCell> cells = RenderField(_fldIsHTML, String.Empty);

                string cellIdPrefix = "mailIsHTML_";
                // GCH - #32338 : L'envoi des mails unitaires est désormais forcé en mode HTML,
                // dans ce cas l'option est cachée car cela évite de faire de grosses modifications à l'engine.
                Dictionary<HtmlTextWriterStyle, string> cellStyles = new Dictionary<HtmlTextWriterStyle, string>();
                cellStyles.Add(HtmlTextWriterStyle.Display, "none");
                AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix, cellStyles);

                return true;
            }

            return false;
        }
    }
}