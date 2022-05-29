using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class BookmarkDetailModel : ListDetailModel
    {
        public CpltParam Param;

        public class CpltParam
        {
            public int RelationDescId;

        }

    }
}