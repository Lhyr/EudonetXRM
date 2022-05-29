using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;
using System.Configuration;
using Com.Eudonet.Internal.Payment;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de trakcing de l'application Eudonet
    /// Particularité : cette page est appelé depuiss l'exterieur sans authentification !
    /// </summary>
    public partial class frm : eExternalPage<LoadQueryStringForm>
    {
        /// <summary> infos sur le formulaire </summary>
        private eFormularFile _eFileForm = null;

        /// <summary>
        /// si c'est l'url contient des infos du paiement en ligne
        /// </summary>
        private bool isCallFromOnlinePayment = false;

        /// <summary>
        /// Formulaire avancé
        /// </summary>
        public bool FormAdv2
        {
            get
            {
                return _eFileForm != null && _eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED && _eFileForm.Version == FormularVersion.ADV_V2;
            }
        }

        /// <summary>
        /// Si le formulaire est publique ou pas
        /// </summary>
        public bool IsPublic
        {
            get
            {
                return _eFileForm != null && _eFileForm.TplFileId <= 0;
            }
        }

        /// <summary> Savoir si on a posté le formulaire </summary>
        private bool _isPostBack = false;
        eFormularFileRenderer rend = null;



        /// <summary> Clé du captcha </summary>
        public String CaptchaSiteKey
        {
            get
            {
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("CaptchaSiteKey")))
                    return ConfigurationManager.AppSettings.Get("CaptchaSiteKey");
                return string.Empty;
            }
        }
        /// <summary> Langue du captcha. C'est la langue du browser </summary>
        public String CaptchaFormularLanguage
        {
            get
            {
                string lang = "fr";
                if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length > 0)
                {
                    var browserLanguage = HttpContext.Current.Request.UserLanguages[0].Split('-')[0];
                    if (browserLanguage != null && browserLanguage.Length > 0)
                        lang = browserLanguage.Split(';')[0];
                }
                return lang;
            }
        }
        /// <summary>
        /// Type d'external page
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp { get { return eExternal.ExternalPageType.FORMULAR; } }

        /// <summary>
        /// Mis à jour la variable eIsPostBack pour savoir si la page est en postback
        /// </summary>
        protected override void OnPreInit(EventArgs e)
        {
            // HLA - Deplacé dans ProcessPage
            //_isPostBack = Request.Form != null && Request.Form["re"] != null && Request.Form["re"].Equals("1");
            base.OnPreInit(e);
        }

        /// <summary>
        /// Récupère les donnée du fomulaire depuis la session
        /// </summary>
        protected void Page_PreInit(object sender, EventArgs e)
        {
            //if (Session["formular"] != null)
            //  _dataForm = Session["formular"] as FormularLite;
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessPage()
        {
            #region ajout des css

            PageRegisters.AddCss("eFormular");

            #endregion

            try
            {
                ExtPgTrace("Debut frm.aspx");

                _isPostBack = _requestTools.GetRequestFormKeyB("re") ?? false;

                if (!_isPostBack)
                    GenerateFormular(Response);
            }
            catch (FormularRendExp exp)
            {
                Exception origExp = exp.InnerException;
                string expMsg = eLibTools.GetExceptionMsg(origExp, 3);
                _anError = true;
                _msgFeedback.Add(expMsg);
                ExtPgTrace(expMsg);
            }
        }

        /// <summary>
        /// Fait le rendu du formulaire
        /// </summary>
        private void GenerateFormular(HttpResponse response)
        {
            RendType = ExternalPageRendType.FORM_VISU;

            try
            {
                string baseUrl = eLibTools.GetAppUrl(Request);
                isCallFromOnlinePayment = DataParam.ParamData.TranDescId >= 0;
                rend = eRendererFactory.CreateFormularFileRenderer(
                    DataParam.CsData.UID, baseUrl, _pref, DataParam.CsData.FormularId, DataParam.ParamData.ParentFileId, DataParam.ParamData.TplFileId, DataParam.ParamData.TranDescId);

                // Remarque : toute exception de type FormularExp est gérée par eExternalPage.aspx.cs
                if (rend.InnerException != null)
                    throw rend.InnerException;

                if (rend.ErrorMsg.Length > 0)
                    throw new Exception(rend.ErrorMsg);
            }
            catch (Exception exp)
            {
                // On recentre toute les erreurs lié à la construction du rendu
                throw new FormularRendExp(exp);
            }

            _eFileForm = rend.FormFile;

            //demande #84 702
            if (_eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED)
            {
                if (Request.Browser.Browser.ToUpper().IndexOf("IE") >= 0
                                       || Request.Browser.Browser.ToLower().IndexOf("internetexplorer") >= 0
                    )
                {
                    int idLng = EudoCommonHelper.EudoHelpers.GetLangIdFromRequestCulture();
                    //IE pas compatible
                    Response.Redirect("badBrowser.aspx?lng=" + idLng, true);
                    return;
                }

                PageRegisters.AddCss("eFormularAdv");
                PageRegisters.AddCss("eMain");
                PageRegisters.AddCss("../../../IRISBlack/Front/Assets/CSS/advForm/googleFonts");
                PageRegisters.AddCss("../../../IRISBlack/Front/Scripts/libraries/vuetify/vuetify.min");
                PageRegisters.AddCss("../../../IRISBlack/Front/Assets/CSS/font-awesome/css/all");

                //Injection des CSS des fonts du nouvel éditeur des formulaires dans le rendu des formulaires
                PageRegisters.AddCss("../../../IRISBlack/Front/Assets/CSS/advForm/advForm");
            }

            if (CheckFormularExpirationAvailability())
            {
                if (rend.FormFile.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED)
                    AddMetaSocialNetworks(rend);
                return;
            }

            //Teste la soumission unique et date d'expiration
            if (CheckUniqueSubmission())
                return;

            //Modifier le title de la page
            if (rend.FormFile.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED && !string.IsNullOrEmpty(rend.FormFile.MetaTitle))
                PageTitle = rend.FormFile.MetaTitle;
            else
                PageTitle = rend.FormFile.Label;

            if(rend.FormFile.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED && rend.FormFile.AllBtnWorldLineDescInfo.Count > 0)
            {
                if(rend.IsConflictBetweenTransaction)
                {
                    if (_eFileForm.SubmissionRedirectUrl.Length > 0)
                    {
                        response.Redirect(_eFileForm.SubmissionRedirectUrl, true);
                        return;
                    }
                    else
                    {
                        FormPanel.Controls.Add(TemplateControl.ParseControl("<div class=\"conflictTr warning\">" + rend.ConflictBetweenTransactionMessage + "</div>"));
                        PageRegisters.SetRawCss(rend.BodySubmissionCss);
                        FormPanel.Controls.Add(TemplateControl.ParseControl(rend.BodySubmissionMerge));
                        return;
                    }
                }
                else
                {
                    bool bAlreadyPaid = rend.FormFile.TplFileId != 0 && WLTransactionTools.TargetHasPaidTransaction(_pref, rend.FormFile.Tab, rend.FormFile.TplFileId);
                    if (_eFileForm.isAlreadyPaid || bAlreadyPaid || rend.FormFile.checkIfOneOfPaymentIndicatorIsCheked)
                    {
                        if (_eFileForm.SubmissionRedirectUrl.Length > 0)
                        {
                            response.Redirect(_eFileForm.SubmissionRedirectUrl, true);
                            return;
                        }
                        else
                        {
                            PageRegisters.SetRawCss(rend.BodySubmissionCss);
                            FormPanel.Controls.Add(TemplateControl.ParseControl(rend.BodySubmissionMerge));
                            string paymentMessage = (isCallFromOnlinePayment ? rend.MsgPaymentIsDone : rend.MsgPaymentAlreadyDone);
                            FormPanel.Controls.Add(TemplateControl.ParseControl("<div id=\"snackbar\" class=\"messageSuccess\"><i id=\"messageIcon\" class=\"fas fa-check-circle\"></i>" + paymentMessage + "<i id=\"messageCloseIcon\" class=\"fas fa-times\"></i></div>"));
                            return;
                        }
                    }
                }
            }
                                 
                
            //Fait le rendu du formulaire
            FormPanel.Controls.Add(rend.DivGlobalParam);
            if (_eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED && _eFileForm.Version == FormularVersion.ADV_V2)
            {
                if (rend.FormFile.isPaymentCancelled)
                    FormPanel.Controls.Add(TemplateControl.ParseControl("<div id=\"snackbar\" class=\"messageError\"><i  id=\"messageIcon\" class=\"fas fa-exclamation-triangle\"></i>" + rend.MsgPaymentCanceled + "<i id=\"messageCloseIcon\" class=\"fas fa-times\"></i></div>"));
                FormPanel.Controls.Add(TemplateControl.ParseControl("<div id=\"app\"></div>"));
            }
            else
                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.BodyMerge));

            if (rend.RawScript != null && rend.RawScript.Length > 0)
                PageRegisters.RawScrip.Append(rend.RawScript.ToString());

            #region Ajout des partages réseaux sociaux

            if (_eFileForm.FacebookShare)
                GenerateFacebookShare();
            if (_eFileForm.GoogleShare)
                GenerateGoogleShare();
            if (_eFileForm.TwitterShare)
                GenerateTwitterShare();
            if (_eFileForm.LinkedinShare)
                GenerateLinkedinShare();

            #endregion

            AddMetaSocialNetworks(rend);

            ScriptContainer.InnerHtml = rend.AppendInitScript();

            //dans le cas où c'est un formulaire avancé, on ajoute un css custom pour ce type de formulaire
            if (_eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED)
            {
                rend.ListCssToRegister.Add("grapesjs/grapesjs-responsive-eudonet");

                //Pour la version V2 du formulaire avancé, on ajoute les css de vue et vuetify
                if (_eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_ADVANCED && _eFileForm.Version == FormularVersion.ADV_V2)
                {
                    //Add vue/eudofront js et css
                    rend.ListScriptToRegister.Add("~/IRISBlack/Front/Scripts/Libraries/vue/vue");
                    rend.ListScriptToRegister.Add("~/IRISBlack/Front/Scripts/Libraries/vuex/vuex");
                    rend.ListScriptToRegister.Add("~/IRISBlack/Front/scripts/Libraries/vuetify/vuetify.min");
#if DEBUG
                    rend.ListScriptToRegister.Add("~/IRISBlack/Front/scripts/Libraries/eudofront/eudoFront.umd");
#else
                    rend.ListScriptToRegister.Add("~/IRISBlack/Front/scripts/Libraries/eudofront/eudoFront.umd.min");
#endif

                    rend.ListCssWithPathToRegister.Add("IRISBlack/Front/Scripts/Libraries/vuetify/vuetify.min");
                    rend.ListCssWithPathToRegister.Add("IRISBlack/Front/Assets/CSS/advForm/materialdesign/materialdesignicons.min");
                    rend.ListCssWithPathToRegister.Add("IRISBlack/Front/Assets/CSS/advForm/googleFonts");
                    rend.ListCssWithPathToRegister.Add("IRISBlack/Front/Assets/CSS/advForm/eudoFront");

                    //Tâche 3 452: ajout du meta viewport
                    var tag = new HtmlMeta();
                    tag.Attributes.Add("name", "viewport");
                    tag.Content = "width=device-width, initial-scale=1.0";
                    MetaPlaceHolder.Controls.Add(tag);
                }
            }

            PageRegisters.AddRangeScript(rend.ListScriptToRegister);
            PageRegisters.AddRangeCss(rend.ListCssToRegister);
            PageRegisters.AddRangeCssWithPath(rend.ListCssWithPathToRegister);

            if (rend.BodyCss != null)
                PageRegisters.SetRawCss(rend.BodyCss);

            //Ajout des infos a la page du rendu pour le js
            AddClientContextInfo(rend);
        }

        /// <summary>
        /// Ajout des Metas des réseaux sociaux
        /// </summary>
        /// <param name="rend">Renderer</param>
        private void AddMetaSocialNetworks(eFormularFileRenderer rend)
        {
            #region Ajout des Meta réseaux sociaux

            foreach (HtmlMeta meta in rend.MetaSharingTags)
                MetaSocialNetworksPlaceHolder.Controls.Add(meta);

            #endregion

            #region Ajout des Meta réseaux sociaux

            // Ajout des meta
            foreach (HtmlMeta meta in rend.MetaTags)
                MetaPlaceHolder.Controls.Add(meta);

            #endregion
        }

        


        /// <summary>
        /// Ajoute des infos sur le contexte du formulaire (tab, descids, re) sous forme d'inputs cachées
        /// necessaire si on veux faire des vérification sur les champs obligatoires 
        /// </summary>
        /// <param name="rend">Renderer</param>
        private void AddClientContextInfo(eFormularFileRenderer rend)
        {
            //Pour que la fonction "getFieldsInfos(nTab, nFileId)" puisse récupérer les rubriques de invit/cible 
            //a fin de faire des vérification     
            foreach (var ctrl in rend.GetClientContextInfo(_pageQueryString.GetConserveInfo()))
                ContextPanel.Controls.Add(ctrl);
        }

        /// <summary>
        /// On vérifie si soumission unique, le cas echeant on affiche un message
        /// Retourne un vrai si le formulaire à déjà été soumis.
        /// </summary>
        private bool CheckUniqueSubmission()
        {
            if (_eFileForm != null && _eFileForm.IsUniqueSubmission && _eFileForm.AlreadySubmitted)
            {
                FormPanel.Controls.Add(TemplateControl.ParseControl(_eFileForm.LabelAlreadySubmit));
                return true;
            }

            return false;
        }

        /// <summary>
        /// On vérifie si le formulaire avancé est non disponible ou expiré
        /// </summary>
        private bool CheckFormularExpirationAvailability()
        {
            if (_eFileForm.Status == 1 || _eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_CLASSIC)
            {
                if (_eFileForm != null || _eFileForm.ExpireDate != null || _eFileForm.StartDate != null)
                {
                    if (_eFileForm.FormularType == EudoQuery.FORMULAR_TYPE.TYP_CLASSIC)
                    {
                        if (_eFileForm.IsExpired)
                        {
                            FormPanel.Controls.Add(TemplateControl.ParseControl(_eFileForm.ExpireMessage));
                            return true;
                        }
                    }
                    else
                    {
                        if (_eFileForm.ExpireDate != null && _eFileForm.StartDate != null)
                        {
                            if (_eFileForm.IsStarted && !_eFileForm.IsExpired)
                            {
                                return false;
                            }
                            else if (!_eFileForm.IsStarted && _eFileForm.IsExpired)
                            {
                                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.MsgDateEnd));
                                return true;
                            }
                            else if (!_eFileForm.IsStarted && !_eFileForm.IsExpired)
                            {
                                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.MsgDateStart));
                                return true;
                            }
                            else if (_eFileForm.IsStarted && _eFileForm.IsExpired)
                            {
                                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.MsgDateEnd));
                                return true;
                            }
                        }
                        else if (_eFileForm.ExpireDate != null && _eFileForm.StartDate == null)
                        {
                            if (!_eFileForm.IsExpired)
                            {
                                return false;
                            }
                            else
                            {
                                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.MsgDateEnd));
                                return true;
                            }
                        }
                        else if (_eFileForm.ExpireDate == null && _eFileForm.StartDate != null)
                        {
                            if (_eFileForm.IsStarted)
                            {
                                return false;
                            }
                            else
                            {
                                FormPanel.Controls.Add(TemplateControl.ParseControl(rend.MsgDateStart));
                                return true;
                            }
                        }
                        else if (_eFileForm.ExpireDate == null && _eFileForm.StartDate == null)
                        {
                            return false;
                        }
                    }
                }
            }

            return false;

        }

        /// <summary>
        /// Charge les tokens du formulaire de la queryString
        /// </summary>
        protected override void LoadQueryString()
        {
            DataParam = new LoadQueryStringForm(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);
        }

        /// <summary>
        /// Classe interne de la levé d'exception propre à la construction du rendu
        /// </summary>
        class FormularRendExp : Exception
        {
            public FormularRendExp(Exception innerException)
                : base("", innerException)
            {

            }
        }

        /// <summary>Génère un bouton "J'aime" et "Partage" Facebook</summary>
        private void GenerateFacebookShare()
        {
            string urlToShare = "window.location";

            string lang = "fr_FR";
            if (HttpContext.Current.Request.UserLanguages != null)
            {
                lang = HttpContext.Current.Request.UserLanguages[0].Replace('-', '_');
            }

            HtmlGenericControl div = new HtmlGenericControl("div");

            LiteralControl literal = new LiteralControl();
            literal.Text = string.Concat("<div id='fb-root'></div>",
                "<script>(function(d, s, id) {",
                  "var js, fjs = d.getElementsByTagName(s)[0];",
                  "if (d.getElementById(id)) return;",
                  "js = d.createElement(s); js.id = id;",
                  "js.src = '//connect.facebook.net/", lang, "/sdk.js#xfbml=1&version=v2.6';",
                  "fjs.parentNode.insertBefore(js, fjs);",
                "}(document, 'script', 'facebook-jssdk'));</script>",

                    "<div class='fb-like' id='fb-like' data-layout='button' data-action='like' data-show-faces='true' data-share='true'></div>",

                    "<script type='text/javascript'>",
                    "document.getElementById('fb-like').setAttribute('data-href', ", urlToShare, ");",
                "</script>");

            div.Controls.Add(literal);

            FormPanel.Controls.Add(div);

            HtmlMeta tag = new HtmlMeta();
            tag.Attributes.Add("property", "og:url");
            tag.Content = "http://www.eudonet.com";
            MetaPlaceHolder.Controls.Add(tag);

            tag = new HtmlMeta();
            tag.Attributes.Add("property", "og:title");
            tag.Content = _eFileForm.Label;
            MetaPlaceHolder.Controls.Add(tag);

            // TODO: Même chose pour l'image : à défini


        }

        /// <summary>Génère un bouton de partage Google +</summary>
        private void GenerateGoogleShare()
        {
            LiteralControl lit = new LiteralControl();
            lit.Text = string.Concat("<link rel='canonical' href=''>");
            CustomPlaceHolder.Controls.Add(lit);

            HtmlGenericControl div = new HtmlGenericControl("div");

            lit = new LiteralControl();
            lit.Text = string.Concat(
                "<script src='https://apis.google.com/js/platform.js' async defer></script><g:plus action=\"share\" ", "></g:plus>");

            div.Controls.Add(lit);
            FormPanel.Controls.Add(div);
        }

        /// <summary>Génère un bouton de partage Twitter</summary>
        private void GenerateTwitterShare()
        {
            string baseUrl = "https://twitter.com/intent/tweet?text=";

            HtmlGenericControl div = new HtmlGenericControl("div");

            LiteralControl lit = new LiteralControl();
            lit.Text = string.Concat(
                "<div><a class='twitter-share-button' id='twitter-share-button' href='", baseUrl, _eFileForm.Label, "&url='>Tweet</a></div>",
                "<script type='text/javascript'>var userLang = navigator.language || navigator.userLanguage; document.getElementById('twitter-share-button').setAttribute('data-lang', userLang);</script>",
                "<script>!function(d,s,id){var js,fjs=d.getElementsByTagName(s)[0],p=/^http:/.test(d.location)?'http':'https';if(!d.getElementById(id)){js=d.createElement(s);js.id=id;js.src=p+'://platform.twitter.com/widgets.js';fjs.parentNode.insertBefore(js,fjs);}}(document, 'script', 'twitter-wjs');</script>");

            div.Controls.Add(lit);
            FormPanel.Controls.Add(div);
        }

        /// <summary>Génère un lien de partage Linkedin</summary>
        private void GenerateLinkedinShare()
        {
            HtmlMeta tag = new HtmlMeta();
            tag.Attributes.Add("property", "og:url");
            tag.Content = "";
            MetaPlaceHolder.Controls.Add(tag);

            tag = new HtmlMeta();
            tag.Attributes.Add("property", "og:title");
            tag.Content = _eFileForm.Label;
            MetaPlaceHolder.Controls.Add(tag);

            string lang = "fr_FR";
            if (HttpContext.Current.Request.UserLanguages != null)
            {
                lang = HttpContext.Current.Request.UserLanguages[0].Replace('-', '_');
            }

            HtmlGenericControl div = new HtmlGenericControl("div");

            LiteralControl lit = new LiteralControl();
            lit.Text = string.Concat("<script src=\"//platform.linkedin.com/in.js\" type=\"text/javascript\"> lang: ", lang, "</script><script type=\"IN/Share\"></script>");

            div.Controls.Add(lit);
            FormPanel.Controls.Add(div);
        }


    }
}