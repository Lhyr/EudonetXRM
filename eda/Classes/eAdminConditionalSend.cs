using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// classe de gestion des r_gles communication conditionnelles
    /// </summary>
    public class eAdminConditionalSend
    {



        /// <summary>
        /// Sauvegarde ou crée un uservalue pour le user passé en constructeur
        ///   Les clées pour uservalue sont TYPE ET USERID ET TAB ET DESCID
        /// </summary>
        ///<param name="tab">Tab ds condtiona a mettre a jour</param>
        ///<param name="lst">List condition a maj</param>
        ///<param name="pref">prf de cnx</param>
        /// <returns></returns>
        public static eAdminResult Save(int tab, List<SetParam<eLibConst.TREATID>> lst, ePref pref)
        {
            return Core.Model.eAdminConditionalSend.Save(tab, lst, pref);
        }

    }
}