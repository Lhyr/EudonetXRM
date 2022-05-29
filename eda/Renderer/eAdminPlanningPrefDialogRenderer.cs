using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPlanningPrefDialogRenderer : eRenderer
    {
        int _userId;

        private eAdminPlanningPrefDialogRenderer(ePref pref, int tab, int userid)
        {
            Pref = pref;
            this._tab = tab;
            this._userId = userid;
        }

        public static eAdminPlanningPrefDialogRenderer CreateAdminPlanningPrefDialogRenderer(ePref pref, int tab, int userid)
        {
            return new eAdminPlanningPrefDialogRenderer(pref, tab, userid);
        }

        public Int32 GetCurrentTab
        {

            get
            {
                return _tab;

            }
        }
        protected override bool Build()
        {
            if (base.Build())
            {
                GenerateTableChoice();

                GenerateOptions();

                return true;
            }
            else
            {
                return false;
            }
        }


        private void GenerateTableChoice()
        {
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 1300);
            label.Attributes.Add("for", "ddlPlanningTab");

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlPlanningTab";
            ddl.Attributes.Add("lastvalid", this._tab.ToString());
            ddl.Attributes.Add("onchange", String.Concat("refreshPlanningPref(this, ", this._userId, ")"));



            List<Tuple<int, String>> tablesList = eAdminTools.GetPlanningTables(Pref, null);
            if (tablesList.Count > 1)
            {

                ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 436), "0"));
                if (_tab == 0)
                {
                    //  _tab = tablesList[0].Item1;
                }
            }
            else
            {
                _tab = tablesList[0].Item1;

            }

            foreach (Tuple<int, String> t in tablesList)
            {
                ddl.Items.Add(new ListItem(t.Item2, t.Item1.ToString()));
            }

            ddl.SelectedValue = this._tab.ToString();

            PgContainer.Controls.Add(label);
            PgContainer.Controls.Add(ddl);
        }

        private void GenerateOptions()
        {
            Panel wrapper = new Panel();
            wrapper.ID = "planningPrefContent";

            if (this._tab != 0)
            {
                eRenderer rdr = eAdminRendererFactory.CreateAdminPlanningPrefRenderer(Pref, _tab, _userId);
                wrapper.Controls.Add(rdr.PgContainer);
            }

            PgContainer.Controls.Add(wrapper);
        }


    }
}