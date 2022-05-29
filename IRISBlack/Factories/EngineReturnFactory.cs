using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class EngineReturnFactory
    {
        EngineResult EngineResult { get; set; }



        #region constructeurs
        /// <summary>
        /// Constructeur Pour la factory
        /// </summary>
        /// <param name="egnR"></param>
        private EngineReturnFactory(EngineResult egnR)
        {
            EngineResult = egnR;
        }
        #endregion

        #region Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe FldTypedInfosFactory.
        /// </summary>
        /// <param name="egnR"></param>
        /// <returns></returns>
        public static EngineReturnFactory initEngineReturnFactory(EngineResult egnR)
        {
            return new EngineReturnFactory(egnR);
        }
        #endregion

        #region privée
        /// <summary>
        /// Permet de retourner une instance de EngineReturnModel initialisé avec 
        /// les premiers éléments de EngineResult.
        /// </summary>
        /// <returns></returns>
        private void GetEngineReturnFactory(IEngineReturnModel engRM)
        {

            engRM.MainTab = EngineResult.Record.Tab;
            engRM.ReloadInfos = new ReloadInfosModel
            {
                ReloadHeader = EngineResult.ReloadHeader,
                ReloadDetail = EngineResult.ReloadDetail,
                ReloadFileHeader = EngineResult.ReloadFileHeader
            };
            engRM.RefreshFieldNewValues = new List<ListRefreshFieldNewValue>();
            engRM.DescidRuleUpdated = EngineResult.ListDescidRuleUpdated?.Select(fru => fru.Descid).ToList();
            engRM.ReloadBkms = EngineResult.RefreshBkm.ToList();
            engRM.Messages = EngineResult.ListProcMsgBox.
            GroupBy(x => new { x.Title, x.Msg, x.Detail, x.DescId }).
            Select(g => g.First()).Select(msg => new CustomMessage
            {
                Criticity = eLibConst.MSG_TYPE.INFOS,               //Les messages issus de l'ORM ne fournissent pas le niveau de criticité et il est indiqué à CRITICAL par défaut... c'est la solution la moins invasive pour corriger le pb
                Title = msg.Title,
                Description = msg.Msg,
                Detail = msg.Detail
            }).ToList();

            // On stocke séparément les membres de EngineResult.Confirm (ResultConfirmBox) car tous ne sont pas sérialisables (publics)
            // On utilise les dictionnaires XmlAttributes et XmlElements fournis par les classes pour ne récupérer que ce qu'elles souhaitent exposer
            engRM.ConfirmBoxMode = EngineResult.Confirm.Mode;
            engRM.ConfirmBoxInfoAttributes = EngineResult.Confirm.BoxInfo?.XmlAttributes();
            engRM.ConfirmBoxInfoElements = EngineResult.Confirm.BoxInfo?.XmlElements();
            engRM.OtherParameters = EngineResult.GetOthersParam;

            #region pages spécifiques à appeler en retour
            engRM.V7URLs = EngineResult.ListProcPg.ToList();
            engRM.URLs = EngineResult.ListSpecifXRM.Select(specif => specif.GetUrlParams()).ToList();
            #endregion

            if (EngineResult.ListRefreshFields != null)
                engRM.RefreshFieldNewValues.AddRange(EngineResult.ListRefreshFields);

            #region gestion des retours de formules
            if (EngineResult.Error != null)
            {
                CustomMessage errorCustomMessage = new CustomMessage();
                errorCustomMessage.Criticity = EngineResult.Error.TypeCriticity;
                errorCustomMessage.Title = EngineResult.Error.Title;
                errorCustomMessage.Description = EngineResult.Error.Msg;
                errorCustomMessage.Detail = EngineResult.Error.Detail;
                if (eLibTools.IsLocalOrEudoMachine())
                    errorCustomMessage.DebugMsg = EngineResult.Error.DebugMsg;


                engRM.ErrorMessages = new List<CustomMessage>();
                engRM.ErrorMessages.Add(errorCustomMessage);
            }
            #endregion gestion des retours de formules
        }
        /// <summary>
        /// Permet d'instancier un objet CreateReturnModel, qui hérite de EngineReturnModel
        /// en passant en paramètres un objet IEngineReturnModel qui possède déjà les éléments de base
        /// </summary>
        /// <returns></returns>
        private CreateReturnModel GetCreateReturnModel()
        {
            CreateReturnModel crm = new CreateReturnModel();
            GetEngineReturnFactory(crm);

            crm.OperationType = EngineOperationType.CREATE;
            crm.FilesId = EngineResult.NewRecord.FilesId;

            return crm;
        }
        /// <summary>
        /// Permet d'instancier un objet UpdateReturnModel, qui hérite de EngineReturnModel
        /// en passant en paramètres un objet IEngineReturnModel qui possède déjà les éléments de base
        /// </summary>
        /// <returns></returns>
        private UpdateReturnModel GetUpdateReturnModel()
        {
            UpdateReturnModel urm = new UpdateReturnModel();
            GetEngineReturnFactory(urm);

            urm.OperationType = EngineOperationType.UPDATE;
            urm.Id = EngineResult.Record.Id;
            urm.IsLinkChanged = EngineResult.IsLinkChange;
            urm.IsHistoChanged = EngineResult.HistoFieldChanged;

            return urm;
        }
        #endregion
        #region public
        /// <summary>
        /// renvoie le bon modele en fonction de l'operation réalisée
        /// </summary>
        /// <returns></returns>
        public IEngineReturnModel GetEngineResult()
        {

            IEngineReturnModel returnModel;
            EngineOperationType operationType = EngineOperationType.UNDEFINED;

            if (!EngineResult.NewRecord.Empty)
                operationType = EngineOperationType.CREATE;
            else if (!EngineResult.DelRecord.Empty)
                operationType = EngineOperationType.DELETE;
            else if (!EngineResult.MergeRecord.Empty)
                operationType = EngineOperationType.MERGE;
            else
                operationType = EngineOperationType.UPDATE;


            switch (operationType)
            {
                case EngineOperationType.CREATE:
                    returnModel = GetCreateReturnModel();
                    break;
                case EngineOperationType.UPDATE:
                    returnModel = GetUpdateReturnModel();
                    break;
                case EngineOperationType.DELETE:
                    throw new NotImplementedException();
                case EngineOperationType.MERGE:
                    throw new NotImplementedException();
                default:
                    throw new EudoException("Le systeme n'a pas pu déterminer le type d'opération réalisée");
            }

            return returnModel;
        }
        #endregion

    }
}