using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eAdminPlanningPrefRenderer : eRenderer
    {
        int _userId;
        Dictionary<ePrefConst.PREF_PLANNING, String> _defValues;
        Dictionary<ePrefConst.PREF_PLANNING, String> _listPref;
        String _weekFirstDay;
        Boolean _bDefaultPref;

        private eAdminPlanningPrefRenderer(ePref pref, int tab, int userid)
        {
            Pref = pref;
            this._tab = tab;
            this._userId = userid;
            _listPref = new Dictionary<ePrefConst.PREF_PLANNING, String>();

            _bDefaultPref = (userid == 0) ? true : false;
        }

        public static eAdminPlanningPrefRenderer CreateAdminPlanningPrefRenderer(ePref pref, int tab, int userid)
        {
            return new eAdminPlanningPrefRenderer(pref, tab, userid);
        }


        protected override bool Build()
        {

            if (base.Build())
            {
                if (_tab > 0)
                {
                    SetDefaultValues();

                    LoadPlanningPref();

                    GenerateHoursPart();

                    GenerateWorkWeekPart();

                    GenerateDayTasksPart();

                    GenerateColorsPart();

                    GenerateOptionsPart();
                }


                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Charge les préférences nécessaires aux options du planning
        /// </summary>
        private void LoadPlanningPref()
        {
            eEnumTools<ePrefConst.PREF_PLANNING> eta = new eEnumTools<ePrefConst.PREF_PLANNING>();

            _listPref = _bDefaultPref ? Pref.GetPrefCalendarDefault(_tab, eta.GetList) : Pref.GetPrefCalendar(_tab, eta.GetList);
        }

        /// <summary>
        /// Valeurs par défaut reprises de la V7 lorsque aucune valeur n'est définie pour une pref
        /// </summary>
        private void SetDefaultValues()
        {
            _defValues = new Dictionary<ePrefConst.PREF_PLANNING, string>();
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarViewHourBegin, "08:00");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarViewHourEnd, "19:30");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarWorkingDays, "2;3;4;5;6");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarWorkHourBegin, "09:00");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarWorkHourEnd, "18:00");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarMinutesInterval, "30");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarItemDefaultDuration, "120");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarItemOverLap, "5");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarTodayOnLogin, "0");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarTaskMode, "0");
            _defValues.Add(ePrefConst.PREF_PLANNING.CalendarConflictEnabled, "1");
        }

        /// <summary>
        /// Lorsqu'aucune valeur n'est définie pour la pref, on retourne la valeur par défaut (si elle est définie dans _defValues)
        /// </summary>
        /// <param name="prefCol"></param>
        /// <returns></returns>
        private string GetPrefVal(ePrefConst.PREF_PLANNING prefCalendarCol)
        {
            string value = _listPref[prefCalendarCol];
            if (String.IsNullOrEmpty(value))
                _defValues.TryGetValue(prefCalendarCol, out value);
            return value;
        }

        private void GenerateHoursPart()
        {
            HtmlGenericControl label;
            Panel divOption;
            DropDownList ddl;
            ListItem selectedItem;

            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerText = eResApp.GetRes(Pref, 1235);

            Panel div = new Panel();
            div.CssClass = "partContent";
            div.ID = "hoursOptions";

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            // Heure de début
            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 909);
            ddl = GenerateListHours(23 * 60);
            ddl.ID = "ddlViewBeginHour";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARVIEWHOURBEGIN.GetHashCode()));
            selectedItem = ddl.Items.FindByText(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarViewHourBegin));
            if (selectedItem != null)
                selectedItem.Selected = true;
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);

            // Heure de fin

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 910);
            ddl = GenerateListHours(23 * 60);
            ddl.ID = "ddlViewEndHour";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARVIEWHOUREND.GetHashCode()));
            selectedItem = ddl.Items.FindByText(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarViewHourEnd));
            if (selectedItem != null)
                selectedItem.Selected = true;
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);

            PgContainer.Controls.Add(h3);
            PgContainer.Controls.Add(div);
        }

        /// <summary>
        /// Génération de la partie "Semaine de travail"
        /// </summary>
        private void GenerateWorkWeekPart()
        {
            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerText = eResApp.GetRes(Pref, 900);

            Panel div = new Panel();
            div.CssClass = "partContent";
            div.ID = "weekOptions";

            String workingDays = GetPrefVal(ePrefConst.PREF_PLANNING.CalendarWorkingDays);
            String[] arrWorkingDays = workingDays.Trim().Split(';');

            _weekFirstDay = arrWorkingDays[0];

            #region Liste des jours de la semaine
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.ID = "listWeekDays";
            ul.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARWORKINGDAYS.GetHashCode()));

            AddDayHtmlListItem(ul, 2, arrWorkingDays);
            AddDayHtmlListItem(ul, 3, arrWorkingDays);
            AddDayHtmlListItem(ul, 4, arrWorkingDays);
            AddDayHtmlListItem(ul, 5, arrWorkingDays);
            AddDayHtmlListItem(ul, 6, arrWorkingDays);
            AddDayHtmlListItem(ul, 7, arrWorkingDays);
            AddDayHtmlListItem(ul, 1, arrWorkingDays);

            div.Controls.Add(ul);

            #endregion

            #region Options
            // Premier jour de la semaine
            Panel divOption = new Panel();
            divOption.CssClass = "weekOption";

            HtmlGenericControl label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 901);
            DropDownList ddl = new DropDownList();
            ddl.ID = "ddlFirstDay";
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 45), "2"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 46), "3"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 47), "4"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 48), "5"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 49), "6"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 50), "7"));
            ddl.Items.Add(new ListItem(eResApp.GetRes(Pref, 44), "1"));

            ddl.SelectedValue = _weekFirstDay;

            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);
            div.Controls.Add(divOption);

            #region Heure de début

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 909);
            ddl = GenerateListHours(23 * 60);
            ddl.ID = "ddlBeginHour";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARWORKHOURBEGIN.GetHashCode()));
            ListItem selectedItem = ddl.Items.FindByText(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarWorkHourBegin));
            if (selectedItem != null)
                selectedItem.Selected = true;
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);
            #endregion

            #region Heure de fin

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 910);
            ddl = GenerateListHours(1410, 30);
            ddl.ID = "ddlEndHour";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARWORKHOUREND.GetHashCode()));
            selectedItem = ddl.Items.FindByText(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarWorkHourEnd));
            if (selectedItem != null)
                selectedItem.Selected = true;
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);
            #endregion

            #region Durée des intervalles

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 1197);
            ddl = new DropDownList();
            ddl.ID = "ddlRangesDuration";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARMINUTESINTERVAL.GetHashCode()));
            ddl.Items.Add(new ListItem("00:05", "5"));
            ddl.Items.Add(new ListItem("00:10", "10"));
            ddl.Items.Add(new ListItem("00:15", "15"));
            ddl.Items.Add(new ListItem("00:20", "20"));
            ddl.Items.Add(new ListItem("00:30", "30"));
            ddl.Items.Add(new ListItem("01:00", "60"));

            int interval = eLibTools.GetNum(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarMinutesInterval));
            ListItem item = ddl.Items.FindByValue(interval.ToString());
            if (item != null)
            {
                item.Selected = true;
            }
            else
            {
                // On doit arrondir la valeur existante par rapport à la nouvelle liste d'intervalles
                List<int> listIntervals = new List<int> { 5, 10, 15, 20, 30, 60 };
                int nearestInterval = 5;

                if (interval > 60)
                    nearestInterval = 60; // Si l'intervalle est supérieure à l'intervalle max, on sélectionne l'intervalle max
                else if (interval < 5)
                    nearestInterval = 5; // Si l'intervalle est inférire à l'intervalle min, on sélectionne l'intervalle min
                else
                {
                    nearestInterval = FindNearestValue(listIntervals, interval);
                }

                ddl.SelectedValue = nearestInterval.ToString();
            }
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);

            #endregion

            #region Durée par défaut d'un rendez-vous

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 1018);
            ddl = new DropDownList();
            ddl.ID = "ddlDefaultRange";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARITEMDEFAULTDURATION.GetHashCode()));
            ddl.Items.Add(new ListItem("00:05", "5"));
            ddl.Items.Add(new ListItem("00:10", "10"));
            ddl.Items.Add(new ListItem("00:15", "15"));
            ddl.Items.Add(new ListItem("00:20", "20"));
            ddl.Items.Add(new ListItem("00:30", "30"));
            ddl.Items.Add(new ListItem("01:00", "60"));
            ddl.Items.Add(new ListItem("02:00", "120"));
            ddl.Items.Add(new ListItem("03:00", "180"));
            ddl.Items.Add(new ListItem("04:00", "240"));
            ddl.Items.Add(new ListItem("05:00", "300"));
            ddl.Items.Add(new ListItem("06:00", "360"));
            ddl.Items.Add(new ListItem("07:00", "420"));
            ddl.Items.Add(new ListItem("08:00", "480"));
            ddl.Items.Add(new ListItem("09:00", "540"));
            ddl.Items.Add(new ListItem("10:00", "600"));
            ddl.Items.Add(new ListItem("12:00", "720"));
            ddl.Items.Add(new ListItem("24:00", "1440"));

            int duration = eLibTools.GetNum(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarItemDefaultDuration));
            item = ddl.Items.FindByValue(duration.ToString());
            if (item != null)
            {
                item.Selected = true;
            }
            else
            {
                // On doit arrondir la valeur existante par rapport à la nouvelle liste d'intervalles
                List<int> listIntervals = new List<int> { 5, 10, 15, 20, 30, 60, 120, 180, 240, 300, 360, 420, 480, 540, 600, 720, 1440 };
                int nearestInterval = 5;

                if (interval > 1440)
                    nearestInterval = 1440; // Si l'intervalle est supérieure à l'intervalle max, on sélectionne l'intervalle max
                else if (interval < 5)
                    nearestInterval = 5; // Si l'intervalle est inférire à l'intervalle min, on sélectionne l'intervalle min
                else
                {
                    nearestInterval = FindNearestValue(listIntervals, duration);
                }

                ddl.SelectedValue = nearestInterval.ToString();
            }

            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);
            #endregion

            // Nb maximal de chevauchements

            divOption = new Panel();
            divOption.CssClass = "weekOption";

            label = new HtmlGenericControl("label");
            label.InnerText = eResApp.GetRes(Pref, 1019);
            ddl = new DropDownList();
            ddl.ID = "ddlMaxCh";
            ddl.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", ADMIN_PREF.CALENDARITEMOVERLAP.GetHashCode()));
            ddl.Items.Add(new ListItem("1"));
            ddl.Items.Add(new ListItem("2"));
            ddl.Items.Add(new ListItem("3"));
            ddl.Items.Add(new ListItem("4"));
            ddl.Items.Add(new ListItem("5"));
            ddl.Items.Add(new ListItem("6"));
            ddl.Items.Add(new ListItem("7"));
            ddl.Items.Add(new ListItem("8"));
            ddl.Items.Add(new ListItem("9"));
            ddl.Items.Add(new ListItem("10"));
            ddl.SelectedValue = GetPrefVal(ePrefConst.PREF_PLANNING.CalendarItemOverLap);
            divOption.Controls.Add(label);
            divOption.Controls.Add(ddl);

            div.Controls.Add(divOption);

            #endregion


            PgContainer.Controls.Add(h3);
            PgContainer.Controls.Add(div);
        }

        /// <summary>
        /// Ajoute un contrôle "li" correspondant à un jour de la semaine
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="dayNum"></param>
        /// <param name="selectedDays"></param>
        private void AddDayHtmlListItem(HtmlGenericControl ul, int dayNum, String[] selectedDays)
        {
            HtmlGenericControl li = new HtmlGenericControl("li");
            li.Attributes.Add("data-daynum", dayNum.ToString());
            String dayLabel = String.Empty;
            switch (dayNum)
            {
                case 1: dayLabel = eResApp.GetRes(Pref, 44); break;
                case 2: dayLabel = eResApp.GetRes(Pref, 45); break;
                case 3: dayLabel = eResApp.GetRes(Pref, 46); break;
                case 4: dayLabel = eResApp.GetRes(Pref, 47); break;
                case 5: dayLabel = eResApp.GetRes(Pref, 48); break;
                case 6: dayLabel = eResApp.GetRes(Pref, 49); break;
                case 7: dayLabel = eResApp.GetRes(Pref, 50); break;
            }
            li.InnerText = dayLabel;
            if (selectedDays.Contains(dayNum.ToString()))
            {
                li.Attributes.Add("class", "selected");
            }
            ul.Controls.Add(li);
        }

        /// <summary>
        /// A partir d'une liste de valeurs, on retourne la valeur la plus proche de la valeur en entrée 
        /// </summary>
        /// <param name="listIntervals"></param>
        /// <returns></returns>
        private int FindNearestValue(List<int> listIntervals, int value)
        {
            int nearestInterval = listIntervals[0];

            for (int i = 0; i < listIntervals.Count; i++)
            {
                if (listIntervals[i] < value && value < listIntervals[i + 1])
                {
                    if (listIntervals[i + 1] - value < value - listIntervals[i])
                        nearestInterval = listIntervals[i + 1];
                    else
                        nearestInterval = listIntervals[i];

                    break;
                }
                else
                {
                    continue;
                }
            }
            return nearestInterval;
        }

        /// <summary>
        /// Génère une DropDownlist avec la liste des heures avec un intervalle défini
        /// </summary>
        /// <param name="endMinute"></param>
        /// <param name="beginMinute"></param>
        /// <returns></returns>
        private DropDownList GenerateListHours(int endMinute, int beginMinute = 0, int interval = 30)
        {
            DropDownList ddl = new DropDownList();

            for (int i = beginMinute; i <= endMinute; i += interval)
            {
                ddl.Items.Add(new ListItem(ConvertMinutesToHour(i)));
            }

            return ddl;
        }

        /// <summary>
        /// Affichage des minutes dans le format 00:00
        /// </summary>
        /// <param name="minutes"></param>
        /// <returns></returns>
        private String ConvertMinutesToHour(int minutes)
        {
            int hour = minutes / 60;
            int min = minutes % 60;
            return String.Concat(hour.ToString().PadLeft(2, '0'), ":", min.ToString().PadLeft(2, '0'));
        }

        /// <summary>
        /// Génération de la partie "Tâches affichées en mode jour"
        /// </summary>
        private void GenerateDayTasksPart()
        {
            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerText = eResApp.GetRes(Pref, 7127);

            Panel div = new Panel();
            div.CssClass = "partContent";

            Dictionary<String, String> items = new Dictionary<String, String>();
            items.Add(CalendarTaskMode.CALENDAR_ITEM_NO_TASK.GetHashCode().ToString(), eResApp.GetRes(Pref, 1022));
            items.Add(CalendarTaskMode.CALENDAR_ITEM_ALL_TASK.GetHashCode().ToString(), eResApp.GetRes(Pref, 1642));
            items.Add(CalendarTaskMode.CALENDAR_ITEM_USER_TASK.GetHashCode().ToString(), eResApp.GetRes(Pref, 1643));

            String value = String.IsNullOrEmpty(GetPrefVal(ePrefConst.PREF_PLANNING.CalendarTaskMode)) ? "0" : GetPrefVal(ePrefConst.PREF_PLANNING.CalendarTaskMode);

            eAdminField field = new eAdminRadioButtonField(0, "", eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.CALENDARTASKMODE.GetHashCode(), "rbDayOption", items, value: value);
            field.Generate(div);

            PgContainer.Controls.Add(h3);
            PgContainer.Controls.Add(div);
        }

        /// <summary>
        /// Génération de la partie "Couleurs de la bordure gauche d'un rendez-vous
        /// </summary>
        private void GenerateColorsPart()
        {
            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerText = eResApp.GetRes(Pref, 7132);

            Panel div = new Panel();
            div.CssClass = "partContent";

            HtmlGenericControl p = new HtmlGenericControl("p");
            p.InnerText = eResApp.GetRes(Pref, 902);

            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.ID = "rdvColors";

            HtmlGenericControl li = new HtmlGenericControl("li");

            HtmlGenericControl spanColor = new HtmlGenericControl();
            HtmlGenericControl spanText = new HtmlGenericControl();
            spanColor.Attributes.Add("class", "rdvColor");
            SetSpanAttributes(spanColor, GetPrefVal(ePrefConst.PREF_PLANNING.CalendarGripUserOwnerColor), ADMIN_PREF.CALENDARGRIPUSEROWNERCOLOR, "gripuserownercolor");
            li.Controls.Add(spanColor);
            spanText.InnerText = eResApp.GetRes(Pref, 907);
            li.Controls.Add(spanText);
            ul.Controls.Add(li);

            li = new HtmlGenericControl("li");

            spanColor = new HtmlGenericControl();
            spanText = new HtmlGenericControl();
            spanColor.Attributes.Add("class", "rdvColor");
            SetSpanAttributes(spanColor, GetPrefVal(ePrefConst.PREF_PLANNING.CalendarGripConfidentialColor), ADMIN_PREF.CALENDARGRIPCONFIDENTIALCOLOR, "gripconfidentialcolor");
            li.Controls.Add(spanColor);
            spanText.InnerText = eResApp.GetRes(Pref, 908);
            li.Controls.Add(spanText);
            ul.Controls.Add(li);

            li = new HtmlGenericControl("li");

            spanColor = new HtmlGenericControl();
            spanText = new HtmlGenericControl();
            spanColor.Attributes.Add("class", "rdvColor");
            SetSpanAttributes(spanColor, GetPrefVal(ePrefConst.PREF_PLANNING.CalendarGripMultiOwnerColor), ADMIN_PREF.CALENDARGRIPMULTIOWNERCOLOR, "gripmultiownercolor");
            li.Controls.Add(spanColor);
            spanText.InnerText = eResApp.GetRes(Pref, 906);
            li.Controls.Add(spanText);
            ul.Controls.Add(li);

            li = new HtmlGenericControl("li");

            spanColor = new HtmlGenericControl();
            spanText = new HtmlGenericControl();
            spanColor.Attributes.Add("class", "rdvColor");
            SetSpanAttributes(spanColor, GetPrefVal(ePrefConst.PREF_PLANNING.CalendarGripPublicColor), ADMIN_PREF.CALENDARGRIPPUBLICCOLOR, "grippubliccolor");
            li.Controls.Add(spanColor);
            spanText.InnerText = eResApp.GetRes(Pref, 905);
            li.Controls.Add(spanText);
            ul.Controls.Add(li);

            li = new HtmlGenericControl("li");

            spanColor = new HtmlGenericControl();
            spanText = new HtmlGenericControl();
            spanColor.Attributes.Add("class", "rdvColor");
            SetSpanAttributes(spanColor, GetPrefVal(ePrefConst.PREF_PLANNING.CalendarGripOtherConfidentialColor), ADMIN_PREF.CALENDARGRIPOTHERCONFIDENTIALCOLOR, "gripotherconfidentialcolor");
            li.Controls.Add(spanColor);
            spanText.InnerText = eResApp.GetRes(Pref, 904);
            li.Controls.Add(spanText);
            ul.Controls.Add(li);

            div.Controls.Add(p);
            div.Controls.Add(ul);

            PgContainer.Controls.Add(h3);
            PgContainer.Controls.Add(div);
        }


        /// <summary>
        /// Définit la couleur du span
        /// </summary>
        /// <param name="span"></param>
        /// <param name="color"></param>
        /// <param name="prefProp"></param>
        /// <param name="id"></param>
        private void SetSpanAttributes(HtmlGenericControl span, String color, ADMIN_PREF prefProp, String id)
        {
            span.ID = id;
            span.Attributes.Add("dsc", String.Concat(eAdminUpdateProperty.CATEGORY.PREF.GetHashCode(), "|", prefProp.GetHashCode()));
            if (!String.IsNullOrEmpty(color))
            {
                span.Style.Add("background-color", color);
                span.Attributes.Add("value", color);
            }
            else
            {
                span.Style.Add("background-color", "white");
            }

            string tooltip = eResApp.GetRes(Pref, 7591);
            if (!String.IsNullOrEmpty(tooltip))
            {
                span.Attributes.Add("title", tooltip);
            }
        }

        /// <summary>
        /// Génération de la partie "Options"
        /// </summary>
        private void GenerateOptionsPart()
        {
            HtmlGenericControl h3 = new HtmlGenericControl("h3");
            h3.InnerText = eResApp.GetRes(Pref, 444);

            Panel mainDiv = new Panel();
            mainDiv.CssClass = "partContent";
			
			HtmlGenericControl ul = new HtmlGenericControl("ul");
			ul.ID = "miscOptions";

			HtmlGenericControl li = new HtmlGenericControl("li");
            Panel subDiv = new Panel();
            subDiv.CssClass = "miscOption";
            subDiv.ID = "miscOptionCalendarConflictEnabled";
            eAdminField field = new eAdminCheckboxField(0, eResApp.GetRes(Pref, 1408), eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.CALENDARCONFLICTENABLED.GetHashCode(), value: GetPrefVal(ePrefConst.PREF_PLANNING.CalendarConflictEnabled) == "1", chkID: "chkConflictEnabled");
            field.Generate(subDiv);
            li.Controls.Add(subDiv);
			ul.Controls.Add(li);

            li = new HtmlGenericControl("li");
            subDiv = new Panel();
            subDiv.CssClass = "miscOption";
            subDiv.ID = "miscOptionCalendarTodayOnLogin";
            field = new eAdminCheckboxField(0, eResApp.GetRes(Pref, 7129), eAdminUpdateProperty.CATEGORY.PREF, ADMIN_PREF.CALENDARTODAYONLOGIN.GetHashCode(), value: GetPrefVal(ePrefConst.PREF_PLANNING.CalendarTodayOnLogin) == "1", chkID: "chkTodayOnLogin");
            field.Generate(subDiv);
            li.Controls.Add(subDiv);
            ul.Controls.Add(li);

            li = new HtmlGenericControl("li");
            subDiv = new Panel();
            subDiv.CssClass = "miscOption";
            subDiv.ID = "miscOptionHistoBypassEnabled";
            field = new eAdminCheckboxField(0, eResApp.GetRes(Pref, 7592), eAdminUpdateProperty.CATEGORY.PREF, (int)ADMIN_PREF.HISTOBYPASSENABLED, value: GetPrefVal(ePrefConst.PREF_PLANNING.HistoByPassEnabled) == "1", chkID: "chkHistoByPassEnabled");
            field.Generate(subDiv);
            li.Controls.Add(subDiv);
            ul.Controls.Add(li);

            mainDiv.Controls.Add(ul);

            PgContainer.Controls.Add(h3);
            PgContainer.Controls.Add(mainDiv);

        }
    }
}