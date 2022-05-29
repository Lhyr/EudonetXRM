using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminAdvSearchDialogRenderer : eAdminRenderer
    {
        eAdminTableInfos _tabInfos;

        protected string _listCol;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="listCol"></param>
        private eAdminAdvSearchDialogRenderer(ePref pref, int nTab, string listCol = "")
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _tab = nTab;
            _listCol = listCol;
        }

        public static eAdminAdvSearchDialogRenderer CreateAdminAdvSearchDialogRenderer(ePref pref, int nTab, string listCol = "")
        {
            return new eAdminAdvSearchDialogRenderer(pref, nTab, listCol);
        }


        protected override bool Build()
        {
            String error = String.Empty;

            Control hid = eAdminFieldBuilder.BuildField(_pgContainer, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.COLSPREF, eLibConst.PREF_COLSPREF.Col.GetHashCode());
            hid.ID = "hidCols";

            ExtendedDictionary<int, String> availableFields = _tabInfos.GetAvailableFieldsFromParentTables(Pref, true);
            ExtendedDictionary<int, String> allSelectedFields = new ExtendedDictionary<int, string>();

            String listCol = string.Empty;
            // Récupération des rubriques sélectionnées
            if (String.IsNullOrEmpty(_listCol))//Si listCol non vide c est qu on est dans le cas d un widget liste pour la page d accueil
            {
                eColsPref colsPref = new eColsPref(Pref, _tab, ColPrefType.FINDERPREF);
                listCol = colsPref.GetColsDefaultPref(eLibConst.PREF_COLSPREF.Col);
            }
            else
            {
                listCol = _listCol.CompareTo("all") == 0 ? "" : _listCol;
            }  

            if (String.IsNullOrEmpty(listCol))
                allSelectedFields = new ExtendedDictionary<int, string>();
            else
            {
                Dictionary<int, String> dicFields = eLibTools.GetRes(Pref, listCol, Pref.Lang, out error);
                allSelectedFields = new ExtendedDictionary<int, string>(dicFields);
            }

            eFieldsSelectRenderer rdr = eFieldsSelectRenderer.CreateFieldsSelectRenderer(Pref, availableFields, allSelectedFields);
            rdr.Generate();

            _pgContainer.Controls.Add(rdr.PgContainer);

            return base.Build();
        }

    }
}