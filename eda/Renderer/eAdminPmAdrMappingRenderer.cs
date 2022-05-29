using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// renderer du paramétrage du mapping entre les champs de société et d'adresse.
    /// </summary>
    public class eAdminPmAdrMappingRenderer : eAdminRenderer
    {
        private EudoQuery.EudoQuery _queryPM, _queryAdr;
        private ePmAddressMapping _mapping;
        private List<Field> _fldsPM, _fldsAdr;

        private List<FieldFormat> lstExclFieldFormat = new List<FieldFormat>() {FieldFormat.TYP_ALIAS
                                                , FieldFormat.TYP_ALIASRELATION
                                                , FieldFormat.TYP_AUTOINC
                                                , FieldFormat.TYP_CHART
                                                , FieldFormat.TYP_ID
                                                , FieldFormat.TYP_IFRAME
                                                , FieldFormat.TYP_PJ
                                                };

        /// <summary>
        /// indique s'il faut construire tout le corps (false pour rajouter une ligne de correspondance vierge.)
        /// </summary>
        public bool DoBuild = true;

        private eAdminPmAdrMappingRenderer(ePref pref, ePmAddressMapping mapping)
        {
            Pref = pref;
            _mapping = mapping;
        }

        internal static eAdminPmAdrMappingRenderer CreateAdminPmAdrMappingRenderer(ePref pref, ePmAddressMapping mapping)
        {
            return new eAdminPmAdrMappingRenderer(pref, mapping);
        }

        /// <summary>
        /// initialisation des données pour le renderer
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _queryPM = eLibTools.GetEudoQuery(Pref, (int)TableType.PM, ViewQuery.FILE);
            _queryAdr = eLibTools.GetEudoQuery(Pref, (int)TableType.ADR, ViewQuery.FILE);

            // Load la request
            _queryPM.LoadRequest();
            _queryAdr.LoadRequest();

            _fldsAdr = _queryAdr.GetFieldHeaderList;
            _fldsPM = _queryPM.GetFieldHeaderList;

            _fldsAdr = _fldsAdr.OrderBy<Field, string>(f => f.Libelle).Where<Field>(f => f.Table.DescId == (int)TableType.ADR).ToList<Field>();
            _fldsPM = _fldsPM.OrderBy<Field, string>(f => f.Libelle).Where<Field>(f => f.Table.DescId == (int)TableType.PM).ToList<Field>();

            _fldsAdr = _fldsAdr.Where<Field>(f => !lstExclFieldFormat.Contains(f.Format)).ToList<Field>();
            _fldsPM = _fldsPM.Where<Field>(f => !lstExclFieldFormat.Contains(f.Format)).ToList<Field>();

            return true;
        }

        /// <summary>
        /// construction du corps
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            string sPMLabel = _fldsPM[0].Table.Libelle, sAdrLabel = _fldsAdr[0].Table.Libelle;

            _pgContainer.ID = "AdminPmAdrMapping";
            _pgContainer.Attributes.Add("class", "stepContent");

            Panel pnTitle = new Panel();
            pnTitle.CssClass = "title";
            pnTitle.Controls.Add(new LiteralControl(String.Format(eResApp.GetResConcat("", Pref, 1860, 1862, 1863), sPMLabel, sAdrLabel)));
            pnTitle.Controls.Add(new LiteralControl("<br/><br/>" + eResApp.GetRes(Pref, 1865)));
            _pgContainer.Controls.Add(pnTitle);

            try
            {
                _pgContainer.Controls.Add(GetMappingTable());

            }
            catch (Exception e)
            {
                _eException = e;
                _sErrorMsg = "Erreur lors du rendu de la page";
                return false;
            }

            Panel pnAddLine = new Panel();
            pnAddLine.Attributes.Add("onclick", "al();");
            pnAddLine.ID = "divAddLine";
            _pgContainer.Controls.Add(pnAddLine);
            HtmlGenericControl pnIcon = new HtmlGenericControl("span");
            pnAddLine.Controls.Add(pnIcon);
            pnIcon.Attributes.Add("Class", "icon-add");
            pnAddLine.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 1869)));


            pnTitle = new Panel();
            pnTitle.CssClass = "title";
            pnTitle.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 1884)));
            _pgContainer.Controls.Add(pnTitle);

            string sBtnLabel = eResApp.GetRes(Pref, 7494);
            if (FilterTools.GetSpecialFilterId((int)TableType.ADR, TypeFilter.UPD_ADR_FROM_PM, Pref) > 0)
                sBtnLabel += " (1)";


            eAdminButtonField btnFiltre = new eAdminButtonField(sBtnLabel, "CpltFilter", onclick: String.Format("top.nsAdminFile.openSpecFilter({0})", (int)TypeFilter.UPD_ADR_FROM_PM));
            btnFiltre.Generate(_pgContainer);

            Panel pnTemp = new Panel();
            _pgContainer.Controls.Add(pnTemp);
            pnTemp.ID = "divTmp";
            pnTemp.Style.Add(HtmlTextWriterStyle.Display, "none");

            return base.Build();
        }

        /// <summary>
        /// retourne la table html qui permet de modifier le mapping
        /// </summary>
        /// <returns></returns>
        public System.Web.UI.WebControls.Table GetMappingTable()
        {
            System.Web.UI.WebControls.Table tb = new System.Web.UI.WebControls.Table();
            tb.ID = "TbMapping";
            foreach (ePmAddressMapping.FieldMatching match in _mapping.FieldsMatching)
            {
                tb.Rows.Add(GetMatchingLine(match));
            }

            return tb;

        }

        /// <summary>
        /// effectue le rendu pour un ligne de correspondance
        /// </summary>
        /// <param name="matching"></param>
        /// <returns></returns>
        public TableRow GetMatchingLine(ePmAddressMapping.FieldMatching matching)
        {
            DropDownList ddlPM = new DropDownList(), ddlAdr = new DropDownList();
            Field PMField = null, ADRField = null;
            TableRow tr = new TableRow();
            TableCell tcPM = new TableCell(), tcAdr = new TableCell(), tcDel = new TableCell();
            tr.Cells.Add(tcAdr);
            tr.Cells.Add(tcPM);
            tr.Cells.Add(tcDel);

            ddlAdr.Items.Add(new ListItem("", "0"));
            ddlPM.Items.Add(new ListItem("", "0"));

            tcPM.Controls.Add(ddlPM);
            tcAdr.Controls.Add(ddlAdr);

            Panel pnDelMatching = new Panel();
            tcDel.Controls.Add(pnDelMatching);
            pnDelMatching.CssClass = "icon-delete";
            pnDelMatching.Attributes.Add("onclick", "del(this);");
            pnDelMatching.ToolTip = eResApp.GetRes(Pref, 19);

            #region Champs de la table adresse
            foreach (Field fld in _fldsAdr)
            {
                ListItem li = new ListItem(String.Format("{0}.{1}", fld.Table.Libelle, fld.Libelle), fld.Descid.ToString());
                ddlAdr.Items.Add(li);
                ddlAdr.Attributes.Add("onchange", "rfsh();");
                if (fld.Descid == matching.ADRField)
                {
                    li.Selected = true;
                    ADRField = fld;
                }
                else if (_mapping.FieldsMatching.Exists(m => m.ADRField == fld.Descid))
                {
                    li.Attributes.Add("disabled", "");
                }

            }
            #endregion

            #region tri sur la liste des rubriques de société
            List<Field> lstFldPM = _fldsPM;

            if (ADRField?.Format == FieldFormat.TYP_NUMERIC || ADRField?.Format == FieldFormat.TYP_MONEY)
            {
                lstFldPM = lstFldPM.Where<Field>(f => f.Format == FieldFormat.TYP_NUMERIC || f.Format == FieldFormat.TYP_MONEY).ToList<Field>();
            }
            else if (ADRField?.Format == FieldFormat.TYP_BIT || ADRField?.Format == FieldFormat.TYP_BITBUTTON)
            {
                lstFldPM = lstFldPM.Where<Field>(f => f.Format == FieldFormat.TYP_BIT || f.Format == FieldFormat.TYP_BITBUTTON).ToList<Field>();
            }
            else if (ADRField?.Format == FieldFormat.TYP_GEOGRAPHY_V2)
            {
                lstFldPM = lstFldPM.Where<Field>(f => f.Format == FieldFormat.TYP_GEOGRAPHY_V2).ToList<Field>();
            }
            else if (ADRField?.Format == FieldFormat.TYP_USER)
            {
                lstFldPM = lstFldPM.Where<Field>(f => f.Format == FieldFormat.TYP_USER).ToList<Field>();
                if (ADRField.Multiple)
                    lstFldPM = lstFldPM.Where<Field>(f => f.Multiple).ToList<Field>();
            }
            else if (ADRField?.Popup == PopupType.DATA || ADRField?.Popup == PopupType.SPECIAL)
            {
                lstFldPM = lstFldPM.Where<Field>(f => f.Popup == ADRField?.Popup && f.PopupDescId == ADRField?.PopupDescId).ToList<Field>();
            }
            #endregion

            #region liste des rubriques de sociétés
            foreach (Field fld in lstFldPM)
            {
                ListItem li = new ListItem(String.Format("{0}.{1}", fld.Table.Libelle, fld.Libelle), fld.Descid.ToString());
                ddlPM.Items.Add(li);
                ddlPM.Attributes.Add("onchange", "rfsh();");

                if (fld.Descid == matching.PMField)
                {
                    li.Selected = true;
                    PMField = fld;
                }
                else if (_mapping.FieldsMatching.Exists(m => m.PMField == fld.Descid))
                {
                    li.Attributes.Add("disabled", "");
                }
            }

            #endregion

            return tr;
        }
    }
}
