export const eWorkflowMixin = {
    data() {
        return {
        }
    },
    components: {
    },
    created() {
    },
    computed: {
    },
    methods: {
        closeModal(cmapignId) {
            if ((this.$store.state.Datas != null && this.$store.state.Datas.html != '' && !flowy.output()
                || this.$store.state.Datas != null && this.$store.state.Datas.html != '' && this.$store.state.Datas.html.replace('<div class="indicator invisible"></div>', '').replace(' selectedblock', '') != flowy.output().html.replace('<div class="indicator invisible"></div>', '').replace(' selectedblock', ''))
                || (this.$store.state.Datas && this.$store.state.Datas.html == '' && flowy.output() && flowy.output().html && flowy.output().html != '')
                || this.$store.state.Label != this.$store.state.TmpLabel)
                this.$store.commit("setShowDialog", true);
            else {
                if (cmapignId)
                    top.loadFile(106000, cmapignId);
                closeWorkflowModal();
            }

        },
    }
}