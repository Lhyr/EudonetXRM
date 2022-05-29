using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Xml;

namespace Com.Eudonet.Xrm
{
    /// <className>eFilterWizardManager</className>
    /// <summary>Gestion des filtres</summary>
    /// <purpose></purpose>
    /// <authors>JBE</authors>
    /// <date>2011-03-12</date>
    public partial class eFilterWizardManager : eEudoManager
    {
        /// <summary>
        /// Gestion des filtres
        /// </summary>
        protected override void ProcessManager()
        {
            AdvFilter filter;
            AdvFilterTab filterTab;
            AdvFilterLine filterLine;
            XmlDocument xmlResult;

            string error = String.Empty;

            #region Variables du post
            string action = _requestTools.GetRequestFormKeyS("action") ?? "none";

            int mainTab = _requestTools.GetRequestFormKeyI("maintab") ?? 0;
            
            int nTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            string tabTable = _requestTools.GetRequestFormKeyS("tab") ?? String.Empty;
            int filterId = _requestTools.GetRequestFormKeyI("filterid") ?? 0;
            string filterName = _requestTools.GetRequestFormKeyS("filtername") ?? "";
            TypeFilter filterType = (TypeFilter)(_requestTools.GetRequestFormKeyI("filtertype") ?? 0);

            int nTabIndex = _requestTools.GetRequestFormKeyI("tabindex") ?? 0;
            InterOperator tabInterOp = (InterOperator)(_requestTools.GetRequestFormKeyI("taboperator") ?? 0);

            int nLineIndex = _requestTools.GetRequestFormKeyI("lineindex") ?? 0;
            int lineDescId = _requestTools.GetRequestFormKeyI("descid") ?? 0;
            Operator lineOp = (Operator)(_requestTools.GetRequestFormKeyI("operator") ?? 0);
            InterOperator lineInterOp = (InterOperator)(_requestTools.GetRequestFormKeyI("lineoperator") ?? 0);

            bool onlyFilesOpt = _requestTools.GetRequestFormKeyB("onlyfilesopt") ?? false;
            bool withEndOperator = _requestTools.GetRequestFormKeyB("endoperator") ?? false;
            bool bFromTreat = _requestTools.GetRequestFormKeyB("fromtreat") ?? false;

            string prefixFilter = _requestTools.GetRequestFormKeyS("prefixFilter") ?? "";
            int reportId = _requestTools.GetRequestFormKeyI("reportId") ?? 0;
            // Valeurs des critères de rubrique en mode filtre question
            string emptyFilterParam = _requestTools.GetRequestFormKeyS("emptyfilterparams") ?? String.Empty;
            #endregion

            #region Informations pour la sauvegarde du filtre
            string filterParams = _requestTools.GetRequestFormKeyS("filterparams") ?? String.Empty;
            bool applyfilter = _requestTools.GetRequestFormKeyB("applyfilter") ?? false;
            bool saveAs = _requestTools.GetRequestFormKeyB("saveas") ?? false;

            int filterUserId = _pref.User.UserId;
            // UserId = 0 : Filtre public
            // UserId = -1 : On reprend le userid courant (modification d'un filtre public vers un filtre non public)
            string reqUserId = _requestTools.GetRequestFormKeyS("userid");
            if (reqUserId != null && reqUserId != "-1")
                filterUserId = eLibTools.GetNum(reqUserId);
            bool isPublic = filterUserId == 0;
            #endregion

            bool bAdminMode = _requestTools.GetRequestFormKeyB("adminMode") ?? false;

            switch (action)
            {
                #region LOAD
                case "load":
                    break;
                #endregion

                #region EMPTY LINE
                case "emptyline":
                    filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, tabTable);
                    filterLine = filterTab.Lines[0];
                    filterLine.LineIndex = nLineIndex;
                    filterLine.DescId = lineDescId;
                    filterLine.LineOperator = lineInterOp;

                    filter = AdvFilter.GetNewFilter(_pref, filterType, mainTab);
                    filter.FilterTabs.Add(filterTab);

                    // TODO - Transformer comme tout nos autres wizard, faire du htmlcontrol plutôt que du string
                    string resultEmptyLine = eFilterRenderer.WizardManager.GetEmptyLine(_pref, filter, withEndOperator, bAdminMode);

                    RenderResult(RequestContentType.HTML, delegate () { return resultEmptyLine; });
                    break;
                #endregion
                #region emptyExpressFilterline Propre a l'assitant du graphique
                case "emptyExpressFilterline":

                    filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, tabTable);
                    filterLine = filterTab.Lines[0];
                    filterLine.LineIndex = nLineIndex;
                    filterLine.DescId = lineDescId;
                    filterLine.LineOperator = lineInterOp;

                    filter = AdvFilter.GetNewFilter(_pref, filterType, mainTab);
                    filter.FilterTabs.Add(filterTab);
                    FieldFormat[] lstFildsFormat = new FieldFormat[] { FieldFormat.TYP_USER, FieldFormat.TYP_BITBUTTON, FieldFormat.TYP_NUMERIC, FieldFormat.TYP_MONEY, FieldFormat.TYP_DATE, FieldFormat.TYP_BIT, FieldFormat.TYP_CHAR };

                    eFilterRenderer.eFilterRendererParams param = new eFilterRenderer.eFilterRendererParams(
                        prefixFilter: prefixFilter.ToLower(), 
                        bInitialEfChart: (lineDescId == -1), 
                        bSpecialExpressFilter: prefixFilter != string.Empty, 
                        lstFildsFormat: lstFildsFormat,
                        bGetFilterOpAndValue: !prefixFilter.ToLower().Equals(eLibConst.COMBINED_Z.ToLower()),
                        bSetFieldAction: !prefixFilter.ToLower().Equals(eLibConst.COMBINED_Z.ToLower()), bFromChartReport: true);

                    HtmlGenericControl emptyLine = eFilterRenderer.WizardManager.GetHtmlEMptyLine(_pref, filter, withEndOperator, param);
                    RenderResultHTML(emptyLine);
                    break;
                #endregion
                #region chartFilterline Propre a l'assitant du graphique
                //BSE: #73 842 =>
                case "chartFilterline":
                    eReport report;
                    if (reportId > 0)
                    {
                        report = new eReport(_pref, reportId);
                        if (!report.LoadFromDB())
                        {
                            this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), String.Concat("eWizard.report.Load() :", report.ErrorMessage));
                            LaunchError();
                        } 
                    }
                    else
                    {
                        report = new eReport(_pref) { IsNew = true, ReportType = TypeReport.CHARTS };
                        report.LoadParams(eChartWizardRenderer.GetBasicParam(mainTab));
                    }
                       
                    eReportChartFilterWizardRenderer filterRenderer = new eReportChartFilterWizardRenderer(_pref, report, nTab, prefixFilter);
                    Control ddl = filterRenderer.FillFilter();
                    RenderResultHTML(ddl);
                    break;
                #endregion
                //
                #region EMPTY TAB
                case "emptytab":
                    filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, mainTab.ToString());
                    filterTab.TabOperator = tabInterOp;

                    filter = AdvFilter.GetNewFilter(_pref, filterType, mainTab);
                    filter.FilterTabs.Add(filterTab);

                    StringBuilder resultEmptyTab = new StringBuilder();

                    //Opérateur inter Tabs
                    if (!filterTab.TabOperator.Equals(InterOperator.OP_NONE))
                    {
                        HtmlGenericControl opDiv = new HtmlGenericControl("div");
                        opDiv.Attributes.Add("class", "operateur_principal");
                        opDiv.ID = "operateur_principal_" + nTabIndex;
                        opDiv.Controls.Add(eFilterRenderer.GetLogicalOperatorList(_pref, "link_" + nTabIndex, filterTab.TabOperator, "onChangeFilterTabOp(this)", true));
                        resultEmptyTab.Append(eTools.GetControlRender(opDiv));
                    }

                    // TODO - Transformer comme tout nos autres wizard, faire du htmlcontrol plutôt que du string
                    resultEmptyTab.Append(eFilterRenderer.WizardManager.GetEmptyTab(_pref, filter, onlyFilesOpt));

                    RenderResult(RequestContentType.HTML, delegate () { return resultEmptyTab.ToString(); });
                    break;
                #endregion

                #region RELOAD VALUES
                case "reloadvalues":
                    filterTab = AdvFilterTab.GetNewEmpty(nTabIndex, eLibTools.GetTabFromDescId(lineDescId).ToString());
                    filterLine = filterTab.Lines[0];
                    filterLine.LineIndex = nLineIndex;
                    filterLine.DescId = lineDescId;
                    filterLine.Operator = lineOp;

                    filter = AdvFilter.GetNewFilter(_pref, filterType, mainTab);
                    filter.FilterTabs.Add(filterTab);

                    eFilterRenderer.eFilterRendererParams filterParam = new eFilterRenderer.eFilterRendererParams(prefixFilter: prefixFilter.ToLower());
                    HtmlGenericControl resultReloadValues = eFilterRenderer.WizardManager.GetEmptyLineValues(_pref, filter, bFromTreat, param: filterParam);

                    RenderResultHTML(resultReloadValues);
                    break;
                #endregion

                #region VALID FILTER
                case "validfilter":
                    eRightFilter eRF = new eRightFilter(_pref);
                    if (isPublic && !eRF.HasRight(eLibConst.TREATID.PUBLIC_FILTER) && filterType != TypeFilter.DEFAULT && filterType != TypeFilter.DBL)
                        LaunchError(eErrorContainer.GetUserError(eLibConst.MSG_TYPE.INFOS, eResApp.GetRes(_pref, 6834), eResApp.GetRes(_pref, 6838)));

                    filter = new AdvFilter(_pref, filterId);
                    filter.FilterTab = mainTab;
                    filter.FilterName = filterName;
                    filter.FilterParams = filterParams;
                    filter.FilterType = filterType;
                    filter.UserId = filterUserId;
                    filter.FilterLastModified = DateTime.Now;

                    xmlResult = new XmlDocument();

                    ValidFilter(filter, isPublic, saveAs, applyfilter, xmlResult);

                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region RENAME
                case "rename":
                    xmlResult = new XmlDocument();

                    RenameFilter(filterId, filterName, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region GETDESCRIPTION
                case "getdesc":
                    xmlResult = new XmlDocument();

                    //récupère la description
                    GetDescription(filterId, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region DELETE
                case "delete":
                    xmlResult = new XmlDocument();

                    //supprime la description
                    DeleteFilter(filterId, mainTab, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region CHECKFILTERNAME
                case "checkfiltername":
                    xmlResult = new XmlDocument();

                    //teste le nom
                    CheckFilterName(filterName, filterId, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region FILTRE FORMULAIRE
                case "filterquestionprocess":
                    xmlResult = new XmlDocument();

                    FilterQuestionProcess(filterId, mainTab, filterName, emptyFilterParam, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
                    break;
                #endregion

                #region TEST SI FILTRE FORMULAIRE
                case "checkfilterform":
                    _context.Response.Clear();
                    _context.Response.ClearContent();

                    xmlResult = new XmlDocument();

                    CheckFilterQuestion(filterId, xmlResult);

                    //Retourne le flux XML
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });

                    break;
                #endregion

                default:
                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), eResApp.GetRes(_pref, 2024).Replace("<PARAM>", ""));
                    LaunchError();
                    break;

            }
        }

        private void CheckFilterName(string filterName, int filterId, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode countNode = xmlResult.CreateElement("count");
            baseResultNode.AppendChild(countNode);
            #endregion

            countNode.InnerText = AdvFilter.IsFilterNameExists(_pref, filterName, filterId) ? "1" : "0";
        }

        private void FilterQuestionProcess(int filterId, int filterTab, string filterName, string emptyFilterParam, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = xmlResult.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            XmlNode errNode = xmlResult.CreateElement("err");
            baseResultNode.AppendChild(errNode);
            #endregion

            string error = String.Empty;

            string[] aParams = emptyFilterParam?.Split("&") ?? new string[] { "" };

            Dictionary<string, string> dParams = new Dictionary<string, string>();
            foreach (string item in aParams)
            {
                if (item.Length == 0)
                    continue;

                string[] aParam = item.Split('=');
                if (aParam.Length == 2)
                    dParams[aParam[0]] = aParam[1];
            }

            // Effectue le traitement
            AdvFilter.Question.SaveParam(_pref, _pref.User.UserId, filterTab, filterId, filterName, dParams, out error);
            if (!String.IsNullOrEmpty(error))
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat("eFilterWizardManager > FilterQuestionProcess > DoEmptyFilterParams ", Environment.NewLine, " =====> ", error));
                LaunchError();
            }

            successNode.InnerText = (error.Length == 0 ? "1" : "0");
            errNode.InnerText = error;
        }

        private void CheckFilterQuestion(int filterId, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode formNode = xmlResult.CreateElement("isformular");
            // Par defaut
            formNode.InnerText = "0";
            baseResultNode.AppendChild(formNode);

            XmlNode errNode = xmlResult.CreateElement("err");
            baseResultNode.AppendChild(errNode);

            #endregion

            try
            {
                bool isFilterQuestion = AdvFilter.IsFilterQuestion(_pref, filterId);
                formNode.InnerText = isFilterQuestion ? "1" : "0";
            }
            catch (Exception e)
            {
                errNode.InnerText = e.Message;
            }
        }

        /// <summary>
        /// Supprime un filter
        /// </summary>
        /// <returns></returns>
        private bool DeleteFilter(int filterId, int tabDid, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = xmlResult.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = xmlResult.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = xmlResult.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = xmlResult.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            //
            XmlNode filterIdNode = xmlResult.CreateElement("filterid");
            filterIdNode.InnerText = filterId.ToString();
            contentNode.AppendChild(filterIdNode);

            #endregion

            // Si le filtre à supprimer est utilisé actuellement, ne pas le supprimer
            if (_pref.Context.Filters.ContainsKey(tabDid) && _pref.Context.Filters[tabDid] != null)
            {
                if (_pref.Context.Filters[tabDid].FilterSelId == filterId)
                {
                    successNode.InnerText = "0";
                    errorCodeNode.InnerText = "2";
                    errorMsgNode.InnerText = eResApp.GetRes(_pref, 5014) + ".";
                    return false;
                }
            }

            AdvFilterCrudInfos delInfos = AdvFilter.DeleteFilter(_pref, filterId);
            switch (delInfos.Code)
            {
                case AdvFilterCrudInfos.ReturnCode.OK:
                    successNode.InnerText = "1";
                    return true;
                case AdvFilterCrudInfos.ReturnCode.RULE_USED:
                case AdvFilterCrudInfos.ReturnCode.FILTER_USED:
                    // Filtre utilisée sur une ou plusieurs règles
                    successNode.InnerText = "0";
                    errorCodeNode.InnerText = "2";
                    StringBuilder errMsgRulesDel = new StringBuilder();
                    if (delInfos.Code == AdvFilterCrudInfos.ReturnCode.RULE_USED)
                        errMsgRulesDel.Append(eResApp.GetRes(_pref, 865));
                    else
                        errMsgRulesDel.Append(eResApp.GetRes(_pref, 1830));
                    errMsgRulesDel.Append("[[BR]][[BR]][[UL]]");
                    foreach (string s in delInfos.FieldsUseRule)
                        errMsgRulesDel.Append("[[LI]] * ").Append(s);
                    errMsgRulesDel.Append("[[/UL]]");

                    errorMsgNode.InnerText = errMsgRulesDel.ToString();
                    return false;
                default:
                    // Erreur de chargement du filtre
                    successNode.InnerText = "0";
                    errorCodeNode.InnerText = "1";
                    errorMsgNode.InnerText = delInfos.ErrorMsg;
                    return false;
            }
        }

        /// <summary>
        /// Récupère la description d'un filtre et renseigne un xml document avec cette description
        /// </summary>
        /// <param name="filterId">Id du filtre</param>
        /// <param name="xmlResult">Flux xml</param>
        /// <returns>succès/echec de la récupération</returns>
        private void GetDescription(int filterId, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = xmlResult.CreateElement("success");
            // Par defaut
            successNode.InnerText = "0";
            baseResultNode.AppendChild(successNode);

            // Content
            XmlNode contentNode = xmlResult.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            XmlNode filterNameNode = xmlResult.CreateElement("filtername");
            contentNode.AppendChild(filterNameNode);

            XmlNode filterDescNode = xmlResult.CreateElement("filterdescription");
            contentNode.AppendChild(filterDescNode);

            XmlNode filterIdNode = xmlResult.CreateElement("filterid");
            filterIdNode.InnerText = filterId.ToString();
            contentNode.AppendChild(filterIdNode);

            #endregion

            string filterName;
            string error;
            string description = AdvFilter.GetDescription(_pref, filterId, out filterName, out error);

            if (error.Length != 0)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "");
                this.ErrorContainer.AppendDetail = error;
                LaunchError();
            }

            successNode.InnerText = "1";
            filterNameNode.InnerText = filterName;
            filterDescNode.InnerText = description;
        }

        /// <summary>
        /// Renomme un filtre 
        ///  - Retourne vrai si le renommage a réussi
        ///  - maj le xmlResult pour renvoi vers l'html
        /// </summary>
        /// <param name="filterId">Id du filtre</param>
        /// <param name="filterName">Nouveau Nom</param>
        /// <param name="xmlResult">flux xml de retour vers l'html</param>
        /// <returns>Vrai si renommage OK</returns>
        private void RenameFilter(int filterId, string filterName, XmlDocument xmlResult)
        {
            #region INIT XML
            XmlNode baseResultNode;

            // BASE DU XML DE RETOUR            
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            // Content
            XmlNode successNode = xmlResult.CreateElement("success");
            baseResultNode.AppendChild(successNode);

            // Num erreur
            XmlNode errorCodeNode = xmlResult.CreateElement("ErrorCode");
            baseResultNode.AppendChild(errorCodeNode);

            // Msg Erreur
            XmlNode errorMsgNode = xmlResult.CreateElement("ErrorDescription");
            baseResultNode.AppendChild(errorMsgNode);


            // Content
            XmlNode contentNode = xmlResult.CreateElement("Content");
            baseResultNode.AppendChild(contentNode);

            // ASY : protection de la valeur
            filterName = eLibTools.CleanXMLChar(filterName);

            XmlNode filterNameNode = xmlResult.CreateElement("filtername");
            filterNameNode.InnerText = filterName;
            contentNode.AppendChild(filterNameNode);

            XmlNode filterIdNode = xmlResult.CreateElement("filterid");
            filterIdNode.InnerText = filterId.ToString();
            contentNode.AppendChild(filterIdNode);
            #endregion

            AdvFilterCrudInfos cruInfos = AdvFilter.RenameFilter(_pref, filterId, filterName);
            switch (cruInfos.Code)
            {
                case AdvFilterCrudInfos.ReturnCode.OK:
                    successNode.InnerText = "1";
                    break;
                case AdvFilterCrudInfos.ReturnCode.FOUND:
                    successNode.InnerText = "0";
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "");
                    ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 440).Replace("<NAME>", filterName);
                    LaunchError();
                    break;
                default:
                    successNode.InnerText = "0";
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "");
                    ErrorContainer.AppendDetail = eResApp.GetRes(_pref, 6237);
                    LaunchError();
                    break;
            }
        }

        private void ValidFilter(AdvFilter filter, bool isPublic, bool saveAs, bool applyfilter, XmlDocument xmlResult)
        {
            string loadDescError = String.Empty;
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            try
            {
                dal.OpenDatabase();

                #region ViewPermId
                int viewPermId = 0;
                bool viewPermActive = _requestTools.GetRequestFormKeyB("viewperm") ?? false;
                if (viewPermActive)
                {
                    int viewPermMode = _requestTools.GetRequestFormKeyI("viewpermmode") ?? 0;
                    if (viewPermMode != -1)
                    {
                        // #58270 : On ré-utilise le même permid que si ce n'est pas un "enregistrer sous"
                        if (!saveAs)
                            viewPermId = _requestTools.GetRequestFormKeyI("viewpermid") ?? 0;

                        string viewPermUsersId = _requestTools.GetRequestFormKeyS("viewpermusersid") ?? String.Empty;

                        int viewPermLevel = _requestTools.GetRequestFormKeyI("viewpermlevel") ?? 0;

                        //Enregistrement dans la base
                        ePermission permView = new ePermission(viewPermId, (ePermission.PermissionMode)viewPermMode, viewPermLevel, viewPermUsersId);
                        permView.Save(dal);

                        // On renseigne la permid du filtre pour la visu
                        filter.ViewPermId = permView.PermId;
                    }
                }
                #endregion

                #region UpdatePermId
                int updatePermId = 0;
                bool updatePermActive = _requestTools.GetRequestFormKeyB("updateperm") ?? false;
                if (updatePermActive)
                {
                    int updatePermMode = _requestTools.GetRequestFormKeyI("updatepermmode") ?? 0;
                    if (updatePermMode != -1)
                    {
                        // #58270 : On ré-utilise le même permid que si ce n'est pas un "enregistrer sous"
                        if (!saveAs)
                            updatePermId = _requestTools.GetRequestFormKeyI("updatepermid") ?? 0;

                        string updatePermUsersId = _requestTools.GetRequestFormKeyS("updatepermusersid") ?? String.Empty;

                        int updatePermLevel = _requestTools.GetRequestFormKeyI("updatepermlevel") ?? 0;

                        //Enregistrement dans la base
                        ePermission permUpdate = new ePermission(updatePermId, (ePermission.PermissionMode)updatePermMode, updatePermLevel, updatePermUsersId);
                        permUpdate.Save(dal);

                        // On renseigne la permid du filtre pour la modif
                        filter.UpdatePermId = permUpdate.PermId;
                    }
                }
                #endregion

                if (saveAs) //Si enregistrer-sous le filtre on force l appartenance 
                {
                    if (!isPublic)
                        filter.UserId = _pref.User.UserId;

                    // Un filtre de notification ne créra pas de copie, on mis a jour l'existant
                    if (filter.FilterType != TypeFilter.NOTIFICATION)
                        filter.FilterId = 0;//on crée un nouveau filtre en copiant les params de l'ancien
                }



                // TODO - Si le save ne réusi pas, on ne teste pas le retour de la méthode !
                filter.Save(dal);

                // On charge la description du filtre
                filter.LoadDescription(_pref, dal, out loadDescError);
            }
            finally
            {
                dal?.CloseDatabase();
            }

            if (applyfilter)
            {
                FilterSel fltr = new FilterSel(filter.FilterId, filter.FilterName);
                _pref.Context.Filters.AddOrUpdateValue(filter.FilterTab, fltr, true);
                //CNA 19/01/2016 - Remise à zero du pagging pour les EUDOPART
                _pref.Context.Paging.resetInfo();
                _context.Session["Pref"] = _pref;
            }

            //retourner le filterId
            xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
            XmlNode baseResultNode = xmlResult.CreateElement("result");
            xmlResult.AppendChild(baseResultNode);

            XmlNode filterIdNode = xmlResult.CreateElement("filterid");
            filterIdNode.InnerText = filter.FilterId.ToString();
            baseResultNode.AppendChild(filterIdNode);

            // MCR/SPH 40260 : resize de la dernière colonne du tableau à 100%, sur une edition de filtres en ++
            // ajout d un nouveau champ dans le reponse : filteridtab
            XmlNode filterIdTabNode = xmlResult.CreateElement("filteridtab");
            filterIdTabNode.InnerText = filter.FilterTab.ToString();
            baseResultNode.AppendChild(filterIdTabNode);

            // Nom du filtre
            XmlNode filterNameNode = xmlResult.CreateElement("filtername");
            filterNameNode.InnerText = filter.FilterName.ToString();
            baseResultNode.AppendChild(filterNameNode);

            // Description du filtre
            XmlNode filterDescNode = xmlResult.CreateElement("filterdescription");
            if (loadDescError.Length == 0)
                filterDescNode.InnerText = HttpUtility.HtmlDecode(filter.GetFilterDescription(false, "&#13;")).ToString();
            baseResultNode.AppendChild(filterDescNode);

            // Description du filtre
            XmlNode filterIsFormular = xmlResult.CreateElement("filterformular");
            filterIsFormular.InnerText = filter.IsQuestionFilter ? "1" : "0";
            baseResultNode.AppendChild(filterIsFormular);
        }
    }
}