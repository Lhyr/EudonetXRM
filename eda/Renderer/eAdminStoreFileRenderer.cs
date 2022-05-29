using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI;
using System.Text;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    ///  Renderer d'une extension en mode fiche disponible en administration
    /// </summary>
    public class eAdminStoreFileRenderer : eAdminStoreRenderer
    {

        /// <summary> 
        /// Indique si on ajoute l'onglet paramètres 
        /// </summary>
        private bool _existsParamPanel { get { return Extension.ShowParametersTab; } }

        private HtmlGenericControl _divModal;

        /// <summary>
        /// Extension à afficher
        /// </summary>
        protected eAdminExtension _extension = null;
        /// <summary>
        /// Métadonnées de l'extension
        /// </summary>
        protected eExtension _eRegisteredExt = null;
        /// <summary>
        /// Mail de contact pour l'entête, si  l'extension n'est pas compatible ou
        /// qu'elle ne peut pas être installée.
        /// </summary>
        private const string _emailContact = "marketing@eudonet.com";

        /// <summary>
        /// Liste de docs d'une extension récupérés depuis l'API
        /// </summary>
        private List<ProductDescriptionDoc> listDocsProductDesc = new List<ProductDescriptionDoc>();

        /// <summary>
        /// Liste des annexes d'une doc de l'onglet "Produit Description"
        /// </summary>
        private List<Tuple<string, string, int>> listPjs = new List<Tuple<string, string, int>>(); //DbValue, Value, FileId

        /// <summary>
        /// Liste des annexes d'une doc de l'onglet "Produit Description"
        /// </summary>
        private static int totalPJsCount = 0;

        /// <summary>
        /// Vérifie si tous les paramètres obligatoires de l'extension Addin Outlook sont valides 
        /// </summary>
        //protected bool invalidMandatoryParams;


        #region Constructeurs
        /// <summary>
        /// Constructeur eAdminExtension
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="extension">Extension à afficher</param>
        public eAdminStoreFileRenderer(ePref pref, eAdminExtension extension) : base(pref)
        {
            Pref = pref;
            _extension = extension;
        }
        #endregion

        #region Getters / Setters
        /// <summary>
        /// Accesseurs extension
        /// </summary>
        public eAdminExtension Extension { get { return _extension; } }
        /// <summary>
        /// Accesseurs extension registrée
        /// </summary>
        public eExtension RegisteredExt { get; set; }
        /// <summary>
        /// Accesseurs conteneur des paramètres de l'extension
        /// </summary>
        public Panel ExtensionParametersContainer { get; set; }

        #endregion

        /// <summary>
        /// Retourne le renderer d'une extension en mode fiche
        /// </summary>
        /// <param name="pref">Préférences utilisateur</param>
        /// <param name="extension">Extension à afficher</param>
        internal static eAdminStoreFileRenderer GetAdminStoreFileRenderer(ePref pref, eAdminExtension extension)
        {
            return new eAdminStoreFileRenderer(pref, extension);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (base.Init())
            {
                if (!string.IsNullOrWhiteSpace(this.Extension?.Infos?.ExtensionNativeId))
                {
                    getPj(Pref, this.Extension);
                    var list = eExtension.GetExtensionsByCode(Pref, this.Extension.Infos.ExtensionNativeId);
                    if (list.Count == 1)
                    {
                        _eRegisteredExt = list[0];

                        Extension.SetDefaultParam(_eRegisteredExt);
                    }
                    else
                    {
                        _eRegisteredExt = eExtension.GetNewExtension(this.Extension.Infos.ExtensionNativeId, status: EXTENSION_STATUS.STATUS_NON_INSTALLED);
                        Extension.SetDefaultParam(_eRegisteredExt);
                    }


                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (base.Build())
            {
                //detail header

                Panel pnOverflowCtn = new Panel { CssClass = "overflow-container" };
                pnOverflowCtn.Controls.Add(GetEnteteStoreApplication());

                //container area
                HtmlGenericControl divContainerArea = new HtmlGenericControl("div");
                divContainerArea.Attributes.Add("class", "store-container store-body-container ");
                pnOverflowCtn.Controls.Add(divContainerArea);

                //container area > the modal
                _divModal = new HtmlGenericControl("div");
                _divModal.ID = "myModal";
                _divModal.Attributes.Add("class", "store-modal ");
                //if (!Extension.Infos.IsEnabled)
                divContainerArea.Controls.Add(_divModal);

                //container area > tab
                HtmlGenericControl divTab = new HtmlGenericControl("div");
                divTab.Attributes.Add("class", string.Concat("store-content-container ", !Extension.Infos.IsEnabled ? " " : "installed"));
                divContainerArea.Controls.Add(divTab);

                HtmlGenericControl divCol = new HtmlGenericControl("div");
                divCol.Attributes.Add("class", "store-col-container ");
                divTab.Controls.Add(divCol);

                //store-tab-container
                divCol.Controls.Add(GetStoreFileTabs());

                // Ajout des conteneurs des signets
                List<Panel> divs = GetStoreFileDivs();
                foreach (Panel div in divs)
                    divCol.Controls.Add(div);

                PgContainer.Controls.Add(pnOverflowCtn);

                AddCallBackScript("fnStoreFile();", true);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Retourne la liste des conteneurs des signets de l'extension
        /// </summary>
        /// <returns>HtmlGenericControl</returns>
        protected HtmlGenericControl GetStoreFileTabs()
        {
            HtmlGenericControl storeTabs = new HtmlGenericControl("div");
            storeTabs.Attributes.Add("class", "store-tab-container ");

            #region Création des cellules correspondant à chaque onglet

            if (Extension.Infos.IsEnabled)
            {
                //Description
                HtmlGenericControl buttonDesc = new HtmlGenericControl("button");
                //SHA : correction bug 71 446 : 
                //Cas spécial pour l'extension ADDIN OUTLOOK : ouvrir directement l'onglet de paramètres lors de l'activation de l'extension 
                //afin d'informer de l'obligation de renseigner les champs obligatoires Champs Table Email , Nom, Prénom, Courriel "Adresses",
                //Courriel "Contacts" (Recherche)
                buttonDesc.Attributes.Add("class", String.Concat("store-tablink ",
                    (Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN) ? "" : "active"));
                buttonDesc.Attributes.Add("onclick", "nsAdmin.openPage('description', this, 'white');");
                buttonDesc.ID = "defaultOpen";
                buttonDesc.InnerHtml = eResApp.GetRes(Pref, 5131); //Description
                storeTabs.Controls.Add(buttonDesc);

                //Installation et utilisation
                HtmlGenericControl buttonInstall = new HtmlGenericControl("button");
                buttonInstall.Attributes.Add("class", "store-tablink ");
                buttonInstall.Attributes.Add("onclick", "nsAdmin.openPage('installation', this, 'white');");
                buttonInstall.InnerHtml = eResApp.GetRes(Pref, 2255); //Installation et utilisation
                storeTabs.Controls.Add(buttonInstall);

                //Paramètres
                if (_existsParamPanel)
                {
                    HtmlGenericControl buttonParams = new HtmlGenericControl("button");
                    buttonParams.Attributes.Add("class", String.Concat("store-tablink ",
                        //SHA : correction bug 71 446
                        (Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN) ? "active" : ""));
                    buttonParams.Attributes.Add("onclick", "nsAdmin.openPage('settings', this, 'white');");
                    buttonParams.InnerHtml = eResApp.GetRes(Pref, 7906); // Paramètres
                    storeTabs.Controls.Add(buttonParams);
                }
            }

            #endregion
            return storeTabs;
        }

        /// <summary>
        /// Retourne un bloc d'information
        /// </summary>
        /// <param name="id">id js</param>
        /// <param name="sectionLabel"></param>
        /// <param name="sectionTabLabel"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected Panel GetInfosSection(string id, string sectionLabel, string sectionTabLabel, List<Tuple<string, string, eAdminField>> infos)
        {
            List<eBasicInputField> infos2 = infos.Select(w => new eBasicInputField() { Value = w.Item2, Label = w.Item1, Fld = w.Item3, EndId = "" }).ToList();
            return GetInfosSection(id, sectionLabel, sectionTabLabel, infos2);
        }


        /// <summary>
        /// Retourne un bloc d'information
        /// </summary>
        /// <param name="id">id js</param>
        /// <param name="sectionLabel"></param>
        /// <param name="sectionTabLabel"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        protected Panel GetInfosSection(string id, string sectionLabel, string sectionTabLabel, List<eBasicInputField> infos)
        {
            Panel sectionPanel = GetModuleSection(String.Concat("section_", id), sectionLabel);
            Panel sectionPanelContainer = (Panel)sectionPanel.Controls[sectionPanel.Controls.Count - 1];


            if (infos != null)
            {
                int n = 1;
                foreach (var fieldInfos in infos)
                {
                    Panel field = new Panel();
                    field.CssClass = "field fieldinfosection";
                    field.ID = String.Concat(id, "_fieldinfosection_", n);
                    if (!string.IsNullOrEmpty(fieldInfos.EndId))
                        field.Attributes.Add("ednid", String.Concat(id, "_", fieldInfos.EndId));

                    HtmlGenericControl fieldLabel = new HtmlGenericControl("label");
                    fieldLabel.InnerText = fieldInfos.Label;
                    field.Controls.Add(fieldLabel);

                    if (fieldInfos.Value.Length > 0)
                    {
                        HtmlInputText fieldValue = new HtmlInputText();
                        fieldValue.Value = fieldInfos.Value;
                        fieldValue.Attributes.Add("title", eResApp.GetRes(_ePref, 2314)); //Double-cliquez pour copier la valeur dans le presse papier
                        fieldValue.Attributes.Add("ondblclick", "nsAdminField.CopyValueToClipBoard(event)");
                        fieldValue.Attributes.Add("readonly", "1");
                        fieldValue.Size = 60;

                        field.Controls.Add(fieldValue);
                    }
                    else
                    {
                        fieldLabel.Style.Add("width", "100%");
                        fieldLabel.Style.Add("text-align", "center");
                    }

                    if (fieldInfos.Fld != null)
                        fieldInfos.Fld.Generate(field);

                    sectionPanelContainer.Controls.Add(field);
                    n++;
                }
            }
            return sectionPanel;
        }



        /// <summary>
        /// Retourne la liste des conteneurs des signets de l'extension
        /// </summary>
        /// <returns> List(Panel) </returns>
        protected List<Panel> GetStoreFileDivs()
        {
            List<Panel> fileDivs = new List<Panel>();

            #region Création des conteneurs correspondant à chaque signet

            //Description
            Panel pnExtensionTabDesc = new Panel();
            pnExtensionTabDesc.ID = "description";
            pnExtensionTabDesc.Attributes.Add("class", String.Concat("store-tabcontent description ",
                //SHA : correction bug 71 446
                (Extension.Infos.IsEnabled && Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN) ? "" : "store-visible"));

            if (Extension.Infos.Screenshots.Count > 0)
                pnExtensionTabDesc.Controls.Add(getDiaporamaPanel());

            pnExtensionTabDesc.Controls.Add(getDescriptionPanel());
            fileDivs.Add(pnExtensionTabDesc);

            if (Extension.Infos.IsEnabled)
            {
                //Installation et utilisation
                Panel pnExtensionTabInstall = new Panel();
                pnExtensionTabInstall.ID = "installation";
                pnExtensionTabInstall.Attributes.Add("class", "store-tabcontent ");
                pnExtensionTabInstall.Controls.Add(getInstallUsePanel(Extension.Infos.InstallationInfos));
                fileDivs.Add(pnExtensionTabInstall);

                //Paramètres
                if (_existsParamPanel)
                {
                    CreateSettingsPanel();
                    fileDivs.Add(ExtensionParametersContainer);
                }
            }

            #endregion
            return fileDivs;
        }

        /// <summary>
        /// Ajoute le panel de rendu des paramètres
        /// </summary>
        /// <returns></returns>
        protected virtual void CreateSettingsPanel()
        {
            ExtensionParametersContainer = new Panel();
            ExtensionParametersContainer.ID = "settings";
            ExtensionParametersContainer.Attributes.Add("class", String.Concat("store-tabcontent ",
                //SHA : correction bug 71 446
                (Extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_OUTLOOKADDIN) ? "store-visible" : ""));
            PgContainer.Controls.Add(ExtensionParametersContainer);
        }

        #region PANEL TYPE HTML

        /// <summary>
        /// Renvoie le contenu de l'onglet "Installation et utilisation" de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Panel getInstallUsePanel(string sContent)
        {
            Panel pnFlexContent = new Panel { CssClass = "store-flex-content store-presentation-row " };

            //Panel Installation et utilisation
            Panel pnTextContent = new Panel { CssClass = "store-col-70 store-text-content" };
            pnFlexContent.Controls.Add(pnTextContent);

            #region Titre
            //HtmlGenericControl h2InstallTitle = new HtmlGenericControl("h2");
            //h2InstallTitle.Attributes.Add("class", "store-title-level-2 install-title ");
            //h2InstallTitle.InnerHtml = eResApp.GetRes(Pref, 2255); //Installation et utilisation
            //pnTextContent.Controls.Add(h2InstallTitle);
            #endregion

            #region Contenu
            HtmlGenericControl pnInstallUse = new HtmlGenericControl("div");
            pnInstallUse.Attributes.Add("class", "store-text-area");
            pnInstallUse.InnerHtml = sContent;
            pnTextContent.Controls.Add(pnInstallUse);
            #endregion

            #region Manuels et Tutoriels vidéos 

            if (totalPJsCount > 0)
            {
                //Panel Manuels et Tutoriels vidéos 
                Panel pnAddInfos = new Panel { CssClass = "store-col-30 store-add-infos " };

                //Manuels
                HtmlGenericControl h3TitleManuals = new HtmlGenericControl("h3");
                h3TitleManuals.Attributes.Add("class", "store-title-level-3 ");
                h3TitleManuals.InnerHtml = eResApp.GetRes(Pref, 2294); //Manuels
                pnAddInfos.Controls.Add(h3TitleManuals);

                //download-list Manuels
                HtmlGenericControl ulDLManuals = new HtmlGenericControl("ul");
                ulDLManuals.Attributes.Add("class", "download-list ");
                pnAddInfos.Controls.Add(ulDLManuals);

                //Manuel d'installation
                HtmlGenericControl liInstallManual = new HtmlGenericControl("li");
                ulDLManuals.Controls.Add(liInstallManual);

                HtmlGenericControl iIconFilePdf = new HtmlGenericControl("i");
                iIconFilePdf.Attributes.Add("class", "icon-file-pdf-o ");
                liInstallManual.Controls.Add(iIconFilePdf);

                HtmlGenericControl aLinkInstallManual = new HtmlGenericControl("a");
                aLinkInstallManual.Attributes.Add("class", "store-install-link store-link ");
                aLinkInstallManual.Attributes.Add("target", "_blank");
                bool hasPjLinkInstallManual;
                aLinkInstallManual.Attributes.Add("href", getPjManualTutorial(3238, out hasPjLinkInstallManual).Item2); //Manuel d'installation                
                aLinkInstallManual.InnerHtml = eResApp.GetRes(Pref, 2297); //Manuel d'installation
                liInstallManual.Controls.Add(aLinkInstallManual);
                if (!hasPjLinkInstallManual)
                    liInstallManual.Visible = false;

                //Manuel d'utilisation
                HtmlGenericControl liUseManual = new HtmlGenericControl("li");
                ulDLManuals.Controls.Add(liUseManual);

                HtmlGenericControl iIconFilePdf2 = new HtmlGenericControl("i");
                iIconFilePdf2.Attributes.Add("class", "icon-file-pdf-o ");
                liUseManual.Controls.Add(iIconFilePdf2);

                HtmlGenericControl aInstallLinkUse = new HtmlGenericControl("a");
                aInstallLinkUse.Attributes.Add("class", "store-install-link store-link ");
                aInstallLinkUse.Attributes.Add("target", "_blank");
                bool hasPjInstallLinkUse;
                aInstallLinkUse.Attributes.Add("href", getPjManualTutorial(3237, out hasPjInstallLinkUse).Item2); //Manuel d'utilisation                
                aInstallLinkUse.InnerHtml = eResApp.GetRes(Pref, 2298); //Manuel d'utilisation
                liUseManual.Controls.Add(aInstallLinkUse);
                if (!hasPjInstallLinkUse)
                    liUseManual.Visible = false;

                if (!hasPjLinkInstallManual && !hasPjInstallLinkUse)
                    h3TitleManuals.Visible = false;

                //Tutoriels vidéos
                HtmlGenericControl h3TitleTutos = new HtmlGenericControl("h3");
                h3TitleTutos.Attributes.Add("class", "store-title-level-3 ");
                h3TitleTutos.InnerHtml = eResApp.GetRes(Pref, 2295); //Tutoriels vidéos
                pnAddInfos.Controls.Add(h3TitleTutos);

                //download-list Tutoriels vidéos
                HtmlGenericControl ulDLTutos = new HtmlGenericControl("ul");
                ulDLTutos.Attributes.Add("class", "download-list ");
                pnAddInfos.Controls.Add(ulDLTutos);

                //Vidéo d'installation
                HtmlGenericControl liInstallTuto = new HtmlGenericControl("li");
                ulDLTutos.Controls.Add(liInstallTuto);

                HtmlGenericControl iIconFileVideo = new HtmlGenericControl("i");
                iIconFileVideo.Attributes.Add("class", "icon-file-video-o ");
                liInstallTuto.Controls.Add(iIconFileVideo);

                HtmlGenericControl aLinkInstallTuto = new HtmlGenericControl("a");
                aLinkInstallTuto.Attributes.Add("class", "store-video-link store-link ");
                aLinkInstallTuto.Attributes.Add("target", "_blank");
                bool hasPjLinkInstallTuto;
                aLinkInstallTuto.Attributes.Add("href", getPjManualTutorial(3240, out hasPjLinkInstallTuto).Item2); //Tutoriel d'installation                
                aLinkInstallTuto.InnerHtml = eResApp.GetRes(Pref, 2299); //Tutoriel d'installation
                liInstallTuto.Controls.Add(aLinkInstallTuto);
                if (!hasPjLinkInstallTuto)
                    liInstallTuto.Visible = false;

                //Vidéo d'utilisation
                HtmlGenericControl liUseTuto = new HtmlGenericControl("li");
                ulDLTutos.Controls.Add(liUseTuto);

                HtmlGenericControl iIconFileVideo2 = new HtmlGenericControl("i");
                iIconFileVideo2.Attributes.Add("class", "icon-file-video-o ");
                liUseTuto.Controls.Add(iIconFileVideo2);

                HtmlGenericControl aVideoLinkUse = new HtmlGenericControl("a");
                aVideoLinkUse.Attributes.Add("class", "store-video-link store-link ");
                aVideoLinkUse.Attributes.Add("target", "_blank");
                bool hasPjVideoLinkUse;
                aVideoLinkUse.Attributes.Add("href", getPjManualTutorial(3239, out hasPjVideoLinkUse).Item2); //Tutoriel d'utilisation                
                aVideoLinkUse.InnerHtml = eResApp.GetRes(Pref, 2300); //Vidéo d'utilisation
                liUseTuto.Controls.Add(aVideoLinkUse);
                if (!hasPjVideoLinkUse)
                    liUseTuto.Visible = false;

                if (!hasPjLinkInstallTuto && !hasPjVideoLinkUse)
                    h3TitleTutos.Visible = false;

                pnFlexContent.Controls.Add(pnAddInfos);
            }
            else
                //Centrer le texte d'installation et utilisation s'il n'y a pas de manuels/tutoriels
                pnFlexContent.CssClass += "emptyManusTutos";

            #endregion

            return pnFlexContent;
        }

        //SHA
        /// <summary>
        /// Retourne le lien de l'annexe demandé
        /// </summary>
        /// <param name="docType">Type du document (Manuel d'installation / Manuel d'utilisation / Tutoriel d'installation / Tutoriel d'utilisation)</param>
        /// <param name="hasPj">Si l'annexe existe</param>
        /// <returns></returns>
        private Tuple<string, string, int> getPjManualTutorial(int docType, out bool hasPj)
        {
            //Liste des ids des docs
            List<int> fileIds = new List<int>();
            foreach (ProductDescriptionDoc docProductDesc in listDocsProductDesc)
            {
                if (docProductDesc.ManualTuto == docType)
                    fileIds.Add(docProductDesc.DocFileId);
            }

            if (fileIds.Any() && listPjs.Any())
            {
                hasPj = true;
                return listPjs.First(i => i.Item3 == fileIds.FirstOrDefault());
            }

            hasPj = false;
            return new Tuple<string, string, int>("", "", 0);
        }

        private void getPj(ePref pref, eAdminExtension oExtension)
        {
            eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(pref);
            ExtensionGlobalInfo oExtensionGlobalInfo = new ExtensionGlobalInfo();
            int totalDocsCount;
            //totalPJsCount = 0;
            //liste des docs du signet Doc.
            oExtensionGlobalInfo.DescriptionInfos = storeAccess.GetProductDescription(oExtension.Infos.ExtensionFileId);
            listDocsProductDesc = storeAccess.GetProductDescDocsList(oExtensionGlobalInfo.DescriptionInfos.FileId, out totalDocsCount);

            List<int> fileIds = new List<int>();
            foreach (ProductDescriptionDoc docProductDesc in listDocsProductDesc)
            {
                fileIds.Add(docProductDesc.DocFileId);
            }
            fileIds = fileIds.Distinct().ToList();

            //liste des annexes
            if (fileIds.Any())
                listPjs = storeAccess.GetPJsDico(fileIds, out totalPJsCount);
        }

        #region diaporama

        private void setModalDiaporama()
        {
            HtmlGenericControl span = new HtmlGenericControl("span") { InnerText = "x" };
            _divModal.Controls.Add(span);
            span.Attributes.Add("class", "store-close");

            HtmlImage image = new HtmlImage() { ID = "img01" };
            _divModal.Controls.Add(image);
            image.Style.Add(HtmlTextWriterStyle.Display, "none");
            image.Attributes.Add("class", "store-modal-content");

            HtmlIframe iframe = new HtmlIframe();
            _divModal.Controls.Add(iframe);
            iframe.Attributes.Add("class", "store-modal-iframe");
            iframe.Attributes.Add("allow", "accelerometer; autoplay; encrypted-media; gyroscope; picture-in-picture");
            iframe.Attributes.Add("allowfullscreen", "");
            image.Style.Add(HtmlTextWriterStyle.Display, "none");

            HtmlGenericControl p = new HtmlGenericControl("p");
            _divModal.Controls.Add(p);
            p.Attributes.Add("class", "store-slide-left");

            span = new HtmlGenericControl("span");
            p.Controls.Add(span);
            span.Attributes.Add("class", "store-chevron left");

            p = new HtmlGenericControl("p");
            _divModal.Controls.Add(p);
            p.Attributes.Add("class", "store-slide-right");

            span = new HtmlGenericControl("span");
            p.Controls.Add(span);
            span.Attributes.Add("class", "store-chevron right");

            Panel pnCaption = new Panel();
            _divModal.Controls.Add(pnCaption);
            pnCaption.ID = "store-caption";


        }


        private Panel getDiaporamaPanel()
        {

            setModalDiaporama();

            Panel pnContentSlider = new Panel();
            pnContentSlider.CssClass = "store-content-slider";

            Panel pnSlider = new Panel();
            pnContentSlider.Controls.Add(pnSlider);

            pnSlider.CssClass = "store-csslider infinity";
            pnSlider.ID = "slider1";

            foreach (eAPIProductScreenshot sc in Extension.Infos.Screenshots)
            {
                pnSlider.Controls.Add(getDiapoRadioButton(sc.DisplayOrder));
            }

            HtmlGenericControl ulScreenShotsSlider = new HtmlGenericControl("ul");
            pnSlider.Controls.Add(ulScreenShotsSlider);

            foreach (eAPIProductScreenshot sc in Extension.Infos.Screenshots)
            {
                ulScreenShotsSlider.Controls.Add(getScreenShotHtmlLI(sc));
            }

            Panel pnArrow = new Panel();
            pnSlider.Controls.Add(pnArrow);
            pnArrow.CssClass = "store-arrows";

            HtmlGenericControl lblArrow;
            //foreach (eAPIProductScreenshot sc in Extension.Infos.Screenshots)
            //{
            //    lblArrow = new HtmlGenericControl("Label");
            //    pnArrow.Controls.Add(lblArrow);

            //    lblArrow.Attributes.Add("class", "store-slideArrow");
            //    lblArrow.Attributes.Add("for", String.Format("slides_{0}", sc.DisplayOrder));
            //}

            IEnumerable<int> ieScreenShots = Extension.Infos.Screenshots.Select(sc => sc.DisplayOrder).DefaultIfEmpty();

            lblArrow = new HtmlGenericControl("Label");
            pnArrow.Controls.Add(lblArrow);
            lblArrow.Attributes.Add("class", "goto-first");
            lblArrow.Attributes.Add("for", String.Format("slides_{0}", ieScreenShots.Min()));

            lblArrow = new HtmlGenericControl("Label");
            pnArrow.Controls.Add(lblArrow);
            lblArrow.Attributes.Add("class", "goto-last");
            lblArrow.Attributes.Add("for", String.Format("slides_{0}", ieScreenShots.Max()));

            Panel pnNavigation = new Panel();
            pnSlider.Controls.Add(pnNavigation);
            pnNavigation.CssClass = "store-navigation";

            Panel pnSlide;
            foreach (eAPIProductScreenshot sc in Extension.Infos.Screenshots)
            {
                pnSlide = new Panel();
                pnNavigation.Controls.Add(pnSlide);

                pnSlide.ID = String.Format("slides_{0}", sc.DisplayOrder);
            }

            return pnContentSlider;

        }
        private HtmlInputRadioButton getDiapoRadioButton(int iDisplayOrder)
        {
            return new HtmlInputRadioButton() { Name = "slides", ID = String.Format("slides_{0}", iDisplayOrder), Checked = iDisplayOrder == 1 };
        }

        private HtmlGenericControl getScreenShotHtmlLI(eAPIProductScreenshot sc)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "store-tooltip");

            HtmlGenericControl storeImgSliderContainer = new HtmlGenericControl("div");
            storeImgSliderContainer.Attributes.Add("class", "store-img-slider-container ");

            HtmlImage img = new HtmlImage();

            //img.Attributes.Add("class", String.Concat(sc.VideoURL.Length > 0 ? "video-content " : "", "img-slider"));
            img.Attributes.Add("class", String.Format("store-img-slider{0}", sc.VideoURL.Length > 0 ? " video-content" : ""));
            img.Src = sc.ImageURL;
            img.Alt = sc.Label;

            if (sc.VideoURL.Length > 0)
            {
                img.Attributes.Add("ednvid", sc.VideoURL);
            }
            storeImgSliderContainer.Controls.Add(img);

            li.Controls.Add(storeImgSliderContainer);

            HtmlGenericControl span = new HtmlGenericControl("span");
            li.Controls.Add(span);
            span.Attributes.Add("class", "store-tooltiptext");
            span.InnerText = eResApp.GetRes(Pref, 2288);

            Panel pnHoverExt = new Panel();
            pnHoverExt.Attributes.Add("class", "store-hoverExt");
            li.Controls.Add(pnHoverExt);
            HtmlGenericControl p = new HtmlGenericControl("p");
            pnHoverExt.Controls.Add(p);
            p.InnerText = sc.Label;


            return li;

        }


        #endregion diaporama

        #region description

        /// <summary>
        /// Renvoie le contenu de l'onglet "Description" de l'extension en mode Fiche
        /// </summary>
        /// <returns></returns>
        private Panel getDescriptionPanel()
        {
            //variables anonymisées réutilisables
            HtmlGenericControl a, span, i;
            StringBuilder sb;

            Panel pn = new Panel();
            pn.CssClass = "store-flex-content store-presentation-row";

            Panel pnTextContent = new Panel();
            //pnTextContent.Attributes.CssStyle.Add("border-bottom", "1px solid #ccc");
            pnTextContent.CssClass = "store-col-70 store-text-content store-text-area";
            pn.Controls.Add(pnTextContent);

            pnTextContent.Controls.Add(new LiteralControl(Extension.Infos.Description));

            Panel pnAddInfos = new Panel();
            pnAddInfos.CssClass = "store-col-30 store-add-infos";
            pn.Controls.Add(pnAddInfos);

            HtmlGenericControl h3Compatibility = new HtmlGenericControl("h3");
            pnAddInfos.Controls.Add(h3Compatibility);
            h3Compatibility.Attributes.Add("class", "store-title-level-3");
            h3Compatibility.InnerText = eResApp.GetRes(Pref, 2289); //Compatibilité


            HtmlGenericControl ulFeatures = new HtmlGenericControl("ul");
            pnAddInfos.Controls.Add(ulFeatures);
            ulFeatures.Attributes.Add("class", "store-features");

            #region verification de la version

            HtmlGenericControl liFeature = new HtmlGenericControl("li");
            ulFeatures.Controls.Add(liFeature);

            i = new HtmlGenericControl("i");
            liFeature.Controls.Add(i);

            span = new HtmlGenericControl("span");
            liFeature.Controls.Add(span);
            span.Attributes.Add("class", "store-features-txt");

            if (Extension.Infos.IsCompatible)
            {
                i.Attributes.Add("class", "icon-check");
                span.InnerText = eResApp.GetRes(_ePref, 2301); //Cette extension est compatible avec votre version d’Eudonet
            }
            else
            {
                i.Attributes.Add("class", "icon-edn-cross");
                span.Controls.Add(new LiteralControl(
                   String.Format(eResApp.GetRes(_ePref, 2302), Extension.Infos.MinEudoVersion) //Cette extension est compatible à partir de la version XX.XXX d’Eudonet.
                    ));

                span.Controls.Add(new LiteralControl(" "));

                a = new HtmlGenericControl("a") { InnerText = eResApp.GetRes(_ePref, 2291) }; //Contactez-nous
                span.Controls.Add(a);

                span.Controls.Add(new LiteralControl(" "));

                span.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 2307))); // pour migrer dans la bonne version.


                sb = new StringBuilder("mailto:");
                sb.Append(_emailContact)
                    .Append("?subject=").AppendFormat(eResApp.GetRes(_ePref, 2303), Pref.EudoBaseName, Extension.Title) //[Eudonet <NumBase>] Extension <Nom>
                    .Append("&body=").AppendFormat(eResApp.GetRes(_ePref, 2304), Extension.Title, Pref.ClientInfos.ClientOffer, Pref.User.UserDisplayName); //Bonjour, je viens de découvrir l’extension <Nom> et je souhaite pouvoir l’utiliser dans ma base Eudonet. Il semble qu’il y ait une incompatibilité de version ou qu’elle ne soit pas disponible pour l’offre <OffreActive>. Pouvez-vous me recontacter à ce sujet ? Merci, Cordialement, <Nom utilisateur>
                a.Attributes.Add("href", sb.ToString());
            }
            #endregion verification de la  version


            #region verification de l'offre
            liFeature = new HtmlGenericControl("li");
            ulFeatures.Controls.Add(liFeature);

            i = new HtmlGenericControl("i");
            liFeature.Controls.Add(i);

            span = new HtmlGenericControl("span");
            liFeature.Controls.Add(span);
            span.Attributes.Add("class", "store-features-txt");

            if (Extension.Infos.IsAvailableInOffer(Pref.ClientInfos.ClientOffer))
            {
                i.Attributes.Add("class", "icon-check");
                span.InnerText = String.Format(eResApp.GetRes(_ePref, 2306), Pref.ClientInfos.ClientOffer); //Cette extension est disponible pour votre offre XXXX.
            }
            else
            {
                i.Attributes.Add("class", "icon-edn-cross");
                span.Controls.Add(new LiteralControl(
                    string.Format(eResApp.GetRes(_ePref, 2305), eLibTools.GetFormattedString(Pref, Extension.Infos.OffersLst?.Select(eo => eo.Offer))) //Cette extension est disponible pour les offres YYYY et ZZZZ.
                    ));

                span.Controls.Add(new LiteralControl(" "));


                a = new HtmlGenericControl("a") { InnerText = eResApp.GetRes(_ePref, 2291) }; //Contactez-nous
                span.Controls.Add(a);

                span.Controls.Add(new LiteralControl(" "));

                span.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 2307))); // pour migrer dans la bonne version.

                sb = new StringBuilder("mailto:");
                sb.Append(_emailContact)
                    .Append("?subject=").AppendFormat(eResApp.GetRes(_ePref, 2303), Pref.EudoBaseName, Extension.Title) //[Eudonet <NumBase>] Extension <Nom>
                    .Append("&body=").AppendFormat(eResApp.GetRes(_ePref, 2304), Extension.Title, Pref.ClientInfos.ClientOffer, Pref.User.UserDisplayName); //Bonjour, je viens de découvrir l’extension <Nom> et je souhaite pouvoir l’utiliser dans ma base Eudonet. Il semble qu’il y ait une incompatibilité de version ou qu’elle ne soit pas disponible pour l’offre <OffreActive>. Pouvez-vous me recontacter à ce sujet ? Merci, Cordialement, <Nom utilisateur>
                a.Attributes.Add("href", sb.ToString());


            }

            #endregion verification de l'offre


            //Annulé vu avec ALEBR

            //HtmlGenericControl h3Version = new HtmlGenericControl("h3");
            //pnAddInfos.Controls.Add(h3Version);
            //h3Version.Attributes.Add("class", "store-title-level-3");
            //h3Version.InnerText = eResApp.GetRes(Pref, 2290); //Version


            //HtmlGenericControl p = new HtmlGenericControl("p");
            //pnAddInfos.Controls.Add(p);
            //p.InnerHtml = Extension.Infos.Version;

            //Signalez un problème
            //a = new HtmlGenericControl("a");
            //a.Attributes.Add("href", "#");
            //a.Attributes.Add("class", "store-warning");
            //pnAddInfos.Controls.Add(a);
            //a.Controls.Add(new LiteralControl(eResApp.GetRes(_ePref, 2296)));





            return pn;
        }


        #endregion description

        #endregion

        #region "EnTête Fichiers"
        /// <summary>
        /// Fonction qui va créer une entête pour chaque élément de l'EudoStore.
        /// Elle retourne un Panel possédant tous les panels enfants (si possible des composants Web
        /// System.Web.UI.WebControls, suivant
        /// les demandes faites par les utilisateurs, le type d'élément de l'Eudostore...
        /// </summary>
        /// <returns></returns>

        private Panel GetEnteteStoreApplication()
        {
            #region Bandeau

            eAdminExtensionInfo infos = _extension.Infos;

            string strSsTitre = (infos.Categories.Count > 0 && !infos.Categories.ContainsKey("-1")) ? string.Join(", ", infos.Categories.Values.Select(category => category.Trim())) : "&nbsp;";
            string sCssExtInstalled;
            string sTxtExtInstalled;
            string sTootlTip;
            string sCssBeingInstalled = string.Empty;

            bool bAddStatusIcn = AddStatusIcon(_extension, out sCssExtInstalled, out sTootlTip, out sTxtExtInstalled);
            bool bActionActivate = (((infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED) && (Pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN))
                                    || ((infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED) && (Pref.User.UserLevel < (int)EudoQuery.UserLevel.LEV_USR_SUPERADMIN))
                                    || (!infos.IsEnabled));

            /** Gros Hack des familles. 
             * le isEnabled est à false quand on demande une désactivation. Donc, il faut palier. 
             */
            if (((infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED) && (Pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN))
                //Correction bug "Annulation demande d'installation" impossible à faire pour un administrateur
                || (infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED) && (Pref.User.UserLevel == (int)EudoQuery.UserLevel.LEV_USR_ADMIN))
            {
                bActionActivate = false;
            }

            string sLienBouton = "javascript:nsAdmin.enableExtension('" + Extension.Module.ToString() + "', " + ((bActionActivate) ? "true" : "false") + ", false, { extensionFileId: " + infos.ExtensionFileId + ", extensionCode: '" + infos.ExtensionNativeId + "', extensionLabel: '" + Extension.Infos.Title.Replace("'", "\\'") + "' } )";
            string sCssHlInstaller = "store-btn store-btn-success store-btn-big";
            string sInfoHlInstaller = eResApp.GetRes(Pref, 8752);
            string sTxtHlInstaller = eResApp.GetRes(Pref, 336);

            string sObjMail = eResApp.GetRes(Pref, 2274).Replace("<NumBase>", Pref.GetBaseName).Replace("<Nom>", infos.Title);
            string sCorpsMail = eResApp.GetRes(Pref, 2275).Replace(@"\r\n", @"%0D%0A").Replace("<Nom>", infos.Title).Replace("<OffreActive>", Pref.ClientInfos.ClientOffer.ToString()).Replace("<Nom utilisateur>", Pref.User.UserName);

            if (infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED)
            {
                sInfoHlInstaller = eResApp.GetRes(Pref, 2282);
                sTxtHlInstaller = eResApp.GetRes(Pref, 2279);
                sCssHlInstaller = "store-btn store-btn-remove store-btn-big";
                sCssBeingInstalled = " being-installed";

                if (Pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                {
                    sInfoHlInstaller = eResApp.GetRes(Pref, 2281);
                    sTxtHlInstaller = eResApp.GetRes(Pref, 2280);
                    sCssHlInstaller = "store-btn store-btn-success store-btn-big";
                }
            }
            else if (infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED)
            {
                sInfoHlInstaller = eResApp.GetRes(Pref, 2284);
                sTxtHlInstaller = eResApp.GetRes(Pref, 2283);
                sCssHlInstaller = "store-btn store-btn-remove store-btn-big";
                sCssBeingInstalled = " being-installed";

                if (Pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                {
                    sInfoHlInstaller = eResApp.GetRes(Pref, 2286);
                    sTxtHlInstaller = eResApp.GetRes(Pref, 2285);
                }
            }
            else if (infos.IsEnabled)
            {
                sCssHlInstaller = "store-btn store-btn-remove store-btn-big";
                sInfoHlInstaller = eResApp.GetRes(Pref, 2287);
                sTxtHlInstaller = eResApp.GetRes(Pref, 2278);

                if (!Extension.IsUnInstallable)
                {
                    sInfoHlInstaller = eResApp.GetRes(Pref, 8508);
                    sCssHlInstaller = "store-btn store-btn-disabled store-btn-big";
                    sLienBouton = "";
                }
            }
            else
            {
                if (!infos.Price.Included)
                    sTxtHlInstaller = eResApp.GetRes(Pref, 2277);
            }

            #region Initilisation des contrôles du bandeau
            Panel pnStoreXtdDtlHeader = new Panel { CssClass = "store-extension-detail-header" + sCssBeingInstalled };
            Panel pnStoreContainer = new Panel { CssClass = "store-container" };
            Panel pnDtlHeaderCtn = new Panel { CssClass = "detail-header-container" };
            Panel pnFstCol = new Panel { CssClass = "store-first-col" };
            Panel pnStoreDtlFst = new Panel { CssClass = "store-detail-first" };
            Panel pnDtlLogoCtn = new Panel { CssClass = "store-detail-logo-container" };

            Image imgLogo = new Image
            {
                ImageUrl = !string.IsNullOrEmpty(infos.Icon) ? infos.Icon : "",
                //ToolTip = infos.Tooltip,
                CssClass = "store-detail-logo"
            };

            Panel pnStoreTitleContainer = new Panel { CssClass = "store-title-container" };
            Panel pnStoreTitle = new Panel { CssClass = "store-title" };

            HtmlGenericControl h1Titre = new HtmlGenericControl("h1");
            h1Titre.Attributes.Add("class", "store-title");
            h1Titre.InnerHtml = _extension.Title;

            Panel pnStoreIconContainer = new Panel { CssClass = "store-icon-container" };

            Label lblSousTitle = new Label { CssClass = "sous-title", Text = strSsTitre };
            Panel pnStoreHeaderInfos = new Panel { CssClass = "store-header-infos" };
            Label lblIconCheck = new Label { CssClass = "icon-check" };

            Label lblInclusOffre = new Label();

            Label lblInclusOffreBdo = new Label();
            Label lblInclusOffreBdoInstall = new Label();
            Label lblInclusOffreBdoPrix = new Label
            {
                ForeColor = System.Drawing.Color.Black
            };
            Label lblInclusOffreBdoSeparation = new Label
            {
                ForeColor = System.Drawing.Color.Black
            };

            string strInclusOffre = string.Empty;

            if (infos.Price == null)
                strInclusOffre = "";
            else if (infos.Price.Included)
                strInclusOffre = eResApp.GetRes(Pref, 2263);
            else
                strInclusOffre = (!string.IsNullOrEmpty(infos.Price?.Price) ? infos.Price.Price.Trim() + "&nbsp;" + infos.Price.Unit.Trim() : "");

            lblInclusOffre.Text = strInclusOffre;
            lblInclusOffreBdoSeparation.Text = (!string.IsNullOrEmpty(strInclusOffre) && !string.IsNullOrEmpty(sTootlTip) ? "&nbsp;|&nbsp;" : "");

            lblInclusOffreBdoPrix.Text = strInclusOffre;
            lblInclusOffreBdoInstall.Text = ((bAddStatusIcn && (infos.IsEnabled || (infos.Status == EXTENSION_STATUS.STATUS_ACTIVATION_ASKED)
                                                                                || (infos.Status == EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED))) ? sTootlTip : "");

            Label lblExtInstalledBdo = new Label
            {
                Text = sTxtExtInstalled,
                Visible = bAddStatusIcn
            };

            Panel pnStoreLeftCol = new Panel { CssClass = "store-left-col" + ((!infos.IsCompatible) ? " store-incompatible" : "") };
            HyperLink hlInstaller = new HyperLink
            {
                ToolTip = sInfoHlInstaller,
                CssClass = sCssHlInstaller,
                Text = sTxtHlInstaller,
                NavigateUrl = sLienBouton
            };

            HtmlGenericControl hP = new HtmlGenericControl("p")
            {
                InnerText = eResApp.GetRes(Pref, 2273)
            };
            HyperLink hlContact = new HyperLink
            {
                Text = eResApp.GetRes(Pref, 2291),
                NavigateUrl = $"mailto:{_emailContact}?cc={Pref.User.UserMail}&subject={sObjMail}&body={sCorpsMail}"
            };
            HyperLink hlContactScroll = new HyperLink
            {
                Text = eResApp.GetRes(Pref, 2291),
                NavigateUrl = $"mailto:{_emailContact}?cc={Pref.User.UserMail}&subject={sObjMail}&body={sCorpsMail}",
                CssClass = "store-contact-scroll"
            };
            Panel pnStoreWarning = new Panel
            {
                CssClass = "store-warning-msg"
            };

            #endregion


            #region insertion des controles du bandeau
            pnDtlLogoCtn.Controls.Add(imgLogo);

            if (_extension?.Infos?.OffersLst != null)
                _extension.Infos.OffersLst = _extension.Infos.OffersLst.OrderByDescending(ex => (int)ex.Offer);

            // Offres
            foreach (var elem in GetOfferImg(_extension, false).ToList())
                pnStoreIconContainer.Controls.Add(elem);

            if (bAddStatusIcn)
            {
                pnStoreLeftCol.Controls.Add(lblExtInstalledBdo);
            }

            //if (infos.Price?.Included ?? false)
            //{
            //    pnStoreHeaderInfos.Controls.Add(lblIconCheck);
            //}
            lblInclusOffreBdo.Controls.Add(lblInclusOffreBdoInstall);
            lblInclusOffreBdo.Controls.Add(lblInclusOffreBdoSeparation);
            lblInclusOffreBdo.Controls.Add(lblInclusOffreBdoPrix);
            pnStoreHeaderInfos.Controls.Add(lblInclusOffreBdo);

            if (!infos.IsEnabled && (!infos.IsCompatible || !infos.IsAvailableInOffer(Pref.ClientInfos.ClientOffer)))
            {
                pnStoreWarning.Controls.Add(hP);
                pnStoreWarning.Controls.Add(hlContact);

                pnStoreLeftCol.Controls.Add(pnStoreWarning);
                pnStoreLeftCol.Controls.Add(hlContactScroll);
            }
            else
            {
                pnStoreLeftCol.Controls.Add(lblInclusOffre);
                pnStoreLeftCol.Controls.Add(hlInstaller);
            }

            pnStoreTitle.Controls.Add(h1Titre);
            pnStoreTitle.Controls.Add(pnStoreIconContainer);

            pnStoreTitleContainer.Controls.Add(pnStoreTitle);
            pnStoreTitleContainer.Controls.Add(lblSousTitle);
            pnStoreTitleContainer.Controls.Add(pnStoreHeaderInfos);

            pnStoreDtlFst.Controls.Add(pnDtlLogoCtn);
            pnStoreDtlFst.Controls.Add(pnStoreTitleContainer);

            pnDtlHeaderCtn.Controls.Add(pnFstCol);
            pnDtlHeaderCtn.Controls.Add(pnStoreLeftCol);

            pnFstCol.Controls.Add(pnStoreDtlFst);
            pnStoreContainer.Controls.Add(pnDtlHeaderCtn);
            pnStoreXtdDtlHeader.Controls.Add(pnStoreContainer);
            #endregion

            return pnStoreXtdDtlHeader;
            #endregion
        }
        #endregion
    }


    /// <summary>
    /// représentation basique d'un field input
    /// </summary>
    public struct eBasicInputField
    {
        /// <summary>
        /// Label de l'inout
        /// </summary>
        public string Label;

        /// <summary>
        /// EdnId de l'input
        /// </summary>
        public string EndId;

        /// <summary>
        /// valeur de l'inout
        /// </summary>
        public string Value;

        /// <summary>
        /// Field eventuel
        /// </summary>
        public eAdminField Fld;
    }

}