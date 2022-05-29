using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.renderer
{
    public class eListFilteredSelectionRenderer : eListMainRenderer
    {

        private eFilteredSelection _eFilteredSel;
        /// <summary>
        /// Objet eFilteredSelection du renderer
        /// </summary>
        public eFilteredSelection FilteredSel
        {
            get { return _eFilteredSel; }
        }

        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="pref"></param>
        private eListFilteredSelectionRenderer(ePref pref)
            : base(pref)
        {
            _rType = RENDERERTYPE.ListSelection;
        }

        /// <summary>
        /// Retourne un renderer eListRendererMain
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="list">objet liste dont il faut faire le rendu</param>        
        /// <param name="height">Hauteur du bloc de rendu</param>
        /// <param name="width">Largeur du bloc de rendu</param>
        /// <param name="nPage">Page</param>
        /// <param name="nRow">Nombdre de ligne par page</param>
        internal static eListFilteredSelectionRenderer GetFilteredSelectionListRenderer(ePref pref, int tabID, eList list, Int32 height, Int32 width, Int32 nPage, Int32 nRow)
        {

            // Instanciation
            eListFilteredSelectionRenderer elRenderer = new eListFilteredSelectionRenderer(pref);

            // Initialistion
            elRenderer._list = list;
            elRenderer._height = height;
            elRenderer._width = width;
            elRenderer._page = nPage;
            elRenderer._rows = nRow;
            elRenderer._rowsCalculated = nRow;
            elRenderer._tab = list.CalledTabDescId;

            elRenderer._eFilteredSel = eFilteredSelection.GetFilteredSelection(pref, tabID, ((eListFilteredSelection)list).CalledTabDescId);

            return elRenderer;

        }


        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes type ++ sont toujours paginées
        /// </summary>
        protected override void SetPagingInfo()
        {
            _tblMainList.Attributes.Add("eNbCnt", _list.NbTotalRows.ToString());
            _tblMainList.Attributes.Add("eNbTotal", _list.NbTotalRows.ToString());

            _tblMainList.Attributes.Add("eHasCount", "1");
            _tblMainList.Attributes.Add("nbPage", _list.NbPage.ToString());
            _tblMainList.Attributes.Add("cnton", "1");
        }

        /// <summary>
        ///La liste est générée à part
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {

        }

        /// <summary>
        /// on met la valeur sel dans l'attribut ednmode
        /// on ajoute le tab du template
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            bool bReturn = base.End();

            _tblMainList.Attributes.Add("ednmode", "selection");

            //Fiche cochées
            HtmlInputHidden checkedFiles = new HtmlInputHidden();
            checkedFiles.ID = String.Concat("checked");

            _divHidden.Controls.Add(checkedFiles);

            _pgContainer.CssClass = "tabeul dest";
            _pgContainer.ID = "mainDiv";

            // #33 598 - Redimensionnement de la liste des destinataires ++
            // On affecte à la liste la hauteur passée par le JavaScript, qui a pu, entretemps, être ajustée par eModalDialog
            // si la fenêtre a été réduite lorsqu'on se trouve en basse résolution
            // cf. corrections faites sur eModalDialog dans le cadre des demandes #33 098, #32 972, #33 601, #33 620
            int itemListHeight = this._height;
            _pgContainer.Style.Add(HtmlTextWriterStyle.Height, String.Concat(itemListHeight, "px"));

            return bReturn;
        }


        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row">Objet eRecord de la ligne en cours</param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, String sAltLineCss)
        {
            TableCell cellSelect = new TableCell();
            cellSelect.CssClass = String.Concat(sAltLineCss, " icon");

            //Boolean bChecked = ((eListFilteredSelection)_list)..Contains(sHash);
            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(false, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");


            Int32 nADRID = row.MainFileid;
            //chkSelect.AddClick(String.Concat("nsInvitWizard.chkInvitFile(this );"));


            chkSelect.Attributes.Add("name", "chkMF");

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);
        }



        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher avec un context menu
        /// </summary>
        /// <param name="headerRow"></param>
        /// <param name="iWidth">largeur utilisée par la cellule</param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {
            // Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            TableHeaderCell cellSelect = new TableHeaderCell();
            cellSelect.CssClass = "head icon";
            cellSelect.Attributes.Add("nomove", "1");
            cellSelect.Attributes.Add("width", String.Concat(_sizeTdCheckBox, "px"));


            eCheckBoxCtrl chkSelectAll = new eCheckBoxCtrl(_eFilteredSel.AllSelected, false);
            chkSelectAll.ID = String.Concat("chkAll_", VirtualMainTableDescId);

            chkSelectAll.ToolTipChkBox = eResApp.GetRes(Pref, 530);
            chkSelectAll.AddClass("chkAction");
            chkSelectAll.AddClick("nsSelectionWizard.getContextMenu(this);");
            chkSelectAll.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cellSelect.Controls.Add(chkSelectAll);

            headerRow.Cells.Add(cellSelect);
        }
    }
}