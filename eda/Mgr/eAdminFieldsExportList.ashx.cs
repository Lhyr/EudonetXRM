using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.eda.Renderer;
using EudoQuery;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Com.Eudonet.Core.Model.eda;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminFieldsExportList
    /// </summary>
    public class eAdminFieldsExportList : eEudoManager
    {

        private const string EXPORTCHARTFILENAME = "ExportListe";
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
            eAdminFieldsListRendererForExport _data = null;
            Int32? _nTab = _requestTools.GetRequestFormKeyI("nTab");
            string _tableLibelle = string.Empty;
            bool bResFound = false;
            if (_nTab.HasValue)
            {
                _data = new eAdminFieldsListRendererForExport(_pref, _nTab.Value);
                _data.Generate();

                ExcelEngine excelEngine = new ExcelEngine();
                IApplication application = excelEngine.Excel;
                IWorkbook workbook = application.Workbooks.Create(1);

                _tableLibelle = eAdminFieldsRGPDListRendererForExport.GetTableName(_pref, _nTab.Value);

                if (_data != null)
                {
                    List<string> lines = _data.GetListForHeader();

                    if (lines.Count > 0)
                    {
                        int nbLine = 1;                        
                        application.DefaultVersion = eTools.GetOfficeVersion(_pref);
                        IWorksheet worksheet = workbook.Worksheets[0];

                        for (int i = 0; i < lines.Count; i++)
                        {
                            worksheet.Range[nbLine, i + 1].Text = lines[i];
                        }


                        foreach (eAdminFieldInfos fi in _data.GetFieldsListForBody())
                        {
                            nbLine++;

                            int nbCol = 1;

                            // Nom SQL du champ
                            worksheet.Range[nbLine, nbCol++].Text = fi.FieldName;

                            // DescId
                            worksheet.Range[nbLine, nbCol++].Text = fi.DescId.ToString();


                            //// Libellé
                            //fi.Labels[Pref.LangId],
                            worksheet.Range[nbLine, nbCol++].Text = fi.Labels[_pref.LangId];

                            //// Type
                            worksheet.Range[nbLine, nbCol++].Text = (fi.Format != FieldFormat.TYP_CHAR ? eAdminTools.GetFieldTypeLabel(_pref, fi.Format) : eAdminTools.GetCharTypeLabel(_pref, fi.DescId, fi.PopupType, fi.PopupDescId));

                            //// Administration restreinte
                            worksheet.Range[nbLine, nbCol++].Text = fi.SuperAdminOnly ? "1" : "0";

                            ////historique
                            worksheet.Range[nbLine, nbCol++].Text = (fi.Historic) ? eResApp.GetRes(_pref, 58) : eResApp.GetRes(_pref, 59);


                            //// Infobulle
                            worksheet.Range[nbLine, nbCol++].Text = fi.ToolTipText;


                            //// Filigrane
                            worksheet.Range[nbLine, nbCol++].Text = fi.WaterMark;

                            //// Annulation de la saisie autorisée
                            worksheet.Range[nbLine, nbCol++].Text = fi.CancelLastValueAllowed ? "1" : "0";

                            //// Valeur par défaut
                            worksheet.Range[nbLine, nbCol++].Text = (fi.Default.ToLower().Contains("select")) ? String.Concat("<", eResApp.GetRes(_pref, 7707), ">") : fi.Default;

                            //// Ordre d'affichage
                            worksheet.Range[nbLine, nbCol++].Text = fi.Disporder.ToString();

                            //// Ordre de saisie
                            worksheet.Range[nbLine, nbCol++].Text = fi.TabIndex.ToString();

                            //// Longueur/décimales
                            worksheet.Range[nbLine, nbCol++].Text = (fi.Format == FieldFormat.TYP_CHAR || fi.Format == FieldFormat.TYP_NUMERIC) ? fi.Length.ToString() : "";

                            //// Somme dans les entêtes
                            worksheet.Range[nbLine, nbCol++].Text = (fi.ComputedFieldEnabled) ? eResApp.GetRes(_pref, 58) : eResApp.GetRes(_pref, 59);

                            //// Nature de catalogue
                            if (fi.PopupType != EudoQuery.PopupType.NONE)
                                worksheet.Range[nbLine, nbCol++].Text = eAdminTools.GetCatalogNature(_pref, fi.PopupType, fi.Multiple, fi.GetFileDataParam(_pref).TreeView);
                            else
                                worksheet.Range[nbLine, nbCol++].Text = string.Empty;

                            //// Source
                            if (fi.DescId != fi.PopupDescId && fi.PopupDescId != 0 && fi.PopupDescId % 100 > 1)
                                worksheet.Range[nbLine, nbCol++].Text = eLibTools.GetFullNameByDescId(eLibTools.GetEudoDAL(_pref), fi.PopupDescId, _pref.Lang);
                            else
                                worksheet.Range[nbLine, nbCol++].Text = string.Empty;

                            //// Onglet lié
                            if (fi.DescId != fi.PopupDescId && fi.PopupDescId != 0 && fi.PopupDescId % 100 == 1)
                                worksheet.Range[nbLine, nbCol++].Text = _data.GetResInternal().GetRes(fi.PopupDescId - fi.PopupDescId % 100, out bResFound);
                            else
                                worksheet.Range[nbLine, nbCol++].Text = string.Empty;

                            //// Visibilité
                            worksheet.Range[nbLine, nbCol++].Text = GetRule(TypeTraitConditionnal.FieldView, fi.DescId);

                            //// Modification
                            worksheet.Range[nbLine, nbCol++].Text = GetRule(TypeTraitConditionnal.FieldUpdate, fi.DescId);

                            //// Saisie obligatoire
                            worksheet.Range[nbLine, nbCol++].Text = GetRule(TypeTraitConditionnal.FieldObligat, fi.DescId);

                            //// Après enregistrement - Formule du haut
                            if (!String.IsNullOrEmpty(fi.Formula))
                                worksheet.Range[nbLine, nbCol++].Text = eResApp.GetRes(_pref, 1700);
                            else
                                worksheet.Range[nbLine, nbCol++].Text = string.Empty;

                            //// Dépendances
                            worksheet.Range[nbLine, nbCol++].Text = GetDependances(fi.DescId);

                            //// Déclencheurs de
                            worksheet.Range[nbLine, nbCol++].Text = GetAutomatisms(fi, _data.GetDicTriggers());
                        }


                        worksheet.Name = string.Concat(eResApp.GetRes(_pref, 7694), " (", _tableLibelle, ")");
                        worksheet.UsedRange.AutofitColumns();
                    }

                }

                workbook.SaveAs(string.Concat(EXPORTCHARTFILENAME, "_", _tableLibelle, "_", DateTime.Now.ToString("yyyyMMddHHmmssffff"), ".xls"), ExcelSaveType.SaveAsXLS, _context.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
                excelEngine.Dispose();

            }
            else
            {
                throw new Exception("Erreur sur l'export de la liste des rubriques : Id de la table est inconnu");
            }



        }

        /// <summary>
        /// Création d'une cellule avec la liste des conditions de la règle
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="type"></param>
        /// <param name="descid"></param>
        string GetRule(TypeTraitConditionnal type, int descid)
        {
            string ruleDef = string.Empty;
            eRules listRules = eRules.GetRules(type, descid, _pref);
            if (listRules.AllRules.Count > 0)
            {
                foreach (Tuple<AdvFilter, InterOperator> f in listRules.AllRules[0].ListFilter)
                {
                    ruleDef = String.Concat(ruleDef, ruleDef.Length > 0 ? Environment.NewLine : string.Empty, f.Item1.FilterName);
                }
            }
            return ruleDef;

        }

        /// <summary>
        /// Retourne la liste des dépendances
        /// </summary>
        /// <param name="descid">descid de la rubrique</param>
        string GetDependances(int descid)
        {

            String dependences = string.Empty;
            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

            try
            {
                eDal?.OpenDatabase();
                foreach (String d in eSqlDesc.GetSQLDependencies(eDal, descid))
                {
                    dependences = String.Concat(dependences, dependences.Length > 0 ? Environment.NewLine : string.Empty, d);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                eDal?.CloseDatabase();
            }


            return dependences;

        }

        /// <summary>
        /// Retourne la liste des automatismes /"Déclencheur de"/
        /// </summary>
        /// <param name="fi">Rubrique</param>
        /// <param name="dicTriggers"></param>
        string GetAutomatisms(eAdminFieldInfos fi, Dictionary<int, List<eAdminTriggerField>> dicTriggers)
        {
            String automatisms = string.Empty;

            if (dicTriggers.ContainsKey(fi.DescId))
            {
                foreach (eAdminTriggerField trigger in dicTriggers[fi.DescId])
                {
                    automatisms = String.Concat(automatisms, automatisms.Length > 0 ? Environment.NewLine : string.Empty, trigger.ToString());
                }
            }

            return automatisms;

        }
    }
}