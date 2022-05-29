using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
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
    public class eMenuGridParamRenderer : eAbstractMenuRenderer
    {


        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>     
        public eMenuGridParamRenderer(ePref pref, bool isVisible, eFile file = null, eXrmWidgetContext context = null) : base(isVisible, file, context)
        {
            _tab = (int)TableType.XRMGRID;
            Pref = pref;
        }


        /// <summary>
        ///  Construit le menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            // Paramètres de la grille
            RenderMenu("paramTab3", eResApp.GetRes(Pref, 8368), eResApp.GetRes(Pref, 8150)); // "Paramètres de la grille", "Positionnée dans l'onglet accueil"

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

            // Onglet parent
            menuContainer.Controls.Add(RenderMenuItem(1, eResApp.GetRes(Pref, 7251), false, BuildParentTabPart()));

            // permissions
            menuContainer.Controls.Add(RenderMenuItem(2, eResApp.GetRes(Pref, 7406), false, BuildPermissionPart()));

            // langues et paramètres régionaux
            menuContainer.Controls.Add(RenderMenuItem(2, eResApp.GetRes(Pref, 6816), false, BuildTranslationsPart()));
        }

        /// <summary>
        /// Construit un block de caractéristiques
        /// </summary>
        /// <returns></returns>
        private Control BuildGridPropPart()
        {

            String error = string.Empty;
            int descidTitle = (int)XrmGridField.Title;
            String valueTitle = null;

            // Récupération de la RES du nom de la grille
            List<eSqlResFiles> listRes = eSqlResFiles.LoadRes(Pref, new List<int> { descidTitle }, _file.FileId, Pref.LangId, out error);
            if (String.IsNullOrEmpty(error))
            {
                eSqlResFiles resTitle = listRes.Find(r => r.DescID == descidTitle);
                if (resTitle != null)
                    valueTitle = resTitle.Value;
            }

            HtmlGenericControl fieldContainer = new HtmlGenericControl("div");
            fieldContainer.Attributes.Add("class", "field-container");
            fieldContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8156), _file.GetField(descidTitle), value: valueTitle, toBeTranslated: true, reloadHeader: true));
            // fieldContainer.Controls.Add(BuildInputField("Infobulle visible au survol", _file.GetField((int)XrmHomePageField.Tooltip)));
            fieldContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 7595), _file.GetField((int)XrmGridField.DisplayOrder), reloadHeader: true));

            return fieldContainer;
        }

        /// <summary>
        /// Construit un block de relations
        /// </summary>
        /// <returns></returns>
        private Control BuildParentTabPart()
        {
            eFieldRecord field = _file.GetField((int)XrmGridField.ParentTab);

            var tabs = eAdminTools.GetListTabsWithIcon(Pref);

            // ajout de l'option accueil
            tabs.Insert(0, new Tuple<int, string, string>(115200, eResApp.GetRes(Pref, 551), "icon-home"));

            //ALISTER => Demande / Request 83750
            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "relationparentContainer");

            HtmlGenericControl iconSpan = new HtmlGenericControl("span");
            iconSpan.ID = "icon-selected-tab";

            HtmlGenericControl textParentGrid;
            string icon = "";
            foreach (var t in tabs)
            {
                //ALISTER Demande / Request #83750 use span instead of select
                textParentGrid = new HtmlGenericControl("span");
                icon = eFontIcons.GetFontClassName(t.Item3);
                textParentGrid.Attributes.Add("class", "relationparent");
                textParentGrid.InnerText = t.Item2.ToString();
                textParentGrid.Attributes.Add("icon", icon);

                if (field.Value == t.Item1.ToString())
                {
                    textParentGrid.Attributes.Add("disabled", "");
                    iconSpan.Attributes.Add("class", "parentIconTab " + icon);
                    container.Controls.Add(textParentGrid);
                }

            }

            container.Controls.Add(iconSpan);
            return container;
        }

        /// <summary>
        /// Construit un block de permissions
        /// </summary>
        /// <returns></returns>
        private Control BuildPermissionPart()
        {
            eFieldRecord perm = _file.GetField((int)XrmGridField.ViewPermId);
            eFieldRecord parentTab = _file.GetField((int)XrmGridField.ParentTab);
            eFieldRecord parentFileId = _file.GetField((int)XrmGridField.ParentFileId);
            if (parentFileId.Value == string.Empty)
                parentFileId.Value = "0";

            return BuildBtnField(eResApp.GetRes(Pref, 7406), String.Empty, perm, string.Concat("oGridController.config.showViewPerm(", parentTab.Value, ",", parentFileId.Value, ",", _file.FileId, ");"));
        }


        /// <summary>
        /// Construit un block de langues et paramètres régionaux
        /// </summary>
        /// <returns></returns>
        private Control BuildTranslationsPart()
        {
            return BuildBtnField(eResApp.GetRes(Pref, 7716), eResApp.GetRes(Pref, 7362), null, String.Format("nsAdmin.openTranslations({0}, '9', null, {1});", _tab, _file.FileId));
        }
    }
}