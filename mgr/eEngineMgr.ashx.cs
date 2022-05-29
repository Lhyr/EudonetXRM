using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Engine.Result.Data;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Manager avec retour spécifique d'engine
    /// </summary>
    public abstract class eEngineMgr : eEudoManager
    {
        /// <summary>Message d'erreur</summary>
        protected string _localError = String.Empty;

        /// <summary>Résultat de l'opération Engine</summary>
        protected EngineResult _engResult = null;

        /// <summary>Résultat de l'opération Engine sur adresse</summary>
        protected EngineResult _engAdrResult = null;

        /// <summary>Résultat de l'opération Engine des champs Geography</summary>
        protected EngineResult _engResultGeo = null;

        /// <summary>Résultat de l'opération Engine des signets rattaché à la fiche principale</summary>
        protected EngineResult _engResultBkm = null;

        /// <summary>Retour de la mise à jour MRU</summary>
        protected Dictionary<int, string> _mruResult = null;

        /// <summary>Xml de retour</summary>
        private XmlDocument _xmlDocReturn = null;
        /// <summary>Node xml du resultat</summary>
        private XmlNode _xmlNodeEdnResult = null;

        private bool IsCreate { get { return !_engResult.NewRecord.Empty; } }
        private bool IsDelete { get { return !_engResult.DelRecord.Empty; } }
        private bool IsMerge { get { return !_engResult.MergeRecord.Empty; } }
        private bool IsUpdate { get { return !IsCreate && !IsDelete && !IsMerge; } }

        /// <summary>
        /// Reprend les erreurs venant de Engine sur l'object global ErrorContainer
        /// </summary>
        protected void InitializeError()
        {
            if (ErrorContainer?.IsSet ?? false)
                return;

            if (_localError.Length > 0 || _engResult == null || !_engResult.Success
                || (_engResultGeo != null && !_engResultGeo.Success)
                || (_engResultBkm != null && !_engResultBkm.Success))
            {
                ErrorContainer = new eErrorContainer();
                ErrorContainer.IsSet = true;
                if (_localError.Length > 0)
                    ErrorContainer.AppendDebug = _localError.ToString();
                else if (_engResult == null)
                    ErrorContainer.AppendDebug = "Pas de retour de l'Engine.";
                else if (!_engResult.Success)
                {
                    InitializeError_AddNodeInfo();
                    if (_engResult.Error == null)
                        ErrorContainer.AppendDebug = "Erreur sur Engine inconnue.";
                    else
                        ErrorContainer = _engResult.Error;
                }
                else if (_engResultGeo != null && !_engResultGeo.Success)   //CNA - Gestion d'erreur lors de la duplication des champs type Geography
                {
                    InitializeError_AddNodeInfo();
                    if (_engResultGeo.Error == null)
                        ErrorContainer.AppendDebug = "Erreur sur Engine inconnue (Geography).";
                    else
                        ErrorContainer = _engResultGeo.Error;
                }
                else if (_engResultBkm != null && !_engResultBkm.Success)   //GCH - SPRINT 2014.16 - 33267 - Gestion d'erreur lors de la duplication des signets depuis une duplication de fiche
                {
                    InitializeError_AddNodeInfo();
                    if (_engResultBkm.Error == null)
                        ErrorContainer.AppendDebug = "Erreur sur Engine inconnue (BKM).";
                    else
                        ErrorContainer = _engResultBkm.Error;
                }
            }
        }
        /// <summary>
        /// Ajout des informations essentiels de contextes pour gérer correctement le retour d'erreur
        /// </summary>
        protected void InitializeError_AddNodeInfo()
        {
            //Mode création on ajoute des valeurs pour gérer les cas d'erreur d'engine spécifiques
            if (_engResult != null && _engResult.NewRecord != null && _engResult.NewRecord.FilesId.Count > 0)
            {
                EudoError.AddCustomXmlNode("fid", eLibTools.Join<Int32>(";", _engResult.NewRecord.FilesId));
                EudoError.AddCustomXmlNode("tab", _engResult.Record.Tab.ToString());
            }
        }

        /// <summary>Gestion du retour XML suite à la mise à jour</summary>
        protected void DoResponse()
        {
            _xmlDocReturn = new XmlDocument();

            _xmlNodeEdnResult = _xmlDocReturn.CreateElement("ednResult");
            _xmlDocReturn.AppendChild(_xmlNodeEdnResult);

            XmlNode xmlNode = null;

            #region Gestion d'erreur

            InitializeError();
            LaunchError();

            #endregion

            #region Gestion des messages de confirmation

            if (_engResult.Confirm.Mode != EngineConfirmMode.NONE)
            {
                XmlAttribute xmlNodeAttr = null;

                xmlNode = _xmlDocReturn.CreateElement("confirmmode");
                xmlNode.InnerText = _engResult.Confirm.Mode.GetHashCode().ToString();
                _xmlNodeEdnResult.AppendChild(xmlNode);

                if (_engResult.Confirm.BoxInfo != null)
                {
                    XmlNode xmlNodeConfirm = _xmlDocReturn.CreateElement("confirm");
                    _xmlNodeEdnResult.AppendChild(xmlNodeConfirm);

                    foreach (var kv in _engResult.Confirm.BoxInfo.XmlAttributes())
                    {
                        xmlNodeAttr = _xmlDocReturn.CreateAttribute(kv.Key);
                        xmlNodeAttr.Value = kv.Value;
                        xmlNodeConfirm.Attributes.Append(xmlNodeAttr);
                    }

                    foreach (var kv in _engResult.Confirm.BoxInfo.XmlElements())
                    {
                        xmlNode = _xmlDocReturn.CreateElement(kv.Key);
                        xmlNode.InnerText = kv.Value;
                        xmlNodeConfirm.AppendChild(xmlNode);
                    }
                }
            }

            #endregion

            else
            {
                // Infos lors de la creation
                if (IsCreate)
                    XmlRespCreatedRecord();

                // Infos lors de la mise à jour
                if (IsUpdate)
                    XmlRespUpdatedRecord();

                // Infos lors de la suppression
                if (IsDelete)
                    XmlRespDeletedRecord();

                // Infos lors de la fusion
                if (IsMerge)
                    XmlRespMergedRecord();

                // Infos communes
                XmlRespReloads();
                XmlRespReloadsBkm();
                XmlRespRefreshFld();
                XmlRespMruFld();
                XmlRespDescIdListRule();
                XmlRespMsg();
                XmlRespPg();
            }

            #region Paramètres génériques

            if (_engResult != null)
            {
                foreach (KeyValuePair<String, String> keyValue in _engResult.GetOthersParam)
                {
                    xmlNode = _xmlDocReturn.CreateElement(keyValue.Key);
                    xmlNode.InnerText = keyValue.Value;
                    _xmlNodeEdnResult.AppendChild(xmlNode);
                }
            }

            #endregion

            RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });
        }

        /// <summary>
        /// Gestion du retour après suppression de fiche
        /// </summary>
        private void XmlRespCreatedRecord()
        {
            XmlNode xmlNode = _xmlDocReturn.CreateElement("createdrecord");
            _xmlNodeEdnResult.AppendChild(xmlNode);

            XmlAttribute xmlNodeAttr = _xmlDocReturn.CreateAttribute("tab");
            xmlNodeAttr.Value = _engResult.Record.Tab.ToString();
            xmlNode.Attributes.Append(xmlNodeAttr);

            if (_pref.Context.Paging.Tab == _engResult.Record.Tab)
                _pref.Context.Paging.resetInfo();

            xmlNodeAttr = _xmlDocReturn.CreateAttribute("ids");
            xmlNodeAttr.Value = eLibTools.Join(";", _engResult.NewRecord.FilesId);
            xmlNode.Attributes.Append(xmlNodeAttr);
        }

        /// <summary>
        /// Information sur l'enregistrement modifié
        /// </summary>
        private void XmlRespUpdatedRecord()
        {
            XmlNode xmlNode = _xmlDocReturn.CreateElement("updatedrecord");
            _xmlNodeEdnResult.AppendChild(xmlNode);

            XmlAttribute xmlNodeRecAttr = _xmlDocReturn.CreateAttribute("tab");
            xmlNodeRecAttr.Value = _engResult.Record.Tab.ToString();
            xmlNode.Attributes.Append(xmlNodeRecAttr);

            xmlNodeRecAttr = _xmlDocReturn.CreateAttribute("id");
            xmlNodeRecAttr.Value = _engResult.Record.Id.ToString();
            xmlNode.Attributes.Append(xmlNodeRecAttr);

            // Indique si il y a eu des modifications de liaison direct ou par le biais d'une rubrique specialpopup
            xmlNodeRecAttr = _xmlDocReturn.CreateAttribute("anlnk");
            xmlNodeRecAttr.Value = _engResult.IsLinkChange ? "1" : "0";
            xmlNode.Attributes.Append(xmlNodeRecAttr);

            // Indique si il y a eu des modifications sur un champ histo
            xmlNodeRecAttr = _xmlDocReturn.CreateAttribute("histoupd");
            xmlNodeRecAttr.Value = _engResult.HistoFieldChanged ? "1" : "0";
            xmlNode.Attributes.Append(xmlNodeRecAttr);

        }

        /// <summary>
        /// Gestion du retour après suppression de fiche
        /// </summary>
        private void XmlRespDeletedRecord()
        {
            XmlNode xmlNode = _xmlDocReturn.CreateElement("deletedrecord");
            _xmlNodeEdnResult.AppendChild(xmlNode);

            XmlAttribute xmlNodeAttr = _xmlDocReturn.CreateAttribute("maintab");
            xmlNodeAttr.Value = _engResult.Record.Tab.ToString();
            xmlNode.Attributes.Append(xmlNodeAttr);

            if (_pref.Context.Paging.Tab == _engResult.Record.Tab)
                _pref.Context.Paging.resetInfo();

            // Est-il necessaire de remonter les files id de chaques tables ?
            // _engResult.DelRecord.TabFilesList
        }

        /// <summary>
        /// Gestion du retour après fusion de fiche
        /// </summary>
        private void XmlRespMergedRecord()
        {
            XmlNode xmlNode = _xmlDocReturn.CreateElement("mergedrecord");
            _xmlNodeEdnResult.AppendChild(xmlNode);

            XmlAttribute xmlNodeAttr = _xmlDocReturn.CreateAttribute("maintab");
            xmlNodeAttr.Value = _engResult.Record.Tab.ToString();
            xmlNode.Attributes.Append(xmlNodeAttr);

            xmlNodeAttr = _xmlDocReturn.CreateAttribute("masterFileId");
            xmlNodeAttr.Value = _engResult.MergeRecord.MasterFileId.ToString();
            xmlNode.Attributes.Append(xmlNodeAttr);
        }

        /// <summary>
        /// Reloads des formules "ReloadHeader" / "ReloadDetail" / "ReloadFileHeader"
        /// </summary>
        private void XmlRespReloads()
        {
            XmlNode xmlNode = _xmlDocReturn.CreateElement("reloadheader");
            xmlNode.InnerText = _engResult.ReloadHeader ? "1" : "0";
            _xmlNodeEdnResult.AppendChild(xmlNode);

            xmlNode = _xmlDocReturn.CreateElement("reloaddetail");
            xmlNode.InnerText = _engResult.ReloadDetail ? "1" : "0";
            _xmlNodeEdnResult.AppendChild(xmlNode);

            xmlNode = _xmlDocReturn.CreateElement("reloadfileheader");
            xmlNode.InnerText = _engResult.ReloadFileHeader ? "1" : "0";
            _xmlNodeEdnResult.AppendChild(xmlNode);


            xmlNode = _xmlDocReturn.CreateElement("reloadlist");
            xmlNode.InnerText = _engResult.ReloadList ? "1" : "0";
            _xmlNodeEdnResult.AppendChild(xmlNode);
        }

        /// <summary>
        /// Liste des rubriques dépendantes d'une régle modifié par le changement de valeur des rubriques modifiées
        /// </summary>
        private void XmlRespDescIdListRule()
        {
            if (_engResult.ListDescidRuleUpdated == null)
                return;

            XmlNode xmlNodeDescIdRules = _xmlDocReturn.CreateElement("descidrule");
            xmlNodeDescIdRules.InnerText = eLibTools.Join(";", _engResult.ListDescidRuleUpdated,
                delegate (FieldRuleUpdated tmp) { return tmp.Descid.ToString(); });
            if (xmlNodeDescIdRules.InnerText.Length > 0)
                _xmlNodeEdnResult.AppendChild(xmlNodeDescIdRules);
        }

        /// <summary>
        /// Indique les signets à mettre à jour
        /// </summary>
        private void XmlRespReloadsBkm()
        {
            if (_engResult.RefreshBkm.Count == 0)
                return;

            XmlNode xmlNodeBookmarks = _xmlDocReturn.CreateElement("bookmarks");
            xmlNodeBookmarks.InnerText = eLibTools.Join(";", _engResult.RefreshBkm);
            _xmlNodeEdnResult.AppendChild(xmlNodeBookmarks);
        }

        /// <summary>
        /// Message venant de formule
        /// </summary>
        private void XmlRespMsg()
        {
            XmlNode xmlNode = null;
            XmlNode xmlNodeMsg = null;
            XmlNode xmlNodeMsgs = _xmlDocReturn.CreateElement("procmessages");
            IEnumerable<ResultBoxInfo> listProcMsgBox = _engResult.ListProcMsgBox.GroupBy(x => new { x.Title, x.Msg, x.Detail, x.DescId }).Select(g => g.First());
            foreach (ResultBoxInfo i in listProcMsgBox)
            {
                xmlNodeMsg = _xmlDocReturn.CreateElement("msg");
                xmlNodeMsgs.AppendChild(xmlNodeMsg);

                xmlNode = _xmlDocReturn.CreateElement("title");
                xmlNode.InnerText = i.Title;
                xmlNodeMsg.AppendChild(xmlNode);

                xmlNode = _xmlDocReturn.CreateElement("desc");
                xmlNode.InnerText = i.Msg;
                xmlNodeMsg.AppendChild(xmlNode);

                xmlNode = _xmlDocReturn.CreateElement("detail");
                xmlNode.InnerText = i.Detail;
                xmlNodeMsg.AppendChild(xmlNode);
            }

            if (xmlNodeMsgs.ChildNodes.Count > 0)
                _xmlNodeEdnResult.AppendChild(xmlNodeMsgs);
        }

        /// <summary>
        /// Page web à lancer venant de formule
        /// </summary>
        private void XmlRespPg()
        {
            XmlNode xmlNodeUrl = null;
            XmlNode xmlNodePages = _xmlDocReturn.CreateElement("procpages");

            // Specif V7
            foreach (String url in _engResult.ListProcPg)
            {
                xmlNodeUrl = _xmlDocReturn.CreateElement("url");
                xmlNodeUrl.InnerText = url;
                xmlNodePages.AppendChild(xmlNodeUrl);
            }

            // Specif XRM
            foreach (SpecifXrmInfo specif in _engResult.ListSpecifXRM)
            {
                xmlNodeUrl = _xmlDocReturn.CreateElement("urlxrm");
                xmlNodeUrl.InnerText = specif.GetUrlParams();
                xmlNodePages.AppendChild(xmlNodeUrl);
            }

            if (xmlNodePages.ChildNodes.Count > 0)
                _xmlNodeEdnResult.AppendChild(xmlNodePages);
        }

        /// <summary>
        /// Informations sur les rubriques modifiées avec leurs nouvelles valeurs
        /// </summary>
        private void XmlRespRefreshFld()
        {
            if (_engResult == null || _engResult.ListRefreshFields == null)
                return;

            //BSE #52 292 : rendre le champ de type lien cliquable aprés l'exécution d'un automatisme et l'update du champ en question  
            Field fld = null;
            XmlNode xmlNodeRecords = null;
            XmlNode xmlNodeField = null, xmlNodeRecord = null;
            XmlAttribute xmlNodeAttr = null;

            XmlNode xmlNodeFields = _xmlDocReturn.CreateElement("fields");

            foreach (ListRefreshFieldNewValue refreshFieldNewValue in _engResult.ListRefreshFields)
            {
                fld = (Field)refreshFieldNewValue.Field;

                xmlNodeField = _xmlDocReturn.CreateElement("field");
                xmlNodeFields.AppendChild(xmlNodeField);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("descid");
                xmlNodeAttr.Value = fld.Descid.ToString();
                xmlNodeField.Attributes.Append(xmlNodeAttr);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("format");
                xmlNodeAttr.Value = ((int)(fld.AliasSourceField?.Format ?? fld.Format)).ToString();
                xmlNodeField.Attributes.Append(xmlNodeAttr);

                if (fld.Popup == PopupType.SPECIAL)
                {
                    xmlNodeAttr = _xmlDocReturn.CreateAttribute("isLink");
                    xmlNodeAttr.Value = "1";
                    xmlNodeField.Attributes.Append(xmlNodeAttr);
                }

                xmlNodeRecords = _xmlDocReturn.CreateElement("records");
                xmlNodeField.AppendChild(xmlNodeRecords);

                foreach (RefreshFieldNewValue newVal in refreshFieldNewValue.List)
                {
                    xmlNodeRecord = _xmlDocReturn.CreateElement("record");
                    xmlNodeRecord.InnerText = eLibTools.CleanXMLChar(HttpUtility.HtmlDecode(newVal.DisplayValue));
                    xmlNodeRecords.AppendChild(xmlNodeRecord);

                    xmlNodeAttr = _xmlDocReturn.CreateAttribute("fid");
                    xmlNodeAttr.Value = newVal.FileId.ToString();
                    xmlNodeRecord.Attributes.Append(xmlNodeAttr);

                    if (newVal.DbValue != null)
                    {
                        xmlNodeAttr = _xmlDocReturn.CreateAttribute("dbv");
                        xmlNodeAttr.Value = eLibTools.CleanXMLChar(newVal.DbValue);
                        xmlNodeRecord.Attributes.Append(xmlNodeAttr);
                    }

                    if (newVal.ParentDbValue != null)
                    {
                        xmlNodeAttr = _xmlDocReturn.CreateAttribute("pdbv");
                        xmlNodeAttr.Value = newVal.ParentDbValue;
                        xmlNodeRecord.Attributes.Append(xmlNodeAttr);
                    }
                }
            }

            if (_engResult.Record.Tab == TableType.PP.GetHashCode() && _engAdrResult != null)
            {
                _xmlDocReturn.CreateElement("fields");

                int adrId = _engAdrResult.NewRecord.FilesId[0];
                xmlNodeField = _xmlDocReturn.CreateElement("field");
                xmlNodeFields.AppendChild(xmlNodeField);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("descid");
                xmlNodeAttr.Value = 401.ToString();
                xmlNodeField.Attributes.Append(xmlNodeAttr);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("format");
                xmlNodeAttr.Value = fld.Format.GetHashCode().ToString();
                xmlNodeField.Attributes.Append(xmlNodeAttr);

                xmlNodeRecords = _xmlDocReturn.CreateElement("records");
                xmlNodeField.AppendChild(xmlNodeRecords);

                xmlNodeRecord = _xmlDocReturn.CreateElement("record");
                xmlNodeRecord.InnerText = eLibTools.CleanXMLChar("");
                xmlNodeRecords.AppendChild(xmlNodeRecord);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("fid");
                xmlNodeAttr.Value = adrId.ToString();
                xmlNodeRecord.Attributes.Append(xmlNodeAttr);
            }

            if (xmlNodeFields.ChildNodes.Count > 0)
                _xmlNodeEdnResult.AppendChild(xmlNodeFields);
        }


        /// <summary>
        /// Action ajout d'attribut au node xml result
        /// <param name="xn">Node de résultar des mru</param>
        /// </summary>
        protected virtual void XmlMruResultNodeAddAttribute(XmlNode xn)
        {

        }

        /// <summary>
        /// Liste des mru de rubrique modifiées avec leurs valeurs
        /// </summary>
        private void XmlRespMruFld()
        {
            if (IsDelete)
            {
                XmlNode xmlNode = _xmlDocReturn.CreateElement("mrutab");
                xmlNode.InnerText = eLibTools.Join(";",
                    _engResult.DelRecord.TabsInfos.Values.Where(ti => ti.RefreshMru).Select(ti => ti.T.DescId));
                _xmlNodeEdnResult.AppendChild(xmlNode);

                return;
            }

            if (_mruResult == null || _mruResult.Count == 0)
                return;

            XmlNode xmlNodeMru = null;
            XmlAttribute xmlNodeAttr = null;
            XmlNode xmlNodeMrus = _xmlDocReturn.CreateElement("mrus");
            _xmlNodeEdnResult.AppendChild(xmlNodeMrus);

            foreach (KeyValuePair<Int32, String> keyValue in _mruResult)
            {
                xmlNodeMru = _xmlDocReturn.CreateElement("mru");
                xmlNodeMru.InnerText = eLibTools.CleanXMLChar(keyValue.Value); // #46948 : XML incorrect donc aucun résultat 
                xmlNodeMrus.AppendChild(xmlNodeMru);

                xmlNodeAttr = _xmlDocReturn.CreateAttribute("descid");
                xmlNodeAttr.Value = keyValue.Key.ToString();
                xmlNodeMru.Attributes.Append(xmlNodeAttr);
            }


        }
    }
}