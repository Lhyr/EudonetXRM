using System;
using Com.Eudonet.Internal;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// DEVELOPPEUR	: SPH
    /// DATE			: 10/06/2013
    /// DESCRIPTION 			: Transfert Spécif XRM -> V7
    ///   	Ce manager construit un flux xml contenant les nodes ::
    ///   	 -> token : Le token de connexion pour la v7
    ///   	 -> baseurl : l'url d'appel de la page de transfer v7
    ///   	 -> type : le type de spécif
    /// </summary>
    public class eExportToV7 : eEudoManager
    {

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            //
            String error = String.Empty;

            Int32 nUrlId = 0;
            String sUrl = String.Empty;


            Int32 nType = 0;
            eLibConst.SPECIF_TYPE cType = eLibConst.SPECIF_TYPE.TYP_SPECIF;



            if (!_allKeys.Contains("urlid"))
            {

                String sDevMsg = "pas de spécif";

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError();
            }


            sUrl = _context.Request.Form["urlid"].ToString();

            //
            if (!(_allKeys.Contains("type") && Int32.TryParse(_context.Request.Form["type"], out nType) && nType >= 1 && nType <= 7))
            {


                String sDevMsg = "type de spécif invalide";

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError();
            }

            cType = (eLibConst.SPECIF_TYPE)nType;

            //Si type spécif classique ou favoiris, 
            if (cType == eLibConst.SPECIF_TYPE.TYP_SPECIF || cType == eLibConst.SPECIF_TYPE.TYP_FAVORITE)
                Int32.TryParse(sUrl, out nUrlId);

            Int32 nTab = 0;
            if (_allKeysQS.Contains("tab"))
                Int32.TryParse(_context.Request.QueryString["tab"], out nTab);

            Int32 nFileId = 0;
            if (_allKeysQS.Contains("fileid"))
                Int32.TryParse(_context.Request.QueryString["fileid"], out nFileId);






            eSpecifToken es = eSpecifToken.GetSpecifTokenV7(_pref, cType, sUrl, nTab, nFileId);
            if (es.IsError)
            {

                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);

                if (es.InnerException != null)
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", es.InnerException.Message, Environment.NewLine, "Exception StackTrace :", es.InnerException.StackTrace);
                }
                else
                {
                    sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Message d'erreur : ", es.ErrorMsg);
                }

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));

                LaunchError();
            }
            else
            {

                XmlDocument xmlResult = new XmlDocument();
                XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlResult.AppendChild(mainNode);

                XmlNode rootNode = xmlResult.CreateElement("root");
                xmlResult.AppendChild(rootNode);

                XmlNode successNode = xmlResult.CreateElement("success");
                rootNode.AppendChild(successNode);
                successNode.InnerText = "1";


                XmlNode tokenNode = xmlResult.CreateElement("token");
                rootNode.AppendChild(tokenNode);
                tokenNode.InnerText = eLibTools.CleanXMLChar(es.Token);


                XmlNode urlNode = xmlResult.CreateElement("baseurl");
                rootNode.AppendChild(urlNode);
                urlNode.InnerText = eLibTools.CleanXMLChar(es.BaseSiteV7URL);

                XmlNode typeNode = xmlResult.CreateElement("type");
                rootNode.AppendChild(typeNode);
                typeNode.InnerText = eLibTools.CleanXMLChar(cType.GetHashCode().ToString());


                RenderResult(RequestContentType.XML, delegate()
                {
                    return xmlResult.OuterXml;
                });
            }

            return;
        }


    }
}