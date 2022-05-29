import { eFileComponentsMixin } from '../../../mixins/eFileComponentsMixin.js?ver=803000'
import { updateListVal, showInformationIco, displayInformationIco } from '../../eComponentsMethods.js?ver=803000';
import { getTabDescid } from "../../eMainMethods.js?ver=803000";

/**
 * Mixin commune aux composants eImage.
 * */
export const eImageMixin = {
    mixins: [eFileComponentsMixin],
    data() {
        return {
            modif: false,
            that: this
        };
    },
    mounted() {
        this.displayInformationIco();

    },
    methods: {
        displayInformationIco,
        showInformationIco,
        getTabDescid,
        //updateListVal,
        OpenModalPicture(elem) {
            // Données transmises au contrôleur
            let TabDescId = this.getTabDescid(this.dataInput.DescId);
            //let fileId = this.dataInput.FileId;
            let oImg = elem.target;
            //let sName = "COL" + TabDescId + "_" + fileId;
            //KHA n'est utile que pour l'appel depuis une edition de memo / mail / formulaire etc.
            //let sFrom = oImg.hasOwnProperty('editorType') ? oImg.editorType : "";
            let strType = 'IMAGE_FIELD';

            //setAttributeValue(elem.target, "COL" + TabDescId + "_" + fileId);

            //KHA bloc de code pour les formulaires 
            //- A traiter ultérieurement ou à supprimer si le présent component n'est pas réutilisé

            //if ((sFrom == "") && (oImg.name == "sharingImage"))
            //    sFrom = "sharringimage";

            if (this.dataInput.DescId % 100 == 75) {
                /** SI le numéro termine par 75. */
                strType = TabDescId == "101000" ? 'USER_AVATAR_FIELD' : 'AVATAR_FIELD';
            }
            //KHA ancienne mise en page --> ne sera jamais utilisé en IrisBlack
            //else if (oImg.id == "vcCadre") {
            //    strType = 'OLD_AVATAR_FIELD'; /* Avatar de PP/PM avant nouvelle mise en page */
            //}

            this.doGetImageGeneric(oImg, strType/*, sFrom*/);
        },

        // Fonction permettant d'appeler la fenêtre d'ajout d'image (photo, avatar, champ Image, e-mailing...)
        doGetImageGeneric(oImg, strType/*, sFrom*/) {

            // CRU : Empêcher la modification de l'image si le champ est en lecture seule
            if (this.dataInput.ReadOnly == true)
                return;

            var bDisplayDeleteBtn = true;
            var deleteImageFct = this.deleteImage;
            top.setWait(true);
            try {
                /*********************** Titre de la modale ***********************/
                var strModalDialogTitle = this.getRes(6286); // Insérer une image depuis votre ordinateur
                if (strType == 'AVATAR')
                    strModalDialogTitle = this.getRes(6180); // Télécharger votre avatar
                else if (strType == 'IMAGE_FIELD' || (strType == "AVATAR_FIELD")) {
                    strModalDialogTitle = this.dataInput.Label;
                }

                /*********************** Taille de la modale ***********************/
                // #32 312 - Taille de la fenêtre = 160 + marge pour afficher l'image "Introuvable" renvoyée par le navigateur si l'image est impossible à charger
                var initialWindowWidth = 460; // valeur à modifier également dans eImageDialog.aspx
                var initialWindowHeight = 180; // valeur à modifier également dans eImageDialog.aspx

                top.modalImage = new eModalDialog(strModalDialogTitle, 0, "eImageDialog.aspx", initialWindowWidth, initialWindowHeight);
                // On mémorise la taille que l'on souhaitait initialement affecter à la fenêtre dans deux variables JS, que l'on rattachera en propriétés de
                // l'objet eModalDialog afin que la page chargée à l'intérieur puisse déclencher son redimensionnement si elle masque le conteneur d'image
                // dans le cas où il n'y a aucune image à charger
                top.modalImage.initialWindowWidth = initialWindowWidth;
                top.modalImage.initialWindowHeight = initialWindowHeight;
                top.modalImage.addParam("ImageType", strType, "post");
                //KHA dans le cas d'un champ de type Image CalledFrom peut ne pas être renseigné
                //top.modalImage.addParam("CalledFrom", sFrom, "post");


                /*********************** RECUPERATION DE CERTAINS PARAMETRES EN FONCTION DU TYPE D'IMAGE A GERER ***********************/

                var descId, fileId;
                descId = this.dataInput.DescId;
                fileId = this.dataInput.FileId;

                // Contexte du champ Mémo à mettre à jour : pour l'insertion dans des champs Mémo uniquement
                // KHA tout le bloc suivant est annulé pour l'instant car hors de propos (edition d'image depuis memo, mail, formulaire, etc.)
                // on se concentre sur les champs de type Image
                if (false) {
                    //if (strType == 'MEMO' || strType == 'MEMO_SETDIALOGURL') {

                    //    top.modalImage.parentMemoEditor = oImg;
                    //    //descId = getAttributeValue(objHeaderCell, "did");
                    //    //// Récupération du fileId en mode Fiche
                    //    //fileId = this.imgGetCurrentFileId(oImg);

                    //}
                    //else if (strType == 'TXT_URL') {

                    //    top.modalImage.parentMemoEditor = oImg;
                    //    //descId = getAttributeValue(objHeaderCell, "did");
                    //    //// Récupération du fileId en mode Fiche
                    //    //fileId = this.imgGetCurrentFileId(oImg);

                    //    // Seule la suppression des images de diffusion d'un formulaire est gérée pour ce type d'image
                    //    // L'action consiste simplement à vider le champ sur la fenêtre parente, et c'est à l'annulation ou à l'enregistrement de celle-ci que la suppression sera effectuée
                    //    if (oImg.id == "sharingImage") {
                    //        bDisplayDeleteBtn = true;
                    //        deleteImageFct = function () {
                    //            if (oFormular) {
                    //                oFormular.onSharingImageChange(document.getElementById("sharingImage").value);
                    //            }
                    //            document.getElementById("sharingImage").value = '';
                    //        };
                    //    }
                    //    else
                    //        bDisplayDeleteBtn = false;

                    //}
                }

                if (strType == "AVATAR" || strType == "USER_AVATAR_FIELD") {

                    if (strType == "AVATAR" || strType == "USER_AVATAR_FIELD")
                        descId = "101075";

                    top.modalImage.addParam("DescId", descId, "post");
                    top.modalImage.addParam("FileId", fileId, "post");
                }
                // Contexte du champ à mettre à jour : pour les champs Image uniquement
                else if (strType == 'IMAGE_FIELD' || (strType == "AVATAR_FIELD")) {

                    top.modalImage.addParam("DescId", descId, "post");
                    top.modalImage.addParam("FileId", fileId, "post");

                    // Taille de l'image à gérer : pour l'avatar et les champs Image
                    // Récupération du contexte (mode Fiche/mode Liste/mode Signet)
                    //KHA non applicable dans IRIS
                    //var strContext = '';
                    //if (!document.getElementById("fileDiv_" + nGlobalActiveTab))
                    //    strContext = "list";
                    //else {
                    //    var nTab = GetMainTableDescId(getAttributeValue(oImg, 'ename'));
                    //    var oCurTab = document.getElementById("mt_" + nTab);
                    //    if (getAttributeValue(oCurTab, "ednmode") == "bkm")
                    //        strContext = "bkm";
                    //    else
                    //        strContext = "file";
                    //}
                    // Récupération de la taille à partir des propriétés de l'image si existantes
                    var width = NaN;
                    var height = NaN;
                    //KHA non applicable dans IRIS
                    //if (oImg.style) {
                    //    width = getNumber(oImg.style.width);
                    //    height = getNumber(oImg.style.height);
                    //}
                    //if ((isNaN(width) && strContext == "file")) {
                    //    width = 0;
                    //}
                    //else if ((isNaN(width) || width == "") && oImg.firstChild && oImg.firstChild.tagName.toLowerCase() == "img") {
                    //    width = oImg.firstChild.style.width || oImg.firstChild.width;
                    //}

                    //if ((isNaN(height) || height == "") && oImg.firstChild && oImg.firstChild.tagName.toLowerCase() == "img") {
                    //    height = oImg.firstChild.style.height || oImg.firstChild.width;
                    //}
                    //// Si la taille n'a pas pu être récupérée, pour le mode Liste ou Signet, on utilise la taille Vignette
                    //if ((isNaN(width) || isNaN(height) || width == "" || height == "") && (strContext != "file")) {
                    //    if ((isNaN(width) || width == ''))
                    //        width = 16;
                    //    if ((isNaN(height) || height == ''))
                    //        height = 16;
                    //}

                    if (this.propSignet != undefined) {
                        width = 16;
                        height = 16;
                    }
                    //KHA voyons ce que ça donne sans déjà...
                    //top.modalImage.addParam("ImageWidth", width, "post");
                    //top.modalImage.addParam("ImageHeight", height, "post");
                }


                //KHA : ce bloc est destiné à gérer les valeurs temporaires dans le cas où on n'est pas en updateonblur
                //if (oImg.querySelector) {
                //    var myImpg = oImg.querySelector("img");
                //    if (getAttributeValue(myImpg, "isb64") == "1") {
                //        top.modalImage.addParam("isb64", "1", "post");
                //        top.modalImage.addParam("b64val", getAttributeValue(myImpg, "src"), "post");
                //    }
                //}


                // AUTRES PARAMETRES

                top.modalImage.addParam("modalVarName", "modalImage", "post");
                //KHA : pas de popup pour l'instant
                //top.modalImage.addParam("parentIsPopup", isPopup() ? "1" : "0", "post");
                top.modalImage.addParam("parentIsPopup", "0", "post");
                //KHA : pour l'instant on est toujours en updateonblur (mise à jour en sortie de champ)
                //top.modalImage.addParam("updateOnBlur", isUpdateOnBlur() ? "1" : "0", "post");
                top.modalImage.addParam("updateOnBlur", "1", "post");
                top.modalImage.sourceObj = oImg;

            }
            finally {
                top.setWait(false);
            }
            top.setWait(true);
            top.modalImage.ErrorCallBack = function () { top.setWait(false); }
            top.modalImage.onIframeLoadComplete = function () { top.setWait(false); };
            top.modalImage.show();


            /*********************** BOUTONS ***********************/
            top.modalImage.addButton(this.getRes(29), this.onImageCancel, "button-gray", this.jsVarName, "btnCancel"); // Annuler

            if (bDisplayDeleteBtn)
                top.modalImage.addButton(this.getRes(19), deleteImageFct, "button-red", this.jsVarName, "btnDelete"); // Supprimer 

            var sendToUpdate = () => {
                this.dataInput.Value = document.querySelector('[id^="frm"]').contentWindow.document.querySelector("#filMyFile").files[0].name;
                updateListVal(this, "", this.dataInput.Value, "");
            }

            top.modalImage.addButton(this.getRes(28), function () { sendToUpdate(); sendImageDialogForm(oImg); }, "button-green", this.jsVarName, "btnSend"); // Valider

        },


        deleteImageIris() {
            var oFrmButton = null;
            if (top.modalImage && typeof (top.modalImage.getIframe) == "function") {
                var oFrm = top.modalImage.getIframe();
                if (oFrm && oFrm.window && oFrm.window.document) {
                    oFrmButton = oFrm.window.document.getElementById("cmdDelete");
                }
            }

            if (oFrmButton) {
                //eConfirm(1, this.getRes(6347), top._res_6348, '', 500, 200, // eConfirm précédemment utilisé sur eGoogleImageGet avec des libellés différents - A VALIDER
                eConfirm(1, this.getRes(29), this.getRes(1225), '', 450, 200,
                    function () {
                        top.setWait(true); oFrmButton.click();
                    },
                    function () { return false; });
            }
        },

        ErrorImageDeleteReturn(oRes) {
            top.setWait(false);
        },

        ImageDeleteReturn(oRes, engineObject, afterUpload) {
            top.setWait(false);
        },

        // Cette fonction est conservée dans eMain.js, vu qu'elle est déclenchée depuis eImageDialog.aspx
        /*
        onImageSubmit() {
        },
        */

        /*Fermer la fenêtre au clique sur annuler*/
        onImageCancel() {
            if (top.setWait != null && typeof (top.setWait) == 'function')
                top.setWait(false);

            top.window['modalImage'].hide();
        },
    },
    props: ["dataInput", "propHead", "propListe", "propSignet", "propIndexRow", "propAssistant", "propDetail", "propAssistantNbIndex"],
}