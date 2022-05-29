using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Web.UI;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu d'une liste déroulante en administration
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminField" />
    public class eAdminDropdownField : eAdminField
    {
        #region Propriétés
        ListItem[] _items = new ListItem[] { };

        Dictionary<string, string> _customDropdownStyleAttributes = new Dictionary<string, string>();
        Dictionary<string, string> _customPanelStyleAttributes = new Dictionary<string, string>();
        Dictionary<string, string> _customLabelStyleAttributes = new Dictionary<string, string>();
        string _customDropdownCSSClasses = string.Empty;
        string _customPanelCSSClasses = string.Empty;
        string _customLabelCSSClasses = string.Empty;
        #endregion

        #region Accesseurs
        /// <summary>
        /// Tri des items par libellé
        /// </summary>
        public Boolean SortItemsByLabel { get; set; }
        /// <summary>
        /// Type d'affichage de la liste déroulante
        /// </summary>
        public eAdminDropdownFieldRenderType RenderType { get; set; }
        /// <summary>
        /// Ajout d'une valeur vide "Aucun"
        /// </summary>
        public bool AddEmptyOption { get; set; }
        /// <summary>
        /// JS exécuté sur l'événement "onchange" de la dropdownlist
        /// </summary>
        public string OnChange { get; set; }
        /// <summary>
        /// Classes CSS sur le libellé
        /// </summary>
        public string CustomLabelCSSClasses
        {
            get
            {
                return _customLabelCSSClasses;
            }

            set
            {
                _customLabelCSSClasses = value;
            }
        }
        /// <summary>
        /// Classes CSS sur le Panel contenant le DDL
        /// </summary>
        public string CustomPanelCSSClasses
        {
            get
            {
                return _customPanelCSSClasses;
            }

            set
            {
                _customPanelCSSClasses = value;
            }
        }
        /// <summary>
        /// Classes CSS sur le DDL
        /// </summary>
        public string CustomDropdownCSSClasses
        {
            get
            {
                return _customDropdownCSSClasses;
            }

            set
            {
                _customDropdownCSSClasses = value;
            }
        }
        /// <summary>
        /// Dictionnaire des attributs personnalisés sur le libellé
        /// </summary>
        public Dictionary<string, string> CustomLabelStyleAttributes
        {
            get
            {
                return _customLabelStyleAttributes;
            }

            set
            {
                _customLabelStyleAttributes = value;
            }
        }
        /// <summary>
        /// Dictionnaire des attributs personnalisés sur le Panel contenant le Dropdownlist
        /// </summary>
        public Dictionary<string, string> CustomPanelStyleAttributes
        {
            get
            {
                return _customPanelStyleAttributes;
            }

            set
            {
                _customPanelStyleAttributes = value;
            }
        }
        /// <summary>
        /// Dictionnaire des attributs personnalisés sur le DropDownList
        /// </summary>
        public Dictionary<string, string> CustomDropdownStyleAttributes
        {
            get
            {
                return _customDropdownStyleAttributes;
            }

            set
            {
                _customDropdownStyleAttributes = value;
            }
        }
        /// <summary>
        /// Tableau des items
        /// </summary>
        public ListItem[] Items
        {
            get
            {
                return _items;
            }

            set
            {
                _items = value;
            }
        }
        #endregion

        /// <summary>
        /// Types d'affichage de la liste déroulante
        /// </summary>
        public enum eAdminDropdownFieldRenderType
        {
            /// <summary>
            /// Sur une ligne
            /// </summary>
            INLINE,
            /// <summary>
            /// Libellé au-dessus
            /// </summary>
            LABELABOVE,
            /// <summary>
            /// Libellé en-dessous
            /// </summary>
            SELECTABOVE
        }

        /// <summary>
        /// Constructeur simple d'une liste déroulante
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="descid">Descid de la rubrique/table</param>
        /// <param name="label">Libellé du champ</param>
        /// <param name="propCat">Catégorie de la propriété : DESC/RES/PREF/CONFIG...</param>
        /// <param name="propCode">Hashcode de la propriété</param>
        public eAdminDropdownField(ePref pref, int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode) : base(descid, label, propCat, propCode)
        {
            this.Pref = pref;
        }
        /// <summary>
        /// Constructeur d'un champ de type liste déroulante
        /// </summary>
        /// <param name="descid">Descid de la rubrique/table</param>
        /// <param name="label">Libellé du champ</param>
        /// <param name="propCat">Catégorie de la propriété : DESC/RES/PREF/CONFIG...</param>
        /// <param name="propCode">Hashcode de la propriété</param>
        /// <param name="items">Liste d'items</param>
        /// <param name="tooltiptext">Tooltip</param>
        /// <param name="value">Valeur sélectionnée</param>
        /// <param name="valueFormat">Format de la valeur</param>
        /// <param name="renderType">Type of the render.</param>
        /// <param name="customDropdownStyleAttributes">The custom dropdown style attributes.</param>
        /// <param name="customDropdownCSSClasses">The custom dropdown CSS classes.</param>
        /// <param name="customPanelStyleAttributes">The custom panel style attributes.</param>
        /// <param name="customPanelCSSClasses">The custom panel CSS classes.</param>
        /// <param name="customLabelStyleAttributes">The custom label style attributes.</param>
        /// <param name="customLabelCSSClasses">The custom label CSS classes.</param>
        /// <param name="sortItemsByLabel">if set to <c>true</c> [sort items by label].</param>
        /// <param name="onChange">The on change.</param>
        /// <param name="bNoUpdate">if set to <c>true</c> [b no update].</param>
        /// <param name="mandatory">if set to <c>true</c> [b mandatory].</param>
        public eAdminDropdownField(
            int descid, String label, eAdminUpdateProperty.CATEGORY propCat, Int32 propCode, ListItem[] items, String tooltiptext = "", String value = "", FieldFormat valueFormat = FieldFormat.TYP_NUMERIC, eAdminDropdownFieldRenderType renderType = eAdminDropdownFieldRenderType.INLINE,
            Dictionary<string, string> customDropdownStyleAttributes = null, string customDropdownCSSClasses = "",
            Dictionary<string, string> customPanelStyleAttributes = null, string customPanelCSSClasses = "",
            Dictionary<string, string> customLabelStyleAttributes = null, string customLabelCSSClasses = "",
            bool sortItemsByLabel = true, string onChange = "", bool bNoUpdate = false, bool mandatory = false
        )
            : base(descid, label, propCat, propCode, tooltiptext, value)
        {
            _items = items;
            SortItemsByLabel = sortItemsByLabel;
            RenderType = renderType;
            Format = valueFormat;
            NoUpdate = bNoUpdate;
            _customDropdownStyleAttributes = customDropdownStyleAttributes;
            _customDropdownCSSClasses = customDropdownCSSClasses;
            _customPanelStyleAttributes = customPanelStyleAttributes;
            _customPanelCSSClasses = customPanelCSSClasses;
            _customLabelStyleAttributes = customLabelStyleAttributes;
            _customLabelCSSClasses = customLabelCSSClasses;
            this.OnChange = onChange;
            this.OnChange = onChange;
            Mandatory = mandatory;
        }

        /// <summary>
        /// Builds the specified panel.
        /// </summary>
        /// <param name="panel">The panel.</param>
        /// <returns></returns>
        protected override Boolean Build(Panel panel)
        {
            if (base.Build(panel))
            {
                // Si aucune valeur, on n'affiche pas le dropdownlist
                if (_items.Length > 0)
                {
                    if (SortItemsByLabel)
                        _items = _items.OrderBy(item => item.Text).ToArray<ListItem>();

                    DropDownList ddl = new DropDownList();
                    ddl.Attributes.Add("dsc", this.AttrDsc);
                    if (_items.Length > 0)
                        ddl.Items.AddRange(_items);

                    // Ajout de l'option vide en première position
                    if (this.AddEmptyOption && this.Pref != null)
                        ddl.Items.Insert(0, new ListItem(String.Concat("<", eResApp.GetRes(this.Pref, 141), ">"), "0")); // Vide

                    this.FieldControl = ddl;

                    // Valeur sélectionnée
                    if (!String.IsNullOrEmpty(this.Value))
                    {
                        ListItem selectedListItem = ddl.Items.FindByValue(this.Value);
                        if (selectedListItem != null)
                        {
                            selectedListItem.Selected = true;
                        };
                    }

                    if (!String.IsNullOrEmpty(this.OnChange))
                        ddl.Attributes.Add("onchange", this.OnChange);

                    if (IsOptional)
                        ddl.Attributes.Add("opt", "1");

                    HtmlGenericControl htmlLabel = new HtmlGenericControl("label");
                    htmlLabel.InnerText = this.Label;

                    //Mandatory
                    if (Mandatory.HasValue)
                    {
                        htmlLabel.Attributes.Add("mandatory", Mandatory.Value ? "1" : "0");
                        if (Mandatory.Value)
                        {
                            HtmlGenericControl mandatoryParam = new HtmlGenericControl("span");
                            mandatoryParam.Attributes.Add("class", "mandatoryParam");
                            mandatoryParam.Style.Add(HtmlTextWriterStyle.Display, "contents");
                            mandatoryParam.InnerText = "*";
                            htmlLabel.Controls.Add(mandatoryParam);
                        }
                    }

                    this.FieldLabel = htmlLabel;

                    if (RenderType == eAdminDropdownFieldRenderType.INLINE)
                    {
                        this.PanelField.Controls.Add(htmlLabel);
                        this.PanelField.Controls.Add(ddl);
                    }
                    else if (RenderType == eAdminDropdownFieldRenderType.LABELABOVE)
                    {
                        base.PanelField.CssClass = String.Concat(base.PanelField.CssClass, " ", "verticalSelect");
                        this.PanelField.Controls.Add(htmlLabel);
                        this.PanelField.Controls.Add(ddl);
                    }
                    else if (RenderType == eAdminDropdownFieldRenderType.SELECTABOVE)
                    {
                        base.PanelField.CssClass = String.Concat(base.PanelField.CssClass, " ", "verticalSelect");
                        this.PanelField.Controls.Add(ddl);
                        this.PanelField.Controls.Add(htmlLabel);
                    }

                    panel.Controls.Add(this.PanelField);
                }

                return true;
            }

            return false;

        }

        /// <summary>
        /// Actions exécutées après génération de la liste déroulante
        /// </summary>
        /// <returns></returns>
        protected override Boolean End()
        {
            if (base.End())
            {
                #region Ajout des styles et CSS additionnels

                #region Sur le contrôle
                if (_customDropdownStyleAttributes != null && _customDropdownStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customDropdownStyleAttributes)
                        ((DropDownList)this.FieldControl).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customDropdownCSSClasses))
                {
                    ((DropDownList)this.FieldControl).CssClass = String.Concat(((DropDownList)this.FieldControl).CssClass, " ", _customDropdownCSSClasses).Trim();
                }
                #endregion

                #region Sur le libellé
                if (_customLabelStyleAttributes != null && _customLabelStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customLabelStyleAttributes)
                        ((HtmlGenericControl)this.FieldLabel).Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customLabelCSSClasses))
                {
                    ((HtmlGenericControl)this.FieldLabel).Attributes["class"] = String.Concat(((HtmlGenericControl)this.FieldLabel).Attributes["class"], " ", _customLabelCSSClasses).Trim();
                }
                #endregion

                #region Sur le conteneur
                if (_customPanelStyleAttributes != null && _customPanelStyleAttributes.Count > 0)
                {
                    foreach (KeyValuePair<string, string> style in _customPanelStyleAttributes)
                        this.PanelField.Style.Add(style.Key, style.Value);
                }

                if (!String.IsNullOrEmpty(_customPanelCSSClasses))
                {
                    this.PanelField.CssClass = String.Concat(this.PanelField.CssClass, " ", _customPanelCSSClasses).Trim();
                }
                #endregion

                #endregion
            }

            return true;
        }

    }
}