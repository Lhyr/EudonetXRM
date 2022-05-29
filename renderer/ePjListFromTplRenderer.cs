using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class ePjListFromTplRenderer : ePjListRenderer
    {
        private ePjListFromTpl _pjListFromTpl;

        /// <summary>Indique si on affiche tout ou uniquement les PJ sélectionnées </summary>
        public string ViewType = "all";

        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="pj"></param>
        public ePjListFromTplRenderer(ePref pref, ePJToAdd pj)
            : base(pref, pj)
        {
            _rType = RENDERERTYPE.ListPjFromTpl;
        }

        protected override void GenerateList()
        {
            if (_list == null)
                _list = eListFactory.CreatePjList(Pref, Attachment, InitialPjIds);
            _pjListFromTpl = (ePjListFromTpl)_list;
        }
        /// <summary>
        /// ajoute la cellule d'en tete contenant la case à cocher "selectionner tout"
        /// </summary>
        /// <param name="headerRow"></param>
        protected override void AddSelectCheckBoxHead(TableRow headerRow)
        {
            bool bDisabled = false;
            // TOCHECK SMS
            if ((Attachment.ParentTab.EdnType == EdnType.FILE_MAIL || Attachment.ParentTab.EdnType == EdnType.FILE_SMS) && Attachment.FileId > 0 && Attachment.IsMailFixed)
            {
                bDisabled = true;
            }

            // Case a cocher marked file pour le mode liste
            // Ajout de l'entête de la case à côcher de sélection
            TableHeaderCell cellSelect = new TableHeaderCell();
            cellSelect.CssClass = "head icon";
            cellSelect.Attributes.Add("nomove", "1");
            cellSelect.Attributes.Add("width", string.Concat(_sizeTdCheckBox, "px"));

            eCheckBoxCtrl chkSelectAll = new eCheckBoxCtrl(_pjListFromTpl.DoCheckAll, bDisabled);
            chkSelectAll.ID = string.Concat("chkAll_", VirtualMainTableDescId);

            chkSelectAll.ToolTipChkBox = eResApp.GetRes(Pref, 6302);
            chkSelectAll.AddClass("chkAction");
            chkSelectAll.AddClick("selectAllPj(this);");
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
        protected override void AddSelectCheckBox(eRecord row, TableRow trDataRow, string sAltLineCss)
        {
            TableCell cellSelect = new TableCell();
            cellSelect.CssClass = string.Concat(sAltLineCss, " icon");
            bool bDisabled = false;


            if ((Attachment.ParentTab.EdnType == EdnType.FILE_MAIL || Attachment.ParentTab.EdnType == EdnType.FILE_SMS) && Attachment.FileId > 0 && Attachment.IsMailFixed)
            {
                bDisabled = true;
            }
            else
            {
                eFieldRecord fldTable = row.GetFieldByAlias(string.Concat(TableType.PJ.GetHashCode(), "_", PJField.FILE.GetHashCode()));
                if (fldTable != null)
                {
                    bDisabled = fldTable.Value == Attachment.ParentTab.TabName;
                }
            }
            eCheckBoxCtrl chkSelect = new eCheckBoxCtrl(row.IsMarked, bDisabled);

            chkSelect.ToolTipChkBox = eResApp.GetRes(Pref, 293);
            chkSelect.AddClass("chkAction");
            chkSelect.AddClick("selPj(this);");

            cellSelect.Controls.Add(chkSelect);
            trDataRow.Cells.Add(cellSelect);
        }

        /// <summary>
        /// Si on rencontre une fiche rattachée au template ET à un parent,
        /// on n'affiche pas la ligne pour le template
        /// on coche la pj pour le parent.
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        protected override bool HasRight(eRecord row)
        {
            if (ViewType == "checkedonly" && !row.IsMarked)
                return false;

            eFieldRecord fldLibelle = row.GetFieldByAlias(string.Concat(TableType.PJ.GetHashCode(), "_", PJField.LIBELLE.GetHashCode()));
            eFieldRecord fldTab = row.GetFieldByAlias(string.Concat(TableType.PJ.GetHashCode(), "_", PJField.FILE.GetHashCode()));
            if (fldLibelle == null || fldTab == null)
                return true;
            string sLibelle = fldLibelle.Value;

            if (Attachment.ParentTab.TabName == fldTab.Value && _pjListFromTpl.IsInParent(sLibelle))
                return false;

            return true;

        }

        /// <summary>
        /// Retourne une instance de eFilterReportListRenderer
        /// </summary>
        /// <param name="pref">Préférence de l'utilisateur en cours</param>
        /// <param name="nFileId">FileId de la fiche</param>
        /// <param name="nTab">DescId de la fiche template parente</param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static ePjListFromTplRenderer GetPjListFromTplRenderer(ePref pref, ePJToAdd attachment, int height, int width)
        {
            ePjListFromTplRenderer myRenderer = new ePjListFromTplRenderer(pref, attachment);
            myRenderer._height = height;
            myRenderer._width = width;
            myRenderer._tab = attachment.Tab;
            myRenderer._nFileId = attachment.FileId;

            // Droits de traitements d'ajout d'annexes sur l'onglet
            myRenderer.RightManager = new eRightAttachment(pref, attachment.Tab);

            return myRenderer;
        }

        /// <summary>
        /// méthode à override pour décider s'il faut ou non afficher les cases à cocher
        /// </summary>
        /// <returns></returns>
        protected override bool DisplayCheckBox()
        {
            return true;
        }




    }
}