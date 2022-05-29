using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminRGPDTreatmentLogListRenderer : eAdminRenderer
    {

        eFullMainListRenderer _eTreatmentLogListRenderer = null;

        bool _bFullRenderer;

        private int _nPage = 1;
        private int _nRows = 1;



        /// <summary>
        /// Constructeur par défaut. Aucun paramètres particuliers, le seul paramètre étant epref
        /// </summary>
        /// <param name="pref">epref utsers</param>
        /// <param name="bFull">Renderer complet : group + user + conteneur global. Sinon, retourne seulement le bloc user</param>
        private eAdminRGPDTreatmentLogListRenderer(ePref pref, bool bFull, int nPage, int nRows, int height, int width)
        {
            Pref = pref;
            _bFullRenderer = bFull;
            _width = width;
            _height = height;
            _nPage = nPage;
            _nRows = nRows;

            _rType = RENDERERTYPE.FullMainList;
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
        public static eAdminRGPDTreatmentLogListRenderer CreateAdminRGPDTreatmentLogListRenderer(ePref pref, bool bFullRend, int nPage, int nRows, int height, int width)
        {

            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminRGPDTreatmentLogListRenderer lst = new eAdminRGPDTreatmentLogListRenderer(pref, bFullRend, nPage, nRows, height, width);

            lst.Generate();

            return lst;
        }


        protected override bool Init()
        {
            if (base.Init())
            {
                return eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.AdminRGPD);
            }
            return false;
        }


        /// <summary>
        /// Construction des objets HTML
        /// Construit la liste des user et les groupes si besoins
        /// </summary>
        /// <returns></returns>
        protected override bool Build()
        {
            _eTreatmentLogListRenderer = (eFullMainListRenderer)eRendererFactory.CreateFullMainListRenderer(Pref, (int)TableType.RGPDTREATMENTSLOGS, _nPage, _nRows, _height, _width);
            _eTreatmentLogListRenderer.Generate();

            if (_eTreatmentLogListRenderer.ErrorMsg.Length > 0)
            {
                _sErrorMsg = _eTreatmentLogListRenderer.ErrorMsg;
                _nErrorNumber = _eTreatmentLogListRenderer.ErrorNumber;
                _eException = _eTreatmentLogListRenderer.InnerException;
                return false;
            }

            DicContent = _eTreatmentLogListRenderer.DicContent;

            return true;
        }

    }
}