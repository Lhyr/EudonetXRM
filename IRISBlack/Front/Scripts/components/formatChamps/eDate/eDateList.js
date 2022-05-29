import { eDateMixin } from './eDateMixin.js?ver=803000'

export default {
    name: "eDateList",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eDateMixin],
    template: `

    <div v-if="propListe" v-bind:class="[propListe ? 'listRubriqueDate' : '', 'ellips input-group hover-input']"  :title="!dataInput.ToolTipText ? dataInput.Value : dataInput.ToolTipText">

        <!-- Si le champ date n'est pas dans le head et est modifiable -->
        <div v-on:click="[!IsDisplayReadOnly ?  openCalendar($event): '']" type="text" v-bind:class="[IsDisplayReadOnly ? 'readOnlyDate' : '', 'ellipsDiv form-control input-line fname']">
            <!-- <div v-on:mouseout="icon = false" v-on:mouseover="icon = true" class="targetIsTrue" v-bind:style="{ color: dataInput.ValueColor}">{{dataInput.Value}}
            </div> -->

            <input :placeholder="dataInput.Watermark" :field="'field'+dataInput.DescId" :value="getFormat" v-on:focus="focusIn = true" autocomplete="off" v-on:blur="blurInputValidate($event); focusIn = false" v-on:click="openCalendar($event, 'focused')" v-bind:id="[propDetail ? 'id_input_detail_' + dataInput.DescId : propHead ? 'id_input_head_' + dataInput.DescId : propAssistant ? 'id_input_assistant_' + propAssistantNbIndex + '_' + dataInput.DescId : propListe ? 'id_input_liste_' + propIndexRow + '_' + dataInput.DescId : '']"  v-if="!IsDisplayReadOnly"  type="text" :class="'form-control input-line fname id_' + dataInput.DescId">
            <span  v-on:mouseout="icon = false" v-on:mouseover="icon = true" class="readOnly dateInput" v-else>{{getFormat}}</span>

        </div>

        <!-- Icon -->
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
