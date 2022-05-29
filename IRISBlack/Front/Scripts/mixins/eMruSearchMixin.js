import { eMruComponentsMixin } from '../mixins/eMruComponentsMixin.js?ver=803000';
import { GetMruSearch } from '../methods/eMRUMethods.js?ver=803000'

export const eMruSearchMixin = {
    mixins: [eMruComponentsMixin],
    data() {
        return {
            newSearch: this.dataInput.DisplayValue,
            activeSearch: false,
            timer: null
        };
    },
    computed: {
        isSearchActivated: function () {
            return (this.activeSearch) ? 'fas fa-times' : 'fas fa-search';
        }
    },
    watch:{
        focusSearch: function(val){
            if(val){
                this.$refs.SearchInput.select();
            }
        }
    },
    methods: {
        GetMruSearch,
        UpdateDataSource: async function () {
            let mru = await this.GetMruSearch(this.dataInput, this.newSearch);
            this.$emit('UpdateMruData', mru);
        },
        SetSearchTimer: function () {
            clearTimeout(this.timer);
            this.timer = setTimeout(this.UpdateDataSource, 300);
        },
        removeDisplay: function (event) {
            if (event.target == this.$refs.searchBtn && this.activeSearch) {
                this.newSearch = '';
                this.$emit('ResetDataMru');
                this.activeSearch = false;
            }
        },
        searchFilter: function () {
            if (this.newSearch.length > 0) {
                this.SetSearchTimer();
                this.activeSearch = true;
            } else {
                this.$emit('ResetDataMru');
                this.activeSearch = false;
            }
        },
        newLabel: function (newVal) {
            displayLabel = newVal;
        },
        focus: function () {
            this.$refs.SearchInput.focus();
            this.newSearch = this.dataInput.DisplayValue;
        }
    },
    props:{
        value:{
            type:String
        },
        dataInput:{
            type:Object
        },
        focusSearch:{
            type:Boolean
        }
    },
}