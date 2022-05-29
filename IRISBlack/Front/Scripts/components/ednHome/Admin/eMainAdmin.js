import { store } from '../../../store/store.js?ver=803000';
import vuetify from '../../../plugins/Vuetify.js?ver=803000';

export function initVue(oProps) {

    Vue.use(eudoFront.default);
    Vue.use(Vuex);

    const app_menu = () => import(AddUrlTimeStampJS("./endSubMenuAdmin.js"));

    return new Vue({
        vuetify,
        store,
        render: h => h(app_menu, {
            props: {
                oProps: oProps
            }
        }),
    }).$mount('#app-menu');
}