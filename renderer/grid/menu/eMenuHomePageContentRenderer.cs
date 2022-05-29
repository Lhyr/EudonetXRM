using Com.Eudonet.Internal;
using Com.Eudonet.Xrm.eda;
using EudoQuery;
using System.Web.UI;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Objet permettant d'afficher le contenu de la page d'accueil
    /// </summary>
    public class eMenuHomePageContentRenderer : eAbstractMenuRenderer
    {

        /// <summary>
        /// TODO Besoin d'un objet metier de la table
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="isVisible">if set to <c>true</c> [is visible].</param>
        /// <param name="file">The file.</param>
        /// <param name="context">The context.</param>
        public eMenuHomePageContentRenderer(ePref pref, bool isVisible, eFile file, eXrmWidgetContext context) : base(isVisible, file, context)
        {
            Pref = pref;
            _tab = (int)TableType.XRMHOMEPAGE;
        }

        /// <summary>
        ///  Construit le menu
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            RenderMenu("paramTab1", eResApp.GetRes(Pref, 8157), "");
            return true;
        }

        /// <summary>
        /// On fait le rendu des items du menu dans le container
        /// </summary>
        /// <param name="menuContainer">The menu container.</param>
        protected override void RenderContentMenu(Panel menuContainer)
        {
            menuContainer.Controls.Add(BuildAddGridLink());
        }

        /// <summary>
        /// Affiche les options de la barre d'outil du widget
        /// </summary>
        /// <returns></returns>
        private Control BuildAddGridLink()
        {

            Panel panelContent = eAdminRendererFactory.CreateAdminWebTabRenderer(Pref).PanelContent;
            panelContent.Attributes.Add("data-active", "1");
            panelContent.Attributes.Add("eactive", "1");

            return panelContent;
        }
    }
}