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
    /// <className>eTargetProgressManager</className>
    /// <summary>Progression d'import des cibles étendues</summary>
    /// <authors>Madjid</authors>
    /// <date>14/05/2018</date>
    public class eTargetProgressManager : eEudoManagerReadOnly
    {
        private eImportParams _importParams;
       
        /// <summary>
        /// Chargement de la progression
        /// </summary>
        protected override void ProcessManager()
        {
            _importParams = new eImportParams(_requestTools);

            switch (_importParams.Action)
            {               
                case eImportAction.CHECK_PROGRESS:
                    RenderProgress();

                    break;
                case eImportAction.DO_NOTHING:
                default:
                    break;
            }
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
            else
            {
                TotalLine.InnerText =  "0";
                TotalSuccessLine.InnerText = "0";
                TotalErrorLine.InnerText = "0";
                TotalProcessedLine.InnerText = "0";
                Percent.InnerText = "0"; 
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }
    }
}