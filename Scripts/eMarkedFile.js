
/**********************************/
/*  Classe eMarkedFile            */
/*  GCH & SPH                     */
/**********************************/
function eMarkedFileEditor(jsVarName) {

    var that = this;

    this.jsVarName = jsVarName;
    this.modalMarkedFile = null;
    this.value = null;
    this.confirm = false;
    this.modalMarkedFileTab = top.nGlobalActiveTab;
    

    /****************************************************************************/
    /* Actions sur les sélections de fiches marquées via le bouton "Action"     */
    /****************************************************************************/
    this.markedFile = function (nMode, oContextMenu) {

        //CallBack par défaut
        if (oContextMenu != null) {
            var callBackFct = function () {
                oContextMenu.hide();
                top.firstpage(this.modalMarkedFileTab);
            };
        }
        else {
            var callBackFct = function () {
                top.firstpage(this.modalMarkedFileTab);
            };
        }

        var callBackFuncMarkedFile;
        nMode = Number(nMode);

        //paramètres
        if (nMode < 0 || nMode > 7 || Number(this.modalMarkedFileTab) <= 0)
            return;

        //Paramètres communs
        var oMarkedFileManager = new eUpdater("mgr/eMarkedFilesManager.ashx", 0);
        oMarkedFileManager.ErrorCallBack = function () { };
        oMarkedFileManager.addParam("tab", this.modalMarkedFileTab, "post");
        oMarkedFileManager.addParam("type", nMode, "post");

        //Paramètres Spécifiques
        callBackFuncMarkedFile = '';
        switch (nMode) {

            case 0: // Afficher tout
                callBackFuncMarkedFile = callBackFct;
                break;
            case 1: // Afficher seulement la sélection
                callBackFuncMarkedFile = callBackFct;
                break;
            case 2: // Enregistrer la sélection                
                oMarkedFileManager.addParam("label", this.value, "post");

                if (this.confirm)
                    oMarkedFileManager.addParam("confirm", "1", "post");

                callBackFuncMarkedFile = that.afterSave;
                break;
            case 3: // Coche/décoche une fiche - gérer ailleurs
                break;
            case 4: //ajoute/retire une page - gérer ailleurs
                break;
            case 5: // Supprime la sélection

                oMarkedFileManager.addParam("markedFileId", this.value, "post");
                //callBackFuncMarkedFile = that.afterDeleteMF;
                callBackFuncMarkedFile = launchInContext(that,  that.afterDeleteMF);
                break;
            case 6: // Renomme
                break;
            case 7: // Charger sélection
                that.modalMarkedFile.hide();
                oMarkedFileManager.addParam("markedFileId", this.value, "post");
                callBackFuncMarkedFile = that.afterLoad;
                break;
        }


        if (typeof (callBackFuncMarkedFile) == 'function')
            oMarkedFileManager.send(callBackFuncMarkedFile);

    };

    /*********************************************************************************/
    /*  Fonctions d'actions liées  à la fenêtre modal de sélection de fiches marquées     */
    /*********************************************************************************/

    //Retiens la sélection clicker
    this.setSelect = function (nid) {
        that.value = nid;
    }

    //Suprime la sélection de fiche marquées
    // TODO : Gestion des ressources (n 5056)
    this.delMarkedFileSel = function (nid) {
        that.value = nid;
        that.confirm = true;
        that.markedFile(5);
    }

    //Charge la sélection de fiche marquée
    this.loadMarkedFileSel = function (nid) {

        //loadMarkedFileSel peut être appeler sans param (si click sur valider). le click sur la ligne charge that.value
        if (nid && typeof (nid) == "number" && Number(nid) > 0)
            that.value = nid;

        // Ne charge la liste que si that.value est renseigné
        if (typeof (that.value) == "number" && Number(that.value) > 0)
            that.markedFile(7);
    }

    //sauvegarde la sélection de fiche marquée
    this.saveMarkedFileSel = function () {
        var objModalDoc = that.modalMarkedFile.getIframe().document;
        var sLabel = objModalDoc.getElementById("txtMarkedFileName").value;
        that.value = sLabel;
        that.markedFile(2);
    }

    //Ferme la fenêtre de selections de la liste des fiches marquées
    this.closeMarkedFileSelWin = function () {
        if (that.modalMarkedFile && that.modalMarkedFile.hide)
            that.modalMarkedFile.hide();
    }

    /****************************************/
    /*  Fonction de callback                */
    /****************************************/

    //Supprime la ligne de selection sur la liste modal
    this.afterDeleteMF = function (oRes) {


     
        if (getXmlTextNode(oRes.getElementsByTagName("reload")[0]) == "1")
            top.firstpage(this.modalMarkedFileTab);

        if (getXmlTextNode(oRes.getElementsByTagName("success")[0]) == "1") {
            var nId = getXmlTextNode(oRes.getElementsByTagName("deleted")[0]);
            

            var oTrRemove = top.window['_md']["markedDialog"].getIframe().document.getElementById("eMfId" + nId);
            
            oTrRemove.parentNode.removeChild(oTrRemove);
            var oTable = top.window['_md']["markedDialog"].getIframe().document.getElementById("markedFileTab");
            var oTrs = oTable.getElementsByTagName("TR");

            bSwitch = false;
            for (var nCmpt = 0; nCmpt < oTrs.length; nCmpt++) {
                var oCurrTr = oTrs[nCmpt];
                if (bSwitch)
                    removeClass(oCurrTr, 'eMarkedFileDialogValuesAltColor');
                else
                    addClass(oCurrTr, 'eMarkedFileDialogValuesAltColor');
                bSwitch = !bSwitch;
            }
        }

        top.updtMarkedCnt(oRes);
        that.closeMarkedFileSelWin();
    }

    //After Load
    this.afterLoad = function (oRes) {
        top.firstpage(this.modalMarkedFileTab);
        top.updtMarkedCnt(oRes);
        that.closeMarkedFileSelWin();
    }

    //Après sauvegarde
    this.afterSave = function (oRes) {

        var bNeedConfirm = (getXmlTextNode(oRes.getElementsByTagName("needconfirm")[0]) == "1");
        if (!bNeedConfirm) {
            that.closeMarkedFileSelWin();
            top.updtMarkedCnt(oRes);
        }
        else {
            eConfirm(1, top._res_118, top._res_6220, '', 500, 200, function () { that.confirm = true; that.markedFile(2);  });
        }
    }


    /******************************************************/
    /* Ouvre la fenêtre de sélection des fiches marquées  */
    /*   Type 0 : Enregistrement                            */
    /*   Type 1 : Chargement                            */
    /******************************************************/
    this.openDialog = function (nType) {

        var nWidth = 500;
        var nHeight = 440;

        that.modalMarkedFile = new eModalDialog(top._res_6611, '0', 'eMarkedFileDialog.aspx', nWidth, nHeight, "markedDialog"); // Sélections des fiches marquées
        that.modalMarkedFile.addParam("type", nType, "post");

        

        that.modalMarkedFile.ErrorCallBack = launchInContext(this.modalMarkedFile, this.modalMarkedFile.hide);

        that.modalMarkedFile.show();
        //Fonctions (On***Function à implémenter dans la portée de la page de script)         
        that.modalMarkedFile.addButtonFct(top._res_29, that.closeMarkedFileSelWin, 'button-gray');

        if (nType == 1)
            that.modalMarkedFile.addButtonFct(top._res_28, that.loadMarkedFileSel, 'button-green');     //Load
        else
            that.modalMarkedFile.addButtonFct(top._res_286, that.saveMarkedFileSel, 'button-green');     //Save



        that.modalMarkedFile.addFunction("goSelect", that.loadMarkedFileSel);
        that.modalMarkedFile.addFunction("setSelect", that.setSelect);
        
    };


    /*  Catalogue  */
    /* TODO : vérifier le fonctionement et ajouter le check de confirmation pour ne pas avoir 2 sélections avec le même nom   */
    this.modalMarkedFile_LblEditor = null;
    this.eMarkedFile_LblEditor = null;
    this.initEditors = function () {
        // Pour le positionnement du efieldeditor, on position le ePopup directement dans le body du document (de iframe)
        //var parentElement = document.getElementById('eCEDValues');

        if (!this.modalMarkedFile_LblEditor) {
            this.modalMarkedFile_LblEditor = new ePopup(jsVarName + '.eCEDLblEditorPopup', '255px', '25px', 0, 0, document.body, false);
            this.eMarkedFile_LblEditor = new eFieldEditor('inlineEditor', this.modalMarkedFile_LblEditor, jsVarName + '.eMarkedFile_LblEditor', 'catEditLbl');
            this.eMarkedFile_LblEditor.action = 'renameMarkedFileValue';
        }
    };

}