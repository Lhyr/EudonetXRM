using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
     class eAdminTargetMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminTargetMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour Cible étendues
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }
    }
}