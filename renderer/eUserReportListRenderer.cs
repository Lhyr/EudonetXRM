
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// 
    /// </summary>
    public class eUserReportListRenderer : eRenderer
    {

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        protected eUserReportListRenderer(ePref pref)
        {
            Pref = pref;
            _rType = RENDERERTYPE.UserReportList;
        }



        /// <summary>
        /// Retourne un renderer contanant la liste des rapport de l'utilisateur
        /// </summary>
        /// <returns></returns>
        public static eUserReportListRenderer GetUserReportList(ePref pref )
        {

            eUserReportListRenderer er = new eUserReportListRenderer(pref);


            return er;
        }

    }
}