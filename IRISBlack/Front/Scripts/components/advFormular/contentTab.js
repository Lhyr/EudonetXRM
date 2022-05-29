
/**
 * Conteneur principal pour les différents "entrée" du menu principal
 * A ce stade :
 *  - GrapesJS (grapejs) 
 *  - Paramètres (contentContainer.js) 
 *  - Publication (contentContainer.js)
 *  
 *  Utilisation de v-show, le contenu des autres éléments devant être présent dans le dom 
 *   pour diverses raisons
 * */
const genericContent = () => import("./contentContainer.js");
const grapesJSComponent = () => import("./grapesJS/grapesJSComponent.js")

import { menuJson } from "./jsonparams/menuItems.js"

export default {
    name: "contentTab",
    data() {
        return {
            tabs: menuJson
        };
    },
    components: {
        genericContent,
        grapesJSComponent
    },


    computed: {
        wizardActiveTab() {
            return this.$store.state.wizardActivTab
        }
        ,
        isActiveTab() {
            return function (tabName) { return this.wizardActiveTab == tabName }
        },

        getRes() {
            return function (resid) { return this.$store.getters.getRes(resid) }
        }
    },
    template: `

    <div class="contentElems" ref="contentTabs">

     <!-- Bloc Création graphique -->
      <div
        v-show="isActiveTab('CreatGraphique')"
        ref="CreatGraphique"
        class="tabcontent"
      >
        <grapesJSComponent></grapesJSComponent>
      </div>

      <!-- Bloc paramètres -->
      <div v-show="isActiveTab('Param')" ref="Param" class="tabcontent">
        <genericContent
          :prop-data-component="tabs.find((a) => a.ref == 'param')"
          defaultactive=""
        ></genericContent>
      </div>

      <!-- Bloc Publication -->
      <div
        v-show="isActiveTab('Publication')"
        ref="Publication"
        class="tabcontent"
      >
        <genericContent
          :prop-data-component="tabs.find((a) => a.ref == 'publi')"
          defaultactive=""
        ></genericContent>
      </div>
    </div>

`,

};