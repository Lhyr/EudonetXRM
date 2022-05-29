import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "fileSettings",
    data() {
        return {
            aValues: [],
            bResumeMapPulse: false,
        }
    },
    props: {
        objItm: {
          type: Object
        },
    },
    computed: {
        /** Les clés qui contiennent les champs 
         * Bizzarerie de javascript, certains éléments sont passés par ref.
         * Donc, on copie (slice) le tableau dans un nouveau tableau.
         * Sinon, on ajoute un élément masqué.
         */
        fieldRows() {
            let obj = this.ObjItm?.content?.filter(itm => itm.type == "fields");
            obj.forEach(elm => {
                elm.content?.forEach(arr => {
                    if (Array.isArray(arr?.values) && arr.label != "Titre")
                        arr.values.splice(0, 0, { text: this.getRes(1432), val: 0 });
                    else if(Array.isArray(arr?.values))
                        arr.values = arr.values.slice();
                });
            });

            return obj;
        },
        /** Initialise et récupère les valeurs des selects */
        selectValue:{
            get(){
                return this.aValues;
            },
            set(obj){
                this.aValues[obj.rowid][obj.idx].value = obj.newVal;
            }

        },
        /** Renvoie la props qui contient le JSON */
        ObjItm:{
            get(){
                if(this.aValues.length < 1){
                    this.initSelectValues(); 
                }
                return this.objItm;
            },
            set(value){
                this.$emit('update:objItm',value);
            }
        }
    },
    methods: {
        /** Set l'image de la fiche pour montrer se qui sera rendu en affichage */
        setMap(input, rowid, idx, pulsed) {
            let inputRef = this.$refs["input-" + rowid + "-" + idx];
            if (Array.isArray(inputRef))
                inputRef = inputRef?.find(n => n);

            if (!this.selectValue?.[rowid]?.[idx]?.value?.val > 0) {
                inputRef.style.display = "none";
                this.bResumeMapPulse = pulsed;
                setTimeout(() => {
                    this.bResumeMapPulse = false;
                }, 2000);
            } else {
                inputRef.style.display = rowid == 2 ? "flex" : "block";
                let sClass = rowid != 2 ? 'pulse-color' : 'pulse-color-bg';
                pulsed ? inputRef.classList.add(sClass) : '';
                setTimeout(() => {
                    inputRef.classList.remove(sClass);
                }, 2000);
            }
        },
        /** Intialise les valeurs des champs select */
        initSelectValues(){
            this.objItm?.content?.filter(itm => itm.type == 'fields').forEach(field => {
                let obj = [];
                field?.content?.forEach(content => {
                    obj.push(content)
                })
                this.aValues.push(obj)
            })
        },
        /** Récupère les valeurs quand un champs se met à jour */
        getFieldSettingsValues(newVal,idx,rowid){
            this.selectValue = {newVal,idx,rowid}
        },
        /** Retourne l'objet demandé de la props objItm (footer ou contenu de headFiche) */
        getContentValues(name, key) {
            let obj = this.ObjItm?.content?.find(itm => itm.name == name)?.[key];
            return obj;
        },
        /** Récupère les valeurs des champs mise à jour avant enregistrement */
        getFieldsContent(){
            let arr = this.fieldRows;
            arr.forEach((itm,idx) =>{
                itm.content = this.aValues[idx];
            })

            arr = this.objItm.content.concat(
                arr.filter(
                    (item) => this.objItm.content.map(e => e.name).indexOf(item.name) < 0
                )
            );

            return arr;
        },
        /** action à emettre au parent */
        action(action){
            let param = '';
            if(action == 'save'){
                param = this.getFieldsContent();
            }
            this.$emit(action,param);
        },
        /**
         * 
         * @param {*} obj valeur qui remplace l'actuel quand on vide le champs
         */
        clearValue(rowid, idx, input){
            let clearObj = Object.assign({}, {
                rowid:rowid, 
                idx:idx
            });
            clearObj.newVal = {
                text: this.getRes(1432), 
                val: 0 
            };
            this.selectValue = clearObj;
            this.setMap(input, rowid, idx, true);
        },
        /*
         * Renvoie la ressource/le libellé affiché si la zone est mise en lecture seule
         * @param (object) row Rubrique concernée
         */
        getReadOnlyWarning(row) {
            return this.getRes(2026) + row?.title; // Vous n'avez pas les droits suffisants pour effectuer une mise à jour sur XXXXX
        },
    },
    mounted: function () {
        /** Set l'image de la fiche pour montrer se qui sera rendu en affichage */
        this.fieldRows.forEach((zone, index) => {
            zone.content.forEach((input, idx) => {
                this.setMap(input, index, idx, false);
            })
        })
    },
    mixins: [eFileMixin],
    template: `
    <v-card class="file-settings__wrapper">  
        <v-card-title class="text-h5 file-settings__title">
            <span class="flex-grow-1">{{getContentValues('header','content')}}</span>
            <v-btn
                icon
                @click="action('close')">
                <v-icon>mdi-close</v-icon>
            </v-btn> 
      </v-card-title>       
        <v-divider></v-divider>   
        <v-card-text 
        class="file-settings__content" 
        style="ObjItm?.style">
            <v-container fluid>
                <v-row>
                    <v-col>
                        <div class="wrapper-map">
                            <div :class="bResumeMapPulse ? 'pulse-color-bg' : ''"  class="my-3 resume-map">
                                <v-row>
                                    <div class="px-2 pt-1 grp-resume-map">
                                        <div ref="input-0-0" class="my-1 title-resume-map"></div>
                                        <div ref="input-0-1" class="my-1 s-title-resume-map"></div>
                                    </div>
                                </v-row>
                                <v-row>
                                    <div class="py-1 grp-resume-input-map">
                                        <div ref="input-0-2" class="ml-2 my-1 circle-avatar-resume"></div>
                                        <div class="grp-input-resume-map">
                                            <div :ref="'input-1-'+idx" v-for="(input, idx) in 6" class="my-1 mx-2 input-resume-map"></div>
                                        </div>
                                    </div>
                                </v-row>
                            </div>
                            <div ref="input-2-0" class="py-2 justify-center d-flex align-center my-3 assist-map">
                                <div class="circle-assist-map"></div>
                                <div v-for="circle in 7" class="ml-9 circle-assist-map"></div>
                            </div>
                            <div class="my-3 detail-map">
                                
                            </div>
                        </div>
                    </v-col>
                    <v-col>
                        <template v-for="(row,rowid) in fieldRows">
                            <v-divider v-if="row?.divider" />
                            <v-row class="pb-6">
                                <v-col cols="12 overline" class="file-settings__stitle">{{getContentValues(row?.name,'title')}}</v-col>
                                <div v-if="row?.readOnly" class="file-settings__warning">
                                    {{getReadOnlyWarning(row)}}
                                </div>                                
                                <v-col v-for="(field,idx) in getContentValues(row?.name,'content')" :key="field?.id" :cols="field?.col">
                                    <v-select 
                                        v-model="selectValue?.[rowid]?.[idx].value" 
                                        :items="field?.values" 
                                        :disabled="row?.readOnly"
                                        item-text="text"
                                        item-value="val"
                                        :label="field?.label" 
                                        @click:append-outer="clearValue(rowid, idx, field)"
                                        append-outer-icon="mdi-close"
                                        return-object
                                        @change="setMap(field, rowid, idx, true)">
                                    </v-select>
                                </v-col>
                            </v-row>
                        </template>
                    </v-col>
                </v-row>
            </v-container>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="file-settings__footer">
            <v-spacer></v-spacer>
            <v-btn v-for="btn in getContentValues('footer','content')" :key="btn?.id" 
            :color="btn?.color" 
            text 
            @click="action(btn?.action)">
                {{btn?.title}}
            </v-btn>
        </v-card-actions>
    </v-card>
    `
};
