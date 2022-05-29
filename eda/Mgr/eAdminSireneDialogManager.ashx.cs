using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminAutocompleteAddressDialogManager
    /// </summary>
    public class eAdminSireneDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminSireneUpdate
        {
            /// <summary>
            /// Champ déclencheur de recherche n°1
            /// </summary>
            [DataMember]
            public int searchField1;
            /// <summary>
            /// Champ déclencheur de recherche n°2
            /// </summary>
            [DataMember]
            public int searchField2;
            /// <summary>
            /// Champ dans lequel sera envoyé le libellé complet d'un résultat de recherche (adresse complète) - US #1224
            /// </summary>
            [DataMember]
            public int resultLabelField;
            /// <summary>
            /// Liste des mappings de champs
            /// </summary>
            [DataMember]
            public List<eAdminSireneUpdateMapping> mappings;
            /// <summary>
            /// Formule du milieu
            /// </summary>
            [DataMember]
            public bool midFormula;
            /// <summary>
            /// Automatismes liés à PM
            /// </summary>
            [DataMember]
            public bool pmAutomation;
        }

        [DataContract]
        private class eAdminSireneUpdateMapping
        {
            // Nom du champ Sirene
            [DataMember]
            public string field;
            /// <summary>
            /// DescID de la rubrique Eudonet correspondant au champ mappé
            /// </summary>
            [DataMember]
            public int descId;
            /// <summary>
            /// Indique, pour un catalogue, si les nouvelles valeurs doivent être ajoutées parmi celles du catalogue
            /// </summary>
            [DataMember]
            public bool addNewValue;
        }

        [DataContract]
        private class eAdminSireneDialogResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        private int _nTab;
        private eAdminSireneUpdate _mappings;
        private eudoDAL _edal;

        private Dictionary<int, List<eSireneMapping>> _listExistingMappings;

        private eAdminTableInfos _tabInfos;

        /// <summary>
        /// Processus exécuté par le Manager
        /// </summary>
        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string mappingsJson = _requestTools.GetRequestFormKeyS("mappingsJson") ?? String.Empty;

            try
            {
                _mappings = JsonConvert.DeserializeObject<eAdminSireneUpdate>(mappingsJson);
            }
            catch (JsonReaderException ex)
            {
                error = ex.Message;
            }

            if (_nTab == 0 || _mappings == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des mappings Sirene : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    _tabInfos = new eAdminTableInfos(_pref, _nTab);

                    //nettoyage de tous les descid avec autocomplete activé, mais uniquement ceux actuellement utilisés par Sirene, en excluant ceux que l'on s'apprête à définir comme nouveaux déclencheurs
                    //(on ne nettoie pas les autres qui peuvent être utilisés par l'adresse prédictive. C'est le manager de la fenêtre d'adresses prédictives qui s'en chargera.)
                    Dictionary<Int32, Int32> dicoAutocompleteAddress = _tabInfos.GetAutocompleteAddressFields(_pref);
                    string[] sireneEnabledFields = eSireneMapping.GetSireneEnabledFields(_pref, _tabInfos.DescId);

                    foreach (string sDescid in sireneEnabledFields)
                    {
                        int nDescid = 0;
                        if (!int.TryParse(sDescid, out nDescid) || nDescid <= 0)
                            continue;

                        int[] iMapSearchFields = { _mappings.searchField1, _mappings.searchField2 };

                        if (iMapSearchFields.Contains(nDescid))
                            continue;

                        eAdminDesc field = new eAdminDesc(nDescid);
                        field.SetDesc(eLibConst.DESC.AUTOCOMPLETION, "");
                        field.Save(_pref, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }

                    //modification du descid avec autocomplete enabled
                    if (_mappings.searchField1 != 0)
                    {
                        AutoCompletion flags = AutoCompletion.ENABLED;

                        // les formules ainsi que l'automatisme PM sont toujours pris en compte

                        //if (_mappings.midFormula)
                        //    flags |= AutoCompletion.MID_FORMULA_ENABLED;

                        //if (_tabInfos.DescId == TableType.PM.GetHashCode() && _mappings.pmAutomation)
                        //    flags |= AutoCompletion.PM_AUTOMATION_ENABLED;

                        eAdminDesc field = new eAdminDesc(_mappings.searchField1);
                        field.SetDesc(eLibConst.DESC.AUTOCOMPLETION, flags.GetHashCode().ToString());
                        field.Save(_pref, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }
                    if (_mappings.searchField2 != 0)
                    {
                        AutoCompletion flags = AutoCompletion.ENABLED;

                        // les formules ainsi que l'automatisme PM sont toujours pris en compte

                        //if (_mappings.midFormula)
                        //    flags |= AutoCompletion.MID_FORMULA_ENABLED;

                        //if (_tabInfos.DescId == TableType.PM.GetHashCode() && _mappings.pmAutomation)
                        //    flags |= AutoCompletion.PM_AUTOMATION_ENABLED;

                        eAdminDesc field = new eAdminDesc(_mappings.searchField2);
                        field.SetDesc(eLibConst.DESC.AUTOCOMPLETION, flags.GetHashCode().ToString());
                        field.Save(_pref, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }

                    _listExistingMappings = eSireneMapping.GetMappings(_pref, out error);
                    if (error.Length != 0)
                        throw new Exception(error);

                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();

                    // Ajout/mise à jour/suppression des mappings
                    if (_mappings != null && _mappings.mappings != null)
                    {
                        foreach (eAdminSireneUpdateMapping updatedMappingData in _mappings.mappings)
                        {
                            eSireneMapping existingMapping = null;
                            if (_listExistingMappings.ContainsKey(_nTab))
                                existingMapping = _listExistingMappings[_nTab].Find(mapping => mapping.Field == updatedMappingData.field);
                            else
                                _listExistingMappings.Add(_nTab, new List<eSireneMapping>());
                            eSireneMapping updatedMapping = new eSireneMapping(updatedMappingData.field, updatedMappingData.descId, updatedMappingData.addNewValue);
                            if (existingMapping == null)
                                _listExistingMappings[_nTab].Add(updatedMapping);
                            else
                            {
                                int existingMappingIndex = _listExistingMappings[_nTab].IndexOf(existingMapping);
                                existingMapping = updatedMapping;
                                _listExistingMappings[_nTab][existingMappingIndex] = existingMapping;
                            }
                        }
                    }

                    // Sauvegarde des champs déclencheurs d'adresses prédictives Sirene
                    // US #1224 - Sauvegarde le DescId du champ destiné à recevoir la ligne de résultat complète lors du clic sur un résultat ("Adresse complète")
                    eAdminResult adminResult = eSireneMapping.SaveSireneEnabledAndResultLabelFields(_pref, _nTab, new int[] { _mappings.searchField1, _mappings.searchField2 }, _mappings.resultLabelField);
                    if (!adminResult.Success)
                        error = String.Concat(error, " - ", adminResult.UserErrorMessage);

                    // Sauvegarde des champs "utilisateur"
                    error = eSireneMapping.SaveMappings(_pref, _listExistingMappings);

                    success = String.IsNullOrEmpty(error);

                    if (!success)
                        throw new Exception(error);
                }
                catch (Exception ex)
                {
                    error = String.Concat("Erreur lors de l'enregistrement des mappings du référentiel Sirene : ", ex.Message);
                }
                finally
                {
                    _edal.CloseDatabase();
                }
            }

            #region Résultat
            eAdminSireneDialogResult result = new eAdminSireneDialogResult()
            {
                Success = success,
                Error = error
            };

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });
            #endregion
        }
    }
}