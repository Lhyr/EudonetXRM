using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Création d'un bloc menu pour l'admin
    /// </summary>
    public class eAdminBlockRenderer : eAdminRenderer
    {
        /// <summary>Titre du bloc</summary>
        public String BlockTitle { get; private set; }
        /// <summary>Sous-titre du bloc</summary>
        public String BlockTitleInfo { get; private set; }
        /// <summary>
        /// Infobulle au survol du titre du bloc
        /// </summary>
        public String BlockTitleTooltip { get; protected set; }
        /// <summary>ID du bloc</summary>
        public String BlockID { get; private set; }
        /// <summary>
        /// The panel content
        /// </summary>
        protected Panel _panelContent;
        /// <summary>
        /// Panel du contenu
        /// </summary>
        public Panel PanelContent { get { return _panelContent; } }
        /// <summary>
        /// Object eAdminTableInfos
        /// </summary>
        protected eAdminTableInfos _tabInfos;
        /// <summary>
        /// Le bloc est-il déplié ?
        /// </summary>
        public Boolean OpenedBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminBlockRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tabInfos">The tab infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="idBlock">The identifier block.</param>
        /// <param name="bOpenedBlock">if set to <c>true</c> [b opened block].</param>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        protected eAdminBlockRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo = "", String idBlock = "", Boolean bOpenedBlock = false)
        {

            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            this.BlockTitle = title;
            this.BlockTitleInfo = titleInfo;
            this.BlockID = idBlock;
            this._tabInfos = tabInfos;
            if (tabInfos != null)
                this._tab = tabInfos.DescId;
       
            this.Pref = pref;
            this.OpenedBlock = bOpenedBlock;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminBlockRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="idBlock">The identifier block.</param>
        /// <param name="bOpenedBlock">if set to <c>true</c> [b opened block].</param>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        protected eAdminBlockRenderer(ePref pref, int tab, String title, String titleInfo = "", String idBlock = "", Boolean bOpenedBlock = false)
        {
            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            this.BlockTitle = title;
            this.BlockTitleInfo = titleInfo;
            this.BlockID = idBlock;
            this._tab = tab;
            this._tabInfos = null;

            this.Pref = pref;
            this.OpenedBlock = bOpenedBlock;
        }


        
        /// <summary>
        /// Génère un bloc menu droit
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tabInfos"></param>
        /// <param name="title"></param>
        /// <param name="titleInfo"></param>
        /// <param name="idBlock"></param>
        /// <param name="bOpenedBlock"></param>
        /// <returns></returns>
        public static eAdminBlockRenderer CreateAdminBlockRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo = "", String idBlock = "", Boolean bOpenedBlock = false)
        {
            eAdminBlockRenderer blockRdr = new eAdminBlockRenderer(pref, tabInfos, title, titleInfo, idBlock, bOpenedBlock: bOpenedBlock);
            return blockRdr;
        }



        /// <summary>
        /// Bloc de base
        /// </summary>
        protected virtual void BuildMainContent()
        {
            Panel blockPart;
            Panel panelContent;

            CreateCollapsibleMenu(out blockPart, out panelContent, OpenedBlock, this.BlockTitle, this.BlockTitleInfo, this.BlockTitleTooltip, this.BlockID);
            this._pgContainer = blockPart;
            this._panelContent = panelContent;

        }

        /// <summary>
        /// pour les blocs, on retourne le pgocntent
        /// </summary>
        /// <returns></returns>
        public override Panel GetContents()
        {
            return _panelContent;
        }

        /// <summary>Construction du squelette du bloc : header + div contenu</summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            BuildMainContent();

            return true;
        }
        /// <summary>
        /// Construit des objets html annexes/place des appel JS d'apres chargement
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {

            if (base.End())
            {

                DicContent.Add("MainPanel", new Content() { Ctrl = _pgContainer });
                DicContent.Add("Content", new Content() { Ctrl = _panelContent });

                return true;
            }
            else
                return false;


        }


        /// <summary>
        /// Creation d'un menu pliant
        /// </summary>
        /// <param name="blockPanel">Control englobant le menu</param>
        /// <param name="contentPanel">Control englobant le contenu du menu</param>
        /// <param name="bOpenedBlock">Indique si le menu est affiché plié ou déplié</param>
        /// <param name="sBlockTitle">Titre du menu</param>
        /// <param name="sBlockTitleInfo">Sous Titre du menu</param>
        /// <param name="sBlockTitleTooltip">Infobulle</param>
        /// <param name="sBlockID">Id de menu</param>
        /// <param name="nHeaderLevel">The header level.</param>
        /// <param name="bRights">if set to <c>true</c> [rights].</param>
        /// <param name="bBgBlue">if set to <c>true</c> [bg blue].</param>
        /// <returns></returns>
        protected bool CreateCollapsibleMenu(out Panel blockPanel,
                                            out Panel contentPanel,
                                            bool bOpenedBlock,
                                            string sBlockTitle,
                                            string sBlockTitleInfo = "",
                                            string sBlockTitleTooltip = "",
                                            string sBlockID = "",
                                            int nHeaderLevel = 4,
                                            bool bRights = false,
                                            bool bBgBlue = false)
        {
            contentPanel = new Panel();
            blockPanel = new Panel();
            blockPanel.CssClass = "paramPart";
            if (!String.IsNullOrEmpty(sBlockID))
                blockPanel.ID = sBlockID;

            blockPanel.Controls.Add(CreateCollapsibleMenuHeader(bOpenedBlock, sBlockTitle, sBlockTitleInfo, sBlockTitleTooltip, nHeaderLevel, bRights: bRights, bBgBlue: bBgBlue));

            //contentPanel.CssClass = !bRights? "paramPartContent" : "paramPartContent blueParamPartContent";
            contentPanel.CssClass = "paramPartContent";
            blockPanel.Controls.Add(contentPanel);

            if (bOpenedBlock)
            {
                contentPanel.Attributes.Add("data-active", "1");
                contentPanel.Attributes.Add("eactive", "1");
            }


            return true;
        }

        /// <summary>
        /// Retourne le nombre de notifications
        /// </summary>
        /// <param name="tab">The tab.</param>
        /// <param name="descid">The descid.</param>
        /// <returns></returns>
        protected string GetNotificationNumber(Int32 tab, Int32 descid)
        {

            eAdminAutomationList list = eAdminAutomationList.GetListMainAutomation(Pref, tab, descid, 1, AutomationType.NOTIFICATION);
            if (list.Generate() && list.ListRecords != null)
                return list.ListRecords.Count.ToString();

            return "--";

        }


        /// <summary>
        /// Creation du header contenant le titre et éventuellement le sous-titre
        /// </summary>
        /// <param name="bOpenedBlock">Indique si le menu est affiché plié ou déplié</param>
        /// <param name="sBlockTitle">Titre</param>
        /// <param name="sBlockTitleInfo">Sous Titre</param>
        /// <param name="sBlockTitleTooltip">Infobulle</param>
        /// <param name="nLevel">Taille du titre</param>
        /// <param name="bRights">if set to <c>true</c> [rights].</param>
        /// <param name="bBgBlue">if set to <c>true</c> [bg blue].</param>
        /// <returns></returns>
        protected virtual HtmlGenericControl CreateCollapsibleMenuHeader(bool bOpenedBlock, string sBlockTitle, string sBlockTitleInfo = "", string sBlockTitleTooltip = "", int nLevel = 4, bool bRights = false, bool bBgBlue = false)
        {
            HtmlGenericControl header = new HtmlGenericControl("header");
            HtmlGenericControl iconArrow = new HtmlGenericControl("div");
            iconArrow.Attributes.Add("class", (bOpenedBlock) ? bRights ? "icon-unvelop" : "icon-caret-down" : bRights ? "icon-develop" : "icon-caret-right");
            header.Controls.Add(iconArrow);

            if (nLevel < 1)
                nLevel = 1;
            if (nLevel > 6)
                nLevel = 6;
            HtmlGenericControl htmlTitle = new HtmlGenericControl("h" + nLevel.ToString());
            htmlTitle.InnerText = sBlockTitle;
            header.Controls.Add(htmlTitle);
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

            if (bBgBlue)
                header.Attributes.Add("class", "btnLink");
            return header;
        }

    }





}