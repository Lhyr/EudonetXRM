using Com.Eudonet.Xrm;
using EudoQuery;
using System;
using System.Collections.Generic;
using static Com.Eudonet.Xrm.eConst;

namespace Com.Eudonet.Internal
{
    /// <summary>
    /// Classe de gestion de la disponibilité des fonctionnalités
    /// </summary>
    public class eFeaturesManager
    {
        XrmFeature _feature;
        ePrefUser _pref;

        /// <summary>
        /// Vérifie si la fonctionnalité f est disponible par rapport à la version et l'offre en cours
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="f">XrmFeature : fonctionnalité</param>
        /// <returns>
        ///   <c>true</c> if [is feature available]; otherwise, <c>false</c>.
        /// </returns>
        public static Boolean IsFeatureAvailable(ePrefUser pref, XrmFeature f)
        {
#if DEBUG
            return true;
#endif
            eFeaturesManager mgr = new eFeaturesManager(pref, f);
            return mgr.IsFeatureAvailableForVersion(eConst.VERSION)
                && mgr.IsFeatureAvailableForOffer(pref.ClientInfos.ClientOffer);

        }

        //public static eAdminExtension.ExtensionAvailability IsExtensionAvailable(ePref pref, XrmExtension ext)
        //{
        //    eAdminExtension.ExtensionAvailability dispo = eAdminExtension.ExtensionAvailability.Included;
        //    eLibConst.ClientOffer offer = pref.ClientInfos.ClientOffer;

        //    if (offer == eLibConst.ClientOffer.ACCES)
        //    {
        //        dispo = GetAvailibilityForAccesOffer(ext);
        //    }
        //    else if (offer == eLibConst.ClientOffer.STANDARD)
        //    {
        //        dispo = GetAvailibilityForStandardOffer(ext);
        //    }
        //    else if (offer == eLibConst.ClientOffer.PREMIER)
        //    {
        //        dispo = GetAvailibilityForPremierOffer();
        //    }
        //    else
        //    {
        //        dispo = eAdminExtension.ExtensionAvailability.Unavailable;
        //    }

        //    return dispo;
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="eFeaturesManager"/> class.
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="feature">The feature.</param>
        protected eFeaturesManager(ePrefUser pref, XrmFeature feature)
        {
            _pref = pref;
            _feature = feature;
        }

        /// <summary>
        /// Vérifie si la fonctionnalité est disponible pour la version
        /// </summary>
        /// <param name="version">Version 10.XXX.XXX</param>
        /// <returns>
        ///   <c>true</c> if [is feature available for version] [the specified version]; otherwise, <c>false</c>.
        /// </returns>
        Boolean IsFeatureAvailableForVersion(String version)
        {
            int nCurrentVersion = eLibTools.GetNum(version.Replace(".", ""));

            String releaseVersion = GetFeatureReleaseVersion(_feature);
            int nReleaseVersion = eLibTools.GetNum(releaseVersion.Replace(".", ""));

            if (nCurrentVersion >= nReleaseVersion)
                return true;
            return false;
        }

        /// <summary>
        /// Vérifie si la fonctionnalité f est disponible pour l'offre
        /// </summary>
        /// <param name="offer">Offre du client</param>
        /// <returns>
        ///   <c>true</c> if [is feature available]; otherwise, <c>false</c>.
        /// </returns>
        Boolean IsFeatureAvailableForOffer(eLibConst.ClientOffer offer)
        {
            // Si l'utilisateur est super admin, on ne vérifie pas son offre
            if (_pref.User.UserLevel >= UserLevel.LEV_USR_SUPERADMIN.GetHashCode())
                return true;

            if (GetFeatureAllowedOffers(_feature).Contains(offer))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Retourne la liste des offres pour lesquelles la fonctionnalité est disponible
        /// </summary>
        /// <param name="f">XrmFeature : fonctionnalité</param>
        /// <returns></returns>
        List<eLibConst.ClientOffer> GetFeatureAllowedOffers(XrmFeature f)
        {
            List<eLibConst.ClientOffer> offers;
            switch (f)
            {
                case XrmFeature.Admin:
                case XrmFeature.AdminParameters:
                case XrmFeature.AdminParameters_Logo:

                case XrmFeature.AdminAccess:
                case XrmFeature.AdminAccess_GroupsAndUsers:
                case XrmFeature.AdminHome:
                case XrmFeature.AdminExtensions:
                case XrmFeature.AdminDashboard:
                case XrmFeature.Grid_RightsManager:
                case XrmFeature.File_CancelLastEntries:
                case XrmFeature.File_RefSIRET:
                case XrmFeature.Maps_InternationalPredictiveAddr:
                case XrmFeature.Widget_Tile:
                case XrmFeature.Import:
                case XrmFeature.HTMLTemplateEditor: // #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs)
                    offers = new List<eLibConst.ClientOffer>()
                    {
                        eLibConst.ClientOffer.ACCES,
                        eLibConst.ClientOffer.ESSENTIEL,
                        eLibConst.ClientOffer.STANDARD,
                        eLibConst.ClientOffer.PREMIER,
                        eLibConst.ClientOffer.PRO,
                    };
                    break;
                case XrmFeature.AdminParameters_Navigation:
                case XrmFeature.Widget_Kanban:
                case XrmFeature.AdminTabs:
                    offers = new List<eLibConst.ClientOffer>()
                    {
                        eLibConst.ClientOffer.STANDARD,
                        eLibConst.ClientOffer.PREMIER,
                        eLibConst.ClientOffer.PRO,
                        eLibConst.ClientOffer.ESSENTIEL,

                    };
                    break;
                case XrmFeature.AdminParameters_Localization:
                case XrmFeature.AdminParameters_Supervision:
                case XrmFeature.AdminAccess_Security:
                case XrmFeature.AdminAccess_Preferences:
                case XrmFeature.Grid_WebSubtabInFirstPosition:
                case XrmFeature.Grid_ListModeReturn:   
                case XrmFeature.Bkm_Grid:
                    offers = new List<eLibConst.ClientOffer>()
                    {
                        eLibConst.ClientOffer.STANDARD,
                        eLibConst.ClientOffer.PREMIER,
                        eLibConst.ClientOffer.PRO,

                    };
                    break;
                case XrmFeature.AdminParameters_Advanced:
                    offers = new List<eLibConst.ClientOffer>()
                    {
                        eLibConst.ClientOffer.PREMIER
                    };
                    break;
                case XrmFeature.AdminProduct:
                default:
                    // Si la fonctionnalité n'est pas définie, elle est disponible dans toutes les offres
                    offers = new List<eLibConst.ClientOffer>()
                    {
                        eLibConst.ClientOffer.XRM,
                        eLibConst.ClientOffer.ACCES,
                        eLibConst.ClientOffer.ESSENTIEL,
                        eLibConst.ClientOffer.STANDARD,
                        eLibConst.ClientOffer.PREMIER,
                        eLibConst.ClientOffer.PRO,
                    };
                    break;
            }
            return offers;
        }

        /// <summary>
        /// Récupère la version à partir de laquelle la fonctionnalité est disponible
        /// </summary>
        /// <param name="f">XrmFeature : Fonctionnalité</param>
        /// <returns>Version sous la forme "10.XXX.XXX"</returns>
        String GetFeatureReleaseVersion(XrmFeature f)
        {
            String version;
            switch (f)
            {
                case XrmFeature.Maps_InternationalPredictiveAddr:
                case XrmFeature.GraphExpressFilter:
                    version = "10.308.000";
                    break;
                case XrmFeature.Widget_Tile:
                case XrmFeature.AdminProduct:
                    version = "10.309.000";
                    break;

                case XrmFeature.Import:
                case XrmFeature.Field_Association:
                case XrmFeature.File_CancelLastEntries:
                case XrmFeature.File_RefSIRET:
                case XrmFeature.AdminRGPD:
                case XrmFeature.Widget_Kanban:
                case XrmFeature.Bkm_Grid:
                    version = "10.312.000";
                    break;
                case XrmFeature.AdminEventStep:
                    version = "10.412.000";
                    break;
                // #68 13x et Backlog #375 - Editeur de templates HTML avancé (grapesjs)
                case XrmFeature.HTMLTemplateEditor:
                    version = "10.412.000";
                    break;
                case XrmFeature.GraphTeams:
                    version = "10.709.000";
                    break;
                default:
                    version = eConst.VERSION;
                    break;
            }
            return version;
        }

        //public static eAdminExtension.ExtensionAvailability GetAvailibilityForAccesOffer(XrmExtension ext)
        //{
        //    eAdminExtension.ExtensionAvailability dispo = eAdminExtension.ExtensionAvailability.Included;

        //    switch (ext)
        //    {
        //        case XrmExtension.Notifications:
        //        case XrmExtension.Cartographie:
        //        case XrmExtension.Timeline:
        //        case XrmExtension.Enquetes:
        //        case XrmExtension.DemandesEntrantes:
        //        case XrmExtension.Sepa:
        //        case XrmExtension.Compta:
        //        case XrmExtension.Gescom:
        //        case XrmExtension.Dedoublonnage:
        //        case XrmExtension.Objectifs:
        //        case XrmExtension.Mobile:
        //        case XrmExtension.EudoDrop:
        //        case XrmExtension.EudoImport:
        //        case XrmExtension.EudoSync:
        //            dispo = eAdminExtension.ExtensionAvailability.Extension;
        //            break;
        //        case XrmExtension.API:
        //        case XrmExtension.Grilles:
        //        case XrmExtension.Workflow:
        //        case XrmExtension.Scoring:
        //            dispo = eAdminExtension.ExtensionAvailability.Unavailable;
        //            break;
        //    }
        //    return dispo;
        //}

        //public static eAdminExtension.ExtensionAvailability GetAvailibilityForStandardOffer(XrmExtension ext)
        //{
        //    eAdminExtension.ExtensionAvailability dispo = eAdminExtension.ExtensionAvailability.Included;

        //    switch (ext)
        //    {
        //        case XrmExtension.API:
        //        case XrmExtension.Timeline:
        //        case XrmExtension.Workflow:
        //        case XrmExtension.Compta:
        //        case XrmExtension.Gescom:
        //        case XrmExtension.EudoSync:
        //            dispo = eAdminExtension.ExtensionAvailability.Extension;
        //            break;
        //    }
        //    return dispo;
        //}

        //public static eAdminExtension.ExtensionAvailability GetAvailibilityForPremierOffer()
        //{
        //    return eAdminExtension.ExtensionAvailability.Included;
        //}
    }
}
