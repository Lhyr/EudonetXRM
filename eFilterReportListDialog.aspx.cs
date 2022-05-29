using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.import;
using EudoQuery;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Page affichant une liste de filtre/report
    /// </summary>
    public partial class eFilterReportListDialog : eEudoPage
    {
        private int _nUserId = 0;
        private int _nType = -1;
        private int _nTabBkm = -1;
        private int _nTabFrom = -1;
        private int _nParentFileId = -1;
        private EdnType _listType;
        private eRenderer _myMainList;
        private int _nUserLevel = 0;
        private string _lang = string.Empty;
        private int _nFileId = 0;
        private int _nBkmFileId = 0;
        private IRightTreatment _oRightManager;
        private Size winSize = new Size(800, 600);
        private TypeReport _typReport = TypeReport.NONE;
        private TypeMailTemplate _typTemplate = TypeMailTemplate.MAILTEMPLATE_EMAIL;

        private bool _deselectAllowed = false;

        private bool _adminMode = false;
        private int _formularType = 0;

        /// <summary>
        /// Mode de selection de filtre sans en appliquer celui-ci
        /// </summary>
        private bool _selectFilterMode = false;


        #region Propriétés Publiques

        /// <summary>
        /// Chaine contenant le code javascript "ouvert" à déclarer dynamiquement
        /// ex : chargement d'une variable Iframe àpartir de l'id de la modal appelante
        /// </summary>
        public string _sGeneratedJavaScript = string.Empty;

        /// <summary>
        /// Filtre en cours
        /// </summary>
        public int _activeFilter = 0;

        /// <summary>
        /// IFrame de la frm ModalDialog de la liste 
        /// </summary>
        public string _sIframeId = string.Empty;
        /// <summary>
        /// Onglet en cours
        /// </summary>
        public int _nTab = 0;

        /// <summary>
        /// Chaine contenant le javascript des function js dynamique à ajouter au chargement de la page
        /// </summary>
        public string _strAddFunction = string.Empty;

        /// <summary>
        /// Chaine contenant le javascript de l'évènement onLoad du body de la page :
        /// (initFilterList pour la liste des filtres, initReportList pour la liste des rapports)
        /// </summary>
        public string _BodyLoadJavascript = string.Empty;

        /// <summary>
        /// Chaine générant la liste de sélection des différent type de Reporting pour changer depuis la liste
        /// </summary>
        public string _strReportSwitch = string.Empty;

        /// <summary>
        /// Chaine contenant le libellé de la fonction "ajouter" de la liste
        /// </summary>
        public string _strFunctionLabel = string.Empty;

        public bool _fromEmailing = false;

        public int TabList = 0;
        /// <summary>Options de la liste de modèles de mail</summary>
        public string StrMailTemplatesOptions = string.Empty;

        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder ()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load (object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eActions");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eActionList");
            PageRegisters.AddCss("eCatalog");

            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");

            #endregion

            #region add js
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eButtons");
            PageRegisters.AddScript("eList");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eExpressFilter");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eEngine");
            PageRegisters.AddScript("eFilterReportListDialog");
            PageRegisters.AddScript("eReportCommon");
            PageRegisters.AddScript("eEvent");
            // eMailingWizard.js est ajouté dans le cas de l'affichage des modèles de mails unitaires (cf. ci-dessous)
            #endregion

            #region Variables de session


            _nUserLevel = _pref.User.UserLevel;
            HashSet<string> allKeys = new HashSet<string>(Request.Form.AllKeys);

            _nType = -1;
            if (allKeys.Contains("type") && !string.IsNullOrEmpty(Request.Form["type"]))
                _nType = eLibTools.GetNum(Request.Form["type"].ToString());

            if (allKeys.Contains("tab") && !string.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());

            if (allKeys.Contains("fid") && !string.IsNullOrEmpty(Request.Form["fid"]))
                _nFileId = eLibTools.GetNum(Request.Form["fid"].ToString());

            _nBkmFileId = _requestTools.GetRequestFormKeyI("bkmfid") ?? 0;

            if (_nBkmFileId > 0)
                _nFileId = _nBkmFileId;

            if (allKeys.Contains("tabBKM") && !string.IsNullOrEmpty(Request.Form["tabBKM"]))
                _nTabBkm = eLibTools.GetNum(Request.Form["tabBKM"].ToString());

            if (allKeys.Contains("parentFileId") && !string.IsNullOrEmpty(Request.Form["parentFileId"]))
                _nParentFileId = eLibTools.GetNum(Request.Form["parentFileId"].ToString());

            if (allKeys.Contains("value") && !string.IsNullOrEmpty(Request.Form["value"]))
                _activeFilter = eLibTools.GetNum(Request.Form["value"].ToString());

            if (allKeys.Contains("frmId") && !string.IsNullOrEmpty(Request.Form["frmId"]))
            {
                _sIframeId = Request.Form["frmId"].ToString();
                _sGeneratedJavaScript = "var oIframe = top.document.getElementById('" + _sIframeId + "');      ";
            }

            if (allKeys.Contains("lstType") && !string.IsNullOrEmpty(Request.Form["lstType"]))
                _listType = (EdnType)Enum.Parse(typeof(EdnType), Request.Form["lstType"].ToString());

            if (allKeys.Contains("deselectAllowed") && !string.IsNullOrEmpty(Request.Form["deselectAllowed"]))
                _deselectAllowed = Request.Form["deselectAllowed"].ToString() == "1";

            if (allKeys.Contains("adminMode") && !string.IsNullOrEmpty(Request.Form["adminMode"]))
                _adminMode = Request.Form["adminMode"].ToString() == "1";

            //AAB tâche 1 882
            if (allKeys.Contains("formularType") && !string.IsNullOrEmpty(Request.Form["formularType"]))
                _formularType = eLibTools.GetNum(Request.Form["formularType"].ToString());

            _selectFilterMode = _requestTools.GetRequestFormKeyB("selectFilterMode") ?? false;

            switch (_listType)
            {
                case EdnType.FILE_FILTER:
                    TabList = TableType.FILTER.GetHashCode();
                    break;
                case EdnType.FILE_REPORT:
                    TabList = TableType.REPORT.GetHashCode();
                    _typReport = (TypeReport)_nType;
                    break;
                case EdnType.FILE_FORMULARXRM:
                    TabList = TableType.FORMULARXRM.GetHashCode();
                    break;
                case EdnType.FILE_MAILTEMPLATE:

                    _typTemplate = eLibTools.GetEnumFromCode<TypeMailTemplate>(_nType);
                        

                    TabList = TableType.MAIL_TEMPLATE.GetHashCode();
                    PageRegisters.AddScript("ePerm");
                    PageRegisters.AddScript("eMailingTpl");
                    PageRegisters.AddScript("eMailingWizard");
                    PageRegisters.AddScript("eMailing");
                    _sGeneratedJavaScript = string.Concat(
                        _sGeneratedJavaScript, Environment.NewLine,
                        "   var _eCurrentSelectedMailTpl = null;", Environment.NewLine,
                        "   var _ePopupVNEditor;", Environment.NewLine,
                        "   var _eMailTplNameEditor;", Environment.NewLine
                    );
                    // Les variables requises par les scripts ci-dessus seront ajoutées par initMailTpl(), appelée par le onLoad() de la page
                    break;
                case EdnType.FILE_IMPORTTEMPLATE:
                    int activeImportTemplate = 0;
                    if (allKeys.Contains("activeImportTemplate") && !string.IsNullOrEmpty(Request.Form["activeImportTemplate"]))
                        activeImportTemplate = eLibTools.GetNum(Request.Form["activeImportTemplate"].ToString());
                    TabList = (int)TableType.IMPORT_TEMPLATE;
                    _sGeneratedJavaScript = string.Concat(
                       _sGeneratedJavaScript, Environment.NewLine,
                       "   var eCurrentSelectedTemplateImport = top.oImportWizard.Wizard.ImportWizardInternal;", Environment.NewLine
                   , " var _activeImportTemplate = ", activeImportTemplate, ";", Environment.NewLine);
                    break;
                default:
                    break;
            }

            if (_nTabBkm > 0)
                _nTabFrom = _nTabBkm;
            else
                _nTabFrom = _nTab;

            int nPage = 0;
            if (allKeys.Contains("page"))
                nPage = eLibTools.GetNum(Request.Form["page"].ToString()); // Page

            if (nPage < 1)
                nPage = 1;

            if (allKeys.Contains("fromemailing"))
                _fromEmailing = eLibTools.GetNum(Request.Form["fromemailing"].ToString()) == 1; // Page

            #endregion

            #region Génération de la liste

            // Table de la sélection         
            _nUserId = _pref.User.UserId;

            // US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
            // (Même si non réellement utilisé dans ce contexte)
            IDictionary<eLibConst.CONFIGADV, string> dicEmailAdvConfig = eLibTools.GetConfigAdvValues(_pref,
                new HashSet<eLibConst.CONFIGADV> {
                                eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD
                });
            string useNewUnsubscribeMethodValue = String.Empty;
            bool useNewUnsubscribeMethod = false;
            if (dicEmailAdvConfig.TryGetValue(eLibConst.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, out useNewUnsubscribeMethodValue) && useNewUnsubscribeMethodValue == "1")
                useNewUnsubscribeMethod = true;

            // ASY : (#31582) Ajouter Tous dans l'assistant Rapport - Supprimer le bouton Nouveau rapport lorsqu'on selectionne tous
            //if (_nTabFrom > 0 && _nUserId > 0 && _nType >= 0 && _nType <= 7)                   
            if ((
                _nTabFrom > 0 && _nUserId > 0 && _nType >= 0 && (_nType <= 7 
                || _nType == TypeReport.ALLFORWIZARD.GetHashCode() || _typReport == TypeReport.FIELDCHART))
                || (TabList == TableType.FILTER.GetHashCode() && _nType == (int)TypeFilter.NOTIFICATION)
                || (_nType == -1 && TabList == (int)TableType.IMPORT_TEMPLATE))
            {
                string strEdnType = "report";
                switch (_listType)
                {
                    case EdnType.FILE_FILTER:
                        _oRightManager = new eRightFilter(_pref);
                        strEdnType = "filter";
                        _BodyLoadJavascript = "initFilterList();";
                        break;

                    case EdnType.FILE_FORMULARXRM:
                        _oRightManager = new eRightFormular(_pref);
                        strEdnType = "formular";
                        _BodyLoadJavascript = "initReportList();"; // TODOMAB à valider avec MOU
                        break;

                    case EdnType.FILE_MAILTEMPLATE:
                        _oRightManager = new eRightMailTemplate(_pref);
                        strEdnType = "mailtemplate";
                        _BodyLoadJavascript = string.Concat("initMailTpl(true, ", _nTabBkm, ", ", _nTab, ", ", _nParentFileId, ");");
                        break;

                    case EdnType.FILE_REPORT:
                        _oRightManager = new eRightReport(_pref, (TypeReport)_nType);
                        strEdnType = "report";
                        _BodyLoadJavascript = "initReportList();";
                        break;

                    case EdnType.FILE_IMPORTTEMPLATE:
                        strEdnType = "importtemplate";
                        _BodyLoadJavascript = string.Concat("initImportTpl(true, ", _nTabBkm, ", ", _nTab, ", ", _nParentFileId, ");");
                        break;
                    default:
                        break;
                }

                if (allKeys.Contains("width"))
                    winSize.Width = eLibTools.GetNum(Request.Form["width"].ToString());

                if (allKeys.Contains("height"))
                    winSize.Height = eLibTools.GetNum(Request.Form["height"].ToString());

                //La largeur de la liste de choix des rapports en popup sur Chrome/Safari IPad n'est pas calculée correctement. #37028             
                //filterListGlobal.Style.Add(HtmlTextWriterStyle.Width, winSize.Width + "px");
                filterListGlobal.Style.Add(HtmlTextWriterStyle.Width, "100%");
                filterListGlobal.Style.Add(HtmlTextWriterStyle.Height, "calc(100% - 40px)");

                //Génration du renderer

                _myMainList = LoadList(_listType, _pref);

                if (_myMainList.ErrorMsg.Length == 0)
                {

                    mainDiv.Attributes.Add("edntype", strEdnType);
                    mainDiv.Attributes.Add("type", _nType.ToString());
                    mainDiv.Attributes.Add("tabBKM", _nTabBkm.ToString());
                    mainDiv.Attributes.Add("pfid", _nParentFileId.ToString());
                    mainDiv.Attributes.Add("tab", _nTab.ToString());
                    mainDiv.Attributes.Add("deselectAllowed", _deselectAllowed ? "1" : "0");
                    mainDiv.Attributes.Add("adminMode", _adminMode ? "1" : "0");
                    mainDiv.Attributes.Add("selectFilterMode", _selectFilterMode ? "1" : "0");

                    // ASY : (#31582) Ajouter Tous dans l'assistant Rapport - Supprimer le bouton Nouveau rapport lorsqu'on selectionne tous
                    if (
                            _nType != TypeReport.ALLFORWIZARD.GetHashCode()  // Pas de add pour tous
                            && !((_listType == EdnType.FILE_REPORT && _nType == TypeReport.SPECIF.GetHashCode())) //pas de add pour raport de type spécif
                        )
                    {

                        if (_oRightManager?.CanAddNewItem() ?? false)
                        {
                            switch (_listType)
                            {
                                case EdnType.FILE_FILTER:
                                    TypeFilter tf = TypeFilter.USER;
                                    Enum.TryParse<TypeFilter>(_nType.ToString(), out tf);
                                    _strFunctionLabel = eResApp.GetRes(_pref, 6247); //6247 : Nouveau Filtre
                                    switch (tf)
                                    {
                                        case TypeFilter.USER:
                                            _strFunctionLabel = eResApp.GetRes(_pref, 6247); //6247 : Nouveau Filtre
                                            break;
                                        case TypeFilter.RULES:
                                            _strFunctionLabel = eResApp.GetRes(_pref, 7173); //6247 : Nouvelle règle
                                            break;
                                        default:
                                            break;
                                    }

                                    if (_adminMode)
                                        _strAddFunction = string.Concat("AddNewFilterV2(", _nTabFrom, ", null, true, false);");
                                    else if (_selectFilterMode)
                                        _strAddFunction = string.Concat("AddNewFilterV2(", _nTabFrom, ", null, false, true);");
                                    else
                                        _strAddFunction = string.Concat("AddNewFilter(", _nTabFrom, ");");


                                    break;

                                case EdnType.FILE_FORMULARXRM:

                                    //A le droit d'jout et qu'il ne vient pas depuis l'emailing
                                    if (!_fromEmailing)
                                    {
                                        _strFunctionLabel = eResApp.GetRes(_pref, 1726); //1726 : Nouveau Formulaire //2076
                                        _strAddFunction = string.Concat("AddNewFormular(", _nTabBkm, ", ", _nParentFileId, ", ", _formularType, ");");

                                    }
                                    else
                                        _strAddFunction = string.Empty;
                                    break;

                                case EdnType.FILE_MAILTEMPLATE:
                                    _strFunctionLabel = eResApp.GetRes(_pref, 327);
                                    // grapesjs désactivé sur les mails unitaires pour le moment (cf. backlog #43) - Pour réactiver, décommenter la condition ci-dessous
                                    // US #1002 - Tâche #1732 - Passage du type de lien de désinscription à afficher (avec ou sans gestion de consentements)
                                    _strAddFunction = string.Concat("AddNewModele(", /*_nTabBkm, ", ", _nParentFileId,*/ "null, ", TypeMailTemplate.MAILTEMPLATE_EMAIL.GetHashCode(), ", ", /*eFeaturesManager.IsFeatureAvailable(_pref, eConst.XrmFeature.HTMLTemplateEditor) ? "true" :*/ "false", ", ", useNewUnsubscribeMethod ? "true" : "false", ");");

                                    break;

                                case EdnType.FILE_REPORT:
                                    _strFunctionLabel = eResApp.GetRes(_pref, 6367); //6367 : Nouveau Rapport
                                    if (_nType == (int)TypeReport.FIELDCHART)
                                        _strAddFunction = string.Concat("AddReport(", _nTabFrom, ",", (int)TypeReport.CHARTS, ");");
                                    else
                                        _strAddFunction = string.Concat("AddReport(", _nTabFrom, ",", _nType, ");");

                                    break;
                                case EdnType.FILE_IMPORTTEMPLATE:
                                    GetLstSelPagging.Attributes.Add("class", "paggingImportPopupList");
                                    break;


                                default:
                                    break;
                            }

                            //JAS : Affichage de la liste déroulante permettant de changer le type de reporting de la liste en cours.

                            btnAdd.Attributes.Add("onclick", _strAddFunction);
                            btnAdd.Attributes.Add("class", "buttonAdd");
                            HtmlGenericControl icnSpan = new HtmlGenericControl("span");
                            icnSpan.Attributes.Add("class", "icon-add");

                            HtmlGenericControl span = new HtmlGenericControl("span");
                            span.InnerHtml = _strFunctionLabel;
                            span.Attributes.Add("class", "catToolAddLibSp");

                            if (_strAddFunction.Length > 0)
                                btnAdd.Controls.Add(icnSpan);

                            btnAdd.Controls.Add(span);

                            if (_listType == EdnType.FILE_MAILTEMPLATE
                                &&
                                (_typTemplate == TypeMailTemplate.MAILTEMPLATE_SMS
                                || _typTemplate == TypeMailTemplate.MAILTEMPLATE_SMSING))
                                {
                                catToolAdd.Visible = false; //ALISTER => Demande 90556 / Request 90556
                                btnAdd.Visible = false;
                            }
                        }
                    }


                    if (_listType.Equals(EdnType.FILE_REPORT))
                    {
                        eRightReport oRightManagerReport = _oRightManager as eRightReport;

                        StringBuilder divBuilder = new StringBuilder();

                        divBuilder.Append("<div id=\"listHeader\" class=\"listHeader\">").Append(Environment.NewLine).Append("\t")
                        .Append("<span>" + eResApp.GetRes(_pref, 6607) + "</span>").Append(Environment.NewLine).Append("\t\t")
                        .Append(Environment.NewLine).Append("\t\t")
                        .Append("<select style=\"width:150px;\" id=\"lstreportype\" onchange=\"ReloadReportList(this);\">")
                        .Append(Environment.NewLine).Append("\t\t\t");


                        if (_typReport != TypeReport.FIELDCHART)
                        {
                            // ASY : Ajout de tous dans la liste - checker les droits sur tous (99?)
                            divBuilder.Append("<option value=\"99\"").Append(_typReport == TypeReport.ALLFORWIZARD ? " selected " : "").Append(">")
                                .Append(eResApp.GetRes(_pref, 435)).Append("</option>")//Tous
                            .Append(Environment.NewLine).Append("\t\t\t");

                            if (oRightManagerReport.HasRight(eLibConst.TREATID.PRINT))//droit d impression
                            {
                                divBuilder.Append("<option value=\"0\"").Append(_typReport == TypeReport.PRINT ? " selected " : "").Append(">")
                                    .Append(eResApp.GetRes(_pref, 6451)).Append("</option>")//Impression
                                .Append(Environment.NewLine).Append("\t\t\t");
                            }

                            if (oRightManagerReport.HasRight(eLibConst.TREATID.EXPORT))//droit d export
                            {
                                divBuilder.Append("<option value=\"2\"").Append(_typReport == TypeReport.EXPORT ? " selected " : "").Append(">")
                                    .Append(eResApp.GetRes(_pref, 6303)).Append("</option>") //export
                                .Append(Environment.NewLine).Append("\t\t\t");
                            }

                            if (oRightManagerReport.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_WORD)
                                || oRightManagerReport.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_HTML)
                                || oRightManagerReport.HasRight(eLibConst.TREATID.PUBLIPOSTAGE_PDF))//droit de publipostage
                            {
                                divBuilder.Append("<option value=\"3\"").Append(_typReport == TypeReport.MERGE ? " selected " : "").Append(">")
                                    .Append(eResApp.GetRes(_pref, 438)).Append("</option>") //Publipostage
                                .Append(Environment.NewLine).Append("\t\t\t");
                            }
                        }

                        if (oRightManagerReport.HasRight(eLibConst.TREATID.GRAPHIQUE) && _nBkmFileId == 0)//droit de faire des graphiques
                        {
                            divBuilder.Append("<option value=\"6\"").Append(_typReport == TypeReport.CHARTS || _typReport == TypeReport.FIELDCHART ? " selected " : "").Append(">")
                                .Append(eResApp.GetRes(_pref, 1005)).Append("</option>")//Graphique
                            .Append(Environment.NewLine).Append("\t\t");
                        }

                        if (_typReport != TypeReport.FIELDCHART)
                        {
                            //Rapport spécifiques
                            divBuilder.Append("<option value=\"4\"").Append(_typReport == TypeReport.SPECIF ? " selected " : "").Append(">")
                               .Append(eResApp.GetRes(_pref, 6405)).Append("</option>")
                               .Append(Environment.NewLine).Append("\t\t");
                        }
                        divBuilder.Append("</select>").Append("\t")
                        .Append(Environment.NewLine).Append("\t\t")
                        .Append("</div>")
                        .Append(Environment.NewLine).Append("\t")
                        .Append(Environment.NewLine).Append("\t");

                        this._strReportSwitch = divBuilder.ToString();
                    }
                    else if (_listType.Equals(EdnType.FILE_MAILTEMPLATE)
                        && _typTemplate != TypeMailTemplate.MAILTEMPLATE_SMS
                        && _typTemplate != TypeMailTemplate.MAILTEMPLATE_SMSING
                        )
                    {
                        // Récupération de l'ID du modèle par défaut
                        // #58 412 : filtrage sur le UserID et le TabID, afin de ne récupérer que les modèles concernant l'utilisateur et l'onglet en cours
                        IDictionary<eLibConst.PREFADV, string> dicPrefAdv = eLibTools.GetPrefAdvValues(_pref,
                             new HashSet<eLibConst.PREFADV>() {
                            eLibConst.PREFADV.DEFAULT_EMAILTEMPLATE,
                            },
                            _pref.UserId,
                            _nTab
                        );
                        string valueDefaultTplID = string.Empty;
                        dicPrefAdv.TryGetValue(eLibConst.PREFADV.DEFAULT_EMAILTEMPLATE, out valueDefaultTplID);

                        //HtmlHiddenField hidDefaultMailTpl = new HiddenField();
                        //hidDefaultMailTpl.ID = "HidDefaultTplID";
                        //hidDefaultMailTpl.Value = valueDefaultTplID;
                        //listOptions.Controls.Add(hidDefaultMailTpl);

                        HtmlGenericControl c = new HtmlGenericControl("input");
                        c.Attributes.Add("type", "hidden");
                        c.ID = "HidDefaultTplID";
                        c.Attributes.Add("value", valueDefaultTplID);
                        listOptions.Controls.Add(c);

                        //#region Bouton "Modèle par défaut"
                        eCheckBoxCtrl checkbox = new eCheckBoxCtrl(false, false);
                        checkbox.ID = "DefaultTemplate";
                        checkbox.Attributes.Add("mtid", "");
                        checkbox.AddText(eResApp.GetRes(_pref, 6900));
                        checkbox.AddClick("onDefaultTemplateCheck(this, 1)");
                        listOptions.Controls.Add(checkbox);

                        //this.StrMailTemplatesOptions = 
                        //#endregion
                    }


                    //déplace les éléments du conteneur généré (myMainList) vers le conteneur final (listcontent)
                    // On ne peut pas ajouter directement myMainList dans listcontent : il ne faut 
                    // pas ajouter le div englobant de myMainList (listContent.Controls.resp(_myMainList.PgContainer);)
                    // , cela perturbe js et css               
                    while (_myMainList.PgContainer.Controls.Count > 0)
                    {
                        listContent.Controls.Add(_myMainList.PgContainer.Controls[0]);
                    }

                    // Paging pour les modes filtres
                    switch (_listType)
                    {
                        case EdnType.FILE_FILTER:
                            ((eFilterListRenderer)_myMainList).CreatePagingBar(GetLstSelPagging);
                            break;
                        case EdnType.FILE_FORMULARXRM:
                            ((eFormularListRenderer)_myMainList).CreatePagingBar(GetLstSelPagging);
                            break;
                        case EdnType.FILE_MAILTEMPLATE:
                            ((eMailTemplateListRendrer)_myMainList).CreatePagingBar(GetLstSelPagging);
                            break;
                        case EdnType.FILE_IMPORTTEMPLATE:
                            ((eImportTemplateListRenderer)_myMainList).CreatePagingBar(GetLstSelPagging);
                            break;

                        default:
                            GetLstSelPagging.Visible = false;
                            break;
                    }

                }
                else
                {

                    if (_myMainList.ErrorNumber == QueryErrorType.ERROR_NUM_TREATMENT_NOT_ALLOWED)
                    {

                        ErrorContainer = eErrorContainer.GetUserError(
                       eLibConst.MSG_TYPE.INFOS,
                       eResApp.GetRes(_pref, 259),
                       string.Concat(_myMainList.ErrorMsg, "<br />", eResApp.GetRes(_pref, 6721)),
                       eResApp.GetRes(_pref, 5080));
                    }
                    else
                    {

                        string sDevMsg = string.Concat("Erreur sur eFilterReportListDialog - Chargement de la liste : \n", _myMainList.ErrorMsg);

                        if (_myMainList.InnerException != null)
                        {
                            sDevMsg = string.Concat(sDevMsg, Environment.NewLine, Environment.NewLine, "Message Exception : ", _myMainList.InnerException.Message,
                                Environment.NewLine, "Exception StackTrace :", _myMainList.InnerException.StackTrace

                                );
                            /// ABBA bug #76 847 
                            if (_myMainList.InnerException is EudoException)
                            {
                                var title = ((EudoException)_myMainList.InnerException).UserMessageTitle;
                                var details = ((EudoException)_myMainList.InnerException).UserMessageDetails;
                                var messageuser = ((EudoException)_myMainList.InnerException).UserMessage;

                                ErrorContainer = eErrorContainer.GetDevUserError(
                              eLibConst.MSG_TYPE.INFOS,
                              eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                              string.Concat( messageuser , "<br />", eResApp.GetRes(_pref, 6721)),  //  Détail : pour améliorer...
                              eResApp.GetRes(_pref, 5080),  //   titre
                               sDevMsg);
                            }
                            else
                            {
                                //"\n Message Exception
                                ErrorContainer = eErrorContainer.GetDevUserError(
                                    eLibConst.MSG_TYPE.CRITICAL,
                                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                    string.Concat(eResApp.GetRes(_pref, 422), "<br />", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                    eResApp.GetRes(_pref, 72),  //   titre
                                 sDevMsg);
                            }

                        }

                    }
                }
            }
            else
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    string.Concat(eResApp.GetRes(_pref, 422), "<br />", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    string.Concat("Erreur sur eFilterReportListDialog - Chargement des paramètres de querystring : _nTab = ->", _nTab, "_nTabBkm = ->", _nTabBkm, "<- type= ->", _nType, "<-")

                    );



            }
            #endregion

            //Lance l'erreur si besoin
            try
            {
                LaunchError();
            }
            catch (eEndResponseException)
            {

            }

        }


        /// <summary>
        /// Charge les paramètres adaptés à la liste affichée
        /// </summary>
        protected eRenderer LoadList (EdnType fileType, ePref pref)
        {
            string ListFilterField = string.Empty;
            TableType listType = TableType.FILTER;

            switch (fileType)
            {
                case EdnType.FILE_FILTER:
                    listType = TableType.FILTER;
                    break;
                case EdnType.FILE_FORMULARXRM:
                    listType = TableType.FORMULARXRM;
                    break;
                case EdnType.FILE_MAILTEMPLATE:
                    listType = TableType.MAIL_TEMPLATE;
                    break;
                case EdnType.FILE_IMPORTTEMPLATE:
                    listType = TableType.IMPORT_TEMPLATE;
                    break;
                default:
                    listType = TableType.REPORT;
                    break;
            }


            List<SetParam<ePrefConst.PREF_PREF>> prefFilter = new List<SetParam<ePrefConst.PREF_PREF>>();

            // Affectation du champ de filtre actif par défaut : Appartient à
            switch (listType)
            {
                case TableType.FILTER:
                    if (_nType == (int)TypeFilter.USER)
                    {
                        ListFilterField = FilterField.USERID.GetHashCode().ToString();

                        if (pref.Context.Filters.ContainsKey(_nTabFrom))
                            _activeFilter = pref.Context.Filters[_nTabFrom].FilterSelId;
                    }
                    break;
                case TableType.REPORT:
                    ListFilterField = ReportField.USERID.GetHashCode().ToString();
                    break;
                case TableType.FORMULARXRM:
                    ListFilterField = FormularField.USERID.GetHashCode().ToString();
                    break;
                case TableType.MAIL_TEMPLATE:
                    ListFilterField = MailTemplateField.OWNERUSER.GetHashCode().ToString();
                    break;
                case TableType.IMPORT_TEMPLATE:
                    ListFilterField = ((int)ImportTemplateField.USERID).ToString();
                    break;
            }

            // RAZ le filtre sur la liste de filtre : remet le filtre sur l'utilisateur en cours

            if (!string.IsNullOrEmpty(ListFilterField))
            {
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERCOL, ListFilterField));
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTEROP, EudoQuery.Operator.OP_EQUAL.GetHashCode().ToString()));        // egale à
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERVALUE, pref.User.UserId.ToString())); // utilisateur en cours
            }
            else
            {
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERCOL, ""));
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTEROP, ""));
                prefFilter.Add(new SetParam<ePrefConst.PREF_PREF>(ePrefConst.PREF_PREF.BKMFILTERVALUE, ""));
            }


            pref.SetPref((int)listType, prefFilter);
            switch (listType)
            {
                case TableType.FILTER:
                    return eRendererFactory.CreateFilterListRenderer(pref, _nTabFrom, _oRightManager, (TypeFilter)_nType, 1,
                        deselectAllowed: _deselectAllowed, adminMode: _adminMode, selectFilterMode: _selectFilterMode);
                case TableType.FORMULARXRM:
                    //AAB tâche 1 882
                    FORMULAR_TYPE formularType = eLibTools.GetEnumFromCode<FORMULAR_TYPE>(_formularType, true);
                    return eRendererFactory.CreateFormularListRenderer(pref, winSize.Width, winSize.Height, _nTabFrom, _oRightManager, new eFormularListFilterParams() { AddPublicItem = true, FormularType = formularType }, 1, bHideActionBtn: _fromEmailing);
                case TableType.MAIL_TEMPLATE:
                    return eRendererFactory.CreateMailTemplateListRenderer(pref, winSize.Width, winSize.Height, _nTabFrom, (TypeMailTemplate)_nType, _oRightManager);
                case TableType.REPORT:
                    return eRendererFactory.CreateReportListRenderer(pref, winSize.Width, winSize.Height, _nTabFrom, _typReport, _oRightManager, nFileId: _nFileId,
                        deselectAllowed: _deselectAllowed);
                case TableType.IMPORT_TEMPLATE:
                    return eRendererFactory.CreateImportTemplateListRenderer(pref, winSize.Width, winSize.Height, _nTabFrom);
            }
            return null;
        }




    }
}