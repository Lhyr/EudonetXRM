using Com.Eudonet.Internal;
using System;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet qui permet d'ajouter la barre d'outils
    /// </summary>
    public class eXrmWidgetContent : eXrmWidgetDecorator
    {
        ePref _pref;
        int _width = 0;
        int _height = 0;

        public eXrmWidgetContent(IXrmWidgetUI XrmWidgetUI, ePref pref, int width = 0, int height = 0) : base(XrmWidgetUI)
        {
            _width = width;
            _height = height;
            _pref = pref;
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            // Pour la sécurité on mis toujour le contenu dans une iframe
            HtmlGenericControl content = new HtmlGenericControl("div");
            content.Attributes.Add("class", "xrm-widget-content");
            if (_width > 0)
                content.Style.Add("width", _width + "px");
            if (_height > 0)
                content.Style.Add("height", _height + "px");

            widgetContainer.Controls.Add(content);

            try
            {    
              
                 widgetUI.Build(content);
               
            }
            catch (Exception ex)
            {
                // Feedback sélentieux pour les dev
                eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, ex.StackTrace), _pref);

                // Msg à l'utilisateur contenu dans le widget
                HtmlGenericControl span = new HtmlGenericControl("span");
                span.Attributes.Add("class", "xrm-widget-error");
                span.InnerHtml = eResApp.GetRes(_pref, 8193);
                content.Controls.Add(span);

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