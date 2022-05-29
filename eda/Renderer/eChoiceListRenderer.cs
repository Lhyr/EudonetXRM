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
    public class eChoiceListRenderer : eRenderer
    {
        protected Dictionary<Int32, String> _fields;
        protected String _listLabel;
        protected String _tableID;

        protected eChoiceListRenderer(ePref pref, Dictionary<Int32, String> fields)
        {
            Pref = pref;
            _fields = fields;
        }


        protected override Boolean Build()
        {
            this.PgContainer.ID = this._tableID;

            Panel p = new Panel();
            p.CssClass = "choiceTable";
            
 
            // Header
            HtmlGenericControl header = new HtmlGenericControl("div");
            header.Attributes.Add("class", "tableHeader");
            header.InnerText = _listLabel;

            p.Controls.Add(header);

            int lineCount = 0;
            bool oddLine = true;

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "listFields");
            HtmlGenericControl li;

            foreach (KeyValuePair<Int32, String> field in _fields)
            {
                li = new HtmlGenericControl("li");
                li.ID = String.Concat("field", field.Key);

                if (oddLine)
                    li.Attributes.Add("class", "line1");

                li.InnerText = field.Value;

                li.Attributes.Add("did", field.Key.ToString());
                li.Attributes.Add("draggable", "true");
                li.Attributes.Add("data-active", "1");

                ul.Controls.Add(li);

                oddLine = !oddLine;
                ++lineCount;
            }

            p.Controls.Add(ul);

            this.PgContainer.Controls.Add(p);

            CreateSelectAllButton();

            return true;
        }

        protected virtual void CreateSelectAllButton()
        {

        }
    }
}