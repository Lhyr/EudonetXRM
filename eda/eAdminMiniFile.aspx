<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminMiniFile.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminMiniFile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer les MiniFiches</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style type="text/css">        
        #divResult
        {
            margin-top: 30px;
        }

        #tblMapping
        {
            width: 80%;
        }

        #tblMapping th, #tblMapping td
        {
           text-align: center;
        }

        #tblMapping tr.header, #tblMapping tr.footer
        {
           background-color: #E0E0E0;
        }

        #tblMapping tr.even
        {
           background-color: #FFFFFF;
        }

        #tblMapping tr.odd
        {
           background-color: #F0F0F0;
        }

        #tblMapping tr.even:hover, #tblMapping tr.odd:hover
        {
           background-color: #7FFF8E;
        }
    </style>
    <script type="text/javascript">
        loadListTab();
    </script>
</head>
<body>
    <h1>
        Formulaire d'administration du mapping des MiniFiches
    </h1>
    <p>
        <b>Onglet</b> : descid de la table à mapper
    </p>
    <br />
    Mappings :
    <ul>
        <li><b>Image</b> : descid du champ à mapper pour le logo.</li>
        <li><b>Id</b> : id sql de l'enregistrement dans [FILEMAP_PARTNER].</li>
        <li><b>Type</b> : type de mapping (champ/titre/ligne separatrice). Le type Champ/Titre affiche un champ mappé avec le style d'un titre.</li>
        <li><b>Afficher Libellé</b> : indique si le libellé doit être affiché avec la valeur du champ. Ex: si cochée, affichera "Société : Eudonet", si décoché affichera uniquement "Eudonet".</li>
        <li><b>DescId</b> : descid du champ à mapper. Ex: 301 pour [PM].[PM01]. Pour les mapping de type Titre, permet de faire la liason avec la table [RES], pour affiché le libellé d'un champ comme titre. Ex: 301 affichera le titre "Société"</li>
        <li><b>Ordre</b> : ordre d'affichage du mapping</li>
    </ul>
    <br />
    <form id="form1" runat="server">
        <div id="divLoader">
            <label for="tabid">Onglet : </label>
            <select id="tabid" name="tabid" onchange="loadMapping();">
            </select>
            <input type="button" value="Charger" id="btnReload" name="btnReload" onclick="loadMapping();" />
        </div>
        <div id="divResult"></div>
    </form>
</body>
</html>
