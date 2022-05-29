using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System.Web.UI.WebControls;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet qui permet d'afficher le element de menu de droite
    /// </summary>
    abstract class eAbstractAdminTabMenuRenderer
    {
        eAdminTableInfos _tabInfos;
        ePref _pref;

        /// <summary>
        /// Liste des blocs ouvert pour l'onglet param de l'onglet (pour maintenir le statut entre les reload)
        /// </summary>
        public string[] ParamsOpenedBlock = null;

        protected eAbstractAdminTabMenuRenderer(ePref pref, eAdminTableInfos tabInfos)
        {
            _tabInfos = tabInfos;
            _pref = pref;
        }

        /// <summary>
        /// Créer objet qui permet de faire un rendu de menu admin des tables
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="infoTab"></param>
        /// <returns></returns>
        public static eAbstractAdminTabMenuRenderer GetAdminTabMenuRenderer(ePref pref, eAdminTableInfos infoTab)
        {
            switch (infoTab.TabType)
            {
                case TableType.PP:
                case TableType.PM:
                    return new eAdminPmPpMenuRenderer(pref, infoTab);
                case TableType.ADR:
                    return new eAdminAddressMenuRenderer(pref, infoTab);
                case TableType.HISTO:
                    return new eAdminHistoricMenuRenderer(pref, infoTab);
                case TableType.PJ:
                    return new eAdminPjMenuRenderer(pref, infoTab);
                case TableType.TEMPLATE:
                    if (infoTab.IsExtendedTargetSubfile)
                        return new eAdminTargetMenuRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_MAIL)
                        return new eAdminMailMenuRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_SMS)
                        return new eAdminSMSMenuRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_RELATION)
                        return new eAdminRelationMenuRenderer(pref, infoTab);
                    else if (infoTab.EdnType == EdnType.FILE_WEBTAB || infoTab.EdnType == EdnType.FILE_GRID)
                        return new eAdminWebTabFileMenuRenderer(pref, infoTab);
                    else
                        goto default;
                default:
                    // Par défaut, on fait un rendu complet du menu
                    return new eAdminGenericMenuRenderer(pref, infoTab);

            }

        }

        /// <summary>
        /// Fait un rendu du menu caractéristiques
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderFeatures(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFeaturesRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 6809), "", "", bOpen: ParamsOpenedBlock?.Contains("FeaturesPart") ?? true);


            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// Fait un rendu du menu relations
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderRelations(Panel menuContainer)
        {
            if (_tabInfos.DescId < (int)TableType.HISTO || _tabInfos.DescId == (int)TableType.INTERACTION)
            {

                eAdminRenderer renderer = eAdminRendererFactory.CreateAdminRelationsRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 1117), bOpen: ParamsOpenedBlock?.Contains("RelationsPart") ?? false);

                menuContainer.Controls.Add(renderer.PgContainer);
            }
        }

        /// <summary>
        /// Fait un rendu du menu performances
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderPerformances(Panel menuContainer) //
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminPerfRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 6811),
                 bOpen: ParamsOpenedBlock?.Contains("PerfPart") ?? false);
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// Fait un rendu du menu régles
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderRules(Panel menuContainer)
        {
            if (_tabInfos.EdnType == EdnType.FILE_PJ)
                return;

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminRightsAndRulesRenderer(_pref, _tabInfos,
                 bOpen: ParamsOpenedBlock?.Contains("RulesPart") ?? false);
            menuContainer.Controls.Add(renderer.PgContainer);

        }

        /// <summary>
        /// Fait un rendu du menu préfréneces
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderPreferences(Panel menuContainer)
        {

            if (_tabInfos.EdnType == EdnType.FILE_PJ)
                return;

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminPreferencesRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 445), bOpen: ParamsOpenedBlock?.Contains("PrefPart") ?? false);
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu Cartographie
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderCartography(Panel menuContainer)
        {

            if (_tabInfos.EdnType == EdnType.FILE_PJ)
                return;

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminMapRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 7105), bOpen: ParamsOpenedBlock?.Contains("MapPart") ?? false);
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu Référentiel de données
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderDataRepositories(Panel menuContainer)
        {
            if (_tabInfos.EdnType == EdnType.FILE_PJ)
                return;

            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminDataRepositoriesRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 8564), bOpen: ParamsOpenedBlock?.Contains("DataRepositoriesPart") ?? false); // TODORES
            menuContainer.Controls.Add(renderer.PgContainer);
        }


        public virtual void RenderExtensionsParam(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminExtensionsTabParamRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 7177));
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu Options de recherche
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderSearchOptions(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminSearchOptionsRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 7110));
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu Filtre Doublons
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderFiltersAndDuplicate(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminFiltersAndDuplicatesRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 6813));
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu Appartenance et traçabilité
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderTraceability(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminTraceabilityRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 6915));
            menuContainer.Controls.Add(renderer.PgContainer);

        }

        /// <summary>
        /// fait un rendu de menu  Raccourcis/traitements
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderTreatments(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminWebLinksAndTreatments(_pref, _tabInfos.DescId, eResApp.GetRes(_pref, 6815));
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu  Langues
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderLangues(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminBlockTranslationsRenderer(_pref, _tabInfos);
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu  Rapport
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderReports(Panel menuContainer)
        {
            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminBlockRenderer(_pref, _tabInfos, eResApp.GetRes(_pref, 155));
            menuContainer.Controls.Add(renderer.PgContainer);
        }

        /// <summary>
        /// fait un rendu de menu RGPD
        /// </summary>
        /// <param name="menuContainer"></param>
        public virtual void RenderDataQuality(Panel menuContainer)
        {
            if (_tabInfos.EdnType == EdnType.FILE_PJ)
                return;


            eAdminRenderer renderer = eAdminRendererFactory.CreateAdminDataQualityRenderer(_pref, _tabInfos);
            menuContainer.Controls.Add(renderer.PgContainer);
        }
    }
}
