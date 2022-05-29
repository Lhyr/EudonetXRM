using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eNotificationToastersRenderer : eRenderer
    {
        /// <summary>Objet d'accès aux données</summary>
        public eList _list;
        public eNotificationList _listCountUnread;

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
        private eNotificationToastersRenderer(ePref pref)
        {
            Pref = pref;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static eNotificationToastersRenderer GetNotificationToastersRenderer(ePref pref)
        {
            eNotificationToastersRenderer renderer = new eNotificationToastersRenderer(pref);
            return renderer;
        }


        /// <summary>
        /// Génère l'objet _list du renderer
        /// </summary>
        /// <returns></returns>
        protected virtual bool GenerateList()
        {
            _list = eListFactory.CreateListNotificationToasters(Pref);
            if (_list.ErrorMsg.Length > 0 || _list.InnerException != null)
            {
                _eException = _list.InnerException;
                _sErrorMsg = _list.ErrorMsg;
                return false;
            }
            _listCountUnread = eListFactory.CreateListNotificationCountUnread(Pref) as eNotificationList;
            if (_listCountUnread.ErrorMsg.Length > 0 || _listCountUnread.InnerException != null)
            {
                _eException = _listCountUnread.InnerException;
                _sErrorMsg = _listCountUnread.ErrorMsg;
                return false;
            }


            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            //Génération de l'objet _list
            bool bOk = GenerateList();
            if (!bOk)
            {
                EudoException ex = new EudoException(ErrorMsg, sUserMessage: eResApp.GetRes(Pref, 491), innerExcp: InnerException, launchFeedback: true);
                ex.UserMessageTitle = eResApp.GetRes(Pref, 72);
                ex.UserMessageDetails = eResApp.GetRes(Pref, 492);
                throw ex;
            }
            return bOk;
        }

        /// <summary>
        /// Construit la structure HTML de l'élément
        /// </summary>
        /// <returns></returns>
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

            XmlNode listNotificationsNode = xmlResult.CreateElement("ListNotifications");
            baseResultNode.AppendChild(listNotificationsNode);

            // Les notifications en attente de fusion
            HashSet<Int32> PendingNotifs = new HashSet<Int32>();

            string aliasTab = EudoQuery.TableType.NOTIFICATION.GetHashCode().ToString();
            string alias = String.Empty;

            Int32 nAlertCheckInterval = 0;
            bool bAlertCheckInterval = false;
            Int32 nFuturAlertMinInterval = Int32.MaxValue;
            bool bFuturAlertMinInterval = false;

            foreach (eRecord row in _list.ListRecords)
            {
                //récupère valeurs de la bdd
                Int32 notificationId = row.MainFileid;

                // Les notification en statut Pendings doivent etre fusionner avant l'affichage, on les garde pour 
                alias = aliasTab + "_" + EudoQuery.NotificationField.STATUS.GetHashCode().ToString();
                int status = eLibTools.GetNum(row.GetFieldByAlias(alias).Value);
                if (status == NotifConst.Status.PENDING.GetHashCode())
                {
                    PendingNotifs.Add(notificationId);
                    continue;
                }

                alias = aliasTab + "_" + EudoQuery.NotificationField.NOTIFICATION_DATE.GetHashCode().ToString();
                DateTime date = new DateTime();

                if (DateTime.TryParse(row.GetFieldByAlias(alias).Value, out date))
                {
                    //la notification est dans le passé - on la notifie
                    if (date <= DateTime.Now)
                    {
                        string sError;
                        string notifSound = string.Empty;

                        bool bPostPoneNotif = false;
                        bool bDisplayNotif = false;

                        alias = aliasTab + "_" + EudoQuery.NotificationField.TYPE.GetHashCode().ToString();
                        int type = eLibTools.GetNum(row.GetFieldByAlias(alias).Value);

                        if (type == NotifConst.Type.ALERT.GetHashCode()) //Notifications Type Alert : gestion des alertes reprogrammées
                        {
                            alias = aliasTab + "_" + EudoQuery.NotificationField.PARENT_TAB.GetHashCode().ToString();
                            int parentTab = eLibTools.GetNum(row.GetFieldByAlias(alias).Value);

                            alias = aliasTab + "_" + EudoQuery.NotificationField.PARENT_FILEID.GetHashCode().ToString();
                            int parentFileId = eLibTools.GetNum(row.GetFieldByAlias(alias).Value);

                            if (parentTab != 0 && parentFileId != 0)
                            {
                                //récupérer la date de début de la fiche planning date notification                   
                                DateTime dDateDebut = new DateTime();
                                if (eNotification.GetPlanningFileInfos(Pref, parentTab, parentFileId, out dDateDebut, out notifSound, out sError))
                                {
                                    if (DateTime.Now <= dDateDebut) //la fiche planning n'est pas encore passée
                                    {
                                        //on affiche la notification puis on la reporte
                                        bDisplayNotif = true;
                                        bPostPoneNotif = true;
                                    }
                                    else //la fiche planning est déjà passée
                                    {
                                        //on marque la notification comme toastée mais on ne l'affiche pas
                                        eNotification.MarkAsToasted(Pref, notificationId, dDateDebut, out sError);
                                        bDisplayNotif = false;
                                    }
                                }
                            }
                        }
                        else //autres types de Notifications
                        {
                            bDisplayNotif = true;
                        }

                        if (bDisplayNotif)
                        {
                            if (eNotification.MarkAsToasted(Pref, notificationId, DateTime.Now, out sError))
                            {
                                //récupère valeurs de la bdd
                                alias = aliasTab + "_" + EudoQuery.NotificationField.TITLE.GetHashCode().ToString();
                                string title = row.GetFieldByAlias(alias).DisplayValue;

                                alias = aliasTab + "_" + EudoQuery.NotificationField.DESCRIPTION.GetHashCode().ToString();
                                string description = row.GetFieldByAlias(alias).DisplayValue;

                                alias = aliasTab + "_" + EudoQuery.NotificationField.COLOR.GetHashCode().ToString();
                                string color = row.GetFieldByAlias(alias).Value;

                                //Image
                                //TODO un truc par défaut
                                string image = "icon-catalog";// "https://cdn0.iconfinder.com/data/icons/basic-ui-elements-colored/700/09_bell-3-128.png";
                                bool isPicto = false;
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
                                idNode.InnerText = notificationId.ToString();
                                notificationNode.AppendChild(idNode);

                                XmlNode typeNode = xmlResult.CreateElement("type");
                                typeNode.InnerText = type.ToString();
                                notificationNode.AppendChild(typeNode);

                                XmlNode titleNode = xmlResult.CreateElement("title");
                                titleNode.InnerText = title;
                                notificationNode.AppendChild(titleNode);

                                /*
                                XmlNode descriptionNode = xmlResult.CreateElement("description");
                                descriptionNode.InnerText = description;
                                notificationNode.AppendChild(descriptionNode);
                                */

                                XmlNode iconNode = xmlResult.CreateElement("icon");
                                iconNode.InnerText = image;
                                notificationNode.AppendChild(iconNode);

                                XmlNode pictoNode = xmlResult.CreateElement("picto");
                                pictoNode.InnerText = isPicto ? "1" : "0";
                                notificationNode.AppendChild(pictoNode);

                                XmlNode colorNode = xmlResult.CreateElement("color");
                                colorNode.InnerText = color;
                                notificationNode.AppendChild(colorNode);

                                // On joue un fichier audio pour la notification si paramétrée
                                // par exemple : son alert planning
                                if (!string.IsNullOrWhiteSpace(notifSound))
                                {
                                    XmlNode soundNode = xmlResult.CreateElement("sound");
                                    soundNode.InnerText = notifSound;
                                    notificationNode.AppendChild(soundNode);
                                }

                                listNotificationsNode.AppendChild(notificationNode);
                            }
                        }

                        if (bPostPoneNotif) //On reporte la notif de type alert
                        {
                            if (!bAlertCheckInterval)
                            {
                                bAlertCheckInterval = eNotification.GetAlertCheckInterval(Pref, out nAlertCheckInterval, out sError);

                                if (nAlertCheckInterval == 0)
                                {
                                    nAlertCheckInterval = NotifConst.NOTIFALERTDEFAULTINTERVAL;
                                }
                            }

                            DateTime datePostPoned = DateTime.Now.AddMilliseconds(nAlertCheckInterval);

                            eNotification.PostPone(Pref, notificationId, datePostPoned, NotifConst.Status.IDLE, out sError);
                        }
                    }
                    //la notification est dans le futur: il s'agit d'une notification type alert dans un futur proche
                    else
                    {
                        //on calcul le temps de l'interval pour relancer le call au bon moment
                        Int32 interval = (Int32)(date - DateTime.Now).TotalMilliseconds;

                        if (interval < nFuturAlertMinInterval)
                        {
                            nFuturAlertMinInterval = interval;
                            bFuturAlertMinInterval = true;
                        }
                    }
                }
            }

            if (PendingNotifs.Count > 0)
                MergeNotifs(PendingNotifs);

            if (bAlertCheckInterval)
            {
                nAlertCheckInterval += 1000;  //on ajoute une seconde pour avoir une marge
            }
            else
            {
                nAlertCheckInterval = NotifConst.NOTIFTOASTERSINTERVAL;
            }

            if (bFuturAlertMinInterval)
            {
                nFuturAlertMinInterval += 1000; //on ajoute une seconde pour avoir une marge

                if (nFuturAlertMinInterval < nAlertCheckInterval)
                {
                    nAlertCheckInterval = nFuturAlertMinInterval;
                }
            }

            XmlNode CallIntervalNode = xmlResult.CreateElement("CallInterval");
            CallIntervalNode.InnerText = nAlertCheckInterval.ToString();
            baseResultNode.AppendChild(CallIntervalNode);

            _xmlDoc = xmlResult;

            return true;
        }

        /// <summary>
        /// Demande à engineNotif la fusion des notifications en param <paramref name="PendingNotifs"/>
        /// </summary>
        /// <param name="PendingNotifs">List des id des notification a fusionner</param>
        private void MergeNotifs(HashSet<Int32> PendingNotifs)
        {
            try
            {
                NotifEngineSchedule.CreateEngine(Pref, Pref.User, eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ROOT), PendingNotifs).Process();
            }
            catch (Exception exx)
            {
#if DEBUG
                eLibTools.EudoTraceLog(exx.Message + " >> " + exx.StackTrace, eLibTools.GetDatasDir(Pref.GetBaseName), Pref.GetBaseName);
#endif

            }
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