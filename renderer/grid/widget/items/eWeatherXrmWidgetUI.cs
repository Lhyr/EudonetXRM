using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eWeatherXrmWidgetUI : eAbstractXrmWidgetUI
    {
        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {


            HtmlGenericControl iframe = new HtmlGenericControl("iframe");
            iframe.Attributes.Add("src", _widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.ContentSource)).Value);

            widgetContainer.Controls.Add(iframe);

            // TODO Attributs aditionnal paramètre de site

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