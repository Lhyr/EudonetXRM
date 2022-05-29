/*
Plugin CKEditor Eudonet pour la sélection d'un formulaire
CREATION : GCH le 23/07/2014
*/

CKEDITOR.plugins.add(
	'xrmFormular',
	{
	    init: function (editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        editor.addCommand(
				'xrmFormular',
				CKEDITOR.plugins.xrmFormular
			);
	        editor.ui.addButton && editor.ui.addButton(
				'xrmFormular',
				{
				    label: editor.lang.xrmFormular,
				    command: 'xrmFormular',
				    icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/form.png'
				}
			);
	    }
	}
);

CKEDITOR.plugins.xrmFormular = {
    exec: function (editor) {
        if (editor.xrmMemoEditor) {
            editor.xrmMemoEditor.openFormular(-1); // appel de la fonction de eMemoEditor.js
        }
    },
    canUndo: true
}

