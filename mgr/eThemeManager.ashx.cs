using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eThemeManager
    /// </summary>
    public class eThemeManager : eEudoManager
    {
        private string _action = String.Empty;

        protected override void ProcessManager()
        {
            if (_pref.User.UserId == 0)
            {
                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 6562),
                    eResApp.GetRes(_pref, 72),
                    "eThemeManager.ashx : Paramètre UserId manquant"));
            }

            XmlDocument xmlResult = new XmlDocument();

            // Init le document XML
            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode detailsNode = xmlResult.CreateElement("thememanager");
            xmlResult.AppendChild(detailsNode);

            XmlNode successNode = xmlResult.CreateElement("success");
            detailsNode.AppendChild(successNode);

            XmlNode resultNode = xmlResult.CreateElement("result");
            detailsNode.AppendChild(resultNode);

            // Param
            _action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;

            if(_action == "getThemeCssUrl")
            {
                int themeId = _pref.ThemeXRM.Id;
                int themeVersion = _pref.ThemeXRM.Version;
                string themeFolder = _pref.ThemeXRM.Folder;
                string themeColor = _pref.ThemeXRM.Color;               
                string sCSSVersion = String.Concat(eConst.VERSION, ".", eConst.REVISION);
                string fileWebName = eTools.WebPathCombine("themes", themeFolder, "css", "theme.css");
                string cssUrl = String.Concat(fileWebName, "?ver=", sCSSVersion);

                XmlNode themeIdNode = xmlResult.CreateElement("themeId");
                detailsNode.AppendChild(themeIdNode);

                XmlNode themeVersionNode = xmlResult.CreateElement("themeVersion");
                detailsNode.AppendChild(themeVersionNode);

                XmlNode urlNode = xmlResult.CreateElement("url");
                detailsNode.AppendChild(urlNode);

                XmlNode themeFolderNode = xmlResult.CreateElement("themeFolder");
                detailsNode.AppendChild(themeFolderNode);

                XmlNode themeMainColorNode = xmlResult.CreateElement("themeMainColor");
                detailsNode.AppendChild(themeMainColorNode);
                               
                themeIdNode.InnerText = themeId.ToString();
                urlNode.InnerText = cssUrl;
                themeFolderNode.InnerText = themeFolder;
                themeMainColorNode.InnerText = themeColor;
                themeVersionNode.InnerText = themeVersion.ToString();

                successNode.InnerText = "1";
            }
            else
            {
                LaunchError(eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 72),
                    "eThemeManager.ashx : action non prise en charge"));
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }
    }
}