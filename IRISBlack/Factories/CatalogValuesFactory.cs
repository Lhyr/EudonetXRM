using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.IRISBlack.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.IRISBlack.Factories
{
    /// <summary>
    /// création des listes de valeurs
    /// </summary>
    public class CatalogValuesFactory
    {
        /// <summary>
        /// Génération du modèle de réponse à partir du catalogue
        /// </summary>
        /// <param name="catalog"></param>
        /// <returns></returns>
        public static CatalogValuesModel GetModel(eCatalog catalog) {
            CatalogValuesModel catModel;
            catModel = new CatalogValuesModel();
            catalog.Values.ForEach(v =>
                catModel.Values.Add(new CatalogValuesModel.Value()
                {
                    DbValue = v.DbValue,
                    Code = v.Data,
                    DisplayLabel = v.DisplayValue,
                    Hidden = v.IsDisabled,
                    ParentId = v.ParentId,
                    ToolTipText = v.ToolTipText,
                })
            );
            return catModel;

        }
    }
}