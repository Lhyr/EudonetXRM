using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    class eAdminHistoricMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminHistoricMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de menu Relations pour l'Historiques
        /// </summary>
        /// <param name="menuContainer"></param>
        /// <param name="openedBlocks"></param>
        public override void RenderRelations(Panel menuContainer ){}

        /// <summary>
        ///  pas de menu Cartographie pour l'Historiques
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderCartography(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour Historique
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }
    }
}