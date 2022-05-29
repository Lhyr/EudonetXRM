

function rf(tabId, fileId) {
    top.loadFile(tabId, fileId, 3); //3=>eConst.eFileType.FILE_MODIF
    top.cancelOrga();
}