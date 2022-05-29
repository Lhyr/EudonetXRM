using System;
using System.Web.UI;
using Com.Eudonet.Internal;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// classe de gestion des alertes
    /// </summary>
    public partial class eAdvAlertParam : eEudoPage
    {
        /// <summary>Tab en cours</summary>
        public Int32 nTab;


        /// <summary> Date de l'Alerte </summary>
        public String strAlertDate;

        /// <summary>Delai de l'Alerte</summary>
        public Int32 nAlertTime;

        /// <summary>Son de l'Alerte</summary>
        public String strAlertSound;

        /// <summary>Date de debut</summary>
        public String strBeginDate;

        /// <summary> Indique si la fiche template est nouvelle </summary>
        public Boolean bNew;


        public string strNumber;

        public string strDatePart;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// gestion des alertes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            PageRegisters.AddCss("eCatalog");
            PageRegisters.AddCss("eList");

            if (_allKeys.Contains("New")
                && _allKeys.Contains("Tab")
                && _allKeys.Contains("AlertTime")
                && _allKeys.Contains("AlertSound")
                && _allKeys.Contains("AlertDate")
                && _allKeys.Contains("BeginDate")
                )
            {

                bNew = Request.Form["New"].ToString().Equals("1");
                nTab = eLibTools.GetNum(Request.Form["Tab"].ToString());
                nAlertTime = eLibTools.GetNum(Request.Form["AlertTime"].ToString());
                strAlertSound = Request.Form["AlertSound"].ToString();
                strAlertDate = Request.Form["AlertDate"].ToString();
                strBeginDate = Request.Form["BeginDate"].ToString();


                string[] aNumberAndDatePart = getNbrAndDPartFromMin(nAlertTime).Split('$');
                strNumber = aNumberAndDatePart[0];
                strDatePart = aNumberAndDatePart[1];
            }
        }

        /// <summary>
        /// récupère une ressource
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetRes(int id)
        {
            return eResApp.GetRes(_pref, id);
        }


        String getNbrAndDPartFromMin(int lMinutes)
        {
            int lMinute = 1;
            int lHour = 60 * lMinute;
            int lDay = 24 * lHour;
            int lWeek = 7 * lDay;

            string strMinute = "n";
            string strHour = "h";
            string strDay = "d";
            string strWeek = "w";

            string strDatePart = String.Empty;
            Int32 nNumber = 0;

            if (lMinutes >= lWeek)
            {
                strDatePart = strWeek;
                nNumber = lMinutes / lWeek;
            }
            else
                if (lMinutes < lWeek && lMinutes >= lDay)
                {
                    strDatePart = strDay;
                    nNumber = lMinutes / lDay;
                }
                else
                    if (lMinutes < lDay && lMinutes >= lHour)
                    {
                        strDatePart = strHour;
                        nNumber = lMinutes / lHour;
                    }
                    else
                    {
                        strDatePart = strMinute;
                        nNumber = lMinutes;
                    }
            return string.Concat(nNumber, "$", strDatePart);
        }

    }
}