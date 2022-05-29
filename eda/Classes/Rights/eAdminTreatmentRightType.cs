using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// type possible des traitements
    /// </summary>
    public enum eTreatmentType
    {

        /// <summary>Action unitaire (création/suppression unitaire) /// </summary>
        ATOMIC = 0,

        /// <summary>Action de traitement de masse (tous les traitements de masse de maj, duppli, suppression...)</summary>
        MASS = 1,

        /// <summary>Action liée aux options de communication</summary>
        COMMUNICATION,

        /// <summary>Action liées aux options d'analyse (raport...) </summary>
        ANALYZE,

        /// <summary>Action liées à la manipulation des pref (crée/définir des sélections) </summary>
        PREF,

        /// <summary>Action liée aux pj </summary>
        PJ,

        /// <summary>Autres...</summary>
        OTHER,

        /// <summary>Visualisation </summary>
        VIEW,

        /// <summary>Modification </summary>
        CHANGE,

        /// <summary>Catalogues </summary>
        CATALOG,

        /// <summary>Import en masse </summary>
        IMPORT
    }

    /// <summary>
    /// Ressources des type de traitement
    /// </summary>
    public class eAdminTreatmentTypeRes
    {
        public static string GetRes(eTreatmentType t, ePrefLite p)
        {
            switch (t)
            {
                case eTreatmentType.ATOMIC:
                    return eResApp.GetRes(p, 8281); //Action unitaire

                case eTreatmentType.IMPORT:
                    return eResApp.GetRes(p, 8280); //Import en masse

                case eTreatmentType.MASS:
                    return eResApp.GetRes(p, 8279); //Traitement de masse

                case eTreatmentType.COMMUNICATION:
                    return eResApp.GetRes(p, 6854); //communucation

                case eTreatmentType.ANALYZE:
                    return eResApp.GetRes(p, 6606); //Analyse

                case eTreatmentType.PREF:
                    return eResApp.GetRes(p, 445); //Préférence
                case eTreatmentType.PJ:

                    return eResApp.GetRes(p, 5042); //annexes
                case eTreatmentType.OTHER:
                    return eResApp.GetRes(p, 75); //Autre
                case eTreatmentType.VIEW:
                    return eResApp.GetRes(p, 6594); //Visu

                case eTreatmentType.CHANGE:
                    return eResApp.GetRes(p, 805); //Modif
                case eTreatmentType.CATALOG:
                    return eResApp.GetRes(p, 225); //Catalogue


                default:
                    return "UNDEFINED";
            }

        }
    }
}