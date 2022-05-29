using EudoQuery;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public class eImageParamRenderer : eWidgetSpecificParamRenderer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="eImageParamRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="param">The parameter.</param>
        public eImageParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param)
        {
        }


        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _pgContainer.Controls.Add(BuildImageArea(_file.GetField((int)XrmWidgetField.ContentSource)));

            _pgContainer.Controls.Add(BuildHiddenField(_contentParamField, "width", _widgetParams.GetParamValue("width")));
            _pgContainer.Controls.Add(BuildHiddenField(_contentParamField, "height", _widgetParams.GetParamValue("height")));

        }


    }
}