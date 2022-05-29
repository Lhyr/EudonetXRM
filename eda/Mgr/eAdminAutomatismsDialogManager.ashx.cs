using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Description résumée de eAdminAutomatismsDialogManager
    /// </summary>
    public class eAdminAutomatismsDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        private class eAdminAutomatismsUpdate
        {
            [DataMember]
            public string TopFormula;
            [DataMember]
            public string MiddleFormula;
            [DataMember]
            public string BottomFormula;
        }

        [DataContract]
        private class eAdminAutomatismsDialogManagerResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion

        private int _nTab;
        private int _nField;
        private eAdminAutomatismsUpdate _formulas;

        private eudoDAL _edal;

        private eAdminTableInfos _tabInfos;
        private eAdminFieldInfos _fieldInfos;

        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _nField = _requestTools.GetRequestFormKeyI("field") ?? 0;

            string formulasJson = _requestTools.GetRequestFormKeyS("formulasJson") ?? String.Empty;

            try
            {
                _formulas = JsonConvert.DeserializeObject<eAdminAutomatismsUpdate>(formulasJson);
            }
            catch (JsonReaderException ex)
            {
                //
            }

            if (_nTab == 0 || _nField == 0 || _formulas == null)
            {
                error = String.Concat("Erreur lors de l'enregistrement des automatismes avancés : ", "Propriétés invalides");
            }
            else
            {
                try
                {
                    _tabInfos = new eAdminTableInfos(_pref, _nTab);
                    _fieldInfos = eAdminFieldInfos.GetAdminFieldInfos(_pref, _nField);

                    eAdminDesc adminDesc = new eAdminDesc(_fieldInfos.DescId);

                    // mise à jour formule du haut
                    if (!String.IsNullOrEmpty(_formulas.TopFormula))
                    {
                        adminDesc.SetDesc(eLibConst.DESC.DEFAULT, _formulas.TopFormula);
                        adminDesc.SetDesc(eLibConst.DESC.DEFAULTFORMAT, "1");
                    }
                    else
                    {
                        if(!String.IsNullOrEmpty(_fieldInfos.Default) && _fieldInfos.DefaultFormat == true)
                        {
                            adminDesc.SetDesc(eLibConst.DESC.DEFAULT, _formulas.TopFormula);
                            adminDesc.SetDesc(eLibConst.DESC.DEFAULTFORMAT, "0");
                        }
                    }

                    // mise à jour formule du bas
                    adminDesc.SetDesc(eLibConst.DESC.FORMULA, _formulas.BottomFormula);

                    adminDesc.Save(_pref, out error);
                    if (error.Length > 0)
                        throw new Exception(error);

                    // mise à jour formule du milieu
                    _edal = eLibTools.GetEudoDAL(_pref);
                    _edal.OpenDatabase();

                    eUserValue userValue = new eUserValue(_edal, _fieldInfos.DescId, TypeUserValue.FORMULA, _pref.User, _tabInfos.DescId);
                    userValue.Value = _formulas.MiddleFormula;
                    userValue.Enabled = true;
                    if (!userValue.Save(_edal, true))
                        throw new Exception(userValue.Error);

                    success = true;
                }
                catch (Exception ex)
                {
                    error = String.Concat("Erreur lors de l'enregistrement des automatismes avancés : ", ex.Message);
                }
                finally
                {
                    _edal.CloseDatabase();
                }
            }


            #region Résultat
            eAdminAutomatismsDialogManagerResult result = new eAdminAutomatismsDialogManagerResult()
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