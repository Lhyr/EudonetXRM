import { eModalComponentsMixin } from '../../../Scripts/mixins/eModalComponentsMixin.js?ver=803000';

export default {
    name: "eDropdown",
    mixins: [eModalComponentsMixin],
    data() {
        return {
       
            dropDownList: false
        };
    },
    computed: {
        isOpen() {
            if (this.dropDownList) {
                return 'is-open'
            }
        }
    },
    mounted() {
        if (this.$parent && this.handleOutsideClick)
            this.$parent.$el.addEventListener('click', () => { this.dropDownList = false })

        //if (this.$refs.realselect) {
        //    this.$refs.realselect.focus();
        //}
    },
    methods: {
        /**
         * Lie le select au dropdown customisé
         * */
        changeOptVal() {
          
        //    this.propDropdown.optionSelected = this.$refs.realselect.value
            this.$emit('input', this.$refs.realselect.value)

            this.goAction(this.propDropdown.opt.find(x => this.getOptVal(x,"goaction") == this.$refs.realselect.value))
        },

        getOptVal(opt,sfrom) {           
            let val = (typeof opt.value != "undefined") ? opt.value : opt.name            
            return val
        },

        getOptLabel(val) {
            let label;
            if (this.propDropdown.optionSelected == this.getRes(2596))
                label = this.getRes(2596);
            else
                label = this.propDropdown.opt.find(x => this.getOptVal(x, "goaction") == val).name;
            return label;
        },

        /**
         * Methode qui lie le changement d'options à une action si définie dans l'objet propDropdown
         * @param {any} opt
         */
        goAction(opt) {

            this.$refs.realselect.value = this.getOptVal( opt)
            this.$emit('input', this.$refs.realselect.value)
            if (this.propDropdown.action)
                this.propDropdown.action(opt, this);
        }
    },
    //props: ["propDropdown", "handleOutsideClick"],
    /* Objet attendu par le dropdown */
    props: {
        propDropdown: {
            optionSelected: "",
            opt: [{
                name: "",
                view:"",
            }],
            action:""
        },
        handleOutsideClick: false,
        value:""
    },

    template: `
    <div class="dropdown-container">
    <div class="select-container">
        <span @click.stop="dropDownList = !dropDownList;"  class="select-option visible-option">
            <div class="visible-value" :title="getOptLabel(propDropdown.optionSelected)" >{{getOptLabel(propDropdown.optionSelected)}}</div>
            <span class="arrow-down"></span>
        </span>
        <div :class="[isOpen, 'option-container']">
            <span @click="goAction(opt);dropDownList = false;" class="select-option" v-for="opt in propDropdown.opt">
                {{opt.name}}
            </span>
        </div>
        <select class="real-select" :value-selected="propDropdown.optionSelected" @input="changeOptVal" ref="realselect" tabindex="0" :value="propDropdown.optionSelected" >
            <option v-for="opt in propDropdown.opt" :value="getOptVal(opt)">{{opt.name}}</option>
        </select>
    </div>
</div>
`
};