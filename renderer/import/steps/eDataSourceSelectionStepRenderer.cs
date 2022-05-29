using Com.Eudonet.Internal;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using EudoExtendedClasses;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Core.Model;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.import
{
    /// <summary>
    /// class pour faire un rendu de l'étape de sélection de fichier
    /// </summary>
    public class eDataSourceSelectionStepRenderer : IWizardStepRenderer
    {
        private ePref _pref;
        private eImportWizardParam _wizrdParam;
        private ImportSpecification _specification;

        /// <summary>
        /// Retourne le renderer de la première étape: sélection de la source de donnée
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="wizrdParam">Paramètres du wizard</param>
        /// <param name="specification">paramètre de l'import</param>
        public eDataSourceSelectionStepRenderer(ePref pref, eImportWizardParam wizrdParam, ImportSpecification specification)
        {
            this._pref = pref;
            this._wizrdParam = wizrdParam;
            this._specification = specification;
        }

        /// <summary>
        /// Initialisation de l'etape
        /// </summary>
        /// <returns></returns>
        public IWizardStepRenderer Init() { return this; }

        /// <summary>
        /// Execute l'opération du rendu de l'étape
        /// </summary>
        /// <returns></returns>
        public Panel Render()
        {
            Panel stepContainer = CreateDataSourceStepContainer();

            RenderDataSourceOptionPart(stepContainer);

            RenderSeparatorPart(stepContainer);

            RenderTextQualifierPart(stepContainer);

            RenderHeaderLineOptionPart(stepContainer);

            return stepContainer;
        }

        /// <summary>
        /// Créer et initialise le conteneur de l'etape
        /// </summary>
        /// <returns></returns>
        private Panel CreateDataSourceStepContainer()
        {
            Panel stepContainer = new Panel();
            stepContainer.CssClass = "data-source-step";
            stepContainer.Style.Add(HtmlTextWriterStyle.Height, (_wizrdParam.Height - 220) + "px"); // 170px représente les étapes et les bouton
            return stepContainer;
        }

        /// <summary>
        /// Fait le rendu de la partie de choix de la source de données
        /// </summary>
        private void RenderDataSourceOptionPart(Panel stepContainer)
        {
            // Source de données cochée par défaut
            string dataSourceType = "file"; // "paste"
            //string dataSourceType = "paste"; 

            HtmlGenericControl lineContainer = new HtmlGenericControl("ul");
            lineContainer.ID = "DataSourceOption";
            lineContainer.Attributes.Add("source", dataSourceType);
            stepContainer.Controls.Add(lineContainer);

            // Choix du fichier - DataSourceType = "file"
            CreateLine(lineContainer, line =>
            {
                RadioButton rbFileSelection = new RadioButton();
                rbFileSelection.ID = "rbFileOption";
                rbFileSelection.GroupName = "DataSource";
                rbFileSelection.Checked = dataSourceType.Equals("file");
                rbFileSelection.Text = string.Concat(eResApp.GetRes(_pref, 1114), " (", ImportSpecification.FileFormats.Join(", "), ") ");

                rbFileSelection.Attributes.Add("onclick", "oImportWizardInternal.SetAttributeValueById('" + lineContainer.ID + "', 'source', 'file')");

                line.Controls.Add(rbFileSelection);
            });

            // Bouton choisissez le fichier dans un formulaire
            CreateLine(lineContainer, line =>
            {
                FileUpload fileUpload = new FileUpload();
                fileUpload.ID = "btnFileUpload";
                fileUpload.Style.Add(HtmlTextWriterStyle.Display, "none");
                fileUpload.Attributes.Add("onchange", "oImportWizardInternal.UpdateSelcetedFileLabel(this);");
                line.Controls.Add(fileUpload);

                //Demande #79 020: La langue des éléments <input> dont l'attribut type "file" dépend de celle du browser
                //Pour contourner ce problème on ajoute un bouton qui va être liée à l'ancien input
                Button btn = new Button();
                btn.UseSubmitBehavior = false;
                btn.Attributes.Add("class", "mapping_btn");
                btn.ID = "btn_import_file";
                btn.Text = eResApp.GetRes(_pref, 2567);
                btn.Attributes.Add("onclick", string.Concat("document.getElementById('btnFileUpload').click();return false;"));
                line.Controls.Add(btn);
                HtmlGenericControl modelName = new HtmlGenericControl("label");
                modelName.ID = "lbl_import_file";
                modelName.InnerHtml = eResApp.GetRes(_pref, 2568);
                modelName.Attributes.Add("class", "import_template_name");

                line.Controls.Add(modelName);
            }, new Dictionary<string, string>() { { "innerText", eResApp.GetRes(_pref, 588) } });

            // Choix copier/coller  - DataSourceType = "paste"
            CreateLine(lineContainer, line =>
            {
                RadioButton rbTextPaste = new RadioButton();
                rbTextPaste.ID = "rbPasteOption";
                rbTextPaste.GroupName = "DataSource";
                rbTextPaste.Checked = dataSourceType.Equals("paste"); ;
                rbTextPaste.Text = eResApp.GetRes(_pref, 6345);

                // Règle css affiche le textarea si l'attribut source contient "paste" comme valeur sinon le masque
                rbTextPaste.Attributes.Add("onclick", string.Concat("oImportWizardInternal.SetAttributeValueById('", lineContainer.ID, "', 'source', 'paste')"));

                line.Controls.Add(rbTextPaste);
            });

            // Choix copier/coller Text
            CreateLine(lineContainer, line =>
            {
                HtmlTextArea pasteArea = new HtmlTextArea();
                pasteArea.ID = "txtPasteArea";
#if DEBUG
                pasteArea.InnerText = string.Concat(
                    "Nom;Prènom;Civilité;Situation familiale;Appartient à",
                    Environment.NewLine,
                    "AZERTY;QWERTY;PROFESSEUR;Célibataire;", _pref.User.UserLogin);
#endif
                pasteArea.Style.Add(HtmlTextWriterStyle.Height, (_wizrdParam.Height - 470) + "px");
                line.Controls.Add(pasteArea);
            });
        }


        /// <summary>
        /// Fait le rendu de la partie de choix de séparateur de colonnes
        /// </summary>
        private void RenderSeparatorPart(Panel stepContainer)
        {
            HtmlGenericControl lineContainer = new HtmlGenericControl("ul");
            lineContainer.ID = "DataSeparatorOption";
            stepContainer.Controls.Add(lineContainer);

            // Liste des séparateurs
            CreateLine(lineContainer, line =>
            {
                Label separatorLabel = new Label();
                separatorLabel.Text = eResApp.GetRes(_pref, 6723);
                line.Controls.Add(separatorLabel);

                // Séparateurs
                DropDownList separatorList = new DropDownList();
                separatorList.ID = "separatorList";
                separatorList.Attributes.Add("onchange", "oImportWizardInternal.ShowHideOtherSeparator()");
                // Spec IEudoImportSpecification                
                separatorList.DataSource = ImportSpecification.Separators;
                separatorList.DataBind();
                //BSE:#62 771
                ListItem item = new ListItem(eResApp.GetRes(_pref, 75), "0");
                separatorList.Items.Add(item);
                ListItem defaultSep = separatorList.Items.FindByText(ImportSpecification.DefaultTextSeparator);
                defaultSep.Selected = true;
                // Autre type de séparatur de text
                HtmlInputText otherSeparator = new HtmlInputText();
                otherSeparator.ID = "otherSeparator";
                otherSeparator.Attributes.Add("maxlength", "1");
                otherSeparator.Style.Add(HtmlTextWriterStyle.Display, "none");
                line.Controls.Add(separatorList);
                line.Controls.Add(otherSeparator);
            });

        }

        /// <summary>
        /// Fait le rendu de la partie de choix de l'identificateur de texte
        /// </summary>
        private void RenderTextQualifierPart(Panel stepContainer)
        {
            HtmlGenericControl lineContainer = new HtmlGenericControl("ul");
            lineContainer.ID = "DataTextQualifierOption";
            stepContainer.Controls.Add(lineContainer);

            // Identificateur de text
            CreateLine(lineContainer, line =>
            {
                Label separatorLabel = new Label();
                separatorLabel.Text = eResApp.GetRes(_pref, 1665);
                line.Controls.Add(separatorLabel);

                // identificateur
                HtmlInputText textQualifier = new HtmlInputText();
                textQualifier.ID = "textQualifier";
                textQualifier.Value = ImportSpecification.DefaultTextQualifier;
                line.Controls.Add(textQualifier);
            });

        }

        /// <summary>
        /// Fait le rendu de la partie sur l'êntete de la source de donnée
        /// </summary>
        private void RenderHeaderLineOptionPart(Panel stepContainer)
        {
            HtmlGenericControl lineContainer = new HtmlGenericControl("ul");
            stepContainer.Controls.Add(lineContainer);

            // Entête de la source
            CreateLine(lineContainer, line =>
            {
                CheckBox headerLine = new CheckBox();
                headerLine.ID = "firstLineHeader";
                headerLine.Checked = true;
                headerLine.Text = eResApp.GetRes(_pref, 1666);
                line.Controls.Add(headerLine);
            });
        }

        /// <summary>
        /// Fonction utilitaire pour ajouter des contenu à la ligne
        /// </summary>
        /// <param name="ul">Conteneur de lignes</param>
        /// <param name="AddLiContent">Fonction qui ajoute du contenu à  la nouvelle ligne créée</param>
        /// <param name="lstAttr">Attributs supplémentaires</param>
        public void CreateLine(HtmlGenericControl ul, Action<HtmlGenericControl> AddLiContent, Dictionary<string, string> lstAttr = null)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            if (lstAttr != null && lstAttr.Count > 0)
                foreach (var attr in lstAttr)
                    li.Attributes.Add(attr.Key, attr.Value);

            ul.Controls.Add(li);
            AddLiContent(li);
        }
    }
}
