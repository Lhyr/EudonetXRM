import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import { eModalMixin } from '../../mixins/eModalMixin.js?ver=803000';
import { tabFormatForbid, tabFormatForbidHeadEdit } from '../../../Scripts/methods/eFileConst.js?ver=803000';
import { FieldType } from '../../../Scripts/methods/Enum.min.js?ver=803000'
import { forbiddenFormatHead, specialCSS } from '../../../Scripts/methods/eFileMethods.js?ver=803000';

export default {
    name: "alertModal",
    data() {
        return {
            animate: false,
            maximized: false,
            tabFormatForbid,
            tabFormatForbidHeadEdit,
            FieldType,
            rightMenuWidth: null
        };
    },
    components: {},
    props: ['propOptionsModal'],
    mixins: [eModalMixin],
    computed: {
        /** Renvoie la largeur de la modale si le menu de droite est présent ou pas. On fait un calc pour garder le côté responsive notamment si la fenêtre est resizée */
        getRightMenuWidth() {
            return (this.rightMenuWidth != null || this.rightMenuWidth != undefined)  ? `calc(100% - ${this.rightMenuWidth}px)` : ''
        },
        getTitleCssClass() {
            return !this.propOptionsModal.datas.sTitle ? 'titleOnly' : !this.propOptionsModal.datas.title ? 'sTitleOnly': '';
        }
    },
    template: `
    <div :id="propOptionsModal.id">
            <div ref="eudonetModal" class="eudonetModal" v-bind:class="{'eudo-maximized': this.maximized,'eudo-out': !this.animate,'eudo-hidden': !this.animate,'eudo-in': this.animate, 'eudo-closable': propOptionsModal.close,'eudo-maximizable': propOptionsModal.maximize,'eudo-zoom': propOptionsModal.type =='zoom'  }">
                <div class="eudo-dimmer"></div>    
                <div  class="eudo-modal">
                    <div :style="'width:' + propOptionsModal.width + 'px'" class="eudo-dialog">
                        <div class="eudo-commands">
                            <button class="eudo-pin"></button>
                            <button @click="setmaximized" class="eudo-maximize"></button>
                            <button @click="closeModal" class="eudo-close"></button>
                        </div>
                        <div class="eudo-header">
                            <div class="eudo-title">
                                <h3 class="title-h3-list-user"><i :class="propOptionsModal.titleIcon ? propOptionsModal.titleIcon : 'fa fa-folder' "></i>{{propOptionsModal.title}}</h3>
                            </div>
                        </div>
                        <div class="eudo-body">
                            <div v-if="!propOptionsModal.isHtml" class="eudo-content">
                                <div v-if="propOptionsModal.id == 'modifHead'">
                                    <div v-if="(!forbiddenFormatHead(propOptionsModal.datas.title.Format,tabFormatForbidHeadEdit) && propOptionsModal.datas.title) || !propOptionsModal.datas.title" :class="['col-inline titleModif',getTitleCssClass,propOptionsModal.col ? propOptionsModal.col : 'col-md-6']">
                                        <template v-if="propOptionsModal.datas.titleError.value">
                                            <p class="text-danger" >
                                                <span class="fas fa-exclamation-circle error-icon"></span>
                                                {{getErrorMsg(propOptionsModal.datas.titleError.title,'title')}}
                                            </p>
                                        </template>
                                        <span class="lib">{{getRes(7216)}} :</span>                                                                  
                                        <div :FileId="propOptionsModal.datas.title.FileId"  :DivDescId="propOptionsModal.datas.title.DescId" :class="['form-group',isReadOnly(propOptionsModal.datas.title)] ">
                                            <div class="modal-field-label boldLabel">
                                                {{propOptionsModal.datas.title.Label}}
                                            </div>
                                            <component :prop-resume-edit="true" :data-input="propOptionsModal.datas.title" :is="dynamicFormatChamps(propOptionsModal.datas.title)"></component>
                                    </div>                                                                  
                                    <div 
                                        v-if="propOptionsModal?.datas?.titleComplementValue && Object.keys(propOptionsModal?.datas?.titleComplementValue).length"
                                        :FileId="propOptionsModal?.datas?.titleComplementValue.FileId" 
                                        :DivDescId="propOptionsModal?.datas?.titleComplementValue.DescId" 
                                        class="form-group"
                                     >
                                            <div class="modal-field-label boldLabel">
                                                {{propOptionsModal?.datas?.titleComplementValue?.Label}}
                                            </div>
                                            <component 
                                                :prop-resume-edit="true" 
                                                :data-input="propOptionsModal.datas.titleComplementValue" 
                                                :is="dynamicFormatChamps(propOptionsModal.datas.titleComplementValue)"
                                            />
                                        </div>
                                    </div>
                                    <template 
                                        v-if="(forbiddenFormatHead(propOptionsModal.datas.title.Format,tabFormatForbidHeadEdit) && propOptionsModal.datas.title)"
                                    >
                                        <span class="lib">{{getRes(7216)}} :</span>
                                        <p class="text-danger alert-msg" >
                                            <span class="fas fa-exclamation-circle error-icon"></span>
                                            {{getErrorMsg(propOptionsModal.datas.title,'title')}}
                                        </p>
                                    </template>
                                    <div 
                                        v-if="!forbiddenFormatHead(propOptionsModal.datas.sTitle.Format,tabFormatForbidHeadEdit) && propOptionsModal.datas.sTitle"  
                                        :class="['col-inline StitleModif',getTitleCssClass,propOptionsModal.col ? propOptionsModal.col : 'col-md-6']"
                                    >
                                        <span class="lib">{{getRes(7109)}} :</span>                                        
                                        <div :FileId="propOptionsModal.datas.sTitle.FileId"  :DivDescId="propOptionsModal.datas.sTitle.DescId"  :class="['group-inner form-group',isReadOnly(propOptionsModal.datas.sTitle)] ">
                                            <div class="modal-field-label boldLabel">
                                                {{propOptionsModal.datas.sTitle.Label}}
                                            </div>
 
                                            <component :prop-resume-edit="true" :data-input="propOptionsModal.datas.sTitle" :is="dynamicFormatChamps(propOptionsModal.datas.sTitle)"></component>
                                        </div>                                                                               
                                        <div :FileId="propOptionsModal.datas.sTitleComplementValue.FileId" :DivDescId="propOptionsModal.datas.sTitleComplementValue.DescId" v-if="propOptionsModal.datas.sTitleComplementValue" :class="['form-group',isReadOnly   (propOptionsModal.datas.sTitleComplementValue)] ">
                                            <div class="modal-field-label boldLabel">
                                                {{propOptionsModal.datas.sTitleComplementValue.Label}}
                                            </div>
                                            <component :prop-resume-edit="true" :data-input="propOptionsModal.datas.sTitleComplementValue" :is="dynamicFormatChamps(propOptionsModal.datas.sTitleComplementValue)"></component>
                                        </div>
                                    </div>
                                    <template v-if="forbiddenFormatHead(propOptionsModal.datas.sTitle.Format,tabFormatForbidHeadEdit) && propOptionsModal.datas.sTitle" >
                                        <span class="lib alert-msg">{{getRes(7109)}} :</span>
                                        <p class="text-danger alert-msg" >
                                            <span class="fas fa-exclamation-circle error-icon"></span>
                                            {{getErrorMsg(propOptionsModal.datas.sTitle,'Subtitle')}}
                                        </p>
                                    </template>
                                </div>
                                <template v-else>
                                    <div v-if="input.Format != 17" v-bind:class="propOptionsModal.col ? propOptionsModal.col : 'col-md-6' " class="col-inline" v-for="input in propOptionsModal.datas" :key="input.id">
                                        <div class="form-group">
                                            <div class="modal-field-label">
                                                <div class=" text-muted">{{input.Label}}</div>
                                            </div>
                                            <component :data-input="input" :is="dynamicFormatChamps(input)"></component>
                                        </div>
                                    </div>
                                    <div v-if="input.Format == 17" :class="getMemoCssClass(input)" class="col-md-12 col-inline memoProp" v-for="input in propOptionsModal.datas" :key="input.id">
                                        <div class="form-group">
                                            <div class="modal-field-label">
                                                <div class=" text-muted">{{input.Label}}</div>
                                            </div>
                                            <component rows="3" resize="vertical" :data-input="input" :is="dynamicFormatChamps(input)"></component>
                                        </div>
                                    </div>
                                </template>
                            </div>
                         <div v-else class="eudo-content"">
                            <div v-html="propOptionsModal.content" class="col-md-12"></div>
                        </div>
                        </div>
                        <div class="eudo-footer">
                            <div class="eudo-buttons" v-bind:class="{ 'eudo-auxiliary': btn.type == 'left','eudo-primary': btn.type == 'right'}" v-for="btn in propOptionsModal.btns">
                                <button @click="closeModal" :class="'eudo-button btn btn-'+ btn.color">{{btn.lib}}</button>
                            </div>
                        </div>
                    </div>
                </div>  
            </div>
        </div>
`,
    methods: {
        forbiddenFormatHead,
        specialCSS,
        dynamicFormatChamps,
        setmaximized() {
            this.maximized = !this.maximized
        },
        closeModal() {
            document.body.style.overflow = null
            let that = this
            setTimeout(function () {
                that.animate = false;
            }, 100);

            setTimeout(function () {
                that.$emit('close');
            }, 600);
        },
        isReadOnly(elem) {
            return elem.ReadOnly ? 'readOnlyComponent' : ''
        },
        getKeyByValue(object, value) {
            return Object.keys(object).find(key => object[key] === value);
        },
        getErrorMsg(elem, area) {
            return area == 'Subtitle' ? this.getRes(2791).replace('<AREA>', this.getRes(7109)).replace('<FORMATNAME>', this.getKeyByValue(FieldType, elem.Format)) : this.getRes(2791).replace('<AREA>', this.getRes(7216)).replace('<FORMATNAME>', this.getKeyByValue(FieldType, elem.Format));
        },
        isInError(field) {
            return (field.ErrorMessage && field.ErrorMessage != '') ? 'text-danger' : ''
        },
        getMemoCssClass(input) {
            return input.ReadOnly ? 'memoReadonly':''
        }
    },
    destroyed() {
        if (this.propOptionsModal.observeMenu)
            this.propOptionsModal.observeMenu(false, this);
    },
    mounted() {
        document.body.style.overflow = "hidden"
        let that = this
        setTimeout(function () {
            that.animate = true;
        }, 100);

        this.rightMenuWidth = this.propOptionsModal.rightMenuWidth ? this.propOptionsModal.rightMenuWidth : null

        if (this.propOptionsModal.observeMenu)
            this.propOptionsModal.observeMenu(true, this)

    },
};