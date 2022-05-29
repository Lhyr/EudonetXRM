using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de la liste des modules disponibles en administration
    /// </summary>
    public class eAdminStoreListRenderer : eAdminStoreRenderer
    {
        /// <summary>
        /// 
        /// </summary>
        const int PRODUCTPERPAGE = 22;

        /// <summary>
        /// Extensions correspondant aux critères de recherche ET à la pagination
        /// </summary>
        protected List<eAdminExtension> _includedExtensionList;

        /// <summary>
        /// Page à charger
        /// </summary>
        private int CurrentPage { get; set; }

        /// <summary>
        /// Indique le nombre d'extensions à afficher par page
        /// </summary>
        private int ProductPerPage { get; set; }

        /// <summary>
        /// Total des extensions
        /// </summary>
        public int TotalExtensionsCount { get; set; } = 0;

        public int TotalExtensionsPages { get; set; } = 0;

        private StoreListTypeRefresh TypRefresh { get; set; }

        private StoreListCriteres Criteres { get; set; }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nPage"></param>
        /// <param name="nRows"></param>
        /// <param name="typRefresh"></param>
        /// <param name="criteres"></param>
        private eAdminStoreListRenderer(ePref pref, int nPage, int nRows, StoreListTypeRefresh typRefresh, StoreListCriteres criteres)
            : base(pref)
        {
            Pref = pref;
            ProductPerPage = nRows <= 0 ? PRODUCTPERPAGE : nRows;
            CurrentPage = nPage <= 0 ? 1 : nPage;
            Criteres = criteres;
            TypRefresh = typRefresh;
        }

        /// <summary>
        /// Static d'acces au constructeur
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="nPage"></param>
        /// <param name="nRows"></param>
        /// <param name="typRefresh"></param>
        /// <param name="criteres"></param>
        /// <returns></returns>
        public static eAdminStoreListRenderer CreateAdminStoreListRenderer(ePref pref, int nPage, int nRows, StoreListTypeRefresh typRefresh, StoreListCriteres criteres)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminStoreListRenderer(pref, nPage, nRows, typRefresh, criteres);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminExtensions))
                return false;

            bool bStoreAccessOk = false;

            #region Ajout des extensions à partir de la liste référencée sur HotCom

            if (bInternet)
            {
                int extensionsCount, extensionsPages;
                eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(Pref);
                _includedExtensionList = storeAccess.GetExtensionList(CurrentPage, ProductPerPage, Criteres, out extensionsCount, out extensionsPages);

                if (storeAccess.ApiErrors.Trim().Length == 0)
                {
                    bStoreAccessOk = true;
                    TotalExtensionsCount = extensionsCount;
                    TotalExtensionsPages = extensionsPages;
                }
            }

            #endregion

            #region En cas d'échec : ajout des extensions natives

            if (!bStoreAccessOk)
            {
                // TODO - Faut revoir cette partie, il existe des propriétés plus utilisé et les criteres ne semble plus etre repris pour le mode deconnecté
                _includedExtensionList = new List<eAdminExtension>();

                //Chargement de la liste extension depuis le json
                List<eAdminExtension> extensionFromJson = eAdminExtension.initListExtensionFromJson(Pref);

                //Gestion pagination et recherche
                for (int i = 0; i < extensionFromJson.Count; i++)
                {
                    eAdminExtension currentExtension = extensionFromJson[i];

                    if (
                        // Filtre sur les termes de recherche
                        (
                            !Criteres.FilterSearchEnabled ||
                            (
                                    //CurrentSearch.Trim().Length > 0 && ( //CNA - Inutile car déjà testé dans FilterCategoryEnabled
                                    (!string.IsNullOrEmpty(currentExtension.Infos.Title) && currentExtension.Infos.Title.ToLower().Contains(Criteres.Search.ToLower())) ||
                                    (!string.IsNullOrEmpty(currentExtension.Infos.Summary) && currentExtension.Infos.Summary.ToLower().Contains(Criteres.Search.ToLower())) ||
                                    (!string.IsNullOrEmpty(currentExtension.Infos.Description) && currentExtension.Infos.Description.ToLower().Contains(Criteres.Search.ToLower())) ||
                                    (!string.IsNullOrEmpty(currentExtension.Infos.Author) && currentExtension.Infos.Author.ToLower().Contains(Criteres.Search.ToLower())) ||
                                    (!string.IsNullOrEmpty(currentExtension.Infos.Tooltip) && currentExtension.Infos.Tooltip.ToLower().Contains(Criteres.Search.ToLower())) ||
                                    (currentExtension.Infos.Categories != null && currentExtension.Infos.Categories.Values.Contains(Criteres.Search, StringComparer.OrdinalIgnoreCase))
                            //)
                            )
                        )
                        // Filtre sur les catégories
                        &&
                        (
                            !Criteres.FilterCategoryEnabled ||
                            (
                                    //CurrentFilterCategory.Trim().Length > 0 && ( //CNA - Inutile car déjà testé dans FilterCategoryEnabled
                                    (currentExtension.Infos.Categories != null && currentExtension.Infos.Categories.Keys.Contains(Criteres.Category, StringComparer.OrdinalIgnoreCase)) ||
                                    (Criteres.Category == "-1" && (currentExtension.Infos.Categories == null || currentExtension.Infos.Categories.Keys.Contains("-1")))
                            //)
                            )
                        )
                    )
                    {
                        _includedExtensionList.Add(currentExtension);
                    }
                }

                TotalExtensionsCount = _includedExtensionList.Count;
                TotalExtensionsPages = 1;       // Pas de gestion de pagination en hors ligne
            }

            #endregion

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            if (!base.Build())
                return false;

            switch (TypRefresh)
            {
                case StoreListTypeRefresh.FULL:
                    Panel pnlGlobal = new Panel();
                    pnlGlobal.ID = "extension-ui";
                    _pgContainer.Controls.Add(pnlGlobal);

                    Panel pnlContainer = new Panel();
                    pnlContainer.CssClass = "fukol";
                    pnlContainer.Attributes.Add("onscroll", "top.nsAdmin.StoreListScroll(this);");
                    pnlGlobal.Controls.Add(pnlContainer);

                    // Pas dans la V1
                    //pnlContainer.Controls.Add(GetStoreSlider());

                    pnlContainer.Controls.Add(GetStoreSearch());

                    pnlContainer.Controls.Add(GetStoreListPanel());
                    
                    AddCallBackScript("nsAdmin.LoadStoreList();");
                    DicContent.Add("MainPanel", new Content() { Ctrl = _pgContainer });
                    break;

                case StoreListTypeRefresh.LIST:
                    DicContent.Add("ListCount", new Content() { Ctrl = GetCount() });
                    DicContent.Add("ListPanel", new Content() { Ctrl = GetStoreListPanel() });
                    break;

                case StoreListTypeRefresh.ADD_ITEM:
                    // Pour activer le mode multipart on y ajoute un panel fictif
                    DicContent.Add("EmptyPanel", new Content() { Ctrl = new Control() });
                    DicContent.Add("ListPanel", new Content() { Mode = (int)ContentMode.ADD_CHILDS, Ctrl = GetStoreListPanel() });
                    break;
            }

            return true;
        }

        class SlideInfos
        {
            public string Img { get; set; }
            public string ImgSmall { get; set; }

            /// <summary>
            /// Liste des offres pour lesquelles l'extension est dispo
            /// </summary>
            public IEnumerable<ExtensionOffer> Offers { get; set; }

            public string Title { get; set; }
            public string Description { get; set; }
        }

        /// <summary>
        /// Correspond à la partie "nb d'extensions"
        /// </summary>
        /// <returns></returns>
        Panel GetStoreSlider()
        {
            // UserStorie : Eudostore > Liste > Diaporama #601

            HtmlGenericControl element;

            Panel pnlContentSlider = new Panel();
            pnlContentSlider.CssClass = "content-slider";

            Panel pnlSlider = new Panel();
            pnlContentSlider.Controls.Add(pnlSlider);
            pnlSlider.ID = "slider1";
            pnlSlider.CssClass = "csslider infinity";

            // TODO - On va chercher les info ou ?
            IEnumerable<SlideInfos> slides = new List<SlideInfos> {
                new SlideInfos() {
                    Img = "img/1.jpg",
                    ImgSmall = "img/petit1.jpg",
                    Offers = new List<ExtensionOffer>() {
                        new ExtensionOffer() { DbId = 0, Offer = eLibConst.ClientOffer.ACCES },
                        new ExtensionOffer() { DbId = 1, Offer = eLibConst.ClientOffer.PREMIER },
                        new ExtensionOffer() { DbId = 2, Offer = eLibConst.ClientOffer.STANDARD }
                    },
                    Title = "LastPass: Free Password Manager",
                    Description = "Positionnez vos fiches sur un plan et analysez, exploitez et modifiez les informations !"
                },
                new SlideInfos() {
                    Img = "img/2.jpg",
                    ImgSmall = "img/petit2.jpg",
                    Offers = new List<ExtensionOffer>() {
                        new ExtensionOffer() { DbId = 1, Offer = eLibConst.ClientOffer.PREMIER },
                        new ExtensionOffer() { DbId = 2, Offer = eLibConst.ClientOffer.STANDARD }
                    },
                    Title = "LastPass: Free Password Manager",
                    Description = "Positionnez vos fiches sur un plan et analysez, exploitez et modifiez les informations !"
                }
            };

            HtmlGenericControl ul = new HtmlGenericControl("ul");

            Panel pnlArrow = new Panel();
            pnlArrow.CssClass = "arrows";

            Panel pnlNavGlobal = new Panel();
            pnlNavGlobal.CssClass = "navigation";
            Panel pnlNav = new Panel();
            pnlNavGlobal.Controls.Add(pnlNav);

            int cnt = 0;
            foreach (SlideInfos slide in slides)
            {
                HtmlInputRadioButton r = new HtmlInputRadioButton();
                pnlSlider.Controls.Add(r);
                r.Name = "slides";
                if (cnt == 0)
                    r.Checked = true;
                r.ID = string.Format("slides_{0}", ++cnt);

                HtmlGenericControl li = new HtmlGenericControl("li");
                ul.Controls.Add(li);

                HtmlImage img = new HtmlImage();
                li.Controls.Add(img);
                img.Src = slide.Img;

                Panel pnlGlobal = new Panel();
                li.Controls.Add(pnlGlobal);
                pnlGlobal.CssClass = "hoverExt";

                Panel pnlOffer = new Panel();
                pnlGlobal.Controls.Add(pnlOffer);
                pnlOffer.CssClass = "offre-hover";

                foreach (var elem in GetOfferImg(slide.Offers))
                    pnlOffer.Controls.Add(elem);

                HtmlImage imgSmall = new HtmlImage();
                pnlGlobal.Controls.Add(imgSmall);
                imgSmall.Src = slide.ImgSmall;

                Panel pnlContent = new Panel();
                pnlGlobal.Controls.Add(pnlContent);
                pnlContent.CssClass = "content-text-hover";

                Panel pnlContentTitle = new Panel();
                pnlContent.Controls.Add(pnlContentTitle);
                pnlContentTitle.CssClass = "title";

                Label label = new Label();
                pnlContentTitle.Controls.Add(label);
                label.Text = slide.Title;

                Panel pnlContentDesc = new Panel();
                pnlContent.Controls.Add(pnlContentDesc);
                pnlContentDesc.CssClass = "descri";

                element = new HtmlGenericControl("p");
                pnlContentDesc.Controls.Add(element);
                element.InnerText = slide.Description;

                Panel pnlBtn = new Panel();
                pnlGlobal.Controls.Add(pnlBtn);
                pnlBtn.CssClass = "button-hover";

                element = new HtmlGenericControl("button");
                pnlBtn.Controls.Add(element);
                element.Attributes.Add("class", "btn-success");
                element.InnerText = "Découvrir"; // TODO - RES

                // Arrows
                element = new HtmlGenericControl("label");
                pnlArrow.Controls.Add(element);
                element.Attributes.Add("class", "slideArrow");
                element.Attributes.Add("for", string.Format("slides_{0}", cnt));

                // Navigation
                element = new HtmlGenericControl("label");
                pnlNav.Controls.Add(element);
                element.Attributes.Add("for", string.Format("slides_{0}", cnt));
            }


            // Debut et fin des arrows
            element = new HtmlGenericControl("label");
            pnlArrow.Controls.Add(element);
            element.Attributes.Add("class", "goto-first slideArrow");
            element.Attributes.Add("for", "slides_1");
            element = new HtmlGenericControl("label");
            pnlArrow.Controls.Add(element);
            element.Attributes.Add("class", "goto-last slideArrow");
            element.Attributes.Add("for", string.Format("slides_{0}", cnt));

            pnlSlider.Controls.Add(ul);
            pnlSlider.Controls.Add(pnlArrow);
            pnlSlider.Controls.Add(pnlNavGlobal);

            return pnlContentSlider;
        }

        Control GetCount()
        {
            Label label = new Label();
            label.ID = "nbfounditems";
            label.Text = TotalExtensionsCount.ToString();
            return label;
        }

        /// <summary>
        /// Correspond à la barre des filtres
        /// </summary>
        /// <returns></returns>
        Panel GetStoreSearch()
        {
            Label label;

            Panel pnlSearch = new Panel();
            pnlSearch.CssClass = "resultSearch";

            Panel pnlContentSearch = new Panel();
            pnlSearch.Controls.Add(pnlContentSearch);
            pnlContentSearch.CssClass = "contentResultSearch";

            #region infos

            Panel pnlInfoVers = new Panel();
            pnlContentSearch.Controls.Add(pnlInfoVers);
            pnlInfoVers.CssClass = "infoVersion";
            //pnlInfoVers.Attributes.Add("title", "Vous avez 2 extensions actives sur 22 disponibles"); vu avec ALEB annulé

            Panel pnlInfoActive = new Panel();
            pnlInfoVers.Controls.Add(pnlInfoActive);
            pnlInfoActive.CssClass = "infoActive";

            Panel pnlSpanActive = new Panel();
            pnlInfoActive.Controls.Add(pnlSpanActive);
            pnlSpanActive.CssClass = "secondSpanActive";

            pnlSpanActive.Controls.Add(GetCount());

            label = new Label();
            pnlSpanActive.Controls.Add(label);
            label.Text = string.Concat(" ", eResApp.GetRes(Pref, 2996));

            #endregion

            #region search

            Panel pnlSearchGroup = new Panel();
            pnlContentSearch.Controls.Add(pnlSearchGroup);
            pnlSearchGroup.CssClass = "group";

            HtmlInputText searchTxt = new HtmlInputText();
            pnlSearchGroup.Controls.Add(searchTxt);
            searchTxt.ID = "storeSearch";
            searchTxt.Attributes.Add("placeholder", eResApp.GetRes(_ePref, 5067));
            searchTxt.Attributes.Add("required", "");
            searchTxt.Attributes.Add("onkeyup", "nsAdmin.StoreSearch(this.value, event);");
            searchTxt.Attributes.Add("maxlength", "100");
            searchTxt.Value = Criteres.Search;

            label = new Label();
            pnlSearchGroup.Controls.Add(label);
            label.CssClass = "bar";

            #endregion

            return pnlSearch;
        }

        /// <summary>
        /// Retourne le rendu de la liste des extensions
        /// </summary>
        /// <returns></returns>
        Panel GetStoreListPanel()
        {
            Panel pnlBlocks = new Panel();
            pnlBlocks.ID = "product-grid";
            pnlBlocks.CssClass = "fukol-grid";

            foreach (eAdminExtension e in _includedExtensionList)
            {
                Control ctrl = GetExtensionPanel(e);
                if (ctrl != null)
                    pnlBlocks.Controls.Add(ctrl);
            }

            return pnlBlocks;
        }

        Control GetExtensionPanel(eAdminExtension extension)
        {
            HtmlImage img;
            HtmlGenericControl global = new HtmlGenericControl("figure");

            string jsModuleData = string.Concat("{ extensionFileId: ", extension.Infos.ExtensionFileId, ", extensionCode: '", extension.Infos.ExtensionNativeId, "', extensionLabel: '", extension.Infos.Title.Replace("'", "\\'"), "' }");
            global.Attributes.Add("onclick", string.Format("javascript: nsAdmin.loadAdminModule('{0}', null, {1});", extension.Module.ToString(), jsModuleData));

            #region intro

            string statusCssClass, statusTxt, statusToolTip;
            if (AddStatusIcon(extension, out statusCssClass, out statusToolTip, out statusTxt))
            {
                Label title = new Label();
                global.Controls.Add(title);
                title.CssClass = statusCssClass;
                title.ToolTip = statusTxt;
            }

            // Icone
            img = new HtmlImage();
            global.Controls.Add(img);
            img.Alt = "";   // TODO - On met quoi ?
            img.Attributes.Add("class", "transform BigImg");
            img.Src = (extension.Infos.Icon.StartsWith("http://") || extension.Infos.Icon.StartsWith("https://")) ? extension.Infos.Icon : "";

            HtmlGenericControl separateur = new HtmlGenericControl("hr");
            global.Controls.Add(separateur);
            separateur.Attributes.Add("class", "sepa");

            #endregion

            #region caption

            HtmlGenericControl figcap = new HtmlGenericControl("figcaption");
            global.Controls.Add(figcap);
            figcap.Attributes.Add("class", "container");

            HtmlGenericControl figcapParagraphe = new HtmlGenericControl("p");
            figcap.Controls.Add(figcapParagraphe);
            figcapParagraphe.Attributes.Add("class", "descrFig");
            figcapParagraphe.InnerHtml = extension.Infos.TexteLogo;

            Panel figcapPnlFadeOut = new Panel();
            figcap.Controls.Add(figcapPnlFadeOut);
            figcapPnlFadeOut.CssClass = "fadeOut";

            #endregion

            Panel pnlContent = new Panel();
            global.Controls.Add(pnlContent);
            pnlContent.CssClass = "content-txt";

            Panel pnlContTitle = new Panel();
            pnlContent.Controls.Add(pnlContTitle);
            pnlContTitle.CssClass = "title";

            HyperLink link = new HyperLink();
            pnlContTitle.Controls.Add(link);
            link.NavigateUrl = "";
            link.Controls.Add(new LiteralControl(extension.Title));

            Label label = new Label();
            pnlContTitle.Controls.Add(label);
            label.CssClass = "fadeOutTitle";

            HtmlGenericControl description = new HtmlGenericControl("p");
            pnlContent.Controls.Add(description);
            description.Attributes.Add("class", "description");
            description.InnerHtml = extension.Infos.Summary;

            Label descFadeOut = new Label();
            description.Controls.Add(descFadeOut);
            descFadeOut.CssClass = "fadeOutDescri";

            Panel contentPnlFadeOut = new Panel();
            pnlContent.Controls.Add(contentPnlFadeOut);
            contentPnlFadeOut.CssClass = "fadeOut";

            Panel pnlOffers = new Panel();
            pnlContent.Controls.Add(pnlOffers);
            pnlOffers.CssClass = "view";

            foreach (var elem in GetOfferImg(extension))
                pnlOffers.Controls.Add(elem);


            return global;
        }
    }
}