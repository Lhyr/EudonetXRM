import { eModalComponentsMixin } from '../../../Scripts/mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "ePopOver",
    mixins: [eModalComponentsMixin],
    data() {
        return {

        };
    },
    computed: {
        /** Récupère la classe CSS de la popover ( ombre portée + position) */
        getPopOverContentClass(){
            return this.position + ' elevation-'+ this.elevation;        
        }
    },
    mounted() {
    },
    methods: {
        /** Récupère la hauteur de la popover afin de calculer la position top */
        setPopOverHeight(){           
            document?.documentElement?.style?.setProperty('--popOverHeight', (this.$refs?.popOverContent?.offsetHeight / 2) + 'px');
        }
    },
    /* Objet attendu par la liste des liens */
    props: {
        propClassPopOver: [Array,Object],
        position:{
            type:String,
            default:'center'
        },
        elevation:{
            type:Number,
            default:1
        }
    },
    template: `
<div ref="popOver" class="popover__wrapper" :class="propClassPopOver">
    <a @mouseover="setPopOverHeight()" href="#">
        <slot name="popTitle"></slot>
    </a>
    <div ref="popOverContent" class="popover__content" :class="getPopOverContentClass" >
        <slot name="popContent"></slot>
    </div>
</div>
`
};