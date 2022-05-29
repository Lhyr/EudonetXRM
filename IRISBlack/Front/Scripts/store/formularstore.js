const state = {
    TimeStamp : 0
}

const getters = {

}

const actions = {
}

const mutations = { 
    
    setFormularTS(state) {
        state.TimeStamp =  Date.now()
    },

}

 

const formularStore = {

    namespaced: true,

    state,

    actions,
 
    mutations,

    getters

}

export default formularStore