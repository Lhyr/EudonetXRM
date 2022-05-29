<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ePJPties.aspx.cs"
    Inherits="Com.Eudonet.Xrm.ePJPties" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

</head>
<body>
    <form id="form1" runat="server">
        <div >
             <input type="hidden" id="ro" value="0" runat="server" />
            <input type="hidden" id="supp" value="1" runat="server"/>
            <input type="hidden" id="flds" value="inptFileName|oldFileName|inptTip|inptDsc|inptLimitDate|dtCrea" />
        </div>
        <div style="margin-top: -15px">
           


            <table class="tbPJPties">

                <tr>
                    <td id="lblFileName" runat="server" class="lblCol"></td>
                    <td class="valCol">
                        <input id="inptFileName" name="inptFileName" runat="server" /><input type="hidden" id="oldFileName" name="oldFileName" runat="server" /></td>
                </tr>

                <tr id="trEudoTable" runat="server">
                    <td id="lblEudoTable" runat="server" class="lblCol"></td>
                    <td id="tdEudoTable" class="valCol"></td>

                </tr>
                <tr id="trEudoFile" runat="server">
                    <td id="lblEudoFile" runat="server" class="lblCol"></td>
                    <td id="tdEudoFile" class="valCol"></td>
                </tr>

                <tr>
                    <td id="lblType" runat="server" class="lblCol"></td>
                    <td class="valCol" runat="server" id="tdFileType"></td>
                </tr>
                <tr>
                    <td id="lblPath" runat="server" class="lblCol"></td>
                    <td class="valCol" id="tdPath" runat="server"></td>
                </tr>
                <tr>
                    <td id="lblSize" runat="server" class="lblCol"></td>
                    <td class="valCol" id="tdSize" runat="server"></td>
                </tr>
                <tr>
                    <td id="lblCrea" runat="server" class="lblCol"></td>
                    <td class="valCol" id="tdCrea" runat="server"></td>
                </tr>
               <%-- <tr>
                    <td id="lblModif" runat="server" class="lblCol"></td>
                    <td class="valCol" id="tdModif" runat="server"></td>
                </tr>--%>
                <tr>
                    <td id="lblLimitDate" runat="server" class="lblCol"></td>
                    <td class="valCol" id="dtLimitDate" runat="server">
                        <input runat="server" name="inptLimitDate" type="text" id="inptLimitDate" onChange="validateDate('inptLimitDate');" class="LNKFREETEXT edit"/></td> <!--onchange="doOnchangeDate(1000);"-->
                    <td id="calLimitDate" class="icon-agenda btnIe8 " onclick="selectDate('inptLimitDate')"></td>
                </tr>
                <tr>
                    <td id="lblTip" runat="server" class="lblCol"></td>
                    <td class="valCol">
                        <input id="inptTip" name="inptTip" runat="server" /></td>
                </tr>
                <tr>
                    <td id="lblDsc" runat="server" class="lblCol"></td>
                    <td class="valCol">
                        <input id="inptDsc" name="inptDsc" runat="server" /></td>
                </tr>
                <tr id="trExtLnk" runat="server">
                    <td id="lblExtLnk" runat="server" class="lblCol"></td>
                    <td class="valCol">
                        <input id="inptExtLnk" name="inptExtLnk" runat="server" readonly="1" /></td>
                </tr>
            </table>
            <input type="hidden" id="dtCrea" runat="server" />
        </div>
    </form>
</body>
</html>
