using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu de la liste des rubriques RGPD
    /// </summary>
    public class eAdminFieldsRGPDListRenderer : eAdminRenderer
    {
        /// <summary>Liste des rubriques</summary>
        protected List<eAdminFieldInfos> _listFields;

        /// <summary>Liste des colonnes</summary>
        protected List<eAdminFieldsListRenderer.ListCol> _listColumns;

        /// <summary>Liste des paramètres DESCADV</summary>
        protected DescAdvDataSet _descAdv;

        /// <summary>Liste des utilisateurs et groupes</summary>
        List<eUser.UserListItem> _listUsersAndGroups;

        /// <summary>Liste des libellés Nature</summary>
        protected Dictionary<DESCADV_RGPD_NATURE, string> _dicoNatureLabels;

        /// <summary>Liste des labels Catégories de données personnel</summary>
        protected Dictionary<DESCADV_RGPD_PERSONNAL_CATEGORY, string> _dicoPersonalCatLabels;

        /// <summary>Liste des labels Catégories de données sensibles</summary>
        protected Dictionary<DESCADV_RGPD_SENSITIVE_CATEGORY, string> _dicoSensitiveCatLabels;

        /// <summary>
        /// Constructeur interne
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="tab">descid de la table</param>
        protected eAdminFieldsRGPDListRenderer(ePref pref, int tab)
        {
            Pref = pref;
            _tab = tab;
        }

        /// <summary>
        /// Méthode d'intanciation externe
        /// </summary>
        /// <param name="pref">Preferences</param>
        /// <param name="tab">descid de la table</param>
        /// <returns></returns>
        public static eAdminFieldsRGPDListRenderer CreateAdminFieldsRGPDListRenderer(ePref pref, int tab)
        {
            return new eAdminFieldsRGPDListRenderer(pref, tab);
        }

        /// <summary>
        /// Charge les infos nécéssaires au rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {

                _listColumns = new List<eAdminFieldsListRenderer.ListCol>()
                {
                    new eAdminFieldsListRenderer.ListCol("ID", 60),
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 223), 150), //Libellé
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 7727), 100), //Nature
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 7586), 150), //Catégorie
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 75), 150), //Autre
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 8331), 150), //Utilisation
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 8332), 130), //Pseudonymisation
                    //new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 964), 100), //Règle
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 8319), 140), //Responsable du traitement
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 8333), 140), //Délégué à la protection des données
                    new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 8334), 140), //Responsable
                    //new eAdminFieldsListRenderer.ListCol(eResApp.GetRes(Pref, 75), 120), //Autre
                };

                eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                bool bDalOpen = dal.IsOpen;
                try
                {
                    if (!bDalOpen)
                        dal.OpenDatabase();

                    //chargement list fields
                    _listFields = eAdminFieldInfos.GetFieldsList(Pref, dal, _tab, out _sErrorMsg);
                    if (!String.IsNullOrEmpty(_sErrorMsg))
                        return false;

                    //chargement paramètres RGPD dans DescADV
                    _descAdv = new DescAdvDataSet();
                    _descAdv.LoadAdvParams(dal, _listFields.Select<eAdminFieldInfos, int>(f => f.DescId).ToList(), eDataQualityTools.GetListRGPDDescAdvParameter());

                    //chargement list user et group
                    eUser usrObj = new eUser(dal, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, new List<string>());
                    StringBuilder sbError = new StringBuilder();
                    _listUsersAndGroups = usrObj.GetUserList(true, false, "", sbError);
                }
                finally
                {
                    if (!bDalOpen)
                        dal.CloseDatabase();
                }

                //Chargement des labels nature/catégorie
                _dicoNatureLabels = new Dictionary<DESCADV_RGPD_NATURE, string>();
                foreach (DESCADV_RGPD_NATURE nature in Enum.GetValues(typeof(DESCADV_RGPD_NATURE)))
                {
                    _dicoNatureLabels.Add(nature, eDataQualityTools.GetNatureLabel(Pref, nature));
                }

                _dicoPersonalCatLabels = new Dictionary<DESCADV_RGPD_PERSONNAL_CATEGORY, string>();
                foreach (DESCADV_RGPD_PERSONNAL_CATEGORY category in Enum.GetValues(typeof(DESCADV_RGPD_PERSONNAL_CATEGORY)))
                {
                    _dicoPersonalCatLabels.Add(category, eDataQualityTools.GetCategoryLabel(Pref, category));
                }

                _dicoSensitiveCatLabels = new Dictionary<DESCADV_RGPD_SENSITIVE_CATEGORY, string>();
                foreach (DESCADV_RGPD_SENSITIVE_CATEGORY category in Enum.GetValues(typeof(DESCADV_RGPD_SENSITIVE_CATEGORY)))
                {
                    _dicoSensitiveCatLabels.Add(category, eDataQualityTools.GetCategoryLabel(Pref, category));
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Fait le rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                CreateTable();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Rendu du tableau principale
        /// </summary>
        private void CreateTable()
        {
            HtmlGenericControl htmlTable = new HtmlGenericControl("table");
            HtmlGenericControl sectionThead = new HtmlGenericControl("thead");
            HtmlGenericControl sectionTbody = new HtmlGenericControl("tbody");
            //sectionTbody.Style.Add("height", "500px");
            HtmlGenericControl tableTr = new HtmlGenericControl("tr");
            HtmlGenericControl tableTh = new HtmlGenericControl("th");
            HtmlGenericControl tableTd = new HtmlGenericControl("td");

            htmlTable.ID = "tableFieldsList";
            htmlTable.Attributes.Add("class", "mTab");

            #region HEADER
            htmlTable.Controls.Add(sectionThead);

            sectionThead.Controls.Add(tableTr);

            int colNum = 0;
            String thClass;
            foreach (eAdminFieldsListRenderer.ListCol col in _listColumns)
            {
                thClass = "hdBgCol";

                tableTh = new HtmlGenericControl("th");
                if (col.Format == FieldFormat.TYP_BIT)
                {
                    thClass += " chkCol";
                }
                else if (col.Format == FieldFormat.TYP_NUMERIC)
                {
                    thClass += " numCol";
                }
                if (col.HideForPrint)
                {
                    thClass += " hiddenForPrint";
                }

                tableTh.Attributes.Add("class", thClass);
                tableTh.Style.Add("min-width", col.Width + "px");
                tableTh.Attributes.Add("title", col.Label);
                tableTr.Controls.Add(tableTh);

                HtmlGenericControl divColLabel = new HtmlGenericControl("div");
                divColLabel.InnerText = col.Label;
                tableTh.Controls.Add(divColLabel);

                HtmlGenericControl divSortButtons = new HtmlGenericControl("div");
                divSortButtons.Attributes.Add("class", "buttonsSort");
                divColLabel.Controls.Add(divSortButtons);

                HtmlGenericControl spanButtonUp = new HtmlGenericControl("span");
                spanButtonUp.Attributes.Add("class", "icon-caret-up btnSort");
                spanButtonUp.Attributes.Add("onclick", String.Concat("nsAdminFieldsList.sortTable('tableFieldsList', ", colNum, ", 1, ", (col.Format == FieldFormat.TYP_NUMERIC) ? "true" : "false", ")"));
                divSortButtons.Controls.Add(spanButtonUp);

                HtmlGenericControl spanButtonDown = new HtmlGenericControl("span");
                spanButtonDown.Attributes.Add("class", "icon-caret-down btnSort");
                spanButtonDown.Attributes.Add("onclick", String.Concat("nsAdminFieldsList.sortTable('tableFieldsList', ", colNum, ", -1, ", (col.Format == FieldFormat.TYP_NUMERIC) ? "true" : "false", ")"));
                divSortButtons.Controls.Add(spanButtonDown);

                colNum++;
            }
            #endregion

            #region BODY
            string yesLabel = eResApp.GetRes(Pref, 58);
            string noLabel = eResApp.GetRes(Pref, 59);

            int lineNum = 1;
            htmlTable.Controls.Add(sectionTbody);

            foreach (eAdminFieldInfos field in _listFields)
            {
                bool rgpdEnabled = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_ENABLED, "0") == "1";

                if (!rgpdEnabled)
                    continue;

                tableTr = new HtmlGenericControl("tr");
                tableTr.Attributes.Add("class", String.Concat("line", lineNum));
                tableTr.Attributes.Add("data-did", field.DescId.ToString());
                sectionTbody.Controls.Add(tableTr);

                lineNum = (lineNum == 1) ? 2 : 1;

                // Nom SQL du champ
                CreateCell(tableTr, field.DescId, field.FieldName);

                //Libellé
                CreateCell(tableTr, field.DescId, field.Labels[Pref.LangId]);

                //Nature
                string natureLabel = String.Empty;
                string nature = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_NATURE, ((int)DESCADV_RGPD_DEFAULT_VALUES.NATURE).ToString());
                DESCADV_RGPD_NATURE natureValue;
                if (DESCADV_RGPD_NATURE.TryParse(nature, out natureValue))
                    natureLabel = _dicoNatureLabels[natureValue];

                CreateCell(tableTr, field.DescId, natureLabel);

                //Catégorie
                string categoryLabel = String.Empty;
                if (natureValue == DESCADV_RGPD_NATURE.PERSONAL)
                {
                    string category = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.PERSONAL_CATEGORY).ToString());
                    DESCADV_RGPD_PERSONNAL_CATEGORY categoryValue;
                    if (DESCADV_RGPD_PERSONNAL_CATEGORY.TryParse(category, out categoryValue))
                        categoryLabel = _dicoPersonalCatLabels[categoryValue];
                }
                else if (natureValue == DESCADV_RGPD_NATURE.SENSITIVE)
                {
                    string category = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_SENSIBLE_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.SENSITIVE_CATEGORY).ToString());
                    DESCADV_RGPD_SENSITIVE_CATEGORY categoryValue;
                    if (DESCADV_RGPD_SENSITIVE_CATEGORY.TryParse(category, out categoryValue))
                        categoryLabel = _dicoSensitiveCatLabels[categoryValue];
                }

                CreateCell(tableTr, field.DescId, categoryLabel);

                //Catégorie Autre
                string categoryPrecision = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION, String.Empty);
                CreateCell(tableTr, field.DescId, categoryPrecision);

                //Utilisation
                string dataPurpose = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_DATA_PURPOSE, String.Empty);
                CreateCell(tableTr, field.DescId, dataPurpose);

                //Pseudonymisation
                bool pseudoEnabled = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_ENABLED, "0") == "1";
                CreateCell(tableTr, field.DescId, pseudoEnabled ? yesLabel : noLabel);

                //Règle
                //string pseudoRules = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_PSEUDONYMISATION_RULES, String.Empty);
                //CreateCell(tableTr, field.DescId, pseudoRules);

                //Responsable traitement 1
                CreateCell(tableTr, field.DescId, GetUserGroupLabel(field.DescId, DESCADV_PARAMETER.RGPD_RESPONSIBLE_1));

                //Responsable traitement 2
                CreateCell(tableTr, field.DescId, GetUserGroupLabel(field.DescId, DESCADV_PARAMETER.RGPD_RESPONSIBLE_2));

                //Responsable traitement 3
                CreateCell(tableTr, field.DescId, GetUserGroupLabel(field.DescId, DESCADV_PARAMETER.RGPD_RESPONSIBLE_3));

                //Responsable Autre
                //string responsibleOther = _descAdv.GetAdvInfoValue(field.DescId, DESCADV_PARAMETER.RGPD_RESPONSIBLE_OTHER, String.Empty);
                //CreateCell(tableTr, field.DescId, responsibleOther);
            }
            #endregion

            _pgContainer.Controls.Add(htmlTable);
        }

        /// <summary>
        /// Création d'une cellule avec sa valeur
        /// </summary>
        /// <param name="tr">Ligne "tr"</param>
        /// <param name="descid">descid de la rubrique</param>
        /// <param name="value">Valeur</param>
        /// <param name="bShowTooltip">Affichage d'une infobulle</param>
        /// <param name="tooltip">Infobulle</param>
        /// <param name="hideForPrint">Cache la rubrique lors de l'impression</param>
        private void CreateCell(HtmlGenericControl tr, int descid,
            String value = "", Boolean bShowTooltip = true, String tooltip = "", Boolean hideForPrint = false)
        {
            String cssClass = "";

            HtmlGenericControl td = new HtmlGenericControl("td");
            if (hideForPrint)
                cssClass += " hiddenForPrint";
            td.Attributes.Add("class", cssClass);

            if (bShowTooltip)
                td.Attributes.Add("title", String.IsNullOrEmpty(tooltip) ? value : tooltip);

            td.Attributes.Add("value", value);

            HtmlGenericControl spanContent = new HtmlGenericControl("span");
            spanContent.Attributes.Add("class", "cellContent");
            td.Controls.Add(spanContent);

            HtmlGenericControl spanValue = new HtmlGenericControl("span");
            spanValue.Attributes.Add("class", "cellValue");
            spanValue.InnerText = value;
            spanContent.Controls.Add(spanValue);

            tr.Controls.Add(td);
        }

        /// <summary>
        /// Renvoi le libelle des responsable
        /// </summary>
        /// <param name="descid">descid de la rubrique</param>
        /// <param name="parameter">paramètre RGPD dans DESCADV</param>
        /// <returns></returns>
        protected string GetUserGroupLabel(int descid, DESCADV_PARAMETER parameter)
        {
            string userLabel = String.Empty;

            string responsable = _descAdv.GetAdvInfoValue(descid, parameter, String.Empty);
            if (!String.IsNullOrEmpty(responsable))
            {
                if (_listUsersAndGroups.Exists(u => u.ItemCode == responsable))
                    userLabel = _listUsersAndGroups.First(u => u.ItemCode == responsable).Libelle;
            }

            return userLabel;
        }
    }
}