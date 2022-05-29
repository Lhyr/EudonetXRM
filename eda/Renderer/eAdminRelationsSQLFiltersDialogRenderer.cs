using Com.Eudonet.Internal.eda;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRelationsSQLFiltersDialogRenderer : eAdminRenderer
    {
        eAdminTableInfos _tabInfos;
        String _value;
        int _parentTab;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminRelationsSQLFiltersDialogRenderer(ePref pref, eAdminTableInfos tabInfos, int parentTab, String sValue)
        {
            Pref = pref;
            _tabInfos = tabInfos;
            _parentTab = parentTab;
            _value = sValue;
        }

        public static eAdminRelationsSQLFiltersDialogRenderer CreateAdminRightsSQLFiltersDialogRenderer(ePref pref, eAdminTableInfos tabInfos, int parentTab, String sValue)
        {
            return new eAdminRelationsSQLFiltersDialogRenderer(pref, tabInfos, parentTab, sValue);
        }


        protected override bool Build()
        {
            HtmlInputHidden updateCategory = new HtmlInputHidden();
            updateCategory.ID = "updateCategory";
            updateCategory.Value = eAdminUpdateProperty.CATEGORY.BKMPREF.GetHashCode().ToString();

            HtmlInputHidden updateFieldTab = new HtmlInputHidden();
            updateFieldTab.ID = "updateFieldTab";
            updateFieldTab.Value = ePrefConst.PREF_BKMPREF.TAB.GetHashCode().ToString();

            HtmlInputHidden updateFieldBkm = new HtmlInputHidden();
            updateFieldBkm.ID = "updateFieldBkm";
            updateFieldBkm.Value = ePrefConst.PREF_BKMPREF.BKM.GetHashCode().ToString();

            HtmlInputHidden updateFieldAddBkmWhere = new HtmlInputHidden();
            updateFieldAddBkmWhere.ID = "updateFieldAddedBkmWhere";
            updateFieldAddBkmWhere.Value = ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE.GetHashCode().ToString();

            // DescID de la table parente
            HtmlInputHidden valueTab = new HtmlInputHidden();
            valueTab.ID = "valueTab";
            valueTab.Value = _parentTab.ToString();

            // DescID de la table affichée en signet
            HtmlInputHidden valueBkm = new HtmlInputHidden();
            valueBkm.ID = "valueBkm";
            valueBkm.Value = _tabInfos.DescId.ToString();

            HtmlTextArea valueAddedBkmWhere = new HtmlTextArea();
            valueAddedBkmWhere.ID = "valueAddedBkmWhere";
            valueAddedBkmWhere.InnerText = _value; // #36751 : On fait un InnerText à la place d'un InnerHTML pour ne pas décoder la valeur

            _pgContainer.Controls.Add(updateCategory);
            _pgContainer.Controls.Add(updateFieldTab);
            _pgContainer.Controls.Add(updateFieldBkm);
            _pgContainer.Controls.Add(updateFieldAddBkmWhere);

            _pgContainer.Controls.Add(valueTab);
            _pgContainer.Controls.Add(valueBkm);
            _pgContainer.Controls.Add(valueAddedBkmWhere);

            return base.Build();
        }
    }
}