using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAdvancedCatalogDialogRenderer : eAdminRenderer
    {
        #region propriétés
        private Int32 _nTab;
        private eAdminTableInfos _tabInfos;
        private String _tabName;

        private Int32 _nField;
        private eAdminFieldInfos _fieldInfos;
        private String _fieldName;

        private eAdminFiledataParam _fileDataParam;

        private string strUserLangRes;
        private string strTextRes;
        private string strDataRes;
        private const string strTextField = "[TEXT]";
        private const string strDataField = "[DATA]";

        private List<ListItem> _listMaskOptions;

        private Dictionary<int, string> _dicoPopupFulltext;
        private Dictionary<int, short> _dicoPopupType;

        private Dictionary<int, string> _dicoBoundFulltext;
        private Dictionary<int, bool> _dicoBoundTreeview;

        private eudoDAL _dal;

        private Boolean _adminAllowed = true;

        private OrmMappingInfo _ormInfos = new OrmMappingInfo();

        private bool _isProductUpdateLocked = false;
        #endregion


        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        public eAdminAdvancedCatalogDialogRenderer(ePref pref, int nTab, int nField)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;

            _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(pref, nField);
            _nField = nField;
            _fieldName = _fieldInfos.Labels[pref.LangId];


        }

        public static eAdminAdvancedCatalogDialogRenderer CreateAdminAdvancedCatalogDialogRenderer(ePref pref, int nTab, int nField)
        {
            return new eAdminAdvancedCatalogDialogRenderer(pref, nTab, nField);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (base.Init())
            {
                strUserLangRes = eResApp.GetRes(Pref, 978);
                strTextRes = eResApp.GetRes(Pref, 223);
                strDataRes = eResApp.GetRes(Pref, 973);

                _fileDataParam = _fieldInfos.GetFileDataParam(Pref);

                _listMaskOptions = GetMaskOptions();

                _adminAllowed = _fieldInfos.IsUserAllowedToUpdate();
                 _ormInfos = eLibTools.OrmLoadAndGetMapWeb(_ePref);
                string sError;
                _dal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    _dal.OpenDatabase();

                    if (!eSqlCatalog.GetCatalogForPopup(Pref, _dal, _fieldInfos, out _dicoPopupFulltext, out _dicoPopupType, out sError))
                        throw new Exception(sError);

                    if (!eSqlCatalog.GetCatalogForBound(Pref, _dal, _fieldInfos, _tabInfos, out _dicoBoundFulltext, out _dicoBoundTreeview, out sError))
                        throw new Exception(sError);



                    if (_fileDataParam.UpdatePermission > 0 && Pref.User.UserLevel < (int)UserLevel.LEV_USR_PRODUCT)
                    {
                        //vérification des perm
                        ePermission eUpdate = new ePermission(_fileDataParam.UpdatePermission, _dal, Pref);
                        if (eUpdate.PermLevel >= (int)UserLevel.LEV_USR_PRODUCT)
                            _isProductUpdateLocked = true;
                    }


                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    _dal.CloseDatabase();
                }

                return true;
            }

            return false;
        }

        protected override bool Build()
        {
            _pgContainer.ID = "advancedCatalogAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            #region valeurs cachées
            HtmlGenericControl inputFieldId = new HtmlGenericControl("input");
            inputFieldId.Attributes.Add("type", "hidden");
            inputFieldId.Attributes.Add("id", "edaFieldId");
            inputFieldId.Attributes.Add("name", "edaFieldId");
            inputFieldId.Attributes.Add("value", _fieldInfos.DescId.ToString());
            _pgContainer.Controls.Add(inputFieldId);

            HtmlGenericControl inputIsSuperAdmin = new HtmlGenericControl("input");
            inputIsSuperAdmin.Attributes.Add("type", "hidden");
            inputIsSuperAdmin.Attributes.Add("id", "edaIsSuperAdmin");
            inputIsSuperAdmin.Attributes.Add("name", "edaIsSuperAdmin");
            inputIsSuperAdmin.Attributes.Add("value", Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN ? "1" : "0");
            _pgContainer.Controls.Add(inputIsSuperAdmin);
            #endregion

            #region Etape 1 : Mode d’affichage et tri des valeurs du catalogue
            GenerateStep1();
            #endregion

            #region Etape 2 : Droits sur les valeurs du catalogue
            GenerateStep2();
            #endregion

            #region Etape 3 : Traduction des valeurs du catalogue
            GenerateStep3();
            #endregion

            #region Etape 4 : Options avancées du catalogue
            GenerateStep4();
            #endregion

            return base.Build();
        }



        private List<ListItem> GetMaskOptions()
        {

            // TODO : à changer : ces valeurs sont insérée telquelle dans sql, ce qui ouvre la porte à de potentiel injection sql

            List<ListItem> listItems = new List<ListItem>();
            listItems.Add(new ListItem(String.Concat(strTextRes, " (", strUserLangRes, ")"), strTextField)); //Libellé (selon la langue de l'utilisateur)
            listItems.Add(new ListItem(String.Concat(strTextRes, " ", strDataRes), String.Concat(strTextField, "+' '+", strDataField))); //Libellé Code
            listItems.Add(new ListItem(String.Concat(strTextRes, " (", strDataRes, ")"), String.Concat(strTextField, "+' ('+", strDataField, "+')'"))); //Libellé (Code)
            listItems.Add(new ListItem(String.Concat(strTextRes, " - ", strDataRes), String.Concat(strTextField, "+' - '+", strDataField))); //Libellé – Code
            listItems.Add(new ListItem(strDataRes, strDataField)); //Code
            listItems.Add(new ListItem(String.Concat(strDataRes, " ", strTextRes), String.Concat(strDataField, "+' '+", strTextField))); //Code Libellé
            listItems.Add(new ListItem(String.Concat(strDataRes, " (", strTextRes, ")"), String.Concat(strDataField, "+' ('+", strTextField, "+')'"))); //Code (Libellé)
            listItems.Add(new ListItem(String.Concat(strDataRes, " - ", strTextRes), String.Concat(strDataField, "+' - '+", strTextField))); //Code – Libellé

            return listItems;
        }

        private HtmlGenericControl GenerateStep(String sNum, String title, Boolean active)
        {
            Panel panelStep;

            panelStep = new Panel();
            panelStep.CssClass = "divStep";

            panelStep.Controls.Add(GenerateStepTitle(sNum, title, active));

            HtmlGenericControl stepContent = GenerateStepContent(active);
            panelStep.Controls.Add(stepContent);

            _pgContainer.Controls.Add(panelStep);

            return stepContent;
        }

        /// <summary>
        /// Génère la partie titre de l'étape
        /// </summary>
        /// <param name="sNum"></param>
        /// <param name="title"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private HtmlGenericControl GenerateStepTitle(String sNum, String title, Boolean active)
        {
            String classActive = (active) ? " active" : String.Empty;

            HtmlGenericControl step = new HtmlGenericControl("div");
            step.ID = String.Concat("stepTitle", sNum);
            step.Attributes.Add("class", "paramStep" + classActive);

            HtmlGenericControl span = new HtmlGenericControl();
            span.InnerText = sNum;
            span.Attributes.Add("class", "stepNum");
            step.Controls.Add(span);
            span = new HtmlGenericControl();
            span.InnerText = title;
            span.Attributes.Add("class", "stepTitle");
            step.Controls.Add(span);

            return step;
        }

        /// <summary>
        /// Génère la partie content de l'étape
        /// </summary>
        /// <param name="active"></param>
        /// <returns></returns>
        private HtmlGenericControl GenerateStepContent(Boolean active)
        {
            HtmlGenericControl step = new HtmlGenericControl("div");
            step.Attributes.Add("class", "stepContent");
            if (active)
                step.Attributes.Add("data-active", "1");
            else
                step.Attributes.Add("data-active", "0");

            return step;
        }

        private void GenerateStep1()
        {
            HtmlGenericControl stepContent;
            Panel divField;
            HtmlGenericControl spanlabel;

            stepContent = GenerateStep("1", eResApp.GetRes(Pref, 7320), true); //"Mode d’affichage et tri des valeurs du catalogue"

            //Affichage d'un code (filedataparam.dataEnabled)
            divField = new Panel();
            divField.Attributes.Add("class", "field");
            stepContent.Controls.Add(divField);

            Dictionary<String, String> listItems = new Dictionary<String, String>();
            listItems.Add("1", eResApp.GetRes(Pref, 58));
            listItems.Add("0", eResApp.GetRes(Pref, 59));
            eAdminField field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7321), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblDataEnabled", listItems,
                value: _fileDataParam.DataEnabled ? "1" : "0");
            field.ReadOnly = !_adminAllowed || (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN && !String.IsNullOrEmpty(_fileDataParam.DataAutoFormula));
            field.IsLabelBefore = true;
            field.Generate(divField);

            //Format d'affichage (filedataparam.DisplayMask)
            divField = new Panel();
            divField.Attributes.Add("class", "field");
            stepContent.Controls.Add(divField);

            spanlabel = new HtmlGenericControl("span");
            spanlabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7322), " :"); //"Sélectionner le mode d'affichage des valeurs"
            divField.Controls.Add(spanlabel);

            bool displayMaskMatch = false;
            HtmlGenericControl dllDisplayMask = new HtmlGenericControl("select");
            dllDisplayMask.Attributes.Add("id", "ddlDisplayMask");
            dllDisplayMask.Attributes.Add("name", "ddlDisplayMask");
            divField.Controls.Add(dllDisplayMask);
            foreach (ListItem item in _listMaskOptions)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", item.Value);
                option.InnerText = item.Text;

                if (_fileDataParam.DisplayMask == item.Value && !displayMaskMatch)
                {
                    option.Attributes.Add("selected", "selected");
                    displayMaskMatch = true;
                }

                dllDisplayMask.Controls.Add(option);
            }

            if (!displayMaskMatch)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", _fileDataParam.DisplayMask);
                option.InnerText = _fileDataParam.DisplayMask.Replace(strTextField, strTextRes).Replace(strDataField, strDataRes);
                option.Attributes.Add("selected", "selected");
                dllDisplayMask.Controls.Add(option);
            }


            //Format de tri (filedataparam.SortBy)
            spanlabel = new HtmlGenericControl("span");
            spanlabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7323), " :"); //"et le tri par défaut"
            divField.Controls.Add(spanlabel);

            bool sortByMatch = false;
            HtmlGenericControl dllSortBy = new HtmlGenericControl("select");
            dllSortBy.Attributes.Add("id", "ddlSortBy");
            dllSortBy.Attributes.Add("name", "ddlSortBy");
            divField.Controls.Add(dllSortBy);
            foreach (ListItem item in _listMaskOptions)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", item.Value);
                option.InnerText = item.Text;

                if (_fileDataParam.SortBy == item.Value && !sortByMatch)
                {
                    option.Attributes.Add("selected", "selected");
                    sortByMatch = true;
                }

                dllSortBy.Controls.Add(option);
            }

            if (!sortByMatch)
            {
                HtmlGenericControl option = new HtmlGenericControl("option");
                option.Attributes.Add("value", _fileDataParam.SortBy);
                option.InnerText = _fileDataParam.SortBy.Replace(strTextField, strTextRes).Replace(strDataField, strDataRes);
                option.Attributes.Add("selected", "selected");
                dllSortBy.Controls.Add(option);
            }
        }

        private void GenerateStep2()
        {
            HtmlGenericControl stepContent = GenerateStep("2", eResApp.GetRes(Pref, 7324), false); //"Indiquer les droits d'ajout, de modification et de suppresion pour chaque utilisateur, groupe et niveau"

            Panel divField = new Panel();
            divField.Attributes.Add("class", "field linkRights");
            stepContent.Controls.Add(divField);

            if (CanManageCatalogValueRight())
            {
                string types = ((int)eTreatmentType.CATALOG).ToString();
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7325), "", onclick: String.Concat("nsAdmin.confRights(", _fieldInfos.DescId, ", null, '", types, "')")); //"Administrer les droits sur les valeurs"
                button.Generate(divField);
            }
            else
            {
                HtmlGenericControl spanlabel = new HtmlGenericControl("span");
                spanlabel.InnerText = eResApp.GetRes(Pref, 2396);
                divField.Controls.Add(spanlabel);
            }
        }

        private bool CanManageCatalogValueRight()
        {
            if (_fieldInfos.DescId == (int)InteractionField.ConsentObtainedBy)
                return false;

            return true;
        }

        private void GenerateStep3()
        {
            HtmlGenericControl stepContent = GenerateStep("3", eResApp.GetRes(Pref, 7326), false); //"Traduire les valeurs du catalogue"
            Panel divField = new Panel();
            divField.Attributes.Add("class", "field linkRights");
            stepContent.Controls.Add(divField);




            //Traductions
            if (!_isProductUpdateLocked)
            {
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7716), "",
                    onclick: String.Format("nsAdmin.openTranslations({0}, {1});", _fieldInfos.DescId, (int)eAdminTranslation.NATURE.Catalog)

                    );
                button.Generate(divField);
            }
            else
            {
                
                divField = new Panel();
                divField.Attributes.Add("class", "field");
                stepContent.Controls.Add(divField);

                HtmlGenericControl spanlabel = new HtmlGenericControl("span");
                spanlabel.InnerText = String.Concat(eResApp.GetRes(Pref, 6434)); //Vous n'avez pas les droits suffisants pour modifier une valeur de ce catalogue
                divField.Controls.Add(spanlabel);
            }
        }

        private void GenerateStep4()
        {
            HtmlGenericControl stepContent;
            Panel divContainer;
            Panel divField;
            HtmlGenericControl spanlabel;

            stepContent = GenerateStep("4", eResApp.GetRes(Pref, 7327), false); //"Définir les options avancées du catalogue"


            //Type unitaire/multiple (desc.length + desc.multiple)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            Dictionary<String, String> listItems = new Dictionary<String, String>();
            listItems.Add("0", eResApp.GetRes(Pref, 7329));
            listItems.Add("1", eResApp.GetRes(Pref, 247));
            eAdminField field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7328), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblMultiple", listItems,
                value: _fieldInfos.Multiple ? "1" : "0");
            field.ReadOnly = !_adminAllowed || _ormInfos.GetAllMappedDescid.Contains(_fieldInfos.DescId);
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            //Format liste/arborescent (filedataparam.Treeview)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            listItems = new Dictionary<String, String>();
            listItems.Add("0", eResApp.GetRes(Pref, 179));
            listItems.Add("1", eResApp.GetRes(Pref, 7331));
            field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7330), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblTreeView", listItems,
                value: _fileDataParam.TreeView ? "1" : "0");
            field.ReadOnly = !_adminAllowed;
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            #region Format étape
            divContainer = new Panel();
            divContainer.ID = "stepModeParameter";
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            listItems = new Dictionary<String, String>();
            listItems.Add("0", eResApp.GetRes(Pref, 59));
            listItems.Add("1", eResApp.GetRes(Pref, 58));
            field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7976), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblStepMode", listItems,
                value: _fileDataParam.StepMode ? "1" : "0");
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            field = new eAdminColorField(_nField, eResApp.GetRes(Pref, 7930), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "selectedValueColorPicker", "txtSelectedValueColor",
                    value: _fileDataParam.SelectedValueColor);
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            listItems = new Dictionary<String, String>();
            listItems.Add("0", eResApp.GetRes(Pref, 59));
            listItems.Add("1", eResApp.GetRes(Pref, 58));
            field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7973), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblSequenceMode", listItems,
            value: _fileDataParam.SequenceMode ? "1" : "0");
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            #endregion

            //rubrique parente (desc.popupdescid)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            divField = new Panel();
            divField.CssClass = "field";
            divContainer.Controls.Add(divField);

            spanlabel = new HtmlGenericControl("span");
            spanlabel.InnerText = eResApp.GetRes(Pref, 244); //"Utiliser le catalogue de la rubrique"
            divField.Controls.Add(spanlabel);

            HtmlGenericControl ddlPopupDescId = new HtmlGenericControl("select");
            ddlPopupDescId.Attributes.Add("id", "ddlPopupDescId");
            ddlPopupDescId.Attributes.Add("name", "ddlPopupDescId");
            divField.Controls.Add(ddlPopupDescId);

            HtmlGenericControl option = new HtmlGenericControl("option");
            option.Attributes.Add("value", _fieldInfos.DescId.ToString());
            if (_fieldInfos.PopupDescId == _fieldInfos.DescId)
                option.Attributes.Add("selected", "selected");
            option.InnerText = eResApp.GetRes(Pref, 248); //Aucune rubrique
            ddlPopupDescId.Controls.Add(option);

            foreach (KeyValuePair<int, string> kvp in _dicoPopupFulltext)
            {
                if (_dicoPopupType.ContainsKey(kvp.Key)
                    && ((_dicoPopupType[kvp.Key] == (short)PopupType.DATA && _fieldInfos.PopupType == PopupType.DATA)
                        || (_dicoPopupType[kvp.Key] != (short)PopupType.DATA && _fieldInfos.PopupType != PopupType.DATA)
                    ))
                {
                    option = new HtmlGenericControl("option");
                    option.Attributes.Add("value", kvp.Key.ToString());
                    option.InnerText = kvp.Value;

                    if (_fieldInfos.PopupDescId == kvp.Key)
                        option.Attributes.Add("selected", "selected");

                    ddlPopupDescId.Controls.Add(option);
                }

            }
            ddlPopupDescId.Disabled = !_adminAllowed;


            //lier les valeurs de la rubrique (desc.bounddescid)
            divField = new Panel();
            divField.CssClass = "field";
            divContainer.Controls.Add(divField);

            spanlabel = new HtmlGenericControl("span");
            spanlabel.InnerText = eResApp.GetRes(Pref, 7332).Replace("<FIELD>", _fieldName); //"Lier les valeurs de la rubrique « <FIELD> » à la rubrique"
            divField.Controls.Add(spanlabel);

            HtmlGenericControl ddlBoundDescId = new HtmlGenericControl("select");
            ddlBoundDescId.Attributes.Add("id", "ddlBoundDescId");
            ddlBoundDescId.Attributes.Add("name", "ddlBoundDescId");
            ddlBoundDescId.Disabled = !_adminAllowed;
            divField.Controls.Add(ddlBoundDescId);

            option = new HtmlGenericControl("option");
            option.Attributes.Add("value", "0");
            if (_fieldInfos.BoundDescId == 0)
                option.Attributes.Add("selected", "selected");
            option.InnerText = eResApp.GetRes(Pref, 248); //Aucune rubrique
            ddlBoundDescId.Controls.Add(option);

            foreach (KeyValuePair<int, string> kvp in _dicoBoundFulltext)
            {
                if (_dicoBoundTreeview.ContainsKey(kvp.Key) && !_dicoBoundTreeview[kvp.Key])
                {
                    option = new HtmlGenericControl("option");
                    option.Attributes.Add("value", kvp.Key.ToString());
                    option.InnerText = kvp.Value;

                    if (_fieldInfos.BoundDescId == kvp.Key)
                        option.Attributes.Add("selected", "selected");

                    ddlBoundDescId.Controls.Add(option);
                }
            }


            //recheche obligatoire + seuil de recherche (filedataparam.NoAutoLoad + filedataparam.SearchLimit)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            listItems = new Dictionary<String, String>();
            listItems.Add("0", eResApp.GetRes(Pref, 7334));
            listItems.Add("1", eResApp.GetRes(Pref, 7335));
            field = new eAdminRadioButtonField(_nField, eResApp.GetRes(Pref, 7333), eAdminUpdateProperty.CATEGORY.FILEDATAPARAM, 0, "rblNoAutoLoad", listItems,
                value: _fileDataParam.NoAutoLoad ? "1" : "0");
            field.ReadOnly = !_adminAllowed;
            field.IsLabelBefore = true;
            field.Generate(divContainer);

            #region SearchLimit
            HtmlGenericControl p = new HtmlGenericControl("p");
            p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7336) + " ")); // "Dans tous les cas, la recherche automatique est déclenchée à partir de"

            HtmlGenericControl txtSearchLimit = new HtmlGenericControl("input");
            txtSearchLimit.Attributes.Add("type", "text");
            txtSearchLimit.Attributes.Add("id", "txtSearchLimit");
            txtSearchLimit.Attributes.Add("name", "txtSearchLimit");
            txtSearchLimit.Attributes.Add("value", _fileDataParam.SearchLimit.ToString());
            p.Controls.Add(txtSearchLimit);

            p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7337))); // caractères

            divField.Controls.Add(p);
            #endregion


            //Catalogue arborescent : valeur parent ou enfants uniquement (filedataparam.TreeViewOnlyLastChildren)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            divField = new Panel();
            divField.CssClass = "field";
            divContainer.Controls.Add(divField);

            spanlabel = new HtmlGenericControl("span");
            spanlabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7338), " :"); //"À la sélection de valeurs dans ce catalogue arborescent"
            divField.Controls.Add(spanlabel);

            RadioButtonList rblLastChildren = new RadioButtonList();
            rblLastChildren.ID = "rblTreeViewOnlyLastChildren";
            rblLastChildren.Items.Add(new ListItem(eResApp.GetRes(Pref, 7339), "0")); //"Les valeurs sélectionnées et leurs valeurs parentes seront affichées dans la rubrique"
            rblLastChildren.Items.Add(new ListItem(eResApp.GetRes(Pref, 7340), "1")); //"Seules les valeurs sélectionnées seront affichées dans la rubrique"
            rblLastChildren.RepeatDirection = RepeatDirection.Vertical;
            rblLastChildren.SelectedValue = _fileDataParam.TreeViewOnlyLastChildren == 1 ? "1" : "0";
            divField.Controls.Add(rblLastChildren);


            //compteur auto + début compteur + formule sql (filedataparam.DataAutoEnabled + filedataparam.DataAutoStart + filedataparam.DataAutoFormula)
            divContainer = new Panel();
            divContainer.CssClass = "fieldsContainer";
            stepContent.Controls.Add(divContainer);

            divField = new Panel();
            divField.CssClass = "field";
            divContainer.Controls.Add(divField);

            eAdminCheckboxField chxDataAutoEnabled = new eAdminCheckboxField(0, eResApp.GetRes(Pref, 7341), 0, 0, value: _fileDataParam.DataAutoEnabled); //"Générer automatiquement les codes des valeurs de ce catalogue"
            chxDataAutoEnabled.ReadOnly = !_adminAllowed;
            chxDataAutoEnabled.SetFieldControlID("chxDataAutoEnabled");
            chxDataAutoEnabled.Generate(divField);

            HtmlGenericControl divDataAutoStart = new HtmlGenericControl("div");
            divField.Controls.Add(divDataAutoStart);

            HtmlGenericControl rblDataAutoStart = new HtmlGenericControl("input");
            rblDataAutoStart.Attributes.Add("id", "rblDataAutoStart");
            rblDataAutoStart.Attributes.Add("type", "radio");
            rblDataAutoStart.Attributes.Add("name", "dataAuto");
            rblDataAutoStart.Attributes.Add("value", "dataAutoStart");
            if (String.IsNullOrEmpty(_fileDataParam.DataAutoFormula))
                rblDataAutoStart.Attributes.Add("checked", "checked");
            divDataAutoStart.Controls.Add(rblDataAutoStart);

            HtmlGenericControl lblDataAutoStart = new HtmlGenericControl("label");
            lblDataAutoStart.Attributes.Add("for", "rblDataAutoStart");
            lblDataAutoStart.InnerText = eResApp.GetRes(Pref, 7342); //"À partir d'un compteur commençant à"
            divDataAutoStart.Controls.Add(lblDataAutoStart);

            HtmlGenericControl txtDataAutoStart = new HtmlGenericControl("input");
            txtDataAutoStart.Attributes.Add("type", "text");
            txtDataAutoStart.Attributes.Add("id", "txtDataAutoStart");
            txtDataAutoStart.Attributes.Add("name", "txtDataAutoStart");
            txtDataAutoStart.Attributes.Add("value", _fileDataParam.DataAutoStart.ToString());
            divDataAutoStart.Controls.Add(txtDataAutoStart);

            HtmlGenericControl divDataAutoFormula = new HtmlGenericControl("div");
            divField.Controls.Add(divDataAutoFormula);

            HtmlGenericControl rblDataAutoFormula = new HtmlGenericControl("input");
            rblDataAutoFormula.Attributes.Add("id", "rblDataAutoFormula");
            rblDataAutoFormula.Attributes.Add("type", "radio");
            rblDataAutoFormula.Attributes.Add("name", "dataAuto");
            rblDataAutoFormula.Attributes.Add("value", "dataAutoFormula");
            if (!String.IsNullOrEmpty(_fileDataParam.DataAutoFormula))
                rblDataAutoFormula.Attributes.Add("checked", "checked");
            divDataAutoFormula.Controls.Add(rblDataAutoFormula);

            HtmlGenericControl lblDataAutoFormula = new HtmlGenericControl("label");
            lblDataAutoFormula.Attributes.Add("for", "rblDataAutoFormula");
            lblDataAutoFormula.InnerText = eResApp.GetRes(Pref, 7343); //"À partir de la formule SQL suivante"
            divDataAutoFormula.Controls.Add(lblDataAutoFormula);

            HtmlGenericControl txtDataAutoFormula = new HtmlGenericControl("input");
            txtDataAutoFormula.Attributes.Add("type", "text");
            txtDataAutoFormula.Attributes.Add("id", "txtDataAutoFormula");
            txtDataAutoFormula.Attributes.Add("name", "txtDataAutoFormula");
            txtDataAutoFormula.Attributes.Add("value", _fileDataParam.DataAutoFormula);
            divDataAutoFormula.Controls.Add(txtDataAutoFormula);
        }



    }
}