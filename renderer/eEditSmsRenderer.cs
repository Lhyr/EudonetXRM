using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu SMS unitaire
    /// </summary>
    public class eEditSmsRenderer : eEditMailRenderer
    {
        #region CONSTRUCTEUR

        StringBuilder _script = new StringBuilder();


        /// <summary>
        /// Affichage pour la création et la modification
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="nTab">Table de l'email</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du renderer à créer</param>
        /// <param name="nHeight">Hauteur du renderer à créer</param>
        public eEditSmsRenderer(ePref pref, Int32 nTab, Int32 nFileId, Int32 nWidth, Int32 nHeight, String strMailTo, Boolean bMailForward, Boolean bMailDraft, Boolean bEmailing)
            : base(pref, nTab, nFileId, nWidth, nHeight, strMailTo, bMailForward, bMailDraft, bEmailing)
        {
            _rType = RENDERERTYPE.EditSMS;

            _script = _script.Append(BuildSmsScript());
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
                string inputDisplayNameValue = eLibTools.GetEmailSenderDisplayName(Pref, Pref.User) + " <" + Pref.User.UserTel + ">";

                List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, true, null, null, null, inputDisplayNameAttr);

                // Pour le champ "De" des SMS, on ajoute uniquement le champ de saisie en tant que "Nom apparent"
                if (displayName != null && displayName.Count > 1 && displayName[1] != null)
                    webControl.Controls.Add(displayName[1]);

                return true;
            }
            else
            {
                return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
            }
        }


        /// <summary>
        /// Champ spécifique SMS
        /// </summary>
        /// <param name="fields"></param>
        protected override void RetrieveFields(List<eFieldRecord> fields)
        {
            base.RetrieveFields(fields);

            _fldStatus = fields.FirstOrDefault(fld =>
            fld.FldInfo.Descid == _myFile.ViewMainTable.DescId + (int)MailField.DESCID_SMS_STATUS);

            //Mode consultation SMS
            if ((_fldDate != null && _fldDate.DisplayValue.Length > 0)  || _nFileId > 0)
                _fldBody.RightIsUpdatable = _fldTo.RightIsUpdatable = false;
        }


        /// <summary>
        /// Fait le rendu de champ de type mémo
        /// </summary>
        /// <param name="row">ligne d'enregisstremet  afficher</param>        
        /// <param name="fieldRow">champ a afficher</param>
        /// <param name="webControl">control sur lequel on fait un rendu </param>
        /// <param name="ednWebCtrl"></param>
        /// <param name="sbClass">non utilisé pour sms</param>
        /// <param name="sClassAction">non utilisé pour sms</param>
        protected override void RenderMemoFieldFormat(eRecord row,
            eFieldRecord fieldRow,
            EdnWebControl ednWebCtrl,
            WebControl webControl,
            StringBuilder sbClass,
            ref String sClassAction)
        {
            webControl.Attributes.Add("html", "0");
            GetRawMemoControl(ednWebCtrl, fieldRow.DisplayValue);
            MemoIds.Add(webControl.ID);

        }

        /// <summary>
        /// Faire un rendu de chaque champ dans la table en parametre
        /// </summary>
        /// <param name="tab">table html</param>
        protected override void RenderFields(System.Web.UI.WebControls.Table tab)
        {

            //ALISTER => Demande #79 357
            RenderStatusHeader(tab);

            AddAttr(tab);

            //From
            //RenderFldFrom(tr);

            //Destinataire
            RenderFldTo(tab);

            //corps
            RenderFldBody(tab);

            TableRow tr = new TableRow();
            tab.Rows.Add(tr);

            //Mode consultation SMS
            if ((_fldDate == null || _fldDate.DisplayValue.Length <= 0) && _nFileId <= 0)
            {
                // Le compteur de caracatère
                RenderCharCounterLabel(tab);

                // sauvegarder le model si droit ok
                eRightMailTemplate traitRight = new eRightMailTemplate(_ePref);
                if (traitRight.CanAddNewItem() || traitRight.CanEditItem())
                {
                    tr = new TableRow();
                    tab.Rows.Add(tr);
                    TableCell cellSaveBtn = new TableCell();
                    tr.Cells.Add(cellSaveBtn);
                    RenderSaveButton(cellSaveBtn);
                }
            }
            // Le compteur de caracatère
            //RenderMailTabOption(tr);

            //Status
            RenderFldStatus(tab);
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
            nbChar.InnerHtml = _fldBody != null ? _fldBody.DisplayValue.Length.ToString() : "0";
            span.Controls.Add(nbChar);

            HtmlGenericControl nbCharMax = new HtmlGenericControl("label");

            nbCharMax.ID = "smsMaxCharMsgId";

            //nbCharMax.InnerHtml = " " + eResApp.GetRes(Pref.LangId, 1461);
            span.Controls.Add(nbCharMax);

            //cell.Style.Add(HtmlTextWriterStyle.PaddingTop, "0px");

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
        /// Construit le javascript nécessaire au bon fonctionnement des contrôles SMS
        /// </summary>
        /// <returns>Code Javascript de la page.</returns>
        private String BuildSmsScript()
        {
            String js = String.Concat(
            " var _eCurrentSelectedMailTpl = null;", Environment.NewLine,
            " var _ePopupVNEditor;", Environment.NewLine,
            " var textareaevents = {'keyup' : \"oSmsing.UpdateSmsContent\" };", Environment.NewLine,
            " var listIframe = null; ", Environment.NewLine,
            " var wizardIframe = null;", Environment.NewLine,
            " var iCurrentStep = 1;", Environment.NewLine,
            " var htmlTemplate = null;", Environment.NewLine,
            " var htmlHeader = null;", Environment.NewLine,
            " var htmlFooter = null;", Environment.NewLine,
            " var iTotalSteps;", Environment.NewLine,
            "function OnSMSDocLoad()", Environment.NewLine,
            "{", Environment.NewLine,
            "     iTotalSteps = 1;", Environment.NewLine,
            "     Init('smsmailing');", Environment.NewLine,
            "     oSmsing.SetCampaignId(@nCampId);", Environment.NewLine,
            "     oSmsing.SetParentFileId(@nPrtFileId);", Environment.NewLine,
            "     oSmsing.SetParentTabId(@nPrtTab);", Environment.NewLine,
            "     oSmsing.SetParentBkmId(@nPrtBkm);", Environment.NewLine,
            "}; OnSMSDocLoad(); ");

            js = js.Replace("@nCampId", "0");
            js = js.Replace("@nPrtBkm", _nTabFrom == 0 ? _tab.ToString() : _nTabFrom.ToString());
            js = js.Replace("@nPrtFileId", _nFileId.ToString());
            js = js.Replace("@nPrtTab", _nTabFrom == 0 ? _tab.ToString() : _nTabFrom.ToString());

            return js;
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
        /// Rendu du statut du sms
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected override Boolean RenderStatusHeader(System.Web.UI.WebControls.Table tab)
        {
            //ALISTER => Demande #79 357, On sort de la fonction si le sms n'est pas envoyé ou est en création
            if (_nFileId == 0)
                return false;

            if (_fldDate.DisplayValue.Length == 0)
                return false;

            TableRow trStatusSent = new TableRow();
            TableCell tcStatus = new TableCell();

            HtmlGenericControl label = new HtmlGenericControl("span");

            if (_fldStatus != null)
            {

                if (_fldStatus.DisplayValue.Length > 0)
                {

                    if (_fldDate != null)
                    {
                        //TODO voir si on affiche les autre statuts - personaliser le libellé -
                        if (_fldDate.DisplayValue.Length > 0)
                        {
                            //ALISTER => Demande #79 357, On met les informations directement dans le tcStatus
                            label.InnerHtml = String.Concat(eResApp.GetRes(Pref, 659), " ", _fldDate.DisplayValue);
                            tcStatus.Attributes.Add("class", "mail-head-text-sent");
                            tcStatus.Controls.Add(label);
                            trStatusSent.Controls.Add(tcStatus);
                            tab.Controls.Add(trStatusSent);
                        };
                    }
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Rendu de l'expediteur
        /// </summary>
        /// <param name="tr">table html</param>
        /// <returns></returns>
        protected override Boolean RenderFldFrom(System.Web.UI.WebControls.TableRow tr)
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
        /// Rendu du status du mail
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected Boolean RenderFldStatus(System.Web.UI.WebControls.Table tab)
        {

            if (_fldStatus != null && _fldStatus.RightIsVisible && this._nFileId != 0)
            {

                List<TableCell> cells = RenderField(_fldStatus, "smsStatus");
                string cellIdPrefix = "smsStatus_";

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
                // pour les SMS l'option est cachée car cela évite de faire de grosses modifications à l'engine.
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
        /// <param name="tab">Tableau HTML sur lequel ajouter le champ</param>
        /// <returns></returns>
        protected override bool RenderFldBody(System.Web.UI.WebControls.Table tab, bool bCkeditor = false)
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

            myLabelCell.Attributes.Add("class", "table_labels_top");
            //myLabelCell.Controls.Add(yourSMS);

            //'Choisir un modèle' ne s'affiche pas en mode consultation SMS
            if (_fldBody.RightIsUpdatable)
            {
                HtmlGenericControl spanButtonSelectModel = new HtmlGenericControl("span");
                spanButtonSelectModel.InnerHtml = eResApp.GetRes(Pref, 3048); // Select a template
                spanButtonSelectModel.Attributes.Add("class", "buttonSelectModel");
                spanButtonSelectModel.Attributes.Add("id", "smsingShowTplLstBtn");
                spanButtonSelectModel.Attributes.Add("onclick", "oSmsing.ShowTemplateList()");
                myLabelCell.Controls.Add(spanButtonSelectModel);
            }


            myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, _fldBody, 0, Pref);
            myValueCell.Style.Add(HtmlTextWriterStyle.Width, "600px");
            myValueCell.Style.Add(HtmlTextWriterStyle.Height, "120px");

            // Une fois le champs memo est chargé, on injecte la css
            //'instanceReady' : 'editor.injectCSS(oFormular.GetParam(\"bodycss\"))',

            myValueCell.Attributes.Add("listeners", "textareaevents");

            //setEventListener(me._oMemoEditor.textEditor, "keyup", me.UpdateCharCount);

            myTr.Cells.Add(myLabelCell);
            tr.Cells.Add(myValueCell);

            tab.Controls.Add(myTr);
            tab.Controls.Add(tr);

            return true;

        }


        /// <summary>
        /// Champ entête sms
        /// </summary>
        /// <param name="sortedFields"></param>
        /// <param name="systemFields"></param>
        /// <returns></returns>
        protected override List<eFieldRecord> GetHeaderFields(List<eFieldRecord> sortedFields, List<eFieldRecord> systemFields)
        {

            String[] aHeaderFields = eConst.SMS_HEADER_FIELDS.Split(';');
            List<String> lHeaderFields = new List<string>();
            lHeaderFields.AddRange(aHeaderFields);
            List<eFieldRecord> headerFields = new List<eFieldRecord>();

            // Séparation champs système / champs utilisateur
            foreach (eFieldRecord fld in sortedFields)
            {
                if (
                        !headerFields.Exists(f => f.FldInfo.Descid == fld.FldInfo.Descid)
                    && fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId
                    && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())
                  )
                {
                    headerFields.Add(fld);
                }
            }

            foreach (eFieldRecord fld in systemFields)
            {
                if (!headerFields.Exists(f => f.FldInfo.Descid == fld.FldInfo.Descid)
                         && fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId
                      && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString()))
                {
                    headerFields.Add(fld);
                }
            }

            return headerFields;
        }

        /// <summary>
        /// Créée un label
        /// </summary>
        protected override HtmlGenericControl CreateHeaderLabel()
        {

            //ALISTER => Demande #79 357, On récupère les informations nécessaires pour la date d'envoi, etc...
            RetrieveFields(base.headerFields);
            HtmlGenericControl label = new HtmlGenericControl("div");
            if (_nFileId > 0)
                if (_fldDate.DisplayValue.Length > 0)
                {
                    //Lorsque le message est envoyé
                    label.InnerText = "";
                }
                else
                {
                    //Lors de la modification
                    label.InnerText = eResApp.GetRes(Pref.LangId, 8754); //label.InnerText = eResApp.GetRes(Pref.LangId, 6857);
                    label.Attributes.Add("class", "mail-head-text");
                }
            else
            {
                //Lors de la création
                label.InnerText = eResApp.GetRes(Pref.LangId, 8754); //label.InnerText = eResApp.GetRes(Pref.LangId, 6857);
                label.Attributes.Add("class", "mail-head-text");
            }

            return label;

        }

        /// <summary>
        /// Rendu de format de text
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldIsHTML(System.Web.UI.WebControls.TableRow tr)
        {
            // Les SMS, c'est du texte :)
            return false;
        }

        /// <summary>
        /// Rendu de Destinataire
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected virtual bool RenderFldTo(System.Web.UI.WebControls.Table tab)
        {
            if (_fldTo != null)
            {
                if (!String.IsNullOrEmpty(_strMailTo))
                {
                    // Si la fenêtre a été ouverte depuis un num sms de l'application
                    _fldTo.Value = _strMailTo;
                    _fldTo.DisplayValue = _strMailTo;
                }
                // Si la fenêtre a été ouverte depuis un signet E-mail d'une fiche Contact (contact associé) ou autre,
                // on préremplit le champ "A :" avec son adresse
                // #59 789 : même chose pour SMS, mais avec un signet SMS et des champs de type Téléphone
                else if (File.ViewMainTable.EdnType == EdnType.FILE_SMS)
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

                // A (To)
                if (_fldTo.RightIsVisible)
                {
                    List<TableCell> cells = RenderField(_fldTo, "mailTo");

                    string cellIdPrefix = "mailTo_";

                    TableRow tr = new TableRow();
                    tab.Rows.Add(tr);
                    //AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);
                    tr.Cells.Add(cells[0]);

                    TableRow mytr = new TableRow();
                    tab.Rows.Add(mytr);
                    mytr.Cells.Add(cells[1]);

                    cells[2].Attributes.Add("class", "button-add");

                    mytr.Cells.Add(cells[2]);

                    return true;
                }
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
            //if (_rType == RENDERERTYPE.EditMailing && ((eEditMailingRenderer)this).UseCkeditor)
            //{
            // nIdx = 1;
            //}

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
                TableCell myButtonCell = new TableCell();

                AddMailAddrBtn(myButtonCell, myField);
                fieldCells.Add(myValueCell);
                fieldCells.Add(myButtonCell);

            }
            else
            {
                //tâche #2 477
                FieldsDescId.Add(myField.FldInfo.Descid.ToString());
                fieldCells.Add(GetCustomObjectTableCell(myLabel.ClientID, eTools.GetFieldValueCellId(_myFile.Record, myField, nIdx), myField.DisplayValue));
            }



            return fieldCells;
        }
    }
}