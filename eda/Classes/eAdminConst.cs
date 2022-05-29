using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminConst
    {
        /// <summary>Indique la version actuelle de l'administration d'XRM</summary>
        public const int ADMIN_CURRENTVERSION = (int)ADMIN_VERSION.V1;

        /// <summary>
        /// Versions de l'administration d'XRM
        /// Utile pour cacher certaines options en cours de développement
        /// </summary>
        public enum ADMIN_VERSION
        {
            V1 = 1,
            V2 = 2
        }
    }
}