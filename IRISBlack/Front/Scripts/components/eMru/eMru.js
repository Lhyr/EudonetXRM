import { eMruMixin } from '../../mixins/eMruMixin.js?ver=803000';

/*Solution plus pérenne pour import dynamique timestamp
//import { eMruMixin } from '../../mixins/eMruMixin.js';
var eMruMixin;
await import(`../../mixins/eMruMixin.js?v=${top._jsVer}`).then(module => {
    eMruMixin = module.eMruMixin;
});
*/

export default {
    name: "eMru",
    mixins: [eMruMixin],
    data() {
        return {
        };
    },
    components: {
        eMruSearch: () => import(AddUrlTimeStampJS("./eMruSearch.js")),
        eMruFooter: () => import(AddUrlTimeStampJS("./eMruFooter.js")),
        eMruBody: () => import(AddUrlTimeStampJS("./eMruBody.js"))
    },
    template: `
                    <div>
                        <eMruSearch  
                            ref="MRUSearch"
                            :prop-data-detail="propDetail" 
                            :data-input="dataInput"   
                            :placeholder="$attrs.placeholder"
                            :focus-search="focusSearch"
                            v-model="dataInput.DisplayValue"
                            @UpdateMruData="UpdateMruData"
                            @ResetDataMru="ResetDataMru">
                        </eMruSearch>
                        <eMruBody                     
                            :data-input.sync="getInput"                            
                            :data-mru="MruDataSource">
                        </eMruBody>
                        <eMruFooter 
                            :data-input="dataInput"
                            @UpdateMruData="UpdateMruData"
                            @openSpecificDialog="openSpecificDialog"
                            @addMethod="addMethod">
                        </eMruFooter>  
                    </div>
`,
};