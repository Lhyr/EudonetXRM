/**
 * Objet qui gère les evenement xrm
 * 
 * S'abonner/Se désabonner à/de n'import quel evenement
 * Si l'evenement est déclanché tous les abonnés seront notifiés
 *
 **/

//Objet de gestion des evenements
var oEvent = (function () {

    //On a besoin que d'une seule instance
    if (top && top.oEvent)
        return top.oEvent;

    //objet contenant une liste de listeners pour chaque type d'evenement
    var listeners = {};
    var debug_listener = null;


    //TODO utiliser celle dans eTools Généateur d'clientId
    function generateId() {

        var CLIENT_ID_LENGTH = 7;

        var id = "";
        var alphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
        for (var i = 0; i < CLIENT_ID_LENGTH; i++)
            // on prends une position alétoire dans l'alphaNumeric
            id += alphaNumeric.charAt(Math.floor(Math.random() * alphaNumeric.length));

        return id;
    }
    //on notifie les listeners avec data comme param
    function fireEvent(evtName, data) {
        var clientNb = 0;
        var clientEventName = evtName;

        for (var clientId in listeners[evtName]) {
            if (listeners[evtName].hasOwnProperty(clientId) && listeners[evtName][clientId] != null) {
                listeners[evtName][clientId]({ 'clientId': clientId, 'name': clientEventName, 'data': data });
                clientNb++;
            }
        }

        if (debug_listener != null && typeof (debug_listener) == 'function')
            debug_listener({ 'clientId': 'debug_listener', 'name': evtName, 'data': data, 'date': getDate() });

        if (clientNb == 0)
            log("Pas d'abonnés à l'evenement '" + evtName + "'");
    }

    function log(str) {
        //console.log(str);
    }

    function getDate() {

        // format yyyy-mm-dd hh:mm:ss.mmm
        return new Date().toISOString().replace(/T/, ' ').substring(0, 23);
    }


    return {
        //S'abonner
        on: function (evtName, handler) {

            // Une fonction doit etre rensignée
            if (typeof (handler) != 'function') {
                log("Impossible de s'abonner '" + handler + "' n'est pas une function");
                return 0;
            }

            var clientId = generateId();

            // S'abonner à tous les listeners
            if (evtName == "debug") {
                debug_listener = handler;
                log("Abonné à tous les évenements");
                return 'debug_listener';
            }

            if (!(evtName in listeners))
                listeners[evtName] = new Array();

            listeners[evtName][clientId] = handler;

            log("Abonné l'évenement '" + evtName + "'");

            return clientId;

        },

        //Se désabonner
        off: function (evtName, clientId) {

            // On enlève pas l'element, on se contente de mettre null
            if (evtName == "debug") {
                debug_listener = null;
                log("Désabonné de tous les évenements");
                return;
            }

            // Désbonnement sur un evenment          
            if (evtName in listeners) {
                if (clientId in listeners[evtName]) {
                    delete listeners[evtName][clientId];
                    log("Désabonné de l'évenement '" + evtName + "'");
                    return;
                }
            }

            log("Pas d'abonné pour l'évenement '" + evtName + "'");
        },

        //on notifie les listener
        fire: function (evtName, data) {

            if (debug_listener != null && typeof (debug_listener) == 'function') {
                if (typeof (data) == 'undefined')
                    data = {};
                data.caller = {};
                if (arguments.callee && arguments.callee.caller && arguments.callee.caller.name)
                    data.caller.name = arguments.callee.caller.name;
            }                         

            fireEvent(evtName, data);
        },

        // Journalisation
        log: log,
        debug: function (active) {
            active = active == true || active == 1;
            if (active)
                this.on('debug', this.trace);            
            else
                this.off('debug', "");

            return "debug = " + active;
        },
        // Journalisation
        trace: function (evt) {
            try {

                if (typeof (evt.date) == "undefined")
                    evt.date = getDate();

                if (evt.name == 'log-info' && typeof (console.info) == 'function')
                    console.info('[' + evt.date + "] info : " + JSON.stringify(evt.data));
                else if (evt.name == 'log-warn' && typeof (console.warn) == 'function')
                    console.warn('[' + evt.date + "] warning : " + JSON.stringify(evt.data));
                else if (evt.name == 'log-error' && typeof (console.error) == 'function')
                    console.error('[' + evt.date + "] error : " + JSON.stringify(evt.data));
                else if (typeof (console.debug) == 'function')
                    console.debug('[' + evt.date + "] " + evt.name + ' : ' + JSON.stringify(evt.data));
                else if (typeof (console.log) == 'function')
                    console.log('[' + evt.date + "] " + evt.name + ' : ' + JSON.stringify(evt.data));
                else if (evt.name == 'log-error')
                    debugger;


            } catch (ex) { }
        }

    }
}());


function onListMouseOver(evt) {
    oEvent.fire('list-mouseover', { mouseevent: evt });
}
function onListMouseOut(evt) {
    oEvent.fire('list-mouseout', { mouseout: evt });
}







