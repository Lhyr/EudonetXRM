using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.tools.coremodel;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Création du bloc Caractéristiques pour l'admin d'un onglet
    /// </summary>
    public class eAdminFeaturesRenderer : eAdminBlockRenderer
    {
        private bool _bEventStepAvailable = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="eAdminFeaturesRenderer"/> class.
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tabInfos">The tab infos.</param>
        /// <param name="title">The title.</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="tooltip">The tooltip</param>
        private eAdminFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo, String tooltip)
            : base(pref, tabInfos, title, titleInfo, "FeaturesPart", true)
        {
            _tab = tabInfos.DescId;
            this.BlockTitleTooltip = tooltip;

            OpenedBlock = true; //true par défaut
        }

        /// <summary>Création du bloc "Caractéristiques" du menu de droite</summary>
        /// <param name="pref">Préférences</param>
        /// <param name="tabInfos">infos table</param>
        /// <param name="title">Titre</param>
        /// <param name="titleInfo">The title information.</param>
        /// <param name="tooltip">The tooltip</param>
        /// <returns></returns>
        public static eAdminFeaturesRenderer CreateAdminFeaturesRenderer(ePref pref, eAdminTableInfos tabInfos, String title, String titleInfo, String tooltip)
        {
            eAdminFeaturesRenderer features = new eAdminFeaturesRenderer(pref, tabInfos, title, titleInfo, tooltip);
            return features;
        }

        /// <summary>Initialisation des params</summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            //SHA
            //if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminEventStep))
            if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminEventStep) && this._tabInfos.TabType == TableType.EVENT)
            {
                //Cette option n'etant pas réversible, si elle est déjà désactivée il n'est pas nécessaire de faire une requête inutile sur les bkm
                //Pas possible d'activer sur une table Étape
                if (_tabInfos.EventStepEnabled || _tabInfos.IsEventStep)
                    _bEventStepAvailable = false;
                else
                {
                    eudoDAL dal = eLibTools.GetEudoDAL(Pref);
                    try
                    {
                        dal.OpenDatabase();
                        eBkmPref bkmPref = new eBkmPref(Pref, _tab);
                        _bEventStepAvailable = bkmPref.GetLinkedBkmMultiAddDel(dal, true).Count > 0;
                    }
                    finally
                    {
                        dal.CloseDatabase();
                    }
                }
            }

            return true;
        }

        /// <summary>Construction du bloc Caractéristiques</summary>
        /// <returns></returns>
        protected override bool Build()
        {
            //  this.OpenedBlock = true;

            base.Build();

            Dictionary<String, String> listPref = Pref.GetPrefDefault(_tab, new List<String> { "CALENDARENABLED" });

            eAdminField adminField;

            // Nom de l'onglet
            adminField = new eAdminTextboxField(_tab, eResApp.GetResWithColon(Pref, 6817), eAdminUpdateProperty.CATEGORY.RES, Pref.LangId, value: _tabInfos.TableLabel);
            adminField.SetFieldControlID("txtTabName");
            adminField.Generate(_panelContent);

            //Label des résultats
            adminField = new eAdminInfoField(_tab, eResApp.GetResWithColon(this.Pref, 2703), "");
            adminField.Generate(_panelContent);

            // Label des résultats au singulier
            adminField = new eAdminTextboxField(_tab, eResApp.GetRes(Pref, 2699), eAdminUpdateProperty.CATEGORY.RESADV, eLibConst.RESADV_TYPE.RESULT_LABEL_SINGULAR.GetHashCode(),
                value: _tabInfos.ResultsLabelSingular
                , labelType: eAdminTextboxField.LabelType.INLINE
                , customPanelCSSClasses: "LabelResult"
                );
            adminField.Generate(_panelContent);

            // Label des résultats au pluriel
            adminField = new eAdminTextboxField(_tab, eResApp.GetRes(Pref, 2700), eAdminUpdateProperty.CATEGORY.RESADV, eLibConst.RESADV_TYPE.RESULT_LABEL_PLURAL.GetHashCode(),
                value: _tabInfos.ResultsLabelPlural
                , labelType: eAdminTextboxField.LabelType.INLINE
                , customPanelCSSClasses: "LabelResult"
                );
            adminField.Generate(_panelContent);



            // Infobulle
            adminField = new eAdminTextboxField(_tab, eResApp.GetRes(Pref, 6818), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.TOOLTIPTEXT.GetHashCode(), type: AdminFieldType.ADM_TYPE_MEMO, value: _tabInfos.ToolTipText); // TODORES WITHCOLON une fois libellé raccourci
            adminField.Generate(_panelContent);

            // Pictogramme
            adminField = new eAdminPictoField(_tab, eResApp.GetResWithColon(Pref, 6819), eResApp.GetRes(Pref, 6823), _tabInfos.Icon, _tabInfos.IconColor);
            adminField.Generate(_panelContent);

            #region Produit et fonctionnalité 
            if (eFeaturesManager.IsFeatureAvailable(this.Pref, eConst.XrmFeature.AdminProduct))
            {
                #region Fonctionnalité
                bool featureReadOnly = true;
                #region Vérif lecture seule
                if (_tabInfos.ProductID > 0)
                {
                    // Si on est sur un onglet "Produit", seul l'admin produit peut modifier la fonctionnalité
                    featureReadOnly = !eAdminTools.IsUserAllowedForProduct(this.Pref, this.Pref.User, _tabInfos.ProductID);
                }
                else if (this.Pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                {
                    // Sinon tous les administrateurs peuvent modifier la fonctionnalité
                    featureReadOnly = false;
                }
                #endregion

                List<eTabFeature> featuresList = eTabFeature.GetFeaturesList(this.Pref, null);

                adminField = new eAdminTextboxField(_tab, eResApp.GetResWithColon(Pref, 8481), eAdminUpdateProperty.CATEGORY.RESADV, (int)eLibConst.RESADV_TYPE.FEATURE,
                    value: _tabInfos.Feature, optional: true, readOnly: featureReadOnly, id: "txtFeature", autocplList: featuresList.Select(f => f.Feature).ToList());
                adminField.Generate(_panelContent);
                #endregion

                #region Produit : n'est affiché que pour les utilisateurs de niveau 100+
                if (this.Pref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                {
                    string productName = string.Empty;
                    eProduct product = eProduct.GetProduct(this.Pref, _tabInfos.ProductID);
                    if (product != null)
                        productName = product.ProductCode;

                    if (!String.IsNullOrEmpty(productName))
                    {
                        // Produit : champ en lecture seule
                        adminField = new eAdminInfoField(_tab, eResApp.GetResWithColon(this.Pref, 8328), productName);
                        adminField.Generate(_panelContent);
                    }
                }
                #endregion


            }
            #endregion

            if (!_tabInfos.IsEventStep)
            {
                // Afficher en onglet
                if (_tabInfos.EdnType != EdnType.FILE_RELATION
                && !_tabInfos.IsEventStep
                )
                {
                    adminField = new eAdminCheckboxField(_tab, eResApp.GetRes(Pref, 6820), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.ACTIVETAB.GetHashCode(), eResApp.GetRes(Pref, 6824), _tabInfos.ActiveTab); // Tooltip : Cocher ce paramètre pour permettre l'affichage de cet onglet dans la barre de navigation. Si ce paramètre est décoché, l'onglet est uniquement visible sous la forme d'un signet depuis un autre onglet principal.
                    adminField.Generate(_panelContent);
                }

                #region oldcode
                // Mettre la première fiche Adresse créée  automatiquement en Principale
                // Cette options est toujours activée dans engine, commenté pour l'instant
                /*
                if (_tabInfos.TabType == TableType.ADR)
                {

                    Boolean ADR_CREATE_FIRST_MAIN = Pref.GetConfigDefault(new List<eLibConst.CONFIG_DEFAULT> { eLibConst.CONFIG_DEFAULT.ADRCREATEFIRSTMAIN })[eLibConst.CONFIG_DEFAULT.ADRCREATEFIRSTMAIN] == "1"; 

                    eAdminTableInfos pp = new eAdminTableInfos(Pref, (int)TableType.PP);
                    eAdminFieldInfos AdrMainField = eAdminFieldInfos.GetAdminFieldInfos(Pref, (int)AdrField.PRINCIPALE);

                    String labelField = AdrMainField.Labels[0]; // AdrMainField.Length > Pref.LangId ? AdrMainField.Labels[Pref.LangId] : AdrMainField.Labels[0];
                    String label = eResApp.GetRes(Pref, 7652).Replace("<ADDRESS>", _tabInfos.TableLabel).Replace("<MAIN>", labelField);
                    String title = eResApp.GetRes(Pref, 7653).Replace("<ADDRESS>", _tabInfos.TableLabel).Replace("<MAIN>", labelField).Replace("<PP>", pp.TableLabel);

                    adminField = new eAdminCheckboxField(_tab, label, eAdminUpdateProperty.CATEGORY.CONFIGDEFAULT, eLibConst.CONFIG_DEFAULT.ADRCREATEFIRSTMAIN.GetHashCode(), title, ADR_CREATE_FIRST_MAIN); 
                    adminField.Generate(_panelContent);
                }
                */
                #endregion oldcode

                if (_tabInfos.ActiveTab && _tabInfos.EdnType == EdnType.FILE_PLANNING)
                {
                    // Afficher le planning en mode graphique
                    adminField = new eAdminCheckboxField(_tab, eResApp.GetRes(Pref, 7122), eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.CALENDARENABLED.GetHashCode(), eResApp.GetRes(Pref, 7123), listPref["CALENDARENABLED"] == "1"); // Tooltip : Cocher ce paramètre pour permettre l’affichage de l’onglet en mode planning graphique.
                    adminField.SetFieldControlID("chkCalendarEnabled");
                    adminField.Generate(_panelContent);

                    adminField = new eAdminButtonField(eResApp.GetRes(Pref, 7124), "graphicModePref", eResApp.GetRes(Pref, 7125), String.Concat("showPlanningPrefDialog(", _tab, ", 0)"));
                    adminField.Generate(_panelContent);
                }


                if (_tabInfos.TabType == TableType.EVENT || _tabInfos.TabType == TableType.ADR)
                {
                    bool readOnly = false;

                    if (_tabInfos.ProductID > 0)
                    {
                        readOnly = !eAdminTools.IsUserAllowedForProduct(this.Pref, this.Pref.User, _tabInfos.ProductID);
                    }

                    // Créer automatiquement
                    adminField = new eAdminCheckboxField(_tab, eResApp.GetRes(Pref, 6821), eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.AUTOCREATE.GetHashCode(), eResApp.GetRes(Pref, 6825), _tabInfos.AutoCreate, readOnly: readOnly); // Cocher ce paramètre pour permettre la création d’une fiche directement en plein écran. Ce paramètre autorise la création d’une fiche sans tenir compte des champs obligatoires. Son comportement est identique lorsque la création est déclenchée depuis la liste des fiches ou depuis un signet.
                    adminField.Generate(_panelContent);
                }
                else if (_tabInfos.TabType == TableType.TEMPLATE
                        && _tabInfos.EdnType != EdnType.FILE_RELATION
                        && _tabInfos.EdnType != EdnType.FILE_WEBTAB
                        && _tabInfos.EdnType != EdnType.FILE_GRID
                        )
                {
                    // Sauvegarder automatiquement
                    adminField = new eAdminCheckboxField(_tab, eResApp.GetRes(Pref, 6822), eAdminUpdateProperty.CATEGORY.DESC, eLibConst.DESC.AUTOSAVE.GetHashCode(), eResApp.GetRes(Pref, 6826), _tabInfos.AutoSave); // Cocher ce paramètre pour enregistrer automatiquement chaque modification lorsque la fiche est ouverte dans une fenêtre détachée.\r\nUne fenêtre détachée ou popup est une fenêtre réduite, ouverte au premier-plan qui nécessite obligatoirement une action de validation ou d’annulation.
                    adminField.Generate(_panelContent);
                }

                //Activer la création de "étape"            
                if (eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminEventStep)
                    && _tabInfos.TabType == TableType.EVENT)
                {
                    adminField = new eAdminCheckboxField(_tab, eResApp.GetRes(Pref, 1967), eAdminUpdateProperty.CATEGORY.DESCADV, (int)DESCADV_PARAMETER.EVENT_STEP_ENABLED, eResApp.GetRes(Pref, 1968),
                        value: _tabInfos.EventStepEnabled == true, readOnly: _bEventStepAvailable == false);
                    adminField.Generate(_panelContent);
                }
            }

            return true;
        }



    }
}