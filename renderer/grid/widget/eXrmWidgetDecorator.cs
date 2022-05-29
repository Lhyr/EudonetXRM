using Com.Eudonet.Internal;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Class qui permet d'ajouter des options dynamiquement aux widgets
    /// </summary>
    public abstract class eXrmWidgetDecorator : IXrmWidgetUI
    {
        /// <summary>
        /// Objet widget à décorer
        /// </summary>
        protected IXrmWidgetUI widgetUI;

        /// <summary>
        /// Acces aux données du widget
        /// </summary>
        protected eRecord widgetRecord;

        /// <summary>
        /// Acces aux pref du widget
        /// </summary>
        protected eXrmWidgetPref widgetPref;

        /// <summary>
        /// Initializes a new instance of the <see cref="eXrmWidgetDecorator"/> class.
        /// </summary>
        /// <param name="widgetUI">The widget UI.</param>
        public eXrmWidgetDecorator(IXrmWidgetUI widgetUI)
        {
            this.widgetUI = widgetUI;
        }

        /// <summary>
        /// Permet d'initialiser le renderer du widget
        /// Ne pas oublier d'appeler la méthode de base lors d'un override
        /// </summary>
        /// <param name="widgetRecord">un enregistrement de widget</param>
        /// <param name="widgetPref">Préférences utilisateur pour le widget</param>
        /// <param name="widgetParam">Paramètres du widget</param>
        /// <param name="widgetContext">The widget context.</param>
        public virtual void Init(eRecord widgetRecord, eXrmWidgetPref widgetPref, eXrmWidgetParam widgetParam, eXrmWidgetContext widgetContext)
        {
            this.widgetRecord = widgetRecord;
            this.widgetPref = widgetPref;
            widgetUI.Init(widgetRecord, widgetPref, widgetParam, widgetContext);
        }

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        public abstract void Build(HtmlControl widgetContainer);


        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        public abstract void AppendScript(StringBuilder scriptBuilder);


        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="scriptBuilder">builder de styles</param>
        public abstract void AppendStyle(StringBuilder styleBuilder);

    }
}