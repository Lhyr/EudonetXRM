using Com.Eudonet.Engine.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{

    public class CreateReturnModel : EngineReturnModel
    {

        public IReloadInfosModel ReloadInfos { get; set; }

        public IList<int> FilesId { get; set; }

    }
}