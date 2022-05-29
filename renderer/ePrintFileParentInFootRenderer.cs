using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class ePrintFileParentInFootRenderer : eFileParentInFootRenderer
    {
        public ePrintFileParentInFootRenderer(ePref pref, eMainFileRenderer efRdr)
            : base(pref, efRdr)
        {
        }

        public ePrintFileParentInFootRenderer(ePref pref, eFile ef)
            : base(pref, ef)
        {

        }

        /// <summary>
        /// remplit le web control avec le contenu souhaité
        /// </summary>
        protected override void GetHTMLMemoControl(EdnWebControl ednWebCtrl, String sValue)
        {
            // [BUG XRM] Impression fiche avec champ Mémo HTML - #49286
            sValue = HtmlTools.StripHtml(sValue);

            WebControl webCtrl = ednWebCtrl.WebCtrl;

            HtmlGenericControl div = new HtmlGenericControl("div");
            div.InnerHtml = sValue;
            webCtrl.Controls.Add(div);
        }
    }
}