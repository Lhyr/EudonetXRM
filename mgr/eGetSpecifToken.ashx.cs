using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// obtient un token à transmettre aux specif
    /// </summary>
    public class eGetSpecifToken : eEudoManager
    {
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 nSpecifId = 0;
            Int32 nFileId = 0;
            Int32 nTab = 0;
            Int32 nParentTab = 0;
            Int32 nParentFileId = 0;
            Int32 nDescId = 0;

            if (!_requestTools.AllKeys.Contains("sid") || !Int32.TryParse(_context.Request.Form["sid"].ToString(), out nSpecifId))
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, "", "sid non fourni");
            }
            if (_requestTools.AllKeys.Contains("fid"))
                Int32.TryParse(_context.Request.Form["fid"].ToString(), out nFileId);
            if (_requestTools.AllKeys.Contains("tab"))
                Int32.TryParse(_context.Request.Form["tab"].ToString(), out nTab);
            if (_requestTools.AllKeys.Contains("parenttab"))
                Int32.TryParse(_context.Request.Form["parenttab"].ToString(), out nParentTab);
            if (_requestTools.AllKeys.Contains("parentfid"))
                Int32.TryParse(_context.Request.Form["parentfid"].ToString(), out nParentFileId);
            if (_requestTools.AllKeys.Contains("descid"))
                Int32.TryParse(_context.Request.Form["descid"].ToString(), out nDescId);

            String sToken = "";
        
            eSpecif spec = eSpecif.GetSpecif(_pref, nSpecifId);

            bool bTokError = true;
            if (spec != null && spec.IsViewable)
            {
                eSpecifToken token = eSpecifToken.GetSpecifTokenXRM(_pref, spec, nTab, nFileId, nParentTab, nParentFileId, nDescId);
                sToken = spec.IsStatic ? HttpUtility.UrlEncode( token.ShortToken ): token.Token;

                bTokError = token.IsError;
            }

            XmlDocument xmlResult;

            LaunchError();

            xmlResult = new XmlDocument();
            XmlNode xmlroot = xmlResult.CreateElement("Root");
            xmlResult.AppendChild(xmlroot);

            XmlNode xmlSuccess = xmlResult.CreateElement("success");
            xmlroot.AppendChild(xmlSuccess);

            XmlNode xmlToken = xmlResult.CreateElement("token");
            xmlroot.AppendChild(xmlToken);

            XmlNode xmlLabel = xmlResult.CreateElement("label");
            xmlroot.AppendChild(xmlLabel);

            XmlNode xmlUrl = xmlResult.CreateElement("url");
            xmlroot.AppendChild(xmlUrl);


            XmlNode xmlUrlParam = xmlResult.CreateElement("urlparam");
            xmlroot.AppendChild(xmlUrlParam);

            XmlNode xmlOpenMode = xmlResult.CreateElement("openmode");
            xmlroot.AppendChild(xmlOpenMode);

            XmlNode xmlErr = xmlResult.CreateElement("error");
            xmlroot.AppendChild(xmlErr);

            XmlNode xmlStatic = xmlResult.CreateElement("static");
            xmlroot.AppendChild(xmlStatic);


            if (spec != null && spec.IsViewable && !bTokError)
            {
                xmlLabel.AppendChild(xmlResult.CreateTextNode(spec.Label));
                xmlUrl.AppendChild(xmlResult.CreateTextNode(spec.GetRelativeUrlFromRoot(_pref)));
                xmlUrlParam.AppendChild(xmlResult.CreateTextNode(spec.UrlParam));
                xmlOpenMode.AppendChild(xmlResult.CreateTextNode(spec.OpenMode.GetHashCode().ToString()));
                xmlSuccess.AppendChild(xmlResult.CreateTextNode("1"));
                xmlStatic.AppendChild(xmlResult.CreateTextNode(spec.IsStatic ? "1" : "0"));
                xmlToken.AppendChild(xmlResult.CreateTextNode(sToken));
            }
            else
            {
                xmlSuccess.AppendChild(xmlResult.CreateTextNode("0"));
                xmlErr.AppendChild(xmlResult.CreateTextNode(eResApp.GetRes(_pref, 149)));
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }



    }
}