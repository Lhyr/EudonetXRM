using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{

    /// <summary>
    /// informations nécessaires à la gestion du paging
    /// </summary>
    public class PagingInfoModel
    {
        public int RowsPerPage { get; set; }
        public int NbTotalRows { get; set; }
        public int Page { get; set; }
        public int NbPages { get; set; }
    }
}