/// <reference path="eMemoEditor.js" />

///enum MailingAction
var MailingAction =
{
    /// <summary>pas d'action (ne doit pas se produire)</summary>
    NONE: { KEY: 0, RES: top._res_6524 }, //"Invalide"
    /// <summary>Enregistrement de la campagne</summary>
    INSERT: { KEY: 1, RES: top._res_6381 },//"Enregistrement"
    /// <summary>Mise à jours de la campagne</summary>
    UPDATE: { KEY: 2, RES: top._res_961 },//"Mise à jour"
    /// <summary>Envoyer la campagne</summary>
    SEND: { KEY: 3, RES: top._res_6632 },// "Envoi"
    /// <summary>Annuler la campagne</summary>
    CANCEL: { KEY: 4, RES: top._res_6633 },//Annulation
    /// <summary>Tester l'envoi</summary>
    SEND_TEST: { KEY: 5, RES: top._res_6634 },//"Envoi TEST"
    /// <summary>Vérification des liens</summary>
    CHECK_LINKS: { KEY: 6, RES: top._res_6634 },//"Vérifications liens"

    /// <summary>Vérification des liens</summary>
    RESET_MAILTESTER: { KEY: 7, RES: top._res_307 }//"veuillez patienter..."
}



/// <summary>
/// Type d'envoi différé utilisé
/// </summary>
var MailingQueryMode =
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

/// <summary>Backlog #83 - Identifiant du modèle "Vierge" proposé dans la liste des modèles (remplace "Pas de modèle")</summary>
var EMAILTEMPLATE_BLANKTEMPLATEID = "12";


var paramWin = top.getParamWindow();
var objThm = JSON.parse(paramWin.GetParam("currenttheme").toString());

function eMailing(mailingId, CampaignTab, mailingType, tab, parentTab, parentFileId) {

    this._debug = false;

    //variables communes à toutes les étapes
    var me = this;

    this._oUpdater = null;
    this._url = "mgr/eMailingManager.ashx";

    this._existingCampaign = mailingId && mailingId > 0 ? true : false;
    this._mailingId = mailingId || 0;
    this._tab = tab; //table principale 
    this._nParentTabId = parentTab; // table parente
    this._nParentFileId = parentFileId;

    this._mailingType = mailingType;
    this._nCampaignTab = CampaignTab;
    this._action = MailingAction.NONE;

    //Catégorie est/n'est pas obligatoire 
    this._catIsOblicat = false;

    this._aMailingParams = new Array();
    this._iParamLength = 0;


    //affichage du champ copie invisible
    this._bDisplayMailCC = true;

    this._pjIds = "";

    //les champs destinataires :  
    this._recipientInput;
    this._recipientCountLabel;

    //la liste des champs de type mail à faire apparaitre dans la liste
    this._dicMailAdresses = new Array();

    this._oSubjectEditor = null;
    this._oMemoEditor = null;

    this._waitDialog = null;
    this._success = false;
    this._currentStep = 1;

    this._modalSchedule = null;
    //Si ces scores ne sont pas encore calculés, on les affiches différement
    this._mailTestScoreCalculated = false;
    this._campaignTypeScoreCalculated = false;
    this._TestType = 0;
    //pour gérer le score en cas d'erreur mailtester
    this._mailTesterExtextionActivated = false;
    this._isErrorOnMailTester = false;
    this._mailTesterReportId = false;

    this._scoring = {
        mailtester: 0,
        other: 0
    }

    this._MediaType = 0;//Type de média
    this._CampaignType = 0;//Type de campagne


    // mis a jour la liste des champs de type mail
    this.SetMailAdresses = function (dicMailAdresses) {
        me._dicMailAdresses = dicMailAdresses;
    }

    //Mis a jour par une fois le chargement de CKEditor ait terminé
    this.SetSubjectEditor = function (oFieldEditor) {
        if (oFieldEditor)
            this._oSubjectEditor = oFieldEditor;
    }

    //Mis a jour par une fois le chargement de CKEditor ait terminé
    this.SetMemoEditor = function (oMemoEditor) {
        if (oMemoEditor)
            this._oMemoEditor = oMemoEditor;
    }

    //Indique si aucun template n'est sélectionné au sein de la campagne
    this.NoTemplate = function () {
        //return (this._mailingType == 2);
        return false;
    };

    //Met à jour l'éditeur principal (le 1er ) avec le secondaire ("xxx_1") si ce dernier est l'éditeur en cours
    this.majMainEditor = function (callback) {
        setWait(true);
        //
        if (me._oMemoEditor != null && typeof ("".endsWith) == "function" && me._oMemoEditor.name.endsWith("_1")) {

            if (nsMain.getAllMemoEditorIDs().length > 1) {
                var mainEditor = nsMain.getMemoEditor(0);
                me.copieEditorContent(me._oMemoEditor, mainEditor, callback);
            }
        }
        setWait(false);
    }

    //recopie le contenu (html, css, ..) d'un editeur (oSrc) vers un autre (oDest) et appel un callback après
    this.copieEditorContent = function (oSrc, oDest, callback) {

        // eTools.consoleLogFctCall({ fullstack: true, asObj: false, comments: "" })

        top.setWait(true);

        function f() {

            //on ne switch que si les 
            if (oSrc.name != oDest.name) {


                var dataSource = oSrc.getData()
                //                console.log("GetData Source Length  " + dataSource.length)

                var dtStart = new Date()
                oDest.setData(dataSource);

                var dtEnd = new Date()

                //TempSetdata
                //console.log("SetData " + oSource.name + " ==> " + oDesination.name + " : " + (dtEnd.getTime() - dtStart.getTime()))


                //var dataDest = oDest.getData();
                //console.log("GetData Destination Length  " + dataDest.length)

                //
                oDest.setColor(oSrc.getColor()); // Backlog #619 - On transfère la couleur de fond d'un éditeur à l'autre (positionné sur le wrapper de grapesjs)

                var bgColor = oSrc.getColor()
                if (bgColor) {
                    var bodyCss = oSrc.getCss()
                    var result = eTools.setCssRuleFromString(bodyCss, "body", "background-color", bgColor, true);

                    if (result.hasChanged)
                        oSrc.setCss(result.value)
                }



                // cf #617, #619, #648 - Le CSS doit être récupéré de l'éditeur principal (grapesjs ou CKEditor si IE/Edge) comme pour le corps de mail, on ne met plus les css a jour ici
                oDest.injectCSS(oSrc.getCss()); // Backlog #617 - Transfert des CSS externes de chaque éditeur l'un vers l'autre

                //89 478
                if (oDest.htmlTemplateEditor)
                    oDest.injectCSS(" .Column:empty:before {	background-color: #ddd;	color: #000;	font-size: 16px;	font-weight: bold;	height: 100%;	display: flex;	align-items: center; text-align: center;	justify-content: center;	min-height: 50px;	padding: 0 10px;	opacity: 0.3;	border-radius: 3px;	overflow: hidden;	text-overflow: ellipsis;	content: '" + top._res_2991 + "'}", true, true);

                //Appel de la fct de callback
                if (typeof callback == "function") {
                    try {
                        callback()
                    }
                    catch (e) {
                    }
                }

                top.setWait(false);
            }
            else
                top.setWait(false);
        }

        //lancement via setTimout pour que le spinner d'attente se lance avant le getdata
        //f()
        setTimeout(f, 0);
    }

    //Switch entre 2 éditeurs
    this.switchEditor = function (nIndex) {
        if (me._oMemoEditor != null && typeof ("".endsWith) == "function") {
            if (nsMain.getAllMemoEditorIDs().length > 1) {
                setWait(true);

                nsMain.getAllMemoEditorIDs().forEach(function (memeEdit, idx) {
                    var myMemeoEdit = nsMain.getMemoEditor(memeEdit);
                    if (myMemeoEdit.name.endsWith("_" + nIndex)) {

                        //Si l'editeur demandé est différent de l'éditeur en cours
                        if (myMemeoEdit.name != currentMemoEditor.name) {

                            me.copieEditorContent(me._oMemoEditor, myMemeoEdit, function () {
                                myMemeoEdit.container.style.display = "";
                                me._oMemoEditor = myMemeoEdit;
                                currentMemoEditor = myMemeoEdit;
                            });

                        }

                    }
                    else {
                        myMemeoEdit.container.style.display = "none";
                    }
                });

                setWait(false);
            }
        }
    }

    //Function executé au chargement de l'assistant
    this.Init = function () {

        if (nsMain.getAllMemoEditorIDs().length > 1) {
            //Si plusieurs editeurs, a l'initialisation on affiche et active uniquement le premier

            currentMemoEditor = nsMain.getMemoEditor(0);


            nsMain.getAllMemoEditorIDs().forEach(function (memeEdit, idx) {

                //Ler 1er editeur sera initialisé de façon "standard"
                if (idx == 0)
                    return

                //Masque l'éditeur
                var myMemeoEdit = nsMain.getMemoEditor(memeEdit);
                myMemeoEdit.container.style.display = "none";

                //Initialise les param pour le 2é editeur
                myMemeoEdit._nParentTabId = me._nParentTabId;// table parente
                myMemeoEdit._nParentFileId = me._nParentFileId;// fileid table parente        
                myMemeoEdit._tab = me._tab;//table pricipale    


            })
        }

        if (typeof (currentMemoEditor) != "undefined" && currentMemoEditor != null) {
            me._oMemoEditor = currentMemoEditor;

            if (mailingType != TypeMailing.SMS_MAILING_FROM_BKM)
                //on insere les styles css une fois l'instance CKEditor soit complètement chargée
                me._oMemoEditor.htmlEditor.on("instanceReady", me.InsertCss);
            else
                me._oMemoEditor.forceCompactMode = true;
        }
        me._oMemoEditor._nParentTabId = this._nParentTabId;// table parente
        me._oMemoEditor._nParentFileId = this._nParentFileId;// fileid table parente        
        me._oMemoEditor._tab = this._tab;//table pricipale       

        ////recipientInputId viens de js gnéré par la classe eEditMailingRendrer 
        if (typeof recipientInputId != 'undefined')
            me._recipientInput = document.getElementById(recipientInputId);
        me._recipientCountLabel = document.getElementById(me._nCampaignTab + "_recipient_count");

        setAttributeValue(me._recipientInput, "disabled", "disabled");

        var oValueCat = document.querySelector('input[name="catYesNo"][checked]');
        var bCatObligat = (oValueCat && oValueCat.value == "rd_yes");


        if (bCatObligat || (me.GetParam("category") != "" && parseInt(me.GetParam("category")) > 0)) {
            me.DisplayCat(true);
            me._catIsOblicat = true;

            if (me.GetParam("category") == "") {
                var oCat = document.getElementById("value_106000_106008");
                if (oCat) {
                    setAttributeValue(oCat, "eAlert", "1");
                }
            }

        } else {
            me.DisplayCat(false);
            me._catIsOblicat = false;
        }


        var size = me.SizeOf(me._dicMailAdresses);

        //liste des destinataires en Copie invisible est gérée uniquement en mode création
        if (me._mailingId == 0) {

            //Initialisation...
            for (var value in me._dicMailAdresses) {
                if (size == 1)
                    me.SetParam("mailFieldDescId", value);
            }
            //s'il y q'une adresse mail on a pas le select de copie invisible
            //SHA : correction bug #71 207
            if (size == 0 || size == 1) {
                //display none pour le tr contenant le champ mailCC
                var id = me._nCampaignTab + "_tr_mail_cc";
                Display(id, false);
                me._bDisplayMailCC = false;
            }
            else {
                // si on a plusieurs adresses on ajoute une option vide
                me._dicMailAdresses["0"] = "";
            }

        }
        //Dans l'assistant d'emailing,la table active est Campaign 
        nGlobalActiveTab = me._nCampaignTab;

        _defaultTemplate = document.getElementById("HidDefaultTplID").value;
        oTemplate.Init();

        //mis a jour de l'etape 1
        me.SwitchStep(me._currentStep);
        me.Update(me._currentStep);
    }



    ///injection css dans le champ memo
    this.InsertCss = function () {
        me._oMemoEditor.injectCSS(me.GetParam("bodyCss"));
        me._oMemoEditor.htmlEditor.removeListener("instanceReady", me.InsertCss);
    }

    /// met à jour l'etape numéro "step"
    this.Update = function (step) {

        //La choix par defaut d'adresse mail utilisée
        if (me.GetStepName(step) == "infosCampaign") {

            if (me._mailingType != TypeMailing.MAILING_FROM_CAMPAIGN)
                me._recipientInput.value = "[" + me._dicMailAdresses[me.GetParam("mailFieldDescId")] + "]";

            setAttributeValue(document.getElementById("ulAdrMail_opt2"), "sid", me.GetParam("mailFieldDescId"));
            setAttributeValue(document.getElementById("ulAdrMail_opt2"), "slbl", me._dicMailAdresses[me.GetParam("mailFieldDescId")]);

            //me.SetParam('templateType', oTemplate.GetType());
            //me.SetParam('templateId', oTemplate.GetTplId());
        }

        //On affiche le nombre de destinataires
        //if (me.GetParam("nMailCount") + "" != "0")
        //    me._recipientCountLabel.innerHTML = me.GetParam("nMailCount");


        if (me._mailingType == TypeMailing.MAILING_FROM_CAMPAIGN)
            return;

        me.ChangeCc();
    }

    this.ChangeCc = function () {
        //Mis a jour le champs mailCC 
        var select = document.getElementById(me._nCampaignTab + "_optmail_cc");
        var selectedValue = me.GetParam('ccrecipient');
        //on enleve toutes les options
        if (select != null) {
            while (select.options.length > 0)
                select.removeChild(select.firstChild);
        }


        var fieldId = parseInt(me.GetParam("mailFieldDescId"));
        var nTab = fieldId - fieldId % 100;

        //on insere les options nécéssaires depuis le dictionnaire
        for (var value in me._dicMailAdresses) {

            if (value != me.GetParam("mailFieldDescId")) {

                var opt = document.createElement("option");

                opt.setAttribute("value", value);

                if (value == 0)//par defaut pas de copie
                {
                    opt.innerHTML = "";
                    //opt.setAttribute("selected", true);
                    //Copie invisible                
                    //  me.SetParam("ccrecipient", "0");
                }
                else {
                    opt.innerHTML = top._res_385 + " " + me._dicMailAdresses[value];
                }
                //element selectionné
                //NHA : Correction du Bug 73201              
                if (selectedValue && value == selectedValue || !selectedValue && value == 0) {
                    opt.setAttribute("selected", true);
                }
                //on affiche que les champs adresse mail de la meme table 
                if (nTab == (value - value % 100) || value == 0) {
                    if (select != null)
                        select.insertBefore(opt, select.firstChild);
                }
            }
        }

        //s'il y a que l'option vide on l affiche pas
        if (select != null) {
            var displaySelect = select.options.length != 1;
            var id = me._nCampaignTab + "_tr_mail_cc";
            Display(id, displaySelect);
            me._bDisplayMailCC = displaySelect;
        }

    }

    ///summary
    /////Ajout des adresses mail
    ///<param name="descId">descId du champs</param>
    ///summary
    this.SetMailField = function (descId) {
        //on reinitialize le champs ccrecipient
        me.SetParam('ccrecipient', '0');
        //on reinseigne de le descid du champs mail dans la campagne
        me.SetParam('mailFieldDescId', descId);
    }

    /// délégation de chargement de modèle à l'objet otemplate dans eMailingWizard.js    
    this.LoadTemplate = function (oParentDialog) {
        if (me.BodyAlreadyLoaded())
            return;

        // Backlog #83 - Choisir "Pas de modèle" en bouton radio provoque désormais le chargement du modèle "Courriel simple"
        if (this.NoTemplate() || oTemplate.NoTemplate()) {
            oTemplate.SetType(1);
            oTemplate.id = EMAILTEMPLATE_BLANKTEMPLATEID;
        }

        oTemplate._parentDialog = oParentDialog; // permet la fermeture de la fenêtre parente (cas des mails unitaires)

        //On délègue à oTemplate le chargement du modèle 
        if (me.BodyIsEmpty()) {
            oTemplate.Load();
        }
        // Etes-vous sûr de vouloir charger le modèle ? Les données existantes seront écrasées
        else {
            eConfirm(1, top._res_748, top._res_6666, top._res_6667, 450, 180, oTemplate.Load);
        }
    }

    ///vérifier si l'objet ou le corps du mail à été déjà chargé
    this.BodyAlreadyLoaded = function () {

        var type = me.GetParam("templateType");
        var tplId = me.GetParam("templateId");

        return type == oTemplate.GetType() && tplId == oTemplate.GetTplId();
    }

    ///vérifier si l'objet ou le corps du mail contient déja des données 
    this.BodyIsEmpty = function () {
        // Le corps de mail contient déja des donnée
        var data = me._oMemoEditor.getData() + "";

        //TODO vérifier l'objet
        data = data.replace(/\s/g, "");

        return data.length == 0;
    }

    //Type de l'emailing : depuis evt, pp, pm, invit, cible etendu
    this.GetType = function () {
        return me._mailingType;
    }

    //En fonction de numero de l'etape on retourne le nom associé
    this.GetStepName = function (step) {

        var stepDiv = document.getElementById("editor_" + step);

        return getAttributeValue(stepDiv, "stepName")
    }
    ///
    this.GetRecipientCount = function () {

        var tz = document.getElementById("delayedMail_TimeZone");
        if (tz) {
            this.SetParam('sendingTimeZone', tz.value);
        }

        me._url = "mgr/eMailingManager.ashx";
        me.InitUpdater(1);
    }

    /*Private*/
    this.InitUpdater = function () {
        me._oUpdater = new eUpdater(me._url, 0);
        me._oUpdater.addParam("fileid", me._mailingId, "post");
        me._oUpdater.addParam("tab", me._tab, "post"); //callable tab
        me._oUpdater.addParam("parentfileid", me._nParentFileId, "post");
        me._oUpdater.addParam("parenttab", me._nParentTabId, "post");
        me._oUpdater.addParam("typeMailing", me._mailingType, "post");

        me._oUpdater.ErrorCallBack = me.ManageReturnedError;
    }

    //Passage des parametre a l updater
    this.AddParams = function () {

        //ajout des styles css
        // Backlog #617, #619, #648 - Le CSS doit être récupéré de l'éditeur principal (grapesjs ou CKEditor si IE/Edge) comme pour le corps de mail
        // Celui-ci ayant été préalablement mis à jour à partir de l'éditeur secondaire (CKEditor) via majMainEditor()
        // On récupère donc la référence au mainEditor de la même façon que le fait majMainEditor()
        var mainEditor = me._oMemoEditor;
        if (me._oMemoEditor != null && typeof ("".endsWith) == "function" && me._oMemoEditor.name.endsWith("_1")) {
            if (nsMain.getAllMemoEditorIDs().length > 1) {
                mainEditor = nsMain.getMemoEditor(0);
            }
        }
        //SHA : tâche #1 939
        //var lblsubject = document.querySelectorAll("td[lib='Objet']");
        //var _subject = document.querySelectorAll("input[ename=" + lblsubject[0].id + "]");
        //var subject = document.getElementById(_subject[0].id).value;
        var subject = document.getElementById("COL_106000_106005_0_0_0");
        if (subject != null && typeof (subject) != "undefined") {
            if (subject.tagName == 'DIV')
                subject = subject.innerHTML;
            else
                subject = subject.value;
            me.SetParam("subject", subject, "post");
        }

        //var lblPreheader = document.querySelectorAll("td[lib='Texte d\\'aperçu']");
        //var _preheader = document.querySelectorAll("input[ename=" + lblPreheader[0].id + "]");
        //var preheader = document.getElementById(_preheader[0].id).value;
        var preheader = document.getElementById("COL_106000_106047_0_0_0");
        if (preheader != null && typeof (preheader) != "undefined") {
            preheader = preheader.value;
            me.SetParam("preheader", preheader, "post");
        }

        me.SetParam("bodyCss", mainEditor.getCss(), "post");

        //action souhaitée
        me.SetParam("operation", me._action.KEY, "post");

        //Nom apparent
        me.SetParam("displayName", document.getElementById("mailing_DN").value);

        if (me._mailingType != TypeMailing.MAILING_FOR_MARKETING_AUTOMATION)
            me.SetParam("RequestMode", getCheckedRadio("RequestMode").value);



        //Type de média
        var selectMediaType = document.getElementById('campaginSelectMediaType');
        if (selectMediaType != null) {
            me.SetParam("mediaType", selectMediaType.options[selectMediaType.selectedIndex].value);
        }

        //Type de média
        var selectCampaignType = document.getElementById('selectCampaignType');
        if (selectCampaignType != null && selectCampaignType.selectedIndex > 0) {
            me.SetParam("category", selectCampaignType.options[selectCampaignType.selectedIndex].value);
        }

        //Optin
        var iptCampaignSwOpti = document.getElementById('swcampaignSwOptin');
        if (iptCampaignSwOpti != null) {
            me.SetParam("OptInEnabled", iptCampaignSwOpti.checked ? "1" : "0");
        }

        //noOpt
        var iptCampaignSwNoopt = document.getElementById('swcampaignSwNoopt');
        if (iptCampaignSwNoopt != null) {
            me.SetParam("NoConsentEnabled", iptCampaignSwNoopt.checked ? "1" : "0");
        }

        //OptOut
        var iptCampaignSwOptout = document.getElementById('swcampaignSwOptout');
        if (iptCampaignSwOptout != null) {
            me.SetParam("OptOutEnabled", iptCampaignSwOptout.checked ? "1" : "0");
        }

        //switch Valid Adress
        var iptqualityAdressEmailSwValide = document.getElementById('swqualityAdressEmailSwValide');
        if (iptqualityAdressEmailSwValide != null) {
            me.SetParam("AdressEmailSwValideEnabled", iptqualityAdressEmailSwValide.checked ? "1" : "0");
        }


        //switch Not verified Adress
        var iptqualityAdressEmailSwNotVerified = document.getElementById('swqualityAdressEmailSwNotVerified');
        if (iptqualityAdressEmailSwNotVerified != null) {
            me.SetParam("AdressEmailSwNotVerifiedEnabled", iptqualityAdressEmailSwNotVerified.checked ? "1" : "0");
        }

        //switch Invalid Adress
        var iptqualityAdressEmailSwInvalide = document.getElementById('swqualityAdressEmailSwInvalide');
        if (iptqualityAdressEmailSwInvalide != null) {
            me.SetParam("AdressEmailSwInvalideEnabled", iptqualityAdressEmailSwInvalide.checked ? "1" : "0");
        }

        //switch Remove Doublon
        var iptRemoveDoublon = document.getElementById('swcampaignSwDedoublonnage');
        if (iptRemoveDoublon != null) {
            me.SetParam("removeDoubles", iptRemoveDoublon.checked ? "1" : "0");
        }

        if (me.GetParam('eventStepDescId') == '0') {
            var inputEventStepDescId = document.getElementById('eventStepDescId');
            if (inputEventStepDescId != null)
                me.SetParam('eventStepDescId', getAttributeValue(inputEventStepDescId, 'value'));
        }


        me.SetParam("scoring", JSON.stringify(me._scoring));


        //paramètres généraux
        for (var paramkey in me._aMailingParams)
            me._oUpdater.addParam(paramkey, me.GetParam(paramkey), "post");

        //id des pj à mettre à jour
        me._oUpdater.addParam("pjids", me._pjIds, "post");



        //  scoring
        //me._oUpdater.addParam("scoring", JSON.stringify(me._scoring), "post");

        /*********************************************************************************
         * A la création de  la campaigne, les elements html de l'etape d'edition de mail de l'assistant, ont des
         * id de type : 106000_106014_0_0_0 (0 est l'id de la fiche campaigne). 
         * Si on enregistre la campagne sans fermer l'assistant, on aura un id différent de zéro, bien que les inputs htmls
         * ont toujours les mêmes id (106000_106014_0_0_0 ...etc)
         * Dans ce cas, il ne faut pas passer l'id de la campaigne en paramètre à la fonction getFieldsInfos()
         * Le seul cas ou il faudrait passer l'id , c'est quand on modifie la campaigne existante.
         **********************************************************************************/
        if ((me._mailingType == TypeMailing.MAILING_FROM_CAMPAIGN || (me._mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION && this._existingCampaign)) && me._mailingId > 0)
            var fields = getFieldsInfos(me._nCampaignTab, me._mailingId);
        else
            var fields = getFieldsInfos(me._nCampaignTab);

        //paramètres spécifiques à l'edition du mail
        for (var fldKey in fields) {
            if (typeof (fields[fldKey]) != 'function') {
                me._oUpdater.addParam('field_' + fields[fldKey].descId, fields[fldKey].GetSerialize(), "post");
            }
        }
    }

    //Etape en lecture seule une fois l'enregistrement est fait
    this.SetReadOnlyInputs = function (stepName) {
        if (me._mailingId <= 0)
            return;

        if (stepName == "recipient") {
            var divStep = document.getElementById("editor_" + 1);

            //tous les elms input en lecture seule
            var inputs = divStep.getElementsByTagName("input");
            for (var i = 0; i < inputs.length; i++) {
                setAttributeValue(inputs[i], "disabled", "disabled");
                setAttributeValue(inputs[i], "dis", "1"); //type eudo
            }

            //tous les elms input en lecture seule
            var hrefs = divStep.getElementsByTagName("a");
            for (var i = 0; i < hrefs.length; i++) {
                setAttributeValue(hrefs[i], "disabled", "disabled");
                setAttributeValue(hrefs[i], "dis", "1");//type eudo
            }
        }

    }

    //Affiche/cache l'element d'in elementId
    this.DisplayCat = function (bDisplay) {
        //dans wizard.js
        var elmCat = document.getElementById("catUnsub");
        var elmUnsub = document.getElementById("filtreUnsub");

        if (bDisplay) {
            setAttributeValue(elmCat, "style", "display:block;");
            setAttributeValue(elmUnsub, "style", "display:block;");
        } else {
            setAttributeValue(elmCat, "style", "display:none;");
            setAttributeValue(elmUnsub, "style", "display:none;");
        }

        me._catIsOblicat = bDisplay;
    }

    /*****************************************************************
     *                                             
     *   Actions/vérification à effectuer a chaque changement d'etapes
     *
     *****************************************************************/

    //Vérification des champs obligatoires pour chaque etape
    this.ControlStep = function (step) {

        if (me._debug)
            return true;

        switch (me.GetStepName(step)) {
            case "recipient":
                //rien a vérifier

                return true;
            case "template":
                if (oTemplate.GetType() != 2 && oTemplate.GetTplId() == 0) {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_6377, top._res_6617);
                    return false;
                }



                return true;
            case "mailck":
            case "mail":

                //saisie obligatoire pour les rubriques obligatoires
                //Les champs généré par eRendrer
                var aFields = getFieldsInfos(me._nCampaignTab);

                //Vérification des champs obligatoire dans l'etape 4
                var obligatFields = "";

                for (var key in aFields) {
                    //rubrique obligatoires
                    if (aFields[key].obligat == true && aFields[key].newValue == "") {
                        var label = document.getElementById("COL_" + me._nCampaignTab + "_" + aFields[key].descId);
                        if (label != null) {
                            obligatFields += (label.innerText != null ? label.innerText : label.textContent).replace("*", "") + "<br />";
                        }
                    }

                    var infoElm = document.getElementById("COL_" + me._nCampaignTab + "_" + aFields[key].descId);
                    var fmt = getAttributeValue(infoElm, "fmt") + ""; // aFields[key].format n'est jamais rempli coté serveur ???
                    var newValue = aFields[key].newValue + "";

                    //Inutile de vérifier si la valeur ou le format ne sont pas renseignés,   
                    if (newValue.length == 0 || fmt.length == 0)
                        continue;

                    if (fmt == eValidator.format.EMAIL) {
                        var parts = newValue.split('<');
                        if (parts.length >= 2) {
                            newValue = parts[1].slice(0, parts[1].length - 1);
                        }
                    }

                    var isValid = eValidator.isValid({ value: newValue, format: fmt });
                    if (!isValid) {
                        //6275, "Format incorrect"
                        var _title = top._res_6275;
                        //2021, "La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
                        _errorMessage = top._res_2021.replace("<VALUE>", aFields[key].newValue).replace("<FIELD>", infoElm.innerHTML.replace("*", ""));

                        eAlert(MsgType.MSG_CRITICAL.toString(), top._res_6275, _errorMessage);

                        return false;
                    }
                }
                if (obligatFields.length > 0) {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, obligatFields);
                    obligatFields = "";
                    return false;
                }

                return true;

            case "send":
                //Vérification des champs obligatoires dans l'étape 4
                var obligatFields = "";
                if (me._catIsOblicat && oMailing.GetParam("category") == "") {
                    obligatFields += top._res_6479.replace(":", "") + "<br />";

                    var oCat = document.getElementById("value_106000_106008");
                    if (oCat) {
                        setAttributeValue(oCat, "eAlert", "1");
                    }
                }

                if (oMailing.GetParam("mailTabDescId") == "0")
                    obligatFields += top._res_6408.replace(":", "") + "<br />";

                if (oMailing.GetParam("trackLnkPurgedDate") == "")
                    obligatFields += top._res_6483.replace(":", "") + "<br />";

                if (oMailing.GetParam("trackLnkLifeTime") == "")
                    obligatFields += top._res_6482.replace(":", "") + "<br />";

                if ((oMailing.GetParam("immediateSending") == "0" && oMailing.GetParam("recurrentSending") == "0") && oMailing.GetParam("sendingDate") == "")
                    obligatFields += top._res_6422.replace(":", "") + "<br />";

                if ((me.GetParam("immediateSending") == "0" && me.GetParam("recurrentSending") == "1") && me.sSchedule == "")
                    obligatFields += top._res_1991.replace(":", "") + "<br />";

                if (obligatFields != "") {
                    eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, obligatFields);
                    obligatFields = "";
                    return false;
                }
                if (!me.validDates())
                    return false;

                if (!me.validRequestMode())
                    return false;

                return true;
            case "infosCampaign":
                me.ChangeCc();
                me.FilterCampagneType();
                return true;
            case "controlBeforeSend":
                return true;
                break;
            default:
                break;
        }
    }

    this.FilterCampagneType = function () {
        FilterTargetCampaign();
    }

    ///vérifie les dates d emailing
    this.validDates = function () {

        //Vérification des formats de donées
        if (!eValidator.isDate(me.GetParam("trackLnkPurgedDate"))) {
            //6275, "Format incorrect"
            var elmInfo = document.getElementById("MailingLifeTrack_Date");
            var _title = top._res_6275;
            //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
            var errorMessage = top._res_2021.replace("<VALUE>", elmInfo.value).replace("<FIELD>", top._res_6483);

            eAlert(MsgType.MSG_CRITICAL.toString(), top._res_6275, errorMessage, top._res_959);
            return false;
        }

        if (!eValidator.isDate(me.GetParam("trackLnkLifeTime"))) {
            //6275, "Format incorrect"
            var elmInfo = document.getElementById("trackLnkLifeTime");
            var _title = top._res_6275;
            //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
            var errorMessage = top._res_2021.replace("<VALUE>", elmInfo.value).replace("<FIELD>", top._res_6482);

            eAlert(MsgType.MSG_CRITICAL.toString(), top._res_6275, _errorMessage, top._res_959);
            return false;
        }

        if ((oMailing.GetParam("immediateSending") == "0" && oMailing.GetParam("recurrentSending") == "0") && oMailing.GetParam("sendingDate") == "") {
            if (!eValidator.isDate(me.GetParam("sendingDate"))) {
                //6275, "Format incorrect"
                var elmInfo = document.getElementById("sendingDate");
                var _title = top._res_6275;
                //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
                var errorMessage = top._res_2021.replace("<VALUE>", elmInfo.value).replace("<FIELD>", top._res_6422);

                eAlert(MsgType.MSG_CRITICAL.toString(), top._res_6275, _errorMessage, top._res_959);
                return false;
            }
        }

        return true;
    }

    this.validRequestMode = function () {
        if ((me.GetParam("immediateSending") == "0" && me.GetParam("recurrentSending") == "0") && (me.GetParam("RequestMode") != MailingQueryMode.NORMAL && me.GetParam("RequestMode") != MailingQueryMode.QUERY_RUN_AGAIN)) {
            //2041,"Destinataires de l'envoi différé"
            eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_6564, top._res_2041);
            return false;
        }

        if (me.GetParam("immediateSending") == "0" && me.GetParam("recurrentSending") == "1") {
            var obligatFields = "";

            //2040,"Planification"
            if (getNumber(me.GetParam("scheduleId")) <= 0)
                obligatFields += top._res_2040 + "<br />";

            //2042,"Destinataires de l'envoi récurrent"
            if (me.GetParam("RequestMode") != MailingQueryMode.RECURRENT_ALL && me.GetParam("RequestMode") != MailingQueryMode.RECURRENT_FILTER)
                obligatFields += top._res_2042 + "<br />";

            //2043,"Filtre destinataires"
            if (me.GetParam("RequestMode") == MailingQueryMode.RECURRENT_FILTER && getNumber(me.GetParam("recipientsFilterId")) <= 0)
                obligatFields += top._res_2043 + "<br />";

            if (obligatFields != "") {
                eAlert(MsgType.MSG_CRITICAL.toString(), top._res_372, top._res_2158, "");
                return false;
            }
        }

        return true;
    }


    //Actions lancées sur le changement d'étape
    this.SwitchStep = function (step, bFromStepClick) {

        var oldStep = me._currentStep;
        me._currentStep = step;
        switch (me.GetStepName(step)) {
            case "recipient":
                //Une fois enregistré et l'assistant est tjrs ouvert, la modification des destinataires n'est plus possible
                me.SetReadOnlyInputs("recipient");
                break;
            case "template":
                //si y a pas de modèle utilisateur alors on affiche les modèles persos
                var hf = document.getElementById("NbUserTpl_107000");
                var nbTotal = parseInt(getAttributeValue(hf, "value"));
                if (nbTotal <= 0) {
                    document.getElementById("rbCustomTemplates").click();
                }

                // Template mail par défaut
                if (_defaultTemplate && _defaultTemplate != "0" && _defaultTemplate != "") {

                    // Ligne en couleur
                    var trMailTemplate = document.querySelector("tr[mtid='" + _defaultTemplate + "']");
                    eTools.SetClassName(trMailTemplate, "defaultTemplate");


                }

                break;
            case "mailck":
            case "mail":

                me.LoadTemplate();
                me.Update(step);
                break;
            case "infosCampaign":
                me.Update(step);
                break;
            case "controlBeforeSend":
                break;
            case "send":
                break;

        }
    }

    this.ControlDefaultTemplate = function () {
        if (_defaultTemplate && _defaultTemplate != "0" && _defaultTemplate != "" && this.GetStepName(this._currentStep) != "mail"
            && this.GetStepName(this._currentStep) != "mailck"
        ) {
            //if (oldStep <= me._currentStep && !bFromStepClick) {
            oTemplate.setSelectedUserTemplate(_defaultTemplate);

            if (typeof (onApplySelectedMailTemplate) == "function")
                onApplySelectedMailTemplate();
        }
    }

    /******************************************
     *                                             
     *   Mise à jour des paramètres de la campagne
     *
     *******************************************/
    ///summary
    ///Vérifie dans le dictionnaire des paramètres si la clé passée en paramètre existe
    ///<param name="key">clé de paramètre de mailing</param>
    ///summary
    this.KeyExists = function (key) {
        return (key in this._aMailingParams);
    }

    ///Summary
    /// Retourne la valeur associée à la clé
    ///<param name="key">clé de paramètre de mailing</param>
    ///summary
    this.GetParam = function (key) {
        if (key == null)
            return null;
        if (this.KeyExists(key))
            return this._aMailingParams[key];
        else
            return null;
    }

    ///Summary
    /// Retourne la liste des pjId 
    ///summary
    this.GetPjIds = function () {
        return me._pjIds;
    }

    ///summary
    ///Charge le dictionnaire de paramètres internes à partir du paramètre transmit
    ///<param name="aParams">Dictionnaire de paramètres de mailing</param>
    ///summary
    this.LoadParam = function (aParams) {
        this._aMailingParams = aParams;
        _iParamLength = this.ParamLength();
    }

    ///summary
    ///Retourne la taille du dictionnaire interne de paramètres
    ///summary
    this.ParamLength = function () {
        var length = 0, key;
        for (key in this._aMailingParams) {
            if (this._aMailingParams.hasOwnProperty(key)) length++;
        }
        return length;
    };

    ///summary
    ///Affecte un paramètre simple(valeur unique associée à la clé) au mailing
    ///<param name="key">clé de paramètre de mailing</param>
    ///<param name="val">valeur</param>
    ///summary
    this.SetParam = function (key, val) {
        if (this.KeyExists(key))
            this._aMailingParams[key] = val;

    }

    ///summary
    ///Compte le nombre de 'cle:val' contenant dans l'objet
    ///summary
    this.SizeOf = function (obj) {
        if (obj == null)
            return 0;

        var cpt = 0;
        for (var key in obj)
            cpt++;

        return cpt;
    }

    this.SetPjIds = function (strPjIds) {
        me._pjIds = strPjIds;
    }

    /******************************************
     *                                             
     *   Intéraction avec le serveur
     *
     *******************************************/
    ///summary
    ///Sauvegarde la campaigne mail
    ///summary
    this.SaveCampaign = function () {
        //Control des valeurs avant la sauvegarde
        if (!me.ControlStep(me._currentStep))
            return;

        var tz = document.getElementById("delayedMail_TimeZone");
        if (tz) {
            this.SetParam('sendingTimeZone', tz.value);
        }

        me._url = "mgr/eMailingManager.ashx";
        me.InitUpdater();

        //Operation d insertion ou de mise a jour
        if (me._mailingId > 0)
            me._action = MailingAction.UPDATE;
        else
            me._action = MailingAction.INSERT;

        me.AddParams();

        me.Send();
    };

    ///Sauvegarde l'objet et le corps du mail  comme modele
    // paramètre : modal du modèle de mail
    this.SaveAsTemplate = function (oMod) {
        beforeSaveTemplate(oMod);
    };

    ///summary
    ///Lance l'envoi de la campaigne 
    ///summary
    this.SendCampaign = function (bSend) {

        if (!bSend)
            return;

        var tz = document.getElementById("delayedMail_TimeZone");
        if (tz) {
            this.SetParam('sendingTimeZone', tz.value);
        }

        me._url = "mgr/eMailingManager.ashx";
        me.InitUpdater();
        if (me._mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION) {
            me._action = MailingAction.CHECK_LINKS;
            me._action = MailingAction.SEND;
        }
        else {
            me._action = MailingAction.CHECK_LINKS;
        }
        me.AddParams();
        me._oUpdater.send(me.Feedback); // la demande de confirmation se fait dans la fonction Feedback
    };

    ///summary
    ///Envoi un mail de test
    ///nType: 0 / Undefined = classique
    ///       1  = bon a tirer
    ///summary
    this.SendTestMail = function (nType) {

        nType = nType || 0;

        me._TestType = nType;
        var ctx = this;

        var fctSend = function () {
            var tz = document.getElementById("delayedMail_TimeZone");
            if (tz) {
                ctx.SetParam('sendingTimeZone', tz.value);
            }

            //Adding sendingType for mailtester
            me.SetParam('MailTestType', nType);

            me._url = "mgr/eMailingManager.ashx";
            me.InitUpdater();
            me._action = MailingAction.SEND_TEST;

            getMailRecipientsBeforeSending(top._res_6163, 0, 0, null, true, me);
        }

        //On met à jour l'éditeur principal
        if (oCurrentWizard && oCurrentWizard.GetStepName(oCurrentWizard._currentStep) == "mailck") {
            //le corps du mail étant pris de l'éditeur principal, on met à jour celui-ci
            me.majMainEditor(fctSend);
        }
        else {

            // Gestion couleur de fond
            var oMem = me._oMemoEditor;
            var bgColor = oMem.getColor()
            if (bgColor) {
                var bodyCss = oMem.getCss()
                var result = eTools.setCssRuleFromString(bodyCss, "body", "background-color", bgColor, true);

                if (result.hasChanged)
                    oMem.setCss(result.value)
            }

            fctSend()
        }

    };

    /*Private*/
    this.Send = function () {

        var title = top._res_307;
        var message = top._res_6636.replace("<ACTION>", me._action.RES);

        me._oWaitDialog = showWaitDialog(title, message);

        me._oUpdater.send(me.Feedback);
    };

    ///<summary>
    /// feedback 
    ///<summary>
    this.Feedback = function (oDoc) {
        var failure = getXmlTextNode(oDoc.getElementsByTagName("success")[0]) != "1";
        var message = getXmlTextNode(oDoc.getElementsByTagName("message")[0]);
        var userAdress = getXmlTextNode(oDoc.getElementsByTagName("useraddress")[0]);
        var userMainEmail = getXmlTextNode(oDoc.getElementsByTagName("usermainemail")[0]);
        var id = getNumber(getXmlTextNode(oDoc.getElementsByTagName("id")[0]));
        var eventStepFileId = getNumber(getXmlTextNode(oDoc.getElementsByTagName("eventStepFileId")[0]));
        var mailTesterExtextionActivated = getXmlTextNode(oDoc.getElementsByTagName("mailTesterExtextionActivated")[0]) == "1";

        var msgType;
        var detail = "";
        if (failure) {
            setWait(true);
            msgType = MsgType.MSG_CRITICAL.toString();
            var operation = getXmlTextNode(oDoc.getElementsByTagName("operation")[0]);
            if (me._action == MailingAction.UPDATE) {
                message = top._res_1760;
                msgType = MsgType.MSG_CRITICAL.toString();
            }

            detail = getXmlTextNode(oDoc.getElementsByTagName("detail")[0]);

            if (me._action == MailingAction.CHECK_LINKS) {
                var untrackedLinks = getNumber(getXmlTextNode(oDoc.getElementsByTagName("untrackedlinks")[0]));
                detail = detail.replace("<COUNT>", untrackedLinks);
                msgType = MsgType.MSG_EXCLAM.toString();
            }

        }
        else {

            msgType = MsgType.MSG_INFOS.toString();
            if (me._action == MailingAction.SEND) {

                if (me._mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION) {
                    var title = getXmlTextNode(oDoc.getElementsByTagName("title")[0]);
                    var description = getXmlTextNode(oDoc.getElementsByTagName("description")[0]);
                    if (me._oWaitDialog != null)
                        me._oWaitDialog.hide();
                    if (top.updateCampaignInfo && typeof (top.updateCampaignInfo) == 'function')
                        top.updateCampaignInfo(id, title, description, 9);
                    me._forceClose = true;
                    me.Close();
                    return;
                }

                if (me.GetParam("immediateSending") == "0" && me.GetParam("recurrentSending") == "1") {
                    message = top._res_306;
                    detail = top._res_6263;
                    msgType = MsgType.MSG_INFOS.toString();
                } else {
                    message = top._res_306;
                    detail = top._res_6128 + "<br />" + top._res_6077.replace("<USER_ADDRESS>", userMainEmail);
                    msgType = MsgType.MSG_INFOS.toString();
                }
            }

            //On passe les infos au workflow après une sauvegarde
            if (me._action == MailingAction.INSERT) {
                if (me._mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION && id > 0) {
                    var title = getXmlTextNode(oDoc.getElementsByTagName("title")[0]);
                    var description = getXmlTextNode(oDoc.getElementsByTagName("description")[0]);
                    if (me._oWaitDialog != null)
                        me._oWaitDialog.hide();
                    if (top.updateCampaignInfo && typeof (top.updateCampaignInfo) == 'function')
                        top.updateCampaignInfo(id, title, description, 8);
                }
            }

            if (me._action == MailingAction.UPDATE) {
                message = top._res_1761;
                detail = "";
                msgType = MsgType.MSG_SUCCESS.toString();
                var title = getXmlTextNode(oDoc.getElementsByTagName("title")[0]);
                var description = getXmlTextNode(oDoc.getElementsByTagName("description")[0]);
                if (top.updateCampaignInfo && typeof (top.updateCampaignInfo) == 'function')
                    top.updateCampaignInfo(id, title, description, 8);
            }
            if (me._action == MailingAction.INSERT) {
                message = top._res_1711;
                detail = "";
                msgType = MsgType.MSG_SUCCESS.toString();
                //les destinataires sont enregistrés, on ne peut plus modifier l'etape de choix de destinataire
                me.SetReadOnlyInputs("recipient");
            }
        }

        me._success = !failure;

        if (me._action == MailingAction.CHECK_LINKS) {
            // Affichage de la confirmation d'envoi de la campagne
            me._action = MailingAction.SEND;
            me.InitUpdater();
            me.AddParams();
            eConfirm(1, me._action.RES, detail + '<br />' + top._res_6635, '', 500, 180, me.Send);
            setWait(false);
        }
        else if (me._action == MailingAction.SEND_TEST) {

                if (me._mailingType == TypeMailing.MAILING_FOR_MARKETING_AUTOMATION) {
                    var title = getXmlTextNode(oDoc.getElementsByTagName("title")[0]);
                    var description = getXmlTextNode(oDoc.getElementsByTagName("description")[0]);
                    if (me._oWaitDialog != null)
                        me._oWaitDialog.hide();
                    if (top.updateCampaignInfo && typeof (top.updateCampaignInfo) == 'function')
                        top.updateCampaignInfo(id, title, description, 10);
                }

            if (!failure) {
                //switch le bloc bon a tirer avec les boutons de validation/invalidation

                var dt = eDate.ConvertDtToDisplay(new Date(Date.now()))
                document.getElementById("blocMailTestLibelleResultDateSpan").innerHTML = top._res_2899.replace("<DATETIME>", dt)
                //si l'extension est activé on reinitialize le bloc dans contrôle avant envoie
                if (me._TestType == 0 && mailTesterExtextionActivated)
                    me.SwitchCheckBeforeSendMailTester(0);
                else
                    me.SwitchCheckBeforeSendMailTester(1);

                me._mailTesterExtextionActivated = mailTesterExtextionActivated;
                var mailTesterLink = document.getElementById("blocMailTesterReportlink");
                if (mailTesterExtextionActivated) {
                    //on ajoute le nouveau lien vers mailTester
                    me._isErrorOnMailTester = false;

                    var errorMailTester = getXmlTextNode(oDoc.getElementsByTagName("errorOnMailTester")[0]) == "1";
                    if (errorMailTester) {//en cas d'erreur mail-tester
                        me._isErrorOnMailTester = true;
                        var errorMailTesterDetails = getXmlTextNode(oDoc.getElementsByTagName("errorMailTester")[0]);
                        DisplayMailTesterError(errorMailTesterDetails);
                        me._mailTesterReportId = "";
                    }
                    else {
                        //Lancer le test sur le statut du mailtester
                        var mailTesterReportWaitMessage = document.getElementById("mailTesterReportWaitMessage");
                        if (mailTesterReportWaitMessage)
                            mailTesterReportWaitMessage.style.display = "";

                        var mailTesterHref = getXmlTextNode(oDoc.getElementsByTagName("mailTesterLink")[0]);
                        mailTesterLink.style.display = "none";
                        mailTesterLink.setAttribute("href", mailTesterHref);
                        var mailTesterReportId = getXmlTextNode(oDoc.getElementsByTagName("mailTesterReportId")[0]);
                        mailTesterLink.setAttribute("rptId", mailTesterReportId);
                        me._mailTesterReportId = mailTesterReportId;
                        me._isErrorOnMailTester = false;
                        //on lance la vérif par interval
                        me.GetMailTesterStatus(mailTesterReportId);
                    }
                }


            }

            top.eAlert(msgType, top._res_5080, message, detail, 450, 180, function () {

                //#56383 problème d'annexes supprimées, il ne faut pas appeler me.Close
                if (me._oWaitDialog != null)
                    me._oWaitDialog.hide();

            });

        } else {
            top.eAlert(msgType, top._res_5080, message, detail, 450, 180, me.Close);
        }

        me._mailingId = isNaN(id) ? 0 : id;
        if (!isNaN(eventStepFileId))
            me.SetParam("eventStepFileId", eventStepFileId);

        // Mise à jour du file Id de la div de la fiche
        var fileDiv = document.getElementById("fileDiv_" + me._nCampaignTab);
        if (fileDiv)
            setAttributeValue(fileDiv, 'fid', me._mailingId);

        setWait(false);
    }

    this.GetMailTesterStatus = function (reportId) {
        var that = this;
        var increment = 5;//5tentative
        var timer = setInterval(function () {
            increment--;
            //on fait un appel asynchrone
            try {
                var request = new XMLHttpRequest();
                request.open('GET', "https://www.mail-tester.com/" + reportId + "&format=json", false);  // `false` makes the request synchronous
                request.send(null);

                if (request.status === 200) {
                    var t = JSON.parse(request.responseText);
                    if (!t.access || (!t.status && t.title != "Mail not found. Please wait a few seconds and try again.")) {
                        clearInterval(timer);
                        that._isErrorOnMailTester = true;
                        var mailTesterReportWaitMessage = document.getElementById("mailTesterReportWaitMessage");
                        if (mailTesterReportWaitMessage)
                            mailTesterReportWaitMessage.style.display = "none";
                        that.DisplayMailTesterError(t.title);//en cas d'erreur mail-tester
                    }
                    else if (t.status) {
                        that._isErrorOnMailTester = false;
                        var mailTesterReportWaitMessage = document.getElementById("mailTesterReportWaitMessage");
                        if (mailTesterReportWaitMessage)
                            mailTesterReportWaitMessage.style.display = "none";
                        var mailTesterLink = document.getElementById("blocMailTesterReportlink");
                        if (mailTesterLink)
                            mailTesterLink.style.display = "";
                        if (that._mailTesterExtextionActivated && !that._isErrorOnMailTester)
                            that.SetScore(
                                {
                                    mailtester: ((10 + t.mark) * 2.5).toFixed(2)
                                });
                        clearInterval(timer);//mail vérifié par mail-tester
                    }
                }
            }
            catch (e) {
                clearInterval(timer);//TODO:voir si après x tentative ou erreur on affiche un message d'erreur
            }

            if (increment = 0) {//TODO:voir si après x tentative ou erreur on affiche un message d'erreur
                clearInterval(timer);
            }
        }, 2000);//on fait un test chaque 2 sec
    }
    //Afficher l'erreur mailtester
    this.DisplayMailTesterError = function (errorDevDetails) {
        var oErrorObj = new Object();
        oErrorObj.Type = "0";
        oErrorObj.Title = top._res_416; // Erreur
        oErrorObj.Msg = top._res_3043; // Une erreur est survenue
        oErrorObj.DetailMsg = " ";
        oErrorObj.DetailDev = errorDevDetails;
        objReturn = "";
        eAlertError(oErrorObj)
    }

    /// confirmation avant de quitter ...
    this.OnClose = function () {
        me._forceClose = true;
        //TODO Message de confirmation
        me.Close();
    }



    ///<summary>
    /// Switch le bloc mailtest entre le bouton 'valider mon bat' et les bountons de validation
    /// passer true pour les bouton de validation
    /// 0 : mode envoi test
    /// 1 : retour de test
    /// 2 : test validé
    ///<summary>
    this.SwitchCheckBeforeSendMailTester = function (nType) {


        //subbloc
        var omailTesterSendContainer = document.getElementById("mailTesterSendContainer");
        var omailTesterValidateContainer = document.getElementById("mailTesterValidateContainer");
        var omailTesterSuccessContainer = document.getElementById("mailTesterSuccessContainer");


        //libelle
        var blocMailTestLibelleResultDate = document.getElementById("blocMailTestLibelleResultDate");
        var blocMailTestLibelleResult = document.getElementById("blocMailTestLibelleResult");
        var blocMailTestLibelleSend = document.getElementById("blocMailTestLibelleSend");

        switch (nType) {
            case 0:
                //Envoi de test
                omailTesterSendContainer.style.display = "";

                omailTesterValidateContainer.style.display = "none";
                omailTesterSuccessContainer.style.display = "none";

                blocMailTestLibelleResultDate.style.display = "none";
                blocMailTestLibelleResultDateSpan.innerHTML = "";

                blocMailTestLibelleResult.style.display = "none";
                blocMailTestLibelleSend.style.display = "";

                break;
            case 1:
                //retour envoi de test
                omailTesterSendContainer.style.display = "none";
                omailTesterValidateContainer.style.display = "";
                omailTesterSuccessContainer.style.display = "none";
                this._mailTestScoreCalculated = true;

                blocMailTestLibelleResultDate.style.display = "";
                blocMailTestLibelleResult.style.display = "";
                blocMailTestLibelleSend.style.display = "none";
                break;

            case 2:
                //envoi de test validé
                omailTesterSendContainer.style.display = "none";
                omailTesterValidateContainer.style.display = "none";
                omailTesterSuccessContainer.style.display = "";

                blocMailTestLibelleResultDate.style.display = "";
                blocMailTestLibelleResult.style.display = "";
                blocMailTestLibelleSend.style.display = "none";
                break;
        }


    }

    this.SetScoreInHtmlElement = function (element, scoreValue, colorClass, circleColor) {
        element.innerHTML = scoreValue;
        if (element.parentElement.classList.length > 1) {
            let classToRemove = element.parentElement.classList[1];
            if (classToRemove)
                element.parentElement.classList.remove(classToRemove);
        }
        if (colorClass && colorClass != "")
            element.parentElement.classList.add(colorClass);

        var circleElem = element.parentElement.parentElement.querySelector(".percent-circle");
        if (circleElem) {
            circleElem.setAttribute("stroke-dasharray", "158");// A voir ce calcul
            let scoredash = 158 - (158 * scoreValue / 25);
            if (scoreValue == 0) {
                scoredash = 0;
                if (this._mailTestScoreCalculated && element.id == "mailtesterScoreValue")
                    scoredash = 0;
                else if (!this._mailTestScoreCalculated && element.id == "mailtesterScoreValue")
                    scoredash = 158;
                else if (this._campaignTypeScoreCalculated && element.id == "campaignTypeScoreValue")
                    scoredash = 0;
                else if (!this._campaignTypeScoreCalculated && element.id == "campaignTypeScoreValue")
                    scoredash = 158;
            }

            if (scoredash == 158)
                scoredash++;
            circleElem.setAttribute("stroke-dashoffset", scoredash);
            circleElem.setAttribute("class", "percent-circle " + circleColor);
        }
    }

    this.SetScore = function (objScore) {
        me._scoring = Object.assign(me._scoring, objScore);

        let nTotal = 0;
        let classValue = "";

        for (var prop in me._scoring) {
            if (me._scoring.hasOwnProperty(prop)) {

                var docScore = document.getElementById(prop + 'ScoreValue');
                if (docScore) {
                    let nTVal = me._scoring[prop] * 1;
                    docScore.innerHTML = nTVal;
                    //TODO: enraichir cette partie selon le score                   
                    if (nTVal >= 20) {
                        this.SetScoreInHtmlElement(docScore, nTVal, "green-value", "green-circle");
                    }
                    else if (nTVal >= 10 && nTVal < 20) {
                        this.SetScoreInHtmlElement(docScore, nTVal, "orange-value", "orange-circle");
                    }
                    else if (nTVal < 10) {
                        this.SetScoreInHtmlElement(docScore, nTVal, "", "");
                    }

                    nTotal += nTVal;
                }
            }
        }

        //met a jour le scoreglobal
        var totSpan = document.getElementById("delivrabilityScoreSpanValue");
        if (totSpan)
            this.changeValue(totSpan, nTotal);
    }

    this.changeValue = function (totSpan, total) {
        var progressWrapper = document.querySelector('.progress-wrapper');
        var oldValue = totSpan.innerHTML;
        totSpan.innerHTML = total;
        this.animateValue('.progress-value', parseInt(oldValue), total, 500);
        if (total >= 80)
            progressWrapper.setAttribute("class", "blocDelivrabilityScore progress-wrapper green-value");
        else if (total <= 79 && total >= 40)
            progressWrapper.setAttribute("class", 'blocDelivrabilityScore progress-wrapper orange-value');
        else
            progressWrapper.setAttribute("class", 'blocDelivrabilityScore progress-wrapper red-value');

        var progressFile = document.getElementById('file-progress');
        if (progressFile)
            progressFile.setAttribute("value", total);
    }

    this.animateValue = function (cssClass, start, end, duration) {
        if (start === end) return;
        var range = end - start;
        var current = start;
        var increment = end > start ? 1 : -1;
        var stepTime = Math.abs(Math.floor(duration / range));
       
        var timer = setInterval(function () {
            current += increment;
            if (current >= end) {
                clearInterval(timer);
            }
        }, stepTime);
    }

    this.ResetMailTester = function () {



        if (me._mailingId > 0) {

            var upd = new eUpdater("mgr/eMailingManager.ashx", 0);
            upd.addParam("fileid", me._mailingId, "post");
            upd.addParam("operation", MailingAction.RESET_MAILTESTER.KEY, "post");   //reset les infos mailtester
            upd.addParam("sender", "RESET_MAIL_TESTER", "post");
            upd.ErrorCallBack = function () { top.setWait(false) };
            upd.send();
        }


        //Passe le score a 0 et retourne sur l'étape design et switch les boutons de test
        this.SetScore(
            {
                mailtester: 0
            }
        )

        //repasse en mode mailtester
        this.SwitchCheckBeforeSendMailTester(0);

        if (this._mailTesterExtextionActivated) {
            var mailTesterLink = document.getElementById("blocMailTesterReportlink");
            if (mailTesterLink) {
                mailTesterLink.style.display = "none";
                mailTesterLink.setAttribute("href", "");
                mailTesterLink.setAttribute("rptId", "");
            }

            var mailTesterReportWaitMessage = document.getElementById("mailTesterReportWaitMessage");
            if (mailTesterReportWaitMessage) {
                mailTesterReportWaitMessage.style.display = "";
            }
        }
    }

    ///<summary>
    ///Valide le design d'un mail, aprés l'avoir tester
    /// passer true pour les bouton de validation
    ///<summary>
    this.ValidDesign = function (bIsValid) {

        if (bIsValid) {
            this._isUserNotValidDesign = false;
            //Passe le score a 25 pour mail testert
            if (!this._mailTesterExtextionActivated || (this._mailTesterExtextionActivated && this._isErrorOnMailTester)) {
                this.SetScore(
                    {
                        mailtester: !this._mailTesterExtextionActivated || (this._mailTesterExtextionActivated && this._isErrorOnMailTester) ? 25 : 0//sil'extension mail-tester ou en cas d'erreur dans le test on garde l'ancien fonctionnement cad la note est 25
                    }
                );
            }

            this.SwitchCheckBeforeSendMailTester(2)

        }
        else {
            this.ResetMailTester()

            //Repasse a l'étape conception
            var mystep = document.querySelector("div[stepname='mail']")
            if (mystep == null)
                mystep = document.querySelector("div[stepname='mailck']")

            if (mystep != null) {


                var nStep = mystep.id.replace('editor_', '')
                if (nStep >= 2 && nStep <= 3)
                    SwitchStep(nStep)
            }

        }
    }

    //On ferme l'assistant
    this.Close = function () {

        //Si on ferme l'assitant sans enregistrement ou erreur, on supprime les pj
        if (me._mailingId == 0)
            DeletePJ(me._nCampaignTab, 0, me._pjIds, false, null);

        if (me._oWaitDialog != null)
            me._oWaitDialog.hide();

        //En cas de success  on ferme l'assistant
        if ((me._success && me._action == MailingAction.SEND) || me._forceClose)
            if (top) {
                if (top.window['_md']['MailingWizard'])
                    top.window['_md']['MailingWizard'].hide();
            }
    }

    ///<summary>
    /// Gestion des erreurs de retour
    ///<summary>
    this.ManageReturnedError = function (oErr) {
        me.Close();
    }


    this.SetCssExistingCampaign = function () {

        var css = this.GetParam('bodyCss');
        this._oMemoEditor.injectCSS(css);
        this._oMemoEditor.setColor(this._oMemoEditor.getColorFromCSS(css));
    }


    this.OnSelectDelayed_Now = function () {
        this.SetParam('immediateSending', '1');
        this.SetParam('sendingDate', '');
        this.SetParam('recurrentSending', '0');
        if (this.checked)
            setAttributeValue(document.getElementById('delayedMail_Date'), 'value', '');

        DisplayRequestMode(false);
        DisplayRecurrentMode(false);
        DisplayBtnSaveCampaign(true);
    };

    this.OnSelectDelayed_Later = function () {
        this.SetParam('immediateSending', '0');
        this.SetParam('recurrentSending', '0');
        DisplayRequestMode(true);
        DisplayRecurrentMode(false);
        DisplayBtnSaveCampaign(true);
    };

    this.OnSelectDelayed_Recurrent = function () {
        this.SetParam('immediateSending', '0');
        this.SetParam('sendingDate', '');
        this.SetParam('recurrentSending', '1');
        DisplayRequestMode(false);
        DisplayRecurrentMode(true);
        DisplayBtnSaveCampaign(false);
    };

    this.OnSelectRequestMode = function (oRBRequestMode) {
        this.SetParam('RequestMode', oRBRequestMode.value);
        DisplayRecipientsFilter(oRBRequestMode.value == MailingQueryMode.RECURRENT_FILTER);
    };

    ///<summary>
    /// Affiche ou cache les choix de l'envoi différé
    ///<summary>
    function DisplayRequestMode(bDelayedMode) {
        var oDelayedMail_RequestMode = document.getElementById("delayedMail_RequestMode");
        if (!oDelayedMail_RequestMode) {
            return;
            //   throw ("oDelayedMail_RequestMode introuvable !");
        }
        oDelayedMail_RequestMode.style.display = (bDelayedMode) ? "block" : "none";
    };

    ///<summary>
    /// Affiche ou cache les choix de l'envoi récurrent
    ///<summary>
    function DisplayRecurrentMode(bDelayedRecurrentMode) {
        var oDelayedMailRecurrent_RequestMode = document.getElementById("delayedMailRecurrent_RequestMode");
        if (!oDelayedMailRecurrent_RequestMode) {
            return;
            //   throw ("oDelayedMailRecurrent_RequestMode introuvable !");
        }
        oDelayedMailRecurrent_RequestMode.style.display = (bDelayedRecurrentMode) ? "block" : "none";
    };



    ///<summary>
    /// Affiche ou cache le bouton d'enregistrement de la campagne
    ///<summary>
    function DisplayBtnSaveCampaign(bDisplay) {
        var modalWizard = top.modalWizard;
        var buttonModalDiv = modalWizard.getIframe().parent.document.getElementById("ButtonModal" + modalWizard.iframeId.replace("frm", ""));
        var btnSaveCampaign = buttonModalDiv.ownerDocument.getElementById("savecampaign_btn");
        btnSaveCampaign.style.visibility = (bDisplay) ? "" : "hidden";

    }
    ///<summary>
    /// Affiche ou cache le lien du filtre
    ///<summary>
    function DisplayRecipientsFilter(bRecipientsFilter) {
        var oDelayedMailRecurrent_Filter = document.getElementById("delayedMailRecurrent_Filter");
        if (!oDelayedMailRecurrent_Filter)
            throw ("oDelayedMailRecurrent_Filter introuvable !");
        oDelayedMailRecurrent_Filter.style.display = (bRecipientsFilter) ? "block" : "none";
    };

    /// 42325 : Recherche des liens sans tracking défini
    this.checkLinksWithoutTracking = function (mailBody) {
        if (mailBody) {
            // Recherche des liens avec ednc="lnk" et ednd="0"
            var regex = new RegExp("<a.*(ednc=\"lnk\"){1}.*(ednd=\"0\"){1}.*>.*<\/a>");
            if (mailBody.match(regex)) {
                return true;
            }
        }

        return false;
    };

    this.openScheduleParameter = function () {
        var nNew = 0;
        if (me._mailingId != 0)
            nNew = 1;

        // On choisi le prochain creaneau d'heure
        var d = new Date();
        while (d.getMinutes() % 30 != 0) {
            d.setMinutes(d.getMinutes() + 1);
        }
        var hours = ("0" + d.getHours()).slice(-2);
        var minutes = ("0" + d.getMinutes()).slice(-2);

        me._modalSchedule = new eModalDialog(top._res_1049, 0, "eSchedule.aspx", 450, 500);

        me._modalSchedule.addParam("scheduletype", 2, "post");
        me._modalSchedule.addParam("New", nNew, "post");
        me._modalSchedule.addParam("iframeScrolling", "yes", "post");
        me._modalSchedule.addParam("EndDate", 0, "post");
        me._modalSchedule.addParam("BeginDate", 0, "post");
        me._modalSchedule.addParam("ScheduleId", me.GetParam("scheduleId"), "post");
        me._modalSchedule.addParam("Tab", 0, "post");
        me._modalSchedule.addParam("Workingday", "TODO", "post");
        me._modalSchedule.addParam("calleriframeid", 0, "post");
        me._modalSchedule.addParam("hour", hours + ":" + minutes, "post");
        me._modalSchedule.addParam("AppType", 0, "post");
        me._modalSchedule.addParam("FileId", me._mailingId, "post");

        me._modalSchedule.ErrorCallBack = me.openScheduleParameterCancelReturn();

        me._modalSchedule.show();
        me._modalSchedule.addButtonFct(top._res_29, function () { me.openScheduleParameterCancelReturn(); }, "button-gray", 'cancel');
        me._modalSchedule.addButtonFct(top._res_28, function () { me.openScheduleParameterValidReturn(); }, "button-green");
    };

    this.openScheduleParameterValidReturn = function (oModal) {
        me._modalSchedule.getIframe().Valid(me.ValidMailingScheduleTreatment);
    }

    this.openScheduleParameterCancelReturn = function () {
        me._modalSchedule.hide();
    }

    this.TypeWizard = "mailing",


        this.ValidMailingScheduleTreatment = function (oRes) {
            var scheduleId = getXmlTextNode(oRes.getElementsByTagName("scheduleid")[0]);

            me.SetParam("scheduleId", scheduleId);
            me.SetParam("scheduleUpdated", "1");
            me._modalSchedule.hide();

            var scheduleInfo = document.getElementById("lnkScheduleInfo");
            if (scheduleInfo) {
                scheduleInfo.style.display = "";

                SetText(scheduleInfo, "\n" + getXmlTextNode(oRes.getElementsByTagName("scheduleinfo")[0]));
            }
        }

    this.openRecipientsFilterModal = function (nTab) {
        var options = {
            tab: nTab,
            onApply: function (modal) {
                me.applyRecipientsFilterModal(modal);
            },
            value: me.GetParam("recipientsFilterId"),
            deselectAllowed: true,
            selectFilterMode: true,
        }
        filterListObjet(0, options);
    }

    this.applyRecipientsFilterModal = function (modal) {
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
        me.SetParam("recipientsFilterId", currentFilterId);

        var filterInfo = document.getElementById("lnkFilterInfo");
        if (filterInfo) {
            filterInfo.style.display = currentFilterId != "0" ? "" : "none";
            SetText(filterInfo, " : \"" + currentFilterLib + "\"");
        }

        modal.hide();
    }

    //Enable or desable switch
    this.ChangeSwitch = function (elem, id) {


        var swtchInput = document.getElementById("sw" + id);
        if (swtchInput) {
            swtchInput.checked = !swtchInput.checked;
        }

        FilterTargetCampaign()


    }

    this.CalculScore = function (id) {
        let score = 0;
        var pos = id.indexOf("campaign");

        if (pos != -1) {
            var swtchInputFirst = document.getElementById("swcampaignSwOptin");
            var swtchInputSecond = document.getElementById("swcampaignSwNoopt");
            var swtchInputThird = document.getElementById("swcampaignSwOptout");
        }
        else {
            var swtchInputFirst = document.getElementById("swqualityAdressEmailSwValide");
            var swtchInputSecond = document.getElementById("swqualityAdressEmailSwNotVerified");
            var swtchInputThird = document.getElementById("swqualityAdressEmailSwInvalide");
        }



        var optin = document.getElementById("spnNbrDestcampaignSwOptin").innerHTML;
        var Noconsent = document.getElementById("spnNbrDestcampaignSwNoopt").innerHTML;
        if (!swtchInputFirst.checked && !swtchInputSecond.checked && !swtchInputThird.checked) {
            score = 0;
            var divcampaignscore = document.getElementById('campaignTypeScoreValue');
            let classToRemove = divcampaignscore.parentElement.classList[1];
            if (classToRemove) {
                divcampaignscore.parentElement.classList.remove(classToRemove);
                divcampaignscore.innerHTML = score;
            }

        } else {
            if (swtchInputThird.checked)
                score = 0;
            else {

                if (swtchInputFirst.checked && swtchInputSecond.checked) {
                    if (Noconsent != 0 && optin != 0)
                        score = Math.round(((optin * 1 / (optin * 1 + Noconsent * 1)) * 20))
                    else if (Noconsent == 0 && optin == 0)
                        score = 0;
                    else if ((Noconsent == 0 && optin != 0))
                        score = 25;
                } else if (swtchInputFirst.checked || !swtchInputSecond.checked) {
                    score = 25;
                } else if (!swtchInputFirst.checked || swtchInputSecond.checked) {
                    score = 10;
                }
            }

            if (pos != -1) {
                this.SetScore(
                    {
                        campaignType: score
                    }
                )
            } else {
                this.SetScore(
                    {
                        qualityEmailAdresses: score
                    }
                )
            }

        }
    }


}




function switchToContainer(input, event) {
    let AllContents = document.getElementsByClassName('field-container--content');
    for (var i = 0; i < AllContents.length; i++) {
        AllContents[i].style.display = 'none';
    }
    let idElem = input.id;
    let contentElem = document.getElementsByClassName(idElem)[0];
    contentElem.style.display = 'block';

    if (idElem == "immediateDispatch")
        oMailing.OnSelectDelayed_Now();
    if (idElem == "delayedSending")
        oMailing.OnSelectDelayed_Later();
    if (idElem == "recurringSending")
        oMailing.OnSelectDelayed_Recurrent()
}


