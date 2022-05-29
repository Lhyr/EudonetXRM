using Com.Eudonet.Internal;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eUserOptionsPrefMruModeRenderer : eUserOptionsRenderer
    {
        public eUserOptionsPrefMruModeRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_MRUMODE)
        {

        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            Panel pnlContents = new Panel();
            pnlContents.ID = "admntCntnt";
            pnlContents.CssClass = "adminCntnt";

            HtmlGenericControl pnlTitle = new HtmlGenericControl("div");
            pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
            pnlTitle.InnerText = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_MRUMODE, Pref);

            HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div");
            pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            pnlSubTitle.InnerText = eResApp.GetRes(Pref, 7985); // Lors de la saisie dans une rubrique catalogue ou relationnelle, souhaitez-vous qu'Eudonet propose les dernières valeurs utilisées (MRU) ?

            _pgContainer.Controls.Add(pnlTitle);
            pnlContents.Controls.Add(pnlSubTitle);

            DropDownList select = new DropDownList();
            select.ID = "ddlMruMode";
            select.Items.Add(new ListItem(eResApp.GetRes(Pref, 58), "1"));
            select.Items.Add(new ListItem(eResApp.GetRes(Pref, 59), "0"));
            select.SelectedValue = Pref.GetConfig(eLibConst.PREF_CONFIG.MRUMODE);

            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";
            eButtonCtrl btn = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, "setMruMode();"); // Valider

            btnPart.Controls.Add(btn);

            pnlContents.Controls.Add(select);
            pnlContents.Controls.Add(btnPart);

            _pgContainer.Controls.Add(pnlContents);

            return true;
        }
    }
}