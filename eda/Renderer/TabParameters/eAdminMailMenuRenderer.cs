using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
     class eAdminMailMenuRenderer : eAbstractAdminTabMenuRenderer
    {
        public eAdminMailMenuRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }
              

        /// <summary>
        ///  pas de menu Cartographie pour Les Mails
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderCartography(Panel menuContainer) { }


        /// <summary>
        /// Pas de menu Options de Recherche pour les Mails
        /// </summary>
        /// <param name="menuContainer"></param>
        public override void RenderSearchOptions(Panel menuContainer) { }
    }
}