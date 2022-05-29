<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="eAdminAutocompleteAddress.aspx.cs" Inherits="Com.Eudonet.Xrm.eda.eAdminAutocompleteAddress" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Administrer la recherche d'adresse prédictive</title>
    <asp:PlaceHolder runat="server" ID="scriptHolder"></asp:PlaceHolder>
    <style type="text/css"">
        #divMapping label
        {
            display: inline-block;
            width: 150px;
            text-align: right;
        }

        #divMapping select
        {
            width: 200px;
        }
    </style>
    <script type="text/javascript">
        loadListTab();
    </script>
</head>
<body>
    <h1>
        Formulaire d'administration de la recherche prédictive d'adresses
    </h1>    
    <form id="form1" runat="server">
        <div id="divSelectTab">
            <label for="tabid">Onglet : </label>
            <select id="tabid" name="tabid" onchange="loadMapping();">
            </select>            
        </div>
        <br />
        <div id="divSelectField">
            <label for="fieldid">Rubrique : </label>
            <select id="fieldid" name="fieldid">
            </select>
            <input type="button" id="btnSetAutocompleteField" value="Définir comme rubrique de recherche prédictive" onclick="setAutocompleteField();"/>
        </div>
        <br />
        <div id="divTriggers">
            A la sélection d’une suggestion, déclencher les automatismes avant enregistrement :
            <input type="radio" id="rdTriggerYes" name="rdTrigger" value="yes" /><label for="rdTriggerYes">Oui</label>
            <input type="radio" id="rdTriggerNo" name="rdTrigger" value="no" /><label for="rdTriggerNo">Non</label>
            <br />
            A la sélection d’une suggestion, déclencher l’automatisme natif depuis l’onglet PM :
            <input type="radio" id="rdTriggerPMYes" name="rdTriggerPM" value="yes" disabled="disabled" /><label for="rdTriggerPMYes">Oui</label>
            <input type="radio" id="rdTriggerPMNo" name="rdTriggerPM" value="no" disabled="disabled" /><label for="rdTriggerPMNo">Non</label>
        </div>
        <br />
        <div id="divMapping">
            <label for="ddlHousenumber">Housenumber</label>
            <select id="ddlHousenumber" name="ddlHousenumber" disabled="disabled"></select>
            <input type="hidden" id="hdnHousenumber" name="hdnHousenumber" disabled="disabled"/>
            <br />

            <label for="ddlStreet">Street</label>
            <select id="ddlStreet" name="ddlStreet"></select>
            <input type="hidden" id="hdnStreet" name="hdnStreet" />
            <br />

            <label for="ddlPlace">Place</label>
            <select id="ddlPlace" name="ddlPlace" disabled="disabled"></select>
            <input type="hidden" id="hdnPlace" name="hdnPlace" disabled="disabled"/>
            <br />

            <label for="ddlVillage">Village</label>
            <select id="ddlVillage" name="ddlVillage" disabled="disabled"></select>
            <input type="hidden" id="hdnVillage" name="hdnVillage" disabled="disabled"/>
            <br />

            <label for="ddlTown">Town</label>
            <select id="ddlTown" name="ddlTown" disabled="disabled"></select>
            <input type="hidden" id="hdnTown" name="hdnTown" disabled="disabled"/>
            <br />

            <label for="ddlCity">City</label>
            <select id="ddlCity" name="ddlCity"></select>
            <input type="hidden" id="hdnCity" name="hdnCity" />
            <br />

            <label for="ddlPostcode">Postcode</label>
            <select id="ddlPostcode" name="ddlPostcode"></select>
            <input type="hidden" id="hdnPostcode" name="hdnPostcode" />
            <br />

            <label for="ddlCitycode">Citycode</label>
            <select id="ddlCitycode" name="ddlCitycode" disabled="disabled"></select>
            <input type="hidden" id="hdnCitycode" name="hdnCitycode" disabled="disabled"/>
            <br />

            <label for="ddlLatitude">Latitude</label>
            <select id="ddlLatitude" name="ddlLatitude" disabled="disabled"></select>
            <input type="hidden" id="hdnLatitude" name="hdnLatitude" disabled="disabled"/>
            <br />

            <label for="ddlLongitude">Longitude</label>
            <select id="ddlLongitude" name="ddlLongitude" disabled="disabled"></select>
            <input type="hidden" id="hdnLongitude" name="hdnLongitude" disabled="disabled"/>
            <br />

            <input type="button" id="btnSetMapping" value="Valider" onclick="setMapping();"/>
        </div>
    </form>
</body>
</html>
