const state = {
    nFileId: null,
    lastUpdate: null,
    nTab: null,
    nEvtid: null,
    nPPid: null,
    nPMid: null,
    sFileValue: "",
    baseUrl: "",
    tkStructPage: null,
    tkDataPage: null,
    tkStructBkm: null,
    tkStructCat: null,
    baseName: null,
    mtxBookmarks: null,
    tooltipObj: {},
    oBkmPage: {},
    revision: 0,
    oFileMenu: null,
    bFileLoading: true,
    bFileRoot:null
};

const actions = {

    /**
     * Permet de valider la modification du nTab dans le store
     * @param {any} commit
     * @param {any} newnTab
    */
    setnTab({ commit }, newnTab) { commit('setnTab', newnTab); },
    /**
     * Permet de changer le evt dans le store
     * @param {any} state
     * @param {any} newnEvtid
    */
    setnEvtid({ commit }, newnEvtid) { commit('setnEvtid', newnEvtid); },
    /**
     * Permet de valider la modification du nPP dans le store
     * @param {any} commit
     * @param {any} nNewPPid
    */
    setnPPid({ commit }, nNewPPid) { commit('setnPPid', nNewPPid); },
    /**
     * Permet de valider la modification du nPM dans le store
     * @param {any} commit
     * @param {any} nNewPMid
    */
    setnPMid({ commit }, nNewPMid) { commit('setnPMid', nNewPMid); },

    /**
     * Permet de changer le value de la page dans le store
     * @param {any} commit
     * @param {any} newnTab
    */
    setFileValue({ commit }, newsFileValue) { commit('setFileValue', newsFileValue) },
    /**
     * Permet de valider la modification du fileId dans le store
     * @param {any} commit
     * @param {any} newFileId
     */
    setFileId({ commit }, newFileId) { commit('setFileId', newFileId); },
    /**
     * Permet de valider la modification du lastUpdate dans le store
     * @param {any} commit
     * @param {any} newLastUpdate
     */
    setLastUpdate({ commit }, newLastUpdate) { commit('setLastUpdate', newLastUpdate); },
    /**
     * Permet de valider la modification du baseUrl dans le store
     * @param {any} commit
     * @param {any} newBaseUrl
     */
    setBaseUrl({ commit }, newBaseUrl) { commit('setBaseUrl', newBaseUrl); },

    /**
     * Permet de valider la modification de la structure de la page dans le store
     * @param {any} commit
     * @param {any} newTkStructPage
     */
    setTkStructPage({ commit }, newTkStructPage) { commit('setTkStructPage', newTkStructPage); },

    /**
     * Permet de valider la modification des données de la page dans le store
     * @param {any} commit
     * @param {any} newTkDataPage
     */
    setTkDataPage({ commit }, newTkDataPage) { commit('setTkDataPage', newTkDataPage); },

    /**
     * Permet de valider la modification des structures des bookmarks  dans le store
     * @param {any} commit
     * @param {any} newTkStructBkm
     */
    setTkStructBkm({ commit }, newTkStructBkm) { commit('setTkStructBkm', newTkStructBkm); },
    /**
     * Permet de valider la modification des structures de la wizardbar  dans le store
     * @param {any} commit
     * @param {any} newTkStructCat
     */
    setTkStructCat({ commit }, newTkStructCat) { commit('setTkStructCat', newTkStructCat); },

    /**
     * Nom de la base en cours.
     * @param {any} commit
     * @param {any} newBaseName
     */
    setBaseName({ commit }, newBaseName) { commit('setBaseName', newBaseName); },

    /**
     * Permet de valider la modification du mutex pour les signets.
     * @param {any} commit
     * @param {any} newBaseName
     */
    setMtxBookmarks({ commit }, newMtxBookmarks) { commit('setMtxBookmarks', newMtxBookmarks); },


    /**
     * Permet de valider la modification du mutex pour les menus du fichiers.
     * @param {any} commit
     * @param {any} newFileMenu
     */
    setFileMenu({ commit }, newFileMenu) { commit('setFileMenu', newFileMenu); },

    /**
     * Renvoie l'objet du contenu de l'infobulle
     * @param {any} commit
     * @param {any} tooltipObj
     */
    setTooltipObj({ commit }, tooltipObj) { commit('setTooltipObj', tooltipObj); },

    /**
     * Permet de valider la modification de l'enregistrement de la page courante pour les signets.
     * @param {any} commit
     * @param {any} newBkmPage
     */
    setBkmPage({ commit }, newBkmPage) { commit('setBkmPage', newBkmPage); },
    /**
     * Nom de la revision en cours.
     * @param {any} commit
     * @param {any} newRevision
     */
    setRevision({ commit }, newRevision) { commit('setRevision', newRevision) },
    /**
     * Permet de valider la valeur du chargement du mode Fiche guidé.
     * @param {any} commit
     * @param {any} newLoading La valeur à stocker dans le store (true ou false)
     */
    setFileLoading({ commit }, newLoading) { commit('setFileLoading', newLoading); },
    setFileRoot({ commit }, newFileRoot) { commit('setFileRoot', newFileRoot); },
}

export { state, actions };
export { getters } from "./IrisBlackStoreGetters.js?ver=803000"
export { mutations } from "./IrisBlackStoreMutations.js?ver=803000"