var eMemoDialogEditorObject = null;
function init(modal) {
   
    var bHTML = true;
    var bCompactMode = false;
    eMemoDialogEditorObject = new eMemoEditor(
                                           "eMemoEditorValue",
                                            bHTML,
                                            document.getElementById('eExpressMessageMemoEditorContainer'),
                                            null,
                                             document.getElementById('eMemoEditorValue').value,
                                            bCompactMode,
                                            'eMemoDialogEditorObject'
                                       );
    eMemoDialogEditorObject.childDialog = modal;
    eMemoDialogEditorObject.title = top._res_383;

    //eMemoDialogEditorObject.descId = '4438';
    //eMemoDialogEditorObject.fileId = '45641';
    eMemoDialogEditorObject.inlineMode = false;
    eMemoDialogEditorObject.isFullScreen = false;
    eMemoDialogEditorObject.focusOnShow = false;
    //eMemoDialogEditorObject.preventCompactMode = true;
    eMemoDialogEditorObject.updateOnBlur = false;
    eMemoDialogEditorObject.readOnly = false;
    eMemoDialogEditorObject.editorType = '';
    eMemoDialogEditorObject.toolbarType = '';
    if (bHTML) {
        eMemoDialogEditorObject.setSkin('eudonet');
    }
    eMemoDialogEditorObject.config.width = '99%';
    eMemoDialogEditorObject.config.height = '100px';
    eMemoDialogEditorObject.show();

    eMemoDialogEditorObject.setToolBarDisplay(true, true);

    return eMemoDialogEditorObject;
}