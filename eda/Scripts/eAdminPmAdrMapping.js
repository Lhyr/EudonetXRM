function rfsh() {
    nsAdminPmAdrMapping.CallRefreshMapping();
}

function al() {
    nsAdminPmAdrMapping.CallAddMatchingLine();
}

function del(elt) {
    nsAdminPmAdrMapping.DelMatchingLine(elt);
}




function eAdminPmAdrMapping() {
    this.FieldsMatching = new Array();

    this.AddMatching = function (pmfield, adrfield) {
        for (var i = 0; i < this.FieldsMatching.length; i++) {
            var match = this.FieldsMatching[i];
            if (pmfield == match.PMField || adrfield == match.ADRField) {
                return;
            }
        }
        this.FieldsMatching.push(new Matching(pmfield, adrfield));

    };

    function Matching(pmfield, adrfield) {
        this.PMField = pmfield;
        this.ADRField = adrfield;
    }
}

var divTmp = document.getElementById("divTmp");
var Action =
   {
       Init: 0,
       Refresh: 1,
       AddLine: 2,
       Update: 4
   };


var nsAdminPmAdrMapping = {};


nsAdminPmAdrMapping.getMapping = function () {
    var mapping = new eAdminPmAdrMapping();
    var TbMapping = document.getElementById("TbMapping");

    for (var i = 0; i < TbMapping.rows.length; i++) {
        var tr = TbMapping.rows[i];
        var aSelect = tr.querySelectorAll("select");
        if (aSelect.length != 2)
            continue;

        var pmfield = eTools.getSelectedValue(aSelect[1]);
        var adrfield = eTools.getSelectedValue(aSelect[0]);

        mapping.AddMatching(pmfield, adrfield);

    }

    return mapping;
};


nsAdminPmAdrMapping.CallRefreshMapping = function () {
    var upd = new eUpdater("eda/Mgr/eAdminPmAdrMapping.ashx", 1);
    upd.addParam("m", JSON.stringify(nsAdminPmAdrMapping.getMapping()), "post");
    upd.addParam("a", Action.Refresh, "post");
    upd.send(nsAdminPmAdrMapping.RefreshMapping);
};

nsAdminPmAdrMapping.RefreshMapping = function (oRes) {
    var divTmp = document.getElementById("divTmp");
    divTmp.innerHTML = oRes;
    var tbMappingRefresh = divTmp.querySelector("table");
    tbMappingRefresh.parentNode.removeChild(tbMappingRefresh);

    var tbMapping = document.getElementById("TbMapping");
    tbMapping.parentNode.appendChild(tbMappingRefresh);
    tbMapping.parentNode.replaceChild(tbMappingRefresh, tbMapping);
};

nsAdminPmAdrMapping.CallAddMatchingLine = function () {
    var upd = new eUpdater("eda/Mgr/eAdminPmAdrMapping.ashx", 1);
    upd.addParam("m", JSON.stringify(nsAdminPmAdrMapping.getMapping()), "post");
    upd.addParam("a", Action.AddLine, "post");
    upd.send(nsAdminPmAdrMapping.AddMatchingLine);
};

nsAdminPmAdrMapping.AddMatchingLine = function (oRes) {
    var divTmp = document.getElementById("divTmp");
    divTmp.innerHTML = oRes;
    var trAddLine = divTmp.querySelector("tr");

    var tbMapping = document.getElementById("TbMapping");

    tbMapping.tBodies[0].appendChild(trAddLine);

};

nsAdminPmAdrMapping.DelMatchingLine = function (elt) {
    var tr = findUp(elt, "TR");
    var tbMapping = document.getElementById("TbMapping");
    tbMapping.tBodies[0].removeChild(tr);
    nsAdminPmAdrMapping.CallRefreshMapping();
};

nsAdminPmAdrMapping.Update = function () {
    var upd = new eUpdater("eda/Mgr/eAdminPmAdrMapping.ashx", 1);
    upd.addParam("m", JSON.stringify(nsAdminPmAdrMapping.getMapping()), "post");
    upd.addParam("a", Action.Update, "post");
    upd.send(top.modalPmAdrMapping.hide);
};






