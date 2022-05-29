const navBarCustomMessage = () =>
    import(AddUrlTimeStampJS("./navBarCustomMessage.js"))

const navBarCustomAlert = () =>
    import(AddUrlTimeStampJS("./navBarCustomAlert.js"))


const navBarCustomWarning = () =>
    import(AddUrlTimeStampJS("./navBarCustomWarning.js"))


const navBarCustomUser = () =>
    import(AddUrlTimeStampJS("./navBarCustomUser.js"))

export default {
    name: "navBarCustom",
    data() {
        return {
            btnAddActive: false
        };
    },
    components: {
        navBarCustomMessage,
        navBarCustomAlert,
        navBarCustomWarning,
        navBarCustomUser
    },
    mounted() {
        window.addEventListener('resize', this.getaddBtn);
        this.getaddBtn();
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.getaddBtn);
    },
    methods: {
        initCustomUser(e) {
            this.$emit('init-navbar-custom', e)
        },
        getaddBtn() {
            let addBtn = document.getElementById("navLi");
            if (addBtn == null && window.innerWidth > 768 || addBtn == undefined && window.innerWidth > 768) {
                this.btnAddActive = true
            } else {
                this.btnAddActive = false
            }
        }
    },
    template: `
    <div ref="navBarCustom" id="navBarCustom" class="navbar-custom-menu">
        <ul class="nav navbar-nav">
            <li v-if="this.btnAddActive" id="navLiResponsive" class="dropdown rubonNav">
                <a id="addMenu" href="#" class="dropbtn">
                    <i class="fa fa-plus"></i>
                </a>
                <div class="dropdown-content" role="menu">
                    <a href="#" data-toggle="modal" data-target="#modal-wizard-choix-onglets">Choix des onglets</a>
                    <a class="addOnglet" href="#">Nouvelle vue</a>
                </div>
            </li>
            <navBarCustomMessage ></navBarCustomMessage>
            <navBarCustomAlert></navBarCustomAlert>
            <navBarCustomWarning></navBarCustomWarning>
            <navBarCustomUser @init-custom-user="initCustomUser"></navBarCustomUser>
            <li>
                <a id="toggle" href="#" data-toggle="control-sidebar"><i class="fa fa-gears"></i></a>
            </li>
        </ul>
    </div>
`
}