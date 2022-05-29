/*
Plugin CKEditor Eudonet XRM pour l'insertion d'un lien de désinscription
CREATION : GMA le 06/01/2014
*/

CKEDITOR.plugins.add(
	'xrmLinkUnsubscribe',
	{
	    init: function (editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        editor.addCommand(
				'xrmLinkUnsubscribe',
				CKEDITOR.plugins.xrmLinkUnsubscribe
			);
	        editor.ui.addButton && editor.ui.addButton(
				'xrmLinkUnsubscribe',
				{
				    label: editor.lang.xrmLinkUnsubscribe,
				    command: 'xrmLinkUnsubscribe',
				    icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/inserer-lien-desabonnement.png'
				}
			);
	    }
	}
);

CKEDITOR.plugins.xrmLinkUnsubscribe = {
    exec: function (editor) {
        if (editor.xrmMemoEditor) {
            editor.xrmMemoEditor.unsubscribe(editor); // appel de la fonction de eMemoEditor.js
        }
    },
    canUndo: true
}