/*
Plugin CKEditor Eudonet XRM pour indiquer si l'on souhaite insérer un champ de fusion en lecture ou écriture (formulaires uniquement)
CREATION : MAB le 02/09/2014
*/

CKEDITOR.dialog.add( 'xrmFormularReadWriteDialog', function( editor ) {
	var size = CKEDITOR.document.getWindow().getViewPaneSize();

	// 800 px de large maximum
	var width = Math.min( size.width - 70, 800 );

	// 2/3 de la surface d'affichage en hauteur
	var height = size.height / 1.5;

	return {
		title: editor.lang.xrmFormularReadWriteDialog.title,
		minWidth: 100,
		minHeight: 100,

		onShow: function() {
			this.setValueOf( 'main', 'formularRW', 'R' );
		},

		onOk: function() {
			if (this && this._ && this._.editor && this._.editor.xrmMemoEditor && typeof(this._.editor.xrmMemoEditor.insertMergeField) == 'function') {
				var selectedRWValue = this.getValueOf( 'main', 'formularRW' );
				this._.editor.xrmMemoEditor.insertMergeField(this._.editor.ui.get('xrmMergeFields'), this._.editor.xrmMemoEditor.selectedMergeField, selectedRWValue);
			}
		},

		contents: [ {
			id: 'main',
			label: editor.lang.xrmFormularReadWriteDialog.title,
			elements: [ {
				type: 'radio',
				id: 'formularRW',
				label: editor.lang.xrmFormularReadWriteDialog.radioLabel,
				items: [
					[ editor.lang.xrmFormularReadWriteDialog.radioButtonRead, 'R' ],
					[ editor.lang.xrmFormularReadWriteDialog.radioButtonWrite, 'W' ]
				],
				style: 'cursor: hand; text-align: left; margin-top: 10px;'
			} ]
		} ]
	};
} );
