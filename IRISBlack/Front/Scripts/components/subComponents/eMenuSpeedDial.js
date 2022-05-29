import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "eMenuSpeedDial",
    mixins: [eFileComponentsMixin],
    data() {
        return {
            direction: 'top',
            transition: 'slide-y-reverse-transition',
            direction: "bottom",
            left: false,
            right:false,
            top: false,
            bottom: false,
            fab: false
        }
    },
    props: {
        oTblItem: {
            type: Object
        },
        hover: {
            type: Boolean,
            default: false
        }
    },
    computed: {
        /** Direction du v-speed-dial */
        getDirection: function () {
            return this.oTblItem?.direction || this.direction;
        },

        /** si on active sur le hover. */
        getHover: function () {
            return this.oTblItem?.hover || this.hover;
        },

        /** speeddial en haut */
        getTop: function () {
            return this.oTblItem?.top || this.top;
        },
        /** speeddial à droite */
        getRight: function () {
            return this.oTblItem?.right || this.right;
        },
        /** speeddial en bas */
        getBottom: function () {
            return this.oTblItem?.bottom || this.bottom;
        },
        /** speeddial à gauche */
        getLeft: function () {
            return this.oTblItem?.left || this.left;
        },
        /** la transition speeddial */
        getTransition: function () {
            return this.oTblItem?.transition || this.transition;
        }
    },
    watch: {
        fab: function () {
            this.$emit("@update:oTblItem", this.oTblItem);
        }
    },
    methods: {
    },
    template:
        `
    <v-speed-dial
        v-model="fab"
        :top="getTop"
        :bottom="getBottom"
        :right="getRight"
        :left="getLeft"
        :direction="getDirection"
        :open-on-hover="getHover"
        :transition="transition"
    >
        <template v-slot:activator>
            <slot name="button"></slot>
        </template>
        <slot name="menu"></slot>
    </v-speed-dial>



`
}