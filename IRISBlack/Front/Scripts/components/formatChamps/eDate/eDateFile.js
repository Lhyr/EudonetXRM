import { eDateMixin } from './eDateMixin.js?ver=803000'

export default {
    name: "eDateFile",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eDateMixin],
    template: `
    <div ref="date" v-on:mouseout="showTooltip(false,'date',icon,IsDisplayReadOnly,dataInput, getFormat)" v-on:mouseover="showTooltip(true,'date',icon,IsDisplayReadOnly,dataInput, getFormat)" v-if="!propListe" v-bind:class="[focusIn ? 'focusIn': '', IsDisplayReadOnly?'read-only':'', propHead ? 'headReadOnly' : '', 'ellips input-group hover-input']">

        <!-- <span ref="info" v-if="showInformationIco()" class="icon-info-circle info-tooltip"></span> -->

        <input :placeholder="dataInput.Watermark" :field="'field'+dataInput.DescId" :value="getFormat" v-on:focus="focusIn = true" autocomplete="off" v-on:blur="blurInputValidate($event); focusIn = false" v-on:click="openCalendar($event, 'focused')" v-bind:id="[propDetail ? 'id_input_detail_' + dataInput.DescId : propHead ? 'id_input_head_' + dataInput.DescId : propAssistant ? 'id_input_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId : propListe ? 'id_input_liste_' + propIndexRow + '_' + dataInput.DescId : '']"  v-if="!IsDisplayReadOnly"  type="text" :class="'form-control input-line fname id_' + dataInput.DescId">
        <span class="readOnly dateInput" v-else>{{getFormat}}</span>
        
        <span v-on:click="focus($event)" class="input-group-addon">
            <a href="#!" class="hover-pen" >
                <i :class="[
                    (IsDisplayReadOnly && !icon)?'pencil-alt-slash-eudo'
                    :(IsDisplayReadOnly && icon)?'fas fa-calendar-plus'
                    :(!IsDisplayReadOnly && icon)?'fas fa-calendar-plus'
                    :'fas fa-pencil-alt']" >
                </i>
            </a>
        </span>
    </div>
`
};
