using Com.Eudonet.Cache;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.wcfs.data.import;
using Com.Eudonet.Common.Import;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu de l'étape du mise en correspondance 
    /// </summary>
    public class eMappingStepRenderer : eComunImportRenderer
    {
        private const int TABWIDTH = 110;
        private const int TABPADDING = 247;
        private ImportRow _headerLine;
        private ImportRow _dataLine;

        private int maxLength = 30;
        private ImportTemplateWizard _importTemplate;
        private double _maxTab
        {
            get { return (this._nMaxWidth / TABWIDTH); }
        }
        private double _nbDisplayTab;
        private double _nbPaging;
        private int _nMaxWidth
        {
            get { return WizardParam.Width - TABPADDING - (WizardParam.Width * 18 / 100); }
        }


        /// <summary>
        /// Génére le contenu HTML / Contrôles de l'étape du mapping 
        /// </summary>
        /// <param name="pref">Pref de l'utilisateur</param>
        /// <param name="wizardParam">Paramètres globales du wizard</param>
        /// <param name="result">Résultat de l'étape</param>
        public eMappingStepRenderer(ePref pref, eImportWizardParam wizardParam, eImportSourceInfosCallReturn result)
            : base(pref, wizardParam, result)
        {
            this._headerLine = result?.SourceInfos?.HeaderLine;
            this._dataLine = result?.SourceInfos?.FirstDataLine;

            if (wizardParam.ImportTemplateParams.ImportTemplateId > 0)
            {
                this._importTemplate = ImportTemplateWizard.GetImportTemplate(Pref, WizardParam.ImportTemplateParams.ImportTemplateId);
                CheckDataSourceForImportTemplateMapping();
                wizardParam.ImportTemplateParams = this._importTemplate.ImportTemplateLine;
            }

            if (this._headerLine == null || this._dataLine == null)
                throw new ImportException(ImportResultCode.EmptyDataSource, eResApp.GetRes(pref, 8648));


        }


        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public override IWizardStepRenderer Init()
        {
            return this;
        }

        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public override Panel Render()
        {
            return CreateMappingStepContainer();
        }


        /// <summary>
        /// Créer et initialise le conteneur de l'etape
        /// </summary>
        /// <returns></returns>
        private Panel CreateMappingStepContainer()
        {
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "data-source-second-step";
            stepContainer.Style.Add(HtmlTextWriterStyle.Height, (WizardParam.Height - 180) + "px"); // 170px représente les étapes et les bouton
            stepContainer.Controls.Add(CreateMappingTabsContainer());
            stepContainer.Controls.Add(CreateMappingRappelContainer());

            //if (WizardParam.ImportTemplateParams.ImportTemplateId > 0)
            //    GetPermission(stepContainer, ref template);
            //GetDivPaging();
            return stepContainer;
        }


        /// <summary>
        /// Retourne le Code HTML du conteneur
        /// </summary>
        /// <returns></returns>
        private Panel CreateMappingTabsContainer()
        {
            bool bEmptyModelName = this._importTemplate == null || string.IsNullOrEmpty(this._importTemplate.ImportTemplateLine.ImportTemplateName);
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "data-source-mappingTab-step";
            stepContainer.Style.Add(HtmlTextWriterStyle.Height, (WizardParam.Height - 180) + "px");

            HtmlGenericControl literal = new HtmlGenericControl("div");
            literal.Attributes.Add("class", "mapping_header_title");
            literal.InnerHtml = eResApp.GetRes(Pref, 8341);
            stepContainer.Controls.Add(literal);

            #region Séléctionnez un modèle
            Button btn = new Button();
            btn.UseSubmitBehavior = false;
            btn.Attributes.Add("class", "mapping_btn");
            btn.ID = "btn_import_template_wizard";
            btn.Text = eResApp.GetRes(Pref, 8684);
            btn.Attributes.Add("onclick", string.Concat("oImportTemplateWizardManager.ShowImportTemplateListWizard();"));
            stepContainer.Controls.Add(btn);
            HtmlGenericControl modelName = new HtmlGenericControl("label");
            modelName.InnerHtml = bEmptyModelName ? eResApp.GetRes(Pref, 1111) : this._importTemplate.ImportTemplateLine.ImportTemplateName;
            modelName.Attributes.Add("class", "import_template_name");
            GetTemplateName(modelName);

            stepContainer.Controls.Add(modelName);

            // eFilterReportList
            #endregion

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "field_Global_container");
            container.Controls.Add(GetOrigineTabs());
            container.Controls.Add(GetDestinationTabs());

            stepContainer.Controls.Add(GetDivPaging());
            stepContainer.Controls.Add(container);
            stepContainer.Controls.Add(GetTableOptions());

            return stepContainer;
        }

        /// <summary>
        /// Retourne le nom du modèle d'import
        /// </summary>
        /// <param name="modelName">Composant HTML</param>
        protected override void GetTemplateName(HtmlGenericControl modelName)
        {

            if (WizardParam.ImportTemplateParams.ImportTemplateId > 0 && this._importTemplate != null && !this._importTemplate.ImportTemplateLine.IsNotSavedImportTemplate)
            {
                WizardParam.SetTabParams(this._importTemplate?.ImportTemplateLine.ImportParams?.Tables);
                modelName.Attributes.Add("title", eResApp.GetRes(Pref, 8707));
                modelName.Attributes.Add("onclick", "oImportWizardInternal.ResetImportTemplate(this)");
                modelName.Attributes.Add("class", "resume_import_template_name");
                modelName.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            }
        }

        /// <summary>
        /// Retourne le code HTML da la partie Rappel de l'assistant
        /// </summary>
        /// <returns></returns>
        private Panel CreateMappingRappelContainer()
        {
            string id = "mappingRappelText";
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "data-source-mappingRappel-step";

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "mappingRappelContainer");
            container.InnerHtml = string.Concat(eResApp.GetRes(Pref, 6890), " : <br/>");

            HtmlGenericControl txt = new HtmlGenericControl("div");
            txt.Attributes.Add("class", id);
            txt.ID = id;
            txt.InnerText = eResApp.GetRes(Pref, 8398);
            stepContainer.Controls.Add(container);
            stepContainer.Controls.Add(txt);
            return stepContainer;
        }


        /// <summary>
        /// Retourne information complémentaire sur le champ
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string GetTypeInfos(Field field)
        {

            switch (field.Format)
            {
                case FieldFormat.TYP_CHAR:

                    switch (field.Popup)
                    {
                        case PopupType.NONE:
                        case PopupType.FREE:
                            int fieldLenght = field.Length;
                            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                            try
                            {
                                dal?.OpenDatabase();
                                fieldLenght = eLibTools.CheckFieldSize(dal, field.Table.TabName, field.RealName, fieldLenght, false);
                            }
                            catch (Exception)
                            {

                                throw;
                            }
                            finally
                            {
                                dal?.CloseDatabase();
                            }

                            return eResApp.GetRes(Pref, 8375).Replace("<NBCHAR>", fieldLenght.ToString());
                        case PopupType.ONLY:
                        case PopupType.DATA:
                            if (field.Multiple)
                                return eResApp.GetRes(Pref, 8377).Replace("<SEPARATOR>", ImportSpecification.CatalagValuesSeparator);
                            else
                                return eResApp.GetRes(Pref, 8661);

                        default:
                            return eResApp.GetRes(Pref, 8377).Replace("<SEPARATOR>", ImportSpecification.CatalagValuesSeparator);

                    }

                case FieldFormat.TYP_DATE:

                    string resdate = eResApp.GetRes(Pref, 8376).Replace("<DATEFORMAT>", eDate.ConvertBddToDisplay(Pref.CultureInfo, String.Concat(String.Concat("31/12/", DateTime.Now.Year))));
                    if (field.Table.EdnType == EdnType.FILE_PLANNING && field.Table.CalendarEnabled && field.Descid % 100 == (int)PlanningField.DESCID_TPL_END_TIME)
                    {
                        return string.Concat(resdate, " ", eResApp.GetRes(Pref, 2698));
                    }
                    else
                        return resdate;

                case FieldFormat.TYP_NUMERIC:
                case FieldFormat.TYP_MONEY:

                    if (field.Table.EdnType == EdnType.FILE_PLANNING && field.Table.CalendarEnabled && field.Descid % 100 == (int)PlanningField.DESCID_CALENDAR_ITEM)
                    {


                        return eResApp.GetRes(Pref, 2697);
                    }
                    else
                    {

                        string decimalDelimeter = eLibTools.GetConfigAdvValues(Pref, new List<eLibConst.CONFIGADV>() { eLibConst.CONFIGADV.NUMBER_DECIMAL_DELIMITER })[eLibConst.CONFIGADV.NUMBER_DECIMAL_DELIMITER];

                        string res = (field.Length > 0 ? eResApp.GetRes(Pref, 8425).Replace("<NBDECIMAL>", field.Length.ToString()).Replace("<DECIMALSEPARATOR>", string.IsNullOrEmpty(decimalDelimeter) ? eLibConst.DEFAULT_NUMBER_DECIMAL_DELIMITER : decimalDelimeter) : eResApp.GetRes(Pref, 8424));

                        return res;
                    }

                case FieldFormat.TYP_BIT:
                    return eResApp.GetRes(Pref, 8428);
                case FieldFormat.TYP_BITBUTTON:
                    return eResApp.GetRes(Pref, 8429);

                case FieldFormat.TYP_USER:
                    if (field.Multiple)
                        return eResApp.GetRes(Pref, 8378).Replace("<SEPARATOR>", ImportSpecification.CatalagValuesSeparator);
                    else
                        return eResApp.GetRes(Pref, 8662);

                case FieldFormat.TYP_EMAIL:
                    return eResApp.GetRes(Pref, 8433);
                case FieldFormat.TYP_GROUP: 
                    return eResApp.GetRes(Pref, 8377).Replace("<SEPARATOR>", ImportSpecification.CatalagValuesSeparator);
                default:
                    return string.Empty;
            }
        }


        /// <summary>
        /// Retourne le format en Texte du champs
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string GetTypeField(Field field)
        {
            switch (field.Format)
            {
                case FieldFormat.TYP_CHAR:
                    switch (field.Popup)
                    {
                        case PopupType.NONE:
                        case PopupType.FREE:
                            return eResApp.GetRes(Pref, 8421);
                        case PopupType.ONLY:
                        case PopupType.DATA:
                            return eResApp.GetRes(Pref, 8422);
                        default:
                            return eResApp.GetRes(Pref, 8377);

                    }

                case FieldFormat.TYP_GROUP:
                    return eResApp.GetRes(Pref, 8422);

                case FieldFormat.TYP_DATE:
                    return string.Empty;

                case FieldFormat.TYP_BIT:
                    return eResApp.GetRes(Pref, 8426);

                case FieldFormat.TYP_NUMERIC:
                case FieldFormat.TYP_MONEY:
                    return eResApp.GetRes(Pref, 8423);
                case FieldFormat.TYP_AUTOINC:
                    return string.Concat(eResApp.GetRes(Pref, 423), ".");
                case FieldFormat.TYP_EMAIL:
                    return eResApp.GetRes(Pref, 8432);

                case FieldFormat.TYP_WEB:
                    return eResApp.GetRes(Pref, 8434);

                case FieldFormat.TYP_USER:
                    return eResApp.GetRes(Pref, 8435);

                case FieldFormat.TYP_MEMO:
                    return eResApp.GetRes(Pref, 8430);

                case FieldFormat.TYP_PHONE:
                    return eResApp.GetRes(Pref, 8431);

                case FieldFormat.TYP_BITBUTTON:
                    return eResApp.GetRes(Pref, 8427);

                default:
                    return eResApp.GetRes(Pref, 229);
            }
        }



        /// <summary>
        /// Retourne la partie haute 
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetOrigineTabs()
        {

            HtmlGenericControl tabFieldContainer = new HtmlGenericControl("div");
            tabFieldContainer.Attributes.Add("class", "field_source_container");
            ImportColumn column;
            for (int i = 0; i < _headerLine.Columns.Count; i++)
            {
                string mapping = String.Join(";", ImportTemplateWizard.GetImportTemplateFieldMApped(this._importTemplate, new KeyValuePair<int, string>(i, _headerLine.Columns[i].SourceValue), this.ExcludedTab));
                column = _headerLine.Columns[i];
                string fieldId = i.ToString();
                HtmlGenericControl tabContainer = new HtmlGenericControl("div");
                tabContainer.Attributes.Add("field", fieldId);
                tabContainer.Attributes.Add("class", "fieldItem");
                tabContainer.Attributes.Add("draggable", "true");
                tabContainer.Attributes.Add("ondragstart", "oImportWizardInternal.DragStart(event)");
                tabContainer.Attributes.Add("ondrag", "oImportWizardInternal.Dragging(event)");
                tabContainer.Attributes.Add("ondragend", "oImportWizardInternal.DragEnd(event)");
                tabFieldContainer.Controls.Add(tabContainer);
                //tabContainer.Attributes.Add("fActive", "1");

                HtmlGenericControl opacityField = new HtmlGenericControl("div");
                opacityField.Attributes.Add("class", "opacityField");
                tabContainer.Controls.Add(opacityField);

                HtmlGenericControl subFieldHead = new HtmlGenericControl("div");
                subFieldHead.ID = fieldId;
                subFieldHead.Attributes.Add("ednkey", "");
                subFieldHead.Attributes.Add("ednorig", i.ToString());
                subFieldHead.Attributes.Add("edndescid", mapping);
                subFieldHead.Attributes.Add("ednlbl", "");
                tabContainer.Controls.Add(subFieldHead);

                HtmlGenericControl fieldLabel = new HtmlGenericControl("span");
                fieldLabel.Attributes.Add("class", "fieldcibleSpan");
                fieldLabel.InnerText = GetTrancat(column.SourceValue);
                fieldLabel.Attributes.Add("title", column.SourceValue);


                subFieldHead.Controls.Add(fieldLabel);
                subFieldHead.Attributes.Add("class", string.Concat("subFieldHead", mapping.Length > 0 ? " ednMappedOrg" : ""));

                HtmlGenericControl subFieldBottom = new HtmlGenericControl("div");
                subFieldBottom.Attributes.Add("class", "subFieldBottom");
                subFieldBottom.InnerText = _dataLine.Columns.Where(c => c.ColumnIndex.Equals(i)).Count() == 1 ? GetTrancat(_dataLine.Columns[i]?.SourceValue) : string.Empty;
                tabContainer.Controls.Add(subFieldBottom);

            }

            return tabFieldContainer;
        }



        /// <summary>
        /// Retourne la partie haute 
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetDestinationTabs()
        {
            int i = 1;
            string destinationContainer = "field_destination_container";
            List<Field> fields = new List<Field>();
            HtmlGenericControl tabFieldContainer = new HtmlGenericControl("div");
            tabFieldContainer.Attributes.Add("class", destinationContainer);
            tabFieldContainer.ID = destinationContainer;
            HtmlGenericControl globalTabFieldContainer = new HtmlGenericControl("div");
            globalTabFieldContainer.Attributes.Add("class", string.Concat("global_", destinationContainer));


            HtmlGenericControl tablsContainer = new HtmlGenericControl("div");
            tablsContainer.Attributes.Add("class", "tablsContainer");
            tablsContainer.Attributes.Add("bgc", Pref.ThemeXRM.Color);

            HtmlGenericControl headerTablsContainer = new HtmlGenericControl("div");
            headerTablsContainer.Attributes.Add("class", "tabls_destination_container");
            tablsContainer.Controls.Add(headerTablsContainer);

            headerTablsContainer.Style.Add(HtmlTextWriterStyle.BackgroundColor, Pref.ThemeXRM.Color);


            foreach (Cache.ImportInfo tab in this.ListImportInfo)
            {

                ICollection<ImportTabParams> importTemplateTabParams = new List<ImportTabParams>();
                // si aucune rubrique ne respecte les conditions 
                //Ne pas présenter la table parente en mode liste
                //Par contre, si une rubrique relationnelle à le même descid de la table parente, dans ce cas là, il faut la présenter(demande #67 414)
                if (tab.Info.Fields.Count() == 0 || (tab.Info.Table.DescId == WizardParam.ParentTab && tab.linkField == null))
                    continue;

                //en mode signet => Si on hérite du pp et pm du parent et que la table à afficher c'est adresse, on fait rien
                if (WizardParam.ParentTab != 0 && AddAdrTab() && tab.Info.Table.DescId == (int)TableType.ADR)
                    continue;

                string tabJsKey = tab.GetJsKey();



                if (this._importTemplate != null && this._importTemplate.ImportTemplateLine.ImportParams != null && this._importTemplate.ImportTemplateLine.ImportParams.Tables.Count() > 0)
                    importTemplateTabParams = this._importTemplate.ImportTemplateLine.ImportParams.Tables.ToList().Where(t => t.TabInfo.TabInfoId.Equals(tabJsKey)).ToList();

                SetTableHtml(i, tab, importTemplateTabParams.Count > 0 ? importTemplateTabParams.First() : null, headerTablsContainer, tab.Info.ImportRight ? "1" : "0", i == 1);


                if (tab.HideTab && (IsPPLinkFromParent() || IsPMLinkFromParent()))
                {
                    HtmlGenericControl txtDiv = new HtmlGenericControl("div");
                    txtDiv.Attributes.Add("class", "txtTabHeadImport fieldItemDisplayNone");
                    txtDiv.Attributes.Add("title", string.Format(eResApp.GetRes(Pref, 1921), ParentTab.Table.Libelle));
                    txtDiv.ID = String.Concat("txttab_", tabJsKey);
                    txtDiv.Attributes.Add("tEdndescid", tabJsKey);
                    //txtDiv.Attributes.Add("fActive", "0");
                    txtDiv.InnerHtml = string.Format(eResApp.GetRes(Pref, 1921), ParentTab.Table.Libelle);
                    tabFieldContainer.Controls.Add(txtDiv);
                }

                else
                    //BSE:#61 399 tri par libellé de la rubrique
                    foreach (Field field in tab.Info.Fields.OrderBy(a => a.Libelle))
                    {
                        // Pas de champ de mapping de champ de liaison
                        // Ne pas présenter les rubriques relationnelles
                        if (field.Alias.Contains("_LNK_") || field.Popup == PopupType.SPECIAL)
                            continue;

                        SetFieldHtml(tab, field, tabFieldContainer, importTemplateTabParams.Count > 0 ? importTemplateTabParams.First() : null, i == 1);
                    }

                i++;
            }

            this._nbDisplayTab = i - 1;

            globalTabFieldContainer.Controls.Add(tablsContainer);
            globalTabFieldContainer.Controls.Add(tabFieldContainer);

            return globalTabFieldContainer;
        }



        /// <summary>
        /// Déssiner un onglet
        /// </summary>
        /// <param name="i">position de l'onglet</param>
        /// <param name="tab">descid de la table</param>
        /// <param name="tableModelParam">paramètres du modèle d'import</param>
        /// <param name="importRight">Droit d'import sur la table</param>
        /// <param name="headerTablsContainer">container html</param>
        /// <param name="activated">Indique si la table est activée</param>
        private void SetTableHtml(int i, Cache.ImportInfo tab, ImportTabParams tableModelParam, HtmlGenericControl headerTablsContainer, string importRight, bool activated = false)
        {
            string tableKey = tab.GetJsKey();
            string libelle = tab.GetLibelle();
            string headerClass = string.Concat("subTableHead navEntry ", activated ? " navImportTitleActive navTitleActiveLight" : "");
            bool bMainTab = false;
            bool oblig = tab.IsRequired() || (tab.linkField == null && (tab.Info.Table.DescId == (int)TableType.PP && this.MainTabImportInfo.Info.Table.InterPPNeeded) || (tab.Info.Table.DescId == (int)TableType.PM && this.MainTabImportInfo.Info.Table.InterPMNeeded) || (tab.Info.Table.DescId == this.MainTabImportInfo.Info.Table.InterEVTDescid && this.MainTabImportInfo.Info.Table.InterEVTNeeded));

            HtmlGenericControl subTabledHead = new HtmlGenericControl("div");
            subTabledHead.ID = String.Concat("t_", tableKey);
            subTabledHead.Attributes.Add("edndescid", tableKey);
            subTabledHead.Attributes.Add("eImportRight", importRight);

            //Si la table Main
            if (this.MainTabImportInfo.GetJsKey() == tableKey)
            {
                subTabledHead.Attributes.Add("tActive", "1");
                subTabledHead.Attributes.Add("tMainTab", "1");
                bMainTab = true;
            }
            else
                subTabledHead.Attributes.Add("tActive", "0");

            if (tableModelParam != null && tableModelParam.IsKey && !bMainTab)
            {
                headerClass = string.Concat(headerClass, " ednKeyImportField");
                subTabledHead.Attributes.Add("ednTabkey", "1");
            }
            subTabledHead.Attributes.Add("class", headerClass);

            subTabledHead.Attributes.Add("ednTabLabelle", libelle);

#if DEBUG
            subTabledHead.Attributes.Add("title", libelle + "\nTabKey : " + tableKey);
#else
            subTabledHead.Attributes.Add("title", libelle);
#endif
            //subTabledHead.InnerText = GetTrancat(libelle);
            subTabledHead.InnerText = libelle;

            subTabledHead.Attributes.Add("tActiveTabPaging", (i <= this._maxTab) ? "1" : "0");

            subTabledHead.Attributes.Add("tPage", Math.Ceiling(i / this._maxTab).ToString());
            subTabledHead.Attributes.Add("oblig", oblig ? "1" : "0");
            subTabledHead.Attributes.Add("canbekey", tab.linkTab == null && !bMainTab ? "1" : "0");

            //BSE:#61 399 : placer l'astérisque à droite pour les rubriques relationnelles/relaions principales obligatoires
            if (oblig)
            {
                HtmlGenericControl fieldLabelAs = new HtmlGenericControl("span");
                fieldLabelAs.Attributes.Add("class", "tableCibleSpanAs");
                fieldLabelAs.InnerText = "*";
                subTabledHead.Controls.Add(fieldLabelAs);
            }


            headerTablsContainer.Controls.Add(subTabledHead);
        }

        /// <summary>
        /// Génère le code HTMl d'une rubrique 
        /// </summary>
        /// <param name="tab">la table à la quelle elle appartient la rubrique</param>
        /// <param name="field">La rubrique</param>
        /// <param name="tabFieldContainer">Le contenneur</param>
        /// <param name="tableParam">Le contenneur</param>
        /// <param name="activated">Indique si la rubrique est affiché au démarage de l'assistant</param>
        private void SetFieldHtml(Cache.ImportInfo tab, Field field, HtmlGenericControl tabFieldContainer, ImportTabParams tableParam, bool activated = false)
        {
            IEnumerable<ImportMapField> mapFields = tableParam?.Mapp.Where(f => f.Field.Equals(field.Descid));
            ImportMapField mappedField = null;

            // Rubrique autorisée pour l'import
            bool fieldImportRight = !tab.Info.NotAllowedFields.Contains(field.Descid);

            // si on a pas le droits d'import sur la table et/ ou on a pas les droits d'import sur la rubrique

            string importInformation = !tab.Info.ImportRight ? string.Concat(" ", eResApp.GetRes(Pref, 8437)) : fieldImportRight ?
                string.Empty : string.Concat(" ", eResApp.GetRes(Pref, 8436));

            if (field.Format == FieldFormat.TYP_AUTOINC)
            {
                importInformation = eResApp.GetRes(Pref, 2419);
            }

            string fieldId = String.Concat("f_", tab.GetJsKey() + "_" + field.Descid);
            string infoField = string.Concat(GetTypeInfos(field), importInformation);


            if (tableParam != null && mapFields.Count() > 0)
                mappedField = mapFields.First();

            List<ImportColumn> mappedHeaderField = _headerLine.Columns.Where(col => col.SourceValue.ToLower().Equals(mappedField?.ColName?.ToLower())).ToList();
            bool existeCol = mapFields != null && mappedHeaderField != null && mappedHeaderField.Count() > 0;
            if (!existeCol && tableParam != null)
            {
                ImportParams param = WizardParam.ImportTemplateParams.ImportParams;
                ImportTemplateWizard.RemoveMappedCol(ref param, mappedField?.ColName.ToLower());
                WizardParam.ImportTemplateParams.ImportParams = param;
            }


            bool bMapped = mappedField != null && !string.IsNullOrEmpty(mappedField.ColName) && mappedField.Field > 0 && existeCol;
            bool bKey = bMapped && tableParam.Keys.Contains(field.Descid);
            HtmlGenericControl fieldContainer = new HtmlGenericControl("div");
            fieldContainer.Attributes.Add("class", string.Concat("fieldItem", activated ? "" : " fieldItemDisplayNone"));
            fieldContainer.Attributes.Add("field", fieldId);
            fieldContainer.Attributes.Add("importRightField", fieldImportRight ? "1" : "0");

            tabFieldContainer.Controls.Add(fieldContainer);

            //fieldContainer.Attributes.Add("fActive", activated ? "1" : "0");


            fieldContainer.Attributes.Add("tEdndescid", tab.GetJsKey());

            HtmlGenericControl opacityField = new HtmlGenericControl("div");
            opacityField.Attributes.Add("class", "opacityField");
            fieldContainer.Controls.Add(opacityField);

            HtmlGenericControl subFieldHead = new HtmlGenericControl("div");
            subFieldHead.ID = fieldId;
            subFieldHead.Attributes.Add("class", string.Concat("subFieldHeadImport", bKey ? " ednKeyImportField" : string.Empty));
            subFieldHead.Attributes.Add("ednkey", bKey ? "1" : string.Empty);
            subFieldHead.Attributes.Add("ednorig", bMapped ? mappedHeaderField?.First()?.ColumnIndex.ToString() : string.Empty);
            subFieldHead.Attributes.Add("ednlabel", field.Libelle);
            subFieldHead.Attributes.Add("ednTablabel", field.Table.Libelle);
            subFieldHead.Attributes.Add("edndescid", field.Descid.ToString());
            subFieldHead.Attributes.Add("ednTabdescid", tab.GetJsKey());
            subFieldHead.Attributes.Add("ednlbl", string.Empty);
            subFieldHead.Attributes.Add("ednType", GetTypeField(field));
            subFieldHead.Attributes.Add("ednFormat", ((int)field.Format).ToString());
            subFieldHead.Attributes.Add("endmulti", field.Multiple ? "1" : "0");

            if (!string.IsNullOrEmpty(infoField))
                subFieldHead.Attributes.Add("ednTypeInfos", infoField);


            fieldContainer.Controls.Add(subFieldHead);
            HtmlGenericControl fieldLabel = new HtmlGenericControl("span");
            fieldLabel.Attributes.Add("class", "fieldcibleSpan");



#if DEBUG
            fieldLabel.Attributes.Add("title", field.Libelle + "\nDescId : " + field.Descid);
#else
             fieldLabel.Attributes.Add("title", field.Libelle);
#endif

            fieldLabel.InnerText = GetTrancat(field.Libelle);
            //Rubrique en italic si on  a pas les droits d'import
            if (!fieldImportRight)
                fieldLabel.Style.Add(HtmlTextWriterStyle.FontStyle, "italic");

            subFieldHead.Controls.Add(fieldLabel);

            // Si rubrique obligatoire sans valeur par défaut
            if (eLibTools.CheckObligat(field) && string.IsNullOrEmpty(field.DefaultValue))
            {

                HtmlGenericControl fieldLabelAs = new HtmlGenericControl("span");
                fieldLabelAs.Attributes.Add("class", "fieldcibleSpanAs");
                fieldLabelAs.Attributes.Add("title", string.Concat(field.Libelle, " <", eResApp.GetRes(Pref, 6304), ">"));
                fieldLabelAs.InnerText = "*";
                subFieldHead.Attributes.Add("ednoblig", "1");
                subFieldHead.Controls.Add(fieldLabelAs);
            }

            //Ajouter les options US #998
            HtmlGenericControl options = DrawFieldOptions(tab, field, mappedField);
            if (options != null)
                fieldLabel.Controls.Add(options);

            

            HtmlGenericControl subFieldBottom = new HtmlGenericControl("div");
            subFieldBottom.Attributes.Add("class", string.Concat("subFieldBottom", bMapped ? " ednMappedTgt" : string.Empty));
            if (bMapped)
            {
                subFieldBottom.Attributes.Add("ednorig", mappedHeaderField?.First()?.ColumnIndex.ToString());
                HtmlGenericControl subFielMappeddBottom = new HtmlGenericControl("span");
                subFielMappeddBottom.Attributes.Add("title", mappedField.ColName);
                subFielMappeddBottom.InnerText = GetTrancat(mappedField.ColName);
                subFieldBottom.Controls.Add(subFielMappeddBottom);
            }
            fieldContainer.Controls.Add(subFieldBottom);
        }

        /// <summary>
        /// Génère un Trancat d'une chaine de caractère
        /// </summary>
        /// <param name="value">la valeur à traiter</param>
        /// <returns></returns>
        private string GetTrancat(string value, bool stripHtml = true)
        {
            // #74 612 - On supprime les tags HTML de la chaîne source, car si la chaîne est tronquée au milieu d'une balise et utilisée sur un InnerHtml, ça impacte l'affichage
            // La suppression est faite avant vérification de la longueur de la chaîne, pour pouvoir ensuite conserver un maximum de caractères sur la chaîne tronquée
            if (value != null && stripHtml)
                value = HtmlTools.StripHtml(value);

            return value?.Length > maxLength ? string.Concat(value?.Substring(0, maxLength), "...") : value;
        }

        /// <summary>
        /// Retourne un HtmlGenericControl de la partie Afficher /Cacher les rubriques mappées
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetOptions(string txt, string id, bool check = false)
        {
            HtmlGenericControl option = new HtmlGenericControl("div");
            option.Attributes.Add("class", "divOptions");
            RadioButton rbTextPaste = new RadioButton();
            rbTextPaste.Attributes.Add("class", "rdBOptions");
            rbTextPaste.GroupName = "DataSource";
            rbTextPaste.Checked = check;
            rbTextPaste.Text = txt;
            rbTextPaste.ID = id;//check ? "rdbShow" : "rdbHide";
            // Règle css affiche ou masque les rubrique mappées
            //rbTextPaste.Attributes.Add("onclick", check ? string.Concat("oImportWizardInternal.ShowMapElement(event);") : string.Concat("oImportWizardInternal.HideMapElement(event);"));
            option.Controls.Add(rbTextPaste);
            return option;
        }


        /// <summary>
        /// Retourne le block d'informations concernant les actions possibles sur l'assistant de l'import à l'étape du mapping
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetTableKeysHeader()
        {
            string obligatoire = eResApp.GetRes(Pref, 6304);
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "tables_header");

            HtmlGenericControl containerDiv = new HtmlGenericControl("div");
            HtmlGenericControl tablsText = new HtmlGenericControl("div");
            tablsText.Attributes.Add("class", "tables_header_title ednKey");
            tablsText.InnerHtml = eResApp.GetRes(Pref, 8342);
            containerDiv.Controls.Add(tablsText);
            header.Controls.Add(containerDiv);

            containerDiv = new HtmlGenericControl("div");
            tablsText = new HtmlGenericControl("div");
            tablsText.Attributes.Add("class", "tables_header_title ednKey");
            tablsText.InnerHtml = string.Format(eResApp.GetRes(Pref, 1913), this.MainTabImportInfo.GetLibelle());
            containerDiv.Controls.Add(tablsText);
            header.Controls.Add(containerDiv);

            HtmlGenericControl oblig = new HtmlGenericControl("div");
            oblig.Attributes.Add("class", "red");
            oblig.Attributes.Add("title", obligatoire);
            oblig.InnerHtml = string.Concat("* ", obligatoire);


            header.Controls.Add(oblig);

            return header;
        }


        /// <summary>
        /// Retourne le code HTML d'un onglet
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetTableOptions()
        {
            HtmlGenericControl tablsText = new HtmlGenericControl("div");
            tablsText.Attributes.Add("class", "options_header");

            tablsText.Controls.Add(GetOptionsDiv());
            tablsText.Controls.Add(GetTableKeysHeader());

            return tablsText;
        }

        /// <summary>
        /// Retourne le code html d'un Radio Button
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetOptionsDiv()
        {
            HtmlGenericControl tablsText = new HtmlGenericControl("div");
            tablsText.Attributes.Add("class", "optionsMapping");
            tablsText.Controls.Add(GetOptions(eResApp.GetRes(Pref, 8343), "rdbShow", true));
            tablsText.Controls.Add(GetOptions(eResApp.GetRes(Pref, 8344), "rdbHide"));
            tablsText.Controls.Add(GetOptions(eResApp.GetRes(Pref, 1914), "rdbHideFreeElement"));

            return tablsText;
        }

        /// <summary>
        /// Retourne la pagination des onglets
        /// </summary>
        /// <returns></returns>
        private HtmlGenericControl GetDivPaging()
        {

            HtmlGenericControl divPaging = new HtmlGenericControl("div");
            divPaging.ID = "divBkmPaging";
            divPaging.Attributes.Add("class", "divBkmPaging");



            this._nbPaging = Math.Ceiling(this._nbDisplayTab / this._maxTab);

            if (this._maxTab < this._nbDisplayTab)
            {
                for (int i = 0; i < this._nbPaging; i++)
                {

                    HtmlGenericControl spanPaging = new HtmlGenericControl("span");
                    spanPaging.ID = string.Concat("swImportPg", i.ToString());
                    spanPaging.Attributes.Add("tpage", (i + 1).ToString());
                    spanPaging.Attributes.Add("class", i == 0 ? "icon-circle imgActn" : "icon-circle-thin imgInact");
                    spanPaging.Attributes.Add("onclick", string.Concat("oImportWizardInternal.SwitchActivePage(", (i + 1).ToString(), ")"));
                    spanPaging.Attributes.Add("ondragover", string.Concat("oImportWizardInternal.SwitchActivePage(", (i + 1).ToString(), ")"));
                    divPaging.Controls.Add(spanPaging);
                }

            }


            return divPaging;
        }


        /// <summary>
        /// Récupérer les informations absentes du modèle d'import et existantes dans le fichier d'import
        /// </summary>
        private void CheckDataSourceForImportTemplateMapping()
        {
            if (this._importTemplate != null)
            {
                this._importTemplate.ImportTemplateLine.ImportTemplateOriginNotFoundList = new HashSet<string>();

                foreach (ImportTabParams table in this._importTemplate.ImportTemplateLine.ImportParams.Tables)
                {
                    for (int i = table.Mapp.Count - 1; i >= 0; i--)
                    {
                        ImportMapField map = table.Mapp[i];
                        if (!_headerLine.Columns.Any(e => e.SourceValue.ToLower().Equals(map.ColName.ToLower())))
                        {
                            if (!this._importTemplate.ImportTemplateLine.ImportTemplateOriginNotFoundList.Contains(map.ColName))
                            {
                                this._importTemplate.ImportTemplateLine.ImportTemplateOriginNotFoundList.Add(map.ColName);
                            }
                            table.Mapp.Remove(map);
                        }
                        else
                        {
                            int index = _headerLine.Columns.First(e => e.SourceValue.ToLower().Equals(map.ColName.ToLower())).ColumnIndex;
                            map.SetColIndex(index);
                        }
                    }

                }

            }

        }

        private HtmlGenericControl DrawFieldOptions(ImportInfo tab, Field field, ImportMapField mappedField)
        {
            using (HtmlGenericControl catalogueMenu = new HtmlGenericControl("div"))
            {
                bool merge = false, keep = false;
                if (mappedField != null && mappedField.Options != null)
                {
                    //Si catalogue multiple
                    if (field.Multiple)
                    {
                        IEnumerable<ImportFieldOptions> optionsMerge = mappedField.Options.Where(o => o.Name == ImportFieldOptionsType.MERGEDATA);
                        if (optionsMerge != null && optionsMerge.Any() && optionsMerge.First() != null)
                            merge = optionsMerge.First().Value;
                    }

                    //On ajoute l'option 'conserver la valeur si c'est vide'
                    IEnumerable<ImportFieldOptions> optionsKeep = mappedField.Options.Where(o => o.Name == ImportFieldOptionsType.KEEPDATA);
                    if (optionsKeep != null && optionsKeep.Any() && optionsKeep.First() != null)
                        keep = optionsKeep.First().Value;
                }

                catalogueMenu.Attributes.Add("class", "dropdown");
                catalogueMenu.Style.Add(HtmlTextWriterStyle.Display, mappedField != null ? "block" : "none");
                HtmlGenericControl btn = new HtmlGenericControl("button");
                btn.Attributes.Add("class", "dropbtn");
                //btn.Attributes.Add("onclick", "openMenu(this)");


                HtmlGenericControl ellipsis = new HtmlGenericControl("i");
                ellipsis.Attributes.Add("class", "icon-ellipsis-v");
                ellipsis.Attributes.Add("aria-hidden", "true");

                btn.Controls.Add(ellipsis);

                HtmlGenericControl dropDownContent = new HtmlGenericControl("div");
                dropDownContent.Attributes.Add("class", "dropdown-content");

                foreach (ImportFieldOptionsType option in Enum.GetValues(typeof(ImportFieldOptionsType)))
                    if (option != ImportFieldOptionsType.UNDEFINED)
                    {
                        if (!field.Multiple && option == ImportFieldOptionsType.MERGEDATA)
                            continue;
                        HtmlGenericControl div = GetOptionHtml(option, tab.GetJsKey(), field.Descid.ToString(), (option == ImportFieldOptionsType.MERGEDATA) ? merge : keep);
                        if (div.Controls.Count > 0)
                            dropDownContent.Controls.Add(div);
                    }

                catalogueMenu.Controls.Add(btn);
                catalogueMenu.Controls.Add(dropDownContent);


                return catalogueMenu;
            }
        }

        /// <summary>
        /// Retourne HTML d'une option
        /// </summary>
        /// <param name="optionType">Le type de l'option</param>
        /// <param name="tabId">DescId de le table</param>
        /// <param name="fieldDescid">DescId de la rubrique</param>
        /// <param name="check">L'option est cochée ou pas</param>
        /// <returns></returns>
        private HtmlGenericControl GetOptionHtml(ImportFieldOptionsType optionType, string tabId, string fieldDescid, bool check)
        {
            HtmlGenericControl div = new HtmlGenericControl("div");
            switch (optionType)
            {
                case ImportFieldOptionsType.MERGEDATA:
                    HtmlGenericControl aMerge = new HtmlGenericControl("a");
                    aMerge.Attributes.Add("class", "rChk");
                    aMerge.Attributes.Add("opt", "merge");
                    aMerge.Attributes.Add("align", "top");
                    aMerge.Attributes.Add("edndescid", fieldDescid);
                    aMerge.Attributes.Add("edntabdescid", tabId);
                    aMerge.Attributes.Add("href", "#");

                    HtmlGenericControl spanMerge = new HtmlGenericControl("span");
                    spanMerge.Attributes.Add("class", check ? "icon-check-square" : "icon-square-o");
                    aMerge.Controls.Add(spanMerge);
                    HtmlGenericControl libelleMerge = new HtmlGenericControl("span")
                    {
                        InnerText = eResApp.GetRes(Pref, 2418)
                    };
                    aMerge.Controls.Add(libelleMerge);
                    div.Controls.Add(aMerge);

                    break;

                case ImportFieldOptionsType.KEEPDATA:
                    HtmlGenericControl aKeep = new HtmlGenericControl("a");
                    aKeep.Attributes.Add("class", "rChk");
                    aKeep.Attributes.Add("align", "top");
                    aKeep.Attributes.Add("opt", "keep");
                    aKeep.Attributes.Add("edndescid", fieldDescid);
                    aKeep.Attributes.Add("edntabdescid", tabId);
                    aKeep.Attributes.Add("href", "#");

                    HtmlGenericControl spanKeep = new HtmlGenericControl("span");
                    spanKeep.Attributes.Add("class", check ? "icon-check-square" : "icon-square-o");
                    aKeep.Controls.Add(spanKeep);
                    HtmlGenericControl libelleKeep = new HtmlGenericControl("span")
                    {
                        InnerText = eResApp.GetRes(Pref, 2966)
                    };
                    aKeep.Controls.Add(libelleKeep);
                    div.Controls.Add(aKeep);

                    break;
                default:
                    break;
            }

            return div;
        }
    }
}
