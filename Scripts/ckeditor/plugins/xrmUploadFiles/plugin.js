/*
Plugin CKEditor Eudonet XRM pour le chargement des fichiers
CREATION : MOU le 25/11/2013 17h10
*/

CKEDITOR.plugins.add(
	'xrmUploadFiles',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmUploadFiles',
				CKEDITOR.plugins.xrmUploadFiles
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmUploadFiles',
				{
				    label: editor.lang.xrmUploadFiles,
				    command: 'xrmUploadFiles',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/annexes.png'
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmUploadFiles = {
	exec: function (editor) {
	    if (editor.xrmMemoEditor) {
	        editor.xrmMemoEditor.uploadFiles(); // appel de la fonction de eMemoEditor.js
	    }
	},
	canUndo: true
}
