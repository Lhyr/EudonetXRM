using Com.Eudonet.Internal;
using EudoQuery;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public class eWebPageParamRenderer : eWidgetSpecificParamRenderer
    {

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>    
        public eWebPageParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param)
        {
        }


        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _pgContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 5143), _file.GetField((int)XrmWidgetField.ContentSource)));
            _pgContainer.Controls.Add(BuildInputField(eResApp.GetRes(Pref, 8147), _contentParamField, paramName: "urlparam", value: _widgetParams.GetParamValue("urlparam")));
        }
    }
}