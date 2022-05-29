/*
Plugin CKEditor Eudonet XRM pour afficher une popup d'aide contenant les différents raccourcis
CREATION : MAB le 09/04/2014 15h00
*/

CKEDITOR.plugins.add(
	'xrmHelp',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmHelp',
				CKEDITOR.plugins.xrmHelp
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmHelp',
				{
				    label: editor.lang.xrmHelp,
				    command: 'xrmHelp',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/help.png'
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmHelp = {
	exec: function (editor) {
	    if (editor.xrmMemoEditor) {
	        editor.xrmMemoEditor.help(); // appel de la fonction de eMemoEditor.js
	    }
	},
	canUndo: true
}
