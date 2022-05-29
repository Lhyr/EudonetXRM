<%@ Import Namespace="Com.Eudonet.Xrm" %>
<%@ Import Namespace="Com.Eudonet.Internal" %>

<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eGoogleImageGet.aspx.cs"
    Inherits="Com.Eudonet.Xrm.eGoogleImageGet" %>

<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>


    <script type="text/javascript" lang="javascript">
        var modalVarName;
        var frameModal;
        var nTab = 0;
        var nFileId = 0;
        var sEmptyIconPath;
        var sFullInconPath;
        function init() {
            frameModal = top;
            modalVarName = frameModal["<%=_modalVarName %>"];
            <%=sbJsInput.ToString() %>
            if (top.setWait != null && typeof (top.setWait) == 'function')
                top.setWait(false);
        }
        /*
            fonction appelée après la validation côté serveur, permettant de mettra à jours les les avatar affichées sur la fiche.
        */
        function onImageSubmit(urlImg, sPhy) {

            if (top.setWait != null && typeof (top.setWait) == 'function')
                top.setWait(false);

            //#55196 Si on met a jour l'avatar utilisateur, ne pas modifier l'avatar de la fiche PP ou PM
            if (nTab != 101000 && top.document.getElementById("vcImg")) {
                //mode fiche
                top.document.getElementById("vcImg").style.backgroundImage = (urlImg != "") ? "url('" + urlImg + "')" : "";

            } else if (nTab == 101000) {

                var oAvatar = top.document.getElementById("UserAvatar");
                if (oAvatar && getAttributeValue(oAvatar, "userid") == nFileId) {
                    oAvatar.src = urlImg + '?t=' + new Date().getTime();;
                }


            }

            var bModeList = top.document.getElementById("fileDiv_" + nTab) == null;

            // préselection des items concerné
            var oLst = top.document.querySelectorAll("td[id*='00_" + (nTab + 75) + "_'][id*='" + nFileId + "_']");
            if (oLst.length > 0) {

                for (var nI = 0; nI < oLst.length; nI++) {
                    //
                    var oCurrentTD = oLst[nI];

                    //On vérifie plus précisément le format
                    var sID = oCurrentTD.id;
                    var aID = sID.split("_");
                    if (aID.length > 2) {
                        sBaseId = aID[aID.length - 2]
                        if (sBaseId == nFileId) {
                            var myImg = oCurrentTD.querySelector("img");
                            if (myImg == null)
                            {
                                var varoDiv = oCurrentTD.querySelector("div[data-eemptypicturearea]");
                                oCurrentTD.removeChild(varoDiv);

                                myImg = document.createElement("img");
                                myImg.setAttribute("fid", nFileId);
                                myImg.setAttribute("tab", nTab);
                                myImg.style = "border-width:0px;max-height:100%;max-width:100%;";
                                oCurrentTD.appendChild(myImg);
                                
                            }


                            if (urlImg == "") {
                                myImg.src = sEmptyIconPath + '?t=' + new Date().getTime();
                            }
                            else {
                                if (bModeList)
                                    myImg.src = sFullIconPath;
                                else {
                                    myImg.src = urlImg.replace("_thumb", "") + '?t=' + new Date().getTime();;
                                    myImg.style = "border-width:0px;max-height:100%;max-width:100%;"
                                    setAttributeValue(oCurrentTD, "dbv", sPhy);
                                }
                            }

                        }
                    }

                }
            }

            //Popup
            if (top.eModFile) {

                var oImgTd = top.eModFile.getIframe().document.querySelectorAll("td[id*='00_" + (nTab + 75) + "_'][id*='" + nFileId + "_']");
                if (oImgTd.length == 1) {

                    setAttributeValue(oImgTd[0], "dbv", sPhy);
                    var oImg = oImgTd[0].querySelector("img");

                    if (oImg == null) {
                        var varoDiv = oImgTd[0].querySelector("div[data-eemptypicturearea]");
                        oImgTd[0].removeChild(varoDiv);

                        oImg = document.createElement("img");
                        oImg.setAttribute("fid", nFileId);
                        oImg.setAttribute("tab", nTab);
                        oImg.style = "border-width:0px;max-height:100%;max-width:100%;";
                        oImgTd[0].appendChild(oImg);

                    }

                    oImg.src = urlImg.replace("_thumb", "") + '?t=' + new Date().getTime();
                    oImg.style = "border-width:0px;max-height:100%;max-width:100%;"
                }


            }

            modalVarName.fade(250);
            //setTimeout(modalVarName.hide, 250);
        }



        var oOldSelImg;
        /*
            fonction appelée à la sélection d'une image sur l'interface Google 
        */
        function onGetImageClose(oImg, strType) {
            document.getElementById("FromGoogle").checked = true;
            document.getElementById("tbImageGoogle").value = oImg.src;
            if (((typeof (oOldSelImg) != "undefined") || (oOldSelImg)) && (oImg != oOldSelImg))
                oOldSelImg.className = "";
            oImg.className += " avatar_dl-selected";
            oOldSelImg = oImg;
        }
        /*onFileSelect :
            fonction appelée pour cochée la case depuis le poste en local
        */
        function onFileSelect() {
            document.getElementById("FromLocal").checked = true;
        }
    </script>
</head>
<body onload="init()">
    <form id="formGoogleImageGet" runat="server" class="eMainContainer">
        <asp:Panel ID="DivCurrentPicture" class="divCurrentPicture" runat="server" ondragover="UpFilDragOver(this, event);return false;" ondragleave="UpFilDragLeave(this); return false;" ondrop="UpFilDrop(this,event,null,null,1);return false;">
        </asp:Panel>
        <!-- OBSOLETE -->
        <div class="avatar-search_mode" style="display: none;">
            <input type="radio" id="FromGoogle" name="FromSrc" runat="server" />
            <input type="hidden" id="tbImageGoogle" name="newImageGoogle" runat="server" />
            <label for="FromGoogle">
                <%=eResApp.GetRes(_pref, 6287)%></label>
        </div>
        <div id="DivFromGoogle" style="display: none;">
            <div class="avatar-search">
                <span>
                    <%=eResApp.GetRes(_pref,595)%>
                :</span>
                <asp:TextBox runat="server" ID="txtKeyword" AutoPostBack="True"></asp:TextBox>
            </div>
            <div>
                <span style="margin-bottom: 20px;">
                    <%=eResApp.GetRes(_pref, 6284)%>
                :</span>
                <asp:TextBox ID="txtresultCount" runat="server" AutoPostBack="True" CssClass="avatar-nbr_rslt"
                    value="10" size="3" Columns="2" MaxLength="2"></asp:TextBox>
            </div>
            <div class="avatar-table-container">
                <asp:Table ID="GoogleImageResults" border="0" Style="width: 100px;" runat="server">
                </asp:Table>
            </div>
        </div>
        <div class="avatar-search_mode">
            <input type="radio" id="FromLocal" name="FromSrc" runat="server" checked="true" />
            <label for="FromLocal">
                <%=eResApp.GetRes(_pref, 6285)%></label>
        </div>
        <div id="DivFromLocal">
            <label style="margin-bottom: 20px;" for="myfile">
                <%=eResApp.GetRes(_pref, 6286)%>
            :</label>
            <div>
                <asp:Label ID="lblError" runat="server"></asp:Label>
                <input id="filMyFile" style="width: 100%" type="file" runat="server" onclick="onFileSelect();" />
            </div>
            <div>
                <%=eResApp.GetRes(_pref, 6320).Replace("<VALUE>", "jpg, png et gif")%>
            </div>
        </div>
        <div style="text-align: right; display: none;">
            <asp:Button CssClass="button-green-old" ID="cmdSend" runat="server" Text="<%=eResApp.GetRes(_pref.Lang, 5003)%>"
                OnClick="cmdSend_Click"></asp:Button>
            <asp:Button CssClass="button-red-old" ID="cmdDelete" runat="server" Text="<%=eResApp.GetRes(_pref.Lang, 6347)%>"
                OnClick="cmdDelete_Click"></asp:Button>
        </div>
        <br />
    </form>
</body>
</html>
