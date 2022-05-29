const blocksComponent = () => import("./blocksComponent.js");

export default {
    name: "blocksPanel",
    data: function () {
        return {
            activetab: 1,
        }
    },
    components: {
        blocksComponent
    },
    computed: {
        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        },
        workflowBlocks: {
            get() {
                return this.$store.state.WorkflowJSBlocks;
            }
        },
    },
    methods: {
    },
    mounted() {
    },
    template: `
     <div id="leftcard">
        <div id="tabsPanel" class="containerLeftPanel">
            <div id="subnav" class="tabsLeftPanel">
                 <div id="triggers" class="side" v-on:click="activetab=1" v-bind:class="[ activetab === 1 ? 'navactive' : 'navdisabled' ]">{{getRes(8794,'Triggers')}}</div>
                <div id="actions" class="side" v-on:click="activetab=2" v-bind:class="[ activetab === 2 ? 'navactive' : 'navdisabled' ]">{{getRes(8795,'Actions')}}</div>
            </div>
            <div class="contentLeftPanel">
                <div v-if="activetab === 1" class="contentTriggers">
                    <div id="blocklist">
                        <div v-for="block in this.workflowBlocks.panelBlockTrigger" class="trigger blockelem create-flowy noselect">
                            <input type="hidden" name='blockelemtype' class="blockelemtype" :value="block.index">
                            <div class="grabme"></div>
                            <blocksComponent :icon="block.icon" :title="block.title" :description="block.description"/>
                        </div>
                    </div>
                </div>
                <div v-if="activetab === 2" class="tabcontentActions">
                    <div id="blocklist">
                        <div v-for="block in this.workflowBlocks.panelBlockAction" class="action blockelem create-flowy noselect">
                            <input type="hidden" name='blockelemtype' class="blockelemtype" :value="block.index">
                            <div class="grabme"></div>
                            <blocksComponent :icon="block.icon" :title="block.title" :description="block.description"/>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
`,
};