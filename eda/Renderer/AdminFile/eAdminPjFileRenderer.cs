using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPjFileRenderer : eAdminFileRenderer
    {


        public eAdminPjFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }
        /// <summary>
        /// pas d'appartenance et traçabilité
        /// </summary>
        /// <param name="tcFileOwner"></param>
        protected override void Get99TableCell(TableCell tcFileOwner) {
            return;
        }


        ///// <summary>
        ///// Pas de règle de mesures
        ///// </summary>

        // protected override void AddParentHead() { }

        /// <summary>
        /// Pas de signet 
        /// </summary>
        protected override void GetBookMarkBlock() { }
    }
}