using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminHeaderRenderer : eAdminBlockRenderer
    {
        #region Propriétés
        private Int32 _nTab;
        #endregion

        public static eAdminHeaderRenderer CreateAdminHeaderRenderer(ePref pref, String title, string[] openedBlocks = null)
        {
            eAdminHeaderRenderer r = new eAdminHeaderRenderer(pref, title, openedBlocks);
            if (string.IsNullOrEmpty(r.ErrorMsg))
            {

            }

            return r;
        }

        private eAdminHeaderRenderer(ePref pref, String title, string[] openedBlocks = null)
            : base(pref, null, title, idBlock: "partHeaderFields", bOpenedBlock: openedBlocks != null && openedBlocks.Contains("partHeaderFields"))
        {
        }


        /// <summary>
        /// Build
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {

                _panelContent.ID = "HeaderPart";

                //HtmlGenericControl ul = new HtmlGenericControl("ul");
                //HtmlGenericControl li = new HtmlGenericControl("li");
                //ul.Attributes.Add("click", "");
                //ul.Attributes.Add("eactive", "0");

                ////drag&droppage 
                //li.Attributes.Add("draggable", "true");
                //li.Attributes.Add("edragtype", DragType.HEAD_LINK.GetHashCode().ToString());

                //HtmlGenericControl icon = new HtmlGenericControl("span");
                //icon.Attributes.Add("class", "icon-link fieldIcon");
                //li.Controls.Add(icon);
                //HtmlGenericControl span = new HtmlGenericControl("span");
                //span.InnerText = "Société"; 
                //li.Controls.Add(span);

                //ul.Controls.Add(li);

                //_panelContent.Controls.Add(ul);


                bool readOnly =
                    _tabInfos != null &&
                    !eAdminTools.IsUserAllowedForProduct(this.Pref, this.Pref.User, _tabInfos.ProductID);

                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7365), "buttonAdminRelations", onclick: "javascript:nsAdmin.confRelations(" + (readOnly ? "true" : "false") + ")");
                button.Generate(_panelContent);

                return true;
            }
            else
            {
                return false;
            }

        }
    }
}