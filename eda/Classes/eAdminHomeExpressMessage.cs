using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHomeExpressMessage : eAdminHomepage
    {


        public eAdminHomeExpressMessage(ePref pref, int id, String label, String userid, String displayUsers, String content = "") :
            base(pref, id, label, userid, displayUsers, content: content)
        {


        }

        /// <summary>
        /// Retourne la liste des objets eAdminHomepage
        /// </summary>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="types">Types de homepages recherchés</param>
        /// <returns></returns>
        public static List<eAdminHomeExpressMessage> GetHomeExpressMessageList(ePref pref, eudoDAL eDal)
        {
            String sError = String.Empty;

            List<eAdminHomeExpressMessage> list = eSqlHomeExpressMessage.GetExpressMessage(pref, eDal, out sError);

            if (!String.IsNullOrEmpty(sError))
                throw new Exception(sError);

            return list;
        }

    }
}