using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eBkmDiscussionRenderer : eBookmarkRenderer
    {
        Panel _pnMainDiv = new Panel();


        /// <summary>
        /// Création d'un BookmarkRenderer à partir d'un eBookmark chargé
        /// </summary>
        /// <param name="ePref"></param>
        /// <param name="eBkm"></param>
        /// <param name="bDisplayAll"></param>
        /// <param name="bPrintMode"></param>
        public eBkmDiscussionRenderer(ePref ePref, eBookmark eBkm, Boolean bDisplayAll, Boolean bPrintMode)
            : base(ePref, eBkm, bDisplayAll, bPrintMode)
        {

        }

        /// <summary>
        /// build de la classe parente
        /// </summary>
        /// <returns></returns>
        protected override bool BaseBuild()
        {

            insertMemo();

            _pgContainer.Attributes.Add("did", _bkm.DiscCustomFields.NotesDescId.ToString());
            _pgContainer.Attributes.Add("dtdid", _bkm.DiscCustomFields.DateDescId.ToString());
            _pgContainer.Attributes.Add("usrdid", _bkm.DiscCustomFields.UserDescId.ToString());

            try
            {
                _pnMainDiv.ID = String.Concat("mt_", VirtualMainTableDescId);
                SetPagingInfo();
                foreach (eRecord rec in _bkm.ListRecords)
                {
                    _pnMainDiv.Controls.Add(eBkmDiscCommRenderer.GetComm(Pref, rec, _bkm.DiscCustomFields));
                }
                _pgContainer.Controls.Add(_pnMainDiv);
            }
            catch (Exception e)
            {
                _sErrorMsg = String.Concat(e.Message, e.StackTrace);
                return false;
            }
            return true;
        }

        /// <summary>
        /// identifie les paramètres de pagination
        /// </summary>
        protected override void SetPagingInfo()
        {
            //Information de paging
            _pnMainDiv.Attributes.Add("cPage", _page.ToString());
            _pnMainDiv.Attributes.Add("top", ((_page - 1) * _rows).ToString());
            _pnMainDiv.Attributes.Add("bof", _page <= 1 ? "1" : "0");

            _pnMainDiv.Attributes.Add("eNbCnt", eNumber.FormatNumber(Pref, _list.NbTotalRows, 0, true));
            _pnMainDiv.Attributes.Add("eNbTotal", eNumber.FormatNumber(Pref, _list.NbTotalRows, 0, true));
            _pnMainDiv.Attributes.Add("eHasCount", "1");

            //Compteur appararent
            if (_list.GetParam<Boolean>("PagingEnabled") || !_list.GetParam<Boolean>("CountOnDemand"))
            {
                _pnMainDiv.Attributes.Add("cnton", "1");
                _pnMainDiv.Attributes.Add("nbPage", _list.NbPage.ToString());
            }
            else
                _pnMainDiv.Attributes.Add("cnton", "0");

        }



        /// <summary>
        /// Les instructions de cette méthodes ne sont pas nécessaires à ce cas
        /// Voir plus tard si des ajustements sont nécessaires
        /// </summary>
        protected override bool End()
        {
            return true;
        }

        private void insertMemo()
        {
            //return;
            if (!_bkm.IsAddAllowed)
                return;


            eFile file = eFileLite.CreateFileLite(Pref, _bkm.ViewTabDescId, 0);
            eFieldRecord fldNotes = file.Record.GetFieldByAlias(String.Concat(_bkm.ViewMainTable.DescId, "_", _bkm.DiscCustomFields.NotesDescId));
            if (fldNotes == null)
                return;


            #region équivalent du FillContainer
            Panel divmt = new Panel();
            divmt.ID = String.Concat("div", _bkm.DiscCustomFields.NotesDescId);
            divmt.CssClass = "divmTab";
            _pgContainer.Controls.Add(divmt);

            String mainTableId = String.Concat("mt_", _bkm.DiscCustomFields.NotesDescId);

            // Div de champ caché
            HtmlGenericControl divHidden = new HtmlGenericControl("div");
            divHidden.Style.Add("visibility", "hidden");
            divHidden.Style.Add("display", "none");
            divHidden.ID = String.Concat("hv_", _bkm.DiscCustomFields.NotesDescId);

            //CSS ICON STANDARD
            // MAB - TODO
            String sCSSStdIcon = String.Empty;
            //sCSSStdIcon = String.Concat("background:url(themes/", Pref.Theme, "/images/iFileIcon/", _list.ViewMainTable.GetIcon, ") center center no-repeat  !important ");
            HtmlInputHidden inptDefIconCss = new HtmlInputHidden();
            inptDefIconCss.ID = "ICON_DEF_" + _bkm.DiscCustomFields.NotesDescId;
            inptDefIconCss.Attributes.Add("etype", "css");
            inptDefIconCss.Attributes.Add("ecssname", String.Concat("iconDef_", _tab));
            inptDefIconCss.Attributes.Add("ecssclass", sCSSStdIcon);
            divHidden.Controls.Add(inptDefIconCss);

            // MIN_COL_WIDTH - TODO - A SUP
            HtmlInputHidden inputMinColWidth = new HtmlInputHidden();
            inputMinColWidth.ID = "minColWidth";
            inputMinColWidth.Value = eConst.MIN_COL_WIDTH.ToString();
            divHidden.Controls.Add(inputMinColWidth);

            //Ajout du div caché
            _pgContainer.Controls.Add(divHidden);

            #endregion

            #region equivalent Body
            HtmlTextArea memoEditorValueControl = new HtmlTextArea();
            memoEditorValueControl.ID = String.Concat("eBkmMemoEditorValue_", _bkm.DiscCustomFields.NotesDescId);
            memoEditorValueControl.Style.Add("display", "none");
            memoEditorValueControl.InnerText = fldNotes.Value; // #36751 CRU/SPH : On fait un InnerText à la place d'un InnerHTML pour ne pas décoder la valeur


            HtmlGenericControl memoEditorContainerControl = new HtmlGenericControl("div");
            memoEditorContainerControl.ID = String.Concat("eBkmMemoEditorContainer_", _bkm.DiscCustomFields.NotesDescId);
            // Ajout des attributs pour le fonctionnement de eUpdater/eEngine depuis eMemoEditor.update()
            memoEditorContainerControl.Attributes.Add("ename", String.Concat("eBkmMemoEditorContainer_", _bkm.DiscCustomFields.NotesDescId));
            memoEditorContainerControl.Attributes.Add("did", _bkm.DiscCustomFields.NotesDescId.ToString());
            memoEditorContainerControl.Attributes.Add("fid", "0");
            memoEditorContainerControl.Attributes.Add("html", (fldNotes.FldInfo.IsHtml ? "1" : "0"));
            memoEditorContainerControl.Attributes.Add("frominnertext", "1"); //en fonction du mode d'ouverture (popup, fiche...), l'init du ck editor est différente. 
            if (!fldNotes.FldInfo.IsHtml)
                memoEditorContainerControl.Attributes.Add("nbrows", fldNotes.FldInfo.PosRowSpan.ToString()); // #37575 CRU : Ajout info nombre lignes paramétré
            if (fldNotes.FldInfo.ObligatReadOnly || fldNotes.FldInfo.ReadOnly || !fldNotes.RightIsUpdatable)
            {
                memoEditorContainerControl.Attributes.Add("readonly", "readonly");
                memoEditorContainerControl.Attributes.Add("ero", "1");
            }
            // Ajout des contrôles au conteneur de signet
            _pgContainer.Controls.Add(memoEditorValueControl);
            _pgContainer.Controls.Add(memoEditorContainerControl);
            _pgContainer.Attributes.Add("disc", "1");

            // L'exécution du JS instanciant eMemoEditor sera prise en charge par updateBkmList()

            #endregion

            WebControl memoNew = GetFieldValueCell(file.Record, fldNotes, 0, Pref);

            _pgContainer.Controls.Add(memoNew);

        }

    }
}