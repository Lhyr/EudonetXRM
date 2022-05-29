using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public class ExceptionModel
    {
        public string Message { get; set; }
        public string DebugMessage { get; set; }
        public string UserMessage { get; set; }
        public string StackTrace { get; set; }
        public int Code { get; set; }
    }
}