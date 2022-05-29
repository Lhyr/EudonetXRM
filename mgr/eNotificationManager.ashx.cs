using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using System;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Manager qui gère les requetes AJAX pour les notifications
    /// </summary>
    public class eNotificationManager : eEudoManagerReadOnly
    {
        /// <summary>Appelé à l'appel de la page</summary>
        protected override void ProcessManager()
        {
            bool post = _requestTools.GetRequestFormKeyB("post") ?? false;
            String action = _requestTools.GetRequestFormKeyS("action") ?? string.Empty;
            Int32 nNotifId = _requestTools.GetRequestFormKeyI("notifid") ?? 0;

            Int32 nNbRows = _requestTools.GetRequestFormKeyI("nbrows") ?? -1;
            Int32 nIndexLower = _requestTools.GetRequestFormKeyI("indexlower") ?? -1;
            Int32 nIndexUpper = _requestTools.GetRequestFormKeyI("indexupper") ?? -1;
            bool unread = _requestTools.GetRequestFormKeyB("unread") ?? false;

            if (!post)
            {
                action = "getList";
                if (!String.IsNullOrEmpty(_context.Request.QueryString["action"]))
                {
                    action = _context.Request.QueryString["action"];
                }

                nNbRows = 10;
                if (!String.IsNullOrEmpty(_context.Request.QueryString["nbrows"]))
                {
                    Int32.TryParse(_context.Request.QueryString["nbrows"], out nNbRows);
                }

                nIndexLower = 1;
                if (!String.IsNullOrEmpty(_context.Request.QueryString["indexlower"]))
                {
                    Int32.TryParse(_context.Request.QueryString["indexlower"], out nIndexLower);
                }

                nIndexUpper = 10;
                if (!String.IsNullOrEmpty(_context.Request.QueryString["indexupper"]))
                {
                    Int32.TryParse(_context.Request.QueryString["indexupper"], out nIndexUpper);
                }

                unread = false;
                if (!String.IsNullOrEmpty(_context.Request.QueryString["unread"]))
                {
                    Boolean.TryParse(_context.Request.QueryString["unread"], out unread);
                }
            }

            if (action == "getList" && nIndexLower != -1 && nIndexUpper != -1)
            {
                doList(nIndexLower, nIndexUpper, unread);
            }
            else if (action == "getToaster")
            {
                doToasters();
            }
            else if (action == "tagRead" && nNotifId != 0)
            {
                doTagRead(nNotifId);
            }
            else if (action == "tagAllRead")
            {
                doTagAllRead();
            }
            else if (action == "unsubscribe" && nNotifId != 0)
            {
                doUnsubscribe(nNotifId);
            }
        }

        /// <summary>
        /// Génération d'une liste de notifications
        /// </summary>
        /// <param name="indexLower">Index notifications début</param>
        /// <param name="indexUpper">Index notifications fin</param>
        /// <param name="unread">Indique si on veut uniquement les notifications non lues</param>
        /// 
        private void doList(int indexLower, int indexUpper, bool unread)
        {
            eNotificationListRenderer notificationList = eRendererFactory.CreateNotificationListRenderer(_pref, indexLower, indexUpper, unread) as eNotificationListRenderer;
            RenderResult(RequestContentType.XML, delegate () { return notificationList.XMLDoc.OuterXml; });
        }

        /// <summary>
        /// Génération d'une liste de notifications
        /// </summary>
        private void doToasters()
        {
            eNotificationToastersRenderer notificationToasters = eRendererFactory.CreateNotificationToastersRenderer(_pref) as eNotificationToastersRenderer;
            RenderResult(RequestContentType.XML, delegate () { return notificationToasters.XMLDoc.OuterXml; });
        }

        /// <summary>
        /// Marquer une notification comme lue
        /// </summary>
        /// <param name="nNotifId">Id de la notification</param>
        private void doTagRead(Int32 nNotifId)
        {
            string error;
            bool success = eNotification.TagRead(_pref, nNotifId, out error);

            doTagReadRenderResult(success, error);
        }

        /// <summary>
        /// Marquer toutes les notifications comme lues
        /// </summary>
        private void doTagAllRead()
        {
            eList _list = eListFactory.CreateListNotificationUnread(_pref, 0);

            string error = String.Empty;
            bool success = false;

            if (_list.ListRecords.Count > 0)
            {
                foreach (eRecord row in _list.ListRecords)
                {
                    Int32 notifId = row.MainFileid;

                    success = eNotification.TagRead(_pref, notifId, out error);

                    if (!success)
                        break;
                }
            }
            else
            {
                error = String.Empty;
                success = true;
            }

            doTagReadRenderResult(success, error);
        }

        private void doTagReadRenderResult(bool bSuccess, string sError)
        {
            XmlDocument xmlResult = new XmlDocument();

            xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = bSuccess.ToString();
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = sError;
            baseResultNode.AppendChild(errorResultNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        /// <summary>
        /// se désabonner à un type de notification
        /// </summary>
        /// <param name="nNotifId">Id de la notification</param>
        private void doUnsubscribe(Int32 nNotifId)
        {
            string error = "Not implemented";
            bool success = false;

            doTagReadRenderResult(success, error);
        }
    }
}