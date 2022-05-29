using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe de rendu de la modale pour la liste des rubriques d'un onglet
    /// </summary>
    public class eAdminFieldsListDialogRenderer : eAdminRenderer
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        private eAdminFieldsListDialogRenderer(ePref pref, int tab)
        {
            Pref = pref;
            _tab = tab;
        }

        /// <summary>
        /// Crée l'objet demandé en faisant appel à son constructeur
        /// </summary>
        /// <param name="pref">ePref object</param>
        /// <param name="tab">Tab descid</param>
        /// <returns></returns>
        public static eAdminFieldsListDialogRenderer CreateAdminFieldsListDialogRenderer(ePref pref, int tab)
        {
            return new eAdminFieldsListDialogRenderer(pref, tab);
        }


        /// <summary>
        /// Construit le html de l'objet demandé
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                // Liste des onglets
                DropDownList ddl = new DropDownList();
                ddl.ID = "ddlTabsList";

                List<Tuple<int, String>> tabsList = eAdminTools.GetListTabs(Pref);
                foreach (Tuple<int, String> tuple in tabsList)
                {
                    ddl.Items.Add(new ListItem(tuple.Item2, tuple.Item1.ToString()));
                }
                ddl.SelectedValue = _tab.ToString();
                ddl.Attributes.Add("onchange", "nsAdminFieldsList.changeTab(this)");

                _pgContainer.Controls.Add(ddl);

                _pgContainer.Controls.Add(eAdminTools.CreateSearchBar("searchContainer", "tableFieldsList"));

                // Tableau liste des rubriques

                Panel pTableContainer = new Panel();
                pTableContainer.ID = "fieldsListContainer";
                pTableContainer.CssClass = "adminCntnt";

                eAdminRenderer rdr = eAdminRendererFactory.CreateAdminFieldsListRenderer(Pref, _tab);
                //perte de la pile d'appel
                //if (rdr.InnerException != null)
                //    throw rdr.InnerException;

                if (rdr.ErrorMsg.Length > 0)
                    throw new Exception(rdr.ErrorMsg);

                pTableContainer.Controls.Add(rdr.PgContainer);

                _pgContainer.Controls.Add(pTableContainer);

                return true;
            }

            return false;
        }

    }
}