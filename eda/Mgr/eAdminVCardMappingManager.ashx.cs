using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminVCardMappingManager
    /// </summary>
    public class eAdminVCardMappingManager : eAdminManager
    {
        #region Classes pour serialisation et deserialisation
        [DataContract]
        private class eAdminVCardMappingManagerResult
        {
            [DataMember]
            public bool Success;
            [DataMember]
            public string Error;
        }
        #endregion


        protected override void ProcessManager()
        {
            bool success = false;
            string error = String.Empty;

            string action = _requestTools.GetRequestFormKeyS("action") ?? String.Empty;

            try
            {
                if (action == "updateMapping")
                {
                    string typeMapping = _requestTools.GetRequestFormKeyS("typeMapping") ?? String.Empty;
                    int descid = _requestTools.GetRequestFormKeyI("descid") ?? -1;

                    if (!String.IsNullOrEmpty(typeMapping) && descid > -1)
                    {
                        //chargement mapping
                        string sMapping = String.Empty;
                        eudoDAL dal = eLibTools.GetEudoDAL(_pref);

                        try
                        {
                            dal.OpenDatabase();
                            string sError;
                            sMapping = eSqlVCardMapping.GetMapping(dal, out sError);
                            if (sError.Length > 0)
                                throw new Exception(sError);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(String.Concat("Erreur chargement mapping : ", ex.Message));
                        }
                        finally
                        {
                            dal.CloseDatabase();
                        }

                        // Parse du mapping
                        Dictionary<string, int> dicMappings = new Dictionary<string, int>();
                        try
                        {
                            XmlDocument xmlVCard = new XmlDocument();
                            xmlVCard.LoadXml(sMapping);
                            foreach (XmlNode xField in xmlVCard.SelectSingleNode("//VCardMapping"))
                            {
                                Int32 nFieldDescId = 0;
                                if (!dicMappings.ContainsKey(xField.Name) && Int32.TryParse(xField.InnerText, out nFieldDescId))
                                    dicMappings.Add(xField.Name, nFieldDescId);
                            }
                        }
                        catch (XmlException ex)
                        {
                            //xml parse error
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }

                        //modification/ajout du noeud
                        if (dicMappings.ContainsKey(typeMapping))
                            dicMappings[typeMapping] = descid;
                        else
                            dicMappings.Add(typeMapping, descid);

                        foreach(eLibConst.VCARD_MAPPING_TYPE type in Enum.GetValues(typeof(eLibConst.VCARD_MAPPING_TYPE)))
                        {
                            string nodeName = eLibTools.GetVCARDMappingNodeName(type);

                            if (!dicMappings.ContainsKey(nodeName))
                                dicMappings.Add(nodeName, 0);
                        }

                        //génération du xml
                        XmlDocument newXmlVCard = new XmlDocument();
                        XmlDeclaration declarationNode = newXmlVCard.CreateXmlDeclaration("1.0", "", "");
                        newXmlVCard.AppendChild(declarationNode);

                        XmlNode rootNode = newXmlVCard.CreateElement("VCardMapping");
                        newXmlVCard.AppendChild(rootNode);

                        foreach(KeyValuePair<string, int> kvp in dicMappings)
                        {
                            XmlNode node = newXmlVCard.CreateElement(kvp.Key);
                            node.InnerText = kvp.Value.ToString();
                            rootNode.AppendChild(node);
                        }

                        //sauvegarde du xml
                        string sNewMappings = newXmlVCard.OuterXml;
                        success = _pref.SetConfigDefault(new List<SetParam<String>>() { new SetParam<String>(eLibConst.CONFIG_DEFAULT.VCARDMAPPING.ToString(), sNewMappings) });
                    }
                    else
                    {
                        throw new Exception("Propriétés invalides");
                    }
                }
                else
                {
                    throw new Exception("action non implémentée");
                }
            }
            catch(Exception ex)
            {
                success = false;
                error = String.Concat("Erreur dans eAdminVCardMappingManager.ProcessManager : ", ex.Message);
            }
            

            #region Résultat
            eAdminVCardMappingManagerResult result = new eAdminVCardMappingManagerResult()
            {
                Success = success,
                Error = error
            };

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(result);
            });
            #endregion
        }
    }
}