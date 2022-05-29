using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminAdvancedCatalogDialogManager
    /// </summary>
    public class eAdminAdvancedCatalogDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        private class eAdminAdvancedCatalogUpdate
        {
            [DataMember]
            public bool DataEnabled;
            [DataMember]
            public string DisplayMask;
            [DataMember]
            public string SortBy;

            [DataMember]
            public bool Multiple;
            [DataMember]
            public bool TreeView;
            [DataMember]
            public string PopupDescId;
            [DataMember]
            public string BoundDescId;
            [DataMember]
            public bool NoAutoLoad;
            [DataMember]
            public string SearchLimit;
            [DataMember]
            public bool TreeViewOnlyLastChildren;
            [DataMember]
            public bool DataAutoEnabled;
            [DataMember]
            public string DataAutoStart;
            [DataMember]
            public string DataAutoFormula;
            [DataMember]
            public bool StepMode;
            [DataMember]
            public bool SequenceMode;
            [DataMember]
            public String SelectedValueColor;
        }

        [DataContract]
        private class eAdminAdvancedCatalogManagerResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        #region Propriétés
        private int _nTab;
        private int _nField;

        private eAdminAdvancedCatalogUpdate _properties;

        private eudoDAL _edal;

        private eAdminTableInfos _tabInfos;
        private eAdminFieldInfos _fieldInfos;
        #endregion

        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _nField = _requestTools.GetRequestFormKeyI("field") ?? 0;

            string propertiesJson = _requestTools.GetRequestFormKeyS("propertiesJson") ?? String.Empty;

            try
            {
                _properties = JsonConvert.DeserializeObject<eAdminAdvancedCatalogUpdate>(propertiesJson);
            }
            catch (JsonReaderException ex)
            {
                //
                _properties = null;
            }

            if (_nTab == 0 || _nField == 0 || _properties == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des options de catalogue : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    _tabInfos = new eAdminTableInfos(_pref, _nTab);


                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();


                    _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(_edal, _pref, _nField);

                    eAdminFiledataParam fileDataparam = eAdminFiledataParam.Load(_edal, _fieldInfos.DescId); //new eAdminFiledataParam(_fieldInfos.DescId);
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.DataEnabled, _properties.DataEnabled ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.DisplayMask, _properties.DisplayMask);
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.SortBy, _properties.SortBy);
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.TreeView, _properties.TreeView ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.NoAutoLoad, _properties.NoAutoLoad ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.SearchLimit, _properties.SearchLimit);
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.TreeViewOnlyLastChildren, _properties.TreeViewOnlyLastChildren ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.DataAutoEnabled, _properties.DataAutoEnabled ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.DataAutoStart, _properties.DataAutoStart);
                    if (_pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                        fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.DataAutoFormula, _properties.DataAutoFormula);
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.StepMode, _properties.StepMode ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.SequenceMode, _properties.SequenceMode ? "1" : "0");
                    fileDataparam.SetDesc(eLibConst.FILEDATAPARAM.SelectedValueColor, _properties.SelectedValueColor);

                    eAdminResult saveResult = fileDataparam.Save(_pref);
                    if (!saveResult.Success)
                        throw new Exception(saveResult.UserErrorMessage);


                    eAdminDesc desc = new eAdminDesc(_fieldInfos.DescId);
                    desc.SetDesc(eLibConst.DESC.MULTIPLE, _properties.Multiple ? "1" : "0");
                    desc.SetDesc(eLibConst.DESC.POPUPDESCID, _properties.PopupDescId);
                    desc.SetDesc(eLibConst.DESC.BOUNDDESCID, _properties.BoundDescId);

                    saveResult = desc.Save(_pref, out error);
                    if (!saveResult.Success)
                        throw new Exception(saveResult.UserErrorMessage);

                    success = true;
                }
                catch (Exception ex)
                {
                    error = String.Concat("Erreur lors de l'enregistrement des options du catalogue : ", ex.Message);
                }
                finally
                {
                    _edal.CloseDatabase();
                }
            }

            #region Résultat
            eAdminAdvancedCatalogManagerResult result = new eAdminAdvancedCatalogManagerResult()
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