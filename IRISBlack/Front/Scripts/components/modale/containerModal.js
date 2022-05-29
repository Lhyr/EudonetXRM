import { eModalMixin } from '../../mixins/eModalMixin.js?ver=803000';

export default {
    name: "containerModal",
    data() {
        return {
            modalAlert: false,
            showMotherModal: false
        };
    },
    components: {
        MotherOfAllModals: () => import(AddUrlTimeStampJS("./MotherOfAllModals.js")),
        eDropdown: () => import(AddUrlTimeStampJS("../subComponents/eDropdown.js")),
        eLinksOptions: () => import(AddUrlTimeStampJS("../subComponents/eLinksOptions.js")),
        eAlertBox: () => import(AddUrlTimeStampJS("./alertBox.js"))
    },
    computed: {
        displayDropDown: function () { return this.propOptionsModal.main.dropdown && this.propOptionsModal.main.dropdown.opt && this.propOptionsModal.main.dropdown.opt.length > 1; },
        displayLinks: function () { return this.propOptionsModal.main.links && this.propOptionsModal.main.links.length > 1; },
        showIcon: function () { return this.propOptionsModal.main.icon && this.propOptionsModal.main.icon.length > 1; }
    },
    props: ['propOptionsModal'],
    mixins: [eModalMixin],
    template: `
    <div class="containerModal">
        <MotherOfAllModals :prop-options-modal="propOptionsModal">
            <template v-slot:header>
                <div :class="propOptionsModal.main.componentsClass">
                    <span v-if="showIcon" :class="[propOptionsModal.main.icon, 'modal-icon']"></span>
                    <h3 tabindex="0" ref="relationtitle" class="relation-title">{{propOptionsModal.main.title}}</h3>
                    <eDropdown v-if="displayDropDown" :prop-dropdown="propOptionsModal.main.dropdown" :handle-outside-click="true" >
                    </eDropdown>
                    <eLinksOptions v-if="displayLinks" :prop-links="propOptionsModal.main.links"></eLinksOptions>
 		            <eAlertBox :multiple="propOptionsModal.main.alert" :warning="true" v-if="modalAlert" >
		            </eAlertBox>
                </div>
            </template>
        </MotherOfAllModals>
    </div>
`
};