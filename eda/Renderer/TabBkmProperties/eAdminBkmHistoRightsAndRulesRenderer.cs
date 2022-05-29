using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{


    /// <summary>
    /// Propriété de signet historique
    /// </summary>
    public class eAdminBkmHistoRightsAndRulesRenderer : eAdminBkmRightsAndRulesRenderer
    {
        /// <summary>
        /// Constructeur par défuat
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="parentInfos"></param>
        internal eAdminBkmHistoRightsAndRulesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos parentInfos) : base(pref, tabInfos, parentInfos)
        {
        }
        
        protected override void BuildAdminRights()
        {
            eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7406), "buttonAdminRights", onclick: "nsAdmin.confRights(" + (int)TableType.HISTO + ")", readOnly: _readOnly);
            button.Generate(_panelContent);
        }

    }
}