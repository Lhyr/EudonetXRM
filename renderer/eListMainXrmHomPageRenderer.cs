using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de rendu pour la page de liste des pages d'accueil XRM
    /// </summary>
    public class eListMainXrmHomPageRenderer : eActionListRenderer
    {
        /// <summary>
        /// Constructeur privé du renderer
        /// Pour obtenir une instance de l'objet, utiliser GetXrmHomPageListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nPage">Num page</param>
        /// <param name="nRows">Nombre de lignes</param>
        /// <param name="width">Largeur</param>
        /// <param name="height">Hauteur</param>
        private eListMainXrmHomPageRenderer(ePref pref, Int32 nPage, Int32 nRows, Int32 width, Int32 height)
            : base(pref)
        {

            _width = width;
            _height = height;
            _page = nPage;
            _rows = nRows;
            _tab = (int)TableType.XRMHOMEPAGE;

            _sizeTdCheckBox = 150;

        }

        ///// <summary>
        /////  Pas de menu
        ///// </summary>
        ///// <param name="pMainListContent"></param>
        // protected override void BuildTopMenu(Panel pMainListContent) { }
        // protected override void BuildDivInfos(Panel container) { }

        /// <summary>
        /// CRéer un nouveau mode liste des pages d'accueil XRM
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nPage">Num page</param>
        /// <param name="nRows">Nombre de lignes</param>
        /// <param name="width">Largeur</param>
        /// <param name="height">Hauteur</param>
        /// <returns></returns>
        public static eListMainRenderer CreateXrmHomPageListRenderer(ePref pref, Int32 nPage, Int32 nRows, Int32 width, Int32 height)
        {
            return new eListMainXrmHomPageRenderer(pref, nPage, nRows, width, height);
        }


        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected override void GenerateList()
        {
            _list = eListFactory.CreateMainList(Pref, _tab, _page, _rows, true, _bFullList);

            if (!String.IsNullOrEmpty(_list.ErrorMsg))
                SetError(_list.ErrorType, _list.ErrorMsg, _list.InnerException);
        }

        private int NbRecords()
        {
            if (_list.ListRecords == null)
                return 0;

            return _list.ListRecords.Count;
        }
        /*
        protected override bool Build()
        {
            //_pgContainer.ID = "listheader";

            // Nombre de page d'accueil XRM
            Panel pnlSubTitle = new Panel();
            pnlSubTitle.CssClass = "adminCntntTtl adminCntntTtlHomepages";
            pnlSubTitle.Controls.Add(new LiteralControl(String.Concat(NbRecords(), " ", eResApp.GetRes(_ePref, 348))));
            _pgContainer.Controls.Add(pnlSubTitle);

            // Champ de recherche
            Panel eFS = eAdminTools.CreateSearchBar("eFSContainerAdminHomepages", "mt_" + ((int)TableType.XRM_HOME_PAGE).ToString());
            _pgContainer.Controls.Add(eFS);

            
            return base.Build();
        }
        */


        /// <summary>
        /// Indique si le champ de recherche doit être affiché
        /// </summary>
        public override Boolean DrawSearchField
        {
            get
            {
                return true;
            }
        }



        /// <summary>
        /// Ajoute les specifités sur la cell en fonction du rendu
        /// </summary>
        /// <param name="row">record</param>
        /// <param name="trRow">Objet tr courant</param>
        /// <param name="fieldRow">field record</param>
        /// <param name="trCell">Objet cellule courant</param>
        /// <param name="idxCell">index de la colonne</param>
        protected override void CustomTableCell(eRecord row, TableRow trRow, eFieldRecord fieldRow, TableCell trCell, Int32 idxCell)
        {
            base.CustomTableCell(row, trRow, fieldRow, trCell, idxCell);

            if (fieldRow.FldInfo.Descid == (int)XrmHomePageField.UserAssign || fieldRow.FldInfo.Descid == (int)XrmHomePageField.GroupAssign)
            {
                bool bUsrAssign = fieldRow.FldInfo.Descid == (int)XrmHomePageField.UserAssign;
                trCell.Attributes.Add("onclick", "oGridController.page.assign(this," + row.MainFileid.ToString() + ", " + (bUsrAssign ? "'user'" : "'group'") + ");");
                trCell.Attributes.Add("fmt", ((int)fieldRow.FldInfo.Format).ToString());

                if (string.IsNullOrEmpty(fieldRow.Value))
                    trCell.Text = "_";
            }
        }

        /// <summary>
        /// Vérifie les droits de traitement
        /// </summary>
        protected override void InitDrawButtonsAction()
        {
            // base.InitDrawButtonsAction();

            _drawBtnEdit = false;
            _drawBtnDuplicate = false; // TODO
            _drawBtnRename = false;
            _drawBtnDelete = true;
            _drawBtnTooltip = false;
        }

        /// <summary>
        /// Ajout des attributs specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("action", "delete");
        }
        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row)
        {
            // webCtrl.Attributes.Add("action", "duplicate");
        }


        /// <summary>
        /// Pas de btn edit
        /// </summary>
        /// <param name="webCtrl"></param>
        /// <param name="row"></param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row)
        {

        }

        /// <summary>
        /// Pas de btn rename
        /// </summary>
        /// <param name="webCtrl"></param>
        /// <param name="sElementValueId"></param>
        /// <param name="row"></param>
        protected override void BtnActionRename(WebControl webCtrl, string sElementValueId, eRecord row)
        {

        }

        /// <summary>
        /// Pas de tooltip
        /// </summary>
        /// <param name="webCtrl"></param>
        /// <param name="row"></param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row)
        {

        }

        /// <summary>
        /// Pas d'icone liste
        /// </summary>
        /// <param name="row"></param>
        /// <param name="trDataRow"></param>
        /// <param name="idxLine"></param>
        /// <param name="sLstRulesCss"></param>
        /// <param name="lIcon"></param>
        //protected override void BodyListIcon(eRecord row, TableRow trDataRow, int idxLine, ref string sLstRulesCss, List<string> lIcon)
        //{

        //}

        /// <summary>
        /// Pas de colonne pour les icon liste
        /// </summary>
        /// <param name="headerRowicon"></param>
        protected override void AddIconHead(TableRow headerRowicon)
        {

        }
        /// <summary>
        /// Ajoute les Icones d'ouverture en début de ligne
        /// </summary>
        /// <param name="rowIcon">Enregistrement</param>
        /// <param name="bodyRowicon">Ligne à modifier</param>
        /// <param name="cssIcon">Classe CSS d'origine</param>
        /// <param name="idxLine">index de la ligne</param>
        protected override void AddIconBody(eRecord rowIcon, TableRow bodyRowicon, String cssIcon, Int32 idxLine)
        {

        }

        /// <summary>
        /// méthode à override pour décider s'il faut ou non afficher les cases à cocher
        /// </summary>
        /// <returns></returns>
        protected override bool DisplayCheckBox()
        {
            return true;
        }

        /// <summary>
        /// Ajoute dans le rang de donnée la check box permettant d'effectuer une selection
        /// </summary>
        /// <param name="row">Objet eRecord de la ligne en cours</param>
        /// <param name="trDataRow"></param>
        /// <param name="sAltLineCss"></param>
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, string sAltLineCss)
        {
            int hpId = row.MainFileid;

            TableCell cellSelect = new TableCell();
            cellSelect.CssClass = String.Concat(sAltLineCss, " icon");

            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(this._ePref.DefaultXrmHomePageId == hpId, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 1870);
            chkSelect.AddClass("chkAction chkDefaultHP");

            chkSelect.AddClick("nsAdmin.updateDefaultHomepage(this)");

            chkSelect.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.CONFIGADV, "|", (int)eLibConst.CONFIGADV.DEFAULT_HOMEPAGE_ID, "|", (int)eLibConst.CONFIGADV_CATEGORY.SYSTEM));
            chkSelect.Attributes.Add("data-hpid", hpId.ToString());

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);

        }

        /// <summary>
        /// ajoute la cellule d'en tete
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {

            // Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            TableHeaderCell cellSelect = new TableHeaderCell();
            cellSelect.ID = "defaultHpChkCol";
            cellSelect.CssClass = "head chkCol";
            cellSelect.Attributes.Add("nomove", "1");
            cellSelect.Attributes.Add("width", String.Concat(_sizeTdCheckBox, "px"));
            cellSelect.Text = eResApp.GetRes(_ePref, 1870);

            headerRow.Cells.Add(cellSelect);
        }

    }
}