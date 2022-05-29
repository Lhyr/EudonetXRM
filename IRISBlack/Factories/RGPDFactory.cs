using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour la gestion des caractéristiques RGPD
    /// </summary>
    public class RGPDFactory
    {
        /// <summary>
        /// Objet Pref
        /// </summary>
        private ePref _pref;

        #region static initializers
        /// <summary>
        /// Initialisation de la Factory
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <returns>Objet RGPDFactory</returns>
        public static RGPDFactory initRGPDFactory(ePref pref)
        {
            return new RGPDFactory(pref);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref">objet Pref</param>
        private RGPDFactory(ePref pref)
        {
            _pref = pref;
        }
        #endregion

        #region Méthodes
        /// <summary>
        /// Récupère les paramètres RGPD à partir de DESCADV pour le champ (DescID) concerné
        /// </summary>
        /// <param name="descIds">DescIDs des champs pour lesquels récupérer les données RGPD</param>
        /// <returns>Paramètres RGPD depuis DESCADV</returns>
        public IDictionary<int, RGPDModel> GetRGPDData(IEnumerable<int> descIds)
        {
            IDictionary<int, RGPDModel> rgpdData = new Dictionary<int, RGPDModel>();

            #region Récupération des paramètres
            eudoDAL dal = null;
            DescAdvDataSet descAdv = new DescAdvDataSet();
            try
            {
                dal = eLibTools.GetEudoDAL(_pref);
                dal.OpenDatabase();
                descAdv.LoadAdvParams(dal, descIds, eDataQualityTools.GetListRGPDDescAdvParameter());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();
            }
            #endregion

            #region Renvoi des données à partir des paramètres trouvés en base
            // Tâche #3123 : on ne renvoie les infos RGPD que pour les champs où la gestion RGPD est activée, même si un champ avec RGPD désactivée dispose d'infos RGPD en base
            descIds.Where(f => descAdv.GetAdvInfoValue(f, DESCADV_PARAMETER.RGPD_ENABLED, "0") == "1").ForEach(fld =>
            {
                int nTypeCat = 0;

                RGPDModel rgpdM = new RGPDModel
                {
                    Enabled = true,
                    NatureType = eLibTools.GetEnumFromCode<DESCADV_RGPD_NATURE>(descAdv.GetAdvInfoValue(fld, DESCADV_PARAMETER.RGPD_NATURE, ((int)DESCADV_RGPD_DEFAULT_VALUES.NATURE).ToString()), true)
                };

                rgpdM.NatureLabel = eDataQualityTools.GetNatureLabel(_pref, rgpdM.NatureType);

                if (rgpdM.NatureType == DESCADV_RGPD_NATURE.SENSITIVE)
                {
                    rgpdM.CategoryType = descAdv.GetAdvInfoValue(fld, DESCADV_PARAMETER.RGPD_SENSIBLE_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.SENSITIVE_CATEGORY).ToString());
                    rgpdM.CategoryLabel = eDataQualityTools.GetCategoryLabel(_pref, eLibTools.GetEnumFromCode<DESCADV_RGPD_SENSITIVE_CATEGORY>(rgpdM.CategoryType, true));
                }
                else if (rgpdM.NatureType == DESCADV_RGPD_NATURE.PERSONAL)
                {
                    rgpdM.CategoryType = descAdv.GetAdvInfoValue(fld, DESCADV_PARAMETER.RGPD_PERSONNAL_CATEGORY, ((int)DESCADV_RGPD_DEFAULT_VALUES.PERSONAL_CATEGORY).ToString());
                    rgpdM.CategoryLabel = eDataQualityTools.GetCategoryLabel(_pref, eLibTools.GetEnumFromCode<DESCADV_RGPD_PERSONNAL_CATEGORY>(rgpdM.CategoryType, true));
                }

                if (int.Parse(rgpdM.CategoryType) == 1)
                {
                    rgpdM.CategoryLabel = descAdv.GetAdvInfoValue(fld, DESCADV_PARAMETER.RGPD_CATEGORY_PRECISION);
                }

                if(!rgpdData.ContainsKey(fld))
                    rgpdData.Add(fld, rgpdM);
            });
            #endregion

            return rgpdData;
        }

        #endregion
    }
}