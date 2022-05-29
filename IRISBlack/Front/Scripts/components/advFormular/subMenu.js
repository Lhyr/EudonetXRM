/**
 * Conteneur  menu "gauche"
 * */
export default {
  name: "subMenu",
  data() {
    return {};
  },

  methods: {
    //set l'entrée principale (et secondaire si fourni) du menu selectionné
    activeLink(evt, menuEntry, link) {
      link = link || { refChildren: null };

      this.activeEntry = {
        mainEntry: menuEntry.refChildren,
        subEntry: link.refChildren,
      };
    },
  },

  computed: {
    activeEntry: {
      get: function () {
        return this.value;
      },
      set: function (activentry) {
        // envoye le changement de valeur au composent parent
        this.$emit("input", activentry);
      },
    },
    getRes() {
      return function (resid) {
        return this.$store.getters.getRes(resid);
      };
    },
  },
  props: ["tabsBarAside", "value"],

  template: `
  <div class="settingsPanel">
      <div class="sidebar">
          <ul class="sidebar-menu tree">
              <!-- Entrées principales -->
              <li v-for="tab in tabsBarAside" 
                      v-bind:class="[tab.refChildren == activeEntry.mainEntry ? ' active menu-open' : '', 'treeview navLink_publi']">
                  <a 
                    :ref="tab.ref" 
                    v-on:click="activeLink($event, tab)" 
                    href="#!">

                      <i class="panelIcon" :class="tab.igitcon"></i>
                      <span class="treeTitle">{{  getRes( tab.txtHeader )  }}</span>
                      <span v-if="tab.blocks" class="pull-right-container">
                          <i class="fa fa-angle-left pull-right"></i>
                      </span>
                  </a>

                  <!--  Sous menu-->
                  <ul v-if="tab.blocks" v-bind:style="[tab.refChildren == activeEntry.mainEntry ? { height: tab.blocks.length * 35 + 'px'} : { height: '0px'} ]" class="treeview-menu">
                      <li   
                        :ref="block.ref"                                   
                        v-bind:class="[block.refChildren === activeEntry.subEntry ? ' ' : '', 'link_run']"
                        v-for="(block, idx) in tab.blocks" >
                          <a 
                            v-on:click="activeLink($event,tab, block)" 
                            >{{ getRes(block.title)}}
                          </a>
                          
                      </li>                               
                  </ul>
              </li>
          </ul>
      </div>
  </div>
`,
};
