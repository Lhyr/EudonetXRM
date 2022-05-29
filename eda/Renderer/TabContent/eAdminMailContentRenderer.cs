using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminMailContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminMailContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de colonnes pour les Mail
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderColumns(Panel contentMenu) { }

        /// <summary>
        /// Pas de Rubriques pour Mails
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderField(Panel contentMenu) { }

        /// <summary>
        /// Pas de Signet Web pour Mail
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderWebBkm(Panel contentMenu) { }
    }
}
