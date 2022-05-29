using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    class eAdminWebTabFileMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminWebTabFileMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        ///  pas de menu Cartographie pour la table virtuelle onglet web
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderCartography(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour la Table virtuelle onglet web
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Filtres et doublons pour la Table Relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderFiltersAndDuplicate(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Appartenance et traçabilité pour la Table Relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderTraceability(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Raccourcis/traitements pour la table relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderTreatments(Panel menuContainer) { }

        /// <summary>
        /// Pas de Menu Relations pour les onglets web
        /// </summary>
        /// <param name="menuContainer"></param>
        /// <param name="openedBlocks"></param>
        public override void RenderRelations(Panel menuContainer )
        {
            
        }

        /// <summary>
        /// Pas de Menu Perforamnces pour les onglets web
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderPerformances(Panel menuContainer)
        {
            
        }
        /// <summary>
        /// Pas de règles pour les onglets web
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderRules(Panel menuContainer)
        {
            
        }

        /// <summary>
        /// Pas de menu préférences
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderPreferences(Panel menuContainer)
        {
            
        }

        /// <summary>
        /// Pas de menu rapports
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderReports(Panel menuContainer)
        {
            
        }
    }
}