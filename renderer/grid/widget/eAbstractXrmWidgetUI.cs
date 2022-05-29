using Com.Eudonet.Internal;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// eAbstractXrmWidgetUI : classe abstraite de rendu de widget
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.IXrmWidgetUI" />
    public class eAbstractXrmWidgetUI : IXrmWidgetUI
    {
        /// <summary>
        /// The record for the widget
        /// </summary>
        protected eRecord _widgetRecord;
        /// <summary>
        /// The widget preference
        /// </summary>
        protected eXrmWidgetPref widgetPref;
        /// <summary>
        /// The widget parameter
        /// </summary>
        protected eXrmWidgetParam _widgetParam;
        /// <summary>
        /// The widget context
        /// </summary>
        protected eXrmWidgetContext _widgetContext;

        /// <summary>
        /// Permet d'initialiser le renderer du widget
        /// </summary>
        /// <param name="widgetRecord">un enregistrement de widget</param>
        /// <param name="widgetPref">Objet Pref du widget</param>
        /// <param name="widgetParam">Objet Param du widget</param>
        /// <param name="widgetContext">The widget context.</param>
        public void Init(eRecord widgetRecord, eXrmWidgetPref widgetPref, eXrmWidgetParam widgetParam, eXrmWidgetContext widgetContext)
        {
            this._widgetRecord = widgetRecord;
            this.widgetPref = widgetPref;
            this._widgetParam = widgetParam;
            this._widgetContext = widgetContext;
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public virtual void Build(HtmlControl widgetContainer) { }

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public virtual void AppendScript(StringBuilder scriptBuilder) { }

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="styleBuilder">builder de styles</param>
        public virtual void AppendStyle(StringBuilder styleBuilder) { }


    }
}