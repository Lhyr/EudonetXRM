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
    public class eFieldsSelectRenderer : eRenderer
    {
        Dictionary<Int32, String> _avalaibleFields;
        Dictionary<Int32, String> _selectedFields;

        private eFieldsSelectRenderer(ePref pref, Dictionary<Int32, String> avalaibleFields, Dictionary<Int32, String> selectedFields)
        {
            Pref = pref;

            _selectedFields = selectedFields;
            // On retire les rubriques sélectionnées de la liste des rubriques disponibles
            foreach (KeyValuePair<Int32, String> item in _selectedFields)
            {
                avalaibleFields.Remove(item.Key);
            }
            _avalaibleFields = avalaibleFields;
        }

        public static eFieldsSelectRenderer CreateFieldsSelectRenderer(ePref pref, Dictionary<Int32, String> avalaibleFields, Dictionary<Int32, String> selectedFields)
        {
            return new eFieldsSelectRenderer(pref, avalaibleFields, selectedFields);
        }


        protected override bool Build()
        {
            Panel divTitle = new Panel();
            divTitle.ID = "titleBlock";
            this._pgContainer.Controls.Add(divTitle);

            HtmlGenericControl spanTitle = new HtmlGenericControl("span");
            spanTitle.InnerText = String.Concat(eResApp.GetRes(Pref, 6712), " :");
            divTitle.Controls.Add(spanTitle);


            Panel divSearch = new Panel();
            divSearch.ID = "searchBlock";

            //HtmlGenericControl label = new HtmlGenericControl("label");
            //label.InnerText = String.Concat(eResApp.GetRes(Pref, 595), " : ");
            TextBox txtSearch = new TextBox();
            txtSearch.ID = "txtSearchField";
            txtSearch.Attributes.Add("placeholder", eResApp.GetRes(Pref, 595));

            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-edn-cross");
            icon.ID = "iconRemoveSearch";
            icon.Style.Add("display", "none");

            //divSearch.Controls.Add(label);
            divSearch.Controls.Add(txtSearch);
            divSearch.Controls.Add(icon);

            this._pgContainer.Controls.Add(divSearch);

            eRenderer rdr = eAvailableFieldsRenderer.CreateAvailableFieldsRenderer(Pref, _avalaibleFields);
            rdr.Generate();
            this._pgContainer.Controls.Add(rdr.PgContainer);

            HtmlGenericControl arrows = new HtmlGenericControl("div");
            arrows.Attributes.Add("class", "horizontalArrows");
            HtmlGenericControl arrow = new HtmlGenericControl("div");
            arrow.Attributes.Add("class", "moveBetween icon-item_add");
            arrow.ID = "moveRight";
            arrows.Controls.Add(arrow);
            arrow = new HtmlGenericControl("div");
            arrow.Attributes.Add("class", "moveBetween icon-item_rem");
            arrow.ID = "moveLeft";
            arrows.Controls.Add(arrow);
            this._pgContainer.Controls.Add(arrows);

            rdr = eSelectedFieldsRenderer.CreateSelectedFieldsRenderer(Pref, _selectedFields);
            rdr.Generate();
            this._pgContainer.Controls.Add(rdr.PgContainer);

            arrows = new HtmlGenericControl("div");
            arrows.Attributes.Add("class", "verticalArrows");
            arrow = new HtmlGenericControl("div");
            arrow.Attributes.Add("class", "moveUpDown icon-item_up");
            arrow.ID = "moveUp";
            arrows.Controls.Add(arrow);
            arrow = new HtmlGenericControl("div");
            arrow.Attributes.Add("class", "moveUpDown icon-item_down");
            arrow.ID = "moveDown";
            arrows.Controls.Add(arrow);
            this._pgContainer.Controls.Add(arrows);

            return true;
        }
    }
}