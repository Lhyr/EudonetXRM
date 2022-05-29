using Com.Eudonet.Internal;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTabRightMenuRenderer : eAdminTabFieldRenderer
    {
        string[] _openedBlocks = null;

        public string[] OpenedBlocks
        {
            get
            {
                return _openedBlocks;
            }

            set
            {
                _openedBlocks = value;
            }
        }

        private eAdminTabRightMenuRenderer(ePref pref, Int32 nTab, string[] openedBlocks = null)
        {
            Pref = pref;
            DescId = nTab;
            OpenedBlocks = openedBlocks;
        }

        public static eAdminTabRightMenuRenderer CreateAdminTabRightMenuRenderer(ePref pref, Int32 nTab, string[] openedBlocks = null)
        {
            return new eAdminTabRightMenuRenderer(pref, nTab, openedBlocks);
        }

        /// <summary>
        /// Constructuin du rendu de l'admin du menu des droits
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            _pgContainer.Controls.Add(GetNavBar());

            //flèche qui pointe vers le haut et fait le lien entre le block affiché et l'icone qui le représente
            Panel pnArrow = new Panel();
            _pgContainer.Controls.Add(pnArrow);
            pnArrow.ID = "slidingArrow";

            //block 1
            eAdminRenderer rdr = eAdminRendererFactory.CreateAdminTabContentRenderer(Pref, DescId);
            _pgContainer.Controls.Add(rdr.PgContainer);

            //block 2 
            rdr = eAdminRendererFactory.CreateAdminFieldsParamsRenderer(Pref,  0, false);
            _pgContainer.Controls.Add(rdr.PgContainer);

            //block 3
            rdr = eAdminRendererFactory.CreateAdminTabParametersRenderer(Pref, DescId, OpenedBlocks);
            _pgContainer.Controls.Add(rdr.PgContainer);

            _pgContainer.Controls.Add(GetFooter());

            return base.Build();
        }


        protected HtmlGenericControl GetNavBar()
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.ID = "navTabs";
            ul.Attributes.Add("did", DescId.ToString());

            HtmlGenericControl li = new HtmlGenericControl("li");
            ul.Controls.Add(li);
            li.Attributes.Add("class", "navIcon");
            HtmlGenericControl span = new HtmlGenericControl("span");
            span.Attributes.Add("onclick", "nsAdmin.showBlock('paramTab1');");
            span.ID = "paramTabPicto1";
            span.Attributes.Add("class", "icon-mise-en-page paramTabPicto");
            li.Attributes.Add("title", eResApp.GetRes(Pref, 7819));
            li.Controls.Add(span);

            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);
            li.Attributes.Add("class", "navIcon");
            span = new HtmlGenericControl("span");
            span.Attributes.Add("onclick", "nsAdmin.showBlock('paramTab2');");
            span.ID = "paramTabPicto2";
            span.Attributes.Add("class", "icon-parametres paramTabPicto");
            li.Attributes.Add("title", eResApp.GetRes(Pref, 7820));
            li.Controls.Add(span);

            li = new HtmlGenericControl("li");
            ul.Controls.Add(li);
            li.Attributes.Add("class", "navIcon");
            span = new HtmlGenericControl("span");
            span.Attributes.Add("onclick", "nsAdmin.showBlock('paramTab3');");
            span.ID = "paramTabPicto3";
            span.Attributes.Add("class", "icon-param-onglet paramTabPicto");
            span.Attributes.Add("title", eResApp.GetRes(Pref, 6919));
            li.Attributes.Add("title", eResApp.GetRes(Pref, 7821));
            li.Controls.Add(span);


            return ul;

        }

        protected HtmlGenericControl GetFooter()
        {

            HtmlGenericControl footer = new HtmlGenericControl("footer");

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            footer.Controls.Add(ul);
            ul.Attributes.Add("class", "hLinks");

            HtmlGenericControl li = new HtmlGenericControl("li");
            ul.Controls.Add(li);
            li.Attributes.Add("class", "decBtn");
            li.Attributes.Add("onclick", "doDisco();");

            HtmlGenericControl span = new HtmlGenericControl("span");
            li.Controls.Add(span);

            span.Attributes.Add("class", "icon-logout");

            LiteralControl literal = new LiteralControl(eResApp.GetRes(Pref, 5008));
            li.Controls.Add(literal); //Déconnexion



            return footer;


        }
    }
}