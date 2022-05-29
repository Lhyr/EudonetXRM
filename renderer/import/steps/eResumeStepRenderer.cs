
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// Class pour faire un rendu de l'étape du récapitulatif
    /// </summary>
    public class eResumeStepRenderer : eComunImportRenderer
    {
        /// <summary>
        /// Etape qui permer de choisir les options à appliquer sur une table
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="wizardParam">Paramètres de parsing du fichier d'import</param>
        public eResumeStepRenderer(ePref pref, eImportWizardParam wizardParam) : base(pref, wizardParam)
        {
        }

        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public override IWizardStepRenderer Init() { return this; }

        /// <summary>
        /// Execute l'opération du rendu
        /// </summary>
        /// <returns></returns>
        public override Panel Render()
        {
            IEnumerable<Int32> tabs = this.ListTab;
            Panel ctn = new Panel();

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "data-source-step");
            //Ajouter le nom du modèle d'import s'il existe
            GetTemplateName(container);
            container.Controls.Add(GetHeaderTabLine(eResApp.GetRes(Pref, 8348)));

            foreach (Cache.ImportInfo tab in this.ListImportInfo)
            {
                HtmlGenericControl renderer = GetOptionsTabLine(tab);
                if (renderer != null)
                    container.Controls.Add(renderer);
            }

            ctn.Controls.Add(container);

            HtmlGenericControl checkboxContainer = new HtmlGenericControl("div");


            ImportTemplateWizard importTemplate = GetImportTemplate(WizardParam.ImportTemplateParams.ImportTemplateId);

            //creation checkbox
            checkboxContainer.Attributes.Add("class", "data-source-step");
            CheckBox headerLine = new CheckBox();
            headerLine.ID = "trigger-automations";

            if (importTemplate != null && importTemplate.ImportTemplateLine.ImportParams.DisableAutomatismsORM && WizardParam.ImportTemplateParams.ImportTemplateId != 0)
                headerLine.Checked = true;

            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                headerLine.Attributes.Add("style", "display:none;");

            headerLine.Text = eResApp.GetRes(_pref, 2963);
            checkboxContainer.Controls.Add(headerLine);

            if (_pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN && WizardParam.ImportTemplateParams.ImportTemplateId != 0 && importTemplate != null)
            {

                if (importTemplate.ImportTemplateLine.ImportParams.DisableAutomatismsORM)
                {
                    checkboxContainer.Attributes.Add("class", "data-source-step alloworm");
                    HtmlGenericControl messagegauche = new HtmlGenericControl("div");
                    messagegauche.Attributes.Add("class", "block-orange");
                    checkboxContainer.Controls.Add(messagegauche);

                    HtmlGenericControl messagedroite = new HtmlGenericControl("div");
                    messagedroite.Attributes.Add("style", "padding-left: 10px;");

                    HtmlGenericControl warninginput = new HtmlGenericControl("Label");
                    warninginput.InnerText = "Remarque";
                    warninginput.Attributes.Add("class", "label-remarque");
                    messagedroite.Controls.Add(warninginput);

                    HtmlGenericControl input = new HtmlGenericControl("Label");
                    input.InnerText = eResApp.GetRes(_pref, 2964);
                    messagedroite.Controls.Add(input);

                    checkboxContainer.Controls.Add(messagedroite);
                }

            }

            ctn.Controls.Add(checkboxContainer);

            return ctn;
        }


        /// <summary>
        /// Permet de générer le code html d'une ligne par table Mappée
        /// </summary>
        /// <param name="table">La table Mappée</param>
        /// <param name="cssName">le font icon de la table Mappée</param>
        /// <returns></returns>
        public override HtmlGenericControl GetOptionsLine(Cache.ImportInfo table, string cssName)
        {
            IEnumerable<ImportTabParams> importTemplateParams;
            if (WizardParam.ImportTemplateParams.ImportTemplateId > 0)
                importTemplateParams = WizardParam.ImportTemplateParams.ImportParams.Tables.Where(t => t.TabInfo.TabInfoId == table.GetJsKey());

            bool bContainTabKey = WizardParam.ImportParams.Tables.Where(t => t.IsKey).Count() > 0;
            bool bMainTab = table.GetJsKey() == MainTabImportInfo.GetJsKey();
            //BSE:#66 310 + #67 965 => Aficher l'onglet adresse si 
            //On a mappé adresse
            //On a mappé ou pas adresse et: On hérite du PP ou PM ou les 2 de la table parente 
            //On a mappé ou pas adresse et: On hérite ni de PP ni de PM de la table parente et on a mappé PP et PM liés au signet
            bool bAdrAdd = table.Info.Table.DescId == (int)TableType.ADR && BaddAdrTAb();

            //Paramètres d'import de la table en cours
            IEnumerable<ImportTabParams> importParams = WizardParam.ImportParams.Tables.Where(t => t.TabInfo.TabInfoId == table.GetJsKey());

            //Si on a des paramètres pour cette table ou si c'est une table adresse spécifique, on continue
            if (importParams != null && (importParams.Count() == 1 || bAdrAdd))
            {
                HtmlGenericControl header = GetHtmlHeader(table, cssName);
                HtmlGenericControl headerTab = GetTableHeader(table);

                bool bBlamedPP = table.Info.Table.DescId == (int)TableType.ADR && BlamedTab != null && BlamedTab.Contains(((int)TableType.PP).ToString());
                bool bBlamedPM = table.Info.Table.DescId == (int)TableType.ADR && BlamedTab != null && BlamedTab.Contains(((int)TableType.PM).ToString());
                //Ajouter ou pas la ddl de creation de la fiche
                bool addCreateFile = (importParams != null && importParams.Count() > 0 && importParams.First() != null && importParams.First().Mapp.Count > 0) || bAdrAdd;

                //Ajouter ou pas la ddl de mise à jour de la fiche
                bool addUpdateFile = addCreateFile && (importParams != null && importParams.Count() > 0 && importParams.First() != null && importParams.First().Keys.Count() > 0 || (bContainTabKey && bMainTab));

                if (addCreateFile)
                {
                    //Charger les rubriques obligatoires d'une table et vérifier si elles sont toutes mappées=> dans le cas contraire, on ajoute pas la ddl de creation de fiche
                    foreach (Field f in table.Info.ObligateFields)
                    {
                        if (importParams.Count() > 0 && importParams.First().Mapp.Where(m => m.Field == f.Descid).Count() == 0)
                        {
                            IEnumerable<ImportTabParams> lImport = WizardParam.ImportParams.Tables.Where(t => t.TabInfo.IsRelation && t.TabInfo.RelationField == f.Descid && t.Mapp.Count > 0);
                            if (lImport.Count() > 0)
                            {
                                IEnumerable<Cache.ImportInfo> relationTab = this.ListImportInfo.Where(t => t.linkField != null && t.linkField.Descid == f.Descid);
                                if (relationTab.Count() > 0)
                                {
                                    IEnumerable<Field> relationTabObligField = relationTab.First().Info.Fields.Where(ff => ff.Obligat);
                                    foreach (Field field in relationTabObligField)
                                    {
                                        if (lImport.First().Mapp.Where(m => m.Field == field.Descid).Count() == 0)
                                        {
                                            addCreateFile = false;
                                            break;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                addCreateFile = false;
                                break;
                            }

                        }
                    }
                }

                //Les différents cas 'affichages des onglets à l'étape 2 de l'assistant: cas spécifiques pour ADR/PP/PM et table main
                if (bBlamedPP || (bBlamedPM && importParams.Count() == 0))
                {
                    headerTab.Controls.Add(GetAddFileTxtContainer(eResApp.GetRes(Pref, 1926)));//Aucune opération n’est disponible pour cet onglet.
                    BlamedTab.Add(table.GetJsKey());
                }
                else if (bAdrAdd && (importParams.Count() == 0 || (importParams.Count() == 1 && importParams.First().Mapp.Count == 0)))
                {
                    if (importParams.Count() == 1 && importParams.First() != null)
                    {
                        headerTab.Controls.Add(GetFirstSelect(table, eResApp.GetRes(Pref, 8345), importParams.First()));
                        headerTab.Controls.Add(GetSecondSelect(table, eResApp.GetRes(Pref, 8346), importParams.First(), addUpdateFile: addUpdateFile));
                    }
                    else
                    {
                        headerTab.Controls.Add(GetFirstSelect(table, eResApp.GetRes(Pref, 8345), null, bAddAdr: true));
                        headerTab.Controls.Add(GetSecondSelect(table, eResApp.GetRes(Pref, 8346), null, bAddAdr: true));
                    }
                }
                else if (importParams.Count() == 1 && importParams.First() != null && !addCreateFile && !addUpdateFile)
                {
                    headerTab.Controls.Add(GetAddFileTxtContainer(eResApp.GetRes(Pref, 1926)));//Aucune opération n’est disponible pour cet onglet.
                    BlamedTab.Add(table.GetJsKey());
                }
                else
                {
                    if (!table.Info.ImportRight)
                        headerTab.Controls.Add(GetAddFileTxtContainer(eResApp.GetRes(Pref, 8445)));//La création ou la modification de fiches ne sont pas autorisées.
                    //BSE:#67 440 si au moin un ongle est utilisé comme clé de dédoublonnage pour la table principale et qu'on est sur la table principale, on autorise la mise à jour.
                    else if (bContainTabKey && bMainTab)
                    {
                        if (importParams.Count() > 0 && importParams.First().Keys.Count() > 0)
                        {
                            headerTab.Controls.Add(GetFirstSelect(table, eResApp.GetRes(Pref, 8345), importParams.First(), addCreateFile: addCreateFile));
                            headerTab.Controls.Add(GetSecondSelect(table, eResApp.GetRes(Pref, 8346), importParams.First(), addUpdateFile: addUpdateFile));//Si la fiche existe     
                        }
                        else
                        {
                            headerTab.Controls.Add(GetAddFileTxtContainer(eResApp.GetRes(Pref, 1926)));//Aucune opération n’est disponible pour cet onglet.
                            BlamedTab.Add(table.GetJsKey());
                        }


                    }
                    else
                    {

                        if (!addUpdateFile && addCreateFile)
                        {
                            headerTab.Controls.Add(GetAddFileTxtContainer(eResApp.GetRes(Pref, table.Info.Table.DescId == (int)TableType.ADR ? 2066 : 8347)));//Aucune clé n'a été définie. Toutes les informations du fichier source seront créées.
                            header.Attributes.Add("ednCreate", "1");
                        }
                        else
                        {
                            headerTab.Controls.Add(GetFirstSelect(table, eResApp.GetRes(Pref, 8345), importParams.First(), addCreateFile: addCreateFile));
                            headerTab.Controls.Add(GetSecondSelect(table, eResApp.GetRes(Pref, 8346), importParams.First(), addUpdateFile: addUpdateFile));//Si la fiche existe

                        }

                    }
                }

                //BSE: #62 851
                //Ajout de la case à cocher pour utiliser un champ relationnel ou une relation principale comme clé de dédoublonnage s'il y'a au moins une rubrique identifiée comme clé pour la table.
                //BSE:#67 439 clé de dédoublonnage pour les champs relationnels est géré à l'étape du mapping
                //GetIsKeyTabHtmlCode(table, importParams.First(), ref headerTab);
                header.Controls.Add(headerTab);
                return header;
            }
            else
                return null;

        }


        /// <summary>
        /// Retourne le code HTML du conteneur deu rendu de la table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public HtmlGenericControl GetTableHeader(Cache.ImportInfo table)
        {
            HtmlGenericControl headerTab = new HtmlGenericControl();
            headerTab.Attributes.Add("class", "divTabOptions");
            headerTab.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);
            headerTab.InnerHtml = table.GetLibelle();

            return headerTab;
        }

        /// <summary>
        /// Retourne le code HTML du conteneur deu rendu de la table
        /// </summary>
        /// <param name="table"></param>
        /// <param name="cssName"></param>
        /// <returns></returns>
        public HtmlGenericControl GetHtmlHeader(Cache.ImportInfo table, string cssName)
        {
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "headerOptionsLine");
            header.Attributes.Add("edntabid", table.GetJsKey());

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", string.Concat(cssName, " iconHeaderOptions"));
            icon.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);

            header.Controls.Add(icon);

            HtmlGenericControl headerTab = new HtmlGenericControl();
            headerTab.Attributes.Add("class", "divTabOptions");
            headerTab.Style.Add(HtmlTextWriterStyle.Color, Pref.ThemeXRM.Color);
            headerTab.InnerHtml = table.GetLibelle();

            return header;
        }

        /// <summary>
        /// Retourne une liste déroulante à 2 choix : Création / pas de création de la fiche
        /// </summary>
        /// <param name="table">Table Mappée</param>
        /// <param name="txt">Ressource à affciher</param>
        /// <param name="importParams">Paramètre d'import de la table</param>
        /// <param name="addCreateFile">Indique si on autorise la création d'une fiche</param>
        /// <param name="bAddAdr">Cas spécifique pour Adresse si l'onglet principale est lié à PP et PM</param>
        /// <returns></returns>
        private HtmlGenericControl GetFirstSelect(Cache.ImportInfo table, string txt, ImportTabParams importParams = null, bool addCreateFile = true, bool bAddAdr = false)
        {

            HtmlGenericControl globalOptionsLine = new HtmlGenericControl("div");
            globalOptionsLine.Attributes.Add("class", "optionsClass");


            HtmlGenericControl optionsLine = new HtmlGenericControl("div");
            optionsLine.Attributes.Add("class", "optionsLine");


            optionsLine.InnerHtml = txt;

            HtmlGenericControl select = new HtmlGenericControl("select");
            select.ID = string.Concat("nexist_", table.GetJsKey());
            select.Attributes.Add("class", "selectClass");

            HtmlGenericControl option;
            if (addCreateFile || (importParams == null && bAddAdr))
            {
                option = new HtmlGenericControl("option");
                option.InnerHtml = eResApp.GetRes(Pref, 8400);
                if (bAddAdr || (importParams != null && importParams.Create))
                    option.Attributes.Add("selected", "selected");
                option.Attributes.Add("value", "1");
                select.Controls.Add(option);
            }


            option = new HtmlGenericControl("option");
            option.InnerHtml = eResApp.GetRes(Pref, 8402);
            option.Attributes.Add("value", "2");
            if ((!addCreateFile && !bAddAdr) || (importParams != null && !importParams.Create))
                option.Attributes.Add("selected", "selected");

            select.Controls.Add(option);



            globalOptionsLine.Controls.Add(optionsLine);
            globalOptionsLine.Controls.Add(select);
            return globalOptionsLine;
        }


        /// <summary>
        /// Retourne une liste déroulante à 2 choix : Mise à jours / pas de Mise à jours de la fiche
        /// </summary>
        /// <param name="table">Table Mappée</param>
        /// <param name="txt">Ressource à affciher</param>
        /// <param name="importTabParams">Paramètres d'import de la table</param>
        /// <param name="addUpdateFile">Autoriser la mise à jour</param>
        /// <param name="bAddAdr">Cas spécifique pour Adresse si l'onglet principale est lié à PP et PM</param>
        /// <returns></returns>
        private HtmlGenericControl GetSecondSelect(Cache.ImportInfo table, string txt, ImportTabParams importTabParams = null, bool addUpdateFile = true, bool bAddAdr = false)
        {
            HtmlGenericControl globalOptionsLine = new HtmlGenericControl("div");
            globalOptionsLine.Attributes.Add("class", "optionsClass");

            HtmlGenericControl optionsLine = new HtmlGenericControl("div");
            optionsLine.Attributes.Add("class", "optionsLine");
            optionsLine.InnerHtml = txt;

            HtmlGenericControl select = new HtmlGenericControl("select");
            select.ID = string.Concat("exist_", table.GetJsKey());
            select.Attributes.Add("class", "selectClass");

            HtmlGenericControl option;
            if (addUpdateFile || (importTabParams == null && bAddAdr))
            {
                option = new HtmlGenericControl("option");
                option.InnerHtml = eResApp.GetRes(Pref, 8401);
                if (importTabParams == null || (importTabParams != null && importTabParams.Update))
                    option.Attributes.Add("selected", "selected");
                option.Attributes.Add("value", "1");
                select.Controls.Add(option);

            }

            option = new HtmlGenericControl("option");
            option.InnerHtml = eResApp.GetRes(Pref, 8403);
            option.Attributes.Add("value", "2");
            if ((!addUpdateFile && !bAddAdr) || (importTabParams != null && !importTabParams.Update))
                option.Attributes.Add("selected", "selected");
            select.Controls.Add(option);

            globalOptionsLine.Controls.Add(optionsLine);
            globalOptionsLine.Controls.Add(select);
            return globalOptionsLine;
        }

        /// <summary>
        /// Retourne la case à cocher pour permettre de définir la table relationnelle comme clé de dédoublonnage
        /// </summary>
        /// <param name="table">Table relationnelle</param>
        /// <param name="importParam">Table relationnelle</param>
        /// <param name="header">Container HTML</param>
        /// <returns></returns>
        private void GetIsKeyTabHtmlCode(Cache.ImportInfo table, ImportTabParams importParam, ref HtmlGenericControl header)
        {
            bool addCheckBox = this.MainTabImportInfo != null
                && (
                    (this.MainTabImportInfo.Info.Table.InterPM && importParam.TabInfo.TabDescId == (int)TableType.PM)
                    || (this.MainTabImportInfo.Info.Table.InterPP && importParam.TabInfo.TabDescId == (int)TableType.PP)
                    || (this.MainTabImportInfo.Info.Table.InterEVT && importParam.TabInfo.TabDescId == this.MainTabImportInfo.Info.Table.InterEVTDescid)
                    || importParam.TabInfo.IsRelation
                   )
                   && importParam.Keys.Count() > 0
                ;

            if (addCheckBox)
            {
                HtmlGenericControl globalOptionsLine = new HtmlGenericControl("div");
                globalOptionsLine.Attributes.Add("class", "optionsClass");

                HtmlGenericControl optionsLine = new HtmlGenericControl("div");
                optionsLine.Attributes.Add("class", "optionsKeyLine");
                optionsLine.InnerHtml = string.Format(eResApp.GetRes(Pref, 8737), this.MainTabImportInfo.GetLibelle());
                eCheckBoxCtrl checkBox = GetCheckBoxCode(table.GetJsKey(), importParam.IsKey);
                optionsLine.Controls.Add(checkBox);
                globalOptionsLine.Controls.Add(optionsLine);
                header.Controls.Add(globalOptionsLine);
            }

        }

        /// <summary>
        /// Retourne code HTML d'une case à cocher
        /// </summary>
        /// <param name="id">Id de la case à cocher</param>
        /// <param name="bChecked">Cocher/Pas cocher</param>
        /// <returns></returns>
        private eCheckBoxCtrl GetCheckBoxCode(string id, bool bChecked)
        {
            eCheckBoxCtrl checkbox = new eCheckBoxCtrl(bChecked, false);
            checkbox.AddClass("chkAction");
            checkbox.AddClass("TVChk");
            checkbox.ID = String.Concat("chkValue_", id);
            checkbox.Attributes.Add("name", "chkValue");
            checkbox.AddClick(String.Empty);
            checkbox.ToolTip = string.Format(eResApp.GetRes(Pref, 8737), this.MainTabImportInfo.GetLibelle());
            checkbox.ToolTipChkBox = string.Format(eResApp.GetRes(Pref, 8737), this.MainTabImportInfo.GetLibelle());

            return checkbox;
        }

        /// <summary>
        /// Retourne le code HTML d'une div avec un innerHtml passé en paramètre
        /// </summary>
        /// <param name="msg">InnerHtml à afficher dans la DIV</param>
        /// <returns></returns>
        private HtmlGenericControl GetAddFileTxtContainer(string msg)
        {
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "optionsClass");
            header.InnerHtml = msg;

            return header;

        }

        enum TYPE_RDB
        {
            OPT_EXEC = 1,
            OPT_TAB = 2
        }

    }
}
