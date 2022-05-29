using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Linq;

namespace Com.Eudonet.Xrm
{
    public class eUserOptionsPrefFontSizeRenderer : eUserOptionsRenderer
    {
        public eUserOptionsPrefFontSizeRenderer(ePref pref)
            : base(pref, eUserOptionsModules.USROPT_MODULE.PREFERENCES_FONTSIZE)
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
            pnlTitle.InnerText = eUserOptionsModules.GetModuleMiddleTitleLabel(eUserOptionsModules.USROPT_MODULE.PREFERENCES_FONTSIZE, Pref);




            HtmlGenericControl pnlSubTitle = new HtmlGenericControl("div"); // pas de InnerText sur un contrôle Panel...
            pnlSubTitle.Attributes.Add("class", "adminCntntTtl ");
            pnlSubTitle.InnerText = eResApp.GetRes(Pref, 7983); // Sélectionner la taille de police de caractère que vous souhaitez utiliser pour votre profil:

            _pgContainer.Controls.Add(pnlTitle);
            pnlContents.Controls.Add(pnlSubTitle);




            HtmlSelect selectFontSize = new HtmlSelect();
            selectFontSize.ID = "ftsize";
            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";
            eButtonCtrl btn = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, "setFontSize();"); // Valider

            //Recuperation des font size
            IEnumerable<int> IEnFontPro = eTools.GetFontSize().OfType<int>();
            //IEnumerable<int> IEnFontThm = Pref.ThemeXRM.FontSizeMax;

            /** Ici on fait l'intersection entre les polices proposées, et si on en a défini, les polices du thème. */
            //IDictionary<string, int> listFontSize = new Dictionary<string, int>();
            //foreach (int fontSize in IEnFontPro)
            //    listFontSize.Add(eTools.GetFontSizeLabel(Pref, fontSize), fontSize);

            IDictionary<string, int> listFontSize = IEnFontPro.ToDictionary(fontSize => eTools.GetFontSizeLabel(Pref, fontSize), fontSize => fontSize);

            selectFontSize.Items.Clear();
            //Binding ddl
            selectFontSize.DataSource = listFontSize;
            selectFontSize.DataTextField = "Key";
            selectFontSize.DataValueField = "Value";
            selectFontSize.DataBind();

            //Récuperation prefFontSize user et selection dans la liste
            string sUserFontSize = eTools.GetUserFontSize(Pref);
            if (selectFontSize.Items.FindByValue(sUserFontSize) != null)
                selectFontSize.Items.FindByValue(sUserFontSize).Selected = true;

            btnPart.Controls.Add(btn);

            pnlContents.Controls.Add(selectFontSize);
            pnlContents.Controls.Add(btnPart);



            _pgContainer.Controls.Add(pnlContents);

            return true;
        }
    }
}