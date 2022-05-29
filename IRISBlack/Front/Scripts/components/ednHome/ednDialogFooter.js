import { eDialogMixin } from '../../mixins/eDialogMixin.js?ver=803000';

export default {
    name: "ednDialogFooter",
    mixins: [eDialogMixin],
    data() {
        return {
            sColor: "rgba(0, 0, 0, 0)",
            sBtnText: "",
        };
    },
    components: {
        ednDialogButton: () => import("./ednDialog/ednDialogButton.js"),
        ednDialogToolBar: () => import("./ednDialog/ednDialogToolBar.js"),
    },
    props: {
        oProps:Object,
    },   
    computed: {
        /** retourne une props pour le bouton. */
        getPropsButton: function () {
            return {
                bLight: false,
                sClass: "text-none",
                sColor: "rgba(ff, ff, ff, 0.6)",
                bTile: true,
                bLight: true,
                bRight: true,
            }
        },

        /** props pour la toolbar. */
        getToolBarProps: function () {
            return {
                sColor: this.sColor,
                sClass: "elevation-0 px-10",
            }
        }
    },
    created() {
        this.sColor = this.oProps?.sColor || this.sColor;
        this.sBtnText = this.oProps?.sBtnText || this.sBtnText;
    },
    template: `
        <div class="edn--home--dialog--footer">
            <v-card-actions :color="sColor" class="pb-10">
                <ednDialogToolBar :oProps="getToolBarProps">
                    <v-spacer></v-spacer>
                    <ednDialogButton :oProps="getPropsButton" @action="action">{{sBtnText}}</ednDialogButton>
                </ednDialogToolBar>
            </v-card-actions>
            </v-card>
        </div>                          
`,
};