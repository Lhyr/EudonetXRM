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
    public class eSelectedFieldsRenderer : eChoiceListRenderer
    {


        private eSelectedFieldsRenderer(ePref pref, Dictionary<Int32, String> fields) : base(pref, fields)
        {
        }

        public static eSelectedFieldsRenderer CreateSelectedFieldsRenderer(ePref pref, Dictionary<Int32, String> fields)
        {
            return new eSelectedFieldsRenderer(pref, fields);
        }

        protected override Boolean Init()
        {
            this._listLabel = eResApp.GetRes(Pref, 6230); //Rubriques sélectionnées
            this._tableID = "selectedFields";
            return true;
        }

        protected override void CreateSelectAllButton()
        {
            Panel p = new Panel();
            p.CssClass = "btnSelectAll";
            p.ID = "btnDeselectAll";

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-remove_all");

            HtmlGenericControl text = new HtmlGenericControl();
            text.Attributes.Add("class", "selectAllText");
            text.InnerText = eResApp.GetRes(Pref, 432); //Tout désélectionner

            p.Controls.Add(icon);
            p.Controls.Add(text);

            this.PgContainer.Controls.Add(p);
        }

    }
}