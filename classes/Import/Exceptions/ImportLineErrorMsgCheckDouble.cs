using Com.Eudonet.Internal;
using System;

namespace Com.Eudonet.Xrm.Import.Exceptions
{
    public class ImportLineErrorMsgCheckDouble : ImportLineErrorMsg
    {
        public String Error { get; set; }

        protected override string GetUserSpecMsg(ePrefLite pref)
        {
            // 1818 - Vérification de doublon en échec
            return eResApp.GetRes(pref, 1818);
        }
    }
}