using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminTabBkmParametersRenderer : eAdminRenderer
    {
        public Int32 Tab { get; protected set; }
        public Int32 BkmDescId { get; protected set; }
        private eAdminTableInfos _tabInfos;
        private eAdminTableInfos _bkmTabInfos;

        protected eAdminTabBkmParametersRenderer(ePref pref, Int32 nTab, Int32 nBkmDescId)
        {
            Pref = pref;
            BkmDescId = nBkmDescId;

            _tabInfos = new eAdminTableInfos(pref, nTab);
            Tab = _tabInfos.DescId;
        }




        public static eAdminTabBkmParametersRenderer CreateAdminTabBkmParamsRenderer(ePref pref, Int32 nTab, Int32 nBkmDescId)
        {
            return new eAdminTabBkmParametersRenderer(pref, nTab, nBkmDescId);
        }






        protected override bool Init()
        {
            // Signet Standard
            // Signet Doublons
            if (BkmDescId % 100 == 0
                || BkmDescId == (int)TableType.DOUBLONS
                || BkmDescId % 100 == AllField.ATTACHMENT.GetHashCode())
            {
                _bkmTabInfos = new eAdminTableInfos(Pref, BkmDescId);
            }


            return base.Init();
        }

        protected override bool Build()
        {
            String error = String.Empty;

            _pgContainer.ID = "paramTab2";
            _pgContainer.Attributes.Add("class", "paramBlock");
            _pgContainer.Style.Add(HtmlTextWriterStyle.Display, "none");

            // Titre
            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.ID = "paramTitleTab2";
            title.InnerHtml = eResApp.GetRes(Pref, 7388);
            _pgContainer.Controls.Add(title);

            // Signet Standard
            // Signet Doublon
            if (BkmDescId % 100 == 0 || BkmDescId == (int)TableType.DOUBLONS)
            {
                String tabTooltip = String.Empty;
                String tabType = string.Empty;

                if (_bkmTabInfos.EdnType == EdnType.FILE_GRID)
                    tabType = "Onglet signet Grille";
                else
                    tabType = eAdminTools.GetTabTypeName(_bkmTabInfos, Pref, out tabTooltip);

                HtmlGenericControl panelTab = new HtmlGenericControl("p");
                panelTab.Attributes.Add("class", "info");
                panelTab.InnerText = tabType;
                panelTab.Attributes.Add("title", tabTooltip);

                _pgContainer.Controls.Add(title);
                _pgContainer.Controls.Add(panelTab);

                #region Contenu
                if (_bkmTabInfos.EdnType == EdnType.FILE_BKMWEB)
                {
                    //signet web 
                    BuildStandard();

                }
                else if (_bkmTabInfos.EdnType == EdnType.FILE_GRID)
                {
                    // signet grille
                    BuildBkmGrid();
                }
                else if (_bkmTabInfos.DescId == (int)TableType.DOUBLONS
               || _bkmTabInfos.TabType == TableType.PP
               || _bkmTabInfos.TabType == TableType.PM
               || _bkmTabInfos.TabType == TableType.EVENT
               || _bkmTabInfos.TabType == TableType.TEMPLATE
               || _bkmTabInfos.TabType == TableType.ADR
               || _bkmTabInfos.TabType == TableType.DOUBLONS
               || _bkmTabInfos.TabType == TableType.HISTO)
                {
                    BuildStandard();
                }
                else if (_bkmTabInfos.TabType == TableType.BOUNCEMAIL // Emails non remis
                    || _bkmTabInfos.TabType == TableType.CAMPAIGNSTATS
                    || _bkmTabInfos.TabType == TableType.CAMPAIGNSTATSADV
                    || _bkmTabInfos.TabType == TableType.TRACKLINK
                    || _bkmTabInfos.TabType == TableType.UNSUBSCRIBEMAIL)
                {
                    BuildCampaignSysBKM();
                }
                else
                {
                    BuildDefault();
                }

                #endregion
            }
            else if (BkmDescId % 100 == AllField.BKM_PM_EVENT.GetHashCode()) // Signet EVENT de l'EVENT
            {
                BuildEventBKM();
            }
            else if (BkmDescId % 100 == AllField.ATTACHMENT.GetHashCode()) // Annexes
            {
                BuildAnnexesBKM();
            }
            else
            {
                BuildDefault();
            }

            return base.Build();
        }

        /// <summary>
        /// Paramètrage du Signet Grille
        /// </summary>
        protected void BuildBkmGrid()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            eAdminRenderer renderer;

            // Caractéristiques
            renderer = eAdminRendererFactory.CreateAdminBkmGridFeaturesRenderer(Pref, _tabInfos, _bkmTabInfos, eResApp.GetRes(Pref, 6809));
            panelContent.Controls.Add(renderer.PgContainer);

            // Relations          
            //renderer = eAdminRendererFactory.CreateAdminBkmRelationsRenderer(Pref, _tabInfos, _bkmTabInfos, eResApp.GetRes(Pref, 1117));
            //panelContent.Controls.Add(renderer.PgContainer);

            //Droits et comportements conditionnels
            renderer = eAdminRendererFactory.CreateAdminRightsAndRulesRenderer(Pref, _bkmTabInfos, _tabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            //Langues
            renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, _bkmTabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            _pgContainer.Controls.Add(panelContent);
        }

        protected void BuildStandard()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            eAdminRenderer renderer;

            // Caractéristiques
            renderer = eAdminRendererFactory.CreateAdminBkmFeaturesRenderer(Pref, _tabInfos, _bkmTabInfos, eResApp.GetRes(Pref, 6809));
            panelContent.Controls.Add(renderer.PgContainer);
            
            // Relations
            if (_bkmTabInfos.DescId != (int)TableType.DOUBLONS
                && _bkmTabInfos.EdnType != EdnType.FILE_BKMWEB
                //  && _bkmTabInfos.EdnType != EdnType.FILE_GRID
                && (_bkmTabInfos.TabType == TableType.PP
                    || _bkmTabInfos.TabType == TableType.PM
                    || _bkmTabInfos.TabType == TableType.EVENT
                    || _bkmTabInfos.TabType == TableType.TEMPLATE)
                && !_bkmTabInfos.IsEventStep)
            {
                renderer = eAdminRendererFactory.CreateAdminBkmRelationsRenderer(Pref, _tabInfos, _bkmTabInfos, eResApp.GetRes(Pref, 1117));
                panelContent.Controls.Add(renderer.PgContainer);
            }

            //Droits et comportements conditionnels
            if (!_bkmTabInfos.IsEventStep)
            {
                renderer = eAdminRendererFactory.CreateAdminRightsAndRulesRenderer(Pref, _bkmTabInfos, _tabInfos);
                panelContent.Controls.Add(renderer.PgContainer);
            }

            //Langues
            renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, _bkmTabInfos);
            panelContent.Controls.Add(renderer.PgContainer);


            //Préférences
            //TODO - À spécifier

            //Cartographie
            //TODO - À spécifier

            //Raccourcis et traitements spécifiques 
            //TODO - À spécifier

            _pgContainer.Controls.Add(panelContent);
        }

        protected void BuildAnnexesBKM()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            eAdminRenderer renderer;

            // Caractéristiques
            renderer = eAdminRendererFactory.CreateAdminBkmFeaturesRenderer(Pref, _tabInfos, _bkmTabInfos, eResApp.GetRes(Pref, 6809));
            panelContent.Controls.Add(renderer.PgContainer);

            //Droits et comportements conditionnels
            renderer = eAdminRendererFactory.CreateAdminRightsAndRulesRenderer(Pref, _bkmTabInfos, _tabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            //Securité
            renderer = eAdminRendererFactory.CreateAdminBlockSecurityRenderer(Pref, _bkmTabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            //Langues
            renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, _bkmTabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            _pgContainer.Controls.Add(panelContent);
        }

        /// <summary>
        /// Construit le menu Paramètres du signet pour les signets système de Campagnes
        /// </summary>
        protected void BuildCampaignSysBKM()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            eAdminRenderer renderer;

            //Langues
            renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(Pref, _bkmTabInfos);
            panelContent.Controls.Add(renderer.PgContainer);

            _pgContainer.Controls.Add(panelContent);
        }

        /// <summary>
        /// Construit seulement le bloc Droits
        /// </summary>
        protected void BuildEventBKM()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            eAdminFieldInfos fInfos = eAdminFieldInfos.GetAdminFieldInfos(Pref, BkmDescId);
            eRenderer rdr = eAdminRendererFactory.CreateAdminFieldRightsAndRulesRenderer(Pref, fInfos);

            panelContent.Controls.Add(rdr.PgContainer);

            _pgContainer.Controls.Add(panelContent);
        }

        protected void BuildDefault()
        {
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";

            _pgContainer.Controls.Add(panelContent);
        }
    }
}
