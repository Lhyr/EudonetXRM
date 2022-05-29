import { eModalMixin } from '../../mixins/eModalMixin.js?ver=803000';
import { tabAstuces, tabParentForbidden } from '../../methods/eComponentConst.js?ver=803000'

export default {
    name: "tooltipModale",
    data() {
        return {
            visible: false,
            msgHover: "",
            position: '',
            focused: false,
            values: "",
            info: false,
            top: 0,
            left: 0,
            removedValue: false,
            reversed: false,
            test: false,
            opened: false,
            titleAction: top._res_380,
            titleTip: top._res_2507,
            it: 0,
            tabParentForbidden: tabParentForbidden,
            tabAstuces: tabAstuces,
            mruCat: '',
            inputHeight: 35,
            iconWidth: 40,
            margin: 5,
            mouseMargin: 100,
            merginCell: {
                width: 50,
                height: 50
            },
            tooltipTriangle: {
                width: 20,
                height:10
            },
            scrolled: false,
            scrollableElem: null,
            animationDelay: '0ms',
            tooltipElm:undefined
        };
    },
    mixins: [eModalMixin],
    async created() {
        this.position = this.propOptionsModal.position;
        this.reversed = false;
        let regCat = /\w*mru\w*/i;
        this.mruCat = regCat.test(this.propOptionsModal.elem);

    },
    beforeDestroy() {
        this.scrollableElem.removeEventListener('scroll', this.removeTooltip);
    },
    methods: {
        /**
         * ELAIZ - Autrefois l'infobulle était cachée au scroll via un eventlistener directement sur la balise #mainContentWrap même lorsque
          nous n'avions aucune infobulle. Cet eventlistener est retiré un fois que l'utilisateur scrolle et au beforeDestroy du composant tooltip.
         * */
        removeTooltip() {
            if (!(this.scrolled || this.propOptionsModal.elem == "bkmScroll")) {
                this.scrolled = true;
                this.$emit('closeTooltipModale');
                this.scrollableElem.removeEventListener('scroll', this.removeTooltip);
            }
        }
    },
    computed: {
        /**
         * retourne si on n'autorise pas l'affichage de certains éléments dans le composant parent.
         * @returns {any} un booléen qui dit que c'est un des parents interdits.
         */
        forbiddenToolTip() {
            return !this.tabParentForbidden
                .includes(this.propOptionsModal.parentElement)
                && this.propOptionsModal.msgTooltip
                && this.propOptionsModal.msgTooltip != "";
        },
        /**
         * On trouve ici des astuces suivant le format du composant qui appelle.
         * */
        fnTipsAndTricks() {
            return !this.propOptionsModal.readonly
                ? this.tabAstuces[this.propOptionsModal.currentElement]
                : "";
        },
        /**
         * Permet de savoir s'il y a des données à afficher.
         * */
        hasDataToShow() {
            return (this.propOptionsModal
                && this.propOptionsModal.newData
                && this.propOptionsModal.newData.length > 0)
                || this.info || this.propOptionsModal.emailVerif?.description
        },
        /** l'infobulle apparaît à droite de l'élément */
        rightSide() {
            return this.mruCat;
        },
        /** l'infobulle apparaît à gauche de l'élément */
        leftSide() {
            return this.position?.left + this.tooltipElm?.offsetWidth > this.$root?.$el?.offsetWidth;
        },
        tooltipClass() {
            return (this.mruCat) ? 'mru-content' : 'tooltip-content';
        },
        isFixed() {
            return (this.propOptionsModal.elem != "bkmScroll") ? 'fixed' : 'absolute';
        },
        /** Pour le RGPD on affiche des trucs... */
        getRGPDToShow: function () {
            return this.propOptionsModal.rgpd && Object.keys(this.propOptionsModal.rgpd).length > 0 ? true : false
        },
        /** retourne le label de categorie s'il y en a 1 */
        getCategoryLabel: function () {
            return this.propOptionsModal.rgpd.CategoryLabel
                && this.propOptionsModal.rgpd.CategoryLabel != ""
                && " " + this.getRes(2780) + " <b>" + this.propOptionsModal.rgpd.CategoryLabel.toLowerCase() + "</b>"
        },
        /** retourne l'elément à afficher dans tooltip pour la vérification email */
        emailVerifTooltip() {
            if (this.propOptionsModal?.emailVerif?.name == "VALID" || this.propOptionsModal?.emailVerif?.name == "UNCHECKED" || this.propOptionsModal?.emailVerif?.name == "VERIFICATION_IN_PROGRESS")
                return this.propOptionsModal?.emailVerif?.description;
            else
                return this.propOptionsModal?.emailVerif?.statuseudoTechSub?.name;
        },
        emailVerifTooltipTitle() {
            if (this.propOptionsModal?.emailVerif?.name != "UNCHECKED" &&
                this.propOptionsModal?.emailVerif?.name != "VALID" &&
                this.propOptionsModal?.emailVerif?.name != "VERIFICATION_IN_PROGRESS" &&
                this.propOptionsModal?.emailVerif)
                return this.propOptionsModal?.emailVerif?.description;
        },
        /** Classes CSS du chevron de l'infobulle */
        getChevronCssClass(){
            return {
                'reversed': this.reversed,
                'on-right-side': this.rightSide,
                'on-left-side': this.leftSide
            }
        },
        /** Classes CSS du container de l'infobulle */
        getWrapperCssClass(){
            return {
                'top':this.top ,
                'left':this.left,
                'position':this.isFixed,
                'animation-delay':this.propOptionsModal?.animationDelay,
                'animation-duration':this.propOptionsModal?.animationDuration
            }
        }
    },
    directives: {
        loaded: {
            // ELAIZ - On passe par une directive plutôt que par un computed car à ce moment là nous avons accès aux refs contrairement au computed au 1er appel
            inserted: function (el, binding, vnode) {
                let that = vnode.context;
                that.tooltipElm = that.$el;
                if (that.mruCat) {
                    that.left = that.propOptionsModal.mousePosition.mouseX + that.mouseMargin + 'px';
                    that.top = that.position.top - (that.propOptionsModal.position.height / 2) + 'px';
                } else if (that.propOptionsModal.elem == "bkmScroll") {
                    that.left = (that.propOptionsModal.position.width / 2) - (that.$el.offsetWidth / 2) + 'px';
                    that.top = -Math.abs(that.$el.offsetHeight) + 'px';
                    that.reversed = true;
                } else if (that.propOptionsModal.elem == "merginButton") {
                    that.left = (that.position.left - that.merginCell.width) + 'px';
                    that.top = that.position.top + that.merginCell.height + 'px';
                } else if (that.propOptionsModal.elem == "shortcut") {
                    that.left = that.position.left - (that.$el.offsetWidth / 2)  + 'px';
                    that.top = that.position.top + (that.inputHeight + that.margin) + 'px';
                } else if (that.propOptionsModal.emailVerif) {
                    that.left = that.position.left - (that.$el.offsetWidth / 2) + 'px';
                    that.top = that.position.top + (that.inputHeight + that.margin) + 'px';
                } else if (!that.propOptionsModal.info) {
                    that.left = that.position.left + 'px';
                    that.top = that.position.top + (that.inputHeight + that.margin) + 'px';
                } else if (that.leftSide) {
                    that.left = (that.position.left - that.$el.offsetWidth) + 'px';
                    that.top = that.position.top - that.inputHeight + 'px';
                }else {
                    that.left = that.position.left + that.iconWidth + 'px';
                    that.top = that.position.top - that.inputHeight + 'px';
                }

                if (that.propOptionsModal.visible && that.position.top + (that.$refs.tooltips.offsetHeight + that.inputHeight) > window.innerHeight) {
                    that.top = (that.position.top - (that.$refs.tooltips.offsetHeight + that.inputHeight)) + 'px';
                    that.reversed = true;
                }

                if (that.propOptionsModal.elem != "bkmScroll")
                    document.querySelector('#mainContentWrap').addEventListener('scroll', that.removeTooltip);
            }
        }
    },
    mounted() {
        this.scrollableElem = this.$root.$children.find(child => child).$children.find(child => child).$el;
    },
    components: {},
    props: { propOptionsModal:Object},
    template: `
    <span v-loaded ref="tooltips" 
        v-if="(propOptionsModal.visible && !focused) || opened" 
        :class="{'info':propOptionsModal.info}" class="tooltip-container justify-center" 
        :style="getWrapperCssClass">
        <span v-if="emailVerifTooltipTitle" class="tooltip-title  main-title underlined-text">
            {{emailVerifTooltipTitle}}
        </span>
        <span v-if="(hasDataToShow || emailVerifTooltip) && propOptionsModal.currentElement != 'logic'" :class="tooltipClass" >
            {{propOptionsModal.newData}}       
            {{emailVerifTooltip}}           
        </span>
        <span v-if="propOptionsModal.msgError" class="error">
            {{propOptionsModal.msgError}}
        </span>
        <span one v-if="forbiddenToolTip" :class="{'tooltip-content' : hasDataToShow, 'tooltip-content-pre' : hasDataToShow}" >{{propOptionsModal.msgTooltip}}</span>
        <span two v-if="getRGPDToShow" :class="{'tooltip-content' : hasDataToShow || forbiddenToolTip}">
            {{getRes(2779)}} <b>{{propOptionsModal.rgpd.NatureLabel.toLowerCase()}}</b><span v-html="getCategoryLabel"></span>
        </span>
        <span three v-if="fnTipsAndTricks" class="tooltip-content">
            <span class="tooltip-title font-weight-bold">{{titleTip}}</span>
            <span>{{fnTipsAndTricks}}</span>
        </span>
        <span :class="getChevronCssClass" class="chevron-tooltip"></span>
    </span>
`
};