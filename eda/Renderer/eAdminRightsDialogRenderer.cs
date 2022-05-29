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
namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Render pour la gestion des droits de traitements
    /// </summary>
    public class eAdminRightsDialogRenderer : eAdminRenderer
    {
        protected int _descid;
        public Boolean ListOnly = false;
        public HashSet<eTreatmentType> LstTreatmentTypes = new HashSet<eTreatmentType>();
        public ePermission SelectedPermission = null;
        public int From = 0;
        public int Field = 0;
        public String Function = "";
        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nDescId"></param>
        protected eAdminRightsDialogRenderer(ePref pref, Int32 nDescId)
        {
            Pref = pref;
            if (nDescId == -1)
            {
                _descid = nDescId;
            }
            else if (nDescId % 100 == 0)
            {
                _descid = nDescId;
            }
            else
            {
                _descid = eLibTools.GetTabFromDescId(nDescId);
                Field = nDescId;
            }
        }

        public static eAdminRightsDialogRenderer CreateAdminRightsDialogRenderer(ePref pref, Int32 nDescId)
        {
            return new eAdminRightsDialogRenderer(pref, nDescId);
        }


        protected override bool Build()
        {
            _pgContainer.ID = "rightsAdminModalContent";
            _pgContainer.Attributes.Add("class", "adminModalContent");


            try
            {
                if (!ListOnly)
                {
                    CreateHeaderFilters();
                    CreateTableFilters();
                    CreateTreatmentsButton();
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

            TableHeaderRow trHead = new TableHeaderRow();
            trHead.TableSection = TableRowSection.TableHeader;
            tblList.Rows.Add(trHead);

            //Icone
            TableHeaderCell tcH = new TableHeaderCell();
            trHead.Cells.Add(tcH);
            tcH.Style.Add("width", "2%");

            //Onglet
            TableHeaderCell tcTab = new TableHeaderCell();
            trHead.Cells.Add(tcTab);
            tcTab.Style.Add("width", "9%");
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 264); // Onglet
            tcTab.Controls.Add(label);

            //Type
            TableHeaderCell tcType = new TableHeaderCell();
            trHead.Cells.Add(tcType);
            tcType.Style.Add("width", "12%");
            label = new HtmlGenericControl("label");
            label.InnerText = "Types"; // TODO RES
            tcType.Controls.Add(label);

            //Depuis
            TableHeaderCell tcFrom = new TableHeaderCell();
            trHead.Cells.Add(tcFrom);
            tcFrom.Style.Add("width", "6%");
            label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 7613));
            tcFrom.Controls.Add(label);

            //Depuis
            TableHeaderCell tcField = new TableHeaderCell();
            trHead.Cells.Add(tcField);
            tcField.Style.Add("width", "24%");
            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 222);
            tcField.Controls.Add(label);

            //Fonctions
            TableHeaderCell tcFct = new TableHeaderCell();
            trHead.Cells.Add(tcFct);
            tcFct.Style.Add("width", "16%");
            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 607); // Fonction
            tcFct.Controls.Add(label);

            //Niveau
            TableHeaderCell tcLevel = new TableHeaderCell();
            trHead.Cells.Add(tcLevel);
            tcLevel.Style.Add("width", "13%");
            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 7416); //"Niveaux"
            tcLevel.Controls.Add(label);

            // User & Groupe
            TableHeaderCell tcUser = new TableHeaderCell();
            trHead.Cells.Add(tcUser);
            tcUser.Style.Add("width", "18%");
            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(_ePref, 7556);// "Utilisateurs et groupes"; 
            tcUser.Controls.Add(label);

            eAdminTreatmentRightList oRightsList = new eAdminTreatmentRightList(Pref);
            oRightsList.Tab = _descid;
            oRightsList.TreatTypes = LstTreatmentTypes;
            oRightsList.From = From;
            oRightsList.Field = Field;
            oRightsList.Function = Function;
            oRightsList.LoadTreamentsList();
            IEnumerable<eAdminTreatmentRight> k;

            if (_descid > 0)
                k = from elem in oRightsList.RightsList orderby eAdminTreatmentRight.GetResType(elem.Type, Pref), elem.TraitLabel select elem;
            else
                k = oRightsList.RightsList;

            List<eUser.UserListItem> usrList = null;
            eudoDAL dal = eLibTools.GetEudoDAL(Pref);
            try
            {
                dal.OpenDatabase();
                eUser usrObj = new eUser(dal, Pref.User, eUser.ListMode.USERS_AND_GROUPS, Pref.GroupMode, new List<string>());
                StringBuilder sbError = new StringBuilder();
                usrList = usrObj.GetUserList(true, false, "", ref sbError);
            }
            catch (Exception e)
            {
            }
            finally
            {
                dal.CloseDatabase();
            }

            // Compteur pour l'alternance des lignes
            int count = 0;

            foreach (eAdminTreatmentRight t in k)
            {
                TableRow tr = new TableRow();
                tr.TableSection = TableRowSection.TableBody;
                tr.CssClass = (count % 2 != 0) ? "alternateLine" : "";

                tr.Attributes.Add("tl", ((int)t.TreatLoc).ToString());
                tr.Attributes.Add("tid", t.TraitID.ToString());
                tr.Attributes.Add("did", t.DescID.ToString());
                if (t.Perm != null)
                {
                    tr.Attributes.Add("pid", t.Perm.PermId.ToString());
                }
                tblList.Rows.Add(tr);

                //Icon
                TableCell tcI = new TableCell();
                tr.Cells.Add(tcI);
                if (SelectedPermission != null)
                    if (DoPermMatch(SelectedPermission, t.Perm, usrList, _ePref))
                    {
                        //tcI.Text = "A le droit";
                        tcI.CssClass = "icon-unlock";
                    }
                    else
                    {
                        //tcI.Text = "N'a pas le droit";
                        tcI.CssClass = "icon-lock";
                    }



                //Onglet
                TableCell tcTabV = new TableCell();
                tr.Cells.Add(tcTabV);
                label = new HtmlGenericControl("label");
                label.InnerHtml = String.Concat("<span class='icon-edn-info'></span>", t.TabLabel);
                label.Attributes.Add("col", "tab");
                tcTabV.Controls.Add(label);

                //Type
                TableCell tcTypeV = new TableCell();
                tr.Cells.Add(tcTypeV);
                label = new HtmlGenericControl("label");
                label.InnerText = eAdminTreatmentRight.GetResType(t.Type, Pref);
                label.Attributes.Add("col", "typ");
                tcTypeV.Controls.Add(label);

                //Depuis
                TableCell tcFromV = new TableCell();
                tr.Cells.Add(tcFromV);
                label = new HtmlGenericControl("label");
                label.InnerText = t.TabFromLabel;
                label.Attributes.Add("col", "from");
                tcFromV.Controls.Add(label);

                //Rubrique
                TableCell tcFieldV = new TableCell();
                tr.Cells.Add(tcFieldV);
                label = new HtmlGenericControl("label");
                label.InnerText = t.FieldLabel;
                label.Attributes.Add("col", "fld");
                tcFieldV.Controls.Add(label);


                //Fonctions
                TableCell tcFctV = new TableCell();
                tr.Cells.Add(tcFctV);
                label = new HtmlGenericControl("label");
                label.InnerText = t.TraitLabel;
                label.Attributes.Add("col", "fct");
                tcFctV.Controls.Add(label);

                //Niveau
                TableCell tcLevelV = new TableCell();
                tr.Cells.Add(tcLevelV);
                label = new HtmlGenericControl("div");
                label.Attributes.Add("class", "divRanges");

                //<td><input type="range" class="rangeLevels"/></td>
                //HtmlGenericControl inp = new HtmlGenericControl("input");
                //inp.ID = String.Concat("levelTrait", t.TraitID);
                //inp.Attributes.Add("tid", t.TraitID.ToString());
                //inp.Attributes.Add("type", "range");
                //inp.Attributes.Add("class", "rangeLevels");
                //inp.Attributes.Add("oninput", "top.nsAdminRights.updateTreatment(document, this);");

                String levelValue = "0";
                if (t.Perm != null)
                {
                    //inp.Attributes.Add("value", t.Perm.PermLevel.ToString());
                    if (t.Perm.PermLevel >= eLibConst.USER_LEVEL.LEV_USR_ADMIN.GetHashCode())
                    {
                        levelValue = "6";
                    }
                    else if (t.Perm.PermLevel == 0)
                    {
                        levelValue = "7"; // Ne pas tenir compte du niveau
                    }
                    //else if (t.Perm.PermLevel == 0)
                    //{
                    //    if (t.Perm.PermMode == ePermission.PermissionMode.MODE_LEVEL_ONLY 
                    //        || t.Perm.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL 
                    //        || t.Perm.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL)
                    //    {
                    //        levelValue = "7"; // Aucun niveau
                    //    }
                    //    else
                    //    {
                    //        levelValue = "0"; // Tous niveaux
                    //    }
                    //}
                    else
                        levelValue = t.Perm.PermLevel.ToString();
                }

                //inp.Attributes.Add("value", levelValue);
                //inp.Attributes.Add("lastvalid", levelValue);
                //inp.Attributes.Add("min", "1");
                //inp.Attributes.Add("max", "7");
                //inp.Attributes.Add("step", "1");
                //inp.Attributes.Add("perm", "level");

                HtmlGenericControl slider = eAdminFieldBuilder.BuildRangeSlider(tcLevelV, String.Concat("levelTrait", t.TraitID), 1, 7, 1, eLibTools.GetNum(levelValue));
                slider.Attributes.Add("perm", "level");
                slider.Attributes.Add("lastvalid", levelValue);
                slider.Attributes.Add("class", "nouislider rangeSlider");

                HtmlGenericControl output = new HtmlGenericControl("output");
                output.Attributes.Add("for", String.Concat("levelTrait", t.TraitID));
                output.ID = String.Concat("output", t.TraitID);
                output.InnerText = eAdminTools.GetUserLevelLabel(Pref, levelValue);

                label.Controls.Add(slider);
                label.Controls.Add(output);


                tcLevelV.Controls.Add(label);

                // User & Groupe
                TableCell tcUserV = new TableCell();
                tr.Cells.Add(tcUserV);
                TextBox textbox = new TextBox();
                String value = String.Empty;

                if (t.Perm.PermUser == "null")
                    t.Perm.PermUser = "";

                if (t.Perm.PermMode != ePermission.PermissionMode.MODE_NONE && t.Perm.PermMode != ePermission.PermissionMode.MODE_LEVEL_ONLY)
                {

                    if (t.Perm.PermUser.Length > 0)
                        tcUserV.Text = t.Perm.PermUserLabel;
                    else
                    {
                        tcUserV.Text = eResApp.GetRes(Pref, 513); // aucun user
                    }

                }
                else if (t.Perm.PermMode != ePermission.PermissionMode.MODE_LEVEL_ONLY)
                {
                    tcUserV.Text = eResApp.GetRes(Pref, 6869);
                }
                else
                {
                    tcUserV.Text = eResApp.GetRes(Pref, 6869);
                }

                // Dans le cas où aucun utilisateur n'est sélectionné

                if (t.Perm.PermUser.Length <= 0)
                {
                    // Si aucun niveau
                    if ((t.Perm.PermMode == ePermission.PermissionMode.MODE_USER_ONLY || t.Perm.PermMode == ePermission.PermissionMode.MODE_USER_OR_LEVEL))
                    {
                        tcUserV.Text = eResApp.GetRes(Pref, 513); // aucun user
                    }
                    else
                    {
                        tcUserV.Text = eResApp.GetRes(Pref, 6869); // Tous les utilisateurs
                        value = "0";
                    }
                }
                else
                {
                    value = t.Perm.PermUser;
                    if (value == "0")
                        tcUserV.Text = eResApp.GetRes(Pref, 6869); // Tous les utilisateurs
                }
                tcUserV.Attributes.Add("title", textbox.Text);
                tcUserV.Attributes.Add("ednvalue", value);
                tcUserV.Attributes.Add("lastvalid", value);
                tcUserV.Attributes.Add("onclick", "top.nsAdmin.showUsersCat(this);");
                tcUserV.Attributes.Add("class", "colCatUsers");
                tcUserV.Attributes.Add("tid", t.TraitID.ToString());
                tcUserV.Attributes.Add("perm", "user");
                tcUserV.ID = String.Concat("usersTrait", t.TraitID);

                //tcUserV.Controls.Add(textbox);


                tidList = String.Concat(!String.IsNullOrEmpty(tidList) ? tidList + ";" : "", t.TraitID);

                count++;
            }

            // Champ caché contenant la liste des TID
            HiddenField hidTidList = new HiddenField();
            hidTidList.ID = "hidTidList";
            hidTidList.Value = tidList;

            wrapper.Controls.Add(hidTidList);
            wrapper.Controls.Add(tblList);

            if (oRightsList.RightsList.Count == 0)
            {
                Panel pnEmptyMsg = new Panel();
                pnEmptyMsg.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 7612)));
                pnEmptyMsg.CssClass = "EmptyMsg";
                wrapper.Controls.Add(pnEmptyMsg);
            }

            //// Ajout du script Slider
            //HtmlGenericControl script = new HtmlGenericControl("script");
            //script.Attributes.Add("type", "text/javascript");
            //script.Attributes.Add("src", String.Concat("eda/Scripts/rangeSlider.js"));
            //wrapper.Controls.Add(script);

            _pgContainer.Controls.Add(wrapper);
        }


        /// <summary>Génération des filtres "Onglet" et "Type"</summary>
        protected virtual void CreateHeaderFilters()
        {
            Panel panel = new Panel();
            panel.ID = "headerFilters";

            // Onglets
            Panel field = new Panel();
            field.CssClass = "field";
            field.ID = "fltTab";
            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = String.Concat(eResApp.GetRes(Pref, 264), " :");
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlListTabs";
            ddl.SelectedValue = _descid.ToString();
            ddl.Attributes.Add("onchange", "nsAdminRights.refreshRightsLaunch(this);");

            Dictionary<int, String> tabs = eSqlDesc.LoadTabs(Pref);
            ddl.DataSource = tabs;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Key";
            ddl.DataBind();

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

            field.Controls.Add(label);
            field.Controls.Add(ddl);

            panel.Controls.Add(field);

            #region Depuis et Rubrique

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

            //Séparateur
            li = new ListItem(sSep, "0");
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


            // Rubrique
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
            //Séparateur
            li = new ListItem(sSep, "0");
            ddl.Items.Insert(0, li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            //AUCUN
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 436), "-1"));
            //TOUS
            ddl.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 435), "0"));
            if (Field > 0)
                ddl.SelectedValue = Field.ToString();


            #endregion

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


            //
            List<ListItem> lst = new List<ListItem>();
            foreach (eTreatmentType tt in Enum.GetValues(typeof(eTreatmentType)))
            {
                ListItem item = new ListItem(eAdminTreatmentRight.GetResType(tt, Pref), tt.GetHashCode().ToString());
                if (LstTreatmentTypes.Count == 1 && LstTreatmentTypes.First() == tt)
                    item.Selected = true;
                lst.Add(item);
            }

            //tri alpha
            lst.Sort((a, b) => a.Text.CompareTo(b.Text));


            //Ajout tous
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 22), eLibConst.USER_LEVEL.LEV_USR_ADMIN.GetHashCode().ToString()));

            //Séparateur
            li = new ListItem(sSep, "0");
            ddl.Items.Add(li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");


            ddl.Items.AddRange(lst.ToArray());

            panel.Controls.Add(field);


            //Fonctions
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

            panel.Controls.Add(field);


            //TOUS
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 435), ""));

            //Séparateur
            li = new ListItem(sSep, "0");
            ddl.Items.Add(li);
            li.Attributes.Add("disabled", "1");
            li.Attributes.Add("class", "BotSep");

            _pgContainer.Controls.Add(panel);
        }

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
            ddl.Items.Add(new ListItem(eLibTools.GetUserLevelLabel(_ePref, eLibConst.USER_LEVEL.LEV_USR_ADMIN), eLibConst.USER_LEVEL.LEV_USR_ADMIN.GetHashCode().ToString()));
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
                List<eUser.UserListItem> usrList = usrObj.GetUserList(true, false, "", ref sbError);

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

        /// <summary>Génération du bouton Traitements</summary>
        protected virtual void CreateTreatmentsButton()
        {

            eAdminField button = new eAdminButtonField(eResApp.GetRes(_ePref, 295), "treatmentsButton", onclick: "top.nsAdmin.showTreatmentsRights()");
            button.Generate(_pgContainer);
        }

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
            Boolean bLevel = permSearch.PermLevel >= permSearchIn.PermLevel;

            //comparaison des deux listes d'utilisateurs
            // ce test est assez gourmand, on ne le fait que si le permMode le sollicite
            Boolean bUser = false;

            // Si aucun utilisateur n'est autorisé à faire l'action :
            Boolean bNoUserAllowed = String.IsNullOrEmpty(permSearchIn.PermUser) &&
                (permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_ONLY || permSearchIn.PermMode == ePermission.PermissionMode.MODE_USER_AND_LEVEL);

            if (bNoUserAllowed)
                return false;

            if (String.IsNullOrEmpty(permSearch.PermUser) || !bNoUserAllowed)
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
                        hsGpSearchIn.Add(s);
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
    }
}