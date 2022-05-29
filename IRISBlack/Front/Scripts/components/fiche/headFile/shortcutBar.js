import { showVCardOrMiniFile } from '../../../methods/eComponentsMethods.js?ver=803000';
import { FieldType } from "../../../methods/Enum.min.js?ver=803000";
import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000';


export default {
    name: "shortcutBar",
    mixins: [eFileComponentsMixin],
    data() {
        return {
            FieldType,
            dataInput:{}
        }
    },
    components: {
        eMenuSpeedDial: () => import(AddUrlTimeStampJS("../../subComponents/eMenuSpeedDial.js")),
    },
    methods: {
        showVCardOrMiniFile,
        /**
         * Affiche ou masque la mini-fiche
         * @param {object} event Evènement déclencheur
         * @param {boolean} show true pour afficher la minifiche, false pour la masquer
         * @param {object} icon
         * */
        showMiniCard($event, status, button) {
            this.dataInput = button?.Fields?.[0];             
            this.showVCardOrMiniFile($event, status);
        },
        /**
        * On récupère la linkId du relation
        * @param {object} icon
        */
        LnkId(avatar) {
            return avatar?.FileId;
        },
        /**
        * On récupère la icone du bouton.
        * @param {String} icon
        */
        getAvatarIcon (icon) {
            if (icon == 'icon-phone') {
                return 'mdi mdi-phone';
            }
            return icon;
        },
        downloadMiniCard(avatar) {
            window.open('mgr/eVCardManager.ashx?vc=' + avatar.EncryptedLink, '_blank');
        }
    },
    props: {
        propsAvatars: {
            type: Array,
            default: []
        }
    },
    template:
        `
<v-row>
    <v-col
        class="pa-0 shortcut d-inline-flex justify-xs-start justify-sm-end align-center"
    >
        <div
            v-for="(avatar, id) in propsAvatars"
            :key="id"
            v-if="avatar?.Fields.length"
            :ref="'shortcut-' + avatar?.Fields?.[0]?.DescId"
            class="shortcut-avatar"
            :id="avatar.Icon"
        >   
            <eMenuSpeedDial
                class="shortcut-list"
                :oTblItem="avatar?.Fields?.[0]"
                :hover="true"
            >
                <template v-slot:button>
                    <v-btn
                        v-if="avatar?.Icon != 'icon-vcard'"
                        fab
                        dark
                        depressed
                        color="primary"
                        :width="avatar?.IconSizes?.btnSize"
                        :height="avatar?.IconSizes?.btnSize"
                        :class="'shortcut-' + avatar?.Icon"
                        v-on="avatar?.Button?.length == 1 ? { click: () => avatar?.Button[0].action() } : {}"
                    >
                        <v-tooltip 
                            color="rgb(0,0,0,.8)" 
                            :nudgeTop="150" 
                            :left="avatar?.tooltipPosition == 'left'"
                            :bottom="avatar?.tooltipPosition == 'bottom'"
                            :disabled="!avatar?.message"
                        >
                        <template v-slot:activator="{ on, attrs }">
                            <v-avatar
                                :size="24"
                                v-bind="attrs"
                                v-on="on"
                            >
                                <v-icon
                                    dark
                                    :size="avatar?.IconSizes?.iconSize"
                                >{{ avatar?.IconSizes?.icon }}</v-icon>
                            </v-avatar>
                        </template>
                            <span>{{avatar?.message}}</span>
                        </v-tooltip>
                    </v-btn>
                    <v-btn
                        v-else
                        fab
                        dark
                        depressed
                        color="primary"
                        :width="avatar.IconSizes.btnSize"
                        :height="avatar.IconSizes.btnSize"
                        :class="'shortcut-' + avatar?.Icon"
                        @mouseover="showMiniCard($event, true, avatar)"
                        @mouseout="showMiniCard($event, false, avatar)"
                        @click="downloadMiniCard(avatar)"
                    >
                        <v-avatar
                            size="24"
                        >
                            <v-icon
                                dark
                                size="14"
                                :lnkid="LnkId(avatar?.Fields?.[0])"
                            >{{ avatar?.IconSizes?.icon }}</v-icon>
                        </v-avatar>
                    </v-btn>
                </template>
                <template v-slot:menu>
                    <v-list
                        v-if="avatar?.Icon != 'icon-vcard' && avatar?.Button?.length != 1"
                        class="pa-0"
                        min-height="40"
                        min-width="300"
                    >
                        <template  
                        v-for="(button, btnId) in avatar?.Button" 
                        >
                            <v-subheader 
                                v-if="button?.Label"
                                :key="btnId"
                            >
                                {{button?.Label}}
                            </v-subheader>
                            <v-list-item
                                v-else
                                @click="button.action()"
                            >
                                <v-list-item-title>{{button?.text}}</v-list-item-title>
                            </v-list-item>
                        </template>
                    </v-list>
                </template>
            </eMenuSpeedDial>
        </div>
    </v-col>
</v-row>
`
}