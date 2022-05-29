using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// Renderer de l'administration des user
    ///  -> Appel eFullMainListRenderer pour l'heritage sur eFullListUserRenderer
    /// </summary>
    public class eAdminUsersListRenderer : eAdminRenderer
    {

        eFullMainListRenderer _eUserListRenderer = null;

        bool _bFullRenderer;

        private int _nPage = 1;
        private int _nRows = 1;



        /// <summary>
        /// Constructeur par défaut. Aucun paramètres particuliers, le seul paramètre étant epref
        /// </summary>
        /// <param name="pref">epref utsers</param>
        /// <param name="bFull">Renderer complet : group + user + conteneur global. Sinon, retourne seulement le bloc user</param>
        private eAdminUsersListRenderer(ePref pref, bool bFull, int nPage, int nRows, int height, int width)
        {
            Pref = pref;
            _bFullRenderer = bFull;
            _width = width;
            _height = height;
            _nPage = nPage;
            _nRows = nRows;

            _rType = RENDERERTYPE.AdminUserList;
        }


        /// <summary>
        /// Construction du renderer mode liste de l'admin des users
        /// </summary>
        /// <param name="pref"></param>
        ///<param name="nPage">Numéro de page</param>
        ///<param name="bFullRend">Retourne l'intégralité de l'admin user : group + user + conteneur global. Sinon, retourne seulement le bloc user</param>
        ///<param name="nRows">Nombre de ligne par page</param>
        ///<param name="height">Hauteur de pa fenetere</param>
        ///<param name="width">Largeur</param>
        /// <returns></returns>
        public static eAdminUsersListRenderer CreateAdminUsersListRenderer(ePref pref, bool bFullRend, int nPage, int nRows, int height, int width)
        {

            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminUsersListRenderer lst = new eAdminUsersListRenderer(pref, bFullRend, nPage, nRows, height, width);

            lst.Generate();

            return lst;
        }


        protected override bool Init()
        {
            if (base.Init())
            {
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminAccess_GroupsAndUsers);
            }
            return false;
        }


        /// <summary>
        /// Construction des objets HTML
        /// Construit la liste des user et les groupes si besoins
        /// </summary>
        /// <returns></returns>
        protected override Boolean Build()
        {

            _eUserListRenderer = (eFullMainListUserRenderer)eRendererFactory.CreateFullMainListRenderer(Pref, (int)TableType.USER, _nPage, _nRows, _height, _width);


            if (_eUserListRenderer.ErrorMsg.Length > 0)
            {
                _sErrorMsg = _eUserListRenderer.ErrorMsg;
                _nErrorNumber = _eUserListRenderer.ErrorNumber;
                _eException = _eUserListRenderer.InnerException;
                return false;
            }

            DicContent = _eUserListRenderer.DicContent;



            return true;
        }



    }
}