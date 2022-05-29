using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Internal;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de fichier de type SMS pour l'admin - #67 326
    /// </summary>
    public class eAdminSMSFileRenderer : eAdminMailFileRenderer
    {     
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        public eAdminSMSFileRenderer(ePref pref, eAdminTableInfos tabInfos) : base(pref, tabInfos) {}

        /// <summary>
        /// Pas de barre de signets
        /// </summary>
        protected override void GetBookMarkBlock() { }

        /// <summary>
        /// Pas de barre d'outils (indiquant notamment le nombre d'emplacements libres disponibles)
        /// </summary>
        /// <returns></returns>
        protected override Panel GetToolBar() { return new Panel(); }

        /// <summary>
        /// Pas de la règle graduée pour régler la largeur des champs, ces derniers étant dimensionnés en dur lors du rendu d'un fichier de type SMS
        /// </summary>
        protected override void AddDrawingScale() { }

        /// <summary>
        /// Pas de zone de dépôt de nouveaux champs, les champs proposés par les fichiers SMS étant figés
        /// </summary>
        /// <param name="maTable">La table de destination</param>
        protected override void AddDropFieldsArea(System.Web.UI.WebControls.Table maTable) { }

        /// <summary>
        /// Pas de champ Mémo "Notes" système
        /// </summary>
        protected override void AddMemoField() { }

        /// <summary>
        /// On supprime toutes les interlignes et espaces vides, pour ne conserver que les champs autorisés
        /// </summary>
        protected override void AddTableRow(System.Web.UI.WebControls.Table maTable, TableRow myTr, int nbMaxCols, int y, bool bLineNotEmpty, bool bHasPageSep, bool bHasSep, bool bDoNotAddTR) {
            int cellIndex = 0;
            while (cellIndex < myTr.Cells.Count)
            {
                TableCell tc = myTr.Cells[cellIndex];
                tc.ColumnSpan = 0;

                if (tc.CssClass.Contains("free"))
                    myTr.Cells.RemoveAt(cellIndex);
                else
                    cellIndex++;
            }

            myTr.Attributes.Add("data-nbline", _numLine.ToString());
            if (myTr.Cells.Count > 0)
                maTable.Rows.Add(myTr);

            _numLine++;
        }

        /// <summary>
        /// Filtrage et tri de la liste des rubriques de la fiche en cours - Seuls A et Corps sont autorisés pour les SMS
        /// </summary>
        protected override void SortFields(out List<eFieldRecord> sortedFields)
        {
            base.SortFields(out sortedFields);

            // Suppression des champs non désirés sur les fichiers de type SMS en admin
            // Leur suppression à la source peut avoir des effets de bords, donc on les retire après construction du tableau
            // comme le fait eMasterFileRenderer
            sortedFields.RemoveAll(
               delegate (eFieldRecord fld)
               {
                   return
                       fld.FldInfo.Descid != fld.FldInfo.Table.DescId + (int)MailField.DESCID_MAIL_TO 
                    && fld.FldInfo.Descid != fld.FldInfo.Table.DescId + (int)MailField.DESCID_MAIL_HTML
                   // && fld.FldInfo.Descid != fld.FldInfo.Table.DescId + (int)MailField.DESCID_SMS_STATUS
                    ;
               });

            // On force le texte brut sur les champs Mémo pour les SMS
            sortedFields.FindAll(f => f.FldInfo.Format == FieldFormat.TYP_MEMO);
        }
    }
}