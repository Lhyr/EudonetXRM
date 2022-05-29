<%@ Import Namespace="Com.Eudonet.Xrm" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCatalogDialog.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eCatalogDialog" EnableSessionState="true" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html>
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>




    <!-- TODO: styles en dur pour IE 7 -->
    <!--[if lte IE 7]>
	    <style>
		    ul.eTVRoot li a.eTVP {
			    width: 100%;
		    }
		    ul.eTVRoot .eTVI {
			    cursor: hand;
		    }
	    </style>
    <![endif]-->
</head>
<body class="catDlg" onload="init();">
    <script type="text/javascript" language="javascript">

        var catOperationSearchOcc = <%=CountOccurencesOperation%>;
        var catOperationInsert = <%=InsertOperation%>;
        var catOperationDelete = <%=DeleteOperation%>;
        var catOperationSynchro = <%=SynchroOperation%>;
        var catOperationChange = <%=ChangeOperation%>;

        var eC;
        var eTV;

        function init() {
            eC = new eCatalog(
                'eC',
                'eCatalogDialog.aspx',
                '<%= CatDescId %>',
                '<%= CatParentId %>',
                '<%= CatBoundPopup.GetHashCode() %>',
                '<%= CatBoundDescid %>',
                '<%= CatPopupType.GetHashCode() %>',
                '<%= (CatMultiple ? 1 : 0) %>',
                '<%= (CatTreeView ? 1 : 0) %>'
            );
		
		    <% if (CatTreeView) { %>
			    eTV = new eTreeView('eTV');
		    <% } %>
            <% else if (CatMultiple) { %>
                initDragOpt();  //DRAG AND DROP
            <% } %>


		    <% if (!CatTreeView) { %>
            adjustColsWidth();
		    <% } %>


<%=InitJSOutput%>
        }
    
// EVENEMENT LORS DU RESIZE DE LA MODALDIALOG DU CATALOGUE
function onFrameSizeChange(w, h) {
    var oDivGlobal = document.getElementById("divCatDialog");
    //var oDivSrch = document.getElementById("catDivSrch");
    //var oDivAct = document.getElementById("catDivHead");

    var oDivCatVal = document.getElementById("eCEDValues"); // DIV contenant les valeurs du catalogue

    //Le resize est différent pour le catalogue barbulescent
    if(eC.treeview)
    {
        if (oDivGlobal)
            oDivGlobal.style.height = (parseInt(h)) + "px";
        if (oDivCatVal)
            oDivCatVal.style.height = (parseInt(h) - 180) + "px";
    }
    else if (eC.catMultiple) {	//Le resize est différent pour le multiple
        if (document.getElementById("eCEDSelValues")) {
            var oDivCatSelMul = document.getElementById("eCEDSelValues"); // DIV contenant les valeurs sélectionnées
            if (oDivCatSelMul) {
                // Taille de chaque liste = taille de la fenêtre / 2 - marge de 5% de cette taille
                oDivCatSelMul.style.width = Math.round((parseInt(w) / 2) - (parseInt(w) * (5 / 100))) + "px";
                oDivCatSelMul.style.height = (parseInt(h) - 200) + "px";
            }
            if (oDivCatVal) {
                // Taille de chaque liste = taille de la fenêtre / 2 - marge de 5% de cette taille
                oDivCatVal.style.width = Math.round((parseInt(w) / 2) - (parseInt(w) * (5 / 100))) + "px";
                oDivCatVal.style.height = (parseInt(h) - 200) + "px";
            }
        }
    }
    else {
        if (oDivGlobal)
            oDivGlobal.style.height = (parseInt(h)) + "px";
        if (oDivCatVal)
            oDivCatVal.style.height = (parseInt(h) - 140) + "px";
    }
    adjustColsWidth();

}
    </script>
    <div id="divCatDialog" runat="server">
    </div>
    <script type="text/javascript" language="javascript">
    <%=EndJSOutput%>
    var bIsTablet = false;
    try {
        if (typeof (isTablet) == 'function')
            bIsTablet = isTablet();
        else if (typeof (top.isTablet) == 'function')
            bIsTablet = top.isTablet();
    }
    catch (e) {

    }

    if (!bIsTablet) {
        // Donne le focus à la textbox de recherche
        if (document.getElementById('eTxtSrch'))
            document.getElementById('eTxtSrch').focus();
    }
    </script>
</body>
</html>
