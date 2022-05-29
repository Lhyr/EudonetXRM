using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;
using System;
using System.Linq;
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
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Internal.Payment;
using static Com.Eudonet.Internal.Payment.IngenicoRestAPICall;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Common.CommonDTO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Mail;
using EudoCommonHelper;

namespace Com.Eudonet.Xrm
{
    /// <className>eFormularFileRenderer</className>
    /// <summary>Classe gérant la partie rendu des fiches formulaires</summary>
    /// <authors>GCH</authors>
    /// <date>2014-08-26</date>
    public class eFormularFileRenderer : eRenderer
    {
        /// <summary>
        /// Permet de choisir le type de datepicker utilisé sur le formulaire
        /// </summary>
        private enum DATEPICKER_TYP
        {
            JQUERYUI,
            EDATEPICKER
        }

        #region Constantes
        private const DATEPICKER_TYP datePickerType = DATEPICKER_TYP.EDATEPICKER;
        #endregion

        #region VARIABLES

        ///// <summary>Preference de l'user en cours</summary>
        //protected ePrefLite _prefLite;
        private string _uid = string.Empty;
        private string _appExternalUrl = string.Empty;

        //Pour savoir si on inclut ou pas les fichier des composants
        private Boolean hasDateField = false;
        private Boolean hasCatalogField = false;
        private Boolean hasMemoField = false;

        /// <summary>
        /// S'il y a un conflit de transactions dans le paiement en ligne
        /// </summary>
        private bool isConflictBetweenTransaction = false;

        /// <summary>
        /// S'il y a un conflit de transactions dans le paiement en ligne
        /// </summary>
        private string conflictBetweenTransactionMessage = string.Empty;

        //resultat du paiement
        private Dictionary<string, IngenicoGetStatusResult> paymentResult = new Dictionary<string, IngenicoGetStatusResult>();
        /// <summary>Liste des descid avec default value</summary>
        protected Dictionary<string, string> _DicoDescIdVModelValue = null;

        //_lstRecords
        protected eRecord row = null;
        protected Dictionary<int, eFieldRecord> dicFlds = null;
        protected ePref.CalculateDynamicTheme ThemePaths = null;
        protected FormularBuildParam fbp = null;
        #endregion

        #region ACCESSEURS

        /// <summary>
        /// copyright
        /// </summary>
        public string copyrightDiv = "";

        /// <summary>Rendu des paramètres du formulaire (internationalisation)</summary>
        public WebControl DivGlobalParam { get; protected set; }

        /// <summary>Css de l'étape 1 du formulaire</summary>
        public string BodyCss { get; protected set; }
        /// <summary>Rendu de l'étape 1 du formulaire</summary>
        public string BodyMerge { get; protected set; }

        /// <summary>Css de l'étape 2 du formulaire</summary>
        public string BodySubmissionCss { get; protected set; }
        /// <summary>Rendu de l'étape 2 du formulaire</summary>
        public string BodySubmissionMerge { get; protected set; }

        /// <summary>
        /// message formulaire pas encore ouvert
        /// </summary>
        public string MsgDateStart { get; protected set; }

        /// <summary>
        /// Message formulaire expiré
        /// </summary>
        public string MsgDateEnd { get; protected set; }

        /// <summary>Liste des fichiers css à charger pour le rendu</summary>
        public List<string> ListCssToRegister { get; private set; }

        /// <summary>Liste des fichiers css des composants utilisés à charger pour le rendu dans le meme  repertoire que le js</summary>
        public List<string> ListCssWithPathToRegister { get; private set; }

        /// <summary>Liste des fichiers script à charger pour le rendu</summary>
        public List<string> ListScriptToRegister { get; private set; }
        /// <summary>Javascript à charger</summary>
        public StringBuilder RawScript { get; private set; }

        /// <summary>Javascript chargé dans le body de la page </summary>
        public StringBuilder InnerRawScript { get; private set; }

        /// <summary>Javascript chargé pour le rendu en Vue </summary>
        public StringBuilder VueAppScript { get; private set; }

        /// <summary> Table template</summary>
        public eFormularFile FormFile { get; private set; }

        /// <summary>Liste des balises meta</summary>
        public List<HtmlMeta> MetaTags { get; protected set; }
        /// <summary>Liste des balises meta pour les reseaux sociaux</summary>
        public List<HtmlMeta> MetaSharingTags { get; private set; }

        /// <summary>
        /// Message paiement déjà effectué
        /// </summary>
        public string MsgPaymentAlreadyDone
        {
            get
            {
                return eResApp.GetRes(this.Pref.LangId, 8829);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConflictBetweenTransaction
        {
            get
            {
                return isConflictBetweenTransaction;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ConflictBetweenTransactionMessage
        {
            get
            {
                return conflictBetweenTransactionMessage;
            }
        }

        /// <summary>
        /// Message paiement effectué
        /// </summary>
        public string MsgPaymentIsDone
        {
            get
            {
                return eResApp.GetRes(this.Pref.LangId, 8833);
            }
        }

        /// Message paiement annulé
        /// </summary>
        public string MsgPaymentCanceled
        {
            get
            {
                return eResApp.GetRes(this.Pref.LangId, 8828);
            }
        }
        #endregion

        /// <summary>Construction du Formulaire de rendu du fichier</summary>
        /// <param name="uid">UID de base de données</param>
        /// <param name="appExternalUrl">Url externe du formulaire</param>
        /// <param name="pref">Preferences</param>
        /// <param name="formFile">Données du formlaires</param>
        public eFormularFileRenderer(string uid, string appExternalUrl, ePref pref, eFormularFile formFile)
        {
            Pref = pref;
            if (FormFile == null)
                FormFile = formFile;
            _uid = uid;
            _appExternalUrl = appExternalUrl;
            ListCssToRegister = new List<string>();
            ListScriptToRegister = new List<string>();
            ListCssWithPathToRegister = new List<string>();
            RawScript = new StringBuilder();
            InnerRawScript = new StringBuilder();
            VueAppScript = new StringBuilder();
            BodyCss = string.Empty;
            MetaTags = new List<HtmlMeta>();
            MetaSharingTags = new List<HtmlMeta>();
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
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            fbp = new FormularBuildParam();
            fbp.Uid = _uid;
            fbp.AppExternalUrl = _appExternalUrl;
            fbp.FormularId = FormFile.FormularId;
            fbp.TplFileId = FormFile.TplFileId;
            fbp.ParentFileId = FormFile.EvtFileId;
            dicFlds = new Dictionary<int, eFieldRecord>();
            if (FormFile.ListRecords.Count > 0)
            {
                row = FormFile.ListRecords[0];
                if (row != null)
                    row.GetFields.ForEach((fld) =>
                    {
                        if (!dicFlds.ContainsKey(fld.FldInfo.Descid))
                            dicFlds.Add(fld.FldInfo.Descid, fld);
                    });
            }

            ThemePaths = new ePref.CalculateDynamicTheme(ePref.Theme.GetDefaultTheme());

            //Rendu d'un champ
            fbp.FieldRender = FieldRender;

            //Rendu d'un formulaire avancé
            fbp.FieldRenderAdv = FieldRenderAdv;

            //récupérer la valeur à partir du desc
            fbp.GetDisplayValueFromDesc = GetDisplayValueFromDesc;

            //Rendu des informations de Cultures
            Dictionary<string, string> dicGLobalParam = new Dictionary<string, string> {
              {"CultureInfo",eLibConst.DateFormat[Pref.CultureInfo]},
              {"NumberDecimalDelimiter",Pref.NumberDecimalDelimiter},
              {"NumberSectionsDelimiter",Pref.NumberSectionsDelimiter}
            };
            DivGlobalParam = new eDivCtrl();
            DivGlobalParam.Style.Add(HtmlTextWriterStyle.Display, "none;");

            foreach (var item in dicGLobalParam)
                DivGlobalParam.Controls.Add(new HtmlInputHidden() { ID = item.Key, Value = item.Value });

            //Rendu du Captcha, Bouton Valider et Annuler






            fbp.BtnValidRender = GetValidRender;
            fbp.BtnAdvValidRender = GetValidFrmAdvRender;
            fbp.BtnWorldlinePaimentRender = GetWorldlinePaimentRender;
            _rType = RENDERERTYPE.FormularFile;

            if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
            {
                if (FormFile.AddFooterEudonetTradeMark)
                {
                    string sLabelTM = eResApp.GetRes(0, 2665);
                    if (FormFile.LangId != 0)
                        sLabelTM = eResApp.GetRes(1, 2665);

                    copyrightDiv = string.Concat("<div id='FormCopyrightAdv' class='advformeudotm'><a class='eudolink' href =\"https://group.eudonet.com\" target=\"_blank\">",
                        sLabelTM,
                        "&nbsp;",
                        "<span class='advformeudo'>",
                        eResApp.GetRes(Pref, 8673),
                        "</span></a></div>");
                }
            }
            else
            {
                copyrightDiv = string.Concat("<div id='FormCopyright'><a href=\"http://www.eudonet.fr\" target=\"_blank\">",
               "<img src='themes/default/images/eMain_logo.png' alt='Powered by Eudonet' title='Eudonet' /><br />Powered by Eudonet</a></div>");
            }

            BodySubmissionCss = FormFile.BodySubmissionCss;
            BodyCss = FormFile.BodyCss;

            if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
            {
                //on charge la langue du formulaire pour les Res
                Pref.LangServId = Pref.User.UserLangId = FormFile.LangId;
                Pref.Lang = Pref.User.UserLang = "LANG_" + FormFile.LangId.ToString().PadLeft(2, '0');
                //on instancie le dict
                _DicoDescIdVModelValue = new Dictionary<string, string>();
            }

            BodyMerge = string.Concat(HttpUtility.HtmlDecode(eMergeTools.GetBodyMerge_Formular(FormFile.BodyObject, fbp))
              , copyrightDiv
              );
            BodyMerge = BodyMerge.Replace("<form", "<div").Replace("</form>", "</div>");

            BodySubmissionMerge = string.Concat(HttpUtility.HtmlDecode(eMergeTools.GetBodyMerge_Formular(FormFile.BodySubmissionObject, fbp))
                , copyrightDiv
                );

            MetaTags = FormFile.ListHtmlMeta;

            if (!string.IsNullOrEmpty(FormFile.MetaTitle))
            {
                if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                    MetaSharingTags.AddRange(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.TITLE, FormFile.MetaTitle));
                else
                    MetaSharingTags.Add(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.TITLE, FormFile.MetaTitle).FirstOrDefault());
            }

            if (!string.IsNullOrEmpty(FormFile.MetaDescription))
            {
                if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                    MetaSharingTags.AddRange(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.DESCRIPTION, FormFile.MetaDescription));
                else
                    MetaSharingTags.Add(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.DESCRIPTION, FormFile.MetaDescription).FirstOrDefault());
            }

            string sUrl = String.Empty;
            if (string.IsNullOrEmpty(FormFile.MetaImgURL) && FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
            {
                sUrl = string.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request),
                  "/themes/default/images/xrm-cover-connect.jpg");
            }
            else if (!string.IsNullOrEmpty(FormFile.MetaImgURL))
            {
                sUrl = FormFile.MetaImgURL;
                if (!FormFile.MetaImgURL.ToLower().StartsWith("http"))
                {
                    sUrl = eLibTools.GetImgExternalLink(sUrl, _ePref.GetBaseName, eLibConst.FOLDER_TYPE.FILES, _ePref.AppExternalUrl);
                }
            }


            if (!String.IsNullOrEmpty(sUrl))
            {
                if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                    MetaSharingTags.AddRange(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.IMAGE, sUrl));
                else
                    MetaSharingTags.Add(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.IMAGE, sUrl).FirstOrDefault());

                if (FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(sUrl);
                    System.Net.WebResponse response = request.GetResponse();
                    System.Drawing.Image image = System.Drawing.Image.FromStream(response.GetResponseStream());
                    MetaSharingTags.AddRange(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.IMAGEHEIGHT, image.Height.ToString()));
                    MetaSharingTags.AddRange(eTools.GetHtmlMetaSocialNetwork(eTools.MetaSocialNetworkType.IMAGEWIDTH, image.Width.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(FormFile.MetaTitle) && !string.IsNullOrEmpty(FormFile.MetaDescription))
            {
                MetaSharingTags.Add(eTools.GetHtmlMetaTwitterSummaryCard());
            }

            return true;
        }

        private string GetSelect2Options(eFieldRecord fieldRow)
        {
            StringBuilder sb = new StringBuilder();

            //les options du select2
            sb.Append("{");
            if (fieldRow.FldInfo.Multiple)
                sb.Append(" placeholder: '").Append(eResApp.GetRes(Pref.LangId, 6798)).Append("',");
            else
                sb.Append(" placeholder: '").Append(eResApp.GetRes(Pref.LangId, 1173)).Append("',");

            sb.Append(" allowClear: true,");
            sb.Append(" language: lg");
            sb.Append("}");

            return sb.ToString();


        }
        /// <summary>
        /// Script d'initialisation
        /// </summary>
        /// <returns></returns>
        public virtual string AppendInitScript()
        {
            StringBuilder sb = new StringBuilder();
            //récupère langue du navigateur, avec valeur par défaut = "fr"
            sb.Append("<script type='text/javascript'>").AppendLine()
                .Append("var vueJSInstance;").AppendLine()
            .Append("$(document).ready(function() {;").AppendLine()
            .Append("     var lg = window.navigator.userLanguage || window.navigator.language;").AppendLine()
            .Append("     if(typeof(lang) == \"undefined\") lg = \"fr\";").AppendLine()
            .Append("     lg = lg.split('-')[0].toLowerCase();").AppendLine();

            if (hasMemoField)
                sb.Append("var toolbarBasic =[['Font','FontSize', 'Bold', 'Italic','-', 'TextColor','BGColor', '-', 'JustifyLeft','JustifyCenter','JustifyRight','JustifyBlock' ,'-', 'NumberedList', 'BulletedList', '-', 'Link', 'Unlink' ]];").AppendLine();



            // On  ajoute l'initialisation de composant date
            if (hasDateField)
            {
                // #51942 - CNA - Remplacement du composant datepicker à utiliser
                if (datePickerType == DATEPICKER_TYP.JQUERYUI)
                {
                    sb.Append("     var dateOptions = ").Append(AppendClientDateOptions()).Append(";").AppendLine();
                    //sb.Append("     $.datetimepicker.setLocale(lg);").AppendLine();
                    sb.Append("     $.datepicker.setDefaults($.datepicker.regional[lg]);").AppendLine();
                }
                else if (datePickerType == DATEPICKER_TYP.EDATEPICKER)
                {
                    sb.Append("     eFormularPickADate.Init();").AppendLine();
                }
            }

            sb.Append(InnerRawScript).AppendLine()
            .Append("});").AppendLine();
            sb.Append("</script>");
            sb.Append(VueAppScript);
            return sb.ToString();
        }

        /// <summary>
        /// Ajout des fichiers de langues correspondant aux langues installées sur le navigateur clients
        /// </summary>
        public void AddClientLocalScripts()
        {

            string url = "formular/select2/select2_locale_@lang";

            //exemple  Accept-Language: fr-FR,   fr;q=0.8,  en-US;q=0.6,  en;q=0.4
            string[] langs = HttpContext.Current.Request.UserLanguages; //langues installées sur le navigateur clients
            List<string> addedLang = new List<string>();
            string lang = "fr";

            foreach (string language in langs)
            {
                //ex: ["fr-FR", "fr;q=0.8", "en-US;q=0.6", "en;q=0.4"]
                lang = language.Split('-')[0].Split(';')[0];

                // "en" est ajoutée par défaut, pas besoin de fichier langue
                if (lang == "" || lang == "en" || addedLang.Contains(lang))
                    continue;

                addedLang.Add(lang);
                ListScriptToRegister.Add(url.Replace("@lang", lang));
            }
        }

        protected bool IsCatalogEditable(eFieldRecord fld)
        {
            return IsCatalog(fld) && !IsCatalogLink(fld) && !IsCatalogBound(fld);
        }

        /// <summary>
        /// Retourne vrai si le champ est de type catalogue
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private bool IsCatalog(eFieldRecord fld)
        {
            return fld.FldInfo.Format == FieldFormat.TYP_CHAR && fld.FldInfo.Popup != PopupType.NONE;
        }

        /// <summary>
        /// Retourne vrai si le champ est de type catalogue de liaison
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private bool IsCatalogLink(eFieldRecord fld)
        {
            return IsCatalog(fld) && fld.FldInfo.Descid != fld.FldInfo.PopupDescId && (fld.FldInfo.PopupDescId % 100 == 1) && fld.FldInfo.Popup == PopupType.SPECIAL;
        }

        /// <summary>
        /// Retourne vrai si le champ est de type catalogue de liaison
        /// </summary>
        /// <param name="fld"></param>
        /// <returns></returns>
        private bool IsCatalogBound(eFieldRecord fld)
        {
            return IsCatalog(fld) && fld.FldInfo.BoundDescid > 0;
        }

        private string AppendClientDateOptions()
        {
            // #51942 - CNA - Remplacement du composant datepicker à utiliser
            // return "{format:'@format', step:1, dayOfWeekStart:1, timepicker:false}".Replace("@format", eLibConst.FormularDateFormat[Pref.CultureInfo]);
            return "{dateFormat:'@format', firstDay:1, showButtonPanel:true, changeMonth:true, changeYear:true}".Replace("@format", eLibConst.FormularDateFormat[Pref.CultureInfo]);
        }

        /// <summary>
        /// Ajout du format du champ si necessaire
        /// </summary>
        /// <param name="webCtrl">Champ auquel on souhaite ajouter le format</param>
        /// <param name="myField">information sur le champ</param>
        protected override void AddFormat(WebControl webCtrl, eFieldRecord myField)
        {
            webCtrl.Attributes.Add("frm", myField.FldInfo.Format.GetHashCode().ToString());
        }

        /// <summary>
        /// On fait un rendu différent  pour le formulaire
        /// </summary>
        /// <param name="row">enregistrement de la fiche invit/cibl etendu du form</param>
        /// <param name="fieldRow">champ de rendu</param>
        /// <param name="webControl">le control de rendu</param>
        /// <param name="themePaths"></param>
        /// <param name="sClassAction">class css a mettre a jour</param>
        /// <param name="nbCol">nombre de colonnes qu'occupera la cellule</param>
        /// <param name="nbRow">nombre de lignes qu'occupera la cellule</param>
        protected override void RenderImageFieldFormat(eRecord row, eFieldRecord fieldRow, WebControl webControl, ePref.CalculateDynamicTheme themePaths, ref string sClassAction,
            int nbCol = 1, int nbRow = 1)
        {
            Image oImg = new Image();
            string sImageUrl = "";

            switch (fieldRow.FldInfo.ImgStorage)
            {
                case EudoQuery.ImageStorage.STORE_IN_FILE:

                    if (fieldRow.RightIsUpdatable)
                        sClassAction = "LNKCATIMG";

                    if (fieldRow.Value.Length > 0)
                        sImageUrl = string.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, Pref.GetBaseName), "/", fieldRow.Value);

                    break;

                case EudoQuery.ImageStorage.STORE_IN_URL:

                    if (fieldRow.RightIsUpdatable)
                        sClassAction = "LNKOPENIMG";

                    if (fieldRow.Value.Length > 0)
                        sImageUrl = fieldRow.Value;

                    break;

                case EudoQuery.ImageStorage.STORE_IN_DATABASE:
                    // throw new Exception("Type d'image n'est pas supportée pour les formulaires : fieldRow.FldInfo.STORE_IN_DATABASE ");
                    break;
                default:
                    throw new Exception("Type d'image inconnu : fieldRow.FldInfo.ImgStorage = " + fieldRow.FldInfo.ImgStorage);

            }


            //Bouton X pour supprimer l image
            HtmlGenericControl btn = new HtmlGenericControl("button");
            btn.ID = "imgcloseId_" + fieldRow.FldInfo.Descid;
            btn.Attributes.Add("class", "btnClose close");

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerHtml = "x";

            if (fieldRow.Value.Length == 0 && fieldRow.RightIsUpdatable)
            {
                oImg.CssClass = "imgEmpty";
                oImg.Style.Add("border-width", "1px");
                webControl.CssClass += "imgContainer";

            }
            else
            {
                oImg.CssClass = "imgFill";

            }

            oImg.ImageUrl = sImageUrl;


            webControl.Style.Add("text-align", "center");
            webControl.Attributes.Add("fid", fieldRow.FileId.ToString());
            webControl.Attributes.Add("dbv", fieldRow.Value);

            //TODO Ajouter 
            // btn.Controls.Add(span);
            // webControl.Controls.Add(btn);
            webControl.Controls.Add(oImg);
        }

        /// <summary>
        /// Craétion d'un contrôle éditable
        /// </summary>
        /// <param name="fieldRow">eFieldRecord</param>
        /// <returns></returns>
        protected override EdnWebControl CreateEditEdnControl(eFieldRecord fieldRow)
        {
            if (IsCatalogEditable(fieldRow))
                return new EdnWebControl() { WebCtrl = new Label(), TypCtrl = EdnWebControl.WebControlType.LABEL };

            return base.CreateEditEdnControl(fieldRow);
        }

        /// <summary>
        /// On fait un rendu différent  du catalog surtout pour le formulaire
        /// </summary>
        /// <param name="row">enregistrement de la fiche invit/cibl etendu du form</param>
        /// <param name="fieldRow">champ de rendu</param>
        /// <param name="ednWebControl">le control de rendu</param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction">class css a mettre a jour</param>
        protected override Boolean RenderCharFieldFormat(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {

            if (IsCatalogEditable(fieldRow) && fieldRow.RightIsUpdatable)
                return this.RenderCatalogControl(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

            return base.RenderCharFieldFormat(row, fieldRow, ednWebControl, sbClass, ref sClassAction);

        }


        /// <summary>
        /// Affiche une checkbox sans le libellé
        /// </summary>
        /// <param name="rowRecord">Ligne de la liste a afficher</param>
        /// <param name="fieldRecord">Le champ binaire</param>
        /// <param name="sClassAction">classe CSS choisi pour l'element</param>
        /// <returns>Retourne le control généré pour la rubrique de type BIT (retourne un eCheckBoxCtrl)</returns>
        protected override WebControl RenderBitFieldFormat(eRecord rowRecord, eFieldRecord fieldRecord, ref string sClassAction)
        {
            fieldRecord.FldInfo.Libelle = string.Empty;
            return base.RenderBitFieldFormat(rowRecord, fieldRecord, ref sClassAction);
        }

        /// <summary>
        /// Fait un rendu
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldRow"></param>
        /// <param name="ednWebControl"></param>
        /// <param name="sbClass"></param>
        /// <param name="sClassAction"></param>
        /// <returns></returns>
        private Boolean RenderCatalogControl(eRecord row, eFieldRecord fieldRow, EdnWebControl ednWebControl, StringBuilder sbClass, ref string sClassAction)
        {
            WebControl webControl = ednWebControl.WebCtrl;
            webControl.Attributes.Add("dbv", fieldRow.Value);
            webControl.Attributes.Add("usrfrm", "1"); // pour que getfieldsinfo reconnait cet element

            HtmlGenericControl select = new HtmlGenericControl("select");
            select.ID = "select_" + fieldRow.FldInfo.Descid;
            select.Attributes.Add("refid", webControl.ID); //le span parent contenant les infos necessaire à l'engine

            if (fieldRow.FldInfo.Multiple)
                select.Attributes.Add("multiple", "true");

            select.Attributes.Add("class", "catalog-select");
            eCatalog cat = LoadCatalog(fieldRow);
            HtmlGenericControl option;

            //Si uniqument une liste non multiple, on ajoute le choix vide
            if (!fieldRow.FldInfo.Multiple)
            {
                option = new HtmlGenericControl("option");
                //option.InnerHtml = "--- " + eResApp.GetRes(Pref.LangId, 1173) + " ---";
                select.Controls.Add(option);

                if (string.IsNullOrEmpty(fieldRow.Value))
                    option.Attributes.Add("selected", "true");
            }

            foreach (eCatalog.CatalogValue cv in cat.Values)
            {
                option = new HtmlGenericControl("option");
                option.Attributes.Add("value", cv.DbValue);
                option.InnerHtml = cv.DisplayValue;
                select.Controls.Add(option);

                if (!string.IsNullOrEmpty(fieldRow.Value) && (";" + fieldRow.Value + ";").Contains(";" + cv.DbValue + ";"))
                    option.Attributes.Add("selected", "true");
            }

            webControl.Controls.Add(select);
            return true;
        }

        /// <summary>
        /// Charge les valeur de la rubrique du catalogue
        /// </summary>
        /// <param name="fieldRow"></param>
        /// <returns></returns>
        protected eCatalog LoadCatalog(eFieldRecord fieldRow)
        {
            eCatalog cat = null;
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            try
            {
                dal.OpenDatabase();

                if (fieldRow.FldInfo.PopupDescId != fieldRow.FldInfo.Descid)
                {
                    if (fieldRow.FldInfo.PopupDescId % 100 == 1 && fieldRow.FldInfo.Popup == PopupType.SPECIAL)
                    {
                        // Catalogue "utiliser les valeurs de la rubrique" - Liaisons
                        // TODO
                        cat = new eCatalog(dal, Pref, fieldRow.FldInfo.Popup, Pref.User, fieldRow.FldInfo.Descid);
                    }
                    else
                    {
                        // Catalogue "utiliser le catalogue de la rubrique"
                        cat = new eCatalog(dal, Pref, fieldRow.FldInfo.Popup, Pref.User, fieldRow.FldInfo.PopupDescId);
                    }
                }
                else
                {
                    cat = new eCatalog(dal, Pref, fieldRow.FldInfo.Popup, Pref.User, fieldRow.FldInfo.Descid);
                }
            }
            catch (Exception)
            {
                //TODO MOU
                throw;
            }
            finally
            {
                dal?.CloseDatabase();
            }

            return cat;
        }

        /// <summary>
        /// Rendu du Captcha, Bouton Valider et Annuler
        /// </summary>
        /// <returns></returns>
        protected string GetValidRender()
        {
            var ulForm = new eUlCtrl();
            ulForm.CssClass = "form_valid_ul";
            //Captcha que en mode anonyme (fileid à 0)
            //Id du template++ lié à ce formulaire(0=>mode non authentifié)
            if (FormFile.TplFileId <= 0)
            {
                #region Captcha
                var ul = new eUlCtrl();
                ul.CssClass = "form_Captcha_ul";
                ulForm.AddLi().Controls.Add(ul);

                var li = ul.AddLi();

                var testImg = li.AddControl<System.Web.UI.WebControls.Image>();
                testImg.ID = "ImgCapcha";
                testImg.ImageUrl = string.Concat("~/ecaptchaget.aspx?date=", DateTime.Now.Ticks);
                testImg.AlternateText = "Captcha";

                li = ul.AddLi();

                var div = li.AddControl<eDivCtrl>();
                div.ID = "captcha_lbl";
                div.InnerText = eResApp.GetRes(Pref, 6766);

                var tb = li.AddControl<TextBox>();
                tb.ID = "txt_captcha";
                tb.Attributes.Add("required", "");


                ulForm.AddLi().Controls.Add(new LiteralControl("&nbsp;"));

                //Reloaded du captcha
                var divNewCode = ulForm.AddLi().AddControl<eDivCtrl>();
                divNewCode.OnClick = "eUserForm.InitNewCode();return;";
                divNewCode.CssClass = "div_reload";
                var logo = divNewCode.AddControl<eDivCtrl>();
                logo.CssClass = "logo_reload";
                var txt = divNewCode.AddControl<eDivCtrl>();
                txt.CssClass = "txt_reload";
                txt.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6225)));
                #endregion
            }
            ulForm.AddLi().Controls.Add(new LiteralControl("&nbsp;"));

            var btnValidNew = new HtmlInputButton("SUBMIT");
            btnValidNew.Value = eResApp.GetRes(Pref.User.UserLangId, 5003);
            ulForm.AddLi().Controls.Add(btnValidNew);

            /*GCH : Bouton annulé retiré car on ne peut pas faire top.close, window.close... avec chrome et Firefox
            eButtonCtrl btnCancel = new eButtonCtrl(eResApp.GetRes(_prefLite.Lang, 29), eButtonCtrl.ButtonType.GRAY, "top.close();");
            btnList.Controls.Add(btnCancel);*/
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            ulForm.RenderControl(tw);
            return sb.ToString();
        }

        /// <summary>
        /// Création du bloc de la validation du formulaire. Il peut contenir aussi la partie captcha
        /// </summary>
        /// <param name="className"></param>
        /// <param name="Libelle"></param>
        /// <returns></returns>
        protected string GetValidFrmAdvRender(string className, string Libelle)
        {
            var ulForm = new eUlCtrl();
            ulForm.CssClass = "form_valid_ul";
            //on ajoute la partie Captcha pour les formulaires publiques
            if (FormFile.TplFileId <= 0)
            {
                HtmlGenericControl captchaElem = new HtmlGenericControl("div");
                captchaElem.ID = "reCaptchContainer";
                ulForm.AddLi().Controls.Add(captchaElem);
                HtmlGenericControl captchaErrorElem = new HtmlGenericControl("div");
                captchaErrorElem.ID = "g-recaptcha-error";
                ulForm.AddLi().Controls.Add(captchaErrorElem);
                ulForm.AddLi().Controls.Add(new LiteralControl("&nbsp;"));
            }

            var btnValidNew = new HtmlGenericControl("edn-btn");
            btnValidNew.InnerText = Libelle;

            //couleur de fond des bouttons
            if (!string.IsNullOrEmpty(FormFile.FormularExtendedParam.ButtonBackgroundColor))
                btnValidNew.Attributes.Add("style", string.Concat("background-color: ", FormFile.FormularExtendedParam.ButtonBackgroundColor.Substring(0, 7), "!important; border-color: ", FormFile.FormularExtendedParam.ButtonBackgroundColor.Substring(0, 7), "!important;"));

            btnValidNew.Attributes.Add("validation", "");
            //btnValidNew.Attributes.Add("type", "submit");
            btnValidNew.Attributes.Add("@click", "validateAndSubmitForm(false);");
            ulForm.AddLi().Controls.Add(btnValidNew);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            ulForm.RenderControl(tw);

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="Libelle"></param>
        /// <param name="edndPa"></param>
        /// <param name="edndTr"></param>
        /// <param name="edndPi"></param>
        /// <returns></returns>
        protected string GetWorldlinePaimentRender(string className, string Libelle, int edndPa, int edndTr, int edndPi)
        {
            List<WorldLineDescInfo> worldLineInfo = new List<WorldLineDescInfo>();
            //on ajoute les infos des desciD
            if (edndPa > 0)
            {
                FieldsDescId.Add(edndPa.ToString());
                AllowedFieldsDescId.AddContains(edndPa.ToString());
                worldLineInfo.Add(new WorldLineDescInfo
                {
                    DescId = edndPa,
                    WorldLineDescInfoType = WorldLineDescInfoType.PAYMENT_AMOUNT
                });
            }

            if (edndTr > 0)
            {
                FieldsDescId.Add(edndTr.ToString());
                AllowedFieldsDescId.AddContains(edndTr.ToString());
                worldLineInfo.Add(new WorldLineDescInfo
                {
                    DescId = edndTr,
                    WorldLineDescInfoType = WorldLineDescInfoType.TRANSACTION_REFERENCE
                });
            }

            if (edndPi > 0)
            {
                FieldsDescId.Add(edndPi.ToString());
                AllowedFieldsDescId.AddContains(edndPi.ToString());
                worldLineInfo.Add(new WorldLineDescInfo
                {
                    DescId = edndPi,
                    WorldLineDescInfoType = WorldLineDescInfoType.PAYMENT_INDICATOR
                });
            }

            this.FormFile.AllBtnWorldLineDescInfo.Add(new WorldLineBtnInfo
            {
                WorldLineDescInfoList = worldLineInfo,
                Libelle = Libelle
            });

            var ulForm = new eUlCtrl();
            ulForm.CssClass = "form_valid_ul";
            //on ajoute la partie Captcha pour les formulaires publiques
            if (FormFile.TplFileId <= 0)
            {
                HtmlGenericControl captchaElem = new HtmlGenericControl("div");
                captchaElem.ID = "reCaptchContainer";
                captchaElem.Attributes.Add("class", "reCaptchContainer");
                ulForm.AddLi().Controls.Add(captchaElem);
                HtmlGenericControl captchaErrorElem = new HtmlGenericControl("div");
                captchaErrorElem.ID = "g-recaptcha-error";
                ulForm.AddLi().Controls.Add(captchaErrorElem);
                ulForm.AddLi().Controls.Add(new LiteralControl("&nbsp;"));
            }

            Action setCanceledPayment = () =>
            {
                FormFile.isPaymentCancelled = true;
            };

            paymentResult = Task.Run(async () => await WLTransactionTools.VerifyTargetTransactionInfos(this.Pref, FormFile.Tab, FormFile.TplFileId, "", EngineContext.APPLI, setCanceledPayment)).Result;

            //recherche des transactions de cette fiche
            bool bAlreadyPaid = FormFile.TplFileId != 0 && WLTransactionTools.TargetHasPaidTransaction(this.Pref, FormFile.Tab, FormFile.TplFileId);


            var btnValidNew = new HtmlGenericControl("edn-btn");

            btnValidNew.InnerText = Libelle;
            btnValidNew.ID = String.Concat("btnValid", "_", edndPa, "_", edndTr, "_", edndPi);
            btnValidNew.Attributes.Add("class", "btnWorldlinePayment");
            btnValidNew.Attributes.Add("validation", "");
            //btnValidNew.Attributes.Add("type", "submit");

            if (bAlreadyPaid)
            {
                btnValidNew.Disabled = true;
                btnValidNew.Attributes.Add("@click", "");

                return "<div>TODO RES/DESIGN ALREADY PAID</div>";
            }
            else
                btnValidNew.Attributes.Add("@click", "validateAndSubmitForm(true,{'edndPa': " + edndPa + ", 'edndTr':" + edndTr + ", 'edndPi': " + edndPi + "});");

            ulForm.AddLi().Controls.Add(btnValidNew);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            ulForm.RenderControl(tw);

            return sb.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="nDescId"></param>
        /// <param name="isUpdatable"></param>
        /// <returns></returns>
        protected virtual string FieldRender(Int32 nDescId, Boolean isUpdatable)
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

            WebControl labelCell = new Label();
            GetFieldLabelCell(labelCell, row, new List<eFieldRecord> { fld });   //Ajout du libellé
            labelCell.Style.Add(HtmlTextWriterStyle.Display, "none");  //le libellé doit-être dans la page mais masqué

            WebControl valueCell = GetFieldValueCell(row, new List<eFieldRecord> { fld }, 0, Pref, ThemePaths);

            if (fld.RightIsUpdatable && fld.FldInfo.Format == FieldFormat.TYP_MEMO && !fld.FldInfo.IsHtml)
            {
                valueCell.Attributes.Add("onblur", string.Format("eUsrFld.OB(event, '{0}');", valueCell.ID));
                valueCell.Attributes.Add("onclick", string.Format("eUsrFld.OC(event, '{0}');", valueCell.ID));
                valueCell.Attributes.Add("onkeyup", string.Format("eUsrFld.OKU(event, '{0}');", valueCell.ID));
                //  valueCell.Attributes.Add("onselect", string.Format("eUsrFld.OS(event, '{0}');", valueCell.ID));
            }

            //Ajout des descid modifiable uniquement, pour le champ mémo la lecture
            if (isUpdatable)
            {
                FieldsDescId.Add(nDescId.ToString());
                AllowedFieldsDescId.AddContains(nDescId.ToString());

                if (fld.FldInfo.Format == FieldFormat.TYP_DATE)
                {
                    hasDateField = true;

                    // #51942 - CNA - Remplacement du composant datepicker à utiliser
                    if (datePickerType == DATEPICKER_TYP.JQUERYUI)
                    {
                        //InnerRawScript.Append(" $('#").Append(valueCell.ID).Append("').datetimepicker(dateOptions);").AppendLine();
                        InnerRawScript.Append(" $('#").Append(valueCell.ID).Append("').datepicker(dateOptions);").AppendLine();
                    }
                }
                else if (IsCatalogEditable(fld))
                {
                    hasCatalogField = true;
                    string selectId = "select_" + fld.FldInfo.Descid;
                    InnerRawScript.Append(" $('#").Append(selectId).Append("').select2(").Append(GetSelect2Options(fld)).Append(");").AppendLine();
                    InnerRawScript.Append(" $('#").Append(selectId).Append("').on(\"change\", oCatalog.change);").AppendLine();
                }
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
                else
                {
                    // Pour les champs mémo html, on instancie un CKEditor pour chaque mémo
                    hasMemoField = true;
                    if (!fld.RightIsUpdatable || !isUpdatable)
                        InnerRawScript.Append(" CKEDITOR.inline( '").Append(valueCell.ID).Append("');");
                    else
                    {
                        InnerRawScript.Append(" CKEDITOR.replace( '").Append(valueCell.ID).Append("', { readOnly:").Append(isUpdatable ? "false" : "true")
                            .Append(", toolbar:toolbarBasic,  removePlugins:'elementspath', skin:'eudonet,skins/eudonet/',resize_enabled:false } ); ").AppendLine();
                        // A chaque modification du ckeditor on mét à jour son textarea pour que getFieldsInfos recupère le contenu.
                        // La config.autoUpdateElement=true met a jour le textarea au moment du poste, c'est déjà trop tard pour la fonction getFieldsInfos
                        InnerRawScript.Append(" CKEDITOR.instances['").Append(valueCell.ID).Append("'].on('change', function(evt){ evt.editor.updateElement(); })").AppendLine();
                    }

                }
            }
            #endregion

            Boolean bDisplayBtn = isUpdatable;
            Label buttonCell = new Label();
            GetBaseButtonCell(buttonCell, valueCell, bDisplayBtn);
            buttonCell.Attributes.Add("onclick", string.Format("eUsrFld.OC(event, '{0}');", valueCell.ID));

            parentContainer.Controls.Add(labelCell);
            parentContainer.Controls.Add(valueCell);
            parentContainer.Controls.Add(buttonCell);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            parentContainer.RenderControl(tw);

            return sb.ToString();
        }

        /// <summary>
        /// </summary>
        /// <param name="advancedFormularParam"></param>
        /// <returns></returns>
        protected virtual string FieldRenderAdv(AdvancedFormularParam advancedFormularParam)
        {
            eFieldRecord fld = null;
            if (dicFlds.ContainsKey(advancedFormularParam.DescId))
                fld = dicFlds[advancedFormularParam.DescId];
            if (row == null || fld == null)
                return string.Empty;

            if (advancedFormularParam.IsMandatory && !fld.IsMandatory)
                fld.IsMandatory = true;

            string labelResult = HttpUtility.HtmlDecode(eMergeTools.GetContentMerge_Formular(advancedFormularParam.LabelData, fbp, advancedFormularParam.IsMandatory || fld.IsMandatory, advancedFormularParam.IsCheckBox));

            //si en mode création, la case à cocher est coché par défaut et que ce n'est pas fait coté admin, on rajoute cette info
            if (advancedFormularParam.IsChecked && fld.FldInfo.Format == FieldFormat.TYP_BIT
                && (string.IsNullOrEmpty(fld.DisplayValue) || fld.DisplayValue == "0"))
                fld.DisplayValue = "1";

            fld.DisplayValue = HtmlTools.StripHtml(fld.DisplayValue);

            // Si le champs est en lecture seule alors pas de droit de modification
            fld.RightIsUpdatable = !fld.FldInfo.ReadOnly;

            HtmlGenericControl parentContainer = new HtmlGenericControl("div");
            parentContainer.Attributes.Add("class", advancedFormularParam.ClassName);

            HtmlGenericControl labelCell = new HtmlGenericControl("div");
            labelCell.InnerHtml = labelResult;

            WebControl labelHidenCell = new Label();
            GetFieldLabelCell(labelHidenCell, row, new List<eFieldRecord> { fld });   //Ajout du libellé
            labelHidenCell.Style.Add(HtmlTextWriterStyle.Display, "none");  //le libellé doit-être dans la page mais masqué

            WebControl valueCell = GetFieldValueCell(row, new List<eFieldRecord> { fld }, 0, Pref, ThemePaths);

            //Ajout des descid modifiable uniquement, pour le champ mémo la lecture
            FieldsDescId.Add(advancedFormularParam.DescId.ToString());
            AllowedFieldsDescId.AddContains(advancedFormularParam.DescId.ToString());

            if (fld.FldInfo.Format == FieldFormat.TYP_DATE)
            {
                hasDateField = true;

                // #51942 - CNA - Remplacement du composant datepicker à utiliser
                if (datePickerType == DATEPICKER_TYP.JQUERYUI)
                {
                    //InnerRawScript.Append(" $('#").Append(valueCell.ID).Append("').datetimepicker(dateOptions);").AppendLine();
                    InnerRawScript.Append(" $('#").Append(valueCell.ID).Append("').datepicker(dateOptions);").AppendLine();
                }
            }
            else if (IsCatalogEditable(fld))
            {
                hasCatalogField = true;
                string selectId = "select_" + fld.FldInfo.Descid;
                InnerRawScript.Append(" $('#").Append(selectId).Append("').select2(").Append(GetSelect2Options(fld)).Append(");").AppendLine();
                InnerRawScript.Append(" $('#").Append(selectId).Append("').on(\"change\", oCatalog.change);").AppendLine();
            }
            else if (!advancedFormularParam.IsCheckBox)
            {
                valueCell.CssClass = advancedFormularParam.InputClass;
                if (!string.IsNullOrEmpty(advancedFormularParam.Placeholder))
                    valueCell.Attributes.Add("placeholder", advancedFormularParam.Placeholder);
            }



            Boolean bDisplayBtn = true;
            Label buttonCell = new Label();
            GetBaseButtonCell(buttonCell, valueCell, bDisplayBtn);
            buttonCell.Attributes.Add("onclick", string.Format("eUsrFld.OC(event, '{0}');", valueCell.ID));
            //on ajoute les composants du rendu champ de saisie
            parentContainer.Controls.Add(labelHidenCell);
            if (advancedFormularParam.IsCheckBox)
            {
                parentContainer.Controls.Add(valueCell);
                parentContainer.Controls.Add(labelCell);
            }
            else
            {
                parentContainer.Controls.Add(labelCell);
                parentContainer.Controls.Add(valueCell);
            }

            //V1: on vire les bouttons mail, tél
            //parentContainer.Controls.Add(buttonCell);

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            HtmlTextWriter tw = new HtmlTextWriter(sw);
            parentContainer.RenderControl(tw);

            return sb.ToString();
        }

        /// <summary>
        /// Chargement de la liste des fichiers CSS necessaires
        /// </summary>
        private void InitCssFile()
        {

            //compasant date et select2, css se trouve dans les répertoires js
            ListCssWithPathToRegister.Add("scripts/formular/formular");

            // Si on a au moins un champs modifiable de type catalogue on inclut les scripts
            if (hasCatalogField)
            {
                ListCssWithPathToRegister.Add("scripts/formular/select2/select2");
                ListCssWithPathToRegister.Add("scripts/formular/select2/select2-bootstrap");
            }

            // Si on a au moins un champs modifiable de type catalogue on inclut les scripts
            if (hasDateField)
            {
                // #51942 - CNA - Remplacement du composant datepicker à utiliser
                if (datePickerType == DATEPICKER_TYP.JQUERYUI)
                {
                    //ListCssWithPathToRegister.Add("scripts/formular/datetimepicker-master/jquery.datetimepicker");
                    ListCssWithPathToRegister.Add("scripts/formular/jquery-ui/jquery-ui.min");
                }
            }

            ListCssToRegister.Add("eControl");
            ListCssToRegister.Add("eTitle");
            ListCssToRegister.Add("eMain");
            ListCssToRegister.Add("eButtons");
            ListCssToRegister.Add("eFormular");
            ListCssToRegister.Add("eModalDialog");
        }

        /// <summary>
        /// Chargement de la liste des fichiers JS necessaires
        /// </summary>
        private void InitScriptFile()
        {
            ListScriptToRegister.Add("formular/jquery-1.11.3.min");

            // Si on a au moins un champs modifiable de type catalogue on inclut les scripts
            if (hasCatalogField)
            {
                ListScriptToRegister.Add("formular/select2/select2.min");
                AddClientLocalScripts();
            }

            if (hasMemoField)
                ListScriptToRegister.Add("ckeditor/ckeditor");

            // Si on a au moins un champs modifiable de type date on inclut les scripts
            if (hasDateField)
            {
                // #51942 - CNA - Remplacement du composant datepicker à utiliser
                if (datePickerType == DATEPICKER_TYP.JQUERYUI)
                {
                    //ListScriptToRegister.Add("formular/datetimepicker-master/build/jquery.datetimepicker.full.min");
                    ListScriptToRegister.Add("formular/jquery-ui/jquery-ui.min");
                    foreach (string lang in eLibConst.FormularDateListLang)
                    {
                        ListScriptToRegister.Add(string.Concat("formular/jquery-ui/i18n/datepicker-", lang));
                    }
                }
                else if (datePickerType == DATEPICKER_TYP.EDATEPICKER)
                {
                    ListScriptToRegister.Add("ePickADate");
                    ListScriptToRegister.Add("formular/eFormularPickADate");
                }
            }

            if (this.FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED && this.FormFile.Version == FormularVersion.ADV_V2)
            {
                //ListScriptToRegister.Add("eModalDialog");
                //ListScriptToRegister.Add("eEngine");
                ListScriptToRegister.Add("eUpdater");
                ListScriptToRegister.Add("eUserFormAdv");
            }
            else
            {
                ListScriptToRegister.Add("eTools");
                ListScriptToRegister.Add("eUpdater");
                ListScriptToRegister.Add("eEngine");
                ListScriptToRegister.Add("eMain");
                ListScriptToRegister.Add("eModalDialog");
                ListScriptToRegister.Add("eUserForm");
            }

        }

        private void GenerateScript()
        {
            List<int> ResList = new List<int> { 28, 29, 30, 143, 314, 468, 712, 767, 821, 1216, 2021, 2391, 2657, 2776, 5003, 5017, 6275, 6377, 6564 }; // Ordonnez svp !
            RawScript.AppendLine()
                .Append(" function LoadRes(){").AppendLine();
            foreach (int resId in ResList)
                RawScript.Append("   _res_").Append(resId).Append(" = \"").Append(HttpUtility.JavaScriptStringEncode(eResApp.GetRes(Pref, resId))).Append("\";").AppendLine();


            RawScript.Append("}").AppendLine();
        }

        /// <summary>
        /// Ajoute des infos sur le contexte du formulaire (tab, descids, re) sous forme d'inputs cachées
        /// necessaire si on veux faire des vérification sur les champs obligatoires 
        /// </summary>
        /// <param name="pageConserveInfo">informations a conserver de la page d'origine</param>
        public IEnumerable<Control> GetClientContextInfo(IEnumerable<KeyValuePair<string, string>> pageConserveInfo)
        {
            var listCtrl = new HashSet<Control>();

            //Pour que la fonction "getFieldsInfos(nTab, nFileId)" puisse récupérer les rubriques de invit/cible 
            //a fin de faire des vérification   
            HtmlGenericControl hidden;

            if (pageConserveInfo != null)
            {
                foreach (KeyValuePair<string, string> keyValue in pageConserveInfo)
                {
                    hidden = new HtmlGenericControl("input");
                    hidden.ID = keyValue.Key;
                    hidden.Attributes.Add("name", keyValue.Key);
                    hidden.Attributes.Add("value", keyValue.Value);
                    hidden.Attributes.Add("type", "hidden");
                    listCtrl.Add(hidden);
                }
            }

            hidden = new HtmlGenericControl("input");
            hidden.ID = "fieldsId_" + FormFile.CalledTabDescId;
            hidden.Attributes.Add("name", "fields");
            hidden.Attributes.Add("value", string.Join(";", FieldsDescId));
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            //Tab
            hidden = new HtmlGenericControl("input");
            hidden.ID = "tab";
            hidden.Attributes.Add("name", "tab");
            hidden.Attributes.Add("value", FormFile.CalledTabDescId.ToString());
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            //IsPostback 
            hidden = new HtmlGenericControl("input");
            hidden.ID = "re";
            hidden.Attributes.Add("name", "re");
            hidden.Attributes.Add("value", "0");
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            //IsSubmit 
            hidden = new HtmlGenericControl("input");
            hidden.ID = "sub";
            hidden.Attributes.Add("name", "re");
            hidden.Attributes.Add("value", "0");
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            //on ajoute le type du formulaire dans le renderer 
            hidden = new HtmlGenericControl("input");
            hidden.ID = "frmType";
            hidden.Attributes.Add("name", "frmType");
            hidden.Attributes.Add("value", FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED ? "1" : "0");
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            //id de la fiche Invitation.++ 
            hidden = new HtmlGenericControl("input");
            hidden.ID = "fileId";
            hidden.Attributes.Add("name", "fileId");
            hidden.Attributes.Add("value", FormFile.TplFileId.ToString());
            hidden.Attributes.Add("type", "hidden");
            listCtrl.Add(hidden);

            return listCtrl;
        }


        /// <summary>
        /// Construit des objets html annexes/place des appel JS d'apres chargement
        /// </summary>
        /// <returns></returns>
        protected override Boolean End()
        {
            InitCssFile();
            InitScriptFile();
            GenerateScript();
            if (this.FormFile != null && this.FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED && this.FormFile.AllBtnWorldLineDescInfo.Count() > 0)
                UpdateTransctionTable();
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateTransctionTable()
        {
            Dictionary<string, string> dicTranError = new Dictionary<string, string>();
            eFile objFile = eFileLite.CreateFileLite(FormFile.Pref, FormFile.EvtTabId, FormFile.EvtFileId);
            string campaignName = objFile.Record.MainFileLabel;
            if (FormFile != null )
            {
                if (this.FormFile.AllBtnWorldLineDescInfo.Count() > 0 && this.FormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                {
                    if (string.IsNullOrEmpty(_ePref.DatabaseUid) && !string.IsNullOrEmpty(_uid))
                        _ePref.DatabaseUid = _uid;

                    if (FormFile.TranDescId > 0)
                    {
                        var _fieldRecord = FormFile.GetRecordFieldFromDesc(FormFile.TranDescId);
                        if (_fieldRecord != null)
                        {
                            //si il nya pas une transaction 
                            ResultTransction resTrans = WLTransactionTools.GetTransactionByOrderId(Pref, _fieldRecord.Value);
                            if (resTrans.resErreur != 0)
                            {
                                sendEMail(_ePref, _ePref.UserId, 8825, resTrans.resErreur, _fieldRecord.Value, campaignName);
                                return;
                            }
                        }
                    }

                    int fileId = FormFile.TplFileId;
                    int descId = FormFile.Tab;
                    if (fileId != 0 && descId != 0)
                    {
                        List<int> lstPaidSub = new List<int>()
                    {

                        (int)PaymentSubStatus.PENDING_CAPTURE,
                         (int)PaymentSubStatus.CAPTURED,
                          (int)PaymentSubStatus.CAPTURE_REQUESTED,
                    };

                       
                        var resTran = WLTransactionTools.ListTransactionTargetRefIds(Pref, FormFile.Tab, FormFile.TplFileId, lstPaidSub);
                        foreach (var refTransaction in resTran.Keys)
                        {
                            var tranRes = resTran[refTransaction];

                            #region vérifier si la transaction payée correspond à la dernière page de paiement validéé
                            if (FormFile.TranDescId > 0)
                            {
                                var _fieldRecord = FormFile.GetRecordFieldFromDesc(FormFile.TranDescId);
                                if (_fieldRecord != null && _fieldRecord.Value != tranRes.PaymentRefEudo)
                                {

                                    eFile file = eFileLite.CreateFileLite(FormFile.Pref, FormFile.Tab, FormFile.TplFileId);
                                    string fileName = file.Record.MainFileLabel;

                                    isConflictBetweenTransaction = true;
                                    conflictBetweenTransactionMessage = eResApp.GetRes(this.Pref.LangId, 8836).Replace("{sRefEudo}", tranRes.PaymentRefEudo);
                                    sendEMail(_ePref, _ePref.UserId, 8825, 8834, tranRes.PaymentRefEudo, fileName);
                                    continue;
                                }
                            }

                            #endregion
                            //en cas de conflit on met pas à jour la table transaction
                            if (isConflictBetweenTransaction)
                                return;
                            Engine.Engine engTransaction = eModelTools.GetEngine(FormFile.Pref, (int)TableType.PAYMENTTRANSACTION, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                            Engine.Engine engTarget = eModelTools.GetEngine(FormFile.Pref, (int)descId, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                            ResultTransction res = WLTransactionTools.UpdateTransactionAfterPayment(tranRes.TranId, FormFile.TplFileId, _ePref, engTransaction);

                            if (res != null && !string.IsNullOrEmpty(res.Erreur))
                            {
                                if (res.resErreur != 0)
                                    sendEMail(_ePref, _ePref.UserId, 8825, res.resErreur, res.sCheckHostedId, campaignName);
                                throw new Exception(res.Erreur);
                            }
                            else if (res != null)
                            {
                                if (FormFile.CheckIfPaymentIsAlreadyDone(res.Indicateur)  && paymentResult.Count > 0)
                                {
                                   sendEMail(_ePref, _ePref.UserId, 8825, 8826, res.sCheckHostedId, campaignName);
                                }
                                else
                                {
                                    ResultTransction restarger = WLTransactionTools.UpdateTargetTable(engTarget, res);
                                    if (String.IsNullOrEmpty(restarger.Erreur))
                                        this.FormFile.isAlreadyPaid = true;
                                }
                            }

                        }

                    }
                }
            }
        }

        /// <summary>
        /// send email
        /// </summary>
        /// <param name="pSQL"></param>
        /// <param name="userId"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="orderID"></param>
        /// <param name="campaignName"></param>
        /// <param name="bSendToAdmin"></param>
        public static void sendEMail(ePrefSQL pSQL, int userId, int subject, int body, string orderID, string campaignName, bool bSendToAdmin =  true)
        {
            #region Send Error to User
            try
            {
                eMailSmtpOptions mSmtpOpt = eMailSmtpOptions.GetEmptyConfig(pSQL);
                string strSubject = string.Empty, mailBody = string.Empty, to, error, lang, baseName;

                eudoDAL dalEudo = eLibTools.GetEudoDAL(pSQL);

                DataTableReaderTuned dtr = null;

                try
                {
                    dalEudo.OpenDatabase();

                    // Requête SQL
                    StringBuilder sbQuery = new StringBuilder()
                        .Append("SELECT [UserId], [UserLogin], [UserMail], [Lang] ")
                        .Append(" FROM [User] ")
                        .Append(" WHERE [User].[UserId] = @UserId");

                    RqParam rqQuery = new RqParam();
                    rqQuery.SetQuery(sbQuery.ToString());

                    rqQuery.AddInputParameter("@UserId", System.Data.SqlDbType.Int, userId);

                    // Execute la commande SQL
                    dtr = dalEudo.Execute(rqQuery, out error);

                    // Mauvais Login
                    if (dtr == null || !dtr.Read())
                    {
                        throw new EudoException("erreur lors de récupération d'utilisateur: ", error);
                    }

                    to = dtr.GetString("UserMail");
                    lang = dtr.GetString("Lang");
                }
                catch (Exception ex)
                {
                    throw new EudoException("erreur lors d'envoi d'email", innerExcp: ex);
                }

                #region EudoLog
                //Recherche dans eudolog du dbuid
                string sSQL = String.Concat("SELECT [BaseName]   ",
                        " FROM [DATABASES] ",
                        " WHERE [UID] = @UID ");

                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@UID", System.Data.SqlDbType.VarChar, pSQL.DatabaseUid);

                ePrefSQL pEudoLog = eLoginTools.GetPrefSQL("EUDOLOG", true);

                using (eudoDAL dal = eLibTools.GetEudoDAL(pEudoLog))
                {
                    try
                    {
                        dal.OpenDatabase();
                        using (DataTableReaderTuned dtrLog = dal.Execute(rq, out error))
                        {
                            if (error.Length > 0)
                                throw dal.InnerException ?? new Exception(error);

                            if (dtrLog == null || !dtrLog.HasRows || !dtrLog.Read())
                                throw new TokenInvalidException("Base non trouvée");


                            baseName = dtrLog.GetString("BaseName");
                        }
                    }
                    finally
                    {
                        dal.CloseDatabase();
                    }
                }
                #endregion

                if (string.IsNullOrEmpty(baseName))
                    baseName = pSQL.GetBaseName;
                int langId = EudoHelpers.GetLangId(lang);
                strSubject = eResApp.GetRes(langId, subject);
                if (body == 8826)
                    mailBody = string.Format(eResApp.GetRes(langId, body), orderID, campaignName, baseName);
                else if (body == 8827)
                    mailBody = string.Format(eResApp.GetRes(langId, body), orderID, baseName);
                else if (body == 8834)
                    mailBody = eResApp.GetRes(langId, body).Replace("{sRefEudo}", orderID).Replace("{sFileName}", campaignName).Replace("{sBaseName}", baseName);

                mailBody = string.Concat("<html><head></head><body><div style='font-size:9pt;font-family:tahoma'>", mailBody, "</div></body>");

                if (String.IsNullOrEmpty(mSmtpOpt.MailSmtp.Host))
                    throw new Exception("Host du server SMTP vide.");

                MailMessage oMail = new MailMessage();
                string sBcc = string.Empty;
                //envoi mail a admin de la base
                List<string> lstFrom = new List<string>();
                eWorldlinePaymentSetting worldlineSettings = eLibTools.GetSerializedWorldlineSettingsExtension(pSQL);
                if (worldlineSettings != null && !string.IsNullOrEmpty(worldlineSettings.LstMailAlert))
                {
                    to = worldlineSettings.LstMailAlert;
                    foreach (var mail in worldlineSettings.LstMailAlert.Split(';'))
                    {
                        string sMailLower = mail.ToLower();

                        if (!lstFrom.Contains(sMailLower) && eLibTools.IsEmailAddressValid(sMailLower))
                            lstFrom.Add(sMailLower);
                    }
                }

                if(bSendToAdmin)
                {
                    sBcc = "dev@eudonet.com";

                    if (string.IsNullOrEmpty(to))
                    {
                        to = "dev@eudonet.com";
                        sBcc = "";
                    }

                    if (lstFrom.Contains("dev@eudonet.com"))
                        sBcc = "";
                }

                try
                {
                    oMail = mSmtpOpt.InitMailMessage("EudoReport [reportnotifier@eudonet.com]", to);

                    oMail.IsBodyHtml = true;
                    oMail.Priority = System.Net.Mail.MailPriority.High;
                    oMail.Body = mailBody;
                    oMail.Subject = strSubject;
                    MailAddress sFrom = new MailAddress("noreply@eudonet.com");
                    oMail.From = sFrom;
                    if (!string.IsNullOrEmpty(sBcc))
                        oMail.Bcc.Add(sBcc);

                    try
                    {
                        mSmtpOpt.SendMail();
                    }
                    catch
                    {
                       //c'est un mail informatif , il faut pas bloquer le traitement de paiement en ligne
                    }                     
                          
                }
                finally
                {
                    oMail.Dispose();
                }

            }
            catch
            {
                //c'est un mail informatif , il faut pas bloquer le traitement de paiement en ligne
            }
            #endregion
        }

        /// <summary>Récupérer la valeur de la rubrique à partir de descId       
        /// <param name="decId)">descId de la rubrique</param>
        protected string GetDisplayValueFromDesc(int decId)
        {
            if (this.FormFile.FormularType == FORMULAR_TYPE.TYP_CLASSIC)//On désactive le lien pour les formulaires classiques coté back
                return "";
            eFieldRecord fieldRecord;
            if (dicFlds.TryGetValue(decId, out fieldRecord))
                return fieldRecord.DisplayValue;
            return "";
        }


        /// <summary>
        /// Récupération du rendu de la prévisualisation
        /// </summary>
        /// <param name="sAppUrl"></param>
        /// <param name="formular"></param>
        /// <returns></returns>
        public static eFormularFileRenderer LoadPreview(String sAppUrl, eFormular formular)
        {
            formular.AnalyseAndSerializeFields();
            formular.InitFirstFileId();
            formular.Init();
            formular.AlreadyInit = true;
            var rend = eRendererFactory.CreateFormularFileRenderer(formular.Pref.DatabaseUid, sAppUrl, formular.Pref, formular);

            if (rend.InnerException != null)
                throw rend.InnerException;

            if (rend.ErrorMsg.Length > 0)
                throw new Exception(rend.ErrorMsg);
            return rend;
        }


    }
}