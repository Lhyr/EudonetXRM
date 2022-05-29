using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    public class eSelectionCriteriaRenderer : eRenderer
    {
        eudoDAL _eDal;
        /// <summary>Tab DescID du signet</summary>
        int _tabID;

        private eSelectionCriteriaRenderer(ePref pref, int bkmTabID)
        {
            Pref = pref;
            _eDal = eLibTools.GetEudoDAL(Pref);
            _tabID = bkmTabID;
        }

        public static eSelectionCriteriaRenderer CreateSelectionCriteriaRenderer(ePref pref, int bkmTabID)
        {
            eSelectionCriteriaRenderer criteria = new eSelectionCriteriaRenderer(pref, bkmTabID);
            return criteria;
        }

        protected override Boolean Build()
        {
            PgContainer.ID = "blockCriteria";

            try
            {
                String error = String.Empty;
                List<eFilemapPartner> listFilters = eFilemapPartner.LoadMapping(Pref, null, EudoQuery.FILEMAP_TYPE.SELECTION_FILTERS, _tabID, out error);

                // Tri par Ordre
                listFilters = listFilters.OrderBy(f => f.Order).ToList();

                foreach (eFilemapPartner filter in listFilters)
                {
                    CreateFilter(filter);
                }

                if (!String.IsNullOrEmpty(error))
                {
                    // Retourner l'erreur
                    return false;
                }
            }
            catch (Exception exc)
            {
                throw exc;
                return false;
            }


            return true;
        }


        private void CreateFilter(eFilemapPartner filter)
        {
            String error = String.Empty;

            try
            {

                eFieldLiteSelectionCriteria field = new eFieldLiteSelectionCriteria(Pref, filter.SourceDescId, filter.FieldLabel, filter.Order);
                PgContainer.Controls.Add(field.GetRender());

                //if ()
                //{
                //    Panel fieldDiv = field.GetRender();
                //    p.Controls.Add(fieldDiv);
                //}
                //else
                //{
                //    throw new Exception("eSelectionCriteriaRenderer.CreateFilter => Erreur récupération des données du champ de critère : " + error);
                //}
            }
            catch (Exception e)
            {
                throw e;
            }


        }
    }
}