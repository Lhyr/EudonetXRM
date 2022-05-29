export default {
    name: "tabStartDate",
    data() {
        return {
            MsgDateStart: this.$store.state.MsgDateStart
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
        memoEditor: () => import("../../shared/memoEditorComponent.js")
    },
    computed: {
        getMutationNameMethodForStartDate() {
            return 'setMsgDateStart';
        },
        getHyperLinksMergeFields() {
            return this.$store.state.hyperLinksMergeFields;
        },
        getStartDate: {
            get() {
                return this.$store.state.StartDate;
            },
            set(startDate) {
                this.$store.commit("setStartDate", startDate);
            }
        },
        getMergeFields() {
            return this.$store.state.mergeFieldsWithoutExtended;
        },
        //Date et heure de début
        getStartDateTimeLabel() {
            return this.$store.getters.getRes(2740);
        }
    },
    props: ["dataTab"],
    template: `
    <div class="moduleContent StartDate" >
       <section class="content-header">
          <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
          <div>
             <edn-date-time :label="getStartDateTimeLabel"
                 v-model="getStartDate" placeholder="dd/MM/yyyy" format="dd/MM/yyyy"/>
          </div>
          <div id="EndDebut" class="ememodate formularAdvMemo">
             <memoEditor v-model="MsgDateStart"  :idMemo="'DateDebutMemoId'" 
                :mutationMethod="getMutationNameMethodForStartDate"  :hyperLinksMergeFields="getHyperLinksMergeFields"  :mergeFields="getMergeFields"/>
          </div>
       </section>
    </div>
`,
};