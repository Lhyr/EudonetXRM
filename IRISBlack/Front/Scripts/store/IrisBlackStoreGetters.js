/** Getters pour le store
 * Il est directement importé puis réexporté dans IrisBlackStoreFeatures.js */
const getters = {
    /** l'onglet */
    getTab: state => state.nTab,
    /** la liaison Evt */
    getEvtid: state => state.nEvtid,
    /** la liaison PP */
    getPPid: state => state.nPPid,
    /** la liaison PM */
    getPMid: state => state.nPMid,
    /** la valeur de la fiche courante. */
    getFileValue : state => state.sFileValue,
    /** la fiche */
    getFileId: state => state.nFileId,
    /** la date de dernier affichage de la fiche */
    getLastUpdate: state => state.lastUpdate,    
    /** Retourne l'url Web du dossier passé en paramètre (WebPathData) */
    getBaseUrl: state => state.baseUrl,
    /**
     * retourne la structure de la page
     */
    getTkStructPage: state => state.tkStructPage,

    /**
     * retourne les données de la page
     */
    getTkDataPage: state => state.tkDataPage,

    /**
     * retourne les structures des bookmarks
     */
    getTkStructBkm: state => state.tkStructBkm,

    /**
     * retourne les structures de la wizardbar
     */
    getTkStructCat: state => state.tkStructCat,

    /**
     * Nom de la base en cours.
     */
    getBaseName: state => state.baseName,

    /** 
     *  retourne le mutex des bookmarks
     */
    getMtxBookmarks: state => state.mtxBookmarks,

    /** retourne un mutex pour le menu. */
    getFileMenu: state => state.oFileMenu,

    /**
     * Renvoi l'objet du contenu de  l'infobulle
     * @param {any} state
     */
    getTooltipObj: state => state.tooltipObj,
    /**
     * Renvoi un objet contenant la page de chaque signet.
     * @param {any} state
     */
    getBkmPage: state => state.oBkmPage,
    /**
     * Nom de la revision en cours.
     */
    getRevision: state => state.revision,

    getHostName: state => window.location.hostname,
    /** pour l'indicateur global de chargement du mode Fiche guidé */
    getFileLoading: state => state.bFileLoading,
    getFileRoot: state => state.bFileRoot,

};

export { getters }