using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// paramétrage de l'extension du paramétrage
    /// </summary>
    public class eAdminStoreSnapshotRenderer : eAdminStoreFileRenderer
    {


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreSnapshotRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }

        /// <summary>
        /// statique qui génère l'extenison de paramétrage
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreSnapshotRenderer CreateAdminStoreSnapshotRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreSnapshotRenderer rdr = new eAdminStoreSnapshotRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }


    }
}