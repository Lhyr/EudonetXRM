var oImageManager = (function () {

    /******************* Refresh du logo à la mise à jour *******************/
    function refreshLogo(modal, imgSrc) {
        top.setWait(false);

        if (modal.sourceObj.tagName == "IMG") {
            modal.sourceObj.src = imgSrc;
        }
        top.document.querySelector(".hLogo").style.backgroundImage = "url(" + imgSrc + ")";
        modal.hide();
    }

    /******************* Refresh de l'avatar utilisateur à la mise à jour *******************/
    function refreshUserAvatar(modal, imgSrc) {
        top.setWait(false);

        updateAvatar(top.document.getElementById("hAvatar"), imgSrc);
        updateAvatar(top.document.getElementById("hAvatarFromMyEudonet"), imgSrc);

        modal.hide();
    }

    /******************* Refresh de l'élément avec l'URL *******************/
    function updateAvatar(element, imgSrc) {

        if (element) {
            if (imgSrc != "") {
                var img = findFieldImgElement(element);
                if (img) {
                    img.src = imgSrc + '?t=' + new Date().getTime();
                }
            }
            else {
                element.innerHTML = "<div class='icon-picture-o emptyPictureArea' data-eemptypicturearea='1'></div>";
            }
        }
    }

    /******************* Refresh de l'avatar d'une fiche *******************/
    function refreshAvatarField(modal, imgSrc, storedInSession) {
        top.setWait(false);
        var vc = modal.sourceObj;
        if (vc && vc.id == "vcImg") {
            var image = vc.children[0];
            if (imgSrc != "") {
                image.src = imgSrc;
            }
            else {
                if (vc.className == "vcImgPpFile" || vc.className == "vcImgPp")
                    image.src = "themes/default/images/ui/avatar.png";
                else if (vc.className == "vcImgPmFile" || vc.className == "vcImgPm")
                    image.src = "themes/default/images/iVCard/unknown_pm.png";
            }

            if (storedInSession)
                setAttributeValue(vc, "session", "1");
        }
        
        modal.hide();
    }

    /******************* Affichage du nom de l'avatar *******************/

    function displayAvatarName() {
        var src = document.querySelector('#avatarName');
        var imgPreview = document.querySelector('#imgPreview');
        src.innerHTML = "".concat(top._res_103, ": ").concat(event.target.files[0].name);
        var reader = new FileReader();

        reader.onload = function (e) {
            if(imgPreview)
                imgPreview.src = e.target.result;
        };

        event.target.files[0] && reader.readAsDataURL(event.target.files[0]);
    }

    /******************* Affichage du logo de chargement Avatar *******************/

    function displayAvatarWaiter() {
        var src = document.querySelector('#imgPreview');
        if (src) {
            if (!src.classList.contains("loading")) {
                src.classList.add('loading');
            } else {
                src.classList.remove('loading');
            }
        }
    }


    /******************* Fonctions publiques *******************/
    return {
        init: function () {
            
        },

        refreshLogo: function (modal, imgSrc) {
            refreshLogo(modal, imgSrc);
        },
        refreshUserAvatar: function (modal, imgSrc) {
            refreshUserAvatar(modal, imgSrc);
        },
        refreshAvatarField: function (modal, imgSrc) {
            refreshAvatarField(modal, imgSrc);
        },
        displayAvatarName: function () {
            displayAvatarName();
        },
        displayAvatarWaiter: function () {
            displayAvatarWaiter();
        }
    }

})();