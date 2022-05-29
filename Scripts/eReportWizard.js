// Variables pour l assitant des rapports graphoques
var CHART_PIE = "pie"; //camembert
var CHART_SEMI_PIE = "semipie"; //Demi camembert
var CHART_DOUNHNUT = "doughnut"; //ring
var CHART_SEMI_DOUNHNUT = "semidoughnut"; //Demi Anneau 
var CHART_HISTO = "column"; //histogramme
var CHART_BATON = "bar"; //baton
var CHART_AREA = "area"; //aire
var CHART_LINE = "line"; //Linéaire
var CHART_SPLINE = "spline"; //courbe
var CHART_FUNNEL = "funnel"; //funnel
var CHART_PYRAMID = "pyramid"; //pyramid
var CHART_COMBINED = "combine"; //combiné
var CHART_CIRCULARGAUGE = "circulargauge"; //jauge circulaire

var default_chart_type = CHART_HISTO;

//type de chart
var SINGLE_CHART_TYPE = 1;
var MULTI_CHART_TYPE = 2;
var EMPILE_CHART_TYPE = 3;
var SPECIAL_CHART_TYPE = 4;

//ppour sauvegarder les params du  rapport graphique 
var oParamsMem = new Array();

///summary
///Initalise sauvegarde les parametres du rapport graphique
///summary
function SaveParams() {
    //les params par defaut
    var seriestype = oReport.GetParam("seriestype");



    //on sauvegarde les params actuel du report graphique
    for (key in oReport.aReportParams)
        oParamsMem[key] = oReport.GetParam(key);


    //type graphique
    var modelChartType = document.getElementById("chartPanelSelect").getAttribute("model");
    SetDynGraph(modelChartType);

}

///summary
/// Affiche la liste de champs de l'onglet sélectionné
/// <param name="fileList">Id de la liste d'onglets</param>
///summary
function DisplayFieldList(fileList) {
    var fieldList = new Array();
    var tmpDivList = document.getElementsByTagName("span");
    for (var i = 0; i < tmpDivList.length; i++) {
        if (tmpDivList[i].getAttribute("field_list") != null)
            fieldList.push(tmpDivList[i]);
    }

    var fieldId = "editor_field_" + fileList.value;

    if (fileList.value == 0) {
        ShowLinkFileMenu();
    }
    for (idx = 0; idx < fieldList.length; idx++) {
        fieldList[idx].style.display = fieldList[idx].id == (fieldId) ? "inline" : "none";
    }
}



function DispSelFld(obj, UpdatePanels) {

    var sSelChart = oReport.GetParam("typechart");
    var nSelChartType = parseInt(sSelChart.split("|")[0]);
    var nTypeChart = parseInt(sSelChart.split("|")[1]);
    var displayOnlyCat = (nSelChartType == SINGLE_CHART_TYPE && (nTypeChart == 13 || nTypeChart == 14));

    var ddls = document.getElementById(obj.parentElement.id);
    var bCombined = (obj.id.indexOf('Combined') != -1);

    var prefix = '';


    if (bCombined) {
        prefix = obj.id.replace('EtiquettesFile', '');
        prefix = obj.id.replace('ValuesFile', '');
    }

    for (var i = 1; i < ddls.children.length; i++) {


        var select = ddls.children[i];

        if (select.id.indexOf("ValuesOperation") != -1 || select.id.indexOf('EtiquettesGroup') != -1 || select.id == obj.id) {
            continue;
        }

        if (bCombined && (select.id.replace('Z', 'Y') == obj.id || select.id.replace('Y', 'Z') == obj.id || select.id.indexOf('label') != -1))
            continue;

        if (select.id == obj.id.replace("File", "Field") + '_' + obj.value) {
            SetDisplay(true, new Array(select.id));
            if (bCombined)
                document.getElementById(select.id).onchange();
            //On met  à jour l'etiquette de de groupe
            if (obj.id.toLowerCase() == "etiquettesfile") {
                DisplEtiqGroup(select);
            }

        } else {
            if (select.id == '')
                continue;
            SetDisplay(false, new Array(select.id));
        }
    }

    if (UpdatePanels != false)
        UpdatePanelSelectFields(false, prefix);



    if (bCombined && obj.id == prefix + 'EtiquettesFile') {
        var selectedEtiquetteFileValue = obj.options[obj.selectedIndex].value;
        document.getElementById(prefix + 'EtiquettesField_' + selectedEtiquetteFileValue).onchange();
    }

    if (displayOnlyCat) {
        ActiveFieldsForFunnelAndPyramid();
    }

}

function DisplEtiqGroup(obj, id) {

    if (typeof id == 'undefined' || id == null)
        id = 'EtiquettesGroup';
    //Si on a un champs de type date de la ddl etiquettesFields sélectionnée alors on affiche l etiquettes de groupe (jour, mois, année,...) 
    var selectFields = document.getElementById(obj.getAttribute("id"));


    var fmt = selectFields.options[selectFields.selectedIndex].getAttribute("fmt");
    var bDisplayEtiquettesGroupe = fmt == DATE_FORMAT;

    SetDisplay(bDisplayEtiquettesGroupe, new Array(id));
    document.getElementById(id).onchange();
}


function UpdateSelectedGoup(obj, id) {
    if (typeof id == 'undefined' || id == null)
        id = 'EtiquettesGroup';

    var selectFields = document.getElementById(obj.getAttribute("id"));
    document.getElementById(id).selectedIndex = selectFields.options[selectFields.selectedIndex].index;
    setAttributeValue(document.getElementById(id), 'disabled', 'disabled');
    document.getElementById(id).onchange();

}

function UpdatValuesFileList(obj, prefix, fixedValue) {
    var combinedYEtiquettesFile = document.getElementById(top._CombinedY + 'EtiquettesFile');
    var combinedYEtiquettesFileValue = combinedYEtiquettesFile.options[combinedYEtiquettesFile.selectedIndex].value;
    var etiquetteFileValue = obj.options[obj.selectedIndex].value;

    if (fixedValue != null && typeof fixedValue != 'undefined' && !isNaN(parseInt(fixedValue)))
        etiquetteFileValue = fixedValue;
    //console.log(etiquetteFileValue);
    var indexSelected = 0;
    var firstDisplayIndex = 0;
    var nbDisplay = 0;
    var selectedFile = getAttributeValue(document.getElementById(prefix + 'ValuesFile'), 'selectedfile');

    Array.prototype.slice.apply(document.querySelectorAll("select[name='" + prefix + "ValuesFile'] option")).forEach(
        function (optionElem, index) {
            var linkedtab = getAttributeValue(optionElem, 'linkedtab');
            if (linkedtab != '' && (';' + linkedtab + ';').indexOf(';' + etiquetteFileValue + ';') != -1) {
                setAttributeValue(optionElem, 'display', '1');
                optionElem.removeAttribute('disabled');
                nbDisplay++;
                if (nbDisplay == 1)
                    firstDisplayIndex = index;
                if (indexSelected == 0 && selectedFile != '' && selectedFile == getAttributeValue(optionElem, 'value'))
                    indexSelected = index;
            }
            else {


                setAttributeValue(optionElem, 'display', '0');
                setAttributeValue(optionElem, 'disabled', 'disabled');
            }
        }
    );


    if (nbDisplay > 0)
        document.getElementById(prefix + 'ValuesFile').selectedIndex = (indexSelected > 0 ? indexSelected : firstDisplayIndex);

    //déclenchement de l'Event on change sur la DDl des tables pour Display DDL fields
    document.getElementById(prefix + 'ValuesFile').onchange();

    //mise à jour de la liste des fields : séléctionner la premier rubrique qui répond au condiitons: type de champs et popup descid
    document.getElementById(top._CombinedY + 'EtiquettesField_' + combinedYEtiquettesFileValue).onchange();

    UpdatExpressFilterList(etiquetteFileValue, prefix.toLowerCase());
}

function UpdatFilterList(obj, prefix, forcingIndex) {
    var value = obj.options[obj.selectedIndex].value;
    var nbDisplay = 0;

    //BSE: #73 842 =>  Charger la liste des filtres de la table séléctionnée
    document.getElementById(prefix + 'liAddFilter').lastChild.disabled = true;
    setWait(true);
    getFilterListForTab(value, prefix, drawListFilter, prefix + 'liAddFilter');

    if (forcingIndex != null && typeof forcingIndex != 'undefined' || nbDisplay == 1) {
        document.querySelector('#' + prefix + 'ddlfilter').selectedIndex = 0;
        oReport.SetParam(prefix.toLowerCase() + 'filterid', '0');
    }

    var ddlPrefix = 'EtiquettesFile';

    if (oReport.bCircularGauge)
        ddlPrefix = 'ValuesFile';

    //SPH - 73742 : on est sur le comineZ, il faut prendre l'étiquette Z
    var etiquetteFileDdl = document.getElementById(top._CombinedZ + ddlPrefix);
    //  alert(top._CombinedY + ddlPrefix)

    var selectedFile = etiquetteFileDdl.options[etiquetteFileDdl.selectedIndex].value;
    //Mise à jour des filtres express => prise en compte des relation de la tables séléctionnée
    UpdatExpressFilterList(selectedFile, top._CombinedZ.toLowerCase());

}

function UpdatExpressFilterList(etiquetteFileValue, prefix) {
    var nbDisplay = 0;
    var indexSelected = 0;
    var expressFilterFile = prefix + "file_";
    var nbExpressFilter = document.querySelectorAll("select[name*='" + expressFilterFile + "']").length;
    for (var i = 0; i < nbExpressFilter; i++) {
        var filterFile = expressFilterFile + i;
        var sel = document.querySelectorAll("select[name='" + filterFile + "']");

        Array.prototype.slice.apply(document.querySelectorAll("select[name='" + filterFile + "'] option")).forEach(
            function (optionElem, index) {
                var linkedtab = getAttributeValue(optionElem, 'linkedtab');

                if (linkedtab != '' && (';' + linkedtab + ';').indexOf(';' + etiquetteFileValue + ';') != -1) {
                    setAttributeValue(optionElem, 'display', '1');
                    optionElem.removeAttribute('disabled');
                    nbDisplay++;
                    if (indexSelected == 0)
                        indexSelected = index;
                }
                else {

                    //SPH - 73742 
                    //Si l'option a masquer est sélectinné, on place le selectedindex à 0 (cf plus bas)
                    if (optionElem.index == sel.selectedIndex)
                        sel.selectedIndex = 0;

                    setAttributeValue(optionElem, 'display', '0');
                    setAttributeValue(optionElem, 'disabled', 'disabled');
                }
            }
        );

        //SPH - 73742  - On ne change pas systématiquement le selectedindex
        //if (nbDisplay > 0)
        //  document.getElementById(filterFile).selectedIndex = indexSelected;



        //déclenchement de l'Event on change sur la DDl des tables pour Display DDL fields
        onChangeChartReportFile(document.getElementById(filterFile), prefix, false);
        //document.getElementById(filterFile).onchange();

        //mise à jour de la liste des fields : séléctionner la premier rubrique qui répond au condiitons: type de champs et popup descid
        //document.getElementById(top._CombinedY + 'EtiquettesField_' + combinedYEtiquettesFileValue).onchange();
    }




}


function UpdateLineaireGraphiqueSelection(obj, prefix) {
    var selectField = obj;
    var selectedValue = getAttributeValue(selectField, 'selectedValue');
    var fmt = getAttributeValue(selectField.options[selectField.selectedIndex], "fmt");
    var pud = getAttributeValue(selectField.options[selectField.selectedIndex], "pud");
    var lineaireSelectedFile = document.getElementById(prefix + 'EtiquettesFile');
    var histoSelectedFile = document.getElementById(prefix.replace('Z', 'Y') + 'EtiquettesFile');
    var histoSelectedFileValue = histoSelectedFile.options[histoSelectedFile.selectedIndex].value;
    var lineaireSelectedFileValue = lineaireSelectedFile.options[lineaireSelectedFile.selectedIndex].value;
    var lineaireSelectedField = document.getElementById(prefix + 'EtiquettesField_' + lineaireSelectedFileValue);
    var lineaireFmt = getAttributeValue(lineaireSelectedField.options[lineaireSelectedField.selectedIndex], 'fmt');
    var lineairePud = getAttributeValue(lineaireSelectedField.options[lineaireSelectedField.selectedIndex], 'pud');
    var combineZid = prefix + 'EtiquettesField_';


    DisplayCombineZFields(combineZid, fmt, pud)

    var oRes = {};
    oRes.prefix = prefix;
    oRes.selectField = selectField;
    oRes.selectedValue = selectedValue;
    oRes.combineZid = combineZid;
    oRes.lineaireSelectedField = lineaireSelectedField;
    oRes.lineaireSelectedFileValue = lineaireSelectedFileValue;
    oRes.lineaireSelectedFile = lineaireSelectedFile;
    oRes.histoSelectedFile = histoSelectedFile;
    oRes.mainEttiqueteFileIndex = document.querySelector('option[value="' + lineaireSelectedFileValue + '"]').index;

    if ((histoSelectedFileValue != lineaireSelectedFileValue && (fmt != lineaireFmt || pud != lineairePud)) || (histoSelectedFileValue == lineaireSelectedFileValue && oReport.GetId() == 0)) {
        var options = lineaireSelectedField.querySelectorAll('option[display="1"]');
        if (options.length > 0) {
            lineaireSelectedField.selectedIndex = options[0].index;
            setAttributeValue(selectField, 'selectedValue', selectField.options[selectField.selectedIndex].value);
            document.getElementById(top._CombinedZ + 'EtiquettesField_' + lineaireSelectedFileValue).onchange();
            //mise à jour des tables pour le lineaire graph
            DisplayCombineZFiles(prefix);
        } else {

            ReturnError(oRes);
        }
    }
}


function ReturnError(oRes) {

    eAlert(1, top._res_72, top._res_8538, top._res_8539.replace('<TABLE>', oRes.lineaireSelectedFile.options[oRes.lineaireSelectedFile.selectedIndex].text), null, null, function () {

        oRes.selectField.selectedIndex = oRes.selectField.querySelector('option[value="' + oRes.selectedValue + '"]').index;
        oReport.SetParam(top._CombinedY.toLowerCase() + 'etiquettesfield', oRes.selectedValue);

        var fmt = getAttributeValue(oRes.selectField.options[oRes.selectField.selectedIndex], "fmt");
        var pud = getAttributeValue(oRes.selectField.options[oRes.selectField.selectedIndex], "pud");
        DisplayCombineZFields(oRes.combineZid, fmt, pud);

        var options = oRes.lineaireSelectedField.querySelectorAll('option[display="1"]');
        if (options.length > 0)
            oRes.lineaireSelectedField.selectedIndex = options[0].index;

        oRes.histoSelectedFile.selectedIndex = oRes.mainEttiqueteFileIndex;
        document.getElementById(oRes.histoSelectedFile.id).onchange();

    });
}

function DisplayCombineZFields(combineZId, fmt, pud) {
    Array.prototype.slice.apply(document.querySelectorAll("select[name^='" + combineZId + "'] option")).forEach(
        function (optionElem) {
            var combineZIdPud = getAttributeValue(optionElem, 'pud');
            var combineZIdFmt = getAttributeValue(optionElem, 'fmt');

            if (combineZIdFmt == fmt) {
                if (pud == combineZIdPud) {
                    setAttributeValue(optionElem, 'display', '1');
                    optionElem.removeAttribute('disabled');
                }

                else {

                    setAttributeValue(optionElem, 'display', '0');
                    setAttributeValue(optionElem, 'disabled', 'disabled');
                }
            }
            else {

                setAttributeValue(optionElem, 'display', '0');
                setAttributeValue(optionElem, 'disabled', 'disabled');
            }
        }
    );
}


function DisplayCombineZFiles(prefix) {
    Array.prototype.slice.apply(document.querySelectorAll("select[name='" + prefix + "EtiquettesFile'] option")).forEach(
        function (optionElem) {
            var val = getAttributeValue(optionElem, 'value');
            var lineaireFieldsdSelected = document.getElementById(prefix + 'EtiquettesField_' + val);
            var optionDisplay = lineaireFieldsdSelected.querySelectorAll('option[display="1"]');

            if (optionDisplay.length == 0) {


                setAttributeValue(optionElem, 'display', '0');
                setAttributeValue(optionElem, 'disabled', 'disabled');
            }

            else {
                setAttributeValue(optionElem, 'display', '1');
                optionElem.removeAttribute('disabled');
            }
        }
    );
}




///summary
///Méthode appelée par SwitchStep de eWizard.js pour gérer les étapes spécifiques à l'assistant Report
///<param name="step">Etape actuelle </param>
///<param name="reportType">Type de rapport</param>
///summary

function SwitchStepReport(step) {
    if (oReport.GetType() != 6) {
        //traitement dédié à chaque étapes.        
        switch (parseInt(step)) {
            case 1:
                //Chargement de la sélection des rubriques
                LoadFieldList();
                LoadConfigurableFields();
                // gestion des options de filtrage
                ManageFilterMenuDisplay();
                // chargement du menu de tri/regroupement;
                ManageSortMenuDisplay();
            case 2:
                //Chargement de la liste des options sur les champs sélecionnés
                LoadConfigurableFields();
                break;
            case 3:
                //Format du rapport
                ManageFormatDisplay();
                loadSelectedPdf();
                break;
            case 4:
                // Sauvegarde params mapping
                savePdfMapping();
                // Sauvegarde des préférences de police pour les rapports
                saveFontPref();
                // gestion des options de filtrage
                ManageFilterMenuDisplay();
                // chargement du menu de tri/regroupement;
                ManageSortMenuDisplay();
                break;
            case 5:
                //ALISTER => Demande 83 739
                // Sauvegarde params mapping
                savePdfMapping();
                // Sauvegarde des préférences de police pour les rapports
                saveFontPref();
                // Gestion des options liées à Power BI
                ManagePowerBIMenuDisplay();
            default:
                break;
        }
    }
}

///summary
///Appelle la méthode de contrôle de validité d'interface en fonction du type de rapport et de l'étape concernée
///<param name="step">Etape actuelle </param>
///<param name="reportType">Type de rapport</param>
///summary
function ControlReportWizardDisplay(step, reportType) {

    switch (reportType) {
        case 6:
            return ControlChartDisplay(step);
            break;
        case 3:
            return ControlMergeDisplay(step);
            break;
        case 2:
            return ControlExportDisplay(step);
            break;
        case 0:
            return ControlPrintDisplay(step);
        default:
            break;
    }
}

function ControlChartDisplay(step) {

    switch (parseInt(step)) {
        case 1:
            if (oReport.GetParam("typechart") == "") {
                eAlert(1, top._res_6377, top._res_6382); //Vous devez sélectionner un modèle de graphique pour passer à l'étape suivante
                return false;
            }
            else
                return true;
            break;
        case 2:
            if (oReport.GetParam("typechart") == "4|1" &&
                (oReport.GetParam(top._CombinedZ.toLowerCase() + "filterid") == "" || oReport.GetParam(top._CombinedZ.toLowerCase() + "filterid") == "0")) {
                eAlert(1, top._res_6377, top._res_8575); //Vous devez sélectionner Filtre
                return false;
            }
            else
                return true;
            break;
        case 3:
            var editor_saveas = document.getElementById("editor_saveas");
            if (!editor_saveas || editor_saveas.value != "")
                return true;

            editor_saveas.value = oReport.GetParam('title');
            oReport.SetParam("saveas", editor_saveas.value);

            break;
        case 4:


            break;
        default:
            break;
    }
    return true;

}

function ControlExportDisplay(step) {
    var returnValue = true;

    switch (parseInt(step)) {
        case 1:

            var usedFieldList = document.getElementById("ItemsUsed");

            var nSel = 0;
            try {
                nSel = usedFieldList.querySelectorAll("div[value]").length;
            }
            catch (e) {
                nSel = usedFieldList.getElementsByTagName("div").length - 1; // un div caché existe dans la liste
            }

            if (nSel <= 0) {
                eAlert(1, top._res_6377, top._res_6582);
                returnValue = false
            }
            else {
                SetSelectedDescIds();
                returnValue = true;

            }
            break;
        case 2:
            break;
        case 3:
            break;
        case 4:
            break;
        case 5:
            break;
        default:
            break;
    }
    return returnValue;
}

///summary
///Controle les tests d'affichage par étapes de l'assistant
///<TODO>les méthodes pour chaque type de reporting sont des copié/collés de la même racines en attendant d'avoir le spectre total des tests
///Si les tests sont suffisamment identiques dans tous les Type, les méthodes seront fusionnées, sinon elles seront de fait
///suffisamment spécialisées pour justifier leur existence.</TODO>
///<param name="step">Numéro de l'étape à valider </param>
///summary
function ControlPrintDisplay(step) {
    var returnValue = true;

    switch (parseInt(step)) {
        case 1: //Selection des rubriques

            var usedFieldList = document.getElementById("ItemsUsed");

            var nSel = 0;
            try {
                nSel = usedFieldList.querySelectorAll("div[value]").length;
            }
            catch (e) {
                nSel = usedFieldList.getElementsByTagName("div").length - 1; // un div caché existe dans la liste
            }

            if (nSel <= 0) {
                eAlert(1, top._res_6377, top._res_6582);
                returnValue = false
            }
            else {
                SetSelectedDescIds();
                returnValue = true;

            }

            break;

        case 2: //Options des rubriques
            break;
        case 3: //Choix du format
            break;
        case 4: //Filtres/Tris/Regroupements
            break;
        case 5: //Enregistrement
            break;
        default:
            break;
    }
    return returnValue;
}

///summary
///Controle les tests d'affichage par étapes de l'assistant
///<TODO>les méthodes pour chaque type de reporting sont des copié/collés de la même racines en attendant d'avoir le spectre total des tests
///Si les tests sont suffisamment identiques dans tous les Type, les méthodes seront fusionnées, sinon elles seront de fait
///suffisamment spécialisées pour justifier leur existence.</TODO>
///<param name="step">Numéro de l'étape à valider </param>
///summary
function ControlMergeDisplay(step) {
    var returnValue = true;

    switch (parseInt(step)) {
        case 1: //Selection des rubriques
            var usedFieldList = document.getElementById("ItemsUsed");

            var nSel = 0;
            try {
                nSel = usedFieldList.querySelectorAll("div[value]").length;
            }
            catch (e) {
                nSel = usedFieldList.getElementsByTagName("div").length - 1; // un div caché existe dans la liste
            }

            if (nSel <= 0) {
                eAlert(1, top._res_6377, top._res_6582);
                returnValue = false
            }
            else {
                SetSelectedDescIds();
                returnValue = true;

            }
            break;

        case 2: //Options des rubriques
            returnValue = true;
        case 3: //Choix du format
            break;
        case 4: //Filtres/Tris/Regroupements
            break;
        case 5: //Enregistrement
            break;
        default:
            break;
    }
    return returnValue;
}

///summary
///Affiche les blocs d'options supplémentaires à la sélection du format d'export (séparateur pour le format texte, choix du modèle pour les format Excel et OpenOffice
///<param name="value">Format d'export</param>
///Summary
function ManageFormatDisplay() {

    var templateVisible = false;
    var templateText = eTools.getRes(3143)+" @format :";
    var templatetitle = null;
    var formatOption = null
    var pdfOptionsDiv = document.getElementById("editor_template_pdf");
    var pdfOptionsFieldsDiv = document.getElementById("editor_template_pdffields");
    var htmlOptionsFieldsDiv = document.getElementById("editor_template_htmlfields");
    var txtFormatOption = document.getElementById("editor_txtformatoptions");
    var excelTemplateOption = document.getElementById("editor_template_excel");
    var word_OOTemplateOption = document.getElementById("editor_template_texteditors");
    var ooFileOutPutBloc = document.getElementById("editor_template_texteditors_oopenoffice");
    var htmlFontOption = document.getElementById("editor_html_font");

    if (pdfOptionsDiv != null)
        pdfOptionsDiv.style.display = "none";
    if (pdfOptionsFieldsDiv != null)
        pdfOptionsFieldsDiv.style.display = "none";
    if (htmlOptionsFieldsDiv != null)
        htmlOptionsFieldsDiv.style.display = "none";
    if (txtFormatOption != null)
        txtFormatOption.style.display = "none";
    if (excelTemplateOption != null)
        excelTemplateOption.style.display = "none";
    if (word_OOTemplateOption != null)
        word_OOTemplateOption.style.display = "none";
    if (htmlFontOption != null)
        htmlFontOption.style.display = "none";
    //KHA le 05/08/214 - //la gestion du chemin de sortie pour open office est gérée en cs
    //if (ooFileOutPutBloc != null)
    //    ooFileOutPutBloc.style.display = "none";

    textOptionsVisible = false;
    templateVisible = false;

    for (i = 1; i <= 6; i++) {
        formatOption = document.getElementById("editor_format_label_" + i);


        //Blocs d'options additionnelles spécifiques aux formats
        //Masquage des blocs par défaut en début de boucle
        if (!formatOption)
            continue;


        if (formatOption.checked)
            switch (formatOption.value) {
                case "1": //TEXTE
                    if (txtFormatOption != null)
                        txtFormatOption.style.display = "block";

                    document.getElementById("editor_sep").value = oReport.GetParam("sep");
                    document.getElementById("editor_side").value = oReport.GetParam("side");
                    break;
                case "2": //EXCEL
                    templatetitle = document.getElementById("editor_template_excellabel");
                    templateText = templateText.replace("@format", "Microsoft Excel");
                    if (excelTemplateOption != null)
                        excelTemplateOption.style.display = "block";
                    break;
                case "3": //WORD
                    templatetitle = document.getElementById("editor_template_texteditors_label");

                    /* Demande (36218) "US 1491-> tache 3132" */
                    //templateText = templateText.replace("@format", "Microsoft Word");
                    templateText = top._res_2664  //'Indiquer une adresse réseau ou une URL de votre modèle de publipostage'

                    if (word_OOTemplateOption != null)
                        word_OOTemplateOption.style.display = "block";
                    break;
                case "4": //HTML
                    if (htmlOptionsFieldsDiv != null)
                        htmlOptionsFieldsDiv.style.display = "none"; // Caché pour l'instant
                    if (htmlFontOption != null)
                        htmlFontOption.style.display = "block";

                    var browser = new getBrowser();
                    if (browser.isIE) {
                        var options = document.querySelectorAll("#selectFontSize option");
                        for (var i = 0; i < options.length; i++) {
                            options[i].style.fontSize = "8pt";
                        }
                    }
                    break;
                case "5": //Open Office
                    //KHA le 05/08/214 - //la gestion du chemin de sortie pour open office est gérée en cs

                    //templatetitle = document.getElementById("editor_template_texteditors_label");
                    //templateText = templateText.replace("@format", "Open Office");

                    //if (word_OOTemplateOption != null)
                    //    word_OOTemplateOption.style.display = "block";
                    //if (ooFileOutPutBloc != null)
                    //    ooFileOutPutBloc.style.display = "block";
                    break;
                case "6": //PDF
                    if (pdfOptionsDiv != null)
                        pdfOptionsDiv.style.display = "block";
                    if (pdfOptionsFieldsDiv != null)
                        pdfOptionsFieldsDiv.style.display = "block";
                    break;
            }
    }
    if (templatetitle != null)
        SetText(templatetitle, templateText);
    //Blocs d'options additionnelles spécifiques aux formats

    if (oReport.GetParam("format") == "" || oReport.GetParam("format") == null)
        oReport.SetParam("format", GetDefaultFormat(oReport.GetType()));
}

///summary
///Retourne la valeur réelle d'un paramètre pour un type de reporting donné en fonction de son index dans la radio list
///Utilisé pour compenser le fait qu'en V7 l'indice de format n'est pas le même sur chaque type de rapport.
///par exemple Export HHTMl = format 4 Publipostage HTML = Format 1 
///<param name="reportType"> format du champ</param>
///<param name="formatIndex"> format du champ</param>
///summary
function MapFormatValue(reportType, formatIndex) {

    switch (parseInt(reportType)) {
        //impression             
        case 0:
            return "0"; //Impression uniquement au format HTML, le paramètre de format n'est pas impactant, on retourne donc 0
            break;
        //Export             
        case 2:
            return formatIndex; //la liste indexé des radio de l'interface à été conçu avec comme schéma par défaut le schéma de format d'export. les valeurs correspondent.
            break;
        //Publipostage             
        case 3:
            switch (parseInt(formatIndex)) {
                case 1:
                    return "0"; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                case 2:
                    return "0"; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                case 3:
                    return "0"; //si Text Return valeur par défaut : WORD (ne devrait pas arriver)
                case 4:
                    return "1"; //HTML
                case 5:
                    return "0"; //Open Office fonctionne comme word en dehors des paramètres de modèles qui sont différents et gérés dans les champs prévu à cet effet
                case 6:
                    return "2"; //PDF
                default:
                    return "0";
            }
            break;
        default:
            return "0";
    }
}

function GetDefaultFormat(reportType) {
    switch (parseInt(reportType)) {

        case 0:
            return "4"; //En impression le format par défaut( et le seul format) est HTML
        case 2:
            return "1"; //En Export le format par défaut est Texte
        case 3:
            return "0"; //En Publipostage le format par défaut est Microsoft Word
    }
}
///summary
///retourne la liste des clés de paramètres relatives au informations du champs transmises en paramètres.
///<param name="fieldFormat">format du champ</param>
///<param name="fieldDescId">descid du champ</param>
///<param name="fieldPopupDescId">popupdescid du champ</param>
///<param name="fieldIsPopupData">le champ est un catalogue avancé</param>
///Summary
function GetFieldRelatedKeys(fieldFormat, fieldDescId, fieldPopupDescId, fieldIsPopupData) {
    var format = parseInt(fieldFormat);
    var descId = parseInt(fieldDescId);
    var popupDescId = parseInt(fieldPopupDescId);
    var specificParams = "";
    var paramKeys = "";
    switch (format) {
        case 1:
        case 4:
        case 6:
        case 7:
        case 9:
        case 11:
        case 12:
        case 17:
            paramKeys = "count;nw;truncate";
            break;
        case 2:
            paramKeys = "count;nw;monthyear;dateonlyday;noyeardate";
            break;
        case 3:
            paramKeys = "count;nw;bitlabel";
            break;
        case 5:
        case 10:
            paramKeys = "count;min;max;sum;avg;digittochar;nullvalue";
            break;
        case 8:
        case 14:
            paramKeys = "count;uinf";
            break;
        case 9:
            paramKeys = "count;nw";
    }
    if (descId == 201) {
        specificParams = ";particule;concate";
    }
    else if (descId == 401) {
        specificParams = ";adrperso";
    }
    else if (fieldIsPopupData == true) {
        specificParams = ";popupdata";
    }
    else if ((popupDescId % 100) == 1 && popupDescId != descId && fieldIsPopupData == false) {
        specificParams = ";scinfos;";
    }
    return paramKeys + specificParams;
}
///summary
///Ajoute un champ dans la liste des champs sélectionnés pour le rapport
///summary
function AddReportField() {

    var listTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("editor_filelist").options[document.getElementById("editor_filelist").selectedIndex].value;
    var listFrom = document.getElementById("editor_field_" + tabDescId);
    var tabShortDescId = tabDescId.split("_")[1]; //on prends le réel descid de la table et pas de la table lié pour détecter ADR même si lié depuis un fichier
    var fieldDescId = 0;
    var fieldList = null;
    var added = false;


    if (tabShortDescId == 400 || tabShortDescId == 300) {

        var oSelectedItem = listFrom.querySelector("div.SelectedItem");
        fieldDescId = getAttributeValue(oSelectedItem, "value");

        if (fieldDescId == 402 || fieldDescId == 302) {


            added = true;
            SelectItem(listFrom.id, listTo.id);

            eConfirm("1", top._res_6583, top._res_6584, "", wizardAlertWidth, wizardAlertHeight, function () {
                AddPostalFields(fieldDescId);
            }, null);

        }

    }

    if (!added)
        SelectItem(listFrom.id, listTo.id);

    SetSelectedDescIds();
}

//Ajoute les champs complémentaire d'adresse constituant les rubriques postales.
function AddPostalFields(nStartField) {
    var listTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("editor_filelist").options[document.getElementById("editor_filelist").selectedIndex].value;
    var listFrom = document.getElementById("editor_field_" + tabDescId);
    var tabShortDescId = tabDescId.split("_")[0];
    var fieldDescId = 0;
    var fieldList = listFrom.getElementsByTagName("div");


    //Ajout des rubrique postale à l'ajout du champs adr02
    var lstAdr;
    if (tabShortDescId == 300) {
        lstAdr = [302, 304, 307, 309, 310, 303];
    }
    else {
        lstAdr = [402, 404, 407, 409, 410, 403];
    }


    for (var idx = 0; idx < lstAdr.length; idx++) {



        fieldDescId = parseInt(lstAdr[idx]);

        var oDiv = listFrom.querySelector("div[value='" + fieldDescId + "']");

        if (oDiv) {
            setElementSelected(oDiv);
            SelectItem(listFrom.id, listTo.id);

        }


    }

}
///summary
///Supprime un champ de la liste des champs sélectionnés pour le rapport (et le réinjecte dans sa liste d'origine)
///summary
function DelReportField() {
    var listFrom = document.getElementById('ItemsUsed');
    var tabDescId = listFrom.getElementsByTagName("div")[getSelectedIndex(document.getElementById(listFrom.id))].getAttribute("edntab");
    var fromTabDescId = listFrom.getElementsByTagName("div")[getSelectedIndex(document.getElementById(listFrom.id))].getAttribute("linkedFromTab");
    if (fromTabDescId == 0)
        fromTabDescId = tabDescId;
    var listTo = document.getElementById("editor_field_" + tabDescId + "_" + fromTabDescId);
    SelectItem(listFrom.id, listTo.id);
    SetSelectedDescIds();
}
///summary
///Ajoute tous les champs dans la liste des champs sélectionnés pour le rapport
///summary
function AddAllReportFields() {
    var listTo = document.getElementById('ItemsUsed');
    var tabDescId = document.getElementById("editor_filelist").options[document.getElementById("editor_filelist").selectedIndex].value;
    var listFrom = document.getElementById("editor_field_" + tabDescId);
    MoveAllItems(listFrom.id, listTo.id);
    SetSelectedDescIds();
}
///summary
///Supprime tous les champs dans la liste des champs sélectionnés pour le rapport
///summary
function RemoveAllReportFields() {
    var listFrom = document.getElementById('ItemsUsed');
    var oSelectdFields = listFrom.getElementsByTagName("div");
    var tabDescId = null;
    var fromTabDescId = null;
    var listTo = null;
    var originalListLength = oSelectdFields.length;
    var idx = 0;
    for (idx = 0; idx < originalListLength; idx++) {
        tabDescId = oSelectdFields[getSelectedIndex(document.getElementById(listFrom.id))].getAttribute("edntab");
        fromTabDescId = oSelectdFields[getSelectedIndex(document.getElementById(listFrom.id))].getAttribute("linkedFromTab");
        if (fromTabDescId == 0)
            fromTabDescId = tabDescId;
        listTo = document.getElementById("editor_field_" + fromTabDescId + "_" + tabDescId);
        SelectItem(listFrom.id, listTo.id);
    }
    SetSelectedDescIds();
}

///summary
///Récupère la liste des descId correspondants aux champs sélectionnés pour le rapport et les affecte dans l'objet Rapport oReport.
///summary
function SetSelectedDescIds() {
    /*
    <div class="cell2" onclick="setElementSelected(this);" ednformat="1" tooltiptext="Nom" oldCss="cell2" edntab="200" value="201">
    */
    var idx = 0;
    var fields = [];
    var oUsedFields = document.getElementById('ItemsUsed').getElementsByTagName("div");
    for (idx = 0; idx < oUsedFields.length; idx++) {
        if (parseInt(oUsedFields[idx].getAttribute("value")) > 0) {
            if (parseInt(oUsedFields[idx].getAttribute("linkedFromTab")) > 0)
                fields.push(oUsedFields[idx].getAttribute("value") + "," + oUsedFields[idx].getAttribute("linkedFromTab"));
            else
                fields.push(oUsedFields[idx].getAttribute("value"));
        }
    }

    oReport.SetParam("field", fields.join(";"));
    ManageAddressWarningMessage();
}

///summary
///charge les champs sélectionnés dans le rapport dans la liste des champs projetés
///summary
function LoadFieldList() {
    var descIds = oReport.GetParam("field").split(";");

    if (oReport.GetParam("field") == "" || descIds.length == 0)
        return;

    var availableFields = document.getElementById("editor_sourcelist").getElementsByTagName("div");
    var selectedFields = document.getElementById("ItemsUsed");
    var idxDescIds = 0;
    var idxDiv = 0;
    var fieldDescId = 0;
    var linkedFromTabDescId = 0;
    for (idxDescIds = 0; idxDescIds < selectedFields.length; idxDescIds++)
        selectedFields.removeChild(selectedFields[idxDescIds]);

    //Si le champ fait partie de la sélection on l'ajoute dans la liste des champs sélectionnés
    for (idxDescIds = 0; idxDescIds < descIds.length; idxDescIds++) {
        for (idxDiv = 0; idxDiv < availableFields.length; idxDiv++) {

            if (descIds[idxDescIds].indexOf(",") > 0) {
                fieldDescId = descIds[idxDescIds].split(",")[0];
                linkedFromTabDescId = descIds[idxDescIds].split(",")[1];
            }
            else {
                fieldDescId = descIds[idxDescIds];
                linkedFromTabDescId = 0;
            }

            if (availableFields[idxDiv].getAttribute("value") == fieldDescId && availableFields[idxDiv].getAttribute("linkedFromTab") == linkedFromTabDescId) {

                var obj = availableFields[idxDiv];

                var sShLab = getAttributeValue(obj, "shlb");
                var sLgLab = getAttributeValue(obj, "lglb");

                if (sLgLab != "")
                    obj.innerHTML = sLgLab;


                selectedFields.appendChild(availableFields[idxDiv]);
            }


        }


    }

    // Mise à jour des couleurs des lignes
    var tabDescId = document.getElementById("editor_filelist").options[document.getElementById("editor_filelist").selectedIndex].value;
    var listFrom = document.getElementById("editor_field_" + tabDescId);
    setCssList(listFrom, "cell", "cell2");
    setCssList(selectedFields, "cell", "cell2");
}


///summary
///Charge  la liste des champs à configurer.
///summary
function LoadConfigurableFields() {
    var descIds = oReport.GetParam("field").split(";");
    var selectedFields = document.getElementById("ItemsUsed").getElementsByTagName("div");
    var configurableFieldDiv = document.getElementById("editor_configurablelist");
    var idxFields = 0;
    var idxDiv = 0;
    var divField = null;
    var cssclass = "cell2";
    var linkedFromTab = 0;
    var fieldDescId = 0;
    var paramDescId = null;
    var isFirstElement = true;

    //on vide le div si jamais il contenait des élements
    configurableFieldDiv.innerHTML = "";
    for (idxDiv = 0; idxDiv < selectedFields.length; idxDiv++) {
        cssclass = cssclass == "cell2" ? "cell" : "cell2";
        fieldDescId = selectedFields[idxDiv].getAttribute("value");
        linkedFromTab = selectedFields[idxDiv].getAttribute("linkedFromTab");

        for (idxFields = 0; idxFields < descIds.length; idxFields++) {
            paramDescId = descIds[idxFields].split(",");

            if (
                (fieldDescId == paramDescId[0] && paramDescId.length == 1 && linkedFromTab == 0)
                ||
                (fieldDescId == paramDescId[0] && paramDescId.length == 2 && linkedFromTab == paramDescId[1])
            ) {

                divField = document.createElement("div");
                divField.id = "editor_configurablefielddiv_" + selectedFields[idxDiv].getAttribute("value");
                divField.setAttribute("class", cssclass);
                divField.setAttribute("oldCss", cssclass);
                divField.setAttribute("onclick", "DisplayFieldOptions(this);");
                divField.setAttribute("descid", selectedFields[idxDiv].getAttribute("value"));
                divField.setAttribute("ednformat", selectedFields[idxDiv].getAttribute("ednformat"));
                divField.setAttribute("linkedFromTab", selectedFields[idxDiv].getAttribute("linkedFromTab"));
                divField.setAttribute("tooltiptext", selectedFields[idxDiv].getAttribute("tooltiptext"));
                divField.setAttribute("title", GetText(selectedFields[idxDiv]));
                divField.setAttribute("edntab", selectedFields[idxDiv].getAttribute("edntab"));
                divField.setAttribute("value", selectedFields[idxDiv].getAttribute("value"));
                divField.setAttribute("popuptype", selectedFields[idxDiv].getAttribute("popuptype"));
                divField.setAttribute("popupdescid", selectedFields[idxDiv].getAttribute("popupdescid"));


                divField.setAttribute("linkedfile", selectedFields[idxDiv].getAttribute("linkedfile"));
                divField.setAttribute("ismultiple", selectedFields[idxDiv].getAttribute("ismultiple")); // 42068

                if (divField.innerText != null)
                    divField.innerText = selectedFields[idxDiv].innerText;
                else
                    divField.textContent = selectedFields[idxDiv].textContent;

                configurableFieldDiv.appendChild(divField);
                //chargement des paramètres du premier champ
                if (isFirstElement) {
                    DisplayFieldOptions(divField);
                    isFirstElement = false;
                }
            }
        }
    }
}
///summary
///Affiche les options adaptées au format du champ sélectionné dans la liste.
///<param name="field">Div du champ sélectionné</param>
///summary
function DisplayFieldOptions(field) {
    var format = field.getAttribute("ednformat");
    var popupType = field.getAttribute("popuptype");
    var popupDescId = field.getAttribute("popupdescid");
    var descId = field.getAttribute("value");
    var ismultiple = field.getAttribute("ismultiple");
    var optionList = document.getElementById("editor_fieldoptions");
    var options = optionList.getElementsByTagName("li");
    var idx = 1;
    var formatArray = null;
    var display = false;

    //Mise en surbrillance de l'élément sélectionné
    setElementSelected(field);

    //affectation du descid dans le champs input "caché" pour avoir une référence au descid sans avoir à remonter tout le DOM.
    SetText(document.getElementById("editor_fieldlabel"), top._res_6581 + " " + GetText(field) + " :");

    document.getElementById("editor_currentfielddescid").value = descId;

    /*On pars de l'index 1 car le premier LI est la ligne de libellé de la liste.
    Si l'attribut format compatible du LI contient le formatdu champ cliqué, on affiche ce LI.*/

    for (idx = 0; idx < options.length; idx++) {
        display = false;
        if (options[idx].getAttribute("compatibleformats") != null) {
            //cas spécifique de PP01 ==> Afficher les options de concaténation nom +  particule
            //cas particuliers : 

            if (options[idx].id == "editor_concateoption") {
                //PP -> Nom + Prénom
                display = parseInt(descId) == 201;
            }
            else if (options[idx].id == "editor_particuleoption") {
                //PP -> Particule
                display = parseInt(descId) == 201;
            }
            else if (options[idx].id == "editor_adrpersooption") {
                //Adresse.Adresse Masquer le libellé
                display = parseInt(descId) == 401;
            }

            else if (options[idx].id == "editor_popupdataoption") {
                //Catalogues avancés
                if (format == 1 && popupType == 3)
                    display = true;
            }
            else if (options[idx].id == "editor_scinfosoption") {
                //Champs de liaison
                if (format == 1 && popupType == 2 && popupDescId > 0 && descId > 0 && popupDescId != descId) {
                    display = true;

                    //TODO res_XXX à renommer une fois les ressources intégrées.
                    var filename = getAttributeValue(field, "linkedfile");


                    var label = options[idx].getElementsByTagName("span")[1];
                    if (label != null && label.innerText != null)
                        label.innerText = top._res_1424.replace("<TAB>", filename);
                    else
                        label.textContent = top._res_1424.replace("<TAB>", filename);
                }
            }
            else if (options[idx].id == "editor_uinfoption") {
                // Afficher le détail de l'utilisateur
                if (ismultiple == 0 && format == 8) {
                    display = true;
                }
            }
            else {
                formatArray = options[idx].getAttribute("compatibleformats").split(";");

                var subIdx = 0;
                for (subIdx = 0; subIdx < formatArray.length; subIdx++) {
                    if (formatArray[subIdx] == format)
                        display = true;
                }
            }
        }
        else
            display = true;

        options[idx].style.display = display ? "block" : "none";
    }
    LoadFieldSavedParameters(descId, GetFieldRelatedKeys(format, descId, popupDescId, popupType == 3));
}
///summary
///Gere l'affichage des options de champs catalogues avancés
///summary
function ManageFileDataOptions() {
    var checkBox = document.getElementById("editor_popupdata");
    var optionList = document.getElementById("editor_popupdatadisplayoption");
    var langList = document.getElementById("editor_popupdatalangoption");
    var langPanel = document.getElementById("editor_popupdatalangpanel");

    optionList.disabled = checkBox.getAttribute("chk") == 1 ? "" : "disabled";
    langList.selectedIndex = checkBox.getAttribute("chk") == 1 ? langList.selectedIndex : 0;
    langPanel.style.display = (optionList.value > 1) ? "block" : "none";

    PostFileDataOption();
}

function PostComplexValue(key) {
    var checkBox = document.getElementById("editor_" + key);
    var checked = checkBox.getAttribute("chk") == 1;
    var value = document.getElementById("editor_currentfielddescid").value;
    var element = document.getElementById("editor_configurablefielddiv_" + value);
    if (typeof (element) != "undefined" && typeof (element.getAttribute("linkedfromtab")) != "undefined" && element.getAttribute("linkedfromtab") > 0)
        value = value + "," + element.getAttribute("linkedfromtab");

    oReport.SetComplexParam(key, value, !checked);
}


function PostAdvComplexValue(key, oCheckbox, addedParam) {
    var bdelete = oCheckbox.getAttribute("chk") == "0";
    var currentfielddescid = document.getElementById("editor_currentfielddescid").value;

    oReport.SetComplexParam(key, currentfielddescid + "," + addedParam, bdelete);
}

///summary
///Enregitre dans l'object Javascript oReport la ligne d'options de catalogue avancé pour le champ en cours.
///summary
function PostFileDataOption() {
    var checkBox = document.getElementById("editor_popupdata");
    var optionList = document.getElementById("editor_popupdatadisplayoption");
    var langList = document.getElementById("editor_popupdatalangoption");

    var addedParam = optionList.options[optionList.selectedIndex].value + "," + (optionList.selectedIndex > 0 ? langList.selectedIndex : "0");
    PostAdvComplexValue("popupdata", checkBox, addedParam);
}
///summary
///Charge dans l'objet oReport les paramètres saisis sur le champ checkbox en cours
///summary
function PostCheckBoxOption() {
    var checkBox = document.getElementById("editor_bitlabel");
    var optionList = document.getElementById("editor_bitlabeldisplayoption");

    PostAdvComplexValue("bitlabel", checkBox, optionList.selectedIndex);
}
///summary
///Charge dans l'objet oReport les paramètres de troncature saisis sur le champ checkbox en cours
///summary
function PostTruncateOption() {
    var checkBox = document.getElementById("editor_truncate");
    var truncateChar = document.getElementById("editor_truncatechar");

    truncateChar.disabled = checkBox.getAttribute("chk") == "0" ? "disabled" : "";
    PostAdvComplexValue("truncate", checkBox, truncateChar.value);
}
///summary
///Charge dans l'objet oReport les paramètres de tris/regroupement
///<param name="optionType">Type de menu (sort/group)</param>
///<param name="lineIndex">Index de la ligne d'option à mettre à jour</param>
///summary
function PostSortOrGroupOption(optionType, lineIndex) {
    var fieldBoxName = null;
    var optionBoxName = null;

    if (optionType == "sort") {
        fieldBoxName = "editor_orderby";
        optionBoxName = "editor_ordersort";
        oReport.SetParam("orderby" + lineIndex, document.getElementById(fieldBoxName + lineIndex).value);
        oReport.SetParam("ordersort" + lineIndex, document.getElementById(optionBoxName + lineIndex).value);
    }
    else if (optionType == "group") {
        fieldBoxName = "editor_group";
        optionBoxName = "editor_grouporder";
        oReport.SetParam("group" + lineIndex, document.getElementById(fieldBoxName + lineIndex).value);
        oReport.SetParam("grouporder" + lineIndex, document.getElementById(optionBoxName + lineIndex).value);
    }
    else {
        return;
    }



}
///summary
///gestion de la désactivation/activation des lignes de tri/regroupement
///<param name="dropDownList">Liste de choix de champ</param>
///<param name="optionType">Type d'option (sort/group)</param>
///summary
function ManageOptionLine(dropDownList, optionType) {
    var fieldBoxName = null;
    var optionBoxName = null;
    var lineName = null;

    if (optionType == "sort") {
        fieldBoxName = "editor_orderby";
        optionBoxName = "editor_ordersort";
        lineName = "editor_sortinfos";
    }
    else if (optionType == "group") {
        fieldBoxName = "editor_group";
        optionBoxName = "editor_grouporder";
        lineName = "editor_groupinfos";
    }
    else {
        return;
    }

    if (dropDownList != null) {
        var itemIndex = parseInt(dropDownList.attributes["index"].value);
        if (dropDownList.selectedIndex > 0) {
            if (itemIndex < 3) {
                document.getElementById(fieldBoxName + (itemIndex + 1)).disabled = false;
                document.getElementById(optionBoxName + (itemIndex + 1)).disabled = false;
                document.getElementById(lineName + (itemIndex + 1)).className = "";
                if (optionType == "group")
                    document.getElementById("editor_grouppagebreak" + (itemIndex + 1)).disabled = false;
            }
        }
        else {
            if (itemIndex < 3) {
                for (idx = 3; idx > itemIndex; idx--) {
                    document.getElementById(fieldBoxName + (idx)).disabled = true;
                    document.getElementById(fieldBoxName + (idx)).selectedIndex = 0;
                    document.getElementById(optionBoxName + (idx)).disabled = true;
                    document.getElementById(optionBoxName + (idx)).selectedIndex = 0;
                    document.getElementById(lineName + (idx)).className = "disabledline";
                    if (optionType == "group")
                        document.getElementById("editor_grouppagebreak" + idx).disabled = true;
                }
            }
        }
    }
}
///summary
///Gère en fonction des valeurs qui leurs sont affectées, la nature d'affichage des options de tri et regroupement.
///summary
function ManageSortMenuDisplay() {
    var idx = 0;
    var sType;
    var fieldList = oReport.GetParam("field").split(";");
    var fieldTab = 0
    var fieldLinkedFromTab = 0;
    var tabList = "";
    var fieldDescId = 0;
    var fieldDiv = null;
    var FieldLabel = null;
    var sortList = null;
    var groupList = null;
    var processedDescIds = ";";
    var opt = null;
    var format = oReport.GetParam("format").split(";");

    // Pour Power BI, pas d'option de cumul des filtres en cours, ni d'options de tri - cf. captures d'écran dans les spécs
    var bDisplaySortOptions = (format != 7);
    if (document.getElementById("editor_sortinfos"))
        document.getElementById("editor_sortinfos").style.display = bDisplaySortOptions ? "block" : "none";

    //reset des dropdownlist avant mise à jour
    for (idx = 1; idx <= 3; idx++) {
        sortList = document.getElementById("editor_orderby" + idx)
        groupList = document.getElementById("editor_group" + idx)

        if (sortList != null) {
            while (sortList.options.length > 0)
                sortList.options.remove(0);

            sortList.options.add(new Option(top._res_167, "0"));

        }

        if (groupList != null) {
            while (groupList.options.length > 0)
                groupList.options.remove(0);
        }



    }


    //chargement des champs projetés du rapport dans chacune des listes de tri/regroupement.

    for (fieldIdx = 0; fieldIdx < fieldList.length; fieldIdx++) {
        //détermination de la table parente du champ
        if (fieldList[fieldIdx].indexOf(',') > 0) // champs liés
        {
            fieldLinkedFromTab = parseInt(fieldList[fieldIdx].split(',')[1])
            fieldDescId = parseInt(fieldList[fieldIdx].split(',')[0])
            fieldTab = fieldTab - (fieldTab % 100);
        }
        else if (fieldList[fieldIdx] == "") {
            continue;
        }
        else {
            fieldDescId = parseInt(fieldList[fieldIdx])
            fieldTab = fieldTab - (fieldTab % 100);
            fieldLinkedFromTab = fieldTab;
        }

        fieldDiv = null;
        fieldLabel = null;

        fieldDiv = document.getElementById("editor_configurablefielddiv_" + fieldDescId);

        if (processedDescIds.indexOf(";" + fieldDescId + ";") >= 0)
            continue;

        if (fieldDiv)
            fieldLabel = GetText(fieldDiv);
        else if (fieldIdx == 0 && fieldDescId == 0)
            fieldLabel = top._res_167;
        else
            continue;

        for (idx = 1; idx <= 3; idx++) {
            sortList = document.getElementById("editor_orderby" + idx)
            groupList = document.getElementById("editor_group" + idx)

            if (sortList != null) {
                opt = new Option(fieldLabel, fieldList[fieldIdx]);
                if (opt.value == oReport.GetParam("orderby" + idx)) // #57053
                    opt.selected = true;
                sortList.options.add(opt);
            }

            if (groupList != null) {
                if (fieldIdx == 0) {
                    groupList.options.add(new Option(top._res_167, "0")); // Pas de tri
                }
                opt = new Option(fieldLabel, fieldList[fieldIdx]);
                if (fieldDescId == parseInt(oReport.GetParam("group" + idx)))
                    opt.selected = true;
                groupList.options.add(opt);
            }
        }

        processedDescIds = processedDescIds + fieldDescId + ";"
    }


    //tris
    sType = "sort";
    for (idx = 1; idx <= 3; idx++)
        ManageOptionLine(document.getElementById("editor_orderby" + idx), sType);
    //regroupement
    sType = "group";
    for (idx = 1; idx <= 3; idx++)
        ManageOptionLine(document.getElementById("editor_group" + idx), sType);

    //listes construites on envoie l'appel AJAX pour récupérer la listes des champs disponibles
    var upd = new eUpdater("eWizard.aspx", 0);
    upd.asyncFlag = false;
    //upd.addParam("tab", tab, "post");
    upd.addParam("fieldlist", oReport.GetParam("field"), "post");
    upd.addParam("wizardtype", "groupsort", "post");
    upd.ErrorCallBack = function () { };  //TODO Gestion d'erreur
    upd.send(GetGroupAndSortFields, null);
}

///summary
///
///<param name="fieldList">Flux XML contenant les informations des champs à ajouter dans les dropDownLists</param>
///summary
function GetGroupAndSortFields(fieldListResult) {

    var opt = null;
    var success = getXmlTextNode(fieldListResult.getElementsByTagName("success")[0]);
    if (success != "1") {
        alert(getXmlTextNode(fieldListResult.getElementsByTagName("ErrorDescription")[0]));
        return;
    }

    var fieldList = fieldListResult.getElementsByTagName("field");

    if (fieldList == null)
        return;

    var fieldLabel = "";
    var fieldValue = null;
    var fieldLinkedFromTab = null;
    var innerIndex = 0;
    var sortList;

    // #57053 : Sélection de la valeur si elle existe déjà dans la dropdownlist, pour chaque ligne
    for (idx = 1; idx <= 3; idx++) {
        sortList = document.getElementById("editor_orderby" + idx);
        if (sortList)
            sortList.value = oReport.GetParam("orderby" + idx);
    }

    for (fieldIdx = 0; fieldIdx < fieldList.length; fieldIdx++) {
        fieldLabel = decode(GetText(fieldList[fieldIdx]));;
        fieldLinkedFromTab = fieldList[fieldIdx].getAttribute("linkedfromtab");
        //KJE #65 565: s'il s'agit d'un élément appartenant à une table liéée, on rajoute l'id en table 
        fieldValue = fieldLinkedFromTab ? fieldList[fieldIdx].getAttribute("descid") + ',' + fieldLinkedFromTab : fieldList[fieldIdx].getAttribute("descid");
        
        for (idx = 1; idx <= 3; idx++) {
            sortList = document.getElementById("editor_orderby" + idx)
            groupList = document.getElementById("editor_group" + idx)
            if (sortList != null) {
                //ajouts des séparateurs avec les champs non sélectionnées dans la projection
                if (fieldIdx == 0)
                    sortList.options.add(new Option("----------", "0"));
                opt = new Option(fieldLabel, fieldValue);

                if (fieldValue == oReport.GetParam("orderby" + idx))
                    opt.selected = true;

                sortList.options.add(opt);
            }

            if (groupList != null) {
                //ajouts des séparateurs avec les champs non sélectionnées dans la projection
                if (fieldIdx == 0)
                    groupList.options.add(new Option("----------", "0"));
                opt = new Option(fieldLabel, fieldValue);
                if (fieldValue == oReport.GetParam("group" + idx))
                    opt.selected = true;
                groupList.options.add(opt);
            }
        }
    }
}

///summary
///Gère en fonction du contexte, les options de filtrage
///summary
function ManageFilterMenuDisplay() {
    var format = oReport.GetParam("format").split(";");

    // Pour Power BI, pas d'option de cumul des filtres en cours - cf. captures d'écran dans les spécs
    var bDisplayCurrentFilterOptions = (format != 7);
    if (document.getElementById("editor_addcurrentfilter"))
        document.getElementById("editor_addcurrentfilter").style.display = bDisplayCurrentFilterOptions ? "block" : "none";
}

///summary
///Gère en fonction du contexte, les options liées à Power BI
///summary
function ManagePowerBIMenuDisplay() {
    var format = oReport.GetParam("format").split(";");

    var bDisplayPowerBIOptions = (format == 7);
    if (document.getElementById("editor_powerbi"))
        document.getElementById("editor_powerbi").style.display = bDisplayPowerBIOptions ? "block" : "none";

    // #60 560 - cf. specs - Pas d'options de planification pour le format Power BI
    if (document.getElementById("editor_reportschedule"))
        document.getElementById("editor_reportschedule").style.display = bDisplayPowerBIOptions ? "none" : "block";

    // #63 665 - cf. specs - Si le format Power BI est sélectionné et si on ajoute un nouveau rapport :
    // - l'utilisateur exécutant est celui connecté (présélectionné via le renderer, donc on ajoute la valeur du champ dans les paramètres)
    oReport.SetParam("powerbiuser", getAttributeValue(document.getElementById('editor_powerbi_user_input'), "ednvalue"));

    // - les options de sécurité sont cochées par défaut pour les administrateurs
    if (bDisplayPowerBIOptions && oReport.GetId() == 0) {
        var viewChk = document.getElementById('chk_OptViewFilter');
        var updChk = document.getElementById('chk_OptUpdateFilter');
        var viewUsersChk = document.getElementById('chk_OptLevels_View');
        var updUsersChk = document.getElementById('chk_OptLevels_Update');

        if (getAttributeValue(viewChk, "chk") == "0")
            chgChk(viewChk);
        onCheckOption('OptViewFilter');

        if (getAttributeValue(updChk, "chk") == "0")
            chgChk(updChk);
        onCheckOption('OptUpdateFilter');

        if (getAttributeValue(viewUsersChk, "chk") == "0")
            chgChk(viewUsersChk);
        onCheckOption('OptLevels_View');

        if (getAttributeValue(updUsersChk, "chk") == "0")
            chgChk(updUsersChk);
        onCheckOption('OptLevels_Update');

        document.getElementById('LevelLst_View').value = "99";
        document.getElementById('LevelLst_Update').value = "99";
    }

}

///summary
///Charge la valeur des options sélectionnées sur ce champ depuis l'objet javascript oReport
///<param name="descId">DescId du champ</param>
///<param name="relatedKeys">Clés de paramètre du rapport adaptées à ce champ</param>
///summary
function LoadFieldSavedParameters(descId, relatedKeys) {
    var params = "";
    var paramValue = "";
    var key = "";
    var bActiveParam = false;
    var usedKeys = relatedKeys.split(";");
    var optionDescription = "";

    for (keyIndex = 0; keyIndex < usedKeys.length; keyIndex++) {
        params = null;
        paramValue = "";
        bActiveParam = false;
        key = usedKeys[keyIndex];
        if (key == "")
            continue;

        params = ";" + oReport.GetParam(key) + ";";

        //vérifie si le champs est selectionné pour ce paramètre
        if (params.indexOf(";" + descId + ";") >= 0 || params.indexOf(";" + descId + ",") >= 0) {
            params = oReport.GetParam(key).split(";");
            for (paramIdx = 0; paramIdx < params.length; paramIdx++) {
                if (params[paramIdx].indexOf(",")) {
                    if (parseInt(params[paramIdx].split(",")[0]) == descId) {
                        paramValue = params[paramIdx];
                        bActiveParam = true;
                        break;
                    }
                }
                else {
                    if (parseInt(params[paramIdx]) == descId) {
                        paramValue = params[paramIdx];
                        bActiveParam = true;
                        break;
                    }
                }
            }
        }
        LoadFieldOption(key, paramValue, bActiveParam);
        //optionDescription = optionDescription + GetParamDisplaytext(key, paramValue, bActiveParam);
    }
}
///summary
///Charge une option standard (checkBox sans paramètres complémentaire) dans le menu des options de champ
///<param name="key">clé de paramètre de rapport</param>
///<param name="checked">cochée</param>
///summary
function LoadSimpleOption(key, checked) {

    var item = document.getElementById("editor_" + key);
    if (item == null)
        return;
    item.className = "rChk chk";
    if (checked) {

        chgChk(item, true);

        //    item.setAttribute("chk", "1");
    }
    else {
        chgChk(item, false);
        //  item.setAttribute("chk", "0");
    }
}
///summary
///Charge les options avancées (catalogues avancés, logique, troncature 
///<param name="key">clé de paramètre de rapport</param>
///<param name="value">valeur sélectionnée</param>
///<param name="checked">cochée</param>
///summary
function LoadFieldOption(key, value, checked) {
    LoadSimpleOption(key, checked);
    switch (key) {
        case "popupdata":
            LoadFieldFileDataOption(value, checked);
            break;
        case "bitlabel":
            LoadFieldCheckBoxOption(value, checked);
            break;
        case "truncate":
            LoadFieldTruncateOption(value, checked);
            break;
        default:
            break;
    }
}
///summary
///Charge les paramètres de l'option catalogue avancé dans les controles HTML correspondants
///<param name="value">valeur sélectionnée</param>
///<param name="checked">cochée</param>
///summary
function LoadFieldFileDataOption(value, checked) {
    var param = value.split(",");
    var idxDisplayMask = parseInt(param[1]);

    var langPanel = document.getElementById("editor_popupdatalangpanel");
    var langList = document.getElementById("editor_popupdatalangoption");
    var optionList = document.getElementById("editor_popupdatadisplayoption");
    optionList.disabled = checked ? "" : "disabled";
    optionList.selectedIndex = idxDisplayMask <= 0 ? 0 : (idxDisplayMask - 1);
    langPanel.style.display = checked && idxDisplayMask > 1 ? "block" : "none";
    langList.selectedIndex = parseInt(param[2]);

}
///summary
///Charge les paramètres de l'option checkbox dans les controles HTML correspondants
///<param name="value">valeur saisie</param>
///<param name="checked">cochée</param>
///summary
function LoadFieldCheckBoxOption(value, checked) {
    var checkBox = document.getElementById("editor_bitlabel");
    var optionList = document.getElementById("editor_bitlabeldisplayoption");

    if (checked)
        optionList.selectedIndex = parseInt(value.split(",")[1]);

    ManageCheckBox(checkBox);
}
///summary
///Charge les paramètres de l'option truncate  dans les controles HTML correspondants
///<param name="value">valeur saisie</param>
///<param name="checked">cochée</param>
///summary
function LoadFieldTruncateOption(value, checked) {
    var checkBox = document.getElementById("editor_truncate");
    var charField = document.getElementById("editor_truncatechar");

    charField.disabled = checked ? "" : "disabled";

    if (checked)
        charField.value = value.split(",")[1];
    else
        charField.value = "";

    ManageCheckBox(checkBox);
}
///summary
/// Gère l'affichage de l'input de saisie de la procédure à exécuter en fin d'export
///<param name="checkbox">controle html de case a cocherXRM</param>
///summary
function ManageEndProcInputDisplay(checkbox) {
    var checked = checkbox.getAttribute("chk") == "1";
    var endProcInput = document.getElementById("editor_endprocedure");
    var endProcBloc = document.getElementById("editor_endprocedurebloc");

    if (checked) {
        endProcBloc.style.display = "inline";
    }
    else {
        endProcBloc.style.display = "none";
        endProcInput.value = "";
    }
}

///summary
///Permission utilisateur
///<param name="itemPrefix">Préfixe de nommage des champs</param>
///<param name="permissionType">Type de permission(view, delete, update)</param>
///Summary
var modalUserCat;
function showPermissionCat(itemPrefix, permissionType) {

    var oTarget = document.getElementById(itemPrefix + "_" + permissionType + "perminput");
    var checkBoxField = document.getElementById(itemPrefix + "_" + "chk" + permissionType + "permuser");
    //Champ value
    if (oTarget == null || checkBoxField == null || checkBoxField.getAttribute("chk") != "1")
        return;
    var sMulti = "1";
    var defValue = oTarget.getAttribute("ednvalue");

    modalUserCat = new eModalDialog("utilisateurs", 0, "eCatalogDialogUser.aspx", 550, (sMulti == "1") ? 610 : 550);
    top.eTabCatUserModalObject.Add(modalUserCat.iframeId, modalUserCat);
    modalUserCat.addParam("iframeId", modalUserCat.iframeId, "post");
    modalUserCat.ErrorCallBack = function () { setWait(false); }
    modalUserCat.addParam("multi", sMulti, "post");
    modalUserCat.addParam("selected", defValue, "post");
    modalUserCat.addParam("modalvarname", "modalUserCat", "post");
    modalUserCat.addParam("showvalueempty", "1", "post"); //si à 1 => Affiche <Vide> sur le catalogue unique
    modalUserCat.addParam("showvaluepublicrecord", "0", "post"); //si à 1 => Affiche <Fiche Publique> sur le catalogue unique
    modalUserCat.show();
    modalUserCat.addButton(top._res_29, OnPermissionCancel, "button-gray", null, "cancel", true);
    modalUserCat.addButton(top._res_28, OnPermissionValidate, "button-green", oTarget.id, "ok");
}
function OnPermissionCancel() {
    modalUserCat.hide();
}

///summary
///Affecte les valeurs sélectionnées dans le catalogue utilisateur au champ utilisateur des permissions
///<param name="fieldId">identifiant du champ input à affecter</param>
///summary
function OnPermissionValidate(fieldId) {
    var strReturned = modalUserCat.getIframe().GetReturnValue();
    modalUserCat.hide();
    var vals = strReturned.split('$|$')[0];
    var libs = strReturned.split('$|$')[1];
    var oTarget = document.getElementById(fieldId);

    oTarget.value = libs;
    oTarget.setAttribute("ednvalue", vals);

    if (fieldId.indexOf("view") > 0)
        oReport.SetViewPermParam("user", vals);

    if (fieldId.indexOf("update") > 0)
        oReport.SetUpdatePermParam("user", vals);
}


///summary
///Gère le masquage/affichage du bloc d'options détaillée au clic sur la permission
///<param name="checkBox">Option en cours</param>
///<param name="itemPrefix">Préfixe de nommage des champs</param>
///<param name="permissionType">Type de permission(view, delete, update)</param>
///summary
function PermissionClick(itemPrefix, permissionType) {
    //var checkBox = document.getElementById("chk_Opt" + permissionType + "Filter")
    var checked = checkBox.getAttribute("chk") == "1";
    var levelperminfo = document.getElementById(itemPrefix + "_" + permissionType + "lvlinfo");
    var userperminfo = document.getElementById(itemPrefix + "_" + permissionType + "userinfo");
    var levelCheckBox = document.getElementById(itemPrefix + "_chk" + permissionType + "permlvl");
    var levelList = document.getElementById(itemPrefix + "_" + permissionType + "permlvl");
    var userCheckBox = document.getElementById(itemPrefix + "_chk" + permissionType + "permuser");
    var userInput = document.getElementById(itemPrefix + "_" + permissionType + "perminput");

    //option active, on affiche les éléments relatifs
    if (!checked) {
        if (userCheckBox.getAttribute("chk") == "1") {
            chgChk(userCheckBox);
            userInput.setAttribute("ednvalue", "");
            userInput.value = "";
        }
        if (levelCheckBox.getAttribute("chk") == "1") {
            chgChk(levelCheckBox);
            levelList.selectedIndex = 0;
            levelList.disabled = "disabled";
        }
        levelperminfo.style.display = "none";
        userperminfo.style.display = "none";
    }
    else {
        levelperminfo.style.display = "";
        userperminfo.style.display = "";
    }
    switch (permissionType) {
        case "view":
            oReport.SetViewPermParam("id", "0");
            oReport.SetViewPermParam("mode", "0");
            oReport.SetViewPermParam("level", "0");
            oReport.SetViewPermParam("user", "");
            break;
        case "update":
            oReport.SetUpdatePermParam("id", "0");
            oReport.SetUpdatePermParam("mode", "0");
            oReport.SetUpdatePermParam("level", "0");
            oReport.SetUpdatePermParam("user", "");
            break;
    }

}

function PermissionClick() {

}

///summary
///Affecte le niveau sélectionné dans l'objet javascript rapport.
///<param name="dropDownList">Liste déroulante des niveaux</param>
///summary
function SetLevel(dropDownList) {
    //visualisation
    if (dropDownList.id.indexOf("view") > 0) {
        oReport.SetViewPermParam("level", dropDownList.options[dropDownList.selectedIndex].value);
    }
    if (dropDownList.id.indexOf("update") > 0) {
        oReport.SetUpdatePermParam("level", dropDownList.options[dropDownList.selectedIndex].value);
    }
}

///summary
///Gère le clic/déclic sur l'option niveau
///<param name="checkBox">Option en cours</param>
///<param name="itemPrefix">Préfixe de nommage des champs</param>
///<param name="permissionType">Type de permission(view, delete, update)</param>
///summary
function PermissionLevelClick(checkBox, itemPrefix, permissionType) {
    var ddlLevelList = document.getElementById(itemPrefix + "_" + permissionType + "permlvl");
    var checked = checkBox.getAttribute("chk") == "1";

    ddlLevelList.disabled = checked ? "" : "disabled";
    if (!checked) {
        ddlLevelList.options.selectedIndex = 0;
        ddlLevelList.setAttribute("ednvalue", "");

    } else {
        SetLevel(ddlLevelList);
    }
}

///summary
///Gère le clic/déclic sur l'option utilisateurs
///<param name="checkBox">Option en cours</param>
///<param name="itemPrefix">Préfixe de nommage des champs</param>
///<param name="permissionType">Type de permission(view, delete, update)</param>
///summary
function PermissionUserClick(checkBox, itemPrefix, permissionType) {
    var userInputField = document.getElementById(itemPrefix + "_" + permissionType + "perminput");
    var checked = checkBox.getAttribute("chk") == "1";

    if (!checked) {
        userInputField.value = "";
        userInputField.setAttribute("ednvalue", "");

        switch (permissionType) {
            case "view":
                oReport.SetViewPermParam("user", "");
                break;
            case "update":
                oReport.SetUpdatePermParam("user", "");
                break;
        }

    }
}


var modalLinkFile;

function ShowLinkFileMenu(tab) {



    modalLinkFile = new eModalDialog(top._res_982, 0, "eWizard.aspx", 400, 175, "LinkFileToReport");
    modalLinkFile.EudoType = ModalEudoType.WIZARD.toString(); // Type Wizard

    modalLinkFile.addParam("tab", oReport.GetTab(), "post");
    modalLinkFile.addParam("wizardtype", "linkfile", "post");
    modalLinkFile.ErrorCallBack = launchInContext(modalLinkFile, modalLinkFile.hide);
    modalLinkFile.show();

    var myFct = function () {
        modalLinkFile.hide();

        var list = document.getElementById("editor_filelist");
        if (list) {
            list.options.selectedIndex = 0;
            DisplayFieldList(list);
        }
    }

    modalLinkFile.addButton("Annuler", myFct, "button-gray", "", null, true); // Annuler
    modalLinkFile.addButton("valider", validLinkFile, "button-green", "", null, false); // Valider   
}

function onChangeLinkFileTab(lst) {
    if (lst.id != "editor_linkedfromlist")
        return;

    var tab = lst.options[lst.selectedIndex].value;
    var upd = new eUpdater("eWizard.aspx", 1);
    upd.addParam("tab", tab, "post");
    upd.addParam("wizardtype", "reloadlinkedfile", "post");
    upd.ErrorCallBack = function () { };
    upd.send(OnChangeLinkFileTreatment, null);
}

function OnChangeLinkFileTreatment(oRes) {
    var list = document.getElementById("editor_filelist");
    list.innerHTML = oRes;
}

function validLinkFile() {

    //var iframe = modalLinkFile.getIframe();
    //iframe.getReturnValueLinked();
    getReturnValueLinked();
    //fermer le popup
    modalLinkFile.hide();
}

//TOADAPT
function getReturnValueLinked() {
    var oDoc = modalLinkFile.getIframe().document;
    var lstLinkedFrom = oDoc.getElementById("editor_linkedfromlist");
    var lstLinked = oDoc.getElementById("editor_linkedfilelist");
    var descIdFrom = lstLinkedFrom.options[lstLinkedFrom.selectedIndex].value
    var descIdLinked = lstLinked.options[lstLinked.selectedIndex].value;

    //TODO listes nulles

    var tab = lstLinked.options[lstLinked.selectedIndex].value;
    var fromTab = lstLinkedFrom.options[lstLinkedFrom.selectedIndex].value;
    var upd = new eUpdater("eWizard.aspx", 1);
    upd.asyncFlag = false;
    upd.addParam("tab", tab, "post");
    upd.addParam("fromtab", fromTab, "post");
    upd.addParam("wizardtype", "linkedfields", "post");
    upd.ErrorCallBack = function () { };

    upd.send(GetLinkedFieldRender);
}
///summary
///Récupère le div de sélection et l'insère dans le HTML de l'éditeur
///Inséère également une nouvelle entre dans la dropdowlist des fichiers disponibles
///<param name="ofieldDiv">Element div field_list comprenant la liste des champs du fichier lié</param>
///summary
function GetLinkedFieldRender(ofieldDiv) {
    var wizardDocument = document;

    //Chargement du contenu dans un div, pour pouvoir utiliserle DOM afin de récupérer les informations nécessaires.

    var divResult = wizardDocument.createElement("div");
    divResult.innerHTML = ofieldDiv;
    var divErrorList = divResult.getElementsByTagName("error");
    if (divErrorList.length > 0) {
        top.showWarning(divErrorList[0].innerHTML);
    }
    else {
        try {
            var fieldList = new Array();
            var tmpDivList = divResult.getElementsByTagName("span");
            for (var i = 0; i < tmpDivList.length; i++) {
                if (tmpDivList[i].getAttribute("field_list") != null)
                    fieldList.push(tmpDivList[i]);
            }
            // TOCHECK: On laisse remonter une exception si fieldBloc est null ?
            var fieldBloc = fieldList[0];
            var wizardFieldSourceList = wizardDocument.getElementById("editor_sourcelist");
            var wizardFileList = wizardDocument.getElementById("editor_filelist");

            //Création d'une option pour ce fichier et ajout de cette option dans la liste des fichier de l'éditeur
            //affectation de la liste des champs dans le menu de sélection
            // ex : Affaire: descid 100, offre 1900 :
            //"editor_field_100_1900" : Offre depuis Affaire

            var fieldBlocId = fieldBloc.id.split("_");
            var fileOption = wizardDocument.createElement("option");
            fileOption.text = decode(fieldBloc.getAttribute("ednlabel"));
            fileOption.value = fieldBlocId[2] + "_" + fieldBlocId[3];
            fileOption.selected = "selected";
            wizardFileList.options.add(fileOption);
            wizardFieldSourceList.appendChild(fieldBloc);
        }
        catch (exception) {
            eAlert(0, top._res_6586, top._res_6587, exception.Description, wizardAlertWidth, wizardAlertHeight, null);
        }
    }
}

///summary
///Gère la validation des éléments du rapport en fonction de son mode (impression, export, publipostage etc...)
///summary
function ManageReportValidation() {

    switch (parseInt(oReport.GetType())) {
        //Impressions 
        case 0:
            return ManagePrintValidation();
            break;
        case 1:
            return true;
            break;
        case 2:
            //Exports
            return ManageExportValidation();
            break;
        case 3:
            // Publipostage
            // MCR: 39939: gere la validation de l'Assistant de publipostage, verifier que le champ 'enregistrer modele d export' est saisi
            return ManageMailingValidation();
            break;
        case 4:
            return true;
            break;
        case 5:
            return true;
            break;
        case 6:
            return ManageChartValidation();
            break;
        default:
            return true;
            break;
    }
}



///summary
///Gère la validation des éléments de publipostage (vous devez nommer votre rapport)
///   MCR: 39939: manager l'Assistant de publipostage pour la validation, verifier que le champ nom du rapport est saisi
///summary
function ManageMailingValidation() {
    var errorReport = "";

    //Controles d'erreurs génériques et suppression des espaces a gauche/ a droite
    if (oReport.GetParam("saveas").trim() == "") {
        errorReport = errorReport += "- " + top._res_6378 + "\r\n";                    //vous devez nommer votre rapport
    }


    //pas d'erreurs on valide la fonction
    if (errorReport == "")
        return true;
    else {
        eAlert("0", top._res_6583, top._res_6585, errorReport);
        return false;
    }
    return false;
}



///summary
///Gère la validation des rendus fusioncharts
///summary
function ManageChartValidation() {

    //vérification cohérence 'type' de série avec le modèle de chart
    // en cas de contradiction modèle série simple/ série en série multiple, le modèle est prioritaire (c'est lui qui pilote le reste)

    try {

        var sT = oReport.GetParam("seriestype"); // type de série (simple/multiple par valeur ou regroupement)
        var sC = oReport.GetParam("typechart"); // modèle serie simple / multi série / empilé
        var nSelChartType = sC.split("|")[0];
        if (nSelChartType + "" == "1" && sT + "" != "0") {
            // pour les modèles de graphique en série simple, le type de série est tjs 0
            oReport.SetParam("seriestype", "0");
        }

    }
    catch (e) {

    }

    if (oReport.GetParam("saveas").length == 0) {
        eAlert(0, top._res_6377, top._res_6378); //vous devez nommer votre rapport
        return false;
    }

    return true;
}
///summary
///Gère la validation des éléments d'impression
///summary
function ManagePrintValidation() {

    if (oReport.GetParam("saveas").length == 0) {
        eAlert(0, top._res_6377, top._res_6378); //vous devez nommer votre rapport
        return false;
    }
    return true;
}

///summary
///Gère la validation des éléments d'export
///summary
function ManageExportValidation() {
    var errorReport = "";

    //Controles d'erreurs génériques
    if (oReport.GetParam("saveas") == "") {
        errorReport = errorReport += "- " + top._res_6378 + "\r\n";
    }

    //Si le rapport n'est pas au format texte, on réinitialise les paramètre avancé de rapport texte.
    if (parseInt(oReport.GetParam("format")) != 1) {
        oReport.SetParam("sep", "");
        oReport.SetParam("side", "");
    }
    //Si le rapport n'est pas au format excel ou OpenOffice, on réinitialise les paramètres d'export dans un modèle.
    if (parseInt(oReport.GetParam("format")) != 2 && parseInt(oReport.GetParam("format")) != 5) {
        oReport.SetParam("usetemplate", "");
        oReport.SetParam("template", "");
    }

    // #63 665 - Pour le format Power BI, la sélection de l'utilisateur exécutant est obligatoire
    // Cette vérification ne devrait toutefois jamais se produire, car le champ est prérempli avec l'utilisateur connecté par défaut, et la fenêtre de saisie oblige à en sélectionner un
    if (parseInt(oReport.GetParam("format")) == 7 && (parseInt(oReport.GetParam("powerbiuser")) < 1 || isNaN(parseInt(oReport.GetParam("powerbiuser"))))) {
        eAlert(1, top._res_372, top._res_373.replace('<ITEM>', document.querySelector('.editor_powerbi_user_label').firstChild.innerText));
        return false;
    }

    //pas d'erreurs on valide la fonction
    if (errorReport == "")
        return true;
    else {
        eAlert("0", top._res_6583, top._res_6585, errorReport);
        return false;
    }
    return false;
}

//sauvegarde les anciens valeurs
function DisplayChartsList(obj) {
    var selectArray = [
        top._CombinedY + 'EtiquettesFile',
        top._CombinedY + 'ValuesFile',

        top._CombinedY + 'ddlfilter',
        top._CombinedY + 'EtiquettesGroup',
        top._CombinedZ + 'EtiquettesFile',
        top._CombinedZ + 'ValuesFile',

        top._CombinedZ + 'ddlfilter',
        top._CombinedZ + 'EtiquettesGroup'
    ];
    if (!isNaN(parseInt(obj.value)))
        oReport.SetParam("seriestype", (parseInt(obj.value) - 1).toString());

    if (parseInt(obj.value) == 4) {
        for (var i = 0; i < selectArray.length; i++) {
            var element = document.getElementById(selectArray[i]);
            oReport.SetParam(selectArray[i].toLowerCase(), element.options[element.selectedIndex].value);
        }
        document.getElementById(top._CombinedY + 'ValuesOperation').onchange();
        document.getElementById(top._CombinedZ + 'ValuesOperation').onchange();
        //document.getElementById(top._CombinedY + 'EtiquettesFile').onchange();
        //document.getElementById(top._CombinedY + 'ValuesFile').onchange();
        //document.getElementById(top._CombinedZ + 'EtiquettesFile').onchange();
        //document.getElementById(top._CombinedZ + 'ValuesFile').onchange();
    }

    if (parseInt(obj.value) == 2 || parseInt(obj.value) == 3) {
        document.getElementById("rdbtn_groupby").checked = true;
        document.getElementById("ddlfilter").selectedIndex = 0;
    }

    if (parseInt(obj.value) == 1) {
        document.getElementById("ddlfilter").selectedIndex = 0;
    }


    if (oReport.GetId() > 0)
        return;
    //BSE : 54 285 remplacer la DropDownList par des radios buttons
    var liChart = document.getElementById("liChrts_" + obj.value);

    var divsChart = document.querySelectorAll("li[id*='liChrts_']");

    for (var i = 0; i < divsChart.length; i++) {
        var id = divsChart[i].id;
        if (id == liChart.id)
            divsChart[i].style.display = "";
        else
            divsChart[i].style.display = "none";
    }


    // le tri n'existe que pour les séries simples
    var chkSort = document.getElementById("Sort");
    if (obj.value == "1")
        chkSort.style.display = "";
    else
        chkSort.style.display = "none";


    // on désactive le chart précédemment sélectionné
    var oldSelChart = oReport.GetParam("typechart");
    var oldSelChartImg = document.getElementById("chrt_" + oldSelChart.replace("|", "_"));
    removeClass(oldSelChartImg, "graphCadreSel");

    if (oParamsMem["typechart"].split("|")[0] == obj.value)
        LoadFromSavedParams();
    else {


        oReport.SetParam("typechart", obj.value + "|1");

        //Si graphuqye multi series ou serie empilés on active la propritèe de regroupement par defaut
        if (obj.value == "2" || obj.value == "3") {

            var radio = document.getElementById("rdbtn_groupby");
            //On click sur la radio (fr inter)
            fireEvent(radio, 'click');
            setAttributeValue(radio, "checked", "true");
        }
    }
    //on active le nouveau chart
    var newSelChart = oReport.GetParam("typechart");
    var newSelChartImg = document.getElementById("chrt_" + newSelChart.replace("|", "_"));

    //On click sur le premier graphique
    fireEvent(newSelChartImg, 'click');

}

function LoadFromSavedParams() {


    //hide de la rubrique etiquette X
    HideOldDropList("EtiquettesFile", "EtiquettesField");

    //hide de la rubrique de regroupement des series X
    HideOldDropList("SeriesFile", "SeriesField");

    //Hide de  la rubrique Y
    HideOldDropList("ValuesFile", "ValuesField");

    var oldCheckFilter = oReport.GetParam("addcurrentfilter");

    //On charge les params dans l objet oReport
    for (key in oParamsMem)
        oReport.SetParam(key, oParamsMem[key]);

    //choix des séries
    document.getElementById("rdbtn_groupby").checked = oReport.GetParam("seriestype") == "1" ? true : false;
    document.getElementById("rdbtn_field").checked = oReport.GetParam("seriestype") == "2" ? true : false;

    //chargement de la rubrique etiquette X
    loadIntoDropList("EtiquettesFile", "EtiquettesField", "EtiquettesGroup");

    //chargement de la rubrique de regroupement des series X
    loadIntoDropList("SeriesFile", "SeriesField", "");

    //chargement de  la rubrique Y
    loadIntoDropList("ValuesFile", "ValuesField", "ValuesOperation");
    UpdateAddedRubriquesY(); //pour les multi serie avec plusieurs champs
    UpdateFirstRubriqueY();


    //Chargement du filter.
    document.getElementById("ddlfilter").value = oReport.GetParam("filterid");
    var CheckFilter = document.getElementById("addCurrentFilter");

    if (oldCheckFilter != oReport.GetParam("addcurrentfilter")) {
        CheckFilter.attributes["chk"] = oReport.GetParam("addcurrentfilter") == "1" ? true : false;
        chgChk(CheckFilter);
    }
}
///
///On selectionne les ddl a partir des parametre sauvegardés
///
function loadIntoDropList(file, field, op) {

    var ddlFile = document.getElementById(file);
    ddlFile.value = oReport.GetParam(file.toLowerCase());

    var ddlField = document.getElementById(field + "_" + ddlFile.options[ddlFile.selectedIndex].value);
    ddlField.value = oReport.GetParam(field.toLowerCase());

    SetDisplay(true, new Array(ddlField.id));

    if (op.length > 0) {
        var ddlOp = document.getElementById(op);
        ddlOp.value = oReport.GetParam(op.toLowerCase());
        if (op != "ValuesOperation") {

            DisplEtiqGroup(ddlField);

        }
    }
}
function HideOldDropList(file, field) {

    var ddlFile = document.getElementById(file);
    var ddlField = document.getElementById(field + "_" + ddlFile.options[ddlFile.selectedIndex].value);
    ddlFile.value = oReport.GetParam(file.toLowerCase());
    ddlField.value = oReport.GetParam(field.toLowerCase());

    SetDisplay(false, new Array(ddlField.id));
}

function selectChart(e) {

    // Récupération de l'évènement
    if (!e)
        var e = window.event;

    // Objet source
    var oSourceObj = e.target || e.srcElement;

    if (oSourceObj.tagName != "TD") {
        return;
    }

    ActiveChart(oSourceObj);
}

function ActiveChart(oSourceObj) {
    var echart = getAttributeValue(oSourceObj, "echrt");
    if (typeof (echart) == 'undefined' || echart == null || echart.indexOf("|") < 0)
        return;

    // on désactive la chart précédemment sélectionné
    var oldSelChart = oReport.GetParam("typechart");
    var oldSelChartImg = document.getElementById("chrt_" + oldSelChart.replace("|", "_"));


    //Récupère type de serie et type de graphique
    var modelChartType = getAttributeValue(oSourceObj, "type");
    var modelChart3DType = (getAttributeValue(oSourceObj, "is3D") == '1');
    var modelChartSerie = echart.split("|")[0];

    //BSE: Afficher que les rubriques type Catalogue et utilisateur pour les graphiques Funnel et Pyramid
    specialTraitementForFunnelAndPyramid(modelChartType, oldSelChartImg);

    //Si le type de la série est spéciale, on applique un traitement spécifique au graphique type jauge
    if (modelChartSerie == SPECIAL_CHART_TYPE) {
        specialTraitementForGaugeChart(modelChartType);
    }


    removeClass(oldSelChartImg, "graphCadreSel");

    // on active celui surlequel on vient de cliquer
    oReport.SetParam("typechart", echart);
    addClass(oSourceObj, "graphCadreSel");
    // TO-DO en fonction de type de graphique on affiche l apercu correspondant 
    // var src = getAttributeValue(oSourceObj, "src");

    //Indique si on est sur un graphique type jauge circulaire
    oReport.bCircularGauge = (echart == '4|2');

    //Si le modèle est: PIE ou DOUGHNUT alors les symbôles (X) et (Y) seront masqués. 
    var liLabelY = document.getElementById("rbqValsLabelY");
    var liLabelX = document.getElementById("rbqValsLabelX");

    if (modelChartType == CHART_PIE || modelChartType == CHART_DOUNHNUT || modelChartType == CHART_SEMI_PIE || modelChartType == CHART_SEMI_DOUNHNUT) {
        liLabelY.innerHTML = liLabelY.innerHTML.replace(" (Y):", " :");
        liLabelX.innerHTML = liLabelX.innerHTML.replace(" (X):", " :");
    } else {
        liLabelY.innerHTML = liLabelY.innerHTML.replace(" :", " (Y):");
        liLabelX.innerHTML = liLabelX.innerHTML.replace(" :", " (X):");
    }

    //Si le modèle est: PIE ou DOUGHNUT ou Series Empilés en bar/Baton, alors on affiche la case à cocher valeurs en pourcentage.
    var chkDispValPct = document.getElementById("DispValPct");
    if (modelChartType == CHART_PIE
        || modelChartType == CHART_DOUNHNUT || modelChartType == CHART_PYRAMID || modelChartType == CHART_FUNNEL
        || modelChartType == CHART_SEMI_PIE || modelChartType == CHART_SEMI_DOUNHNUT
        || (modelChartSerie == EMPILE_CHART_TYPE && (modelChartType == CHART_HISTO || modelChartType == CHART_BATON))
    ) {
        chkDispValPct.style.display = "";
    } else {
        setAttributeValue(chkDispValPct, "chk", "1");
        chkDispValPct.style.display = "none";
        oReport.SetParam('displayvaluespercent', '0');
        chgChk(chkDispValPct);
    }


    //Si le modèle est Series Empilés en bar/Baton, alors on affiche la case à cocher Échelle en pourcentage. 
    var chkDispStckPct = document.getElementById("DispStckPct");
    if (modelChartSerie == EMPILE_CHART_TYPE && (modelChartType == CHART_HISTO || modelChartType == CHART_BATON)) {
        chkDispStckPct.style.display = "";

    } else {
        setAttributeValue(chkDispStckPct, "chk", "1");
        chkDispStckPct.style.display = "none";
        oReport.SetParam('displaystackedpercent', '0');
        chgChk(chkDispStckPct);
    }

    //Sur les séries simples histogramme et baton (2D et 3D)uniquement :"Utiliser la couleur du thème"
    var chkUseThemeColor = document.getElementById("UseThemeColor");
    var liUseThemeColor = document.getElementById("LiUseThemeColor");
    if ((modelChartSerie == SINGLE_CHART_TYPE && (modelChartType == CHART_HISTO || modelChartType == CHART_BATON)) || (modelChartSerie == SPECIAL_CHART_TYPE && modelChartType == CHART_COMBINED)) {
        liUseThemeColor.style.display = "";

    } else {
        setAttributeValue(chkUseThemeColor, "chk", "1");
        liUseThemeColor.style.display = "none";
        oReport.SetParam('useThemeColor', '0');
        chgChk(chkUseThemeColor);
    }


    //Sur les histogrammes (2D et 3D) Area et Lineaire uniquement: on peut modifier le choix pour les étiquettes 
    var lisDisplayEtiquette = document.getElementById("displayEtiquette");
    var lstdisplayx = document.getElementById("lstdisplayx");
    if (modelChartType == CHART_COMBINED || modelChartType == CHART_HISTO || modelChartType == CHART_AREA || modelChartType == CHART_LINE || modelChartType == CHART_SPLINE) {
        lisDisplayEtiquette.style.display = "block";
        if (modelChart3DType == false) {
            lstdisplayx.options[0].style.display = "block";
            lstdisplayx.options[2].style.display = "block";
        } else {
            lstdisplayx.options[0].style.display = "none";
            lstdisplayx.options[2].style.display = "none";
            lstdisplayx.selectedIndex = 1;
        }
        oReport.SetParam('displayx', '1');
        oReport.SetParam('lstdisplayx', '1');
    } else if (modelChartType == CHART_BATON) {
        lisDisplayEtiquette.style.display = "block";
        lstdisplayx.options[0].style.display = "none";
        lstdisplayx.options[2].style.display = "none";
        lstdisplayx.selectedIndex = 1;
        oReport.SetParam('lstdisplayx', '1');
        oReport.SetParam('displayx', '1');
    }
    else {
        lisDisplayEtiquette.style.display = "none";
        oReport.SetParam('lstdisplayx', '1');
        oReport.SetParam('displayx', '0');
    }


    //BSE:#61 234
    //Sur les séries simples seulement: "Afficher la grille des valeurs"
    var chkDisplayGrid = document.getElementById("DispGrid");
    var liUseGrid = document.getElementById("chartDisplayGrid");
    if (modelChartSerie == SINGLE_CHART_TYPE || (modelChartSerie == SPECIAL_CHART_TYPE && modelChartType == CHART_COMBINED)) {
        liUseGrid.style.display = "";

    } else {
        setAttributeValue(chkDisplayGrid, "chk", "1");
        liUseGrid.style.display = "none";
        oReport.SetParam('DispGrid', '0');
        chgChk(chkDisplayGrid);
    }

    //Sur les séries simples seulement: "Afficher le tri sur les valeurs"
    var chkDisplayGrid = document.getElementById("SortBox");
    var liUseGrid = document.getElementById("Sort");
    var sortOrder = document.getElementById('SortOrder');
    if (modelChartSerie == SINGLE_CHART_TYPE && modelChartType != CHART_PYRAMID && modelChartType != CHART_FUNNEL) {
        liUseGrid.style.display = "";
    } else {
        setAttributeValue(chkDisplayGrid, "chk", "1");
        liUseGrid.style.display = "none";
        oReport.SetParam('sortenabled', '0');
        oReport.SetParam('sortOrder', '');
        sortOrder.selectedIndex = 0;
        chgChk(chkDisplayGrid);
    }

    //BSE:#64 664 / Ajuster l'echelle des 2 graphiques pour le graphique combiné seulement
    var chkDisplayZaxe = document.getElementById("DisplayZaxe");
    var liDisplayZaxe = document.getElementById("chartDisplayZaxe");

    if (modelChartSerie == SPECIAL_CHART_TYPE && modelChartType == CHART_COMBINED) {
        liDisplayZaxe.style.display = "";
        setAttributeValue(chkDisplayZaxe, "chk", "1");
        oReport.SetParam('displayzaxe', '1');
        chgChk(chkDisplayZaxe, true);

    } else {
        setAttributeValue(chkDisplayZaxe, "chk", "1");
        liDisplayZaxe.style.display = "none";
        oReport.SetParam('displayzaxe', '0');
        chgChk(chkDisplayZaxe);
    }



    if (modelChartSerie != SPECIAL_CHART_TYPE || modelChartType == CHART_COMBINED) {
        //Afficher la position de la légende
        var chkDisplayLegend = document.getElementById("displayLegend");
        setAttributeValue(chkDisplayLegend, "chk", "1");
        chkDisplayLegend.parentElement.style.display = "";
        oReport.SetParam('displaylegend', '1');
        chgChk(chkDisplayLegend, true);
    }

    SetDynGraph(modelChartType);
    UpdatePanelSelectFields(true);
    UpdateUlWrappeurDisplay(modelChartSerie, modelChartType)
}

function SetDynGraph(modelChart) {


    //Si le type de chart est simple on exit
    var sSelChart = oReport.GetParam("typechart");
    var nSelChartType = parseInt(sSelChart.split("|")[0]);

    var dynGraphImage = document.getElementById("dynGraphImageDiv");
    var dynNameY = document.getElementById("DynNameDiv_Y");
    var dynNameX = document.getElementById("DynNameDiv_X");

    var dynGraphImageFormat = document.getElementById("dynGraphImageFormat");
    var dynNameYBis = document.getElementById("DynNameDivBis_Y");
    var dynNameXBis = document.getElementById("DynNameDivBis_X");


    SetDisplay(true, new Array("DynNameDiv_X", "DynNameDiv_Y"));
    SetDisplay(true, new Array("DynNameDivBis_X", "DynNameDivBis_Y"));


    switch (modelChart) {
        case CHART_PIE:
            dynGraphImage.className = "dynGraph cam";
            dynNameX.className = "valueItem camX"
            dynNameY.className = "nameItem camY";
            break;
        case CHART_SEMI_PIE:
            dynGraphImage.className = "dynGraph semiCam";
            dynNameX.className = "valueItem camX"
            dynNameY.className = "nameItem camY";
            break;
        case CHART_DOUNHNUT:
            dynGraphImage.className = "dynGraph ring";
            dynNameX.className = "valueItem ringX"
            dynNameY.className = "nameItem ringY";
            break;
        case CHART_SEMI_DOUNHNUT:
            dynGraphImage.className = "dynGraph semiAnn";
            dynNameX.className = "valueItem ringX"
            dynNameY.className = "nameItem ringY";
            break;
        case CHART_FUNNEL:
            dynGraphImage.className = "dynGraph fun";
            dynNameX.className = "valueItem ringX"
            dynNameY.className = "nameItem ringY";
            break;
        case CHART_PYRAMID:
            dynGraphImage.className = "dynGraph pyr";
            dynNameX.className = "valueItem ringX"
            dynNameY.className = "nameItem ringY";
            break;
        case CHART_SPLINE:
            dynGraphImage.className = "dynGraph spline";
            dynNameX.className = "valueItem courbeX"
            dynNameY.className = "nameItem courbeY";
            break;

        case CHART_HISTO:
            if (nSelChartType == SINGLE_CHART_TYPE) {


                dynGraphImage.className = "dynGraph histo";
                dynNameX.className = "valueItem histoX"
                dynNameY.className = "nameItem histoY";

            } else {

                if (nSelChartType == MULTI_CHART_TYPE) {

                    dynGraphImage.className = "dynGraph histoMul";

                } else {

                    dynGraphImage.className = "dynGraph histoEmp";

                }
                SetDisplay(false, new Array("DynNameDiv_X", "DynNameDiv_Y"));
                SetDisplay(false, new Array("DynNameDivBis_X", "DynNameDivBis_Y"));
            }
            break;

        case CHART_BATON:
            if (nSelChartType == SINGLE_CHART_TYPE) {


                dynGraphImage.className = "dynGraph bar";
                dynNameX.className = "valueItem barX"
                dynNameY.className = "nameItem barY";

            } else {

                if (nSelChartType == MULTI_CHART_TYPE) {

                    dynGraphImage.className = "dynGraph barMul";

                } else {

                    dynGraphImage.className = "dynGraph barEmp";

                }
                SetDisplay(false, new Array("DynNameDiv_X", "DynNameDiv_Y"));
                SetDisplay(false, new Array("DynNameDivBis_X", "DynNameDivBis_Y"));
            }
            break;
        case CHART_LINE:
            if (nSelChartType == SINGLE_CHART_TYPE) {
                dynGraphImage.className = "dynGraph courbe";
                dynNameX.className = "valueItem courbeX"
                dynNameY.className = "nameItem courbeY";
            }
            else {
                dynGraphImage.className = "dynGraph courbeMulEmp";
                SetDisplay(false, new Array("DynNameDiv_X", "DynNameDiv_Y"));
                SetDisplay(false, new Array("DynNameDivBis_X", "DynNameDivBis_Y"));
            }
            break;

        case CHART_AREA:

            switch (nSelChartType) {
                case SINGLE_CHART_TYPE:
                    dynGraphImage.className = "dynGraph area";
                    break;
                case MULTI_CHART_TYPE:
                    dynGraphImage.className = "dynGraph multiArea";
                    break;
                case EMPILE_CHART_TYPE:
                    dynGraphImage.className = "dynGraph empArea";
                    break;
                default:
                    dynGraphImage.className = "dynGraph area";
                    break;

                    SetDisplay(false, new Array("DynNameDiv_X", "courbeX"));
                    SetDisplay(false, new Array("DynNameDivBis_X", "courbeY"));
            }
            break;
        case CHART_CIRCULARGAUGE:
            dynGraphImage.className = "dynGraph circularGauge";
            SetDisplay(false, new Array("DynNameDiv_X", "DynNameDiv_Y"));
            SetDisplay(false, new Array("DynNameDivBis_X", "DynNameDivBis_Y"));
            SetDisplay(false, new Array("DynNameDiv_X", "courbeX"));
            SetDisplay(false, new Array("DynNameDivBis_X", "courbeY"));
            break;
        case CHART_COMBINED:
            dynGraphImage.className = "dynGraph combined";
            break;
        default:
            break;

    }

    dynGraphImageFormat.setAttribute("class", dynGraphImage.getAttribute("class"));
    dynNameXBis.setAttribute("class", dynNameX.getAttribute("class"));
    dynNameYBis.setAttribute("class", dynNameY.getAttribute("class"));
    dynNameXBis.innerHTML = dynNameX.innerHTML;
    dynNameYBis.innerHTML = dynNameY.innerHTML;

}

///************************  MOU ************************
/// Fonction qui rafraichit le panel de l'ecran 2 de l assitant graphique et met a jour les params
///***********************************************
function UpdatePanelSelectFields(forcingSelectedIndex, prefix) {

    var bCombinedZ = false;
    var bCombinedY = false;
    var etiquetteFileSelected;
    var etiquetteFieldSelected;

    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';


    if (prefix.indexOf('Combined') != -1 && prefix.indexOf('File') == -1 && prefix.indexOf('Field') == -1) {
        UpdateFirstRubriqueY(prefix);

    } else if (prefix == '') {
        if (oReport.GetParam("typechart").split('|')[0] == SPECIAL_CHART_TYPE && oReport.GetParam("typechart").split('|')[1] == '1') {
            bCombinedY = (document.getElementById(top._CombinedY + 'EtiquettesFile') != null);
            if (bCombinedY) {
                etiquetteFileSelected = document.getElementById(top._CombinedY + 'EtiquettesFile');
                prefix = top._CombinedY;
                DoUpdating(prefix);
            }

            bCombinedZ = (document.getElementById(top._CombinedZ + 'EtiquettesFile') != null);
            if (bCombinedZ) {
                prefix = top._CombinedZ;
                if (etiquetteFileSelected != null && typeof etiquetteFileSelected != 'undefined') {
                    etiquetteFieldSelected = document.getElementById(top._CombinedY + 'EtiquettesField_' + etiquetteFileSelected.value);
                    DoUpdating(prefix, etiquetteFieldSelected);
                }

                else
                    DoUpdating(prefix);
            }
        }

    }

    if (!bCombinedZ && !bCombinedY && prefix == '')
        DoUpdating();

}

function UpdateUlWrappeurDisplay(modelChartSerie, modelChartType) {

    var display = '';
    Array.prototype.slice.apply(document.querySelectorAll("li[ctype='gauge']")).forEach(
        function (elem, index) {
            switch (getAttributeValue(elem, 'gauge')) {
                case 'expressfilter':
                    elem.innerText = top._res_8521 + ' :';
                    break;
                case top._CombinedY.toLowerCase():
                    elem.innerText = top._res_8520.replace('{0}', top._res_1567);
                    break;
                case top._CombinedZ.toLowerCase():
                    elem.innerText = top._res_8520.replace('{0}', top._res_1569);
                    break;
                case 'filter':
                    elem.getElementsByClassName('libelleExpressFilterBold')[0].innerText = top._res_182 + ' ' + getAttributeValue(elem, 'index');
                    elem.getElementsByClassName('libelleExpressFilterBold')[1].innerText = top._res_8537.replace('{0}', getAttributeValue(elem, 'index'));
                    break;
                default:
                    elem.style.display = 'none';
                    break;
            }
        });

    if (modelChartSerie == SPECIAL_CHART_TYPE) {
        if (modelChartType == CHART_CIRCULARGAUGE) {
            display = 'none';
            if (document.getElementById(top._CombinedY + 'cumulerFilter') != null)
                document.getElementById(top._CombinedY + 'cumulerFilter').style.display = display;

            document.getElementById(top._CombinedZ + 'cumulerFilter').style.display = '';
            //#78 244 KJE: enlever le positionnement de la case à cocher "Cumuler avec le filtre en cours" par défaut à true
            //TODO: voir pourquoi ça été fait comme ça et s'il y a des effets de bord suite à cette modif
            //oReport.SetParam(top._CombinedY.toLowerCase() + 'addcurrentfilter', '0');
            //oReport.SetParam(top._CombinedZ.toLowerCase() + 'addcurrentfilter', '1');
            Array.prototype.slice.apply(document.getElementsByClassName('btnValue')).forEach(
                function (elem, index) {
                    if (index == 0)
                        elem.parentElement.style.display = '';
                    elem.style.display = '';
                });

            Array.prototype.slice.apply(document.querySelectorAll("li[cType='gauge']")).forEach(
                function (elem, index) {
                    switch (getAttributeValue(elem, 'gauge')) {
                        case 'expressfilter':
                            elem.innerText = top._res_8234 + ' :';
                            break;
                        case top._CombinedY.toLowerCase():
                            elem.innerText = top._res_1876;
                            break;
                        case top._CombinedZ.toLowerCase():
                            elem.innerText = top._res_1878;
                            break;
                        case 'filter':
                            elem.getElementsByClassName('libelleExpressFilterBold')[0].innerText = top._res_1899.replace('{0}', getAttributeValue(elem, 'index'));
                            elem.getElementsByClassName('libelleExpressFilterBold')[1].innerText = top._res_1898.replace('{0}', getAttributeValue(elem, 'index'));
                            break;
                        default:
                            elem.style.display = '';
                            break;
                    }
                });

        } else {
            if (document.getElementById(top._CombinedY + 'cumulerFilter') != null)
                document.getElementById(top._CombinedY + 'cumulerFilter').style.display = display;

            document.getElementById(top._CombinedZ + 'cumulerFilter').style.display = 'none';
            //#78 244 KJE: enlever le positionnement de la case à cocher "Cumuler avec le filtre en cours" par défaut à true
            //TODO: voir pourquoi ça été fait comme ça et s'il y a des effets de bord suite à cette modif
            //oReport.SetParam(top._CombinedY.toLowerCase() + 'addcurrentfilter', '1');
            //oReport.SetParam(top._CombinedZ.toLowerCase() + 'addcurrentfilter', '0');
            Array.prototype.slice.apply(document.getElementsByClassName('btnValue')).forEach(
                function (elem, index) {
                    if (index == 0)
                        elem.parentElement.style.display = 'none';
                    elem.style.display = 'none';
                });
        }


        Array.prototype.slice.apply(document.querySelectorAll("li[id*='ValsContentX']")).forEach(
            function (elem, index) {
                elem.style.display = display;
                if (elem.nextSibling.firstChild.innerText) {
                    elem.nextSibling.firstChild.style.display = display;

                }
            });

    }

    if (document.getElementById('combinedulWrappeur') != null)
        document.getElementById('combinedulWrappeur').style.display = (modelChartSerie == SPECIAL_CHART_TYPE ? 'block' : 'none');
    if (document.getElementById('ulWrappeur'))
        document.getElementById('ulWrappeur').style.display = (modelChartSerie != SPECIAL_CHART_TYPE ? 'block' : 'none');



}

function DoUpdating(prefix, etiquetteFieldSelected) {

    var updateFilter = false;
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string')
        prefix = '';


    //document.getElementById(prefix + 'EtiquettesFile').onchange();
    //document.getElementById(prefix + 'ValuesFile').onchange();

    if (typeof etiquetteFieldSelected != 'undefined' && etiquetteFieldSelected != null) {
        UpdateLineaireGraphiqueSelection(etiquetteFieldSelected, prefix);
        updateFilter = true;
    }

    //Mise à jour des liste de filtre si on est en graphique combiné
    if (prefix != '' && updateFilter) {
        var nModelChartType = oReport.GetParam("typechart").split("|")[0];
        if (nModelChartType == SPECIAL_CHART_TYPE && oReport.GetParam("typechart").split('|')[1] == '1')
            UpdatFilterList(document.getElementById(prefix + 'ValuesFile'), prefix);
    }


    UpdateAddedRubriquesY(); // met a jour les rubriques Y ajoutées sauf la première ligne
    UpdateDisplays(); //affiche des options en fonction des choix sélectionés
    UpdateDescription(prefix); // met a jour la description
    UpdateParams(prefix); // met a jour les params dans oReport
    UpdateFirstRubriqueY(prefix); //met a jour la premiere ligne des rubrique Y
}


function UpdateFirstRubriqueY(prefix) {
    if (typeof prefix == 'undefined' || prefix == null && typeof prefix != 'string') {
        prefix = '';
    }

    //Mise a jour de la premiere rubrique Y
    var ddlValuesFile = document.getElementById(prefix + "ValuesFile");
    var ddlValuesField = document.getElementById(prefix + "ValuesField_" + ddlValuesFile.options[ddlValuesFile.selectedIndex].value);
    var ddlValuesOperation = document.getElementById(prefix + "ValuesOperation");



    ddlValuesField.disabled = false;
    if ("count" == ddlValuesOperation.value.toLowerCase()) {
        ddlValuesField.disabled = true;

        if (ddlValuesField.selectedIndex == -1)
            ddlValuesOperation.disabled = true;
    }

    ddlValuesFile.disabled = true;

    var liMultiLines = document.getElementById("liMultiLines");

    if ((liMultiLines != null && typeof liMultiLines != 'undefined' && liMultiLines.children.length == 0) || prefix != '')
        ddlValuesFile.disabled = false;
}

function UpdateAddedRubriquesY() {


    //Si le type de chart est simple on exit
    var sSelChart = oReport.GetParam("typechart");
    var nSelChartType = sSelChart.split("|")[0];
    var bIsSingleChart = nSelChartType == SINGLE_CHART_TYPE ? true : false;



    //le type de chart est soit multi-serie ou empilé
    var tabFields = oReport.GetParam("valuesfield").split(";");
    var tabOperations = oReport.GetParam("valuesoperation").split(";");
    var bIsRegroupement = document.getElementById("rdbtn_groupby").checked;

    var liMultiLines = document.getElementById("liMultiLines");

    if (bIsRegroupement || bIsSingleChart) {

        if (liMultiLines != null && typeof liMultiLines != 'undefined' && liMultiLines.children.length > 0)
            //si on choisit par Regroupement, on supprime les lignes s'il y en a
            while (liMultiLines.children.length > 0) {

                DeleteRubriqueY(liMultiLines.children[0]);

            }

    } else {

        //on insert les lignes en fonction de nombre de valeur que contient valuesfield 
        var length = tabFields.length;
        if (liMultiLines.children.length == 0) {
            for (var index = 1; index < length; index++) {

                InsertRubriqueY(index);
            }
        }
    }
}

function UpdateDisplays() {
    var sSelChart = oReport.GetParam("typechart");
    var nSelChartType = sSelChart.split("|")[0];
    var nModelChartType = sSelChart.split("|")[1];
    var bIsSingleChart = nSelChartType == SINGLE_CHART_TYPE ? true : false;

    displayEtiquettesTri(nModelChartType, nSelChartType);
    displayFusionCombinedGraph(nSelChartType, nModelChartType);

    if (bIsSingleChart) {
        SetDisplay(false, new Array("choixSeriesLabel", "choixSeriesContent"));
        SetDisplay(false, new Array("rbqSeriesLabel", "rbqSeriesContent"));
        SetDisplay(false, new Array("rbqDescLabel", "rbqDescContent"));
        SetDisplay(false, new Array("liAddBtn"));
        SetDisplay(false, new Array("liMultiLines"));


    } else {

        // Chart de type multi-serie ou empilé
        // Affiche le choix des series
        SetDisplay(true, new Array("choixSeriesLabel", "choixSeriesContent"));


        var IsRegroupement = document.getElementById("rdbtn_groupby").checked;

        if (IsRegroupement) {

            // Affiche les champs et les labels qui correspondent au choix de série par regroupement
            SetDisplay(true, new Array("rbqSeriesLabel", "rbqSeriesContent")); //masque la rubrique des série
            SetDisplay(true, new Array("rbqDescLabel", "rbqDescContent")); //masque la description

            //Le bouton ajouter des nouvelles valeur Y ne s affiche pas
            SetDisplay(false, new Array("liAddBtn"));
            SetDisplay(false, new Array("liMultiLines"));

        } else {

            //Masque les champs et les labels qui correspondent pas au choix de série par champs 
            SetDisplay(false, new Array("rbqSeriesLabel", "rbqSeriesContent"));
            SetDisplay(false, new Array("rbqDescLabel", "rbqDescContent"));

            //Affiche Bouton ajouter pour ajouter des rubriques   
            SetDisplay(true, new Array("liAddBtn"));
            SetDisplay(true, new Array("liMultiLines"));
        }
    }
}

function UpdateDescription(prefix) {

    if (typeof prefix == 'undefined' || prefix == null || typeof prefix != 'string')
        prefix = '';

    var seriesFieldText = '';
    var serieFileText = '';
    var seriesField;

    var sSelChart = oReport.GetParam("typechart");
    var nSelChartType = sSelChart.split("|")[0];
    var bIsSingleChart = nSelChartType == SINGLE_CHART_TYPE ? true : false;


    //les ddl des files
    var valuesFile = document.getElementById(prefix + "ValuesFile");
    var etiquettesFile = document.getElementById(prefix + "EtiquettesFile");
    var seriesFile = document.getElementById(prefix + "SeriesFile");


    var valuesOperation = document.getElementById(prefix + "ValuesOperation");


    if (valuesOperation.selectedIndex == -1)
        valuesOperation.selectedIndex = 0;
    //Les ddl des fields
    var valuesField = document.getElementById(prefix + "ValuesField_" + valuesFile.options[valuesFile.selectedIndex].value);
    var etiquettesField = '';
    if (etiquettesFile != null)
        etiquettesField = document.getElementById(prefix + "EtiquettesField_" + etiquettesFile.options[etiquettesFile.selectedIndex].value);

    if (seriesFile != null && typeof seriesFile != 'undefined') {

        seriesField = document.getElementById(prefix + "SeriesField_" + seriesFile.options[seriesFile.selectedIndex].value);
        if (seriesField != null && typeof seriesFile != 'undefined')
            seriesFieldText = seriesField.options[seriesField.selectedIndex].text;

        serieFileText = seriesFile.options[seriesFile.selectedIndex].text;
    }




    //la description 
    var hfResDescription = document.getElementById("hf_res_description");
    var uiDescription = document.getElementById("rbqDescContent");

    var bIsCountOperation = valuesOperation.options[valuesOperation.selectedIndex].value.toUpperCase() == "COUNT" ? true : false;

    //Mettre à jour les valeurs des axes X et Y du graphique
    var dynNameY = document.getElementById("DynNameDiv_Y");
    var dynNameYBis = document.getElementById("DynNameDivBis_Y");

    var operation = valuesOperation.options[valuesOperation.selectedIndex].text;
    if (valuesField != null && valuesField.selectedIndex > -1)
        var value = bIsCountOperation ? valuesFile.options[valuesFile.selectedIndex].text : valuesField.options[valuesField.selectedIndex].text;
    else {
        bIsCountOperation = true;
        var value = valuesFile.options[valuesFile.selectedIndex].text;
    }


    dynNameY.innerHTML = operation + " (" + value + ")";
    dynNameYBis.innerHTML = dynNameY.innerHTML;

    var dynNameX = document.getElementById("DynNameDiv_X");
    var dynNameXBis = document.getElementById("DynNameDivBis_X");

    var etiquettesFieldValue = '';
    var etiquettesFileValue = '';
    if (etiquettesField != '' && etiquettesField != null) {
        etiquettesFieldValue = etiquettesField.options[etiquettesField.selectedIndex].text;
        etiquettesFileValue = etiquettesFile.options[etiquettesFile.selectedIndex].text;
        dynNameX.innerHTML = etiquettesFieldValue;
        dynNameXBis.innerHTML = etiquettesFieldValue;

    }

    //on met a jour les champs
    if (!bIsSingleChart) {

        uiDescription.innerHTML = hfResDescription.value.replace("@operation", valuesOperation.options[valuesOperation.selectedIndex].text)
            .replace("[@valuesfield]", bIsCountOperation ? "" : "[" + valuesField.options[valuesField.selectedIndex].text + "]")
            .replace("@valuesfile", valuesFile.options[valuesFile.selectedIndex].text)
            .replace("@etiquettesfield", etiquettesFieldValue)
            .replace("@etiquettesfile", etiquettesFileValue)
            .replace("@seriesfield", seriesFieldText)
            .replace("@seriesfile", serieFileText);
    }
}

function UpdateParams(prefix) {

    if (typeof prefix == 'undefined' || prefix == null || typeof prefix != 'string')
        prefix = '';

    //on mis a jour les params dans oReport
    var ddlFile = document.getElementById(prefix + "ValuesFile");
    var ddlField = document.getElementById(prefix + "ValuesField_" + ddlFile.options[ddlFile.selectedIndex].value);
    var ddlOperation = document.getElementById(prefix + "ValuesOperation");
    var liMultiLines = document.getElementById("liMultiLines");
    var tabFields = new Array();
    var tabOperations = new Array();
    //BSE:#56 239 Si la liste des rubriques est vide on force les valeurs
    if (ddlField.value == "") {
        tabFields[0] = parseInt(ddlFile.options[ddlFile.selectedIndex].value) + 99;
        tabOperations[0] = 'COUNT';
    }
    else {
        tabFields[0] = ddlField.value;
        tabOperations[0] = ddlOperation.value;
    }


    if (liMultiLines != null && typeof liMultiLines != 'undefined')
        for (var i = 0; i < liMultiLines.children.length; i++) {

            var ddlFieldNew = document.getElementById(ddlField.id + "_" + liMultiLines.children[i].id);
            var ddlOperationNew = document.getElementById(ddlOperation.id + "_" + liMultiLines.children[i].id);

            tabFields[i + 1] = ddlFieldNew.value;
            tabOperations[i + 1] = ddlOperationNew.value;
        }



    oReport.SetParam(prefix.toLowerCase() + "valuesfield", tabFields.join(";"));
    oReport.SetParam(prefix.toLowerCase() + "valuesoperation", tabOperations.join(";"));



}

function InsertRubriqueY(index) {

    var ddlFile = document.getElementById("ValuesFile");
    var ddlField = document.getElementById("ValuesField_" + ddlFile.options[ddlFile.selectedIndex].value);
    var ddlOperation = document.getElementById("ValuesOperation");

    var random = Math.ceil(10000000 * Math.random());

    //Le clonage  
    var ddlFieldNew = CloneNode(ddlField);
    ddlFieldNew.id = ddlField.id + "_" + random;
    ddlFieldNew.disabled = false;

    var ddlOperationNew = CloneNode(ddlOperation);
    ddlOperationNew.id = ddlOperation.id + "_" + random;

    var tabFields = oReport.GetParam("valuesfield").split(";");
    var tabOperations = oReport.GetParam("valuesoperation").split(";");

    //Changement de res pour l option de valeur "count" (la mettre a "Nombre d'occurences")
    for (var i = 0; i < ddlOperationNew.options.length; i++) {

        if (ddlOperationNew.options[i].value == "count") {
            ddlOperationNew.options[i].text = top._res_6366;
        }

    }

    //selection les valeur des ddl en fonction des valeurs presentent dans la table param
    if (index < tabFields.length) {

        ddlFieldNew.value = tabFields[index];
        ddlOperationNew.value = tabOperations[index];
    }
    //Ajout des evenements aux nouveaux selects
    ddlFieldNew.setAttribute("onchange", "UpdateParams();");
    ddlOperationNew.setAttribute("onchange", "UpdateParams();");

    var div = document.createElement("div");
    div.setAttribute("class", "logoDeleteLine");
    div.setAttribute("onclick", "DeleteRubriqueY(this.parentElement);");

    var liLine = document.createElement("li");
    liLine.id = random;
    liLine.appendChild(ddlFieldNew);
    liLine.appendChild(ddlOperationNew);
    liLine.appendChild(div)

    var liMultiLine = document.getElementById("liMultiLines");
    liMultiLine.appendChild(liLine);

    //Si pas de ligne ajoutée alors on reactive la premiere rubrique
    UpdateFirstRubriqueY();
}

function DeleteRubriqueY(obj) {


    //On supprime la ligne
    var liMultiLines = document.getElementById("liMultiLines");
    liMultiLines.removeChild(obj);

    //On met à jour les params
    UpdateParams();

    //rafraichit l ecran 2
    UpdateFirstRubriqueY();

}


function AddGaugePicker(elem, maxGaugeInterval) {
    var getFirstInterval = false;
    if (isNaN(parseInt(maxGaugeInterval)) || maxGaugeInterval < 2)
        maxGaugeInterval = 2;

    var nbInterval = 0;
    Array.prototype.slice.apply(elem.parentElement.getElementsByClassName('rbqValsLabelGauge')).forEach(
        function (liElement, index) {
            if (getAttributeValue(liElement, 'index') != '') {
                nbInterval++;
            }

        });

    if (nbInterval > maxGaugeInterval)
        return;
    else if (nbInterval == maxGaugeInterval)
        addClass(elem.querySelector('div[class^="logoGaugeAddLine"]'), 'logoGaugeAddLineDisabled');

    var parentLi = elem.parentElement;
    var index = parseInt(getAttributeValue(elem, 'index')) - 1;
    getFirstInterval = (index < 0);

    var maxIntervalElement = parentLi.querySelectorAll('li[index="' + (getFirstInterval ? 0 : index) + '"]')[0];
    var newGaugePiker = CloneNode(maxIntervalElement);
    var txtOriginImput = maxIntervalElement.getElementsByClassName('txtGaugeValue')[0];
    var txtImput = newGaugePiker.getElementsByClassName('txtGaugeValue')[0];
    var txtImputColor = newGaugePiker.getElementsByClassName('txtColor')[0];
    var colorPickerWrapper = newGaugePiker.getElementsByClassName('colorPickerWrapper')[0];
    var spanColorPicker = newGaugePiker.getElementsByClassName('colorPicker')[0];
    var value = (getFirstInterval ? 5 : parseInt(txtOriginImput.value) + 1);
    var logo = newGaugePiker.getElementsByClassName('logoGaugeAddLine')[0];

    if (value >= 100) {
        addClass(logo, 'logoGaugeAddLineDisabled');
        return;
    }


    setAttributeValue(txtImput, 'value', value);
    txtImput.value = value;

    SetIdForColorPicker(newGaugePiker, index + 1);
    SetIdForColorPicker(elem, index + 2);

    if (getFirstInterval) {
        txtImput.removeAttribute('disabled');
        setAttributeValue(logo, 'onclick', 'DeleteGaugePicker(this.parentElement);');
        logo.innerHTML = '';
        removeClass(logo, 'logoGaugeAddLine');
        addClass(logo, 'logoGaugeDeleteLine');
    }
    parentLi.insertBefore(newGaugePiker, elem);
    UpdateIntervalsValue();
    var cgnbinterval = oReport.GetParam('cgnbinterval');

    if (isNaN(parseInt(cgnbinterval)))
        oReport.SetParam('cgnbinterval', '1');
    else
        oReport.SetParam('cgnbinterval', (parseInt(cgnbinterval) + 1) + '');
}

function DeleteGaugePicker(elem) {
    var parent = elem.parentElement;
    parent.removeChild(elem);
    UpdateIntervalsValue();
}


function UpdateIntervalsValue() {
    var i = 0;
    var maxVal = 0;
    var cgintervals = new Array();

    Array.prototype.slice.apply(document.getElementsByClassName('rbqValsLabelGauge')).forEach(
        function (liElement, index) {
            if (getAttributeValue(liElement, 'index') != '') {
                var txtImputColor = liElement.getElementsByClassName('txtColor')[0];
                var valueTxt = liElement.getElementsByClassName('txtGaugeValue')[0].value;
                SetIdForColorPicker(liElement, index - i);
                var valueInt = parseInt(valueTxt);
                if (!isNaN(valueInt) && valueInt <= 100) {
                    if (maxVal < valueInt && valueInt < 100)
                        maxVal = valueInt;
                    cgintervals.push(valueTxt + ',' + txtImputColor.value);
                }

            } else
                i++;
        });

    if (maxVal < 99 && cgintervals.length < 5)
        removeClass(document.querySelector('div[class*="logoGaugeAddLineDisabled"]'), 'logoGaugeAddLineDisabled');
    if (cgintervals.length > 0) {
        oReport.SetParam('cgintervals', cgintervals.join(';'));
        oReport.SetParam('cgnbinterval', cgintervals.length + '');
    }
}

function CheckIntervalMinMaxValue(intervalElement) {
    var newVal = intervalElement.value;
    var txtGaugeValue = document.getElementsByClassName('txtGaugeValue');
    var logo = document.getElementsByClassName('logoGaugeAddLine')[0];

    for (var i = 0; i < txtGaugeValue.length; i++) {
        if (txtGaugeValue[i].id == intervalElement.id && i < (txtGaugeValue.length - 1) && i > 0) {
            if (parseInt(intervalElement.value) >= parseInt(txtGaugeValue[i + 1].value))
                intervalElement.value = parseInt(txtGaugeValue[i + 1].value) - 1;

            if (parseInt(intervalElement.value) <= parseInt(txtGaugeValue[i - 1].value))
                intervalElement.value = parseInt(txtGaugeValue[i - 1].value) + 1;

            var index = getAttributeValue(intervalElement.parentElement, 'index');
            var cgintervals = oReport.GetParam('cgintervals').split(';');

            if (cgintervals.length > 0 && index != '' && !isNaN(parseInt(index)) && cgintervals.length > parseInt(index))
                cgintervals[index] = intervalElement.value + ',' + cgintervals[index].split(',')[1];
            oReport.SetParam('cgintervals', cgintervals.join(';'));

            var maxIntervalValue = GetMaxIntervalValue();

            if (intervalElement.value >= 99 || maxIntervalValue == 99)
                addClass(logo, 'logoGaugeAddLineDisabled');
            else if (cgintervals.length < 5)
                removeClass(logo, 'logoGaugeAddLineDisabled');

            break;
        }
    }
}

//Retourne la valeur maximum et inférieur à 100 des inervalles 
function GetMaxIntervalValue() {
    var max = 0;
    var cgintervals = oReport.GetParam('cgintervals');
    if (cgintervals != '') {
        var intervalElements = cgintervals.split(';');
        if (intervalElements.length > 0) {
            for (var i = 0; i < intervalElements.length; i++) {
                var val = intervalElements[i].split(',')[0];
                if (!Number.isNaN(parseInt(val)) && parseInt(val) > max && parseInt(val) < 100)
                    max = parseInt(val);
            }
        }
    }
    return max;
}

function DeleteColorPickerValue(index) {
    if (!isNaN(parseInt(index))) {
        var intervalValue = document.getElementById('txtGaugeValue_' + index).value;
        var colorValue = document.getElementById('txtColorPicker_' + index).value;
        var cgintervals = oReport.GetParam('cgintervals').split(';');
        if (cgintervals.length > 0) {
            cgintervals.splice(index, 1);
            oReport.SetParam('cgintervals', cgintervals.join(';'));
            oReport.SetParam('cgnbinterval', cgintervals.length + '');
        }
    }
}

function SetIdForColorPicker(element, index) {

    var txtImput = element.getElementsByClassName('txtGaugeValue')[0];
    var txtImputColor = element.getElementsByClassName('txtColor')[0];
    var colorPickerWrapper = element.getElementsByClassName('colorPickerWrapper')[0];
    var spanColorPicker = element.getElementsByClassName('colorPicker')[0];
    element.id = element.id.split('_')[0] + '_' + index;
    txtImput.id = txtImput.id.split('_')[0] + '_' + index;
    setAttributeValue(element, 'index', index);
    txtImputColor.id = txtImputColor.id.split('_')[0] + '_' + index;
    txtImputColor.name = txtImputColor.id;
    spanColorPicker.id = spanColorPicker.id.split('_')[0] + '_' + index;
    setAttributeValue(txtImputColor, 'onchange', 'UpdateIntervalsValue()');
    setAttributeValue(colorPickerWrapper, 'onclick', 'top.nsAdmin.openColorPicker(document.getElementById("' + spanColorPicker.id + '"), document.getElementById("' + txtImputColor.id + '"));');
}

function UpdateParamReport(key, value, prefix) {

    //key qui a des Valeurs multiple
    if (key == 'valuesoperation' || key == 'valuesfield')
        UpdateParams();
    else {
        oReport.SetParam(key, value);
        UpdateDescription(prefix);
    }
    var sSelChart = oReport.GetParam("typechart");
    displayFusionCombinedGraph(sSelChart.split("|")[0], sSelChart.split("|")[1]);
}

function onAddLogo(bAddLogo) {

    var selLogo = document.getElementById('selLogos');
    var bIsChked = bAddLogo.getAttribute("chk") == "1" ? true : false;


    selLogo.disabled = !bIsChked;
    if (!bIsChked) {
        oReport.SetParam('logo', 'nologo');

        document.getElementById('selectedLogoDiv').style.display = "none";
    }
    else {
        document.getElementById('selectedLogoDiv').style.display = "";
    }
    onChangeLogo(selLogo);

}

function onChangeLogo(selLogo) {

    var selectedLogo = document.getElementById('selectedLogoDiv');
    var sCstWebPath = document.getElementById('cstWebPath').value;

    if (!selLogo.disabled && selLogo.value != "") {

        selectedLogoDiv.style.backgroundImage = "url('" + sCstWebPath + selLogo.value + "')";
        oReport.SetParam('logo', selLogo.value);

    }
    else {
        selectedLogoDiv.style.backgroundImage = "url('ghost.gif')";
    }

}


function openTemplateDialog(type) {

    if (!type) {
        type = "excel";
    }

    var templateDialog = new eModalDialog(top._res_103, 0, 'eFieldFiles.aspx', 850, 500);
    templateDialog.addParam("descid", "", "post");
    templateDialog.addParam("folder", 2, "post"); // correspond aux dossier MODELES dans DATAS
    templateDialog.addParam("files", "", "post");
    templateDialog.addParam("mult", "0", "post");
    templateDialog.addParam("filetype", type, "post");

    templateDialog.show();

    var myFunct = (function (obj, typeFile) { return function () { validTemplateDialog(obj, typeFile); } })(templateDialog, type);
    templateDialog.addButton(top._res_5003, myFunct, "button-green", "", "ok"); //Valider

}



function validTemplateDialog(templateDialog, type) {
    var aSelectedFiles = templateDialog.getIframe().getSelectedFiles();
    var sSelectedFile = aSelectedFiles[0];
    var aFiles = templateDialog.getIframe().getAllFiles();

    var selTemplate;
    if (type == "pdf") {
        selTemplate = document.getElementById("editor_template_pdflist");
    }
    else {
        selTemplate = document.getElementById("editor_template_excel_List");
    }

    if (sSelectedFile == "") {
        sSelectedFile = selTemplate.options[selTemplate.selectedIndex];
    }

    while (selTemplate.options.length > 0) {
        selTemplate.options.remove(0);
    }
    var o = document.createElement("option");
    selTemplate.add(o);

    for (var i = 0; i < aFiles.length; i++) {
        o = document.createElement("option");
        o.label = aFiles[i];
        o.text = aFiles[i];
        o.value = aFiles[i];
        o.selected = aFiles[i] == sSelectedFile;
        selTemplate.add(o);
    }

    if (sSelectedFile != "" && type == "pdf") {
        // Appel du manager pour analyse PDF
        analyzePDF(selTemplate);
    }


    if (type != 'pdf')
        setTemplate(selTemplate)

    templateDialog.hide();
}

// Analyse du PDF pour récupérer la liste des champs formulaire
function analyzePDF(elementSelect) {
    var sSelectedFile = elementSelect.value;

    var oManager = new eUpdater("mgr/ePdfFormManager.ashx", 0);
    oManager.ErrorCallBack = function () { }
    oManager.addParam("filename", sSelectedFile, "post");

    oManager.send(buildPdfMapping);
}

// Création de la liste déroulante contenant les champs formulaire du fichier PDF
function buildPDFFieldsList(oRes, descid) {
    var value = "";
    var oElmList = oRes.getElementsByTagName("field");
    var ddl = "<select id='ddlPdfFields" + descid + "'>";
    ddl += "<option value=''>" + top._res_375 + "</option>";
    if (oElmList.length > 0) {
        for (var i = 0; i < oElmList.length; i++) {
            value = getXmlTextNode(oElmList[i]);
            ddl += "<option value='" + descid + ";" + value + "'>" + value + "</option>";
        }
    }
    ddl += "</select>";
    return ddl;
}

// Création de l'interface de mapping des champs PDF
function buildPdfMapping(oRes) {
    var divPdf = document.getElementById("editor_template_pdfcols");

    var descIds = oReport.GetParam("field").split(";");
    var selectedFields = document.getElementById("ItemsUsed").getElementsByTagName("div");
    var idxFields = 0;
    var idxDiv = 0;
    var divField = null;
    var linkedFromTab = 0;
    var fieldDescId = 0;
    var paramDescId = null;
    var fieldLabel;
    var ddl = "";

    // On vide le div si jamais il contenait des élements
    divPdf.innerHTML = "";
    for (idxDiv = 0; idxDiv < selectedFields.length; idxDiv++) {
        fieldDescId = selectedFields[idxDiv].getAttribute("value");
        linkedFromTab = selectedFields[idxDiv].getAttribute("linkedFromTab");

        for (idxFields = 0; idxFields < descIds.length; idxFields++) {
            paramDescId = descIds[idxFields].split(",");

            if (
                (fieldDescId == paramDescId[0] && paramDescId.length == 1 && linkedFromTab == 0)
                ||
                (fieldDescId == paramDescId[0] && paramDescId.length == 2 && linkedFromTab == paramDescId[1])
            ) {
                if (fieldLabel)
                    fieldLabel = selectedFields[idxDiv].innerText;
                else
                    fieldLabel = selectedFields[idxDiv].textContent;

                divField = document.createElement("div");
                divField.setAttribute("class", "mappingField");

                // Label
                var label = document.createElement("label");
                if (label.innerText != null)
                    label.innerText = selectedFields[idxDiv].innerText;
                else
                    label.textContent = selectedFields[idxDiv].textContent;
                // Liste déroulante des champs du PDF
                var ddlHTML = buildPDFFieldsList(oRes, fieldDescId);
                divField.appendChild(label);
                divField.innerHTML += ddlHTML;

                divPdf.appendChild(divField);

            }
        }
    }

    // Sélections
    if (oReport.GetParam("pdfmap") != "") {
        var arrMapping = oReport.GetParam("pdfmap").split(',');
        var arrMappingForDescid, descid, selectField;
        for (var i = 0; i < arrMapping.length; i++) {
            arrMappingForDescid = arrMapping[i].split(';');
            descid = arrMappingForDescid[0];
            selectField = document.getElementById("ddlPdfFields" + descid);
            if (selectField)
                selectField.value = descid + ";" + arrMappingForDescid[1];
        }
    }
    /*
    else {
        // Sélection par défaut à "Aucune rubrique sélectionnée"
        var selects = document.querySelectorAll("select[id^='ddlPdfFields']");
        for (var i = 0; i < selects.length; i++) {
            selects[i].value = "";
        }
    } */
}

// Sauvegarde des paramètres de mapping
function savePdfMapping() {
    var div = document.getElementById("editor_template_pdfcols");
    if (div) {
        var selects = div.getElementsByTagName("select");
        var select = "";
        var pdfmap = "";
        var selectedValue = "";
        for (var i = 0; i < selects.length; i++) {
            select = selects[i];
            if (pdfmap != "")
                pdfmap += ",";
            selectedValue = select.options[select.selectedIndex].value;
            pdfmap += selectedValue;
        }
        // Sauvegarde du fichier sélectionné et du mapping
        var fileSelect = document.getElementById("editor_template_pdflist");
        if (fileSelect) {
            if (fileSelect.selectedIndex != 0) {
                oReport.SetParam("pdftemplate", fileSelect.options[fileSelect.selectedIndex].value);
                oReport.SetParam("usetemplate", "1");
                oReport.SetParam("pdfmap", pdfmap);
            }
            else {
                oReport.SetParam("pdftemplate", "");
                oReport.SetParam("usetemplate", "0");
            }
        }
    }
}

// Sauvegarde de la police et taille d'impression
function saveFontPref() {
    // Si format HTML
    if (oReport.GetParam("format") == "4") {
        var fontfamily = document.getElementById("selectFont");
        var fontsize = document.getElementById("selectFontSize");
        if (fontfamily && fontsize) {
            oReport.SetParam("fontfamily", fontfamily.value);
            oReport.SetParam("fontsize", fontsize.value);
        }
    }
}

// Chargement du mapping PDF
function loadSelectedPdf() {

    if (oReport.GetParam("pdftemplate") != "") {
        // Sélection du fichier PDF configuré
        var select = document.getElementById("editor_template_pdflist");
        select.value = oReport.GetParam("pdftemplate");

        analyzePDF(select);
    }

}

function setTemplate(obj) {
    if (obj.selectedIndex == 0) {
        oReport.SetParam("template", "");
        oReport.SetParam("usetemplate", "0");
    }
    else {
        oReport.SetParam("template", obj.value);
        oReport.SetParam("usetemplate", "1");

    }
}


function onChangeChartReportFile(lst, prefix, forceField) {
    if (forceField == undefined || forceField == null)
        forceField = true;

    if (typeof prefix == 'undefined' || prefix == null)
        prefix = '';

    var tabDescid = lst.options[lst.selectedIndex].value;
    var aInfos = lst.id.split('_');
    var tabIndex = 0;
    var lineIndex = aInfos[1];
    var field = -1;
    var argument = 'line_0_' + lineIndex;
    if (prefix != '')
        argument = argument + '&#&' + prefix;

    if (forceField)
        field = parseInt(tabDescid) + 95;
    getExpressFilterEmptyLine(tabIndex, lineIndex, field, tabDescid, 0, 0, onChangeExpressFilterFieldTreatment, argument);
}


function onChangeChartReportField(lst, prefix) {

    if (typeof prefix == 'undefined' || prefix == null)
        prefix = '';
    var argument = lst.id;
    var aInfos = lst.id.split('_');
    var tabIndex = aInfos[1];
    var lineIndex = aInfos[2];
    var value = lst.options[lst.selectedIndex].value;
    var tabDescid = document.getElementById(prefix + "file_" + lineIndex).options[document.getElementById(prefix + "file_" + lineIndex).selectedIndex].value;

    var fieldDescid = lst.options[lst.selectedIndex].value;

    var oLineOp = document.getElementById("and_" + tabIndex + "_" + lineIndex);
    var lineOp = 0;
    if (oLineOp != null)
        lineOp = oLineOp.options[oLineOp.selectedIndex].value;

    if (prefix != '')
        argument = argument + '&#&' + prefix;
    if (value == '-1')
        argument = argument + '&#|#&' + value;
    getExpressFilterEmptyLine(tabIndex, lineIndex, fieldDescid, tabDescid, 0, lineOp, onChangeExpressFilterFieldTreatment, argument);
}

function getFilterListForTab(tab, prefix, treatFunc, arg) {

    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);

    upd.ErrorCallBack = function (result) {
        document.getElementById(listId).lastChild.outerHTML = '';
        console.log(result);
        setWait(false);
    };

    upd.addParam("action", "chartFilterline", "post");
    upd.addParam("maintab", tab, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("reportId", oReport.GetId(), "post");
    //Ajout du prefix pour le filtre express Combiné
    if (typeof prefix != 'undefined' && prefix != null && prefix != '')
        upd.addParam("prefixFilter", prefix, "post");

    upd.send(treatFunc, arg);
}

function drawListFilter(result, listId) {
    document.getElementById(listId).lastChild.outerHTML = result;
    document.getElementById(listId).lastChild.disabled = false;
    setWait(false);
}

function getExpressFilterEmptyLine(tabIdx, lineIdx, descId, tab, bEndOperator, oLineOp, treatFunc, fctArg) {
    setWait(true);
    var prefix = fctArg.split('&#|#&')[0].split('&#&')[1];
    var upd = new eUpdater("mgr/eFilterWizardManager.ashx", 1);


    //Gestion d'erreur : a priori, pas de traitement particulier
    upd.ErrorCallBack = function () {
    };

    upd.addParam("action", "emptyExpressFilterline", "post");
    upd.addParam("filtertype", 0, "post");
    upd.addParam("maintab", tab, "post");
    upd.addParam("tabindex", tabIdx, "post");
    upd.addParam("lineindex", lineIdx, "post");
    upd.addParam("descid", descId, "post");
    upd.addParam("tab", tab, "post");
    upd.addParam("endoperator", bEndOperator, "post");
    upd.addParam("lineoperator", oLineOp, "post");
    //Ajout du prefix pour le filtre express Combiné
    if (typeof prefix != 'undefined' && prefix != null && prefix != '')
        upd.addParam("prefixFilter", prefix, "post");
    upd.send(treatFunc, fctArg);
}

function onChangeExpressFilterFieldTreatment(oRes, lstId) {
    
    var value = lstId.split('&#|#&')[1];
    var prefix = lstId.split('&#|#&')[0].split('&#&')[1];
    var display = '';

    if (typeof prefix == 'undefined' || prefix == null)
        prefix = '';

    var aInfos = lstId.split('&#&')[0].split('_');
    var tabIndex = aInfos[1];
    var lineIndex = aInfos[2].split('&#|#&')[0];
    var oLine = document.getElementById(prefix + "line_" + tabIndex + "_" + lineIndex);
    oLine.style.opacity = 0;
    oLine.style.filter = 'alpha(opacity = 0)';
    var divTmp = document.createElement("div");
    divTmp.innerHTML = oRes;

    oLine.innerHTML = divTmp.getElementsByTagName("div")[0].innerHTML;
    fadeThis(oLine.id, document);

    if (value == '-1')
        display = 'none';

    if (prefix != '') {
        var selectedIndex = 0;
        var nbDisplayed = 0;
        if (prefix.replace('y', '') != prefix) {

            document.getElementById(prefix.replace('y', 'z') + 'file_' + lineIndex).style.display = display;
            document.getElementById(prefix.replace('y', 'z') + 'label_' + lineIndex).style.display = display;
            document.getElementById(prefix.replace('y', 'z') + 'fieldsline_' + lineIndex).style.display = (value == '-1' ? display : 'inline-block');
            var combinedY = document.getElementById(prefix + 'field_0_' + lineIndex);
            var combinedZ = document.getElementById(prefix.replace('y', 'z') + 'field_0_' + lineIndex);
            var fmt = getAttributeValue(combinedY.options[combinedY.selectedIndex], 'fmt');
            var pud = getAttributeValue(combinedY.options[combinedY.selectedIndex], 'pud');

            Array.prototype.slice.apply(combinedZ.querySelectorAll("option")).forEach(
                function (optionElem, index) {
                    if ((getAttributeValue(optionElem, 'fmt') == fmt && getAttributeValue(optionElem, 'pud') == pud) || index == 0) {
                        setAttributeValue(optionElem, 'display', '1');
                        optionElem.removeAttribute('disabled');
                        nbDisplayed++;
                        if (selectedIndex == 0)
                            selectedIndex = index;
                    }
                    else {



                        setAttributeValue(optionElem, 'display', '0');
                        setAttributeValue(optionElem, 'disabled', 'disabled');
                    }

                }
            );
            combinedZ.selectedIndex = selectedIndex;
        } else {
            var combinedZ = document.getElementById(prefix + 'field_0_' + lineIndex);
            var combinedY = document.getElementById(prefix.replace('z', 'y') + 'field_0_' + lineIndex);
            var fmt = getAttributeValue(combinedY.options[combinedY.selectedIndex], 'fmt');
            var pud = getAttributeValue(combinedY.options[combinedY.selectedIndex], 'pud');


            Array.prototype.slice.apply(combinedZ.querySelectorAll("option")).forEach(
                function (optionElem, index) {
                    if ((getAttributeValue(optionElem, 'fmt') == fmt && getAttributeValue(optionElem, 'pud') == pud) || index == 0) {


                        setAttributeValue(optionElem, 'display', '1');
                        optionElem.removeAttribute('disabled');
                        nbDisplayed++;
                        if (selectedIndex == 0)
                            selectedIndex = index;
                    }
                    else {



                        setAttributeValue(optionElem, 'display', '0');
                        setAttributeValue(optionElem, 'disabled', 'disabled');
                    }

                }
            );
            combinedZ.selectedIndex = selectedIndex;
        }
    }

    setWait(false);

}

//Récuperer les filtres express pour les graphiques
function setExpressFilterValuesParams() {

    var val;
    var file;
    var field;
    var op;
    var prefixY = '';
    var prefixZ = '';
    var nModelChartSerieType = oReport.GetParam("typechart").split("|")[0];
    var nModelChartType = oReport.GetParam("typechart").split("|")[1];
    var idWrappeur = 'ulWrappeur';
    if (nModelChartSerieType == SPECIAL_CHART_TYPE) {
        prefixY = top._CombinedY.toLowerCase();
        prefixZ = top._CombinedZ.toLowerCase();
        idWrappeur = 'combined' + idWrappeur;
    }

    if (nModelChartSerieType == SPECIAL_CHART_TYPE && nModelChartType == '2') {
        SetGaujeExpressFilter(idWrappeur, prefixY);
        SetGaujeExpressFilter(idWrappeur, prefixZ);
    } else {
        SetFilterValue(idWrappeur, prefixY);

        if (prefixZ != '')
            SetFilterValue(idWrappeur, prefixZ, prefixY);
    }
}


function SetFilterValue(ul, prefix, elem) {

    if (typeof elem != 'string' && elem == null || elem == '')
        elem = prefix;

    var nbExpressFilter = document.querySelectorAll("ul#" + ul + " li[id*='selectExpressFilter']").length;

    for (var i = 0; i < nbExpressFilter; i++) {
        //Get FILE VALUE
        val = document.getElementById(prefix + 'file_' + i);
        if (typeof val != 'undefined' && val != null)
            oReport.SetParam(val.id, val.options[val.selectedIndex].value);
        //GET FIELD VALUE
        val = document.getElementById(prefix + 'field_0_' + i);

        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {

            var v = val.options[val.selectedIndex].value;
            if (!Number.isNaN(parseInt(v)) && v != -1)
                oReport.SetParam(val.id, val.options[val.selectedIndex].value);
            else
                oReport.SetParam(prefix + 'field_0_' + i, '');

        } else
            oReport.SetParam(prefix + 'field_0_' + i, '');

        //GET OPERATOR VALUE
        val = document.getElementById(elem + 'op_0_' + i);
        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {
            if (elem == prefix)
                oReport.SetParam(val.id, val.options[val.selectedIndex].value);
            else
                oReport.SetParam(val.id.replace(elem, prefix), val.options[val.selectedIndex].value);
        }

        else {
            if (elem == prefix)
                oReport.SetParam(elem + 'op_0_' + i, '');
            else
                oReport.SetParam(prefix + 'op_0_' + i, '');

        }
        //GET FILTER VALUE
        val = document.getElementById(elem + 'value_0_' + i);
        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {
            if (elem == prefix)
                oReport.SetParam(val.id, getAttributeValue(val, 'ednvalue'));
            else
                oReport.SetParam(val.id.replace(elem, prefix), getAttributeValue(val, 'ednvalue'));
        }

        else {
            if (elem == prefix)
                oReport.SetParam(elem + 'value_0_' + i, '');
            else
                oReport.SetParam(prefix + 'value_0_' + i, '');
        }

    }
}


function SetGaujeExpressFilter(ul, prefix) {
    var nbExpressFilter = document.querySelectorAll("ul#" + ul + " li[id*='selectExpressFilter']").length;
    for (var i = 0; i < nbExpressFilter; i++) {
        //Get FILE VALUE
        val = document.getElementById(prefix + 'file_' + i);
        if (typeof val != 'undefined' && val != null)
            oReport.SetParam(val.id, val.options[val.selectedIndex].value);
        //GET FIELD VALUE
        val = document.getElementById(prefix + 'field_0_' + i);

        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {

            var v = val.options[val.selectedIndex].value;
            if (!Number.isNaN(parseInt(v)) && v != -1)
                oReport.SetParam(val.id, val.options[val.selectedIndex].value);
            else
                oReport.SetParam(prefix + 'field_0_' + i, '');

        } else
            oReport.SetParam(prefix + 'field_0_' + i, '');

        //GET OPERATOR VALUE
        val = document.getElementById(top._CombinedY.toLowerCase() + 'op_0_' + i);
        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {
            oReport.SetParam(val.id, val.options[val.selectedIndex].value);
        } else
            oReport.SetParam(prefix + 'op_0_' + i, '');

        //GET FILTER VALUE
        val = document.getElementById(top._CombinedY.toLowerCase() + 'value_0_' + i);
        if (typeof val != 'undefined' && val != null && val.selectedIndex != -1) {
            oReport.SetParam(val.id, getAttributeValue(val, 'ednvalue'));

        } else
            oReport.SetParam(prefix + 'value_0_' + i, '');

    }
}

function specialTraitementForFunnelAndPyramid(modelChartType, oldSelChartImg) {


    if (modelChartType != CHART_FUNNEL && modelChartType != '13' && modelChartType != CHART_PYRAMID && modelChartType != '14') {
        Array.prototype.slice.apply(document.querySelectorAll("ul#ulWrappeur select option[display='0']")).forEach(
            function (optionElem) {
                setAttributeValue(optionElem, 'display', '1');
                optionElem.removeAttribute('disabled');
            }
        );
    }
    else {

        ActiveFieldsForFunnelAndPyramid(oldSelChartImg);
    }

}

//Activer les rubriques type catalogue et utilisateur pour les graphiques funnel et pyramid
function ActiveFieldsForFunnelAndPyramid(oldSelChartImg) {

    var etiquettesFile = document.getElementById("EtiquettesFile");
    var selectedOptionFile = etiquettesFile.options[etiquettesFile.selectedIndex].value;
    var textSelectedOptionFile = etiquettesFile.options[etiquettesFile.selectedIndex].text;

    var selectedEtiquettesFileIndex;
    Array.prototype.slice.apply(document.querySelectorAll("ul#ulWrappeur select[id*='EtiquettesField_" + selectedOptionFile + "'] option")).forEach(
        function (optionElem, index) {
            var pud = getAttributeValue(optionElem, 'pud');
            if (pud == '0' || pud == '' || (!isNaN(parseInt(pud)) && parseInt(pud) % 100 == 1)) {


                setAttributeValue(optionElem, 'display', '0');
                setAttributeValue(optionElem, 'disabled', 'disabled');
            }
            else {
                if (isNaN(parseInt(selectedEtiquettesFileIndex))) {
                    selectedEtiquettesFileIndex = index;
                }
            }
        }
    );

    if (typeof selectedEtiquettesFileIndex == 'undefined') {
        eAlert(1, top._res_72, top._res_8538, top._res_8577.replace('<TABLE>', textSelectedOptionFile), null, null, function () {
            Array.prototype.slice.apply(document.querySelectorAll("ul#ulWrappeur select option[display='0']")).forEach(
                function (optionElem) {
                    setAttributeValue(optionElem, 'display', '1');
                    optionElem.removeAttribute('disabled');
                }
            );

            if (oldSelChartImg)
                ActiveChart(oldSelChartImg);
        });
        return;
    } else {
        document.getElementById('EtiquettesField_' + selectedOptionFile).selectedIndex = selectedEtiquettesFileIndex;
        document.getElementById('EtiquettesField_' + selectedOptionFile).onchange();
    }
}

function specialTraitementForGaugeChart(modelChartType) {
    var valuesFilesuffix = 'ValuesFile';
    var valuesFileId = top._CombinedY + valuesFilesuffix;//CombinedYValuesFile
    var tabMain = oReport.GetTab();
    var nbDisplay = 0;
    var firstDisplayIndex = -1;

    //var etiquetteFileValue = obj.options[obj.selectedIndex].value;
    if (modelChartType == CHART_CIRCULARGAUGE) {
        Array.prototype.slice.apply(document.querySelectorAll("#" + valuesFileId + " option")).forEach(
            function (optionElem) {
                setAttributeValue(optionElem, 'display', '1');
                optionElem.removeAttribute('disabled');
            }
        );

        var selectdeValue = getAttributeValue(document.getElementById(top._CombinedY + valuesFilesuffix), 'selectedfile');
        UpdatValuesFileList(document.getElementById(top._CombinedY + 'EtiquettesFile'), top._CombinedZ, selectdeValue);

    } else {
        Array.prototype.slice.apply(document.querySelectorAll("#" + valuesFileId + " option")).forEach(
            function (optionElem, index) {
                var linkedtab = getAttributeValue(optionElem, 'linkedtab');
                if (!(linkedtab != '' && (';' + linkedtab + ';').indexOf(';' + tabMain + ';') != -1)) {


                    setAttributeValue(optionElem, 'display', '0');
                    setAttributeValue(optionElem, 'disabled', 'disabled')
                } else {
                    nbDisplay++;
                    if (firstDisplayIndex < 0)
                        firstDisplayIndex = index;
                }
            }
        );

        if (nbDisplay > 0)
            document.getElementById(valuesFileId).selectedIndex = (firstDisplayIndex > 0 ? firstDisplayIndex : 0);

        //déclenchement de l'Event on change sur la DDl des tables pour Display DDL fields
        document.getElementById(valuesFileId).onchange();
    }
}


function displayEtiquettesTri(modelChartType, nSelChartType) {

    if (nSelChartType != SINGLE_CHART_TYPE) {
        document.getElementById("EtiquettesTri").style.display = "none";
        document.getElementById("labelEtiquettesTri").style.display = "none";
        return;
    }
    //Affichage du tri pour les catalogue, utilisé seulement dans les graphyque type funnel
    var etiquettesTri = document.getElementById("EtiquettesTri");
    var labelEtiquettesTri = document.getElementById("labelEtiquettesTri");

    if (etiquettesTri != null && typeof etiquettesTri != 'undefined' && labelEtiquettesTri != null && typeof labelEtiquettesTri != 'undefined') {
        if (modelChartType != CHART_FUNNEL && modelChartType != '13' && modelChartType != CHART_PYRAMID && modelChartType != '14') {
            document.getElementById("EtiquettesTri").style.display = "none";
            document.getElementById("labelEtiquettesTri").style.display = "none";
        }
        else {
            document.getElementById("EtiquettesTri").style.display = "";
            document.getElementById("labelEtiquettesTri").style.display = "";
        }
    }

}

//Permet d'afficher l'option des fusions de l'èchelle pour le graphique combiné et affecte les paramètres
function displayFusionCombinedGraph(nSelChartType, nModelChartType) {
    //BSE:#67 048
    if (nSelChartType != SPECIAL_CHART_TYPE)
        return;

    var chartType = (nModelChartType == '1' ? CHART_COMBINED : CHART_CIRCULARGAUGE);
    UpdateUlWrappeurDisplay(nSelChartType, chartType);

    var displayFusion = true;
    var liDisplayZaxe = document.getElementById("chartDisplayZaxe");
    var chkDisplayZaxe = document.getElementById("DisplayZaxe");
    if (!(nSelChartType == SPECIAL_CHART_TYPE && nModelChartType == '1'))
        displayFusion = false;

    else {
        var idReport = oReport.GetId();
        var displayzaxe = oReport.GetParam('displayzaxe');

        var operationItem = document.querySelectorAll('#combinedulWrappeur select[name$="ValuesOperation"]');
        var i = 0;
        while (i < operationItem.length) {
            if (operationItem[i].options[operationItem[i].selectedIndex].value == 'COUNT') {
                if (i > 0 && 'COUNT' == operationItem[i - 1].options[operationItem[i - 1].selectedIndex].value)
                    displayFusion = true;
                else
                    displayFusion = false;
            }
            i += 1;
        }
    }

    if (displayFusion) {
        var value = idReport > 0 ? displayzaxe : '1';
        liDisplayZaxe.style.display = "";
        setAttributeValue(chkDisplayZaxe, "chk", value);
        oReport.SetParam('displayzaxe', value);
        chgChk(chkDisplayZaxe, value == '1');

    } else {

        setAttributeValue(chkDisplayZaxe, "chk", "1");
        liDisplayZaxe.style.display = "none";
        oReport.SetParam('displayzaxe', '0');
        chgChk(chkDisplayZaxe);
    }

    //BSE:#67 048
    if (nSelChartType == SPECIAL_CHART_TYPE && chartType == CHART_CIRCULARGAUGE) {
        //ne pas Afficher la légende
        var chkDisplayLegend = document.getElementById("displayLegend");
        setAttributeValue(chkDisplayLegend, "chk", "1");
        chkDisplayLegend.parentElement.style.display = "none";
        oReport.SetParam('displaylegend', '0');
        chgChk(chkDisplayLegend);

        //ne pas "Utiliser la couleur du thème"
        var chkUseThemeColor = document.getElementById("UseThemeColor");
        var liUseThemeColor = document.getElementById("LiUseThemeColor");
        setAttributeValue(chkUseThemeColor, "chk", "1");
        liUseThemeColor.style.display = "none";
        oReport.SetParam('useThemeColor', '0');
        chgChk(chkUseThemeColor);

        //ne pas "Afficher la grille des valeurs"
        var chkDisplayGrid = document.getElementById("DispGrid");
        var liUseGrid = document.getElementById("chartDisplayGrid");
        setAttributeValue(chkDisplayGrid, "chk", "1");
        liUseGrid.style.display = "none";
        oReport.SetParam('DispGrid', '0');
        chgChk(chkDisplayGrid);

        // on n'affiche pas le choix pour les étiquettes 
        var lisDisplayEtiquette = document.getElementById("displayEtiquette");
        var lstdisplayx = document.getElementById("lstdisplayx");
        lisDisplayEtiquette.style.display = "none";
        oReport.SetParam('lstdisplayx', '1');
        oReport.SetParam('displayx', '0');

        //BSE:#68 382 => A la création d'un graphique type Jauge, il faut forcer le type de valeur à fixe 
        if (oReport.GetId() == 0 && oReport.GetParam('cgvtype') == '')
            oReport.SetParam('cgvtype', '0');
    }
}

//Récupérer le tri pour les catalogues: graphyque type Funnel et Pyramid
//BSE:#60 562
function setEtiquettesTriValuesParams() {
    var xTri = document.getElementById('EtiquettesTri');
    oReport.SetParam('etiquettestri', xTri.options[xTri.selectedIndex].value);
}

function toggle(btnRadioId, toggleID) {
    var btn = document.getElementById(btnRadioId);
    var toggle = document.getElementById(toggleID);
    updateToggle = btn.checked ? toggle.disabled = false : toggle.disabled = true;
}


