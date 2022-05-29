/*
Plugin CKEditor Eudonet XRM pour l'insertion de la date et du nom d'utilisateur en cours (User Message)
CREATION : MAB le 28/02/2011 - Refonte le 11/06/2012
*/

CKEDITOR.plugins.add(
	'xrmUserMessage',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmUserMessage',
				CKEDITOR.plugins.xrmUserMessage
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmUserMessage',
				{
					label: editor.lang.xrmUserMessage,
					command: 'xrmUserMessage',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/user.png'
				}
			);
		}
	}
);

CKEDITOR.plugins.xrmUserMessage = {
	exec: function (editor) {
	    if (editor.xrmMemoEditor) {
			editor.xrmMemoEditor.insertUserMessage(); // appel de la fonction de eMemoEditor.js
	    }
	},
	canUndo: true
}
