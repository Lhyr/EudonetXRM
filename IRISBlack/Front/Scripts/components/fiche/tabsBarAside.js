import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import { FieldType } from '../../methods/Enum.min.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { observeRightMenu } from '../../methods/eFileMethods.js?ver=803000';

export default {
    name: "tabAside",
    data() {
        return {
            panel:0,
            panelHeight: 48,
            minHeightPanelDeployed: 250,
            BackgroundColor: "",
            ForeColor: "",
            Width: "",
            CssClassDrawer: "DrawerAsideShutter",
            //CssClass: "DrawerAside",
            CssClassBtn: ["btnDrawerAside d-flex align-center"],
            CssClassBtnCtner: {
                btnDrawerCtner: 'btnDrawerContainer d-flex flex-column',
                drawerCtner: 'flex-auto drawerContainer',
                drawerTitle: 'btnDrawerAsideTitle',
            },
            CssClassList: ["btnDrawerAsideClose d-flex flex-auto align-center justify-end"],
            IcoClose: "fas fa-indent",
            FieldType: FieldType,
            bDisplayLabel: false,
            rightMenuWidth: null,
            idxBookmark: 0,           
        };
    },
    props: {
        propTabAside: [Object, Array],
        bActivityComponent: Boolean,
        iBookmark: Number,
        memoOpenedFullModeObj: Object,
        navTabsSticky: Boolean,
        btnDrawerInlineStyle: {
            type: String,
            default: ''
        },
        tabletDisplay: {
            type: Boolean,
            default: false
        }
    },
    mixins: [eFileMixin],
    components: {
        tabsBarAsideNavigationDrawer: () => import(AddUrlTimeStampJS("./tabsBarAside/tabsBarAsideNavigationDrawer.js")),
        tabsBarAsideButtonDrawer: () => import(AddUrlTimeStampJS("./tabsBarAside/tabsBarAsideButtonDrawer.js")),
    },
    computed: {
        /** Renvoie la hauteur d'un panel expend */
        getStylePanel: function () {
            return { height: 'calc(100vh - ' + (this.minHeightPanelDeployed + (this.panelHeight * this.propTabAside.length)) + 'px)' }
        },
        /** renvoie un objet qui sert de props avec tous les éléments importants
         * pour le NavigationDrawer. */
        getPropsNavigationDrawer: function () {
            return {
                CssClassDrawer: this.CssClassDrawer,
                CssClass: !this.bActivityShow ? this.CssClass : "",
                BackgroundColor: this.BackgroundColor,
                ForeColor: this.ForeColor,
                CssClassBtnCtner: this.CssClassBtnCtner,
                itm: this.propTabAside?.map((ppt, index) => {
                    return {
                        Label: ppt.Label,
                        BackgroundColor: ppt.BackgroundColor,
                        DescId: ppt.DescId,
                        FileId: ppt.FileId,
                        LabelHidden: ppt.LabelHidden,
                        ToolTipText: ppt.ToolTipText,
                        Icon: ppt.IconIn,
                        Value: index,
                        CssClass: this.CssClassBtn
                    }
                })
            }
        },
        /** envoie une props au bouton de fermeture. */
        getPropsButtonDrawer: function () {
            return {
                CssClass: this.CssClassBtn,
                BackgroundColor: this.BackgroundColor,
                ForeColor: this.ForeColor,
                IcoClose: this.IcoClose,
                CssClassList: this.CssClassList,
                CssClassBtnCtner: this.CssClassBtnCtner
            }
        },
        /** Sert à activer ou masquer la zone note au dessus. */
        ActivityComponent: {
            get: function () {
                return this.bActivityComponent || this.tabletDisplay;
            },
            set: function (value) {
                this.$emit("update:bActivityComponent", value, this.idxBookmark);
            }
        },
        getStickyNav() {
            return this.navTabsSticky ? 'stickyNavSignet':''
        },
        /** Renvoie l'état de l'accordéon, quel panel ou rien... et le met à jour si besoin */
        Panel:{
            get(){
                return this.panel;
            },
            set(value){
                this.$emit("update:bActivityComponent", this.bActivityComponent, value);
                this.panel = value;
            }
        }
    },
    methods: {
        dynamicFormatChamps,
        setBtnDisplayLabel: function(bLabel) {
            this.bDisplayLabel = bLabel;
            this.$emit('activityClosed',bLabel);
        },
        /**
         * Ce qui se passe quand on clique dans un bouton du navigation drawer.
         * @param {any} event
         */
        getNavigationDrawerButtonClick(event, index) {
            this.panel = index
            this.idxBookmark = index;
            this.ActivityComponent = !this.ActivityComponent;
        },
        /**
         * On ferme le panneau et on afficeh la tirette de droite.
         * @param {any} event
         */
        closeDrawer: function (event) {
            if (event?.type == "click")
                this.ActivityComponent = false
        },
        /**
         * Ce qui se passe lorsqu'on effectue une action dans le navigation drawer.
         * On est d'accord, un switch, sur un événement, c'est un peu du gachis.
         * Mais il peut faire tellement, tellement plus...
         * @param {any} event
         */
        getNavigationDrawerAction(event, index) {
            switch (event?.type) {
                case "click": this.getNavigationDrawerButtonClick(event, index); break;
                case "mouseover": this.setBtnDisplayLabel(true) ; break;
                case "mouseleave": this.setBtnDisplayLabel(false) ; break;
            }

            return false;
        },
        /**
         * fait remonter l'information en cas de changement d'onglet.
         * @param {any} idx
         */
        setIndexBookmark(idx) {
            this.idxBookmark = idx;
            this.$emit("update:bActivityComponent", this.ActivityComponent, idx);
        }
    },
    created() {
        //this.BackgroundColor = this.getBackgroundCSSColor();
        this.ForeColor = this.getForeCSSColor();
        this.idxBookmark = this.iBookmark;
        this.Panel = this.iBookmark;
    },
    template: `

<tabsBarAsideNavigationDrawer 
    :tabletDisplay="tabletDisplay"
    :oTblItm="getPropsNavigationDrawer"
    :bActivityShow="ActivityComponent"
    :bDisplayLabel="bDisplayLabel"
    :btnDrawerInlineStyle="btnDrawerInlineStyle"
    :getStickyNav="getStickyNav"
    @action="getNavigationDrawerAction"
>

    <div class="nav-tabs-custom">

        <div id="nav-tabs-ul" class="nav-expend px-3 nav nav-tabs d-flex">
            {{getRes(5081)}} & {{getRes(104)}} <tabsBarAsideButtonDrawer :oTblItm="getPropsButtonDrawer" @action="closeDrawer" />
        </div>

        <v-expansion-panels v-model="Panel" accordion ref="navTabsUl" class="nav nav-tabs d-flex">
            <v-expansion-panel v-for="(tab,index) in propTabAside" :key="tab.DescId" :class="{'active': index===idxBookmark,'btnDrawer':true}">
                
                <v-expansion-panel-header class="py-0">
                    <template v-if="tab.Format == 17">
                        <v-icon @click="$emit('iconClick',index)">
                            fas fa-expand
                        </v-icon>
                    </template>
                    {{tab.Label}}
                </v-expansion-panel-header>
                <v-expansion-panel-content v-bind:style="getStylePanel" :id="tab.DescId + '_id'" v-bind:class="{'active': index===idxBookmark}" :divdescid="tab.DescId" class="tab-pane">
                    <component v-if="tab.TableType != FieldType.Logic"
                        :cptFromAside="true"
                        :memoOpenedFullMode="index === memoOpenedFullModeObj.index ? memoOpenedFullModeObj.value : null"
                        :prop-aside="true"
                        :data-input="tab"
                        :is="dynamicFormatChamps(tab)"
                    />
                    <div v-else>
                        {{tab.Label}}
                    </div>
                </v-expansion-panel-content>
            </v-expansion-panel>
        </v-expansion-panels>
    </div>
</tabsBarAsideNavigationDrawer>`,

};