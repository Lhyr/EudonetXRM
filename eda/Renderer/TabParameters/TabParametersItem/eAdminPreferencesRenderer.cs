using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPreferencesRenderer : eAdminBlockRenderer
    {
        protected IDictionary<eLibConst.CONFIGADV, String> _advConfigs = new Dictionary<eLibConst.CONFIGADV, String>();

        public eAdminPreferencesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "PrefPart")
        {

        }

        public static eAdminPreferencesRenderer CreateAdminPreferencesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminPreferencesRenderer features = new eAdminPreferencesRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        protected override bool Init()
        {
            base.Init();

            if (_tabInfos.DescId == (int)TableType.PP)
            {
                _advConfigs = eLibTools.GetConfigAdvValues(Pref, new HashSet<eLibConst.CONFIGADV> {
                    eLibConst.CONFIGADV.CREATE_PP_ADR_MODE
                });
            }

            return true;
        }

        private Dictionary<int, string> getParents()
        {
            Dictionary<int, string> dicParents = new Dictionary<int, string>();
            if (_tabInfos.InterPP)
                dicParents.Add((int)TableType.PP + 1, String.Empty);

            if (_tabInfos.InterPM)
                dicParents.Add((int)TableType.PM + 1, String.Empty);

            if (_tabInfos.InterEVT)
                dicParents.Add(_tabInfos.InterEVTDescid + 1, String.Empty);

            if (dicParents.Count == 0)
                return dicParents;

            List<int> lstRes = dicParents.Keys.Select(d => d - 1).ToList();
            eRes res = new eRes(_ePref, eLibTools.Join(",", lstRes));


            foreach (int id in lstRes)
            {
                dicParents[id + 1] = res.GetRes(id);
            }


            return dicParents;

        }

        protected override bool Build()
        {
            base.Build();

            string sBtnAdminPref = "eTools.AlertNotImplementedFunction();";
            bool bAfficBtnPref = true;
            EdnType[] ieForbiddenTypes = { EdnType.FILE_PLANNING, EdnType.FILE_RELATION, EdnType.FILE_MAIL, EdnType.FILE_SMS, EdnType.FILE_VOICING };
            eAdminField field;


            Dictionary<int, string> parents = getParents();


            //BSE: Exclure Campagne Mail qui sera géré dans une version ultérieure à la 10.603
            sBtnAdminPref = $@"nsAdmin.confPreferences(`{
                JsonConvert.SerializeObject(new {
                    _tabInfos.DescId,
                    _tabInfos.EudonetXIrisBlackStatus,
                    _tabInfos.EudonetXIrisCrimsonListStatus,
                    _tabInfos.EudonetXIrisPurpleGuidedStatus,
                    _tabInfos.CanUpdateWizardBar,
                    sJSonPageIrisBlack = JsonConvert.DeserializeObject(_tabInfos.sJSonPageIrisBlack.Replace("`", "")),
                    sJsonGuidedsJSonGuidedIrisPurple = JsonConvert.DeserializeObject(_tabInfos.sJSonGuidedIrisPurple.Replace("`", "")),
                    PurpleActivatedFrom = _tabInfos.PurpleActivatedFrom,
                    DESCADV_PARAMETER = new {
                        DESCADV_PARAMETER.ERGONOMICS_IRIS_BLACK,
                        DESCADV_PARAMETER.JSON_STRUCTURE_IRIS_BLACK,
                        DESCADV_PARAMETER.ERGONOMICS_LIST_IRIS_CRIMSON,
                        DESCADV_PARAMETER.ERGONOMICS_GUIDED_IRIS_PURPLE,
                        DESCADV_PARAMETER.JSON_STRUCTURE_GUIDED_IRIS_PURPLE,
                        DESCADV_PARAMETER.PURPLE_ACTIVATED_FROM
                    },
                    LOCATION_PURPLE_ACTIVATED = new {
                        LOCATION_PURPLE_ACTIVATED.NAVBAR,
                        LOCATION_PURPLE_ACTIVATED.MENU,
                        LOCATION_PURPLE_ACTIVATED.BOOKMARK
                    },
                    CATEGORY = eAdminUpdateProperty.CATEGORY.DESCADV,
                    parents = parents
                })}`, {System.Web.HttpContext.Current.IsDebuggingEnabled.ToString().ToLower()});";

            bAfficBtnPref = !(ieForbiddenTypes.Contains(_tabInfos.EdnType) || _tabInfos.DescId == TableType.CAMPAIGN.GetHashCode()) && _ePref.User.UserLevel > (int)UserLevel.LEV_USR_ADMIN;

            #region Construction des Liens
            // administrer les préférences
            if (bAfficBtnPref)
            {
                field = eAdminButtonField.GetEAdminButtonField(new eAdminButtonParams
                {
                    Label = eResApp.GetRes(Pref, 7019),
                    ID = "buttonAdminPreferences",
                    OnClick = sBtnAdminPref
                });

                field.Generate(_panelContent);
            }

            // administrer la minifiche
            // Ne pas afficher pour PP
            if (_tabInfos.DescId != (int)TableType.PP)
            {
                field = eAdminButtonField.GetEAdminButtonField(new eAdminButtonParams
                {
                    Label = eResApp.GetRes(Pref, 7020),
                    ID = "buttonAdminMiniFile",
                    ToolTip = eResApp.GetRes(Pref, 7021),
                    OnClick = "nsAdmin.confMiniFile()"
                });

                field.Generate(_panelContent);
            }
            else
            {
                // Format du nom complet                
                field = new eAdminDropdownField(_tab, eResApp.GetRes(Pref, 198), eAdminUpdateProperty.CATEGORY.DESCADV, DESCADV_PARAMETER.COMPLETENAMEFORMAT.GetHashCode(),
                    BuildNameChoices(), eResApp.GetRes(Pref, 7989), _tabInfos.NameFormat.GetHashCode().ToString(),
                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE, sortItemsByLabel: false);
                field.Generate(_panelContent);

                //Mode de création d'adresse
                List<ListItem> list = new List<ListItem>();
                list.Add(new ListItem(eResApp.GetRes(Pref, 1888), ((int)eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.LET_USER_CHOOSE).ToString()));
                list.Add(new ListItem(eResApp.GetRes(Pref, 1889), ((int)eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.DO_NOT_SUGGEST).ToString()));
                list.Add(new ListItem(eResApp.GetRes(Pref, 1890), ((int)eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.FORCE_ADR_CREATION).ToString()));

                eLibConst.CONFIGADV_CREATE_PP_ADR_MODE createAdrMode = eLibConst.CONFIGADV_CREATE_PP_ADR_MODE.LET_USER_CHOOSE;
                string strAdrMode = String.Empty;
                if (_advConfigs.TryGetValue(eLibConst.CONFIGADV.CREATE_PP_ADR_MODE, out strAdrMode))
                    Enum.TryParse<eLibConst.CONFIGADV_CREATE_PP_ADR_MODE>(strAdrMode, out createAdrMode);

                const string onChangeDefault = "nsAdmin.sendJson(this, false, true, true);"; // this : contrôle (DOM), false = confirmation de la modification, true = pas de vérification du DescID, true = Ne pas rafraichir le champ
                field = new eAdminDropdownField(0, eResApp.GetRes(Pref, 1886), eAdminUpdateProperty.CATEGORY.CONFIGADV, ((int)eLibConst.CONFIGADV.CREATE_PP_ADR_MODE),
                    list.ToArray(), eResApp.GetRes(Pref, 1887), ((int)createAdrMode).ToString(),
                    renderType: eAdminDropdownField.eAdminDropdownFieldRenderType.LABELABOVE, sortItemsByLabel: false);
                field.Generate(_panelContent);
                ((DropDownList)field.FieldControl).Attributes.Add("onchange", onChangeDefault);
                ((DropDownList)field.FieldControl).Attributes.Add("tabfld", eAdminTools.GetTabFldFromConfigKeyType(typeof(eLibConst.CONFIGADV)));
            }
            #endregion

            return true;
        }

        /// <summary>
        /// Liste des formats possibles pour le nom complet pour PP
        /// </summary>
        /// <returns></returns>
        private ListItem[] BuildNameChoices()
        {
            List<ListItem> list = new List<ListItem>();

            Dictionary<int, string> dicRes;
            Dictionary<int, int> dicFormat;
            _tabInfos.GetFields(Pref, out dicRes, out dicFormat);

            String label03 = String.Empty;
            int descid01 = TableType.PP.GetHashCode() + 1;
            int descid02 = TableType.PP.GetHashCode() + 2;
            int descid03 = TableType.PP.GetHashCode() + 3;

            if (dicRes.ContainsKey(descid01) && dicRes.ContainsKey(descid02))
            {
                if (dicRes.ContainsKey(descid03))
                    label03 = dicRes[descid03];

                list.Add(new ListItem(String.Concat(label03, " ", dicRes[descid01], " ", dicRes[descid02]), CompleteNameFormat.PP03_PP01_PP02.GetHashCode().ToString()));
                list.Add(new ListItem(String.Concat(dicRes[descid01], " (", label03, ") ", dicRes[descid02]), CompleteNameFormat.PP01_PP03_PP02.GetHashCode().ToString()));
                list.Add(new ListItem(String.Concat(dicRes[descid02], " ", label03, " ", dicRes[descid01]), CompleteNameFormat.PP02_PP03_PP01.GetHashCode().ToString()));
            }

            return list.ToArray();
        }
    }
}