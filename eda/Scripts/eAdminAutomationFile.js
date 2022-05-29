
// les fonctions propre à l'admin des automatismes
var oAutomation = (function () {

    var _nTargetTab;
    var _automationId;
    var _fields;
    var _trigger_query = "fld_114200_";
    var _trigger_res_query = "fld_114100_";

    function displayPicto(target) {

        var oPicto = new ePictogramme(target.id, {
            'tab': _nTargetTab,
            'color': getAttributeValue(target, "picto-color"),
            'iconKey': getAttributeValue(target, "picto-key"),
            'width': 800,
            'height': 600,
            'callback': update
        });

        oPicto.show();
    }
    function update(picto) {
        var target = document.getElementById(picto.targetId);
        setAttributeValue(target, "dbv", picto.key);

        var colorTarget = document.getElementById(getAttributeValue(target, "fld-color"));
        setAttributeValue(colorTarget, "dbv", picto.color);
    }
    function manageMenuClick(evt, imageSourceId, imageDescId) {
        var target = evt.srcElement || evt.target;
        var menu = document.getElementById("visualMenu");
        if (target.tagName == "LI")
            target = target.querySelector("span");

        if (target.id == "visualMenu" || target.id == "selected-visual" || target.id == "arrow-down") {

            var arrow = document.getElementById("arrow-down");
            if (getAttributeValue(menu, "display") == "on") {
                switchClass(arrow, "icon-caret-up", "icon-caret-down");
                setAttributeValue(menu, "display", "off");
            }
            else {
                switchClass(arrow, "icon-caret-down", "icon-caret-up");
                setAttributeValue(menu, "display", "on");
            }

        } else {

            var span = document.getElementById("selected-visual");

            if (target.id == "")
                return;

            // on affiche le picto picker
            if (target.id == "2") {

                setAttributeValue(menu, "picto-picker", "on");
                var picto = document.getElementById(getAttributeValue(target.parentElement, "picto"));
                setTimeout(function () { displayPicto(picto) }, 100);
            }
            else {
                setAttributeValue(menu, "picto-picker", "off");
            }

            var arrow = document.getElementById("arrow-down");
            switchClass(arrow, "icon-caret-up", "icon-caret-down");


            // on mis à jour la sélection

            setAttributeValue(span, "title", target.innerHTML);
            setAttributeValue(span, "dbv", target.id);

            // masque le menu             
            setAttributeValue(menu, "display", "off");

            // mis a jour les champ sourceImage et sourceDescId
            var imgSrc = getAttributeValue(target.parentElement, "imgsrc");
            var imageSource = document.getElementById(imageSourceId);
            setAttributeValue(imageSource, "dbv", imgSrc);

            var descid = getAttributeValue(target.parentElement, "descid");
            var imageDesc = document.getElementById(imageDescId);
            setAttributeValue(imageDesc, "dbv", descid);

            span.innerHTML = getAttributeValue(target.parentElement, "lib");;
        }
    }
    function fillFields(elements, elementsRes) {

        _fields = new Array();
        var parts, subFields;
        forEachElement(elements, function (fld) {
            parts = fld.id.split("_");
            if (parts.length < 2)
                return;

            var dbv = "";
            if (hasClass(fld, "chk")) {
                dbv = getAttributeValue(fld, "chk");
            }
            else {
                dbv = getAttributeValue(fld, "dbv");
            }

            _fields.push({
                descId: parts[2],
                dbv: dbv,
                fmt: getAttributeValue(fld, "fmt"),
                obligat: getAttributeValue(fld, "obligat")
            });
        });
        forEachElement(elementsRes, function (fld) {
            parts = fld.id.split("_");
            if (parts.length < 2)
                return;

            _fields.push({
                descId: parts[2],
                dbv: encode(fld.innerHTML),
                fmt: 9, // memo
                obligat: 0
            });
        });
    }
    function checkFields() {

    }
    function save(callback) {

        var mainDiv = document.getElementById("mainDiv");

        var oUpd = new eUpdater("eda/mgr/eAdminAutomationManager.ashx", 0);

        oUpd.addParam("fileid", _automationId, "post");
        oUpd.addParam("action", "save", "post");
        oUpd.addParam("tab", _nTargetTab, "post");
        forEachElement(_fields, function (fld) { oUpd.addParam("fld_" + fld.descId, fld.dbv, "post"); });

        setWait(true);
        oUpd.send(function (oRes) {
            setWait(false);
            if (success(oRes) && typeof (callback) == "function")
                callback(oRes);
        });
    }
    function success(oRes) {
        if (!oRes)
            return false;

        try {
            var bSuccess = getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1";
            if (bSuccess) return true;

            var error = getXmlTextNode(oRes.getElementsByTagName("error")[0]);
            eAlert(0, top._res_416, error);
        }
        catch (e) { eAlert(0, top._res_416, e.message); }

        return false;
    }
    function forEachElement(elements, fct) {
        var elm;
        for (var i = 0; i < elements.length ; i++) {
            elm = elements[i];
            fct(elm);
        }
    }
    function openUserCat(targetId) {

        var modalUserCat = new eModalDialog(top._res_246, 0, "eCatalogDialogUser.aspx", 550, 610);
        //top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);
        modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
        modalUserCat.ErrorCallBack = function () { setWait(false); }
        modalUserCat.addParam("multi", "1", "post");

        //var topModal = document.getElementById(modalRights.iframeId);
        //var topModalContent = topModal.contentDocument || topModal.contentWindow.document;
        var oTarget = document.getElementById(targetId);
        modalUserCat.addParam("selected", getAttributeValue(oTarget, "dbv"), "post");

        modalUserCat.addParam("modalvarname", "modalUserCat", "post");
        modalUserCat.show();

        modalUserCat.addButton(top._res_29, function () {
            modalUserCat.hide();
        }, "button-gray", null, null, true);

        modalUserCat.addButton(top._res_28, function () {

            var strReturned = modalUserCat.getIframe().GetReturnValue();
            var vals = strReturned.split('$|$')[0];
            var libs = strReturned.split('$|$')[1];

            oTarget.setAttribute("title", libs);
            oTarget.innerHTML = libs
            oTarget.setAttribute("dbv", vals);
            modalUserCat.hide();

        }, "button-green", targetId);
    }
    function openMemo(descid, title) {


        var lWidth = 900;
        var lHeight = 600;
        var oAutomationModal = new eModalDialog(title, 0, "eMemoDialog.aspx", lWidth, lHeight);

        var target = document.getElementById(_trigger_res_query + "" + descid);
        var html = "";
        if (target)
            html = encode(target.innerHTML);


        // Instanciation d'un objet eMemoEditor "interne"
        var oAutomationMemo = new eMemoEditor("oAutoMemo_" + descid, true, null, null, "", true, "oAutomationMemo");
        oAutomationMemo.childDialog = oAutomationModal;
        oAutomationMemo.mergeFields = oMergeFields;   // liste des champs de fusion 
        oAutomationMemo.toolbarType = "automation";
        oAutomationMemo.automationEnabled = true;

        //oAutomationModal.addParam("EditorJsVarName", "oAutomationMemo", "post");
        oAutomationModal.addParam("ParentFrameId", oAutomationModal.iframeId, "post");
        oAutomationModal.addParam("ParentEditorJsVarName", "oAutoMemo_" + descid, "post");
        oAutomationModal.addParam("divMainWidth", lWidth, "post");
        oAutomationModal.addParam("divMainHeight", lHeight, "post");
        oAutomationModal.addParam("IsHTML", "1", "post");
        oAutomationModal.addParam("Value", html, "post");
        oAutomationModal.addParam("toolbarType", oAutomationMemo.toolbarType, "post");

        oAutomationModal.ErrorCallBack = launchInContext(oAutomationModal, oAutomationModal.hide);

        oAutomationModal.onIframeLoadComplete = function () { oAutomationModal.getIframe().onFrameSizeChange(lWidth, lHeight - 40); };
        oAutomationModal.show();

        oAutomationModal.addButton(top._res_29, oAutomationModal.hide, "button-gray", "oAutomationMemo"); // Annuler
        oAutomationModal.addButton(top._res_28,
            function () {
                target.innerHTML = oAutomationModal.getIframe().eMemoDialogEditorObject.getData(); oAutomationModal.hide();
            }, "button-green", "oAutomationMemo"); // Valider

    }
    function openFieldSelect(target) {


        var fieldSelectorModal = new eModalDialog(top._res_96, 0, "eFieldsSelect.aspx", 850, 550);

        fieldSelectorModal.addParam("tab", _nTargetTab, "post");
        fieldSelectorModal.addParam("action", "tabuserfields", "post");
        fieldSelectorModal.addParam("listCol", getAttributeValue(target, "dbv"), "post");

        fieldSelectorModal.ErrorCallBack = fieldSelectorModal.hide;
        fieldSelectorModal.show();

        fieldSelectorModal.addButton(top._res_29, fieldSelectorModal.hide, "button-gray", _nTargetTab);
        fieldSelectorModal.addButton(top._res_28,
            function () {

                var result = fieldSelectorModal.getIframe().getSelectedDescIdObject();

                setAttributeValue(target, "dbv", result.dbv);
                target.innerHTML = result.lib;

                fieldSelectorModal.hide();

            }, "button-green", _nTargetTab);
    }
    function showFilter(fieldId) {

        var field = document.getElementById(fieldId);
        var filterId = getAttributeValue(field, "dbv");

        // cancelFilter est dans eFilterWizard
        var cancelBtn = new eModalButton(top._res_29, function () { cancelAutomationFilter(); }, "button-gray");
        var validateBtn = new eModalButton(top._res_28, function () { validateFilter({ targetId: fieldId, filterId: filterId }); }, "button-green");
        var renameBtn = new eModalButton(top._res_86, function () { renameFilter({ targetId: fieldId, filterId: filterId }); }, "button-gray");

        if (filterId > 0) {
            editFilter(filterId, _nTargetTab, [cancelBtn, validateBtn, renameBtn]);
            oModalFilterWizard.addParam("CalllBackSuccess", function (oRes) { validateFilterReturn({ oRes: oRes, targetId: fieldId, modal: oModalFilterWizard }); });
        } else {
            removeFilter(fieldId)
        }
    }
    function cancelAutomationFilter() {
        if (oModalFilterWizard)
            oModalFilterWizard.hide();

    }
    function validateFilter(options) {

        var target = document.getElementById(options.targetId);
        if (getAttributeValue(target, "dbv") == "0")
            saveFilter();
        else
            oModalFilterWizard.getIframe().saveDb(0, null, false,
                function (oRes) { validateFilterReturn({ oRes: oRes, targetId: options.targetId, modal: oModalFilterWizard }); });
    }
    function validateFilterReturn(options) {

        var filterId = getXmlTextNode(options.oRes.getElementsByTagName("filterid")[0]);
        var filterName = getXmlTextNode(options.oRes.getElementsByTagName("filtername")[0]);
        var filterTitle = getXmlTextNode(options.oRes.getElementsByTagName("filterdescription")[0]);

        var target = document.getElementById(options.targetId);
        setAttributeValue(target, "dbv", filterId);

        target = document.getElementById("filterNameBlock");
        target.innerHTML = filterName;
        setAttributeValue(target.parentElement, "title", filterTitle);

        updateConditionBlockDisplay(filterId);
        options.modal.hide();
    }
    function renameFilter(options) {
        saveFilterAs();
    }
    function removeFilter(fieldId) {
        var target = document.getElementById(fieldId);
        setAttributeValue(target, "dbv", "0");
        target = document.getElementById("filterNameBlock");
        target.innerHTML = "";
        setAttributeValue(target.parentElement, "title", "");
        updateConditionBlockDisplay(0);
    }
    function updateConditionBlockDisplay(filterId) {

        var allBlockItems = document.querySelectorAll("[id^=trigger_all_]");
        var ifBlockItems = document.querySelectorAll("[id^=trigger_filter_]");

        // masque/affiche les blocks "DANS TOUS LES CAS",  "Ajouter une condtion"
        forEachElement(allBlockItems, function (item) {
            if (filterId > 0)
                setAttributeValue(item, "data-active", "0");
            else
                setAttributeValue(item, "data-active", "1");
        });

        // masque/affiche les blocks "IF",  "condition" et "ALORS"
        forEachElement(ifBlockItems, function (item) {
            if (filterId > 0)
                setAttributeValue(item, "data-active", "1");
            else
                setAttributeValue(item, "data-active", "0");
        });
    }
    function updateFilterField(options) {

        var filterContext = options.modal.getIframe();
        if (filterContext._eCurentSelectedFilter != null && filterContext._eCurentSelectedFilter.getAttribute("eid")) {
            var oId = filterContext._eCurentSelectedFilter.getAttribute("eid").split('_');
            var nTab = filterContext._eCurentSelectedFilter.getAttribute("eft");
            var filterId = oId[oId.length - 1];

            var url = "mgr/eFilterWizardManager.ashx";
            var ednu = new eUpdater(url, 0);
            ednu.addParam("action", "getdesc", "post");
            ednu.addParam("filterid", filterId, "post");
            ednu.send(function (oRes) {
                validateFilterReturn({ oRes: oRes, targetId: options.targetId, filterId: options.filterId, modal: options.modal });
            });
        }
        else {
            showWarning(top._res_719, top._res_430, "");
        }
    }
    return {
        init: function () {
            var mainDiv = document.getElementById("mainDiv");
            _nTargetTab = getAttributeValue(mainDiv, "tab");
            _automationId = getAttributeValue(mainDiv, "fileid");
        },
        picto: function (target) { displayPicto(target); },
        newCondition: function (fieldId) {
            filterListObjet(8, { // 8 : type de filtre notification
                tab: _nTargetTab,
                adminMode: true,
                onApply: function (modal) {
                    updateFilterField({ targetId: fieldId, filterId: 0, modal: modal });

                },
            });
        },
        clickMenu: function (evt, imageSourceId, imageDescId) {
            manageMenuClick(evt, imageSourceId, imageDescId);
        },
        openMemo: function (descid, title) { openMemo(descid, title); },
        broadcast: function (cbo, attr, broadcastId, broadcastValue) {
            var checked = getAttributeValue(cbo, "chk") == "1";
            var parent = document.getElementById('innerCases');
            if (parent)
                setAttributeValue(parent, attr, checked ? "1" : "0");

            var broadcastField = document.getElementById(broadcastId);
            var current = getAttributeValue(broadcastField, "dbv");


            var mail = getAttributeValue(parent, "mail") == "1";
            var others = getAttributeValue(parent, "others") == "1";

            if (checked)
                current = current | broadcastValue;
            else
                if (attr == "describe" || (!others && !mail))
                    current = current & ~broadcastValue;

            setAttributeValue(broadcastField, "dbv", current);
        },
        freeMail: function (cbo, freeMailId) {
            var checked = getAttributeValue(cbo, "chk") == "1";
            var freeMailInput = document.getElementById(freeMailId);
            if (freeMailInput) {
                if (checked) {
                    setAttributeValue(freeMailInput, "dbv", getAttributeValue(freeMailInput, "old"));
                } else {
                    setAttributeValue(freeMailInput, "old", getAttributeValue(freeMailInput, "dbv"));
                    setAttributeValue(freeMailInput, "dbv", "");
                }
            }
        },
        setDbvVal: function (id, val) {
            var element = document.getElementById(id);
            if (element)
                setAttributeValue(element, "dbv", val);

        },
        validate: function (callback) {
            // debute par _query
            var elements = document.querySelectorAll("[id^=" + _trigger_query + "]"); // déclencheurs
            var elementsRes = document.querySelectorAll("[id^=" + _trigger_res_query + "]"); // ressources déclencheur
            fillFields(elements, elementsRes);
            checkFields();
            save(callback);
        },
        openUserCat: function (targetId) {
            openUserCat(targetId);
        },
        fldSelector: function (descid) {
            var target = document.getElementById(_trigger_query + descid);
            if (target)
                openFieldSelect(target);
        },
        editFilter: function (fieldId) { showFilter(fieldId); },
        deleteFilter: function (fieldId) {
            var field = document.getElementById(fieldId);
            var filterId = getAttributeValue(field, "dbv");
            if (filterId != "" && filterId > 0)
                eAdvConfirm({
                    'criticity': eAdvConfirm.CRITICITY.MSG_EXCLAM,
                    'title': top._res_806,
                    'message': top._res_7483, // "L'automatisme sera déclenché dans tous les cas.",
                    'details': top._res_7484, //"Retirer la condition ?",
                    'bOkGreen': false,
                    'resOk': top._res_28,
                    'resCancel': top._res_29,
                    'okFct': function () { removeFilter(fieldId); }
                });

        },
        checkDurationValue: function (elem, val) {
            var max = parseInt(getAttributeValue(elem, 'emax'));
            var min = parseInt(getAttributeValue(elem, 'emin'));
            if (isNaN(parseInt(val))) {
                eAlert(0, top._res_1833, top._res_8018, top._res_1128, null, null, function () {
                    elem.value = '';
                    elem.focus();
                    elem.select();
                });

                return;
            }
            if (!isNaN(min) && !isNaN(max)) {
                if (val > max) {
                    val = max;
                    eAlert(1, top._res_6733, top._res_499.replace('<MAX_DAYS>', val));

                }

                if (val < min) {
                    val = min;
                    eAlert(1, top._res_6733, top._res_609.replace('<MIN_DAYS>', val));
                }
                elem.value = val;
            }
            setAttributeValue(elem, 'dbv', val);
        }
    }
}());


