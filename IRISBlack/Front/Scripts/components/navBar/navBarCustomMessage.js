export default {
    name: "navBarCustomMessage",
    data() {
        return {
            messagesData: {
                countMessageNoRead: 5,
                messages: [{
                    id: "msg1",
                    name: "Hugo",
                    time: "5 mins",
                    avatar: "imgs/avatar5.png",
                    inner: `Inter has ruinarum varietates a Nisibi quam tuebatur accitus 
                    Vrsicinus,cui nos obsecuturos iunxerat imperiale praeceptum, 
                    dispicere litis exitialis certamina cogebatur abnuens et reclamans, 
                    adulatorum oblatrantibus turmis, bellicosus sane milesque semper et militum ductor sed 
                    forensibus iurgiis longe discretus, qui metu sui discriminis anxius cum accusatores 
                    quaesitoresque subditivos sibi consociatos ex isdem foveis cerneret emergentes, 
                    quae clam palamve agitabantur, occultis Constantium litteris edocebat inplorans subsidia,
                    quorum metu tumor notissimus Caesaris exhalaret.`
                }, {
                    id: "msg2",
                    name: "BELKACEM",
                    time: "22 mins",
                    avatar: "imgs/avatar.png",
                    inner: `Aetatem stipite qui adscitus feceris.`
                }, {
                    id: "msg3",
                    name: "KAREN",
                    time: "34 mins",
                    avatar: "imgs/avatar2.png",
                    inner: `Subiratus abscessit cessaveris ultimum sciens abscessit 
                    ambage et iubebo solo Caesar eius ut palatii praegressa cessaveris subiratus prope si et.`
                }, {
                    id: "msg4",
                    name: "CELINE",
                    time: "2 heures",
                    avatar: "imgs/avatar3.png",
                    inner: `Ne hostibus non etiamsi vos.`
                }, {
                    id: "msg5",
                    name: "MAXIME",
                    time: "3 heures",
                    avatar: "imgs/avatar4.png",
                    inner: `Nihil est enim virtute amabilius, nihil quod magis adliciat ad 
                    diligendum, quippe cum propter virtutem et probitatem etiam eos, quos 
                    numquam vidimus, quodam modo diligamus. Quis est qui C. Fabrici, M'.
                     Curi non cum caritate aliqua benevola memoriam usurpet, quos numquam 
                     viderit? quis autem est, qui Tarquinium Superbum, qui Sp. Cassium, Sp. 
                     Maelium non oderit? Cum duobus ducibus de imperio in Italia est decertatum, Pyrrho et Hannibale; 
                    ab altero propter probitatem eius non nimis alienos animos habemus, 
                    alterum propter crudelitatem semper haec civitas oderit.`
                }]
            },

        };
    },
    template: `
    <li ref="messagesBtn" class="dropdown messages-menu">
        <a href="#" class="dropdown-toggle" data-toggle="dropdown" aria-expanded="false">
            <i class="fa fa-envelope-o"></i>
            <span class="label label-success">{{messagesData.countMessageNoRead}}</span>
        </a>
        <ul class="dropdown-menu">
            <li v-if="messagesData.countMessageNoRead > 1" class="header">Vous avez {{messagesData.countMessageNoRead}} nouveaux messages</li>
            <li v-else class="header">Vous avez {{messagesData.countMessageNoRead}} nouveau message</li>
            <li>
                <ul class="menu">
                    <li v-for="message in messagesData.messages" :key="message.id">
                        <a href="#">
                            <div class="pull-left">
                                <img :src="message.avatar" class="img-circle" alt="User Image">
                            </div>
                            <h4>{{message.name}}
                                <small><i class="fa fa-clock-o"> </i> {{message.time}}</small>
                            </h4>
                            <p class="textInnerMessagerie">{{message.inner}}</p>
                        </a>
                    </li>
                </ul>
            </li>
            <li class="footer"><a href="#">Voir tous les messages</a></li>
        </ul>
    </li>
`
}