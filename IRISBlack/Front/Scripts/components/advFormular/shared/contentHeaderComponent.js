
export default {
    name: "contentHeader",
    props: ["title", "subtitle"],

    computed: {
        getRes() { return function (resid) { return this.$store.getters.getRes(resid) } }
    },

    template: `    
        <h1>{{  getRes(title) }}
        <small>{{ getRes(subtitle) }}</small></h1>
`,

};