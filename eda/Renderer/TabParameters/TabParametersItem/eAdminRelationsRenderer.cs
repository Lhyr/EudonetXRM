using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using System.Linq;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Admin : Rendu du bloc "Relation" (liaisons native) d'une table
    /// </summary>
    public class eAdminRelationsRenderer : eAdminBlockRenderer
    {
        #region Propriétés
        private string parentTabName = String.Empty;
        private string ppTabName = String.Empty;
        private string pmTabName = String.Empty;
        private string adrTabName = String.Empty;
        bool _updateEnabled = true;
        #endregion

        enum LinkType
        {
            ParentTab,
            PP,
            PM,
            ADR
        }

        private eAdminRelationsRenderer(ePref pref, eAdminTableInfos tabInfos, string title, string titleInfo)
            : base(pref, tabInfos, title, titleInfo, idBlock: "RelationsPart")
        {

        }


        public static eAdminRelationsRenderer CreateAdminRelationsRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo)
        {
            eAdminRelationsRenderer features = new eAdminRelationsRenderer(pref, tabInfos, title, titleInfo);
            return features;
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if (_tabInfos.TabType != TableType.PP && _tabInfos.TabType != TableType.PM)
                {
                    List<string> listDescIds = new List<string>()
                        {
                            EudoQuery.TableType.PP.GetHashCode().ToString(),
                            EudoQuery.TableType.PM.GetHashCode().ToString(),
                            EudoQuery.TableType.ADR.GetHashCode().ToString()
                        };

                    if (_tabInfos.InterEVT)
                        listDescIds.Add(_tabInfos.InterEVTDescid.ToString());

                    eRes res = new eRes(Pref, String.Join(",", listDescIds.ToArray()));

                    bool bResFound = false;
                    ppTabName = res.GetRes(EudoQuery.TableType.PP.GetHashCode(), out bResFound);
                    if (!bResFound)
                        ppTabName = "Contacts";

                    bResFound = false;
                    pmTabName = res.GetRes(EudoQuery.TableType.PM.GetHashCode(), out bResFound);
                    if (!bResFound)
                        ppTabName = "Sociétés";

                    bResFound = false;
                    adrTabName = res.GetRes(EudoQuery.TableType.ADR.GetHashCode(), out bResFound);
                    if (!bResFound)
                        ppTabName = "Adresses";

                    if (_tabInfos.InterEVT)
                    {
                        bResFound = false;
                        parentTabName = res.GetRes(_tabInfos.InterEVTDescid, out bResFound);
                        if (!bResFound)
                            parentTabName = "Onglet parent";
                    }
                }

                _updateEnabled = eAdminTools.IsUserAllowedForProduct(Pref, Pref.User, _tabInfos.ProductID);

                return true;
            }
            return false;
        }

        /// <summary>Construction du bloc Relations</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            base.Build();
            _panelContent.CssClass = "paramPartContent";


            if (_tabInfos.TabType != TableType.PP
                && _tabInfos.TabType != TableType.PM
                //&& _tabInfos.TabType != TableType.ADR
                //&& _tabInfos.EdnType != EdnType.FILE_RELATION
                )
            {
                Panel SubMenuEVT, SubMenuPP, SubMenuPM;
                DropDownList ddlInterEVT, ddlInterPP, ddlInterPM;
                eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                dal.OpenDatabase();



                try
                {
                    #region Icones des fichiers parents pour affichage
                    String sError = "";
                    ExtendedDictionary<Int32, eAdminTableInfos.TabIcon> dicParentTablesIcon = _tabInfos.GetParentTablesIcons(dal, out sError);
                    if (sError.Length > 0)
                    {
                        eFeedbackXrm.LaunchFeedbackXrm(
                            eErrorContainer.GetDevError(
                                eLibConst.MSG_TYPE.CRITICAL,
                                String.Concat("---eAdminRelationsRenderer.Build()---", Environment.NewLine, "eAdminTableInfos.GetParentTablesIcons", Environment.NewLine, sError)
                                ),
                            Pref);
                    }
                    #endregion

                    #region Récupère les liaisons parentes représentées dans le corps de la fiche.

                    Dictionary<int, int> dicParentRelationsInFile = eSqlDesc.GetParentRelationsInFile(dal, _tab, out sError);


                    if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                    {
                        List<int> lstHiddenTab = new List<int>();
                        lstHiddenTab = eLibTools.GetDescAdvInfo(Pref, dicParentRelationsInFile.Values.Select(ww => (ww + 10) * 100).ToList(),
                          new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT })
                              .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && dd.Item2 == "1") != null)
                              .Select(t => t.Key).ToList();


                        if (lstHiddenTab.Contains(_tabInfos.InterEVTDescid))
                            lstHiddenTab.Remove(_tabInfos.InterEVTDescid);

                        foreach (int nHidden in lstHiddenTab)
                        {

                            if (dicParentRelationsInFile.ContainsKey(TableLite.CalculDidToInterEvtNum(nHidden)))
                                dicParentRelationsInFile.Remove(TableLite.CalculDidToInterEvtNum(nHidden));
                        }
                    }

                    #endregion


                    #region INTEREVT
                    SubMenuEVT = new Panel();
                    SubMenuEVT.CssClass = "parentParam";


                    _panelContent.Controls.Add(SubMenuEVT);

                    ddlInterEVT = new DropDownList();
                    ddlInterEVT.ID = "ddlInterEVT";
                    ddlInterEVT.CssClass = "relationparent";
                    ddlInterEVT.Attributes.Add("did", this._tabInfos.DescId.ToString());
                    ddlInterEVT.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.DESC, "|", (int)eLibConst.DESC.INTEREVENT));
                    SubMenuEVT.Controls.Add(ddlInterEVT);

                    ExtendedDictionary<Int32, string> dicParentTables = _tabInfos.GetParentTables(dal);

                    if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                    {
                        List<int> lstHiddenTab = new List<int>();
                        lstHiddenTab = eLibTools.GetDescAdvInfo(Pref, dicParentTables.Keys.Select(ww => (ww + 10) * 100).ToList(),
                          new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT })
                              .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && dd.Item2 == "1") != null)
                              .Select(t => t.Key).ToList();


                        if (lstHiddenTab.Contains(_tabInfos.InterEVTDescid))
                            lstHiddenTab.Remove(_tabInfos.InterEVTDescid);

                        foreach (int nHidden in lstHiddenTab)
                        {
                            if (dicParentTables.ContainsKey(TableLite.CalculDidToInterEvtNum(nHidden)))
                                dicParentTables.Remove(TableLite.CalculDidToInterEvtNum(nHidden));
                        }
                    }

                    ddlInterEVT.Items.Add(new ListItem(eResApp.GetRes(Pref, 6967), "0")); // Aucun Fichier Parent sélectionné

                    eAdminCapsule<eAdminUpdateProperty> caps = new eAdminCapsule<eAdminUpdateProperty>();
                    eAdminUpdateProperty pty;
                    caps.DescId = _tab;
                    pty = new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                        Property = (int)eLibConst.DESC.INTEREVENTNEEDED,
                        Value = "0"
                    };
                    caps.ListProperties.Add(pty);

                    pty = new eAdminUpdateProperty()
                    {
                        Category = (int)eAdminUpdateProperty.CATEGORY.PREF,
                        Property = (int)ADMIN_PREF.ADRJOIN,
                        Value = "0"
                    };
                    caps.ListProperties.Add(pty);

                    ddlInterEVT.Items[0].Attributes.Add("cplt", JsonConvert.SerializeObject(caps));


                    ListItem li;
                    int interevtnum = 0;
                    if (_tabInfos.InterEVT)
                    {
                        interevtnum = TableLite.CalculDidToInterEvtNum(_tabInfos.InterEVTDescid);
                    }


                    List<int> lstTabDescId = new List<int>(dicParentTables.Keys.Select(ww => (ww + 10) * 100));

                    if (Pref.User.UserLevel < (int)UserLevel.LEV_USR_SUPERADMIN)
                    {
                        List<int> lstHiddenTab = new List<int>();
                        lstHiddenTab = eLibTools.GetDescAdvInfo(Pref, lstTabDescId,
                          new List<DESCADV_PARAMETER>() { DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT })
                              .Where(aa => aa.Value.Find(dd => dd.Item1 == DESCADV_PARAMETER.HIDDEN_TAB_PRODUCT && dd.Item2 == "1") != null)
                              .Select(t => t.Key).ToList();


                        if (lstHiddenTab.Contains(_tabInfos.InterEVTDescid))
                            lstHiddenTab.Remove(_tabInfos.InterEVTDescid);

                        foreach (int nHidden in lstHiddenTab)
                        {
                            if (dicParentTables.ContainsKey(TableLite.CalculDidToInterEvtNum(nHidden)))
                                dicParentTables.Remove(TableLite.CalculDidToInterEvtNum(nHidden));
                        }
                    }


                    foreach (KeyValuePair<int, string> parentTab in dicParentTables)
                    {
                        String sLabel = GenerateLinkParentTitle(parentTab.Value);
                        li = new ListItem(sLabel, "1");
                        caps = new eAdminCapsule<eAdminUpdateProperty>();
                        caps.DescId = _tab;
                        pty = new eAdminUpdateProperty()
                        {
                            Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                            Property = (int)eLibConst.DESC.INTEREVENTNUM,
                            Value = parentTab.Key.ToString()
                        };
                        caps.ListProperties.Add(pty);

                        li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));

                        ddlInterEVT.Items.Add(li);

                        if (_tabInfos.InterEVT && interevtnum == parentTab.Key)
                        {
                            li.Selected = true;
                        }
                    }

                    ddlInterEVT.ToolTip = ddlInterEVT.SelectedItem.Text;

                    ddlInterEVT.Enabled = _updateEnabled;

                    if (_tabInfos.InterEVT)
                    {
                        #region Insertion de l'icone et du lien vers l'admin du fichier parent
                        if (dicParentTablesIcon.ContainsKey(_tabInfos.InterEVTDescid))
                        {
                            SubMenuEVT.Controls.Add(GenerateParentIcon(dicParentTablesIcon[_tabInfos.InterEVTDescid]));
                        }
                        #endregion

                        SubMenuEVT.Controls.Add(GenerateFeatures(parentTabName, LinkType.ParentTab, true));

                        generateDragAliasCtrl(SubMenuEVT, parentTabName, LinkType.ParentTab, dicParentRelationsInFile);
                    }
                    #endregion

                    ddlInterPP = new DropDownList();
                    if (_tabInfos.TabType != TableType.INTERACTION)
                    {
                        #region INTERPP
                        SubMenuPP = new Panel();
                        SubMenuPP.CssClass = "parentParam";


                        ddlInterPP.ID = "ddlInterPP";
                        ddlInterPP.CssClass = "relationparent";
                        ddlInterPP.Attributes.Add("did", this._tabInfos.DescId.ToString());
                        ddlInterPP.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.DESC, "|", (int)eLibConst.DESC.INTERPP));

                        ddlInterPP.Items.Add(new ListItem(GenerateNoLinkPPPMTitle(ppTabName), "0"));

                        caps = new eAdminCapsule<eAdminUpdateProperty>();
                        caps.DescId = _tab;

                        pty = new eAdminUpdateProperty()
                        {
                            Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                            Property = (int)eLibConst.DESC.INTERPPNEEDED,
                            Value = "0"
                        };
                        caps.ListProperties.Add(pty);

                        pty = new eAdminUpdateProperty()
                        {
                            Category = (int)eAdminUpdateProperty.CATEGORY.PREF,
                            Property = (int)ADMIN_PREF.ADRJOIN,
                            Value = "0"
                        };
                        caps.ListProperties.Add(pty);

                        ddlInterPP.Items[0].Attributes.Add("cplt", JsonConvert.SerializeObject(caps));

                        ddlInterPP.Items.Add(new ListItem(GenerateLinkParentTitle(ppTabName), "1"));
                        if (_tabInfos.InterPP || _tabInfos.TabType == TableType.ADR)
                        {
                            ddlInterPP.Items[1].Selected = true;
                        }
                        else
                        {
                            ddlInterPP.Items[0].Selected = true;
                        }
                        SubMenuPP.Controls.Add(ddlInterPP);
                        _panelContent.Controls.Add(SubMenuPP);

                        ddlInterPP.ToolTip = ddlInterPP.SelectedItem.Text;

                        ddlInterPP.Enabled = _updateEnabled;

                        //#51763 On ajoute le lien vers PP quand il y a la liaison directe ou liaison native
                        if (_tabInfos.InterPP || _tabInfos.TabType == TableType.ADR)
                        {

                            #region Insertion de l'icone et du lien vers l'admin du fichier parent
                            if (dicParentTablesIcon.ContainsKey((int)TableType.PP))
                            {
                                SubMenuPP.Controls.Add(GenerateParentIcon(dicParentTablesIcon[(int)TableType.PP]));
                            }
                            #endregion

                            if (_tabInfos.TabType != TableType.ADR)
                            {
                                SubMenuPP.Controls.Add(GenerateFeatures(ppTabName, LinkType.PP, true));
                            }
                            generateDragAliasCtrl(SubMenuPP, ppTabName, LinkType.PP, dicParentRelationsInFile);
                        }
                        #endregion
                    }

                    ddlInterPM = new DropDownList();
                    if (_tabInfos.TabType != TableType.INTERACTION)
                    {
                        #region INTERPM ET ADRJOIN

                        SubMenuPM = new Panel();


                        _panelContent.Controls.Add(SubMenuPM);
                        SubMenuPM.CssClass = "parentParam";

                        ddlInterPM.ID = "ddlInterPM";
                        ddlInterPM.CssClass = "relationparent";
                        ddlInterPM.Attributes.Add("did", this._tabInfos.DescId.ToString());
                        ddlInterPM.Attributes.Add("dsc", String.Concat((int)eAdminUpdateProperty.CATEGORY.DESC, "|", (int)eLibConst.DESC.INTERPM));

                        SubMenuPM.Controls.Add(ddlInterPM);
                        string sLabelNoLinkPMADR = "";

                        if (_tabInfos.TabType == TableType.TEMPLATE && _tabInfos.InterPP && _tabInfos.InterEVT)
                            sLabelNoLinkPMADR = GenerateNoLinkPMADRTitle(pmTabName, adrTabName);
                        else
                            sLabelNoLinkPMADR = GenerateNoLinkPPPMTitle(pmTabName);

                        #region pas de liaison 

                        li = new ListItem(sLabelNoLinkPMADR, "0");
                        ddlInterPM.Items.Add(li);

                        caps = new eAdminCapsule<eAdminUpdateProperty>();
                        caps.DescId = _tab;

                        // si INTERPM = false, INTERPMNEEDED = false
                        pty = new eAdminUpdateProperty()
                        {
                            Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                            Property = (int)eLibConst.DESC.INTERPMNEEDED,
                            Value = "0"
                        };
                        caps.ListProperties.Add(pty);

                        // si Si pas de liaison, ADRJOIN = false
                        pty = new eAdminUpdateProperty()
                        {
                            Category = (int)eAdminUpdateProperty.CATEGORY.PREF,
                            Property = (int)ADMIN_PREF.ADRJOIN,
                            Value = "0"
                        };
                        caps.ListProperties.Add(pty);

                        li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));

                        #endregion

                        #region liaison société

                        // pas liaison avec société quand il s'agit de Cibles Etendues 
                        if (!_tabInfos.IsExtendedTargetSubfile)
                        {
                            li = new ListItem(GenerateLinkParentTitle(pmTabName), "1");
                            ddlInterPM.Items.Add(li);

                            if (_tabInfos.InterPM || _tabInfos.TabType == TableType.ADR)
                            {
                                #region Insertion de l'icone et du lien vers l'admin du fichier parent
                                if (dicParentTablesIcon.ContainsKey((int)TableType.PM))
                                {
                                    SubMenuPM.Controls.Add(GenerateParentIcon(dicParentTablesIcon[(int)TableType.PM]));
                                }
                                #endregion

                                li.Selected = true;

                                if (_tabInfos.TabType != TableType.ADR)
                                {
                                    SubMenuPM.Controls.Add(GenerateFeatures(pmTabName, LinkType.PM, true));
                                }
                                generateDragAliasCtrl(SubMenuPM, pmTabName, LinkType.PM, dicParentRelationsInFile);

                            }

                            caps = new eAdminCapsule<eAdminUpdateProperty>();
                            caps.DescId = _tab;

                            pty = new eAdminUpdateProperty()
                            {
                                Category = (int)eAdminUpdateProperty.CATEGORY.PREF,
                                Property = (int)ADMIN_PREF.ADRJOIN,
                                Value = "0"
                            };
                            caps.ListProperties.Add(pty);

                            li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                        }
                        #endregion

                        #region liaison address               

                        if (_tabInfos.TabType == TableType.TEMPLATE && ((_tabInfos.InterPP && _tabInfos.InterEVT) || _tabInfos.IsExtendedTargetSubfile))
                        {
                            //liaison Adresse
                            li = new ListItem(GenerateLinkParentTitle(adrTabName), "0");
                            ddlInterPM.Items.Add(li);

                            if (_tabInfos.AdrJoin)
                                li.Selected = true;

                            caps = new eAdminCapsule<eAdminUpdateProperty>();
                            caps.DescId = _tab;

                            pty = new eAdminUpdateProperty()
                            {
                                Category = (int)eAdminUpdateProperty.CATEGORY.PREF,
                                Property = (int)ADMIN_PREF.ADRJOIN,
                                Value = "1"
                            };
                            caps.ListProperties.Add(pty);

                            //si ADRJOIN est à true INTERPMNEEDED est à false
                            pty = new eAdminUpdateProperty()
                            {
                                Category = (int)eAdminUpdateProperty.CATEGORY.DESC,
                                Property = (int)eLibConst.DESC.INTERPMNEEDED,
                                Value = "0"
                            };
                            caps.ListProperties.Add(pty);

                            li.Attributes.Add("cplt", JsonConvert.SerializeObject(caps));
                        }
                        #endregion

                        ddlInterPM.ToolTip = ddlInterPM.SelectedItem.Text;

                        ddlInterPM.Enabled = _updateEnabled;

                        #region Insertion de l'icone et du lien vers l'admin du fichier parent
                        if (_tabInfos.AdrJoin && dicParentTablesIcon.ContainsKey((int)TableType.ADR))
                        {
                            //#51763 l'icon de Addresse etait dans le menu de SubMenuEvt
                            SubMenuPM.Controls.Add(GenerateParentIcon(dicParentTablesIcon[(int)TableType.ADR]));
                            generateDragAliasCtrl(SubMenuPM, adrTabName, LinkType.ADR, dicParentRelationsInFile);
                        }
                        #endregion


                        #endregion
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                finally
                {
                    dal.CloseDatabase();
                }

                // Cibles étendues ou Addresse, les liste PP et Pm sont non modifiables
                if (_tabInfos.TabType == TableType.ADR || _tabInfos.IsExtendedTargetSubfile)
                {
                    // Pas de liaison parente pour adresse
                    if (_tabInfos.TabType == TableType.ADR)
                        _panelContent.Controls.Remove(SubMenuEVT);

                    if (_tabInfos.TabType != TableType.INTERACTION)
                    {
                        ddlInterPP.SelectedIndex = 1;
                        ddlInterPP.Enabled = false;

                        ddlInterPM.SelectedIndex = 1;
                        ddlInterPM.Enabled = false;
                    }
                }

            }


            if (_tabInfos.EdnType != EdnType.FILE_RELATION
                && _tabInfos.TabType != TableType.INTERACTION
                )
            {
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7365), "buttonAdminRelations", eResApp.GetRes(Pref, 7364), onclick: "javascript:nsAdmin.confRelations(" + (_updateEnabled ? "false" : "true") + ")");
                button.Generate(_panelContent);
            }

            if (_tabInfos.TabType == TableType.ADR)
            {
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 1851), "buttonAdminPmAdrMapping", eResApp.GetRes(Pref, 1852), onclick: "javascript:nsAdmin.confPmAdrMapping();");
                button.Generate(_panelContent);

            }

            //onglets relationnels
            if (_tabInfos.EdnType == EdnType.FILE_RELATION)
            {
                //boutton filtre relationnel
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7394), "", tooltiptext: eResApp.GetRes(Pref, 7395), onclick: String.Concat("nsAdminFile.openSpecFilter(", (int)TypeFilter.RELATION, ");"));
                button.Generate(_panelContent);
            }

            //cibles étendu
            if (_tabInfos.IsExtendedTargetSubfile)
            {
                //boutton Import étendu
                eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7496), "", tooltiptext: eResApp.GetRes(Pref, 7497), onclick: "javascript:nsAdmin.confExtendedTargetMappings();");
                button.Generate(_panelContent);
            }

            return true;
        }

        private HtmlGenericControl GenerateParentIcon(eAdminTableInfos.TabIcon tabIcon)
        {
            HtmlGenericControl icon = new HtmlGenericControl();
            eFontIcons.FontIcons font = eFontIcons.GetFontIcon(tabIcon.Icon);
            icon.Attributes.Add("class", String.Concat(font.CssName, " parentIcon"));
            if (!String.IsNullOrEmpty(tabIcon.IconColor))
                icon.Style.Add("color", tabIcon.IconColor);

            icon.Attributes.Add("onclick", String.Concat("nsAdmin.loadAdminFile(", tabIcon.DescId, ");"));

            return icon;
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

            Panel divFeatures = new Panel();
            divFeatures.CssClass = "relationsFeatures";
            divFeatures.Attributes.Add("data-active", (bOpened) ? "1" : "0");


            if (_tabInfos.EdnType != EdnType.FILE_RELATION)
            {
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
                dicRB.Add("0_0", eResApp.GetRes(Pref, 6965));
                dicRB.Add("1_1", eResApp.GetRes(Pref, 6966));

                eAdminRadioButtonField rb = new eAdminRadioButtonField(_tabInfos.DescId, String.Empty /*GenerateLinkTitle(tabParent)*/,
                    eAdminUpdateProperty.CATEGORY.DESC, intereventneededHashcode, rbGroupName, dicRB, value: (valueInterNeeded) ? "1_1" : "0_0", readOnly: !_updateEnabled);
                rb.SetFieldControlID(rbGroupName);
                rb.IsLabelBefore = true;
                rb.Generate(divFeatures);
                #endregion

            }
            return divFeatures;
        }

        /// <summary>
        /// Ajout d'un alias de la rubrique dans le corps de la page
        /// </summary>
        /// <param name="parentCtrl"></param>
        /// <param name="tabParent"></param>
        /// <param name="linkType"></param>
        private void generateDragAliasCtrl(Panel parentCtrl, string tabParent, LinkType linkType, Dictionary<int, int> dicParentRelationsInFile)
        {


            int iParentTab = 0;
            switch (linkType)
            {
                case LinkType.ParentTab:
                    iParentTab = _tabInfos.InterEVTDescid;
                    break;
                case LinkType.PP:
                    iParentTab = (int)TableType.PP;
                    break;
                case LinkType.PM:
                    iParentTab = (int)TableType.PM;
                    break;
                case LinkType.ADR:
                    iParentTab = (int)TableType.ADR;
                    break;
                default:
                    break;
            }

            //si un control existe déjà on quit
            if (dicParentRelationsInFile.ContainsValue(iParentTab))
                return;


            // Alias 
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            parentCtrl.Controls.Add(ul);
            HtmlGenericControl ctrl = eAdminFieldsTypesRenderer.BuildFieldType("icon-clone", String.Format(eResApp.GetRes(Pref, 8247), tabParent));
            ul.Controls.Add(ctrl);
            ctrl.Attributes.Add("class", "dragField");
            ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESC.GetHashCode(), "_", (int)eLibConst.DESC.FORMAT), ((int)FieldFormat.TYP_ALIASRELATION).ToString());
            ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.RES.GetHashCode(), "_", Pref.LangId), tabParent);
            ctrl.Attributes.Add(String.Concat(eAdminUpdateProperty.CATEGORY.DESCADV.GetHashCode(), "_", (int)DESCADV_PARAMETER.ALIAS_RELATION), iParentTab.ToString());
            eAdminFieldsTypesRenderer.MakeCtrlDraggable(ctrl);

        }

        private string GenerateLinkParentTitle(string resTab)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eResApp.GetRes(Pref, 6964));
            sb.Replace("<TABPARENT>", resTab);
            sb.Replace("<TAB>", _tabInfos.TableLabel);

            return sb.ToString();
        }

        private string GenerateNoLinkParentTitle()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eResApp.GetRes(Pref, 6967));

            return sb.ToString();
        }

        private string GenerateNoLinkPPPMTitle(string resTab)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eResApp.GetRes(Pref, 6969));
            sb.Replace("<TAB>", resTab);

            return sb.ToString();
        }

        private string GenerateNoLinkPMADRTitle(string resTabPM, string resTabADR)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eResApp.GetRes(Pref, 6968));
            sb.Replace("<TABPM>", resTabPM);
            sb.Replace("<TABADR>", resTabADR);

            return sb.ToString();
        }
    }
}
