/*
Plugin CKEditor Eudonet XRM pour l'insertion de la signature
CREATION : MOU le 25/11/2013 17h10
*/

CKEDITOR.plugins.add(
	'xrmUserSignature',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmUserSignature',
				CKEDITOR.plugins.xrmUserSignature
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmUserSignature',
				{
				    label: editor.lang.xrmUserSignature,
				    command: 'xrmUserSignature',                   
				    icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/pen.png'
				   //, iconOffset:50 //en cas de sprite
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmUserSignature = {
	exec: function (editor) {
	    if (editor.xrmMemoEditor) {
	        editor.xrmMemoEditor.insertUserSignature(); // appel de la fonction de eMemoEditor.js
	    }
	},
	canUndo: true
}
