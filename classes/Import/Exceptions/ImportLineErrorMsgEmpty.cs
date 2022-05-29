
using Com.Eudonet.Internal;
namespace Com.Eudonet.Xrm.Import.Exceptions
{
    public class ImportLineErrorMsgEmpty : ImportLineErrorMsg
    {
        protected override string GetUserSpecMsg(ePrefLite pref)
        {
            // 1816 - Ligne vide
            return eResApp.GetRes(pref, 1816);
        }
    }
}