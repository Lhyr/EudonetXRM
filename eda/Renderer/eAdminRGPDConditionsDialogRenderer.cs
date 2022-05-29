using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe Renderer de la popup des conditions RGPD
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRenderer" />
    public class eAdminRGPDConditionsDialogRenderer : eAdminRenderer
    {
        #region Propriétés
        RGPDRuleType _ruleType;
        string _tabLabel = string.Empty;
        bool _ruleActive = false;
        int _nbMonths = 0;
        #endregion


        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminRGPDConditionsDialogRenderer(ePref pref, int tab, RGPDRuleType ruleType)
        {
            Pref = pref;
            _ruleType = ruleType;
            _tab = tab;
        }

        /// <summary>
        /// Creates the admin RGPD conditions dialog renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="ruleType">Type of the rule.</param>
        /// <returns></returns>
        public static eAdminRGPDConditionsDialogRenderer CreateAdminRGPDConditionsDialogRenderer(ePref pref, int tab, RGPDRuleType ruleType)
        {
            return new eAdminRGPDConditionsDialogRenderer(pref, tab, ruleType);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                eudoDAL eDal = null;
                try
                {
                    eDal = eLibTools.GetEudoDAL(this.Pref);
                    eDal.OpenDatabase();

                    // Libellé de la table
                    _tabLabel = eLibTools.GetFullNameByDescId(eDal, _tab, Pref.Lang);

                    // Chargement des params de la règle
                    eRGPDRuleParam ruleParams = eRGPDRuleParam.LoadRGPDRuleParam(eDal, _tab, _ruleType);
                    if (ruleParams != null)
                    {
                        _ruleActive = ruleParams.RuleActive;
                        _nbMonths = ruleParams.RuleNbMonths;
                    }
                }
                finally
                {
                    if (eDal != null)
                        eDal.CloseDatabase();
                }


                return true;
            }
            return false;
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {



            this._pgContainer.CssClass = "adminModalContent";
            this._pgContainer.ID = "rgpdConditionsAdminModalContent";



            #region Champs cachés
            _divHidden = new HtmlGenericControl("div");
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            this._pgContainer.Controls.Add(_divHidden);

            HtmlInputHidden hidRuleType = new HtmlInputHidden();
            hidRuleType.ID = "hidRuleType";
            hidRuleType.Value = ((int)_ruleType).ToString();
            hidRuleType.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.RGPDRULEPARAM, "|", (int)eLibConst.RGPDRuleParam.RGPDType));
            _divHidden.Controls.Add(hidRuleType);
            #endregion

            #region Titre
            Panel panel = new Panel();
            panel.CssClass = "edaFormulaField";

            string titleName = "edaFormulaTitle";
            // Titre
            HtmlGenericControl titleLabel = new HtmlGenericControl("label");
            titleLabel.Attributes.Add("for", titleName);
            titleLabel.Attributes.Add("class", titleName);
            titleLabel.InnerText = String.Concat(eResApp.GetRes(Pref, 7216), " :");
            panel.Controls.Add(titleLabel);
            // Textbox
            TextBox titleTextbox = new TextBox();
            titleTextbox.Attributes.Add("name", titleName);
            titleTextbox.CssClass = titleName;
            titleTextbox.Enabled = false;

            switch(_ruleType)
            {
                case RGPDRuleType.Pseudonym:
                    titleTextbox.Text = eResApp.GetRes(Pref, 8511);
                    break;
                case RGPDRuleType.PJDeletion:
                    titleTextbox.Text = eResApp.GetRes(Pref, 8566);
                    break;
                default:
                    titleTextbox.Text = eResApp.GetRes(Pref, 8355);
                    break;
            }                
            panel.Controls.Add(titleTextbox);

            _pgContainer.Controls.Add(panel);
            #endregion

            #region Introduction
            panel = new Panel();
            panel.CssClass = "edaFormulaField";
            HtmlGenericControl p = new HtmlGenericControl();

            switch(_ruleType)
            {
                case RGPDRuleType.Archiving:
                    p.InnerText = String.Format(eResApp.GetRes(Pref, 8356), _tabLabel);
                    break;
                case RGPDRuleType.Deleting:
                    p.InnerText = String.Format(eResApp.GetRes(Pref, 8357), _tabLabel);
                    break;
                case RGPDRuleType.Pseudonym:
                    p.InnerText = String.Format(eResApp.GetRes(Pref, 8512), _tabLabel);
                    break;
                case RGPDRuleType.PJDeletion:
                    p.InnerText = String.Format(eResApp.GetRes(Pref, 8568), _tabLabel);
                    break;
            }            
            panel.Controls.Add(p);
            _pgContainer.Controls.Add(panel);
            #endregion

            #region La règle est actuellement active/inactive

            panel = new Panel();
            panel.CssClass = "edaFormulaField";
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("class", "edaFormulaActive");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 8358), " : ");

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlActive";
            ddl.CssClass = "edaFormulaActive";
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 6672), "1"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 8359), "0"));
            ddl.SelectedValue = _ruleActive ? "1" : "0";
            ddl.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.RGPDRULEPARAM, "|", (int)eLibConst.RGPDRuleParam.Active));
            panel.Controls.Add(label);
            panel.Controls.Add(ddl);
            _pgContainer.Controls.Add(panel);

            #endregion

            #region Param
            panel = new Panel();
            panel.CssClass = "paramStep active";
            p = new HtmlGenericControl();
            p.Attributes.Add("class", "stepTitle");
            p.InnerText = eResApp.GetRes(Pref, 8360);
            panel.Controls.Add(p);
            _pgContainer.Controls.Add(panel);

            panel = new Panel();
            panel.CssClass = "stepContent";
            p = new HtmlGenericControl();
            p.Attributes.Add("class", "stepTitle");
            switch(_ruleType)
            {
                case RGPDRuleType.Pseudonym:
                    p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8514)));
                    break;
                case RGPDRuleType.PJDeletion:
                    p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8569)));
                    break;
                default:
                    p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8361)));
                    break;
            }       
            TextBox tb = new TextBox();
            tb.ID = "txtNbMonths";
            tb.CssClass = "txtNbMonths";
            tb.Text = (_nbMonths == 0) ? string.Empty : _nbMonths.ToString();
            tb.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.RGPDRULEPARAM, "|", (int)eLibConst.RGPDRuleParam.NbMonthsDeadline));
            p.Controls.Add(tb);
            p.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 854).ToLower()));
            panel.Controls.Add(p);
            _pgContainer.Controls.Add(panel);
            #endregion

            return true;
        }
    }
}