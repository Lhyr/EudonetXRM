import { state, getters, mutations, actions } from './IrisBlackStoreFeatures.js?ver=803000'
/**
 * Le store pour stocker les éléments accessibles de partout.
 * */
Vue.use(Vuex);
export const store = new Vuex.Store({
    state: state,
    getters: getters,
    mutations: mutations,
    actions: actions,
});
