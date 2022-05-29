using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    /// <summary>
    /// Hérite de eAdminDescManager sans le test sur les droits Admin
    /// Utilisé pour l'enregistrement des préférences utilisateur du planning
    /// </summary>
    public class eObjManager : eAdminDescManager
    {

        /// <summary>
        /// Comme cette classe hérite d'une classe admin mais est utilisé pour les utilisateur standard,
        /// on vérifie si la capsule ne comporte que des modification "autorisée", en l'occurrence la modification
        /// de propriété d'affichage de planning pour l'utilisateur en cours uniquement
        /// </summary>
        /// <param name="caps"></param>
        /// <returns></returns>
        protected override eAdminCapsule<eAdminUpdateProperty> CheckCapsule(eAdminCapsule<eAdminUpdateProperty> caps)
        {

            try
            {
                //  caps = eAdminTools.GetAdminCapsule<eAdminCapsule<eAdminUpdateProperty>, eAdminUpdateProperty>(_context.Request.InputStream);

                if (caps.ListUser != null && caps.ListUser.Count() > 0)
                    throw new EudoException("Utilisation non conforme de eObjManager", "Vous n'avez pas les droits pour effectuer cette opération", launchFeedback: false);


                foreach (var prop in caps.ListProperties)
                {
                    if (prop.Category != (int)eAdminUpdateProperty.CATEGORY.PREF)
                        throw new EudoException("Utilisation non conforme de eObjManager", "Vous n'avez pas les droits pour effectuer cette opération", launchFeedback: false);

                    //
                    List<ADMIN_PREF> allowedPref = new List<ADMIN_PREF>() {

                        ADMIN_PREF.CALENDARCONFLICTENABLED,
                        ADMIN_PREF.CALENDARGRIPCONFIDENTIALCOLOR,
                        ADMIN_PREF.CALENDARGRIPMULTIOWNERCOLOR,
                        ADMIN_PREF.CALENDARGRIPOTHERCONFIDENTIALCOLOR,
                        ADMIN_PREF.CALENDARGRIPPUBLICCOLOR,
                        ADMIN_PREF.CALENDARGRIPUSEROWNERCOLOR,
                        ADMIN_PREF.CALENDARITEMDEFAULTDURATION,
                        ADMIN_PREF.CALENDARITEMOVERLAP,
                        ADMIN_PREF.CALENDARMINUTESINTERVAL,
                        ADMIN_PREF.CALENDARTASKMODE,
                        ADMIN_PREF.CALENDARTODAYONLOGIN,
                        ADMIN_PREF.CALENDARVIEWHOURBEGIN,
                        ADMIN_PREF.CALENDARVIEWHOUREND,
                        ADMIN_PREF.CALENDARWORKHOURBEGIN,
                        ADMIN_PREF.CALENDARWORKHOUREND,
                        ADMIN_PREF.CALENDARWORKINGDAYS,

                        ADMIN_PREF.HISTOBYPASSENABLED
                    };

                    /* 4 13 14 15 16 17 18 19 20 21 22 23 24 25 26 29 30 34   */
                    var t = eLibTools.GetEnumFromCode<ADMIN_PREF>(prop.Property);
                    if (!allowedPref.Contains(t))
                        throw new EudoException("Utilisation non conforme de eObjManager", "Vous n'avez pas les droits pour effectuer cette opération", launchFeedback: false);

                }

            }
            catch (Exception)
            {

                throw;
            }

            return caps;
        }

        protected override void CheckAdminRight()
        {


            return;
        }
    }
}