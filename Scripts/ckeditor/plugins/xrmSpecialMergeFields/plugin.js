/*
Plugin CKEditor Eudonet XRM pour l'insertion des champs de fusion speciaux
*/

CKEDITOR.plugins.add(
	'xrmSpecialMergeFields',
	{
	    init: function (editor) {

	        var _mergeFields = null;
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        if (editor.xrmMemoEditor) {
				if (editor.xrmMemoEditor.loadSpecialMergeFields) {
					editor.xrmMemoEditor.loadSpecialMergeFields();
				}
				if (editor.xrmMemoEditor.specialMergeFields) {
					_mergeFields = editor.xrmMemoEditor.specialMergeFields;
				}
			}

	        editor.ui.addRichCombo && editor.ui.addRichCombo('xrmSpecialMergeFields',
			{
			    label: editor.lang.xrmSpecialMergeFields.label,
			    title: editor.lang.xrmSpecialMergeFields.panelTitle,
			    voiceLabel: editor.lang.xrmSpecialMergeFields.label,
			    panel:
				{
				    css: [CKEDITOR.getUrl('skins/eudonet/editor.css')].concat(editor.config.contentsCss),
				    multiSelect: false,
				    attributes: { 'aria-label': editor.lang.xrmSpecialMergeFields.panelTitle }
				},

			    init: function () { 
			        if (_mergeFields) {			            
			            for (var i = 0; i < _mergeFields.length; i++) 
			                // syntaxe : add( value, html, text )
			                this.add(_mergeFields[i].value, _mergeFields[i].label, _mergeFields[i].label);
			        }
			    },

			    onClick: function (value) {
			        editor.focus();
			        editor.fire('saveSnapshot');
			        if (editor.xrmMemoEditor) {
			            editor.xrmMemoEditor.insertSpecialMergeField(this, value, '');
			        }
			        editor.fire('saveSnapshot');
			    },

			    onChange: function (value) {
			        editor.focus();
			        editor.fire('saveSnapshot');
			        if (editor.xrmMemoEditor) {
			            editor.xrmMemoEditor.insertSpecialMergeField(this, value, '');
			        }
			        editor.fire('saveSnapshot');
			    }
			});

	    }
	}
);
