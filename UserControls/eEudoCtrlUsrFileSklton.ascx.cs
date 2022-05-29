using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Com.Eudonet.Xrm.UserControls
{
    public partial class eEudoCtrlUsrFileSklton : System.Web.UI.UserControl
    {
        /// <summary>
        /// INitialise les tableaux Header de la fiche du skeleton.
        /// </summary>
        /// <param name="tbl"></param>
        private void initTableHeaderFooter(Table tbl)
        {
            TableRow tr = new TableRow();

            tr.Cells.AddRange(Enumerable.Range(1, 4).Select(rng =>
            {
                TableCell td = new TableCell();

                if (tbl.ID != "tblFooter")
                    td.CssClass = "col1";

                if (rng == 1 && tbl.ID == "tblHeader1")
                    td.CssClass = "col2";

                if ((rng == 2 && tbl.ID == "tblHeader1")
                    || (rng != 1 && tbl.ID == "tblHeader2"))
                    td.CssClass = "white";

                if (rng == 4 && tbl.ID == "tblFooter")
                    td.CssClass = "white col8";

                td.Controls.Add(new Label());

                return td;

            }).ToArray());

            tbl.Rows.Add(tr);
        }

        /// <summary>
        /// Initialise le skeleton pour les stepper.
        /// </summary>
        /// <param name="tbl"></param>
        private void initTableStepper(Table tbl)
        {
            tbl.Rows.AddRange(Enumerable.Range(1, 2).Select(rngTr => {
                TableRow tr = new TableRow();

                tr.Cells.AddRange(Enumerable.Range(1, 2).Select(rngTd =>
                {

                    TableCell td = new TableCell();

                    if (rngTr == 2 && rngTd > 1)
                    {
                        td.CssClass = "white";
                    }

                    td.Controls.Add(new Label());

                    return td;

                }).ToArray());

                return tr;

            }).ToArray());

        }
        /// <summary>
        /// Initialise le skeleton pour les stepper.
        /// </summary>
        /// <param name="tbl"></param>
        private void initTableStepperBig(Table tbl)
        {
            TableRow tr = new TableRow();

            tr.Cells.AddRange(Enumerable.Range(1, 13).Select(rng =>
            {
                TableCell td = new TableCell();
                td.Controls.Add(new Label());

                td.CssClass = "circle";

                if (rng == 1 || rng > 12)
                    td.CssClass = "white";
                else if (rng % 2 == 0)
                {
                    td.CssClass = "circle";
                    td.Controls.Add(new Panel());
                }
                else
                    td.CssClass = "col1 sub-temp";


                return td;

            }).ToArray());

            tbl.Rows.Add(tr);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tbl"></param>
        private void initTableList(Table tbl)
        {

            tbl.Rows.AddRange(Enumerable.Range(1, 8).Select(i =>
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
        /// A l'initialisation du controle ajoute le skeleton des fiches.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            initTableHeaderFooter(tblHeader1);
            initTableHeaderFooter(tblHeader2);
            initTableStepperBig(tblStepper1);
            initTableStepper(tblStepper2);
            initTableHeaderFooter(tblFooter);
            initTableList(tblListSklton);
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