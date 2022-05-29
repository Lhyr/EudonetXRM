<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminTest.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.AdminTest" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
    <script type="text/javascript" src="../Scripts/eTools.js"></script>
    <script type="text/javascript" src="../Scripts/eModalDialog.js"></script>
    <script type="text/javascript" src="../Scripts/eUpdater.js"></script>
    <script type="text/javascript" src="Scripts/eAdmin.js"></script>
    <script type="text/javascript">


        function GenJson(tabfld) {
            var descid = "0";
            if (document.getElementById(tabfld + "descid")) {
                descid = document.getElementById(tabfld + "descid").value;
            }
            var caps = new Capsule(descid);
            var s = document.getElementById(tabfld + "pty");
            var sPty = s.options[s.selectedIndex].value.split("|");
            caps.AddProperty(sPty[0], sPty[1], document.getElementById(tabfld + "value").value, sPty.length > 2 ? sPty[2] : null);
            document.getElementById(tabfld + "json").value = JSON.stringify(caps);
        }

        function SendJson(tabfld) {
            GenJson(tabfld);

            var json = document.getElementById(tabfld + "json").value;
            var upd;

            if (tabfld == "config" || tabfld == "configdefault") {
                upd = new eUpdater("Mgr/eAdminConfigManager.ashx", 0);
            }
            else if (tabfld == "cfgadv") {
                upd = new eUpdater("Mgr/eAdminConfigAdvManager.ashx", 0);
            }
            else {
                upd = new eUpdater("Mgr/eAdminDescManager.ashx", 0);
            }

            upd.json = json;
            upd.send(null);
        }

        function update(tabfld) {
            GenJson(tabfld);
            SendJson(tabfld);
        }

        function focusField(fieldId) {
            document.getElementById(fieldId).value = '';
            document.getElementById(fieldId).focus();
        }

    </script>
    <style>
        #page {
            width: 50%;
            margin: auto;
            font-family: Verdana, 'Trebuchet MS';
        }

        textarea {
            width: 100%;
            height: 100px;
            margin-top: 5px;
        }
        h2 {
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div id="page">
        <div>
            <h2>Mise à jour table</h2>
            <input id="tabdescid" name="tabdescid" />
            <select id="tabpty" name="tabpty">
                <option value="<%=interPM%>">Lier à Société</option>
                <option value="<%=interPP%>">Lier à Contacts</option>
                <%--<option value="<%=interADR%>">Lier à Adresses</option>--%>
                <option value="<%=interEVT %>">Lier à un autre fichier principal</option>
                <option value="<%=lFr %>">Label Français</option>
                <option value="<%=lEn%>">Label Anglais</option>
            </select>
            <input id="tabvalue" name="tabvalue" />
            <button onclick="GenJson('tab')">Générer le json</button>
            <textarea id="tabjson" name="tabjson"></textarea>
            <button onclick="SendJson('tab')">Envoyer</button>
        </div>
        <div>
            <h2>Mise à jour champ</h2>
            <input id="flddescid" name="flddescid" placeholder="DescID" />

            <select id="fldpty" name="fldpty" onchange="focusField('fldvalue');">
                <option value="<%=sLength%>">Taille du champ</option>
                <option value="<%=sCase%>">Casse</option>
                <option value="<%=sObligat%>">Champ obligatoire</option>
                <option value="<%=sTooltip%>">Infobulle</option>
                <option value="<%=lFr %>">Label Français</option>
                <option value="<%=lEn%>">Label Anglais</option>
            </select>
            <input id="fldvalue" name="fldvalue" placeholder="Valeur" />
            <button onclick="update('fld')">Mettre à jour</button>
            <button onclick="GenJson('fld')">Générer le json</button>
            <textarea id="fldjson" name="fldjson"></textarea>
            <button onclick="SendJson('fld')">Envoyer</button>
        </div>
        <div>
            <h2>Mise à jour CONFIG</h2>
            <input id="userid" name="userid" placeholder="User ID" />
            <select id="configpty" name="configpty">
                <option value="<%=sOfficeRelease%>">Version Office</option>
                <option value="<%=sTooltipEnabled%>">Infobulles actives</option>
            </select>
            <input id="configvalue" name="configvalue" placeholder="Valeur" />
            <button onclick="update('config')">Mettre à jour</button>
            <button onclick="GenJson('config')">Générer le json</button>
            <textarea id="configjson" name="configjson"></textarea>
            <button onclick="SendJson('configjson')">Envoyer</button>
        </div>
        <div>
            <h2>Mise à jour CONFIG DEFAULT</h2>
            <select id="configdefaultpty" name="configdefaultpty">
                <option value="<%=sServerSMTP%>">Serveur SMTP</option>
            </select>
            <input id="configdefaultvalue" name="configdefaultvalue" placeholder="Valeur" />
            <button onclick="update('configdefault')">Mettre à jour</button>
            <button onclick="GenJson('configdefault')">Générer le json</button>
            <textarea id="configdefaultjson" name="configdefaultjson"></textarea>
            <button onclick="SendJson('configdefaultjson')">Envoyer</button>
        </div>
        <div>
            <h2>Mise à jour CONFIGADV</h2>
            <select id="cfgadvpty" name="cfgadvpty">
                <option value="<%=sSectionSep%>">Séparateur de millier</option>
                <option value="<%=sDecimalSep%>">Séparateur de décimales</option>
            </select>
            <input id="cfgadvvalue" name="cfgadvvalue" placeholder="Valeur" />
            <button onclick="GenJson('cfgadv')">Générer le json</button>
            <textarea id="cfgadvjson" name="cfgadvjson"></textarea>
            <button onclick="SendJson('cfgadv')">Envoyer</button>
        </div>

    </div>
</body>
</html>
