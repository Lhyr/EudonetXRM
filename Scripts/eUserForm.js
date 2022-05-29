/// <reference path="eTools.js" />
/// <reference path="eEngine.js" />


///Formulaire utilisateur
var eUserForm = (function () {

    var _sw = (top.setWait) ? top.setWait : setWait;

    //vérification du format coté client
    var _fields, _nTab, _fileId, _formularType;

    var _isInit = false;


    ///Vérifie la validité du formulaire
    function isValidForm() {

        //champs saisie formulaire
        _fields = getFieldsInfos(_nTab, _fileId);

        var obligatFields = "";
        var chckObliNotCoched = false;
        for (var key in _fields) {
            //rubrique obligatoires
            if (_fields[key].obligat == true && _fields[key].newValue == "") {
                var label = document.getElementById("COL_" + _nTab + "_" + _fields[key].descId);
                if (label != null) {

                    var mylab = label.innerHTML.replace("*", "");
                    var div = document.createElement("div");
                    div.innerHTML = mylab;
                    mylab = div.textContent || div.innerText || mylab;

                    obligatFields += mylab + "<br />";
                }
            }

            if (_formularType == "1" && _fields[key].obligat == true && _fields[key].newValue == "0") {
                chckObliNotCoched = true;
            }

            if (!checkFormat(_fields[key]))
                return false;
        }

        //rubriques obligatoires
        if (chckObliNotCoched) {
            _sw(false);
            eAlert(1, top._res_6377, top._res_2657);
            return false;
        }
        else if (obligatFields.length > 0) {
            _sw(false);
            eAlert(1, top._res_6377, top._res_6564, obligatFields);
            return false;
        }

        //le formulaire est valide
        return true;
    }
    var _errorIsDisplayed = false;
    ///vérifie la validité _field[key].newValue
    function checkFormat(oFieldEngine, errCallBack) {
      
        //si on veut eviter de surcharger le serveur 
        var infoElm = document.getElementById("COL_" + _nTab + "_" + oFieldEngine.descId);
        var frm = getAttributeValue(infoElm, "frm") + "";
        var newValue = oFieldEngine.newValue + "";

        //Inutile de vérifier si la valeur ou le format ne sont pas renseignés,   
        if (newValue.length == 0 || frm.length == 0)
            return true;

        var isValid = false;

        if (isFormatNumeric(frm)) {
            var trueValue = document.getElementById(oFieldEngine.cellId).value;
            eNumber.ConvertDisplayToBdd(trueValue);
            isValid = eNumber.IsValid();
        }
        else if (frm == FLDTYP.DATE) {
            var trueValue = document.getElementById(oFieldEngine.cellId).value;

            isValid =  eValidator.isDateJS(trueValue)
            /*
            eDate.ConvertDisplayToBdd(trueValue);
            isValid = eDate.IsValid();
            */
        }
        else
            isValid = eValidator.isValid({ value: newValue, format: frm });

        if (!isValid) {

            _sw(false);
            //6275, "Format incorrect"
            var title = top._res_6275;
            //2021,"La valeur saisie ""<VALUE>"" ne correspond pas au format et/ou à la longueur de la rubrique <FIELD>"
            var errorMessage = top._res_2021.replace("<VALUE>", oFieldEngine.newValue).replace("<FIELD>", infoElm.innerHTML.replace("*", ""));
            if (!_errorIsDisplayed) {
                _errorIsDisplayed = true;
                eAlert(1, title, errorMessage, null, null, null,
                        function () {
                            eUserForm.ErrorAlertClosed();
                            document.getElementById(oFieldEngine.cellId).focus();
                        }
                    );
            }
            return false;
        }

        return true;
    }

    //Poste les données du formulaire
    function submitFormular() {
        if (document.getElementById("sub").value == "1")
            return;

        disableSubmit(true);

        //IsPostBack
        document.getElementById("re").value = "1";
        var form = document.getElementById("userForm");

        //Initialisation
        var eEngineUpdater = new eEngine();
        eEngineUpdater.Init();
        eEngineUpdater.Formular = true;

        //Ajoute les champs
        forEach(getFieldsInfos(_nTab, _fileId), function (fld) {
            eEngineUpdater.AddOrSetField(fld);
        });

        //Ajoute les paramètres spécifiques
        var oD = document.getElementById("ContextPanel");
        if (oD) {
            var lstInput = oD.getElementsByTagName("input");
            forEach(lstInput, function (inpt) {
                eEngineUpdater.AddOrSetParam(inpt.name, inpt.value);
            });
            if (_fileId <= 0) {
                var sCaptcha = document.getElementById("txt_captcha");
                if (!sCaptcha)
                    sCaptcha = "";
                else
                    sCaptcha = sCaptcha.value;

                eEngineUpdater.AddOrSetParam("Captcha", sCaptcha);
            }

            //on ajoute recaptcha google
            if (_formularType == '1') {
                var sReCaptcha = document.getElementById("g-recaptcha-response");
                if (!sReCaptcha)
                    sReCaptcha = "";
                else
                    sReCaptcha = sReCaptcha.value;

                eEngineUpdater.AddOrSetParam("reCaptcha", sReCaptcha);
            }
        }

        //Méthode de callback de succès
        eEngineUpdater.SuccessCallbackFunction = function (engResult, oRes) {

            disableSubmit(false); //réactive le post

            var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");
            if (success) {
                var mode = getXmlTextNode(oRes.getElementsByTagName("mode")[0])
                if (mode == "MSG") {

                    var msg = getXmlTextNode(oRes.getElementsByTagName("msg")[0]);
                    var css = getXmlTextNode(oRes.getElementsByTagName("css")[0]);

                    var oDoc = document.getElementById("ContainerPanel");
                    if (oDoc) {
                        oDoc.innerHTML = msg;

                        //retire toutes les css
                        //clearHeader("ALL", "CSS");

                        //met les css approprié
                        //addCss("eFormular", "FORM");
                        //addCss("eMain", "FORM");
                        //addCss("eControl", "FORM");                  

                        //et le css custom si besoin
                        if (css.length > 0)
                            addCSSText(css);
                    }

                }
                else if (mode == "REDIRECT") {
                    var sURL = getXmlTextNode(oRes.getElementsByTagName("url")[0]);
                    top.location = sURL;
                }
            }

            _sw(false); //setWait - submitFormular fin
            _sw(false); //AAAA => Fin
        };

        //Message d'alerte spécifique pour les formulaires
        // erreur de l'appel a l'updater
        eEngineUpdater.ErrorCustomAlert = function (oObjErr, fctCallBack) {
            _sw(false);  //setWait - submitFormular fin
            _sw(false); //AAAA => Fin
            disableSubmit(false);
            
            window.scrollTo(0, 0); // #53522
            eAlert(oObjErr.Type, oObjErr.Title, oObjErr.Msg, oObjErr.DetailMsg);

            if (typeof (fctCallBack) == "function") {
                fctCallBack();
            }
        };

        //Message d'alerte spécifique pour les formulaires
        // erreur du retour de l'engine
        eEngineUpdater.ErrorCallbackFunction = function (engResult) {
            _sw(false);  //setWait - submitFormular fin       
            _sw(false); //AAAA => Fin
            disableSubmit(false);
        };

        eEngineUpdater.AddOrSetParam("action", "UPDTFORM");
        eEngineUpdater.UpdateLaunch();  //AAAA => Il y a un un set wait dedans
    }

    //
    function disableSubmit(bDisable) {
        var oSubmited = document.getElementById("sub");
        var oBtn = document.querySelector(".eButtonCtrl");

        if (oSubmited) {
            if (bDisable)
                oSubmited.value = "1";
            else
                oSubmited.value = "0";
        }

        if (oBtn) {
            if (bDisable)
                oBtn.style.backgroundColor = "#B5B5B5";
            else
                oBtn.style.backgroundColor = "";
        }
    }


    ///Initilisation
    var initParam = function () {
        if (!_isInit) {
            _nTab = document.getElementById("tab").value;
            _fileId = document.getElementById("fileId").value;
            _formularType = document.getElementById("frmType").value;
            eNumber.SetDecimalDelimiter(document.getElementById("NumberDecimalDelimiter").value);
            eNumber.SetSectionDelimiter(document.getElementById("NumberSectionsDelimiter").value);
            eDate.SetCultureInfo(document.getElementById("CultureInfo").value);
            _isInit = true;
        }   
    };
    
    //Function publiques
    return {
        //Appelé par le code pour indiquer que la fenetre d'erreur est fermée
        ErrorAlertClosed: function () {
            _errorIsDisplayed = false;
        },

        //
        CheckFieldFormat: function (oField) {
            initParam();
            if (oField == null)
                return true;
            var fldEngine = getFldEngFromElt(oField);
            if (fldEngine == null)
                return true;
            return checkFormat(fldEngine);

        },

        LaunchSubmit: function () {

            var oForm = document.getElementById("userForm");
            oForm.submit();
        },

        ///Post
        SubmitForm: function (evt) {
            _sw(true);
            try {

                initParam();

                // Les champs obligatoires sont pas encore gérés pour  les formulaire
                if (isValidForm()) {
                    submitFormular();
                }

            }
            catch (e) {
               console.log(e);
            }
            finally {

                return false;

            }
        },
        InitNewCode: function () {
            var oImgCode = document.getElementById("ImgCapcha");
            if (oImgCode)
                document.getElementById("ImgCapcha").src = "ecaptchaget.aspx?date=" + ((new Date()).getTime());
        },
        ChkLabelClick: function (elem) {
            $(elem).closest('.inputCanvas').find('a.chk').click();
        },
        InitFormularAdvCaptcha: function (site_key, lang) {
            var that = this;
            grecaptcha.render('reCaptchContainer', {
                'sitekey': site_key,
                'callback': that.ReCaptchaCallback,
                lang: lang,//Pour la langue audio
                theme: 'light', //light or dark    
                type: 'image',// image or audio    
                size: 'normal'//normal or compact    
            });
            
            //Tâche #3 081: on ajoute l'aspect obligatoire à la validation du formulaire
            grecaptcha.ready(function () {
                var $recaptcha = document.querySelector('#g-recaptcha-response');
                if ($recaptcha) {
                    $recaptcha.setAttribute("required", "required");
                }
            });
           
            
        },
        ReCaptchaCallback: function (response) {
            if (response !== '') {
                //TODO: ajouter un traitement coté front lorsque l'appel du captcha est réussi
            }
        }
    };

})();


//Représente les actions sur les champs
var eUsrFld = (function () {

    //Gestionnaire d'actions sur les champs
    var usrFldAction = function (event, trigger, sIdField) {
        var oField = document.getElementById(sIdField);
        if (!oField)
            return;

        var sAction = oField.getAttribute("eAction");

        switch (trigger) {
            case "blur":
                eUserForm.CheckFieldFormat(oField);
                break;
            case "click":
                switch (sAction) {
                    case 'LNKFREETEXT': // Champ saisie libre
                    case 'LNKFREECAT':  //catalogue saisie libre
                    case 'LNKNUM': // Edition champ numérique
                    case "LNKDATE": // Champ Date
                    case 'LNKPHONE': // Champ téléphone
                    case 'LNKMAIL': // Catalogue Adresse Mail
                    case 'LNKWEBSIT':   // Catalogue Site Web
                    case 'LNKSOCNET':   // Catalogue Reseau Social
                    case 'LNKCAT':  //catalogue simple
                    case 'LNKADVCAT':   // Catalogue avancée
                    case 'LNKCATDESC':   // Catalogue DESC
                    case 'LNKCATENUM':   // Catalogue ENUM
                        oField.focus();
                        break;
                    case 'LNKCATIMG':
                        var manager = new oImageManager(oField);
                        manager.openFileExplorer();
                        break;
                    case 'LNKOPENIMG':
                        var manager = new oImageManager(oField);
                        manager.openPrompt();
                        break;
                    case 'LNKCATUSER':  // catalogue user
                    case "LNKMNGFILE":
                    case "LNKOPENFILE":
                    case 'LNKCATFILE': // Choix fiche
                    case "LNKGOFILE":
                    case "LNKOPENMAIL":
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    };

    //Function publiques
    return {
        //Action à la sortie du champ
        OB: function (event, sIdField) {
            usrFldAction(event, "blur", sIdField);
        },
        //Action au clic
        OC: function (event, sIdField) {
            usrFldAction(event, "click", sIdField);
        },
        //Action à l'appui d'une touche
        OKU: function (event, sIdField) {
            usrFldAction(event, "keyup", sIdField);
        },
        //Action à la sélection
        OS: function (event, sIdField) {
            usrFldAction(event, "select", sIdField);
        },
    };
})();

var oCatalog = (function () {

    return {
        change: function (evt) {
            //le composant select2 renvoie un evt avec la liste des selctions dans un tableau val             
            var element = document.getElementById(getAttributeValue(evt.target, "refid"));
            var header = document.getElementById(getAttributeValue(element, "ename"));
            var bMultiple = getAttributeValue(header, "mult") == "1";
            setAttributeValue(element, "dbv", bMultiple ? joinString(";", evt.val) : evt.val);
        }
    }
}())

/// Onjet qui gére l upload des images lien/server
function oImageManager(oField) {
    var _oField = oField;
    var _fldEngine = getFldEngFromElt(oField);
    var that = this;

    function updateImageFld(attrs) {
        setAttributeValue(_oField, "dbv", attrs.dbv);
        setAttributeValue(_oField, "title", attrs.title);

        var img = _oField.querySelector("img");
        if (img != null) {

            if (attrs.src.length > 0) {
                switchClass(img, "imgEmpty", "imgFill");
                img.style.borderWidth = "0px";
            }
            else {
                switchClass(img, "imgFill", "imgEmpty");
                img.style.borderWidth = "1px";
            }

            img.src = attrs.src;
        }
    }

    return {
        openPrompt: function () {
            that = this;
            var url = _oField.getAttribute("dbv");

            var labElm = document.getElementById(getAttributeValue(_oField, "ename"));
            var label = labElm.innerHTML;

            DialogBox.init({ 'width': 450, 'title': label, 'msgTitle': _res_712 + ' (jpg, png, gif )' });
            DialogBox.onVlidate({ 'onClick': that.updateField });
            DialogBox.onCancel({ 'onClick': that.cancelEdit });
            DialogBox.showPrompt({ id: 'urlvalue', defaultValue: url });
        },
        updateField: function (evt) {

            var elm = document.getElementById("urlvalue");
            var urlValue = elm.value;

            updateImageFld({ 'src': urlValue, 'dbv': urlValue, 'title': urlValue });

            DialogBox.hide();
        },
        cancelEdit: function (evt) {
            DialogBox.hide();
        },
        openFileExplorer: function () {
            that = this;

            var btn = document.getElementById("FileToUpload");
            setEventListener(btn, "change", that.saveFile);
            btn.click();
        },
        saveFile: function (evt) {

            var src = evt.srcElement || evt.target
            if (src.id != "FileToUpload" || (src.files && src.files.length <= 0))
                return;

            setAttributeValue(_oField, "dbvtmp", src.files[0].name);

            //image a uploader
            var oFileUploader = new FileUploder(src);
            oFileUploader.init({ 'action': 'FILE_UPLOAD', 'descid': _fldEngine.descId });
            oFileUploader.setHandler({ evtName: 'success', 'handler': that.success });
            oFileUploader.setHandler({ evtName: 'error', 'handler': that.error });
            oFileUploader.upload();

            unsetEventListener(src, "change", that.saveFile);
        },
        success: function (oRes) {

            var success = (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1");
            if (success) {
                var url = getXmlTextNode(oRes.getElementsByTagName("url")[0])
                if (url != null && url.length > 0) {

                    var imgName = getAttributeValue(_oField, 'dbvtmp');
                    updateImageFld({ 'src': url, 'dbv': imgName, 'title': imgName });
                }

            } else {
                that.error(oRes);
            }
        },
        error: function (oRes) {


            //alert(oRes);
            //eAlert(oRes.Type, oRes.Title, oRes.Msg, oRes.DetailMsg);
        }
    }
}

///Objet qui gére l upload des fichiers
var FileUploder = function (src) {
    var fileBtn = src;
    var formData = {};
    var handle = {};
    var jqXHR = null;
    var that = this;
    function appendFormParam(id) {
        var elm = document.getElementById(id);
        formData.append(elm.name, elm.value);
    }
    return {
        init: function (data) {
            var wait = (top.setWait) ? top.setWait : setWait
            handle = new Array();
            handle['complete'] = function () { wait(false); };
            handle['success'] = function () { };
            handle['error'] = function () { };
            handle['beforeSend'] = function () { wait(true); };

            formData = new FormData();
            formData.append('action', data.action);
            formData.append('descid', data.descid);
            appendFormParam("tab");
            appendFormParam("fileId");
            appendFormParam("tok");
            appendFormParam("cs");
            appendFormParam("p");
        },
        setHandler: function (options) {
            if (options.evtName in handle)
                handle[options.evtName] = options.handler;
        },
        upload: function () {

            var files = fileBtn.files;
            if (files.length == 0)
                return;

            for (var i = 0 ; i < files.length; i++)
                formData.append(files[i].name, files[i]);

            //on pourrait remplacer ce truc par autre truc....ou du js natif
            jqXHR = $.ajax({
                url: "mgr/eUsrFrmManager.ashx",
                type: 'POST',
                contentType: false,
                processData: false,
                cache: false,
                complete: handle['complete'],
                success: handle['success'],
                error: handle['error'],
                beforeSend: handle['beforeSend'],
                data: formData
            });
        },
        cancel: function () {
            if (jqXHR.abort != null)
                jqXHR.abort();
        }
    }
}

// Objet modal dialog
var DialogBox = (function () {

    var size = { w: 500, h: 300 };
    var dialog = null;
    var that = this;
    function build() {

        dialog = document.getElementById("modalDialog");
        if (dialog == null) {
            var element = "<div id='modalcontent' class='modal-content' >";
            element += "  <div id='modalHeader' class='modal-header'>";
            element += "     <button id='crossBtn' type='button' class='close'><span>×</span></button>";
            element += "     <h4 id='modalTitle' class='modal-title' ></h4>";
            element += "  </div>";
            element += "  <div id='modalBody' class='modal-body' >";
            element += "       <div id='msgTitle' ></div>";
            element += "       <div id='msgBody'  class='msgBody'></div>";
            element += "  </div>";
            element += "  <div  id='modalFooter' class='modal-footer'>";
            element += "    <input   id='cancelBtn' class='hide' type='button' value='" + _res_29 + "' /> ";
            element += "    <input id='validBtn' class='hide' type='button' value='" + _res_28 + "'/>";
            element += "  </div>";
            element += "</div>";

            dialog = document.createElement("div");
            dialog.id = "modalDialog";
            dialog.innerHTML = element;
            setAttributeValue(dialog, "class", "modal-dialog");

            document.body.appendChild(dialog);
        }

        dialog.style.width = size.w + "px";
        display(false);
    }
    function setInnerHtml(id, htmlValue) {
        var elm = document.getElementById(id);
        if (elm)
            elm.innerHTML = htmlValue;
    }
    function setEvent(id, eventName, handler) {
        var elm = document.getElementById(id);
        if (elm)
            setEventListener(elm, eventName, handler, false);
    }
    function removeEvent(id, eventName) {
        try {
            var elm = document.getElementById(id);
            if (elm)
                unsetEventListener(elm, eventName);
        } catch (ex) { }
    }
    function centerDialog(options) {
        var innerdialog = document.getElementById("modalDialog");
        var docSize = getWindowSize();

        innerdialog.style.left = ((docSize.w - size.w) / 2) + "px";
        innerdialog.style.top = (docSize.h / 4) + "px"; //((docSize.h - dialogSize.h) / 3) + "px";
    }
    function display(bShow) {
        dialog.style.display = bShow ? "block" : "none";

        if (!bShow) {
            addClass(document.getElementById("validBtn"), 'hide');
            addClass(document.getElementById("cancelBtn"), 'hide');
        }
    }
    return {
        init: function (options) {
            that = this;

            if (options.width)
                size.w = options.width;

            if (options.height)
                size.h = options.height;
            build();
            setInnerHtml('modalTitle', options.title);
            setInnerHtml('msgTitle', options.msgTitle);

            setEvent("crossBtn", options.eventName, this.hide);

            return this;
        },
        setContent: function (htmlContent) {
            setInnerHtml('msgBody', htmlContent);

            return this;
        },
        onVlidate: function (options) {

            removeClass(document.getElementById("validBtn"), 'hide');
            setEvent("validBtn", "click", options.onClick);

            return this;
        },
        onCancel: function (options) {

            removeClass(document.getElementById("cancelBtn"), 'hide');
            setEvent("cancelBtn", "click", options.onClick);
            setEvent("crossBtn", "click", options.onClick);

            return this;
        },
        show: function (options) {
            centerDialog(options);
            display(true);

            return this;
        },
        showPrompt: function (options) {

            var value = "";
            if (options && options.defaultValue && options.defaultValue.length > 0)
                value = options.defaultValue;
            that.setContent('<input type="text" id="' + options.id + '" value="' + value + '" />');

            setEvent(options.id, 'focus', function (evt) { document.getElementById(options.id).select(); });
            that.show();

            return this;
        },
        hide: function () {
            display(false);

            removeEvent("validBtn", "click");
            removeEvent("cancelBtn", "click");
            removeEvent("crossBtn", "click");

            return this;
        }
    }
}());


