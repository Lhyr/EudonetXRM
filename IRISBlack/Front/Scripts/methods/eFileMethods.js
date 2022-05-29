import EventBus from "../bus/event-bus.js?ver=803000";
import { setNormalizeString, JSONTryParse } from "./eMainMethods.js?ver=803000"
import eAxiosHelper from "../helpers/eAxiosHelper.js?ver=803000";
import { loadFileBookmark, loadFileCatalog, loadFileDetail, loadFileLayout, loadStatusNewFile, getParamWindow } from '../shared/XRMWrapperModules.js?ver=803000'
import { PinnedBookmarkControllerOperation, typeBkm } from "./Enum.js?ver=803000";

/**
 * Récupère les données auprès du controleur
 * via une variable du store.
 * @param {any} getter
 */
function LoadDataFromStore(getter) {
    let helper = getter?.link;
    let params = getter?.params;

    return helper.GetAsync({
        params: params,
        responseType: 'json'
    });
}

/**
 * Chargement de la structure de la page.
 * @param {boolean} bForceReload
 * */
async function LoadStructPage(bForceReload = false) {

    if (bForceReload)
        this.setTkStructPage({ link: this.getTkStructPage.link, params: this.getTkStructPage.params, results: LoadDataFromStore(this.getTkStructPage) });

    try {
        this.DataJson = !this.DataJson || bForceReload ? JSONTryParse(await this.getTkStructPage.results) : this.DataJson;

        this.JsonSummary = JSONTryParse(this.DataJson.JsonSummary || "{}");
        this.JsonWizardBar = JSONTryParse(this.DataJson.JsonWizardBar || "{}");
        this.NbCols = parseInt(this.DataJson.NbCols || 0);

    } catch (e) {
        throw e;
    }
}


/**
 * Chargement de la structure des signets.
 * @param {boolean} bForceReload
 * */
async function LoadStructBkm(bForceReload = false) {

    // if(this.propDetail?.length > 1)
    //     return 

    if (bForceReload)
        this.setTkStructBkm({ link: this.getTkStructBkm.link, params: this.getTkStructBkm.params, results: LoadDataFromStore(this.getTkStructBkm) });

    try {
        this.BkmStruct = !this.BkmStruct || bForceReload ? JSONTryParse(await this.getTkStructBkm.results) : this.BkmStruct;
    } catch (e) {
        throw e;
    }
}

/**
 * Chargement des données de la page.
 * @param {boolean} bForceReload
 * */
async function LoadDataPage(bForceReload = false) {

    if (bForceReload)
        this.setTkDataPage({ link: this.getTkDataPage.link, params: this.getTkDataPage.params, results: LoadDataFromStore(this.getTkDataPage) });

    try {
        this.DataStruct = !this.DataStruct || bForceReload ? JSONTryParse(await this.getTkDataPage.results) : this.DataStruct;
    } catch (e) {
        throw e;
    }
}

/**
 * Chargement du catalogue pour la Wizardbar
 * @param {boolean} bForceReload
 * @param {object} oNewParams Les nouveaux paramètres de rechargement à utiliser. Si non indiqués ou null, on reprendra les paramètres utilisés lors du dernier appel
 */
async function LoadWzCatalog(bForceReload = false, oNewParams = null) {

    if (!this.getTkStructCat) {

        if (!oNewParams)
            return;

        let oDataCatalog = loadFileCatalog();

        this.setTkStructCat({
            link: new eAxiosHelper(oDataCatalog.url),
            params: oNewParams,
            results: linkToCall(new eAxiosHelper(oDataCatalog.url), oNewParams)
        });
    }

    if (!oNewParams)
        oNewParams = this.getTkStructCat.params;


    if (!oNewParams)
        return;

    if (bForceReload)
        this.setTkStructCat({ link: this.getTkStructCat.link, params: oNewParams, results: LoadDataFromStore({ link: this.getTkStructCat.link, params: oNewParams }) });

    try {
        this.DataCatalogue = !this.DataCatalogue || bForceReload ? JSONTryParse(await this.getTkStructCat.results) : this.DataCatalogue ;
    } catch (e) {
        throw e;
    }
}


/**
 * Charge les données pour un catalogue, en appelant le back.
 * @param {any} descid
 */
async function CallCatalogBack(descId) {

    let oDataCatalog = loadFileCatalog();

    if (!(oDataCatalog))
        return;

    let param = {
        descid: descId
    };

    return linkToCall(
        oDataCatalog.url,
        { ...oDataCatalog.params, ...param }
    );
}




/**
 * Chargement des catalogues
 * @param {int} descId le descid du champs du catalogue.
 */
async function LoadCatalog(descId) {
    this.DataCatalogue = JSONTryParse(await CallCatalogBack(descId))
}



/**
 * Chargement de la structure de la page.
 * @param {int} bkmDescId le descId du bkm
 * @param {int} number le nombre d'enregistrements à retourner.
 * */
async function LoadSignet(bkmDescId, number, newPageCall, type) {
    let oDataBookmark = await loadFileBookmark();

    if (!(oDataBookmark))
        return;

    let param = {
        ParentTab: this.did,
        ParentFileId: this.fid,
        Bkm: bkmDescId,
        RowsPerPage: (!number) ? 9 : number,
        Page: (!newPageCall || !newPageCall.pageNum) ? 1 : newPageCall.pageNum,
    };

    let oDataJson = await linkToCall(
        oDataBookmark.url,
        { ...oDataBookmark.params, ...param }
    )

    if (type != typeBkm.grid)
        oDataJson = JSONTryParse(
            oDataJson
        );

    return oDataJson;
}

/**
 * Chargement des MRU et des valeurs d'un catalogue utilisateur
 * @param {any} UserValuesRequestModel le modèle de données comportant les données à transmettre au backend
 */
async function LoadUserValues(UserValuesRequestModel) {
    let oDataUsers = this.loadFileUsers();

    if (!(oDataUsers))
        return;

    let param = {
        UserValuesRequestModel: UserValuesRequestModel
    };

    this.DataMru = JSONTryParse(
        await linkToPost(
            oDataUsers.url,
            { ...oDataUsers.params, ...param }
        )
    )?.Values;
}

/**
 * Tri les éléments d'une liste basé sur une autre liste
 */
function MapOrder(array, order, key) {

    array.sort(function (a, b) {
        var A = a[key], B = b[key];

        if (order.indexOf(A) > order.indexOf(B)) {
            return 1;
        } else {
            return -1;
        }

    });

    return array;
};

/**
 * Construction du Header de la fiche.
 * Suite à la demande 83 502, l'avatar dans le champ 75 n'est plus l'avatar par défaut.
 * Si on n'a pas d'avatar, on n'affiche rien.
 * @param {object} ctx le contexte.
 * */
function ConstructHeaderFile(ctx) {
    let idFirstField = 1;
    let idSecondField = 202;
    let idAvatar = 75;

    // DEBUT HEAD FICHE //
    // Set value titre header fiche
    let titleValue = ctx.tabAllDetail.find(a => a.DescId == ctx.JsonSummary.title);
    let defaultTitleValue = ctx.tabAllDetail.find(a => a.DescId == this.getTab + 1);
    let titleComplementValue = {};
    if (this.getTab == 200) {
        titleComplementValue = ctx.tabAllDetail.find(a => a.DescId == idSecondField);
    }

    ctx.tabHeader.titleError = { value: false };

    //ELAIZ - Par défaut si on a pas de titre dans le json, on met par défaut PP01/PM01...
    if (!titleValue || titleValue.DisplayValue == "" && titleValue.Value == "" || ctx.forbiddenFormatHead(titleValue.Format, ctx.tabFormatForbidHeadEdit)) {
        //ctx.tabHeade.title = false
        ctx.tabHeader.title = defaultTitleValue;
        ctx.tabHeader.titleComplementValue = titleComplementValue;
        ctx.tabHeader.titleError = { value: ctx.forbiddenFormatHead(titleValue?.Format, ctx.tabFormatForbidHeadEdit), title: titleValue };
    } else {
        ctx.tabHeader.title = titleValue;

        if (ctx.JsonSummary.title == parseInt(ctx.did) + 1 && titleComplementValue)
            ctx.tabHeader.titleComplementValue = titleComplementValue
    }


    // Set value sous titre header fiche
    let sTitleValue = ctx.tabAllDetail.find(a => a.DescId == ctx.JsonSummary.sTitle);

    if (!sTitleValue) {
        ctx.tabHeader.sTitle = false;
    } else {
        ctx.tabHeader.sTitle = sTitleValue;
        if (ctx.JsonSummary.sTitle == parseInt(ctx.did) + idFirstField && titleComplementValue)
            ctx.tabHeader.sTitleComplementValue = titleComplementValue;
    }

    // Set value avatar header fiche
    let avatarValue;

    if (ctx.JsonSummary.avatar % 100 != 1)
        avatarValue = ctx.tabAllDetail?.find(a => a.DescId == ctx.JsonSummary.avatar);
    //var avatar = avatarValue ? avatarValue : ctx.tabAllDetail.find(a => a.DescId == parseInt(ctx.did) + idAvatar);

    ctx.tabHeader.avatar = !avatarValue ? false : avatarValue;

    if (this.JsonSummary.inputs) {
        //Filtre les détails des tables disponibles
        ctx.tabHeader.inputs = ctx.tabAllDetail
            ?.filter(a => this.JsonSummary.inputs.find(b => a?.DescId == b?.DescId))
            .map(a => {
                if (a.DescId % 100 == 1
                    && a.DescId != this.getTab + 1) {
                    a.LabelHidden = false;
                    //a.ReadOnly = true;
                    a.IsMiniFileEnabled = true;
                }

                return a;
            });

        //Sert à stocker les identifiants
        var item_order = [];

        //Ajoute chaque "DescId" dans une liste
        for (var i = 0; i < this.JsonSummary.inputs.length; i++) {
            item_order.push(this.JsonSummary.inputs[i].DescId);
        }

        /* Tri la liste par identifiant, en se basant sur les identifiants précédemment récupérés
         * dans le item_order.
         */
        ctx.tabHeader.inputs = MapOrder(ctx.tabHeader.inputs, item_order, 'DescId');
        // Vue.set(ctx.tabHeader.inputs, MapOrder(ctx.tabHeader.inputs, item_order, 'DescId'),ctx.tabHeader.inputs)
    }
    ctx.summaryGoSkeleton = false;
}

/**
 * Construit les propriétés de la fiche.
 * @param {any} EdnType un enum dont j'ai besoin plutot que de l'importer
 * */
function ConstructPropertyFile(EdnType) {
    let tabIDMainFile = [99, 88, 84, 95, 97, 93, 96, 98, 74];
    let tabIDElseType = [99, 90, 84, 95, 97, 93, 96, 98, 74];
    //Utilisation de la méthode sort() pour mettre dans l'ordre souhaité les champs (par rapport à sortingArr)
    let sortingArr = [99, 88, 90, 96, 98, 84, 95, 97, 74, 93];

    // DEBUT ZONE PROPRIETE DE LA FICHE //
    // Set value proppriété de la fiche
    this.propertyFiche = this.tabAllDetail.filter(a => (
        (this.DataStruct.Structure.StructFile.EdnType == EdnType.FILE_MAIN
            && tabIDMainFile.map(num => parseInt(this.did) + num).includes(a.DescId))
        || tabIDElseType.map(num => parseInt(this.did) + num).includes(a.DescId)
    )).sort((a, b) => {
        return sortingArr.indexOf(a.DescId - this.did) - sortingArr.indexOf(b.DescId - this.did);
    });
}

/**
 * COnstruction des signets
 * @param {any} EdnType un enum dont j'ai besoin plutot que de l'importer
 */
async function ConstructBookmarkFile(EdnType, bForceReload= true) {

    // DEBUT SIGNETS DE LA FICHE (en construction) //
    this.tabs = this.tabs || [
        {
            id: "signet",
            Label: top._res_859,
            active: false,
            type: 'signet',
            signets: [],
            removable: false,
            pinnedTab: false
        },
        {
            id: "details",
            Label: top._res_860,
            active: false,
            type: 'detail',
            removable: false,
            pinnedTab: false
        }
    ];
    // Ajout des signets épinglés dans un tableau séparé pour pouvoir agir dessus séparément (tri)
    //this.pinnedTabs = [];

    this.tabs.forEach((tb) => {
        if (tb.id == "signet") {
            tb.signets.forEach((bkm, ix) => {
                let bkmStrctFound = this.BkmStruct.find(bks => bks.DescId == bkm.DescId);

                if (!bkmStrctFound) {

                    if (bkm.IsPinned) {
                        let pinned = this.tabs.find(bk => bk.DescId == bkm.DescId);

                        if (pinned) {
                            let idxPinned = this.tabs.findIndex(bk => bk.DescId == bkm.DescId);
                            let idxTabPinned = this.pinnedTabs.findIndex(bk => bk.DescId == bkm.DescId);
                            this.tabs.splice(idxPinned, 1);
                            this.pinnedTabs.splice(idxTabPinned, 1);
                        }
                    }

                    tb.signets.splice(ix, 1);

                }
            });
        }
    });

    this.BkmStruct
        ?.filter(bkm => bkm.Label != null && bkm.Error == null)
        ?.forEach(bkm => {
            let descId = bkm.DescId;
            let Id = 'id_' + bkm.DescId + '_' + setNormalizeString(bkm.Label);
            let lbl = bkm.Label;
            let tblType = bkm.TableType;
            let act = bkm.Actions;
            let relationdescid = bkm.RelationFieldDescId;

            if (bkm.TableType == EdnType.FILE_GRID) {
                let oTab = {
                    DescId: descId,
                    id: Id,
                    Label: lbl,
                    active: false,
                    type: 'grille',
                    TableType: tblType,
                    Actions: act,
                    removable: false,
                    pinnedTab: true
                }

                let actBkm = this.tabs.find(bkmTab => bkmTab.DescId == bkm.DescId);

                if (actBkm)
                    actBkm = { ...actBkm, ...oTab }
                else
                    this.tabs.push(oTab);
            } else {
                /** Attentions réécriture de StructBkmModel */
                let oBkm = {
                    DescId: descId,
                    HistoricActived: bkm.HistoricActived,
                    ExpressFilterActived: bkm.ExpressFilterActived,
                    IsMarkettingStepHold: bkm.IsMarkettingStepHold,
                    ViewMode: bkm.ViewMode,
                    id: Id,
                    Label: lbl,
                    loaded: false,
                    preventLoad: true,
                    type: 'signet',
                    TableType: tblType,
                    Actions: act,
                    RelationFieldDescId: relationdescid,
                    IsPinned: bkm.IsPinned,
                    PinnedOrder: bkm.PinnedOrder,
                    ListTargetScenario: act.EventTargetForScenario ? bkm.ListTargetScenario : null
                }

                let actBkm = this.tabs[0].signets?.find(bkmTab => bkmTab.DescId == bkm.DescId);

                if (actBkm)
                    actBkm = { ...actBkm, ...oBkm }
                else
                    this.tabs[0].signets.push(oBkm);
                /*}*/
            }

            let oDataDetail = loadFileDetail();
            let oFileLayout = loadFileLayout();
            let oDataBookmark = loadFileBookmark();
            if (bkm.IsPinned) {

                let oPinnedBkm = {
                    DescId: descId,
                    HistoricActived: bkm.HistoricActived,
                    ExpressFilterActived: bkm.ExpressFilterActived,
                    IsMarkettingStepHold: bkm.IsMarkettingStepHold,
                    ViewMode: bkm.ViewMode,
                    id: "pinned_" + Id,
                    Label: lbl,
                    active: false,
                    type: 'pinned-bkm',
                    loaded: false,
                    preventLoad: true,
                    TableType: tblType,
                    Actions: act,
                    RelationFieldDescId: relationdescid,
                    IsPinned: bkm.IsPinned,
                    PinnedOrder: bkm.PinnedOrder,
                    removable: true,
                    pinnedTab: true,
                    empty: false,
                    promBkmData: linkToCall(oDataBookmark.url,
                        { ...oDataBookmark.params, ParentTab: this.getTab, ParentFileId: this.getFileId, Bkm: parseInt(descId), RowsPerPage: 1, Page: 1, IsPinned: true }
                    ).catch(error => top.eAlert(1, this.getRes(412), error.message, error.stack)),
                    promBkmFileLayout: linkToCall(oFileLayout.url, { ...oFileLayout.params, nTab: parseInt(descId) }).catch(error => top.eAlert(1, this.getRes(412), error.message, error.stack))
                };
                // Ajout du signet épinglé dans l'ordre donné par le back
                if (bkm.PinnedOrder > -1)
                    this.pinnedTabs[bkm.PinnedOrder] = oPinnedBkm;
                // Ou à la fin à défaut
                else
                    this.pinnedTabs.push(oPinnedBkm);
            }

        });

    // Vérifie que la fiche épinglée n'est pas vide
    this.pinnedTabs.forEach(async tab => {
        var detailsFile = await tab.promBkmData;
        if (typeof detailsFile == 'string')
            detailsFile = await JSONTryParse(detailsFile);
        if (detailsFile?.Data?.length < 1 || detailsFile?.length < 1) {
            tab.empty = true;
        }
    })

    // Ajout des signets épinglés ordonnés dans le tableau principal, à la fin des autres, par spread operator
    this.tabs.push(...this.pinnedTabs.filter(n => n));
    this.tabs = this.tabs.filter((pin, idx, arrTab) =>
        idx === arrTab.findIndex((p) => (
        p.id === pin.id
        ))
    );

    let oSearch = { nTab: parseInt(this.getTab) };

    this.dbBkm = this.dbBkm || await this.initDatabaseIrisBlack();
    let savedPinnedBkmTab = await this.whereDataDbAsync(this.dbBkm, 'Bookmark', oSearch);
    let activeBkm;
    if(Array.isArray(savedPinnedBkmTab))
        activeBkm = savedPinnedBkmTab?.find(pin => pin)?.bkm || 'details';
    else
        activeBkm = savedPinnedBkmTab?.bkm || 'details';

    // if the bkm is not actived, the default page will be the file detail
    let activeTab = this.tabs.find(tabElm => tabElm.id == activeBkm);
    if (activeTab) {
        activeTab.active = true;
    } else {
        let detailTab = this.tabs.find(tabElm => tabElm.id == 'details');
        detailTab.active = true;
    }
    this.tabs = this.tabs.filter(tab => tab);

    this.tabs
        .filter(tab => tab.id == 'signet')
        .flatMap(tab => tab.signets)
        .forEach(signet => {
            signet.loaded = false;
            if (bForceReload) {
                signet.preventLoad = true;
            }
        });

    // ELAIZ/MABBE - Demande #90 121 et TK #5 834 - Ces 2 booléens gèrent l'affichage du skeleton, et le chargement des signets uniquement s'ils figurent dans la zone affichée à l'écran (Intersection Observer)
}
/**
 * Construction du Catalogue
 * @param {any} ctx le contexte appelant.
 * */
async function ConstructCatalog(ctx) {

    let fldById = ctx.JsonWizardBar?.FieldsById;

    ctx.propStep = ctx.getTabAllDetailMinusPLinks.find(a => a.DescId == ctx.JsonWizardBar.DescId);
    if (!ctx.propStep) {
        top.eAlert(0, this.getRes(7050), this.getRes(6486).replace('<FIELDTYPE>', ctx.JsonWizardBar.DescId).replace('<TABNAME>', nGlobalActiveTab), null);
        return false;
    }
    ctx.propStep.FieldsById = [];


    //on masque les valeurs cachées (désactivées) non sélectionnée
    ctx.DataCatalogue["Values"] = ctx.DataCatalogue?.Values?.filter(value => !value.Hidden || ctx.propStep.Value == value.DbValue);

    if (!ctx.DataCatalogue["Values"])
        return;


    ctx.DataCatalogue.Values.forEach((a) => {

        let findItemDataCat = fldById?.find(b => b.DataId == a.DbValue);

        if (findItemDataCat) {

            findItemDataCat.DataIdPrevious ? a.DataIdPrevious = findItemDataCat.DataIdPrevious : null;
            findItemDataCat.DataIdNext ? a.DataIdNext = findItemDataCat.DataIdNext : null;
            if (!Array.isArray(a.DataIdPrevious) && Number(a.DataIdPrevious) > 0)
                a.DataIdPrevious = [a.DataIdPrevious];
            if (!Array.isArray(a.DataIdNext) && Number(a.DataIdNext) > 0)
                a.DataIdNext = [a.DataIdNext];

            a.DisplayedDescId = findItemDataCat.DisplayedFields;
            ctx.propStep.FieldsById.push(a);

            a.DisplayedDescId?.forEach((b, idx) => {
                let findItemData = ctx?.propDetail?.find(c => c?.DescId == b?.DescId);
                if (findItemData) {
                    a.DisplayedDescId[idx] = findItemData;
                }
            });

            let bTabStep = ctx.propStep.Value == a.DbValue;
            a.active = bTabStep;
            a.validate = bTabStep;
            a.focus = bTabStep;
        }
    });


    await Vue.nextTick();

    //EndingCatalog();
}

/**
 * Fin de la construction d'un catalogue.
 * */
function EndingCatalog() {
    let iReduceHeight = 200;
    let options = {
        float: true,
        width: 12,
        height: 0,
        staticGrid: true,
        animate: true,
        alwaysShowResizeHandle: true,
        cellHeight: 20,
        verticalMargin: 10,
        horizontalMargin: 10,
        minWidth: 1100,
        placeholderClass: 'grid-stack-placeholder',
        acceptWidgets: '.grid-stack-item.mainItem'
    };
    let droppables = [{
        x: 0,
        y: 0,
        width: 1,
        height: 1
    }];


    // On set la grille "gridStack" quand on change d'onglet

    $('.grid-stack.outer').gridstack(_.defaults(options));
    $('.grid-stack-item.mainItem').draggable({ cancel: ".not-draggable" });

    $('.grid-stack-item.mainItem').resize((el, width, height) => {
        let MyHeight = el.target.clientHeight;

        let note = el.target.getElementsByClassName('note-steps');
        if (note.length > 0) {
            note.forEach(nte => nte.style.maxHeight = MyHeight - iReduceHeight + 'px');
        }
    });

}


/**
 * Observateur pour chaque signet. 
 * Si on lui rentre dedans, alors, il s'affiche.
 * ATTENTION : cette fonction est déclenchée par un updated() sur BkmUnit et BkmAnnexe.
 * Or, updated() se déclenche lorsque le template est mis à jour, via des éléments réactifs notamment.
 * Par conséquent, il ne faut PAS modifier un élément du template dans cette fonction, mais uniquement des props.
 * Les accès aux éléments du template/DOM ne doivent se faire qu'en lecture.
 * Dans le cas contraire, cela pourrait entraîner des comportements imprévisibles et très difficiles à débugguer.
 * @param {any} signet
 */
function setBkmObserver(signet, props) {

    let observerHead = new IntersectionObserver(async ([e]) => {

        // On refuse le chargement du signet si l'onglet parent "Signets ..." n'est pas encore affiché dans le DOM, ou si un autre ("Détails", "Grille", ...) est sélectionné
        // TODO: à améliorer. Pour l'instant, on fait une vérification niveau DOM et non pas niveau VueJS (template) car les variablaes du template peuvent avoir la bonne valeur (ex : tabContent.type == "signet")
        // alors que le contenu n'est en réalité pas encore visible à l'écran (page en cours de chargement). Mais ce type de test est loin d'être idéal, et reste fragile si on modifie la structure de la page, ou les CSS.
        let bIsBkmListHidden = (!document.getElementById("signet")
            || (document.getElementById("signet")?.className
                && document.getElementById("signet").className.indexOf("active") < 0));
        // Le signet est-il épinglé et affiché ?
        let bIsCurrentBookmarkPinned = (props?.type == "pinned-bkm" || props?.pinnedTab == true);
        // identification du DescID du signet épinglé actif (si on est sur un signet épinglé)
        let nActivePinnedBkmDescID = 0;
        if (bIsCurrentBookmarkPinned) {
            let sPinnedBkmPrefix = "pinned_id_"; // préfixe utilisé sur les ID de signets épinglés. Exemple d'ID complet : pinned_id_3100_Courriels
            let sActivePinnedBkmID = this.$el.id.replace(sPinnedBkmPrefix, ""); // on le supprime : pinned_id_3100_Courriels devient 3100_Courriels
            let aActivePinnedBkmDescIDLabel = sActivePinnedBkmID?.split("_"); // on divise l'ID : 3100_Courriels devient ["3100", "Courriels"]
            if (aActivePinnedBkmDescIDLabel?.length > 0)
                nActivePinnedBkmDescID = Number(aActivePinnedBkmDescIDLabel[0]); // "3100" devient 3100
        }
        // TK #6141 - fallback en cas d'erreur : on autorise aussi le chargement si on est sur un signet épinglé dont on a pas pu déterminer le DescID avec le code du haut (= si le code ci-dessus devient inopérant)
        // Mieux vaut charger un signet de manière non optimisée (= quand il n'est pas visible) que de ne jamais le charger en affectant preventLoad à true par erreur
        let bIsCurrentBookmarkPinnedAndHidden = bIsCurrentBookmarkPinned && !isNaN(nActivePinnedBkmDescID) && nActivePinnedBkmDescID > 0 && nActivePinnedBkmDescID != props?.DescId;
        // Si le signet n'a pas encore fait l'objet d'un premier chargement
        if (props?.loaded) {
            observerHead.unobserve(signet);
            // On autorise le chargement (preventLoad == false) si le signet n'a pas encore été chargé, mais uniquement si "Signets ..." est actif (bIsBkmListHidden == false) et qu'on affiche pas un signet épinglé (bIsCurrentBookmarkPinnedAndHidden = false)
            props.preventLoad = bIsBkmListHidden && bIsCurrentBookmarkPinnedAndHidden;
            return;
        }

        // Sinon, on vérifie s'il est dans la zone visible par l'utilisateur (= pas besoin de scroller)
        if (e.isIntersecting && e.intersectionRatio > 0) {
            props.loaded = true;
            props.preventLoad = bIsBkmListHidden && bIsCurrentBookmarkPinnedAndHidden; // On autorise le chargement (preventLoad == false) si le signet est dans la zone visible du scroll ET uniquement si "Signets ..." est actif (bIsBkmListHidden == false) et qu'on affiche pas un signet épinglé (bIsCurrentBookmarkPinnedAndHidden = false)
            /* Pour le moment en commentaire. A voir si on laisse là, car notamment ca pose le problème du scroll vers le haut et 
             * de l'ancre, qui est en fait un gros scroll des familles */
            //this.dbBkm = await this.initDatabaseBookmark();
            //this.mergeDataDbAsync(this.dbBkm, "Bookmark", { nTab: this.getTab, type: "signet", bkm: props.id });

            // TK #6050 (reprise #5992) - Le contenu d'un signet épinglé doit toujours être chargé lorsqu'on affiche explicitement ledit signet épinglé.
            // Même si la version non épinglée de ce même signet ne se trouve pas dans la zone visible lorsqu'il est affiché dans la vue "Signets..."
            if (!this.bkmLoaded && (props.type == "pinned-bkm" || props.pinnedTab) && props.promBkmData) {
                this.bkmLoaded = true;
                props.preventLoad = false;
            }


        }
        // Si le signet ne se situe pas dans la zone visible, on bloque son affichage dans tous les cas de figure, que l'onglet "Signets ..." soit sélectionné ou non (bIsBkmListHidden)
        else {
            props.preventLoad = true;
        }
    }, {} /*{ threshold: [0, 1] }*/);

    return observerHead;
}



/**
 * Pour les CSS suivant le format on a des affichages spéciaux.
 * @param {any} tab les formats en compte
 * @param {any} format le format de l'élément.
 * @returns {boolean} vrai si l'élément est dans le tableau.
 */
function specialCSS(tab, format) {
    return tab.includes(format);
}
/**
 * retourne si un format n'est pas autorisé dans le head.
 * Pour bien suivre, ça retourne vrai si le format est interdit.
 * Elle se sert de la fonction précédente, qui est similaire.
 * @param {any} format le format en question.
 * @param {any} tab par défaut il réprésente la liste des rubriques interdit dans le head mais peut être remplacé
 * @returns {any} un booléen qui dit que c'est un des format interdit.
 */
function forbiddenFormatHead(format, tab = this.tabFormatForbid) {
    return this.specialCSS(tab, format);
}


function addNewFile(nBkmTab) {
    var percent = 90;
    var size = getWindowSize();
    size.h = size.h * percent / 100;
    size.w = size.w * percent / 100;
    let descIdSignet = nBkmTab.DescIdSignet;
    var afterValidate = function () {
        var options = {
            id: nBkmTab.id,
            signet: descIdSignet,
            nbLine: 9,
            pageNum: 1
        };
        EventBus.$emit('reloadSignet_' + nBkmTab.id, options);
    };

    let tableType = this.$options.propsData.propAction ? this.$options.propsData.propAction.TableType : this.signet.TableType

    switch (tableType) {
        case EDNTYPE_MAIL:
            shFileInPopup(descIdSignet, 0, nBkmTab.name, size.w, size.h, 1, null, false, afterValidate, CallFromSendMail);
            break;
        case EDNTYPE_PJ:
            setPreventLoadBkmList(true);
            showAddPJ(null, () => {
                afterValidate();
                setPreventLoadBkmList(false);
            },{ width:650 , height:550 });
            break;
        default:
            shFileInPopup(descIdSignet, 0, nBkmTab.name, size.w, size.h, false, null, true, afterValidate, CallFromBkm);
            break;
    }

    if (eModFile)
        eModFile.ErrorCallBack = function () { console.log("Error survenue lors de la creation d'une nouvelle fiche dans le signet") }

}

/**
 * Permet d'observer si le menu de droite est ajouté ou non
 * @param {any} bVal lance ou arrête l'observation du menu de droite
 * @param {any} ctx récupère le contexte du composant modale ( this étant égale à fiche ou headFiche dans ce cas)
 */
function observeRightMenu(bVal, ctx) {

    if (bVal) {
        const targetNode = document.querySelector('#rightMenu');

        const config = { attributes: true, childList: true, subtree: true };

        const callback = (mutationsList, observer) => {
            for (const mutation of mutationsList) {
                if (mutation.type === 'attributes' && mutation.target == document.querySelector('#rightMenu')) {
                    ctx.rightMenuWidth = window.GetRightMenuWidth()
                    return;
                }
            }
        };

        this.observer = new MutationObserver(callback);

        this.observer.observe(targetNode, config);
    } else {
        this.observer.disconnect();
    }
}

/**
 * Retourne les liaisons hautes de la fiche, indexées par TabID (ex : ["200"] = ParentPP, ["300"] = ParentPM, etc.)
 * si aucun TabID n'est passé en paramètre. Sinon, renvoie directement le FileID correspondant au TabID parent passé en paramètre
 * */
function getFkLinksByTab(tab) {
    let lnkInfos = document.getElementById("lnkid_" + this.getTab);
    let fkLinks = new Array();
    if (lnkInfos) {
        lnkInfos.value.split(";").forEach(element => {
            let keyValuePair = element.split("=");
            if (keyValuePair?.length > 1)
                fkLinks[keyValuePair[0]] = keyValuePair[1];
        });
    }
    if (tab)
        return fkLinks[tab];
    else
        return fkLinks;
}

/**
 * Initialise la connexion a la base Bookmark de IndexedDB
 * Et revoie une promesse les enregistrements.
 * */
async function initDatabaseIrisBlack() {
    try {
        return this.initDatabase({
            Bookmark: "&nTab,type,bkm",
            Activity: "&nTab,active,index",
            ComputedValue: "[tab+bkm+descId],active",
            Separator: "[tab+descId],closed"
        });
    }
    catch (e) {
        this.manageDatabaseIrisBlack(e);
    }
}


/**
 * Initialise la connexion a la base de IndexedDB
 * Et revoie une promesse les enregistrements.
 * @param {any} colName
 */
async function initDatabase(colName) {
    try {
        let nmDb = `IrisBlack_${this.getHostName}_${this.getBaseName}_${this.getUserID}`;

        return this.initIndexedDB(nmDb, colName, this.getRevision);
    }
    catch (e) {
        this.manageDatabaseIrisBlack(e);
    }
}

/**
 * Fonction vouée à intercepter toutes les erreurs survenant sur la gestion de la base de données
 */
async function manageDatabaseIrisBlack(e) {

    if (e.inner.name == "VersionError") {
        try {
            Dexie.delete(`IrisBlack_${this.getHostName}_${this.getBaseName}_${this.getUserID}`);
            return this.initDatabaseIrisBlack();
        } catch (e) {
            this.manageIndexedDB(e);
        }
    }

    return this.manageIndexedDB(e);
}

/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * Permet de récupérer les données.
 * @param {any} url L'URL d'appel
 * @param {any} params Les paramètres d'appel
 * @param {any} clbck La fonction à rappeler après exécution en callback
 */
function linkToCall(url, params, clbck) {
    let helper = new eAxiosHelper(url);

    return helper.GetAsync({
        params: params,
        responseType: 'json'
    }, clbck);
}

/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * Permet d'enregistrer les données.
 * @param {any} url L'URL d'appel
 * @param {any} params Les paramètres d'appel
 * @param {any} clbck La fonction à rappeler après exécution en callback
 */
function linkToPost(url, params, clbck) {
    let helper = new eAxiosHelper(url);

    return helper.PostAsync(params, clbck);
}

/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * Permet d'enregistrer les données.
 * @param {any} url L'URL d'appel
 * @param {any} params Les paramètres d'appel
 * @param {any} config La configuration spécifique pour l'envoi avec entêtes
 * @param {any} clbck La fonction à rappeler après exécution en callback
 */
function linkToPostWHeader(url, params, config, clbck) {
    let helper = new eAxiosHelper(url);

    return helper.PostAsyncWHeader({ params: params, responseType: 'json' }, config, clbck);
}

/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * Permet d'enregistrer les données.
 * @param {any} url L'URL d'appel
 * @param {any} params Les paramètres d'appel
 * @param {any} clbck La fonction à rappeler après exécution en callback
 */
function linkToPut(url, params, clbck) {
    let helper = new eAxiosHelper(url);

    return helper.PutAsync(params, clbck);
}

/**
 * retourne une promesse des données et de la structure de la page
 * promesse qui ne sera tenue que bien plus tard (comme toujours...)
 * Permet de supprimer les données.
 * @param {any} url L'URL d'appel
 * @param {any} params Les paramètres d'appel
 * @param {any} clbck La fonction à rappeler après exécution en callback
 */
function linkToDelete(url, params, clbck) {
    let helper = new eAxiosHelper(url);

    /*
    Remarque importante : sur un appel à DELETE, il faut encapsuler les paramètres
    dans un noeud { data : objet } pour forcer Axios à envoyer un corps (body) de requête, ce qu'il ne fait pas par défaut, mais qui est requis par .NET.
    Source : https://stackoverflow.com/questions/62124061/axios-http-delete-request-returns-415-error?rq=1
    */
    return helper.DeleteAsync({ data: params }, clbck);
}

/**
 * Quand on clique sur la croix d'un signet épinglé.
 * En fait, on va récupérer les signets épinglés, et on va envoyer la liste au back.
 * @param {any} tab
 */
async function setClosePinnedBkm(tab) {
    // rm the pinned tab from tabs
    let index = this.tabs.findIndex(tb => tb.id == tab.id);

    // if the index = -1, the tab is not exist in tabs
    if (index == -1)
        return;

    let nTbPinned = this.tabs
        ?.splice(index, 1)
        ?.find(n => n)
        ?.DescId;

    if (nTbPinned) {

        let arTabsPinned = this.tabs?.flatMap(tb => tb.signets)?.filter(bkm => bkm && bkm.IsPinned)?.map(bkm => bkm.DescId) || [];

        this.tabs.forEach(tb => {
            if (tb.signets) {
                tb.signets.forEach(bkm => {
                    if (bkm.DescId == tab.DescId)
                        bkm.IsPinned = false;
                })
            }
        });

        nTbPinned = parseInt(nTbPinned);

        let oPinnedBookmark = this.loadPinnedBookmark();

        if (!(oPinnedBookmark && this.getTab && this.getFileId))
            return;

        try {
            await this.linkToPost(oPinnedBookmark.url, { nTab: parseInt(this.getTab), nFileId: parseInt(this.getFileId), nPinnedBookmarkDescId: parseInt(nTbPinned), aPinnedBookmarkDescIdList: arTabsPinned, oOperation: PinnedBookmarkControllerOperation.DELETE });
        } catch (e) {
            console.log(e);
        }
    }
}


/**
 * On switch du mode liste au mode fiche et vice et versa. Et vice et versaaaaa!
 * @param {boolean} vwMode
 * @param {any} nBkmTab
 * 
 */
async function setViewMode(vwMode, DescIdSignet) {

    let oPinnedBookmark = this.loadPinnedBookmark();

    if (!(this.getTab && this.getFileId))
        return;

    try {
        await this.linkToPost(oPinnedBookmark.url, { nTab: parseInt(this.getTab), nFileId: parseInt(this.getFileId), nPinnedBookmarkDescId: parseInt(DescIdSignet), oOperation: PinnedBookmarkControllerOperation.UPDATE, oViewMode: vwMode });
    } catch (e) {
        console.log(e);
    }

}

async function saveScrollPosition() {
    this.$emit('saveScrollPosition')
}

/** émet un appel pour afficher ou masquer le eProgressSpinner de fiche.js (setWait sauce IRIS)
* @param {boolean} bOn true si on doit l'afficher, false si on doit le masquer
* @param {any} nOpacity opacité à appliquer à la place de celle par défaut, de 0 à 1 par pas de 0.1
*/
async function setWaitIris(bOn, nOpacity) {
    this.$emit('setWaitIris', bOn, nOpacity);
}

/**
 * Quand on clique sur le status du nouveau mode fiche (activé, prévisualisé, désactivé).
 * @param {any} tab
 * @param {any} EudonetXIrisBlackStatus
 */
async function setStatusNewFile(tab, EudonetXIrisBlackStatus) {
    let oLoadStatusNewFile = loadStatusNewFile();
    try {
        await this.linkToPost(oLoadStatusNewFile.url, { Tab: tab, EudonetXIrisBlackStatus: EudonetXIrisBlackStatus });

        if (EudonetXIrisBlackStatus > 0)
            this.fnForceChckNwThm();

    } catch (e) {
        console.log(e);
    }
    
}


/**
 * Permet au clique de changer la couleur du bouton et d'activé, de prévisualisé ou désactivé le nouveau mode fiche
 * @param {any} button
 */
async function callbackFloatButton(button) {
    let oeParam = getParamWindow();
    let dvIrisBlackInput = oeParam.GetParam("dvIrisBlackInput");
    let dvIrisBlackInputPreview = oeParam.GetParam("dvIrisBlackInputPreview");
    let tabActive = dvIrisBlackInput?.split(";").map(x => parseInt(x)).includes(nGlobalActiveTab);
    let tabPreview = dvIrisBlackInputPreview?.split(";").map(x => parseInt(x)).includes(nGlobalActiveTab);
    let bNeoReloaded = false;

    function arrayRemove(arr, value) {
        return arr.filter(function (geeks) {
            return geeks != value;
        });
    }

    if (button.function == 'openDialogSetting') {
        // Popup de configuration des zones
        this.openDialogSetting = true;
    } else if (button.function == 'activeNewFileMode') {
        // Active le nouveau mode fiche
        this.floatingButtons.actions.forEach(function (button) {
            if (button.function != 'openDialogSetting') {
                button.colorBtn = '#3a454b'
            } else {
                button.disabled = false;
            }
        });
        button.colorBtn = 'primary'
        await this.setStatusNewFile(this.getTab, 1);

        // Retire dans eParam (prévisualisation)
        if (dvIrisBlackInputPreview) {
            var array = dvIrisBlackInputPreview.split(";").map(x => parseInt(x));
            var result = arrayRemove(array, nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInputPreview", result.join(';'));
        }

        // Ajout dans eParam (active)
        if (!dvIrisBlackInput) {
            var array = [];
            array.push(nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInput", array.join(';'));
        } else {
            var array = dvIrisBlackInput.split(";").map(x => parseInt(x));

            if (!array.includes(nGlobalActiveTab)) {
                array.push(nGlobalActiveTab);
                oeParam.SetParam("dvIrisBlackInput", array.join(';'));
            }
        }

        if (!tabPreview)
            bNeoReloaded = true;

    } else if (button.function == 'activeNewFileModePreview') {
        // Active le nouveau mode fiche en prévisualitation
        this.floatingButtons.actions.forEach(function (button) {
            if (button.function != 'openDialogSetting') {
                button.colorBtn = '#3a454b'
            } else {
                button.disabled = false;
            }
        });
        button.colorBtn = 'primary';
        await this.setStatusNewFile(this.getTab, 2);
        
        // Retire dans eParam (active)
        if (dvIrisBlackInput) {
            var array = dvIrisBlackInput.split(";").map(x => parseInt(x));
            var result = arrayRemove(array, nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInput", result.join(';'));
        }

        // Ajout dans eParam (prévisualisation)
        if (!dvIrisBlackInputPreview) {
            var array = [];
            array.push(nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInputPreview", array.join(';'));
        } else {
            var array = dvIrisBlackInputPreview.split(";").map(x => parseInt(x));
            if (!array.includes(nGlobalActiveTab)) {
                array.push(nGlobalActiveTab);
                oeParam.SetParam("dvIrisBlackInputPreview", array.join(';'));
            }
        }

        if (!tabActive)
            bNeoReloaded = true;

    } else if (button.function == 'desactiveNewFileMode') {
        // Désactive le nouveau mode fiche
        this.floatingButtons.actions.forEach(function (button) {
            if (button.function != 'openDialogSetting') {
                button.colorBtn = '#3a454b'
            } else {
                button.disabled = true;
            }
        });
        button.colorBtn = 'primary';

        await this.setStatusNewFile(this.getTab, 0);
        
        if (tabActive) {
            var array = dvIrisBlackInput.split(";").map(x => parseInt(x));
            var result = arrayRemove(array, nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInput", result.join(';'));
        } else if (tabPreview) {
            var array = dvIrisBlackInputPreview.split(";").map(x => parseInt(x));
            var result = arrayRemove(array, nGlobalActiveTab);
            oeParam.SetParam("dvIrisBlackInputPreview", result.join(';'));
        }

        if (tabActive || tabPreview)
            bNeoReloaded = true;
    }


    if (bNeoReloaded)
        top.loadFile(this.getTab, this.getFileId);
}

/** Vérifie l'utilisateur est autorisé. Donc superadmin. */
function getAuthorizedAdminUser() {
    let oUser = JSONTryParse(this.getUserInfos);

    if (oUser)
        return oUser.userLevel > UserLevel.LEV_USR_5

    return false;
}


function capitalizeString(string){
    return string?.charAt(0)?.toUpperCase() + string?.slice(1);
}

/** Computed qui retourne si on doit afficher ou non les boutons d'activation du nouveau mode fiche en fonction de l'userlevel */
function getUserAuth() {
    return JSONTryParse(this.getUserInfos).userLevel > UserLevel.LEV_USR_5;
}

export {
    LoadStructPage, observeRightMenu, LoadSignet, LoadStructBkm, LoadUserValues, LoadDataPage, LoadWzCatalog, LoadCatalog, CallCatalogBack,
    ConstructHeaderFile, ConstructPropertyFile, ConstructBookmarkFile, ConstructCatalog,
    setBkmObserver, specialCSS, forbiddenFormatHead, addNewFile, getFkLinksByTab,
    initDatabaseIrisBlack, initDatabase, manageDatabaseIrisBlack,

    linkToCall, linkToPost, linkToPut, linkToPostWHeader, linkToDelete, setClosePinnedBkm,
    setViewMode, saveScrollPosition, setStatusNewFile, callbackFloatButton, getAuthorizedAdminUser, getUserAuth, capitalizeString, setWaitIris
};