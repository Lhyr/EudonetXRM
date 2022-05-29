import { eMruFooterMixin } from '../../mixins/eMruFooterMixin.js?ver=803000';

export default {
    name: "eMruFooter",  
    mixins: [eMruFooterMixin],   
    data() {
        return {
        };
    },      
    template: `
             <ul ref="eMruFooter" class="mru-button-footer">            
                <!--Charger toutes les valeurs-->		       
                <li ref="allMru" v-if="bDisplayLoadAllOption" class="mru-li" 
                @mouseout="hoveringElm(false)"  
                @mouseover="hoveringElm(true,'allMru',getRes(1126))" 
                @click.prevent.stop="loadAllValues">{{getRes(1126)}}</li>
                <!--Ajouter-->	
                <li v-if="bDisplayAddOption" ref="mruModal" class="mru-li"
                @mouseout="hoveringElm(false)"  
                @mouseover="hoveringElm(true,'mruAdd',getRes(18))" 
                @click.prevent.stop="addMethod">
                    <i class="fas fa-plus navSearchSpan list-opening" :title="getRes(18)"></i>
                    {{getRes(18)}}
                </li>               
                <!--Toute la liste-->
                <li ref="mruModal" class="mru-li"
                @mouseout="hoveringElm(false)"  
                @mouseover="hoveringElm(true,'mruModal',getRes(6193))" 
                @click.prevent.stop="openSpecificDialog($event)">
                    <i class="fas fa-search navSearchSpan list-opening" :title="getRes(6193)"></i>
                    {{getRes(6193)}}
                </li>
            </ul>
`,   
};