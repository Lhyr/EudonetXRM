using Com.Eudonet.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eChekcPJExists
    /// </summary>
    public class eCheckPJExists : eEudoManager
    {
        /// <summary>
        /// Action du manager
        /// </summary>
        public enum MgrAction
        {
            /// <summary>
            /// Première vérification à l'upload
            /// </summary>
            Check = 0,
            /// <summary>
            /// Interface de confirmation de renommage ou remplacement des fichiers
            /// </summary>
            Confirmation = 1
        }




        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {
            int nAction = _requestTools.GetRequestFormKeyI("action") ?? 0;
            MgrAction action = (MgrAction)nAction;

            string sFiles = _requestTools.GetRequestFormKeyS("files");
            List<PJUploadInfo> files = new List<PJUploadInfo>();
            try
            {
                files = JsonConvert.DeserializeObject<List<PJUploadInfo>>(sFiles);
            }
            catch (Exception)
            {
                return;
            }

            switch (action)
            {
                case MgrAction.Check:

                    if (files.Count == 0)
                    {
                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), "Aucun nom de fichier spécifié"));
                    }

                    Dictionary<String, String> dicRename = new Dictionary<string, string>();
                    foreach (PJUploadInfo fi in files)
                    {
                        dicRename.Add(fi.FileName, ePJTraitements.EscapeSpecialCharactersInFilename(fi.SaveAs.Length > 0 ? fi.SaveAs : fi.FileName));
                    }
                    String sTargetPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName);

                    _xmlResult = new XmlDocument();

                    XmlNode maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                    _xmlResult.AppendChild(maintNode);

                    XmlNode xmlRoot = _xmlResult.CreateElement("xmlRoot");
                    _xmlResult.AppendChild(xmlRoot);

                    bool bAllSuccess = true;
                    XmlNode xmlSuccess = _xmlResult.CreateElement("success");
                    xmlRoot.AppendChild(xmlSuccess);

                    List<String> liNames = new List<string>();

                    foreach (KeyValuePair<String, String> kvp in dicRename)
                    {

                        XmlNode xmlFile = _xmlResult.CreateElement("file");
                        xmlRoot.AppendChild(xmlFile);

                        XmlAttribute xmlId = _xmlResult.CreateAttribute("id");
                        xmlFile.Attributes.Append(xmlId);
                        xmlId.InnerText = kvp.Key;

                        XmlNode xmlFileSuccess = _xmlResult.CreateElement("success");
                        xmlFile.AppendChild(xmlFileSuccess);

                        XmlNode xmlWindowTitle = _xmlResult.CreateElement("title");
                        xmlFile.AppendChild(xmlWindowTitle);

                        XmlNode xmlWindowDescription = _xmlResult.CreateElement("description");
                        xmlFile.AppendChild(xmlWindowDescription);

                        XmlNode xmlSuggestedName = _xmlResult.CreateElement("suggestedname");
                        xmlFile.AppendChild(xmlSuggestedName);

                        string sTargetPathWithFileName = string.Concat(sTargetPath, "\\", kvp.Value);
                        if (File.Exists(sTargetPathWithFileName))
                        {
                            xmlFileSuccess.InnerText = "0";
                            xmlSuggestedName.InnerText = ePJTraitements.GetValidFileName(sTargetPath, kvp.Value, liNames);
                            xmlWindowTitle.InnerText = eResApp.GetRes(_pref, 6879); // Les fichiers suivants existent déjà. Veuillez les renommer.
                            xmlWindowDescription.InnerText = eResApp.GetRes(_pref, 8693); // Humm, il semble que certains fichiers portent le même nom, que souhaitez-vous faire ?
                            bAllSuccess = false;
                        }
                        // #31 762 : on gère également d'autres cas de noms de fichiers invalides (ex : chemin total trop long)
                        else
                        {
                            // Pour tester la longueur du chemin total, on tente d'écrire un fichier vide à cet endroit, et on intercepte PathTooLongException
                            try
                            {
                                TextWriter tw = new StreamWriter(sTargetPathWithFileName);
                                tw.Flush();
                                tw.Close();
                                // Un fichier de test a pu être sauvegardé avec ce nom à cet endroit : on valide le nom
                                xmlFileSuccess.InnerText = "1";
                                xmlSuggestedName.InnerText = kvp.Value;
                            }
                            // Si le fichier ne peut pas être enregistré à cause d'un chemin trop long : on en suggère un autre, et on indique d'afficher la popup
                            catch (PathTooLongException)
                            {
                                Exception innerException = null;
                                xmlFileSuccess.InnerText = "0";
                                xmlSuggestedName.InnerText = ePJTraitements.GetTruncatedFileName(sTargetPath, kvp.Value, out innerException, liNames);
                                xmlWindowTitle.InnerText = eResApp.GetRes(_pref, 1923); // Un ou plusieurs fichiers portent un nom trop long. Veuillez les raccourcir.
                                xmlWindowDescription.InnerText = eResApp.GetRes(_pref, 1924); // Humm, il semble que certains fichiers portent un nom trop long, que souhaitez-vous faire ?
                                bAllSuccess = false;
                            }
                            finally
                            {
                                try
                                {
                                    // Le fichier ayant forcément été créé après vérification de non-existence, on supprime sans vérification ici, car on est, du coup,
                                    // certains que le fichier a été créé par le StreamWriter ci-dessus
                                    if (File.Exists(sTargetPathWithFileName))
                                        File.Delete(sTargetPathWithFileName);
                                }
                                catch
                                {
                                }
                            }
                        }
                        liNames.Add(xmlSuggestedName.InnerText);
                    }

                    xmlSuccess.InnerText = bAllSuccess ? "1" : "0";
                    RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });

                    break;
                case MgrAction.Confirmation:
                    AddHeadAndBody = true;

                    PageRegisters.AddScript("eTools");
                    PageRegisters.AddScript("eMain");
                    PageRegisters.AddScript("eUpdater");

                    PageRegisters.AddCss("eMain");
                    PageRegisters.AddCss("eudoFont");
                    PageRegisters.AddCss("eModalDialog");
                    PageRegisters.AddCss("ePJ");

                    int tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
                    int fid = _requestTools.GetRequestFormKeyI("fileid") ?? 0;
                    string windowDescription = _requestTools.GetRequestFormKeyS("description");

                    ePJCheckerRenderer rdr = ePJCheckerRenderer.CreatePJCheckerRenderer(_pref, tab, fid, windowDescription, files);
                    rdr.Generate();
                    RenderResultHTML<Panel>(rdr.PgContainer);
                    break;
            }

        }


    }
}