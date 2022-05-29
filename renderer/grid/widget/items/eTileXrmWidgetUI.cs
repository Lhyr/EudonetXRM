using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using static Com.Eudonet.Xrm.eConst;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu du widget TUILE
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eAbstractXrmWidgetUI" />
    public class eTileXrmWidgetUI : eAbstractXrmWidgetUI
    {
        ePref _pref;

        /// <summary>
        /// Initializes a new instance of the <see cref="eTileXrmWidgetUI"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        public eTileXrmWidgetUI(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            int specifID = _widgetParam.GetParamValueInt("specifid");
            if (specifID > 0)
                widgetContainer.Attributes.Add("sid", specifID.ToString());

            Panel wrapper = new Panel();
            wrapper.CssClass = "tileWrapper";
            wrapper.Style.Add("background-color", _widgetParam.GetParamValue("backgroundColor"));

            XrmWidgetTileAction tileAction = (XrmWidgetTileAction)_widgetParam.GetParamValueInt("tileaction");
            wrapper.Attributes.Add("data-action", _widgetParam.GetParamValue("tileaction"));
            if (tileAction == XrmWidgetTileAction.OpenWebpage)
            {
                wrapper.Attributes.Add("data-url", _widgetParam.GetParamValue("url"));
            }
            else if (tileAction == XrmWidgetTileAction.OpenSpecif)
            {
                wrapper.Attributes.Add("data-widgetid", _widgetRecord.MainFileid.ToString());
                wrapper.Attributes.Add("data-specifid", specifID.ToString());

            }
            else if (tileAction == XrmWidgetTileAction.OpenTab)
            {
                int filterID = _widgetParam.GetParamValueInt("filterid");

                wrapper.Attributes.Add("data-tab", _widgetParam.GetParamValue("tab"));
                wrapper.Attributes.Add("data-filterid", filterID.ToString());
            }
            else if (tileAction == XrmWidgetTileAction.CreateFile || tileAction == XrmWidgetTileAction.GoToFile)
            {
                int tab = _widgetParam.GetParamValueInt("tab");
                if (tab > 0)
                {
                    int fileid = _widgetParam.GetParamValueInt("fileid");

                    eAdminTableInfos tl = eAdminTableInfos.GetAdminTableInfos(_pref, tab);

                    wrapper.Attributes.Add("data-tab", tab.ToString());
                    wrapper.Attributes.Add("data-fileid", fileid.ToString());
                    wrapper.Attributes.Add("data-edntype", tl.EdnType.GetHashCode().ToString());

                    if ((XrmWidgetTileCreateFileValidationBehaviour)_widgetParam.GetParamValueInt("fileValidationMode") == XrmWidgetTileCreateFileValidationBehaviour.StayOnGrid)
                        wrapper.Attributes.Add("data-noloadfile", "1");

                    if (tileAction == XrmWidgetTileAction.GoToFile)
                        wrapper.Attributes.Add("data-openmode", _widgetParam.GetParamValue("fileOpenMode"));
                }


            }

            widgetContainer.Controls.Add(wrapper);


            Panel content = new Panel();
            content.CssClass = "tileContent";
            wrapper.Controls.Add(content);

            // Affichage picto            
            if (_widgetParam.GetParamValueInt("visutype") == 0)
            {
                String picto = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.PictoIcon)).Value;
                if (!String.IsNullOrEmpty(picto))
                {
                    HtmlGenericControl icon = new HtmlGenericControl();
                    icon.Attributes.Add("class", String.Concat(eFontIcons.GetFontClassName(picto), " tileIcon"));
                    icon.Style.Add("color", _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.PictoColor)).Value);
                    content.Controls.Add(icon);
                }

            }
            else
            {
                // Cas d'une image
                //String filename = String.Concat("widget_image_", record.MainFileid, ".jpg");
                String filename = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.ContentSource)).Value;

                HtmlImage image = new HtmlImage();
                image.Attributes.Add("data-w", _widgetParam.GetParamValue("width"));
                image.Attributes.Add("data-h", _widgetParam.GetParamValue("height"));
                image.Src = String.Concat(_pref.AppExternalUrl?.TrimEnd('/'), "/", eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.WIDGET, _pref.GetBaseName), "/", filename);
                content.Controls.Add(image);
            }

            // Titre
            HtmlGenericControl title = new HtmlGenericControl("h2");
            title.Attributes.Add("class", "tileTitle");
            title.InnerText = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.Title)).Value;

            string titleColor = _widgetParam.GetParamValue("titlecolor");
            string subtitleColor = _widgetParam.GetParamValue("subtitlecolor");

            if (!String.IsNullOrEmpty(titleColor))
                title.Style.Add("color", titleColor);
            content.Controls.Add(title);

            // Sous-titre
            HtmlGenericControl subtitle = new HtmlGenericControl();
            subtitle.Attributes.Add("class", "tileSubtitle");
            subtitle.InnerText = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.SubTitle)).Value;
            if (!String.IsNullOrEmpty(titleColor) || !String.IsNullOrEmpty(subtitleColor))
                subtitle.Style.Add("color", (!String.IsNullOrEmpty(subtitleColor) ? subtitleColor : titleColor));
            content.Controls.Add(subtitle);

            base.Build(widgetContainer);
        }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public override void AppendScript(StringBuilder scriptBuilder)
        {
            base.AppendScript(scriptBuilder);
        }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="styleBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}