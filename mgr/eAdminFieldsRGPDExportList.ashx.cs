using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eAdminFieldsRGPDExportList
    /// </summary>
    public class eAdminFieldsRGPDExportList : eEudoManager
    {

        private const string EXPORTCHARTFILENAME = "ExportRGPD";
        private Int32 _ilang = 0;

        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {

            if (_pref != null)
                _ilang = _pref.LangId;
            try
            {
                SaveFile();
            }
            catch (Exception ex)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", HttpContext.Current.Request.Url.Segments[HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", ex.Message, Environment.NewLine, "Exception StackTrace :", ex.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                       eLibConst.MSG_TYPE.CRITICAL,
                       eResApp.GetRes(_ilang, 72),   // Message En-tête : Une erreur est survenue
                       String.Concat(eResApp.GetRes(_ilang, 422), "<br>", eResApp.GetRes(_ilang, 544)),  //  Détail : pour améliorer...
                       eResApp.GetRes(_ilang, 72),  //   titre
                       String.Concat(sDevMsg));

                LaunchErrorHTML(true, ErrorContainer, "this.close()");

            }


        }

        /// <summary>
        /// Exporter fichier
        /// </summary>
        private void SaveFile()
        {
            eAdminFieldsRGPDListRendererForExport _data = null;
            Int32? _nTab = _requestTools.GetRequestFormKeyI("nTab");
            string _tableLibelle = string.Empty;
            if (_nTab.HasValue)
            {
                _data = new eda.eAdminFieldsRGPDListRendererForExport(_pref, _nTab.Value);
                _data.Generate();

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                IWorkbook workbook = application.Workbooks.Create(1);

                eTableLiteWithLib tab = new eTableLiteWithLib(_nTab.Value, _pref.Lang);
                _tableLibelle = eAdminFieldsRGPDListRendererForExport.GetTableName(_pref, _nTab.Value);

                if (_data != null)
                {
                    List<string[]> lines = _data.GetExportLine();

                    if (lines.Count > 0)
                    {
                        application.DefaultVersion = eTools.GetOfficeVersion(_pref);
                        IWorksheet worksheet = workbook.Worksheets[0];

                        for (int i = 0; i < lines.Count; i++)
                        {

                            for (int j = 0; j < lines[i].Length; j++)
                            {
                                worksheet.Range[j + 1, i + 1].Text = lines[i][j];
                            }
                        }

                        worksheet.Name = eResApp.GetRes(_pref, 8323);
                        worksheet.UsedRange.AutofitColumns();
                    }

                }

                workbook.SaveAs(string.Concat(EXPORTCHARTFILENAME, "_", _tableLibelle, "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"), ".xls"), ExcelSaveType.SaveAsXLS, _context.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();

            }
            else
            {
                throw new Exception("Erreur sur l'export de la liste RGPD : Id de la table est inconnu");
            }



        }

    }


}