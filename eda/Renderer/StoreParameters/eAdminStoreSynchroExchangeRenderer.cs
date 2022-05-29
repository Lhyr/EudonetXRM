using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.synchroExchange;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Rendu des paramètres de la synchro Office 365
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eda.eAdminStoreFileRenderer" />
    public class eAdminStoreSynchroExchangeRenderer : eAdminStoreFileRenderer
    {
        #region Propriétés
        IDictionary<eLibConst.CONFIGADV, String> _configs;
        List<ListItem> _usersItemsList = new List<ListItem>();
        List<ListItem> _timezonesItemsList = new List<ListItem>();
        List<ListItem> _planningTabsItemsList = new List<ListItem>();

        List<eSyncUser> _usersList = new List<eSyncUser>();

        int _mappingTab = 0;



        string _syncUsers = string.Empty;
        string _syncUsersLabel = string.Empty;
        #endregion

        #region Struct/Sous-classes
        public struct eSyncUser
        {
            public int UserId;
            public string Login;
            public string DisplayName;
            public string Email;
        }

        #endregion




        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreSynchroExchangeRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// Creates the admin extension synchro exchange renderer.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="ext">The extension file identifier from store.</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public static eAdminStoreSynchroExchangeRenderer CreateAdminStoreSynchroExchangeRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();


            eAdminStoreSynchroExchangeRenderer rdr = new eAdminStoreSynchroExchangeRenderer(pref, ext);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Initialisation des données
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                List<eExtension> extensionList = eExtension.GetExtensionsByCode(_ePref, ExtensionCode.SYNCROEXCHANGE);
                if (extensionList.Count > 0 && !String.IsNullOrEmpty(extensionList[0].Param))
                {

                }

                // CONFIGADV
                _configs = eLibTools.GetConfigAdvValues(Pref,
               new HashSet<eLibConst.CONFIGADV> {
                       eLibConst.CONFIGADV.SYNC365_ENABLED,
                       eLibConst.CONFIGADV.SYNC365_CLIENTID,
                       eLibConst.CONFIGADV.SYNC365_SECRETID,
                       eLibConst.CONFIGADV.SYNC365_TENANTID,
                       eLibConst.CONFIGADV.SYNC365_EXT_APPOINTMENT_USER,
                       eLibConst.CONFIGADV.SYNC365_REF_TIMEZONE,
                       eLibConst.CONFIGADV.SYNC365_MAPPING_TAB,
                       eLibConst.CONFIGADV.SYNC365_USERS
               });

                StringBuilder sbError = new StringBuilder();
                eudoDAL eDal = eLibTools.GetEudoDAL(Pref);
                try
                {
                    eDal.OpenDatabase();

                    #region  Liste des utilisateurs
                    eDataFillerGeneric df = new eDataFillerGeneric(_ePref, (int)TableType.USER, ViewQuery.CUSTOM);
                    df.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                    {
                        eq.SetListCol = String.Concat(
                            (int)UserField.LOGIN, ";", (int)UserField.UserDisplayName, ";", (int)UserField.EMAIL
                            );
                    };
                    df.Generate();
                    if (df.ErrorMsg.Length > 0)
                        return false;

                    string login = string.Empty;
                    string displayName = string.Empty;
                    string email = string.Empty;
                    foreach (eRecord item in df.ListRecords)
                    {
                        login = item.GetFieldByAlias(String.Concat((int)TableType.USER, "_", (int)UserField.LOGIN))?.DisplayValue ?? "";
                        displayName = item.GetFieldByAlias(String.Concat((int)TableType.USER, "_", (int)UserField.UserDisplayName))?.DisplayValue ?? "";
                        email = item.GetFieldByAlias(String.Concat((int)TableType.USER, "_", (int)UserField.EMAIL))?.DisplayValue ?? "";

                        _usersList.Add(new eSyncUser
                        {
                            UserId = item.MainFileid,
                            DisplayName = displayName,
                            Login = login,
                            Email = email
                        });

                    }

                    _usersItemsList = _usersList.Select(u => new ListItem(u.Login, u.UserId.ToString())).ToList();
                    _usersItemsList.Insert(0, new ListItem(eResApp.GetRes(_ePref, 6211), "0"));
                    #endregion

                    // Utilisateurs synchronisés
                    _syncUsers = _configs[eLibConst.CONFIGADV.SYNC365_USERS];
                    if (!String.IsNullOrEmpty(_syncUsers))
                    {
                        eUser userObj = new eUser(eDal, Pref.User, eUser.ListMode.USERS_ONLY, SECURITY_GROUP.GROUP_NONE);
                        List<eUser.UserListItem> usersList = userObj.GetUserList(true, true, "", _syncUsers.Split(';').ToList(), sbError);
                        if (usersList.Count > 0)
                        {
                            _syncUsersLabel = String.Join(";", usersList.Select(u => u.Libelle));
                        }
                    }

                    // Liste des tables planning
                    List<Tuple<int, string>> planningTables = eAdminTools.GetPlanningTables(_ePref, eDal);
                    _planningTabsItemsList = planningTables.Select(p => new ListItem(p.Item2, p.Item1.ToString())).ToList();
                    _planningTabsItemsList.Insert(0, new ListItem(eResApp.GetRes(_ePref, 6211), "0"));

                    // Mapping
                    _mappingTab = eLibTools.GetNum(_configs[eLibConst.CONFIGADV.SYNC365_MAPPING_TAB]);

                    if (_mappingTab > 0)
                    {
                        //Si on a sélectionné une table, on ajoutes les colonnes supplémentaire nécessaire (guest, timezone, etc)
                        SynchroExchangeTools.CreateMissingPlanningColumn(_mappingTab, Pref, eDal);
                    }
                }
                finally
                {
                    eDal.CloseDatabase();
                }

                // Liste des fuseaux horaires
                ReadOnlyCollection<TimeZoneInfo> tz = TimeZoneInfo.GetSystemTimeZones();
                _timezonesItemsList = tz.Select(t => new ListItem(t.DisplayName, t.Id)).ToList();
                _timezonesItemsList.Insert(0, new ListItem(eResApp.GetRes(_ePref, 6211), "0"));

                return true;
            }

            return false;
        }

        /// <summary>
        /// écran de paramétrage
        /// </summary>
        protected override void CreateSettingsPanel()
        {
            base.CreateSettingsPanel();

            Panel section = null;

            #region Section "Paramètres généraux"
            section = GetModuleSection(Extension.Module.ToString(), eResApp.GetRes(Pref, 7179));
            ExtensionParametersContainer.Controls.Add(section);
            BuildParametersSection(GetSectionContentPanel(section));

            #endregion

            #region Section Mapping
            section = GetModuleSection(
                Extension.Module.ToString(),
                Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE ? eResApp.GetRes(_ePref, 2180) : eResApp.GetRes(_ePref, 8526));
            ExtensionParametersContainer.Controls.Add(section);
            Panel content = GetSectionContentPanel(section);
            content.ID = "syncMappingContent";
            BuildMappingSection(content);

            #endregion

            #region Section Utilisateurs synchronisés
            section = GetModuleSection(Extension.Module.ToString(), eResApp.GetRes(_ePref, 8530));
            ExtensionParametersContainer.Controls.Add(section);
            content = GetSectionContentPanel(section);
            content.ID = "syncUsersContent";
            BuildUsersSection(content);
            #endregion

        }


        /// <summary>
        /// Création de la section Paramètres généraux
        /// </summary>
        /// <param name="targetPanel">The target panel.</param>
        /// <returns></returns>
        bool BuildParametersSection(Panel targetPanel)
        {
            if (targetPanel == null)
                return false;

            eLibConst.CONFIGADV_CATEGORY configADVCat = eLibConst.CONFIGADV_CATEGORY.SYNCHRO_OFFICE365;

            // Activer synchro
            AddCheckboxOptionField(
                targetPanel, "chkActiveSync",
                Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE ? eResApp.GetRes(_ePref, 2181) : eResApp.GetRes(_ePref, 8522),
                "", eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SYNC365_ENABLED, typeof(eLibConst.CONFIGADV),
                _configs[eLibConst.CONFIGADV.SYNC365_ENABLED] == "1", configAdvCat: configADVCat);

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.Attributes.Add("class", "info");
            p.InnerHtml = eResApp.GetRes(_ePref, 8523);
            targetPanel.Controls.Add(p);

            Panel pInfosAPI = new Panel();
            pInfosAPI.ID = "syncAPIInfos";
            targetPanel.Controls.Add(pInfosAPI);

            // ClientID
            AddTextboxOptionField(pInfosAPI, "txtClientId", eResApp.GetRes(Pref, 8490), "",
               eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.SYNC365_CLIENTID.GetHashCode(), typeof(eLibConst.CONFIGADV),
               _configs[eLibConst.CONFIGADV.SYNC365_CLIENTID], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField", configAdvCat: configADVCat);

            // SecretID
            AddTextboxOptionField(pInfosAPI, "txtSecretId", eResApp.GetRes(Pref, 8491), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.SYNC365_SECRETID.GetHashCode(), typeof(eLibConst.CONFIGADV),
                _configs[eLibConst.CONFIGADV.SYNC365_SECRETID], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField", configAdvCat: configADVCat);

            // TenantID
            AddTextboxOptionField(pInfosAPI, "txtTenantId", eResApp.GetRes(Pref, 8492), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, eLibConst.CONFIGADV.SYNC365_TENANTID.GetHashCode(), typeof(eLibConst.CONFIGADV),
                _configs[eLibConst.CONFIGADV.SYNC365_TENANTID], EudoQuery.AdminFieldType.ADM_TYPE_CHAR, eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField", configAdvCat: configADVCat);

            // Afficher les RDV des organisateurs externes à l'utilisateur...
            AddDropdownOptionField(targetPanel, "ddlExternalAppointmentsUser", eResApp.GetRes(_ePref, 8524), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SYNC365_EXT_APPOINTMENT_USER, typeof(eLibConst.CONFIGADV),
                _usersItemsList, _configs[eLibConst.CONFIGADV.SYNC365_EXT_APPOINTMENT_USER], FieldFormat.TYP_USER, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, sortItemsByLabel: false, configAdvCat: configADVCat);

            // Fuseau horaire de référence pour l'agenda Eudonet
            AddDropdownOptionField(targetPanel, "ddlTimeZones", eResApp.GetRes(_ePref, 8525), "",
                eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SYNC365_REF_TIMEZONE, typeof(eLibConst.CONFIGADV),
                _timezonesItemsList, _configs[eLibConst.CONFIGADV.SYNC365_REF_TIMEZONE], FieldFormat.TYP_CHAR, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, configAdvCat: configADVCat);

            return true;
        }

        /// <summary>
        /// Création de la section Mapping
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        bool BuildMappingSection(Panel container)
        {
            // Onglet synchronisé
            AddDropdownOptionField(container, "ddlSyncOffice365MappingTab", eResApp.GetRes(_ePref, 8527), "", eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SYNC365_MAPPING_TAB, typeof(eLibConst.CONFIGADV),
                _planningTabsItemsList, _mappingTab.ToString(), FieldFormat.TYP_NUMERIC, eAdminDropdownField.eAdminDropdownFieldRenderType.INLINE, configAdvCat: eLibConst.CONFIGADV_CATEGORY.SYNCHRO_OFFICE365,
                customLabelCSSClasses: "bold");

            eAdminButtonField btnField = new eAdminButtonField(
                Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE ?
                    eResApp.GetRes(_ePref, 2180) :
                    eResApp.GetRes(_ePref, 8526),
                "btnSync365Mapping",
                onclick: String.Concat("nsAdminSynchroExchange.confMapping('", Extension.Module.ToString(), "')")
            );
            btnField.Generate(container);
            btnField.PanelField.CssClass = "field linkButton";

            return true;
        }




        /// <summary>
        /// Construction de la section "Utilisateurs synchronisés"
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        bool BuildUsersSection(Panel container)
        {

            eAdminIconField icon = new eAdminIconField("btnSyncUsers", "icon-catalog", onclick: "nsAdminSynchroExchange.showUsersCat(this)");

            eAdminField txtField = new eAdminTextboxField(0, eResApp.GetRes(_ePref, 8531), eAdminUpdateProperty.CATEGORY.CONFIGADV, (int)eLibConst.CONFIGADV.SYNC365_USERS, value: _syncUsersLabel,
                icon: icon, labelType: eAdminTextboxField.LabelType.INLINE, customTextboxCSSClasses: "optionField");
            txtField.SetFieldControlID("txtSyncUsers");
            txtField.SetConfigAdvCategory(eLibConst.CONFIGADV_CATEGORY.SYNCHRO_OFFICE365);
            txtField.Generate(container);
            TextBox textbox = ((TextBox)txtField.FieldControl);
            textbox.Attributes.Add("onchange", DEFAULT_OPTION_ONCHANGE);
            textbox.Attributes.Add("tabfld", "cfgadv");
            textbox.Attributes.Add("dbv", _syncUsers);
            textbox.ReadOnly = true;
            textbox.ToolTip = _syncUsersLabel;

            BuildSyncUsersTable(container);

            return true;
        }

        void BuildSyncUsersTable(Panel container)
        {
            //List<string> colHeaders = new List<string>()
            //{
            //     eResApp.GetRes( _ePref , 411),  eResApp.GetRes( _ePref , 198),  eResApp.GetRes( _ePref , 656), ""
            //};
            //List<List<string>> cellsLabels = new List<List<string>>();
            //foreach (eUser.UserListItem u in _usersList)
            //{
            //    cellsLabels.Add()
            //}
            //HtmlTable tabTable = GetTable("syncUsersTable", colHeaders,        );

            TableRow tr;
            TableCell tc;
            HtmlGenericControl icon;



            System.Web.UI.WebControls.Table table = new System.Web.UI.WebControls.Table();
            table.ID = "syncUsersTable";
            table.CssClass = "mTab";

            #region Entête
            TableHeaderRow thr = new TableHeaderRow();
            thr.CssClass = "hdBgCol";
            TableHeaderCell thc = new TableHeaderCell();
            thc.Text = eResApp.GetRes(_ePref, 411);
            thr.Cells.Add(thc);
            thc = new TableHeaderCell();
            thc.Text = eResApp.GetRes(_ePref, 198);
            thr.Cells.Add(thc);
            thc = new TableHeaderCell();
            thc.Text = eResApp.GetRes(_ePref, 656);
            thr.Cells.Add(thc);
            thc = new TableHeaderCell();
            thc.ID = "tcWarningHeader";
            thc.Text = "";
            thr.Cells.Add(thc);
            table.Rows.Add(thr);
            #endregion

            #region Utilisateurs
            string warning = string.Empty;
            int altLine = 1;
            List<string> syncUsersDid = _syncUsers.Split(";").ToList();
            List<eSyncUser> syncUsers = _usersList.Where(u => syncUsersDid.Contains(u.UserId.ToString())).ToList();
            foreach (eSyncUser u in syncUsers)
            {
                tr = new TableRow();
                tr.CssClass = String.Concat("line", altLine);
                altLine = (altLine == 1) ? 2 : 1;

                tc = new TableCell();
                tc.Text = u.Login;
                tr.Cells.Add(tc);

                tc = new TableCell();
                tc.Text = u.DisplayName;
                tr.Cells.Add(tc);

                tc = new TableCell();
                tc.Text = u.Email;
                tr.Cells.Add(tc);

                tc = new TableCell();
                tc.CssClass = "tcWarning";
                icon = new HtmlGenericControl();
                icon.Attributes.Add("class", "icon-warning2 syncWarning");
                if (IsWarningIconDisplayed(syncUsers, u, out warning))
                {
                    icon.Visible = true;
                    icon.Attributes.Add("title", warning);
                }
                else
                {
                    icon.Visible = false;
                }
                tc.Controls.Add(icon);
                tr.Cells.Add(tc);

                table.Rows.Add(tr);
            }
            #endregion


            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.InnerText = String.Concat(syncUsers.Count, " ", eResApp.GetRes(_ePref, 7850));
            container.Controls.Add(title);

            container.Controls.Add(table);
        }

        /// <summary>
        /// Determines whether [is warning icon displayed] for the synchronized user
        /// </summary>
        /// <param name="syncUsers">The synchronized users list</param>
        /// <param name="user">The user.</param>
        /// <param name="warning">The warning.</param>
        /// <returns>
        ///   <c>true</c> if [is warning icon displayed] [the specified synchronize users]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsWarningIconDisplayed(List<eSyncUser> syncUsers, eSyncUser user, out string warning)
        {
            bool display = false;
            warning = string.Empty;
            if (String.IsNullOrEmpty(user.Email))
            {
                display = true;
                warning = eResApp.GetRes(_ePref, 8533);
            }
            else
            {
                if (syncUsers.Exists(u => u.UserId != user.UserId && u.Email.ToLower().Trim() == user.Email.ToLower().Trim()))
                {
                    display = true;
                    warning = eResApp.GetRes(_ePref, 8534);
                }
            }

            return display;
        }
    }
}