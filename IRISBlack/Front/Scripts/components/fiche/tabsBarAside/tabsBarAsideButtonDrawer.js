import { eFileMixin } from '../../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "tabsBarAsideButtonDrawer",
    data() {
        return {
            BackgroundColor: "",
            ForeColor: "white",
            CssClass: "",
            CssClassList: "",
        };
    },
    components: {
        eButton: () => import(AddUrlTimeStampJS("../../subComponents/eButton.js")),
        eIcon: () => import(AddUrlTimeStampJS("../../subComponents/eIcon.js")),
    },
    computed: {
        /** renvoie un objet qui sert de props avec tous les éléments importants
        * pour le NavigationDrawer. */
        getPropsNavigationDrawer: function () {
            return {
                BackgroundColor: this.oTblItm?.BackgroundColor || this.BackgroundColor,
                ForeColor: this.oTblItm?.ForeColor || this.ForeColor,
                CssClass: this.oTblItm?.CssClass || this.CssClass,
            }
        },
        /** renvoie la classe à mettre sur l'élément de la liste. */
        getCssClass: function () {
            return this.oTblItm?.CssClassList || this.CssClassList;
        }
    },
    methods: {
        /**
         * l'action a effectuer quand l'élément est sélectionné.
         * @param {any} event
         */
        action: function (event) {
            this.$emit("action", event)
        },
    },
    mixins: [eFileMixin],
    props: {
        oTblItm: Object,
    },
    template: `
    <li :class="getCssClass">
        <eButton :oTblItm="getPropsNavigationDrawer" @action="action">
            <eIcon>{{ oTblItm.IcoClose }}</eIcon> 
        </eButton>
    </li>
`
};