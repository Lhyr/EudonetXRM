using EudoQuery;

using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public class eCartoSelectionParamRenderer : eWidgetSpecificParamRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="eCartoSelectionParamRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        public eCartoSelectionParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param)
        {
        }


        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _pgContainer.Controls.Add(BuildBtnField(eResApp.GetRes(Pref, 2035), eResApp.GetRes(Pref, 2036), clientClick: $"nsAdminCartoSelection.loadSettings({this._widgetParams.WidgetId});"));         

        }
    }
}