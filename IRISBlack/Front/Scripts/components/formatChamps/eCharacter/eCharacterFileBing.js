import { eCharacterMixin } from './eCharacterMixin.js?ver=803000'

export default {
    name: "eCharacterFileBing",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eCharacterMixin],
    template: `
    <div :class="[provider == 'bing' ? 'bingAutoComplete' : provider == 'datagouv' ? 'dataGouvAutoComplete' : '', 'contentAutoComplete']" v-if="(provider == 'datagouv' || provider == 'bing') && objAutoCompletion && objAutoCompletion.length > 0 && openAutoCompletion" >
        <ul>
            <li @click="setValueAutocompletion(address, $event);  OnUpdateAutocompletion = true" v-for="address in objAutoCompletion">
                <a class="suggestLink">
                    <div class="as_suggestion_root_inside">
                        <div class="as_img_maps_address"><i class="fas fa-map-marker-alt"></i></div>
                        <div class="as_lines_root">
                            <p v-if="address.label" class="autoComplete_line1">{{address.label ? address.label : ''}}</p>
                            <p v-if="(address.postalCode || address.city || address.country) && provider == 'bing' " class="autoComplete_line2">{{address.postalCode ? address.postalCode : ''}} {{address.city ? address.city : '' }} {{ address.country ? address.country : ''}}</p>
                            <p v-if="(address.region || address.department || address.country) && provider == 'datagouv' " class="autoComplete_line2">{{address.department ? address.department : '' }} {{ address.region ? '(' + address.region + ')' : ''}} {{ address.country ? address.country : ''}}</p>
                        </div>
                    </div>
                </a>
            </li>
        </ul>
    </div>
`
};