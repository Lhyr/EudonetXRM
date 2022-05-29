using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack
{

    /// <summary>
    /// Information utilisateur
    /// </summary>
    public class AdvFormularUserInfoModel
    {

        /// <summary>
        /// Login utilisateur
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// 
        /// Id lang
        /// </summary>
        public int LangId { get; set; } = 0;
    }
}