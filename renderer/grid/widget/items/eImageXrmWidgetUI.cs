using System;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace Com.Eudonet.Xrm
{
    public class eImageXrmWidgetUI : eAbstractXrmWidgetUI
    {
        private ePref _pref;

        public eImageXrmWidgetUI(ePref pref)
        {
            this._pref = pref;
        }
        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {

            widgetContainer.Style.Add("display", "table-cell");
            widgetContainer.Style.Add("vertical-align", "middle");

            HtmlImage image = new HtmlImage();
            widgetContainer.Controls.Add(image);

            image.ID = "widget-content-" + _widgetRecord.MainFileid;

            // Pour garder la proportion          
            image.Width = this._widgetParam.GetParamValueInt("width");
            image.Height = this._widgetParam.GetParamValueInt("height");
            image.Attributes.Add("w", image.Width.ToString());
            image.Attributes.Add("h", image.Height.ToString());

            String filename = _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentSource)).Value;

            // Image par défaut si vide
            if (string.IsNullOrWhiteSpace(filename))
            {
                //image.Src = "themes/default/images/404.png";
                image.Style.Add(System.Web.UI.HtmlTextWriterStyle.Display, "none");
                if (_pref.AdminMode)
                {
                    Panel pn = new Panel();
                    widgetContainer.Controls.Add(pn);
                    pn.CssClass = "block";

                    pn.Controls.Add(new LiteralControl(eResApp.GetRes(_pref, 3149)));

                }
            }
            else
            {
                string relativePath = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.WIDGET, _pref.GetBaseName);
                // garder la compatibilité avec les anciennes images
                if (filename.Contains(relativePath))
                    image.Src = filename;
                else
                    // Url relative de l'image
                    image.Src = eTools.WebPathCombine(relativePath, filename) + "?ver=" + DateTime.Now.TimeOfDay;
            }

            // String filename = String.Concat("widget_image_", record.MainFileid, ".jpg");
            // image.Src = String.Concat(_pref.AppExternalUrl, eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.WIDGET, _pref.GetBaseName), "/", filename);
            // Regression _pref.AppExternalUrl n'a pas toujours / à la fin 


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
        /// <param name="scriptBuilder">builder de styles</param>
        public override void AppendStyle(StringBuilder styleBuilder)
        {
            base.AppendStyle(styleBuilder);
        }
    }
}