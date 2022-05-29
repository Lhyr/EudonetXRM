import { updateMethod, focusInput, verifComponent, updateListVal, showInformationIco, displayInformationIco } from '../../methods/eComponentsMethods.js?ver=803000';
import { isImgLinkBroken, OpenModalPicture, doGetImageGeneric, deleteImageIris, ErrorImageDeleteReturn, ImageDeleteReturn, onImageCancel } from '../../methods/eComponentImageMethods.js?ver=803000';
import { PropType } from '../../methods/Enum.min.js?ver=803000';
import { eFileComponentsMixin } from '../../mixins/eFileComponentsMixin.js?ver=803000';
import { DEFAULT_FIELD_HEIGHT, DEFAULT_FIELD_MARGIN } from '../../methods/eFileConst.js?ver=803000';

export default {
    name: "eImage",
    data() {
        return {
            modif: false,
            that: this,
            brokenImgLink: false,
            isValidUrl: false,
            imgHeight: 115,
            yPadding: 30,
            baseUrl:'',
        };
    },
    components: {
        eFileLoader: () => import(AddUrlTimeStampJS("../fiche/FileLoader.js"))
    },
    mounted() {
        this.setContextMenu();
        this.displayInformationIco();
    },
    methods: {
        displayInformationIco,
        showInformationIco,
        OpenModalPicture,
        doGetImageGeneric,
        deleteImageIris,
        ErrorImageDeleteReturn,
        ImageDeleteReturn,
        onImageCancel,
        isImgLinkBroken
    },
    computed: {
        /** hauteur des images en fonction du nombre de ligne et de la hauteur des champs */
        getImageMaxHeight: function () {
            return {height: !this.propAssistant ? (this.dataInput.Rowspan * DEFAULT_FIELD_HEIGHT - DEFAULT_FIELD_MARGIN) + 'px' : ''};
        },
        getAvatar: function () {
            let baseUrl = document.location.origin + '/' + document.location.pathname.split('/')[1] + '/' + "IRISBlack/Front/Assets/Imgs/default.jpg";

            if (this.dataInput.Value != '' && !this.brokenImgLink) 
                baseUrl = document.location.origin + '/' + document.location.pathname.split('/')[1] + '/' + this.getBaseUrl + "files/" + this.dataInput.Value;

            return baseUrl;
        },
        /** border-radius en fonction du type de fiche */
        getImgRadius() {
            return this.getTab == eTools.DescIdEudoModel.TBL_PP ? 'rounded-circle' : ''
        },
        /** classe css en fonction du type de fiche */
        getImgCssClass() {
            return this.getTab == eTools.DescIdEudoModel.TBL_PP ? 'contactImg' : ''
        },
        isReadOnly() {
            return this.dataInput.ReadOnly && this.propListe ? 'img-read-only' : !this.dataInput.ReadOnly && (this.propHead && this.propHeadAvatar) ? 'enableClickImg':'';
        },
        getHeight() {
            return this.getTab != eTools.DescIdEudoModel.TBL_PP ? `maxHeight:${this.imgHeight}px` : '';
        },
        /** Classes CSS de l'image */
        getImageCssClass(){
            return {
                'default-img': this.dataInput.Value == '',
                'cursor-pointer': !this.dataInput.ReadOnly,
                'single-line': this.dataInput.Rowspan === 1,
            }
        },
        /** Classe css du conteneur de l'image */
        getImgCtrClass(){
            return{
                'image-container':true,
                'image-container__head':this.propHead,
                'image-container__step':this.propAssistant
            }
        }
    },
    props: ["dataInput", "propHead", "propHeadAvatar", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
    mixins: [eFileComponentsMixin],
    watch: {
        "dataInput.Value": function () {
            this.brokenImgLink = false;
        }
    },
    template: `


<div
    :class="getImgCtrClass" 
    :did="[dataInput.DescId]" 
    :fid="[dataInput.fid]" 
    :dbv="[dataInput.dbv]" 
    :eavatar="[dataInput.eavatar]" 
    :tab="[]" 
    :id="['COL_' + getTab + '_' + dataInput.DescId]" 
    :efld="1">
    <!-- FICHE -->
    <template v-if="propHead && propHeadAvatar">
        <!-- US #1904 - Avatar affiché par eImage -->
        <div ref="imgFiche" :class="['profile-user-img-content-zr d-flex justify-center align-center', getImgCssClass]">
            <img             
            :class="['profile-user-img-zr img-responsive',getImgRadius,isReadOnly]" 
            :src="getAvatar" 
            :alt="getRes(7461)"
            irisblackimg
            @error="isImgLinkBroken()" 
            @click="OpenModalPicture($event, this.getAvatarOpenModalPictureType);">
        </div>
    </template>
    <template v-else>
        <div 
        v-if="!propListe" 
        :class="['ellips input-group hover-input rubriqueImage', getImageCssClass]"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText"
        >
            <img
            @error="isImgLinkBroken()" 
            @click="OpenModalPicture($event, 'IMAGE_FIELD')"  
            :src="getAvatar" 
            :ename="['COL_' + getTab  + '_' + dataInput.DescId]" 
            irisblackimg>       
        </div>
    </template>

    <div v-if="propListe" :class="['ellips input-group hover-input rubriqueImage img-field--list',isReadOnly,getImageCssClass]"
        :title="!dataInput.ToolTipText ? dataInput.DisplayValue : dataInput.ToolTipText">
        <img @click="OpenModalPicture($event, 'IMAGE_FIELD')" 
        :src="[dataInput.Value == '' ? 'IRISBlack/Front/Assets/Imgs/default.jpg' : getBaseUrl+'files'+'/'+dataInput.Value]" 
        :ename="['COL_' + getTab  + '_' + dataInput.DescId]" 
        irisblackimg>     
    </div>

</div>

`

};