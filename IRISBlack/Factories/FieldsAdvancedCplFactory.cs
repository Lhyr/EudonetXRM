using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Tous les éléments qui nécessitent une liste de descid.
    /// </summary>
    public class FieldsAdvancedCplFactory
    {
        ePref Pref { get; set; }
        IEnumerable<int> LstDescId { get; set; }


        #region constructeurs
        /// <summary>
        /// constructeur avec les prefs et une liste de descId
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="lstDescId"></param>
        private FieldsAdvancedCplFactory(ePref pref, IEnumerable<int> lstDescId)
        {
            Pref = pref;
            LstDescId = lstDescId;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour créer un élément FieldsAdvancedCplFactory qui permet pour une
        /// liste de champs de récupérer leurs éléments.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="lstDescId"></param>
        /// <returns></returns>
        public static FieldsAdvancedCplFactory InitFieldsAdvancedCplFactory(ePref pref, IEnumerable<int> lstDescId)
        {
            return new FieldsAdvancedCplFactory(pref, lstDescId);
        }

        #endregion

        #region public


        #region GetDescAdvDataSet
        /// <summary>
        /// Recupère les éléments dans descadv suivant le champ.
        /// </summary>
        /// <returns></returns>
        public DescAdvDataSet GetDescAdvDataSet()
        {
            DescAdvDataSet descAdv;
            using (eudoDAL dal = eLibTools.GetEudoDAL(Pref))
            {
                if (LstDescId.Count() > 0)
                {
                    dal.OpenDatabase();
                    descAdv = new DescAdvDataSet();
                    descAdv.LoadAdvParams(dal, LstDescId);

                    return descAdv;
                }

                return null;
            }
        }
        #endregion

        #region GetResInternal
        /// <summary>
        /// Retourne une liste de res pour une liste de descid.
        /// </summary>
        /// <returns></returns>
        public eResInternal GetResInternal()
        {

            if (LstDescId.Count() > 0)
                return new eResInternal(Pref, LstDescId);

            return null;
        }
        #endregion


        #endregion



    }
}