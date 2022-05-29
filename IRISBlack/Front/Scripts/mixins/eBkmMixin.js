import { eMotherClassMixin } from './eMotherClassMixin.js?ver=803000';
import EventBus from '../bus/event-bus.js?ver=803000';
import { BKMVIEWMODE } from '../methods/Enum.js?ver=803000';

/**
 * Mixin commune aux listes et aux composants directs (mais pas leurs sous-composants).
 * */
export const eBkmMixin = {
    data() {
        return {
            oPinParams : {
                pin:false,
                descId:this.signet.DescId,
                Label:this.signet.Label,
                id:"pinned_" + this.signet.DescId,
                type: '',
                BKMVIEWMODE
            }
        }
    },
    mixins: [eMotherClassMixin],
    components: {
    },
    created() {
    },
    computed: {
        /** Récupère le contenu de l'entête des signets */
        getBkmHeaderContent(){
            return{
                historiqueTitle: this.getRes(2569),
                expressTitle: this.getRes(6369),
                pinnedBkmTitle: this.getRes(3067),
                pinnedBkm:this.bPinned
            }
        },
        /** Récupère ou modifie le statut du signet épinglé */
        pinnedBkmVal:{
            get(){
                return this.bPinned;
            },
            set(bVal){
                this.$emit('update:bPinned',bVal)
            }
        }
    },
    props:{
        bPinned:{
            type:Boolean,
            default:()=> true
        },
        signet: {
            type: Object,
            default: {}
        }
    },
    methods: {
        SetHistoric(nBkmTab, callBack) {
            var that = this;
            var updatePref = "tab=" + this.getTab + ";$;bkmhisto" + ";$;bkm=" + nBkmTab.DescId;
            this.setUserBkmPref({
                updatePref,
                callback: function() {
                    that.reloadBkm(nBkmTab, function () {
                        nBkmTab.HistoricActived = !nBkmTab.HistoricActived;
                        if (typeof callBack === 'function')
                            callBack();
                    });

                }
            });

        },
        reloadBkm(bkm, func) {
            var options = {
                id: bkm.id,
                signet: bkm.DescId,
                nbLine: 9,
                pageNum: 1

            };
            EventBus.$emit('reloadSignet_' + bkm.id, options);
            if (typeof func === 'function')
                func();
        },
        /**
        * Renvoie si le signet est epinglé
        * @param {any} bVal
        * @param {any} promBkmData
        * @param {any} promLayout
        * @param {any} bkmViewMode
        * @param {any} fileId
        * @param {any} idxRow
        * @param {any} aFileId
        */
        pinBkm(bVal, promBkmData, promLayout, bkmViewMode = BKMVIEWMODE.LIST, fileId, idxRow, aFileId)  {
            let oPinnedBkm = {
                ...this.signet,
                pin: bVal,
                descId: this.signet.DescId,
                ViewMode: bkmViewMode,
                fileId: fileId,
                promBkmData: promBkmData,
                promBkmFileLayout: promLayout,
                idxRow: idxRow,
                aFileId: aFileId

            };
            this.$emit('pinnedBkm', oPinnedBkm);
        },
        /** Renvoie si la fiche est epinglé */
        pinFile(oVal) {
            this.pinInTab({
                ...this.oPinParams,
                ...{
                    pin:oVal.val,
                    Label:oVal.content.MainFileLabel,
                    id: "pinned_" + oVal.content.MainFileId,
                    fileId: oVal.content.MainFileId,
                    type:oVal.type
                }
            });
        },
        /** Epingle l'élément */
        pinInTab(oElem){
            this.pinnedBkmVal = oElem.pin;
            this.$emit('pinnedBkm',oElem);
        }
    }
}