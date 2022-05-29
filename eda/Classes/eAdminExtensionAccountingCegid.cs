using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension correspondant à l'interface de comptabilité Cegid
    /// Backlog #331
    /// </summary>
    public class eAdminExtensionAccountingCegid : eAdminExtensionAccounting
    {
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionAccountingCegid(eAdminExtensionInfo infos) : base(infos)
        {

        }

        /// <summary>
        /// Constructeur pour l'initialisation avec Pref
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionAccountingCegid(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID)
        {

        }
    }
}