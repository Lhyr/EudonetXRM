const mutations = {

    /**
     * Permet de changer le nTab dans le store
     * @param {any} state
     * @param {any} newnTab
    */
    setnTab(state, newnTab) { state.nTab = newnTab },
    /**
     * Permet de changer le evt dans le store
     * @param {any} state
     * @param {any} newnEvtid
    */
    setnEvtid(state, newnEvtid) { state.nEvtid = newnEvtid },
    /**
     * Permet de changer le nPP dans le store
     * @param {any} state
     * @param {any} newnPPid
    */
    setnPPid(state, newnPPid) { state.nPPid = newnPPid },

    /**
     * Permet de changer le nPM dans le store
     * @param {any} state
     * @param {any} newnPMid
    */
    setnPMid(state, newnPMid) { state.nPMid = newnPMid },

    /**
     * Permet de changer le value de la page dans le store
     * @param {any} state
     * @param {any} newnTab
    */
    setFileValue(state, newsFileValue) { state.sFileValue = newsFileValue },

    /**
     * Permet de changer le fileId dans le store
     * @param {any} state
     * @param {any} newFileId
     */
    setFileId(state, newFileId) { state.nFileId = newFileId },

    /**
     * Permet de mettre à jour la date de dernier affichage d'une fiche dans le store
     * @param {any} state
     * @param {any} newLastUpdate
     */
    setLastUpdate(state, newLastUpdate) { state.lastUpdate = newLastUpdate },


    /**
     * Permet de changer le basurl dans le store
     * @param {any} state
     * @param {any} newBaseUrl
     */
    setBaseUrl(state, newBaseUrl) { state.baseUrl = newBaseUrl },

    /**
     * Permet de changer la structure de la page dans le store
     * @param {any} state
     * @param {any} newTkStructPage
     */
    setTkStructPage(state, newTkStructPage) { state.tkStructPage = newTkStructPage },

    /**
     * Permet de changer les données de la page dans le store
     * @param {any} state
     * @param {any} newTkDataPage
     */
    setTkDataPage(state, newTkDataPage) { state.tkDataPage = newTkDataPage },

    /**
     * Permet de changer les structures des bookmarks  dans le store
     * @param {any} state
     * @param {any} newTkStructBkm
     */
    setTkStructBkm(state, newTkStructBkm) { state.tkStructBkm = newTkStructBkm },


    /**
     * Permet de changer les structures de la wizardbar dans le store
     * @param {any} state
     * @param {any} newTkStructCat
     */
    setTkStructCat(state, newTkStructCat) { state.tkStructCat = newTkStructCat },

    /**
     * Nom de la base en cours.
     * @param {any} state
     * @param {any} newBaseName
     */
    setBaseName(state, newBaseName) { state.baseName = newBaseName },

    /**
     * Permet de déclarer un mutex global pour les bookmarks.
     * @param {any} state
     * @param {any} newMtxBookmarks
     */
    setMtxBookmarks(state, newMtxBookmarks) { state.mtxBookmarks = newMtxBookmarks },

    /**
     * Permet de déclarer un mutex global pour le file menu.
     * @param {any} state
     * @param {any} newFileMenu
     */
    setFileMenu(state, newFileMenu) { state.oFileMenu = newFileMenu },

    /**
    * Construit/Ecrase l'objet du contenu de l'infobulle (texte, position etc.)
    * @param {any} state
    * @param {any} objUpdateVal
    */
    setTooltipObj(state, objUpdateVal) { state.tooltipObj = objUpdateVal },
    /**
     * Permet de modifier la page courante pour les signets.
     * @param {any} state
     * @param {any} newBkmPage
     */
    setBkmPage(state, newBkmPage) { state.oBkmPage = newBkmPage },

    /**
     * Nom de la revision en cours.
     * @param {any} state
     * @param {any} newRevision
     */
    setRevision(state, newRevision) { state.revision = newRevision },
    /**
    * Permet d'indiquer si le mode Fiche guidé est chargé ou non
    * @param {any} state
    * @param {any} loading
    */
    setFileLoading(state, newLoading) { state.bFileLoading = newLoading },
    setFileRoot(state, newFileRoot) { state.bFileRoot = newFileRoot },

};

export { mutations }