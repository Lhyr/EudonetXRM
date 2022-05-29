
export default {
    name: "eButtonList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    template: `
    <div v-if="propListe" v-bind:style="{ width: '100%!important', textAlign: 'center'}"  v-bind:class="[propListe ? 'listRubriqueBtn' : '', 'btn_eudo_content ellips input-group hover-input']"  
        :title="!dataInput.ToolTipText ? dataInput.Label : dataInput.ToolTipText">
        
        <!-- modification -->
        <button v-if="!dataInput.ReadOnly" v-on:click="verifButton($event, that)"  v-bind:style="{ width: 'auto!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}"   :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
        
        <!-- non modification -->
        <button v-if="dataInput.ReadOnly" disabled v-bind:style="{ width: 'auto!important', color: dataInput.StyleForeColor+'!important', background: dataInput.BackgroundColor+'!important', borderColor: dataInput.BackroundColor+'!important'}"  :value="dataInput.Value" type="button" class="btn_eudo btn btn-default nouveau-contact">{{dataInput.Label}}</button>
       
    </div>
`
};