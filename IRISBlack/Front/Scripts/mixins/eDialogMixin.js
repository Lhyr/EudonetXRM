import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import { setNewsMessageStopDisplay } from "../shared/XRMWrapperModules.js?ver=803000"

export const eDialogMixin = {   
    data() {
        return {
          
        };
    },
    methods: {
        setNewsMessageStopDisplay,

        /*** emit vers le eDialog (parent) ***/
        action: function (e) {
            this.$emit('action', e);
        }      
    },
    created() {
       
    },
mixins:[eMotherClassMixin],
    mounted: function () {
        
    },   
}