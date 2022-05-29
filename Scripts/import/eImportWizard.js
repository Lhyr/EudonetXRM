
/// <summary>
/// Objet permettant de gérer les étapes de l'assistant d'import depuis l'exterieur du wizard
/// </summary>
var oImportWizard = {
    /// <summary>
    /// Assistant d'import 
    /// </summary>
    'Wizard': {

        /// <summary>
        /// Valeurs par défaut des paramétres de l'assistant d'import
        /// Si 'ImportTab' = 0 l'assistant ne se lance pas
        /// </summary>
        _defaultParams: {
            'ParentTab': 0,
            'ParentFileId': 0,
            'ImportTab': 0,
            'ImportTemplateParams': {
                'ImportTemplateId': 0
            },
            'SizeFactor': 0.,
            'Width': 900,
            'Height': 600
        },

        /// <summary>
        /// Mis à jour des valeurs non fournie avec celles par défaut
        /// </summary>
        _setDefault: function (wizardParams) {

            // copie interne pour plus de sécurité 
            var params = wizardParams;

            // Table cible d'import
            if (typeof (params) == 'undefiend' || typeof (params.ImportTab) == 'undefined')
                params.ImportTab = this._defaultParams.ImportTab;

            // table parente
            if (typeof (params.ParentTab) == 'undefined' || typeof (params.ParentFileId) == 'undefined') {
                params.ParentTab = this._defaultParams.ParentTab;
                params.ParentFileId = this._defaultParams.ParentFileId;
            }


            // taille de la fenêtre
            if (typeof (params.Width) != 'number' || typeof (params.Height) != 'number') {
                params.Width = this._defaultParams.Width;
                params.Height = this._defaultParams.Height;
            }

            // Table cible d'import
            if (typeof (params) == 'undefiend' || typeof (params.SizeFactor) == 'undefined')
                params.SizeFactor = this._defaultParams.SizeFactor;

            // Le facteur de taille est prioritaire à width et height
            if (typeof (params.SizeFactor) == 'number' && params.SizeFactor <= 1. && params.SizeFactor > 0.) {
                var winSize = top.getWindowSize().scale(params.SizeFactor);
                params.Width = winSize.w;
                params.Height = winSize.h;
            }

            this.WizardParams = params;
            return params;
        },

        /// <summary>
        /// Référence à l'objet d'import interne à l'assistant
        /// </summary>
        ImportWizardInternal: null,

        /// <summary>
        /// Référence à l'objet de création du modèle d'import
        /// </summary>
        ImportTemplateNewInternal: null,

        WizardParams: null,
        /// <summary>
        /// Référence à la modal de l'assistant
        /// </summary>
        Modal: null,

        /// <summary>
        /// Les params d'assistant d'import
        /// wizardParams :
        ///   'ParentTab': descId de la table parente
        ///   'ParentFileId': fileId de la fiche parente,
        ///   'ImportTab': table cible pour l'import,
        ///   'SizeFactor': facteur de la taille de la fenêtre entre 0 et 1 : prioritaire au width et height
        ///   'Width': largeur de l'assistant en pixel
        ///   'Height': heuateur de l'assistant en en pixel
        /// 
        /// </summary>
        Show: function (wizardParams) {

            wizardParams = this._setDefault(wizardParams);

            // Table d'import doit etre valide
            if (wizardParams.ImportTab == 0 || wizardParams.ImportTab % 100 != 0) {
                oEvent.fire('log-error', "Import wizard : DescId de la table d'import n'est pas valide !");
                return;
            }

            this.Modal = new eModalDialog(top._res_6713, 0, "eWizard.aspx", wizardParams.Width, wizardParams.Height, 'ImportWizard');

            this.Modal.addParam("wizardType", "import", "post");

            this.Modal.addParam("parentTab", wizardParams.ParentTab, "post");
            this.Modal.addParam("parentFileId", wizardParams.ParentFileId, "post");

            this.Modal.addParam("importTab", wizardParams.ImportTab, "post");
            this.Modal.addParam("importTemplateId", wizardParams.ImportTemplateParams.ImportTemplateId, "post");
            this.Modal.addParam("width", wizardParams.Width, "post");
            this.Modal.addParam("height", wizardParams.Height, "post");

            this.Modal.onIframeLoadComplete = function () { oImportWizard.Wizard.WizardLoaded(wizardParams); }


            this.Modal.ErrorCallBack = function () { setWait(false); oImportWizard.Wizard.Close(); };

            this.Modal.hideMaximizeButton = true;

            this.Modal.show();


            this.Modal.addButton(top._res_26, this.NextStep, "button-green-rightarrow", null, "next_btn");// Suivant
            this.Modal.addButton(top._res_8479, this.NextStep, "button-green-rightarrow", null, "import_btn", 'right');  //Importer  
            this.Modal.addButton(top._res_286, function () { oImportWizard.Wizard.ShowNewImportTemplateWizard(false); }, 'button-green', null, "savemodel_btn");
            this.Modal.addButton(top._res_118, function () { oImportWizard.Wizard.ShowNewImportTemplateWizard(true); }, 'button-green', null, "savemodelas_btn");
            this.Modal.addButton(top._res_30, this.Dispose, "button-green", null, "cancel_btn");//Fermer
            this.Modal.addButton(top._res_25, this.PreviousStep, "button-gray-leftarrow", null, "previous_btn");// Précedent 

            // Ne pas afficher les boutons  au démarrage. L'assistant gère leur affichage
            this.Modal.hideButtons();

            // Référence à la modal depuis la page princiaple
            //top.window["_md"]["ImportWizard"] = this.Modal;

           
        },

        /// <summary>
        /// Afficher la modal des modèles d'import
        /// </summary>
        ShowNewImportTemplate: function (importTemplateId, saveAs) {
            var nPopupWidth = 400;
            var that = this;
            var templateModal = new eModalDialog(top._res_6472, 0, 'mgr/import/eImportTemplateWizardManager.ashx', 400, 400, 'NewImportTemplateWizard');
            templateModal.saveAs = saveAs;
            templateModal.ErrorCallBack = launchInContext(this.Modal, this.Modal.hide);
            templateModal.addParam("width", nPopupWidth, "post");
            templateModal.addParam("height", 400, "post");
            templateModal.addParam("action", 5, "post");
            templateModal.addParam("tab", this.WizardParams.ImportTab, "post");
            templateModal.addParam("frmId", this.Modal.iframeId, "post");
            templateModal.addParam("importtemplateid", importTemplateId, "post");
            templateModal.addParam("saveas", (saveAs ? 1 : 0), "post");
            templateModal.addParam("lstType", 26, "post");
            templateModal.onIframeLoadComplete = function () { oImportWizard.Wizard.ImportTemplateNewInternal = templateModal; }
            templateModal.ErrorCallBack = function () { setWait(false); templateModal.hide(); };
            templateModal.show();
            templateModal.addButtonFct(top._res_29, templateModal.hide, 'button-gray');
            templateModal.addButtonFct(top._res_28, function () {
                var saveAs = oImportWizard.Wizard.ImportTemplateNewInternal.saveAs;
                oImportWizard.Wizard.ImportWizardInternal.SaveImportTemplate(saveAs, false, true);
            }, 'button-green');
            templateModal.hideMaximizeButton = true;
            // Référence à la modal depuis la page princiaple
            //top.window["_md"]["NewImportTemplateWizard"] = templateModal;
        },

        ShowNewImportTemplateWizard: function (saveAs) {
            if (this.WizardParams.ImportGlobalTabParam.Tables.length > 0)
                this.ShowNewImportTemplate(this.WizardParams.ImportTemplateParams.ImportTemplateId, saveAs);
            else
                top.eAlert(0, top._res_8365, top._res_8708, '', null, null, null);

        },
        /// <summary>
        /// Une fois l'assistant est completement chargé, on récupère l'objet interne du wizard
        /// </summary>
        WizardLoaded: function (wizardParams) {
            var ImportWizardInternal = oImportWizard.Wizard.Modal.getIframe().oImportWizardInternal;

            // Initialisation des paramétres d'import
            ImportWizardInternal.SetWizardParam(wizardParams);

            // Sauvegarde de la réference
            oImportWizard.Wizard.ImportWizardInternal = ImportWizardInternal;
        },


        /// <summary>
        /// fermer l'assistant
        /// </summary>
        Close: function () {
            if (oImportWizard.Wizard.ImportWizardInternal != null) {
                if (oImportWizard.Wizard.ImportWizardInternal.GetCurrentStep() >= oImportWizard.Wizard.ImportWizardInternal.STEP.PROGESS)
                    oImportWizard.Wizard.ImportWizardInternal.Dispose(false, oImportWizard.Wizard.Modal.hide);
                else
                    oImportWizard.Wizard.ImportWizardInternal.Dispose(true, oImportWizard.Wizard.Modal.hide);
            } else
                oImportWizard.Wizard.Modal.hide();

        },
        Dispose: function () {
            oImportWizard.Wizard.Close()
        },

        /// <summary>
        /// Passer à l'étape suivante
        /// </summary>
        NextStep: function () {
            oImportWizard.Wizard.ImportWizardInternal.MoveStep(true, true);
        },

        /// <summary>
        /// Passer à l'étape précédente
        /// </summary>
        PreviousStep: function () {
            var currentStep = oImportWizard.Wizard.ImportWizardInternal.GetCurrentStep();
            if (currentStep == oImportWizard.Wizard.ImportWizardInternal.STEP.MAPPING || currentStep == oImportWizard.Wizard.ImportWizardInternal.STEP.OPTIONS) {
                oImportWizard.Wizard.ImportWizardInternal.ControlStep(currentStep - 1);
            }
            else {
                if (currentStep == oImportWizard.Wizard.ImportWizardInternal.STEP.END)
                    oImportWizard.Wizard.ImportWizardInternal.SwitchVisibilityStep(currentStep + 1, 'inline');
                oImportWizard.Wizard.ImportWizardInternal.MoveStep(false);
            }


        },

        HeaderWizardClick: function (step) {
            var currentStep = oImportWizard.Wizard.ImportWizardInternal.GetCurrentStep();
            if (step > currentStep)
                this.NextStep();
            else
                this.PreviousStep();

        }
    },

    /// <summary>
    /// Permet de vérifier si le serveur est dispo pour l'import
    /// </summary>
    'CheckServer': function (wizardParam) {

        // on vérifie s'il y a des import en cours     
        setWait(true);

        var oUpdater = new eUpdater("mgr/eImportProcessManager.ashx", 1);
        oUpdater.asyncFlag = true;

        // identification de la fiche parente
        oUpdater.addParam("ParentTab", wizardParam.ParentTab, "post");
        oUpdater.addParam("ParentFileId", wizardParam.ParentFileId, "post");

        // Table cible de l'import
        oUpdater.addParam("ImportTab", wizardParam.ImportTab, "post");

        // Identifiant de l'import
        oUpdater.addParam("ImportId", wizardParam.ImportId, "post");

        // Action a excuter coté serveur pour savoir la surcharge
        oUpdater.addParam("Action", 7, "post");

        // l'etape concernée de l'Action
        oUpdater.addParam("Step", 1, "post");

        // taille du rendu            
        var winSize = getWindowSize();
        oUpdater.addParam("height", winSize.h, "post");
        oUpdater.addParam("width", winSize.w, "post");

        // En cas d'erreur
        oUpdater.ErrorCallBack = function (result) {

            var res = JSON.parse(result);
            setWait(false);
            eAlert(1, top._res_72, top._res_1806, res.ErrorDetail);
        }

        oUpdater.send(function (result) {
            var res = JSON.parse(result);

            setWait(false);

            // On ferme le wizard
            if (!res.Success)
                eAlert(2, res.ErrorTitle, res.ErrorMsg, res.ErrorDetail, 500, 200);
            else
                oImportWizard.Wizard.Show(wizardParam);
        });
    },

    /// <summary>
    /// Permet de lancer l'assistant d'import depuis un signet
    /// </summary>
    'ShowBkmWizard': function (parentTab, paranteFileId, importTab) {
        this.CheckServer({
            'ParentTab': parentTab,
            'ParentFileId': paranteFileId,
            'ImportTab': importTab,
            'SizeFactor': 0.95,
            'ImportTemplateParams': {
                'ImportTemplateId': 0
            }
        });

    },

    /// <summary>
    /// Permet de lancer l'assistant d'import depuis un onglet
    /// </summary>
    'ShowTabWizard': function (importTab) {
        this.CheckServer({
            'ImportTab': importTab,
            'SizeFactor': 0.95,
            'ImportTemplateParams': {
                'ImportTemplateId': 0
            },
        });


    }
};

