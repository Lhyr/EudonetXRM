using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// permet d'obtenir un token pour les spécifs de la base
    /// </summary>
    public partial class eGetTokenSpecifXRM : eEudoPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            List<eSpecif> lst = eSpecif.GetSpecifList(_pref, Internal.eLibConst.SPECIF_TYPE.TYP_ALL);


            if (lst.Count > 0)
            {
                Response.Write("<table border=1>");
                Response.Write("<tr><td>Nom</td><td>Table</td><td>token</td></tr>");
                foreach (eSpecif spec in lst)
                {
                    Response.Write("<tr>");
                    Response.Write(String.Concat("<td>",spec.Label, "</td><td>", spec.Tab, "</td><td><textarea rows=10 cols=120>", eSpecifToken.GetSpecifTokenXRM(_pref, 4, spec.SpecifId,spec.Tab).Token, "</textarea></td>"));
                    Response.Write("</tr>");
                }
                Response.Write("</table>");
            }
        }

        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }
    }
}