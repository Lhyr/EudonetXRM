using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm
{
    public partial class testControls : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Exemple de UL LI
            eUlCtrl ctrlUl = new eUlCtrl();
            ctrlUl.CssClass = "classUL";
            ctrlUl.ID = "idUL";
            ctrlUl.Attributes.Add("ednTruc", "0");
            PANEL.Controls.Add(ctrlUl);

            eLiCtrl ctrlLI = ctrlUl.AddLi();
            ctrlLI.CssClass = "classLI";
            ctrlLI.ID = "idLI";
            ctrlLI.InnerText = "li 1";
            //ctrlLI.Controls.Add(new LiteralControl("li 1"));
            ctrlUl.Controls.Add(ctrlLI);

            ctrlLI = ctrlUl.AddLi();
            ctrlLI.Controls.Add(new LiteralControl("li 2"));
            ctrlUl.Controls.Add(ctrlLI);

            ctrlLI = ctrlUl.AddLi();
            ctrlLI.Controls.Add(new LiteralControl("li 3"));
            ctrlUl.Controls.Add(ctrlLI);
            
            //Exemple de contrôl générique
            eGenericWebControl webCtrlGen = new eGenericWebControl(HtmlTextWriterTag.P);
            webCtrlGen.Controls.Add(new LiteralControl("mon paragraphe"));
            PANEL.Controls.Add(webCtrlGen);
            

        }
    }
}