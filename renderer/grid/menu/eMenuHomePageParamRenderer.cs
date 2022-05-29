using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le contenu de la page d'accueil
    /// </summary>
    public class eMenuHomePageParamRenderer : eAbstractMenuRenderer
    {

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public eMenuHomePageParamRenderer(ePref pref, bool isVisible, eFile file = null, eXrmWidgetContext context = null) : base(isVisible, file, context)
        {
            _tab = (int)TableType.XRMHOMEPAGE;
            Pref = pref;
        }


        /// <summary>
        ///  Construit le menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            // Paramètres de la grille
            RenderMenu("paramTab3", eResApp.GetRes(Pref, 8158), eResApp.GetRes(Pref, 8150));

            return true;
        }

        /// <summary>
        /// On fait le rendu des items du menu dans le container
        /// </summary>
        /// <param name="paramBlockContent"></param>
        protected override void RenderContentMenu(Panel menuContainer)
        {
            if (_file == null)
                return;

            // caractéristiques
            menuContainer.Controls.Add(RenderMenuItem(0, eResApp.GetRes(Pref, 6809), true, BuildGridPropPart()));

            // Droits
            menuContainer.Controls.Add(RenderMenuItem(1, eResApp.GetRes(Pref, 7406), false, BuildPermissionPart()));

            // Traductions
            menuContainer.Controls.Add(RenderMenuItem(1, eResApp.GetRes(Pref, 6816), true, BuildTranslationsPart()));

        }

        /// <summary>
        /// Construit un block de caractéristiques
        /// </summary>
        /// <returns></returns>
        private Control BuildGridPropPart()
        {
            String error = string.Empty;
            String valueTitle = null, valueTooltip = null;

            // Récupération des RES
            List<eSqlResFiles> listRes = eSqlResFiles.LoadRes(Pref, new List<int> { (int)XrmHomePageField.Title, (int)XrmHomePageField.Tooltip }, _file.FileId, Pref.LangId, out error);
            if (String.IsNullOrEmpty(error))
            {
                eSqlResFiles res = listRes.Find(r => r.DescID == (int)XrmHomePageField.Title);
                if (res != null)
                    valueTitle = res.Value;
                res = listRes.Find(r => r.DescID == (int)XrmHomePageField.Tooltip);
                if (res != null)
                    valueTooltip = res.Value;
            }

            HtmlGenericControl fieldContainer = new HtmlGenericControl("div");
            fieldContainer.Attributes.Add("class", "field-container");

            fieldContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8159), _file.GetField((int)XrmHomePageField.Title), value: valueTitle, toBeTranslated: true));
            fieldContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 7557), _file.GetField((int)XrmHomePageField.Tooltip), value: valueTooltip, toBeTranslated: true));


            return fieldContainer;
        }



        /// <summary>
        /// Construit un block de permissions
        /// </summary>
        /// <returns></returns>
        private Control BuildPermissionPart()
        {
            eFieldRecord field = _file.GetField((int)XrmGridField.ViewPermId);
            return BuildBtnField(eResApp.GetRes(Pref, 7406), String.Empty, field, "oGridController.config.showViewPerm(" + (int)TableType.XRMHOMEPAGE + ", " + _file.FileId + ", 0);");
        }

        private Control BuildTranslationsPart()
        {
            return BuildBtnField(eResApp.GetRes(Pref, 7716), eResApp.GetRes(Pref, 7362), null, String.Format("nsAdmin.openTranslations({0}, '9', null, {1});", _tab, _file.FileId));
        }
    }
}