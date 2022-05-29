using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminNavBarRenderer : eAdminRenderer
    {
        eAdminTableInfos _tabInfos;
        eAdminTableInfos _bkmInfos;
        bool _gridRequested = false;

        Int32 _bkm;
        int _extensionFileId = 0;
        string _extensionLabel = "";
        string _extensionCode = "";


        /// <summary>
        /// Type de module Administration / Options utilisateur concerné par le menu
        /// </summary>
        eUserOptionsModules.USROPT_MODULE _targetModule = eUserOptionsModules.USROPT_MODULE.UNDEFINED;

        public eAdminNavBarRenderer(ePref P, Int32 nTab, Int32 nBkm, eUserOptionsModules.USROPT_MODULE targetModule, int extensionFileId, string extensionLabel, string extensionCode)
        {

            Pref = P;
            _tab = nTab;
            _bkm = nBkm;
            _targetModule = targetModule;
            // TODO: à modifier plus tard suivant les besoins
            if (_tab > 0)
                _tabInfos = new eAdminTableInfos(Pref, _tab);
            if (_bkm > 0)
            {
                // Cas d'un onglet affiché en signet sur lui-même
                // CNA - doublons: ne pas passer dans ce cas particulier
                // BBA - Cas des annexes
                if (_bkm % 100 != 0 && _bkm % 100 != (int)AllField.ATTACHMENT && _bkm != (int)TableType.DOUBLONS)
                    _bkm = _bkm - _bkm % 100;

                _bkmInfos = new eAdminTableInfos(Pref, _bkm);
            }

            // Si la demande concerne le paramètrage de la grille, on bascule au paramètrage de l'onglet parent 
            // en ajoutant le libellé de la grille passé par le javascript
            if (targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TAB_GRID)
            {
                _gridRequested = true;
                _targetModule = eUserOptionsModules.USROPT_MODULE.ADMIN_TAB;
            }

            _extensionFileId = extensionFileId;
            _extensionLabel = extensionLabel;
            _extensionCode = extensionCode;
        }

        public static eAdminNavBarRenderer CreateAdminNavBarRenderer(ePref P, Int32 nTab, Int32 nBkm, eUserOptionsModules.USROPT_MODULE module, int extensionFileId, string extensionLabel, string extensionCode)
        {

            eAdminNavBarRenderer enavbar = new eAdminNavBarRenderer(P, nTab, nBkm, module, extensionFileId, extensionLabel, extensionCode);

            return enavbar;
        }


        /// <summary>
        /// Construction de la navbar admin
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.ID = "adminNav";
            _pgContainer.CssClass = "adminNav";

            #region Partie gauche : Boutons de navigation, et libellé de l'onglet dans le cas de l'administration d'un onglet

            if ((_targetModule == eUserOptionsModules.USROPT_MODULE.UNDEFINED || _targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TAB) && _tabInfos != null)
                _targetModule = eUserOptionsModules.USROPT_MODULE.ADMIN_TABS;

            #region Boutons de navigation (fil d'Ariane des modules)

            HtmlGenericControl ulNavbarButtonUL = new HtmlGenericControl("ul");
            ulNavbarButtonUL.Attributes.Add("class", "navLinks navLinksLeft");
            _pgContainer.Controls.Add(ulNavbarButtonUL);

            // On génère les liens et menus de chaque module en partant du module en cours et
            // en remontant jusqu'au module Administration racine
            //List<HtmlGenericControl> moduleLinksAndMenus = new List<HtmlGenericControl>();
            eUserOptionsModules.USROPT_MODULE currentModule = _targetModule;
            bool gotRootModule = false;
            eConst.XrmFeature feature = eConst.XrmFeature.Undefined;
            while (!gotRootModule)
            {
                feature = eUserOptionsModules.GetModuleFeature(currentModule);
                if (eFeaturesManager.IsFeatureAvailable(Pref, feature))
                {
                    if (currentModule != eUserOptionsModules.USROPT_MODULE.ADMIN_TAB) //Cas particulier: l'admin d'un TAB n'as pas d'entrée menu
                                                                                      // En inserant le parent à la position 0, ca assurera l'ordre inverse du menu final 
                        ulNavbarButtonUL.Controls.AddAt(0, GetNavBarLinkMenu(currentModule));
                }

                gotRootModule = currentModule == eUserOptionsModules.USROPT_MODULE.ADMIN;
                if (!gotRootModule)
                    currentModule = eUserOptionsModules.GetModuleParent(currentModule, false);
            }

            // Puis on ajoute les éléments générés à la structure HTML en parcourant la liste en
            // sens inverse, pour ajouter d'abord les parents, puis les enfants
            //  for (int i = moduleLinksAndMenus.Count - 1; i > -1; i--)
            //     ulNavbarButtonUL.Controls.Add(moduleLinksAndMenus[i]);

            #endregion

            #region Cas particulier de l'administration des onglets : ajout des infos concernant l'onglet actuellement affiché
            if (_targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TABS && _tabInfos != null)
            {
                #region Libellé de l'onglet (avec crayon)
                Panel panelInput = new Panel();
                panelInput.ID = "divLabelTabName";
                panelInput.Attributes.Add("class", "navTitle" + (_gridRequested ? " navTitleSelected" : ""));

                if (_gridRequested)
                {
                    panelInput.Attributes.Add("onclick", String.Concat("nsAdmin.loadAdminFile(", _tab.ToString(), ");"));
                }
                else
                {
                    panelInput.Attributes.Add("onclick", String.Concat("nsAdmin.tabNameOnClick(event, ", _tab.ToString(), ");"));
                    panelInput.ToolTip = _tabInfos.TableLabel;
                }


                HtmlGenericControl labelTab = new HtmlGenericControl("span");
                labelTab.InnerText = _tabInfos.TableLabel;
                // Si on affiche également les propriétés d'un signet : on préfixe le nom de l'onglet avec Onglet "" (#51 135)
                if (_bkmInfos != null)
                    labelTab.InnerText = String.Concat(eResApp.GetRes(Pref, 264).ToUpper(), " \"", labelTab.InnerText, "\"");
                labelTab.ID = "labelTabName";
                labelTab.Attributes.Add("class", "navEntryLabel");
                panelInput.Controls.Add(labelTab);

                // Pas d'edition du libellé d'onglet si on est en param de la grille
                if (!_gridRequested)
                {
                    // Icône crayon
                    HtmlGenericControl iconPen = new HtmlGenericControl("span");
                    iconPen.Attributes.Add("class", "icon-edn-pen");
                    iconPen.Attributes.Add("title", eResApp.GetRes(Pref, 6847));
                    panelInput.Controls.Add(iconPen);
                }

                HtmlGenericControl liNavbarTabOption = new HtmlGenericControl("li");
                liNavbarTabOption.Attributes.Add("class", "navEntry");
                liNavbarTabOption.Controls.Add(panelInput);
                ulNavbarButtonUL.Controls.Add(liNavbarTabOption);
                #endregion

                #region Libellé de l'onglet, mode édition

                // Pas d'edition du libellé d'onglet si on est en param de la grille
                if (!_gridRequested)
                {
                    panelInput = new Panel();
                    panelInput.ID = "divInputTabName";
                    panelInput.Attributes.Add("class", "navTitle");
                    panelInput.Style.Add("display", "none");
                    HtmlInputText inpt = new HtmlInputText();
                    inpt.ID = "inputTabName";
                    inpt.Value = _tabInfos.TableLabel;
                    inpt.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.RES.GetHashCode(), "|", Pref.LangId));
                    inpt.Attributes.Add("did", _tab.ToString()); // Ajout du DescID pour simplifier la mise à jour via JSON
                    inpt.Attributes.Add("onblur", "nsAdmin.updateTabNameOnBlur(this);");
                    HtmlGenericControl iconPen = new HtmlGenericControl("span");
                    iconPen.Attributes.Add("class", "icon-edn-pen");

                    panelInput.Controls.Add(inpt);
                    panelInput.Controls.Add(iconPen);

                    liNavbarTabOption = new HtmlGenericControl("li");
                    liNavbarTabOption.Attributes.Add("class", "navEntry");
                    liNavbarTabOption.Controls.Add(panelInput);
                    ulNavbarButtonUL.Controls.Add(liNavbarTabOption);
                }

                #endregion

                if (_bkmInfos != null)
                {
                    #region Libellé du signet (avec crayon)
                    panelInput = new Panel();
                    panelInput.ID = "divLabelBkmName";
                    panelInput.Attributes.Add("class", "navTitle");
                    panelInput.Attributes.Add("onclick", String.Concat("nsAdmin.bkmNameOnClick(event, ", _bkmInfos.DescId, ");"));
                    panelInput.ToolTip = _bkmInfos.TableLabel;

                    HtmlGenericControl labelBkm = new HtmlGenericControl("span");
                    labelBkm.InnerText = String.Concat(eResApp.GetRes(Pref, 7587).ToUpper(), " \"", _bkmInfos.TableLabel, "\""); // Signet ""
                    labelBkm.ID = "labelBkmName";
                    labelBkm.Attributes.Add("class", "navEntryLabel");
                    panelInput.Controls.Add(labelBkm);

                    // Icône crayon
                    HtmlGenericControl iconPen = new HtmlGenericControl("span");
                    iconPen.Attributes.Add("class", "icon-edn-pen");
                    iconPen.Attributes.Add("title", eResApp.GetRes(Pref, 6847));
                    panelInput.Controls.Add(iconPen);

                    liNavbarTabOption = new HtmlGenericControl("li");
                    liNavbarTabOption.Attributes.Add("class", "navEntry");
                    liNavbarTabOption.Controls.Add(panelInput);
                    ulNavbarButtonUL.Controls.Add(liNavbarTabOption);
                    #endregion

                    #region Libellé du signet, mode édition
                    panelInput = new Panel();
                    panelInput.ID = "divInputBkmName";
                    panelInput.Attributes.Add("class", "navTitle");
                    panelInput.Style.Add("display", "none");
                    HtmlInputText inpt = new HtmlInputText();
                    inpt.ID = "inputBkmName";
                    inpt.Value = _bkmInfos.TableLabel;
                    inpt.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.RES.GetHashCode(), "|", Pref.LangId));
                    inpt.Attributes.Add("did", _bkm.ToString()); // Ajout du DescID pour simplifier la mise à jour via JSON
                    inpt.Attributes.Add("onblur", "nsAdmin.updateBkmNameOnBlur(this);");
                    iconPen = new HtmlGenericControl("span");
                    iconPen.Attributes.Add("class", "icon-edn-pen");

                    panelInput.Controls.Add(inpt);
                    panelInput.Controls.Add(iconPen);

                    liNavbarTabOption = new HtmlGenericControl("li");
                    liNavbarTabOption.Attributes.Add("class", "navEntry");
                    liNavbarTabOption.Controls.Add(panelInput);
                    ulNavbarButtonUL.Controls.Add(liNavbarTabOption);
                    #endregion
                }

                else if (_gridRequested)
                {
                    #region affichage et edition du libellé de la grille (avec crayon)

                    AddNavBarLinkItem(ulNavbarButtonUL, true,
                    delegate (Panel panelLabel, HtmlGenericControl textLabel, HtmlInputText textInput)
                    {
                        panelLabel.Attributes.Add("class", "navTitle navTitleSelected");
                        panelLabel.Attributes.Add("onclick", String.Concat("nsAdmin.gridNameOnClick(this);"));
                        panelLabel.ToolTip = "Cliquez pour modifier le libellé de la grille";

                        textLabel.InnerText = _extensionLabel;

                        textInput.Attributes.Add("did", ((int)XrmGridField.Title).ToString());
                        textInput.Attributes.Add("gid", _extensionFileId.ToString());
                        textInput.Value = _extensionLabel;
                        textInput.Attributes.Add("onblur", "nsAdmin.updateGridNameOnBlur(this);");
                    });

                    #endregion
                }
            }
            #endregion

            #region Cas particulier de l'administration d'un utilisateur
            if (_targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TAB_USER && _tabInfos != null)
            {
                //a voir selon spécifs 

            }


            #endregion

            // Ajout des liens speifiques au fil d'ariane
            AddSpecificNavBarLinks(ulNavbarButtonUL);



            #endregion

            //HtmlGenericControl ulNavbarOptionUL = new HtmlGenericControl("div");
            //ulNavbarOptionUL.Attributes.Add("class", "navLinks navLinksRight");
            //_pgContainer.Controls.Add(ulNavbarOptionUL);

            //Quitter l'admin
            HtmlGenericControl divNavBarOption = new HtmlGenericControl("div");
            divNavBarOption.ID = "navLeaveAdmin";
            Panel navTitleOption = new Panel();
            navTitleOption.CssClass = "navTitle";
            navTitleOption.ID = "tab_header_" + _tab;
            navTitleOption.Attributes.Add("onclick", "nsAdmin.reloadHome();");

            HtmlGenericControl lblOption = new HtmlGenericControl("span");
            lblOption.Attributes.Add("class", "icon-item_rem");
            navTitleOption.Controls.Add(lblOption);
            lblOption = new HtmlGenericControl("span");
            lblOption.InnerHtml = HttpUtility.HtmlEncode(eResApp.GetRes(Pref, 7231)); //"Retour à l'utilisation";
            navTitleOption.Controls.Add(lblOption);

            divNavBarOption.Controls.Add(navTitleOption);

            if (_tab != 0)
                divNavBarOption.Controls.Add(CreateMruMenu());

            // 56 608 - Ajout des paramètres ExtensionFileId et ExtensionLabel dans des champs cachés sur le corps de page afin que nsAdmin.loadAdminModule() puisse
            // récupérer ces informations si elles ne lui sont pas transmises autrement
            HtmlInputHidden hExtensionFileId = new HtmlInputHidden();
            hExtensionFileId.ID = "extensionFileId";
            hExtensionFileId.Name = hExtensionFileId.ID;
            hExtensionFileId.Value = _extensionFileId.ToString();

            HtmlInputHidden hExtensionLabel = new HtmlInputHidden();
            hExtensionLabel.ID = "extensionLabel";
            hExtensionFileId.Name = hExtensionLabel.ID;
            hExtensionLabel.Value = _extensionLabel;

            //SHA : tâche #1 873
            HtmlInputHidden hExtensionCode = new HtmlInputHidden();
            hExtensionLabel.ID = "extensionCode";
            hExtensionFileId.Name = hExtensionCode.ID;
            hExtensionLabel.Value = _extensionCode;

            _pgContainer.Controls.Add(hExtensionFileId);
            _pgContainer.Controls.Add(hExtensionLabel);

            _pgContainer.Controls.Add(divNavBarOption);

            return true;
        }

        /// <summary>
        /// Ajout un lien specific editable de la navebar
        /// </summary>
        /// <param name="ulNavbarButtonUL"></param>
        /// <param name="func"></param>
        protected void AddNavBarLinkItem(HtmlGenericControl ulNavbarButtonUL, bool editable, Action<Panel, HtmlGenericControl, HtmlInputText> func)
        {
            Panel panelInput = new Panel();
            panelInput.ID = "divLabelTabName";
            panelInput.Attributes.Add("edit", "0");

            // Label pour affichage
            HtmlGenericControl labelTab = new HtmlGenericControl("span");
            labelTab.ID = "labelTabName";
            labelTab.Attributes.Add("class", "navEntryLabel");
            panelInput.Controls.Add(labelTab);

            // input pour l'edition
            HtmlInputText inpt = new HtmlInputText();
            inpt.ID = "inputTabName";
            if (editable)
            {
                panelInput.Controls.Add(inpt);
            }

            // Icône crayon
            if (editable)
            {
                HtmlGenericControl iconPen = new HtmlGenericControl("span");
                iconPen.Attributes.Add("class", "icon-edn-pen");
                panelInput.Controls.Add(iconPen);
            }

            HtmlGenericControl liNavbarTabOption = new HtmlGenericControl("li");
            liNavbarTabOption.Attributes.Add("class", "navEntry navModuleWithMenu");
            liNavbarTabOption.Controls.Add(panelInput);

            ulNavbarButtonUL.Controls.Add(liNavbarTabOption);

            // passe la main au client pour ajouter des infos des attributs 
            func(panelInput, labelTab, inpt);
        }

        /// <summary>
        /// Ajoute des liens specifique au fil d'ariane
        /// </summary>
        /// <param name="ulNavbarButtonUL"></param>
        protected virtual void AddSpecificNavBarLinks(HtmlGenericControl ulNavbarButtonUL)
        {

        }

        /// <summary>
        /// Génère un contrôle correspondant à un bouton de la navbar, avec son libellé, son icône et son lien
        /// </summary>
        /// <param name="label">Libellé du bouton</param>
        /// <param name="icon">Icône (classe CSS font) du bouton</param>
        /// <param name="tooltip">Info-bulle du bouton</param>
        /// <param name="href">Lien (JavaScript) du bouton</param>
        /// <param name="cssClass">Classe CSS à ajouter sur le bouton</param>
        /// <param name="subContainerCssClass">Classe CSS à ajouter sur le conteneur interne du bouton</param>
        /// <param name="bWidthSubMenu">Affiche une flèche bas lorsque le lien contient un sous-menu</param>
        /// 
        /// <returns>Le contrôle bouton généré</returns>
        protected HtmlGenericControl GetNavBarLink(string label, string icon, string tooltip, string href, string cssClass = "navTitle", string subContainerCssClass = "", string textCssClass = "", Boolean bWidthSubMenu = false)
        {
            HtmlGenericControl button = new HtmlGenericControl("li");
            button.Attributes.Add("class", cssClass);
            if (tooltip.Length > 0)
                button.Attributes.Add("title", tooltip);

            Panel navTitle = new Panel();
            if (subContainerCssClass.Length > 0)
                navTitle.CssClass = subContainerCssClass;

            // Ancre vers un point de la page : nécessite une balise lien <a>
            if (href.StartsWith("#"))
            {
                HyperLink link = new HyperLink();
                link.NavigateUrl = href;
                link.Controls.Add(navTitle);
                button.Controls.Add(link);
            }
            // Autre : peut fonctionner avec un onclick sur la balise <div>
            else
            {
                navTitle.Attributes.Add("onclick", href);
                button.Controls.Add(navTitle);
            }

            HtmlGenericControl lbl = new HtmlGenericControl("span");
            lbl.Attributes.Add("class", icon);
            navTitle.Controls.Add(lbl);
            lbl = new HtmlGenericControl("span");
            string spanLabelClass = "navEntryLabel";
            if (textCssClass.Length > 0)
                spanLabelClass = String.Concat(spanLabelClass, " ", textCssClass);
            lbl.Attributes.Add("class", spanLabelClass);
            navTitle.Controls.Add(lbl);
            lbl.InnerHtml = label;

            if (bWidthSubMenu)
            {
                HtmlGenericControl spanIcon = new HtmlGenericControl();
                spanIcon.Attributes.Add("class", "icon-caret-down");
                navTitle.Controls.Add(spanIcon);
            }

            return button;
        }


        /// <summary>
        /// Lien cliquableRendu du module 
        /// </summary>
        /// <param name="currentModule"></param>
        /// <returns></returns>
        protected virtual HtmlGenericControl GetNavBarLinkMenu(eUserOptionsModules.USROPT_MODULE currentModule)
        {


            // Cas particulier : si on n'est pas dans le module "Paramètres généraux", lien vers le module et l'ancre
            // Sinon ancre vers la sous-partie correspondante
            // TODO: à rendre plus générique avec une fonction similaire à ModuleExists renvoyant false pour les modules dépendants d'un autre ?
            string moduleLink = String.Concat("nsAdmin.loadAdminModule('", currentModule, "',null, { tab: ", this._tab, ", extensionFileId: ", _extensionFileId, ", extensionCode: '", _extensionCode, "', extensionLabel: '", _extensionLabel.Replace("'", "\\'"), "' } );");
            eUserOptionsModules.USROPT_MODULE parentModule = eUserOptionsModules.GetModuleParent(currentModule, false);
            if (parentModule == eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
            {
                moduleLink = String.Concat("nsAdmin.loadAdminModule('", eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL, "');");
                if (_targetModule == currentModule || _targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
                    moduleLink = String.Concat("#", currentModule);
            }
            else if (currentModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TAB_USER)
            {

                moduleLink = "";
            }
            else if (currentModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TABS)
            {

                moduleLink = String.Concat("nsAdmin.loadAdminModule('", eUserOptionsModules.USROPT_MODULE.ADMIN_TABS, "');");
            }

            Boolean bTabAdmin = _targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_TABS && _tabInfos != null;

            // Cas des extensions issues du store : on récupère le titre passé en JavaScript du mode Liste au mode Fiche
            string moduleLabel = _extensionLabel;
            if (parentModule != eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS)
                moduleLabel = eUserOptionsModules.GetModuleLabel(currentModule, Pref);

            List<eUserOptionsModules.USROPT_MODULE> childrenModules = eUserOptionsModules.GetModuleChildren(currentModule);
            HtmlGenericControl currentModuleLink = GetNavBarLink(
                        moduleLabel,
                        eUserOptionsModules.GetModuleIcon(currentModule),
                        eUserOptionsModules.GetModuleTooltip(currentModule, Pref),
                        moduleLink,
                        "navEntry",
                        currentModule == _targetModule && !bTabAdmin ? "navTitle navTitleSelected" : "navTitle",
                        bWidthSubMenu: (currentModule != eUserOptionsModules.USROPT_MODULE.ADMIN_TABS && !currentModule.ToString().Contains("ADMIN_EXTENSION") && childrenModules.Count > 0) ? true : false // pas de petite flèche bas pour le module Onglets
                );
            List<eUserOptionsModules.USROPT_MODULE> addedChildModules = new List<eUserOptionsModules.USROPT_MODULE>();
            if (childrenModules.Count > 0)
            {
                HtmlGenericControl ulCurrentModuleMenu = new HtmlGenericControl("ul");
                ulCurrentModuleMenu.Attributes.Add("class", "sbMenu");
                // l'ajout du menu au lien en cours sera fait si au moins une entrée est ajoutée au menu
                HtmlGenericControl liCurrentModuleMenu = new HtmlGenericControl("li");
                liCurrentModuleMenu.Attributes.Add("class", "sbmA");
                ulCurrentModuleMenu.Controls.Add(liCurrentModuleMenu);
                HtmlGenericControl ulCurrentModuleSubMenu = new HtmlGenericControl("ul");
                ulCurrentModuleSubMenu.Attributes.Add("class", "Action_sMC");
                liCurrentModuleMenu.Controls.Add(ulCurrentModuleSubMenu);
                foreach (eUserOptionsModules.USROPT_MODULE childModule in childrenModules)
                {
                    // Si le parent immédiat du module se trouve déjà dans le menu, on ne l'ajoute pas
                    // Exemple : cas de USROPT_MODULE_ADMIN_ACCESS_USERGROUPS, qui est enfant de USROPT_MODULE_ADMIN, mais également de USROPT_MODULE_ADMIN
                    // On ne doit donc pas l'ajouter dans ce menu qui ne doit contenir que les enfants directs du module en cours (USROPT_MODULE_ADMIN dans notre exemple)
                    // Sauf s'il s'agit du menu du module Administration racine qui, lui, doit comporter tous les liens de l'ensemble de l'admin
                    parentModule = eUserOptionsModules.GetModuleParent(childModule, false);
                    bool parentModuleAdded = addedChildModules.Contains(parentModule);
                    if (eUserOptionsModules.ModuleCanAppearInNavbar(Pref, childModule) &&
                        eUserOptionsModules.ModuleChildrenCanAppearInNavBar(parentModule) &&
                        (currentModule == eUserOptionsModules.USROPT_MODULE.ADMIN || !parentModuleAdded)
                    )
                    {
                        addedChildModules.Add(childModule);

                        string linkClass = "navAction";
                        // Si on ajoute un enfant de module déjà présent, on lui adjoint une classe pour matérialiser la hiérarchie (ex : retrait à gauche)
                        if (parentModuleAdded)
                            linkClass = String.Concat(linkClass, " navSubAction");
                        else if (currentModule == eUserOptionsModules.USROPT_MODULE.ADMIN)
                            linkClass = String.Concat(linkClass, " navRootAction");
                        // De même s'il s'agit du module actuellement affiché
                        if (childModule == _targetModule)
                            linkClass = String.Concat(linkClass, " navActionSelected");

                        // Cas particulier : si on n'est pas dans le module "Paramètres généraux", lien vers le module et l'ancre
                        // Sinon ancre vers la sous-partie correspondante
                        // TODO: à rendre plus générique avec une fonction similaire à ModuleExists renvoyant false pour les modules dépendants d'un autre ?
                        string childModuleLink = String.Concat("nsAdmin.loadAdminModule('", childModule, "');");
                        if (parentModule == eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
                        {
                            childModuleLink = String.Concat("nsAdmin.loadAdminModule('", eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL, "');");
                            if (_targetModule == childModule || _targetModule == eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL)
                                childModuleLink = String.Concat("#", childModule);
                        }

                        // Cas des extensions issues du store : on récupère le titre passé en JavaScript du mode Liste au mode Fiche
                        moduleLabel = _extensionLabel;
                        if (parentModule != eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS)
                            moduleLabel = eUserOptionsModules.GetModuleLabel(childModule, Pref);


                        ulCurrentModuleSubMenu.Controls.Add(
                            GetNavBarLink(
                                moduleLabel,
                                eUserOptionsModules.GetModuleIcon(childModule),
                                eUserOptionsModules.GetModuleTooltip(childModule, Pref),
                                childModuleLink,
                                linkClass,
                                String.Empty,
                                "navActionText"
                            )
                        );
                    }
                }
                // Si des liens ont été générés dans le sous-menu, on ajoute les éléments nécessaires à son affichage dans la structure HTML
                if (ulCurrentModuleSubMenu.Controls.Count > 0)
                {
                    currentModuleLink.Attributes["class"] = String.Concat(currentModuleLink.Attributes["class"], " navModuleWithMenu");
                    if (currentModule == _targetModule)
                        currentModuleLink.Attributes["class"] = String.Concat(currentModuleLink.Attributes["class"], " navModuleWithMenuSelected");
                    currentModuleLink.Controls.Add(ulCurrentModuleMenu);
                }
            }

            return currentModuleLink;
        }

        /// <summary>Création du menu des MRU</summary>
        /// <returns></returns>
        HtmlGenericControl CreateMruMenu()
        {
            HtmlGenericControl ulMenu = new HtmlGenericControl("ul");
            ulMenu.Attributes.Add("class", "sbMenu");

            // MRU
            HtmlGenericControl liMru = new HtmlGenericControl("li");
            liMru.Attributes.Add("class", "sbmMRU");

            HtmlGenericControl ulMru = new HtmlGenericControl("ul");
            ulMru.ID = String.Concat("ul_mru_", _tab);
            ulMru.Attributes.Add("class", "listResult_sMC2");
            liMru.Controls.Add(ulMru);

            // Actions
            HtmlGenericControl liActions = new HtmlGenericControl("li");
            liActions.Attributes.Add("class", "sbmA");

            HtmlGenericControl ulActions = new HtmlGenericControl("ul");
            ulActions.Attributes.Add("class", "Action_sMC");

            HtmlGenericControl liNavAction = new HtmlGenericControl("li");
            liNavAction.Attributes.Add("class", "navAction");

            if (_tabInfos != null)
            {
                if (_tabInfos.EdnType != EdnType.FILE_PLANNING)
                    liNavAction.Attributes.Add("onclick", "javascript:nsAdmin.goTabList(" + _tab + ");");
                else
                    liNavAction.Attributes.Add("onclick", "javascript:nsAdmin.goTabList(" + _tab + ");setCalViewMode(0, " + _tab + ");");
                liNavAction.InnerText = eResApp.GetRes(Pref, 1485);
            }
            ulActions.Controls.Add(liNavAction);

            liActions.Controls.Add(ulActions);

            ulMenu.Controls.Add(liMru);
            ulMenu.Controls.Add(liActions);

            return ulMenu;
        }
    }
}