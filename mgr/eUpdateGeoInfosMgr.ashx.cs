using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eUpdateGeoInfosMgr
    /// </summary>
    public class eUpdateGeoInfosMgr : eEudoManager
    {

        /// <summary>
        /// Gestion des préférences de bookmark
        /// </summary>
        protected override void ProcessManager()
        {


            _xmlResult = new XmlDocument();

            // BASE DU XML DE RETOUR            
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));


            XmlNode rootNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(rootNode);

            XmlNode successNode = _xmlResult.CreateElement("success");
            rootNode.AppendChild(successNode);

            XmlNode msgNode = _xmlResult.CreateElement("msg");
            rootNode.AppendChild(msgNode);

            if (!_requestTools.AllKeysQS.Contains("tab"))
            {
                successNode.InnerText = "0";
                msgNode.InnerText = "Pas de Tab renseigné";
                RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });
                return;
            }

            Int32 nTab = 0;
            Int32.TryParse(_context.Request.QueryString["tab"], out nTab);

            eUpdateGeoInfos ugi = new eUpdateGeoInfos(_pref, nTab);
            String sError = "", sMsg = "";
            if (ugi.LaunchUpdate(out sError, out sMsg))
                successNode.InnerText = "1";
            else
                successNode.InnerText = "0";
            
            msgNode.InnerText = String.Concat(sError,Environment.NewLine, sMsg  );


            RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });

        }
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}