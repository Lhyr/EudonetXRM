<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eWizard.aspx.cs" EnableSessionState="true"
    EnableViewState="false" Inherits="Com.Eudonet.Xrm.eWizard" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <style type="text/css" id="customCss" title="customCss"></style>
</head>
<body <%=_strBodyAttributes %>>
    <div class="waiter" id="ImgLoading" style="display: none;">
        <div class="contentWait" id="contentWait">
            <br />
            <img alt="wait" src="themes/default/images/wait.gif" /><br />
            <br />
            <br />
        </div>
    </div>
    <div id="waiter" class="waitOff">
    </div>
    <form id="wizardform" name="wizardform" runat="server" enableviewstate="false">
        <input id="srch" onkeyup="doSearch(this);" style="position:fixed;top:-500px;"/>

        <div id="content" class="window_iframe" runat="server" enableviewstate="false">
        </div>
    </form>
    <iframe height="0" width="0" name="fratmp" id="fratmp" style="display: none;"></iframe>
    <script language="javascript" type="text/javascript">
        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {
            var oMailingDivUserTemplates = document.getElementById("divUserTemplates");
            if (oMailingDivUserTemplates) {
                var oListContent = oMailingDivUserTemplates.querySelector("div.list-content");
                var browser = new getBrowser();
                if (oListContent && browser.isIOS) {
                    var oListContentCoord = getAbsolutePosition(oListContent);
                    var nNewListHeight = nNewHeight - oListContentCoord.y - 50; // 50 : hauteur de la barre de boutons
                    oListContent.style.height = nNewListHeight + 'px';
                    var oDivMTab = oMailingDivUserTemplates.querySelector("div.divmTab");
                    if (oDivMTab)
                        oDivMTab.style.height = nNewListHeight + 'px';
                }
            }

            // Recalcul des la hauteur du contenu des modèle cf. 36770
            var oCustomTemplates = document.getElementById("templateContainer");
            if (oCustomTemplates) {
                // 180 la taille des boutons + le header
                oCustomTemplates.style.height = nNewHeight - 180 + 'px';
            }


            // #33 098, #32 972, #33 601, #33 620 - Redimensionnement des listes de rubriques
            var oFieldsSelectLists = document.querySelectorAll("div.ItemList");
            if (oFieldsSelectLists) {
                var oListCoord = new Array();

                // On capture d'abord les coordonnées de toutes les listes, avant redimensionnement (et non pas au fur et à mesure, car le redimensionnement d'une liste peut modifier le positionnement d'une autre)
                for (var i = 0; i < oFieldsSelectLists.length; i++) {
                    oListCoord.push(getAbsolutePosition(oFieldsSelectLists[i]));
                }

                // Puis on redimensionne chaque liste en fonction de ses coordonnées capturées avant tout redimensionnement
                for (var i = 0; i < oFieldsSelectLists.length; i++) {
                    // Uniquement si la liste se situe sur l'étape actuellement visible par l'utilisateur, et si son ID figure parmi ceux qu'on accepte de redimensionner
                    // (ce filtrage par ID permet de s'assurer que le redimensionnement s'opère sur les listes souhaitées)
                    //var oCurrentWizardStep = document.querySelector("div.editor-on");
                    //--> KHA le 09/04/2015 on doit redimensionner tous les panneaux en meme temps

                    // Traitement des listes de l'étape 1 de l'Assistant Reporting
                    var oReportWizardStep1ResizableListIDs = new Array('editor_sourcelist', 'editor_DivTargetList');
                    if (oReportWizardStep1ResizableListIDs.indexOf(oFieldsSelectLists[i].id) != -1) {/*&& top.isParentElementOf(oFieldsSelectLists[i], oCurrentWizardStep)*/
                        //var nNewListHeight = nNewHeight - oListCoord[i].y - 75 - 60; // 75 pixels pour les options du bas et 60 pour les boutons de la modal dialog - cf. eReportWizardRenderer.BuildSelectFieldsPanel
                        var nNewListHeight = nNewHeight - 190 - 75 - 60; // on met la hauteur de l'élément en dur car  celle ci ne peut être calculée lorsque le panneau est caché
                        oFieldsSelectLists[i].style.height = nNewListHeight + 'px';
                    }

                    // Traitement des listes de l'étape 2 de l'Assistant Reporting
                    var oReportWizardStep2ResizableListIDs = new Array('editor_configurablelist');
                    if (oReportWizardStep2ResizableListIDs.indexOf(oFieldsSelectLists[i].id) != -1) {/* && top.isParentElementOf(oFieldsSelectLists[i], oCurrentWizardStep)*/
                        //var nNewListHeight = nNewHeight - oListCoord[i].y - 80 - 60; // 80 pixels pour les options du bas et 60 pour les boutons de la modal dialog - cf. eReportWizardRenderer.BuildSelectFieldsPanel
                        var nNewListHeight = nNewHeight - 140 - 80 - 60; // on met la hauteur de l'élément en dur car  celle ci ne peut être calculée lorsque le panneau est caché
                        oFieldsSelectLists[i].style.height = nNewListHeight + 'px';
                    }
                }
            }

            var chartPanelSelect = document.getElementById("chartPanelSelect");
            if (chartPanelSelect) {

                chartPanelSelect.style.width = "85%";
            }
        }
    </script>
</body>
</html>
