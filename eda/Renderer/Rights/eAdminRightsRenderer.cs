using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer pour la gestion des droits
    /// </summary>
    public abstract class eAdminRightsRenderer : eAdminRenderer
    {
        /// <summary>table ou champs sur lequel la liste de droits est filtrée</summary>
        protected int _descid;
        /// <summary>indique qu'on ne génère que le rendu de la liste, sans les entetes, filtres, etc. </summary>
        public Boolean ListOnly = false;
        /// <summary>Filtre sur les Droits de traitements à afficher</summary>
        public HashSet<eTreatmentType> LstTreatmentTypes = new HashSet<eTreatmentType>();
        /// <summary>Objet Permission associé </summary>
        public ePermission SelectedPermission = null;
        /// <summary>Colonne "Vu Depuis"</summary>
        public int From = 0;
        /// <summary>Rubrique concernée</summary>
        public int Field = 0;
        /// <summary>Fonction affichée </summary>
        public String Function = "";
        /// <summary>
        /// Evenement click sur Export
        /// </summary>
        public EventHandler ExportHandler { get; set; }

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId">DescId de la table ou de la rubrique</param>
        protected eAdminRightsRenderer(ePref pref, Int32 nDescId)
        {
            Pref = pref;
            if (nDescId == -1 || nDescId % 100 == 0 || nDescId == (int)TableType.DOUBLONS)
            {
                _descid = nDescId;
                if (nDescId == (int)TableType.DOUBLONS)
                    _tab = (int)TableType.DOUBLONS;
                else
                    _tab = eLibTools.GetTabFromDescId(nDescId);
            }
            else
            {
                _descid = eLibTools.GetTabFromDescId(nDescId);
                Field = nDescId;
            }
        }

        /// <summary>
        /// Construction du rendu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.ID = "rightsAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");

            try
            {
                if (!ListOnly)
                {
                    CreateHiddenInfos();
                    CreateHeaderFilters();
                    CreateTableFilters();
                    CreateActionsButtons();
                }


                CreateListRight();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {

            }

            return base.Build();
        }

        #region HIDDEN INFOS
        void CreateHiddenInfos()
        {
            _divHidden = new HtmlGenericControl("div");
            _divHidden.ID = "rightsHiddenInfos";
            _divHidden.Style.Add("visibility", "hidden");
            _divHidden.Style.Add("display", "none");
            _pgContainer.Controls.Add(_divHidden);

            HtmlInputHidden inputTab = new HtmlInputHidden();
            _divHidden.Controls.Add(inputTab);
            inputTab.ID = "hidTab";
            inputTab.Value = _tab.ToString();

            HtmlInputHidden inputFct = new HtmlInputHidden();
            _divHidden.Controls.Add(inputFct);
            inputFct.ID = "hidFct";
            inputFct.Value = Function;
        }
        #endregion

        #region HEADER FILTERS

        /// <summary>
        /// Ajoute des entrées "spécial" dans la drop down des onglet
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void AddSpecialTab(DropDownList ddl)
        {
            String sSep = "--------------------";
            //Séparateur
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //Droits Globaux
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 7611), "-1"));

            //Tous les onglets
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 435), "0"));
        }


        /// <summary>
        /// Ajoute les entrées spécial de "rubrique"
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void AddSpecialField(DropDownList ddl)
        {
            //Séparateur
            String sSep = "--------------------";
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //AUCUN
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), "-1"));
            //TOUS
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 435), "0"));
            if (Field > 0)
                ddl.SelectedValue = Field.ToString();
        }

        /// <summary>Génération des filtres "Onglet" et "Type"</summary>
        protected virtual void CreateHeaderFilters()
        {
            Panel panel = new Panel();
            panel.ID = "headerFilters";
            _pgContainer.Controls.Add(panel);

            #region  Onglets
            Panel field = new Panel();
            field.CssClass = "field";
            field.ID = "fltTab";
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 264), " :");

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlListTabs";

            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");

            FillTabList(ddl);
            AddSpecialTab(ddl);
            SelectTabItem(ddl);


            field.Controls.Add(label);
            field.Controls.Add(ddl);

            panel.Controls.Add(field);

            #endregion

            #region Vu depuis 

            // Depuis
            field = new Panel();
            field.CssClass = "field";
            field.ID = "fltFrom";
            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 7613), " :");
            ddl = new DropDownList();
            ddl.ID = "ddlListFrom";
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");
            field.Controls.Add(label);
            field.Controls.Add(ddl);
            panel.Controls.Add(field);

            FillTabFromList(ddl);

            String sSep = "--------------------";
            //Séparateur
            ListItem li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");


            //AUCUN
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), "-1"));
            //TOUS
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 435), "0"));

            if (From > 0)
                ddl.SelectedValue = From.ToString();


            // on remplace adresse par société
            li = ddl.Items.FindByValue(((int)TableType.ADR).ToString());

            if (li != null)
            {
                Int32 iPmDescId = (int)TableType.PM;
                string sPMDescId = iPmDescId.ToString();
                eRes res = new eRes(Pref, sPMDescId);
                li.Value = sPMDescId;
                li.Text = res.GetRes(iPmDescId);
            }

            #endregion

            #region Rubrique

            field = new Panel();
            field.CssClass = "field";
            field.ID = "fltField";
            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 222), " :");
            ddl = new DropDownList();
            ddl.ID = "ddlListFields";
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");
            field.Controls.Add(label);
            field.Controls.Add(ddl);
            panel.Controls.Add(field);

            FillFieldList(ddl);
            AddSpecialField(ddl);


            #endregion

            #region Type
            // Type
            field = new Panel();
            field.CssClass = "field";
            field.ID = "fltType";
            label = new HtmlGenericControl("label");
            label.InnerText = "Type :";


            ddl = new DropDownList();
            ddl.ID = "ddlListTypes";
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");

            field.Controls.Add(label);
            field.Controls.Add(ddl);

            FillTypeList(ddl);

            //Séparateur
            li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //Ajout tous
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 22), UserLevel.LEV_USR_ADMIN.GetHashCode().ToString()));

            panel.Controls.Add(field);

            #endregion

            #region Fonctions

            field = new Panel();
            field.CssClass = "field";
            field.ID = "fltFct";
            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 607), " :");
            ddl = new DropDownList();
            ddl.ID = "ddlListFct";
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");
            field.Controls.Add(label);
            field.Controls.Add(ddl);

            FillFonctionList(ddl);

            //Séparateur
            li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);

            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //TOUS
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 435), ""));

            panel.Controls.Add(field);

            #endregion


        }

        /// <summary>
        /// Remplit la la drop liste avec la liste des tables
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void FillTabList(DropDownList ddl)
        {
            Dictionary<int, String> tabs = eSqlDesc.LoadTabs(Pref, GetFilteredTabs().ToArray());


            ddl.DataSource = tabs;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();
        }




        /// <summary>
        /// Selection de la table d'ou vient
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void SelectTabItem(DropDownList ddl)
        {
            ddl.SelectedValue = _descid.ToString();
        }

        /// <summary>
        /// On enlève les tables systèmes, notifs, widgets, grille, homepages ...
        /// </summary>
        private HashSet<int> GetFilteredTabs()
        {

            HashSet<int> res = new HashSet<int>();




            foreach (TableType tType in Enum.GetValues(typeof(TableType)))
            {
                if (
                        (int)tType > (int)TableType.HISTO
                    && !ePermission.AllowedSystemTabPermission.Contains((int)tType)
                    && tType != TableType.USER
                    )
                {

                    res.Add((int)tType);
                }
            }

            return res;
        }

        /// <summary>
        /// Remplit la la drop liste avec la liste des tables parentes
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void FillTabFromList(DropDownList ddl)
        {
            if (_descid > 0)
            {
                Dictionary<int, string> dicParents = eAdminTools.GetListParentTabs(Pref, _descid);
                if (dicParents.Count > 0)
                {
                    ddl.DataSource = dicParents;
                    ddl.DataValueField = "Key";
                    ddl.DataTextField = "Value";
                    ddl.DataBind();

                }
            }
        }

        /// <summary>
        /// Remplit la la drop liste avec la liste avec des champs de la table 
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void FillFieldList(DropDownList ddl)
        {
            Dictionary<int, string> dicfields;
            Dictionary<int, int> dicformat;
            if (_descid > 0)
            {
                eSqlDesc.LoadTabFields(Pref, eLibTools.GetTabFromDescId(_descid), out dicfields, out dicformat);

                if (dicfields.Count > 0)
                {
                    ddl.DataSource = dicfields;
                    ddl.DataTextField = "Value";
                    ddl.DataValueField = "Key";
                    ddl.DataBind();

                }
            }
        }

        /// <summary>
        /// Remplit la la drop liste avec la liste des différent types de droits
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void FillTypeList(DropDownList ddl)
        {
            //
            List<ListItem> lst = new List<ListItem>();
            foreach (eTreatmentType tt in Enum.GetValues(typeof(eTreatmentType)))
            {
                if (!CanManageTreatment(tt))
                    continue;

                ListItem item = new ListItem(eAdminDescTreatmentRight.GetResType(tt, Pref), tt.GetHashCode().ToString());
                if (LstTreatmentTypes.Count == 1 && LstTreatmentTypes.First() == tt)
                    item.Selected = true;
                lst.Add(item);
            }

            //tri alpha
            lst.Sort((a, b) => a.Text.CompareTo(b.Text));
            ddl.Items.AddRange(lst.ToArray());
        }

        /// <summary>
        /// Savoir si on peut paramètre un type de droit
        /// </summary>
        /// <param name="tt"></param>
        /// <returns></returns>
        protected virtual bool CanManageTreatment(eTreatmentType tt)
        {
            if (tt == eTreatmentType.IMPORT)
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Import);
            return true;
        }

        /// <summary>
        /// Remplit la la drop liste avec la liste des différentes fonctions
        /// </summary>
        /// <param name="ddl"></param>
        protected virtual void FillFonctionList(DropDownList ddl)
        {

        }

        /// <summary>
        /// Savoir si on peut paramètrer un droit d'une rubrique/type de traitement
        /// </summary>
        /// <param name="descid"></param>
        /// <param name="tt"></param>
        /// <returns></returns>
        private bool CanManageFieldRights(int descid, eTreatmentType tt)
        {
            if (descid == (int)InteractionField.ConsentObtainedBy && tt == eTreatmentType.CATALOG)
                return false;

            return true;
        }

        #endregion

        #region USER AND GROUP FILTERS
        /// <summary>Génération des filtres du tableau de droits</summary>
        protected virtual void CreateTableFilters()
        {
            Panel panel = new Panel();
            panel.ID = "rightsTabFilters";
            panel.CssClass = "rightsButton";

            HtmlGenericControl iconWrapper = new HtmlGenericControl("div");
            iconWrapper.ID = "iconWrapper";
            HtmlGenericControl spanIcon = new HtmlGenericControl("span");
            spanIcon.Attributes.Add("class", "icon-bt-actions-left");
            iconWrapper.Controls.Add(spanIcon);

            Panel content = new Panel();
            content.ID = "content";
            HtmlGenericControl label = new HtmlGenericControl("label");

            label.InnerText = eResApp.GetRes(Pref, 6868);


            content.Controls.Add(label);

            Panel selects = new Panel();

            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlLevels";
            selects.Controls.Add(ddl);
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 435), ""));
            for (int i = 1; i <= 5; i++)
            {
                ddl.Items.Add(new ListItem(i.ToString()));
            }
            // TODO SUPERADMIN ?
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(Pref, UserLevel.LEV_USR_ADMIN), UserLevel.LEV_USR_ADMIN.GetHashCode().ToString()));
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");


            ddl = new DropDownList();
            ddl.ID = "ddlGroupsAndUsers";
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 435), ""));
            selects.Controls.Add(ddl);
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {

                dal.OpenDatabase();
                eUser usrObj = new eUser(dal, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, new List<string>());
                StringBuilder sbError = new StringBuilder();
                List<eUser.UserListItem> usrList = usrObj.GetUserList(true, false, "", sbError);

                foreach (eUser.UserListItem userItem in usrList)
                {
                    ListItem li = new ListItem(userItem.Libelle, userItem.ItemCode);
                    ddl.Items.Add(li);
                    if (userItem.Type == eUser.UserListItem.ItemType.GROUP)
                        li.Attributes.Add("class", "grp");
                }
                ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");

            }
            catch (Exception e)
            {
                //TODO gestion d'erreur 
            }
            finally
            {
                dal.CloseDatabase();
            }
            content.Controls.Add(selects);

            panel.Controls.Add(iconWrapper);
            panel.Controls.Add(content);

            _pgContainer.Controls.Add(panel);
        }

        #endregion

        #region ACTIONS BUTTONS
        /// <summary>Génération du bouton Traitements</summary>
        protected virtual void CreateActionsButtons()
        {
            eAdminField button;
            Panel pActions = new Panel();
            pActions.ID = "actionsButtons";

            //button = new eAdminButtonField(eResApp.GetRes(Pref, 7538), "exportButton");
            Panel pLink = new Panel();
            pLink.CssClass = "linkButton";
            pActions.Controls.Add(pLink);
            LinkButton btnExport = new LinkButton();
            btnExport.ID = "exportButton";
            btnExport.Click += ExportHandler;
            //btnExport.OnClientClick = "top.setWait(true)";
            btnExport.Text = eResApp.GetRes(Pref, 7538);
            pLink.Controls.Add(btnExport);

            button = new eAdminButtonField(eResApp.GetRes(Pref, 295), "treatmentsButton", onclick: "nsAdminRights.showTreatmentsRights()");
            button.Generate(pActions);

            _pgContainer.Controls.Add(pActions);
        }

        #endregion

        #region DATA LIST TABLE

        /// <summary>
        /// Création de la liste des droits à afficher
        /// </summary>
        protected virtual void CreateListRight()
        {
            String tidList = String.Empty;

            Panel wrapper = new Panel();
            wrapper.ID = "tableWrapper";

            System.Web.UI.WebControls.Table tblList = new System.Web.UI.WebControls.Table();
            tblList.ID = "tableFilters";
            tblList.Style.Add("table-layout", "fixed");
            tblList.Style.Add("text-overflow", "ellipsis");

            // Entête de la table
            tblList.Rows.Add(BuildTableHeader());

            // Liste des utilisateurs de la base
            List<eUser.UserListItem> usrList = GetUserList();

            // Chargement des traitements
            List<IAdminTreatmentRight> rights = GetTreatmentRights();

            // Tri selon l'implementation de IAdminTreatmentRight. 
            //KHA annulé car pas perf. Tri géré par SQL
            //rights.Sort();

            foreach (IAdminTreatmentRight right in rights)
            {
                if (!CanManageTreatment(right.Type))
                    continue;

                if (!CanManageFieldRights(right.DescID, right.Type))
                    continue;

                TableRow tr = BuildTableRow(right, tblList.Rows.Count % 2 == 0);
                tblList.Rows.Add(tr);

                //Icon
                CreateRowCellIcon(tr, GetIconClassName(right.Perm, usrList));

                //Onglet
                CreateRowCell(tr, right.TabLabel, "tab");

                //Type
                CreateRowCell(tr, right.TypeLabel, "typ");

                //Depuis
                CreateRowCell(tr, right.TabFromLabel, "from");

                //Rubrique
                CreateRowCell(tr, right.FieldLabel, "fld");

                //Fonctions
                CreateRowCell(tr, right.TraitLabel, "fct");

                //Niveau
                CreateRowCellSlider(tr, right);

                // User & Groupe  
                CreateRowCellUserGroup(tr, right.TraitID, right.Perm);
            }

            // Sauvegarde des ids des traitements
            tidList = eLibTools.Join(";", rights, r => r.TraitID.ToString());

            // Champ caché contenant la liste des TID
            HiddenField hidTidList = new HiddenField();
            hidTidList.ID = "hidTidList";
            hidTidList.Value = tidList;

            // Champ caché contenant la fonction sélectionnée
            HiddenField hidFct = new HiddenField();
            hidFct.ID = "hidFctSelected";
            hidFct.Value = Function;

            wrapper.Controls.Add(hidTidList);
            wrapper.Controls.Add(hidFct);
            wrapper.Controls.Add(tblList);

            if (rights.Count() == 0)
            {
                Panel pnEmptyMsg = new Panel();
                pnEmptyMsg.Controls.Add(new LiteralControl(eResApp.GetRes(Pref, 7612)));
                pnEmptyMsg.CssClass = "EmptyMsg";
                wrapper.Controls.Add(pnEmptyMsg);
            }

            _pgContainer.Controls.Add(wrapper);
        }



        /// <summary>
        /// Prépare une dataRow avec des infos système - à redéfinir si besoin
        /// </summary>
        /// <param name="right">eAdminTreatmentRight</param>
        /// <param name="even"></param>
        /// <returns></returns>
        protected virtual TableRow BuildTableRow(IAdminTreatmentRight right, bool even)
        {
            TableRow tr = new TableRow();
            tr.TableSection = TableRowSection.TableBody;
            tr.CssClass = even ? "alternateLine" : "";

            tr.Attributes.Add("tl", ((int)right.TreatLoc).ToString());
            tr.Attributes.Add("tid", right.TraitID.ToString());
            tr.Attributes.Add("did", right.DescID.ToString());
            if (right.Perm != null)
                tr.Attributes.Add("pid", right.Perm.PermId.ToString());

            if (right.TreatLoc == eLibConst.TREATMENTLOCATION.UpdatePermId || right.TreatLoc == eLibConst.TREATMENTLOCATION.ViewPermId)
            {
                tr.CssClass += " updateOrViewPerm";
            }

            return tr;
        }

        /// <summary>
        /// Récupère la cellule de l'icone
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="className"></param>
        private void CreateRowCellIcon(TableRow tr, string className)
        {
            TableCell tcI = new TableCell();
            tr.Cells.Add(tcI);

            if (string.IsNullOrEmpty(className))
                return;

            tcI.CssClass = className;
        }

        /// <summary>
        /// Récupère l'icon de cadenas en fonction du niveau ou l'utilisateur avec permission déjà sélectionnée
        /// </summary>
        /// <param name="perm"></param>
        /// <param name="userList"></param>
        /// <returns></returns>
        private String GetIconClassName(ePermission perm, List<eUser.UserListItem> userList)
        {
            if (SelectedPermission != null)
            {
                if (DoPermMatch(SelectedPermission, perm, userList, Pref))
                    //tcI.Text = "A le droit";
                    return "icon-unlock";
                else
                    //tcI.Text = "N'a pas le droit";
                    return "icon-lock";
            }

            return null;
        }

        /// <summary>
        /// Creation de la cellule de la representation de la permission
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="traitID">Id du traitement</param>
        /// <param name="perm">Permission associée</param>
        private void CreateRowCellUserGroup(TableRow tr, int traitID, ePermission perm)
        {

            string textValue, levelValue;
            eAdminTools.ExportPermValues(this.Pref, perm, out textValue, out levelValue);

            TableCell tcUserV = new TableCell();
            tr.Cells.Add(tcUserV);

            tcUserV.Attributes.Add("title", textValue);
            tcUserV.Attributes.Add("ednvalue", levelValue);
            tcUserV.Attributes.Add("lastvalid", levelValue);

            if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_PRODUCT || perm.PermLevel < (int)UserLevel.LEV_USR_PRODUCT)
            {
                tcUserV.Attributes.Add("onclick", "nsAdminRights.showUsersCat(this);");
                tcUserV.Attributes.Add("class", "colCatUsers");
            }
            else
            {
                tcUserV.Attributes.Add("disabled", "1");
                tcUserV.Attributes.Add("style", "cursor:not-allowed");
            }



            tcUserV.Attributes.Add("tid", traitID.ToString());
            tcUserV.Attributes.Add("perm", "user");

            if (traitID > 0)
                tcUserV.ID = String.Concat("usersTrait", traitID);

            tcUserV.Text = textValue;
        }



        /// <summary>
        /// Retourne une liste des userlistitem
        /// </summary>
        /// <returns></returns>
        private List<eUser.UserListItem> GetUserList()
        {
            List<eUser.UserListItem> usrList = new List<eUser.UserListItem>();
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal.OpenDatabase();
                eUser usrObj = new eUser(dal, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, new List<string>());
                StringBuilder sbError = new StringBuilder();
                usrList = usrObj.GetUserList(true, false, "", sbError);
            }
            catch (Exception e)
            {
            }
            finally
            {
                dal.CloseDatabase();
            }

            return usrList;

        }

        /// <summary>
        /// Récupère la valeur du niveau de la permission
        /// </summary>
        /// <param name="perm"></param>
        /// <returns></returns>
        private string GetLevelValue(ePermission perm)
        {
            String levelValue = "0";
            if (perm != null)
            {
                //inp.Attributes.Add("value", t.Perm.PermLevel.ToString());
                if (perm.PermLevel == (int)UserLevel.LEV_USR_ADMIN)
                    levelValue = "6";
                else if (perm.PermLevel == (int)UserLevel.LEV_USR_SUPERADMIN)
                    levelValue = "8";
                else if (perm.PermLevel == (int)UserLevel.LEV_USR_PRODUCT)
                    levelValue = "9";
                else if (perm.PermLevel == 0)
                    levelValue = "7"; // Ne pas tenir compte du niveau                                  
                else
                    levelValue = perm.PermLevel.ToString();
            }

            return levelValue;
        }

        /// <summary>
        /// Créée un slider de permission dans une cellule
        /// </summary>
        /// <param name="tr"></param>
        /// <param name="right"></param>
        private void CreateRowCellSlider(TableRow tr, IAdminTreatmentRight right)
        {

            int traitID = right.TraitID;
            string levelValue = GetLevelValue(right.Perm);
            int nUsrLvl = _ePref.User.UserLevel;
            int nPermLvl = right.Perm.PermLevel;

            bool bDisabled = nUsrLvl < nPermLvl;


            TableCell tcLevelV = new TableCell();
            tr.Cells.Add(tcLevelV);
            HtmlGenericControl label = new HtmlGenericControl("div");
            label.Attributes.Add("class", "divRanges");

            string eltID = (traitID > 0) ? String.Concat("levelTrait", traitID) : "";
            HtmlGenericControl slider;

            if (traitID == 101018)
            {
                slider = eAdminFieldBuilder.BuildRangeSlider(tcLevelV, eltID, 6, 7, 1, eLibTools.GetNum(levelValue));
            }
            else if (nUsrLvl == (int)UserLevel.LEV_USR_SUPERADMIN && nPermLvl < (int)UserLevel.LEV_USR_PRODUCT)
                slider = eAdminFieldBuilder.BuildRangeSlider(tcLevelV, eltID, 1, 8, 1, eLibTools.GetNum(levelValue));
            else if (nUsrLvl > (int)UserLevel.LEV_USR_SUPERADMIN || nPermLvl == (int)UserLevel.LEV_USR_PRODUCT)
                slider = eAdminFieldBuilder.BuildRangeSlider(tcLevelV, eltID, 1, 9, 1, eLibTools.GetNum(levelValue));
            else
                slider = eAdminFieldBuilder.BuildRangeSlider(tcLevelV, eltID, 1, 7, 1, eLibTools.GetNum(levelValue));


            if (bDisabled)
                slider.Attributes.Add("disabled", "1");


            slider.Attributes.Add("perm", "level");
            slider.Attributes.Add("lastvalid", levelValue);
            slider.Attributes.Add("class", "nouislider rangeSlider");

            eltID = (traitID > 0) ? String.Concat("output", traitID) : "";
            HtmlGenericControl output = new HtmlGenericControl("output");
            output.Attributes.Add("for", String.Concat("levelTrait", traitID));
            output.ID = eltID;
            output.InnerText = eAdminTools.GetUserLevelLabel(Pref, levelValue);

            label.Controls.Add(slider);
            label.Controls.Add(output);
            tcLevelV.Controls.Add(label);
        }

        /// <summary>
        /// Creation de l'entete de la table
        /// </summary>
        /// <returns></returns>
        private TableHeaderRow BuildTableHeader()
        {
            TableHeaderRow trHead = new TableHeaderRow();
            trHead.TableSection = TableRowSection.TableHeader;

            //Icone           
            CreateHeaderCell(trHead, null, "2%"); // sans label

            //Onglet  
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 264), "9%");

            //Types
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 8659), "12%");

            //Vu depuis
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 7613), "16%");

            //Rubrique
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 222), "24%");

            //Fonctions
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 607), "16%");

            //Niveau
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 7416), "13%");

            // User & Groupe
            CreateHeaderCell(trHead, eResApp.GetRes(Pref, 7556), "18%");

            return trHead;
        }

        /// <summary>
        /// Création d'une cellule d'entete de la table
        /// </summary>
        /// <param name="trHead"></param>
        /// <param name="labelText"></param>
        /// <param name="widthValue"></param>
        private void CreateHeaderCell(TableHeaderRow trHead, string labelText, string widthValue)
        {
            TableHeaderCell tcTab = new TableHeaderCell();
            trHead.Cells.Add(tcTab);
            tcTab.Style.Add("width", widthValue);

            if (string.IsNullOrEmpty(labelText))
                return;

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = labelText;
            tcTab.Controls.Add(label);
        }

        /// <summary>
        /// Création d'une cellule de la table
        /// </summary>
        /// <param name="trRow">ligne</param>
        /// <param name="valueText"> label</param>
        /// <param name="col">type de colonne</param>
        protected virtual void CreateRowCell(TableRow trRow, string valueText, string col)
        {
            TableCell tcTab = new TableCell();
            trRow.Cells.Add(tcTab);
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.Attributes.Add("col", col);
            label.InnerText = valueText;
            tcTab.Controls.Add(label);
        }

        /// <summary>
        /// Retourne la liste des permissions
        /// </summary>
        /// <returns></returns>
        protected virtual List<IAdminTreatmentRight> GetTreatmentRights()
        {
            eAdminDescTreatmentRightCollection oRightsList = new eAdminDescTreatmentRightCollection(Pref);
            oRightsList.Tab = _descid;
            oRightsList.TreatTypes = LstTreatmentTypes;
            oRightsList.From = From;
            oRightsList.Field = Field;
            oRightsList.Function = Function;
            oRightsList.LoadTreamentsList();

            return oRightsList.RightsList;
        }



        #endregion

        #region HELPERS

        /// <summary>
        /// Compare une permission à une autre, pour savoir une la permission définie donne les droits à une autre
        /// </summary>
        /// <param name="permSearch">Permission définie</param>
        /// <param name="permSearchIn">Permission dans laquelle on fait la recherche</param>
        /// <returns></returns>
        public static Boolean DoPermMatch(ePermission permSearch, ePermission permSearchIn, List<eUser.UserListItem> usrList, ePref pref)
        {
            if (permSearchIn.PermMode == ePermission.PermissionMode.MODE_NONE)
                return true;

            if (usrList == null)
                return false;

            //comparaison du niveau
            Boolean bLevel = true;
            if (permSearch.PermLevel >= 0)
                bLevel = permSearch.PermLevel >= permSearchIn.PermLevel;

            //comparaison des deux listes d'utilisateurs
            // ce test est assez gourmand, on ne le fait que si le permMode le sollicite
            Boolean bUser = false;

            // Si aucun utilisateur n'est autorisé à faire l'action :
            Boolean bNoUserAllowed = (String.IsNullOrEmpty(permSearchIn.PermUser) || permSearchIn.PermUser == "0")
                &&
                (permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_ONLY || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL);

            if (bNoUserAllowed)
                return false;

            if (String.IsNullOrEmpty(permSearch.PermUser) || String.IsNullOrEmpty(permSearchIn.PermUser))
            {
                bUser = false;
            }
            else if (permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL
                || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL
                || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_ONLY
                )
            {
                String[] sUserSearch = permSearch.PermUser.Split(';');
                String[] sUserSearchIn = permSearchIn.PermUser.Split(';');
                HashSet<String> hsUserSearch = new HashSet<String>(), hsUserSearchIn = new HashSet<String>(), hsGpSearch = new HashSet<String>(), hsGpSearchIn = new HashSet<String>();
                Regex reGroup = new Regex("^G[0-9]+$");

                foreach (String s in sUserSearch)
                {
                    if (reGroup.IsMatch(s))
                        hsGpSearch.Add(s);
                    else
                        hsUserSearch.Add(s);
                }

                eDataFillerGeneric dtfUsers;

                #region recherche des users ayant un niveau suffisant

                if (permSearchIn.PermMode == ePermission.PermissionMode.MODE_LEVEL_ONLY
                    || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL
                    || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL
                    )
                {
                    if (!bLevel && (hsUserSearch.Count > 0 || hsGpSearch.Count > 0))
                    {
                        dtfUsers = new eDataFillerGeneric(pref, TableType.USER.GetHashCode(), ViewQuery.CUSTOM);
                        dtfUsers.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                        {
                            eq.SetListCol = String.Concat(UserField.LOGIN.GetHashCode().ToString());

                            List<WhereCustom> liWcSearch = new List<WhereCustom>(), liWcTt = new List<WhereCustom>();
                            if (hsUserSearch.Count > 0)
                                liWcSearch.Add(new WhereCustom("MAINID", Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsUserSearch), EudoQuery.InterOperator.OP_OR));
                            if (hsGpSearch.Count > 0)
                                liWcSearch.Add(new WhereCustom(UserField.GroupId.GetHashCode().ToString(), Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsGpSearch), EudoQuery.InterOperator.OP_OR));

                            if (liWcSearch.Count > 0)
                                liWcTt.Add(new WhereCustom(liWcSearch, InterOperator.OP_AND));

                            liWcTt.Add(new WhereCustom(((int)UserField.LEVEL).ToString(), Operator.OP_GREATER_OR_EQUAL, permSearchIn.PermLevel.ToString(), InterOperator.OP_AND));

                            eq.AddCustomFilter(new WhereCustom(liWcTt));
                        };
                        dtfUsers.Generate();
                        if (dtfUsers.ErrorMsg.Length == 0 && dtfUsers.ListRecords.Count > 0)
                            bLevel = true;


                    }
                }
                #endregion


                foreach (String s in sUserSearchIn)
                {
                    if (reGroup.IsMatch(s))
                        hsGpSearchIn.Add(s.Replace("G", ""));
                    else
                        hsUserSearchIn.Add(s);
                }

                #region recherche des users en commun dans les deux listes

                if (permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_ONLY
                    || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL
                    || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL
                    )
                {

                    dtfUsers = new eDataFillerGeneric(pref, TableType.USER.GetHashCode(), ViewQuery.CUSTOM);
                    dtfUsers.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                    {
                        eq.SetListCol = String.Concat(UserField.LOGIN.GetHashCode().ToString());

                        List<WhereCustom> liWcSearch = new List<WhereCustom>(), liWcSearchIn = new List<WhereCustom>(), liWcTt = new List<WhereCustom>();
                        if (hsUserSearch.Count > 0)
                            liWcSearch.Add(new WhereCustom("MAINID", Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsUserSearch), EudoQuery.InterOperator.OP_OR));
                        if (hsGpSearch.Count > 0)
                            liWcSearch.Add(new WhereCustom(UserField.GroupId.GetHashCode().ToString(), Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsGpSearch), EudoQuery.InterOperator.OP_OR));
                        if (liWcSearch.Count > 0)
                            liWcTt.Add(new WhereCustom(liWcSearch, InterOperator.OP_AND));

                        if (hsUserSearchIn.Count > 0)
                            liWcSearchIn.Add(new WhereCustom("MAINID", Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsUserSearchIn), EudoQuery.InterOperator.OP_OR));
                        if (hsGpSearchIn.Count > 0)
                            liWcSearchIn.Add(new WhereCustom(UserField.GroupId.GetHashCode().ToString(), Operator.OP_IN_LIST, eLibTools.Join<String>(";", hsGpSearchIn), EudoQuery.InterOperator.OP_OR));
                        if (liWcSearchIn.Count > 0)
                            liWcTt.Add(new WhereCustom(liWcSearchIn, InterOperator.OP_AND));

                        eq.AddCustomFilter(new WhereCustom(liWcTt));
                    };
                    dtfUsers.Generate();
                    if (dtfUsers.ErrorMsg.Length == 0 && dtfUsers.ListRecords.Count > 0)
                        bUser = true;

                }

                #endregion



                //if (
                //        usrList.Find(
                //            delegate (eUser.UserListItem usrItem)
                //            {
                //                return (hsUserIn.Contains(usrItem.ItemCode) || hsGpIn.Contains(usrItem.GroupId))
                //                        && (hsUserOut.Contains(usrItem.ItemCode) || hsGpOut.Contains(usrItem.GroupId));
                //            }
                //        ) != null
                //    )
                //    bUser = true;


            }

            switch (permSearchIn.PermMode)
            {
                case ePermission.PermissionMode.MODE_NONE:
                    return true;
                    break;
                case ePermission.PermissionMode.MODE_LEVEL_ONLY:
                    return bLevel;
                    break;
                case ePermission.PermissionMode.MODE_USER_ONLY:
                    return bUser;
                    break;
                case ePermission.PermissionMode.MODE_USER_OR_LEVEL:
                    return bUser || bLevel;
                    break;
                case ePermission.PermissionMode.MODE_USER_AND_LEVEL:
                    return bUser && bLevel;
                    break;
                default:
                    break;
            }

            return false;
        }

        #endregion
    }
}