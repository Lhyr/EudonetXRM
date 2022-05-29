

var eAdminCartoSelection = function () {

    var _wid;

    function internalSave(callback) {

        // Contenu de 
        var textarea = document.getElementById("CartoConfig");
        var data = "";
        if (textarea != null || textarea.value != "") {
            data = textarea.value;
        }

        // On vérifie la validité du JSON
        var config = {};
        try {
            config = JSON.parse(data);
        }
        catch (e) {
            top.eAlert(2, eTools.getRes(416), eTools.getRes(2186), e.message);
            return;
        }

        // On envoi la nouvelle config
        var upd = new eUpdater("eda/mgr/eAdminCartoSelectionManager.ashx", 1);
        upd.addParam("wid", _wid, "post");
        upd.addParam("data", JSON.stringify(config), "post");
        upd.addParam("action", "save-widget-param", "post");

        top.setWait(true);
        upd.send(function (oRes) {
            if (oRes == null)
                return;

            var result = JSON.parse(oRes);
            if (!result.Success) {
                top.eAlert(2, eTools.getRes(416), result.ErrorTitle, result.ErrorMsg);
                return;
            }

            top.setWait(false);
            callback();

        });
    }

    function getDefault(callback) {

        // On envoi la nouvelle config
        var upd = new eUpdater("eda/mgr/eAdminCartoSelectionManager.ashx", 1);
        upd.addParam("wid", _wid, "post");
        upd.addParam("action", "get-widget-default-param", "post");

        top.setWait(true);
        upd.send(function (oRes) {
            if (oRes == null) {
                refreshTextArea(eTools.getRes(416) + " : " + eTools.getRes(7153));
                return;
            }

            var result = JSON.parse(oRes);
            if (!result.Success) {
                top.eAlert(2, eTools.getRes(416), result.ErrorTitle, result.ErrorMsg);
                return;
            }

            top.setWait(false);
            callback(result.Config);
        });
    }


    function refreshTextArea(value) {
        var textarea = document.getElementById("CartoConfig");
        if (textarea != null) {
            textarea.value = value;
        }
    }

    return {
        "init": function (widgetid) {
            _wid = widgetid;
        },
        "save": function (callback) {
            internalSave(callback);
        },
        "delete": function () {
            refreshTextArea("");
        },

        "reset": function () {
            getDefault(function (cfg) { refreshTextArea(JSON.stringify(cfg, null, 4)); });
        },
    }
};

var AdminCartoSelection = new eAdminCartoSelection();



/*
           var textarea = document.getElementById("CartoConfig");
           
            var fact = new factory(textarea.value, "preview");
            fact.when("/TableSelectionSection/", function (data, outer) {

                var label = document.createElement("label");
                label.innerHTML = "TableSelectionSection : "
                    + data.value.SourceTab + ";"
                    + data.value.DestinationTab + ";"
                    + data.value.SelectionTab + ";"
                    + data.value.RelationDescId;

                outer.appendChild(label);

                return {
                    "inner": outer,
                    "discard": true,
                    "handled": true
                };
            })
            .when("/FilterSection/Groups/", function (data, outer) {

                var inner = document.createElement("ul");
                outer.appendChild(inner);

                return {
                    "inner": inner,
                    "discard": false,
                    "handled": true
                };
            })
            .when("/FilterSection/Groups/[*]/", function (data, outer) {

                var inner = document.createElement("li");
                var h3 = document.createElement("h3");
                var p = document.createElement("p");

                h3.innerHTML = data.value.Name.Label;
                p.innerHTML = data.value.Description.Label;

                inner.appendChild(h3);
                inner.appendChild(p);

                outer.appendChild(inner);

                return {
                    "inner": inner,
                    "discard": false,
                    "handled": true
                };
            })
            .when("/FilterSection/Groups/[*]/Filters/", function (data, outer) {

                var inner = document.createElement("ul");
                outer.appendChild(inner);

                return {
                    "inner": inner,
                    "discard": false,
                    "handled": true
                };
            })
            .when("/FilterSection/Groups/[*]/Filters/[*]/", function (data, outer) {

                var inner = document.createElement("li");
                var h3 = document.createElement("h3");
                var p = document.createElement("p");

                h3.innerHTML = data.value.SourceDescId;
                p.innerHTML = data.value.View;

                inner.appendChild(h3);
                inner.appendChild(p);

                outer.appendChild(inner);

                return {
                    "inner": inner,
                    "discard": true,
                    "handled": true
                };
            })
            .traverse();



*/
