using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Classe d'export de la liste des droits
    /// </summary>
    public class eAdminRightsExport
    {
        HttpContext _context;
        ePref _pref;
        eAdminRightsFilters _filters;

        private eAdminRightsExport(HttpContext context, ePref pref, eAdminRightsFilters filters)
        {
            _context = context;
            _pref = pref;
            _filters = filters;
        }



        List<IAdminTreatmentRight> GetList()
        {
            eAdminDescTreatmentRightCollection oRightsList = new eAdminDescTreatmentRightCollection(_pref);
            oRightsList.Tab = _filters.Tab;
            oRightsList.TreatTypes = _filters.TreatTypes;
            oRightsList.From = _filters.From;
            oRightsList.Field = _filters.Field;
            oRightsList.Function = _filters.Function;
            oRightsList.LoadTreamentsList();

            return oRightsList.RightsList;
        }



        void CreateWorkbook()
        {
            IAdminTreatmentRight right;

            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = excelEngine.Excel;
            IWorkbook workbook = application.Workbooks.Create(1);
            application.DefaultVersion = eTools.GetOfficeVersion(_pref);
            IWorksheet worksheet = workbook.Worksheets[0];

            List<IAdminTreatmentRight> list = GetList();

            if (list.Count > 0)
            {

                #region Entête
                worksheet.Range[1, 1].Text = eResApp.GetRes(_pref, 264);
                worksheet.Range[1, 2].Text = eResApp.GetRes(_pref, 8659);
                worksheet.Range[1, 3].Text = eResApp.GetRes(_pref, 7613);
                worksheet.Range[1, 4].Text = eResApp.GetRes(_pref, 222);
                worksheet.Range[1, 5].Text = eResApp.GetRes(_pref, 607);
                worksheet.Range[1, 6].Text = eResApp.GetRes(_pref, 7416);
                worksheet.Range[1, 7].Text = eResApp.GetRes(_pref, 7556);

                #endregion

                int rowIndex = 0;
                string userLabel = string.Empty;
                string levelValue = string.Empty;
                for (int i = 0; i < list.Count; i++)
                {
                    rowIndex = i + 2;

                    right = list[i];

                    eAdminTools.ExportPermValues(_pref, right.Perm, out userLabel, out levelValue);

                    worksheet.Range[rowIndex, 1].Text = right.TabLabel;
                    worksheet.Range[rowIndex, 2].Text = right.TypeLabel;
                    worksheet.Range[rowIndex, 3].Text = right.TabFromLabel;
                    worksheet.Range[rowIndex, 4].Text = right.FieldLabel;
                    worksheet.Range[rowIndex, 5].Text = right.TraitLabel;
                    worksheet.Range[rowIndex, 6].Text = eAdminTools.GetUserLevelLabel(_pref, GetLevelValue(right.Perm));
                    worksheet.Range[rowIndex, 7].Text = userLabel;
                }

                worksheet.Name = eResApp.GetRes(_pref, 1653);
                worksheet.UsedRange.AutofitColumns();

            }




            workbook.SaveAs(string.Concat("ExportRights", "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"), ".xls"), ExcelSaveType.SaveAsXLS, _context.Response, ExcelDownloadType.PromptDialog);
            workbook.Close();

            excelEngine.Dispose();

        }

        /// <summary>
        /// Récupère la valeur du niveau de la permission
        /// </summary>
        /// <param name="perm"></param>
        /// <returns></returns>
        private string GetLevelValue(ePermission perm)
        {
            String levelValue = "0";
            if (perm != null)
            {
                //inp.Attributes.Add("value", t.Perm.PermLevel.ToString());
                if (perm.PermLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode())
                    levelValue = "6";
                else if (perm.PermLevel == 0)
                    levelValue = "7"; // Ne pas tenir compte du niveau                                  
                else
                    levelValue = perm.PermLevel.ToString();
            }

            return levelValue;
        }

        /// <summary>
        /// Exporter la liste des droits
        /// </summary>
        /// <param name="context">Contexte HTTP</param>
        /// <param name="pref">ePref</param>
        /// <param name="filters">Filtres</param>
        public static void ExportList(HttpContext context, ePref pref, eAdminRightsFilters filters)
        {
            eAdminRightsExport export = new eAdminRightsExport(context, pref, filters);
            export.CreateWorkbook();

        }
    }
}