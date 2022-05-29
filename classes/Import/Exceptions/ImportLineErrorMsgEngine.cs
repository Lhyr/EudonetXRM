using Com.Eudonet.Internal;
using System;

namespace Com.Eudonet.Xrm.Import.Exceptions
{
    public class ImportLineErrorMsgEngine : ImportLineErrorMsg
    {
        public eErrorContainer ErrContainer { get; set; }

        protected override string GetUserSpecMsg(ePrefLite pref)
        {
            return String.Concat(ErrContainer.Msg, " (", ErrContainer.Detail, ")");
        }

        protected override string GetDevSpecMsg()
        {
            return String.Format("\n\ruser : {0}\n\rdev : {1}", ErrContainer.Msg, ErrContainer.DebugMsg);
        }
    }
}