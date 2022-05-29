using Com.Eudonet.Internal;

using EudoQuery;
using System;
using System.Collections.Generic;
using System.Xml;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// Description résumée de eSelectionWizardManager
    /// </summary>
    public class eCartographyManager : eEudoManager
    {
        // BASE DU XML DE RETOUR            
        XmlDocument xmlResult;
        XmlElement _successNode;
        XmlElement _infoNode;
        XmlElement _recordsNode;

        // 
        Int32 _nTab;
        Int32 _nPage = 1;
        Int32 _nRows = 300;
        String _mktPolygonFilter = String.Empty;
        Boolean _bMaxRow = false;


        /// <summary>
        /// Processes the manager.
        /// </summary>
        protected override void ProcessManager()
        {
            _nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            _nRows = _requestTools.GetRequestFormKeyI("rows") ?? 300;
            _bMaxRow = _requestTools.GetRequestFormKeyB("maxRows") ?? false;
            _nPage = _requestTools.GetRequestFormKeyI("page") ?? 1;
            _mktPolygonFilter = _requestTools.GetRequestFormKeyS("geofilter");

            try
            {
                eListCartography list = eListCartography.GetListDataMaps(_pref, _nTab, _nRows, _nPage, _bMaxRow);
                if (!String.IsNullOrEmpty(_mktPolygonFilter))
                    list.MktPolygonCustomFilter = _mktPolygonFilter;


                list.Generate();
                RenderXmlList(list);
            }
            catch (Exception ex)
            {
                RenderXmlError(ex);
            }
        }



        private void RenderXmlError(Exception ex)
        {
            _successNode.InnerText = "0";
#if DEBUG
            _infoNode.InnerText = ex.Message + "<br />" + ex.StackTrace;
            _infoNode.SetAttribute("dev", ex.Message);
            _infoNode.SetAttribute("stack", ex.StackTrace);

#else
           _infoNode.InnerText = ex.Message + "<br />" + ex.StackTrace;

#endif

        }

        private void RenderXmlList(eListCartography listDataMap)
        {
            BuildXmlHeader();
            _recordsNode.SetAttribute("filter", listDataMap.Map[CartographySsType.GEOGRAPHY.GetHashCode()].DescId.ToString());

            XmlElement recordNode;
            Int32 rows = 0;
            string tabIconColor = listDataMap.ViewMainTable.GetIconColor;

            foreach (eRecord r in listDataMap.ListRecords)
            {
                recordNode = xmlResult.CreateElement("record");


                if (!RenderMappingFields(r, recordNode, listDataMap.Map))
                    continue;

                _recordsNode.AppendChild(recordNode);

                BuildXmlNode("marked", recordNode, r.IsMarked ? "1" : "0");

                // couleur gray si le pushpin a été visié sinon couleur du thème  
                BuildXmlNode("bgColor", recordNode, _pref.ThemeXRM.Color != "#FFF" ? _pref.ThemeXRM.Color : _pref.ThemeXRM.Color2);

                // couleur conditionnelle
                BuildXmlNode("ruleColor", recordNode, r.RuleColor.HasRuleColor ? r.RuleColor.Color : tabIconColor);

                // icon 
                BuildXmlNode("ruleIcon", recordNode, r.RuleColor.HasRuleColor ? eFontIcons.GetFontClassName(r.RuleColor.Icon) : String.Empty);

                rows++;
            }
            _successNode.InnerText = "1";
            _recordsNode.SetAttribute("rows", rows + "/" + listDataMap.ListRecords.Count.ToString());
            _recordsNode.SetAttribute("icon", eFontIcons.GetFontClassName(listDataMap.ViewMainTable.GetIcon).ToString());
            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }


        private Boolean RenderMappingFields(eRecord record, XmlElement recordNode, Dictionary<int, eMapItem> map)
        {

            eFieldRecord field;
            String sFieldValue;
            int descid = 0;

            #region fileId
            BuildXmlNode("fileId", recordNode, record.MainFileid.ToString());
            #endregion

            #region Champ Geo

            if (!map.ContainsKey(CartographySsType.GEOGRAPHY.GetHashCode()))
                throw new Exception("Mapping - Champ de type Geographique est requis pour afficher la carte !");

            var mapItem = map[CartographySsType.GEOGRAPHY.GetHashCode()];
            field = record.GetFieldByAlias(_nTab + "_" + mapItem.DescId);

            if (field == null || String.IsNullOrEmpty(field.Value) || !field.RightIsVisible)
                return false;

            BuildXmlNode(CartographySsType.GEOGRAPHY.ToString().ToLower(), recordNode, field.Value).SetAttribute("descid", field.FldInfo.Descid.ToString());

            #endregion

            #region Champ Title

            if (map.ContainsKey(CartographySsType.TITLE.GetHashCode()))
            {
                mapItem = map[CartographySsType.TITLE.GetHashCode()];
                field = record.GetFieldByAlias(_nTab + "_" + mapItem.DescId);

                if (field != null)
                {
                    descid = field.FldInfo.Descid;
                    sFieldValue = (mapItem.DisplayLabel && !String.IsNullOrEmpty(field.DisplayValue) ? String.Concat(field.FldInfo.Libelle, " : ") : String.Empty)
                            + field.DisplayValue;
                    XmlElement elt = BuildXmlNode(CartographySsType.TITLE.ToString().ToLower(), recordNode, sFieldValue);
                    elt.SetAttribute("descid", descid.ToString());

                    if (field.FldInfo.Table.EdnType == EdnType.FILE_MAIN)
                        elt.SetAttribute("onclick", $"top.loadFile({field.FldInfo.Table.DescId}, {field.FileId}, 3, false, LOADFILEFROM.TAB)");
                    else
                        elt.SetAttribute("onclick", $"top.shFileInPopup({field.FldInfo.Table.DescId}, {field.FileId}, top._res_190, null, null, 0, '', true, null, 13, null, null, null, {{ noLoadFile: true }});");


                }
            }

            #endregion
            #region Champ SubTitle

            if (map.ContainsKey(CartographySsType.SUBTITLE.GetHashCode()))
            {
                mapItem = map[CartographySsType.SUBTITLE.GetHashCode()];
                field = record.GetFieldByAlias(_nTab + "_" + mapItem.DescId);

                if (field != null)
                {
                    sFieldValue = (mapItem.DisplayLabel && !String.IsNullOrEmpty(field.DisplayValue) ? String.Concat(field.FldInfo.Libelle, " : ") : String.Empty)
                            + field.DisplayValue;
                    BuildXmlNode(CartographySsType.SUBTITLE.ToString().ToLower(), recordNode, sFieldValue).SetAttribute("descid", field.FldInfo.Descid.ToString());
                }
            }

            #endregion

            #region Champ Image

            if (map.ContainsKey(CartographySsType.IMAGE.GetHashCode()))
            {
                mapItem = map[CartographySsType.IMAGE.GetHashCode()];
                field = record.GetFieldByAlias(_nTab + "_" + mapItem.DescId);

                if (field != null)
                {
                    sFieldValue = field.DisplayValue;
                    if (!String.IsNullOrEmpty(field.Value) && field.FldInfo.Format == FieldFormat.TYP_IMAGE && field.FldInfo.ImgStorage == ImageStorage.STORE_IN_FILE)
                        sFieldValue = String.Concat(eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref.GetBaseName), "/", field.Value);

                    BuildXmlNode(CartographySsType.IMAGE.ToString().ToLower(), recordNode, sFieldValue).SetAttribute("descid", field.FldInfo.Descid.ToString());
                }
            }

            #endregion

            #region Champ01 à Champ05

            if (map.ContainsKey(CartographySsType.FIELD.GetHashCode()))
            {
                mapItem = map[CartographySsType.FIELD.GetHashCode()];
                for (int i = 0; i < mapItem.Items.Count; i++)
                {
                    field = record.GetFieldByAlias(_nTab + "_" + mapItem.Items[i].DescId);

                    if (field != null)
                    {
                        sFieldValue = (mapItem.Items[i].DisplayLabel && !String.IsNullOrEmpty(field.DisplayValue) ? String.Concat(field.FldInfo.Libelle, " : ") : String.Empty)
                            + field.DisplayValue;
                        BuildXmlNode(CartographySsType.FIELD.ToString().ToLower() + i.ToString("00"), recordNode, sFieldValue).SetAttribute("descid", field.FldInfo.Descid.ToString());
                    }
                }
            }

            #endregion

            return true;
        }




        private XmlElement BuildXmlNode(String elementName, XmlElement recordNode, String innerText)
        {
            XmlElement node = xmlResult.CreateElement(elementName);
            node.InnerText = innerText;
            recordNode.AppendChild(node);

            return node;
        }

        private void BuildXmlHeader()
        {
            // BASE DU XML DE RETOUR            
            xmlResult = new XmlDocument();
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));


            XmlElement resultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(resultNode);

            _successNode = xmlResult.CreateElement("success");
            resultNode.AppendChild(_successNode);

            _infoNode = xmlResult.CreateElement("info");
            resultNode.AppendChild(_infoNode);

            _recordsNode = xmlResult.CreateElement("records");
            resultNode.AppendChild(_recordsNode);

        }
    }
}