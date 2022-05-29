import { eMruSearchMixin } from '../../mixins/eMruSearchMixin.js?ver=803000';
import { sizeForm } from '../../methods/eComponentsMethods.js?ver=803000';

export default {
    name: "eMruSearch",  
    mixins: [eMruSearchMixin],
    data() {
        return {
        };
    },
    methods: {
        sizeForm
    },
    template: `
        <div class="searchMru">

            <input
            autofocus="true" 
            :placeholder="$attrs.placeholder" 
            :size="sizeForm(newSearch)" 
            spellcheck="false" 
            ref="SearchInput" 
            autocomplete="off" 
            type="text" 
            maxlength="100" 
            class="navSearch mru-search" 
            v-model="newSearch" 
            @keyup="searchFilter"/>

            <span 
            ref="searchBtn" 
            id="btnSrch"
            :class="isSearchActivated" 
            class="navSearchSpan mru-icon" 
            :title="getRes(1040)" 
            @click="removeDisplay($event)"></span>                     
        </div>               
`,
   
};