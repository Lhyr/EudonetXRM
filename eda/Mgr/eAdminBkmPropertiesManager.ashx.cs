using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminBkmPropertiesManager
    /// </summary>
    public class eAdminBkmPropertiesManager : eAdminManager
    {
        /// <summary>
        /// type d'action pour le manager
        /// </summary>
        public enum BkmManagerAction
        {
            /// <summary>action indéfini </summary>
            UNDEFINED = 0,
            /// <summary>Rendu html des propriétés du signet</summary>
            GETINFOS = 1            
        }

        private BkmManagerAction action = BkmManagerAction.UNDEFINED;
        private Int32 _nTab;
        private Int32 _nBkmid;

        private eAdminResult result = null;

        protected override void ProcessManager()
        {
            if (_requestTools.AllKeys.Contains("action") && !String.IsNullOrEmpty(_context.Request.Form["action"]))
            {
                if (!Enum.TryParse(_context.Request.Form["action"], out action))
                    action = BkmManagerAction.UNDEFINED;
            }

            switch (action)
            {
                case BkmManagerAction.UNDEFINED:
                    ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Aucune action définie.");
                    LaunchError();
                    break;
                case BkmManagerAction.GETINFOS:
                    _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                    _nBkmid =  _requestTools.GetRequestFormKeyI("bkm") ?? 0;

                    //tab et bkm obligatoire
                    if (_nTab == 0 || _nBkmid == 0)
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "Paramètres non renseignés.");
                        LaunchError();
                    }

                    // Création du rendu
                    eAdminRenderer rdr = eAdminRendererFactory.CreateAdminTabBkmParamsRenderer(_pref, _nTab, _nBkmid);
                    RenderResultHTML(rdr.PgContainer);

                    break;
            }
        }
    }
}