using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du mode fiche d'une extension disponible en administration
    /// </summary>
    public abstract class eAdminExtensionFileRenderer : eAdminExtensionRenderer
    {
        private eAdminExtension _extension = null;
        private Panel _extensionParametersContainer = null;
        private IDictionary<string, string> _extensionTabs = null;
        private string _initialTab = "description";

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionFileRenderer(ePref pref, eAdminExtension extension, string initialTab)
            : base(pref)
        {
            Pref = pref;
            _extension = extension;
            _initialTab = initialTab;
        }

        public Panel ExtensionParametersContainer
        {
            get
            {
                return _extensionParametersContainer;
            }
        }

        public eAdminExtension Extension
        {
            get
            {
                return _extension;
            }
        }


        protected eExtension _eRegistredExt = null;

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminExtensions))
                return false;

            if (base.Init())
            {
                if( !string.IsNullOrWhiteSpace( this.Extension?.Infos?.ExtensionNativeId))
                {
                   var lst = eExtension.GetExtensionsByCode(Pref, this.Extension.Infos.ExtensionNativeId);
                    if (lst.Count == 1)
                        _eRegistredExt = lst[0];

                }

                // TODO/TOCHECK: ne pas afficher certains onglets s'ils ne contiennent aucune info ? (ex : visuels)
                _extensionTabs = new Dictionary<string, string>();
                _extensionTabs.Add("description", eResApp.GetRes(Pref, 7900)); // "Descriptif"
                _extensionTabs.Add("screenshots", eResApp.GetRes(Pref, 7901)); // "Visuels"
                _extensionTabs.Add("reviews", eResApp.GetRes(Pref, 7902)); // "Avis"
                _extensionTabs.Add("install", eResApp.GetRes(Pref, 7903)); // "Installation"
                _extensionTabs.Add("versions", eResApp.GetRes(Pref, 7904)); // "Versions"
                _extensionTabs.Add("info", eResApp.GetRes(Pref, 7905)); // "Informations"
                _extensionTabs.Add("settings", eResApp.GetRes(Pref, 7906)); // "Paramètres"

                return true;
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                Panel pnlContainer = new Panel();
                pnlContainer.CssClass = "admin_cont";
                _pgContainer.Controls.Add(pnlContainer);

                // Rappel des infos de l'extension affichées en mode Liste
                pnlContainer.Controls.Add(GetExtensionPanel());

                // Ajout des onglets/signets de l'extension
                pnlContainer.Controls.Add(GetExtensionFileTabs());

                // Ajout des conteneurs des onglets/signets
                List<Panel> panels = GetExtensionFilePanels();
                foreach (Panel panel in panels)
                    pnlContainer.Controls.Add(panel);

                // Le dernier conteneur généré est celui destiné à recevoir les paramètres spécifiques de chaque extension
                // On l'affecte donc dans une propriété du renderer pour que chaque extension y incruste ses paramètres
                Panel panelParameters = panels[panels.Count - 1];

                _extensionParametersContainer = new Panel();
                panelParameters.Controls.Add(_extensionParametersContainer);

                #region Ajout des JS permettant d'initialiser le rendu
                AddCallBackScript(
                    String.Concat(
                        "nsAdmin.initExtensionFile(); "
                    )
                );
                #endregion

                return true;
            }

            return false;
        }

        public Panel GetExtensionPanel()
        {
            return GetExtensionPanel(Pref, Extension, false);
        }

        public override Panel GetExtensionPanel(eAdminExtension specificExtension)
        {
            return GetExtensionPanel(Pref, specificExtension, false);
        }

        private System.Web.UI.WebControls.Table GetExtensionFileTabs()
        {
            System.Web.UI.WebControls.Table tabBkm = new System.Web.UI.WebControls.Table();
            tabBkm.ID = String.Concat("extensionTabs");
            tabBkm.CssClass = "bkmBar";

            TableRow trBkm = new TableRow();
            tabBkm.Rows.Add(trBkm);
            trBkm.CssClass = "bkmTr";
            trBkm.ID = "bkmtr";

            TableCell tdBkmSep;
            IDictionary<Int32, IList<TableCell>> bkmPages = new Dictionary<Int32, IList<TableCell>>();
            IList<TableCell> bkmTds = new List<TableCell>();

            #region Création des cellules correspondant à chaque onglet
            bool showParametersTab = Extension.ShowParametersTab && Extension.Infos.IsEnabled;
            foreach (KeyValuePair<string, string> kvp in _extensionTabs)
            {
                // "Le signet « Paramètres » est affiché si le module est activé sauf s’il ne requiert pas d’autre paramètre de fonctionnement que celui de son activation."
                bool isVisible = kvp.Key != "settings" || (kvp.Key == "settings" && Extension.ShowParametersTab && Extension.Infos.IsEnabled);

                if (kvp.Key == "reviews" || kvp.Key == "info")
                    isVisible = false;

                // Par défaut, on affiche l'onglet "Description", sauf si le JS appelant le précise autrement
                //bool isSelected = showParametersTab ? kvp.Key == "settings" : kvp.Key == "description";
                bool isSelected = (kvp.Key == _initialTab);
                if (kvp.Key == "settings" && !showParametersTab)
                    isSelected = false;
                if (kvp.Key == "description" && _initialTab == "settings" && !showParametersTab)
                    isSelected = true;

                TableCell tdBkm = new TableCell();
                trBkm.Cells.Add(tdBkm);
                tdBkm.CssClass = String.Concat("bkmHead", isSelected ? "Sel" : "");
                tdBkm.Style.Add(HtmlTextWriterStyle.Display, isVisible ? "table-cell" : "none");
                tdBkm.ID = String.Concat("extensionBkm_", kvp.Key);

                HyperLink htmlLink = new HyperLink();
                htmlLink.Attributes.Add("class", "txtAll");
                htmlLink.Text = HttpUtility.HtmlEncode(kvp.Value);
                htmlLink.Attributes.Add("onclick", String.Concat("nsAdmin.displayExtensionTab('", kvp.Key, "');"));
                tdBkm.Controls.Add(htmlLink);

                // Séparateur, sauf sur l'onglet Paramètres
                // On crée malgré tout systématiquement le séparateur de Informations dans le DOM, même si Paramètres est masqué,
                // car il doit pouvoir être réaffiché réaffiché en JS en rendant l'onglet Paramètres visible
                if (kvp.Key != "settings")
                {
                    tdBkmSep = new TableCell();
                    trBkm.Cells.Add(tdBkmSep);
                    tdBkmSep.CssClass = "bkmSep";
                    if (!isVisible || (isVisible && kvp.Key == "info" && !showParametersTab))
                        tdBkmSep.Style.Add(HtmlTextWriterStyle.Display, "none");
                    tdBkmSep.ID = String.Concat("extensionBkmSep_", kvp.Key);
                }
            }
            #endregion

            return tabBkm;
        }

        private List<Panel> GetExtensionFilePanels()
        {
            List<Panel> panels = new List<Panel>();

            #region Création des conteneurs correspondant à chaque onglet
            bool showParametersTab = Extension.ShowParametersTab && Extension.Infos.IsEnabled;
            foreach (KeyValuePair<string, string> kvp in _extensionTabs)
            {
                // Par défaut, on affiche l'onglet "Description", sauf si le JS appelant le précise autrement
                //bool isSelected = showParametersTab ? kvp.Key == "settings" : kvp.Key == "description";
                bool isSelected = (kvp.Key == _initialTab);
                if (kvp.Key == "settings" && !showParametersTab)
                    isSelected = false;
                if (kvp.Key == "description" && _initialTab == "settings" && !showParametersTab)
                    isSelected = true;

                Panel panel = new Panel();
                panel.ID = String.Concat("extensionCnt_", kvp.Key);
                panel.CssClass = "extensionCnt";
                panel.Style.Add(HtmlTextWriterStyle.Display, isSelected ? "block" : "none");

                switch (kvp.Key)
                {
                    case "description":
                        panel.Controls.Add(GetExtensionFilePanelContentsDescription());
                        break;
                    case "screenshots":
                        panel.Controls.Add(GetExtensionFilePanelContentsScreenshots());
                        break;
                    case "reviews":
                        panel.Controls.Add(GetExtensionFilePanelContentsReviews());
                        break;
                    case "install":
                        panel.Controls.Add(GetExtensionFilePanelContentsInstall());
                        break;
                    case "versions":
                        panel.Controls.Add(GetExtensionFilePanelContentsVersions());
                        break;
                    case "info":
                        panel.Controls.Add(GetExtensionFilePanelContentsInfo());
                        break;
                        // L'onglet Paramètres sera alimenté par chaque renderer d'extension en accédant à la propriété spécifique ExtensionParametersContainer
                }
                panels.Add(panel);
            }
            #endregion

            return panels;
        }




        /// <summary>
        /// Renvoie le contenu de l'onglet Visuels de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsScreenshots()
        {
            HtmlGenericControl li;

            Panel carousel = new Panel();
            carousel.CssClass = "js-carousel";
            carousel.ID = "screenshotsCarousel";

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            foreach (eAPIProductScreenshot ss in _extension.Infos.Screenshots)
            {
                ul.Controls.Add(ss.CreateImageItem());
            }

            carousel.Controls.Add(ul);

            return carousel;
        }

        /// <summary>
        /// Renvoie le contenu de l'onglet Avis de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsReviews()
        {
            return new Panel();
        }

        #region PANEL TYPE HTML


        /// <summary>
        /// Renvoie le contenu de l'onglet Descriptif de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsDescription()
        {
            //Produit Descriptio.Description ( evt89)
            return GetExtensionFilePanelContentsHTMLContent("Description", _extension.Infos.Description);
        }

        /// <summary>
        /// Renvoie le contenu de l'onglet Installation de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsInstall()
        {
            //Produit Descriptio.Installaion ( evt03)
            return GetExtensionFilePanelContentsHTMLContent("Installation", _extension.Infos.InstallationInfos);
        }

        /// <summary>
        /// Renvoie le contenu de l'onglet Versions de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsVersions()
        {
            return GetExtensionFilePanelContentsHTMLContent("Version", _extension.Infos.Version);
        }

        /// <summary>
        /// Signet Extension type HTML
        /// Le signet doit être initialisé via  le js nsAdmin.initHtmlContent(sName);
        /// </summary>
        /// <param name="sName">Nom du controle</param>
        /// <param name="sContent">Contenu du control</param>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsHTMLContent(string sName, string sContent)
        {
            Panel versionPanel = new Panel();

            HtmlGenericControl descriptionFrame = new HtmlGenericControl("iframe");
            descriptionFrame.ID = "extension" + sName + "ContentsContainer";
            descriptionFrame.Attributes.Add("class", "extensionDescriptionContentsContainer");
            versionPanel.Controls.Add(descriptionFrame);


            // .InnerHtml est inopérant sur une iframe. Le contenu est donc injecté dans un contrôle annexe caché, puis sera réinjecté dans l'iframe en JS
            HtmlGenericControl descriptionFrameContents = new HtmlGenericControl("div");
            descriptionFrameContents.ID = "extension" + sName + "Contents";
            descriptionFrameContents.Attributes.Add("class", "extension" + sName + "Contents  baseExtensionContentsBody");
            descriptionFrameContents.InnerHtml = sContent;
            descriptionFrameContents.Style.Add(HtmlTextWriterStyle.Display, "none");
            versionPanel.Controls.Add(descriptionFrameContents);

            return versionPanel;
        }


        #endregion

        /// <summary>
        /// Renvoie le contenu de l'onglet Informations de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Control GetExtensionFilePanelContentsInfo()
        {
            return GetExtensionFilePanelContentsHTMLContent("Informations", "");
        }
    }
}