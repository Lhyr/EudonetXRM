using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    class eAdminInteractionContentRenderer : eAbstractTabContentRenderer
    {
        public eAdminInteractionContentRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }
        
        /// <summary>
        /// Pas d'entête pour Interactions
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderHeader(Panel contentMenu, string[] openBlocks) { }
        
        /*
        /// <summary>
        /// Pas de Rubriques pour Interactions
        /// </summary>
        /// <param name="contentMenu"></param>
        public override void RenderField(Panel contentMenu) { }
        */
    }
}