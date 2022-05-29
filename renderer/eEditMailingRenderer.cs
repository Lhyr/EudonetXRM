using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de rendu d emailing en  mode assistant
    /// </summary>
    public class eEditMailingRenderer : eMailFileRenderer
    {
        /// <summary>
        /// Objet métier de mailing
        /// </summary>
        protected eMailing _mailing;




        /// <summary>
        /// table mail ou seront stocké les sms
        /// </summary>
        protected eFieldRecord _fldRecipientTab = null;

        /// <summary>
        /// Champ Send Type . (uniquement pour la campagne : Eudonet/ECircle)
        /// </summary>
        protected eFieldRecord _fldCampaignSendType = null;


        protected Boolean _bIsExternalEmailing = false;

        /// <summary>
        /// Type d'envoi (uniquement pour la campagne)
        /// </summary>
        protected MAILINGSENDTYPE _eCampaignSendType = MAILINGSENDTYPE.EUDONET;

        protected Boolean _bReadonly = false;



        protected override bool ReadonlyRenderer
        {
            get
            {
                return _bReadonly;
            }
        }



        #region CONSTRUCTEUR


        /// <summary>
        ///  Crée un objet pour le rendu de la fiche compaign en mode assistant
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="nTab">Table de l'email</param>
        /// <param name="nFileId">Id de la fiche</param>
        /// <param name="nWidth">Largeur du renderer à créer</param>
        /// <param name="nHeight">Hauteur du renderer à créer</param>
        public eEditMailingRenderer(ePref pref, Int32 nTab, eMailing mailing, Int32 nWidth, Int32 nHeight, String strMailTo, Boolean bMailForward, Boolean bMailDraft)
            : base(pref, nTab, mailing.Id, nWidth, nHeight, strMailTo, false)
        {
            _mailing = mailing;
            _bMailForward = bMailForward;
            _bMailDraft = bMailDraft;
            _rType = RENDERERTYPE.EditMailing;

            _bIsExternalEmailing = _mailing.SendeType == MAILINGSENDTYPE.ECIRCLE;
        }

        #endregion

        internal bool UseCkeditor;

        protected override bool Init()
        {
            try
            {
                //Génération d'un objet "métier" de type file

                _myFile = eFileMailing.CreateEditMailingFile(Pref, _tab, _nFileId, _dicParams);

                _dicParams.TryGetValueConvert("globalaffect", out GlobalAffect);
                _dicParams.TryGetValueConvert("globalinvit", out GlobalInvit);


                int n;
                if (_dicParams.TryGetValueConvert<int>("useckeditor", out n))
                    UseCkeditor = n == 1;


                Int32 eFileTypeNum = 0;
                _dicParams.TryGetValueConvert("eFileType", out eFileTypeNum, -1);
                if (eFileTypeNum != -1)
                    _bReadonly = ((eConst.eFileType)eFileTypeNum) == eConst.eFileType.FILE_CONSULT;

                if (_myFile.ErrorMsg.Length > 0)
                {
                    _eException = _myFile.InnerException;
                    _sErrorMsg = String.Concat("eEditMailingRenderer.Init ", Environment.NewLine, _myFile.ErrorMsg);
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

                return true;
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat("eEditMailingRenderer.Init ", Environment.NewLine, e.Message);
                _nErrorNumber = QueryErrorType.ERROR_NUM_DEFAULT;
                _eException = e;
                return false;
            }
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

            // Sauvegarde l'id de l'input 
            if (_fldTo == fieldRow)
            {
                base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);
                this._strScriptBuilder.AppendLine().Append("var  recipientInputId = '").Append(webControl.ID).Append("';").AppendLine();

                return true;
            }

            if (fieldRow == _fldFrom)
            {
                if (fieldRow.RightIsUpdatable)
                {
                    #region De
                    Boolean itemSelected = false;


                    HtmlGenericControl selectFrom = new HtmlGenericControl("select");
                    webControl.Controls.Add(selectFrom);
                    selectFrom.ID = "sender-opt";
                    selectFrom.Attributes.Add("name", selectFrom.ID);
                    selectFrom.Attributes.Add("class", "mailFrom select-theme");
                    selectFrom.Attributes.Add("onchange",
                    String.Concat("document.getElementById('mailing_DN').value = this.options[this.selectedIndex].getAttribute('dn');RefreshSenderAlias(", _bIsExternalEmailing ? "true" : "false", ");"));

                    AddMailOption(selectFrom, _fldFrom, false, true, _mailing);

                    #endregion

                    HtmlGenericControl divSubContainer = new HtmlGenericControl("div");
                    divSubContainer.Attributes.Add("class", "mailSenderSubContainer");
                    webControl.Controls.Add(divSubContainer);

                    #region Nom apparent pour emailing
                    if (_fldDisplayName != null)
                    {
                        Dictionary<string, string> inputDisplayNameAttr = new Dictionary<string, string>();
                        inputDisplayNameAttr.Add("onblur", "oMailing.SetParam('displayName', this.value);");
                        string inputDisplayNameValue = _fldDisplayName.FileId > 0 ? _fldDisplayName.DisplayValue : eLibTools.GetEmailSenderDisplayName(Pref, Pref.User);

                        List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, false, String.Concat(webControl.Attributes["ename"], "_DN"), null, "mailing_DN", inputDisplayNameAttr);

                        // On ajoute tous les champs générés
                        foreach (Control ctrl in displayName)
                            if (ctrl != null)
                                divSubContainer.Controls.Add(ctrl);
                    }
                    #endregion
                }
                else
                {
                    GetValueContentControl(ednWebControl, fieldRow.DisplayValue);

                    #region Nom apparent pour emailing

                    if (_fldDisplayName != null)
                    {
                        // #68 131 - on centralise désormais la génération et on utilise un champ input disabled pour ce rendu, plutôt qu'un label contenant la valeur
                        Dictionary<string, string> inputDisplayNameAttr = new Dictionary<string, string>();
                        inputDisplayNameAttr.Add("onblur", "oMailing.SetParam('displayName', this.value);");
                        string inputDisplayNameValue = _fldDisplayName.FileId > 0 ? _fldDisplayName.DisplayValue : eLibTools.GetEmailSenderDisplayName(Pref, Pref.User);

                        List<HtmlControl> displayName = GetDisplayNameControls(inputDisplayNameValue, true, String.Concat(webControl.Attributes["ename"], "_DN"), null, "mailing_DN", inputDisplayNameAttr);

                        // On ajoute tous les champs générés
                        foreach (Control ctrl in displayName)
                            if (ctrl != null)
                                webControl.Controls.Add(ctrl);
                    }

                    #endregion
                }
            }
            else // Si ce n'est pas un champ 'Email.de' on fait un rendu par défaut
                return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            return true;
        }


        /// <summary>
        /// Remplace le domaine de mail d'origine par l'alias de partenaire
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        private string DecorateWithAliasDomain(string mail)
        {
            if (_mailing.SendeType != MAILINGSENDTYPE.ECIRCLE || string.IsNullOrEmpty(_mailing.SenderDomainAlias))
                return mail;

            return eLibTools.ReplaceOriginMailDomainWithAlias(mail, _mailing.SenderDomainAlias) + " (" + mail + ")";
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
                    || fld.FldInfo.Descid == (int)CampaignField.ISHTML
                    || fld.FldInfo.Descid == (int)CampaignField.DATESENT
                    || fld.FldInfo.Descid == (int)CampaignField.STATUS
                    || fld.FldInfo.Descid == (int)CampaignField.SENDER
                    || fld.FldInfo.Descid == (int)CampaignField.MAILADDRESSDESCID
                    || fld.FldInfo.Descid == (int)CampaignField.CCRECIPIENTS
                    || fld.FldInfo.Descid == (int)CampaignField.DISPLAYNAME
                    || fld.FldInfo.Descid == (int)CampaignField.REPLYTO
                    || fld.FldInfo.Descid == (int)CampaignField.SUBJECT
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == (int)CampaignField.PREHEADER
                    || fld.FldInfo.Descid == (int)CampaignField.BODY
                    || fld.FldInfo.Descid == (int)CampaignField.HISTO
                    || fld.FldInfo.Descid == (int)CampaignField.SENDTYPE
                    )
                {
                    headerFields.Add(fld);
                }
            }

            foreach (eFieldRecord fld in systemFields)
            {
                if (
                     fld.FldInfo.Table.DescId == _myFile.ViewMainTable.DescId && lHeaderFields.Contains((fld.FldInfo.Descid - _myFile.ViewMainTable.DescId).ToString())
                    || fld.FldInfo.Descid == (int)CampaignField.ISHTML
                    || fld.FldInfo.Descid == (int)CampaignField.SENDER
                    || fld.FldInfo.Descid == (int)CampaignField.DATESENT
                    || fld.FldInfo.Descid == (int)CampaignField.STATUS
                    || fld.FldInfo.Descid == (int)CampaignField.MAILADDRESSDESCID
                    || fld.FldInfo.Descid == (int)CampaignField.CCRECIPIENTS
                    || fld.FldInfo.Descid == (int)CampaignField.DISPLAYNAME
                    || fld.FldInfo.Descid == (int)CampaignField.REPLYTO
                    || fld.FldInfo.Descid == (int)CampaignField.SUBJECT
                    //SHA: tâche #1 939
                    || fld.FldInfo.Descid == (int)CampaignField.PREHEADER
                    || fld.FldInfo.Descid == (int)CampaignField.BODY
                    || fld.FldInfo.Descid == (int)CampaignField.HISTO
                    || fld.FldInfo.Descid == (int)CampaignField.SENDTYPE
                    )
                    headerFields.Add(fld);
            }

            return headerFields;
        }

        /// <summary>
        /// Recupere les champs de la fiche Campaign et les stocke dans des variables de class
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
                    case CampaignField.DATESENT:
                        _fldDate = fld;
                        break;
                    case CampaignField.ISHTML:
                        _fldIsHTML = fld;
                        break;
                    case CampaignField.SENDER:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldFrom = fld;
                        break;
                    case CampaignField.MAILADDRESSDESCID:
                        fld.RightIsVisible = true;
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldTo = fld;
                        break;
                    case CampaignField.CCRECIPIENTS:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldCc = fld;
                        break;
                    case CampaignField.BCCRECIPIENTS:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldBcc = fld;
                        break;
                    case CampaignField.DISPLAYNAME:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldDisplayName = fld;
                        break;
                    case CampaignField.REPLYTO:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldReplyTo = fld;
                        break;
                    case CampaignField.SUBJECT:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldSubject = fld;
                        break;
                    //SHA: tâche #1 939
                    case CampaignField.PREHEADER:
                        fld.FldInfo.Format = FieldFormat.TYP_CHAR;
                        _fldPreheader = fld;
                        break;
                    case CampaignField.BODY:
                        _fldBody = fld;
                        _fldBody.FldInfo.Format = FieldFormat.TYP_MEMO;
                        break;
                    case CampaignField.HISTO:
                        _fldHisto = fld;
                        break;
                    case CampaignField.SENDTYPE:
                        _fldCampaignSendType = fld;
                        _fldCampaignSendType.FldInfo.Format = FieldFormat.TYP_NUMERIC;
                        break;
                }
            }
        }

        /// <summary>
        /// Ajout des fonctions ou des variables js (champs de fusion, liens tracking ..)
        /// </summary>
        /// <param name="strScriptBuilder"></param>
        protected override void AppendScript(StringBuilder strScriptBuilder)
        {
            if (!ReadonlyRenderer)
            {
                //KJE, tâche #2 431: on crée la liste des champs de fusion pour l'objet du mail sauf les champs de type Mémo, Images, graphiques, Page web
                strScriptBuilder.Append(GetMergeAndTrackFields(this.Pref, this._nTabFrom, bGetOnlyTxtMergedField: true)).Append("; ");
                strScriptBuilder.Append(" var mailObjectMergeFields = mailMergeFields; ");
                strScriptBuilder.Append(GetMergeAndTrackFields(this.Pref, this._nTabFrom));
                if (_fldTo != null)
                    strScriptBuilder.AppendLine().Append("var  recipientInputId = '").Append(_fldTo.FldInfo.Alias).Append("';").AppendLine();
            }
        }

        /// <summary>
        /// Retourne les champs de fusion et les champs de tracking
        /// </summary>
        /// <param name="pref">Preference utilisateur</param>
        /// <param name="nTabFrom"> table d'ou on vient</param>
        /// <param name="bGetOnlyTxtMergedField"> charger uniquement les champs de fusion pour l'objet du mail</param>
        /// <returns> variables javascript champs de fusion et track fields</returns>
        public static String GetMergeAndTrackFields(ePref pref, int nTabFrom, bool bGetOnlyTxtMergedField = false)
        {
            StringBuilder strScriptBuilder = new StringBuilder();

            #region Champs de fusion
            string strJavaScript = String.Empty;
            string strWebsiteFieldsJavaScript = String.Empty;
            string error = String.Empty;

            eTableLiteMailing table;
            IEnumerable<eFieldLiteWithLib> fields = null;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            try
            {
                dal.OpenDatabase();

                if (nTabFrom == 0)
                    throw new Exception("Invalide nTabFrom = " + nTabFrom);

                table = new eTableLiteMailing(nTabFrom, pref.Lang);
                table.ExternalLoadInfo(dal, out error);
                if (error.Length > 0)
                    throw new Exception(error);

                //Tous les champs de fusion
                List<int> AllMergeFields = eLibTools.GetMergeFieldsMailingList(dal, pref, table, table.ProspectEnabled, bGetOnlyTxtMergedField: bGetOnlyTxtMergedField);

                //Tous les champs de fusion de type site web
                List<int> AllWebsiteMergeFields = eLibTools.GetSpecificMergeFieldsMailingList(dal, pref, table, table.ProspectEnabled, false, false, FieldFormat.TYP_WEB);

                //On filtre la  liste par rapport aux droits de visu
                List<int> AllowedMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllMergeFields.ToArray()), false).Keys);

                //On filtre la liste des champs de type site web
                List<int> WebsiteMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(pref, pref.User, String.Join(";", AllWebsiteMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, AllowedMergeFields, null, null, null, null, null, null, out strJavaScript);

                //on construit la liste des champs de type site web
                eLibTools.GetMergeFieldsData(dal, pref, pref.User, WebsiteMergeFields, null, null, null, null, null, null, out strWebsiteFieldsJavaScript);

                fields = Internal.RetrieveFields.GetEmpty(pref)
                    .AddOnlyThisTabs(new int[] { nTabFrom })
                    .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_BIT, FieldFormat.TYP_NUMERIC })
                    .AddExcludeSystemFields(true)
                    .SetExternalDal(dal)
                    .ResultFieldsInfo(eFieldLiteWithLib.Factory(pref));
            }
            catch (Exception ex)
            {
                throw new Exception("eEditMailingRenderer::GetMergeAndTrackFields:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            strScriptBuilder.Append(" var mailMergeFields = ").Append(String.IsNullOrEmpty(strJavaScript) ? "{}" : strJavaScript).Append(";").AppendLine();

            #endregion

            #region Champs tracking
            //pour l'objet de mail, pas besoin de charger la liste des champs tracking
            if (!bGetOnlyTxtMergedField)
            {
                strScriptBuilder.AppendLine()
                .Append(" var oTrackFields = { link :{href:'', ednc:'lnk', ednt:'on', ednd:'0', ednn:'', ednl:'0', title:'', target:'_blank'}, fields : [ ")
                .Append("['<").Append(eResApp.GetRes(pref, 141)).Append(">', '0', ''], ");     // option vide

                if (fields != null && fields.Count() > 0)
                {
                    string prefName = HttpUtility.JavaScriptStringEncode(table.Libelle);

                    //insère des options au format suivant : [libellé, id, format]
                    strScriptBuilder
                        .Append(eLibTools.Join(",", fields, delegate (eFieldLiteWithLib fld)
                        {
                            return new StringBuilder()
                                .Append("['").Append(prefName).Append(".").Append(HttpUtility.JavaScriptStringEncode(fld.Libelle))
                                .Append("', '").Append(fld.Descid).Append("', '").Append(fld.Format.GetHashCode()).Append("']").ToString();
                        }));
                }

                strScriptBuilder.Append(" ]}; ");
            }
            #endregion

            #region Champs de fusion site web
            if (!bGetOnlyTxtMergedField)
            {
                strScriptBuilder.AppendLine()
                .Append(" var oMergeHyperLinkFields = { link :{href:'', ednc:'lnk', ednt:'on', ednd:'0', ednn:'', ednl:'0', title:'', target:'_blank'}, fields : [ ")
                .Append("['<").Append(eResApp.GetRes(pref, 141)).Append(">', '0', '']");     // option vide

                if (String.IsNullOrEmpty(strWebsiteFieldsJavaScript))
                    strScriptBuilder.Append(", []");
                else
                {
                    string[] websiteFields = strWebsiteFieldsJavaScript.Split(',');
                    foreach (String wbF in websiteFields)
                    {
                        string[] strlist1 = wbF.Split(':');
                        string fldLibelle = Regex.Replace(strlist1[0], "(\\r|\\n|\\|\u0022|{)*", String.Empty);
                        string[] strlist2 = strlist1[1].Split(';');
                        string fldDescId = String.Concat("'", strlist2[0].Replace("\"", String.Empty), "'").Replace(" ", String.Empty);
                        string fldFormat = String.Concat("'", strlist2[3], "'").Replace(" ", String.Empty);
                        string wsField = String.Concat(", [", HttpUtility.HtmlDecode(fldLibelle), ",", fldDescId, ",", fldFormat, "]");
                        strScriptBuilder.Append(wsField);
                    }
                }

                strScriptBuilder.Append(" ]}; ");
            }

            #endregion

            return strScriptBuilder.ToString();
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
            TableCell tcStatusLabel = new TableCell();
            trStatusSent.Controls.Add(tcStatusLabel);
            TableCell tcStatus = new TableCell();

            if (_fldStatus != null)
            {
                //TODO: déterminer statut brouillon/envoyé/non envoyé selon info stockée en base
                if (_fldStatus.DisplayValue.Length > 0)
                {
                    tcStatus.Text = _fldStatus.DisplayValue;
                    tcStatus.CssClass = "mail-head-text-sent";
                    trStatusSent.Controls.Add(tcStatus);
                    tab.Controls.Add(trStatusSent);

                    if (_fldDate != null)
                    {
                        //TODO voir si on affiche les autre statuts - personaliser le libellé -
                        if (_fldDate.DisplayValue.Length > 0)
                        {
                            tcStatus.Text = String.Concat(tcStatus.Text, " - ", _fldDate.DisplayValue);
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
        /// <param name="tr">table html</param>
        /// <returns></returns>
        protected override Boolean RenderFldFrom(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {  // Expéditeur (From - De), Nom apparent si Email
                if (_fldFrom != null && _fldFrom.RightIsVisible)
                {
                    // De + Nom Apparent (sur la même ligne)
                    List<TableCell> cells = RenderField(_fldFrom, "mailFrom");

                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, "mailFrom_");

                    if ((_fldFrom.RightIsUpdatable && _bIsExternalEmailing) || (!_fldFrom.RightIsUpdatable && _fldCampaignSendType != null && _fldCampaignSendType.Value == ((int)MAILINGSENDTYPE.ECIRCLE).ToString()))
                    {
                        // Nom de domaine partinaire + email apparent (sur la même ligne)
                        TableRow trSenderAliasDomain = new TableRow();
                        List<TableCell> cellsSenderAliasDomain = RenderFldSenderAliasDomain("mailSenderAliasDomain");
                        trSenderAliasDomain.ID = "mailAliasDomain";

                        // TODO MAB - Dégueulasse, comme dirait HLA. A refacto après réception de la maquette de QBO
                        ((System.Web.UI.WebControls.Table)tr.Parent).Controls.Add(trSenderAliasDomain);
                    }
                    return true;
                }
            }
            return false;
        }



        private void AddSenderAliasDomain(HtmlGenericControl webControl)
        {

            string senderAliasDomainValue = _mailing.GetParamValue("senderAliasDomain");


            #region Domaine
            HtmlGenericControl labelinputAlias = new HtmlGenericControl("label");
            labelinputAlias.Attributes.Add("Id", string.Concat(eTools.GetFieldValueCellName(_myFile.Record, _fldFrom), "_DN"));
            labelinputAlias.InnerHtml = eResApp.GetRes(Pref, 7915);//nom de domaine
            webControl.Controls.Add(labelinputAlias);

            HtmlGenericControl selectDomain = new HtmlGenericControl("select");
            webControl.Controls.Add(selectDomain);
            selectDomain.ID = "domain-opt";
            selectDomain.Attributes.Add("name", selectDomain.ID);
            selectDomain.Attributes.Add("class", "mailSenderAliasDomain select-theme");
            selectDomain.Attributes.Add("onchange", String.Concat("oMailing.SetParam('senderAliasDomain', this.value);RefreshSenderAlias(", _bIsExternalEmailing ? "true" : "false", ");"));

            Dictionary<string, string> dicoDomains = eLibTools.GetConfigAdvPrefAdvValuesFromPrefix(Pref, eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString());
            HashSet<string> distinctDomains = new HashSet<string>();

            Boolean itemSelected = false;
            foreach (KeyValuePair<string, string> domain in dicoDomains)
            {
                if (!distinctDomains.Contains(domain.Value))
                {
                    distinctDomains.Add(domain.Value);

                    //TODO REFACTORISATION
                    HtmlGenericControl optionDomain = new HtmlGenericControl("option");
                    optionDomain.Attributes.Add("value", domain.Value);
                    optionDomain.InnerText = domain.Value;

                    if (senderAliasDomainValue == domain.Value && !itemSelected)
                    {
                        optionDomain.Attributes.Add("selected", "selected");
                        itemSelected = true;
                    }
                    selectDomain.Controls.Add(optionDomain);
                }
            }
            #endregion

  

            #region De apparent
            //Création du libellé
     


            HtmlGenericControl labelDisplayName = new HtmlGenericControl("label");
            labelDisplayName.Attributes.Add("Id", String.Concat(webControl.Attributes["ename"], "_SenderAlias"));
            labelDisplayName.InnerHtml = eResApp.GetRes(Pref, 8217);//Alias
            webControl.Controls.Add(labelDisplayName);

            //Génération du rendu HTML pour le champ
            string FromValueTemp = !String.IsNullOrEmpty(_fldFrom.Value) ? _fldFrom.Value : Pref.User.UserMail;
            string SenderAliasDomainTemp = itemSelected ? senderAliasDomainValue : (dicoDomains.Count > 0 ? dicoDomains.First().Value : String.Empty);
            HtmlInputText inputDisplayName = new HtmlInputText();
            webControl.Controls.Add(inputDisplayName);
            inputDisplayName.Value = eLibTools.ReplaceOriginMailDomainWithAlias(FromValueTemp, SenderAliasDomainTemp);
            inputDisplayName.Attributes.Add("class", "readonly mailSenderAlias");
            inputDisplayName.ID = "mailing_SenderAlias";
            //inputDisplayName.Attributes.Add("onblur", "oMailing.SetParam('displayName', this.value);");
            inputDisplayName.Attributes.Add("disabled", "disabled");
 
            #endregion



        }

        /// <summary>
        /// Rendu de l'expediteur
        /// </summary>
        /// <param name="customCssClass">Classe CSS custom à utiliser</param>
        /// <returns></returns>
        private List<TableCell> RenderFldSenderAliasDomain(string customCssClass)
        {
            //Création de la cellule
            TableCell myLabel = new TableCell();

            //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
            //this.GetFieldLabelCell(myLabel, _myFile.Record, myField);            
            myLabel.Attributes.Add("class", "table_labels");
            myLabel.Text = eResApp.GetRes(Pref, 7915); //Domaine

            //Génération du rendu HTML pour chaque champ à afficher
            //C'est dans cette méthode du eRenderer générique que sont gérées les exceptions visuelles à appliquer sur certains champs d'E-mail
            //ex : combobox pour le champ De (From)
            //TableCell myValueCell = (TableCell)GetFieldValueCell(_myFile.Record, myField, 0, Pref);
            TableCell myValueCell = new TableCell();

            myValueCell.RowSpan = 1;
            myValueCell.ColumnSpan = 1;
            myValueCell.CssClass = String.Concat(myValueCell.CssClass, " table_values ", customCssClass);

            //KHA ajout d'un bouton permettant l'ajout de mails de contacts.
            //AddMailAddrBtn(myValueCell, myField);

            EdnWebControl ednWebControl = new EdnWebControl();
            ednWebControl.WebCtrl = myValueCell;
            ednWebControl.TypCtrl = EdnWebControl.WebControlType.TABLE_CELL;
            WebControl webControl = ednWebControl.WebCtrl;

            string senderAliasDomainValue = _mailing.GetParamValue("senderAliasDomain");

            if (_fldFrom.RightIsUpdatable)
            {
                #region Domaine
                HtmlGenericControl selectDomain = new HtmlGenericControl("select");
                webControl.Controls.Add(selectDomain);
                selectDomain.ID = "domain-opt";
                selectDomain.Attributes.Add("name", selectDomain.ID);
                selectDomain.Attributes.Add("class", "mailSenderAliasDomain select-theme");
                selectDomain.Attributes.Add("onchange", String.Concat("oMailing.SetParam('senderAliasDomain', this.value);RefreshSenderAlias(", _bIsExternalEmailing ? "true" : "false", ");"));

                Dictionary<string, string> dicoDomains = eLibTools.GetConfigAdvPrefAdvValuesFromPrefix(Pref, eLibConst.CONFIGADV.EUDOMAILING_SENDER_ALIAS_DOMAIN.ToString());
                HashSet<string> distinctDomains = new HashSet<string>();

                Boolean itemSelected = false;
                foreach (KeyValuePair<string, string> domain in dicoDomains)
                {
                    if (!distinctDomains.Contains(domain.Value))
                    {
                        distinctDomains.Add(domain.Value);

                        //TODO REFACTORISATION
                        HtmlGenericControl optionDomain = new HtmlGenericControl("option");
                        optionDomain.Attributes.Add("value", domain.Value);
                        optionDomain.InnerText = domain.Value;

                        if (senderAliasDomainValue == domain.Value && !itemSelected)
                        {
                            optionDomain.Attributes.Add("selected", "selected");
                            itemSelected = true;
                        }
                        selectDomain.Controls.Add(optionDomain);
                    }
                }
                #endregion

                HtmlGenericControl divSubContainer = new HtmlGenericControl("div");
                divSubContainer.Attributes.Add("class", "mailSenderSubContainer");
                webControl.Controls.Add(divSubContainer);

                #region De apparent
                //Création du libellé
                HtmlGenericControl labelDisplayName = new HtmlGenericControl("span");
                labelDisplayName.InnerText = eResApp.GetRes(Pref, 8217);
                labelDisplayName.ID = String.Concat(webControl.Attributes["ename"], "_SenderAlias");
                labelDisplayName.Attributes.Add("class", "table_labels spanYAdjust");

                //Génération du rendu HTML pour le champ
                string FromValueTemp = !String.IsNullOrEmpty(_fldFrom.Value) ? _fldFrom.Value : Pref.User.UserMail;
                string SenderAliasDomainTemp = itemSelected ? senderAliasDomainValue : (dicoDomains.Count > 0 ? dicoDomains.First().Value : String.Empty);
                HtmlInputText inputDisplayName = new HtmlInputText();
                inputDisplayName.Value = eLibTools.ReplaceOriginMailDomainWithAlias(FromValueTemp, SenderAliasDomainTemp);
                inputDisplayName.Attributes.Add("class", "readonly mailSenderAlias");
                inputDisplayName.ID = "mailing_SenderAlias";
                //inputDisplayName.Attributes.Add("onblur", "oMailing.SetParam('displayName', this.value);");
                inputDisplayName.Attributes.Add("disabled", "disabled");

                divSubContainer.Controls.Add(labelDisplayName);
                divSubContainer.Controls.Add(inputDisplayName);
                #endregion
            }
            else
            {
                GetValueContentControl(ednWebControl, senderAliasDomainValue);

                #region De apparent
                //Création du libellé
                HtmlGenericControl labelDisplayName = new HtmlGenericControl("span");
                labelDisplayName.InnerText = eResApp.GetRes(Pref, 8217);
                labelDisplayName.ID = String.Concat(webControl.Attributes["ename"], "_SenderAlias");
                labelDisplayName.Attributes.Add("class", "table_labels spanYAdjust");

                //Génération du rendu HTML pour le champ                
                HtmlGenericControl inputDisplayName = new HtmlGenericControl("label");
                inputDisplayName.InnerHtml = eLibTools.ReplaceOriginMailDomainWithAlias(_fldFrom.Value, senderAliasDomainValue);
                inputDisplayName.Attributes.Add("class", "readonly mailSenderAlias");
                inputDisplayName.ID = "mailing_SenderAlias";
                //inputDisplayName.Attributes.Add("onblur", "oMailing.SetParam('displayName', this.value);");
                inputDisplayName.Attributes.Add("disabled", "disabled");

                webControl.Controls.Add(labelDisplayName);
                webControl.Controls.Add(inputDisplayName);
                #endregion
            }

            return new List<TableCell>() { myLabel, myValueCell };
        }


        /// <summary>
        /// Génère le champ "Répondre à"
        /// A appeler A LA PLACE de eRenderer.GetTableCell
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldReplyTo(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {
                if (_fldReplyTo != null && _fldReplyTo.RightIsVisible)
                {
                    // Répondre à
                    List<TableCell> cells = RenderField(_fldReplyTo, "mailReplyTo");

                    string cellIdPrefix = "mail_fldReplyTo_";
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Une tableCell si c'est un replyTo
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        protected override EdnWebControl CreateEditEdnControl(eFieldRecord fieldRow)
        {
            if (fieldRow == _fldReplyTo)
                return new EdnWebControl() { WebCtrl = new TableCell(), TypCtrl = EdnWebControl.WebControlType.TABLE_CELL };

            return new EdnWebControl() { WebCtrl = new TextBox(), TypCtrl = EdnWebControl.WebControlType.TEXTBOX };
        }

        /// <summary>
        /// Champ CC
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldCc(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {
                if (_fldCc != null && _fldCc.RightIsVisible)
                {
                    Int32 nSelectedCCDescId = eLibTools.GetNum(_mailing.GetParamValue("ccrecipient"));
                    if (ReadonlyRenderer && nSelectedCCDescId == 0)
                        return true;

                    //Cas spécifique E-mail
                    //Certains champs spéciaux ont un type et une taille déclaré(e)s en base qui n'est pas ce qui est souhaité en affichage (Corps de mail, De, A, Cc, Bcc, Objet)
                    //Pour ces champs-là, on change la taille d'affichage et le format
                    int nFieldDescId;
                    int nFieldGenericDescId;

                    nFieldDescId = _fldCc.FldInfo.Descid;
                    nFieldGenericDescId = nFieldDescId % 100;
                    _fldCc.FldInfo.PosRowSpan = 1;

                    //Création de la cellule
                    TableCell myLabel = new TableCell();
                    TableCell myValue = new TableCell();

                    //Appel à GetFieldLabelCell du Renderer pour renseigner plusieurs attributs
                    this.GetFieldLabelCell(myLabel, _myFile.Record, _fldCc);
                    //Modification des attributs de base

                    myLabel.Text = _fldCc.FldInfo.Libelle;
                    myLabel.ID = eTools.GetFieldValueCellName(_myFile.Record, _fldCc);
                    myLabel.Attributes.Add("did", nFieldDescId.ToString());
                    myLabel.Attributes.Add("lib", myLabel.Text);

                    //Pour le js  afin de faire les vérifs
                    myLabel.Attributes.Add("fmt", FieldFormat.TYP_EMAIL.GetHashCode().ToString());

                    //Génération du rendu HTML pour chaque champ à afficher
                    //C'est dans cette méthode du eRenderer générique que sont gérées les exceptions visuelles à appliquer sur certains champs d'E-mail
                    //ex : combobox pour le champ De (From)
                    TableCell myValueCell = new TableCell();
                    EdnWebControl ednMyValueCell = new EdnWebControl() { WebCtrl = myValueCell, TypCtrl = EdnWebControl.WebControlType.TABLE_CELL };

                    //On ajoute une liste qui sera rempli dynamique coté client, en fonction d'adresse mail sélectionnée
                    DropDownList dropList = new DropDownList();
                    dropList.ID = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_optmail_cc");
                    dropList.Attributes.Add("class", "mailCc");

                    dropList.Attributes.Add("onchange", "oMailing.SetParam('ccrecipient', this.options[this.selectedIndex].value);");

                    if (_mailing.Id > 0)
                    {

                        //fill droplist
                        Int32 mailFieldDescId = eLibTools.GetNum(_mailing.GetParamValue("mailFieldDescId"));
                        Int32 nTab = eLibTools.GetTabFromDescId(mailFieldDescId);

                        if (nTab > 0)
                        {
                            //option de ne pas choisir .
                            dropList.Items.Add(new ListItem("", "0"));

                            foreach (KeyValuePair<int, string> kv in _mailing.EmailFields)
                            {
                                if (eLibTools.GetTabFromDescId(kv.Key) == nTab && kv.Key != mailFieldDescId)
                                    dropList.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));
                            }
                        }

                        //on preselection la valeur choisie
                        if (_mailing.EmailFields.Count > 0)
                            if (nSelectedCCDescId > 0)
                                dropList.SelectedValue = nSelectedCCDescId.ToString();
                            else
                                dropList.SelectedValue = "0";
                    }

                    if (ReadonlyRenderer)
                        //myValueCell = this.GetFieldValueCell(_myFile.Record, _fldCc, 0, Pref);
                        GetValueContentControl(ednMyValueCell, _fldCc.DisplayValue);  //En mode lecture seule
                    else
                        myValueCell.Controls.Add(dropList);

                    myValueCell.RowSpan = 1;
                    myValueCell.ColumnSpan = 1;
                    myValueCell.CssClass = String.Concat(myValueCell.CssClass, " table_values ", "mailCc");


                    myLabel.Attributes.Add("class", "table_labels");

                    string cellIdPrefix = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_tr_mail_cc_");
                    myLabel.ID = String.Concat(cellIdPrefix, 0);
                    myValueCell.ID = String.Concat(cellIdPrefix, 1);

                    tr.Cells.Add(myLabel);
                    tr.Cells.Add(myValueCell);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Champ BCC
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldBcc(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {
                if (_fldCc != null && _fldCc.RightIsVisible)
                {
                    base.RenderFldBcc(tr);
                }
            }

            return false;
        }
        /// <summary>
        /// Rendu de Destinataire
        /// </summary>
        /// <param name="tr">table html</param>
        /// <returns></returns>
        protected override Boolean RenderFldTo(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {
                // A (To)
                if (_fldTo != null && _fldTo.RightIsVisible)
                {
                    List<TableCell> cells = RenderField(_fldTo, "mailTo");

                    //Label nombre destinataires
                    HtmlGenericControl NbDestinataires = new HtmlGenericControl("label");
                    NbDestinataires.ID = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_recipient_count");
                    NbDestinataires.Attributes.Add("class", "mailing_recipient_count");

                    NbDestinataires.InnerHtml = String.Empty;
                    if (cells.Count > 0)
                    {
                        cells[cells.Count - 1].Controls.Add(NbDestinataires);
                        cells[cells.Count - 1].CssClass = String.Concat(cells[cells.Count - 1].CssClass, " ", " mailTo_with_recipient_count");
                    }

                    string cellIdPrefix = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_mail_to_");
                    AddCellsToTableAndFillIDsIfNotEmpty(tr, cells, cellIdPrefix);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// ajout d'un bouton permettant l'ajout de mails de contacts. non effectif dan sle cas de l'emailing
        /// </summary>
        /// <param name="myValueCell">Cellule de tableau dans laquelle le bouton doit être inséré</param>
        /// <param name="myField">Champ correspondant à la valeur à saisir (To, CC, BCC)</param>
        protected override void AddMailAddrBtn(TableCell myValueCell, eFieldRecord myField)
        {
            return;
        }

        /// <summary>
        /// Rendu du corps de mail avec spécificités de l'assistant d'e-mailing
        /// </summary>
        /// <param name="tab">table html</param>
        /// <returns></returns>
        protected override bool RenderFldBody(System.Web.UI.WebControls.Table tab, bool bCkEditor = false)
        {
            bool baseRenderingDone = base.RenderFldBody(tab, bCkEditor);

            if (baseRenderingDone && tab.Rows.Count > 0 && tab.Rows[tab.Rows.Count - 1].Cells.Count > 0)
            {
                TableCell myValueCell = tab.Rows[tab.Rows.Count - 1].Cells[0];

                // Taille du champ Mémo ajustée après génération de la cellule
                // Calcul : taille de la fenêtre - espace occupé par les autres éléments

                int nBodyHeight;
                if (ReadonlyRenderer)
                    nBodyHeight = _nHeight - 110; //345; //mode popup lecture seule
                else
                    nBodyHeight = _nHeight - 160; //395; //mode assistant

                // Si la taille est trop réduite, on impose une taille minimale
                if (nBodyHeight < 100)
                    nBodyHeight = 100;
                // Ajout
                if (nBodyHeight > 0)
                    myValueCell.Style.Add(HtmlTextWriterStyle.Height, String.Concat(nBodyHeight, "px"));
            }

            return baseRenderingDone;
        }


        /// <summary>
        /// Rendu du champ Historique pour l'emailing
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override Boolean RenderFldHistoTrack(System.Web.UI.WebControls.TableRow tr)
        {
            //if (_fldHisto == null)
            //    return false;

            //// Historisé 
            //// Ces deux champs devant être affichés côte-à-côte, on utilise les méthodes usuelles pour générer le rendu, puis on récupère les contrôles case à cocher
            //// pour les ajouter dans une ligne séparée
            //if (_fldHisto.RightIsVisible )
            //{
            //    List<TableCell> trHisto = RenderField(_fldHisto, String.Empty);
            //    List<TableCell> trTracking = new List<TableCell>();
            //    if (_fldTracking != null)
            //        trTracking = RenderField(_fldTracking, String.Empty);

            //    TableCell tcHistoTrackingValue = new TableCell();
            //    tcHistoTrackingValue.ColumnSpan = 2;

            //    //Création du conteneur de contrôle (équivalent de la cellule de tableau attendue par l'Engine)
            //    HtmlGenericControl valueHisto = new HtmlGenericControl("span");
            //    valueHisto.ID = trHisto[trHisto.Count - 1].ID;
            //    valueHisto.Attributes.Add("class", trHisto[trHisto.Count - 1].CssClass);
            //    valueHisto.Style.Add(HtmlTextWriterStyle.MarginLeft, "2px");

            //    foreach (string attrKey in trHisto[trHisto.Count - 1].Attributes.Keys)
            //        valueHisto.Attributes.Add(attrKey, trHisto[trHisto.Count - 1].Attributes[attrKey]);

            //    foreach (Control ctrl in trHisto[trHisto.Count - 1].Controls)
            //        valueHisto.Controls.Add(ctrl);

            //    tcHistoTrackingValue.Controls.Add(valueHisto);

            //    //Création du libellé
            //    HtmlGenericControl labelHisto = new HtmlGenericControl("span");
            //    labelHisto.InnerText = trHisto[0].Text;
            //    labelHisto.ID = trHisto[0].ID;
            //    labelHisto.Attributes.Add("class", trHisto[0].CssClass);

            //    foreach (string attrKey in trHisto[0].Attributes.Keys)
            //        labelHisto.Attributes.Add(attrKey, trHisto[0].Attributes[attrKey]);

            //    tcHistoTrackingValue.Controls.Add(labelHisto);
            //    tr.Controls.Add(tcHistoTrackingValue);
            //    return false;
            //}

            return true;
        }



        /// <summary>
        /// ajoute les liaisons parentes en pied de page
        /// </summary>
        protected override void AddParentInFoot() { /*ne pas afficher les liaisons*/ }

        /// <summary>
        /// Rendu de format de text
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override bool RenderFldIsHTML(System.Web.UI.WebControls.TableRow tr)
        {
            if (_bReadonly)
            {
                // Format du mail (HTML ou texte)
                if (_fldIsHTML != null && _fldIsHTML.RightIsVisible)
                {
                    base.RenderFldIsHTML(tr);
                }
            }

            return false;
        }

        /// <summary>
        /// Sujet du mail
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override bool RenderFldSubject(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldSubject != null)
            {
                base.RenderFldSubject(tr);
            }

            return false;
        }

        /// <summary>
        /// Preheader (texte d'aperçu) du mail
        /// </summary>
        /// <param name="tr">Ligne de tableau HTML sur laquelle ajouter le champ</param>
        /// <returns></returns>
        protected override bool RenderFldPreheader(System.Web.UI.WebControls.TableRow tr)
        {
            if (_fldPreheader != null && _fldPreheader.RightIsVisible)
            {
                base.RenderFldPreheader(tr);
            }

            return false;
        }

        /// <summary>
        /// creation de information sur l'envois
        /// </summary>
        /// <param name="formCampaignInfo"> div form</param>
        /// <returns></returns>
        public HtmlGenericControl RenderFldSendinginfos(HtmlGenericControl formCampaignInfo)
        {
            // first div fro sendingInfo
            HtmlGenericControl divSendingInformation = new HtmlGenericControl("div");
            divSendingInformation.Attributes.Add("class", "block--sendingInfos");

            HtmlGenericControl labelSendingInfs = new HtmlGenericControl("h3");
            labelSendingInfs.InnerHtml = eResApp.GetRes(this.Pref, 2883);
            divSendingInformation.Controls.Add(labelSendingInfs);

            #region From
            // input From 
            HtmlGenericControl divFrom = new HtmlGenericControl("div");
            divFrom.Attributes.Add("class", "field-container");
            divFrom.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldFrom.FldInfo.Descid, "_0_0_0");
            divFrom.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldFrom)));
            divFrom.Attributes.Add("colspan", "1");
            divFrom.Attributes.Add("rowspan", "1");
            divFrom.Attributes.Add("did", _fldFrom.FldInfo.Descid.ToString());
            divSendingInformation.Controls.Add(divFrom);

            HtmlGenericControl labelinputFrom = new HtmlGenericControl("label");
            labelinputFrom.InnerHtml = eResApp.GetRes(Pref, 554);//FROM
            divFrom.Controls.Add(labelinputFrom);

            Boolean bIsExternalEmailing = _mailing.SendeType == MAILINGSENDTYPE.ECIRCLE;

            HtmlGenericControl selectFrom = new HtmlGenericControl("select");
            divFrom.Controls.Add(selectFrom);
            selectFrom.ID = "sender-opt";
            selectFrom.Attributes.Add("name", selectFrom.ID);
            selectFrom.Attributes.Add("class", "mailFrom select-theme");
            selectFrom.Attributes.Add("onchange",
            String.Concat("document.getElementById('mailing_DN').value = this.options[this.selectedIndex].getAttribute('dn');RefreshSenderAlias(", bIsExternalEmailing ? "true" : "false", ");"));

            AddMailOption(selectFrom, _fldFrom, false, true, _mailing);

            FieldsDescId.Add(_fldFrom.FldInfo.Descid.ToString());

            #region Alias
            HtmlGenericControl labelinputAlias = new HtmlGenericControl("label");
            labelinputAlias.Attributes.Add("Id", string.Concat(eTools.GetFieldValueCellName(_myFile.Record, _fldFrom), "_DN"));
            labelinputAlias.InnerHtml = eResApp.GetRes(Pref, 6308);//Alias
            divFrom.Controls.Add(labelinputAlias);


            HtmlGenericControl inputAlias = new HtmlGenericControl("input");
            inputAlias.Attributes.Add("Id", "mailing_DN");
            inputAlias.Attributes.Add("type", "text");
            divFrom.Controls.Add(inputAlias);

            Dictionary<string, string> inputDisplayNameAttr = new Dictionary<string, string>();
            inputDisplayNameAttr.Add("onblur", "oMailing.SetParam('displayName', this.value);");
            string inputDisplayNameValue = _fldDisplayName.FileId > 0 ? _fldDisplayName.DisplayValue : eLibTools.GetEmailSenderDisplayName(Pref, Pref.User);
            inputAlias.Attributes.Add("value", inputDisplayNameValue);
            FieldsDescId.Add(_fldDisplayName.FldInfo.Descid.ToString());
            #endregion
            #endregion

            #region mapp/ecircle

            //dans le cas de mapp via ecircle/mapp on propose les noms de domaines configurés
            if (bIsExternalEmailing)
            {



                AddSenderAliasDomain(divFrom);




            }
            #endregion

            #region ReplyTo
            HtmlGenericControl divReplyTo = new HtmlGenericControl("div");
            divReplyTo.Attributes.Add("class", "field-container");
            divReplyTo.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldReplyTo.FldInfo.Descid, "_0_0_0");
            divReplyTo.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldReplyTo)));
            divReplyTo.Attributes.Add("colspan", "1");
            divReplyTo.Attributes.Add("rowspan", "1");
            divReplyTo.Attributes.Add("did", _fldReplyTo.FldInfo.Descid.ToString());
            divSendingInformation.Controls.Add(divReplyTo);

            HtmlGenericControl labelinputReplyTo = new HtmlGenericControl("label");
            labelinputReplyTo.InnerHtml = eResApp.GetRes(Pref, 6309);//Alias
            divReplyTo.Controls.Add(labelinputReplyTo);

            HtmlGenericControl selectReplyTo = new HtmlGenericControl("select");
            divReplyTo.Controls.Add(selectReplyTo);
            selectReplyTo.ID = "replyto-opt";
            selectReplyTo.Attributes.Add("name", selectReplyTo.ID);
            selectReplyTo.Attributes.Add("class", "mailReply select-theme");

            AddMailOption(selectReplyTo, _fldReplyTo, false, true, _mailing);

            FieldsDescId.Add(_fldReplyTo.FldInfo.Descid.ToString());
            #endregion

            #region To
            // input To 
            HtmlGenericControl divTo = new HtmlGenericControl("div");
            divTo.Attributes.Add("class", "field-container");
            divTo.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldTo.FldInfo.Descid, "_0_0_0");
            divTo.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldTo)));
            divTo.Attributes.Add("colspan", "1");
            divTo.Attributes.Add("rowspan", "1");
            divTo.Attributes.Add("did", _fldTo.FldInfo.Descid.ToString());
            divSendingInformation.Controls.Add(divTo);

            HtmlGenericControl labelinputTo = new HtmlGenericControl("label");
            labelinputTo.InnerHtml = eResApp.GetRes(Pref, 5087);//To
            divTo.Controls.Add(labelinputTo);

            HtmlGenericControl inputTo = new HtmlGenericControl("input");
            inputTo.Attributes.Add("Id", _fldTo.FldInfo.Alias);
            inputTo.Attributes.Add("Maxlength", "20");
            inputTo.Attributes.Add("type", "text");
            inputTo.Attributes.Add("disabled", "true");
            inputTo.Attributes.Add("value", _fldTo.DisplayValue);
            divTo.Controls.Add(inputTo);

            // FieldsDescId.Add(_fldTo.FldInfo.Descid.ToString());
            #endregion

            #region Cc
            HtmlGenericControl divCc = new HtmlGenericControl("div");
            divCc.Attributes.Add("class", "field-container");
            divCc.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldCc.FldInfo.Descid, "_0_0_0");
            divCc.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldCc)));
            divCc.Attributes.Add("colspan", "1");
            divCc.Attributes.Add("rowspan", "1");
            divCc.Attributes.Add("did", _fldCc.FldInfo.Descid.ToString());
            divSendingInformation.Controls.Add(divCc);

            HtmlGenericControl labelinputCc = new HtmlGenericControl("label");
            labelinputCc.InnerHtml = eResApp.GetRes(Pref, 390);//CC                                                              
            labelinputCc.Attributes.Add("fmt", FieldFormat.TYP_EMAIL.GetHashCode().ToString());
            divCc.Controls.Add(labelinputCc);

            Int32 nSelectedCCDescId = eLibTools.GetNum(_mailing.GetParamValue("ccrecipient"));
            int nFieldDescId;
            int nFieldGenericDescId;

            nFieldDescId = _fldCc.FldInfo.Descid;
            nFieldGenericDescId = nFieldDescId % 100;
            _fldCc.FldInfo.PosRowSpan = 1;

            HtmlGenericControl selectCC = new HtmlGenericControl("select");
            divCc.Controls.Add(selectCC);
            selectCC.ID = String.Concat(TableType.CAMPAIGN.GetHashCode(), "_optmail_cc");
            selectCC.Attributes.Add("name", selectCC.ID);
            selectCC.Attributes.Add("class", "mailCc");
            selectCC.Attributes.Add("onchange", "oMailing.SetParam('ccrecipient', this.options[this.selectedIndex].value);");

            if (_mailing.Id > 0)
            {
                //fill droplist
                Int32 mailFieldDescId = eLibTools.GetNum(_mailing.GetParamValue("mailFieldDescId"));
                Int32 nTab = eLibTools.GetTabFromDescId(mailFieldDescId);

                if (nTab > 0)
                {

                    foreach (KeyValuePair<int, string> kv in _mailing.EmailFields)
                    {
                        if (eLibTools.GetTabFromDescId(kv.Key) == nTab && kv.Key != mailFieldDescId)
                        {
                            HtmlGenericControl option = new HtmlGenericControl("option");
                            option.Attributes.Add("value", kv.Value);
                            option.InnerText = kv.Value;
                            if (nSelectedCCDescId > 0)
                                option.Attributes.Add("selected", nSelectedCCDescId.ToString());
                            selectCC.Controls.Add(option);
                        }
                    }
                }

            }

            //  FieldsDescId.Add(_fldCc.FldInfo.Descid.ToString());
            #endregion

            #region BCC
            HtmlGenericControl divBCC = new HtmlGenericControl("div");
            divBCC.Attributes.Add("class", "field-container");
            divBCC.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldBcc.FldInfo.Descid);
            divBCC.Attributes.Add("colspan", "1");
            divBCC.Attributes.Add("rowspan", "1");
            divBCC.Attributes.Add("did", _fldBcc.FldInfo.Descid.ToString());
            divSendingInformation.Controls.Add(divBCC);

            HtmlGenericControl labelinputBCc = new HtmlGenericControl("label");
            labelinputBCc.InnerHtml = eResApp.GetRes(Pref, 391);//BCC
            divBCC.Controls.Add(labelinputBCc);

            HtmlGenericControl inputBcc = new HtmlGenericControl("input");
            inputBcc.Attributes.Add("Id", string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldBcc.FldInfo.Descid, "_0_0_0"));
            inputBcc.Attributes.Add("name", string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldBcc.FldInfo.Descid, "_0_0_0"));
            inputBcc.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldBcc)));
            inputBcc.Attributes.Add("type", "text");
            inputBcc.Attributes.Add("efld", "1");
            divBCC.Controls.Add(inputBcc);
            FieldsDescId.Add(_fldBcc.FldInfo.Descid.ToString());
            #endregion

            #region HTML/TEXT
            HtmlGenericControl divHtmlText = new HtmlGenericControl("div");
            divHtmlText.Attributes.Add("class", "radio");
            divHtmlText.ID = string.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_0_0_0");
            divHtmlText.Attributes.Add("ename", (eTools.GetFieldValueCellName(_myFile.Record, _fldIsHTML)));
            divHtmlText.Attributes.Add("colspan", "1");
            divHtmlText.Attributes.Add("rowspan", "1");
            divHtmlText.Attributes.Add("did", _fldIsHTML.FldInfo.Descid.ToString());
            divHtmlText.Attributes.Add("rval", "1"); // la valeur du bouton radio est conservée en attribut de la div parente
            divSendingInformation.Controls.Add(divHtmlText);

            #region div HTML

            //input radio
            HtmlInputRadioButton inputradioHTML = new HtmlInputRadioButton();

            inputradioHTML.Name = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R");
            inputradioHTML.ID = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R_RH");

            inputradioHTML.Checked = (_fldIsHTML.DisplayValue == "1");
            inputradioHTML.Value = "1";
            inputradioHTML.Attributes.Add("onclick", "this.parentNode.setAttribute('rval', this.value);");
            divHtmlText.Controls.Add(inputradioHTML);

            //input label HTML
            HtmlGenericControl labelinputradiohtml = new HtmlGenericControl("label");
            labelinputradiohtml.InnerHtml = eResApp.GetRes(Pref, 1004);//html
            labelinputradiohtml.Attributes.Add("class", "radio-label");
            labelinputradiohtml.Attributes.Add("for", String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R_RH"));
            divHtmlText.Controls.Add(labelinputradiohtml);
            #endregion

            #region div Text
            //div Text

            //input radio
            HtmlInputRadioButton inputradioText = new HtmlInputRadioButton();
            inputradioText.Name = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R");
            inputradioText.ID = String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R_RT");
            inputradioHTML.Checked = (_fldIsHTML.DisplayValue == "1");
            inputradioText.Value = "0";
            inputradioText.Attributes.Add("onclick", "this.parentNode.setAttribute('rval', this.value);");
            divHtmlText.Controls.Add(inputradioText);

            //input label Text
            HtmlGenericControl labelinputradioText = new HtmlGenericControl("label");
            labelinputradioText.InnerHtml = eResApp.GetRes(Pref, 1001);//html
            labelinputradioText.Attributes.Add("class", "radio-label");
            labelinputradioText.Attributes.Add("for", String.Concat("COL_", TableType.CAMPAIGN.GetHashCode(), "_", _fldIsHTML.FldInfo.Descid, "_R_RT"));
            divHtmlText.Controls.Add(labelinputradioText);
            #endregion

            FieldsDescId.Add(_fldIsHTML.FldInfo.Descid.ToString());
            #endregion


            HtmlInputHidden fieldsDescIdInfosCampaign = new HtmlInputHidden();
            fieldsDescIdInfosCampaign.ID = String.Concat("fieldsId_InfosCampaign_", _tab);
            _divHidden.Controls.Add(fieldsDescIdInfosCampaign);
            fieldsDescIdInfosCampaign.Value = eLibTools.Join(";", FieldsDescId);

            formCampaignInfo.Controls.Add(divSendingInformation);

            return formCampaignInfo;
        }

    }
}