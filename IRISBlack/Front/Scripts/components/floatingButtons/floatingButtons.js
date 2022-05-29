export default {
    name: "floatingButtons",
    data() {
        return {
            alignItem: -500,
            widthIcon: 31
        }
    },
    computed: {
        /** Computed qui retourne les class. */
        getClass: function () {
            return this.propsFloatingButton?.align == 'right' ? 'floatingRight' : 'floatingLeft';
        },

        /** Computed qui retourne les css. */
        getStyle: function () {
            this.getWidth();
            return {
                zIndex: this.propsFloatingButton?.zIndex || "6",
                bottom: this.propsFloatingButton?.alignVertical || "30vh",
                right: this.propsFloatingButton?.align == 'right' ? this.alignItem + 'px': '',
                left: this.propsFloatingButton?.align == 'left' ? this.alignItem + 'px': ''
            }
        },
        /** Computed iris est activé ou pas, si c'est e17, isIris not existé*/
        checkIris: function () {
            return typeof isIris == 'function';
        }
    },
    methods: {
        /** Methods qui permet de calculer la width des boutons peut importe la langue  */
        getWidth: function () {
            this.$nextTick(() => {
                var left = this.$refs.btnGroupSettings.clientWidth - this.widthIcon;
                if (!isIris(this.propTab)) {
                    // Demande 95059
                    // Quand on est pas sur le nouveau mode fiche seulement
                    // Solution de contournement lié au déchargement/rechargement du css
                    setTimeout(() => {
                        this.alignItem = left *= -1;
                    }, 1000);
                } else {
                    this.alignItem = left *= -1;
                }
            });
        },

        /** Renvoie l'action au parent  */
        action: function (buttonSetting) {
            this.$emit('callback', buttonSetting)
        }
    },
    props: ['propsFloatingButton', 'propTab'],
    template: `
    <div 
        v-if="checkIris"
        ref="btnGroupSettings" 
        v-bind:style="getStyle" 
        :class="getClass"
        class="btn-group-settings"
    >
        <div class="flex-column d-flex">
            <v-btn v-if="!button.disabled" @click="action(button)" v-for="(button, idx) in propsFloatingButton.actions" :key="idx" :color="button.colorBtn" :class="[button.sClassBtn, 'justify-end']" tile>
                <v-icon v-if="propsFloatingButton?.align == 'right'" :size="button.sizeIcon" left dark> {{button.icon}}</v-icon>
                {{button.text}}
                <v-icon v-if="propsFloatingButton?.align == 'left'" :size="button.sizeIcon" right dark> {{button.icon}}</v-icon>
            </v-btn>
        </div> 
    </div>
`
}