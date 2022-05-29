export default {
    name: 'memoEditor',

    render(createElement) {
        return createElement('div', {}, [
            createElement(this.tagName)
        ]);
    },

    props: {
        value: {
            type: String,
            default: ''
        },
        tagName: {
            type: String,
            default: 'textarea'
        },
        idMemo: {
            type: String,
            default: 'SubmitMemoId'
        },
        mutationMethod: {
            type: String,
            default: ''
        },
        mergeFields: {
            type: String,
            default: ''
        },
        hyperLinksMergeFields: {
            type: String,
            default: ''
        }
    },

    mounted() {
        // Instanciation de l'objet eMemoEditor
        const element = this.$el.firstElementChild;
        const strMemoId = this.idMemo;
        const isHtml = true;
        const value = this.value;
        const currentMemoEditor = new eMemoEditor(
            'edt' + strMemoId,
            isHtml,
            element.parentElement,
            null,
            value,
            false,
            'nsMain.getMemoEditor(\'edt' + strMemoId + '\')'
        );

        // Si la page actuelle définit un type d'éditeur, on l'indique à eMemoEditor
        currentMemoEditor.editorType = 'formularsubmission';

        // #68 13x - Type de champ Mémo - Editeur de templates HTML avancé (grapesjs) ou CKEditor
        // Si la page actuelle indique que l'on doit instancier un éditeur de templates HTML avancé (ex : pour l'e-mailing), on l'indique à eMemoEditor
        currentMemoEditor.enableTemplateEditor = false;

        // Si la page actuelle définit un type de barre d'outils spécifique à afficher, on l'indique à eMemoEditor
        currentMemoEditor.toolbarType = 'formular';

        //tracking externalisé activé
        currentMemoEditor.externalTrackingEnabled = false;

        //Gestion des consentements
        currentMemoEditor.useNewUnsubscribeMethod = false;

        // Si la page actuelle a défini une liste de champs de fusion utilisables sur ce champ Mémo dans une variable JS, on
        // affecte le contenu de cette variable à la propriété mergeFields de eMemoEditor pour que son plugin puisse les prendre en charge

        try {
            currentMemoEditor.mergeFields = JSON.parse(this.mergeFields);
            currentMemoEditor.oMergeHyperLinkFields = JSON.parse(this.hyperLinksMergeFields);
        }
        catch (ex) {
        }

        // Taille par défaut du champ Mémo
        if (getNumber(element.style.width) != NaN && getNumber(element.style.width) > 0)
            currentMemoEditor.config.width = element.style.width;
        else
            currentMemoEditor.config.width = '99%';
        currentMemoEditor.config.height = '195px';

        //Pour les sms on a besoin que la textarea, pas bordure ni de barres d'outils
        currentMemoEditor.borderlessMode = false;

        // En revanche, sur l'écran d'édition d'E-mail/E-mailing, on interdit l'affichage de l'éditeur avec une barre d'outils réduite
        if ((getCurrentView(document) == "FILE_CREATION" || getCurrentView(document) == "FILE_MODIFICATION") && nodeExist(document, "mailDiv")) {
            currentMemoEditor.preventCompactMode = true;
            currentMemoEditor.config.width = '100%';
        }

        currentMemoEditor.inlineMode = false;

        // Mise à jour en base lors de la sortie du champ
        currentMemoEditor.updateOnBlur = true;

        // Mode lecture seule ou écriture
        currentMemoEditor.readOnly = false;
        currentMemoEditor.uaoz = false;
        currentMemoEditor.fromParent = false;

        // Affichage du champ Mémo après paramétrage
        currentMemoEditor.show();

		// Tableau destiné à contenir tous les objets de types eMemoEditor dans les cas de la modification et de la création
		// N'est pas réinitialisé s'il existe déjà. Permet de ne pas écraser le contexte aux yeux d'un champ Mémo enfant (ex : affiché via une fenêtre Plein écran) si une MAJ a lieu en arrière-plan, et provoque le réaffichage complet de toute l'application (RefreshFile global), rendant ainsi la Modal Dialog du champ Mémo Plein écran orpheline.
		// Charge au code qui se chargera de remplir ce tableau précédemment remis à zéro, de faire les MAJ appropriées sur un tableau existant
		// Cette vérification a été portée sur eMain.js et eFile.js dans le cadre de la demande #83 082
        if (!nsMain.hasMemoEditors()) {
            nsMain.initMemoEditorsArray();
        }

        nsMain.setMemoEditor('edt' + strMemoId, currentMemoEditor);
        

        this.instance = currentMemoEditor.htmlEditor;
        this.instance.on('change', evt => {
            const data = this.instance.getData();

            if (this.value !== data) {
                this.$emit('input', data, evt, this.instance);
            }
        });
    },

    watch: {
        value(val) {
            if(val != this.instance.getData())
                this.instance.setData(val);
            this.$store.commit(this.mutationMethod, val);
        }
    },

    beforeDestroy() {
        if (this.instance) {
            this.instance.destroy();
        }

        this.$_destroyed = true;
    },

    methods: {
    }
};
