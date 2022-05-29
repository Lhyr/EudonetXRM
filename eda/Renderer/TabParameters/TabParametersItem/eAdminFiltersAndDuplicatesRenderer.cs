using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminFiltersAndDuplicatesRenderer : eAdminBlockRenderer
    {
        public eAdminFiltersAndDuplicatesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "FiltersSearchesDuplicatesPart")
        {

        }

        public static eAdminFiltersAndDuplicatesRenderer CreateAdminFiltersAndDuplicatesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminFiltersAndDuplicatesRenderer features = new eAdminFiltersAndDuplicatesRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        protected override bool Build()
        {
            base.Build();

            #region Filtres rapides
            if (_tabInfos.TabType != TableType.ADR)
            {
                BuildQuickFilterSelects();
            }
            #endregion

            #region filtre doublon/defaut
            if (_tabInfos.TabType != TableType.ADR)
            {
                //doublon seulement pour les fichiers principaux
                if (this._tabInfos.EdnType == EdnType.FILE_MAIN)
                {
                    eAdminButtonField filtBtn = new eAdminButtonField(eResApp.GetRes(Pref, 7053), "eAdmnBtnDup", "Cliquer pour paramétrer la fonctionnalité de déduplication des fiches de l’onglet ", String.Concat("nsAdminFile.openSpecFilter(", (int)TypeFilter.DBL, ")"));

                    filtBtn.Generate(_panelContent);

                }
                eAdminButtonField btnFilterDefault = new eAdminButtonField(eResApp.GetRes(Pref, 1102), "eAdmnBtnDefault", "Cliquer pour spécifier les fiches devant être masquées lors de l’ouverture de l’onglet", String.Concat("nsAdminFile.openSpecFilter(", (int)TypeFilter.DEFAULT, ")"));

                btnFilterDefault.Generate(_panelContent);
            }
            #endregion

            #region Historique
            if (_tabInfos.TabType != TableType.HISTO)
            {
                List<ListItem> histoListItems = new List<ListItem>();
                histoListItems.Add(new ListItem(eResApp.GetRes(Pref, 248), "0")); //Aucune rubrique
                Dictionary<int, string> diFields = _tabInfos.GetBooleanFields(Pref);
                ;
                foreach (KeyValuePair<int, string> kvp in diFields)
                {
                    histoListItems.Add(new ListItem(kvp.Value, kvp.Key.ToString()));
                }


                if (_tabInfos.DescId == (int)TableType.PP || _tabInfos.DescId == (int)TableType.PM)
                {
                    Dictionary<int, string> diAdrFields = eSqlDesc.LoadBooleanFields(Pref, (int)TableType.ADR, true);
                    foreach (KeyValuePair<int, string> kvp in diAdrFields)
                    {
                        histoListItems.Add(new ListItem(kvp.Value, kvp.Key.ToString()));
                    }


                }

                eAdminDropdownField histoDdl = new eAdminDropdownField(_tabInfos.DescId,
                    eResApp.GetResWithColon(Pref, 7247),
                    eAdminUpdateProperty.CATEGORY.PREF,
                    ADMIN_PREF.HISTODESCID.GetHashCode(),
                    histoListItems.ToArray(),
                    eResApp.GetRes(Pref, 7248),
                    Math.Abs(_tabInfos.HistoDescId).ToString(),
                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                    onChange: "nsAdminFile.updateHistoDescId(this);");
                histoDdl.NoUpdate = true;
                histoDdl.SortItemsByLabel = false;
                histoDdl.Generate(_panelContent);

                Dictionary<string, string> dicRB = new Dictionary<string, string>();
                dicRB.Add("", eResApp.GetRes(Pref, 2011));
                dicRB.Add("-", eResApp.GetRes(Pref, 2012));

                eAdminRadioButtonField radio = new eAdminRadioButtonField(_tabInfos.DescId,
                    "",
                    eAdminUpdateProperty.CATEGORY.PREF,
                    (int)ADMIN_PREF.HISTODESCID,
                    "radioHistoDescId",
                    dicRB,
                    value: _tabInfos.HistoDescId < 0 ? "-" : "",
                    onChange: "nsAdminFile.updateHistoDescId(this);");
                radio.NoUpdate = true;

                radio.Generate(_panelContent);
            }
            #endregion

            return true;
        }


        private void BuildQuickFilterSelects()
        {
            #region Chargement des options
            Dictionary<int, string> dicFieldsRes;
            Dictionary<int, FieldFormat> dicFieldsFormat;
            Dictionary<int, PopupType> dicFieldsPopup;
            _tabInfos.GetFields(Pref, out dicFieldsRes, out dicFieldsFormat, out dicFieldsPopup);
            Dictionary<int, string> dicParentRes = LoadParentRes();

            List<ListItem> quickFilterListItems0 = GetQuickFilterSelectOptions(dicFieldsRes, dicParentRes, dicFieldsFormat, dicFieldsPopup, 0);
            List<ListItem> quickFilterListItems1 = GetQuickFilterSelectOptions(dicFieldsRes, dicParentRes, dicFieldsFormat, dicFieldsPopup, 1);
            #endregion

            #region chargement des uservalue
            eUserValue quickFilter0;
            eUserValue quickFilter1;
            List<eUserValue> listUserValue = _tabInfos.GetQuickFilters(Pref, out quickFilter0, out quickFilter1);

            string value0 = "0";
            if (quickFilter0 != null)
            {
                value0 = quickFilter0.DescId.ToString();
                if (!String.IsNullOrEmpty(quickFilter0.Label))
                    value0 = String.Concat(value0, ";", quickFilter0.Label);
            }

            string value1 = "0";
            if (quickFilter1 != null)
            {
                value1 = quickFilter1.DescId.ToString();
                if (!String.IsNullOrEmpty(quickFilter1.Label))
                    value1 = String.Concat(value1, ";", quickFilter1.Label);
            }
            #endregion

            string quickfilterInfobulle = eResApp.GetRes(Pref, 7250);

            eAdminDropdownField quickFilterDdl0 = new eAdminDropdownField(_tabInfos.DescId, eResApp.GetResWithColon(Pref, 7249),
                eAdminUpdateProperty.CATEGORY.USERVALUE,
                TypeUserValueAdmin.FILTER_QUICK0.GetHashCode(), 
                quickFilterListItems0.ToArray(),
                tooltiptext: quickfilterInfobulle, value: value0, valueFormat: FieldFormat.TYP_CHAR, renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE,
                sortItemsByLabel: false);
            quickFilterDdl0.Generate(_panelContent);

            eAdminDropdownField quickFilterDdl1 = new eAdminDropdownField(_tabInfos.DescId, String.Empty,
                eAdminUpdateProperty.CATEGORY.USERVALUE, TypeUserValueAdmin.FILTER_QUICK1.GetHashCode(), quickFilterListItems1.ToArray(),
                tooltiptext: quickfilterInfobulle, value: value1, valueFormat: FieldFormat.TYP_CHAR, renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.SELECTABOVE,
                sortItemsByLabel: false);
            quickFilterDdl1.Generate(_panelContent);
        }

        private Dictionary<int, string> LoadParentRes()
        {
            List<int> listRes = new List<int>();

            if (_tabInfos.InterPP)
            {
                listRes.Add(TableType.PP.GetHashCode());
                listRes.Add(TableType.PP.GetHashCode() + 1);
            }
            if (_tabInfos.InterPM)
            {
                listRes.Add(TableType.PM.GetHashCode());
                listRes.Add(TableType.PM.GetHashCode() + 1);
            }
            if (_tabInfos.InterEVT && _tabInfos.TabType != TableType.EVENT)
            {
                listRes.Add(_tabInfos.InterEVTDescid);
                listRes.Add(_tabInfos.InterEVTDescid + 1);
            }

            Dictionary<int, string> dicParentRes = new Dictionary<int, string>();
            if (listRes.Count > 0)
            {
                eRes res = new eRes(Pref, String.Join(",", listRes.Select(i => i.ToString()).ToArray()));

                string tabName = String.Empty;
                string fieldName = String.Empty;
                bool tabNameFound = false;
                bool fieldNameFound = false;

                if (_tabInfos.InterPP)
                {
                    int tabDescid = TableType.PP.GetHashCode();
                    int fieldDescid = tabDescid + 1;

                    tabName = res.GetRes(tabDescid, out tabNameFound);
                    if (tabNameFound)
                        dicParentRes.Add(tabDescid, tabName);
                    else
                        dicParentRes.Add(tabDescid, eResApp.GetRes(Pref, 5134));

                    fieldName = res.GetRes(fieldDescid, out fieldNameFound);
                    if (fieldNameFound)
                        dicParentRes.Add(fieldDescid, fieldName);
                    else
                        dicParentRes.Add(fieldDescid, eResApp.GetRes(Pref, 5136));

                }
                if (_tabInfos.InterPM)
                {
                    int tabDescid = TableType.PM.GetHashCode();
                    int fieldDescid = tabDescid + 1;

                    tabName = res.GetRes(tabDescid, out tabNameFound);
                    if (tabNameFound)
                        dicParentRes.Add(tabDescid, tabName);
                    else
                        dicParentRes.Add(tabDescid, eResApp.GetRes(Pref, 5129));

                    fieldName = res.GetRes(fieldDescid, out fieldNameFound);
                    if (fieldNameFound)
                        dicParentRes.Add(fieldDescid, fieldName);
                    else
                        dicParentRes.Add(fieldDescid, eResApp.GetRes(Pref, 5130));
                }
                if (_tabInfos.InterEVT && _tabInfos.TabType != TableType.EVENT)
                {
                    int tabDescid = _tabInfos.InterEVTDescid;
                    int fieldDescid = tabDescid + 1;

                    tabName = res.GetRes(tabDescid, out tabNameFound);
                    if (tabNameFound)
                        dicParentRes.Add(tabDescid, tabName);
                    else
                        dicParentRes.Add(tabDescid, eResApp.GetRes(Pref, 7251));

                    fieldName = res.GetRes(fieldDescid, out fieldNameFound);
                    if (fieldNameFound)
                        dicParentRes.Add(fieldDescid, fieldName);
                    else
                        dicParentRes.Add(fieldDescid, eResApp.GetRes(Pref, 7252));
                }
            }

            return dicParentRes;
        }

        private List<ListItem> GetQuickFilterSelectOptions(Dictionary<int, string> dicFieldsRes, Dictionary<int, string> dicParentsRes, Dictionary<int, FieldFormat> dicFieldsFormat, Dictionary<int, PopupType> dicFieldsPopup, int index)
        {
            List<ListItem> quickFilterListItems = new List<ListItem>();
            quickFilterListItems.Add(new ListItem(eResApp.GetRes(Pref, 248), "0"));

            //champs de la table en cours
            foreach (KeyValuePair<int, string> fieldRes in dicFieldsRes)
            {
                FieldFormat format = FieldFormat.TYP_HIDDEN;
                if (!dicFieldsFormat.TryGetValue(fieldRes.Key, out format))
                    continue;

                PopupType popup = PopupType.NONE;
                if (!dicFieldsPopup.TryGetValue(fieldRes.Key, out popup))
                    continue;

                int fieldNum = fieldRes.Key % 100;
                //faire une exception pour EVT01
                if (fieldNum == 1 && popup == PopupType.NONE)
                    continue;

                if (format != FieldFormat.TYP_CHAR && format != FieldFormat.TYP_BIT && format != FieldFormat.TYP_USER)
                    continue;

                string libelle = String.Concat( fieldRes.Value);

                //champ Appartient à - uniquement disponible en 2nd position
                if (fieldNum == (int)AllField.MULTI_OWNER || fieldNum == (int)AllField.TPL_MULTI_OWNER)
                {
                    if (index == 1)
                    {
                        string valueUsergroup = String.Concat(fieldRes.Key.ToString(), ";", "usergroup");
                        quickFilterListItems.Add(new ListItem(libelle, valueUsergroup));
                    }
                }
                else
                    quickFilterListItems.Add(new ListItem(libelle, fieldRes.Key.ToString()));
            }

            //champ 01 des parents
            if (_tabInfos.InterPP)
            {
                int tabDescid = TableType.PP.GetHashCode();
                int fieldDescid = tabDescid + 1;

                string libelle = String.Concat(dicParentsRes[tabDescid], ".", dicParentsRes[fieldDescid]);
                quickFilterListItems.Add(new ListItem(libelle, fieldDescid.ToString()));
            }
            if (_tabInfos.InterPM)
            {
                int tabDescid = TableType.PM.GetHashCode();
                int fieldDescid = tabDescid + 1;

                string libelle = String.Concat(dicParentsRes[tabDescid], ".", dicParentsRes[fieldDescid]);
                quickFilterListItems.Add(new ListItem(libelle, fieldDescid.ToString()));
            }
            if (_tabInfos.InterEVT && _tabInfos.TabType != TableType.EVENT)
            {
                int tabDescid = _tabInfos.InterEVTDescid;
                int fieldDescid = tabDescid + 1;

                string libelle = String.Concat(dicParentsRes[tabDescid], ".", dicParentsRes[fieldDescid]);
                quickFilterListItems.Add(new ListItem(libelle, fieldDescid.ToString()));
            }

            return quickFilterListItems;
        }
    }
}