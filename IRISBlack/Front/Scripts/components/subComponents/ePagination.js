export default {
    name: "ePagination",
    data() {
        return {};
    },
    methods: {
        /**
         * l'action a effectuer quand on clique sur next, prev ou enter (input).
         * @param {any} event
         */
        actions: function (event) {
            this.$emit("actions", event)
        }
    },
    props: { propFile: Number, porpNbFiles:Number },
    template: `
    <div class="paginationIncrustedFile">
        <button @click="actions('prev')" class="buttonPaginationIncrustedFile prev" type="button" aria-label="Precedent" :disabled="propFile < 2">
            <i class="fas fa-chevron-left"></i>
        </button>
        <input class="inputPaginationIncrustedFile" ref="searchFileValue" :value="propFile" @keyup.enter="actions('searchFile')">
        <span class="txtPaginationIncrustedFile"> / </span>
        <span class="txtPaginationIncrustedFile">{{porpNbFiles}}</span>
        <button @click="actions('next')" class="buttonPaginationIncrustedFile next" type="button" aria-label="Suivant" :disabled="propFile == porpNbFiles">
            <i class="fas fa-chevron-right"></i>
        </button>
    </div>
`
};