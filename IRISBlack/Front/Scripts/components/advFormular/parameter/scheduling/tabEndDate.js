export default {
    name: "tabEndDate",
    data() {
        return {
            MsgDateEnd: this.$store.state.MsgDateEnd
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
        memoEditor: () => import("../../shared/memoEditorComponent.js")
    },
    computed: {
        setMutationNameMethodForEnd() {
            return 'setMsgDateEnd';
        },
        getExpirationDate: {
            get() {
                return this.$store.state.ExpireDate;
            },
            set(expireDate) {
                this.$store.commit("setExpireDate", expireDate);
            }
        },
        getMergeFields() {
            return this.$store.state.mergeFieldsWithoutExtended;
        },
        getHyperLinksMergeFields() {
            return this.$store.state.hyperLinksMergeFields;
        },
        //Date et heure de fin
        getExpirationDateTimeLabel() {
            return this.$store.getters.getRes(2741);
        }
    },
    props: ["dataTab"],
    template: `
    <div class="moduleContent EndDate" >
        <section class="content-header">
            <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
            <div>
                <edn-date-time :label="getExpirationDateTimeLabel" 
                 v-model="getExpirationDate" placeholder="dd/MM/yyyy" format="dd/MM/yyyy"/>
            </div>
            <div id="Enddate" class="ememodate formularAdvMemo"  >
                <memoEditor v-model="MsgDateEnd" :idMemo="'DateFinMemoId'"
                :mutationMethod="setMutationNameMethodForEnd"  :hyperLinksMergeFields="getHyperLinksMergeFields"  :mergeFields="getMergeFields"/>
            </div>
        </section>
    </div>
`,
};