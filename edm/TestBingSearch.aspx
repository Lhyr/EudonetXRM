<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestBingSearch.aspx.cs" Inherits="Com.Eudonet.Xrm.edm.TestBingSearch" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>Test API Bing Search</title>
    <style>
        h3 {

        }
        #criteres {
            float: left;
        }
        span {
            display: inline-block;
            width: 150px;
            text-align: right;
            margin-right: 10px;
        }
        div {
            margin-bottom: 5px;
        }
        #ButtonSearch {
            width: 100%;
            cursor: pointer;
        }
        #PanelResults {
            clear: both;
        }
        li {
            display: inline-block;
            list-style-type: none;
        }

    </style>
</head>
<body>
    <form id="formBingSearch" runat="server">

    <div id="criteres">
        <div>
            <asp:Label ID="Label4" runat="server">Recherche</asp:Label><asp:TextBox runat="server" ID="TextSearch"></asp:TextBox>
        </div>

            <div>
                <asp:Label ID="Label5" runat="server">Taille</asp:Label><asp:DropDownList runat="server" ID="DdlSize">
                    <asp:ListItem Text="Small"></asp:ListItem>
                    <asp:ListItem Text="Medium"></asp:ListItem>
                    <asp:ListItem Text="Large"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div>
                <asp:Label ID="Label1" runat="server">Couleur</asp:Label><asp:DropDownList runat="server" ID="DdlColor">
                    <asp:ListItem Text="Color"></asp:ListItem>
                    <asp:ListItem Text="Monochrome"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div>
                <asp:Label ID="Label3" runat="server">Style</asp:Label><asp:DropDownList runat="server" ID="DdlStyle">
                    <asp:ListItem Text="Photo"></asp:ListItem>
                    <asp:ListItem Text="Graphics"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div>
                <asp:Label ID="Label2" runat="server">Nb de résultats</asp:Label><asp:TextBox runat="server" ID="TextTop" Text="20"></asp:TextBox>
            </div>

            <asp:Button runat="server" ID="ButtonSearch" OnClick="ButtonSearch_Click" Text="GO" />
        </div>
        


    <asp:Panel runat="server" ID="PanelResults">

    </asp:Panel>
    </form>
</body>
</html>
