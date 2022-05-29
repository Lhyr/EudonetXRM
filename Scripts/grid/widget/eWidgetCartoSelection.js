
// Utilitaire de linkage d'objet
// permet de déléguer un traitement à un autre objet
if (!Object.create) {
    Object.create = function (o) {
        var f = function () { }
        f.prototype = o;
        return new f();
    }
}

//Bind
if (!Function.prototype.bind) {
    Function.prototype.bind = function (oThis) {

        if (typeof this !== "function") {
            // au plus proche de la fonction interne 
            // ECMAScript 5 IsCallable
            throw new TypeError("Function.prototype.bind - ce qui est à lier ne peut être appelé");
        }

        var aArgs = Array.prototype.slice.call(arguments, 1),
            fToBind = this,
            fNOP = function () { },
            fBound = function () {
                return fToBind.apply(this instanceof fNOP
                       ? this
                       : oThis,
                       aArgs.concat(Array.prototype.slice.call(arguments)));
            };
        if (this.prototype) {
            // Les fonctions natives (Function.prototype) n'ont 
            // pas de prototype
            fNOP.prototype = this.prototype;
        }
        fBound.prototype = new fNOP();

        return fBound;
    };
}

// Construit 
var FieldsFactory = {
    "renderField": function (field, fieldKey, container) {
        switch (field.Action) {

            case "LNKFREETEXT":
                this.text(field, fieldKey, container);
                break;
            case "LNKFREECAT":
            case "LNKCAT":
            case "LNKADVCAT":
                this.catalog(field, fieldKey, container);
                break;
            case "LNKDATE":
                this.dateInterval(field, fieldKey, container);
                break;
            case "LNKCHECK":
                this.radio(field, fieldKey, container);
                break;
            case "LNKNUM":
                this.numInterval(field, fieldKey, container);
                break;
            case "NOSUPPORT":
            default:
                this.notSupported(field, fieldKey, container);
                break;
        }
    },
    "text": function (field, fieldKey, container) {

        var input = document.createElement("input");
        input.id = field.Meta["ename"] + "_" + fieldKey;
        setAttributeValue(input, "type", "text");
        setAttributeValue(input, "eaction", field.Action);
        setAttributeValue(input, "ename", field.Meta["ename"]);
        setAttributeValue(input, "class", "carto-edit");
        container.appendChild(input);
        setEventListener(input, "change", function (descId, evt) {
            var src = evt.srcElement || evt.target;
            if (!src)
                return;

            var data = this.getData(descId);
            data.Type = "text";
            data.Value = src.value;

            this.setData(descId, data);

        }.bind(this, field.SourceDescId));

    },
    "catalog": function (field, fieldKey, container) {

        // minimum      
        var inner = document.createElement("div");
        inner.className = "cat-ctn";
        container.appendChild(inner);

        var input = document.createElement("input");
        input.id = field.Meta["ename"] + "_" + fieldKey;

        setAttributeValue(input, "type", "text");
        setAttributeValue(input, "eaction", field.Action);
        setAttributeValue(input, "ename", field.Meta["ename"]);
        setAttributeValue(input, "efld", "1");
        setAttributeValue(input, "class", "readonly carto-edit");
        setAttributeValue(input, "readonly", "readonly");
        inner.appendChild(input);

        var label = document.createElement("label");
        setAttributeValue(label, "class", "icon-catalog btn-action");
        setAttributeValue(label, "eacttg", input.id);
        setAttributeValue(label, "for", input.id);
        setAttributeValue(label, "eaction", field.Action);
        inner.appendChild(label);

        setEventListener(input, "click", function (fld, evt) {
            var src = evt.srcElement || evt.target;
            if (!src)
                return;
            // eTools
            eTools.showCatalogView(src, {
                "title": fld.Label,
                "value": getAttributeValue(src, "dbv"),
                "onValidate": function (catalogDialog, srcId, trgId, tabSelectedLabels, tabSelectedValues, selectedIDs) {
                    src.value = tabSelectedLabels;
                    setAttributeValue(src, "dbv", selectedIDs);
                    setAttributeValue(src, "title", tabSelectedLabels);
                    this.setData(fld.SourceDescId, { "Type": "catalog", "Value": selectedIDs, "Display": tabSelectedLabels });

                }.bind(this)
            });
        }.bind(this, field));


    },
    "numInterval": function (field, fieldKey, container) {

        // minimum      
        var divMin = document.createElement("div");
        divMin.className = "interval";
        container.appendChild(divMin);

        var unitLeft = document.createElement("span");
        unitLeft.innerText = field.Unit;
        divMin.appendChild(unitLeft);


        var labelMin = document.createElement("label");
        setAttributeValue(labelMin, "class", "sp-italic");
        labelMin.innerText = "Minimum";
        divMin.appendChild(labelMin);

        var inputMin = document.createElement("input");
        inputMin.id = "fld_" + field.Meta["ename"] + "_" + fieldKey + "_min";
        setAttributeValue(inputMin, "class", "carto-edit");
        divMin.appendChild(inputMin);

        setEventListener(inputMin, "change", function (descId, evt) {
            var startSrc = evt.srcElement || evt.target;
            if (!startSrc)
                return;

            var endSrc = document.getElementById(getAttributeValue(startSrc, "otherid"));
            if (!endSrc)
                return;

            removeClass(startSrc, "edit-error");
            removeClass(endSrc, "edit-error");


            var storeValue = eNumber.ConvertNumToBdd(startSrc.value);
            if (startSrc.value != "" && !eNumber.IsValid()) {
                this.setMinValue(descId, "");
                addClass(startSrc, "edit-error");
                showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(6275));
                return;
            }

            startSrc.value = eNumber.ConvertBddToDisplayFull(storeValue);
            if (this.intervalCheck(descId, startSrc.value, endSrc.value, this.numComparer)) {

                var valueStart = eNumber.ConvertNumToBdd(startSrc.value);
                var valueEnd = eNumber.ConvertNumToBdd(endSrc.value);

                this.setMinValue(descId, valueStart);
                this.setMaxValue(descId, valueEnd);

            } else {
                this.setMinValue(descId, "");
                addClass(startSrc, "edit-error");
                showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(2050));
            }

        }.bind(this, field.SourceDescId));

        // maximum
        var divMax = document.createElement("div");
        divMax.className = "interval";
        container.appendChild(divMax);

        // Unité
        var unitRight = document.createElement("span");
        unitRight.innerText = field.Unit;
        divMax.appendChild(unitRight);

        var labelMax = document.createElement("label");
        setAttributeValue(labelMax, "class", "sp-italic");
        labelMax.innerText = "Maximum";
        divMax.appendChild(labelMax);

        var inputMax = document.createElement("input");
        inputMax.id = "fld_" + field.Meta["ename"] + "_" + fieldKey + "_max";
        setAttributeValue(inputMax, "class", "carto-edit");
        divMax.appendChild(inputMax);

        setEventListener(inputMax, "change", function (descId, evt) {
            var endSrc = evt.srcElement || evt.target;
            if (!endSrc)
                return;

            var startSrc = document.getElementById(getAttributeValue(endSrc, "otherid"));
            if (!startSrc)
                return;

            removeClass(startSrc, "edit-error");
            removeClass(endSrc, "edit-error");

            var storeValue = eNumber.ConvertNumToBdd(endSrc.value);
            if (endSrc.value != "" && !eNumber.IsValid()) {
                this.setMaxValue(descId, "");
                addClass(endSrc, "edit-error");
                showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(6275));
                return;
            }

            endSrc.value = eNumber.ConvertBddToDisplayFull(storeValue);
            if (this.intervalCheck(descId, startSrc.value, endSrc.value, this.numComparer)) {

                var valueStart = eNumber.ConvertNumToBdd(startSrc.value);
                var valueEnd = eNumber.ConvertNumToBdd(endSrc.value);

                this.setMinValue(descId, valueStart);
                this.setMaxValue(descId, valueEnd);
            } else {
                this.setMaxValue(descId, "");
                addClass(endSrc, "edit-error");
                showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(2050));
            }

        }.bind(this, field.SourceDescId));

        // echanges des id
        setAttributeValue(inputMin, "otherid", inputMax.id);
        setAttributeValue(inputMax, "otherid", inputMin.id);
    },
    "dateInterval": function (field, fieldKey, container) {

        // minimum      
        var divMin = document.createElement("div");
        divMin.className = "interval";
        container.appendChild(divMin);

        var labelMin = document.createElement("label");
        setAttributeValue(labelMin, "class", "sp-italic");
        labelMin.innerText = eTools.getRes(426);
        divMin.appendChild(labelMin);

        var inputMin = document.createElement("input");
        inputMin.id = field.Meta["ename"] + "_" + fieldKey + "_start";
        setAttributeValue(inputMin, "eaction", field.Action);
        setAttributeValue(inputMin, "class", "readonly carto-edit");
        setAttributeValue(inputMin, "readonly", "readonly");
        setAttributeValue(inputMin, "type", "text");
        setAttributeValue(inputMin, "ename", field.Meta["ename"]);

        divMin.appendChild(inputMin);

        var spanMin = document.createElement("label");
        setAttributeValue(spanMin, "class", "icon-agenda btn-action");
        setAttributeValue(spanMin, "eacttg", inputMin.id);
        setAttributeValue(spanMin, "for", inputMin.id);
        setAttributeValue(spanMin, "eaction", field.Action);
        divMin.appendChild(spanMin);

        setEventListener(inputMin, "click", function (fld, evt) {
            var startSrc = evt.srcElement || evt.target;
            if (!startSrc)
                return;

            var endSrc = document.getElementById(getAttributeValue(startSrc, "otherid"));
            if (!endSrc)
                return;

            var dateTime = new eDateTimePicker({
                "title": eTools.getRes(426),
                "okLabel": eTools.getRes(28),
                "cancelLabel": eTools.getRes(29),
                "hideEmptyDate": false,
                "hideHoursAndMinutes": true,
                "value": startSrc.value,
                "onValidate": function (displayDate) {
                    removeClass(startSrc, "edit-error");
                    removeClass(endSrc, "edit-error");
                    startSrc.value = displayDate;
                    if (this.intervalCheck(fld.SourceDescId, startSrc.value, endSrc.value, this.dateComparer)) {
                        var dateStart = eDate.ConvertDisplayToBdd(startSrc.value);
                        var dateEnd = eDate.ConvertDisplayToBdd(endSrc.value);
                        this.setMinValue(fld.SourceDescId, dateStart, startSrc.value);
                        this.setMaxValue(fld.SourceDescId, dateEnd, endSrc.value);
                    } else {
                        this.setMinValue(fld.SourceDescId, "");
                        addClass(startSrc, "edit-error");
                        showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(2051));
                    }
                }.bind(this) // pour valider la valeur
            });
        }.bind(this, field));

        // maximum
        var divMax = document.createElement("div");
        divMax.className = "interval";
        container.appendChild(divMax);

        var labelMax = document.createElement("label");
        setAttributeValue(labelMax, "class", "sp-italic");
        labelMax.innerText = eTools.getRes(271);// "Fin";
        divMax.appendChild(labelMax);

        var inputMax = document.createElement("input");
        inputMax.id = field.Meta["ename"] + "_" + fieldKey + "_end";
        setAttributeValue(inputMax, "eaction", field.Action);
        setAttributeValue(inputMax, "class", "readonly carto-edit");
        setAttributeValue(inputMax, "readonly", "readonly");
        setAttributeValue(inputMax, "type", "text");
        setAttributeValue(inputMax, "ename", field.Meta["ename"]);

        divMax.appendChild(inputMax);

        var spanMax = document.createElement("label");
        setAttributeValue(spanMax, "class", "icon-agenda btn-action");
        setAttributeValue(spanMax, "eacttg", inputMax.id);
        setAttributeValue(spanMax, "for", inputMax.id);
        setAttributeValue(spanMax, "eaction", field.Action);
        divMax.appendChild(spanMax);

        setEventListener(inputMax, "click", function (fld, evt) {
            var endSrc = evt.srcElement || evt.target;
            if (!endSrc)
                return;

            var startSrc = document.getElementById(getAttributeValue(endSrc, "otherid"));
            if (!startSrc)
                return;

            var dateTime = new eDateTimePicker({
                "title": eTools.getRes(271),
                "okLabel": eTools.getRes(28),
                "cancelLabel": eTools.getRes(29),
                "hideEmptyDate": false,
                "hideHoursAndMinutes": true,
                "value": endSrc.value,
                "onValidate": function (displayDate) {
                    removeClass(startSrc, "edit-error");
                    removeClass(endSrc, "edit-error");
                    endSrc.value = displayDate;
                    if (this.intervalCheck(fld.SourceDescId, startSrc.value, endSrc.value, this.dateComparer)) {
                        var dateStart = eDate.ConvertDisplayToBdd(startSrc.value);
                        var dateEnd = eDate.ConvertDisplayToBdd(endSrc.value);
                        this.setMinValue(fld.SourceDescId, dateStart, startSrc.value);
                        this.setMaxValue(fld.SourceDescId, dateEnd, endSrc.value);
                    } else {
                        this.setMaxValue(fld.SourceDescId, "");
                        addClass(endSrc, "edit-error");
                        showWarning(eTools.getRes(2049), eTools.getRes(2048), eTools.getRes(2051));
                    }
                }.bind(this) // pour valider la valeur              
            });
        }.bind(this, field));

        // liaisons mutuelles
        setAttributeValue(inputMin, "otherid", inputMax.id);
        setAttributeValue(inputMax, "otherid", inputMin.id);
    },
    "radio": function (field, fieldKey, container) {

        // Yes
        var inputYes = document.createElement("input");
        setAttributeValue(inputYes, "type", "radio");
        inputYes.id = "fld_" + field.Meta["ename"] + "_" + fieldKey + "__1";
        setAttributeValue(inputYes, "name", "nm_" + field.Meta["ename"] + "_" + fieldKey);

        // Oui est coché par défaut ?
        // setAttributeValue(inputYes, "checked", "checked");
        // this.setData(field.SourceDescId, { "type": "logic", "value": true });

        container.appendChild(inputYes);

        var labelYes = document.createElement("label");
        labelYes.innerText = eTools.getRes(58);
        setAttributeValue(labelYes, "for", inputYes.id);
        container.appendChild(labelYes);

        setEventListener(inputYes, "click", function (fld, evt) {
            this.setData(fld.SourceDescId, { "Type": "logic", "Value": true, "Display": eTools.getRes(58) });
        }.bind(this, field));

        // No
        var inputNo = document.createElement("input");
        setAttributeValue(inputNo, "type", "radio");
        inputNo.id = "fld_" + field.Meta["ename"] + "_" + fieldKey + "__0";
        setAttributeValue(inputNo, "name", "nm_" + field.Meta["ename"] + "_" + fieldKey);
        container.appendChild(inputNo);

        var labelNo = document.createElement("label");
        labelNo.innerText = eTools.getRes(59);
        setAttributeValue(labelNo, "for", inputNo.id);
        container.appendChild(labelNo);

        setEventListener(inputNo, "click", function (fld, evt) {
            this.setData(fld.SourceDescId, { "Type": "logic", "Value": false, "Display": eTools.getRes(59) });
        }.bind(this, field));


    },
    "notSupported": function (field, fldKey, container) {
        var span = document.createElement("span");
        span.innerText = eTools.getRes(6275);
        span.style.color = "red";
        container.appendChild(span);
    },
    "dateComparer": function (strMinDate, strMaxDate) {
        var minValue = eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(strMinDate));
        var maxValue = eDate.Tools.GetDateFromString(eDate.ConvertDisplayToBdd(strMaxDate));
        return minValue.getTime() <= maxValue.getTime();
    },
    "numComparer": function (strMinValue, strMaxValue) {
        // Pour la vérif, on convertit la virgule en un point
        var minValue = eNumber.ConvertNumToBdd(strMinValue).replace(",", ".") * 1;
        var maxValue = eNumber.ConvertNumToBdd(strMaxValue).replace(",", ".") * 1;
        return minValue <= maxValue;
    },
    "intervalCheck": function (descId, minValue, maxValue, compare) {
        if (minValue == "" || maxValue == "")
            return true;
        return compare(minValue, maxValue);
    }
}

// Construit un filtre
var FilterBuilder = Object.create(FieldsFactory);
FilterBuilder.renderFilter = function (filter, filterKey, filterContainer) {

    var span = document.createElement("span");
    span.className = "labelIntro";
    filterContainer.appendChild(span);

    var h5 = document.createElement("h5");
    h5.innerText = filter.Label;
    span.appendChild(h5);
    span.id = filter.Meta["ename"];
    // meta info
    for (var k in filter.Meta) {
        if (!filter.Meta.hasOwnProperty(k) || k == "ename")
            continue;

        setAttributeValue(span, k, filter.Meta[k]);
    }

    var innerFilter = document.createElement("div");
    innerFilter.className = "innerFilter";
    filterContainer.appendChild(innerFilter);

    this.meta[filter.SourceDescId] = {
        "Name": filter.FilterName,
        "Action": filter.Action,
        "Mult": filter.Meta["mult"],
        "Unit": filter.Unit
    };
    this.renderField(filter, filterKey, innerFilter);
}
// Cosntruit un group
var GroupBuilder = Object.create(FilterBuilder);
GroupBuilder.renderGroup = function (group, groupKey, groupContainer) {

    var partieCollapse = document.createElement("div");
    partieCollapse.className = "partieCollapse";
    setAttributeValue(partieCollapse, "show", "0");
    // setAttributeValue(partieCollapse, "title", group.Description);
    groupContainer.appendChild(partieCollapse);

    var icon = document.createElement("i");
    icon.className = "icon-caret-right";
    partieCollapse.appendChild(icon);

    var label = document.createElement("label");
    label.className = "titreCollapse";
    label.innerText = group.Name;
    partieCollapse.appendChild(label);

    setEventListener(label, "click", function (evt) {
        var src = evt.srcElement || evt.target;

        if (src.tagName.toLowerCase() == "i")
            src = src.parentElement;

        if (src.className != "titreCollapse")
            return;

        var parent = label.parentElement;
        var newValue = getAttributeValue(parent, "show") == "1" ? "0" : "1";
        setAttributeValue(parent, "show", newValue);

        var i = parent.querySelector("i[class^='icon-caret']");
        if (i != null)
            i.className = (i.className == "icon-caret-right") ? "icon-caret-down" : "icon-caret-right";
    });

    var partieCollapseInner = document.createElement("div");
    partieCollapseInner.className = "partieCollapseInner";
    partieCollapse.appendChild(partieCollapseInner);

    // affichage des filtres
    group.Filters.forEach(function (group, filterKey) {
        this.renderFilter(group, groupKey + "_" + filterKey, partieCollapseInner);
    }, this);
}

// Construit le menu
var MenuBuilder = Object.create(GroupBuilder);
MenuBuilder.init = function (wid) {
    if (!this.store)
        this.store = {};

    if (!this.meta)
        this.meta = {};

    this.widgetId = wid;
};
MenuBuilder.render = function (menuConfig, menuContainer) {

    // afichage des filtres
    menuConfig.Groups.forEach(function (group, index) {
        this.renderGroup(group, index, menuContainer);
    }, this);
};
MenuBuilder.setMinValue = function (descid, value, displayMin) {
    var data = this.getData(descid);
    data.Type = "interval";
    data.MinValue = value;

    if (typeof (displayMin) === "undefined")
        displayMin = value;
    data.DisplayMin = displayMin;

    if (typeof (data.MaxValue) === "undefined") {
        data.MaxValue = "";
        data.DisplayMax = "";
    }

    if (data.MinValue == "" && data.MaxValue == "")
        this.setData(descid, { "Type": "empty", "Value": "", "MinValue": "", "MaxValue": "", "Display": "", "DisplayMin": "", "DisplayMax": "" });
    else
        this.setData(descid, data);
};
MenuBuilder.setMaxValue = function (descid, value, displayMax) {
    var data = this.getData(descid);
    data.Type = "interval";
    data.MaxValue = value;

    if (typeof (displayMax) === "undefined")
        displayMax = value;

    data.DisplayMax = displayMax;

    if (typeof (data.MinValue) === "undefined") {
        data.MinValue = "";
        data.DisplayMin = "";
    }

    if (data.MinValue == "" && data.MaxValue == "")
        this.setData(descid, { "Type": "empty", "Value": "", "MinValue": "", "MaxValue": "", "Display": "", "DisplayMin": "", "DisplayMax": "" });
    else
        this.setData(descid, data);
};
MenuBuilder.setData = function (descid, data) {

    if (data.Type != "interval" && typeof (data.Value) !== "undefined" && data.Value === "")
        data = { "Type": "empty", "Value": "", "MinValue": "", "MaxValue": "", "Display": "" };

    this.store[descid] = data;

    this.debug();
};
MenuBuilder.getData = function (descid) {
    var value = this.store[descid];
    if (!value)
        value = { "Type": "empty", "Value": "", "MinValue": "", "MaxValue": "", "Display": "", "DisplayMin": "", "DisplayMax": "" };

    return value;
};
MenuBuilder.buildResume = function (breakline) {

    var criteria = this.internalCriteria(
        function (descId, data) {
            var newData = {};
            newData.DescId = descId;
            newData.Type = data.Type;

            newData.Value = data.Value;
            newData.MinValue = data.MinValue;
            newData.MaxValue = data.MaxValue;

            newData.DisplayMin = data.DisplayMin;
            newData.DisplayMax = data.DisplayMax;
            newData.Display = data.Display;

            newData.Action = this.meta[descId].Action;
            newData.Mult = this.meta[descId].Mult;
            newData.Name = this.meta[descId].Name;
            newData.Unit = this.meta[descId].Unit;
            return newData;
        }.bind(this));

    if (criteria == null || criteria.length == 0)
        return null;

    var resume = [];
    for (var descId in criteria) {
        if (criteria.hasOwnProperty(descId)) {
            var data = criteria[descId];
            resume.push(this.buildResumeLine(data));
        }
    }

    return joinString(breakline, resume);
};
MenuBuilder.buildResumeLine = function (data) {

    var value = "";
    switch (data.Type) {
        case "interval":
            var before = "inférieur ou égale à @maxValue@unit";
            var after = "supérieur ou égale à @minValue@unit";
            var between = "est dans l'intervalle [@minValue@unit - @maxValue@unit]";
            if (data.Action == "LNKDATE") {
                var before = "avant le @maxValue";
                var after = "après le @minValue";
                var between = "entre le @minValue et le @maxValue";
            }
            if (data.MinValue.length == 0 && data.MaxValue.length > 0)
                value = before;
            else if (data.MinValue.length > 0 && data.MaxValue.length == 0)
                value = after;
            else if (data.MinValue.length > 0 && data.MaxValue.length > 0)
                value = between;
            value = value.replace("@minValue", data.DisplayMin).replace("@maxValue", data.DisplayMax).replace(/@unit/g, data.Unit);
            break;
        case "logic":
            value = "@value"
            value = value.replace("@value", data.Display);
            break;
        case "text":
            value = "contient la valeur @value";
            value = value.replace("@value", data.Value);
            break;
        case "catalog":
            value = "égale à @value";
            if (data.Mult == "1")
                value = "est dans la liste (@value)";
            value = value.replace("@value", data.Display);
            break;
        default:
            value = "@value";
            value = value.replace("@value", data.Value);
    }

    return data.Name + " " + value;
};
MenuBuilder.internalCriteria = function (dataSelector) {
    if (typeof (dataSelector) != "function") {
        var dataSelector = function (descId, data) {
            var newData = {};
            newData.DescId = descId;
            newData.Type = data.Type;
            newData.Value = data.Value;
            newData.MinValue = data.MinValue;
            newData.MaxValue = data.MaxValue;
            return newData;
        };
    }

    this.finalCriteria = [];
    for (var k in this.store) {
        if (this.store.hasOwnProperty(k)) {
            var data = this.store[k];
            if (data.Type != "empty") {
                hasCriteria = true;
                this.finalCriteria.push(dataSelector(k, data));
            }
        }
    }

    return this.finalCriteria;
};
MenuBuilder.getCriteria = function () {
    return this.internalCriteria(
        function (descId, data) {
            var newData = {};
            newData.DescId = descId;
            newData.Type = data.Type;
            newData.Value = data.Value;
            newData.MinValue = data.MinValue;
            newData.MaxValue = data.MaxValue;
            return newData;
        });
};
MenuBuilder.debug = function () {

    for (var k in this.store) {
        if (!this.store.hasOwnProperty(k))
            continue;

        console.log(k + " : " + JSON.stringify(this.store[k]));
    }
}

var __ClientServer = {
    'load_debug': function (callback) {
        callback(dataTest);
    },
    'load': function (callback) {
        var upd = new eUpdater(this.url, 1);
        upd.addParam("wid", this.wid, "post");
        upd.addParam("action", this.action, "post");
        upd.send(function (oRes) {
            if (oRes == null) {
                top.eAlert(2, eTools.getRes(72), eTools.getRes(2052));
                return;
            }

            var result = JSON.parse(oRes);
            callback(result);
        });

    },
    'update': function () { },
    'error': function () { },
}
var __AbstractCardMenu = Object.create(__ClientServer);
__AbstractCardMenu.render = function (card, fileId) {

    var box = this.cardContainer.querySelector("div[fid='" + fileId + "'");
    if (box != null) {
        var boxBuilder = "";

        // rendu images
        var imageBuilder = "";
        card.Images.forEach(function (imageData, index) {
            var showFirstImage = index == 0 ? "1" : "0";
            imageBuilder += this.imageTemplate
                .replace("{VALUE}", encode(imageData))
                .replace("{SHOW_IMAGE}", showFirstImage);
        }, this);

        // rendu des tables
        var tabBuilder = "";
        card.Tabs.forEach(function (tab) {
            var fldBuilder = "";
            tab.Fields.forEach(function (fld, index) { fldBuilder += this.fieldTemplate.replace("{VALUE}", fld); }, this);
            tabBuilder += this.tabTemplate.replace("{VALUE}", tab.Title).replace("{FIELDS}", fldBuilder);
        }, this);

        boxBuilder += this.cardTemplate.replace("{IMAGES}", imageBuilder).replace("{TABLES}", tabBuilder);
        box.innerHTML = boxBuilder;

        // Pas d'image on masque le conteneur d'image
        var entete = box.querySelector(".enteteResultat");
        if (entete && card.Images.length == 0)
            entete.style.height = "35px";
    }
};
__AbstractCardMenu.setCardWait = function (container) {

    // Chargement en cours ...
    var waiter = document.createElement("div");
    waiter.className = "xrm-widget-waiter";
    var img = document.createElement("img");
    setAttributeValue(img, "alt", "waiting...");
    setAttributeValue(img, "src", "themes/default/images/wait.gif");
    waiter.appendChild(img);
    container.appendChild(waiter);
};
__AbstractCardMenu.remove = function (fileId) { };
__AbstractCardMenu.clearSelection = function () { this.selected = []; };
__AbstractCardMenu.insert = function (fileId, card) {

    var selectionEnbled = getAttributeValue(this.menuContainer, "selection") == "1";
    var box = this.cardContainer.querySelector("div[fid='" + fileId + "'");
    if (box != null) {

        this.cardContainer.insertBefore(box, this.cardContainer.firstChild);
        var checkbox = box.querySelector(".absCheck[type='checkbox']");
        checkbox.checked = selectionEnbled || checkbox.checked;
        this.updateSelectedBox(box, selectionEnbled);
        this.updateSelectedCount();
        this.onCheck({ "id": fileId, "selected": checkbox.checked });

    } else {

        var boxResult = document.createElement("div");
        boxResult.className = "boxResultat";
        boxResult.style.height = "0px";
        boxResult.style.opacity = "0";
        boxResult.style.marginBottom = "-20px";

        setAttributeValue(boxResult, "fid", fileId);
        this.setCardWait(boxResult);

        // On repositionne la scroll   

        this.cardContainer.scrollTop = "0px";
        this.cardContainer.insertBefore(boxResult, this.cardContainer.firstChild);

        // affichage des filtres

        this.render(card, fileId);

        var bodyResult = boxResult.querySelector(".bodyResult");

        var cbo = boxResult.querySelector(".absCheck[type='checkbox']");

        cbo.checked = selectionEnbled;
        this.updateSelectedBox(boxResult, selectionEnbled);
        this.updateSelectedCount();

        setEventListener(cbo, "click", function (evt) { this.selectOne(evt); }.bind(this));

        var heigthBox = bodyResult.offsetHeight;// || 50;

        entete = 150;
        if (card.Images.length == 0)
            entete = 35;

        boxResult.style.height = (heigthBox + entete) + "px";
        boxResult.style.marginBottom = "15px";

        setTimeout(function () {
            boxResult.style.opacity = "1";
        }, 500);

        var btnLeft = boxResult.querySelector("#btnLeft");
        var btnRight = boxResult.querySelector("#btnRight");
        if (card.Images.length <= 1) {
            btnLeft.style.display = "none";
            btnRight.style.display = "none";
        } else {
            setEventListener(btnLeft, "click", function (evt) { this.showNextImage(evt, fileId, false); }.bind(this));
            setEventListener(btnRight, "click", function (evt) { this.showNextImage(evt, fileId, true); }.bind(this));

            var currentImg = boxResult.querySelector("#currentImg");
            if (currentImg && card.Images.length > 1) {
                currentImg.innerHTML = "1/" + card.Images.length;
            }
        }

        this.onCheck({ "id": fileId, "selected": selectionEnbled });
    }
       
       
};
__AbstractCardMenu.updateSelectedBox = function (box, selected) {
    setAttributeValue(box, "selected", selected ? "1" : "0");

}
__AbstractCardMenu.updateSelectedCount = function () {
    // Cartes sélécetionnées
    var selectedCount = this.cardContainer.querySelectorAll(".boxResultat .absCheck:checked");
    var items = this.menuContainer.querySelectorAll("span[sid='selectedCardCount_" + this.wid + "']");
    if (selectedCount && items.length > 0) {
        Array.prototype.slice.call(items).forEach(function (item) { item.innerText = selectedCount.length; });;
    }

    var all = this.cardContainer.querySelectorAll(".boxResultat .absCheck");

    var allSelectCard = this.menuContainer.querySelector("#inputAll input");
    if (allSelectCard)
        allSelectCard.checked = all.length == selectedCount.length;

}
__AbstractCardMenu.setCard = function (options) {
    var boxResult = this.cardContainer.querySelector("div[fid='" + options.id + "']");
    if (boxResult) {
        var cbo = boxResult.querySelector(".absCheck[type='checkbox']");
        if (cbo) {
            cbo.checked = options.selected;
            this.updateSelectedBox(boxResult, options.selected);
            this.updateSelectedCount();
        }
    }
}
__AbstractCardMenu.switchSelection = function () {
    // Affichage uniquement des cartes sélectionnées sinon toutes les cartes

    var currentValue = getAttributeValue(this.menuContainer, "selection") == "1";
    var newValue = currentValue ? "0" : "1";
    setAttributeValue(this.menuContainer, "selection", newValue);

}
__AbstractCardMenu.selectAll = function () {
    // Affichage uniquement des cartes sélectionnées sinon toutes les cartes
    var items = this.cardContainer.querySelectorAll(".boxResultat");
    Array.prototype.slice.call(items).forEach(function (item) {
        var fid = getAttributeValue(item, 'fid');
        var selected = getAttributeValue(item, 'selected');
        if (selected != "1") {// si pas sélectionner
            this.setCard({ 'id': fid, 'selected': true });
            this.onCheck({ "id": fid, "selected": true });
        }
    }, this);
}
__AbstractCardMenu.resetUnselected = function () {
    // Affichage uniquement des cartes sélectionnées sinon toutes les cartes
    var items = this.cardContainer.querySelectorAll(".boxResultat[selected='0']");
    Array.prototype.slice.call(items).forEach(function (item) { item.parentElement.removeChild(item); }, this);
}
__AbstractCardMenu.resetAll = function () {
    // Affichage uniquement des cartes sélectionnées sinon toutes les cartes
    var items = this.cardContainer.querySelectorAll(".boxResultat");
    Array.prototype.slice.call(items).forEach(function (item) { item.parentElement.removeChild(item); }, this);
}
__AbstractCardMenu.selectOne = function (evt) {
    var src = evt.srcElement || evt.target;
    if (!src)
        return;

    var selected = src.checked;
    while (src.className != "boxResultat" && src.tagName.toLowerCase() != "body")
        src = src.parentElement

    if (src.tagName.toLowerCase() == "body")
        return;

    this.updateSelectedBox(src, selected);
    this.updateSelectedCount();

    var fid = getAttributeValue(src, "fid");
    this.onCheck({ "id": fid, 'selected': selected });


}
__AbstractCardMenu.showNextImage = function (evt, fileId, towards) {
    var src = evt.srcElement || evt.target;
    if (!src)
        return;

    var box = this.cardContainer.querySelector(".boxResultat[fid='" + fileId + "']");
    if (box == null)
        return;


    var images = box.querySelectorAll(".card");
    if (images.length <= 1)
        return;

    var currentActive = box.querySelector(".card[data-active='1']");
    if (currentActive == null)
        return;

    var nextImage = 0;
    for (var i = 0; i < images.length; i++) {
        if (images[i] == currentActive) {
            if (i == 0)
                nextImage = towards ? 1 : images.length - 1;
            else if (i == images.length - 1)
                nextImage = towards ? 0 : i - 1;
            else
                nextImage = towards ? i + 1 : i - 1;
            break;
        }
    }

    setAttributeValue(currentActive, "data-active", "0");
    setAttributeValue(images[nextImage], "data-active", "1");

    var currentImg = box.querySelector("#currentImg");
    if (currentImg && images.length > 1) {
        currentImg.innerHTML = (nextImage + 1) + "/" + images.length;
    }

}
__AbstractCardMenu.getCard = function (fileId) {
    var boxResult = this.cardContainer.querySelector("div[fid='" + fileId + "']");
    if (boxResult) {
        var cbo = boxResult.querySelector(".absCheck[type='checkbox']");
        if (cbo)
            return { 'id': fileId, 'selected': cbo.checked };
    }
        
    var selectionEnbled = getAttributeValue(this.menuContainer, "selection") == "1";

    return { 'id': fileId, 'selected': selectionEnbled };
}
__AbstractCardMenu.getSelectedCards = function () {
    // Affichage uniquement des cartes sélectionnées sinon toutes les cartes
    var selected = [];
    var items = this.cardContainer.querySelectorAll(".boxResultat[selected='1']");
    Array.prototype.slice.call(items).forEach(function (item) {
        var fid = getAttributeValue(item, 'fid');
        selected.push(fid);
    }, this);

    return selected;
}
__AbstractCardMenu.onCardSelectionChanged = function (onChange) {
    if (typeof (onChange) == "function")
        this.onCheck = onChange;
}
// Objet permettant de gérer le menu des mini-fiches
var CardMenu = Object.create(__AbstractCardMenu);
CardMenu.init = function (widgetId, container) {

    this.url = "mgr/eCartoSelectionManager.ashx";
    this.wid = widgetId;
    this.selected = [];
    this.onCheck = function () { };
    this.action = "";
    this.menuContainer = container;
    this.cardContainer = container.querySelector("#contentResultat");

    this.imageTemplate = '<div class="card" style="background-image: url(\'{VALUE}\');" data-active="{SHOW_IMAGE}"></div>';
    this.fieldTemplate = '<div class="col-md-12"><div class="valeurResultat">{VALUE}</div></div>';
    this.tabTemplate = '<div class="col-md-12 titreResultat">{VALUE}</div>{FIELDS}';
    this.cardTemplate = '\
	    <div class="enteteResultat">\
	    <input type="checkbox" class="absCheck"></input>\
        <button class="icon-caret-left" id="btnLeft" type="button" value="<"></button>\
	    {IMAGES}\
        <button class="icon-caret-right" id="btnRight" type="button" value=">"></button>\
        <span id="currentImg"></span> \
	    </div>\
	    <div class="bodyResult">\
		   {TABLES}\
	    </div>';
};


// Objet permettant d'interagir avec le serveur
function eWidgetCartoController(container) {

    if (container == null)
        return

    var infoboxTemplate = '\
    <div class="exPointer">\
        <div class="ex_infoBulle">\
	        <div class="entete_infoBulle" title=\'{ImageTitle}\' style="background-image: url(\'{ImageSource}\');">\
		        <input type="checkbox" {Selected}></input>\
		        <div title=\'{Title}\' class="prix_infoBulle">{Title}</div>\
	        </div>\
	        <div class="content_infoBulle">\
		        <span title=\'{SubTitle}\' class="adresse_infoBulle">{SubTitle}</span>\
                <div id="propriInfo" style="max-height:25px;">\
                    <div title=\'{Fields}\' id="innerHeight">{Fields}</div>\
                    <span id="overflowText">...</span>\
                </div>\
	        </div>\
        </div>\
    </div>\
';
    // contenur du widget
    var _container = container;
    var _config;
    var _selectionConfig;
    var _filterMenu;
    var _mapAPI;
    var _cardMenu;
    var _results = {};
    var _filters = [];

    function attachMenuEvents(menu, thumbtack, menuCarto, menuHover) {
        if (!menu || !thumbtack || !menu)
            return;

        var handler = null;

        // menu ouvert
        setEventListener(thumbtack, "click", function (evt) {
            var i = evt.srcElement || evt.target;
            if (i) {
                var val = pin(i) ? "1" : "0";
                setAttributeValue(menu, "pinned", val);
                setAttributeValue(menu, "hover", val);
            }
        });

        // menu caché
        setEventListener(menuCarto, "mouseover", function (e) {
            setAttributeValue(menu, "hover", "1");
        }, true);

        // dans contenu menu ouvert
        setEventListener(menuHover, "mouseover", function (e) {
            if (handler != null)
                clearTimeout(handler);
            setAttributeValue(menu, "hover", "1");
        }, true);

        // dans contenu menu ouvert
        setEventListener(menuHover, "mouseout", function (e) {
            if (!mouseIn(e, menuHover))
                handler = setTimeout(function () { setAttributeValue(menu, "hover", "0"); }, 700);
        });
    }
    function attachBtnEvents() {
        var btnApply = _container.querySelector("#applyButton");
        setEventListener(btnApply, "click", function (e) {

            if (!_filterMenu) {
                setTimeout(showWarning( eTools.getRes(307), eTools.getRes(644), "").hide, 2000);                
                return;
            }

            if (!_mapAPI) {
                setTimeout(showWarning(eTools.getRes(307), eTools.getRes(644), "").hide, 2000);
                return;
            }

            _filters = _filterMenu.getCriteria();
            if (_filters == null) {
                _filters = [];
            }

            _cardMenu.resetAll();
            loadData();

        }.bind(this), true);

        var btnSelection = _container.querySelector("#btnSelection button");
        setEventListener(btnSelection, "click", function () { _cardMenu.switchSelection(); });

        var btnBackAll = _container.querySelector("#backAll button");
        setEventListener(btnBackAll, "click", function () { _cardMenu.switchSelection(); });

        var btnSelectAll = _container.querySelector("#inputAll input");
        setEventListener(btnSelectAll, "click", function () { _cardMenu.selectAll(); });

        var applySelection = _container.querySelector("#selectionButton");
        setEventListener(applySelection, "click", function (e) { createSelection(); }.bind(this), true);
    }
    function loadData() {

        if (!_mapAPI)
            return;

        _mapAPI.Reset();

        // Rectangle de la carte
        var mapBounds = _mapAPI.GetMapBounds();
        var mapCenter = _mapAPI.GetMapCenter();

        var handler = showWaitDialog(eTools.getRes(307), eTools.getRes(642));
        
        load({
            WidgetId: _filterMenu.widgetId,
            Action: "apply-filter",
            Criteria: {
                Filters: _filters,
                MapBounds: mapBounds,
                MapCenter: mapCenter
            }
        },
        function (result) {
            handler.hide();
            criteriaReturn(result);
        });
    }
    function criteriaReturn(data) {

        _results = [];
        var sources = []
        var j = 0;
        for (var i = 0; i < data.Results.length; i++) {
            var record = data.Results[i];
            if (!record.Infobox || !record.Infobox.Geography || record.Infobox.Geography == "")
                continue;
            j++;
            _results[record.FileId] = record;
            sources.push({
                metadata: {
                    "id": record.FileId,
                    "selected": false,
                    "visited": false,
                    "tab": record.SourceTab
                },
                "geo": record.Infobox.Geography
            });

            if (j == data.MaxRecord)
                break;
        }

        if (j == 0) {
            showWarning(eTools.getRes(595), eTools.getRes(2056), "");
            return;
        }

        var msg, detail = "";
        if (data.Results.length > data.MaxRecord) {
            msg = eTools.getRes(2057).replace("{{COUNT}}", j);
            detail = eTools.getRes(2058).replace("{{MAX_RECORD}}", data.MaxRecord)
            showWarning(eTools.getRes(595), msg, detail);
        }
        else {
            msg = eTools.getRes(2059).replace("{{COUNT}}", j);
            // si tout va bien, Message d'info qui se ferme au bout de 2 secondes
            setTimeout(eAlert(3, eTools.getRes(595), msg, detail).hide, 2500);
        }

        _mapAPI.Display(sources);

        // On ferme le menu des filtres s'il y a un résultat
        var pinElm = _container.querySelector("#cartoTitle i[pinned='1']");
        if (pinElm) {
            var menu = _container.querySelector("#cartoLeft");
            var val = pin(pinElm) ? "1" : "0";
            setAttributeValue(menu, "pinned", val);
            setAttributeValue(menu, "hover", val);
        }
        _mapAPI.AutoMapZoom();

    }
    function createSelection() {

        var selected = _cardMenu.getSelectedCards();
        if (selected.length == 0) {
            top.eAlert(2, top._res_8018, eTools.getRes(2053));
            return;
        }

        // Valeur par défaut du champ notes
        var resumeKey = _selectionConfig.Options.FilterResumeDescId;
        var resumeValue = _filterMenu.buildResume(_selectionConfig.Options.FilterResumeBreakline);
        var defVal = {};
        if (resumeValue != null)
            defVal[resumeKey] = resumeValue;

        showAdvPopup({
            "tab": _selectionConfig.SelectionTab.DescId,
            "fileId": 0,
            "defaultValues": JSON.stringify(defVal),
            "fileLoad": function (t) { console.log(t); },
            "afterSave": function (result) { afterSelectionSave(result, selected); }
        });
    }
    function afterFileLoad(win) { }
    function afterSelectionSave(result, selected) {

        if (!result.success) {
            top.eAlert(2, top._res_8018, eTools.getRes(2054));
            console.log(result.data);
            return;
        }

        if (result.fid == 0) {
            top.eAlert(2, top._res_8018, eTools.getRes(2055));
            return;
        }

        load({
            WidgetId: _filterMenu.widgetId,
            Action: "save-selection",
            Selection: {
                ParentSelectionId: result.fid,
                Selections: selected
            }
        }, function (data) {
            saveSelectionReturn(data, result);
        });
    }
    function saveSelectionReturn(data, result) {

        if (!data.Success) {
            top.eAlert(2, data.ErrorTitle, data.ErrorMsg);
            return;
        }
        var msg = joinString("<br />", data.Results);
        top.eAlert(3, eTools.getRes(2045), msg, "");
        //#fbbc05
        top.loadFile(result.tab, result.fid, 3);
    }
    function pin(elm) {

        var pinned = getAttributeValue(elm, "pinned") == "1";
        if (pinned) {
            setAttributeValue(elm, "pinned", "0");
            return false;
        } else {
            setAttributeValue(elm, "pinned", "1");
            return true;
        }
    }
    function mouseIn(e, menuLeft) {
        var oMouse = GetTip(e);
        var oMenu = getAbsolutePosition(menuLeft);
        return (oMouse && oMenu &&
            oMouse.x >= oMenu.x && oMouse.x <= (oMenu.x + oMenu.w) &&
            oMouse.y >= oMenu.y && oMouse.y <= (oMenu.y + oMenu.h));
    }
    function loadCartoLeft() {
        var id = getAttributeValue(_container, "fid") * 1;
        if (id == 0)
            return;

        load({
            WidgetId: id,
            Action: "get-config",
            Criteria: null
        }, cartoLeftReturn);
    }
    function cartoLeftReturn(result) {

        _selectionConfig = result.Selection;
        _config = result.CriteriaConfig;
        var wid = getAttributeValue(_container, "fid") * 1;
        if (wid == 0)
            return;

        var cartoFilters = _container.querySelector("#cartoFilters");
        cartoFilters.innerHTML = "";
        _filterMenu = Object.create(MenuBuilder);
        _filterMenu.init(wid);
        _filterMenu.render(_config, cartoFilters);

        var cartoRight = _container.querySelector("#cartoRight");
        _cardMenu = Object.create(CardMenu);
        _cardMenu.init(wid, cartoRight);      

        // chargement de la carte
        var mapContainer = _container.querySelector("#mapContainer");
        if (mapContainer) {
            var iframe = document.createElement("iframe");
            setAttributeValue(iframe, "src", "eCartography.aspx");
            setEventListener(iframe, "load", function (evt) {
                var frame = evt.srcElement || evt.target
                var content = frame.contentWindow;
                var BingAPI = content.BingAPI;
                BingAPI.Events.On("map-loaded", function () {
                    _mapAPI = BingAPI;
                    _mapAPI.LocateFromName(_selectionConfig.Options.DefaultMapLocationName);

                    // cocher la mini-fiche, coche l'infobulle si ouverte
                    _cardMenu.onCardSelectionChanged(function (data) {
                        _mapAPI.Infobox.Select({ "id": data.id, "selected": data.selected });
                    });
                });
                BingAPI.Events.On("map-filtered", function (pins) {
                    log("Map filtred");
                    for (var i = 0; i < pins.length; i++) {
                        var record = _results[pins[i].metadata.id];
                        if (record) {
                            _cardMenu.insert(pins[i].metadata.id, record.Card);
                        }
                    }
                });
                BingAPI.Events.On("pushpin-clicked", function (pin) {
                    log("Pushpin " + pin.metadata.id + " clicked");
                    var data = {
                        'id': pin.metadata.id,
                        'html': infoboxTemplate,
                        'location': pin.location,
                        'setEvents': function (infobox) {
                            if (infobox) {
                                var cbo = infobox.querySelector("input[type='checkbox']");
                                setEventListener(cbo, "click", function (e) {
                                    _cardMenu.setCard({ 'id': pin.metadata.id, 'selected': cbo.checked });
                                    BingAPI.SelectPin({ 'id': pin.metadata.id, 'selected': cbo.checked });
                                    log("Checkbox " + pin.metadata.id + " clicked");
                                });

                                var height = infobox.querySelector("#innerHeight").offsetHeight;
                                var maxHeight = infobox.querySelector("#propriInfo").offsetHeight + 5;
                                if (height > maxHeight) {
                                    infobox.querySelector("#overflowText").style.display = "block";
                                }
                                else {
                                    infobox.querySelector("#overflowText").style.display = "none";
                                }
                            }
                        }
                    }

                    var record = _results[pin.metadata.id];
                    if (record) {

                        var cardOptions = _cardMenu.getCard(pin.metadata.id);
                        data.html = data.html
                        .replace("{Selected}", cardOptions.selected ? "checked" : "unchecked")
                        .replace(/{Title}/g, record.Infobox.Title)
                        .replace(/{SubTitle}/g, record.Infobox.SubTitle)
                        .replace("{ImageSource}", encode(record.Infobox.ImageSource))
                        .replace("{ImageTitle}", record.Infobox.ImageTitle)
                        .replace(/{Fields}/g, joinString(" | ", record.Infobox.Fields));


                        _cardMenu.insert(record.FileId, record.Card);

                        BingAPI.Infobox.Show(data);
                    }

                });
                BingAPI.Events.On("pushpin-hovered", function (pin) {
                    log("Pushpin " + pin.metadata.id + " hovered");
                });
                BingAPI.Events.On("map-reset", function () {
                    _cardMenu.resetUnselected();
                    _results = {};
                });
                BingAPI.Events.On("data-refresh", function () {
                    loadData();
                });
                BingAPI.Init();
            });

            mapContainer.appendChild(iframe);
        }


    };
    function log(msg) {
        console.log(msg);
    }
    function load(cartoRequest, callback) {

        var defaultCartoRequest = {
            WidgetId: 0,
            Action: "",
            Criteria: null
        }

        cartoRequest.WidgetId = cartoRequest.WidgetId || defaultCartoRequest.WidgetId;
        cartoRequest.Action = cartoRequest.Action || defaultCartoRequest.Action;
        cartoRequest.Criteria = cartoRequest.Criteria || defaultCartoRequest.Criteria;

        // On envoi la nouvelle config
        var upd = new eUpdater("mgr/eCartoSelectionManager.ashx", 1);
        upd.json = JSON.stringify(cartoRequest);;

        upd.ErrorCallBack = function () {
            var cartoFilters = _container.querySelector("#cartoFilters");
            cartoFilters.innerHTML = eTool.getRes(7164);// Erreur inconnue
        };
        upd.send(function (oRes) {
            if (oRes == null) {
                top.eAlert(2, eTools.getRes(72), eTools.getRes(2052));
                return;
            }

            var result = JSON.parse(oRes);
            if (!result.Success) {
                var cartoFilters = _container.querySelector("#cartoFilters");
                cartoFilters.innerHTML = result.ErrorMsg;
                return;
            }

            callback(result);
        });
    }
    function init() {

        // Menu de gauche
        var menuLeft = _container.querySelector("#cartoLeft");
        var menuLeftHover = _container.querySelector("#cartoLeft .content-param-carto");
        var thumbtackLeft = _container.querySelector("#cartoTitle i");
        var menuCartoLeft = _container.querySelector("#menuCartoLeft i");
        attachMenuEvents(menuLeft, thumbtackLeft, menuCartoLeft, menuLeftHover);

        // menu de droite
        var menuRight = _container.querySelector("#cartoRight");
        var menuRightHover = _container.querySelector("#cartoRight .content-param-carto");
        var thumbtackRight = _container.querySelector("#displayResultNumber i");
        var menuCartoRight = _container.querySelector("#menuCartoRight i");
        attachMenuEvents(menuRight, thumbtackRight, menuCartoRight, menuRightHover);

        attachBtnEvents();
        loadCartoLeft();
    }
    init();
};

