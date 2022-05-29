using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.Import;
using Com.Eudonet.Xrm.renderer;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <className>eTargetImportProcessManager</className>
    /// <summary>Gestionnaire d'import des cibles étendues</summary>
    /// <authors>JBE</authors>
    /// <date>12/2012</date>
    public class eTargetProcessManager : eEudoManager
    {
        private eImportParams _importParams;
        private eImportEventArgs _importEvent;
        private Int32 _contentSize = 0;

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            _importParams = new eImportParams(_requestTools);

            switch (_importParams.Action)
            {
                case eImportAction.LOAD_FROM_FILE:
                    // On sauvegarde le contenu du fichier dans la session
                    string ContentPreview = SaveContentInSession();

                    // On fait un rendu de prévisualisation du contenu
                    RenderFilePreview(ContentPreview);

                    break;
                case eImportAction.LOAD_FROM_TEXT:
                    // On récupere le contenu text (session ou request) et on importe les cibles etendu
                    ProcessImport(GetContentText());

                    Render();

                    break;
                case eImportAction.CHECK_PROGRESS:
                    RenderProgress();

                    break;
                case eImportAction.DO_NOTHING:
                default:
                    break;
            }
        }

        /// <summary>
        /// Récupère le contenu du fichier et le sauvegarde dans la session
        /// </summary>
        private string SaveContentInSession()
        {

            HttpPostedFile file = _context.Request.Files["filesrc"];
            if (file != null && file.ContentLength > 0 && (file.FileName.EndsWith(".txt") || file.FileName.EndsWith(".csv")))
            {
                StringBuilder sbReturn = new StringBuilder();

                string fname = Path.GetFileName(file.FileName);
                StreamReader sr = new StreamReader(file.InputStream, Encoding.Default);
                StringBuilder input = new StringBuilder();
                StringBuilder inputPreview = new StringBuilder();
                StringBuilder inputHead = new StringBuilder();
                int idx = 0;
                while (sr.Peek() > -1)
                {
                    string line = sr.ReadLine();
                    input = input.Append(line).Append("<br>");

                    // La 1ere et la 2eme ligne pour la prévisualisation
                    if (idx < 2)
                        inputPreview.Append(line).Append("<br>");

                    if (idx < 10)
                    {
                        // MCR 38463 supprimez l encodage qui pose probleme le & est transofrme en &amp; le ; est un separateur de colonnes pour un CSV
                        //  inputHead = inputHead.Append(HttpUtility.HtmlEncode(line)).Append("<br>");

                        inputHead = inputHead.Append(line).Append("<br>");
                    }
                    idx++;
                }
                sr.Close();

                _context.Session["ImportFileContent"] = input.ToString();

                return inputPreview.ToString();
            }
            else
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION,
                  eResApp.GetRes(_pref, 6568),
                  eResApp.GetRes(_pref, 6569),
                  eResApp.GetRes(_pref, 416));//erreur

                // Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            return string.Empty;
        }

        /// <summary>
        /// Fait un rendu de fichier text/csv
        /// </summary>
        private void RenderFilePreview(string preview)
        {
            HtmlGenericControl divReturn = new HtmlGenericControl("div");
            divReturn.ID = "result";
            divReturn.InnerHtml = preview;// _context.Session["ImportFileContent"].ToString();
            RenderResultHTML(divReturn);
        }

        /// <summary>
        /// Récupère le contenu text à parser
        /// </summary>
        private String GetContentText()
        {
            String content = String.Empty;

            if (_importParams.FromFile)
            {
                if (_context.Session["ImportFileContent"] != null)
                {
                    content = _context.Session["ImportFileContent"].ToString();
                    _context.Session.Remove("ImportFileContent");
                }

                content = content.Replace("<br>", "\n");
            }
            else
            {
                content = _requestTools.GetRequestFormKeyS("filecontent") ?? String.Empty;
            }

            return content;
        }

        /// <summary>
        /// Lance l'import
        /// </summary>
        private void ProcessImport(String content)
        {
            eImportContent impContent = new eImportContent();
            impContent.Load(content, _importParams);

            _contentSize = impContent.ContentSize;

            try
            {
                eTargetImport target = new eTargetImport(_pref);

                target.StartImport += SaveImportState;
                target.EndLineImport += SaveImportState;
                target.EndImport += CleanSession;

                target.ImportData(_importParams, impContent);
            }
            finally
            {
                // On libère ici l'objet car il peux être volumineux pour la mémoire
                impContent.Destroy();
            }
        }

        /// <summary>
        /// Initilise les valeur de l'import dans la session
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="importEvt"></param>
        private void SaveImportState(object Sender, eImportEventArgs importEvt)
        {
            _importEvent = importEvt;
            _context.Session["ImportEventArgs"] = importEvt;
        }

        /// <summary>
        /// A la fin de l'import on nétoie la session
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="args"></param>
        private void CleanSession(object Sender, eImportEventArgs importEvt)
        {
            _importEvent = importEvt;
            _context.Session.Remove("ImportEventArgs");
        }

        /// <summary>
        /// Lance l'import
        /// </summary>
        private void Render()
        {
            _xmlResult = new XmlDocument();
            XmlNode mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(mainNode);

            XmlNode detailsNode = _xmlResult.CreateElement("import");
            _xmlResult.AppendChild(detailsNode);

            // Nombre de ligne
            XmlNode countNode = _xmlResult.CreateElement("GlobalCount");
            detailsNode.AppendChild(countNode);
            countNode.InnerText = _importEvent.TotalLine.ToString();

            // Taille de text avec l'unité (octets ou koctets)
            XmlNode sizeNode = _xmlResult.CreateElement("GlobalSize");
            detailsNode.AppendChild(sizeNode);
            sizeNode.InnerText = eLibTools.GetSizeString(_pref, _contentSize);

            // Nombre de ligne succes
            XmlNode validLinesNode = _xmlResult.CreateElement("ValidLines");
            detailsNode.AppendChild(validLinesNode);
            XmlAttribute attCntValid = _xmlResult.CreateAttribute("CountLinks");
            validLinesNode.Attributes.Append(attCntValid);
            attCntValid.InnerText = _importEvent.TotalSucessLine.ToString();

            // Nombre de ligne erreur
            XmlNode errorLinesNode = _xmlResult.CreateElement("ErrorLines");
            detailsNode.AppendChild(errorLinesNode);
            XmlAttribute attCntErr = _xmlResult.CreateAttribute("CountLinks");
            errorLinesNode.Attributes.Append(attCntErr);
            attCntErr.InnerText = _importEvent.TotalErrorLine.ToString();

            // Message d'erreur
            errorLinesNode.InnerText = _importEvent.ErrorContainer == null ?
                String.Empty : _importEvent.ErrorContainer.Msg + Environment.NewLine + _importEvent.ErrorContainer.Detail;

            // Poubelle ?
            /*
            XmlNode paramNode = _xmlResult.CreateElement("param");
            _detailsNode.AppendChild(paramNode);
            XmlNode errorNode = _xmlResult.CreateElement("GlobalError");
            _detailsNode.AppendChild(errorNode);
            */

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        /// <summary>
        /// Fait le rendu des valeur de la progress bar
        /// </summary>
        private void RenderProgress()
        {
            _xmlResult = new XmlDocument();
            XmlNode mainNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(mainNode);

            XmlNode detailsNode = _xmlResult.CreateElement("Result");
            _xmlResult.AppendChild(detailsNode);

            XmlNode TotalLine = _xmlResult.CreateElement("TotalLine");
            detailsNode.AppendChild(TotalLine);

            XmlNode TotalSuccessLine = _xmlResult.CreateElement("TotalSuccessLine");
            detailsNode.AppendChild(TotalSuccessLine);

            XmlNode TotalErrorLine = _xmlResult.CreateElement("TotalErrorLine");
            detailsNode.AppendChild(TotalErrorLine);

            XmlNode TotalProcessedLine = _xmlResult.CreateElement("TotalProcessedLine");
            detailsNode.AppendChild(TotalProcessedLine);

            XmlNode Percent = _xmlResult.CreateElement("Percent");
            detailsNode.AppendChild(Percent);

            if (_context.Session["ImportEventArgs"] != null)
            {
                eImportEventArgs import = (eImportEventArgs)_context.Session["ImportEventArgs"];

                TotalLine.InnerText = import.TotalLine.ToString();
                TotalSuccessLine.InnerText = import.TotalSucessLine.ToString();
                TotalErrorLine.InnerText = import.TotalErrorLine.ToString();
                TotalProcessedLine.InnerText = import.TotalLineProcessed.ToString();

                if (import.TotalLine > 0)
                    Percent.InnerText = ((Int32)import.TotalLineProcessed / import.TotalLine).ToString();
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

    }
}