
// Action du manager sur la widget
XrmWidgetAction =
{
    // Créer un nouveau widget avec ou sans liaison
    UNKNOWN_ACTION : -1,
    // Créer un nouveau widget avec ou sans liaison
    CREATE_WIDGET: 0,
    //Mets à jour
    UPDATE_WIDGET: 1,  
    // Suppression du widget sans supprimer la liaison
    DELETE_WIDGET: 2,
    // Ajout d'une liaison entre un widget et une page
    LINK_WIDGET: 3,
    // Supprime la liaison entre la page d'accueil et le widget
    UNLINK_WIDGET: 4,
    // Sauvegarde les prefs du widget pour l'utilisateur en cours
    SAVE_WIDGET_PREF: 5,
    // Supprime les pref concernant le widget pour la page d'accueil
    DELETE_WIDGET_PREF: 6,
    // Rafraichit le widget
    REFRESH_WIDGET: 7,
    // Sauvegarde l'affichage ou le masquague des widgets
    SAVE_VISIBLE_PREF: 8,
    // Sauvegarde d'un paramètre du widget
    SAVE_WIDGET_PARAM: 9
}


/// Action du client sur le widget demandée par le serveur
 XrmClientWidgetAction = 
{
    // Na rien faire
    DO_NOTHING:-1,
    // Retire le widget de dom
    REMOVE_FROM_DOM : 0,
    //Recharge le widget
    RELOAD_WIDGET: 1,
    //nouveau le widget
    NEW_WIDGET : 2
}

// Type de widget
var XrmWidgetType = {
    Image : 0,
    Editor : 1,
    WebPage : 2,
    Chart : 3,
    List : 4,
    Today : 5,
    Weather : 6,
    RSS : 7,
    Tuile : 8,
    Dashboard : 9,
    Historic : 10,
    Notification : 11,
    Mail: 12,
    Specif: 13,
    Indicator: 14,
    ExpressMessage: 15,
    Kanban: 16,
    Carto_Selection: 17,
    Unknown : -1
}
