using Com.Eudonet.Core.Model;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Classe pour initialiser un PagingInfoModel
    /// </summary>
    public class PagingInfoModelFactory
    {

        #region propriétés
        eList objectList { get; set; }
        #endregion

        #region construteurs
        /// <summary>
        /// constructeur pour l'objet
        /// </summary>
        /// <param name="objLst"></param>
        private PagingInfoModelFactory(eList objLst)
        {
            objectList = objLst;
        }
        #endregion

        #region static initializer
        /// <summary>
        /// Initialiseur statique pour la classe.
        /// Retourne une instance de la classe.
        /// </summary>
        /// <param name="objLst"></param>
        /// <returns></returns>
        public static PagingInfoModelFactory InitPagingInfoModelFactory(eList objLst)
        {
            return new PagingInfoModelFactory(objLst);
        }
        #endregion

        #region privées
        #endregion

        #region public
        /// <summary>
        /// Crée une intance de ListDetailModel.StructureModel et la retourne.
        /// </summary>
        /// <returns></returns>
        public PagingInfoModel GetPagingInfoModel()
        {
            return new PagingInfoModel
            {
                RowsPerPage = objectList.RowsByPage,
                NbTotalRows = objectList.NbTotalRows,
                NbPages = objectList.NbPage,
                Page = objectList.Page
            };
        }
        #endregion
    }
}