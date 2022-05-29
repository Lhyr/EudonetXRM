using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminRelationContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminRelationContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }
      
        /// <summary>
        /// Pas de colonnes pour les Relations
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderColumns(Panel contentMenu) { }

        /// <summary>
        /// Pas d'entête pour Relations
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }

        /// <summary>
        /// Pas de Rubriques pour Relation
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderField(Panel contentMenu) { }

        /// <summary>
        /// Pas de Onglet Web pour Relations
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebTab(Panel contentMenu) { }

        /// <summary>
        /// Pas de Signet Web pour Relations
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebBkm(Panel contentMenu) { }

    }
}
