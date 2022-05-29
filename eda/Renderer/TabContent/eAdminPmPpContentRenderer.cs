using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminPmPpContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminPmPpContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas d'entête pour PM
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }

    }
}
