using Com.Eudonet.Internal;
using EudoQuery;
using System.Web.UI.WebControls;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Wrapper admin du renderer de la grille
    /// </summary>
    public class eAdminXrmGridRenderer : eAdminModuleRenderer
    {
        eRenderer _gridRenderer;
        int _gridId;

        private eAdminXrmGridRenderer(ePref pref, int gridId, int nWidth, int nHeight)
            : base(pref)
        {
            _gridId = gridId;
            _width = nWidth;
            _height = nHeight;

        }

        /// <summary>
        /// Instantiation de renderer d'administration des grilles
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="gridId"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <returns></returns>
        public static eAdminXrmGridRenderer CreateAdminXrmGridRenderer(ePref pref, int gridId, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminXrmGridRenderer rdr = new eAdminXrmGridRenderer(pref, gridId, nWidth, nHeight);

            rdr.Generate();

            return rdr;
        }


        /// <summary>
        /// Initialisation du renderer des grilles
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            _gridRenderer = eRendererFactory.CreateXrmGridRenderer(Pref, _gridId, _width, _height);
            return true;
        }

        /// <summary>
        /// Génération de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            if (!_gridRenderer.Generate())
            {
                SetError(_gridRenderer.ErrorNumber, _gridRenderer.ErrorMsg, _gridRenderer.InnerException);
                return false;

            }

            return true;
        }

        /// <summary>
        /// Recuperation du container de la grille
        /// </summary>
        /// <returns></returns>
        public override Panel GetContents()
        {
            return _gridRenderer.PgContainer;
        }
    }
}