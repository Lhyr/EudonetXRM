using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using CsvHelper;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExportCatalogManager
    /// </summary>
    public class eAdminExportCatalogManager : eAdminManager
    {
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessManager ()
        {
            //try catch eudo exceptio
            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                try
                {

                    int nCatDescId = (int)_requestTools.GetRequestQSKeyI("descid");
                    Dictionary<string, string> usersLang = new Dictionary<string, string>();
                    dal.OpenDatabase();
                    try
                    {
                        eCatalog cc = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, nCatDescId, showHiddenValues: true, bypassNoAutoload: false, loadAllLang: true, treeView: false, searchedValues: new List<eCatalog.CatalogValue>());
                        if (cc.Values.Count > 0)
                        {
                            // creation un flux pour memoriser les données
                            using (var mem = new MemoryStream())
                            using (var writer = new StreamWriter(mem))
                            using (var csvWriter = new CsvWriter(writer))
                            {
                                csvWriter.Configuration.Delimiter = ";";

                                // ALISTER Demande / Request 79 555 Tout en miniscule
                                csvWriter.WriteField("id");
                                csvWriter.WriteField("parentid");
                                csvWriter.WriteField("code");
                                csvWriter.WriteField("desactive");
                                usersLang = eDataTools.GetUsersLangFilter(_pref);
                                foreach (KeyValuePair<int, Tuple<string, string>> langAndTooltip in cc.Values[0].ListOfLangAndTooltip)
                                {
                                    string langi = string.Empty;
                                    usersLang.TryGetValue(langAndTooltip.Key.ToString(), out langi);
                                    if (string.IsNullOrEmpty(langi))
                                    {
                                        //KJE, Demande #78 698
                                        csvWriter.WriteField(string.Concat("lib_lang_", (langAndTooltip.Key < 10) ? ("0" + langAndTooltip.Key.ToString()) : langAndTooltip.Key.ToString()));
                                        csvWriter.WriteField(string.Concat("infobulle_lang_", (langAndTooltip.Key < 10) ? ("0" + langAndTooltip.Key.ToString()) : langAndTooltip.Key.ToString()));
                                    }
                                    else
                                    {
                                        csvWriter.WriteField(string.Concat("lib_", langi.ToUpper()));
                                        csvWriter.WriteField(string.Concat("infobulle_", langi.ToUpper()));
                                    }
                                }

                                csvWriter.NextRecord();

                                foreach (var catalogvalue in cc.Values)
                                {
                                    csvWriter.WriteField(catalogvalue.Id);
                                    csvWriter.WriteField(catalogvalue.ParentId);
                                    csvWriter.WriteField(catalogvalue.Data);
                                    csvWriter.WriteField(catalogvalue.IsDisabled);

                                    foreach (KeyValuePair<int, Tuple<string, string>> langAndTooltip in catalogvalue.ListOfLangAndTooltip)
                                    {
                                        csvWriter.WriteField(langAndTooltip.Value.Item1);
                                        csvWriter.WriteField(langAndTooltip.Value.Item2);
                                    }

                                    csvWriter.NextRecord();
                                }

                                writer.Flush();
                                var result = Encoding.UTF8.GetString(mem.ToArray());
                                HttpContext.Current.Response.Clear();
                                //HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.UTF8;
                                HttpContext.Current.Response.ContentEncoding = System.Text.Encoding.GetEncoding("iso-8859-1"); // ALISTER Demande / Request 86 059 (Encodage pour é, è, à, etc... / Encoding for é, è, à, etc...)
                                HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=catalogExport.csv");
                                HttpContext.Current.Response.Charset = "";
                                HttpContext.Current.Response.ContentType = "application/csv";
                                //HttpContext.Current.Response.Output.Write(result.ToString().Replace(",", ";"));
                                HttpContext.Current.Response.Output.Write(result.ToString()); // ALISTER Demande / Request 86 059 (Pas besoin de remplacer "," / Doesn't need to replace ",")
                                                                                             
                            }

                        }
                    }
                    catch (EudoException ee)
                    {
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.Write(ee.UserMessageTitle);
                        HttpContext.Current.Response.Write(ee.UserMessage);
                        HttpContext.Current.Response.Write(ee.UserMessageDetails);
                    }
                    catch (Exception e)
                    {
                        LaunchError(
                        eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72), "Contactez le support", "", e.Message));
                    }
                    finally
                    {
                        dal?.CloseDatabase();
                    }

                }
                catch (EudoException ee)
                {
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.Write(ee.UserMessageTitle);
                    HttpContext.Current.Response.Write(ee.UserMessage);
                    HttpContext.Current.Response.Write(ee.UserMessageDetails);
                }
            }
        }

        protected override void CheckAdminRight ()
        {
            _pref.AdminMode = true;
            base.CheckAdminRight();
        }

    }
}