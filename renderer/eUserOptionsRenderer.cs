using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// Renderer racine pour tout renderer affichable en Administration
    /// </summary>
    public class eUserOptionsRenderer : eRenderer
    {
        /// <summary>
        /// Module dont les options doivent être généré
        /// </summary>
        private eUserOptionsModules.USROPT_MODULE _module = eUserOptionsModules.USROPT_MODULE.UNDEFINED;

        /// <summary>
        /// Module dont les options doivent être généré
        /// </summary>
        public eUserOptionsModules.USROPT_MODULE Module
        {
            get { return _module; }
            set { _module = value; }
        }

        private bool IsSSOApp { get; set; }
        private bool IsADFS { get; set; }
        private bool IsLDAPEnabled { get; set; }

        /// <summary>
        /// Id de l'utilisateur dont on veut rendre les options
        /// </summary>
        protected int _nUserId;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eUserOptionsRenderer(ePref pref, int nUserId = 0)
        {

            Pref = pref;
            _rType = RENDERERTYPE.Admin;
            _module = eUserOptionsModules.USROPT_MODULE.UNDEFINED;
            if (pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                _nUserId = nUserId;
            else
                _nUserId = nUserId;
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        public eUserOptionsRenderer(ePref pref, eUserOptionsModules.USROPT_MODULE module, int nUserId = 0)
        {
            Pref = pref;
            _rType = RENDERERTYPE.Admin;
            _module = module;

            if (pref.User.UserLevel >= (int)UserLevel.LEV_USR_ADMIN)
                _nUserId = nUserId;
            else
                _nUserId = nUserId; // _nUserId = pref.UserId; // si pas admin, on ne peut modifier que ses options

            if (module == eUserOptionsModules.USROPT_MODULE.PREFERENCES || module == eUserOptionsModules.USROPT_MODULE.MAIN)
                this.GetAuthentificationOptions();
        }


        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {
            bool bIsUserAdmin = Pref.User.UserLevel >= UserLevel.LEV_USR_ADMIN.GetHashCode();

            Panel pnlAdminCont = new Panel();
            pnlAdminCont.CssClass = "admin_cont";
            pnlAdminCont.ID = "myEudonet";

            //string subPanelCssClass = "twoblocks";
            //if (_module == eUserOptionsModules.USROPT_MODULE.ADMIN)
            //    subPanelCssClass = "threeblocks";

            #region Block profil
            if (_module == eUserOptionsModules.USROPT_MODULE.MAIN)
            {
                pnlAdminCont.Controls.Add(BuildUserBlock());
            }
            #endregion

            #region Blocs de droite

            Panel panelRight = new Panel();
            panelRight.CssClass = "blockRight";

            #region Blocs Préférences
            if (_module == eUserOptionsModules.USROPT_MODULE.MAIN || _module == eUserOptionsModules.USROPT_MODULE.PREFERENCES || _module.ToString().StartsWith(eUserOptionsModules.USROPT_MODULE.PREFERENCES.ToString()))
                panelRight.Controls.Add(GetAdminLinksPanel(eUserOptionsModules.USROPT_MODULE.PREFERENCES, "myBlockPrefs", "choiceAdmin"));
            #endregion

            // Le bloc n'est pas ajouté s'il est vide. Permet d'éviter que le bloc de droite s'affiche tout seul à droite
            if (panelRight.Controls.Count > 0)
                pnlAdminCont.Controls.Add(panelRight);

            #endregion

            #region Blocs Options avancées
            if (_module == eUserOptionsModules.USROPT_MODULE.MAIN || _module == eUserOptionsModules.USROPT_MODULE.ADVANCED || _module.ToString().StartsWith(eUserOptionsModules.USROPT_MODULE.ADVANCED.ToString()))
                panelRight.Controls.Add(GetAdminLinksPanel(eUserOptionsModules.USROPT_MODULE.ADVANCED, "myBlockPrefs", "choiceAdmin"));
            #endregion

            // Le bloc n'est pas ajouté s'il est vide.
            if (panelRight.Controls.Count > 0)
                pnlAdminCont.Controls.Add(panelRight);

            // Ajout des deux groupes de blocs gauche et droit
            _pgContainer.Controls.Add(pnlAdminCont);

            return true;
        }

        /// <summary>
        /// Bloc utilisateur
        /// </summary>
        /// <returns></returns>
        public Panel BuildUserBlock()
        {
            Panel block = new Panel();
            block.ID = "blockUserProfile";

            HtmlGenericControl element, icon;
            HtmlImage image;
            String filepath = String.Empty;

            // Avatar
            element = new HtmlGenericControl("div");
            element.ID = "hAvatarFromMyEudonet";
            element.Attributes.Add("class", "hAvatarFromMyEudonet");
            element.Attributes.Add("ondragover", "UpFilDragOver(this, event);return false;");
            element.Attributes.Add("ondragleave", "UpFilDragLeave(this); return false;");
            element.Attributes.Add("ondrop", "UpFilDrop(this,event,null,null,1);return false;");
            element.Attributes.Add("onclick", "doGetImage(this, 'AVATAR')");
            element.Attributes.Add("userid", Pref.User.UserId.ToString());
            element.Attributes.Add("fid", Pref.User.UserId.ToString());
            element.Attributes.Add("tab", TableType.USER.GetHashCode().ToString());
            element.Attributes.Add("title", eResApp.GetRes(Pref, 6180));

            Boolean hasAvatar = eImageTools.GetAvatar(Pref, out filepath);

            if (hasAvatar)
            {
                image = new HtmlImage();
                image.Src = filepath;
                element.Controls.Add(image);
            }
            else
            {
                element.Controls.Add(this.GetEmptyImagePanel(0));
            }



            //if (!hasAvatar)
            //{
            //    image.Src = "";
            //    image.Width = 0;
            //    image.Height = 0;
            //    Panel panel = this.GetEmptyImagePanel(9);
            //    element.Controls.Add(panel);
            //}


            block.Controls.Add(element);

            Panel infos = new Panel();
            infos.ID = "infosWrapper";

            // Nom prénom
            element = new HtmlGenericControl("div");
            element.ID = "userDisplayName";
            element.InnerText = Pref.User.UserDisplayName;
            infos.Controls.Add(element);

            // Fonction
            element = new HtmlGenericControl("p");
            element.ID = "userFunction";
            element.InnerText = Pref.User.UserFunction;
            infos.Controls.Add(element);

            // Tél
            icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-phone3");
            element = new HtmlGenericControl("p");
            element.Attributes.Add("class", "phoneInfo");
            element.Controls.Add(icon);
            element.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 603), " : ", Pref.User.UserTel)));
            infos.Controls.Add(element);

            // Mobile
            icon = new HtmlGenericControl();
            icon.Attributes.Add("class", "icon-mobile3");
            element = new HtmlGenericControl("p");
            element.Attributes.Add("class", "phoneInfo");
            element.Controls.Add(icon);
            element.Controls.Add(new LiteralControl(String.Concat(eResApp.GetRes(Pref, 605), " : ", Pref.User.UserMobile)));
            infos.Controls.Add(element);

            // E-mail
            element = new HtmlGenericControl("p");
            element.InnerText = Pref.User.UserMail;
            infos.Controls.Add(element);

            block.Controls.Add(infos);

            #region Footer 
            Panel footer = new Panel();
            footer.ID = "blockUserFooter";

            element = new HtmlGenericControl();
            element.Attributes.Add("class", "icon-lock2");
            footer.Controls.Add(element);

            element = new HtmlGenericControl("p");
            element.InnerText = String.Concat(eResApp.GetRes(Pref, 7979), " ", Pref.User.UserLevel);
            footer.Controls.Add(element);

            element = new HtmlGenericControl("p");
            element.InnerText = String.Concat(eResApp.GetRes(Pref, 7980), " ", Pref.User.UserGroupName);
            footer.Controls.Add(element);

            block.Controls.Add(footer);
            #endregion

            return block;
        }



        /// <summary>
        /// Construit un block titre contenant dans une bande semi-grise et semi-transparent 
        /// </summary>
        /// <param name="title"></param>
        protected void BuildTitle(String title)
        {
            HtmlGenericControl pnlTitle = new HtmlGenericControl("div");
            pnlTitle.Attributes.Add("class", "adminModalMiddleTitle ");
            pnlTitle.InnerText = title;

            _pgContainer.Controls.Add(pnlTitle);
        }

        /// <summary>
        /// Construit un block contenant les boutons de validations/annulation
        /// </summary>
        /// <param name="jsValidateCall">js appélé au click sur le btn valider</param>
        /// <param name="jsCancelCall">js appélé au click sur le btn annuler</param>
        protected void BuildBtns(String jsValidateCall, String jsCancelCall)
        {
            Panel btnPart = new Panel();
            btnPart.CssClass = "adminBtnPart";
            eButtonCtrl btnCancel = new eButtonCtrl(eResApp.GetRes(Pref, 29), eButtonCtrl.ButtonType.GRAY, jsCancelCall); // Annuler
            eButtonCtrl btnValidate = new eButtonCtrl(eResApp.GetRes(Pref, 28), eButtonCtrl.ButtonType.GREEN, jsValidateCall); // Valider
            btnPart.Controls.Add(btnValidate);
            btnPart.Controls.Add(btnCancel);
            _pgContainer.Controls.Add(btnPart);
        }

        protected void BuildHiddenDiv(eConst.eFileType filetype)
        {
            // On ajoute un div fantôme "fileDiv" avec un attribut permettant à getCurrentView() de savoir sur quel type de page on se trouve
            // On considère l'admin comme un affichage de type Fiche en Modification
            HtmlGenericControl divFakeFileDiv = new HtmlGenericControl("div");
            divFakeFileDiv.Style.Add("visibility", "hidden");
            divFakeFileDiv.Style.Add("display", "none");
            divFakeFileDiv.ID = String.Concat("fileDiv_-1");
            divFakeFileDiv.Attributes.Add("ftrdr", filetype.GetHashCode().ToString());
            _pgContainer.Controls.Add(divFakeFileDiv);
        }


        /// <summary>
        /// Renvoie le bloc contenant les liens d'une section de l'admin
        /// </summary>
        /// <param name="module">Module parent pour lequel renvoyer les liens</param>
        /// <param name="blockClass">Classe CSS à appliquer sur le bloc (div)</param>
        /// <param name="listClass">Classe CSS à appliquer sur la liste des liens (ul)</param>
        /// <returns>Un objet Panel correspondant au bloc souhaité</returns>
        private Panel GetAdminLinksPanel(eUserOptionsModules.USROPT_MODULE module, string blockClass, string listClass)
        {
            string title = eUserOptionsModules.GetModuleLabel(module, Pref);
            string iconClass = String.Concat("icon-", eUserOptionsModules.GetModuleIcon(module));

            Panel pnlBlock = new Panel();
            Panel pnlTitle = new Panel();
            HtmlGenericControl pnlTitleIconSpan = new HtmlGenericControl("span");
            HtmlGenericControl pnlTitleTextSpan = new HtmlGenericControl("span");
            HtmlGenericControl ulLinkList = new HtmlGenericControl("ul");

            pnlBlock.CssClass = blockClass;
            pnlTitle.CssClass = "titlePart";
            pnlTitleTextSpan.InnerText = title;
            pnlTitleTextSpan.Attributes.Add("onclick", String.Concat("javascript:loadUserOption('", module.ToString(), "');"));
            pnlTitleIconSpan.Attributes.Add("class", iconClass);
            pnlTitleIconSpan.Attributes.Add("onclick", String.Concat("javascript:loadUserOption('", module.ToString(), "');"));
            ulLinkList.Attributes.Add("class", listClass);

            // On liste tous les modules d'admin dont le renderer est implémenté, et qui sont enfants du module qui nous intéresse
            List<eUserOptionsModules.USROPT_MODULE> childmodules = eUserOptionsModules.GetModuleChildren(module);

            // Puis on parcourt chaque module enfant implémenté pour afficher son lien
            foreach (eUserOptionsModules.USROPT_MODULE childmodule in eUserOptionsModules.GetOrderedModuleList(childmodules))
            {
                // BSE: #58 359 Masquer le lien du changement de mot de passe si on est en SSO ou ADFS
                if (childmodule == eUserOptionsModules.USROPT_MODULE.PREFERENCES_PASSWORD && (this.IsADFS || this.IsSSOApp))
                    continue;



                HtmlGenericControl li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "onClick");
                if (childmodule == eUserOptionsModules.USROPT_MODULE.ADVANCED_PLANNING)
                    li.Attributes.Add("onClick", String.Concat("javascript:showPlanningPrefDialog(0, ", Pref.UserId, ");"));
                else if (childmodule == eUserOptionsModules.USROPT_MODULE.PREFERENCES_THEME)
                    li.Attributes.Add("onClick", String.Concat("javascript:nsAdmin.openThemeChoice();"));
                else
                {
                    //BSE:#58 359 Alert si LDAP est activé 
                    string action = String.Concat("javascript:loadUserOption('", childmodule.ToString(), "');");

                    if (childmodule == eUserOptionsModules.USROPT_MODULE.PREFERENCES_PASSWORD && this.IsLDAPEnabled)
                        action = "javascript:ChangePasworAccessDenied();";

                    li.Attributes.Add("onClick", action);
                }



                li.InnerText = eUserOptionsModules.GetModuleLabel(childmodule, Pref);
                ulLinkList.Controls.Add(li);
            }

            pnlTitle.Controls.Add(pnlTitleIconSpan);
            pnlTitle.Controls.Add(pnlTitleTextSpan);
            pnlBlock.Controls.Add(pnlTitle);
            pnlBlock.Controls.Add(ulLinkList);

            return pnlBlock;
        }

        /// <summary>
        /// Retourne le mode des authentification activés pour la base
        /// </summary>
        private void GetAuthentificationOptions()
        {
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("SSOApplication")))
                this.IsSSOApp = (ConfigurationManager.AppSettings.Get("SSOApplication") == "1");


            this.IsADFS = eLibTools.GetServerConfig("ADFSApplication", "0") == "1";


            Dictionary<eLibConst.CONFIG_DEFAULT, String> dicConf = this.Pref.GetConfigDefault(new HashSet<eLibConst.CONFIG_DEFAULT> {
                        eLibConst.CONFIG_DEFAULT.LDAPEnabled });
            this.IsLDAPEnabled = dicConf[eLibConst.CONFIG_DEFAULT.LDAPEnabled] == "1";
        }

    }
}