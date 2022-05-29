var eFormularPickADate = eFormularPickADate || {};

eFormularPickADate.Init = function () {
    var aDateBtns = document.querySelectorAll("span.icon-agenda");
    for (var k = 0; k < aDateBtns.length; k++) {
        var btn = aDateBtns[k];
        setEventListener(btn, 'click', (function (elt) { return function () { eFormularPickADate.pickADate(elt); }; })(btn), false);
    }
}

eFormularPickADate.pickADate = function (btn) {
    var input = btn.parentElement.querySelector("#" + getAttributeValue(btn, "eacttg"));
    var label = btn.parentElement.querySelector("#" + getAttributeValue(input, "ename"));
    var sDate = getAttributeValue(input, "dbv");

    var cultureInfo = document.getElementById("CultureInfo").value;
    var param_tok = document.getElementById("tok").value;
    var param_cs = document.getElementById("cs").value;
    var param_p = document.getElementById("p").value;

    eFormularPickADate.pickADateDialog = new eModalDialog(label.getAttribute("lib"), 0, "mgr/ePickADateExternalManager.ashx", 300, 415);
    eFormularPickADate.pickADateDialog.addParam("date", sDate, "post");
    eFormularPickADate.pickADateDialog.addParam("from", "3", "post");
    eFormularPickADate.pickADateDialog.addParam("tok", param_tok, "post");
    eFormularPickADate.pickADateDialog.addParam("cs", param_cs, "post");
    eFormularPickADate.pickADateDialog.addParam("p", param_p, "post");

    eFormularPickADate.pickADateDialog.onIframeLoadComplete = (function (modal) {
        return function () {
            modal.getIframe().eDate.SetCultureInfo(cultureInfo);
        };
    })(eFormularPickADate.pickADateDialog);

    eFormularPickADate.pickADateDialog.show();
    eFormularPickADate.pickADateDialog.addButton(top._res_29, null, "button-gray", null, "cancel"); // Annuler
    eFormularPickADate.pickADateDialog.addButton(top._res_28, (function (elt) { return function () { eFormularPickADate.validDate(elt) }; })(input), "button-green", null, "ok"); // Valider
};

eFormularPickADate.validDate = function (eltInput) {
    var objResult = eFormularPickADate.pickADateDialog.getIframe().nsPickADate.getReturnValue();
    eltInput.setAttribute("dbv", objResult.dbv);
    eltInput.value = objResult.disp;
    eFormularPickADate.pickADateDialog.hide();
};