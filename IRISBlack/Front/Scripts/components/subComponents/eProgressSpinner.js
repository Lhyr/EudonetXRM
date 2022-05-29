import { eModalComponentsMixin } from '../../mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eProgressSpinner",
    data() {
        return {
            nOpacity: 0.9,
            sBackgroundColor: "background",
            arSize: [120, 80, 40],
            arRotate: [0, 120, 240],
            arWidth: [10, 5, 2],
            nZIndex: 5,
            sStyle: "transform: scaleX(-1)",
        }
    },
    computed: {
        /** Récupère la valeur la plus haute de Z-Index 
         * ou la valeur par défaut. */
        getMaxZIndex: function () {
            return Math.max(
                ...Array.from(document.querySelectorAll('body *'), el =>
                    parseFloat(window.getComputedStyle(el).zIndex),
                ).filter(zIndex => !Number.isNaN(zIndex)),
                this.nZIndex,
            ) + 1;
        },
        /** on récupère l'opacité de l'overlay */
        getOverlayOpacity: function () {
            return this.oTblItm?.nOpacity || this.nOpacity;
        },

        /** on récupère l'opacité de l'overlay */
        getOverlayColor: function () {
            return this.oTblItm?.sBackgroundColor || this.sBackgroundColor;
        },
    },
    props: {
        oTblItm: Object,
        display: {
            type: Boolean,
            default: true,
        }
    },
    mixins: [eModalComponentsMixin],
    template: `
     <v-overlay
         :color="getOverlayColor"
         :opacity="getOverlayOpacity"
         :z-index="getMaxZIndex"
         :value="display"
      >
      <v-progress-circular
        :size="arSize[0]"
        color="primary93"
        :width="arWidth[0]"
        :rotate="arRotate[0]"
        indeterminate
      >
      <v-progress-circular
        :size="arSize[1]"
        color="primary34"
        :width="arWidth[1]"
        :rotate="arRotate[1]"
        :style="sStyle"
        indeterminate
      >
          <v-progress-circular
            :size="arSize[2]"
            color="primary96"
            :width="arWidth[2]"
            :rotate="arRotate[2]"
            :style="sStyle"
            indeterminate
          >
          </v-progress-circular>
      </v-progress-circular>
      </v-progress-circular>
     </v-overlay>
`
}