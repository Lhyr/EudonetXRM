using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Manager de eAdminTabs
    /// </summary>
    public class eAdminExtensionSireneManager : eAdminManager
    {

        /// <summary>
        /// Gestion de la demande de rendu du menu d'admin
        /// </summary>
        protected override void ProcessManager()
        {
            string strError = String.Empty;

            // Initialisation
            string strAction = _requestTools.GetRequestFormKeyS("action") ?? "changeField";
            string strSection = _requestTools.GetRequestFormKeyS("section") ?? String.Empty;
            string strField = _requestTools.GetRequestFormKeyS("field") ?? String.Empty;
            int nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            int nDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            bool bAddNewValue = _requestTools.GetRequestFormKeyB("addnewvalue") ?? false;

            // Chargement des mappings existants
            string error = String.Empty;
            Dictionary<int, List<eSireneMapping>> mappings = eSireneMapping.GetMappings(_pref, out error);
            if (!String.IsNullOrEmpty(error))
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 7854)); // "La mise à jour du mapping a échoué"
                LaunchError();
            }
            
            JSONReturnExtensionSirene res = new Mgr.JSONReturnExtensionSirene();

            switch (strAction)
            {
                case "changeField":
                    // Création du mapping pour ce tabId si inexistant
                    if (!mappings.ContainsKey(nTab))
                        mappings.Add(nTab, new List<eSireneMapping>());

                    // Récupération de la ligne de mapping correspondant au champ, ou création si inexistant
                    eSireneMapping existingMapping = mappings[nTab].Find(mapping => mapping.Field == strField);
                    if (existingMapping == null)
                    {
                        existingMapping = new eSireneMapping(strField, nDescId, bAddNewValue);
                        mappings[nTab].Add(existingMapping);
                    }
                    else
                    {
                        int existingMappingIndex = mappings[nTab].IndexOf(existingMapping);
                        existingMapping.AddNewValue = bAddNewValue;
                        existingMapping.DescId = nDescId;
                        mappings[nTab][existingMappingIndex] = existingMapping;
                    }

                    // Sauvegarde de l'ensemble des mappings en base
                    string result = eSireneMapping.SaveMappings(_pref, mappings);
                    if (!String.IsNullOrEmpty(result))
                    {
                        ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), String.Concat(eResApp.GetRes(_pref, 7854), " - ", error)); // "La mise à jour du mapping a échoué"
                        LaunchError();
                    }
                    else
                    {
                        res.Action = "changeField";
                        res.Section = strSection;
                        res.Key = strField;
                        res.Success = true;
                        res.Result = "1";
                        RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Classe d'extension JSON pour les envois et retours du manager gérant le paramétrage Sirene
    /// </summary>
    public class JSONReturnExtensionSirene : JSONReturnGeneric
    {
        /// <summary>
        /// Action à effectuer/action effectuée (ex : changeField)
        /// </summary>
        public string Action = String.Empty;

        /// <summary>
        /// Section concernée
        /// </summary>
        public string Section = String.Empty;

        /// <summary>
        /// Clé correspondant à la modification à effectuer
        /// </summary>
        public string Key = String.Empty;

        /// <summary>
        /// Résultat de l'opération
        /// </summary>
        public string Result = String.Empty;
    }
}