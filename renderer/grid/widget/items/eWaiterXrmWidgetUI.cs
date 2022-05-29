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
    public class eWaiterXrmWidgetUI : eAbstractXrmWidgetUI
    {
        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {

            // Chargement en cours ...
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "xrm-widget-waiter");

            HtmlGenericControl img = new HtmlGenericControl("img");
            img.Attributes.Add("alt", "wait");
            img.Attributes.Add("src", "themes/default/images/wait.gif");
            div.Controls.Add(img);

            widgetContainer.Controls.Add(div);                  

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