using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class RecordMailModel 
        : RecordModel
    {
        /// <summary>
        /// statuts des mails
        /// </summary>
        public int MailStatus { get; set; }
    }
}