using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminMapTooltipDialogManager
    /// </summary>
    public class eAdminMapTooltipDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminMapTooltipUpdate
        {
            [DataMember]
            public Rubrique Geography;
            [DataMember]
            public Rubrique Title;
            [DataMember]
            public Rubrique Subtitle;
            [DataMember]
            public Rubrique Rubrique1;
            [DataMember]
            public Rubrique Rubrique2;
            [DataMember]
            public Rubrique Rubrique3;
            [DataMember]
            public Rubrique Rubrique4;
            [DataMember]
            public Rubrique Rubrique5;
            [DataMember]
            public Rubrique Image;
        }

        [DataContract]
        private class Rubrique
        {
            [DataMember]
            public int value;
            [DataMember]
            public int id;
            [DataMember]
            public bool label;
        }

        [DataContract]
        private class eAdminMapTooltipDialogResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        #region propriétés
        private int _nTab;
        private eAdminMapTooltipUpdate _mappings;
        private eudoDAL _edal;

        private List<eFilemapPartner> _listExistingMappings;

        private eAdminTableInfos _tabInfos;
        #endregion

        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string mappingsJson = _requestTools.GetRequestFormKeyS("mappingsJson") ?? String.Empty;

            try
            {
                _mappings = JsonConvert.DeserializeObject<eAdminMapTooltipUpdate>(mappingsJson);
            }
            catch (JsonReaderException ex)
            {
                //
            }

            if (_nTab == 0 || _mappings == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des mappings de l'infobulle de position : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    _tabInfos = new eAdminTableInfos(_pref, _nTab);
                    _listExistingMappings = eFilemapPartner.LoadCartoMapping(_pref, _nTab, out error);
                    if (error.Length != 0)
                        throw new Exception(error);

                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();



                    //Géocodage
                    ProcessRubrique(_mappings.Geography, CartographySsType.GEOGRAPHY, 0);

                    //Titre
                    ProcessRubrique(_mappings.Title, CartographySsType.TITLE, 0);

                    //Sous-titre
                    ProcessRubrique(_mappings.Subtitle, CartographySsType.SUBTITLE, 0);

                    //Rubriques 1 à 5
                    int order = 0;
                    ProcessRubrique(_mappings.Rubrique1, CartographySsType.FIELD, ++order);
                    ProcessRubrique(_mappings.Rubrique2, CartographySsType.FIELD, ++order);
                    ProcessRubrique(_mappings.Rubrique3, CartographySsType.FIELD, ++order);
                    ProcessRubrique(_mappings.Rubrique4, CartographySsType.FIELD, ++order);
                    ProcessRubrique(_mappings.Rubrique5, CartographySsType.FIELD, ++order);

                    //Image
                    ProcessRubrique(_mappings.Image, CartographySsType.IMAGE, 0);

                    //Suppression des anciennes fiches mappings invalides
                    foreach (eFilemapPartner existingMapping in new List<eFilemapPartner>(_listExistingMappings))
                    {
                        DeleteMapping(existingMapping.Id);
                    }

                    success = true;
                }
                catch (Exception ex)
                {
                    error = String.Concat("Erreur lors de l'enregistrement des mappings de l'infobulle de position : ", ex.Message);
                }
                finally
                {
                    _edal.CloseDatabase();
                }
            }

            #region Résultat
            eAdminMapTooltipDialogResult result = new eAdminMapTooltipDialogResult()
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

        private void ProcessRubrique(Rubrique rubrique, EudoQuery.CartographySsType type, int nOrder)
        {
            if (rubrique.value == 0 && rubrique.id != 0)
                DeleteMapping(rubrique.id);
            else if (rubrique.value != 0 && rubrique.id == 0)
                AddMapping(type, rubrique.label, rubrique.value, nOrder);
            else if (rubrique.value != 0 && rubrique.id != 0)
                UpdateMapping(rubrique.id, type, rubrique.label, rubrique.value, nOrder);
        }

        private void DeleteMapping(int nMappingId)
        {
            string sError = String.Empty;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(EudoQuery.FILEMAP_TYPE.CARTOGRAPHY.GetHashCode());

            mapping.DeleteMapping(_edal, nMappingId);

            eFilemapPartner existingMapping = _listExistingMappings.FirstOrDefault(mp => mp.Id == nMappingId);
            if (existingMapping != null)
                _listExistingMappings.Remove(existingMapping);
        }

        private void AddMapping(EudoQuery.CartographySsType type, bool bDisplayLabel, int nDescid, int nOrder)
        {
            UpdateMapping(0, type, bDisplayLabel, nDescid, nOrder);
        }

        private void UpdateMapping(int nMappingId, EudoQuery.CartographySsType type, bool bDisplayLabel, int nDescid, int nOrder)
        {
            string sError = String.Empty;

            string sLabel = String.Empty;
            if (nDescid != 0)
            {
                int tab = nDescid - (nDescid % 100);

                string resTab = eLibTools.GetResSingle(_pref, tab, _pref.Lang, out sError);

                if (tab != nDescid)
                {
                    string resField = eLibTools.GetResSingle(_pref, nDescid, _pref.Lang, out sError);
                    sLabel = String.Concat(resTab, ".", resField);
                }
                else
                {
                    sLabel = resTab;
                }
            }

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(
                EudoQuery.FILEMAP_TYPE.CARTOGRAPHY.GetHashCode()
                , sourceDescid: _nTab
                , ssType: type.GetHashCode()
                , source: sLabel
                , sourceType: bDisplayLabel ? EudoQuery.CartographySourceType.LABEL_DISPLAY.GetHashCode() : EudoQuery.CartographySourceType.LABEL_HIDE.GetHashCode()
                , descid: nDescid
                , order: nOrder
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