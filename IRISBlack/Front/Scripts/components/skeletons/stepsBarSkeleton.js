export default {
    name: "stepsBarSkeleton",
    data() {
		return {
			bSteps: 6, 
			bInput: 5
		};
    },
	computed: {
		getskeleton: function () {
			return this.stepsProps.FieldsById
		},
		getNbSteps: function () {
			return this.stepsProps.FieldsById.length > 0 ? this.stepsProps.FieldsById.length : this.bSteps
        }
	},
	mounted() {},
    methods: {},
	props: ["stepsProps"],
    template: `
    <div v-if="getskeleton" class="stepsBar-skeletons-wrapper">
        <ul class="row skeleton-progressbar">
			<li v-for="step in bSteps" :key="step" class="fa listStep col-2">
				<div  class="skeleton-circle-container skeleton"></div>
				<div class="skeleton skeleton-subtitle-progress"></div>
			</li>
        </ul>
		<div class="col-md-12 skeleton-content-progress">
			<div v-for="input in bInput" :key="input" class="col-md-3 skeleton skeleton-subtitle-progress skeleton-progress-field"></div>
		</div>
    </div>
    `
};
