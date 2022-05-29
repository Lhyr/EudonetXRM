using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPlanningFileRenderer : eAdminTemplateFileRenderer
    {
        public eAdminPlanningFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos)
        {
        }


        protected override void FillContent(List<eFieldRecord> sortedFields)
        {
            Int32 nbColByLine = _myFile.ViewMainTable.ColByLine;
            List<eFieldRecord> nonSysFields;
            Int32 nBreakLine = GetBreakLine();

            AddAdminDropWebTab();

            _backBoneRdr.PnFilePart1.Controls.Add(GetHeader());

            AddFilePropertiesBlock();

            int i = ((TableMain)_myFile.ViewMainTable).DateDescId;
            System.Web.UI.WebControls.Table fileTabSysFields = new System.Web.UI.WebControls.Table();
            System.Web.UI.WebControls.Table fileTabMain = new System.Web.UI.WebControls.Table();
            System.Web.UI.WebControls.Table fileTabInBkm = new System.Web.UI.WebControls.Table();

            //On recupere les champs systeme. On recupere le champs fin a part pour l inserer volontairement en 3eme position
            List<eFieldRecord> headerFields = _myFile.Record.GetFields.FindAll(
                x => x.FldInfo.Descid == _myFile.ViewMainTable.HistoDescId
                || x.FldInfo.Descid == _myFile.ViewMainTable.DateDescId
                || x.FldInfo.Descid % 100 == PlanningField.DESCID_ALERT.GetHashCode()
                || x.FldInfo.Descid % 100 == PlanningField.DESCID_SCHEDULE_ID.GetHashCode()
                || x.FldInfo.Descid % 100 == PlanningField.DESCID_CALENDAR_ITEM.GetHashCode());
            headerFields.Insert(2, _myFile.Record.GetFields.Find(x => x.FldInfo.Descid % 100 == PlanningField.DESCID_TPL_END_TIME.GetHashCode()));

            //On recalcule le disporder pour les champs admin
            int newPosDisporder = 1;
            foreach (eFieldRecord fld in headerFields)
            {
                fld.FldInfo.PosDisporder = newPosDisporder;
                newPosDisporder++;
            }

            //Creation partie systeme
            fileTabSysFields = CreateHtmlTable(headerFields, eFileLayout.NB_COL_HEAD_PLANNING, String.Empty, 0, true);
            fileTabSysFields.ID = "fts_" + _tab.ToString();

            _backBoneRdr.PnFilePart1.Controls.Add(fileTabSysFields);

            //On supprime les champs systeme
            sortedFields.RemoveAll(x => headerFields.Contains(x));
            nonSysFields = sortedFields;

            if (nonSysFields.Count > 0)
            {
                // ajout du cadre contenant les informations parentes
                AddParentHead();

                fileTabMain = CreateHtmlTable(sortedFields, nbColByLine, String.Concat("SEP_", _myFile.ViewMainTable.DescId, "_0"), 0);
                fileTabMain.ID = "ftm_" + _tab.ToString();

                // au dela de la break line, on affiche le reste du tableau en signet
                if (nBreakLine > 0 || _tab == TableType.CAMPAIGN.GetHashCode())
                    fileTabInBkm = SetHtmlTabInBkm(fileTabMain, nbColByLine, nBreakLine);

                _backBoneRdr.PnFilePart1.Controls.Add(fileTabMain);
            }

            HtmlInputHidden inptFileTabInBkm = new HtmlInputHidden();
            inptFileTabInBkm.ID = "bftbkm";
            inptFileTabInBkm.Value = _bFileTabInBkm ? "1" : "0";
            _divHidden.Controls.Add(inptFileTabInBkm);

            // Appel de fonction de fin de fill content
            EndFillContent();

            AddDropFieldsArea(fileTabInBkm != null && fileTabInBkm.Rows.Count > 0 ? fileTabInBkm : fileTabMain);

            //En mode créa et en affichage sous la forme de popup, 
            //le champ note est affiché dans le corps de la fiche, car il n'y a pas de signet
            AddMemoField();

            GetBookMarkBlock();
        }
    }
}