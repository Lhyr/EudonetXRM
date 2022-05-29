using Com.Eudonet.Internal;
using System;

namespace Com.Eudonet.Xrm.Import.Exceptions
{
    public abstract class ImportLineErrorMsg
    {
        public eImportContentLine Line { get; set; }
        public String DevMsg { get; set; }

        protected abstract String GetUserSpecMsg(ePrefLite pref);

        public String GetUserMsg(ePrefLite pref)
        {
            // 6830 - Ligne n°<NB> :
            return String.Concat(eResApp.GetRes(pref.LangId, 6830).Replace("<NB>", (Line.Index + 1).ToString()), " ", GetUserSpecMsg(pref));
        }

        protected virtual String GetDevSpecMsg()
        {
            return String.Empty;
        }

        public String GetDevMsg(ePrefLite pref)
        {
            String specMsg = GetDevSpecMsg();
            if (specMsg.Length != 0)
            {
                // 6830 - Ligne n°<NB> :
                return String.Concat(eResApp.GetRes(pref.LangId, 6830).Replace("<NB>", (Line.Index + 1).ToString()), " ", specMsg);
            }

            return String.Empty;
        }
    }
}