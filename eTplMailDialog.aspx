<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>
<%@ Import Namespace="EudoQuery" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eTplMailDialog.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eTplMailDialog" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title></title>
    <%--<link href="themes/default/css/eAdmin.css" rel="stylesheet" />--%>

    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


    <script type="text/javascript">

        function titleClick(element) {
            var icon = document.getElementById("permTitleIcon");
            var permBlock = document.getElementById("PermOptions");
            if (getAttributeValue(element, "data-developed") == "1") {
                icon.className = "icon-develop";
                setAttributeValue(element, "data-developed", "0");
                permBlock.style.display = "none";
            }
            else {
                icon.className = "icon-unvelop";
                setAttributeValue(element, "data-developed", "1");
                permBlock.style.display = "block";
            }
        }



    </script>

</head>
<body class="memoDlg memoTplMailDlg bodyWithScroll" onload="init();">
    <div id="fileDiv_<%= TableType.MAIL_TEMPLATE.GetHashCode().ToString() %>" fid="<%= TemplateMailId.ToString() %>">


        <%
            // Test dispo de grapsjs - AJout du br si non dispo
            if (
                (int)_pref.ClientInfos.ClientOffer == 0
              || eTools.IsMSBrowser
              || !eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
              || MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAIL /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */
            )
            {
        %>
        <br />
        <%} %>

        <input type="hidden" id="mtType" name="mtType" value="<%= MailTemplateType.GetHashCode().ToString() %>" />

        <textarea id="eTplMailDialogEditorCustomCSS" style="display: none;"><%= BodyCss %></textarea>
        <textarea id="eTplMailDialogEditorValue" runat="server" style="display: none;"></textarea>

        <div id="railroad" runat="server">
        </div>

        <div id="DivObject" class="ObjectIput">
            <table>
                <tr>
                    <td><span class="ObjectLabel"><%=eResApp.GetRes(_pref, 6485)%></span></td>
                    <td>
                        <input type="text" name="LabelName" id="lbl" value="<%=Libelle %>" /></td>
                    <td></td>
                </tr>

                <tr>
                    <td class="memoTplMailDlgSubject"><span class="ObjectLabel"><%=eResApp.GetRes(_pref, 5090)%></span></td>
                    <td>
                        <%--// ALISTER demande # 81112 "Cache le bouton editer (crayon) pour le modèle d'email unitaire"--%>
                         <%if (MailTemplateType != TypeMailTemplate.MAILTEMPLATE_EMAIL) { %>
                           <div class="value-container  align-class-mailtpl">
                                <div id="obj" class="field-value"></div>
                                  <div class="icon-edn-pen field-btn" onclick="openMemo_TplObj('obj')"></div>
                           </div>
                        <% } else {%>
                           <input id="obj" class="field-value2" type="text" value="<%=Objet %>"/>
                        <% }%>

                        <script type="text/javascript" language="javascript">

                            function decodeHTMLEntities(text) {
                                var entities = [
                                    ['amp', '&'],
                                    ['apos', '\''],
                                    ['#x27', '\''],
                                    ['#x2F', '/'],
                                    ['#39', '\''],
                                    ['#47', '/'],
                                    ['lt', '<'],
                                    ['gt', '>'],
                                    ['nbsp', ' '],
                                    ['quot', '"']
                                ];

                                for (var i = 0, max = entities.length; i < max; ++i)
                                    text = text.replace(new RegExp('&' + entities[i][0] + ';', 'g'), entities[i][1]);

                                return text;
                            }

                            var htmlScript = '<%=Objet %>';
                            document.getElementById('obj').innerHTML = decodeHTMLEntities(htmlScript);


                        </script>
                        <%--<input type="text" name="ObjectName" id="obj" value="<%=Objet %>" />
                        <div id="objCKEditor"  />--%>

                    </td>

                    <td><span class="icon-annex" id="btnPJ" runat="server">(<%=NbPJ %>)</span></td>
                    
                    
                    <%--// ALISTER demande # 81112 "Cache le texte d'aperçu pour le modèle d'email unitaire"--%>
                    <%if(MailTemplateType != TypeMailTemplate.MAILTEMPLATE_EMAIL) { %>
                        <%--// AABBA tache # 1 940--%>
                        <td class="memoTplMailDlgPreheader" title="<%=eResApp.GetRes(_pref, 2475)%>"><span class="PreheaderLabel"><%=eResApp.GetRes(_pref, 2472)%></span></td>
                        <td>
                            <input type="text" name="Preheader" id="preheader" value="<%=Preheader %>" maxlength="<%=eLibConst.MAX_PREHEADER_LENGTH %>" /></td>
                    <% }%>
               </tr>

            </table>

        </div>

        <h3 id="permTitle" onclick="titleClick(this);" data-developed="1"><span class="icon-unvelop" id="permTitleIcon"></span><span id="label"><%=eResApp.GetRes(_pref, 505)%></span></h3>
        <div id="PermOptions" runat="server"></div>

        <div id="eTplMailDialogEditorContainer" edneditor="0">
            <div id="DivTplMail">
                <script type="text/javascript" language="javascript">
                    nGlobalActiveTab = <%= TableType.MAIL_TEMPLATE.GetHashCode().ToString() %>;
                    var jsVarName = '';
                    var eTplMailDialogEditorObject = null;

                    <%=MailMergeFields%>

                    function init() {
                    <%=InitJSOutput%>
                    }

                    // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
                    function onFrameSizeChange(nNewWidth, nNewHeight) {
                        if (eTplMailDialogEditorObject) {

                            if (eTplMailDialogEditorObject.isHTML) {
                                eTplMailDialogEditorObject.resize(nNewWidth, nNewHeight - 56);
                            }
                            else {
                                eTplMailDialogEditorObject.resize(nNewWidth - 4, nNewHeight - 56);
                            }
                        }
                    }

                    function onFileSubmit() {
                        top.setWait(true);
                        if (eTplMailDialogEditorObject) {
                            document.getElementById('Value').value = eTplMailDialogEditorObject.getData();
                        }
                    }
                </script>
            </div>
        </div>

        <%
            // Test dispo de grapsjs
            if ((int)_pref.ClientInfos.ClientOffer > 0
              && !eTools.IsMSBrowser
              && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
              && MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAILING /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */)
            {
                //Block DIV
        %>
        <div id="eTplMailDialogEditorCKE" runat="server" style="display: none" edneditor="1">
            <script language="javascript" type="text/javascript">
                var eTplMailDialogEditorObjectCKe = null;
            </script>

        </div>
        <%
            }
        %>
        <script type="text/javascript" language="javascript">
            <%=EndJSOutput%> 

            var modal = eTools.GetModal("EmailingTemplateDialog")


            modal.switchButtonDisplay("cancel_btn", false)





            <%
            // Test dispo de grapsjs
            if ((int)_pref.ClientInfos.ClientOffer > 0
              && !eTools.IsMSBrowser
              && eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor)
              && MailTemplateType == TypeMailTemplate.MAILTEMPLATE_EMAILING /* grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, supprimer cette condition */)
            {
              %> 

            modal.switchButtonDisplay("next_btn", false)

            var eTplMail = {
                currentStep: 1,
                currentEditor: 1
            }


            eTplMail.StepClick = function (step) {


                //Cast num
                step = parseInt(step)

                if (step == 1) {
                    document.getElementById("permTitle").style = "display:none;";
                    document.getElementById("PermOptions").style = "display:none;";
                }
                else {
                    document.getElementById("permTitle").style = "";
                    document.getElementById("PermOptions").style = "";
                }

                if (this.currentStep === step)
                    return;


                this.currentStep = step

                //gestion de la railroad
                var oSteps = document.getElementById("railroad").querySelectorAll("div[id^=step]");
                [].slice.call(oSteps).forEach(function (myStep) {
                    if (myStep.id === "step_" + step) {
                        myStep.classList.remove("state_grp");
                        myStep.classList.add("state_grp-current");
                    }
                    else {
                        myStep.classList.remove("state_grp-current");
                        myStep.classList.add("state_grp");
                    }
                });

                this.HandleButtonStep(step)

                this.SwitchEditor(step)
            }

            eTplMail.HandleButtonStep = function (step) {
                step = parseInt(step)
                switch (step) {
                    case 1:
                        modal.switchButtonDisplay("next_btn", false)
                        modal.switchButtonDisplay("prev_btn", true)
                        modal.switchButtonDisplay("val_btn", true)
                        break;

                    case 2:
                        modal.switchButtonDisplay("next_btn", true)
                        modal.switchButtonDisplay("prev_btn", false)
                        modal.switchButtonDisplay("val_btn", false)
                        break;

                }
            }


            eTplMail.SwitchEditor = function (editor) {
                //  debugger
                // 1 : grapJS
                // 2 : CKeditor

                editor = parseInt(editor)
                if (this.currentEditor == editor)
                    return;

                top.setWait(true);
                try {
                    switch (editor) {
                        case 1:   //GrapJS

                            //Masquage CK
                            document.getElementById("eTplMailDialogEditorCKE").style.display = "none";

                            //transfert du contenu CK-->Grap
                            if (this.currentEditor == 2) {
                                var content = eTplMailDialogEditorObjectCKe.getData();
                                var color = eTplMailDialogEditorObjectCKe.getColor();
                                var css = eTplMailDialogEditorObjectCKe.getCss();


                                if (color) {

                                    var result = eTools.setCssRuleFromString(css, "body", "background-color", color, true);

                                    if (result.hasChanged) {
                                        eTplMailDialogEditorObjectCKe.setCss(result.value)
                                        css = result.value;
                                    }
                                }

                                eTplMailDialogEditorObject.setData(content);
                                eTplMailDialogEditorObject.setColor(color); // Backlog #619 - On transfère la couleur de fond d'un éditeur à l'autre (positionné sur le wrapper de grapesjs)
                                eTplMailDialogEditorObject.setCss(css); // Backlog #617 - Transfert des CSS externes de chaque éditeur l'un vers l'autre





                            }

                            //Affichage Grapjs
                            document.getElementById("eTplMailDialogEditorContainer").style.display = "";

                            break;

                        case 2: //CKeditor


                            //Masquage Graps
                            document.getElementById("eTplMailDialogEditorContainer").style.display = "none";

                            //transfert du contenu CK-->Grap
                            if (this.currentEditor == 1) {
                                var content = eTplMailDialogEditorObject.getData();
                                var color = eTplMailDialogEditorObject.getColor();
                                var css = eTplMailDialogEditorObject.getCss();

                                if (color) {

                                    var result = eTools.setCssRuleFromString(css, "body", "background-color", color, true);

                                    if (result.hasChanged) {
                                        eTplMailDialogEditorObject.setCss(result.value)
                                        css = result.value;
                                    }
                                }

                                eTplMailDialogEditorObjectCKe.setData(content);
                                eTplMailDialogEditorObjectCKe.setColor(color); // Backlog #619 - On transfère la couleur de fond d'un éditeur à l'autre (positionné sur le wrapper de grapesjs)
                                eTplMailDialogEditorObjectCKe.setCss(css); // Backlog #617 - Transfert des CSS externes de chaque éditeur l'un vers l'autre
                            }

                            //Affichage CKEDITOR
                            document.getElementById("eTplMailDialogEditorCKE").style.display = "";



                            break
                    }
                }
                catch (e) {
                    top.setWait(false);
                }

                this.currentEditor = editor;
                top.setWait(false);
            }

            <%
            }
            else
            {
                %>
            modal.switchButtonDisplay("val_btn", false)

            <%
            }
            %>


</script>
    </div>
</body>
</html>
