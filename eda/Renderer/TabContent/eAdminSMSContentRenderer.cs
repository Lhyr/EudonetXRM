using Com.Eudonet.Internal.eda;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    class eAdminSMSContentRenderer : eAdminMailContentRenderer
    {
        public eAdminSMSContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) { }
    }
}
