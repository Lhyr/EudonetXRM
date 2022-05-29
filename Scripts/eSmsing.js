/// <reference path="eMemoEditor.js" />
/// <reference path="eTools.js" />
/// <reference path="eTools.js" />

/***
 * 
*  Objet permetant de génrer l'envoi de la campagne SMS
*  
*
**/



oSmsing = (function () {

    /// <summary>
    /// Type d'envoi différé utilisé
    /// </summary>
    var SMSingQueryMode =
    {
        /// <summary>0 : Envoi avec la photo des id</summary>
        NORMAL: 0,
        /// <summary>1 : Envoi en enregistrant la requête</summary>
        QUERY_RUN_AGAIN: 1,
        /// <summary>2 : Envoi récurrent vers tous les destinataires</summary>
        RECURRENT_ALL: 2,
        /// <summary>3 : Envoi récurrent vers les destinataires répondant aux critères d'un filtre</summary>
        RECURRENT_FILTER: 3
    }

    /// <summary>
    /// SMS en Masse/Unitaire
    /// </summary>
    var SMSingWizartType =
    {
        /// <summary>0 : Envoi en masse</summary>
        SMSing: 0,
        /// <summary>1 : Envoi unitaire</summary>
        UnitSMS: 1,
    }

    var that = this
    var _modalSchedule;
    var __oSmsParams = {}
    var __sSmsParamPrefix = ""

    var versionPresta = ''


    var _sLabel = '';
    var _oMemoEditor;


    var paramWin = top.getParamWindow();
    var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

    //Activé : Affiche des logs dans la console 
    var bDebug = false;

    // id de la campagne à modifier
    var nCampaignId = 0;

    // id de template
    var nTemplateID = 0;

    //Table des campaigne Mail
    var nCamapignTab = 106000;

    //Signet ++/cible
    var nParentBkmId = 0;

    // Table de l'evenement parent
    var nParentTabId = 0;

    //Fiche de l'evenement parent 
    var nParentFileId = 0;


    //Id du template SMS
    var nTemplateId = 0;

    // fenêtre de eConfirm ou eAlert
    var oMessageBox = null;

    var oMessageBoxType = { ALERT: 0, CONFIRM: 1, WAIT: 2 };

    var oWizardType = SMSingWizartType.UnitSMS;//par défaut

    var maxLengthMessageAttempt = false;//used to block the button for Next Step


    // Initialisation de sms
    function init() {
        oTemplate = new eMailingTemplate()

        nsMain.getAllMemoEditorIDs().forEach(function (memeEdit, idx) {
            _oMemoEditor = nsMain.getMemoEditor(memeEdit);
        })

        var counter = document.getElementById("smsMaxCharId");
        counter.innerText = "0/160 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 1 SMS";

        if (_oMemoEditor) {
            _oMemoEditor.htmlEditor.on('change', function (evt) {
                var data = _oMemoEditor.getMemoBody().innerText;
                CalculateSMSLength(data);
            });

            _oMemoEditor.htmlEditor.on('key', function (e) {
                var self = this;
                setTimeout(function () {
                    var data = _oMemoEditor.getMemoBody().innerText;
                    CalculateSMSLength(data);
                }, 10);
            });

            oWizardType = SMSingWizartType.SMSing;
        }
        else//SMS unitaire
            oWizardType = SMSingWizartType.UnitSMS;
    }

    // calcul du nombre de caractère de sms
    function CalculateSMSLength(data) {
        var length_allowed1 = 0, length_sms = 0, length_stop = 0, length_allowed2 = 0;

        var first_link = false;
        var second_link = false;
        var length_links = 0;
        var length_pattern = 0;
        var messageMergefield = document.getElementById("spnMergeFieldsMsg");

        let pattern_allowed1 = /[!"#$%&\'()*+,-.\/:;<>=?@_£¤§ÄÅÆÇÉÑÖØÜß¿àäåæèéìñòöøùü¥ΔΦΓΛΩΠΨΣΘΞa-zA-Z0-9\s\n\r]/g;
        let pattern_allowed2 = /[~€\[\]{}¡^|\\\f]/g;
        let pattern_not_allowed = /[^~€\[\]{}¡^|\\\f!"#$%&\'()*+,-.\/:;<>=?@_£¤§ÄÅÆÇÉÑÖØÜß¿àäåæèéìñòöøùü¥ΔΦΓΛΩΠΨΣΘΞa-zA-Z0-9\s\n\r\u200B]/g;
        let pattern_mergefield = /\s{\w(.*?)}|{\w(.*?)}/g;
        let pattern_link = /({\w[^.^}]*}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|www\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\.[^\s]{2,}|https?:\/\/(?:www\.|(?!www))[a-zA-Z0-9]+\.[^\s]{2,}|www\.[a-zA-Z0-9]+\.[^\s]{2,})/g;
        data = data.trim();

        //$$STOP$$ mergefield
        let match = data.match(/STOP \$\$STOP\$\$/g);
        length_stop = (match && match[0] === "STOP $$STOP$$") ? match.length * 5 : 0;
        data = data.replace(/STOP \$\$STOP\$\$/g, "");

        //link and formular
        links = data.match(pattern_link);
        if (links) {
            for (var link of links) {

                if (!first_link) {
                    length_links += 13;
                    first_link = true;
                }
                else {
                    if (link.match(/{\w[^.^}]*}/g)) {
                        messageMergefield.innerText = eTools.getRes(8764);
                        second_link = true;
                    }
                    else {
                        length_pattern = length_pattern + link.length;
                        messageMergefield.innerText = "";
                    }
                }

            }
        }
        data = data.replace(pattern_link, "");

        //mergefield
        match = data.match(pattern_mergefield);
        if (match || second_link) {
            messageMergefield.innerText = eTools.getRes(8764);
        }
        else {
            messageMergefield.innerText = "";
        }
        data = data.replace(pattern_mergefield, "");

        //allowed point 1
        match = data.match(pattern_allowed1);
        length_allowed1 = match ? match.length : 0;

        //allowed point 2
        match = data.match(pattern_allowed2);
        length_allowed2 = match ? match.length * 2 : 0;

        //not allowed
        var messageNotSupported = document.getElementById("spnCharNotSupportedMsg");
        match = data.match(pattern_not_allowed);

        if (match) {
            let uniqueChars = removeDuplicates(match);
            if (uniqueChars.length > 1)
                messageNotSupported.innerText = eTools.getRes(8769) + " " + uniqueChars + " " + eTools.getRes(8770);
            else
                messageNotSupported.innerText = eTools.getRes(8762) + " " + uniqueChars + " " + eTools.getRes(8763);
        }
        else {
            messageNotSupported.innerText = "";
        }

        //total length
        length_sms = length_allowed1 + length_allowed2 + length_stop + length_links + length_pattern;

        updateCharCounter(length_sms);
    }

    //removed the duplicate chars
    function removeDuplicates(data) {
        let unique = data.reduce(function (a, b) {
            if (a.indexOf(b) < 0)
                a.push(b);
            return a;
        }, []);
        return unique;
    }


    // mise a jour de nombre de caractère de sms
    function updateCharCounter(length) {
        maxLengthMessageAttempt = false;
        var counter = document.getElementById("smsMaxCharId");
        if (counter) {
            var btnSave = document.getElementById("savecampaign_btn-mid-sms");
            if (length >= 0 && length <= 160) {
                counter.innerText = length + "/160 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 1 SMS";
                counter.style.cssText = 'color:#777';
                btnSave.setAttribute("onclick", "oSmsing.SaveAsTemplate()");
            }
            else if (length >= 161 && length <= 306) {
                counter.innerText = length + "/306 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 2 SMS";
                counter.style.cssText = 'color:#777';
                btnSave.setAttribute("onclick", "oSmsing.SaveAsTemplate()");
            }
            else if (length >= 307 && length <= 459) {
                counter.innerText = length + "/459 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 3 SMS";
                counter.style.cssText = 'color:#777';
                btnSave.setAttribute("onclick", "oSmsing.SaveAsTemplate()");
            }
            else if (length >= 460 && length <= 612) {
                counter.innerText = length + "/612 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 4 SMS";
                counter.style.cssText = 'color:#777';
                btnSave.setAttribute("onclick", "oSmsing.SaveAsTemplate()");
            }
            else if (length >= 613 && length <= 765) {
                counter.innerText = length + "/765 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 5 SMS";
                counter.style.cssText = 'color:#777';
                btnSave.setAttribute("onclick", "oSmsing.SaveAsTemplate()");
            }
            else if (length > 765) {
                counter.innerText = length + "/765 " + eTools.getRes(1461) + " " + eTools.getRes(8767) + " 5 SMS";
                counter.style.cssText = 'color:red';
                maxLengthMessageAttempt = true;
                btnSave.setAttribute("onclick", "eAlert(0, '', top._res_8768 );")
            }

        } else {
            log("oSmsing.updateCharCounter::document.getElementById('smsMaxCharId') n'est pas définit");
        }
    }

    // vérifie les champ obligat
    function isReadyToSend() {
        //Les champs généré par eRendrer
        var fields = getFieldsInfos(nCamapignTab);

        // vérification des champs en ecriture obligatoires
        for (var fldKey in fields) {
            if (typeof fields[fldKey] !== 'function' && fields[fldKey].obligat && fields[fldKey].newValue.trim() === "" && !fields[fldKey].readOnly) {

                showMessageBox
                    ({
                        'type': oMessageBoxType.ALERT,
                        'criticity': eMsgBoxCriticity.MSG_QUESTION,
                        'title': top._res_372,
                        'message': top._res_373.replace("<ITEM>", fields[fldKey].lib),
                        'detail': '',
                        'width': 450,
                        'height': 200,
                        'afterHide': null
                    });

                return false;
            }
        }
        return true;
    }

    // envoi de la campagne sms
    function send() {
        log("Sending...");
        //Les champs généré par eRendrer
        var fields = getFieldsInfos(nCamapignTab);

        var updater = new eUpdater("mgr/eSmsingManager.ashx", 0);
        updater.addParam("fileid", nCampaignId, "post");
        updater.addParam("tab", nParentBkmId, "post");

        var oSelectDestTab = document.getElementById("selectRecipientTab");
        if (oSelectDestTab) {
            updater.addParam("mailTabDescId", oSelectDestTab.options[oSelectDestTab.selectedIndex].value, "post");
        }
        updater.addParam("operation", "3", "post"); //1:save, 3:saveAndSend
        updater.addParam("parenttab", nParentTabId, "post");
        updater.addParam("parentfileid", nParentFileId, "post");
        updater.addParam("removeDoubles", "1", "post");
        updater.addParam("typeMailing", "3", "post");

        var description = GetParam("description");
        if (description != null && description.length > 0)
            updater.addParam("description", description, "post");

        var recSend = GetParam("recurrentSending") == 1
        var rqMode = GetParam("RequestMode")


        if (GetParam('eventStepDescId') == '0' || GetParam('eventStepDescId') == null) {
            var inputEventStepDescId = document.getElementById('eventStepDescId');
            if (inputEventStepDescId != null)
                SetParam('eventStepDescId', getAttributeValue(inputEventStepDescId, 'value'));
        }


        //Si envoi programmé
        if (recSend) {

            if (GetParam('eventStepDescId') == '0' || GetParam('eventStepDescId') == null) {
                var inputEventStepDescId = document.getElementById('eventStepDescId');
                if (inputEventStepDescId != null)
                    SetParam('eventStepDescId', getAttributeValue(inputEventStepDescId, 'value'));
            }

            //Etapes Marketting
            if (GetParam('eventStepDescId') == '0' || GetParam('eventStepDescId') == null) {
                eAlert(MsgType.MSG_CRITICAL.toString(), top._res_416, top._res_2683, "");
                return;
            }

            //Champs obligatoire
            var obligatFields = "";

            //2040,"Planification"
            if (getNumber(GetParam("scheduleId")) <= 0 || GetParam("scheduleId") == null)
                obligatFields += " - " + top._res_2040 + "[[BR]]";

            //2042,"Destinataires de l'envoi récurrent"
            if (GetParam("RequestMode") != SMSingQueryMode.RECURRENT_ALL && GetParam("RequestMode") != SMSingQueryMode.RECURRENT_FILTER)
                obligatFields += " - " + top._res_2042 + "[[BR]]";


            //2043,"Filtre destinataires"
            if (GetParam("RequestMode") == SMSingQueryMode.RECURRENT_FILTER && (GetParam("recipientsFilterId") == null || getNumber(GetParam("recipientsFilterId")) <= 0))
                obligatFields += " - " + top._res_2043 + "[[BR]]";



            if (obligatFields != "") {
                obligatFields = top._res_6564 + "[[BR]][[BR]]" + obligatFields;
                eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, obligatFields, "");
                return false;
            }




            //Récupération des paramètre de planification
            var evtStepDescId = GetParam('eventStepDescId');
            var schld = GetParam("scheduleId");
            var flt = GetParam("recipientsFilterId");





            //
            updater.addParam("eventStepDescId", evtStepDescId, "post");

            //id du schedule            
            updater.addParam("scheduleid", schld, "post");
            updater.addParam("recurrentSending", 1, "post");
            updater.addParam("immediateSending", 0, "post");
            updater.addParam("requestmode", rqMode, "post");



            //filtre           
            if (flt != null)
                updater.addParam("recipientsFilterId", flt, "post");
        }
        else {
            var evtStepDescId = GetParam('eventStepDescId');
            if (evtStepDescId)
                updater.addParam("eventStepDescId", evtStepDescId, "post");
            updater.addParam("immediateSending", 1, "post");
            updater.addParam("recurrentSending", 0, "post");
        }

        //paramètres spécifiques à l'edition du sms
        for (var fldKey in fields)
            if (typeof fields[fldKey] !== 'function')
                updater.addParam('field_' + fields[fldKey].descId, fields[fldKey].GetSerialize(), "post");

        updater.ErrorCallBack = manageError;

        showMessageBox
            ({
                'type': oMessageBoxType.WAIT,
                'title': top._res_307,
                'message': top._res_6636.replace("<ACTION>", top._res_6632)
            });

        updater.send(manageResult);
    }

    // gére le retour serveur
    function manageResult(oDoc) {
        log("manageResult");
        log(oDoc);
        // 
        var success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]) === "1";
        var message = getXmlTextNode(oDoc.getElementsByTagName("message")[0]);
        var userAdress = getXmlTextNode(oDoc.getElementsByTagName("useraddress")[0]);
        var userMainEmail = getXmlTextNode(oDoc.getElementsByTagName("usermainemail")[0]);
        var id = getNumber(getXmlTextNode(oDoc.getElementsByTagName("id")[0]));

        var detail = "";
        if (!success) {

            log("oSmsing.manageResult.failure");
            message = message;
            detail = getXmlTextNode(oDoc.getElementsByTagName("detail")[0]);
        }
        else {
            var recSend = GetParam("recurrentSending") == 1; //recSend : envoi programmé
            if (typeof recSend !== "undefined") {
                message = recSend ? top._res_2707 : top._res_306;
                detail = recSend ? top._res_2817 : top._res_6128 + "<br />" + top._res_6077.replace("<USER_ADDRESS>", userMainEmail);
            }
        }

        showMessageBox({
            'type': oMessageBoxType.ALERT,
            'criticity': eMsgBoxCriticity.MSG_INFOS,
            'title': top._res_5080,
            'message': message,
            'detail': detail,
            'width': 450,
            'height': 200,
            'afterHide': success ? closeWizard : null
        });
    }

    //Ferme l'assistant
    function closeWizard() {

        if (oMessageBox !== null && typeof oMessageBox.hide === "function") {
            oMessageBox.hide();
            oMessageBox = null;
        }
        else
            log("oSmsing.manageError::oMessageBox n'est pas initialisée");

        if (top) {
            if (top.window['_md']['SmsMailingWizard'])
                top.window['_md']['SmsMailingWizard'].hide();
            else
                log("oSmsing.closeWizard :: top.window['_md']['SmsMailingWizard'] n'est pas définit");
        } else
            log("oSmsing.closeWizard :: top n'est pas définit");
    }

    //Affiche un message en fonction de type de traitement
    function showMessageBox(oMsgBoxOptions) {

        // On ferme l'ancienne box (pas free)
        if (oMessageBox !== null && typeof oMessageBox.hide === "function") {
            oMessageBox.hide();
            oMessageBox = null;
        }
        else {
            log("oSmsing.manageError::oMessageBox n'est pas initialisée");
        }

        // on ouvre la nouvelle box (pas b)
        switch (oMsgBoxOptions.type) {
            case oMessageBoxType.ALERT:
                oMessageBox = eAlert(oMsgBoxOptions.criticity, oMsgBoxOptions.title, oMsgBoxOptions.message, oMsgBoxOptions.detail, oMsgBoxOptions.width, oMsgBoxOptions.height, oMsgBoxOptions.afterHide);
                break;
            case oMessageBoxType.CONFIRM:
                oMessageBox = eConfirm(oMsgBoxOptions.criticity, oMsgBoxOptions.title, oMsgBoxOptions.message, oMsgBoxOptions.detail, oMsgBoxOptions.width, oMsgBoxOptions.height, oMsgBoxOptions.onOk, oMsgBoxOptions.onCancel, oMsgBoxOptions.okGreen);
                break;
            case oMessageBoxType.WAIT:
                oMessageBox = showWaitDialog(oMsgBoxOptions.title, oMsgBoxOptions.message);
                break;
        }
    }

    // problème de connexion serveur
    function manageError(oDoc) {
        log("manageError");
        log(oDoc);

        if (oMessageBox !== null && typeof oMessageBox.hide === "function") {
            oMessageBox.hide();
            oMessageBox = null;
        }
        else
            log("oSmsing.manageError::oMessageBox n'est pas initialisée");

    }

    // affiche des logs sur la console
    function log(msg) {
        if (bDebug && console)
            console.log(msg);
    }

    ///summary
    ///Affecte un paramètre simple(valeur unique associée à la clé) au sms
    ///<param name="key">clé de paramètre de sms</param>
    ///<param name="val">valeur</param>
    ///summary
    function SetParam(key, val) {

        if (key == null || typeof key !== "string" || key.length < 1)
            return null;


        key = key.toLocaleLowerCase();

        __oSmsParams[__sSmsParamPrefix + key] = val;

    }

    function GetParam(key) {

        if (key == null || typeof key !== "string" || key.length < 1)
            return null;


        key = key.toLocaleLowerCase();

        if (__oSmsParams.hasOwnProperty(__sSmsParamPrefix + key))
            return __oSmsParams[__sSmsParamPrefix + key]

        return null;
    }

    ///<summary>
    /// Affiche ou cache les choix de l'envoi différé
    ///<summary>
    function DisplayRequestMode(bDelayedMode) {

    }

    ///<summary>
    /// Affiche ou cache les choix de l'envoi récurrent
    ///<summary>
    function DisplayRecurrentMode(bDelayedRecurrentMode) {
        var oDelayedMailRecurrent_RequestMode = document.getElementById("recurringSendingBlock");
        if (!oDelayedMailRecurrent_RequestMode) {
            return;
            //throw ("oDelayedMailRecurrent_RequestMode introuvable !");
        }

        oDelayedMailRecurrent_RequestMode.style.display = (bDelayedRecurrentMode) ? "block" : "none";
    }

    function DisplayRecipientsFilter(bRecipientsFilter) {
        var oDelayedMailRecurrent_Filter = document.getElementById("delayedMailRecurrent_Filter");
        if (!oDelayedMailRecurrent_Filter) {
            return;
            throw ("oDelayedMailRecurrent_Filter introuvable !");
        }
        oDelayedMailRecurrent_Filter.style.display = (bRecipientsFilter) ? "block" : "none";
    }

    function openScheduleParameterValidReturn(oModal) {
        oModal.getIframe().Valid(ValidMailingScheduleTreatment, oModal);
    }

    function openScheduleParameterCancelReturn(oModal) {
        oModal.hide();
    }

    function ValidMailingScheduleTreatment(oRes, oModal) {
        var scheduleId = getXmlTextNode(oRes.getElementsByTagName("scheduleid")[0]);


        SetParam("scheduleId", scheduleId);
        SetParam("scheduleUpdated", "1");
        oModal.hide();

        var scheduleInfo = document.getElementById("lnkScheduleInfo");
        if (scheduleInfo) {
            scheduleInfo.style.display = "";
            SetText(scheduleInfo, " : \"" + getXmlTextNode(oRes.getElementsByTagName("scheduleinfo")[0]) + "\"");
        }
    }

    function applyRecipientsFilterModal(modal) {
        var currentFilterId = "0";
        var currentFilterLib = "";
        var currentFilter = modal.getIframe()._eCurentSelectedFilter;
        if (currentFilter) {
            // Recup de l'id du filtre
            var oId = currentFilter.getAttribute("eid").split('_');
            currentFilterId = oId[oId.length - 1];

            // Recupe du libelle du filtre
            var tabLibFilter = currentFilter.querySelectorAll("div[ename='COL_104000_104001']");
            if (tabLibFilter.length > 0) {
                currentFilterLib = tabLibFilter[0].innerHTML.trim();
            }
        }
        SetParam("recipientsFilterId", currentFilterId);

        var filterInfo = document.getElementById("lnkFilterInfo");
        if (filterInfo) {
            filterInfo.style.display = currentFilterId != "0" ? "" : "none";
            SetText(filterInfo, " : \"" + currentFilterLib + "\"");
        }

        modal.hide();
    }

    //Créer une instance au chargement du fichier
    return {
        CalculateSMSLength: CalculateSMSLength,
        GetType: function () { return "smsing" },

        IsMaxLengthMessageAttempted: function () { return maxLengthMessageAttempt; },

        SetParam: SetParam,

        GetParam: GetParam,

        GetWizardType: function () { return oWizardType },

        SetDebug: function (bActive) { bDebug = bActive; },


        TemplateId: nTemplateId,

        SetVersionPresta: function (sPresta) {
            versionPresta = sPresta
        },


        SetBkmLabel: function (sLabel) {
            _sLabel = sLabel;
        },

        GetVersionPresta: function () {
            return versionPresta
        },

        //Params de la campagne de sms
        SetCampaignId: function (nCampId) { nCampaignId = nCampId; },

        SetParentFileId: function (nPrtFileId) {

            nParentFileId = nPrtFileId;
            if (_oMemoEditor)
                _oMemoEditor._nParentFileId = nPrtFileId;
        },

        SetParentTabId: function (nPrtTab) {
            nParentTabId = nPrtTab;
            if (_oMemoEditor)
                _oMemoEditor._nParentTabId = nPrtTab;
        },

        SetParentBkmId: function (nPrtBkm) {

            nParentBkmId = nPrtBkm;
            if (_oMemoEditor)
                _oMemoEditor._tab = nPrtBkm;
        },


        GetMemoEditor: function () {
            if (_oMemoEditor)
                return _oMemoEditor
            else {
                nsMain.getAllMemoEditorIDs().forEach(function (memeEdit, idx) {
                    _oMemoEditor = nsMain.getMemoEditor(memeEdit);
                })

                return _oMemoEditor

            }
        },

        SetTemplateId: function (nTplId) {

            nTemplateID = nTplId
        },


        GetTemplateId: function () {

            return nTemplateID
        },

        GetBkmTab: function () { return nParentBkmId },

        GetParentTab: function () { return nParentTabId },


        GetParentFileId: function () { return nParentFileId },


        // Initilisation l'objet
        Init: function () {
            log("Init");
            init();
        },

        //Mise à jour du compteur des caractères
        UpdateSmsContent: function (evt) {
            var textarea = evt.srcElement || evt.target;
            if (textarea) {
                CalculateSMSLength(textarea.value);
            } else {
                log("oSmsing.UpdateSmsContent:: evt.srcElement || evt.target n'est pas définit");
                log(evt);
            }
        },

        // Lance la commande d'envoi après confirmation
        Send: function () {
            if (isReadyToSend())
                showMessageBox
                    ({
                        'type': oMessageBoxType.CONFIRM,
                        'criticity': eMsgBoxCriticity.MSG_QUESTION,
                        'title': top._res_201,
                        'message': top._res_6635,
                        'detail': '',
                        'width': 450,
                        'height': 200,
                        'onOk': send,
                        'onCancel': null,
                        'okGreen': true
                    });
        },

        // Annule l'edition du sms après confirmation
        Cancel: function () {
            log("Cancel");

            showMessageBox
                ({
                    'type': oMessageBoxType.CONFIRM,
                    'criticity': eMsgBoxCriticity.MSG_QUESTION,
                    'title': top._res_201,
                    'message': top._res_6863,
                    'detail': top._res_6864,
                    'width': 450,
                    'height': 200,
                    'onOk': closeWizard,
                    'onCancel': null,
                    'okGreen': true
                });
        },

        OnSelectDelayed_Now: function () {
            SetParam('immediateSending', '1');
            SetParam('sendingDate', '');
            SetParam('recurrentSending', '0');

            /*ELAIZ - demande de régression 76119 - rajout d'une class CSS open sur le conteneur de la programmation d'envoi d' 
        une campagne emailing*/
            if (objThm.Version == 2 && event.target.checked)
                document.querySelector('.mailingDelayedMail').classList.remove('open');



            DisplayRecurrentMode(false);

        },

        //Mode programmé
        OnSelectDelayed_Recurrent: function () {

            SetParam('immediateSending', '0');
            SetParam('sendingDate', '');
            SetParam('recurrentSending', '1');
            DisplayRequestMode(false);
            DisplayRecurrentMode(true);

            /*ELAIZ - demande de régression 76119 - rajout d'une class CSS open sur le conteneur de la programmation d'envoi d'
    une campagne emailing*/
            if (objThm.Version == 2 && event.target.checked)
                document.querySelector('.mailingDelayedMail').classList.add('open');
        },

        OnSelectRequestMode: function (oRBRequestMode) {
            this.SetParam('RequestMode', oRBRequestMode.value);
            DisplayRecipientsFilter(oRBRequestMode.value == SMSingQueryMode.RECURRENT_FILTER);
        },

        openScheduleParameter: function () {
            var nNew = 0;
            if (nCampaignId != 0)
                nNew = 1;

            // On choisi le prochain creaneau d'heure
            var d = new Date();
            while (d.getMinutes() % 30 != 0) {
                d.setMinutes(d.getMinutes() + 1);
            }
            var hours = ("0" + d.getHours()).slice(-2);
            var minutes = ("0" + d.getMinutes()).slice(-2);

            _modalSchedule = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 450, 500);

            _modalSchedule.addParam("scheduletype", 4, "post");
            _modalSchedule.addParam("New", nNew, "post");
            _modalSchedule.addParam("iframeScrolling", "yes", "post");
            _modalSchedule.addParam("EndDate", 0, "post");
            _modalSchedule.addParam("BeginDate", 0, "post");
            _modalSchedule.addParam("ScheduleId", GetParam("scheduleId"), "post");
            _modalSchedule.addParam("Tab", 0, "post");
            _modalSchedule.addParam("Workingday", "TODO", "post");
            _modalSchedule.addParam("calleriframeid", 0, "post");

            if (GetParam("scheduleId") == null)
                _modalSchedule.addParam("hour", hours + ":" + minutes, "post");

            _modalSchedule.addParam("AppType", 0, "post");
            _modalSchedule.addParam("FileId", nCampaignId, "post");

            _modalSchedule.ErrorCallBack = openScheduleParameterCancelReturn(_modalSchedule);

            _modalSchedule.show();
            _modalSchedule.addButtonFct(top._res_29, function () { openScheduleParameterCancelReturn(_modalSchedule); }, "button-gray", 'cancel');
            _modalSchedule.addButtonFct(top._res_28, function () { openScheduleParameterValidReturn(_modalSchedule); }, "button-green");
        },

        openRecipientsFilterModal: function (nTab) {
            var options = {
                tab: nTab,
                onApply: applyRecipientsFilterModal,
                value: GetParam("recipientsFilterId"),
                deselectAllowed: true,
                selectFilterMode: true,
            }

            filterListObjet(0, options);
        }
        ,

        TypeWizard: "smsing",

        SaveAsTemplate: function () {
            if (oCurrentWizard.TypeWizard == undefined && oSmsing.GetType() == "smsing") {
                wizardType = 'smsmailing';
                oCurrentWizard = oSmsing;
                thtabevt.init();
                CreateHtmlEditors(wizardType);
                oCurrentWizard.Init();
            }

            beforeSaveTemplate();
        },

        ShowTemplateList: function () {


            ShowMailTemplateList(2
                , _sLabel
                , nParentBkmId
                , nParentFileId
                , null
                , this.GetMemoEditor()
                , false);
        },

        ControlStep: function (step) {


            switch (this.GetStepName(step)) {
                case "smsbody":
                    if (oSmsing.GetMemoEditor().getMemoBody().textContent) {
                        if (oSmsing.GetMemoEditor().getMemoBody().textContent.length == 0) {
                            eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, "Message");
                            return false;
                        }
                    }
                    else if (oSmsing.GetMemoEditor().getMemoBody().value) {
                        if (oSmsing.GetMemoEditor().getMemoBody().value.length == 0) {
                            eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, "Message");
                            return false;
                        }
                    }
                    return true;

                case "cpgtyp":
                    return true;
            }
        },

        GetStepName: function (step) {

            var stepDiv = document.getElementById("editor_" + step);

            return getAttributeValue(stepDiv, "stepName")
        }
    };
}());

function switchToContainer(input, event) {
    let AllContents = document.getElementsByClassName('field-container--content');
    for (var i = 0; i < AllContents.length; i++) {
        AllContents[i].style.display = 'none';
    }
    let idElem = input.id;
    let contentElem = document.getElementsByClassName(idElem)[0];
    contentElem.style.display = 'block';

    if (idElem == "immediateDispatch")
        oSmsing.OnSelectDelayed_Now();
    if (idElem == "recurringSending")
        oSmsing.OnSelectDelayed_Recurrent()
}


