function FieldsSelect() {

    var selectedElement = null;
    var origSelectedFields = "";
    var NB_MAX_SELECTED = 20;

    // Retourne la liste des rubriques sélectionnées sous la forme "101;102;103"
    this.getSelectedFieldsList = function () {
        return getSelectedFields();
    }
    // Retourne la liste des rubriques sélectionnées d'origine
    this.getOrigSelectedFieldsList = function () {
        return origSelectedFields;
    }

    function getSelectedFields() {
        var descidList = "";
        var list = document.querySelectorAll("#selectedFields ul li");
        for (var i = 0; i < list.length; i++) {
            if (getAttributeValue(list[i], "did") != "") {
                descidList = descidList + getAttributeValue(list[i], "did") + ";";
            }
        }
        return descidList;
    }

    // Sélection d'une rubrique
    function selectField(element) {
        var selectedItem = element.parentElement.querySelector(".selectedItem");
        eTools.RemoveClassName(selectedItem, "selectedItem");
        eTools.SetClassName(element, "selectedItem");
        selectedElement = element;
    }

    // Transférer toutes les rubriques d'une liste à l'autre
    function transferAllFields(btn, bToRight) {
        var fields = btn.parentElement.querySelectorAll(".choiceTable ul li[data-active='1']");

        if (fields.length > 0) {
            // On limite le nombre de rubriques lorsqu'on passe de gauche à droite
            var dest;
            var max = fields.length;
            if (bToRight) {
                dest = document.querySelector("#selectedFields ul");
                if (dest.childNodes.length < NB_MAX_SELECTED) {
                    max = NB_MAX_SELECTED - dest.childNodes.length;
                    var labelTitle = top._res_8468; //Sélection de rubriques
                    var message = top._res_8469.replace("<NB_MAX>", NB_MAX_SELECTED); //Le nombre de rubriques sélectionnées est limité à <NB_MAX>
                    eAlert(3, labelTitle, message);
                }
            }

            for (var i = 0; i < max; i++) {
                transferField(fields[i], false, true);
            }
            refreshAltRows();
        }
        
    }

    // Transfère une rubrique d'une liste à l'autre
    function transferField(element, bRefresh, bFromTransferAllFields) {
        var elementToSelect = element;
        var tableParent = element.parentElement.parentElement.parentElement;
        var dest;

        if (tableParent.id == "availableFields") {
            dest = document.querySelector("#selectedFields ul");
            if (dest.childNodes.length > NB_MAX_SELECTED && !bFromTransferAllFields) {
                var labelTitle = top._res_8468; //Sélection de rubriques
                var message = top._res_8469.replace("<NB_MAX>", NB_MAX_SELECTED); //Le nombre de rubriques sélectionnées est limité à <NB_MAX>
                eAlert(2, labelTitle, message);
                return;
            }
        }
        else {
            dest = document.querySelector("#availableFields ul");
        }

        element.parentNode.removeChild(element);

        var selectedItem = dest.querySelector(".selectedItem");
        if (selectedItem != null) {
            dest.insertBefore(elementToSelect, selectedItem.nextSibling);
            eTools.RemoveClassName(selectedItem, "selectedItem");
        }
        else {
            dest.appendChild(elementToSelect);
        }

        if (bRefresh)
            refreshAltRows();
    }

    // Rafraîchissement des couleurs de lignes
    function refreshAltRows() {
        var table, li;
        var count;
        var tables = document.getElementsByClassName("choiceTable");
        for (var i = 0; i < tables.length; i++) {
            count = 0;
            table = tables[i];
            li = table.querySelectorAll("li");
            for (var j = 0; j < li.length; j++) {
                if (count == 0) {
                    eTools.RemoveClassName(li[j], "line1");
                    count = 1;
                }
                else {
                    eTools.SetClassName(li[j], "line1");
                    count = 0;
                }
                    
            }
        }
    }

    // Initialisation des événements
    this.load = function () {

        origSelectedFields = getSelectedFields();

        var field;
        var fields = document.querySelectorAll(".choiceTable li");
        for (var i = 0; i < fields.length; i++) {
            field = fields[i];
            field.addEventListener("dblclick", function () {
                transferField(this, true);
            });
            field.addEventListener("click", function () {
                selectField(this);
            });
            // Drag&Drop
            field.addEventListener("dragstart", function (ev) {
                ev.dataTransfer.setData("text", ev.target.id);
            })
        }

        // DRAG&DROP

        document.addEventListener("dragover", function (ev) {
            ev.preventDefault();
        })

        document.addEventListener("dragenter", function (ev) {
            if (ev.target.tagName == "LI" && ev.target.parentElement.className == "listFields")
                ev.target.style.borderBottom = "2px solid #37A7DE";
            if (ev.target.tagName == "UL" && ev.target.className == "listFields")
                ev.target.style.backgroundColor = "#BEDCFB";
        })

        document.addEventListener("dragleave", function (ev) {
            if (ev.target.tagName == "LI" && ev.target.parentElement.className == "listFields")
                ev.target.style.borderBottom = "";
            if (ev.target.tagName == "UL" && ev.target.className == "listFields")
                ev.target.style.backgroundColor = "";
        });

        document.addEventListener("drop", function (ev) {
            ev.preventDefault();
            var data = ev.dataTransfer.getData("text");
            if (ev.target.parentElement.tagName == "UL" && ev.target.parentElement.className == "listFields") {

                if (ev.target.tagName == "LI")
                    ev.target.style.borderBottom = "";

                if (ev.target.parentElement.parentElement.parentElement.id == "selectedFields" && ev.target.parentElement.childNodes.length > NB_MAX_SELECTED) {
                    eAlert(2, "Sélection de rubriques", "Le nombre de rubriques sélectionnées est limité à " + NB_MAX_SELECTED);
                    return;
                }
                ev.target.parentElement.insertBefore(document.getElementById(data), ev.target.nextSibling);

            }
            else if (ev.target.tagName == "UL" && ev.target.className == "listFields") {
                ev.target.appendChild(document.getElementById(data));
                ev.target.style.backgroundColor = "";
            }

            refreshAltRows();
        })


        // Boutons "tout sélectionner" et "tout déselectionner"
        var buttons = document.getElementsByClassName("btnSelectAll");
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].addEventListener("click", function () {
                var bToRight = (this.id == "btnSelectAll");
                transferAllFields(this, bToRight);
            });
        }

        // Boutons "gauche" et "droite"
        var btnBetween = document.getElementsByClassName("moveBetween");
        for (var i = 0; i < btnBetween.length; i++) {
            btnBetween[i].addEventListener("click", function () {
                if (selectedElement != null) {
                    transferField(selectedElement, true);
                }
            });
        }
        // Boutons "haut" et "bas"
        var btnUpDown = document.getElementsByClassName("moveUpDown");
        for (var i = 0; i < btnUpDown.length; i++) {
            btnUpDown[i].addEventListener("click", function () {
                if (selectedElement != null) {
                    if (this.id == "moveDown") {
                        selectedElement.parentElement.insertBefore(selectedElement.nextSibling, selectedElement);
                    }
                    else if (this.id == "moveUp") {
                        selectedElement.parentElement.insertBefore(selectedElement, selectedElement.previousSibling);
                    }
                    refreshAltRows();
                }
            });
        }

        // Recherche 
        

        var btnSearch = document.getElementById("txtSearchField");
        if (btnSearch) {
            btnSearch.addEventListener("keyup", function () {

                var value = this.value;

                if (value.length >= 2) {

                    btnRemoveSearch.style.display = "inline-block";

                    delay(function () {
                        var list = document.querySelectorAll("#availableFields ul li");
                        for (var i = 0; i < list.length; i++) {
                            setAttributeValue(list[i], "data-active", "1"); // On réactive la rubrique pour pouvoir tester le innerText juste après
                            if (list[i].innerText.toLowerCase().indexOf(value.toLowerCase()) < 0) {
                                setAttributeValue(list[i], "data-active", "0");
                            }
                        }
                        refreshAltRows();
                    }, 300);

                }
                else if (value == "") {
                    cancelSearch();
                }
            });
        }
       
        var btnRemoveSearch = document.getElementById("iconRemoveSearch");
        if (btnRemoveSearch) {
            btnRemoveSearch.addEventListener("click", function () {
                var textbox = document.getElementById("txtSearchField");
                textbox.value = "";
                cancelSearch();
                this.style.display = "none";
            });
        }
        

        var delay = (function () {
            var timer = 0;
            return function (callback, ms) {
                clearTimeout(timer);
                timer = setTimeout(callback, ms);
            };
        })();

    }

    function cancelSearch() {
        var list = document.querySelectorAll("#availableFields ul li");
        for (var i = 0; i < list.length; i++) {
            setAttributeValue(list[i], "data-active", "1");
        }
    }

    this.load();
}