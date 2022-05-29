import { store } from '../appLoader/main.js';

export default {
	name: 'grapesJSComponent',
	data() {
		return {
              DialogHelpHeader :this.$store.getters.getRes(6187, '')
		};
	},
	components: {
  },

	mounted() {
		//t�che #2 459, KJE: on instancie grapesJs � l'aide de eMemoEditor
		//eGrapesJSEditor permet de cr�er des instances de grapesJs
		grapesJsEditor = new eGrapesJSEditor(
            "gjs", true, document.getElementById('templateEditor_gjs'), null, store.state.body, true, "oMemoInfos"
		);
		grapesJsEditor.enableTemplateEditor = true;
		grapesJsEditor.enableAdvancedFormular = true;
		grapesJsEditor.config.width = '100%';
		grapesJsEditor.setGrapesJSCustomInfos(store.state.grapesJSBlocks, mainJS.store.state.tab, mainJS.store.state.worldlinePaimentBlocs);
		//tache #2 676 chargement de formulaire existant
		grapesJsEditor.setCss(store.state.bodyCss);
		//on injecte la liste la liste des champs de fusion dans grapesJS
		grapesJsEditor.mergeFields = store.state.mergeFields;
    grapesJsEditor.showHTMLTemplateEditor();
    let that = this;
    grapesJsEditor.helpDialog =  function () {
      that.$store.commit("setDialogHelp",true);
    }
		//On d�sactive CKEDITOR dans le formulaire avanc� (on garde la possibilit� d'int�grer CKEditor dans le formulaire avanc� en supprimant cette ligne)
		// CKEDITOR.instances['oMemoInfos'] = false;
	},
	methods: {
	    closeDialog ()
	    {
	      this.$store.commit("setDialogHelp",false);
	    }
  },
	computed: {
		showDialog: {
			get() {
				return this.$store.state.DialogHelp;
			}
    },
		    srcIframe: {
		      get() {
		        if(this.$store.getters.getUserLangID == 0)
		        {
		          return './help/formular/fr.index.html';
		        }
		        else if(this.$store.getters.getUserLangID == 1)
		        {
		          return './help/formular/en.index.html';
		        }
		        else
		           return './help/formular/en.index.html';        
		      }
		    }
	},
	template: `
    <div style='height:100%;width:100%;'>
    <div id="templateEditor_gjs" style='height:100%;width:100%;'></div>

  <v-row justify="center" class="test">
    <v-dialog
      v-model="showDialog"  
     
     
    transition="dialog-bottom-transition"
     
    >
     
      <v-card style="height:90vh;overflow: hidden">
        <v-toolbar
          dark
          color="primary"
        >         
          <v-toolbar-title>{{DialogHelpHeader}}</v-toolbar-title>
          <v-spacer></v-spacer>
          <v-toolbar-items>
          <v-btn
            icon
            dark
            @click="closeDialog"
          >
            <v-icon>mdi-close</v-icon>
          </v-btn>
          </v-toolbar-items>
        </v-toolbar>
        <iframe style="height:calc(90vh - 64px);width:100%" :src="srcIframe"></iframe>
      </v-card>
    </v-dialog>
  </v-row>
  </div>
`
};
