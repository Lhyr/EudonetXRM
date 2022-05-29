export default {
    name: 'networkShare',
    data() {
        return {
            notPublishedError: this.$store.getters.getRes(2730, ''),
            sharing: {
                url: '',
                title: this.$store.getters.getRes(2729, ''),
                description: '',
                quote: '',
                hashtags: '',
                twitterUser: ''
            },
            networks: [
                { network: 'facebook', name: 'Facebook', icon: 'fab fah fa-lg fa-facebook-f', color: '#1877f2' },
                { network: 'linkedin', name: 'LinkedIn', icon: 'fab fah fa-lg fa-linkedin', color: '#007bb5' },
                { network: 'twitter', name: 'Twitter', icon: 'fab fah fa-lg fa-twitter', color: '#1da1f2' }
            ],
            inputUploadFile: this.$store.getters.getRes(2810, ''),
            tooltipUploadFile_1: this.$store.getters.getRes(2811, ''),
            tooltipUploadFile_2: this.$store.getters.getRes(2840, ''),
            acceptFormat: 'image/png, image/jpeg, image/svg, image/gif, image/jpg',
            rules: [(value) => !value || value.size < 5000000 || this.$store.getters.getRes(2813, '')],

            dataImage: null,
            hasChanged: false
        };
    },
    components: {
        contentHeader: () => import('../../shared/contentHeaderComponent.js')
    },
    computed: {
        published() {
            return this.$store.state.Published;
        },
        url() {
            return this.$store.state.FormularLink + '&ts=' + this.$store.state.formularStore.TimeStamp;
        },

        integrationScript() {
            return this.$store.state.FormularIntegrationScript;
        },

        //Libellés
        getTitleLabel() {
            return this.$store.getters.getRes(7216);
        },
        getInfoTitleLabel() {
            return this.$store.getters.getRes(2745);
        },
        getDescriptionLabel() {
            return this.$store.getters.getRes(5131);
        },
        getInfoDescriptionLabel() {
            return this.$store.getters.getRes(2746);
        },
        inputMetaTitle: {
            get() {
                return this.$store.state.MetaTitle;
            },
            set(metaTitle) {
                this.$store.commit('setMetaTitle', metaTitle);
            }
        },
        inputMetaDescription: {
            get() {
                return this.$store.state.MetaDescription;
            },
            set(metaDescription) {
                this.$store.commit('setMetaDescription', metaDescription);
            }
        },
        getUploadMetaImgURL() {
            if (this.dataImage == null)
                return this.$store.getters.getRes(2812);
            else
                return "";
        },
        ImageFile: {
            get() {
                return this.$store.state.FileImage;
            },
            set(fileimage) {
                this.$store.commit('setFileImage', fileimage);
                if (!(fileimage instanceof File))
                    this.$store.commit('setMetaImgURL', '');
            }
        }
    },
    mounted() { },
    methods: {
        InputImageFile(value) {
            if (value) {
                this.$store.commit('setMetaImgURL', value.name);
                this.$store.commit('setFileImage', value);

            }
            else {
                this.$store.commit('setMetaImgURL', '');
                this.$store.commit('setFileImage', File);
            }
        },
        haschangedAction(value) {
            if (value) {
                this.$store.commit('setImageHasChanged', value);
            }
        }
    },
    props: ['dataTab'],
    template: `
    <div class="moduleContent">
	    <section class="content-header">
		    <contentHeader :title=dataTab.title :subtitle=dataTab.txtSubTitle />
	    </section>
	    <div class="v-messages theme--light error--text" v-show="!published">{{ notPublishedError }}</div>
	    <div class="publishContent disable" v-if="!published">
		    <a class="share-network-facebook" style="background-color: rgb(193 196 199);">
			    <i class="fab fah fa-lg fa-facebook-f"></i>
			    <span>Facebook</span>
		    </a>
		    <a class="share-network-linkedin" style="background-color: rgb(193 196 199);">
			    <i class="fab fah fa-lg fa-linkedin"></i>
			    <span>LinkedIn</span>
		    </a>
		    <a class="share-network-twitter" style="background-color: rgb(193 196 199);">
			    <i class="fab fah fa-lg fa-twitter"></i>
			    <span>Twitter</span>
		    </a>
	    </div>
	    <div class="publishContent" v-if="published">
		    <ShareNetwork
			    v-for="network in networks"
			    :network="network.network"
			    :key="network.network"
			    :style="{backgroundColor: network.color}"
			    :url="url"
			    :title="sharing.title"
			    :description="sharing.description"
			    :quote="sharing.quote"
			    :hashtags="sharing.hashtags"
			    :twitterUser="sharing.twitterUser"
		      >
			    <i :class="network.icon"></i>
			    <span>{{ network.name }}</span>
		    </ShareNetwork>
	    </div>
	    <div>
		    <template>
			    <div class="text-center d-flex align-center">
				    <v-tooltip bottom>
					    <template v-slot:activator="{ on, attrs }">
						    <div class="v-messages theme--light" style="font-family: 'Roboto', sans-serif;" >
							    <small>{{ inputUploadFile }}</small>
						    </div>
						    <div class="v-input__append-inner" >
							    <i aria-hidden="true" class="v-icon notranslate mdi mdi-help-circle-outline theme--light" 
					    style="cursor: pointer;" v-bind="attrs" v-on="on"></i>
							    <span class="v-tooltip v-tooltip--top"></span>
						    </div>
					    </template>
					    <span> {{ tooltipUploadFile_1 }} <br /> {{ tooltipUploadFile_2 }} </span>
				    </v-tooltip>
			    </div>
		    </template>
		    <edn-file :acceptFormat="acceptFormat" :label="getUploadMetaImgURL" 
		    @getImage="dataImage = $event" @click:clear="dataImage = null,ImageFile={}"  
		    :rules="rules" :value="ImageFile" v-on:uploadFile="InputImageFile"
		     @hasChanged="haschangedAction"></edn-file>
		    <v-img v-if="dataImage != null" :src="dataImage" clear contain height="150"></v-img>
	    </div>
	    <edn-field :label="getTitleLabel" v-model="inputMetaTitle"  :maxlength="70" :tooltip="getInfoTitleLabel"></edn-field>
	    <edn-memo :label="getDescriptionLabel" v-model="inputMetaDescription" :maxlength="155" :rows="4"  :tooltip="getInfoDescriptionLabel" style="word-break: normal;"></edn-memo>
</div>
`
};
