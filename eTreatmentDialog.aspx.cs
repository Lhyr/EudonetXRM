using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Page pour les traitements de masse
    /// </summary>
    public partial class eTreatmentDialog : eEudoPage
    {

        private Int32 _nUserId = 0;
        /// <summary>Tab de provenance :
        ///     - pour PP : si adresse sélectionné depuis PP alorssera égal à PP
        ///     - pour PM : si adresse sélectionné depuis PM alorssera égal à PM
        ///     - pour les autres sera l'événement sélectionné
        ///     </summary>
        private Int32 _nTabFrom = -1;
        /// <summary>Tab en cours :
        ///     - pour PP : si adresse sélectionné depuis PP alorssera égal à adresse)
        ///     - pour PM : si adresse sélectionné depuis PM alorssera égal à adresse)
        ///     - pour les autres sera l'événement sélectionné -idem à tabfrom
        ///     </summary>
        private Int32 _nTargetTab = -1; //TODO : Gérer le cas de la sélection d'adresse depuis PP ou PM
        private int _nUserLevel = 0;
        private String _lang = String.Empty;

        #region Propriétés Publiques

        /// <summary>
        /// Chaine contenant le code javascript "ouvert" à déclarer dynamiquement
        /// ex : chargement d'une variable Iframe àpartir de l'id de la modal appelante
        /// </summary>
        public StringBuilder _generatedJavaScript = new StringBuilder();

        /// <summary>
        /// IFrame de la frm ModalDialog de la liste 
        /// </summary>
        private String _sIframeId = String.Empty;
        /// <summary>
        /// Onglet en cours
        /// </summary>
        private Int32 _nTab = 0;
        /// <summary>
        /// Nombre total de fiches sur la liste précédente
        /// </summary>
        private int _nTotalCount = 0;
        /// <summary>
        /// Nombre total de fiches affichées sur la liste précédente
        /// </summary>
        private int _nCount = 0;

        #endregion

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region ajout des css

            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eList");
            PageRegisters.AddCss("eIcon");
            PageRegisters.AddCss("eControl");
            PageRegisters.AddCss("eTitle");
            PageRegisters.AddCss("eActions");
            PageRegisters.AddCss("eContextMenu");
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eTreatment");

            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");
            #endregion


            #region ajout des js



            PageRegisters.AddScript("eButtons");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("ePopup");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eContextMenu");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eFieldEditor");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eTreatment");
            #endregion

            #region Variables de session

            _nUserLevel = _pref.User.UserLevel;
            HashSet<String> allKeys = new HashSet<String>(Request.Form.AllKeys);

            if (allKeys.Contains("tab") && !String.IsNullOrEmpty(Request.Form["tab"]))
                _nTab = eLibTools.GetNum(Request.Form["tab"].ToString());
            _nTabFrom = _nTab;
            _nTargetTab = _nTabFrom;
            //Si l'on vient de contact/société on peut sélectionné Adresse donc tabfrom sera égal à 200/300 et targetTab à 400 sinon ils seront toujours égaux
            if (allKeys.Contains("targetTab") && !String.IsNullOrEmpty(Request.Form["targetTab"]))
            {
                _nTargetTab = eLibTools.GetNum(Request.Form["targetTab"].ToString());

            }


            Boolean bFilterAvailable = true;
            string strPpTabName = "";
            string strAdrTabName = "";
            string strPmTabName = "";
            IDictionary<int, string> diTablabels = new Dictionary<int, string>();
            String sError = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            TableLite tabFrom = null;
            TableLite tabTarget = null;
            try
            {
                dal.OpenDatabase();
                tabFrom = new EudoQuery.TableLite(_nTabFrom);
                tabFrom.ExternalLoadInfo(dal, out sError);
                if (sError.Length > 0)
                    throw new Exception(sError);
                if (_nTargetTab == _nTabFrom)
                    tabTarget = tabFrom;
                else
                {
                    tabTarget = new EudoQuery.TableLite(_nTargetTab);
                    tabTarget.ExternalLoadInfo(dal, out sError);
                    if (sError.Length > 0)
                        throw new Exception(sError);
                }

                List<int> liTab = new List<int>() {
                    _nTabFrom
                    ,(int)TableType.ADR
                    ,(int)TableType.PM
                    ,(int)TableType.PP
                    ,_nTargetTab
                };

                diTablabels = eLibTools.GetPrefName(dal, _pref.Lang, liTab);

            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                    String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                    eResApp.GetRes(_pref, 72),  //   titre
                    String.Concat("Erreur sur eTreatmentDialog - Récup de EudoQuery.TableLite : _nTab = ->", _nTab, " : ", ex.ToString())
                );
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();
            }
            LaunchError();

            string strCurrentTabName = diTablabels[_nTabFrom]; // eLibTools.GetPrefName(_pref, _nTabFrom);



            if (allKeys.Contains("frmId") && !String.IsNullOrEmpty(Request.Form["frmId"]))
            {
                _sIframeId = Request.Form["frmId"].ToString();
                _generatedJavaScript.Append("var oIframe = top.document.getElementById('").Append(_sIframeId).Append("');").AppendLine();
            }
            if (_nTargetTab == TableType.PP.GetHashCode())
            {
                strPpTabName = strCurrentTabName;
                strPmTabName = diTablabels[(int)TableType.PM];// eLibTools.GetPrefName(_pref, TableType.PM.GetHashCode());
                strAdrTabName = diTablabels[(int)TableType.ADR];
            }
            else if (_nTargetTab == TableType.PM.GetHashCode())
            {
                strPmTabName = strCurrentTabName;
                strPpTabName = diTablabels[(int)TableType.PP]; // eLibTools.GetPrefName(_pref, TableType.PP.GetHashCode());
                strAdrTabName = diTablabels[(int)TableType.ADR];
            }
            else if (_nTargetTab == TableType.ADR.GetHashCode())
            {
                strCurrentTabName = diTablabels[_nTargetTab]; // eLibTools.GetPrefName(_pref, _nTargetTab);
            }



            #region EudoQuery de comptage
            String strSQLCountQ = string.Empty, strSQLCountAllQ = string.Empty, sSQL = string.Empty;

            FilterSel FilterSel = null;
            _pref.Context.Filters.TryGetValue(_nTabFrom, out FilterSel);
            Int32 nFilterId = (FilterSel != null && FilterSel.FilterSelId > 0) ? FilterSel.FilterSelId : 0;
            MarkedFilesSelection mks = null;
            _pref.Context.MarkedFiles.TryGetValue(_nTabFrom, out mks);
            //Fiche marquées sont seulement affichées si : des fiches sont sélectionnées ET que le filtre n'affiché que les fiche marqués est actif ET que l'on a pas sélectionné Toute les fiches
            Boolean bDisplayMarkedFilesOnly = ((mks != null) && (mks.NbFiles > 0) && mks.Enabled);

            //INIT
            EudoQuery.EudoQuery EudoQueryTool = eLibTools.GetEudoQuery(_pref, _nTabFrom, ViewQuery.TREATMENT);
            if (EudoQueryTool.GetError.Length > 0)
                throw (new Exception(String.Concat("EudoQueryTool.init => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO

            try
            {
                if (nFilterId > 0) EudoQueryTool.SetFilterId = nFilterId;

                EudoQueryTool.SetCountTabDescId = _nTargetTab;
                EudoQueryTool.SetDisplayMarkedFile = bDisplayMarkedFilesOnly;
                if (EudoQueryTool.GetError.Length > 0)
                    throw (new Exception(String.Concat("EudoQueryTool.AfterInit => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO

                //CHARGEMENT
                EudoQueryTool.LoadRequest();
                if (EudoQueryTool.GetError.Length > 0)
                {
                    if (EudoQueryTool.GetErrorType != QueryErrorType.ERROR_NUM_FILTER_NOT_AVAILABLE)
                        throw (new Exception(String.Concat("EudoQueryTool.LoadRequest => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO
                    else
                        bFilterAvailable = false;
                }

                if (bFilterAvailable)
                {
                    //GENERATION
                    EudoQueryTool.BuildRequest();
                    if (EudoQueryTool.GetError.Length > 0)
                        throw (new Exception(String.Concat("EudoQueryTool.BuildRequest => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO

                    //REQUETES
                    sSQL = EudoQueryTool.EqQuery;
                    strSQLCountQ = EudoQueryTool.EqCountQuery;
                    strSQLCountAllQ = EudoQueryTool.EqCountAllQuerySecurity;

                    if (EudoQueryTool.GetError.Length > 0)
                        throw (new Exception(String.Concat("EudoQueryTool.REQUETES => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO

                    //Récupération des informations des tables affichées
                    List<EudoQuery.Table> aTableHead = EudoQueryTool.GetTableHeaderList;
                    if (EudoQueryTool.GetError.Length > 0)
                        throw (new Exception(String.Concat("EudoQueryTool.GetTableHeaderList => ", Environment.NewLine, EudoQueryTool.GetError)));//TODO
                }
            }
            finally
            {
                EudoQueryTool.CloseQuery();
                EudoQueryTool = null;
            }
            #endregion

            if (bFilterAvailable)
            {
                try
                {
                    dal.OpenDatabase();
                    _nCount = GetCount(strSQLCountQ, dal, out sError);    //TODO : voir si 'lon peut pas récupéré le nb dans la liste direcment en post
                    if (sError.Length > 0)
                        throw (new Exception(string.Concat("Page_Load.GetCount : ", Environment.NewLine, sError)));
                    _nTotalCount = GetCount(strSQLCountAllQ, dal, out sError);    //TODO : voir si 'lon peut pas récupéré le nb dans la liste direcment en post
                    if (sError.Length > 0)
                        throw (new Exception(string.Concat("Page_Load.GetTotalCount : ", Environment.NewLine, sError)));
                }
                catch (Exception ex)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        String.Concat("Erreur sur eTreatmentDialog - Récup de EudoQuery.TableLite : _nTab = ->", _nTab, " : ", ex.ToString())
                    );
                }
                finally
                {
                    if (dal != null)
                        dal.CloseDatabase();
                }
                LaunchError();

                if (_nTotalCount < 0)
                    _nTotalCount = _nCount;

                String sNbFiles = CryptoTripleDES.Encrypt(String.Concat(_nCount.ToString(), "#", _nTotalCount), CryptographyConst.KEY_CRYPT_LINK1);

                #endregion

                #region Vérification de la connexion
                // Table de la sélection         
                _nUserId = _pref.User.UserId;

                String sDevMsg = String.Empty;
                if (_nTabFrom == 0 || _nUserId == 0)
                {
                    mainDiv.Attributes.Add("tab", _nTab.ToString());

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                        String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                        eResApp.GetRes(_pref, 72),  //   titre
                        String.Concat("Erreur sur eTreatmentDialog - Chargement des paramètres de querystring : _nTab = ->", _nTab)
                    );


                }
                #endregion

                //Lance l'erreur si besoin
                LaunchError();

                #region Génération du contenu de la page

                #region Création des contrôles et rattachements


                HtmlInputHidden inputTrtAttributes = new HtmlInputHidden();
                mainDiv.Controls.Add(inputTrtAttributes);
                inputTrtAttributes.ID = "trtAttributes";
                inputTrtAttributes.Attributes.Add("eTabName", strCurrentTabName);
                inputTrtAttributes.Attributes.Add("eAdrTabName", strAdrTabName);
                inputTrtAttributes.Attributes.Add("ePpTabName", strPpTabName);
                inputTrtAttributes.Attributes.Add("ePmTabName", strPmTabName);
                inputTrtAttributes.Attributes.Add("eNbFiles", sNbFiles);


                inputTrtAttributes.Attributes.Add("eTabFromId", _nTabFrom.ToString());
                inputTrtAttributes.Attributes.Add("eTargetTabId", _nTargetTab.ToString());
                inputTrtAttributes.Attributes.Add("eUserAddress", _pref.User.UserMail);

                inputTrtAttributes.Attributes.Add("eCntAll", _nTotalCount.ToString());
                inputTrtAttributes.Attributes.Add("eCntOnlyselect", _nCount.ToString());



                // UL
                HtmlGenericControl ulSelection = new HtmlGenericControl("ul");
                mainDiv.Controls.Add(ulSelection);
                HtmlGenericControl ulRandomSample = new HtmlGenericControl("ul");
                mainDiv.Controls.Add(ulRandomSample);
                HtmlGenericControl ulActions = new HtmlGenericControl("ul");
                mainDiv.Controls.Add(ulActions);

                ulActions.ID = "ulActions";

                // LI
                HtmlGenericControl liSelection = new HtmlGenericControl("li");
                ulSelection.Controls.Add(liSelection);

                HtmlGenericControl liSelectionOnlySelected = new HtmlGenericControl("li");
                ulSelection.Controls.Add(liSelectionOnlySelected);

                HtmlGenericControl liSelectionAll = new HtmlGenericControl("li");
                ulSelection.Controls.Add(liSelectionAll);

                HtmlGenericControl liRandomSample = new HtmlGenericControl("li");
                ulRandomSample.Controls.Add(liRandomSample);

                HtmlGenericControl liRandomSampleSelect = new HtmlGenericControl("li");
                ulRandomSample.Controls.Add(liRandomSampleSelect);

                HtmlGenericControl liActions = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActions);

                HtmlGenericControl liActionsSubHeader = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsSubHeader);

                HtmlGenericControl liActionsNew = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsNew);

                // Action "Mettre à jour"
                HtmlGenericControl liActionsUpdate = new HtmlGenericControl("li");
                liActionsUpdate.ID = "liActionsUpdate";
                ulActions.Controls.Add(liActionsUpdate);

                //Valeur de mise à jour "réelle"
                HtmlInputHidden htmlValue = new HtmlInputHidden();
                liActionsUpdate.Controls.Add(htmlValue);
                htmlValue.ID = "action_update_withnew_value";
                htmlValue.Disabled = false;

                //Mettre à jour à partir des valeurs de la rubriques
                HtmlGenericControl liActionsUpdateFromExisting = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsUpdateFromExisting);
                liActionsUpdateFromExisting.ID = "liActionsUpdateFromExisting";
                liActionsUpdateFromExisting.Attributes.Add("ednUpdateOption", "1");


                //Metre à jour avec une nouvelle valeur
                HtmlGenericControl liActionsUpdateWithNew = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsUpdateWithNew);
                liActionsUpdateWithNew.ID = "liActionsUpdateWithNew";
                liActionsUpdateWithNew.Attributes.Add("ednUpdateOption", "1");


                //Ecraser les valeurs de la rubrique
                HtmlGenericControl liActionsUpdateWithNewEraseExisting = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsUpdateWithNewEraseExisting);
                liActionsUpdateWithNewEraseExisting.ID = "liActionsUpdateWithNewEraseExisting";
                liActionsUpdateWithNewEraseExisting.Attributes.Add("ednUpdateOption", "1");




                HtmlGenericControl liActionsDelete = new HtmlGenericControl("li");
                ulActions.Controls.Add(liActionsDelete);
                HtmlGenericControl liActionsDuplicate = new HtmlGenericControl("li");
                // 01/09/2015 : Masque de la fonction "Dupliquer la sélection" car fonctionnalité incomplète
                ulActions.Controls.Add(liActionsDuplicate);
                // Contrôles de formulaire (options)
                HtmlInputRadioButton rbSelectionOnlySelected = new HtmlInputRadioButton();
                liSelectionOnlySelected.Controls.Add(rbSelectionOnlySelected);
                HtmlInputRadioButton rbSelectionAll = new HtmlInputRadioButton();
                liSelectionAll.Controls.Add(rbSelectionAll);
                eCheckBoxCtrl chkRandomSampleSelect = new eCheckBoxCtrl(false, false);
                liRandomSampleSelect.Controls.Add(chkRandomSampleSelect);


                // Radios Bouttons d'Actions
                //Ajouter une nouvelle viche
                HtmlInputRadioButton rbActionsNew = new HtmlInputRadioButton();
                liActionsNew.Controls.Add(rbActionsNew);


                //
                HtmlInputRadioButton rbActionsUpdate = new HtmlInputRadioButton();
                liActionsUpdate.Controls.Add(rbActionsUpdate);
                HtmlInputRadioButton rbActionsUpdateFromExisting = new HtmlInputRadioButton();
                liActionsUpdateFromExisting.Controls.Add(rbActionsUpdateFromExisting);

                HtmlInputRadioButton rbActionsUpdateWithNew = new HtmlInputRadioButton();
                liActionsUpdateWithNew.Controls.Add(rbActionsUpdateWithNew);




                HtmlInputRadioButton rbActionsDelete = new HtmlInputRadioButton();
                liActionsDelete.Controls.Add(rbActionsDelete);
                HtmlInputRadioButton rbActionsDuplicate = new HtmlInputRadioButton();
                liActionsDuplicate.Controls.Add(rbActionsDuplicate);

                // Labels
                HtmlGenericControl labelSelection = new HtmlGenericControl("label");
                liSelection.Controls.Add(labelSelection);

                HtmlGenericControl labelSelectionOnlySelected = new HtmlGenericControl("label");
                liSelectionOnlySelected.Controls.Add(labelSelectionOnlySelected);

                HtmlGenericControl labelSelectionAll = new HtmlGenericControl("label");
                liSelectionAll.Controls.Add(labelSelectionAll);

                HtmlGenericControl labelRandomSample = new HtmlGenericControl("label");
                liRandomSample.Controls.Add(labelRandomSample);

                HtmlGenericControl labelActions = new HtmlGenericControl("label");
                liActions.Controls.Add(labelActions);

                HtmlGenericControl labelActionsSubHeader = new HtmlGenericControl("label");
                liActionsSubHeader.Controls.Add(labelActionsSubHeader);

                HtmlGenericControl labelActionsNew = new HtmlGenericControl("label");
                liActionsNew.Controls.Add(labelActionsNew);


                HtmlGenericControl labelActionsUpdate = new HtmlGenericControl("label");
                liActionsUpdate.Controls.Add(labelActionsUpdate);


                HtmlGenericControl labelActionsUpdateFromExisting = new HtmlGenericControl("label");
                liActionsUpdateFromExisting.Controls.Add(labelActionsUpdateFromExisting);


                HtmlGenericControl labelActionsUpdateWithNew = new HtmlGenericControl("label");
                liActionsUpdateWithNew.Controls.Add(labelActionsUpdateWithNew);


                HtmlGenericControl labelActionsUpdateWithNewEraseExisting = new HtmlGenericControl("label");
                liActionsUpdateWithNewEraseExisting.Controls.Add(labelActionsUpdateWithNewEraseExisting);





                HtmlGenericControl labelActionsDelete = new HtmlGenericControl("label");
                liActionsDelete.Controls.Add(labelActionsDelete);

                HtmlGenericControl labelActionsDuplicate = new HtmlGenericControl("label");
                liActionsDuplicate.Controls.Add(labelActionsDuplicate);

                // Contrôles de formulaire (valeurs)
                HtmlInputText inputRandomSampleSelectCount = new HtmlInputText();
                liRandomSampleSelect.Controls.Add(inputRandomSampleSelectCount);
                HtmlGenericControl labelRandomSampleSelect2 = new HtmlGenericControl("label");
                liRandomSampleSelect.Controls.Add(labelRandomSampleSelect2); // fiches
                HtmlSelect selectActionsNew = new HtmlSelect();
                liActionsNew.Controls.Add(selectActionsNew);

                HtmlSelect selectActionsUpdate = new HtmlSelect();
                liActionsUpdate.Controls.Add(selectActionsUpdate);

                HtmlSelect selectActionsUpdateFromExisting = new HtmlSelect();
                liActionsUpdateFromExisting.Controls.Add(selectActionsUpdateFromExisting);
                HtmlInputText inputActionsUpdateWithNew = new HtmlInputText();
                liActionsUpdateWithNew.Controls.Add(inputActionsUpdateWithNew);

                #endregion

                #region Paramétrage des contrôles de formulaire (select/input/...)
                // IDs et noms
                rbSelectionOnlySelected.ID = "selection_onlyselect"; rbSelectionOnlySelected.Name = "selection";
                rbSelectionAll.ID = "selection_all"; rbSelectionAll.Name = "selection";
                chkRandomSampleSelect.ID = "random_select";
                inputRandomSampleSelectCount.ID = "random_select_count"; inputRandomSampleSelectCount.Name = inputRandomSampleSelectCount.ID;
                rbActionsNew.ID = "action_new"; rbActionsNew.Name = "action";
                rbActionsUpdate.ID = "action_update"; rbActionsUpdate.Name = "action";
                rbActionsUpdateFromExisting.ID = "action_update_fromexisting"; rbActionsUpdateFromExisting.Name = "action_update";

                rbActionsUpdateWithNew.ID = "action_update_withnew"; rbActionsUpdateWithNew.Name = "action_update";
                rbActionsDelete.ID = "action_delete"; rbActionsDelete.Name = "action";
                rbActionsDuplicate.ID = "action_duplicate"; rbActionsDuplicate.Name = "action";


                //Affectation d'une nouvelle fiche
                selectActionsNew.ID = "action_new_value";
                selectActionsNew.Name = selectActionsNew.ID;
                selectActionsNew.Attributes.Add("class", "selectActionsNew");




                selectActionsUpdate.ID = "action_update_value";
                selectActionsUpdate.Name = selectActionsUpdate.ID;
                selectActionsUpdate.Attributes.Add("onchange", "onChangeUpdatedField(this.options[this.selectedIndex].value)");
                selectActionsUpdate.Attributes.Add("class", "selectActionsUpdate");


                selectActionsUpdateFromExisting.ID = "action_update_fromexisting_value";
                selectActionsUpdateFromExisting.Name = selectActionsUpdateFromExisting.ID;
                selectActionsUpdate.Attributes.Add("class", "selectActionsUpdateFromExisting");



                inputActionsUpdateWithNew.ID = "action_update_withnew_value";
                inputActionsUpdateWithNew.Name = inputActionsUpdateWithNew.ID;

                // Valeurs
                rbSelectionOnlySelected.Attributes.Add("value", "onlyselected");
                rbSelectionAll.Attributes.Add("value", "all");
                rbActionsNew.Attributes.Add("value", "new");
                rbActionsUpdate.Attributes.Add("value", "update");
                rbActionsUpdate.Checked = true;
                rbActionsUpdateWithNew.Checked = true;
                rbActionsUpdateFromExisting.Attributes.Add("value", "fromexisting");

                rbActionsUpdateWithNew.Attributes.Add("value", "withnew");
                rbActionsDelete.Attributes.Add("value", "delete");
                rbActionsDuplicate.Attributes.Add("value", "duplicate");


                rbSelectionAll.Attributes.Add("NbFiles", _nTotalCount.ToString());
                rbSelectionOnlySelected.Attributes.Add("NbFiles", _nCount.ToString());
                // Elements cochés et grisés
                // Le statut des cases à cocher (eCheckBoxCtrl) est géré à l'instanciation (cf. ci-dessus)
                if ((_nCount == _nTotalCount) || (_nCount == 0))
                {
                    rbSelectionOnlySelected.Disabled = true;
                    rbSelectionAll.Checked = true;
                }
                else
                    rbSelectionOnlySelected.Checked = true;

                rbActionsUpdateFromExisting.Disabled = true;
                rbActionsUpdateFromExisting.Checked = true;
                selectActionsUpdateFromExisting.Disabled = true;
                rbActionsUpdateWithNew.Disabled = true;
                inputActionsUpdateWithNew.Disabled = true;
                inputRandomSampleSelectCount.Disabled = true;
                selectActionsUpdate.Disabled = true;


                // Interactions JavaScript
                chkRandomSampleSelect.AddClick("onChange(this.id);"); // CheckBox Sélection aléatoire


                /*  Type d'action */
                rbActionsNew.Attributes.Add("onchange", "onChange(this.id);");
                rbActionsUpdate.Attributes.Add("onchange", "onChange(this.id);");
                rbActionsDelete.Attributes.Add("onchange", "onChange(this.id);");
                rbActionsDuplicate.Attributes.Add("onchange", "onChange(this.id);");


                /*  Sous type d'action pour action  Mise à Jour */
                rbActionsUpdateFromExisting.Attributes.Add("onchange", "onChange(this.id);");
                rbActionsUpdateWithNew.Attributes.Add("onchange", "onChange(this.id);");




                #endregion

                #region Pas d'affectation d'une nouvelle fiche, si on ne vient pas depuis un EVT, PP et PM


                if (sError.Length > 0)
                {
                    #region Feedback
                    // Table de la sélection         
                    Int32 nUserId = _pref.User.UserId;


                    if (_nTabFrom == 0 || _nUserId == 0)
                    {
                        mainDiv.Attributes.Add("tab", _nTab.ToString());

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref, 72),  //   titre
                            String.Concat("Erreur sur eTreatmentDialog - _nTab = ->", _nTab, Environment.NewLine, "Error:", sError)
                        );
                    }
                    //Lance l'erreur si besoin
                    LaunchError();
                    #endregion

                }

                if (tabTarget.EdnType == EdnType.FILE_MAIN
                    || _nTargetTab == (int)TableType.PP
                    || _nTargetTab == (int)TableType.PM
                    || _nTargetTab == (int)TableType.ADR)
                {

                    selectActionsNew.Disabled = false;
                    rbActionsNew.Disabled = false;

                    rbActionsNew.Checked = true;
                    selectActionsUpdate.Disabled = true;

                    rbActionsUpdate.Checked = false;




                }
                else
                {
                    selectActionsNew.Disabled = true;
                    rbActionsNew.Disabled = true;
                    rbActionsNew.Checked = false;

                    selectActionsUpdate.Disabled = false;
                    rbActionsUpdate.Checked = true;

                    //Si desactivé on l affiche pas ?
                    // liActionsNew.Style.Add(HtmlTextWriterStyle.Display, "none");
                }


                #endregion

                #region Paramétrage des contrôles de structure (ul/li/label)


                // CSS
                string strClassHeader = "trt_Header";
                string strClassSubHeader = "trt_SubHeader";
                string strClassOpt1 = "trt_Opt1";
                string strClassOpt2 = "trt_Opt2";
                string strClassOpt3 = "trt_Opt3";


                liSelection.Attributes.Add("class", strClassHeader);
                liSelectionOnlySelected.Attributes.Add("class", strClassOpt1);
                liSelectionAll.Attributes.Add("class", strClassOpt1);
                liRandomSample.Attributes.Add("class", strClassHeader);
                liRandomSampleSelect.Attributes.Add("class", strClassOpt1);
                liActions.Attributes.Add("class", strClassHeader);
                liActionsSubHeader.Attributes.Add("class", strClassSubHeader);
                liActionsNew.Attributes.Add("class", strClassOpt1);
                liActionsUpdate.Attributes.Add("class", strClassOpt1);
                liActionsUpdateFromExisting.Attributes.Add("class", strClassOpt2);





                liActionsUpdateWithNew.Attributes.Add("class", strClassOpt2);
                liActionsUpdateWithNewEraseExisting.Attributes.Add("class", strClassOpt3);

                liActionsDelete.Attributes.Add("class", strClassOpt1);
                liActionsDuplicate.Attributes.Add("class", strClassOpt1);
                inputRandomSampleSelectCount.Attributes.Add("class", "trt_RndCnt");
                inputActionsUpdateWithNew.Attributes.Add("class", "trt_UpdWithNew");

                // Rattachements libellé/contrôle
                labelSelection.Attributes.Add("for", liSelection.ID);
                labelSelectionOnlySelected.Attributes.Add("for", rbSelectionOnlySelected.ID);
                labelSelectionAll.Attributes.Add("for", rbSelectionAll.ID);
                labelRandomSample.Attributes.Add("for", chkRandomSampleSelect.ID);
                labelRandomSampleSelect2.Attributes.Add("for", inputRandomSampleSelectCount.ID);
                labelActions.Attributes.Add("for", liActions.ID);
                labelActionsSubHeader.Attributes.Add("for", liActionsSubHeader.ID);
                labelActionsNew.Attributes.Add("for", rbActionsNew.ID);
                labelActionsUpdate.Attributes.Add("for", rbActionsUpdate.ID);
                labelActionsUpdateFromExisting.Attributes.Add("for", rbActionsUpdateFromExisting.ID);

                labelActionsUpdateWithNew.Attributes.Add("for", rbActionsUpdateWithNew.ID);
                labelActionsDelete.Attributes.Add("for", rbActionsDelete.ID);
                labelActionsDuplicate.Attributes.Add("for", rbActionsDuplicate.ID);
                // Textes des libellés
                labelSelection.InnerHtml = GetRes(187);
                labelSelectionOnlySelected.InnerHtml = HttpUtility.HtmlEncode(String.Concat(GetRes(6388).Replace("<PREF_NAME>", String.Concat("\"", strCurrentTabName, "\"")), " (", _nCount, " ", GetRes(300), ")")); // Uniquement les fiches <X> sélectionnées (X fiches)

                labelSelectionAll.InnerHtml = HttpUtility.HtmlEncode(String.Concat(GetRes(530), " \"", strCurrentTabName, "\" (", _nTotalCount, " ", GetRes(300), ")")); // Toutes les fiches <X> (x fiches)
                labelRandomSample.InnerHtml = HttpUtility.HtmlEncode(GetRes(6371)); // Echantillon aléatoire


                chkRandomSampleSelect.AddText(GetRes(1413)); // Obtenir un échantillon aléatoire de

                labelRandomSampleSelect2.InnerHtml = HttpUtility.HtmlEncode(GetRes(300)); // fiches

                labelActions.InnerHtml = GetRes(296); // Actions
                labelActionsSubHeader.InnerHtml = HttpUtility.HtmlEncode(GetRes(6372).Replace("<PREF_NAME>", strCurrentTabName)); // Pour chaque fiche <X> :
                labelActionsNew.InnerHtml = HttpUtility.HtmlEncode(GetRes(297)); // Affecter une nouvelle fiche
                labelActionsUpdate.InnerHtml = HttpUtility.HtmlEncode(GetRes(303)); // Mettre à jour la rubrique
                labelActionsUpdateFromExisting.InnerHtml = HttpUtility.HtmlEncode(GetRes(302)); // A partir des valeurs de la rubrique
                labelActionsUpdateWithNew.InnerHtml = HttpUtility.HtmlEncode(GetRes(301)); // Avec une nouvelle valeur
                labelActionsUpdateWithNewEraseExisting.InnerHtml = HttpUtility.HtmlEncode(GetRes(643)); // Ecraser les valeurs existantes de la rubrique



                labelActionsDelete.InnerHtml = HttpUtility.HtmlEncode(GetRes(834)); // Supprimer la sélection
                labelActionsDuplicate.InnerHtml = HttpUtility.HtmlEncode(GetRes(876)); // Dupliquer la sélection
                #endregion

                #region vérification des droits pour la suppression et la duplication

                try
                {
                    dal.OpenDatabase();

                    ProcessRights[] rights = new ProcessRights[]
                    {
                        ProcessRights.PRC_RIGHT_DELETE,
                        ProcessRights.PRC_RIGHT_DELETE_MULTIPLE,
                        ProcessRights.PRC_RIGHT_ADD,
                        ProcessRights.PRC_RIGHT_CLONE,
                        ProcessRights.PRC_RIGHT_CLONE_MULTIPLE
                    };

                    IDictionary<ProcessRights, bool> dico = eLibDataTools.GetTreatmentTabRight(dal, _pref.User, _nTabFrom, rights);

                    //droit de suppression & droit de suppression en masse
                    bool bDeleteAllowed = dico[ProcessRights.PRC_RIGHT_DELETE] && dico[ProcessRights.PRC_RIGHT_DELETE_MULTIPLE];

                    rbActionsDelete.Disabled = !bDeleteAllowed;

                    // droit d'ajout & droit de duplication & droit de duplication en masse
                    bool bDuplicateAllowed = dico[ProcessRights.PRC_RIGHT_ADD] && dico[ProcessRights.PRC_RIGHT_CLONE] && dico[ProcessRights.PRC_RIGHT_CLONE_MULTIPLE];

                    rbActionsDuplicate.Disabled = !bDuplicateAllowed;
                }
                catch (Exception)
                {
                    // Je conserve la non gestion d'erreur présente avant la refacto
                }
                finally
                {
                    dal?.CloseDatabase();
                }

                #endregion

                #region Remplissage des valeurs
                //Remplissage par des liaisons bkm
                if (tabTarget.EdnType == EdnType.FILE_MAIN
                    || _nTargetTab == (int)TableType.PP
                    || _nTargetTab == (int)TableType.PM
                    || _nTargetTab == (int)TableType.ADR
                    )
                {
                    this.FillFromLinkedBkm(selectActionsNew);
                }



                // ASY (Demande 25 081) : [Traitement de masse] - Liste des champs affichés - Cette liste n'est pas assez filtrée actuellement et fait apparaître des champs systèmes ou internes.
                IDictionary<Int32, String> dicFieldAllowed = eLibTools.GetListFieldsAllowedGlobalAffect(_pref, _nTargetTab, true);
                foreach (KeyValuePair<Int32, String> descIdLabels in dicFieldAllowed)
                {
                    ListItem item = new ListItem(descIdLabels.Value, descIdLabels.Key.ToString());
                    selectActionsUpdate.Items.Add(item);
                }
                selectActionsUpdate.SelectedIndex = -1;

                #endregion

                #region Désactivations de composant
                if ((tabFrom.EdnType == EdnType.FILE_MAIL)
                    || (tabFrom.EdnType == EdnType.FILE_SMS)
                    || (tabFrom.EdnType == EdnType.FILE_HISTO)
                    || (tabFrom.EdnType == EdnType.FILE_VOICING)
                    )
                    rbActionsDuplicate.Disabled = true; //Pas de duplication sur EMAIL, VOICING ET HISTO !

                if (selectActionsNew.Items.Count <= 0)  //Si pas de signet à affecter on désactive cette option
                {
                    selectActionsNew.Disabled = true;
                    rbActionsNew.Disabled = true;
                    rbActionsNew.Checked = false;

                    rbActionsUpdate.Checked = true;
                }

                if (selectActionsUpdate.Items.Count <= 0)
                {
                    selectActionsUpdate.Disabled = true;
                    rbActionsUpdate.Checked = false;
                    rbActionsUpdate.Disabled = true;

                    rbActionsUpdateFromExisting.Disabled = true;
                    rbActionsUpdateFromExisting.Checked = false;

                    rbActionsUpdateWithNew.Disabled = true;
                    rbActionsUpdateWithNew.Checked = false;
                }


                if (selectActionsUpdateFromExisting.Items.Count <= 0)
                {
                    selectActionsUpdateFromExisting.Disabled = true;

                    rbActionsUpdateFromExisting.Disabled = true;
                    rbActionsUpdateFromExisting.Checked = false;

                    if (rbActionsUpdate.Checked)
                        rbActionsUpdateWithNew.Checked = true;
                }
                #endregion

                #region Declaration des variables JavaScript (interaction boutons radio/valeurs)

                _generatedJavaScript
                    .Append("var nGlobalActiveTab = ").Append(_nTab).Append(";").AppendLine()
                    .Append("var inputRandomSampleSelectCount = '").Append(inputRandomSampleSelectCount.ID).Append("';").AppendLine()
                    .Append("var selectActionsNew = '").Append(selectActionsNew.ID).Append("';").AppendLine()
                    .Append("var selectActionsUpdate = '").Append(selectActionsUpdate.ID).Append("';").AppendLine()
                    .Append("var chkRandomSampleSelect = '").Append(chkRandomSampleSelect.ID).Append("';").AppendLine()
                    .Append("var rbActionsNew = '").Append(rbActionsNew.ID).Append("';").AppendLine()
                    .Append("var rbActionsUpdateFromExisting = '").Append(rbActionsUpdateFromExisting.ID).Append("';").AppendLine()
                    .Append("var rbActionsUpdateWithNew = '").Append(rbActionsUpdateWithNew.ID).Append("';").AppendLine()
                    .Append("var selectActionsUpdateFromExisting = '").Append(selectActionsUpdateFromExisting.ID).Append("';").AppendLine()
                    .Append("var inputActionsUpdateWithNew = '").Append(inputActionsUpdateWithNew.ID).Append("';").AppendLine()
                    .Append("var rbActionsDuplicate = '").Append(rbActionsDuplicate.ID).Append("';").AppendLine()
                    .Append("var rbActionsDelete = '").Append(rbActionsDelete.ID).Append("';").AppendLine()
                    .Append("var rbActionsUpdate = '").Append(rbActionsUpdate.ID).Append("';").AppendLine();

                #endregion

                #endregion
            }
            else
            {
                // #35140 : Si le filtre n'existe pas, on propose à l'utilisateur d'annuler le filtre
                message.InnerHtml = String.Concat(eResApp.GetRes(_pref, 815), ". <a href='#' onclick=\"cancelAdvFlt(", _nTab, ");\">", eResApp.GetRes(_pref, 1179), "</a>.");
            }

        }


        private void FillFromLinkedBkm(HtmlSelect select)
        {


            List<eModelTools.FileStruct> files = eDataTools.GetLinkedBkm(_pref, _nTargetTab);
            foreach (eModelTools.FileStruct file in files)
            {
                ListItem item = new ListItem(file.sFileName, file.nFileTab.ToString());
                item.Attributes.Add("edntype", file.eEdnType);

                if (_pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                    item.Attributes.Add("title", file.eFileType);

                select.Items.Add(item);
            }

        }

        /// <summary>
        /// Comptage total actif (visible pour l'utilisateur)
        /// </summary>
        /// <param name="sqlCount">Requete sql pour le comptage</param>
        /// <param name="dal">Objet connexion</param>
        /// <param name="sError">Erreur s'étant produite si besoin</param>
        /// <returns>nombre de fiche</returns>
        private Int32 GetCount(string sqlCount, eudoDAL dal, out string sError)
        {
            return eDataTools.GetCountByQuery(sqlCount, dal, out sError);
        }
        /// <summary>
        /// récupère une ressource donnée
        /// </summary>
        /// <returns></returns>
        public string GetRes(int resId)
        {
            return eResApp.GetRes(_pref, resId);
        }



    }
}