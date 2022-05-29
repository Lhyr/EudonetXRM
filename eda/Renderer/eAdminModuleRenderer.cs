using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de l'administration des onglets
    /// </summary>
    public class eAdminModuleRenderer : eAdminRenderer
    {
        public const string DEFAULT_OPTION_ONCHANGE = "nsAdmin.sendJson(this, false, true);"; // this : contrôle (DOM), false = confirmation de la modification, true = pas de vérification du DescID

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        protected eAdminModuleRenderer(ePref pref)
            : base()
        {
            Pref = pref;
        }


        public static eAdminModuleRenderer CreateAdminModuleRenderer(ePref pref)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminModuleRenderer(pref);
        }

        /// <summary>
        /// Crée un conteneur de section pour module d'administration, repliable, avec un ID, un titre et une ancre correspondant à l'ID.
        /// </summary>
        /// <param name="id">Identifiant du conteneur. Sera également utilisé comme cible pour l'ancre générée près du titre pour être atteint via <a href="#id"/></param>
        /// <param name="label">Libellé de la section à afficher comme titre</param>
        /// <param name="number">Si souhaité, ajoute un numéro à la section. Si la valeur est inférieure à 1, le numéro est ignoré</param>
        /// <param name="bCollapsable">Indique si l'entrée de module doit pouvoir être replié</param>
        /// <returns>Renvoie le contrôle généré, de façon à pouvoir y ajouter des contrôles enfants via Controls.Add</returns>
        public static Panel GetModuleSection(string id, string label, int number = 0, bool bCollapsable = true)
        {
            Panel sectionContainer = new Panel();
            HyperLink sectionAnchor = new HyperLink();
            Panel sectionTitleContainer = new Panel();
            HtmlGenericControl sectionTitle = new HtmlGenericControl();
            HtmlGenericControl sectionIcon = new HtmlGenericControl();
            Panel sectionContentsContainer = new Panel();

            sectionContainer.Attributes.Add("class", "divStep");

            sectionAnchor.Attributes.Add("name", id);

            sectionTitleContainer.ID = id;
            sectionTitleContainer.Attributes.Add("class", "paramStep active" + (bCollapsable ? "" : " paramStepDefPointer")); // TODO: ajout d'active sous condition ?

            sectionIcon.Attributes.Add("class", "icon-unvelop");

            sectionTitle.Attributes.Add("class", "stepTitle");
            sectionTitle.InnerText = label;
            //sectionTitle.Attributes.Add("onclick", String.Concat("nsAdmin.hideShowSection('", id.Replace("'", "\\'"), "');"));

            sectionContentsContainer.Attributes.Add("class", "stepContent");
            sectionContentsContainer.ID = "stepContent_" + id;
            sectionContentsContainer.Attributes.Add("data-active", "1");

            if (number > 0)
            {
                HtmlGenericControl sectionNum = new HtmlGenericControl("span");
                sectionTitleContainer.Controls.Add(sectionNum);
            }

            if (bCollapsable)
                sectionTitleContainer.Controls.Add(sectionIcon);

            sectionTitleContainer.Controls.Add(sectionTitle);

            sectionContainer.Controls.Add(sectionAnchor);
            sectionContainer.Controls.Add(sectionTitleContainer);
            sectionContainer.Controls.Add(sectionContentsContainer);

            return sectionContainer;
        }

        /// <summary>
        /// Retourne le panel du contenu de la section
        /// </summary>
        /// <param name="section">Section</param>
        /// <returns></returns>
        public static Panel GetSectionContentPanel(Panel section)
        {
            Panel targetPanel = null;
            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];

            return targetPanel;
        }

        /// <summary>
        /// Crée un contrôle lien (<a/>) avec un ID, une icône, une cible JavaScript, et un libellé
        /// </summary>
        /// <param name="id">Identifiant (DOM) du lien</param>
        /// <param name="jsOnClick">Code JavaScript à exécuter au lien. Sera automatiquement préfixé de "javascript:"</param>
        /// <param name="label">Libellé du lien. Code HTML accepté. Sera également ajouté en info-bulle de l'icône et du lien</param>
        /// <param name="tooltip">Info-bulle du lien et de l'icône. Si vide, le libellé sera utilisé</param>
        /// <param name="iconClass">Classe CSS de l'icône à afficher, sans le préfixe "icon-" (qui sera automatiquement ajouté). Si vide, aucune icône n'est ajoutée</param> 
        /// <returns></returns>
        public static HtmlGenericControl GetLink(string id, string jsOnClick, string label, string tooltip = "", string iconClass = "")
        {
            HtmlGenericControl aLink = new HtmlGenericControl("a");
            HtmlGenericControl aLinkSpan = new HtmlGenericControl("span");

            if (String.IsNullOrEmpty(tooltip))
                tooltip = label;

            aLink.ID = id;
            aLink.Attributes.Add("onclick", String.Concat("javascript:", jsOnClick));
            aLink.Attributes.Add("title", tooltip);
            aLink.InnerHtml = label;

            if (!String.IsNullOrEmpty(iconClass))
            {
                aLink.InnerHtml = String.Concat("&nbsp;", label);
                aLinkSpan.Attributes.Add("class", String.Concat("icon-", iconClass));
                aLinkSpan.Attributes.Add("title", tooltip);
                aLinkSpan.Attributes.Add("alt", tooltip);
                aLink.Controls.AddAt(0, aLinkSpan);
            }

            return aLink;
        }

        /// <summary>
        ///  Génère et retourne un contrôle tableau HTML (HtmlTable) contenant les données passées en paramètre
        /// </summary>
        /// <param name="tableId">Identifiant (DOM) du tableau HTML</param>
        /// <param name="columnHeaders">Liste de libellés à ajouter en tant qu'entêtes de colonnes</param>
        /// <param name="rowCellLabels">Liste de liste de libellés pour la création des cellules de données, sous la forme [Ligne][Colonne] = texte de la cellule</param>
        /// <param name="rowCellLinks">Liste de liste de liens à ajouter aux cellules de données, sous la forme [Ligne][Colonne] = lien de la cellule</param>
        /// <param name="rowCellTooltips">Liste de liste d'infobulles à ajouter aux cellules de données, sous la forme [Ligne][Colonne] = infobulle de la cellule</param>
        /// <param name="rowCellAttributes">Liste de liste de collections d'attributs HTML additionnels à ajouter aux cellules de données, sous la forme [Ligne][Colonne] = collection d'attributs de la cellule</param>
        /// <param name="rowLinks">Liste de liens à ajouter sur la globalité de chaque ligne, sous la forme [Ligne] = lien global de la ligne. Peuvent être surchargés par les liens de cellules</param>
        /// <param name="rowTooltips">Liste d'infobulles à ajouter sur la globalité de chaque ligne, sous la forme [Ligne] = infobulle globale de la ligne. Peuvent être surchargés par les infobulles de cellules</param>
        /// <param name="rowAttributes">Liste de collections d'attributs HTML à ajouter sur la globalité de chaque ligne, sous la forme [Ligne] = collection d'attributs globaux de la ligne. Peuvent être surchargés par les collections d'attributs de cellules</param>
        /// <param name="tableAdditionalClass">Classe(s) CSS additionnelles à ajouter au tableau HTML</param>
        /// <param name="encodeRowCellLabels">Indique si les libellés contenus dans rowCellLabels doivent être insérés en tant que texte (HTML encodé = true) ou HTML (non encodé = false)</param>
        /// <returns>Le tableau HTML intégralement généré</returns>
        public static HtmlTable GetTable(
            string tableId,
            List<string> columnHeaders,
            List<List<string>> rowCellLabels, List<List<string>> rowCellLinks, List<List<string>> rowCellTooltips, List<List<AttributeCollection>> rowCellAttributes,
            List<string> rowLinks, List<string> rowTooltips, List<AttributeCollection> rowAttributes,
            string tableAdditionalClass = "", bool encodeRowCellLabels = true
        )
        {
            HtmlTable table = new HtmlTable();

            // ID et classe de la table
            table.ID = tableId;
            string tableClass = "mTab";
            if (!String.IsNullOrEmpty(tableAdditionalClass))
                tableClass = String.Concat(tableClass, " ", tableAdditionalClass);
            table.Attributes.Add("class", tableClass);

            // Ligne d'entête
            HtmlTableRow tableHeaderRow = new HtmlTableRow();
            tableHeaderRow.Attributes.Add("class", "hdBgCol");
            foreach (string column in columnHeaders)
            {
                HtmlTableCell tableHeaderCell = new HtmlTableCell("th");
                tableHeaderCell.InnerText = column;
                tableHeaderRow.Cells.Add(tableHeaderCell);
            }
            table.Rows.Add(tableHeaderRow);

            // Lignes de données
            int r = -1;
            foreach (List<string> rowLabels in rowCellLabels)
            {
                r++;

                HtmlTableRow tableRow = new HtmlTableRow();

                // CLasse permettant d'alterner les couleurs de chaque ligne
                tableRow.Attributes.Add("class", (r % 2 == 0) ? "line1" : "line2");

                // Info-bulle de la ligne
                string rowTooltip = String.Empty;
                if (rowTooltips != null && rowTooltips.Count > r)
                    rowTooltip = rowTooltips[r];
                if (!String.IsNullOrEmpty(rowTooltip))
                    tableRow.Attributes.Add("title", rowTooltip);

                // Lien de la ligne
                string rowLink = String.Empty;
                if (rowLinks != null && rowLinks.Count > r)
                    rowLink = rowLinks[r];
                if (!String.IsNullOrEmpty(rowLink))
                {
                    tableRow.Attributes.Add("onclick", rowLink);
                    tableRow.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                }

                // Attributs de la ligne
                AttributeCollection rowAttributeCollection = null;
                if (rowAttributes != null && rowAttributes.Count > r)
                    rowAttributeCollection = rowAttributes[r];
                if (rowAttributeCollection != null)
                    foreach (string attributeKey in rowAttributeCollection.Keys)
                        tableRow.Attributes.Add(attributeKey, rowAttributeCollection[attributeKey]);

                // Cellules de la ligne
                int c = -1;
                foreach (string label in rowLabels)
                {
                    c++;

                    HtmlTableCell tableCell = new HtmlTableCell();
                    tableCell.Attributes.Add("class", "cell");
                    tableRow.Cells.Add(tableCell);

                    // Libellé de la cellule : encodé (via InnerText) ou non (encapsulé dans un LiteralControl) selon paramètre
                    if (encodeRowCellLabels)
                        tableCell.InnerText = label;
                    else
                    {
                        Panel content = new Panel();
                        content.CssClass = "contentCellLAb";
                        content.Attributes.Add("data-label", label);
                        content.Controls.Add(new LiteralControl(label));
                        tableCell.Controls.Add(content);
                    }


                    // Info-bulle de la cellule
                    string cellTooltip = String.Empty;
                    if (rowCellTooltips != null && rowCellTooltips.Count > r && rowCellTooltips[r] != null && rowCellTooltips[r].Count > c)
                        cellTooltip = rowCellTooltips[r][c];
                    if (!String.IsNullOrEmpty(cellTooltip))
                        tableCell.Attributes.Add("title", cellTooltip);

                    // Lien de la cellule
                    string cellLink = String.Empty;
                    if (rowCellLinks != null && rowCellLinks.Count > r && rowCellLinks[r] != null && rowCellLinks[r].Count > c)
                        cellLink = rowCellLinks[r][c];
                    if (!String.IsNullOrEmpty(cellLink))
                    {
                        tableCell.Attributes.Add("onclick", cellLink);
                        tableCell.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                    }

                    // Attributs de la cellule
                    AttributeCollection cellAttributeCollection = null;
                    if (rowCellAttributes != null && rowCellAttributes.Count > r && rowCellAttributes[r] != null && rowCellAttributes[r].Count > c)
                        cellAttributeCollection = rowCellAttributes[r][c];
                    if (cellAttributeCollection != null)
                        foreach (string attributeKey in cellAttributeCollection.Keys)
                            tableCell.Attributes.Add(attributeKey, cellAttributeCollection[attributeKey]);
                }

                table.Rows.Add(tableRow);
            }

            return table;
        }

        /// <summary>
        /// Ajoute les icônes de tri sur les colonnes indiquées du tableau généré par GetTable()
        /// </summary>
        /// <param name="tableHeaderCell">Cellule d'entête sur laquelle rajouter le critère de tri</param>
        /// <param name="sortFunction">Fonction JavaScript à déclencher au clic sur l'icône de tri. Indiquer "SORTORDER" entre chevrons dans cette chaîne pour remplacer par l'ordre de tri (0 pour ascendant, 1 pour descendant)</param>
        public void AddSortIconsToTableHeader(HtmlTableCell tableHeaderCell, string sortFunction)
        {
            // Bouton Ascendant
            tableHeaderCell.Controls.Add(new LiteralControl(" "));
            HtmlImage imgIcn = new HtmlImage();
            tableHeaderCell.Controls.Add(imgIcn);
            imgIcn.Src = "ghost.gif";
            imgIcn.Attributes.Add("class", "rIco SortAsc");
            imgIcn.Attributes.Add("onclick", sortFunction.Replace("<SORTORDER>", "0"));

            tableHeaderCell.Controls.Add(new LiteralControl(" "));
            imgIcn = new HtmlImage();
            tableHeaderCell.Controls.Add(imgIcn);
            imgIcn.Src = "ghost.gif";
            imgIcn.Attributes.Add("class", "rIco SortDesc");
            imgIcn.Attributes.Add("onclick", sortFunction.Replace("<SORTORDER>", "1"));
        }

        public Panel GetCheckBox(String idCheckbox, String label, String onClick, Boolean bChecked, Boolean bDisabled, Control control)
        {
            return GetCheckBox(idCheckbox, label, onClick, bChecked, bDisabled, null, 0, control);
        }

        /// <summary>Création de la ligne de Checkbox</summary>
        /// <param name="idCheckbox">The identifier checkbox.</param>
        /// <param name="label">The label.</param>
        /// <param name="onClick">Script déclenché au clic sur la case à cocher (sans le préfixe javascript:)</param>
        /// <param name="bChecked">if set to <c>true</c> [b checked].</param>
        /// <param name="bDisabled">if set to <c>true</c> [b disabled].</param>
        /// <param name="control">Contrôle à ajouter</param>
        /// <returns></returns>
        public Panel GetCheckBox(String idCheckbox, String label, String onClick, Boolean bChecked, Boolean bDisabled, Type configKeyType, int configKey, Control control)
        {
            Panel panel = new Panel();
            panel.CssClass = "field";

            Panel chkField = new Panel();
            chkField.CssClass = "checkboxField";

            if (onClick.Length > 0)
                chkField.Attributes.Add("onclick", onClick);

            if (configKeyType != null)
            {
                chkField.Attributes.Add("cat", GetCatFromConfigKeyType(configKeyType).ToString());
                chkField.Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(configKeyType));
                chkField.Attributes.Add("prop", configKey.ToString());
            }

            eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(bChecked, bDisabled);
            chkCtrl.ID = idCheckbox;
            chkCtrl.AddClick();
            chkCtrl.AddText(label);

            chkField.Controls.Add(chkCtrl);

            panel.Controls.Add(chkField);

            if (control != null)
                panel.Controls.Add(control);

            return panel;
        }

        /// <summary>Création d'un couple libellé/contrôle</summary>
        /// <param name="label">Libellé</param>
        /// <param name="control">Contrôle à ajouter</param>
        /// <returns></returns>
        public Panel GetLabelField(String label, Control control)
        {
            Panel panel = new Panel();
            panel.CssClass = "field";

            Panel lblField = new Panel();
            lblField.CssClass = "labelField";

            LiteralControl lblControl = new LiteralControl(label);

            lblField.Controls.Add(lblControl);

            panel.Controls.Add(lblField);
            panel.Controls.Add(control);

            return panel;
        }

        public static Control AddTextboxOptionField(
            Panel targetPanel,
            string id, string label, string tooltip,
            eAdminUpdateProperty.CATEGORY propCat, int propKeyCode, Type propKeyType,
            string currentValue,
            AdminFieldType adminFieldType, eAdminTextboxField.LabelType labelType = eAdminTextboxField.LabelType.ABOVE,
            int nbRows = 0, string prefixText = "", string suffixText = "",
            bool passwordField = false,
            Dictionary<string, string> customTextboxStyleAttributes = null, string customTextboxCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "",
            string onChange = DEFAULT_OPTION_ONCHANGE, bool readOnly = false, eLibConst.CONFIGADV_CATEGORY configAdvCat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED, bool mandatory = false)
        {

            eAdminField txtField = new eAdminTextboxField(0, label, propCat, propKeyCode, adminFieldType, tooltip, currentValue, nbRows, customTextboxStyleAttributes, customTextboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses, prefixText, suffixText, labelType, passwordField, readOnly: readOnly, mandatory: mandatory);
            txtField.SetFieldControlID(id);
            if (configAdvCat != eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                txtField.SetConfigAdvCategory(configAdvCat);
            txtField.Generate(targetPanel);
            ((TextBox)txtField.FieldControl).Attributes.Add("onchange", onChange);
            ((TextBox)txtField.FieldControl).Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(propKeyType));
            // Sur les champs de type Mot de passe, la propriété Text n'est pas remplie par défaut pour des questions de sécurité
            // Il faut alors paramétrer l'attribut "value" en dur, mais le mot de passe apparaît alors en clair dans le code de la page.
            // http://stackoverflow.com/questions/16739473/textbox-using-textmode-password-not-showing-text-asp-net-c-sharp
            if (passwordField)
            {
                // HLA - Pour le cas du password, on genere un pass fictif à la longue de la vrai valeur pour indiquer qu'il y a deja un pass en base
                int passLen = currentValue?.Length ?? 0;
                string fictiveValue = passLen > 0 ? "".PadLeft(passLen, 'X') : "";

                ((TextBox)txtField.FieldControl).Attributes["value"] = fictiveValue;
                // Pour selectionner la valeur fictive au click sur la rubrique
                ((TextBox)txtField.FieldControl).Attributes.Add("onclick", "this.select();");
            }

            return txtField.FieldControl;
        }



        public Control AddDropdownOptionField(
            Panel targetPanel,
            string id, string label, string tooltip,
            eAdminUpdateProperty.CATEGORY propCat, int propKeyCode, Type propKeyType,
            List<ListItem> items, string selectedValue, FieldFormat valueFormat,
            eAdminDropdownField.eAdminDropdownFieldRenderType renderType,
            Dictionary<string, string> customDropdownStyleAttributes = null,
            string customDropdownCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null,
            string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null,
            string customLabelCSSClasses = "",
            string onChange = DEFAULT_OPTION_ONCHANGE, bool sortItemsByLabel = true, eLibConst.CONFIGADV_CATEGORY configAdvCat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED, bool mandatory = false)
        {
            eAdminDropdownField ddField = new eAdminDropdownField(0, label, propCat, propKeyCode, items.ToArray(), tooltip, selectedValue, valueFormat, renderType, customDropdownStyleAttributes, customDropdownCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses, sortItemsByLabel: sortItemsByLabel, mandatory: mandatory);
            ddField.SetFieldControlID(id);
            if (configAdvCat != eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                ddField.SetConfigAdvCategory(configAdvCat);
            ddField.Generate(targetPanel);
            ((DropDownList)ddField.FieldControl).Attributes.Add("onchange", onChange);
            ((DropDownList)ddField.FieldControl).Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(propKeyType));

            return ddField.FieldControl;
        }


        /// <summary>
        /// Ajoute une case à cocher "eAdminCheckboxField" (panel contenant une cac et un label) sur un panel et la retourne
        /// Utilisé pour mettre à jour une option de type BIT en admin
        /// </summary>
        /// <param name="targetPanel">Panel sur lequel la cac doit être ajouté</param>
        /// <param name="id">ID de la cac</param>
        /// <param name="label">Texte de la cac</param>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="propCat">Catégorie de l'option. Permet de déterminer la table à maj</param>
        /// <param name="propKeyCode">Code de l'option à maj</param>
        /// <param name="propKeyType">Type de l'enum de l'option à maj</param>
        /// <param name="selectedValue">Valeur de l'option</param>
        /// <param name="customCheckboxStyleAttributes">Dictionaire de style sur la checkbox</param>
        /// <param name="customCheckboxCSSClasses">Classe sur la check Box</param>
        /// <param name="customPanelStyleAttributes">Dictionaire de style sur le panel eAdminCheckboxField </param>
        /// <param name="customPanelCSSClasses">Classe ur le panel eAdminCheckboxField</param>
        /// <param name="customLabelStyleAttributes">Dictionaire de style sur le label de la checkbox</param>
        /// <param name="customLabelCSSClasses">Classe sur le label de la checkbox</param>
        /// <param name="onClick">Action sur le click de l'option</param>
        /// <param name="swap">Invers text/libellé pour case à cocher</param>
        /// <param name="configAdvCat">catégorie de la propriété</param>
        /// <param name="readOnly">En lecture seule</param>
        /// /// <returns>eAdminCheckboxField ajouté au targetPanel </returns>
        public Control AddCheckboxOptionField(
            Panel targetPanel,
            string id, string label, string tooltip,
            eAdminUpdateProperty.CATEGORY propCat, int propKeyCode, Type propKeyType,
            bool selectedValue,
            Dictionary<string, string> customCheckboxStyleAttributes = null,
            string customCheckboxCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null,
            string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null,
            string customLabelCSSClasses = "",
            string onClick = DEFAULT_OPTION_ONCHANGE,
            bool swap = false,
            eLibConst.CONFIGADV_CATEGORY configAdvCat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED,
            bool readOnly = false)
        {
            eAdminCheckboxField chkField = new eAdminCheckboxField(0, label, propCat, propKeyCode, tooltip, selectedValue, id, customCheckboxStyleAttributes, customCheckboxCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses, "", readOnly);
            chkField.SetFieldControlID(id);
            if (configAdvCat != eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                chkField.SetConfigAdvCategory(configAdvCat);
            chkField.Generate(targetPanel);
            ((eCheckBoxCtrl)chkField.FieldControl).AddClick(onClick); // on modifie le onclick par défaut pour le câbler sur la fonction passée en paramètre
            ((eCheckBoxCtrl)chkField.FieldControl).Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(propKeyType));

            if (swap)
            {
                //
                chkField.SetControlAttribute("swap", "1");

                Control a = chkField.FieldControl.Controls[0];
                Control b = chkField.FieldControl.Controls[1];
                chkField.FieldControl.Controls.Clear();

                chkField.FieldControl.Controls.Add(b);
                chkField.FieldControl.Controls.Add(a);
            }


            return chkField.FieldControl;
        }


        /// <summary>
        /// Ajoute un Champ de type bit
        /// </summary>
        /// <param name="targetPanel">Conteneur</param>
        /// <param name="id">Id du btn</param>
        /// <param name="label">Libellé</param>
        /// <param name="tooltip">Tooltip</param>
        /// <param name="onClick">action sur click</param>
        /// <param name="url"></param>
        /// <param name="disabled">boutton désactivé</param>
        /// <returns></returns>
        public static Control AddButtonOptionField(
            Panel targetPanel,
            string id, string label, string tooltip,
            string onClick = DEFAULT_OPTION_ONCHANGE, string url = "", bool disabled = false)
        {
            eAdminButtonField btnField = eAdminButtonField.GetEAdminButtonField(
                    new eAdminButtonParams()
                    {
                        Label = label,
                        ID = id,
                        ToolTip = tooltip,
                        OnClick = onClick,
                        Href = url,
                        Disabled = disabled
                    }
              );



            btnField.SetFieldControlID(id);
            btnField.Generate(targetPanel);
            return btnField.FieldControl;
        }

        public Control AddRadioButtonOptionField(
            Panel targetPanel,
            string id, string groupName, string label, string tooltip,
            eAdminUpdateProperty.CATEGORY propCat, int propKeyCode, Type propKeyType,
            Dictionary<string, string> items, string selectedValue, FieldFormat valueFormat,
            Dictionary<string, string> customRadioButtonStyleAttributes = null, string customRadioButtonCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "",
            string onClick = DEFAULT_OPTION_ONCHANGE, eLibConst.CONFIGADV_CATEGORY configAdvCat = eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
        {
            eAdminRadioButtonField rbField = new eAdminRadioButtonField(0, label, propCat, propKeyCode, groupName, items, tooltip, selectedValue, valueFormat, customRadioButtonStyleAttributes, customRadioButtonCSSClasses, customPanelStyleAttributes, customPanelCSSClasses, customLabelStyleAttributes, customLabelCSSClasses);
            rbField.SetFieldControlID(id);
            if (configAdvCat != eLibConst.CONFIGADV_CATEGORY.UNDEFINED)
                rbField.SetConfigAdvCategory(configAdvCat);
            rbField.IsLabelBefore = true;
            rbField.Generate(targetPanel);

            ControlCollection rbOptions = ((HtmlGenericControl)rbField.FieldControl).Controls;
            foreach (HtmlControl rbOption in rbOptions)
            {
                foreach (HtmlControl rbOptionChild in rbOption.Controls)
                {
                    if (rbOptionChild.TagName == "input")
                    {
                        rbOptionChild.Attributes["onclick"] = onClick; // on modifie le onclick par défaut pour le câbler sur la fonction passée en paramètre
                        rbOptionChild.Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(propKeyType));
                    }
                }
            }

            LastPanel = rbField.ParentPanel;
            return rbField.FieldControl;
        }

        public Panel LastPanel { get; private set; } = new Panel();

        private int GetCatFromConfigKeyType(Type configKeyType)
        {
            int cat = 0;
            if (configKeyType == typeof(eLibConst.PREF_CONFIG))
                cat = (int)eAdminUpdateProperty.CATEGORY.CONFIG;
            if (configKeyType == typeof(eLibConst.CONFIG_DEFAULT))
                cat = (int)eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT;
            if (configKeyType == typeof(eLibConst.CONFIGADV))
                cat = (int)eAdminUpdateProperty.CATEGORY.CONFIGADV;
            return cat;
        }


    }
}