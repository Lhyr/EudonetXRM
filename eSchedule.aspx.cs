using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.wcfs.data.report;
using EudoExtendedClasses;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page de choix des options de récurrence
    /// </summary>
    public partial class eSchedule : eEudoPage
    {
        public int nScheduleId;
        public int nType;
        public int nFrequency;
        public int nDay;
        public int nMonth;
        public int nOrder;
        public string strWeekDay;
        public string strBeginDate;
        public string strEndDate;
        public string strRangeCount;
        public int nTab;
        public int nCount = 0;
        public int nDefaultOrder = 0;
        public string WeekDaystrBeginDate;
        public string DaystrBeginDate;
        public string MonthstrBeginDate;
        public string _Res_Javascript = string.Empty;
        public string strWorkingDay = string.Empty;
        public bool bAppointment = false;
        public int nFileId = 0;
        public eLibConst.SCHEDULE_TYPE eScheduleType = eLibConst.SCHEDULE_TYPE.PLANNING;

        public int _nNbOcc = 5;
        public string _sHour = "22:00";

        /// <summary>Nombre maximal d'occurrence</summary>
        public int MAX_REPEAT = (int)eLibConst.SCHEDULE_FREQUENCY.SCHEDULE_MAX_RANGE_COUNT;

        int nCountDefault = 5;

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {

            ListItem ls = new ListItem();



            ScheduleTypeList.Items.Add(new ListItem(GetResHtml(1050), "daily"));
            ScheduleTypeList.Items.Add(new ListItem(GetResHtml(1051), "weekly"));
            ScheduleTypeList.Items.Add(new ListItem(GetResHtml(1052), "monthly"));
            ScheduleTypeList.Items.Add(new ListItem(GetResHtml(1053), "yearly"));
            ScheduleTypeList.Items.Add(new ListItem(GetResHtml(2682), "once")); //ALISTER => Demande/Requests #86 154
#if DEBUG
            ScheduleTypeList.Items.Add(new ListItem("10 minutes", "10minutely"));
            ScheduleTypeList.Items.Add(new ListItem("5 minutes", "5minutely"));
            ScheduleTypeList.Items.Add(new ListItem("1 minutes ", "minutely"));
#endif


            #region ajout des css

            PageRegisters.AddCss("eSchedule", "all");

            if (Request.Browser.MajorVersion == 7 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie7-styles");

            if (Request.Browser.MajorVersion == 8 && Request.Browser.Browser == "IE")
                PageRegisters.AddCss("ie8-styles");
            #endregion

            #region ajout des js

            PageRegisters.AddScript("eModaldialog");
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eUpdater");
            PageRegisters.AddScript("eSchedule");

            #endregion

            eScheduleType = eLibConst.SCHEDULE_TYPE.PLANNING;
            if (_requestTools.AllKeys.Contains("scheduletype"))
                Enum.TryParse(Request.Form["scheduletype"].ToString(), true, out eScheduleType);

            MainScheduleType.Value = ((int)eScheduleType).ToString();

            try
            {
                //PARAMETRES
                nScheduleId = eLibTools.GetNum(Request.Form["ScheduleId"].ToString());
                string strWeekWorkDayUser = Request.Form["WorkingDay"].ToString();
                strBeginDate = Request.Form["BeginDate"].ToString();
                string strParentEndDate = Request.Form["EndDate"].ToString();

                nTab = eLibTools.GetNum(Request.Form["Tab"].ToString());
                int nDateDescId = 0;
                if (_requestTools.AllKeys.Contains("DateDescId"))
                    nDateDescId = eLibTools.GetNum(Request.Form["DateDescId"].ToString());

                bAppointment = eLibTools.GetNum(Request.Form["AppType"].ToString()) > 0;
                nFileId = _requestTools.GetRequestFormKeyI("FileId") ?? 0;

                nType = 0;
                nFrequency = 0;
                nDay = 0;
                nOrder = 0;
                strWeekDay = string.Empty;
                nMonth = 0;
                strEndDate = string.Empty;

                switch (eScheduleType)
                {
                    case eLibConst.SCHEDULE_TYPE.PLANNING:
                    case eLibConst.SCHEDULE_TYPE.EMAILING:
                    case eLibConst.SCHEDULE_TYPE.SMSING:
                    case eLibConst.SCHEDULE_TYPE.RECIPIENT:
                    case eLibConst.SCHEDULE_TYPE.RECURRENT_TRIGGER:
                        LoadFromDB();
                        break;
                    case eLibConst.SCHEDULE_TYPE.REPORT:
                        LoadFromJSON();
                        break;
                    case eLibConst.SCHEDULE_TYPE.UNDEFINED:

                        break;
                    default:
                        break;
                }

                // 2017-05-05 - #54 037
                // Si on paramètre une nouvelle périodicité, on récupère les infos à partir de la date de début préremplie.
                // Si on modifie une périodicité existante, on utilise celles en provenance de la table SCHEDULE
                // (renseignées par l'appel à la méthode Schedule* ci-dessus)
                if (nScheduleId == 0)
                {
                    DaystrBeginDate = DateTime.Parse(strBeginDate).Day.ToString();
                    WeekDaystrBeginDate = (DateTime.Parse(strBeginDate).DayOfWeek.GetHashCode() + 1).ToString();
                    MonthstrBeginDate = DateTime.Parse(strBeginDate).Month.ToString();
                }
                else
                {
                    DaystrBeginDate = nDay.ToString();
                    string[] tab = strWeekDay.Split(';');
                    if (tab.Length > 0)
                        WeekDaystrBeginDate = tab[0];
                    MonthstrBeginDate = nMonth.ToString();
                }

                strWorkingDay = _pref.GetPref(nTab, ePrefConst.PREF_PREF.CALENDARWORKINGDAYS);

                // Si il n'y a pas d'heure (Tâches), on rajoute l'heure
                if (strBeginDate.Trim().Length <= 10)
                    strBeginDate = string.Concat(strBeginDate.Trim(), " 00:00");

                nDefaultOrder = 0;
                if (nDay <= 7)
                    nDefaultOrder = 1;
                else if (nDay <= 14)
                    nDefaultOrder = 2;
                else if (nDay <= 21)
                    nDefaultOrder = 3;
                else
                    nDefaultOrder = 4;

                if (eScheduleType == eLibConst.SCHEDULE_TYPE.REPORT)
                {
                    MAX_REPEAT = 500;
                }

                switch (eScheduleType)
                {
                    case eLibConst.SCHEDULE_TYPE.PLANNING:
                        hourDiv.Style.Add("display", "none");
                        hourTitle.Style.Add("display", "none");

                        recipientsDiv.Style.Add("display", "none");
                        recipientsTitle.Style.Add("display", "none");

                        break;
                    case eLibConst.SCHEDULE_TYPE.REPORT:
                    case eLibConst.SCHEDULE_TYPE.EMAILING:
                    case eLibConst.SCHEDULE_TYPE.SMSING:
                    case eLibConst.SCHEDULE_TYPE.RECIPIENT:
                    case eLibConst.SCHEDULE_TYPE.RECURRENT_TRIGGER:
                        // On reprend l'heure transmis
                        if (nScheduleId <= 0 || string.IsNullOrEmpty(_sHour))
                            _sHour = _requestTools.GetRequestFormKeyS("hour") ?? _sHour;

                        LoadHours();

                        break;
                }

                ftpTitle.Style.Add("display", "none");
                ftpDiv.Style.Add("display", "none");
            }
            catch (eEndResponseException) { Response.End(); }
            catch (ThreadAbortException) { }    // Laisse passer le response.end du RenderResult
            catch (Exception ex)
            {
                try
                {
                    LaunchError(eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 72),
                        eResApp.GetRes(_pref, 6342),
                        eResApp.GetRes(_pref, 1049),
                        string.Concat("Erreur non gérée eSchedule.espx : ", Environment.NewLine, ex.ToString())));
                }
                catch (eEndResponseException) { }
            }
        }

        /// <summary>
        /// Raccourcis pour recup la ressource en front (aspx)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetResHtml(int id)
        {
            return eResApp.GetRes(_pref, id);
        }

        /// <summary>
        /// Retourne le nombre de jours dans un mois donné
        /// </summary>
        public int GetDaysOfMont(DateTime date, int month)
        {
            if (month == 2)
            {
                if (date.Year % 4 == 0)
                    return 29;
                else
                    return 28;
            }
            else
            {
                DateTime dFirstDateOfMonth = new DateTime(date.Year, month, 1);
                DateTime dLastDateOfMonth = dFirstDateOfMonth.AddMonths(1).AddDays(-1);
                return (Int32)(dLastDateOfMonth - dFirstDateOfMonth).TotalDays + 1;
            }
        }

        private void LoadHours()
        {
            HtmlGenericControl lst = new HtmlGenericControl("select");
            lst.ID = "HourSelect";
            lst.Attributes.Add("name", "HourSelect");
            tdHourLaunch.Controls.Add(lst);

            DateTime currentTime = DateTime.Now;
            SortedSet<string> listHours = new SortedSet<string>();
            listHours.Add(_sHour);

            int nInterval = 30;

#if DEBUG

            nInterval = 2;
#endif

            for (int i = 0; i < 24 * 60; i += nInterval)
            {
                currentTime = currentTime.Date.AddMinutes(i);
                string sHour = string.Concat(currentTime.Hour.ToString().PadLeft(2, '0'), ":", currentTime.Minute.ToString().PadLeft(2, '0'));
                listHours.Add(sHour);
            }

            foreach (string sHour in listHours)
            {
                HtmlGenericControl interval = new HtmlGenericControl("option");
                interval.Attributes.Add("value", sHour);

                if (_sHour == sHour)
                    interval.Attributes.Add("selected", "");

                interval.InnerHtml = sHour;
                lst.Controls.Add(interval);
            }
        }

        private void LoadFromJSON()
        {
            nScheduleId = -1;

            if (!_requestTools.AllKeys.Contains("jsonparam"))
            {
                strBeginDate = DateTime.Now.ToString("dd/MM/yyyy");
                return;
            }

            string sParamSchedule = Request.Form["jsonparam"].ToString();

            try
            {
                eScheduledReportData eR = JsonConvert.DeserializeObject<eScheduledReportData>(sParamSchedule,
                    new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                strBeginDate = eR.StartDate;
                strEndDate = eR.EndDate;
                if (strEndDate == DateTime.MinValue.ToString("dd/MM/yyyy"))
                    strEndDate = "";

                nType = ((int)eR.FrequencyType);
                nFrequency = eR.Frequency;

                nDay = eR.Day;
                nOrder = eR.Order;
                //ALISTER  #89754 -> #95546 On met le +1 au lieu de ne pas le mettre car chaque type de fréquence (FrequencyType) on besoin d'être incrémenté
                //We increase by 1 instead of not increase it, because every frequency type need to be incremented
                strWeekDay = string.Join(";", eR.WeekDays);
                nMonth = eR.Month;

                nCount = eR.Repeat;
                _sHour = eR.Hour;
            }
            catch (Exception)
            {
                throw new Exception("Impossible d'initialisé le schedule.");
            }
        }

        private void LoadFromDB()
        {
            eScheduleObject schedule = new eScheduleObject(nScheduleId);
            try
            {
                DateTime dtBeginDate = DateTime.Now;
                if (DateTime.TryParse(strBeginDate, out dtBeginDate))
                    schedule.LoadFromDB(_pref, true, dtBeginDate);
                else
                    schedule.LoadFromDB(_pref);

                nType = (int)schedule.Type;
                nFrequency = schedule.Frequency;
                nDay = schedule.Day;
                nOrder = schedule.Order;
                if (eScheduleType == eLibConst.SCHEDULE_TYPE.SMSING && nScheduleId == 0)
                {
                    strWeekDay = string.Empty;
                    nCount = 1;
                }
                else
                {
                    strWeekDay = string.Join<int>(";", schedule.WeekDays);
                    nCount = schedule.RangeCount;
                }
                nMonth = schedule.Month;
                if (eScheduleType == eLibConst.SCHEDULE_TYPE.EMAILING || eScheduleType == eLibConst.SCHEDULE_TYPE.SMSING
                    || eScheduleType == eLibConst.SCHEDULE_TYPE.RECIPIENT || eScheduleType == eLibConst.SCHEDULE_TYPE.RECURRENT_TRIGGER)

                    strBeginDate = schedule.BeginDate.ToShortDateString();
                else
                    strBeginDate = schedule.BeginDate.ToString();

                strEndDate = schedule.EndDate.HasValue ? schedule.EndDate.Value.ToString() : string.Empty;


                TimeSpan? tsTime = schedule.Time;
                if (tsTime.HasValue)
                    _sHour = string.Format("{0:00}:{1:00}", tsTime.Value.Hours, tsTime.Value.Minutes);
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.EXCLAMATION,
                            eResApp.GetRes(_pref, 72),
                            eResApp.GetRes(_pref, 6342),
                            eResApp.GetRes(_pref, 6523),
                            string.Concat("eSchedule.aspx : ", ex.Message));

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }
        }
    }
}