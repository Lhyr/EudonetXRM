using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class ActionDetailFactory
    {

        #region variables
        DescAdvDataSet descAdv { get; set; } = new DescAdvDataSet();
        ePref Pref { get; set; }
        int nDescId { get; set; }
        #endregion

        #region constructeurs

        /// <summary>
        /// Constructeur privé initialisant la factory;
        /// </summary>
        private ActionDetailFactory() { }
        /// <summary>
        /// Constructeur privé initialisant la factory avec un objet de type DescAdvDataSet;
        /// </summary>
        /// <param name="_descId"></param>
        /// <param name="_pref"></param>
        private ActionDetailFactory(int _descId, ePref _pref)
        {
            nDescId = _descId;
            Pref = _pref;
        }
        #endregion
        #region  Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe ActionDetailFactory.
        /// </summary>
        /// <returns></returns>
        public static ActionDetailFactory initActionDetailFactory()
        {
            return new ActionDetailFactory();
        }
        /// <summary>
        /// Initialisation statique de la classe ActionDetailFactory avec l'objet de type Table.
        /// </summary>
        /// <param name="_descId"></param>
        /// <param name="_pref"></param>
        /// <returns></returns>
        public static ActionDetailFactory initActionDetailFactory(int _descId, ePref _pref)
        {
            return new ActionDetailFactory(_descId, _pref);
        }
        #endregion
        #region private

        /// <summary>
        /// charge les informations pour le mode fiche téléguidé
        /// Copier-coller de Bookmark... :D
        /// </summary>
        private bool loadPurpleActions()
        {
            try
            {
                using (eudoDAL dal = eLibTools.GetEudoDAL(Pref))
                {
                    dal.OpenDatabase();
                    descAdv.LoadAdvParams(eDal: dal,
                        listDescid: new List<int> { nDescId },
                        searchedParams: new List<DESCADV_PARAMETER> { DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM }
                        );
                }

                List<LOCATION_PURPLE_ACTIVATED> locationsPurpleActivated = descAdv.GetAdvInfoValue(nDescId, DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM)
                    .ConvertToList<LOCATION_PURPLE_ACTIVATED>(",", new Converter<string, LOCATION_PURPLE_ACTIVATED?>(
                        (string s) =>
                        {
                            LOCATION_PURPLE_ACTIVATED location = LOCATION_PURPLE_ACTIVATED.UNDEFINED;
                            if (!Enum.TryParse<LOCATION_PURPLE_ACTIVATED>(s, out location))
                                return LOCATION_PURPLE_ACTIVATED.UNDEFINED;

                            return location;
                        }));

                return locationsPurpleActivated.Contains(LOCATION_PURPLE_ACTIVATED.MENU);
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
        #region public
        /// <summary>
        /// Retourne le modèle recouvrant les informations propres à un onglet.
        /// </summary>
        /// <returns></returns>
        public ActionDetailModel getActionDetailModel()
        {

            return new ActionDetailModel
            {
                AddPurpleFileFromMenu = loadPurpleActions(),
            };
        }
        #endregion





    }
}