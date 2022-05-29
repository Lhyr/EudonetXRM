using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Rendu des paramètres du widget RSS
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eWidgetSpecificParamRenderer" />
    public class eRSSParamRenderer : eWidgetSpecificParamRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="eRSSParamRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        public eRSSParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param, true)
        {

        }

        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _pgContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 5143), _contentParamField, paramName: "url", value: _widgetParams.GetParamValue("url")));

        }
    }
}