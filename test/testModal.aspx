<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="testModal.aspx.cs" Inherits="XRM.testModal"
    EnableSessionState="true" EnableViewState="false" %>

<html>
<head>
    <link rel="stylesheet" type="text/css" href="../themes/default/css/eModalDialog.css" />
    <link rel="stylesheet" type="text/css" href="../themes/default/css/ebuttons.css" />
    <script type="text/javascript" language="javascript" src="../scripts/eUpdater.js"></script>
    <script type="text/javascript" language="javascript" src="../scripts/eTools.js"></script>
    <script type="text/javascript" language="javascript" src="../scripts/eModalDialog.js"></script>
    <script type="text/javascript" language="javascript" src="../scripts/ePopUp.js"></script>
    <script type="text/javascript" language="javascript">
        function onOk() {
            alert(this);
            alert("Ok");
        }
        function onAnnuler() {
            alert("Annuler");
        }
        function showIt() {
            var modal = new eModalDialog("ceci est un test", 0, "http://localhost:54680/eLogin.aspx", document.getElementById("pWidth").value, document.getElementById("pHeight").value);
            modal.addParam("test1", "test", "post");
            modal.addParam("test2", "test", "get");
            modal.show();
            modal.addButton("ok", onOk, "button-green", null);
            /*var modal = new eModalDialog("MsgBox", 1, null, document.getElementById("pWidth").value, document.getElementById("pHeight").value);
            modal.setMessage("message de test", "details du message de test", 2);
            modal.show();
            modal.addButton("ok", onOk);
            modal.addButton("annuler", onAnnuler);*/


        }

        function onLoadDoc() {

            //document.onselectstart = new Function("return false");
            //document.onmousedown = drags;
            //document.onmouseup = doMouseUp;
        }

        function onValid() {
            alert("onValid");
        }

        function showModal() {

            var strTitre = document.getElementById("TextTitre").value;
            var strType = document.getElementById("SelectType").options[document.getElementById("SelectType").selectedIndex].value;
            var strUrl = document.getElementById("TextUrl").value;
            var nWidth = document.getElementById("TextWidth").value;
            var nHeight = document.getElementById("TextHeight").value;

            var textIcon = document.getElementById("TextIcon").options[document.getElementById("TextIcon").selectedIndex].value;
            //begin
            var oModal = new eModalDialog(strTitre, strType, strUrl, nWidth, nHeight);
            var textMessage = document.getElementById("TextMessage").value;
            var textMessageDetails = document.getElementById("TextMessageDetails").value;
            if (strType == "1") {
                oModal.setMessage(textMessage, textMessageDetails, textIcon);
            }

            if (strType == "2") {

                var PromptLabel = document.getElementById("PromptLabel").value;
                var PromptValue = document.getElementById("PromptValue").value;
                oModal.setPrompt(PromptLabel, PromptValue);
            }

            oModal.show();

            oModal.addButton("Valider", onValid, "button-green", null);
            oModal.addButton("Annuler", onAnnuler, "button-gray", null);

        }





        function generateJavascript() {

            var strTitre = document.getElementById("TextTitre").value;
            var strType = document.getElementById("SelectType").options[document.getElementById("SelectType").selectedIndex].value;
            var strUrl = document.getElementById("TextUrl").value;
            var nWidth = document.getElementById("TextWidth").value;
            var nHeight = document.getElementById("TextHeight").value;
            var textIcon = document.getElementById("TextIcon").options[document.getElementById("TextIcon").selectedIndex].value;
            var textMessage = document.getElementById("TextMessage").value;
            var textMessageDetails = document.getElementById("TextMessageDetails").value;

            var PromptLabel = document.getElementById("PromptLabel").value;
            var PromptValue = document.getElementById("PromptValue").value;



            //begin$

            var strJs = "\n\n\n";


            strJs += "<link rel='stylesheet' type='text/css' href='themes/default/css/eModalDialog.css' />\n";
            strJs += "<link rel='stylesheet' type='text/css' href='themes/default/css/ebuttons.css' />\n";

            strJs += "<script type='text/javascript' language='javascript' src='scripts/eUpdater.js' />\n";
            strJs += "<script type='text/javascript' language='javascript' src='scripts/eModalDialog.js' />\n";
            strJs += "<script type='text/javascript' language='javascript' src='scripts/ePopUp.js' />\n";
            strJs += "<script type='text/javascript' language='javascript'>\n";

            strJs += "//*****************************************************//\n";
            strJs += "//***********Script généré automatiquement*************//\n";
            strJs += "//*****************************************************//\n\n\n";


            strJs += "//***********     Début du code de création de popup *************//\n\n\n\n";
            strJs += "function createPopUpOrMsgBoxOrPrompt()\n";
            strJs += "{\n";
            strJs += "  var strTitre ='" + strTitre.replace("'", "\\'") + "';\n";
            strJs += "  var strType ='" + strType.replace("'", "\\'") + "';\n";

            strJs += "  //Uniquement si PopUp\n";
            strJs += "  var strUrl ='" + strUrl.replace("'", "\\'") + "';\n";

            strJs += "  var nWidth =" + nWidth + ";\n";
            strJs += "  var nHeight =" + nHeight + ";\n";
            strJs += "  var textIcon ='" + textIcon.replace("'", "\\'") + "';\n";


            strJs += "  var PromptLabel ='" + PromptLabel.replace("'", "\\'") + "';\n";
            strJs += "  var PromptValue ='" + PromptValue.replace("'", "\\'") + "';\n";


            strJs += "  //Uniquement si MessageBox\n";
            strJs += "  var textMessage ='" + textMessage.replace("'", "\\'") + "';\n";
            strJs += "  //Uniquement si MessageBox\n";
            strJs += "  var textMessageDetails ='" + textMessageDetails.replace("'", "\\'") + "';\n";

            strJs += "  var oModal = new eModalDialog(strTitre, strType, strUrl, nWidth,nHeight);\n";

            if (strType == "1") {
                strJs += "  oModal.setMessage( textMessage, textMessageDetails, textIcon);\n";
            }


            if (strType == "2") {
                strJs += "  oModal.setPrompt( PromptLabel, PromptValue);\n";
            }



            strJs += "  oModal.show();\n";
            strJs += "  //Fonctions (On***Function à implémenter dans la portée de la page de script) \n";
            strJs += "  oModal.addButton('Ok', onOkFunction, 'button-green', null);\n";
            strJs += "  oModal.addButton('Annuler', onCancelFunction, 'button-gray', null);\n";
            strJs += "  oModal.addButton('Autre', onValidFunction, 'button-green', null);\n";
            strJs += "}\n\n\n\n";
            strJs += "/**********************************************************\n";
            strJs += "/**   Action des boutons***\n";
            strJs += "/**********************************************************\n";
            strJs += "function onOkFunction{alert('onOkFunction');}\n";
            strJs += "function onOkFunction{alert('onCancelFunction');}\n";
            strJs += "function onOkFunction{alert('onValidFunction');}\n";

            strJs += "  //***********     Fin création de popup *************//\n";
            document.getElementById('txtJavascript').value = strJs
        }
    </script>
</head>
<body onload="onLoadDoc();">
    <!--
    <p>
    <strong>page html</strong></p>
    <input type="text" id="pWidth" value="500" />
    <input type="text" id="pHeight" value="500" />
    <input type="button" onclick="showIt();" />
    -->
    <hr />
    <table>
        <tr>
            <td>
                width :
            </td>
            <td>
                <input type="text" id="TextWidth" value="500" />
            </td>
        </tr>
        <tr>
            <td>
                height :
            </td>
            <td>
                <input type="text" id="TextHeight" value="500" />
            </td>
        </tr>
        <tr>
            <td>
                Type popup :
            </td>
            <td>
                <select id="SelectType">
                    <option value="0">PopUp(window.open)</option>
                    <option value="1">MessageBox</option>
                    <option value="2">Prompt</option>
                </select>
            </td>
        </tr>
        <tr>
            <td>
                Titre :
            </td>
            <td>
                <input type="text" id="TextTitre" value="TextTitre" />
            </td>
        </tr>
        <tr>
            <td>
                Url (si window.open) :
            </td>
            <td>
                <input type="text" id="TextUrl" value="http://localhost:54680/eLogin.aspx" />
            </td>
        </tr>
        <tr>
            <td>
                Icone de notification (si msgbox) :
            </td>
            <td>
                <select id="TextIcon">
                    <option value="0">MSG_CRITICAL</option>
                    <option value="1">MSG_QUESTION</option>
                    <option value="2">MSG_EXCLAM</option>
                    <option value="3">MSG_INFOS</option>
                    <option value="4">MSG_SUCCESS</option>
                </select>
            </td>
        </tr>
        <tr>
            <td>
                Message (si msgbox) :
            </td>
            <td>
                <input type="text" id="TextMessage" value="texte du Message" />
            </td>
        </tr>
        <tr>
            <td>
                Message détaillé (si msgbox) :
            </td>
            <td>
                <input type="text" id="TextMessageDetails" value="détails du message texte" />
            </td>
        </tr>
        <tr>
            <td>
                Label prompt (si Prompt) :
            </td>
            <td>
                <input type="text" id="PromptLabel" value="Introduire une valeur :" />
            </td>
        </tr>
        <tr>
            <td>
                Defaut value prompt (si Prompt) :
            </td>
            <td>
                <input type="text" id="PromptValue" value="Valeur par défaut" />
            </td>
        </tr>
    </table>
    <input type="button" value="Générer popup" onclick="showModal();" aria-atomic="False" />
    <input type="button" value="Générer javascript" onclick="generateJavascript();" />
    <br />
    <textarea id="txtJavascript" cols="150" rows="20"></textarea>
</body>
</html>
