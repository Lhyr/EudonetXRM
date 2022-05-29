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

        // Si la page actuelle d�finit un type d'�diteur, on l'indique � eMemoEditor
        currentMemoEditor.editorType = 'formularsubmission';

        // #68 13x - Type de champ M�mo - Editeur de templates HTML avanc� (grapesjs) ou CKEditor
        // Si la page actuelle indique que l'on doit instancier un �diteur de templates HTML avanc� (ex : pour l'e-mailing), on l'indique � eMemoEditor
        currentMemoEditor.enableTemplateEditor = false;

        // Si la page actuelle d�finit un type de barre d'outils sp�cifique � afficher, on l'indique � eMemoEditor
        currentMemoEditor.toolbarType = 'formular';

        //tracking externalis� activ�
        currentMemoEditor.externalTrackingEnabled = false;

        //Gestion des consentements
        currentMemoEditor.useNewUnsubscribeMethod = false;

        // Si la page actuelle a d�fini une liste de champs de fusion utilisables sur ce champ M�mo dans une variable JS, on
        // affecte le contenu de cette variable � la propri�t� mergeFields de eMemoEditor pour que son plugin puisse les prendre en charge

        try {
            currentMemoEditor.mergeFields = JSON.parse(this.mergeFields);
            currentMemoEditor.oMergeHyperLinkFields = JSON.parse(this.hyperLinksMergeFields);
        }
        catch (ex) {
        }

        // Taille par d�faut du champ M�mo
        if (getNumber(element.style.width) != NaN && getNumber(element.style.width) > 0)
            currentMemoEditor.config.width = element.style.width;
        else
            currentMemoEditor.config.width = '99%';
        currentMemoEditor.config.height = '195px';

        //Pour les sms on a besoin que la textarea, pas bordure ni de barres d'outils
        currentMemoEditor.borderlessMode = false;

        // En revanche, sur l'�cran d'�dition d'E-mail/E-mailing, on interdit l'affichage de l'�diteur avec une barre d'outils r�duite
        if ((getCurrentView(document) == "FILE_CREATION" || getCurrentView(document) == "FILE_MODIFICATION") && nodeExist(document, "mailDiv")) {
            currentMemoEditor.preventCompactMode = true;
            currentMemoEditor.config.width = '100%';
        }

        currentMemoEditor.inlineMode = false;

        // Mise � jour en base lors de la sortie du champ
        currentMemoEditor.updateOnBlur = true;

        // Mode lecture seule ou �criture
        currentMemoEditor.readOnly = false;
        currentMemoEditor.uaoz = false;
        currentMemoEditor.fromParent = false;

        // Affichage du champ M�mo apr�s param�trage
        currentMemoEditor.show();

		// Tableau destin� � contenir tous les objets de types eMemoEditor dans les cas de la modification et de la cr�ation
		// N'est pas r�initialis� s'il existe d�j�. Permet de ne pas �craser le contexte aux yeux d'un champ M�mo enfant (ex : affich� via une fen�tre Plein �cran) si une MAJ a lieu en arri�re-plan, et provoque le r�affichage complet de toute l'application (RefreshFile global), rendant ainsi la Modal Dialog du champ M�mo Plein �cran orpheline.
		// Charge au code qui se chargera de remplir ce tableau pr�c�demment remis � z�ro, de faire les MAJ appropri�es sur un tableau existant
		// Cette v�rification a �t� port�e sur eMain.js et eFile.js dans le cadre de la demande #83 082
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
