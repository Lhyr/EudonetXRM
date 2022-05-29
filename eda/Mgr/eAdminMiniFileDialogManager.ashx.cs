using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminMiniFileDialogManager
    /// </summary>
    public class eAdminMiniFileDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminMiniFileUpdate
        {
            [DataMember]
            public Rubrique title;
            [DataMember]
            public Rubrique image;

            [DataMember]
            public Rubrique rub1;
            [DataMember]
            public Rubrique rub2;
            [DataMember]
            public Rubrique rub3;
            [DataMember]
            public Rubrique rub4;
            [DataMember]
            public Rubrique rub5;
            [DataMember]
            public Rubrique rub6;

            [DataMember]
            public RubriqueParent pmrub;
            [DataMember]
            public RubriqueParent pprub;
            [DataMember]
            public RubriqueParent prtrub;
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
        private class RubriqueParent
        {
            [DataMember]
            public bool value;
            [DataMember]
            public int id;
            [DataMember]
            public int sepid;
            [DataMember]
            public bool label;

            [DataMember]
            public Rubrique rub1;
            [DataMember]
            public Rubrique rub2;
            [DataMember]
            public Rubrique rub3;
            [DataMember]
            public Rubrique rub4;
        }

        [DataContract]
        private class eAdminMiniFileDialogResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        private int _nTab;
        private eAdminMiniFileUpdate _mappings;
        private eudoDAL _edal;
        private MiniFileType _minifileType;
        private FILEMAP_TYPE _filemapType = FILEMAP_TYPE.MINI_FILE;

        private List<eFilemapPartner> _listExistingMappings = new List<eFilemapPartner>();

        private eAdminTableInfos _tabInfos;

        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string mappingsJson = _requestTools.GetRequestFormKeyS("mappingsJson") ?? String.Empty;
            int type = _requestTools.GetRequestFormKeyI("type") ?? 0;

            if (Enum.IsDefined(typeof(MiniFileType), type))
                _minifileType = (MiniFileType)type;

            try
            {
                _mappings = JsonConvert.DeserializeObject<eAdminMiniFileUpdate>(mappingsJson);
            }
            catch (JsonReaderException ex)
            {
                error = String.Concat("Erreur lors de la désérialisation de l'objet : ", ex.Message);
            }

            if (_nTab == 0 || _mappings == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des mappings de la minifiche : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    if (_minifileType == MiniFileType.Kanban)
                        _filemapType = FILEMAP_TYPE.KANBAN_MINIFILE;

                    _tabInfos = new eAdminTableInfos(_pref, _nTab);

                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();

                    #region Suppression
                    if (_minifileType == MiniFileType.File)
                    {
                        _listExistingMappings = eFilemapPartner.LoadFileMapPartner(_pref, _nTab, _filemapType);
                        if (error.Length != 0)
                            throw new Exception(error);

                        //Suppression des anciennes fiches mappings invalides
                        foreach (eFilemapPartner existingMapping in new List<eFilemapPartner>(_listExistingMappings))
                        {
                            DeleteMapping(existingMapping.Id);
                        }
                    }
                    else if (_minifileType == MiniFileType.Kanban)
                    {
                        eFilemapPartner.DeleteMappingsByType(_edal, _filemapType, out error);
                        if (error.Length != 0)
                            throw new Exception(error);
                    }
                    #endregion

                    int order = 0;
                    //image
                    ProcessRubrique(_mappings.image, FILEMAP_MINIFILE_TYPE.IMAGE, order);

                    //titre
                    ProcessRubrique(_mappings.title, FILEMAP_MINIFILE_TYPE.FIELD_TITLE, ++order);

                    //rubriques
                    ProcessRubrique(_mappings.rub1, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                    ProcessRubrique(_mappings.rub2, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                    ProcessRubrique(_mappings.rub3, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                    ProcessRubrique(_mappings.rub4, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                    ProcessRubrique(_mappings.rub5, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                    ProcessRubrique(_mappings.rub6, FILEMAP_MINIFILE_TYPE.FIELD, ++order);

                    //rubriques parents
                    order = 20;
                    ProcessRubriqueParent(EudoQuery.TableType.PM.GetHashCode(), _mappings.pmrub, order);
                    order = 30;
                    ProcessRubriqueParent(EudoQuery.TableType.PP.GetHashCode(), _mappings.pprub, order);
                    order = 40;
                    ProcessRubriqueParent(_tabInfos.InterEVTDescid, _mappings.prtrub, order);


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
            eAdminMiniFileDialogResult result = new eAdminMiniFileDialogResult()
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

        private void ProcessRubrique(Rubrique rubrique, EudoQuery.FILEMAP_MINIFILE_TYPE type, int nOrder)
        {
            if (rubrique.value == 0 && rubrique.id != 0)
                DeleteMapping(rubrique.id);
            else if (rubrique.value != 0 && rubrique.id == 0)
                AddMapping(type, rubrique.label, rubrique.value, nOrder);
            else if (rubrique.value != 0 && rubrique.id != 0)
                UpdateMapping(rubrique.id, type, rubrique.label, rubrique.value, nOrder);
        }

        private void ProcessRubriqueParent(int tabParent, RubriqueParent rubriqueParent, int order)
        {
            //l'affichage de la rubrique parent est activé
            if (rubriqueParent.value)
            {
                //champ separateur
                if (rubriqueParent.sepid == 0)
                    AddMapping(FILEMAP_MINIFILE_TYPE.SEPARATOR, false, tabParent, order);
                else
                    UpdateMapping(rubriqueParent.sepid, FILEMAP_MINIFILE_TYPE.SEPARATOR, false, tabParent, order);

                //champ titre
                if (rubriqueParent.id == 0)
                    AddMapping(FILEMAP_MINIFILE_TYPE.FIELD_TITLE, rubriqueParent.label, tabParent + 1, ++order);
                else
                    UpdateMapping(rubriqueParent.id, FILEMAP_MINIFILE_TYPE.FIELD_TITLE, rubriqueParent.label, tabParent + 1, ++order);

                //Rubriques
                ProcessRubrique(rubriqueParent.rub1, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                ProcessRubrique(rubriqueParent.rub2, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                ProcessRubrique(rubriqueParent.rub3, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
                ProcessRubrique(rubriqueParent.rub4, FILEMAP_MINIFILE_TYPE.FIELD, ++order);
            }
            //la rubrique parent est completement masquée
            else
            {
                if (rubriqueParent.sepid != 0)
                    DeleteMapping(rubriqueParent.sepid);

                if (rubriqueParent.id != 0)
                    DeleteMapping(rubriqueParent.id);

                if (rubriqueParent.rub1.id != 0)
                    DeleteMapping(rubriqueParent.rub1.id);

                if (rubriqueParent.rub2.id != 0)
                    DeleteMapping(rubriqueParent.rub2.id);

                if (rubriqueParent.rub3.id != 0)
                    DeleteMapping(rubriqueParent.rub3.id);

                if (rubriqueParent.rub4.id != 0)
                    DeleteMapping(rubriqueParent.rub4.id);
            }
        }

        private void DeleteMapping(int nMappingId)
        {
            string sError = String.Empty;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(_filemapType.GetHashCode());

            mapping.DeleteMapping(_edal, nMappingId);

            eFilemapPartner existingMapping = _listExistingMappings.FirstOrDefault(mp => mp.Id == nMappingId);
            if (existingMapping != null)
                _listExistingMappings.Remove(existingMapping);
        }

        private void AddMapping(EudoQuery.FILEMAP_MINIFILE_TYPE type, bool bDisplayLabel, int nDescid, int nOrder)
        {
            UpdateMapping(0, type, bDisplayLabel, nDescid, nOrder);
        }

        private void UpdateMapping(int nMappingId, EudoQuery.FILEMAP_MINIFILE_TYPE type, bool bDisplayLabel, int nDescid, int nOrder)
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
                _filemapType.GetHashCode()
                , ssType: _nTab
                , source: sLabel
                , sourceDescid: type.GetHashCode()
                , sourceType: bDisplayLabel ? EudoQuery.FILEMAP_MINIFILE_SOURCETYPE.LABEL_DISPLAY.GetHashCode() : EudoQuery.FILEMAP_MINIFILE_SOURCETYPE.LABEL_HIDE.GetHashCode()
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