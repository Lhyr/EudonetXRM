using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRelationsDialogRenderer : eAdminRenderer
    {
        private Int32 _nTab;

        private enum LinkType
        {
            ParentTab,
            PP,
            PM
        }

        private String _tabName;
        private eAdminTableInfos _tabInfos;
        private String _tabParent;
        private Dictionary<String, String> _prefAutolink;
        private String[] _autolinkFiles;
        private Dictionary<int, String> _listRES;

        private eUserValue _uvSearchAllParentTab = null;
        private eUserValue _uvSearchAllPP = null;
        private eUserValue _uvSearchAllPM = null;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        private eAdminRelationsDialogRenderer(ePref pref, int nTab)
        {
            Pref = pref;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _nTab = nTab;
            _tabName = _tabInfos.TableLabel;
            _listRES = new Dictionary<int, string>();
        }

        public static eAdminRelationsDialogRenderer CreateAdminRightsDialogRenderer(ePref pref, int nTab)
        {
            return new eAdminRelationsDialogRenderer(pref, nTab);
        }

        protected override bool Init()
        {
            if (!base.Init())
                return false;

            eudoDAL dal = eLibTools.GetEudoDAL(Pref);

            try
            {
                dal.OpenDatabase();

                foreach (LinkType link in Enum.GetValues(typeof(LinkType)))
                {
                    InitUserValue(dal, link);
                }
            }
            finally
            {
                if (dal != null)
                    dal.CloseDatabase();
            }

            return true;
        }

        private void InitUserValue(eudoDAL dal, LinkType link)
        {
            Int32 nParentTabDescId = 0;
            eUserValue uvSearchAll = null;

            if (link == LinkType.ParentTab)
            {
                nParentTabDescId = _tabInfos.InterEVTDescid;
            }
            else if (link == LinkType.PP)
            {
                nParentTabDescId = (int)TableType.PP;
            }
            else if (link == LinkType.PM)
            {
                nParentTabDescId = (int)TableType.PM;
            }

            if (nParentTabDescId != 0)
            {
                Int32 nUservalueTab, nUservalueField;
                nUservalueTab = _tabInfos.DescId;
                nUservalueField = nParentTabDescId + 1;

                uvSearchAll = new eUserValue(dal, nUservalueField, TypeUserValue.SEARCH_ALL, Pref.User, nUservalueTab);
                uvSearchAll.Build();
            }

            if (uvSearchAll != null)
            {
                if (link == LinkType.ParentTab)
                {
                    _uvSearchAllParentTab = uvSearchAll;
                }
                else if (link == LinkType.PP)
                {
                    _uvSearchAllPP = uvSearchAll;
                }
                else if (link == LinkType.PM)
                {
                    _uvSearchAllPM = uvSearchAll;
                }
            }
        }


        protected override bool Build()
        {
            // Chargement des ressources nécessaires
            LoadRES();

            #region Etape 1 : Choix des liaisons

            Panel panelStep = new Panel();
            panelStep.CssClass = "divStep";

            panelStep.Controls.Add(
                GenerateStepTitle((_tabInfos.EdnType == EdnType.FILE_MAIN) ? "1" : "", eResApp.GetRes(Pref, 7387).Replace("<TABLE>", _tabName), true)
                );

            _prefAutolink = Pref.GetPrefDefault(_nTab, new List<String> { "AutoLinkCreation", "AutoLinkEnabled", "AutoLinkFile" });
            _autolinkFiles = _prefAutolink["AutoLinkFile"].Split(';');

            HtmlGenericControl stepContent = new HtmlGenericControl("div");
            stepContent.Attributes.Add("class", "stepContent");
            stepContent.Attributes.Add("data-active", "1");
            if (_tabInfos.TabType != TableType.ADR)
                stepContent.Controls.Add(GenerateRelationTable(LinkType.ParentTab));

            stepContent.Controls.Add(GenerateRelationTable(LinkType.PP));

            stepContent.Controls.Add(GenerateRelationTable(LinkType.PM));

            // Options fenêtre d'association
            if (_tabInfos.TabType == TableType.TEMPLATE)
            {
                stepContent.Controls.Add(GenerateOtherOptions());
            }

            panelStep.Controls.Add(stepContent);

            _pgContainer.Controls.Add(panelStep);

            #endregion Etape 1 : Choix des liaisons

            #region Etape 2 : Liste des rubriques

            if (_tabInfos.EdnType == EdnType.FILE_MAIN && _tabInfos.TabType != TableType.ADR)
            {
                panelStep = new Panel();
                panelStep.CssClass = "divStep";

                panelStep.Controls.Add(
                    GenerateStepTitle("2", eResApp.GetRes(Pref, 7386).Replace("<TABLE>", _tabName), false)
                    );

                stepContent = new HtmlGenericControl("div");
                stepContent.ID = "stepContent2";
                stepContent.Attributes.Add("class", "stepContent");
                stepContent.Attributes.Add("data-active", "0");

                // Le contenu sera créé avec le manager
                //eAdminRelationsFieldsRenderer rdr = eAdminRendererFactory.CreateAdminRelationsFieldsRenderer(Pref, _tabInfos.DescId);
                //stepContent.Controls.Add(rdr.PgContainer);

                panelStep.Controls.Add(stepContent);

                _pgContainer.Controls.Add(panelStep);
            }

            #endregion Etape 2 : Liste des rubriques

            return base.Build();
        }

        /// <summary>
        /// Chargement des ressources
        /// </summary>
        private void LoadRES()
        {
            String error = String.Empty;
            _listRES = eLibTools.GetRes(Pref, "200;300;400", Pref.Lang, out error);
            if (!String.IsNullOrEmpty(error))
            {
                throw new Exception("eAdminRelationsDialogRenderer.LoadRES => " + error);
            }
        }

        /// <summary>
        /// Génère la partie titre de l'étape
        /// </summary>
        /// <param name="sNum"></param>
        /// <param name="title"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        private HtmlGenericControl GenerateStepTitle(String sNum, String title, Boolean active)
        {
            HtmlGenericControl span;

            String classActive = (active) ? " active" : String.Empty;

            HtmlGenericControl step = new HtmlGenericControl("div");
            step.ID = String.Concat("stepTitle", sNum);
            step.Attributes.Add("class", "paramStep" + classActive);

            if (!String.IsNullOrEmpty(sNum))
            {
                span = new HtmlGenericControl();
                span.InnerText = sNum;
                span.Attributes.Add("class", "stepNum");
                step.Controls.Add(span);
            }

            span = new HtmlGenericControl();
            span.InnerText = title;
            span.Attributes.Add("class", "stepTitle");
            step.Controls.Add(span);

            return step;
        }

        /// <summary>
        /// Génère la partie Paramètres d'une liaison parente
        /// </summary>
        /// <param name="link"></param>
        /// <returns></returns>
        private HtmlGenericControl GenerateRelationTable(LinkType link)
        {

            String linkName = String.Empty;
            _tabParent = String.Empty;

            ExtendedDictionary<Int32, string> dicParentTables = _tabInfos.GetParentTables();

          

   

            int interevtnum = TableLite.CalculDidToInterEvtNum(_tabInfos.InterEVTDescid);


            if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
            {
                List<int> lstHiddenTab = new List<int>();
                lstHiddenTab = eLibTools.GetDescAdvInfo(Pref, dicParentTables.Keys.Select(ww => (ww + 10) * 100).ToList(),
                    new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT })
                        .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && dd.Item2 == "1") != null)
                        .Select(t => t.Key).ToList();


             

                if (lstHiddenTab.Contains(_tabInfos.InterEVTDescid))
                    lstHiddenTab.Remove(_tabInfos.InterEVTDescid);

                foreach(int nHidden in lstHiddenTab)
                {

                    if (dicParentTables.ContainsKey(TableLite.CalculDidToInterEvtNum(nHidden)))
                        dicParentTables.Remove(TableLite.CalculDidToInterEvtNum(nHidden));
                }
            }

            if (link == LinkType.ParentTab)
            {
                if (interevtnum > 0)
                    dicParentTables.TryGetValue(interevtnum, out linkName);
                else linkName = eResApp.GetRes(Pref, 7251); // Onglet parent

                _tabParent = String.Concat("<span class='parentTabName'>", linkName, "</span>");
            }
            else if (link == LinkType.PP)
            {
                linkName = _listRES[200];
            }
            else if (link == LinkType.PM)
            {
                if (_tabInfos.EdnType == EdnType.FILE_MAIN || _tabInfos.TabType == TableType.ADR)
                    linkName = _listRES[300];
                else
                {
                    linkName = String.Concat(_listRES[300], "/", _listRES[400]);
                }

                _tabParent = String.Concat("<span class='pmaddressTabName'>", linkName, "</span>");
            }
            HtmlGenericControl wrapper = new HtmlGenericControl("div");
            wrapper.Attributes.Add("class", "adminTableRelation");

            #region Titre table parente

            Panel div = new Panel();
            div.Attributes.Add("class", "adminTableRelationTitle");

            HtmlGenericControl subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "relationTableName");
            subDiv.InnerText = linkName; // Onglet parent
            div.Controls.Add(subDiv);

            subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "relationOpener");

            HtmlGenericControl divOpener = new HtmlGenericControl("div");
            divOpener.Attributes.Add("class", "openerButton");
            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-chevron-down openerButtonIcon");
            divOpener.Controls.Add(icon);

            subDiv.Controls.Add(divOpener);

            div.Controls.Add(subDiv);

            wrapper.Controls.Add(div);

            #endregion Titre table parente

            #region Contenu

            div = new Panel();
            div.Attributes.Add("class", "adminTableRelationContent");
            div.Attributes.Add("data-active", "0");

            HtmlGenericControl p = new HtmlGenericControl("p");

            div.Controls.Add(p);

            if (link == LinkType.ParentTab)
            {
                eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(_tabInfos.InterEVT, false);
                chkCtrl.ID = "chkInterEVT";
                chkCtrl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.INTEREVENT.GetHashCode()));
                chkCtrl.AddClick("nsAdminRelations.optionOnClick(this)");
                chkCtrl.AddText(eResApp.GetRes(Pref, 7383).Replace("<TABLE>", _tabName).Replace("<PARENT>", ""));
                p.Controls.Add(chkCtrl);

                DropDownList ddlTable = new DropDownList();
                ddlTable.ID = "ddlInterTables";
                ddlTable.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.INTEREVENTNUM.GetHashCode()));
                ddlTable.Attributes.Add("onchange", "nsAdminRelations.updateParentTab(this);");
                ddlTable.DataSource = dicParentTables;
                ddlTable.DataTextField = "Value";
                ddlTable.DataValueField = "Key";
                ddlTable.DataBind();
                ddlTable.Items.Insert(0, new ListItem(eResApp.GetRes(Pref, 7382), "")); // Sélectionner un onglet parent

                if (_tabInfos.InterEVT)
                {
                    ddlTable.SelectedValue = interevtnum.ToString();
                }

                p.Controls.Add(ddlTable);

                //txt = new LiteralControl();
                //txt.Text = "&nbsp;Puis, définir les caractéristiques de cette liaison :";
                //p.Controls.Add(txt);

                #region Caractéristiques (radio boutons)

                div.Controls.Add(GenerateFeatures(_tabParent, link, _tabInfos.InterEVT));

                #endregion Caractéristiques (radio boutons)
            }
            else if (link == LinkType.PP)
            {
                eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(_tabInfos.InterPP, false);
                chkCtrl.ID = "chkInterPP";
                chkCtrl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "|", eLibConst.DESC.INTERPP.GetHashCode()));
                chkCtrl.AddClick("nsAdminRelations.optionOnClick(this);");
                chkCtrl.AddText(eResApp.GetRes(Pref, 7383).Replace("<TABLE>", _tabName).Replace("<PARENT>", linkName));
                if (_tabInfos.IsExtendedTargetSubfile || _tabInfos.TabType == TableType.ADR)
                    chkCtrl.SetDisabled(true);

                p.Controls.Add(chkCtrl);

                div.Controls.Add(GenerateFeatures(linkName, link, _tabInfos.InterPP));
            }
            else if (link == LinkType.PM)
            {
                eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl(_tabInfos.InterPM || _tabInfos.AdrJoin, false);
                chkCtrl.ID = "chkInterPM";
                chkCtrl.AddClick("nsAdminRelations.optionOnClick(this);");
                chkCtrl.AddText(eResApp.GetRes(Pref, 7383).Replace("<TABLE>", _tabName).Replace("<PARENT>", ""));
                if (_tabInfos.IsExtendedTargetSubfile || _tabInfos.TabType == TableType.ADR)
                    chkCtrl.SetDisabled(true);

                p.Controls.Add(chkCtrl);

                DropDownList ddlTable = new DropDownList();
                ddlTable.ID = "ddlInterPMADR";
                ddlTable.Attributes.Add("onchange", "nsAdminRelations.updateParentTab(this);");

                ddlTable.Items.Add(new ListItem(_listRES[300], "300"));
                if (_tabInfos.EdnType != EdnType.FILE_MAIN)
                {
                    ddlTable.Items.Add(new ListItem(_listRES[400], "400"));
                    ddlTable.Items.Insert(0, (new ListItem(eResApp.GetRes(Pref, 7382), "")));
                }

                if (_tabInfos.TabType == TableType.TEMPLATE)
                {
                    if (_tabInfos.InterPM)
                    {
                        ddlTable.SelectedValue = "300";
                    }
                    else if (_tabInfos.AdrJoin)
                    {
                        ddlTable.SelectedValue = "400";

                        // La liaison vers Adresses est native pour Cible Etendu, pas de modification 
                        if (_tabInfos.IsExtendedTargetSubfile)
                            ddlTable.Enabled = false;
                    }
                }
                else
                {
                    ddlTable.SelectedValue = "300";
                    ddlTable.Enabled = false;
                }

                p.Controls.Add(ddlTable);

                div.Controls.Add(GenerateFeatures(_tabParent, link, _tabInfos.InterPM || _tabInfos.AdrJoin));

                Control c = eAdminFieldBuilder.BuildField(div, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.ADRJOIN.GetHashCode());
                c.ID = "hidAdrJoin";

                c = eAdminFieldBuilder.BuildField(div, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.INTERPM.GetHashCode());
                c.ID = "hidInterPM";
            }

            wrapper.Controls.Add(div);

            #endregion Contenu

            return wrapper;
        }

        /// <summary>
        /// Génère la partie options de la fenêtre d'association (pour un template)
        /// </summary>
        /// <returns></returns>
        private Panel GenerateOtherOptions()
        {
            #region Autolinkenabled

            // Titre
            Panel wrapper = new Panel();
            wrapper.CssClass = "adminTableRelation";

            Panel div = new Panel();
            div.Attributes.Add("class", "adminTableRelationTitle");

            HtmlGenericControl subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "relationTableName");
            subDiv.InnerText = eResApp.GetRes(Pref, 7814);
            div.Controls.Add(subDiv);

            subDiv = new HtmlGenericControl("div");
            subDiv.Attributes.Add("class", "relationOpener");

            HtmlGenericControl divOpener = new HtmlGenericControl("div");
            divOpener.Attributes.Add("class", "openerButton");
            HtmlGenericControl icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-chevron-down openerButtonIcon");
            divOpener.Controls.Add(icon);

            subDiv.Controls.Add(divOpener);

            div.Controls.Add(subDiv);

            wrapper.Controls.Add(div);

            // Contenu
            Panel panel = new Panel();
            panel.CssClass = "adminTableRelationContent";
            panel.Attributes.Add("data-active", "0");

            // Case à cocher AutoLinkEnabled
            HtmlGenericControl p = new HtmlGenericControl("p");
            eCheckBoxCtrl chkCtrl = new eCheckBoxCtrl((_prefAutolink["AutoLinkEnabled"] == "1"), false);
            chkCtrl.ID = "chkAutolinkEnabled";
            chkCtrl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.AUTOLINKENABLED.GetHashCode()));
            chkCtrl.AddClick("nsAdminRelations.optionOnClick(this);");
            chkCtrl.AddText(eResApp.GetRes(Pref, 8077).Replace("<TAB>", _tabName));
            p.Controls.Add(chkCtrl);

            panel.Controls.Add(p);

            Panel panelFeatures = new Panel();
            panelFeatures.CssClass = "relationsFeatures";
            panelFeatures.Attributes.Add("data-active", _prefAutolink["AutoLinkEnabled"]);

            // Champ caché pour obtenir l'attribut "dsc"
            Control hidAutolinkfile = eAdminFieldBuilder.BuildField(panelFeatures, AdminFieldType.ADM_TYPE_HIDDEN, "", eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOLINKFILE.GetHashCode());
            hidAutolinkfile.ID = "autolinkfile";

            String autolinkcreationValue = String.Empty;
            _prefAutolink.TryGetValue("AutoLinkCreation", out autolinkcreationValue);

            Dictionary<string, string> dicRB = new Dictionary<string, string>();
            dicRB.Add(AutolinkActions.ATL_ASK.GetHashCode().ToString(), eResApp.GetRes(Pref, 8078));
            dicRB.Add(AutolinkActions.ATL_CREATE.GetHashCode().ToString(), eResApp.GetRes(Pref, 8079));
            dicRB.Add(AutolinkActions.ATL_CANCEL.GetHashCode().ToString(), eResApp.GetRes(Pref, 8080));
            eAdminField rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8081),
                eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOLINKCREATION.GetHashCode(), "autolinkcreation", dicRB, value: (String.IsNullOrEmpty(autolinkcreationValue) ? "0" : autolinkcreationValue));
            rb.SetFieldControlID("autolinkcreation");
            rb.IsLabelBefore = true;
            rb.Generate(panelFeatures);

            panel.Controls.Add(panelFeatures);

            wrapper.Controls.Add(panel);

            return wrapper;

            #endregion Autolinkenabled
        }

        /// <summary>
        /// Affichage des caractéristiques de la table liée
        /// </summary>
        /// <param name="tabParent">Libellé de la table liée</param>
        /// <param name="linkType">Type de lien (enum) : un onglet, PP ou PM</param>
        /// <param name="bOpened">Bloc affiché à l'ouverture de la popup</param>
        /// <returns></returns>
        private Panel GenerateFeatures(String tabParent, LinkType linkType, Boolean bOpened)
        {
            Dictionary<string, string> dicRB;
            eAdminRadioButtonField rb;

            eUserValue uvSearchAll = null;
            string rbSearchAllIdSuffix = String.Empty;
            TypeUserValueAdmin uvSearchAllType = TypeUserValueAdmin.SEARCHALL_EVT;
            TypeUserValueAdmin uvSearchAllBlockedType = TypeUserValueAdmin.SEARCHALLBLOCKED_EVT;


            Panel divFeatures = new Panel();
            divFeatures.CssClass = "relationsFeatures";
            divFeatures.ID = "block" + linkType.ToString();
            divFeatures.Attributes.Add("data-active", (bOpened) ? "1" : "0");

            #region Option "Liaison obligatoire"

            int intereventneededHashcode = 0;
            String rbGroupName = String.Empty;
            Boolean valueInterNeeded = false;
            if (linkType == LinkType.ParentTab)
            {
                intereventneededHashcode = eLibConst.DESC.INTEREVENTNEEDED.GetHashCode();
                rbGroupName = "intereventneeded";
                valueInterNeeded = _tabInfos.InterEventNeeded;
            }
            else if (linkType == LinkType.PP)
            {
                intereventneededHashcode = eLibConst.DESC.INTERPPNEEDED.GetHashCode();
                rbGroupName = "interppneeded";
                valueInterNeeded = _tabInfos.InterPpNeeded;
            }
            else if (linkType == LinkType.PM)
            {
                intereventneededHashcode = eLibConst.DESC.INTERPMNEEDED.GetHashCode();
                rbGroupName = "interpmneeded";
                valueInterNeeded = _tabInfos.InterPmNeeded;
            }


            dicRB = new Dictionary<string, string>();
            dicRB.Add("0", eResApp.GetRes(Pref, 8082).Replace("<TAB>", _tabParent));
            dicRB.Add("1", eResApp.GetRes(Pref, 8083).Replace("<TAB>", _tabParent));
            string sQuestion = "";
            if (_tabInfos.TabType == TableType.ADR && linkType == LinkType.PM)
            {
                //1907 : Cette option n'est pas disponible pour la liaison entre {0} et {1}. La liaison sera toujours facultative pour les adresses personnelles et obligatoire pour les adresses professionnelles.
                HtmlGenericControl c = new HtmlGenericControl();
                c.Attributes.Add("class", "icon-question-circle-o");
                c.Attributes.Add("Title", string.Format(eResApp.GetRes(Pref, 1907), _tabInfos.TableLabel, _listRES[300]));
                sQuestion = eRenderer.RenderControl(c);
            }

            rb = new eAdminRadioButtonField(_nTab, String.Concat(eResApp.GetRes(Pref, 8084).Replace("<TAB>", _tabParent)," ", sQuestion),
                eAdminUpdateProperty.CATEGORY.DESC, intereventneededHashcode, rbGroupName, dicRB, value: (valueInterNeeded) ? "1" : "0");
            rb.SetFieldControlID(rbGroupName);
            rb.IsLabelBefore = true;
            rb.IsDisplayed = (!_tabInfos.AdrJoin);
            if (_tabInfos.TabType == TableType.ADR && linkType == LinkType.PM)
            {
                rb.ReadOnly = true;
                rb.TooltipText = string.Format(eResApp.GetRes(Pref, 1907), _tabInfos.TableLabel, _tabParent);
            }
            rb.Generate(divFeatures);
            if (linkType == LinkType.PM)
                rb.SetFieldAttribute("data-optFor", "pm"); // On marque l'option pour l'affichage PM/ADR


            #endregion Option "Liaison obligatoire"

            if (linkType == LinkType.ParentTab)
            {
                uvSearchAll = _uvSearchAllParentTab;
                rbSearchAllIdSuffix = "EVT";
                uvSearchAllType = TypeUserValueAdmin.SEARCHALL_EVT;
                uvSearchAllBlockedType = TypeUserValueAdmin.SEARCHALLBLOCKED_EVT;

                #region Option "Liaison masquée"

                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 8085).Replace("<TAB>", _tabParent));
                dicRB.Add("1", eResApp.GetRes(Pref, 8086).Replace("<TAB>", _tabParent));
                rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8087),
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.INTEREVENTHIDDEN.GetHashCode(), "intereventhidden", dicRB, value: (_tabInfos.InterEventHidden) ? "1" : "0");
                rb.SetFieldControlID("intereventhidden");
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);

                #endregion Option "Liaison masquée"

                #region Option liaison reprise

                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 8088).Replace("<TAB>", _tabParent));
                dicRB.Add("1", eResApp.GetRes(Pref, 8089).Replace("<TAB>", _tabParent));
                rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8090).Replace("<TAB>", _tabName),
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NODEFAULTLINK_100.GetHashCode(), "nodefaultlink100", dicRB, value: (_tabInfos.NoDefaultLink_100) ? "1" : "0");
                rb.SetFieldControlID("nodefaultlink100");
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);

                #endregion Option liaison reprise

                if (_tabInfos.TabType == TableType.TEMPLATE)
                {
                    #region Autolinkfile

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8091).Replace("<TAB>", _tabParent));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8092).Replace("<TAB>", _tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8093).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOLINKFILE.GetHashCode(), "autolinkfile100", dicRB,
                        value: _autolinkFiles.Contains(_tabInfos.InterEVTDescid.ToString()) ? "1" : "");
                    rb.SetFieldControlID("autolinkfile100");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #endregion Autolinkfile

                    #region Option liaison proposée

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("0", eResApp.GetRes(Pref, 8091).Replace("<TAB>", _tabParent));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8094).Replace("<TAB>", _tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8095).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.DEFAULTLINK_100.GetHashCode(), "defaultlink100", dicRB, value: (_tabInfos.DefaultLink_100) ? "1" : "0");
                    rb.SetFieldControlID("defaultlink100");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #endregion Option liaison proposée

                    #region Champ Notes masqué/affiché

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8096).Replace("<TAB>", _tabParent));
                    dicRB.Add("194", eResApp.GetRes(Pref, 8097).Replace("<TAB>", _tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8087),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.TPL_100.GetHashCode(), "tpl100", dicRB, value: String.IsNullOrEmpty(_tabInfos.Tpl_100) ? "" : _tabInfos.Tpl_100);
                    rb.SetFieldControlID("tpl100");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #endregion Champ Notes masqué/affiché
                }
            }
            else if (linkType == LinkType.PP)
            {
                uvSearchAll = _uvSearchAllPP;
                rbSearchAllIdSuffix = "PP";
                uvSearchAllType = TypeUserValueAdmin.SEARCHALL_PP;
                uvSearchAllBlockedType = TypeUserValueAdmin.SEARCHALLBLOCKED_PP;

                // Liaison reprise
                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 8088).Replace("<TAB>", _tabParent));
                dicRB.Add("1", eResApp.GetRes(Pref, 8089).Replace("<TAB>", _tabParent));
                rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8090).Replace("<TAB>", _tabName),
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NODEFAULTLINK_200.GetHashCode(), "nodefaultlink200", dicRB, value: (_tabInfos.NoDefaultLink_200) ? "1" : "0");
                rb.SetFieldControlID("nodefaultlink200");
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);

                if (_tabInfos.TabType == TableType.EVENT)
                {
                    // Cascade PP -> PM
                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("0", eResApp.GetRes(Pref, 8109).Replace("<PM>", _listRES[300]).Replace("<PP>", _listRES[200]).Replace("<TAB>", _tabName));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8110).Replace("<PM>", _listRES[300]).Replace("<TAB>", _tabName));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8111).Replace("<PP>", _listRES[200]).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NOCASCADEPPPM.GetHashCode(), "nocascadepppm", dicRB, value: (_tabInfos.NoCascadePPPM) ? "1" : "0");
                    rb.SetFieldControlID("nocascadepppm");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);
                }
                else if (_tabInfos.TabType == TableType.TEMPLATE)
                {
                    #region Autolinkfile

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8091).Replace("<TAB>", tabParent));
                    dicRB.Add("200", eResApp.GetRes(Pref, 8092).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8093).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOLINKFILE.GetHashCode(), "autolinkfile200", dicRB,
                        value: _autolinkFiles.Contains("200") ? "200" : "");
                    rb.SetFieldControlID("autolinkfile200");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #endregion Autolinkfile

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("0", eResApp.GetRes(Pref, 8091).Replace("<TAB>", tabParent));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8094).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8095).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.DEFAULTLINK_200.GetHashCode(), "defaultlink200", dicRB, value: (_tabInfos.DefaultLink_200) ? "1" : "0");
                    rb.SetFieldControlID("defaultlink200");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #region Champ Notes masqué/affiché

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8096).Replace("<TAB>", tabParent));
                    dicRB.Add("294", eResApp.GetRes(Pref, 8097).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8087),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.TPL_200.GetHashCode(), "tpl200", dicRB, value: String.IsNullOrEmpty(_tabInfos.Tpl_200) ? "" : _tabInfos.Tpl_200);
                    rb.SetFieldControlID("tpl200");
                    rb.IsLabelBefore = true;
                    rb.Generate(divFeatures);

                    #endregion Champ Notes masqué/affiché
                }
            }
            else if (linkType == LinkType.PM)
            {
                uvSearchAll = _uvSearchAllPM;
                rbSearchAllIdSuffix = "PMADR";
                uvSearchAllType = TypeUserValueAdmin.SEARCHALL_PMADR;
                uvSearchAllBlockedType = TypeUserValueAdmin.SEARCHALLBLOCKED_PMADR;

                #region Liaison PM
                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 8088).Replace("<TAB>", tabParent));
                dicRB.Add("1", eResApp.GetRes(Pref, 8089).Replace("<TAB>", tabParent));
                rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8090).Replace("<TAB>", _tabName),
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NODEFAULTLINK_300.GetHashCode(), "nodefaultlink300", dicRB, value: (_tabInfos.NoDefaultLink_300) ? "1" : "0");
                rb.SetFieldControlID("nodefaultlink300");
                rb.IsLabelBefore = true;
                rb.IsDisplayed = (!_tabInfos.AdrJoin);
                rb.Generate(divFeatures);
                rb.SetFieldAttribute("data-optFor", "pm");

                if (_tabInfos.TabType == TableType.EVENT)
                {
                    // Cascade PM -> PP
                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("0", eResApp.GetRes(Pref, 8109).Replace("<PM>", _listRES[200]).Replace("<PP>", _listRES[300]).Replace("<TAB>", _tabName));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8110).Replace("<PM>", _listRES[200]).Replace("<TAB>", _tabName));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8111).Replace("<PP>", _listRES[300]).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.NOCASCADEPMPP.GetHashCode(), "nocascadepmpp", dicRB, value: (_tabInfos.NoCascadePMPP) ? "1" : "0");
                    rb.SetFieldControlID("nocascadepmpp");
                    rb.IsLabelBefore = true;
                    rb.IsDisplayed = (!_tabInfos.AdrJoin);
                    rb.Generate(divFeatures);
                    rb.SetFieldAttribute("data-optFor", "pm");
                }
                else if (_tabInfos.TabType == TableType.TEMPLATE)
                {
                    #region Autolinkfile

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8091).Replace("<TAB>", tabParent));
                    dicRB.Add("300", eResApp.GetRes(Pref, 8092).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8093).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOLINKFILE.GetHashCode(), "autolinkfile300", dicRB,
                        value: _autolinkFiles.Contains("300") ? "300" : "");
                    rb.SetFieldControlID("autolinkfile300");
                    rb.IsLabelBefore = true;
                    rb.IsDisplayed = (!_tabInfos.AdrJoin);
                    rb.Generate(divFeatures);
                    rb.SetFieldAttribute("data-optFor", "pm");

                    #endregion Autolinkfile

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("0", eResApp.GetRes(Pref, 8091).Replace("<TAB>", tabParent));
                    dicRB.Add("1", eResApp.GetRes(Pref, 8094).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8095).Replace("<TAB>", _tabName),
                        eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.DEFAULTLINK_300.GetHashCode(), "defaultlink300", dicRB, value: (_tabInfos.DefaultLink_300) ? "1" : "0");
                    rb.SetFieldControlID("defaultlink300");
                    rb.IsLabelBefore = true;
                    rb.IsDisplayed = (!_tabInfos.AdrJoin);
                    rb.Generate(divFeatures);
                    rb.SetFieldAttribute("data-optFor", "pm");

                    #region Champ Notes masqué/affiché

                    dicRB = new Dictionary<string, string>();
                    dicRB.Add("", eResApp.GetRes(Pref, 8096).Replace("<TAB>", tabParent));
                    dicRB.Add("394", eResApp.GetRes(Pref, 8097).Replace("<TAB>", tabParent));
                    rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8087),
                        eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.TPL_300.GetHashCode(), "tpl300", dicRB, value: String.IsNullOrEmpty(_tabInfos.Tpl_300) ? "" : _tabInfos.Tpl_300);
                    rb.SetFieldControlID("tpl300");
                    rb.IsLabelBefore = true;
                    rb.IsDisplayed = (!_tabInfos.AdrJoin);
                    rb.Generate(divFeatures);
                    rb.SetFieldAttribute("data-optFor", "pm");

                    #endregion Champ Notes masqué/affiché
                }
                #endregion

                #region Liaison Adresses

                Panel panelAdrOptions = new Panel();

                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 8112));
                dicRB.Add("1", eResApp.GetRes(Pref, 8113));
                rb = new eAdminRadioButtonField(_nTab, eResApp.GetRes(Pref, 8114),
                    eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.AUTOSELECTENABLED.GetHashCode(), "autoselectenabled", dicRB,
                    value: (_tabInfos.AutoSelectEnabled) ? "1" : "0",
                    onclick: "nsAdminRelations.checkAutoSelectEnabled(this)");
                rb.SetFieldControlID("autoselectenabled");
                rb.IsLabelBefore = true;
                rb.IsDisplayed = (_tabInfos.AdrJoin);
                rb.Generate(divFeatures);
                rb.SetFieldAttribute("data-optFor", "adr");

                eAdminField f = new eAdminTextboxFieldInline(_nTab, "", eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.AUTOSELECTVALUE.GetHashCode(), AdminFieldType.ADM_TYPE_NUM,
                    idBlock: "blockAutoSelectValue",
                    idField: "txtAutoSelectValue",
                    value: _tabInfos.AutoSelectValue.ToString(), inputWidth: 60,
                    prefixtext: eResApp.GetRes(Pref, 8115) + " ", suffixtext: " " + eResApp.GetRes(Pref, 300));
                f.IsDisplayed = (_tabInfos.AdrJoin && _tabInfos.AutoSelectEnabled);
                f.Generate(divFeatures);
                f.SetFieldAttribute("data-optFor", "adr");
                #endregion

            }

            #region Recherche sur toutes les fiches xxxx
            if (uvSearchAll != null)
            {
                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 1896).Replace("<PARENTTAB>", tabParent).Replace("<TAB>", _tabInfos.TableLabel));
                dicRB.Add("1", eResApp.GetRes(Pref, 1897));
                rb = new eAdminRadioButtonField(uvSearchAll.DescId, eResApp.GetRes(Pref, 1895).Replace("<PARENTTAB>", tabParent), eAdminUpdateProperty.CATEGORY.USERVALUE, (int)uvSearchAllType, String.Concat("rbSearchAll_", rbSearchAllIdSuffix), dicRB, value: uvSearchAll.Enabled ? "1" : "0");
                rb.SetFieldControlID(String.Concat("rbSearchAll_", rbSearchAllIdSuffix));
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);

                dicRB = new Dictionary<string, string>();
                dicRB.Add("0", eResApp.GetRes(Pref, 7944));
                dicRB.Add("1", eResApp.GetRes(Pref, 7945));
                rb = new eAdminRadioButtonField(uvSearchAll.DescId, eResApp.GetRes(Pref, 7946), eAdminUpdateProperty.CATEGORY.USERVALUE, (int)uvSearchAllBlockedType, String.Concat("rbSearchAllBlocked_", rbSearchAllIdSuffix), dicRB, value: uvSearchAll.Index == 1 ? "1" : "0");
                rb.SetFieldControlID(String.Concat("rbSearchAllBlocked_", rbSearchAllIdSuffix));
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);
            }

            #endregion

            #region Activer les filtres SQL (administration des ADDEDBKMWHERE)

            if ((int)Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
            {
                string sTab = String.Empty;
                if (linkType == LinkType.PM)
                    sTab = TableLite.CalculDidToInterEvtNum(TableType.PM.GetHashCode()).ToString();
                else if (linkType == LinkType.PP)
                    sTab = TableLite.CalculDidToInterEvtNum(TableType.PP.GetHashCode()).ToString();

                HtmlGenericControl aLink = new HtmlGenericControl("a");
                HtmlGenericControl aLinkSpan = new HtmlGenericControl("span");
                aLinkSpan.Attributes.Add("class", "icon-cog");
                aLink.ID = "actionLink";
                aLink.Attributes.Add("onclick", String.Concat("javascript:nsAdmin.confRelationsSQLFilters(", sTab, ")"));
                aLink.InnerHtml = eResApp.GetRes(Pref, 8116).Replace("<TAB>", _tabName).Replace("<PARENTTAB>", _tabParent);
                aLink.Controls.AddAt(0, aLinkSpan);
                divFeatures.Controls.Add(aLink);
            }

            #endregion Activer les filtres SQL (administration des ADDEDBKMWHERE)

            return divFeatures;
        }
    }
}