/// <reference path="ePerm.js" />

/// Object qui traite la selection des modèles d'emailing
function eMailingTemplate() {
    //enum Operation
    var LOAD_OPERATION = 7;

    ///Editeur actif (grapjs / ck / ...)
    var activeEditor = 0;

    var self = this;
    ///id du modele 
    this.id = 0;
    ///libellé pour les modèles utilisateurs
    this.name = "";
    ///type de modele utilisateur/personnalisable
    this.type = 0;

    this.bFromCampaign = false;
    ///type de modele de mail (emailing/mail unitaire)
    this.mtType = 0;

    ///permissions
    this.oPerm = new ePermission();

    this.selectedUserTpl = null;
    this.selectedCustomTpl = null;

    this._waitDialog = null;
    this._parentDialog = null;
    this._success = false;
    ///Initialisation de l'objet
    this.Init = function () {
        self.id = oMailing.GetParam("templateId");
        self.type = oMailing.GetParam("templateType");
        self.mtType = 1; // Modèle de mail pour e-mailing
        if (self.id > 0) {
            //Modele personnalisable
            if (self.type == 0) {

                elemId = "tpl_" + oMailing.GetParam("templateId");
                self.selectedCustomTpl = document.getElementById(elemId);

                if (self.selectedUserTpl != null)
                    removeClass(self.selectedUserTpl, "eSel");

                if (self.selectedCustomTpl != null)
                    addClass(self.selectedCustomTpl, "graphCadreSel");
            } else {

                if (self.type == 0)
                    initMailTpl();
            }
        }


    }

    ///Retourne les permission sur le modèle en cours
    this.GetPerm = function () {
        return self.oPerm;
    }

    ///Reset les permissions sur le modèle en cours
    this.SetPerm = function (newPerm) {
        self.oPerm = newPerm;
    }

    /// Savoir si la campagne actuelle est sauvegardée comme modele
    this.SetFromCampaign = function (value) {
        self.bFromCampaign = value;
    }


    ///Mis a jour du libellé modèle
    this.SetName = function (newName) {
        self.name = newName;
    }

    ///Mis a jour le Type du modèle : utilisateurs/personalisable 
    this.SetType = function (type) {
        self.type = type;
    }

    ///retourne le Type du modèle : utilisateurs/personalisable 
    this.GetType = function () {
        return self.type;
    }

    ///Met a jour le Type de modèle en base : emailing/mail unitaire
    this.SetMailTemplateType = function (type) {
        self.mtType = type;
    }

    ///retourne le Type de modèle en base : emailing/mail unitaire
    this.GetMailTemplateType = function () {
        return self.mtType;
    }

    ///Id du model
    this.GetTplId = function () {
        return self.id;
    }

    ///Libellé du modèle
    this.GetName = function () {
        return self.name;
    }

    ///Ne pas utiliser de modèle ou modèle venant de la campagne en cours
    this.NoTemplate = function () {
        return self.type == 2 || this.bFromCampaign;
    }

    /// Affiche la div correspondant au type du modèle 
    this.DisplayBlock = function (type) {

        self.SetType(type);

        switch (parseInt(type)) {
            case 0: //choix des modèles personnalisables
                self.hide('divUserTemplates').show('divCustomTemplates');
                break;

            case 1: //liste des modèles utilisateurs           
                self.show('divUserTemplates').hide('divCustomTemplates');
                if (typeof (onFrameSizeChange) == 'function' && top.modalWizard)
                    onFrameSizeChange(top.modalWizard.getDivMain().offsetWidth, top.modalWizard.getDivMainHeight());
                break;

            case 2: // pas de moddèle

                self.id = 0;
                self.name = "";
                self.hide('divUserTemplates').hide('divCustomTemplates');

                if (self.selectedUserTpl != null)
                    removeClass(self.selectedUserTpl, "eSel");

                if (self.selectedCustomTpl != null)
                    removeClass(self.selectedCustomTpl, "graphCadreSel");
                break;
        }
    }

    ///Selection du modèle utilisateur
    this.selectUserTpl = function (obj) {

        // Get parent
        var elem = obj;
        while (elem.tagName != 'TR') {

            elem = elem.parentNode || elem.parentElement;
            if (elem.tagName == 'TBODY')
                return;
        }

        if (self.selectedUserTpl != null)
            removeClass(self.selectedUserTpl, "eSel");

        if (self.selectedCustomTpl != null)
            removeClass(self.selectedCustomTpl, "graphCadreSel");

        if (addClass != null)
            addClass(elem, "eSel");

        self.id = getAttributeValue(elem, "eid").split("_")[1];

        self.selectedUserTpl = elem;
        self.selectedCustomTpl = null;

        // [0 : owener template] ou [1 : costum template] ou [2 : no template ] 
        self.SetType(0);

        // Choix d'un modèle existant, ce  template ne provient pas de la campagne en cours
        self.SetFromCampaign(false);
    }

    ///Mise à jour de l'objet 
    this.UpdateUserTpl = function (oDoc) {

        var newId = getXmlTextNode(oDoc.getElementsByTagName("iMailTemplateId")[0]);
        var newType = getXmlTextNode(oDoc.getElementsByTagName("mailTplType")[0]);
        var newName = getXmlTextNode(oDoc.getElementsByTagName("mailTplname")[0]);

        var viewPermId = getXmlTextNode(oDoc.getElementsByTagName("ViewPermId")[0]);
        var updatePermId = getXmlTextNode(oDoc.getElementsByTagName("UpdatePermId")[0]);

        self.SetType(0); //modèle utilisateur
        self.id = newId;
        self.name = newName;

        self.oPerm.SetPermParam("view", "id", viewPermId);
        self.oPerm.SetPermParam("update", "id", updatePermId);

        //On force l'affichage de la liste des modèles (e-mailing uniquement)
        if (document.getElementById("rbMyTemplates") && !self.bFromCampaign) {
            document.getElementById("rbMyTemplates").click();
        }

        //Annule le modèle perso
        if (self.selectedCustomTpl != null)
            removeClass(self.selectedCustomTpl, "graphCadreSel");

        self.selectedCustomTpl = null;

        //Quand on fait enregistrer le modèle à l'etape d'edition du mail
        //On met à jour l'emailing 
        //TODO, Actuellement, les pj du modèle utilisé sont dupliqué à l'enregistrement de la campagne Mail       
        /*
        if (typeof (iCurrentStep) != "undefined") {
            if (oMailing.GetStepName(iCurrentStep) == "mail") {
                oMailing.SetParam("templateId", self.id);
                oMailing.SetParam("templateType", self.type);
            }
        }*/
    }

    ///On selectionne un template
    this.selectCustomTpl = function (e) {

        // Récupération de l'évènement
        if (!e)
            var e = window.event;

        // Objet source
        var oSourceObj = e.target || e.srcElement;

        //radio button no-template 
        var bIsFromEmptyTpl = document.getElementById("rbNoTemplate").checked;

        if (oSourceObj.tagName != "DIV" || bIsFromEmptyTpl) {
            return;
        }

        // on désactive la template précédemment sélectionnée
        if (self.selectedCustomTpl != null)
            removeClass(self.selectedCustomTpl, "graphCadreSel");

        if (self.selectedUserTpl != null)
            removeClass(self.selectedUserTpl, "eSel");

        // on active celui surlequel on vient de cliquer
        var elmId = getAttributeValue(oSourceObj, "id")
        self.selectedCustomTpl = document.getElementById(elmId);

        self.id = elmId.split("_")[1];

        addClass(self.selectedCustomTpl, "graphCadreSel");

        // [0 : owener template] ou [1 : costum template] ou [2 : no template ]       
        self.SetType(1);

        self.selectedUserTpl = null;
        self.name = ""; //pas de nom pour les modèle personnalisable
    }


    this.setSelectedUserTemplate = function (id) {

        self.id = id;

        // Custom template   
        self.SetType(0);

        self.selectedUserTpl = null;
        self.selectedCustomTpl = null;

        self.name = ""; //pas de nom pour les modèle personnalisable
    }

    ///Action au double clique d'un modèle d'tuilisateur : sélection puis passage à l'étape suivante (assistant d'e-mailing) ou validation
    ///(fenêtre de sélection de modèle de mail unitaire)
    ///obj : ligne sélectionnée
    this.dblclckUserTemplate = function (obj) {
        selectMailTpl(obj);
        if (typeof (MoveStep) == "function")
            MoveStep(true, 'mailing');
        else if (typeof (onApplySelectedMailTemplate) == "function")
            onApplySelectedMailTemplate();
    };
    ///Action au double clique d'un modèle personnalisable : sélection puis passage à l'étape suivante
    ///e : evennement déclencheur pour retrouvé le modèle sélectionné.
    this.dblclckSysTemplate = function (e) {
        this.selectCustomTpl(e);
        if (self.selectedCustomTpl)
            MoveStep(true, 'mailing');
    }
    ///gestion de l'affichage
    this.show = function (id) {
        SetDisplay(true, new Array(id));
        return self;
    }

    this.hide = function (id) {
        SetDisplay(false, new Array(id));
        return self;
    }

    ///Charge un modèle de mail depuis le serveur
    this.Load = function () {
        var oUpdater = new eUpdater("mgr/eMailingTemplateManager.ashx", 0);

        oUpdater.addParam("MailTemplateId", self.id, "post");
        oUpdater.addParam("operation", LOAD_OPERATION, "post");
        oUpdater.addParam("tplType", self.type, "post");
        oUpdater.addParam("tplTypeDb", self.mtType, "post");

        oUpdater.ErrorCallBack = self.ManageReturneError;

        var title = top._res_748;
        var message = top._res_644;
        self._oWaitDialog = showWaitDialog(title, message);
        self._oParentDialog = oTemplate._parentDialog;

        oUpdater.send(self.ManageFeedback);
    }

    ///retour serveur
    this.ManageFeedback = function (oDoc) {        
        if (!oDoc) return;

        self._success = getXmlTextNode(oDoc.getElementsByTagName("success")[0]) == "1";

        if (!self._success) {
            var message = getXmlTextNode(oDoc.getElementsByTagName("message")[0]);
            //TODORES
            message = top._res_8173; // "Impossible de charger le modèle !";
            var detail = top._res_6342;
            eAlert(1, top._res_5080, message, detail, 400, 180, function () { });
        }
        else {
            var table = getXmlTextNode(oDoc.getElementsByTagName("table")[0]);
            var css = getXmlTextNode(oDoc.getElementsByTagName("body_css")[0]);
            //Html  
            var isHtml = getXmlTextNode(oDoc.getElementsByTagName("body_html")[0]);
            var isHtmlDescId = getAttributeValue(oDoc.getElementsByTagName("body_html")[0], "id");

            //Nom modèle
            self.name = getXmlTextNode(oDoc.getElementsByTagName("mailTplname")[0]);
            self.type = getXmlTextNode(oDoc.getElementsByTagName("mailTplType")[0]);
            self.mtType = getXmlTextNode(oDoc.getElementsByTagName("mailTplTypeDb")[0]);
            self.id = getXmlTextNode(oDoc.getElementsByTagName("iMailTemplateId")[0]);

            //Corps de mail
            var body = getXmlTextNode(oDoc.getElementsByTagName("body")[0]);
            var bodyDescId = getAttributeValue(oDoc.getElementsByTagName("body")[0], "id");


            /* smsing */
            if (self.mtType == 2 || self.mtType == 3) {

                var modalSMSWizard = eTools.GetModal("SmsMailingWizard");
                if (modalSMSWizard
                    && typeof modalSMSWizard.getIframe == "function"
                    && modalSMSWizard.getIframe().oSmsing) {
                    var _oSmsing = modalSMSWizard.getIframe().oSmsing;
                    _oSmsing.GetMemoEditor().setData(body, null, true);
                    if (_oSmsing.GetWizardType() == 1)//Dans le cas où c'est un sms unitaire, on recalcule le nombre de caract 
                        _oSmsing.CalculateSMSLength(body);
                    var label = modalSMSWizard.getIframe().document.getElementById("smsingShowTplLstBtn");

                   
                    _oSmsing.SetTemplateId(self.id)
                    var tplframewiz = modalSMSWizard.getIframe().oTemplate;

                    if (tplframewiz) {
                        tplframewiz.name = self.name
                        tplframewiz.id = self.id

                    }

                    if (label)
                        label.innerHTML = top._res_464 + " : " + self.name;
                }
                else {

                    updateMemoData(bodyDescId, body, css); //depuis eMain.js
                }

            


            }

            else {

                /* emailing */

                //Objet de mail
                var subject = getXmlTextNode(oDoc.getElementsByTagName("subject")[0]);
                var subjectDescId = getAttributeValue(oDoc.getElementsByTagName("subject")[0], "id");
                var item = document.getElementById(subjectDescId);
                // Dans le cas des modèles de mails unitaires, il faut mettre à jour la propriété réelle "value" du champ input
                if (item == null && oMailing._oSubjectEditor != null) {
                    item = oMailing._oSubjectEditor;
                    item.value = subject;
                }
                // Puis attributs spécifiques Eudo
                if (item) {
                    item.innerHTML = subject;//KJE tâche 2 334
                    setAttributeValue(item, "value", subject);
                    setAttributeValue(item, "ednvalue", subject);
                }

                //SHA : tâche #1 939
                //Texte d'aperçu (preheader)
                var preheader = getXmlTextNode(oDoc.getElementsByTagName("preheader")[0]);
                var preheaderDescId = getAttributeValue(oDoc.getElementsByTagName("preheader")[0], "id");
                var itemPreheader = document.getElementById(preheaderDescId);
                setAttributeValue(itemPreheader, "value", preheader);
                setAttributeValue(itemPreheader, "ednvalue", preheader);





                // Liste des PJ rattacés au modèle
                var pjIds = getXmlTextNode(oDoc.getElementsByTagName("templatePjIds")[0]);
                var nbPj = getAttributeValue(oDoc.getElementsByTagName("templatePjIds")[0], "nbpj");


                if (oMailing._oMemoEditor) {
                    //Important l'ordre : data puis les styles
                    // Backlog #619 - RAZ de la couleur de fond
                    // #71 938 : ATTENTION, il faut faire ici strictement la même chose que dans eMemoEditor.injectHTMLTemplateEditorData, sinon, des différences de rendu auront lieu entre l'édition du modèle et son utilisation !
                    oMailing._oMemoEditor.setData(body, null, true);
                    oMailing._oMemoEditor.injectCSS(css);
                    //Ajout du css pour placeholder
                    oMailing._oMemoEditor.injectCSS(".Column:empty:before {	background-color: #ddd;	color: #000;	font-size: 16px;	font-weight: bold;	height: 100%;	display: flex;	align-items: center; text-align: center;	justify-content: center;	min-height: 50px;	padding: 0 10px;	opacity: 0.3;	border-radius: 3px;	overflow: hidden;	text-overflow: ellipsis;	content: '" + top._res_2991 + "'}", true, true);
                    oMailing._oMemoEditor.setColor(oMailing._oMemoEditor.getColorFromCSS(css)); // Backlog #619 : injecter la couleur de fond sélectionnée dans grapesjs lors de l'édition du modèle
                } else {
                    updateMemoData(bodyDescId, body, css); //depuis eMain.js
                }


                var pjDescid = ((table * 1) + 91);
                var name = 'COL_' + table + '_' + pjDescid;
                var hidPJ = document.querySelector("[ename='" + name + "']");
                var dspCntPj = document.getElementById("dspCntPj_" + table);

                if (top.eModFile && top.eModFile.isModalDialog) {

                    if (typeof (top.eModFile.getIframe) == "function") {

                        var myModFrame = top.eModFile.getIframe();

                        if (myModFrame != null) {
                            if (typeof hidPJ == 'undefined' || hidPJ == null)
                                hidPJ = top.eModFile.getIframe().document.querySelector("[ename='" + name + "']");

                            if (typeof dspCntPj == 'undefined' || dspCntPj == null)
                                dspCntPj = top.eModFile.getIframe().document.getElementById("dspCntPj_" + table);
                        }
                    }
                }

                if (typeof dspCntPj != 'undefined' && dspCntPj != null)
                    dspCntPj.innerHTML = "(" + nbPj + ")";


                setAttributeValue(hidPJ, "pjids", pjIds);
                setAttributeValue(hidPJ, "nbpj", nbPj);


                //mise a jour emailing
                oMailing.SetParam("bodyCss", css);
                oMailing.SetParam("templateId", self.id);
                oMailing.SetParam("templateType", self.type);
            }
        }
        if (self._oWaitDialog != null)
            self._oWaitDialog.hide();

        // Fermeture de la fenêtre passée en paramètre (cas des mails unitaires)
        if (self._oParentDialog != null)
            self._oParentDialog.hide();
    }

    this.ManageEmailing = function () {

    }


    this.ManageSmsing = function () {

    }

    ///Erreur serveur
    this.ManageReturnedError = function (oErrDoc) {
        if (self._oWaitDialog != null)
            self._oWaitDialog.hide();
    }

    this.AddCKEditorSubject = function () {

        var subjectContainer = document.getElementById('CKEditorObject');
        var subject = document.getElementById("obj");
        if (subject) {
            var eMemoDialogEditorObject = new eMemoEditor("eMemoDialogEditor", true
                , subjectContainer
                , null
                , subject.innerHTML
                , false, 'eMemoDialogEditorObject');
            eMemoDialogEditorObject.inlineMode = true;
            eMemoDialogEditorObject.setSkin('eudonet');
            eMemoDialogEditorObject.config.width = "100%";
            eMemoDialogEditorObject.config.height = "150px";
            eMemoDialogEditorObject.show();
        }

    }


    this.AddCKEditorSubject();
}



