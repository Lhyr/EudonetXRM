/*
Plugin CKEditor Eudonet XRM pour l'insertion des lien de tracking
CREATION : MOU3 11/12/2013
*/

var LinkCat =
{
	URL: "url",       //URL
	MERGEHYPERLINK: "mergehyperlink", // MERGEHYPERLINK
    ANCHOR: "anchor",       //encre
    EMAIL: "email",       //Email
    VISU: "visu",       //VISUALISATION
    UNSUB: "unsub",     //DESINCRIPTION
    FORMU: "formu"      //FORMULAIRE
};

var ArrayTrack = new Array();

CKEDITOR.plugins.add(
	'xrmLinkAdapter',
	{
	    init: function (editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        // When opening a dialog, its "definition" is created for it, for
	        // each editor instance. The "dialogDefinition" event is then
	        // fired. We should use this event to make customizations to the
	        // definition of existing dialogs.

	        //pour récuperer les champ tracking et les valeur par defaut des options
	        var _oXrmLinkAdapter = null;

	        if (editor.xrmMemoEditor && editor.xrmMemoEditor.oTracking)
	            _oXrmLinkAdapter = editor.xrmMemoEditor.oTracking;
	        else
	            //On a rien à faire ici... :(
	            return;




	        CKEDITOR.on('dialogDefinition', function (ev) {
	            // Take the dialog name and its definition from the event
	            // data.
	            var dialogName = ev.data.name;
	            var dialogDefinition = ev.data.definition;

	            // Check if the definition is from the dialog we're
	            // interested on (the "Link" dialog).
	            if (dialogName == 'link') {

	                /*************************************************************************************************/
	                /*Options de trackings*/
	                ArrayTrack = [
                            {
                                type: 'checkbox',
                                attr: 'ednt',
                                label: editor.lang.xrmDisableTrackingLink,// 'Désactiver le lien tracking'
                                id: 'xrmTrackDisabled',
                                'default': _oXrmLinkAdapter.link.ednt != 'on' ? 'checked' : '',
                                validate: function () { /*pas de logique de vérification de validation*/ },
                                setup: function (data) {
                                    //si selection on recupere la valeur de l attribut 
									// Backlog #260, #318, #319, 428 - L'instance représentée par la variable editor est celle correspondant à la dernière instance de CKEditor créée sur la page.
									// Ceci, car la (re)définition de la boîte de dialogue se fait sur toutes les instances de CKEditor présentes sur la page, par ordre de création/instanciation,
									// et uniquement la première fois que la fenêtre est affichée pour une instance de CKEditor.
									// De ce fait, editor semble toujours représenter la dernière instance de CKEditor créée sur la page, dans tous les cas.
									// On utilisera donc ici la variable superglobale CKEDITOR.currentInstance pour effectuer le ciblage sur l'instance actuellement sélectionnée/focusée
									// Ce qui a d'autant plus de sens, que les options de la boîte de dialogue Lien doivent ici refléter le lien sur lequel on a cliqué, et donc, sur l'instance
									// de CKEditor disposant du focus
									// Cette variable currentInstance étant renseignée par le noyau lors du déclenchement du focus sur l'éditeur (cf. core de ckeditor.js),
									// elle n'est pas censée être null, mais certains tests ont montré qu'elle pouvait quand même l'être : https://dev.ckeditor.com/ticket/7509
									// On sélectionne alors la variable editor du contexte du plugin à défaut dans ce cas
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
                                    if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Track désactivé pour VISU et UNSUB mais pas pour formu et autres
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
                               'default': _oXrmLinkAdapter.link.ednn,
                               validate: function () { /*pas de logique de vérification de validation*/ },
                               setup: function (data) {
                                   //si selection on recupere la valeur de l attribut 
									// Backlog #260, #318, #319, 428 - L'instance représentée par la variable editor est celle correspondant à la dernière instance de CKEditor créée sur la page.
									// Ceci, car la (re)définition de la boîte de dialogue se fait sur toutes les instances de CKEditor présentes sur la page, par ordre de création/instanciation,
									// et uniquement la première fois que la fenêtre est affichée pour une instance de CKEditor.
									// De ce fait, editor semble toujours représenter la dernière instance de CKEditor créée sur la page, dans tous les cas.
									// On utilisera donc ici la variable superglobale CKEDITOR.currentInstance pour effectuer le ciblage sur l'instance actuellement sélectionnée/focusée
									// Ce qui a d'autant plus de sens, que les options de la boîte de dialogue Lien doivent ici refléter le lien sur lequel on a cliqué, et donc, sur l'instance
									// de CKEditor disposant du focus
									// Cette variable currentInstance étant renseignée par le noyau lors du déclenchement du focus sur l'éditeur (cf. core de ckeditor.js),
									// elle n'est pas censée être null, mais certains tests ont montré qu'elle pouvait quand même l'être : https://dev.ckeditor.com/ticket/7509
									// On sélectionne alors la variable editor du contexte du plugin à défaut dans ce cas
                                    var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
									if (link) {
										if (link.$.attributes["ednn"])
											this.setValue(link.$.attributes["ednn"].value);
									}
                               },
                               commit: function (data) {

                                   if (typeof (data.advanced) == 'undefined')
                                       data.advanced = {};

                                   if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Track désactivé pour VISU et UNSUB mais pas pour formu et autres
                                       data.advanced[this.id] = { attr: this.attr, val: "" };
                                   else
                                       data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                               }
                           },
                            {
                                type: 'select',
                                attr: 'ednd',
                                id: 'xrmTrackLink',
                                label: editor.lang.xrmTrackingField,//'Champ de tracking'
                                items: _oXrmLinkAdapter.fields,
                                'default': _oXrmLinkAdapter.link.ednd,
                                onChange: function (api) {
                                },
                                setup: function (data) {
                                    //si selection on recupere la valeur de l attribut 
									// Backlog #260, #318, #319, 428 - L'instance représentée par la variable editor est celle correspondant à la dernière instance de CKEditor créée sur la page.
									// Ceci, car la (re)définition de la boîte de dialogue se fait sur toutes les instances de CKEditor présentes sur la page, par ordre de création/instanciation,
									// et uniquement la première fois que la fenêtre est affichée pour une instance de CKEditor.
									// De ce fait, editor semble toujours représenter la dernière instance de CKEditor créée sur la page, dans tous les cas.
									// On utilisera donc ici la variable superglobale CKEDITOR.currentInstance pour effectuer le ciblage sur l'instance actuellement sélectionnée/focusée
									// Ce qui a d'autant plus de sens, que les options de la boîte de dialogue Lien doivent ici refléter le lien sur lequel on a cliqué, et donc, sur l'instance
									// de CKEditor disposant du focus
									// Cette variable currentInstance étant renseignée par le noyau lors du déclenchement du focus sur l'éditeur (cf. core de ckeditor.js),
									// elle n'est pas censée être null, mais certains tests ont montré qu'elle pouvait quand même l'être : https://dev.ckeditor.com/ticket/7509
									// On sélectionne alors la variable editor du contexte du plugin à défaut dans ce cas
                                    var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);
                                    if (link) {
                                        if (link.$.attributes["ednd"])
                                            this.setValue(link.$.attributes["ednd"].value);
                                    }

                                },
                                commit: function (data) {

                                    if (typeof (data.advanced) == 'undefined')
                                        data.advanced = {};
                                    if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB)    //Track désactivé pour VISU et UNSUB mais pas pour formu et autres
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
                                'default': _oXrmLinkAdapter.link.ednc,
                                validate: function () { },
                                setup: function (data) {
									// Backlog #260, #318, #319, 428 - L'instance représentée par la variable editor est celle correspondant à la dernière instance de CKEditor créée sur la page.
									// Ceci, car la (re)définition de la boîte de dialogue se fait sur toutes les instances de CKEditor présentes sur la page, par ordre de création/instanciation,
									// et uniquement la première fois que la fenêtre est affichée pour une instance de CKEditor.
									// De ce fait, editor semble toujours représenter la dernière instance de CKEditor créée sur la page, dans tous les cas.
									// On utilisera donc ici la variable superglobale CKEDITOR.currentInstance pour effectuer le ciblage sur l'instance actuellement sélectionnée/focusée
									// Ce qui a d'autant plus de sens, que les options de la boîte de dialogue Lien doivent ici refléter le lien sur lequel on a cliqué, et donc, sur l'instance
									// de CKEditor disposant du focus
									// Cette variable currentInstance étant renseignée par le noyau lors du déclenchement du focus sur l'éditeur (cf. core de ckeditor.js),
									// elle n'est pas censée être null, mais certains tests ont montré qu'elle pouvait quand même l'être : https://dev.ckeditor.com/ticket/7509
									// On sélectionne alors la variable editor du contexte du plugin à défaut dans ce cas
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

                                },
                                commit: function (data) {

									// Backlog #260, #318, #319, 428 - L'instance représentée par la variable editor est celle correspondant à la dernière instance de CKEditor créée sur la page.
									// Ceci, car la (re)définition de la boîte de dialogue se fait sur toutes les instances de CKEditor présentes sur la page, par ordre de création/instanciation,
									// et uniquement la première fois que la fenêtre est affichée pour une instance de CKEditor.
									// De ce fait, editor semble toujours représenter la dernière instance de CKEditor créée sur la page, dans tous les cas.
									// On utilisera donc ici la variable superglobale CKEDITOR.currentInstance pour effectuer le ciblage sur l'instance actuellement sélectionnée/focusée
									// Ce qui a d'autant plus de sens, que les options de la boîte de dialogue Lien doivent ici refléter le lien sur lequel on a cliqué, et donc, sur l'instance
									// de CKEditor disposant du focus
									// Cette variable currentInstance étant renseignée par le noyau lors du déclenchement du focus sur l'éditeur (cf. core de ckeditor.js),
									// elle n'est pas censée être null, mais certains tests ont montré qu'elle pouvait quand même l'être : https://dev.ckeditor.com/ticket/7509
									// On sélectionne alors la variable editor du contexte du plugin à défaut dans ce cas
                                    var link = CKEDITOR.plugins.link.getSelectedLink(CKEDITOR.currentInstance || editor);

                                    if (typeof (data.advanced) == 'undefined')
                                        data.advanced = {};
                                    if (data.type == LinkCat.VISU || data.type == LinkCat.UNSUB || data.type == LinkCat.FORMU) {    //Valeurs custom de Catégorie custom pour visu, unsub et formu.
                                        this.setValue(data.type);
                                        data.type = "anchor";
                                    } else {
                                        this.setValue("lnk");
                                    }

                                    data.advanced[this.id] = { attr: this.attr, val: this.getValue() };
                                }
                            }];
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
	                // Add a checkbox field to the "info" tab.
	                // GMA 10/01/2014 : Ne pas modifier l'ordre des liens, sans quoi l'appel d'une fenêtre dialog customisée ne fonctionnera plus correctement
	                
					//SPH 02/07/2015 : modification dans eMemoEditor.js pour ne plus se baser sur les index mais sur le type de l'option - on peut donc retirer des options/changer l'ordre
	              

					if (!editor.xrmMemoEditor.externalTrackingEnabled) {
						var valueVisu = [editor.lang.xrmLinkVisuOption, LinkCat.VISU];
						if (OptionsIndexOfArray(linkTypeItems, valueVisu) < 0)
							linkTypeItems.push(valueVisu); // Index 3
	                }

					var valueUnsub = [editor.lang.xrmLinkUnsubOption, LinkCat.UNSUB];
					if (OptionsIndexOfArray(linkTypeItems, valueUnsub) < 0)
						linkTypeItems.push(valueUnsub); // Index 4

	                //On ajoute des elements html a la tab info de la fenetre ckeditor
					var newOptions = {
						type: "vbox",
						id: "xrmOptionsContainer",
						children: [{
							type: "vbox",
							id: "xrmOptions",
							children: ArrayTrack,
							setup: function (data) {
								var oDialog = this.getDialog();
								//on affiche ces element que en cas le select 'linktype' est affiché
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

					// Backlogs #260, #318 et #319
					// Si les options ont déjà été ajoutées dans une première définition de la fenêtre pour une instance précédente de CKEditor, il faut les supprimer puis les recréer
					// afin que le contexte (variable editor passée à init() et utilisée ensuite dans les fonctions internes) corresponde à l'instance courante de CKEditor, et non à la
					// première instance pour laquelle les options ont été ajoutées
					// Si on laisse tel quel sans recréer, editor correspondrait à la première instance créée, ce qui ne serait donc pas le bon contexte lorsqu'on ouvrira la fenêtre
					// sur une deuxième, troisième, quatrième... instance de CKEditor créée
					// https://ckeditor.com/old/forums/Support/How-remove-Element-particular-Tab
					var existingOptions = infoTab ? infoTab.get("xrmOptionsContainer") : null;

					if (existingOptions)
						infoTab.remove("xrmOptionsContainer");

					if (infoTab)
						infoTab.add(newOptions);

					// Backlog #456 - https:// proposé par défaut
					// https://github.com/ckeditor/ckeditor-dev/blob/master/plugins/link/dialogs/link.js ligne 269
					// https://github.com/ckeditor/ckeditor-dev/issues/2227
					infoTab.get("protocol").default = 'https://';

					//GCH #33296 : Lors du clique sur le lien de visu/unsub, désormais il n'y a plus les options de tracking
	                this.ManageTrackingButtons = function (oDialog, selectedValue) {
	                    var monElem = oDialog.getContentElement('info', 'xrmOptions');
	                    if (selectedValue == LinkCat.FORMU || selectedValue == LinkCat.URL) {
	                        monElem.enable();
	                        monElem.getElement().show();
	                    }
	                    else {
	                        monElem.disable();
	                        monElem.getElement().hide();
	                    }
	                }

	                //On redefinis la fonction onOk de la fenetre pour inserer les attribut xrm (ednn, ednc, ednt, ednd)
	                dialogDefinition.onOk = function () {
	                    var attributes = {},
                            removeAttributes = [],
                            data = {},
                            me = this,
                            editor = this.getParentEditor();

	                    this.commitContent(data);

	                    // Compose the URL.
	                    switch (data.type || 'url') {
	                        case 'url':
	                            var protocol = (data.url && data.url.protocol != undefined) ? data.url.protocol : 'http://',
                                    url = (data.url && CKEDITOR.tools.trim(data.url.url)) || '';
	                            attributes['data-cke-saved-href'] = (url.indexOf('/') === 0) ? url : protocol + url;
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

	                            for (var i = 0 ; i < featureLength ; i++)
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


	                        //COSTUM ATTRIBUTES ***********************************************************************
	                        // Advanced xrm attributes.	                    
	                        attributes[data.advanced['xrmTrackDisabled'].attr] = data.advanced['xrmTrackDisabled'].val;
	                        attributes[data.advanced['xrmTrackLinkName'].attr] = data.advanced['xrmTrackLinkName'].val;
	                        attributes[data.advanced['xrmTrackLink'].attr] = data.advanced['xrmTrackLink'].val;
	                        attributes[data.advanced['xrmTrackType'].attr] = data.advanced['xrmTrackType'].val;
	                        //****************************************************************************************************************
	                    }


	                    var selection = editor.getSelection();

	                    // Browser need the "href" fro copy/paste link to work. (#6641)
	                    attributes.href = attributes['data-cke-saved-href'];




	                    if (!this._.selectedElement) {
	                        // Create element if current selection is collapsed.
	                        var ranges = selection.getRanges(true);
	                        if (ranges.length == 1 && ranges[0].collapsed) {
	                            // Short mailto link text view (#5736).
	                            var text = new CKEDITOR.dom.text(data.type == 'email' ?
                                        data.email.address : attributes['data-cke-saved-href'], editor.document);
								// Backlog #434 - Si on insère "#" dans le cas d'un lien de désinscription ou autre (recopié à partir de data-cke-saved-href), ça perturbe l'analyse des
								// liens par le parseur côté .NET (ParseurMergeLinks) car il confond le # du texte avec le # du href sur certains navigateurs qui positionnent les attributs
								// dans un ordre différent (ex : Edge). Dans ce cas de figure, on insère donc un texte différent de "#"
								if (text.$.data == "#")
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
	    }
	});

