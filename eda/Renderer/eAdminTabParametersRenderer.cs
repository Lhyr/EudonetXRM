using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Renderer des "paramètres"
    /// </summary>
    public class eAdminTabParametersRenderer : eAdminTabFieldRenderer
    {

        eAbstractAdminTabMenuRenderer _tabMenu;
        eAdminTableInfos _tabInfos;
        string[] _openedBlocks = null;

        /// <summary>
        /// blocs "ouverts"
        /// </summary>
        public string[] OpenedBlocks
        {
            get
            {
                return _openedBlocks;
            }

            set
            {
                _openedBlocks = value;
            }
        }

        /// <summary>
        /// Enum pour les différentes parties de ce Renderer
        /// </summary>
        public enum TabParameterPart
        {
            /// <summary>
            /// caractéristiques "apparences"
            /// </summary>
            FEATURES,

            /// <summary>
            /// gestions des relations
            /// </summary>
            RELATIONS,

            /// <summary>
            /// options de comptage de fiches
            /// </summary>
            PERFORMANCES,

            /// <summary>
            /// droits et règles
            /// </summary>
            RULES,

            /// <summary>
            /// filtres des doublons/rapide/...
            /// </summary>
            FILTERS,

            /// <summary>
            /// historique
            /// </summary>
            TRACEABILITY,

            /// <summary>
            /// ?
            /// </summary>
            SHORTCUTS_TREATMENTS,

            /// <summary>
            /// localisation
            /// </summary>
            GLOBALIZATION
        }


        private eAdminTabParametersRenderer(ePref pref, Int32 nTab, string[] openedBlocks = null)
        {
            Pref = pref;
            DescId = nTab;
            OpenedBlocks = openedBlocks;
            _tabInfos = new eAdminTableInfos(pref, nTab);
            _tabMenu = eAbstractAdminTabMenuRenderer.GetAdminTabMenuRenderer(Pref, _tabInfos);
            _tabMenu.ParamsOpenedBlock = OpenedBlocks;
        }


        /// <summary>
        /// Instanciation d'un renderer admin table pour le bloc "paramètre de l'onglet"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="openedBlocks"></param>
        /// <returns></returns>
        public static eAdminTabParametersRenderer CreateAdminTabParametersRenderer(ePref pref, Int32 nTab, string[] openedBlocks = null)
        {
            return new eAdminTabParametersRenderer(pref, nTab, openedBlocks);
        }

        /// <summary>Construit le html de l'objet demandé</summary>
        /// <returns></returns>
        protected override Boolean Build()
        {
            // Titre
            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.InnerText = eResApp.GetRes(Pref, 6808);

            #region Contenu
            Panel panelContent = new Panel();
            panelContent.CssClass = "paramBlockContent";


            String tabTooltip = String.Empty;
            String tabType = eAdminTools.GetTabTypeName(_tabInfos, Pref, out tabTooltip);


            HtmlGenericControl panelTab = new HtmlGenericControl("p");
            panelTab.Attributes.Add("class", "info");
            panelTab.InnerText = tabType;
            panelTab.Attributes.Add("title", tabTooltip);


            // Caractéristiques
            _tabMenu.RenderFeatures(panelContent);

            // Relations
            if (
                    !_tabInfos.IsEventStep
                && _tabInfos.TabType != EudoQuery.TableType.CAMPAIGNSTATSADV
                )
            {
                _tabMenu.RenderRelations(panelContent);

                // Performances
                _tabMenu.RenderPerformances(panelContent);

                // Règles
                _tabMenu.RenderRules(panelContent);

                // Préférences
                _tabMenu.RenderPreferences(panelContent);

                // Infobulle de position et Cartographie             
                _tabMenu.RenderCartography(panelContent);

                // Référentiel de données
                _tabMenu.RenderDataRepositories(panelContent);

                // Administration des extensions (paramètres propres à chaque table, Teams par ex.)
                eExtension eRegisteredExt = eExtension.GetExtensionByCode(_ePref, eLibConst.EXTENSION.TEAMS.ToString());
                if (eRegisteredExt?.Status == EXTENSION_STATUS.STATUS_READY 
                    &&eFeaturesManager.IsFeatureAvailable(_ePref, eConst.XrmFeature.GraphTeams)
                    &&_tabInfos.EdnType == EudoQuery.EdnType.FILE_MAIN)
                {
                    //TODOKHA : n'afficher que si l'extension est activée.
                    _tabMenu.RenderExtensionsParam(panelContent);
                }


                // Options de recherche
                _tabMenu.RenderSearchOptions(panelContent);

                // Filtres et doublons           
                _tabMenu.RenderFiltersAndDuplicate(panelContent);

                // Appartenance et traçabilité
                _tabMenu.RenderTraceability(panelContent);

                // Raccourcis/traitements           
                _tabMenu.RenderTreatments(panelContent);

                // Langues
                _tabMenu.RenderLangues(panelContent);

                // Rapports
                _tabMenu.RenderReports(panelContent);

                // RGPD
                _tabMenu.RenderDataQuality(panelContent);
            }
            else if (_tabInfos.TabType == EudoQuery.TableType.CAMPAIGNSTATSADV)
            {

                // Langues
                _tabMenu.RenderLangues(panelContent);
            }
            #endregion

            _pgContainer.ID = "paramTab3";
            _pgContainer.CssClass = "paramBlock";
            _pgContainer.Style.Add("display", "block");

            _pgContainer.Controls.Add(title);

            _pgContainer.Controls.Add(panelTab);
            _pgContainer.Controls.Add(panelContent);


            return true;
        }
    }
}