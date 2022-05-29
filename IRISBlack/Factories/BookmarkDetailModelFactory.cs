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
    public class BookmarkDetailModelFactory : ListDetailModelFactory
    {
        #region propriétés

        eBookmark bkm;
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
        private BookmarkDetailModelFactory(ePref pref, StructureModel sm, eList dtf, PagingInfoModel pagingInfo)
            : base(pref, sm, dtf, pagingInfo)
        {
            try
            {
                bkm = (eBookmark)dtf;
            }
            catch (Exception e)
            {

                throw;
            }
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
        public static BookmarkDetailModelFactory InitBookmarkDetailModelFactory(ePref pref, StructureModel sm, eList dtf, PagingInfoModel pagingInfo)
        {
            return new BookmarkDetailModelFactory(pref, sm, dtf, pagingInfo);
        }
        #endregion

        #region privées


        #endregion

        #region public

        /// <summary>
        /// Crée une intance de BookmarkDetailModel et la retourne.
        /// </summary>
        /// <returns></returns>
        public BookmarkDetailModel GetBookmarkDetailModel()
        {

            return new BookmarkDetailModel
            {
                Data = recordsModel?.ToList() ?? new List<IRecordModel>(),
                PagingInfo = pagingInfoModel,
                Structure = strucModel,
                NbResults = getNbResults(),
                Param = new BookmarkDetailModel.CpltParam()
                {
                    RelationDescId = bkm?.RelationFieldDescid ?? 0
                }
            };
        }


        #endregion

    }
}