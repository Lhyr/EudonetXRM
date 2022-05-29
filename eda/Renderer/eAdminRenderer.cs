using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// classe parente qui permet de vérifier le niveau de l'utilisateur avant d'accéder aux classes de renderer de l'administration
    /// </summary>
    public class eAdminRenderer : eRenderer
    {
        /// <summary>
        /// type d'élément drag n dropable
        /// </summary>
        public enum DragType
        {
            OTHER = 0,
            FIELD = 1,
            WEB_TAB_MENU = 2,
            WEB_SIGNET = 3,
            HEAD_LINK = 4,
            WEB_TAB = 5,
            GRID_MENU = 6,
            GRID = 7,
            WEB_TAB_LIST = 8,
            BKM_GRID = 9
        }


        /// <summary>
        /// Initialisation des params
        /// </summary>
        /// <returns></returns>
        protected override bool Init()
        {
            if (Pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            if (!eFeaturesManager.IsFeatureAvailable(Pref, eConst.XrmFeature.Admin))
                throw new EudoAdminInvalidRightException();

            return base.Init();
        }
    }
}