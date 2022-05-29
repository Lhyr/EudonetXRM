import { eMruComponentsMixin } from '../mixins/eMruComponentsMixin.js?ver=803000';
import { GetMru } from '../methods/eMRUMethods.js?ver=803000'

export const eMruMixin = {
    mixins: [eMruComponentsMixin],

    data() {
        return {
            MruDataSource: null, //source sur laquelle se base l'alimentation du menu
            DataMru: null       //mru au sens strict du terme = valeurs récemment appelées par l'utilisateur
        };
    },
    computed:{
        getInput:{
            get: function(){
                return this.dataInput
            },
            set:function(input){
                let dataInput = {...this.dataInput,...input}
                this.$emit('update:dataInput',dataInput)
            }
        },
        getInputValue:{
            get: function(){
                return this.dataInput.Value
            },
            set:function(val){
                let finalDtVal = {...this.dataInput,...{Value:val}}
                this.$emit('update:dataInput',finalDtVal)
            }
        }
    },
    methods: {
        GetMru,
        /**
         * Charge les Mru
         * @param {any} descid : si non renseigné on prend celui de datainput, par contre pour les relations on prendra TargetTab (cf eRelations.js)
         */
        LoadMru: async function (descid) {
            this.DataMru = await this.GetMru(descid || this.dataInput.DescId);
            this.ResetDataMru();
            if (this.$refs.MRUSearch)
                this.$refs.MRUSearch.focus();
        },
        UpdateMruData: function (newValues) {
            this.MruDataSource = newValues;
        },
        ResetDataMru: function () {
            this.MruDataSource = this.DataMru;
        },
        openSpecificDialog: function () {
            this.$emit('openSpecificDialog');
        },
        addMethod: function () {
            this.$emit('addMethod');
        }
    },
    props:{
        focusSearch:{
            type:Boolean
        },
        dataInput:{
            type:Object
        },
        propDetail:{
            type:[Object,Array]
        }
    }
}