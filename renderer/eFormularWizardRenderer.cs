using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.renderer;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.Script.Serialization;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer de création des formulaires...
    /// </summary>
    public class eFormularXrmWizardRenderer : eMainFileRenderer
    {
        /// <summary>
        /// Nombre d'etape de l'assistant
        /// </summary>
        public static Int32 TOTAL_STEPS = 3;

        /// <summary>Hauteur de l'écran</summary>
        private Int32 _iheight = eConst.DEFAULT_WINDOW_WIDTH;

        /// <summary>Hauteur de la fenêtre parente</summary>
        private Int32 _iheightParent = eConst.DEFAULT_WINDOW_WIDTH;

        ///// <summary>Largeur de l'écran</summary>
        // private Int32 _iwidth = eConst.DEFAULT_WINDOW_HEIGHT;



        /// <summary>Formulaire généré/modifé par le wizard</summary>
        private eFormular _oFormular = null;

        /// <summary> Passer des parametres js à l'objet formular.js </summary>
        private StringBuilder _sbJavaScript = null;

        /// <summary> les champs de fusion </summary>
        private String _strMergeFields = String.Empty;

        /// <summary>Table du formulaire ++/cible etendu</summary>
        private Int32 _iTab;

        /// <summary>Fichier parent ++/cible etendu</summary>
        private Int32 _iParentFileId;

        #region Constructeurs

        /// <summary>
        /// Constructeur privé
        /// </summary>
        /// <param name="ePref">Preferences Utilisateur</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="tab">Signet en cours ++/cible etendue</param>
        /// <param name="nHeightParent">hauteur de la fenêtre parente</param>
        private eFormularXrmWizardRenderer(ePref ePref, Int32 tab, Int32 parentFileId, Int32 formularId, Int32 height, Int32 nHeightParent)
        {


            _iheight = height;
            _iheightParent = nHeightParent;
            // _iwidth = width;
            Pref = ePref;
            _iTab = tab;
            _iParentFileId = parentFileId;
            _rType = RENDERERTYPE.FormularWizard;
            _nFileId = formularId;
            _oFormular = new eFormular(formularId, ePref, nEvtFileId: parentFileId);
            _oFormular.Tab = tab;
        }

        #endregion

        #region Méthodes Statique pour le rendu

        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant de creétion des formulaire et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="tab">Signet d'origine</param>
        /// <param name="height">hauteur de l'interface</param>
        /// <param name="heightParent">hauteur de la fenetre parente</param>      
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eFormularXrmWizardRenderer GetFormularXrmWizardRenderer(ePref ePref, Int32 tab, Int32 parentFileId, Int32 formularId, Int32 height, Int32 heightParent)
        {
            return new eFormularXrmWizardRenderer(ePref, tab, parentFileId, formularId, height, heightParent);
        }

        #endregion

        #region Méthodes héritées de eRenderer, process de création du renderer

        /// <summary>
        /// Initialisation du renderer en cas d'ajout d'un nouveau formulaire
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            _sbJavaScript = new StringBuilder();

            _pgContainer.ID = "wizard";

            // Div de champ caché
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _divHidden.ID = String.Concat("hv_", TableType.FORMULARXRM.GetHashCode());

            //Ajout du div caché
            _pgContainer.Controls.Add(_divHidden);

            if (_oFormular != null && !_oFormular.Init())
            {
                _eException = _oFormular.ErrorException;
                return false;
            }

            loadMergeFields();
            return true;
        }

        /// <summary>
        /// Construit le corps de l'assistant, composé d'un div de param et d'un div par étape d'assistant
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            //header (Etapes)
            Panel Header = this.BuildHeader();
            Header.ID = "formDiv";

            //body (assistant)
            Panel Editor = this.BuildBody();

            //footer (boutons)
            this.PgContainer.Controls.Add(Header);
            this.PgContainer.Controls.Add(Editor);

            return true;
        }


        /// <summary>
        /// Ajout des traitements post build du renderer 
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            // Ajout les ids des champs memo dans la div cachée
            HtmlInputHidden memoDescIds = new HtmlInputHidden();
            memoDescIds.ID = String.Concat("memoIds_", TableType.FORMULARXRM.GetHashCode());
            memoDescIds.Value = String.Join(";", _sMemoIds.ToArray());
            _divHidden.Controls.Add(memoDescIds);

            // Ajout d'attributs pour la div parente
            HtmlGenericControl fileDiv = new HtmlGenericControl("div");

            fileDiv.ID = String.Concat("fileDiv_", TableType.FORMULARXRM.GetHashCode());
            fileDiv.Attributes.Add("fid", _oFormular.FormularId.ToString());
            fileDiv.Attributes.Add("ftrdr", eConst.eFileType.FILE_MODIF.GetHashCode().ToString());

            _pgContainer.Controls.Add(fileDiv);

            //on appelle serialize avant End
            JavaScriptSerialize(_sbJavaScript);
            _oFormular.Dispose();

            // On crée un bloc JavaScript contenant les champs disponibles pour la fusion
            HtmlGenericControl javaScript = new HtmlGenericControl("script");
            javaScript.Attributes.Add("type", "text/javascript");
            javaScript.Attributes.Add("language", "javascript");

            javaScript.InnerHtml = _sbJavaScript.ToString();
            fileDiv.Controls.Add(javaScript);

            return true;
        }

        #endregion

        #region Construction du header de la page

        /// <summary>
        /// Construit la partie Haute de l'assistant reporting
        /// Contenant les boutons et libellés des différentes étapes.
        /// </summary>
        /// <returns>Div conteneur de la partie haute de l'assistant</returns>
        private Panel BuildHeader()
        {
            Panel header = new Panel();
            header.ID = "wizardheader";
            header.CssClass = "wizardheader";
            Panel stepGroup = new Panel();

            //TODOMOU changer la css de placement
            //tepGroup.CssClass = String.Concat("states_placement", "stpPlcmtChrt");

            stepGroup.CssClass = "states_placement";
            Int32 nActiveStep = 1;

            for (Int32 i = 1; i <= TOTAL_STEPS; i++)
            {
                stepGroup.Controls.Add(BuildStepDiv(i, i == nActiveStep));
                if (i < TOTAL_STEPS)
                    stepGroup.Controls.Add(BuildSeparatorDiv());
            }

            header.Controls.Add(stepGroup);
            return header;
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected virtual Panel BuildStepDiv(Int32 step, Boolean isActive)
        {
            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();
            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();

            stepBloc.Attributes.Add("onclick", String.Concat("StepClick('", step, "');"));

            numberBloc.Controls.Add(new LiteralControl(step.ToString()));
            Label lbl = new Label();


            switch (step)
            {
                case 1:
                    lbl.Text = eResApp.GetRes(Pref, 6699);
                    break;
                case 2:
                    lbl.Text = eResApp.GetRes(Pref, 6698);
                    break;
                case 3:
                    lbl.Text = eResApp.GetRes(Pref, 8007);
                    break;
                default:
                    lbl.Text = String.Concat(eResApp.GetRes(Pref, 1617) + " : ", step);
                    break;
            }

            //demande 78 213 - [REGRESSION][10.512] :Étapes dans assistant formulaire
            if (this._ePref.ThemeXRM.Version > 1)
                stepBloc.CssClass = isActive ? "state_grp-current" : "state_grp";
            else
                stepBloc.CssClass = isActive ? "state_grp-current" : _oFormular == null ? "state_grp" : "state_grp-validated";

            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);

            return stepBloc;
        }

        /// <summary>
        /// Construit le bloc de séparation entre deux boutons d'étape.
        /// </summary>
        /// <returns>Panel (div) de Séparation entre deux étapes</returns>
        private Panel BuildSeparatorDiv()
        {
            Panel sepBloc = new Panel();
            sepBloc.CssClass = "state_sep";
            return sepBloc;
        }

        /// <summary>
        /// Génere du javascript nécessaire pour la fiche mail : obj merge field, ...
        /// </summary>
        /// <param name="js">Code javascript a ajouter dans le block script</param>
        private void AppendScript(String js)
        {
            this._sbJavaScript.AppendLine();
            this._sbJavaScript.Append(js);
        }

        /// <summary>
        /// On charge les champ de fusions depuis la base
        /// </summary>
        private void loadMergeFields()
        {

            #region Champs de fusion

            String strErr = String.Empty;

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            try
            {
                dal.OpenDatabase();

                if (_iTab == 0)
                    throw new Exception("Invalide nTabFrom = " + _iTab);

                eTableLiteMailing table = eLibTools.GetTableInfo(dal, _iTab, eTableLiteMailing.Factory(Pref));

                //Tous les champs de fusion
                List<int> AllMergeFields = eLibTools.GetMergeFieldsMailingList(dal, Pref, table, table.ProspectEnabled, bFormular: true);

                //On filtre la  liste par rapport aux droits de visu
                List<int> AllowedMergeFields = new List<int>(eLibTools.GetAllowedFieldsFromDescIds(Pref, Pref.User, String.Join(";", AllMergeFields.ToArray()), false).Keys);

                //on construit la liste des champs
                eLibTools.GetMergeFieldsData(dal, Pref, Pref.User, AllowedMergeFields, null, null, null, null, null, null, out _strMergeFields);
            }
            catch (Exception ex)
            {
                throw new Exception("eFormularWizardRenderer::AppendMergeFields:", ex);
            }
            finally
            {
                dal.CloseDatabase();
            }

            #endregion
        }

        /// <summary>
        /// On ajoute les champs de fusion dans le script à générer
        /// </summary>
        /// <param name="JsVarName">Variable js de l'objet</param>
        /// <param name="appendBtn">on ajoute ou pas le bouton valider dans la liste</param>
        private void AppendMergeFields(String JsVarName, Boolean appendBtn)
        {
            String js = _strMergeFields.ToString();
            //TODOMOU Ajouter le bouton valider lors de l appel a eLibTools.GetMergeFieldsData
            if (appendBtn)
                //on retire les acolades et on ajoute le bouton
                js = String.Concat("{ \"", eResApp.GetRes(Pref, 6709), "\" :\"000;submit;button;1;true;0\",", js.Substring(1, js.Length - 2), " }");

            AppendScript(String.Concat(" var ", JsVarName, "=", js, ";"));
        }



        #endregion

        #region construction du body

        #region Bloc de construction principal

        /// <summary>
        /// Construit le div englobant le contenu de l'éditeur et y ajoute le contenu
        /// </summary>
        private Panel BuildBody()
        {
            Panel wizardBody = new Panel();
            wizardBody.ID = "wizardbody";
            wizardBody.CssClass = "wizardbody";

            //Si la hauteur de l'écran est <900, la barre d'outil de ck est sur 2 lignes ce qui change le calcul de la hauteur optimal
            if (_iheightParent > 900)
                wizardBody.Style["height"] = string.Format("{0}px", (Int32)(.85 * this._iheight));
            else
                wizardBody.Style["height"] = string.Format("{0}px", (Int32)(.75 * this._iheight));

            for (int i = 1; i <= TOTAL_STEPS; i++)
                wizardBody.Controls.Add(BuildBodyStep(i));

            return wizardBody;
        }

        /// <summary>
        /// Construit le bloc div d'une étape donnée de l'assitant et le retourne
        /// </summary>
        /// <param name="step">Numéro d'étape de l'assistant</param>
        /// <returns>Panel(div) de l'étape demandée</returns>
        protected virtual Panel BuildBodyStep(Int32 step)
        {
            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            switch (step)
            {
                case 1:
                    #region Première Page
                    pEditDiv.Controls.Add(this.BuildFormularBodyPanel());
                    #endregion
                    break;
                case 2:
                    #region Seconde Page
                    pEditDiv.Controls.Add(this.BuildResponseOptionsPanel());
                    #endregion
                    break;
                case 3:
                    #region Troisieme Page
                    pEditDiv.Controls.Add(this.BuildFormularSettingsPanel());
                    #endregion
                    break;
            }

            return pEditDiv;
        }

        /// <summary>
        /// Retourne un panel de l'étape de creation de fomulaire
        /// </summary>
        /// <returns></returns>
        private Control BuildFormularBodyPanel()
        {
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "form-body-step";

            //Le sous-titre de l'etape 1
            HtmlGenericControl stepLabel = new HtmlGenericControl("label");
            stepLabel.Attributes.Add("class", "title-step");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6702); // Créer votre formulaire
            stepContainer.Controls.Add(stepLabel);

            //Corps de formulaire
            Panel stepBody = new Panel();
            EdnWebControl ednStepBody = new EdnWebControl() { WebCtrl = stepBody, TypCtrl = EdnWebControl.WebControlType.PANEL };
            stepBody.ID = "BodyMemoId";
            stepBody.CssClass = "body-step";


            if (_iheightParent > 900)
                stepBody.Style["height"] = string.Format("{0}px", (Int32)(this._iheight * .70));
            else
                stepBody.Style["height"] = string.Format("{0}px", (Int32)(this._iheight * .60));

            stepBody.Style["width"] = "100%";
            // Editor html
            stepBody.Attributes.Add("html", "1");

            // Les champs de fusion
            String JsVarName = "oMergeFieldsBody";
            stepBody.Attributes.Add("mergefieldsjsvarname", JsVarName);
            AppendMergeFields(JsVarName, true);

            // Une fois le champs memo est chargé, on injecte la css
            //'instanceReady' : 'editor.injectCSS(oFormular.GetParam(\"bodycss\"))',
            String JsEventListeners = "memoBodyListeners";
            stepBody.Attributes.Add("listeners", JsEventListeners);
            AppendScript(String.Concat("var ", JsEventListeners, " = {'instanceReady' : \"function(event){ oFormular.InjectCSS('edtBodyMemoId','bodycss');}\", 'data-changed' : \"oFormular.update\" };"));
            //Mode Inline
            stepBody.Attributes.Add("inlinemode", "0");

            // type de champ Mémo et de barre d'outils
            // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
            stepBody.Attributes.Add("editortype", "formular");
            stepBody.Attributes.Add("enabletemplateeditor", "0"); // Backlog #409 - grapesjs non proposé sur cet écran v1 pour l'instant
            //stepBody.Attributes.Add("enabletemplateeditor", eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor) ? "1" : "0"); // Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)
            stepBody.Attributes.Add("toolbartype", "formular");

            // DescID du fichier depuis lequel on utilise le formulaire
            stepBody.Attributes.Add("descid", _iTab.ToString());

            // Pour pouvoir instancier le champ memo coté js avec initMemoFields
            this.MemoIds.Add(stepBody.ID);

            //Remplit stepBody par la valeur du corps du formulaire
            GetHTMLMemoControl(ednStepBody, _oFormular.Body);

            stepContainer.Controls.Add(stepBody);

            return stepContainer;
        }

        /// <summary>
        /// Crée le panel de l'etape options de reponse
        /// </summary>
        /// <returns></returns>
        private Control BuildResponseOptionsPanel()
        {
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "form-body-step";

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "ul-step");

            #region Le sous-titre de l'etape 2

            HtmlGenericControl stepLabel = new HtmlGenericControl("label");
            stepLabel.Attributes.Add("class", "title-step");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6730);

            stepContainer.Controls.Add(stepLabel);

            #endregion

            #region Url redirect

            HtmlGenericControl li = new HtmlGenericControl("li");

            //L'option de redirection est activé sauf s'il y ait une url 
            Boolean bUrlRedirectEnabled = _oFormular.SubmissionRedirectUrl.Length > 0;

            HtmlGenericControl cbUrlRedirect = new HtmlGenericControl("input");
            cbUrlRedirect.ID = "cbo-url-id";
            cbUrlRedirect.Attributes.Add("type", "radio");
            if (bUrlRedirectEnabled)
                cbUrlRedirect.Attributes.Add("checked", "true"); //bUrlRedirectEnabled ? "true" : "false");
            cbUrlRedirect.Attributes.Add("name", "response");

            li.Controls.Add(cbUrlRedirect);

            HtmlGenericControl cbUrlLabel = new HtmlGenericControl("label");
            cbUrlLabel.Attributes.Add("for", "cbo-url-id");
            cbUrlLabel.InnerHtml = eResApp.GetRes(Pref, 6729);

            li.Controls.Add(cbUrlLabel);

            HtmlGenericControl inputRedirectUrl = new HtmlGenericControl("input");
            inputRedirectUrl.ID = "input-url-id";
            inputRedirectUrl.Attributes.Add("type", "text");
            inputRedirectUrl.Attributes.Add("value", _oFormular.SubmissionRedirectUrl.Length > 0 ? _oFormular.SubmissionRedirectUrl : "http://"); //;TODO MOU http ?
            li.Controls.Add(inputRedirectUrl);

            ul.Controls.Add(li);
            #endregion

            #region Afficher le message suivant

            li = new HtmlGenericControl("li");

            HtmlGenericControl cbDisplayMsg = new HtmlGenericControl("input");
            cbDisplayMsg.ID = "cbo-body-id";
            cbDisplayMsg.Attributes.Add("type", "radio");
            if (!bUrlRedirectEnabled)
                cbDisplayMsg.Attributes.Add("checked", "true"); //!bUrlRedirectEnabled ? "true" : "false");
            cbDisplayMsg.Attributes.Add("name", "response");

            li.Controls.Add(cbDisplayMsg);

            HtmlGenericControl cbDisplayMsgLabel = new HtmlGenericControl("label");
            cbDisplayMsgLabel.Attributes.Add("for", "cbo-body-id");
            cbDisplayMsgLabel.InnerHtml = eResApp.GetRes(Pref, 6731);

            li.Controls.Add(cbDisplayMsgLabel);

            ul.Controls.Add(li);
            #endregion

            #region Corps de réponse aprés soumission

            li = new HtmlGenericControl("li");

            //Corps de réponse
            Panel stepBody = new Panel();
            EdnWebControl ednStepBody = new EdnWebControl() { WebCtrl = stepBody, TypCtrl = EdnWebControl.WebControlType.PANEL };
            stepBody.ID = "SubmitMemoId";
            stepBody.CssClass = "body-step";


            if (_iheightParent > 900)
                stepBody.Style["height"] = string.Format("{0}px", (Int32)(this._iheight * .65));
            else
                stepBody.Style["height"] = string.Format("{0}px", (Int32)(this._iheight * .50));

            stepBody.Style["width"] = "100%";
            // Editor html
            stepBody.Attributes.Add("html", "1");

            // Les champs de fusion
            String JsVarName = "oMergeFieldSubmit";
            stepBody.Attributes.Add("mergefieldsjsvarname", JsVarName);
            AppendMergeFields(JsVarName, false);


            //Mode Inline
            stepBody.Attributes.Add("inlinemode", "0");

            // Une fois le champs memo est chargé, on injecte la css
            //'instanceReady' : 'editor.injectCSS(oFormular.GetParam(\"bodycss\"))',
            String JsEventListeners = "memoSubmitListeners";
            stepBody.Attributes.Add("listeners", JsEventListeners);
            AppendScript(String.Concat("var ", JsEventListeners, " = {'instanceReady' : \"function(event){oFormular.InjectCSS('edtSubmitMemoId','submitbodycss');}\", \"data-changed\" : \"oFormular.update\" };"));


            // Type de champ Mémo et de barre d'outils
            // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
            stepBody.Attributes.Add("toolbartype", "formular");
            stepBody.Attributes.Add("editortype", "formularsubmission");
            stepBody.Attributes.Add("enabletemplateeditor", "0"); // Backlog #409 - grapesjs non proposé sur cet écran v1 pour l'instant
            //stepBody.Attributes.Add("enabletemplateeditor", eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.HTMLTemplateEditor) ? "1" : "0"); // Indique si on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing)

            // Pour pouvoir instancier le champ memo coté js avec initMemoFields
            this.MemoIds.Add(stepBody.ID);

            //Remplit stepBody par la valeur du corps du formulaire
            GetHTMLMemoControl(ednStepBody, _oFormular.BodySubmission);

            li.Controls.Add(stepBody);
            ul.Controls.Add(li);
            #endregion

            stepContainer.Controls.Add(ul);

            return stepContainer;
        }

        /// <summary>
        /// Crée l'etape de saisie des parametre de formulaire
        /// </summary>
        /// <returns></returns>
        private Control BuildFormularSettingsPanel()
        {
            //TODO MOU 
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "form-body-step";

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "ul-step-settings");

            #region Sauvegarde
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-title-step");

            HtmlGenericControl stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = String.Concat(eResApp.GetRes(Pref, 617), " :");

            li.Controls.Add(stepLabel);
            ul.Controls.Add(li);

            #region Sauvegarde as

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");

            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6703); // Enregistrer le formulaire sous :
            li.Controls.Add(stepLabel);

            //AppendScript(String.Concat(
            //    "function updateUrl() {", Environment.NewLine,
            //        "var strLabel = document.getElementById('input-save-id').value;", Environment.NewLine,
            //        "if (document.getElementById('rwUrl') && document.getElementById('hRwUrl')) { document.getElementById('rwUrl').value = document.getElementById('hRwUrl').value.replace('formularlabel', top.convertStringToRewrittenUrl(strLabel)); }", Environment.NewLine,
            //    "}"
            //    )
            //);

            HtmlGenericControl inputSaveAs = new HtmlGenericControl("input");
            inputSaveAs.ID = "input-save-id";
            inputSaveAs.Attributes.Add("type", "text");
            inputSaveAs.Attributes.Add("value", _oFormular.Label);
            inputSaveAs.Attributes.Add("onkeypress", "oFormular.UpdateURL(this.value);");
            inputSaveAs.Attributes.Add("onkeydown", "oFormular.UpdateURL(this.value);");
            inputSaveAs.Attributes.Add("onkeyup", "oFormular.UpdateURL(this.value)");
            inputSaveAs.Attributes.Add("onchange", "oFormular.SaveAsOnChange();");
            li.Controls.Add(inputSaveAs);

            ul.Controls.Add(li);
            #endregion


            #endregion

            #region Options

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-title-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6704); // Options :

            li.Controls.Add(stepLabel);
            ul.Controls.Add(li);

            #region Differentes Options

            //Soumission unique
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");
            eCheckBoxCtrl cbUniqueSubmit = new eCheckBoxCtrl(_oFormular.IsUniqueSubmission, false);
            cbUniqueSubmit.ID = "unique-submit";
            cbUniqueSubmit.AddText(eResApp.GetRes(Pref, 1735)); // Soumission unique
            cbUniqueSubmit.AddClick("checkSubmitMsg(this)");

            li.Controls.Add(cbUniqueSubmit);
            ul.Controls.Add(li);

            //Message en cas de soumission multiple
            li = new HtmlGenericControl("li");
            li.Attributes.Add("id", "li-msg-submit-id");
            li.Attributes.Add("class", String.Concat("li-content-step inner-li", _oFormular.IsUniqueSubmission ? " hidden - li" : ""));
            HtmlGenericControl innerDiv = new HtmlGenericControl("div");
            innerDiv.Attributes.Add("class", "inner-div");

            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6705); // Message à afficher en cas de soumission multiple
            innerDiv.Controls.Add(stepLabel);

            HtmlGenericControl inputMsg = new HtmlGenericControl("input");
            inputMsg.ID = "input-multiple-submit";
            inputMsg.Attributes.Add("type", "text");
            inputMsg.Attributes.Add("value", _oFormular.LabelAlreadySubmit);
            innerDiv.Controls.Add(inputMsg);

            li.Controls.Add(innerDiv);
            ul.Controls.Add(li);

            //Date limite
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 1734); // Date limite :
            li.Controls.Add(stepLabel);

            //Catalogue Date 
            HtmlInputText inpt = new HtmlInputText();
            inpt.ID = "expire-date";

            // Input readonly + button
            inpt.Attributes.Add("readonly", "true");
            inpt.Attributes.Add("disabled", "true");
            inpt.Attributes.Add("value", eDate.ConvertBddToDisplay(Pref.CultureInfo, _oFormular.ExpireDate.Value.ToString("dd/MM/yyyy")));
            li.Controls.Add(inpt);

            Image imgDate = new Image();
            li.Controls.Add(imgDate);
            imgDate.CssClass = "LNKDATEbtn btn"; // TODO: Remplacer par font

            imgDate.Attributes.Add("onclick", "selectExpireDate('expire-date');"); //TODO
            imgDate.Attributes.Add("action", "LNKDATE");

            imgDate.ImageUrl = "ghost.gif";
            imgDate.Style.Add("border-width", "0px");

            ul.Controls.Add(li);

            // Formulaire expiré
            li = new HtmlGenericControl("li");
            li.Attributes.Add("id", "li-msg-expire-id");
            li.Attributes.Add("class", String.Concat("li-content-step inner-li", _oFormular.ExpireDate.ToString().Length > 0 ? "" : " hidden-li"));
            innerDiv = new HtmlGenericControl("div");
            innerDiv.Attributes.Add("class", "inner-div");

            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6706); // 6703 : Message à afficher lorsque le formulaire a expiré :
            innerDiv.Controls.Add(stepLabel);

            inputMsg = new HtmlGenericControl("input");
            inputMsg.ID = "input-expirate-msg";
            inputMsg.Attributes.Add("type", "text");
            inputMsg.Attributes.Add("value", _oFormular.ExpireMessage);
            innerDiv.Controls.Add(inputMsg);

            li.Controls.Add(innerDiv);
            ul.Controls.Add(li);

            //URL réecrite
            li = new HtmlGenericControl("li");
            li.Attributes.Add("id", "liRwUrl");
            li.Attributes.Add("class", "li-content-step");
            if (_nFileId == 0)
                li.Style.Add("display", "none");
            ul.Controls.Add(li);

            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6707); // 6704 : URL réécrite
            li.Controls.Add(stepLabel);

            HtmlInputText stepRewrittenUrl = new HtmlInputText();
            stepRewrittenUrl.Attributes.Add("readonly", "readonly");
            stepRewrittenUrl.Attributes.Add("class", "redirect-url");
            stepRewrittenUrl.ID = "rwUrl";
            stepRewrittenUrl.Value = _nFileId > 0 ? _oFormular.GetRewrittenURL(true) : String.Empty;
            stepRewrittenUrl.Attributes.Add("onfocus", "this.select();");
            stepRewrittenUrl.Attributes.Add("onclick", "this.select();");
            li.Controls.Add(stepRewrittenUrl);

            HtmlInputHidden stepHiddenRewrittenUrl = new HtmlInputHidden();
            stepHiddenRewrittenUrl.ID = "hRwUrl";
            stepHiddenRewrittenUrl.Value = _nFileId > 0 ? _oFormular.GetRewrittenURL(false) : String.Empty;
            li.Controls.Add(stepHiddenRewrittenUrl);

            #endregion

            #endregion

            #region Diffusion
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-title-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 8025), " :"); //Diffusion

            li.Controls.Add(stepLabel);
            ul.Controls.Add(li);

            //Titre
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.ID = "sharingTitleLabel";
            stepLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 6167), " :"); //Titre
            li.Controls.Add(stepLabel);

            TextBox txtSharingTitle = new TextBox();
            txtSharingTitle.Columns = 125;
            txtSharingTitle.ID = "sharingTitle";
            txtSharingTitle.Text = _oFormular.MetaTitle;

            li.Controls.Add(txtSharingTitle);
            ul.Controls.Add(li);

            //Description
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.ID = "sharingDescriptionLabel";
            stepLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 104), " :"); //Description
            li.Controls.Add(stepLabel);

            TextBox txtSharingDescription = new TextBox();
            txtSharingDescription.TextMode = TextBoxMode.MultiLine;
            txtSharingDescription.Rows = 5;
            txtSharingDescription.Columns = 125;
            txtSharingDescription.ID = "sharingDescription";
            txtSharingDescription.Text = _oFormular.MetaDescription;

            li.Controls.Add(txtSharingDescription);
            ul.Controls.Add(li);

            //Image
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");
            ul.Controls.Add(li);

            stepLabel = new HtmlGenericControl("label");
            stepLabel.ID = "sharingImageLabel";
            stepLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 1216), " :"); //Image
            li.Controls.Add(stepLabel);

            TextBox txtSharingImage = new TextBox();
            txtSharingImage.Columns = 100;
            txtSharingImage.ID = "sharingImage";
            txtSharingImage.Text = _oFormular.MetaImgURL;
            txtSharingImage.Attributes.Add("onchange", "oFormular.onSharingImageChange();");
            li.Controls.Add(txtSharingImage);

            //bouton Choisir une image
            HtmlGenericControl btnOpenImage = new HtmlGenericControl("div");
            btnOpenImage.Attributes.Add("id", "btnSharingImage");
            btnOpenImage.Attributes.Add("onclick", "doGetImage(document.getElementById('sharingImage'), 'TXT_URL')");
            btnOpenImage.InnerText = eResApp.GetRes(Pref, 8058); //Choisir une image
            li.Controls.Add(btnOpenImage);


            //URL
            //TODO

            //Boutons de partage
            li = new HtmlGenericControl("li");
            li.ID = "sharingButtons";
            li.Attributes.Add("class", "li-content-step");
            if (_nFileId == 0)
                li.Style.Add("display", "none");
            ul.Controls.Add(li);

            stepLabel = new HtmlGenericControl("label");
            stepLabel.ID = "sharingButtonsLabel";
            stepLabel.InnerText = " ";
            li.Controls.Add(stepLabel);

            li.Controls.Add(BuildSharingPanel());
            #endregion

            #region Options avancées - Obsolète
            //li = new HtmlGenericControl("li");
            //li.Attributes.Add("class", "li-title-step");
            //stepLabel = new HtmlGenericControl("label");
            //stepLabel.InnerHtml = eResApp.GetRes(Pref, 597) + " : ";

            //li.Controls.Add(stepLabel);
            //ul.Controls.Add(li);

            //li = new HtmlGenericControl("li");
            //li.Attributes.Add("class", "li-content-step");
            //stepLabel = new HtmlGenericControl("label");
            //stepLabel.ID = "metaLabel";
            //stepLabel.InnerHtml = eResApp.GetRes(Pref, 6901);
            //li.Controls.Add(stepLabel);

            //TextBox txtMeta = new TextBox();
            //txtMeta.TextMode = TextBoxMode.MultiLine;
            //txtMeta.Rows = 5;
            //txtMeta.Columns = 125;
            //txtMeta.ID = "metaTags";
            //txtMeta.Text = _oFormular.MetaTags;

            //li.Controls.Add(txtMeta);

            //ul.Controls.Add(li);            
            #endregion

            #region Réseaux sociaux - Obsolète
            //li = new HtmlGenericControl("li");
            //li.Attributes.Add("class", "li-title-step");
            //stepLabel = new HtmlGenericControl("label");
            //stepLabel.InnerHtml = eResApp.GetRes(Pref, 6906) + " : ";

            //li.Controls.Add(stepLabel);
            //ul.Controls.Add(li);

            //li = new HtmlGenericControl("li");
            //li.Attributes.Add("class", "li-content-step");
            //eCheckBoxCtrl cbReseauxSoc = new eCheckBoxCtrl(_oFormular.DisplayFacebookShare, false);
            //cbReseauxSoc.AddClick();
            //cbReseauxSoc.ID = "facebookShare";
            //cbReseauxSoc.AddText(eResApp.GetRes(Pref, 6902));
            //li.Controls.Add(cbReseauxSoc);
            //cbReseauxSoc = new eCheckBoxCtrl(_oFormular.DisplayGoogleShare, false);
            //cbReseauxSoc.AddClick();
            //cbReseauxSoc.ID = "googleShare";
            //cbReseauxSoc.AddText(eResApp.GetRes(Pref, 6903));
            //li.Controls.Add(cbReseauxSoc);
            //cbReseauxSoc = new eCheckBoxCtrl(_oFormular.DisplayTwitterShare, false);
            //cbReseauxSoc.AddClick();
            //cbReseauxSoc.ID = "twitterShare";
            //cbReseauxSoc.AddText(eResApp.GetRes(Pref, 6904));
            //li.Controls.Add(cbReseauxSoc);
            //cbReseauxSoc = new eCheckBoxCtrl(_oFormular.DisplayLinkedinShare, false);
            //cbReseauxSoc.AddClick();
            //cbReseauxSoc.ID = "linkedinShare";
            //cbReseauxSoc.AddText(eResApp.GetRes(Pref, 6905));
            //li.Controls.Add(cbReseauxSoc);

            //ul.Controls.Add(li);
            #endregion

            #region Securité

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-title-step");
            stepLabel = new HtmlGenericControl("label");
            stepLabel.InnerHtml = eResApp.GetRes(Pref, 6708); // Sécurité :

            li.Controls.Add(stepLabel);
            ul.Controls.Add(li);
            #region Renderer Sécurité

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "li-content-step");

            ePermissionRenderer rend = new ePermissionRenderer(Pref, _oFormular.IsPublic, _oFormular.ViewPerm, _oFormular.UpdatePerm);
            li.Controls.Add(rend.GetSavePermOptions());

            //ePermissionRenderer.GetHtmlRender(_oFormular.ViewPerm, ePermissionRenderer.PermType.VIEW, Pref, li);
            //ePermissionRenderer.GetHtmlRender(_oFormular.UpdatePerm, ePermissionRenderer.PermType.UPDATE, Pref, li);

            ul.Controls.Add(li);
            #endregion

            #endregion

            stepContainer.Controls.Add(ul);

            return stepContainer;
        }

        private Control BuildSharingPanel()
        {
            string formularUrl = _nFileId > 0 ? _oFormular.GetRewrittenURL(true) : String.Empty;

            HtmlGenericControl divButtons = new HtmlGenericControl("div");
            divButtons.Attributes.Add("class", "shareButtons");

            //twitter            
            HtmlGenericControl spanButton = new HtmlGenericControl("div");
            spanButton.Attributes.Add("class", "shareButton");
            divButtons.Controls.Add(spanButton);

            spanButton.Controls.Add(eSocialNetworkTools.GetTwitterShareButton(Pref, formularUrl, _oFormular.MetaTitle, linkId: "twitterShareButtonLink"));

            //facebook            
            spanButton = new HtmlGenericControl("div");
            spanButton.Attributes.Add("class", "shareButton");
            divButtons.Controls.Add(spanButton);

            spanButton.Controls.Add(eSocialNetworkTools.GetFacebookShareButton(Pref, formularUrl, linkId: "facebookShareButtonLink"));

            //linkedin
            spanButton = new HtmlGenericControl("div");
            spanButton.Attributes.Add("class", "shareButton");
            divButtons.Controls.Add(spanButton);

            spanButton.Controls.Add(eSocialNetworkTools.GetLinkedInShareButton(Pref, formularUrl, _oFormular.MetaTitle, _oFormular.MetaDescription, linkId: "linkedinShareButtonLink"));

            return divButtons;
        }

        #endregion


        #endregion

        /// <summary>
        /// On serialise le formulaire en un objet js 
        /// </summary>
        /// <param name="sbJavaScript"></param>
        public void JavaScriptSerialize(StringBuilder sbJavaScript)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();

            sbJavaScript.AppendLine();
            sbJavaScript.Append(" var oParams = {");
            sbJavaScript.Append("userid:\"").Append(_oFormular.UserId).Append("\", ");
            sbJavaScript.Append("id:\"").Append(_oFormular.FormularId).Append("\", ");

            sbJavaScript.Append("label:").Append(serializer.Serialize(_oFormular.Label)).Append(", ");
            sbJavaScript.Append("saveas:").Append("0, ");
            sbJavaScript.Append("tabbkm:\"").Append(_oFormular.Tab).Append("\", ");
            sbJavaScript.Append("parentfileid:\"").Append(_oFormular.EvtFileId).Append("\", ");

            sbJavaScript.Append("unique:\"").Append(_oFormular.IsUniqueSubmission ? "1" : "0").Append("\", ");
            sbJavaScript.Append("expiredate:\"").Append(_oFormular.ExpireDate.Value.ToString("dd/MM/yyyy")).Append("\", ");
            sbJavaScript.Append("redirecturl:").Append(serializer.Serialize(_oFormular.SubmissionRedirectUrl)).Append(", ");

            sbJavaScript.Append("alreadylabel:").Append(serializer.Serialize(_oFormular.LabelAlreadySubmit)).Append(",");
            sbJavaScript.Append("expiratelabel:").Append(serializer.Serialize(_oFormular.ExpireMessage)).Append(",");
            sbJavaScript.Append("metatags:").Append(serializer.Serialize(_oFormular.MetaTags)).Append(",");

            sbJavaScript.Append("facebookshare:").Append(_oFormular.DisplayFacebookShare ? "1" : "0").Append(",");
            sbJavaScript.Append("googleshare:").Append(_oFormular.DisplayGoogleShare ? "1" : "0").Append(",");
            sbJavaScript.Append("twittershare:").Append(_oFormular.DisplayTwitterShare ? "1" : "0").Append(",");
            sbJavaScript.Append("linkedinshare:").Append(_oFormular.DisplayLinkedinShare ? "1" : "0").Append(",");

            sbJavaScript.Append("sharingTitle:").Append(serializer.Serialize(_oFormular.MetaTitle)).Append(",");
            sbJavaScript.Append("sharingDescription:").Append(serializer.Serialize(_oFormular.MetaDescription)).Append(",");
            sbJavaScript.Append("sharingImage:").Append(serializer.Serialize(_oFormular.MetaImgURL)).Append(",");

            //Le contenu est présent dans ckeditor 
            //pas le peine de l'ajouter ici
            sbJavaScript.Append("body:\"\", ");
            sbJavaScript.Append("submitbody:\"\", ");

            //pour pouvoir l'injecter dans ckeditor
            sbJavaScript.Append("bodycss:").Append(serializer.Serialize(_oFormular.BodyCss)).Append(", ");
            sbJavaScript.Append("submitbodycss:").Append(serializer.Serialize(_oFormular.BodySubmissionCss)).Append(" ");



            //sbJavaScript.Append("baseurl:'").Append(GetRewrittenURL()).Append("' ");
            sbJavaScript.Append("}; ");

            sbJavaScript.AppendLine();

            #region ajout des permissions

            sbJavaScript.Append("var oViewPerm = {");
            if (_oFormular.ViewPerm != null)
                sbJavaScript
                .Append("\"id\":\"").Append(_oFormular.ViewPerm.PermId).Append("\", ")
                .Append("\"mode\":\"").Append(_oFormular.ViewPerm.PermMode).Append("\", ")
                .Append("\"level\":\"").Append(_oFormular.ViewPerm.PermLevel).Append("\", ")
                .Append("\"user\":\"").Append(_oFormular.ViewPerm.PermUser).Append("\"}; ");
            else
                sbJavaScript
                 .Append("\"id\":\"0\", ")
                 .Append("\"mode\":\"-1\", ")
                 .Append("\"level\":\"0\", ")
                 .Append("\"user\":\"\"}; ");

            sbJavaScript.AppendLine();

            sbJavaScript.Append("var oUpdatePerm = {");
            if (_oFormular.UpdatePerm != null)
                sbJavaScript
                .Append("\"id\":\"").Append(_oFormular.UpdatePerm.PermId).Append("\", ")
                .Append("\"mode\":\"").Append(_oFormular.UpdatePerm.PermMode).Append("\", ")
                .Append("\"level\":\"").Append(_oFormular.UpdatePerm.PermLevel).Append("\", ")
                .Append("\"user\":\"").Append(_oFormular.UpdatePerm.PermUser).Append("\"};");
            else
                sbJavaScript
                .Append("\"id\":\"0\", ")
                .Append("\"mode\":\"-1\", ")
                .Append("\"level\":\"0\", ")
                .Append("\"user\":\"\"}; ");

            #endregion

            sbJavaScript.AppendLine();
        }

    }
}