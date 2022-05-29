using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet qui permet d'ajouter la barre d'outils
    /// </summary>
    public class eXrmWidgetWithAdminToolbar : eXrmWidgetDecorator
    {
        public eXrmWidgetWithAdminToolbar(IXrmWidgetUI XrmWidgetUI) : base(XrmWidgetUI) { }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            widgetContainer.Controls.Add(new LiteralControl("eXrmWidgetWithAdminToolbar"));

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