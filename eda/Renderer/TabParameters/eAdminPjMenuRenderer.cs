using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
     class eAdminPjMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminPjMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }
       
        /// <summary>
        /// Pas de menu Relations pour les Annexes
        /// </summary>
        /// <param name="menuContainer"></param>
        /// <param name="openedBlocks"></param>
        public override void RenderRelations(Panel menuContainer ) { }

        /// <summary>
        ///  pas de menu Cartographie pour les Annexes
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderCartography(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour les Annexes
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Filtres et doublons pour la Table Relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderFiltersAndDuplicate(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Appartenance/tracabilité pour les Annexes
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderTraceability(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Raccourcis/traitements pour les Annexes
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderTreatments(Panel menuContainer) { }

    }
}