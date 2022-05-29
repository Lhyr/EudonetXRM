/// <reference path="eTools.js" />
/// <reference path="eMailing.js" />



/******************************************************* *
 * MOU-27/06/2014 : Objet qui traite la creation du formulaire *
 *
 *******************************************************/
function eFormular() {

    var me = this;

    //attribut privés
    this.debug = true;

    //parametre de formulaire
    this.aParams = new Array();
    this.iParamLength = 0;
    this.bDateConfirmed = false;
    this._oUpdater = null;

    ///permissions view et update
    this.oPerm = new ePermission();

    this.GetPerm = function () {
        return me.oPerm;
    }

    this.Init = function () {
        me.InitUpdater();
    }

    this.update = function (CKEvent) {
        //alert("Mise à jour : " + oMemo);
    }

    /*Private*/
    this.InitUpdater = function () {
        me._oUpdater = new eUpdater("mgr/eFormularManager.ashx", 0);
        me._oUpdater.ErrorCallBack = me.ManageReturnedError;
    }


    this.InjectCSS = function (momoId, cssKey)
    {      
        try {
            var memo = nsMain.getMemoEditor(momoId);           
            memo.injectCSS(this.GetParam(cssKey));

        } catch (ex) {
            
        }

    }

    /// Gestion des erreurs de retour
    this.ManageReturnedError = function (oErr) {

        top.eAlert(0, top._res_416, top._res_516, top._res_1806, 450, 180, me.Close);
        setWait(false);
    }

    /// poste les donnée du "formulaire" 
    this.Save = function (bClose) {
        if (bClose == null)
            bClose = false;

        //on controle la dernière etape avant le post
        if (!me.ControlStep(iCurrentStep)) //iCurrentStep depuis eWizard.js
            return;

        me.InitUpdater();
        // 1 : Sauvegarde 
        me._oUpdater.addParam('operation', "1", "post");

        //ajou de parametres de formulaire
        for (key in this.aParams) {
            me._oUpdater.addParam(key, me.GetParam(key), "post");
        }

        setWait(true);
        me._oUpdater.send(me.Feedback, bClose);
    }

    this.SaveAndExit = function () {
        me.Save(true);
    }

    /// Annule la création ou la modification du formulaire
    this.Cancel = function (bClose) {
        if (bClose == null)
            bClose = true;

        me._closeWizard = bClose;

        //on controle la dernière etape avant le post
        //if (!me.ControlStep(iCurrentStep)) //iCurrentStep depuis eWizard.js
        //    return;

        /// 57 513 : On supprime l'éventuelle image de diffusion ajoutée si le formulaire est en création (Id 0) ou si l'image indiquée dans le champ input n'est pas celle indiquée en base (upload effectué mais modification de formulaire annulé)
        me.DeleteSharingImage(null, bClose, false);
    }

    this.onSharingImageChange = function (e, sharingImageValue) {
        // On stocke la précédente image utilisée à chaque modification, afin de pouvoir la supprimer à la validation du formulaire lorsqu'on vide le champ

        if (!sharingImageValue) {
            var oImg = document.getElementById("sharingImage");
            if (oImg != null)
                sharingImageValue = oImg.value;
        }

            if (sharingImageValue && sharingImageValue != '')
                me.addParam("previousSharingImage", sharingImageValue);
    }
 
    this.DeleteSharingImage = function (e, bClose, bCloseUploadWindow) {
        if (bClose == null)
            bClose = false;

        // Le formulaire n'étant pas forcément créé, on stocke l'URL de l'image de diffusion actuellement renseignée sur le wizard 
        // afin de pouvoir la supprimer.
        // En modification, cela permettra de vérifier si l'image indiquée dans le champ d'upload est différente de celle en base. Auquel cas, on supprimera l'image indiquée dans le champ
        var sharingImage = document.getElementById("sharingImage");
        if (sharingImage != null && sharingImage.value != '')
            me.addParam("previousSharingImage", sharingImage.value);

        if (bCloseUploadWindow)
            me.addParam("closeuploadwindow", "1");

        me.InitUpdater();
        // 7 : Suppression d'image de diffusion
        me._oUpdater.addParam('operation', "7", "post");

        //ajou de parametres de formulaire
        for (key in this.aParams) {
            me._oUpdater.addParam(key, me.GetParam(key), "post");
        }

        setWait(true);
        me._oUpdater.send(me.Feedback, bClose);
    }

    var myAlert;

    ///<summary>
    /// feedback 
    ///<summary>
    this.Feedback = function (oDoc, bClose) {

        var failure = getXmlTextNode(oDoc.getElementsByTagName("success")[0]) != "1";
        var message = getXmlTextNode(oDoc.getElementsByTagName("message")[0]);
        var detail = getXmlTextNode(oDoc.getElementsByTagName("detail")[0]);
        var closeWizard = getXmlTextNode(oDoc.getElementsByTagName("closewizard")[0]) == "1";
        var id = getNumber(getXmlTextNode(oDoc.getElementsByTagName("formularid")[0]));

        me._success = !failure;
        me._closeWizard = closeWizard;

        setWait(false);

        me.SetParam("id", id);

        // Si on affiche un message à l'utilisateur, on câble la fermeture de l'assistant à la fermeture du message
        if (message != '') {
            myAlert = eAlert(failure ? 0 : 1, top._res_5080, message, detail, 500, 180, bClose ? me.Close : null);
        }
        // Sinon, on ferme directement la fenêtre
        else {
            if (bClose)
                me.Close();
        }

        if (!failure && !bClose)
        {
            var rewrittenurl = getXmlTextNode(oDoc.getElementsByTagName("rewrittenurl")[0]);
            var hiddenrewrittenurl = getXmlTextNode(oDoc.getElementsByTagName("hiddenrewrittenurl")[0]);
            var twittersharerlink = getXmlTextNode(oDoc.getElementsByTagName("twittersharerlink")[0]);
            var facebooksharerlink = getXmlTextNode(oDoc.getElementsByTagName("facebooksharerlink")[0]);
            var linkedinsharerlink = getXmlTextNode(oDoc.getElementsByTagName("linkedinsharerlink")[0]);
            var sharingimage = getXmlTextNode(oDoc.getElementsByTagName("sharingimage")[0]);

            me.RefreshRewrittenURL(rewrittenurl, hiddenrewrittenurl);
            me.DisplayRewrittenURL();
            me.RefreshSharingButtonUrl("twitter", twittersharerlink);
            me.RefreshSharingButtonUrl("facebook", facebooksharerlink);
            me.RefreshSharingButtonUrl("linkedin", linkedinsharerlink);
            me.DisplaySharingButton();

            document.getElementById("sharingImage").value = sharingimage;
            me.onSharingImageChange(sharingimage);
        }

        var closeuploadwindow = getXmlTextNode(oDoc.getElementsByTagName("closeuploadwindow")[0]);
        if (closeuploadwindow && typeof (top.modalImage) != "undefined") {
            top.modalImage.hide();
            top.setWait(false);
        }
    }

    //On ferme l'assistant
    this.Close = function () {

        if (myAlert && myAlert.hide)
            myAlert.hide();

        //En cas de success ou une demande explicite de serveur de fermer l'assistant(visualisation non autorisée)
        //on ferme l'assistant



        if (me._success || me._closeWizard)
            if (top) {

                if (top.window['_md']['FormularWizard']) {
                    top.window['_md']['oModalFormularList'].getIframe().ReloadList();
                    try {
                        top.window['_md']['FormularWizard'].hide();
                        top.setWait(false);
                    }
                    catch (e) {

                    }
                }
            }

    }

    this.GetType = function () { return 'formular'; }
    this.ControlStep = function (step) {

        switch (parseInt(step)) {
            case 1:
                memo = nsMain.getMemoEditor('edtBodyMemoId');
                me.SetParam("body", memo.getData());
                me.SetParam("bodycss", memo.getCss());
                return true;
            case 2:

                var bRedirectUrl = document.getElementById("cbo-url-id").checked;
                if (bRedirectUrl) {
                    var url = document.getElementById("input-url-id").value;
                    if (eValidator.isUrl(url)) {

                        me.SetParam("redirecturl", url);
                        me.SetParam("submitbody", "");
                        me.SetParam("submitbodycss", "");

                    } else {

                        eAlert(0, top._res_6275, top._res_6717, url);
                        return false;
                    }

                } else {

                    memo = nsMain.getMemoEditor('edtSubmitMemoId');
                    me.SetParam("submitbody", memo.getData());
                    me.SetParam("submitbodycss", memo.getCss());
                    me.SetParam("redirecturl", "");
                }

                return true;
            case 3:

                //sauvegarde
                var saveas = document.getElementById("input-save-id").value + "";
                if (saveas.length == 0 || saveas.trim().length == 0) {
                    eAlert(0, top._res_5080, top._res_372, top._res_1722);
                    return false;
                }

                me.SetParam("label", saveas);

                //options
                var isUniqueSubmit = getAttributeValue(document.getElementById("unique-submit"), "chk") == "1";
                var multipleSubmit = document.getElementById("input-multiple-submit").value;;

                me.SetParam("unique", isUniqueSubmit ? "1" : "0");
                me.SetParam("alreadylabel", isUniqueSubmit ? multipleSubmit : "");

                var expirateDate = document.getElementById("expire-date").value + "";
                if (expirateDate.length == 0 || expirateDate.trim().length == 0) {
                    eAlert(0, top._res_5080, top._res_372, top._res_1722);
                    return false;
                }

                var expirateMsg = document.getElementById("input-expirate-msg").value;

                me.SetParam("expiredate", expirateDate);
                me.SetParam("expiratelabel", expirateDate.length > 0 ? expirateMsg : "");

                // Balises meta

                //var metatags = document.getElementById("metaTags").value;
                //me.SetParam("metatags", metatags);

                var sharingTitle = document.getElementById("sharingTitle");
                if (sharingTitle != null)
                    me.SetParam("sharingTitle", sharingTitle.value);
                var sharingDescription = document.getElementById("sharingDescription");
                if (sharingDescription != null)
                    me.SetParam("sharingDescription", sharingDescription.value);
                var sharingImage = document.getElementById("sharingImage");
                if (sharingImage != null)
                    me.SetParam("sharingImage", sharingImage.value);

                // Partages réseaux sociaux

                var facebookShare = document.getElementById("facebookShare");
                me.SetParam("facebookshare", getAttributeValue(facebookShare, "chk"));
                var googleShare = document.getElementById("googleShare");
                me.SetParam("googleshare", getAttributeValue(googleShare, "chk"));
                var twitterShare = document.getElementById("twitterShare");
                me.SetParam("twittershare", getAttributeValue(twitterShare, "chk"));
                var linkedinShare = document.getElementById("linkedinShare");
                me.SetParam("linkedinshare", getAttributeValue(linkedinShare, "chk"));

                //sécurité : parametres de permission depuis l'objet oPerm

                var modalWizard = top.window['_md']['FormularWizard'];
                var childwindow = modalWizard.getIframe();

                var bPublic = getAttributeValue(childwindow.document.getElementById("chk_OptPublicFilter"), "chk") == "1";

                me.oPerm.SetPublic(bPublic);

                var objRetValue = getPermReturnValue("View", childwindow);
                me.oPerm.SetViewPermParam("level", objRetValue.levels);
                me.oPerm.SetViewPermParam("user", objRetValue.users);
                me.oPerm.SetViewPermParam("mode", objRetValue.perMode);

                var objRetValue = getPermReturnValue("Update", childwindow);
                me.oPerm.SetUpdatePermParam("level", objRetValue.levels);
                me.oPerm.SetUpdatePermParam("user", objRetValue.users);
                me.oPerm.SetUpdatePermParam("mode", objRetValue.perMode);

                me.oPerm.AppendParams(me);

                //Comparaison de timestamp

                //Comparaison de dates



                var parts = eDate.ConvertDisplayToBdd(expirateDate).split("/");
                if (parts.length < 2) {
                    eAlert(0, top._res_5080, "Format de date non valide", "Format de date non valide");
                    return false;
                }

                var expirateDay = new Date(parseInt(parts[2]), parseInt(parts[1]) - 1, parseInt(parts[0]), 0, 0, 0, 0);
                var toDay = new Date();

                //faire une difference 
                if (expirateDay <= toDay && !me.bDateConfirmed) {

                    me.bDateConfirmed = false;
                    eConfirm(1, top._res_201, top._res_6718, top._res_6719, 450, 200,
                        function () {
                            me.Save();
                            me.bDateConfirmed = true;
                        });
                    return false;
                }
                return true;
        }
        return true;
    }

    //traitements à executer au moment du chargement de l'etape
    this.SwitchStep = function (step) {

    }
    this._oModalLnkFile = null;
    //Prévisualisation
    this.Preview = function () {
        setWait(true);
        var nStep = 1;
        if (iCurrentStep == 2)
            nStep = 2;
        //on controle la dernière etape avant le post
        if (!me.ControlStep(nStep)) //iCurrentStep depuis eWizard.js
            return;

        me._oModalLnkFile = new eModalDialog(top._res_1142, 0, 'mgr/eFormularManager.ashx');  //TODORES
        //1142
        me._oModalLnkFile.ErrorCallBack = me.ManageModalReturnedError;
        // 1 : Sauvegarde 
        me._oModalLnkFile.addParam("operation", "6", "post");
        me._oModalLnkFile.addParam("step", nStep, "post");

        //ajout de parametres de formulaire
        for (key in this.aParams) {
            me._oModalLnkFile.addParam(key, me.GetParam(key), "post");
        }
        // 41590 CRU
        me._oModalLnkFile.addParam("iframeScrolling", "yes", "post");

        me._oModalLnkFile.onIframeLoadComplete = me.PreviewReturn;

        me._oModalLnkFile.show();


        me._oModalLnkFile.addButton(top._res_30, null, "button-gray", null, "cancel"); // Fermer



    }


    ///summary
    /// Méthode de callBack de la recherche sur un champ catalogue, depuis la MRU
    ///Traite les éléments de retours pour construire la liste des valeurs et la passer à renderValues
    ///<param name="oRes"></param>
    ///<param name="jsVarName"></param>
    ///<param name="bSilent"></param>
    ///summary
    this.PreviewReturn = function () {

        setWait(false);

    }
    this.ManageModalReturnedError = function () {
        setWait(false);
        me._oModalLnkFile.hide();
    }

    /// Retourne la valeur associée à la clé
    this.GetParam = function (key) {
        if (key == null)
            return null;
        if (this.KeyExists(key))
            return this.aParams[key];
        else
            return null;
    }

    ///Vérifie dans le dictionnaire des paramètres si la clé passée en paramètre existe
    this.KeyExists = function (key) {
        return (key in this.aParams);
    }

    ///Affecte un paramètre simple(valeur unique associée à la clé) 
    this.SetParam = function (key, val) {
        if (this.KeyExists(key))
            this.aParams[key] = val;

    }

    ///ajoute un parametre dans le dico
    this.addParam = function (key, val, requestType) {
        if (!this.KeyExists(key))
            this.iParamLength++;
        this.aParams[key] = val;
    }

    ///Retourne la taille du dictionnaire interne de paramètres
    this.ParamLength = function () {
        var length = 0, key;
        for (key in this.aParams) {
            if (this.aParams.hasOwnProperty(key)) length++;
        }
        return length;
    };

    ///Charge le dictionnaire de paramètres internes à partir du paramètre transmit
    this.LoadParam = function (aFormParams) {
        this.aParams = aFormParams;
        iParamLength = this.ParamLength();
    }

    ///
    this.DisplayRewrittenURL = function () {
        var liRwUrl = document.getElementById("liRwUrl");
        if (liRwUrl != null)
            liRwUrl.style.removeProperty("display");
    }

    ///
    this.RefreshRewrittenURL = function (rewrittenurl, hiddenrewrittenurl) {
        var rwUrl = document.getElementById("rwUrl");
        if (rwUrl != null)
            rwUrl.value = rewrittenurl;

        var hRwUrl = document.getElementById("hRwUrl");
        if (hRwUrl != null)
            hRwUrl.value = hiddenrewrittenurl;
    }

    ///
    this.DisplaySharingButton = function () {
        var sharingButtons = document.getElementById("sharingButtons");
        if (sharingButtons != null)
            sharingButtons.style.removeProperty("display");
    }

    ///
    this.RefreshSharingButtonUrl = function (shareType, url) {
        var link = null;
        switch (shareType) {
            case "twitter":
                link = document.getElementById("twitterShareButtonLink");
                break;
            case "facebook":
                link = document.getElementById("facebookShareButtonLink");
                break;
            case "linkedin":
                link = document.getElementById("linkedinShareButtonLink");
                break
            default:
                break;
        }

        if (link != null && link.tagName == "A") {
            link.setAttribute("href", url);
        }
    }

    // Fonction exécutée sur le onchange du libellé du formulaire
    // Met à jour l'URL et fait un SetParam
    this.SaveAsOnChange = function () {
        var strLabel = document.getElementById('input-save-id').value;
        this.UpdateURL(strLabel);

        if (strLabel.trim().toLowerCase() != this.GetParam("label").trim().toLowerCase())
            this.SetParam("saveas", "1");
    }

    this.UpdateURL = function (label) {
        // Modif de l'URL
        if (document.getElementById('rwUrl') && document.getElementById('hRwUrl')) {
            document.getElementById('rwUrl').value = document.getElementById('hRwUrl').value.replace('formularlabel', top.convertStringToRewrittenUrl(label));
        }
    }
}

