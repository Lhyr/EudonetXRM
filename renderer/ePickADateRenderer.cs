using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// renderer de sélection de date pour les filtres et les valeurs par défaut
    /// </summary>
    public class ePickADateRenderer : eRenderer
    {
        public enum From
        {
            None = -1,
            DefaultValue = 0,
            Filter = 1, //non utilisé pour l'instant à voir si on homogeneise ou non
            TreatmentFilter = 2, //non utilisé pour l'instant à voir si on homogeneise ou non
            Formular = 3 //Générateur de formulaire
        }

        private String _sDate = "";
        private From _from;
        private Panel _pnDivDateSelect;
        private TableCell tdWizSep;
        private TableCell tdWizDecal;

        public ePickADateRenderer(ePref pref, From from, String sDate = "")
        {
            Pref = pref;
            _pgContainer = new Panel();
            _sDate = sDate;
            _from = from;
        }

        protected override bool Init()
        {

            _divHidden = new HtmlGenericControl();
            _pgContainer.Controls.Add(_divHidden);

            Table tabOptions = new Table();
            _pgContainer.Controls.Add(tabOptions);
            tabOptions.CssClass = "tabOptions";

            TableRow tr = new TableRow();
            tabOptions.Rows.Add(tr);

            TableCell tc = new TableCell();
            tr.Cells.Add(tc);
            tc.ColumnSpan = 3;
            tc.CssClass = "TdTitle";
            tc.Text = eResApp.GetRes(Pref, 7923);

            tr = new TableRow();
            tabOptions.Rows.Add(tr);

            tc = new TableCell();
            tr.Cells.Add(tc);
            tc.CssClass = "TdOpt";
            tc.Attributes.Add("onclick", "nsPickADate.onCalOptClick()");

            Panel pnCalendar = new Panel();
            tc.Controls.Add(pnCalendar);
            pnCalendar.ID = "Calendar";
            pnCalendar.CssClass = "CalContainer";

            Panel pnTopCalDiv = new Panel();
            pnCalendar.Controls.Add(pnTopCalDiv);
            pnTopCalDiv.ID = "topCalDiv";

            Panel pnBottomCalDiv = new Panel();
            pnCalendar.Controls.Add(pnBottomCalDiv);
            pnBottomCalDiv.ID = "bottomCalDiv";

            tdWizSep = new TableCell();
            tr.Cells.Add(tdWizSep);
            tdWizSep.CssClass = "TdOpt";
            tdWizSep.ID = "tdWizSep";

            Panel pnvSep = new Panel();
            tdWizSep.Controls.Add(pnvSep);
            pnvSep.ID = "vSep";
            pnvSep.CssClass = "VSep";

            tdWizDecal = new TableCell();
            tr.Cells.Add(tdWizDecal);
            tdWizDecal.CssClass = "TdOpt";
            tdWizDecal.ID = "tdWizDecal";

            _pnDivDateSelect = new Panel();
            tdWizDecal.Controls.Add(_pnDivDateSelect);
            _pnDivDateSelect.ID = "DivDateSelect";
            _pnDivDateSelect.CssClass = "DateOpt";

            return true;
        }

        protected override bool Build()
        {
            Int32 nRange = 60;
            String strChange = String.Empty;

            //strMoveVisibility =" style='visibility:hidden' "
            bool bMoveVisible = false;

            bool bNoYear = false;
            if (_from != From.DefaultValue)
            {
                //Date Anniversaire
                if (_sDate.Contains("[NOYEAR]"))
                {
                    _sDate = _sDate.Replace("[NOYEAR]", "");
                    bNoYear = true;
                }
            }


            if (_sDate.Contains("+"))
            {
                String[] aValue = _sDate.Split('+');
                _sDate = aValue[0].Trim();
                strChange = String.Concat("+", aValue[1].Trim());
            }
            else
            {
                if (_sDate.Contains("-"))
                {
                    String[] aValue = _sDate.Split('-');
                    _sDate = aValue[0].Trim();

                    strChange = String.Concat("-", aValue[1].Trim());
                }
            }

            bool bSel0Checked = false;
            bool bSel1Checked = false;
            bool bSel2Checked = false;
            bool bSel3Checked = false;
            bool bSel4Checked = false;
            bool bSel5Checked = false;
            bool bSel6Checked = false;
            bool bFixedDate = false;
            String strLabelMove = String.Empty;

            switch (_sDate)
            {
                case "":
                    bSel0Checked = true;
                    break;
                case "<DATE>":
                    bSel1Checked = true;
                    strLabelMove = eResApp.GetRes(Pref, 853);
                    bMoveVisible = true;
                    //bNoYear = true;
                    break;
                case "<DATETIME>":
                    bSel2Checked = true;
                    break;
                case "<MONTH>":
                    bSel4Checked = true;
                    strLabelMove = eResApp.GetRes(Pref, 854);
                    bMoveVisible = true;
                    //bNoYear = true;
                    break;
                case "<WEEK>":
                    bSel5Checked = true;
                    strLabelMove = eResApp.GetRes(Pref, 852);
                    bMoveVisible = true;
                    break;
                case "<YEAR>":
                    bSel6Checked = true;
                    strLabelMove = eResApp.GetRes(Pref, 855);
                    bMoveVisible = true;
                    break;
                case "<DAY>":
                    strLabelMove = eResApp.GetRes(Pref, 1234);
                    bMoveVisible = true;
                    // bNoYear = true;
                    break;
                default:
                    bSel3Checked = true;
                    bFixedDate = true;
                    break;
            }
            _pnDivDateSelect.Attributes.Add("class", "DateSelect");
            HtmlGenericControl frmSelect = new HtmlGenericControl("form");
            frmSelect.ID = "radioOptForm";
            frmSelect.Attributes.Add("name", "radioOptForm");

            _pnDivDateSelect.Controls.Add(frmSelect);
            frmSelect.Controls.Add(eTools.GetRadioButton("NoDate", "date", bSel0Checked, eResApp.GetRes(Pref, 314), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<NONE>"));
            frmSelect.Controls.Add(eTools.GetRadioButton("CurrDate", "date", bSel1Checked, eResApp.GetRes(Pref, 367), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<DATE>"));
            frmSelect.Controls.Add(eTools.GetRadioButton("DateTime", "date", bSel2Checked, eResApp.GetRes(Pref, 368), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<DATETIME>"));
            frmSelect.Controls.Add(eTools.GetRadioButton("FixedDate", "date", bSel3Checked, eResApp.GetRes(Pref, 369), false, sValue: "<CALENDAR>"));
            if (_from == From.TreatmentFilter || _from == From.Filter)
            {
                //DivDateSelect.Controls.Add(eTools.GetRadioButton("7", "date", bSel7Checked, _Res.GetRes(_pref.Lang, 1234)));
                frmSelect.Controls.Add(eTools.GetRadioButton("Month", "date", bSel4Checked, eResApp.GetRes(Pref, 693), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<MONTH>"));
                frmSelect.Controls.Add(eTools.GetRadioButton("Week", "date", bSel5Checked, eResApp.GetRes(Pref, 694), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<WEEK>"));
                frmSelect.Controls.Add(eTools.GetRadioButton("Year", "date", bSel6Checked, eResApp.GetRes(Pref, 778), sJSOnClick: "nsPickADate.onSelectRadio(this)", sValue: "<YEAR>"));
            }
            //else
            //{
            //    frmSelect.Controls.Add(eTools.GetEmptyInput("4"));
            //    frmSelect.Controls.Add(eTools.GetEmptyInput("5"));
            //    frmSelect.Controls.Add(eTools.GetEmptyInput("6"));
            //}



            // Décalage
            HtmlGenericControl divMoveDiv = new HtmlGenericControl("div");
            _pnDivDateSelect.Controls.Add(divMoveDiv);
            divMoveDiv.Style.Add(HtmlTextWriterStyle.Display, bMoveVisible ? "block" : "none");
            divMoveDiv.ID = "DivMove";
            HtmlGenericControl pnMoveLbl = new HtmlGenericControl("div");
            pnMoveLbl.Attributes.Add("class", "LblMove");
            divMoveDiv.Controls.Add(pnMoveLbl);
            pnMoveLbl.InnerText = eResApp.GetRes(Pref, 848);

            DropDownList ddlMoveLst = new DropDownList();
            ddlMoveLst.Attributes.Add("class", "LblMove");
            divMoveDiv.Controls.Add(ddlMoveLst);
            ddlMoveLst.ID = "lstMove";

            for (int i = -nRange; i <= nRange; i++)
            {

                Boolean bSelect = false;
                String strSign = i > 0 ? "+" : (i < 0 ? "-" : String.Empty);


                if (String.IsNullOrEmpty(strChange) && i == 0)
                    bSelect = true;
                if (strSign + Math.Abs(i) == strChange)
                    bSelect = true;

                ListItem li = new ListItem(String.Concat(strSign, ' ', Math.Abs(i)));
                ddlMoveLst.Items.Add(li);
                li.Selected = bSelect;

            }

            HtmlGenericControl moveLabel = new HtmlGenericControl("span");
            moveLabel.ID = "LabelMove";
            moveLabel.Attributes.Add("class", "LblMove");
            moveLabel.InnerText = strLabelMove;
            divMoveDiv.Controls.Add(moveLabel);


            if (_from != From.DefaultValue)
            {
                HtmlGenericControl noyearDiv = new HtmlGenericControl("div");
                noyearDiv.ID = "DivNoYear";


                divMoveDiv.Controls.Add(noyearDiv);

                noyearDiv.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(Pref, 1496), "ChkNoYear", bNoYear, false, string.Empty, "onCheckOption"));
            }

            //Calendar
            if (_sDate.Trim().Length == 0)
                _sDate = DateTime.Now.ToString("dd/MM/yyyy");

            String strHour = "00";
            String strMin = "00";

            if (bFixedDate)
            {
                //La date récupérée est systématiquement au format fr : dd/MM/yyyy 
                // il faut donc "forcer" le parse à utiliser la culture fr-FR
                int nHour = DateTime.Parse(_sDate, CultureInfo.CreateSpecificCulture("fr-Fr")).Hour;
                int nMin = DateTime.Parse(_sDate, CultureInfo.CreateSpecificCulture("fr-Fr")).Minute;


                strHour = nHour.ToString().PadLeft(2);
                strMin = nMin.ToString().PadLeft(2);
            }
            //if (nHour + nMin != 0)
            //{
            //    if (nHour < 10)
            //        strHour = string.Concat("0", nHour);
            //    else
            //        strHour = nHour.ToString();

            //    if (nMin < 10)
            //        strMin = "0" + nMin;
            //    else
            //        strMin = nMin.ToString();

            //} 

            HtmlInputHidden input = new HtmlInputHidden();
            _divHidden.Controls.Add(input);
            input.ID = "date";
            input.Value = _sDate;

            input = new HtmlInputHidden();
            _divHidden.Controls.Add(input);
            input.ID = "userdate";
            input.Value = DateTime.Now.ToString("dd/MM/yyyy");

            input = new HtmlInputHidden();
            _divHidden.Controls.Add(input);
            input.ID = "hour";
            input.Value = strHour;

            input = new HtmlInputHidden();
            _divHidden.Controls.Add(input);
            input.ID = "min";
            input.Value = strMin;



            if (_from == From.TreatmentFilter || _from == From.Formular)
            {
                tdWizSep.Style.Add("display", "none");
                tdWizDecal.Style.Add("display", "none");
            }

            return true;
        }

    }
}