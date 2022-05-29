// eColorPicker
// Options disponibles : title, color, picker, pickerTxt, onHide
function eColorPicker(pickerOptions) {

    var _modalColorPicker = null;
    var _doc = document;

    // Options du picker
    pickerOptions = pickerOptions || {};
    var _title = pickerOptions.title || top._res_8132;
    var _color = pickerOptions.color || "";
    var _picker = pickerOptions.picker;
    var _pickerTxt = pickerOptions.pickerTxt;
    var _onHide = pickerOptions.onHide || null;


    // Initialisation des événements
    function createEventListeners() {

        getModalDocument();

        // Clic/double-clic sur la couleur
        var colorPickerWrapper = _doc.getElementById("pnlColors");
        if (colorPickerWrapper) {
            setEventListener(colorPickerWrapper, "click", selectColor);
            setEventListener(colorPickerWrapper, "dblclick", function (event) {
                selectColor(event);
                setPickerColor();
                _modalColorPicker.hide();
                if (typeof (_onHide) === 'function')
                    _onHide();
            });
        }

        // Modification du champ Couleur
        var colorHexa = _doc.getElementById("txtPictoColor");
        if (colorHexa) {
            setEventListener(colorHexa, "change", customColor);

        }
            
    }

    // Personnalisation de la couleur
    function customColor(evt) {

        var element = evt.srcElement || evt.target;
        _color = element.value;

        var pickedColor = _doc.getElementById("pnlDisplayedColor");
        if (pickedColor) {
            pickedColor.style.backgroundColor = _color;
        }
    }

    // Affiche la couleur sélectionnée
    function selectColor(evt) {

        var element = evt.srcElement || evt.target;
        _color = getAttributeValue(element, "title");
        
        setColor();
    }

    // Définit une nouvelle couleur
    function setColor() {
        // On met à jour la couleur affichée
        var pickedColor = _doc.getElementById("pnlDisplayedColor");
        if (pickedColor) {
            pickedColor.style.backgroundColor = _color;
        }
        // On met à jour l'hexa de la couleur et la couleur du picto sélectionné
        var txtColor = _doc.getElementById("txtPictoColor");
        if (txtColor) {
            txtColor.value = _color;
        }
    }

    // Réinitilisation de la couleur
    function resetColor() {
        _color = "";
        setColor();
    }

    // Retourne le "document" de la modale
    function getModalDocument() {
        _doc = _modalColorPicker.getIframe().document;
        return _doc;
    }

    // Rafraîchit la couleur choisie dans le picker
    function setPickerColor() {
        if (_picker) {
            _picker.style.backgroundColor = _color;
            setAttributeValue(_picker, "value", _color);
        }
            
        if (_pickerTxt) {
            _pickerTxt.value = _color;
            setAttributeValue(_pickerTxt, "value", _color);
            if (typeof (_pickerTxt.onchange) === 'function') {
                _pickerTxt.onchange();
            }
        }
        
    }

    // Fermeture de la modale
    function hide() {
        _modalColorPicker.hide();
        if (typeof (_onHide) === 'function')
            _onHide();
    }

    return {
        getSelectedColor: function () {
            return _color;
        },
        // Affichage du color picker
        show: function () {
            _modalColorPicker = new eModalDialog(_title, 0, "eColorPickerDialog.aspx", 720, 180, "modalColorPicker");
            _modalColorPicker.addParam("color", _color, "post");
            _modalColorPicker.onIframeLoadComplete = createEventListeners;
            _modalColorPicker.show();

            // Bouton "Fermer"
            _modalColorPicker.addButton(top._res_30, function () {
                hide();
            }, 'button-gray', null);
            // Bouton "Valider"
            _modalColorPicker.addButton(top._res_28, function () {
                setPickerColor();
                hide();
            }, 'button-green', null);
            // Bouton "Couleur par défaut"
            _modalColorPicker.addButton(top._res_7975, function () {
                resetColor();
                setPickerColor();
                hide();
            }, 'button-gray', null);
        }
    }
}