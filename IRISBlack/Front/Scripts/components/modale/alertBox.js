import { typeAlert } from '../../methods/Enum.min.js?ver=803000'

export default {
    name: "eAlertBox",
    data:function() {
        return {
            typeAlert,
        }
    },
    props: ["bSuccess", "warning", "multiple", "cssClass", "cssStyle", "typeCssAlert"],
    computed: {
        alertClass() {
            let cssClass;

            /** On force le css, plutot que de laisser la modale décider. */
            if (this.typeCssAlert) {
                switch (this.typeCssAlert) {
                    case typeAlert.Error: return 'alert-danger';
                    case typeAlert.Success: return 'alert-danger';
                    case typeAlert.Warning: return 'alert-warning';
                    case typeAlert.Standard: return '';
                    default: ;
                }
            }

            if (this.bSuccess)
                cssClass = 'alert-success'
            else if (this.warning)
                cssClass = 'alert-warning'
            else
                cssClass = 'alert-danger'

            return cssClass;
        }
    },
    methods: {},
    template: `
    <div :class="['eAlertBox', alertClass, cssClass]" :style="cssStyle">
        <template v-if="!multiple" name="fade">
            <slot></slot>
        </template>
        <template v-else>
            <span :class="span.class" v-for="(span,idx) in multiple">{{span.value}}</span>
        </template>
    </div>
`
};