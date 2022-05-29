//using EudoQuery;
//using System;

//namespace Com.Eudonet.Xrm.eda.SQL
//{


//    public static class eSqlConfig
//    {

//        /// <summary>
//        /// Réinitialise l'ordre d'affichage des onglets pour l'utilisateur
//        /// </summary>
//        /// <param name="eDal">Connexion ouverte</param>
//        /// <param name="userID">Utilisateur</param>
//        public static Boolean ResetTabOrder(eudoDAL eDal, int userID)
//        {
//            String error = String.Empty;
//            String query = String.Concat("UPDATE [CONFIG] SET [TabOrder] = NULL, [TABORDERID] = ISNULL( (",
//                    "SELECT TOP 1 [SELECTID] FROM [SELECTIONS] WHERE [TAB] = 0 AND USERID = @userid AND ISNULL([DEFAULTSELECTID],0) <> 0 ORDER BY [SELECTID] desc",
//                "),[TABORDERID]) WHERE UserId = @userid");

//            RqParam rqParam = new RqParam(query);
//            rqParam.AddInputParameter("@userid", System.Data.SqlDbType.Int, userID);

//            int result = eDal.ExecuteNonQuery(rqParam, out error);

//            if (result < 1)
//                return false;

//            if (error.Length != 0)
//                throw new Exception(String.Concat("eSqlConfig.ResetTabOrder", error));

//            return true;
//        }
        
//    }
//}