using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eFinderSelectionRenderer : eActionListRenderer
    {
        private string _sListCol = "";
        private List<Int32> _liSelIds = null;
        /// <summary>
        /// Constructeur du renderer
        /// </summary>
        /// <param name="pref"></param>
        private eFinderSelectionRenderer(ePref pref, Int32 nTab, String sListCol, List<Int32> liSelIds)
            : base(pref)
        {

            _rType = RENDERERTYPE.FinderSelection;
            _sListCol = sListCol;
            _liSelIds = liSelIds;
            _tab = nTab;
        }

        /// <summary>
        /// (redéfinit pour avoir le nombre de ligne totale à afficher car l'on affiche entre 0 et 200 ligne max)
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            // Pas de paging sur ces mode de liste
            _rows = _list.ListRecords.Count;

            return true;
        }

        protected override void InitDrawButtonsAction()
        {
            //base.InitDrawButtonsAction();
        }

        /// <summary>
        /// génère la liste
        /// </summary>
        protected override void GenerateList()
        {
            WhereCustom wherecustom;
            if (_liSelIds.Count > 0)
            {
                wherecustom = new WhereCustom("MAINID", Operator.OP_IN, eLibTools.Join(",", _liSelIds));
            }
            else
            {
                wherecustom = new WhereCustom("MAINID", Operator.OP_EQUAL, "0");
            }

            _list = eFinderSelection.CreateFinderSelection(Pref, _tab, _sListCol, wherecustom);
        }

        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// Dans ce cas les cases à cohcher ne sont rajoutée que pour rester homogène
        /// avec le tableau du dessus
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {
            // Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            TableHeaderCell cellSelect = new TableHeaderCell();
            cellSelect.CssClass = "head icon";
            cellSelect.Attributes.Add("nomove", "1");
            cellSelect.Attributes.Add("width", String.Concat(_sizeTdCheckBox, "px"));

            eCheckBoxCtrl chkSelectAll = new eCheckBoxCtrl(false, false);
            chkSelectAll.ID = String.Concat("chkAll_", VirtualMainTableDescId);

            chkSelectAll.ToolTipChkBox = eResApp.GetRes(Pref, 6302);
            chkSelectAll.AddClass("chkAction");
            chkSelectAll.AddClass("hidden");
            //chkSelectAll.AddClick("selectAllList(this);");
            chkSelectAll.Style.Add(HtmlTextWriterStyle.Height, "18px");
            cellSelect.Controls.Add(chkSelectAll);

            headerRow.Cells.Add(cellSelect);
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

            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(row.IsMarked, false);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");
            chkSelect.AddClass("hidden");


            //chkSelect.AddClick("chkMarkedFile(this);");


            chkSelect.Attributes.Add("name", "chkMF");

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);
        }


        /// <summary>
        /// renomme les controles pour ne pas perturber le javascript
        /// </summary>
        protected override void RenameControls()
        {

            _divmt.ID = String.Concat("divSel", VirtualMainTableDescId);
            _tblMainList.ID = String.Concat("mt_Sel", VirtualMainTableDescId);

        }


        #region Boutons actions

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton d'edition (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionEdit(WebControl webCtrl, eRecord row) { }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de duplication (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDuplicate(WebControl webCtrl, eRecord row) { }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de rename (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu</param>
        /// <param name="row">record</param>
        protected override void BtnActionRename(WebControl webCtrl, String sElementValueId, eRecord row) { }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de tooltip (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionTooltip(WebControl webCtrl, eRecord row) { }

        /// <summary>
        /// Ajout des attributes specifiques sur le bouton de supp (onclick, ...)
        /// </summary>
        /// <param name="webCtrl">Bouton action concerné</param>
        /// <param name="row">record</param>
        protected override void BtnActionDelete(WebControl webCtrl, eRecord row)
        {
            webCtrl.Attributes.Add("onclick", "removeFile('" + row.ViewTab.ToString() + "_" + row.MainFileid.ToString() + "');");
        }

        /// <summary>
        /// Retourne le control contenant toutes les actions de la ligne
        /// </summary>
        /// <param name="row">Ligne sur laquel on generé les actions</param>
        /// <param name="sElementValueId">Id de l'élément dont il faut modifier le contenu notament dans le cas du renommage</param>
        /// <returns>retourne le control</returns>
        protected override Control GetActionButton(eRecord row, String sElementValueId = "")
        {
            Panel div;
            Int32 nNbAction = 0;
            Boolean btnActionDel = false;

            Panel divAction = new Panel();
            divAction.CssClass = GetActionCssClass(); ;

            //Dernier filtre non sauvegardé
            Boolean bIsLastUnsavedFilter = (row.GetFields[0].Value.Length == 0);



            div = new Panel();
            div.CssClass = "icon-delete";
            div.ToolTip = eResApp.GetRes(Pref, 19);
            BtnActionDelete(div, row);
            divAction.Controls.Add(div);
            nNbAction++;
            btnActionDel = true;

            Int32 width = nNbAction * 24;
            if (btnActionDel)       // Le margin du btn sup
                width += 8;


            if (nNbAction > _nNbMaxAction)
                _nNbMaxAction = nNbAction;

            if (width > 0)
            {
                divAction.Style.Add("width", width.ToString() + "px");
                return divAction;
            }
            else
                return null;
        }

        /// <summary>
        /// retourne la classe css adéquate pour encapsuler les boutons d'actions
        /// </summary>
        /// <returns></returns>
        protected override string GetActionCssClass()
        {
            return "logo_modifs";
            // return "logo_modifs w";
        }

        #endregion

        protected override bool End()
        {
            PgContainer.CssClass = "tabeulmulti";
            PgContainer.ID = "selContent";
            return base.End();
        }


        protected override void CustomTableRow(eRecord row, TableRow trRow, int idxLine)
        {
            base.CustomTableRow(row, trRow, idxLine);

            eFieldRecord fldMainLabel = row.GetFieldByAlias(String.Concat(_tab, "_", _tab + 1));
            trRow.Attributes.Add("label", _tab == TableType.PP.GetHashCode() ? fldMainLabel.DisplayValuePPName : fldMainLabel.DisplayValue);
        }

        /// <summary>
        /// Fonction statique permettant de générer le renderer
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="sListCol"></param>
        /// <param name="liSelIds"></param>
        /// <returns></returns>
        public static eFinderSelectionRenderer GetFinderSelectionRenderer(ePref pref, Int32 nTab, String sListCol, List<Int32> liSelIds)
        {
            eFinderSelectionRenderer rdr = new eFinderSelectionRenderer(pref, nTab, sListCol, liSelIds);
            return rdr;

        }


    }
}