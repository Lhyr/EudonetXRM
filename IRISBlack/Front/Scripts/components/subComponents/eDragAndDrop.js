//import { UpFilDragLeave, UpFilDrop, UpFilDragOver, callCheckPjExists, checkPjExists, SendFile, activateProgressBar, activateProgressBarv2, cancelNewPjName } from '../../methods/tabMethods.js?ver=803000';
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';

export default {
    name: "eDragAndDrop",
    data() {
        return {
            draggedOver: false
        }
    },
    computed: {
        isDraggedOver() {
            return (this.draggedOver) ? 'dragged' : '';
        },
        isExpanded() {
            return (this.dragAndDropExpanded) ? 'expanded' : '';
        }
    },
    methods: {
        /**
         * Méthode qui permet de récupérer le fichier une fois déposé et qui réaffiche le tableau à la suite
         * @param {any} event
         */
        dropIt(event) {
            event.preventDefault();
            let onLoad = true;

            this.draggedOver = false;

            if (this.dropFunction != undefined)
                this.$emit(this.dropFunction, { ev: event, loading: onLoad });
        },
        /**
         * Méthode qui permet de savoir si l'on survol la zone on l'on peut déposer le fichier
         * @param {any} event
         */
        dragOverArea(event) {
            event.preventDefault();
            this.draggedOver = true;
            this.$emit('leavingDrag',true);
            if (this.dragFunction != undefined)
                this.$emit(this.dragFunction, event);
        },
        /**
         * ELAIZ - ajout de Node.contains() pour vérifier que l'on ne sort pas à l'intérieur de la zone
         * mais à l'extérieur car le dragleave se déclenche entre le header et le body par exemple
         * @param {any} event
         */
        hideUpload(event) {
            if (this.dragAndDropOptions.nbRows > 0 && !this.draggedOver && !this.$el.contains(event.relatedTarget)) {
                this.draggedOver = false;
                this.$emit('leavingDrag', false);
            }
        },
        /**
         * ELAIZ - Retour homol 2 909 - Méthode qui permet de vérifier que l'utilisateur n'a pas changer d'avis
         * et remis le fichier dans l'explorateur de fichier. Dans ce cas si ce dernier est trop près
         * de la dragarea il ne détecte pas le dragleave au moment du retour. 
         * @param {any} event correspond au dragleave
         */
        hideDragArea(event) {
            this.draggedOver = false;
            if (this.dragAndDropOptions.nbRows < 1) {
                return false;
            }
            let hideDrag = this.dragAndDropExpanded;
            let isChild = [...event.target.children].find(x => x == event.relatedTarget);
            let relTarget = event.relatedTarget;
            let dragleave = () => {
                hideDrag = false;
                event.target.removeEventListener('dragleave', dragleave);
            }
            /*vérifier que event.related est égale à null(qui correspond à la sortie de la fenêtre, 
            la cible étant inconnu) */
            if (relTarget == null || relTarget == undefined) {
                hideDrag = false;
            } else if ((relTarget != null || relTarget != undefined) && isChild) {
                /* permet de vérifier aussi sur les éléments enfants des sorties. par exemple, le clic droit n'est pas détecter, hors il annule l'action de drag du fichier */
                relTarget.addEventListener('dragleave', dragleave)
            }
            this.$emit('update:dragAndDropExpanded',false)
            this.$emit('leavingDrag', hideDrag);
        }

    },
    props: {
        cssClassDragContainer: {
            type: String,
            default: 'drag-drop-container'
        },
        cssClassDragArea: {
            type: String,
            default: 'drop-area'
        },
        dragAndDropOptions: {
            upload: Boolean,
            draggedOver: Boolean,
            blocked: {
                type: Boolean,
                default: true
            },
            title:String
        },
        dragAndDropTitle:String,
        dragAndDropExpanded: Boolean,
        dropFunction: String,
        dragFunction: String
    },
    mixins: [eFileMixin],
    created() {
    },
    updated() {
    },
    mounted() {
    },
    template: `
        <div 
        :class="[cssClassDragContainer,isExpanded]"
        @mouseout="$emit('mouseoutOnDropArea')" 
        @mouseover="$emit('hoverOnDropArea')" 
        @click="$emit('clickOnDropArea')" 
        @drop="dropIt($event)" 
        @dragleave="hideUpload($event)">
            <div ref="droparea" 
            :class="[cssClassDragArea,isDraggedOver,isExpanded]" 
            @dragleave="hideDragArea($event)" 
            @dragover.stop="dragOverArea($event)"> 
                <i class="fas fa-cloud-upload-alt"></i>
                <span class="drag-drop-title">{{dragAndDropOptions?.title}}</span>
            </div>
        </div>
`
}