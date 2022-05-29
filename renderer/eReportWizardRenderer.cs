using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.renderer;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer décidé à l'assistant Reporting
    /// </summary>
    public class eReportWizardRenderer : eRenderer
    {
        // Taile de l'écran
        /// <summary>Hauteur de l'écran</summary>
        private int _iheight = eConst.DEFAULT_WINDOW_WIDTH;
        /// <summary>Largeur de l'écran</summary>
        protected int _iwidth = eConst.DEFAULT_WINDOW_HEIGHT;
        private TypeReport _reportType;
        private int _iTotalStep;
        private bool _bFromBkm = false;
        protected bool _bSpecialFilterForGraphique = false;
        private eRes _ressources = null;
        private eRightReport _oRightManager;

        /// <summary>Rapport généré/modifé par le wizard</summary>
        protected eReport _report = null;

        private ISet<int> _listOfTabId;

        private Dictionary<int, ISet<int>> _listOfLinkedTabId = new Dictionary<int, ISet<int>>();

        private ISet<int> _lisEtiquetteXLinkedId;

        /// <summary>Table du rapport</summary>
        protected int _iTab;

        /// <summary>Objet métier du wizard</summary>
        protected eReportWizard _reportWizard;

        /// <summary>Objet eReport associé au renderer s'il y a lieu</summary>
        public eReport Report
        {
            get { return _report; }
            set { _report = value; }
        }



        #region Constructeurs

        /// <summary>
        /// Constructeur privé
        /// </summary>
        /// <param name="ePref">Preferences Utilisateur</param>
        /// <param name="height">Hauteur de la fenêtre</param>
        /// <param name="width">Largeur de la fenêtre</param>
        /// <param name="reportType">Type de rapport</param>
        /// <param name="tab">Onglet en cours</param>
        protected eReportWizardRenderer(ePref ePref, int width, int height, TypeReport reportType, int tab)
        {
            _iwidth = width;
            _iheight = height;
            Pref = ePref;
            _reportType = reportType;
            _iTab = tab;
            _iTotalStep = eReportWizard.GetTotalSteps(reportType);
            _rType = RENDERERTYPE.ReportWizard;

            _oRightManager = new eRightReport(ePref, reportType);
        }

        protected eReportWizardRenderer(ePref ePref, int tab)
        {
            Pref = ePref;
            _iTab = tab;
            _rType = RENDERERTYPE.ChartXML;

        }

        protected eReportWizardRenderer(ePref ePref)
        {
            Pref = ePref;
            _rType = RENDERERTYPE.ChartXML;

        }


        #endregion

        #region Méthodes Statique pour le rendu

        /// <summary>
        /// Génère un renderer paramétrés pour l'assistant Reporting et le retourne
        /// </summary>
        /// <param name="ePref">Préférences de l'utilisateur</param>
        /// <param name="height">hauteur de l'interface</param>
        /// <param name="width">largeur de l'interface</param>
        /// <param name="reportType">Type de rapport pour l'assistant</param>
        /// <param name="tab">Fichier d'origine</param>
        /// <returns>Renderer contenant l'interface graphique de l'assistant</returns>
        public static eReportWizardRenderer GetReportWizardRenderer(ePref ePref, int width, int height, TypeReport reportType, int tab)
        {
            eReportWizardRenderer myRenderer;

            if (reportType == TypeReport.CHARTS)
                myRenderer = eChartWizardRenderer.GetChartWizardRenderer(ePref, width, height, reportType, tab);
            else
                myRenderer = new eReportWizardRenderer(ePref, width, height, reportType, tab);

            return myRenderer;
        }

        /// <summary>
        /// Construit le tableau Javascript de paramètres de l'objet Report.
        /// </summary>
        /// <param name="report">Objet eReport</param>
        /// <returns>Chaine de construction de la variable javascript</returns>
        public static String BuildJavascriptReportParams(eReport report = null)
        {
            StringBuilder javascriptString = new StringBuilder("paramReportArray = ");
            IDictionary<string, string> dicoParams = eReport.GetReportParamValues(report);
            string strDicoJSON = new JavaScriptSerializer().Serialize(dicoParams);
            javascriptString.Append(strDicoJSON);
            javascriptString.Append(";");

            return javascriptString.ToString();
        }

        /// <summary>
        /// Construit et retourne la listes des champs pour le fichier lié depuis afin de les intégrer dans le html de l'éditeur.
        /// </summary>
        /// <param name="pref">préférences de l'utilisateur</param>
        /// <param name="selectedTabName">Libellé du fichier sélectionné</param>
        /// <param name="selectedTab">Descid du fichier sélectionné</param>
        /// <param name="linkedFromTab">Libellé du fichier depuis lequel le fichier sélectionné est lié</param>
        /// <param name="linkedFromTabLabel">Descid du fichier depuis lequel le fichier sélectionné est lié</param>
        /// <returns>Elément "DIV" contenant autant de div que de champs rattaché à ce fichier.</returns>
        public static HtmlGenericControl BuildLinkedList(ePref pref, String selectedTabName, int selectedTab, int linkedFromTab, String linkedFromTabLabel)
        {
            HtmlGenericControl ddlstFields;
            String optClass = "cell";
            int counter = 0;
            HtmlGenericControl listItem = new HtmlGenericControl("div");
            ddlstFields = new HtmlGenericControl("span");
            ddlstFields.Attributes.Add("field_list", "");
            ddlstFields.ID = String.Concat("editor_field", "_", String.Concat(selectedTab, "_", linkedFromTab));
            ddlstFields.Style.Add("width", "100%");
            ddlstFields.Attributes.Add("ednlabel", Uri.EscapeUriString(String.Concat(selectedTabName, "(Depuis ", linkedFromTabLabel, ")")));

            ddlstFields.Style.Add("display", "inline");

            List<eReportWizardField> tabFields = eReportWizard.GetLinkedFields(pref, selectedTab, linkedFromTab);

            //TODO : Récupérer les champs du tab à lier

            foreach (eReportWizardField field in tabFields)
            {
                if (!field.IsDrawable)
                    continue;
                listItem = new HtmlGenericControl("div");
                optClass = counter % 2 == 0 ? "cell" : "cell2";

                listItem.Attributes.Add("value", field.DescId.ToString());
                listItem.Attributes.Add("popuptype", field.PopupType.GetHashCode().ToString());
                listItem.Attributes.Add("popupdescid", field.PopupDescId.ToString());
                listItem.Attributes.Add("edntab", field.Tab.ToString());
                listItem.Attributes.Add("class", optClass);
                listItem.Attributes.Add("oldCss", optClass);
                listItem.InnerHtml = String.Concat(selectedTabName, ".", field.Label);
                listItem.Attributes.Add("tooltiptext", field.ToolTipText);
                listItem.Attributes.Add("onclick", String.Concat("setElementSelected(this);"));
                listItem.Attributes.Add("ednformat", field.Format.GetHashCode().ToString());
                listItem.Attributes.Add("linkedfile", field.LinkedFile);
                listItem.Attributes.Add("linkedFromTab", field.LinkedFromTab.ToString());
                listItem.Attributes.Add("ismultiple", (field.IsMultiple) ? "1" : "0"); // 42068

                ddlstFields.Controls.Add(listItem);

                counter++;
            }

            //Actions JavaScript
            ddlstFields.Attributes.Add("ondblclick", String.Concat("AddReportField(this);"));

            return ddlstFields;
        }

        private static HtmlGenericControl BuildFieldList(KeyValuePair<String, String> tab, List<eReportWizardField> fieldList, bool displayList)
        {
            HtmlGenericControl ddlstFields;
            int counter = 0;
            String optClass = String.Empty;
            HtmlGenericControl itm = null;
            int tabDescId = 0;
            int linkedFromTabDescId = 0;
            if (!int.TryParse(tab.Key.Split('_')[0], out tabDescId) || (tab.Key.IndexOf('_') > 0 && !int.TryParse(tab.Key.Split('_')[1], out linkedFromTabDescId)))
                throw new Exception(String.Concat("Le fichier ", tab.Value, " n'existe pas/plus ou ses paramètres sont incorrects"));

            String fieldId = tab.Key.IndexOf('_') > 0 ? tab.Key : String.Concat(tabDescId, "_", tabDescId);

            ddlstFields = new HtmlGenericControl("span");
            ddlstFields.Attributes.Add("field_list", "");
            ddlstFields.Style.Add("width", "100%");
            ddlstFields.Attributes.Add("SelectedIndex", "0");
            if (!displayList)
                ddlstFields.Style.Add("display", "none");
            ddlstFields.ID = String.Concat("editor_field", "_", fieldId);
            ddlstFields.Attributes.Add("SelectedIndex", "0");

            ddlstFields.Attributes.Add("onclick", "doInitSearch(this, event)");


            List<eReportWizardField> tabFields = fieldList.FindAll(
                    delegate (eReportWizardField fld)
                    {
                        return fld.Tab.Equals(tabDescId)
                        && fld.LinkedFromTab == linkedFromTabDescId
                        && fld.Format != FieldFormat.TYP_ALIASRELATION
                        && fld.Format != FieldFormat.TYP_PASSWORD
                        && fld.TargetDescdId != 1;

                    }
                );

            foreach (eReportWizardField field in tabFields)
            {
                if (!field.IsDrawable)
                    continue;
                itm = new HtmlGenericControl("div");
                optClass = counter % 2 == 0 ? "cell" : "cell2";

                itm.Attributes.Add("value", field.DescId.ToString());
                itm.Attributes.Add("popuptype", field.PopupType.GetHashCode().ToString());
                itm.Attributes.Add("popupdescid", field.PopupDescId.ToString());
                itm.Attributes.Add("edntab", field.Tab.ToString());
                itm.Attributes.Add("class", optClass);
                itm.Attributes.Add("oldCss", optClass);
                itm.Attributes.Add("ismultiple", (field.IsMultiple) ? "1" : "0"); // 42068

                itm.InnerHtml = HttpUtility.HtmlEncode(field.Label);
                itm.Attributes.Add("shlb", field.Label);
                itm.Attributes.Add("lglb", String.Concat(tab.Value, ".", field.Label));

                itm.Attributes.Add("tooltiptext", field.ToolTipText);
                itm.Attributes.Add("onclick", String.Concat("setElementSelected(this);"));
                itm.Attributes.Add("ednformat", field.Format.GetHashCode().ToString());
                itm.Attributes.Add("linkedfile", field.LinkedFile);
                itm.Attributes.Add("linkedFromTab", field.LinkedFromTab.ToString());
                itm.Attributes.Add("onmousedown", "strtDrag(event);");

                // #22845
                if (field.DescId % 100 == 1)
                {
                    itm.Attributes.Add("data-mainfield", "1");
                }

                ddlstFields.Controls.Add(itm);

                counter++;
            }

            // Création du guide de déplacement
            itm = new HtmlGenericControl("div");
            itm.ID = String.Concat("AllListElmntGuidRW_", fieldId);
            itm.Attributes.Add("class", "dragGuideTab");
            itm.Attributes.Add("syst", "");
            itm.Style.Add("display", "none");
            ddlstFields.Controls.Add(itm);

            ddlstFields.Attributes.Add("ondblclick", String.Concat("AddReportField(this);"));

            return ddlstFields;
        }

        /// <summary>
        /// Retourne sous forme de XML la liste des champs à insérer dans les listes de regroupement/tri de l'assistant
        /// </summary>
        /// <param name="pref">préférences de l'utilisateur</param>
        /// <param name="reportFields">paramètres field du rapport</param>
        /// <returns>Liste des champs sous forme de XML</returns>
        public static XmlDocument GetSortAndGroupAvailableFields(ePref pref, String reportFields)
        {
            XmlDocument resultDocument = new XmlDocument();
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            String[] fieldParam = reportFields.Split(';');
            List<eReportWizardField> fieldList = new List<eReportWizardField>();
            List<eReportWizardField> tempList = new List<eReportWizardField>();
            int workingTab = 0, workingLinkedTab = 0;
            String errorMsg = String.Empty;
            String resList = String.Empty;
            List<int> loadedTabs = new List<int>();
            eRes resManager = null;
            bool itemFound = false;

            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            resultDocument.AppendChild(resultDocument.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = resultDocument.CreateElement("result");
            resultDocument.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = resultDocument.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = resultDocument.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = resultDocument.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = resultDocument.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            #endregion

            try
            {
                dal.OpenDatabase();

                #region récupération des champs

                foreach (String val in fieldParam)
                {
                    if (val.IndexOf(',') > 0)
                    {
                        if (!int.TryParse(val.Split(',')[0], out workingTab))
                            continue;
                        if (!int.TryParse(val.Split(',')[1], out workingLinkedTab))
                            continue;

                        workingTab = workingTab - workingTab % 100;
                        if (!loadedTabs.Contains(workingTab))
                        {
                            fieldList.AddRange(eReportWizard.AddFields(pref, dal, workingTab, workingLinkedTab, out errorMsg));
                            resList = String.Concat(resList, workingTab, ",");
                            loadedTabs.Add(workingTab);
                        }

                        if (!int.TryParse(val.Split(',')[1], out workingTab))
                            continue;
                        workingTab = workingTab - workingTab % 100;
                        if (!loadedTabs.Contains(workingTab))
                        {
                            tempList = eReportWizard.AddFields(pref, dal, workingTab, null, out errorMsg);
                            foreach (eReportWizardField fieldElement in tempList)
                            {
                                fieldElement.LinkedFromTab = workingTab;
                            }
                            fieldList.AddRange(tempList);
                            resList = String.Concat(resList, workingTab, ",");
                            tempList.Clear();
                            loadedTabs.Add(workingTab);
                        }
                    }
                    else
                    {
                        if (!int.TryParse(val, out workingTab))
                            continue;
                        workingTab = workingTab - workingTab % 100;
                        if (!loadedTabs.Contains(workingTab))
                        {
                            fieldList.AddRange(eReportWizard.AddFields(pref, dal, workingTab, null, out errorMsg));
                            resList = String.Concat(resList, workingTab, ",");
                            loadedTabs.Add(workingTab);
                        }
                    }
                }
                #endregion

                if (resList.Length > 0)
                {
                    resList = resList.Remove(resList.LastIndexOf(','), 1);
                    resManager = new eRes(pref, resList);
                }
                else
                    if (fieldParam.Length > 0 && !String.IsNullOrEmpty(fieldParam[0]))
                    errorMsg = "eReportWizardRenderer.GetSortAndGroupAvailableFields() : Aucun champs ni fichier n'a été trouvé";
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            finally
            {
                dal.CloseDatabase();
            }

            #region affectation des champs dans le XML
            XmlNode fieldNode = null;
            XmlAttribute attr = null;
            String FieldLabel = String.Empty;
            String FileLabel = String.Empty;
            foreach (eReportWizardField field in fieldList)
            {
                fieldNode = resultDocument.CreateElement("field");
                attr = resultDocument.CreateAttribute("descid");
                attr.Value = field.DescId.ToString();
                fieldNode.Attributes.Append(attr);
                if (field.LinkedFromTab > 0)
                {
                    FileLabel = eReportWizard.GetLinkedFileLabel(resManager.GetRes(field.Tab, out itemFound), resManager.GetRes(field.LinkedFromTab, out itemFound));
                    FieldLabel = String.Concat(FileLabel, ".", field.Label);
                    //KJE #65 565: s'il s'agit d'un élément appartenant à une table liéée, on rajoute un attribut qui contient le descID de la table liée
                    attr = resultDocument.CreateAttribute("linkedfromtab");
                    attr.Value = field.LinkedFromTab.ToString();
                    fieldNode.Attributes.Append(attr);
                }
                else
                    FileLabel = resManager.GetRes(field.Tab, out itemFound);

                FieldLabel = String.Concat(FileLabel, ".", field.Label);
                fieldNode.InnerText = Uri.EscapeUriString(FieldLabel);
                contentNode.AppendChild(fieldNode);
            }
            #endregion

            successNode.InnerText = errorMsg.Length > 0 ? "0" : "1";
            errorCodeNode.InnerText = errorMsg.Length > 0 ? "1" : "0";
            errorMsgNode.InnerText = errorMsg;
            return resultDocument;
        }
        #endregion

        #region Méthodes héritées de eRenderer, process de création du renderer
        /// <summary>
        /// Initialisation du renderer
        /// </summary>
        /// <param name="report">Objet eReport pour le chargement d'un rapport en modification</param>
        /// <returns></returns>
        protected bool Init(eReport report)
        {
            //todo Chargement du report
            return true;
        }

        /// <summary>
        /// Initialisation du renderer en cas d'ajout de nouveau rapport
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            try
            {
                // Recupe des ressources
                //private String _strAddressFileLabel = String.Empty;
                ISet<int> resIdList = new HashSet<int>() { TableType.PP.GetHashCode(), TableType.ADR.GetHashCode(), _iTab, 201, 202, 203 };
                _ressources = new eRes(Pref, eLibTools.Join(",", resIdList));

                if (_report == null)
                    _reportWizard = new eReportWizard(Pref, _iTab, _reportType, _bFromBkm);
                else
                    _reportWizard = new eReportWizard(Pref, _iTab, _report, _bFromBkm);
                return true;
            }
            catch (Exception e)
            {
                _eException = e;
                return false;
            }
        }

        /// <summary>
        /// Construit le corps de l'assistant, composé d'un div de param et d'un div par étape d'assistant
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            //header (Etapes)
            Panel Header = this.BuildHeader();
            //body (assistant)
            Panel Editor = this.BuildBody();
            //footer (boutons)


            this.PgContainer.Controls.Add(Header);
            this.PgContainer.Controls.Add(Editor);

            return true;
        }
        #endregion

        #region Construction du header de la page
        /// <summary>
        /// Constuit la partie Haute de l'assistant reporting
        /// Contenant les boutons et libellés des différentes étapes.
        /// </summary>
        /// <returns>Div conteneur de la partie haute de l'assistant</returns>
        private Panel BuildHeader()
        {
            Panel header = new Panel();
            header.ID = "wizardheader";
            header.CssClass = "wizardheader";
            Panel stepGroup = new Panel();
            stepGroup.CssClass = String.Concat("states_placement", _rType == RENDERERTYPE.ChartWizard ? " stpPlcmtChrt" : "");
            //stepGroup.CssClass = "states_placement";
            int nActiveStep = 1;

            for (int i = 1; i <= _iTotalStep; i++)
            {
                stepGroup.Controls.Add(BuildStepDiv(i, i == nActiveStep));
                if (i < _iTotalStep)
                    stepGroup.Controls.Add(BuildSeparatorDiv());
            }
            header.Controls.Add(stepGroup);
            return header;
        }

        /// <summary>
        /// Construit le blocs de boutons d'étapes de la partie haute
        /// </summary>
        /// <param name="step">Numéro d'étape</param>
        /// <param name="isActive">étape active de l'assistant</param>
        /// <returns>Panel (div) de l'étape</returns>
        protected virtual Panel BuildStepDiv(int step, bool isActive)
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
                    lbl.Text = eResApp.GetRes(Pref, 6380);
                    break;
                case 2:
                    lbl.Text = eResApp.GetRes(Pref, 6578);
                    break;
                case 3:
                    lbl.Text = eResApp.GetRes(Pref, 6579);
                    break;
                case 4:
                    lbl.Text = eResApp.GetRes(Pref, 6580);
                    break;
                case 5:
                    lbl.Text = eResApp.GetRes(Pref, 6381);
                    break;
                default:
                    lbl.Text = String.Concat(eResApp.GetRes(Pref, 1617) + " : ", step);
                    break;
            }
            stepBloc.CssClass = isActive ? "state_grp-current" : _report == null ? "state_grp" : "state_grp-validated";
            stepBloc.Controls.Add(numberBloc);
            stepBloc.Controls.Add(lbl);


            return stepBloc;
        }

        /// <summary>
        /// Construit le bloc de séparation entre deux boutons d'étape.
        /// </summary>
        /// <returns>Panel (div) de Séparation entre deux étapes</returns>
        private Panel BuildSeparatorDiv()
        {
            Panel sepBloc = new Panel();
            sepBloc.CssClass = "state_sep";
            return sepBloc;
        }
        #endregion

        #region construction du body

        #region Bloc de construction principal
        /// <summary>
        /// Construit le div englobant le contenu de l'éditeur et y ajoute le contenu
        /// </summary>
        private Panel BuildBody()
        {
            Panel wizardBody = new Panel();
            wizardBody.ID = "wizardbody";
            wizardBody.CssClass = "wizardbody";

            for (int i = 1; i <= this._iTotalStep; i++)
                wizardBody.Controls.Add(BuildBodyStep(i));

            return wizardBody;
        }

        /// <summary>
        /// Construit le bloc div d'une étape donnée de l'assitant et le retourne
        /// </summary>
        /// <param name="step">Numéro d'étape de l'assistant</param>
        /// <returns>Panel(div) de l'étape demandée</returns>
        protected virtual Panel BuildBodyStep(int step)
        {
            Panel pEditDiv = new Panel();
            pEditDiv.ID = String.Concat("editor_", step);
            pEditDiv.CssClass = step == 1 ? "editor-on" : "editor-off";
            HtmlGenericControl lblFormat = new HtmlGenericControl("div");

            switch (step)
            {
                case 1:
                    #region Première Page
                    pEditDiv.Controls.Add(this.BuildSelectFieldsPanel());
                    #endregion
                    break;

                case 2:
                    #region Seconde Page
                    pEditDiv.Controls.Add(this.BuildConfigurableFieldsPanel());
                    #endregion
                    break;
                case 3:
                    #region Troisième Page

                    Panel pResult = new Panel();
                    pResult.CssClass = "reportBase";

                    //Titre du menu
                    lblFormat.ID = "editor_format_choice";
                    lblFormat.InnerText = eResApp.GetRes(Pref, 696);
                    lblFormat.Attributes.Add("Class", "opt_inst");

                    pResult.Controls.Add(lblFormat);
                    pResult.Controls.Add(BuildFormatPanel());

                    //Blocs de détail du modèles spéciques au publipostage
                    if (_reportType.Equals(TypeReport.MERGE))
                    {
                        pResult.Controls.Add(BuildPdfTemplateFieldsMenu());
                        pResult.Controls.Add(BuildHTMLTemplateFieldsMenu());
                        pResult.Controls.Add(BuildTextEditorTemplateMenu());
                    }
                    else if (_reportType == TypeReport.PRINT || _reportType == TypeReport.PRINT_FILE || _reportType == TypeReport.EXPORT)
                    {
                        pResult.Controls.Add(BuildFontOptions());
                    }

                    pEditDiv.Controls.Add(pResult);

                    #endregion
                    break;
                case 4:
                    #region Quatrième Page
                    Panel pResultOptions = new Panel();
                    pResultOptions.CssClass = "reportFilter";
                    HtmlGenericControl ulGroup = new HtmlGenericControl("ul");
                    HtmlGenericControl ulSort = new HtmlGenericControl("ul");

                    pResultOptions.Controls.Add(BuildFilterList());
                    pResultOptions.Controls.Add(BuildSortMenu());

                    //Regroupements uniquement en impression
                    if (_reportType.Equals(TypeReport.PRINT))
                    {
                        HtmlGenericControl divGroupMenu = BuildGroupMenu();
                        pResultOptions.Controls.Add(divGroupMenu);
                    }
                    pEditDiv.Controls.Add(pResultOptions);
                    #endregion
                    break;
                case 5:
                    #region Cinquième Page
                    pEditDiv.Controls.Add(BuildRecordingPanel());
                    #endregion
                    break;
            }

            return pEditDiv;
        }
        #endregion

        #region blocs de construction annexes

        /// <summary>
        /// Construit le corps de page de l'étape de choix de format
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected virtual Panel BuildFormatPanel()
        {
            eCheckBoxCtrl input;

            Label lblFormatChoice = new Label();
            Panel pFormat = new Panel();

            Panel pOutputFile = new Panel();
            HtmlGenericControl textBloc = null;
            HtmlGenericControl word_OOBloc = null;
            HtmlGenericControl excelBloc = null;

            HtmlGenericControl templateInput = new HtmlGenericControl("input");
            HtmlGenericControl formatOptionBtn = new HtmlGenericControl("input");
            HtmlGenericControl list = new HtmlGenericControl("ul");
            HtmlGenericControl listItem = new HtmlGenericControl("li");

            //Div englobant la liste d'options
            pFormat.ID = "editor_format_div";
            pFormat.CssClass = "template_sel";

            //Choix du fichier modèle de document
            listItem.Attributes.Add("class", "editor_formatli");

            list.Controls.Add(listItem);

            listItem = new HtmlGenericControl("li");
            listItem.Attributes.Add("class", "editor_formatli");

            if (GetReportParam(_report, "usetemplate") != "0")
                templateInput.Attributes.Add("value", GetReportParam(_report, "template"));

            input = new eCheckBoxCtrl(false, false);
            input.ID = "editor_usetemplate";
            input.Style.Add("display", "none");
            list.Controls.Add(listItem);


            list = new HtmlGenericControl("ul");
            list.Attributes.Add("class", "editor_formatoptions");

            foreach (ReportFormat format in Enum.GetValues(typeof(ReportFormat)))
            {
                //Si on n'a pas les droits de traitement on zappe
                if (!HasTreatmentRight(format))
                    continue;

                if (format == ReportFormat.OPEN_OFFICE) //KHA le 05/08/2014 cette option (OpenOffice) est prise en compte à partir des paramètres de l'utilisateur et non pas des paramètres du rapport
                    continue;

                if (format == ReportFormat.POWERBI)
                {
                    /* Spécifications Power BI :
                    - En V1, les rapports Power BI ne sont accessibles en création, modification, et suppression qu’aux administrateurs.
                    > cf. #63 663 : ajout de droits de traitement pour cette restriction. On sera donc sorti de la boucle par !HasTreatmentRight(format) ci-dessus dans ce cas
                    - L’affichage des informations respecte les droits de visualisation
                    - Power BI est disponible si l’extension est activée
                    */

                    if (!eExtension.IsReady(Pref, ExtensionCode.POWERBI))
                        continue;
                }

                bool bDisabled = false;
                listItem = new HtmlGenericControl("li");
                formatOptionBtn = new HtmlGenericControl("input");
                formatOptionBtn.Attributes.Add("type", "radio");
                formatOptionBtn.Attributes.Add("name", "editor_format");
                formatOptionBtn.Attributes.Add("class", "editor_formatradio");
                formatOptionBtn.ID = String.Concat("editor_format_label_", format.GetHashCode());

                lblFormatChoice = new Label();
                formatOptionBtn.Attributes.Add("onclick", "oReport.SetParam(\"format\",MapFormatValue(oReport.GetType(), this.value));ManageFormatDisplay(this.value);");
                formatOptionBtn.Attributes.Add("value", format.GetHashCode().ToString());

                //on charge la valeur depuis le rapport existant, dans le cas d'un nouveau rapport on sélectionne le format par défaut

                switch (_reportType)
                {
                    /*En V7 lors des impression le format n'était pas renseigné car il n'y en avait qu'un (HTML format d'index 4).
                    *Dans le cas d'une gestion centralisée pour tous les types de rapport, il est donc nécessaire de prendre en compte l'absence de format dans les paramètres
                    *et également le format unique de l'impression, on centralise donc le test uniquement sur l'index de format HTML d'impression(4)
                    */
                    case TypeReport.PRINT:
                        if (format == ReportFormat.HTML)
                            formatOptionBtn.Attributes.Add("checked", "checked");
                        else
                        {
                            bDisabled = true;
                            formatOptionBtn.Attributes.Add("disabled", "disabled");
                            listItem.Style.Add("display", "none");
                        }
                        break;
                    case TypeReport.EXPORT:
                        excelBloc = BuildExcelTemplateMenu();
                        textBloc = BuildTxtFormatOptions();
                        if (format == ReportFormat.PDF)
                        {
                            bDisabled = true;
                            formatOptionBtn.Attributes.Add("disabled", "disabled");
                            listItem.Style.Add("display", "none");
                        }
                        else
                            if ((_report == null && format == ReportFormat.TEXT) ||
                                (_report != null && format.GetHashCode() == int.Parse(GetReportParam(_report, "format"))))
                            formatOptionBtn.Attributes.Add("checked", "checked");
                        break;
                    case TypeReport.MERGE:

                        //word_OOBloc = BuildTextEditorTemplateMenu();
                        if (format == ReportFormat.TEXT || format == ReportFormat.EXCEL || format == ReportFormat.POWERBI)
                        {
                            bDisabled = true;
                            formatOptionBtn.Attributes.Add("disabled", "disabled");
                            listItem.Style.Add("display", "none");
                        }
                        else
                            if ((_report == null && format == ReportFormat.WORD) ||
                                (_report != null && eReportWizard.MapFormatValue(_reportType, format.GetHashCode()) == int.Parse(GetReportParam(_report, "format"))))
                            formatOptionBtn.Attributes.Add("checked", "checked");
                        break;
                    default:
                        break;
                }

                // Label
                HtmlGenericControl span = new HtmlGenericControl("label");

                span.Attributes.Add("for", formatOptionBtn.ID);

                // Bouton radio
                span.Controls.Add(formatOptionBtn);
                // Icône
                String labelClass = "icon-word";
                switch (format)
                {
                    // TOCHECK : et OpenOffice ?
                    case ReportFormat.EXCEL:
                        labelClass = "icon-excel"; break;
                    case ReportFormat.TEXT:
                        labelClass = "icon-text"; break;
                    case ReportFormat.WORD:
                        labelClass = "icon-word"; break;
                    case ReportFormat.PDF:
                        labelClass = "icon-pdf"; break;
                    case ReportFormat.HTML:
                        labelClass = "icon-html"; break;
                    case ReportFormat.POWERBI:
                        labelClass = "icon-power-bi"; break;
                }
                HtmlGenericControl iconSpan = new HtmlGenericControl("span");
                iconSpan.Attributes.Add("class", labelClass);
                span.Controls.Add(iconSpan);

                LiteralControl labelText = new LiteralControl(eParamListTools.ReportGetFormatLabel(Pref, (int)format, _reportType));
                span.Controls.Add(labelText);

                listItem.Controls.Add(span);

                listItem.Attributes.Add("class", String.Concat(GetCssClass(format), bDisabled ? " disabled" : ""));
                if (_reportType.Equals(TypeReport.MERGE) && format == ReportFormat.PDF)
                    listItem.Controls.Add(BuildPdfFormatOption());


                list.Controls.Add(listItem);
            }
            pFormat.Controls.Add(list);
            if (textBloc != null)
                pFormat.Controls.Add(textBloc);

            if (word_OOBloc != null)
                pFormat.Controls.Add(word_OOBloc);

            if (excelBloc != null)
                pFormat.Controls.Add(excelBloc);

            return pFormat;
        }

        /// <summary>
        /// Savoir si on a le droit d'ajouter un nouveau rapport  
        /// </summary>
        /// <param name="format">format de sortie de rapport</param>
        /// <returns>A le droit d'ajout</returns>
        private bool HasTreatmentRight(ReportFormat format)
        {
            // Pour tous les formats hors Power BI, et les formats PDF/HTML/WORD hors Publipostage, on renvoie le droit défini par la classe
            if (_reportType != TypeReport.MERGE && format != ReportFormat.POWERBI)
                return _oRightManager.CanAddNewItem();

            // Si on traite un cas de publipostage (MERGE), on vérifie les droits spécifiques au format ; sinon, on vérifie le droit d'ajout défini sur la classe en cours (CanAddNewItem)
            // Si on traite un format Power BI, on vérifie les droits par rapport à ce format quel que soit le type de rapport
            return _oRightManager.HasRightByFormat(eRightReport.RightByFormat.Operation.ADD, format);
        }

        /// <summary>
        /// En foction du format du rapport, on retourne la classe css 
        /// </summary>
        /// <param name="format">Format de sortie</param>
        /// <returns>Class css</returns>
        private static string GetCssClass(ReportFormat format)
        {
            switch (format)
            {
                case ReportFormat.TEXT:
                    return "icon_texte";
                case ReportFormat.EXCEL:
                    return "icon_excel";
                case ReportFormat.WORD:
                    return "icon_word";
                case ReportFormat.HTML:
                    return "icon_html";
                case ReportFormat.OPEN_OFFICE:
                    return "icon_ooffice";
                case ReportFormat.PDF:
                    return "icon_pdf";
                case ReportFormat.POWERBI:
                    return "icon_powerbi";
                default:
                    throw new Exception(String.Concat("eReportWizardRenderer.GetCssClass: Format de rapport inconu '", format, "' !"));
            }
        }

        /// <summary>
        /// Construit le corps de page de l'étape de sélection des champs du rapport
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        protected virtual Panel BuildSelectFieldsPanel()
        {
            /* JAS : 17/08/2012 Recontruction côté serveur du tableau de sélection des rubriquede eFieldSelect.aspx 
                   * Voué à une refactorisation une fois l'ensemble de la page opérationnelle 
                   * */
            Panel pFieldSelectDiv = new Panel();
            DropDownList ddlFileList = BuildFileList();
            HtmlTable MainTable = new HtmlTable();
            HtmlTableRow row = new HtmlTableRow();
            HtmlTableCell cell = new HtmlTableCell();
            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            HtmlGenericControl Div = new HtmlGenericControl("div");
            HtmlGenericControl DivIcon = new HtmlGenericControl("div");
            HtmlGenericControl DivText = new HtmlGenericControl("div");
            HtmlGenericControl centerTag = new HtmlGenericControl("center");
            Panel pAddress = new Panel();
            Panel pWarning = new Panel();
            Panel pActive = new Panel();
            Panel pMain = new Panel();
            Label lblAddressMsg = new Label();
            Label lblAddress = new Label();
            //Label lblActive = new Label();
            bool bChecked = false;
            //Label lblMain = new Label();

            // #33 098, #32 972, #33 601, #33 620 - Redimensionnement des listes de rubriques - Calcul de la hauteur à donner aux listes en fonction de celle de la fenêtre
            // 25 pixels réservés pour la barre de titre
            // 150 pixels pour la partie haute (rail de navigation + combobox de sélection du fichier de rubriques)
            // 35 pixels pour l'entête des listes ("Rubriques disponibles" - "Rubriques sélectionnées")
            // 125 pixels pour les options du bas ("Ne retenir que les fiches Adresse Actives / Principales")
            // 60 pixels d'espace réservé aux boutons de la modal dialog
            // A ajuster si des options se rajoutent sur la fenêtre par la suite
            int itemListHeight = _iheight - 25 - 150 - 35 - 125 - 60;

            pFieldSelectDiv.ID = "editor_fieldSelectDiv";
            pFieldSelectDiv.CssClass = "fieldSelectDiv";

            MainTable.ID = "editor_field_selection";
            MainTable.Attributes.Add("cellpadding", "0");
            MainTable.Attributes.Add("cellspacing", "0");

            cell.Style.Add("width", "4%");
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Attributes.Add("colspan", "4");
            cell.Controls.Add(new LiteralControl("<br/>"));

            titleDiv.ID = "editor_DivTitle";
            titleDiv.Attributes.Add("class", "eTitle");
            switch (_reportType)
            {
                case TypeReport.EXPORT:
                    // 6711 - Sélectionnez et glissez les rubriques à exporter
                    titleDiv.InnerHtml = eResApp.GetRes(Pref, 6711);
                    break;
                case TypeReport.MERGE:
                    // 1796 - Sélectionnez et glissez les rubriques à exporter pour publipostage
                    titleDiv.InnerHtml = eResApp.GetRes(Pref, 1796);
                    break;
                case TypeReport.PRINT:
                    // 1797 - Sélectionnez et glissez les rubriques à imprimer
                    titleDiv.InnerHtml = eResApp.GetRes(Pref, 1797);
                    break;
                default:
                    titleDiv.InnerHtml = String.Empty;      // Cas non géré
                    break;
            }

            cell.Controls.Add(titleDiv);
            cell.Controls.Add(new LiteralControl("<br />"));
            row.Controls.Add(cell);
            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "4%");
            row.Controls.Add(cell);

            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "38%");

            Div.ID = "editor_filediv";
            Div.Attributes.Add("class", "eMainFileList");
            Div.Style.Add("margin-bottom", "18px");
            Div.Controls.Add(ddlFileList);
            cell.Controls.Add(Div);
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "10%");
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "38%");
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "20%");
            row.Controls.Add(cell);
            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            row.Controls.Add(new HtmlTableCell());

            //Rubriques sélectionnées
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "tdHeader");
            cell.InnerHtml = eResApp.GetRes(Pref, 6229);
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Controls.Add(new LiteralControl("&nbsp;"));
            row.Controls.Add(cell);

            //RubriquesDisponibles
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "tdHeader");
            cell.InnerHtml = eResApp.GetRes(Pref, 6230);
            row.Controls.Add(cell);
            cell = new HtmlTableCell();
            cell.Controls.Add(new LiteralControl("&nbsp;"));
            row.Controls.Add(cell);

            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            row.Controls.Add(new HtmlTableCell());
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "ItemListTd");

            Div = new HtmlGenericControl("div");
            Div.ID = "editor_sourcelist";
            Div.Attributes.Add("class", "ItemList");
            Div.Style.Add(HtmlTextWriterStyle.Height, String.Concat(itemListHeight, "px"));

            foreach (HtmlGenericControl ddl in BuildFieldList())
            {
                Div.Controls.Add(ddl);
            }

            cell.Controls.Add(Div);
            row.Controls.Add(cell);

            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "ItemListSep");

            Div = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonSelectUnit";
            Div.Attributes.Add("class", "icon-item_add");
            Div.Attributes.Add("onclick", "AddReportField()");
            centerTag.Controls.Add(Div);
            centerTag.Controls.Add(new LiteralControl("<br />"));

            Div = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonUnselectUnit";
            Div.Attributes.Add("class", "icon-item_rem");
            Div.Attributes.Add("onclick", "DelReportField()");
            centerTag.Controls.Add(Div);
            cell.Controls.Add(centerTag);
            row.Controls.Add(cell);

            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "ItemListTd");
            Div = new HtmlGenericControl("div");
            Div.ID = "editor_DivTargetList";
            Div.Attributes.Add("class", "ItemList");
            Div.Controls.Add(GetUsedFields());
            Div.Style.Add(HtmlTextWriterStyle.Height, String.Concat(itemListHeight, "px"));

            cell.Controls.Add(Div);
            row.Controls.Add(cell);

            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "ItemListSep");
            centerTag = new HtmlGenericControl("center");
            Div = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonUp";
            Div.Attributes.Add("class", "icon-item_up");
            Div.Attributes.Add("onclick", "MoveComboItem(true);");
            centerTag.Controls.Add(Div);
            centerTag.Controls.Add(new LiteralControl("<br />"));

            Div = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonDown";
            Div.Attributes.Add("class", "icon-item_down");
            Div.Attributes.Add("onclick", "MoveComboItem(false);");
            centerTag.Controls.Add(Div);
            cell.Controls.Add(centerTag);
            row.Controls.Add(cell);

            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            row.Controls.Add(new HtmlTableCell());
            cell = new HtmlTableCell();
            cell.Attributes.Add("align", "center");
            cell.Controls.Add(new LiteralControl("<br />"));

            Div = new HtmlGenericControl("div");
            DivIcon = new HtmlGenericControl("div");
            DivText = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonSelectAll";
            DivIcon.ID = "editor_DivButtonSelectAllIcon";
            DivText.ID = "editor_DivButtonSelectAllText";
            Div.Attributes.Add("class", "btnSelectUnselectAll");
            DivIcon.Attributes.Add("class", "btnSelectUnselectAllIcon icon-select_all");
            DivText.Attributes.Add("class", "btnSelectUnselectAllText");
            Div.Attributes.Add("onclick", "AddAllReportFields();");
            DivText.InnerHtml = String.Concat(" ", eResApp.GetRes(Pref, 431));
            Div.Controls.Add(DivIcon);
            Div.Controls.Add(DivText);
            cell.Controls.Add(Div);

            row.Controls.Add(cell);
            row.Controls.Add(new HtmlTableCell());

            cell = new HtmlTableCell();
            cell.Attributes.Add("align", "center");
            cell.Controls.Add(new LiteralControl("<br />"));

            Div = new HtmlGenericControl("div");
            DivIcon = new HtmlGenericControl("div");
            DivText = new HtmlGenericControl("div");
            Div.ID = "editor_DivButtonUnselectAll";
            DivIcon.ID = "editor_DivButtonUnselectAllIcon";
            DivText.ID = "editor_DivButtonUnselectAllText";
            Div.Attributes.Add("class", "btnSelectUnselectAll");
            DivIcon.Attributes.Add("class", "btnSelectUnselectAllIcon icon-remove_all");
            DivText.Attributes.Add("class", "btnSelectUnselectAllText");
            Div.Attributes.Add("onclick", "RemoveAllReportFields();");
            DivText.InnerHtml = String.Concat(" ", eResApp.GetRes(Pref, 432));
            Div.Controls.Add(DivIcon);
            Div.Controls.Add(DivText);
            cell.Controls.Add(Div);

            row.Controls.Add(cell);
            row.Controls.Add(new HtmlTableCell());

            MainTable.Controls.Add(row);

            pAddress.ID = "editor_adr";
            pAddress.CssClass = "adr-msg";

            pWarning.ID = "editor_warning";
            pWarning.CssClass = "adr-warning";
            pWarning.Style.Add("visibility", _report == null ? "hidden" : _report.ContainsAddressFields() ? "visible" : "hidden");
            pActive.ID = "editor_adr_active";
            pActive.CssClass = "adr-active";
            pActive.Style.Add("display", "inline;");

            pMain.ID = "editor_adr_main";
            pMain.CssClass = "adr-main";
            pMain.Style.Add("display", "inline;");

            lblAddress.Text = HttpUtility.HtmlEncode(eResApp.GetRes(Pref, 156).Replace("<PREFNAME>", _ressources.GetRes(TableType.ADR.GetHashCode())));

            lblAddressMsg.Text = HttpUtility.HtmlEncode(eResApp.GetRes(Pref, 6516)
                .Replace("<PP>", _ressources.GetRes(TableType.PP.GetHashCode()))
                .Replace("<ADR>", _ressources.GetRes(TableType.ADR.GetHashCode())));
            pWarning.Controls.Add(lblAddressMsg);

            bChecked = GetReportParam(_report, "active") == "1" ? true : false;

            eCheckBoxCtrl chkActive = new eCheckBoxCtrl(bChecked, false);
            chkActive.ID = "editor_active";
            chkActive.AddClick("oReport.SetParam(\"active\", this.attributes[\"chk\"].value);");
            chkActive.AddText(eResApp.GetRes(Pref, 6295));    //Actives
            bChecked = GetReportParam(_report, "main") == "1" ? true : false;
            eCheckBoxCtrl chkMain = new eCheckBoxCtrl(bChecked, false);
            chkMain.ID = "editor_main";
            chkMain.AddClick("oReport.SetParam(\"main\", this.attributes[\"chk\"].value);");
            chkMain.AddText(eResApp.GetRes(Pref, 6294));    //Principales

            pActive.Controls.Add(chkActive);
            pMain.Controls.Add(chkMain);


            lblAddress.Style.Add("visibility", "hidden");
            pActive.Style.Add("visibility", "hidden");
            pMain.Style.Add("visibility", "hidden");

            TableLite TargetTab = null;
            string error = String.Empty;

            try
            {
                TargetTab = eLibTools.GetTableInfo(Pref, _iTab, TableLite.Factory());
            }
            catch (Exception exp)
            {
                error = exp.Message;
            }

            if (TargetTab != null && error.Length == 0)
            {
                if (
                       (TargetTab.TabType == TableType.EVENT && (TargetTab.InterPM || TargetTab.InterPM))
                    || (TargetTab.TabType == TableType.PP)
                    || (TargetTab.TabType == TableType.PM)
                    || (TargetTab.TabType == TableType.TEMPLATE && TargetTab.AdrJoin)
                    )
                {
                    lblAddress.Style.Add("visibility", "visible");
                    pActive.Style.Add("visibility", "visible");
                    pMain.Style.Add("visibility", "visible");
                }
            }

            pAddress.Controls.Add(pWarning);
            pAddress.Controls.Add(lblAddress);
            pAddress.Controls.Add(pActive);
            pAddress.Controls.Add(pMain);

            /*-------------------------------------------*/
            pFieldSelectDiv.Controls.Add(MainTable);
            pFieldSelectDiv.Controls.Add(pAddress);

            return pFieldSelectDiv;
        }

        /// <summary>
        /// Construit le corps de page de l'étape des configuration d'options sur les champs
        /// </summary>
        /// <returns>Panel de type DIV contenant le code HTML de l'étape</returns>
        private Panel BuildConfigurableFieldsPanel()
        {
            Panel pReportBase = new Panel();
            eCheckBoxCtrl input = new eCheckBoxCtrl(false, false);
            pReportBase.CssClass = "reportBase";
            HtmlGenericControl ulFields = new HtmlGenericControl("ul");
            HtmlGenericControl ulOptions = new HtmlGenericControl("ul");
            HtmlGenericControl li = new HtmlGenericControl("li");
            HtmlGenericControl titleDiv = new HtmlGenericControl("div");
            bool bChecked = false;
            //champs
            ulFields.Style.Add("float", "left");
            ulFields.Style.Add("width", "40%");
            li.Style.Add("margin-bottom", "10px");

            titleDiv = new HtmlGenericControl("div");
            titleDiv.ID = "editor_ConfigurationDivTitle";
            titleDiv.Attributes.Add("class", "eTitle");
            titleDiv.InnerHtml = eResApp.GetRes(Pref, 6488);
            li.Controls.Add(titleDiv);

            ulFields.Controls.Add(li);

            li = new HtmlGenericControl("li");

            li.Style.Add("height", "auto");
            li.Controls.Add(BuildConfigurableFieldsList());
            ulFields.Controls.Add(li);

            li = new HtmlGenericControl("li");

            input.ID = "editor_cuinf";
            bChecked = GetReportParam(_report, "cuinf") == "1" ? true : false;
            input.AddClick("oReport.SetParam(\"cuinf\", this.attributes[\"chk\"].value);");
            input.AddText(eResApp.GetRes(Pref, 5018));
            li.Controls.Add(input);
            ulFields.Controls.Add(li);

            //options
            ulOptions = BuildFieldsOptionlist();

            Panel pDescriptionBlock = new Panel();
            Panel pDescription = new Panel();
            pDescription.ID = "editor_fieldoptiondescription";
            pDescriptionBlock.CssClass = "optDtls";
            pDescriptionBlock.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6490)));
            pDescriptionBlock.Controls.Add(pDescription);


            pReportBase.Controls.Add(ulFields);
            pReportBase.Controls.Add(ulOptions);
            //pReportBase.Controls.Add(pDescriptionBlock);  //GCH : on masque la fonctionnalité : #34099 - DEV - XRM - CANADA - AQESSS - Détails des options assistant reporting - Masquage de la fonctionnalité

            return pReportBase;
        }

        /// <summary>
        /// Construit le menu déroulant proposant le choix des tables disponibles.
        /// La valeur sélectionnée est systématiquement l'onglet en cours.
        /// </summary>
        /// <returns>Liste déroulante des fichiers accessible sur le rapport</returns>
        private DropDownList BuildFileList()
        {
            DropDownList ddlstFiles = new DropDownList();
            ddlstFiles.ID = "editor_filelist";
            ddlstFiles.Attributes.Add("onchange", "DisplayFieldList(this);");
            foreach (KeyValuePair<String, String> kvp in this._reportWizard.AvailableFiles)
            {
                ListItem listOption = new ListItem();
                if (kvp.Key.IndexOf('_') > 0)
                    ddlstFiles.Items.Add(new ListItem(kvp.Value, kvp.Key));
                else
                    ddlstFiles.Items.Add(new ListItem(kvp.Value, String.Concat(kvp.Key, "_", kvp.Key)));

            }
            ddlstFiles.Items.Add(new ListItem(eResApp.GetRes(Pref, 6491), "0"));
            ListItem selectedItem = ddlstFiles.Items.FindByValue(String.Concat(this._iTab, "_", this._iTab));
            if (selectedItem == null)
                ddlstFiles.SelectedIndex = 0;
            else
                selectedItem.Selected = true;

            return ddlstFiles;
        }

        /// <summary>
        /// Construit le bloc d'option pour les export format texte
        /// </summary>
        /// <returns>Div contenant les options</returns>
        private HtmlGenericControl BuildTxtFormatOptions()
        {
            HtmlGenericControl pTextOptions = new HtmlGenericControl("div");
            HtmlGenericControl list = new HtmlGenericControl("ul");
            HtmlGenericControl listItem = new HtmlGenericControl("li");
            HtmlGenericControl input = new HtmlGenericControl("input");
            HtmlGenericControl label = new HtmlGenericControl("span");

            list.Attributes.Add("class", "txtformatoptionsul");
            pTextOptions.ID = "editor_txtformatoptions";
            pTextOptions.Style.Add("display", GetReportParam(_report, "format") == "1" ? "" : "none");
            pTextOptions.Attributes.Add("class", "editor_txtformatoptions");

            //libellé de l'option séparateur de texte
            label.InnerText = eResApp.GetRes(Pref, 614);
            label.Attributes.Add("class", "editor_txtformatoptions_septitlelib");

            //champ texte pour le séparateur
            input.Attributes.Add("type", "text");
            input.ID = "editor_sep";
            input.Attributes.Add("class", "editor_txtformatoptions_sep");

            input.Attributes.Add("onblur", "oReport.SetParam(\"sep\",this.value);");

            if (_report != null)
                input.Attributes.Add("value", GetReportParam(_report, "sep"));

            listItem.Controls.Add(label);
            listItem.Controls.Add(input);

            //Libellé complémentaire de l'option séparateur de texte
            label = new HtmlGenericControl("span");
            label.Attributes.Add("class", "editor_txtformatoptions_seplib");
            label.InnerText = eResApp.GetRes(Pref, 678);
            listItem.Controls.Add(label);
            list.Controls.Add(listItem);

            //seconde ligne pour l'encadrement des valeurs
            listItem = new HtmlGenericControl("li");
            label = new HtmlGenericControl("span");
            input = new HtmlGenericControl("input");

            label.InnerText = eResApp.GetRes(Pref, 615);
            label.Attributes.Add("class", "editor_txtformatoptions_sidelib");

            listItem.Controls.Add(label);

            //champ texte pour l'encadrement des valeurs
            input.Attributes.Add("type", "text");
            input.ID = "editor_side";
            input.Attributes.Add("class", "editor_txtformatoptions_side");

            input.Attributes.Add("onblur", "oReport.SetParam(\"side\",this.value);");
            input.Attributes.Add("value", GetReportParam(_report, "side"));
            listItem.Controls.Add(input);
            list.Controls.Add(listItem);
            pTextOptions.Controls.Add(list);

            return pTextOptions;
        }


        /// <summary>
        /// Construit le bloc des champs d'option du format de type PDF pour les publipostages
        /// </summary>
        /// <returns>Panel DIV contenant le code html des options</returns>
        private HtmlGenericControl BuildPdfFormatOption()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.ID = "editor_template_pdf";
            div.Attributes.Add("class", "editor_template_pdf");

            DropDownList templateList = new DropDownList();
            templateList.ID = "editor_template_pdflist";
            templateList.Attributes.Add("class", "editor_templatelist");
            templateList.Attributes.Add("name", "editor_template");
            templateList.Attributes.Add("onchange", "analyzePDF(this);");
            eCheckBoxCtrl chk = new eCheckBoxCtrl(GetReportParam(_report, "addpdf") == "1", false);
            chk.AddClick("oReport.SetParam(\"addpdf\", this.getAttribute(\"chk\"));");
            chk.AddClass("editor_template_pdf_label");
            templateList.Items.Add(new ListItem(eResApp.GetRes(Pref, 1111), ""));
            foreach (String s in eTools.GetTemplateFilesList(Pref.GetBaseName, ".pdf"))
            {
                templateList.Items.Add(s);
            }

            eButtonCtrl templateBtn = new eButtonCtrl(eResApp.GetRes(Pref, 6498), eButtonCtrl.ButtonType.GRAY, String.Concat("openTemplateDialog('pdf');"));
            templateBtn.ID = "editor_templatebutton";
            templateBtn.AddClass("editor_templatebutton");

            // Liste des fichiers PDF
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.InnerText = eResApp.GetRes(Pref, 6495);
            div.Controls.Add(span);
            div.Controls.Add(templateList);
            div.Controls.Add(templateBtn);

            // Annexer le document PDF à la fiche

            span.Attributes.Add("class", "editor_template_pdf_label");
            div.Controls.Add(chk);
            span = new HtmlGenericControl("span");
            span.InnerText = eResApp.GetRes(Pref, 1364).Replace("<TAB>", _ressources.GetRes(_iTab));
            span.Attributes.Add("class", "editor_template_pdf_label");
            div.Controls.Add(span);



            return div;
        }

        /// <summary>
        /// Construit le bloc de mapping des champs avec les champs du modèle PDF
        /// </summary>
        /// <returns>Div contenant le bloc</returns>
        private HtmlGenericControl BuildPdfTemplateFieldsMenu()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");

            div.ID = "editor_template_pdffields";
            div.Attributes.Add("class", "editor_template_pdffields");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.ID = "editor_template_pdffields_label";
            p.Attributes.Add("class", "editor_template_pdffields_label");
            p.Style.Add(HtmlTextWriterStyle.FontWeight, "bold");
            p.InnerText = eResApp.GetRes(Pref, 6867);

            div.Controls.Add(p);
            HtmlGenericControl divcols = new HtmlGenericControl("div");

            divcols.ID = "editor_template_pdfcols";
            divcols.Attributes.Add("class", "editor_template_pdfcols");

            // Géré en JS
            /*
            for (int i = 1; i <= 3; i++)
            {
                HtmlGenericControl column = new HtmlGenericControl("div");
                column.ID = String.Concat("editor_template_pdffieldCol", "_", i);
                column.Attributes.Add("class", "editor_template_pdfcol");
                HtmlGenericControl ul = new HtmlGenericControl("ul");
                ul.ID = String.Concat("editor_template_pdffields_menu", "_", i);
                ul.Attributes.Add("class", String.Concat("editor_template_pdffield_col", "_", i));
                //Champ 1
                HtmlGenericControl li = new HtmlGenericControl("li");
                li.InnerText = "champ 1";
                DropDownList select = new DropDownList();
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                li.Controls.Add(select);
                ul.Controls.Add(li);
                //Champ 2
                li = new HtmlGenericControl("li");
                li.InnerText = "champ 2";
                select = new DropDownList();
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                select.Items.Add(new ListItem(String.Concat("PDF.FIELD", i)));
                li.Controls.Add(select);
                ul.Controls.Add(li);

                //Champ 3
                li = new HtmlGenericControl("li");
                li.InnerText = "champ 3";
                select = new DropDownList();
                select.Items.Add(new ListItem("PDF.FIELD01"));
                select.Items.Add(new ListItem("PDF.FIELD02"));
                select.Items.Add(new ListItem("PDF.FIELD03"));
                li.Controls.Add(select);
                ul.Controls.Add(li);

                //Champ 4
                li = new HtmlGenericControl("li");
                li.InnerText = "champ 4";
                select = new DropDownList();
                select.Items.Add(new ListItem("PDF.FIELD01"));
                select.Items.Add(new ListItem("PDF.FIELD02"));
                select.Items.Add(new ListItem("PDF.FIELD03"));
                li.Controls.Add(select);
                ul.Controls.Add(li);
                column.Controls.Add(ul);
                divcols.Controls.Add(column);
            } */

            //TODO à répéter une fois le nombre de champs du modèle identifié et le lecteur pdf identifié également

            div.Controls.Add(divcols);


            return div;
        }

        /// <summary>
        /// Construit le bloc de définition des en-tête, pied-de-page et modèle HTML
        /// </summary>
        /// <returns>Div contenant le bloc</returns>
        private HtmlGenericControl BuildHTMLTemplateFieldsMenu()
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.ID = "editor_template_htmlfields";
            div.Attributes.Add("class", "editor_template_htmlfields");


            /*Bloc Modèle HTML*/
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.ID = "editor_html_html_templatelabel";
            span.Attributes.Add("class", "editor_template_html_editorlabel");
            span.InnerText = "Définir le modèle : ";

            HtmlGenericControl template = new HtmlGenericControl("div");
            template.ID = "editor_template_html_template";
            template.Attributes.Add("class", "editor_template_html_editor");
            template.Attributes.Add("width", "550px");
            template.Attributes.Add("height", "550px");
            div.Controls.Add(span);
            div.Controls.Add(template);

            /*Bloc En-tête HTML*/
            span = new HtmlGenericControl("span");
            span.ID = "editor_html_html_templatelabel";
            span.Attributes.Add("class", "editor_template_html_editorlabel");
            span.InnerText = "Définir un en-tête : ";

            HtmlGenericControl header = new HtmlGenericControl("div");
            header.ID = "editor_template_html_header";
            header.Attributes.Add("class", "editor_template_html_editor");
            header.Attributes.Add("width", "550px");
            header.Attributes.Add("height", "550px");

            div.Controls.Add(span);
            div.Controls.Add(header);

            /*Bloc Pied-de-page HTML*/
            span = new HtmlGenericControl("span");
            span.ID = "editor_html_html_templatelabel";
            span.Attributes.Add("class", "editor_template_html_editorlabel");
            span.InnerText = "Définir un pied-de-page : ";

            HtmlGenericControl footer = new HtmlGenericControl("div");
            footer.ID = "editor_template_html_footer";
            footer.Attributes.Add("class", "editor_template_html_editor");
            footer.Attributes.Add("width", "550px");
            footer.Attributes.Add("height", "550px");

            div.Controls.Add(span);
            div.Controls.Add(footer);

            return div;
        }

        /// <summary>
        /// Construit le bloc de mapping des champs avec les champs du modèle Word ou Open Office
        /// </summary>
        /// <returns>Div contenant le bloc</returns>
        private HtmlGenericControl BuildTextEditorTemplateMenu()
        {
            HtmlGenericControl pTemplate = new HtmlGenericControl("div");
            pTemplate.ID = "editor_template_texteditors";
            pTemplate.Attributes.Add("class", "editor_template");
            pTemplate.Style.Add("display", GetReportParam(_report, "usetemplate") == "1" ? "" : "none");

            Label lblTemplateFile = new Label();
            lblTemplateFile.CssClass = "editor_template_label";
            lblTemplateFile.ID = "editor_template_texteditors_label";

            pTemplate.Controls.Add(lblTemplateFile);

            //KHA le 01/08/2014 - il n'est pas possible de rentrer le chemin du fichier via le bouton parcourir.
            //eButtonCtrl templateBtn = new eButtonCtrl(eResApp.GetRes(Pref, 6498), eButtonCtrl.ButtonType.GRAY, String.Concat("alert(\"Liste des modèles\");"));
            //templateBtn.ID = "editor_template_texteditors_template_button";
            //templateBtn.AddClass("editor_templatebutton");


            HtmlInputText input = new HtmlInputText();
            input.ID = "editor_template_texteditors_template";
            input.Attributes.Add("class", "editor_templatelist");
            input.Attributes.Add("onchange", "oReport.SetParam(\"wordfilepath\", this.value);");
            input.Value = GetReportParam(_report, "wordfilepath")?.Replace("#AMP#", "&");
            input.Attributes.Add("placeholder", @"http:// ou \\...");

            pTemplate.Controls.Add(input);
            //pTemplate.Controls.Add(templateBtn);
            int iOffice = 0;
            if (int.TryParse(Pref.GetConfig(eLibConst.PREF_CONFIG.OFFICERELEASE), out iOffice) && iOffice == OfficeRelease.OFFICE_OPEN_OFFICE.GetHashCode())
            {

                lblTemplateFile = new Label();
                lblTemplateFile.CssClass = "editor_template_label";
                lblTemplateFile.ID = "editor_template_texteditors_output_label";
                lblTemplateFile.Text = eResApp.GetRes(Pref, 6499);

                //KHA le 01/08/2014 - il n'est pas possible de rentrer le chemin du fichier via le bouton parcourir.
                //templateBtn = new eButtonCtrl(eResApp.GetRes(Pref, 6498), eButtonCtrl.ButtonType.GRAY, String.Concat("alert(\"", eResApp.GetRes(Pref, 6500), "\");"));
                //templateBtn.ID = "editor_template_texteditors_output_button";
                //templateBtn.AddClass("editor_templatebutton");

                input = new HtmlInputText("input");
                input.ID = "editor_template_texteditors_output";
                input.Attributes.Add("class", "editor_templatelist");
                input.Attributes.Add("onchange", "oReport.SetParam(\"wordfileoutput\", this.value);");
                input.Value = GetReportParam(_report, "wordfileoutput");

                HtmlGenericControl openOfficeDiv = new HtmlGenericControl("div");
                openOfficeDiv.ID = "editor_template_texteditors_oopenoffice";
                openOfficeDiv.Controls.Add(lblTemplateFile);
                openOfficeDiv.Controls.Add(input);
                //openOfficeDiv.Controls.Add(templateBtn);
                pTemplate.Controls.Add(openOfficeDiv);
            }

            HtmlGenericControl fusionDiv = new HtmlGenericControl("div");
            fusionDiv.ID = "editor_template_fusion_section";
            fusionDiv.Attributes.Add("class", "editor_template_chk");
            eCheckBoxCtrl chkfusion = new eCheckBoxCtrl(GetReportParam(_report, "automerge") == "1", false);
            chkfusion.ID = "editor_automerge";
            chkfusion.AddClick("oReport.SetParam(\"automerge\", this.getAttribute(\"chk\"));");
            chkfusion.AddText(eResApp.GetRes(Pref, 1431));


            fusionDiv.Controls.Add(chkfusion);


            pTemplate.Controls.Add(fusionDiv);


            return pTemplate;
        }

        /// <summary>
        /// Construit le bloc de mapping des champs avec les champs du modèle Excel
        /// </summary>
        /// <returns>Div contenant le bloc</returns>
        private HtmlGenericControl BuildExcelTemplateMenu()
        {
            string reportTempalteName = string.Empty;
            HtmlGenericControl pTemplate = new HtmlGenericControl("div");
            pTemplate.ID = "editor_template_excel";
            pTemplate.Attributes.Add("class", "editor_template");
            pTemplate.Style.Add("display", GetReportParam(_report, "usetemplate") == "1" ? "" : "none");

            Label lblTemplateFile = new Label();
            lblTemplateFile.CssClass = "editor_template_label";
            lblTemplateFile.ID = "editor_template_excellabel";

            pTemplate.Controls.Add(lblTemplateFile);

            eButtonCtrl templateBtn = new eButtonCtrl(eResApp.GetRes(Pref, 6498), eButtonCtrl.ButtonType.GRAY, String.Concat("openTemplateDialog();"));
            templateBtn.ID = "editor_templatebutton";
            templateBtn.AddClass("editor_templatebutton");

            DropDownList ddlTemplates = new DropDownList();
            ddlTemplates.Attributes.Add("class", "editor_templatelist");
            ddlTemplates.Attributes.Add("onchange", "setTemplate(this)");
            ListItem item = new ListItem("", "");
            ddlTemplates.ID = "editor_template_excel_List";
            ddlTemplates.Items.Add(item);
            foreach (String templateFile in eTools.GetTemplateFilesList(Pref.GetBaseName, ".xls;.xlsx"))
            {
                item = new ListItem(templateFile, templateFile);
                if (_report != null)
                {
                    reportTempalteName = _report.GetParamValue("template");
                    if (reportTempalteName.Contains("#AMP#"))
                        reportTempalteName = reportTempalteName.Replace("#AMP#", "&");

                    if (templateFile.Equals(reportTempalteName))
                        item.Selected = true;
                }

                ddlTemplates.Items.Add(item);
            }

            pTemplate.Controls.Add(ddlTemplates);
            pTemplate.Controls.Add(templateBtn);

            //Chemin de sortie du modèle dans le cadre du publipostage OO
            //Fusion automatique dans le cadre du publipostage
            if (_reportType.Equals(TypeReport.MERGE))
            {
                eCheckBoxCtrl chkfusion = new eCheckBoxCtrl(GetReportParam(_report, "automerge") == "1", false);
                chkfusion.ID = "editor_automerge";
                chkfusion.Attributes.Add("onchange", "oReport.SetParam(\"automerge\", this.getAttribute(\"chk\"));");
                chkfusion.AddText(eResApp.GetRes(Pref, 1431));
                pTemplate.Controls.Add(chkfusion);
            }


            return pTemplate;
        }
        /// <summary>
        /// Créé le div ItemsUsed représentant le div englobant des rubriques sélectionnées pour le rapport.
        /// </summary>
        /// <returns>Controle Générique HTML de type DIV.</returns>
        private HtmlGenericControl GetUsedFields()
        {
            HtmlGenericControl usedFields = new HtmlGenericControl("div");
            usedFields.ID = "ItemsUsed";
            usedFields.Attributes.Add("ondblclick", "DelReportField();");
            usedFields.Attributes.Add("class", "ItemsUsed");

            // Création du guide de déplacement
            HtmlGenericControl listItem = new HtmlGenericControl("div");
            listItem.ID = "SelectedListElmntGuidRW";
            listItem.Attributes.Add("class", "dragGuideTab");
            listItem.Attributes.Add("syst", "");
            listItem.Style.Add("display", "none");
            usedFields.Controls.Add(listItem);

            return usedFields;
        }

        /// <summary>
        /// Construit d'option de langue du champ d'option des catalogues avancés
        /// </summary>
        /// <returns>Div contenant les composants HTML de l'option de langue</returns>
        private Panel GetFiledataLangField()
        {
            Panel pFiledataLangs = new Panel();
            pFiledataLangs.ID = "editor_popupdatalangpanel";
            pFiledataLangs.Style.Add("display", "none");
            pFiledataLangs.Style.Add("margin-left", "22px");
            pFiledataLangs.Style.Add("margin-top", "20px");
            pFiledataLangs.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 5058)));


            DropDownList ddlFiledataLangOptions = new DropDownList();
            ddlFiledataLangOptions.ID = "editor_popupdatalangoption";

            eCatalog advCat = new eCatalog();
            ddlFiledataLangOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 238), "0"));
            ddlFiledataLangOptions.Attributes.Add("onchange", "PostFileDataOption();");

            for (int langIdx = 0; langIdx < 10; langIdx++)
            {
                if (String.Format("LANG_##", langIdx).Equals(Pref.Lang))
                    continue;
                String strLangLabel = eResApp.GetRes(langIdx, 0);
                if (!String.IsNullOrEmpty(strLangLabel))
                    ddlFiledataLangOptions.Items.Add(new ListItem(strLangLabel));
            }

            pFiledataLangs.Controls.Add(ddlFiledataLangOptions);

            return pFiledataLangs;
        }

        /// <summary>
        /// Construit la liste Les champs sélectionnés pour le rapport
        /// </summary>
        /// <returns>Tableau HTML</returns>
        private HtmlTable BuildConfigurableFieldsList()
        {
            /* JAS : 17/08/2012 Recontruction côté serveur du tableau de sélection des rubriquede eFieldSelect.aspx 
                     * Voué à une refactorisation une fois l'ensemble de la page opérationnelle 
                     * */
            /*Panel pFieldSelectDiv = new Panel();
            pFieldSelectDiv.ID = "fieldOptionDiv";
            pFieldSelectDiv.CssClass = "fieldSelectDiv";*/

            HtmlTable MainTable = new HtmlTable();
            MainTable.ID = "editor_field_configuration";
            MainTable.Attributes.Add("cellpadding", "0");
            MainTable.Attributes.Add("cellspacing", "0");
            MainTable.Style.Add("width", "100%");
            MainTable.Style.Add("height", "200px");
            MainTable.Style.Add("padding-bottom", "15px");

            HtmlTableRow row = new HtmlTableRow();
            HtmlTableCell cell = new HtmlTableCell();
            row.Controls.Add(cell);
            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Attributes.Add("width", "38%");
            HtmlGenericControl Div = new HtmlGenericControl("div");
            row.Controls.Add(cell);
            MainTable.Controls.Add(row);

            row = new HtmlTableRow();

            //Rubriques sélectionnées
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "tdHeader");
            cell.InnerHtml = eResApp.GetRes(Pref, 6230);
            row.Controls.Add(cell);

            MainTable.Controls.Add(row);

            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Attributes.Add("class", "ItemListTd");

            Div = new HtmlGenericControl("div");
            Div.ID = "editor_configurablelist";
            Div.Attributes.Add("class", "ItemList");
            // #33 098, #32 972, #33 601, #33 620 - Redimensionnement des listes de rubriques - Calcul de la hauteur à donner aux listes en fonction de celle de la fenêtre
            // 25 pixels réservés pour la barre de titre
            // 125 pixels pour la partie haute (rail de navigation + combobox de sélection du fichier de rubriques)
            // 35 pixels pour l'entête des listes ("Rubriques disponibles" - "Rubriques sélectionnées")
            // 125 pixels pour les options du bas ("Ne retenir que les fiches Adresse Actives / Principales")
            // 60 pixels d'espace réservé aux boutons de la modal dialog
            // A ajuster si des options se rajoutent sur la fenêtre par la suite
            int itemListHeight = _iheight - 25 - 125 - 35 - 125 - 60;
            Div.Style.Add(HtmlTextWriterStyle.Height, String.Concat(itemListHeight, "px"));

            cell.Controls.Add(Div);
            row.Controls.Add(cell);

            MainTable.Controls.Add(row);

            return MainTable;
        }

        /// <summary>
        /// Construit le bloc HTML UL des options de champs
        /// </summary>
        /// <returns>Composant UL contenant les LI d'options de champs</returns>
        private HtmlGenericControl BuildFieldsOptionlist()
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            HtmlGenericControl ulOptions = new HtmlGenericControl("ul");
            eCheckBoxCtrl input = new eCheckBoxCtrl(false, false);

            ulOptions.ID = "editor_fieldoptions";
            ulOptions.Style.Add("margin-left", "30px");
            ulOptions.Style.Add("float", "left");

            li = new HtmlGenericControl("li");
            li.Controls.Add(new LiteralControl("line tmp"));
            li.Style.Add("visibility", "hidden");

            ulOptions.Controls.Add(li);

            li = new HtmlGenericControl("li");
            li.ID = "editor_fieldlabelline";
            HtmlGenericControl divFieldLabel = new HtmlGenericControl("div");
            divFieldLabel.ID = "editor_fieldlabel";
            li.Controls.Add(divFieldLabel);
            HtmlGenericControl descIdInput = new HtmlGenericControl("input");
            descIdInput.Style.Add("display", "none");
            descIdInput.ID = "editor_currentfielddescid";
            descIdInput.Attributes.Add("type", "text");

            li.Style.Add("line-height", "30px");
            li.Style.Add("font-Weight", "bold");
            li.InnerText = "";
            li.Controls.Add(divFieldLabel);
            li.Controls.Add(descIdInput);

            ulOptions.Controls.Add(li);

            #region Comptez le nombre d'occurence
            li = new HtmlGenericControl("li");
            li.ID = "editor_countoption";
            input = new eCheckBoxCtrl(false, false);
            input.ID = "editor_count";
            input.AddText(eResApp.GetRes(Pref, 624));
            input.AddClick("PostComplexValue(\"count\");");
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "1;2;3;4;5;6;7;8;9;10;11;12;13;14");
            ulOptions.Controls.Add(li);
            #endregion

            #region Tronquer les valeurs
            li = new HtmlGenericControl("li");
            li.ID = "editor_truncateoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostTruncateOption();");
            input.AddText(eResApp.GetRes(Pref, 625));
            li.Controls.Add(input);
            input.ID = "editor_truncate";

            HtmlGenericControl txtInput = new HtmlGenericControl("input");
            txtInput.ID = "editor_truncatechar";
            txtInput.Attributes.Add("type", "search");
            txtInput.Style.Add("margin-left", "10px");
            txtInput.Style.Add("width", "40px");
            txtInput.Attributes.Add("onblur", "PostTruncateOption();");
            li.Controls.Add(txtInput);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "1");
            ulOptions.Controls.Add(li);
            #endregion

            #region Masquer le libellé
            li = new HtmlGenericControl("li");
            li.ID = "editor_adrpersooption";
            li.Attributes.Add("compatibleformats", "1");
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("oReport.SetParam(\"adrperso\", this.getAttribute(\"chk\") == 1 ? document.getElementById(\"editor_currentfielddescid\").value: \"\");");
            input.ID = "editor_adrperso";
            input.AddText(eResApp.GetRes(Pref, 626));
            li.Controls.Add(input);

            li.Style.Add("display", "none");
            ulOptions.Controls.Add(li);
            #endregion

            #region Ne pas renvoyer à la ligne automatiquement
            li = new HtmlGenericControl("li");
            li.ID = "editor_nwoption";
            input = input = new eCheckBoxCtrl(false, false);
            input.Attributes.Add("width", "30px");
            input.Attributes.Add("type", "checkbox");
            input.AddClick("PostComplexValue(\"nw\");");
            input.ID = "editor_nw";
            input.AddText(eResApp.GetRes(Pref, 969));

            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "1");
            ulOptions.Controls.Add(li);
            #endregion

            #region Exporter (catalogues avancés)
            li = new HtmlGenericControl("li");
            // li.Attributes.Add("class", "FiledataLi");
            li.Style.Add("display", "none");
            li.ID = "editor_popupdataoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("ManageFileDataOptions();");
            input.ID = "editor_popupdata";
            input.AddText(string.Concat(eResApp.GetRes(Pref, 16), " :"));
            li.Controls.Add(input);
            li.Attributes.Add("compatibleformats", "");
            DropDownList ddlFiledataOptions = new DropDownList();
            ddlFiledataOptions.ID = "editor_popupdatadisplayoption";
            ddlFiledataOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 1193), "1"));
            ddlFiledataOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 1194), "2"));
            ddlFiledataOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 1614), "3"));
            ddlFiledataOptions.Attributes.Add("onchange", "ManageFileDataOptions();");
            ddlFiledataOptions.Enabled = false;


            li.Controls.Add(ddlFiledataOptions);
            li.Controls.Add(new LiteralControl("<br />"));
            li.Controls.Add(GetFiledataLangField());

            ulOptions.Controls.Add(li);
            #endregion

            #region Ajouter les rubriques du fichier liaison
            li = new HtmlGenericControl("li");
            li.ID = "editor_scinfosoption";
            li.Attributes.Add("compatibleformats", "1");
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"scinfos\");");
            input.ID = "editor_scinfos";
            input.AddText(eResApp.GetRes(Pref, 1424));
            li.Controls.Add(input);
            /*      Label lblLink = new Label();
                  lblLink.ID = "editor_linkedfieldlabel";
                  li.Controls.Add(lblLink);*/
            li.Style.Add("display", "none");
            ulOptions.Controls.Add(li);
            #endregion

            #region Modifier l'affichage de la rubrique

            li = new HtmlGenericControl("li");
            li.ID = "editor_bitlabeloption";
            li.Style.Add("display", "none");
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("ManageCheckBox(this);PostCheckBoxOption();");
            input.ID = "editor_bitlabel";
            input.AddText(eResApp.GetRes(Pref, 6508));
            li.Attributes.Add("compatibleformats", "3");
            li.Controls.Add(input);
            DropDownList CheckBoxOptions = new DropDownList();
            CheckBoxOptions.ID = "editor_bitlabeldisplayoption";
            CheckBoxOptions.Attributes.Add("onchange", "PostCheckBoxOption();");
            CheckBoxOptions.Style.Add("margin-left", "22px");
            CheckBoxOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 1287), "0"));  //Masquer les libellés 'Non'
            CheckBoxOptions.Items.Add(new ListItem(eResApp.GetRes(Pref, 1288), "1"));  //Afficher les cases à cocher
            CheckBoxOptions.Enabled = false;
            li.Controls.Add(CheckBoxOptions);

            ulOptions.Controls.Add(li);
            #endregion

            #region Ne pas prendre en compte le jour
            li = new HtmlGenericControl("li");
            li.ID = "editor_monthyearoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"monthyear\");");
            input.ID = "editor_monthyear";
            input.AddText(eResApp.GetRes(Pref, 1026));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "2");
            ulOptions.Controls.Add(li);
            #endregion

            #region Ne pas prendre en compte les heures
            li = new HtmlGenericControl("li");
            li.ID = "editor_dateonlydayooption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"dateonlyday\");");
            input.ID = "editor_dateonlyday";
            input.AddText(eResApp.GetRes(Pref, 1423));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "2");
            ulOptions.Controls.Add(li);
            #endregion

            #region Ne pas prendre en compte l'année
            li = new HtmlGenericControl("li");
            li.ID = "editor_noyeardateoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"noyeardate\");");
            input.ID = "editor_noyeardate";
            input.AddText(eResApp.GetRes(Pref, 1496));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "2");
            ulOptions.Controls.Add(li);
            #endregion

            #region Afficher la valeur minimum
            li = new HtmlGenericControl("li");
            li.ID = "editor_minoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"min\");");
            input.ID = "editor_min";
            input.AddText(eResApp.GetRes(Pref, 622));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Afficher la valeur Maximum
            li = new HtmlGenericControl("li");
            li.ID = "editor_maxoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"max\");");
            input.ID = "editor_max";
            input.AddText(eResApp.GetRes(Pref, 623));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Calculer la somme des valeurs
            li = new HtmlGenericControl("li");
            li.ID = "editor_sumoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"sum\");");
            input.ID = "editor_sum";
            input.AddText(eResApp.GetRes(Pref, 620));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Calculer la moyenne des valeurs
            li = new HtmlGenericControl("li");
            li.ID = "editor_avgoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"avg\");");
            input.ID = "editor_avg";
            input.AddText(eResApp.GetRes(Pref, 621));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Tenir compte des valeurs nulles
            li = new HtmlGenericControl("li");
            li.ID = "editor_nullvalueoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"nullvalue\");");
            input.ID = "editor_nullvalue";
            input.AddText(eResApp.GetRes(Pref, 1469));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Afficher en toutes lettres le nombre
            li = new HtmlGenericControl("li");
            li.ID = "editor_digittocharoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"digittochar\");");
            input.ID = "editor_digittochar";
            input.AddText(eResApp.GetRes(Pref, 1719));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "5;10");
            ulOptions.Controls.Add(li);
            #endregion

            #region Ajouter le détail de l'utilisateur
            li = new HtmlGenericControl("li");
            li.ID = "editor_uinfoption";
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"uinf\");");
            input.ID = "editor_uinf";
            input.AddText(eResApp.GetRes(Pref, 677));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            li.Attributes.Add("compatibleformats", "8");
            ulOptions.Controls.Add(li);
            #endregion

            #region Concaténation du nom et du prénom
            li = new HtmlGenericControl("li");
            li.ID = "editor_concateoption";
            li.Attributes.Add("compatibleformats", "1");
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"concate\");");
            input.ID = "editor_concate";

            input.AddText(eResApp.GetRes(Pref, 996).Replace("<ITEM1>", _ressources.GetRes(201)).Replace("<ITEM2>", _ressources.GetRes(202)));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            ulOptions.Controls.Add(li);
            #endregion

            #region Concaténer la particule
            li = new HtmlGenericControl("li");
            li.ID = "editor_particuleoption";
            li.Attributes.Add("compatibleformats", "1");
            input = new eCheckBoxCtrl(false, false);
            input.AddClick("PostComplexValue(\"particule\");");
            input.ID = "editor_particule";
            input.AddText(eResApp.GetRes(Pref, 6517).Replace("<ITEM1>", _ressources.GetRes(203)));
            li.Controls.Add(input);
            li.Style.Add("display", "none");
            ulOptions.Controls.Add(li);
            #endregion

            return ulOptions;

        }

        /// <summary>
        /// Charge le bloc dédié aux filtres sur les rapports
        /// </summary>
        /// <returns>Element UL</returns>
        private HtmlGenericControl BuildFilterList()
        {
            HtmlGenericControl ulFilterList = new HtmlGenericControl("ul");
            ulFilterList.ID = "editor_filterinfos";
            HtmlGenericControl li = new HtmlGenericControl("li");
            bool checkedOption = false;
            string sOpt = GetReportParam(_report, "addcurrentfilter");
            checkedOption = GetReportParam(_report, "addcurrentfilter") == "0" ? false : true;
            eCheckBoxCtrl chkAddCurrentFilter = new eCheckBoxCtrl(checkedOption, false);
            DropDownList ddlFilters = new DropDownList();
            chkAddCurrentFilter.ID = "editor_addcurrentfilter";
            chkAddCurrentFilter.AddClick("oReport.SetParam(\"addcurrentfilter\", this.getAttribute(\"chk\"));");
            chkAddCurrentFilter.AddText(eResApp.GetRes(Pref, 6509));
            ddlFilters.ID = "editor_fitlerid";
            ddlFilters.Style.Add("width", "500px");
            ddlFilters.Attributes.Add("onchange", "oReport.SetParam(\"filterid\",this.value);");

            ddlFilters.Items.Add(new ListItem(eResApp.GetRes(Pref, 430), ""));
            int ListIndex = 1;
            int selectedIndex = 0;
            foreach (KeyValuePair<int, String> kvp in _reportWizard.RelatedFilters)
            {
                if (GetReportParam(_report, "filterid") == kvp.Key.ToString())
                    selectedIndex = ListIndex;
                ddlFilters.Items.Add(new ListItem(kvp.Value, kvp.Key.ToString()));
                ListIndex++;
            }
            ddlFilters.SelectedIndex = selectedIndex;
            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6511)));
            ulFilterList.Controls.Add(li);

            li = new HtmlGenericControl("li");
            li.Controls.Add(ddlFilters);
            ulFilterList.Controls.Add(li);

            // Pour Power BI, pas d'option de cumul des filtres en cours - cf. captures d'écran dans les spécs
            // Ce contrôle sera donc affiché/masqué en JS à la sélection des boutons radio
            li = new HtmlGenericControl("li");
            li.Controls.Add(chkAddCurrentFilter);

            ulFilterList.Controls.Add(li);

            return ulFilterList;
        }

        /// <summary>
        /// Construit la liste de tri/regroupement secondaire (croissant/décroissant)
        /// </summary>
        /// <returns>Liste déroulante</returns>
        private DropDownList GetFinalOptionsDropDownList(int SelectedValue)
        {
            DropDownList list = new DropDownList();
            list.Items.Add(new ListItem(eResApp.GetRes(Pref, 158), "0"));
            list.Items.Add(new ListItem(eResApp.GetRes(Pref, 159), "1"));
            list.SelectedIndex = SelectedValue;

            return list;
        }

        /// <summary>
        /// Construit le bloc dédié aux options de tri sur les rapports
        /// </summary>
        /// <returns>element UL</returns>
        private HtmlGenericControl BuildSortMenu()
        {
            // Pour Power BI, pas d'option de cumul des filtres en cours - cf. captures d'écran dans les spécs
            // Ce contrôle sera donc affiché/masqué en JS à la sélection des boutons radio

            HtmlGenericControl ulSortMenu = new HtmlGenericControl("ul");
            DropDownList ddl;
            HtmlGenericControl li = new HtmlGenericControl("li");
            ulSortMenu.ID = "editor_sortinfos";
            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6512)));

            ulSortMenu.Controls.Add(li);
            int SortValue = 0;
            for (int idx = 1; idx <= 3; idx++)
            {
                li = new HtmlGenericControl("li");
                li.ID = String.Concat("editor_sortinfos", idx);
                li.Style.Add("margin-left", "33px");
                ddl = new DropDownList();

                ddl.ID = String.Concat("editor_orderby", idx);
                ddl.Style.Add("width", "200px");
                ddl.Style.Add("margin-right", "30px");
                ddl.Style.Add("margin-left", "5px");
                ddl.Attributes.Add("index", idx.ToString());
                ddl.Attributes.Add("onchange", "ManageOptionLine(this,\"sort\");PostSortOrGroupOption(\"sort\"," + idx.ToString() + ");");
                li.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 169), idx, " : ")));
                li.Controls.Add(ddl);
                li.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 161), " : ")));


                int.TryParse(GetReportParam(_report, String.Concat("ordersort", idx.ToString())), out SortValue);
                ddl = GetFinalOptionsDropDownList(SortValue);
                ddl.ID = String.Concat("editor_ordersort", idx);
                ddl.Style.Add("width", "200px");
                ddl.Style.Add("margin-right", "30px");
                ddl.Style.Add("margin-left", "5px");
                ddl.Attributes.Add("onchange", "PostSortOrGroupOption(\"sort\"," + idx.ToString() + ");");
                ddl.Attributes.Add("index", idx.ToString());
                li.Controls.Add(ddl);
                ulSortMenu.Controls.Add(li);
            }

            return ulSortMenu;
        }

        /// <summary>
        /// Construit le bloc dédié aux options de regroupements sur les rapports
        /// </summary>
        /// <returns>element UL</returns>
        private HtmlGenericControl BuildGroupMenu()
        {
            HtmlGenericControl ulSortMenu = new HtmlGenericControl("ul");
            DropDownList ddl;
            HtmlGenericControl li = new HtmlGenericControl("li");
            eCheckBoxCtrl checkBox = null;
            ulSortMenu.ID = "editor_groupinfos";

            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6513)));
            ulSortMenu.Controls.Add(li);
            int SortValue = 0;

            for (int idx = 1; idx <= 3; idx++)
            {
                li = new HtmlGenericControl("li");
                li.ID = String.Concat("editor_groupinfos", idx);
                li.Style.Add("margin-left", "6px");
                ddl = new DropDownList();
                ddl.ID = String.Concat("editor_group", idx);
                ddl.Style.Add("width", "200px");
                ddl.Style.Add("margin-right", "30px");
                ddl.Style.Add("margin-left", "5px");
                ddl.Attributes.Add("index", idx.ToString());
                ddl.Attributes.Add("onchange", "ManageOptionLine(this,\"group\");PostSortOrGroupOption(\"group\"," + idx.ToString() + ");");
                li.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 160), idx, " : ")));
                li.Controls.Add(ddl);
                li.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 161), " : ")));

                int.TryParse(GetReportParam(_report, String.Concat("grouporder", idx.ToString())), out SortValue);

                ddl = GetFinalOptionsDropDownList(SortValue);
                ddl.ID = String.Concat("editor_grouporder", idx);
                ddl.Style.Add("width", "200px");
                ddl.Style.Add("margin-right", "30px");
                ddl.Style.Add("margin-left", "5px");
                ddl.Attributes.Add("onchange", "PostSortOrGroupOption(\"group\"," + idx.ToString() + ");");
                ddl.Attributes.Add("index", idx.ToString());
                li.Controls.Add(ddl);

                checkBox = new eCheckBoxCtrl(GetReportParam(_report, String.Concat("grouppagebreak", idx)) == "1", false);
                checkBox.ID = String.Concat("editor_grouppagebreak", idx);
                checkBox.AddClick(String.Concat("oReport.SetParam(\"grouppagebreak", idx, "\", this.getAttribute(\"chk\"));"));
                checkBox.AddText(eResApp.GetRes(Pref, 613));

                li.Controls.Add(checkBox);

                ulSortMenu.Controls.Add(li);
            }

            //Masquer les ligne de détails
            li = new HtmlGenericControl("li");
            li.Style.Add("margin-left", "85px");
            li.ID = "editor_hiddendetailinfos";
            checkBox = new eCheckBoxCtrl(GetReportParam(_report, "hiddendetail") == "1", false);
            checkBox.ID = "editor_hiddendetail";
            checkBox.AddClick("oReport.SetParam(\"hiddendetail\", this.getAttribute(\"chk\"));");
            checkBox.AddText(eResApp.GetRes(Pref, 1101));
            li.Controls.Add(checkBox);
            ulSortMenu.Controls.Add(li);
            return ulSortMenu;
        }

        /// <summary>
        /// Construit le bloc dédié aux permissions sur les rapports
        /// </summary>
        /// <returns>element UL</returns>
        protected virtual void BuildHistoMenu(HtmlGenericControl ul)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            bool checkedOption = false;
            eCheckBoxCtrl checkBox = new eCheckBoxCtrl(false, false);
            li = new HtmlGenericControl("li");
            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 17)));

            ul.Controls.Add(li);

            checkedOption = GetReportParam(_report, "histo") == "1" ? true : false;
            li = new HtmlGenericControl("li");
            li.Style.Add("margin-left", "50px");
            checkBox = new eCheckBoxCtrl(checkedOption, false);
            checkBox.ID = "editor_chkhisto";
            checkBox.AddClick("oReport.SetParam(\"histo\",this.getAttribute(\"chk\"));");
            checkBox.AddText(eResApp.GetRes(Pref, 816));
            li.Controls.Add(checkBox);

            ul.Controls.Add(li);

            checkedOption = _report != null && !String.IsNullOrEmpty(_report.EndProcedure) ? true : false;
            li = new HtmlGenericControl("li");
            li.Style.Add("margin-left", "50px");
            li.ID = "editor_endprocinfo";
            li.Style.Add("display", "none");
            checkBox = new eCheckBoxCtrl(checkedOption, false);
            checkBox.ID = "editor_chkendproc";
            checkBox.AddClick("ManageEndProcInputDisplay(this);");
            checkBox.AddText(eResApp.GetRes(Pref, 1700));
            li.Controls.Add(checkBox);

            ul.Controls.Add(li);
            li = new HtmlGenericControl("li");
            li.ID = "editor_endprocedurebloc";
            li.Style.Add("margin-left", "50px");
            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6514)));

            HtmlGenericControl input = new HtmlGenericControl("input");
            input.ID = "editor_endprocedure";
            input.Attributes.Add("type", "text");
            input.Attributes.Add("value", _report == null ? "" : _report.EndProcedure);
            input.Attributes.Add("onblur", "oReport.SetEndProcedure(this.value);");
            input.Style.Add("margin-left", "15px");
            input.Style.Add("background-color", "white");
            input.Style.Add("border", "solid 1px black");

            li.Style.Add("display", checkedOption ? "inline" : "none");

            li.Controls.Add(input);

            ul.Controls.Add(li);
        }

        /// <summary>
        /// Construit le bloc de nommage du rapport et de qualification en rapport public
        /// </summary>
        /// <returns>Controle Générique HTML de type ul.</returns>
        private HtmlGenericControl BuildReportTitleMenu()
        {
            HtmlGenericControl ulNameMenu = new HtmlGenericControl("ul");
            bool bModifyTitle = false;
            bModifyTitle = GetReportParam(_report, "modifytitle") == "1" ? true : false;
            eCheckBoxCtrl chkModifyTitle = new eCheckBoxCtrl(bModifyTitle, false);
            HtmlGenericControl li = new HtmlGenericControl("li");

            HtmlGenericControl input = new HtmlGenericControl("input");
            input.ID = "editor_modifytitle";
            input.Attributes.Add("class", "editor_reportnameinput");
            input.Attributes.Add("type", "text");
            input.Attributes.Add("value", GetReportParam(_report, "title"));
            input.Attributes.Add("onchange", "oReport.SetParam(\"title\", this.value);");
            input.Attributes.Add("onblur", "oReport.SetParam(\"title\", this.value);");

            ulNameMenu.ID = "editor_reporttitleinfos";
            li.Attributes.Add("class", "reportnameli");

            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 172) + " :"));

            ulNameMenu.Controls.Add(li);

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "reportinputli");
            li.Controls.Add(input);

            Label lblspace = new Label();
            lblspace.ID = "editor_publicspace";
            lblspace.Attributes.Add("class", "editor_publicspace");

            chkModifyTitle.ID = "editor_public";
            chkModifyTitle.AddClick("oReport.SetParam(\"modifytitle\",this.getAttribute(\"chk\"));");
            chkModifyTitle.AddText(eResApp.GetRes(Pref, 721));
            lblspace.Controls.Add(chkModifyTitle);
            li.Controls.Add(lblspace);

            ulNameMenu.Controls.Add(li);

            return ulNameMenu;
        }


        /// <summary>
        /// Construit le bloc de nommage du rapport et de qualification en rapport public
        /// </summary>
        /// <returns>Controle Générique HTML de type ul.</returns>
        private HtmlGenericControl BuildReportNameMenu()
        {
            HtmlGenericControl ulNameMenu = new HtmlGenericControl("ul");
            bool bChecked = false;
            bChecked = GetReportParam(_report, "public") == "1" && _report.Owner == 0 ? true : false;
            eCheckBoxCtrl chkPublic = new eCheckBoxCtrl(bChecked, false);
            HtmlGenericControl li = new HtmlGenericControl("li");

            HtmlGenericControl input = new HtmlGenericControl("input");
            input.ID = "editor_saveas";
            input.Attributes.Add("class", "editor_reportnameinput");
            input.Attributes.Add("type", "text");
            input.Attributes.Add("value", GetReportParam(_report, "saveas"));
            input.Attributes.Add("onchange", "oReport.SetParam(\"saveas\", this.value);");
            input.Attributes.Add("onblur", "oReport.SetParam(\"saveas\", this.value);");

            //MOU simplifier l interaction avec l utilisateur, on ajoute le titre preselectionné dans le input du nom de rapport;
            input.Attributes.Add("onclick", "this.select();");
            input.Attributes.Add("onfocus", "if(this.value.length == 0) this.value = oReport.GetParam('title');");

            ulNameMenu.ID = "editor_reportnameinfos";
            li.Attributes.Add("class", "reportnameli");
            switch (_reportType)
            {
                case TypeReport.EXPORT:
                    // 6515 - Enregistrer le modèle d'export sous :
                    li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6515)));
                    break;
                case TypeReport.MERGE:
                    // 6510 - Enregistrer le modèle de publipostage sous :
                    li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6510)));
                    break;
                case TypeReport.PRINT:
                    // 6507 - Enregistrer le modèle d'impression sous :
                    li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 6507)));
                    break;
                default:
                    // 173 - Enregistrer le rapport sous
                    li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 173)));
                    break;
            }
            ulNameMenu.Controls.Add(li);

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "reportinputli");
            li.Controls.Add(input);

            //BSE : #48 291 - on active la case à cocher Public seulement si on a le droit 
            //BSE : #64 140 - droits spécifiques pour les rapports de type Graphique
            //MAB : #64 140 - pas de vérification du type de graphique si on en ajoute un nouveau (_report est alors null)
            TypeReport fictifType = _report?.ReportType ?? TypeReport.NONE;

            bool drawPublicNode = false;
            if (fictifType == TypeReport.CHARTS)
                drawPublicNode = _oRightManager.CanEditItem();
            else
                drawPublicNode = _oRightManager.HasRight(eLibConst.TREATID.PUBLIC_EXPORT_REPORT);

            if (drawPublicNode)
            {
                Label lblspace = new Label();
                lblspace.ID = "editor_publicspace";
                lblspace.Attributes.Add("class", "editor_publicspace");

                chkPublic.ID = "editor_public";
                chkPublic.AddClick("oReport.SetParam(\"public\",this.getAttribute(\"chk\"));");
                chkPublic.AddText(eResApp.GetRes(Pref, 618));
                lblspace.Controls.Add(chkPublic);
                li.Controls.Add(lblspace);
            }

            ulNameMenu.Controls.Add(li);

            return ulNameMenu;
        }


        /// <summary>
        /// Construit le bloc des options de permissions sur le rapport
        /// </summary>
        /// <param name="itemPrefix"></param>
        /// <returns>Controle Générique HTML de type ul.</returns>
        private HtmlGenericControl BuildPermissionMenu(String itemPrefix)
        {
            HtmlGenericControl ulwrap = new HtmlGenericControl("ul");
            HtmlGenericControl li = new HtmlGenericControl("li");
            HtmlGenericControl span = new HtmlGenericControl("span");
            HtmlGenericControl input = new HtmlGenericControl("input");
            eIconCtrl userCatalogIconButton = new eIconCtrl();

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "liSecurityLbl");
            li.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 206)));
            ulwrap.Controls.Add(li);


            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ulwrap.Controls.Add(ul);
            ul.Attributes.Add("style", "margin-left: 50px;");
            ul.ID = String.Concat(itemPrefix, "_", "permissioninfos");

            ePermissionRenderer.GetHtmlRender(_report != null && !_report.IsNew ? _report.ViewPerm : null, ePermissionRenderer.PermType.VIEW, Pref, ul, "li");
            ePermissionRenderer.GetHtmlRender(_report != null && !_report.IsNew ? _report.UpdatePerm : null, ePermissionRenderer.PermType.UPDATE, Pref, ul, "li");
            /*
            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);*/
            AddActivateConditonalSendingOption(ul, itemPrefix);
            BuildHistoMenu(ul);
            return ulwrap;
        }



        /// <summary>
        /// Ajoute la ligne de plannification
        /// </summary>
        /// <returns></returns>
        protected HtmlGenericControl BuildScheduleMenu()
        {

            HtmlGenericControl ulNameMenu = new HtmlGenericControl("ul");
            HtmlGenericControl li = new HtmlGenericControl("li");


            string sScheduleParam = "";
            int nOwnerId = 0;
            int reportid = 0;
            if (_report != null)
            {
                sScheduleParam = _report.ScheduleParam;
                nOwnerId = _report.Owner;
                reportid = _report.Id;

            }

            // Titre
            li = new HtmlGenericControl("li");
            ulNameMenu.ID = "editor_reportschedule";
            li.Attributes.Add("class", "reportnameli");
            li.Controls.Add(new LiteralControl(
                String.Concat(
                eResApp.GetRes(Pref, 6907)
                )

                ));
            ulNameMenu.Controls.Add(li);



            //Contrôle caché pour les param plannif
            HtmlGenericControl inputHiddenParam = new HtmlGenericControl("input");
            inputHiddenParam.ID = "editor_scheduleparam";
            inputHiddenParam.Attributes.Add("type", "hidden");
            inputHiddenParam.Attributes.Add("value", sScheduleParam);
            li.Controls.Add(inputHiddenParam);


            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "reportinputli");
            ulNameMenu.Controls.Add(li);

            //
            Label lblspace = new Label();
            lblspace.ID = "editor_schedulespace";
            lblspace.Attributes.Add("class", "editor_publicspace");
            li.Controls.Add(lblspace);

            bool bHasSchedule = sScheduleParam.Length > 0;


            //Case à cocher pour activer la planif 
            eCheckBoxCtrl chkHasSchedule = new eCheckBoxCtrl(bHasSchedule, nOwnerId != Pref.UserId);
            chkHasSchedule.ID = "report_schedule_enabled";

            chkHasSchedule.AddClick("oReport.SetIsScheduled( this.getAttribute(\"chk\"), true);");

            chkHasSchedule.AddText(eResApp.GetRes(Pref, 6887) + String.Concat("  (", eResApp.GetRes(Pref, 6921), ")"));
            lblspace.Controls.Add(chkHasSchedule);

            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "reportinputli");
            ulNameMenu.Controls.Add(li);
            lblspace = new Label();
            lblspace.ID = "editor_schedulespace2";
            lblspace.Attributes.Add("class", "editor_publicspace");
            li.Controls.Add(lblspace);


            Label lnk = new Label();
            lnk.Text = eResApp.GetRes(Pref, 6888);
            lnk.ID = "editor_schedulemanage";



            lnk.Attributes.Add("class", String.Concat("gofile editor_publicspace", bHasSchedule ? "" : " disabled"));
            lnk.Attributes.Add("onclick", "nsFilterReportList.onPlanifyWiz(" + reportid + ")");
            li.Controls.Add(lnk);

            /*
            eIconCtrl userCatalogIconButton = new eIconCtrl();
            userCatalogIconButton.AddClass("icon-catalog");
            userCatalogIconButton.AddClass("btn");
            userCatalogIconButton.Attributes.Add("onclick", "nsFilterReportList.onPlanifyWiz(" + _report.Id + ")");
            li.Controls.Add(userCatalogIconButton);*/

            //seulement pour les impression/export/publipostage
            if (

                !_reportType.Equals(TypeReport.PRINT)
                && !_reportType.Equals(TypeReport.EXPORT)
                && !_reportType.Equals(TypeReport.MERGE)

                )
                ulNameMenu.Style.Add("display", "none");


            return ulNameMenu;

        }

        /// <summary>
        /// Ajoute les options liées à Power BI
        /// </summary>
        /// <returns>Le contrôle généré</returns>
        protected HtmlGenericControl BuildPowerBIMenu()
        {
            int reportId = Report != null ? Report.Id : 0;
            eReportInformationRenderer er = eReportInformationRenderer.GetReportInformationRenderer(Pref, reportId, true);
            return (HtmlGenericControl)er.PgContainer.Controls[0];
        }

        /// <summary>
        /// option d'envois conditionnel
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="itemPrefix"></param>
        protected virtual void AddActivateConditonalSendingOption(HtmlGenericControl ul, String itemPrefix)
        {
            //Envois conditionnels (Reports)
            HtmlGenericControl li = new HtmlGenericControl("li");
            eCheckBoxCtrl checkBox = new eCheckBoxCtrl(false, false);
            checkBox.ID = String.Concat(itemPrefix, "_", "chkruleperm");
            checkBox.AddClick("");
            checkBox.AddText(eResApp.GetRes(Pref, 1325));
            li.Controls.Add(checkBox);
            //TODO LISTE DES REGLES ET VALIDATION
            eIconCtrl userCatalogIconButton = new eIconCtrl();
            userCatalogIconButton.AddClass("icon-catalog");
            userCatalogIconButton.AddClass("btn");
            userCatalogIconButton.Attributes.Add("onclick", String.Concat("alert(\"Liste des règles\");")); //TODO
            li.Controls.Add(userCatalogIconButton);
            //ul.Controls.Add(li); TODO
        }

        /// <summary>
        /// Construit une list de menus , un pour chaque table, contenant la liste des champs de la table.
        /// A la génération on affecte un display : none à tous les éléments ne se rapportant pas à l'onglet en cours., 
        /// valeur par défaut de la liste déroulante des fichiers. (voir "buildfileList")
        /// </summary>
        /// <returns>Liste de Div contenant les champs de chaque fichier disponible</returns>
        private List<HtmlGenericControl> BuildFieldList()
        {
            List<HtmlGenericControl> fieldsList = new List<HtmlGenericControl>();
            HtmlGenericControl ddlstFields;
            bool displayElement = false;
            int tabDescId = 0;
            int fromTabDescId = 0;



            foreach (KeyValuePair<String, String> kvp in this._reportWizard.AvailableFiles)
            {

                if (!int.TryParse(kvp.Key.Split('_')[0], out tabDescId))
                    continue;

                if (kvp.Key.IndexOf('_') > 0)
                    int.TryParse(kvp.Key.Split('_')[1], out fromTabDescId);

                //On affiche le div si c'est le premier de la liste et qu'il n'est pas lié depuis
                //Pour éviter par exemple : 
                //Onglet contact
                //Onglet contact lié depuis Affaire
                //Affichage des champs de contact et de "contact depuis affaire" à la suite sur la sélection de contact.

                displayElement = (!displayElement && tabDescId == this._iTab && (fromTabDescId == 0));
                List<eReportWizardField> tabFields = this._reportWizard.Fields.FindAll(
                        delegate (eReportWizardField fld)
                        {
                            return fld.Tab.Equals(tabDescId) && fld.LinkedFromTab.Equals(fromTabDescId);
                        }
                    );
                //Appelle à la static (voir région de méthodes statiques de rendu) 
                ddlstFields = eReportWizardRenderer.BuildFieldList(kvp, tabFields, displayElement);


                fieldsList.Add(ddlstFields);
            }

            return fieldsList;
        }


        /// <summary>
        /// Construit le panel d'enregistrement du rapport graphique
        /// </summary>
        /// <returns></returns>
        protected Panel BuildRecordingPanel()
        {
            Panel contentPanel = new Panel();
            contentPanel.CssClass = "reportFinalize";

            // Regroupements (uniquement en impression)
            if (_reportType.Equals(TypeReport.PRINT))
                contentPanel.Controls.Add(BuildReportTitleMenu());

            // Enregistrer le modèle d'export sous
            contentPanel.Controls.Add(BuildReportNameMenu());

            // Source de données pour Power BI
            contentPanel.Controls.Add(BuildPowerBIMenu());

            // Envoi planifié
            contentPanel.Controls.Add(BuildScheduleMenu());

            // Sécurité
            contentPanel.Controls.Add(BuildPermissionMenu("editor"));

            return contentPanel;
        }

        /// <summary>
        /// Construit le panel des options de police : taille et police
        /// </summary>
        /// <returns></returns>
        private Panel BuildFontOptions()
        {
            Panel panelFont = new Panel();
            Panel panelSelects = new Panel();
            HtmlGenericControl label, select, option;
            string fontFamily = string.Empty;
            string fontSize = string.Empty;
            List<string> fontsList = new List<string>()
            {
                "Arial", "Comic Sans MS", "Courrier New", "Georgia", "Lucida Sans Unicode", "Tahoma", "Times New Roman", "Trebuchet MS", "Verdana", "Calibri", "Cambria", "Microsoft Sans Serif", "Century Gothic"
            };
            List<string> fontSizesList = new List<string>()
            {
                "8", "9", "10", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72"
            };
            fontsList.Sort();

            if (_report != null)
            {
                fontFamily = _report.GetParamValue("fontfamily") ?? string.Empty;
                fontSize = _report.GetParamValue("fontsize") ?? string.Empty;
            }


            panelFont.CssClass = "editor_template";
            panelFont.ID = "editor_html_font";
            panelSelects.ID = "fontSelects";

            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(_ePref, 1838), " :");
            panelFont.Controls.Add(label);

            #region Polices
            select = new HtmlGenericControl("select");
            select.ID = "selectFont";
            select.Attributes.Add("title", eResApp.GetRes(_ePref, 1837));

            option = new HtmlGenericControl("option");
            option.InnerText = eResApp.GetRes(_ePref, 1510);
            option.Attributes.Add("value", "");
            option.Attributes.Add("disabled", "disabled");
            option.Attributes.Add("selected", "selected");
            select.Controls.Add(option);

            foreach (string font in fontsList)
            {
                option = new HtmlGenericControl("option");
                option.Style.Add(HtmlTextWriterStyle.FontFamily, font);
                option.Attributes.Add("value", font);
                if (font.ToLower() == fontFamily)
                    option.Attributes.Add("selected", "selected");
                option.InnerText = font;
                select.Controls.Add(option);
            }
            panelSelects.Controls.Add(select);

            #endregion

            #region Tailles de police
            select = new HtmlGenericControl("select");
            select.ID = "selectFontSize";
            select.Attributes.Add("title", eResApp.GetRes(_ePref, 1836));

            option = new HtmlGenericControl("option");
            option.InnerText = eResApp.GetRes(_ePref, 639);
            option.Attributes.Add("value", "");
            option.Attributes.Add("disabled", "disabled");
            option.Attributes.Add("selected", "selected");
            select.Controls.Add(option);

            foreach (string size in fontSizesList)
            {
                option = new HtmlGenericControl("option");
                option.Style.Add(HtmlTextWriterStyle.FontSize, size + "pt");
                option.Attributes.Add("value", size);
                if (size == fontSize)
                    option.Attributes.Add("selected", "selected");
                option.InnerText = size;
                select.Controls.Add(option);
            }
            panelSelects.Controls.Add(select);
            #endregion

            panelFont.Controls.Add(panelSelects);

            return panelFont;
        }

        #endregion
        #endregion

        #region méthodes utilitaires
        /// <summary>
        /// Retourne le paramètre du rapport pour la clé donnée.
        /// </summary>
        /// <param name="report">Rapport cible</param>
        /// <param name="paramKey">Clé</param>
        /// <returns>valeur pour la clé /si le rapport n'a pas été instancié , String.empty</returns>
        protected String GetReportParam(eReport report, String paramKey)
        {
            if (report == null)
                return String.Empty;

            return report.GetParamValue(paramKey, false);
        }

        /// <summary>
        /// détermine selon l'index transmit en paramètre si le format correspondant est activé ou non pour
        /// le type de rapport transmis en paramètres.
        /// </summary>
        /// <param name="reportType">Type de rapport</param>
        /// <param name="formatIndex">Index du format</param>
        /// <returns>True si le format est activé, false s'il est désactivé</returns>
        private bool IsFormatEnabled(TypeReport reportType, int formatIndex)
        {
            switch (reportType)
            {
                case TypeReport.EXPORT:
                    return formatIndex > 0 && formatIndex < 6;
                case TypeReport.PRINT:
                    return formatIndex == 4;
                case TypeReport.PRINT_FILE:
                    return formatIndex == 4;
                case TypeReport.MERGE:
                    return formatIndex > 0 && formatIndex < 6;
                default:
                    return false;
            }
        }
        #endregion





        /// <summary>
        /// Construit la rquete de recup des table et des champs
        /// </summary>
        /// <param name="sParamFile"></param>
        /// <param name="listOflinkedId"></param>
        /// <returns></returns>
        protected RqParam BuildQuery(String sParamFile, IEnumerable<int> listOflinkedId)
        {


            //Liste de catalogue 'systeme' dont le type est numeric dans [desc]
            List<int> lstCatalogSys = new List<int>() { 106016, 106017 };

            RqParam rqFileAndFields = new RqParam();
            StringBuilder sSqlListParam = new StringBuilder();
            int cnt = 0;
            foreach (int tab in listOflinkedId)
            {
                if (cnt > 0)
                    sSqlListParam.Append(", ");

                String sParam = String.Concat("@tab", cnt);
                sSqlListParam.Append(sParam);
                cnt++;

                rqFileAndFields.AddInputParameter(sParam, SqlDbType.Int, tab);
            }

            rqFileAndFields.AddInputParameter("@userid", SqlDbType.Int, Pref.User.UserId);
            rqFileAndFields.AddInputParameter("@userlevel", SqlDbType.Int, Pref.User.UserLevel);
            rqFileAndFields.AddInputParameter("@group", SqlDbType.Int, Pref.User.UserGroupId);

            String sWhereAdd = String.Empty;
            switch (sParamFile)
            {
                case "SeriesFile":
                    sWhereAdd = String.Concat("AND ( ",

                        " FldDesc.Format IN (@formatuser, @formatgroup) ",
                       " OR FldDesc.Popup >= @popup ",
                         "OR FldDesc.DESCID IN ( ", string.Join(",", lstCatalogSys), ") ",
                       " OR (TabDESC.[Type] = @FILEMAIN  AND FldDesc.DescId % 100 = 1)",
                       ")");

                    rqFileAndFields.AddInputParameter("@formatuser", SqlDbType.Int, (int)FieldFormat.TYP_USER);
                    rqFileAndFields.AddInputParameter("@formatgroup", SqlDbType.Int, (int)FieldFormat.TYP_GROUP);
                    rqFileAndFields.AddInputParameter("@popup", SqlDbType.Int, PopupType.FREE);
                    rqFileAndFields.AddInputParameter("@FILEMAIN", SqlDbType.Int, (int)EdnType.FILE_MAIN);

                    break;
                case "ValuesFile":
                    sWhereAdd = string.Concat(
                        " AND FldDesc.Format IN (@formatnum, @formatmoney, @formatbit)",
                        " AND  FldDesc.DESCID NOT IN ( ", string.Join(",", lstCatalogSys), ") ");

                    rqFileAndFields.AddInputParameter("@formatnum", SqlDbType.Int, (int)FieldFormat.TYP_NUMERIC);
                    rqFileAndFields.AddInputParameter("@formatmoney", SqlDbType.Int, (int)FieldFormat.TYP_MONEY);
                    rqFileAndFields.AddInputParameter("@formatbit", SqlDbType.Int, (int)FieldFormat.TYP_BIT);
                    break;
                case "EtiquettesFile":
                    sWhereAdd = String.Concat("AND (  ",
                        " FldDesc.Format IN (@formatuser, @formatgroup, @formatdate) ",
                        " OR FldDesc.DESCID IN(", string.Join(", ", lstCatalogSys), ") ",
                        " OR FldDesc.Popup >= @popup ",
                        " OR (TabDESC.[Type] = @FILEMAIN  AND FldDesc.DescId % 100 = 1)",
                        ")");
                    rqFileAndFields.AddInputParameter("@formatuser", SqlDbType.Int, (int)FieldFormat.TYP_USER);
                    rqFileAndFields.AddInputParameter("@formatgroup", SqlDbType.Int, (int)FieldFormat.TYP_GROUP);
                    rqFileAndFields.AddInputParameter("@formatdate", SqlDbType.Int, (int)FieldFormat.TYP_DATE);
                    rqFileAndFields.AddInputParameter("@formatchar", SqlDbType.Int, (int)FieldFormat.TYP_CHAR);
                    rqFileAndFields.AddInputParameter("@popup", SqlDbType.Int, PopupType.FREE);
                    rqFileAndFields.AddInputParameter("@FILEMAIN", SqlDbType.Int, (int)EdnType.FILE_MAIN);
                    break;
                default:
                    break;
            }

            String sSql = String.Concat("SELECT TabRES.ResId TabId, TabRES.", Pref.Lang, " TabLabel, FldRES.ResId FldId, FldRES.", Pref.Lang, " FldLabel, ISNULL(FldDesc.Popup,0) Popup , ISNULL(FldDesc.PopupDescId,0) PopupDescId ,", Environment.NewLine,
                 " isnull(TabViewPerm.P,1) isTabViewPerm, isnull(FldViewPerm.P,1) isFldViewPerm, FldDesc.Format", Environment.NewLine,
                 " FROM RES FldRES INNER JOIN RES TabRES ", Environment.NewLine,
                 "	ON FldRES.ResId - FldRES.ResId % 100 = TabRES.ResId", Environment.NewLine,
                 "		AND TabRES.resid >= 100 ", Environment.NewLine,
                 "		AND FldRES.ResId % 100 > 0", Environment.NewLine,
                 "		AND TabRES.ResId % 100 = 0", Environment.NewLine,
                 " INNER JOIN [DESC] FldDesc ON FldRES.resid = FldDesc.descid", Environment.NewLine,
                 " INNER JOIN [DESC] TabDESC ON TabRES.ResId = TabDESC.DescId", Environment.NewLine,
                 " LEFT JOIN dbo.cfc_getPermInfo(@userid, @userlevel, @group) FldViewPerm ON FldDesc.ViewPermId = FldViewPerm.PermissionId ", Environment.NewLine,
                 " LEFT JOIN dbo.cfc_getPermInfo(@userid, @userlevel, @group) TabViewPerm ON TabDESC.ViewPermId = TabViewPerm.PermissionId ", Environment.NewLine,
                 "", Environment.NewLine,
                 " WHERE TabRES.ResId IN (" + sSqlListParam + ") ", Environment.NewLine,
                 " AND FldRES.ResId % 100 not in (@fieldcolor, @fieldsched, @fieldcalitem) ", Environment.NewLine,
                 sWhereAdd, Environment.NewLine,
                 " ORDER BY TabLabel,TabId, FldLabel");

            rqFileAndFields.SetQuery(sSql);

            rqFileAndFields.AddInputParameter("@fieldcolor", SqlDbType.Int, PlanningField.DESCID_CALENDAR_COLOR.GetHashCode());
            rqFileAndFields.AddInputParameter("@fieldsched", SqlDbType.Int, PlanningField.DESCID_SCHEDULE_ID.GetHashCode());
            rqFileAndFields.AddInputParameter("@fieldcalitem", SqlDbType.Int, PlanningField.DESCID_CALENDAR_ITEM.GetHashCode());

            return rqFileAndFields;
        }


        /// <summary>
        /// Récupère pour chaque fichier la liste des champs
        /// </summary>       
        /// <param name="eDal"></param>
        /// <param name="sParamFile">Type des axes du graphique : série, etiquettes X, Y</param> 
        /// <param name="listOflinkedId">liste des tables liées</param>
        /// <param name="folder">un reader qui sera rempli</param>
        /// <param name="sErr">erreur généré</param>
        /// <returns>Succès ou pas</returns>
        protected bool GetFileAndField(eudoDAL eDal, String sParamFile, ISet<int> listOflinkedId, out DescItem folder, out String sErr)
        {
            folder = new DescItem();

            if (listOflinkedId.Contains(TableType.PP.GetHashCode()) || listOflinkedId.Contains(TableType.PM.GetHashCode()))
            {
                listOflinkedId.Add(TableType.PP.GetHashCode());
                listOflinkedId.Add(TableType.PM.GetHashCode());
                listOflinkedId.Add(TableType.ADR.GetHashCode());
            }

            if (eDal != null && !eDal.IsOpen)
                eDal.OpenDatabase();
            //Exec de la requete
            RqParam rq = BuildQuery(sParamFile, listOflinkedId);
            DataTableReaderTuned reader = eDal.Execute(rq, out sErr);

            if (sErr.Length > 0 || reader == null || !reader.HasRows)
            {
                if (eDal != null)
                    eDal.CloseDatabase();
                _sErrorMsg = sErr;

                return false;
            }

            folder = GetDescItemList(reader);

            return true;
        }


        /// <summary>
        /// Retourne la liste des DescItem a partir du reader
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected static DescItem GetDescItemList(DataTableReaderTuned reader)
        {
            DescItem folder = new DescItem();

            DescItem file, field;

            bool iterating = reader.Read();
            while (iterating)
            {
                file = new DescItem();
                file.DescId = reader.GetEudoNumeric("TabId");
                file.Label = reader.GetString("TabLabel");
                file.AllowedView = reader.GetString("isTabViewPerm") == "1";

                while (iterating)
                {
                    field = new DescItem();
                    field.DescId = reader.GetEudoNumeric("FldId");
                    Enum.TryParse(reader.GetString("Popup"), out field.Popup);
                    field.PopupDescId = reader.GetEudoNumeric("PopupDescId");
                    field.Label = reader.GetString("FldLabel");
                    field.AllowedView = reader.GetString("isFldViewPerm") == "1";

                    Enum.TryParse<FieldFormat>(reader.GetString("Format"), out field.Format);

                    //La meme table, on ajoute les fields   
                    if (file.DescId == field.DescId - field.DescId % 100)
                        file.Items.Add(field);
                    else
                        //Table différente on remonte pour créer un nouveau file
                        break;

                    iterating = reader.Read();
                }

                folder.Items.Add(file);
            }

            return folder;
        }


        /// <summary>
        /// Crée un element html avec les proprietés passées en paramétre
        /// </summary>
        /// <param name="sHtmlTag"> Champs obligatoire : tag html a creer</param>
        /// <param name="sCssClass">Champs optionnel : la class css a appliquer a l'element, vide par defaut </param>
        /// <param name="sInnerHtml">Champs optionnel : le contenu de l'element, vide par defaut </param>
        /// <param name="sId">Champs optionnel : l'id de l'element, vide par defaut</param>
        /// <returns></returns>
        protected static HtmlGenericControl CreateHtmlTag(String sHtmlTag, String sCssClass = "", String sInnerHtml = "", String sId = "")
        {
            HtmlGenericControl element = new HtmlGenericControl(sHtmlTag);

            if (sCssClass.Length > 0)
                element.Attributes.Add("class", sCssClass);

            if (sId.Length > 0)
                element.ID = sId;

            if (sInnerHtml.Length > 0)
                element.InnerHtml = HttpUtility.HtmlEncode(sInnerHtml);

            return element;

        }


        /// <summary>
        /// Fait un rendu d un drop list des fonctions d'agrégations dans le container li
        /// </summary>
        /// <param name="liContainer"></param>
        /// <param name="SelectedFunc"></param>
        /// <param name="onChange">la fonction à déclencher sur le onchange event</param>
        /// <param name="dllPrefix">Prefix utilisé pour le chart combiné</param>
        protected void RenderAgregateFuncionsIntoContainer(HtmlGenericControl liContainer, String SelectedFunc, String onChange, string dllPrefix = "")
        {
            DropDownList funcDropList = RenderAgregateFuncions(SelectedFunc, dllPrefix);
            funcDropList.Attributes["onchange"] = onChange;
            liContainer.Controls.Add(funcDropList);
        }

        /// <summary>
        /// Fait un rendu d un drop list des fonctions d'agrégations en selecetionant la valeur correspondant a l index
        /// </summary>
        /// <param name="SelectedFunc"></param>
        /// <param name="dllPrefix"></param>
        protected DropDownList RenderAgregateFuncions(String SelectedFunc, string dllPrefix = "")
        {
            DropDownList funcDropList = RenderDropDownList(string.Concat(dllPrefix, "ValuesOperation"), "editor_select", "UpdateParams();");
            funcDropList.Items.Add(NewListItem(eCommunChart.TypeAgregatFonction.COUNT, eResApp.GetRes(Pref, 437), SelectedFunc));
            funcDropList.Items.Add(NewListItem(eCommunChart.TypeAgregatFonction.SUM, eResApp.GetRes(Pref, 633), SelectedFunc));
            funcDropList.Items.Add(NewListItem(eCommunChart.TypeAgregatFonction.AVG, eResApp.GetRes(Pref, 634), SelectedFunc));
            funcDropList.Items.Add(NewListItem(eCommunChart.TypeAgregatFonction.MAX, eResApp.GetRes(Pref, 636), SelectedFunc));
            funcDropList.Items.Add(NewListItem(eCommunChart.TypeAgregatFonction.MIN, eResApp.GetRes(Pref, 635), SelectedFunc));
            return funcDropList;
        }


        /// <summary>
        /// Avec un rendu d un drop list avec les attributs en params
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cssClass"></param>
        /// <param name="onChange"></param>
        /// <param name="attributes"></param>
        /// <returns></returns>
        protected DropDownList RenderDropDownList(String id, String cssClass, String onChange, Dictionary<string, string> attributes = null)
        {
            DropDownList ddlSelectFile = new DropDownList();
            ddlSelectFile.ID = id;
            ddlSelectFile.CssClass = cssClass;

            ddlSelectFile.Attributes.Add("onchange", onChange);

            if (attributes != null && attributes.Count > 0)
            {
                foreach (KeyValuePair<string, string> item in attributes)
                {
                    ddlSelectFile.Attributes.Add(item.Key, item.Value);
                }
            }
            return ddlSelectFile;
        }


        /// <summary>
        /// Construit un listItem générique
        /// </summary>
        /// <param name="ItemValue"></param>
        /// <param name="ItemText"></param>
        /// <param name="ItemSelectedValue"></param>
        /// <returns></returns>
        private ListItem NewListItem(eCommunChart.TypeAgregatFonction ItemValue, String ItemText, string ItemSelectedValue)
        {
            ListItem item = new ListItem(ItemText, ItemValue.ToString());
            item.Selected = ItemValue.ToString().ToLower() == ItemSelectedValue;
            return item;
        }

        /// <summary>
        /// Fait le rendu multiline en cas de graphique multisérie avec plusieurs rubriques
        /// </summary>
        /// <param name="liMultiLines"></param>
        /// <param name="files"></param>
        protected void RenderMultiLine(HtmlGenericControl liMultiLines, DescItem folder)
        {

            HtmlGenericControl liLine, logoDeleteDiv;
            DropDownList FieldsDropList;
            DropDownList AgregatFuncsDropList;

            String[] selectedFuncs = _report.GetParamValue("ValuesOperation").Split(';');
            String[] selectedFields = _report.GetParamValue("ValuesField").Split(';');

            // Params invalides
            if (selectedFuncs.Length != selectedFields.Length)
                throw new Exception("eChartWizardRenderer::RenderMultiLine: Paramétres du raport graphique incohérents : Nombre de ligne de ValuesOperation != Nombre de lignes de ValuesField");

            //Le première ligne contenant : [table champ operation] est déjà générée
            if (selectedFuncs.Length <= 1)
                return;

            Random random = new Random();
            for (int i = 1; i < selectedFuncs.Length; i++)
            {
                liLine = new HtmlGenericControl("li");
                liLine.ID = random.Next(10000000).ToString();

                //La liste des champs
                FieldsDropList = RenderDropDownList("ValuesField_" + folder.SelecetedItem.DescId + "_" + liLine.ID, "editor_select", "UpdateParams();");
                FieldsDropList.Attributes.Add("name", "ValuesField_" + folder.SelecetedItem.DescId);
                FillDropList(FieldsDropList, folder.SelecetedItem.Items, selectedFields[i]);

                //La liste des fonctions d'agrégation
                AgregatFuncsDropList = RenderAgregateFuncions(selectedFuncs[i]);
                AgregatFuncsDropList.ID = "ValuesOperation_" + liLine.ID;

                //La pubelle
                logoDeleteDiv = new HtmlGenericControl("div");
                logoDeleteDiv.Attributes.Add("class", "logoDeleteLine");
                logoDeleteDiv.Attributes.Add("onclick", "DeleteRubriqueY(this.parentElement);");

                liLine.Controls.Add(FieldsDropList);
                liLine.Controls.Add(AgregatFuncsDropList);
                liLine.Controls.Add(logoDeleteDiv);

                liMultiLines.Controls.Add(liLine);
            }
        }

        /// <summary>
        /// Rempli la liste par les rubrique FieldItems
        /// </summary>
        /// <param name="FieldsDropList"></param>
        /// <param name="Fields"></param>
        /// <param name="p"></param>
        private void FillDropList(DropDownList FieldsDropList, List<DescItem> Fields, String SelectedFld)
        {
            ListItem newItem;
            foreach (DescItem fld in Fields)
            {
                newItem = new ListItem(fld.Label, fld.DescId.ToString());
                newItem.Attributes.Add("fmt", fld.Format.GetHashCode().ToString());
                if (SelectedFld.Equals(fld.DescId.ToString()))
                    newItem.Selected = true;

                FieldsDropList.Items.Add(newItem);
            }
        }


        /// <summary>
        /// Recupère les tables et les rubrique correspondant en faisant un rendu dans le container.
        /// </summary>
        /// <param name="liContainer">container</param>
        /// <param name="sParamFile"></param>
        /// <param name="sParamField"></param>
        /// <param name="param">Les paramètres du rendu</param>
        /// <param name="folder">collection des tables </param>
        /// <param name="bVisible">collection des tables </param>
        /// <returns></returns>

        //, 
        //eCommunChart.TypeChart typeChart = eCommunChart.TypeChart.GLOBALCHART, 
        //bool filesOnly = false, 
        //bool bFilesVisible = true, 
        //string prefixFilter = "", 
        //bool getAllTab = false, 
        //int nFiltredTab = 0, 
        //bool onlyDataField = false, 
        //bool addGaugePart = false

        protected bool FillFileAndFields(HtmlGenericControl liContainer,
            String sParamFile, String sParamField,
            ChartSpecificParams param, out DescItem folder, out bool bVisible)
        {
            bVisible = true;
            String sErr;
            string onChange = "UpdateParamReport";
            if (param.CategoryChart == eCommunChart.TypeChart.STATCHART)
                onChange = "UpdateGraph";


            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);

            try
            {
                ISet<int> listOflinkedId = new HashSet<int>();
                folder = new DescItem();

                //On récupère la liste des table liées
                if (param.CategoryChart == eCommunChart.TypeChart.STATCHART)
                {
                    listOflinkedId.Add(_iTab);
                }
                else
                {
                    if (param.GetAllTab)
                    {
                        if (!GetAllTab(eDal, listOflinkedId, _listOfLinkedTabId, out sErr))
                            throw new Exception("Impossible de récupérer la liste des ids liés: " + sErr);

                        this._listOfTabId = listOflinkedId;
                    }
                    else
                    {
                        if (!GetLinkedId(eDal, listOflinkedId, out sErr))
                            throw new Exception("Impossible de récupérer la liste des ids liés: " + sErr);
                    }


                }


                //On récupère ,pour chaque table, la liste des ses champs 
                if (!GetFileAndField(eDal, sParamFile, listOflinkedId, out folder, out sErr))
                    throw new Exception("Impossible de récupérer la liste des champs des table liées : " + sErr);


                //On fait le rendu de la première ligne [file - champ - ...] ce type de série graphique
                if (!RenderFirstFileAndFields(liContainer, sParamFile, sParamField, onChange, folder, param, out bVisible))
                    throw new Exception("Impossible de faire le rendu du graphique sParamFile = " + sParamFile);



                return true;
            }
            finally
            {
                if (eDal != null)
                    eDal.CloseDatabase();
            }
        }


        /// <summary>
        /// rempli le control fourni avec un dropdown contenant la liste des filtes
        /// </summary>
        /// <param name="li">La liste en cours</param>
        /// <param name="prefix">s'il existe: utilisé pour le graphique combiné</param>
        /// <param name="seletedTab">la table préselectionnée</param>
        /// <returns></returns>
        protected bool FillFilter(HtmlGenericControl li, string prefix = "", string seletedTab = "")
        {
            int nSlectedTab = 0;
            if (!int.TryParse(seletedTab, out nSlectedTab))
                nSlectedTab = _iTab;


            DropDownList ddlFilters = new DropDownList();
            li.Controls.Add(ddlFilters);
            ddlFilters.ID = string.Concat(prefix, "ddlfilter");

            li.ID = string.Concat(prefix, "liAddFilter");

            ListItem defaultItem = new ListItem();
            ddlFilters.Items.Add(defaultItem);
            ddlFilters.CssClass = string.Concat(prefix, "filterSelect");
            //Aucun filtre sélectionné
            defaultItem.Text = eResApp.GetRes(Pref, 430);
            defaultItem.Value = "0";

            int nSelfilterId;
            int.TryParse(GetReportParam(_report, string.Concat(prefix, "filterId")), out nSelfilterId);


            #region récupération de la liste des filtres
            eDataFillerGeneric filler = new eDataFillerGeneric(Pref, 104000, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eq)
                {


                    //Type
                    List<WhereCustom> list = new List<WhereCustom>();

                    list.Add(new WhereCustom(FilterField.TYPE.GetHashCode().ToString(), Operator.OP_0_EMPTY, TypeFilter.USER.GetHashCode().ToString(), InterOperator.OP_AND));


                    WhereCustom wType = new WhereCustom(list);


                    //Libelle non vide
                    list = new List<WhereCustom>();
                    list.Add(new WhereCustom(FilterField.LIBELLE.GetHashCode().ToString(), Operator.OP_IS_NOT_EMPTY, ""));
                    WhereCustom wLibelle = new WhereCustom(list, InterOperator.OP_AND);


                    //Utilisateur en cours ou filtre sélectioner
                    list = new List<WhereCustom>();
                    list.Add(new WhereCustom(FilterField.USERID.GetHashCode().ToString(), Operator.OP_EQUAL, Pref.UserId.ToString(), InterOperator.OP_OR));
                    list.Add(new WhereCustom(FilterField.USERID.GetHashCode().ToString(), Operator.OP_0_EMPTY, "", InterOperator.OP_OR));
                    //list.Add(new WhereCustom(FilterField.ID.GetHashCode().ToString(), Operator.OP_EQUAL, nSelfilterId.ToString(), InterOperator.OP_OR));

                    WhereCustom wCuser = new WhereCustom(list, InterOperator.OP_AND);

                    //Table
                    list = new List<WhereCustom>();
                    //if (this._listOfTabId != null && this._listOfTabId.Count > 0)
                    //    list.Add(new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_IN_LIST, eLibTools.Join<int>(";", this._listOfTabId)));
                    //else
                    list.Add(new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, nSlectedTab.ToString()));
                    WhereCustom wcTab = new WhereCustom(list, InterOperator.OP_AND);

                    //
                    list = new List<WhereCustom>();
                    list.Add(wLibelle);
                    list.Add(wType);
                    list.Add(wCuser);
                    list.Add(wcTab);


                    eq.AddCustomFilter(new WhereCustom(list));

                    eq.SetListCol = string.Concat(FilterField.LIBELLE.GetHashCode().ToString(), ";", FilterField.TAB.GetHashCode().ToString());

                };

            filler.Generate();


            if (filler.ErrorMsg.Length > 0)
            {
                _sErrorMsg = filler.ErrorMsg;
                _eException = filler.InnerException;

                return false;
            }
            else if (filler.ListRecords == null)
            {
                _sErrorMsg = "eChartWizardRendererFillFilter : erreur de la génération de la liste des filtres disponible :ListRecords ==null ";
                return false;
            }
            #endregion


            ListItem filterItem;
            String sFilterId;
            String sLibelle;

            foreach (eRecord er in filler.ListRecords)
            {
                //     sFilterId = dtrFilters.GetSafeValue("FilterId").ToString();
                sFilterId = er.MainFileid.ToString();

                eFieldRecord efLibelle = er.GetFields.Find(delegate (eFieldRecord ef)
                {

                    return ef.FldInfo.Alias == string.Concat("104000_", FilterField.LIBELLE.GetHashCode().ToString());
                });

                if (efLibelle == null || efLibelle.Value.Length == 0)
                    continue;

                eFieldRecord efTab = er.GetFields.Find(delegate (eFieldRecord ef)
                {

                    return ef.FldInfo.Alias == string.Concat("104000_", FilterField.TAB.GetHashCode().ToString());
                });

                if (efTab == null || efTab.Value.Length == 0)
                    continue;

                //     sLibelle = dtrFilters.GetSafeValue("Libelle").ToString();
                sLibelle = efLibelle.DisplayValue;

                filterItem = new ListItem(sLibelle, sFilterId);
                filterItem.Selected = sFilterId.Equals(nSelfilterId.ToString()) ? true : false;
                filterItem.Attributes.Add("tab", efTab.Value);
                ddlFilters.Items.Add(filterItem);

                if (!string.IsNullOrEmpty(seletedTab) && seletedTab != efTab.Value)
                {
                    filterItem.Attributes.Add("display", "0");
                    filterItem.Attributes.Add("disabled", "disabled");
                }

            }

            ddlFilters.Attributes.Add("onchange", String.Concat("oReport.SetParam('", prefix.ToLower(), "filterid',this.value);"));
            li.Controls.Add(ddlFilters);


            return true;
        }


        /// <summary>
        /// Permet de générer un filtre expresse pour les graphieu et l'attaché à un container (HtmlGenericControl)
        /// </summary>
        /// <param name="nReportId">Numéro du rapport</param>
        /// <param name="ul">ul du wraper</param>
        /// <param name="folder">collection de table ou rubrique</param>
        /// <param name="bVisible"></param>
        /// <param name="bcombineFilter"></param>
        /// <param name="display">Permet d'afficher le filtr expresse ou le masquer, utilisé pour le graphique combiné</param>
        protected void SetListExpressFilter(int nReportId, HtmlGenericControl ul, DescItem folder, ChartSpecificParams param, bool bVisible, bool bcombineFilter = false, bool display = true)
        {
            param.GetFilesOnly = true;
            param.CategoryChart = eCommunChart.TypeChart.UNDEFINED;

            if (bcombineFilter)
            {
                /*, getAllTab: true, filesOnly: true, prefixFilter: yPrefix*/
                param.GetAllTab = true;
                param.GetFilesOnly = true;
                param.PrefixFilter = param.CombinedZprefix;
                param.GetLinkedTabOnly = true;
                listSpecialExpresFilter(nReportId, ul, folder, param, bVisible, display: display);
            }

            else
                listExpressFilter(nReportId, ul, folder, param, bVisible);


        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected void listExpressFilter(int nReportId, HtmlGenericControl ul, DescItem folder, ChartSpecificParams param, bool bVisible)
        {
            HtmlGenericControl liLabel;
            HtmlGenericControl liContent;
            AdvFilterTab filterTab;
            AdvFilterLine filterLine;
            AdvFilter filter;
            TypeFilter filterType = 0;
            int nTabIndex = 0;
            int nTabFilter;
            int nFieldFilter;
            Operator filterOperator = Operator.OP_UNDEFINED;
            bool bInitial = (nReportId == 0);


            FieldFormat[] lstFildsFormat = new FieldFormat[] { FieldFormat.TYP_USER, FieldFormat.TYP_BITBUTTON, FieldFormat.TYP_NUMERIC, FieldFormat.TYP_MONEY, FieldFormat.TYP_DATE, FieldFormat.TYP_BIT, FieldFormat.TYP_CHAR };

            int nSelExpressFilterfile;
            int nSelExpressFilterfield;


            liLabel = CreateHtmlTag("li", "filterExpressLabel", String.Concat(eResApp.GetRes(Pref, 8234), " :"));



            ul.Controls.Add(liLabel);
            param.GetFilesOnly = true;
            param.PrefixFilter = string.Empty;
            for (int i = 0; i < eModelConst.NB_CHART_EXPRESS_FILTER; i++)
            {
                bInitial = (nReportId == 0);
                nTabFilter = _iTab;
                nFieldFilter = 0;
                liContent = CreateHtmlTag("li", "selectExpressFilter", sId: "selectExpressFilter" + i.ToString());


                liContent.Controls.Add(CreateHtmlTag("label", "libelleExpressFilterBold", sInnerHtml: String.Concat(eResApp.GetRes(Pref, 182), " ", (i + 1).ToString())));
                ul.Controls.Add(liContent);

                FillFileAndFields(liContent, "file_" + i.ToString(), "ExpressFilterFirstFieldValue", param, out folder, out bVisible);

                if (int.TryParse(GetReportParam(_report, "file_" + i.ToString()), out nSelExpressFilterfile)) nTabFilter = nSelExpressFilterfile;
                if (int.TryParse(GetReportParam(_report, "field_0_" + i.ToString()), out nSelExpressFilterfield)) nFieldFilter = nSelExpressFilterfield;
                Enum.TryParse(GetReportParam(_report, "op_0_" + i.ToString()), out filterOperator);


                filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, nTabFilter.ToString());
                filterLine = filterTab.Lines[0];
                filterLine.LineIndex = i;
                filterLine.DescId = nFieldFilter;
                filterLine.LineOperator = InterOperator.OP_NONE;
                filterLine.Operator = filterOperator;
                filterLine.Value = _report.GetParamValue("value_0_" + i.ToString(), false);
                filter = AdvFilter.GetNewFilter(Pref, filterType, nTabFilter);
                filter.FilterTabs.Add(filterTab);

                if (nTabFilter == 0 || nFieldFilter == 0 || filterOperator == Operator.OP_UNDEFINED)
                    bInitial = true;

                eFilterRenderer.eFilterRendererParams filterParams = new eFilterRenderer.eFilterRendererParams(bInitialEfChart: bInitial, lstFildsFormat: lstFildsFormat, bFromChartReport: true);
                liContent.Controls.Add(eFilterRenderer.WizardManager.GetHtmlEMptyLine(Pref, filter, false, param: filterParams));

            }
        }



        /// <summary>
        /// Filre expresse pour le graphqieu combiné
        /// </summary>
        /// <param name="nReportId">numéro du rapport</param>
        /// <param name="ul">Container</param>
        /// <param name="folder">Liste de table et rubrique</param>
        /// <param name="bVisible">peut être afficher ou masquer</param>
        /// <param name="display">afficher ou masquer le filtre cominé</param>
        /// <param name="param">Paramètres spécifiques à l'affichage</param>
        protected void listSpecialExpresFilter(int nReportId, HtmlGenericControl ul, DescItem folder, ChartSpecificParams param, bool bVisible, bool display = true)
        {
            HtmlGenericControl liLabel;
            HtmlGenericControl liContent;
            AdvFilterTab filterTab;
            AdvFilterLine filterLine;
            AdvFilter filter;
            TypeFilter filterType = 0;
            int nTabIndex = 0;
            int nTabFilter;
            int nFieldFilter;
            Operator filterOperator = Operator.OP_UNDEFINED;
            bool bInitial = (nReportId == 0);
            DescItem folderTemp = folder;
            bool getFilterOpAndValue = true;

            FieldFormat[] lstFildsFormat = new FieldFormat[] { FieldFormat.TYP_USER, FieldFormat.TYP_BITBUTTON, FieldFormat.TYP_NUMERIC, FieldFormat.TYP_MONEY, FieldFormat.TYP_DATE, FieldFormat.TYP_BIT, FieldFormat.TYP_CHAR };

            int nSelExpressFilterfile;
            int nSelExpressFilterfield;
            int nFiltredCombinedZFileTab = 0;



            liLabel = CreateHtmlTag("li", "specialFilterExpressLabel", String.Concat(eResApp.GetRes(Pref, 8521), " :"));
            liLabel.Attributes.Add("ctype", "gauge");
            liLabel.Attributes.Add("gauge", "expressfilter");
            if (!display)
                liLabel.Style.Add(HtmlTextWriterStyle.Display, "none");
            ul.Controls.Add(liLabel);

            for (int i = 0; i < eModelConst.NB_CHART_EXPRESS_FILTER; i++)
            {
                if (param.ChartType == eModelConst.Chart.CIRCULARGAUGE && _report.Id != 0)
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.CombinedZprefix, "valuesfile")), out nFiltredCombinedZFileTab);
                else
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.CombinedZprefix, "etiquettesfile")), out nFiltredCombinedZFileTab);

                param.CobinedFilterdTabId = nFiltredCombinedZFileTab;

                bInitial = (nReportId == 0);
                param.ShowFiles = true;
                nTabFilter = _iTab;
                nFieldFilter = 0;
                liContent = CreateHtmlTag("li", "selectExpressFilter", sId: string.Concat("selectExpressFilter", i.ToString()));
                liContent.Attributes.Add("cType", "gauge");
                liContent.Attributes.Add("gauge", "filter");
                liContent.Attributes.Add("index", (i + 1).ToString());
                if (!display)
                    liContent.Style.Add(HtmlTextWriterStyle.Display, "none");
                liContent.Controls.Add(CreateHtmlTag("label", "libelleExpressFilterBold", sInnerHtml: String.Concat(eResApp.GetRes(Pref, 182), " ", (i + 1).ToString())));
                ul.Controls.Add(liContent);

                #region Filtre Histogramme
                param.PrefixFilter = param.CombinedYprefix;

                if (_report.Id > 0)
                    param.GetAllTab = !(param.ChartType == eModelConst.Chart.CIRCULARGAUGE || param.ChartType == eModelConst.Chart.COMBINED);

                #region Chargement des tables 
                FillFileAndFields(liContent, string.Concat("file_", i.ToString()), "ExpressFilterFirstFieldValue", param, out folder, out bVisible);
                #endregion

                if (int.TryParse(GetReportParam(_report, string.Concat(param.CombinedYprefix, "file_", i.ToString())), out nSelExpressFilterfile))
                    nTabFilter = nSelExpressFilterfile;

                if (int.TryParse(GetReportParam(_report, string.Concat(param.CombinedYprefix, "field_0_", i.ToString())), out nSelExpressFilterfield))
                    nFieldFilter = nSelExpressFilterfield;

                Enum.TryParse(GetReportParam(_report, string.Concat(param.CombinedYprefix, "op_0_", i.ToString())), out filterOperator);


                eFieldLiteWithLib fieldFilterSelected = GetStatFieldFormat(Pref, nFieldFilter);

                filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, nTabFilter.ToString());
                filterLine = filterTab.Lines[0];
                filterLine.LineIndex = i;
                filterLine.DescId = nFieldFilter;
                filterLine.LineOperator = InterOperator.OP_NONE;
                filterLine.Operator = filterOperator;
                filterLine.Value = _report.GetParamValue(string.Concat(param.CombinedYprefix, "value_0_", i.ToString()), false);
                filter = AdvFilter.GetNewFilter(Pref, filterType, nTabFilter);
                filter.FilterTabs.Add(filterTab);

                if (nTabFilter == 0 || nFieldFilter == 0 || filterOperator == Operator.OP_UNDEFINED)
                {
                    bInitial = true;
                    getFilterOpAndValue = true;
                }

                #region Chargement du filtre

                eFilterRenderer.eFilterRendererParams filterParams = new eFilterRenderer.eFilterRendererParams(bInitialEfChart: bInitial, lstFildsFormat: lstFildsFormat, bSpecialExpressFilter: _bSpecialFilterForGraphique, prefixFilter: param.CombinedYprefix, bFromChartReport: true);


                HtmlGenericControl emptyLine = eFilterRenderer.WizardManager.GetHtmlEMptyLine(Pref, filter, false, param: filterParams);

                #endregion

                liContent.Controls.Add(emptyLine);
                #endregion

                //bInitial = (nReportId == 0);
                getFilterOpAndValue = false;

                #region Filtre linéaire

                HtmlGenericControl lineaireLabel = CreateHtmlTag("label", "libelleExpressFilterBold", sInnerHtml: String.Format(eResApp.GetRes(Pref, 8537), (i + 1).ToString()));
                lineaireLabel.Attributes.Add("gauge", "filter");
                lineaireLabel.ID = string.Concat(param.CombinedZprefix, "label_", i.ToString());

                if (bInitial)
                    lineaireLabel.Style.Add(HtmlTextWriterStyle.Display, "none");
                liContent.Controls.Add(lineaireLabel);
                param.PrefixFilter = param.CombinedZprefix;
                param.ShowFiles = !bInitial;

                if (param.ChartType == eModelConst.Chart.CIRCULARGAUGE && _report.Id != 0)
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.CombinedYprefix, "valuesfile")), out nFiltredCombinedZFileTab);
                else
                    Int32.TryParse(GetReportParam(_report, string.Concat(param.CombinedZprefix, "etiquettesfile")), out nFiltredCombinedZFileTab);

                param.CobinedFilterdTabId = nFiltredCombinedZFileTab;
                if (_report.Id > 0)
                    param.GetAllTab = (param.ChartType == eModelConst.Chart.CIRCULARGAUGE || param.ChartType == eModelConst.Chart.COMBINED);
                /*, eCommunChart.TypeChart.UNDEFINED, filesOnly: true, bFilesVisible: !bInitial, prefixFilter: zPrefix, getAllTab: true, nFiltredTab: nFiltredCombinedZFileTab */
                FillFileAndFields(liContent, string.Concat("file_", i.ToString()), "ExpressFilterFirstFieldValue", param, out folder, out bVisible);


                if (int.TryParse(GetReportParam(_report, string.Concat(param.CombinedZprefix, "file_", i.ToString())), out nSelExpressFilterfile))
                    nTabFilter = nSelExpressFilterfile;


                if (int.TryParse(GetReportParam(_report, string.Concat(param.CombinedZprefix, "field_0_", i.ToString())), out nSelExpressFilterfield))
                    nFieldFilter = nSelExpressFilterfield;
                else
                    nFieldFilter = -1;


                Enum.TryParse(GetReportParam(_report, string.Concat(param.CombinedYprefix, "op_0_", i.ToString())), out filterOperator);


                filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, nTabFilter.ToString());
                filterLine = filterTab.Lines[0];
                filterLine.LineIndex = i;
                filterLine.DescId = nFieldFilter;
                filterLine.LineOperator = InterOperator.OP_NONE;
                filterLine.Operator = filterOperator;
                filterLine.Value = _report.GetParamValue(string.Concat(param.CombinedYprefix, "value_0_", i.ToString()), false);
                filter = AdvFilter.GetNewFilter(Pref, filterType, nTabFilter);
                filter.FilterTabs.Add(filterTab);

                if (nTabFilter == 0 || nFieldFilter == 0 || filterOperator == Operator.OP_UNDEFINED)
                {
                    bInitial = true;
                    getFilterOpAndValue = true;
                }

                filterParams = new eFilterRenderer.eFilterRendererParams(bInitialEfChart: bInitial, lstFildsFormat: lstFildsFormat, bSpecialExpressFilter: _bSpecialFilterForGraphique, prefixFilter: param.CombinedZprefix, bSetFieldAction: false, bGetFilterOpAndValue: getFilterOpAndValue, sDisplayedFiledsFmt: fieldFilterSelected?.Format, nDisplayedFieldsPud: fieldFilterSelected?.PopupDescId, bFromChartReport: true);


                emptyLine = eFilterRenderer.WizardManager.GetHtmlEMptyLine(Pref, filter, false, param: filterParams);

                emptyLine.ID = string.Concat(param.CombinedZprefix, "fieldsline_", i.ToString());
                if (bInitial)
                    emptyLine.Style.Add(HtmlTextWriterStyle.Display, "none");
                liContent.Controls.Add(emptyLine);
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="nReportId"></param>
        /// <param name="index"></param>
        /// <param name="erChartInfo"></param>
        /// <returns></returns>
        protected static HtmlGenericControl getExpressFilterLine(ePref pref, eudoDAL dal, int nReportId, int index, eReport erChartInfo)
        {
            int nEfValue;
            int opValue;
            string err;
            string opLibelle = string.Empty;
            eTableLiteWithLib tab;
            eFieldLiteWithLib field;
            Operator op;
            ISet<Operator> ops;
            IEnumerable<KeyValuePair<Operator, string>> opInfos;
            AdvFilterTab filterTab;
            AdvFilter filter;
            AdvFilterLine filterLine;
            TypeFilter filterType = 0;
            AdvFilterContext filterContext;
            AdvFilterLineIndex filterLineIndex = new AdvFilterLineIndex(0, 0);


            string eFValue = erChartInfo.GetParamValue("field_0_" + index.ToString(), false);
            int.TryParse(eFValue, out nEfValue);
            field = new eFieldLiteWithLib(nEfValue, pref.Lang);
            tab = new eTableLiteWithLib(nEfValue - nEfValue % 100, pref.Lang);
            tab.ExternalLoadInfo(dal, out err);
            field.ExternalLoadInfo(dal, tab, out err);


            #region Récupération des valeurs

            filter = AdvFilter.GetNewFilter(pref, filterType, tab.DescId);
            filterTab = AdvFilterTab.GetNewEmpty(0, tab.DescId.ToString());
            filter.FilterTabs.Add(filterTab);
            filterContext = new AdvFilterContext(pref, dal, filter);
            filterLine = filterTab.Lines[0];


            #region Récupération de l'opérateur

            if (int.TryParse(erChartInfo.GetParamValue("op_0_" + index.ToString(), false), out opValue))
            {
                op = (Operator)opValue;
                ops = new HashSet<Operator>();
                ops.Add(op);
                opInfos = eResApp.GetSortedOperator(pref.LangId, ops);
                foreach (KeyValuePair<Operator, string> keyValue in opInfos)
                    opLibelle = keyValue.Value;

                filterLine.Operator = op;
            }

            #endregion

            filterLine.LineIndex = index;
            filterLine.DescId = field.Descid;
            filterLine.LineOperator = InterOperator.OP_NONE;

            filterLine.Value = erChartInfo.GetParamValue("value_0_" + index.ToString(), false);
            filterLine = filterLineIndex.GetLine(filterContext.Filter);

            if (String.IsNullOrEmpty(opLibelle) || String.IsNullOrEmpty(field.Libelle) || String.IsNullOrEmpty(tab.Libelle))
                return null;
            //Div Conteneur
            HtmlGenericControl divExpressFilterLine = new HtmlGenericControl("div");
            divExpressFilterLine.ID = String.Concat("expressFilterFile_" + index.ToString(), nReportId);
            divExpressFilterLine.Attributes.Add("class", "divExpressFilter");
            divExpressFilterLine.InnerText = eResApp.GetRes(pref, 8245).Replace("<FIELD>", eLibTools.GetUppercaseFirst(field.Libelle)).Replace("<TABLE>", eLibTools.GetUppercaseFirst(tab.Libelle)).Replace("<OPERATOR>", eLibTools.GetUppercaseFirst(opLibelle));
            eFilterRenderer.eFilterRendererParams filterParams = new eFilterRenderer.eFilterRendererParams(bFromChartReport: true);
            divExpressFilterLine.Controls.Add(eFilterLineRenderer.GetValuesList(filterContext, filterLineIndex, param: filterParams));
            #endregion

            return divExpressFilterLine;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="dal"></param>
        /// <param name="nReportId"></param>
        /// <param name="index"></param>
        /// <param name="eF"></param>
        /// <returns></returns>
        protected static HtmlGenericControl getExpressFilterLine(ePref pref, eudoDAL dal, int nReportId, int index, eChartExpressFilter eF)
        {
            string opLibelle = string.Empty;
            eTableLiteWithLib tab;
            eFieldLiteWithLib field;
            ISet<Operator> ops;
            IEnumerable<KeyValuePair<Operator, string>> opInfos;
            AdvFilterTab filterTab;
            AdvFilter filter;
            AdvFilterLine filterLine;
            TypeFilter filterType = 0;
            AdvFilterContext filterContext;
            AdvFilterLineIndex filterLineIndex = new AdvFilterLineIndex(0, 0);

            field = eF.EfField;
            tab = eF.EfTabe;

            #region Récupération des valeurs

            filter = AdvFilter.GetNewFilter(pref, filterType, tab.DescId);
            filterTab = AdvFilterTab.GetNewEmpty(0, tab.DescId.ToString());
            filter.FilterTabs.Add(filterTab);
            filterContext = new AdvFilterContext(pref, dal, filter);
            filterLine = filterTab.Lines[0];


            #region Récupération de l'opérateur
            ops = new HashSet<Operator>();
            ops.Add(eF.Op);
            opInfos = eResApp.GetSortedOperator(pref.LangId, ops);
            foreach (KeyValuePair<Operator, string> keyValue in opInfos)
                opLibelle = keyValue.Value;

            filterLine.Operator = eF.Op;


            #endregion

            filterLine.LineIndex = index;
            filterLine.DescId = field.Descid;
            filterLine.LineOperator = InterOperator.OP_NONE;

            filterLine.Value = eF.EfValue;
            filterLine = filterLineIndex.GetLine(filterContext.Filter);

            if (String.IsNullOrEmpty(opLibelle) || String.IsNullOrEmpty(field.Libelle) || String.IsNullOrEmpty(tab.Libelle))
                return null;
            //Div Conteneur
            HtmlGenericControl divExpressFilterLine = new HtmlGenericControl("div");
            divExpressFilterLine.ID = String.Concat("expressFilterFile_", index.ToString(), "_", nReportId);
            divExpressFilterLine.Attributes.Add("class", "divExpressFilter");
            divExpressFilterLine.Attributes.Add("op", ((int)filterLine.Operator).ToString());

            HtmlGenericControl innerText = new HtmlGenericControl("div");
            innerText.Attributes.Add("class", "fEinnerText");
            string value = String.Concat(eResApp.GetRes(pref, 8245).Replace("<FIELD>", eLibTools.GetUppercaseFirst(field.Libelle)).Replace("<TABLE>", eLibTools.GetUppercaseFirst(tab.Libelle)).Replace("<OPERATOR>", eLibTools.GetUppercaseFirst(opLibelle)), " ");

            innerText.InnerText = value;
            innerText.Attributes.Add("title", value);
            divExpressFilterLine.Controls.Add(innerText);

            if (!eF.EfIsSpecialOperator)
            {
                eFilterRenderer.eFilterRendererParams filterParams = new eFilterRenderer.eFilterRendererParams(bFromChartReport: true);
                divExpressFilterLine.Controls.Add(eFilterLineRenderer.GetValuesList(filterContext, filterLineIndex, param: filterParams));
            }



            #endregion

            return divExpressFilterLine;


        }
        /// <summary>
        /// Création d'un button
        /// </summary>
        /// <returns></returns>
        protected static HtmlGenericControl getButtonvalidationFilter(ePref pref, int nReportId)
        {
            HtmlGenericControl actuDiv = new HtmlGenericControl("div");

            actuDiv.Attributes.Add("class", "button-green");
            actuDiv.Attributes.Add("ednmodalbtn", "1");
            actuDiv.Attributes.Add("onclick", string.Concat("UpdateDynamicGraph(", nReportId.ToString(), ")"));
            actuDiv.Style.Add(HtmlTextWriterStyle.Display, "inline-block");
            //actuDiv.Style.Add("float", "right");

            HtmlGenericControl actuDivLeft = new HtmlGenericControl("div");
            actuDivLeft.Attributes.Add("class", "button-green-left");
            actuDiv.Controls.Add(actuDivLeft);

            HtmlGenericControl actuDivMid = new HtmlGenericControl("div");
            actuDivMid.Attributes.Add("class", "button-green-mid");
            actuDivMid.InnerHtml = eResApp.GetRes(pref, 1127);
            actuDiv.Controls.Add(actuDivMid);


            HtmlGenericControl actuDivRight = new HtmlGenericControl("div");
            actuDivRight.Attributes.Add("class", "button-green-right");
            actuDiv.Controls.Add(actuDivRight);

            return actuDiv;
        }

        /// <summary>
        /// Récupère les liaisons
        /// </summary>
        /// <param name="eDal">couche acces a la base</param>
        /// <param name="listOflinkedId"> liste des id des table liées qui sera remplie</param>
        /// <param name="sErr">erreur générée</param>
        /// <param name="tab">Présente la table pour laquelle on veut récupérer les tables liées</param>
        /// <param name="addItab">Ajouter la table main à la collection des tables liées</param>
        /// <returns>Succès ou pas</returns>
        private bool GetLinkedId(eudoDAL eDal, ISet<int> listOflinkedId, out string sErr, int tab = 0, bool addItab = true)
        {
            if (tab == 0)
                tab = _iTab;

            // on récupère la liste des ids des tables liées à la table d'origine
            StringBuilder sbSql = new StringBuilder().AppendLine("SELECT DISTINCT RelationFileDescId FROM (");

            if (
                tab != (int)TableType.PP
                && tab != (int)TableType.PM
                && tab != (int)TableType.ADR
                )
                sbSql.AppendLine("SELECT RelationFileDescId FROM dbo.cfc_getLiaison(@tab) L WHERE isrelation = 1")
                    .AppendLine("Union");

            sbSql.AppendLine("SELECT RelationFileDescId FROM dbo.cfc_getLinked(@tab)L WHERE isrelation = 1")
                    .AppendLine("Union")
                    .AppendLine("SELECT @tab as RelationFileDescId")
                    .Append(") tt ");


            RqParam rqFiles = new RqParam(sbSql.ToString());
            rqFiles.AddInputParameter("@tab", SqlDbType.Int, tab);

            eDal.OpenDatabase();

            DataTableReaderTuned dtrFiles = null;
            if (addItab)
                listOflinkedId.Add(_iTab);

            try
            {
                dtrFiles = eDal.Execute(rqFiles, out sErr);

                if (sErr.Length > 0 || dtrFiles == null || !dtrFiles.HasRows)
                {
                    if (eDal != null)
                        eDal.CloseDatabase();
                    _sErrorMsg = sErr;
                    return false;
                }



                while (dtrFiles.Read())
                {
                    int nTab = dtrFiles.GetEudoNumeric(0);
                    if (nTab > 0)
                        listOflinkedId.Add(nTab);
                }
            }
            finally
            {
                if (dtrFiles != null)
                    dtrFiles.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Récupére toutes les tables ainsi que leurs liaisons
        /// </summary>
        /// <param name="eDal">eudodatl déjà construit mais fermé</param>
        /// <param name="listOflinkedId">la liste des tables lié à la table main en retour</param>
        /// <param name="lstLinkedByTab">Liste des tables lié par table donnée</param>
        /// <param name="sErr">ereur de retour si elle existe</param>
        /// <returns></returns>
        private bool GetAllTab(eudoDAL eDal, ISet<int> listOflinkedId, IDictionary<int, ISet<int>> lstLinkedByTab, out string sErr)
        {
            ISet<int> list = null;

            // on récupère la liste des ids des tables 
            string sSql = string.Concat("SELECT DISTINCT DESCID FROM [DESC] WHERE [DESC].[Format]  = 0 and [DESC].[Field] IN ('EVT','TPL','PM','PP','ADR') ORDER BY DESCID");
            RqParam rqFiles = new RqParam(sSql);

            eDal.OpenDatabase();

            DataTableReaderTuned dtrFiles = null;

            listOflinkedId.Add(_iTab);

            try
            {
                dtrFiles = eDal.Execute(rqFiles, out sErr);

                if (sErr.Length > 0 || dtrFiles == null || !dtrFiles.HasRows)
                {
                    if (eDal != null)
                        eDal.CloseDatabase();
                    _sErrorMsg = sErr;
                    return false;
                }

                while (dtrFiles.Read())
                {
                    int nTab = dtrFiles.GetEudoNumeric(0);
                    if (nTab > 0)
                    {
                        listOflinkedId.Add(nTab);

                        //Récuperer les tables liées
                        list = new HashSet<int>();
                        GetLinkedId(eDal, list, out sErr, tab: nTab, addItab: false);

                        if (sErr.Length > 0)
                        {
                            if (eDal != null)
                                eDal.CloseDatabase();
                            _sErrorMsg = sErr;
                            return false;
                        }

                        if (nTab == (int)TableType.PM)
                            list.Add((int)TableType.PP);

                        if (nTab == (int)TableType.PP)
                            list.Add((int)TableType.PM);

                        if (!lstLinkedByTab.ContainsKey(nTab))
                            lstLinkedByTab.Add(nTab, list);
                    }

                }
            }
            finally
            {
                if (dtrFiles != null)
                    dtrFiles.Dispose();
            }

            return true;
        }


        /// <summary>
        /// Fait le rendu des options de chaque de type de série graphique
        /// </summary>
        /// <param name="li">Control html</param>
        /// <param name="sParamFile">Type série : SérieFile, ValuesFile, EtiquettesFile</param>
        /// <param name="sParamField"></param>
        /// <param name="onChange">action à exécuter sur le on change </param>
        /// <param name="folder">collection des tables </param>
        /// <param name="typeChart">stat ou rapport </param>
        /// <param name="bVisible"></param>
        /// <param name="filesOnly"></param>
        /// <param name="bFilesVisible"></param>
        /// <param name="prefixFilter"></param>
        /// <param name="getAllTab">Permet de récupérer toutes les tables de la base : TPL,EVT,PP,PM,ADR</param>
        /// <param name="nFiltredTab">Inique a quelle table les tables liées devrait être récupérées</param>
        /// <param name="onlyDataField">Inique si on veut récupèerer que les rubrique de type catalogue</param>
        /// <returns></returns>
        protected bool RenderFirstFileAndFields(HtmlGenericControl li, String sParamFile, String sParamField, String onChange, DescItem folder, ChartSpecificParams param, out bool bVisible)
        {
            bVisible = true;
            int nSelectedChart = 0;
            String sTypeChart = GetReportParam(_report, "TypeChart");
            if (sTypeChart.Length > 0)
                int.TryParse(sTypeChart.Split('|')[1], out nSelectedChart);

            string changeSelectValue = string.Concat("UpdateDescription('", param.PrefixFilter, "');");
            if (param.CategoryChart == eCommunChart.TypeChart.STATCHART)
                changeSelectValue = "changeSelectValue(this);";

            string action = String.Concat("oReport.SetParam('", param.PrefixFilter.ToLower(), sParamFile.ToLower(), "',this.value);");

            if (!param.GetFilesOnly)
                action = string.Concat(action,
                    string.Concat("DispSelFld(this", param.ShowFiles ? "" : ",false", string.Concat(");",
                    sParamFile.Contains("ValuesFile") && (param.GetAllTab || (param.ChartType == eModelConst.Chart.CIRCULARGAUGE && param.Prefix == eLibConst.COMBINED_Z)) ? "UpdatFilterList(this,'" + param.PrefixFilter + "', true);" : "")),
                    param.GetAllTab && sParamFile.Contains("EtiquettesFile") ? "UpdatValuesFileList(this,'" + param.PrefixFilter + "');" : "");
            else
                action = string.Concat(action, string.Concat("onChangeChartReportFile(this", !string.IsNullOrEmpty(param.PrefixFilter) ? ",'" + param.PrefixFilter + "'" : "", ");"));

            DropDownList ddlSelectFile = RenderDropDownList(string.Concat(param.PrefixFilter, sParamFile), "editor_select", action);

            if (param.CategoryChart != eCommunChart.TypeChart.STATCHART)
                li.Controls.Add(ddlSelectFile);

            string sValuesFile = !string.IsNullOrEmpty(GetReportParam(_report, string.Concat(param.PrefixFilter, sParamFile))) ? GetReportParam(_report, string.Concat(param.PrefixFilter, sParamFile)) : _iTab.ToString();
            string[] sValuesField = GetReportParam(_report, string.Concat(param.PrefixFilter, sParamField)).Split(';');

            ListItem item;
            DropDownList ddlFields = new DropDownList();
            DropDownList ddlSelectedFields = new DropDownList();

            if (param.CobinedFilterdTabId > 0)
                this._lisEtiquetteXLinkedId = this._listOfLinkedTabId[param.CobinedFilterdTabId];
            foreach (DescItem file in folder.Items)
            {
                // dans les stats , on génére les champs de ntab et pas les relations
                if (param.CategoryChart == eCommunChart.TypeChart.STATCHART && !sValuesFile.Equals(file.DescId.ToString()))
                    continue;

                // Pas de droit de visu
                if (!file.AllowedView)
                    continue;

                // création d'un nouvelle DropDownList pour cette table
                if (!param.GetFilesOnly && !_bSpecialFilterForGraphique)
                    ddlFields = RenderDropDownList(string.Concat(param.PrefixFilter, sParamField, "_", file.DescId), "editor_select", string.Concat(changeSelectValue, onChange, "('", string.Concat(param.PrefixFilter, sParamField).ToLower(), "', this.value);"));

                int i = 0;
                // Rendu des rubriques de la table
                if (!param.GetFilesOnly)
                {

                    foreach (DescItem field in file.Items)
                    {
                        if (!field.AllowedView)
                            continue;

                        item = new ListItem(field.Label, field.DescId.ToString());
                        item.Attributes.Add("fmt", field.Format.GetHashCode().ToString());
                        item.Attributes.Add("pud", field.PopupDescId.ToString());
                        item.Attributes.Add("id", field.DescId.ToString());
                        item.Selected = sValuesField[0].Equals(field.DescId.ToString())
                            || (string.IsNullOrEmpty(sValuesField[0]) && field.DescId % 100 == (int)AllField.DATE_CREATE);

                        if (item.Selected)
                            file.SelecetedItem = field;

                        if (param.OnlyDataField)
                        {
                            if (nSelectedChart == (int)SYNCFUSIONCHART.FUNNEL && (field.Popup == PopupType.NONE || field.Popup == PopupType.SPECIAL)
                                || (field.PopupDescId % 100 == 1 && field.Popup == PopupType.ONLY)
                                )
                            {
                                item.Attributes.Add("display", "0");
                                item.Attributes.Add("disabled", "disabled");
                            }

                            else
                                i++;
                        }
                        ddlFields.Items.Add(item);
                    }
                    //NHA : Bug 75 626 en commentaire car il crée une regression
                    //SHA : correction bug 71 583
                    //if (ddlFields.Items.Count == 0)
                    //    continue;
                }


                //si on a des rubriques qui répondent aux conditions ou on est en mode file seulement(Filtre express des graphiques)=> on ajoute la table
                // Rendu de la table               
                item = new ListItem(file.Label, file.DescId.ToString());
                if (i == 0 && nSelectedChart == (int)SYNCFUSIONCHART.FUNNEL && param.OnlyDataField)
                {
                    item.Attributes.Add("display", "0");
                    item.Attributes.Add("disabled", "disabled");
                }


                if (/*param.GetAllTab &&*/param.Prefix != string.Empty && this._listOfLinkedTabId.ContainsKey(file.DescId))
                    item.Attributes.Add("linkedTab", this._listOfLinkedTabId[file.DescId].Join<int>(";"));





                ddlSelectFile.Items.Add(item);

                if (sValuesFile.Equals(file.DescId.ToString()))
                {
                    ddlSelectFile.Attributes.Add("selectedfile", file.DescId.ToString());
                    ddlSelectFile.SelectedValue = file.DescId.ToString();
                    ddlSelectedFields = ddlFields;
                    folder.SelecetedItem = file;
                    if (param.GetAllTab && param.CobinedFilterdTabId == 0 && this._listOfLinkedTabId.ContainsKey(file.DescId))
                        this._lisEtiquetteXLinkedId = this._listOfLinkedTabId[file.DescId];
                }
                else
                {
                    if (!param.GetFilesOnly)
                        ddlFields.Style.Add(HtmlTextWriterStyle.Display, "none");
                }


                if ((param.GetAllTab) &&
                    this._lisEtiquetteXLinkedId != null &&
                    this._lisEtiquetteXLinkedId.Count > 0 &&
                    !this._lisEtiquetteXLinkedId.Contains(file.DescId)
                    && param.GetLinkedTabOnly
                    //&& (param.ChartType != eModelConst.Chart.CIRCULARGAUGE && param.Prefix != eLibConst.COMBINED_Z)
                    )
                {
                    item.Attributes.Add("display", "0");
                    item.Attributes.Add("disabled", "disabled");
                }

                if (!param.ShowFiles)
                    ddlFields.Style.Add(HtmlTextWriterStyle.Display, "none");

                if (!param.GetFilesOnly)
                    li.Controls.Add(ddlFields);
            }

            if (!param.ShowFiles)
            {
                ddlSelectFile.Style.Add(HtmlTextWriterStyle.Display, "none");
                ddlSelectedFields.Style.Add(HtmlTextWriterStyle.Display, "none");

            }
            else
                ddlSelectedFields.Style.Remove(HtmlTextWriterStyle.Display);



            if (ddlFields.Items.Count == 0)
                bVisible = false;


            //sValuesField ca peut etre multiple
            sValuesField[0] = ddlSelectedFields.SelectedValue;
            _report.SetParamValue(string.Concat(param.PrefixFilter, sParamField), eLibTools.Join<String>(";", sValuesField));

            return true;
        }


        /// <summary>
        ///  Retourne un eFieldLiteWithLib
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nField"></param>
        /// <returns>string</returns>
        public static eFieldLiteWithLib GetStatFieldFormat(ePref pref, int nField)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                eFieldLiteWithLib field = new eFieldLiteWithLib(nField, pref.Lang);

                string err = string.Empty;
                dal?.OpenDatabase();
                field.ExternalLoadInfo(dal, out err);
                return field;
            }
            finally
            {
                dal?.CloseDatabase();
            }


        }

    }
}