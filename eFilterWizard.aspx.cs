using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.renderer;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    ///  classe d'assistant de création et de modification de filtre
    /// </summary>
    public partial class eFilterWizard : eEudoPage
    {
        /// <summary>
        /// Appel depuis la fenêtre des traitements de masse
        /// </summary>
        bool _bFromTreat = false;
        /// <summary>
        /// Type de widget, dans le cas où c'est un filtre widget
        /// </summary>
        XrmWidgetType _widgetType = XrmWidgetType.Unknown;

        /// <summary>
        /// descid de la table à la quelle est lié le filtre
        /// </summary>
        public int _tabDid = 0;

        /// <summary>Type de filtre (filtre , règle , etc.)</summary>
        public TypeFilter _filterType = TypeFilter.USER;

        /// <summary>
        /// ID de la Iframe parent ou le wizard est placé
        /// </summary>
        public string _parentIframe = String.Empty;

        /// <summary>Ressources utilisées par le code javascript</summary>
        public string _sJsRes = String.Empty;

        /// <summary>Action à mener </summary>
        public string sAction;
        /// <summary>Nom de l'objet modal appelant</summary>
        public string _modalvarname = String.Empty;
        public string _currentIframeId = String.Empty;
        /// <summary>Date sélectionnée par l'utilisateur en tant que valeur</summary>
        public string strDateValue = String.Empty;
        /// <summary>Date sélectionnée par défaut (Date du jour)</summary>
        public string userDate = DateTime.Now.ToString("dd/MM/yyyy");
        /// <summary>heure sélectionnée par l'utilisateur en tant que valeur</summary>
        public string strHour = String.Empty;
        /// <summary>minute sélectionnée par l'utilisateur en tant que valeur</summary>
        public string strMin = String.Empty;

        enum Action
        {
            Add = 0,
            Update = 1,
            Delete = 2
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// page affichant l'assistant à la création de filtre
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("efilterwizard");
            PageRegisters.AddCss("eCalendar");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("theme");
            PageRegisters.AddCss("eudoFont");
            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");
            #endregion

            #region addjs


            PageRegisters.AddScript("eMd5");
            PageRegisters.AddScript("etools");
            PageRegisters.AddScript("eUpdater");
            //#66723 - Ajout de eMain.js pour récupérer certaines variables globales, mais au dessus de eFilterWizardLight.js et eFilterWizard.js car certaines fonction de eMain.js sont redéfinies
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eFilterWizardLight");
            PageRegisters.AddScript("ePerm");
            PageRegisters.AddScript("eFilterWizard");
            PageRegisters.AddScript("eCalendar");
            PageRegisters.AddScript("ePopup");

            #endregion

            eRightFilter eRF = new eRightFilter(_pref);

            //Pas de gestion d'erreur spécifique, ajout d'un try catch global pour la gestion des erreurs
            try
            {
                int filterId = _requestTools.GetRequestFormKeyI("filterid") ?? 0;

                _tabDid = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                sAction = _requestTools.GetRequestFormKeyS("action");
                _filterType = eLibTools.GetEnumFromCode<TypeFilter>(_requestTools.GetRequestFormKeyI("type") ?? 0);
                bool bAdminMode = _requestTools.GetRequestFormKeyB("adminMode") ?? false;
                bool bSelectFilterMode = _requestTools.GetRequestFormKeyB("selectFilterMode") ?? false;

                if (filterId == 0 && _filterType == TypeFilter.USER && !eRF.CanAddNewItem())      //Droit d'ajout
                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6835)));
                else if (filterId == -1)
                {
                    //Filtre Doublon/Defaut/Relation
                    if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6836)));

                    if (!FilterTools.IsSpecialFilter(_filterType))
                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6836)));

                    filterId = FilterTools.GetSpecialFilterId(_tabDid, _filterType, _pref);
                    if (filterId == 0)
                        sAction = "addnew";
                }
                else if (_filterType == TypeFilter.RULES)
                {
                    if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6836)));
                }
                else if (sAction == "edit" && filterId > 0 && !eRF.CanEditItem())    // droit de modif
                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6836)));

                _modalvarname = _requestTools.GetRequestFormKeyS("modalvarname");
                _currentIframeId = _requestTools.GetRequestFormKeyS("_parentiframeid");

                // Indique si on doit afficher la page avec les feuilles de style pour tablettes
                bool tabletMode = _requestTools.GetRequestFormKeyB("tabletMode") ?? false;

                // Taille de la fenêtre popup - pour adapter l'affichage
                int popupWidth = _requestTools.GetRequestFormKeyI("popupWidth") ?? 0;

                _bFromTreat = _requestTools.GetRequestFormKeyB("fromtreat") ?? false;

                String sFilterName = _requestTools.GetRequestFormKeyS("name");

                if (_filterType == TypeFilter.WIDGET)
                {
                    _widgetType = (XrmWidgetType)(_requestTools.GetRequestFormKeyI("widgetType") ?? XrmWidgetType.Tuile.GetHashCode());
                }

                if (tabletMode)
                {
                    if (popupWidth < 1024)
                        PageRegisters.AddCss("mobile/mobile-styles-portrait");
                    else
                        PageRegisters.AddCss("mobile/mobile-styles-landscape");
                }

                switch (sAction)
                {
                    #region EDITION
                    case "edit":
                        try
                        {
                            MainWizard.Controls.Add(eFilterRenderer.Wizard.GetFilterRender(_pref, filterId, bAdminMode, _widgetType));
                        }
                        catch (eFilterRenderer.eFilterRightException filterRightExc)
                        {
                            // Droits insuffisants
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.EXCLAMATION,
                                eResApp.GetRes(_pref, 6834),
                                eResApp.GetRes(_pref, 8207),
                                eResApp.GetRes(_pref, 6834),
                                filterRightExc.Message);
                        }
                        catch (Exception exp)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                String.Concat(eResApp.GetRes(_pref, 149), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                eResApp.GetRes(_pref, 72),  //   titre
                                String.Concat("Erreur sur eFilterWizard - ", exp.Message)
                            );
                            LaunchErrorHTML(false);
                        }
                        break;
                    #endregion

                    #region Nouveau Filtre
                    case "addnew":
                        try
                        {
                            MainWizard.Controls.Add(eFilterRenderer.Wizard.GetNewFilterRender(_pref, _filterType, _tabDid, bAdminMode, sFilterName, _widgetType));
                        }
                        catch (Exception exp)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                String.Concat(eResApp.GetRes(_pref, 149), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                eResApp.GetRes(_pref, 72),  //   titre
                                String.Concat("Erreur sur eFilterWizard - ", exp.Message)
                            );
                            LaunchErrorHTML(false);
                        }
                        break;
                    #endregion

                    #region FilterName
                    case "filtername":
                        String filterName = Request.Form["filtername"];
                        bool filterNameIsReadOnly = Request.Form["filterNameIsReadOnly"].ToString().Equals("1");

                        Int32 viewPermId = eLibTools.GetNum(Request.Form["viewpermid"].ToString());
                        Int32 viewPermMode = eLibTools.GetNum(Request.Form["viewpermmode"].ToString());
                        String viewPermUsersId = Request.Form["viewpermusersid"].ToString();
                        int viewPermLevel = eLibTools.GetNum(Request.Form["viewpermlevel"].ToString());
                        bool bViewPerm = Request.Form["viewperm"].ToString().Equals("1");

                        Int32 updatePermId = eLibTools.GetNum(Request.Form["updatepermid"].ToString());
                        Int32 updatePermMode = eLibTools.GetNum(Request.Form["updatepermmode"].ToString());
                        String updatePermUsersId = Request.Form["updatepermusersid"].ToString();
                        int updatePermLevel = eLibTools.GetNum(Request.Form["updatepermlevel"].ToString());
                        bool bUpdatePerm = Request.Form["updateperm"].ToString().Equals("1");

                        bool bPublic = eLibTools.GetNum(Request.Form["userid"].ToString()) == 0;

                        ePermissionRenderer premRend = new ePermissionRenderer(_pref, eResApp.GetRes(_pref, 6282), filterName, bPublic, viewPermId, updatePermId, (ePermission.PermissionMode)viewPermMode, viewPermUsersId, viewPermLevel, (ePermission.PermissionMode)updatePermMode, updatePermUsersId, updatePermLevel, eRT: eRF, bLabelReadOnly: filterNameIsReadOnly);
                        if (_filterType == TypeFilter.RULES)
                            premRend.DoAddPermOptions = false;
                        HtmlGenericControl main = premRend.GetSaveAsBlock();
                        RenderResultHTML(main);
                        break;
                    #endregion

                    #region FiltreFormulaire
                    case "filterquestion":
                        try
                        {
                            MainWizard.Controls.Add(eFilterRenderer.Wizard.GetEmptyLinesRender(_pref, filterId, _tabDid));
                        }
                        catch (Exception exp)
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                String.Concat(eResApp.GetRes(_pref, 149), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                eResApp.GetRes(_pref, 72),  //   titre
                                String.Concat("Erreur sur eFilterWizard - ", exp.Message)
                            );
                            LaunchErrorHTML(false);
                        }
                        break;
                    #endregion

                    #region GET LINKED FILE
                    case "getlinkedfile":
                        HtmlGenericControl fromDiv = new HtmlGenericControl("div");
                        HtmlGenericControl fromSpan = new HtmlGenericControl("span");
                        fromSpan.InnerText = eResApp.GetRes(_pref, 535).ToCapitalize();
                        fromDiv.Controls.Add(fromSpan);
                        int firstFile = 0;

                        fromDiv.Controls.Add(eFilterTabRenderer.GetLinkedFileList(_tabDid, _pref, "LinkedFromList", out firstFile));

                        HtmlGenericControl fileDiv = new HtmlGenericControl("div");
                        HtmlGenericControl fileSpan = new HtmlGenericControl("span");
                        HtmlGenericControl fileListdiv = new HtmlGenericControl("div");
                        fileListdiv.Style.Add(HtmlTextWriterStyle.Display, "inline");
                        fileListdiv.ID = "FileListDiv";

                        fileSpan.InnerText = eResApp.GetRes(_pref, 103);
                        fileDiv.Controls.Add(fileSpan);
                        fileDiv.Controls.Add(fileListdiv);

                        fileListdiv.Controls.Add(eFilterTabRenderer.GetLinkedFileList(firstFile, _pref, "LinkedList", out firstFile));

                        LinkFileResult.Controls.Add(fromDiv);
                        LinkFileResult.Controls.Add(fileDiv);
                        break;
                    #endregion

                    #region RELOAD LINKED FILE
                    case "reloadlinkedfile":
                        HtmlGenericControl htmlCtrl = eFilterTabRenderer.GetLinkedFileList(_tabDid, _pref, "LinkedList", out firstFile);
                        RenderResult(RequestContentType.HTML, delegate () { return eTools.GetControlRender(htmlCtrl); });
                        break;
                    #endregion

                    #region CAL SELECT
                    case "calselect":

                        int nRange = 60;
                        string strMove = String.Empty;
                        string strDate = Request.Form["date"].ToString().ToUpper();

                        strDate = strDate.Replace("&LT;", "<").Replace("&GT;", ">");
                        int nType = eLibTools.GetNum(Request.Form["type"].ToString());

                        //strMoveVisibility =" style='visibility:hidden' "
                        bool bMoveVisible = false;

                        //Date Anniversaire
                        bool bNoYear = false; ;
                        if (strDate.Contains("[NOYEAR]"))
                        {
                            strDate = strDate.Replace("[NOYEAR]", "");
                            bNoYear = true;
                        }

                        if (strDate.Contains("+"))
                        {
                            String[] aValue = strDate.Split('+');
                            strDate = aValue[0].Trim();
                            strMove = String.Concat("+", aValue[1].Trim());
                        }
                        else
                        {
                            if (strDate.Contains("-"))
                            {
                                String[] aValue = strDate.Split('-');
                                strDate = aValue[0].Trim();

                                strMove = String.Concat("-", aValue[1].Trim());
                            }
                        }

                        bool bSel0Checked = false;
                        bool bSel1Checked = false;
                        bool bSel2Checked = false;
                        bool bSel3Checked = false;
                        bool bSel4Checked = false;
                        bool bSel5Checked = false;
                        bool bSel6Checked = false;

                        string strLabelMove = String.Empty;

                        switch (strDate)
                        {
                            case "":
                                bSel0Checked = true;
                                break;
                            case "<DATE>":
                                bSel1Checked = true;
                                strLabelMove = eResApp.GetRes(_pref, 853);
                                bMoveVisible = true;
                                //bNoYear = true;
                                break;
                            case "<DATETIME>":
                                bSel2Checked = true;
                                break;
                            case "<MONTH>":
                                bSel4Checked = true;
                                strLabelMove = eResApp.GetRes(_pref, 854);
                                bMoveVisible = true;
                                //bNoYear = true;
                                break;
                            case "<WEEK>":
                                bSel5Checked = true;
                                strLabelMove = eResApp.GetRes(_pref, 852);
                                bMoveVisible = true;
                                break;
                            case "<YEAR>":
                                bSel6Checked = true;
                                strLabelMove = eResApp.GetRes(_pref, 855);
                                bMoveVisible = true;
                                break;
                            case "<DAY>":
                                strLabelMove = eResApp.GetRes(_pref, 1234);
                                bMoveVisible = true;
                                // bNoYear = true;
                                break;
                            default:
                                bSel3Checked = true;
                                strDateValue = strDate;
                                break;
                        }

                        DivDateSelect.Attributes.Add("class", "DateSelect");
                        HtmlGenericControl frmSelect = new HtmlGenericControl("form");
                        frmSelect.ID = "radioOptForm";
                        frmSelect.Attributes.Add("name", "radioOptForm");

                        DivDateSelect.Controls.Add(frmSelect);
                        frmSelect.Controls.Add(eTools.GetRadioButton("0", "date", bSel0Checked, eResApp.GetRes(_pref, 314)));
                        frmSelect.Controls.Add(eTools.GetRadioButton("1", "date", bSel1Checked, eResApp.GetRes(_pref, 367)));
                        frmSelect.Controls.Add(eTools.GetRadioButton("2", "date", bSel2Checked, eResApp.GetRes(_pref, 368)));
                        frmSelect.Controls.Add(eTools.GetRadioButton("3", "date", bSel3Checked, eResApp.GetRes(_pref, 369), false));
                        if (nType == 1)
                        {
                            //DivDateSelect.Controls.Add(eTools.GetRadioButton("7", "date", bSel7Checked, _Res.GetRes(_pref.Lang, 1234)));
                            frmSelect.Controls.Add(eTools.GetRadioButton("4", "date", bSel4Checked, eResApp.GetRes(_pref, 693)));
                            frmSelect.Controls.Add(eTools.GetRadioButton("5", "date", bSel5Checked, eResApp.GetRes(_pref, 694)));
                            frmSelect.Controls.Add(eTools.GetRadioButton("6", "date", bSel6Checked, eResApp.GetRes(_pref, 778)));
                        }
                        else
                        {
                            frmSelect.Controls.Add(eTools.GetEmptyInput("4"));
                            frmSelect.Controls.Add(eTools.GetEmptyInput("5"));
                            frmSelect.Controls.Add(eTools.GetEmptyInput("6"));
                        }

                        HtmlGenericControl _dateInpt = new HtmlGenericControl("input");
                        _dateInpt.Style.Add(HtmlTextWriterStyle.Display, "none");
                        _dateInpt.Attributes.Add("type", "text");
                        _dateInpt.Attributes.Add("class", "InptDate");
                        _dateInpt.Attributes.Add("value", strDateValue);
                        _dateInpt.ID = "dateValue";
                        DivDateSelect.Controls.Add(_dateInpt);

                        // Décalage
                        HtmlGenericControl _moveDiv = new HtmlGenericControl("div");
                        DivDateSelect.Controls.Add(_moveDiv);
                        _moveDiv.Style.Add(HtmlTextWriterStyle.Display, bMoveVisible ? "block" : "none");
                        _moveDiv.ID = "DivMove";
                        HtmlGenericControl _moveLbl = new HtmlGenericControl("div");
                        _moveLbl.Attributes.Add("class", "LblMove");
                        _moveDiv.Controls.Add(_moveLbl);
                        _moveLbl.InnerText = eResApp.GetRes(_pref, 848);

                        HtmlGenericControl _moveLst = new HtmlGenericControl("select");
                        _moveLst.Attributes.Add("class", "LblMove");
                        _moveDiv.Controls.Add(_moveLst);
                        _moveLst.ID = "lstMove";

                        for (int i = -nRange; i <= nRange; i++)
                        {
                            bool bSelect = false;
                            string strSign = i > 0 ? "+" : (i < 0 ? "-" : String.Empty);

                            if (String.IsNullOrEmpty(strMove) && i == 0)
                                bSelect = true;
                            if (strSign + Math.Abs(i) == strMove)
                                bSelect = true;
                            HtmlGenericControl _moveOpt = new HtmlGenericControl("option");
                            _moveLst.Controls.Add(_moveOpt);
                            _moveOpt.Attributes.Add("value", strSign + Math.Abs(i));
                            _moveOpt.InnerText = strSign + Math.Abs(i);
                            if (bSelect)
                                _moveOpt.Attributes.Add("selected", "selected");
                        }

                        HtmlGenericControl _moveLabel = new HtmlGenericControl("span");
                        _moveLabel.ID = "LabelMove";
                        _moveLabel.Attributes.Add("class", "LblMove");
                        _moveLabel.InnerText = strLabelMove;
                        _moveDiv.Controls.Add(_moveLabel);

                        HtmlGenericControl _noyearDiv = new HtmlGenericControl("div");
                        _noyearDiv.ID = "DivNoYear";

                        _moveDiv.Controls.Add(_noyearDiv);

                        _noyearDiv.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(_pref, 1496), "ChkNoYear", bNoYear, false, string.Empty, "onCheckOption"));

                        //Calendar
                        if (strDateValue.Trim().Length == 0)
                            strDateValue = userDate;

                        //La date récupérée est systématiquement au format fr : dd/MM/yyyy 
                        // il faut donc "forcer" le parse à utiliser la culture fr-FR
                        int nHour = DateTime.Parse(strDateValue, CultureInfo.CreateSpecificCulture("fr-Fr")).Hour;
                        int nMin = DateTime.Parse(strDateValue, CultureInfo.CreateSpecificCulture("fr-Fr")).Minute;

                        strHour = string.Empty;
                        strMin = string.Empty;
                        if (nHour + nMin != 0)
                        {
                            if (nHour < 10)
                                strHour = string.Concat("0", nHour);
                            else
                                strHour = nHour.ToString();

                            if (nMin < 10)
                                strMin = "0" + nMin;
                            else
                                strMin = nMin.ToString();
                        }

                        if (_bFromTreat)
                        {
                            tdWizSep.Style.Add("display", "none");
                            tdWizDecal.Style.Add("display", "none");

                        }
                        break;
                    #endregion

                    default:

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref, 72),  //   titre
                            String.Concat("Erreur sur eFilterWizard - Action non prévue = ->", sAction, "<- ")

                            );


                        break;
                }
            }
            catch (eEndResponseException) { Response.End(); }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {
                string sDevMsg = String.Concat("Erreur sur eFilterWizard - Action sur filtre= -> : \n");
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message Exception : ", exp.Message,
                    Environment.NewLine, "Exception StackTrace :", exp.StackTrace
                    );

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    sDevMsg);
            }

            //Lance l'erreur si besoin - Appel via eUpdater
            try
            {
                LaunchError();
            }
            catch (eEndResponseException)
            {

            }
        }
    }
}