var nsAdminTeamsMapping = {};

nsAdminTeamsMapping.BookmarksOptions = [];
/**
 * les objets contenus dans ce tableaux sont au format suivant
  {
    "Tab": 2100,
    "GetMailFieldsOptions": {
        "2107": "Courriel"
    },
    "GetNameFieldsOptions": {
        "2116": "",
        "2119": "",
        "2120": "",
        "2121": "",
        "2176": "Alertes - Son",
        "2180": "Couleurs",
        "2113": "CP",
        "2104": "Nom",
        "2106": "Particule",
        "2115": "Pays",
        "2105": "Prénom",
        "2109": "Raison Sociale",
        "2110": "Rue 1",
        "2111": "Rue 2",
        "2112": "Rue 3",
        "2154": "Url QR code pour emailing",
        "2114": "Ville"
    },
    "GetFirstnameFieldsOptions": {
        "2116": "",
        "2119": "",
        "2120": "",
        "2121": "",
        "2176": "Alertes - Son",
        "2180": "Couleurs",
        "2113": "CP",
        "2104": "Nom",
        "2106": "Particule",
        "2115": "Pays",
        "2105": "Prénom",
        "2109": "Raison Sociale",
        "2110": "Rue 1",
        "2111": "Rue 2",
        "2112": "Rue 3",
        "2154": "Url QR code pour emailing",
        "2114": "Ville"
    }
}

 */



nsAdminTeamsMapping.init = function () {
    if (FieldsSelect) {
        nsAdminTeamsMapping.objFieldsSelect = new FieldsSelect();
    }
};


/**
 * un champ ne peut être utilisé qu'une fois
 * désactive l'option sélectionnée dans les autres listes de sélection
 * rend l'option qui vient d'être libérée dans les autres listes de sélection
 * @param {any} ddl
 */
nsAdminTeamsMapping.verifyAllOptions = function (ddl) {
    //console.log("verifyAllOptions")

};


nsAdminTeamsMapping.onChangeAction = function (ddl) {



    var iDescid = getNumber(eTools.getSelectedValue(ddl));

    if (ddl.id == "ddlBookmark")
        //Signet
        nsAdminTeamsMapping.loadBookmarkMappingOptions(iDescid);
    else
        //Rubriques
        nsAdminTeamsMapping.verifyAllOptions(ddl);


};

nsAdminTeamsMapping.loadBookmarkMappingOptions = function (nBkm) {


    if (!(nBkm > 0)) {

        ["ddlBkmMail", "ddlBkmName", "ddlBkmFirstName"].forEach(ddlId => {
            var ddl = document.getElementById(ddlId);

            while (ddl.options.length > 1) {
                ddl.remove(1); //on préserve "aucun élément selectionné"
            }
        }

        );


        return;
    }

    /*
     * on regarde si on les a deja
    if()

*/
    //sinon on va les chercher
    var upd = new eUpdater("eda/mgr/eAdminTeamsMapping.ashx", "1");

    upd.addParam("action", nsAdminTeamsMapping.Action.GetBookmarksFields, "post");
    upd.addParam("tab", nBkm, "post");

    upd.send(nsAdminTeamsMapping.loadBookmarkCallback);

};

nsAdminTeamsMapping.loadBookmarkCallback = function (oRes) {

    var oOptionsSet = JSON.parse(oRes);

    nsAdminTeamsMapping.BookmarksOptions.push(oOptionsSet);

    nsAdminTeamsMapping.refreshBookmarkInterface(oOptionsSet);

};


nsAdminTeamsMapping.refreshBookmarkInterface = function (oOptionsSet) {
    nsAdminTeamsMapping.refreshBookmarkDDL("ddlBkmMail", oOptionsSet.GetMailFieldsOptions);
    nsAdminTeamsMapping.refreshBookmarkDDL("ddlBkmName", oOptionsSet.GetNameFieldsOptions);
    nsAdminTeamsMapping.refreshBookmarkDDL("ddlBkmFirstName", oOptionsSet.GetFirstnameFieldsOptions);

};

nsAdminTeamsMapping.refreshBookmarkDDL = function (ddlId, oOptions) {
    var ddl = document.getElementById(ddlId);

    while (ddl.options.length > 1) {
        ddl.remove(1); //on préserve "aucun élément selectionné"
    }

    for (const [key, value] of Object.entries(oOptions)) {
        ddl.add(new Option(value, key));
    }

}


nsAdminTeamsMapping.getMapping = function () {
    var mapping = nsAdminTeamsMapping.getNewMapping();

    mapping.Enabled = getAttributeValue(document.getElementById("cbEnableTeams"), "chk") == "1";

    mapping.CreateSaveBtnDescId = eTools.getSelectedValueFromID("ddlSave");

    mapping.StartDateDescId = eTools.getSelectedValueFromID("ddlStart");
    mapping.EndDateDescId = eTools.getSelectedValueFromID("ddlEnd");
    mapping.TitleDescId = eTools.getSelectedValueFromID("ddlTitle");
    mapping.DescriptionDescId = eTools.getSelectedValueFromID("ddlDescription");

    mapping.UserRecipientsDescids = nsAdminTeamsMapping.objFieldsSelect.getSelectedFieldsList()
        .split(';')
        .map(d => getNumber(d))
        .filter(d => d > 0);



    var recipientFields = nsAdminTeamsMapping.getNewRecipientFields();
    recipientFields.TabDescId = eTools.getSelectedValueFromID("ddlBookmark");
    recipientFields.MailDescId = eTools.getSelectedValueFromID("ddlBkmMail");
    recipientFields.NameDescId = eTools.getSelectedValueFromID("ddlBkmName");
    recipientFields.FirstNameDescId = eTools.getSelectedValueFromID("ddlBkmFirstName");

    mapping.RecipientsFieldsDescIds = [recipientFields];
    mapping.URLMeetingDescid = eTools.getSelectedValueFromID("ddlURL");
    mapping.TeamsEventIdDescid = eTools.getSelectedValueFromID("ddlTeamsID");

    return mapping;
};

nsAdminTeamsMapping.isMappingValid = function (mapping) {
    //return mapping.StartDateDescId > 0
    //    && mapping.EndDateDescId > 0
    //    && mapping.TitleDescId > 0
    //    && mapping.TeamsEventIdDescid > 0;

    var recipientFields = mapping.RecipientsFieldsDescIds[0];
    if (recipientFields.TabDescId > 0 && !(recipientFields.MailDescId > 0))
        return false;

    return [mapping.StartDateDescId, mapping.EndDateDescId, mapping.TitleDescId, mapping.TeamsEventIdDescid].every(descid => descid > 0);
};

/*Enum d'interaction */

nsAdminTeamsMapping.Action = {
    Initial: 0,
    Save: 1,
    GetBookmarksFields: 2
};



/*Modèle des objets de mapping*/

nsAdminTeamsMapping.getNewMapping = function () {
    return {
        "Enabled": false,
        "CreateSaveBtnDescId": 0,
        "DeleteBtnDescId": 0,
        "StartDateDescId": 0,
        "EndDateDescId": 0,
        "TitleDescId": 0,
        "DescriptionDescId": 0,
        "UserRecipientsDescids": [],
        "RecipientsFieldsDescIds": [],
        "URLMeetingDescid": 0,
        "TeamsEventIdDescid": 0
    };
};

nsAdminTeamsMapping.getNewRecipientFields = function () {

    return {
        "TabDescId": 0,
        "MailDescId": 0,
        "NameDescId": 0,
        "FirstNameDescId": 0
    };
};

