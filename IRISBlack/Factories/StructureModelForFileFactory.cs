using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static Com.Eudonet.Xrm.IRISBlack.Model.FileDetailModel;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// CLasse factory qui permet d'initialiser StructureModel.
    /// </summary>
    public class StructureModelForFileFactory
    {

        #region propriétés
        eFile objFile { get; set; }
        #endregion

        #region constructeur
        /// <summary>
        /// Constructeur de la classe StructureModelForFileFactory
        /// </summary>
        /// <param name="file"></param>
        private StructureModelForFileFactory(eFile file)
        {
            objFile = file;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe StructureModelForFileFactory.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static StructureModelForFileFactory InitStructureModelForFileFactory(eFile file)
        {
            return new StructureModelForFileFactory(file);
        }
        #endregion

        #region Private

        /// <summary>
        /// Permet de vérifier et de corriger la position des éléments.
        /// </summary>
        /// <param name="lstRec"></param>
        /// <returns></returns>
        private async Task CheckFieldsPosNull(List<Field> lstRec)
        {
            List<Tuple<int, EudoException>> lstErr = new List<Tuple<int, EudoException>>();
            bool bSuccess = true;
            int ViewMainTab = objFile.ViewMainTable.DescId;
            var lstRecMainTbl = lstRec
                .Where(rec => rec.PosX == 0 && rec.PosY == 0
                    && (rec.Descid % 100 < eLibConst.MAX_NBRE_FIELD)
                    && (EudoCommonHelper.EudoHelpers.GetTabFromDescId(rec.Descid) == ViewMainTab));

            int nRecToPosO = lstRecMainTbl.Count();

            if (nRecToPosO > 1)
            {
                bSuccess = false;
                using (eudoDAL dal = eLibTools.GetEudoDAL(objFile.Pref))
                {
                    dal.OpenDatabase();
                    try
                    {
                        lstErr = await Task.Run(() => eFileLayout.UpdateTabsPositions(objFile.Pref, dal, new List<int>() { ViewMainTab }));
                        bSuccess = true;
                    }
                    catch (Exception)
                    {
                        bSuccess = false;
                    }
                }

                if (lstErr.Count > 0 || !bSuccess)
                {
                    int nCol = (objFile.ViewMainTable.ColByLine > 0 ? objFile.ViewMainTable.ColByLine : 1);

                    Parallel.ForEach(lstRecMainTbl, rec =>
                    {
                        rec.PosX = (rec.PosDisporder - 1) % nCol;
                        rec.PosY = (rec.PosDisporder - 1) / nCol;
                    });


                    StringBuilder sbError = new StringBuilder();

                    foreach (var tpl in lstErr)
                    {
                        sbError.AppendLine(tpl.Item2.Message);
                    }

                    throw new EudoException(sbError.ToString());
                }
            }
        }
        #endregion

        #region Public
        /// <summary>
        /// Retourne le structureModel, classe imbriquée de FileDetailModel
        /// Initialisé à l'aide des obbjets passés en constructeur de StructureModelForFileFactory.
        /// </summary>
        /// <returns></returns>
        public async Task<StructureModel> GetStructureModelForFile(IDictionary<int, RGPDModel> rgpdData)
        {
            StructureModel sm = new StructureModel();

            await CheckFieldsPosNull(objFile.FldFieldsInfos);

            var EdnType = objFile.ViewMainTable.EdnType;

            if (objFile.ViewMainTable.DescId == (int)TableType.ADR)
                EdnType = EdnType.FILE_ADR;

            sm.StructFile = new StructFileModel()
            {
                DescId = objFile.ViewMainTable.DescId,
                Label = objFile.ViewMainTable.Libelle,
                EdnType = (int)EdnType,
                FileHash = FileDetailFactory.InitFileDetailFactory(objFile).GetFileHash(),
                Table = TableInfosModelFactory.initTableInfosModelFactory(objFile.ViewMainTable).getTableInfosModel(),
            };

            var ieFields = objFile.FldFieldsInfos.Distinct(new FieldComparer()).Where(f => f.PermViewAll);
            var ieDescId = ieFields
                .Select(fld => fld.Descid)
                .Union(ieFields.Where(fld => fld.AliasSourceField != null).Select(fld => fld.AliasSourceField.Descid))
                .Union(ieFields.Select(fld => eLibTools.GetTabFromDescId(fld.Descid)));


            var oFldAdv = FieldsAdvancedCplFactory.InitFieldsAdvancedCplFactory(objFile.Pref, ieDescId);
            var oRes = oFldAdv.GetResInternal();
            var oDescAdv = oFldAdv.GetDescAdvDataSet();

            // on retire les informations propres aux champs de liaison. les informations sur le champs lui-même étant suffisantes
            sm.LstStructFields.AddRange(ieFields
                                      .Select(fldInfos
                                        => FldTypedInfosFactory.InitFldTypedInfosFactory(fldInfos, objFile.Pref, dataFiller: objFile, rgpdM: rgpdData.ContainsKey(fldInfos.Descid) ? rgpdData[fldInfos.Descid] : null)
                                        .GetFldTypedInfos(oRes, oDescAdv, rgpdData)));

            #region  Pour la saisie de date par intervalle on vient préciser le datestartdescid en fonction des dateenddescid renseignés
            try
            {
                IEnumerable<DateFieldInfos> dateFields = sm.LstStructFields
                    .Where(f => f.Format == FieldType.Date)
                    .OfType<DateFieldInfos>();

                IEnumerable<DateFieldInfos> dateFieldEnd = dateFields.Where(df => df.DateEndDescId > 0);

                foreach (DateFieldInfos dateField in dateFieldEnd)
                {
                    //on cherche le champs référencé dans le dateenddescid dans la collection
                    DateFieldInfos dateEndFieldInfo = dateFields.FirstOrDefault(df => df.DescId == dateField.DateEndDescId);
                    if (dateEndFieldInfo != null)
                    {
                        dateEndFieldInfo.DateStartDescId = dateField.DescId;
                    }
                }
            }
            catch (Exception e)
            {
                eModelTools.EudoTraceLog(string.Concat("erreur lors de la définition des dates pour la saisie par intervalle : ", e.Message, Environment.NewLine, e.StackTrace), pref: objFile.Pref);
            }

            #region Traitement du signet Annexe
            //Si le signet Annexes n'est pas présent dans la selection user on le traite manuellement
            //if (!objFile.LstBookmark.Any(b => b.CalledTabDescId % 100 == (int)AllField.ATTACHMENT) && objFile.Record.IsPJViewable)
            //{
            //    Field fldInfosPJ = objFile.FldFieldsInfos.First(f => f.Descid == objFile.ViewMainTable.DescId + (int)AllField.ATTACHMENT);

            //    if (fldInfosPJ != null && fldInfosPJ.PermViewAll)
            //        LstStructBkm.Add(new StructBkmModel()
            //        {
            //            DescId = fldInfosPJ.Descid,
            //            Label = fldInfosPJ.Libelle,
            //            TableType = (int)EdnType.FILE_PJ,
            //            HistoricActived = false,
            //            Actions = new StructBkmModel.DisplayButtons()
            //            {
            //                Add = objFile.ViewMainTable.PermAddPj,
            //                AddFromFilter = false,
            //                Historic = false,
            //                DeleteFromFilter = false,
            //                Import = false,
            //                ImportTarget = false,
            //                Export = false,
            //                Merge = false,
            //                Chart = false,
            //                Print = false,
            //                Mailing = false,
            //                SMS = false,
            //                Formular = false,

            //            }


            //        });


            //}
            #endregion

            #endregion

            return sm;
        }

        #endregion
    }
}