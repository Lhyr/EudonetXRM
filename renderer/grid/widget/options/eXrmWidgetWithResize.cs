using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eXrmWidgetWithResize : eXrmWidgetDecorator
    {
        ePref _pref;

        public eXrmWidgetWithResize(IXrmWidgetUI XrmWidgetUI, ePref pref) : base(XrmWidgetUI)
        {
            _pref = pref;
        }


        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            // on ajoute d'abord le contenu puis la div pour le resize
            widgetUI.Build(widgetContainer);

            // Redimensionnable
            eFieldRecord field = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(EudoQuery.XrmWidgetField.Resize));
            if (field.Value == "1" || field.Value.ToLower() == "true")
            {
                HtmlGenericControl resize = new HtmlGenericControl("div");
                resize.ID = "xrm-widget-resize";
                resize.Attributes.Add("class", "xrm-widget-resize");
               // resize.Attributes.Add("style", "border-right-color: " + (_pref.AdminMode ? "#37A7DE" : _pref.ThemeXRM.Id == 11 ? _pref.ThemeXRM.Color2 : _pref.ThemeXRM.Color) + ";");
                resize.Attributes.Add("r", "1");

                // Permet de faire resize meme si le cursor entre dans une iframe
                HtmlGenericControl resizeProtector = new HtmlGenericControl("div");
                resizeProtector.Attributes.Add("class", "xrm-widget-resize-protector");
                widgetContainer.Controls.Add(resizeProtector);

                widgetContainer.Controls.Add(resize);
            }
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