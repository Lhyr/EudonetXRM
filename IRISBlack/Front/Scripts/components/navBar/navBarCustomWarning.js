export default {
    name: "navBarCustomWarning",
    data() {
        return {
            warningsData: {
                countWarning: 2,
                warnings: [{
                    id: "warning1",
                    type: "graph-jauge",
                    time: "1 min",
                    important: "danger",
                    resultGraph: "90",
                    title: 'Rapport d\'utilisateur enregistré par jours',
                    link: '#'

                }, {
                    id: "warning2",
                    type: "user",
                    time: "18 mins",
                    important: "danger",
                    name: "HUGO",
                    msg: 'La demande <a href="#">[E2017 Ergonomie] Font Size panel d\'administration</a> a été ré-ouverte par  HUGO'
                }]
            },

        };
    },
    template: `
    <li ref="warningBtn" class="dropdown tasks-menu notifications-menu messages-menu">
        <a href="#" class="dropdown-toggle" data-toggle="dropdown" aria-expanded="false">
            <i class="fa fa-flag-o"></i>
            <span class="label label-danger">{{warningsData.countWarning}}</span>
        </a>
        <ul class="dropdown-menu">
            <li v-if="warningsData.countWarning > 1" class="header">Vous avez {{warningsData.countWarning}} notifications</li>
            <li v-else class="header">Vous avez {{warningsData.countWarning}} notification</li>
            <li>
                <ul class="menu">
                    <li class="notificationImportant" v-for="warning in warningsData.warnings" :key="warning.id">
                        <div v-if="warning.type == 'graph-jauge'">
                            <i style="float:left" class="fas fa-times-circle text-red"></i>
                            <h4>GRAPHIQUE
                                <small><i class="fa fa-clock-o"> </i> {{warning.time}}</small>
                            </h4>
                            <small class="pull-right">{{warning.resultGraph}}%</small>
                            <div class="progress xs">
                                <div class="progress-bar progress-bar-danger" v-bind:style="{width: warning.resultGraph + '%' }"  role="progressbar" :aria-valuenow="warning.resultGraph" aria-valuemin="0" aria-valuemax="100">
                                    <span class="sr-only">{{warning.resultGraph}}% Complete</span>
                                </div>
                            </div>
                            <p class="textInnerMessagerie"><a :href="warning.link">{{warning.title}}</a></p>
                        </div>
                        <div v-if="warning.type == 'user'">
                        <i style="float:left" class="fas fa-times-circle text-red"></i>
                            <h4>{{warning.name}}
                                <small><i class="fa fa-clock-o"> </i> {{warning.time}}</small>
                            </h4>
                            <p class="textInnerMessagerie" v-html="warning.msg"></p>
                        </div> 
                    </li>
                </ul>
            </li>
            <li class="footer"><a href="#">Tous voir</a></li>
        </ul>
    </li>
`
}