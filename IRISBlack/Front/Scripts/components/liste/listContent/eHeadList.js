import { eListComponentsMixin } from '../../../mixins/eListComponentsMixin.js?ver=803000';
import { FieldType } from '../../../methods/Enum.min.js?ver=803000'
import EventBus from '../../../bus/event-bus.js?ver=803000';
import { initDatabaseIrisBlack, initDatabase, manageDatabaseIrisBlack, getFkLinksByTab, linkToCall } from "../../../methods/eFileMethods.js?ver=803000";
import { initIndexedDB, manageIndexedDB, setDataDbAsync, getDataDbAsync, mergeDataDbAsync, filterDataDbAsync, countDataDbAsync, firstDataDbAsync } from "../../../methods/eBkmMethods.js?ver=803000";
import { getIrisPurpleActived, loadComputedValue } from '../../../shared/XRMWrapperModules.js?ver=803000'
import { JSONTryParse } from "../../../methods/eMainMethods.js?ver=803000"

export default {
    name: "eHeadList",
    data() {
        return {
            minHeaderWidth: 250,
            elemDragStart: Object,
            objDragStart: Object,
            elemDragEnd: Object,
            objDragEnd: Object,
            onresize: false,
            canMerge: true, // ELAIZ - rajout d'une variable en attendant d'avoir la réponse du back si on a les droits de fusion,
            canResizeAfterUpdate: false, /* indique si on peut mettre à jour la taille des colonnes au updated(). Indispensable dans certains cas où le DOM est mis à jour (ex : somme des colonnes) */
            filterWidth: 35,
            computedValues: Array,
            computedValuesLastUpdate: new Date().getTime(), /* permet d'indiquer à VueJS de réafficher les sommes des colonnes après mise à jour */
        };
    },
    mixins: [eListComponentsMixin],
    components: {
        eLoaderTable: () => import(AddUrlTimeStampJS("./eLoaderTable.js")),
    },
    computed: {
        /** taille des colonnes*/
        heightCol() {
            //return 50 * this.dataJson.Data.length;
            return 18;
        },
        /** computed vers un computed parent similaire. */
        getFlatTableToDisplay() {
            return this.$parent.getFlatTableToDisplay;
        },
        /** retourne la structure des éléments depuis le back. */
        getLstStructFields: function () {
            return this.dataJson?.Structure?.LstStructFields;
        }
    },
    methods: {
        getIrisPurpleActived,
        loadComputedValue,

        /**
         * Les styles à appliquer à l'en-tête du tri.
         * @param {any} headColumn
         */
        getThSortStyle(headColumn) {
            let sClassList = ["fas", "fa-sort-amount-down"];

            if (this.sortActivated == headColumn.DescId)
                sClassList.unshift("sortActivated");

            if (headColumn.SortOrder == 0)
                sClassList[sClassList.length - 1] += "-alt ";

            sClassList.push("i-table");

            return sClassList;
        },
        /**
        * Les styles à appliquer à l'en-tête des filtres.
        * @param {any} headColumn
        */
        getThFilterStyle(headColumn) {
            return headColumn.ExpressFilterActived ? 'FilterActivated ' : '';
        },

        /**
         * Les styles à appliquer à l'en-tête pour l'icône Somme des colonnes
         * Si la somme des colonnes est activée sur la colonne en question, on ajoute une classe permettant de faire apparaître le bouton comme actif
         * @param {any} headColumn
         */
        getComputedColumnClass(headColumn) {
            return "ComputedColumn" + (this.hasComputedColumnValue(headColumn) ? " ComputedColumnEnabled" : "");
        },

        /**
         * La valeur de la colonne calculée
         * @param {any} headColumn
         */
        getComputedColumnValue(headColumn) {
            this.canResizeAfterUpdate = true; // autorise la mise à jour de la taille des colonnes au updated(), qui sera déclenché lorsque la valeur ci-dessous sera renvoyé au template
            let computedValue = this.computedValues?.find(element => element.DescId == headColumn.DescId);
            if (computedValue?.Active)
                return computedValue.Value;
            else
                return ""; // somme des colonnes désactivée : pas d'affichage de valeur
        },

        /**
         * Indique si la colonne actuelle a été calculée
         * @param {any} headColumn
         */
        hasComputedColumnValue(headColumn) {
            return this.computedValues?.find(element => element.DescId == headColumn.DescId)?.Active;
        },

        /**
        * Appelle le contrôleur afin de récupérer la somme des colonnes dont les DescIDs sont passés en paramètre
        * @param {any} aDescIds
        */
        async getComputedValuesFromController(aDescIds) {
            let oDataComputedValue = this.loadComputedValue();

            if (!(oDataComputedValue))
                return;

            let param = {
                ListCol: aDescIds.join(";"),
                Tab: Number(this.propSignet.DescId),
                ParentTab: Number(this.getTab),
                ParentFileId: Number(this.getFileId),
                ParentPMFileId: Number(this.getFkLinksByTab(TAB_PM))
            };

            let oDataJson = JSONTryParse(
                await linkToCall(
                    oDataComputedValue.url,
                    { ...oDataComputedValue.params, ...param }
                )
            );

            return oDataJson;
        },

        /**
         * Pour la colonne donnée, on inverse le statut d'activation actuel du calcul de la somme
         * Et si elle devient active, on récupère et met à jour sa valeur calculée
         * @param {any} headColumn Colonne sur laquelle activer/désactiver la somme
         */
        toggleComputedValueColumn(headColumn) {
            let oComputedValue = this.computedValues?.find(element => element.DescId == headColumn.DescId);
            if (oComputedValue) {
                oComputedValue.Active = !oComputedValue.Active;
                return this.setComputedValueColumns([oComputedValue]);
            }
        },

        /**
         * Met à jour la base de données IndexedDB avec le statut d'activation de la somme des colonnes, sur la colonne concernée.
         * Puis met à jour le computed/data local
         * @param {any} columns Tableau de colonnes à mettre à jour
         */
        async setComputedValueColumns(columns) {
            let aDescIdsToGet = new Array();

            // Pour chaque colonne transmise...
            for (const column of columns) {
                // On construit l'objet au format base de données, contenant les valeurs mises à jour
                // La base ne stockant pas toutes les données passées en paramètre, on ne garde que celles dont on a besoin
                let oDbData = { tab: this.getTab, bkm: this.propSignet.DescId, descId: column.DescId, active: column.Active };
                // On ajoute ou met à jour la colonne dans le tableau local
                let nComputedValueIndex = this.computedValues?.findIndex(element => element.DescId == column.DescId);
                if (nComputedValueIndex >= 0)
                    this.computedValues[nComputedValueIndex] = column;
                else
                    this.computedValues.push(column);
                // Pour chaque colonne activée, on ajoute son DescID dans la liste de ceux à demander au contrôleur
                if (column.Active)
                    aDescIdsToGet.push(column.DescId);
                // Et on met à jour les infos d'activation dans IndexedDB
                try {
                    await this.mergeDataDbAsync(this.dbComputedValue, "ComputedValue", oDbData);
                }
                catch (ex) {
                    console.log("Erreur de mise à jour de l'information dans IndexedDB : " + ex.message);
                }
            };

            // Puis, pour chaque valeur maintenue activée, on appelle le contrôleur pour obtenir le calcul mis à jour
            if (aDescIdsToGet.length > 0) {
                try {
                    let oComputedValues = await this.getComputedValuesFromController(aDescIdsToGet);
                    for (const cv of oComputedValues) {
                        let nComputedValueIndex = this.computedValues?.findIndex(element => element.DescId == cv.DescId);
                        // Mise à jour du tableau interne avec les retours du contrôleur
                        if (nComputedValueIndex >= 0) {
                            this.computedValues[nComputedValueIndex].Value = cv.Value;
                            this.computedValues[nComputedValueIndex].DecimalValue = cv.DecimalValue;
                            this.computedValues[nComputedValueIndex].DecimalCount = cv.DecimalCount;
                        }
                    }
                }
                catch (ex) {
                    console.log("Erreur d'appel au contrôleur : " + ex.message);
                }
            }
            // Et enfin, on remet à jour le flag qui sert de clé aux éléments HTML affichant les compteurs dans le template, afin de forcer VueJS à rafraîchir le rendu
            this.computedValuesLastUpdate = new Date().getTime();
        },

        /**
         * Permet de savoir qui on doit cibler.
         * @param {any} e
         */
        getTarget(e, id) {
            let target = this.$refs.thData[id];
            return target;
        },

        /**
         * Ici est organisé le tri des colonnes.
         * @param {any} elm
         */
        setSorting(elm) {
            let tab = this.getTab;
            let bkm = this.propSignet.DescId
            let listsort = elm.DescId;
            let mainFld = this.dataJson.Structure.MainFieldId;
            let listorder = (elm.SortOrder == 1) ? 0 : 1;
            //ELAIZ - on vérifie que le filtre de tri est différent de l'ancien si il exite pour le remplacer par le nouveau
            if (this.sortActivated != elm.DescId)
                this.$emit('update:sortActivated', elm.DescId);
            let newPageCall = {
                pageNum: 1,
                nbLine: this.nbLineCall
            };

            // appel l'updater pour les tris en mode signet
            let updatePref = 'tab=' + tab + ';$;bkm=' + bkm + ';$;mainFld=' + mainFld + ';$;listsort=' + listsort + ';$;listorder=' + listorder;
            this.setUserBkmPref({ updatePref });
            this.$emit("callSignetWithFilter", newPageCall);
        },

        /**
         * Quand on redimensionne les colonnes.
         * @param {any} dataColumn permet de récupérer les paramètres de l'entêtes
         * @param {any} event
         * @param {any} id permet de récupérer l'index de l'entête
         * @param {boolean} Redimensionnement automatique
         */
        resizeColumns(dataColumn, event, id, bAutoResize) {
            this.onresize = true;
            let tab = this.getTab;
            let bkm = this.propSignet.DescId;
            let elem = event.target;
            let pageX, curCol, curColWidth;
            //curCol = elem.parentElement;
            curCol = this.$refs.thData[id];
            pageX = event.pageX;
            curColWidth = curCol.offsetWidth;
            //let minWidth = curCol.children[0]?.clientWidth + 75;
            /*ELAIZ - On vérifie que les entêtes sont filtrables,triables afin de rajouter la largeur des icônes de tri/filtre */

            let fileContainer = document.getElementById('fileDiv_' + tab);
            let margin = 5;
            let sortWidth = dataColumn.IsSortable ? this.$refs['thSort_' + dataColumn.DescId].find(th => th).clientWidth + margin : 0;
            let filterWidth = dataColumn.IsFiltrable ? this.$refs['thFilter_' + dataColumn.DescId].find(th => th).clientWidth + margin : 0;
            let computedWidth = dataColumn.IsComputable ? this.$refs['thComputed_' + dataColumn.DescId].find(th => th).clientWidth + margin : 0;
            let menuWidth = dataColumn.IsComputable ? this.$refs['thMenu_' + dataColumn.DescId].find(th => th).clientWidth : 26;
            let sumWidth = dataColumn.IsComputable && this.hasComputedColumnValue(dataColumn) ? this.$refs['sumFilter_' + dataColumn.DescId]?.find(th => th)?.offsetWidth - this.$refs['thLabel_' + dataColumn.DescId][0]?.clientWidth : 0;
            let padding = parseInt(window.getComputedStyle(this.$refs.thData[id], null).getPropertyValue('padding-left')) + parseInt(window.getComputedStyle(this.$refs.thData[id], null).getPropertyValue('padding-right'));
            let columnHeaderElementsWidth = menuWidth + sumWidth + /*sortWidth + filterWidth + computedWidth*/ + padding;
            let minWidth = this.$refs['thLabel_' + dataColumn.DescId][0]?.clientWidth + columnHeaderElementsWidth;
            //let cursorSibling = elem.nextElementSibling;
            //cursorSibling.style.opacity = "1";

            // Si on doit donner une taille automatique à la colonne, on la calcule
            if (bAutoResize) {
                let dataJSON = this.dataJson.Data.filter(f1 => f1.LstDataFields.filter(f => f.DescId == dataColumn.DescId));
                let maxContentWidth = this.getOptimalColumnSize(dataColumn, dataJSON, fileContainer);
                // On empêche la colonne de dépasser la largeur affichable du mode Fiche
                let containerWidth = fileContainer.clientWidth;
                let maxWidth = containerWidth - columnHeaderElementsWidth;
                // Puis on retient la largeur la plus adaptée
                // Selon si la largeur calculée est supérieure à celle du conteneur du mode Fiche, ou non
                if (maxContentWidth > maxWidth)
                    curColWidth = maxWidth; // largeur retenue : le maximum affichable par le conteneur du nouveau mode Fiche
                else
                    curColWidth = maxContentWidth; // largeur retenue : celle du plus grand élément contenu dans la colonne
                // Si la largeur calculée devient plus petite que la largeur minimale requise pour afficher l'entête, on privilégie
                // la largeur minimale requise
                if (curColWidth < minWidth)
                    curColWidth = minWidth;
            }

            let vmDivWdID = Vue.extend({
                data() {
                    return {
                        widthInnerHTML: (dataColumn.Width != 0)
                            ? dataColumn.Width
                            : this.minHeaderWidth,
                    }
                },
                template: `<div id="widthInnerID" ref="widthInnerID" class="widthInner">
                            {{widthInnerHTML}}
                           </div>`
            });

            let cptvmDivWdID = new vmDivWdID().$mount();
            curCol.appendChild(cptvmDivWdID.$el);

            // Fonction interne qui met à jour le DOM
            let fctUpdateDisplay = function (nNewWidth) {
                dataColumn.Width = nNewWidth;
                cptvmDivWdID.$el.innerHTML = dataColumn.Width + ' px';
            };

            // Fonction interne qui met à jour en base
            let fctSave = function (nNewWidth, ctx) {
                let widthInnerID = document.getElementById("widthInnerID");

                if (widthInnerID)
                    widthInnerID.remove();

                ctx.dataJson.Data
                    .flatMap(dt => dt.LstDataFields)
                    .filter(dtfd => dtfd.DescId == dataColumn.DescId)
                    .forEach(dtfd => dtfd.Width = nNewWidth);

                that.onresize = false;
                let listwidth = dataColumn.DescId + ';' + dataColumn.Width;
                let updatePref = 'tab=' + tab + ';$;bkm=' + bkm + ';$;mainFld=' + ctx.dataJson.Structure.MainFieldId + ';$;listwidth=' + listwidth
                ctx.setUserBkmPref({ updatePref });

                //cursorSibling.style.opacity = "0";
            };

            if (bAutoResize) {
                fctUpdateDisplay(curColWidth);
                fctSave(curColWidth, this);
            }
            else {
                let mousemove = (e) => {
                    if (curCol) {
                        this.diffX = e.pageX - pageX;
                        if (minWidth <= (curColWidth + this.diffX)) {
                            fctUpdateDisplay(curColWidth + this.diffX + padding);
                        }
                    }
                }
                document.addEventListener('mousemove', mousemove);

                let mouseup = (e) => {
                    fctSave(curColWidth + this.diffX, this);

                    document.removeEventListener('mousemove', mousemove);
                    document.removeEventListener('mouseup', mouseup);
                    this.onresize = false;
                };
                document.addEventListener('mouseup', mouseup);
            }

            
        },

        /**
         * Calcule la largeur optimale à donner à la colonne, en fonction des valeurs situées en-dessous de cette colonne
         * Chaque valeur présente sous la colonne est placée dans un conteneur temporaire invisible,
         * dimensionné avec une largeur automatique, puis comparée aux autres valeurs du signet pour la colonne en question.
         * Le conteneur invisible permet au navigateur de renvoyer la taille réellement requise par la valeur sans tenir compte des
         * contraintes de taille déjà présentes sur la colonne (trop grande ou trop petite).
         * Fonctions équivalentes E17 (qui, elles, s'appuyaient sur un conteneur généré côté serveur pour chaque ligne) : 
         * eListMainRenderer.AdjustCol() côté serveur, et eList.resizeMove() + eList.getColMaxSize() côté client
         * @param {any} dataColumn Colonne pour laquelle calculer la taille
         * @param {any} datAJSON Données à examiner pour la colonne en cours (permet de filtrer les données vides)
         * @param {any} tempContainerParentNode Elément du DOM auquel rattacher le conteneur invisible (il ne sera pas détruit pour être réutilisé au prochain appel, pour des questions de performances)
         */
        getOptimalColumnSize(dataColumn, dataJSON, tempContainerParentNode) {
            // On récupère la largeur du plus gros élément parmi les lignes affichées dans le signet qui contiennent du texte
            // Pour calculer cette largeur sans être influencé par la taille actuelle de la colonne, on examine la taille que
            // ferait chaque élément, dans un conteneur masqué non soumis à une contrainte de taille
            // Si le conteneur a déjà été créé lors d'un appel précédent, on le réutilise, sinon, on le crée
            let oResizeColumnsWidthContainer = document.getElementById("resizeColumnsWidthContainer");
            if (!oResizeColumnsWidthContainer) {
                oResizeColumnsWidthContainer = document.createElement("div");
                oResizeColumnsWidthContainer.id = "resizeColumnsWidthContainer";
                oResizeColumnsWidthContainer.style.whiteSpace = "nowrap"; // Pour ne pas autoriser de retour à la ligne ni ellipsis
                oResizeColumnsWidthContainer.style.display = "block"; // Il faut être en display: block pour que la largeur soit calculée et renvoyée par le navigateur
                oResizeColumnsWidthContainer.style.visibility = "hidden"; // Donc en compensation, pour ne pas afficher l'élément, visibility: hidden;
                oResizeColumnsWidthContainer.style.position = "absolute"; // Valeur indispensable pour que la largeur du conteneur ne soit pas dépendante de celle de son parent
                oResizeColumnsWidthContainer.style.width = "auto"; // Autre valeur indispensable pour que le conteneur s'ajuste à la largeur du contenu
                tempContainerParentNode.appendChild(oResizeColumnsWidthContainer);
            }
            // On récupère la taille actuelle de la colonne aux yeux du DOM
            let nCurrentColWidth = 0;
            let oCurrentCol = document.querySelector("th[did='" + dataColumn.DescId + "']");
            if (oCurrentCol)
                nCurrentColWidth = getNumber(getComputedStyle(oCurrentCol).width);
            // Puis...
            let nOptimalWidth =
                Math.max.apply( /* On prend la plus grande valeur parmi... */
                    Math,
                    Array.from(document.querySelectorAll("#signet *[divdescid='" + dataColumn.DescId + "'] > *")) /* ...tous les lignes de la colonne... */
                        /* ...sur lesquelles on applique le traitement suivant pour effectuer la comparaison : */
                        .map(f2 => {
                            // On vide le conteneur temporaire
                            if (oResizeColumnsWidthContainer.hasChildNodes())
                                oResizeColumnsWidthContainer.removeChild(oResizeColumnsWidthContainer.lastChild);
                            oResizeColumnsWidthContainer.appendChild(f2.cloneNode(true)); /* on place une COPIE de l'élément ciblé dans le conteneur temporaire pour l'avoir sans overflow (càd. sans ellipsis) à sa vraie largeur */

                            // Dans le cas où la valeur est contenue dans un champs, on le cache et met la valeur dans un span afin d'ajuster la largeur car les
                            // champs on tendance à ne pas s'ajuster à la largeur des valeurs
                            if (oResizeColumnsWidthContainer.querySelector(".input-group > div.input-line > input, .input-group.listRubriqueCaractere > input")) {
                                let inputValue = oResizeColumnsWidthContainer.querySelector(".input-group input")?.value;
                                let span = `<span class="input-value">${inputValue}</span>`
                                oResizeColumnsWidthContainer.querySelector(".input-group .input-line, .input-group.listRubriqueCaractere").insertAdjacentHTML('afterbegin', span);

                                oResizeColumnsWidthContainer.querySelector(".input-group input, .input-group.listRubriqueCaractere > input").style.display = "none"
                            }

                            // Si la donnée actuelle est vide, on l'exclut du calcul
                            let nCurrentFileId = getNumber(f2.parentElement.getAttribute("fileid"));
                            let oCurrentFileData = dataJSON?.find(f => f.MainFileId == nCurrentFileId);
                            // Si il n'y a pas de displayValue, on prend la Value sinon il considère qu'il n'y a pas de valeurs
                            let sCurrentValue = oCurrentFileData?.LstDataFields.find(f => f.DescId == f2.parentElement.getAttribute("divdescid"))?.DisplayValue
                            || oCurrentFileData?.LstDataFields.find(f => f.DescId == f2.parentElement.getAttribute("divdescid"))?.Value;
                            if (sCurrentValue?.trim() == "")
                                return 0;
                            // Sinon
                            else {
                                // Pour certains types de composants/champs, il faut visiblement rajouter les marges posées sur les cellules parentes (td) pour que le calcul soit plus juste
                                let nAdditionalWidth = 0;
                                let bNeedsAdditionalWidth = oResizeColumnsWidthContainer.querySelector(".globalDivComponent"); 
                                if (bNeedsAdditionalWidth) {
                                    let nParentCellPaddingLeft = getNumber(f2.parentElement?.parentElement?.style.paddingLeft); /* on récupère les marges des conteneurs parents posées en style inline */
                                    let nParentCellPaddingRight = getNumber(f2.parentElement?.parentElement?.style.paddingRight); /* on calcule les marges des conteneurs parents posées en style inline */
                                    nAdditionalWidth = nParentCellPaddingLeft + nParentCellPaddingRight;
                                }
                                return oResizeColumnsWidthContainer.lastChild.offsetWidth + nAdditionalWidth; /* puis on renvoie à map() la largeur calculée par le navigateur dans ce conteneur pour effectuer la comparaison via Math */
                            }
                        })
                );
            // Et on renvoie une valeur exploitable par l'appelant
            // (Renvoyer 0 en cas d'erreur permettra à celui-ci d'appliquer la largeur minimale de la colonne, plutôt que la largeur maximale)
            return nOptimalWidth ? nOptimalWidth : nCurrentColWidth; // à défaut, on renvoie la largeur actuelle de la colonne, plutôt que 0 ou undefined, false, NaN...
        },

        /**
         * On entre dans la zone du drag n drop.
         * @param {any} rubrique
         * @param {any} e
         */
        dragenter(rubrique, e, id) {
            if (this.$parent.blocked) {
                return false;
            }
            var divDash = document.getElementById("divDash")

            if (!divDash)
                return false;

            var target = this.getTarget(e, id);

            divDash.style.left = target.offsetLeft + 4 + "px";

        },

        /**A la fin du drag n drop.
         * 
         * @param {any} rubrique
         * @param {any} e
         */
        dragend(rubrique, e) {
            var ui_transit = document.getElementById("ui-transit");
            var divDash = document.getElementById("divDash");

            if (ui_transit)
                ui_transit.remove();

            if (divDash)
                divDash.remove();
        },

        /**
         * Quand on relace la souris sur une autre colonne.
         * @param {any} rubrique
         * @param {any} e
         */
        drop(rubrique, e, id) {
            this.objDragEnd = rubrique;
            var that = this;

            this.elemDragEnd = this.getTarget(e, id);

            if (rubrique.DescId != this.objDragStart.DescId) {
                this.dataJson.Structure.LstStructFields = this.dragArray(this.dataJson.Structure.LstStructFields, rubrique.DescId, this.objDragStart.DescId);
                this.dataJson.Data.forEach(a => {
                    a.LstDataFields = this.dragArray(a.LstDataFields, rubrique.DescId, this.objDragStart.DescId);
                });


                let tab = this.getTab;
                let bkm = this.propSignet.DescId
                let listmove = this.objDragStart.DescId + ";" + rubrique.DescId
                let updatePref = 'tab=' + tab + ';$;bkm=' + bkm + ';$;mainFld=' + this.dataJson.Structure.MainFieldId + ';$;listmove=' + listmove

                this.setUserBkmPref({ updatePref, callback: function () { that.callBackDrag(rubrique) } });
            }
        },

        /**
         * Ajout d'une classe qu'on supprime ensuite à la fin du drag.
         * @param {any} obj
         */
        callBackDrag(obj) {
            this.elemDragEnd.classList.add("dropped");
            setTimeout(() => this.elemDragEnd.classList.remove("dropped"), 400);
        },

        /**
         * Permutation  des colonnes.
         * @param {any} arr
         * @param {any} aDescId
         * @param {any} bDescId
         */
        dragArray(arr, aDescId, bDescId) {
            let _arr = [...arr]
            let toIndex = _arr.findIndex(r => r.DescId == aDescId);
            let fromIndex = _arr.findIndex(r => r.DescId == bDescId);

            if (toIndex == -1 || fromIndex == -1)
                return;

            let element = _arr[fromIndex];
            _arr.splice(fromIndex, 1);
            _arr.splice(toIndex, 0, element);

            return _arr
        },

        /**
         * On demarre le drag'n'drop des colonnes.
         * @param {any} rubrique
         * @param {any} e
         */
        dragstart(rubrique, e) {
            e.dataTransfer.setData('text', this.innerHTML);
            if (this.onresize) {
                e.preventDefault();
                return false;
            }

            this.elemDragStart = e.target;

            if (e.target.localName == "a")
                this.elemDragStart = e.target.parentElement;

            this.objDragStart = rubrique;

            let vmUiTransit = Vue.extend({
                data() {
                    return {
                        Label: rubrique.Label,
                        Width: rubrique.Width,
                    }
                },
                computed: {
                    /** Détermine la taille de la div par rapport à la rubrique.
                     * */
                    getUiTransitWidth() {
                        let width = 250;
                        if (this.Width > 0 && this.Width < width)
                            width = this.Width

                        return width;
                    },
                },
                template: `<div id="ui-transit" class="clsUiTransit" ref="ui-transit" :style="{width:(getUiTransitWidth - 13) + 'px'}">
                            {{Label}}
                           </div>`
            });


            let vmDivDash = Vue.extend({
                template: `<div id="divDash" ref="divDash" class="clsDivDash">
                                <i class="clsIconTop fas fa-caret-down"></i>
                                <i class="clsIconBottom fas fa-caret-up"></i>
                           </div>`
            })

            let cptUiTransit = new vmUiTransit().$mount();
            let cptDivDash = new vmDivDash().$mount();
            e.dataTransfer.setDragImage(cptUiTransit.$el, (cptUiTransit.getUiTransitWidth / 2), 18);

            document.body.appendChild(cptUiTransit.$el);

            let tr = this.elemDragStart.parentElement

            tr.appendChild(cptDivDash.$el)

        },

        /**
         * Filtres express.
         * @param {any} options
         */
        globalFilterUpdate(options) {
            let tab = this.getTab;
            let bkm = this.propSignet.DescId;
            let did = options.did;
            let newPageCall = {
                pageNum: 1,
                nbLine: this.nbLineCall
            };

            if (options.type == "LOGIC") {
                let prefFldVal;

                prefFldVal = `tab=${tab};$;bkm=${bkm};$;filterExpress=${did};|;` +
                    (options.action == "change")
                    ? ` ${0} ;|;${options.value}`
                    : `;|;$cancelthisfilter$`;

                this.setUserBkmPref({ updatePref: prefFldVal, callback: loadBkm(bkm) });


                this.$emit("callSignetWithFilter", newPageCall);
            }
        },

        /**
         * Ici se décide les filtres.
         * @param {any} e
         * @param {any} obj
         * @param {any} fullData
         */
        async emitMethod(e, obj, fullData) {
            let rect = e.target.getBoundingClientRect();
            let options = {
                maxHeight: obj.Format == 5 ? 190
                    : obj.Format == 14 ? this.minHeaderWidth
                        : [10, 11, 17].includes(obj.Format) ? 160
                            : [25, 2].includes(obj.Format) ? 230
                                : 150,
                posTop: rect.top,
                posRight: rect.right,
                posBottom: rect.bottom,
                posLeft: rect.left,
                typeModal: 'date',
                type: "expressFilter",
                close: "auto",
                id: 'ExpressFilterDate',
                datas: obj,
                fullDatas: fullData,
                propSignet: this.propSignet
            };

            EventBus.$emit('globalExpressFilter', options);
        },

        /** mise à jour de la taille des colonnes. */
        majWidth() {
            if (!this.dataJson || !this.$refs["thData"])
                return false;

            let tailleMini = 64;

            //let bkm = [...this.$refs["thData"]]
            //    .filter(bk => bk.firstChild.firstChild.clientWidth + tailleMini > bk.clientWidth);

            // On récupère uniquement les signets susceptibles d'être trop petits
            let bkm = [...this.$refs["thData"]]
                .filter(
                    bk => bk.clientWidth <
                        this.$refs['thLabel_' + bk.attributes.did.value][0]?.clientWidth + /* libellé */
                        //(this.$refs['thSort_' + bk.attributes.did.value]?.length > 0 ? this.$refs['thSort_' + bk.attributes.did.value][0]?.clientWidth : 0) + /* tri */
                        //(this.$refs['thFilter_' + bk.attributes.did.value]?.length > 0 ? this.$refs['thFilter_' + bk.attributes.did.value][0]?.clientWidth : 0) + /* filtre */
                        //(this.$refs['thComputed_' + bk.attributes.did.value]?.length > 0 ? this.$refs['thComputed_' + bk.attributes.did.value][0]?.clientWidth : 0) + /* somme de colonne */
                        tailleMini /* taille mini */
                );

            bkm.forEach(bk => {
                let desc = parseInt(bk.getAttribute("did"));
                //let minWidth = bk.firstChild.firstChild.clientWidth + tailleMini;

                let findItemData = this.dataJson.Structure.LstStructFields.find(b => b.DescId == desc);
                let margin = 5;
                let errorMargin = margin;
                let padding = parseInt(window.getComputedStyle(bk, null).getPropertyValue('padding-left')) + parseInt(window.getComputedStyle(bk, null).getPropertyValue('padding-right'));
                let sortWidth = findItemData.IsSortable ? this.$refs['thSort_' + desc].find(th => th).clientWidth + margin : 0;
                let filterWidth = findItemData.IsFiltrable ? this.$refs['thFilter_' + desc].find(th => th).clientWidth + margin : 0;
                let computedWidth = findItemData.IsComputable ? this.$refs['thComputed_' + desc].find(th => th).clientWidth + margin : 0;
                let sumWidth = findItemData.IsComputable && this.hasComputedColumnValue(findItemData) ? this.$refs['sumFilter_' + desc]?.find(th => th)?.offsetWidth - this.$refs['thLabel_' + desc][0]?.clientWidth : 0;
                let menuWidth = findItemData.IsComputable ? this.$refs['thMenu_' + desc].find(th => th).clientWidth : 26;

                let minWidth = this.$refs['thLabel_' + desc][0]?.clientWidth /*+ sortWidth + filterWidth + computedWidth*/ + sumWidth + menuWidth + padding + errorMargin;

                if (findItemData && (this.$refs['thLabel_' + desc][0]?.clientWidth != 0 || bk.clientWidth != 0))
                    findItemData.Width = minWidth;

                this.getFlatTableToDisplay.forEach(a => {
                    let findItemData = a.LstDataFields.find(b => b.DescId == desc);

                    if (findItemData)
                        findItemData.Width = minWidth;
                })
            });
        },
        /**
         * Permet de rajouter une classe css sur la colonne annexes
         * @param {any} col correspond à this.dataJson.Structure.LstStructFields
         */
        attachmentHead(col) {
            return (col.Format == FieldType.PJ) ? 'attachment-head' : this.hasComputedColumnValue(col) ? 'is-computing' : '';
        },
        /**
         * Permet de calcluler la largeur mini à appliquer aux colonnes (autrefois directement mis dans le template)
         * @param {any} col correspond à this.dataJson.Structure.LstStructFields
         */
        setMinWidth(col) {
            return (col.Width != 0 ? col.Width : this.minHeaderWidth) + 'px'
        },

        getFkLinksByTab,

        initIndexedDB,
        manageIndexedDB,
        setDataDbAsync,
        getDataDbAsync,
        mergeDataDbAsync,
        filterDataDbAsync,
        countDataDbAsync,
        firstDataDbAsync,
        initDatabaseIrisBlack,
        manageDatabaseIrisBlack,
        initDatabase,

        /** Initialisation de la base de donnée locale avec les sommes des colonnes **/
        initLocalDatabase: async function () {
            this.computedValues = new Array();
            try {
                this.dbComputedValue = await this.initDatabaseIrisBlack();
            }
            catch (e) {
                this.manageDatabaseIrisBlack(e);
            }

            let aComputedValuesToUpdate = new Array();

            if (!this.dataJson?.Structure?.LstStructFields)
                return;

            // Pour chaque champ présent sur le signet, si calculable, on récupère son statut "calcul activé" dans la base locale IndexedDB
            for (const field of this.dataJson?.Structure?.LstStructFields.filter(field => field.IsComputable)) {
                let oUpdatedComputedValue = { DescId: field.DescId, Active: false };
                // On vérifie si la colonne figure dans la base, et si la somme est activée pour elle
                let oSearch = { tab: this.getTab, bkm: this.propSignet.DescId, descId: field.DescId };
                let computedValueCount = 0;
                try {
                    computedValueCount = await this.countDataDbAsync(this.dbComputedValue, "ComputedValue", oSearch);
                }

                catch (e) {
                    this.manageDatabaseIrisBlack(e);
                }
                if (computedValueCount > 0) {
                    try {
                        let oComputedValueInDb = await this.firstDataDbAsync(this.dbComputedValue, "ComputedValue", oSearch);
                        oUpdatedComputedValue.Active = oComputedValueInDb?.active;
                    }
                    catch (e) {
                        this.manageDatabaseIrisBlack(e);
                    }
                }
                // Ajoute le champ dans la liste de ceux à créer ou mettre à jour en base
                aComputedValuesToUpdate.push(oUpdatedComputedValue);
            }

            // Crée ou met à jour le statut "activé" des colonnes dans la base, et dans le tableau local computedValues
            // Puis appelle le contrôleur pour obtenir les valeurs calculées des colonnes activées
            if (aComputedValuesToUpdate.length > 0)
                this.setComputedValueColumns(aComputedValuesToUpdate);
        },
        /**
         * 
         * @param {any} Renvoie au menu vertical des filtres si l'un d'eux est actif
         */
        getMenuCssClass(headColumn) {
            let cssClass = '';
            if (this.sortActivated == headColumn.DescId
                || headColumn.ExpressFilterActived != null
                || this.hasComputedColumnValue(headColumn))
                cssClass += 'filters-activated ';
            if (this.hasComputedColumnValue(headColumn))
                cssClass += 'is-computing ';

            return cssClass;
        },
        /**
         * Ajoute un écouteur qui permet de déclencher le recalcul de la somme sur les colonnes */
        async setRefreshComputedValuesListener() {
            EventBus.$on('RefreshComputedValues_' + this.propSignet.DescId, (options) => {
                // Récupération des colonnes concernées sur lesquelles la somme est activée
                let computedValuesToRefresh = this.computedValues?.find(element => element.Active && options.descIds.indexOf(element.DescId) > -1);
                if (computedValuesToRefresh)
                    this.setComputedValueColumns([computedValuesToRefresh]);
            });
        }
    },
    props: {
        dataJson: [Object, Array],
        newLine: Boolean,
        nbLineCall: Number,
        sortActivated: Number
    },
    async created() {
        EventBus.$on('updateBkmExpressFilter', (options) => {
            this.globalFilterUpdate(options)
        });

        EventBus.$off("RefreshComputedValues_" + this.propSignet.DescId);

        await this.initLocalDatabase();
    },
    updated() {
        // La mise à jour de la taille des colonnes est en conflit avec celle opérée par resizeColumns() si on la fait systématiquement au updated(). Elle était donc désactivée suite au correctif #83 509
        // Mais elle est nécessaire pour mettre à jour la taille des colonnes lorsqu'une somme a été calculée dessus. (Tâche #4 361)
        // updated() étant le seul évènement correctement déclenché après le rendu définitif de la valeur calculée dans le DOM (permettant ainsi de calculer la bonne taille)
        // on utilise ici un booléen pour autoriser cette MAJ uniquement dans les cas qui nous intéressent réellement après MAJ du DOM par VueJS.
        if (this.canResizeAfterUpdate) {
            this.majWidth();
            this.canResizeAfterUpdate = false;
        }
    },
    mounted() {
        this.majWidth();

        // Ajoute un écouteur qui réagit lorsqu'on demande une mise à jour de la somme de certaines colonnes
        this.setRefreshComputedValuesListener();
    },
    template: `
    <thead id="thead-infinite-scroll" class="clsTHeadSticky">
        <tr class="first_th">
            <th :class="getAddPurplFileClass" class="thead-icon"></th>
            <th v-if="propSignet.DescId == 2 && canMerge"></th>
            <th ref="thData" 
                :did="headColumn.DescId" 
                v-for="(headColumn,id) in getLstStructFields"
                :key="headColumn.id" 
                :class="[attachmentHead(headColumn), 'thead th_3 sorting_disabled']"  
                :style="{minWidth:setMinWidth(headColumn)}"
                rowspan="1" colspan="1" 
                draggable="true"
                droppable="false"
                @dragenter.stop="dragenter(headColumn, $event, id)"
                @dragend="dragend(headColumn, $event)"
                @drop="drop(headColumn, $event, id)"
                @dragstart="dragstart(headColumn, $event)"
            >
                <div class="d-flex">
                    <span :ref="'thLabel_' + headColumn.DescId" v-if="headColumn.Format != 27" :class="getMenuCssClass(headColumn)" class="overFlow headStyle">
                        <span class="headcol-label">{{headColumn.Label}}</span>
                        <!--<div v-if="hasComputedColumnValue(headColumn)" class="sum-elem">
                            <i class="fa fa-calculator" ></i>
                            <span :key="computedValuesLastUpdate">{{getComputedColumnValue(headColumn)}}</span>
                        </div>-->
                    </span>
                    <span :ref="'thLabel_' + headColumn.DescId" v-else-if="headColumn.Format === 27" :class="getMenuCssClass(headColumn)" class="overFlow headStyle">
                        <i class="fa fa-paperclip"></i>
                    </span>
                    <div class="menu-filters--container">
                        <div v-if="hasComputedColumnValue(headColumn)" class="sum-elem">
                            <i class="fa fa-calculator" ></i>
                            <span :ref=" 'sumFilter_' + headColumn.DescId " :key="computedValuesLastUpdate">{{getComputedColumnValue(headColumn)}}</span>
                        </div>
                        <i v-if="headColumn.IsSortable || headColumn.IsFiltrable" :ref="'thMenu_' + headColumn.DescId" :class="['fas fa-ellipsis-v i-table filter-menu',getMenuCssClass(headColumn)]"></i>
                        <div class="bkm-filters d-flex">
                            <a  class="p-relative" v-if="headColumn.IsSortable" @click="setSorting(headColumn)" href="#!">
                                <i :ref="'thSort_' + headColumn.DescId" :class="getThSortStyle(headColumn)" style="float:left"></i></a>
                            <a class="p-relative"  @click="emitMethod($event, headColumn, dataJson);" v-if="headColumn.IsFiltrable" href="#!">
                                <i :ref=" 'thFilter_' + headColumn.DescId " ref="button" :class="getThFilterStyle(headColumn)"
                                    class="i-table fa fa-filter" style="float:left;"></i>
                            </a>
                            <a  @click="toggleComputedValueColumn(headColumn);" v-if="headColumn.IsComputable" href="#!">
                                <i :ref=" 'thComputed_' + headColumn.DescId " ref="button" :class="getComputedColumnClass(headColumn)"
                                    class="i-table fa fa-calculator" style="float:left;">
                                    <!--<span :ref="'thComputedValue_' + headColumn.DescId" class="ComputedColumnValue" v-if="headColumn.IsComputable" :key="computedValuesLastUpdate">{{getComputedColumnValue(headColumn)}}</span>-->
                                </i>
                            </a>
                        </div>
                    </div>
                    <div @mousedown="resizeColumns(headColumn, $event, id, false)" @dblclick="resizeColumns(headColumn, $event, id, true)" class="resizeColumnHeader">&nbsp;</div>
                    <!-- <div :style="{height: heightCol + 'px'}" class="cursorResize">&nbsp;</div> --> 
                </div>
            </th>
        </tr>
        <eLoaderTable css-class="scrollForPrev clsTblLnLoader" v-if="newLine" />
    </thead>
`
};