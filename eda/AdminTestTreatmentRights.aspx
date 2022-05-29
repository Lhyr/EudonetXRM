<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminTestTreatmentRights.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.AdminTestTreatmentRights" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <h1>Test administration traitements de droits</h1>
    <form id="formTest" runat="server">
    <div>
        <asp:Label Text="ID TRAIT" AssociatedControlID="textID" runat="server"></asp:Label>
        <asp:TextBox runat="server" ID="textID"></asp:TextBox>
        <asp:Button runat="server" Text="Rechercher" OnClick="Search_Click" />

        <hr />

        <asp:Label Text="Niveau" runat="server"></asp:Label>
        <asp:TextBox runat="server" ID="textLevel"></asp:TextBox>
        <asp:Label Text="Utilisateurs" runat="server"></asp:Label>
        <asp:TextBox runat="server" ID="textUsers"></asp:TextBox>

        <asp:Button runat="server" Text="Mettre à jour" />
    </div>
    </form>
</body>
</html>
