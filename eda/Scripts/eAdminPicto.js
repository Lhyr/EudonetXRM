

///
/// La fenêtre du selection du pictogramme
/// Si le descid de la table est rensigné, on ira chercher dans la base l'icon et la couleur
/// sinon on prend la coleur et la clé de la font ou les valeur par défaut
///
function ePictogramme(targetId, pictoOptions) {
    
    pictoOptions = pictoOptions || {};

    var _modalDocument;
    var _tab = pictoOptions.tab || 0;
    var _descid = pictoOptions.descid || 0;
    var _color = pictoOptions.color || "";
    var _iconKey = pictoOptions.iconKey || "";
    var _title = pictoOptions.title || top._res_7481; 
    var _width = pictoOptions.width || 800;
    var _height = pictoOptions.height || 600;    

    var _open = false;
    var _modalPicto = null;
    var _callback = pictoOptions.callback || function () { };
    var _targetId = targetId;
    var _picto = {};

    function hidePicto() {
        if (_open && _modalPicto != null)
            _modalPicto.hide();

        _open = false;
        _modalPicto = null;
    }

    // Mis à jour l'element ciblé 
    function updatePicto() {
       
        //var modalDocument = getModalDocument();

        var color = _modalDocument.getElementById("txtPictoColor");
        var key = _modalDocument.getElementById("hidSelectedPicto");
        var icon = _modalDocument.querySelector("#tablePicto td[data-selected='1'] span");

        _picto.key = key.value;
        _picto.color = color.value;
        _picto.className = icon.className;
        _picto.targetId = _targetId;
        // Rafraîchissement du btn
        // Attention c'est le document en cours
        var btnPicto = document.getElementById(_targetId);
        if (btnPicto) {

            btnPicto.className = _picto.className;
            btnPicto.style.color = _picto.color;

            setAttributeValue(btnPicto, "picto-key", _picto.key);
            setAttributeValue(btnPicto, "picto-color", _picto.color);
            setAttributeValue(btnPicto, "picto-class", _picto.className);            
        }  

        if (_callback)
            _callback(_picto, _descid);

        hidePicto();
    }

    /// L'iframe de la fenetre est completement chargé, on attaches des events
    function attachEvents() {
      
        _modalDocument = getModalDocument();

        // Colorpicker
        var colorPickerWrapper = _modalDocument.getElementById("pnlColors");
        if (colorPickerWrapper)
            setEventListener(colorPickerWrapper, "click", selectColor);

        // Color hexa
        var colorHexa = _modalDocument.getElementById("txtPictoColor");
        if (colorHexa) {
            setEventListener(colorHexa, "change", customColor);
         
        }
            
              
        // Picto className
        var tablePicto = _modalDocument.getElementById("tablePicto");
        if (tablePicto) {
            setEventListener(tablePicto, "click", selectPicto);
            setEventListener(tablePicto, "dblclick", selectPictoAndValidate);

            var listIcons = tablePicto.querySelectorAll("td span");
            for (var i = 0; i < listIcons.length; i++) {
                setEventListener(listIcons[i], "mouseover", hoverPicto);
                setEventListener(listIcons[i], "mouseout", mouseoutPicto);
            }
        }

        
    }

    function customColor(evt) {

        var element = evt.srcElement || evt.target;
        var color = element.value;

        var pickedColor = _modalDocument.getElementById("pnlDisplayedColor");
        if (pickedColor) {
            pickedColor.style.backgroundColor = color;
        }

        var selectedIcon = _modalDocument.querySelector("td[data-selected='1'] span");
        selectedIcon.style.color = color;
    }

    // Affcihe la colour du pictogramme
    function selectColor(evt) {
        //var modalDocument = getModalDocument();
        //var picker = modalDocument.getElementById("colorPicker");
        //var pickerText = modalDocument.getElementById("txtPictoColor");
        //_picto.color = pickerText.value;
        //pickColor(picker, pickerText, updateTablePictoColor);
        var element = evt.srcElement || evt.target;
        var color = getAttributeValue(element, "title");
     
        setColor(color);
    }

    // Définit la couleur
    function setColor(color) {
        // On met à jour la couleur affichée
        var pickedColor = _modalDocument.getElementById("pnlDisplayedColor");
        if (pickedColor) {
            pickedColor.style.backgroundColor = color;
        }
        // On met à jour l'hexa de la couleur et la couleur du picto sélectionné
        var txtColor = _modalDocument.getElementById("txtPictoColor");
        if (txtColor) {
            txtColor.value = color;
            _picto.color = color;
            var selectedIcon = _modalDocument.querySelector("td[data-selected='1'] span");
            selectedIcon.style.color = color;
        }
    }

    // Mis à jour de la coleur de la table
    //function updateTablePictoColor() {
    //    var modalDocument = getModalDocument();
    //    var element = modalDocument.getElementById("txtPictoColor");
    //    var tablePicto = modalDocument.getElementById("tablePicto");
    //    if (tablePicto && element && element.id == "txtPictoColor") {
    //        tablePicto.style.color = element.value;         
    //    }
    //}

    // Au survol du picto, on affecte la couleur sélectionnée au picto
    function hoverPicto(evt) {
        var elementPicto = evt.srcElement || evt.target;

        var txtColor = _modalDocument.getElementById("txtPictoColor");

        elementPicto.style.color = txtColor.value;
    }

    // Lorsque le picto n'est plus survolé, on retire la couleur sélectionnée
    function mouseoutPicto(evt) {
        var elementPicto = evt.srcElement || evt.target;
        // Si le picto n'est pas sélectionné, on retire la couleur
        if (getAttributeValue(elementPicto.parentElement, "data-selected") != "1")
            elementPicto.style.color = "";
    }

    // Mis à jour la selection du picto
    function selectPicto(evt) {
        var elementPicto = evt.srcElement || evt.target;
        var key = getAttributeValue(elementPicto, "pictokey");
        if (!elementPicto || elementPicto == null || elementPicto.tagName != "SPAN" || key == null || key == "")
            return false; 

        //var modalDocument = getModalDocument();       
        var hidSelectedPicto = _modalDocument.getElementById("hidSelectedPicto");
        hidSelectedPicto.value = key;

        var iconParent = _modalDocument.querySelector("#tablePicto td[data-selected='1']");
        setAttributeValue(iconParent, "data-selected", "0");
        iconParent.firstChild.style.color = "";
        setAttributeValue(elementPicto.parentElement, "data-selected", "1");

        var txtColor = _modalDocument.getElementById("txtPictoColor");
        if (txtColor) {
            elementPicto.style.color = txtColor.value;
        }
        
        return true;
    };

    // Mis a jour de la selection et la valide
    function selectPictoAndValidate(evt) {
        if (selectPicto(evt))
            updatePicto();
    };

    function resetColor() {
        setColor("");
    }


    function getModalDocument()
    {        
        //var modalContent = modal.contentDocument || modal.contentWindow.document;
        return _modalPicto.getIframe().document;
    }

    return {
        picto: function () { return _picto; },
        hide: function () {
            hidePicto();
        },
        show: function () {
            if (_open)
                return;

            _modalPicto = new eModalDialog(_title, 0, "eda/eAdminPictoDialog.aspx", _width, _height, "modalAdminPicto");

            _modalPicto.noButtons = false;
            _modalPicto.addParam("iframeScrolling", "yes", "post");
            _modalPicto.addParam("tab", _tab, "post");
            _modalPicto.addParam("descid", _descid, "post");
            _modalPicto.addParam("color", _color, "post");
            _modalPicto.addParam("iconkey", _iconKey, "post");
            _modalPicto.onIframeLoadComplete = attachEvents;
            _modalPicto.show();
            _modalPicto.addButton(top._res_30, function () { hidePicto(); }, 'button-gray', null);
            _modalPicto.addButton(top._res_28, function () { updatePicto(); }, 'button-green', null);
            _modalPicto.addButton(top._res_7975, function () { resetColor(); }, 'button-gray', null);
            _open = true;
        },
    }
}