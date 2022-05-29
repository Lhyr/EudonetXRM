//Formular Store
import formularStore from "../../../store/formularstore.js"

var getters = {
    nFileId: state => state.nFileId,

    getRes: function (state) {
        return function (id) {

            if (typeof id === "number") {

                if (top && top.hasOwnProperty("_res_" + id))
                    return top["_res_" + id];
                else
                    return "##[INVALID_RES_" + id + "]##";
            }
            else if (typeof id === "string") {
                return "(TODO RES) " + id
            }

            return "";
        }
    },
    getUrl: function () {
        let domaine = window.location.href.split("/");
        domaine.pop();
        return domaine.join("/");
    },
    /** Récupère l'id de la langue de l'utilisateur. */
    getUserLangID: function () {
        return (top && top.hasOwnProperty("_userLangId")) ? top._userLangId : 0
    },
    getPermMode: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return state.ViewPermMode;
                    break;
                case 'UPDATE':
                    return state.UpdatePermMode;
                    break;
                default: break;
            }
        }
    },
    getPermLevel: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return state.ViewPermLevel;
                    break;
                case 'UPDATE':
                    return state.UpdatePermLevel;
                    break;
                default: break;
            }
        }
    },
    getPermUserDisplayValue: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return state.ViewPermUserDisplay;
                    break;
                case 'UPDATE':
                    return state.UpdatePermUserDisplay;
                    break;
                default: break;
            }
        }
    },
    getPermUserValue: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return state.ViewPermUser;
                    break;
                case 'UPDATE':
                    return state.UpdatePermUser;
                    break;
                default: break;
            }
        }
    },
    getPermShow: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return state.ShowViewPerm;
                    break;
                case 'UPDATE':
                    return state.ShowUpdatePerm;
                    break;
                default: break;
            }
        }
    },
    //pour adapter le composant eUser au formulaire, il faut lui associer un descId
    //EventBus
    getPermDescId: function (state) {
        return function (permType) {
            switch (permType) {
                case 'VIEW':
                    return -10000001;
                    break;
                case 'UPDATE':
                    return -10000002;
                    break;
                default: break;
            }
        }
    },

}

var mutations = {}

var domaine = window.location.href.split("/");
domaine.pop();
var urlStruc = domaine.join("/");

Vue.use(Vuex);

const getDefaultState = () => {
    return {
        nFormularId: 0,
        nFileId: null,
        url: urlStruc,
        tab: 0,
        wizardActivTab: "CreatGraphique",
        formularName: "",
        body: "",
        bodycss: "",
        LangId: 0,
        submissionBody: "",
        submissionBodyCss: "",
        submissionRedirectUrl: "",
        FormularLink: "",
        FormularIntegrationScript: "",
        AllAvailableLng: [
            { "0": "Français" },

        ],
        UserInfos: {
            LangId: 0,
            Login: "",

        },
        Published: false,
        PublicFormular: false,
        AllAvailableLevels: {},
        //Droits de visualisation sur le formulaire avancé
        ViewPermId: 0,
        ViewPermMode: -1,
        ViewPermLevel: "",
        ViewPermUser: "",
        ViewPermUserDisplay: "",
        ShowViewPerm: false,
        //Droits de modifs sur le formulaire avancé
        UpdatePermId: 0,
        UpdatePermMode: -1,
        UpdatePermLevel: "",
        UpdatePermUser: "",
        UpdatePermUserDisplay: "",
        ShowUpdatePerm: false,

        ExpireDate: new Date(),
        StartDate: new Date(),
        MsgDateStart: "",
        MsgDateEnd: "",
        MetaTitle: "",
        MetaDescription: "",
        FullPageCode: "",
        AccentuationColor: "",
        ButtonBackgroundColor: "",
        LinkColor: "",
        PoliceColor: "",
        ButtonPoliceColor: "",
        FontSize: "",
        FontName: "",
        AllAvailableFonts: [
            { value: '"Andale Mono", AndaleMono, monospace', name: 'Andale Mono' },
            { value: 'Arial, Helvetica, sans-serif', name: 'Arial' },
            { value: '"Arial Black", Gadget, sans-serif', name: 'Arial Black' },
            { value: '"Brush Script MT"', name: 'Brush Script MT' },
            { value: 'Cabin', name: 'Cabin' },
            { value: '"Comic Sans MS", cursive, sans-serif', name: 'Comic Sans MS' },
            { value: '"Concert One"', name: 'Concert One' },
            { value: 'Courier New, Courier, monospace', name: 'Courier New' },
            { value: 'Georgia, serif', name: 'Georgia' },
            { value: 'Helvetica, serif', name: 'Helvetica' },
            { value: 'Impact, Charcoal, sans-serif', name: 'Impact' },
            { value: 'Lato', name: 'Lato' },
            { value: 'Lora', name: 'Lora' },
            { value: '"Lucida Sans Unicode"', name: 'Lucida Sans Unicode' },
            { value: 'Merriweather', name: 'Merriweather' },
            { value: '"Merriweather Sans"', name: 'Merriweather Sans' },
            { value: 'Montserrat', name: 'Montserrat' },
            { value: '"Nunito Sans"', name: 'Nunito Sans' },
            { value: '"Open Sans", sans-serif', name: 'Open Sans' },
            { value: '"Open Sans Condensed"', name: 'Open Sans Condensed' },
            { value: 'Oswald', name: 'Oswald' },
            { value: '"Playfair Display"', name: 'Playfair Display' },
            { value: '"Prompt"', name: 'Prompt' },
            { value: '"PT Sans"', name: 'PT Sans' },
            { value: 'Raleway', name: 'Raleway' },
            { value: 'Roboto', name: 'Roboto' },
            { value: '"Roboto Condensed"', name: 'Roboto Condensed' },
            { value: '"Source Sans Pro"', name: 'Source Sans Pro' },
            { value: '"Space Mono"', name: 'Space Mono' },
            { value: 'Tahoma, Geneva, sans-serif', name: 'Tahoma' },
            { value: '"Times New Roman", Times, serif', name: 'Times New Roman' },
            { value: '"Trebuchet MS"', name: 'Trebuchet MS' },
            { value: 'Verdana, Geneva, sans-serif', name: 'Verdana' },
            { value: '"Work Sans", sans-serif', name: 'Work Sans' }
        ],
        MetaImgURL: "",
        FileImage: {},
        ImageHasChanged: false,
        DialogHelp: false,
        worldlinePaimentBlocs: null
    }
}

const defaultState = getDefaultState();

export const store = new Vuex.Store({
    state: defaultState,

    modules: {
        formularStore
    },

    getters: getters,

    mutations: {

        setWorldLinePaimentBlocs(state, blocs) {
            state.worldlinePaimentBlocs = blocs;
        },
        setFormularLink(state, link) {
            state.FormularLink = link;
        },

        setFormularIntegrationScript(state, link) {
            state.FormularIntegrationScript = link;
        },

        setWizardActivTab(state, wizTab) {
            state.wizardActivTab = wizTab;
        },

        setFormularName(state, name) {
            state.formularName = name;
        },

        setLangdid(state, langid) {
            state.LangId = langid
        },

        setUserInfos(state, user) {
            state.UserInfos = { ...state.UserInfos, user }
        },

        setPublished(state, published) {
            state.Published = published
        },

        resetState(state) {
            Object.assign(state, getDefaultState())
        },

        setAcknowledgmentSelect(state, acknowledgmentSelect) {
            state.AcknowledgmentSelect = acknowledgmentSelect
        },

        setSubmissionRedirectUrl(state, submissionRedirectUrl) {
            state.submissionRedirectUrl = submissionRedirectUrl;
        },

        setSubmissionBody(state, submissionBody) {
            state.submissionBody = submissionBody;
        },

        setSubmissionBodyCss(state, submissionBodyCss) {
            state.submissionBodyCss = submissionBodyCss;
        },

        setMergeFieldsWithoutExtended(state, mergeFieldsWithoutExtended) {
            state.mergeFieldsWithoutExtended = mergeFieldsWithoutExtended;
        },

        setHyperLinksMergeFields(state, hyperLinksMergeFields) {
            state.hyperLinksMergeFields = hyperLinksMergeFields;
        },

        setIsValidRedirectUrl(state, isValidRedirectUrl) {
            state.isValidRedirectUrl = isValidRedirectUrl;
        },
        setEvtFileId(state, evtFileId) {
            state.EvtFileId = evtFileId;
        },

        setPublicFormular(state, publicFormular) {
            state.PublicFormular = publicFormular;
        },

        setViewPermId(state, viewPermId) {
            state.ViewPermId = viewPermId;
        },

        setViewPermMode(state, viewPermMode) {
            state.ViewPermMode = viewPermMode;
        },

        setViewPermLevel(state, viewPermLevel) {
            state.ViewPermLevel = viewPermLevel;
        },

        setViewPermUser(state, viewPermUser) {
            state.ViewPermUser = viewPermUser;
        },

        setShowViewPerm(state, showView) {
            state.ShowViewPerm = showView;
        },

        setUpdatePermId(state, updatePermId) {
            state.UpdatePermId = updatePermId;
        },

        setUpdatePermMode(state, updatePermMode) {
            state.UpdatePermMode = updatePermMode;
        },

        setUpdatePermLevel(state, updatePermLevel) {
            state.UpdatePermLevel = updatePermLevel;
        },

        setUpdatePermUser(state, updatePermUser) {
            state.UpdatePermUser = updatePermUser;
        },

        setViewPermUserDisplay(state, viewPermUserDisplay) {
            state.ViewPermUserDisplay = viewPermUserDisplay;
        },
        setUpdatePermUserDisplay(state, updatePermUserDisplay) {
            state.UpdatePermUserDisplay = updatePermUserDisplay;
        },

        setShowUpdatePerm(state, showUpdate) {
            state.ShowUpdatePerm = showUpdate;
        },

        setExpireDate(state, expireDate) {
            state.ExpireDate = expireDate;
        },

        setStartDate(state, startDate) {
            state.StartDate = startDate;
        },

        setMsgDateStart(state, msgDateStart) {
            state.MsgDateStart = msgDateStart;
        },

        setMsgDateEnd(state, msgDateEnd) {
            state.MsgDateEnd = msgDateEnd;
        },
        setMetaTitle(state, metaTitle) {
            state.MetaTitle = metaTitle;
        },
        setMetaDescription(state, metaDescription) {
            state.MetaDescription = metaDescription;
        },
        setFullPageCode(state, code) {
            state.FullPageCode = code;
        },
        setAccentuationColor(state, accentuationColor) {
            state.AccentuationColor = accentuationColor;
        },
        setButtonBackgroundColor(state, buttonBackgroundColor) {
            state.ButtonBackgroundColor = buttonBackgroundColor;
        },
        setButtonPoliceColor(state, buttonPoliceColor) {
            state.ButtonPoliceColor = buttonPoliceColor;
        },
        setLinkColor(state, linkColor) {
            state.LinkColor = linkColor;
        },
        setPoliceColor(state, policeColor) {
            state.PoliceColor = policeColor;
        },
        setFontSize(state, fontSize) {
            state.FontSize = fontSize;
        },
        setFontName(state, fontName) {
            state.FontName = fontName;
        },
        setbodycss(state, cssnewFormularModel) {
            state.bodyCss = cssnewFormularModel;
        },
        setformularbody(state, newFormularBody) {
            state.body = newFormularBody;
        },
        setMetaImgURL(state, fileName) {
            state.MetaImgURL = fileName;
        },
        setFileImage(state, fileimage) {
            state.FileImage = fileimage;
        },
        setImageHasChanged(state, hasChanged) {
            state.ImageHasChanged = hasChanged;
        },
        setDialogHelp(state, dialogHelp) {
            state.DialogHelp = dialogHelp;
        }

    }
});

export function initVue() {

    Vue.use(eudoFront.default);
    Vue.use(Vuetify);
    Vue.use(VueSocialSharing.default);


    const opts = {
        icons: {
            iconfont: "mdi",
        },
        theme: {
            options: {
                customProperties: true,
            },
            themes: {
                light: {
                    primary: "#bb1515",
                    secondary: "#757575",
                    accent: "#82B1FF",
                    error: "#FF5252",
                    info: "#2196F3",
                    success: "#4CAF50",
                    warning: "#FFC107",
                },
            },
        },
    };
    const vuetify = new Vuetify(opts);

    const App_assist_form = () => import("../advFormMainApp.js");
    return new Vue({
        render: h => h(App_assist_form),
        store,
        vuetify,
        destroyed() {
            //on reset le store
            store.commit('resetState')
        }
    }).$mount('#app_assist_form');
}