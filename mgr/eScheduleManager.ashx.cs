using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Data;
using System.Xml;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eScheduleManager</className>
    /// <summary>Manager pour gérer la récurrence des RDV</summary>
    /// <purpose></purpose>
    /// <authors>JBE</authors>
    /// <date>2012-08</date>
    public class eScheduleManager : eEudoManager
    {
        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            #region Variables de session
            Int32 _groupId = _pref.User.UserGroupId;
            Int32 _userLevel = _pref.User.UserLevel;

            String _lang = _pref.Lang;

            Int32 _userId = _pref.User.UserId;
            String _instance = _pref.GetSqlInstance;
            String _baseName = _pref.GetBaseName;

            if (_userId == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 6572), // Récurrence des RDV
                   eResApp.GetRes(_pref, 72), // "Une erreur est survenue"
                   eResApp.GetRes(_pref, 416), //"Erreur"
                   eResApp.GetRes(_pref, 6562)); // Pas de userid

                //Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            #endregion

            int nScheduleId;	//Id de la périodicité
            int nType;			//Type de la périodicité
            int nFrequency;		//Frequence de la périodicité
            int nDay;			//Jour de la périodicité
            int nMonth;			//Mois de la périodicité
            int nOrder;			//Ordre de la périodicité
            string strWeekDay;		//Jours de la périodicité
            string strBeginDate;	//Date de début de la périodicité
            string strEndDate;		//Date de fin de la périodicité
            string strSql;			//Requête SQL
            //bool bNoEndDate;		//Indicateur de date de fin
            int nCount;
            int nTab;
            string strHour;         //Horaire
            
            //'PARAMETRES
            string sAction = _context.Request.Form["action"];
            nScheduleId = eLibTools.GetNum(_context.Request.Form["ScheduleId"]);
            RqParam rq = new RqParam();
            bool bExecuteQuery = false;

            switch (sAction)
            {
                case "update":
                    nType = eLibTools.GetNum(_context.Request.Form["ScheduleType"]);
                    nFrequency = eLibTools.GetNum(_context.Request.Form["ScheduleFrequency"]);
                    nDay = eLibTools.GetNum(_context.Request.Form["ScheduleDay"]);
                    nMonth = eLibTools.GetNum(_context.Request.Form["ScheduleMonth"]);
                    nOrder = eLibTools.GetNum(_context.Request.Form["ScheduleOrder"]);
                    strWeekDay = _context.Request.Form["ScheduleWeekDay"];
                    strBeginDate = _context.Request.Form["ScheduleRangeBegin"];
                    strEndDate = _context.Request.Form["ScheduleRangeEnd"];
                    nCount = eLibTools.GetNum(_context.Request.Form["ScheduleRangeCount"]);
                    nTab = eLibTools.GetNum(_context.Request.Form["Tab"]);
                    strHour = _context.Request.Form["ScheduleHour"];

                //Ajout ou Modification d'une périodicité
                if ((nScheduleId == 0))
                {
                    strSql = String.Concat("INSERT INTO [SCHEDULE]([Type], [Frequency], [Day], [Order], [WeekDay], [Month], [RangeBegin], [RangeEnd], [RangeCount], [Time])",
                            " VALUES (@nType, @nFrequency, @nDay, @nOrder, @strWeekDay, @nMonth, @dtBeginDate, @dtEndDate, @nCount, @tsHour);SELECT SCOPE_IDENTITY()");
                }
                else
                {
                    strSql = String.Concat("UPDATE [SCHEDULE] SET [Type]=@nType, [Frequency]=@nFrequency, [Day]=@nDay, [Order]=@nOrder,",
                        " [WeekDay]=@strWeekDay, [Month]=@nMonth, [RangeBegin]=@dtBeginDate, [RangeEnd]=@dtEndDate, [RangeCount]=@nCount, [Time]=@tsHour",
                        " WHERE [ScheduleId]=@nScheduleId");
                }

                    rq = new RqParam(strSql);
                    rq.AddInputParameter("@nType", System.Data.SqlDbType.Int, nType);
                    rq.AddInputParameter("@nFrequency", System.Data.SqlDbType.Int, nFrequency > 0 ? (object)nFrequency : DBNull.Value);
                    rq.AddInputParameter("@nDay", System.Data.SqlDbType.Int, nDay > 0 ? (object)nDay : DBNull.Value);
                    rq.AddInputParameter("@nOrder", System.Data.SqlDbType.Int, nOrder > 0 ? (object)nOrder : DBNull.Value);
                    rq.AddInputParameter("@strWeekDay", System.Data.SqlDbType.VarChar, !String.IsNullOrEmpty(strWeekDay) ? (object)strWeekDay : DBNull.Value);
                    rq.AddInputParameter("@nMonth", System.Data.SqlDbType.Int, nMonth > 0 ? (object)nMonth : DBNull.Value);
                    rq.AddInputParameter("@dtBeginDate", System.Data.SqlDbType.DateTime, !String.IsNullOrEmpty(strBeginDate) ? (object)DateTime.Parse(strBeginDate) : DBNull.Value);
                    rq.AddInputParameter("@dtEndDate", System.Data.SqlDbType.DateTime, !String.IsNullOrEmpty(strEndDate) ? (object)DateTime.Parse(strEndDate) : DBNull.Value);
                    rq.AddInputParameter("@nCount", System.Data.SqlDbType.Int, nCount > 0 ? (object)nCount : DBNull.Value);
                    rq.AddInputParameter("@tsHour", System.Data.SqlDbType.Time, !String.IsNullOrEmpty(strHour) ? (object)TimeSpan.Parse(strHour) : DBNull.Value);

                    bExecuteQuery = true;

                    break;
                case "delete":
                    rq = new RqParam("DELETE FROM [SCHEDULE] WHERE [ScheduleId]=@nScheduleId");
                    bExecuteQuery = true;

                    break;
            }
            if (sAction == "update")
            {
                
            }


            if (sAction == "delete" && nScheduleId > 0)
            {
                
            }

            if (nScheduleId > 0)
            {
                rq.AddInputParameter("@nScheduleId", System.Data.SqlDbType.Int, nScheduleId);
            }

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();

            string scheduleInfo = null;
            string err = string.Empty;
            try
            {
                if (bExecuteQuery)
                {
                    DataTableReaderTuned dtr = dal.Execute(rq, out err);

                    if (err.Length > 0)
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref, 6572), // Récurrence des RDV
                            eResApp.GetRes(_pref, 72), // "Une erreur est survenue"
                            eResApp.GetRes(_pref, 416), //"Erreur"
                            err);

                        //Arrete le traitement et envoi l'erreur
                        LaunchError();
                    }


                    if (nScheduleId == 0) //on le créée
                    {
                        dtr.Read();
                        nScheduleId = dtr.GetEudoNumeric(0);
                    }
                    dtr?.Dispose();

                    try
                    {
                        scheduleInfo = eScheduleInfos.GetScheduleLabelFromBase(0, _pref, nScheduleId);
                    }
                    catch {
                        // Chargement impossible
                        scheduleInfo = eResApp.GetRes(_pref, 8177);
                    }
                }

                XmlDocument xmlResult = new XmlDocument();
                xmlResult.AppendChild(xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null));
                XmlNode baseResultNode = xmlResult.CreateElement("result");
                xmlResult.AppendChild(baseResultNode);

                XmlNode scheduleIdNode = xmlResult.CreateElement("scheduleid");
                scheduleIdNode.InnerText = nScheduleId.ToString();
                baseResultNode.AppendChild(scheduleIdNode);

                XmlNode scheduleInfoNode = xmlResult.CreateElement("scheduleinfo");
                scheduleInfoNode.InnerText = scheduleInfo;
                baseResultNode.AppendChild(scheduleInfoNode);

                _context.Response.Clear();
                _context.Response.ClearContent();

                RenderResult(RequestContentType.XML, delegate() { return xmlResult.OuterXml; });
            }
            finally
            {
                dal.CloseDatabase();

            }

        }
    }
}