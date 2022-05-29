/*
Plugin CKEditor Eudonet XRM pour l'insertion des champs de fusion
CREATION : MAB le 28/02/2011, refonte le 04/12/2012
*/

CKEDITOR.plugins.add(
	'xrmMergeFields',
	{
	    init: function (editor) {
	        var _mergeFields = null;
			
			// MAB - #68 13x - Transmission de contexte entre CKEditor et grapesjs
			if (!editor.xrmMemoEditor && grapesjs.xrmMemoEditor) {
				editor.xrmMemoEditor = grapesjs.xrmMemoEditor;
				if (!editor.lang)
					editor.lang = editor.xrmMemoEditor.htmlEditorLang[editor.config.language];
			}
			
	        if (editor.xrmMemoEditor && editor.xrmMemoEditor.mergeFields)
	            _mergeFields = editor.xrmMemoEditor.mergeFields;

	        editor.ui.addRichCombo && editor.ui.addRichCombo('xrmMergeFields',
			{
			    label: editor.lang.xrmMergeFields.label,
			    title: editor.lang.xrmMergeFields.panelTitle,
			    voiceLabel: editor.lang.xrmMergeFields.label,
			    panel:
				{
				    css: [CKEDITOR.getUrl('skins/eudonet/editor.css')].concat(editor.config.contentsCss),
				    multiSelect: false,
				    attributes: { 'aria-label': editor.lang.xrmMergeFields.panelTitle }
				},

			    init: function () {
			        if (_mergeFields == null && editor.xrmMemoEditor && editor.xrmMemoEditor.mergeFields)
			            _mergeFields = editor.xrmMemoEditor.mergeFields;
			        if (_mergeFields) {
			            var currentGroup = '';
			            for (var i in _mergeFields) {
			                if (typeof i != "string")
			                    continue;

			                if (_mergeFields[i] == null)
			                    continue;

			                if ((i.indexOf('.') > 0) && (i.substring(0, i.indexOf('.')) != currentGroup)) {
			                    currentGroup = i.substring(0, i.indexOf('.'));
			                    this.startGroup(currentGroup);
			                }
			                // syntaxe : add( value, html, text )
			                this.add(_mergeFields[i], i, i);
			            }
			        }
			    },

			    onClick: function (value) {
			        editor.focus();
			        editor.fire('saveSnapshot');
			        if (editor.xrmMemoEditor) {
			            editor.xrmMemoEditor.insertMergeField(this, value, '');
			        }
			        editor.fire('saveSnapshot');
			    },

			    onChange: function (value) {
			        editor.focus();
			        editor.fire('saveSnapshot');
			        if (editor.xrmMemoEditor) {
			            editor.xrmMemoEditor.insertMergeField(this, value, '');
			        }
			        editor.fire('saveSnapshot');
			    }
			});

          /*

	        var myCommand = {
	            label: editor.lang.image.menu,
	            command: 'myCommand',
	            group: 'mergeFieldsGroup'
	        };
	        editor.addMenuGroup('mergeFieldsGroup', 1);
	        editor.contextMenu.addListener(function (element, selection) {	           
	            return { myCommand: CKEDITOR.TRISTATE_OFF };
	        });
	        editor.addCommand("myCommand", {
	            exec: function (editor, data) {
	                console.log(data);
	            }
	        });

	        if (_mergeFields == null && editor.xrmMemoEditor && editor.xrmMemoEditor.mergeFields)
	            _mergeFields = editor.xrmMemoEditor.mergeFields;

	        var fileMenuItem = "";
	        var fileMenuItems = {};
	        var fileMenuSubItems= {};
	        var index = 0;
	        fileMenuSubItems[index + ""] = {};
	        if (_mergeFields) {
	            var currentGroup = '';
	            for (var i in _mergeFields) {
	                if (typeof i != "string")
	                    continue;

	                if (_mergeFields[i] == null)
	                    continue;

	                var fileLabel = i.substring(0, i.indexOf('.'));
	                var fieldLabel = i.split(".")[1];
	                var descid = getNumber(_mergeFields[i].split(';')[0]);

	                editor.addMenuItem(descid + "", {
	                    label:fileLabel + "." + fieldLabel + "(" + descid + ")",
	                   // icon: 'http://1.bp.blogspot.com/-ko4BbarSWng/VZ3QaGBtweI/AAAAAAAAQX8/1cHKPcwnfm0/s1600/goofy-smiley.jpg',
	                    //command: "myCommand",
	                    value : _mergeFields[i],
	                    onClick: function () {
	                        if (editor.xrmMemoEditor) {
	                            editor.xrmMemoEditor.insertMergeField(editor.ui.get('xrmMergeFields'), this.value, '');
	                        }
	                    },
	                    group: 'mergeFieldsGroup',
	                    order: 1



	                });	                

	                if (fileLabel != fileMenuItem && fileMenuItem != "") {

	                    var tab = descid - descid % 100;
	                    var array = fileMenuSubItems;
	                    editor.addMenuItem(tab + "", {
	                        label: fileMenuItem,
	                        //icon: 'http://1.bp.blogspot.com/-ko4BbarSWng/VZ3QaGBtweI/AAAAAAAAQX8/1cHKPcwnfm0/s1600/goofy-smiley.jpg',
	                        group: 'mergeFieldsGroup',
	                        order: 100,
	                        getItems: function () { return fileMenuSubItems[index+""]; }
	                    });

	                    fileMenuItems[tab + ""] = CKEDITOR.TRISTATE_OFF;
	                    index++;
	                    fileMenuSubItems[index + ""] = {};
	                    
	                }

	                fileMenuSubItems[index + ""][descid + ""] = CKEDITOR.TRISTATE_OFF;
	                fileMenuItem = fileLabel;
	            }
	        }

	        editor.addMenuItems({
	            myCommand: {
	                label: editor.lang.xrmMergeFields.label,
	               // icon: 'http://1.bp.blogspot.com/-ko4BbarSWng/VZ3QaGBtweI/AAAAAAAAQX8/1cHKPcwnfm0/s1600/goofy-smiley.jpg',
	                group: 'mergeFieldsGroup',
	                order: 10,
	                getItems: function () { return fileMenuItems; }
	            }
	        });

           */
	    }
	}
);
