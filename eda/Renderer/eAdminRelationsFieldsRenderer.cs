using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRelationsFieldsRenderer : eAdminRenderer
    {
        eAdminTableInfos _tabInfos;
        int _nTab;
        private eAdminRelationsFieldsRenderer(ePref pref, int tab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, tab);
            _nTab = _tabInfos.DescId;
        }

        public static eAdminRelationsFieldsRenderer CreateRelationsFieldsRenderer(ePref pref, int tab)
        {
            return new eAdminRelationsFieldsRenderer(pref, tab);
        }

        protected override bool Build()
        {
            #region Récupération des rubriques disponibles/sélectionnées

            ExtendedDictionary<int, String> allSelectedFields = new ExtendedDictionary<int, string>();
            ExtendedDictionary<int, String> availableFields = _tabInfos.GetAvailableFieldsFromParentTables(Pref);

            Dictionary<String, String> headerPref = Pref.GetPrefDefault(_nTab, new List<String> { "HEADER_300", "HEADER_200" });
            String header300 = headerPref["HEADER_300"];
            String header200 = headerPref["HEADER_200"];
            String headerFields = String.Concat(header300, (String.IsNullOrEmpty(header300)) ? "" : ";", header200);
            ExtendedDictionary<int, String> selectedFields = _tabInfos.GetSelectedFieldsFromParentTables(Pref, headerFields);

            int nHeaderField;
            String[] arrHeaderFields = headerFields.Split(';');
            foreach (String headerField in arrHeaderFields)
            {
                nHeaderField = eLibTools.GetNum(headerField);
                if (selectedFields.ContainsKey(nHeaderField))
                {
                    allSelectedFields.AddOrUpdateValue(nHeaderField, selectedFields[nHeaderField], true);
                }

            }
            #endregion

            eRenderer rdr;
            rdr = eFieldsSelectRenderer.CreateFieldsSelectRenderer(Pref, availableFields, allSelectedFields);
            rdr.Generate();
            this.PgContainer.Controls.Add(rdr.PgContainer);

            // Champ caché pour avoir l'attribut "dsc" pour la liste des rubriques sélectionnées
            Control hidSelectedFieldsLeft = eAdminFieldBuilder.BuildField(this.PgContainer, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.HEADER_300.GetHashCode());
            hidSelectedFieldsLeft.ID = "hidHeader300";
            Control hidSelectedFieldsRight = eAdminFieldBuilder.BuildField(this.PgContainer, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.HEADER_200.GetHashCode());
            hidSelectedFieldsRight.ID = "hidHeader200";

            return true;
        }
    }
}