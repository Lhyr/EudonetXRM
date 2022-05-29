using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Model
{
    public interface IEngineReturnModel
    {
        /// <summary>
        /// Indique s'il s'agit d'une opération de Mise àjour 
        /// </summary>
        EngineOperationType OperationType { get; set; }
        IReloadInfosModel ReloadInfos { get; set; }

        int MainTab { get; set; }
        List<int> ReloadBkms { get; set; }

        List<int> DescidRuleUpdated { get; set; }

        /// <summary>
        /// Liste de messages fonctionnels (ex : ORM) à afficher (différents des messages d'erreur)
        /// </summary>
        List<CustomMessage> Messages { get; set; }

        /// <summary>
        /// Liste de messages d'erreur à afficher (différents des messages fonctionnels type ORM)
        /// </summary>
        List<CustomMessage> ErrorMessages { get; set; }
        /// <summary>
        /// Mode d'affichage de la boîte de dialogue liée à une formule du milieu
        /// </summary>
        EngineConfirmMode ConfirmBoxMode { get; set; }
        /// <summary>
        /// Liste des attributs publics exposés par Engine pour afficher la boîte de dialogue liée à une formule du milieu
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> ConfirmBoxInfoAttributes { get; set; }
        /// <summary>
        /// Liste des éléments publics exposés par Engine pour afficher la boîte de dialogue liée à une formule du milieu
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> ConfirmBoxInfoElements { get; set; }
        /// <summary>
        ///Autres paramètres renvoyés par EngineResult
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> OtherParameters { get; set; }

        List<string> V7URLs { get; set; }
        List<string> URLs { get; set; }

        List<ListRefreshFieldNewValue> RefreshFieldNewValues { get; set; }
    }

    public interface IReloadInfosModel
    {
        bool ReloadHeader { get; set; }
        bool ReloadDetail { get; }
        bool ReloadFileHeader { get; }
    }
    public enum EngineOperationType
    {
        UNDEFINED = 0,
        CREATE = 1,
        UPDATE = 2,
        DELETE = 3,
        MERGE = 4
    }

}