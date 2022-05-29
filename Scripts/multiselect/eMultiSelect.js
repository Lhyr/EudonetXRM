



/// Objet permettant d'ouvrir la fenetre de selection de plusieurs valeurs 
/// sans tenir compte de la source de données
/// Il dialogue avec l'objet oMultiSelectInternal de l'iframe eMultiSelect.aspx
var eMultiSelect = function (multiSelectType, multiSelectOptions) {

    // Type définit de la sélection multiple
    var _msType = multiSelectType;

    // Options de la sélection multiple
    var _msOptions = setDefault(multiSelectOptions);

    // Objet interne à l'iframe qui permet de récupere les valeurs selectionnées
    var _oInternalMultiSelect = null;

    var _modal = null;
    var _params = null;
    var _groupSep = ""

    // TODO Valeurs par défaut si non fournies
    function setDefault(options) {
        var obj = {};
        obj = options;
        obj.validateRes = options.validateRes || top._res_28;
        obj.cancelRes = options.cancelRes || top._res_29;
        obj.groupSep = options.groupSep || ";";
        obj.valueSep = options.valueSep || ":";
        obj.title = options.title || "";
        obj.size = options.size || { width: 800, height: 530 };
        obj.autoClose = options.autoClose || false;
        obj.validate = function () { };
        obj.cancel = function () { };

        return obj;
    }

    // Constructeur de l'objet
    function init() {
        _modal = new eModalDialog(_msOptions.title, 0, "eMultiSelect.aspx", _msOptions.size.width, _msOptions.size.height);

        _modal.addParam("type", _msType, "post");
        _modal.addParam("width", _msOptions.size.width, "post");
        _modal.addParam("height", _msOptions.size.height, "post");

        _modal.ErrorCallBack = error;

        // Une l'iframe est chargée on récupère la référence l'objet interne de MultiSelect.aspx
        _modal.onIframeLoadComplete = function () { _oInternalMultiSelect = _modal.getIframe().oMultiSelectInternal; }

        _params = new Array();
    }

    // Ferme la modal
    function dispose() {
        if (_modal != null)
            _modal.hide();

        _modal = null;
        _params = null;
    }

    // Erreur de chargement
    function error() {
        _msOptions.cancel({ 'success': '0', 'error': top._res_8651});
    }

    function show() {

        _modal.addParam("param", _params, "post");

        _modal.show();
        _modal.addButton(_msOptions.cancelRes, function () { cancel(); }, "button-gray");
        _modal.addButton(_msOptions.validateRes, function () { validate(); }, "button-green");
    }

    // Clique sur la croix ou annuler
    function cancel() {
        _msOptions.cancel({ 'success': '1' });

        dispose();
    }

    // Validation de la sélection
    function validate() {
        _msOptions.validate({ 'success': '1', 'data': _oInternalMultiSelect.GetSelectedItems() });

        if (_msOptions.autoClose)
            dispose();
    }

    // Initilisation de l'objet
    init();

    // fonctions publiques
    return {
        setParam: function (key, value) { _params += key + _msOptions.valueSep + value + _msOptions.groupSep; },
        onValidate: function (callback) { _msOptions.validate = callback; },
        onCancel: function (callback) { _msOptions.cancel = callback; },
        show: function () { show(); },
        close: function () { dispose(); }
    };
}

///
///Correspondance avec l'enum c#
///
eMultiSelect.eType = {
    'Empty': 0,
    'Widget': 1,
}