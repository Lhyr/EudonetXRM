//*****************************************************************************************************//
//*****************************************************************************************************//
//*** JBE - 12/2011 - Affichage des menu contextuels
//*** Nécessite eTools.js
//*****************************************************************************************************//
//*****************************************************************************************************//

function eContextMenu(width, nTop, left, handle, parentPopup, sCustomCssClass) {
    var windowSize = getWindowSize();

    this.parentPopup = parentPopup;
    this.childElement = null;
    this.items = new Array();
    this.uId = randomID(10);
    this.width = width;
    this.top = nTop;
    this.origTop = nTop;
    this.topMenuUl = null;
    this.middleMenuUl = null;
    this.bottomMenuUl = null;
    this.IsContextMenu = true;
    this.Handle = handle;
    this.customCssClass = sCustomCssClass;
    var that = this;

    if (!parentPopup) {
        if (!ePopupObject)
            ePopupObject = new ePopup('ePopupObject', 220, 250, 0, 0, document.body, false);
        this.parentPopup = ePopupObject;
    }

    this.mainDiv = this.parentPopup.div;
    this.parentPopup.sourceElement = null;
    this.parentPopup.openerElement = null;
    if (this.parentPopup.isShown)
        this.parentPopup.hide();

    //Ajout de le menu contextuel dans le tableau global des contextuel
    if (typeof handle != "undefined") {
        if (top) {
            if (!top.window['_cm'])
                top.window['_cm'] = [];

            top.window['_cm'][handle] = this;
        }
    }
    var FIXED_WIDTH = 280;

    if (!this.width)
        this.width = FIXED_WIDTH;


    if (left + this.width >= windowSize.dw)
        left = windowSize.dw - this.width - 6;

    this.parentPopup.left = left;
    this.parentPopup.top = this.top;

    this.parentPopup.width = this.width;
    //document.body.appendChild(this.mainDiv);

    this.topMenuUl = document.createElement("ul");
    this.parentPopup.div.appendChild(this.topMenuUl);
    this.middleMenuUl = document.createElement("ul");
    this.parentPopup.div.appendChild(this.middleMenuUl);
    this.bottomMenuUl = document.createElement("ul");
    this.parentPopup.div.appendChild(this.bottomMenuUl);
    //active le fait de décaller vers le haut la popup si elle sort de la fenêtre.
    this.autoAdjust = true;
    /*KHA - Affiche le menu contextuel vers le haut s'il n'y a pas la place vers le bas*/
    this.adjVertPos = function () {
        if (this.autoAdjust) {
            var oPopUpSize = getAbsolutePosition(this.mainDiv);
            if (oPopUpSize) {
                if (oPopUpSize.y + oPopUpSize.h > windowSize.h)
                    this.top = windowSize.h - oPopUpSize.h;
                this.parentPopup.top = this.top;
                this.parentPopup.div.style.top = this.parentPopup.top + "px";
            }
        }
    };

    //To : 0 : ajout dans le Top
    //     1 : ajout dans le Middle
    //     2 : ajout dans le Bottom
    this.getTarget = function (to) {
        var target = null;
        switch (to) {
            case 0:
                target = this.topMenuUl;
                break;
            case 1:
                target = this.middleMenuUl;
                break;
            case 2:
                target = this.bottomMenuUl;
                break;
        }
        return target;

    }



    this.addLabel = function (label, to, niveau, cssClass) {

        var nRet = 0;
        if (typeof (niveau) == "number") {
            nRet = niveau * 20;
        }

        var liMenu = document.createElement('li');
        var target = this.getTarget(to);
        if (target == null)
            return;

        addClass(liMenu, cssClass);
        if (nRet > 0) {
            liMenu.style.paddingLeft = nRet + "px";
        }

        target.appendChild(liMenu);
        liMenu.innerHTML = label;

        this.adjVertPos();

    }



    /// <summary>
    /// Ajoute une entrée sur le menu contextuel
    /// </summary>
    /// <param name="html">Libellé de l'entrée</param>
    /// <param name="jsAction">Texte de la fonction sur le onclick</param>
    /// <param name="to">?</param>
    /// <param name="niveau">Décalage</param>
    /// <param name="cssClass">Class css a appliquer</param>
    /// <param name="toolTipText">Tooltip sur l'entrée</param>
    /// <param name="addedAttribute">Autre attribut (sous forme text : 'key0=value0&key1=value1')</param>
    /// <returns></returns>
    this.addItem = function (html, jsAction, to, niveau, cssClass, toolTipText, addedAttribute) {

        // Pour matérialiser un "niveau" d'item, on simule des retraits similaires aux tabulations d'une liste à puces
        // en multipliant le niveau indiqué par un facteur, donnant la taille du retrait en pixels
        var nRet = 0;
        if (typeof (niveau) == "number") {
            nRet = niveau * 6;
        }

        var liMenu = document.createElement('li');

        if (jsAction != null)
            liMenu.onclick = new Function(jsAction);
        var target = this.getTarget(to);
        if (target == null)
            return;

        addClass(liMenu, cssClass);

        if (toolTipText != null)
            liMenu.title = toolTipText;
        else
            liMenu.title = removeHTML(html);

        target.appendChild(liMenu);

        //Attribut
        if (typeof addedAttribute == 'string' && addedAttribute.length > 0) {

            //
            var aAtts = addedAttribute.split("&");

            //boucle sur tous les fields
            for (var idxAtt = 0; idxAtt < aAtts.length; idxAtt++) {
                var sAttribute = aAtts[idxAtt];
                if (sAttribute.indexOf('=') > 0) {

                    var aAttribute = sAttribute.split('=');
                    if (aAttribute.length == 2) {


                        liMenu.setAttribute(aAttribute[0], aAttribute[1]);


                    }
                }

            }
        }

        var liMenuChild = document.createElement('span');
        liMenuChild.innerHTML = html;
        // Application du retrait
        if (nRet > 0) {
            liMenuChild.style.paddingLeft = nRet + "px";

            // 40339 MCR  si le navigateur est IE alors j'applique le nouveau style , 
            //            pour les libelles de filtre trop long, la propriete text-overflow:ellipsis ne fonctionne pas
            //            application d un style pour la balise span : ExpressFilterspan dans le cas d un menu item de filtre express             
            var myBrowser = new getBrowser();
            if (myBrowser != null && myBrowser.isIE) {
                if (cssClass == "actionItem icon-list_filter")
                    addClass(liMenuChild, "ExpressFilterspan");
            }


        }

        liMenu.appendChild(liMenuChild);


        //// Ajout du tooltip par le biais du title pour les MRU.
        //liMenu.setAttribute("title", html);

        this.adjVertPos();

    };



    /// <summary>
    /// Ajoute un élément dans le menu
    /// </summary>
    /// <param name="label">Label de l'entrée</param>
    /// <param name="fct">Fonction a lancer sur le click</param>
    /// <param name="to">Bloc sur lequel ajouter l'entrée (0 : haut, 1: milieu, 2:bas) </param>
    /// <param name="niveau">Niveau de retrait de l'entrée (pour création de sous-niveau dans le menu)</param>
    /// <param name="cssClass">Classe css</param>
    /// <param name="toolTipText">Tool tip de l'entrée/param>
    /// <returns>Vrai si renommage OK</returns>
    this.addItemFct = function (label, fct, to, niveau, cssClass, toolTipText, cssImg) {

        if (typeof (fct) == "function") {
            var tabLastCssClass = cssClass.split(" ");
            var lastCssClass = tabLastCssClass[tabLastCssClass.length - 1];
            var nRet = 0;
            if (typeof (niveau) == "number") {
                nRet = niveau * 20;
            }

            var liMenu = document.createElement('li');

            var ImgMenu = document.createElement('span');
            addClass(ImgMenu, cssImg);
            liMenu.appendChild(ImgMenu);

            liMenu.onclick = fct;

            //
            if (toolTipText != null)
                liMenu.title = toolTipText;

            var target = this.getTarget(to);
            if (target == null)
                return;

            addClass(liMenu, cssClass);


            if (nRet > 0) {
                liMenu.style.paddingLeft = nRet + "px";
            }
            var labelMenu = document.createElement('span');
            labelMenu.innerHTML = label;
            addClass(labelMenu, lastCssClass + "Txt");
            liMenu.appendChild(labelMenu);

            target.appendChild(liMenu);

            this.adjVertPos();

            return liMenu;

        }
    };


    this.addHtmlElement = function (elm, to) {
        var divMenu = document.createElement('li');
        divMenu.className = "eCMSep";
        divMenu.appendChild(elm);
        var target = this.getTarget(to);
        if (target == null)
            return;
        target.appendChild(divMenu);

        this.adjVertPos();

    };


    this.addHtmlElementSearch = function (elm, to) {
        var divMenu = document.createElement('li');
        divMenu.appendChild(elm);
        var target = this.getTarget(to);
        if (target == null)
            return;
        target.appendChild(divMenu);

        this.adjVertPos();

    };


    this.addSeparator = function (to) {
        var divMenu = document.createElement('li');
        divMenu.className = "eCMSep";

        var target = this.getTarget(to);
        if (target == null)
            return;

        target.appendChild(divMenu);

        this.adjVertPos();


    };


    this.clearList = function (to) {
        var target = this.getTarget(to);
        if (target == null)
            return;
        while (target.hasChildNodes()) {
            target.removeChild(target.childNodes[0])
        }
    };


    this.hide = function () {
        if (this.parentPopup)
            this.parentPopup.hide();

        //if (this.mainDiv) {
        //    try {
        //        document.body.removeChild(this.mainDiv);
        //    }
        //    catch (e) {

        //    }
        //    this.mainDiv = null;
        //}


        ////Retire la modal du tableau global
        //try {

        //    if (typeof that.Handle != "undefined" && top.window['_cm'])
        //        delete (top.window['_cm'][that.Handle]);

        //}
        //catch (exp) {
        // //  alert("ERREUR X lors du masquage : Impossible de retirer le handle du tableau global : " + exp.Description);
        //}
    };


    this.alignElement = function (obj, sPos, sAdjust, left, bContextMenuInPopup) {
        
        var obj_pos = getAbsolutePosition(obj);
        var myX = obj_pos.x;
        var myTopAdjust = 0;
        var myLeftAdjust = 0;

        if (typeof (sAdjust) == "string") {


            var aAdjust = sAdjust.split("|");
            if (aAdjust.length == 2) {
                try {

                    myTopAdjust = Number(aAdjust[0]);
                    myLeftAdjust = Number(aAdjust[1]);
                }
                catch (e) {
                    myTopAdjust = 0;
                    myLeftAdjust = 0;
                }
            }
        }


        sPos += '';
        sPos = sPos.toUpperCase();

        this.parentPopup.left = myX + myLeftAdjust;
        if (typeof left == 'undefined' || !left) {

            if (typeof bContextMenuInPopup === "undefined")
                bContextMenuInPopup = false;

            var winSize = (bContextMenuInPopup) ? windowSize.w : windowSize.dw;
            if (this.parentPopup.left + this.width > winSize)
                this.parentPopup.left = winSize - this.width - 6;


        } else
            this.parentPopup.left = left - myLeftAdjust - this.width;


        switch (sPos) {
            case "UNDER":
                var myY = obj_pos.y;
                this.parentPopup.top = myY + myTopAdjust + obj.offsetHeight;
                break;

            case "ABOVE":
                var myY = obj_pos.y - this.parentPopup.div.offsetHeight;
                this.parentPopup.top = myY + myTopAdjust;
                break;
            case "BEFORE":
                var myY = obj_pos.y;
                this.parentPopup.top = myY + myTopAdjust;
                this.parentPopup.zIndex = GetMaxZIndex(top.document, 100) + 1;
                break;
        }
        this.parentPopup.show();
    };

    this.menuAddClass = function (sClassName) {
        eTools.SetClassName(this.parentPopup.div, sClassName);
    };


    this.menuRemoveClass = function (sClassName) {
        removeClass(this.parentPopup.div, sClassName);
    };


    this.hideEmpty = function () {

        if (this.topMenuUl.childNodes.length == 0)
            this.topMenuUl.style.display = 'none';

        if (this.middleMenuUl.childNodes.length == 0)
            this.middleMenuUl.style.display = 'none';

        if (this.bottomMenuUl.childNodes.length == 0)
            this.bottomMenuUl.style.display = 'none';


        this.adjVertPos();

    }

    var nFsize = top.eTools.GetFontSize();
    var sFontSizeCssName = "fs_" + nFsize + "pt";

    this.menuAddClass("control_contextmenu " + sFontSizeCssName);
    if (this.customCssClass) {
        this.menuAddClass(this.customCssClass);
    }
    this.adjVertPos();
    this.parentPopup.show();
}