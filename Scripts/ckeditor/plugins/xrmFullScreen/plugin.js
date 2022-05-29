/*
Plugin CKEditor Eudonet XRM pour l'affichage du champ en plein écran
CREATION : MAB le 07/03/2011 - Refonte le 11/06/2012
*/

CKEDITOR.plugins.add(
	'xrmFullScreen',
	{
		init: function(editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
			editor.addCommand(
				'xrmFullScreen',
				CKEDITOR.plugins.xrmFullScreen
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmFullScreen',
				{
					label: editor.lang.xrmFullScreen,
					command: 'xrmFullScreen',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/fullscreen.png',
				}
			);
			
			editor.addCommand(
				'xrmFullScreenDialog',
				CKEDITOR.plugins.xrmFullScreenDialog
			);
			editor.ui.addButton && editor.ui.addButton(
				'xrmFullScreenDialog',
				{
					label: editor.lang.xrmFullScreenDialog,
					command: 'xrmFullScreenDialog',
					icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/fullscreen.png',
				}
			);			
		}
	}
);

	CKEDITOR.plugins.xrmFullScreen = {
	    exec: function (editor) {
	        if (editor.xrmMemoEditor)
	            editor.xrmMemoEditor.switchFullScreen(false);
	    },
	    canUndo: false
	}

	CKEDITOR.plugins.xrmFullScreenDialog = {
	    exec: function (editor) {
	        if (editor.xrmMemoEditor)
	            editor.xrmMemoEditor.switchFullScreen(true);
	    },
	    canUndo: false
	}