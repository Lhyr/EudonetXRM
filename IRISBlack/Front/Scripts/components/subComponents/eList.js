export default {
    name: "eList",
    mixins: [],
    data() {
        return {

        };
    },
    computed: {
    },
    mounted() {
    },
    methods: {
        /** Emet le clic sur 1 élément de la liste */
        clickAction(list, idxRow, bodyRow) {
            this.$emit('clickAction', list, idxRow, bodyRow)
        }
    },
    props: {
        idxRow: {
            type: Number,
            default: null
        },
        bodyRow: {
            type: Object,
            default: {}
        },
        propList: Array,
        cssClass: Object
    },
    template: `
        <ul :class="[cssClass?.listCtr,'e-list']">
            <template v-for="(list, idx) in propList">
                <li
                    :class="[cssClass?.listElm,'e-list--elm']"
                    @click="clickAction(list, idxRow, bodyRow)"
                >
                    <slot name="prepend">
                        <i
                            v-if="list.prependIcon"
                            :class="[list.prependIcon,'e-list--prepend-icon e-list--icon']"
                        />
                    </slot>
                    <span 
                        :title="list?.name"
                        :class="[cssClass?.listVal,'e-list--value text-truncate']"
                    >{{list?.name}}</span>
                    <slot name="append">
                        <i v-if="list.appendIcon" :class="[list.appendIcon,'e-list--append-icon e-list--icon']"></i>
                    </slot>
                </li>
            </template>
        </ul>
`
};