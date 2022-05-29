/**
 * Conteneur de block principaux (paramètres et publication) hors grapejs
 *  composant "Menu gauche" (subMenu.js)
 * + conteneur "reéel" (subContent.js)
 * */
//
const tabSubContent = () => import("./subContent.js");
const tabSubMenu = () => import("./subMenu.js");

export default {
  name: "paramComponent",
  data() {
    return {

      activeEntry:{
        mainEntry: null,
        subEntry: null,
      }
    };
  },

  computed: {
    getRes() {
      return function (resid) {
        return this.$store.getters.getRes(resid);
      };
    },
  },

  props: ["propDataComponent"],

  components:{tabSubContent, tabSubMenu},

  methods: {

     /*
      AABBA tache 2 842 creation de composant Page de remerciement
       retourne les données correspondant au menu selectionné
    */
    getSpecificData(tab) {
      let subMenu = this.propDataComponent.tabsBarAside.find(
        (t) => t.refChildren == tab
      );
      if (subMenu != null) return subMenu;

      return [];
    },
  },

  mounted() {
    try{
    this.activeEntry = { mainEntry: this.propDataComponent.tabsBarAside.find( z => z.active).refChildren, subEntry:null}
    }
    catch{
      this.activeEntry = {mainEntry : null, subEntry:null};
    }
   },

  template: `
    <div class="contentTab" ref="paramComponent">
        <div class="leftPanel">
        <tabSubMenu
          v-model="activeEntry"          
          :tabsBarAside="this.propDataComponent.tabsBarAside"
        />
        </div>
        <div class="stageScroller">
            <div 
            style="display: 'block'" 
            class="modules_publi modal-adv">
                <tabSubContent v-if="activeEntry!==null && activeEntry.mainEntry != null" 
                :data-tab="getSpecificData(activeEntry.mainEntry)"                
                :activeSubEntry="activeEntry.subEntry"
                ></tabSubContent>
            </div>
        </div>
    </div>
`,
};
