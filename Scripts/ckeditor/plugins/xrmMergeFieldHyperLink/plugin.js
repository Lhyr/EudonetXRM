/*
Plugin CKEditor Eudonet XRM pour l'insertion d'un lien via un champ de fusion
CREATION : KJE|SHA le 18/02/2021
*/

var LinkCat =
{
    URL: "url",       //URL
    MERGEHYPERLINK: "mergehyperlink", // MERGEHYPERLINK
    ANCHOR: "anchor",       //Ancre
    EMAIL: "email",       //Email
    VISU: "visu",       //VISUALISATION
    UNSUB: "unsub",     //DESINCRIPTION
    FORMU: "formu"      //FORMULAIRE
};

var ArrayTrack = new Array();
var ArrayMergeFields = new Array();

CKEDITOR.plugins.add(
    'xrmMergeFieldHyperLink',
    {
        init: function (editor) {

            // Transmission de contexte entre CKEditor et grapesjs
            if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
                editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
                if (!editor.lang)
                    editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
            }

            //pour récupérer les champs tracking et les valeurs par défaut des options
            var _oXrmMergeFieldHyperLink, _oXrmLinkAdapter = null;

            if (editor.xrmMemoEditor && editor.xrmMemoEditor.oMergeHyperLinkFields)
                _oXrmMergeFieldHyperLink = editor.xrmMemoEditor.oMergeHyperLinkFields;
            else
                return;

            if (editor.xrmMemoEditor && editor.xrmMemoEditor.oTracking)
                _oXrmLinkAdapter = editor.xrmMemoEditor.oTracking;


            CKEDITOR.on('dialogDefinition', function (ev) {
                var dialogName = ev.data.name;
                var dialogDefinition = ev.data.definition;
                if (dialogName == 'link') {

                    //Champs de fusion
                    ArrayMergeFields = [
                        {
                            type: 'select',
                            attr: 'ednl',
                            id: 'xrmMergeFieldLink',
                            label: editor.lang.xrmMergeField,//'Champ de fusion'
                            items: _oXrmMergeFieldHyperLink.fields,
                            'default': _oXrmMergeFieldHyperLink.link.ednl,
                            onChange: function (api) { },
                            setup: function (data) {
                                var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                if (link) {
                                    if (link.$.attributes["ednl"]) {
                                        this.setValue(link.$.attributes["ednl"].value);
                                        if (link.$.attributes["ednl"].value && link.$.attributes["ednl"].value > 0)
                                            this.getDialog().getContentElement('info', 'linkType').setValue('mergehyperlink')
                                    }
                                }
                            },
                            commit: function (data) {
                                if (typeof (data.advanced) == 'undefined')
                                    data.advanced = {};
                                if (typeof (data.target) == 'undefined')
                                    data.target = {};
                                if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Track désactivé pour VISU et UNSUB mais pas pour formu et autres
                                    data.advanced[this.id] = { attr: this.attr, val: "" };

                                data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                            }
                        }
                    ];

                    /*Options de trackings*/
                    ArrayTrack = _oXrmLinkAdapter ? [
                        {
                            type: 'checkbox',
                            attr: 'ednt',
                            label: editor.lang.xrmDisableTrackingLink,// 'Désactiver le lien tracking'
                            id: 'xrmTrackDisabled',
                            'default': _oXrmMergeFieldHyperLink.link.ednt != 'on' ? 'checked' : '',
                            validate: function () { /*pas de logique de vérification de validation*/ },
                            setup: function (data) {
                                var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                if (link) {
                                    if (link.$.attributes["ednt"])
                                        this.setValue(link.$.attributes["ednt"].value != 'on' ? 'checked' : '');
                                }
                            },
                            commit: function (data) {

                                if (typeof (data.advanced) == 'undefined')
                                    data.advanced = {};

                                //si visu ou unsub on commite une valeur vide
                                if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Tracking désactivé pour VISU et UNSUB mais pas pour formu et autres
                                    data.advanced[this.id] = { attr: this.attr, val: "" };
                                else
                                    data.advanced[this.id] = { attr: this.attr, val: this.getValue() ? 'off' : 'on' };
                            }
                        },
                        {
                            type: 'text',
                            attr: 'ednn',
                            label: editor.lang.xrmLinkName,// 'Nom du lien'
                            id: 'xrmTrackLinkName',
                            'default': _oXrmMergeFieldHyperLink.link.ednn,
                            validate: function () { /*pas de logique de vérification de validation*/ },
                            setup: function (data) {
                                var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                if (link) {
                                    if (link.$.attributes["ednn"])
                                        this.setValue(link.$.attributes["ednn"].value);
                                }
                            },
                            commit: function (data) {

                                if (typeof (data.advanced) == 'undefined')
                                    data.advanced = {};

                                if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Tracking désactivé pour VISU et UNSUB mais pas pour formu et autres
                                    data.advanced[this.id] = { attr: this.attr, val: "" };
                                else
                                    data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                            }
                        },
                        {
                            type: 'select',
                            attr: 'ednd',
                            id: 'xrmTrackLink',
                            label: editor.lang.xrmTrackingField,//'Champ du lien'
                            items: _oXrmLinkAdapter.fields,
                            'default': _oXrmLinkAdapter.link.ednd,
                            onChange: function (api) {
                            },
                            setup: function (data) {
                                var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                if (link) {
                                    if (link.$.attributes["ednd"])
                                        this.setValue(link.$.attributes["ednd"].value);
                                }

                            },
                            commit: function (data) {

                                if (typeof (data.advanced) == 'undefined')
                                    data.advanced = {};
                                if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Tracking désactivé pour VISU et UNSUB mais pas pour formu et autres
                                    data.advanced[this.id] = { attr: this.attr, val: "" };
                                else
                                    data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                            }
                        },
                        {
                            type: 'text',
                            attr: 'ednc',
                            id: 'xrmTrackType',
                            hidden: true,
                            'default': _oXrmMergeFieldHyperLink.link.ednc,
                            validate: function () { },
                            setup: function (data) {
                                var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                if (link) {
                                    //*On affiche l'option Formulaire seulement si l'attribut edne contenant l'id du formulaire est renseigné
                                    var linkType = this.getDialog().getContentElement('info', 'linkType');
                                    var olinkType = linkType.getInputElement().$;
                                    var valueForm = [editor.lang.xrmLinkFormular, LinkCat.FORMU];
                                    if (link.$.attributes["edne"]) {
                                        if (OptionsIndexOfArray(linkType.items, valueForm) < 0) {
                                            olinkType.options[olinkType.options.length] = new Option(editor.lang.xrmLinkFormular, LinkCat.FORMU);
                                            linkType.items.push(valueForm);
                                        }
                                    }
                                    else {
                                        var idx = OptionsIndexOfArray(linkType.items, valueForm);
                                        if (idx >= 0) {
                                            olinkType.options[idx].remove();
                                            linkType.items.splice(idx);
                                        }
                                    }
                                    //**************************************************************************
                                    if (link.$.attributes["ednc"])
                                        this.setValue(link.$.attributes["ednc"].value);
                                }
                                if (this.getValue() == LinkCat.VISU || this.getValue() == LinkCat.UNSUB || this.getValue() == LinkCat.FORMU) {  //Catégorie custom pour visu, unsub et formu.
                                    this.getDialog().getContentElement('info', 'linkType').setValue(this.getValue());
                                }
                                else if (link && link.$.attributes["ednl"] && link.$.attributes["ednl"].value > 0)
                                    this.getDialog().getContentElement('info', 'linkType').setValue('mergehyperlink');
                            },
                            commit: function (data) {
                                if (typeof (data.advanced) == 'undefined')
                                    data.advanced = {};
                                if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB || data.type == LinkCat.FORMU) {    //Valeurs custom de Catégorie custom pour visu, unsub et formu.
                                    this.setValue(data.type);
                                    data.type = "anchor";
                                }
                                else
                                    this.setValue("lnk");

                                data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                            }
                        }
                    ] : [];
                    /*************************************************************************************************/

                    //Compare une liste d'Array et un Array, et retourne la position de l'arrays dans la liste d'array si elle est présente sinon -1
                    var OptionsIndexOfArray = function (items, keyArray) {
                        var bIsFound = false;
                        for (var i = 0; i < items.length; i++) {
                            bIsFound = true;
                            for (var j = 0; j < keyArray.length; keyArray++) {
                                if (items[i][j] != keyArray[j]) {
                                    bIsFound = false;
                                    break;
                                }
                            }
                            if (bIsFound)
                                return i;
                        }
                        return -1;
                    }

                    // Get a reference to the "Link Info" tab.
                    var infoTab = dialogDefinition.getContents('info');
                    var linkTypeItems = infoTab.get("linkType").items;

                    //SPH 02/07/2015 : modification dans eMemoEditor.js pour ne plus se baser sur les index mais sur le type de l'option - on peut donc retirer des options/changer l'ordre

                    var valueMergeField = [editor.lang.xrmMergeField, LinkCat.MERGEHYPERLINK];
                    if (OptionsIndexOfArray(linkTypeItems, valueMergeField) < 0)
                        linkTypeItems.splice(1, 0, valueMergeField);

                    if (!editor.xrmMemoEditor.externalTrackingEnabled) {
                        var valueVisu = [editor.lang.xrmLinkVisuOption, LinkCat.VISU];
                        if (OptionsIndexOfArray(linkTypeItems, valueVisu) < 0)
                            linkTypeItems.push(valueVisu);
                    }

                    var valueUnsub = [editor.lang.xrmLinkUnsubOption, LinkCat.UNSUB];
                    if (OptionsIndexOfArray(linkTypeItems, valueUnsub) < 0)
                        linkTypeItems.push(valueUnsub);

                    //On ajoute des elements html à la tab info de la fenêtre ckeditor
                    var newOptions = {
                        type: "vbox",
                        id: "xrmOptionsContainer",
                        children: [{
                            type: "vbox",
                            id: "xrmOptions",
                            children: ArrayMergeFields.concat(ArrayTrack),
                            setup: function (data) {
                                var oDialog = this.getDialog();
                                //on n'affiche ces éléments qu'en cas du select 'linktype' est affiché
                                if (!this.getDialog().getContentElement('info', 'linkType')) {
                                    this.getElement().show();
                                }
                                oDialog.getContentElement('info', 'linkType').on("change",
                                    function (evt) {
                                        CKEDITOR.ManageTrackingButtons(oDialog, evt.data.value);
                                    });
                            }

                        }]
                    };

                    var existingOptions = infoTab ? infoTab.get("xrmOptionsContainer") : null;

                    if (existingOptions)
                        infoTab.remove("xrmOptionsContainer");

                    if (infoTab)
                        infoTab.add(newOptions);

                    infoTab.get("protocol").default = 'https://';

                    //GCH #33296 : Lors du clic sur le lien de visu/unsub, désormais il n'y a plus les options de tracking
                    this.ManageTrackingButtons = function (oDialog, selectedValue) {
                        var monElem = oDialog.getContentElement('info', 'xrmOptions');
                        var mergeFieldLinkElem = oDialog.getContentElement('info', 'xrmMergeFieldLink');
                        if (selectedValue == LinkCat.FORMU || selectedValue == LinkCat.URL || selectedValue == LinkCat.MERGEHYPERLINK) {
                            monElem.enable();
                            monElem.getElement().show();
                        }
                        else {
                            monElem.disable();
                            monElem.getElement().hide();
                        }

                        if (selectedValue == LinkCat.MERGEHYPERLINK) {
                            oDialog.showPage('target');
                            mergeFieldLinkElem.enable();
                            mergeFieldLinkElem.getElement().show();
                        }

                        else if (selectedValue == LinkCat.URL || selectedValue == LinkCat.FORMU) {
                            mergeFieldLinkElem.disable();
                            mergeFieldLinkElem.getElement().hide();
                        }
                    }


                    //On redéfinit la fonction onOk de la fenêtre pour insérer les attributs xrm (ednn, ednc, ednt, ednd, ednl)
                    dialogDefinition.onOk = function () {
                        var attributes = {},
                            removeAttributes = [],
                            data = {},
                            me = this,
                            editor = this.getParentEditor();

                        this.commitContent(data);

                        // Compose the URL
                        switch (data.type || 'mergehyperlink') {
                            case 'url':
                                var protocol = (data.url && data.url.protocol != undefined) ? data.url.protocol : 'http://',
                                    url = (data.url && CKEDITOR.tools.trim(data.url.url)) || '';
                                attributes['data-cke-saved-href'] = (url.indexOf('/') === 0) ? url : protocol + url;
                                break;
                            case 'mergehyperlink':
                                attributes['data-cke-saved-href'] = 'ednl';
                                break;
                            case 'anchor':
                                var name = (data.anchor && data.anchor.name),
                                    id = (data.anchor && data.anchor.id);
                                attributes['data-cke-saved-href'] = '#' + (name || id || '');
                                break;
                            case 'email':

                                var linkHref,
                                    email = data.email,
                                    address = email.address,
                                    emailProtection = ''    //Variable manquante même dans l'exemple donné sur le net :/
                                    ;

                                switch (emailProtection) {
                                    case '':
                                    case 'encode':
                                        {
                                            var subject = encodeURIComponent(email.subject || ''),
                                                body = encodeURIComponent(email.body || '');

                                            // Build the e-mail parameters first.
                                            var argList = [];
                                            subject && argList.push('subject=' + subject);
                                            body && argList.push('body=' + body);
                                            argList = argList.length ? '?' + argList.join('&') : '';

                                            if (emailProtection == 'encode') {
                                                linkHref = ['javascript:void(location.href=\'mailto:\'+',
                                                    protectEmailAddressAsEncodedString(address)];
                                                // parameters are optional.
                                                argList && linkHref.push('+\'', escapeSingleQuote(argList), '\'');

                                                linkHref.push(')');
                                            }
                                            else
                                                linkHref = ['mailto:', address, argList];

                                            break;
                                        }
                                    default:
                                        {
                                            // Separating name and domain.
                                            var nameAndDomain = address.split('@', 2);
                                            email.name = nameAndDomain[0];
                                            email.domain = nameAndDomain[1];

                                            linkHref = ['javascript:', protectEmailLinkAsFunction(email)];
                                        }
                                }

                                attributes['data-cke-saved-href'] = linkHref.join('');
                                break;
                        }

                        // Popups and target.
                        if (data.target) {
                            if (data.target.type == 'popup') {
                                var onclickList = ['window.open(this.href, \'',
                                    data.target.name || '', '\', \''];
                                var featureList = ['resizable', 'status', 'location', 'toolbar', 'menubar', 'fullscreen',
                                    'scrollbars', 'dependent'];
                                var featureLength = featureList.length;
                                var addFeature = function (featureName) {
                                    if (data.target[featureName])
                                        featureList.push(featureName + '=' + data.target[featureName]);
                                };

                                for (var i = 0; i < featureLength; i++)
                                    featureList[i] = featureList[i] + (data.target[featureList[i]] ? '=yes' : '=no');
                                addFeature('width');
                                addFeature('left');
                                addFeature('height');
                                addFeature('top');

                                onclickList.push(featureList.join(','), '\'); return false;');
                                attributes['data-cke-pa-onclick'] = onclickList.join('');

                                // Add the "target" attribute. (#5074)
                                removeAttributes.push('target');
                            }
                            else {
                                if (data.target.type != 'notSet' && data.target.name)
                                    attributes.target = data.target.name;
                                else
                                    removeAttributes.push('target');

                                removeAttributes.push('data-cke-pa-onclick', 'onclick');
                            }
                        }

                        // Advanced attributes.
                        if (data.advanced) {
                            var advAttr = function (inputName, attrName) {
                                var value = data.advanced[inputName];
                                if (value)
                                    attributes[attrName] = value;
                                else
                                    removeAttributes.push(attrName);
                            };

                            advAttr('advId', 'id');
                            advAttr('advLangDir', 'dir');
                            advAttr('advAccessKey', 'accessKey');

                            if (data.advanced['advName'])
                                attributes['name'] = attributes['data-cke-saved-name'] = data.advanced['advName'];
                            else
                                removeAttributes = removeAttributes.concat(['data-cke-saved-name', 'name']);

                            advAttr('advLangCode', 'lang');
                            advAttr('advTabIndex', 'tabindex');
                            advAttr('advTitle', 'title');
                            advAttr('advContentType', 'type');
                            advAttr('advCSSClasses', 'class');
                            advAttr('advCharset', 'charset');
                            advAttr('advStyles', 'style');
                            advAttr('advRel', 'rel');


                            //CUSTOM ATTRIBUTES ***********************************************************************
                            // Advanced xrm attributes.	   
                            if (data.advanced['xrmTrackDisabled'])
                                attributes[data.advanced['xrmTrackDisabled'].attr] = data.advanced['xrmTrackDisabled'].val;
                            if (data.advanced['xrmTrackLinkName'])
                                attributes[data.advanced['xrmTrackLinkName'].attr] = data.advanced['xrmTrackLinkName'].val;
                            if (data.advanced['xrmTrackLink'])
                                attributes[data.advanced['xrmTrackLink'].attr] = data.advanced['xrmTrackLink'].val;
                            if (data.advanced['xrmTrackType'])
                                attributes[data.advanced['xrmTrackType'].attr] = data.advanced['xrmTrackType'].val;
                            if (data.advanced['xrmMergeFieldLink'])
                                attributes[data.advanced['xrmMergeFieldLink'].attr] = data.advanced['xrmMergeFieldLink'].val;
                            //****************************************************************************************************************
                        }


                        var selection = editor.getSelection();

                        // Browser need the "href" from copy/paste link to work. (#6641)
                        attributes.href = attributes['data-cke-saved-href'];




                        if (!this._.selectedElement) {
                            // Create element if current selection is collapsed.
                            var ranges = selection.getRanges(true);
                            if (ranges.length == 1 && ranges[0].collapsed) {
                                // Short mailto link text view (#5736).
                                var text = new CKEDITOR.dom.text(data.type == 'email' ?
                                    data.email.address : attributes['data-cke-saved-href'], editor.document);

                                if (text.$.data == "#" || data.type == 'mergehyperlink')
                                    text.$.data = top._res_1500; // "Lien"
                                
                                ranges[0].insertNode(text);
                                ranges[0].selectNodeContents(text);
                                selection.selectRanges(ranges);
                            }

                            // Apply style.
                            var style = new CKEDITOR.style({ element: 'a', attributes: attributes });
                            style.type = CKEDITOR.STYLE_INLINE;		// need to override... dunno why.
                            //style.apply(editor.document);
                            editor.applyStyle(style);

                        }
                        else {
                            // We're only editing an existing link, so just overwrite the attributes.
                            var element = this._.selectedElement,
                                href = element.data('cke-saved-href'),
                                textView = element.getHtml();

                            element.setAttributes(attributes);
                            element.removeAttributes(removeAttributes);

                            if (data.advanced && data.advanced.advName && CKEDITOR.plugins.link.synAnchorSelector)
                                element.addClass(element.getChildCount() ? 'cke_anchor' : 'cke_anchor_empty');

                            // Update text view when user changes protocol (#4612).
                            if (href == textView || data.type == 'email' && textView.indexOf('@') != -1) {
                                // Short mailto link text view (#5736).
                                element.setHtml(data.type == 'email' ?
                                    data.email.address : attributes['data-cke-saved-href']);
                            }

                            selection.selectElement(element);
                            delete this._.selectedElement;
                        }
                    }
                }
            });

            editor.addCommand(
                'xrmMergeFieldHyperLink',
                CKEDITOR.plugins.xrmMergeFieldHyperLink
            );

            editor.ui.addButton && editor.ui.addButton(
                'xrmMergeFieldHyperLink',
                {
                    label: editor.lang.xrmMergeFieldHyperLink,
                    command: 'xrmMergeFieldHyperLink',
                    icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/inserer-lien-champs-fusion.png'
                }
            );
        }
    }
);

CKEDITOR.plugins.xrmMergeFieldHyperLink = {
    exec: function (editor) {
        if (editor.xrmMemoEditor) {
            editor.xrmMemoEditor.mergeFieldHyperLink(editor); // appel de la fonction de eMemoEditor.js
        }
    },
    canUndo: true
}