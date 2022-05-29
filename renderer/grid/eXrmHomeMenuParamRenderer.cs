using Com.Eudonet.Internal;
using System;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le menu de paramétrage d'une instance de la page d'accueil
    /// TODO Refactorisation pour une encapsulation dans un objet spécialisé
    /// </summary>
    public class eXrmHomeMenuParamRenderer : eRenderer
    {
        /// <summary>Id de la page d'accueil</summary>
        private eFile _file;
        private int _fileId = 0;
        eXrmWidgetContext _context;

        /// <summary>
        /// Crée une instance de renderer
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nTab">The n tab.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="nWidth">Width of the n.</param>
        /// <param name="nHeight">Height of the n.</param>
        /// <param name="context">The context.</param>
        public eXrmHomeMenuParamRenderer(ePref pref, int nTab, int id, int nWidth, int nHeight, eXrmWidgetContext context)
        {
            Pref = pref;
            _width = nWidth;
            _height = nHeight;
            _tab = nTab;
            _fileId = id;
            _context = context;
        }

        /// <summary>
        /// On récupère l'objet metier
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {

            if (_fileId > 0)
                _file = eFileMain.CreateMainFile(Pref, _tab, _fileId, -2);

            return true;
        }

        /// <summary>
        /// Lance la construction du menu complet
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _pgContainer.Controls.Add(BuildVerticalMenuBar());
            _pgContainer.Controls.Add(BuildGerard()); // Gerard pour le contenu de menu ! 

            return true;
        }

        /// <summary>
        /// Construit la barre vertical de menu de droite
        /// Encapsuler dans le pattern Builder
        /// </summary>
        /// <returns></returns>
        private Control BuildVerticalMenuBar()
        {
            Panel menuBar = new Panel();
            menuBar.ID = "menuBar";
            menuBar.CssClass = "menuBar";
            menuBar.Attributes.Add("pinned", "1");
            menuBar.Attributes.Add("adminmode", "1");
            menuBar.Style.Add("cursor", "default");

            HiddenField input = new HiddenField();
            input.Value = "0";

            menuBar.Controls.Add(input);

            return menuBar;
        }

        /// <summary>
        /// Créér le menu de droit
        /// </summary>
        /// <returns></returns>
        private Control BuildGerard()
        {
            Panel gerard = new Panel();
            gerard.ID = "Gerard";
            gerard.CssClass = string.Concat("Gerard gerardWidth no-selection ", eTools.GetClassNameFontSize(_ePref));

            gerard.Controls.Add(BuildUserProfile());
            gerard.Controls.Add(BuildNavTabs());
            gerard.Controls.Add(BuildSlidingArrow());


            // Il y a une relation d'exlusivité sur la visibilité (1 à la fois)// par défaut la première table        
            gerard.Controls.Add(eXrmWidgetFactory.GetMenuContentRenderer(Pref, _tab, _file, true, _context).BuildMenu());
            gerard.Controls.Add(eXrmWidgetFactory.GetMenuWidgetParamRenderer(Pref, 0, false, _context).BuildMenu());
            gerard.Controls.Add(eXrmWidgetFactory.GetMenuParamRenderer(Pref, _tab, _file, false, _context).BuildMenu());

            gerard.Controls.Add(BuildLogoutLink());

            return gerard;
        }


        /// <summary>
        /// Construit la partie profil utilisateur : image , nom
        /// </summary>
        /// <returns></returns>
        private Control BuildUserProfile()
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "encartProfil");

            // li image
            HtmlGenericControl li = new HtmlGenericControl("li");
            Image img = new Image();
            img.ID = "UserAvatar";
            img.CssClass = "hAvatar";
            img.Style.Add("max-height", "30px");
            img.Style.Add("max-width", "30px");

            img.ImageUrl = eImageTools.GetAvatar(Pref);
            img.Attributes.Add("onclick", "doGetImage(this, 'AVATAR');");
            img.Attributes.Add("userid", Pref.UserId.ToString());
            img.Attributes.Add("fid", Pref.UserId.ToString());
            img.Attributes.Add("tab", "101000");


            li.Controls.Add(img);
            ul.Controls.Add(li);

            // li bonjour
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "helloMnu");
            li.InnerHtml = String.Concat(eResApp.GetRes(Pref, 6181), ","); // Bonjour,
            ul.Controls.Add(li);

            // li nickname
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "nickMnu");
            li.Attributes.Add("href", "#");
            li.Attributes.Add("title", String.Format("Userid : {0} \nLogin: {1} \nGroup: {2} \nMail: {3}", Pref.UserId, Pref.User.UserLogin, Pref.User.UserGroupName, Pref.User.UserMail));

            li.InnerHtml = Pref.User.UserLogin;
            ul.Controls.Add(li);

            return ul;
        }

        /// <summary>
        /// Construit le menu  navbatab 
        /// </summary>
        /// <returns></returns>
        private Control BuildNavTabs()
        {
            HtmlGenericControl navTabs = new HtmlGenericControl("ul");
            navTabs.ID = "navTabs";
            navTabs.Attributes.Add("did", _tab.ToString());


            navTabs.Controls.Add(BuildNavTabItem(eResApp.GetRes(Pref, 2735), "paramTabPicto1", "paramTab1", "icon-mise-en-page"));
            navTabs.Controls.Add(BuildNavTabItem(eResApp.GetRes(Pref, 2736), "paramTabPicto2", "paramTab2", "icon-parametres"));
            navTabs.Controls.Add(BuildNavTabItem(eResApp.GetRes(Pref, 2737), "paramTabPicto3", "paramTab3", "icon-param-onglet"));

            return navTabs;
        }

        /// <summary>
        /// Construit un navbartab du menu
        /// </summary>
        /// <param name="title">titre afficher au survol</param>
        /// <param name="navTabItemId">id de la navTab</param>
        /// <param name="contentTabId">contenu cible à rendre visible</param>
        /// <param name="iconName">icon pour la navtab</param>
        /// <returns></returns>
        private static Control BuildNavTabItem(string title, string navTabItemId, string contentTabId, string iconName)
        {
            HtmlGenericControl navTabItem = new HtmlGenericControl("li");
            navTabItem.Attributes.Add("class", "navIcon");
            navTabItem.Attributes.Add("title", title);

            HtmlGenericControl span = new HtmlGenericControl("span");
            span.ID = navTabItemId;
            span.Attributes.Add("onclick", "oAdminGridMenu.showBlock('" + contentTabId + "')");
            span.Attributes.Add("class", iconName + " paramTabPicto");

            navTabItem.Controls.Add(span);

            return navTabItem;
        }

        /// <summary>
        /// Construit une flèche vers le haut
        /// </summary>
        /// <returns></returns>
        private Control BuildSlidingArrow()
        {
            Panel slidingArrow = new Panel();
            slidingArrow.ID = "slidingArrow";

            // On se positionne par défaut sur navTab des widgets

            //todo : la gestion de ce menu (grid) est différente des autres menu. Il faudrait revenir dans le standard.
            // il faudrait aussi éviter des mettre des style inlines...
            int nFontSize = 8;
            int.TryParse(eTools.GetUserFontSize(_ePref), out nFontSize);

            if (nFontSize < 14)
                slidingArrow.Style.Add("left", "25px");
            else
                slidingArrow.Style.Add("left", "44px");

            return slidingArrow;
        }


        /// <summary>
        /// Construit le bouton de déconnexion
        /// </summary>
        /// <returns></returns>
        private Control BuildLogoutLink()
        {
            HtmlGenericControl footer = new HtmlGenericControl("footer");
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "hLink");
            footer.Controls.Add(ul);

            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("onclick", "doDisco();");
            li.Attributes.Add("class", "decBtn");
            li.InnerHtml = "<span class='icon-logout'></span>" + eResApp.GetRes(Pref, 5008); // "Déconnexion";
            ul.Controls.Add(li);

            return footer;
        }

        protected override bool End()
        {
            return true;
        }
    }
}