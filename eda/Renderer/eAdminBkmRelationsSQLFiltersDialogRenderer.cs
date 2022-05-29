using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBkmRelationsSQLFiltersDialogRenderer : eAdminRenderer
    {
        #region propriétés
        private Int32 _nTab;
        private String _tabName;
        private eAdminTableInfos _tabInfos;

        private string tabNameEVT = String.Empty;
        private string tabNamePP = String.Empty;
        private string tabNamePM = String.Empty;

        string valuePP;
        string valuePM;
        string valueEVT;
        #endregion


        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminBkmRelationsSQLFiltersDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = _tabInfos.DescId;
            _tabName = _tabInfos.TableLabel;
        }

        public static eAdminBkmRelationsSQLFiltersDialogRenderer CreateAdminBkmRelationsSQLFiltersDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminBkmRelationsSQLFiltersDialogRenderer(pref, nTab);
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                LoadTabRes();
                LoadBkmPrefValues();

                return true;
            }
            return false;
        }

        protected void LoadTabRes()
        {
            List<string> listDescIds = new List<string>()
                    {
                        EudoQuery.TableType.PP.GetHashCode().ToString(),
                        EudoQuery.TableType.PM.GetHashCode().ToString()
                    };

            if (_tabInfos.InterEVT)
                listDescIds.Add(_tabInfos.InterEVTDescid.ToString());

            eRes _res = new eRes(Pref, String.Join(",", listDescIds.ToArray()));

            bool bResFound = false;
            tabNamePP = _res.GetRes(EudoQuery.TableType.PP.GetHashCode(), out bResFound);
            if (!bResFound)
                tabNamePP = "Contacts";

            bResFound = false;
            tabNamePM = _res.GetRes(EudoQuery.TableType.PM.GetHashCode(), out bResFound);
            if (!bResFound)
                tabNamePM = "Sociétés";

            if (_tabInfos.InterEVT)
            {
                bResFound = false;
                tabNameEVT = _res.GetRes(_tabInfos.InterEVTDescid, out bResFound);
                if (!bResFound)
                    tabNameEVT = "Onglet parent";
            }
        }

        protected void LoadBkmPrefValues()
        {
            if (_tabInfos.InterPP)
            {
                eBkmPref bkmPref = new eBkmPref(Pref, (int)TableType.PP, _nTab);
                valuePP = bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE);
            }
            if (_tabInfos.InterPM)
            {
                eBkmPref bkmPref = new eBkmPref(Pref, (int)TableType.PM, _nTab);
                valuePM = bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE);
            }
            if (_tabInfos.InterEVT)
            {
                eBkmPref bkmPref = new eBkmPref(Pref, _tabInfos.InterEVTDescid, _nTab);
                valueEVT = bkmPref.GetBkmPref(ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE);
            }
        }

        protected override bool Build()
        {
            if(base.Build())
            {
                _pgContainer.ID = "bkmRelationsSQLFiltersModalContent";
                _pgContainer.Attributes.Add("class", "adminModalContent");

                HtmlInputHidden updateCategory = new HtmlInputHidden();
                updateCategory.ID = "updateCategory";
                updateCategory.Value = ((int)eAdminUpdateProperty.CATEGORY.BKMPREF).ToString();

                HtmlInputHidden updateFieldTab = new HtmlInputHidden();
                updateFieldTab.ID = "updateFieldTab";
                updateFieldTab.Value = ((int)ePrefConst.PREF_BKMPREF.TAB).ToString();

                HtmlInputHidden updateFieldBkm = new HtmlInputHidden();
                updateFieldBkm.ID = "updateFieldBkm";
                updateFieldBkm.Value = ((int)ePrefConst.PREF_BKMPREF.BKM).ToString();

                HtmlInputHidden updateFieldAddBkmWhere = new HtmlInputHidden();
                updateFieldAddBkmWhere.ID = "updateFieldAddedBkmWhere";
                updateFieldAddBkmWhere.Value = ((int)ePrefConst.PREF_BKMPREF.ADDEDBKMWHERE).ToString();

                // DescID de la table affichée en signet
                HtmlInputHidden valueBkm = new HtmlInputHidden();
                valueBkm.ID = "valueBkm";
                valueBkm.Value = _tabInfos.DescId.ToString();

                _pgContainer.Controls.Add(updateCategory);
                _pgContainer.Controls.Add(updateFieldTab);
                _pgContainer.Controls.Add(updateFieldBkm);
                _pgContainer.Controls.Add(updateFieldAddBkmWhere);
                _pgContainer.Controls.Add(valueBkm);

                //entête
                Panel divContainer = new Panel();
                divContainer.CssClass = "field fieldHeader";
                _pgContainer.Controls.Add(divContainer);

                HtmlGenericControl label = new HtmlGenericControl("label");
                label.InnerText = eResApp.GetRes(Pref, 7462).Replace("<TAB>", _tabInfos.TableLabel);
                divContainer.Controls.Add(label);


                if (_tabInfos.InterPP)
                    BuildSection("PP", tabNamePP, (int)TableType.PP, valuePP);

                if (_tabInfos.InterPM)
                    BuildSection("PM", tabNamePM, (int)TableType.PM, valuePM);

                if (_tabInfos.InterEVT)
                    BuildSection("EVT", tabNameEVT, _tabInfos.InterEVTDescid, valueEVT);

                return true;
            }
            return false;
        }

        private void BuildSection(string sectionName, string parentTabName, int parentTabDescId, string value)
        {
            Panel divContainer = new Panel();
            divContainer.CssClass = "field";
            _pgContainer.Controls.Add(divContainer);

            string textareaId = String.Concat("valueAddedBkmWhere", sectionName);

            // DescID de la table parente
            HtmlInputHidden valueTab = new HtmlInputHidden();
            valueTab.ID = String.Concat("valueTab", sectionName);
            valueTab.Value = parentTabDescId.ToString();
            divContainer.Controls.Add(valueTab);

            Panel divLeft = new Panel();
            divLeft.CssClass = "divContainerLeft";
            divContainer.Controls.Add(divLeft);

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("for", textareaId);
            label.InnerText = eResApp.GetRes(Pref, 7463);
            divLeft.Controls.Add(label);

            label = new HtmlGenericControl("label");
            label.Attributes.Add("for", textareaId);
            label.InnerText = eResApp.GetRes(Pref, 7464).Replace("<TAB>", parentTabName);
            divLeft.Controls.Add(label);


            Panel divRight = new Panel();
            divRight.CssClass = "divContainerRight";
            divContainer.Controls.Add(divRight);            

            HtmlTextArea valueAddedBkmWhere = new HtmlTextArea();
            valueAddedBkmWhere.ID = textareaId;
            valueAddedBkmWhere.Attributes.Add("maxlength", "500");
            valueAddedBkmWhere.InnerText = value; // #36751 : On fait un InnerText à la place d'un InnerHTML pour ne pas décoder la valeur
            if(Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                valueAddedBkmWhere.Attributes.Add("disabled", "disabled");
            divRight.Controls.Add(valueAddedBkmWhere);
        }
    }
}