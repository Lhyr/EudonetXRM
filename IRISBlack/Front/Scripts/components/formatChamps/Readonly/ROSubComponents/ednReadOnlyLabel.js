import { eFileComponentsMixin } from '../../../../mixins/eFileComponentsMixin.js?ver=803000';

export default {
    name: "ednReadOnlyLabel",
    mixins: [eFileComponentsMixin],
    props: { oTblItm: Object },
    template: `
    <span>
        <slot></slot>
    </span>
`
};