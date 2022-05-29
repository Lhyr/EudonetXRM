/*
Plugin CKEditor Eudonet XRM pour l'insertion d'une feuille de style au contenu de l'éditeur
CREATION : MAB le 28/02/2011, refonte le 04/12/2012
*/

CKEDITOR.plugins.add(
	'xrmInsertCSS',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmInsertCSS',
				CKEDITOR.plugins.xrmInsertCSS
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmInsertCSS',
				{
					label: editor.lang.xrmInsertCSS,
					command: 'xrmInsertCSS',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/css.png'
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmInsertCSS = {
	exec: function (editor) {
	    if (editor.xrmMemoEditor) {
			editor.xrmMemoEditor.insertCSS(); // appel de la fonction de eMemoEditor.js
		}
	},
	canUndo: true
}
