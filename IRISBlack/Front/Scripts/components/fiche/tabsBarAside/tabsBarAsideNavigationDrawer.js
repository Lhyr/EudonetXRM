import { eFileMixin } from '../../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "tabsBarAsideNavigationDrawer",
    data() {
        return {
            BackgroundColor: "",
            ForeColor: "white",
            CssClass: ""
        };
    },
    components: {
        eNavigationDrawer: () => import(AddUrlTimeStampJS("../../subComponents/eNavigationDrawer.js")),
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
        windowMaxWidth() {
            return window.matchMedia('(min-width: 1200px)').matches;
        }
    },
    methods: {
        /**
         * l'action a effectuer quand l'élément est sélectionné.
         * @param {any} event
         */
        action: function (event, index) {
            this.$emit("action", event, index);
        }
    },
    mixins: [eFileMixin],
    props: {
        oTblItm: Object,
        bActivityShow: {
            type: Boolean,
            default: false
        },
        bDisplayLabel: {
            type: Boolean,
            default: false
        },
        memoOpenedFullMode: Boolean,
        getStickyNav: String,
        btnDrawerInlineStyle:String
    },
    template: `
    <eNavigationDrawer :oTblItm="getPropsNavigationDrawer" @action="action">
        <template #activator>
            <div v-if="!bActivityShow" :style="btnDrawerInlineStyle" :class="oTblItm?.CssClassBtnCtner?.btnDrawerCtner" >
                <eButton v-for="itm in oTblItm?.itm" @action="action($event, itm.Value)" :oTblItm="itm" :key="itm.Label">
                    <eIcon>{{ itm?.Icon }}</eIcon>
                        <span :class="oTblItm?.CssClassBtnCtner?.drawerTitle">{{" " + itm?.Label}}</span>
                    </eButton>
                </div>
        </template>
        <div v-if="bActivityShow" :class="oTblItm?.CssClassBtnCtner?.drawerCtner" >
            <slot></slot>
        </div>
        <!--slot></slot-->
    </eNavigationDrawer>
`
};