
//NS des traitment de duplication
nsDuplicationTreatment = {

};



//Object wizard de duplication - objet unique dans le ns des dupli, initialisé à l'ouverture du wizard, cf eDupliWizardRenderer
nsDuplicationTreatment.eDuplicationWizard = function () {

    var that = this;


    that.CurrentStep = 1;
    that.TotalStep = 2;
    that.BtnNext = null;
    that.BtnPrev = null;
    that.BtnCancel = null;
    that.BtnValidate = null;


    //fenêtre modal associée
    that.ModalDialog = eTools.GetModal("DupliGlobalAffectWizard");

    that.WizardDocument = null;

    var myDoc;

    ///Type de rapport
    /// Utilisé dans eWizard.js via oCurrentWizard (cf la méthode init dans eWizard.js)
    that.GetType = function () {
        return "-1"; //Le type d'un wizard fait référence à un type de report. Dans le cas d'un wizard dupli cela n'a pas de sens, on passe donc -1 (NONE dans l'énum)


    };


    //gestion du cochage de la sélection des rubriques
    that.onCheckFieldDuplicate = function (sCheckId) {

        if (that.ModalDialog == null)
            return;


        var myChkHide = myDoc.getElementById(sCheckId);

        var myChk = myDoc.getElementById("chk_" + sCheckId);
        var myChkOrig = myChk.parentNode;

        var nIdx = getAttributeValue(myChkOrig, "ednidx");
        var nDescId = getAttributeValue(myChkOrig, "edndescid");

        if (myChk && myChkHide && myChkOrig) {

            if (getAttributeValue(myChk, "chk") == "1") {
                myChkHide.checked = true;
                that.loadField(myChkOrig);

            }
            else {


                var myTd = myDoc.getElementById("td_dupli_val_" + nIdx);

                if (myTd) {

                    myTd.innerHTML = "";

                    var myInp = myDoc.createElement("input");
                    myInp.id = "inptDup" + nDescId;
                    myInp.name = "inptDup" + nDescId;
                    myInp.disabled = true;
                    setAttributeValue(myInp, "ednup", "1");
                    setAttributeValue(myInp, "edndescid", nDescId);
                    setAttributeValue(myInp, "type", "text");

                    myTd.appendChild(myInp);
                    myChkHide.checked = false;

                }
            }

        }
    };

    ///Charge l'input du champ
    that.loadField = function (oCheck) {


        var nDescId = getAttributeValue(oCheck, "edndescid");
        var nTab = getNumber(getAttributeValue(document.getElementById("dupliTreatSelectFields"), "tab"));
        var nIdx = getAttributeValue(oCheck, "ednidx");
        var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);


        upd.ErrorCallBack = function () { };

        upd.addParam("action", "reloadvalues", "post");
        upd.addParam("maintab", nTab, "post");
        upd.addParam("descid", nDescId, "post");
        upd.addParam("operator", 0, "post");
        upd.addParam("filtertype", 0, "post");
        upd.addParam("tabindex", 0, "post");
        upd.addParam("lineindex", nIdx, "post");
        upd.addParam("fromtreat", "1", "post");


        var fct = function (oRes) {
            that.updateField(oRes, oCheck);
        }

        upd.send(fct);
    };

    //Met à jour le champ de saisie
    that.updateField = function (oRes, oCheck) {


        var nDescId = getAttributeValue(oCheck, "edndescid");
        var nTab = nDescId - nDescId % 100;
        var nIdx = getAttributeValue(oCheck, "ednidx");
        var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);
        var nFormat = getAttributeValue(oCheck, "edntype");

        var sDefaultValue = getAttributeValue(oCheck, "edndefaultvalue");
        var sDefaultValueDb = getAttributeValue(oCheck, "edndefaultvaluedb");


        var oInputOP = myDoc.createElement("input");
        oInputOP.id = "op_0_" + nIdx;
        oInputOP.value = 0;
        oInputOP["selectedIndex"] = 0;
        oInputOP["options"] = [{ "value": "0" }];
        oInputOP.style.display = "none";

        var inpt = myDoc.getElementById("inptDup" + nDescId);

        var t = inpt.parentNode;
        t.innerHTML = oRes;
        t.appendChild(oInputOP);

        var oInputVal = myDoc.getElementById("value_0_" + nIdx);
        if (oInputVal) {

            setAttributeValue(oInputVal, "ednvalue", sDefaultValueDb);
            oInputVal.value = sDefaultValue;

            if (nFormat == 3) {
                //gestion typ_bit
                var sel = myDoc.getElementById("selbitdupli_0_" + nIdx);
                if (sDefaultValue + "" == "0")
                    sel.options.selectedIndex = 0;
                else
                    sel.options.selectedIndex = 1;
            }

        }

    };

    that.addBkmToCloneLst = function (obj) {


        if (getAttributeValue(obj, "chk") != "1") {
            if (getAttributeValue(myDoc.getElementById("SelAll"), "chk") == "1") {
                chgChk(myDoc.getElementById("SelAll"));
            }

        }


        if (getAttributeValue(obj, "chk") == "1") {
            if (getAttributeValue(myDoc.getElementById("UnSelAll"), "chk") == "1") {
                chgChk(myDoc.getElementById("UnSelAll"));
            }

        }

    };


    //
    that.selectAll = function (obj) {


        if (getAttributeValue(obj, "chk") != "1")
            return;

        var bSel = false;

        var ochkOther;
        if (obj.id == "UnSelAll") {
            bSel = false;
            ochkOther = myDoc.getElementById("SelAll");
        }
        else if (obj.id == "SelAll") {
            bSel = true;
            ochkOther = myDoc.getElementById("UnSelAll");
        }

        if (getAttributeValue(ochkOther, "chk") == "1") {

            chgChk(ochkOther);
        }





        var oTbBkms = myDoc.getElementById("BkmsCloneTb");

        if (!oTbBkms)
            return;


        var lstBkmd = oTbBkms.querySelectorAll("a[chk]");


        for (var i = 0; i < lstBkmd.length; i++) {

            var oChk = lstBkmd[i];

            if ((bSel && getAttributeValue(oChk, 'chk') != "1") || (!bSel && getAttributeValue(oChk, 'chk') == "1"))
                chgChk(oChk);

            that.addBkmToCloneLst(oChk);
        }

    };

    //Valide la duplication
    that.onValidate = function () {


        //Champs obligatoire
        var o = that.controlObligat(function () { StepClick('1'); });
        if (!o.Success) {

            o.FctErr();

            return;
        }

        /*Récupération de la liste des valeurs*/
        //Récup des info des champs affichés actuellement sur la fiche en cours
        var SEPARATOR_LVL1 = "#|#";
        var SEPARATOR_LVL2 = "#$#";
        var SEPARATOR_LVL0 = "#=#";




        var oLstInp = myDoc.querySelectorAll("a[id^='chk_chkDup'][chk='1']");
        var oLstValue = new Array();

        for (var ii = 0; ii < oLstInp.length; ii++) {
            var parChk = oLstInp[ii].parentNode;

            var nIdxSel = getAttributeValue(parChk, "ednidx");
            var myValueInpt = myDoc.getElementById("value_0_" + nIdxSel);
            oLstValue.push(myValueInpt);
        }






        //objet de retour
        var oResult = {};


        oResult.FieldsList = oLstValue;

        var oTbBkms = myDoc.getElementById("BkmsCloneTb");
        var lstBkmd = oTbBkms.querySelectorAll("a[chk='1']");

        var sLstBkm = "";
        for (var t = 0; t < lstBkmd.length; t++) {

            if (sLstBkm.length > 0)
                sLstBkm += ";";

            sLstBkm += getAttributeValue(lstBkmd[t], "bkm");
        }

        oResult.BkmList = sLstBkm;



        return oResult;


    };


    //Controle des champs obligatoires
    that.controlObligat = function (fct) {

        var lstObligat = myDoc.querySelectorAll("div[ednobligat='1'] > a[chk='1']");


        var oRes = {};
        oRes.LstObligatEmpty = new Array();
        oRes.Success = true;
        oRes.FctErr = function () { };

        for (var ii = 0; ii < lstObligat.length; ii++) {

            var o = lstObligat[ii];
            var oCheck = o.parentNode;
            var nIdx = getAttributeValue(oCheck, "ednidx");
            var sLabel = getAttributeValue(oCheck, "ednlabel");
            var inpt = myDoc.getElementById("value_0_" + nIdx);


            if (inpt) {
                if (getAttributeValue(inpt, "ednvalue") == "") {
                    oRes.LstObligatEmpty.push(sLabel);
                    oRes.Success = false;
                }
            }

        }


        if (!oRes.Success) {
            var strObligatFields = "";
            for (var i = 0; i < oRes.LstObligatEmpty.length; i++) {
                strObligatFields += oRes.LstObligatEmpty[i] + '[[BR]]';
            }

            if (typeof (fct) != "function")
                fct = function () { };

            oRes.FctErr = function () { top.eAlert(0, top._res_372, top._res_1268, strObligatFields, null, null, fct) };
        }

        return oRes;
    }


    //valide le changement d'étape
    that.ControlStep = function (nStep) {

        if (nStep == 1) {

            //vérification des champs obligatoire
            var oRes = that.controlObligat();
            if (!oRes.Success)
                oRes.FctErr();

            return oRes.Success;



        } else
            return true;
    };

    //Change d'étape
    that.SwitchStep = function (nStep) {

        that.CurrentStep = nStep;

    };

    //Affiche ou masque les bouttons en fonctions de l'étape
    that.ManageButtonDisplay = function () {

        var strStep = "first";
        if (that.CurrentStep > 1 && that.CurrentStep < that.TotalStep)
            strStep = "middle";
        else if (that.CurrentStep >= that.TotalStep)
            strStep = "last";


        switch (strStep) {
            case "first":

                that.BtnCancel.style.display = "inline";
                that.BtnNext.style.display = "inline";
                that.BtnPrev.style.display = "none";
                that.BtnValidate.style.display = "none";

                break;
            case "middle":
                that.BtnCancel.style.display = "inline";
                that.BtnNext.style.display = "inline";
                that.BtnPrev.style.display = "none";
                that.BtnValidate.style.display = "none";
                break;
            case "last":
                that.BtnCancel.style.display = "inline";
                that.BtnNext.style.display = "none";
                that.BtnPrev.style.display = "inline";
                that.BtnValidate.style.display = "inline";
                break;
        }

    }



    that.updateBitDuplTreat = function (obj) {

        var idx = getAttributeValue(obj, "ednidx");
        var inpt = myDoc.querySelector("[id='value_0_" + idx + "']");
        var val = obj.options[obj.selectedIndex].value;

        setAttributeValue(inpt, "ednvalue", val);
        inpt.value = val;


    }

    that.updateMemoDuplTreat = function (obj) {

        var idx = getAttributeValue(obj, "ednidx");
        var inpt = myDoc.querySelector("[id='value_0_" + idx + "']");

        var nDescid = getAttributeValue(inpt, "edndescid");

        var val = obj.options[obj.selectedIndex].value;

        if (val + "" == 0) {
            inpt.value = "";
            setAttributeValue(inpt, "ednvalue", "");

        }
        else {


            var oCheck = myDoc.querySelector("div[edndescid='" + nDescid + "']");
            var sDefaultValue = getAttributeValue(oCheck, "edndefaultvalue");
            var sDefaultValueDb = getAttributeValue(oCheck, "edndefaultvaluedb");

            inpt.value = sDefaultValue;
            setAttributeValue(inpt, "ednvalue", sDefaultValueDb);
        }

    }


    //initialisation des champs à ne pas dupliquer par défaut
    that.InitNoClone = function () {

        var lstNoClone = myDoc.querySelectorAll("div[ednautoload='1'] > a[chk]");
        for (var ii = 0; ii < lstNoClone.length; ii++) {
            lstNoClone[ii].click();
        }
    }

    //Initialisation de l'object
    that.InitOk = (function () {



        // Récupération du doc de la modal
        if (that.ModalDialog == null) {
            return false;
        }


        that.WizardDocument = that.ModalDialog.getIframe().document;

        myDoc = that.WizardDocument;

        var buttonModalDiv;
        //Barre des boutons
        try {
            buttonModalDiv = that.ModalDialog.getIframe().parent.document.getElementById("ButtonModal" + that.ModalDialog.iframeId.replace("frm", ""));

            if (!buttonModalDiv)
                return false;
        }
        catch (e) {
            return false;
        }


        //bouton
        that.BtnCancel = buttonModalDiv.querySelector("div[id='cancel_btn']");
        that.BtnNext = buttonModalDiv.querySelector("div[id='next_btn']");
        that.BtnValidate = buttonModalDiv.querySelector("div[id='validate_btn']");
        that.BtnPrev = buttonModalDiv.querySelector("div[id='previous_btn']");
        if (!that.BtnCancel || !that.BtnNext || !that.BtnValidate || !that.BtnValidate)
            return false;


        return true;
    })();

    if (!that.InitOk)
        return null;

}


