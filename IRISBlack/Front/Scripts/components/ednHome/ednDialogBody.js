import { eMotherClassMixin } from '../../mixins/eMotherClassMixin.js?ver=803000';

export default {
    name: "ednDialogBody",
    props: {
        sUrl: {
            type: String,
            default: "https://fr.eudonet.com/news-admin/News-Admin.html"
        }
    },
    components: {},
    computed: {},
    mixins: [eMotherClassMixin],
    methods: {

    },
    async mounted() {       
        await this.$nextTick();
        let currentActiveDialog = document.querySelector(".v-dialog__content--active");
        let currentWaiterContainer = top.document.getElementById("contentWait");
        if (currentWaiterContainer?.nodeType != Node.ELEMENT_NODE || currentActiveDialog?.nodeType != Node.ELEMENT_NODE)
            return;
        let currentWaiterContainerStyle = getComputedStyle(currentWaiterContainer);
        let currentActiveDialogStyle = getComputedStyle(currentActiveDialog);
        if (Number(currentWaiterContainerStyle?.zIndex) > Number(currentActiveDialogStyle?.zIndex))
            currentActiveDialog.style.zIndex = Number(currentWaiterContainerStyle?.zIndex) + 1;  
    },
    template: `
        <iframe :src="sUrl" class="edn--home--dialog--body"></iframe>
    `,

};