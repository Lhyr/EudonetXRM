using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    class eAdminRelationMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminRelationMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { } 

        /// <summary>
        ///  pas de menu Cartographie pour la table Relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderCartography(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour la Table Relations
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
    }
}