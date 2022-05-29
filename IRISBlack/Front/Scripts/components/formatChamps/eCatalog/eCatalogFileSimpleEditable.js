import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogFileSimpleEditable",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
        <div @click="showCatalogGenericViewIris" v-bind:style="{ color: dataInput.ValueColor}" v-if="!(propHead || dataInput.ReadOnly) && !dataInput.Multiple"  type="text" class="ellipsDiv multiRenderer form-control input-line fname">
        <ul :ref="'field'+dataInput.DescId" :field="'field'+dataInput.DescId">
            <li class="cat-placeholder" v-if="this.dataInput.Value == ''  && dataInput.Watermark != null" >{{dataInput.Watermark}}</li>
            <li v-if="dataInput.DisplayValue">
                {{dataInput.DisplayValue}}
                <span ref="uniqueval" v-on:mouseout.stop="showTooltip(false,'uniqueval',false,IsDisplayReadOnly,dataInput)" v-on:mouseover.stop="showTooltip(true,'uniqueval',false,IsDisplayReadOnly,dataInput)" v-if="dataInput.DisplayValue" class="multiple_choice_remove fas fa-times-circle cursor_pointeur" role="presentation" @click="removeOneValue($event, valueMultiple)"></span>
            </li>
        </ul>
        </div>
`
};