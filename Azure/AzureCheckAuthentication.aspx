<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AzureCheckAuthentication.aspx.cs" Inherits="Com.Eudonet.Xrm.AzureCheckAuthentication" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style>
        body {
            display: flex;
            justify-content: center;
            padding: 75px;
            font-family: lato-regular;
            font-size: 16pt;
        }

        #GlobalContentDiv {
            display: flex;
        }


        #TextualDiv p, #TextualDiv h3 {
            margin: 1.3em 15px 1.3em 15px;
        }

        #TextualDiv h3 {
            color: #e6413f;
            font-family: lato-bold;
            font-size: 1.5em;
            white-space: nowrap
        }

        #TextualDiv #LinkDiv {
            margin-top: 2em;
            display: flex;
            justify-content: space-evenly;
            flex-wrap: wrap;
        }

            #TextualDiv #LinkDiv a {
                margin: 0.2em 1em;
                white-space: nowrap;
                font-size: 0.9em;
            }

        p {
            text-align: justify;
        }

        ul {
            list-style: none;
            padding: 0;
            margin: 0;
        }

        li {
            padding-left: 40px;
            text-indent: -16px;
            font-size: 0.8em;
        }

            li::before {
                content: "• ";
                color: #e6413f;
                line-height: 0.8;
                vertical-align: text-bottom;
                font-size: 1.3em;
            }

            li highlight {
                white-space: nowrap;
            }

        a, a:visited {
            color: #337ab7;
        }

        highlight {
            color: #e6413f;
            font-family: lato-bold;
        }

        .authok #EudoBoy {
            content: url(../IRISBlack/Front/Scripts/components/IrisPurpleFile/img/Buzzy_Success_alt.png);
        }

        .authok #MessageDiv{
            margin-bottom:0.5em;
        }




        .authdown #TextualDiv ul {
            display: none;
        }

        .authdown #EudoBoy {
            content: url(../IRISBlack/Front/Scripts/components/IrisPurpleFile/img/Buzzy_Error.png);
        }
    </style>

</head>
<body>
    <form id="form1" runat="server">
        <div runat="server" id="GlobalContentDiv">
            <img id="EudoBoy" />
            <div id="TextualDiv">
                <h3 runat="server" id="TitleH3"></h3>
                <p runat="server" id="MessageDiv"></p>
                <ul>
                    <li runat="server" id="EudoDiv"></li>
                    <li runat="server" id="AzureDiv"></li>
                </ul>
                <p runat="server" id="AddonInformationDiv"></p>
                <div runat="server" id="LinkDiv"></div>
            </div>
            <input runat="server" id="resultInput" style="display: none" />

        </div>
    </form>
</body>
</html>
