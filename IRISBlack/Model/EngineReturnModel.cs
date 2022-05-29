using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{

    public class CustomMessage
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Detail { get; set; }
        public string DebugMsg { get; set; }
        public eLibConst.MSG_TYPE Criticity { get; set; }
    }

    public class EngineReturnModel : IEngineReturnModel
    {
        public EngineOperationType OperationType { get; set; }

        public IReloadInfosModel ReloadInfos { get; set; }

        public int MainTab { get; set; }
        public List<int> ReloadBkms { get; set; }

        public List<int> DescidRuleUpdated { get; set; }

        /// <summary>
        /// Liste de messages fonctionnels (ex : ORM) à afficher (différents des messages d'erreur)
        /// </summary>
        public List<CustomMessage> Messages { get; set; }

        /// <summary>
        /// Liste de messages d'erreur à afficher (différents des messages fonctionnels type ORM)
        /// </summary>
        public List<CustomMessage> ErrorMessages { get; set; }
        /// <summary>
        /// Mode d'affichage de la boîte de dialogue liée à une formule du milieu
        /// </summary>
        public EngineConfirmMode ConfirmBoxMode { get; set; }
        /// <summary>
        /// Liste des attributs publics exposés par Engine pour afficher la boîte de dialogue liée à une formule du milieu
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ConfirmBoxInfoAttributes { get; set; }
        /// <summary>
        /// Liste des éléments publics exposés par Engine pour afficher la boîte de dialogue liée à une formule du milieu
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> ConfirmBoxInfoElements { get; set; }
        /// <summary>
        ///Autres paramètres renvoyés par EngineResult
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> OtherParameters { get; set; }

        public List<string> V7URLs { get; set; }
        public List<string> URLs { get; set; }

        public List<ListRefreshFieldNewValue> RefreshFieldNewValues { get; set; } = new List<ListRefreshFieldNewValue>();
    }
}