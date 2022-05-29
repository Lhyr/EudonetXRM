export default {
    template: `
    <div class="dropdown edn-dropdown-btn">
        <v-menu 
            nudgeTop="85" offset-y 
            content-class="elevation-0" 
        >
            <template v-slot:activator="{ on, attrs }">
                <v-btn
                    v-bind="attrs"
                    v-on="on"
                    text
                    light
                    height="100%"
                    class="pa-2"
                >
                    <slot name="prepend"></slot>
                    <slot name="content"></slot>
                    <slot name="append"></slot>
                </v-btn>
            </template>
            <v-list outlined>
                <v-list-item
                    v-for="(stepDropDown,index) in aStepDrop"
                    :key="stepDropDown.id" 
                    @click="actionDropDown(stepDropDown)"
                >
                    <v-list-item-title>{{stepDropDown.label}}</v-list-item-title>
                </v-list-item>
            </v-list>
      </v-menu>
    </div>
`,
    name: "eDropDownButton",
    data() {
        return {}
    },
    created() { },
    mounted() {
    },
    updated() {},
    computed: {},
    methods: {
        /**
        * L'action qui va s'éxecuter lorsqu'une action provient du bouton du dropdown.
        * @param {any} action
        */
        actionDropDown: function (action) {
            this.$emit("actionDropDownClicked", action);
        },
    },
    props: {
        sAlign : String,
        sButtonTitle: String,
        sButtonId: String,
        sButtonClass: String,
        sLeftIcon: String,
        sRightIcon: String,
        aStepDrop: Array,
        index: Number,
        PropStep: Object,
        step: Object
    },
}