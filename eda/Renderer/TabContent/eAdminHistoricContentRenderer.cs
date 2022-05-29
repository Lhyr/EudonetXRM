using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminHistoricContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminHistoricContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de colonnes pour Historiques
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderColumns(Panel contentMenu) { }

        /// <summary>
        /// Pas d'entête pour Historiques
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }

        /// <summary>
        /// Pas de Rubriques pour Historiques
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderField(Panel contentMenu) { }

        /// <summary>
        /// Pas de Signet Web pour Historique
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebBkm(Panel contentMenu) { }
    }
}
