using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using System.Text;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminNotifTriggerManager
    /// </summary>
    public class eAdminNotifTriggerManager : eAdminManager
    {
        protected override void ProcessManager()
        {
            String action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;
            Int32 nTriggerId = _requestTools.GetRequestFormKeyI("triggerid") ?? 0;
            Int32 nTabId = _requestTools.GetRequestFormKeyI("tabid") ?? 0;
            String sTargetSelectId = _requestTools.GetRequestFormKeyS("targetSelectId") ?? String.Empty;
            String sTargetSelectValue = _requestTools.GetRequestFormKeyS("targetSelectValue") ?? String.Empty;
            bool bImageType = _requestTools.GetRequestFormKeyB("image") ?? false;

            String sLabel = _requestTools.GetRequestFormKeyS("label") ?? String.Empty;

            if (action == "listtabs")
            {
                doListTabs();
            }
            else if (action == "listfields")
            {
                doListFields(nTabId, sTargetSelectId, sTargetSelectValue, bImageType);
            }
            else if (action == "listTriggers")
            {
                doLoadListTrigger();
            }
            else if (action == "add")
            {
                doAddTrigger();
            }
            else if (action == "delete" && nTriggerId != 0)
            {
                doDeleteTrigger(nTriggerId);
            }
            else if (action == "loadFile" && nTriggerId != 0)
            {
                doLoadFileTrigger(nTriggerId);
            }
            else if (action == "update" && nTriggerId != 0)
            {
                doUpdateTrigger(nTriggerId, sLabel);
            }
        }

        private void doListTabs()
        {
            bool bSuccess = true;
            string sError = string.Empty;

            StringBuilder sqlTabs = new StringBuilder();
            sqlTabs.Append("SELECT [desc].[descid], [res].[" + _pref.Lang + "] as libelle ")
                    .Append("FROM [desc] left join [res] on [desc].[descid] = [res].[resid] ")
                    .Append("WHERE (isnull([desc].[activetab], 0) <> 0 ")
                    .Append("AND (([desc].[descid] >= 1000 AND [desc].[descid] like '%00') OR [desc].[descid] in (100,200,300,102000)))");

            eudoDAL eDal = eLibTools.GetEudoDAL(_pref);

            List<Tuple<int, string>> listTab = new List<Tuple<int, string>>();

            try
            {
                eDal.OpenDatabase();

                RqParam rq = new RqParam();
                rq.SetQuery(sqlTabs.ToString());

                DataTableReaderTuned dtrt = eDal.Execute(rq, out sError);

                if (sError.Length != 0 || dtrt == null)
                {
                    throw new Exception(sError);
                }
                else
                {
                    while (dtrt.Read())
                    {
                        int descid = dtrt.GetEudoNumeric("descid");
                        string libelle = dtrt.GetString("libelle");

                        listTab.Add(new Tuple<int, string>(descid, libelle));
                    }
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError += "eAdminNotifTriggerManager.doListTabs error :" + e.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }

            //RenduXML
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode listtabsNode = xmlResult.CreateElement("listtabs");
            baseResultNode.AppendChild(listtabsNode);

            listTab.Sort(delegate (Tuple<int, string> x, Tuple<int, string> y)
            {
                if (x.Item2 == y.Item2) return x.Item1.CompareTo(y.Item1);
                else return x.Item2.CompareTo(y.Item2);
            });

            foreach (Tuple<int, string> tab in listTab)
            {
                XmlNode tabNode = xmlResult.CreateElement("tab");
                listtabsNode.AppendChild(tabNode);

                XmlNode descidNode = xmlResult.CreateElement("descid");
                descidNode.InnerText = tab.Item1.ToString();
                tabNode.AppendChild(descidNode);

                XmlNode libelleNode = xmlResult.CreateElement("libelle");
                libelleNode.InnerText = tab.Item2;
                tabNode.AppendChild(libelleNode);
            }

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = bSuccess.ToString();
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = sError;
            baseResultNode.AppendChild(errorResultNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        private void doListFields(Int32 nTabId, string sTargetSelectId, string sTargetSelectValue, bool bImageType)
        {
            bool bSuccess = true;
            string sError = string.Empty;

            IEnumerable<eFieldLiteWithLib> listTabFields = null;
            if (bImageType)
                listTabFields = RetrieveFields.GetDefault(_pref)
                    .AddOnlyThisTabs(new int[] { nTabId })
                    .AddOnlyThisFormats(new FieldFormat[] { FieldFormat.TYP_IMAGE })
                    .ResultFieldsInfo(eFieldLiteWithLib.Factory(_pref));
            else
                listTabFields = RetrieveFields.GetDefault(_pref)
                    .AddOnlyThisTabs(new int[] { nTabId })
                    .ResultFieldsInfo(eFieldLiteWithLib.Factory(_pref));

            // Exclus les rubriques avec libelle vide (cibles etendues)
            listTabFields = listTabFields?.Where(fld => !String.IsNullOrEmpty(fld.Libelle));

            // Tri par libelle
            listTabFields = listTabFields?.OrderBy(fld => fld.Libelle);

            if (listTabFields == null)
                listTabFields = new List<eFieldLiteWithLib>();

            //RenduXML
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode tabIdNode = xmlResult.CreateElement("tabId");
            tabIdNode.InnerText = nTabId.ToString();
            baseResultNode.AppendChild(tabIdNode);

            XmlNode targetSelectIdNode = xmlResult.CreateElement("targetSelectId");
            targetSelectIdNode.InnerText = sTargetSelectId;
            baseResultNode.AppendChild(targetSelectIdNode);

            XmlNode targetSelectValueNode = xmlResult.CreateElement("targetSelectValue");
            targetSelectValueNode.InnerText = sTargetSelectValue;
            baseResultNode.AppendChild(targetSelectValueNode);

            XmlNode listFieldsNode = xmlResult.CreateElement("listFields");
            baseResultNode.AppendChild(listFieldsNode);

            foreach (eFieldLiteWithLib field in listTabFields)
            {
                XmlNode fieldNode = xmlResult.CreateElement("field");
                listFieldsNode.AppendChild(fieldNode);

                XmlNode descidNode = xmlResult.CreateElement("descid");
                descidNode.InnerText = field.Descid.ToString();
                fieldNode.AppendChild(descidNode);

                XmlNode labelNode = xmlResult.CreateElement("label");
                labelNode.InnerText = field.Libelle;
                fieldNode.AppendChild(labelNode);
            }

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = bSuccess.ToString();
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = sError;
            baseResultNode.AppendChild(errorResultNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        /// <summary>
        /// Liste les déclencheurs de notifications déclarés dans la base
        /// </summary>
        /// <param name="nDescId"></param>
        private void doLoadListTrigger()
        {
            bool bSuccess = false;
            string sError = String.Empty;

            List<Tuple<Int32, String>> listTriggers = new List<Tuple<int, string>>();

            try
            {
                List<int> listCol = new List<int>(){
                        EudoQuery.NotificationTriggerField.LABEL.GetHashCode(),
                        EudoQuery.NotificationTriggerField.ICON.GetHashCode(),
                        EudoQuery.NotificationTriggerField.COLOR.GetHashCode(),
                        EudoQuery.NotificationTriggerField.IMAGE.GetHashCode(),
                        EudoQuery.NotificationTriggerField.IMAGESOURCE.GetHashCode(),

                        EudoQuery.NotificationTriggerField.TRIGGER_ACTION.GetHashCode(),
                        EudoQuery.NotificationTriggerField.TRIGGER_TARGET_DESCID.GetHashCode(),

                        EudoQuery.NotificationTriggerField.FILTER_TRIGGER.GetHashCode(),
                        EudoQuery.NotificationTriggerField.SQL_TRIGGER.GetHashCode(),

                        EudoQuery.NotificationTriggerField.NOTIFICATION_ACTION_ID.GetHashCode(),
                        EudoQuery.NotificationTriggerField.NOTIFICATION_TYPE.GetHashCode(),

                        EudoQuery.NotificationTriggerField.BROADCAST_TYPE.GetHashCode(),
                        EudoQuery.NotificationTriggerField.EXPIRATION_DATE.GetHashCode(),
                        EudoQuery.NotificationTriggerField.LIFE_TIME_IN_SECOND.GetHashCode(),

                        EudoQuery.NotificationTriggerField.SUBSCRIBERS.GetHashCode(),
                        EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL.GetHashCode(),
                        EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_PHONE.GetHashCode(),
                        EudoQuery.NotificationTriggerField.SUBSCRIBERS_USER_FIELD.GetHashCode(),

                        EudoQuery.NotificationTriggerField.CREATED_ON.GetHashCode(),
                        EudoQuery.NotificationTriggerField.MODIFIED_ON.GetHashCode(),
                        EudoQuery.NotificationTriggerField.CREATED_BY.GetHashCode(),
                        EudoQuery.NotificationTriggerField.MODIFIED_BY.GetHashCode(),
                        EudoQuery.NotificationTriggerField.OWNERUSER.GetHashCode()
                    };

                eList _list = eListFactory.CreateCustomList(_pref, EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), eLibTools.Join(";", listCol));

                if (_list != null && _list.ListRecords != null)
                {
                    string aliasLabel = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.LABEL.GetHashCode());

                    foreach (eRecord record in _list.ListRecords)
                    {
                        Int32 id = record.MainFileid;

                        string label = record.GetFieldByAlias(aliasLabel).DisplayValue;

                        listTriggers.Add(new Tuple<Int32, string>(id, label));
                    }
                }

                bSuccess = true;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = "Erreur - eAdminNotifTriggerManager.doLoadListTrigger : " + e.Message;
            }

            //Rendu XML
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

            XmlNode listTriggersNode = xmlResult.CreateElement("listTriggers");
            baseResultNode.AppendChild(listTriggersNode);

            foreach (Tuple<Int32, string> trigger in listTriggers)
            {
                XmlNode triggerNode = xmlResult.CreateElement("trigger");
                listTriggersNode.AppendChild(triggerNode);

                XmlNode idNode = xmlResult.CreateElement("id");
                idNode.InnerText = trigger.Item1.ToString();
                triggerNode.AppendChild(idNode);

                XmlNode labelNode = xmlResult.CreateElement("label");
                labelNode.InnerText = trigger.Item2.ToString();
                triggerNode.AppendChild(labelNode);
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        //afficher fiche trigger
        private void doLoadFileTrigger(Int32 nTriggerId)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            List<Tuple<Int32, NotificationTrigger>> listTriggerFields = new List<Tuple<int, NotificationTrigger>>();

            try
            {
                eFile _file = eFileLite.CreateFileLite(_pref, EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), nTriggerId);

                if (_file != null && _file.ListRecords != null)
                {
                    string aliasLabel = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.LABEL.GetHashCode());
                    string aliasIcon = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.ICON.GetHashCode());
                    string aliasColor = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.COLOR.GetHashCode());
                    string aliasImage = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.IMAGE.GetHashCode());
                    string aliasImageSource = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.IMAGESOURCE.GetHashCode());

                    string aliasTriggerAction = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.TRIGGER_ACTION.GetHashCode());
                    string aliasTriggerTargetDescId = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.TRIGGER_TARGET_DESCID.GetHashCode());

                    string aliasFilterTrigger = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.FILTER_TRIGGER.GetHashCode());
                    string aliasSqlTrigger = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.SQL_TRIGGER.GetHashCode());

                    string aliasActionId = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.NOTIFICATION_ACTION_ID.GetHashCode());
                    string aliasNotificationType = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.NOTIFICATION_TYPE.GetHashCode());

                    string aliasBroadcastType = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.BROADCAST_TYPE.GetHashCode());
                    string aliasExpirationDate = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.EXPIRATION_DATE.GetHashCode());
                    string aliasLifeTime = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.LIFE_TIME_IN_SECOND.GetHashCode());

                    string aliasSubscribers = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.SUBSCRIBERS.GetHashCode());
                    string aliasSubscribersFreeEmail = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL.GetHashCode());
                    string aliasSubscribersFreePhone = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_PHONE.GetHashCode());
                    string aliasSubscribersUserField = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.SUBSCRIBERS_USER_FIELD.GetHashCode());

                    string aliasCreatedOn = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.CREATED_ON.GetHashCode());
                    string aliasModifiedOn = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.MODIFIED_ON.GetHashCode());
                    string aliasCreatedBy = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.CREATED_BY.GetHashCode());
                    string aliasModifiedBy = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.MODIFIED_BY.GetHashCode());
                    string aliasOwnerUser = String.Concat(EudoQuery.TableType.NOTIFICATION_TRIGGER.GetHashCode(), "_", EudoQuery.NotificationTriggerField.OWNERUSER.GetHashCode());

                    foreach (eRecord record in _file.ListRecords)
                    {
                        Int32 id = record.MainFileid;

                        string label = record.GetFieldByAlias(aliasLabel).DisplayValue;
                        string icon = record.GetFieldByAlias(aliasIcon).DisplayValue;
                        string color = record.GetFieldByAlias(aliasColor).DisplayValue;
                        string image = record.GetFieldByAlias(aliasImage).DisplayValue;
                        string imageSource = record.GetFieldByAlias(aliasImageSource).DisplayValue;

                        string triggerAction = record.GetFieldByAlias(aliasTriggerAction).DisplayValue;
                        string triggerTargetDescId = record.GetFieldByAlias(aliasTriggerTargetDescId).DisplayValue;

                        string filterTrigger = record.GetFieldByAlias(aliasFilterTrigger).DisplayValue;
                        string sqlTrigger = record.GetFieldByAlias(aliasSqlTrigger).DisplayValue;

                        string actionId = record.GetFieldByAlias(aliasActionId).DisplayValue;
                        string notificationType = record.GetFieldByAlias(aliasNotificationType).DisplayValue;

                        string broadcastType = record.GetFieldByAlias(aliasBroadcastType).DisplayValue;
                        string expirationDate = record.GetFieldByAlias(aliasExpirationDate).DisplayValue;
                        string lifeTime = record.GetFieldByAlias(aliasLifeTime).DisplayValue;

                        string subscribers = record.GetFieldByAlias(aliasSubscribers).DisplayValue; // TODO: format à gérer
                        string subscribersFreeEmail = record.GetFieldByAlias(aliasSubscribersFreeEmail).DisplayValue;
                        string subscribersFreePhone = record.GetFieldByAlias(aliasSubscribersFreePhone).DisplayValue;
                        string subscribersUserField = record.GetFieldByAlias(aliasSubscribersUserField).DisplayValue;

                        string createdOn = record.GetFieldByAlias(aliasCreatedOn).DisplayValue;
                        string modifiedOn = record.GetFieldByAlias(aliasModifiedOn).DisplayValue;
                        Tuple<string, string> createdBy = new Tuple<string, string>(record.GetFieldByAlias(aliasCreatedBy).Value, record.GetFieldByAlias(aliasCreatedBy).DisplayValue);
                        Tuple<string, string> modifiedBy = new Tuple<string, string>(record.GetFieldByAlias(aliasModifiedBy).Value, record.GetFieldByAlias(aliasModifiedBy).DisplayValue);
                        Tuple<string, string> ownerUser = new Tuple<string, string>(record.GetFieldByAlias(aliasOwnerUser).Value, record.GetFieldByAlias(aliasModifiedBy).DisplayValue);

                        NotificationTrigger notificationTrigger = new NotificationTrigger(id, label, icon, color, imageSource, image, triggerAction, triggerTargetDescId, filterTrigger, sqlTrigger, actionId, notificationType, broadcastType, expirationDate, lifeTime, subscribers, subscribersFreeEmail, subscribersFreePhone, subscribersUserField, createdOn, modifiedOn, createdBy, modifiedBy, ownerUser);

                        listTriggerFields.Add(new Tuple<Int32, NotificationTrigger>(id, notificationTrigger));
                    }
                }
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = "Erreur - eAdminNotifTriggerManager.doLoadFileTrigger : " + e.Message;
            }

            bSuccess = true;

            //Rendu XML
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

            XmlNode triggerNode = xmlResult.CreateElement("triggerFile");
            XmlAttribute triggerNodeAttrDescId = xmlResult.CreateAttribute("descId");
            triggerNodeAttrDescId.Value = nTriggerId.ToString();
            triggerNode.Attributes.Append(triggerNodeAttrDescId);
            baseResultNode.AppendChild(triggerNode);

            foreach (Tuple<Int32, NotificationTrigger> trigger in listTriggerFields)
            {
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, 0, "id", "DescID", FieldFormat.TYP_NUMERIC, false, trigger.Item1.ToString()));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.LABEL, "label", "Libellé", "", FieldFormat.TYP_CHAR, true, false, trigger.Item2.Label));
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.ICON, "icon", "Icône", "",  FieldFormat.TYP_CHAR, false, trigger.Item2.Icon)); // TODO
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.COLOR, "color", "Couleur", "",  FieldFormat.TYP_CHAR, false, trigger.Item2.Color)); // TODO
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.TRIGGER_ACTION, "triggerAction", "Onglet déclencheur", "Catalogue proposant l’ensemble des onglets administrables de la base de données.", FieldFormat.TYP_NUMERIC, true, false, true, "TAB", false, new Tuple<string, string>("", trigger.Item2.TriggerAction)));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.IMAGESOURCE, "imageSource", "Rubrique d'image", "Catalogue proposant l’ensemble des rubriques de type Image de l’onglet déclencheur (y compris les rubriques logo et avatar sur PM et PP)", FieldFormat.TYP_NUMERIC, true, false, true, "FIELD", true, new Tuple<string, string>("", trigger.Item2.ImageSource)));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.IMAGE, "image", "Type d'image", "Catalogue proposant les valeurs suivantes : Pictogramme de l’onglet (= 0), Avatar utilisateur de la rubrique « appartient à » (= 1), Avatar utilisateur connecté (= 2), Image de la fiche (= 3)", FieldFormat.TYP_NUMERIC, true, false, trigger.Item2.Image));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.TRIGGER_TARGET_DESCID, "triggerTargetDescId", "Rubrique déclencheuse", "Catalogue proposant l’ensemble des rubriques de l’onglet déclencheur", FieldFormat.TYP_NUMERIC, true, false, true, "FIELD", false, new Tuple<string, string>("", trigger.Item2.TriggerTargetDescId)));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.FILTER_TRIGGER, "filterTrigger", "Condition de déclenchement", "", FieldFormat.TYP_NUMERIC, true, false, trigger.Item2.FilterTrigger));
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.SQL_TRIGGER, "sqlTrigger", "Déclenchement basé sur une condition SQL", "",  FieldFormat.TYP_CHAR, true, false, trigger.Item2.SqlTrigger));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.NOTIFICATION_ACTION_ID, "actionId", "Evènement déclencheur", "Catalogue proposant les valeurs suivantes : Notification désactivée (= 0), Notification déclenchée à la création(= 1), Notification déclenchée à la modification (= 2), Notification déclenchée à la création et la modification (= 3)", FieldFormat.TYP_NUMERIC, true, false, trigger.Item2.ActionId));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.NOTIFICATION_TYPE, "notificationType", "Type de notification", "Catalogue proposant les valeurs suivantes : A la validation (= 0), Programmé(= 1) (non opérant), Rappel planning (= 2)", FieldFormat.TYP_NUMERIC, true, false, trigger.Item2.NotificationType));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.BROADCAST_TYPE, "broadcastType", "Mode de diffusion", "Catalogue à choix multiple proposant les valeurs suivantes : Notification non diffusée (= 0) (non opérant), Notification XRM (= 1), Notification mobile (= 2) (non opérant), Notification Mail (= 4), Notification SMS (= 8) (non opérant)", FieldFormat.TYP_NUMERIC, true, true, trigger.Item2.BroadcastType));
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.EXPIRATION_DATE, "expirationDate", "Date d'expiration", "",  FieldFormat.TYP_DATE, true, false, trigger.Item2.ExpirationDate));
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.LIFE_TIME_IN_SECOND, "lifeTime", "Date de déclenchement (durée de vie)", "",  FieldFormat.TYP_DATE, true, false, trigger.Item2.LifeTime));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.SUBSCRIBERS, "subscribers", "Destinataires fixes", "", FieldFormat.TYP_USER, true, true, true, "USER", false, new Tuple<string, string>("", trigger.Item2.Subscribers)));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL, "subscribersFreeEmail", "Destinataires par e-mail", "", FieldFormat.TYP_CHAR, true, false, trigger.Item2.SubscribersFreeEmail));
                //triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.SUBSCRIBERS_FREE_PHONE, "subscribersFreePhone", "Destinataires par SMS", "",  FieldFormat.TYP_CHAR, true, false, trigger.Item2.SubscribersFreePhone)); // TODO
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.SUBSCRIBERS_USER_FIELD, "subscribersUserField", "Rubrique des abonnés dans la fiche référence", "", FieldFormat.TYP_NUMERIC, true, false, true, "FIELD", false, new Tuple<string, string>("", trigger.Item2.SubscribersUserField)));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.CREATED_ON, "createdOn", "Créée le", "", FieldFormat.TYP_DATE, false, false, trigger.Item2.CreatedOn));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.MODIFIED_ON, "modifiedOn", "Modifiée le", "", FieldFormat.TYP_DATE, false, false, trigger.Item2.ModifiedOn));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.CREATED_BY, "createdBy", "Créée par", "", FieldFormat.TYP_USER, false, false, true, "USER", false, trigger.Item2.CreatedBy));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.MODIFIED_BY, "modifiedBy", "Modifiée par", "", FieldFormat.TYP_USER, false, false, true, "USER", false, trigger.Item2.ModifiedBy));
                triggerNode.AppendChild(getTriggerFileFieldNode(xmlResult, EudoQuery.NotificationTriggerField.OWNERUSER, "ownerUser", "Appartient à", "", FieldFormat.TYP_USER, false, false, true, "USER", false, trigger.Item2.OwnerUser));
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        private XmlNode getTriggerFileFieldNode(XmlDocument rootDocument, NotificationTriggerField field, string name, string friendlyName, string description, FieldFormat format, bool editable, bool multiple, bool isCatalog, string catalogType, bool isImage, Tuple<string, string> valueAndDisplayValue)
        {

            XmlNode triggerChildNode = rootDocument.CreateElement("triggerField");

            XmlAttribute triggerChildNodeAttrDescId = rootDocument.CreateAttribute("descId");
            XmlAttribute triggerChildNodeAttrName = rootDocument.CreateAttribute("name");
            XmlAttribute triggerChildNodeAttrFriendlyName = rootDocument.CreateAttribute("friendlyName");
            XmlAttribute triggerChildNodeAttrDescription = rootDocument.CreateAttribute("description");
            XmlAttribute triggerChildNodeAttrFormat = rootDocument.CreateAttribute("format");
            XmlAttribute triggerChildNodeAttrEditable = rootDocument.CreateAttribute("editable");
            XmlAttribute triggerChildNodeAttrMultiple = rootDocument.CreateAttribute("multiple");
            XmlAttribute triggerChildNodeAttrDbValue = rootDocument.CreateAttribute("dbValue");
            XmlAttribute triggerChildNodeAttrIsCatalog = rootDocument.CreateAttribute("isCatalog");
            XmlAttribute triggerChildNodeAttrCatalogType = rootDocument.CreateAttribute("catalogType");
            XmlAttribute triggerChildNodeAttrImage = rootDocument.CreateAttribute("image");

            triggerChildNodeAttrDescId.Value = field.GetHashCode().ToString();
            triggerChildNodeAttrName.Value = name;
            triggerChildNodeAttrFriendlyName.Value = friendlyName;
            triggerChildNodeAttrDescription.Value = description;
            triggerChildNodeAttrFormat.Value = format.GetHashCode().ToString();
            triggerChildNodeAttrEditable.Value = editable ? "1" : "0";
            triggerChildNodeAttrMultiple.Value = multiple ? "1" : "0";
            triggerChildNodeAttrDbValue.Value = valueAndDisplayValue.Item1;
            triggerChildNodeAttrIsCatalog.Value = isCatalog ? "1" : "0";
            triggerChildNodeAttrCatalogType.Value = catalogType;
            triggerChildNodeAttrImage.Value = isImage ? "1" : "0";
            triggerChildNode.InnerText = valueAndDisplayValue.Item2;

            triggerChildNode.Attributes.Append(triggerChildNodeAttrDescId);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrName);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrFriendlyName);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrDescription);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrFormat);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrEditable);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrMultiple);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrDbValue);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrIsCatalog);
            triggerChildNode.Attributes.Append(triggerChildNodeAttrCatalogType);

            return triggerChildNode;
        }

        private XmlNode getTriggerFileFieldNode(XmlDocument rootDocument, NotificationTriggerField field, string name, string friendlyName, string description, FieldFormat format, bool editable, bool multiple, string valueOrDisplayValue)
        {
            return getTriggerFileFieldNode(rootDocument, field, name, friendlyName, description, format, editable, multiple, false, "NONE", false, new Tuple<string, string>("", valueOrDisplayValue));
        }

        private void addNewEngineValue(Engine.Engine engine, NotificationTriggerField field)
        {
            if (engine == null)
                return;

            string value = _requestTools.GetRequestFormKeyS(String.Concat("FieldValue_", field.GetHashCode())) ?? String.Empty;

            if (value.Length > 0)
                engine.AddNewValue(field.GetHashCode(), value);
        }

        //modifier fiche trigger
        private void doUpdateTrigger(Int32 nTriggerId, string sLabel)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            int triggerId = 0;

            try
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.NOTIFICATION_TRIGGER, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = nTriggerId;

                if (sLabel.Length > 0)
                    eng.AddNewValue(NotificationTriggerField.LABEL.GetHashCode(), sLabel);
                else
                    addNewEngineValue(eng, NotificationTriggerField.LABEL);

                addNewEngineValue(eng, NotificationTriggerField.ICON);
                addNewEngineValue(eng, NotificationTriggerField.COLOR);
                addNewEngineValue(eng, NotificationTriggerField.IMAGESOURCE);
                addNewEngineValue(eng, NotificationTriggerField.IMAGE);

                addNewEngineValue(eng, NotificationTriggerField.TRIGGER_ACTION);
                addNewEngineValue(eng, NotificationTriggerField.TRIGGER_TARGET_DESCID);

                addNewEngineValue(eng, NotificationTriggerField.FILTER_TRIGGER);
                addNewEngineValue(eng, NotificationTriggerField.SQL_TRIGGER);

                addNewEngineValue(eng, NotificationTriggerField.NOTIFICATION_ACTION_ID);
                addNewEngineValue(eng, NotificationTriggerField.NOTIFICATION_TYPE);

                addNewEngineValue(eng, NotificationTriggerField.BROADCAST_TYPE);
                addNewEngineValue(eng, NotificationTriggerField.EXPIRATION_DATE);
                addNewEngineValue(eng, NotificationTriggerField.LIFE_TIME_IN_SECOND);

                addNewEngineValue(eng, NotificationTriggerField.SUBSCRIBERS);
                addNewEngineValue(eng, NotificationTriggerField.SUBSCRIBERS_FREE_EMAIL);
                addNewEngineValue(eng, NotificationTriggerField.SUBSCRIBERS_FREE_PHONE);
                addNewEngineValue(eng, NotificationTriggerField.SUBSCRIBERS_USER_FIELD);

                addNewEngineValue(eng, NotificationTriggerField.CREATED_ON);
                addNewEngineValue(eng, NotificationTriggerField.MODIFIED_ON);
                addNewEngineValue(eng, NotificationTriggerField.CREATED_BY);
                addNewEngineValue(eng, NotificationTriggerField.MODIFIED_BY);
                addNewEngineValue(eng, NotificationTriggerField.OWNERUSER);

                eng.EngineProcess(new Engine.StrategyCruSimple());

                Engine.Result.EngineResult engResult = eng.Result;
                if (!engResult.Success)
                {
                    sError = "Erreur de mise à jour via Engine : " + engResult.Error.Msg;
                }
                else
                {
                    IList<int> listId = engResult.NewRecord.FilesId;
                    if (listId.Count > 0)
                        triggerId = listId[0];
                    else
                        triggerId = nTriggerId;
                }

                bSuccess = engResult.Success;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }

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

            XmlNode triggerIdNode = xmlResult.CreateElement("triggerId");
            triggerIdNode.InnerText = triggerId.ToString();
            baseResultNode.AppendChild(triggerIdNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        //supprimer fiche trigger
        private void doDeleteTrigger(Int32 nTriggerId)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            try
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.NOTIFICATION_TRIGGER, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = nTriggerId;

                eng.EngineProcess(new Engine.StrategyDelSimple());

                Engine.Result.EngineResult engResult = eng.Result;
                if (!engResult.Success)
                {
                    sError = "Erreur de suppression via Engine : " + engResult.Error.Msg;
                }
                else
                {

                }

                bSuccess = engResult.Success;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }

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

            XmlNode triggerIdNode = xmlResult.CreateElement("triggerId");
            triggerIdNode.InnerText = nTriggerId.ToString();
            baseResultNode.AppendChild(triggerIdNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        //ajouter fiche trigger
        private void doAddTrigger()
        {
            bool bSuccess = false;
            string sError = String.Empty;

            int triggerId = 0;

            try
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, (int)TableType.NOTIFICATION_TRIGGER, eEngineCallContext.GetCallContext(EngineContext.APPLI));

                eng.AddNewValue(NotificationTriggerField.LABEL.GetHashCode().GetHashCode(), "");

                eng.EngineProcess(new Engine.StrategyCruSimple());

                Engine.Result.EngineResult engResult = eng.Result;
                if (!engResult.Success)
                {
                    sError = "Erreur de création via Engine : " + engResult.Error.Msg;
                }
                else
                {
                    IList<int> listId = engResult.NewRecord.FilesId;
                    if (listId.Count > 0)
                        triggerId = listId[0];
                }

                bSuccess = engResult.Success;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }

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

            XmlNode triggerIdNode = xmlResult.CreateElement("triggerId");
            triggerIdNode.InnerText = triggerId.ToString();
            baseResultNode.AppendChild(triggerIdNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }


        //todo
        //afficher liste res
        //afficher fiche notifTriggerRes

    }

    public class NotificationTrigger
    {
        private Int32 _id = 0;

        public Int32 Id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _label = "";

        public string Label
        {
            get { return _label; }
            set { _label = value; }
        }
        private string _icon = "";

        public string Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }
        private string _color = "";

        public string Color
        {
            get { return _color; }
            set { _color = value; }
        }
        private string _imageSource = "";

        public string ImageSource
        {
            get { return _imageSource; }
            set { _imageSource = value; }
        }
        private string _image = "";

        public string Image
        {
            get { return _image; }
            set { _image = value; }
        }

        private string _triggerAction = "";

        public string TriggerAction
        {
            get { return _triggerAction; }
            set { _triggerAction = value; }
        }
        private string _triggerTargetDescId = "";

        public string TriggerTargetDescId
        {
            get { return _triggerTargetDescId; }
            set { _triggerTargetDescId = value; }
        }

        private string _filterTrigger = "";

        public string FilterTrigger
        {
            get { return _filterTrigger; }
            set { _filterTrigger = value; }
        }
        private string _sqlTrigger = "";

        public string SqlTrigger
        {
            get { return _sqlTrigger; }
            set { _sqlTrigger = value; }
        }

        private string _actionId = "";

        public string ActionId
        {
            get { return _actionId; }
            set { _actionId = value; }
        }
        private string _notificationType = "";

        public string NotificationType
        {
            get { return _notificationType; }
            set { _notificationType = value; }
        }

        private string _broadcastType = "";

        public string BroadcastType
        {
            get { return _broadcastType; }
            set { _broadcastType = value; }
        }
        private string _expirationDate = "";

        public string ExpirationDate
        {
            get { return _expirationDate; }
            set { _expirationDate = value; }
        }
        private string _lifeTime = "";

        public string LifeTime
        {
            get { return _lifeTime; }
            set { _lifeTime = value; }
        }

        private string _subscribers = "";

        public string Subscribers
        {
            get { return _subscribers; }
            set { _subscribers = value; }
        }
        private string _subscribersFreeEmail = "";

        public string SubscribersFreeEmail
        {
            get { return _subscribersFreeEmail; }
            set { _subscribersFreeEmail = value; }
        }
        private string _subscribersFreePhone = "";

        public string SubscribersFreePhone
        {
            get { return _subscribersFreePhone; }
            set { _subscribersFreePhone = value; }
        }
        private string _subscribersUserField = "";

        public string SubscribersUserField
        {
            get { return _subscribersUserField; }
            set { _subscribersUserField = value; }
        }

        private string _createdOn = "";

        public string CreatedOn
        {
            get { return _createdOn; }
            set { _createdOn = value; }
        }
        private string _modifiedOn = "";

        public string ModifiedOn
        {
            get { return _modifiedOn; }
            set { _modifiedOn = value; }
        }
        private Tuple<string, string> _createdBy = new Tuple<string, string>("", "");

        public Tuple<string, string> CreatedBy
        {
            get { return _createdBy; }
            set { _createdBy = value; }
        }
        private Tuple<string, string> _modifiedBy = new Tuple<string, string>("", "");

        public Tuple<string, string> ModifiedBy
        {
            get { return _modifiedBy; }
            set { _modifiedBy = value; }
        }
        private Tuple<string, string> _ownerUser = new Tuple<string, string>("", "");

        public Tuple<string, string> OwnerUser
        {
            get { return _ownerUser; }
            set { _ownerUser = value; }
        }

        public NotificationTrigger(
            Int32 id,
            string label, string icon, string color, string imageSource, string image,
            string triggerAction, string triggerTargetDescId, string filterTrigger, string sqlTrigger,
            string actionId, string notificationType, string broadcastType, string expirationDate, string lifeTime,
            string subscribers, string subscribersFreeEmail, string subscribersFreePhone, string subscribersUserField,
            string createdOn, string modifiedOn, Tuple<string, string> createdBy, Tuple<string, string> modifiedBy, Tuple<string, string> ownerUser)
        {
            _label = label;
            _icon = icon;
            _color = color;
            _imageSource = imageSource;
            _image = image;
            _triggerAction = triggerAction;
            _triggerTargetDescId = triggerTargetDescId;
            _filterTrigger = filterTrigger;
            _sqlTrigger = sqlTrigger;
            _actionId = actionId;
            _notificationType = notificationType;
            _broadcastType = broadcastType;
            _expirationDate = expirationDate;
            _lifeTime = lifeTime;
            _subscribers = subscribers;
            _subscribersFreeEmail = subscribersFreeEmail;
            _subscribersFreePhone = subscribersFreePhone;
            _subscribersUserField = subscribersUserField;
            _createdOn = createdOn;
            _modifiedOn = modifiedOn;
            _createdBy = createdBy;
            _modifiedBy = modifiedBy;
            _ownerUser = ownerUser;
        }
    }
}