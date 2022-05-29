using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFieldsRGPDListDialogRenderer : eAdminRenderer
    {
        List<Tuple<int, String>> _listTabs;
        
        private eAdminFieldsRGPDListDialogRenderer(ePref pref, int tab)
        {
            Pref = pref;
            _tab = tab;
        }

        public static eAdminFieldsRGPDListDialogRenderer CreateAdminFieldsRGPDListDialogRenderer(ePref pref, int tab)
        {
            return new eAdminFieldsRGPDListDialogRenderer(pref, tab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                //chargement list onglet
                _listTabs = eAdminTools.GetListTabs(Pref);

                return true;
            }

            return false;
        }

        protected override bool Build()
        {
            if (base.Build())
            {
                CreateNavBar();

                CreateTable();

                return true;
            }

            return false;
        }

        private void CreateNavBar()
        {
            // Liste des onglets
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlTabsList";

            foreach (Tuple<int, String> tuple in _listTabs)
            {
                ddl.Items.Add(new ListItem(tuple.Item2, tuple.Item1.ToString()));
            }
            ddl.SelectedValue = _tab.ToString();
            ddl.Attributes.Add("onchange", "nsAdminFieldsRGPDList.changeTab(this)");

            _pgContainer.Controls.Add(ddl);

            //Champ de recherche
            _pgContainer.Controls.Add(eAdminTools.CreateSearchBar("searchContainer", "tableFieldsList"));
        }

        private void CreateTable()
        {
            Panel pTableContainer = new Panel();
            pTableContainer.ID = "fieldsListContainer";
            pTableContainer.CssClass = "adminCntnt";

            eAdminRenderer rdr = eAdminRendererFactory.CreateAdminFieldsRGPDListRenderer(Pref, _tab);
            pTableContainer.Controls.Add(rdr.PgContainer);

            _pgContainer.Controls.Add(pTableContainer);
        }
    }
}
