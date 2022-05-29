import EventBus from '../bus/event-bus.js?ver=803000';

/**
 * Permet de partager un lien.
 * @param {any} nTab
 * @param {any} nFileId
 * @param {any} hash
 */
function getShareFileURL(nTab, nFileId, hash) {
    var sGoUrl = window.location.protocol + "//" + window.location.host + window.location.pathname;
    sGoUrl = sGoUrl.substring(0, sGoUrl.lastIndexOf("/") + 1);
    sGoUrl = sGoUrl + "eGoToFile.aspx?tab=" + nTab + "&fid=" + nFileId + "&hash=" + encodeURIComponent(hash);
    return sGoUrl;
};

/**
 * Permet de partager un lien.
 * @param {any} nTab
 * @param {any} nFileId
 * @param {any} hash
 */
function shareFile(nTab, nFileId, hash) {
    var sGoUrl = getShareFileURL(nTab, nFileId, hash);

    if (navigator.clipboard) {
        navigator.clipboard.writeText(sGoUrl);
        var notifClipboard = {};
        notifClipboard.id = "shareFile";
        notifClipboard.title = this.getRes(2630); // Le lien a été copié
        notifClipboard.color = "green";
        // S'agissant d'une information importante pouvant potientiellement être perdue lors de la copie automatique, on propose de copier le lien dans une fenêtre eModalDialog séparée au clic sur la notification
        notifClipboard.selectOnClick = true;
        notifClipboard.selectTitle = this.getRes(2497); // Partager la fiche avec d'autres utilisateurs
        notifClipboard.selectLabel = notifClipboard.title;
        notifClipboard.selectValue = sGoUrl;
        notifToast(notifClipboard);
    }
    else {
        var promptLabel = "";
        if (window.location.protocol != 'https')
            promptLabel = this.getRes(2704).replace('<LINK>', ""); // Nous ne pouvons pas copier le lien automatiquement car vous accédez à l'application par un canal HTTP non sécurisé. Lien vers votre fiche : <LINK>
        else
            promptLabel = this.getRes(2631).replace('<LINK>', ""); // Votre navigateur ne permet pas de copier le lien automatiquement. Lien vers votre fiche : <LINK>
        ePrompt(top._res_2497, promptLabel, sGoUrl, 550, 200, true, false); // Partager la fiche avec d'autres utilisateurs
    }
}

/** Dialoge pour les propriétés. */
function emitPropertiesDialog() {
    let options = {
        typeModal: "info",
        type: "zoom",
        close: true,
        maximize: true,
        id: "prop-fiche",
        observeMenu: (bVal, ctx) => {
            this.observeRightMenu(bVal, ctx)
        },
        rightMenuWidth: window.GetRightMenuWidth(),
        title: this.getRes(54),
        btns: [{ lib: this.getRes(30), color: "default", type: "left" }],
        datas: this.propertyFiche,
    };

    EventBus.$emit('globalModal', options);
}


function emitMethod(ctx) {
    let options = {
        typeModal: "alert",
        color: "info",
        type: "zoom",
        close: true,
        maximize: true,
        id: 'alert-modal',
        title: ctx.getRes(7905),
        width: 600,
        btns: [{
            lib: ctx.getRes(30),
            color: 'default',
            type: 'left'

        }],
        datas: ctx.getRes(6368)
    }
    EventBus.$emit('globalModal', options);
}

/**
 * Accède à la fiche
 * @param {any} ntab
 * @param {any} nFileId
 */
function openPlainFile(ntab, nFileId) {
    var tabOrder = document.getElementById('eParam').contentWindow.document.getElementById('TabOrder');
    var tab = tabOrder.value.split(';');

    var ntabFound = tab.find(function (element) {
        return element == ntab;
    });

    if (ntabFound == undefined || ntab <= 0) {
        emitMethod(this);
    } else {
        top.loadFile(ntabFound, nFileId, 3);
    }
}

/**
 * Permet de convertir un onglet en mode liste.
 * @param {any} DescIdSignet
 * @param {any} id
 */
function reloadBkm(DescIdSignet, id) {
    var options = {
        id: id,
        signet: DescIdSignet,
        nbLine: 9,
        pageNum: 1,
        scrollLeft: this.$parent?.$el?.scrollLeft,
        bkmLoaded: false
    };
    EventBus.$emit('reloadSignet_' + id, options);
}

/**
 * Change le mode de la fiche.
 * @param {any} id
 * @param {any} nTab
 * @param {any} nBkm
 * @param {any} bkmviewmode
 */
async function changeViewMode(id, nBkm, bkmviewmode) {
    await this.setViewMode(bkmviewmode, nBkm);
    this.reloadBkm(nBkm, id);
}

export { shareFile, getShareFileURL, openPlainFile, emitPropertiesDialog, reloadBkm, changeViewMode };