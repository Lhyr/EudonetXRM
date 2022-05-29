using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;
using Com.Eudonet.Internal;
using System.Text.RegularExpressions;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Gère les actions sur les fichiers depuis les champs de type fichier (ouverture renommage suppression)
    /// </summary>
    public class eFieldFilesManager : eEudoManager
    {
        /// <summary>
        /// Gestion des fichiers
        /// </summary>
        protected override void ProcessManager()
        {
            if (!_allKeys.Contains("folder") || !_allKeys.Contains("action") || !_allKeys.Contains("file"))
                return;

            if (String.IsNullOrEmpty(_context.Request.Form["folder"].ToString())
                || String.IsNullOrEmpty(_context.Request.Form["action"].ToString())
                || String.IsNullOrEmpty(_context.Request.Form["file"].ToString()))
                return;



            String sAction = _context.Request.Form["action"].ToString();
            String sFolder = _context.Request.Form["folder"].ToString();
            String sFile = _context.Request.Form["file"].ToString();


            Int32 iFolderType = 0;
            eLibConst.FOLDER_TYPE folderType = eLibConst.FOLDER_TYPE.FOLDERS;
            Regex reFolders = new Regex(@"^\[[0-9]\]+$");
            if (!reFolders.IsMatch(sFolder) && Int32.TryParse(sFolder, out iFolderType))
            {
                try
                {
                    folderType = (eLibConst.FOLDER_TYPE)iFolderType;
                    if (folderType == eLibConst.FOLDER_TYPE.MODELES)
                        sFolder = "";
                }
                catch (Exception ex)
                {
                    folderType = eLibConst.FOLDER_TYPE.FOLDERS;
                }
            }

            String sFolderPath = String.Concat(eModelTools.GetPhysicalDatasPath(folderType, _pref.GetBaseName), @"\", sFolder, @"\");

            String sError = "";
            String sUsrErr = "";

            XmlDocument xmlDoc = new XmlDocument();
            XmlNode xmlRoot = xmlDoc.CreateElement("return");
            xmlDoc.AppendChild(xmlRoot);

            XmlNode xmlSuccess = xmlDoc.CreateElement("success");
            xmlRoot.AppendChild(xmlSuccess);
            XmlNode xmlResultNode;
            switch (sAction)
            {
                case "del":
                    Int32 descid;
                    Int32.TryParse(sFolder.Replace("[", "").Replace("]", ""), out descid);
                    sError = ePJTraitementsLite.DeletePjFromDisk(sFolderPath, _pref.GetBaseName, sFile);
                    bool refresh = false;
                    if (sError.Length <= 0)
                    {

                        TableLite tab = new TableLite(descid);
                        sError = ePJTraitements.updateFieldFiles(_pref, tab, sFile, out refresh, out sUsrErr);
                    }

                    xmlResultNode = xmlDoc.CreateElement("deletedfile");
                    xmlRoot.AppendChild(xmlResultNode);
                    xmlResultNode.InnerText = sFile;

                    xmlResultNode = xmlDoc.CreateElement("refresh");
                    xmlRoot.AppendChild(xmlResultNode);
                    xmlResultNode.InnerText = refresh ? "1" : "0";

                    break;
                case "ren":

                    if (!_allKeys.Contains("newname") || String.IsNullOrEmpty(_context.Request.Form["newname"].ToString()))
                        return;

                    Boolean bMultiple = false;
                    String sLstFiles = "";
                    Int32 idx = 0;

                    if (_allKeys.Contains("mult"))
                        bMultiple = _context.Request.Form["mult"].ToString() == "1";

                    if (_allKeys.Contains("files"))
                        sLstFiles = _context.Request.Form["files"].ToString();

                    if (_allKeys.Contains("idx"))
                        Int32.TryParse(_context.Request.Form["idx"].ToString(), out idx);


                    String sNewFileName = _context.Request.Form["newname"].ToString();
                    sError = ePJTraitementsLite.RenamePjOnDisk(_pref, sFolderPath, sFile, sNewFileName, out sUsrErr);

                    xmlResultNode = xmlDoc.CreateElement("renamedfile");
                    xmlRoot.AppendChild(xmlResultNode);

                    //eFieldFilesRenderer rdr = new eFieldFilesRenderer(_pref, sFolder, bMultiple, sLstFiles);
                    //Panel pnFileLine = rdr.GetFileLine(sNewFileName, idx);

                    xmlResultNode.InnerXml = sNewFileName;

                    break;
                default:
                    break;
            }

            if (sError.Length > 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", sFile), sUsrErr, eResApp.GetRes(_pref, 72), sError));
                return;
            }
            else if (sUsrErr.Length > 0)
            {
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", sFile), sUsrErr, eResApp.GetRes(_pref, 72)));
                return;
            }
            else
            {
                xmlSuccess.InnerText = "1";
            }

            _context.Response.Clear();
            _context.Response.ClearContent();
            _context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
            _context.Response.ContentType = "text/xml";
            _context.Response.Write(xmlDoc.OuterXml);
            //_context.Response.End();
            throw new eEndResponseException();


        }

    }
}