import { store } from '../../../store/store.js?ver=803000';
import vuetify from '../../../plugins/Vuetify.js?ver=803000';


export function initVue(oProps) {

    Vue.use(eudoFront.default);
    Vue.use(Vuex);

    const App_edn_home = () => import(AddUrlTimeStampJS("../ednHomeMainApp.js"));

    return new Vue({
        vuetify,
        store,
        render: h => h(App_edn_home, {
            props: {
                oProps: oProps
            }
        }),
    }).$mount('#app_edn_home');
}