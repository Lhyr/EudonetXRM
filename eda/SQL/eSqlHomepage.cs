using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eSqlHomepage
    {
        /// <summary>
        /// Récupère la liste des homepages
        /// </summary>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="sError">Erreur</param>
        /// <returns></returns>
        public static List<eAdminHomepage> GetHomepages(ePref pref, eudoDAL eDal, out String sError)
        {
            String sSQL = String.Empty;

            //String sTypes = String.Join(",", types.Select(t => t.GetHashCode()));
            List<eAdminHomepage> listHP = new List<eAdminHomepage>();
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            try
            {
                sSQL = String.Concat(@"
SELECT homepageid
	, Homepagetitle
	, Tooltip
	, ISNULL(UserList, '') + CASE WHEN GroupList IS NULL THEN '' ELSE ';' + ISNULL(GroupList, '') END AS [UserId]
	, dbo.getUserDisplay(ISNULL(UserList, '') + ';' + ISNULL(GroupList, ''), ';') AS [UserDisplayName]
FROM (
	SELECT hp.homepageid
		, hp.homepagetitle
		, hp.tooltip
		, STUFF((
				SELECT DISTINCT ';' + CAST(USERID AS VARCHAR)
				FROM CONFIG
				WHERE AdvancedHomePageId = hp.Homepageid
				FOR XML PATH('')
				), 1, 1, '') AS UserList
		, STUFF((
				SELECT DISTINCT ';' + 'G' + CAST(GroupId AS VARCHAR)
				FROM [GROUP]
				WHERE AdvancedHomePageId = hp.Homepageid
				FOR XML PATH('')
				), 1, 1, '') AS GroupList
	FROM [HOMEPAGEADVANCED] hp
	) TEMP
ORDER BY Homepagetitle

    ");
                RqParam rq = new RqParam(sSQL);
                dtr = eDal.Execute(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
                while (dtr.Read())
                {
                    listHP.Add(new eAdminHomepage(pref, dtr.GetEudoNumeric("HomePageId"), dtr.GetString("HomePageTitle"), dtr.GetString("UserId"), dtr.GetString("UserDisplayName"), dtr.GetString("Tooltip")));
                }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
            finally
            {
                if (dtr != null)
                    dtr.Dispose();
            }

            return listHP;
        }

        /// <summary>
        /// Mise à jour de la table [HOMEPAGEADVANCED]
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="prop"></param>
        /// <param name="value"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public static Boolean UpdateHomepage(eudoDAL eDal, int id, String prop, String value, out String sError)
        {
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = String.Concat("UPDATE [HOMEPAGEADVANCED] SET [", prop, "] = @value WHERE HomePageId = @id");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@value", System.Data.SqlDbType.NVarChar, value);
                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                result = eDal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }

            if (result > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Mise à jour des utilisateurs autorisés pour une page d'accueil
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="sError"></param>
        public static void UpdateHomepageUsers(eudoDAL eDal, int id, String value, out String sError)
        {
            sError = String.Empty;
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                int nUser;
                //On vide
                UpdateHomepageEmptyById(eDal, id, out sError);
                if (!String.IsNullOrEmpty(value))
                {
                    String[] arrUsers = value.Split(';');
                    foreach (String user in arrUsers)
                    {
                        if (user.IndexOf('G') >= 0)
                        {
                            sSQL = String.Concat("UPDATE [GROUP] SET [AdvancedHomePageId] = @id WHERE GroupId = @groupid");
                            RqParam rq = new RqParam(sSQL);
                            rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                            rq.AddInputParameter("@groupid", System.Data.SqlDbType.Int, user.Remove(0, 1));
                            result = eDal.ExecuteNonQuery(rq, out sError);
                        }
                        else if (int.TryParse(user, out nUser))
                        {
                            sSQL = String.Concat("UPDATE [CONFIG] SET [AdvancedHomePageId] = @id WHERE UserId = @userid");
                            RqParam rq = new RqParam(sSQL);
                            rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                            rq.AddInputParameter("@userid", System.Data.SqlDbType.Int, nUser);
                            result = eDal.ExecuteNonQuery(rq, out sError);

                        }


                        if (sError.Length > 0) { throw new Exception(sError); }

                    }
                }

            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
        }


        /// <summary>
        /// mettre à null les utilisateurs d'une page d'accueil
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id">Id de la page d'accueil</param>
        /// <param name="sError"></param>
        public static void UpdateHomepageEmptyById(eudoDAL eDal, int id, out String sError)
        {
            sError = String.Empty;
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = String.Concat("UPDATE [CONFIG] SET [AdvancedHomePageId] = NULL WHERE AdvancedHomePageId = @id; ",
                         "UPDATE [GROUP] SET [AdvancedHomePageId] = NULL WHERE AdvancedHomePageId = @id");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                result = eDal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
        }

        /// <summary>
        /// Suppression de la page d'accueil
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="sError"></param>
        internal static void DeleteHomepage(eudoDAL eDal, int id, out string sError)
        {
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = String.Concat("DELETE FROM [HOMEPAGEADVANCED] WHERE HomePageId = @id; ",
                    "DELETE FROM [EUDOPARTRELATION] WHERE HomePageId = @id; ",
                    "UPDATE [CONFIG] SET AdvancedHomePageId = NULL WHERE AdvancedHomePageId = @id;",
                    "UPDATE [GROUP] SET AdvancedHomePageId = NULL WHERE AdvancedHomePageId = @id;");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                result = eDal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
        }

        /// <summary>
        /// Dupliquer une page d'accueil
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="clonePrefix"></param>
        /// <param name="sError"></param>
        /// <returns></returns>
        public static Boolean CloneHomepage(eudoDAL eDal, int id, String clonePrefix, out String sError)
        {
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = String.Concat("INSERT INTO [HOMEPAGEADVANCED] (HomepageTitle, HomepageConfig, Userid, Locked, HideHeaderEnabled, Tooltip) ",
                    "SELECT '", clonePrefix, "' + '[' + HomepageTitle + ']', HomepageConfig, Userid, Locked, HideHeaderEnabled, Tooltip FROM [HOMEPAGEADVANCED] WHERE HomePageId = @id");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                result = eDal.ExecuteNonQuery(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }

            if (result > 0)
                return true;
            return false;
        }
    }
}