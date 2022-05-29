using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class BkmRequestModel
    {
        public int ParentTab { get; set; }
        public int ParentFileId { get; set; }
        public int Bkm { get; set; }
        public int RowsPerPage { get; set; }
        public int Page { get; set; }

        public bool IsPinned { get; set; }
        public int BkmFileId { get; set; } = 0;
        public int BkmFilePos { get; set; } = 0;
        public bool DisplayAllRecord { get; set; } = false;

    }
}