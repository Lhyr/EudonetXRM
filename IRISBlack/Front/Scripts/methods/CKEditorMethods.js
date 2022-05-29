/**
 * Module étrange permettant de faire un insertHtml pour
 * ckeditor, sans le insertHtml.
 * https://github.com/ckeditor/ckeditor4/issues/3791
 * @param {any} input
 */

function htmlDecode(input) {
    var e = document.createElement('div');
    e.innerHTML = input;
    return e.childNodes[0].nodeValue;
}

/**
 * Insère du code HTML ou du texte dans le champ Mémo, en prenant le soin d'effectuer des traitements en interne si nécessaire
 * strData = code HTML ou texte à insérer
 * bInsertRawHTML = indique si on insère directement le code (true, méthode recommandée sur IE),
 * ou s'il faut d'abord créer un élément HTML dans le DOM avant de l'insérer en tant qu'élément (false, recommandée sur les autres navigateurs)
 * ATTENTION : Ce paramètre doit être passé à true si on insère uniquement du texte
 * bFocusBeforeInsert : si true, on positionne le focus dans le champ AVANT insertion (peut être requis par certains navigateurs)
 * nCursorPositionBeforeInsert : si défini à autre chose que -1, on positionne le curseur à cette position AVANT de faire l'insertion
 * bSkipHTMLTemplateEditor : si true, effectue l'insertion directement via CKEditor sans passer par grapesjs, même si l'éditeur utilisé est grapesjs
 *  -> cas de l'insertion des champs de fusion, cf. backlog #320
 * @param {any} strData
 * @param {any} bInsertRawHTML
 * @param {any} bFocusBeforeInsert
 * @param {any} nCursorPositionBeforeInsert
 * @param {any} bSkipHTMLTemplateEditor
 */
function insertData(strData, bInsertRawHTML, bFocusBeforeInsert, nCursorPositionBeforeInsert, bSkipHTMLTemplateEditor) {
    // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois insérés, notamment avec des champs de fusion, donnant des ??? à l'interprétation
    // Et autres caractères inutiles de ce style
    // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
    // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
    // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
    strData = eTools.removeHiddenSpecialChars(strData);

    if (bFocusBeforeInsert)
        this.focus();

    if (typeof (nCursorPositionBeforeInsert) != "undefined" && nCursorPositionBeforeInsert > -1)
        this.setCursorPosition(nCursorPositionBeforeInsert);

    // Pour l'éditeur de templates avancé
    if (this.htmlTemplateEditor && !bSkipHTMLTemplateEditor) {
        // https://github.com/artf/grapesjs/issues/111
        var canvasDoc = this.htmlTemplateEditor.Canvas.getBody().ownerDocument;
        // Insert text at the current pointer position
        canvasDoc.execCommand("insertHTML", false, strData);
    }

    // Pour CKEditor
    else if (this.dataInput.IsHtml && this.CKEditorInstance && !this.CKEditorInstance.isFake) {
        if (bInsertRawHTML) {
            try {
                /** Fix de mmmm, de très haute qualité, pour le insert html, 
                 * qui sans, ne fonctionne plus...
                 * https://github.com/ckeditor/ckeditor4/issues/490
                 * G.L */
                var sel = this.CKEditorInstance.getSelection();
                var range = sel.getRanges()[0];

                if (!range) {
                    range = this.CKEditorInstance.createRange();
                    range.selectNodeContents(this.CKEditorInstance.editable());

                    sel.selectRanges([range]);

                    let element = sel.getStartElement();
                    range.setStart(element, 0);
                    range.setEnd(element, 0); //cursor
                    sel.selectRanges([range]);
                }
            /************************************************************/

                //this.addEndSpaceWithMergeFields = true;
            this.CKEditorInstance.insertHtml(strData);

        }
            finally {
            //this.setCursorPosition(strData.length + 1);
        }
    }
    else {
        // #72 278 - Si le code à insérer débute par &nbsp; (cas si this.addStartSpaceWithMergeFields = true), il faut insérer ce &nbsp; à part pour ne pas provoquer
        // de plantage sur l'API CKEditor
        if (strData.indexOf("&nbsp;") == 0) {
            this.CKEditorInstance.insertHtml("&nbsp;");
            strData = strData.substring(6);
        }
        var oElt = CKEDITOR.dom.element.createFromHtml(strData);
        this.CKEditorInstance.insertElement(oElt);
        // #72 278 - Même chose si le code finit par &nbsp;, il faut l'insérer explicitement car createFromHtml() l'ignore
        if (strData.indexOf("&nbsp;") == strData.length - 6) {
            this.CKEditorInstance.insertHtml("&nbsp;");
        }
    }
}

    // Pour l'éditeur de texte brut
    else {
    var newValue = this.getData() + strData;
    var returnValue = this.setData(newValue);
    this.setCursorPosition(newValue.length);
    this.focus();
}
};


/**
 * Insertion de la signature dans CKEditor
 * */
function insertSignature() {

    if (this.readOnly)
        return;

    var strUserSignature = '';
    try {
        var oeParam = getParamWindow();
        strUserSignature = oeParam.GetParam('UserSignature');
        if (!this.dataInput.IsHtml)
            strUserSignature = removeHTML(strUserSignature); // depuis eTools.js
    }
    catch (ex) {
        strUserSignature = '';
    }


    if (this.dataInput.IsHtml) {

        strUserSignature = decode(strUserSignature) + '&nbsp;';
        /*
        Sur IE, il est difficile de positionner le curseur au tout début du champ pour réaliser une insertion si le début du contenu commence par une balise HTML
        Exemple : <b><u>Mon texte</u></b> : appeler this.setCursorPosition(0) positionnera le curseur après les balises : <b><u>|Mon texte</u></b>
        Ce qui provoquera l'insertion à l'intérieur des balises, et non devant. Pour éviter ce phénomène, il faudrait insérer un espace avant le message, ce qui se voit ;
        On utilise donc un algorithme spécifique pour IE, qui utilise insertAdjacentHTML pour incruster le code juste après la balise <body> de l'iframe CKEditor
        cf. correctifs réalisés sur RTEF en v7
        */
        if (CKEDITOR.env.ie) {
            var oObj = this.getMemoBody();
            oObj.insertAdjacentHTML("afterBegin", "<br /><br />" + strUserSignature);
            this.setCursorPosition(0); // removeHTML se trouve dans eTools.js
        }
        else {
            /*
            Avec les autres navigateurs, le support de insertAdjacentHTML est récent et n'est pas garanti sur toutes les versions (ex : Firefox 8 minimum).
            On utilise donc l'API de CKEditor pour gérer l'insertion, vu que sur ces autres navigateurs, le positionnement du curseur se fait bien en-dehors des balises.
            Pour pouvoir faire passer le contenu existant à la ligne et insérer le texte avant, il faut faire l'insertion en 2 fois
            */


            this.setData(this.getData() + "<br /><br />" + strUserSignature);
            this.setCursorPosition(0);
        }
    }
    else {
        strUserSignature = strUserSignature + ' ';
        this.setData(this.getData() + '\n\n' + strUserSignature);
        this.focus();
        this.setCursorPosition(0);

    }
}

/**
 * Renvoie le contenu du champ Mémo
 * */
function getData() {
    var memoData = this.value;
    if (this.dataInput.IsHtml) {
        if (this.CKEditorInstance && !this.CKEditorInstance.isFake) {
            try {
                this.CKEditorInstance.updateElement(); // mise à jour du champ <textarea> original avec le contenu modifié
                memoData = this.CKEditorInstance.getData();
            }
            catch (ex) {
                if (!this.inlineMode && !this.htmlEditor?.inlineMode) {
                    if (this.CKEditorInstance.document &&
                        this.CKEditorInstance.document.$ &&
                        this.CKEditorInstance.document.$.body &&
                        this.CKEditorInstance.document.$.body.innerHTML
                    ) {
                        memoData = this.CKEditorInstance.document.$.body.innerHTML;
                    }
                }
            }
        }
        else {
            memoData = this.virtualTextAreaValue;
        }
    }
    else {
        if (this.textEditor)
            memoData = this.textEditor.value;
        else
            memoData = this.virtualTextAreaValue;
    }

    // #68 13x - Prise en charge de l'éditeur de templates avancé
    if (this.htmlTemplateEditor) {
        // https://github.com/artf/grapesjs/wiki/API-Editor#gethtml
        // https://github.com/artf/grapesjs/issues/557
        // Backlog #261, #295, #445 - Le contenu renvoyé par l'API grapesjs ne se met à jour que lorsque le curseur sort du composant édité
        // Pour forcer la mise à jour lors d'un appel à getData() alors que le curseur s'y trouve toujours (cas de l'édition d'un modèle et d'un clic sur le bouton
        // Valider sans cliquer au préalable ailleurs), on désactive l'édition en cours avant de renvoyer le contenu
        // https://github.com/artf/grapesjs/issues/1767
        // https://github.com/artf/grapesjs/issues/1327
        // https://github.com/artf/grapesjs/issues/319
        // https://github.com/artf/grapesjs/issues/179
        this.disableEditing();
        // Backlog #427 - Le contenu HTML du canevas doit être renvoyé avec des styles inline pour rester compatible avec Outlook
        // Il faut pour cela utiliser la commande gjs-get-inlined-html de grapesjs, plutôt que sa fonction getHtml()
        // Source : https://github.com/artf/grapesjs-preset-newsletter/issues/4
        // Dans ce cas précis, il n'y a pas besoin de renvoyer les styles définis par grapesjs séparément.
        // Notre CSS, contenant entre autres le nécessaire pour le responsive, sera, quand à elle, réinjectée via injectCSS()
        //memoData = this.htmlTemplateEditor.getHtml(); // Avec grapesjs, les CSS inline (dans le body) ne sont PAS renvoyées par getHtml(). Pour récupérer les CSS, utiliser eMemoEditor.getCss()
        // Demande #72 138/#72 207 - Nettoyage des CSS grapesjs également à la lecture (ex : pour l'envoi du mail définitif)
        memoData = this.cleanEditorCss(this.htmlTemplateEditor.runCommand("gjs-get-inlined-html"));
    }

    // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois présents dans le contenu, notamment à l'insertion de champs de fusion,
    // donnant des ??? à l'interprétation. Et autres caractères inutiles de ce style
    // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
    // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
    // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
    memoData = eTools.removeHiddenSpecialChars(memoData);

    if (this.debugLevel > 1) {
        this.trace("Valeur initiale du champ Mémo (getData) : " + this.value);
        this.trace("Valeur actuelle du champ Mémo (getData) : " + memoData);
    }

    return memoData;
};

/**
 * Met à jour le contenu du champ Mémo
 * @param {any} memoData
 * @param {any} callback
 * @param {any} resetColor
 */
function setData(memoData, callback, resetColor) {
    setWait(true);

    // Backlog #451 - Suppression des caractères Zero-width Space (Unicode) parfois insérés, notamment avec des champs de fusion, donnant des ??? à l'interprétation
    // Et autres caractères inutiles de ce style
    // cf. correctif précédemment effectué sur UserMessage pour la même raison : #31 571
    // Les insertions de ce genre de caractères sont souvent provoquées par les méthodes de manipulation de Sélections/Ranges en JavaScript
    // https://stackoverflow.com/questions/11305797/remove-zero-width-space-characters-from-a-javascript-string
    memoData = eTools.removeHiddenSpecialChars(memoData);

    if (this.dataInput.IsHtml) {
        // #68 13x - Mise à jour de l'éditeur de templates avancé
        if (this.htmlTemplateEditor) {

            // Backlog #450 - Reset du canevas et de ses CSS existantes avant injection de memoData
            // Semble poser de nombreux problèmes, donc désactivé pour l'instant. On ne fait que remplacer le contenu via setComponents()
            // https://github.com/artf/grapesjs/issues/351
            // https://github.com/artf/grapesjs/issues/1115
            // https://github.com/artf/grapesjs/issues/1357
            // https://github.com/artf/grapesjs/issues/986
            // https://github.com/artf/grapesjs/issues/552
            // #71 938 - Suite à la mise en place d'options influençant la gestion des CSS (ex : forceClass),
            // reprise du reset des CSS pour corriger les différences de rendu entre 2 grapesjs de contexte différent
            // (ex : édition de modèle/utilisation) : https://github.com/artf/grapesjs/issues/488
            this.htmlTemplateEditor.setStyle('');
            this.htmlTemplateEditor.CssComposer.getAll().reset();
            this.htmlTemplateEditor.DomComponents.getWrapper().setStyle('');
            this.htmlTemplateEditor.DomComponents.getWrapper().set('content', '');
            this.htmlTemplateEditor.DomComponents.clear();
            //this.injectCSS("grapesjs/grapesjs-eudonet.css", true);
            // Demande #71 938/72 070 - Nettoyage des classes CSS c*** de l'éditeur de templates HTML avancé (grapesjs)
            // US #918 - Demande #72 814 - On appelle ici une version surchargée de setComponents (cf. createHTMLTemplateEditor) qui renverra une eAlert si le code à charger comporte des erreurs provoquant une DOMException
            this.htmlTemplateEditor.setComponents(this.cleanEditorCss(memoData));
            //this.htmlTemplateEditor.setStyle(memoData.trim());
        }

        // Mise à jour d'un champ CKEditor physiquement présent sur la page via appel asynchrone (CKEDITOR.setData)
        // qui exécutera ensuite la fonction passée en callback une fois que la mise à jour de CKEditor sera effective
        else if (this.CKEditorInstance && !this.CKEditorInstance.isFake) {
            try {
                this.CKEditorInstance.setData(memoData, callback);
                if (this.debugLevel > 1)
                    this.trace("Valeur envoyée à CKEditor pour mise à jour : " + memoData);
            }
            catch (ex) {
                if (this.CKEditorInstance.document &&
                    this.CKEditorInstance.document.$ &&
                    this.CKEditorInstance.document.$.body &&
                    this.CKEditorInstance.document.$.body.innerHTML
                ) {
                    this.CKEditorInstance.document.$.body.innerHTML = memoData;
                }
            }
        }

        // Sinon, mise à jour de la variable interne représentant le contenu d'un champ Mémo HTML
        else {
            this.virtualTextAreaValue = memoData;
            if (this.debugLevel > 1)
                this.trace("Valeur mise à jour sur le champ Mémo HTML : " + memoData);

            // Exécution du code de sortie passé à la fonction
            if (callback) {
                callback();
            }
        }

        // Backlog #619 - Remise à zéro de la couleur de fond
        if (resetColor) {
            this.setColor("");
        }
    }
    else {
        // Mise à jour directe du textarea physiquement présent sur la page
        if (this.textEditor) {
            this.textEditor.value = memoData;
        }
        // Sinon, mise à jour de la variable interne représentant le contenu d'un champ Mémo texte brut
        else {
            this.virtualTextAreaValue = memoData;
        }
        if (this.debugLevel > 1)
            this.trace("Valeur mise à jour sur le champ Mémo : " + memoData);

        // Exécution du code de sortie passé à la fonction
        if (callback) {
            callback();
        }
    }

    setWait(false);
};


/**
 * Récupère le message utilisateur (par défaut : nom d'utilisateur et date du jour) à insérer via le bouton dédié, depuis eParamIFrame
 * */
function getUserMessage() {
    // Initialisation
    var strUserMessage = '';
    var now = new Date();

    // MOU/MAB - Récupération de la date
    var day = now.getDate();
    if (day < 10) day = '0' + day;
    var month = now.getMonth() + 1; // le mois commence à 0 en JS 
    if (month < 10) month = '0' + month;
    var year = now.getFullYear();
    var localDate = day + "/" + month + "/" + year;

    // GCH/MOI - Récupération de l'heure au moment de l'appel à la fonction (#22 232)
    var localTime = now.toLocaleTimeString();
    if (CKEDITOR.env.ie) {
        // GCH - Bug IE11 demande #31571 : Insertion intempestive du caractère " ? " dans le champ fusion Date
        // chez Microsoft : https://connect.microsoft.com/IE/feedback/details/863366/ie11-javascript-date-tolocaletimestring-length-incorrect
        // lorsque sur ie11 on appelle toLocaleTimeString il est rajouté des caractères de type marqueur de gauche à droite (numéro 8206)
        // le patch ici est donc de remplacer les 8206 par rien !
        // Backlog #451 - Utilisation d'une fonction centralisée qui remplace ce caractère-là, mais aussi d'autres
        //localTime = localTime.replace(/\u200E/g, "");   //\u200E (8206 in hex)
        localTime = eTools.removeHiddenSpecialChars(localTime);
    }

    // Récupération d'un éventuel formatage du UserMessage paramétré en base
    try {
        var oeParam = getParamWindow();
        strUserMessage = oeParam.GetParam('UserMessage');

        if (!this.dataInput.IsHtml)
            strUserMessage = removeHTML(strUserMessage); // depuis eTools.js
        else {
            // #57407 : Correctif du mauvais encodage du &amp;nbsp; car trop d'impacts si on corrige à la source
            strUserMessage = strUserMessage.replace(/&amp;nbsp;/g, '&nbsp;');
        }
    }
    catch (ex) {
        strUserMessage = '';
    }

    // S'il n'y a pas de formatage en base, on utilise le format par défaut "JJ/MM/AAAA USERNAME :" sans heure
    if (!strUserMessage || typeof (strUserMessage) == 'undefined' || strUserMessage == '') {
        // Internationalisation : on convertit la date seule au format défini dans la base
        localDate = eDate.ConvertBddToDisplay(localDate);

        if (this.dataInput.IsHtml)
            strUserMessage = "<strong>" + localDate + "</strong>&nbsp;:";
        else
            strUserMessage = localDate + " :";
    }
    // Sinon, on utilise le formatage défini en base
    // @ClientDateTime et @ClientDate sont des mots-clés transmis par le serveur, on les remplace par la date et/ou l'heure courante
    // Internationalisation : on convertit le couple date et heure au format défini dans la base
    else {
        strUserMessage = strUserMessage.replace(/@ClientDateTime/g, eDate.ConvertBddToDisplay(localDate + " " + localTime));
        strUserMessage = strUserMessage.replace(/@ClientDate/g, eDate.ConvertBddToDisplay(localDate));
    }

    return strUserMessage;
};

/**
 * Insère le message utilisateur (par défaut : date et nom)
 * */
function insertMessage() {
    if (this.readOnly)
        return;

    var strUserMessage = this.getUserMessage();
    if (this.dataInput.IsHtml) {
        strUserMessage = decode(strUserMessage) + '&nbsp;';

        this.insertData('<br /><br />', true, false, 0);
        this.insertData(strUserMessage, true, false, 0);
    }
    else {
        strUserMessage = strUserMessage + ' ';
        this.setData(strUserMessage + '\n\n' + this.getData());
        this.setCursorPosition(strUserMessage.length);
    }
};

/**
 * Place le curseur à l'intérieur du textarea
 * */
function focus() {
    if (this.dataInput.IsHtml && this.CKEditorInstance && this.CKEditorInstance.focus) {
        this.CKEditorInstance.focus(); // appel de la fonction de CKEditor
    }
    else {
        if (this.textEditor) {
            this.textEditor.focus();
        }
    }
};



/**
 * Place l'ascenseur de la page au niveau de l'élément HTML dont l'ID a été renseigné via this.scrollIntoViewId
 *
 * Canceled by KHA le 31/01/2013 et remplacé par une fonction asynchrone cf emain.js updateFile
 *   this.scrollIntoView = function () {
 *   if (this.scrollIntoViewId && document.getElementById(this.scrollIntoViewId))
 *   document.getElementById(this.scrollIntoViewId).scrollIntoView(true);
 *   };
 *
 * JLA & MAB - Place le curseur à une position précise par rapport au texte visible, sans tenir compte des tags HTML
 * Exemple : le texte contient <br><br>Test<br><br>, un positionnement à 2 placera le curseur entre "e" et "s" de "Test"
 * @param {any} nPos
 * @param {any} offsetNodes
 */
function setCursorPosition(nPos, offsetNodes) {

    var oObj = null;
    var oSel = null;
    var bUseAlternateMethod = false;

    if (!nPos) { nPos = 0; }

    if (this.dataInput.IsHtml && this.CKEditorInstance && this.CKEditorInstance.getSelection() && this.CKEditorInstance.getSelection().getNative()) {
        oObj = this.getMemoBody();
        bUseAlternateMethod = CKEDITOR.env.ie;
    }
    else {
        oObj = this.textEditor;
        var sUserAgent = navigator.userAgent.toLowerCase();
        bUseAlternateMethod = false;
    }

    if (bUseAlternateMethod) {
        try {
            var sTextRange = null;
            // CKEditor mode classique (oObj = body de l'iframe)
            if (typeof (oObj.createTextRange) == "function") {
                sTextRange = oObj.createTextRange();
            }
            // CKEditor mode inline (oObj = élément HTML directement éditable)
            else {
                sTextRange = document.body.createTextRange();
                sTextRange.moveToElementText(oObj);
            }
            sTextRange.moveStart('character', nPos);
            //-- Déplace le curseur
            sTextRange.collapse();
            sTextRange.select();
        }
        catch (ex) {
            bUseAlternateMethod = false;
            try {
                this.trace("Une erreur s'est produite lors du positionnement du curseur à l'emplacement " + nPos + " - Utilisation de la méthode standard - Erreur : " + ex);
            }
            catch (ex2) { }
        }
    }

    if (!bUseAlternateMethod) {
        try {
            // Champs Mémo mode Texte
            if (this.dataInput.IsHtml) {
                oSel = this.CKEditorInstance.getSelection().getNative();

                if (nPos == 0 && oSel && oSel.collapse) {
                    oSel.collapse(oObj, 0);
                }
                // MAB - 20090319 - bug #9494
                else {
                    // Recherche du tag HTML (node) situé immédiatement après la position passée en paramètre
                    var referenceNode = null;
                    var charCount = 0;
                    for (var i = 0; i < oObj.childNodes.length; i++) {
                        var currentNodeLength = NaN;

                        // Navigateurs normaux
                        if (oObj.childNodes[i].textContent && oObj.childNodes[i].textContent.length)
                            currentNodeLength = oObj.childNodes[i].textContent.length;
                        // IE 8
                        if (isNaN(currentNodeLength) && oObj.childNodes[i].length)
                            currentNodeLength = oObj.childNodes[i].length;

                        if (!isNaN(currentNodeLength))
                            charCount += currentNodeLength;

                        // si on a dépassé la position indiquée en paramètre, on se positionne au début du tag courant
                        // 20110310 - en appliquant si demandé un décalage de X tags/nodes HTML pour pouvoir poursuivre la saisie après l'insertion de certains codes
                        // ex : insertion d'un champ de fusion -> <label></label> -> il faut se positionner après le tag de fin </label>
                        if (charCount >= nPos) {
                            if (offsetNodes && !isNaN(offsetNodes) && ((i + offsetNodes) < oObj.childNodes.length)) {
                                referenceNode = oObj.childNodes[i + offsetNodes];
                            }
                            else {
                                referenceNode = oObj.childNodes[i];
                            }
                            i = oObj.childNodes.length; // sortie
                        }
                    }
                    // Positionnement
                    var sTextRange = null;
                    var oDoc = this.getMemoDocument();
                    if (oDoc) {
                        sTextRange = oDoc.createRange();
                    }
                    this.focus();

                    sTextRange = getElementSelectionRangeAt(oDoc, 0);

                    if (sTextRange != null && referenceNode != null) {
                        sTextRange.setStartBefore(referenceNode);
                        if (oSel.collapseToStart) { oSel.collapseToStart(); }
                        if (oSel.collapse) { oSel.collapse(oObj, 2); }
                    }
                }
            }

            // Champs Mémo mode Texte brut
            else {
                if (this.textEditor.setSelectionRange) {
                    this.textEditor.focus();
                    this.textEditor.setSelectionRange(nPos, nPos);
                }
                else if (this.textEditor.createTextRange) {
                    var range = this.textEditor.createTextRange();
                    range.collapse(true);
                    range.moveEnd('character', nPos);
                    range.moveStart('character', nPos);
                    range.select();
                }
            }
        }
        catch (ex) {
            try {
                this.trace("Une erreur s'est produite lors du positionnement du curseur à l'emplacement " + nPos + " avec la méthode standard - Erreur : " + ex);
            }
            catch (ex2) { }
        }
    }
};


/**
 *  Renvoie un pointeur vers l'objet JS document affichant le contenu du champ Mémo (intérieur de la balise <body> pour les mémos HTML)
 * */
function getMemoDocument() {
    var oDoc = null;
    if (this.dataInput.IsHtml) {
        // CKEditor
        if (this.CKEditorInstance && !this.CKEditorInstance.isFake && (this.CKEditorInstance.container || this.CKEditorInstance.window)) {
            if (this.inlineMode || this.htmlEditor?.inlineMode) {
                oDoc = this.CKEditorInstance.container.$;
            }
            else {
                var oEditorFrame = this.CKEditorInstance.window.$.frameElement;
                if (oEditorFrame)
                    oDoc = oEditorFrame.contentWindow.document;
                else
                    oDoc = this.CKEditorInstance.window.$.document;
            }
        }

        // grapesjs
        if (this.htmlTemplateEditor) {
            oDoc = this.htmlTemplateEditor.Canvas.getBody().ownerDocument;
        }
    }
    else {
        oDoc = this.textEditor;
    }
    return oDoc;
}

/**
 * Renvoie un pointeur vers l'intérieur du contenu du champ Mémo (intérieur de la balise <body> pour les mémos HTML)
 */
function getMemoBody() {
    var oDoc = this.getMemoDocument();
    if (!this.inlineMode && !this.htmlEditor?.inlineMode) {
        if (oDoc && oDoc.getElementsByTagName)
            oDoc = oDoc.getElementsByTagName("body")[0];
    }

    return oDoc;
}

/**
 * trace du débogage.
 * @param {any} strMessage
 */
function trace(strMessage) {
    if (this.debugMode) {
        try {
            strMessage = new Date() + ' - eMemoEditor [' + this.name + '] -- ' + strMessage;

            if (typeof (console) != "undefined" && console && typeof (console.log) != "undefined") {
                console.log(strMessage);
            }
            else {
                alert(strMessage); // TODO: adopter une solution plus discrète que alert()
            }
        }
        catch (ex) {

        }
    }
};






/**
 * Insère une image à l'intérieur du champ Mémo
 * @param {any} bSetDialogURL
 */
function insertImg(bSetDialogURL) {
    // Backlog #315 : récupération de l'URL pour l'insérer dans la fenêtre Image de CKEditor
    if (bSetDialogURL)
        //doGetImage(this, 'MEMO_SETDIALOGURL');
        doGetImageGenericIris(this, 'MEMO_SETDIALOGURL', '');
    // Sinon, insertion directe de l'image dans l'éditeur
    else
        //doGetImage(this, 'MEMO');
        doGetImageGenericIris(this, 'MEMO_SETDIALOGURL', '');
};

/**
 * Fonction récupérée sur eMain qui permet de récupérer une image qui permet de câbler les bouton de la modale image sur CKEditor
 * @param {any} oImg contexte du composant eMemo
 * @param {any} strType type d'image mais pas utile car pas utiliser dans cette version, à supprimer éventuellement
 * @param {any} sFrom
 */

function doGetImageGenericIris(oImg, strType, sFrom) {

    // CRU : Empêcher la modification de l'image si le champ est en lecture seule
    if (getAttributeValue(oImg, "readonly") == "readonly")
        return;

    var bDisplayDeleteBtn = true;
    var deleteImageFct = deleteImage;
    var objHeaderCell = null;
    var sName = "";
    top.setWait(true);
    try {
        /*********************** Titre de la modale ***********************/
        var strModalDialogTitle = top._res_6286; // Insérer une image depuis votre ordinateur


        /*********************** Taille de la modale ***********************/
        // #32 312 - Taille de la fenêtre = 160 + marge pour afficher l'image "Introuvable" renvoyée par le navigateur si l'image est impossible à charger
        var initialWindowWidth = 460; // valeur à modifier également dans eImageDialog.aspx
        var initialWindowHeight = 180; // valeur à modifier également dans eImageDialog.aspx
        var windowWidth = initialWindowWidth;
        var windowHeight = initialWindowHeight;
        var browser = new getBrowser();
        if (browser.isIE)
            windowHeight += 28;
        else
            windowHeight += 20;

        top.modalImage = new eModalDialog(strModalDialogTitle, 0, "eImageDialog.aspx", windowWidth, windowHeight);
        // On mémorise la taille que l'on souhaitait initialement affecter à la fenêtre dans deux variables JS, que l'on rattachera en propriétés de
        // l'objet eModalDialog afin que la page chargée à l'intérieur puisse déclencher son redimensionnement si elle masque le conteneur d'image
        // dans le cas où il n'y a aucune image à charger
        top.modalImage.initialWindowWidth = initialWindowWidth;
        top.modalImage.initialWindowHeight = initialWindowHeight;
        top.modalImage.addParam("ImageType", strType, "post");
        top.modalImage.addParam("CalledFrom", sFrom, "post");


        /*********************** RECUPERATION DE CERTAINS PARAMETRES EN FONCTION DU TYPE D'IMAGE A GERER ***********************/

        var descId, fileId;

        // Contexte du champ Mémo à mettre à jour : pour l'insertion dans des champs Mémo uniquement

        top.modalImage.parentMemoEditor = oImg;
        descId = getAttributeValue(objHeaderCell, "did");
        // Récupération du fileId en mode Fiche
        fileId = imgGetCurrentFileId(oImg);


        if (oImg.querySelector) {
            var myImpg = oImg.querySelector("img");
            if (getAttributeValue(myImpg, "isb64") == "1") {
                top.modalImage.addParam("isb64", "1", "post");
                top.modalImage.addParam("b64val", getAttributeValue(myImpg, "src"), "post");
            }
        }


        // AUTRES PARAMETRES

        top.modalImage.addParam("modalVarName", "modalImage", "post");
        top.modalImage.addParam("parentIsPopup", isPopup() ? "1" : "0", "post");
        top.modalImage.addParam("updateOnBlur", isUpdateOnBlur() ? "1" : "0", "post");
        top.modalImage.sourceObj = oImg;

    }
    finally {
        top.setWait(false);
    }
    top.setWait(true);
    top.modalImage.ErrorCallBack = function () { top.setWait(false); }
    top.modalImage.onIframeLoadComplete = function () { top.setWait(false); };
    top.modalImage.show();
    /*********************** BOUTONS ***********************/
    top.modalImage.addButton(top._res_29, () => { onImageCancel(); onImageCancelIris(oImg); }, "button-gray", undefined, "btnCancel"); // Annuler

    if (bDisplayDeleteBtn)
        top.modalImage.addButton(top._res_19, deleteImageFct, "button-red", undefined, "btnDelete"); // Supprimer 

    top.modalImage.addButton(top._res_28, function () { sendImageDialogForm(oImg); sendImageDialogFormIris(oImg); }, "button-green", undefined, "btnSend"); // Valider

}

/**
 * modifie le contenu du mémo et de dataInput puis fais une MAJ avec la novuelle image
 * @param {any} oImg réprésente le contexte du composant
 */

function sendImageDialogFormIris(oImg) {
    var files = document.querySelector(".MainModal > iframe").contentWindow.document.querySelector("#filMyFile").files;
    // créer l'élément image
    let oImgElt = '<img src="' + `${oImg.getBaseUrl}images/${[...files].find(x => x.name != "").name}` + '" />';
    // insertData() insert dans le position actuel et setData() insert dans la bas
    // #96 278 - Ajout de deux paramètres sur insertData() pour se rapprocher du comportement (fonctionnel) d'E17 (cf. eMain.sendImageDialogForm)
    // second paramètre : insérer le code HTML directement sans créer d'élément dans le DOM (requis uniquement pour IE. Les autres navigateurs préfèrent une insertion dans le DOM)
    // troisième paramètre : mettre le focus sur le champ avant insertion (INDISPENSABLE pour positionner l'image à l'endroit où se trouve le curseur, et pour que CKEditor sache où insérer)
    oImg.insertData(oImgElt, CKEDITOR.env.ie, true);
    // #96 278 - Dans le même esprit, au moment d'envoyer la MAJ en base, il faut récupérer le contenu modifié par l'éditeur via son API (getData()) et non via le contexte VueJS (this.dataInput.Value)
    // qui n'est pas forcément à jour
    // On notera ici que l'on déclenche la MAJ directement sans repasser par verifMemo/verifComponent (cf. onImageCancelIris/oBlurFct) car on sait que le contenu a été modifié vu qu'on a appelé insertData()
    oImg.updateMethod ? oImg.updateMethod(oImg, oImg.getData(), undefined, undefined, oImg.dataInput) : "";
}

/**
 * à l'annulation de la popup de nouvelle image, redéclenche la vérification du champ pour MAJ son contenu en base s'il avait changé avant l'affichage de la popup d'ajout d'image
 * (cette mise à jour étant court-circuitée à l'ouverture de la popup, cf. isModalImageOpened())
 * @param {any} oImg réprésente le contexte du composant
 */

function onImageCancelIris(oImg) {
    oImg.oBlurFct();
}

function help() {
    if (this.CKEditorInstance && this.CKEditorInstance.lang.xrmHelpContents && this.CKEditorInstance.lang.xrmHelpContents != '') {
        // On masque la barre d'outils, afin qu'elle ne reste pas affichée au-dessus de l'eAlert
        // REMARQUE : On pourrait utiliser blur() pour faire sortir le curseur du champ, mais ce n'est pas forcément utile ici vu que l'on maîtrise à quel moment on réaffichera la
        // barre d'outils, contrairement au mécanisme similaire implémenté sur l'évènement onScroll (cf. implémentation dans on('instanceReady') plus bas)
        //this.blur();
        this.hideShowToolBar(false);
        var onOkFct = (function (eMemoEditorTargetObject) {
            return function () {
                // Le fait de masquer la barre d'outils plus haut nécessite de la réafficher explicitement ensuite, car CKEditor ne le fait pas de lui-même (il ne modifie pas style.display
                // en interne, seulement le positionnement lors de la saisie ou du focus)
                eMemoEditorTargetObject.hideShowToolBar(true);
                // Puis on remet le focus dans le champ, qui est forcément perdu dès lors que l'on cliquera sur l'eAlert pour la fermer (bouton Fermer ou X)
                // Sans ce focus, la barre d'outils serait de nouveau masquée, cette fois par CKEditor lui-même, vu que le curseur ne serait plus dedans
                eMemoEditorTargetObject.focus();
            }
        })(this);
        // Puis on affiche la popup d'aide, en câblant sur le bouton OK un appel à la fonction qui réactivera la barre d'outils
        eAlert(3, top._res_6187, top._res_6187, this.CKEditorInstance.lang.xrmHelpContents, 500, 500, onOkFct);
    }
};


/**
 * Renvoie un pointeur vers l'élément HTML de la barre d'outils, principalement dans le but d'agir sur son affichage en mode inline
 * */
function getToolBarContainer () {
    // Parcours des noeuds enfant à la recherche de la barre d'outils, qui a un ID variable
    // On ne peut pas utiliser getElementsByClassName qui n'est pas supporté par IE 8 : http://caniuse.com/getelementsbyclassname
    try {
        var oInstanciatedObject = document.getElementById('cke_' + this.name);
        if (oInstanciatedObject) {
            return oInstanciatedObject.querySelector('div[class="cke_top"]');
        }
        else {
            return null;
        }
    }
    catch (e) {
        return null;
    }

}

// Affiche ou masque la barre d'outils à la demande
function hideShowToolBar(show) {
    if (this.dataInput.IsHtml) {
        if (this.inlineMode || this.htmlEditor?.inlineMode) {
            var oToolbar = getToolBarContainer();
            if (oToolbar) {
                if (typeof (show) == 'undefined' || show == null) {
                    show = (oToolbar.style.display == 'none');
                }
                oToolbar.style.display = (show ? '' : 'none');
            }
        }
    }
};

function setCss (sCSS) {
    this.customCSS = sCSS;
};

function injectCSS (sNewCSSValue, isNativeCSS) {
    if (!this.dataInput.IsHtml || !sNewCSSValue)
        return;

    this.CKEditorInstance.setMode('wysiwyg');
    // Si la valeur passée en paramètre est une chaîne unique, on la transforme en tableau
    if (typeof (sNewCSSValue) == "string") {
        sNewCSSValue = new Array(sNewCSSValue);
    }
    // On parcourt chaque CSS à ajouter
    for (var i = 0; i < sNewCSSValue.length; i++) {
        // Si la valeur se termine par .css, on considère que c'est une référence à un fichier externe à ajouter comme telle
        if (sNewCSSValue[i].indexOf(".css") == sNewCSSValue[i].length - 4) {
            // CKEditor
            if (this.CKEditorInstance && !this.CKEditorInstance.isFake) {
                //#32178 On applique la css qu'on mode 'wysiwyg' car le mode 'source' ne dispose pas de balise style
                if (this.CKEditorInstance.mode == 'wysiwyg') {
                    // Ajout des styles sans passer par l'API CKEditor pour que les styles soient pris en compte immédiatement
                    // Backlog #267 - TODO si nécessaire

                    // Puis ajout de ces styles avec l'API pour qu'ils soient rechargés lors d'un switch entre mode design et mode HTML
                    // Nouvelle méthode CKEditor 4 https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_dom_document.html#method-appendStyleSheet
                    if (this.CKEditorInstance.document && this.CKEditorInstance.document.appendStyleSheet)
                        this.CKEditorInstance.document.appendStyleSheet(sNewCSSValue[i]);
                    /* Générateur de formulaires */
                    // TODO XRM
                }
            }           
        }
        // Sinon, on injecte le contenu en inline <style>
        else {
            // CKEditor
            if (this.CKEditorInstance && !this.CKEditorInstance.isFake) {
                //#32178 On applique la css qu'on mode 'wysiwyg' car le mode 'source' ne dispose pas de balise style
                if (this.CKEditorInstance.mode == 'wysiwyg') {               
                    // Ajout des styles sans passer par l'API CKEditor pour que les styles soient pris en compte immédiatement
                    var oStyles = this.CKEditorInstance.container.document.$.getElementsByTagName("style");
                    if (oStyles[0].styleSheet) {
                        // IE6,7 et 8
                        oStyles[0].styleSheet.cssText = sNewCSSValue[i];
                    } else {
                        // Firefox 3.x
                        oStyles[0].innerHTML = sNewCSSValue[i];
                    }

                    // Puis ajout de ces styles avec l'API pour qu'ils soient rechargés lors d'un switch entre mode design et mode HTML
                    // Nouvelle méthode CKEditor 4 ? https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_dom_document.html#method-appendStyleText
                    if (this.CKEditorInstance.document && this.CKEditorInstance.document.appendStyleText)
                        this.CKEditorInstance.document.appendStyleText(sNewCSSValue[i]);
                    // Ancienne méthode CKEditor 3 ? https://docs-old.ckeditor.com/ckeditor_api/symbols/CKEDITOR.editor.html#addCss
                    else if (this.CKEditorInstance.addCss)
                        this.CKEditorInstance.addCss(sNewCSSValue[i]);
                    /* Générateur de formulaires */
                    // TODO XRM
                }
            }
           
        }

        // Si on appelle injectCSS pour ajouter une seule CSS personnalisable par l'utilisateur, on la déclare dans customCSS
        // On considère alors qu'un seul appel à injectCSS a été fait (par les modal dialogs) et que la CSS actuelle remplacera toute éventuelle CSS précédemment
        // rattachée à cette propriété customCSS
        // A l'inverse, si injectCSS est appelée en interne pour injecter une CSS native de l'application dans l'iframe de l'éditeur, on ne le fait surtout pas
        if (!isNativeCSS)
            this.setCss(sNewCSSValue[i]);
    }
};

export { insertSignature, getUserMessage, insertMessage, insertData, setData, getData, setCursorPosition, getMemoBody, getMemoDocument, trace, insertImg, help, hideShowToolBar, focus, injectCSS, setCss };