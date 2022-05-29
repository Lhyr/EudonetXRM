<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMemoDialog.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eMemoDialog" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

 


</head>
<body class="memoDlg" onload="init();">
    <% if (UploadContentEnabled)
       { %>
    <form id="memoDlgUploadForm" class="memoContnR" runat="server" onsubmit="onFileSubmit();">
        <div>
            <input type="hidden" name="DescId" id="DescId" value="<%= (Request.Form["DescId"] != null ? Request.Form["DescId"].ToString() : String.Empty) %>" />
            <input type="hidden" name="Title" id="Title" value="<%= (Request.Form["Title"] != null ? Request.Form["Title"].ToString() : String.Empty) %>" />
            <input type="hidden" name="ParentFrameId" id="ParentFrameId" value="<%= (Request.Form["ParentFrameId"] != null ? Request.Form["ParentFrameId"].ToString() : String.Empty) %>" />
            <input type="hidden" name="EditorJsVarName" id="EditorJsVarName" value="<%= (Request.Form["EditorJsVarName"] != null ? Request.Form["EditorJsVarName"].ToString() : String.Empty) %>" />
            <input type="hidden" name="IsHTML" id="IsHTML" value="<%= (Request.Form["IsHTML"] != null ? Request.Form["IsHTML"].ToString() : String.Empty) %>" />
            <input type="hidden" name="EditorType" id="EditorType" value="<%= (Request.Form["EditorType"] != null ? Request.Form["EditorType"].ToString() : String.Empty) %>" />
            <input type="hidden" name="EnableTemplateEditor" id="EnableTemplateEditor" value="<%= (Request.Form["EnableTemplateEditor"] != null ? Request.Form["EnableTemplateEditor"].ToString() : String.Empty) %>" />
            <input type="hidden" name="ToolbarType" id="ToolbarType" value="<%= (Request.Form["ToolbarType"] != null ? Request.Form["ToolbarType"].ToString() : String.Empty) %>" />
            <input type="hidden" name="width" id="width" value="<%= (Request.Form["width"] != null ? Request.Form["width"].ToString() : String.Empty) %>" />
            <input type="hidden" name="height" id="height" value="<%= (Request.Form["height"] != null ? Request.Form["height"].ToString() : String.Empty) %>" />
            <input type="hidden" name="divMainWidth" id="divMainWidth" value="<%= (Request.Form["divMainWidth"] != null ? Request.Form["divMainWidth"].ToString() : String.Empty) %>" />
            <input type="hidden" name="divMainHeight" id="divMainHeight" value="<%= (Request.Form["divMainHeight"] != null ? Request.Form["divMainHeight"].ToString() : String.Empty) %>" />            
            <input type="hidden" name="Value" id="Value" value="<%= (Request.Form["Value"] != null ? Request.Form["Value"].ToString() : String.Empty) %>" />
            <input type="hidden" name="CustomCSS" id="CustomCSS" value="<%= (Request.Form["CustomCSS"] != null ? Request.Form["CustomCSS"].ToString() : String.Empty) %>" />
            <input type="hidden" name="UploadContentEnabled" id="UploadContentEnabled" value="<%= (Request.Form["UploadContentEnabled"] != null ? Request.Form["UploadContentEnabled"].ToString() : String.Empty) %>" />
            <input type="hidden" name="UploadContentLabel" id="UploadContentLabel" value="<%= (Request.Form["UploadContentLabel"] != null ? Request.Form["UploadContentLabel"].ToString() : String.Empty) %>" />
            <input type="hidden" name="UploadContentFileFilter" id="UploadContentFileFilter" value="<%= (Request.Form["UploadContentFileFilter"] != null ? Request.Form["UploadContentFileFilter"].ToString() : String.Empty) %>" />
            <input type="hidden" name="UploadContentLimit" id="UploadContentLimit" value="<%= (Request.Form["UploadContentLimit"] != null ? Request.Form["UploadContentLimit"].ToString() : String.Empty) %>" />
            <input type="hidden" name="UploadContentAppend" id="UploadContentAppend" value="1" />
            <ul id="memoDlgUploadDiv" class="memoDlgUploadDiv">
                <li class="upldFile">
                    <label for="memoDlgFile">
                        <%=UploadContentLabel%>
                    </label>
                    <input id="memoDlgFile" type="file" runat="server" /><div class="tooltipDl"><%=eResApp.GetRes(_pref.Lang, 6320).Replace("<VALUE>", "css")%></div>
                </li>
                <li class="middlLi">
                    <strong><%=eResApp.GetRes(_pref.Lang, 6337)%> :</strong><ul class="imprtCh">
                        <li>
                            <input type="radio" id="UploadContentAppendNo" name="ContentAppend" onclick="onCheck('UploadContentAppend', '0');" value="0" <%= (UploadContentAppend ? "" : "checked=\"checked\"") %> />&nbsp;
                     <label for="UploadContentAppendNo"><%=eResApp.GetRes(_pref.Lang, 6338)%></label></li>
                        <li>
                            <input type="radio" id="UploadContentAppendYes" name="ContentAppend" onclick="onCheck('UploadContentAppend', '1');" value="1" <%= (UploadContentAppend ? "checked=\"checked\"" : "") %> />&nbsp;
                     <label for="UploadContentAppendYes"><%=eResApp.GetRes(_pref.Lang, 6339)%></label></li>
                    </ul>
                </li>
                <li id="memoDlgUploadBtnDiv" class="memoDlgUploadBtnDiv" runat="server">
                    <asp:Label ID="lblError" runat="server" CssClass="error"></asp:Label>
                </li>
            </ul>
        </div>
    </form>
    <% } %>
    <textarea id="eMemoDialogEditorCustomCSS" style="display: none;"><%= CustomCSS %></textarea>
    <textarea id="eMemoDialogEditorValue" runat="server" style="display: none;"></textarea>
    <div id="eMemoDialogEditorContainer"></div>
    <script type="text/javascript" language="javascript">
        var jsVarName = '';
        var eMemoDialogEditorObject = null;        

        function init() { 
         <%=InitJSOutput%>
        }

        ///MOU 06/05/2014 mise a jour des inputs hidden
        function onCheck(targetId, value) {
            document.getElementById(targetId).value = value;
        }

        // EVENEMENT LORS DU REDIMENSIONNEMENT DE LA MODALDIALOG
        function onFrameSizeChange(nNewWidth, nNewHeight) {
            if (eMemoDialogEditorObject) {

                var w, h;
                if (eMemoDialogEditorObject.isHTML) {
                    w = nNewWidth;
                    h = nNewHeight - 56;
                }
                else {
                    w = nNewWidth - 4;
                    h = nNewHeight - 56;
                }

                eMemoDialogEditorObject.resize(w, h);
                if (document.getElementById("divMainWidth")) {
                    document.getElementById("divMainWidth").value = w;
                }
                if (document.getElementById("divMainHeight")) {
                    document.getElementById("divMainHeight").value = h;
                }

            }
        }

        function onFileSubmit() {
            top.setWait(true);
            if (eMemoDialogEditorObject) {
                document.getElementById('Value').value = eMemoDialogEditorObject.getData();
            }
        }
    </script>
    <script type="text/javascript" language="javascript">
        <%=EndJSOutput%>
    </script>
</body>
</html>
