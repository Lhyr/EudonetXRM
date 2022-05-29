using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension correspondant aux demandes entrantes HBS
    /// Backlog #345
    /// </summary>
    public class eAdminExtensionInHBS : eAdminExtensionIn
    {
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionInHBS(eAdminExtensionInfo infos) : base(infos)
        {

        }

        /// <summary>
        /// Constructeur pour l'initialisation avec Pref
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionInHBS(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS)
        {

        }
    }
}