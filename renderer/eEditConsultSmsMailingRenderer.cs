using Com.Eudonet.Internal;
using EudoQuery;
using System;
using EudoExtendedClasses;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.Sms;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu SMS en mode Assistant - Ecriture
    /// </summary>
    public class eEditSmsMailingRenderer : eEditMailingRenderer
    {
        #region CONSTRUCTEUR

        StringBuilder _script = new StringBuilder();


        /// <summary>
        ///  Crée un objet pour le rendu de la fiche compaign en mode assistant
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="nTab">Table de l'email</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du renderer à créer</param>
        /// <param name="nHeight">Hauteur du renderer à créer</param>
        public eEditSmsMailingRenderer(ePref pref, Int32 nTab, eMailing mailing, Int32 nWidth, Int32 nHeight, String strMailTo, Boolean bMailForward, Boolean bMailDraft)
            : base(pref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft)
        {
            _rType = RENDERERTYPE.EditSMSMailing;
        }

        #endregion

        /// <summary>
        /// REDIFINITION DE LA METHODE DE 'RenderCharFieldFormat' DE RENDERER: 
        /// Fait un rendu du champ de type char
        /// </summary>
        /// <param name="row">Ligne de la liste a afficher</param>
        /// <param name="fieldRow">Le record</param>
        /// <param name="ednWebControl">elment html dans lequel on fait le rendu</param>
        /// <param name="sbClass">Classe css à appliquer</param>
        /// <param name="sClassAction">la type d action</param>
        protected override Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {

            WebControl webControl = ednWebControl.WebCtrl;

            if (fieldRow == _fldFrom)
            {
                Dictionary<string, string> inputDisplayNameAttr = new Dictionary<string, string>();
                inputDisplayNameAttr.Add("style", "400px");
                inputDisplayNameAttr.Add("phonenumber", _ePref.User.UserTel);
                string inputDisplayNameValue = eLibTools.GetEmailSenderDisplayName(Pref, Pref.User) + " <" + Pref.User.UserMail + ">";

                List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, true, null, null, null, inputDisplayNameAttr);

                // Pour le champ "De" des SMS, on ajoute uniquement le champ de saisie en tant que "Nom apparent"
                if (displayName != null && displayName.Count > 1 && displayName[1] != null)
                    webControl.Controls.Add(displayName[1]);

                return true;
            }
            else if (fieldRow == _fldRecipientTab)
            {
                /*
                HtmlSelect selectRecipientTab = new HtmlSelect();// HtmlGenericControl("select");  
                selectRecipientTab.ID = "selectRecipientTab";
                selectRecipientTab.Style.Add(HtmlTextWriterStyle.Width, "404px");
                selectRecipientTab.Attributes.Add("class", "mailReply select-theme");
                //SHA : correction bug "Le fichier de destination ne doit proposer que des onglet SMS. Actuellement, ce sont des onglets Email qui sont proposés"
                //FillEmailFiles(selectRecipientTab);
                FillSMSFiles(selectRecipientTab);

                webControl.Controls.Add(selectRecipientTab);
                */
                return true;
            }
            else if (fieldRow == _fldTo)
            {

                HtmlSelect selectPhoneFields = new HtmlSelect();// HtmlGenericControl("select");
                selectPhoneFields.ID = "selectPhoneFields";
                selectPhoneFields.Attributes.Add("class", "mailReply select-theme");
                selectPhoneFields.Style.Add(HtmlTextWriterStyle.Width, "404px");

                fillPhoneFields(selectPhoneFields);

                webControl.Controls.Add(selectPhoneFields);

                return true;
            }
            else
            {

                return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            }
        }


        /// <summary>
        /// Fait le rendu de champ de type memo
        /// </summary>
        /// <param name="Pref">preference utilisateur</param>
        /// <param name="themePaths">path de them</param>
        /// <param name="fieldRow"></param>
        /// <param name="webControl">control sur lequel on fait un rendu </param>
        /// <param name="sClassAction">class css a metter a jour</param>
        protected override void RenderMemoFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebCtrl, WebControl webControl, StringBuilder sbClass, ref String sClassAction)
        {
            webControl.Attributes.Add("html", "0");
            GetRawMemoControl(ednWebCtrl, fieldRow.DisplayValue);
            MemoIds.Add(webControl.ID);

        }



        /// <summary>
        /// Récupère la liste des champs de la fiche E-Mail / 
        /// </summary>
        /// <param name="sortedFields"></param>
        /// <param name="systemFields"></param>
        /// <returns></returns>
        protected override List<eFieldRecord> GetHeaderFields(List<eFieldRecord> sortedFields, List<eFieldRecord> systemFields)
        {
            String[] aHeaderFields = eConst.CAMPAIGN_HEADER_FIELDS.Split(';');
            List<String> lHeaderFields = new List<string>();
            lHeaderFields.AddRange(aHeaderFields);
            List<eFieldRecord> headerFields = new List<eFieldRecord>();

            // Séparation champs système / champs utilisateur
            foreach (eFieldRecord fld in sortedFields)
            {
                if (
                    fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())


                    || fld.FldInfo.Descid == CampaignField.SENDER.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.MAILADDRESSDESCID.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.RECIPIENTTABID.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.BODY.GetHashCode()
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == CampaignField.PREHEADER.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.SENDTYPE.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.STATUS.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.DATESENT.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.DISPLAYNAME.GetHashCode()
                    )
                {
                    headerFields.Add(fld);
                }
            }

            foreach (eFieldRecord fld in systemFields)
            {
                if (
                     fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())

                    || fld.FldInfo.Descid == CampaignField.SENDER.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.MAILADDRESSDESCID.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.RECIPIENTTABID.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.BODY.GetHashCode()
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == CampaignField.PREHEADER.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.SENDTYPE.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.STATUS.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.DATESENT.GetHashCode()
                    || fld.FldInfo.Descid == CampaignField.DISPLAYNAME.GetHashCode()
                    )
                    headerFields.Add(fld);
            }

            return headerFields;
        }

        /// <summary>
        /// Recupere les champs de la fiche Compaign et les stocke dans des varaibale de class
        /// Cela permet des les ordonner correctement
        /// </summary>
        /// <param name="fields">Les champs de la fiche</param>
        protected override void RetrieveFields(List<eFieldRecord> fields)
        {
            foreach (eFieldRecord fld in fields)
            {
                if (fld.FldInfo.Table.DescId != TableType.CAMPAIGN.GetHashCode())
                    continue;

                switch ((CampaignField)fld.FldInfo.Descid)
                {

                    case CampaignField.STATUS:
                        _fldStatus = fld;
                        break;
                    case CampaignField.SENDER:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldFrom = fld;
                        break;
                    case CampaignField.DATESENT:
                        _fldDate = fld;
                        break;
                    case CampaignField.MAILADDRESSDESCID:
                        fld.RightIsUpdatable = true;
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        fld.FldInfo.Libelle = eResApp.GetRes(_ePref, 389);
                        _fldTo = fld;
                        break;

                    case CampaignField.BODY:
                        _fldBody = fld;
                        _fldBody.FldInfo.Format = FieldFormat.TYP_MEMO;
                        _fldBody.FldInfo.Libelle = eResApp.GetRes(_ePref, 383);
                        break;

                    case CampaignField.RECIPIENTTABID:
                        _fldRecipientTab = fld;
                        _fldRecipientTab.FldInfo.Format = FieldFormat.TYP_CHAR;
                        break;
                    case CampaignField.DISPLAYNAME:
                        _fldDisplayName = fld;
                        _fldDisplayName.FldInfo.Format = FieldFormat.TYP_CHAR;
                        break;


                }
            }
        }

        /// <summary>
        /// Faire un rendu de chaque champ dans la table en parametre
        /// </summary>
        /// <param name="tab">Tableau HTML sur lequel ajouter les champs</param>
        protected override void RenderFields(System.Web.UI.WebControls.Table tab)
        {
            AddAttr(tab);

            TableRow tr = new TableRow();
            tab.Rows.Add(tr);

            // De
            // RenderFldFrom(tr);

            tr = new TableRow();
            tab.Rows.Add(tr);

            RendererCampaignInfo(tab);

            // Fichier Destinataires
            RenderFldToOption(tab);

            // Corps (dans sa propre ligne de tableau, il n'est donc pas nécessaire de faire un new TableRow() ici
            RenderFldBody(tab);

            tr = new TableRow();
            tab.Rows.Add(tr);

            // Le compteur de caracatère
            RenderCharCounterLabel(tab);
            tr = new TableRow();
            tab.Rows.Add(tr);

            //sauvegarder le model si droit ok
            eRightMailTemplate traitRight = new eRightMailTemplate(_ePref);
            if (traitRight.CanAddNewItem() || traitRight.CanEditItem())
            {
                tr = new TableRow();
                tab.Rows.Add(tr);
                TableCell cellSaveBtn = new TableCell();
                tr.Cells.Add(cellSaveBtn);
                RenderSaveButton(cellSaveBtn);
            }


            // Autres options
            tr = new TableRow();
            tab.Rows.Add(tr);
            RenderMailTabOption(tr);
        }

        /// <summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected virtual void RendererCampaignInfo(System.Web.UI.WebControls.Table tab)
        {
            TableRow trCampagne = new TableRow();
            TableRow trCampagneDate = new TableRow();
            TableRow trDescriptionLabel = new TableRow();
            TableRow trDescriptionField = new TableRow();

            TableCell labelCampagne = new TableCell();
            HtmlGenericControl labelCampaignInfoLabel = new HtmlGenericControl("label");
            labelCampaignInfoLabel.InnerHtml = eResApp.GetRes(Pref, 6407); // Campagne :
            labelCampagne.Attributes.Add("class", " table_labels");
            labelCampagne.Controls.Add(labelCampaignInfoLabel);
            trCampagne.Cells.Add(labelCampagne);

            TableCell labelCampagneDate = new TableCell();
            HtmlGenericControl labelCampaignDateInfo = new HtmlGenericControl("label");
            labelCampaignDateInfo.InnerHtml = String.Concat(' ', this._mailing.MailingParams["libelle"]);
            labelCampagneDate.Attributes.Add("class", " table_labels");
            labelCampagneDate.Controls.Add(labelCampaignDateInfo);
            trCampagneDate.Cells.Add(labelCampagneDate);

            TableCell labelDescriptionLabel = new TableCell();
            HtmlGenericControl labelCampaignDescriptionLabel = new HtmlGenericControl("label");
            labelCampaignDescriptionLabel.InnerHtml = eResApp.GetRes(Pref, 6410); // Description
            labelDescriptionLabel.Attributes.Add("class", " table_labels");
            labelDescriptionLabel.Controls.Add(labelCampaignDescriptionLabel);
            trDescriptionLabel.Cells.Add(labelDescriptionLabel);

            TableCell labelDescriptionField = new TableCell();
            HtmlInputText txtCampaignInfoDescription = new HtmlInputText();
            labelDescriptionField.Controls.Add(txtCampaignInfoDescription);
            txtCampaignInfoDescription.Value = this._mailing.MailingParams["description"];
            txtCampaignInfoDescription.Attributes.Add("onblur", "oSmsing.SetParam('description', this.value);");
            txtCampaignInfoDescription.Attributes.Add("class", "smsingCampaignInfoDescription");
            int length = 255;
            foreach(Field field in this.File.FldFieldsInfos )
            {
                if(field.Descid == (int)CampaignField.DESCRIPTION)
                {
                    length = field.Length;
                }
            }
            txtCampaignInfoDescription.Attributes.Add("maxlength", length.ToString());
            trDescriptionField.Cells.Add(labelDescriptionField);

            tab.Controls.Add(trCampagne);
            tab.Controls.Add(trCampagneDate);
            tab.Controls.Add(trDescriptionLabel);
            tab.Controls.Add(trDescriptionField);
        }

        /// <summary>
        /// Une tableCell si c'est un replyTo
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        protected override EdnWebControl CreateEditEdnControl(eFieldRecord fieldRow)
        {
            if (fieldRow == _fldTo || fieldRow == _fldRecipientTab)
                return new EdnWebControl() { WebCtrl = new TableCell(), TypCtrl = EdnWebControl.WebControlType.TABLE_CELL };

            return new EdnWebControl() { WebCtrl = new TextBox(), TypCtrl = EdnWebControl.WebControlType.TEXTBOX };


        }

        /// <summary>
        /// Rendu du champ A
        /// </summary>
        /// <param name="tab">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        protected virtual void RenderFldToOption(System.Web.UI.WebControls.Table tab)
        {
            // Répondre à
            if (_fldTo != null && _fldTo.RightIsVisible)
            {
                // Répondre à
                List<TableCell> cells = RenderField(_fldTo, "mailReplyTo");
                string cellIdPrefix = "mailReplyTo_";
                //AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);.
                TableRow tr = new TableRow();
                tab.Rows.Add(tr);
                tr.Cells.Add(cells[0]);

                TableRow mytr = new TableRow();
                tab.Rows.Add(mytr);
                mytr.Cells.Add(cells[1]);
            }
        }

        /// <summary>
        /// remplit la liste champs de type tel dans le select 
        /// </summary>
        /// <param name="select"></param>
        private void fillPhoneFields(HtmlSelect select)
        {
            String err;
            Dictionary<Int32, String> dico = eDataTools.GetAllowedDescIdByFormat(Pref, _nTabFrom, FieldFormat.TYP_PHONE);
            foreach (var kv in dico)
                select.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));
        }

        /// <summary>
        /// Ajoute des attributs
        /// </summary>
        /// <param name="tab"></param>
        protected virtual void AddAttr(System.Web.UI.WebControls.Table tab)
        {
            tab.ID = "tablesms"; //.Add("class", "table-sms");
            tab.Attributes.Add("class", "tablesms");
        }

        /// <summary>
        ///Fait le rendu de choix de la table mail ou sauvegardé les sms
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        protected virtual void RenderMailTabOption(System.Web.UI.WebControls.TableRow tr)
        {
        }

        /// <summary>
        /// Récupère la liste des tables mail
        /// TOCHECK SMS : à recâbler avec FillSMSFiles ?
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        private Boolean FillEmailFiles(HtmlSelect select)
        {
            String err;
            if (eTools.FillEmailFiles(Pref, select, out err))
                return true;


            _eException = new Exception(err);
            return false;
        }

        //SHA : correction bug "Le fichier de destination ne doit proposer que des onglet SMS. Actuellement, ce sont des onglets Email qui sont proposés"
        /// <summary>
        /// Récupère la liste des tables SMS
        /// </summary>
        /// <param name="select"></param>
        /// <returns></returns>
        private Boolean FillSMSFiles(HtmlSelect select)
        {
            String err;
            if (eTools.FillSMSFiles(Pref, select, out err))
                return true;


            _eException = new Exception(err);
            return false;
        }

        /// <summary>
        ///Libellé du compteur de caractère
        /// </summary>
        /// <param name="tab">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        protected virtual void RenderCharCounterLabel(System.Web.UI.WebControls.Table tab)
        {
            //Label nombre char
            TableRow tr = new TableRow();
            TableCell cell = new TableCell();

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("class", "gray-text-italic");

            HtmlGenericControl nbChar = new HtmlGenericControl("label");
            nbChar.ID = "smsMaxCharId";
            //nbChar.InnerHtml = _fldBody == null ? _fldBody.DisplayValue.Length.ToString() : "0";
            span.Controls.Add(nbChar);

            HtmlGenericControl nbCharMax = new HtmlGenericControl("label");
            nbCharMax.ID = "smsMaxCharMsgId";
            //nbCharMax.InnerHtml = "/160 " + eResApp.GetRes(Pref.LangId,1461) + " " + eResApp.GetRes(Pref.LangId,8767) + " 1 SMS";
            span.Controls.Add(nbCharMax);

            cell.Controls.Add(span);
            tr.Cells.Add(cell);
            tab.Controls.Add(tr);

            //Messages à afficher après compter le nombre de caractères de l'SMS
            TableRow trMergeFieldsMsg = new TableRow();
            TableCell tcMergeFieldsMsg = new TableCell();
            HtmlGenericControl spanMergeFieldsMsg = new HtmlGenericControl("span");
            spanMergeFieldsMsg.Attributes.Add("id", "spnMergeFieldsMsg");
            spanMergeFieldsMsg.Attributes.Add("class", "gray-text-italic");
            tcMergeFieldsMsg.Controls.Add(spanMergeFieldsMsg);
            trMergeFieldsMsg.Cells.Add(tcMergeFieldsMsg);
            tab.Controls.Add(trMergeFieldsMsg);

            TableRow trCharNotSupportedMsg = new TableRow();
            TableCell tcCharNotSupportedMsg = new TableCell();
            HtmlGenericControl spanCharNotSupportedMsg = new HtmlGenericControl("span");
            spanCharNotSupportedMsg.Attributes.Add("id", "spnCharNotSupportedMsg");
            spanCharNotSupportedMsg.Attributes.Add("class", "gray-text-italic");
            tcCharNotSupportedMsg.Controls.Add(spanCharNotSupportedMsg);
            trCharNotSupportedMsg.Cells.Add(tcCharNotSupportedMsg);
            tab.Controls.Add(trCharNotSupportedMsg);

            //tr.Cells.Add(new TableCell()); // cellule ville pour le "libellé" et la structure du tableau
            
        }


        /// <summary>
        /// rendu du btn de sauvegarde
        /// </summary>
        /// <param name="wc">WebControl recevant le btn de sauvegarde</param>
        protected virtual void RenderSaveButton(WebControl wc)
        {            //sauvegarder le model
 
            HtmlGenericControl btnSave = new HtmlGenericControl("div");
            HtmlGenericControl divSave = new HtmlGenericControl("div");
            btnSave.Attributes.Add("class", "button-gray smsingSaveTplBtn  button-position-left");
            btnSave.ID = "savetemplate_btn-sms";
            divSave.Attributes.Add("type", "button");
            divSave.InnerHtml = eResApp.GetRes(Pref, 8761);
            divSave.Attributes.Add("onclick", "oSmsing.SaveAsTemplate()");
            divSave.Attributes.Add("class", "smsingSaveTplBtn button-gray-mid");
            divSave.ID = "savecampaign_btn-mid-sms";
            btnSave.Controls.Add(divSave);
            wc.Controls.Add(btnSave);

        }




        /// <summary>
        /// rendu du btn de open template liste
        /// </summary>
        /// <param name="wc">WebControl recevant le btn d'ouverture</param>
        protected virtual void RenderButtonOpenTempalteList(WebControl wc)
        {            

            HtmlGenericControl btnSave = new HtmlGenericControl("span");
            btnSave.Controls.Add( new LiteralControl( eResApp.GetRes(_ePref, 3048)));
            btnSave.Attributes.Add("onclick", "oSmsing.ShowTemplateList()");
            btnSave.Attributes.Add("class", "smsinShowTplBtn");
            btnSave.ID = "smsingShowTplLstBtn";
            wc.Controls.Add(btnSave);

        }

        /// <summary>
        /// Ajout des fonctions ou des variables js (champs de fusion, liens tracking ..)
        /// </summary>
        /// <param name="strScriptBuilder"></param>
        protected override void AppendScript(StringBuilder strScriptBuilder)
        {
            strScriptBuilder.Append(_script.ToString());
        }

        /// <summary>
        /// Ajoute les liaisons parentes en pied de page
        /// </summary>
        protected override void AddParentInFoot()
        {
            // ajout du pied de page contenant les informations parentes en popup
            // if (_bPopupDisplay)
            //{
            //    eRenderer footRenderer = eRendererFactory.CreateParenttInFootRenderer(Pref, this);
            //    Panel pgC = null;

            //    footRenderer.Generate();

            //    if (footRenderer.ErrorMsg.Length > 0)
            //        this._sErrorMsg = footRenderer.ErrorMsg;    //On remonte l'erreur
            //    if (footRenderer != null)
            //        pgC = footRenderer.PgContainer;
            //    _backBoneRdr.PnlDetailsBkms.Controls.Add(footRenderer.PgContainer);
            //}
        }

        /// <summary>
        /// Rendu du statut de l'emailing
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected override Boolean RenderStatusHeader(System.Web.UI.WebControls.Table tab)
        {
            //On affiche pas le statut si on est en preparation (mode assistant)
            if (!ReadonlyRenderer)
                return true;

            // Statut du mail/Envoyé le (Date) - Mail unitaire uniquement
            TableRow trStatusSent = new TableRow();
            TableCell tcStatus = new TableCell();

            if (_fldStatus != null)
            {
                //TODO: déterminer statut brouillon/envoyé/non envoyé selon info stockée en base
                if (_fldStatus.DisplayValue.Length > 0)
                {

                    //Génération du rendu HTML pour le champ
                    HtmlInputText inputDisplayName = new HtmlInputText();
                    inputDisplayName.Attributes.Add("class", "mail-head-text-sent");
                    inputDisplayName.Disabled = true;
                    inputDisplayName.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
                    tcStatus.Controls.Add(inputDisplayName);
                    trStatusSent.Controls.Add(tcStatus);
                    tab.Controls.Add(trStatusSent);

                    if (_fldDate != null)
                    {
                        //TODO voir si on affiche les autre statuts - personaliser le libellé -
                        if (_fldDate.DisplayValue.Length > 0)
                        {
                            //TODO : personnaliser le message selon l'opérateur SMS.

                            if (_fldStatus != null && _fldStatus.Value == ((int)CampaignStatus.MAIL_SENT).ToString())
                                inputDisplayName.Value = String.Concat(eResApp.GetRes(Pref.LangId, 6668), " ", eResApp.GetRes(Pref.LangId, 2337), ", ", eResApp.GetRes(Pref.LangId, 1058), " ", _fldDate.DisplayValue);
                            else
                                inputDisplayName.Value = "";
                        }
                    }
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Rendu de l'expediteur
        /// </summary>
        /// <param name="tab">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected Boolean RenderFldFrom(System.Web.UI.WebControls.Table tab)
        {
            // Expéditeur (From - De), Nom apparent si Email
            if (_fldFrom != null && _fldFrom.RightIsVisible)
            {
                // De + Nom Apparent (sur la même ligne)
                List<TableCell> cells = RenderField(_fldFrom, "mailFrom");

                string cellIdPrefix = "mailFrom_";
                //AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);
                TableRow tr = new TableRow();
                tab.Rows.Add(tr);



                tr.Cells.Add(cells[0]);



                TableRow mytr = new TableRow();
                tab.Rows.Add(mytr);
                mytr.Cells.Add(cells[1]);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Rendu de Destinataire
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected Boolean RenderHiddenFldTo(System.Web.UI.WebControls.TableRow tr)
        {
            // A (To)
            if (_fldTo != null && _fldTo.RightIsVisible)
            {
                List<TableCell> cells = RenderField(_fldTo, "mailTo");

                string cellIdPrefix = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_mail_to_");
                // l'option est cachée par défaut - TODO MAB #68 13x
                Dictionary<HtmlTextWriterStyle, string> cellStyles = new Dictionary<HtmlTextWriterStyle, string>();
                cellStyles.Add(HtmlTextWriterStyle.Display, "none");
                AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                return true;
            }

            return false;
        }


        /// <summary>
        /// Rendu du corps de mail avec spécificités de l'assistant d'e-mailing
        /// </summary>
        /// <param name="bCkEditor">force l'éditeur a être ck plutot que graphs</param>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected override bool RenderFldBody(System.Web.UI.WebControls.Table tab, bool bCkEditor = false)
        {
            // Corps
            if (_fldBody == null || !_fldBody.RightIsVisible)
                return false;

            //Le header
            //Création du cartouche d'entête
            TableRow myTr = new TableRow();
            TableRow tr = new TableRow();

            //Création de la cellule
            Label myLabel = new Label();
            TableCell myLabelCell = new TableCell();
            TableCell myValueCell = new TableCell();

            //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
            GetFieldLabelCell(myLabel, _myFile.Record, _fldBody);
            myLabel.Attributes.Add("fmt", _fldBody.FldInfo.Format.GetHashCode().ToString());
            // myLabel.Style.Add("display", "none");
            myLabelCell.Controls.Add(myLabel);

            //bouton template
            Label myBtnTemplate = new Label();

            eRightMailTemplate traitRight = new eRightMailTemplate(_ePref);
            if (traitRight.CanDisplayItemList() )
            {

                RenderButtonOpenTempalteList(myBtnTemplate);

                myLabelCell.Controls.Add(myBtnTemplate);
            }

            myLabelCell.Attributes.Add("class", "table_labels_top");
            //myLabelCell.Controls.Add(yourSMS);

            myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, _fldBody, 0, Pref);
            myValueCell.Style.Add(HtmlTextWriterStyle.Width, "600px");
            myValueCell.Style.Add(HtmlTextWriterStyle.Height, "120px");

 

            myValueCell.Attributes.Add("listeners", "textareaevents");

            // Champ Mémo Texte brut
            bool bIsHtml=false;
            SmsNetMessageSettingsClient settingsClientExtension = eLibTools.GetSerializedSMSSettingsExtension(_ePref);
            if (!string.IsNullOrEmpty(settingsClientExtension.ApiKeyRest))
                bIsHtml = true;

       
            //myValueCell.Attributes.Add("onkeypress", "return oSmsing.LengthSMSmessage(this);");
            //myValueCell.Attributes.Add("onpaste", "oSmsing.LengthSMSmessage()");

            myValueCell.Attributes.Add("html", bIsHtml ? "1" : "0");
            myValueCell.Attributes.Add("toolbartype", "smsing");
            myValueCell.Attributes.Add("editortype", "smsmailing");
            myValueCell.Attributes.Add("enabletemplateeditor", "0");
            myValueCell.Attributes.Add("mergefieldsjsvarname", "smsMergeFields");
             
            myTr.Cells.Add(myLabelCell);
            tr.Cells.Add(myValueCell);

            tab.Controls.Add(myTr);
            tab.Controls.Add(tr);

            //SHA : backlog #1 104
            // Le compteur de caratères
            if (_rType == RENDERERTYPE.SMSFile)
                RenderCharCounterLabel(tab);
            return true;

        }

        /// <summary>
        /// Créée un label
        /// </summary>
        protected override HtmlGenericControl CreateHeaderLabel()
        {

            HtmlGenericControl label = new HtmlGenericControl("div");
            label.InnerText = eResApp.GetRes(Pref.LangId, 8755);
            label.Attributes.Add("class", "mail-head-text");
            return label;

        }

    }

    /// <summary>
    /// classe de rendu SMS en mode Assistant - Lecture
    /// </summary>
    public class eConsultSmsMailingRenderer : eEditSmsMailingRenderer
    {

        /// <summary>
        ///  Crée un objet pour le rendu de la fiche compaign en mode assistant
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="nTab">Table de l'email</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du renderer à créer</param>
        /// <param name="nHeight">Hauteur du renderer à créer</param>
        public eConsultSmsMailingRenderer(ePref pref, Int32 nTab, eMailing mailing, Int32 nWidth, Int32 nHeight, String strMailTo, Boolean bMailForward, Boolean bMailDraft)
            : base(pref, nTab, mailing, nWidth, nHeight, strMailTo, bMailForward, bMailDraft)
        {
            _rType = RENDERERTYPE.SMSFile;
        }

        /// <summary>
        /// Faire un rendu de chaque champ dans la table en parametre
        /// </summary>
        /// <param name="tab">table html</param>
        protected override void RenderFields(System.Web.UI.WebControls.Table tab)
        {
            AddAttr(tab);

            RenderStatusHeader(tab);

            TableRow tr = new TableRow();
            tab.Rows.Add(tr);

            //From
            RenderFldFrom(tab);

            //Destinataire
            RenderFldToOption(tab);

            //corps
            RenderFldBody(tab);

            //SHA : backlog #1 104
            if (_rType != RENDERERTYPE.SMSFile)
                // Le compteur de caractères
                RenderCharCounterLabel(tab);

            // Le compteur de caracatère
            RenderMailTabOption(tr);
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
        protected override Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref String sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;

            if (fieldRow == _fldFrom || fieldRow == _fldRecipientTab || fieldRow == _fldTo)
            {
                Dictionary<string, string> inputDisplayNameAttr = new Dictionary<string, string>();
                inputDisplayNameAttr.Add("style", "400px");
                inputDisplayNameAttr.Add("phonenumber", _ePref.User.UserTel);

                string inputDisplayNameValue = String.Empty;
                if (fieldRow == _fldFrom && _fldDisplayName != null)
                    inputDisplayNameValue = _fldDisplayName.DisplayValue + " <" + _fldFrom.DisplayValue + ">";
                else
                    inputDisplayNameValue = fieldRow.DisplayValue;

                List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, true, null, null, null, inputDisplayNameAttr);

                // Pour le champ "De" des SMS, on ajoute uniquement le champ de saisie en tant que "Nom apparent"
                if (displayName != null && displayName.Count > 1 && displayName[1] != null)
                    webControl.Controls.Add(displayName[1]);

                return true;
            }

            return false;
        }
    }
}