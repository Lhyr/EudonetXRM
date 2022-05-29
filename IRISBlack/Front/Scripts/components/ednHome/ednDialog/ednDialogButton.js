import { eDialogMixin } from '../../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogButton",
    data() {
        return {
            sId: "",
            bLight: false,
            bIcon: false,
            bText: false,
            bRight: false,
            sColor: "",
            sClass: "",
            bTile: false,
        }
    },
    components: {},
    computed: {},
    mixins: [eDialogMixin],
    computed: {
    },
    methods: {
    },
    props: {
        oProps: Object,
    },
    created() {
        this.sId = this.oProps?.sId || this.sId;
        this.bLight = this.oProps?.bLight;
        this.bIcon = this.oProps?.bIcon;
        this.bText = this.oProps?.bText;
        this.bTile = this.oProps?.bTile;
        this.bRight = this.oProps?.bRight;
        this.sColor = this.oProps?.sColor || this.sColor;
        this.sClass = this.oProps?.sClass || this.sClass;
    },
    template: `<v-btn :id="sId" :icon="bIcon" :light="bLight" 
                :right="bRight" :text="bText" :tile="bTile" 
                :color="sColor"  :class="sClass"
                @click.prevent.stop="action($event)">
                <slot></slot>
               </v-btn>`,
};