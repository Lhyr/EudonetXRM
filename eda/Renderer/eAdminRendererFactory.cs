using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.Teams;
using Newtonsoft.Json;
using System.Web.UI;
using System.Web.UI.WebControls;
using Com.Eudonet.Xrm.eda.Renderer.StoreParameters;
using Com.Eudonet.Xrm.eda.Renderer.ExtensionParameters;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Bibliothèque de fonctions de génération des différents renderer inhérents à l'admin
    /// </summary>
    public static class eAdminRendererFactory
    {

        /// <summary>
        /// Bloc Admin table : Paramètres de l'onglet
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="nTab"></param>
        /// <param name="openedBlocks"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminTabParametersRenderer(ePref Pref, Int32 nTab, string[] openedBlocks = null)
        {
            eAdminTabParametersRenderer rdr = eAdminTabParametersRenderer.CreateAdminTabParametersRenderer(Pref, nTab, openedBlocks);
            rdr.Generate();
            return rdr;

        }

        /// <summary>
        /// Création du menu de droite "paramètres de la rubrique"
        /// </summary>
        /// <param name="Pref">ePref</param>
        /// <param name="nDescid">DescId de la rubrique</param>
        /// <param name="isSys">Est-ce une rubrique système</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFieldsParamsRenderer(ePref Pref, int nDescid, Boolean isSys = false)
        {
            eAdminFieldsParametersRenderer rdr = eAdminFieldsParametersRenderer.CreateAdminFieldsParamsRenderer(Pref, nDescid, isSys);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création de la modale d'administration des langues
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminLanguagesDialogRenderer(ePref pref)
        {
            eAdminLanguagesDialogRenderer rdr = eAdminLanguagesDialogRenderer.CreateAdminLanguagesDialogRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Creates the modal for the admin fields list renderer.
        /// </summary>
        /// <param name="pref">ePref object</param>
        /// <param name="tab">Tab descid</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFieldsListDialogRenderer(ePref pref, int tab)
        {
            eAdminFieldsListDialogRenderer rdr = eAdminFieldsListDialogRenderer.CreateAdminFieldsListDialogRenderer(pref, tab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Creates the admin fields list renderer.
        /// </summary>
        /// <param name="pref">ePref object</param>
        /// <param name="tab">Tab descid</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFieldsListRenderer(ePref pref, int tab)
        {
            eAdminFieldsListRenderer rdr = eAdminFieldsListRenderer.CreateAdminFieldsListRenderer(pref, tab);
            rdr.Generate();
            return rdr;
        }

        public static eAdminRenderer CreateAdminFieldsRGPDListDialogRenderer(ePref pref, int tab)
        {
            eAdminFieldsRGPDListDialogRenderer rdr = eAdminFieldsRGPDListDialogRenderer.CreateAdminFieldsRGPDListDialogRenderer(pref, tab);
            rdr.Generate();
            return rdr;
        }

        public static eAdminRenderer CreateAdminFieldsRGPDListRenderer(ePref pref, int tab)
        {
            eAdminFieldsRGPDListRenderer rdr = eAdminFieldsRGPDListRenderer.CreateAdminFieldsRGPDListRenderer(pref, tab);
            rdr.Generate();
            return rdr;
        }

        public static eAdminRenderer CreateAdminTabContentRenderer(ePref Pref, Int32 nTab)
        {
            eAdminTabContentRenderer rdr = eAdminTabContentRenderer.CreateAdminTabContentRenderer(Pref, nTab);
            rdr.Generate();
            return rdr;

        }

        /// <summary>
        /// Création du bloc des paramètres du signet
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="nTab">Descid de la table parente</param>
        /// <param name="nBkmDescId">Descid du signet</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminTabBkmParamsRenderer(ePref Pref, int nTab, int nBkmDescId)
        {
            eAdminRenderer rdr = eAdminTabBkmParametersRenderer.CreateAdminTabBkmParamsRenderer(Pref, nTab, nBkmDescId);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// génération d'un
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="specifId"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminWebLinkPropertiesRenderer(ePref pref, int tab, int specifId)
        {
            eAdminWebLinkPropertiesRenderer rdr = eAdminWebLinkPropertiesRenderer.CreateAdminWebLinkPropertiesRenderer(pref, tab, specifId);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Génération du bloc menu pour le paramétrage d'un ongletweb
        /// </summary>
        /// <param name="Pref">pref user</param>
        /// <param name="nTab">table du menu</param>
        /// <param name="nSpecifId">Id de la spécif</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminWebTabParameterRenderer(ePref Pref, Int32 nTab, Int32 nSpecifId)
        {
            eAdminWebTabParameterRenderer rdr = eAdminWebTabParameterRenderer.GetAdminWebTabParameterRenderer(Pref, nTab, nSpecifId);
            rdr.Generate();
            return rdr;

        }


        /// <summary>
        /// Génération du bloc menu pour le paramétrage d'un ongletweb
        /// </summary>
        /// <param name="Pref">pref user</param>
        /// <param name="nTab">table du menu</param>
        /// <param name="nSpecifId">Id de la spécif</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBkmWebParameterRenderer(ePref Pref, Int32 nTab, Int32 nSpecifId)
        {
            eAdminBkmWebParameterRenderer rdr = eAdminBkmWebParameterRenderer.GetAdminBkmWebParameterRenderer(Pref, nTab, nSpecifId);
            rdr.Generate();
            return rdr;

        }



        /// <summary>
        /// Fait un rendu du block de paramétrage du signet Grille
        /// </summary>
        /// <param name="pref">préférence utilisateur</param>
        /// <param name="tabInfos">ensemble d'informations sur la table principale</param>
        /// <param name="bkmTabInfos">ensemble d'information sur le siget</param>
        /// <param name="parmLabel">Libellé de l'onglet paramétre du menu de droite</param>
        /// <returns>Un renderer du block</returns>
        public static eAdminRenderer CreateAdminBkmGridFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string parmLabel)
        {
            eAdminBlockRenderer rdr = eAdminBkmGridFeaturesRenderer.GetAdminBkmGridFeaturesRenderer(pref, tabInfos, bkmTabInfos, parmLabel, "TitleInfo", "ToolTip");
            rdr.Generate();
            return rdr;
        }


        public static eAdminRenderer CreateAdminExtensionExternalMailingRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionExternalMailingRenderer rdr = eAdminExtensionExternalMailingRenderer.CreateAdminExtensionExternalMailingRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        public static eAdminRenderer CreateAdminExtensionNotificationsRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionNotificationsRenderer rdr = eAdminExtensionNotificationsRenderer.CreateAdminExtensionNotificationsRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// création de la strucutre du menu droite
        /// </summary>
        /// <param name="Pref">préférence user</param>
        /// <param name="nTab">Table du menu</param>
        /// <returns>HTML du bloc menu</returns>
        public static eAdminRenderer CreateAdminTabRightMenuRenderer(ePref Pref, Int32 nTab, string[] openedBlocks = null)
        {
            eAdminTabRightMenuRenderer rdr = eAdminTabRightMenuRenderer.CreateAdminTabRightMenuRenderer(Pref, nTab, openedBlocks);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminNavBarRenderer(ePref Pref, Int32 nTab, Int32 nBkm, eUserOptionsModules.USROPT_MODULE targetModule, int extensionFileId, string extensionLabel, string extensionCode)
        {
            eAdminNavBarRenderer rdr = eAdminNavBarRenderer.CreateAdminNavBarRenderer(Pref, nTab, nBkm, targetModule, extensionFileId, extensionLabel, extensionCode);
            rdr.Generate();
            return rdr;

        }

        /// <summary>
        /// Crée un renderer specific a la navbar homepage
        /// </summary>
        /// <param name="Pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminHomePageNavBarRenderer(ePref Pref, Int32 nTab, int pageId, int gridId, string gridLabel)
        {
            eAdminNavBarRenderer rdr = eAdminNavBarHomePageRenderer.CreateAdminNavBarRecordRenderer(Pref, nTab, pageId, gridId, gridLabel);
            rdr.Generate();
            return rdr;
        }


        /// <summary>Création d'un rendu de bloc vide du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBlockRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String blockId = "", Boolean bOpenedBlock = false)
        {
            eAdminBlockRenderer rdr = eAdminBlockRenderer.CreateAdminBlockRenderer(pref, infos, title, titleInfo, blockId, bOpenedBlock);
            rdr.Generate();
            return rdr;
        }

        #region Bloc "Paramètres de l'onglet"

        /// <summary>Création du bloc "Caractéristiques" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="tooltip"></param>
        /// <param name="bOpen">Indique si le bloc est ouvert (true par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFeaturesRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "", bool bOpen = true)
        {
            eAdminFeaturesRenderer rdr = eAdminFeaturesRenderer.CreateAdminFeaturesRenderer(pref, infos, title, titleInfo, tooltip);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Performances" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="bOpen">Indique si le bloc est ouvert ou fermé (fermé par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminPerfRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", bool bOpen = false)
        {
            eAdminPerformancesRenderer rdr = eAdminPerformancesRenderer.CreateAdminPerfRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création du bloc "Droit et comportement conditionnel" du menu de droite
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">TableInfos de la table concernée.</param>
        /// <param name="parentInfos">Table infos de la table parente</param>
        /// <param name="bOpen">Indique si le bloc est ouvert ou fermé (fermé par défaut)</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static eAdminRenderer CreateAdminRightsAndRulesRenderer(ePref pref, eAdminTableInfos infos, eAdminTableInfos parentInfos = null, bool bOpen = false)
        {

            eAdminRightsAndRulesRenderer rdr;

            if (infos.EdnType == EdnType.FILE_RELATION && parentInfos == null)
            {
                rdr = eAdminRightsAndRulesRelationTabRenderer.CreateAdminRightsAndRulesRelationTabRenderer(pref, infos);
            }
            else if (infos.EdnType == EdnType.FILE_BKMWEB)
            {
                rdr = eAdminRightsAndRulesBkmWebRenderer.CreateAdminRightsAndRulesBkmWebRenderer(pref, infos, parentInfos);
            }
            else if (parentInfos != null)
            {
                rdr = eAdminBkmRightsAndRulesRenderer.CreateAdminRightsAndRulesBkmRenderer(pref, infos, parentInfos);
            }
            else
            {
                rdr = eAdminRightsAndRulesRenderer.CreateAdminRightsAndRulesRenderer(pref, infos);
            }


            rdr.OpenedBlock = bOpen;
            rdr.Generate();

            if (rdr.ErrorMsg.Length > 0)
                throw rdr.InnerException ?? new Exception(rdr.ErrorMsg);

            return rdr;
        }

        /// <summary>Création de la fenetre des droits</summary>
        /// <param name="pref">ePref</param>
        /// <param name="descid">Descid de la table en cours</param>
        /// <param name="pageid">id de la fiche de la page d'accueil </param>    
        /// <param name="gridid">id de la fiche de la grille </param>     
        /// <param name="sFunction">Fonction de traitement : ajout/modif/supp</param>             
        /// <returns></returns>
        public static eAdminRightsRenderer CreateAdminRightsDialogRenderer(ePref pref, int descid, int pageid = 0, int gridid = 0, bool withGridRights = false)
        {
            // Si le descid réprésente un onglet dans ce cas on ajoute la gestion des droits sur les grilles       
            if (descid > 0 && descid % 100 == 0 && withGridRights)
                return new eAdminRightsDescAndGridsRenderer(pref, descid, pageid, gridid);
            else if (descid == (int)TableType.DOUBLONS)
            {
                //doublon
                return new eAdminRightsDblRenderer(pref);
            }

            else // Si le descid réprésente une rubrique ou l'option "Tous" dans le choix des onglets, on gère pas les grilles
                return new eAdminRightsDescRenderer(pref, descid);
        }


        /// <summary>
        /// Création de la liste des automatismes
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="tab">table sur laquel l'automatisme est activé</param>
        /// <param name="field">rubrique sur laquel l'automatisme est activé</param>
        /// <param name="width"> largeur de la liste </param>
        /// <param name="height"> hauteur de la liste</param>
        /// <param name="page"> numéro de la page a afficher </param>
        /// <param name="autoType">type de l'automatisme</param>
        /// <returns></returns>
        public static eRenderer CreateAdminAutomationListDialogRenderer(ePref pref, int tab, int field, int width, int height, int page, AutomationType autoType)
        {
            eRenderer rdr = eAdminAutomationListDialogRenderer.CreateAdminAutomationListDialogRenderer(pref, tab, field, width, height, page, autoType);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création de l'ecran de paramètrage  d'un automatisme
        /// </summary>
        /// <param name="pref">Préférence utilisateur</param>
        /// <param name="tab">table sur laquel l'automatisme est activé</param>
        /// <param name="field">rubrique sur laquel l'automatisme est activé</param>
        /// <param name="fileId">Id de l'automatisme</param>
        /// <param name="width"> largeur de la liste </param>
        /// <param name="height"> hauteur de la liste</param>       
        /// <param name="autoType">type de l'automatisme</param>
        /// <returns></returns>
        public static eRenderer CreateAdminAutomationFileRenderer(ePref pref, int tab, int field, int fileId, int width, int height, AutomationType autoType, String modalId)
        {

            eRenderer rdr = eAdminAutomationFileRenderer.CreateAdminAutomationFileRenderer(pref, tab, field, fileId, width, height, autoType, modalId);
            rdr.Generate();
            return rdr;
        }


        /// <summary>Création de la popup de traitement sur les droits de traitements</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminRightsTreatmentDialogRenderer(ePref pref)
        {
            eAdminRenderer rdr = eAdminRightsTreatmentDialogRenderer.CreateAdminRightsTreatmentDialogRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la popup de modification d'adresse IP (module Accès - onglet Sécurité)</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAccessSecurityIPDialogRenderer(ePref pref, int ipId, string fieldToFocus)
        {
            eAdminAccessSecurityIPDialogRenderer rdr = eAdminAccessSecurityIPDialogRenderer.CreateAdminAccessSecurityIPDialogRenderer(pref, ipId, fieldToFocus);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la popup de modification de groupes</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminGroupDialogRenderer(ePref pref, int groupId, int action)
        {
            eAdminGroupDialogRenderer rdr = eAdminGroupDialogRenderer.CreateAdminGroupDialogRenderer(pref, groupId, action);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la popup de choix du picto de l'onglet </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nTab">Onglet</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminPictoDialogRenderer(ePref pref, int nTab, int nDescId, eFontIcons.FontIcons icon, string color)
        {
            eAdminPictoDialogRenderer rdr = eAdminPictoDialogRenderer.CreateAdminPictoDialogRenderer(pref, nTab, nDescId, icon, color);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Relations" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="bOpen">Bloc ouvert (false par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminRelationsRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", bool bOpen = false)
        {
            eAdminRelationsRenderer rdr = eAdminRelationsRenderer.CreateAdminRelationsRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Préférences" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="bOpen">Bloc ouvert (false par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminPreferencesRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "", bool bOpen = false)
        {
            eAdminPreferencesRenderer rdr = eAdminPreferencesRenderer.CreateAdminPreferencesRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin : Bloc de paramétrage d'un champ ALIAS - Paramètres de relation
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        internal static eAdminRenderer CreateAdminFieldAliasRelationRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldAliasRelationRenderer rdr = eAdminFieldAliasRelationRenderer.CreateAdminFieldAliasRelationRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Option de recherches" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminSearchOptionsRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "")
        {
            eAdminSearchOptionsRenderer rdr = eAdminSearchOptionsRenderer.CreateAdminSearchOptionsRenderer(pref, infos, title, titleInfo);
            rdr.Generate();
            return rdr;
        }


        /// <summary>Création du bloc "Filtres et doublons" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFiltersAndDuplicatesRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "")
        {
            eAdminFiltersAndDuplicatesRenderer rdr = eAdminFiltersAndDuplicatesRenderer.CreateAdminFiltersAndDuplicatesRenderer(pref, infos, title, titleInfo);
            rdr.Generate();
            return rdr;
        }


        /// <summary>Création du bloc "Cartographie" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="bOpen">Indique si le bloc est ouvert(fermé par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminMapRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "", bool bOpen = false)
        {
            eAdminMapRenderer rdr = eAdminMapRenderer.CreateAdminMapRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Référentiel de données" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="tooltip">Da tooltip.</param>
        /// <param name="bOpen">Indique si le bloc est ouvert (fermé par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminDataRepositoriesRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "", bool bOpen = false)
        {
            eAdminDataRepositoriesRenderer rdr = eAdminDataRepositoriesRenderer.CreateAdminDataRepositoriesRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Référentiel de données" du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="infos">The infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="tooltip">Da tooltip.</param>
        /// <param name="bOpen">Indique si le bloc est ouvert (fermé par défaut)</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminExtensionsTabParamRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "", String tooltip = "", bool bOpen = false)
        {
            eAdminExtensionTabParamRenderer rdr = eAdminExtensionTabParamRenderer.CreateAdminExtensionRenderer(pref, infos, title, titleInfo);
            rdr.OpenedBlock = bOpen;
            rdr.Generate();
            return rdr;
        }



        /// <summary>
        /// Rendu de la modale d'administration de la minifiche
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nTab">Descid de la table</param>
        /// <param name="type">Type de mini-fiche</param>
        /// <param name="widgetId">ID du widget, facultatif</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminMiniFileDialogRenderer(ePref pref, int nTab, MiniFileType type = MiniFileType.File, int widgetId = 0)
        {
            eAdminMiniFileDialogRenderer rdr = eAdminMiniFileDialogRenderer.CreateAdminMiniFileDialogRenderer(pref, type, nTab, widgetId);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Crée le renderer pour paramétrer le mapping entre les champs de société et d'adresse
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="mapping"></param>
        /// <param name="doBuild">indique s'il faut construire tout le corps (false pour rajouter une ligne de correspondance vierge.)</param>
        /// <returns></returns>
        public static eAdminPmAdrMappingRenderer CreateAdminPmAdrMappingRenderer(ePref pref, ePmAddressMapping mapping, bool doBuild = true)
        {
            eAdminPmAdrMappingRenderer rdr = eAdminPmAdrMappingRenderer.CreateAdminPmAdrMappingRenderer(pref, mapping);
            rdr.DoBuild = doBuild;
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Crée le renderer pour paramétrer le mapping entre les champs de société et d'adresse
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eAdminPmAdrMappingRenderer CreateAdminPmAdrMappingRenderer(ePref pref)
        {
            ePmAddressMapping mapping = ePmAddressMapping.GetCurrentMapping(pref);
            return CreateAdminPmAdrMappingRenderer(pref, mapping);
        }

        /// <summary>
        /// Rendu de la modale d'administration de la recherche d'addresses prédictive
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAutocompleteAddressDialogRenderer(ePref pref, int nTab)
        {
            eAdminAutocompleteAddressDialogRenderer rdr = eAdminAutocompleteAddressDialogRenderer.CreateAdminAutocompleteAddressDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de la modale d'administration du référentiel Sirene
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminSireneDialogRenderer(ePref pref, int nTab)
        {
            eAdminSireneDialogRenderer rdr = eAdminSireneDialogRenderer.CreateAdminSireneDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de la modale d'administration du mapping de la synchro Exchange
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminSynchroExchangeMappingDialogRenderer(ePref pref, int tab)
        {
            eAdminExtensionSynchroExchangeMappingRenderer rdr = eAdminExtensionSynchroExchangeMappingRenderer.CreateAdminExtensionSynchroExchangeMappingRenderer(pref, tab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de la modale d'administration de l'infobulle de position
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminMapTooltipDialogRenderer(ePref pref, int nTab)
        {
            eAdminMapTooltipDialogRenderer rdr = eAdminMapTooltipDialogRenderer.CreateAdminMapTooltipDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de l'administration des onglets</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminTabsRenderer(ePref pref, int nWidth, int nHeight, String sSortCol = "LABEL", Int32 iSort = 0, String sSearch = "")
        {
            eAdminTabsRenderer rdr = eAdminTabsRenderer.CreateAdminTabsRenderer(pref, nWidth, nHeight);
            if (sSortCol.Length > 0)
            {
                rdr.SortCol = sSortCol;
                rdr.Sort = iSort;
                rdr.Search = sSearch;
            }
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la fenêtre d'onglets</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminNewTabDialogRenderer(ePref pref)
        {
            eAdminNewTabDialogRenderer rdr = eAdminNewTabDialogRenderer.CreateAdminNewTabDialogRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la fenêtre des messages expresses</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminNewExpressMessageDialogRenderer(ePref pref, eAdminHomepage hpExpressMessage)
        {
            eAdminNewExpressMessageDialogRenderer rdr = eAdminNewExpressMessageDialogRenderer.CreateAdminNewExpressMessageDialogRenderer(pref, hpExpressMessage);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de l'administration des onglets</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminHomepagesRenderer(ePref pref, int nWidth, int nHeight, String sSortCol = "", Int32 iSort = 0)
        {
            eAdminHomepagesRenderer rdr = eAdminHomepagesRenderer.CreateAdminHomepagesRenderer(pref, nWidth, nHeight);
            if (sSortCol.Length > 0)
            {
                rdr.SortCol = sSortCol;
                rdr.Sort = iSort;
            }
            rdr.Generate();
            return rdr;

        }

        /// <summary>Création de la liste des nouvelles pages d'accueil xrm, cet objet uitilise le renderer du mode list standard</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateXrmHomePageListRenderer(ePref pref, int nPage, int nRows, int nWidth, int nHeight)
        {
            eAdminModuleRenderer rdr = eAdminXrmHomePageListRenderer.CreateAdminXrmHomePageListRenderer(pref, nPage, nRows, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de l'administration des onglets</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminHomeExpressMessageRenderer(ePref pref, int nWidth, int nHeight, String sSortCol = "", Int32 iSort = 0)
        {
            eAdminHomeExpressMessageRenderer rdr = eAdminHomeExpressMessageRenderer.CreateAdminHomepagesRenderer(pref, nWidth, nHeight);
            if (sSortCol.Length > 0)
            {
                rdr.SortCol = sSortCol;
                rdr.Sort = iSort;
            }
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du module d'administration "Accès"</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAccessRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE childModule, bool bFullRend, int nPage, int nRows, int nWidth, int nHeight)
        {
            eAdminAccessRenderer rdr = eAdminAccessRenderer.CreateAdminAccessRenderer(pref, childModule, bFullRend, nPage, nRows, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du module d'administration "Grille" > "Grille"</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminXrmGridRenderer(ePref pref, int gridId, int nWidth, int nHeight)
        {
            eAdminXrmGridRenderer rdr = eAdminXrmGridRenderer.CreateAdminXrmGridRenderer(pref, gridId, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du module d'administration "Pages d'accueil" > "Pages"</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminXrmHomePageRenderer(ePref pref, int pageId, int nWidth, int nHeight)
        {
            eAdminXrmHomePageRenderer rdr = eAdminXrmHomePageRenderer.CreateAdminXrmHomePageRenderer(pref, pageId, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du module d'administration "Accès" > "Sécurité"</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAccessSecurityRenderer(ePref pref, int nWidth, int nHeight)
        {
            eAdminAccessSecurityRenderer rdr = eAdminAccessSecurityRenderer.CreateAdminAccessSecurityRenderer(pref, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création de la page d'accueil (Portail) de l'administration</summary>
        /// <param name="pref">ePref</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminPortalRenderer(ePref pref)
        {
            eAdminPortalRenderer rdr = eAdminPortalRenderer.CreateAdminPortalRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la liste des modules de l'administration
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="nRows">Nombre de ligne par page</param>
        /// <param name="typRefresh">Indique si on doit generer la totalité du contenu, ou uniquement la liste ou seulement ajouter du contenu a la liste</param>
        /// <param name="criteres">Filtres</param>
        /// <returns></returns>
        public static eAdminStoreListRenderer CreateAdminStoreListRenderer(ePref pref, int nPage, int nRows, StoreListTypeRefresh typRefresh, StoreListCriteres criteres)
        {
            eAdminStoreListRenderer rdr = eAdminStoreListRenderer.CreateAdminStoreListRenderer(pref, nPage, nRows, typRefresh, criteres);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la liste des modules de l'administration
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="nPage">Numéro de page</param>
        /// <param name="nRows">Nombre de ligne par page</param>
        /// <param name="sFilterCategory">Filtre sur la catégorie</param>
        /// <param name="sSearch">Filtre sur titre</param>
        /// <param name="dFilterNotation">Filtre sur note</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminExtensionListRenderer(ePref pref, int nPage, int nRows, string sSearch, string sFilterCategory, double? dFilterNotation)
        {
            eAdminExtensionListRenderer rdr = eAdminExtensionListRenderer.CreateAdminExtensionListRenderer(pref, nPage, nRows, sSearch, sFilterCategory, dFilterNotation);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du tableau de bord de l'administration
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="w">Largeur</param>
        /// <param name="h">Hauteur</param>
        /// <param name="year">Année</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminDashboardRenderer(ePref pref, int w, int h, int year)
        {
            eAdminDashboardRenderer rdr = eAdminDashboardRenderer.CreateAdminDashboardRenderer(pref, w, h, year);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du tableau de bord RGPD de l'administration
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="w">Largeur</param>
        /// <param name="h">Hauteur</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminDashboardRGPDRenderer(ePref pref, int w, int h)
        {
            eAdminRenderer rdr = eAdminDashboardRGPDRenderer.CreateAdminDashboardRGPDRenderer(pref, w, h);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la page de logs de traitements RGPD
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="w">Largeur</param>
        /// <param name="h">Hauteur</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminDashboardRGPDTreatmentLogRenderer(ePref pref, int w, int h)
        {
            eAdminRenderer rdr = eAdminDashboardRGPDTreatmentLogRenderer.CreateAdminDashboardRGPDTreatmentLogRenderer(pref, w, h);
            rdr.Generate();
            return rdr;
        }

        #region Extensions

        /// <summary>
        /// Crée le mode Fiche correspondant à une extension identifiée par le module passé en paramètre
        /// Fait appel à la méthode CreateAdminExtension*Renderer correspondant à celui-ci
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="module">Objet eUserOptionsModules.USROPT_MODULE correspondant à l'extension à afficher</param>
        /// <param name="extensionFileIdFromStore">Id de l'extension sur hotcom</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns>Renderer généré</returns>
        public static eAdminRenderer CreateAdminExtensionFileRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE module, int extensionFileIdFromStore, string initialTab)
        {
            eAdminRenderer rdr = null;
            bool bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";
            switch (module)
            {
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    rdr = eAdminRendererFactory.CreateAdminExtensionMobileRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    rdr = eAdminRendererFactory.CreateAdminExtensionOutlookAddinRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                    rdr = eAdminRendererFactory.CreateAdminExtensionLinkedinRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    rdr = eAdminRendererFactory.CreateAdminExtensionSynchroRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    // TOCHECK - Backlog #335 - L'extension Synchro Exchange 2016 on Premise partage pour l'instant le même onglet Paramètres que l'extension SMS classique
                    rdr = eAdminRendererFactory.CreateAdminExtensionSynchroExchangeRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE: // TOCHECK - Backlog #216 - L'extension SMS Netmessage partage pour l'instant le même onglet Paramètres que l'extension SMS classique
                    rdr = eAdminRendererFactory.CreateAdminExtensionSMSRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    rdr = eAdminRendererFactory.CreateAdminExtensionCTIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    rdr = eAdminRendererFactory.CreateAdminExtensionAPIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    rdr = eAdminRendererFactory.CreateAdminExtensionExternalMailingRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    rdr = eAdminRendererFactory.CreateAdminExtensionVCardRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    rdr = eAdminRendererFactory.CreateAdminExtensionSnapshotRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    rdr = eAdminRendererFactory.CreateAdminExtensionEmailingRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    rdr = eAdminRendererFactory.CreateAdminExtensionGridRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    rdr = eAdminRendererFactory.CreateAdminExtensionNotificationsRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    rdr = eAdminRendererFactory.CreateAdminExtensionSireneRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    rdr = eAdminRendererFactory.CreateAdminExtensionPowerBIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    rdr = eAdminRendererFactory.CreateAdminExtensionDedicatedIpRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                    rdr = eAdminRendererFactory.CreateAdminExtensionMailStatusVerificationRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    rdr = eAdminRendererFactory.CreateAdminExtensionWorldlinePaymentRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
                    break;
                // Pas d'onglet Paramètres pour les extensions ci-dessous = renderer générique
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                //SHA : tâche #1 873
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                // Cas "générique" d'une extension "non intégrée" référencée sur le Store (données récupérées via l'API)
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                default:
                    rdr = eAdminRendererFactory.CreateAdminExtensionFromStoreRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
                    break;
            }

            // Il ne faut pas faire appel au Generate() de rdr ici, car il est déjà appelé dans les méthodes Create* présentes
            // dans le switch ci-dessous. Cela générerait le contenu HTML deux fois.
            // On se contente donc simplement de renvoyer l'objet généré
            return rdr;
        }


        /// <summary>
        /// Création d'un eRenderer pour présenter une extension en mode fiche
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="targetExtension"></param>
        /// <returns></returns>
        internal static eAdminRenderer CreateAdminStoreFileRenderer(ePref pref, eAdminExtension targetExtension)
        {

            eAdminRenderer rdr = null;
            switch (targetExtension.Module)
            {
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MOBILE:
                    rdr = eAdminStoreMobileRenderer.CreateAdminStoreMobileRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN:
                    rdr = eAdminStorenOutlookAddinRenderer.CreateAdminStoreOutlookAddinRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_LINKEDIN:
                    rdr = eAdminStoreLinkedinRenderer.CreateeAdminStoreLinkedinRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO:
                    rdr = eAdminStoreSynchroRenderer.CreateAdminStoreSynchroRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_OFFICE365:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SYNCHRO_EXCHANGE2016ONPREMISE:
                    // TOCHECK - Backlog #335 - L'extension Synchro Exchange 2016 on Premise partage pour l'instant le même onglet Paramètres que l'extension SMS classique
                    //rdr = eAdminRendererFactory.CreateAdminExtensionSynchroExchangeRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
                    rdr = eAdminStoreSynchroExchangeRenderer.CreateAdminStoreSynchroExchangeRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SMS_NETMESSAGE: // TOCHECK - Backlog #216 - L'extension SMS Netmessage partage pour l'instant le même onglet Paramètres que l'extension SMS classique
                    rdr = eAdminStoreSMSRenderer.CreateAdminStoreSMSRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_CTI:
                    rdr = eAdminStoreCTIRenderer.CreateAdminStoreCTIRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_API:
                    rdr = eAdminStoreAPIRenderer.CreateAdminStoreAPIRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTERNALMAILING:
                    rdr = eAdminStoreExternalMailingRenderer.CreateAdminStoreExternalMailingRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_VCARD:
                    rdr = eAdminStoreVCardRenderer.CreateAdminStoreVCardRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SNAPSHOT:
                    rdr = eAdminStoreSnapshotRenderer.CreateAdminStoreSnapshotRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EMAILING:
                    rdr = eAdminStoreEmailingRenderer.CreateAdminStoreEmailingRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_GRID:
                    rdr = eAdminStoreGridRenderer.CreateAdminStoreGridRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_NOTIFICATIONS:
                    rdr = eAdminStoreNotificationsRenderer.CreateAdminStoreNotificationsRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_SIRENE:
                    rdr = eAdminStoreSireneRenderer.CreateAdminStoreSireneRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ZAPIER:
                    rdr = eAdminStoreZapierRenderer.CreateAdminStoreZapierRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET:
                    rdr = eAdminStoreExtranetRenderer.CreateAdminStoreExtranetRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_POWERBI:
                    rdr = eAdminStorePowerBIRenderer.CreateAdminStorePowerBIRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DEDICATED_IP:
                    rdr = eAdminStoreDedicatedIpRenderer.CreateAdminStoreDedicatedIpRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_MAIL_STATUS_VERIFICATION:
                    rdr = eAdminStoreMailStatusVerifRenderer.CreateAdminStoreMailStatusVerifRenderer(pref, targetExtension);
                    break;
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_WORLDLINE_PAYMENT:
                    rdr = eAdminStoreWorldlinePaymentRenderer.CreateAdminStoreWordlinePaymentRenderer(pref, targetExtension);
                    break;
                // Pas d'onglet Paramètres pour les extensions ci-dessous = renderer générique
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_BUSINESSSOFT:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_CEGID:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SAGE:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_EBP:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ACCOUNTING_SIGMA:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_UBIFLOW:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_IN_HBS:
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_DOCUSIGN:
                //SHA : tâche #1 873
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_ADVANCED_FORM:
                // Cas "générique" d'une extension "non intégrée" référencée sur le Store (données récupérées via l'API)
                case eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_FROMSTORE:
                default:

                    if (targetExtension.Infos.HasCustomParameter && pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                        rdr = eAdminStoreCustomParameter.CreateAdminStoreCustomParamRenderer(pref, targetExtension);
                    else
                        rdr = eAdminStoreSpecifRenderer.CreateAdminStoreSpecifRenderer(pref, targetExtension);
                    break;
            }

            // Il ne faut pas faire appel au Generate() de rdr ici, car il est déjà appelé dans les méthodes Create* présentes
            // dans le switch ci-dessous. Cela générerait le contenu HTML deux fois.
            // On se contente donc simplement de renvoyer l'objet généré
            return rdr;
        }



        /// <summary>
        /// Admin :Rendu du Module extension "FromStore" "Standard" (inclus par défaut dans XRM)
        /// hors cas spécifique ayant leur propre CreateXX
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="module">Type du module</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionFromStoreRenderer(ePref pref, int extensionFileIdFromStore, eUserOptionsModules.USROPT_MODULE module, bool bNoInternet, string initialTab)
        {
            eAdminExtensionFromStoreRenderer rdr = eAdminExtensionFromStoreRenderer.CreateAdminExtensionFromStoreRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin : Rendu du MODULE emailing
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionEmailingRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionEmailingRenderer rdr = eAdminExtensionEmailingRenderer.CreateAdminExtensionEmailingRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration des outils de synchro (EudoSync Exchange Agendas / EudoSync Exchange Contacts)
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionSynchroRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionSynchroRenderer rdr = eAdminExtensionSynchroRenderer.CreateAdminExtensionSynchroRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création du module d'administration des outils de synchro exchange E2017
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="module">Ce renderer pouvant être partagé par plusieurs extensions, on passe l'extension appelante</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionSynchroExchangeRenderer(ePref pref, int extensionFileIdFromStore, eUserOptionsModules.USROPT_MODULE module, bool bNoInternet, string initialTab)
        {
            eAdminExtensionSynchroExchangeRenderer rdr = eAdminExtensionSynchroExchangeRenderer.CreateAdminExtensionSynchroExchangeRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de la version mobile
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionMobileRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionMobileRenderer rdr = eAdminExtensionMobileRenderer.CreateAdminExtensionMobileRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de l'add-in Outlook
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionOutlookAddinRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionOutlookAddinRenderer rdr = eAdminExtensionOutlookAddinRenderer.CreateAdminExtensionOutlookAddinRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création du module d'administration de linkedin
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionLinkedinRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionLinkedinRenderer rdr = eAdminExtensionLinkedinRenderer.CreateAdminExtensionLinkedinRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de Vérification du statut Mail
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionMailStatusVerificationRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionMailStatusVerificaionRenderer rdr = eAdminExtensionMailStatusVerificaionRenderer.CreateAdminExtensionMailStatusVerificationRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de Paiement Worldline
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionWorldlinePaymentRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionWorldlinePaymentRenderer rdr = eAdminExtensionWorldlinePaymentRenderer.CreateAdminExtensionWorldlinePaymentRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration du référentiel Sirene
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionSireneRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionSireneRenderer rdr = eAdminExtensionSireneRenderer.CreateAdminExtensionSireneRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Création du module d'administration de PowerBI
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionPowerBIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionPowerBIRenderer rdr = eAdminExtensionPowerBIRenderer.CreateAdminExtensionPowerBIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de IP Dédiée
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionDedicatedIpRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionDedicatedIpRenderer rdr = eAdminExtensionDedicatedIpRenderer.CreateAdminExtensionDedicatedIpRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration des composants grilles
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionGridRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionGridRenderer rdr = eAdminExtensionGridRenderer.CreateAdminExtensionGridRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration du module d'envoi de SMS
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="module">Ce renderer pouvant être partagé par plusieurs extensions, on passe l'extension appelante</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionSMSRenderer(ePref pref, int extensionFileIdFromStore, eUserOptionsModules.USROPT_MODULE module, bool bNoInternet, string initialTab)
        {
            eAdminExtensionSMSRenderer rdr = eAdminExtensionSMSRenderer.CreateAdminExtensionSMSRenderer(pref, extensionFileIdFromStore, module, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration du module CTI
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionCTIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionCTIRenderer rdr = eAdminExtensionCTIRenderer.CreateAdminExtensionCTIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration du module API
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionAPIRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionAPIRenderer rdr = eAdminExtensionAPIRenderer.CreateAdminExtensionAPIRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration de la VCard
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionVCardRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionVCardRenderer rdr = eAdminExtensionVCardRenderer.CreateAdminExtensionVCardRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du module d'administration du module EudoSnapshot
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="extensionFileIdFromStore">Id sur hotcom de l'extension (pour récupération informations)</param>
        /// <param name="bNoInternet">Pas d'internet : informations via xml inclus</param>
        /// <param name="initialTab">Onglet sur lequel l'affichage se fait au chargement (par défaut, description)</param>
        /// <returns></returns>
        private static eAdminRenderer CreateAdminExtensionSnapshotRenderer(ePref pref, int extensionFileIdFromStore, bool bNoInternet, string initialTab)
        {
            eAdminExtensionSnapshotRenderer rdr = eAdminExtensionSnapshotRenderer.CreateAdminExtensionSnapshotRenderer(pref, extensionFileIdFromStore, bNoInternet, initialTab);
            rdr.Generate();
            return rdr;
        }

        #endregion


        /// <summary>
        /// Création de la popup d'administration des préférences du planning
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eRenderer CreateAdminPlanningPrefDialogRenderer(ePref pref, int tab, int userid)
        {
            eAdminPlanningPrefDialogRenderer rdr = eAdminPlanningPrefDialogRenderer.CreateAdminPlanningPrefDialogRenderer(pref, tab, userid);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création des options préférences du planning
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public static eRenderer CreateAdminPlanningPrefRenderer(ePref pref, int tab, int userid)
        {
            eAdminPlanningPrefRenderer rdr = eAdminPlanningPrefRenderer.CreateAdminPlanningPrefRenderer(pref, tab, userid);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la partie "Traçabilité"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="infos"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminTraceabilityRenderer(ePref pref, eAdminTableInfos infos, String title)
        {
            eAdminTraceabilityRenderer rdr = eAdminTraceabilityRenderer.CreateAdminTraceabilityRenderer(pref, infos, title);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la partie "Liens web et traitements spécifiques"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="tab"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminWebLinksAndTreatments(ePref pref, int tab, String title)
        {
            eAdminWebLinksAndTreatmentsRenderer rdr = eAdminWebLinksAndTreatmentsRenderer.CreateAdminWebLinksAndTreatments(pref, tab, title);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la partie "Sécurité"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBlockSecurityRenderer(ePref pref, eAdminTableInfos infos)
        {
            eAdminBlockSecurityRenderer rdr = eAdminBlockSecurityRenderer.CreateAdminBlockSecurityRenderer(pref, infos);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création de la partie "Langues et paramètres régionaux"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBlockTranslationsRenderer(ePref pref, eAdminTableInfos infos)
        {
            eAdminBlockTranslationsRenderer rdr = eAdminBlockTranslationsRenderer.CreateAdminBlockTranslationsRenderer(pref, infos);
            rdr.Generate();
            return rdr;
        }
        /// <summary>
        /// Création de la partie "Langues et paramètres régionaux"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="infos"></param>
        /// <returns></returns>

        public static eAdminRenderer CreateAdminBlockTranslationsRenderer(ePref pref, eAdminFieldInfos infos)
        {
            eAdminBlockTranslationsRenderer rdr = eAdminBlockTranslationsRenderer.CreateAdminBlockTranslationsRenderer(pref, infos);
            rdr.Generate();
            return rdr;
        }
        /// <summary>
        /// Création du rendu de la popup "Recherche avancée"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="listCol"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAdvSearchDialogRenderer(ePref pref, int nTab, string listCol = "")
        {
            eAdminAdvSearchDialogRenderer rdr = eAdminAdvSearchDialogRenderer.CreateAdminAdvSearchDialogRenderer(pref, nTab, listCol);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du rendu de la popup "Délégations d'appartenance"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBelongingDialogRenderer(ePref pref, int nTab, int nWidth, int nHeight)
        {
            eAdminBelongingDialogRenderer rdr = eAdminBelongingDialogRenderer.CreateAdminBelongingDialogRenderer(pref, nTab, nWidth, nHeight);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du rendu de la popup "Import étendu"
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminExtendedTargetMappingsDialogRenderer(ePref pref, int nTab)
        {
            eAdminExtendedTargetMappingsDialogRenderer rdr = eAdminExtendedTargetMappingsDialogRenderer.CreateAdminExtendedTargetMappingsDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Création du rendu de la popup "Nouvelle règle d'archivage/suppression"
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="tab">DescID de l'onglet</param>
        /// <param name="ruleType">Type de règle : archivage ou suppression</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminRGPDConditionsDialogRenderer(ePref pref, int tab, RGPDRuleType ruleType)
        {
            eAdminRGPDConditionsDialogRenderer rdr = eAdminRGPDConditionsDialogRenderer.CreateAdminRGPDConditionsDialogRenderer(pref, tab, ruleType);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin Champ - Rendu Bloc Qualité des données et RGPD
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminDataQualityRenderer(ePref pref, eAdminTableInfos infos)
        {
            eAdminDataQualityRenderer rdr = eAdminDataQualityRenderer.CreateAdminDataQualityRenderer(pref, infos);
            rdr.Generate();
            return rdr;
        }

        #endregion

        #region Bloc "Mise en page" ou "Contenu de l'onglet"
        /// <summary>Création de la partie "Colonnes" du bloc "Contenu de l'onglet"</summary>
        /// <param name="pref">ePref</param>
        /// <param name="infos">Infos sur la table</param>
        /// <param name="title">Titre de la partie</param>
        /// <param name="titleInfo">Sous-titre</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminColumnsParamRenderer(ePref pref, eAdminTableInfos infos, String title, String titleInfo = "")
        {
            eAdminColumnsParamRenderer rdr = eAdminColumnsParamRenderer.CreateAdminColumnsParamRenderer(pref, infos, title, titleInfo);
            rdr.Generate();
            return rdr;
        }


        /// <summary>Bloc "Rubriques" de la partie "Contenu de l'onglet"</summary>
        /// <param name="pref">ePref</param>
        /// <param name="title">Titre du bloc</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFieldsTypesRenderer(ePref pref, eAdminTableInfos infos, String title)
        {
            eAdminFieldsTypesRenderer rdr = eAdminFieldsTypesRenderer.CreateAdminFieldsTypesRenderer(pref, infos, title);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Bloc de l'entré menu de l'ajout des onglets webs (eAdminTabContentRenderer)
        /// </summary>
        /// <param name="pref"></param>
        /// <returns></returns>
        public static eAdminBlockRenderer CreateAdminWebTabRenderer(ePref pref)
        {
            eAdminBlockRenderer rdr = eAdminWebTabMenuRenderer.CreateWebTabMenuRenderer(pref);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Bloc "Signets Web" de la partie "Contenu de l'onglet"</summary>
        /// <param name="pref">ePref</param>
        /// <param name="title">Titre du bloc</param>
        /// <returns></returns>
        public static eAdminWebBkmRenderer CreateAdminWebBkmRenderer(ePref pref, eAdminTableInfos tabInfos, String title)
        {
            eAdminWebBkmRenderer rdr = eAdminWebBkmRenderer.CreateAdminWebBkmRenderer(pref, tabInfos, title);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Bloc "Entête" de la partie "Contenu de l'onglet"</summary>
        /// <param name="pref">ePref</param>
        /// <param name="title">Titre du bloc</param>
        /// <returns></returns>
        public static eAdminHeaderRenderer CreateAdminHeaderRenderer(ePref pref, String title, string[] openedBlocks = null)
        {
            eAdminHeaderRenderer rdr = eAdminHeaderRenderer.CreateAdminHeaderRenderer(pref, title, openedBlocks);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de la modale d'administration des relations d'entête
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRelationsDialogRenderer CreateAdminRelationsDialogRenderer(ePref pref, int nTab)
        {
            eAdminRelationsDialogRenderer rdr = eAdminRelationsDialogRenderer.CreateAdminRightsDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de l'étape 2 de la modale d'admin des relations d'entête
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRelationsFieldsRenderer CreateAdminRelationsFieldsRenderer(ePref pref, int nTab)
        {
            eAdminRelationsFieldsRenderer rdr = eAdminRelationsFieldsRenderer.CreateRelationsFieldsRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de la modale d'administration des filtres SQL des relations d'entête
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRelationsSQLFiltersDialogRenderer CreateAdminRelationsSQLFiltersDialogRenderer(ePref pref, eAdminTableInfos tabInfos, int parentTab, String sValue)
        {
            eAdminRelationsSQLFiltersDialogRenderer rdr = eAdminRelationsSQLFiltersDialogRenderer.CreateAdminRightsSQLFiltersDialogRenderer(pref, tabInfos, parentTab, sValue);
            rdr.Generate();
            return rdr;
        }

        #endregion

        #region Bloc "Paramètres de la rubrique"

        /// <summary>
        /// Admin Champ - Rendu Bloc Caractéristiques
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldFeaturesRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldFeaturesRenderer rdr = eAdminFieldFeaturesRenderer.CreateAdminFieldFeaturesRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Admin Champ - Rendu Bloc Mise en Page
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldLayoutRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldLayoutRenderer rdr = eAdminFieldLayoutRenderer.CreateAdminFieldLayoutRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Admin Champ - Rendu Bloc Droits, Régles et Comportement Conditionnel
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldRightsAndRulesRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldRightsAndRulesRenderer rdr = eAdminFieldRightsAndRulesRenderer.CreateAdminFieldRightsAndRulesLayoutRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Admin Champ - Rendu Bloc Propriétés de catalouge
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldCatalogRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldCatalogRenderer rdr = eAdminFieldCatalogRenderer.CreateAdminFieldCatalogRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin Champ - Rendu Bloc Tracabilité
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldTraceabilityRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldTraceabilityRenderer rdr = eAdminFieldTraceabilityRenderer.CreateAdminFieldCatalogRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin Champ - Rendu Bloc Relation pour catalogue liaison et champ ALIAS
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldRelationsRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldRelationsRenderer rdr = eAdminFieldRelationsRenderer.CreateAdminFieldRelationsRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin Champ - Rendu Bloc Traitement (Automatisme avancé )
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldProcessRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldProcessRenderer rdr = eAdminFieldProcessRenderer.CreateAdminFieldProcessRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Admin Champ - Rendu Bloc Qualité des données et RGPD
        /// </summary>
        /// <param name="pref">Pref de connexion</param>
        /// <param name="field">Instance objet info sur le champs</param>
        /// <returns>Renderer Admin</returns>
        public static eAdminRenderer CreateAdminFieldDataQualityRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldDataQualityRenderer rdr = eAdminFieldDataQualityRenderer.CreateAdminFieldDataQualityRenderer(pref, field);
            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Créer un renderer d'un automatisme avancé formules
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <param name="nField"></param>
        /// <returns></returns>
        public static eRenderer CreateAdminAdvancedAutomationRenderer(ePref pref, int nTab, int nField)
        {
            eRenderer rdr = eAdminAdvancedAutomationRenderer.CreateAdvancedAutomationRenderer(pref, nTab, nField);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Fenêtre modale d'admin des catalogues avancés
        /// </summary>
        /// <param name="pref">Préf</param>
        /// <param name="nTab">DescId de la table</param>
        /// <param name="nField">DescId du champ</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminAdvancedCatalogDialogRenderer(ePref pref, int nTab, int nField)
        {
            eAdminAdvancedCatalogDialogRenderer rdr = eAdminAdvancedCatalogDialogRenderer.CreateAdminAdvancedCatalogDialogRenderer(pref, nTab, nField);
            rdr.Generate();
            return rdr;
        }
        #endregion


        #region Bloc "Paramètres du signet"
        /// <summary>Création du bloc "Caractéristiques" du signet du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tabInfos">infos de la table</param>
        /// <param name="bkmTabInfos">infos du signet</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBkmFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo = "", string tooltip = "")
        {
            eAdminBkmFeaturesRenderer rdr = eAdminBkmFeaturesRenderer.CreateAdminBkmFeaturesRenderer(pref, tabInfos, bkmTabInfos, title, titleInfo, tooltip);
            rdr.Generate();
            return rdr;
        }

        /// <summary>Création du bloc "Relations" des signets du menu de droite</summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tabInfos">infos de la table</param>
        /// <param name="bkmTabInfos">infos du signet</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBkmRelationsRenderer(ePref pref, eAdminTableInfos tabInfos, eAdminTableInfos bkmTabInfos, string title, string titleInfo = "", string tooltip = "")
        {
            eAdminBkmRelationsRenderer rdr = eAdminBkmRelationsRenderer.CreateAdminBkmRelationsRenderer(pref, tabInfos, bkmTabInfos, title, titleInfo, tooltip);
            rdr.Generate();
            return rdr;
        }



        /// <summary>
        /// Rendu de la modale d'administration des relations additionnelles
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminBkmRelationsSQLFiltersDialogRenderer(ePref pref, int nTab)
        {
            eAdminBkmRelationsSQLFiltersDialogRenderer rdr = eAdminBkmRelationsSQLFiltersDialogRenderer.CreateAdminBkmRelationsSQLFiltersDialogRenderer(pref, nTab);
            rdr.Generate();
            return rdr;
        }
        #endregion

        /// <summary>
        /// Retourne un renderer pour la fenetre  traitements/droits conditionnels pour l'admin
        /// </summary>
        /// <param name="pref">Préférences</param>
        /// <param name="nTab">Tab à administrer</param>
        /// <param name="nParentTab">Tab "parente", pour le cas de l'admin des signet</param>
        /// <param name="typ">Type  de droits/traitements</param>
        /// <returns>Renderer pret</returns>
        public static eAdminRenderer CreateAdminConditionsDialogRenderer(ePref pref, int nTab, int nParentTab, string sIdModal, TypeTraitConditionnal typ)
        {
            eAdminConditionsDialogRenderer rdr = eAdminConditionsDialogRenderer.GetAdminConditionsDialogRenderer(pref, nTab, nParentTab, sIdModal, typ);
            rdr.Generate();
            return rdr;
        }


        /// <summary>
        /// Retourne un renderer pour la fenetre  listes des traitements/droits conditionnels pour l'admin
        /// </summary>
        /// <param name="pref">Préférences</param>
        /// <param name="nDescId">Tab/champ à administrer</param>
        /// <param name="nParentTab">Tab "parente", pour le cas de l'admin des signet</param>
        /// <param name="sIdModal">Id de la modal de la dialog</param>
        /// <param name="nFilter">Filtre sur les droits</param>
        /// <param name="listOnly">indique qu'on ne génère que le rendu de la liste, sans les entetes, filtres, etc.</param>
        /// <returns>Renderer pret</returns>
        public static eAdminRenderer CreateAdminConditionsListDialogRenderer(ePref pref, int nDescId, int nParentTab, string sIdModal, eRules.ConditionsFiltersConcerning nFilter = eRules.ConditionsFiltersConcerning.CURRENT_TAB, bool listOnly = false)
        {
            eAdminConditionsListDialogRenderer rdr = eAdminConditionsListDialogRenderer.GetAdminConditionsListDialogRenderer(pref, nDescId, nParentTab, sIdModal, nFilter);
            rdr.ListOnly = listOnly;

            rdr.Generate();
            return rdr;
        }

        /// <summary>
        /// Rendu de l'éditeur de mapping pour la création de rdv Teams
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nTab"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateTeamsMappingEditorRenderer(ePref pref, int nTab)
        {

            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            eErrorContainer errorContainer = null;
            try
            {
                dal.OpenDatabase();


                eTeamsMapping mapping = eTeamsFactory.GetMapping(pref, nTab);


                eTeamsMappingEditorTools editorTools = eTeamsMappingEditorTools.CreateMappingEditorTools(nTab, pref, dal);

                // si un signet est sélectionné pour les destinataires, on récupère les champs qui peuvent être sélectionnés dans le mapping
                if (mapping.RecipientsFieldsDescIds?.Count > 0)
                {

                    int bkm = mapping.RecipientsFieldsDescIds[0].TabDescId;

                    if (bkm > 0)
                        editorTools.LoadBookmarkEditorTools(bkm, pref, dal);

                }


                eAdminTeamsMappingEditorRenderer rdr = eAdminTeamsMappingEditorRenderer.CreateRenderer(pref, mapping, editorTools, errorContainer: errorContainer);
                return rdr;
            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {
                dal.CloseDatabase();
            }


        }

        /// <summary>
        /// Permet de créer une liste de boutons radio pour savoir quel type de logique on affiche.
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        public static eAdminRenderer CreateAdminFieldLogicDisplayRenderer(ePref pref, eAdminFieldInfos field)
        {
            eAdminFieldCheckBoxPresentation rdr = eAdminFieldCheckBoxPresentation.CreateAdminFieldCheckBoxPresentation(pref, field);
            rdr.Generate();
            return rdr;
        }



    }
}