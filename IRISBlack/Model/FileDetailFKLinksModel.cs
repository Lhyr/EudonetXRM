using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    /// <summary>
    /// Liaisons sur les Fiches.
    /// </summary>
    public class FileDetailFKLinksModel
    {
        public int? ParentPP { get; set; }
        public int? ParentPM { get; set; }
        public int? ParentAdr { get; set; }
        public int ParentEvtDescId { get; set; }
        public int? ParentEvt { get; set; }
    }
}