using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer pour la liste des sélections (ajout depuis un filtre)
    /// </summary>
    public class eListSelRenderer : eListMainRenderer
    {



        private eInvitSelection _eInvitSel;
        private eRes _res;

        /// <summary>
        /// Objet InvitSelection du renderer
        /// </summary>
        public eInvitSelection InvitSel
        {
            get { return _eInvitSel; }
        }

        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="pref"></param>
        private eListSelRenderer(ePref pref)
            : base(pref)
        {

            _rType = RENDERERTYPE.ListInvit;
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
        internal static eListSelRenderer GetSelListRenderer(ePref pref, eList list, Int32 height, Int32 width, Int32 nPage, Int32 nRow)
        {

            // Instanciation
            eListSelRenderer elRenderer = new eListSelRenderer(pref);

            // Initialistion
            elRenderer._list = list;
            elRenderer._height = height;
            elRenderer._width = width;
            elRenderer._page = nPage;
            elRenderer._rows = nRow;
            elRenderer._rowsCalculated = nRow;
            elRenderer._tab = list.CalledTabDescId;

            elRenderer._eInvitSel = eInvitSelection.GetInvitSelection(pref, ((eListSel)list).InvitationBkm);
            return elRenderer;

        }


        /// <summary>
        /// identifie les paramètres de pagination
        /// Les listes type ++ sont toujours paginées
        /// </summary>
        protected override void SetPagingInfo()
        {
            //Information de paging
            //KHA le 20/01/2014 : formatés par cette fonction, les résultats sont inutilisables par le javascript.
            //_tblMainList.Attributes.Add("eNbCnt", eTools.FormatNumber(_list.NbTotalRows, 0, true));
            //_tblMainList.Attributes.Add("eNbTotal", eTools.FormatNumber(_list.NbTotalRows, 0, true));
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
        /// Besoin de Res
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _res = new eRes(Pref, String.Concat(TableType.ADR.GetHashCode(), ",", ((eListSel)_list).ParentDescId));
            return base.Init();
        }

        /// <summary>
        /// on met la valeur sel dans l'attribut ednmode
        /// on ajoute le tab du template
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            bool bReturn = base.End();

            Boolean bAutoCreate = ((eListSel)_list).AutoSelectEnabled;
            Int32 nAutoCreateThreshold = ((eListSel)_list).AutoSelectValue;

            //Seuil de création automatique
            HtmlInputHidden autocreateThreshold = new HtmlInputHidden();
            autocreateThreshold.ID = String.Concat("threshold");
            autocreateThreshold.Name = String.Concat("threshold");
            autocreateThreshold.Attributes.Add("enabled", bAutoCreate ? "1" : "0");
            autocreateThreshold.Attributes.Add("autolaunch", ((_list.NbTotalRows > nAutoCreateThreshold) && bAutoCreate) ? "1" : "0");
            autocreateThreshold.Value = nAutoCreateThreshold.ToString();
            _divHidden.Controls.Add(autocreateThreshold);
            _tblMainList.Attributes.Add("ednmode", "sel");
            _tblMainList.Attributes.Add("ivttab", ((eListSel)_list).InvitationBkm.ToString());

            bool bNotFound;
            _tblMainList.Attributes.Add("adrlbl", _res.GetRes(TableType.ADR.GetHashCode(), out bNotFound));
            _tblMainList.Attributes.Add("evtlbl", _res.GetRes(((eListSel)_list).ParentDescId, out bNotFound));

            //Fiche cochées
            HtmlInputHidden checkedFiles = new HtmlInputHidden();
            checkedFiles.ID = String.Concat("checked");
            checkedFiles.Attributes.Add("nb", _eInvitSel.NbAll.ToString());
            checkedFiles.Attributes.Add("nbPP", _eInvitSel.NbContact.ToString());
            checkedFiles.Attributes.Add("nbAdr", _eInvitSel.NbAddress.ToString());

            _divHidden.Controls.Add(checkedFiles);

            _pgContainer.CssClass = "tabeul dest";

            // #33 598 - Redimensionnement de la liste des destinataires ++
            // On affecte à la liste la hauteur passée par le JavaScript, qui a pu, entretemps, être ajustée par eModalDialog
            // si la fenêtre a été réduite lorsqu'on se trouve en basse résolution
            // cf. corrections faites sur eModalDialog dans le cadre des demandes #33 098, #32 972, #33 601, #33 620
            int itemListHeight = this._height;
            _pgContainer.Style.Add(HtmlTextWriterStyle.Height, String.Concat(itemListHeight, "px"));

            return bReturn;
        }

        /// <summary>
        /// Ajoute les informations pour grisé les lignes si la ligne a un pp/adr en doublon
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trRow"></param>
        /// <param name="idxLine"></param>
        protected override void CustomTableRow(eRecord row, TableRow trRow, Int32 idxLine)
        {
            //Adresse déjà présente
            if (((eListSel)_list).LstAdrIdInPage.Contains(row.AdrId))
            {
                trRow.Attributes.Add("ednadrdbl", "1");
            }

            //PP déjà présente
            if (((eListSel)_list).LstPPIdInPage.Contains(row.PpId))
            {
                trRow.Attributes.Add("ednppdbl", "1");
            }

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

            String sHash = String.Concat(row.PpId.ToString(), "_", row.AdrId.ToString());
            Boolean bChecked = ((eListSel)_list).LstDbl.Contains(sHash);


            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(bChecked, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");


            Int32 nADRID = row.MainFileid;
            chkSelect.AddClick(String.Concat("nsInvitWizard.chkInvitFile(this );"));


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


            eCheckBoxCtrl chkSelectAll = new eCheckBoxCtrl(InvitSel.AllSelected, false);
            chkSelectAll.ID = String.Concat("chkAll_", VirtualMainTableDescId);

            chkSelectAll.ToolTipChkBox = eResApp.GetRes(Pref, 530);
            chkSelectAll.AddClass("chkAction");
            chkSelectAll.AddClick("nsInvitWizard.getContextMenu(this);");
            chkSelectAll.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cellSelect.Controls.Add(chkSelectAll);

            headerRow.Cells.Add(cellSelect);
        }

    }
}