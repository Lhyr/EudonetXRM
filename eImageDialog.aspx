<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eImageDialog.aspx.cs" Inherits=" Com.Eudonet.Xrm.eImageDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <script type="text/javascript">
        var currentImgHeight = 0;
        var currentImgWidth = 0;
        var loadImageTimer = null;
        var loadImageListenerTimer = null;
        var loadImageTimerRetryCount = 0;
        var loadImageListenerTimerRetryCount = 0;
        var maxTimerRetryCount = 2;

        // Définition de la taille par défaut de la fenêtre (réservé à l'affichage des contrôles de modification)
        var minimumWindowWidth = 460;  // valeur à modifier également dans eMain.js
        var minimumWindowHeight = 180;  // valeur à modifier également dans eMain.js
        var defaultMargin = 20;

        // Ajout d'un évènement lors du redimensionnement de la fenêtre pour ajuster le conteneur d'image
        function onFrameSizeChange(nNewWidth, nNewHeight) {
            if (typeof (imgHolder) != "undefined" && imgHolder != null) {
                var browser = new getBrowser();
                // En mode visualisation uniquement, pas besoin de réserver de l'espace pour des contrôles de modification masqués
                if (getAttributeValue(imgHolder, "viewonlymode") == "1")
                    minimumWindowHeight = 28;
                // marge de 2 pixels (véridique) pour IE - empêche l'affichage de barres de défilement incongrues
                var nNewHolderHeight = (nNewHeight - minimumWindowHeight + (browser.isIE ? 2 : 0));
                // #32 132 : ce calcul donnera un nombre négatif si l'image n'a pas pu être affichée. Dans ce cas, on réduit le conteneur au minimum
                if ((nNewHeight - minimumWindowHeight) == 0) {
                    if (browser.isIE)
                        nNewHolderHeight = 28;
                    else
                        nNewHolderHeight = 20;
                }

                imgHolder.style.height = nNewHolderHeight + 'px';

                //ELAIZ: Ajout classe css resized à l'agrandissement

                if (nNewHolderHeight != 16) {
                    imgHolder.classList.add('resized');
                    imgHolder.classList.remove('mini-height');
                }else {
                    imgHolder.classList.remove('resized');
                    imgHolder.classList.add('mini-height');
                }
            }
        }

        function loadImage(strURL) {
            var bLoadWithoutListener = false;
            if (typeof (setEventListener) == "function") {
                setEventListener(document.getElementById("imgPreview"), 'load', imageLoadListener, false);
            }
            else
                if (loadImageTimerRetryCount < maxTimerRetryCount) {
                    loadImageTimer = setTimeout(imageLoadListener, 1000);
                    return false;
                }
                else
                    bLoadWithoutListener = true;

            clearTimeout(loadImageTimer);

            var oUrl = document.getElementById("imageURL");
            if (oUrl && getAttributeValue(oUrl, "isb64") == "1")
            {
                strURL = "data:image/png;base64," + oUrl.value;
            }

            document.getElementById("imgPreview").src = strURL;
            console.log(strURL)

            if (bLoadWithoutListener)
                imageLoadListener(null, true);
        }

        function imageLoadListener(evt, bIgnoreImageSize) {
            var imgHolder = document.getElementById("imgHolder");
            var img = document.getElementById("imgPreview");

            var newImgWidth = 0;
            var newImgHeight = 0;

            // Récupération de la taille de l'image telle qu'affichée sur le navigateur (computedStyle/currentStyle)
            var imgCoordinates = null;
            if (!bIgnoreImageSize) {
                // Pour IE, il faut recup les naturalXxx pour que les offsetWidth et offsetHeight soit bon
                if (img.naturalHeight)
                    var testHei = img.naturalHeight;
                if (img.naturalWidth)
                    var testWid = img.naturalWidth;

                if (typeof (getAbsolutePosition) == "function")
                    imgCoordinates = getAbsolutePosition(img, false);

                // Si l'image n'est pas encore prête, on retentera l'opération dans 1 seconde
                if (imgCoordinates == null || typeof (imgCoordinates.w) == "undefined" || typeof (imgCoordinates.h) == "undefined") {
                    if (loadImageTimerListenerTimerRetryCount < maxTimerRetryCount) {
                        loadImageListenerTimer = setTimeout(imageLoadListener, 1000);
                        return false;
                    }
                }
                else {
                    clearTimeout(loadImageListenerTimer);
                }
            }

            // Si on a pas pu vérifier les coordonnées de l'image (image non prête, ou page partiellement chargées), on indique au script de ne pas se préoccuper d'ajuster la
            // taille de la fenêtre à la taille de l'image, et d'utiliser une fenêtre maximisée
            var bSkipAutoAdjust = false;
            if (imgCoordinates == null) {
                bSkipAutoAdjust = true;
            }

            // Et récupérer l'objet JavaScript correspondant à la fenêtre modale...
            var oModal = null;
            <%
        if (modalVarName.Value.Length > 0)
            Response.Write(String.Concat("oModal = top.", modalVarName.Value, ";"));
            %>

            // On calcule la nouvelle taille à donner à la modale, et on la redimensionne en fonction de la taille de l'image
            if (oModal) {
                // Récupération de la taille actuelle de la fenêtre
                var windowWidth = oModal.width;
                var windowHeight = oModal.height;
                // En mode visualisation uniquement, pas besoin de réserver de l'espace pour des contrôles de modification masqués
                if (getAttributeValue(imgHolder, "viewonlymode") == "1")
                    minimumWindowHeight = 100;

                if (bSkipAutoAdjust) {
                    // Si on a pas pu déterminer les coordonnées de l'image : agrandissement de la fenêtre
                    // La fonction onFrameSizeChange se chargera d'ajuster les éléments suite à l'agrandissement
                    oModal.MaxOrMinModal();
                    return false;
                }

                // Si on a pu déterminer la taille de l'image...
                if (imgCoordinates.w != currentImgWidth)
                    newImgWidth = imgCoordinates.w;
                if (imgCoordinates.h != currentImgHeight)
                    newImgHeight = imgCoordinates.h;

                // Si la taille a été donnée en %, on utilise une taille par défaut correspondant à l'espace réservé aux autres composants de la fenêtre hors image
                // (champ d'ajout d'image, boutons)
                if (isNaN(getNumber(windowWidth)) || windowWidth.indexOf('%') > -1)
                    windowWidth = 0;
                if (isNaN(getNumber(windowHeight)) || windowHeight.indexOf('%') > -1)
                    windowHeight = 0;
                if (windowWidth == 0)
                    windowWidth = minimumWindowWidth;
                if (windowHeight == 0)
                    windowHeight = minimumWindowHeight;

                // Si la taille de l'image est supérieure à la taille minimum requise pour la fenêtre, on définit la nouvelle taille de la fenêtre
                windowWidth = getNumber(windowWidth);
                windowHeight = getNumber(windowHeight);
                var currentWindowWidth = windowWidth;
                var currentWindowHeight = windowHeight;
                if (!isNaN(getNumber(newImgWidth)) && getNumber(newImgWidth) > (minimumWindowWidth + defaultMargin))
                    windowWidth = getNumber(newImgWidth) + defaultMargin;
                if (!isNaN(getNumber(newImgHeight)) && getNumber(newImgHeight) > (minimumWindowHeight + defaultMargin))
                    windowHeight = getNumber(newImgHeight) + minimumWindowHeight + defaultMargin;

                // Si la taille de l'image + espace réservé aux contrôles de modification (champs pour ajout d'image + boutons) est supérieure à la taille de la fenêtre,
                // on ajuste la hauteur à donner à la fenêtre
                if (getNumber(newImgHeight) + minimumWindowHeight + defaultMargin > windowHeight)
                    windowHeight = getNumber(newImgHeight) + minimumWindowHeight + defaultMargin;

                // Si la taille à utiliser est supérieure à l'espace disponible à l'écran, on réduit la taille à donner à la fenêtre
                if (windowWidth > (oModal.docWidth - defaultMargin))
                    windowWidth = oModal.docWidth - defaultMargin;
                if (windowHeight > (oModal.docHeight - defaultMargin))
                    windowHeight = oModal.docHeight - defaultMargin;

                // Si la taille à utiliser est différente de la taille actuelle, on redimensionne
                if (currentWindowWidth != windowWidth || currentWindowHeight != windowHeight)
                    oModal.resizeTo(windowWidth, windowHeight);
            }
        }
    </script>
</head>
<body>
    <asp:Panel ID="imgHolder" CssClass="imgDlgHolder" Visible="true" runat="server">
        <asp:Image ID="imgPreview" Visible="true" runat="server" />
    </asp:Panel>
    <asp:Label ID="lblError" runat="server"></asp:Label>

    <form id="Form1" method="post" runat="server" enctype="multipart/form-data" style="width: 100%">
        <asp:Label ID="lblFile" runat="server"><%=eResApp.GetRes(_pref, 6286)%></asp:Label>

        <input onchange="oImageManager.displayAvatarName()" id="filMyFile" name="filMyFile" accept="image/png, image/jpeg, image/gif" style="width: 100%;" type="file" class="avatar" runat="server" />
        <div onmouseover="oImageManager.displayAvatarWaiter()" onmouseout="oImageManager.displayAvatarWaiter()" class="label-container">
            <span onclick="document.querySelector('#filMyFile').click()" id="avatarName"><%= eResApp.GetRes(_pref, 589) %></span>
            <label for="filMyFile"><%= eResApp.GetRes(_pref, 6498) %></label>  
        </div>
        <input id="imageURL" name="imageURL" type="text" class="imgURL" style="display: none" runat="server" />
        <input id="imageType" name="imageType" type="hidden" runat="server" />
        <input id="CalledFrom" name="CalledFrom" type="hidden" runat="server" />
        <input id="descId" name="descId" type="hidden" runat="server" />
        <input id="fileId" name="fileId" type="hidden" runat="server" />
        <input id="pjId" name="pjId" type="hidden" runat="server" />
        <input id="pjType" name="pjType" type="hidden" runat="server" />
        <input id="width" name="width" type="hidden" runat="server" />
        <input id="height" name="height" type="hidden" runat="server" />
        <input id="imageWidth" name="imageWidth" type="hidden" runat="server" />
        <input id="imageHeight" name="imageHeight" type="hidden" runat="server" />        
        <input id="modalVarName" name="modalVarName" type="hidden" runat="server" />        
        <input id="parentIsPopup" name="parentIsPopup" type="hidden" runat="server" />
        <input id="updateOnBlur" name="updateOnBlur" type="hidden" runat="server" />
        
        <asp:Label ID="lblFormat" runat="server"><%=(eResApp.GetRes(_pref, 1228)).Replace("<MASK>", "*.jpg, *.png, *.gif ")%></asp:Label>

        <div style="text-align: right; display: none;">
            <asp:Button CssClass="button-green" ID="cmdSend" runat="server" Text="<%=eResApp.GetRes(_pref, 5003)%>"></asp:Button>
            <asp:Button CssClass="button-red" ID="cmdDelete" runat="server" Text="<%=eResApp.GetRes(_pref, 29)%>"></asp:Button>
        </div>

        <asp:Panel runat="server" ID="PanelImgAlt" Visible="false">
            <label for="textImageAlt"><%=eResApp.GetRes(_pref, 6804)%></label>
            <asp:TextBox runat="server" ID="textImageAlt"></asp:TextBox>
        </asp:Panel>

    </form>
    <script type="text/javascript">
        <%= _bodyJavaScript %>
    </script>
</body>
</html>