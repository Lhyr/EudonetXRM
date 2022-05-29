/**
 * Conteneur sous entrée de menu
 * */
const contentHeader = () => import("./shared/contentHeaderComponent.js");

const tabAcknowledgement = () => import("./parameter/generalParameters/tabAcknowledgementComponent.js");
const tabParamLang = () => import("./parameter/generalParameters/tabParamLang.js");
const tabAccessRights = () => import("./parameter/securityParameters/tabAccessRightsComponent.js");
const tabStartDate = () => import("./parameter/scheduling/tabStartDate.js");
const tabEndDate = () => import("./parameter/scheduling/tabEndDate.js");

const publicLink = () => import("./publication/shareComponents/paramPublicLink.js");
const publish = () => import("./publication/shareComponents/paramPublish.js");
const networkShare = () => import("./publication/shareComponents/networkShare.js");

const fullPageForm = () => import("./publication/integrationComponents/fullPageFormComponent.js");

const tabDefaultColors = () => import("./parameter/personnalisation/tabDefaultColors.js");
const tabFormColors = () => import("./parameter/personnalisation/tabFormColors.js");
const tabButtonColors = () => import("./parameter/personnalisation/tabButtonColors.js");
const tabFormFont = () => import("./parameter/personnalisation/tabFormFont.js");

export default {
    name: "subContent",
    components: { contentHeader },
    data() {
        return {};
    },

    methods: {
        dynamicTabContent(block) {
            //console.log(JSON.stringify(block))
            switch (block.refChildren) {
                case 'Acknowledgement':
                    return tabAcknowledgement;
                case 'ParamLang':
                    return tabParamLang;
                case 'AccessRights':
                    return tabAccessRights;
                case 'publish':
                    return publish;
                case 'publicLink':
                    return publicLink;
                case 'StartDate':
                    return tabStartDate;
                case 'EndDate':
                    return tabEndDate;
                case 'NetworkShare':
                    return networkShare;
                case 'FullPageForm':
                    return fullPageForm;
                case 'DefaultColors':
                    return tabDefaultColors;
                case 'FormColors':
                    return tabFormColors;
                case 'ButtonColors':
                    return tabButtonColors;
                case 'FormFont':
                    return tabFormFont;
            }
        },
    },
    props: ["dataTab", "activeSubEntry"],
    template: `
    <div class="moduleContent">
     
        <section :id="block.ref" v-for="block in dataTab.blocks"             
            v-bind:class="[block.refChildren === activeSubEntry ? '' : '', 'content-header']"
            >
            <component :data-tab=block :is="dynamicTabContent(block)" 
            :active="block.refChildren===activeSubEntry"
            ></component>
        </section>
    </div>
`,
};
