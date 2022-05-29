<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminNotificationTrigger.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminNotificationTrigger" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer les Déclencheurs de Notifications</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style type="text/css">        
        #divResult
        {
            margin-top: 30px;
        }

        #tblTrigger
        {
            width: 80%;
        }

        #tblTrigger th, #tblTrigger td
        {
           text-align: center;
        }

        #tblTrigger tr.header, #tblTrigger tr.footer
        {
           background-color: #E0E0E0;
        }

        #tblTrigger tr.even
        {
           background-color: #FFFFFF;
        }

        #tblTrigger tr.odd
        {
           background-color: #F0F0F0;
        }

        #tblTrigger tr.even:hover, #tblTrigger tr.odd:hover
        {
           background-color: #7FFF8E;
        }
    </style>
    <script type="text/javascript">
        loadListTab();
        //loadList();
    </script>
</head>
<body>

    <h1>
        Formulaire d'administration des déclencheurs de notifications
    </h1>

    <form id="form1" runat="server">
        <div id="divLoader">
            <select id="tabid" name="tabid" style="display: none;" />
            <input type="hidden" id="selectedTabId" name="selectedTabId" />
            <input type="button" id="btnLoad" name="btnLoadList" onclick="loadList();" value="Charger" />
         </div>
        <div id="divResult"></div>
        <div id="divResultFile"></div>
    </form>
</body>
</html>
