using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eNotificationListRenderer : eRenderer
    {
        ///// <summary>Nombre de notifications</summary>
        //protected Int32 _rows;

        /// <summary>Index notification de début</summary>
        private int _indexLower;
        /// <summary>Index notification de fin</summary>
        private int _indexUpper;

        /// <summary>Objet d'accès aux données</summary>
        private eList _list;
        public eNotificationList _listCountUnread;

        private bool _unreadMode = false;

        /// <summary>Xml rendu par le renderer</summary>
        protected XmlDocument _xmlDoc = new XmlDocument();

        /// <summary>
        /// Conteneur principal du renderer
        /// </summary>
        public XmlDocument XMLDoc
        {
            get { return _xmlDoc; }
        }

        /// <summary>
        /// Constructeur par défaut avec uniquement pref
        /// Base des classe dérivées
        /// </summary>
        /// <param name="pref"></param>
        private eNotificationListRenderer(ePref pref)
        {
            Pref = pref;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static eNotificationListRenderer GetNotificationListRenderer(ePref pref, /*Int32 nRows,*/ int indexLower = 1, int indexUpper = 10, bool unreadMode = false)
        {
            eNotificationListRenderer renderer = new eNotificationListRenderer(pref);
            //renderer._rows = nRows;
            renderer._unreadMode = unreadMode;
            renderer._indexLower = indexLower;
            renderer._indexUpper = indexUpper;

            return renderer;
        }


        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected virtual void GenerateList()
        {
            Int32 nRows = _indexUpper + 10;

            if (_unreadMode)
            {
                _list = eListFactory.CreateListNotificationUnread(Pref, nRows);
            }
            else
            {
                _list = eListFactory.CreateListNotification(Pref, nRows);
            }

            _listCountUnread = eListFactory.CreateListNotificationCountUnread(Pref) as eNotificationList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            //Génération de l'objet _list
            GenerateList();

            return true;
        }

        protected override bool Build()
        {


            XmlDocument xmlResult = new XmlDocument();

            xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode UnreadNode = xmlResult.CreateElement("CountUnread");
            UnreadNode.InnerText = _listCountUnread.GetCountUnread.ToString();
            baseResultNode.AppendChild(UnreadNode);

            /*
            XmlNode ResTagReadNode = xmlResult.CreateElement("ResTagRead");
            ResTagReadNode.InnerText = eResApp.GetRes(Pref, 6877);
            baseResultNode.AppendChild(ResTagReadNode);
            
            XmlNode ResNoNotification = xmlResult.CreateElement("ResNoNotification");
            ResNoNotification.InnerText = eResApp.GetRes(Pref, 6880);
            baseResultNode.AppendChild(ResNoNotification);
            */

            XmlNode listNotificationsNode = xmlResult.CreateElement("ListNotifications");

            int previousFetchCount = 0;
            int nextFetchCount = 0;

            if (_list.ListRecords.Count > 0)
            {
                string aliasTab = EudoQuery.TableType.NOTIFICATION.GetHashCode().ToString();
                string alias = String.Empty;

                int itemIndex = 1;
                foreach (eRecord row in _list.ListRecords)
                {
                    if (itemIndex < _indexLower)
                    {
                        ++previousFetchCount;
                    }
                    else if (itemIndex > _indexUpper)
                    {
                        ++nextFetchCount;
                    }
                    else
                    {


                        //récupère valeurs de la bdd
                        Int32 notifId = row.MainFileid;

                        alias = aliasTab + "_" + EudoQuery.NotificationField.TYPE.GetHashCode().ToString();
                        int type = eLibTools.GetNum(row.GetFieldByAlias(alias).Value);

                        alias = aliasTab + "_" + EudoQuery.NotificationField.TITLE.GetHashCode().ToString();
                        string title = row.GetFieldByAlias(alias).DisplayValue;

                        alias = aliasTab + "_" + EudoQuery.NotificationField.DESCRIPTION.GetHashCode().ToString();
                        string description = row.GetFieldByAlias(alias).Value;

                        alias = aliasTab + "_" + EudoQuery.NotificationField.DESCRIPTION_LONG.GetHashCode().ToString();
                        string descriptionLong = row.GetFieldByAlias(alias).Value;

                        alias = aliasTab + "_" + EudoQuery.NotificationField.NOTIFICATION_DATE.GetHashCode().ToString();
                        DateTime notifDate = eDate.ConvertDisplayToBddDt(Pref.CultureInfo, row.GetFieldByAlias(alias).DisplayValue);

                        TimeSpan timeSpan = DateTime.Now - notifDate;
                        string timeSpanStr = eNotification.GetNotificationTimeSpanString(Pref, timeSpan);

                        alias = aliasTab + "_" + EudoQuery.NotificationField.LUE.GetHashCode().ToString();
                        Boolean read = row.GetFieldByAlias(alias).Value == "1";

                        alias = aliasTab + "_" + EudoQuery.NotificationField.COLOR.GetHashCode().ToString();
                        string color = row.GetFieldByAlias(alias).Value;

                        //Image     
                        bool isPicto = false;
                        string image = "https://cdn0.iconfinder.com/data/icons/basic-ui-elements-colored/700/09_bell-3-128.png";

                        alias = aliasTab + "_" + EudoQuery.NotificationField.IMAGE.GetHashCode().ToString();
                        String img = row.GetFieldByAlias(alias).Value;
                        if (!String.IsNullOrEmpty(img))
                        {
                            image = eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.NOTIFICATION, Pref.GetBaseName) + "/" + img;
                        }
                        else
                        {
                            alias = aliasTab + "_" + EudoQuery.NotificationField.ICON.GetHashCode().ToString();
                            img = row.GetFieldByAlias(alias).Value;
                            image = eFontIcons.GetFontClassName(img);
                            isPicto = true;
                        }


                        //rendu XML
                        XmlNode notificationNode = xmlResult.CreateElement("notification");

                        XmlNode idNode = xmlResult.CreateElement("id");
                        idNode.InnerText = notifId.ToString();
                        notificationNode.AppendChild(idNode);

                        XmlNode typeNode = xmlResult.CreateElement("type");
                        typeNode.InnerText = type.ToString();
                        notificationNode.AppendChild(typeNode);

                        /*
                        XmlNode titleNode = xmlResult.CreateElement("title");
                        titleNode.InnerText = title;
                        notificationNode.AppendChild(titleNode);
                        */

                        XmlNode descriptionNode = xmlResult.CreateElement("description");
                        descriptionNode.InnerText = descriptionLong;
                        notificationNode.AppendChild(descriptionNode);

                        XmlNode iconNode = xmlResult.CreateElement("icon");
                        iconNode.InnerText = image;
                        notificationNode.AppendChild(iconNode);

                        XmlNode pictoNode = xmlResult.CreateElement("picto");
                        pictoNode.InnerText = isPicto ? "1" : "0";
                        notificationNode.AppendChild(pictoNode);

                        XmlNode timespanNode = xmlResult.CreateElement("timespan");
                        timespanNode.InnerText = timeSpanStr;
                        notificationNode.AppendChild(timespanNode);


                        XmlNode colorNode = xmlResult.CreateElement("color");
                        colorNode.InnerText = color;
                        notificationNode.AppendChild(colorNode);

                        XmlNode readNode = xmlResult.CreateElement("read");
                        readNode.InnerText = read ? "true" : "false";
                        notificationNode.AppendChild(readNode);

                        listNotificationsNode.AppendChild(notificationNode);
                    }

                    ++itemIndex;
                }
            }



            XmlNode previousFetchCountNode = xmlResult.CreateElement("PreviousFetchCount");
            previousFetchCountNode.InnerText = previousFetchCount.ToString();
            baseResultNode.AppendChild(previousFetchCountNode);

            XmlNode nextFetchCountNode = xmlResult.CreateElement("NextFetchCount");
            nextFetchCountNode.InnerText = nextFetchCount.ToString();
            baseResultNode.AppendChild(nextFetchCountNode);

            baseResultNode.AppendChild(listNotificationsNode);

            _xmlDoc = xmlResult;

            return true;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            return base.End();
        }
    }
}