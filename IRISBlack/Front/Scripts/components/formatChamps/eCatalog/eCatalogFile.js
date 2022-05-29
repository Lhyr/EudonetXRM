import { eCatalogMixin } from './eCatalogMixin.js?ver=803000'

export default {
    name: "eCatalogFile",
    data() {
        return {};
    },
    components: {
        eCatalogFileSimpleEditable: () => import(AddUrlTimeStampJS("./eCatalogFileSimpleEditable.js")),
        eCatalogFileSimpleNotEditable: () => import(AddUrlTimeStampJS("./eCatalogFileSimpleNotEditable.js")),
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCatalogMixin],
    template: `
<div ref="catalog" v-on:mouseout="showTooltip(false,'catalog',false,IsDisplayReadOnly,dataInput)"  
    v-on:mouseover="showTooltip(true,'catalog',false,IsDisplayReadOnly,dataInput)" v-if="!propListe" v-bind:class="[IsDisplayReadOnly? 'headReadOnly' : '', 'ellips input-group hover-input']"  >

   <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <!-- Si le champ simple et modifiable -->
        <eCatalogFileSimpleEditable />

        <!-- Si le champ simple et pas modifiable -->
        <eCatalogFileSimpleNotEditable />

        <!-- Icon -->
        <span @click="showCatalogGenericViewIris" v-if="!propHead" class="input-group-addon"><a href="#!" class="hover-pen"><i :class="[IsDisplayReadOnly?'fas pencil-alt-slash-eudo':'fas fa-pencil-alt']"></i></a></span>

		<!-- Message d'erreur après la saisie dans le champs -->
		<eAlertBox v-if="this.bEmptyDisplayPopup && !(propHead || dataInput.ReadOnly)" >
			<p>{{getRes(2471)}}</p>
		</eAlertBox>
    </div>
`
};