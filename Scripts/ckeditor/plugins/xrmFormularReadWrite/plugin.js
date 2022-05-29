/*
Plugin CKEditor Eudonet XRM pour indiquer si l'on souhaite insérer un champ de fusion en lecture ou écriture (formulaires uniquement)
CREATION : MAB le 02/09/2014
*/

CKEDITOR.plugins.add(
	'xrmFormularReadWrite',
	{
		init: function(editor) {
			editor.addCommand(
				'xrmFormularReadWrite',
				new CKEDITOR.dialogCommand('xrmFormularReadWriteDialog')
			);
			
			CKEDITOR.dialog.add( 'xrmFormularReadWriteDialog', this.path + 'dialogs/xrmFormularReadWriteDialog.js' );
		}
	}
);