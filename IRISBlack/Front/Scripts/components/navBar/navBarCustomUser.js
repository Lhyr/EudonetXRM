export default {
    name: "navBarCustomUser",
    data() {
        return {
            mobileActive: true,
            user: {
                name: 'Quentin Bolatre',
                avatar: 'imgs/avatar5.png',
                post: 'Développeur UI/UX',
                memberTime: 'Jan. 2017'
            },
        };
    },

    mounted() {
        this.$emit('init-custom-user', true)
        window.addEventListener('resize', this.getMobile);
        this.getMobile();
    },
    beforeDestroy() {
        window.removeEventListener('resize', this.getMobile);
    },
    methods: {
        getMobile() {
            if (window.innerWidth > 450) {
                this.mobileActive = true
            } else {
                this.mobileActive = false
            }
        }
    },
    template: `
    <li ref="userBtn" class="dropdown user user-menu">
        <a href="#" class="dropdown-toggle" data-toggle="dropdown">
            <img :src="user.avatar" class="user-image" alt="User Image">
            <span v-if="mobileActive" class="hidden-xs">{{user.name}}</span>
        </a>
        <ul class="dropdown-menu">
            <li class="user-header">
                <img src="imgs/avatar5.png" class="img-circle" alt="User Image">
                <p>{{user.name}} - {{user.post}}<small>Membre depuis : {{user.memberTime}}</small></p>
            </li>
            <li class="user-body">
                <div class="row">
                    <div class="col-xs-6 text-center">
                        <a href="#">Mon Eudonet</a>
                    </div>
                    <div class="col-xs-6 text-center">
                        <a href="#">Extranet</a>
                    </div>
                </div>
            </li>
            <li class="user-footer">
                <div class="pull-left">
                    <a href="#" class="btn btn-default">Se déconnecter</a>
                </div>
                <div class="pull-right">
                    <a href="#" class="btn btn-default admin-btn">Adminitrateur</a>
                </div>
            </li>
        </ul>
    </li>
`
}