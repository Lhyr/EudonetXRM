using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminBkmRelationsRenderer : eAdminBlockRenderer
    {
        private eAdminTableInfos _bkmTabInfos;
        private eAdminTableInfos _ppInfos;
        private eAdminTableInfos _pmInfos;
        private eAdminTableInfos _adrInfos;
        private eAdminTableInfos _evtInfos;
        private int _bkmTab;

        /*
        private string parentTabName = String.Empty;
        private string ppTabName = String.Empty;
        private string pmTabName = String.Empty;        
        private string adrTabName = String.Empty;
        */

        private eRes _res;

        private eAdminBkmRelationsRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
            : base(pref, tabInfos, title, titleInfo)
        {
            _tab = _tabInfos.DescId;
            this.BlockTitleTooltip = tooltip;

            _bkmTabInfos = bkmTabInfos;
            _bkmTab = _bkmTabInfos.DescId;
        }

        public static eAdminBkmRelationsRenderer CreateAdminBkmRelationsRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo, string tooltip)
        {
            eAdminBkmRelationsRenderer features = new eAdminBkmRelationsRenderer(pref, tabInfos, bkmTabInfos, title, titleInfo, tooltip);
            return features;
        }

        protected override bool Init()
        {
            if (base.Init())
            {
                if (_bkmTabInfos.TabType != TableType.PP && _bkmTabInfos.TabType != TableType.PM && _bkmTabInfos.TabType != TableType.ADR)
                {
                    _ppInfos = new eAdminTableInfos(Pref, (int)TableType.PP);
                    _pmInfos = new eAdminTableInfos(Pref, (int)TableType.PM);
                    _adrInfos = new eAdminTableInfos(Pref, (int)TableType.ADR);
                    if (_bkmTabInfos.InterEVT)
                        _evtInfos = new eAdminTableInfos(Pref, _bkmTabInfos.InterEVTDescid);
                }
                return true;
            }

            return false;
        }

        // <summary>Construction du bloc Relations</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                _panelContent.ID = "edaBkmRelationsMenu";

                if (_bkmTabInfos.TabType != TableType.PP && _bkmTabInfos.TabType != TableType.PM && _bkmTabInfos.TabType != TableType.ADR)
                {
                    //Lien vers EVT
                    Panel SubMenuEVT;
                    if (_bkmTabInfos.InterEVT)
                        SubMenuEVT = CreateBkmRelationSubMenu(GenerateLinkParentTitle(_evtInfos.TableLabel), nTabDescid: _bkmTabInfos.InterEVTDescid, sIcon: _evtInfos.Icon, sColor: _evtInfos.IconColor);
                    else
                        SubMenuEVT = CreateBkmRelationSubMenu(GenerateNoLinkParentTitle());
                    _panelContent.Controls.Add(SubMenuEVT);

                    //Lien vers PP
                    Panel SubMenuPP;
                    if (_bkmTabInfos.InterPP)
                        SubMenuPP = CreateBkmRelationSubMenu(GenerateLinkParentTitle(_ppInfos.TableLabel), nTabDescid: (int)TableType.PP, sIcon: _ppInfos.Icon, sColor: _ppInfos.IconColor);
                    else
                        SubMenuPP = CreateBkmRelationSubMenu(GenerateNoLinkPPPMTitle(_ppInfos.TableLabel));
                    _panelContent.Controls.Add(SubMenuPP);

                    //Lien vers PM/ADR
                    Panel SubMenuPM;
                    if (_bkmTabInfos.InterPM)
                        SubMenuPM = CreateBkmRelationSubMenu(GenerateLinkParentTitle(_pmInfos.TableLabel), nTabDescid: (int)TableType.PM, sIcon: _pmInfos.Icon, sColor: _pmInfos.IconColor);
                    else
                    {
                        string subMenuPMTitle = String.Empty;

                        if (_bkmTabInfos.TabType == TableType.EVENT)
                            subMenuPMTitle = GenerateNoLinkPPPMTitle(_pmInfos.TableLabel);

                        if (_bkmTabInfos.TabType == TableType.TEMPLATE)
                            subMenuPMTitle = GenerateNoLinkPMADRTitle(_pmInfos.TableLabel, _adrInfos.TableLabel);

                        SubMenuPM = CreateBkmRelationSubMenu(subMenuPMTitle);
                    }
                    _panelContent.Controls.Add(SubMenuPM);
                }

                //Lien Relations Aditionnelles SuperAdmin
                if (Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                {
                    eAdminField button = new eAdminButtonField(eResApp.GetRes(Pref, 7394), "buttonAdminBkmRelations", onclick: String.Concat("nsAdmin.confBkmRelationsSQLFilters(", _bkmTab, "); "));
                    button.Generate(_panelContent);
                }

                return true;
            }
            return false;
        }

        private string GenerateLinkParentTitle(string resTab)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(eResApp.GetRes(Pref, 6964));
            sb.Replace("<TABPARENT>", resTab);
            sb.Replace("<TAB>", _bkmTabInfos.TableLabel);

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


        protected Panel CreateBkmRelationSubMenu(string sBlockTitle,
                                            int nTabDescid = 0,
                                            string sBlockTitleInfo = "",
                                            string sBlockTitleTooltip = "",
                                            string sBlockID = "",
                                            int nHeaderLevel = 4,
                                            string sIcon = "",
                                            string sColor = ""
                                            )
        {
            Panel blockPanel = new Panel();
            blockPanel.CssClass = "paramPart";
            if (!String.IsNullOrEmpty(sBlockID))
                blockPanel.ID = sBlockID;

            blockPanel.Controls.Add(CreateBkmRelationSubMenuHeader(sBlockTitle, nTabDescid, sBlockTitleInfo, sBlockTitleTooltip, nHeaderLevel, sIcon, sColor));

            return blockPanel;
        }

        /// <summary>
        /// Creation du header contenant le titre et éventuellement le sous-titre
        /// </summary>
        /// <param name="sBlockTitle">Titre</param>
        /// <param name="sBlockTitleInfo">Sous Titre</param>
        /// <param name="sBlockTitleTooltip">Infobulle</param>
        /// <param name="nLevel">Taille du titre</param>
        /// <returns></returns>
        protected HtmlGenericControl CreateBkmRelationSubMenuHeader(string sBlockTitle, int tabDescid = 0, string sBlockTitleInfo = "", string sBlockTitleTooltip = "", int nLevel = 4, string icon = "", string color = "")
        {
            HtmlGenericControl header = new HtmlGenericControl("header");            

            if (nLevel < 1)
                nLevel = 1;
            if (nLevel > 6)
                nLevel = 6;

            HtmlGenericControl htmlTitle = new HtmlGenericControl("h" + nLevel.ToString());
            //htmlTitle.InnerText = sBlockTitle;
            header.Controls.Add(htmlTitle);

            HtmlGenericControl text = new HtmlGenericControl("label");
            text.ID = "bkmRelationsSubMenuLabel";
            text.InnerText = sBlockTitle;
            htmlTitle.Controls.Add(text);

            if (tabDescid != 0)
            {
                HtmlGenericControl link = new HtmlGenericControl("span");
                link.Attributes.Add("onclick", String.Concat("nsAdmin.loadAdminFile(", tabDescid, ")"));
                htmlTitle.Controls.Add(link);                

                string iconClass = "icon-param-onglet";
                if (!String.IsNullOrEmpty(icon))
                {
                    eFontIcons.FontIcons font = eFontIcons.GetFontIcon(icon);
                    iconClass = font.CssName;
                }
                link.Attributes.Add("class", String.Concat(iconClass, " linkAdminTab"));

                if (!String.IsNullOrEmpty(color))
                    link.Style.Add("color", color);
            }


            if (!String.IsNullOrEmpty(sBlockTitleInfo))
            {
                HtmlGenericControl htmlInfo = new HtmlGenericControl("p");
                htmlInfo.Attributes.Add("class", "info");
                htmlInfo.InnerText = sBlockTitleInfo;
                header.Controls.Add(htmlInfo);
            }
            if (!String.IsNullOrEmpty(sBlockTitleTooltip))
            {
                header.Attributes.Add("title", sBlockTitleTooltip);
            }
            
            return header;
        }
    }
}