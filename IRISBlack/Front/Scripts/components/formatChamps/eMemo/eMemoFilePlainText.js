import { eMemoMixin } from './eMemoMixin.js?ver=803000'

export default {
    name: "eMemoFilePlainText",
    data() {
        return {};
    },
    mounted() { },
    methods: {},
    props: [],
    mixins: [eMemoMixin],
    template: `        
        <textarea @focus="TextAreaFocus()" @blur="TextAreaBlur($event)" v-if="!dataInput.IsHtml" :disabled="dataInput.ReadOnly" :id="GetComponentId" :readonly="dataInput.ReadOnly" class="textareaRubrique form-control input-line fname">{{ValueToDisplay}}</textarea>
        <div ref="dvToolbar" v-if="!dataInput.IsHtml && !dataInput.ReadOnly" class="ToolsBarCK">
            <div class="cke_chrome cke_float"  role="application">
                <div class="cke_inner">
                    <div class="cke_top" role="presentation">
                        <span class="cke_toolbox" role="group">
                            <span  class="cke_toolbar" role="toolbar">
                                <span class="cke_toolgroup" role="presentation">
                                    <a @click="insertMessage()" class="cke_button cke_button__xrmusermessage cke_button_off" href="#!" :title="getRes(57)" role="button">
                                        <span class="cke_button_icon fas fa-user">&nbsp;</span>
                                    </a>
                                    <a  @click="openMemo()" class="cke_button cke_button__xrmfullscreendialog cke_button_off" :title="getRes(6602)" role="button">
                                        <span class="cke_button_icon cke_button__xrmfullscreendialog_icon fas fa-expand">&nbsp;</span> 
                                    </a>
                                </span>
                            </span>
                        </span>
                    </div>
                </div>
            </div>
        </div>
        <!-- Message d'erreur après la saisie dans le champs -->
        <span v-show="!bShowOnFocus" v-if="!cptFromModal" class="polePositionFullScreen fas fa-expand" @click="openMemo()"></span>
`
};
