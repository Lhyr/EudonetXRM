using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminAddressContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminAddressContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas d'entête pour Address
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }

        /// <summary>
        /// Pas de Onglet Web pour Address
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebTab(Panel contentMenu) { }

        /// <summary>
        /// Pas de Signet Web pour Address
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebBkm(Panel contentMenu)
        {
            base.RenderWebBkm(contentMenu);
        }
    }
}
