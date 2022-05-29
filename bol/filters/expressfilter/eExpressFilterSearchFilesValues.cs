using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Génération des valeurs de filtres express pour les champ de liaison (catalogue spécial)
    /// Code extrait du manager des filtres express pour séparation des opérations sql/rendu/métier...
    /// 
    /// </summary>
    public class eExpressFilterSearchFilesValues : eExpressFilterSearchValues
    {
        /// <summary>
        /// Génère le xml
        /// </summary>
        protected override void generateValues( )
        {
            String error = String.Empty;

            string sFilterValue = _ExpressQuery.SearchValue;

            if (_ExpressQuery.SearchDescId == PJField.FILEID.GetHashCode())
            {
                throw new Exception("Pas de filtre express sur Annexe.Fiche");
            }

            WhereCustom wcSearch = null;
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
            if (_ExpressQuery.SearchFieldTab != TableType.PP.GetHashCode() && _ExpressQuery.SearchFieldTab != TableType.PM.GetHashCode() && _ExpressQuery.SearchDescId.ToString().EndsWith("01"))
                customsOperator = Operator.OP_CONTAIN;

            #endregion

            if (!_ExpressQuery.SearchAll)
                wcSearch = new WhereCustom(_ExpressQuery.SearchDescId.ToString(), customsOperator, sFilterValue);
            else
                wcSearch = new WhereCustom(_ExpressQuery.SearchDescId.ToString(), Operator.OP_IS_NOT_EMPTY, "");
            listWCsearch.Add(wcSearch);

            // PP01 : pour la recherche sur PP en utilisant avec nom et prenom
            if (_ExpressQuery.SearchDescId == TableType.PP.GetHashCode() + 1)
            {
                addNameFirstNameFiltersParams = true;

                wcSearch = new WhereCustom("COMPLETE_NAME_FIRSTNAME", customsOperator, sFilterValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);
            }
            // PM01
            else if (_ExpressQuery.SearchDescId == TableType.PM.GetHashCode() + 1)
            {
                // ASY : 23 591 => En liste commission, le filtre sur la colonne société
                //Ne retrouve pas INHA en tapant le sigle (institut national...) alors que ca fonctionne en V7
                // filtrer egalement sur les descid SIGLE et GROUPE
                wcSearch = new WhereCustom(PMField.GROUPE.GetHashCode().ToString(), customsOperator, sFilterValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);

                wcSearch = new WhereCustom(PMField.SIGLE.GetHashCode().ToString(), customsOperator, sFilterValue, InterOperator.OP_OR);
                listWCsearch.Add(wcSearch);
            }

            // HLA - Comportement pas normal pour une recherche sur filtre express, paramètre qui devrai s'appliquer uniquement sur les 01 des table MAIN et sur les rubrique de liaisons - #49725
            #region Champ de recherche complémentaire

            if (_ExpressQuery.FldSearch.Table.EdnType == EdnType.FILE_MAIN && _ExpressQuery.FldSearch.Table.TabType != TableType.ADR)
            {
                // Rubrique 01 ou liaison
                bool mainTableField = _ExpressQuery.SearchDescId % 100 == 1;

                if (mainTableField || _ExpressQuery.FldSearch.Popup == PopupType.SPECIAL)
                {
                    bool addAdvancedSearchDescId = true;
                    int targetTab = _ExpressQuery.FldSearch.Table.DescId;
                    if (_ExpressQuery.FldSearch.Popup == PopupType.SPECIAL)
                    {
                        targetTab = eLibTools.GetTabFromDescId(_ExpressQuery.FldSearch.PopupDescId);

                        EqCacheLinkedTable ca = new EqCacheLinkedTable();
                        ca.SetDal = _dal;
                        TabLink tabLink = ca.Get(new DicoKeyAdvTabLink(_ExpressQuery.FldSearch.Table.TabType, _ExpressQuery.FldSearch.Table.DescId, targetTab));
                        addAdvancedSearchDescId = tabLink.HasLink;
                        ca = null;
                    }

                    if (addAdvancedSearchDescId)
                    {
                        Int32 advancedSearchDescId = 0;

                        RqParam rq = new RqParam("SELECT isnull(AdvancedSearchDescId, 0) as AdvancedSearchDescId FROM [desc] WHERE descid = @TargetTab");
                        rq.AddInputParameter("@TargetTab", System.Data.SqlDbType.Int, targetTab);
                        DataTableReaderTuned dtrTuned = null;
                        try
                        {
                            dtrTuned = _dal.Execute(rq, out error);
                            if (error.Length == 0 && dtrTuned != null && dtrTuned.Read())
                            {
                                // recupere le champs de recherche complementaire
                                advancedSearchDescId = eLibTools.GetNum(dtrTuned.GetString("AdvancedSearchDescId"));
                            }

                            if (advancedSearchDescId != 0)
                            {
                                wcSearch = new WhereCustom(advancedSearchDescId.ToString(), customsOperator, sFilterValue, InterOperator.OP_OR);
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

            TableLite _tb = new TableLite(_ExpressQuery.SearchFieldTab);
            _tb.ExternalLoadInfo(_dal, out error);

            if (_ExpressQuery.SearchFieldTab != TableType.USER.GetHashCode()
                && _ExpressQuery.SearchFieldTab != TableType.FILTER.GetHashCode()
                && _ExpressQuery.SearchFieldTab != TableType.REPORT.GetHashCode())
            {

                if (_tb.AdrJoin && _ExpressQuery.ParentTabFileId > 0)
                {
                    String rqWC = String.Empty;
                    if (_ExpressQuery.SearchFieldTab == TableType.PP.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [", _tb.TabName, "].[PPID] FROM [", _tb.TabName, "] WHERE [", _tb.TabName, "].[EVTID] = ", _ExpressQuery.ParentTabFileId);
                        wcSearch = new WhereCustom("PPID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else if (_ExpressQuery.SearchFieldTab == TableType.PM.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [ADDRESS].[PMID] FROM [", _tb.TabName, "] INNER JOIN [address] ON [", _tb.TabName, "].[ADRID] = [ADDRESS].[ADRID]",
                            " WHERE [", _tb.TabName, "].[EVTID] = ", _ExpressQuery.ParentTabFileId);
                        wcSearch = new WhereCustom("PMID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else if (_ExpressQuery.SearchFieldTab == TableType.ADR.GetHashCode())
                    {
                        rqWC = String.Concat("SELECT [", _tb.TabName, "].[ADRID] FROM [", _tb.TabName, "] WHERE [", _tb.TabName, "].[EVTID] = ", _ExpressQuery.ParentTabFileId);
                        wcSearch = new WhereCustom("ADRID", Operator.OP_IN_LIST, rqWC, InterOperator.OP_AND);
                        wcSearch.IsSubQuery = true;
                    }
                    else
                    {
                        wcSearch = new WhereCustom("EVTID", Operator.OP_EQUAL, _ExpressQuery.ParentTabFileId.ToString(), InterOperator.OP_AND);
                    }

                    listWCglobal.Add(wcSearch);
                }
            }

            #endregion


            #region Filtre sur la tables des FILTRES/RAPPORT/Modèles
            if (_ExpressQuery.ParentTabDescId != _ExpressQuery.SearchFieldTab)
            {
                WhereCustom wcTab = null;
                if (_ExpressQuery.SearchFieldTab == TableType.FILTER.GetHashCode())
                {
                    wcTab = new WhereCustom(FilterField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, _ExpressQuery.ParentTabDescId.ToString());
                }
                else if (_ExpressQuery.SearchFieldTab == TableType.REPORT.GetHashCode())
                {
                    wcTab = new WhereCustom(ReportField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, _ExpressQuery.ParentTabDescId.ToString());
                }
                else if (_ExpressQuery.SearchFieldTab == TableType.MAIL_TEMPLATE.GetHashCode())
                {
                    wcTab = new WhereCustom(MailTemplateField.TAB.GetHashCode().ToString(), Operator.OP_EQUAL, _ExpressQuery.ParentTabDescId.ToString());
                }
                if (wcTab != null)
                {
                    listWCglobal.Add(wcTab);
                }
            }
            #endregion

 
        


            if (listWCsearch != null)
                listWCglobal.Add(new WhereCustom(listWCsearch));

            eDataFillerGeneric filler = new eDataFillerGeneric(_prefUser, _ExpressQuery.SearchFieldTab, ViewQuery.CUSTOM);
            filler.EudoqueryComplementaryOptions =
                    delegate (EudoQuery.EudoQuery eq)
                    {
                        eq.SetListCol = _ExpressQuery.SearchDescId.ToString();

                        if (!_ExpressQuery.SearchAll)
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
                string msgUser = "Erreur de récupération des valeurs de filtre express";


                if (filler.InnerException != null)
                {
                    msg = string.Concat(msg, Environment.NewLine, filler.InnerException.Message);

                    if(filler.InnerException is EudoInternalException)
                        throw filler.InnerException;
                    else if (filler.InnerException is EudoException)
                        msgUser = ((EudoException)filler.InnerException).UserMessage;
                    
                }

                throw EudoInternalException.GetEudoInternalException(
                    sShortUserMessage: "Erreur de récupération des valeurs de filtre express",
                    sTitle: "Erreur de récupération des valeurs de filtre express",
                    sDetailUserMessage: msgUser,
                    sDebugError: msg,
                    typCrit: eLibConst.MSG_TYPE.CRITICAL,
                    ex: filler.InnerException ?? new Exception(msg)
                    );

               
            }

            #region Information sur la rubrique de recherche

            Field fldSrch = filler.FldFieldsInfos.Find(e => e.Descid == _ExpressQuery.SearchDescId);

            // Arrete le traitement et renvoi l erreur
            if (fldSrch == null)
                throw new Exception("Impossible de trouver le FldSearch (" + _ExpressQuery.SearchDescId + ") depuis la liste <GetFieldHeaderList> d'EudoQuery.");

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
                        Decimal.TryParse(_ExpressQuery.SearchValue, out _nQ);

                        XmlNode _xmlNumeric = _xmlResult.CreateElement("isnumeric");
                        _detailsNode.AppendChild(_xmlNumeric);
                        _xmlNumeric.InnerText = "1";

                        _expressValue = _xmlResult.CreateElement("element");

                        _attrib = _xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_EQUAL.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = _xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId , 2000), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_prefUser), new eNumber.SectionParam(_prefUser)), "<span class=\"specialItem\">\"</span>");

                        // _expressValue.InnerText = String.Concat(_res.GetRes(2000), " \"", _nQ, "\"");
                        _detailsNode.AppendChild(_expressValue);

                        _expressValue = _xmlResult.CreateElement("element");
                        _attrib = _xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_LESS.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = _xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2001), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_prefUser), new eNumber.SectionParam(_prefUser)), "<span class=\"specialItem\">\"</span>");
                        //   _expressValue.InnerText = String.Concat(_res.GetRes(2001), " \"", _nQ, "\"");
                        _detailsNode.AppendChild(_expressValue);

                        _expressValue = _xmlResult.CreateElement("element");
                        _attrib = _xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_GREATER.GetHashCode().ToString(), ";|;", _nQ);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = _xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2003), " \"</span>", eNumber.FormatNumber(_nQ, new eNumber.DecimalParam(_prefUser), new eNumber.SectionParam(_prefUser)), "<span class=\"specialItem\">\"</span>");
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
                        _expressValue = _xmlResult.CreateElement("element");
                        _attrib = _xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_START_WITH.GetHashCode().ToString(), ";|;", _ExpressQuery.SearchValue);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = _xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2006), " \"</span>", _ExpressQuery.SearchValue, "<span class=\"specialItem\">\"</span>");
                        _detailsNode.AppendChild(_expressValue);

                        //Contient
                        _expressValue = _xmlResult.CreateElement("element");
                        _attrib = _xmlResult.CreateAttribute("value");
                        _attrib.Value = string.Concat(EudoQuery.Operator.OP_CONTAIN.GetHashCode().ToString(), ";|;", _ExpressQuery.SearchValue);
                        _expressValue.Attributes.Append(_attrib);

                        _attrib = _xmlResult.CreateAttribute("type");
                        _attrib.Value = "operator";
                        _expressValue.Attributes.Append(_attrib);

                        _expressValue.InnerText = String.Concat("<span class=\"specialItem\">", eResApp.GetRes(_uInfos.UserLangServerId, 2009), " \"</span>", _ExpressQuery.SearchValue, "<span class=\"specialItem\">\"</span>");
                        _detailsNode.AppendChild(_expressValue);
                        break;
                    }

                    #endregion
            }

            if (fldSrch.Format == FieldFormat.TYP_NUMERIC || fldSrch.Format == FieldFormat.TYP_MONEY)
                return;

            XmlNode _xmlMultiple = _xmlResult.CreateElement("multiple");
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
                    _expressValue = _xmlResult.CreateElement("element");
                    _attrib = _xmlResult.CreateAttribute("type");
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
                _expressValue = _xmlResult.CreateElement("element");
                _detailsNode.AppendChild(_expressValue);
                _expressValue.InnerText = keyValue.Value;

                _attrib = _xmlResult.CreateAttribute("value");
                _expressValue.Attributes.Append(_attrib);
                _attrib.Value = keyValue.Key;
            }
        }
    }
}