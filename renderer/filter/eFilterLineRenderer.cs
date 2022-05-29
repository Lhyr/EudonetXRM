using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Ligne d'un tab de filtre
    /// </summary>
    internal class eFilterLineRenderer
    {

        /// <summary>                        
        /// Renderer de la line              
        /// </summary>
        internal static HtmlGenericControl GetLineRenderer(AdvFilterContext filterContext, AdvFilterLineIndex lineIndex, bool emptyLineRender = false, string lineReportClass = "ligne", eFilterRenderer.eFilterRendererParams param = null)
        {
            if (param == null)
                param = new eFilterRenderer.eFilterRendererParams();

            AdvFilterLine filterLine = lineIndex.GetLine(filterContext.Filter);

            HtmlGenericControl line = new HtmlGenericControl("div");

            if (emptyLineRender)
                line.Attributes.Add("class", "form_ligne");
            else
                line.Attributes.Add("class", lineReportClass);

            line.ID = String.Concat(param.PrefixFilter, "line_", filterLine.TabLineIndex);

            if (String.IsNullOrEmpty(lineReportClass))
                line.Style.Add(HtmlTextWriterStyle.Display, "inline-block");

            if (!emptyLineRender && !param.bFromChartReport)
            {
                HtmlGenericControl op = new HtmlGenericControl("div");
                line.Controls.Add(op);
                op.Attributes.Add("class", "operateur");
                if (!filterLine.LineOperator.Equals(InterOperator.OP_NONE))
                {
                    op.Controls.Add(eFilterRenderer.GetLogicalOperatorList(filterContext.Pref,
                        string.Concat(param.PrefixFilter, "and_", filterLine.TabLineIndex), filterLine.LineOperator, "onChangeFilterLineOp(this)", false, emptyLineRender));
                }
            }

            line.Controls.Add(GetFieldsList(filterContext, filterLine, emptyLineRender, param: param));

            if (filterLine.DescId > 0 && !param.bInitialEfChart)
            {
                if (param.bGetFilterOpAndValue)
                {
                    line.Controls.Add(GetOperatorsList(filterContext, lineIndex, emptyLineRender, param: param));
                    line.Controls.Add(GetValuesList(filterContext, lineIndex, param: param));
                }


                if (!param.bFromChartReport && !emptyLineRender && filterLine.LineIndex > 0)
                {
                    HtmlGenericControl delete = new HtmlGenericControl("div");
                    delete.Attributes.Add("class", "icon-delete icnFltBtn");
                    delete.Attributes.Add("onclick", String.Concat("DelFilterLine(", filterLine.Tab.TabIndex, ",", filterLine.LineIndex, ",true)"));
                    line.Controls.Add(delete);
                }
            }

            return line;
        }

        /// <summary>
        /// Retourne le champs de selection de valeur
        /// </summary>
        internal static Control GetValuesList(AdvFilterContext filterContext, AdvFilterLineIndex lineIndex, bool bFromTreat = false, eFilterRenderer.eFilterRendererParams param = null)
        {
            if (param == null)
                param = new eFilterRenderer.eFilterRendererParams();

            AdvFilterLine filterLine = lineIndex.GetLine(filterContext.Filter);

            // Valeur à afficher
            string displayedValue = String.Empty;

            ePrefUser pref = filterContext.Pref;

            HtmlGenericControl divReturn = new HtmlGenericControl("div");
            divReturn.ID = String.Concat(param.PrefixFilter, "DivValues_", filterLine.TabLineIndex);
            divReturn.Attributes.Add("class", string.Concat("options ", param.bFromChartReport ? "eFilterOptionsValue" : "optionsValue"));

            Operator op = filterLine.Operator;

            if (!bFromTreat)
            {
                HtmlGenericControl txtOP = new HtmlGenericControl("input");
                txtOP.Attributes.Add("type", "text");
                txtOP.ID = String.Concat(param.PrefixFilter, "op_", filterLine.TabLineIndex);
                txtOP.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                txtOP.Attributes.Add("value", ((int)op).ToString());
                divReturn.Controls.Add(txtOP);
            }

            HtmlGenericControl txtValue;

            if (op == Operator.OP_IS_EMPTY
                || op == Operator.OP_IS_NOT_EMPTY
                || op == Operator.OP_IS_FALSE
                || op == Operator.OP_IS_TRUE)
            {
                txtValue = new HtmlGenericControl("input");
                txtValue.Attributes.Add("type", "text");
                txtValue.Attributes.Add("ednvalue", filterLine.Value);
                txtValue.ID = String.Concat(param.PrefixFilter, "value_", filterLine.TabLineIndex);
                txtValue.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                divReturn.Controls.Add(txtValue);

                // HLA
                eIconCtrl linkAction = new eIconCtrl("LinkActionMulti");
                linkAction.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                divReturn.Controls.Add(linkAction);

                return divReturn;
            }

            string error = String.Empty;
            FieldFilterInfo fld = null, fldBound = null;
            FieldFormat fldFormat = FieldFormat.TYP_CHAR;
            string strCase = String.Empty;

            bool bSendTypeField = false, bStatusField = false, bCalendarItem = false, bIsMailStatusFilter = false;//bIsMailStatusFilter: si c'est un filtre concernat le statut des emails

            bool bFreeText = (op == Operator.OP_LESS
                || op == Operator.OP_LESS_OR_EQUAL
                || op == Operator.OP_GREATER
                || op == Operator.OP_GREATER_OR_EQUAL
                || op == Operator.OP_START_WITH
                || op == Operator.OP_END_WITH
                || op == Operator.OP_CONTAIN
                || op == Operator.OP_NOT_START_WITH
                || op == Operator.OP_NOT_END_WITH
                || op == Operator.OP_NOT_CONTAIN);




            // Recherche propriété de la rubrique ainsi que de sa table
            if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.CURRENT_USER.GetHashCode())
            {
                fldFormat = FieldFormat.TYP_USER;
                if (op == Operator.OP_CONTAIN || op == Operator.OP_NOT_CONTAIN)
                    bFreeText = false;
            }
            else if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.NB_FILES.GetHashCode())
            {
                fldFormat = FieldFormat.TYP_NUMERIC;
            }
            else
            {
                // Si exception, on laisse propager l'erreur
                fld = filterContext.CacheFieldInfo.Get(filterLine.DescId);
                fldFormat = fld.Format;
                bIsMailStatusFilter = fldFormat == FieldFormat.TYP_EMAIL && (op == Operator.OP_MAIL_STATUS_IN_LIST || op == Operator.OP_MAIL_STATUS_EQUAL);//si c'est un filtre concernat le statut des emails


                if (fld.Format == FieldFormat.TYP_DATE)
                    bFreeText = false;

                if ((op == Operator.OP_CONTAIN || op == Operator.OP_NOT_CONTAIN) && (fld.Format == FieldFormat.TYP_USER || fld.Format == FieldFormat.TYP_GROUP))
                    bFreeText = false;



                // Informations sur le Bound
                if (fld.PopupDescId > 0)
                    fldBound = filterContext.CacheFieldInfo.Get(fld.PopupDescId);

                if (bFromTreat && fld.Table.DescId != filterContext.FilterTab && fld.Descid % 100 == 1)
                {
                    fldBound = fld;
                    bFreeText = false;
                }


                #region FILTRE DOUBONS / RELATIONS

                if ((filterContext.FilterType == TypeFilter.DBL || filterContext.FilterType == TypeFilter.RELATION)
                && filterLine.DescId > 0)
                {
                    if (String.IsNullOrEmpty(filterLine.Value))
                        filterLine.Value = String.Concat("$", filterLine.DescId, "$");

                    bool selected = false;

                    HtmlGenericControl _lst = new HtmlGenericControl("select");
                    _lst.Attributes.Add("class", "DblListValue");
                    _lst.ID = String.Concat("value_", filterLine.TabLineIndex);

                    //Rubriques de même type
                    // ISO V5/V7
                    // ==> Pas de gestion de droits sur les champs vu que seul l'admin peut modifier le filtre 
                    //des doublons et l'affichage de tous les champs est nécessaire 
                    string sql = new StringBuilder()
                        .Append("SELECT fld.[DescId], restab.[").Append(pref.Lang).Append("] + '.' + resfld.[").Append(pref.Lang).Append("] AS [FullText] ")
                        .Append("FROM [desc] fld ")
                        .Append("	INNER JOIN [res] restab on fld.[DescId] - fld.[DescId] % 100 = restab.[ResId] ")
                        .Append("	LEFT JOIN [res] resfld ON fld.[DescId] = resfld.[ResId] ")
                        .Append("WHERE fld.[DescId] >= @tab AND fld.[DescId] < @tab+99 AND fld.[Format] = @fieldformat ")
                        .Append("ORDER BY [FullText] ")
                        .ToString();

                    RqParam rq = new RqParam(sql);
                    rq.AddInputParameter("@tab", SqlDbType.Int, fld.Table.DescId);
                    rq.AddInputParameter("@fieldformat", SqlDbType.Int, (int)fld.Format);

                    DataTableReaderTuned dtr = null;
                    try
                    {
                        dtr = filterContext.Dal.Execute(rq, out error);
                        while (dtr.Read())
                        {
                            HtmlGenericControl _itm = new HtmlGenericControl("option");
                            if (filterLine.Value.Equals(string.Concat("$", dtr.GetEudoNumeric("descid").ToString(), "$")))
                            {
                                //bSelected = false;
                                selected = true;
                                _itm.Attributes.Add("selected", "selected");
                            }

                            _itm.Attributes.Add("value", string.Concat("$", dtr.GetEudoNumeric("descid").ToString(), "$"));
                            _itm.InnerText = dtr.GetString("FullText").ToString();
                            _lst.Controls.Add(_itm);
                        }
                    }
                    finally
                    {
                        dtr?.Dispose();
                    }

                    //Pas de champ personnalisé pour les filtres doublons
                    if (filterContext.FilterType != TypeFilter.DBL)
                    {
                        HtmlGenericControl _itm = new HtmlGenericControl("option");
                        _itm.InnerText = String.Concat("<", eResApp.GetRes(pref, 817), ">");
                        //_lst.Controls.Add(_itm);
                    }

                    //Champs spéciaux pour filtre Relation
                    if (filterContext.FilterType == TypeFilter.RELATION && op == Operator.OP_EQUAL)
                    {
                        eRes res = new eRes(pref, String.Join(",", (new List<string>() { ((int)TableType.PP).ToString(), ((int)TableType.PM).ToString(), fld.Table.DescId.ToString() }).ToArray()));

                        string metaValue = "";

                        HtmlGenericControl _itm = new HtmlGenericControl("option"); ;

                        if (fld.PopupDescId == (fld.Table.DescId + 1))
                        {
                            metaValue = "$fileid$";

                            _itm.Attributes.Add("value", metaValue);

                            string text = eResApp.GetRes(pref, 7589); // Fiche <TAB> en cours
                            bool resFound = false;
                            string textReplace = res.GetRes(fld.Table.DescId, out resFound);
                            if (resFound)
                                text = text.Replace("<TAB>", textReplace);
                            else
                                text = text.Replace("<TAB>", "");

                            _itm.InnerText = String.Concat("<", text, ">");
                            _lst.Controls.Add(_itm);
                        }
                        else if (fld.PopupDescId == ((int)TableType.PM + 1))
                        {
                            metaValue = "$pmid$";

                            _itm.Attributes.Add("value", metaValue);

                            string text = eResApp.GetRes(pref, 7590); //Fiche <PARENTTAB> de la fiche <TAB> en cours
                            bool resFound = false;
                            string textReplace = res.GetRes(fld.Table.DescId, out resFound);
                            if (resFound)
                                text = text.Replace("<TAB>", textReplace);
                            else
                                text = text.Replace("<TAB>", "");

                            textReplace = res.GetRes((int)TableType.PM, out resFound);
                            if (resFound)
                                text = text.Replace("<PARENTTAB>", textReplace);
                            else
                                text = text.Replace("<PARENTTAB>", "Société");

                            _itm.InnerText = String.Concat("<", text, ">");
                            _lst.Controls.Add(_itm);
                        }
                        else if (fld.PopupDescId == ((int)TableType.PP + 1))
                        {
                            metaValue = "$ppid$";

                            _itm.Attributes.Add("value", metaValue);

                            string text = eResApp.GetRes(pref, 7590); //Fiche <PARENTTAB> de la fiche <TAB> en cours
                            bool resFound = false;
                            string textReplace = res.GetRes(fld.Table.DescId, out resFound);
                            if (resFound)
                                text = text.Replace("<TAB>", textReplace);
                            else
                                text = text.Replace("<TAB>", "");

                            textReplace = res.GetRes((int)TableType.PP, out resFound);
                            if (resFound)
                                text = text.Replace("<PARENTTAB>", textReplace);
                            else
                                text = text.Replace("<PARENTTAB>", "Contact");

                            _itm.InnerText = String.Concat("<", text, ">");
                            _lst.Controls.Add(_itm);
                        }

                        if (!String.IsNullOrEmpty(metaValue) && filterLine.Value.Equals(metaValue))
                        {
                            selected = true;
                            _itm.Attributes.Add("selected", "selected");
                        }
                    }

                    if (!selected)
                    {
                        HtmlGenericControl _itm = new HtmlGenericControl("option");
                        _itm.Attributes.Add("value", filterLine.Value);
                        _itm.Attributes.Add("selected", "selected");
                        _itm.InnerText = filterLine.Value;
                        _lst.Controls.Add(_itm);
                    }

                    divReturn.Controls.Add(_lst);
                    return divReturn;
                }

                #endregion

                #region  AUTRES TYPES DE FILTRE (REGLES - FILTRE UTILISATEUR...)
                switch (fld.Table.EdnType)
                {
                    case EdnType.FILE_MAIL:
                    case EdnType.FILE_SMS:
                        bSendTypeField = fld.Descid == fld.Table.DescId + MailField.DESCID_MAIL_SENDTYPE.GetHashCode();
                        bStatusField = fld.Descid == fld.Table.DescId + MailField.DESCID_MAIL_STATUS.GetHashCode();
                        break;
                    case EdnType.FILE_PLANNING:
                        bCalendarItem = fld.Descid == fld.Table.DescId + PlanningField.DESCID_CALENDAR_ITEM.GetHashCode();
                        break;
                }
                #endregion

                if (fld.Table.TabType == TableType.CAMPAIGN)
                {
                    bSendTypeField = fld.Descid == CampaignField.SENDTYPE.GetHashCode();
                    bStatusField = fld.Descid == CampaignField.STATUS.GetHashCode();
                }

                #region Case du champs de saisie

                switch (fld.Case)
                {
                    case CaseField.CASE_NONE:
                        strCase = String.Empty;
                        break;
                    case CaseField.CASE_UPPER:
                        strCase = "uppercase;";
                        break;
                    case CaseField.CASE_CAPITALIZE:
                        strCase = "capitalize;";
                        break;

                    case CaseField.CASE_LOWER:
                        strCase = "lowercase;";
                        break;
                }

                #endregion
            }

            StringBuilder stringLabel = new StringBuilder();
            String ednValue = filterLine.Value;
            PopupType popup = PopupType.NONE;
            if (fld != null)
            {
                popup = fld.Popup;
                if (bFromTreat && fld.Table.DescId != filterContext.FilterTab && fld.Descid % 100 == 1)
                    popup = PopupType.SPECIAL;
                else if (bIsMailStatusFilter)
                    popup = PopupType.DATA;
            }

            switch (fldFormat)
            {
                case FieldFormat.TYP_HIDDEN:        //SECTEURS / CLES PP-PM - DEPRICATED DANS LA V7
                    break;
                case FieldFormat.TYP_CHAR:
                case FieldFormat.TYP_FILE:
                    displayedValue = filterLine.Value;

                    if (fld == null)
                        break;

                    switch (popup)
                    {
                        #region PopupType.DATA
                        case PopupType.DATA:
                            // 41988 CRU : retrait de la condition bFreeText car lorsque c'est une valeur de catalogue avancé, la valeur peut être saisie OU sélectionnée
                            //if (bFreeText)
                            //    break;

                            List<eCatalog.CatalogValue> li = new List<eCatalog.CatalogValue>();
                            List<int> lst = filterLine.Value.ConvertToListInt(";");

                            foreach (int id in lst)
                                li.Add(new eCatalog.CatalogValue(id, -1, String.Empty));

                            if (li.Count == 0)
                                break;

                            //#40231 - fldbound utilisé si disponible
                            //#70 096 - on met showHiddenValues à true pour afficher les valeurs désactivées
                            eCatalog _cat = new eCatalog(
                                filterContext.Dal,
                                pref,
                                PopupType.DATA,
                                pref.User,
                                fldBound != null ? fldBound.PopupDescId : filterLine.DescId,
                                false,
                                li,
                                showHiddenValues: true,
                                isSnapshot: pref.IsSnapshot);
                            stringLabel.Length = 0;
                            foreach (eCatalog.CatalogValue v in _cat.Values)
                                stringLabel.Append(v.DisplayValue).Append(";");
                            if (stringLabel.Length > 0)
                                displayedValue = stringLabel.ToString(0, stringLabel.Length - 1);

                            break;
                        #endregion

                        #region PopupType.ONLY
                        case PopupType.ONLY:
                            break;
                        #endregion

                        #region PopupType.SPECIAL
                        case PopupType.SPECIAL:
                            if (bFreeText)
                                break;

                            // HLA - On exclus les possibles liaisons vers les tables non gérées (ADR, NOTIF, etc.)
                            if (fldBound.Table.TabType != TableType.PM && fldBound.Table.TabType != TableType.PP && fldBound.Table.TabType != TableType.EVENT)
                                break;

                            ISet<int> fileIds = new HashSet<int>();
                            ISet<int> lstDescId = new HashSet<int>() { fld.PopupDescId };
                            bool multipleVal = filterLine.Value.Contains(';');

                            if (multipleVal)
                            {
                                fileIds.UnionWith(filterLine.Value.ConvertToListInt(";"));
                            }
                            else
                            {
                                int fileId = eLibTools.GetNum(filterLine.Value);
                                if (fileId > 0)
                                    fileIds.Add(fileId);
                            }

                            if (fileIds.Count == 0)
                                break;

                            eFieldRecord fldRec;
                            Dictionary<int, eFieldRecord> dicFldRecord;
                            stringLabel.Length = 0;
                            foreach (int id in fileIds)
                            {
                                try
                                {
                                    dicFldRecord = eDataFillerGeneric.GetFieldsValue(pref, lstDescId, fldBound.Table.DescId, id);
                                }
                                catch (eDataFillerGeneric.DataFillerRowNotFound)
                                {
                                    // On ignore les enregistrements non trouvés
                                    break;
                                }

                                if (dicFldRecord.TryGetValue(fld.PopupDescId, out fldRec))
                                    stringLabel.Append(fldRec.DisplayValue).Append(";");
                            }
                            if (stringLabel.Length > 0)
                                displayedValue = stringLabel.ToString(0, stringLabel.Length - 1);

                            break;
                        #endregion

                        case PopupType.ENUM:


                            if (!string.IsNullOrEmpty(ednValue))
                            {

                                displayedValue = "";
                                var allVal = ednValue.Split(";");


                                foreach (string val in allVal)
                                {
                                    if (displayedValue.Length > 0)
                                        displayedValue += ";";

                                    if (fld.Descid == (int)RGPDTreatmentsLogsField.PersonnalDataCategoriesID)
                                    {
                                        DESCADV_RGPD_PERSONNAL_CATEGORY a = eLibTools.GetEnumFromCode<DESCADV_RGPD_PERSONNAL_CATEGORY>(val);
                                        if (a != DESCADV_RGPD_PERSONNAL_CATEGORY.UNSPECIFIED)
                                            displayedValue += eResApp.GetRes(pref, Outils.EnumToResId.GetRGPDCategoryResID(a) ?? 0);
                                    }
                                    else if (fld.Descid == (int)RGPDTreatmentsLogsField.SensibleDataID)
                                    {
                                        DESCADV_RGPD_SENSITIVE_CATEGORY a = eLibTools.GetEnumFromCode<DESCADV_RGPD_SENSITIVE_CATEGORY>(val);
                                        if (a != DESCADV_RGPD_SENSITIVE_CATEGORY.UNSPECIFIED)
                                            displayedValue += eResApp.GetRes(pref, Outils.EnumToResId.GetRGPDCategoryResID(a) ?? 0);
                                    }
                                    else if (fld.Descid == (int)UserField.PASSWORD_POLICIES_ALGO)
                                        displayedValue += eResApp.GetRes(pref, Outils.EnumToResId.GetPassworAlgoResID(eLibTools.GetEnumFromCode<PASSWORD_ALGO>(val)));
                                    else if (fld.Descid == (int)FormularField.STATUS)
                                        displayedValue += eResApp.GetRes(pref, Outils.EnumToResId.GetFormularStatusResID(eLibTools.GetEnumFromCode<FORMULAR_STATUS>(val)));

                                }
                            }
                            break;

                        case PopupType.DESC:
                            if (Int32.TryParse(filterLine.Value, out int nRes) && nRes > 0)
                                displayedValue = eLibTools.GetFullNameByDescId(filterContext.Dal, nRes, pref.Lang);
                            else
                                displayedValue = filterLine.Value;
                            break;
                           

                        default:
                            displayedValue = filterLine.Value;
                            break;
                    }
                    break;
                case FieldFormat.TYP_DATE:

                    string[] aDateValue;
                    string strValue = filterLine.Value;
                    int nMoveValue = 0;
                    if (filterLine.Value.IndexOf("+") > 0)
                    {
                        aDateValue = filterLine.Value.Split(' ');
                        strValue = aDateValue[0].Replace(" ", "");
                        nMoveValue = eLibTools.GetNum(aDateValue[1].ToUpper().Replace(" ", "").Replace("[NOYEAR]", ""));
                    }
                    else if (filterLine.Value.IndexOf("-") > 0)
                    {
                        aDateValue = filterLine.Value.Split(' ');
                        strValue = aDateValue[0].Replace(" ", "");
                        nMoveValue = eLibTools.GetNum(aDateValue[1].ToUpper().Replace(" ", "").Replace("[NOYEAR]", ""));
                    }
                    else
                    {
                        nMoveValue = 0;
                    }

                    string strMove2 = String.Empty;

                    //Dates Anniversaires
                    if (filterLine.Value.Contains("[NOYEAR]"))
                    {
                        //bNoYear = true;
                        displayedValue = filterLine.Value.Replace("[NOYEAR]", "");
                        strMove2 = String.Concat(" (", eResApp.GetRes(pref, 1495).ToLower(), ")");
                        strValue = strValue.Replace("[NOYEAR]", "");
                    }

                    string strDateLibelle = String.Empty;
                    switch (strValue.ToUpper())
                    {
                        case "<DATE>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 367), ">");
                            break;
                        case "<DATETIME>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 368), ">");
                            break;
                        case "<MONTH>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 693), ">");
                            break;
                        case "<WEEK>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 694), ">");
                            break;
                        case "<YEAR>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 778), ">");
                            break;
                        case "<DAY>":
                            strDateLibelle = String.Concat("<", eResApp.GetRes(pref, 1234), ">");
                            break;
                        default:
                            // On prend en compte le format de date
                            strDateLibelle = String.IsNullOrEmpty(filterLine.Value) ? String.Empty : eDate.ConvertBddToDisplay(pref.CultureInfo, filterLine.Value);
                            // Internationalisation
                            ednValue = strDateLibelle;
                            break;
                    }

                    string strMoveValue = String.Empty;
                    if (nMoveValue != 0)
                    {
                        if (nMoveValue > 0)
                            strMoveValue = " + " + nMoveValue;
                        else
                            strMoveValue = " - " + Math.Abs(nMoveValue);

                        strDateLibelle = strDateLibelle + strMoveValue;
                    }
                    //Dates Anniversaires
                    if (!String.IsNullOrEmpty(strMove2))
                        strDateLibelle = strDateLibelle + strMove2;


                    //Dates Anniversaires
                    //if( bNoYear )
                    //    strDefaultValue = defValue + "[NOYEAR]";

                    displayedValue = strDateLibelle;

                    break;

                case FieldFormat.TYP_USER:
                case FieldFormat.TYP_GROUP:
                    string strUserLogin = String.Empty;

                    if (filterLine.Value.ToUpper().Equals("<USER>"))
                        displayedValue = String.Concat("<", eResApp.GetRes(pref, 370), ">");
                    else
                        if (filterLine.Value.ToUpper().Equals("<GROUP>"))
                        displayedValue = String.Concat("<", eResApp.GetRes(pref, 963), ">");
                    else
                    {
                        IDictionary<string, string> dicoUser = eLibDataTools.GetUserLogin(pref, filterLine.Value);
                        filterLine.Value = String.Join(";", dicoUser.Keys);
                        displayedValue = String.Join(", ", dicoUser.Values);
                    }

                    break;
                case FieldFormat.TYP_EMAIL:
                    if (!string.IsNullOrEmpty(ednValue) && (op == Operator.OP_MAIL_STATUS_EQUAL || op == Operator.OP_MAIL_STATUS_IN_LIST))
                    {
                        displayedValue = "";
                        var allVal = ednValue.Split(";");

                        foreach (string val in allVal)
                        {
                            if (displayedValue.Length > 0)
                                displayedValue += ";";

                            EmailValidationEudoStatus a = eLibTools.GetEnumFromCode<EmailValidationEudoStatus>(val);
                            displayedValue += eResApp.GetRes(pref, Outils.EnumToResId.GetEmailValidationEudoStatusResID(a) ?? 0);
                        }
                    }
                    else
                        displayedValue = filterLine.Value;
                    break;
                default:
                    displayedValue = filterLine.Value;
                    break;
            }

            txtValue = new HtmlGenericControl("input");
            //Cat Arbo
            divReturn.Controls.Add(txtValue);
            txtValue.Attributes.Add("type", "text");

            if (param.bFromChartReport)
                txtValue.Style.Add(HtmlTextWriterStyle.Height, "13pt");

            txtValue.Attributes.Add("ednformat", fldFormat.GetHashCode().ToString());

            if (fldFormat == FieldFormat.TYP_AUTOINC
                || fldFormat == FieldFormat.TYP_ID
                || fldFormat == FieldFormat.TYP_MONEY
                || fldFormat == FieldFormat.TYP_NUMERIC)
            {
                txtValue.Attributes.Add("ednfreetext", "1");

                //
                Decimal dc;
                if (Decimal.TryParse(ednValue, out dc))
                {
                    if (fld != null)
                    {
                        displayedValue = eNumber.FormatNumber(pref, dc, (dc % 1 != 0) ? fld.Length : 0, true);
                        //converti le format au format bdd sans séparateur de millier
                        ednValue = eNumber.FormatNumber(pref, dc, fld.Length, false);
                    }
                    else
                    {
                        displayedValue = eNumber.FormatNumber(pref, dc, 0, true);
                        //converti le format au format bdd sans séparateur de millier
                        ednValue = eNumber.FormatNumber(pref, dc, 0, false);
                    }
                }
                else
                {
                    // Avant l'internationalisation les valeurs numériques dans les filtres pouvaient être stockée
                    // sous différents format.
                    // par exemple, "50.5" et "50,5" étaient interprété de la même manière
                    // avec l'internationalisation, ce ne plus possible directement
                    // "50.5" n'est par exemple plus valide pour le mode SEMIFR (format ##.###,##)
                    //  on tente donc de préserver cet existant

                    // Après discussion avec l'équipe R&D, il est décidé de considérer le . comme séparateur de millier s'il est unique dans la valeur

                    if (ednValue.Length - ednValue.Replace(".", "").Length == 1 // 1 seul séparateur
                                                                                //&& ((ednValue.Length - ednValueIndexOf(".") - 1) <= fld.Length) // autant ou moins de chiffre décimaux que possible pour le champ
                                                                                //&& ((ednValue.Length - ednValue.IndexOf(".") - 1) != 3) // pas 3 chiffre après le décimal
                        && (Decimal.TryParse(ednValue
                        .Replace(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, "")
                        .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out dc))  // alors on peut considérer que le "." est probablement un séparateur de décimal
                        )
                    {
                        displayedValue = eNumber.FormatNumber(pref, dc, (dc % 1 != 0) ? fld.Length : 0, true);

                        //converti le format au format bdd sans séparateur de millier
                        ednValue = eNumber.FormatNumber(pref, dc, fld.Length, false);

                    }
                }
            }

            txtValue.Attributes.Add("ednvalue", ednValue);
            txtValue.ID = String.Concat(param.PrefixFilter, "value_", filterLine.TabLineIndex);

            txtValue.Attributes.Add("ednfromtreat", bFromTreat ? "1" : "0");

            if (fld != null)
                txtValue.Attributes.Add("treeview", fld.PopupDataRend == PopupDataRender.TREE ? "1" : "0");

            if (!String.IsNullOrEmpty(strCase))
                txtValue.Style.Add("text-transformation", strCase);

            //Débute par ...etc -> champs de saisie libre...
            txtValue.Attributes.Add("onchange", "doOnchangeText(this);");
            if (bFreeText)
            {
                txtValue.Attributes.Add("ednfreetext", "1");
            }
            else
            {
                if (popup != PopupType.NONE || fldFormat == FieldFormat.TYP_USER || fldFormat == FieldFormat.TYP_DATE || fldFormat == FieldFormat.TYP_GROUP)
                {
                    txtValue.Attributes.Add("ReadOnly", "true");
                    txtValue.Attributes.Add("class", "readonly");
                    txtValue.Attributes.Add("ednfreetext", "0");
                }
            }

            if (fld != null)
            {
                txtValue.Attributes.Add("advanced", popup == PopupType.DATA || fld.Popup == PopupType.DATA ? "1" : "0");
                if (bFromTreat && fld.Table.DescId != filterContext.FilterTab && fld.Descid % 100 == 1)
                {
                    txtValue.Attributes.Add("popup", ((int)PopupType.SPECIAL).ToString());
                    txtValue.Attributes.Add("pud", fld.Descid.ToString());

                }
                else
                {
                    if (bIsMailStatusFilter)
                        txtValue.Attributes.Add("popup", PopupType.ENUM.GetHashCode().ToString());
                    else
                        txtValue.Attributes.Add("popup", fld.Popup.GetHashCode().ToString());

                    if (fld.PopupDescId > 0 && fld.PopupDescId != fld.Descid)
                        txtValue.Attributes.Add("pud", fld.PopupDescId.ToString());
                }

            }

            txtValue.Attributes.Add("edndescid", filterLine.DescId.ToString());
            txtValue.Attributes.Add("value", HttpUtility.UrlDecode(displayedValue.Replace("+", "$add$")).Replace("$add$", "+"));

            // HLA
            HtmlGenericControl icnSpan;
            icnSpan = new HtmlGenericControl("span");
            icnSpan.Attributes.Add("class", "");   //A vide car la css de base peut empecher l'affichage de l'image (notament sur eudopart et formulaire de recherche)
            switch (fldFormat)
            {
                case FieldFormat.TYP_HIDDEN:
                    //SECTEURS / CLES PP-PM - Depricated?
                    break;
                case FieldFormat.TYP_CHAR:
                case FieldFormat.TYP_EMAIL:
                case FieldFormat.TYP_WEB:
                case FieldFormat.TYP_SOCIALNETWORK:
                case FieldFormat.TYP_MEMO:
                case FieldFormat.TYP_FILE:
                case FieldFormat.TYP_PHONE:
                    //Débute par ...etc -> champs de saisie libre...
                    txtValue.Attributes.Add("format", fldFormat.GetHashCode().ToString());

                    if (bFreeText)
                    {
                        txtValue.Attributes.Add("ednfreetext", "1");
                    }
                    else
                    {
                        if (fld.Multiple)
                            txtValue.Attributes.Add("ReadOnly", "true");
                        else if (fld.Popup == PopupType.ONLY || fld.Popup == PopupType.DATA || fld.Popup == PopupType.SPECIAL || bIsMailStatusFilter)
                            txtValue.Attributes.Add("ReadOnly", "true");
                        else
                            txtValue.Attributes.Add("ednfreetext", "1");
                    }

                    bool bSpecial = fld.Popup == PopupType.SPECIAL
                        || fld.PopupDescId == 201 || fld.PopupDescId == 301
                        || (fld.Table.EdnType == EdnType.FILE_MAIN && fld.Descid == fld.Table.DescId + 1);

                    txtValue.Attributes.Add("special", bSpecial ? "1" : "0");

                    if (bSpecial)
                    {
                        txtValue.Attributes.Add("popupdescid", ((fld.PopupDescId > 0) ? fld.PopupDescId : eLibTools.GetTabFromDescId(fld.Descid)).ToString());
                        txtValue.Attributes.Add("ednop", ((int)op).ToString());
                    }


                    //Affichage en multiple
                    //   si opérateur = sur un treeview, on ne peut choisir qu'une valeur (iso v7)
                    bool bMultiSelect = (
                        op == Operator.OP_IN_LIST
                        || op == Operator.OP_NOT_IN_LIST
                        || (fld.Multiple && op != Operator.OP_EQUAL)
                        || (fld.Multiple && bFromTreat)
                        || (fldFormat == FieldFormat.TYP_EMAIL && op == Operator.OP_MAIL_STATUS_IN_LIST)
                        );

                    //Catalgue/catalogue spécial 01/ user ou groupe + est dans la liste/n'est pas dans la liste ==> affichage multiple
                    // ASY (25816): changement du test pour que le bouton "Montrer catalogue" ne soit pas affiché s'il ne s'agit pas d'un catalogue
                    //BSE:#57 501 n'afficher que le champ texte à saisie libre si l'opérateur et OP_CONTAIN ou OP_NOT_CONTAIN 
                    // CRU #60211 : Ne pas afficher de catalogue pour le champ principal d'un TEMPLATE
                    if (
                        (fld.Popup != PopupType.NONE ||
                        bIsMailStatusFilter ||
                        (fld.Descid == fld.Table.MainFieldDescId && fld.Descid != (int)UserField.LOGIN && fld.Table.TabType != TableType.TEMPLATE) && filterContext.FilterType != TypeFilter.DEFAULT)
                        && op != Operator.OP_CONTAIN && op != Operator.OP_NOT_CONTAIN
                    )
                    {
                        txtValue.Attributes.Add("multi", bMultiSelect ? "1" : "0");
                        icnSpan.Attributes.Add("onclick", String.Concat("showCat(", filterLine.Tab.TabIndex, ",",
                            filterLine.LineIndex,
                            string.IsNullOrEmpty(param.PrefixFilter) ? "" : ",'" + param.PrefixFilter + "'", ")"));

                        if (fld.IsCatEnum)
                        {
                            txtValue.Attributes.Add("eaction", "LNKCATENUM");

                            txtValue.Attributes.Add("data-enumt", ((int)eCatalogEnum.GetCatalogEnumTypeFromFieldInfo(filterLine.DescId, null)).ToString());
                        }
                        else if (bIsMailStatusFilter)
                        {
                            txtValue.Attributes.Add("data-enumt", ((int)eCatalogEnum.EnumType.MailEudoValidationStatus).ToString());
                            txtValue.Attributes.Add("EnumType", ((int)eCatalogEnum.EnumType.MailEudoValidationStatus).ToString());
                            txtValue.Attributes.Add("CatSource", ((int)eCatalogDialog.CatSource.Enum).ToString());
                            txtValue.Attributes.Add("CatPopupType", "6");
                            txtValue.Attributes.Add("eaction", "LNKCATENUM");
                        }

                        else if (fld.IsCatDesc)
                        {
                            txtValue.Attributes.Add("eaction", "LNKCATDESC");

                            if (
                                    filterLine.DescId == (int)RGPDTreatmentsLogsField.TabsID
                                || filterLine.DescId == (int)WorkflowScenarioField.WFTEVENTDESCID 
                                || filterLine.DescId == (int)WorkflowScenarioField.WFTTARGETDESCID
                                )
                                txtValue.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Table).ToString());
                            else if (filterLine.DescId == (int)RGPDTreatmentsLogsField.FieldsID)
                                txtValue.Attributes.Add("data-desct", ((int)eCatalogDesc.DescType.Field).ToString());
                           
                        }

                    }
                    else
                    {
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    }

                    if (bFromTreat)
                    {
                        txtValue.Attributes.Add("ednbounddescid", fld.BoundDescid.ToString());
                        txtValue.Attributes.Add("ednboundpopup", fldBound != null ? fldBound.Popup.GetHashCode().ToString() : "0");

                        if (fldFormat == FieldFormat.TYP_MEMO)
                        {
                            //affichage d'une combo liste
                            HtmlSelect optionMemo = new HtmlSelect();
                            txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                            optionMemo.Attributes.Add("onchange", String.Concat("oCurrentWizard.updateMemoDuplTreat(this);"));
                            optionMemo.Attributes.Add("ednidx", filterLine.LineIndex.ToString());
                            optionMemo.Attributes.Add("class", "trt_UpdWithNew_bit_Dupli");
                            optionMemo.Attributes.Add("width", "320px");

                            optionMemo.Items.AddRange(new ListItem[] {
                                new ListItem(eResApp.GetRes(pref, 141), "0"),
                                new ListItem(eResApp.GetRes(pref, 528), "1")
                            });

                            optionMemo.SelectedIndex = 1;
                            divReturn.Controls.Add(optionMemo);
                        }
                    }
                    break;

                case FieldFormat.TYP_DATE:
                    // Necessaire pour la conversion de la dislayDate en dbDate #36014
                    txtValue.Attributes.Add("format", fldFormat.GetHashCode().ToString());
                    icnSpan.Attributes.Add("onclick", String.Concat("showCal(", filterLine.Tab.TabIndex, ",",
                        filterLine.LineIndex, ",", bFromTreat ? "1" : "0",
                            string.IsNullOrEmpty(param.PrefixFilter) ? "" : ",'" + param.PrefixFilter + "'", ")"));
                    break;

                case FieldFormat.TYP_BIT:
                    icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");

                    if (bFromTreat)
                    {
                        //affichage d'une combo liste

                        HtmlSelect optionCAC = new HtmlSelect();

                        txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                        optionCAC.Attributes.Add("onchange", String.Concat("oCurrentWizard.updateBitDuplTreat(this);"));
                        optionCAC.Attributes.Add("ednidx", filterLine.LineIndex.ToString());
                        optionCAC.Attributes.Add("class", "trt_UpdWithNew_bit_Dupli");
                        optionCAC.ID = "selbitdupli_" + filterLine.TabLineIndex;
                        optionCAC.Attributes.Add("width", "320px");

                        //fldCtrl.Controls.Add(optionCAC);

                        List<ListItem> listCAC = new List<ListItem>();
                        listCAC.Add(new ListItem(eResApp.GetRes(pref, 309), "0"));   //est décochée
                        listCAC.Add(new ListItem(eResApp.GetRes(pref, 308), "1"));   //est cochée

                        optionCAC.Items.AddRange(listCAC.ToArray());

                        optionCAC.SelectedIndex = 0;
                        divReturn.Controls.Add(optionCAC);

                    }
                    break;

                case FieldFormat.TYP_AUTOINC:
                    icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    break;

                case FieldFormat.TYP_MONEY:
                    icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    break;

                case FieldFormat.TYP_USER:
                case FieldFormat.TYP_GROUP:
                    bool bMultiUsr = (op == Operator.OP_IN_LIST || op == Operator.OP_NOT_IN_LIST || (bFromTreat && fld.Multiple));
                    txtValue.Attributes.Add("multi", bMultiUsr ? "1" : "0");
                    icnSpan.Attributes.Add("onclick", String.Concat("showUserCat(", filterLine.Tab.TabIndex, ",", filterLine.LineIndex, ",", bFromTreat ? "1" : "0",
                            string.IsNullOrEmpty(param.PrefixFilter) ? "" : ",'" + param.PrefixFilter + "'", ")"));
                    break;

                case FieldFormat.TYP_NUMERIC:
                    icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    if (bCalendarItem)
                    {
                        txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                        //if (filterLine.Value == "")
                        //    filterLine.Value = "0";
                        txtValue.Attributes.Add("value", filterLine.Value);

                        divReturn.Controls.Add(GetCalendarItemList(pref, filterLine));
                    }
                    else if (bSendTypeField)
                    {
                        #region Champ type envoi
                        txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                        txtValue.Attributes.Add("value", filterLine.Value);

                        if (fld.Table.TabType == TableType.CAMPAIGN)
                            divReturn.Controls.Add(GetCampaignSendTypeList(pref, filterLine));
                        else
                            divReturn.Controls.Add(GetMailSendTypeList(pref, filterLine));
                        #endregion
                    }
                    else if (bStatusField)
                    {
                        #region Champ statut d'envoi
                        txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                        txtValue.Attributes.Add("value", filterLine.Value);

                        if (fld.Table.TabType == TableType.CAMPAIGN)
                            divReturn.Controls.Add(GetCampaignStatusList(pref, filterLine));
                        else if (fld.Table.TabType == TableType.PAYMENTTRANSACTION)
                        {
                            divReturn.Controls.Add(GetPaymentStatusList(pref, filterLine));
                            divReturn.Controls.Add(GetTransactionStatusList(pref, filterLine));
                            divReturn.Controls.Add(GetStatusCategoryList(pref, filterLine));
                            divReturn.Controls.Add(GetIngenicoStatusList(pref, filterLine));
                        }
                        else
                            divReturn.Controls.Add(GetMailStatusList(pref, filterLine));
                        #endregion
                    }
                    else if (filterLine.DescId == (int)RGPDTreatmentsLogsField.Type || filterLine.DescId == (int)RGPDTreatmentsLogsField.Status)
                    {
                        #region Champs Type et Statut journal de traitements RGPD
                        txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                        txtValue.Attributes.Add("value", filterLine.Value);

                        if (filterLine.DescId == (int)RGPDTreatmentsLogsField.Type)
                            divReturn.Controls.Add(GetEnumList<RGPDRuleType>(pref, filterLine, item => { return Outils.EnumToResId.GetRGPDTypeResID(item); }));
                        else if (filterLine.DescId == (int)RGPDTreatmentsLogsField.Status)
                            divReturn.Controls.Add(GetEnumList<RGPDTreatmentLogStatus>(pref, filterLine, item => { return Outils.EnumToResId.GetRGPDStatusResID(item); }));
                        #endregion
                    }
                    else if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.NB_FILES.GetHashCode())
                    {
                        icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                    }
                    else if (fld.Table.EdnType == EdnType.FILE_HISTO)
                    {
                        if (fld.Descid == HistoField.TYPE.GetHashCode())
                        {
                            txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                            icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                            //if (filterLine.Value == "")
                            //filterLine.Value = "0";
                            txtValue.Attributes.Add("value", filterLine.Value);
                            divReturn.Controls.Add(GetHistoActionList(pref, filterLine));
                        }
                        else if (fld.Descid == HistoField.DESCID.GetHashCode())
                        {
                            txtValue.Style.Add(HtmlTextWriterStyle.Display, "none");
                            icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                            //if (filterLine.Value == "")
                            //    filterLine.Value = "0";
                            txtValue.Attributes.Add("value", filterLine.Value);
                            divReturn.Controls.Add(GetHistoFieldsList(filterContext, filterLine));
                        }
                        else if (fld.Descid == fld.Table.DescId + HistoField.FILEID.GetHashCode())
                        {
                            icnSpan.Attributes.Add("onclick", String.Concat("SearchPpRecord(", filterLine.Tab.TabIndex, ",", filterLine.LineIndex, ")"));
                        }
                        else
                        {
                            icnSpan.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                        }
                    }
                    break;
            }

            // MAB/PNO - Qu'on édite une valeur multiple ou simple, on utilise pour le moment la même icône
            //if (txtValue.Attributes["multi"] == "1")
            //    linkAction.AddClass("rIco LNKCATPOPUPbtn btn");
            //else

            //    if (fld != null && fld.Popup != PopupType.NONE)
            icnSpan.Attributes.Add("class", "rIco icon-catalog btn");

            divReturn.Controls.Add(icnSpan);

            displayedValue = HttpUtility.UrlDecode(displayedValue);


            if (op == Operator.OP_EQUAL && fldFormat == FieldFormat.TYP_BIT && param.bFromChartReport)
            {
                divReturn.Style.Add(HtmlTextWriterStyle.Visibility, "hidden");
                divReturn.Style.Add(HtmlTextWriterStyle.Display, "none");
            }


            return divReturn;
        }

        /// <summary>
        /// Retourne un opérateur inter-lignes
        /// </summary>
        internal static HtmlGenericControl GetInterLineOperator(AdvFilterContext filterContext, AdvFilterLineIndex lineIndex, bool endOperator, bool bFromChartReport = false)
        {
            AdvFilterLine filterLine = lineIndex.GetLine(filterContext.Filter);

            HtmlGenericControl op = new HtmlGenericControl("div");

            String _id = string.Concat("and_", filterLine.TabLineIndex);
            if (endOperator)
            {
                op.ID = "EndLineOperatorContainer_" + filterLine.Tab.TabIndex;
                op.Attributes.Add("class", "EndLineOperator");
                _id = "end_operator_line_" + filterLine.Tab.TabIndex;
            }
            else
                op.Attributes.Add("class", "operateur");

            op.Controls.Add(eFilterRenderer.GetLogicalOperatorList(filterContext.Pref, _id, InterOperator.OP_NONE, "onChangeFilterLineOp(this)", false, bFromChartReport: bFromChartReport));
            return op;
        }

        /// <summary>
        /// Liste des champs
        /// </summary>
        private static HtmlGenericControl GetFieldsList(AdvFilterContext filterContext, AdvFilterLine filterLine, bool emptyLineRender = false, eFilterRenderer.eFilterRendererParams param = null)
        {
            if (param == null)
                param = new eFilterRenderer.eFilterRendererParams();

            IEnumerable<eFieldLiteWithLib> list = null;
            int firstFieldDescId = 0;
            HtmlGenericControl lstDiv = new HtmlGenericControl("div");
            lstDiv.Attributes.Add("class", param.bFromChartReport ? "fChartOptions" : "options");

            HtmlGenericControl lstFields = new HtmlGenericControl("select");
            lstDiv.Controls.Add(lstFields);
            lstFields.ID = String.Concat(param.PrefixFilter, "field_", filterLine.TabLineIndex);
            lstFields.Attributes.Add("name", String.Concat(param.PrefixFilter, "field_", filterLine.TabLineIndex));
            if (!param.bFromChartReport)
                lstFields.Attributes.Add("onchange", String.Concat("onChangeField(this, ", filterContext.IsFromAdmin ? "true" : "false", ")"));

            else
            {
                if (param.bSetFieldAction)
                    lstFields.Attributes.Add("onchange",
                        string.Concat("onChangeChartReportField(this",
                        param.bCombinedExpressFilter ? string.Concat(",'" + param.PrefixFilter.ToLower() + "'") : "",
                        ")"));
                lstFields.Attributes.Add("eF", "1");

                HtmlGenericControl _lstItem = new HtmlGenericControl("option");
                _lstItem.Attributes.Add("value", "-1");
                lstFields.Controls.Add(_lstItem);
            }


            bool bOrderByDescId = false;

            #region Récupération de la liste des DescIDs autorisés rattachés au tabDescId

            if (!param.bFromChartReport)
            {
                var fields = RetrieveFields.GetDefault(filterContext.Pref)
                    .AddOnlyThisTabs(new int[] { filterLine.Tab.Table.DescId })
                    // TODO en attente des operateurs de filtrage sur un champ géo, on l'exclut pour le moment
                    //BSE:US 765 Exclure Rubrique Mot de Passe
                    .AddExcludeFormats(new FieldFormat[] { FieldFormat.TYP_GEOGRAPHY_V2, FieldFormat.TYP_GEOGRAPHY_OLD, FieldFormat.TYP_ALIASRELATION, FieldFormat.TYP_ALIAS, FieldFormat.TYP_PASSWORD });

                // On retire les rubriques non gérés des tables systèmes
                switch (filterLine.Tab.Table.DescId)
                {
                    case (int)TableType.CAMPAIGN:
                        fields.AddExcludeDescId(new int[] { (int)CampaignField.MAILADDRESSDESCID, (int)CampaignField.RECIPIENTTABID });
                        break;
                    case (int)TableType.PJ:
                        fields.AddExcludeDescId(new int[] { (int)PJField.FILE });
                        break;
                    case (int)TableType.HISTO:
                        fields.AddExcludeDescId(new int[] { (int)HistoField.EXPORT_TAB, (int)HistoField.DESCID });
                        break;
                    case (int)TableType.PAYMENTTRANSACTION:
                        fields.AddExcludeDescId(new int[] { (int)PaymentTransactionField.PAYTRANTARGETDESCID });
                        break;
                }

                list = fields.ResultFieldsInfo(eFieldLiteWithLib.Factory(filterContext.Pref));
            }

            else
            {
                list = RetrieveFields.GetDefault(filterContext.Pref)
                .AddOnlyThisTabs(new int[] { filterLine.Tab.Table.DescId })
                .AddOnlyThisFormats(param.LstFildsFormat)
                .AddExcludeFormats(new FieldFormat[] { FieldFormat.TYP_ALIASRELATION, FieldFormat.TYP_ALIAS })
                .ResultFieldsInfo(eFieldLiteWithLib.Factory(filterContext.Pref));

            }
            // Tri par libelle
            list = list?.OrderBy(fld => fld.Libelle);

            //Concatène les descId de la liste
            string sListCol = eLibTools.Join(";", list.Select(fld => fld.Descid));

            // Uniquement les rubriques sur lesquelles on a les permissions, lorsqu'on n'est pas en mode admin

            IEnumerable<eFieldLiteWithLib> allowedFields = list;


            if (param.bFromChartReport)
                allowedFields = list.Where(fld => fld.Popup != PopupType.SPECIAL).Where(fld => fld.Descid - filterLine.Tab.Table.DescId >= 1);

            Dictionary<int, string> fldList = new Dictionary<int, string>();
            if (!filterContext.IsFromAdmin)
            {
                fldList = eLibTools.GetAllowedFieldsFromDescIds(filterContext.Dal, filterContext.Pref.User, sListCol, bOrderByDescId);
                allowedFields = allowedFields.Where(fld => fldList.ContainsKey(fld.Descid));
            }

            #endregion

            string _optCss = "cell";

            foreach (eFieldLiteWithLib field in allowedFields)
            {
                if (!param.bFromChartReport)
                {
                    if (_optCss.Equals("cell"))
                        _optCss = "cell2";
                    else
                        _optCss = "cell";
                }


                HtmlGenericControl _lstItem = new HtmlGenericControl("option");
                _lstItem.Attributes.Add("value", field.Descid.ToString());

                _lstItem.InnerHtml = field.Libelle;
                if (!param.bFromChartReport)
                    _lstItem.Attributes.Add("class", _optCss);
                else
                {
                    _lstItem.Attributes.Add("fmt", ((int)field.Format).ToString());
                    _lstItem.Attributes.Add("ptp", ((int)field.Popup).ToString());

                }

                if (field.PopupDescId > 0)
                {

                    _lstItem.Attributes.Add("pud", field.PopupDescId.ToString());
                }

                if (field.Descid == filterLine.DescId || (!param.bFromChartReport && filterLine.DescId == 0 && field.Descid == filterLine.Tab.Table.DescId + 1))
                {
                    _lstItem.Attributes.Add("selected", "selected");
                    firstFieldDescId = field.Descid;


                    param.sDisplayedFiledsFmt = field.Format;

                    if (param.nDisplayedFieldsPud > 0)
                        param.nDisplayedFieldsPud = field.PopupDescId;
                }


                if (param.PrefixFilter.ToLower() == eLibConst.COMBINED_Z.ToLower())
                {
                    if (param.sDisplayedFiledsFmt.HasValue && param.sDisplayedFiledsFmt.Value != field.Format)
                        _lstItem.Attributes.Add("display", "0");

                    if (param.nDisplayedFieldsPud.HasValue && param.nDisplayedFieldsPud.Value > 0 && param.nDisplayedFieldsPud != field.PopupDescId)
                        _lstItem.Attributes.Add("display", "0");
                }




                lstFields.Controls.Add(_lstItem);

                if (firstFieldDescId == 0)
                    firstFieldDescId = field.Descid;
            }

            // Si la ligne est nouvelle, elle n'a donc pas de rubrique selectionnée au départ. On selectionne donc la première trouvée
            if (filterLine.DescId <= 0)
                filterLine.DescId = firstFieldDescId;

            //Utilisateur Et nombre de fiches
            if (filterContext.FilterType != TypeFilter.DBL && !param.bFromChartReport)
            {
                HtmlGenericControl _lstItem = new HtmlGenericControl("option");
                _lstItem.Attributes.Add("value", "3");
                //                _lstItem.InnerText = String.Concat("<", eResApp.GetRes(filterInfos.Pref, 411), ">");
                _lstItem.InnerText = eResApp.GetRes(filterContext.Pref, 411);
                if (3 == filterLine.DescId)
                    _lstItem.Attributes.Add("selected", "selected");
                lstFields.Controls.Add(_lstItem);

                if (filterContext.FilterType != TypeFilter.RULES && filterContext.FilterTab != filterLine.Tab.Table.DescId && filterLine.LineIndex == 0)
                {
                    HtmlGenericControl _lstItemCnt = new HtmlGenericControl("option");
                    _lstItemCnt.Attributes.Add("value", "4");
                    _lstItemCnt.InnerText = String.Concat("<", eResApp.GetRes(filterContext.Pref, 437), ">");
                    if (4 == filterLine.DescId)
                        _lstItemCnt.Attributes.Add("selected", "selected");
                    lstFields.Controls.Add(_lstItemCnt);
                }
            }

            if (emptyLineRender)
            {
                lstFields.Attributes.Add("disabled", "disabled");
            }

            return lstDiv;
        }

        /// <summary>
        /// Retourne la liste des opérateurs selon le champs
        /// </summary>
        internal static HtmlGenericControl GetOperatorsList(AdvFilterContext filterContext, AdvFilterLineIndex lineIndex, bool emptyLineRender = false, Boolean bFromChartReport = false, eFilterRenderer.eFilterRendererParams param = null)
        {
            if (param == null)
                param = new eFilterRenderer.eFilterRendererParams();

            AdvFilterLine filterLine = lineIndex.GetLine(filterContext.Filter);

            FieldFilterInfo fieldInfo = null;
            FieldFormat fieldFormat = FieldFormat.TYP_CHAR;
            int fieldShortDescId = 0;

            if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.CURRENT_USER.GetHashCode())
            {
                fieldFormat = FieldFormat.TYP_USER;
            }
            else if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.NB_FILES.GetHashCode())
            {
                fieldFormat = FieldFormat.TYP_NUMERIC;
            }
            else
            {
                fieldInfo = filterContext.CacheFieldInfo.Get(filterLine.DescId);

                fieldShortDescId = fieldInfo.Descid - fieldInfo.Table.DescId;
                fieldFormat = fieldInfo.Format;
            }

            #region défini les différents id d'operateur autorisé en fonction du field

            ISet<Operator> ops = new HashSet<Operator>();
            if (filterLine.DescId == eLibConst.FILTER_SPEC_FIELDS.NB_FILES.GetHashCode()) //Nombre de fiches
            {
                ops.UnionWith(new Operator[] { Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL });
            }
            else
            {
                switch (fieldFormat)
                {
                    case FieldFormat.TYP_HIDDEN:
                        //SECTEURS / CLES PP-PM
                        if (filterLine.DescId == 292 || filterLine.DescId == 392 || filterLine.DescId == 290)
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL,
                                Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                                Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        break;
                    case FieldFormat.TYP_CHAR:
                        PopupType popupTyp = fieldInfo?.Popup ?? PopupType.NONE;

                        if (fieldInfo.IsCatEnum
                            || filterLine.DescId == (int)RGPDTreatmentsLogsField.TabsID
                            )
                        {
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_DIFFERENT,
                                Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN,
                                Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        }
                        else if (filterLine.DescId == (int)WorkflowScenarioField.WFTTARGETDESCID
                             || filterLine.DescId == (int)WorkflowScenarioField.WFTEVENTDESCID)
                        {
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, 
                                Operator.OP_DIFFERENT,
                                Operator.OP_IN_LIST, 
                                Operator.OP_NOT_IN_LIST,                                
                                Operator.OP_IS_EMPTY, 
                                Operator.OP_IS_NOT_EMPTY });
                        }
                        else if (filterLine.DescId == (int)RGPDTreatmentsLogsField.FieldsID)
                        {
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_DIFFERENT,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN,
                                Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        }
                        else if (fieldInfo?.Multiple ?? false)
                        {
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL,
                                Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                            if (popupTyp != PopupType.DATA)
                                ops.Add(Operator.OP_SOUNDEX);
                        }
                        else if (popupTyp == PopupType.SPECIAL)
                        {
                            //Catalogue spécial
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                                Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY,
                                Operator.OP_START_WITH, Operator.OP_NOT_START_WITH, Operator.OP_END_WITH, Operator.OP_NOT_END_WITH, Operator.OP_SOUNDEX });
                        }
                        else if (popupTyp == PopupType.FREE || popupTyp == PopupType.ONLY || popupTyp == PopupType.DATA)
                        {
                            // Catalogues
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                                Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY,
                                Operator.OP_START_WITH, Operator.OP_NOT_START_WITH, Operator.OP_END_WITH, Operator.OP_NOT_END_WITH });
                            if (popupTyp != PopupType.DATA)
                                ops.Add(Operator.OP_SOUNDEX);
                        }
                        else
                        {
                            // Texte
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                                Operator.OP_DIFFERENT,
                                Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY,
                                Operator.OP_START_WITH, Operator.OP_NOT_START_WITH, Operator.OP_END_WITH, Operator.OP_NOT_END_WITH, Operator.OP_SOUNDEX });
                        }

                        break;
                    case FieldFormat.TYP_EMAIL:
                        ops.UnionWith(new Operator[] {
                            Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                            Operator.OP_DIFFERENT,
                            Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY,
                            Operator.OP_START_WITH, Operator.OP_NOT_START_WITH, Operator.OP_END_WITH, Operator.OP_NOT_END_WITH, Operator.OP_SOUNDEX,
                            Operator.OP_MAIL_STATUS_EQUAL, Operator.OP_MAIL_STATUS_IN_LIST});
                        break;
                    case FieldFormat.TYP_WEB:
                    case FieldFormat.TYP_SOCIALNETWORK:
                    case FieldFormat.TYP_PHONE:
                        ops.UnionWith(new Operator[] {
                            Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                            Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                            Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY,
                            Operator.OP_START_WITH, Operator.OP_NOT_START_WITH, Operator.OP_END_WITH, Operator.OP_NOT_END_WITH, Operator.OP_SOUNDEX });
                        break;
                    case FieldFormat.TYP_DATE:
                        ops.UnionWith(new Operator[] {
                            Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                            Operator.OP_DIFFERENT, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        break;
                    case FieldFormat.TYP_BIT:
                    case FieldFormat.TYP_BITBUTTON:
                        ops.UnionWith(new Operator[] { Operator.OP_IS_TRUE, Operator.OP_IS_FALSE });
                        break;
                    case FieldFormat.TYP_IMAGE:
                    case FieldFormat.TYP_FILE:
                        ops.UnionWith(new Operator[] { Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        break;
                    case FieldFormat.TYP_AUTOINC:
                    case FieldFormat.TYP_MONEY:
                    case FieldFormat.TYP_NUMERIC:
                    case FieldFormat.TYP_PJ:
                        bool specOp = false;
                        switch (fieldInfo?.Table.EdnType ?? EdnType.FILE_MAIN)
                        {
                            case EdnType.FILE_MAIL:
                            case EdnType.FILE_SMS:
                                specOp = fieldShortDescId == (int)MailField.DESCID_MAIL_SENDTYPE
                                    || fieldShortDescId == (int)MailField.DESCID_MAIL_STATUS;
                                break;
                            case EdnType.FILE_PLANNING:
                                specOp = fieldShortDescId == (int)PlanningField.DESCID_CALENDAR_ITEM;
                                break;
                        }

                        switch (fieldInfo?.Table.TabType ?? TableType.EVENT)
                        {
                            case TableType.HISTO:
                                specOp = fieldShortDescId == (int)HistoField.TYPE || fieldShortDescId == (int)HistoField.DESCID;
                                break;
                            case TableType.PAYMENTTRANSACTION:
                                specOp = fieldShortDescId == (int)PaymentTransactionField.PAYTRANTARGETDESCID;
                                break;
                            case TableType.RGPDTREATMENTSLOGS:
                                specOp = filterLine.DescId == (int)RGPDTreatmentsLogsField.Type || filterLine.DescId == (int)RGPDTreatmentsLogsField.Status;
                                break;
                            case TableType.CAMPAIGN:
                                specOp = fieldShortDescId == (int)CampaignField.SENDTYPE || fieldShortDescId == (int)CampaignField.STATUS;
                                break;
                        }

                        if (specOp)
                            ops.UnionWith(new Operator[] { Operator.OP_EQUAL, Operator.OP_DIFFERENT });
                        else
                            ops.UnionWith(new Operator[] {
                                Operator.OP_EQUAL, Operator.OP_LESS, Operator.OP_LESS_OR_EQUAL, Operator.OP_GREATER, Operator.OP_GREATER_OR_EQUAL,
                                Operator.OP_DIFFERENT, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });

                        break;
                    case FieldFormat.TYP_USER:
                    case FieldFormat.TYP_GROUP:
                        ops.UnionWith(new Operator[] {
                            Operator.OP_EQUAL,
                            Operator.OP_DIFFERENT, Operator.OP_IN_LIST, Operator.OP_NOT_IN_LIST,
                            Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        break;
                    case FieldFormat.TYP_MEMO:
                        ops.UnionWith(new Operator[] { Operator.OP_CONTAIN, Operator.OP_NOT_CONTAIN, Operator.OP_IS_EMPTY, Operator.OP_IS_NOT_EMPTY });
                        break;
                }
            }

            #endregion

            IEnumerable<KeyValuePair<Operator, string>> opInfos = eResApp.GetSortedOperator(filterContext.Pref.LangId, ops);

            HtmlGenericControl lstOps = new HtmlGenericControl("select");
            lstOps.ID = String.Concat(string.IsNullOrEmpty(param.PrefixFilter) ? "" : param.PrefixFilter, "op_", filterLine.TabLineIndex);
            lstOps.Attributes.Add("name", String.Concat(string.IsNullOrEmpty(param.PrefixFilter) ? "" : param.PrefixFilter, "op_", filterLine.TabLineIndex));
            lstOps.Attributes.Add("onchange", string.Concat("onChangeLineOp(this", string.IsNullOrEmpty(param.PrefixFilter) ? "" : ",'" + param.PrefixFilter + "'", ")"));
            if (param.bFromChartReport)
                lstOps.Attributes.Add("eF", "1");

            foreach (KeyValuePair<Operator, string> keyValue in opInfos)
            {
                // On recupe le premier opérateur de la liste
                if (filterLine.Operator == Operator.OP_0_EMPTY)
                    filterLine.Operator = keyValue.Key;

                HtmlGenericControl cOp = new HtmlGenericControl("option");
                cOp.Attributes.Add("value", ((int)keyValue.Key).ToString());
                cOp.InnerText = keyValue.Value;
                if (keyValue.Key == filterLine.Operator)
                    cOp.Attributes.Add("selected", "selected");
                lstOps.Controls.Add(cOp);
            }

            HtmlGenericControl container = new HtmlGenericControl("div");
            container.ID = String.Concat("divop_", filterLine.TabLineIndex);
            container.Attributes.Add("class", param.bFromChartReport ? "fChartOptions" : "options");
            if (emptyLineRender)
            {
                lstOps.Attributes.Add("disabled", "disabled"); ;
            }
            container.Controls.Add(lstOps);
            return container;
        }

        #region Méthodes de gestion des rubriques numériques spécifiques (mail status, type calendrier, historique action, etc.)

        /// <summary>
        /// Retourne la liste des champs historique
        /// </summary>
        private static Control GetHistoFieldsList(AdvFilterContext filterContext, AdvFilterLine filterLine)
        {
            string error = String.Empty;
            string val = filterLine.Value;

            HtmlGenericControl selHisto = new HtmlGenericControl("select");
            selHisto.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(filterContext.Pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selHisto.Controls.Add(selOpt);

            //Rubriques historisées uniquement
            string sql = new StringBuilder()
                .Append("SELECT fld.[DescId], restab.[LANG_00] + '.' + resfld.[LANG_00] AS [FullText] ")
                .Append("FROM [desc] fld")
                .Append("	INNER JOIN [res] restab on fld.[DescId] - fld.[DescId] % 100 = restab.[ResId]")
                .Append("	LEFT JOIN [res] resfld ON fld.[DescId] = resfld.[ResId] ")
                .Append("WHERE ISNULL(fld.[historic], 0) <> 0")
                .Append("ORDER BY [FullText]")
                .ToString();

            DataTableReaderTuned dtr = null;
            try
            {
                dtr = filterContext.Dal.Execute(new RqParam(sql), out error);
                if (error.Length > 0)
                    throw new Exception("eFilter.GetHistoFieldsList" + error);

                while (dtr.Read())
                {
                    selOpt = new HtmlGenericControl("option");
                    selOpt.InnerText = dtr.GetString("FullText");
                    selOpt.Attributes.Add("value", dtr.GetEudoNumeric("DescId").ToString());
                    if (dtr.GetEudoNumeric("DescId") == eLibTools.GetNum(val))
                        selOpt.Attributes.Add("selected", "selected");
                    selHisto.Controls.Add(selOpt);
                }
            }
            finally
            {
                dtr?.Dispose();
            }

            return selHisto;
        }

        private static Control GetHistoActionList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selHisto = new HtmlGenericControl("select");
            selHisto.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");
            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selHisto.Controls.Add(selOpt);

            eEnumTools<HistoType> eta = new eEnumTools<HistoType>();
            foreach (HistoType item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                switch (item)
                {
                    case HistoType.MOD_UPDATE: selOpt.InnerText = eResApp.GetRes(pref, 805); break;
                    case HistoType.MOD_CREATION: selOpt.InnerText = eResApp.GetRes(pref, 738); break;
                    case HistoType.MOD_DELETE: selOpt.InnerText = eResApp.GetRes(pref, 806); break;
                    case HistoType.MOD_HISTO_REPORT: selOpt.InnerText = eResApp.GetRes(pref, 1153); break;
                }
                if (val == ((int)item).ToString())
                    selOpt.Attributes.Add("selected", "selected");
                selOpt.Attributes.Add("value", ((int)item).ToString());
                selHisto.Controls.Add(selOpt);
            }

            return selHisto;
        }

        /// <summary>
        /// Liste HTML des statuts de mails
        /// </summary>
        private static Control GetCampaignStatusList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<CampaignStatus> eta = new eEnumTools<CampaignStatus>();
            foreach (CampaignStatus item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                switch (item)
                {
                    case CampaignStatus.MAIL_IN_PREPARATION: selOpt.InnerText = eResApp.GetRes(pref, 6461); break;
                    case CampaignStatus.MAIL_SENDING: selOpt.InnerText = eResApp.GetRes(pref, 6546); break;
                    case CampaignStatus.MAIL_SENT: selOpt.InnerText = eResApp.GetRes(pref, 685); break;
                    case CampaignStatus.MAIL_DELAYED: selOpt.InnerText = eResApp.GetRes(pref, 820); break;
                    case CampaignStatus.MAIL_CANCELED: selOpt.InnerText = eResApp.GetRes(pref, 6460); break;
                    case CampaignStatus.MAIL_ERROR: selOpt.InnerText = eResApp.GetRes(pref, 416); break;
                    case CampaignStatus.MAIL_RECURRENT: selOpt.InnerText = eResApp.GetRes(pref, 2037); break;
                    case CampaignStatus.MAIL_WRKFLW_IN_PREPARATION: selOpt.InnerText = eResApp.GetRes(pref, 3112); break;
                    case CampaignStatus.MAIL_WRKFLW_READY: selOpt.InnerText = eResApp.GetRes(pref, 3113); break;
                }
                if (val == ((int)item).ToString())
                    selOpt.Attributes.Add("selected", "selected");
                selOpt.Attributes.Add("value", ((int)item).ToString());
                selStatus.Controls.Add(selOpt);
            }

            return selStatus;
        }

        /// <summary>
        /// Liste HTML des statuts de campagnes
        /// </summary>
        private static Control GetMailStatusList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<EmailStatus> eta = new eEnumTools<EmailStatus>();
            foreach (EmailStatus item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                int? idLabel = Outils.EnumToResId.GetMailStatusResID(item);
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Value);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        /// <summary>
        /// Liste HTML des statuts de paiement eudo
        /// </summary>
        private static Control GetPaymentStatusList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<PayTranEudoStatusEnum> eta = new eEnumTools<PayTranEudoStatusEnum>();
            foreach (PayTranEudoStatusEnum item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                var idLabel = Outils.EnumToResId.GetPaymentStatus(item);
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Item1);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        /// <summary>
        /// Liste HTML des statuts de la transaction
        /// </summary>
        private static Control GetTransactionStatusList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<StatusHostedCheckout> eta = new eEnumTools<StatusHostedCheckout>();
            foreach (StatusHostedCheckout item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                var idLabel = Outils.EnumToResId.GetTransactionStatus(item);
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Item1);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        /// <summary>
        /// Liste HTML des statuts prestataire de paiement
        /// </summary>
        private static Control GetIngenicoStatusList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<PaymentSubStatus> eta = new eEnumTools<PaymentSubStatus>();
            foreach (PaymentSubStatus item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                var idLabel = Outils.EnumToResId.GetIngenicoStatus(item);
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Item1);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        // <summary>
        /// Liste HTML des catégories de statuts
        /// </summary>
        private static Control GetStatusCategoryList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<PaymentStatusCategory> eta = new eEnumTools<PaymentStatusCategory>();
            foreach (PaymentStatusCategory item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                var idLabel = Outils.EnumToResId.GetStatusCategory(item);
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Item1);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        /// <summary>
        /// Liste HTML de valeurs d'Enum
        /// </summary>
        private static Control GetEnumList<T>(ePrefLite pref, AdvFilterLine filterLine, Func<T, int?> resIdGetter) where T : struct, IConvertible
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");

            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                selOpt = new HtmlGenericControl("option");
                int? idLabel = resIdGetter((T)item); ;
                if (idLabel != null)
                {
                    selOpt.InnerText = eResApp.GetRes(pref, idLabel.Value);

                    if (val == ((int)item).ToString())
                        selOpt.Attributes.Add("selected", "selected");
                    selOpt.Attributes.Add("value", ((int)item).ToString());
                    selStatus.Controls.Add(selOpt);
                }
            }

            return selStatus;
        }

        /// <summary>Récupère la liste des types d'envoi de campagne : Teradata/Eudonet/Eudonet SMS</summary>
        /// <param name="filterLine">Ligne de filtre</param>
        /// <param name="pref">The preference.</param>
        /// <returns></returns>
        private static Control GetCampaignSendTypeList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selSend = new HtmlGenericControl("select");
            selSend.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");
            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selSend.Controls.Add(selOpt);

            eEnumTools<MAILINGSENDTYPE> eta = new eEnumTools<MAILINGSENDTYPE>();
            foreach (MAILINGSENDTYPE item in eta.GetList)
            {
                if (item == MAILINGSENDTYPE.UNDEFINED)
                    continue;

                selOpt = new HtmlGenericControl("option");
                switch (item)
                {
                    case MAILINGSENDTYPE.ECIRCLE: selOpt.InnerText = "Teradata Email"; break;
                    case MAILINGSENDTYPE.EUDONET: selOpt.InnerText = "Eudonet Email"; break;
                    case MAILINGSENDTYPE.EUDONET_SMS: selOpt.InnerText = "Eudonet SMS"; break;
                }
                if (val == ((int)item).ToString())
                    selOpt.Attributes.Add("selected", "selected");
                selOpt.Attributes.Add("value", ((int)item).ToString());
                selSend.Controls.Add(selOpt);
            }

            return selSend;
        }

        /// <summary>Récupère la liste des types d'envoi de mail : Mail/SMS</summary>
        /// <param name="filterLine">Ligne de filtre</param>
        /// <param name="pref">The preference.</param>
        /// <returns></returns>
        private static Control GetMailSendTypeList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selSend = new HtmlGenericControl("select");
            selSend.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");
            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selSend.Controls.Add(selOpt);

            selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 656);
            if (val == EdnType.FILE_MAIL.GetHashCode().ToString())
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", EdnType.FILE_MAIL.GetHashCode().ToString());
            selSend.Controls.Add(selOpt);

            selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 655);
            if (val == EdnType.FILE_SMS.GetHashCode().ToString())
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", EdnType.FILE_SMS.GetHashCode().ToString());
            selSend.Controls.Add(selOpt);

            return selSend;
        }

        /// <summary>
        /// Liste des types agendas
        /// </summary>
        private static Control GetCalendarItemList(ePrefLite pref, AdvFilterLine filterLine)
        {
            string val = filterLine.Value;

            HtmlGenericControl selStatus = new HtmlGenericControl("select");
            selStatus.Attributes.Add("onchange", "document.getElementById('value_" + filterLine.TabLineIndex + "').setAttribute(\"ednvalue\",this.options[this.selectedIndex].value);");
            HtmlGenericControl selOpt = new HtmlGenericControl("option");
            selOpt.InnerText = eResApp.GetRes(pref, 141);
            if (String.IsNullOrEmpty(val))
                selOpt.Attributes.Add("selected", "selected");
            selOpt.Attributes.Add("value", String.Empty);
            selStatus.Controls.Add(selOpt);

            eEnumTools<PlanningType> eta = new eEnumTools<PlanningType>();
            foreach (PlanningType item in eta.GetList)
            {
                selOpt = new HtmlGenericControl("option");
                selOpt.InnerText = eResApp.GetCalendarItemRes(item, pref);
                if (val == ((int)item).ToString())
                    selOpt.Attributes.Add("selected", "selected");
                selOpt.Attributes.Add("value", ((int)item).ToString());
                selStatus.Controls.Add(selOpt);
            }

            return selStatus;
        }
        #endregion

    }

}