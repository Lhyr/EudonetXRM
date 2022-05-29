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
    public class eXrmWidgetWrapper : eXrmWidgetDecorator
    {
        bool _firstLoad;
        ePref _pref;
        List<eSqlResFiles> _listRes;

        /// <summary>
        /// Initializes a new instance of the <see cref="eXrmWidgetWrapper"/> class.
        /// </summary>
        /// <param name="XrmWidgetUI">The XRM widget UI.</param>
        /// <param name="pref">The preference.</param>
        /// <param name="firstLoad">if set to <c>true</c> [first load].</param>
        public eXrmWidgetWrapper(IXrmWidgetUI XrmWidgetUI, ePref pref, bool firstLoad = false) : base(XrmWidgetUI)
        {
            _pref = pref;
            _firstLoad = firstLoad;
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

            string error;
            _listRes = eSqlResFiles.LoadRes(_pref, new List<int> { (int)XrmWidgetField.Tooltip }, widgetRecord.MainFileid, _pref.LangId, out error);
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public override void Build(HtmlControl widgetContainer)
        {
            HtmlGenericControl widgetWrapper = new HtmlGenericControl("div");
            widgetWrapper.ID = "widget-wrapper-" + widgetRecord.MainFileid.ToString();
            widgetWrapper.Attributes.Add("class", "widget-wrapper");
            widgetWrapper.Attributes.Add("m", "1");
            AppendWidgetAttributes(widgetWrapper);

            // Le widget sera affiché une fois sa position est caclculée par le js
            widgetWrapper.Style.Add("display", "none");

            widgetContainer.Controls.Add(widgetWrapper);

            string tooltipValue = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.Tooltip)).DisplayValue;
            if (_listRes != null)
            {
                eSqlResFiles res = _listRes.Find(r => r.DescID == (int)XrmWidgetField.Tooltip);
                if (res != null && !String.IsNullOrEmpty(res.Value))
                    tooltipValue = res.Value;
            }
            HtmlGenericControl xrmWidget = new HtmlGenericControl("div");
            xrmWidget.Attributes.Add("class", "xrm-widget");
            xrmWidget.Attributes.Add("title", tooltipValue);

            widgetWrapper.Controls.Add(xrmWidget);

            widgetUI.Build(xrmWidget);
        }

        /// <summary>
        /// Ajout des attribut html pour le rendu client
        /// </summary>
        /// <param name="widgetWrapper"></param>
        private void AppendWidgetAttributes(HtmlControl widgetWrapper)
        {
            eFieldRecord fld = widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.Type));


            widgetWrapper.Attributes.Add("f", widgetRecord.MainFileid.ToString());                
            widgetWrapper.Attributes.Add("n", "0");
            widgetWrapper.Attributes.Add("t", fld.Value);
            widgetWrapper.Attributes.Add("x", widgetPref.PosX.ToString());
            widgetWrapper.Attributes.Add("y", widgetPref.PosY.ToString());
            widgetWrapper.Attributes.Add("w", widgetPref.Width.ToString());
            widgetWrapper.Attributes.Add("h", widgetPref.Height.ToString());
            widgetWrapper.Attributes.Add("mr", widgetRecord.GetFieldByAlias(eXrmWidgetTools.GetAlias(XrmWidgetField.ManuelRefresh)).Value.ToString()); // manuel-refresh

            if (_firstLoad && (fld.Value.Equals(((int)XrmWidgetType.List).ToString()) || fld.Value.Equals(((int)XrmWidgetType.Indicator).ToString()) || fld.Value.Equals(((int)XrmWidgetType.Chart).ToString())))
                widgetWrapper.Attributes.Add("async", "1");
            else
                widgetWrapper.Attributes.Add("async", "0");
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