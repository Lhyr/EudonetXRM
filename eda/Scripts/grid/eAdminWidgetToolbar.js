


///***************************************************************************
/// Objet permettant de gérer le paramètrage d'un widget et sa suppression
/// les actions de UserMode sont laisser à l'appelant
///***************************************************************************
var eAdminWidgetToolbar = function (toolbar) {

    /// <summary>
    /// barre d'outils de UserMode pour attacher les evenments : click, mouseup, mousedown
    /// </summary>
    var _userToolbar = toolbar;

    /// <summary>
    /// En AdminMode, on intercepte le click, pour traiter les actions autorisées en admin
    /// </summary>
    var _optionClick = function (evt) { console.log(evt); };

    /// <summary>
    /// Initialisation de la barre d'outil en AdminMode en se basant sur la barre d'outils de UserMode
    /// </summary>
    function initInternal() {   _userToolbar.onClick(clicked);   }

    /// <summary>
    /// Interception du clique sur la barre d'outils
    /// </summary>
    function clicked(evt) {
        fromSource(evt.src).whereAttribute("f", function (f) { return f > 0; }).findParent(function (parent) {

            // sélction l'action souhaité
            switch (evt.action) {
                case "edit":
                    _admin.edit(parent);
                    break;
                case "delete":
                    _admin.delete(evt, parent);
                    break;
                case "unlink":
                    // _admin.unlink(evt, widgetDiv); // on gère pas pour l'instant
                    break;
                case "config":
                    _admin.config(evt, parent);
                    break;
                    // on laisse passer les autres actions à l'appelant
                case "openlink":
                case "zoom":
                case "reload":
                    _optionClick(evt);
                    break;
                    // Garder la main pour ne pas laisser faire n'importe quoi
                default:
                    break;
            }
        });
    }

    /// <summary>
    /// Namespace, représentant des actions autorisées en admin
    /// </summary>
    var _admin = {

        /// <summary>
        /// Configuration d'un widget
        /// </summary>
        'config': function (evt, widget) {
            
            var fromBkm = false;

            var tabGrid = findUpByClass(widget, "gw-tab");
            if (tabGrid) {
                var parentTab = getAttributeValue(tabGrid, "parent-tab") * 1;
                fromBkm = parentTab > 0 && parentTab != 115200;
            }

            var gridContainer = findUpByClass(widget, "widget-grid-container");
            var gid = getAttributeValue(gridContainer, "gid");

            oAdminGridMenu.part.loadConfigWidget({
                'gid': gid,
                'id': getAttributeValue(widget, "f"),
                'fromBkm': fromBkm
            });
        },

        /// <summary>
        /// Supression de la liaison d'un widget de sa grille - OFF pour le moment, peut etre en UserMode aussi
        /// </summary>
        'unlink': function (evt, widget) {
            oGridController.widget.unlink({
                'id': getAttributeValue(widget, "f"),
                'callback': _admin.deleteReturn
            });
        },

        /// <summary>
        /// Supression du widget
        /// </summary
        'delete': function (evt, widget) {

            var widgetContent = widget.querySelector(".xrm-widget-content");
            oGridController.widget.delete({
                'id': getAttributeValue(widget, "f"),
                'callback': _admin.deleteReturn,
                'sid': getAttributeValue(widgetContent, "sid")
            });
        },

        /// <summary>
        /// Retour sur la supression du widget
        /// </summary
        'deleteReturn': function (result) {
            var widget = document.getElementById("widget-wrapper-" + result.WidgetId);

            // on enlève la div du dom une fois supprimée
            _optionClick({ 'action': 'remove'});
           
        },

        /// <summary>
        /// Edition d'un widget : Memo par exemple
        /// </summary
        'edit': function (widget) {
            oGridController.widget.edit({
                'id': getAttributeValue(widget, "f"),
                'srcId': widget.id,
                'callback': _admin.reloadReturn
            });
        }
    };

    /// Lance le constructeur de l'objet
    initInternal();

    return {

        /// <summary>
        /// Annule les evenments js 
        /// </summary
        cancel: function () { _userToolbar.cancel(); },

        /// <summary>
        /// Reçoit la fonction du callback sur l'evenement click
        /// </summary
        onClick: function (onClick) { _optionClick = onClick; },

        /// <summary>
        /// Reçoit la fonction du callback au démarrage du drag and drop
        /// Transmet l'info à la barre d'outil du UserMode
        /// </summary
        onDragStart: function (dragStart) { _userToolbar.onDragStart(dragStart); },

        /// <summary>
        /// Reçoit la fonction du callback pendant le drag and drop
        /// Transmet l'info à la barre d'outil du UserMode
        /// </summary
        onDragMove: function (dragMove) { _userToolbar.onDragMove(dragMove); },

        /// <summary>
        /// Reçoit la fonction du callback en fin de drag and drop
        /// Transmet l'info à la barre d'outil du UserMode
        /// </summary
        onDragEnd: function (dragEnd) { _userToolbar.onDragEnd(dragEnd); },

        /// <summary>
        /// Ajout les les btouton a la toolbar
        /// </summary
        setCustomItems: function (items) {  },
    }
};
