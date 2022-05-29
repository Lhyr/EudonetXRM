import { eImageMixin } from './eImageMixin.js?ver=803000'
import { DEFAULT_FIELD_HEIGHT, DEFAULT_FIELD_MARGIN } from '../../methods/eFileConst.js?ver=803000';
import { OpenModalPicture, doGetImageGeneric, deleteImageIris, ErrorImageDeleteReturn, ImageDeleteReturn, onImageCancel } from '../../methods/eComponentImageMethods.js?ver=803000';

export default {
    name: "eImageFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    computed: {
        getImageMaxHeight: function () {
            return (this.dataInput.Rowspan * DEFAULT_FIELD_HEIGHT - DEFAULT_FIELD_MARGIN) + 'px';
        }
    },
    props: [],
    mixins: [eImageMixin],
    template: `
    <template v-if="propHead && propHeadAvatar">
        <!-- US #1904 - Avatar affiché par eImage -->
        <div ref="imgFiche" class="profile-user-img-content-zr d-flex justify-center">
            <img v-if="reloadImg" v-bind:class="getTab == 200 ? 'img-circle' : ''" class="profile-user-img-zr img-responsive" :src="getAvatar" :alt=""{{getRes(7461)}}" v-on:click="OpenModalPicture($event, this.getAvatarOpenModalPictureType);">
            <div v-else v-bind:class="getTab == 200 ? 'img-circle' : ''" class="profile-user-img-zr img-responsive" :alt=""{{getRes(7461)}}" v-on:click="OpenModalPicture($event, this.getAvatarOpenModalPictureType);"><eFileLoader css-class="loaderHeader" /></div>
        </div>
    </template>
    <template v-else>
        <div v-if="!propListe" v-bind:class="[propHead ? 'headReadOnly' : '', 'ellips input-group hover-input rubriqueImage']"
            :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">

            <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

            <img v-if="!(dataInput.ReadOnly || propHead)"  v-bind:style="{cursor:'pointer',maxWidth: '100%', maxHeight: this.getImageMaxHeight }" v-on:click="OpenModalPicture($event, 'IMAGE_FIELD')" :src="[dataInput.Value == '' ? 'IRISBlack/Front/Assets/Imgs/default.jpg' : getBaseUrl +'files'+'/'+dataInput.Value]" :ename="['COL_' + getTab  + '_' + dataInput.DescId]" irisblackimg>
            <img v-if="dataInput.ReadOnly || propHead" v-bind:style="{ maxWidth: '100%', maxHeight: this.getImageMaxHeight }" v-on:click="OpenModalPicture($event, 'IMAGE_FIELD')" :src="[dataInput.Value == '' ? 'IRISBlack/Front/Assets/Imgs/default.jpg' : getBaseUrl+'files'+'/'+dataInput.Value]" :ename="['COL_' + getTab + '_' + dataInput.DescId]" irisblackimg>        
        </div>
    </template>
`
};
