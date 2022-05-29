<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eCatalogDialogUser.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eCatalogDialogUser" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title>Catalogue utilisateur</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>

    <script language="javascript" type="text/javascript">
        var isMultipleCatalog =<%= _multiple.ToString().ToLower() %>;     
        var eTV;
        var eCU;
        function init() {        
            eCU = new eCatalogUser(
                'eCU',
                '<%=_parentModalVarName %>',
                'eCatalogDialogUser.aspx',
                '<%= (_multiple)?"1":"0" %>',
                '<%= _nDescId%>',
                '<%=(_fullUserList)?"1":"0" %>',
                '<%=(_showUserOnly)?"1":"0" %>',
                '<%=(_showEmptyGroup)?"1":"0" %>',
                '<%=(_useGroup)?"1":"0" %>',
                '<%=(_showValuePublicRecord)?"1":"0" %>',
                '<%=(_showValueEmpty)?"1":"0" %>',
                '<%=(_showAllUsersOption)?"1":"0" %>'
            );
            eCU.iFrameId = '<%=_iFrameId %>';
            eTV = new eTreeView("eTV","eTVOusr icon-contact","eTVCusr icon-contact","eTVSBusr icon-avatar");
            <%=InitJSOutput%>
        }
    </script>

    <!--[if IE 8]>   <link rel="stylesheet" type="text/css" href="themes/default/css/ie8-styles.css?ver=<%=String.Concat(eConst.VERSION,".", eConst.REVISION) %>" /> <![endif]-->
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
<body class="usr_cat_body <%=BodyCSS %>" onload="init();">
    <div id="mainDiv" runat="server">
        <div class="usr_top_srch">
            <div class="usr_search-text"><%=eResApp.GetRes(_pref.Lang, 595)%> : </div>
            <input id="eTxtSrch" class="usr_search-inpt" type="text" onkeyup="eCU.FindValues(event, this.value);" />
            <div onclick="eCU.BtnSrch();" id="eBtnSrch" title="Chercher" class="icon-magnifier srchFldImg"></div>
        </div>
        <!--Entête-->
        <div id="DivTop" runat="server"></div>

        <div class="usr_cat">
            <div id="usrTreeTitle" runat="server">
            </div>
            <div class="userTree">
                <div class="userTreeCustomValues" runat="server" id="ResultDivCustomValues">
                </div>
                <div class="userTreeValues" runat="server" id="ResultDiv">
                </div>
            </div>
        </div>

        <!--Pied-->
        <div id="DivBottom" class="usr_btm" runat="server"></div>
    </div>
    <script language="javascript" type="text/javascript">
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
            //Donne le focus à la textbox de recherche
            document.getElementById('eTxtSrch').focus();
        }

        // EVENEMENT LORS DU RESIZE DE LA MODALDIALOG DU CATALOGUE UTILISATEUR
        function onFrameSizeChange(w, h) {
            var oDivGlobal = document.getElementById("mainDiv");
            var oDivCatVal = document.getElementById("ResultDiv"); // DIV contenant les valeurs du catalogue
            if (oDivGlobal)
                oDivGlobal.style.height = (parseInt(h)) + "px";
            if (oDivCatVal)
                oDivCatVal.style.height = (parseInt(h) - 220) + "px";
        }
    </script>
</body>
</html>
