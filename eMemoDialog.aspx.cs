using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;
using System.IO;
using static Com.Eudonet.Internal.eLibConst;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe qui fabrique un flux HTML pour l'affichage d'un champ Mémo en plein écran
    /// </summary>
    public partial class eMemoDialog : eEudoPage
    {
        #region propriétés


        /// <summary>Javascript des ressources de l'application</summary>
        protected String _resAppJS = String.Empty;

        private String _memoEditorJsVarName = String.Empty;
        private String _parentEditor = String.Empty;
        private String _parentFrameId = String.Empty;

        private String _value = String.Empty;
        private String _customCSS = String.Empty;
        private String _customOnShow = String.Empty;

        private String _divMainWidth = String.Empty;
        private String _divMainHeight = String.Empty;

        private bool _isHTML = true;
        private bool _showXrmFormularBtn = false;
        private bool _inlineMode = false;

        private bool _readOnly = false;

        private StringBuilder _sbInitJSOutput = new StringBuilder();
        private StringBuilder _sbEndJSOutput = new StringBuilder();

        private int _descId = 0;
        private int _fileId = 0;
        private int _tabId = 0;
        /// <summary>
        /// Titre du catalogue
        /// </summary>
        private String _title = String.Empty;
        private bool _showOnlyMergedFields = false;

        /// <summary>
        /// Type de champ Mémo affiché (utilisé pour spécialiser certains traitements, ex : champs de fusion)
        /// </summary>
        private string _editorType = String.Empty;
        /// <summary>
        /// #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
        /// Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
        /// </summary>
        private bool _enableTemplateEditor = false;
        /// <summary>
        /// Barre d'outils à afficher sur le champ Mémo (CKEditor)
        /// </summary>
        private string _toolbarType = String.Empty;

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

        // Si le projet XRM est compilé en mode DEBUG, on génère le JS avec des retours à la ligne pour faciliter le debug
#if DEBUG
        private string _jsOutputNewLine = Environment.NewLine;
#else
        private string _jsOutputNewLine = String.Empty;
#endif

        #endregion

        #region accesseurs pour dispo en JS

        /// <summary>
        /// DescID du champ édité/concerné par le champ Mémo
        /// </summary>
        public int DescId
        {
            get { return _descId; }
        }

        /// <summary>
        /// FileID de l'enregistrement concerné par le champ Mémo
        /// </summary>
        public int FileId
        {
            get { return _fileId; }
        }

        /// <summary>
        /// Onglet concerné par le champ Mémo
        /// </summary>
        public int TabId
        {
            get { return _tabId; }
        }

        /// <summary>
        /// Valeur initiale à insérer dans le champ Mémo
        /// </summary>
        public String Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Feuille de style CSS personnalisée à appliquer sur le champ Mémo
        /// </summary>
        public String CustomCSS
        {
            get { return _customCSS; }
        }

        /// <summary>
        /// JS personnalisé à exécuter à l'affichage du champ
        /// </summary>
        public String CustomOnShow
        {
            get { return _customOnShow; }
        }

        /// <summary>
        /// Largeur du champ
        /// </summary>
        public String Width
        {
            get { return _divMainWidth; }
        }

        /// <summary>
        /// Hauteur du champ
        /// </summary>
        public String Height
        {
            get { return _divMainHeight; }
        }

        /// <summary>
        /// Indique si le champ Mémo doit être en mode HTML ou non
        /// </summary>
        public bool IsHTML
        {
            get { return _isHTML; }
        }

        /// <summary>
        /// Indique s'il faut afficher CKEditor en mode inline editing ou non
        /// </summary>
        public bool InlineMode
        {
            get { return _inlineMode; }
        }

        /// <summary>
        /// Indique si le champ doit être en lecture seule ou non
        /// </summary>
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

        /// <summary>
        /// Barre d'outils à afficher sur le champ Mémo
        /// </summary>
        public string ToolbarType
        {
            get { return _toolbarType; }
        }

        /// <summary>
        /// Nom de la variable JavaScript représentant l'objet eMemoEditor géré par cette fenêtre
        /// </summary>
        public string MemoEditorJSVarName
        {
            get
            {
                return _memoEditorJsVarName;
            }
        }

        /// <summary>
        /// ID de la frame parente de la fenêtre
        /// </summary>
        public string ParentFrameId
        {
            get
            {
                return _parentFrameId;
            }
        }

        /// <summary>
        /// Code JS généré pour l'initialisation
        /// </summary>
        public String InitJSOutput
        {
            get { return _sbInitJSOutput.ToString(); }
        }

        /// <summary>
        /// Code JS généré après initialisation
        /// </summary>
        public String EndJSOutput
        {
            get { return _sbEndJSOutput.ToString(); }
        }

        /// <summary>
        /// true pour afficher un formulaire d'upload de fichiers à insérer dans le champ Mémo de la fenêtre (ex : injection CSS)
        /// </summary>
        public bool UploadContentEnabled
        {
            get { return _bUploadContentEnabled; }
        }

        /// <summary>
        /// Libellé à afficher à côté du champ d'upload
        /// </summary>
        public string UploadContentLabel
        {
            get { return _strUploadContentLabel; }
        }

        /// <summary>
        /// Extensions de fichiers acceptées pour l'upload, séparées par ; (ex : "css;txt")
        /// </summary>
        public string UploadContentFileFilter
        {
            get { return _strUploadContentFileFilter; }
        }

        /// <summary>
        /// Taille limite acceptée pour le fichier uploadé, en octets
        /// </summary>
        public int UploadContentLimit
        {
            get { return _nUploadContentLimit; }
        }

        /// <summary>
        /// Si ce paramètre est à true, le contenu uploadé sera ajouté au contenu existant du champ Mémo.
        /// S'il est à false, le contenu uploadé remplacera le contenu actuel du champ Mémo
        /// </summary>
        public bool UploadContentAppend
        {
            get { return _bUploadContentAppend; }
        }

        #endregion

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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
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

            #endregion


            #region ajout des js

            PageRegisters.AddScript("grapesjs/grapes.min");
            PageRegisters.AddScript("grapesjs/grapesjs-plugin-ckeditor.min");
            PageRegisters.AddScript("grapesjs/grapesjs-blocks-basic.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-newsletter.min");
            PageRegisters.AddScript("grapesjs/grapesjs-preset-webpage.min");
            PageRegisters.AddScript("ckeditor/ckeditor");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eGrapesJSEditor");
            PageRegisters.AddScript("eMemoEditor");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eMain");


            #endregion

            //DateTime debut = DateTime.Now; // Utilisé pour tracer les temps d'execution, à conserver pour d'éventuels tests de performance
            Boolean onError = false;
            String action = String.Empty;
            XmlNode detailsNode = null;
            XmlDocument _xmlResult = new XmlDocument();
            Boolean bFromDB = false;

            #region Initialisation


            try
            {

                if (Request.Form["DescId"] != null)
                    _descId = int.Parse(Request.Form["DescId"].ToString());
                if (Request.Form["FileId"] != null)
                    _fileId = int.Parse(Request.Form["FileId"].ToString());
                if (Request.Form["TabId"] != null)
                    _tabId = int.Parse(Request.Form["TabId"].ToString());
                if (Request.Form["Title"] != null)
                    _title = HttpUtility.UrlDecode(Request.Form["Title"].ToString());
                
                //KJE, tâche #2 551: si l'extension envoie de Mail par Mapp est activée, on masque les champs de fusion
                eExtension eRegisteredExt = eExtension.GetExtensionByCode(_pref, EXTENSION.EXTERNALMAILING.ToString());
                if (eRegisteredExt != null && eRegisteredExt.Status == EXTENSION_STATUS.STATUS_READY)
                    _showOnlyMergedFields = false;
                else if (Request.Form["showOnlyMerged"] != null)
                    _showOnlyMergedFields = Request.Form["showOnlyMerged"].ToString() == "1";

                if (Request.Form["ParentFrameId"] != null)
                    _parentFrameId = Request.Form["ParentFrameId"].ToString();
                if (Request.Form["ParentEditorJsVarName"] != null)
                    _parentEditor = Request.Form["ParentEditorJsVarName"].ToString();
                if (Request.Form["EditorJsVarName"] != null)
                    _memoEditorJsVarName = Request.Form["EditorJsVarName"].ToString();
                if (Request.Form["IsHTML"] != null)
                    _isHTML = Request.Form["IsHTML"].ToString().Equals("1");
                if (Request.Form["showXrmFormularBtn"] != null)
                    _showXrmFormularBtn = Request.Form["showXrmFormularBtn"].ToString().Equals("1");
                if (Request.Form["InlineMode"] != null)
                    _inlineMode = Request.Form["InlineMode"].ToString().Equals("1");
                if (Request.Form["ReadOnly"] != null)
                    _readOnly = Request.Form["ReadOnly"].ToString().Equals("1");
                if (Request.Form["EditorType"] != null)
                    _editorType = Request.Form["EditorType"].ToString();
                if (Request.Form["EnableTemplateEditor"] != null)
                    _enableTemplateEditor = Request.Form["EnableTemplateEditor"].ToString().Equals("1");
                if (Request.Form["ToolbarType"] != null)
                    _toolbarType = Request.Form["ToolbarType"].ToString();
                if (Request.Form["divMainWidth"] != null)
                    _divMainWidth = Request.Form["divMainWidth"].ToString();
                if (Request.Form["divMainHeight"] != null)
                    _divMainHeight = Request.Form["divMainHeight"].ToString();
                if (Request.Form["CustomOnShow"] != null)
                    _customOnShow = HttpUtility.UrlDecode(Request.Form["CustomOnShow"].ToString());
                if (Request.Form["Value"] != null)
                    _value = HttpUtility.UrlDecode(Request.Form["Value"].ToString());
                else
                {
                    #region Récupération de la valeur du champ Mémo depuis la base de données
                    eudoDAL dal = eLibTools.GetEudoDAL(_pref);
                    try
                    {
                        dal.OpenDatabase();
                        eGetFieldManager gfm = new eGetFieldManager();
                        NameValueCollection nvc = new NameValueCollection();
                        nvc.Add("action", "FIELD_VALUE");
                        nvc.Add("fileId", _fileId.ToString());
                        nvc.Add("tabDescId", _tabId.ToString());
                        nvc.Add("fieldDescId", _descId.ToString());
                        nvc.Add("memoId", String.Empty);
                        gfm.Process(_pref, dal, nvc);

                        //Pour les champs type memo, on prend la valeur de db
                        if (gfm.FormatValue == FieldFormat.TYP_MEMO)
                            _value = gfm.DbValue;
                        else
                            _value = gfm.DisplayValue;

                        bFromDB = true;


                    }
                    catch (Exception exp)
                    {
                        onError = true;

                        //Avec exception
                        String sDevMsg = String.Concat("Erreur sur eMemoDialog - Récupération de la valeur du champ Mémo impossible -> : ");
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
                        dal.CloseDatabase();
                    }
                    #endregion
                }
                if (Request.Form["CustomCSS"] != null)
                    _customCSS = HttpUtility.UrlDecode(Request.Form["CustomCSS"].ToString());
                if (Request.Form["UploadContentEnabled"] != null)
                    _bUploadContentEnabled = Request.Form["UploadContentEnabled"].Equals("1");
                if (Request.Form["UploadContentLabel"] != null)
                    _strUploadContentLabel = Request.Form["UploadContentLabel"];
                if (Request.Form["UploadContentFileFilter"] != null)
                    _strUploadContentFileFilter = Request.Form["UploadContentFileFilter"];
                if (Request.Form["UploadContentLimit"] != null)
                    int.TryParse(Request.Form["UploadContentLimit"], out _nUploadContentLimit);
                if (Request.Form["UploadContentAppend"] != null)
                    _bUploadContentAppend = Request.Form["UploadContentAppend"].Equals("1");

                // Ajustement des paramètres
                String strIsHTML = (_isHTML ? "true" : "false");
                if (!_isHTML)
                    _value = HttpUtility.HtmlDecode(_value);
                String strInlineMode = (_inlineMode ? "true" : "false");

                String sFromDB = (bFromDB ? "true" : "false");

                // Création des contrôles
                if (_bUploadContentEnabled)
                {
                    eButtonCtrl btnSend = new eButtonCtrl(eResApp.GetRes(_pref, 944), eButtonCtrl.ButtonType.GREEN);
                    btnSend.OnClick = "document.getElementById('memoDlgUploadForm').submit();";
                    memoDlgUploadBtnDiv.Controls.Add(btnSend);
                }

                // Paramétrage de la configuration du champ Mémo
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("var bHTML = ").Append(strIsHTML).Append(";");
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("var bCompactMode = false;");


                eMemoDialogEditorValue.InnerText = _value;
                   
                // Création et instanciation du champ Mémo
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject = new eMemoEditor(")
                    .Append(_jsOutputNewLine).Append("'eMemoDialogEditor',")
                    .Append(_jsOutputNewLine).Append("bHTML,")
                    .Append(_jsOutputNewLine).Append("document.getElementById('eMemoDialogEditorContainer'),")
                    .Append(_jsOutputNewLine).Append("null,") // eFieldEditor parent = aucun
                    .Append(_jsOutputNewLine).Append("GetText(document.getElementById('eMemoDialogEditorValue')),")
                    //.Append(_jsOutputNewLine).Append("(document.getElementById('eMemoDialogEditorValue').innerText)?document.getElementById('eMemoDialogEditorValue').innerText:'"+  this.Value.Replace("'","\'"). +"',") // #36751 CRU : On met la valeur exacte et non un "InnerHtml" ou "InnerText" de eMemoDialogEditorValue
                    .Append(_jsOutputNewLine).Append("bCompactMode,")
                    .Append(_jsOutputNewLine).Append("'eMemoDialogEditorObject'")
                .Append(_jsOutputNewLine).Append(");")
                .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.title = '").Append(_title.Replace("'", @"\")).Append("';")
                .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.descId = '").Append(_descId).Append("';")
                .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.fileId = '").Append(_fileId).Append("';")
                .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.showXrmFormularBtn = '").Append(_showXrmFormularBtn ? "1" : "0").Append("';");//KJE, Demande #45 664

                if (_showOnlyMergedFields)
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.showOnlyMerged = true").Append(";");//KJE tâche 2 334
                else
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.showOnlyMerged = false").Append(";");
                // Mode inline editing
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.inlineMode = ").Append(strInlineMode).Append(";");

                // Mode fullscreen : pas de bouton FullScreen dans la barre d'outils HTML ou texte
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.isFullScreen = true;");

                // Le champ Mémo étant le seul contrôle de cette fenêtre, on met le focus dessus
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.focusOnShow = true;");

                // Et on empêche son affichage initial en mode compact, même si la surface d'affichage est jugée insuffisante (on est en plein écran)
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.preventCompactMode = true;");

                // Pas de mise à jour du champ Mémo lors de la sortie du champ
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.updateOnBlur = false;");

                // Mode lecture seule ou écriture
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.readOnly = ").Append(_readOnly ? "true" : "false").Append(";");

                // Type de champ Mémo
                if (_editorType.Length > 0)
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.editorType = '").Append(_editorType).Append("';");

                // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
                // Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.enableTemplateEditor = ").Append(_enableTemplateEditor ? "true" : "false").Append(";");

                // Barre d'outils à afficher
                if (_toolbarType.Length > 0)
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.toolbarType = '").Append(_toolbarType).Append("';");

                // Mise à jour de la configuration de base du champ Mémo (mode HTML) avec les propriétés ci-dessus
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("if (bHTML) {")
                    .Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.setSkin('eudonet');");

                // L'affichage de la barre de statut se fait par activation/désactivation de plugin. On gère ce comportement via une méthode spécifique de memoEditor
                // car elle ne fait pas partie de celles proposées par l'objet config de CKEditor
                if (_toolbarType == "mail" || _toolbarType == "formular" || _toolbarType == "automation" || _toolbarType == "mailSubject")
                {
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.setStatusBarEnabled(true);");
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("if(top) {eMemoDialogEditorObject.mergeFields = top.window['_medt']['" + this._parentEditor + "'].mergeFields;}");
                }
                ;
                if (_toolbarType == "mailing" || _toolbarType == "mailingtemplate" || _toolbarType == "mailtemplate")
                {
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.setStatusBarEnabled(true);");
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("if(top) { eMemoDialogEditorObject.mergeFields = top.window['_medt']['" + this._parentEditor + "'].mergeFields;}");
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("if(top) {eMemoDialogEditorObject.oTracking = top.window['_medt']['" + this._parentEditor + "'].oTracking;}");
                    
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("if(top) {eMemoDialogEditorObject.oMergeHyperLinkFields = top.window['_medt']['" + this._parentEditor + "'].oMergeHyperLinkFields;}");
                }

                // Feuille de style CSS personnalisée à injecter à l'instanciation du champ, si demandé
                _sbInitJSOutput.Append(_jsOutputNewLine)
                    .Append("if (document.getElementById('eMemoDialogEditorCustomCSS')) ")
                    .Append(" eMemoDialogEditorObject.setCss(document.getElementById('eMemoDialogEditorCustomCSS').value); ")
                    .Append(_jsOutputNewLine).Append("}");

                // Mise à jour de la configuration commune (HTML ET texte)
                int memoWidthNum = eLibTools.GetNum(_divMainWidth);
                if (memoWidthNum == 0)
                {
                    // Cas que ca ne soit pas un chiffre
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.config.width = '").Append(_divMainWidth).Append("';");
                }
                else
                {
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.config.width = '").Append(memoWidthNum).Append("';");
                }


                int memoHeightNum = eLibTools.GetNum(_divMainHeight);
                if (memoHeightNum == 0)
                {
                    // Cas que ca ne soit pas un chiffre
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.config.height = '").Append(_divMainHeight).Append("';");
                }
                else
                {
                    // TODO - Ckeditor ne gére pas la taille des ces barres en haut et en bas, du coup nous depassons de son conteneur...
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.config.height = '' + (");

                    _sbInitJSOutput.Append(memoHeightNum - 56);
                    if (_bUploadContentEnabled)
                        _sbInitJSOutput.Append(" - document.getElementById('memoDlgUploadDiv').clientHeight");

                    _sbInitJSOutput.Append(") + '';");
                }

                // On affecte une propriété childMemoEditor de l'objet eMemoEditor appelant (parent) avec un pointeur vers l'objet eMemoEditor
                // instancié par cette page/fenêtre eMemoDialog.aspx, afin que l'objet parent puisse communiquer avec l'objet enfant de cette page.
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("var oParentMemoEditor = null;");

                if (!String.IsNullOrEmpty(_memoEditorJsVarName))
                {
                    // Premier cas : si on a indiqué l'ID d'une iframe sur laquelle rechercher l'objet eMemoEditor, on commence par l'analyser
                    if (!String.IsNullOrEmpty(_parentFrameId))
                    {
                        // Dans la plupart des cas, le champ Mémo appelant aura été instancié puis ajouté à un tableau indexé par son DescId.
                        // On tente donc de le récupérer par ce biais
                        String memoJsVarNameOnParentWindow = _memoEditorJsVarName;

                        _sbInitJSOutput.
                            Append(_jsOutputNewLine).Append("if (parent.document.getElementById('").Append(_parentFrameId).Append("') && parent.document.getElementById('").Append(_parentFrameId).Append("').contentWindow) {")
                                .Append(_jsOutputNewLine).Append("oParentMemoEditor = parent.document.getElementById('").Append(_parentFrameId).Append("').contentWindow.").Append(memoJsVarNameOnParentWindow).Append(";")
                                .Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {")
                                .Append("oEditor = parent.document.getElementById('").Append(_parentFrameId).Append("').contentWindow.document.").Append(memoJsVarNameOnParentWindow).Append(";")
                                .Append(_jsOutputNewLine).Append("}");
                        // Et si cela échoue, on teste directement avec le nom de variable passé en paramètre
                        _sbInitJSOutput.
                            Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {");
                        _sbInitJSOutput.Append(_jsOutputNewLine).Append("oParentMemoEditor = parent.document.getElementById('").Append(_parentFrameId).Append("').contentWindow.").Append(_memoEditorJsVarName).Append(";")
                            .Append(_jsOutputNewLine).Append("}")
                            .Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {")
                            .Append("oParentMemoEditor = parent.document.getElementById('").Append(_parentFrameId).Append("').contentWindow.document.").Append(memoJsVarNameOnParentWindow).Append(";")
                            .Append(_jsOutputNewLine).Append("}")
                       .Append("}");
                    }
                    // Second cas : on recherche également l'objet sur le document parent direct du DOM (ex : mémo instancié dans une eModalDialog, parent = eMain.aspx et
                    // non l'iframe de l'eModalDialog), on recherche l'objet sur l'iframe dont l'ID  (parentFrameId) est passé en paramètre
                    _sbInitJSOutput
                        .Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {")
                            .Append(_jsOutputNewLine).Append("if (parent.").Append(_memoEditorJsVarName).Append(") {")
                            .Append(_jsOutputNewLine).Append("oParentMemoEditor = parent.").Append(_memoEditorJsVarName).Append(";")
                            .Append(_jsOutputNewLine).Append("}")
                        .Append(_jsOutputNewLine).Append("}");
                    // Troisième cas : on teste directement si on se trouve en popup via isPopup() et on recherche directement parent.eModFile
                    _sbInitJSOutput
                        .Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {")
                            .Append(_jsOutputNewLine).Append("if (parent.isPopup && parent.isPopup() && parent.eModFile && parent.eModFile.contentWindow) {")
                                .Append(_jsOutputNewLine).Append("oParentMemoEditor = parent.eModFile.contentWindow.").Append(_memoEditorJsVarName).Append(";")
                                .Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) {")
                                .Append(_jsOutputNewLine).Append("      oParentMemoEditor = parent.eModFile.contentWindow.document.").Append(_memoEditorJsVarName).Append(";")
                                .Append(_jsOutputNewLine).Append("}")
                            .Append(_jsOutputNewLine).Append("}")
                        .Append(_jsOutputNewLine).Append("}");
                }

                if (_toolbarType == "mailing" || _toolbarType == "mailingtemplate" || _toolbarType == "mailtemplate" || _toolbarType == "formular")
                {
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("if (!oParentMemoEditor) { oParentMemoEditor = top.window['_medt']['" + this._parentEditor + "'];}").AppendLine();                    
                }

                // Affectation si l'objet a été trouvé
                // On ajoute également un lien enfant -> parent, qui pourra notamment être utilisé pour remonter du champ Mémo de cette fenêtre vers son parent, pour redescendre
                // ensuite vers l'objet eModalDialog qui affiche cette page (eMemoEditorDialogObject.parentMemoEditor.childMemoDialog) et agir dessus (ex : l'agrandir)
                // Attention : si l'affectation n'est pas faite, le champ Mémo de la fenêtre enfant ne pourra pas interagir avec le champ Mémo parent...
                _sbInitJSOutput.Append("if (oParentMemoEditor) { oParentMemoEditor.setChildMemoEditor(eMemoDialogEditorObject); eMemoDialogEditorObject.setParentMemoEditor(oParentMemoEditor); }");

                // Définition d'une fonction à déclencher après affichage si précisé
                // Attention, on écrit directement le contenu du paramètre passé ici (soit un nom de fonction, soit une syntaxe type "function() { }")
                if (_customOnShow.Length > 0)
                {
                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("var oCustomOnShowFct = ").Append(_customOnShow).Append(";");


                    _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.customOnShow = oCustomOnShowFct;");
                }

                // Création du <textarea> dans le container passé à l'initialisation de eMemoEditor et affichage
                // Uniquement si l'objet childMemoEditor a pu être renseigné ci-dessous ; sinon, la fenêtre parente ne pourra pas récupérer le contenu de la fenêtre enfant
                //_sbInitJSOutput.Append(_jsOutputNewLine).Append("if (oParentMemoEditor && oParentMemoEditor.childMemoEditor) { eMemoDialogEditorObject.show(); }");
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.show();");

                //CNA                
                if (memoHeightNum != 0 && memoWidthNum != 0)
                {
                    //_sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.resize(" + memoWidthNum + " - 4," + memoHeightNum + " - 56);");
                }


                String strDisplayToolBar = "true";
                // Sur la fenêtre d'ajout de CSS, la barre d'outils ne comporte pas de bouton. Inutile de l'afficher
                if (_bUploadContentEnabled)
                {
                    strDisplayToolBar = "false";
                }

                // En mode fiche (non zoomé), on n'affiche pas la barre d'outils, qu'on affiche alors en mode Zoom
                // On gère l'affichage de la barre d'outils via une méthode spécifique de memoEditor qui permet d'affecter plusieurs propriétés
                _sbInitJSOutput.Append(_jsOutputNewLine).Append("eMemoDialogEditorObject.setToolBarDisplay(").Append(strDisplayToolBar).Append(", true);");

                #region Gestion de l'upload
                if (memoDlgFile.PostedFile != null)
                {
                    if (memoDlgFile.PostedFile.ContentLength > 0)
                    {
                        HttpPostedFile myFile = memoDlgFile.PostedFile;

                        // Vérification de l'extension du fichier envoyé
                        bool bExtensionAllowed = false;
                        string[] strAllowedExtensions = _strUploadContentFileFilter.Split(';');
                        int nIndex = 0;
                        while (nIndex < strAllowedExtensions.Length && !bExtensionAllowed)
                        {
                            bExtensionAllowed = myFile.FileName.ToLower().EndsWith(String.Concat(".", strAllowedExtensions[nIndex]));
                            nIndex++;
                        }
                        if (!bExtensionAllowed)
                            this.lblError.Text = eResApp.GetRes(_pref, 1545);
                        else
                        {
                            // Vérification de sa taille
                            int nFileLen = myFile.ContentLength;
                            if (nFileLen > _nUploadContentLimit)
                                this.lblError.Text = eResApp.GetRes(_pref, 1537);
                            // Lecture du flux et stockage
                            else if (nFileLen > 0)
                            {
                                TextReader tr = new StreamReader(myFile.InputStream);

                                _value = String.Concat(_bUploadContentAppend ? _value : "", Environment.NewLine,
                                       "/*--------- ", eResApp.GetRes(_pref, 426), ": ", myFile.FileName, " ---------*/",
                                         Environment.NewLine, tr.ReadToEnd(), Environment.NewLine,
                                       "/*--------- ", eResApp.GetRes(_pref, 271), ": ", myFile.FileName, " ---------*/");

                                eMemoDialogEditorValue.InnerText = _value;
                            }
                        }
                    }
                }

                _sbEndJSOutput.Append("top.setWait(false);");

                #endregion
            }
            catch (Exception exp)
            {
                onError = true;

                //Avec exception
                String sDevMsg = String.Concat("Erreur sur eMemoDialog - Page_Load catch global= -> : ");
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
            //eModelTools.EudoTraceLog("Apres res:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
            //debut = DateTime.Now;


            //eModelTools.EudoTraceLog("Apres connection:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
            //debut = DateTime.Now;


            //eModelTools.EudoTraceLog("Catalog chargé:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
            //debut = DateTime.Now;

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

            //eModelTools.EudoTraceLog("Rendu terminé:" + ((TimeSpan)(DateTime.Now - debut)).TotalMilliseconds, _pref);
        }


    }
}