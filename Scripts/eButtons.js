//#region MyRegion

function out(o) {
    var strExt = getExt(o.src);
    o.src = getImg(getImg(getImg(getImg(o.src, 'sel.' + strExt), 'over.' + strExt), 'pressed.' + strExt), 'out.' + strExt) + 'out.' + getExt(o.src);
}
function over(o) {
    var strExt = getExt(o.src);
    o.src = getImg(getImg(getImg(getImg(o.src, 'sel.' + strExt), 'over.' + strExt), 'pressed.' + strExt), 'out.' + strExt) + 'over.' + strExt;
}
function setImgSel(o) {
    var strExt = getExt(o.src);
    o.src = getImg(getImg(getImg(getImg(o.src, 'sel.' + strExt), 'over.' + strExt), 'pressed.' + strExt), 'out.' + strExt) + 'sel.' + strExt;
}
function getImg(strFile, strState) {
    strFile = strFile.toLowerCase();
    if (strFile.lastIndexOf(strState) > 0)
        return strFile.substr(0, strFile.lastIndexOf(strState));
    else
        return strFile;
}
function getExt(strFile) {
    return strFile.substr(strFile.lastIndexOf(".") + 1);

//#endregion
}