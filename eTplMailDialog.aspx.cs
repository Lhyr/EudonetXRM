using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.renderer;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Xml;
using Com.Eudonet.Core.Model;
using System.Web.UI.WebControls;
using System.IO;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe qui fabrique un flux HTML pour l'affichage d'un champ Mémo en plein écran
    /// </summary>
    public partial class eTplMailDialog : eEudoPage
    {
        #region propriétés

        /// <summary>Javascript des ressources de l'application</summary>
        protected String _resAppJS = String.Empty;

        private String _memoEditorJsVarName = String.Empty;
        private String _parentFrameId = String.Empty;
        private String _libelle = String.Empty;
        private String _objet = String.Empty;
        // AABBA tache #1 940
        private String _preheader = String.Empty;
        private String _value = String.Empty;
        private String _bodyCss = String.Empty;
        private TypeMailTemplate _mtType = TypeMailTemplate.MAILTEMPLATE_UNDEFINED;

        private String _divMainHeight = String.Empty;

        private bool _isHTML = true;
        private bool _inlineMode = false;

        private bool _readOnly = false;

        private StringBuilder _sbInitJSOutput = new StringBuilder();
        private StringBuilder _sbEndJSOutput = new StringBuilder();
        private StringBuilder _sbMailMergeFields = new StringBuilder();

        private int _TemplateMailId = 0;
        private int _fileId = 0;
        private int _tabId = 0;

        private int _tabFrom = 0;

        private String _title = String.Empty; // Titre du catalogue

        private string _editorType = String.Empty; // Type de champ Mémo
        /// <summary>
        /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
        /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
        /// Cette variable sera valorisée lors de l'appel à eTplMailDialog par eMailingWizard.js
        /// </summary>
        private bool _enableTemplateEditor = false;
        /// <summary>
        /// Barre d'outils à afficher sur le champ Mémo (CKEditor)
        /// </summary>
        private string _toolbarType = String.Empty;
        /// <summary>
        /// US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
        /// </summary>
        private bool _useNewUnsubscribeMethod = false;

        /// <summary>
        /// true pour afficher un formulaire d'upload de fichiers à insérer dans le champ Mémo de la fenêtre (ex : injection CSS)
        /// </summary>
        private bool _bUploadContentEnabled = false;
        /// <summary>
        /// Libellé à afficher à côté du champ d'upload
        /// </summary>
        private string _strUploadContentLabel = String.Empty;
        /// <summary>
        /// Extensions de fichiers acceptées pour l'upload, séparées par ; (ex : "css;txt")
        /// </summary>
        private string _strUploadContentFileFilter = String.Empty;
        /// <summary>
        /// Taille limite acceptée pour le fichier uploadé, en octets
        /// </summary>
        private int _nUploadContentLimit = 0;
        /// <summary>
        /// Si ce paramètre est à true, le contenu uploadé sera ajouté au contenu existant du champ Mémo.
        /// S'il est à false, le contenu uploadé remplacera le contenu actuel du champ Mémo
        /// </summary>
        private bool _bUploadContentAppend = false;

        private int _nbPJ = 0;

        // Si le projet XRM est compilé en mode DEBUG, on génère le JS avec des retours à la ligne pour faciliter le debug
#if DEBUG
        private string _jsOutputNewLine = Environment.NewLine;
#else
        private string _jsOutputNewLine = String.Empty;
#endif
        /// <summary>Variables à transferer aux memoeditor</summary>
        private String _sAddMemoVar = String.Empty;

        #endregion

        #region accesseurs pour dispo en JS

        public int TemplateMailId
        {
            get { return _TemplateMailId; }
        }

        public int FileId
        {
            get { return _fileId; }
        }

        public int TabId
        {
            get { return _tabId; }
        }

        public String Value
        {
            get { return _value; }
        }
        public String Objet
        {
            get { return _objet; }
        }

        //AABBA tache #1 940
        public String Preheader
        {
            get
            {
                return _preheader;
            }
        }
        public String Libelle
        {
            get { return _libelle; }
        }

        public String BodyCss
        {
            get { return _bodyCss; }
        }

        public bool IsHTML
        {
            get { return _isHTML; }
        }

        public TypeMailTemplate MailTemplateType
        {
            get { return _mtType; }
        }

        public bool ReadOnly
        {
            get { return _readOnly; }
        }
        /// <summary>
        /// Indique le type d'éditeur attendu, pour personnaliser son affichage selon contexte (ex : emailing)
        /// </summary>
        public string EditorType
        {
            get { return _editorType; }
        }

        /// <summary>
        /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
        /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
        /// </summary>
        public bool EnableTemplateEditor
        {
            get { return _enableTemplateEditor; }
        }
        public string ToolbarType
        {
            get { return _toolbarType; }
        }

        public string MemoEditorJSVarName
        {
            get
            {
                return _memoEditorJsVarName;
            }
        }
        public string ParentFrameId
        {
            get
            {
                return _parentFrameId;
            }
        }

        public String InitJSOutput
        {
            get { return _sbInitJSOutput.ToString(); }
        }

        public String MailMergeFields
        {
            get { return _sbMailMergeFields.ToString(); }
        }

        public String EndJSOutput
        {
            get { return _sbEndJSOutput.ToString(); }
        }

        public String NbPJ
        {
            get { return _nbPJ.ToString(); }
        }

        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder ()
        {
            return scriptHolder;
        }


        /// <summary>
        /// Retourne le js d'initialisation du memo editeur
        /// </summary>
        /// <param name="sMemoObjectName">Nom js pour le memo editeur</param>
        /// <param name="sMemoDivId">Div conteneur du memo editeur</param>
        /// <param name="tplEditor">Oui/Non pour l'editeur "avancé" (grapesjs à ce jour 26/02/2019)</param>
        /// <returns></returns>
        private string getJSInitMemeoEditor (string sMemoObjectName, string sMemoDivId, bool tplEditor = false)
        {

            //Si non disponible force grapesjs à false
            if ((int)_pref.ClientInfos.ClientOffer == 0
                 || eTools.IsMSBrowser
                 || !eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
                 || MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAIL /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */
            )
            {
                tplEditor = false;
            }

            if (MailTemplateType != TypeMailTemplate.MAILTEMPLATE_EMAIL)
            {
                /* On met le contenu du template centré dans le textArea qui sert à afficher le contenu de l'éditeur
                 * Grape. Ce contenu sera reversé à CKEditor en cas de besoin (IE11 - Edge...). G.L */
                eMailingTemplate mailTemplateCentre = new eMailingTemplate(_pref);
                mailTemplateCentre.LoadCustom(13);
                eTplMailDialogEditorValue.InnerHtml = string.IsNullOrEmpty(eTplMailDialogEditorValue.InnerHtml.Trim()) ? mailTemplateCentre.Body : eTplMailDialogEditorValue.InnerHtml;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(" = new eMemoEditor(")
                 .Append(_jsOutputNewLine).Append("'").Append(sMemoObjectName).Append("',")
                 .Append(_jsOutputNewLine).Append("bHTML,")
                 .Append(_jsOutputNewLine).Append("document.getElementById('" + sMemoDivId + "'),")
                 .Append(_jsOutputNewLine).Append("null,") // eFieldEditor parent = aucun
                 .Append(_jsOutputNewLine).Append("GetText(document.getElementById('eTplMailDialogEditorValue')),")
                 .Append(_jsOutputNewLine).Append("bCompactMode,")
                 .Append(_jsOutputNewLine).Append("'" + sMemoObjectName + "'")
                 .Append(_jsOutputNewLine).Append(");")
                 .Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".title = '").Append(_title).Append("';");

            //Variables à transferer aux memoeditor
            foreach (string splitMemoVar in _sAddMemoVar.Split(EudoQuery.SEPARATOR.LVL1))
            {
                if (splitMemoVar.Length <= 0)
                    continue;
                string[] tabMemoVar = splitMemoVar.Split(EudoQuery.SEPARATOR.LVL2);
                if (tabMemoVar.Length < 2)
                    continue;
                String key = tabMemoVar[0];
                String value = tabMemoVar[1].Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".").Append(key).Append(" = \"").Append(value).Append("\";");
            }

            String strInlineMode = (_inlineMode ? "true" : "false");
            // Mode inline editing
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".inlineMode = ").Append(strInlineMode).Append(";");

            // Mode fullscreen : pas de bouton FullScreen dans la barre d'outils HTML ou texte
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".isFullScreen = true;");

            // Le champ Mémo étant le seul contrôle de cette fenêtre, on met le focus dessus
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".focusOnShow = true;");

            IDictionary<eLibConst.CONFIGADV, String> dicEmailAdvConfig = eLibTools.GetConfigAdvValues(_pref,
                new HashSet<eLibConst.CONFIGADV> {
                        eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED,
                        eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD
                });

            if (dicEmailAdvConfig[eLibConst.CONFIGADV.EXTERNAL_TRACKING_ENABLED] == "1")
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".externalTrackingEnabled = true;");

            // Et on empêche son affichage initial en mode compact, même si la surface d'affichage est jugée insuffisante (on est en plein écran)
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".preventCompactMode = true;");

            // Pas de mise à jour du champ Mémo lors de la sortie du champ
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".updateOnBlur = false;");

            // Mode lecture seule ou écriture
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".readOnly = ").Append(_readOnly ? "true" : "false").Append(";");

            // Type de champ Mémo à afficher
            if (_editorType.Length > 0)
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".editorType = '").Append(_editorType).Append("';");

            /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
            /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".enableTemplateEditor = ").Append(tplEditor ? "true" : "false").Append(";");

            /// US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
            if (dicEmailAdvConfig[eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD] == "1")
            {
                _useNewUnsubscribeMethod = true;
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".useNewUnsubscribeMethod = true;");
            }

            // Barre d'outils à afficher
            if (_toolbarType.Length > 0)
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".toolbarType = '").Append(_toolbarType).Append("';");

            // Mise à jour de la configuration de base du champ Mémo (mode HTML) avec les propriétés ci-dessus
            sb.Append(_jsOutputNewLine).Append("if (bHTML) {")
                .Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setSkin('eudonet');")
                // En mode fiche (non zoomé), on n'affiche pas la barre d'outils, qu'on affiche alors en mode Zoom
                // On gère l'affichage de la barre d'outils via une méthode spécifique de memoEditor qui permet d'affecter plusieurs propriétés
                .Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setToolBarDisplay(true, true);")
                // L'affichage de la barre de statut se fait par activation/désactivation de plugin. On gère ce comportement via une méthode spécifique de memoEditor
                // car elle ne fait pas partie de celles proposées par l'objet config de CKEditor
                ;

            if (_toolbarType == "mail")
            {
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setStatusBarEnabled(true);");
            }
                ;
            if (_toolbarType == "mailing" || _toolbarType == "mailingtemplate" || _toolbarType == "mailtemplate" || _toolbarType == "formular")
            {
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setStatusBarEnabled(true);");

                sb.Append(_jsOutputNewLine).Append(eEditMailingRenderer.GetMergeAndTrackFields(this._pref, _tabFrom)).Append(";");
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".mergeFields = mailMergeFields;");
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".oMergeHyperLinkFields = oMergeHyperLinkFields;");
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".oTracking = oTrackFields;");
            }

            // Feuille de style CSS personnalisée à injecter à l'instanciation du champ, si demandé
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setCss(document.getElementById('eTplMailDialogEditorCustomCSS').value);");

            // Backlog #619/#652 - Couleur de fond à injecter à l'instanciation du champ, si demandé
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".setColor(").Append(sMemoObjectName).Append(".getColorFromCSS(document.getElementById('eTplMailDialogEditorCustomCSS').value));");

            sb.Append(_jsOutputNewLine).Append("}");

            // Mise à jour de la configuration commune (HTML ET texte)
            int memoHeightNum = eLibTools.GetNum(_divMainHeight);
            if (memoHeightNum == 0)
            {
                // Cas que ca ne soit pas un chiffre
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".config.height = '").Append(_divMainHeight).Append("';");
            }
            else
            {
                // TODO - Ckeditor ne gére pas la taille des ces barres en haut et en bas, du coup nous depassons de son conteneur...
                sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".config.height = '' + (");

                sb.Append(memoHeightNum).Append(" - document.getElementById('DivObject').clientHeight");
                sb.Append(" - document.getElementById('PermOptions').offsetHeight");
                if (_bUploadContentEnabled)
                    sb.Append(" - document.getElementById('memoDlgUploadDiv').clientHeight");

                sb.Append(") + '';");
            }

            // On affecte une propriété childMemoEditor de l'objet eMemoEditor appelant (parent) avec un pointeur vers l'objet eMemoEditor
            // instancié par cette page/fenêtre eTplMailDialog.aspx, afin que l'objet parent puisse communiquer avec l'objet enfant de cette page.
            sb.Append(_jsOutputNewLine).Append("var oParentMemoEditor = null;");
            sb.Append(_jsOutputNewLine).Append(sMemoObjectName).Append(".show(); ");

            return sb.ToString();

        }

        private string getJSInitInput (string sMemoObjectId, string sMemoDivId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("var eMemoDialogEditorObject = null;");
            sb.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject").Append(" = new eMemoEditor(")
                .Append(_jsOutputNewLine).Append("'").Append("eMemoDialogEditorObject").Append("',")
                .Append(_jsOutputNewLine).Append("true,")
                .Append(_jsOutputNewLine).Append("document.getElementById('" + sMemoDivId + "'),")
                .Append(_jsOutputNewLine).Append("document.getElementById('" + sMemoObjectId + "'),") // eFieldEditor parent = aucun
                .Append(_jsOutputNewLine).Append("GetText(document.getElementById('" + sMemoObjectId + "')),")
                .Append(_jsOutputNewLine).Append("false,")
                .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject")
                .Append(_jsOutputNewLine).Append(");eMemoDialogEditorObject.show();");

            return sb.ToString();
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load (object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eMain", "all");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("grapesjs/grapes.min");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-newsletter");
            PageRegisters.AddCss("grapesjs/grapesjs-preset-webpage");
            PageRegisters.AddCss("eMemoEditor");
            PageRegisters.AddCss("eButtons");
            PageRegisters.AddCss("ePerm");
            PageRegisters.AddCss("eTplMail");
            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");
            #endregion

            #region ajout des js




            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("grapesjs/grapes.min");
            PageRegisters.AddScript("grapesjs/grapesjs-plugin-ckeditor.min");
            PageRegisters.AddScript("grapesjs/grapesjs-blocks-basic.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-newsletter.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-webpage.min");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFile");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("ePerm");
            PageRegisters.AddScript("eMailing");
            PageRegisters.AddScript("eMailingTpl");
            PageRegisters.AddScript("eMailingWizard");

            #endregion

            //DateTime debut = DateTime.Now; // Utilisé pour tracer les temps d'execution, à conserver pour d'éventuels tests de performance
            Boolean onError = false;
            String action = String.Empty;
            XmlNode detailsNode = null;
            XmlDocument _xmlResult = new XmlDocument();

            #region Initialisation

            try
            {
                if (Request.Form["MailTemplateId"] != null)
                    _TemplateMailId = int.Parse(Request.Form["MailTemplateId"].ToString());

                if (Request.Form["EditorJsVarName"] != null)
                    _memoEditorJsVarName = Request.Form["EditorJsVarName"].ToString();

                if (Request.Form["IsHTML"] != null)
                    _isHTML = Request.Form["IsHTML"].ToString().Equals("1");
                if (Request.Form["InlineMode"] != null)
                    _inlineMode = Request.Form["InlineMode"].ToString().Equals("1");
                if (Request.Form["EditorType"] != null)
                    _editorType = Request.Form["EditorType"].ToString();
                if (Request.Form["EnableTemplateEditor"] != null)
                    _enableTemplateEditor = Request.Form["EnableTemplateEditor"].ToString().Equals("1");
                if (Request.Form["UseNewUnsubscribeMethod"] != null)
                    _useNewUnsubscribeMethod = Request.Form["UseNewUnsubscribeMethod"].ToString().Equals("1");
                if (Request.Form["ToolbarType"] != null)
                    _toolbarType = Request.Form["ToolbarType"].ToString();
                if (Request.Form["divMainHeight"] != null)
                    _divMainHeight = Request.Form["divMainHeight"].ToString();

                if (Request.Form["tabFrom"] != null)
                    Int32.TryParse(Request.Form["tabFrom"].ToString(), out _tabFrom);

                _mtType = TypeMailTemplate.MAILTEMPLATE_UNDEFINED;
                if (Request.Form["mtType"] != null)
                {
                    if (Request.Form["mtType"].ToString() == TypeMailTemplate.MAILTEMPLATE_EMAIL.GetHashCode().ToString())
                        _mtType = TypeMailTemplate.MAILTEMPLATE_EMAIL;
                    else
                        _mtType = TypeMailTemplate.MAILTEMPLATE_EMAILING;
                }

                if (Request.Form["AddMemoVar"] != null)
                    _sAddMemoVar = Request.Form["AddMemoVar"].ToString();

                if (_TemplateMailId == 0)
                {
                    _value = HttpUtility.UrlDecode(Request.Form["Value"].ToString());
                    try
                    {

                        //Ajout de la css pour les nouveaux modèle
                        string cssFile = Context.Server.MapPath(String.Concat(Context.Request.ApplicationPath, "/", String.Concat(eTools.WebPathCombine("themes", "default", "css", "grapesjs", "grapesjs-eudonet"), ".css")));
                        TextReader tr = new StreamReader(cssFile);
                        _bodyCss = tr.ReadToEnd();
                        tr.Close();
                        tr.Dispose();



                    }
                    catch { }

                    // Rendu des options de permission sur le modèle 
                    ePermissionRenderer rend = new ePermissionRenderer(_pref, bPublic: false, sLabel: "");
                    PermOptions.Controls.Add(rend.GetSavePermOptions());
                }
                else
                {
                    #region Récupération de la valeur du champ Mémo depuis la base de données

                    try
                    {
                        eMailingTemplate EmailingTemplate = new eMailingTemplate(_pref);
                        EmailingTemplate.Load(_TemplateMailId);
                        _libelle = HttpUtility.HtmlEncode(EmailingTemplate.Label);
                        _objet = HttpUtility.HtmlEncode(EmailingTemplate.Subject);
                        /// AABBA tache #1 940
                        _preheader = HttpUtility.HtmlEncode(EmailingTemplate.Preheader);
                        _value = EmailingTemplate.Body;
                        _bodyCss = EmailingTemplate.BodyCss;

                        // Rendu des options de permission sur le modèle 
                        ePermissionRenderer rend = new ePermissionRenderer(_pref
                        , bPublic: EmailingTemplate.Owner_User <= 0
                        , viewPermId: EmailingTemplate.ViewPerm.PermId, updatePermId: EmailingTemplate.UpdatePerm.PermId
                    );
                        PermOptions.Controls.Add(rend.GetSavePermOptions());
                    }
                    catch (Exception exp)
                    {
                        onError = true;

                        //Avec exception
                        String sDevMsg = String.Concat("Erreur sur eTplMailDialog - Récupération de la valeur du champ Mémo impossible -> : ");
                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message,
                            Environment.NewLine, "Exception StackTrace :", exp.StackTrace
                        );

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref, 72),  //   titre
                            String.Concat(sDevMsg)
                        );

                        LaunchError();
                    }
                    finally
                    {
                        //dal.CloseDatabase();
                    }
                    #endregion
                }


                //Si GrapJS, on créé un pseudo chemin de fer pour les étapes GrapJ <---> CKEditor
                if (
                  _pref.ClientInfos.ClientOffer == 0
                  || eTools.IsMSBrowser
                  || !eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
              ) //Dans ce cas, l'étape 3 est la "dernière"
                {
                    railroad.InnerHtml = eResApp.GetRes(_pref, 2225);
                }
                /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */
                else if (MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAIL)
                {

                }
                else
                {

                    railroad.Attributes.Add("class", "states_placement");

                    // 595 - Hack cp/cv pour avoir le "chemin de fer" en dehors d'un wizard

                    //Etape GrapJS
                    Panel stepBloc = new Panel();
                    railroad.Controls.Add(stepBloc);
                    stepBloc.ID = "step_1";
                    stepBloc.CssClass = "state_grp-current";
                    stepBloc.Attributes.Add("onclick", String.Concat("eTplMail.StepClick(", 1, ");"));
                    stepBloc.Attributes.Add("ednStep", "");

                    Panel numberBloc = new Panel();
                    stepBloc.Controls.Add(numberBloc);
                    numberBloc.ID = "txtnum_1";
                    numberBloc.Controls.Add(new LiteralControl("1"));

                    Label lbl = new Label();
                    stepBloc.Controls.Add(lbl);
                    lbl.Text = eResApp.GetRes(_pref, 2227); //Création graphique



                    Panel stepBlocSeparator = new Panel();
                    railroad.Controls.Add(stepBlocSeparator);
                    stepBlocSeparator.Attributes.Add("class", "state_sep");

                    stepBloc = new Panel();
                    railroad.Controls.Add(stepBloc);
                    stepBloc.ID = "step_2";
                    stepBloc.Attributes.Add("ednStep", "");
                    stepBloc.CssClass = "state_grp";// : _mailing == null ? "state_grp" : "state_grp-validated";
                    stepBloc.Attributes.Add("onclick", String.Concat("eTplMail.StepClick(", 2, ");"));

                    numberBloc = new Panel();
                    stepBloc.Controls.Add(numberBloc);
                    numberBloc.ID = "txtnum_2";
                    numberBloc.Controls.Add(new LiteralControl("2"));

                    lbl = new Label();
                    stepBloc.Controls.Add(lbl);
                    lbl.Text = eResApp.GetRes(_pref, 2226);//"Contenus et liens"





                }

                #region Bouton Annexes du modèle de Mail Unitaire

                String pjIDs = String.Empty;
                eMailTemplatePjList pjList = new eMailTemplatePjList(_pref, TableType.MAIL_TEMPLATE.GetHashCode(), _TemplateMailId);
                if (pjList.Generate())
                {
                    pjIDs = String.Join(";", pjList.DicoPj.Keys);
                    _nbPJ = pjList.DicoPj.Count;
                }

                btnPJ.Attributes.Add("PjIds", pjIDs);

                if (_mtType == TypeMailTemplate.MAILTEMPLATE_EMAIL)
                {
                    btnPJ.Attributes.Add("onclick", "showTemplatePJList(" + TableType.MAIL_TEMPLATE.GetHashCode() + ", " + _TemplateMailId + ");");
                    btnPJ.Attributes.Add("title", eResApp.GetRes(_pref, 7883));
                }
                else
                {
                    btnPJ.Style.Add("display", "none");
                }

                #endregion

                // Ajustement des paramètres
                String strIsHTML = (_isHTML ? "true" : "false");
                if (!_isHTML)
                    _value = HttpUtility.HtmlDecode(_value);
                String strInlineMode = (_inlineMode ? "true" : "false");

                // Création des contrôles
                // Paramétrage de la configuration du champ Mémo                
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("var bHTML = ").Append(strIsHTML).Append(";");
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("var bCompactMode = false;");

                eTplMailDialogEditorValue.InnerText = _value;

                // Création et instanciation du champ Mémo
                _sbInitJSOutput
                    .Append(_jsOutputNewLine).Append(getJSInitMemeoEditor("eTplMailDialogEditorObject", "eTplMailDialogEditorContainer", _enableTemplateEditor));

                if (
                        (int)_pref.ClientInfos.ClientOffer > 0
                    && !eTools.IsMSBrowser
                    && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
                    && MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAILING /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */
                )
                {
                    _sbInitJSOutput
                        .Append(_jsOutputNewLine).Append("eTplMail.StepClick(1);")
                        .Append(_jsOutputNewLine).Append(getJSInitMemeoEditor("eTplMailDialogEditorObjectCKe", "eTplMailDialogEditorCKE"))
                        ;
                }
                //KJE, tâche #2 431: on crée la liste des champs de fusion pour l'objet du mail sauf les champs de type Mémo, Images, graphiques, Page web
                _sbMailMergeFields.Append(eEditMailingRenderer.GetMergeAndTrackFields(this._pref, _tabFrom, bGetOnlyTxtMergedField: true)).Append(";");//KJE tâche 2 334
                _sbMailMergeFields.Append(" var mailObjectMergeFields = mailMergeFields; ");

                //_sbInitJSOutput.Append(_jsOutputNewLine).Append(" eTplMailDialogEditorObject.disableForExternal(); ");

            }
            catch (Exception exp)
            {
                onError = true;

                //Avec exception
                String sDevMsg = String.Concat("Erreur sur eTplMailDialog - Page_Load catch global= -> : ");
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message,
                    Environment.NewLine, "Exception StackTrace :", exp.StackTrace
                    );


                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    String.Concat(sDevMsg)

                    );
            }

            #endregion

            if (onError)
            {
                LaunchError();
            }

            //*****  On force le rendu HTML avant qu'il s'affiche dans la page pour le XML  //
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.IO.StringWriter sw = new System.IO.StringWriter(sb);
            HtmlTextWriter hw = new HtmlTextWriter(sw);

            detailsNode = _xmlResult.CreateElement("contents");

            XmlNode _htmlNode = _xmlResult.CreateElement("html");
            _htmlNode.InnerText = sb.ToString();
            detailsNode.AppendChild(_htmlNode);

            XmlNode _jsNode = _xmlResult.CreateElement("js");
            _jsNode.InnerText = _sbInitJSOutput.ToString();
            detailsNode.AppendChild(_jsNode);

        }
    }
}