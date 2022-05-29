export default {
    name: "navBarCustomAlert",
    data() {
        return {
            alertsData: {
                countAlert: 3,
                alerts: [{
                    id: "alert1",
                    type: "graph-jauge",
                    time: "2 mins",
                    important: "info",
                    resultGraph: "20",
                    title: 'Rapport d\'utilisateur enregistré par jours',
                    link: '#'

                }, {
                    id: "alert2",
                    type: "user",
                    time: "52 mins",
                    important: "success",
                    name: "ADMINISTRATEUR",
                    msg: 'Votre nom d\'utilisateur a était changer par <a href="#">ADMINISTRATEUR</a>'
                }, {
                    id: "alert3",
                    type: "user",
                    time: "1 heure",
                    important: "warning",
                    name: "HUGO",
                    msg: 'La demande <a href="#">[E2017 Ergonomie] Font Size panel d\'administration</a> a été ré-ouverte par  HUGO'
                }]
            },

        };
    },
    template: `
    <li ref="alertBtn" class="dropdown notifications-menu messages-menu">
        <a href="#" class="dropdown-toggle" data-toggle="dropdown" aria-expanded="false">
            <i class="fa fa-bell-o"></i>
            <span class="label label-warning">{{alertsData.countAlert}}</span>
        </a>
        <ul class="dropdown-menu">
            <li v-if="alertsData.countAlert > 1" class="header">Vous avez {{alertsData.countAlert}} notifications</li>
            <li v-else class="header">Vous avez {{alertsData.countAlert}} notification</li>
            <li>
                <ul class="menu">
                    <li v-bind:class="{ 'notificationWarning': alert.important == 'warning', 'notificationInfo': alert.important == 'info', 'notificationSuccess': alert.important == 'success'}" v-for="alert in alertsData.alerts" :key="alert.id">
                        <div v-if="alert.type == 'graph-jauge'">
                            <i style="float:left" v-bind:class="{ 'fa-exclamation-circle text-yellow': alert.important == 'warning', 'fa-info-circle text-aqua': alert.important == 'info', 'fa-check-circle text-green': alert.important == 'success'}" class="fas"></i>
                            <h4>GRAPHIQUE
                                <small><i class="fa fa-clock-o"> </i> {{alert.time}}</small>
                            </h4>
                            <small class="pull-right">{{alert.resultGraph}}%</small>
                            <div class="progress xs">
                                <div v-bind:class="{ 'progress-bar-aqua': alert.important == 'info', 'progress-bar-green': alert.important == 'success','progress-bar-warning': alert.important == 'warning'}" class="progress-bar" v-bind:style="{width: alert.resultGraph + '%' }"  role="progressbar" :aria-valuenow="alert.resultGraph" aria-valuemin="0" aria-valuemax="100">
                                    <span class="sr-only">{{alert.resultGraph}}% Complete</span>
                                </div>
                            </div>
                            <p class="textInnerMessagerie"><a :href="alert.link">{{alert.title}}</a></p>
                        </div>
                        <div v-if="alert.type == 'user'">
                            <i style="float:left" v-bind:class="{ 'fa-exclamation-circle text-yellow': alert.important == 'warning', 'fa-info-circle text-aqua': alert.important == 'info', 'fa-check-circle text-green': alert.important == 'success'}" class="fas"></i>
                            <h4>{{alert.name}}
                                <small><i class="fa fa-clock-o"> </i> {{alert.time}}</small>
                            </h4>
                            <p class="textInnerMessagerie" v-html="alert.msg"></p>
                        </div> 
                    </li>
                </ul>
            </li>
            <li class="footer"><a href="#">Tous voir</a></li>
        </ul>
    </li>
`
}