using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension correspondant à l'interface de comptabilité Business Soft
    /// Backlog #330
    /// </summary>
    public class eAdminExtensionAccountingBusinessSoft : eAdminExtensionAccounting
    {
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionAccountingBusinessSoft(eAdminExtensionInfo infos) : base(infos)
        {

        }

        /// <summary>
        /// Constructeur pour l'initialisation avec Pref
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionAccountingBusinessSoft(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT)
        {

        }
    }
}