using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminWebBkmRenderer : eAdminBlockRenderer
    {

        public static eAdminWebBkmRenderer CreateAdminWebBkmRenderer(ePref pref, eAdminTableInfos tabInfos, String title)
        {
            eAdminWebBkmRenderer r = new eAdminWebBkmRenderer(pref, tabInfos, title);
            return r;
        }

        private eAdminWebBkmRenderer(ePref pref, eAdminTableInfos tabInfos, String title)
            : base(pref, tabInfos, title, idBlock: "partWebBkm")
        {
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                bool bGridEnabled = Pref.GetConfigDefault(new eLibConst.CONFIG_DEFAULT[]
                { eLibConst.CONFIG_DEFAULT.GRIDENABLED })[eLibConst.CONFIG_DEFAULT.GRIDENABLED] == "1";

                // Colonnes
                eAdminField adminField = new eAdminTextboxField(_tabInfos.DescId, eResApp.GetResWithColon(Pref, 6918), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.BREAKLINE.GetHashCode(), AdminFieldType.ADM_TYPE_NUM, value: _tabInfos.BreakLine.ToString());
                adminField.SetFieldControlID("admBreakLine");
                adminField.Generate(_panelContent);

                if (bGridEnabled)
                {
                    // Drag du signet web
                    HtmlGenericControl ul = new HtmlGenericControl("ul");
                    HtmlGenericControl li = new HtmlGenericControl("li");
                    ul.Attributes.Add("click", "");
                    ul.Attributes.Add("eactive", "0");

                    if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Bkm_Grid))
                    {
                        // Signet Grille 
                        //drag&droppage 
                        li = new HtmlGenericControl("li");
                        li.Attributes.Add("draggable", "true");
                        li.Attributes.Add("edragtype", DragType.BKM_GRID.GetHashCode().ToString());
                        li.Attributes.Add("esubtype", "2");

                        //li.Attributes.Add("ondragstart", "nsAdmin.webBkmDragStartHandler()");
                        li.Attributes.Add("ondragstart", "nsAdminAddBkmWeb.onDragStart(event);");
                        li.Attributes.Add("ondrag", "nsAdminAddBkmWeb.onDrag(event);");
                        li.Attributes.Add("ondragend", "nsAdminAddBkmWeb.onDragEnd(event);");
                        li.Attributes.Add("title", eResApp.GetRes(_ePref, 8574));

                        HtmlGenericControl icon = new HtmlGenericControl("span");
                        icon.Attributes.Add("class", "icon-cloud-upload fieldIcon");
                        li.Controls.Add(icon);
                        HtmlGenericControl span = new HtmlGenericControl("span");
                        span.InnerText = "Signet Grille";
                        li.Controls.Add(span);

                        ul.Controls.Add(li);

                    }
                    else
                    {

                        // Interne 
                        //drag&droppage 
                        li = new HtmlGenericControl("li");
                        li.Attributes.Add("draggable", "true");
                        li.Attributes.Add("edragtype", DragType.WEB_SIGNET.GetHashCode().ToString());
                        li.Attributes.Add("esubtype", "0");

                        //li.Attributes.Add("ondragstart", "nsAdmin.webBkmDragStartHandler()");
                        li.Attributes.Add("ondragstart", "nsAdminAddBkmWeb.onDragStart(event);");
                        li.Attributes.Add("ondrag", "nsAdminAddBkmWeb.onDrag(event);");
                        li.Attributes.Add("ondragend", "nsAdminAddBkmWeb.onDragEnd(event);");
                        li.Attributes.Add("title", eResApp.GetRes(Pref, 6920));

                        HtmlGenericControl icon = new HtmlGenericControl("span");
                        icon.Attributes.Add("class", "icon-cloud-upload fieldIcon");
                        li.Controls.Add(icon);
                        HtmlGenericControl span = new HtmlGenericControl("span");
                        span.InnerText = eResApp.GetRes(Pref, 6913);
                        li.Controls.Add(span);

                        ul.Controls.Add(li);

                        // Externe 
                        //drag&droppage 
                        li = new HtmlGenericControl("li");
                        li.Attributes.Add("draggable", "true");
                        li.Attributes.Add("edragtype", DragType.WEB_SIGNET.GetHashCode().ToString());
                        li.Attributes.Add("esubtype", "1");
                        //li.Attributes.Add("ondragstart", "nsAdmin.webBkmDragStartHandler()");
                        li.Attributes.Add("ondragstart", "nsAdminAddBkmWeb.onDragStart(event);");
                        li.Attributes.Add("ondrag", "nsAdminAddBkmWeb.onDrag(event);");
                        li.Attributes.Add("ondragend", "nsAdminAddBkmWeb.onDragEnd(event);");
                        li.Attributes.Add("title", eResApp.GetRes(Pref, 6920) + " externe");

                        icon = new HtmlGenericControl("span");
                        icon.Attributes.Add("class", "icon-cloud-upload fieldIcon");
                        li.Controls.Add(icon);
                        span = new HtmlGenericControl("span");
                        span.InnerText = eResApp.GetRes(Pref, 6913) + " externe.";
                        li.Controls.Add(span);

                        ul.Controls.Add(li);
                    }

                    _panelContent.Controls.Add(ul);
                }


                return true;
            }
            return false;

        }
    }
}