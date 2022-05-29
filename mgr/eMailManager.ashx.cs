using System;
using Com.Eudonet.Internal;
using System.Text;
using EudoQuery;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Gestion des traitements asynchrones de l'envoi d'e-mails ou SMS
    /// </summary>
    public class eMailManager : eEudoManager
    {
        /// <summary>
        /// Gestion des traitements asynchrones de l'envoi d'e-mails ou SMS
        /// </summary>
        protected override void ProcessManager()
        {
            eMailTemplate.Operation _operation;
            int _iOp = 0;
            int _iMailTemplateId = 0;
            int _iDescId = 0;
            int _iTargetId = 0;
            String _sMailTemplateValue = _context.Request.Form["MailTemplateValue"];
            String _sErr = String.Empty;
            eudoDAL _eDAL = eLibTools.GetEudoDAL(_pref);
            XmlDocument _xmlDocReturn = new XmlDocument();
            StringBuilder _sbError = new StringBuilder();

            if (!int.TryParse(_context.Request.Form["operation"].ToString(), out _iOp))
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "operation"));
                LaunchError();
            }

            _operation = (eMailTemplate.Operation)_iOp;

            if (
                !int.TryParse(_context.Request.Form["MailTemplateId"].ToString(), out _iMailTemplateId) ||
                _sMailTemplateValue == null
            )
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "MailTemplateId"));
                LaunchError();


            }
            
            try
            {
                _eDAL.OpenDatabase();
              
                eMailTemplate mailTemplate = new eMailTemplate(_eDAL, _pref.User, _iDescId, _iTargetId, _iMailTemplateId, out _sErr);

                switch (_operation)
                {
                    case eMailTemplate.Operation.Insert:
                        if (mailTemplate.AddAllowed)
                        {
                            mailTemplate.Insert(out _sErr);
                            _xmlDocReturn = mailTemplate.GetXmlReportDocument(_operation, _sErr);
                            if (!String.IsNullOrEmpty(_sErr))
                                _sbError.AppendLine(_sErr);
                        }
                        else
                            _sbError.AppendLine("Ajout non autorisé");
                        break;
                    case eMailTemplate.Operation.Update:
                        if (mailTemplate.UpdateAllowed)
                            _eDAL.StartTransaction(out _sErr);
                        else
                            _sErr = "Modification non autorisée";

                        if (!String.IsNullOrEmpty(_sErr))
                        {
                            _sbError.AppendLine(_sErr);
                            _xmlDocReturn = mailTemplate.Update(out _sErr);
                        }
                        break;
                    default:
                        break;
                }

                if (_eDAL.HasActiveTransaction)
                    _eDAL.CommitTransaction(out _sErr);
            }
            catch (Exception ex)
            {
                _eDAL.RollBackTransaction(out _sErr);
                _sErr = string.Concat( ex.Message , " - " ,_sErr);
                if (!String.IsNullOrEmpty(_sErr))
                    _sbError.AppendLine(_sErr);

                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "", "", _sbError.ToString());
                LaunchError();

            }
            finally
            {
                _eDAL.CloseDatabase();

                RenderResult(RequestContentType.XML, delegate() { return _xmlDocReturn.OuterXml; });
            }
        }
    }
}