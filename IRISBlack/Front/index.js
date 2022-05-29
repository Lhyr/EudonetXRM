
import {
    store,
    FieldType,
    EdnType,
    eMailAdress,
    eCharacter,
    eAutoComplete,
    ePhone,
    eRelation,
    eSocialNetwork,
    eHyperLink,
    eFile,
    eCatalog,
    eGeolocation,
    eLogic,
    eDate,
    eButton,
    eMemo,
    eUser,
    eNumeric,
    eAutoCount,
    eMoney,
    eChart,
    eImage,
    ePassword,
    eWebPage,
    eLabel,
    eSeparator,
    ePJ
} from "./Scripts/LoadIrisScripts.js?ver=803000";
import vuetify from './Scripts/plugins/Vuetify.js?ver=803000';
import ErrorService from "./Scripts/error/ErrorService.js?ver=803000";

/**
 * Permet de ressortir le bon composant vue.js a partir
 * du format contenu dans le input.
 * @param {any} input
 */
export function dynamicFormatChamps(input) {

    if (!FieldType)
        return;

    let eFormat = input.AliasSourceField?.Format || input?.Format;


    switch (eFormat) {
        case FieldType.Undefined: return eCharacter;
        case FieldType.Character:
            if (input.IsMainField)
                return eRelation;
            if (input.AutoComplete)
                return eAutoComplete;
            return eCharacter;
        case FieldType.Catalog: return eCatalog;
        case FieldType.MailAddress: return eMailAdress;
        case FieldType.Phone: return ePhone;
        case FieldType.Relation: return eRelation;
        case FieldType.SocialNetwork: return eSocialNetwork;
        case FieldType.Geolocation: return eGeolocation;
        case FieldType.Button: return eButton;
        case FieldType.Alias:
            if (input.AliasSourceField?.TargetTab > 0)
                return eRelation;
            return eCharacter;
        case FieldType.AliasRelation: return eRelation;
        case FieldType.Logic: return eLogic;
        case FieldType.AutoCount: return eAutoCount;
        case FieldType.Date: return eDate;
        case FieldType.Label: return eLabel;
        case FieldType.Separator: return eSeparator;
        case FieldType.Memo: return eMemo;
        case FieldType.Numeric: return eNumeric;
        case FieldType.Money: return eMoney;
        case FieldType.Image: return eImage;
        case FieldType.Chart: return eChart;
        case FieldType.WebPage: return eWebPage;
        case FieldType.HyperLink: return eHyperLink;
        case FieldType.File: return eFile;
        case FieldType.User: return eUser;
        case FieldType.Password: return ePassword;
        case FieldType.PJ: return ePJ;
        case FieldType.Hidden: return eLabel;
        case FieldType.Binary: return eLabel;
        default: return eCharacter
    }
}


export function initVue() {
    const App = () => import(AddUrlTimeStampJS("./Scripts/components/app.js"));

    Vue.config.errorHandler = (err, vm, info) => ErrorService.initErrorServiceVue(err, vm, info).ToString();
    Vue.config.warnHandler = (msg, vm, info) => ErrorService.initErrorServiceVue(msg, vm, info).ToString();
    Vue.use(eudoFront.default);

    oVueInstance = new Vue({
        store,
        vuetify,
        render: h => h(App)
    })

    oVueInstance.$mount(`#app`);
}

/** Permet de destroy une vue existante */
export function destroyVue() {
    if (oVueInstance)
        oVueInstance.$destroy();
}