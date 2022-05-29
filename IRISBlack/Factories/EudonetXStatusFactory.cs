using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory qui permet de gérer les modèles pour récupérer/mettre à jour le statut d'activation des fonctionnalités Eudonet X (Mode Fiche IRIS Black, Mode Liste IRIS Crimson, Saisie Guidée IRIS Purple...)
    /// </summary>
    public class EudonetXStatusFactory
    {

        #region Properties
        /// <summary>
        /// Objet TableInfos regroupant les informations nécessaires
        /// </summary>
        eAdminTableInfos TableInfos { get; set; }
        /// <summary>
        /// Objet Pref pour l'accès en base de données
        /// </summary>
        ePref Pref { get; set; }

        #endregion

        #region Constructeur

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="_tabInfos">Objet TableInfos regroupant les informations nécessaires</param>
        /// <param name="_pref">Objet Pref pour l'accès en base de données</param>
        private EudonetXStatusFactory(eAdminTableInfos _tabInfos, ePref _pref)
        {
            TableInfos = _tabInfos;
            Pref = _pref;
        }

        #endregion

        #region Initialiseurs statiques
        /// <summary>
        /// Initialiseur statique de la classe avec tous les paramètres
        /// </summary>
        /// <param name="_tabInfos"></param>
        /// <param name="_pref"></param>
        /// <returns></returns>
        public static EudonetXStatusFactory InitEudonetXStatusFactory(eAdminTableInfos _tabInfos, ePref _pref)
        {
            return new EudonetXStatusFactory(_tabInfos, _pref);
        }

        #endregion

        #region Public

        /// <summary>
        /// Permet depuis un Json récupéré en back d'avoir
        /// un objet FileLayoutModel.
        /// </summary>
        /// <returns></returns>
        public EudonetXStatusModel GetEudonetXStatus()
        {
            EudonetXStatusModel exsm = new EudonetXStatusModel
            {
                Tab = TableInfos.DescId,
                EudonetXIrisBlackStatus = TableInfos.EudonetXIrisBlackStatus,
                EudonetXIrisCrimsonListStatus = TableInfos.EudonetXIrisCrimsonListStatus,
                EudonetXIrisPurpleGuidedStatus = TableInfos.EudonetXIrisPurpleGuidedStatus,
            };

            return exsm;
        }

        /// <summary>
        /// Permet de mettre à jour en base le statut d'activation d'une ou plusieurs fonctionnalités Eudonet X
        /// un objet FileLayoutModel.
        /// </summary>
        /// <param name="oldEXSM">Modèle énumérant la ou les statuts d'activation des fonctionnalités Eudonet X actuellement présentes en base</param>
        /// <param name="newEXSM">Modèle énumérant la ou les statuts d'activation des fonctionnalités Eudonet X à mettre à jour</param>
        /// <returns>Le résultat de l'opération de mise à jour</returns>
        public eAdminResult SetEudonetXStatus(EudonetXStatusModel oldEXSM, EudonetXStatusModel newEXSM)
        {
            EudonetXStatusModel updatedEXSM = oldEXSM;

            // MAJ en base si le paramètre est défini/passé en entrée dans le Model (si le Front ne l'envoie pas, le modèle le mettra par défaut en UNDEFINED
            eAdminDescAdv descAdv = new eAdminDescAdv(Pref);
            if (newEXSM.EudonetXIrisBlackStatus != EUDONETX_IRIS_BLACK_STATUS.UNDEFINED)
            {
                descAdv.Add(newEXSM.Tab, DESCADV_PARAMETER.ERGONOMICS_IRIS_BLACK, newEXSM.EudonetXIrisBlackStatus.GetHashCode().ToString());
                updatedEXSM.EudonetXIrisBlackStatus = newEXSM.EudonetXIrisBlackStatus;
            }
            if (newEXSM.EudonetXIrisCrimsonListStatus != EUDONETX_IRIS_CRIMSON_LIST_STATUS.UNDEFINED)
            {
                descAdv.Add(newEXSM.Tab, DESCADV_PARAMETER.ERGONOMICS_LIST_IRIS_CRIMSON, newEXSM.EudonetXIrisCrimsonListStatus.GetHashCode().ToString());
                updatedEXSM.EudonetXIrisCrimsonListStatus = newEXSM.EudonetXIrisCrimsonListStatus;
            }
            if (newEXSM.EudonetXIrisPurpleGuidedStatus != EUDONETX_IRIS_PURPLE_GUIDED_STATUS.UNDEFINED)
            {
                descAdv.Add(newEXSM.Tab, DESCADV_PARAMETER.ERGONOMICS_GUIDED_IRIS_PURPLE, newEXSM.EudonetXIrisPurpleGuidedStatus.GetHashCode().ToString());
                updatedEXSM.EudonetXIrisPurpleGuidedStatus = newEXSM.EudonetXIrisPurpleGuidedStatus;
            }
            var adminRes = descAdv.SaveDescAdv();

            // Retour
            adminRes.ReturnObject = updatedEXSM;
            return adminRes;
        }
        #endregion
    }
}