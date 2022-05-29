using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet qui permet d'ajouter la barre d'outils
    /// </summary>
    public class eXrmWidgetWithToolbar : eXrmWidgetDecorator
    {

        bool _adminMode;
        ePref _pref;
        List<eSqlResFiles> _listRes;

        public eXrmWidgetWithToolbar(ePref pref, IXrmWidgetUI XrmWidgetUI, bool adminMode) : base(XrmWidgetUI)
        {
            _adminMode = adminMode;
            _pref = pref;
        }

        /// <summary>
        /// Permet d'initialiser le renderer du widget
        /// Ne pas oublier d'appeler la méthode de base lors d'un override
        /// </summary>
        /// <param name="widgetRecord">un enregistrement de widget</param>
        /// <param name="widgetPref">Préférences utilisateur pour le widget</param>
        /// <param name="widgetParam">Paramètres du widget</param>
        /// <param name="widgetContext">The widget context.</param>
        public override void Init(eRecord widgetRecord, eXrmWidgetPref widgetPref, eXrmWidgetParam widgetParam, eXrmWidgetContext widgetContext)
        {
            base.Init(widgetRecord, widgetPref, widgetParam, widgetContext);

            if (widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ShowHeader)).Value == "1")
            {
                string error;
                _listRes = eSqlResFiles.LoadRes(_pref, new List<int> { (int)XrmWidgetField.Title, (int)XrmWidgetField.SubTitle }, widgetRecord.MainFileid, _pref.LangId, out error);
            }
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {


            // TODO TEST les droits
            HtmlGenericControl toolbar = new HtmlGenericControl("div");
            toolbar.Attributes.Add("class", "xrm-widget-toolbar");
            widgetContainer.Controls.Add(toolbar);

            // TODO TEST les droits déplacement
            HtmlGenericControl dragbar = new HtmlGenericControl("div");

            eFieldRecord fieldMove = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.Move));
            eFieldRecord fieldToolbar = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ShowWidgetToolbar));
            if (fieldMove.Value == "1" || fieldMove.Value.ToLower() == "true")
            {
                dragbar.Attributes.Add("class", "widgetDragBar");
                toolbar.Controls.Add(dragbar);
            }

            if (_adminMode || fieldToolbar.Value == "1")
            {


                // TODO TEST le mode param
                HtmlGenericControl options = new HtmlGenericControl("ul");
                options.Attributes.Add("class", "widgetOptions selected");
                options.Attributes.Add("admin", _adminMode ? "1" : "0");
                toolbar.Controls.Add(options);

                // Type de widget
                eFieldRecord fieldType = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.Type));

                if (fieldType.Value == XrmWidgetType.Editor.GetHashCode().ToString())
                {
                    #region Edition d'un widget type Editeur
                    if (_adminMode)
                    {
                        HtmlGenericControl edit = new HtmlGenericControl("li");
                        edit.ID = "xrm-widget-edit";
                        edit.Attributes.Add("class", "icon-edit background-theme");
                        options.Controls.Add(edit);
                    }
                    #endregion

                }
                else if (fieldType.Value == XrmWidgetType.WebPage.GetHashCode().ToString() || fieldType.Value == XrmWidgetType.Specif.GetHashCode().ToString())
                {
                    #region Ouverture de la page web dans un nouvel onglet
                    if (!_adminMode)
                    {
                        HtmlGenericControl open = new HtmlGenericControl("li");
                        open.ID = "xrm-widget-openlink";
                        open.Attributes.Add("class", "icon-external-link");
                        open.Attributes.Add("title", eResApp.GetRes(_pref, 8070));
                        options.Controls.Add(open);
                    }
                    #endregion
                }



                if (_adminMode)
                {
                    // TODO TEST le mode config
                    HtmlGenericControl reload = new HtmlGenericControl("li");
                    reload.ID = "xrm-widget-reload";
                    reload.Attributes.Add("class", "icon-refresh background-theme");
                    options.Controls.Add(reload);


                    //Zoom non fonctionnel
                    if (fieldType.Value == XrmWidgetType.Image.GetHashCode().ToString()
                        || fieldType.Value == XrmWidgetType.List.GetHashCode().ToString())
                    {
                        /*
                        HtmlGenericControl zoom = new HtmlGenericControl("li");
                        zoom.ID = "xrm-widget-zoom";
                        zoom.Attributes.Add("class", "icon-search-plus background-theme");
                        options.Controls.Add(zoom);
                        */
                    }

                    // TODO TEST le mode config
                    HtmlGenericControl config = new HtmlGenericControl("li");
                    config.ID = "xrm-widget-config";
                    config.Attributes.Add("class", "icon-cog background-theme");
                    options.Controls.Add(config);

                    //déssociation
                    /*
                    HtmlGenericControl unlink = new HtmlGenericControl("li");
                    unlink.ID = "xrm-widget-unlink";
                    unlink.Attributes.Add("class", "icon-unlink background-theme");
                    options.Controls.Add(unlink);
                    */

                    HtmlGenericControl delete = new HtmlGenericControl("li");
                    delete.ID = "xrm-widget-delete";
                    delete.Attributes.Add("class", "deleteOption icon-delete");
                    options.Controls.Add(delete);
                }
                else
                {

                    // TODO TEST le mode config
                    HtmlGenericControl reload = new HtmlGenericControl("li");
                    reload.ID = "xrm-widget-reload";
                    reload.Attributes.Add("class", "icon-refresh");
                    options.Controls.Add(reload);

                    //Zoom non fonctionnel
                    if (fieldType.Value == XrmWidgetType.Image.GetHashCode().ToString()
                        || fieldType.Value == XrmWidgetType.List.GetHashCode().ToString())
                    {
                        /*
                        HtmlGenericControl zoom = new HtmlGenericControl("li");
                        zoom.ID = "xrm-widget-zoom";
                        zoom.Attributes.Add("class", "icon-search-plus");
                        options.Controls.Add(zoom);
                        */
                    }
                }
            }


            if (widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ShowHeader)).Value == "1")
            {
                string titleValue = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.Title)).DisplayValue;
                string subtitleValue = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.SubTitle)).DisplayValue;
                if (_listRes != null)
                {
                    eSqlResFiles res = _listRes.Find(r => r.DescID == (int)XrmWidgetField.Title);
                    if (res != null && !String.IsNullOrEmpty(res.Value))
                        titleValue = res.Value;

                    res = _listRes.Find(r => r.DescID == (int)XrmWidgetField.SubTitle);
                    if (res != null && !String.IsNullOrEmpty(res.Value))
                        subtitleValue = res.Value;
                }

                HtmlGenericControl titleBar = new HtmlGenericControl("div");
                titleBar.Attributes.Add("class", "xrm-widget-titlebar");

                // TODO TEST les droits déplacement
                HtmlGenericControl widgetTitle = new HtmlGenericControl("div");
                widgetTitle.ID = "wid_" + (int)TableType.XRMWIDGET + "_" + (int)XrmWidgetField.Title + "_" + widgetRecord.MainFileid;
                widgetTitle.Attributes.Add("class", "xrm-widget-title");
                widgetTitle.InnerText = titleValue;
                titleBar.Controls.Add(widgetTitle);

                // Sous-titre
                HtmlGenericControl subTitle = new HtmlGenericControl("div");
                subTitle.Attributes.Add("class", "xrm-widget-subtitle");
                subTitle.InnerText = subtitleValue;
                titleBar.Controls.Add(subTitle);

                widgetContainer.Controls.Add(titleBar);

                // Demande #71 615 - Ajout d'un attribut pour indiquer qu'il n'y a pas d'entête, et adapter les styles en fonction
                widgetContainer.Attributes.Add("xrm-widget-header", "1");
            }
            else
            {
                // Demande #71 615 - Ajout d'un attribut pour indiquer qu'il n'y a pas d'entête, et adapter les styles en fonction
                widgetContainer.Attributes.Add("xrm-widget-header", "0");
            }

            // Demande #71 615 - Ajout d'un attribut pour indiquer si on est en admin ou non, et adapter les styles en fonction
            widgetContainer.Attributes.Add("admin", _adminMode ? "1" : "0");

            // on construit d'abord le contenu puis on ajout la barre d'outils
            widgetUI.Build(widgetContainer);

        }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            widgetUI.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            widgetUI.AppendStyle(styleBuilder);
        }
    }
}