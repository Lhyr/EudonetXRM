using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Linq;
using System.Text;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldLayoutRenderer : eAdminBlockRenderer
    {
        private Int32 _descid;
        private eAdminFieldInfos _field;

        private eAdminFieldLayoutRenderer(ePref pref, eAdminFieldInfos field)
           : base(pref, null, eResApp.GetRes(pref, 745), idBlock: "blockLayout")
        {
            _descid = field.DescId;
            _field = field;
        }

        public static eAdminFieldLayoutRenderer CreateAdminFieldLayoutRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldLayoutRenderer features = new eAdminFieldLayoutRenderer(pref, field);
            return features;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();

            eAdminField adminField;
            Dictionary<String, String> dicRb;

            // Champ système si rubrique "Couleurs" de Planning ou si descid dans enum AllField
            Boolean bSystemField = false;
            if ((_field.Table.EdnType == EdnType.FILE_PLANNING && _descid % 100 == PlanningField.DESCID_CALENDAR_COLOR.GetHashCode())
                || Enum.IsDefined(typeof(AllField), _descid % 100))
            {
                bSystemField = true;
            }

            adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 7377), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.COLSPAN.GetHashCode(), AdminFieldType.ADM_TYPE_NUM, value: _field.Colspan.ToString());
            if (bSystemField)
                adminField.ReadOnly = true;
            adminField.Generate(_panelContent);

            if (
                _field.Format == FieldFormat.TYP_MEMO
                || _field.Format == FieldFormat.TYP_IMAGE
                || _field.Format == FieldFormat.TYP_IFRAME
                || _field.Format == FieldFormat.TYP_CHART
                )
            {
                // Nombre de lignes
                adminField = new eAdminTextboxField(_descid, eResApp.GetResWithColon(Pref, 6373), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.ROWSPAN.GetHashCode(), AdminFieldType.ADM_TYPE_NUM, value: _field.Rowspan.ToString());
                adminField.Generate(_panelContent);
            }

            //// Libellé en gras
            //adminField = new eAdminCheckboxField(_descid, "Libellé en gras", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.BOLD.GetHashCode(), value: _field.Bold);
            //adminField.Generate(_panelContent);

            //// Libellé en italique
            //adminField = new eAdminCheckboxField(_descid, "Libellé en italique", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.ITALIC.GetHashCode(), value: _field.Italic);
            //adminField.Generate(_panelContent);

            //// Libellé souligné
            //adminField = new eAdminCheckboxField(_descid, "Libellé souligné", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.UNDERLINE.GetHashCode(), value: _field.UnderLine);
            //adminField.Generate(_panelContent);

            // Style
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem(eResApp.GetRes(Pref, 6056), "8"));
            list.Add(new ListItem(eResApp.GetRes(Pref, 7927), "14"));
            list.Add(new ListItem(eResApp.GetRes(Pref, 7928), "12"));
            adminField = new eAdminDropdownField(_descid, eResApp.GetRes(Pref, 7926), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.FIELDSTYLE.GetHashCode(), list.ToArray(), eResApp.GetRes(Pref, 7929),
                value: _field.FieldStyle.GetHashCode().ToString(), renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE);
            adminField.Generate(_panelContent);

            // GIS : format d'affichage
            adminField = new eAdminFormatField(Pref, _descid, eResApp.GetRes(Pref, 7379), _field.Bold, _field.Italic, _field.UnderLine, eResApp.GetRes(Pref, 7381));
            adminField.Generate(_panelContent);



            // Couleur du libellé
            adminField = new eAdminColorField(_descid, eResApp.GetRes(Pref, 527), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.FORECOLOR.GetHashCode(),
                colorPickerID: "labelColorPicker", txtColorID: "labelTxtColor", value: _field.ForeColor);
            adminField.Generate(_panelContent);


            if (_field.Format == FieldFormat.TYP_MEMO || _field.Format == FieldFormat.TYP_TITLE)
            {
            }
            else if (_field.Format == FieldFormat.TYP_BITBUTTON)
            {
                // Type BITBUTTON
                // Couleur du bouton
                adminField = new eAdminColorField(_descid, eResApp.GetRes(Pref, 7955), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.BUTTONCOLOR.GetHashCode(),
                                 colorPickerID: "buttonColorPicker", txtColorID: "buttonTxtColor", value: _field.ButtonColor);
                adminField.Generate(_panelContent);
            }
            else
            {
                // Couleur de la valeur 
                adminField = new eAdminColorField(_descid, eResApp.GetRes(Pref, 7930), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.VALUECOLOR.GetHashCode(),
                             colorPickerID: "valueColorPicker", txtColorID: "valueTxtColor", value: _field.ValueColor);
                adminField.Generate(_panelContent);
            }

            if (_field.Format != FieldFormat.TYP_TITLE)
            {
                // Visibilité du libellé
                dicRb = new Dictionary<string, string>();
                dicRb.Add("0", eResApp.GetRes(Pref, 7957));
                dicRb.Add("1", eResApp.GetRes(Pref, 1432));
                adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7958), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.LABELHIDDEN.GetHashCode(), "rbLabelVisibility", dicRb, eResApp.GetRes(Pref, 7971),
                    value: _field.LabelHidden ? "1" : "0");
                adminField.IsLabelBefore = true;
                adminField.Generate(_panelContent);

                // Largeur de la valeur
                dicRb = new Dictionary<string, string>();
                dicRb.Add("0", eResApp.GetRes(Pref, 6256));
                dicRb.Add("1", eResApp.GetRes(Pref, 6086));
                adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7966), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.MAXIMIZEVALUE.GetHashCode(), "rbValueWidth", dicRb,
                    tooltiptext: eResApp.GetRes(Pref, 7967), value: _field.MaximizeValue ? "1" : "0");
                adminField.IsLabelBefore = true;
                if (!_field.LabelHidden)
                    adminField.IsDisplayed = false;
                adminField.Generate(_panelContent);
                adminField.PanelField.ID = "fieldRbValueWidth";
            }
            else if (_field.Length != 1)
            {
                // Etiquette "blanche" pour aérerer la mise en page.
                dicRb = new Dictionary<string, string>();
                dicRb.Add("0", eResApp.GetRes(Pref, 7957));
                dicRb.Add("1", eResApp.GetRes(Pref, 1432));
                adminField = new eAdminRadioButtonField(_descid, eResApp.GetRes(Pref, 7958), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.LABELHIDDEN.GetHashCode(), "rbBlankTitle", dicRb, eResApp.GetRes(Pref, 7971),
                    value: _field.LabelHidden ? "1" : "0");
                adminField.IsLabelBefore = true;
                adminField.Generate(_panelContent);

            }

            if (_field.Format == FieldFormat.TYP_DATE && _field.Table.EudonetXIrisBlackStatus == EUDONETX_IRIS_BLACK_STATUS.ENABLED)
            {
                List<ListItem> lstDateFormat = new List<ListItem>();
                //Value.item1 =format de la chaine // value.item2 = resid du complement
                lstDateFormat.AddRange(eLibConst.FieldDateFormat
                    .Select(kvp => new ListItem(kvp.Value.Item2 > 0 ? String.Format("{0} ({1})", kvp.Value.Item1, eResApp.GetRes(_ePref, kvp.Value.Item2)) : kvp.Value.Item1, ((int)kvp.Key).ToString())));
                adminField = new eAdminDropdownField(_descid,
                    eResApp.GetResWithColon(Pref, 7380),
                    eAdminUpdateProperty.CATEGORY.DESCADV,
                    (int)DESCADV_PARAMETER.DISPLAY_FORMAT,
                    items: lstDateFormat.ToArray(),
                    tooltiptext: eResApp.GetRes(Pref, 7381),
                    value: _field.DisplayFormat,
                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                    customLabelCSSClasses: "info");
                adminField.Generate(_panelContent);

                // Intervalle de date
                List<ListItem> lstDateFields = new List<ListItem>();
                lstDateFields.Insert(0, new ListItem(string.Empty, "-1"));
                eudoDAL dal = null;
                dal = eLibTools.GetEudoDAL(Pref);
                dal.OpenDatabase();
                String sError = "";
                Dictionary<int, int> dicFieldsInInterval = eSqlDesc.GetDateInterval(dal, _field.Table.DescId, _field.DescId, out sError);

                //indique si le champs en cours est défini comme date de fin dans un intervalle
                bool bAnEndDate = dicFieldsInInterval.ContainsValue(_field.DescId);

                lstDateFields.AddRange(RetrieveFields.GetDefault(Pref)
                                       .AddOnlyThisTabs(new List<int>() { _field.Table.DescId })
                                       .AddExcludeDescId(new List<int>() { _field.DescId }) // on ne propose pas le champ lui meme dans la liste
                                       .AddExcludeDescId(dicFieldsInInterval.Keys)          //on ne propose pas les champs qui ont déjà une date de fin d'intervalle définie
                                       .AddExcludeDescId(dicFieldsInInterval.Values)        // on ne propose pas les champs qui sont définis comme date de fin d'un intervalle
                                       .AddOnlyThisFormats(new List<FieldFormat>() { FieldFormat.TYP_DATE })
                                       .ResultFieldsInfo(eFieldLiteWithLib.Factory(Pref)).ToList()
                                       .Select(f => new ListItem(f.Libelle, ((int)f.Descid).ToString())));

                adminField = new eAdminDropdownField(_descid,
                                        eResApp.GetRes(Pref, 2494),
                                        eAdminUpdateProperty.CATEGORY.DESCADV,
                                        (int)DESCADV_PARAMETER.DATE_END_DESCID,
                                        items: lstDateFields.ToArray(),
                                        tooltiptext: bAnEndDate ? eResApp.GetRes(Pref, 2502) : eResApp.GetRes(Pref, 2493),
                                        value: _field.DateEndDescId,
                                        renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                                        customLabelCSSClasses: "info");

                adminField.ReadOnly = bAnEndDate;
                adminField.Generate(_panelContent);

            }

            // Format d'affichage
            if (_field.Format == FieldFormat.TYP_CHAR && !bSystemField) // Que pour les types caractères pour l'instant
            {
                adminField = new eAdminDropdownField(_descid, eResApp.GetResWithColon(Pref, 7380), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.CASE.GetHashCode(), GetDisplayCases(),
                tooltiptext: eResApp.GetRes(Pref, 7381),
                value: _field.Case.GetHashCode().ToString(), renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE, customLabelCSSClasses: "info");
                adminField.Generate(_panelContent);
            }

            //GenerateLabelDisplayFormat();

            return true;
        }

        /// <summary>Retourne la liste des items de la DropDownList "Format d'affichage" suivant le format de la rubrique</summary>
        /// <returns></returns>
        private ListItem[] GetDisplayCases()
        {
            List<ListItem> items = new List<ListItem>();

            if (_field.Format == FieldFormat.TYP_CHAR)
            {
                items.Add(new ListItem(eResApp.GetRes(Pref, 2199), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 239), CaseField.CASE_UPPER.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2200), CaseField.CASE_CAPITALIZE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2201), CaseField.CASE_LOWER.GetHashCode().ToString()));
                // TODO : Valeur personnalisée
                //items.Add(new ListItem("Personnalisé", CaseField.CASE_CUSTOM.GetHashCode().ToString()));
            }
            else if (_field.Format == FieldFormat.TYP_NUMERIC)
            {
                items.Add(new ListItem(eResApp.GetRes(Pref, 2199), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2202), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2203), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 702), CaseField.CASE_NONE.GetHashCode().ToString()));
            }
            else if (_field.Format == FieldFormat.TYP_DATE)
            {
                // TODO
            }
            else if (_field.Format == FieldFormat.TYP_BIT)
            {
                items.Add(new ListItem(eResApp.GetRes(Pref, 2204), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2205), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2206), CaseField.CASE_NONE.GetHashCode().ToString()));
            }
            else if (_field.Format == FieldFormat.TYP_PHONE)
            {
                items.Add(new ListItem(eResApp.GetRes(Pref, 2199), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2207), CaseField.CASE_NONE.GetHashCode().ToString()));
            }
            else if (_field.Format == FieldFormat.TYP_GEOGRAPHY_V2)
            {
                items.Add(new ListItem(eResApp.GetRes(Pref, 2208), CaseField.CASE_NONE.GetHashCode().ToString()));
                items.Add(new ListItem(eResApp.GetRes(Pref, 2209), CaseField.CASE_NONE.GetHashCode().ToString()));
            }

            return items.ToArray();
        }
    }
}