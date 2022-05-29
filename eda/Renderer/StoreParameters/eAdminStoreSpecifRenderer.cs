using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Renderer du module d'administration d'une extension issue du Store (avec paramètres gérés par une spécif)
    /// </summary>
    public class eAdminStoreSpecifRenderer : eAdminStoreFileRenderer
    {


        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public eAdminStoreSpecifRenderer(ePref pref, eAdminExtension extension)
            : base(pref, extension)
        {

        }
        /// <summary>
        /// Rendu du paramétrage d'une extension spécifique
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static eAdminStoreSpecifRenderer CreateAdminStoreSpecifRenderer(ePref pref, eAdminExtension ext)
        {
            if (pref.User.UserLevel < UserLevel.LEV_USR_ADMIN.GetHashCode())
                throw new EudoAdminInvalidRightException();

            eAdminStoreSpecifRenderer rdr = new eAdminStoreSpecifRenderer(pref, ext);

            rdr.Generate();
            return rdr;
        }


 


    }
}