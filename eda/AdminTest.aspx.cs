using System;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm.eda
{

    /// <summary>
    /// page de test pour les maj admion
    /// </summary>
    public partial class AdminTest : System.Web.UI.Page
    {
        /// <summary>
        /// test variables de mise à jour liaison PM
        /// </summary>
        public String interPM = String.Concat("0|", eLibConst.DESC.INTERPM.GetHashCode());

        /// <summary>
        /// test variables de mise à jour liaison PP
        /// </summary>
        public String interPP = String.Concat("0|", eLibConst.DESC.INTERPP.GetHashCode());

        /// <summary>
        /// test variables de mise à jour liaison EV
        /// </summary>
        public String interEVT = String.Concat("0|", eLibConst.DESC.INTEREVENT.GetHashCode());
        //public String interADR = String.Concat("1|", ePrefConst.PREF_PREF.ADRJOIN.GetHashCode());

        /// <summary>
        /// test variables de mise à jour langue fr
        /// </summary>
        public String lFr = "2|0";

        /// <summary>
        /// test variables de mise à jour langue en
        /// </summary>
        public String lEn = "2|1";


        /// <summary>
        /// longueur champ
        /// </summary>
        public String sLength = String.Concat("0|", eLibConst.DESC.LENGTH.GetHashCode());

        /// <summary>
        /// case du champ
        /// </summary>
        public String sCase = String.Concat("0|", eLibConst.DESC.CASE.GetHashCode());


        /// <summary>
        /// saisie obligatoire
        /// </summary>
        public String sObligat = String.Concat("0|", eLibConst.DESC.OBLIGAT.GetHashCode());

        /// <summary>
        /// tooltip
        /// </summary>
        public String sTooltip = String.Concat("0|", eLibConst.DESC.TOOLTIPTEXT.GetHashCode());

        // CONFIG

        /// <summary>
        /// test Version office
        /// </summary>
        public String sOfficeRelease = String.Concat("3|", eLibConst.PREF_CONFIG.OFFICERELEASE.GetHashCode());

        /// <summary>
        /// test tooltip
        /// </summary>
        public String sTooltipEnabled = String.Concat("3|", eLibConst.PREF_CONFIG.TOOLTIPTEXTENABLED.GetHashCode());

        /// <summary>
        /// test serveur smpty
        /// </summary>
        public String sServerSMTP = String.Concat("4|", eLibConst.CONFIG_DEFAULT.SMTPSERVERNAME.GetHashCode());


        //CONFIGADV

        /// <summary>
        /// test Séparateur milier
        /// </summary>
        public String sSectionSep = String.Concat("5|", eLibConst.CONFIGADV.NUMBER_SECTIONS_DELIMITER.GetHashCode(), "|0");


        /// <summary>
        /// test Séparateur décimal
        /// </summary>
        public String sDecimalSep = String.Concat("5|", eLibConst.CONFIGADV.NUMBER_DECIMAL_DELIMITER.GetHashCode(), "|0");


        /// <summary>
        /// Page load  - aucune action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}