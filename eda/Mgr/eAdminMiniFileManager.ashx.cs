using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminMiniFileManger
    /// </summary>
    public class eAdminMiniFileManager : eAdminManager
    {



        protected override void ProcessManager()
        {
            String action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;
            Int32 nTabId = _requestTools.GetRequestFormKeyI("tabid") ?? 0;
            Int32 nMappingId = _requestTools.GetRequestFormKeyI("mappingid") ?? 0;

            Int32 nType = _requestTools.GetRequestFormKeyI("type") ?? -1;
            bool bDisplayLabel = _requestTools.GetRequestFormKeyB("displayLabel") ?? false;
            Int32 nDescid = _requestTools.GetRequestFormKeyI("descid") ?? -1;
            Int32 nOrder = _requestTools.GetRequestFormKeyI("order") ?? -1;

            if (action == "listtabs")
            {
                doListTabs();
            }
            if (action == "list" && nTabId != 0)
            {
                doList(nTabId);
            }
            else if (action == "add" && nTabId != 0)
            {
                doAdd(nTabId);
            }
            else if (action == "delete" && nMappingId != 0)
            {
                doDelete(nMappingId);
            }
            else if (action == "update" && nMappingId != 0 && nTabId != 0)
            {
                doUpdate(nMappingId, nTabId, nType, bDisplayLabel, nDescid, nOrder);
            }
            else if (action == "updateImg" && nTabId != 0)
            {
                if (nMappingId != 0 && nDescid == -1)
                {
                    doDelete(nMappingId);
                }
                else if (nDescid != -1)
                {
                    doUpdateImg(nMappingId, nTabId, nDescid);
                }
            }
        }

        private void doListTabs()
        {
            bool bSuccess = true;
            string sError = string.Empty;

            List<Tuple<int, string>> listTab = new List<Tuple<int, string>>();

            try
            {
                listTab = eAdminTools.GetListTabs(_pref);
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError += "eAdminMiniFileManager.doListTabs :" + e.Message;
            }

            //RenduXML
            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            if (listTab != null)
            {
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
        /// Liste le mapping MiniFiche d'une table
        /// </summary>
        /// <param name="nTabId"></param>
        private void doList(Int32 nTabId)
        {
            string sError="";

            IEnumerable<eFieldLiteMiniFileAdmin> listTabFields = eAdminMiniFileDialogRenderer.GetTabFields(_pref, nTabId);
            IEnumerable<eFieldLiteMiniFileAdmin> listTabFieldsImage = listTabFields
                .Where(f => f.Format == FieldFormat.TYP_IMAGE && f.ImgStorage != ImageStorage.STORE_IN_DATABASE);

            Dictionary<int, string> listParentTabs = eAdminTools.GetListParentTabs(_pref, nTabId);
            List<eAdminMiniFileDialogRenderer.MyTabInfo> listParentTabsInfo = new List<eAdminMiniFileDialogRenderer.MyTabInfo>();

            bool bSuccess = true;
            List<eFilemapPartner> listMappings = new List<eFilemapPartner>();
            try
            {
               listMappings = eFilemapPartner.LoadFileMapPartner(_pref, nTabId, EudoQuery.FILEMAP_TYPE.MINI_FILE);
            }
            catch(Exception e)
            {
                sError = e.Message;
                bSuccess = false;
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


            XmlNode listFieldsNode = xmlResult.CreateElement("listFields");
            baseResultNode.AppendChild(listFieldsNode);

            foreach (eFieldLiteMiniFileAdmin field in listTabFields)
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

            XmlNode listFieldsImageNode = xmlResult.CreateElement("listFieldsImage");
            baseResultNode.AppendChild(listFieldsImageNode);

            foreach (eFieldLiteMiniFileAdmin field in listTabFieldsImage)
            {
                XmlNode fieldImageNode = xmlResult.CreateElement("fieldImage");
                listFieldsImageNode.AppendChild(fieldImageNode);

                XmlNode fieldImageDescidNode = xmlResult.CreateElement("descid");
                fieldImageDescidNode.InnerText = field.Descid.ToString();
                fieldImageNode.AppendChild(fieldImageDescidNode);

                XmlNode fieldImageLabelNode = xmlResult.CreateElement("label");
                fieldImageLabelNode.InnerText = field.Libelle.ToString();
                fieldImageNode.AppendChild(fieldImageLabelNode);
            }

            XmlNode listParentTabsNode = xmlResult.CreateElement("listParentTabs");
            baseResultNode.AppendChild(listParentTabsNode);

            eAdminMiniFileDialogRenderer.MyTabInfo tabInfo;
            foreach (KeyValuePair<int, string> parentTab in listParentTabs.OrderBy(kvp => kvp.Value))
            {
                XmlNode parentTabNode = xmlResult.CreateElement("parentTab");
                listParentTabsNode.AppendChild(parentTabNode);

                XmlNode descidNode = xmlResult.CreateElement("descid");
                descidNode.InnerText = parentTab.Key.ToString();
                parentTabNode.AppendChild(descidNode);

                XmlNode labelNode = xmlResult.CreateElement("label");
                labelNode.InnerText = parentTab.Value;
                parentTabNode.AppendChild(labelNode);

                XmlNode listParentTabFieldsNode = xmlResult.CreateElement("listParentTabFields");
                parentTabNode.AppendChild(listParentTabFieldsNode);

                tabInfo = new eAdminMiniFileDialogRenderer.MyTabInfo(_pref, parentTab.Key, parentTab.Value);

                foreach (eFieldLiteMiniFileAdmin parentTabField in tabInfo.Fields)
                {
                    XmlNode parentTabFieldsNode = xmlResult.CreateElement("parentTabField");
                    listParentTabFieldsNode.AppendChild(parentTabFieldsNode);

                    XmlNode parentTabFieldDescidNode = xmlResult.CreateElement("descid");
                    parentTabFieldDescidNode.InnerText = parentTabField.Descid.ToString();
                    parentTabFieldsNode.AppendChild(parentTabFieldDescidNode);

                    XmlNode parentTabFieldLabelNode = xmlResult.CreateElement("label");
                    parentTabFieldLabelNode.InnerText = parentTabField.Libelle.ToString();
                    parentTabFieldsNode.AppendChild(parentTabFieldLabelNode);
                }

                XmlNode listParentsTabFieldsImageNode = xmlResult.CreateElement("listParentTabFieldsImage");
                parentTabNode.AppendChild(listParentsTabFieldsImageNode);

                foreach (eFieldLiteMiniFileAdmin parentTabFieldImage in tabInfo.FieldsImage)
                {
                    XmlNode parentTabFieldsImageNode = xmlResult.CreateElement("parentTabFieldImage");
                    listParentsTabFieldsImageNode.AppendChild(parentTabFieldsImageNode);

                    XmlNode parentTabFieldImageDescidNode = xmlResult.CreateElement("descid");
                    parentTabFieldImageDescidNode.InnerText = parentTabFieldImage.Descid.ToString();
                    parentTabFieldsImageNode.AppendChild(parentTabFieldImageDescidNode);

                    XmlNode parentTabFieldImageLabelNode = xmlResult.CreateElement("label");
                    parentTabFieldImageLabelNode.InnerText = parentTabFieldImage.Libelle.ToString();
                    parentTabFieldsImageNode.AppendChild(parentTabFieldImageLabelNode);
                }
            }

            XmlNode listMappingsNode = xmlResult.CreateElement("listMappings");
            baseResultNode.AppendChild(listMappingsNode);

            foreach (eFilemapPartner mapping in listMappings)
            {
                XmlNode mappingNode = xmlResult.CreateElement("mapping");
                listMappingsNode.AppendChild(mappingNode);

                XmlNode idNode = xmlResult.CreateElement("id");
                idNode.InnerText = mapping.Id.ToString();
                mappingNode.AppendChild(idNode);

                XmlNode typeNode = xmlResult.CreateElement("type");
                typeNode.InnerText = mapping.SourceDescId.ToString();
                mappingNode.AppendChild(typeNode);

                XmlNode displayLabelNode = xmlResult.CreateElement("displayLabel");
                displayLabelNode.InnerText = (mapping.SourceType == 1 ? true : false).ToString();
                mappingNode.AppendChild(displayLabelNode);

                XmlNode descidNode = xmlResult.CreateElement("descid");
                descidNode.InnerText = mapping.DescId.ToString();
                mappingNode.AppendChild(descidNode);

                XmlNode orderNode = xmlResult.CreateElement("order");
                orderNode.InnerText = mapping.Order.ToString();
                mappingNode.AppendChild(orderNode);

                XmlNode hardLabelNode = xmlResult.CreateElement("hardLabel");
                hardLabelNode.InnerText = mapping.Source;
                mappingNode.AppendChild(hardLabelNode);

                XmlNode resLabelNode = xmlResult.CreateElement("resLabel");
                resLabelNode.InnerText = mapping.FieldLabel;
                mappingNode.AppendChild(resLabelNode);
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        private void doAdd(Int32 nTabId)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            int mappingId = 0;

            int order = eFilemapPartner.GetMaxOrder(_pref, nTabId, EudoQuery.FILEMAP_TYPE.MINI_FILE, out sError) + 1;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(EudoQuery.FILEMAP_TYPE.MINI_FILE.GetHashCode(), ssType: nTabId, order: order);

            eudoDAL edal = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                mappingId = mapping.SaveFileMapPartner(edal);
                bSuccess = true;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }
            finally
            {
                edal.CloseDatabase();
            }

            XmlDocument xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode successResultNode = xmlResult.CreateElement("success");
            successResultNode.InnerText = bSuccess.ToString();
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = xmlResult.CreateElement("error");
            errorResultNode.InnerText = sError;
            baseResultNode.AppendChild(errorResultNode);

            XmlNode mappingIdNode = xmlResult.CreateElement("mappingId");
            mappingIdNode.InnerText = mappingId.ToString();
            baseResultNode.AppendChild(mappingIdNode);

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        private void doDelete(Int32 nMappingId)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(EudoQuery.FILEMAP_TYPE.MINI_FILE.GetHashCode());

            eudoDAL edal = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                mapping.DeleteMapping(edal, nMappingId);
                bSuccess = true;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }
            finally
            {
                edal.CloseDatabase();
            }

            XmlDocument xmlResult = new XmlDocument();
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

        private void doUpdate(Int32 nMappingId, Int32 nTab, Int32 nType, bool bDisplayLabel, Int32 nDescid, Int32 nOrder)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            string sLabel = String.Empty;
            if (nDescid != -1)
            {
                string resTab = eLibTools.GetResSingle(_pref, nTab, _pref.Lang, out sError);
                string resField = eLibTools.GetResSingle(_pref, nDescid, _pref.Lang, out sError);

                sLabel = String.Concat(resTab, ".", resField);
            }

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(
                EudoQuery.FILEMAP_TYPE.MINI_FILE.GetHashCode()
                , ssType: nTab
                , source: sLabel
                , sourceDescid: nType
                , sourceType: bDisplayLabel ? 1 : 0
                , descid: nDescid
                , order: nOrder
                );



            eudoDAL edal = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                mapping.SaveFileMapPartner(edal, nMappingId);
                bSuccess = true;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }
            finally
            {
                edal.CloseDatabase();
            }

            XmlDocument xmlResult = new XmlDocument();
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

        private void doUpdateImg(Int32 nMappingId, Int32 nTab, Int32 nDescid)
        {
            bool bSuccess = false;
            string sError = String.Empty;

            string sLabel = String.Empty;
            if (nDescid != -1)
            {
                string resTab = eLibTools.GetResSingle(_pref, nTab, _pref.Lang, out sError);
                string resField = eLibTools.GetResSingle(_pref, nDescid, _pref.Lang, out sError);

                sLabel = String.Concat(resTab, ".", resField);
            }

            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(
                EudoQuery.FILEMAP_TYPE.MINI_FILE.GetHashCode()
                , ssType: nTab
                , sourceDescid: FILEMAP_MINIFILE_TYPE.IMAGE.GetHashCode()
                , source: sLabel
                , descid: nDescid
                , order: 0
                );

            eudoDAL edal = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                mapping.SaveFileMapPartner(edal, nMappingId);
                bSuccess = true;
            }
            catch (Exception e)
            {
                bSuccess = false;
                sError = e.Message;
            }
            finally
            {
                edal.CloseDatabase();
            }

            XmlDocument xmlResult = new XmlDocument();
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
    }
}