using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe de gestion des Plannifications (table Schedule)
    /// </summary>
    public class eScheduleObject
    {
        /// <summary></summary>
        private const int COUNT_DEFAULT = (int)eLibConst.SCHEDULE_FREQUENCY.SCHEDULE_DEFAULT_RANGE_COUNT;

        #region Propriétés de la plannification
        /// <summary>Id Fiche Schedule</summary>
        private int _nScheduleId = 0;

        /// <summary>Type (quotidien/hebdomadaire/mensuel...)</summary>
        private eLibConst.SCHEDULE_FREQUENCY _nType;

        /// <summary>Fréquence</summary>
        private int _nFrequency;

        /// <summary>Jour (mode mensuel)</summary>
        private int _nDay;

        /// <summary>Mois (mode annuel)</summary>
        private int _nMonth;

        /// <summary>Ordre</summary>
        private int _nOrder;

        /// <summary>Jour de la semaine</summary>
        HashSet<int> _hsWeekDays = new HashSet<int>();

        /// <summary>Date de début</summary>
        private DateTime _dtBeginDate;

        /// <summary>Date de fin</summary>
        private DateTime? _dtEndDate;

        /// <summary>Nbre d'occurence</summary>
        private int _nRangeCount = 0;

        /// <summary>Heure</summary>
        private TimeSpan? _tsTime = new TimeSpan(22, 0, 0);
        #endregion

        #region Accesseurs

        /// <summary>Id Fiche Schedule</summary>
        public int ScheduleId
        {
            get { return _nScheduleId; }
        }

        /// <summary>Type (quotidien/hebdomadaire/mensuel...)</summary>
        public eLibConst.SCHEDULE_FREQUENCY Type
        {
            get { return _nType; }
        }

        /// <summary>Fréquence</summary>
        public int Frequency
        {
            get { return _nFrequency; }
        }

        /// <summary>Jour (mode mensuel)</summary>
        public int Day
        {
            get { return _nDay; }
        }

        /// <summary>Mois (mode annuel)</summary>
        public int Month
        {
            get { return _nMonth; }
        }

        /// <summary>Ordre</summary>
        public int Order
        {
            get { return _nOrder; }
        }

        /// <summary>Jour de la semaine</summary>
        public IEnumerable<int> WeekDays
        {
            get { return _hsWeekDays; }
        }

        /// <summary>Date de début</summary>
        public DateTime BeginDate
        {
            get { return _dtBeginDate; }
        }

        /// <summary>Date de fin</summary>
        public DateTime? EndDate
        {
            get { return _dtEndDate; }
        }

        /// <summary>Nbre d'occurence</summary>
        public int RangeCount
        {
            get { return _nRangeCount; }
        }

        /// <summary>Heure</summary>
        public TimeSpan? Time
        {
            get { return _tsTime; }
        }


        #endregion

        /// <summary>
        /// Contructeur par défaut
        /// </summary>
        /// <param name="scheduleId">id de la plannification</param>
        public eScheduleObject(int scheduleId = 0)
        {
            _nScheduleId = scheduleId;
        }


        /// <summary>
        /// Chargement des infos depuis la BDD
        /// </summary>
        /// <param name="pref">Préférences</param>
        /// <param name="initDummyIfNotFound">Si la fiche n'existe pas, Initialise un object avec les paramètres par défaut, sinon lève une exception</param>
        /// <param name="initDummyDateTime">Date par defaut</param>
        public void LoadFromDB(ePref pref, bool initDummyIfNotFound = true, DateTime? initDummyDateTime = null)
        {
            eudoDAL dal = null;
            DataTableReaderTuned dtr = null;
            try
            {
                if (_nScheduleId != 0)
                {
                    RqParam sql = new RqParam("SELECT * FROM [SCHEDULE] WHERE [ScheduleId] = @ScheduleId");
                    sql.AddInputParameter("@ScheduleId", SqlDbType.Int, this._nScheduleId);

                    dal = eLibTools.GetEudoDAL(pref);
                    dal.OpenDatabase();

                    string err = String.Empty;
                    dtr = dal.Execute(sql, out err);
                    if (err.Length > 0)
                        throw new Exception(String.Concat("erreur lors de l'execution de la requete : ", sql.Cmd.CommandText, " - ", err));
                }

                if (dtr != null && dtr.Read())
                {
                    _nType = (eLibConst.SCHEDULE_FREQUENCY)dtr.GetEudoNumeric("type");
                    _nFrequency = dtr.GetEudoNumeric("Frequency");
                    _nDay = dtr.GetEudoNumeric("Day");
                    _nOrder = dtr.GetEudoNumeric("Order");                    
                    _nMonth = dtr.GetEudoNumeric("Month");
                    _dtBeginDate = dtr.GetDateTime("RangeBegin");
                    _dtEndDate = dtr.GetDateTimeNullable("RangeEnd");
                    _nRangeCount = dtr.GetEudoNumeric("RangeCount");
                    _tsTime = dtr.GetTimeSpanNullable("Time");

                    _hsWeekDays = new HashSet<int>();
                    foreach (string strWeekDay in dtr.GetString("WeekDay").Split(';'))
                    {
                        int nWeekDay = -1;
                        if (int.TryParse(strWeekDay, out nWeekDay))
                            _hsWeekDays.UnionWith(new HashSet<int>() { nWeekDay });
                    }
                }
                else
                {
                    if (initDummyIfNotFound)
                        InitDummy(initDummyDateTime);
                    else
                        throw new Exception(String.Format("La fiche SCHEDULE avec l'Id {0} n'existe pas.", _nScheduleId));
                }
            }
            catch(Exception ex)
            {
                throw new Exception(String.Concat("eScheduleObject.LoadFromDB : ", ex.Message), ex);
            }
            finally
            {
                dtr?.Dispose();
                dal?.CloseDatabase();
            }
        }

        /// <summary>
        /// Initialise un Schedule avec des paramètres par défaut
        /// </summary>
        private void InitDummy(DateTime? initDummyDateTime = null)
        {
            if (!initDummyDateTime.HasValue)
                initDummyDateTime = DateTime.Now;

            _nType = 0;
            _nFrequency = 1;
            _nDay = initDummyDateTime.Value.Day;

            if (_nDay <= 7)
                _nOrder = 1;
            else
                if (_nDay <= 14)
                _nOrder = 2;
            else
                    if (_nDay <= 21)
                _nOrder = 3;
            else
                _nOrder = 4;

            _dtBeginDate = initDummyDateTime.Value;

            _nMonth = _dtBeginDate.Month;
            _dtEndDate = null;
            _nRangeCount = COUNT_DEFAULT;

            _hsWeekDays = new HashSet<int>();
            _hsWeekDays.UnionWith(new HashSet<int>() { _dtBeginDate.DayOfWeek.GetHashCode() + 1 });
        }
    }
}