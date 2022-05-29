using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using CsvHelper;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using System.Xml;
using static Com.Eudonet.Internal.eCatalog;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminImportCatalogManager
    /// </summary>
    public class eAdminImportCatalogManager : eAdminManager
    {
        protected override void ProcessManager ()
        {
            string eError = string.Empty;
            List<Tuple<Boolean, string>> result = new List<Tuple<bool, string>>();
            string resultlang = string.Empty;
            List<string> listErreur = new List<string>();
            string EmptyListOfCatalogue = string.Empty;
            JSONReturnGeneric res = new JSONReturnGeneric();
            List<CatalogValue> listofcatalogue = new List<CatalogValue>();
            CatalogValue newcat = null;
            int id = 0, parentid = 0;
            Boolean desactive;

            using (eudoDAL dal = eLibTools.GetEudoDAL(_pref))
            {
                dal.OpenDatabase();
                try
                {
                    int _catDescId = (int)_requestTools.GetRequestFormKeyI("CatDescId");
                    string _importvalue = (string)_requestTools.GetRequestFormKeyS("importvalue");

                    CsvReader csvfile = new CsvReader(new StringReader(_importvalue));
                    csvfile.Configuration.Delimiter = ";";
                    csvfile.Configuration.MissingFieldFound = null;
                    csvfile.Read();
                    csvfile.ReadHeader();
                    List<string> headers = csvfile.Context.HeaderRecord.ToList();

                    res.Success = false;

                    eCatalog ecatalog = new eCatalog(dal, _pref, PopupType.DATA, _pref.User, _catDescId, showHiddenValues: true, bypassNoAutoload: false, loadAllLang: true, treeView: false, searchedValues: new List<eCatalog.CatalogValue>());

                    var lines = _importvalue.Split('\n');
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(lines[i]) && !string.IsNullOrWhiteSpace(lines[i]))
                        {
                            newcat = new CatalogValue();
                            newcat.ListOfValueInTenLan = new Dictionary<string, string>();
                            newcat.ListOfToolTipInTenLan = new Dictionary<string, string>();

                            var valueToImportlines = lines[i].Split(';');

                            if (valueToImportlines.Length == headers.Count)
                            {
                                for (int j = 0; j < valueToImportlines.Length; j++)
                                {
                                    if (headers[j].ToLower() == "id")
                                    {
                                        string idvalue = valueToImportlines[j];
                                        if (int.TryParse(idvalue, out id))
                                        {
                                            newcat.Id = id;
                                        }
                                        else if ((string.IsNullOrEmpty(idvalue) && string.IsNullOrWhiteSpace(idvalue)))
                                        {
                                            newcat.Id = 0;
                                        }
                                        else
                                            throw new CatalogException("le format d'Id de catalogue n'est pas correcte ", _catDescId);
                                    }

                                    if (headers[j].ToLower() == "parentid")
                                    {
                                        if (int.TryParse(valueToImportlines[j], out parentid))
                                            newcat.ParentId = parentid;
                                        else
                                            throw new CatalogException("le format de ParentId de catalogue n'est pas correcte ", _catDescId);
                                    }

                                    if (headers[j].ToLower() == "code")
                                    {
                                        newcat.Data = valueToImportlines[j];
                                    }

                                    if (headers[j].ToLower() == "desactive")
                                    {
                                        if (Boolean.TryParse(valueToImportlines[j], out desactive))
                                            newcat.IsDisabled = Convert.ToBoolean(valueToImportlines[j]);
                                        else
                                            throw new CatalogException("le format de champ DESACTIVE de catalogue n'est pas correcte ", _catDescId);
                                    }

                                    if (headers[j].ToLower().Contains("lib_"))
                                    {
                                        string langname = headers[j].Substring(4);

                                        if ((langname.ToLower().Contains("lang_")))
                                        {
                                            int numbr = -1;
                                            int.TryParse(langname.Substring(5), out numbr);
                                            if (numbr < 10)
                                                resultlang = "Lang_0" + numbr;
                                            else
                                                resultlang = "Lang_" + numbr;
                                        }
                                        else
                                        {
                                            int idlang = ecatalog.GetActiveLang(dal, langname, out eError);
                                            if (idlang < 10)
                                                resultlang = "Lang_0" + idlang;
                                            else
                                                resultlang = "Lang_" + idlang;
                                        }


                                        resultlang = resultlang.ToLower();

                                        newcat.ListOfValueInTenLan.Add(resultlang, valueToImportlines[j]);
                                    }

                                    if (headers[j].Contains("INFOBULLE_"))
                                    {
                                        string langname = headers[j].Substring(10);

                                        if ((langname.Contains("LANG_")))
                                        {
                                            int numbr = -1;
                                            int.TryParse(langname.Substring(5), out numbr);
                                            if (numbr < 10)
                                                resultlang = "Tip_lang_0" + numbr;
                                            else
                                                resultlang = "Tip_lang_" + numbr;

                                        }
                                        else
                                        {
                                            int idlang = ecatalog.GetActiveLang(dal, langname, out eError);
                                            if (idlang < 10)
                                                resultlang = "Tip_lang_0" + idlang;
                                            else
                                                resultlang = "Tip_lang_" + idlang;
                                        }

                                        resultlang = resultlang.ToLower();

                                        newcat.ListOfToolTipInTenLan.Add(resultlang, valueToImportlines[j]);
                                    }

                                }

                                listofcatalogue.Add(newcat);
                            }
                            else
                            {
                                listErreur.Add(eResApp.GetRes(_pref, 2468)+ " ligne :" + i);
                                continue;
                                throw new CatalogException(eResApp.GetRes(_pref, 2468), _catDescId);
                            }
                        }
                    }

                    try
                    {
                        eCatalogImport ss = new eCatalogImport();
                        if (listofcatalogue.Count != 0)
                        {
                            result = ss.ImportCatalogue(listofcatalogue, _catDescId, _pref, _pref.User);

                            foreach (Tuple<Boolean,string> r in result)
                            {
                                if (!r.Item1)
                                {
                                    listErreur.Add(r.Item2);
                                }                                    
                            }                           
                        }
                        else if (listofcatalogue.Count == 0 && listErreur.Count == 0)
                        {
                            EmptyListOfCatalogue = eResApp.GetRes(_pref, 2460);
                            res.ErrorMsg = eResApp.GetRes(_pref, 2460) + "\n";
                            res.ErrorDetailMsg = eResApp.GetRes(_pref, 2461) + "\n";
                            throw new CatalogException(eResApp.GetRes(_pref, 2460), _catDescId);
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    dal.CloseDatabase();

                    if (string.IsNullOrEmpty(EmptyListOfCatalogue) && listofcatalogue.Count > 0)
                    {
                        res.Success = true;
                    }  

                    foreach (string erroritem in listErreur)
                    {
                        res.ErrorMsg = res.ErrorMsg  + erroritem  + "\n";
                    }

                }
            }

            RenderResult(RequestContentType.TEXT, delegate ()
               {
                   return JsonConvert.SerializeObject(res);
               });
        }

        protected override void CheckAdminRight ()
        {
            _pref.AdminMode = true;
            base.CheckAdminRight();
        }
    }
}