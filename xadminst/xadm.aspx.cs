using Com.Eudonet.Internal;
using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.xadminst
{
    public partial class xadm : eEudoPage
    {
        public string strPageTitle = String.Empty;
        public string strPageBreadcrumb = String.Empty;
        public string strPageContents = String.Empty;

        protected void Page_Load(object sender, EventArgs e)
        {

            PageRegisters.RegisterFromRoot = true;

            PageRegisters.AddCss("eudoFont");
            PageRegisters.AddCss("eUserOptions");
            PageRegisters.AddCss("eButtons");

            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eModalDialog");

            // Chargement des paramètres
            // Id de la page à charger
            eUserOptionsModules.USROPT_MODULE module = eUserOptionsModules.USROPT_MODULE.MAIN;

            if (_allKeys.Contains("pageId") && !String.IsNullOrEmpty(Request.Form["pageId"]))
                Enum.TryParse(Request.Form["pageId"], out module);

            // Affichage du titre
            strPageTitle = GetPageTitle(module);

            // Affichage du chemin de fer/fil d'Ariane/breadcrumb/whatever it's called
            strPageBreadcrumb = GetPageBreadcrumb(module);

            // Chargement du contenu demandé
            eUserOptionsRenderer er = eUserOptionsModules.GetModuleRenderer(module, _pref);
            if (er != null)
                strPageContents = GetResultHTML(er.PgContainer);
        }

        /// <summary>
        /// Renvoie le contenu HTML du contrôle passé en paramètre
        /// Similaire à eEudoPage.RenderResultHTML, mais renvoie le code généré plutôt que de l'ajouter directement à la page
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public string GetResultHTML(Control control) {
            string strResult = String.Empty;

            if (control != null) {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                HtmlTextWriter tw = new HtmlTextWriter(sw);
                control.RenderControl(tw);
                strResult = sb.ToString();
            }

            return strResult;
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Renvoie le code HTML affichant le titre de la page en cours
        /// </summary>
        /// <param name="module">Module actuellement affiché</param>
        /// <returns></returns>
        private string GetPageTitle(eUserOptionsModules.USROPT_MODULE module)
        {
            Panel pnlTitle = new Panel();
            Panel pnlTitleIcon = new Panel();
            HtmlGenericControl pnlTitleText = new HtmlGenericControl("div");
            
            pnlTitle.CssClass = "adminModalTitle";
            pnlTitleIcon.CssClass = String.Concat("icon-", eUserOptionsModules.GetModuleIcon(module));
            pnlTitleText.InnerText = eUserOptionsModules.GetModuleLabel(eUserOptionsModules.GetModuleParent(module, false), _pref);

            pnlTitle.Controls.Add(pnlTitleIcon);
            pnlTitle.Controls.Add(pnlTitleText);

            return GetResultHTML(pnlTitle);
        }

        /// <summary>
        /// Renvoie le chemin de fer/fil d'Ariane/breadcrumb/whatever its called de la page en cours
        /// </summary>
        /// <param name="module">Module actuellement affiché</param>
        /// <returns></returns>
        private string GetPageBreadcrumb(eUserOptionsModules.USROPT_MODULE module)
        {
            // Pas de breadcrumb sur la page principale
            if (module == eUserOptionsModules.USROPT_MODULE.MAIN || module == eUserOptionsModules.USROPT_MODULE.ADMIN || module == eUserOptionsModules.USROPT_MODULE.UNDEFINED)
                return String.Empty;

            Panel pnlBreadcrumbs = new Panel();
            string moduleName = module.ToString();
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "adminBreadCrumbs");
            pnlBreadcrumbs.Controls.Add(ul);

            // On ajoute les contrôles en ordre inversé, d'abord le module actuel, puis ses parents, jusqu'à ce que l'on remonte jusqu'au dernier parent
            // On utilise pour cela un tableau de Control qu'on parcourera en sens inverse
            List<Control> modules = new List<Control>();
            modules.Add(GetPageBreadcrumbLink(eUserOptionsModules.GetModuleLabel(module, _pref), module.ToString()));
            eUserOptionsModules.USROPT_MODULE parentModule = eUserOptionsModules.GetModuleParent(module, false);
            while (parentModule != module) {
                modules.Add(GetPageBreadcrumbLink(eUserOptionsModules.GetModuleLabel(parentModule, _pref), parentModule.ToString()));
                module = parentModule;
                parentModule = eUserOptionsModules.GetModuleParent(module, false);
            }
            for (int i = modules.Count - 1; i > -1; i--)
            {
                ul.Controls.Add(modules[i]);
                if (i > 0)
                    ul.Controls.Add(GetPageBreadcrumbSeparator());
            }

            return GetResultHTML(pnlBreadcrumbs);
        }

        private HtmlGenericControl GetPageBreadcrumbLink(string label, string href)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            if (!href.Contains("_"))
            {

                li.Attributes.Add("class", "ariane");
                li.Attributes.Add("onClick", String.Concat("javascript:loadUserOption('", href, "');"));
            }
            else
            {
                li.Attributes.Add("class", "no-ariane");
            }

            li.InnerText = label;
            return li;
        }

        private HtmlGenericControl GetPageBreadcrumbSeparator()
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.InnerHtml = "&nbsp;&gt;&nbsp;";
            return li;
        }
    }
}