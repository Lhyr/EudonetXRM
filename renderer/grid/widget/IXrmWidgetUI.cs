using Com.Eudonet.Internal;
using System.Text;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Interface pour le rendu d'un widget
    /// </summary>
    public interface IXrmWidgetUI
    {

        /// <summary>
        /// Permet d'initialiser le renderer du widget
        /// </summary>
        /// <param name="widgetRecord">un enregistrement de widget</param>
        /// <param name="widgetPref">Préférences utilisateur pour le widget</param>
        /// <param name="widgetParam">Paramètres du widget</param>
        /// <param name="widgetContext">The widget context.</param>
        void Init(eRecord widgetRecord, eXrmWidgetPref widgetPref, eXrmWidgetParam widgetParam, eXrmWidgetContext widgetContext);

        /// <summary>
        /// Fait un rendu du widget dans le container
        /// </summary>
        /// <param name="widgetContainer">Container du widget</param>
        void Build(HtmlControl widgetContainer);

        /// <summary>
        /// Ajoute des scripts js
        /// </summary>
        /// <param name="scriptBuilder">builder de script</param>
        void AppendScript(StringBuilder scriptBuilder);

        /// <summary>
        /// Ajoute des style css
        /// </summary>
        /// <param name="styleBuilder">Buider de style</param>
        void AppendStyle(StringBuilder styleBuilder);
    }
}
