var nsDedicatedIpAdmin = nsDedicatedIpAdmin || {};

nsDedicatedIpAdmin.updateParam = function (event) {

    var inpt = event.target;

    var upd = new eUpdater("eda/Mgr/eAdminDedicatedIpParamManager.ashx", 1);

    upd.ErrorCallBack = function () { setWait(false); };
    if (inpt)
        var obj = { k: "DedicatedIp", v: inpt.value }

    upd.json = JSON.stringify(obj);
    setWait(true);

    upd.send(function (oRes) {
        setWait(false);

        var res = JSON.parse(oRes);

        if (!res.Success)
            top.eAlert(1, res.ErrorTitle, res.ErrorMsg, res.ErrorDetailMsg);
        else {
            //TODO
        }
    });
}