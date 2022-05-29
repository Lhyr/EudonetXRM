<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eMarkedFileDialog.aspx.cs"
    Inherits=" Com.Eudonet.Xrm.eMarkedFileDialog" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    

</head>
<body class="eMarkedFileEditor" onload="init();">
    <script type="text/javascript" language="javascript">
        var eMFEObject;

        function init() {
            // cette fonction est appelée deux fois car en fonction des navigateurs 
            // elle peut être exécutée avant que le ficher emarkedfile.js ne soit chargé
            if (typeof(eMFEObject) == "undefined" && typeof(eMarkedFileEditor) == "function" )
                eMFEObject = new eMarkedFileEditor('eMFEObject');		
        }

        <%= pageHeaderJSOutput %>
    </script>
    <div runat="server" id="divCatalogFinal" />
    <div id="waiter" class="waitOff">
    </div>
    <script type="text/javascript" language="javascript">
        <%= pageFooterJSOutput %>


        // met envisu 
        if(typeof(nSelectedId)!="undefined")
            document.getElementById("eMfId" + nSelectedId ).scrollIntoView(true);

        function switchSelected(id)
        {
            var selectedClass = "eMarkedFileSelected";
            var basicClass = "eMarkedFileEditValues";

            if(typeof(nSelectedId)!="undefined")
                var currentSelected = document.getElementById("eMfId" + nSelectedId );

            if(currentSelected)  {

                if (document.getElementById("markedFileTab").rows.namedItem("eMfId" + nSelectedId).rowIndex % 2 == 1)
                    basicClass = "eMarkedFileEditValues2";

                currentSelected.setAttribute("class", basicClass)
            }

            nSelectedId = id;

            currentSelected = document.getElementById("eMfId" + nSelectedId );
            currentSelected.setAttribute("class", selectedClass)

            document.setSelect(id);
        }
       init();
    </script>
</body>
</html>
