using Com.Eudonet.Engine.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{


    public class UpdateReturnModel : EngineReturnModel
    {
        public int Id { get; set; }
        public bool IsLinkChanged { get; set; }
        public bool IsHistoChanged { get; set; }

    }
}