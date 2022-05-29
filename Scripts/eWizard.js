
var wizardAlertWidth = "500";
var wizardAlertHeight = "200";
var stepSelected = [];

var DATE_FORMAT = 2; // type date

//Pointeur vers l'objet JS contenant toutes les fonctionnalités spécifiques à l'assistant actuellement affiché
//Peut pointer vers oReport, oMailing, oInvitRecipients (oInvitWizard)...
//Variable définie à l'initialisation (Init()) selon le type d'assistant traité
var oCurrentWizard = new Object();
var strCurrentWizardType = "";

///summary
///Initalise l'interface au chargement de la page en fonction du type d'assistant
///summary
function Init(wizardType) {

    strCurrentWizardType = wizardType;
    switch (wizardType) {
        case "duplitreat":
            oCurrentWizard = oDuppiWizard;
            break;

        case "report":
            oCurrentWizard = oReport;
            LoadFieldList();
            LoadConfigurableFields();
            CreateHtmlEditors(wizardType);
            ManageSortMenuDisplay();
            break;
        case "chart":
            //par definition le chart est un report
            strCurrentWizardType = "report";
            oCurrentWizard = oReport;
            SaveParams();
            UpdatePanelSelectFields(true);
            break;
        case "mailing":
            oCurrentWizard = oMailing;
            thtabevt.init();
            CreateHtmlEditors(wizardType);
            oCurrentWizard.Init();
            break;
        case "smsmailing":
            oCurrentWizard = oSmsing;
            thtabevt.init();
            CreateHtmlEditors(wizardType);
            oCurrentWizard.Init();
            break;
        case "formular":
            oCurrentWizard = oFormular;
            thtabevt.init();
            CreateHtmlEditors(wizardType);
            oCurrentWizard.Init();
            break;
        case "import":
            oCurrentWizard = oImportWizardInternal;
            thtabevt.init();
            oCurrentWizard.Init();
            break;

        case "invit":
            oCurrentWizard = oInvitWizard;
            thtabevt.init();
            break;

        case "selection":
            oCurrentWizard = oSelectionWizard;
            thtabevt.init();
            updateSelectionList(1);
            break;
        default:

    }

    ManageButtonDisplay();
}



function GetWizardModal(wizardType) {

    switch (wizardType) {
        case "duplitreat":
            return eTools.GetModal("DupliGlobalAffectWizard");
        case "report":
            return eTools.GetModal("ReportWizard");
        case "chart":
            return eTools.GetModal("ReportWizard");
        case "mailing":
            return eTools.GetModal("MailingWizard");
        case "smsmailing":
            return eTools.GetModal("SmsMailingWizard");
        case "formular":
            return eTools.GetModal("FormularWizard");
        case "import":
            return eTools.GetModal("ImportWizard");
        case "invit":
            return eTools.GetModal("InvitWizard");
        case "selection":
            return eTools.GetModal("SelectionWizard");
        default:
            return null;
    }
}

///summary
/// Déplace l'assisant d'une étape en avant ou en arrière
/// <param name="forward">Déplacement en avant</param>
///summary
function MoveStep(forward, wizardType) {

    //block the next button of the SMS over 765 characters in the message
    if (wizardType == "smsing" && oSmsing && oSmsing.IsMaxLengthMessageAttempted()) {
        eAlert(0, '', top._res_8768);
        return;
    }


    if (iCurrentStep > iTotalSteps)
        iCurrentStep = iTotalSteps;

    var iWizardType = 0;


    if ((!forward && (iCurrentStep == 1)) || (forward && (iCurrentStep == iTotalSteps))) {
        return;
    }

    //on ne fait pas la vérif si on clique précédent
    if (forward && !ControlWizardDisplay(iCurrentStep, strCurrentWizardType, oCurrentWizard.GetType()))
        return;

    SwitchStep(forward == true ? ++iCurrentStep : --iCurrentStep);
}

///summary
///gère le clic sur les bouton étape
///<param name="step">numéro de l'étape</param>
///summary
function StepClick(step) {
    var nextStep = document.getElementById("step_" + parseInt(step));
    var bNext = true;

    //on force la vérification 
    if (parseInt(step) > parseInt(iCurrentStep) || oCurrentWizard.GetType() == 'import') {
        if (step == 1 && oCurrentWizard.GetType() == 'import')
            bNext = ControlWizardDisplay(step, strCurrentWizardType, oCurrentWizard.GetType())
        else
            bNext = ControlWizardDisplay(iCurrentStep, strCurrentWizardType, oCurrentWizard.GetType())
    }


    if ((nextStep.className == "state_grp-validated" || nextStep.className == "state_grp state_grp-validation-cancelled" || step == iCurrentStep) && bNext)
        SwitchStep(step, true);
}

///summary
///Appelle la méthode de contrôle de validité d'interface en fonction du type d'assistant et du sous-type
///<param name="step">Etape actuelle </param>
///<param name="wizardType">Type d'assistant (report, graph, mailing) </param>
///<param name="wizardSubType">Sous-Type d'assistant si nécessaire(impression/publipostage/export/Graphique dans le cas de l'assistant reporting) (</param>
///summary
function ControlWizardDisplay(step, wizardType, wizardSubType) {

    switch (wizardType) {
        case "report":
            if (typeof (ControlReportWizardDisplay) == "function")
                return ControlReportWizardDisplay(step, wizardSubType);
        case "mailing":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            return false;
        case "smsmailing":
            if (typeof (oSmsing.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
        case "formular":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            return false;
        case "invit":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            else
                return false;
        case "duplitreat":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            else
                return false;
            break;
        case "selection":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            else
                return false;
            break;
        case "import":
            if (typeof (oCurrentWizard.ControlStep) == "function")
                return oCurrentWizard.ControlStep(step);
            else
                return false;
            break;
        default:
            break;
    }
}

///summary
/// <param name="step">numéro de l'étape cible</param>
/// Si l'étape à déjà été parcourue, elle est accessible directement par click sur son numéro
/// Sinon il est nécessaire de séquentiellement défiler les étapes avec le bouton suivant.
///summary
function SwitchStep(step, bFromStepClick) {
    var stepDiv = null;
    var height = 0;
    var width = 0;


    var bDiv = true;
    //cas particulier mailing, si la step est la 4, il s'agit d'une "fausse" étape
    // pour pouvoir gérer la dualité grapjs/ckeditor.
    if (strCurrentWizardType == "mailing") {

        var boolHaSwitchToMain = false;
        // gestion editeur multiple
        if (oCurrentWizard.GetStepName(step) == "mail" || oCurrentWizard.GetStepName(step) == "mailck") {

            //Si on était sur mailck, on a déjà switché d'étideur

            if (oCurrentWizard.GetStepName(step) == "mail") //Mail : editeur 1
            {
                oMailing.switchEditor(0);
                boolHaSwitchToMain = true
            }
            else
                oMailing.switchEditor(1); // sinon editeur 2. 


            //si on switch entre les étapes "edition grapjs" -> "edition ckedior", il faut "juste"   switcher les éditeur grap et cke et pas tous le div
            if (oCurrentWizard.GetStepName(step) == "mailck" && oCurrentWizard.GetStepName(oCurrentWizard._currentStep) == "mail")
                bDiv = false;
            else if (oCurrentWizard.GetStepName(step) == "mailck" && oCurrentWizard.GetStepName(oCurrentWizard._currentStep) != "mail") {
                bDiv = false;
                switchClass(document.getElementById("editor_" + (step - 1)), "editor-off", "editor-on")
                switchClass(document.getElementById("editor_" + oCurrentWizard._currentStep), "editor-on", "editor-off")
            }
        }

        if (oCurrentWizard.GetStepName(step) == "controlBeforeSend")
            document.getElementById('content').scrollTop = 0;

        if (oCurrentWizard.GetStepName(step) == "infosCampaign")
            document.getElementById('content').scrollTop = 0;

        //Si on change de step depuis ckeditor, on maj le main editeur si ce n'est pas déjà fait via switchEditor
        if (!boolHaSwitchToMain && oCurrentWizard.GetStepName(oCurrentWizard._currentStep) == "mailck" && oCurrentWizard.GetStepName(step) != "mailck") {
            top.setWait(true)
            oMailing.majMainEditor(function () {
                top.setWait(false)
            });

        }

    }

    //ELAIZ : Rajout d'un tableau stepSelected var qui se remplit avec les éléments sélectionnés pour les garder en mémoire si on veut revenir en arrière dans la stepline

    if (stepSelected.length > 0) {
        stepSelected.map(function (x) {
            return removeClass(x, "state_grp-validated");
        });
        stepSelected.map(function (x) {
            return addClass(x, "state_grp state_grp-validation-cancelled");
        });
        stepSelected.map(function (x) {
            return removeClass(x, "state_grp-current");
        });
    }

    for (var i = 1; i <= iTotalSteps; i++) {
        stepDiv = document.getElementById("step_" + i);
        oEditor = document.getElementById("editor_" + i);

        if (step == i) {
            height = oEditor.style.height;
            width = oEditor.style.width;
            removeClass(stepDiv, "state_grp");
            removeClass(stepDiv, "state_grp-validated");
            addClass(stepDiv, "state_grp-current");
            stepSelected.push(stepDiv);
            removeClass(stepDiv, "state_grp state_grp-validation-cancelled");
            if (bDiv)
                switchClass(oEditor, "editor-off", "editor-on");
        }
        else if (i < step) {
            removeClass(stepDiv, "state_grp");
            removeClass(stepDiv, "state_grp-current");
            addClass(stepDiv, "state_grp-validated");
            stepSelected.push(stepDiv);
            removeClass(stepDiv, "state_grp state_grp-validation-cancelled");
            if (bDiv)
                switchClass(oEditor, "editor-on", "editor-off");
        }
        else {
            if (stepDiv.className == "state_grp-current") {
                removeClass(stepDiv, "state_grp-current");
                removeClass(stepDiv, "state_grp");
                addClass(stepDiv, "state_grp-validated");
            }
            if (bDiv)
                switchClass(oEditor, "editor-on", "editor-off");
        }
    }


    //
    if (strCurrentWizardType == "duplitreat" || strCurrentWizardType == "import") {
        if (typeof (oCurrentWizard.SwitchStep) == 'function') {
            oCurrentWizard.SwitchStep(step);
        }
    }
    else if (strCurrentWizardType == "invit") {
        if (typeof (nsInvitWizard.SwitchStep) == 'function') {
            nsInvitWizard.SwitchStep(step);
        }
    }
    else if (strCurrentWizardType == "selection") {
        if (typeof (nsSelectionWizard.SwitchStep) == 'function') {
            nsSelectionWizard.SwitchStep(step);
        }
    }
    else if (typeof (SwitchStepReport) == 'function') {
        SwitchStepReport(step);
    }
    if (strCurrentWizardType == "mailing") {
        if (typeof (oMailing.SwitchStep) == 'function') {



            oMailing.SwitchStep(step, bFromStepClick);
            // On appelle la fonction post-redimensionnement de la fenêtre pour redimensionner dynamiquement la liste des modèles
            if (typeof (onFrameSizeChange) == 'function' && top.modalWizard)
                onFrameSizeChange(top.modalWizard.getDivMain().offsetWidth, top.modalWizard.getDivMainHeight(), 'toto3');

        }
    }
    if (strCurrentWizardType == "formular") {
        if (typeof (oCurrentWizard.SwitchStep) == 'function')
            oCurrentWizard.SwitchStep(step);
    }

    iCurrentStep = step;

    ManageButtonDisplay();

}

///summary
///Affiche/masque les boutons en fonction du numéro de l'étape active
///summary
function ManageButtonDisplay() {

    var btnCancel = null;
    var btnSave = null;
    var btnSaveNExit = null;
    var btnNext = null;
    var btnPrevious = null;
    var btnSaveCampaign = null;
    var btnValidateSelection = null; // Boutton de validation de la sélection
    var btnImport = null; // Boutton pour lancer l'import
    var btnMailtest = null;
    var btnSaveModel = null;
    var btnSaveModelAs = null;
    var modalWizard = null;

    var bDisplayPowerBIOptions = false;
    if (typeof (oReport) != "undefined") {
        if (oReport) {
            bDisplayPowerBIOptions = oReport.GetParam("format").split(";") == 7;
        }
    }

    //si le wizard js implémente son propre managebutton, on le lance
    if (oCurrentWizard && typeof (oCurrentWizard.ManageButtonDisplay) == "function") {
        oCurrentWizard.ManageButtonDisplay();
        return;
    }

    //Récupération de la modal du wizard à partir de son type
    modalWizard = GetWizardModal(strCurrentWizardType);

    if (modalWizard == null) {
        //Si on ne le trouve pas via son handle,on essaye...autrement....

        // Dans le cas de l'assistant Report, celui-ci est ouvert depuis une autre eModalDialog (celle contenant la liste des rapports = listIframe). On récupère donc le pointeur vers la modal dialog
        // du wizard depuis cette modal dialog parente.
        if (listIframe != null) {
            if (listIframe.contentWindow)
                modalWizard = listIframe.contentWindow.modalWizard;
            else
                modalWizard = listIframe.contentDocument.modalWizard;
        }

        // Dans les autres cas (Mailing), l'assistant est ouvert directement depuis la fenêtre principale de l'application. On récupère donc le pointeur directement dessus
        if (typeof (modalWizard) == 'undefined' || !modalWizard)
            modalWizard = top.modalWizard;

    }

    if (modalWizard) {
        var buttonModalDiv = modalWizard.getIframe().parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));

        if (buttonModalDiv == null || buttonModalDiv == "undefined") {
            eAlert(0, "Notification d'Erreur", "Une erreur s'est produite durant l'affichage des boutons de navigation", "buttonModalDiv est inaccessible", wizardAlertWidth, wizardAlertHeight, null);
            return;
        }


        btnCancel = buttonModalDiv.ownerDocument.getElementById("cancel_btn");
        btnSave = buttonModalDiv.ownerDocument.getElementById("save_btn");
        btnSaveNExit = buttonModalDiv.ownerDocument.getElementById("savenexit_btn");
        btnNext = buttonModalDiv.ownerDocument.getElementById("next_btn");

        btnPrevious = buttonModalDiv.ownerDocument.getElementById("previous_btn");
        btnSaveCampaign = buttonModalDiv.ownerDocument.getElementById("savecampaign_btn");

        btnPreview = buttonModalDiv.ownerDocument.getElementById("preview_btn");
        // button
        btnMailtest = buttonModalDiv.ownerDocument.getElementById("mailtest_btn");
        btnSaveModel = buttonModalDiv.ownerDocument.getElementById("savemodel_btn");
        btnSaveModelAs = buttonModalDiv.ownerDocument.getElementById("savemodelas_btn");
        btnImport = buttonModalDiv.ownerDocument.getElementById("import_btn");

        var btnValidateSelection = buttonModalDiv.ownerDocument.getElementById("inviteValidate_btn");

        //
        if (strCurrentWizardType == "report" && oCurrentWizard.GetType() == 6 && iCurrentStep == 4)
            iCurrentStep++;

        var strStep = "first";
        if (iCurrentStep > 1 && iCurrentStep < iTotalSteps)
            strStep = "middle";
        if (iCurrentStep >= iTotalSteps)
            strStep = "last";

        //Masque les boutons spécifiques à certains assistants
        switch (strStep) {

            case "first":
                btnPrevious.style.display = "none";
                btnNext.style.display = "inline";

                if (strCurrentWizardType == "report")
                    btnCancel.style.display = "none";
                else if (strCurrentWizardType == "import") {
                    btnImport.style.display = "none";
                    btnCancel.style.display = "none";
                    btnSaveModel.style.display = "none";
                    btnSaveModelAs.style.display = "none";
                }
                else
                    btnCancel.style.display = "inline";


                if (strCurrentWizardType == "report"
                    || strCurrentWizardType == "mailing"
                    || strCurrentWizardType == "smsmailing"
                    || strCurrentWizardType == "formular")
                    btnSave.style.display = "none";

                if (strCurrentWizardType == "formular" || strCurrentWizardType == "report") {
                    if (btnPreview != null && typeof btnPreview != 'undefined')
                        btnPreview.style.display = "inline";

                    btnSaveNExit.style.display = "none";
                }

                if (strCurrentWizardType == "mailing") {
                    btnSaveCampaign.style.display = "none";
                    btnMailtest.style.display = "none";
                    btnSaveModel.style.display = "none";
                }

                if (strCurrentWizardType == "invit") {
                    btnValidateSelection.style.display = "none";
                }

                break;
            case "middle":

                if (strCurrentWizardType == "report") {
                    btnCancel.style.display = "none";
                    btnSaveNExit.style.display = "none";
                }
                else if (strCurrentWizardType == "import") {
                    btnImport.style.display = "none";
                    btnCancel.style.display = "none";
                    btnSaveModel.style.display = "none";
                    btnSaveModelAs.style.display = "none";
                }
                else
                    btnCancel.style.display = "inline";

                btnNext.style.display = "inline";
                btnPrevious.style.display = "inline";

                if (strCurrentWizardType == "report"
                    || strCurrentWizardType == "mailing"
                    || strCurrentWizardType == "smsmailing"
                    || strCurrentWizardType == "formular")
                    btnSave.style.display = "none";

                if (strCurrentWizardType == "mailing") {
                    btnSaveCampaign.style.display = "none"; // #42794 CRU : Masquer le bouton "Enregistrer la campagne" à l'étape 3
                    if (oCurrentWizard.GetStepName(iCurrentStep) == "mail" || oCurrentWizard.GetStepName(iCurrentStep) == "mailck") {
                        btnMailtest.style.display = "inline";
                        if (oCurrentWizard.GetParam("canadd") == "1")
                            btnSaveModel.style.display = "inline";
                    } else if (oCurrentWizard.GetStepName(iCurrentStep) == "infosCampaign" || oCurrentWizard.GetStepName(iCurrentStep) == "controlBeforeSend") {
                        btnMailtest.style.display = "none";
                        btnSaveModel.style.display = "none";
                        btnSaveCampaign.style.display = "inline";
                    } else {
                        btnMailtest.style.display = "none";
                        btnSaveModel.style.display = "none";
                        btnSaveCampaign.style.display = "none";
                    }

                    if (strCurrentWizardType != "smsmailing" && oCurrentWizard.GetStepName(iCurrentStep) == "controlBeforeSend")
                        btnSaveCampaign.style.display = "inline";
                }

                if (strCurrentWizardType == "formular") {
                    btnPreview.style.display = "inline";
                    btnSaveNExit.style.display = "none";
                }


                if (strCurrentWizardType == "import") {
                    if (iCurrentStep == 4) {
                        btnPrevious.style.display = "inline";
                        btnNext.style.display = "none";
                        btnImport.style.display = "inline";
                    } else {
                        btnPrevious.style.display = "inline";
                        btnNext.style.display = "inline";
                        btnSaveModel.style.display = "none";
                        if (typeof oCurrentWizard.GetImportTemplateId() != 'undefined') {
                            var modelImportParams = oCurrentWizard.GetImportTemplate();
                            var modelImportId = oCurrentWizard.GetImportTemplateId();
                            var updateEnabled = modelImportParams.IsUpdatable;
                            if (modelImportId != 0) {
                                if ((updateEnabled == true && modelImportParams.ImportTemplateName != ''))
                                    btnSaveModel.style.display = "inline";
                                btnSaveModelAs.style.display = "inline";
                            }
                            else
                                btnSaveModel.style.display = "inline";
                        }
                    }

                    btnCancel.style.display = "none";
                }

                break;
            case "last":

                if (!btnNext && !btnPrevious && strCurrentWizardType == "smsmailing")//Dans le cas d'un sms unitaire ces bouttons sont nuls
                    return;
                btnNext.style.display = "none";
                btnPrevious.style.display = "inline";

                // if (strCurrentWizardType == "smsmailing")
                //    btnPrevious.style.display = "none";

                btnCancel.style.display = "inline";

                if (strCurrentWizardType == "report") {
                    btnSave.style.display = "inline";
                    btnSaveNExit.style.display = (bDisplayPowerBIOptions ? "inline" : "none"); // #64 326
                    btnCancel.style.display = "none";
                }

                if (strCurrentWizardType == "mailing" || strCurrentWizardType == "smsmailing") {
                    btnSave.style.display = "inline";
                    btnMailtest.style.display = "none";
                    btnSaveModel.style.display = "none";
                }

                if (strCurrentWizardType == "formular") {
                    btnSave.style.display = "inline";
                    btnSaveNExit.style.display = "inline";
                    btnPreview.style.display = "none";
                }


                if (strCurrentWizardType == "invit") {

                    if (btnValidateSelection) {
                        btnValidateSelection.style.display = "inline";
                        btnCancel.style.display = "inline";
                    }
                }

                if (strCurrentWizardType == "import") {
                    btnPrevious.style.display = "none";
                    btnNext.style.display = "non";
                    btnImport.style.display = "none";
                    btnSaveModel.style.display = "none";
                    btnSaveModelAs.style.display = "none";
                }

                break;
            default:
                break;
        }
    }
}


function SwitchVisibilityStep(nbStep, visibility) {
    if (visibility != 'none' && visibility != 'inline')
        return;
    var steps = document.querySelectorAll(".state_grp");
    Array.prototype.map.call(steps, function (ele) {
        if (ele.id.replace('step_', '') == nbStep) {
            ele.style.display = visibility;
            var sep = ((nbStep > 1) ? ele.previousElementSibling : ele.nextElementSibling);
            if (hasClass(sep, 'state_sep'))
                sep.style.display = visibility;
        }
    });

}

///summary
/// Vérifie la présence d'au moins un champ d'address dans la liste des champs sélectionnés
///summary
function ManageAddressWarningMessage() {
    var selectedFields = oCurrentWizard.GetParam("field").split(";")
    var idx = 0;
    var descId = 0;
    for (idx = 0; idx < selectedFields.length; idx++) {
        descId = parseInt(parseInt(selectedFields[idx]));
        if (descId - (descId % 100) == 400) {
            document.getElementById("editor_warning").style.visibility = "visible";
            return;
        }
    }
    document.getElementById("editor_warning").style.visibility = "hidden";
}

///summary
///Gère l'affichage des options sur les champs de type case à cocher
///<param name="checkBox">Image de la case à cocher de l'option</param>
///summary
function ManageCheckBox(checkBox) {
    var optionList = document.getElementById("editor_bitlabeldisplayoption");
    var checked = checkBox.getAttribute("chk") == 1;
    optionList.disabled = checked == 1 ? "" : "disabled";

    if (!checked)
        optionList.selectedIndex = 0;
}

///summary
///Analyse la touche pressée afin d'afficher ou masquer le bloc d'automatisme
///<param name="event">Evenement clavier(habituellement keyUp)</param>
///summary
function OnKeyInput(event) {
    var formulaBox = document.getElementById("editor_endprocinfo");
    if (formulaBox) {
        var showFormula = (event.ctrlKey == false && event.altKey == false && event.shiftKey == false && (event.KeyCode == 113 || event.key == "F2" || event.which == 113));

        if (!showFormula) {
            ScanString(event.keyCode);
        }

        if (showFormula && formulaBox.style.display == "")
            showFormula = false;

        formulaBox.style.display = showFormula ? "" : "none";
    }
}


///summary
///Joue le son secret html5
///summary
function PlaySecret() {

    var modalId = listIframe.contentWindow.modalWizard.iframeId.replace("frm_", "");
    var rollBox = top.document.getElementById("MainModal_" + modalId);
    rollBox.style.webkitTransform = "";
    rollBox.style.webkitTransition = "";
    rollBox.style.webkitTransform = ("rotate(360deg)");
    rollBox.style.webkitTransition = ("2000ms");

    PlaySecretSound(rollBox, "./sounds/secret.mp3");
}

///summary
///Parcours la chaine str et la compare au mot de passe paramétré
///pour afficher le son secret
///summary
function ScanString(keyPress) {
    //konamicode : ↑ ↑ ↓ ↓ ← → ← → X Y B A barreDespace
    var password = [38, 38, 40, 40, 37, 39, 37, 39, 88, 89, 66, 65, 32];
    CHECKSECRET(password, keyPress, PlaySecret);
}

function CreateHtmlEditors(wizardType) {
    //eMemoEditor(strInstanceName, bHTML, oContainer, oParentFieldEditor, strValue, bCompactMode, strJSVarName)

    if (wizardType == "report") {
        var divContainer = document.getElementById("editor_template_htmlfields");

        if (divContainer == null)
            return;

        htmlTemplate = new eMemoEditor("htmlTemplate", true, document.getElementById("editor_template_html_template"), null, "<span style=\"color:red;\">Template</span>", true, "htmlTemplate");
        htmlTemplate.config.height = "50px;"
        htmlTemplate.show();

        htmlHeader = new eMemoEditor("htmlHeader", true, document.getElementById("editor_template_html_header"), null, "Header", false, "htmlHeader");
        htmlHeader.config.height = "50px;"
        htmlHeader.show();

        htmlFooter = new eMemoEditor("htmlFooter", true, document.getElementById("editor_template_html_footer"), null, "Footer", false, "htmlFooter");
        htmlFooter.config.height = "50px;"
        htmlFooter.show();
    }

    else if (wizardType == "mailing" || wizardType == "smsmailing") {
        initMemoFields(106000, 2);
    }
    else if (wizardType == "formular") {
        initMemoFields(113000, 2);
    }
}

//Return une copie de node  (DOM)
function CloneNode(node) {

    var clone;
    switch (node.nodeType) {

        case 1: //type Node
            clone = node.cloneNode(false);
            break;
        case 2: //type attribut
            var name = node.getAttribute("name");
            clone = document.createAttribute(name);
            clone.nodeValue = node.nodeValue;
            break;
        case 3: //type text
            clone = document.createTextNode(node.nodeValue)
            break;

        /* Autres types a implémenter si besoin 
        case 4: 
        ...
        */

        default: break;

    }

    var child = node.firstChild;
    while (child) {

        clone.appendChild(CloneNode(child));
        child = child.nextSibling;

    }

    return clone;
}



function UpdateWindowSize() {

    // getWindowSize();
}

function SetVisibility(id, bDisplay) {
    var item = document.getElementById(id);
    if (bDisplay)
        item.style.visibility = "visible";
    else
        item.style.visibility = "hidden";

}

function SetDisplay(bDisplay, tabIds) {

    for (var i = 0; i < tabIds.length; i++)
        Display(tabIds[i], bDisplay);
}

function Display(id, bDisplay) {

    var item = document.getElementById(id);
    if (item != null && typeof item != undefined) {
        var valuesOperation = document.getElementById('ValuesOperation');
        if (bDisplay) {
            item.style.display = "";
            if (item.selectedIndex == -1) {
                item.disabled = "disabled";
                if (typeof (valuesOperation) != 'undefined' && valuesOperation != null) {
                    document.getElementById('ValuesOperation').selectedIndex = 0;
                    document.getElementById('ValuesOperation').disabled = "disabled";
                }
            } else {
                if (id != 'CombinedZEtiquettesGroup') {
                    item.disabled = "";
                    if (typeof (valuesOperation) != 'undefined' && valuesOperation != null)
                        document.getElementById('ValuesOperation').disabled = "";
                }

            }


        }
        else
            item.style.display = "none";
    }


}

//Methode pour Drag and Drop Dans le Onload eWizard.aspx Copi de initDragOpt() avec changement des IDs
function initDragOptRapport() {
    dragOpt.customSourceElement = function (oSrcElm) {
        dragOpt.trace("Elément source spécifique à déterminer à partir de : " + oSrcElm.id);
        if (oSrcElm.getAttribute("field_list") != null) {
            dragOpt.trace("L'élément à analyser est déjà la liste source : " + oSrcElm.id);
            return oSrcElm;
        }
        else {
            var oSrcList = oSrcElm;
            if (oSrcList.className.indexOf("ItemList") == -1) {
                oSrcList = oSrcElm.parentNode;
                dragOpt.trace("L'élément à analyser est une cellule de la liste : " + oSrcElm.id);
            }
            else {
                dragOpt.trace("L'élément à analyser est le conteneur de liste source, ou la liste source elle-même : " + oSrcElm.id);
            }
        }

        var descId = null;
        for (var i = 0; i < oSrcList.children.length; i++) {
            descId = oSrcList.children[i].id.split("_")[2];
            if (descId && dragOpt.eventsObj && dragOpt.eventsObj.origElt && descId == dragOpt.eventsObj.origElt.getAttribute("edntab")) {
                dragOpt.trace('Liste source spécifique : ' + oSrcList.children[i].tagName + "." + oSrcList.children[i].id);
                return oSrcList.children[i];
            }
        }
        return oSrcElm;
    };
    // choix des rubriques
    if (document.getElementById("editor_DivTargetList")) {
        try {
            dragOpt.SrcList = document.getElementById("editor_sourcelist");
            dragOpt.TrgtList = document.getElementById("editor_DivTargetList").children[0];
            dragOpt.FldSel = false;
        }
        catch (e) {
            dragOpt.trace("Une erreur est survenue : aucune liste prenant en charge le drag and drop n'a été détectée. Le drag & drop ne sera pas disponible. " + e);
        }

        dragOpt.init();
    }

}



///Appel pour mise  à jour du contenu de la liste de sélection invitation (++/xx) à partie d'un filtre
function UpdatePPList(page, rows) {
    var upd = new eUpdater("mgr/eInvitWizardManager.ashx", 1);


    upd.ErrorCallBack = function () { setWait(false) };

    //_eCurentSelectedFilter est une variable globale déclarée dans eFilterReportList.js appelée dans l'écran 1 qui présente la liste des filtres
    if (!_activeFilter) {
        var aEid = getAttributeValue(_eCurentSelectedFilter, "eid").split("_");
        if (!_eCurentSelectedFilter || aEid.length < 2) {
            eAlert(0, top._res_428, top._res_430);
            return;
        }
        _activeFilter = aEid[1];
    }
    var init = 0;
    if (typeof (page) == "undefined") {
        page = 1;
        init = 1;
    }
    else
        page = getNumber(page);


    //Vide le contenu du div pour ne pas afficher un contenu erroné le temps que l'ajax rende le résultat
    var oDiv = document.getElementById("PPList");
    if (oDiv) {
        //oDiv.innerHTML = "";
    }

    var oDiv = document.getElementById("content");
    var oDivHeader = document.getElementById("wizardheader");
    var oDivCfmDest = document.getElementById("CfmDest");
    var oDivRadioCfmDest = document.getElementById("RadioCfmDest");

    var odDivPaggin = document.getElementById("GetLstSelPagging");
    var oDivFilter = document.getElementById("InvitListFilter");

    var height = 0;

    if (window.innerHeight) {
        height = window.innerHeight;
        width = window.innerWidth - 100;
    }
    else {
        height = document.documentElement.clientHeight;
        width = document.documentElement.clientWidth;
    }


    if (oDivHeader)
        height -= oDivHeader.offsetHeight;

    if (oDivCfmDest)
        height -= oDivCfmDest.offsetHeight;

    if (oDivRadioCfmDest)
        height -= oDivRadioCfmDest.offsetHeight;

    if (oDivFilter)
        height -= oDivFilter.offsetHeight;

    height -= 80;  // espace des boutons de pagination

    // #56 188 - Nombre de lignes par page personnalisable
    var defaultRowCount = Math.floor(height / (23 + 1)) - 1;

    /* tente de récupérer la  div a partir de sa css*/
    var oTabelu = oDiv.querySelector("div.tabeul.dest");
    if (oTabelu) {
        var nTabeulHeight = getNumber(oTabelu.style.height.replace("px", ""));
        if (nTabeulHeight > 0) {

        }
        else {
            var oRules = getCssSelector("*", ".tabeul");
            if (oRules) {
                nTabeulHeight = getNumber(oRules.style.height.replace("px", ""));
            }
        }

        if (nTabeulHeight > 0) {
            defaultRowCount = Math.floor(nTabeulHeight / (23 + 1)) - 1;
        }
    }

    var bUseDefaultRowCount = false;
    if (typeof (rows) == "undefined") {
        if (document.getElementById('nR'))
            rows = getNumber(document.getElementById('nR').value);
        else
            rows = defaultRowCount;
    }
    else {
        if (rows == "default") {
            rows = defaultRowCount;
            bUseDefaultRowCount = true;
        }
        else
            rows = getNumber(rows);
    }

    // #56 188
    var maxRowsAllowed = rows;
    if (document.getElementById('maxRowsAllowed'))
        maxRowsAllowed = getNumber(document.getElementById('maxRowsAllowed').value);
    var totalRows = rows;
    if (document.getElementById('cfm'))
        totalRows = getNumber(document.getElementById('cfm').value);
    //KHA bug 63 546
    //if (totalRows > 0 && (rows > totalRows || rows > maxRowsAllowed)) {
    //    if (totalRows < maxRowsAllowed)
    //        //rows = totalRows;
    //        ;
    //    else
    //        rows = maxRowsAllowed;
    //}
    if (document.getElementById('ivtMaxNbRows')) {
        if (rows == totalRows || rows == maxRowsAllowed) {
            // Mode "Toutes les lignes" non encore activé
            if (!bUseDefaultRowCount) {
                switchClass(document.getElementById('ivtMaxNbRows'), "ivtMaxNbRows", "ivtMaxNbRowsActive");
            }
            // Mode déjà activé : cliquer sur l'icône le désactive et réinitialise le nombre de lignes par défaut
            else {
                switchClass(document.getElementById('ivtMaxNbRows'), "ivtMaxNbRowsActive", "ivtMaxNbRows");
                rows = defaultRowCount;
            }
        }
        else
            switchClass(document.getElementById('ivtMaxNbRows'), "ivtMaxNbRowsActive", "ivtMaxNbRows");
    }

    // #56 188 - Pas de numéro de page supérieur au nombre maximal possible
    var totalPages = page;
    if (document.getElementById('nbP'))
        totalPages = getNumber(document.getElementById('nbP').value);
    if (totalPages > Math.ceil(totalRows / rows))
        totalPages = Math.ceil(totalRows / rows);
    if (page > totalPages && totalPages > 0)
        page = totalPages;

    // SPH/ MCR: 40260,  resize de la dernière colonne du tableau à 100%, sur une edition de filtres en ++ : utilisation de _activeFilter 
    //              si _eCurentSelectedFilter n'est pas positionne alors recuperation du eft par querySelector sur '104000' filtres
    //              si _activeFilter n'est pas activé, alors utilisation de la variable globale _activeFilterTab (setté dans selFilter dans eMain.js) 
    if (_eCurentSelectedFilter)
        var fltTab = getAttributeValue(_eCurentSelectedFilter, "eft");
    else {
        var fltTab = getAttributeValue(document.querySelector("[eid='104000_" + _activeFilter + "']"), "eft")
        if (fltTab == "") {
            var fltTab = _activeFilterTab;   // sur la sauvegarde du filtre, "mgr/eFilterWizardManager.ashx", la variable nTab est settée
        }
    }

    var bkm = getAttributeValue(oDivHeader, "bkm");


    upd.addParam("tab", fltTab, "post");
    upd.addParam("bkm", bkm, "post");
    upd.addParam("action", 0, "post");
    upd.addParam("fid", _activeFilter, "post");
    upd.addParam("height", height, "post");
    upd.addParam("width", width, "post");
    upd.addParam("rows", rows, "post");
    upd.addParam("page", page, "post");
    upd.addParam("init", init, "post");

    upd.addParam("delete", oInvitWizard.DeleteMode ? "1" : "0", "post");    //Descid de l'event de départ
    upd.addParam("filefromid", oInvitWizard.FileFromId, "post");    //ID de l'event de départ
    upd.addParam("tabfrom", oInvitWizard.TabFrom, "post");    //Descid de l'event de départ

    //Récupération des options sur les adresse à retenir
    var oAct = document.getElementById("radioAdrAct")
    if (oAct && getAttributeValue(oAct, "chk") == "1") {
        upd.addParam("fltact", "1", "post");
    }

    var oPrinc = document.getElementById("radioAdrPrinc")
    if (oPrinc && getAttributeValue(oPrinc, "chk") == "1") {
        upd.addParam("fltprinc", "1", "post");
    }

    //Récupération des options sur les consentements
    var oCampaignType = document.getElementById("invitSelectCampaignType")
    if (oCampaignType) {
        var oCampaignTypeValue = oCampaignType.options[oCampaignType.selectedIndex].value;

        if (oCampaignTypeValue != "" && oCampaignTypeValue != "0") {
            upd.addParam("fltcampaigntype", oCampaignTypeValue, "post");
        }
    }

    var oTypeConsent = document.getElementById("invitHiddenTypeConsent")
    if (oTypeConsent && oTypeConsent.hasAttribute("value")) {
        var oTypeConsentValue = oTypeConsent.getAttribute("value");
        if (oTypeConsentValue != "" && oTypeConsentValue != "0") {
            upd.addParam("flttypeconsent", oTypeConsentValue, "post");
        }
    }

    var oOptin = document.getElementById("invitChbxOptin")
    if (oOptin && getAttributeValue(oOptin, "chk") == "1" && oOptin.hasAttribute("value")) {
        var fltoptinValue = oOptin.getAttribute("value");
        if (fltoptinValue != "" && fltoptinValue != "0")
            upd.addParam("fltoptin", fltoptinValue, "post");
    }

    var oOptout = document.getElementById("invitChbxOptout")
    if (oOptout && getAttributeValue(oOptout, "chk") == "1" && oOptout.hasAttribute("value")) {
        var fltoptoutValue = oOptout.getAttribute("value");
        if (fltoptoutValue != "" && fltoptoutValue != "0")
            upd.addParam("fltoptout", fltoptoutValue, "post");
    }

    var oNoopt = document.getElementById("invitChbxNoopt")
    if (oNoopt && getAttributeValue(oNoopt, "chk") == "1") {
        upd.addParam("fltnoopt", "1", "post");
    }

    setWait(true);

    upd.send(function (oRes) { updReturnPPList(oRes, fltTab, init); });
}



function updReturnPPList(oRes, fltTab, nInit) {
    var oDiv = document.getElementById("PPList");

    if (!oDiv) {
        alert('todo msg erreur');
        return;
    }


    oDiv.innerHTML = oRes;

    var oDivContent = document.getElementById("content");
    initHeadEvents();

    // SPH/ MCR: 40260,  resize de la dernière colonne du tableau à 100%, sur une edition de filtres en ++ 
    //adjustLastCol(fltTab, oDivContent);
    //SHA : bug 70579
    autoResizeColumns(200, oDivContent);

    setWait(false);

    //Test la création automatique
    if (nInit == 1) {
        nsInvitWizard.checkAutoLaunch();
    }
    else {
        nsInvitWizard.updtInvitCnt(null, 0);
    }
}



var modalIvtCol;
function setIvtCol(nIvtTab) {

    modalIvtCol = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);
    modalIvtCol.ErrorCallBack = function () { setWait(false); }

    modalIvtCol.addParam("tab", nIvtTab, "post");
    modalIvtCol.addParam("action", "initivt", "post");
    modalIvtCol.addParam("delete", oInvitWizard.DeleteMode ? "1" : "0", "post");    // Mode suppression


    modalIvtCol.bBtnAdvanced = true;
    modalIvtCol.show();
    modalIvtCol.addButton(top._res_29, onSetIvtColAbort, "button-gray", nIvtTab);
    modalIvtCol.addButton(top._res_28, onSetIvtColOk, "button-green", nIvtTab);

}

function onSetIvtColOk(nTab, popupId) {
    var _frm = modalIvtCol.getIframe();
    var strBkmCol = _frm.getSelectedDescId();

    //Récupération du strBkmCol
    var _oDoc = _frm.document || _frm.contentDocument;
    var cbo = _oDoc.getElementById("AllSelections");

    var updatePref = "tab=" + nTab
        + ";$;listselcol=" + strBkmCol
        + ";$;deletemode=" + (oInvitWizard.DeleteMode ? "1" : "0")
        + ";$;targetmode=" + oInvitWizard.Target;

    updateColsPref(updatePref, function () { UpdatePPList(1); });
    modalIvtCol.hide();

}
function onSetIvtColAbort(v1, popupId) {
    modalIvtCol.hide();
}

var modalSelectionCol;

function setSelectionCol(nTab) {



    modalSelectionCol = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);
    modalSelectionCol.ErrorCallBack = function () { setWait(false); }

    modalSelectionCol.addParam("tab", nTab, "post");
    modalSelectionCol.addParam("action", "initselection", "post");

    modalSelectionCol.bBtnAdvanced = true;
    modalSelectionCol.show();
    modalSelectionCol.addButton(top._res_29, onSetSelectionColAbort, "button-gray", nTab);
    modalSelectionCol.addButton(top._res_28, onSetSelectionColOk, "button-green", nTab);

}

function onSetSelectionColOk(nTab, popupId) {
    var _frm = modalSelectionCol.getIframe();
    var strBkmCol = _frm.getSelectedDescId();

    //Récupération du strBkmCol
    var _oDoc = _frm.document || _frm.contentDocument;
    var cbo = _oDoc.getElementById("AllSelections");

    var updatePref = "tab=" + nTab
        + ";$;listselcol=" + strBkmCol
        + ";$;deletemode=" + (oInvitWizard.DeleteMode ? "1" : "0")
        + ";$;targetmode=" + oInvitWizard.Target;

    updateColsPref(updatePref, function () { updateSelectionList(1); });

    modalSelectionCol.hide();
}

function onSetSelectionColAbort(v1, popupId) {
    modalSelectionCol.hide();
}

function updateSelectionList(page, filters, action, fromPagination) {

    if (typeof (action) === 'undefined' || !action)
        action = "0";

    if (typeof (fromPagination) === 'undefined')
        fromPagination = false;

    var upd = new eUpdater("mgr/eSelectionWizardManager.ashx", 1);

    upd.ErrorCallBack = function () { setWait(false) };

    var init = 0;
    if (typeof (page) == "undefined") {
        page = 1;
        init = 1;
    }

    var oDiv = document.getElementById("content");
    var oDivHeader = document.getElementById("wizardheader");
    var oDivCriteria = document.getElementById("blockCriteria");
    //var odDivPaggin = document.getElementById("GetLstSelPagging");

    var height = 0;

    if (window.innerHeight) {
        height = window.innerHeight;
        width = window.innerWidth - 100;
    }
    else {
        height = document.documentElement.clientHeight;
        width = document.documentElement.clientWidth;
    }

    if (oDivHeader)
        height -= oDivHeader.offsetHeight;

    if (oDivCriteria)
        height -= oDivCriteria.offsetHeight + 30;

    height -= 80;  // espace des boutons de pagination

    height = 570; // Pour caler la carte et la liste

    var rows = Math.floor(height / (23 + 1));

    upd.addParam("tab", oSelectionWizard.Tab, "post");
    upd.addParam("tabsource", oSelectionWizard.TabSource, "post");
    upd.addParam("action", action, "post");
    upd.addParam("reloadmap", !(fromPagination), "post");
    upd.addParam("height", height, "post");
    upd.addParam("width", width, "post");
    upd.addParam("rows", rows, "post");
    upd.addParam("page", page, "post");

    if (typeof (filters) !== 'undefined' && filters != null) {
        upd.addParam("filters", filters, "post");
    }
    else if (document.getElementById("hidFilters") != null) {
        upd.addParam("filters", document.getElementById("hidFilters").value, "post");
    }

    setWait(true);

    upd.send(function (oRes) { updateReturnSelectionList(oRes, oSelectionWizard.TabSource, !(fromPagination)); });
}


function updateReturnSelectionList(oRes, fltTab, bReloadMap) {
    var oDiv = document.getElementById("blockList");

    if (!oDiv) {
        alert('Div introuvable !');
        return;
    }

    oDiv.innerHTML = oRes;

    var oDivContent = document.getElementById("content");
    initHeadEvents();

    //autoResizeColumns(fltTab, oDivContent);

    setWait(false);


    // 

    if (bReloadMap) {
        var blockmap = document.getElementById("blockMap");

        var geoField = document.getElementById("hidGeoField");
        if (geoField) {
            var hasGeoField = geoField.value;
            if (hasGeoField == "") {

                if (blockmap) {
                    blockmap.style.display = "none";
                }

                oDiv.style.width = "100%";

            }
            else {
                // Rechargement de la carte

                if (blockmap) {
                    blockmap.style.display = "block";
                }

                oDiv.style.width = "580px";

                var arrLocations;
                var hidLocations = document.getElementById("hidLocations");
                if (hidLocations) {
                    var hidLocationsValue = hidLocations.value;
                    arrLocations = hidLocationsValue.split('$;$');
                }

                // Message d'affinage de recherche à cacher
                var message = document.getElementById("mapMessage");
                if (message)
                    message.style.display = "none";

                // Conteneur du toolbar à vider
                var toolbarContainer = document.getElementById("toolbarContainer");
                if (toolbarContainer)
                    toolbarContainer.innerHTML = "";

                // Vue de la map : latitude,longitude,zoom
                var mapView = '48.901865,2.261006,15';
                var hidMapView = document.getElementById("hidMapView");
                if (hidMapView) {
                    if (hidMapView.value != "")
                        mapView = hidMapView.value;
                }

                nsSelectionWizard.initMap('map', 'Aia9V-TFKUb44CNZsVp_oxYGgszFUgksJal8-_IW1SSbodepQ4didGSMVp4UiSwR', mapView, '580', '570', arrLocations);

            }
        }
    }
}
