var SearchAddrTimeOut;

function searchAddr(SrchElt, e) {

    clearTimeout(SearchAddrTimeOut);
    if (e && e.type == "keyup" && e.keyCode == 13)
        return;

    SearchAddrTimeOut = setTimeout((function (EltId) { return function () { launchAddr(EltId); } })(SrchElt), 500);
}

function launchAddr(SrchElt) {
    setWait(true)
    var ppid = document.getElementById("ppid").value;
    var pmid = document.getElementById("pmid").value;
    var adrid = document.getElementById("adrid").value;
    var fileType = document.getElementById("fileType").value;

    var updAddr = new eUpdater("mgr/eAddMailAddrMgr.ashx", 0);
    updAddr.addParam("ppid", ppid, "post");
    updAddr.addParam("pmid", pmid, "post");
    updAddr.addParam("adrid", adrid, "post");
    updAddr.addParam("fileType", fileType, "post");
    updAddr.addParam("Search", SrchElt.value, "post");
    updAddr.send(refreshMailsList);
}

function refreshMailsList(oRes) {

    removeAllMails();
    if (!oRes) {
        eAlert(0, top._res_72, top._res_6848, top._res_6849);
        setWait(false);
        return;
    }
    var aXmlMails = oRes.getElementsByTagName("mail");
    var ul = document.getElementById("ulListAddr");
    for (var i = 0; i < aXmlMails.length; i++) {
        var li = document.createElement("li");
        //li.innerText = getXmlTextNode(aXmlMails[i]);
        SetText(li, getXmlTextNode(aXmlMails[i]));
        ul.appendChild(li);
        if (i % 2 == 1)
            addClass(li, "odd");
    }
    setWait(false);
}

function getAddr() {
    var ul = document.getElementById("ulListAddr");
    var aLis = ul.children;
    for (var i = 0; i < aLis.length; i++) {
        var li = aLis[i];
        if (getAttributeValue(li, "sel") == "1")
            return GetText(li);
    }
    return "";


}

function setAddr(evt, bValid) {
    var ul = document.getElementById("ulListAddr");
    var aLis = ul.children;
    for (var i = 0; i < aLis.length; i++) {
        var li = aLis[i];
        li.removeAttribute("sel");
        removeClass(li, "eTVS");
    }

    li = evt.target;
    li.setAttribute("sel", "1");
    addClass(li, "eTVS");
}

function removeAllMails() {
    var ul = document.getElementById("ulListAddr");
    while (ul.children.length > 0) {
        ul.removeChild(ul.children[0]);
    }
}