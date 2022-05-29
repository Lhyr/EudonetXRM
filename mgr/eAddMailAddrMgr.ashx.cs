using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using EudoQuery;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Manager permettant de gérer la fenêtre d'ajout de destinataire (pour les mails unitaires)
    /// </summary>
    public class eAddMailAddrMgr : eEudoManager
    {
        /// <summary>
        /// Manager permettant de gérer la fenêtre d'ajout de destinataire (pour les mails unitaires)
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 _ppid = 0, _adrid = 0, _pmid = 0;
            Int32 _fileType = EdnType.FILE_MAIL.GetHashCode();
            List<FieldFormat> requiredFieldFormats = new List<FieldFormat>() { FieldFormat.TYP_EMAIL };
            List<UserField> requiredUserFields = new List<UserField>() { UserField.EMAIL };
            eAddMailAddrData data = new eAddMailAddrData(_pref);

            if (_requestTools.AllKeys.Contains("ppid") && !String.IsNullOrEmpty(_context.Request.Form["ppid"]))
                Int32.TryParse(_context.Request.Form["ppid"].ToString(), out _ppid);

            if (_requestTools.AllKeys.Contains("pmid") && !String.IsNullOrEmpty(_context.Request.Form["pmid"]))
                Int32.TryParse(_context.Request.Form["pmid"].ToString(), out _pmid);

            if (_requestTools.AllKeys.Contains("adrid") && !String.IsNullOrEmpty(_context.Request.Form["adrid"]))
                Int32.TryParse(_context.Request.Form["adrid"].ToString(), out _adrid);

            if (_requestTools.AllKeys.Contains("Search") && !String.IsNullOrEmpty(_context.Request.Form["Search"]))
                data.Search = _context.Request.Form["Search"].ToString();

            if (_requestTools.AllKeys.Contains("filetype") && !String.IsNullOrEmpty(_context.Request.Form["filetype"]))
                Int32.TryParse(_context.Request.Form["filetype"].ToString(), out _fileType);

            data.PPID = _ppid;
            data.PMID = _pmid;
            data.ADRID = _adrid;
            data.FileType = _fileType;

            if (data.FileType == EdnType.FILE_SMS.GetHashCode())
            {
                data.RequiredFieldFormats = new List<FieldFormat>() { FieldFormat.TYP_PHONE };
                data.RequiredUserFields = new List<UserField>() { UserField.TEL, UserField.MOBILE };
            }
            else
            {
                data.RequiredFieldFormats = new List<FieldFormat>() { FieldFormat.TYP_EMAIL };
                data.RequiredUserFields = new List<UserField>() { UserField.EMAIL };
            }

            data.LaunchSearch();

            _xmlResult = new XmlDocument();
            XmlNode maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            _xmlResult.AppendChild(maintNode);
            XmlNode rootNode = _xmlResult.CreateElement("mails");
            _xmlResult.AppendChild(rootNode);

            foreach (String s in data.DataList)
            {
                XmlNode xmlMail = _xmlResult.CreateElement("mail");
                xmlMail.InnerText = s;
                rootNode.AppendChild(xmlMail);
            }

            RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });

        }


    }
}