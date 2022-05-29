using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu de la modale de traitement des droits
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminRenderer" />
    public class eAdminRightsTreatmentDialogRenderer : eAdminRenderer
    {
        private eAdminRightsTreatmentDialogRenderer(ePref pref)
        {
            Pref = pref;
        }

        /// <summary>
        /// Creates the admin rights treatment dialog renderer.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRightsTreatmentDialogRenderer CreateAdminRightsTreatmentDialogRenderer(ePref pref)
        {
            return new eAdminRightsTreatmentDialogRenderer(pref);
        }

        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.ID = "rightsTreatmentModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = eResApp.GetRes(Pref, 8064);

            _pgContainer.Controls.Add(p);

            // TODO : Refactoriser et modifier 
            Boolean bChecked = false;
            Boolean bDisabled = false;
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlTraitLevel";
            ddl.CssClass = "selectionField";
            ddl.Items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 199), " 1"), "1"));
            ddl.Items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 199), " 2"), "2"));
            ddl.Items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 199), " 3"), "3"));
            ddl.Items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 199), " 4"), "4"));
            ddl.Items.Add(new ListItem(String.Concat(eResApp.GetRes(Pref, 199), " 5"), "5"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 8060), UserLevel.LEV_USR_ADMIN.GetHashCode().ToString()));
            if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 7559), ((int)UserLevel.LEV_USR_SUPERADMIN).ToString()));
            if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_PRODUCT)
                ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 8324), ((int)UserLevel.LEV_USR_PRODUCT).ToString()));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 8059), "7"));
            ddl.Enabled = false;
            _pgContainer.Controls.Add(BuildCheckBox("chkLevel", eResApp.GetRes(Pref, 8061), bChecked, bDisabled, ddl));

            TextBox lbUsers = new TextBox();
            lbUsers.ID = "lbTraitUsers";
            lbUsers.Attributes.Add("class", "selectionField");
            lbUsers.Attributes.Add("active", "0");
            lbUsers.Attributes.Add("onclick", "nsAdminRights.showUsersCatInTreatment(this);");
            lbUsers.Text = eResApp.GetRes(Pref, 141);
            lbUsers.ReadOnly = true;
            _pgContainer.Controls.Add(BuildCheckBox("chkGroupsAndUsers", eResApp.GetRes(Pref, 8062), bChecked, bDisabled, lbUsers));

            p = new HtmlGenericControl("p");
            p.Attributes.Add("class", "warning");
            p.InnerHtml = String.Concat("<span class='icon-warning'></span>", eResApp.GetRes(Pref, 8063));

            _pgContainer.Controls.Add(p);

            return base.Build();
        }

        /// <summary>Création de la ligne de Checkbox</summary>
        /// <param name="idCheckbox">The identifier checkbox.</param>
        /// <param name="label">The label.</param>
        /// <param name="bChecked">if set to <c>true</c> [b checked].</param>
        /// <param name="bDisabled">if set to <c>true</c> [b disabled].</param>
        /// <param name="control">Contrôle à ajouter</param>
        /// <returns></returns>
        private Panel BuildCheckBox(String idCheckbox, String label, Boolean bChecked, Boolean bDisabled, Control control)
        {
            Panel panel = new Panel();
            panel.CssClass = "field";

            Panel chkField = new Panel();
            chkField.CssClass = "checkboxField";

            chkField.Attributes.Add("onclick", "nsAdminRights.activeTraitField(this);");

            eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(bChecked, bDisabled);
            chkCtrl.ID = idCheckbox;
            chkCtrl.AddClick();
            chkCtrl.AddText(label);

            chkField.Controls.Add(chkCtrl);

            panel.Controls.Add(chkField);
            panel.Controls.Add(control);

            return panel;
        }
    }
}