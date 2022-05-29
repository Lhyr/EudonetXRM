import { eImageMixin } from './eImageMixin.js?ver=803000'
import { OpenModalPicture, doGetImageGeneric, deleteImageIris, ErrorImageDeleteReturn, ImageDeleteReturn, onImageCancel } from '../../methods/eComponentImageMethods.js?ver=803000';

export default {
    name: "eImageList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eImageMixin],
    template: `
    <div v-if="propListe" style="textAlign: center;display: block;" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input rubriqueImage']"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
        <img v-if="!dataInput.ReadOnly"  v-bind:style="{cursor:'pointer',maxWidth: '100%', maxHeight:'30px'}" v-on:click="OpenModalPicture($event, 'IMAGE_FIELD')" :src="[dataInput.Value == '' ? 'IRISBlack/Front/Assets/Imgs/default.jpg' : getBaseUrl+'files'+'/'+dataInput.Value]" :ename="['COL_' + getTab  + '_' + dataInput.DescId]" irisblackimg>
        <img v-if="dataInput.ReadOnly" v-bind:style="{ maxWidth: '100%', maxHeight:'30px'}" :src="[dataInput.Value == '' ? 'IRISBlack/Front/Assets/Imgs/default.jpg' : getBaseUrl+'files'+'/'+dataInput.Value]" :ename="['COL_' + getTab + '_' + dataInput.DescId]" irisblackimg>        
    </div>
`
};
