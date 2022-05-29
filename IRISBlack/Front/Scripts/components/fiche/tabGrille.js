import EventBus from '../../bus/event-bus.js?ver=803000';
import { LoadSignet } from "../../methods/eFileMethods.js?ver=803000";
import { addScriptText } from "../../methods/eFile.js?ver=803000";
import { eFileMixin } from '../../mixins/eFileMixin.js?ver=803000';
import { typeBkm } from '../../methods/Enum.min.js?ver=803000';

export default {
    name: "tabGrille",
    data() {
        return {
            timeoutDomSync: 200,
            hDefault: '512px',
            DataJson:undefined
        };
    },
    props: ['propGrille'],
    components: {},
    mixins: [eFileMixin],
    template: `
    <div>
        <div :id="'dvGrille_' + propGrille.DescId" :ref="'dvGrille_' + propGrille.DescId"></div>
    </div>`,
    methods: {
        /**
         * On déplace gw-container vers un tag passé en paramètre.
         * @param {any} tag le tag vers lequel transférer gw-container
         */
        moveGwContainerInTag(tag) {
            let gwCont = document.querySelector('#gw-container');

            if (!gwCont) {
                gwCont = document.createElement("div");
                gwCont.id = "gw-container";
                gwCont.className = "fs_8pt";
            }

            tag.appendChild(gwCont);
        },

        // DEBUT QBO
        /**
         * /
         * @param {any} //grille object
        */
        // Permet le ressize du conteneur grille
        calHeightTabs(tab) {
            var options = {
                timeoutCalculHeight: 200,
                mobileDesign: this.mobile,
                gridId: tab ? tab.id : ''
            };
            EventBus.$emit('timeOutRightNav', options);
        },
        // FIN QBO


        /**
         * Reinitialise les grilles spécifiques à un bookmark
         * à l'aide d'un descid.
         * @param {any} descid Le descid.
         */
        reinitBookmarkGrid(descid) {
            let gwTab = document.querySelectorAll("[id^='gw-tab-" + descid + "'], [id^='gw-bkm-" + descid +"']");
            this.activateGrid(gwTab, "0");

            let bkm = this.$refs["dvGrille_" + descid];

            if (bkm)
                bkm.childNodes.forEach(chld => chld.remove());
        },

        /**
         * Active ou désactive les grilles.
         * @param {any} gwTab le composant des grilles.
         * @param {any} activation active / désactive (un string)
         */
        activateGrid(gwTab, activation) {
            if (gwTab) {
                gwTab.forEach((x, i) => {
                    gwTab[i].setAttribute("active", activation);
                    if (x.childNodes && x.childNodes.length > 0)
                        gwTab[i].childNodes.forEach((nd) => {
                            nd.setAttribute("active", activation);
                            nd.removeAttribute("style");
                        });
                    else
                        gwTab[i].remove();
                });
            }

        },

        LoadSignet,
        /**
         * Chargement du Signet Grille
         * @param {int} bkmDescId le descid du Bkm
         * @returns {bool} en cas de non rechargement ou d'erreur.
         * */
        async loadBkmGrid(bkmDescId) {

            this.fid = this.getFileId;
            this.did = this.getTab;

            try {
                this.DataJson = await this.LoadSignet(bkmDescId, undefined, undefined, typeBkm.grid);
            } catch (e) {
                EventBus.$emit('globalModal', {
                    typeModal: "alert", color: "danger", type: "zoom",
                    close: true, maximize: false, id: 'alert-modal', title: this.getRes(6576), msgData: e, width: 600,
                    btns: [{ lib: this.getRes(30), color: 'default', type: 'left' }], datas: this.getRes(7050)
                });

                return false;
            }

            /** ON met la taille du parent. */
            this.$el.style.height = this.$parent.heigthContentTab || this.hDefault;

            /** Ici, c'est sympa. on récupère la grille, depuis le controleur,
             *  on force un mode html en javascript, et on plaque tout dans le
             * dom à la bonne place. */
            if (this.$refs["dvGrille_" + bkmDescId] && this.DataJson) {
                let dvGrille = this.$refs["dvGrille_" + bkmDescId];
                let parser = new DOMParser();
                let el = parser.parseFromString(this.DataJson, "text/html");

                this.reinitBookmarkGrid(bkmDescId);

                dvGrille.appendChild(el.documentElement.querySelector("body").firstChild);

                let gwCont = document.querySelector("#gw-container");
                if (gwCont && ![...dvGrille.childNodes].includes(gwCont)) {
                    dvGrille.appendChild(gwCont);
                    gwCont.style.height = this.$parent.hBaseHeight;

                    let gwTab = document.querySelectorAll("[id^='gw-tab-" + bkmDescId + "'], [id^='gw-bkm-" + bkmDescId + "']");

                    if (gwTab && gwTab.length > 0) {
                        this.activateGrid(gwTab, "1");
                        document.getElementById("firstSubTabItem").removeAttribute("onclick");
                    }
                }
            }
        },

    },
    mounted() {
        var observerLoading = new IntersectionObserver(([e]) => {
            if (e.isIntersecting && e.intersectionRatio == 1) {

                // DEBUT QBO (permet de savoir si la grille en question est déja charger)
                if (e.target.children.length > 0)
                    return;
                // FIN QBO

                this.loadBkmGrid(this.propGrille.DescId);
            }
            else {

                // DEBUT QBO (permet de savoir si la grille en question est active)
                if (this.$parent.$refs[this.propGrille.id].find(a => a.classList.contains("active")))
                    return;
                // FIN QBO
                this.moveGwContainerInTag(document.querySelector('body'));
                this.reinitBookmarkGrid(this.propGrille.DescId);
            }
        }, { threshold: [0, 1] });

        observerLoading.observe(this.$refs["dvGrille_" + this.propGrille.DescId]);

        /**
        * Ici on met un observer. Si on est en suppression du DOM,
        * donc qu'on quitte le mode fiche à l'arrache, par le menu,
        * on recrée la div qui sert à la grille.
        * */
        var observer = new MutationObserver((mutations) => {

            // DEBUT QBO Lance le resize de la grid)
            this.calHeightTabs(this.propGrille);
            // FIN QBO

            mutations.forEach((mutation) => {
                if (mutation.addedNodes.length > 0) {
                    let gwCont = document.querySelector("#gw-container");
                    let dvgrid = [...mutation.addedNodes]
                        .filter(gw => gw.classList && gw.classList.contains("gw-grid"));

                    if (dvgrid && dvgrid.length > 0) {
                        dvgrid.forEach(gr => gr.removeAttribute("style"));
                    }

                    if (gwCont) {
                        /** Modification de la taille du container de Widget */
                        let dvWidget = gwCont.querySelectorAll(".widget-grid-container");
                        if (dvWidget) {
                            let hGrid = window.innerHeight - document.getElementById("mainDiv").offsetTop - 10;

                            [...dvWidget].forEach(wid => {
                                wid.style.height = hGrid + 'px';
                                let wGrd = wid.querySelector("#widget-grid");
                                if (wGrd)
                                    wGrd.setAttribute("w", hGrid)
                            });
                        }
                    }
                }
            });
        });

        observer.observe(this.$refs["dvGrille_" + this.propGrille.DescId], { childList: true, subtree: true });


        let fd = document.getElementById("mainDiv");

        if (fd) {
            /**
            * Ici on met un observer. Si on est en suppression du DOM,
            * donc qu'on quitte le mode fiche à l'arrache, par le menu,
            * on recrée la div qui sert à la grille.
            * */
            var observerMain = new MutationObserver((mutations) => {
                mutations.forEach((mutation) => {
                    if (mutation.removedNodes.length > 0) {
                        let FileDiv = [...mutation.removedNodes]
                            .filter(gw => gw.id && gw.id.includes("fileDiv"));
                        if (FileDiv && FileDiv.length > 0) {
                            this.moveGwContainerInTag(document.querySelector('body'));
                            top.oGridManager.reset();
                        }
                    }
                });
            });

            observerMain.observe(fd, { childList: true, subtree: true });
        }

    },
    created() {

        addScriptText(`
        /**
         * Les actions sur la grille, au rafraischissement.
         * @param {any} event l'evenement appelant
         * @param {int} descid l'identifiant de table de la grille.
         */
        function setActionOnGrid(event, descid, firstLoad) {
            let bRefresh = getAttributeValue(event.target, 'action').toLowerCase() == 'refresh';
            
            let gwCont = document.querySelector('#gw-container');

            if (!gwCont){
                gwCont = document.createElement("div");
                gwCont.id = "gw-container";
                gwCont.className = "fs_8pt";
            }

            if(bRefresh || firstLoad)
                document.querySelector('body').appendChild(gwCont);

            if (firstLoad) {
                let SubTabMenuCtnr = document.getElementById("SubTabMenuCtnr");
                
                if(SubTabMenuCtnr)
                    SubTabMenuCtnr.setAttribute("ctn", "bkm_" + descid)

                oGridToolbar.load(event);
                event.target.removeAttribute("onclick");
            }
            else {oGridToolbar.click(event);}

            if (bRefresh || firstLoad) {
                let dvGrille = document.querySelector('#dvGrille_' + descid);

                if(dvGrille)
                    dvGrille.appendChild(gwCont);

                setTimeout(() => {
                    let gwTab = document.querySelectorAll("[id^='gw-tab-" + descid +"'], [id^='gw-bkm-" + descid +"']");
                    if (gwTab && gwTab.length > 0) {
                        gwTab.forEach((x, i) => {
                            gwTab[i].setAttribute("active", "1");
                            if (x.childNodes && x.childNodes.length > 0)
                                gwTab[i].childNodes.forEach((nd) => {
                                    nd.setAttribute("active", "1");
                                    nd.removeAttribute("style");
                                });
                            else
                                gwTab[i].remove();
                        });
                    }
                }, 500);
            }

        }`, "scrSetActionOnGrid", this.$refs.navTabsCustom);
    },

};