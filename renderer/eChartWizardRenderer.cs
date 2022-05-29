using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Assistant Graphique pour les reportings Graphiques
    /// </summary>
    public class eChartWizardRenderer : eReportWizardRenderer
    {
        //Le type du chart: Pie, Bar, Column , doughnut or Line
        private string _sGraphicModelType = String.Empty;
        private bool _sGraphicB3DType = false;
        private bool _bIsNewGraphic = false;
        private ICollection<ColorPickerProprieties> ColorPickerList;
        private String _sChartPreview = String.Empty;


        private eChartWizardRenderer(ePref ePref, Int32 width, Int32 height, TypeReport reportType, Int32 tab)
            : base(ePref, width, height, reportType, tab)
        {
            _rType = RENDERERTYPE.ChartWizard;
        }

        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant Reporting et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="height">hauteur de l'interface</param>
        /// <param name="width">largeur de l'interface</param>
        /// <param name="reportType">Type de rapport pour l'assistant</param>
        /// <param name="tab">Fichier d'origine</param>
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eChartWizardRenderer GetChartWizardRenderer(ePref ePref, Int32 width, Int32 height, TypeReport reportType, Int32 tab)
        {

            return new eChartWizardRenderer(ePref, width, height, reportType, tab);

        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected override Panel BuildStepDiv(Int32 step, Boolean isActive)
        {
            Panel stepBloc = new Panel();
            stepBloc.ID = "step_" + step.ToString();
            Panel numberBloc = new Panel();
            numberBloc.ID = "txtnum_" + step.ToString();

            stepBloc.Attributes.Add("onclick", String.Concat("StepClick('", step, "');"));

            numberBloc.Controls.Add(new LiteralControl(step.ToString()));
            Label lbl = new Label();

            switch (step)
            {
                case 1:
                    lbl.Text = eResApp.GetRes(Pref, 6379);
                    break;
                case 2:
                    lbl.Text = eResApp.GetRes(Pref, 6380);
                    break;
                case 3:
                    lbl.Text = eResApp.GetRes(Pref, 1304);

                    break;
                case 4:
                    lbl.Text = eResApp.GetRes(Pref, 6381);
                    break;
                default:
                    lbl.Text = String.Concat(eResApp.GetRes(Pref, 1617), " ", step);
                    break;
            }
            stepBloc.CssClass = isActive ? "state_grp-current" : _report == null ? "state_grp" : "state_grp-validated";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);


            return stepBloc;
        }


        /// <summary>
        /// Construit le bloc div d'une étape donnée de l'assitant et le retourne
        /// </summary>
        /// <param name="step">Numéro d'étape de l'assistant</param>
        /// <returns>Panel(div) de l'étape demandée</returns>
        protected override Panel BuildBodyStep(Int32 step)
        {
            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            Label lblFormat = new Label();

            switch (step)
            {
                case 1:
                    #region Première Page
                    pEditDiv.Controls.Add(BuildSelectChartPanel());
                    #endregion
                    break;
                case 2:
                    #region Seconde Page
                    pEditDiv.Controls.Add(BuildSelectFieldsPanel());
                    #endregion
                    break;
                case 3:
                    #region Troisième Page
                    pEditDiv.Controls.Add(BuildFormatPanel());
                    #endregion
                    break;
                case 4:
                    #region Quatrième Page
                    pEditDiv.Controls.Add(BuildRecordingPanel());
                    #endregion
                    break;
            }

            return pEditDiv;
        }

        /// <summary>
        /// Construit la page contenant les différents modèles de graphique disponibles
        /// </summary>
        /// <returns></returns>
        private Panel BuildSelectChartPanel()
        {
            HtmlInputRadioButton radioBtn;
            Panel pn = new Panel();
            pn.ID = "chartPanelSelect";
            pn.CssClass = "template_sel";
            pn.Attributes.Add("model", "histo"); //par defaut c est histo qui est selectionné


            HtmlGenericControl ul = new HtmlGenericControl("ul");
            pn.Controls.Add(ul);

            HtmlGenericControl liLabel = new HtmlGenericControl("li");
            ul.Controls.Add(liLabel);

            LiteralControl lc = new LiteralControl(string.Concat(eResApp.GetRes(Pref, 6370), " :"));//
            liLabel.Attributes.Add("class", "editor_fieldlabelline");
            liLabel.Controls.Add(lc);


            String sTypeChart = GetReportParam(_report, "TypeChart");
            Int32 nSelectedSerie = 1;
            Int32 nSelectedChart = 0;
            if (sTypeChart.Length > 0)
            {
                Int32.TryParse(sTypeChart.Split('|')[0], out nSelectedSerie);
                Int32.TryParse(sTypeChart.Split('|')[1], out nSelectedChart);
            }

            //Type de série
            HtmlGenericControl liType = new HtmlGenericControl("li");
            liType.Attributes.Add("class", "chart-ml");
            ul.Controls.Add(liType);
            lc = new LiteralControl(String.Concat(eResApp.GetRes(Pref, 105), " : "));
            liType.Controls.Add(lc);

            //BSE : 54 285 remplacer la DropDownList par des radios buttons
            //DropDownList ddlChartType = new DropDownList();
            //liType.Controls.Add(ddlChartType);
            //ddlChartType.ID = "ChartType";

            //if (_report.Id == 0)
            //    ddlChartType.Attributes.Add("onchange", "DisplayChartsList(this);");

            XmlDocument chartsList = new XmlDocument();
            chartsList.Load(AppDomain.CurrentDomain.BaseDirectory + "\\Charts\\Charts.xml");

            foreach (XmlNode xmlSerieType in chartsList.SelectNodes("//series/serie"))
            {

                Int32 nResId;
                Int32 nTypeSerie;
                Int32.TryParse(xmlSerieType.Attributes["res"].Value, out nResId);
                Int32.TryParse(xmlSerieType.Attributes["id"].Value, out nTypeSerie);

                // MCR 39206 : proposer tous les graphes : simples, multiseries, empilees dans la dropdownlist id="ChartType", 
                //  
                // if (nTypeSerie == 1)
                // {

                //BSE : 54 285 remplacer la DropDownList par des radios buttons
                //ListItem optionSerie = new ListItem(eResApp.GetRes(Pref, nResId), nTypeSerie.ToString());
                //optionSerie.Selected = (nTypeSerie == nSelectedSerie);
                //ddlChartType.Items.Add(optionSerie);


                //BSE : 54 285 remplacer la DropDownList par des radios buttons
                radioBtn = new HtmlInputRadioButton();
                radioBtn.Name = "ChartType";
                radioBtn.Value = nTypeSerie.ToString();
                radioBtn.ID = "ChartType" + nTypeSerie.ToString();
                radioBtn.Checked = (nTypeSerie == nSelectedSerie);
                radioBtn.Attributes.Add("class", "btnChart");
                radioBtn.Attributes.Add("onclick", "DisplayChartsList(this);");

                if (_report.Id > 0 && nTypeSerie != nSelectedSerie)
                    radioBtn.Disabled = true;

                HtmlGenericControl label = new HtmlGenericControl("Label") { InnerText = eResApp.GetRes(Pref, nResId) };
                label.Attributes.Add("for", "ChartType" + nTypeSerie.ToString());
                liType.Controls.Add(radioBtn);
                liType.Controls.Add(label);

                label.Style.Add(HtmlTextWriterStyle.Cursor, "not-allowed");



                // }
            }
            //BSE : 54 285 remplacer la DropDownList par des radios buttons
            //if (_report.Id > 0)
            //    ddlChartType.Enabled = false;

            foreach (XmlNode xmlSerieType in chartsList.SelectNodes("//series/serie"))
            {
                Int32 nbCharts = xmlSerieType.SelectSingleNode("charts").ChildNodes.Count;
                Int32 nSerie;
                Int32.TryParse(xmlSerieType.Attributes["id"].Value, out nSerie);
                double nbChartByLine = eLibConst.NB_MAX_ICON_CHART_BY_LINE;
                double minWidth = eLibConst.MAX_WIDTH_ICON_CHART;
                double nbChartInXml = xmlSerieType.SelectSingleNode("charts").ChildNodes.Count;

                if (nSelectedSerie != nSerie && _report.Id > 0)
                    continue;

                HtmlGenericControl liChartTemplates = new HtmlGenericControl("li");
                ul.Controls.Add(liChartTemplates);

                liChartTemplates.ID = String.Concat("liChrts_", nSerie);
                liChartTemplates.Attributes.Add("class", "chart-ml");

                if ((nSelectedSerie != (int)eModelConst.CHART_TYPE.SPECIAL && _report.Id > 0) || _report.Id == 0)
                    liChartTemplates.Attributes.Add("onclick", "selectChart(event);");

                if (nSelectedSerie != nSerie)
                    liChartTemplates.Style.Add("display", "none");

                System.Web.UI.WebControls.Table tbCharts = new System.Web.UI.WebControls.Table();
                liChartTemplates.Controls.Add(tbCharts);

                tbCharts.ID = String.Concat("tbChrts_", nSerie);
                tbCharts.CssClass = "tbChrts";

                Int32 nCpt = 0;
                TableRow tr = new TableRow();
                TableRow trChartLabels = new TableRow();
                foreach (XmlNode xmlChart in xmlSerieType.SelectSingleNode("charts").ChildNodes)
                {
                    if (xmlChart.NodeType == XmlNodeType.Comment)
                    {
                        nbChartInXml--;
                        continue;
                    }

                    String sChartId = String.Concat(nSerie, '|', xmlChart.Attributes["id"].Value);
                    Boolean bSelectedChart = (sTypeChart == sChartId);

                    if (nSelectedSerie == (int)eModelConst.CHART_TYPE.SPECIAL && _report.Id > 0 && !bSelectedChart)
                    {
                        nbChartInXml--;
                        continue;
                    }


                    _bIsNewGraphic = xmlChart.Attributes["isNew"].Value == "1";
                    if (nCpt % nbChartByLine == 0)
                    {
                        tr = new TableRow();
                        trChartLabels = new TableRow();
                        trChartLabels.CssClass = "trChrtLbls";
                        tbCharts.Rows.Add(tr);
                        tbCharts.Rows.Add(trChartLabels);
                    }
                    else
                    {
                        tr.Cells.Add(new TableCell());
                        trChartLabels.Cells.Add(new TableCell());
                    }


                    if (bSelectedChart)
                    {
                        _sGraphicModelType = xmlChart.Attributes["type"].Value;
                        _sGraphicB3DType = xmlChart.Attributes["is3d"].Value == "1";

                    }


                    TableCell tc = new TableCell();
                    tr.Cells.Add(tc);
                    TableCell tcChrtLabel = new TableCell();
                    trChartLabels.Cells.Add(tcChrtLabel);



                    String sImgSrc = String.Concat(@"Charts/Img/", xmlChart.Attributes["imgname"] != null ? xmlChart.Attributes["imgname"].Value : xmlChart.Attributes["name"].Value, "Chart.jpg");

                    tc.Style.Add("background-image", sImgSrc);
                    tc.CssClass = String.Concat("graphCadre", bSelectedChart ? " graphCadreSel" : "");
                    tc.ID = String.Concat("chrt_", sChartId.Replace('|', '_'));
                    tc.Attributes.Add("echrt", sChartId);
                    tc.Attributes.Add("type", xmlChart.Attributes["type"].Value);
                    tc.Attributes.Add("is3D", xmlChart.Attributes["is3d"].Value);
                    //BSE #61 394
                    if (_bIsNewGraphic)
                    {
                        HtmlGenericControl bNew = new HtmlGenericControl("div");
                        bNew.Attributes.Add("class", "imgBnew");
                        //bNew.InnerHtml = eResApp.GetRes(Pref, 31);
                        tc.Controls.Add(bNew);
                    }


                    if (bSelectedChart)
                    {
                        _sChartPreview = xmlChart.Attributes["type"].Value;// sImgSrc;
                        pn.Attributes.Add("model", _sChartPreview);
                    }
                    Int32 iResId;

                    if (Int32.TryParse(xmlChart.Attributes["res"].Value, out iResId))
                    {
                        tcChrtLabel.Text = String.Concat(eResApp.GetRes(Pref, iResId), xmlChart.Attributes["is3d"].Value == "1" ? " 3D" : "");
                    }



                    nCpt++;
                }

                if (nbChartInXml < nbChartByLine)
                {
                    double width = _iwidth > (minWidth * nbChartInXml) ? (nbChartInXml * minWidth) : (_iwidth * nbChartInXml) / nbChartByLine;
                    tbCharts.Style.Add(HtmlTextWriterStyle.Width, string.Concat(width.ToString(), "px"));
                }
                else
                    tbCharts.Style.Add(HtmlTextWriterStyle.Width, "100%");
            }


            return pn;
        }




        /// <summary>
        /// Construit le corps de page de l'étape de sélection des champs du rapport
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected override Panel BuildSelectFieldsPanel()
        {

            // Dossier contenant des table, et pour chaque table continet la liste des champs
            DescItem folder = new DescItem();
            // Debut DIV principale de l'etape 2
            Panel dMainContainer = new Panel();
            dMainContainer.CssClass = "template_sel chartStapClass";

            #region Type de chart, type série, display X, display Y ...

            String sTypeChart = GetReportParam(_report, "TypeChart");
            String sSeriesType = GetReportParam(_report, "SeriesType");




            String sLabel = String.Empty;

            Int32 nSelectedSerie = 0;
            if (sTypeChart.Length > 0)
                Int32.TryParse(sTypeChart.Split('|')[0], out nSelectedSerie);



            Int32 nSelectedGraph = 0;
            if (sTypeChart.Length > 0)
                Int32.TryParse(sTypeChart.Split('|')[1], out nSelectedGraph);


            bool bDisplay_Y_X = _sGraphicModelType.Equals(eModelConst.Chart.PIE) || _sGraphicModelType.Equals(eModelConst.Chart.DOUGHNUT) ? false : true;
            bool bIsSingleChart = nSelectedSerie == eModelConst.CHART_TYPE.SINGLE.GetHashCode();

            //Problème de disparité entre le modèle de chart et le type de série => modèle qui dirige
            if (bIsSingleChart && sSeriesType != "0")
                sSeriesType = "0";

            #endregion

            #region <div> Debut Logo

            HtmlGenericControl divLogo = CreateHtmlTag("div", "dynGraph histo", "", "dynGraphImageDiv");
            dMainContainer.Controls.Add(divLogo);

            HtmlGenericControl divLogoY = CreateHtmlTag("div", "nameItem histoY", "", "DynNameDiv_Y");
            divLogo.Controls.Add(divLogoY);

            HtmlGenericControl divLogoX = CreateHtmlTag("div", "valueItem histoX", "", "DynNameDiv_X");
            divLogo.Controls.Add(divLogoX);

            #endregion </div>

            #region <ul> wrappeur

            HtmlGenericControl ul = CreateHtmlTag("ul");
            ul.ID = "ulWrappeur";
            dMainContainer.Controls.Add(ul);

            #region Choix de série

            //li label choix de série et radios btn
            HtmlGenericControl liLabel = CreateHtmlTag("li", "choixSeriesLabel", String.Concat(eResApp.GetRes(Pref, 1610), " :"), "choixSeriesLabel");
            HtmlGenericControl liContent = CreateHtmlTag("li", "choixSeriesContent", "", "choixSeriesContent");
            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);


            //Bouton radio Groupement
            RadioButton rbByGroup = new RadioButton();
            rbByGroup.ID = "rdbtn_groupby";
            rbByGroup.CssClass = "rdBtnChoisSeries";
            rbByGroup.Text = eResApp.GetRes(Pref, 1612);
            rbByGroup.GroupName = "rbChoixSeries";
            rbByGroup.Checked = sSeriesType.Equals(eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_GROUP.GetHashCode().ToString());
            rbByGroup.Attributes.Add("onclick", "oReport.SetParam('seriestype', '1');UpdatePanelSelectFields();");

            //Bouton radio Champs
            RadioButton rbByField = new RadioButton();
            rbByField.ID = "rdbtn_field";
            rbByField.CssClass = "rdBtnChoisSeries";
            rbByField.Text = eResApp.GetRes(Pref, 1611);
            rbByField.GroupName = "rbChoixSeries";
            rbByField.Checked = sSeriesType.Equals(eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_VALUE.GetHashCode().ToString());
            rbByField.Attributes.Add("onclick", "oReport.SetParam('seriestype', '2');UpdatePanelSelectFields();");


            liContent.Controls.Add(rbByGroup);
            liContent.Controls.Add(rbByField);

            //On affiche pas le choix de série si le modèle est simple.
            liLabel.Style["display"] = bIsSingleChart ? "none" : "";
            liContent.Style["display"] = bIsSingleChart ? "none" : "";

            #endregion

            if (_sGraphicModelType.Equals(eModelConst.Chart.COMBINED))
                ul.Style.Add(HtmlTextWriterStyle.Display, "none");

            AppendGeneralEtiquetteX(bDisplay_Y_X, ul, folder, divLogoX, rbByGroup, rbByField, dMainContainer, bIsSingleChart);

            //Bloc spécifique au graphique combiné : caché si le graphique est différente de combiné

            if (_report.Id == 0 || sSeriesType.Equals(((int)eModelConst.CHART_SERIES_TYPE.SERIES_TYPE_COMBINED).ToString()))
            {
                bool addGaugePart = (_report.Id == 0 || _sGraphicModelType.Equals(eModelConst.Chart.CIRCULARGAUGE));
                string axeY = "Y";
                string axeZ = "Z";
                string prefixY = eLibConst.COMBINED_Y;
                string prefixZ = eLibConst.COMBINED_Z;
                string action = string.Concat("UpdateLineaireGraphiqueSelection(this,'", prefixZ, "');");

                HtmlGenericControl combinedUl = CreateHtmlTag("ul");
                combinedUl.ID = "combinedulWrappeur";
                dMainContainer.Controls.Add(combinedUl);
                if (!_sGraphicModelType.Equals(eModelConst.Chart.COMBINED))
                    combinedUl.Style.Add(HtmlTextWriterStyle.Display, "none");

                ChartSpecificParams param = new ChartSpecificParams()
                {
                    Action = action,
                    Axe = axeY,
                    Prefix = prefixY,
                    PrefixFilter = prefixY,
                    ChartType = _sGraphicModelType,
                    HideCumulFilter = _sGraphicModelType.Equals(eModelConst.Chart.CIRCULARGAUGE),
                    AddGaugePart = addGaugePart,
                    GetAllTab = addGaugePart,
                    DisabelNotLinkedFile = true
                };



                AppendCombinedEtiquette(combinedUl, divLogoX, param);
                liContent = CreateHtmlTag("li", sCssClass: "emptyLi");
                combinedUl.Controls.Add(liContent);

                if (addGaugePart)
                    AppendGaugePickerBloc(combinedUl, divLogoX);

                param = new ChartSpecificParams() { Action = string.Empty, Axe = axeZ, Prefix = prefixZ, PrefixFilter = prefixZ, AddExpressFilter = true, GetAllTab = true, HideCumulFilter = !_sGraphicModelType.Equals(eModelConst.Chart.CIRCULARGAUGE), ChartType = _sGraphicModelType, AddButtonRadio = false, AddGaugePart = addGaugePart };
                AppendCombinedEtiquette(combinedUl, divLogoX, param);
            }


            #endregion </ul>

            return dMainContainer;
        }



        /// <summary>
        /// 
        /// </summary>
        private void AppendGeneralEtiquetteX(bool bDisplay_Y_X,
            HtmlGenericControl ul,
            DescItem folder,
            HtmlGenericControl divLogoX,
            RadioButton rbByGroup,
            RadioButton rbByField,
            Panel dMainContainer,
            bool bIsSingleChart)
        {
            bool bVisible;
            string sLabel;
            HtmlGenericControl liLabel;
            HtmlGenericControl liContent;
            #region Selection la rubrique des etiquettes X
            ListItem item;
            string ddlEtiquettesGroupId = "EtiquettesGroup";
            //label de rubrique des valeurs X et le contenu 
            sLabel = String.Concat(eResApp.GetRes(Pref, 1605), bDisplay_Y_X ? " (X):" : " :");
            liLabel = CreateHtmlTag("li", "rbqValsLabel", sLabel, "rbqValsLabelX");
            liContent = CreateHtmlTag("li", "rbqValsContent", "", "rbqValsContentX");

            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);
            ChartSpecificParams wizardParam = new ChartSpecificParams();
            wizardParam.OnlyDataField = true;
            //ajout des selects etiquetefile et etiquettefield
            FillFileAndFields(liContent, "EtiquettesFile", "EtiquettesField", wizardParam, out folder, out bVisible);

            DropDownList ddlEtiquettesGroup = new DropDownList();
            ddlEtiquettesGroup.ID = ddlEtiquettesGroupId;
            ddlEtiquettesGroup.CssClass = "editor_select";

            liContent.Controls.Add(ddlEtiquettesGroup);
            String sEtiquettesGroup = GetReportParam(_report, ddlEtiquettesGroupId);

            Dictionary<String, String> diEtiquettesGroup = new Dictionary<String, String>();
            diEtiquettesGroup.Add("day", eResApp.GetRes(Pref, 822));
            diEtiquettesGroup.Add("month", eResApp.GetRes(Pref, 405));
            diEtiquettesGroup.Add("year", eResApp.GetRes(Pref, 406));
            diEtiquettesGroup.Add("weekday", eResApp.GetRes(Pref, 1702));
            diEtiquettesGroup.Add("trim", eResApp.GetRes(Pref, 1703));
            diEtiquettesGroup.Add("sem", eResApp.GetRes(Pref, 1704));

            foreach (KeyValuePair<String, String> elm in diEtiquettesGroup)
            {
                item = new ListItem(elm.Value, elm.Key);

                item.Selected = sEtiquettesGroup.Equals(elm.Key);

                ddlEtiquettesGroup.Items.Add(item);

            }

            /*Tri des valeurs catalogue à la restitution du graphique*/
            DropDownList ddlEtiquettesTri = new DropDownList();
            ddlEtiquettesTri.ID = "EtiquettesTri";
            ddlEtiquettesTri.CssClass = "editor_select";

            /*libellé Tri*/
            liContent.Controls.Add(CreateHtmlTag("label", sId: "labelEtiquettesTri", sInnerHtml: String.Concat(eResApp.GetRes(Pref, 168), " ")));

            liContent.Controls.Add(ddlEtiquettesTri);

            Dictionary<String, String> diEtiquettesTri = new Dictionary<String, String>();
            diEtiquettesTri.Add("desc", eResApp.GetRes(Pref, 159));
            diEtiquettesTri.Add("asc", eResApp.GetRes(Pref, 158));

            String sEtiquettesTri = GetReportParam(_report, "etiquettestri");

            foreach (KeyValuePair<String, String> elm in diEtiquettesTri)
            {
                item = new ListItem(elm.Value, elm.Key);

                item.Selected = sEtiquettesTri.Equals(elm.Key);

                ddlEtiquettesTri.Items.Add(item);

            }


            DropDownList ddlEtiquetteFile = (DropDownList)liContent.Controls[0];
            String sSelectedFile = ddlEtiquetteFile.SelectedValue;

            DropDownList ddlEtiquetteFields;
            Control ctl;

            String onChange = String.Concat("UpdateParamReport('valuesoperation', this.value);UpdateFirstRubriqueY();UpdateDescription();");

            for (int i = 1; i < liContent.Controls.Count; i++)
            {
                ctl = liContent.Controls[i];
                if (ctl is DropDownList)
                {
                    ddlEtiquetteFields = (DropDownList)ctl;
                    ddlEtiquetteFields.Attributes.Add("onchange", String.Concat("oReport.SetParam('etiquettesfield',this.value);DisplEtiqGroup(this,'", ddlEtiquettesGroupId, "');UpdateDescription();"));

                    if (ctl.ID == String.Concat("EtiquettesField_", sSelectedFile))
                    {
                        ListItem itmSelFld = ddlEtiquetteFields.SelectedItem;

                        if (itmSelFld.Attributes["fmt"] != FieldFormat.TYP_DATE.GetHashCode().ToString())
                        {
                            ddlEtiquettesGroup.Style.Add("display", "none");
                        }


                        divLogoX.InnerHtml = HttpUtility.HtmlEncode(ddlEtiquetteFields.SelectedItem.Text);
                    }
                    else
                    {
                        //On sélectionne le premier par defaut
                        if (ddlEtiquetteFields.Items.Count > 0 && ddlEtiquetteFields.SelectedItem == null)
                        {
                            ddlEtiquetteFields.Items[0].Selected = true;
                        }

                    }


                }

            }

            ddlEtiquettesGroup.Attributes.Add("onchange", String.Concat("oReport.SetParam('etiquettesgroup',this.value)"));
            ddlEtiquettesTri.Attributes.Add("onchange", String.Concat("oReport.SetParam('EtiquettesTri',this.value)"));
            #endregion

            #region Sélectionnez la rubrique de regroupement des séries (X)

            // label de rubrique des series et le contenu 
            liLabel = CreateHtmlTag("li", "rbqValsLabel", eResApp.GetRes(Pref, 1606), "rbqSeriesLabel");
            liContent = CreateHtmlTag("li", "rbqValsContent", "", "rbqSeriesContent");

            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);

            wizardParam.OnlyDataField = false;
            //ajout des selects seriesfile et seriesfield
            FillFileAndFields(liContent, "SeriesFile", "SeriesField", wizardParam, out folder, out bVisible);

            liLabel.Style["display"] = (bIsSingleChart || !rbByGroup.Checked) ? "none" : "";
            liContent.Style["display"] = (bIsSingleChart || !rbByGroup.Checked) ? "none" : "";

            #endregion

            #region Selection rubrique des valeurs Y

            // label de rubrique des valeur Y et le contenu 
            sLabel = String.Concat(eResApp.GetRes(Pref, 1604), bDisplay_Y_X ? " (Y):" : " :");
            liLabel = CreateHtmlTag("li", "rbqValsLabel", sLabel, "rbqValsLabelY");
            liContent = CreateHtmlTag("li", "rbqValsContent", "", "rbqValsContentY");

            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);

            //Contenu
            FillFileAndFields(liContent, "ValuesFile", "ValuesField", wizardParam, out folder, out bVisible);
            RenderAgregateFuncionsIntoContainer(liContent, GetReportParam(_report, "ValuesOperation").Split(';')[0], onChange);


            #endregion

            #region li Multi Lines et Add button en cas choix de serie par champs

            HtmlGenericControl liMultiLines = CreateHtmlTag("div", "liAddLine", "", "liMultiLines");
            ul.Controls.Add(liMultiLines);

            HtmlGenericControl liAddButton = CreateHtmlTag("li", "liAddLine", "", "liAddBtn");
            ul.Controls.Add(liAddButton);

            HtmlGenericControl divAdd = CreateHtmlTag("div", "logoAddLine", eResApp.GetRes(Pref, 18));
            liAddButton.Controls.Add(divAdd);

            divAdd.Attributes["onclick"] = "InsertRubriqueY(0);";

            if (bIsSingleChart || !rbByField.Checked)
            {
                liMultiLines.Style["display"] = "none";
                liAddButton.Style["display"] = "none";
            }
            else
            {
                RenderMultiLine(liMultiLines, folder);
            }

            #endregion

            #region Description

            //Champs caché pour se sauvegarder des RES
            HiddenField hfRes = new HiddenField();

            hfRes.ID = "hf_res_description";
            hfRes.Value = String.Concat("@operation ", eResApp.GetRes(Pref, 5085).ToLower(),
                                        " [@valuesfield](@valuesfile) ", eResApp.GetRes(Pref, 60).ToLower(),
                                        " [@etiquettesfield](@etiquettesfile) ", eResApp.GetRes(Pref, 1616).ToLower(),
                                        " [@seriesfield](@seriesfile)");

            //label de description et le contenu
            liLabel = CreateHtmlTag("li", "descLabel", String.Concat(eResApp.GetRes(Pref, 104), " :"), "rbqDescLabel");
            liContent = CreateHtmlTag("li", "descContent", hfRes.Value, "rbqDescContent");

            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);

            //Champs caché pour se sauvegarder des RES
            dMainContainer.Controls.Add(hfRes);


            liLabel.Style["display"] = (bIsSingleChart || !rbByGroup.Checked) ? "none" : "";
            liContent.Style["display"] = (bIsSingleChart || !rbByGroup.Checked) ? "none" : "";

            #endregion

            #region Associer un filtre, Cumuler le filtre en cours

            //label de filtre et le contenu
            liLabel = CreateHtmlTag("li", "filterLabel", String.Concat(eResApp.GetRes(Pref, 1037), " :"));
            liContent = CreateHtmlTag("li", "selectFilter", sId: "selectFilter");

            ul.Controls.Add(liLabel);
            ul.Controls.Add(liContent);

            //ajout d un select de filtres
            try
            {
                eReportChartFilterWizardRenderer filter = new eReportChartFilterWizardRenderer(Pref, Report, _iTab);
                filter.FillFilter(liContent);
            }
            catch (Exception e)
            {
                _sErrorMsg = e.Message;
                _eException = e.InnerException;
            }

            //FillFilter(liContent);

            //Cumuler les fitre en cours
            liContent = CreateHtmlTag("li", "cumulerFilter", sId: "cumulerFilter");

            ul.Controls.Add(liContent);


            bool bChecked = GetReportParam(_report, "addcurrentfilter").Equals("0") ? false : true;
            eCheckBoxCtrl chkFilter = new eCheckBoxCtrl(bChecked, false);
            chkFilter.ID = "addCurrentFilter";
            chkFilter.AddClick("oReport.SetParam('addcurrentfilter', this.attributes[\"chk\"].value);");
            chkFilter.AddText(eResApp.GetRes(Pref, 1038));    //Actives

            liContent.Controls.Add(chkFilter);

            #endregion

            #region filtres express

            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.GraphExpressFilter))
            {
                listExpressFilter(_report.Id, ul, folder, wizardParam, bVisible);
            }

            #endregion
        }

        /// <summary>
        /// Ajout du block spécifique pour la série spéciale(combiné et jauge)
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="divLogoX"></param>
        /// <param name="param"></param>
        private void AppendCombinedEtiquette(HtmlGenericControl ul, HtmlGenericControl divLogoX, ChartSpecificParams param)
        {
            string sLabel;
            bool bVisible;
            DescItem folder = new DescItem();
            HtmlGenericControl liLabel;
            HtmlGenericControl liContent;
            ListItem item;
            Label label;
            string ddlEtiquettesFileId = "EtiquettesFile";
            string ddlEtiquettesFieldId = "EtiquettesField";
            string ddlEtiquettesGroupId = "EtiquettesGroup";
            string valuesFileId = "ValuesFile";
            string valuesFieldId = "ValuesField";
            string sFilterCssClass = "padEtiqFilter";
            int cgvType = 0;
            string cgType = _report.GetParamValue("cgvtype");
            if (string.IsNullOrEmpty(cgType))
                cgType = ((int)CIRCULAR_GAUGE_VALUE_TYPE.DYNAMIC_VALUE).ToString();

            bool bGetEtiquetteX = _report.Id == 0 || param.ChartType == eModelConst.Chart.COMBINED || param.ChartType == eModelConst.Chart.CIRCULARGAUGE;
            bool checkFixedValue = _report.Id == 0 || cgType == ((int)CIRCULAR_GAUGE_VALUE_TYPE.FIXED_VALUE).ToString();

            if (!Int32.TryParse(cgType, out cgvType) || (!checkFixedValue && cgvType > 1))
                checkFixedValue = true;


            bool checkBtn = !checkFixedValue && cgType == ((int)CIRCULAR_GAUGE_VALUE_TYPE.DYNAMIC_VALUE).ToString();

            #region Selection la rubrique des etiquettes X

            String onChange = String.Concat("UpdateParamReport('", param.Prefix.ToLower(), "valuesoperation', this.value,'", param.Prefix, "');UpdateFirstRubriqueY('", param.Prefix, "');UpdateDescription('", param.Prefix, "');");

            if (bGetEtiquetteX)
            {
                //label de rubrique des valeurs X et le contenu 
                sLabel = String.Format(eResApp.GetRes(Pref, 8520), eResApp.GetRes(Pref, param.Axe.ToLower().Equals("y") ? 1567 : 1569).ToLower());
                liLabel = CreateHtmlTag("li", "rbqValsLabel", sLabel, string.Concat(param.Prefix, "rbqValsLabel"));
                liLabel.Attributes.Add("ctype", "gauge");
                liLabel.Attributes.Add("gauge", param.Prefix.ToLower());
                ul.Controls.Add(liLabel);

                if (param.AddGaugePart && param.AddButtonRadio)
                {
                    string sValue = _report.GetParamValue("cgvfixedvalue");
                    int iVal = 1;
                    //la valeur fixe doit être supérieur à 0;
                    if (int.TryParse(sValue, out iVal) && iVal <= 0)
                        iVal = 1;

                    liContent = CreateHtmlTag("li", sCssClass: "rbqValsContent", sId: "rbqValsContentY");
                    ul.Controls.Add(liContent);

                    HtmlInputRadioButton radioBtn = new HtmlInputRadioButton();
                    radioBtn.Name = "ValueType";
                    radioBtn.Value = ((int)CIRCULAR_GAUGE_VALUE_TYPE.FIXED_VALUE).ToString();
                    radioBtn.ID = string.Concat("ValueType", ((int)CIRCULAR_GAUGE_VALUE_TYPE.FIXED_VALUE).ToString());
                    radioBtn.Checked = checkFixedValue;
                    radioBtn.Attributes.Add("class", "btnValue");
                    radioBtn.Attributes.Add("onclick", string.Concat("oReport.SetParam('cgvtype','", (int)CIRCULAR_GAUGE_VALUE_TYPE.FIXED_VALUE, "');"));
                    liContent.Controls.Add(radioBtn);

                    label = new Label();
                    label.Text = eResApp.GetRes(Pref, 1891);
                    label.CssClass = "btnValue";
                    label.Attributes.Add("for", "ValueType");
                    liContent.Controls.Add(label);

                    HtmlGenericControl txtValue = new HtmlGenericControl("input");
                    liContent.Controls.Add(txtValue);
                    txtValue.ID = "txtGaugeValue";
                    txtValue.Attributes.Add("type", "number");
                    txtValue.Attributes.Add("onchange", "oReport.SetParam('cgvfixedvalue',this.value)");
                    txtValue.Attributes.Add("class", "txtGaugeFixedValue");
                    txtValue.Attributes.Add("min", "1");
                    if (checkFixedValue && iVal > 0)
                        txtValue.Attributes.Add("value", iVal.ToString());
                }

                //Si c'est un graphique type circulaire, on affiche pas le block CombinedYrbqValsContentX
                if ((param.ChartType != eModelConst.Chart.CIRCULARGAUGE || _report.Id == 0))
                {

                    liContent = CreateHtmlTag("li", sCssClass: "rbqValsContent", sId: string.Concat(param.Prefix, "rbqValsContentX"));
                    ul.Controls.Add(liContent);
                    /*libellé Etiquette X*/
                    liContent.Controls.Add(CreateHtmlTag("label", sId: string.Concat(param.Prefix, "labelEtiquettesCombinedX"),
                        sInnerHtml: String.Concat(eResApp.GetRes(Pref, 7293), " (X) ")));
                    param.GetLinkedTabOnly = true;
                    //ajout des selects etiquetefile et etiquettefield
                    FillFileAndFields(liContent, ddlEtiquettesFileId, ddlEtiquettesFieldId, param, out folder, out bVisible);


                    if (param.ChartType != eModelConst.Chart.CIRCULARGAUGE || _report.Id == 0)
                    {
                        DropDownList ddlEtiquettesGroup = new DropDownList();
                        ddlEtiquettesGroup.ID = string.Concat(param.Prefix, ddlEtiquettesGroupId);
                        ddlEtiquettesGroup.CssClass = "editor_select";

                        liContent.Controls.Add(ddlEtiquettesGroup);

                        String sEtiquettesGroup = GetReportParam(_report, string.Concat(param.Prefix, ddlEtiquettesGroupId));

                        Dictionary<String, String> diEtiquettesGroup = new Dictionary<String, String>();
                        diEtiquettesGroup.Add("day", eResApp.GetRes(Pref, 822));
                        diEtiquettesGroup.Add("month", eResApp.GetRes(Pref, 405));
                        diEtiquettesGroup.Add("year", eResApp.GetRes(Pref, 406));
                        diEtiquettesGroup.Add("weekday", eResApp.GetRes(Pref, 1702));
                        diEtiquettesGroup.Add("trim", eResApp.GetRes(Pref, 1703));
                        diEtiquettesGroup.Add("sem", eResApp.GetRes(Pref, 1704));

                        foreach (KeyValuePair<String, String> elm in diEtiquettesGroup)
                        {
                            item = new ListItem(elm.Value, elm.Key);

                            item.Selected = sEtiquettesGroup.Equals(elm.Key);

                            ddlEtiquettesGroup.Items.Add(item);
                        }


                        DropDownList ddlEtiquetteFile = (DropDownList)liContent.Controls[1];
                        String sSelectedFile = ddlEtiquetteFile.SelectedValue;

                        DropDownList ddlEtiquetteFields;
                        Control ctl;

                        for (int i = 1; i < liContent.Controls.Count; i++)
                        {
                            ctl = liContent.Controls[i];
                            if (ctl.ID == string.Concat(param.Prefix, ddlEtiquettesFileId))
                                continue;
                            if (ctl is DropDownList)
                            {
                                ddlEtiquetteFields = (DropDownList)ctl;
                                ddlEtiquetteFields.Attributes.Add("onchange", String.Concat(param.Action, " oReport.SetParam('", param.Prefix.ToLower(), ddlEtiquettesFieldId.ToLower(), "',this.value);DisplEtiqGroup(this,'", param.Prefix, ddlEtiquettesGroupId, "');UpdateDescription('", param.Prefix, "');"));

                                //SHA : correction bug 71 583
                                if (ddlEtiquetteFields.Items.Count == 0)
                                    continue;

                                ddlEtiquetteFields.Attributes.Add("selectedValue", ddlEtiquetteFields.SelectedItem.Value);
                                if (ctl.ID == String.Concat(param.Prefix, ddlEtiquettesFieldId, "_", sSelectedFile))
                                {
                                    ListItem itmSelFld = ddlEtiquetteFields.SelectedItem;

                                    if (itmSelFld.Attributes["fmt"] != FieldFormat.TYP_DATE.GetHashCode().ToString())
                                    {
                                        ddlEtiquettesGroup.Style.Add("display", "none");
                                    }

                                    divLogoX.InnerHtml = HttpUtility.HtmlEncode(ddlEtiquetteFields.SelectedItem.Text);
                                }
                                else
                                {
                                    //On sélectionne le premier par defaut
                                    if (ddlEtiquetteFields.Items.Count > 0 && ddlEtiquetteFields.SelectedItem == null)
                                    {
                                        ddlEtiquetteFields.Items[0].Selected = true;
                                    }
                                }
                            }

                        }

                        string onDdlGroupChange = String.Concat("oReport.SetParam('", param.Prefix.ToLower(), ddlEtiquettesGroupId.ToLower(), "',this.value)");

                        if (param.Prefix == eLibConst.COMBINED_Y)
                            onDdlGroupChange = string.Concat("UpdateSelectedGoup(this,'", string.Concat(eLibConst.COMBINED_Z, ddlEtiquettesGroupId), "');", onDdlGroupChange);

                        ddlEtiquettesGroup.Attributes.Add("onchange", onDdlGroupChange);
                        //Désactiver la liste pour le graphique combiné lineaire qui reprondra la même valeur que l'histogramme
                        if (param.Prefix == eLibConst.COMBINED_Z)
                            ddlEtiquettesGroup.Attributes.Add("disabled", "disabled");
                    }
                }


            }

            #endregion

            #region Selection rubrique des valeurs Y
            //Contenu

            liContent = CreateHtmlTag("li", sCssClass: "rbqValsContent", sId: string.Concat(param.Prefix, "rbqValsContentY"));

            ul.Controls.Add(liContent);

            /*libellé valeur Y*/
            //Si c'est graphique n'est pas de type jauge ou en cas de création de graphique => on affiche le libéllé, sinon le libéllé est généré dynamiquement
            if (param.ChartType != eModelConst.Chart.CIRCULARGAUGE || _report.Id == 0)
            {
                liContent.Controls.Add(CreateHtmlTag("label", sCssClass: "padEtiqY", sId: string.Concat(param.Prefix, "labelEtiquettesY"),
                sInnerHtml: String.Concat(eResApp.GetRes(Pref, 6828), " (", param.Axe, ") ")));
            }

            if (param.AddGaugePart)
            {
                if (param.AddButtonRadio)
                {

                    HtmlInputRadioButton radioBtn = new HtmlInputRadioButton();
                    radioBtn.Name = "ValueType";
                    radioBtn.Value = ((int)CIRCULAR_GAUGE_VALUE_TYPE.DYNAMIC_VALUE).ToString();
                    radioBtn.ID = String.Concat("ValueType", ((int)CIRCULAR_GAUGE_VALUE_TYPE.DYNAMIC_VALUE).ToString());
                    radioBtn.Checked = checkBtn;
                    radioBtn.Attributes.Add("class", "btnValue");
                    radioBtn.Attributes.Add("onchange", string.Concat("oReport.SetParam('cgvtype','", ((int)CIRCULAR_GAUGE_VALUE_TYPE.DYNAMIC_VALUE).ToString(), "');"));
                    liContent.Controls.Add(radioBtn);
                }

                label = new Label();
                label.Text = eResApp.GetRes(Pref, 6828);
                label.CssClass = "btnValue";
                label.Attributes.Add("for", "ValueType");
                liContent.Controls.Add(label);
            }



            Int32 nFiltredCombinedZFileTab = 0;
            if (param.Prefix == eLibConst.COMBINED_Z)
            {

                if (param.ChartType == eModelConst.Chart.CIRCULARGAUGE && _report.Id != 0)
                {
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.Prefix, "valuesfile")), out nFiltredCombinedZFileTab);
                    param.GetAllTab = false;
                }

                else
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.Prefix, "etiquettesfile")), out nFiltredCombinedZFileTab);
            }
            //else
            //{
            //    if ((param.ChartType == eModelConst.Chart.CIRCULARGAUGE && _report.Id > 0) || (_report.Id == 0 && valuesFileId.ToLower() == "valuesfile"))
            //        param.GetAllTab = false;
            //}


            param.CobinedFilterdTabId = nFiltredCombinedZFileTab;
            param.GetLinkedTabOnly = false;
            FillFileAndFields(liContent, valuesFileId, valuesFieldId, param, out folder, out bVisible);
            RenderAgregateFuncionsIntoContainer(liContent, GetReportParam(_report, string.Concat(param.Prefix.ToLower(), "ValuesOperation")).Split(';')[0], onChange, param.Prefix);

            #endregion

            #region Associer un filtre, Cumuler le filtre en cours

            //label de filtre et le contenu
            liContent = CreateHtmlTag("li", sCssClass: "rbqValsContent", sId: string.Concat(param.Prefix, "SelectFilter"));
            if (param.Prefix == eLibConst.COMBINED_Z)
                sFilterCssClass = string.Empty;
            ul.Controls.Add(liContent);
            liContent.Controls.Add(CreateHtmlTag("label", sCssClass: sFilterCssClass, sInnerHtml: String.Concat(eResApp.GetRes(Pref, 182), " ")));

            string seletedTab = _report.Id == 0 ? _iTab.ToString() : GetReportParam(_report, string.Concat(param.Prefix, valuesFileId));
            //ajout d un select de filtres
            //FillFilter(liContent, param.Prefix, seletedTab: param.Prefix == eLibConst.COMBINED_Z ? seletedTab : string.Empty);
            try
            {
                eReportChartFilterWizardRenderer filter = new eReportChartFilterWizardRenderer(Pref, Report, int.Parse(seletedTab), param.Prefix);
                filter.FillFilter(liContent);
            }
            catch (Exception e)
            {
                _sErrorMsg = e.Message;
                _eException = e.InnerException;
            }

            //Ajouter la case à cocher Cumuler avec le filtre en cours seuleemnt si on est en creation ou le graphique est différents de jauge 
            if (param.ChartType != eModelConst.Chart.CIRCULARGAUGE || _report.Id == 0
                || (param.ChartType == eModelConst.Chart.CIRCULARGAUGE && param.Prefix == eLibConst.COMBINED_Z))
            {
                //Cumuler les fitre en cours
                liContent = CreateHtmlTag("li", string.Concat(param.Prefix, "CumulerFilter"), sId: string.Concat(param.Prefix, "cumulerFilter"));

                if (param.HideCumulFilter)
                    liContent.Style.Add(HtmlTextWriterStyle.Display, "none");
                ul.Controls.Add(liContent);

                if (param.AddFilter)
                {
                    bool bChecked = GetReportParam(_report, string.Concat(param.Prefix.ToLower(), "addcurrentfilter")).Equals("0") ? false : true;
                    eCheckBoxCtrl chkFilter = new eCheckBoxCtrl(bChecked, false);
                    chkFilter.ID = string.Concat(param.Prefix, "addCurrentFilter");
                    chkFilter.AddClick(string.Concat("oReport.SetParam('", param.Prefix.ToLower(), "addcurrentfilter", "', this.attributes[\"chk\"].value);"));
                    chkFilter.AddText(eResApp.GetRes(Pref, 1038));    //Actives

                    liContent.Controls.Add(chkFilter);
                }
            }

            #endregion

            if (param.AddExpressFilter)
            {
                #region filtres express
                this._bSpecialFilterForGraphique = true;
                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.GraphExpressFilter))
                {
                    SetListExpressFilter(_report.Id, ul, folder, param, bVisible, bcombineFilter: true);
                }

                #endregion
            }


        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="divLogoX"></param>
        private void AppendGaugePickerBloc(HtmlGenericControl ul, HtmlGenericControl divLogoX)
        {
            bool bFull = false;
            bool hideAddButton = false;
            HtmlGenericControl liContent;
            ColorPickerProprieties ColorPickerProp;
            ColorPickerList = new List<ColorPickerProprieties>();

            liContent = CreateHtmlTag("li", "rbqValsLabel rbqValsLabelGauge", eResApp.GetRes(Pref, 1877), "rbqValsLabel");
            liContent.Attributes.Add("cType", "gauge");
            ul.Controls.Add(liContent);

            AppendMinGaugePicker(ul);
            Int32 invalidParam = 0;
            Int32 nbInterval = 0;
            Int32.TryParse(_report.GetParamValue("cgnbinterval"), out nbInterval);
            string[] intervalsTab = _report.GetParamValue("cgintervals").Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (intervalsTab.Length != nbInterval)
                nbInterval = intervalsTab.Length;

            for (int i = 0; i < nbInterval; i++)
            {
                ColorPickerProp = new ColorPickerProprieties();
                int val = 0;
                string[] valueTab = intervalsTab[i].Split(new char[] { ',' });
                if (valueTab.Length == 2)
                {
                    if (Int32.TryParse(valueTab[0], out val) && val > 0)
                    {
                        ColorPickerProp.Value = val;

                        bFull = val == eLibConst.MAX_INTERVAL_VALUE_FOR_GAUGE_CHART;
                        hideAddButton = val >= eLibConst.MAX_INTERVAL_VALUE_FOR_GAUGE_CHART - 1;
                        if (!valueTab[1].Contains("#"))
                            valueTab[1] = eLibConst.DAFAULT_COLOR_FOR_GAUGE_CHART_INTERVAL;
                        ColorPickerProp.Color = valueTab[1];
                        ColorPickerProp.Index = i - invalidParam;
                        ColorPickerList.Add(ColorPickerProp);
                    }
                    else
                        invalidParam++;
                }
                else
                    invalidParam++;
            }
            if (!bFull)
            {
                ColorPickerProp = new ColorPickerProprieties();
                ColorPickerProp.Index = nbInterval - invalidParam;
                ColorPickerProp.Value = eLibConst.MAX_INTERVAL_VALUE_FOR_GAUGE_CHART;
                ColorPickerList.Add(ColorPickerProp);
            }

            AppendGaugePicker(ul);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ul"></param>

        private void AppendGaugePicker(HtmlGenericControl ul)
        {
            HtmlGenericControl liContent;
            Label label;
            HtmlGenericControl txtValue;
            HtmlGenericControl divAdd;

            bool hideBtnAdd = ColorPickerList.Where(c => c.Value == 99).Count() > 0;

            foreach (ColorPickerProprieties colorPickerProp in ColorPickerList)
            {
                liContent = CreateHtmlTag("li", sId: "rbqValsLabel_" + colorPickerProp.Index, sCssClass: "rbqValsLabelGauge");
                liContent.Attributes.Add("cType", "gauge");
                liContent.Attributes.Add("index", colorPickerProp.Index.ToString());
                ul.Controls.Add(liContent);

                label = new Label();
                label.CssClass = "gaugeColorLabel";
                liContent.Controls.Add(label);

                txtValue = new HtmlGenericControl("input");
                txtValue.InnerText = "%";
                liContent.Controls.Add(txtValue);
                txtValue.ID = string.Concat("txtGaugeValue_", colorPickerProp.Index);
                txtValue.Attributes.Add("type", "number");
                txtValue.Attributes.Add("min", "0");
                txtValue.Attributes.Add("max", "100");
                txtValue.Attributes.Add("class", "txtGaugeValue");
                txtValue.Attributes.Add("onchange", "CheckIntervalMinMaxValue(this);");
                txtValue.Attributes.Add("value", colorPickerProp.Value.ToString());

                liContent.Controls.Add(BuildColorPicker("gaugeColorPicker_", "txtColorPicker_", colorPickerProp.Color.ToLower(), colorPickerProp.Index));

                if (colorPickerProp.BmaxValue)
                {
                    txtValue.Disabled = true;

                    hideBtnAdd = hideBtnAdd
                       ||
                       (!hideBtnAdd && ColorPickerList.Count < eLibConst.MAX_INTERVAL_FOR_GAUGE_CHART)
                       || (colorPickerProp.AddBtn && ColorPickerList.Count == eLibConst.MAX_INTERVAL_FOR_GAUGE_CHART + 1);

                    if (ColorPickerList.Count == 1)
                        hideBtnAdd = false;

                    divAdd = CreateHtmlTag("div", string.Concat(colorPickerProp.BtnClass, hideBtnAdd ? " logoGaugeAddLineDisabled" : ""), eResApp.GetRes(Pref, 18));
                    divAdd.Attributes.Add("onclick", "AddGaugePicker(this.parentElement," + eLibConst.MAX_INTERVAL_FOR_GAUGE_CHART + ");");
                }
                else
                {
                    divAdd = CreateHtmlTag("div", "logoGaugeDeleteLine");
                    divAdd.Attributes.Add("onclick", "DeleteGaugePicker(this.parentElement);");
                }

                liContent.Controls.Add(divAdd);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ul"></param>
        private void AppendMinGaugePicker(HtmlGenericControl ul)
        {
            HtmlGenericControl liContent;
            Label label;
            HtmlGenericControl txtValue;

            liContent = CreateHtmlTag("li", sId: "rbqValsLabel", sCssClass: "rbqValsLabelGauge");
            liContent.Attributes.Add("cType", "gauge");
            liContent.Attributes.Add("minPercent", "1");
            ul.Controls.Add(liContent);

            label = new Label();
            label.Text = eResApp.GetRes(Pref, 6085);
            label.CssClass = "gaugeColorLabel";
            liContent.Controls.Add(label);

            txtValue = new HtmlGenericControl("input");
            txtValue.InnerText = "%";
            liContent.Controls.Add(txtValue);
            txtValue.ID = "txtGaugeMinValue";
            txtValue.Attributes.Add("type", "number");
            txtValue.Attributes.Add("min", "0");
            txtValue.Attributes.Add("max", "100");
            txtValue.Attributes.Add("class", "txtGaugeValue");
            txtValue.Attributes.Add("value", "0");
            txtValue.Disabled = true;
        }


        /// <summary>
        /// Avec un rendu d un drop list avec les attributs en params
        /// </summary>
        /// <param name="dropList"></param>
        /// <param name="id"></param>
        /// <param name="cssClass"></param>
        /// <param name="onChange"></param>
        /// <returns></returns>
        private void AppendAttributeToDropDownList(DropDownList dropList, String id, String cssClass, String onChange)
        {
            dropList.ID = id;
            dropList.CssClass = cssClass;
            dropList.Attributes.Add("onchange", onChange);
        }



        /// <summary>
        /// Construit le corps de page de l'étape de choix de format
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected override Panel BuildFormatPanel()
        {
            Panel pn = new Panel();
            pn.CssClass = "template_sel chartStapClass";


            #region <div> Debut Chart Logo

            HtmlGenericControl divLogo = CreateHtmlTag("div", "dynGraph histo", "", "dynGraphImageFormat");
            pn.Controls.Add(divLogo);

            HtmlGenericControl divLogoY = CreateHtmlTag("div", "nameItem histoY", "Exemple axe Y", "DynNameDivBis_Y");//TODO nombre de fiche est dynamique
            divLogo.Controls.Add(divLogoY);

            HtmlGenericControl divLogoX = CreateHtmlTag("div", "valueItem histoX", "Exemple axe X", "DynNameDivBis_X"); //TODO user hotcom change dynamiqement
            divLogo.Controls.Add(divLogoX);


            #endregion </div>

            #region <ul>

            HtmlGenericControl ul = CreateHtmlTag("ul");
            ul.ID = "ulWrap";
            pn.Controls.Add(ul);

            #region Saisissez le titre du graphique

            HtmlGenericControl liLabel = CreateHtmlTag("li", "titreChrt", eResApp.GetRes(Pref, 6361));
            ul.Controls.Add(liLabel);

            HtmlGenericControl liContent = CreateHtmlTag("li", "optionsVals titreChrtCont");
            ul.Controls.Add(liContent);

            HtmlInputText inpTitle = new HtmlInputText();
            liContent.Controls.Add(inpTitle);
            inpTitle.ID = "Title";
            inpTitle.Attributes.Add("class", "chartTitleClass");
            inpTitle.Attributes.Add("onchange", "oReport.SetParam('title', this.value);");
            inpTitle.Value = GetReportParam(_report, "Title");

            #endregion

            #region Options d'affichage

            //le libellé  
            liLabel = CreateHtmlTag("li", "titreChrt", string.Concat(eResApp.GetRes(Pref, 6045), " :"));
            ul.Controls.Add(liLabel);

            /***
             * 
             * les options
             * 
             * **/


            // Option "Afficher les valeurs"
            liContent = CreateHtmlTag("li", "optionsVals");
            ul.Controls.Add(liContent);


            Boolean bChked = GetReportParam(_report, "DisplayValues") == "1";
            Boolean bNotDisplayEtiquettes = (_sGraphicModelType == eModelConst.Chart.PIE
                || _sGraphicModelType == eModelConst.Chart.SEMIPIE
                || _sGraphicModelType == eModelConst.Chart.DOUGHNUT
                || _sGraphicModelType == eModelConst.Chart.SEMIDOUGHNUT
                || _sGraphicModelType == eModelConst.Chart.FUNNEL
                || _sGraphicModelType == eModelConst.Chart.PYRAMID
                );


            eCheckBoxCtrl chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "DispValues";
            chkBox.AddClick("oReport.SetParam('displayvalues',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 1607));


            //Récupère type de serie et type de graphique 
            String sTypeChart = GetReportParam(_report, "TypeChart");
            Int32 nSelectedSerie = 1;
            Int32 nSelectedChart = 0;
            if (sTypeChart.Length > 0)
            {
                Int32.TryParse(sTypeChart.Split('|')[0], out nSelectedSerie);
                Int32.TryParse(sTypeChart.Split('|')[1], out nSelectedChart);
            }


            //valeurs en pourcentage
            bChked = GetReportParam(_report, "DisplayValuesPercent") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "DispValPct";
            chkBox.AddClick("oReport.SetParam('displayvaluespercent',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 6060));
            chkBox.Style.Add("display", "none");
            if (_sGraphicModelType == eModelConst.Chart.PIE
                || _sGraphicModelType == eModelConst.Chart.SEMIPIE
                || _sGraphicModelType == eModelConst.Chart.DOUGHNUT
                || _sGraphicModelType == eModelConst.Chart.SEMIDOUGHNUT
                || _sGraphicModelType == eModelConst.Chart.FUNNEL
                || _sGraphicModelType == eModelConst.Chart.PYRAMID
                || (nSelectedSerie == eModelConst.CHART_TYPE.STACKED.GetHashCode() && (_sGraphicModelType == eModelConst.Chart.BAR || (_sGraphicModelType == eModelConst.Chart.COLUMN)))
                )
            {
                chkBox.Style.Remove("display");
            }


            //series empilées en pourcentage
            bChked = GetReportParam(_report, "DisplayStackedPercent") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "DispStckPct";
            chkBox.AddClick("oReport.SetParam('displaystackedpercent',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 6922));

            chkBox.Style.Add("display", "none");
            if (nSelectedSerie == eModelConst.CHART_TYPE.STACKED.GetHashCode() && (_sGraphicModelType == eModelConst.Chart.BAR || (_sGraphicModelType == eModelConst.Chart.COLUMN)))
            {
                chkBox.Style.Remove("display");
            }



            //Utiliser la couleur du thème
            liContent = CreateHtmlTag("li", "optionsVals");
            liContent.ID = "LiUseThemeColor";
            ul.Controls.Add(liContent);
            bChked = GetReportParam(_report, "useThemeColor") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "UseThemeColor";
            chkBox.AddClick("oReport.SetParam('useThemeColor',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 8188));
            liContent.Style.Add("display", "none");


            if (((_sGraphicModelType == eModelConst.Chart.COLUMN
                || _sGraphicModelType == eModelConst.Chart.BAR)
                && nSelectedSerie == (int)eModelConst.CHART_TYPE.SINGLE) || nSelectedSerie == (int)eModelConst.CHART_TYPE.SPECIAL
                )

            {
                liContent.Style.Remove("display");
            }


            // Afficher la grille des valeurs
            liContent = CreateHtmlTag("li", "optionsVals");
            liContent.ID = "chartDisplayGrid";
            ul.Controls.Add(liContent);

            bChked = GetReportParam(_report, "displaygrid") == "1";

            //-------------- MCR 39206 : ne pas afficher la grille des valeurs dans le cas de serie multiples ou empiles (elle est vide! )
            //                           decocher la checkbox "DispGrid" en modification du graphique            
            if (nSelectedSerie != (int)eModelConst.CHART_TYPE.SINGLE && nSelectedSerie != (int)eModelConst.CHART_TYPE.SPECIAL)
            {  // si ce n'est pas une serie simple alors decocher la checkbox "DispGrid" (ne pas afficher les valeurs, bug: c est toujours vide!)
                bChked = false;
                _report.SetParamValue("displaygrid", "0");
            }
            //-------------- fin MCR 39206 -------------------------------------

            //Option afficher la grille de valeurs
            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "DispGrid";
            chkBox.AddClick("oReport.SetParam('displaygrid',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 6059));


            HtmlGenericControl span = CreateHtmlTag("span", "spanNbLigne", eResApp.GetRes(Pref, 6373));
            liContent.Controls.Add(span);
            span.Style.Add("display", "none");
            HtmlInputText inpGridNb = new HtmlInputText();
            liContent.Controls.Add(inpGridNb);
            inpGridNb.ID = "DispGridNb";
            inpGridNb.Value = GetReportParam(_report, "DisplayGridNb");
            inpGridNb.Attributes.Add("onchange", "oReport.SetParam('displaygridnb', this.value);");
            inpGridNb.Attributes.Add("class", "editor_GridNb");
            inpGridNb.Style.Add("display", "none");
            //-------------- MCR 39206 : ne pas afficher la grille des valeurs dans le cas de serie multiples ou empiles (elle est vide! )
            //                           ne pas proposer en creation ou modification de graphique, cocher la checkbox

            if (nSelectedSerie != (int)eModelConst.CHART_TYPE.SINGLE && nSelectedSerie != (int)eModelConst.CHART_TYPE.SPECIAL)
            {
                // sph : on masque la ligne complète
                liContent.Style.Add("display", "none");

            }
            //-------------- fin MCR 39206 -------------------------------------

            //BSE:#64 664
            //Option Ajuster l'echelle pour le graphique combiné
            liContent = CreateHtmlTag("li", "optionsVals");
            liContent.ID = "chartDisplayZaxe";
            ul.Controls.Add(liContent);

            bChked = GetReportParam(_report, "displayzaxe") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "DisplayZaxe";
            chkBox.AddClick("oReport.SetParam('displayzaxe',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 498));

            if (_report.Id == 0)
                liContent.Style.Add("display", "none");

            //TRI
            liContent = CreateHtmlTag("li", "optionsVals", "", "Sort");
            ul.Controls.Add(liContent);

            bChked = GetReportParam(_report, "SortEnabled") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "SortBox";
            chkBox.CssClass = String.Concat(chkBox.CssClass, " SortChk");
            chkBox.AddClick("oReport.SetParam('sortenabled',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 168));


            DropDownList ddlSortOrder = new DropDownList();
            ddlSortOrder.ID = "SortOrder";
            ddlSortOrder.CssClass = "SortDrop";
            liContent.Controls.Add(ddlSortOrder);

            ddlSortOrder.Items.Add(new ListItem("-", ""));
            ddlSortOrder.Items.Add(new ListItem(eResApp.GetRes(Pref, 158), "ASC"));
            ddlSortOrder.Items.Add(new ListItem(eResApp.GetRes(Pref, 159), "DESC"));

            switch (GetReportParam(_report, "SortOrder").ToUpper())
            {
                case "":
                    ddlSortOrder.Items[0].Selected = true;
                    break;
                case "ASC":
                    ddlSortOrder.Items[1].Selected = true;
                    break;
                case "DESC":
                    ddlSortOrder.Items[2].Selected = true;
                    break;
                default:
                    break;
            }


            ddlSortOrder.Attributes.Add("onchange", "oReport.SetParam('sortorder', this.value);");

            if (GetReportParam(_report, "TypeChart").Split('|')[0] != "1" || _sGraphicModelType == eModelConst.Chart.PYRAMID || _sGraphicModelType == eModelConst.Chart.FUNNEL)
            {
                liContent.Style.Add("display", "none");

            }


            //Masquer le titre
            liContent = CreateHtmlTag("li", "optionsVals");
            ul.Controls.Add(liContent);

            bChked = GetReportParam(_report, "HideTitle") == "1";

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "HideTitle";
            chkBox.AddClick("oReport.SetParam('hidetitle',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 1688));

            #region // Légende

            //Positionner la légende
            liContent = CreateHtmlTag("li", "optionsVals");
            ul.Controls.Add(liContent);

            if (String.IsNullOrEmpty(GetReportParam(_report, "displayLegend")))
            {
                bChked = true;
            }
            else
                bChked = GetReportParam(_report, "displayLegend") == "1" ? true : false;

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "displayLegend";
            chkBox.AddClick("oReport.SetParam('displayLegend',  this.attributes[\"chk\"].value);oReport.SetParam('lstdisplayLegend', document.getElementById('lstdisplayLegend').selectedIndex.toString())");
            chkBox.AddText(eResApp.GetRes(Pref, 8416));




            DropDownList ddlDisplayLegend = new DropDownList();
            liContent.Controls.Add(ddlDisplayLegend);
            ddlDisplayLegend.ID = "lstdisplayLegend";


            ddlDisplayLegend.Attributes.Add("onchange", "oReport.SetParam('lstdisplayLegend', this.value);");


            ddlDisplayLegend.Items.Add(new ListItem(eResApp.GetRes(Pref, 8414), ((int)eModelConst.CHART_DISPLAY_LEGEND.TOP).ToString()));
            ddlDisplayLegend.Items.Add(new ListItem(eResApp.GetRes(Pref, 8415), ((int)eModelConst.CHART_DISPLAY_LEGEND.BOTTOM).ToString()));
            ddlDisplayLegend.Items.Add(new ListItem(eResApp.GetRes(Pref, 8413), ((int)eModelConst.CHART_DISPLAY_LEGEND.RIGHT).ToString()));
            ddlDisplayLegend.Items.Add(new ListItem(eResApp.GetRes(Pref, 8412), ((int)eModelConst.CHART_DISPLAY_LEGEND.LEFT).ToString()));


            try
            {
                if (!String.IsNullOrEmpty(GetReportParam(_report, "lstdisplayLegend")))
                {
                    eModelConst.CHART_DISPLAY_LEGEND dispLegend = (eModelConst.CHART_DISPLAY_LEGEND)Int32.Parse(GetReportParam(_report, "lstdisplayLegend"));
                    ddlDisplayLegend.Items[(int)dispLegend].Selected = true;

                }
                else
                    ddlDisplayLegend.Items[(int)eModelConst.CHART_DISPLAY_LEGEND.BOTTOM].Selected = true;

            }
            catch
            {
                throw;
            }

            #endregion

            #region //Afficher les Etiquettes
            //Afficher les Etiquetteseudo126
            liContent = CreateHtmlTag("li", "optionsVals");
            liContent.ID = "displayEtiquette";
            ul.Controls.Add(liContent);
            liContent.Style.Add(HtmlTextWriterStyle.Display, bNotDisplayEtiquettes ? "none" : "block");


            if (String.IsNullOrEmpty(GetReportParam(_report, "DisplayX")))
            {
                bChked = true;
            }
            else
                bChked = GetReportParam(_report, "DisplayX") == "1" ? true : false;

            chkBox = new eCheckBoxCtrl(bChked, false);
            liContent.Controls.Add(chkBox);
            chkBox.ID = "displayx";
            chkBox.AddClick("oReport.SetParam('displayx',  this.attributes[\"chk\"].value);");
            chkBox.AddText(eResApp.GetRes(Pref, 1689));




            DropDownList ddlDisplayX = new DropDownList();
            liContent.Controls.Add(ddlDisplayX);
            ddlDisplayX.ID = "lstdisplayx";


            ddlDisplayX.Attributes.Add("onchange", "oReport.SetParam('lstdisplayx', this.value);");


            ddlDisplayX.Items.Add(new ListItem(eResApp.GetRes(Pref, 1690), eModelConst.CHART_DISPLAY_X.DIAGONALE.GetHashCode().ToString()));
            ddlDisplayX.Items.Add(new ListItem(eResApp.GetRes(Pref, 1691), eModelConst.CHART_DISPLAY_X.HORIZONTALE.GetHashCode().ToString()));
            ddlDisplayX.Items.Add(new ListItem(eResApp.GetRes(Pref, 1692), eModelConst.CHART_DISPLAY_X.VERTICALE.GetHashCode().ToString()));

            //BSE:#55 990 pour les graphiques type bar, on active que l'affichage en horizontale des étiquettes 
            if (_sGraphicModelType == eModelConst.Chart.BAR || _sGraphicB3DType)
            {
                ddlDisplayX.Items[(int)eModelConst.CHART_DISPLAY_X.DIAGONALE].Attributes.CssStyle.Value = "display:none";
                ddlDisplayX.Items[(int)eModelConst.CHART_DISPLAY_X.VERTICALE].Attributes.CssStyle.Value = "display:none";
            }


            try
            {
                if (!String.IsNullOrEmpty(GetReportParam(_report, "lstdisplayx")))
                {
                    if (_sGraphicModelType == eModelConst.Chart.BAR || _sGraphicB3DType)
                        ddlDisplayX.Items[(int)eModelConst.CHART_DISPLAY_X.HORIZONTALE].Selected = true;
                    else
                    {
                        eModelConst.CHART_DISPLAY_X dispX = (eModelConst.CHART_DISPLAY_X)Int32.Parse(GetReportParam(_report, "lstdisplayx"));
                        ddlDisplayX.Items[dispX.GetHashCode()].Selected = true;
                    }

                }
                else
                    ddlDisplayX.Items[(int)eModelConst.CHART_DISPLAY_X.HORIZONTALE].Selected = true;
            }
            catch
            {
                throw;
            }

            #endregion

            #endregion

            #region Taille de l'ecran

            //Taille de l'ecran

            //le libellé  
            liLabel = CreateHtmlTag("li", "titreChrt", string.Concat(eResApp.GetRes(Pref, 6374), " :"), sId: "tailChrt");
            ul.Controls.Add(liLabel);

            // Taille du Graphique
            //W
            liContent = CreateHtmlTag("li", "optionsVals", String.Concat(eResApp.GetRes(Pref, 1508), " : "));
            ul.Controls.Add(liContent);


            HtmlInputText inp = new HtmlInputText();
            liContent.Controls.Add(inp);
            String s = GetReportParam(_report, "W");
            inp.Value = s.Length > 0 ? s : "800";
            inp.ID = "W";
            inp.Attributes.Add("class", "inpChartSsize");
            inp.Attributes.Add("onchange", "oReport.SetParam('w', this.value);");

            Label label = new Label();
            liContent.Controls.Add(label);
            label.CssClass = "dispOptLbl";
            label.Text = String.Concat("px");


            //H
            liContent = CreateHtmlTag("li", "optionsVals", String.Concat(eResApp.GetRes(Pref, 1507), " : "));
            ul.Controls.Add(liContent);



            inp = new HtmlInputText();
            liContent.Controls.Add(inp);
            s = GetReportParam(_report, "H");
            inp.Value = s.Length > 0 ? s : "800";
            inp.ID = "H";
            inp.Attributes.Add("onchange", "oReport.SetParam('h', this.value);");
            inp.Attributes.Add("class", "inpChartSsize");

            label = new Label();
            liContent.Controls.Add(label);
            label.CssClass = "dispOptLbl";
            label.Text = String.Concat("px");

            #endregion

            #endregion </ul>

            return pn;

        }


        /// <summary>
        /// Génerer un colorPicker pour le graphique jauge
        /// </summary>
        /// <param name="colorPickerId"></param>
        /// <param name="txtColorId"></param>
        /// <param name="txtValue"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Panel BuildColorPicker(string colorPickerId, string txtColorId, string txtValue, int index)
        {
            txtColorId = string.Concat(txtColorId, index);
            colorPickerId = string.Concat(colorPickerId, index);
            Panel colorWrapper = new Panel();
            colorWrapper.CssClass = "colorWrapper";

            #region Textbox
            TextBox textbox = new TextBox();
            textbox.Attributes.Add("onchange", string.Concat("UpdateIntervalsValue();"));
            textbox.ID = txtColorId;
            textbox.CssClass = "txtColor";
            textbox.Text = txtValue;
            #endregion

            #region Colorpicker
            Panel colorPickerWrapper = new Panel();
            colorPickerWrapper.CssClass = "colorPickerWrapper";
            colorPickerWrapper.Attributes.Add("onclick", "top.nsAdmin.openColorPicker(document.getElementById('" + colorPickerId + "'), document.getElementById('" + txtColorId + "'));");

            HtmlGenericControl colorPicker = new HtmlGenericControl();
            colorPicker.ID = colorPickerId;
            colorPicker.Attributes.Add("class", "colorPicker");
            if (!String.IsNullOrEmpty(txtValue))
            {
                colorPicker.Style.Add("background-color", txtValue);
            }

            colorPickerWrapper.Controls.Add(colorPicker);
            #endregion


            colorWrapper.Controls.Add(textbox);
            colorWrapper.Controls.Add(colorPickerWrapper);

            return colorWrapper;
        }


        /// <summary>
        /// Construit le bloc dédié aux permissions sur les rapports
        /// </summary>
        /// <returns>element UL</returns>
        protected override void BuildHistoMenu(HtmlGenericControl ul)
        {
        }

        /// <summary>
        /// option d'envois conditionnel
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="itemPrefix"></param>
        protected override void AddActivateConditonalSendingOption(HtmlGenericControl ul, String itemPrefix)
        {
        }



        /// <summary>
        /// Retourne une chaine de caractère représentant le param d'un nouveau chart
        /// </summary>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static String GetBasicParam(Int32 nTab)
        {
            return String.Concat("&valuesfile=", nTab, "&valuesfield=&valuesoperation=count&filterid=&displayvalues=1&displayx=1&lstdisplayx=1&hidelegend=0&displaylegend=1&lstdisplaylegend=1&hidetitle=0&displayvaluespercent=0&displaygrid=0&displaygridnb=&logo=nologo&public=0&typechart=1|1&title=&modifytitle=&h=600&etiquettesfile=", nTab, "&etiquettesfield=&etiquettesgroup=day&saveas=&w=800&seriestype=0&seriesfile=", nTab, "&seriesfield=&addcurrentfilter=1&sortorder=0&sortenabled=0");
        }


    }

    /// <summary>
    ///  Représente une rubrique ou une table dans desc 
    /// </summary>
    public class DescItem
    {
        public Int32 DescId;
        public String Label;
        public Boolean AllowedView;
        public FieldFormat Format = FieldFormat.TYP_FILE;
        public DescItem SelecetedItem;
        public Int32 PopupDescId;
        public PopupType Popup;
        //Liste des enfant de la table
        public List<DescItem> Items = new List<DescItem>();

        public override String ToString()
        {
            return "Descid : " + DescId + ", Label : " + Label + ", AllowedView : " + AllowedView + ", Format : " + Format;
        }
    }
    /// <summary>
    /// Permet de définir des paramètres spécifiques aux graphiques spécifiques
    /// </summary>
    public class ChartSpecificParams
    {
        /// <summary>
        /// 
        /// </summary>
        public eCommunChart.TypeChart CategoryChart = eCommunChart.TypeChart.GLOBALCHART;
        /// <summary>
        /// 
        /// </summary>
        public string Axe;
        /// <summary>
        /// 
        /// </summary>
        public string Prefix;
        /// <summary>
        /// 
        /// </summary>
        public string Action;
        /// <summary>
        /// 
        /// </summary>
        public bool AddFilter = true;

        /// <summary>
        /// Masquer la case à cocher cumuler avec le filtre en cours
        /// </summary>
        public bool HideCumulFilter = false;

        /// <summary>
        /// 
        /// </summary>
        public bool AddButtonRadio = true;

        /// <summary>
        /// 
        /// </summary>
        public bool AddExpressFilter = false;
        /// <summary>
        /// 
        /// </summary>
        public bool GetAllTab = false;
        /// <summary>
        /// 
        /// </summary>
        public bool AddGaugePart = false;
        /// <summary>
        /// 
        /// </summary>
        public string ChartType = eModelConst.Chart.COMBINED;

        public bool DisabelNotLinkedFile = true;

        public bool OnlyDataField = false;

        public bool GetFilesOnly = false;

        public bool ShowFiles = true;

        public int CobinedFilterdTabId = 0;

        public string CombinedYprefix = eLibConst.COMBINED_Y.ToLower();
        public string CombinedZprefix = eLibConst.COMBINED_Z.ToLower();

        public string PrefixFilter = string.Empty;
        public bool GetLinkedTabOnly = false;



    }

    /// <summary>
    /// Class pour définir les propriété du colorPicker dans l'assistant des graphiques
    /// </summary>
    public class ColorPickerProprieties
    {
        /// <summary>
        /// Index du colorPicker
        /// </summary>
        public int Index { get; set; } = 0;
        /// <summary>
        /// Le code couleur affiché pour le colorPicker
        /// </summary>
        public string Color = eLibConst.DAFAULT_COLOR_FOR_GAUGE_CHART_INTERVAL;
        /// <summary>
        /// Indique si la valeur affichée est bie la valeur maximum autorisée
        /// </summary>
        public bool BmaxValue
        {
            get
            {
                return Value == eLibConst.MAX_INTERVAL_VALUE_FOR_GAUGE_CHART;
            }
        }
        /// <summary>
        /// Indique si on peut ajouter le button d'action(ajout/suppression)
        /// </summary>
        public bool AddBtn
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// La valeur affiché pour le colorPicker
        /// </summary>
        public int Value = 0;
        /// <summary>
        /// Indique la class css du button d'action(ajout/suppression)
        /// </summary>
        public string BtnClass { get { return BmaxValue ? "logoGaugeAddLine" : "logoGaugeDeleteLine"; } }

    }
}