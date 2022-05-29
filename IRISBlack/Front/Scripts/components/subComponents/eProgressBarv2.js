import { eModalComponentsMixin } from '../../mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eProgressBar",
    data() {
        return {

        }
    },
    props: {
        min: { type: Number, default: 0 },
        max: { type: Number, default: 100 },
        value: { type: Number, default: 0 },
        label: { type: String, default: this.getRes(6545) },
    },
    mixins: [eModalComponentsMixin],
    template: `
        <div>
            <label for="pgEudo">{{label}}</label>
            <progress id="pgEudo" ref="pgEudo" :max="max" :min="min" :value="value" class="eudoprogress"></progress>
            <output id="spPgEudo">{{getRes(6713)}} ... <strong ref="pgStrongUploadValue" class="pgStrongUploadValue">{{value}}</strong> <b>%</b></output>
        </div>
`
}