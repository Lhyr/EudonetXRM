/*
Plugin CKEditor Eudonet XRM pour l'insertion d'un lien de visualisation
CREATION : GMA le 06/01/2014
*/

CKEDITOR.plugins.add(
	'xrmLinkVisualization',
	{
	    init: function (editor) {
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        editor.addCommand(
				'xrmLinkVisualization',
				CKEDITOR.plugins.xrmLinkVisualization
			);
	        editor.ui.addButton && editor.ui.addButton(
				'xrmLinkVisualization',
				{
				    label: editor.lang.xrmLinkVisualization,
				    command: 'xrmLinkVisualization',
				    icon: this.path.substring(0, this.path.lastIndexOf('/plugins/')) + '/skins/' + editor.config.skin + '/images/inserer-lien-visualiser.png'
				}
			);
	    }
	}
);

CKEDITOR.plugins.xrmLinkVisualization = {
    exec: function (editor) {
        if (editor.xrmMemoEditor) {
            editor.xrmMemoEditor.visualization(editor); // appel de la fonction de eMemoEditor.js
        }
    },
    canUndo: true
}