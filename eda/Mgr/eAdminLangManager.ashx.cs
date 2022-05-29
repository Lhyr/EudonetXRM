using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminLangManager
    /// </summary>
    public class eAdminLangManager : eAdminManager
    {
        public enum LangManagerAction
        {
            UNDEFINED = 0,
            UPDATE = 1,
            DELETE = 2
        }

        protected override void ProcessManager()
        {
            int langId;
            LangManagerAction action = LangManagerAction.UNDEFINED;
            String value;
            eLibConst.MAPLANG_FIELD field = eLibConst.MAPLANG_FIELD.LANG_SYSID;

            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
            {
                if (!Enum.TryParse(_context.Request.Form["action"], out action))
                    action = LangManagerAction.UNDEFINED;
            }

            if (_requestTools.AllKeys.Contains("langId") && !String.IsNullOrEmpty(_context.Request.Form["langId"]))
                Int32.TryParse(_context.Request.Form["langId"], out langId);
            else
                throw new EudoAdminParameterException("Paramètre LangId incorrect");


            //value = _requestTools.GetRequestFormKeyS("value");

            try
            {

                switch (action)
                {
                    case LangManagerAction.UPDATE:

                        if (_requestTools.AllKeys.Contains("updatecol") && !String.IsNullOrEmpty(_context.Request.Form["updatecol"]))
                            if (!Enum.TryParse<eLibConst.MAPLANG_FIELD>(_context.Request.Form["updatecol"], out field))
                                throw new EudoAdminParameterException("Paramètre updatecol incorrect");

                        value = _requestTools.GetRequestFormKeyS("value");

                        eAdminLanguage.Save(_pref, langId, field, value);

                        break;
                    //case LangManagerAction.DELETE:

                    //    eAdminLanguage.Delete(_pref, langId);
                    //    break;
                    default:
                        throw new NotImplementedException(String.Concat("Action non reconnue : ", action.ToString()));
                }

            }
            catch (Exception exc)
            {
                eErrorContainer error = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), "Gestion des langues", devMsg: exc.Message.ToString());
                LaunchError(error);
            }
        }


    }
}