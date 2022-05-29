using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Représente un onglet dans l'assistant à la création d'un filtre
    /// </summary>
    internal class eFilterTabRenderer
    {
        /// <summary>
        /// Liste des tables liés
        /// </summary>
        internal static HtmlGenericControl GetLinkedFileList(int nTab, ePref pref, string controlId, out int firstFile)
        {
            string error = String.Empty;

            HtmlGenericControl lst = new HtmlGenericControl("select");
            lst.Attributes.Add("onchange", "onChangeLinkFileTab(this)");
            lst.Attributes.Add("SelectedIndex", "0");
            lst.ID = controlId;
            lst.Attributes.Add("name", controlId);

            firstFile = 0;

            bool isLinkedToAddress = false;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();

                TableLite etab = new TableLite(nTab);
                etab.ExternalLoadInfo(dal, out error);

                string sql = string.Empty;
                switch (etab.TabType)
                {
                    case TableType.PP:
                    case TableType.PM:
                        sql = string.Concat("select res.", pref.Lang, " as Libelle, RelationFileDescId from cfc_getLinked(", nTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 order by  res.", pref.Lang);
                        break;
                    case TableType.EVENT:
                    case TableType.ADR:
                        // Table de départ EVENT
                        sql = string.Concat("select res.", pref.Lang, " as Libelle, RelationFileDescId from cfc_getLinked(", nTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 "
                                            , " UNION "
                                            , "select res.", pref.Lang, " as Libelle, RelationFileDescId from cfc_getLiaison(", nTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 order by  res.", pref.Lang);
                        break;
                    default:
                        // Table de départ : Template
                        // La liaison doit être sur la table de départ
                        sql = string.Concat("select res.", pref.Lang, " as Libelle, RelationFileDescId from cfc_getLiaison(", nTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1  order by  res.", pref.Lang);
                        break;
                }

                ISet<int> listTab = new HashSet<int>();
                DataTableReaderTuned dtr = null;
                try
                {
                    dtr = dal.Execute(new RqParam(sql), out error);
                    if (!String.IsNullOrEmpty(error))
                        throw new Exception(String.Concat("GetLinkedFileList : ", error));

                    if (dtr != null)
                    {
                        while (dtr.Read())
                        {
                            int tabDescId = dtr.GetEudoNumeric("RelationFileDescId");
                            if (tabDescId != nTab)
                            {
                                // #42620 CRU 
                                if (tabDescId == 400)
                                    isLinkedToAddress = true;

                                listTab.Add(tabDescId);
                            }
                        }
                    }
                }
                finally
                {
                    dtr?.Dispose();
                }

                // Ajout adr, pp et pm si nécessaire
                // #42620 CRU : Si la table est liée à Adresses, on affiche également PP et PM
                if (nTab == 200 || nTab == 300 || nTab == 400 || isLinkedToAddress)
                {
                    if (nTab != (int)TableType.PP)
                        listTab.Add((int)TableType.PP);
                    if (nTab != (int)TableType.PM)
                        listTab.Add((int)TableType.PM);
                    if (nTab != (int)TableType.ADR)
                        listTab.Add((int)TableType.ADR);
                }

                //Droits de visu
                using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(pref.User.UserId, pref.User.UserLevel, pref.User.UserGroupId, pref.User.UserLang, listTab, dal))
                {
                    if (dtrRights == null)
                        throw new Exception(String.Concat("FillMainFileList : ", error));

                    int idx = 0;
                    int descId;
                    while (dtrRights.Read())
                    {
                        if (eLibDataTools.GetTabViewRight(dtrRights))
                        {
                            descId = dtrRights.GetEudoNumeric("DescId");

                            if (idx == 0)
                                firstFile = descId;

                            HtmlGenericControl itm = new HtmlGenericControl("option");
                            itm.InnerText = dtrRights.GetString("Libelle");
                            itm.Attributes.Add("value", descId.ToString());
                            lst.Controls.Add(itm);
                            idx++;
                        }
                    }
                }
            }
            finally
            {
                dal.CloseDatabase();
            }

            return lst;
        }

        /// <summary>
        /// Retourne la liste des tables
        /// </summary>
        private static HtmlGenericControl GetFileList(AdvFilterContext filterContext, AdvFilterTab tab)
        {
            string error = String.Empty;

            bool bAdminMode = filterContext.Pref.AdminMode;
            if (bAdminMode && filterContext.Pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                bAdminMode = false;

            HtmlGenericControl lst = new HtmlGenericControl("select");
            lst.Attributes.Add("onchange", "onChangeFileTab(this)");
            lst.Attributes.Add("SelectedIndex", "0");
            lst.ID = string.Concat("file_", tab.TabIndex);
            lst.Attributes.Add("name", string.Concat("file_", tab.TabIndex));

            TableFilterInfo etab = filterContext.CacheTableInfo.Get(filterContext.FilterTab);

            string sql = string.Empty;
            switch (etab.TabType)
            {
                case TableType.PP:
                case TableType.PM:
                    sql = string.Concat("select res.", filterContext.Pref.Lang, " as Libelle, RelationFileDescId from cfc_getLinked(", filterContext.FilterTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 order by res.", filterContext.Pref.Lang);
                    break;
                case TableType.EVENT:
                case TableType.ADR:
                    // Table de départ EVENT
                    sql = string.Concat("select res.", filterContext.Pref.Lang, " as Libelle, RelationFileDescId from cfc_getLinked(", filterContext.FilterTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 "
                                        , " UNION "
                                        , "select res.", filterContext.Pref.Lang, " as Libelle, RelationFileDescId from cfc_getLiaison(", filterContext.FilterTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1 order by res.", filterContext.Pref.Lang);
                    break;
                default:
                    // Table de départ : Template
                    // La liaison doit être sur la table de départ
                    sql = string.Concat("select res.", filterContext.Pref.Lang, " as Libelle, RelationFileDescId from cfc_getLiaison(", filterContext.FilterTab, ") left join res on RelationFileDescId = res.resid where isnull(isrelation,0) = 1  order by res.", filterContext.Pref.Lang);
                    break;
            }

            ISet<int> listTab = new HashSet<int>();
            DataTableReaderTuned dtr = null;
            try
            {
                RqParam rq = new RqParam(sql);
                dtr = filterContext.Dal.Execute(rq, out error);
                if (!string.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("FillMainFileList : ", error));

                if (dtr != null)
                {
                    while (dtr.Read())
                    {
                        int tabDescId = dtr.GetEudoNumeric("RelationFileDescId");
                        if (tabDescId != 0)
                            listTab.Add(tabDescId);
                    }
                }
            }
            finally
            {
                dtr?.Dispose();
            }

            // Ajout adr, pp et pm si nécessaire
            if (filterContext.FilterTab == 200 || filterContext.FilterTab == 300 || filterContext.FilterTab == 400)
            {
                listTab.Add((int)TableType.PP);
                listTab.Add((int)TableType.PM);
                listTab.Add((int)TableType.ADR);
            }

            // Ajout de la table départ
            listTab.Add(filterContext.FilterTab);

            // On determine quel table est selectionnée
            int nTab;
            if (!tab.Table.isEmpty)
                nTab = tab.Table.DescId;
            else
                nTab = filterContext.FilterTab;

            //Droits de visu
            int idx = 0;
            int ednIndex = 0;

            using (DataTableReaderTuned dtrRights = eLibDataTools.GetRqViewRight(filterContext.Pref.User.UserId, filterContext.Pref.User.UserLevel, filterContext.Pref.User.UserGroupId, filterContext.Pref.User.UserLang, listTab, filterContext.Dal))
            {

                if (dtrRights == null || !String.IsNullOrEmpty(error))
                    throw new Exception(String.Concat("FillMainFileList : ", error));

                while (dtrRights.Read())
                {
                    if (!bAdminMode && !eLibDataTools.GetTabViewRight(dtrRights))
                        continue;

                    int descId = dtrRights.GetEudoNumeric("DescId");
                    if (descId != 0)
                    {
                        HtmlGenericControl _itm = new HtmlGenericControl("option");
                        _itm.InnerText = dtrRights.GetString("Libelle");
                        _itm.Attributes.Add("value", descId.ToString());
                        if (descId == nTab)
                        {
                            _itm.Attributes.Add("Selected", "Selected");
                            ednIndex = idx;
                        }
                        lst.Controls.Add(_itm);
                        idx++;
                    }
                }
            }


            // Ajout de la table principale en cas de liaison
            if (!tab.Table.isEmpty && tab.Table.BoundTabDescId != 0)
            {
                eRes _res = new eRes(filterContext.Pref, String.Concat(tab.Table.DescId, ",", tab.Table.BoundTabDescId));

                HtmlGenericControl _itmLink = new HtmlGenericControl("option");
                bool bFound = false;
                _itmLink.InnerText = String.Concat("<", _res.GetRes(tab.Table.DescId, out bFound), " ", eResApp.GetRes(filterContext.Pref, 535), " ", _res.GetRes(tab.Table.BoundTabDescId, out bFound), ">");
                _itmLink.Attributes.Add("value", tab.Table.GetValue());
                _itmLink.Attributes.Add("Selected", "Selected");
                ednIndex = idx;
                lst.Controls.Add(_itmLink);
                idx++;
            }

            HtmlGenericControl _itmSpec = new HtmlGenericControl("option");
            //_itmSpec.InnerText = String.Concat("<", eResApp.GetRes(pref.Lang, 982), ">");
            _itmSpec.InnerText = eResApp.GetRes(filterContext.Pref, 982);
            _itmSpec.Attributes.Add("value", "0");
            lst.Attributes.Add("ednindex", ednIndex.ToString());
            lst.Controls.Add(_itmSpec);
            idx++;

            return lst;
        }

        /// <summary>
        /// Retourne le rendu HTML d'un tab
        /// </summary>
        internal static HtmlGenericControl GetTabRenderer(AdvFilterContext filterContext, AdvFilterTabIndex tabIndex, bool onlyFilesOpt)
        {
            AdvFilterTab tab = tabIndex.GetTab(filterContext.Filter);

            HtmlGenericControl tabDiv = new HtmlGenericControl("div");
            tabDiv.Attributes.Add("class", "table_filtres");
            tabDiv.ID = String.Concat("table_filtres_", tab.TabIndex);

            HtmlGenericControl headDiv = new HtmlGenericControl("div");
            tabDiv.Controls.Add(headDiv);
            headDiv.Attributes.Add("class", "head-table");
            //bouton développer/fermer
            /*
            HtmlGenericControl openCloseDiv = new HtmlGenericControl("div");
            headDiv.Controls.Add(openCloseDiv);
            openCloseDiv.Attributes.Add("class", "CloseDiv");
            openCloseDiv.Attributes.Add("onclick", "openCloseTab(" + tabIndex + ", this);");
            */

            HtmlGenericControl tabListDiv = new HtmlGenericControl("div");
            headDiv.Controls.Add(tabListDiv);
            tabListDiv.Attributes.Add("class", "head-contact");
            //combo Liste des tables
            tabListDiv.Controls.Add(GetFileList(filterContext, tab));
            // Icône
            HtmlGenericControl iconSelect = new HtmlGenericControl("span");
            iconSelect.Attributes.Add("class", "icon-bt-actions-right");
            tabListDiv.Controls.Add(iconSelect);

            HtmlGenericControl deleteDiv = new HtmlGenericControl("div");
            headDiv.Controls.Add(deleteDiv);
            deleteDiv.Attributes.Add("class", "head-end icon-delete");
            deleteDiv.Attributes.Add("onclick", String.Concat("DelFilterTab(", tab.TabIndex, ", true)"));

            HtmlGenericControl linesDiv = new HtmlGenericControl("div");
            tabDiv.Controls.Add(linesDiv);
            linesDiv.Attributes.Add("class", "choix_filtres");
            linesDiv.ID = String.Concat("choix_filtres_", tab.TabIndex);

            for (int i = 0; i < tab.Lines.Count; i++)
            {
                linesDiv.Controls.Add(eFilterLineRenderer.GetLineRenderer(filterContext, new AdvFilterLineIndex(tabIndex.ArrayTabIndex, i)));
            }


            //Opérateur de fin
            if (linesDiv.Controls.Count > 0)
            {
                HtmlGenericControl op = new HtmlGenericControl("div");
                linesDiv.Controls.Add(op);
                op.Attributes.Add("class", "EndLineOperator");
                op.ID = "EndLineOperatorContainer_" + tab.TabIndex;
                string id = String.Concat("end_operator_line_", tab.TabIndex);
                op.Controls.Add(eFilterRenderer.GetLogicalOperatorList(filterContext.Pref, id, InterOperator.OP_NONE, "onChangeFilterLineOp(this)", false));
            }

            linesDiv.Controls.Add(GetGroupByBlock(filterContext, tabIndex, onlyFilesOpt));

            return tabDiv;
        }

        /// <summary>
        /// Retourne un contrôle correspond aux options de regroupement
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="tab">Objet eFilterTab représentant l'onglet du filtre</param>
        /// <param name="onlyFilesOpt">Indique si l'option "Ne retenir que les premières fiches" est cochée (et donc, si le bloc contenant l'option devrait être affiché ou non)</param>
        /// <returns></returns>
        internal static HtmlGenericControl GetGroupByBlock(AdvFilterContext filterContext, AdvFilterTabIndex tabIndex, bool onlyFilesOpt)
        {
            AdvFilterTab tab = tabIndex.GetTab(filterContext.Filter);

            // Ajout de l'option de Regroupement
            HtmlGenericControl divOptTab = new HtmlGenericControl("div");
            divOptTab.Attributes.Add("class", "OptBlock");
            if (!onlyFilesOpt)
                divOptTab.Style.Add(HtmlTextWriterStyle.Display, "none");
            divOptTab.ID = string.Concat("OptBlock_", tab.TabIndex);

            divOptTab.Controls.Add(eTools.GetCheckBoxOption(eResApp.GetRes(filterContext.Pref, 1251), String.Concat("importance_", tab.TabIndex), tab.Importance, false, string.Empty, "onCheckOption"));
            HtmlGenericControl divGrp = new HtmlGenericControl("div");
            divGrp.ID = string.Concat("divGrp_", tab.TabIndex);
            divGrp.Attributes.Add("name", String.Concat("divGrp_", tab.TabIndex));
            divGrp.Attributes.Add("class", "divGrp");
            string _grpDisp = tab.Importance ? "block" : "none";
            divGrp.Style.Add(HtmlTextWriterStyle.Display, _grpDisp);
            divOptTab.Controls.Add(divGrp);

            HtmlGenericControl spanOpt = new HtmlGenericControl("span");
            spanOpt.InnerText = eResApp.GetRes(filterContext.Pref, 1250);
            divGrp.Controls.Add(spanOpt);

            HtmlGenericControl importanceList = new HtmlGenericControl("select");
            importanceList.ID = "top_" + tab.TabIndex;
            //importanceList.Attributes.Add("onchange", "OnSelectTop(this);");

            for (int i = 1; i <= AdvFilter.MAX_RESTRICTION; i++)
            {
                string strRes = String.Empty;
                if (i == 1)
                    strRes = eResApp.GetRes(filterContext.Pref, 1247);
                else
                    strRes = eResApp.GetRes(filterContext.Pref, 1248).Replace("<COUNT>", i.ToString());

                HtmlGenericControl lstItem = new HtmlGenericControl("option");
                lstItem.Attributes.Add("value", i.ToString());
                lstItem.InnerText = strRes;
                if (i == tab.LstTop)
                    lstItem.Attributes.Add("selected", "selected");
                importanceList.Controls.Add(lstItem);
            }

            divGrp.Controls.Add(importanceList);
            HtmlGenericControl spanOptRecords = new HtmlGenericControl("span");
            spanOptRecords.InnerText = String.Concat(eResApp.GetRes(filterContext.Pref, 300), " ");
            divGrp.Controls.Add(spanOptRecords);

            HtmlGenericControl spanGrp = new HtmlGenericControl("span");
            spanGrp.InnerText = eResApp.GetRes(filterContext.Pref, 1249);
            divGrp.Controls.Add(spanGrp);

            divGrp.Controls.Add(GetGroupByListfields(filterContext, tab));

            return divOptTab;

        }

        private static Control GetGroupByListfields(AdvFilterContext filterContext, AdvFilterTab tab)
        {
            string error = string.Empty;

            HtmlGenericControl lstDiv = new HtmlGenericControl("div");
            lstDiv.Attributes.Add("class", "options");

            // Pour l'operation de regroupement, on recup les rubrique de la table de liaison
            int nDescId = tab.Table.DescId;
            if (tab.Table.BoundTabDescId != 0)
                nDescId = tab.Table.BoundTabDescId;

            HtmlGenericControl lstFields = new HtmlGenericControl("select");
            lstDiv.Controls.Add(lstFields);
            lstFields.ID = String.Concat("groupby_", tab.TabIndex);
            lstFields.Attributes.Add("name", String.Concat("groupby_", tab.TabIndex));

            //pas de regroupement

            HtmlGenericControl lstItem = new HtmlGenericControl("option");
            lstItem.Attributes.Add("value", "0");
            lstItem.InnerText = String.Concat("<", eResApp.GetRes(filterContext.Pref, 157), ">");

            if (tab.LstGroupBy == 0)
                lstItem.Attributes.Add("selected", "selected");

            lstFields.Controls.Add(lstItem);

            bool bOrderByDescId = false;

            IEnumerable<int> allFields = RetrieveFields.GetDefault(filterContext.Pref)
                .SetExternalDal(filterContext.Dal)
                .AddOnlyThisTabs(new int[] { nDescId })
                .ResultFieldsDid();

            //Concatène les descId de la liste
            string sListCol = eLibTools.Join(";", allFields);

            //Uniquement les rubriques sur lesquelles on a les permissions
            Dictionary<int, string> fldList = eLibTools.GetAllowedFieldsFromDescIds(filterContext.Dal, filterContext.Pref.User, sListCol, bOrderByDescId);

            string optCss = "cell";
            foreach (var fld in fldList)
            {
                if (optCss.Equals("cell"))
                    optCss = "cell2";
                else
                    optCss = "cell";
                lstItem = new HtmlGenericControl("option");
                lstItem.Attributes.Add("value", fld.Key.ToString());
                lstItem.InnerHtml = fld.Value;
                lstItem.Attributes.Add("class", optCss);

                if (fld.Key == tab.LstGroupBy)
                    lstItem.Attributes.Add("selected", "selected");

                lstFields.Controls.Add(lstItem);
            }

            return lstDiv;
        }
    }
}