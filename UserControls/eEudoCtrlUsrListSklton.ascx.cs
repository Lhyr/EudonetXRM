using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.UserControls
{
    public partial class eEudoCtrlUsrListSklton : System.Web.UI.UserControl
    {
        /// <summary>
        /// A l'initialisation du controle ajoute le skeleton des listes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            tblListSklton.Rows.AddRange(Enumerable.Range(1, 8).Select(i =>
            {
                TableRow tr = new TableRow();

                tr.Cells.AddRange(Enumerable.Range(1, 4).Select(j =>
                {
                    TableCell td = new TableCell();

                    td.CssClass = "col3";

                    if (j == 1)
                        td.CssClass = "col1";

                    td.Controls.Add(new Label());

                    return td;

                }).ToArray());

                return tr;

            }).ToArray());
        }

        /// <summary>
        /// AU chargement de la page.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}