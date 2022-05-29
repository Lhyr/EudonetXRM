using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Objet permettant d'afficher le mode liste des pages d'accueil XRM en utilisant l'objet standard de rendu de mode liste
    /// </summary>
    public class eAdminXrmHomePageListRenderer : eAdminModuleRenderer
    {
        private int _page;
        private int _rows;

        /// <summary>
        /// Constructeur du mode liste des nouvelles pages XRM
        /// </summary>
        public eAdminXrmHomePageListRenderer(ePref pref, int nPage, int nRows, int nWidth, int nHeight)
            : base(pref)
        {
            Pref = pref;
            _tab = (int)TableType.XRMHOMEPAGE;
            _width = nWidth;
            _height = nHeight;
            _page = nPage;
            _rows = nRows;
        }

        /// <summary>
        /// On expose un eAdmin renderer et non pas un eAdminHomepagesRenderer
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="nPage">The n page.</param>
        /// <param name="nRows">The n rows.</param>
        /// <param name="nWidth">Width of the n.</param>
        /// <param name="nHeight">Height of the n.</param>
        /// <returns></returns>
        /// <exception cref="EudoAdminInvalidRightException"></exception>
        public static eAdminModuleRenderer CreateAdminXrmHomePageListRenderer(ePref pref, int nPage, int nRows, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < (int)UserLevel.LEV_USR_ADMIN)
                throw new EudoAdminInvalidRightException();

            return new eAdminXrmHomePageListRenderer(pref, nPage, nRows, nWidth, nHeight);
        }

        /// <summary>
        /// Initialisation du mode list
        /// </summary>
        /// <returns>true si le contenu a été généré avec succès</returns>
        protected override bool Init()
        {
            if (!base.Init())
                return false;

            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminHome))
                return false;


            _pgContainer.ID = "list-container";
            _pgContainer.CssClass = "list-container";

            return true;
        }

        /// <summary>
        /// On ajoute le rendu de la liste à celui de  la page admin
        /// </summary>
        /// <returns></returns>
        protected override bool End()
        {
            if (!base.End())
                return false;

            _pgContainer.Controls.Add(GetFullList());

            return true;
        }

        /// <summary>
        /// Construit la liste
        /// </summary>
        /// <returns></returns>
        private Control GetFullList()
        {
            return eRendererFactory.CreateFullMainListRenderer(Pref, _tab, _page, _rows, _height, _width).PgContainer;
        }
    }
}