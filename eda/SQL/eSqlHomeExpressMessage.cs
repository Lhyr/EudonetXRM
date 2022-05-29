using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eSqlHomeExpressMessage
    {
        /// <summary>
        /// Récupère la liste des homepages
        /// </summary>
        /// <param name="pref">pref nécessaire au constructeur de HomePage</param>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="sError">Erreur</param>
        /// <returns></returns>
        public static List<eAdminHomeExpressMessage> GetExpressMessage(ePref pref, eudoDAL eDal, out String sError)
        {
            String sSQL = String.Empty;
            //String sTypes = String.Join(",", types.Select(t => t.GetHashCode()));
            List<eAdminHomeExpressMessage> listHP = new List<eAdminHomeExpressMessage>();
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            try
            {
                sSQL = String.Concat(@"SELECT HpgId, Libelle, ISNULL(UserList, '') + CASE WHEN GroupList IS NULL THEN '' ELSE ';' + ISNULL(GroupList, '') END AS[UserId], dbo.getUserDisplay(ISNULL(UserList, '') + ';' + ISNULL(GroupList, ''), ';') AS[UsersDisplay]
                  FROM(
                        SELECT hp.HpgId
                        , hp.Libelle
                        , STUFF((
                                SELECT DISTINCT ';' + CAST(USERID AS VARCHAR)

                                FROM CONFIG

                                WHERE ExpressMsg = hp.HpgId

                                FOR XML PATH('')
                                ), 1, 1, '') AS UserList
                        , STUFF((
                                SELECT DISTINCT ';' + 'G' + CAST(GroupId AS VARCHAR)

                                FROM[GROUP]

                                WHERE ExpressMessageId = hp.HpgId

                                FOR XML PATH('')
                                ), 1, 1, '') AS GroupList

                    FROM [HOMEPAGE] hp WHERE ISNULL([Type],0) = ", (int)eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS, ") TEMP ORDER BY Libelle");
                RqParam rq = new RqParam(sSQL);
                dtr = eDal.Execute(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
                while (dtr.Read())
                {
                    listHP.Add(new eAdminHomeExpressMessage(pref, dtr.GetEudoNumeric("HpgId"), dtr.GetString("Libelle"), dtr.GetString("UserId").TrimEnd(';'), dtr.GetString("UsersDisplay").TrimEnd(';')));
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
        /// Retourne le message express de l'utilisateur
        /// </summary>
        /// <param name="pref">ePref</param>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="sError">Erreur</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static eAdminHomeExpressMessage GetExpressMessageByUserID(ePref pref, eudoDAL eDal, out String sError)
        {
            String sSQL = String.Empty;
            //String sTypes = String.Join(",", types.Select(t => t.GetHashCode()));
            eAdminHomeExpressMessage expressMessage = null;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            int userID = pref.UserId;

            try
            {
                sSQL = String.Concat(@"
                SELECT TOP 1 HpgId
	                ,Libelle
	                ,Value
                FROM [HOMEPAGE]
                INNER JOIN [CONFIG] ON [CONFIG].ExpressMsg = HpgId
                WHERE UserId IN (0, @userid)
                ORDER BY USERID DESC
                ");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@userid", System.Data.SqlDbType.Int, userID);
                dtr = eDal.Execute(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
                if (dtr.Read())
                {
                    expressMessage = new eAdminHomeExpressMessage(pref, dtr.GetEudoNumeric("HpgId"), dtr.GetString("Libelle"), userID.ToString(), pref.User.UserDisplayName, dtr.GetString("Value"));
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

            return expressMessage;
        }

        /// <summary>
        /// Récupère la liste des homepages
        /// </summary>
        /// <param name="pref">pref nécessaire au constructeur de HomePage</param>
        /// <param name="eDal">eudoDAL</param>
        /// <param name="sError">Erreur</param>
        /// <returns></returns>
        public static eAdminHomepage GetExpressMessageByID(ePref pref, int Id, eudoDAL eDal, out String sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;
            eAdminHomepage hmEm = null;
            try
            {
                sSQL = String.Concat(@"SELECT HpgId, Libelle,Value, ISNULL(UserList, '') + CASE WHEN GroupList IS NULL THEN '' ELSE ';' + ISNULL(GroupList, '') END AS[UserId], dbo.getUserDisplay(ISNULL(UserList, '') + ';' + ISNULL(GroupList, ''), ';') AS[UsersDisplay]
                  FROM(
                        SELECT hp.HpgId
                        , hp.Libelle
                        , hp.Value
                        , STUFF((
                                SELECT DISTINCT ';' + CAST(USERID AS VARCHAR)

                                FROM CONFIG

                                WHERE ExpressMsg = hp.HpgId

                                FOR XML PATH('')
                                ), 1, 1, '') AS UserList
                        , STUFF((
                                SELECT DISTINCT ';' + 'G' + CAST(GroupId AS VARCHAR)

                                FROM[GROUP]

                                WHERE ExpressMessageId = hp.HpgId

                                FOR XML PATH('')
                                ), 1, 1, '') AS GroupList

                    FROM [HOMEPAGE] hp WHERE ISNULL([Type],0) = ", (int)eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS,
                    "  AND ISNULL(HpgId,0) = ", Id, " ) TEMP ORDER BY Libelle");
                RqParam rq = new RqParam(sSQL);
                dtr = eDal.Execute(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
                while (dtr.Read())
                {
                    hmEm = new eAdminHomepage(pref, dtr.GetEudoNumeric("HpgId"), dtr.GetString("Libelle"), dtr.GetString("UserId").TrimEnd(';'), dtr.GetString("UsersDisplay").TrimEnd(';'), content: dtr.GetString("Value").TrimEnd(';'));
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

            return hmEm;
        }


        /// <summary>
        /// Suppression d'un message expresse
        /// </summary>
        /// <param name="eDal">objet dal de connexion</param>
        /// <param name="id">Id du message expresse </param>
        /// <param name="sError">Erreur du retour</param>
        internal static void DeleteExpressMessage(eudoDAL eDal, int id, out string sError)
        {
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = String.Concat("DELETE FROM [HOMEPAGE] WHERE HpgId = @id; ",
                    "UPDATE [CONFIG] SET ExpressMsg = NULL WHERE ExpressMsg = @id;");
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
        /// Ajouter un mesage express
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="hpEm"></param>
        /// <param name="sError"></param>
        internal static void AddExpressMessage(eudoDAL eDal, eAdminHomepage hpEm, out string sError)
        {
            String sSQL = String.Empty;
            int result = 0;
            try
            {
                sSQL = String.Concat("INSERT INTO [HOMEPAGE]([Libelle],[Type],[Value])",
                    " Values (@libelle,", (int)eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS, ",@value); SELECT @id= scope_identity();");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@libelle", System.Data.SqlDbType.NVarChar, hpEm.Label);
                rq.AddInputParameter("@value", System.Data.SqlDbType.NVarChar, hpEm.Content);
                rq.AddOutputParameter("@id", System.Data.SqlDbType.Int, 18);
                result = eDal.ExecuteNonQuery(rq, out sError);
                hpEm.Id = eLibTools.GetNum(rq.GetParamValue("@id").ToString());

                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
        }


        /// <summary>
        /// Ajouter un mesage express
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="hpEm"></param>
        /// <param name="sError"></param>
        internal static void UpdateHomepageExpressMessage(eudoDAL eDal, eAdminHomepage hpEm, out string sError)
        {
            String sSQL = String.Empty;
            int result = 0;
            try
            {
                sSQL = String.Concat("UPDATE [HOMEPAGE] SET [Libelle] = @libelle ,[Value] = @value WHERE [Type] =  ",
                                    (int)eConst.HOMEPAGE_TYPE.HPG_MSG_EXPRESS,
                                    " AND [HpgId] = @id ;");
                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@libelle", System.Data.SqlDbType.NVarChar, hpEm.Label);
                rq.AddInputParameter("@value", System.Data.SqlDbType.NVarChar, hpEm.Content);
                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, hpEm.Id);
                result = eDal.ExecuteNonQuery(rq, out sError);

                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
        }

        /// <summary>
        /// Mise à jour des utilisateurs autorisés pour un message express
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <param name="sError"></param>
        public static void UpdateHomepageExpressMessageUsers(eudoDAL eDal, int id, String value, out String sError)
        {
            sError = String.Empty;
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                int nUser;

                // On vide 
                UpdateHomepageExpressMessageEmptyById(eDal, id, out sError);
                if (!String.IsNullOrEmpty(value))
                {
                    String[] arrUsers = value.Split(';');
                    foreach (String user in arrUsers)
                    {
                        if (user.IndexOf('G') >= 0)
                        {
                            sSQL = String.Concat("UPDATE [GROUP] SET [ExpressMessageId] = @id  WHERE [GROUP].[GroupId] = @groupid");
                            RqParam rq = new RqParam(sSQL);
                            rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                            rq.AddInputParameter("@groupid", System.Data.SqlDbType.Int, user.Remove(0, 1));
                            result = eDal.ExecuteNonQuery(rq, out sError);
                        }
                        else
                        {
                            if (int.TryParse(user, out nUser))
                            {
                                sSQL = " UPDATE [CONFIG] SET [ExpressMsg] = @id WHERE UserId = @userid;";
                                RqParam rq = new RqParam(sSQL);
                                rq.AddInputParameter("@id", System.Data.SqlDbType.Int, id);
                                rq.AddInputParameter("@userid", System.Data.SqlDbType.Int, nUser);
                                result = eDal.ExecuteNonQuery(rq, out sError);

                            }
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
        /// mettre à null les utilisateurs d'um message express
        /// </summary>
        /// <param name="eDal"></param>
        /// <param name="id">Id du message express</param>
        /// <param name="sError"></param>
        public static void UpdateHomepageExpressMessageEmptyById(eudoDAL eDal, int id, out String sError)
        {
            sError = String.Empty;
            String sSQL = String.Empty;
            int result = 0;

            try
            {
                sSQL = "UPDATE [CONFIG] SET [ExpressMsg] = NULL WHERE ExpressMsg = @id; UPDATE [GROUP] SET [ExpressMessageId] = NULL WHERE ExpressMessageId = @id;";
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
    }
}