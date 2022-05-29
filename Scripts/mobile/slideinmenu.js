/**
 * 
 * Find more about the slide down menu at
 * http://cubiq.org/slide-in-menu
 *
 * Copyright (c) 2010 Matteo Spinelli, http://cubiq.org/
 * Released under MIT license
 * http://cubiq.org/dropbox/mit-license.txt
 * 
 * Version 0.1beta1 - Last updated: 2010.05.28
 * Modified by MAB - Eudonet
 * 
 */

/*
	el: id of the root container element
	bOpen: true if the container is currently "closed" and needs to be opened, false to do nothing (depending of the initial position of the container)
	strPosition: "left", "right", "top", "bottom"
*/
function slideInMenu(el, bOpen, strPosition) {
	this.container = document.getElementById(el);
	if (!this.container)
		return;
	
    if (this.container.querySelector)
        this.handle = this.container.querySelector('.tabletHandle');

    if (!this.handle)
        this.handle = this.container.children[0];

    if (!this.handle)
		return;
	
	if (strPosition == "top" || strPosition == "bottom") {
		this.openedPosition = this.container.clientHeight;
		this.handleOffset = this.handle.clientHeight;
	}
	else {
		this.openedPosition = this.container.clientWidth;
		this.handleOffset = this.handle.clientWidth;
	}
		
	this.container.style.opacity = '1';

    // Sur iOS 7 (iPad), le positionnement du menu est parfois déclenché alors que ce dernier n'a pas encore de largeur définie.
    // On force donc le calcul en affectant au menu, la taille qu'il devrait avoir par défaut d'après mobile/eMenu.css
    // Il semblerait que sur cette version de l'OS, les injections de JS ou de CSS dans l'entête de la page soient mal acceptés "après coup"
	if (this.openedPosition == 0) {
        // cette valeur doit correspondre à celle de la classe rightMenu dans mobile/eMenu.css
	    this.openedPosition = 200;
	    // On appelle une fonction déclenchant des opérations CSS pour "rafraîchir" la page suite à l'ajout de classes
	    // et permettre, notamment, l'affichage du bouton Accueil rajouté dynamiquement en JavaScript en mode tablettes (...)
	    window.setTimeout(function (currentSlideInMenu) { currentSlideInMenu.close(); }(this), 100);
	}

	switch (strPosition) {
		case "top": this.container.style.top = '-' + (this.openedPosition - this.handleOffset) + 'px'; break;
		case "bottom": this.container.style.bottom = '-' + (this.openedPosition - this.handleOffset) + 'px'; break;
		case "left": this.container.style.left = '-' + (this.openedPosition - this.handleOffset) + 'px'; break;
		case "right": this.container.style.right = '-' + (this.openedPosition - this.handleOffset) + 'px'; break;
    }

    if (strPosition == "right" || strPosition == "bottom") {
        this.openedPosition = 0 - this.openedPosition;
        this.handleOffset = 0 - this.handleOffset;
    }

	this.container.style.webkitTransitionProperty = '-webkit-transform';
	this.container.style.webkitTransitionDuration = '400ms';

	if (this.onHandleFocus)
	    this.handle.addEventListener('focus', this.onHandleFocus);
	if (this.onHandleBlur)
	    this.handle.addEventListener('blur', this.onHandleBlur);

	if (this.onContainerFocus)
	    this.container.addEventListener('focus', this.onContainerFocus);
	if (this.onContainerBlur)
	    this.container.addEventListener('blur', this.onContainerBlur);

	if (bOpen === true) {
	    this.open();
	}

	this.position = strPosition;
	
	this.handle.addEventListener('touchstart', this);
}

slideInMenu.prototype = {
    debugMode: false, // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !
    pos: 0,
    opened: false,
    position: "right",
    onTouchStart: null,
    onTouchEnd: null,
    onBeforeOpen: null,
    onBeforeClose: null,
    onBeforeToggle: null,
    onBeforeTap: null,
    onAfterOpen: null,
    onAfterClose: null,
    onAfterToggle: null,
    onAfterTap: null,
    onHandleFocus: null,
    onHandleBlur: null,
    onContainerFocus: null,
    onContainerBlur: null,

    handleEvent: function (e) {
        switch (e.type) {
            case 'touchstart': this.touchStart(e); break;
            case 'touchmove': this.touchMove(e); break;
            case 'touchend': this.touchEnd(e); break;
        }
    },

    setPosition: function (pos) {
        this.pos = pos;
        if (this.position == "top" || this.position == "bottom")
            this.container.style.webkitTransform = 'translate3d(0,' + pos + 'px,0)';
        else
            this.container.style.webkitTransform = 'translate3d(' + pos + 'px,0,0)';

        if (this.pos == this.openedPosition - this.handleOffset) {
            this.opened = true;
            if (this.onAfterOpen)
                this.onAfterOpen();
        }
        else if (this.pos == 0) {
            this.opened = false;
            if (this.onAfterClose)
                this.onAfterClose();
        }
    },

    touchStart: function (e) {
        this.trace('touchStart');

        e.preventDefault();
        e.stopPropagation();

        this.container.style.webkitTransitionDuration = '0';
        this.startPos = this.pos;

        if (this.position == "top" || this.position == "bottom") {
            this.startDelta = e.touches[0].pageY - this.pos;
        }
        else {
            this.startDelta = e.touches[0].pageX - this.pos;
        }

        this.handle.addEventListener('touchmove', this);
        this.handle.addEventListener('touchend', this);

        if (this.onTouchStart)
            this.onTouchStart();
    },

    touchMove: function (e) {
        this.trace('touchMove (' + this.pos + ')');

        if (this.position == "top" || this.position == "bottom") {
            var delta = e.touches[0].pageY - this.startDelta;
        }
        else {
            var delta = e.touches[0].pageX - this.startDelta;
        }

        if (this.position == "top" || this.position == "left") {
            if (delta < 0) {
                delta = 0;
            } else if (delta > this.openedPosition - this.handleOffset) {
                delta = this.openedPosition - this.handleOffset;
            }
        }
        else {
            if (delta > 0) {
                delta = 0;
            } else if (delta < this.openedPosition - this.handleOffset) {
                delta = this.openedPosition - this.handleOffset;
            }
        }

        this.setPosition(delta);

        if (this.onTouchMove)
            this.onTouchMove();
    },

    touchEnd: function (e) {
        this.trace('touchEnd');

        var strokeLength = this.pos - this.startPos;
        strokeLength *= strokeLength < 0 ? -1 : 1;

        if (strokeLength > 3) {		// It seems that on Android is almost impossible to have a tap without a minimal shift, 3 pixels seems a good compromise
            var openCloseThreshold = 8; // 8 = on ouvre/ferme le menu à partir du moment où on ouvre le menu à 1/x de sa position ouverte
            // Move
            this.container.style.webkitTransitionDuration = '200ms';
            if (this.position >= "top" || this.position == "left") {
                if (this.pos == (this.openedPosition - this.handleOffset) || !this.opened) {
                    this.setPosition(this.pos > (this.openedPosition - this.handleOffset) / openCloseThreshold ? this.openedPosition - this.handleOffset : 0);
                } else {
                    this.setPosition(this.pos > (this.openedPosition - this.handleOffset) ? this.openedPosition - this.handleOffset : 0);
                }
            }
            else {
                if (this.pos == (this.openedPosition - this.handleOffset) || !this.opened) {
                    this.setPosition(this.pos < (this.openedPosition - this.handleOffset) / openCloseThreshold ? this.openedPosition - this.handleOffset : 0);
                } else {
                    this.setPosition(this.pos < (this.openedPosition - this.handleOffset) ? this.openedPosition - this.handleOffset : 0);
                }
            }
        } else {
            // Tap
            if (this.onBeforeTap)
                this.onBeforeTap();
            this.container.style.webkitTransitionDuration = '400ms';
            this.setPosition(!this.opened ? this.openedPosition - this.handleOffset : 0);
            if (this.onAfterTap)
                this.onAfterTap();
        }

        this.handle.removeEventListener('touchmove', this);
        this.handle.removeEventListener('touchend', this);

        if (this.onTouchEnd)
            this.onTouchEnd();
    },

    open: function () {
        if (this.onBeforeOpen)
            this.onBeforeOpen();

        this.trace('open');

        this.setPosition(this.openedPosition - this.handleOffset);

        if (this.onAfterOpen)
            this.onAfterOpen();
    },

    close: function () {
        if (this.onBeforeClose)
            this.onBeforeClose();

        this.trace('close');

        this.setPosition(0);

        if (this.onAfterClose)
            this.onAfterClose();
    },

    toggle: function () {
        if (this.onBeforeToggle)
            this.onBeforeToggle();

        this.trace('toggle');

        if (this.opened) {
            this.close();
        } else {
            this.open();
        }

        if (this.onAfterToggle)
            this.onAfterToggle();
    },

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    trace: function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'SlideInMenu [' + this.container.id + '] -- ' + strMessage;

                if (typeof(console) != "undefined" && console && typeof(console.log) != "undefined") {
                    console.log(strMessage);
                }
                else {
                    alert(strMessage); // TODO: adopter une solution plus discrète que alert()
                }
            }
            catch (ex) {

            }
        }
    }
}
