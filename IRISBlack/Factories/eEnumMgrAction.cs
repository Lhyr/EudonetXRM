using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories {

    /// <summary>
    /// Action du manager
    /// </summary>
    public enum eEnumMgrAction
    {
        /// <summary>
        /// Première vérification à l'upload
        /// </summary>
        Check = 0,
        /// <summary>
        /// Interface de confirmation de renommage ou remplacement des fichiers
        /// </summary>
        Confirmation = 1
    }
}