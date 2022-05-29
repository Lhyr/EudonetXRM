/*
Plugin CKEditor Eudonet pour l'insertion d'image
CREATION : MAB le 28/02/2011, refonte le 16/01/2014
*/

CKEDITOR.plugins.add(
	'xrmImage',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmImage',
				CKEDITOR.plugins.xrmImage
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmImage',
				{
					label: editor.lang.xrmImage,
					command: 'xrmImage',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/image.png'
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmImage = {
	exec: function(editor) {
		if (editor.xrmMemoEditor) {
			editor.xrmMemoEditor.insertImage(); // appel de la fonction de eMemoEditor.js
		}
	},
	canUndo: true
}
