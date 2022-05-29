using Com.Eudonet.Internal;
using EudoExtendedClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de base des écrans affichant les infos liées aux extensions (liste des extensions, fiche d'une extension)
    /// </summary>
    public abstract class eAdminExtensionRenderer : eAdminModuleRenderer
    {
        /// <summary>
        /// Extensions correspondant aux critères de recherche ET à la pagination
        /// </summary>
        protected List<eAdminExtension> _includedExtensionList;
        /// <summary>
        /// Extensions correspondant aux critères de recherche, mais hors pagination
        /// </summary>
        protected List<eAdminExtension> _hiddenExtensionList;
        /// <summary>
        /// Extensions ne correspondant ni aux critères de recherche ni à la pagination
        /// </summary>
        protected List<eAdminExtension> _excludedExtensionList;
        /// <summary>
        /// Indique si l'accès aux informations d'extensions peut se faire en effectuant une connexion API vers HotCom, ou si on doit passer par un cache
        /// d'extensions local (ExtensionList.json dans /eudonetXRM/Res/). Cette variable est mise à true si la connexion vers HotCom échoue,
        /// ou si la clé ServerWithoutInternet est valorisée à 1 dans le fichier server.config de /eudonetXRM (cf. ci-dessous)
        /// </summary>
        protected bool bNoInternet = false;
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminExtensionRenderer(ePref pref)
            : base(pref)
        {
            bNoInternet = eLibTools.GetServerConfig("ServerWithoutInternet", "0") == "1";
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminExtensions))
                return false;

            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            return true;
        }

        /// <summary>
        /// Retourne le Panel correspondant à une extension, à afficher en mode Liste ou Fiche
        /// </summary>
        /// <param name="extension">Extension concernée</param>
        /// <returns></returns>
        public abstract Panel GetExtensionPanel(eAdminExtension extension);

        /// <summary>
        /// Retourne le Panel correspondant à une extension, à afficher en mode Liste ou Fiche
        /// </summary>
        /// <param name="pref">Objet Pref</param>
        /// <param name="extension">Objet eAdminExtension contenant les informations de l'extension concernée</param>
        /// <param name="listMode">Indique si le rendu du panel doit être effectué pour un mode Liste (avec bordures et taille) ou Fiche (sans bordure et plein écran)</param>
        /// <returns></returns>
        public static Panel GetExtensionPanel(ePref pref, eAdminExtension extension, bool listMode)
        {
            eAdminExtensionInfo infos = extension.Infos;


            //Panel Principal
            Panel globalPanel = new Panel();
            globalPanel.CssClass = listMode ? "blockExtension" : "blockExtensionFile";

            string sBaseID = string.Concat(extension.Module.ToString(), "_", infos.ExtensionFileId);

            globalPanel.ID = String.Concat("extensionBlock_", sBaseID);



            string iconClass = String.Empty;
            string iconUrl = String.Empty;

            if (infos.Icon.StartsWith("http://") || infos.Icon.StartsWith("https://"))
                iconUrl = infos.Icon;
            else
                iconClass = infos.Icon;

            #region Main part



            Panel mainPart = new Panel();
            mainPart.CssClass = "mainPart";


            #region sous-partie
            Panel divPart1 = new Panel();
            divPart1.Attributes.Add("class", string.Concat("extensionMain ", (listMode ? "extensionMainList" : "extensionMainFile")));
            divPart1.ID = string.Concat("part1_", sBaseID);

            Panel divPart2 = new Panel();
            divPart2.ID = string.Concat("part2_", sBaseID);
            divPart2.Attributes.Add("class", string.Concat("extensionMain ", (listMode ? "extensionMainList" : "extensionMainFile")));

            Panel divPart3 = new Panel();
            divPart3.ID = string.Concat("part3_", sBaseID);
            divPart3.Attributes.Add("class", string.Concat("extensionMain ", (listMode ? "extensionMainList" : "extensionMainFile")));
            #endregion



            #region ICON

            if (infos.IsNewExtension)
            {
                Panel panelNew = new Panel();
                panelNew.CssClass = "flagNew";
                panelNew.Controls.Add(new LiteralControl(eResApp.GetRes(pref, 31)));
                mainPart.Controls.Add(panelNew);
            }

            // Logo/icône - Soit à partir d'une classe CSS (EudoFont), soit à partir d'une URL
            if (!String.IsNullOrEmpty(iconClass))
            {
                HtmlGenericControl img = new HtmlGenericControl("div");
                img.Attributes.Add("class", string.Concat(iconClass, " extensionIcon ", (listMode ? "extensionIconList" : "extensionIconFile")));

                img.ID = string.Concat("extensionIcon_", sBaseID);

                mainPart.Controls.Add(img);
            }
            else
            {
                HtmlImage img = new HtmlImage();
                img.ID = string.Concat("extensionIcon_", sBaseID);
                img.Alt = infos.Tooltip;
                img.Src = iconUrl;
                img.Attributes.Add("title", infos.Tooltip);
                img.Attributes.Add("class", string.Concat(iconClass, " extensionIcon ", (listMode ? "extensionIconList" : "extensionIconFile")));
                mainPart.Controls.Add(img);
            }



            #endregion

            #region Titre

            //TITRE
            Panel pntTitle = new Panel();
            pntTitle.CssClass = string.Concat("extensionTitle ", (listMode ? "extensionTitleList" : "extensionTitleFile"));

            HtmlGenericControl hTitle = new HtmlGenericControl("h3");
            hTitle.Attributes.Add("class", "extensionTitle");
            hTitle.InnerText = extension.Title;
            hTitle.Attributes.Add("onclick", String.Concat("javascript:nsAdmin.loadAdminModule('", extension.Module.ToString(), "', null, { extensionFileId: ", infos.ExtensionFileId, ", extensionCode: '", infos.ExtensionNativeId, "', extensionLabel: '", extension.Infos.Title.Replace("'", "\\'"), "' });"));
            pntTitle.Controls.Add(hTitle);

            #endregion

            #region Catégorie

            if (extension.Infos.Categories.Count > 0 && !extension.Infos.Categories.ContainsKey("-1"))
            {
                HtmlGenericControl hCategory = new HtmlGenericControl("h6");
                hCategory.Attributes.Add("class", "extensionCategory");
                hCategory.InnerText = String.Join(", ", extension.Infos.Categories.Values.Select(category => category.Trim()).ToArray());
                pntTitle.Controls.Add(hCategory);
            }

            #endregion

            #region Description
            HtmlGenericControl divDescription = new HtmlGenericControl("div");
            divDescription.Attributes.Add("class", "extensionDescription " + (listMode ? "extensionDescriptionList" : "extensionDescriptionFile"));
            divDescription.Controls.Add(GetExtensionInfoControl(pref, extension));
            #endregion

            #region Author
            HtmlGenericControl divAuthor = new HtmlGenericControl("div");
            divAuthor.Attributes.Add("class", "extensionAuthor " + (listMode ? "extensionAuthorList" : "extensionAuthorFile"));
            if (!String.IsNullOrEmpty(infos.Author))
            {
                string authorHtml = infos.Author;
                string authorUrl = infos.AuthorUrl;
                // Ajout d'un protocole par défaut sur l'URL si manquant
                if (!Uri.IsWellFormedUriString(authorUrl, UriKind.Absolute))
                    authorUrl = String.Concat("http://", authorUrl);
                if (Uri.IsWellFormedUriString(authorUrl, UriKind.Absolute))
                    authorHtml = String.Concat("<a href=\"", new Uri(authorUrl).ToString(), "\" target=\"_blank\">", authorHtml, "</a>");
                divAuthor.InnerHtml = eResApp.GetRes(pref, 60) + " <span>" + authorHtml + "</span>";
            }
            else
                divAuthor.InnerHtml = "&nbsp;";
            #endregion

            #region Tarif
            HtmlGenericControl dTarif = new HtmlGenericControl("div");
            dTarif.Attributes.Add("class", "extensionTarifs " + (listMode ? "extensionTarifsList" : "extensionTarifsFile"));
            if (infos.Price != null)
            {
                //foreach (var v in infos.Tarifs)
                //{
                // Prix
                var t = new HtmlGenericControl("p");
                t.InnerHtml = infos.Price.Price;
                dTarif.Controls.Add(t);
                // Unité
                t = new HtmlGenericControl("p");
                t.InnerHtml = infos.Price.Unit;
                dTarif.Controls.Add(t);
                //}
            }
            else
                dTarif.InnerHtml = "&nbsp;";
            #endregion

            #region Bouton

            // Bouton "Activer" et "En savoir plus"
            Panel panelButtons = new Panel();
            panelButtons.CssClass = (listMode ? "extensionButtonsList" : "extensionButtonsFile") + " extensionButtons";


            HtmlGenericControl link = new HtmlGenericControl("a");
            //link.Attributes.Add("title", infos.Tooltip);
            panelButtons.Controls.Add(link);

            // Action et label du bouton
            bool bActionActivate = false;
            string fileModeTooltip = String.Empty;
            switch (extension.Infos.Status)
            {
                case EXTENSION_STATUS.STATUS_ACTIVATION_ASKED:
                    link.InnerText = eResApp.GetRes(pref, 8100); //"Activation demandée"

                    if (listMode)
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " extensionBtnLong"));

                    link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExt btnExtActivationAsked"));

                    if (pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                    {
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExtEnable"));

                        fileModeTooltip = eResApp.GetRes(pref, 6744); //"Activer
                        bActionActivate = true;
                    }
                    else
                    {
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExtDisable"));

                        fileModeTooltip = eResApp.GetRes(pref, 8506); //"Annuler la demande d'activation"
                        bActionActivate = false;
                    }

                    break;
                case EXTENSION_STATUS.STATUS_DESACTIVATION_ASKED:
                    link.InnerText = fileModeTooltip = eResApp.GetRes(pref, 8101); //"Désactivation demandée"

                    if (listMode)
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " extensionBtnLong"));

                    link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExt btnExtDisactivationAsked"));

                    if (pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                    {
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExtDisable"));

                        fileModeTooltip = eResApp.GetRes(pref, 6745);  //"Désactiver
                        bActionActivate = false;
                    }
                    else
                    {
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExtEnable"));

                        fileModeTooltip = eResApp.GetRes(pref, 8507);  //"Annuler la demande de désactivation"
                        bActionActivate = true;
                    }
                    break;
                case EXTENSION_STATUS.STATUS_READY:
                case EXTENSION_STATUS.STATUS_DISABLED:
                default:
                    if (extension.IsEnabled())
                    {
                        link.InnerText = eResApp.GetRes(pref, 7219); //"Actif"
                        if (pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                            fileModeTooltip = eResApp.GetRes(pref, 6745);  //"Désactiver
                        else
                            fileModeTooltip = eResApp.GetRes(pref, 8505);  //"Demander la désactivation"
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExt btnExtActive"));
                        bActionActivate = false;
                    }
                    else
                    {
                        link.InnerText = eResApp.GetRes(pref, 6744); //"Activer"
                        if (pref.User.UserLevel > (int)EudoQuery.UserLevel.LEV_USR_ADMIN)
                            fileModeTooltip = eResApp.GetRes(pref, 6744); //"Activer
                        else
                            fileModeTooltip = eResApp.GetRes(pref, 8504); //"Demander l'activation"
                        link.Attributes.Add("class", string.Concat(link.Attributes["class"], " btnExt btnExtUnactive"));
                        bActionActivate = true;
                    }
                    break;
            }

            if (extension.IsUnInstallable)
            {
                if (listMode)
                {
                    link.Attributes.Add("title", infos.Tooltip);
                    link.Attributes.Add("onclick", String.Concat("javascript:nsAdmin.loadAdminModule('", extension.Module.ToString(), "', null, { extensionFileId: ", infos.ExtensionFileId, ", extensionCode: '", infos.ExtensionNativeId, "', extensionLabel: '", extension.Infos.Title.Replace("'", "\\'"), "' });"));
                }
                else
                {
                    link.Attributes.Add("title", fileModeTooltip);
                    link.Attributes.Add("onclick", String.Concat("javascript:nsAdmin.enableExtension('", extension.Module.ToString(), "', ", bActionActivate ? "true" : "false", ", ", listMode ? "true" : "false, { extensionFileId: ", infos.ExtensionFileId, ", extensionCode: '", infos.ExtensionNativeId, "', extensionLabel: '", extension.Infos.Title.Replace("'", "\\'"), "' }    )"));
                }
            }
            else
            {
                link.Attributes.Add("title", eResApp.GetRes(pref, 8508)); //"Cette extension ne peut être désactivée"
                link.Attributes.Add("class", link.Attributes["class"] + " btnReadOnly");
            }

            #endregion

            #region En Savoir Plus



            // More Info
            Panel pnlMore = new Panel();
            pnlMore.CssClass = "btnMore";
            pnlMore.ID = "btnMore";
            HtmlGenericControl linkMore = new HtmlGenericControl("a");
            linkMore.Attributes.Add("class", "btnMore");
            linkMore.InnerText = eResApp.GetRes(pref, 7870);
            linkMore.Attributes.Add("onclick", String.Concat("javascript:nsAdmin.loadAdminModule('", extension.Module.ToString(), "', null, { extensionFileId: ", infos.ExtensionFileId, ", extensionCode: '", infos.ExtensionNativeId, "', extensionLabel: '", extension.Infos.Title.Replace("'", "\\'"), "' });"));
            pnlMore.Controls.Add(linkMore);

            #endregion

            #region Footer
            Panel footer = new Panel();
            footer.CssClass = "extensionFooter";

            HtmlGenericControl info = null;

            Panel footerLeft = new Panel();
            footerLeft.CssClass = "footerLeft";
            footer.Controls.Add(footerLeft);

            // Note (étoiles)
            HtmlGenericControl note = new HtmlGenericControl("div");
            note.Attributes.Add("class", "notation");
            HtmlGenericControl star;
            for (int i = 1; i <= 5; i++)
            {
                star = new HtmlGenericControl();
                star.Attributes.Add("class", "icon-star " + ((i <= infos.Notation.Value) ? "active" : ""));
                note.Controls.Add(star);
            }
            //  footerLeft.Controls.Add(note);

            // Installée X fois
            if (infos.NbInstallations > 0)
            {
                info = new HtmlGenericControl("p");
                info.InnerText = eResApp.GetRes(pref, 7873).Replace("<NB>", infos.NbInstallations.ToString());
                footerLeft.Controls.Add(info);
            }

            Panel footerRight = new Panel();
            footerRight.CssClass = "footerRight";
            footer.Controls.Add(footerRight);

            // Dernière mise à jour
            HtmlGenericControl lastMaj = null;
            if (infos.LastUpdate > DateTime.MinValue)
            {
                lastMaj = new HtmlGenericControl("p");
                lastMaj.InnerText = String.Concat(eResApp.GetResWithColon(pref, 7871), " ", infos.LastUpdate.ToShortDateString());
                //footerRight.Controls.Add(info);
            }

            // Disponible dans les offres..
            String offers = infos.Offers;
            String[] arrOffers = offers.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < arrOffers.Length; i++)
            {
                arrOffers[i] = arrOffers[i].ToCapitalize().Trim();
            }

            StringBuilder sbOffersLabel = new StringBuilder();
            if (arrOffers.Length > 0)
            {
                sbOffersLabel.AppendFormat(eResApp.GetRes(pref, 8214),
                    (arrOffers.Length > 1) ? eResApp.GetRes(pref, 8216) : eResApp.GetRes(pref, 8215),
                    String.Join(", ", arrOffers)
                    );
            }
            HtmlGenericControl pOffers = new HtmlGenericControl("p");
            pOffers.InnerText = sbOffersLabel.ToString();

            // Version
            HtmlGenericControl isCompatbible = new HtmlGenericControl("p");
            if (infos.IsCompatible)
                isCompatbible.InnerText = eResApp.GetRes(pref, 7872);
            else
                isCompatbible.InnerText = eResApp.GetRes(pref, 8098);


            //En fonction mode liste/mode fiche, l'ordre "compatible/date dernière maj n'est pas le même
            if (listMode)
            {
                if (lastMaj != null)
                    footerRight.Controls.Add(lastMaj);

                footerRight.Controls.Add(isCompatbible);

                footerRight.Controls.Add(pOffers);
            }
            else
            {
                footerRight.Controls.Add(isCompatbible);
                if (lastMaj != null)
                    footerRight.Controls.Add(lastMaj);

                footerRight.Controls.Add(pOffers);
            }


            //
            #endregion

            //Création des blocs
            if (listMode)
            {

                /*
                 *         | --------------------------------------------------------|
                 *         |  ICON    |   TITRE          |     BOUTON                |
                 *         |          | ---------------------------------------------|
                 *         |----------|    Résumé        |        en savoir +        |        
                 *         |          | ---------------------------------------------|
                 *         |          |   auteur         |       Tarifs              |        
                 *         | --------------------------------------------------------|
                 *         |  Nb Install                        Date de Maj          |
                 *         | --------------------------------------------------------|
                 */


                mainPart.Controls.Add(divPart1);
                mainPart.Controls.Add(divPart2);
                mainPart.Controls.Add(divPart3);

                divPart1.Controls.Add(pntTitle);        // Titre
                divPart1.Controls.Add(panelButtons);    // Bouton

                divPart2.Controls.Add(divDescription);   //De                 
                divPart2.Controls.Add(pnlMore);


                divPart3.Controls.Add(divAuthor);
                divPart3.Controls.Add(dTarif);

                mainPart.Controls.Add(divPart3);

                //Information installation, dernière version...

                globalPanel.Controls.Add(footer);

            }
            else
            {

                divPart1.Controls.Add(pntTitle);
                divPart1.Controls.Add(divDescription);
                divPart1.Controls.Add(divAuthor);
                divPart1.Controls.Add(dTarif);
                mainPart.Controls.Add(divPart1);

                footer.Controls.Add(panelButtons);
                globalPanel.Controls.Add(footer);
            }

            globalPanel.Controls.Add(mainPart);


            #endregion




            return globalPanel;
        }

        /// <summary>
        /// Renvoie les informations détaillés concernant l'extension si elle est activée.
        /// Si elle n'est pas activée, renvoie un message invitant l'administrateur à l'activer
        /// </summary>
        /// <param name="extension">Extension concernée</param>
        /// <returns>Informations à afficher sur la tuile de l'extension</returns>
        private static Control GetExtensionInfoControl(ePref pref, eAdminExtension extension)
        {
            if (extension != null && extension.Infos.IsEnabled)
            {
                HtmlGenericControl ulInfo = new HtmlGenericControl("ul");
                ulInfo.Attributes.Add("class", "extensionInfoList");

                List<HtmlGenericControl> moduleInfoControls = extension.GetModuleInfo();

                if (moduleInfoControls.Count > 0)
                {
                    foreach (HtmlGenericControl ctrl in moduleInfoControls)
                    {
                        HtmlGenericControl liInfo = new HtmlGenericControl("li");
                        liInfo.Controls.Add(ctrl);
                        ulInfo.Controls.Add(liInfo);
                    }
                }
                // Si l'extension ne renvoie aucune info personnalisée, on affiche son résumé
                else
                {
                    HtmlGenericControl spanInfo = new HtmlGenericControl("span");
                    HtmlGenericControl spanInfoValue = new HtmlGenericControl("span");
                    spanInfoValue.Attributes.Add("class", "extensionInfoValue");
                    spanInfoValue.InnerHtml = extension.Infos.Summary;
                    spanInfo.Controls.Add(spanInfoValue);
                    ulInfo.Controls.Add(spanInfo);
                }

                return ulInfo;
            }
            else
            {
                HtmlGenericControl spanInfoModuleDisabled = new HtmlGenericControl("span");
                spanInfoModuleDisabled.Attributes.Add("class", "extensionDisabled");
                //spanInfoModuleDisabled.InnerText = eResApp.GetRes(pref, 7864).Replace("<EXTENSION>", extension.Title); // L'extension <EXTENSION> n'est pas activée sur votre base. Cliquez sur le bouton ci-contre pour l'activer

                spanInfoModuleDisabled.InnerHtml = extension.Infos.Summary;

                return spanInfoModuleDisabled;
            }
        }
    }
}




