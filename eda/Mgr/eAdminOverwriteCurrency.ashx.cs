using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminOverwriteCurrency
    /// </summary>
    public class eAdminOverwriteCurrency : eAdminManager
    {

        protected override void ProcessManager()
        {
            eAdminDescAdv descAdv = new eAdminDescAdv(pref: _pref);
            eAdminResult adminRes = descAdv.OverwriteMoneyCurrency();


            if (adminRes?.Success ?? false)
            {
                eMsgContainer msgContainer = new eMsgContainer(msg: eResApp.GetRes(_pref, 3134), title: "", body: "", type: eLibConst.MSG_TYPE.INFOS);
                RenderResult(RequestContentType.XML, delegate () { return msgContainer.GetXML().OuterXml; });


            }
            else if (adminRes != null && !adminRes.Success)
            {
                if (adminRes.DebugErrorMessage.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        "Une erreur s'est produite lors de la mise à jour des rubriques de type monétaire",
                        adminRes.UserErrorMessage,
                        "",
                        adminRes.DebugErrorMessage
                    );
                }

                if (adminRes.UserErrorMessage.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        "Une erreur s'est produite lors de la mise à jour des rubriques de type monétaire",
                        adminRes.UserErrorMessage
                    );
                }

                LaunchError();
            }
        }



    }
}