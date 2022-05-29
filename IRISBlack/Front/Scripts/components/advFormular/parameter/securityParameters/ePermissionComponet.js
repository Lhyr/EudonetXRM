//tâche #3 341 création de composant Droits d'accès par prmission
import EventBus from '../../../../bus/event-bus.js';
export default {
    name: "Permission",
    data() {
        return {
            showPermComponent: false,//Afficher les accès par niveau ou pas users
            showPermLevelAccess: false,//Afficher les liste des niveaux
            showPermUserAccess: false,//Aficher le composant eUser
            input://model du composant eUser
            {
                "DisplayValue": this.$store.getters.getPermUserDisplayValue(this.permType),
                "Value": this.$store.getters.getPermUserValue(this.permType),
                "ReadOnly": false, "FileId": 0, "DescId": this.$store.getters.getPermDescId(this.permType), "IsVisible": true, "Multiple": true, "FullUserList": true, "Format": 25, "ToolTipText": "", "Width": "100"
            }
        };
    },
    
    components: {
        eUser: () => import("../../../formatChamps/eUser.js"),
    },

    mounted() {
        //on déclenche un évennement 'updateUserAccess' après la  modification du composant eUser
        //cet évennement sera intercetpté par le composant parent
        EventBus.$on('valueEdited', (options) => {
            this.$emit('updateUserAccess', {
                permType: this.permType,
                options: options
            })
        });
    },
    computed: {
        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },
        //Retourne le type de permission (Visualisation, modification...)
        getPermMode() {
            return function (permType) {
                return this.$store.getters.getPermMode(permType);
            };
        }, 
        //retourne le niveau de permission
        getPermLevel() {
            return function (permType) {
                return this.$store.getters.getPermLevel(permType);
            };
        }, 
        //v-model: Affiche les droits d'accès par niveau et par users
        showPerm: {
            get() {
                let _showPerm = this.showPermLevelAccess || this.showPermUserAccess;
                this.showPermComponent = _showPerm;
                return _showPerm;
            },
            set(_showPerm) {
                if (!_showPerm) {
                    this.showPermLevelAccess = this.showPermUserAccess = false;
                    this.SetPermMode();
                }
                this.showPermComponent = _showPerm;
            },
        },
        //v-model: droits d'accès par niveau d'utilisateur
        permLevelAccess: {
            get() {
                let _showPermLevelAccess = this.getPermMode(this.permType) == 0 || this.getPermMode(this.permType) == 3;
                this.showPermLevelAccess = _showPermLevelAccess;
                return _showPermLevelAccess;
            },
            set(_showPermLevelAccess) {
                this.showPermLevelAccess = _showPermLevelAccess;
                this.SetPermMode();
            },
        },
        //v-model: droits d'accès par users
        permUserAccess: {
            get() {
                let _showPermUserAccess = this.getPermMode(this.permType) == 1 || this.getPermMode(this.permType) == 3;
                this.showPermUserAccess = _showPermUserAccess;
                return _showPermUserAccess;
            },
            set(_showPermUserAccess) {
                this.showPermUserAccess = _showPermUserAccess;
                this.SetPermMode();
            },
        },
        //remplit la liste déroulante des niveaux
        optPermLevelAccess() {
            that = this;
            for (var i = 1; i <= 5; i++) {
                that.$store.state.AllAvailableLevels[i.toString()] = i.toString();
            }
            that.$store.state.AllAvailableLevels["99"] = that.$store.getters.getRes(194, '');
            return {
                optionSelected: that.getPermLevel(that.permType).toString(),
                opt: Object.keys(that.$store.state.AllAvailableLevels).map(function (key) {
                    return { name: that.$store.state.AllAvailableLevels[key], value: key };
                }),
            };
        },
        //Niveau de user choisit
        permLevelAccessId: {
            get() {
                return this.getPermLevel(this.permType).toString();
            },
            set(_permLevelAccess) {
                //en cas de modif, on déclenche l'événnement de MAJ
                this.$emit('updateLevelAccess', {
                    permType: this.permType,
                    permLevelAccess: _permLevelAccess
                })
            },
        },

    },
    methods: {
        //MAJ le mode de permission
        //MODE_NONE = -1,
        ///// <summary>Niveau seulement</summary>
        //MODE_LEVEL_ONLY = 0,
        ///// <summary>Utilisateur seulement</summary>
        //MODE_USER_ONLY = 1,
        ///// <summary>Utilisateur ou Niveau</summary>
        //MODE_USER_OR_LEVEL = 2,
        ///// <summary>Utilisateur et Niveau</summary>
        //MODE_USER_AND_LEVEL = 3
        SetPermMode() {
            let _permMode = -1;
            if (!this.showPermLevelAccess && !this.showPermUserAccess)
                _permMode = -1;
            else if (this.showPermLevelAccess && !this.showPermUserAccess)
                _permMode = 0;
            else if (!this.showPermLevelAccess && this.showPermUserAccess)
                _permMode = 1;
            if (this.showPermLevelAccess && this.showPermUserAccess)
                _permMode = 3;

            this.$emit('updatePermMode', {
                permMode: _permMode,
                permType: this.permType
            })
        }
    },
    props: ['permLabel', 'permType', 'userAccessLabel', 'levelAccessLabel'],

    template: `
<div>
            <edn-check :label="permLabel" v-model="showPerm"/>
            <div class="ParamLink"  v-show="showPermComponent" >
                 <div class="DivPerm">
                   <div class="CheckPermOpt">
                      <div class="ChkPerm">
                          <edn-check :label="levelAccessLabel" v-model="permLevelAccess"/>
                          <edn-cat v-model="permLevelAccessId" :disabled="!showPermLevelAccess" :items="optPermLevelAccess.opt" item-value="value" item-text="name"/>
                      </div>
                   </div>

                    <div class="CheckPermOpt">
                      <div class="ChkPerm"  style="display:flex">
                        <edn-check class="chkUserFormularAccess" :label="userAccessLabel" v-model="permUserAccess"/>
                        <div class="globalDivComponent userComponent" style="margin-left:20px" v-show="showPermUserAccess" >
                            <eUser :dataInput="input" :propDetail="true" :propUpdateInDatabase="false"/>
                          </div>
                      </div>
                   </div>
                 </div>
            </div>
</div>
`,
};