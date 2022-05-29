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
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public abstract class eAbstractMenuWidgetParamRenderer : eAbstractMenuRenderer
    {
        /// <summary>
        /// The widget parameters
        /// </summary>
        protected eXrmWidgetParam _widgetParams;

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>    
        public eAbstractMenuWidgetParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param, eXrmWidgetContext context) : base(isVisible, file, context)
        {
            _tab = (int)TableType.XRMWIDGET;
            Pref = pref;
            _widgetParams = param;
        }

        /// <summary>
        ///  Construit le menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            if (_file != null)
            {
                // Ajout de type
                XrmWidgetType type;
                eFieldRecord field = _file.GetField((int)XrmWidgetField.Type);
                if (Enum.TryParse(field.Value, out type))
                    field.DisplayValue = eXrmWidgetTools.GetRes(Pref, type);
                else
                    field.DisplayValue = "Widget";

                RenderMenu("paramTab2", eResApp.GetRes(Pref, 8117), field.DisplayValue);
            }
            else
                RenderMenu("paramTab2", eResApp.GetRes(Pref, 8118), eResApp.GetRes(Pref, 8119));
            return true;
        }


        /// <summary>
        /// recupère le contenu du menu
        /// </summary>
        /// <returns></returns>
        protected override void RenderContentMenu(Panel paramBlockContent)
        {
            if (_file == null)
                return;

            paramBlockContent.Controls.Add(RenderMenuItem(0, eResApp.GetRes(Pref, 6809), true, BuildTitlePart()));
            paramBlockContent.Controls.Add(RenderMenuItem(2, eResApp.GetRes(Pref, 7708), false, BuildWidgetUserPref()));
            paramBlockContent.Controls.Add(RenderMenuItem(3, eResApp.GetRes(Pref, 1108), false, BuildWidgetContentPart()));
            // langues et paramètres régionaux
            paramBlockContent.Controls.Add(RenderMenuItem(4, eResApp.GetRes(Pref, 6816), false, BuildTranslationsPart()));
        }

        /// <summary>
        /// Construit un block widget
        /// </summary>
        /// <returns></returns>
        protected virtual Control BuildTitlePart()
        {
            String error = string.Empty;
            String valueTitle = null, valueSubtitle = null, valueTooltip = null;

            // Récupération des RES
            List<eSqlResFiles> listRes = eSqlResFiles.LoadRes(Pref, new List<int> { (int)XrmWidgetField.Title, (int)XrmWidgetField.SubTitle, (int)XrmWidgetField.Tooltip }, _file.FileId, Pref.LangId, out error);
            if (String.IsNullOrEmpty(error))
            {
                eSqlResFiles res = listRes.Find(r => r.DescID == (int)XrmWidgetField.Title);
                if (res != null)
                    valueTitle = res.Value;
                res = listRes.Find(r => r.DescID == (int)XrmWidgetField.SubTitle);
                if (res != null)
                    valueSubtitle = res.Value;
                res = listRes.Find(r => r.DescID == (int)XrmWidgetField.Tooltip);
                if (res != null)
                    valueTooltip = res.Value;
            }


            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "field-container");


            // Ajout de type
            XrmWidgetType type;
            eFieldRecord field = _file.GetField((int)XrmWidgetField.Type);
            if (Enum.TryParse(field.Value, out type))
                field.DisplayValue = eXrmWidgetTools.GetRes(Pref, type);
            field.RightIsUpdatable = false;
            // container.Controls.Add(BuildInputField("Type", field));


            // Ajout du titre
            container.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8120), _file.GetField((int)XrmWidgetField.Title), value: valueTitle, toBeTranslated: true));

            // Ajout de sous titre
            container.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 7109), _file.GetField((int)XrmWidgetField.SubTitle), value: valueSubtitle, toBeTranslated: true));

            // Ajout de l'infobulle
            container.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8121), _file.GetField((int)XrmWidgetField.Tooltip), value: valueTooltip, toBeTranslated: true));


            // Ajout de pictogramme            
            if (Enum.TryParse(field.Value, out type) && type == XrmWidgetType.Tuile)
                container.Controls.Add(BuildPictoField(eResApp.GetRes(Pref, 7535), "", _file.GetField((int)XrmWidgetField.PictoIcon), _file.GetField((int)XrmWidgetField.PictoColor), false));

            //Afficher l entete
            container.Controls.Add(BuildYesNoOptionField(eResApp.GetRes(Pref, 8122), _file.GetField((int)XrmWidgetField.ShowHeader), ""));

            //Afficher la toolbar
            container.Controls.Add(BuildYesNoOptionField(eResApp.GetRes(Pref, 8151), _file.GetField((int)XrmWidgetField.ShowWidgetToolbar), ""));


            return container;

        }

        /// <summary>
        /// Construit le block des préférences utilisateurs
        /// </summary>
        /// <returns></returns>
        private Control BuildWidgetUserPref()
        {
            eFieldRecord field = _file.GetField((int)XrmWidgetField.DisplayOption);
            if (field.Value == "")
                field.Value = ((int)WidgetDisplayOption.DEFAULT_VISIBLE).ToString();


            HtmlGenericControl container = new HtmlGenericControl("div");
            container.Attributes.Add("class", "field-container");
            container.Controls.Add(BuildYesNoOptionField(eResApp.GetRes(Pref, 8123), _file.GetField((int)XrmWidgetField.Resize), ""));
            container.Controls.Add(BuildYesNoOptionField(eResApp.GetRes(Pref, 8124), _file.GetField((int)XrmWidgetField.Move), ""));
            container.Controls.Add(BuildSelectOptionField(eResApp.GetRes(Pref, 8129), "", field, GetOptionList(), ""));

            return container;
        }

        /// <summary>
        ///  retourne les données pour remplir le select
        /// </summary>
        /// <returns></returns>
        private List<Tuple<string, string>> GetOptionList()
        {
            List<Tuple<string, string>> data = new List<Tuple<string, string>>();
            data.Add(new Tuple<string, string>(((int)WidgetDisplayOption.ALWAYS_VISIBLE).ToString(), eResApp.GetRes(Pref, 8125)));
            data.Add(new Tuple<string, string>(((int)WidgetDisplayOption.DEFAULT_VISIBLE).ToString(), eResApp.GetRes(Pref, 8126)));
            data.Add(new Tuple<string, string>(((int)WidgetDisplayOption.DEFAULT_HIDDEN).ToString(), eResApp.GetRes(Pref, 8127)));

            return data;
        }

        /// <summary>
        /// Construction des propriétés d'actualisation du widget
        /// </summary>
        /// <returns></returns>
        protected Control BuildRefreshPropPart()
        {
            return BuildYesNoOptionField(eResApp.GetRes(Pref, 8002), _file.GetField((int)XrmWidgetField.ManuelRefresh), null,
            eResApp.GetRes(Pref, 8008),
            eResApp.GetRes(Pref, 8009),
            eResApp.GetRes(Pref, 8010),
            eResApp.GetRes(Pref, 8011),
            true);
        }

        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected abstract Control BuildWidgetContentPart();

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