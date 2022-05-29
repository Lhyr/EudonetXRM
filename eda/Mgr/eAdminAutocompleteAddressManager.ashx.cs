using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminAutocompleteAddressManager
    /// </summary>
    public class eAdminAutocompleteAddressManager : eAdminManager
    {
        [DataContract]
        private class Mappings
        {
            [DataMember]
            internal Mapping Housenumber;

            [DataMember]
            internal Mapping Street;

            [DataMember]
            internal Mapping Place;

            [DataMember]
            internal Mapping Village;

            [DataMember]
            internal Mapping Town;

            [DataMember]
            internal Mapping City;

            [DataMember]
            internal Mapping Postcode;

            [DataMember]
            internal Mapping Citycode;

            [DataMember]
            internal Mapping Department;

            [DataMember]
            internal Mapping DepartmentNumber;

            [DataMember]
            internal Mapping Region;

            [DataMember]
            internal Mapping Geography;

            [DataMember]
            internal Mapping Country;

            /// <summary>
            /// US #1224 - On envoie le libellé/l'adresse complète du résultat sélectionné dans la fenêtre de recherche
            /// </summary>
            [DataMember]
            internal Mapping Label;
        }

        [DataContract]
        private class Mapping
        {
            [DataMember]
            internal int value;

            [DataMember]
            internal int id;
        }


        private bool _success = true;
        private string _error = String.Empty;

        protected override void ProcessManager()
        {
            string action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;
            int tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            int autocompleteField = _requestTools.GetRequestFormKeyI("autocompletefield") ?? -1;
            string mappingsJSON = _requestTools.GetRequestFormKeyS("mappingsJSON") ?? String.Empty;

            InitXML();

            if (action == "listtabs")
            {
                doListTabs();
            }
            else if (action == "loadmapping" && tab != 0)
            {
                doListFields(tab);
                doLoadMapping(tab);
            }
            else if(action == "setautocompletefield" && tab != 0 && autocompleteField != -1)
            {
                doSetAutocompleteField(tab, autocompleteField);
            }
            else if (action == "savemapping" && tab != 0 && !String.IsNullOrEmpty(mappingsJSON))
            {
                Mappings mappings = JsonConvert.DeserializeObject<Mappings>(mappingsJSON);

                doSave(tab, mappings);
            }
            else
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager error : unknown action";
            }

            RenduXML();
        }

        #region Load mapping
        private void doListTabs()
        {
            List<Tuple<int, string>> listTab = new List<Tuple<int, string>>();

            try
            {
                listTab = eAdminTools.GetListTabs(_pref);
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doListTabs :" + e.Message;
            }

            if (listTab != null)
            {
                //RenduXML            
                XmlNode baseResultNode = _xmlResult.GetElementsByTagName("result")[0];
            
                XmlNode listtabsNode = _xmlResult.CreateElement("listtabs");
                baseResultNode.AppendChild(listtabsNode);

                listTab.Sort(delegate (Tuple<int, string> x, Tuple<int, string> y)
                {
                    if (x.Item2 == y.Item2) return x.Item1.CompareTo(y.Item1);
                    else return x.Item2.CompareTo(y.Item2);
                });

                foreach (Tuple<int, string> tab in listTab)
                {
                    XmlNode tabNode = _xmlResult.CreateElement("tab");
                    listtabsNode.AppendChild(tabNode);

                    XmlNode descidNode = _xmlResult.CreateElement("descid");
                    descidNode.InnerText = tab.Item1.ToString();
                    tabNode.AppendChild(descidNode);

                    XmlNode libelleNode = _xmlResult.CreateElement("libelle");
                    libelleNode.InnerText = tab.Item2;
                    tabNode.AppendChild(libelleNode);
                }
            }
        }

        private void doListFields(int tab)
        {
            try
            {
                // Rub char hors catalogue
                IEnumerable<eFieldLiteWithLib> listTabFieldsChar =
                    eAdminAutocompleteAddressDialogRenderer.GetTabFields(_pref, tab, new FieldFormat[] { FieldFormat.TYP_CHAR }, new PopupType[] { PopupType.NONE });
                // Rub geo
                IEnumerable<eFieldLiteWithLib> listTabFieldsGeo =
                    eAdminAutocompleteAddressDialogRenderer.GetTabFields(_pref, tab, new FieldFormat[] { FieldFormat.TYP_GEOGRAPHY_V2 });

                //RenduXML            
                RenderXMLListField("listfieldschar", listTabFieldsChar);
                RenderXMLListField("listfieldsgeo", listTabFieldsGeo);
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doListFields error :" + e.Message;
            }
        }

        private void doLoadAutocompleteField()
        {
            try
            {
                throw new NotImplementedException();

                int autocompleteField = 0;
                //TODO

                RenderXMLAutocompleteField(autocompleteField);
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doLoadAutocompleteField error :" + e.Message;
            }
        }

        private void doLoadMapping(int nTab)
        {
            try
            {
                List<eFilemapPartner> listMappings = eFilemapPartner.LoadFileMapPartner(_pref, nTab, FILEMAP_TYPE.AUTOCOMPLETE);
 
                RenderXMLListMappings(listMappings);
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doLoadMapping error :" + e.Message;
            }
        }

        #endregion

        #region Save Mapping

        private void doSetAutocompleteField(int nTabId, int autocompleteField)
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doSetAutocompleteField error :" + e.Message;
            }
        }

        private void doSave(int nTabId, Mappings mappings)
        {
            eudoDAL edal = null;

            try
            {
                edal = eLibTools.GetEudoDAL(_pref);
                edal.OpenDatabase();

                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.RUE, mappings.Street.value, mappings.Street.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.NoVOIE, mappings.Housenumber.value, mappings.Housenumber.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.VILLE, mappings.City.value, mappings.City.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.INSEE, mappings.Citycode.value, mappings.Citycode.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.DEPARTEMENT, mappings.Department.value, mappings.Department.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.NoDEPARTEMENT, mappings.DepartmentNumber.value, mappings.DepartmentNumber.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.REGION, mappings.Region.value, mappings.Region.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.GEOGRAPHY, mappings.Geography.value, mappings.Geography.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.PAYS, mappings.Country.value, mappings.Country.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.CODEPOSTAL, mappings.Postcode.value, mappings.Postcode.id);
                UpdateMappings(edal, nTabId, eModelConst.AutocompleteAddressMappings.LABEL, mappings.Label.value, mappings.Label.id);  // US #1224 - On envoie le libellé complet (adresse complète) du résultat sélectionné dans la fenêtre de recherche
            }
            catch (Exception e)
            {
                _success = false;
                _error += "eAdminAutocompleteAddressManager.doSave error :" + e.Message;
            }
            finally
            {
                edal.CloseDatabase();
            }
        }

        private void UpdateMappings(eudoDAL edal, int nTabId, string sSource, int nDescid,  int nMappingId)
        {
            eFilemapPartner mapping = eFilemapPartner.CreateFileMapPartner(
                    EudoQuery.FILEMAP_TYPE.AUTOCOMPLETE.GetHashCode()
                    , ssType: nTabId
                    , descid: nDescid
                    , source: sSource
                    );

            if (nDescid != 0)
            {
                mapping.SaveFileMapPartner(edal, nMappingId);
            }
            else if (nDescid == 0 && nMappingId != 0)
            {
                mapping.DeleteMapping(edal, nMappingId);
            }
        }

        #endregion

        #region Rendus XML
        private void InitXML()
        {
            _xmlResult = new XmlDocument();
            _xmlResult.AppendChild(_xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = _xmlResult.CreateElement("result");
            _xmlResult.AppendChild(baseResultNode);
        }

        private void RenduXML()
        {
            XmlNode baseResultNode = _xmlResult.GetElementsByTagName("result")[0];

            XmlNode successResultNode = _xmlResult.CreateElement("success");
            successResultNode.InnerText = _success.ToString();
            baseResultNode.AppendChild(successResultNode);

            XmlNode errorResultNode = _xmlResult.CreateElement("error");
            errorResultNode.InnerText = _error;
            baseResultNode.AppendChild(errorResultNode);            

            RenderResult(RequestContentType.XML, delegate () { return _xmlResult.OuterXml; });
        }

        private void RenderXMLListField(string nodeName, IEnumerable<eFieldLiteWithLib> listFields)
        {
            XmlNode baseResultNode = _xmlResult.GetElementsByTagName("result")[0];

            XmlNode listfieldsNode = _xmlResult.CreateElement(nodeName);
            baseResultNode.AppendChild(listfieldsNode);

            foreach (eFieldLiteWithLib field in listFields)
            {
                XmlNode fieldNode = _xmlResult.CreateElement("field");
                listfieldsNode.AppendChild(fieldNode);

                XmlNode descidNode = _xmlResult.CreateElement("descid");
                descidNode.InnerText = field.Descid.ToString();
                fieldNode.AppendChild(descidNode);

                XmlNode libelleNode = _xmlResult.CreateElement("libelle");
                libelleNode.InnerText = field.Libelle;
                fieldNode.AppendChild(libelleNode);
            }
        }

        private void RenderXMLAutocompleteField(int autocompleteField)
        {
            XmlNode baseResultNode = _xmlResult.GetElementsByTagName("result")[0];

            XmlNode autocompleteFieldNode = _xmlResult.CreateElement("autocompletefield");
            autocompleteFieldNode.InnerText = autocompleteField.ToString();
            baseResultNode.AppendChild(autocompleteFieldNode);
        }

        private void RenderXMLListMappings(List<eFilemapPartner> listMappings)
        {
            XmlNode baseResultNode = _xmlResult.GetElementsByTagName("result")[0];

            XmlNode listMappingsNode = _xmlResult.CreateElement("listmappings");
            baseResultNode.AppendChild(listMappingsNode);

            foreach (eFilemapPartner mapping in listMappings)
            {
                XmlNode mappingNode = _xmlResult.CreateElement("mapping");
                listMappingsNode.AppendChild(mappingNode);

                XmlNode idNode = _xmlResult.CreateElement("id");
                idNode.InnerText = mapping.Id.ToString();
                mappingNode.AppendChild(idNode);

                XmlNode sourceDescidNode = _xmlResult.CreateElement("sourcedescid");
                sourceDescidNode.InnerText = mapping.SourceDescId.ToString();
                mappingNode.AppendChild(sourceDescidNode);

                XmlNode sourceTypeNode = _xmlResult.CreateElement("sourcetype");
                sourceTypeNode.InnerText = mapping.SourceType.ToString();
                mappingNode.AppendChild(sourceTypeNode);

                XmlNode descidNode = _xmlResult.CreateElement("descid");
                descidNode.InnerText = mapping.DescId.ToString();
                mappingNode.AppendChild(descidNode);

                XmlNode orderNode = _xmlResult.CreateElement("order");
                orderNode.InnerText = mapping.Order.ToString();
                mappingNode.AppendChild(orderNode);

                XmlNode sourceNode = _xmlResult.CreateElement("source");
                sourceNode.InnerText = mapping.Source;
                mappingNode.AppendChild(sourceNode);

                XmlNode fieldLabelNode = _xmlResult.CreateElement("fieldlabel");
                fieldLabelNode.InnerText = mapping.FieldLabel;
                mappingNode.AppendChild(fieldLabelNode);
            }
        }

        #endregion
    }
}