using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Navbar spécifique au paramètrage de la page d'accueil
    /// </summary>
    public class eAdminNavBarHomePageRenderer : eAdminNavBarRenderer
    {
        private eFile _file;
        private int _pageId;
        private int _gridId;
        private string _gridLabel;


        private eAdminNavBarHomePageRenderer(ePref P, Int32 nTab, int pageId, int gridId, string gridLabel)
            : base(P, nTab, 0, eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES, 0, String.Empty, String.Empty)
        {
            _pageId = pageId;
            _gridId = gridId;
            _gridLabel = gridLabel;
        }

        /// <summary>
        /// Init de la navabar
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            _file = eFileMain.CreateMainFile(Pref, _tab, _pageId, -2);
            return true;
        }


        /// <summary>
        /// Lien pour liste des pages d'accueil XRM
        /// </summary>
        /// <param name="currentModule"></param>
        /// <returns></returns>
        protected override HtmlGenericControl GetNavBarLinkMenu(eUserOptionsModules.USROPT_MODULE currentModule)
        {
            if (currentModule != eUserOptionsModules.USROPT_MODULE.ADMIN_HOME_XRM_HOMEPAGES)
                return base.GetNavBarLinkMenu(currentModule);


            return GetNavBarLink(
                       eUserOptionsModules.GetModuleLabel(currentModule, Pref),
                       eUserOptionsModules.GetModuleIcon(currentModule),
                       eUserOptionsModules.GetModuleTooltip(currentModule, Pref),
                       String.Concat("nsAdmin.loadAdminModule('", currentModule, "');"),
                       "navEntry navModuleWithMenu",
                       "navTitle",
                       bWidthSubMenu: false
               );
        }

        /// <summary>
        /// Ajoute des liens specifique au fil d'ariane
        /// </summary>
        /// <param name="ulNavbarButtonUL"></param>
        protected override void AddSpecificNavBarLinks(HtmlGenericControl ulNavbarButtonUL)
        {
            bool pageEdit = _gridId == 0;

            // Libellé de la page d'accueil
            AddNavBarLinkItem(ulNavbarButtonUL, pageEdit,
                  delegate (Panel panelLabel, HtmlGenericControl textLabel, HtmlInputText textInput)
                  {
                      eFieldRecord pageTitle = _file.GetField((int)XrmHomePageField.Title);
                      panelLabel.Attributes.Add("class", "navTitle" + (pageEdit ? " navTitleSelected" : ""));


                      textLabel.InnerText = pageTitle.DisplayValue;

                      if (!pageEdit)
                      {
                          // TODO Refacto
                          panelLabel.Attributes.Add("onclick", String.Concat("oGridController.page.load(" + _pageId+");"));
                      }
                      else
                      {
                          panelLabel.Attributes.Add("onclick", String.Concat("oGridController.config.rename(this);")); // TODO
                          panelLabel.ToolTip = "Cliquez pour modifier le titre de la page d'accueil";
                          textInput.Attributes.Add("did", ((int)XrmHomePageField.Title).ToString());
                          textInput.Attributes.Add("fid", _file.FileId.ToString());
                          textInput.Attributes.Add("tab", ((int)TableType.XRMHOMEPAGE).ToString());
                          textInput.Value = pageTitle.DisplayValue;
                          textInput.Attributes.Add("onblur", "oGridController.config.blur(this);");
                      }                   
                  });

            // Libellé de la grille si sélectionné
            if (_gridId > 0)
            {
                AddNavBarLinkItem(ulNavbarButtonUL, true,
                      delegate (Panel panelLabel, HtmlGenericControl textLabel, HtmlInputText textInput)
                      {
                          panelLabel.Attributes.Add("class", "navTitle navTitleSelected");
                          panelLabel.Attributes.Add("onclick", String.Concat("oGridController.config.rename(this);")); // TODO
                          panelLabel.ToolTip = "Cliquez pour modifier le libellé de la grille";

                          textLabel.InnerText = _gridLabel;

                          textInput.Attributes.Add("did", ((int)XrmGridField.Title).ToString());
                          textInput.Attributes.Add("fid", _gridId.ToString());
                          textInput.Attributes.Add("tab", ((int)TableType.XRMGRID).ToString());
                          textInput.Value = _gridLabel;
                          textInput.Attributes.Add("onblur", "oGridController.config.blur(this);");

                      });
            }
        }

        public static eAdminNavBarHomePageRenderer CreateAdminNavBarRecordRenderer(ePref P, Int32 nTab, int pageId, int gridId, string gridLabel)
        {

            eAdminNavBarHomePageRenderer enavbar = new eAdminNavBarHomePageRenderer(P, nTab, pageId, gridId, gridLabel);

            return enavbar;
        }
    }
}