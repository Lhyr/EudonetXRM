using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Extension correspondant à l'interface de paramétrage de la synchronisation Exchange version 2016 on Premise (API Graph)
    /// Backlog #335
    /// </summary>
    public class eAdminExtensionSynchroExchange2016OnPremise : eAdminExtensionSynchroExchange
    {
        /// <summary>
        /// Constructeur principal
        /// </summary>
        /// <param name="infos"></param>
        [JsonConstructor]
        public eAdminExtensionSynchroExchange2016OnPremise(eAdminExtensionInfo infos) : base(infos)
        {
            this.Infos = infos;
        }

        /// <summary>
        /// Constructeur avec Pref
        /// </summary>
        /// <param name="pref"></param>
        public eAdminExtensionSynchroExchange2016OnPremise(ePref pref) : base(pref, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE)
        {
            string strError = String.Empty;
        }
    }
}