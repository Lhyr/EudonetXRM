using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldFeaturesRenderer : eAdminBlockRenderer
    {
        private int _descid;
        private eAdminFieldInfos _field;

        private eAdminFieldFeaturesRenderer(ePref pref, eAdminFieldInfos field)
            : base(pref, null, eResApp.GetRes(pref, 6809), idBlock: "blockFeatures")
        {
            _descid = field.DescId;
            _field = field;
        }

        public static eAdminFieldFeaturesRenderer CreateAdminFieldFeaturesRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldFeaturesRenderer features = new eAdminFieldFeaturesRenderer(pref, field);
            return features;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            bool userAllowed = _field.IsUserAllowedToUpdate();

            eAdminField adminField;


            // Infobulle
            adminField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 7008), eAdminUpdateProperty.CATEGORY.RESADV, eLibConst.RESADV_TYPE.TOOLTIP.GetHashCode(),
                tooltiptext: _field.ToolTipText,
                value: _field.ToolTipText,
                nbRows: 3);
            adminField.Generate(_panelContent);
            IsParamApplicable(adminField, eLibConst.DESC.TOOLTIPTEXT);

            // champs systèmes sur PJ
            HashSet<int> lstPJSysFields = eEnumTools<PJField>.GetEnumIds<PJField>();
            lstPJSysFields.Remove((int)PJField.DESCRIPTION); //le champs description n'est pas en lecture seule

            List<FieldFormat> lstFormats = new List<FieldFormat>() {
                FieldFormat.TYP_BIT,
                FieldFormat.TYP_CHART,
                FieldFormat.TYP_IFRAME,
                FieldFormat.TYP_IMAGE,
                FieldFormat.TYP_HIDDEN,
                FieldFormat.TYP_COUNT,
                FieldFormat.TYP_MEMO
            };

            if (!lstFormats.Contains(_field.Format)
                && !lstPJSysFields.Contains(_field.DescId)
                )
            {
                // Filigrane
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7009),
                    eAdminUpdateProperty.CATEGORY.RESADV,
                    eLibConst.RESADV_TYPE.WATERMARK.GetHashCode(),
                    tooltiptext: _field.WaterMark,
                    value: _field.WaterMark,
                    nbRows: 1,
                    customTextboxCSSClasses: "watermark");

                adminField.Generate(_panelContent);
                //IsParamApplicable(adminField, eLibConst.DESC.TOOLTIPTEXT); // tocheck : dans certains cas, on a pas d'enum sur desc. depuis, il vaudrait mieux ne pas gérérer le champ plutôt que juste le masquer.
            }

            if (_field.Format == FieldFormat.TYP_IFRAME)
            {


                // URL
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7634),
                    eAdminUpdateProperty.CATEGORY.DESC,
                    (int)eLibConst.DESC.DEFAULT,
                    value: _field.Default,
                    nbRows: 1);
                adminField.Generate(_panelContent);



                adminField = new eAdminCheckboxField(_descid, eResApp.GetRes(Pref, 7635),
                    eAdminUpdateProperty.CATEGORY.DESC,
                    (int)eLibConst.DESC.SCROLLING,
                    value: _field.Scrolling
                    );
                adminField.Generate(_panelContent);
            }

            if (_field.Format == FieldFormat.TYP_SOCIALNETWORK)
            {
                // Pictogramme
                adminField = new eAdminPictoField(_descid, eResApp.GetResWithColon(Pref, 6819), eResApp.GetRes(Pref, 8104), _field.Icon, _field.IconColor);
                adminField.Generate(_panelContent);

                // Base URL
                adminField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 8102), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.ROOTURL.GetHashCode(), value: _field.BaseURL);
                adminField.Generate(_panelContent);
            }

            if (_field.Format == FieldFormat.TYP_SOCIALNETWORK || _field.Format == FieldFormat.TYP_WEB || _field.Format == FieldFormat.TYP_EMAIL)
            {
                //Afficher dans la base d'action
                adminField = new eAdminCheckboxField(_descid, eResApp.GetRes(Pref, 8103), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.DISPLAYINACTIONBAR.GetHashCode(), value: _field.DisplayInActionBar);
                adminField.Generate(_panelContent);

                // Activer la suggestion d'e-mails
                if (_field.Format == FieldFormat.TYP_EMAIL && (_field.Table.DescId == (int)TableType.ADR || _field.Table.DescId == (int)TableType.PP))
                {
                    adminField = new eAdminCheckboxField(_descid, eResApp.GetRes(Pref, 2179), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.EMAIL_SUGGESTIONS_ENABLED.GetHashCode(), value: _field.EmailSuggestionsEnabled);
                    adminField.Generate(_panelContent);
                }
            }


            // Valeur par défaut
            //adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 528), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.DEFAULT.GetHashCode(), value: _field.Default);
            //adminField.Generate(_panelContent);

            // Ordre de saisie
            if (_field.DescId % 100 == (int)AllField.MEMO_NOTES || _field.DescId % 100 == (int)AllField.MEMO_DESCRIPTION)
            {
            }
            else
            {
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7010), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.TABINDEX.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM,
                    tooltiptext: eResApp.GetRes(Pref, 7011),
                    value: _field.TabIndex.ToString());
                adminField.Generate(_panelContent);
                IsParamApplicable(adminField, eLibConst.DESC.TABINDEX);
            }


            if (_field.Format == FieldFormat.TYP_NUMERIC || _field.Format == FieldFormat.TYP_MONEY)
            {
                // Nombre de décimales
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7012), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.LENGTH.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Length.ToString());
                adminField.ReadOnly = !userAllowed;
                adminField.Generate(_panelContent);
                adminField.SetControlAttribute("erngmin", "0");
                adminField.SetControlAttribute("erngmax", "18");
            }
            else if (_field.Format == FieldFormat.TYP_AUTOINC)
            {
                // HLA - Rubrique Compteur identifiant système - Utilisation direct de _field.IsSysId - #62445

                // Début
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7013), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.LENGTH.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Length.ToString());
                adminField.SetFieldControlID("txtLength");
                if (_field.IsSysId)
                    adminField.ReadOnly = true;
                else
                    adminField.ReadOnly = _field.IsSpecialField();
                adminField.Generate(_panelContent);

                // Identifiant système
                adminField = new eAdminCheckboxField(_descid, eResApp.GetResWithColon(Pref, 976), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.SHOWFILEID.GetHashCode(), value: _field.IsSysId);
                adminField.SetFieldControlID("chkShowFileId");
                adminField.ReadOnly = _field.IsSpecialField();
                adminField.Generate(_panelContent);
            }
            else if (_field.Format == FieldFormat.TYP_CHAR || _field.Format == FieldFormat.TYP_COUNT || _field.Format == FieldFormat.TYP_EMAIL || _field.Format == FieldFormat.TYP_PHONE || _field.Format == FieldFormat.TYP_WEB || _field.Format == FieldFormat.TYP_SOCIALNETWORK)
            {
                // Longueur
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7014), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.LENGTH.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Length.ToString());
                adminField.SetFieldControlID("txtLength");
                adminField.ReadOnly = !userAllowed;
                adminField.Generate(_panelContent);
                if (_field.Format == FieldFormat.TYP_CHAR)
                {
                    adminField.SetControlAttribute("erngmin", "0");
                    adminField.SetControlAttribute("erngmax", "8000");
                }
            }

            // HTML
            Dictionary<string, string> itemsRB = new Dictionary<string, string>();
            itemsRB.Add("0", eResApp.GetRes(Pref, 7384)); // Brut
            itemsRB.Add("1", eResApp.GetRes(Pref, 7385)); // Mis en forme
            adminField = new eAdminRadioButtonField(_descid, eResApp.GetResWithColon(Pref, 7015), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.HTML.GetHashCode(), "rbHTML", itemsRB, value: (_field.IsHTML) ? "1" : "0");
            adminField.IsLabelBefore = true;
            adminField.Generate(_panelContent);
            IsParamApplicable(adminField, eLibConst.DESC.HTML);

            //ajoute les unités de mesures
            addUnitParam();

            // Somme dans les entêtes
            adminField = new eAdminCheckboxField(_descid, eResApp.GetResWithColon(Pref, 7016), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.COMPUTEDFIELDENABLED.GetHashCode(),
                tooltiptext: eResApp.GetRes(Pref, 7017),
                value: _field.ComputedFieldEnabled);
            adminField.Generate(_panelContent);
            IsParamApplicable(adminField, eLibConst.DESC.COMPUTEDFIELDENABLED);

            if (_field.Format == FieldFormat.TYP_CHART)
            {

                Panel pnHiddenFields = new Panel();
                pnHiddenFields.Style.Add(HtmlTextWriterStyle.Display, "none");
                _panelContent.Controls.Add(pnHiddenFields);

                //Paramètres
                //champ masqué pas de res
                adminField = new eAdminTextboxField(_descid, "Paramétrage du Chart", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.PARAMETERS.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.GetParameters);
                adminField.SetFieldControlID("ChartParameters");
                adminField.Generate(pnHiddenFields);

                //ChartId
                //champ masqué pas de res
                adminField = new eAdminTextboxField(_descid, "Id du Chart", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.PARAMETERS.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Parameters["reportid"]);
                adminField.SetFieldControlID("ChartId");
                adminField.NoUpdate = true;
                adminField.Generate(pnHiddenFields);

                //Choix du graphique à Afficher
                adminField = new eAdminButtonField(eResApp.GetRes(Pref, 7632), "btnChooseAChart", eResApp.GetRes(Pref, 7633), string.Concat("nsAdminField.showChartsList(", _field.Parameters["reportid"], ");"));
                adminField.Generate(_panelContent);

                //largeur
                adminField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 1508), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.PARAMETERS.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Parameters["w"]);
                adminField.SetFieldControlID("ChartWidth");
                adminField.NoUpdate = true;
                adminField.Generate(_panelContent);
                ((TextBox)adminField.FieldControl).Attributes.Add("onchange", "nsAdminField.SetChartParameters();");
                ((TextBox)adminField.FieldControl).Attributes.Add("lastvalid", ((TextBox)adminField.FieldControl).Text);
                ((TextBox)adminField.FieldControl).Attributes.Remove("dsc");

                //hauteur
                adminField = new eAdminTextboxField(_descid, eResApp.GetRes(Pref, 1507), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.PARAMETERS.GetHashCode(), type: AdminFieldType.ADM_TYPE_NUM, value: _field.Parameters["h"]);
                adminField.SetFieldControlID("ChartHeight");
                adminField.NoUpdate = true;
                adminField.Generate(_panelContent);
                ((TextBox)adminField.FieldControl).Attributes.Add("onchange", "nsAdminField.SetChartParameters();");
                ((TextBox)adminField.FieldControl).Attributes.Add("lastvalid", ((TextBox)adminField.FieldControl).Text);
                ((TextBox)adminField.FieldControl).Attributes.Remove("dsc");

            }


            return true;
        }





        /// <summary>
        /// Détermine si la rubrique DESC doit s'afficher et masque la rubrique dans le cas où ce n'est pas applicable.
        /// </summary>
        /// <param name="descProp">The desc property.</param>
        /// <returns></returns>
        private bool IsParamApplicable(eAdminField field, eLibConst.DESC descProp)
        {
            bool isApplicable = false;
            switch (descProp)
            {
                case eLibConst.DESC.TOOLTIPTEXT: isApplicable = true; break;
                case eLibConst.DESC.TABINDEX:
                    if (_field.Format != FieldFormat.TYP_HIDDEN
                        && _field.Format != FieldFormat.TYP_COUNT
                        && _field.Format != FieldFormat.TYP_CHART
                        && _field.Format != FieldFormat.TYP_IFRAME
                        && _field.Format != FieldFormat.TYP_TITLE
                        )
                    {
                        isApplicable = true;
                    }
                    break;
                case eLibConst.DESC.HTML:
                    // #67 326 - Pour les SMS, le choix HTML/texte brut est masqué
                    if (_field.Format == FieldFormat.TYP_MEMO && _field.Table.EdnType != EdnType.FILE_SMS)
                    {
                        isApplicable = true;
                    }
                    break;
                case eLibConst.DESC.COMPUTEDFIELDENABLED:
                    if (_field.Format == FieldFormat.TYP_NUMERIC || _field.Format == FieldFormat.TYP_MONEY)
                    {
                        isApplicable = true;
                    }
                    break;
            }

            if (!isApplicable)
                field.HideField();

            return isApplicable;
        }


        private void addUnitParam()
        {
            List<FieldFormat> lstFormat = new List<FieldFormat>() { FieldFormat.TYP_MONEY, FieldFormat.TYP_NUMERIC };
            if (!lstFormat.Contains(_field.Format))
                return;


            // Unité
            eAdminTextboxField adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 3123),
                eAdminUpdateProperty.CATEGORY.RESADV,
                eLibConst.RESADV_TYPE.UNIT.GetHashCode(),
                value: _field.Unit?.Unit,
                nbRows: 1,
                customTextboxCSSClasses: "unit");

            //adminField.PlaceHolder = eResApp.GetRes(Pref, 3127);
            adminField.Generate(_panelContent);

            Dictionary<string, string> dicPositionsValue = new Dictionary<string, string>();
            dicPositionsValue.Add(((int)UNIT_POSITION.LEFT).ToString(), eResApp.GetRes(Pref, 3125));
            dicPositionsValue.Add(((int)UNIT_POSITION.RIGHT).ToString(), eResApp.GetRes(Pref, 3126));

            eAdminRadioButtonField rb = new eAdminRadioButtonField(
                descid: _descid,
                label: eResApp.GetResWithColon(Pref, 3128),
                propCat: eAdminUpdateProperty.CATEGORY.DESCADV,
                propCode: (int)DESCADV_PARAMETER.UNIT_POSITION,
                groupName: "rbUnitPosition",
                items: dicPositionsValue,
                value: ((int)(_field.Unit?.Position??UNIT_POSITION.RIGHT)).ToString()
                );
            rb.IsLabelBefore = true;
            rb.Generate(_panelContent);
        }
    }
}