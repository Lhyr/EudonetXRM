using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminPjContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminPjContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de colonnes pour les PJ
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderColumns(Panel contentMenu) { }

        /// <summary>
        /// Pas d'entête pour PJ
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }


        /// <summary>
        /// Pas de Signet Web pour PJ
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebBkm(Panel contentMenu) { }
    }
}
