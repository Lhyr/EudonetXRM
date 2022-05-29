using Com.Eudonet.Xrm.IRISBlack.Model.TypedFields;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// Factory pour la création d'un modèle envoyant les données d'une table.
    /// </summary>
    public class TableInfosModelFactory
    {
        #region variables
        Table table { get; set; }
        #endregion
        #region constructeurs
        /// <summary>
        /// Constructeur privé initialisant la factory avec un objet de type Table;
        /// </summary>
        /// <param name="tbl"></param>
        private TableInfosModelFactory(Table tbl)
        {
            table = tbl;
        }
        #endregion
        #region  Initialisation statique de l'objet
        /// <summary>
        /// Initialisation statique de la classe TableInfosModelFactory avec l'objet de type Table.
        /// </summary>
        /// <param name="tbl"></param>
        /// <returns></returns>
        public static TableInfosModelFactory initTableInfosModelFactory(Table tbl)
        {
            return new TableInfosModelFactory(tbl);
        }
        #endregion
        #region public
        /// <summary>
        /// Retourne le modèle recouvrant les informations propres à un onglet.
        /// </summary>
        /// <returns></returns>
        public TableInfosModel getTableInfosModel()
        {
            if (table == null)
                throw new EudoException("L'enregistrement est introuvable");

            return new TableInfosModel
            {
                InterEVTDescid = table.InterEVTDescid,
                NoDefaultLink100 = table.NoDefaultLink100,
                NoDefaultLink200 = table.NoDefaultLink200,
                NoDefaultLink300 = table.NoDefaultLink300,
                NoCascadePMPP = table.NoCascadePMPP,
                NoCascadePPPM = table.NoCascadePPPM,
                InterEvent = table.InterEVT,
                InterEventNeeded = table.InterEVTNeeded,
                InterPM = table.InterPM,
                InterPMNeeded = table.InterPMNeeded,
                InterPP = table.InterPP,
                InterPPNeeded = table.InterPPNeeded,
                SearchLimit = table.SearchLimit,
            };
        }
        #endregion
    }
}