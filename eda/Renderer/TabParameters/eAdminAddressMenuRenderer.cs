using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    class eAdminAddressMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminAddressMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }

        /// <summary>
        /// Pas de menu Options de Recherche pour Address
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }

        /// <summary>
        /// Pas de menu Raccourcis/traitements pour Address
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderTreatments(Panel menuContainer) { }
    }
}