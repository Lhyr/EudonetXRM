using Com.Eudonet.Internal;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// ePdfFormManager : analyse le formulaire PDF et retourne la liste des champs du formulaire
    /// </summary>
    public class ePdfFormManager : eEudoManager
    {
        String _filename;
        List<String> _pdfFields;

        /// <summary>
        /// Gestion des préférences de bookmark
        /// </summary>
        protected override void ProcessManager()
        {
            String errorDescription = String.Empty;
            Boolean updateSuccess = false;
            _pdfFields = new List<String>();

            if (_requestTools.AllKeys.Contains("filename"))
            {
                String sDatasPath = eModelTools.GetPhysicalDatasPath(_context, eLibConst.FOLDER_TYPE.MODELES, _pref.GetBaseName);

                if (!String.IsNullOrEmpty(_context.Request.Form["filename"].ToString()))
                {
                    String sFilePath = String.Concat(sDatasPath, @"\", _context.Request.Form["filename"].ToString());

                    try
                    {
                        LoadPDFFormFields(sFilePath);
                    }
                    catch (Exception exc)
                    {
                        errorDescription = exc.Message;
                    }
                }
                else
                {
                    errorDescription = "Aucun fichier configuré";
                }
                
            }

            // Init le document XML
            XmlDocument xmlResult = new XmlDocument();
            XmlNode _mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(_mainNode);
            XmlNode pdfResult = xmlResult.CreateElement("pdfresult");
            xmlResult.AppendChild(pdfResult);

            XmlNode listfields = xmlResult.CreateElement("pdffields");
            // Ajout des champs PDF
            if (_pdfFields != null && _pdfFields.Count > 0)
            {
                foreach (String field in _pdfFields)
                {
                    XmlNode _fieldNode = xmlResult.CreateElement("field");
                    _fieldNode.InnerText = field.ToLower();
                    listfields.AppendChild(_fieldNode);
                }
            }
            
            

            pdfResult.AppendChild(listfields);

            XmlNode _resultNode = xmlResult.CreateElement("result");
            if (String.IsNullOrEmpty(errorDescription))
            {

                _resultNode.InnerText = "SUCCESS";
                pdfResult.AppendChild(_resultNode);
            }
            else
            {
                _resultNode.InnerText = "ERROR";
                pdfResult.AppendChild(_resultNode);

                if (!String.IsNullOrEmpty(errorDescription))
                {
                    XmlNode _errDesc = xmlResult.CreateElement("errordescription");
                    _errDesc.InnerText = errorDescription;
                    pdfResult.AppendChild(_errDesc);
                }

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    errorDescription,
                    eResApp.GetRes(_pref, 72),
                    errorDescription
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });
        }

        /// <summary>Charge le PDF et retourne la liste des champs formulaire du PDF</summary>
        /// <param name="filepath">Chemin complet du fichier</param>
        /// <returns></returns>
        private void LoadPDFFormFields(String filepath)
        {
            try
            {
                Stream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);

                PdfLoadedDocument pdfDoc = new PdfLoadedDocument(fileStream);
                PdfLoadedForm form = pdfDoc.Form;

                foreach (PdfLoadedField field in form.Fields)
                {
                    _pdfFields.Add(field.Name);
                }
            }
            catch (Exception exc)
            {
                throw new Exception("Erreur lors du chargement du fichier PDF : " + exc.Message);
            }

        }
    }
}