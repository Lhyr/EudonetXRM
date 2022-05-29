using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Administration de l'extension Grille
    /// </summary>
    public class eAdminStoreGridRenderer : eAdminStoreFileRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreGridRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }
        public string SortCol { get; internal set; }
        public int Sort { get; internal set; }


        /// <summary>
        /// Generation de l'administration de l'extension Grille
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreGridRenderer CreateAdminStoreGridRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreGridRenderer rdr = new eAdminStoreGridRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }

    }
}