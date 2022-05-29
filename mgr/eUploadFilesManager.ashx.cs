using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Xrm.IRISBlack.Model;
using EudoCommonHelper;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Gestion de l'upload de fichier
    /// </summary>
    public class eUploadFilesManager : eEudoManager
    {
        /// <summary>
        /// Gestionaire d'upload
        /// </summary>
        protected override void ProcessManager()
        {
            UploadFileModel filesToUpload = new UploadFileModel();

            string sPrefix = "tmps" + _pref.UserId.ToString();
            string sTmpFolder = eModelTools.GetTempDatasDirectory(_pref, sPrefix);

            try
            {
                if (_context.Request.Form["FileID"] != null)
                {
                    filesToUpload.FileId = eLibTools.GetNum(_context.Request.Form["FileID"].ToString());
                }
                if (_context.Request.Form["Tab"] != null)
                {
                    filesToUpload.Tab = eLibTools.GetNum(_context.Request.Form["Tab"].ToString());
                }

                //parsage des mails envoyés
                List<EmailValidationStatusResult> lstResults = new List<EmailValidationStatusResult>();
                try
                {
                    lstResults = eModelTools.CheckBounceFromHTTPUpload(_context.Request.Files, _pref, _pref.UserId);
                }
                catch (EudoException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw new EudoException("Erreur lors de la vérification des messages non remis - " + e.Message,
                        "Erreur lors de la vérification des messages non remis", e);
                }

                //mise à jour de la table mailstatus
                eLibDataTools.UpdateBouncedMailStatus(_pref, lstResults, true);

                String strError = String.Empty;

                //Création des emails non remis
                eModelTools.CreateBounceMainFromValidationStatus(_pref, lstResults, filesToUpload.FileId, out strError);

                //on retourne le résultat
                RenderResult(_pref, lstResults, strError);
            }
            catch (Exception e)
            {
                if (e is eEndResponseException)
                    return;
                //désérialisation invalide
                throw new EudoException(e.Message, "Données d'upload de fichier non valide", e);
            }
            finally
            {
                //CleanUp le répertoire tmp
                if (sTmpFolder.StartsWith(Path.Combine(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.DATASOURCE, _pref.GetBaseName), sPrefix + ".")))
                    Directory.Delete(sTmpFolder, true);
            }

        }

        /// <summary>
        /// Retouner le resultat après un upload des fichier
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="lstResults"></param>
        /// <param name="error"></param>
        private void RenderResult(ePref pref, List<EmailValidationStatusResult> lstResults, string error)
        {
            XmlDocument _xmlResult = new XmlDocument();

            // BASE DU XML DE RETOUR            
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));


            XmlNode rootNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(rootNode);

            XmlNode successNode = _xmlResult.CreateElement("success");
            rootNode.AppendChild(successNode);

            XmlNode msgNode = _xmlResult.CreateElement("msg");
            rootNode.AppendChild(msgNode);

            if (error.Length > 0)
            {
                successNode.InnerText = "0";
                msgNode.InnerText = error;
            }
            else if (lstResults.Where(r => !r.AnalyseDone || !r.IdentifiedByBounce).Count() > 0)
            {
                successNode.InnerText = "0";
                msgNode.InnerText = eResApp.GetRes(pref, 2936);
            }
            else
            {
                successNode.InnerText = "1";
                msgNode.InnerText = string.Empty;
            }

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }


    }
}