using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Newtonsoft.Json;
using System.Web.UI.HtmlControls;
using static Com.Eudonet.Xrm.eda.eAdminFieldsListRenderer;
using System.Text;
using System.Web.UI;
using static Com.Eudonet.Internal.eUser;
using System.Linq;
using static Com.Eudonet.Common.Cryptography.CryptographyConst;
using Com.Eudonet.Common.Enumerations;
using EudoEnum = Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminParametersAdvRenderer : eAdminModuleRenderer
    {
        IDictionary<EudoEnum.CONFIGADV, String> _configs;

        private eAdminParametersAdvRenderer(ePref pref, IDictionary<EudoEnum.CONFIGADV, String> configs) : base(pref)
        {
            _configs = configs;
        }

        /// <summary>
        /// Génère le bloc paramètres avancés
        /// </summary>
        /// <param name="pref">pref user</param>
        /// <param name="configs">dico des parametres</param>
        /// <param name="opType">Type de parametre avancé</param>
        /// <returns></returns>
        public static eAdminParametersAdvRenderer CreateAdminParametersAdvRenderer(ePref pref, IDictionary<EudoEnum.CONFIGADV, String> configs, eUserOptionsModules.USROPT_MODULE opType = eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
        {
            eAdminParametersAdvRenderer rdr = new eAdminParametersAdvRenderer(pref, configs);
            rdr.OptType = opType;
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// construction html
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {


            Panel section = GetModuleSection(eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL_CONFIGADV.ToString(), eResApp.GetRes(Pref, 6750));
            PgContainer.Controls.Add(section);

            Panel targetPanel = null;
            if (section.Controls.Count > 0 && section.Controls[section.Controls.Count - 1] is Panel)
                targetPanel = (Panel)section.Controls[section.Controls.Count - 1];
            if (targetPanel == null)
                return false;

            String value = String.Empty;
            Dictionary<String, String> items = new Dictionary<String, String>();

            if (OptType != eUserOptionsModules.USROPT_MODULE.ADMIN_ORM)
            {

                AddTextboxOptionField(targetPanel, "textCalendarDayItemWidth", eResApp.GetRes(Pref, 7267), "",
                   eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.CALENDAR_DAY_ITEM_WIDTH.GetHashCode(), typeof(EudoEnum.CONFIGADV),
                   _configs[EudoEnum.CONFIGADV.CALENDAR_DAY_ITEM_WIDTH], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField");

                items = new Dictionary<String, String>();
                items.Add("1", eResApp.GetRes(Pref, 58));
                items.Add("0", eResApp.GetRes(Pref, 59));
                value = String.IsNullOrEmpty(_configs[EudoEnum.CONFIGADV.THUMBNAIL_ENABLED]) ? "0" : _configs[EudoEnum.CONFIGADV.THUMBNAIL_ENABLED];
                AddRadioButtonOptionField(targetPanel, "rbThumbnailEnabled", "rbThumbnailEnabled", eResApp.GetRes(Pref, 7268), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.THUMBNAIL_ENABLED.GetHashCode(), typeof(EudoEnum.CONFIGADV), items, value, EudoQuery.FieldFormat.TYP_BIT);


                items = new Dictionary<String, String>();
                items.Add("1", eResApp.GetRes(Pref, 58));
                items.Add("0", eResApp.GetRes(Pref, 59));
                value = String.IsNullOrEmpty(_configs[EudoEnum.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD]) ? "0" : _configs[EudoEnum.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD];
                AddRadioButtonOptionField(targetPanel, "rbNewUnsubscribeMethod", "rbNewUnsubscribeMethod", eResApp.GetRes(Pref, 1850), "",
                    eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)EudoEnum.CONFIGADV.USE_NEW_UNSUBSCRIBE_METHOD, typeof(EudoEnum.CONFIGADV), items, value, EudoQuery.FieldFormat.TYP_BIT);
            }

            //ORM
            if (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
            {

                Dictionary<string, string> customTextboxStyleAttributes = new Dictionary<string, string>() { { "width", "600px" } };

                //Obsolète
                if (!string.IsNullOrEmpty(_configs[EudoEnum.CONFIGADV.ORM_URL]))
                {
                    AddTextboxOptionField(targetPanel, "textUrlORM_URL", "ORM_URL (" + eResApp.GetRes(Pref, 2797) + ")" + eResApp.GetRes(Pref, 7269), "",
                     eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.ORM_URL.GetHashCode(), typeof(EudoEnum.CONFIGADV),
                     _configs[EudoEnum.CONFIGADV.ORM_URL], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                     customTextboxStyleAttributes: customTextboxStyleAttributes, customTextboxCSSClasses: "optionField");
                }

                // HLA - Ajout de la nouvelle entrée à l'ORM
                AddTextboxOptionField(targetPanel, "textUrlORM_SPECIF", eResApp.GetRes(Pref, 7269), "",
                   eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.ORM_EUDO_SPECIF.GetHashCode(), typeof(EudoEnum.CONFIGADV),
                   _configs[EudoEnum.CONFIGADV.ORM_EUDO_SPECIF], AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                   customTextboxStyleAttributes: customTextboxStyleAttributes, customTextboxCSSClasses: "optionField");


                AddTextboxOptionField(targetPanel, "textUrlORM_EXT", eResApp.GetRes(Pref, 2798), "",
                   eAdminUpdateProperty.CATEGORY.CONFIGADV, EudoEnum.CONFIGADV.ORM_EUDO_EXT.GetHashCode(), typeof(EudoEnum.CONFIGADV),
                   _configs[EudoEnum.CONFIGADV.ORM_EUDO_EXT], AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE,
                   customTextboxStyleAttributes: customTextboxStyleAttributes, customTextboxCSSClasses: "optionField");



                if (OptType == eUserOptionsModules.USROPT_MODULE.ADMIN_ORM)
                {

                    #region bouton d'admin produit
                    //Bouton admin specif
                    if (!string.IsNullOrEmpty(_configs[EudoEnum.CONFIGADV.ORM_ADMIN_EXT]))
                    {
                        string sAdminExtVal = _configs[EudoEnum.CONFIGADV.ORM_ADMIN_EXT];
                        foreach (var spec in sAdminExtVal.Split(';'))
                        {
                            int specId;
                            if (!Int32.TryParse(spec, out specId))
                                continue;

                            string sLabel = "";
                            try
                            {
                                var ew = eSpecif.GetSpecif(Pref, specId);
                                sLabel = ew.Label;
                            }
                            catch
                            {
                                sLabel = eResApp.GetRes(Pref, 3004);
                            }

                            //Btn
                            eAdminButtonField btnAdminExt = eAdminButtonField.GetEAdminButtonField(param:
                                    new eAdminButtonParams()
                                    {
                                        OnClick = "runSpec(" + specId + ");",
                                        Label = sLabel
                                    }
                                    );


                            btnAdminExt.Generate(targetPanel);
                            btnAdminExt.SetFieldAttribute("style", "display:block");

                        }
                    }

                    #endregion

                    #region gestion des jetons

                    Panel sectionToken = GetModuleSection("TOKEN_CNX", "Gestion des tokens");
                    PgContainer.Controls.Add(sectionToken);

                    Panel targetPanelToken = null;
                    if (sectionToken.Controls.Count > 0 && sectionToken.Controls[section.Controls.Count - 1] is Panel)
                        targetPanelToken = (Panel)sectionToken.Controls[sectionToken.Controls.Count - 1];
                    if (targetPanelToken == null)
                        return false;

                    string sError;
                    Panel pLstTok = new Panel();
                    pLstTok.CssClass = "field lsttokenpanel";
                    targetPanelToken.Controls.Add(pLstTok);

                    List<APPKEY> lstKeys = APPKEY.GetAppKeys(_ePref, _ePref.User, new List<TokenType>() { TokenType.ORM_WS, TokenType.APPLICATION_KEY });
                    pLstTok.Controls.Add(CreateTable(lstKeys, out sError));



                    Panel pAddNewTok = new Panel();
                    pAddNewTok.CssClass = "field addtokenpanel";
                    targetPanelToken.Controls.Add(pAddNewTok);

                    #region Type

                    List<ListItem> itemsTokenTypeList = new List<ListItem>();


                    itemsTokenTypeList.Add(new ListItem("Web Service Interne", ((int)TokenType.ORM_WS).ToString()));
                    itemsTokenTypeList.Add(new ListItem("Clée d'Application", ((int)TokenType.APPLICATION_KEY).ToString()));

                    AddDropdownOptionField(pAddNewTok, "INPUT_ORMTOKEN_TYPE", eResApp.GetRes(Pref, 105), "",
                        eAdminUpdateProperty.CATEGORY.CUSTOM,
                        0,
                        typeof(eLibConst.CONFIG_DEFAULT),
                        itemsTokenTypeList, ((int)TokenType.ORM_WS).ToString(),
                        EudoQuery.FieldFormat.TYP_NUMERIC,

                        eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE,
                        onChange: "",
                        customPanelCSSClasses: "admininputinline",
                        customDropdownStyleAttributes: new Dictionary<string, string>() { { "width", "200px" } },
                        customLabelCSSClasses: "info", sortItemsByLabel: true); ;
                    #endregion

                    #region AppName
                    //Application Name
                    Control ctrl = AddTextboxOptionField(
                                 targetPanel: pAddNewTok,
                                 id: "INPUT_ORMTOKEN_APPNAME",
                                 label: eResApp.GetRes(Pref, 5140), //Application
                                 tooltip: "",
                                 propCat: eAdminUpdateProperty.CATEGORY.CUSTOM,
                                 propKeyCode: 0,
                                 propKeyType: typeof(EudoEnum.CONFIGADV),
                                 currentValue: "",
                                 adminFieldType: AdminFieldType.ADM_TYPE_CHAR,
                                 labelType: eAdminTextboxField.LabelType.INLINE,
                                 onChange: "",
                                 customPanelCSSClasses: "admininputinline",
                                 customTextboxStyleAttributes: new Dictionary<string, string>() { { "width", "200px" } },
                                 customTextboxCSSClasses: "optionField"
                                 );

                    //taille max
                    ((TextBox)ctrl).Attributes.Add("maxlength", "50");
                    //((TextBox)ctrl).Attributes.Add("placeholder", "Mon Application");
                    #endregion

                    #region Droits

                    List<ListItem> itemsRightType = new List<ListItem>();
                    itemsRightType.Add(new ListItem("ORM WS", ((int)TokenRight.ORM_WS).ToString()));
                    itemsRightType.Add(new ListItem(eResApp.GetRes(Pref, 6340), ((int)TokenRight.IMPORT).ToString()));
                    itemsRightType.Add(new ListItem(eResApp.GetRes(Pref, 398), ((int)TokenRight.EXPORT).ToString()));
                    itemsRightType.Add(new ListItem(eResApp.GetRes(Pref, 6885), ((int)TokenRight.NOTIFICATIONS_STANDARD).ToString()));


                    Control ctrlSel = AddDropdownOptionField(pAddNewTok, "INPUT_ORMTOKEN_APPRIGHTS", eResApp.GetRes(Pref, 7406), "",
                    eAdminUpdateProperty.CATEGORY.CUSTOM,
                    0,
                    typeof(eLibConst.CONFIG_DEFAULT),
                    itemsRightType,
                    "",
                    EudoQuery.FieldFormat.TYP_NUMERIC,

                    eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE,
                    onChange: "",
                    customPanelCSSClasses: "admininputinline",
                    customDropdownStyleAttributes: new Dictionary<string, string>() { { "width", "200px" } },

                    customLabelCSSClasses: "info", sortItemsByLabel: true);

                    ((DropDownList)ctrlSel).Attributes.Add("multiple", "1");


                    #endregion

                    #region USER
                    //User
                    List<ListItem> itemsList = new List<ListItem>();
                    foreach (var us in _listUsersAndGroups)
                    {
                        if (!us.Disabled && us.Level <= Pref.User.UserLevel)
                            itemsList.Add(new ListItem(us.Libelle, us.ItemCode));
                    }



                    AddDropdownOptionField(pAddNewTok, "INPUT_ORMTOKEN_USERID", eResApp.GetRes(Pref, 6), "",
                        eAdminUpdateProperty.CATEGORY.CUSTOM,
                        0,
                        typeof(eLibConst.CONFIG_DEFAULT),
                        itemsList, Pref.User.UserId.ToString(),
                        EudoQuery.FieldFormat.TYP_NUMERIC,

                        eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE,
                        onChange: "",
                        customPanelCSSClasses: "admininputinline",
                        customDropdownStyleAttributes: new Dictionary<string, string>() { { "width", "200px" } },
                        customLabelCSSClasses: "info", sortItemsByLabel: true);
                    #endregion

                    #region Date expiration




                    Panel pnlDate = new Panel();
                    pAddNewTok.Controls.Add(pnlDate);
                    pnlDate.CssClass = "field admininputinline";
                    pnlDate.Attributes.Add("data-active", "1");

                    HtmlGenericControl labelDateExpiration = new HtmlGenericControl("label");
                    pnlDate.Controls.Add(labelDateExpiration);
                    labelDateExpiration.Attributes.Add("class", "info");


                    labelDateExpiration.InnerHtml = eResApp.GetRes(Pref, 5164); //  date expiration
                    HtmlInputText inptDateExpiration = new HtmlInputText();
                    inptDateExpiration.Attributes.Add("class", "optionField");
                    pnlDate.Controls.Add(inptDateExpiration);

                    eGenericWebControl imgBtmInptDateExpiration = new eGenericWebControl(HtmlTextWriterTag.Span);
                    pnlDate.Controls.Add(imgBtmInptDateExpiration);
                    imgBtmInptDateExpiration.ID = "imgBtmInptDateExpiration";
                    inptDateExpiration.ID = "INPUT_ORMTOKEN_DATEEXP";
                    inptDateExpiration.Name = inptDateExpiration.ID;
                    imgBtmInptDateExpiration.CssClass = "icon-agenda btnIe8 trt_btn";

                    inptDateExpiration.Attributes.Add("class", "mailingCampaignInfoCategory readonly inputDate");
                    inptDateExpiration.Attributes.Add("readonly", "true");

                    inptDateExpiration.Attributes.Add("onclick", "document.getElementById('purgeLinkTrack').click();");

                    imgBtmInptDateExpiration.OnClick = String.Concat("nsAdminORM.onSelectExiprationDate('", inptDateExpiration.ID, "');");

                    #endregion



                    //Btn
                    eAdminButtonField btnAdd = eAdminButtonField.GetEAdminButtonField(param:
                            new eAdminButtonParams()
                            {
                                OnClick = "nsAdminORM.NewToken()",
                                Label = eResApp.GetRes(Pref, 2804),
                            }
                            );


                    btnAdd.Generate(pAddNewTok);
                    btnAdd.SetFieldAttribute("style", "display:block");
                    #endregion

                }
            }



            return true;
        }

        List<UserListItem> _listUsersAndGroups = new List<UserListItem>();


        /// <summary>
        /// type de parametre avancé
        /// </summary>
        public eUserOptionsModules.USROPT_MODULE OptType { get; private set; }

        /// <summary>
        /// Construction du tableau HTML
        /// </summary>
        /// <param name="list"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        private HtmlGenericControl CreateTable(List<APPKEY> list, out String sError)
        {
            sError = String.Empty;

            int lineNum = 1;

            HtmlGenericControl htmlTable = new HtmlGenericControl("table");
            HtmlGenericControl sectionThead = new HtmlGenericControl("thead");
            HtmlGenericControl sectionTbody = new HtmlGenericControl("tbody");
            HtmlGenericControl tableTr = new HtmlGenericControl("tr");
            HtmlGenericControl tableTh = new HtmlGenericControl("th");
            HtmlGenericControl tableTd = new HtmlGenericControl("td");

            htmlTable.ID = "tableFieldsList";
            htmlTable.Attributes.Add("class", "mTab admintabtoken");

            #region HEADER
            htmlTable.Controls.Add(sectionThead);

            sectionThead.Controls.Add(tableTr);

            int colNum = 0;
            String thClass;

            var listColumns = new List<ListCol>()
            {
            //     new ListCol("TOKENID", 20, FieldFormat.TYP_ID),
                 new ListCol( eResApp.GetRes(Pref, 105) , 20, FieldFormat.TYP_CHAR), // Type
                 new ListCol( eResApp.GetRes(Pref, 5140) , 20, FieldFormat.TYP_CHAR), // Application
                 new ListCol( eResApp.GetRes(Pref, 2803), 20, FieldFormat.TYP_CHAR), // Jeton
                 new ListCol(eResApp.GetRes(Pref, 690), 20, FieldFormat.TYP_BIT), //désactivé
                 new ListCol(eResApp.GetRes(Pref, 6), 20, FieldFormat.TYP_NUMERIC), //utilisateur
                 new ListCol(eResApp.GetRes(Pref, 5164), 20, FieldFormat.TYP_DATE), //expiration
                 new ListCol(eResApp.GetRes(Pref, 7406), 20, FieldFormat.TYP_CHAR), // Rights
                 new ListCol(eResApp.GetRes(Pref, 5076), 20, FieldFormat.TYP_DATE), // cree le
                 new ListCol(eResApp.GetRes(Pref, 5078), 20, FieldFormat.TYP_NUMERIC), // cree par
                 new ListCol("", 20),
                 new ListCol("", 20),
              };

            foreach (ListCol col in listColumns)
            {
                thClass = "hdBgCol";

                tableTh = new HtmlGenericControl("th");
                if (col.Format == FieldFormat.TYP_BIT)
                {
                    thClass += " chkCol";
                }
                else if (col.Format == FieldFormat.TYP_NUMERIC)
                {
                    thClass += " numCol";
                }
                if (col.HideForPrint)
                {
                    thClass += " hiddenForPrint";
                }

                tableTh.Attributes.Add("class", thClass);
                tableTh.Style.Add("min-width", col.Width + "px");
                tableTh.InnerHtml = String.Concat(
                    "<div>",
                    col.Label,
                    "<div class='buttonsSort'>",
                    "</div>",
                    "</div>"
                    );
                tableTh.Attributes.Add("title", col.Label);
                tableTr.Controls.Add(tableTh);

                colNum++;
            }
            #endregion

            #region BODY

            htmlTable.Controls.Add(sectionTbody);

            eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
            eDal.OpenDatabase();

            try
            {
                String ruleDef = String.Empty;
                int TokenId = 0;
                eUser usrObj = new eUser(eDal, Pref.User, eUser.ListMode.USERS_ONLY, Pref.GroupMode, new List<string>());
                StringBuilder sbError = new StringBuilder();
                _listUsersAndGroups = usrObj.GetUserList(true, true, "", sbError);

                //sectionTbody.Style.Add("height", _nTableH + "px");
                foreach (APPKEY fi in list)
                {
                    TokenId = fi.Id;

                    string css = "";
                    if (fi.ExpirationDate < DateTime.Now)
                        css = "tokenExpire";

                    tableTr = new HtmlGenericControl("tr");
                    tableTr.Attributes.Add("class", String.Concat("line", lineNum, " ", css));
                    tableTr.Attributes.Add("data-tokdid", fi.Id.ToString());

                    lineNum = (lineNum == 1) ? 2 : 1;


                    //Type
                    CreateCell(tableTr, TokenId, fi.Type == TokenType.ORM_WS ? "Web Service Interne" : "Application");

                    // AppName
                    CreateCell(tableTr, TokenId, fi.AppName);


                    // TOKEN
                    string sPubTok = eLoginTools.GetCnxTokenKey(new CnxToken()
                    {
                        Key = fi.Token,
                        DBUID = Pref.DatabaseUid,
                        tokentype = fi.Type

                    });

                    Dictionary<string, string> d = new Dictionary<string, string>();
                    string sTT = "";
                    if (fi.InvalidToken)
                    {
                        sTT = eResApp.GetRes(Pref, 2802);
                        sPubTok = eResApp.GetRes(Pref, 2801);
                    }
                    else
                    {
                        d["ondblclick"] = "nsAdminField.CopyTextToClipBoard('" + sPubTok + "')";
                        sTT = eResApp.GetRes(Pref, 2314);
                    }

                    string sCssToken = "admincelltoken";
                    CreateCell(tableTr, TokenId, "", tooltip: sTT, dCustomAction: d, cssClass: sCssToken);

                    //Disabled
                    CreateCell(tableTr, TokenId, fi.Disabled ? eResApp.GetRes(Pref, 58) : eResApp.GetRes(Pref, 59), isBtn: false, sActionBtn: "nsAdminORM.Disable(" + TokenId + ")");

                    //Userid
                    var uitem = _listUsersAndGroups.Find(u => u.ItemCode == fi.UserId.ToString());
                    CreateCell(tableTr, TokenId, uitem?.Libelle ?? fi.UserId.ToString());


                    //EXPIRATIONDATE
                    if (fi.ExpirationDate == null)
                        fi.ExpirationDate = DateTime.MaxValue;

                    CreateCell(tableTr, TokenId, fi.ExpirationDate.Value.Year == 9999 ? eResApp.GetRes(Pref, 8249) : fi.ExpirationDate.ToString(), cssClass: css);



                    //rights
                    string sRights = "";
                    if (fi.Rights != null && fi.Rights.Count > 0)
                    {
                        foreach (var ri in fi.Rights)
                        {
                            if (sRights.Length > 0)
                                sRights += ", ";

                            sRights += ri.ToString();
                        }
                    }

                    CreateCell(tableTr, TokenId, sRights);

                    //CREATIONDATE
                    CreateCell(tableTr, TokenId, fi.CreationDate.ToString());

                    //CREATEDBY 
                    uitem = _listUsersAndGroups.Find(u => u.ItemCode == fi.CreatedBy.ToString());
                    CreateCell(tableTr, TokenId, uitem?.Libelle ?? fi.CreatedBy.ToString());

                    //Supprimer
                    CreateCell(tableTr, TokenId, eResApp.GetRes(Pref, 19), isBtn: true, sActionBtn: "nsAdminORM.DeleteToken(" + TokenId + ")");

                    //Désativer
                    CreateCell(tableTr, TokenId,
                        fi.Disabled ? eResApp.GetRes(Pref, 6744) : eResApp.GetRes(Pref, 6745),

                        isBtn: true, sActionBtn: "nsAdminORM.Disable(" + TokenId + ", " + (fi.Disabled ? "0" : "1") + "  )"); ;


                    sectionTbody.Controls.Add(tableTr);
                }


                #endregion
            }
            catch (Exception exc)
            {
                sError = exc.Message;
            }
            finally
            {
                eDal.CloseDatabase();
            }


            return htmlTable;
        }


        /// <summary>
        /// Création d'une cellule avec sa valeur
        /// </summary>
        /// <param name="tr">Ligne "tr"</param>
        /// <param name="tokenId">The descid.</param>
        /// <param name="value">Valeur</param>         
        /// /// <param name="tooltip">Infobulle</param> 
        /// <param name="isBtn">type boutoun</param>
        /// <param name="dCustomAction">list d'action personnalisé</param>
        /// <param name="sActionBtn">Action du bouton</param>
        /// <param name="cssClass">class css</param>

        HtmlGenericControl CreateCell(HtmlGenericControl tr,
            int tokenId,
            String value = "",
            String tooltip = "",
            Boolean isBtn = false,
            string sActionBtn = "",
            Dictionary<string, string> dCustomAction = null,
            string cssClass = ""
            )

        {

            if (dCustomAction == null)
                dCustomAction = new Dictionary<string, string>();


            HtmlGenericControl td = new HtmlGenericControl("td");

            cssClass += " hiddenForPrint";
            td.Attributes.Add("class", cssClass);

            String htmlIcon = String.Empty;


            td.Attributes.Add("value", value);

            if (!isBtn)
            {
                td.InnerHtml = String.Concat("<span class='cellContent'>", "<span class='cellValue'>", value, "</span>", htmlIcon, "</span>");
            }
            else
            {
                eButtonCtrl btn = new eButtonCtrl(value, eButtonCtrl.ButtonType.GRAY, sActionBtn);


                btn.Attributes.Add("did", tokenId.ToString());
                td.Controls.Add(btn);

            }


            if (tooltip != "")
                td.Attributes.Add("title", (tooltip == "") ? value : tooltip);

            if (dCustomAction.Count > 0)
            {
                foreach (var kvp in dCustomAction)
                {
                    td.Attributes.Add(kvp.Key, kvp.Value);
                }
            }

            tr.Controls.Add(td);
            return td;
        }

    }
}