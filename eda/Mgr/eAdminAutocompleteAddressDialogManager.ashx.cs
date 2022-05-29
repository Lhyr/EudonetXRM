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
    public class eAdminAutocompleteAddressDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminAutocompleteAddressUpdate
        {
            [DataMember]
            public int searchField;
            [DataMember]
            public Rubrique houseNumber;
            [DataMember]
            public Rubrique streetName;
            [DataMember]
            public Rubrique postalCode;
            [DataMember]
            public Rubrique city;
            [DataMember]
            public Rubrique cityCode;
            [DataMember]
            public Rubrique departmentNumber;
            [DataMember]
            public Rubrique department;
            [DataMember]
            public Rubrique region;
            [DataMember]
            public Rubrique country;
            [DataMember]
            public Rubrique geography;
            [DataMember]
            public Rubrique label;
            [DataMember]
            public bool midFormula;
            [DataMember]
            public bool pmAutomation;
        }

        [DataContract]
        private class Rubrique
        {
            [DataMember]
            public int value;
            [DataMember]
            public int id;
            [DataMember]
            public bool addcatvalue;
        }

        [DataContract]
        private class eAdminAutocompleteAddressDialogResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        private int _nTab;
        private eAdminAutocompleteAddressUpdate _mappings;
        private eudoDAL _edal;

        private List<eFilemapPartner> _listExistingMappings;

        private eAdminTableInfos _tabInfos;

        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string mappingsJson = _requestTools.GetRequestFormKeyS("mappingsJson") ?? String.Empty;

            try
            {
                _mappings = JsonConvert.DeserializeObject<eAdminAutocompleteAddressUpdate>(mappingsJson);
            }
            catch (JsonReaderException ex)
            {
                //
            }

            if (_nTab == 0 || _mappings == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des mappings de la recherche d'adresses prédictive : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    _tabInfos = new eAdminTableInfos(_pref, _nTab);

                    //nettoyage de tous les descid avec autocomplete enabled (en excluant ceux qui sont utilisés pour Sirene ou qui correspondent au DescId que l'on s'apprête à définir comme nouveau déclencheur)
                    Dictionary<Int32, Int32> dicoAutocompleteAddress = _tabInfos.GetAutocompleteAddressFields(_pref);
                    string[] sireneEnabledFields = eSireneMapping.GetSireneEnabledFields(_pref, _tabInfos.DescId);
                    foreach (
                        KeyValuePair<Int32, Int32> mappingAutoCompletion in dicoAutocompleteAddress.Where(
                            mp => (
                                Field.AutoCompletionEnabledStatic((AutoCompletion)mp.Value) == true &&
                                !sireneEnabledFields.Contains(mp.Key.ToString()) &&
                                !sireneEnabledFields.Contains(_mappings.searchField.ToString())
                            )
                        )
                    )
                    {
                        if (_mappings.searchField != 0 && _mappings.searchField == mappingAutoCompletion.Key)
                            continue;

                        eAdminDesc field = new eAdminDesc(mappingAutoCompletion.Key);
                        field.SetDesc(eLibConst.DESC.AUTOCOMPLETION, "");
                        field.Save(_pref, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }

                    //modification du descid avec autocomplete enabled
                    if (_mappings.searchField != 0)
                    {
                        AutoCompletion flags = AutoCompletion.ENABLED;

                        // les formules ainsi que l'automatisme PM sont toujours pris en compte

                        //if (_mappings.midFormula)
                        //    flags |= AutoCompletion.MID_FORMULA_ENABLED;

                        //if (_tabInfos.DescId == TableType.PM.GetHashCode() && _mappings.pmAutomation)
                        //    flags |= AutoCompletion.PM_AUTOMATION_ENABLED;

                        eAdminDesc field = new eAdminDesc(_mappings.searchField);
                        field.SetDesc(eLibConst.DESC.AUTOCOMPLETION, flags.GetHashCode().ToString());
                        field.Save(_pref, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }

                    _listExistingMappings = eAutoCompletionTools.Load(_pref, _nTab, EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE, EudoQuery.AutoCompletionType.AUTO_COMPLETION_ADDRESS, false, out error);
                    if (error.Length != 0)
                        throw new Exception(error);

                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();

                    // Ajout/mise à jour/suppression des mappings
                    ProcessRubrique(_mappings.houseNumber, eModelConst.AutocompleteAddressMappings.NoVOIE);
                    ProcessRubrique(_mappings.streetName, eModelConst.AutocompleteAddressMappings.RUE);
                    ProcessRubrique(_mappings.postalCode, eModelConst.AutocompleteAddressMappings.CODEPOSTAL);
                    ProcessRubrique(_mappings.city, eModelConst.AutocompleteAddressMappings.VILLE);
                    ProcessRubrique(_mappings.cityCode, eModelConst.AutocompleteAddressMappings.INSEE);
                    ProcessRubrique(_mappings.departmentNumber, eModelConst.AutocompleteAddressMappings.NoDEPARTEMENT);
                    ProcessRubrique(_mappings.department, eModelConst.AutocompleteAddressMappings.DEPARTEMENT);
                    ProcessRubrique(_mappings.region, eModelConst.AutocompleteAddressMappings.REGION);
                    ProcessRubrique(_mappings.country, eModelConst.AutocompleteAddressMappings.PAYS);
                    ProcessRubrique(_mappings.geography, eModelConst.AutocompleteAddressMappings.GEOGRAPHY);
                    ProcessRubrique(_mappings.label, eModelConst.AutocompleteAddressMappings.LABEL); // US #1224 - On envoie le libellé complet (adresse complète) du résultat sélectionné dans la fenêtre de recherche


                    // Suppression des anciennes fiches mappings invalides
                    foreach (eFilemapPartner existingMapping in new List<eFilemapPartner>(_listExistingMappings))
                    {
                        DeleteMapping(existingMapping.Id);
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    error = String.Concat("Erreur lors de l'enregistrement des mappings de la minifiche : ", ex.Message);
                }
                finally
                {
                    _edal.CloseDatabase();
                }
            }

            #region Résultat
            eAdminAutocompleteAddressDialogResult result = new eAdminAutocompleteAddressDialogResult()
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

        private void ProcessRubrique(Rubrique rubrique, string sSource)
        {
            if (rubrique.value == 0 && rubrique.id != 0)
                DeleteMapping(rubrique.id);
            else if (rubrique.value != 0 && rubrique.id == 0)
                AddMapping(sSource, rubrique.value, rubrique.addcatvalue);
            else if (rubrique.value != 0 && rubrique.id != 0)
                UpdateMapping(rubrique.id, sSource, rubrique.value, rubrique.addcatvalue);
        }

        private void DeleteMapping(int nMappingId)
        {
            string sError = String.Empty;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE.GetHashCode());

            mapping.DeleteMapping(_edal, nMappingId);

            eFilemapPartner existingMapping = _listExistingMappings.FirstOrDefault(mp => mp.Id == nMappingId);
            if (existingMapping != null)
                _listExistingMappings.Remove(existingMapping);
        }

        private void AddMapping(string sSource, int nDescid, bool addCatValue = false)
        {
            UpdateMapping(0, sSource, nDescid, addCatValue);
        }

        private void UpdateMapping(int nMappingId, string sSource, int nDescid, bool addCatValue = false)
        {
            string sError = String.Empty;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(
                EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE.GetHashCode()
                , ssType: _nTab
                , sourceType: EudoQuery.AutoCompletionType.AUTO_COMPLETION_ADDRESS.GetHashCode()
                , source: sSource
                , descid: nDescid
                , createCatValue: addCatValue
                );

            mapping.SaveFileMapPartner(_edal, nMappingId);

            if (nMappingId != 0)
            {
                eFilemapPartner existingMapping = _listExistingMappings.FirstOrDefault(mp => mp.Id == nMappingId);
                if (existingMapping != null)
                    _listExistingMappings.Remove(existingMapping);
            }
        }
    }
}