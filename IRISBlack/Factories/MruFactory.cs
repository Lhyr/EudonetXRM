using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.DataFields;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Com.Eudonet.Core.Model.eParam;
using static EudoCommonHelper.EudoHelpers;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    public class MruFactory
    {

        #region propriétés

        private ePref Pref { get; set; }
        private UserValuesModel UserModelValue { get; set; } = null;
        private CatalogValuesModel CatModelValue { get; set; } = null;
        private int DescId { get; set; }
        #endregion



        #region construteurs
        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire MruModel
        /// <param name="_pref"/>
        /// <param name="mdl"/>
        /// </summary>
        private MruFactory(ePref _pref, UserValuesModel mdl)
        {
            Pref = _pref;
            UserModelValue = mdl;
        }

        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire MruModel
        /// <param name="_pref"/>
        /// <param name="mdl"/>
        /// </summary>
        private MruFactory(ePref _pref, CatalogValuesModel mdl)
        {
            Pref = _pref;
            CatModelValue = mdl;
        }

        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire MruModel
        /// <param name="_pref"/>
        /// <param name="mdl"/>
        /// </summary>
        private MruFactory(ePref _pref)
        {
            Pref = _pref;
        }
        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire MruModel
        /// <param name="_pref"/>
        /// <param name="nDescid"/>
        /// </summary>
        private MruFactory(ePref _pref, int nDescid)
        {
            Pref = _pref;
            DescId = nDescid;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="_pref" />
        /// </summary>
        /// <returns></returns>
        public static MruFactory InitMruFactory(ePref _pref) =>
            new MruFactory(_pref);

        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="_pref" />
        /// <param name="nDescid" />
        /// </summary>
        /// <returns></returns>
        public static MruFactory InitMruFactoryDescId(ePref _pref, int nDescid) =>
            new MruFactory(_pref, nDescid);
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="_pref"/>
        /// <param name="uvm"/>
        /// </summary>
        /// <returns></returns>
        public static MruFactory InitMruFactoryUser(ePref _pref, UserValuesModel uvm = null) =>
            new MruFactory(_pref, uvm);

        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre les prefs
        /// Retourne une instance de la classe.
        /// <param name="_pref"/>
        /// <param name="cvm"/>
        /// </summary>
        /// <returns></returns>
        public static MruFactory InitMruFactoryCatalog(ePref _pref, CatalogValuesModel cvm = null) =>
            new MruFactory(_pref, cvm);
        #endregion


        #region private 
        /// <summary>
        /// On cherche à savoir si la recherche se fait sur tous les enregistrements.
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="nTargetTab"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="descid"></param>
        /// <param name="bSearchAllUserDefined"></param>
        /// <returns></returns>
        private bool GetIsAllRecord(eudoDAL dal, int nTargetTab, int nTabFrom, int descid, bool bSearchAllUserDefined)
        {
            bool bTabDescid = nTabFrom != GetTabFromDescId(descid);
            int nUservalueTab = bTabDescid ? nTabFrom : nTargetTab;
            int nUservalueField = bTabDescid ? GetTabFromDescId(descid) + 1 : descid;

            try
            {
                eUserValue uvSearchAll = new eUserValue(dal, nUservalueField, TypeUserValue.SEARCH_ALL, Pref.User, nUservalueTab);
                uvSearchAll.Build();

                if (uvSearchAll.Enabled && (!bSearchAllUserDefined || uvSearchAll.Index == 1))
                    return uvSearchAll.Enabled;

                return bSearchAllUserDefined;
            }
            catch (Exception)
            {
                return false;
            }


        }
        /// <summary>
        /// transforme une valeur de type catalog en valeur de mru
        /// </summary>
        /// <param name="catalogValue"></param>
        /// <returns></returns>
        private MRUModel.Value GetMRUItemFromCatalogValue(ICatalogValue catalogValue)
        {
            if (catalogValue == null)
                return null;

            return new MRUModel.Value()
            {
                DbValue = catalogValue?.DbValue,
                DisplayLabel = catalogValue?.DisplayLabel
            };
        }

        /// <summary>
        /// transforme une valeur de type user en valeur de mru
        /// </summary>
        /// <param name="userVal"></param>
        /// <returns></returns>
        private MRUModel.Value GetMRUItemFromUserValue(IUserValue userVal)
        {
            if (userVal == null)
                return null;

            return new MRUModel.Value
            {
                DbValue = userVal?.ItemCode,
                DisplayLabel = userVal?.Label
            };
        }

        /// <summary>
        /// Permet de récupérer les informations de liaisons de la relation.
        /// </summary>
        /// <param name="rec"></param>
        /// <param name="typeTable"></param>
        private IMruValue GetPMPPToRelation(eRecord rec, TableType typeTable)
        {

            if (rec.TablesFileId == null || rec.CalledTab == typeTable.GetHashCode())
                return null;

            try
            {
                bool bIsPPPMADR =
                    (rec.CalledTab == TableType.PM.GetHashCode() && typeTable == TableType.PP) 
                        || (rec.CalledTab == TableType.PP.GetHashCode() && typeTable == TableType.PM);

                string sFldToRetrieve = bIsPPPMADR
                    ? $"{rec.CalledTab}_{TableType.ADR.GetHashCode()}_{typeTable.GetHashCode() + 1}"
                    : $"{rec.CalledTab}_{typeTable.GetHashCode() + 1}";

                eFieldRecord fieldRow = rec.GetFieldByAlias(sFldToRetrieve);

                if (fieldRow == null || fieldRow.FileId < 1 || !fieldRow.RightIsVisible)
                    return null;

                return new MRUModel.Value
                {
                    DbValue = fieldRow.FileId.ToString(),
                    DisplayLabel = fieldRow.DisplayValue,
                };

            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region public
        #region MRU sur les users et catalog

        /// <summary>
        /// transforme une liste de valeurs de users en valeurs de mru
        /// </summary>
        /// <returns></returns>
        public MRUModel GetMRUFromModel()
        {
            IEnumerable<MRUModel.Value> mruMdVal = null;

            if (CatModelValue != null)
                mruMdVal = CatModelValue?.Values?.Select(value => GetMRUItemFromCatalogValue(value)).Distinct();

            if (UserModelValue != null)
                mruMdVal = UserModelValue?.Values?.Select(value => GetMRUItemFromUserValue(value)).Distinct();

            return new MRUModel()
            {
                Values = mruMdVal?.ToList()
            };
        }

        /// <summary>
        /// Crée un modèle pour les MRU depuis un descid
        /// </summary>
        /// <returns></returns>
        public MRUModel GetBaseMRUModel()
        {
            eParam param = new eParam(Pref);
            string sError;
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            try
            {
                dal.OpenDatabase();
                if (DescId % 100 > 0)
                {
                    #region mru des champs
                    if (!param.LoadFieldMru(dal, DescId.ToString(), out sError))
                    {
                        throw new EudoException(sError);
                    }

                    ParamMruFieldItem paramMruFieldItem = null;

                    if (param.ParamMruField.TryGetValue(DescId, out paramMruFieldItem) && paramMruFieldItem != null)
                    {
                        // #85 754 - Pour les champs Caractère, il faut identifier le type obsolète "catalogue v7", et envoyer en base le libellé en tant que valeur, et non l'ID
                        // On teste donc le PopupType pour le déterminer, en prenant soin de ne pas confondre avec un champ de type Relation, qui a également PopupType = ONLY (2)
                        // cf. code d'identification dans eAdminTools.GetCharTypeLabel
                        bool bIsV7Catalog = false;

                        // Récupération de FieldInfo
                        eFieldLiteWithLib fieldInfo = eLibTools.GetFieldInfo<eFieldLiteWithLib>(Pref, DescId, eFieldLiteWithLib.Factory(Pref));
                        if (fieldInfo.Popup == PopupType.ONLY || fieldInfo.Popup == PopupType.FREE)
                        {
                            bIsV7Catalog = !(fieldInfo.PopupDescId % 100 == 1 && fieldInfo.Popup == PopupType.ONLY && fieldInfo.PopupDescId != DescId);
                        }

                        return new MRUModel()
                        {
                            Values = paramMruFieldItem.Values.Split("$|$")
                            .Select(item => new { sValue = item.Split(";"), item })
                            .Where(oValue => oValue.sValue[0].Length > 1)
                            .Select(oValue =>
                            {
                                string sLabel = oValue.item.Substring(oValue.sValue[0].Length + 1);
                                return new MRUModel.Value { DbValue = bIsV7Catalog ? sLabel : oValue.sValue[0], DisplayLabel = sLabel };
                            }).Distinct().ToList()
                        };

                    }
                    #endregion
                }
                else
                {
                    #region MRU des tables
                    string sValues = param.GetMruTabValue(dal, DescId, out sError);

                    if (sError.Length > 0)
                        throw new EudoException(sError);

                    return new MRUModel()
                    {
                        Values = sValues.Split("$|$")
                        .Select(item => item.Split("$;$"))
                        .Where(sValue => sValue.Length > 1)
                        .Select(sValue => new MRUModel.Value
                        {
                            DbValue = sValue[0],
                            DisplayLabel = sValue[1]
                        })
                        .Distinct()
                        .ToList()
                    };

                    #endregion

                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                dal.CloseDatabase();
            }
        }
        #endregion


        #region GetMRUFromFile
        /// <summary>
        /// transforme une liste de valeurs d'enregistrements en valeurs de mru
        /// </summary>
        /// <param name="nTargetTab"></param>
        /// <param name="search"></param>
        /// <param name="nTabFrom"></param>
        /// <param name="nFileId"></param>
        /// <param name="lstDisplayedUserValueField"></param>
        /// <param name="lstDispValues">IDs à retourner pour le cas des MRU lorsque le champ est affiché sans recherche (ignorés si une recherche est effectuée)</param>
        /// <param name="bSearchAllUserDefined"></param>
        /// <returns></returns>
        public MRUModel GetMRUFromFile(int nTargetTab, string search, IEnumerable<UserValueField> lstDisplayedUserValueField, IEnumerable<int> lstDispValues, IEnumerable<int> lstCols , bool bSearchAllUserDefined, int nTabFrom = 0, int nFileId = 0)
        {
            MRUModel mRUModel = new MRUModel();
            List<int> lstColSpec = new List<int>();
            OrmMappingInfo ormInfo = eLibTools.OrmLoadAndGetMapWeb(Pref);

            int nbResults = 0;

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            string error;

            if (string.IsNullOrEmpty(search))
                nbResults = 8;

            dal.OpenDatabase();
            try
            {
                dal.OpenDatabase();
                TableLite targetTab = null;
                
                List<int> _listCol = new List<int>();
                if (lstCols != null)
                    _listCol.AddRange(lstCols);

                bool bAllRecord = GetIsAllRecord(dal, nTargetTab, nTabFrom, DescId, bSearchAllUserDefined);

                try
                {
                    targetTab = new TableLite(nTargetTab);
                    targetTab.ExternalLoadInfo(dal, out error);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    dal.CloseDatabase();
                }

                if (nTargetTab == 200) //Dans les MRU le contact doit affiché Particule NOM Prénom
                {
                    _listCol.AddRange(new int[] { 201, 202, 203, 301, 401, 412 });
                }
                else if (targetTab != null && (targetTab.TabType == TableType.EVENT || targetTab.TabType == TableType.ADR))
                {
                    //KHA dans les mrus on doit pouvoir rapatrier les ppid pmid liés
                    if (targetTab.InterPP)
                        _listCol.Add(TableType.PP.GetHashCode() + 1);

                    if (targetTab.InterPM)
                        _listCol.Add(TableType.PM.GetHashCode() + 1);
                }

                eFinderList list = eFinderList.CreateFinderList(
                        Pref,
                        nTargetTab: nTargetTab,
                        nTabFrom: nTabFrom,
                        nFileId: nFileId,    //todo
                        nDescId: DescId,
                        sSearch: search,
                        bHisto: true,
                        currentSearchMode: eFinderList.SearchMode.EXTENDED,
                        bPhoneticSearch: false,
                        bAllRecord: bAllRecord,
                        nDispValue: lstDispValues.ToList(),
                        listDisplayedValueField: lstDisplayedUserValueField?.ToList() ?? new List<UserValueField>(),
                        listCol: _listCol,
                        listColSpec: lstColSpec,
                        fileMode: eFinderList.Mode.MRU,
                        nMaxRowsReturns: nbResults);

                if (list.InnerException != null)
                    throw list.InnerException;

                if (!string.IsNullOrEmpty(list.ErrorMsg))
                    throw (new Exception(list.ErrorMsg));

                IEnumerable<MRUModel.Value> lstMruValues = list.ListRecords.Select(rec => GetMRUItemFromFileValue(rec, ormInfo)).Distinct();

                string id = "";
                int idx = 0;
                //List<MRUModel.Value> lstMru = new List<MRUModel.Value>();

                //foreach(MRUModel.Value value in lstMruValues)
                //{
                //    if(id != value.DbValue)
                //    {
                //        lstMru.Add(value);
                //        id = value.DbValue;
                //        idx = lstMru.Count - 1;
                //    }
                //    else
                //    {
                //        if (value.PP != null && lstMru[idx].PP == null)
                //            lstMru[idx].PP = value.PP;
                //        else if (value.PP != null && lstMru[idx].PP != value.PP)
                //            lstMru[idx].PP = null;

                //        if (value.PM != null && lstMru[idx].PM == null)
                //            lstMru[idx].PM = value.PM;
                //        else if (value.PM != null && lstMru[idx].PM != value.PM)
                //            lstMru[idx].PM = null;
                //    }

                //}

                //mRUModel.Values.AddRange(lstMru);
                
                mRUModel.Values.AddRange(lstMruValues);

                return mRUModel;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                dal.CloseDatabase();
            }
        }
        #endregion

        #region Mru sur les tables (utilisé pour les relations)
        /// <summary>
        /// transforme un enregistrement en valeur de mru
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        public MRUModel.Value GetMRUItemFromFileValue(eRecord rec, OrmMappingInfo ormInfo)
        {
            var ieFields = rec.GetFields.Select(f=> f.FldInfo)
                .Distinct(new FieldComparer()).Where(f => f.PermViewAll);

            var ieDescId = ieFields
                .Select(fld => fld.Descid)
                .Union(ieFields.Where(fld => fld.AliasSourceField != null).Select(fld => fld.AliasSourceField.Descid))
                .Union(ieFields.Select(fld => eLibTools.GetTabFromDescId(fld.Descid)));

            var oFldAdv = FieldsAdvancedCplFactory.InitFieldsAdvancedCplFactory(Pref, ieDescId);
            var oRes = oFldAdv.GetResInternal();
            var oDescAdv = oFldAdv.GetDescAdvDataSet();
            var oRgpd = RGPDFactory.initRGPDFactory(Pref).GetRGPDData(ieDescId);

            MRUModel.Value mruValue = new MRUModel.Value()
            {
                DbValue = rec.MainFileid.ToString(),
                DisplayLabel = rec.MainFileLabel,
                PP = GetPMPPToRelation(rec, TableType.PP),
                PM = GetPMPPToRelation(rec, TableType.PM),
                ListDeduplicatingFields = rec.GetFields
                    .Where(f => f.FldInfo.PermViewAll)
                    .Select(f => new  { 
                        Type= FldTypedInfosFactory.InitFldTypedInfosFactory(f.FldInfo).GetFieldType(),
                        Struct = FldTypedInfosFactory.InitFldTypedInfosFactory(f.FldInfo, Pref).GetFldTypedInfos(oRes, oDescAdv, oRgpd),
                        Data = DataFieldModelFactory.initDataFieldModelFactory(f, rec, ormInfo).GetDataField(Pref)
                    })
                    .Select(f => {
                        var data = f?.Data;
                        var oStruct = f?.Struct;

                        if (data == null)
                            return null;

                        return new MRUValuesDeduplicatingModel
                        {
                            DescId = data.DescId,
                            Label = oStruct.Label,
                            Value = data.Value,
                            DisplayValue = data.DisplayValue,
                            Type = f.Type
                        };
                      }),
            };

            return mruValue;
        }
        #endregion
        #endregion
    }
}