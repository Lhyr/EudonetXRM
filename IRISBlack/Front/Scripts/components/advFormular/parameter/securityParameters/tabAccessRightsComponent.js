//tâche #3 341 création de composant Droits d'accès
export default {
    name: "AccessRights",

    data() {
        return {
            };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
        Permission: () => import("./ePermissionComponent.js"),
    },

    mounted() {
    },
    computed: {
        getRes() {
            return function (resid) {
                return this.$store.getters.getRes(resid);
            };
        },
        //Libellés
        getPublicFormularLabel() {
            return this.$store.getters.getRes(2710);
        },

        getViewPermLabel() {
            return this.$store.getters.getRes(1033);
        },

        getUpdatePermLabel() {
            return this.$store.getters.getRes(1034);
        },

        getLevelAccessLabel() {
            return this.$store.getters.getRes(249);
        },

        getUserAccessLabel() {
            return this.$store.getters.getRes(250);
        },
        //Formulaire public ou non
        publicFormular: {
            get() {
                return this.$store.state.PublicFormular;
            },
            set(publicFormular) {
                this.$store.commit("setPublicFormular", publicFormular);
            },
        }
    },
    methods: {
        //MAJ le mode de permission dans le store
        updatePermMode(item) {
            switch (item.permType) {
                case 'VIEW':
                    this.$store.commit("setViewPermMode", item.permMode);
                    break;
                case 'UPDATE':
                    this.$store.commit("setUpdatePermMode", item.permMode);
                    break;
                default: break;
            }
        },
        //MAJ le niveau d'accès dans le store
        updateLevelAccess(item) {
            switch (item.permType) {
                case 'VIEW':
                    this.$store.commit("setViewPermLevel", item.permLevelAccess);
                    break;
                case 'UPDATE':
                    this.$store.commit("setUpdatePermLevel", item.permLevelAccess);
                    break;
                default: break;
            }
        },
        //MAJ la liste des users autorisés dans le store
        updateUserAccess(item) {
            if (this.$store.getters.getPermDescId(item.permType) != item.options.DescId)
                return;
            switch (item.permType) {
                case 'VIEW':
                    this.$store.commit("setViewPermUser", item.options.NewValue);
                    break;
                case 'UPDATE':
                    this.$store.commit("setUpdatePermUser", item.options.NewValue);
                    break;
                default: break;
            }
        },
        updateShowPerm(item) {
            switch (item.permType) {
                case 'VIEW':
                    this.$store.commit("setShowViewPerm", item.showPerm);
                    break;
                case 'UPDATE':
                    this.$store.commit("setShowUpdatePerm", item.showPerm);
                    break;
                default: break;
            }
        }
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
        
            <edn-check :label="getPublicFormularLabel" v-model="publicFormular"/>

            <Permission :permLabel="getViewPermLabel" :permType="'VIEW'" 
            :userAccessLabel=getUserAccessLabel @updatePermMode="updatePermMode" @updateLevelAccess="updateLevelAccess" @updateUserAccess="updateUserAccess" :levelAccessLabel=getLevelAccessLabel @updateShowPerm="updateShowPerm" />

            <Permission :permLabel="getUpdatePermLabel" :permType="'UPDATE'" 
            :userAccessLabel=getUserAccessLabel @updatePermMode="updatePermMode" @updateLevelAccess="updateLevelAccess" @updateUserAccess="updateUserAccess" :levelAccessLabel=getLevelAccessLabel @updateShowPerm="updateShowPerm" />

        </section>
    </div>
`,

};