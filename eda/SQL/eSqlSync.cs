using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda
{
    public class eSqlSync
    {

        public static bool IsFieldImportForbidden(ePref pref, int descid)
        {
            bool returnValue = false;

            eudoDAL dal = eLibTools.GetEudoDAL(pref);
            try
            {
                dal.OpenDatabase();

                string sSql = "SELECT CAST(CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END AS BIT) FROM [SyncField] WHERE [DescId] = @DescId";

                RqParam rq = new RqParam(sSql);
                rq.AddInputParameter("@DescId", SqlDbType.Int, descid);

                string sError = String.Empty;
                returnValue = dal.ExecuteScalar<bool>(rq, out sError);

                if (sError.Length > 0)
                    throw new Exception(sError);
            }
            catch (Exception e)
            {
                throw new Exception(String.Concat("eSqlSync.IsFieldImportForbidden error :", e.Message));
            }
            finally
            {
                dal.CloseDatabase();
            }

            return returnValue;
        }



        public static void UpdateFieldImportForbidden(ePref pref, int descid, bool forbidden)
        {
            eudoDAL dal = eLibTools.GetEudoDAL(pref);

            // Nettoyage de la table [SyncField]
            RqParam rqDeleteSyncField = new RqParam("DELETE FROM [SyncField] WHERE [DescId] = @DescId");
            rqDeleteSyncField.AddInputParameter("@DescId", SqlDbType.Int, descid);

            // Recréation des enregistrements dans [SyncField]
            string sqlInsertSyncField = String.Concat("INSERT INTO [SyncField] ( [DescId], [SyncId] )"
                                                        , " SELECT @DescId, [SyncId]"
                                                        , " FROM [SYNCHRO]"
                                                        , " WHERE [SyncType] <> @typSyncPressReview");
            RqParam rqInsertSyncField = new RqParam(sqlInsertSyncField);
            rqInsertSyncField.AddInputParameter("@DescId", SqlDbType.Int, descid);
            rqInsertSyncField.AddInputParameter("@typSyncPressReview", SqlDbType.Int, SynchroType.TYP_SYNC_PRESS_REVIEW);

            // Nettoyage de la table [SyncLastUpdate]
            RqParam rqDeleteSyncLastUpdate = new RqParam("DELETE FROM [SyncLastUpdate] WHERE [DescId] = @DescId");
            rqDeleteSyncLastUpdate.AddInputParameter("@DescId", SqlDbType.Int, descid);

            // Nettoyage de la table [SyncPriority]
            RqParam rqDeleteSyncPriority = new RqParam("DELETE FROM [SyncPriority] WHERE [DescId] = @DescId");
            rqDeleteSyncPriority.AddInputParameter("@DescId", SqlDbType.Int, descid);


            try
            {
                dal.OpenDatabase();

                String eError;
                dal.StartTransaction(out eError);
                if (eError.Length > 0)
                    throw dal.InnerException ?? new Exception(eError);

                // Nettoyage de la table [SyncField]
                dal.AddToTransaction(rqDeleteSyncField, out eError);
                if (eError.Length > 0)
                    throw dal.InnerException ?? new Exception(eError);

                dal.ExecuteNonQuery(rqDeleteSyncField, out eError);
                if (eError.Length > 0)
                    throw dal.InnerException ?? new Exception(eError);

                if (forbidden)
                {
                    // Recréation des enregistrements dans [SyncField]
                    dal.AddToTransaction(rqInsertSyncField, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);

                    dal.ExecuteNonQuery(rqInsertSyncField, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);
                }
                else
                {
                    // Nettoyage de la table [SyncLastUpdate]
                    dal.AddToTransaction(rqDeleteSyncLastUpdate, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);

                    dal.ExecuteNonQuery(rqDeleteSyncLastUpdate, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);

                    // Nettoyage de la table [SyncPriority]
                    dal.AddToTransaction(rqDeleteSyncPriority, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);

                    dal.ExecuteNonQuery(rqDeleteSyncPriority, out eError);
                    if (eError.Length > 0)
                        throw dal.InnerException ?? new Exception(eError);
                }

                // commit de trasaction
                dal.CommitTransaction(out eError);
                if (eError.Length > 0)
                    throw dal.InnerException ?? new Exception(eError);
            }
            catch (Exception)
            {
                String sRollError;
                dal.RollBackTransaction(out sRollError);
                if (sRollError.Length > 0)
                    throw dal.InnerException ?? new Exception(sRollError);
            }
            finally { dal?.CloseDatabase(); }
        }

    }


  
}