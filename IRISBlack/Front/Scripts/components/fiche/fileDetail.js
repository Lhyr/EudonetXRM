import { dynamicFormatChamps } from '../../../index.js?ver=803000';
import EventBus from '../../bus/event-bus.js?ver=803000';
import { LoadSignet, saveScrollPosition } from "../../methods/eFileMethods.js?ver=803000";
import { showTooltip, infoTooltip, handleExtendedProperties, isShowingInformation, isDisplayingToolTip } from '../../methods/eComponentsMethods.js?ver=803000';
import { getTabDescid } from "../../methods/eMainMethods.js?ver=803000";
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { specialCSS, forbiddenFormatHead } from "../../methods/eFileMethods.js?ver=803000";
import { tabFormatForbid, tabFormatBtnLbl, tabFormatBtnSep, tabFormatForbidLabel } from "../../methods/eFileConst.js?ver=803000";
import { FieldType } from "../../methods/Enum.min.js?ver=803000";

export default {
    name: "fileDetail",
    data() {
        return {
            fid: this.getFileId,
            did: this.getTab,
            requiredTitle: this.getRes(7548),
            mobile: false,
            visible: false,
            msgHover: "",
            position: '',
            focused: false,
            values: "",
            DataJson: null,
            tabFormatForbidLabel,
            FieldType,
        };
    },
    methods: {
        showTooltip,
        specialCSS,
        infoTooltip,
        isShowingInformation,
        isDisplayingToolTip,
        LoadSignet,
        getTabDescid,
        saveScrollPosition,
        /**
         * Cette fonction indique si le champ passé en paramètre peut être affiché dans la zone Détails
         * Les critères de sélection sont les suivants :
         * - son DescID ne doit PAS être celui d'un champ système ou Propriétés de la Fiche
         * - il doit être visible (merci Captain)
         * - [cf. #85 198] son DispOrder dans [DESC] doit être supérieur à 0 et non NULL (ISO E17, cf. filtrage fait dans eMasterFileRenderer.cs, fonction SortFields())
         * @param {any} input // Champ à examiner
         **/
        canBeDisplayed(input) {
            let tabIDMainFile = [99, 90, 88, 89, 84, 95, 97, 93, 96, 98, 74, 91, 94, 92];
            return !tabIDMainFile.map(num => this.getTabDescid(input.DescId) + num).includes(input.DescId) && input.IsVisible && input.DispOrder > 0;
        },
        dynamicFormatChamps,

        responsive: function () {
            this.mobile = ("matchMedia" in window) && !(window.matchMedia("(min-width:1100px)").matches);
        },

        /** détermine les styles des détails. */
        setFileStyle: function (input) {
            let tabStyle = new Array();

            let inputHeight = parseFloat(getComputedStyle(this.$root.$el).getPropertyValue('--input-height'));

            if(input?.Rowspan > 1) tabStyle.push({'height':(inputHeight * input?.Rowspan) + 'rem'})
                
            tabStyle.push(
                {
                    'grid-row-start': input.PosY + 1,
                    'grid-row-end': input.Rowspan + input.PosY + 1,
                    'grid-column-start': input.PosX + 1,
                    'grid-column-end': input.PosX + input.Colspan + 1
                }
            );

            if (this.canBeDisplayed(input)) {
                // Si l'élément est un titre séparateur (input.Format == 16),
                // on lui applique une couleur de fond correspondant à celle définie en admin pour sa police d'affichage,
                // mais avec une opacité de 5 % (en hexadécimal: 0d)
                if (input.Format == 16) {
                    tabStyle.push(
                        {
                            'background': (input.StyleForeColor ? input.StyleForeColor : '#000000') + '0d',
                            'border-left': '1px solid ' + (input.StyleForeColor ? input.StyleForeColor : '#000000')
                        }
                    );
                }
            }

            return tabStyle;
        },
        /**
         * détermine les classes pour les lables des composants.
         * @param {any} input
         * @param {any} index
         */
        setFileLabelClass: function (input, index) {
            return {
                'italicLabel': input.Italic,
                'boldLabel': input.Bold,
                'underLineLabel': input.Underline,
                'labelHidden': this.labelHidden[index],
                'd-none': this.labelHidden[index] && this.specialCSS(this.vuetifyInput, input?.Format),
                'no-label': this.noLabel[index]
            };
        },

        /**
         * Si on est en mode fiche incrustée, on passe ici, et on affiche les informations,
         * Comme en mode fiche normal, mais dans un signet.
         * */
        encrustedFileInit: async function () {

            try {
                this.DataJson = await this.LoadSignet(this.propDataDetail.DescId);
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(7050), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(6432).replace('<BKMRES>', this.propDataDetail.Label)
                });

                return false;
            }


            if (!this.DataJson)
                return false;

            let data = this.DataJson;
            let structure = this.DataJson;


            if (data.length > 0) {

                let tmpJson = this.DataJson.find(n => n);

                if (!tmpJson)
                    return false;

                data = tmpJson;
                structure = tmpJson;
            }

            data = (data.Data.length > 0) ? data.Data.flatMap(dt => dt.LstDataFields) : data.Data.LstDataFields;
            structure = structure.Structure;

            if (!data)
                return false;
                        
            data = data.filter(dtF => this.getTabDescid(dtF.DescId) == this.propDataDetail.DescId);

            if (!data)
                return false;

            data.forEach(dtF => {
                let struct = structure.LstStructFields.find(structF => structF.DescId == dtF.DescId);
                dtF = Object.assign(dtF, struct);
            });

            this.DataJson = data;
        },
        getFormat(format){
            return this.propDataDetail.map(input => {
                if (input.Format == format)
                    return true;
            })
        },
        isReadOnly(input) {
            return input.ReadOnly || input.Format == FieldType.Password
        },
        /** Détermine si le composant est un séparateur */
        isSeparator(input){
            return this.isDisplayed && input.Format == FieldType.Separator;
        },
        updateData(val){
            let idx = this.propDataDetail.map(ar => ar.Value).indexOf(val.Value);
            let newPropDataDetail = this.propDataDetail;
            newPropDataDetail[idx] = val;
                this.$emit('update:propDataDetail',newPropDataDetail)
        },

        /** Remonte l'apel du spinner */
        eWaiterDetail: function (oValue) {
            this.$emit("setWaitIris", oValue.bOn, oValue.nOpacity);
        },
    },
    computed: {
        
        labelHidden: function () {
            return this.propDataDetail.map(input => {
                if (input.LabelHidden || specialCSS(tabFormatBtnLbl, input.Format))
                    return true;
            })
        },
        noLabel: function () {
            return this.propDataDetail.map(input => {
                if (specialCSS(tabFormatBtnSep, input.Format))
                    return true;
            })
        },

        /**
         * Retourne une grille avec le nombre de colonnes 
         * que l'administrateur a passé en paramètre.
         * @return {string} le grid template avec les colonnes en 1fr.
         **/

        setGridTemplateStyle: function() {
            return {
                gridTemplateColumns:`repeat(${this.propCols}, 1fr)`
            }
        },
        isMobile() {
            return this.mobile ? 'reponsiveOK' : ''
        },
        isMemo() {
            return this.getFormat(FieldType.Memo);
        },
        isLogic() {
            return this.getFormat(FieldType.Logic);
        },
        isNumeric() {
            return this.getFormat(FieldType.Numeric);
        },
        getInputValue:{
            get(){
                return this.propDataDetail;
            },
            set(val){
            let idx = this.propDataDetail.map(ar => ar.Value).indexOf(val.Value);
            let newPropDataDetail = this.propDataDetail;
            newPropDataDetail[idx] = val;
                this.$emit('update:propDataDetail',newPropDataDetail)
            }
        },
        /** Computed pour forcer le rafraichissemnt des composants,
         * à partir du composant parent. */
        ForceRefreshTabsBar: {
            get: function () {
                return this.nForceRefreshTabsBar;
            },
            set: function (value) {
                this.$emit('update:nForceRefreshTabsBar', value)
            }
        }
    },
    async created() {
        this.fid = this.getFileId;
        this.did = this.getTab;

        this.DataJson = this.propDataDetail;

        if (this.propDataDetail["DescId"] && this.getTab != this.propDataDetail.DescId) {
            await this.encrustedFileInit();
        }
    },
    mounted() {
        this.responsive();
        this.$refs["filedetail"].addEventListener('resize', this.responsive, false);

        EventBus.$on('valueEdited', (options) => {
            var findItemData = this.DataJson.find(c => c.DescId == options.DescId && c.FileId == options.FileId);
            if (findItemData) {
                findItemData.Value = options.NewValue;
            }

            //gestion des propriétés étendues
            findItemData && handleExtendedProperties(findItemData, options)
     
            findItemData && (findItemData.DisplayValue != null || findItemData.DisplayValue != undefined) ? findItemData.DisplayValue = options.NewDisp : '';
       
        });
    },
    props:{
        propDataDetail:{
            type:Array
        },
        propIndexRow:{
            type:Number
        },
        propCols:{
            type:Number
        },
        propReloadDetailFunction:{
            type:Boolean
        },
        isDisplayed:{
            type:Boolean,
            default: false
        },
        nForceRefreshTabsBar: Number,
    },
    mixins: [eFileMixin],
    template: `
<div ref="filedetail" class="box-body detailContent" :style="setGridTemplateStyle">
    <div
        v-for="(input, index) in propDataDetail"
        :FileId="input.FileId"
        :class="[
            isReadOnly(input) ? 'readOnlyComponent' : '',
            isMemo[index] ? 'memoInput' : ''
        ]"
        v-if="canBeDisplayed(input)"
        class="fileInput"
        :tp="input.Format" 
        :DivDescId="input.DescId" 
        :style="setFileStyle(input)"  
        :x="input.PosX" :y="input.PosY" :colspan="input.Colspan"  
        :rowspan="input.Rowspan"
        :bf="input.Formula ? '1' : '0'"
        :mf="input.HasMidFormula || input.HasORMFormula ? '1' : '0'"
        :ref="'detail-input-' + input.DescId"   
    >
        <div 
            :class="[
                isLogic[index] ? 'reverse' : '',
                isMemo[index] ? 'd-flex flex-column' : '',
                getComponentType(input)
            ]"
            class="form-group"
        >
            <div
                class="left d-flex align-center"
            >
                <div
                    v-if="!specialCSS( tabFormatForbidLabel, input.Format)"
                    :ref="'label_' + input.DescId"
                    :style="{ color: input.StyleForeColor + '!important'}"
                    class="left-label text-truncate text-muted"
                    :class="setFileLabelClass(input)"
                >
                    {{input.Label}}
                </div> 
                <span
                    v-if="input.Required && !isNumeric[index]"
                    :ref="'hidden_' + input.DescId"
                    :title="requiredTitle" 
                    :class="{'labelHidden': labelHidden[index] ,'no-label':noLabel[index]}" 
                     class="requiredRubrique"
                >*</span>
                <span
                    v-if="isShowingInformation(input) && !isNumeric[index]"
                    :ref="'label_info' + input.DescId"
                    @mouseout="isDisplayingToolTip(false, input)"
                    @mouseover="isDisplayingToolTip(true, input)" 
                    class="icon-info-circle info-tooltip"
                ></span>
            </div>
            <component
                ref="fields"
                :isDisplayed="isSeparator(input)"
                :prop-reload-detail-function="propReloadDetailFunction"
                :mobile="mobile"
                :prop-detail="true"
                :dataInput.sync="getInputValue[index]"
                @update:data-input="updateData"
                :is="dynamicFormatChamps(input)"
                @saveScrollPosition="saveScrollPosition"
                @setWaitIris="eWaiterDetail"
                :key="ForceRefreshTabsBar"
            />
        </div>
    </div>
</div>`
};