import { eMruBodyMixin } from '../../mixins/eMruBodyMixin.js?ver=803000';

export default {
    name: "eMruBody",
    mixins: [eMruBodyMixin],
    data() {
        return {
        };
    },
    mounted() {
    },
    template: `
   <ul class="mru-list">
        <!--Non renseigné-->
        <li ref="mruEmptyCatalog" class="mru-li empty-mru" 
        v-show="dataInput.DisplayValue!=''"
        @mouseout="hoveringElm(false)"  
        @mouseover="hoveringElm(true,'mruEmptyCatalog',getRes(EmptyFieldOptionRes))" 
        @click.stop="notEmptyMru">{{getRes(EmptyFieldOptionRes)}}</li>
        <!--Valeur non disponible-->
        <li class="mru-li mru-no-result" :title="getRes(6195)" v-show="IsMRUContent">{{getRes(6195)}}</li>
         <!--affichage liste-->
        <li ref="mruCatalog" 
        @mouseout="hoveringElm(false)"  
        @mouseover="hoveringElm(true,'mruCatalog',lstMru.DisplayLabel,index)" 
        class="mru-li" :id="lstMru.DbValue" v-for="(lstMru,index) in DataMru" :key="index" @click.stop="selectLabelMru(lstMru)">{{lstMru.DisplayLabel}}</li> 
  </ul>
`,
};