using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using EudoExtendedClasses;

namespace Com.Eudonet.Xrm
{
    /// <className>eFormularFileRenderer</className>
    /// <summary>Classe gérant la partie rendu des fiches formulaires</summary>
    /// <authors>GCH</authors>
    /// <date>2014-08-26</date>
    public class eFormularAdvFileRenderer : eFormularFileRenderer
    {
        #region VARIABLES

        //Pour savoir si on inclut ou pas les fichier des composants
        private Boolean hasPhoneField = false;
        /// <summary>Javascript chargé dans le body de la page </summary>
        private StringBuilder dataVueScript;
        private string m_NoDataMessage = "";
        #endregion

        /// <summary>Construction du Formulaire de rendu du fichier</summary>
        /// <param name="uid">UID de base de données</param>
        /// <param name="appExternalUrl">Url externe du formulaire</param>
        /// <param name="pref">Preferences</param>
        /// <param name="formFile">Données du formlaires</param>
        public eFormularAdvFileRenderer(string uid, string appExternalUrl, ePref pref, eFormularFile formFile)
            : base(uid, appExternalUrl, pref, formFile)
        {
            dataVueScript = new StringBuilder();
            m_NoDataMessage = eResApp.GetRes(Pref.LangId, 2807);
        }

        /// <summary>
        /// Appel l'objet métier
        ///  eList/eFiche (l'appel a EudoQuery est fait dans cet appel ainsi que l'appel et le parcours du dataset)
        /// </summary>
        /// <returns></returns>
        protected override Boolean Init()
        {
            _bIsFileRenderer = true;

            if (!FormFile.Generate())
            {
                _eException = FormFile.InnerException;
                _sErrorMsg = FormFile.ErrorMsg;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Construit le corps de l'assistant éditeur de formulaires avancés
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                if (FormFile.BodyMsgDateStart.content != null)
                    MsgDateStart = string.Concat(HttpUtility.HtmlDecode(eMergeTools.GetBodyMerge_Formular(FormFile.BodyMsgDateStart, fbp))
                    , copyrightDiv
                    );

                if (FormFile.BodyMsgDateEnd.content != null)
                    MsgDateEnd = string.Concat(HttpUtility.HtmlDecode(eMergeTools.GetBodyMerge_Formular(FormFile.BodyMsgDateEnd, fbp))
                , copyrightDiv
                );

                //Personnalisation des styles Champs de Saisie
                StringBuilder sbBodyCSS = new StringBuilder(BodyCss);
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.LinkColor))
                {
                    sbBodyCSS.AppendLine().Append(".v-form a:not(.eudolink) { color: " + FormFile.FormularExtendedParam.LinkColor.Substring(0, 7) + "!important;}          ");
                }

                sbBodyCSS.AppendLine().Append("html div.theme--light.v-input input, html div.theme--light.v-select span, html div.theme--light.v-select__slot div { ");
                if (FormFile.FormularExtendedParam.FontSize > 0)
                    sbBodyCSS.Append(string.Concat("font-size:", FormFile.FormularExtendedParam.FontSize.ToString(), "px;")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.PoliceColor))
                    sbBodyCSS.Append(string.Concat("color:", FormFile.FormularExtendedParam.PoliceColor.Substring(0, 7), "!important;")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.FontName))
                    sbBodyCSS.Append(string.Concat("font-family:", FormFile.FormularExtendedParam.FontName)).AppendLine();
                sbBodyCSS.Append("}").AppendLine();

                //demande #86 074
                sbBodyCSS.AppendLine().Append("html div.theme--light.v-textarea, html div.theme--light.v-select span, html div.theme--light.v-select__slot div, html div.theme--light.v-text-field div .v-input__slot .v-text-field__slot textarea, ").Append(string.IsNullOrEmpty(FormFile.FormularExtendedParam.ButtonPoliceColor) ? ".v-btn__content, " : "").Append(".v -messages__message, .v-list-item__content { ");
                if (FormFile.FormularExtendedParam.FontSize > 0)
                    sbBodyCSS.Append(string.Concat("font-size:", FormFile.FormularExtendedParam.FontSize.ToString(), "px;")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.PoliceColor))
                    sbBodyCSS.Append(string.Concat("color:", FormFile.FormularExtendedParam.PoliceColor.Substring(0, 7), "!important;")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.FontName))
                    sbBodyCSS.Append(string.Concat("font-family:", FormFile.FormularExtendedParam.FontName)).AppendLine();
                sbBodyCSS.Append("}").AppendLine();

                sbBodyCSS.AppendLine().Append("html label.theme--light.v-label { ");
                if (FormFile.FormularExtendedParam.FontSize > 0)
                    sbBodyCSS.Append(string.Concat("font-size:", FormFile.FormularExtendedParam.FontSize.ToString(), "px;")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.PoliceColor))
                    sbBodyCSS.Append(string.Concat("color:", FormFile.FormularExtendedParam.PoliceColor.Substring(0, 7), ";")).AppendLine();
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.FontName))
                    sbBodyCSS.Append(string.Concat("font-family:", FormFile.FormularExtendedParam.FontName)).AppendLine();
                sbBodyCSS.Append("}").AppendLine();

                //Couleur de la police des bouttons
                sbBodyCSS.AppendLine().Append(".v-btn__content { ");
                if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.ButtonPoliceColor))
                    sbBodyCSS.Append(string.Concat("color:", FormFile.FormularExtendedParam.ButtonPoliceColor.Substring(0, 7), "!important;")).AppendLine();
                sbBodyCSS.Append("}").AppendLine();

                BodyCss = sbBodyCSS.ToString();
            }
            else
            {
                _eException = FormFile.InnerException;
                _sErrorMsg = FormFile.ErrorMsg;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Script d'initialisation
        /// </summary>
        /// <returns></returns>
        public override string AppendInitScript()
        {
            StringBuilder sb = new StringBuilder();
            //récupère langue du navigateur, avec valeur par défaut = "fr"
            sb.Append("<script type='text/javascript'>").AppendLine()
            .Append("var vueJSInstance;").AppendLine()
            .Append("function validateAndSubmitForm() {").AppendLine()
            .Append("return eUserFormAdv.SubmitForm();").AppendLine()
            .Append("}").AppendLine()
            .Append("$(document).ready(function() {;").AppendLine()
            .Append("     var lg = window.navigator.userLanguage || window.navigator.language;").AppendLine()
            .Append("     if(typeof(lang) == \"undefined\") lg = \"fr\";").AppendLine()
            .Append("     lg = lg.split('-')[0].toLowerCase();").AppendLine()
            .Append("     $('form[name=\"advform\"]').submit(function (event) { event.preventDefault(); validateAndSubmitForm();  });").AppendLine()
            .Append("     $('form[name=\"advform\"]').removeAttr('novalidate');").AppendLine();
            sb.Append(InnerRawScript).AppendLine()
            .Append("});").AppendLine();
            //script de création de l'élément vueJS 

            sb.Append("(() => {").AppendLine();
            sb.Append("const opts = {").AppendLine();
            sb.Append("    icons: {").AppendLine();
            sb.Append("    iconfont: \"mdi\"").AppendLine();
            sb.Append("    },").AppendLine();
            sb.Append("    lang: { ").AppendLine();
            sb.Append("    locales: { fr, en }, ").AppendLine();
            sb.Append("    current: '" + (this.FormFile.LangId == 0 ? "fr" : "en") + "', ").AppendLine();//modifier la langue des res eudofront
            sb.Append("    },").AppendLine();
            sb.Append("    theme:").AppendLine();
            sb.Append("    {").AppendLine();
            sb.Append("    options:").AppendLine();
            sb.Append("        {").AppendLine();
            sb.Append("        customProperties: true").AppendLine();
            sb.Append("       },").AppendLine();

            sb.Append("        themes:").AppendLine();
            sb.Append("        {").AppendLine();
            sb.Append("        light:").AppendLine();
            sb.Append("            {").AppendLine();
            sb.Append("            primary: \"" + ((FormFile.FormularExtendedParam != null && !string.IsNullOrEmpty(FormFile.FormularExtendedParam.AccentuationColor)) ? FormFile.FormularExtendedParam.AccentuationColor.Substring(0, 7) : "#bb1515") + "\",").AppendLine();
            sb.Append("                secondary: \"#757575\", ").AppendLine();
            sb.Append("                accent: \"#82B1FF\", ").AppendLine();
            sb.Append("                error: \"#FF5252\", ").AppendLine();
            sb.Append("                info: \"#2196F3\", ").AppendLine();
            sb.Append("                success: \"#4CAF50\", ").AppendLine();
            sb.Append("                warning: \"#FFC107\"").AppendLine();
            sb.Append("            }").AppendLine();
            sb.Append("        }").AppendLine();
            sb.Append("    },").AppendLine();

            sb.Append("};").AppendLine();

            sb.Append("                Vue.use(eudoFront.default);").AppendLine();
            sb.Append("                Vue.use(Vuetify);").AppendLine();

            sb.Append("                vueJSInstance = new Vue({").AppendLine();
            sb.Append("                    vuetify: new Vuetify(opts),").AppendLine();
            sb.Append("                    el: \"#app\",").AppendLine();
            sb.Append("                    template: `<v-app style='background:var(--v-background-base);'>").AppendLine();
            sb.Append("      <v-form ref='form'  v-model='valid' name='advform'> ").AppendLine();

            //Ajout d'un dialog 
            sb.Append("      <v-dialog").AppendLine();
            sb.Append("      v-model='showDialog' persistent max-width='500' >").AppendLine();
            sb.Append("      <v-card>").AppendLine();
            sb.Append("       <v-card-title class='white--text primary'>").AppendLine();
            sb.Append("      {{ dialogTitle }}").AppendLine();
            sb.Append("       </v-card-title>").AppendLine();
            sb.Append("      <v-card-text class='paddingModal'>").AppendLine();
            sb.Append("      {{ dialogText }}").AppendLine();
            sb.Append("      </v-card-text>").AppendLine();
            sb.Append("       <v-divider></v-divider>").AppendLine();
            sb.Append("      <v-card-actions>").AppendLine();
            sb.Append("      <v-spacer></v-spacer>").AppendLine();
            sb.Append("      <v-btn color='primary darken-1'  text  @click='hideDialogAndRedirect()' > ").AppendLine();
            sb.Append("      OK").AppendLine();
            sb.Append("      </v-btn>").AppendLine();
            sb.Append("      </v-card-actions>").AppendLine();
            sb.Append("      </v-card>").AppendLine();
            sb.Append("      </v-dialog>").AppendLine();

            //si le formulaire contient un boutton de paiement en ligne et que l'extension est désactivée on affiche un message d'erreur
            if (this.FormFile.AllBtnWorldLineDescInfo.Count > 0 && !eExtension.IsReadyStrict(_ePref, "WORLDLINECORE", true))
            {
                sb.Append("      <v-alert").AppendLine();
                sb.Append("      v-show='true'").AppendLine();
                sb.Append("      style='width:50%;margin:0 auto;top:20px' border='right' colored-border   type = 'error'  elevation = '2'    > ").AppendLine();
                sb.Append(eResApp.GetRes(Pref, 8832)).AppendLine();
                sb.Append("       </v-alert>").AppendLine();
            }
            else if (this.FormFile.AllBtnWorldLineDescInfo.Count > 0 && !IsAllBlocBtnPaymentAreMentionned)
            {
                string error = eResApp.GetRes(Pref, 8796).TrimEnd();

                sb.Append("      <v-alert").AppendLine();
                sb.Append("      v-show='true'").AppendLine();
                sb.Append("      style='width:50%;margin:0 auto;top:20px' border='right' colored-border   type = 'error'  elevation = '2'    > ").AppendLine();
                sb.Append(error).AppendLine();
                sb.Append("       </v-alert>").AppendLine();
            }
            else
                sb.Append(BodyMerge.Replace("\"", "\\\""));

            sb.Append("      </v-form>").AppendLine();
            sb.Append("    </v-app>").AppendLine();
            sb.Append("    `,").AppendLine();

            sb.Append("                    computed:").AppendLine();
            sb.Append("        {").AppendLine();
            if (hasPhoneField)
                sb.Append("            getInvalidPhoneMsg(){ '" + eResApp.GetRes(Pref.LangId, 6275) + "'},").AppendLine();
            sb.Append("        },").AppendLine();

            sb.Append("                    methods:").AppendLine();
            sb.Append("        {").AppendLine();
            sb.Append("            validateAndSubmitForm(isOnlinePaymentBtn, onlinePaymentParams){ eUserFormAdv.SubmitForm(isOnlinePaymentBtn, onlinePaymentParams);},").AppendLine();
            sb.Append("            hideDialogAndRedirect(){ ").AppendLine();
            sb.Append("                 this.showDialog = false;").AppendLine();
            sb.Append("                 top.location = this.redirectionUrl;").AppendLine();
            sb.Append("                                   },").AppendLine();
            sb.Append("        },").AppendLine();
            //data
            sb.Append("                    data()").AppendLine();
            sb.Append("        {").AppendLine();
            sb.Append("            return {").AppendLine();
            //on ajoute les v-models
            foreach (KeyValuePair<string, string> keyValue in _DicoDescIdVModelValue)
            {
                sb.Append(string.Concat(keyValue.Key, ":", keyValue.Value, ",")).AppendLine();
            }

            sb.Append(dataVueScript).AppendLine();
            sb.Append("            label: '',").AppendLine();
            sb.Append("            redirectionUrl: '',").AppendLine();
            if (hasPhoneField)
                sb.Append(@"            phonePatern:/^(?:(?:\(?(?:00|\+)([1-4]\d\d|[1-9]\d?)\)?)?[\s\-\.\ \\\/]?)?((?:\(?\d{1,}\)?[\s\-\.\ \\\/]?){0,})(?:[\s\-\.\ \\\/]?(?:#|ext\.?|extension|x)[\s\-\.\ \\\/]?(\d+))?$/,").AppendLine();
            sb.Append("            dialogTitle: '',").AppendLine();
            sb.Append("            dialogText: '',").AppendLine();
            sb.Append("            invalidMontantMsg: \"" + eResApp.GetRes(Pref.LangId, 3086) + "\",").AppendLine();
            sb.Append("            invalidMontantTitle: \"" + eResApp.GetRes(Pref.LangId, 3087) + "\",").AppendLine();
            sb.Append("            valid: true,").AppendLine();
            sb.Append("            showDialog: false,").AppendLine();
            sb.Append("                            input: ''").AppendLine();
            sb.Append("                        }").AppendLine();
            sb.Append("        }").AppendLine();
            sb.Append("    })").AppendLine();

            sb.Append("            })()").AppendLine();

            sb.Append("</script>");
            sb.Append(VueAppScript);
            return sb.ToString();

        }

        /// <summary>
        /// check if columns are missed on payment button
        /// </summary>
        public bool IsAllBlocBtnPaymentAreMentionned
        {
            get
            {
                bool isColumnMissed = false;
                if (this.FormFile != null && this.FormFile.AllBtnWorldLineDescInfo != null && this.FormFile.AllBtnWorldLineDescInfo.Count != 0)
                    this.FormFile.AllBtnWorldLineDescInfo.ForEach(b =>
                    {
                        if (b.WorldLineDescInfoList.Count < 3)
                        {
                            isColumnMissed = true;
                        }
                    });
                return !isColumnMissed;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nDescId"></param>
        /// <param name="isUpdatable"></param>
        /// <returns></returns>
        protected override string FieldRender(Int32 nDescId, Boolean isUpdatable)
        {
            eFieldRecord fld = null;
            if (dicFlds.ContainsKey(nDescId))
                fld = dicFlds[nDescId];
            if (row == null || fld == null)
                return string.Empty;

            // Sécurisation des formulaire
            if (fld.FldInfo.Format != FieldFormat.TYP_MEMO)
                fld.DisplayValue = HtmlTools.StripHtml(fld.DisplayValue);

            //Si pas le droit en écriture sur le champ, on force la lecture seule
            //fld.RightIsUpdatable = (isUpdatable || fld.FldInfo.Format == FieldFormat.TYP_MEMO) && fld.RightIsUpdatable;
            // HLA - #50815 - Mode sans gestion de droits pour les utilisateurs systémes (EdnUser) Dans notre cas, le UserInfo = EDN_FORMULAR
            // Si le champs est en lecture seule alors pas de droit de modification
            fld.RightIsUpdatable = (isUpdatable || fld.FldInfo.Format == FieldFormat.TYP_MEMO) && !fld.FldInfo.ReadOnly;

            WebControl parentContainer = new Label();

            WebControl valueCell = GetFieldValueCell(row, new List<eFieldRecord> { fld }, 0, Pref, ThemePaths);

            if (fld.RightIsUpdatable && fld.FldInfo.Format == FieldFormat.TYP_MEMO && !fld.FldInfo.IsHtml)
            {
                valueCell.Attributes.Add("onblur", string.Format("eUsrFld.OB(event, '{0}');", valueCell.ID));
                valueCell.Attributes.Add("onclick", string.Format("eUsrFld.OC(event, '{0}');", valueCell.ID));
                valueCell.Attributes.Add("onkeyup", string.Format("eUsrFld.OKU(event, '{0}');", valueCell.ID));
                //  valueCell.Attributes.Add("onselect", string.Format("eUsrFld.OS(event, '{0}');", valueCell.ID));
            }



            #region Type Champ mémo
            if (fld.FldInfo.Format == FieldFormat.TYP_MEMO)
            {
                // Pour que getFieldsInfo reconnaisse le textarea
                valueCell.Attributes.Add("efrmr", "1");

                // si l'option lecture seule ou champ en lecture seul
                if (!fld.RightIsUpdatable || !isUpdatable)
                {
                    valueCell.Attributes.Add("readonly", "true");
                    valueCell.Attributes.Add("disabled", "true");
                }

                // Pour le text on utilsera pas ckeditor juste un textarea
                if (!fld.FldInfo.IsHtml)
                {
                    valueCell.Style.Add("width", "99%");
                    valueCell.Style.Add("height", "200px");
                    valueCell.Style.Add("resize", "none");
                }
            }
            #endregion

            Boolean bDisplayBtn = isUpdatable;
            Label buttonCell = new Label();
            GetBaseButtonCell(buttonCell, valueCell, bDisplayBtn);
            buttonCell.Attributes.Add("onclick", string.Format("eUsrFld.OC(event, '{0}');", valueCell.ID));

            parentContainer.Controls.Add(valueCell);
            parentContainer.Controls.Add(buttonCell);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            parentContainer.RenderControl(tw);

            return sb.ToString();
        }

        /// <summary>
        /// Créer le rendu des composants eudo-front
        /// </summary>
        /// <param name="advancedFormularParam">Informations sur le composant</param>
        /// <returns></returns>
        protected override string FieldRenderAdv(AdvancedFormularParam advancedFormularParam)
        {
            eFieldRecord fld = null;
            if (dicFlds.ContainsKey(advancedFormularParam.DescId))
                fld = dicFlds[advancedFormularParam.DescId];
            if (row == null || fld == null)
                return string.Empty;

            string labelResult = HttpUtility.HtmlDecode(eMergeTools.GetContentMerge_Formular(advancedFormularParam.LabelData, fbp, advancedFormularParam.IsMandatory, advancedFormularParam.IsCheckBox));
            HtmlGenericControl labelCell = new HtmlGenericControl("div");
            labelCell.InnerHtml = labelResult;

            fld.DisplayValue = HtmlTools.StripHtml(fld.DisplayValue);


            // Si le champs est en lecture seule alors pas de droit de modification
            fld.RightIsUpdatable = !fld.FldInfo.ReadOnly;

            WebControl labelHidenCell = new Label();
            GetFieldLabelCell(labelHidenCell, row, new List<eFieldRecord> { fld });   //Ajout du libellé
            labelHidenCell.Style.Add(HtmlTextWriterStyle.Display, "none");  //le libellé doit-être dans la page mais masqué

            //Ajout des descid modifiable uniquement, pour le champ mémo la lecture
            FieldsDescId.Add(advancedFormularParam.DescId.ToString());
            AllowedFieldsDescId.AddContains(advancedFormularParam.DescId.ToString());

            //si en mode création, la case à cocher est coché par défaut et que ce n'est pas fait coté admin, on rajoute cette info
            if (advancedFormularParam.IsChecked && fld.FldInfo.Format == FieldFormat.TYP_BIT
                && (string.IsNullOrEmpty(fld.DisplayValue) || fld.DisplayValue == "0"))
                fld.DisplayValue = "1";

            var parentContainer = GetEudoFrontFieldValueCell(row, fld, 0, advancedFormularParam, labelCell.InnerText);
            //TODO: ajouter l'info de type formular dans l'élément eudofront
            if (fld.FldInfo.Format == FieldFormat.TYP_MEMO)
            {
                labelHidenCell.Attributes.Add("efrmr", "1");
            }
            //on ajoute les composants du rendu champ de saisie
            parentContainer.Controls.AddAt(0, labelHidenCell);


            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            parentContainer.RenderControl(tw);

            return sb.ToString();
        }
        /// <summary>
        /// permet de créer un composant html 'eudofront' en écriture à partir de eFieldRecord 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="idx"></param>
        /// <param name="advancedFormularParam"></param>
        /// <param name="labelHtml"></param>
        /// <returns></returns>
        HtmlGenericControl GetEudoFrontFieldValueCell(eRecord row, eFieldRecord fieldRow, Int32 idx, AdvancedFormularParam advancedFormularParam, string labelHtml)
        {
            eFieldRecord fieldRowAliasRelation = fieldRow;
            HtmlGenericControl ednWebCtrl = null;
            HtmlGenericControl parentContainer = new HtmlGenericControl("div");
            parentContainer.Attributes.Add("class", advancedFormularParam.ClassName);
            // Cas particuliers pour l'affichage de certains champs : Email.De, Email.HTML
            bool bIsMailConsultField = _rType == RENDERERTYPE.MailFile || _rType == RENDERERTYPE.SMSFile || (_rType == RENDERERTYPE.PrintFile && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || _rType == RENDERERTYPE.PrintFile && fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS);
            bool bIsMailEditField = _rType == RENDERERTYPE.EditMail || _rType == RENDERERTYPE.EditMailing || _rType == RENDERERTYPE.EditSMS || _rType == RENDERERTYPE.EditSMSMailing;

            bool bIsMailField = bIsMailConsultField || bIsMailEditField;
            bool bIsMailFromField = bIsMailField
                && fieldRow.FldInfo.Descid == ((_rType == RENDERERTYPE.EditMailing || _rType == RENDERERTYPE.EditSMSMailing) ? CampaignField.SENDER.GetHashCode() : MailField.DESCID_MAIL_FROM.GetHashCode() + _tab);

            bool bIsMailHistoField = (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS)
                && fieldRow.FldInfo.Descid == _tab + MailField.DESCID_MAIL_HISTO.GetHashCode();
            bIsMailHistoField = bIsMailHistoField || fieldRow.FldInfo.Descid == CampaignField.HISTO.GetHashCode();

            bool bIsHeaderLink = _rType == RENDERERTYPE.FileParentInHead && fieldRow.FldInfo.Descid % 100 == 1;
            bool bisHeaderField = _rType == RENDERERTYPE.FileParentInHead && fieldRow.FldInfo.Descid % 100 != 1;
            //on créé l'élément Eudofront pour chaque type de rubrique
            switch (fieldRow.FldInfo.Format)
            {
                case FieldFormat.TYP_EMAIL:
                    ednWebCtrl = new HtmlGenericControl("edn-mail");
                    break;
                case FieldFormat.TYP_PHONE:
                    ednWebCtrl = new HtmlGenericControl("edn-phone");
                    hasPhoneField = true;
                    ednWebCtrl.Attributes.Add("invalidPhoneMsg", eResApp.GetRes(Pref.LangId, 6275));
                    ednWebCtrl.Attributes.Add(":pattern", "this.phonePatern");
                    break;
                case FieldFormat.TYP_BIT:
                case FieldFormat.TYP_BITBUTTON:
                    ednWebCtrl = new HtmlGenericControl("edn-check");
                    break;
                case FieldFormat.TYP_CHAR:
                    if (IsCatalogEditable(fieldRow) && fieldRow.RightIsUpdatable)//Si c'est un cataloqgue
                    {
                        if (!fieldRow.FldInfo.Multiple)
                        {
                            ednWebCtrl = new HtmlGenericControl("v-autocomplete");
                            ednWebCtrl.Attributes.Add("edncatsimple", "1");//de type catalogue
                            if (advancedFormularParam.IsMandatory)
                            {
                                //TODO: créer le composant edn-cat simple dans eudofront
                                HtmlGenericControl slot = new HtmlGenericControl("template");
                                slot.Attributes.Add("v-slot:label", "");
                                slot.InnerHtml = string.Concat(new System.Text.RegularExpressions.Regex("(<.*?>\\s*)+", System.Text.RegularExpressions.RegexOptions.Singleline).Replace(labelHtml, " ").Trim(), " <span class='red--text'>*</span>");
                                ednWebCtrl.Controls.Add(slot);
                            }
                        }
                        else
                        {
                            ednWebCtrl = new HtmlGenericControl("edn-cat");
                            //Ajout le rendu d'une rubrique de type choix multiple
                            ednWebCtrl.Attributes.Add("edncatMultiple", "1");
                            ednWebCtrl.Attributes.Add(":multiple", "true");
                            ednWebCtrl.Attributes.Add(":chips", "true");
                            ednWebCtrl.Attributes.Add(":deletable-chips", "true");
                            ednWebCtrl.Attributes.Add("small-chips", "true");
                            ednWebCtrl.Attributes.Add(":autocomplete", "true");
                        }
                    }
                    else
                        ednWebCtrl = new HtmlGenericControl("edn-field");
                    break;
                case FieldFormat.TYP_NUMERIC:
                case FieldFormat.TYP_MONEY:
                    ednWebCtrl = new HtmlGenericControl("edn-num");
                    break;
                case FieldFormat.TYP_DATE:
                    {
                        ednWebCtrl = new HtmlGenericControl("edn-date");
                        ednWebCtrl.Attributes.Add(":popup", "true");
                        ednWebCtrl.Attributes.Add("format", eLibConst.DateFormat[Pref.CultureInfo]);

                    }
                    break;
                case FieldFormat.TYP_MEMO:
                    ednWebCtrl = new HtmlGenericControl("edn-memo");
                    break;
            }

            // Id de la fiche du field
            Int32 fieldFileId = fieldRow.FileId;

            // Id de la table principale
            Int32 nMasterFileId = row.MainFileid;

            // Nom de la cellule - commun à toutes la colonne, entête inclus
            String colName = eTools.GetFieldValueCellName(row, fieldRow);
            String shortColName = colName.Replace("COL_", "");

            // ID de la cellule
            string idElement = eTools.GetFieldValueCellId(row, fieldRow, idx);
            ednWebCtrl.Attributes.Add("id", idElement);
            ednWebCtrl.Attributes.Add("ename", colName);
            ednWebCtrl.Attributes.Add("eudofront", "1");


            // #49 045 - TabIndex (aka. TabOrder)
            // Son indexation doit commencer à 1
            // http://www.w3schools.com/tags/att_global_tabindex.asp
            // Pour éviter de mettre tous les TabIndex à 0, on utilise le DispOrder si TabIndex n'est pas renseigné en base
            //ednWebCtrl.Attributes.Add("tabindex", ((fieldRow.FldInfo.PosTabIndex == 0 ? fieldRow.FldInfo.PosDisporder : fieldRow.FldInfo.PosTabIndex) + 1).ToString());

            if (fieldRow.FldInfo.Watermark.Length > 0)
                ednWebCtrl.Attributes.Add("placeholder", fieldRow.FldInfo.Watermark);

            // Si on est sur la visualisation d'un type E-mail, les champs sont en lecture seule sauf Historisé
            if ((fieldRow.FldInfo.Table.EdnType == EdnType.FILE_MAIL || fieldRow.FldInfo.Table.EdnType == EdnType.FILE_SMS) && !_bIsEditRenderer && !bIsMailHistoField)
                fieldRow.RightIsUpdatable = false;

            //Si campagne non assistante et pas le champ description
            Boolean campaignNotUpdatable = fieldRow.FldInfo.Descid < ((int)TableType.CAMPAIGN + FREE_FIELDS_LIMIT.CAMPAIGN_MIN)
                || fieldRow.FldInfo.Descid > ((int)TableType.CAMPAIGN + FREE_FIELDS_LIMIT.CAMPAIGN_MAX);


            campaignNotUpdatable = campaignNotUpdatable
                && fieldRow.FldInfo.Descid != CampaignField.DESCRIPTION.GetHashCode()
                && fieldRow.FldInfo.Descid != CampaignField.HISTO.GetHashCode();

            if (fieldRow.FldInfo.Table.DescId == TableType.CAMPAIGN.GetHashCode()
                && (_rType != RENDERERTYPE.EditMailing && _rType != RENDERERTYPE.EditSMSMailing)
                && campaignNotUpdatable)
                fieldRow.RightIsUpdatable = false;

            // En admin, rubrique "Couleurs" pour Planning est en lecture seule
            if (fieldRow.FldInfo.Table.EdnType == EdnType.FILE_PLANNING
                && _rType == RENDERERTYPE.AdminFile
                && fieldRow.FldInfo.Descid % 100 == PlanningField.DESCID_CALENDAR_COLOR.GetHashCode())
                fieldRow.RightIsUpdatable = false;

            // Rend le rendu en lecture seule
            if (ReadonlyRenderer || fieldRow.FldInfo.Format == FieldFormat.TYP_PASSWORD)
                fieldRow.RightIsUpdatable = false;

            // Classe de la colonne - Classe de base
            StringBuilder sbClass = new StringBuilder();
            if (!fieldRow.RightIsVisible)
            {
                ednWebCtrl.Attributes.Add("placeholder", fieldRow.FldInfo.Watermark);
            }
            else
            {

                switch (fieldRow.FldInfo.Format)
                {
                    case FieldFormat.TYP_CHAR:
                    case FieldFormat.TYP_MEMO:
                    case FieldFormat.TYP_EMAIL:
                    case FieldFormat.TYP_PHONE:
                        {
                            if (IsCatalogEditable(fieldRow))
                            {
                                ednWebCtrl.Attributes.Add("edncat", "1");//de type catalogue

                                eCatalog cat = LoadCatalog(fieldRow);
                                var dataItemStr = string.Concat(idElement, "Items");
                                dataVueScript.Append(dataItemStr).Append(":[");

                                string selectedValue = "[";

                                foreach (eCatalog.CatalogValue cv in cat.Values)
                                {

                                    string displayValue = cv.DisplayValue.Replace("'", "\\'").Replace(Environment.NewLine, "").Replace("\n", "").Replace("\r", "");

                                    dataVueScript.Append(string.Concat("{ value:'", cv.DbValue, "',", " text:'", displayValue, "'},"));
                                    if (!string.IsNullOrEmpty(fieldRow.Value) && (";" + fieldRow.Value + ";").Contains(";" + cv.DbValue + ";"))
                                        selectedValue += string.Concat("'", cv.DbValue, "',");
                                }

                                dataVueScript.Append("],").AppendLine();
                                selectedValue += "]";
                                if (!_DicoDescIdVModelValue.ContainsKey(idElement))
                                    _DicoDescIdVModelValue.Add(idElement, fieldRow.FldInfo.Multiple ? selectedValue : string.Concat("'", fieldRow.Value, "'"));

                                ednWebCtrl.Attributes.Add(":items", dataItemStr);
                                ednWebCtrl.Attributes.Add("item-text", "text");
                                ednWebCtrl.Attributes.Add("item-value", "value");
                                ednWebCtrl.Attributes.Add("no-data-text", m_NoDataMessage);
                                ednWebCtrl.Attributes.Add("clearable", "clearable");
                                if (advancedFormularParam.IsMandatory)
                                    ednWebCtrl.Attributes.Add(":rules", "[v => !!(v && v.length) || '" + eResApp.GetRes(Pref.LangId, 7548) + "']");//TODO: utiliser les composants eudofront
                            }
                            else
                            {
                                string displayValue = HttpUtility.JavaScriptStringEncode(fieldRow.DisplayValue);  

                                if (!_DicoDescIdVModelValue.ContainsKey(idElement))
                                    _DicoDescIdVModelValue.Add(idElement, string.Concat("'", displayValue, "'"));
                                ednWebCtrl.Attributes.Add("maxlength", fieldRow.FldInfo.Length.ToString());
                            }
                        }
                        break;
                    case FieldFormat.TYP_NUMERIC:
                    case FieldFormat.TYP_MONEY:
                        {

                            if (!_DicoDescIdVModelValue.ContainsKey(idElement))
                                _DicoDescIdVModelValue.Add(idElement, string.Concat("'", fieldRow.DisplayValue, "'"));
                            ednWebCtrl.Attributes.Add("maxlength", "18");
                        }
                        break;
                    case FieldFormat.TYP_BIT:
                    case FieldFormat.TYP_BITBUTTON:
                        {
                            if (!_DicoDescIdVModelValue.ContainsKey(idElement))
                                _DicoDescIdVModelValue.Add(idElement, fieldRow.DisplayValue == "1" ? "true" : "false");
                        }
                        break;
                    case FieldFormat.TYP_DATE:
                        {
                            //On ajoute les valeurs dans v-model pour chaque élément
                            if (!_DicoDescIdVModelValue.ContainsKey(idElement))
                                _DicoDescIdVModelValue.Add(idElement, string.Concat("''"));
                        }
                        break;
                }
            }

            ednWebCtrl.Attributes.Add("v-model", idElement);

            if (!advancedFormularParam.IsCheckBox)
            {
                if (!string.IsNullOrEmpty(advancedFormularParam.Placeholder))
                    ednWebCtrl.Attributes.Add("placeholder", advancedFormularParam.Placeholder);
            }

            if (advancedFormularParam.IsMandatory)
                ednWebCtrl.Attributes.Add("required", eResApp.GetRes(Pref.LangId, 7548));
            ednWebCtrl.Attributes.Add("label", new System.Text.RegularExpressions.Regex("(<.*?>\\s*)+", System.Text.RegularExpressions.RegexOptions.Singleline).Replace(labelHtml, " ").Trim());

            parentContainer.Controls.Add(ednWebCtrl);
            if (fieldRow.FldInfo.Format == FieldFormat.TYP_DATE && advancedFormularParam.IsTimeInfo)
            {
                //on créée l'élément 'edn-time' en passant l'id du date-time liéé
                HtmlGenericControl ednTimeCtrl = new HtmlGenericControl("edn-time");
                ednTimeCtrl.Attributes.Add("label", "");
                ednTimeCtrl.Attributes.Add("popup", "");
                ednTimeCtrl.Attributes.Add("id", string.Format("tm{0}", idElement));
                if (advancedFormularParam.IsMandatory)
                    ednTimeCtrl.Attributes.Add("required", eResApp.GetRes(Pref.LangId, 7548));
                parentContainer.Controls.Add(ednTimeCtrl);
            }
            else if (fieldRow.FldInfo.Format == FieldFormat.TYP_MEMO)
            {
                ednWebCtrl.Attributes.Add("maxlength", "4000");//La taille maximale du contenu est de  4000 caractéres 
                ednWebCtrl.Attributes.Add("rows", advancedFormularParam.Rows);
            }

            return parentContainer;
        }
    }
}