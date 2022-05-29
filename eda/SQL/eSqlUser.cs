using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;

namespace Com.Eudonet.Xrm.eda
{
    public class eSqlUser
    {
        /// <summary>
        /// Retourne la liste de tous les utilisateurs qui ne sont pas des profils
        /// </summary>
        /// <param name="eDal">Connexion ouverte</param>
        /// <returns></returns>
        public static List<int> GetNonProfileUsersList(eudoDAL eDal)
        {
            String error = String.Empty;
            List<int> listUsers = new List<int>();

            String query = "SELECT UserId FROM [USER] WHERE ISNULL(IsProfile, 0) = 0";

            RqParam rq = new RqParam(query);

            DataTableReaderTuned dtr = eDal.Execute(rq, out error);

            if (String.IsNullOrEmpty(error))
            {
                while (dtr.Read())
                {
                    listUsers.Add(dtr.GetEudoNumeric("UserId"));
                }
            }
            else
            {
                throw new EudoAdminSQLException("eSqlUser.GetNonProfileUsersList", error);
            }

            return listUsers;
        }

        /// <summary>
        /// retourne les apparatenance par défaut d'une table
        /// </summary>
        /// <param name="eDal">eudoDAL ouverte</param>
        /// <param name="usersAndGroups">Liste des utilisateurs et groupes séparés par ;</param>
        /// <returns></returns>
        public static string GetUserDisplay(eudoDAL eDal, string usersAndGroups)
        {

            String sError = String.Empty;
            String sSQL = String.Empty;
            string userDisplay = usersAndGroups;

            sSQL = $"SELECT [dbo].[getUserDisplay]('{usersAndGroups}', ';') AS [UserDisplay]";

            RqParam rq = new RqParam(sSQL);
            userDisplay = eDal.ExecuteScalar<string>(rq, out sError);

            if (sError.Length > 0) { throw new Exception("eSqlUser.GetUserDisplay => " + sError); }

            return userDisplay;
        }
    }
}