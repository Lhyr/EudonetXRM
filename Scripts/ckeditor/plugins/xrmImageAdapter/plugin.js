/*
Plugin CKEditor Eudonet XRM pour la personnalisation de la fenêtre Image et l'ajout d'un bouton Parcourir appelant notre fenêtre d'ajout d'image (la même que pour le bouton Image de la barre d'outils)
MAB le 11/12/2018
Backlog #315
*/

var ArrayOptions = new Array();

CKEDITOR.plugins.add(
	'xrmImageAdapter',
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
	        
	        CKEDITOR.on('dialogDefinition', function (ev) {
	            // Take the dialog name and its definition from the event
	            // data.
	            var dialogName = ev.data.name;
	            var dialogDefinition = ev.data.definition;

	            // Check if the definition is from the dialog we're
	            // interested on (the "Image" dialog).
	            if (dialogName == 'image') {

	                /*************************************************************************************************/
	                /*Options additionnelles ajoutées par XRM*/
	                ArrayOptions = [
						{
							type: 'button',
							label: editor.lang.xrmBrowse, // Parcourir...
							id: 'xrmImageBrowse',
							style: "padding-top: 6px", // le bouton est par défaut aligné en haut, soit avec le libellé URL et non le champ URL
							onClick: function () {
								// this = CKEDITOR.ui.dialog.button
								editor.xrmMemoEditor.imageDialog = this.getDialog();
								//editor.xrmMemoEditor.imageDialog.setState(CKEDITOR.DIALOG_STATE_BUSY); // à utiliser pour bloquer la boîte de dialogue tant que l'URL n'est pas récupérée, si souhaité
								editor.xrmMemoEditor.insertImage(true); // Appel de la fonction d'eMemoEditor avec paramètre à true pour indiquer le cas "depuis la fenêtre CKEditor"
							}
						}
					];
	                /*************************************************************************************************/
				
					// Get a reference to the "Info" tab.
					var infoTab = dialogDefinition.getContents('info');

	                //On ajoute des elements html a la tab info de la fenetre ckeditor
					var newOptions = {
						type: "vbox",
						id: "xrmOptionsContainer",
						children: [{
							type: "vbox",
							id: "xrmOptions",
							style: "vertical-align: bottom", // le bouton est par défaut aligné en haut, soit avec le libellé URL et non le champ URL
							children: ArrayOptions
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

					if (infoTab) {
						// On tente d'insérer directement le bouton juste après le champ URL, soit le premier champ de la première série d'options de la fenêtre (children[0].children[0])
						// L'emplacement children[0].children[1] étant occupé par le bouton "browse" système de CKEditor qui n'est activé que si on utilise son système intégré
						// d'upload d'images, ce qui n'est pas le cas
						if (
							infoTab.elements &&
							infoTab.elements.length > 0 &&
							infoTab.elements[0].children &&
							infoTab.elements[0].children.length > 0 &&
							infoTab.elements[0].children[0] &&
							infoTab.elements[0].children[0].children
						)
							infoTab.elements[0].children[0].children.splice(1, 0, newOptions);
						// Sinon, on ajoute le bouton dans sa propre section tout en bas de la fenêtre
						else
							infoTab.add(newOptions);
					}
	            }
	        });
	    }
	});

