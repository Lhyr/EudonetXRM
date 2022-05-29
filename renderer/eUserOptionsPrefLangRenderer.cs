using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Renderer d'options utilisateur - Choix de la langue
    /// </summary>
    public class eAdminUsrOptLangRenderer : eUserOptionsRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminUsrOptLangRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_LANGUAGE)
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

            HtmlGenericControl pnlTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
            pnlTitle.InnerText = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_LANGUAGE, Pref);

            HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            pnlSubTitle.InnerText = eResApp.GetRes(Pref, 6776); // Modifier votre mot de passe :

            _pgContainer.Controls.Add(pnlTitle);
            pnlContents.Controls.Add(pnlSubTitle);

            HtmlSelect selectLngUser = new HtmlSelect();
            selectLngUser.ID = "lguser";
            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";
            eButtonCtrl btn = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, "setLng();"); // Valider

            Dictionary<String, String> aa = eDataTools.GetUsersLang(Pref);
            selectLngUser.Items.Clear();

            foreach (KeyValuePair<String, string> kv in aa)
                selectLngUser.Items.Add(new ListItem(kv.Value, kv.Key.ToString()));


            if (selectLngUser.Items.FindByValue(Pref.LangId.ToString()) != null)
                selectLngUser.Items.FindByValue(Pref.LangId.ToString()).Selected = true;

            btnPart.Controls.Add(btn);

            pnlContents.Controls.Add(selectLngUser);
            pnlContents.Controls.Add(btnPart);

            _pgContainer.Controls.Add(pnlContents);

            return true;
        }
    }
}