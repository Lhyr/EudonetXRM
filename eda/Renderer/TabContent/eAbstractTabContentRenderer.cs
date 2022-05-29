using System;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element du contenu de menu de droite
    /// </summary>
    abstract class eAbstractTabContentRenderer
    {
        eAdminTableInfos _tabInfos;
        ePref _pref;

        protected eAbstractTabContentRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            _tabInfos = tabInfos;
            _pref = pref;
        }

        /// <summary>
        /// Créer objet qui permet de faire un rendu de menu content admin des tables
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="_tabInfos"></param>
        /// <returns></returns>
        public static eAbstractTabContentRenderer GetAdminTabContentRenderer(ePref pref, eAdminTableInfos infoTab)
        {
            switch (infoTab.TabType)
            {
                case TableType.PP:
                case TableType.PM:
                    return new eAdminPmPpContentRenderer(pref, infoTab);

                case TableType.ADR:
                    return new eAdminAddressContentRenderer(pref, infoTab);

                case TableType.TEMPLATE:
                    if (infoTab.EdnType == EdnType.FILE_MAIL)
                        return new eAdminMailContentRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_SMS)
                        return new eAdminSMSContentRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_RELATION)
                        return new eAdminRelationContentRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_WEBTAB || infoTab.EdnType == EdnType.FILE_GRID)
                        return new eAdminWebTabContentRenderer(pref, infoTab);

                    return new eAdminTplContentRenderer(pref, infoTab);
                case TableType.HISTO:
                    return new eAdminHistoricContentRenderer(pref, infoTab);

                case TableType.PJ:
                    return new eAdminPjContentRenderer(pref, infoTab);

                case TableType.INTERACTION:
                    return new eAdminInteractionContentRenderer(pref, infoTab);

                default:
                    // Par défaut, on fait un rendu complet du contenu  
                    return new eAdminGenericContentRenderer(pref, infoTab);
            }
        }

        /// <summary>
        /// Fait un rendu du menu contenu Columns
        /// </summary>
        /// <param name="contentMenu"></param>
        public virtual void RenderColumns(Panel contentMenu)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminColumnsParamRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 732));
            contentMenu.Controls.Add(renderer.PgContainer);
        }


        /// <summary>
        /// Fait un rendu du menu contenu Entête
        /// </summary>
        /// <param name="contentMenu"></param>
        public virtual void RenderHeader(Panel contentMenu, String[] OpenedBlocks)
        {
            eAdminBlockRenderer renderer = eAdminRendererFactory.CreateAdminHeaderRenderer(_pref, eResApp.GetRes(_pref, 8263), OpenedBlocks);
            contentMenu.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// Fait un rendu du menu contenu rubriques
        /// </summary>
        /// <param name="contentMenu"></param>
        public virtual void RenderField(Panel contentMenu)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFieldsTypesRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 20));
            contentMenu.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// Fait un rendu du menu contenu onglet web
        /// </summary>
        /// <param name="contentMenu"></param>
        public virtual void RenderWebTab(Panel contentMenu)
        {
            eAdminBlockRenderer renderer = eAdminRendererFactory.CreateAdminWebTabRenderer(_pref);
            contentMenu.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// Fait un rendu du menu contenu signet web
        /// </summary>
        /// <param name="contentMenu"></param>
        public virtual void RenderWebBkm(Panel contentMenu)
        {
            eAdminBlockRenderer renderer = eAdminRendererFactory.CreateAdminWebBkmRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 6917));
            contentMenu.Controls.Add(renderer.PgContainer);
        }
    }
}
