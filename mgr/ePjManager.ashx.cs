using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools;
using Com.Eudonet.Merge;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Manager qui gère certains traitement AJAX pour les PJs
    /// Suppression de pieces jointes suite à la suppression d'une fiche ou l'annulation de création
    /// Supprime le fichier physiquement ainsi que l'entrée SQL dans la table PJ
    /// 
    /// Recupération de la liste des PJs d'un template
    /// </summary>
    public class ePjManager : eEngineMgr //eEudoManager
    {

        /// <summary>
        /// Action delete ou recup liste de PJ pour tpl mail 
        /// </summary>
        string _action = string.Empty;

        //Nom de la base
        string _baseName = string.Empty;

        // Instance SQL
        string _sqlInstance = string.Empty;

        // FileId concerné 
        int _FileId = 0;

        /// <summary>
        /// Liste de PjIds pour les cas du mode création de template (fileId = 0
        /// </summary>
        string lstPjIds = string.Empty;

        /// <summary>
        /// dico pour la liste des pjs
        /// </summary>
        Dictionary<int, string> dicoLstPj = null;

        /// <summary>
        /// Execution de la suppression
        /// </summary>
        protected override void ProcessManager()
        {
            try
            {
                int nTab = 0;

                #region Récupération des variables

                try
                {
                    if (_requestTools.AllKeys.Contains("action"))
                        _action = _context.Request.Form["action"].ToString();

                    if (_requestTools.AllKeys.Contains("FileId"))
                        _FileId = eLibTools.GetNum(_context.Request.Form["FileId"].ToString());

                    if (_requestTools.AllKeys.Contains("nTab"))
                        nTab = eLibTools.GetNum(_context.Request.Form["nTab"].ToString());
                }
                catch (Exception ex)
                {
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6525), eResApp.GetRes(_pref, 72), ex.ToString()));
                }

                #endregion

                switch (_action)
                {
                    case "delete":
                        {
                            _engResult = null;

                            #region Variables locales

                            // chemin complet de la Pj à supprimer
                            string nFullPath = string.Empty;

                            // Nom du fichier à supprimer
                            string nName = string.Empty;

                            // PjId concerné pour la suppression
                            int npjid;

                            // Liste des ids de pj à supprimer
                            string lstIdsPjTodelete = string.Empty;

                            // Tableau des pjId splités
                            string[] tabId = null;

                            #endregion

                            // Nom de la base
                            _baseName = _pref.GetBaseName;
                            // Instance Name
                            _sqlInstance = _pref.GetSqlInstance;

                            // Ids des fiches à supprimmer
                            if (_requestTools.AllKeys.Contains("IdsForDelete"))
                            {
                                lstIdsPjTodelete = _context.Request.Form["IdsForDelete"].ToString();
                                //     if (lstIdsPjTodelete.Contains(";"))
                                tabId = lstIdsPjTodelete.Split(';');
                            }

                            #region Traitements

                            try
                            {
                                if (tabId != null)
                                {
                                    string error = string.Empty;

                                    ExternalPjInfo pjInfo = new ExternalPjInfo(_pref);
                                    string annexesDatas = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName);
                                    pjInfo.LoadConfigAdvAndInit(annexesDatas, _pref.DatabaseUid);


                                    ePJToAddLite pj;
                                    eErrorContainer errorContainer;
                                    int nType = 0;

                                    for (int i = 0; i < tabId.Length; i++)
                                    {
                                        npjid = eLibTools.GetNum(tabId[i]);

                                        if (npjid == 0)
                                            continue;

                                        // recuperation du nom de fichier à supprimer
                                        // suppression en SQL

                                        Engine.Result.EngineResult formulaResult = ePJTraitements.DeletefromPj(
                                            _pref, npjid, _FileId, nTab,
                                            out nName, out nType, out errorContainer);

                                        if (errorContainer != null)
                                        {
                                            LaunchError(errorContainer);
                                            throw new eEndResponseException();
                                        }

                                        // On ajoute des résultats externe
                                        if (_engResult == null)
                                            _engResult = formulaResult;
                                        else if (formulaResult != null)
                                            _engResult.AddExternalResult(formulaResult);

                                        if (errorContainer != null && errorContainer.DebugMsg.Length > 0)
                                        {
                                            string usrErr = errorContainer.Msg.Length == 0 ? eResApp.GetRes(_pref, 6526) : errorContainer.Msg;
                                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), usrErr, eResApp.GetRes(_pref, 72), errorContainer.DebugMsg));
                                            return;
                                        }

                                        // Suppression du fichier sur le disque
                                        if (nName.Length > 0 && ePJ.IsPhysicalFile(nType))
                                        {
                                            error = ePJTraitements.DeletePjFromDisk(_baseName, nName);
                                            if (error.Length > 0)
                                            {
                                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6527), eResApp.GetRes(_pref, 72), error));
                                                return;
                                            }
                                        }

                                        // Ajout de la pj distante à la liste
                                        if (pjInfo.bExternalPjEnabled && nType == EudoQuery.PjType.REMOTE_CAMPAIGN.GetHashCode())
                                        {
                                            pj = new ePJToAddLite();
                                            pj.PjId = npjid;
                                            pj.Label = nName;
                                            pj.TypePj = EudoQuery.PjType.REMOTE_CAMPAIGN.GetHashCode();

                                            pjInfo.ListOfPj.Add(pj);
                                        }
                                    }

                                    // Lance le ws sur le serveur distant pour la suppression
                                    if (pjInfo.bExternalPjEnabled && pjInfo.ListOfPj.Count > 0)
                                    {
                                        ExternalPjClient client = new ExternalPjClient(new ExternalPjDelete(pjInfo));

                                        if (!client.ProcessRequest(out error))
                                        {
                                            LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6529), eResApp.GetRes(_pref, 72), error));
                                            return;
                                        }
                                    }
                                }
                                else
                                {
                                    LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6528), eResApp.GetRes(_pref, 72)));
                                    return;
                                }
                            }
                            catch (eEndResponseException) { }
                            catch (ThreadAbortException) { }
                            catch (EudoException ex)
                            {
                                LaunchError(eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ex));
                            }
                            catch (Exception ex2)
                            {
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 6529), eResApp.GetRes(_pref, 72), ex2.ToString()));
                            }


                            // Pour les formule bas
                            if (_engResult != null)
                                DoResponse();
                            else
                                ResultXML(string.Empty, eLibConst.MSG_TYPE.INFOS.GetHashCode(), string.Empty, "1");

                            #endregion

                            break;
                        }
                    case "list":
                        {
                            GetListofPj(nTab);

                            break;
                        }
                    case "rename":
                        {

                            #region rename
                            Int32 nPjId = 0;
                            string sOldName = "";
                            string sNewName = "";
                            string sDesc = "";
                            string sToolTip = "";
                            string sLimitDate = "";
                            string sCreaDate = "";
                            DateTime? dtDLimitDate = null;
                            DateTime dtDCreaDate = new DateTime();

                            if (_requestTools.AllKeys.Contains("pjid"))
                                nPjId = eLibTools.GetNum(_context.Request.Form["pjid"].ToString());
                            if (_requestTools.AllKeys.Contains("oldFileName"))
                                sOldName = _context.Request.Form["oldFileName"].ToString();
                            if (_requestTools.AllKeys.Contains("inptFileName"))
                                sNewName = _context.Request.Form["inptFileName"].ToString();
                            if (_requestTools.AllKeys.Contains("inptDsc"))
                                sDesc = _context.Request.Form["inptDsc"].ToString();
                            if (_requestTools.AllKeys.Contains("inptTip"))
                                sToolTip = _context.Request.Form["inptTip"].ToString();
                            if (_requestTools.AllKeys.Contains("inptLimitDate"))
                            {
                                sLimitDate = _context.Request.Form["inptLimitDate"].ToString();
                                if (!string.IsNullOrEmpty(sLimitDate))
                                    dtDLimitDate = eDate.ConvertDisplayToBddDt(_pref.CultureInfo, sLimitDate);
                            }

                            if (_requestTools.AllKeys.Contains("dtCrea"))
                            {
                                sCreaDate = _context.Request.Form["dtCrea"].ToString();
                                if (!string.IsNullOrEmpty(sCreaDate))
                                    dtDCreaDate = eDate.ConvertDisplayToBddDt(_pref.CultureInfo, sCreaDate);
                            }

                            if (nPjId == 0 || sOldName.Length == 0 || sNewName.Length == 0)
                            {
                                StringBuilder sbContext = new StringBuilder("ePjManager.ashx - Rename : ");
                                sbContext.Append("Tab : ").AppendLine(nTab.ToString())
                                    .Append("FileId : ").AppendLine(_FileId.ToString())
                                    .Append("PJId : ").AppendLine(nPjId.ToString())
                                    .Append("Old Name : ").AppendLine(sOldName)
                                    .Append("New Name : ").AppendLine(sNewName);


                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6649), "", devMsg: sbContext.ToString()));
                            }

                            string sDatasPath = eModelTools.GetPhysicalDatasPath(_context, eLibConst.FOLDER_TYPE.ROOT, _pref.GetBaseName);
                            string sUsrError = string.Empty;
                            string sError = ePJTraitements.RenamePJ(_pref, _pref.User, sDatasPath, nTab, _FileId, nPjId, sOldName, sNewName, out sUsrError, sDesc, sToolTip, dtDLimitDate);
                            if (sError.Length > 0)
                            {
                                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", sOldName), sUsrError, eResApp.GetRes(_pref, 72), sError));
                                return;
                            }
                            else if (sUsrError.Length > 0)
                            {
                                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6650).Replace("<FILE>", sOldName), sUsrError, eResApp.GetRes(_pref, 72)));
                                return;
                            }

                            ResultXML(string.Empty, eLibConst.MSG_TYPE.INFOS.GetHashCode(), string.Empty, "1");


                            break;
                        }
                    #endregion



                    case "info":
                        Int32 pjId = 0;
                        if (_requestTools.AllKeys.Contains("pjid"))
                        {
                            pjId = eLibTools.GetNum(_context.Request.Form["pjid"].ToString());
                            ePJ pj = ePJ.CreatePJ(_pref, pjId);

                            RenderXmlResult(pj);
                        }
                        else
                        {
                            throw new Exception(string.Concat("Id = ", pjId, "  de la pj n'est pas valide ! "));

                        }

                        break;
                    case "updatefromparent":
                        ePJToAdd attachment = new ePJToAdd();
                        attachment.FileId = _FileId;
                        attachment.Tab = nTab;
                        string sListPjsIds = "";
                        if (_requestTools.AllKeys.Contains("pjIds") && _context.Request.Form["pjIds"] != null)
                            sListPjsIds = _context.Request.Form["pjIds"].ToString();

                        if (_requestTools.AllKeys.Contains("parentEvtFileId") && _context.Request.Form["parentEvtFileId"] != null)
                        {
                            attachment.ParentEvtFileId = eLibTools.GetNum(_context.Request.Form["parentEvtFileId"].ToString());
                        }
                        if (_requestTools.AllKeys.Contains("parentEvtTabId") && _context.Request.Form["parentEvtTabId"] != null)
                        {
                            attachment.ParentEvtTab = eLibTools.GetNum(_context.Request.Form["parentEvtTabId"].ToString());
                        }

                        if (_requestTools.AllKeys.Contains("ppid") && _context.Request.Form["ppid"] != null)
                        {
                            attachment.PPID = eLibTools.GetNum(_context.Request.Form["ppid"].ToString());
                        }
                        if (_requestTools.AllKeys.Contains("pmid") && _context.Request.Form["pmid"] != null)
                        {
                            attachment.PMID = eLibTools.GetNum(_context.Request.Form["pmid"].ToString());
                        }

                        string sErrorUpdate = "";
                        ePJTraitements.UpdateFromParent(_pref, attachment, sListPjsIds, out sErrorUpdate);
                        if (sErrorUpdate.Length > 0)
                            throw new Exception(sErrorUpdate);
                        break;

                }
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (EudoException ex)
            {
                LaunchError(eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL, ex));
            }
            catch (Exception genEx)
            {
                LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 72), eResApp.GetRes(_pref, 72),
                    eResApp.GetRes(_pref, 72),
                    string.Concat(eResApp.GetRes(_pref, 6530), " : ", Environment.NewLine, genEx.ToString())));
            }
        }

        /// <summary>
        /// Construit et renvois le xml de réponse
        /// </summary>
        private void RenderXmlResult(ePJ pj)
        {
            XmlDocument _xmlDocReturn = new XmlDocument();

            #region XML Declartion, UTF8 ..etc

            XmlNode baseResultNode;
            _xmlDocReturn.AppendChild(_xmlDocReturn.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = NewNode("result", _xmlDocReturn, _xmlDocReturn);

            #endregion

            #region initialisation du retour XML (structure)
            XmlNode xmlNodeSuccessNode = NewNode("success", baseResultNode, _xmlDocReturn);

            //Noeuds de la pj
            XmlNode xmlNodePjId = NewNode("id", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodePjName = NewNode("name", baseResultNode, _xmlDocReturn);
            XmlNode xmlNodePjSrc = NewNode("src", baseResultNode, _xmlDocReturn);

            #endregion

            #region remplit le xml et fait le rendu

            xmlNodePjId.InnerText = pj.PJId.ToString();
            xmlNodePjName.InnerText = pj.FileName;

            // chemin physique de l'annexe            
            xmlNodePjSrc.InnerText = GetPjLink(pj);

            RenderResult(RequestContentType.XML, delegate () { return _xmlDocReturn.OuterXml; });

            #endregion
        }

        /// <summary>
        /// Retourne le chemin physique sur le serveur local, ou l'url du serveur tier pour les annexes externalisées
        /// </summary>
        /// <param name="pj"></param>
        /// <returns></returns>
        private string GetPjLink(ePJ pj)
        {

            if (pj.PJType == PjType.CAMPAIGN || pj.PJType == PjType.REMOTE_CAMPAIGN)
            {
                ExternalPjInfo pjInfo = new ExternalPjInfo(_pref);
                string annexesDatas = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName);
                pjInfo.LoadConfigAdvAndInit(annexesDatas, _pref.DatabaseUid);

                if (pjInfo.bExternalPjEnabled)
                    return pjInfo.GetRemoteLink(pj.PJId);
            }

            //BSE #49729 dans le cas ou le type du PJ est = Web , en renvoie la valeur initiale 
            if (pj.PJType == PjType.FILE || pj.PJType == PjType.CAMPAIGN || pj.PJType == PjType.REMOTE_CAMPAIGN)
            {
                //return string.Concat(eLibTools.GetAppUrl(this._context.Request), "/", eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName), "/", pj.FileName);

                PjBuildParam paramPj = new PjBuildParam()
                {
                    AppExternalUrl = eLibTools.GetAppUrl(this._context.Request),
                    Uid = _pref.DatabaseUid,
                    TabDescId = pj.EudoTabDescId,
                    PjId = pj.PJId,
                    UserId = _pref.UserId,
                    UserLangId = _pref.LangId
                };

                return ExternalUrlTools.GetLinkPJ(paramPj);
            }
            else
                return pj.FileName;


        }

        /// <summary>
        /// Créer un nouveau noeud xml et l'ajoute au noeud parent
        /// </summary>
        /// <param name="Name">nom du noeud</param>
        /// <param name="ParentNode">Noeud parent</param>
        /// <param name="Creator">The creator.</param>
        /// <returns></returns>
        private static XmlNode NewNode(string Name, XmlNode ParentNode, XmlDocument Creator)
        {
            XmlNode child = Creator.CreateElement(Name);
            ParentNode.AppendChild(child);
            return child;
        }

        /// <summary>
        /// Récupère la liste des pj suite à une suppression d'un tpl mail
        /// </summary>
        /// <param name="tabDescId">descid de la table</param>
        private void GetListofPj(Int32 tabDescId)
        {
            string error = string.Empty;

            // Récupération des PjIds pour mode fileId = 0
            if (_allKeys.Contains("pjIds"))
                lstPjIds = _context.Request.Form["pjIds"].ToString();

            List<int> nPJIDs = new List<int>();
            if (lstPjIds.Length > 0)
            {
                foreach (string strID in lstPjIds.Split(';'))
                    nPJIDs.Add(int.Parse(strID));
            }

            if (tabDescId > 0)
            {
                dicoLstPj = new Dictionary<int, string>();
                error = ePJTraitements.PjListSelect(_pref, tabDescId, _FileId, nPJIDs.ToArray(), out dicoLstPj);

                if (error.Length > 0)
                    LaunchError(eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6531), "ePJTraitments.PjListSelect (ePjManager)", eResApp.GetRes(_pref, 72), error));

            }
            else
                LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "DescId Table incorrect", eResApp.GetRes(_pref, 6532), eResApp.GetRes(_pref, 72)));

            ResultXML(string.Empty, eLibConst.MSG_TYPE.SUCCESS.GetHashCode(), string.Empty, "1");
        }


        /// <summary>
        /// Génère un fichier XML de retour
        /// </summary>
        /// <param name="nError"> titre de l'erreur</param>
        /// <param name="nErrorType"> type d'erreur (eLibConst.MSG_TYPE) </param>
        /// <param name="ndetail">detail de l'erreur</param>
        /// <param name="success">1 si success et 0 en cas d'erreur</param>
        private void ResultXML(string nError, int nErrorType, string ndetail, string success)
        {
            XmlDocument xmlResult = new XmlDocument();

            try
            {
                // Init le document XML
                XmlNode _mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                xmlResult.AppendChild(_mainNode);

                // Noeud racine
                XmlNode detailsNode = xmlResult.CreateElement("pjmanager");
                xmlResult.AppendChild(detailsNode);

                // Noeud contenant le retour à récupérer
                XmlNode _resultNode = xmlResult.CreateElement("success");
                detailsNode.AppendChild(_resultNode);
                _resultNode.InnerText = success;

                // Msg d'erreur utilisateur
                XmlNode xmlErrorNode = xmlResult.CreateElement("error");
                detailsNode.AppendChild(xmlErrorNode);
                // Message par defaut : Merci de réessayer ou de contacter le service support.
                xmlErrorNode.InnerText = nError.Length == 0 ? nError : eResApp.GetRes(_pref, 6236);

                // Attribut indiquant le type d'erreur
                XmlNode xmlErrorType = xmlResult.CreateElement("errTyp");
                detailsNode.AppendChild(xmlErrorType);
                xmlErrorType.InnerText = nErrorType.ToString();

                // Msg d'erreur détaillé
                XmlNode xmlErrorDetailNode = xmlResult.CreateElement("detail");
                detailsNode.AppendChild(xmlErrorDetailNode);
                xmlErrorDetailNode.InnerText = ndetail;

                if (dicoLstPj != null && dicoLstPj.Count > 0)
                {
                    XmlNode attachmentsNode = xmlResult.CreateElement("attachments");
                    detailsNode.AppendChild(attachmentsNode);

                    foreach (var item in dicoLstPj)
                    {
                        XmlNode attachmentNode = xmlResult.CreateElement("attachment");

                        XmlNode pjIdNode = xmlResult.CreateElement("pjid");
                        pjIdNode.InnerText = item.Key.ToString();
                        XmlNode namePjNode = xmlResult.CreateElement("name");
                        namePjNode.InnerText = item.Value;

                        attachmentNode.AppendChild(pjIdNode);
                        attachmentNode.AppendChild(namePjNode);

                        attachmentsNode.AppendChild(attachmentNode);
                    }
                }
            }
            catch (Exception ex)
            {
                RenderResult(RequestContentType.TEXT, delegate () { return ex.Message; });
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }
    }
}