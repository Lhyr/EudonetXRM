import eAxiosHelper from "../helpers/eAxiosHelper.js?ver=803000";

const sURI = "/api/GraphTeamsEvent";

async function setTeamsEvent(context) {

    let helper = new eAxiosHelper(context.getUrl + sURI);
    let responseHold = await helper.PostAsync({
        Tab: context.getTab,
        FileId: context.getFileId,
        FieldTrigger: context.dataInput.DescId
    });
    return responseHold;
}


async function deleteTeamsEvent(tab, fileid) {
    console.log("deleteTeamsEvent not implemented");
}



export { setTeamsEvent, deleteTeamsEvent };