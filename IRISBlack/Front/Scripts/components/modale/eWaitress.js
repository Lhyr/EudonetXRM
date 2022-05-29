export default {
    name: "eWaitress",
    data: function {
        return { oAutoCompletionWaiter: null}
    },
    methods: {
            /*addOrDelAttribute : 
            // oTarg : objet cible
            // sAttribute : attribut cible 
            // name : valeur à ajouter dans l'attribut
            // bAdd : Ajout/Suppression de la valeur dans l'attribut
            // bNoMerging : Si vrai on peux ajouter plusieurs fois la même valeur
            */
            addOrDelAttributeValue: function (oTarg, sAttribute, name, bAdd, bNoMerging) {
            if (name && name != "") {   //Que si une valeur est demandée
                var sCaller = oTarg.attributes[sAttribute];
                var nPos = (";" + sCaller + ";").indexOf(";" + name + ";");
                if ((bAdd) && (nPos < 0 || bNoMerging)) {   //Ajout de la valeur pour cet attribut : si pas de fusion on vérifit qu'il n'est pas déjà présent
                    if (sCaller != "")
                        sCaller = sCaller + ";";
                    sCaller = sCaller + name;
                    oTarg.attributes[sAttribute] = sCaller;
                }
                else if (!bAdd && (nPos > 0)) {   //Suppression d'une des valeurs de l'attribut si présent
                    if (sCaller != "") {
                        sCaller = (";" + sCaller + ";").replace(";" + name + ";", ";;");
                        //Suppression des ; rajouté autour
                        sCaller = sCaller.substring(0, 1);
                        sCaller = sCaller.substring(sCaller.length - 2, sCaller.length - 1);
                        oTarg.attributes[sAttribute] = sCaller;
                    }
                }
            }
        },

        //Retourne le niveau max de zindex utilisé sur la page
        //oDoc (facultatif) : indique le document on l'on doit récupérer le zindex max
        //nBaseLevel (facultatif) : indique le zindex minimum souhaité
        //bIgnoreSetWait : Si a true, le z-index du setwait ne sera pas compté
         GetMaxZIndex : function(oDoc, nBaseLevel, bIgnoreSetWait) {
            if (!nBaseLevel)
                nBaseLevel = 1;
            if (!oDoc)
                oDoc = window.document;
            var allParentDocElements = oDoc.getElementsByTagName("*");
            for (var i = 0; i < allParentDocElements.length; i++) {
                var currentObj = allParentDocElements[i];
                if (getNumber(currentObj.style.zIndex) > nBaseLevel) {

                    if (bIgnoreSetWait) {
                        if (currentObj.id != "waiter")  //Si on ne doit pas tenir compte du setwait est selectionné on ignore l'index du setwait
                            nBaseLevel = getNumber(currentObj.style.zIndex);
                    }
                    else
                        nBaseLevel = getNumber(currentObj.style.zIndex);
                }
            }
            return nBaseLevel;
        },

    //Echange 2 classses 
     switchClass: function (elem, oldvalue, newvalue) {

        try {
            removeClass(elem, oldvalue);
        }
        catch (e) {

        }

        try {
            addClass(elem, newvalue);
        }
        catch (e) {
        }

    },

/// Affiche/Masque le div d'attente
        setWait: function (bOn) {

        var maxZIndex = 100;	//zIndex min à 100 pour iso à eModal
        if (oAutoCompletionWaiter) {
            this.addOrDelAttributeValue(oAutoCompletionWaiter, "caller", name, bOn, true);

            if (bOn) {
                clearTimeout(oAutoCompletionWaiterTime);

                this.switchClass(oAutoCompletionWaiter, "waitOff", "waitOn");
                oAutoCompletionWaiterContent.style.display = "";

                var zIndex = GetMaxZIndex(menuContainer, maxZIndex) + 1;
                oAutoCompletionWaiter.style.zIndex = zIndex;
            }
            else {
                oAutoCompletionWaiterTime = setTimeout(() => {
                    this.switchClass(oAutoCompletionWaiter, "waitOn", "waitOff");
                    oAutoCompletionWaiterContent.style.display = "none";
                }, 1);
            }
        }
    }
    },
    template: `
    <div id="waiterIris" class="waitOn" style="z-index: 108;"></div>
`
}