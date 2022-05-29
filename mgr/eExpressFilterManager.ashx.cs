using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.coremodel;
using EudoExtendedClasses;
using EudoQuery;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eExpressFilterManager</className>
    /// <summary></summary>
    /// <purpose></purpose>
    /// <authors>JBE</authors>
    /// <date>2011-12-27</date>
    public class eExpressFilterManager : eEudoManager
    {
        private Int32 _tab = 0;
        private Int32 _searchDescId = 0;
        private Int32 _searchTabDescId = 0;
        private String _searchValue = String.Empty;

        private XmlDocument xmlResult = new XmlDocument();
        private XmlNode _detailsNode = null;
        private Int32 formularType = -1;

        /// <summary>
        /// Chargement de la page - appelé via le  page_load du manager
        /// </summary>
        protected override void ProcessManager()
        {

            string sAction = "values";

            if (_pref.User.UserId == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", " "), " (userId = 0)")
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }


            if (_requestTools.AllKeys.Contains("action"))
                sAction = _context.Request.Form["action"].ToString();

            if (_requestTools.AllKeys.Contains("tab"))
                _tab = Int32.Parse(_context.Request.Form["tab"].ToString());

            // Chargement de id de fiche parent
            Int32 parentFileId = 0;
            if (_context.Request.Form["parentFileId"] != null)
                parentFileId = eLibTools.GetNum(_context.Request.Form["parentFileId"].ToString());

            // Chargement de id de fiche parent
            Int32 tabFrom = 0;
            if (_context.Request.Form["tabfrom"] != null)
                tabFrom = eLibTools.GetNum(_context.Request.Form["tabfrom"].ToString());

            if (_requestTools.AllKeys.Contains("descid"))
                _searchDescId = eLibTools.GetNum(_context.Request.Form["descid"].ToString());

            //AAB tâche 1882
            if (_context.Request.Form["formularType"] != null)
                formularType = eLibTools.GetNum(_context.Request.Form["formularType"].ToString());

            if (_tab == 0 || _searchDescId == 0)
            {
                if (_tab == 0)
                    ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("TabDescId incorrect : ", _context.Request.Form["tab"].ToString()));
                else if (_searchDescId == 0)
                    ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("SearchDescId incorrect : ", _context.Request.Form["descid"].ToString()));
                LaunchError();
            }

            _searchTabDescId = eLibTools.GetTabFromDescId(_searchDescId);


            _searchValue = _requestTools.GetRequestFormKeyS("q");
            if (!string.IsNullOrEmpty(_searchValue))
                _searchValue = eLibTools.CleanXMLChar(_searchValue);

            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            _detailsNode = xmlResult.CreateElement("expressfilterresult");
            xmlResult.AppendChild(_detailsNode);



            eudoDAL dal = eLibTools.GetEudoDAL(_pref);

            try
            {
                dal.OpenDatabase();

                switch (sAction)
                {
                    case "values":

                        if (String.IsNullOrEmpty(_searchValue))
                            // TERMINER LE SCRIPT
                            throw new Exception("Impossible de faire un filtre sur une valeur vide ou null");

                        #region refacto
                        /*
                        xmlResult = eExpressFilterSearchValues.GetValuesXML(

                            pref: _pref,
                            nDescId: _searchDescId,
                            sSearchValue: _searchValue,

                            nTab: _tab,
                            nParentTabDescId: tabFrom,
                            nParentTabFileId: parentFileId);

                        break;*/
                        #endregion

                        #region anciennes version

                        string error = String.Empty;

                        FieldLite fldSrch = new FieldLite(_searchDescId);
                        fldSrch.ExternalLoadInfo(dal, out error);

                        if (error.Length > 0)
                        {
                            // TERMINER LE SCRIPT
                            ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("ExpressFilter (", _tab, " - ", _searchDescId, ") : Erreur lors des informations du champ : ", error));
                            LaunchError();
                        }

                        bool isCatalog = fldSrch.Format == FieldFormat.TYP_CHAR && fldSrch.Popup != PopupType.NONE;
                        bool isSpecCatalog = isCatalog && fldSrch.Descid != fldSrch.PopupDescId && fldSrch.PopupDescId % 100 == 1;

                        if (fldSrch.Format == FieldFormat.TYP_USER)
                        {
                            LoadUsers(dal);
                        }
                        else if (isCatalog && !isSpecCatalog)
                            LoadCatalogValues(dal, fldSrch);
                        else
                            LoadValuesFromQuery(dal, fldSrch);

                        break;
                    #endregion

                    case "loadallvalues":
                        LoadAllValues(dal);
                        break;
                    case "quickuserfilter":
                        QuickUserFilter(dal);
                        break;
                }
            }
            catch (eEndResponseException) { }
            catch (ThreadAbortException)
            {    // Laisse passer le response.end du RenderResult
            }
            catch (EudoInternalException eie)
            {
                //Exception  contenant un conteneur d'erreur déjà généré.
                ErrorContainer = eie.ErrorContainer;
                LaunchError(eie.ErrorContainer);
            }
            catch (EudoException eEx)
            {
                //Exception Eudo : utilisation du message "user" pour le conteneur
                ErrorContainer = eErrorContainer.GetErrorContainerFromEudoException(eLibConst.MSG_TYPE.CRITICAL,
                    sTitle: eResApp.GetRes(_pref, 72),
                    eudoEx: eEx);

                LaunchError();
            }
            catch (Exception genEx)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("Erreur eExpressFilterManager - ", genEx.Message));
                LaunchError();
            }
            finally
            {
                dal.CloseDatabase();
                if (!ErrorContainer.IsSet)
                    RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
            }
        }

        private void LoadCatalogValues(eudoDAL dal, FieldLite fldSrch)
        {
            eCatalog catalog = null;
            if (fldSrch.Popup != PopupType.DESC
                && fldSrch.Popup != PopupType.ENUM)
            {
                catalog = new eCatalog(dal, _pref, fldSrch.Popup, _pref.User, fldSrch.PopupDescId, false, -1, searchpattern: _searchValue, isSnapshot: _pref.IsSnapshot);
            }


            XmlAttribute xmlAttrib;
            XmlNode xmlExpressValue;

            //debute par
            xmlExpressValue = xmlResult.CreateElement("element");
            xmlAttrib = xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", _searchValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2006), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);

            //Contient
            xmlExpressValue = xmlResult.CreateElement("element");
            xmlAttrib = xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_CONTAIN.GetHashCode().ToString(), ";|;", _searchValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2009), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);

            if (catalog != null)
            {
                foreach (eCatalog.CatalogValue catalogValue in catalog.Values)
                {
                    xmlExpressValue = xmlResult.CreateElement("element");

                    xmlAttrib = xmlResult.CreateAttribute("value");
                    xmlAttrib.Value = catalogValue.DbValue;

                    xmlExpressValue.Attributes.Append(xmlAttrib);
                    xmlExpressValue.InnerText = catalogValue.DisplayValue;

                    _detailsNode.AppendChild(xmlExpressValue);
                }
            }

        }

        private void LoadUsers(eudoDAL dal)
        {
            // on obtient la liste des users à partir du même objet qui est utilisé pour ouvrir les catalogues utilisateurs
            List<eUser.UserListItem> uli;
            eUser objUser = new eUser(dal, _searchDescId, _pref.User, eUser.ListMode.USERS_ONLY, _pref.GroupMode, new List<string>());
            StringBuilder sbError = new StringBuilder();
            uli = objUser.GetUserList(false, true, _searchValue, sbError);
            if (sbError.Length > 0)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat("ExpressFilter (", _tab, " - ", _searchDescId, ") : Erreur lors de la récupération des utilisateurs : ", sbError));
                LaunchError();
            }

            XmlAttribute xmlAttrib;
            XmlNode xmlExpressValue;

            // Pour les utilisateurs seul le débute par est géré par eudoquery.
            //debute par
            xmlExpressValue = xmlResult.CreateElement("element");
            xmlAttrib = xmlResult.CreateAttribute("value");
            xmlAttrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", _searchValue);
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlAttrib = xmlResult.CreateAttribute("type");
            xmlAttrib.Value = "operator";
            xmlExpressValue.Attributes.Append(xmlAttrib);

            xmlExpressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2006), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
            _detailsNode.AppendChild(xmlExpressValue);


            foreach (eUser.UserListItem usr in uli)
            {
                if (usr.Hidden)
                    continue;

                xmlExpressValue = xmlResult.CreateElement("element");

                xmlAttrib = xmlResult.CreateAttribute("value");

                xmlAttrib.Value = usr.ItemCode;

                xmlExpressValue.Attributes.Append(xmlAttrib);

                xmlExpressValue.InnerText = usr.Libelle;
                _detailsNode.AppendChild(xmlExpressValue);

            }

        }

        private void EqReturnError(String etapeName, String msg)
        {
            if (msg.Length == 0)
                return;

            ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL,
                    String.Concat("Impossible de ", etapeName, " EudoQuery pour le chargement de valeurs de filtre express.", msg));

            //Arrete le traitement et envoi l'erreur
            LaunchError();
        }

        /// <summary>
        /// Permet de charger les valeurs du filtre
        /// Construit la requete avec EudoQuery
        /// </summary>
        /// <param name="dal"> objet Eudodal pour les accès a la base de donnée</param>
        /// <param name="sourceFldSrch">Rubrique de recherche</param>
        /// <param name="bLoadAll">Chargement de toute les valeurs</param>
        private void LoadValuesFromQuery(eudoDAL dal, FieldLite sourceFldSrch, Boolean bLoadAll = false)
        {
            String error = String.Empty;

            if (_searchDescId == (int)PJField.FILEID)
                throw new Exception("Pas de filtre express sur Annexes.Fiche");


            // Chargement de id de fiche parent
            Int32 parentFileId = 0;
            if (_context.Request.Form["parentFileId"] != null)
                parentFileId = eLibTools.GetNum(_context.Request.Form["parentFileId"].ToString());

            // Chargement de id de fiche parent
            Int32 tabFrom = 0;
            if (_context.Request.Form["tabfrom"] != null)
                tabFrom = eLibTools.GetNum(_context.Request.Form["tabfrom"].ToString());

            WhereCustom wcSearch = null;
            WhereCustom wcFomrularType = null;
            // Liste globale gérant les ++
            List<WhereCustom> listWCglobal = new List<WhereCustom>();
            // Liste des tout les critères
            List<WhereCustom> listWCsearch = new List<WhereCustom>();

            bool addNameFirstNameFiltersParams = false;

            Operator customsOperator = Operator.OP_START_WITH;

            #region MRU sur Champs de liaison
            // JAS lors du cas d'un champ de liaison, on recherche avec un pattern "contient" au lieu de "débute par"
            // ajout de la condtion "endwith : sinon, l'opérateur devenait "contient" pour tous les types de champ (catalogue, utilisateur, numérique...)
            // dans le cas des catalogues utilisateur (cf demande #24236 [Mode liste] - Filtre express sur Catalogue Utilisateurs), cela est critique car l'opérateur "contient" est compris comme un "est dans la liste"
            // et attend en value une liste d'id alors qu'en filtre express, on lui fourni une chaine de char
            // TODO : vérifier la nécessité du comportement - il y a plusieurs transformation sur l'opérateur appliqué sur les champs de type user, il faudrait s'assurer
            // qu'il n'y a pas de contradiction
            if (_searchTabDescId != TableType.PP.GetHashCode() && _searchTabDescId != TableType.PM.GetHashCode() && _searchDescId.ToString().EndsWith("01"))
                customsOperator = Operator.OP_CONTAIN;

            #endregion

            if (!bLoadAll)
            {
                //AAB tâche 1882
                wcSearch = new WhereCustom(_searchDescId.ToString(), customsOperator, _searchValue);
                if (formularType != -1)
                    wcFomrularType = new WhereCustom(FormularField.TYPE.GetHashCode().ToString(), Operator.OP_EQUAL, formularType.ToString());
            }
            else
                wcSearch = new WhereCustom(_searchDescId.ToString(), Operator.OP_IS_NOT_EMPTY, "");

            listWCsearch.Add(wcSearch);
            if (wcFomrularType != null)
                listWCsearch.Add(wcFomrularType);

            // PP01 : pour la recherche sur PP en utilisant avec nom et prenom
            if (_searchDescId == TableType.PP.GetHashCode() + 1)
            {
                addNameFirstNameFiltersParams = true;

                wcSearch = new WhereCustom("COMPLETE_NAME_FIRSTNAME", customsOperator, _searchValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);
            }
            // PM01
            else if (_searchDescId == TableType.PM.GetHashCode() + 1)
            {
                // ASY : 23 591 => En liste commission, le filtre sur la colonne société
                //Ne retrouve pas INHA en tapant le sigle (institut national...) alors que ca fonctionne en V7
                // filtrer egalement sur les descid SIGLE et GROUPE
                wcSearch = new WhereCustom(PMField.GROUPE.GetHashCode().ToString(), customsOperator, _searchValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);

                wcSearch = new WhereCustom(PMField.SIGLE.GetHashCode().ToString(), customsOperator, _searchValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);
            }

            // HLA - Comportement pas normal pour une recherche sur filtre express, paramètre qui devrai s'appliquer uniquement sur les 01 des table MAIN et sur les rubrique de liaisons - #49725
            #region Champ de recherche complémentaire

            if (sourceFldSrch.Table.EdnType == EdnType.FILE_MAIN && sourceFldSrch.Table.TabType != TableType.ADR)
            {
                // Rubrique 01 ou liaison
                bool mainTableField = sourceFldSrch.Descid % 100 == 1;

                if (mainTableField || sourceFldSrch.Popup == PopupType.SPECIAL)
                {
                    bool addAdvancedSearchDescId = true;
                    int targetTab = _searchTabDescId;
                    if (sourceFldSrch.Popup == PopupType.SPECIAL)
                    {
                        targetTab = eLibTools.GetTabFromDescId(sourceFldSrch.PopupDescId);

                        EqCacheLinkedTable ca = new EqCacheLinkedTable();
                        ca.SetDal = dal;
                        TabLink tabLink = ca.Get(new DicoKeyAdvTabLink(sourceFldSrch.Table.TabType, sourceFldSrch.Table.DescId, targetTab));
                        addAdvancedSearchDescId = tabLink.HasLink;
                        ca = null;
                    }

                    if (addAdvancedSearchDescId)
                    {
                        Int32 advancedSearchDescId = 0;

                        RqParam rq = new RqParam("SELECT isnull(AdvancedSearchDescId, 0) as AdvancedSearchDescId FROM [desc] WHERE descid = @TargetTab");
                        rq.AddInputParameter("@TargetTab", SqlDbType.Int, targetTab);
                        DataTableReaderTuned dtrTuned = null;
                        try
                        {
                            dtrTuned = dal.Execute(rq, out error);
                            if (error.Length == 0 && dtrTuned != null && dtrTuned.Read())
                            {
                                // recupere le champs de recherche complementaire
                                advancedSearchDescId = eLibTools.GetNum(dtrTuned.GetString("AdvancedSearchDescId"));
                            }

                            if (advancedSearchDescId != 0)
                            {
                                wcSearch = new WhereCustom(advancedSearchDescId.ToString(), customsOperator, _searchValue, InterOperator.OP_OR);
                                listWCsearch.Add(wcSearch);
                            }
                        }
                        finally
                        {
                            if (dtrTuned != null)
                                dtrTuned.Dispose();
                        }
                        rq = null;
                    }
                }
            }

            #endregion


            #region Criteres depuis une table ++ (ADRJOIN)

            TableLite _tb = new TableLite(_tab);
            _tb.ExternalLoadInfo(dal, out error);

            if (_searchTabDescId != TableType.USER.GetHashCode()
                && _searchTabDescId != TableType.FILTER.GetHashCode()
                && _searchTabDescId != TableType.REPORT.GetHashCode())
            {

                if (_tb.AdrJoin && parentFileId > 0)
                {
                    String rqWC = String.Empty;
                    if (_searchTabDescId == TableType.PP.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [", _tb.TabName, "].[PPID] FROM [", _tb.TabName, "] WHERE [", _tb.TabName, "].[EVTID] = ", parentFileId);
                        wcSearch = new WhereCustom("PPID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else if (_searchTabDescId == TableType.PM.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [ADDRESS].[PMID] FROM [", _tb.TabName, "] INNER JOIN [address] ON [", _tb.TabName, "].[ADRID] = [ADDRESS].[ADRID]",
                            " WHERE [", _tb.TabName, "].[EVTID] = ", parentFileId);
                        wcSearch = new WhereCustom("PMID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else if (_searchTabDescId == TableType.ADR.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [", _tb.TabName, "].[ADRID] FROM [", _tb.TabName, "] WHERE [", _tb.TabName, "].[EVTID] = ", parentFileId);
                        wcSearch = new WhereCustom("ADRID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else
                    {
                        wcSearch = new WhereCustom("EVTID", Operator.OP_EQUAL, parentFileId.ToString(), InterOperator.OP_AND);
                    }

                    listWCglobal.Add(wcSearch);
                }
            }

            #endregion


            #region Filtre sur la tables des FILTRES/RAPPORT/Modèles
            if (tabFrom != _searchTabDescId)
            {
                WhereCustom wcTab = null;
                if (_searchTabDescId == TableType.FILTER.GetHashCode())
                {
                    wcTab = new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, tabFrom.ToString());
                }
                else if (_searchTabDescId == TableType.REPORT.GetHashCode())
                {
                    wcTab = new WhereCustom(ReportField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, tabFrom.ToString());
                }
                else if (_searchTabDescId == TableType.MAIL_TEMPLATE.GetHashCode())
                {
                    wcTab = new WhereCustom(MailTemplateField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, tabFrom.ToString());
                }
                if (wcTab != null)
                {
                    listWCglobal.Add(wcTab);
                }
            }
            #endregion

            if (listWCsearch != null)
                listWCglobal.Add(new WhereCustom(listWCsearch));

            eDataFillerGeneric filler = new eDataFillerGeneric(_pref, _searchTabDescId, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                    delegate (EudoQuery.EudoQuery eq)
                    {
                        eq.SetListCol = _searchDescId.ToString();

                        if (!bLoadAll)
                            eq.SetTopRecord = eConst.MAX_ROWS_EXPRESS;

                        // Filtre particulier sur le nom/prenom de contact
                        if (addNameFirstNameFiltersParams)
                        {
                            eq.AddParam("NameFirstName_FirstNameFieldDescId", (TableType.PP.GetHashCode() + 2).ToString());
                            eq.AddParam("NameFirstName_NameFieldDescId", (TableType.PP.GetHashCode() + 1).ToString());
                        }

                        // Gestion ou non de l'historique - Active le critère sur l'histo dans eudoQuery
                        eq.AddParam("ActiveHisto", "1");

                        // On retourne uniquement le nom du contact et pas son prenom/particule Sinon la recherche est incohérente par la suite
                        eq.AddParam("NAMEONLY", "1");

                        eq.AddCustomFilter(new WhereCustom(listWCglobal));
                    };

            filler.Generate();

            listWCglobal = null;
            listWCsearch = null;
            wcSearch = null;

            if (filler.ErrorMsg.Length != 0 || filler.InnerException != null)
            {
                string msg = filler.ErrorMsg;
                if (filler.InnerException != null)
                    msg = string.Concat(msg, Environment.NewLine, filler.InnerException.Message);
                EqReturnError("Filler", msg);
            }

            #region Information sur la rubrique de recherche

            Field fldSrch = filler.FldFieldsInfos.Find(e => e.Descid == _searchDescId);

            // Arrete le traitement et renvoi l erreur
            if (fldSrch == null)
                throw new Exception("Impossible de trouver le FldSearch (" + _searchDescId + ") depuis la liste <GetFieldHeaderList> d'EudoQuery.");

            #endregion

            XmlAttribute _attrib;
            XmlNode _expressValue;

            switch (fldSrch.Format)
            {
                #region TYP_AUTOINC / TYP_ID / TYP_MONEY / TYP_NUMERIC

                case FieldFormat.TYP_AUTOINC:
                case FieldFormat.TYP_ID:
                case FieldFormat.TYP_MONEY:
                case FieldFormat.TYP_NUMERIC:
                    {
                        //    2000 -   égal à
                        //    2001 -   inférieur à
                        //    2003 -   supérieur à
                        // Je passe de int32 à long pour avoir une longeur plus grande en saisie
                        // GCH 25/03/2015 : Je passe de long à décimal pour gérer les nombres à virgule
                        Decimal _nQ = 0;
                        Decimal.TryParse(_searchValue, out _nQ);

                        XmlNode _xmlNumeric = xmlResult.CreateElement("isnumeric");
                        _detailsNode.AppendChild(_xmlNumeric);
                        _xmlNumeric.InnerText = "1";

                        _expressValue = xmlResult.CreateElement("element");

                        _attrib = xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_EQUAL.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2000), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_pref), new eNumber.SectionParam(_pref)), "<span class=\"specialItem\">\"</span>");

                        // _expressValue.InnerText = String.Concat(_res.GetRes(2000), " \"", _nQ, "\"");
                        _detailsNode.AppendChild(_expressValue);

                        _expressValue = xmlResult.CreateElement("element");
                        _attrib = xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_LESS.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2001), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_pref), new eNumber.SectionParam(_pref)), "<span class=\"specialItem\">\"</span>");
                        //   _expressValue.InnerText = String.Concat(_res.GetRes(2001), " \"", _nQ, "\"");
                        _detailsNode.AppendChild(_expressValue);

                        _expressValue = xmlResult.CreateElement("element");
                        _attrib = xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_GREATER.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2003), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_pref), new eNumber.SectionParam(_pref)), "<span class=\"specialItem\">\"</span>");
                        // _expressValue.InnerText = String.Concat(_res.GetRes(2003), " \"", _nQ, "\"");
                        _detailsNode.AppendChild(_expressValue);

                        break;
                    }

                #endregion
                #region TYPE_USER
                case FieldFormat.TYP_USER:

                    #region KHA LE 10/01/2014 : les users sont maintenant gérés par la méthode LoadUsers

                    // Pour les utilisateurs seul le débute par est géré par eudoquery.
                    //debute par
                    //_expressValue = xmlResult.CreateElement("element");
                    //_attrib = xmlResult.CreateAttribute("value");
                    //_attrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", _searchValue);
                    //_expressValue.Attributes.Append(_attrib);

                    //_attrib = xmlResult.CreateAttribute("type");
                    //_attrib.Value = "operator";
                    //_expressValue.Attributes.Append(_attrib);

                    //_expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref.Lang, 2006), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
                    //_detailsNode.AppendChild(_expressValue);
                    #endregion
                    break;

                #endregion
                #region OTHERS

                default:
                    {
                        // Pour tout les types de données
                        //debute par
                        _expressValue = xmlResult.CreateElement("element");
                        _attrib = xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", _searchValue);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2006), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
                        _detailsNode.AppendChild(_expressValue);

                        //Contient
                        _expressValue = xmlResult.CreateElement("element");
                        _attrib = xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_CONTAIN.GetHashCode().ToString(), ";|;", _searchValue);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_pref, 2009), " \"</span>", _searchValue, "<span class=\"specialItem\">\"</span>");
                        _detailsNode.AppendChild(_expressValue);
                        break;
                    }

                    #endregion
            }

            if (fldSrch.Format == FieldFormat.TYP_NUMERIC || fldSrch.Format == FieldFormat.TYP_MONEY)
                return;

            XmlNode _xmlMultiple = xmlResult.CreateElement("multiple");
            _detailsNode.AppendChild(_xmlMultiple);
            _xmlMultiple.InnerText = fldSrch.Multiple ? "1" : "0";

            bool _boolSep = false; //NBA
                                   // Key : DbValue ; Value : DisplayValue
            IDictionary<string, string> columnValue = new Dictionary<string, string>();

            foreach (var item in filler.ListRecords)
            {
                eFieldRecord fldRecord = item.GetFieldByAlias(fldSrch.Alias);

                if (!fldRecord.RightIsVisible)
                    continue;

                if (!_boolSep)
                {
                    // Mettre un séparateur au dessus du premier resultat (NBA)
                    _expressValue = xmlResult.CreateElement("element");
                    _attrib = xmlResult.CreateAttribute("type");
                    _attrib.Value = "separator";
                    _expressValue.Attributes.Append(_attrib);
                    _detailsNode.AppendChild(_expressValue);
                    _boolSep = true;
                    // Fin séparateur //
                }

                if (fldRecord.FldInfo.Multiple)
                {
                    string[] dbVal = fldRecord.Value.Split(";");
                    string[] displayVal = fldRecord.DisplayValue.Replace(" ; ", ";").Split(";");

                    if (dbVal.Length == displayVal.Length)
                    {
                        for (int i = 0; i < dbVal.Length; i++)
                        {
                            columnValue[dbVal[i]] = displayVal[i];
                        }
                    }
                }
                else
                {
                    columnValue[fldRecord.Value] = fldRecord.DisplayValue;
                }
            }

            foreach (var keyValue in columnValue)
            {
                _expressValue = xmlResult.CreateElement("element");
                _detailsNode.AppendChild(_expressValue);
                _expressValue.InnerText = keyValue.Value;

                _attrib = xmlResult.CreateAttribute("value");
                _expressValue.Attributes.Append(_attrib);
                _attrib.Value = keyValue.Key;
            }
        }

        class AllValuesItem
        {
            public string Value { get; set; }
            public string DisplayValue { get; set; }

            public override int GetHashCode()
            {
                return HashCodeGenerator.GenericHashCode(Value);
            }

            public override bool Equals(object obj)
            {
                AllValuesItem tmp = obj as AllValuesItem;
                if (tmp == null)
                    return false;

                return Value == tmp.Value;
            }

            public override string ToString()
            {
                return string.Format("Value:{0} / DisplayValue:{1}", Value, DisplayValue);
            }
        }

        private void EqAllValues(eudoDAL dal, int descid, ICollection<AllValuesItem> items)
        {
            string error;
            EudoQuery.EudoQuery query = null;
            string eqRequest = null;
            DataTableReaderTuned dtr = null;

            if (items == null)
                throw new ArgumentNullException("items is null !");

            int tab = eLibTools.GetTabFromDescId(descid);

            try
            {
                query = eLibTools.GetEudoQuery(_pref, tab, ViewQuery.FIELD_VALUES);
                if (!string.IsNullOrEmpty(query.GetError))
                    throw new Exception("Erreur EudoQuery.Init pour " + query.GetError);

                query.AddParam("FROMQFXRM", "1");
                query.SetListCol = descid.ToString();

                query.LoadRequest();
                if (!string.IsNullOrEmpty(query.GetError))
                    throw new Exception("Erreur EudoQuery.LoadRequest pour " + query.GetError);

                query.BuildRequest();
                if (!string.IsNullOrEmpty(query.GetError))
                    throw new Exception("Erreur EudoQuery.BuildRequest pour " + query.GetError);

                eqRequest = query.EqQuery;
                if (!String.IsNullOrEmpty(query.GetError))
                    throw new Exception("Erreur EudoQuery.GetQuery pour " + query.GetError);

                List<Field> fields = query.GetFieldHeaderList;

                RqParam rq = new RqParam(eqRequest);
                dtr = dal.Execute(rq, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);
                if (dtr == null)
                    throw new Exception(string.Format("Retour de requête vide {0}.", rq.GetSqlCommandText()));

                string fldAlias = string.Format("{0}_{1}", tab, descid);
                Field fld = fields.Find(d => d.Alias.Equals(fldAlias));
                if (fld == null)
                    throw new Exception(string.Format("Rubrique {0} non trouvée", fldAlias));

                while (dtr.Read())
                    items.Add(new AllValuesItem() { Value = dtr.GetString(fld.Alias), DisplayValue = dtr.GetString(fld.ValueAlias) });
            }
            finally
            {
                dtr?.Dispose();
                query?.CloseQuery();
            }
        }

        /// <summary>
        /// Charge toutes les valeurs dans le menu de filtre express
        /// Traite les cas particulier :
        ///  le chargement des valeurs du champ rubrique de la table historique                
        ///  le chargement des valeurs du champs type de la table Historique
        /// </summary>
        /// <param name="dal"></param>
        private void LoadAllValues(eudoDAL dal)
        {
            String error = String.Empty;
            FieldFormat format;

            FieldLite fldSrch = new FieldLite(_searchDescId);
            fldSrch.ExternalLoadInfo(dal, out error);

            format = fldSrch.Format;
            //if (format == FieldFormat.TYP_USER)
            //{
            //    eUserValue uservalueOnlyGP = new eUserValue(dal, _searchDescId, TypeUserValue.MULTIUSR_GROUP_ONLY, _pref.User);
            //    if (uservalueOnlyGP.Build() && uservalueOnlyGP.Enabled)
            //        format = FieldFormat.TYP_GROUP;
            //}

            //Arrete le traitement et envoi l'erreur
            if (error.Length != 0)
                throw new Exception(String.Concat("Impossible de trouver le FldSearch (", _searchDescId, ")."));

            //Catalogue utilisant les valeurs d'un autre catalogue
            if (fldSrch.Descid != fldSrch.PopupDescId
                && fldSrch.Popup != PopupType.FREE && fldSrch.Popup != PopupType.ONLY // #45624 : Pour les catalogues simples, on ne récupère plus les valeurs du catalogue
                && fldSrch.PopupDescId > 0)
                _searchDescId = fldSrch.PopupDescId;

            if (fldSrch.Descid == (int)HistoField.DESCID
                || fldSrch.Descid == (int)PaymentTransactionField.PAYTRANTARGETDESCID
                || fldSrch.Descid == (int)HistoField.EXPORT_TAB
                || fldSrch.Descid == (int)CampaignField.MAILADDRESSDESCID
                || fldSrch.Descid == (int)CampaignField.RECIPIENTTABID
                || fldSrch.Descid == (int)PJField.FILE
                )
            {
                // TODO : Separer la requête SQL dans un objet à part pour séparer (Utilisation d'EudoQuery?)

                #region Cas du champ rubrique/onglet de la table historique

                // Dans le cas d'un viewquery FIELD_VALUES, il n'y a pas de colonnes de droits, du coup le filler plante. Faudrai que le filler intégre le type d'eudoquery FIELD_VALUES

                try
                {
                    ISet<AllValuesItem> items = new HashSet<AllValuesItem>();
                    EqAllValues(dal, fldSrch.Descid, items);

                    foreach (AllValuesItem item in items)
                    {
                        // Creation du flux de retour qui contiendra toutes les valeurs
                        XmlNode expressValue = xmlResult.CreateElement("element");
                        _detailsNode.AppendChild(expressValue);
                        expressValue.InnerText = item.DisplayValue;

                        XmlAttribute expressAttrib = xmlResult.CreateAttribute("value");
                        expressValue.Attributes.Append(expressAttrib);
                        expressAttrib.Value = item.Value;
                    }
                }
                catch (Exception e)
                {
                    string strBaseError = @"Une erreur est survenue concernant le chargement de toutes les valeurs du champ [{fldSrch.Descid}] de la table historique.";

                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        strBaseError,
                        eResApp.GetRes(_pref, 72),
                        String.Concat(strBaseError, " : ", e.Message)
                    );

                    //Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                #endregion
            }

            else if (fldSrch.Descid == (int)HistoField.TYPE)
            {
                #region Chargement des valeurs du champs type de la table Historique

                foreach (HistoType status in Enum.GetValues(typeof(HistoType)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    Int32 idLabel = 0;
                    switch (status)
                    {
                        case HistoType.MOD_UPDATE:
                            idLabel = 805;
                            break;
                        case HistoType.MOD_CREATION:
                            idLabel = 738;
                            break;
                        case HistoType.MOD_DELETE:
                            idLabel = 806;
                            break;
                        case HistoType.MOD_HISTO_REPORT:
                            idLabel = 1153;
                            break;
                        default:
                            idLabel = 805;
                            break;
                    }



                    _expressValue.InnerText = eResApp.GetRes(_pref, idLabel);
                    _detailsNode.AppendChild(_expressValue);
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)UserField.LEVEL)
            {
                #region UserLevel

                foreach (UserLevel lvl in Enum.GetValues(typeof(UserLevel)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = ((int)lvl).ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    Int32 idLabel = 0;
                    switch (lvl)
                    {
                        case UserLevel.LEV_USR_NONE:
                            idLabel = -2;
                            break;
                        case UserLevel.LEV_USR_READONLY:
                            idLabel = 882;
                            break;
                        case UserLevel.LEV_USR_ADMIN:
                            idLabel = 194;
                            break;
                        case UserLevel.LEV_USR_SUPERADMIN:
                            idLabel = 7559;
                            break;
                        case UserLevel.LEV_USR_PRODUCT:
                            idLabel = 8324;
                            break;
                        default:
                            idLabel = -1;
                            break;
                    }

                    if (idLabel != -2)
                    {
                        if (idLabel != -1)
                            _expressValue.InnerText = eResApp.GetRes(_pref, idLabel);
                        else
                            _expressValue.InnerText = ((int)lvl).ToString();
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)UserField.Product)
            {
                List<eProduct> productsList = eProduct.GetProductsList(_pref, null);
                foreach (eProduct status in productsList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");

                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.ProductID.ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = status.ProductCode;
                    _detailsNode.AppendChild(_expressValue);
                }

            }
            else if (fldSrch.Descid == (int)UserField.PASSWORD_POLICIES_ALGO)
            {
                foreach (PASSWORD_ALGO algo in (PASSWORD_ALGO[])Enum.GetValues(typeof(PASSWORD_ALGO)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = algo.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetPassworAlgoResID(algo));
                    _detailsNode.AppendChild(_expressValue);
                }


            }
            else if (fldSrch.Descid == (int)FormularField.STATUS)
            {
                foreach (FORMULAR_STATUS status in (FORMULAR_STATUS[])Enum.GetValues(typeof(FORMULAR_STATUS)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetFormularStatusResID(status));
                    _detailsNode.AppendChild(_expressValue);
                }


            }
            //Statut de la transaction
            else if (fldSrch.Descid == (int)PaymentTransactionField.PAYTRANSTATUS)
            {
                foreach (StatusHostedCheckout status in (StatusHostedCheckout[])Enum.GetValues(typeof(StatusHostedCheckout)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetTransactionStatus(status).Item1);
                    _detailsNode.AppendChild(_expressValue);
                }
            }
            //Statut du paiement
            else if (fldSrch.Descid == (int)PaymentTransactionField.PAYTRANEUDOSTATUS)
            {
                foreach (PayTranEudoStatusEnum status in (PayTranEudoStatusEnum[])Enum.GetValues(typeof(PayTranEudoStatusEnum)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetPaymentStatus(status).Item1);
                    _detailsNode.AppendChild(_expressValue);
                }
            }
            //Statut prestataire
            else if (fldSrch.Descid == (int)PaymentTransactionField.PAYTRANCATEGORY)
            {
                foreach (PaymentSubStatus status in (PaymentSubStatus[])Enum.GetValues(typeof(PaymentSubStatus)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetIngenicoStatus(status).Item1);
                    _detailsNode.AppendChild(_expressValue);
                }
            }
            //Catégorie du statut
            else if (fldSrch.Descid == (int)PaymentTransactionField.PAYTRANCODE)
            {
                foreach (PaymentStatusCategory status in (PaymentStatusCategory[])Enum.GetValues(typeof(PaymentStatusCategory)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    _expressValue.InnerText = eResApp.GetRes(_pref, Outils.EnumToResId.GetStatusCategory(status).Item1);
                    _detailsNode.AppendChild(_expressValue);
                }
            }
            else if (fldSrch.Descid == (int)CampaignField.STATUS)
            {
                #region Status de campagne

                foreach (CampaignStatus status in Enum.GetValues(typeof(CampaignStatus)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    Int32 idLabel = 0;
                    switch (status)
                    {
                        case CampaignStatus.MAIL_IN_PREPARATION:
                            idLabel = 6461;
                            break;
                        case CampaignStatus.MAIL_WRKFLW_IN_PREPARATION:
                            idLabel = 3112;
                            break;
                        case CampaignStatus.MAIL_WRKFLW_READY:
                            idLabel = 3113;
                            break;
                        case CampaignStatus.MAIL_SENDING:
                            idLabel = 780;
                            break;
                        case CampaignStatus.MAIL_SENT:
                            idLabel = 685;
                            break;
                        case CampaignStatus.MAIL_DELAYED:
                            idLabel = 820;
                            break;
                        case CampaignStatus.MAIL_CANCELED:
                            idLabel = 6460;
                            break;
                        case CampaignStatus.MAIL_ERROR:
                            idLabel = 416;
                            break;
                        case CampaignStatus.MAIL_RECURRENT:
                            idLabel = 2037;
                            break;
                        default:
                            idLabel = 6461;
                            break;
                    }

                    _expressValue.InnerText = eResApp.GetRes(_pref, idLabel);
                    _detailsNode.AppendChild(_expressValue);
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)CampaignField.SENDTYPE)
            {
                #region Type d'envoi de campagne

                foreach (MAILINGSENDTYPE status in Enum.GetValues(typeof(MAILINGSENDTYPE)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    String label = "";
                    switch (status)
                    {
                        case MAILINGSENDTYPE.UNDEFINED:
                            break;
                        case MAILINGSENDTYPE.EUDONET:
                            label = "Eudonet Email";
                            break;
                        case MAILINGSENDTYPE.EUDONET_SMS:
                            label = "Eudonet SMS";
                            break;
                        case MAILINGSENDTYPE.ECIRCLE:
                            label = "Teradata Email";
                            break;
                        default:
                            label = "Eudonet";
                            break;
                    }

                    if (label.Length != 0)
                    {
                        _expressValue.InnerText = label;
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)CampaignStatsField.CATEGORY)
            {
                #region Categorie de stats de campagne

                foreach (CAMPAIGNSTATS_Category status in Enum.GetValues(typeof(CAMPAIGNSTATS_Category)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    Int32 idLabel = 0;
                    switch (status)
                    {
                        case CAMPAIGNSTATS_Category.NB_CLICK_PER_DAY:
                            idLabel = 6463;
                            break;
                        case CAMPAIGNSTATS_Category.NB_BOUNCE:
                            idLabel = 6464;
                            break;
                        case CAMPAIGNSTATS_Category.NB_UNSUBSCRIBE:
                            idLabel = 6466;
                            break;
                        case CAMPAIGNSTATS_Category.NB_VIEW:
                            idLabel = 6467;
                            break;
                        case CAMPAIGNSTATS_Category.NB_TOTAL:
                            idLabel = 6465;
                            break;
                        case CAMPAIGNSTATS_Category.NB_SENT:
                            idLabel = 6324;
                            break;
                        case CAMPAIGNSTATS_Category.NB_SINGLECLICKER:
                            idLabel = 6468;
                            break;
                        case CAMPAIGNSTATS_Category.NB_VIEWCLICKS:
                            idLabel = 1772;
                            break;
                        case CAMPAIGNSTATS_Category.NB_CLICKS:
                            idLabel = 1771;
                            break;
                        default:
                            idLabel = 6275;
                            break;
                    }

                    _expressValue.InnerText = eResApp.GetRes(_pref, idLabel);
                    _detailsNode.AppendChild(_expressValue);
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)RGPDTreatmentsLogsField.Type)
            {
                #region Types de traitements RGPD

                eEnumTools<RGPDRuleType> rgpdTypes = new eEnumTools<RGPDRuleType>();
                foreach (RGPDRuleType rgpdType in rgpdTypes.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = rgpdType.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetRGPDTypeResID(rgpdType);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)RGPDTreatmentsLogsField.Status)
            {
                #region Status journal des traitements

                eEnumTools<RGPDTreatmentLogStatus> treatmentStatus = new eEnumTools<RGPDTreatmentLogStatus>();
                foreach (RGPDTreatmentLogStatus status in treatmentStatus.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetRGPDStatusResID(status);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)RGPDTreatmentsLogsField.PersonnalDataCategoriesID)
            {
                #region Categories de données personnelles RGPD

                eEnumTools<DESCADV_RGPD_PERSONNAL_CATEGORY> rgpdCategories = new eEnumTools<DESCADV_RGPD_PERSONNAL_CATEGORY>();
                foreach (DESCADV_RGPD_PERSONNAL_CATEGORY category in rgpdCategories.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = category.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetRGPDCategoryResID(category);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid == (int)RGPDTreatmentsLogsField.SensibleDataID)
            {
                #region Categories de données sensibles RGPD

                eEnumTools<DESCADV_RGPD_SENSITIVE_CATEGORY> rgpdCategories = new eEnumTools<DESCADV_RGPD_SENSITIVE_CATEGORY>();
                foreach (DESCADV_RGPD_SENSITIVE_CATEGORY category in rgpdCategories.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = category.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetRGPDCategoryResID(category);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid % 100 == (int)MailField.DESCID_MAIL_STATUS)
            {
                #region statut des mails

                eEnumTools<EmailStatus> mailStatus = new eEnumTools<EmailStatus>();
                foreach (EmailStatus status in mailStatus.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetMailStatusResID(status);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion
            }
            else if (fldSrch.Descid % 100 == (int)MailField.DESCID_MAIL_SENDTYPE)
            {
                #region statut des mails

                foreach (MailSendType status in Enum.GetValues(typeof(MailSendType)))
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = status.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);
                    Int32 idLabel = 0;
                    switch (status)
                    {
                        case MailSendType.SMS:
                            idLabel = 655;
                            break;
                        default:
                            idLabel = 656;
                            break;
                    }

                    _expressValue.InnerText = eResApp.GetRes(_pref, idLabel);
                    _detailsNode.AppendChild(_expressValue);
                }

                #endregion
            }
            else if (fldSrch.Descid == FilterField.TAB.GetHashCode())
            {
                #region colonne "Table" de la liste des filtres
                // Creation du flux de retour qui contiendra toutes les valeurs
                // Boucler sur les valeurs du catalogue
                XmlNode _expressValue = xmlResult.CreateElement("element");
                XmlAttribute _attrib = xmlResult.CreateAttribute("value");
                _attrib.Value = "300";
                _expressValue.Attributes.Append(_attrib);
                _expressValue.InnerText = "Sociétés";
                _detailsNode.AppendChild(_expressValue);

                _expressValue = xmlResult.CreateElement("element");
                _attrib = xmlResult.CreateAttribute("value");
                _attrib.Value = "200";
                _expressValue.Attributes.Append(_attrib);
                _expressValue.InnerText = "Contacts";
                _detailsNode.AppendChild(_expressValue);
                #endregion
            }
            else if (fldSrch.Descid % 100 == (int)PlanningField.DESCID_CALENDAR_ITEM)
            {
                #region type de calendrier
                // Creation du flux de retour qui contiendra toutes les valeurs
                XmlNode _expressValueTask = xmlResult.CreateElement("element");
                XmlAttribute _attribTask = xmlResult.CreateAttribute("value");
                _attribTask.Value = PlanningType.CALENDAR_ITEM_TASK.GetHashCode().ToString();
                _expressValueTask.Attributes.Append(_attribTask);
                _expressValueTask.InnerText = eResApp.GetRes(_pref, 842);
                _detailsNode.AppendChild(_expressValueTask);

                XmlNode _expressValueAppointment = xmlResult.CreateElement("element");
                XmlAttribute _attribAppointment = xmlResult.CreateAttribute("value");
                _attribAppointment.Value = PlanningType.CALENDAR_ITEM_APPOINTMENT.GetHashCode().ToString();
                _expressValueAppointment.Attributes.Append(_attribAppointment);
                _expressValueAppointment.InnerText = eResApp.GetRes(_pref, 843);
                _detailsNode.AppendChild(_expressValueAppointment);

                #endregion
            }
            else if (fldSrch.Descid == (int)PJField.FILE)
            {
                //Annexe.Onglet
                #region Annexe.Onglet



                #endregion

            }
            else if (fldSrch.Descid == (int)PJField.TYPE)
            {
                #region Annexe.emplacement

                eEnumTools<PjType> pjTyp = new eEnumTools<PjType>();
                foreach (PjType typ in pjTyp.GetList)
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _expressValue = xmlResult.CreateElement("element");
                    XmlAttribute _attribUpdate = xmlResult.CreateAttribute("value");
                    _attribUpdate.Value = typ.GetHashCode().ToString();
                    _expressValue.Attributes.Append(_attribUpdate);

                    int? idLabel = Outils.EnumToResId.GetPjTypeResId(typ);
                    if (idLabel != null)
                    {
                        _expressValue.InnerText = eResApp.GetRes(_pref, idLabel.Value);
                        _detailsNode.AppendChild(_expressValue);
                    }
                }

                #endregion

            }
            else if (format == FieldFormat.TYP_USER)
            {
                // TODO : Separer la requête SQL dans un objet à part pour séparer (Utilisation d'EudoQuery?)

                #region Catalogue utilisateurs

                DataTableReaderTuned dtr = null;
                RqParam sqlAllValues = new RqParam();

                // MCR : 39565: Affichage Nom complet dans les filtres express catalogue user à la place du login
                // remplacer la colonne [USERLOGIN] par [USERDISPLAYNAME] dans la table : [USER]  et si la valeur de [USERDISPLAYNAME] est vide, prendre [USERLOGIN] à defaut

                // sqlAllValues.SetQuery("SELECT DISTINCT [USERID], [USERLOGIN] FROM [USER] WHERE ISNULL( [USERLOGIN], '' ) <> '' AND [USERDISABLED] <> 1 AND [USERHIDDEN] <> 1 ORDER BY [USERLOGIN]");

                sqlAllValues.SetQuery("SELECT DISTINCT [USERID],  isnull([USERDISPLAYNAME],[USERLOGIN] ) as  [USERDISPLAYNAME]   FROM [USER] WHERE isnull([USERDISPLAYNAME],[USERLOGIN] )  <> ''  AND  [USERDISABLED] <> 1 AND [USERHIDDEN] <> 1 ORDER BY [USERDISPLAYNAME]");

                try
                {
                    dtr = dal.Execute(sqlAllValues, out error);
                    if (dtr == null || !String.IsNullOrEmpty(error))
                    {
                        string strBaseError = "Une erreur est survenue concernant le chargement de toutes les valeurs sur les filtres rapides utilisateur.";

                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 6237),
                            strBaseError,
                            eResApp.GetRes(_pref, 72),
                            String.Concat(strBaseError, " : ", error)
                        );

                        //Arrete le traitement et envoi l'erreur
                        LaunchError();
                    }

                    while (dtr.Read())
                    {
                        // Creation du flux de retour qui contiendra toutes les valeurs
                        XmlNode expressValue = xmlResult.CreateElement("element");

                        XmlAttribute expressAttrib = xmlResult.CreateAttribute("value");

                        // Boucler sur les valeurs du catalogue
                        // MCR : 39565: Affichage Nom complet dans les filtres express catalogue user à la place du login
                        // remplacer la colonne [USERLOGIN] par [USERDISPLAYNAME] dans la table : [USER] 

                        expressAttrib.Value = dtr.GetEudoNumeric("userid").ToString();
                        expressValue.Attributes.Append(expressAttrib);
                        //_expressValue.InnerText = _dtrFieldsUser["userlogin"].ToString();
                        expressValue.InnerText = dtr.GetString("userdisplayname");
                        _detailsNode.AppendChild(expressValue);
                    }
                }
                finally
                {
                    dtr?.Dispose();
                }
                #endregion
            }
            else if (format == FieldFormat.TYP_GROUP)
            {
                XmlNode expressValue;
                XmlAttribute attrib;

                Dictionary<string, string> groups = eGroup.GetGroupIdNameList(_pref);
                var ordered = groups.OrderBy(g => g.Value);
                foreach (var kvp in ordered)
                {
                    expressValue = xmlResult.CreateElement("element");

                    attrib = xmlResult.CreateAttribute("value");
                    attrib.Value = String.Concat("G", kvp.Key);
                    expressValue.Attributes.Append(attrib);

                    expressValue.InnerText = kvp.Value;
                    _detailsNode.AppendChild(expressValue);
                }
            }
            else
            {
                bool isCatalog = format == FieldFormat.TYP_CHAR && fldSrch.Popup != PopupType.NONE;
                bool isSpecCatalog = isCatalog && fldSrch.Descid != fldSrch.PopupDescId && fldSrch.PopupDescId % 100 == 1;

                if (isCatalog && !isSpecCatalog)
                {

                    #region catalogue non spécieaux
                    if (fldSrch.Popup == PopupType.ONLY)
                        _searchDescId = fldSrch.PopupDescId;

                    eCatalog myCat = null;
                    if (fldSrch.Popup == PopupType.ENUM)
                    {
                        int nEnumType = _requestTools.GetRequestFormKeyI("enumtype") ?? 0;
                        eCatalogEnum.EnumType enumType = (eCatalogEnum.EnumType)nEnumType;

                        myCat = new eCatalogEnum(_pref, dal, _pref.User, eCatalogEnum.EnumType.EventStepStatus);
                        ((eCatalogEnum)myCat).Load();
                    }
                   else if (fldSrch.Popup == PopupType.DESC
                        && (
                                fldSrch.Descid == (int)WorkflowScenarioField.WFTTARGETDESCID
                                || fldSrch.Descid == (int)WorkflowScenarioField.WFTEVENTDESCID

                            )
                        )
                    {


                        // pour l'instant limité aux champs WFTTARGETDESCID et WFTEVENTDESCID

                        int nMode = _requestTools.GetRequestFormKeyI("datadesct") ?? 0;
                        eCatalogDesc.DescType tpDesc  = eLibTools.GetEnumFromCode<eCatalogDesc.DescType>(nMode);
                        myCat =   new eCatalogDesc(_pref, dal, _pref.User, tpDesc, new List<int>(), iDescId: 0);

                        ((eCatalogDesc)myCat).CheckTabPermission = true;

                        ((eCatalogDesc)myCat).Load();
                    }
                    else
                    {

                        myCat =
                              new eCatalog(dal, _pref, fldSrch.Popup, _pref.User, _searchDescId, fldSrch.bTreeView,
                              new List<eCatalog.CatalogValue>(), "", _pref.IsSnapshot, true, false, -1);
                    }

                    foreach (eCatalog.CatalogValue val in myCat.Values)
                    {
                        XmlNode _expressValue = xmlResult.CreateElement("element");

                        XmlAttribute _attrib = xmlResult.CreateAttribute("value");

                        // Boucler sur les valeurs du catalogue
                        _attrib.Value = eLibTools.CleanXMLChar(val.DbValue);
                        _expressValue.Attributes.Append(_attrib);
                        _expressValue.InnerText = eLibTools.CleanXMLChar(val.Label);
                        _detailsNode.AppendChild(_expressValue);
                    }
                    #endregion
                }
                else
                {
                    // catalogue spéciaux
                    LoadValuesFromQuery(dal, fldSrch, true);
                }
            }
        }

        private void QuickUserFilter(eudoDAL dal)
        {
            eUser eCatalogUser = new eUser(dal, _pref.User, eUser.ListMode.USERS_AND_GROUPS, _pref.GroupMode, null);

            StringBuilder sbErr = new StringBuilder();
            eCatalogUser.DisplayHiddenUser = false;
            List<eUser.UserListItem> uli = eCatalogUser.GetUserList(false, false, _searchValue, sbErr);

            if (sbErr.Length > 0)
            {
                string strBaseError = "Une erreur est survenue concernant la recherche sur les filtres rapides utilisateur.";

                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    strBaseError,
                    eResApp.GetRes(_pref, 72),
                    String.Concat(strBaseError, " : ", sbErr.ToString())
                );

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            foreach (eUser.UserListItem currentULI in uli)
            {
                if (!currentULI.Hidden)   //Pas les cachés
                {
                    // Creation du flux de retour qui contiendra toutes les valeurs
                    XmlNode _quickValue = xmlResult.CreateElement("element");

                    // Boucler sur les valeurs du catalogue
                    XmlAttribute _attrib = xmlResult.CreateAttribute("value");
                    _attrib.Value = currentULI.ItemCode;
                    _quickValue.Attributes.Append(_attrib);

                    XmlAttribute _attrib2 = xmlResult.CreateAttribute("usrgrp");
                    _attrib2.Value = (currentULI.Type == eUser.UserListItem.ItemType.GROUP) ? "grp" : "usr";
                    _quickValue.Attributes.Append(_attrib2);

                    XmlAttribute _attrib3 = xmlResult.CreateAttribute("disabled");
                    _attrib3.Value = (currentULI.Disabled) ? "1" : "0";
                    _quickValue.Attributes.Append(_attrib3);

                    _quickValue.InnerText = currentULI.Libelle;
                    _detailsNode.AppendChild(_quickValue);
                }
            }
        }
    }
}