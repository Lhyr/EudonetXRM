using System;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer de l'administration des onglets
    /// </summary>
    public class eAdminPortalRenderer : eAdminModuleRenderer
    {
        Panel _pnlContents;
        HtmlGenericControl _pnlSubTitle;
        ExtendedDictionary<Int32, String> _tabList;
        eudoDAL _dal;

        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminPortalRenderer(ePref pref)
            : base(pref)
        {
            Pref = pref;
        }


        public static eAdminPortalRenderer CreateAdminPortalRenderer(ePref pref)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminPortalRenderer(pref);
        }

        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            return base.Init();
        }

        /// <summary>
        /// Génération du contenu
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Build()
        {

            Panel pnlAdminCont = new Panel();
            pnlAdminCont.CssClass = "admin_cont";
            pnlAdminCont.ID = "adminHome";


            CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_GENERAL, "otherBlock");
            CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_ACCESS, "otherBlock");
            CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_TABS, "otherBlock");
            CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_HOME, "otherBlock");
            CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS, "otherBlock");
            if (eUserOptionsModules.ModuleExists(eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD))
                CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_DASHBOARD, "otherBlock");


            if (_ePref.User.UserLevel >= (int)UserLevel.LEV_USR_SUPERADMIN)
                CreatePortalTilePanel(pnlAdminCont, eUserOptionsModules.USROPT_MODULE.ADMIN_ORM, "otherBlock");

            // Ajout des groupes de blocs
            _pgContainer.Controls.Add(pnlAdminCont);

            return true;
        }

        /// <summary>
        /// Renvoie le bloc contenant les tuiles principales de l'admin (page de garde)
        /// </summary>
        /// <param name="module">Module pour lequel renvoyer la tuile</param>
        /// <param name="blockClass">Classe CSS à appliquer sur le bloc (div)</param>
        /// <returns>Un objet Panel correspondant au bloc souhaité</returns>
        private void CreatePortalTilePanel(Panel panelContainer, eUserOptionsModules.USROPT_MODULE module, string blockClass)
        {
            eConst.XrmFeature feature = eUserOptionsModules.GetModuleFeature(module);

            if (eFeaturesManager.IsFeatureAvailable(Pref, feature))
            {
                string title = eUserOptionsModules.GetModuleLabel(module, Pref);
                string subTitle = eUserOptionsModules.GetModuleSubLabel(module, Pref);
                string tooltip = eUserOptionsModules.GetModuleTooltip(module, Pref);
                string iconClass = String.Concat("icon-", eUserOptionsModules.GetModuleIcon(module));

                Panel pnlBlock = new Panel();
                Panel pnlTitle = new Panel();
                HtmlGenericControl pnlTitleIconSpan = new HtmlGenericControl("span");
                HtmlGenericControl pnlTitleTextSpan = new HtmlGenericControl("span");
                HtmlGenericControl pnlTitleSubTextSpan = new HtmlGenericControl("span");

                // Lien du module à afficher en fonction du type de module en question
                // A personnaliser avec un switch (module) si nécessaire, si besoin de charger un module qui ne peut pas passer par loadAdminModule
                string link = String.Concat("javascript:nsAdmin.loadAdminModule('", module.ToString(), "');");

                pnlBlock.CssClass = blockClass;
                pnlBlock.ToolTip = tooltip;
                pnlTitle.CssClass = "tilePart";
                pnlTitleTextSpan.InnerText = title;
                pnlTitleTextSpan.Attributes.Add("onclick", link);
                pnlTitleTextSpan.Attributes.Add("class", "adminTileText");
                pnlTitleIconSpan.Attributes.Add("onclick", link);
                pnlTitleIconSpan.Attributes.Add("class", String.Concat(iconClass, " adminTileIcon"));
                pnlTitleSubTextSpan.Attributes.Add("onclick", link);
                pnlTitleSubTextSpan.Attributes.Add("class", "adminTileSubText");
                pnlTitleSubTextSpan.InnerText = subTitle;

                pnlTitle.Controls.Add(pnlTitleIconSpan);
                pnlTitle.Controls.Add(pnlTitleTextSpan);
                pnlTitle.Controls.Add(pnlTitleSubTextSpan);
                pnlBlock.Controls.Add(pnlTitle);

                panelContainer.Controls.Add(pnlBlock);
            }


        }

    }
}