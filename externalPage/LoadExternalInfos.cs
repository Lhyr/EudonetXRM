using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Com.Eudonet.Internal;
using Com.Eudonet.Merge;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <className>LoadUnsubInfos</className>
    /// <summary>Charge les informations de catégorie de la campagne et le mail du receveur pour la désinscription</summary>
    /// <purpose></purpose>
    /// <authors>HLA</authors>
    /// <date>2014-03-10</date>
    public class LoadExternalInfos
    {
        /// <summary>
        /// Connexion SQL
        /// </summary>
        protected eudoDAL _dal;
        /// <summary>
        /// Pref de l'utilisateur
        /// </summary>
        protected ePrefLite _pref;

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="dal">connexion à la base</param>
        /// <param name="pref">preflite avec le userid de EDN_TRACKS</param>
        /// <param name="unsubParam">Information sur le lien de désinscription</param>
        /// <param name="campaignId">Id de la campagne</param>
        public LoadExternalInfos(eudoDAL dal, ePrefLite pref, ExternalUrlParamMailing unsubParam, Int32 campaignId)
        {
            this._dal = dal;
            this._pref = pref;
        }

        /// <summary>
        /// Construit un datafiller pour charger la DisplayValue de la rubrique demandé
        /// Utiliser la méthode "GetFieldsValue" si le besoin se fait sur plusieurs rubriques
        /// </summary>
        /// <param name="descid">descid de la rubrique</param>
        /// <param name="fileId">id de la fiche</param>
        /// <param name="isEdnUser">Indique que l'utilisateur est un utilisateur système "EDN_xxxx"</param>
        /// <returns></returns>
        protected eFieldRecord GetFieldValue(Int32 descid, Int32 fileId, Boolean isEdnUser = false)
        {
            IDictionary<Int32, eFieldRecord> listFldRecord =
                eDataFillerGeneric.GetFieldsValue(_pref, new HashSet<Int32>() { descid }, eLibTools.GetTabFromDescId(descid), fileId, isEdnUser);

            if (listFldRecord.Count > 0)
            {
                eFieldRecord fldRecord = null;
                if (listFldRecord.TryGetValue(descid, out fldRecord))
                    return fldRecord;
            }

            throw new Exception(String.Concat("Rubrique ", descid, " non trouvé."));
        }
    }
}