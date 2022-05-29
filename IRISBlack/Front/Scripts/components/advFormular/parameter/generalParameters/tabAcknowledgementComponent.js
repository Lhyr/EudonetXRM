//AABBA tache 2 842 creation de composant Page de remerciement
export default {
    name: "tabAcknowledgement",
    //store: store,
    data() {
        return {
            Content: this.$store.state.submissionBody,
            selectedValue: this.$store.state.AcknowledgmentSelect,
            isValidUrl: false,
            radios: [
                {
                    label: this.$store.getters.getRes(2624, ''),
                    value: "1"
                },
                {
                    label: this.$store.getters.getRes(2623, ''),
                    value: "2"
                }
            ],
        };
    },
    components: {
        contentHeader: () => import("../../shared/contentHeaderComponent.js"),
        memoEditor: () => import("../../shared/memoEditorComponent.js")
    },

    mounted() {
     
    },
    computed: {
        radioSelect: {
            get() {
                return this.$store.state.AcknowledgmentSelect;
            },
            set(acknowledgmentSelect) {
                this.selectedValue = acknowledgmentSelect;
                this.$store.commit("setAcknowledgmentSelect", acknowledgmentSelect);
            },
        },
        inputRedirection: {
            get() {
                return this.$store.state.submissionRedirectUrl;
            },
            set(submissionRedirectUrl) {
                //SHA tâche #2 997
                const isInputEvent = submissionRedirectUrl instanceof InputEvent || submissionRedirectUrl instanceof Event;
                if (!isInputEvent)
                    this.$store.commit("setSubmissionRedirectUrl", submissionRedirectUrl);
            },
        },        
        getMergeFields() {
            return this.$store.state.mergeFieldsWithoutExtended;
        },
        getHyperLinksMergeFields() {
            return this.$store.state.hyperLinksMergeFields;
        },
        getMutaionNameMethodForBody() {
            return 'setSubmissionBody';
        },
        getURLRedirectionLabel() {
            return this.$store.getters.getRes(1728, '');
        },
        getWrongMessage() {
            return this.$store.getters.getRes(6717, '');
        }
    },
    methods: {
        urlValidationError(valid) {
          
            this.$store.commit("setIsValidRedirectUrl", valid);
        }
    },
    props: ["dataTab"],

    template: `
    <div class="moduleContent acknowledgment" >
        <section class="content-header">

         <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />

         <edn-radio label="" :radios="radios" v-model="radioSelect"/>

         <div id="remerciement" class="formularAdvMemo" v-show="selectedValue == '1'">
            <memoEditor v-model="Content" :hyperLinksMergeFields="getHyperLinksMergeFields" :mergeFields="getMergeFields" :mutationMethod="getMutaionNameMethodForBody" 
            :idMemo="'SubmitMemoId'" />
                  
         </div>
              
        <div id="redirection" v-show="selectedValue == '2'">
            <edn-url :label="getURLRedirectionLabel" v-model="inputRedirection" @onError="urlValidationError($event)" :wrongUrlMsg="getWrongMessage" />   
        </div>

        </section>
    </div>
`,

};