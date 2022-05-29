using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminUpgradeInProgress
    /// </summary>
    public class eAdminUpgradeInProgressManager : eAdminManager
    {

        /// <summary>
        /// action demandée au manager
        /// </summary>
        public enum Action
        {
            /// <summary>Non défini</summary>
            UNDEFINED = 0,
            /// <summary>Active le blocage de la base le temps de la montée de version </summary>
            ENABLE = 1,
            /// <summary>Desactive le blocage de la base</summary>
            DISABLE = 2

        }

        Action _action = Action.UNDEFINED;

        /// <summary>
        /// Surcharge du manager
        /// </summary>
        protected override void ProcessManager()
        {
            _action = _requestTools.GetRequestFormEnum<Action>("action");

            switch (_action)
            {
                case Action.UNDEFINED:
                    throw new EudoException("Paramètres invalides : Action.UNDEFINED");
                    break;
                case Action.ENABLE:
                    enable(_requestTools.GetRequestFormKeyB("immediate") ?? false);
                    break;
                case Action.DISABLE:
                    disable();
                    break;
                default:
                    break;
            }
        }


        private void enable(bool bImmediate = false)
        {
            eLibTools.EnableUpgradeInProcess(_pref, bImmediate);
        }

        private void disable()
        {
            eLibTools.DisableUpgradeInProcess(_pref);
        }
    }
}