import { eCharacterMixin } from './eCharacterMixin.js?ver=803000'

export default {
    name: "eCharacterFileSirene",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCharacterMixin],
    template: `
    <div class="sireneAutoComplete contentAutoComplete" v-if="provider == 'sirene' && objAutoCompletion && objAutoCompletion.length > 0 && openAutoCompletion" >
        <ul>
            <li @click="setValueAutocompletion(address, $event); OnUpdateAutocompletion = true" v-for="address in objAutoCompletion">
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside">
                        <div class="as_img_maps_address"><i class="fas fa-building"></i></div>
                        <div class="as_lines_root">
                            <span v-bind:class="address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value == '' ? 'openEtab' : 'closeEtab'">{{address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value != '' ? getRes(8558) + ' ' + address.Fields.find(a => a.FieldAlias == 'DFERMETUREET').Value : ''}}</span>
                            <p class="autoComplete_line1">{{address.Fields.find(a => a.FieldAlias == "L1_NORMALISEE").Value}}<span class="sirenFront"> - {{address.Fields.find(a => a.FieldAlias == "SIREN").Value}}</span></p>
                            <p class="autoComplete_line2">{{address.Fields.find(a => a.FieldAlias == "L4_NORMALISEE").Value}} {{address.Fields.find(a => a.FieldAlias == "L6_DECLAREE").Value}} {{address.Fields.find(a => a.FieldAlias == "L7_NORMALISEE").Value != "" ? "(" + address.Fields.find(a => a.FieldAlias == "L7_NORMALISEE").Value + ")" : ""}}</p>
                            <p class="autoComplete_line2"><span>{{address.Fields.find(a => a.FieldAlias == "SIEGE").Value == "0" ? getRes(8557) : getRes(8556)}}</span> - <span class="etabType">{{address.Fields.find(a => a.FieldAlias == "LIBAPET").Value}}</span> </p>         
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
`
};