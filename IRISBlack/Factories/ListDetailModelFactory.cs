using Com.Eudonet.Xrm.IRISBlack.Model;
using static Com.Eudonet.Xrm.IRISBlack.Model.ListDetailModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Permet de créer une instance de ListDetailModel
    /// avec des objets passés en paramètres.
    /// </summary>
    public class ListDetailModelFactory
    {
        #region propriétés
        protected StructureModel strucModel { get; set; }
        protected IEnumerable<IRecordModel> recordsModel { get; set; }
        protected PagingInfoModel pagingInfoModel { get; set; }


        protected ePref pref { get; set; }

        protected eList dataFiller { get; set; }
        #endregion



        #region construteurs
        /// <summary>
        /// constructeur qui prend en paramètre tout ce dont a besoin la 
        /// factory pour construire ListDetailModel
        /// <paramref name="pref"/>
        /// <paramref name="sm" />
        /// <paramref name="dtf" />
        /// <paramref name="pagingInfo" />
        /// </summary>
        protected ListDetailModelFactory(ePref pref, StructureModel sm, eList dtf, PagingInfoModel pagingInfo)
        {
            dataFiller = dtf;
            strucModel = sm;
            recordsModel = GetRecordModelFromERecords();
            pagingInfoModel = pagingInfo;
            this.pref = pref;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Prend en paramètre tout ce dont a besoin la factory pour construire ListDetailModel
        /// Retourne une instance de la classe.
        /// <paramref name="pref"/>
        /// <paramref name="sm" />
        /// <paramref name="dtf" />
        /// <paramref name="pagingInfo" />
        /// </summary>
        /// <returns></returns>
        public static ListDetailModelFactory InitListDetailModelFactory(ePref pref, StructureModel sm, eList dtf, PagingInfoModel pagingInfo)
        {
            return new ListDetailModelFactory(pref, sm, dtf, pagingInfo);
        }
        #endregion

        #region privées

        protected int getNbResults()
        {

            if (!pref.Context.Paging.HasCount)
                try
                {
                    eDataTools.SetCountCurrentList(pref);
                }
                catch (NotInitializedCountingRequest ex)
                {
                    //dans le cas des signets cette partie ne fonctionne pas... TODO?
                }

            return pref.Context.Paging.NbResult;


        }
        /// <summary>
        /// Détermine quel Record model on utilise
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IRecordModel> GetRecordModelFromERecords()
        {
            EdnType edt = dataFiller.ViewMainTable.EdnType;
            var recordsPage = dataFiller.ListRecords?.Take(dataFiller.RowsByPage);
            bool bIsEmptyRecord = recordsPage?.FirstOrDefault() is eEmptyRecord;


            if ((edt == EdnType.FILE_MAIL || edt == EdnType.FILE_SMS) && !bIsEmptyRecord)
                return recordsPage?.Select(rec => RecordMailFactory.InitRecordMailFactory(rec as eRecordMail, dataFiller, pref).ConstructRecordModel());
            else
                return recordsPage?.Select(rec => RecordFactory.InitRecordFactory(rec, dataFiller, pref).ConstructRecordModel());

        }

        #endregion

        #region public
        /// <summary>
        /// Crée une intance de ListDetailModel et la retourne.
        /// </summary>
        /// <returns></returns>
        public ListDetailModel GetListDetailModel()
        {
            return new ListDetailModel
            {
                Data = recordsModel?.ToList() ?? new List<IRecordModel>(),
                PagingInfo = pagingInfoModel,
                Structure = strucModel,
                NbResults = getNbResults()
            };
        }


        #endregion

    }
}