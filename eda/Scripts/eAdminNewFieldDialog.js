nsAdminNewFieldDialog = {};

nsAdminNewFieldDialog.getSpecField = function () {
    var objReturn = {};
    var txtNewFieldLabel = document.getElementById("txtNewFieldLabel");
    var ddlNewFieldDid = document.getElementById("ddlNewFieldDid");
    var ddlNewFieldType = document.getElementById("ddlNewFieldType");
    objReturn.label = txtNewFieldLabel.value;
    objReturn.did = getNumber(eTools.getSelectedValue(ddlNewFieldDid));
    objReturn.fieldType = nsAdmin.getCapsule(ddlNewFieldType);

    return objReturn;
};