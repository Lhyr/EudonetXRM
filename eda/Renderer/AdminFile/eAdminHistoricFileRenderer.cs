using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHistoricFileRenderer : eAdminFileRenderer
    {

        public eAdminHistoricFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }


        /// <summary>
        /// Pas de signet 
        /// </summary>
        protected override void GetBookMarkBlock() { }
    }
}