using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class ReloadInfosModel : IReloadInfosModel
    {
        public bool ReloadHeader { get; set; }
        public bool ReloadDetail { get; set; }
        public bool ReloadFileHeader { get; set; }
    }
}