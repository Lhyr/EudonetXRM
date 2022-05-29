using Com.Eudonet.Internal;
using System.Web.UI;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu pour parametrer le widget
    /// </summary>
    public class eErrorParamRenderer : eWidgetSpecificParamRenderer
    {
        public eErrorParamRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetParam param) : base(pref, isVisible, file, param)
        {
        }


        /// <summary>
        /// Construit un block du contenu specifique au widget
        /// </summary>
        /// <returns></returns>
        protected override void BuildWidgetContentPart()
        {
            _pgContainer.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 8146))); // Pas de paramétrage défini pour ce widget           
        }
    }
}