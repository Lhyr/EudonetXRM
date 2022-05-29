using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
     class eAdminPmPpMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminPmPpMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }
        
        /// <summary>
        /// Pas de menu Relations pour PP
        /// </summary>
        /// <param name="menuContainer"></param>
        /// <param name="openedBlocks"></param>
        public override void RenderRelations(Panel menuContainer ) { }
    }
}