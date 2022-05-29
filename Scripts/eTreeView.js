/*
# info

TreeFolder v1.0.
(c) Copyright 2007 by Ron Valstar, Shapers
http://www.shapers.nl/
http://www.sjeiti.com/

# disclaimer

Permission is hereby granted, free of charge, to any person obtaining a
copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
ITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.

# use

This class will adapt your tree in such a way that you can expand and collapse the tree by clicking the anchors in that tree. To work correctly it requires the tree to be structured in the following way:

<ul class="foldtree">
<li><a href="#">link</a></li>
<li>
<a href="#">link</a>
<ul>
<li>
<a href="#">link</a>
<ul>
<li><a href="#">link</a></li>
<li><a href="#">link</a></li>
</ul>
</li>
<li><a href="#">link</a></li>
<li><a href="#">link</a></li>
</ul>
</li>
</ul>

Add the script in the header of your page. It will search the page for unordered lists that contain the classname 'foldtree'.

There are two additional features.

You can fold the initial state by adding the classname 'folded' to the unordered list. If you add a number to the classname the tree will only be initially folded from that depth.

<ul class="foldtree folded"> <!-- initial state folded from depth 0 -->
<ul class="foldtree folded1"> <!-- initial state folded from depth 1 -->
<ul class="foldtree folded9"> <!-- initial state folded from depth 9 -->

Adding the classname 'onebranch' will only allow one branch to be unfolded. However, this does not count for the initial state (this is not a real choice, I was just too lazy to build that in).

<ul class="foldtree folded1 onebranch">

*/
///////////

// MAB - l'initialisation se fera à la demande de la page appelante uniquement
// MAB - classes paramétrables
//addEvent(window, "load", initTreeView);
function eTreeView(jsVarName, openClassName, closedClassName, subBranchClassName, selectedClassName, foldTreeClassName, foldedClassName, foldIconClassName, oneBranchClassName, pageClassName) {
    var that = this; // pointeur vers l'objet eFieldEditor lui-même, à utiliser à la place de this dans les évènements onclick (ou this correspond alors à l'objet cliqué)

    this.jsVarName = jsVarName;
    this.oSelectedPage = null;

    this.foldTreeClassName = "eTVRoot";
    this.foldedClassName = "eTVF";
    this.foldIconClassName = "eTVI";
    this.oneBranchClassName = "eTVSolo";
    this.pageClassName = "eTVP";
    this.OpenfoldIconClassName = "eTVFI";
    this.openClassName = "icon-folder-open";
    this.closedClassName = "icon-folder";
    this.selectedClassName = "eTVS";

    //this.openIndicatorClassName = 'eTVIO';
    //this.closedIndicatorClassName = 'eTVIC';
    this.openIndicatorClassName = 'icon-unvelop';
    this.closedIndicatorClassName = 'icon-develop';

    this.collaspeClassName = "eTVcol";
    this.subBranchClassName = 'icon-signet-fiche'; // anciennement : eTVSB
    this.LeftCrossLineClassName = 'eTV_LCL';    //Ligne d'arbo gauche de croisement
    this.LeftBottomCornerLineClassName = 'eTV_LBCL';    //Ligne d'arbo gauche de coin bas
    this.MiddleLineClassName = 'eTV_MCL';    //Ligne d'arbo gauche verticale

    this.bUncollapse = false;

    if (typeof (foldTreeClassName) != 'undefined') { this.foldTreeClassName = foldTreeClassName; }
    if (typeof (foldedClassName) != 'undefined') { this.foldedClassName = foldedClassName; }
    if (typeof (foldIconClassName) != 'undefined') { this.foldIconClassName = foldIconClassName; }
    if (typeof (oneBranchClassName) != 'undefined') { this.oneBranchClassName = oneBranchClassName; }
    if (typeof (pageClassName) != 'undefined') { this.pageClassName = pageClassName; }
    if (typeof (openClassName) != 'undefined') { this.openClassName = openClassName; }
    if (typeof (closedClassName) != 'undefined') { this.closedClassName = closedClassName; }
    if (typeof (subBranchClassName) != 'undefined') { this.subBranchClassName = subBranchClassName; }
    if (typeof (selectedClassName) != 'undefined') { this.selectedClassName = selectedClassName; }

    // MAB - l'initialisation se fera après déclaration de toutes les méthodes/fonctions
    //this.init();

    //////////////
    // nothing //
    //function nothing() {
    //}

    ///////////////////
    // addClassName //
    this.addClassName = function (oNode, sClass, bAdd) {
        if (bAdd == null) bAdd = true;
        var aClass = oNode.className.split(" ");
        var iPos = -1;
        for (var i = 0; i < aClass.length; i++) {
            if (aClass[i] == sClass) {
                iPos = i;
                break;
            }
        }
        if (bAdd && iPos == -1) aClass.push(sClass);
        else if (!bAdd && iPos >= 0) aClass.splice(iPos, 1);
        oNode.className = aClass.join(" ");
    };

    //////////////////
    // isClassName //
    this.isClassName = function (oNode, sClass) {
        return (" " + oNode.className + " ").indexOf(" " + sClass + " ") != -1;
    };

    ///////////////
    // addEvent //
    /*
    this.addEvent = function (obj, type, fn) {
        if (obj.attachEvent) {
            obj['e' + type + fn] = fn;
            obj[type + fn] = function () { obj['e' + type + fn](window.event); }
            obj.attachEvent('on' + type, obj[type + fn]);
        } else obj.addEventListener(type, fn, false);
    };
    this.removeEvent = function (obj, type, fn) {
        if (obj.detachEvent) {
            obj.detachEvent('on' + type, obj[type + fn]);
            obj[type + fn] = null;
        } else obj.removeEventListener(type, fn, false);
    };
    */

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    ////////////////
    // construct //
    //var oSelectedPage;
    // MAB - l'initialisation se fera à la demande de la page appelante uniquement
    //var eTreeView = new function() {
    //}

    ///////////
    // init //
    this.init = function (bUncollapse) {
        // Find all unordered lists with classname 'this.foldTreeClassName' and make them foldable
        if (typeof (bUncollapse) != 'undefined')
            this.bUncollapse = bUncollapse;
        else
            this.bUncollapse = true; // Par défaut, items déroulés

        if (!document.getElementsByTagName) return;
        var aUls = document.getElementsByTagName("ul");
        var bIsLast = false;
        for (var i = 0; i < aUls.length; i++) {
            bIsLast = ((i + 1) == aUls.length);
            if (this.isClassName(aUls[i], this.foldTreeClassName)) {
                //var bFold = this.isClassName(aUls[i], this.foldedClassName);
                var iFold = -1;
                var aClass = aUls[i].className.split(" ");
                for (var j = 0; j < aClass.length; j++) {
                    if (aClass[j].indexOf(this.foldedClassName) != -1) {
                        iFold = aClass[j].length == 6 ? 0 : parseInt(aClass[j].substring(6));
                        break;
                    }
                }
                this.prepare(aUls[i], iFold, this.isClassName(aUls[i], this.oneBranchClassName), -1, bIsLast);
            }
        }
    };

    //////////////
    // prepare //
    this.prepare = function (oElement, iFold, bOne, iDepth, bIsLast) {
        if (iDepth == null) var iDepth = -1;
        iDepth++;
        var bFold = iFold >= 0 ? Math.floor(iDepth / 2) >= iFold : false;

        var iUl = -1;
        var iLi = -1;
        var iA = -1;
        var iSpan = -1;
        var bIsLastChild = false;
        for (var i = 0; i < oElement.childNodes.length; i++) {
            bIsLastChild = ((i + 1) == oElement.childNodes.length);
            var oChild = oElement.childNodes[i];
            switch (oChild.nodeName.toLowerCase()) {
                case "ul":
                    iUl = i;

                    bFold = !this.bUncollapse && (!this.isClassName(oChild, this.collaspeClassName));
                    this.prepare(oChild, iFold, bOne, iDepth, bIsLastChild);
                    if (bFold && iDepth != 1) oChild.style.display = "none";
                    break;
                case "li":
                    iLi = i;
                    this.prepare(oChild, iFold, bOne, iDepth, bIsLastChild);
                    break;
                default:
                    iA = i;
                    break;
            }
        }
        // adjust existing anchor
        var oA;
        if (iA >= 0) {
            var oA = oElement.childNodes[iA];
            //GCH : Classe permettant l'espacement vers la droite pour la checkbox
            this.addClassName(oA, this.pageClassName, true);
        }
        if (iLi >= 0) {
            oElement.className = oElement.className;

        }

        if (iA >= 0 && iUl >= 0) { // insert extra anchor   //GCH : ajout du +/- permettant le pli ou dépliement de la branche.
            iOnclick = -1;
            var oUl = oElement.childNodes[iUl];
            for (var j = 0; j < oA.attributes.length; j++) {
                if (oA.attributes[j].nodeName.toLowerCase() == "onclick") {
                    iOnclick = j;
                }
            }
            if (!this.getFirstElementByAttribute(oElement, "class", (bFold && iDepth != 1 ? this.closedIndicatorClassName : this.openIndicatorClassName))) {
                // Ajout du plier/déplier
                var oAfold = document.createElement("a");
                this.addClassName(oAfold, this.foldIconClassName, true);

                oAfold.innerHTML = '<span class="' + (bFold && iDepth != 1 ? this.closedIndicatorClassName : this.openIndicatorClassName) + '">&nbsp;&nbsp;&nbsp;&nbsp;</span> '; //Math.floor(iDepth/2)+" "+iFold+" "+(bFold?1:0)+

                oElement.insertBefore(oAfold, oA);
                setEventListener(oAfold, "click", function () { that.liAClicked(oAfold, bOne); });
                
                // Ajout du repertoire Ouvert/Fermé
                var oSpanDir = document.createElement("span");
                this.addClassName(oSpanDir, (bFold && iDepth != 1) ? this.closedClassName : this.openClassName, true);
                oSpanDir.innerHTML = "    ";
                oElement.insertBefore(oSpanDir, oA);
            }
        }
        else if ((oElement.getElementsByTagName("a").length <= 1) && (oElement.nodeName != "UL")) {
            // Création de la branche de coin bas gauche et de croisement verticale
            var oSpanfold = document.createElement("span");
            this.addClassName(oSpanfold, ((!bIsLast) ? this.LeftCrossLineClassName : this.LeftBottomCornerLineClassName), true);
            oSpanfold.innerHTML = "    ";
            if (oElement.childNodes.length > 0)
                oElement.insertBefore(oSpanfold, oElement.childNodes[0]);
            else
                oElement.appendChild(oSpanfold);
        }

        // MAB - ajout d'une classe sur les branches sans enfant
        if (iA >= 0 && iUl < 0) {
            var oSpanDir = document.createElement("span");
            var currentID = oElement.id;
            var gPos = -1;
            if (currentID)
                gPos = currentID.toString().indexOf("_G");

            //GCH specificité du cat User le bas de la branche peut être un Groupe donc on met le style du groupe dans ce cas
            this.addClassName(oSpanDir, ((gPos < 0) ? this.subBranchClassName : openClassName), true);

            oSpanDir.innerHTML = "    ";

            if (oElement.childNodes.length > 0)
                oElement.insertBefore(oSpanDir, oElement.childNodes[0]);
            else
                oElement.appenChild(oSpanDir);
        }
        if (iUl >= 0 && !bIsLast) {
            this.addClassName(oElement, this.MiddleLineClassName, true);
        }

    };

    //////////////////
    // activateALi //
    this.liAClicked = function (oAnchor, bOne) {
        var oLi = oAnchor.parentNode;
        var aSibling = oLi.childNodes;
        var bDisplay = false;
        for (var i = 0; i < aSibling.length; i++) {
            var oSibling = aSibling[i];
            if (oSibling.nodeName.toLowerCase() == "ul") {
                bDisplay = oSibling.style.display == "none";
                oSibling.style.display = bDisplay ? "block" : "none";
            }
            var oSpan = oAnchor.getElementsByTagName("span")[0];
            this.addClassName(oSpan, this.openIndicatorClassName, bDisplay);
            this.addClassName(oSpan, this.closedIndicatorClassName, !bDisplay);
        }
        if (bOne) {
            var aLiSibling = oLi.parentNode.childNodes;
            for (var i = 0; i < aLiSibling.length; i++) {
                if (aLiSibling[i] != oLi && aLiSibling[i].nodeName.toLowerCase() == "li") {
                    var aLiSiblingChild = aLiSibling[i].childNodes;
                    var oFoldIcon;
                    var bFold = false;
                    for (var j = 0; j < aLiSiblingChild.length; j++) {
                        switch (aLiSiblingChild[j].nodeName.toLowerCase()) {
                            default: if (this.isClassName(aLiSiblingChild[j], this.foldIconClassName)) oFoldIcon = aLiSiblingChild[j]; break;
                            case "ul": if (aLiSiblingChild[j].style.display == "block") bFold = true; break;
                        }
                    }
                    if (bFold) that.liAClicked(oFoldIcon);
                }
            }
        }
        //Met l'image de fermeture de dossier s'il est ouvert et inversion s'il est fermé.
        var oSpanImageFolder = this.getFirstElementByAttribute(oAnchor.parentNode, "class", this.openClassName);
        if (!oSpanImageFolder)
            oSpanImageFolder = this.getFirstElementByAttribute(oAnchor.parentNode, "class", this.closedClassName);
        if (oSpanImageFolder) {
            this.addClassName(oSpanImageFolder, this.openClassName, bDisplay);
            this.addClassName(oSpanImageFolder, this.closedClassName, !bDisplay);
        }
    };
    //Récupère le premier enfant qui confitent l'attribue avec la valeur définit en paramètre.
    this.getFirstElementByAttribute = function (oElement, attribute, paramValue) {
        for (var i = 0; i < oElement.childNodes.length; i++) {
            try {
                var oChild = oElement.childNodes[i];
                if (oChild) {
                    var Values = oChild.getAttribute(attribute);
                    if (Values) {
                        var ListValue = Values.split(" ");
                        for (var j = 0; j < ListValue.length; j++) {
                            if (ListValue[j] == paramValue) {
                                return oChild;
                            }
                        }
                    }
                }
            }
            catch (e) { }

        }
        return null;
    }
    //////////////////
    // activateALi //
    this.pageSelect = function (oAnchor) {
        if (this.oSelectedPage != null) this.addClassName(this.oSelectedPage, this.selectedClassName, false);
        this.addClassName(oAnchor, this.selectedClassName, true);
        this.oSelectedPage = oAnchor;
        oAnchor.blur();
    };


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    this.init();

}