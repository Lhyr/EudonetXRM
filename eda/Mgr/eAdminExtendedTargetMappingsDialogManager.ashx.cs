using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.eda.Classes;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExtendedTargetMappingsDialogManager
    /// </summary>
    public class eAdminExtendedTargetMappingsDialogManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminExtendedTargetMappingsUpdate
        {
            [DataMember]
            public List<ExtendedTargetMapping> mappings;
        }

        private class ExtendedTargetMapping
        {
            [DataMember]
            public int descid;
            [DataMember]
            public int value;
        }

        #endregion

        private int _nTab;
        private eAdminExtendedTargetMappingsUpdate _mappings;



        private eAdminResult HandleTreatment()
        {
            eAdminResult result = new eAdminResult();
            string error = string.Empty;
            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string mappingsJson = _requestTools.GetRequestFormKeyS("mappingsJson") ?? String.Empty;
            string action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;
            Int32 descIdField = _requestTools.GetRequestFormKeyI("descidfield") ?? 0;

            eAdminFieldInfos adminOldField = null;

            if (descIdField == 0)
            {
                result.Success = false;
                result.UserErrorMessage = eResApp.GetRes(_pref, 6524);
            }
            else
            {
                #region Vérification si la rubrique est utilisée
                if (!string.IsNullOrEmpty(action) && action.ToLower() == "updatefield")
                {
                    adminOldField = eAdminFieldInfos.GetAdminFieldInfos(_pref, descIdField);
                    if (adminOldField != null && adminOldField.Error.Length > 0)
                    {
                        result.UserErrorMessage = adminOldField.Error;
                        result.Success = false;
                    }
                    else
                    {

                        try
                        {
                            _mappings = JsonConvert.DeserializeObject<eAdminExtendedTargetMappingsUpdate>(mappingsJson);
                        }
                        catch (JsonReaderException ex)
                        {
                            result.UserErrorMessage = eResApp.GetRes(_pref, 1716);
                            result.DebugErrorMessage = ex.Message;
                            result.InnerException = ex;
                            result.Success = false;

                        }

                        result = eAdminMappingExtendedField.CheckUsedField(_pref, descIdField);

                        //

                    }

                }
                #endregion

                #region Mise à jour du mapping
                else
                {
                    try
                    {
                        _mappings = JsonConvert.DeserializeObject<eAdminExtendedTargetMappingsUpdate>(mappingsJson);
                    }
                    catch (JsonReaderException ex)
                    {
                        result.UserErrorMessage = eResApp.GetRes(_pref, 1716);
                        result.DebugErrorMessage = ex.Message;
                        result.InnerException = ex;
                        result.Success = false;
                    }

                    if (_nTab == 0 || _mappings == null)
                    {
                        result.UserErrorMessage = String.Concat(eResApp.GetRes(_pref, 8667), eResApp.GetRes(_pref, 8018));
                        result.Success = false;
                    }
                    else
                    {
                        try
                        {
                            if (_mappings.mappings.Count > 0)
                            {
                                List<string> listResId = new List<string>();
                                foreach (ExtendedTargetMapping mapping in _mappings.mappings)
                                {
                                    if (!listResId.Contains(mapping.value.ToString()))
                                        listResId.Add(mapping.value.ToString());
                                    int resIdTab = mapping.value - (mapping.value % 100);
                                    if (!listResId.Contains(resIdTab.ToString()))
                                        listResId.Add(resIdTab.ToString());
                                }



                                eRes res = new eRes(_pref, String.Join(",", listResId));

                                // Récupération du mapping précédant
                                Dictionary<int, int> alreadyMapped = eSqlDesc.LoadExtendedTargetMappings(_pref, _nTab);



                                IEnumerable<int> lstMappedDescid = _mappings.mappings.Select(map => map.descid);




                                foreach (var kvpi in alreadyMapped)
                                {
                                    int target = kvpi.Value;

                                    //si le champ n'est plus mappé, on le retire
                                    if (!lstMappedDescid.Contains(kvpi.Key))
                                    {
                                        int descid = kvpi.Key;

                                        eAdminDesc edaDesc = new eAdminDesc(descid);
                                        edaDesc.SetDesc(eLibConst.DESC.PROSPECTENABLED, "1");
                                        edaDesc.Save(_pref, out error);
                                        if (error.Length > 0)
                                            throw new Exception(error);

                                        result = new eAdminResult() { Success = true };
                                    }
                                }




                                foreach (ExtendedTargetMapping mapping in _mappings.mappings)
                                {
                                    // Si le mapping est déjà fait, on passe au suivant
                                    // BSE : #59784 
                                    if (alreadyMapped.ContainsKey(mapping.descid) && alreadyMapped[mapping.descid] == mapping.value)
                                        continue;

                                    int resIdTab = mapping.value - (mapping.value % 100);
                                    bool bTabResFound = false;
                                    string tabName = res.GetRes(resIdTab, out bTabResFound);
                                    bool bFieldFound = false;
                                    string fieldName = res.GetRes(mapping.value, out bFieldFound);
                                    string tooltiptext = eResApp.GetRes(_pref, 1661).Replace("<PHISICALNAME>", String.Concat(tabName, ".", fieldName));

                                    eAdminDesc edaDesc = new eAdminDesc(mapping.descid);
                                    edaDesc.SetDesc(eLibConst.DESC.TOOLTIPTEXT, tooltiptext);
                                    edaDesc.SetDesc(eLibConst.DESC.STORAGE, ((int)ImageStorage.STORE_IN_DATABASE).ToString());
                                    edaDesc.SetDesc(eLibConst.DESC.PROSPECTENABLED, mapping.value.ToString());
                                    edaDesc.SetDesc(eLibConst.DESC.FULLUSERLIST, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.DEFAULT, "");
                                    edaDesc.SetDesc(eLibConst.DESC.DEFAULTFORMAT, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.READONLY, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.HTML, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.LABELALIGN, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.RELATION, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.NODEFAULTCLONE, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.SIZELIMIT, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.SCROLLING, "0");
                                    edaDesc.SetDesc(eLibConst.DESC.COMPUTEDFIELDENABLED, "0");
                                    //Sauvegarder le mapping
                                    edaDesc.Save(_pref, out error);
                                    if (error.Length > 0)
                                        throw new Exception(error);


                                    result = eAdminMappingExtendedField.UpdateFieldContent(_pref, descIdField);
                                }


                            }

                        }
                        catch (Exception ex)
                        {
                            result.UserErrorMessage = eResApp.GetRes(_pref, 6172);
                            result.Success = false;
                            result.DebugErrorMessage = ex.Message;
                        }
                    }

                }
                #endregion

            }

            return result;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            eAdminResult result = HandleTreatment();



            #region Résultat
            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });
            #endregion
        }

        #region Code executé par la v7
        //Code executé par la v7 pour descid = 4718
        //DELETE FROM [FileDataParam] WHERE [DescId] = 4718
        /*
        UPDATE [Desc] SET 
        [computedfieldenabled]=N'0',
        [fulluserlist]=N'0',
        [changerulesid]=NULL,
        [bounddescid]=NULL,
        [readonly]=N'0',
        [viewrulesid]=NULL,
        [tooltiptext]=N'Rubrique associée à Sociétés.Fax',
        [italic]=N'0',
        [relation]=N'0',
        [viewpermid]=675,
        [nodefaultclone]=N'0',
        [obligatrulesid]=NULL,
        [unicode]=N'0',
        [rowspan]=1,
        [case]=0,
        [formula]=NULL,
        [bold]=N'0',
        [underline]=N'0',
        [popup]=0,[html]=N'0',
        [sizelimit]=N'0',
        [scrolling]=N'0',
        [prospectenabled]=N'306',
        [flat]=N'0',
        [forecolor]=NULL,
        [popupdescid]=NULL,
        [obligat]=N'0',
        [updatepermid]=NULL,
        [format]=1,
        [mask]=NULL,
        [default]=N'',
        [defaultformat]=0,
        [labelalign]=N'0',
        [multiple]=0,
        [disporder]=N'13',
        [storage]=N'1',
        [colspan]=N'1',
        [treeviewuserlist]=N'0',
        [length]=100,
        [parameters]=NULL 
        WHERE DescId = 4718
        */
        //DELETE FROM [SyncField] WHERE [DescId] = 4718;DELETE FROM [SyncLastUpdate] WHERE [DescId] = 4718;DELETE FROM [SyncPriority] WHERE [DescId] = 4718
        //UPDATE [Res] SET LANG_00 = N'test fax' WHERE ResId = 4718
        //DELETE FROM [UserValue] WHERE ISNULL( [Type], 0 ) = 6 AND [Tab] = 4700 AND [DescId] = 4718
        //INSERT INTO [UserValue] ([Type],[Enabled],[Tab],[DescId],[Value]) VALUES(6,1,4700,4718,'')
        //DELETE FROM [UserValue] WHERE ISNULL( [Type], 0 ) = 18 AND [DescId] = 4718
        //UPDATE [Catalog] SET ParentId = NULL WHERE ParentId IN (SELECT CatId FROM [Catalog] WHERE DescId = 4718)
        //DELETE FROM [Catalog] WHERE DescId = 4718
        //DELETE FROM [MRU] WHERE Userid > 0 and DescId = 4718
        /*
        INSERT INTO [FileDataParam] ([DescId],[DataEnabled],[SortEnabled],[DisplayMask],[SortBy],[LangUsed],[AddPermission],[UpdatePermission],[DeletePermission],[SynchroPermission],[Treeview],[DataAutoEnabled],[DataAutoStart],[DataAutoFormula],[NoAutoLoad], [SearchLimit], [TreeViewOnlyLastChildren]) 
        VALUES (4718,0,0,'[TEXT]','[TEXT]','0',NULL,NULL,NULL,NULL,0,0,0,NULL,'0',0,'0')
        */
        #endregion
    }
}