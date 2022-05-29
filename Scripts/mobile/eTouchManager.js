//*****************************************************************************************************//
//*****************************************************************************************************//
//*** MAB - 11/2012 - objet JS permettant de gérer les interactions/gestes sur tablettes et écrans tactiles
//*****************************************************************************************************//
//*****************************************************************************************************//

function eTouchManager(targetElement) {
    this.targetElement = document.getElementById(targetElement);
    if (!this.targetElement)
		return;

    this.targetElement.removeEventListener('touchstart', this);
    this.targetElement.removeEventListener('touchmove', this);
    this.targetElement.removeEventListener('touchend', this);

    this.targetElement.addEventListener('touchstart', this);
 

}

eTouchManager.prototype = {
    debugMode: false, // à mettre à true pour afficher certains messages de diagnostic - NE PAS UTILISER EN PRODUCTION !
    posX: 0,
    posY: 0,
    tapMoveThreshold: 3,
    longTapThreshold: 1000,

    onTouchStart: null,
    onTouchMove: null,
    onTouchEnd: null,
    onTap: null,

    handleEvent: function (e) {
        this.trace('Handling event "' + e.type + '"');
        switch (e.type) {
            case 'touchstart': this.touchStart(e); break;
            case 'touchmove': this.touchMove(e); break;
            case 'touchend': this.touchEnd(e); break;
        }
    },

    touchStart: function (e) {
        this.startTimer = new Date();

        this.trace('touchStart | Timer = ' + this.startTimer + ', Start Position = (' + this.posX + ', ' + this.posY + ') ; Current Position = (' + e.touches[0].pageX + ', ' + e.touches[0].pageY + ')');



        this.startPosX = this.posX;
        this.startPosY = this.posY;
        this.startDeltaX = e.touches[0].pageX - this.posX;
        this.startDeltaY = e.touches[0].pageY - this.posY;

        this.targetElement.addEventListener('touchmove', this);
        this.targetElement.addEventListener('touchend', this);


        if (this.onTouchStart) 
            this.onTouchStart(e);

        e.target.originOnclick = e.target.onclick;

        // #39963 : SPH/CRU -> cela permet de ne pas faire de event.preventDefault() lors d'un scrolling
        mytimeout = setTimeout(function () {
            e.target.onclick = function (event) {
                event.preventDefault();
            }
        }, 1500);

        //e.preventDefault();
        e.stopPropagation();

    },

    touchMove: function (e) {

        if (mytimeout) {
            clearTimeout(mytimeout);
        };

        this.moveTimer = new Date();

        var deltaX = e.touches[0].pageX - this.startDeltaX;
        var deltaY = e.touches[0].pageY - this.startDeltaY;

        this.trace('touchMove | Timer = ' + this.moveTimer + ', startDeltaX = ' + this.startDeltaX + ', startDeltaY = ' + this.startDeltaY + ')');

        var strDirection = "none";
        if (deltaX < this.posX) {
            strDirection = "left";
        }
        else if (deltaX > this.posX) {
            strDirection = "right";
        }
        else if (deltaY < this.posY) {
            strDirection = "up";
        }
        else if (deltaY > this.posY) {
            strDirection = "down";
        }


        switch (strDirection) {

            case "left":
                if (this.onTouchMoveLeft) {
                this.onTouchMoveLeft();
            } break;
            case "right":
                if (this.onTouchMoveRight) { this.onTouchMoveRight(); } break;

            case "up": if (this.onTouchMoveUp) { this.onTouchMoveUp(); } break;
            case "down": if (this.onTouchMoveDown) { this.onTouchMoveDown(); } break;
        }

        this.posX = deltaX;
        this.posY = deltaY;

        if (this.onTouchMove)
            this.onTouchMove(e, deltaX, deltaY, strDirection);
    },

    touchEnd: function (e) {


        if (mytimeout)
            clearTimeout(mytimeout);

        // Rétablissement du onclick
        if (e.target.originOnclick && e.target.onclick) {
            e.target.onclick = e.target.originOnclick;
        }

        this.endTimer = new Date();

        this.trace('touchEnd | Timer = ' + this.endTimer);

        this.targetElement.removeEventListener('touchmove', this);
        this.targetElement.removeEventListener('touchend', this);

        var strokeLengthX = this.posX - this.startPosX;
        var strokeLengthY = this.posY - this.startPosY;
        var absStrokeLengthX = strokeLengthX;
        var absStrokeLengthY = absStrokeLengthY;
        absStrokeLengthX *= absStrokeLengthX < 0 ? -1 : 1;
        absStrokeLengthY *= absStrokeLengthY < 0 ? -1 : 1;

        if (!Number(this.tapMoveThreshold))
            this.tapMoveThreshold = 3;

        if (absStrokeLengthX > this.tapMoveThreshold || absStrokeLengthY > this.tapMoveThreshold) {

            this.trace('Accepted movement: X-movement amplitude is ' + absStrokeLengthX + ' and Y-movement amplitude is ' + absStrokeLengthY + ', more than the threshold of ' + this.tapMoveThreshold + ' pixels');
            // Move
            if (this.onTouchEnd)
                this.onTouchEnd(e, strokeLengthX, strokeLengthY);
        }
        else {
            e.preventDefault();

            this.trace('Movement considered as tap (X-movement amplitude is ' + absStrokeLengthX + ' and Y-movement amplitude is ' + absStrokeLengthY + ', shorter than the required threshold of ' + this.tapMoveThreshold + ' pixels)');

            var noTapFunctions = true;
            var tapTime = this.endTimer - this.startTimer;
            if (!Number(this.longTapThreshold))
                this.longTapThreshold = 1000;

            if (this.onTap || this.onLongTap) {

                if (tapTime >= this.longTapThreshold) {
                    this.trace('Movement considered as long tap (movement lasted ' + tapTime + ' milliseconds, more than the threshold of ' + this.longTapThreshold + ' seconds)');
                    if (this.onLongTap) {
                        this.onLongTap(e, tapTime);
                        noTapFunctions = false;
                    }
                    else {
                        this.trace('Triggering normal tap function as long time function is not defined.');
                        // Tap
                        if (this.onTap) {
                            this.onTap(e, tapTime);
                            noTapFunctions = false;
                        }
                    }
                }
                else {
                    this.trace('Movement considered as short tap (movement lasted ' + tapTime + ' milliseconds, less than the required threshold of ' + this.longTapThreshold + ' seconds)');
                    // Tap
                    if (this.onTap) {
                        this.onTap(e, tapTime);
                        noTapFunctions = false;
                    }
                }
            }
            // Tap : déclenchement du clic sur l'élément "tapé"
            if (noTapFunctions && e.target) {
                if (e.target.click) {
                    this.trace('No custom onTap function defined. Triggering target\'s default click action');
                    e.target.click();
                }
                else {
                    this.trace('No custom onTap or native onClick function defined. The tap will be ignored.');
                }
            }
        }
    },

    // Génère des messages de diagnostic ; uniquement si this.debugMode = true
    trace: function (strMessage) {
        if (this.debugMode) {
            try {
                strMessage = 'eTouchManager [' + this.targetElement.id + '] -- ' + strMessage;

                var oDebugFloatingConsole = document.getElementById("debugFloatingConsole");
                if (!oDebugFloatingConsole) {
                    oDebugFloatingConsole = document.createElement("textarea");
                    oDebugFloatingConsole.id = "debugFloatingConsole";
                    oDebugFloatingConsole.style.position = "absolute";
                    oDebugFloatingConsole.style.left = "50%";
                    oDebugFloatingConsole.style.bottom = "5px";
                    oDebugFloatingConsole.style.top = "5px";
                    oDebugFloatingConsole.style.width = "45%";
                    oDebugFloatingConsole.style.zIndex = "0";
                    oDebugFloatingConsole.readOnly = "readonly";
                    oDebugFloatingConsole.onclick = function () { this.style.display = "none"; }
                    document.body.appendChild(oDebugFloatingConsole);
                }

                if (oDebugFloatingConsole) {
                    oDebugFloatingConsole.style.display = "block";
                    oDebugFloatingConsole.innerHTML += strMessage + "\n";
                }
                else
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
    }
}
