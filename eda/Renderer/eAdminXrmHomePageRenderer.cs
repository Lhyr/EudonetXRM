using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Internal;
using EudoQuery;

using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Wrapper admin du renderer de la grille
    /// </summary>
    public class eAdminXrmHomePageRenderer : eAdminModuleRenderer
    {

        int _fileId;

        private eAdminXrmHomePageRenderer(ePref pref, int fileId, int nWidth, int nHeight)
            : base(pref)
        {
            _tab = (int)TableType.XRMHOMEPAGE;
            _fileId = fileId;
            _width = nWidth;
            _height = nHeight;

        }

        /// <summary>
        /// Instantiation de renderer d'administration de la page d'accueil
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="pageId"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <returns></returns>
        public static eAdminXrmHomePageRenderer CreateAdminXrmHomePageRenderer(ePref pref, int pageId, int nWidth, int nHeight)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            return new eAdminXrmHomePageRenderer(pref, pageId, nWidth, nHeight);
        }


        /// <summary>
        /// Initialisation du renderer des pages d'accueil
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminHome);

        }

        /// <summary>
        /// Génération de la grille
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {

            Panel fileDiv = new Panel();
            fileDiv.ID = "fileDiv_" + _tab;
            fileDiv.CssClass = "fileDiv";
            fileDiv.Attributes.Add("fid", _fileId.ToString());
            fileDiv.Attributes.Add("did", _tab.ToString());

            Panel divFilePart1 = new Panel();
            divFilePart1.ID = "divFilePart1";
            fileDiv.Controls.Add(divFilePart1);

            HtmlGenericControl header = new HtmlGenericControl("header");
            header.Controls.Add(eAdminWebTabNavBarForXrmHomePageRenderer.GetAdminWebTabNavBar(Pref, _tab, _fileId).PgContainer);
            divFilePart1.Controls.Add(header);

            _pgContainer.Controls.Add(fileDiv);

            return true;
        }


    }
}