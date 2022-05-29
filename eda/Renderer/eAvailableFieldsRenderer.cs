using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAvailableFieldsRenderer : eChoiceListRenderer
    {


        private eAvailableFieldsRenderer(ePref pref, Dictionary<Int32, String> fields) : base(pref, fields)
        {
            
        }

        public static eAvailableFieldsRenderer CreateAvailableFieldsRenderer(ePref pref, Dictionary<Int32, String> fields)
        {
            return new eAvailableFieldsRenderer(pref, fields);
        }

        protected override Boolean Init()
        {
            this._listLabel = eResApp.GetRes(Pref, 6229); //Rubriques disponibles
            this._tableID = "availableFields";
            return base.Init();
        }

        protected override void CreateSelectAllButton()
        {
            Panel p = new Panel();
            p.CssClass = "btnSelectAll";
            p.ID = "btnSelectAll";

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-select_all");

            HtmlGenericControl text = new HtmlGenericControl();
            text.Attributes.Add("class", "selectAllText");
            text.InnerText = eResApp.GetRes(Pref, 431); //Tout sélectionner

            p.Controls.Add(icon);
            p.Controls.Add(text);

            this.PgContainer.Controls.Add(p);
        }
    }
}