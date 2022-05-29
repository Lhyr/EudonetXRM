using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda
{
    public class eSqlVCardMapping
    {
        /// <summary>
        /// Récupère le mapping VCard
        /// </summary>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns></returns>
        public static string GetMapping(eudoDAL eDal, out string sError)
        {
            String sSQL = String.Empty;
            sError = String.Empty;

            string sMapping = String.Empty;

            try
            {
                sSQL = String.Concat("SELECT isnull([VCardMapping], '') FROM [CONFIG] WHERE [UserId] = 0");
                RqParam rq = new RqParam(sSQL);

                sMapping = eDal.ExecuteScalar<string>(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }
            }
            catch (Exception e)
            {
                sError = String.Concat(e.Message, Environment.NewLine, sSQL, Environment.NewLine, e.StackTrace);
            }
            finally
            {
                
            }

            return sMapping;
        }

        /// <summary>
        /// Récupère la liste des champs pour les listes déroulantes
        /// </summary>
        /// <param name="eDal">Objet eudoDAL</param>
        /// <param name="sError">Erreur éventuellement rencontrée</param>
        /// <returns></returns>
        public static Dictionary<int, string> GetFieldsList(eudoDAL eDal, out string sError)
        {
            String sSQL = String.Empty;
            DataTableReaderTuned dtr = null;
            sError = String.Empty;

            Dictionary<Int32, string> dicFields = new Dictionary<int, string>();

            try
            {
                sSQL = String.Concat("SELECT [field].[DescId], [tab_res].[LANG_00] + '.' + [field_res].[LANG_00] AS [Label]"
                , Environment.NewLine, "FROM [DESC] AS[field]"
                , Environment.NewLine, "INNER JOIN[RES] AS[field_res] ON[field_res].[ResId] = [field].[DescId]"
                , Environment.NewLine, "INNER JOIN[DESC] AS[tab] on[tab].[DescId] = ([field].[DescId] - ([field].[DescId] % 100))"
                , Environment.NewLine, "INNER JOIN[RES] AS[tab_res] ON[tab_res].[ResId] = [tab].[DescId]"
                , Environment.NewLine, "WHERE [field].[DescId] % 100 <> 0"
                , Environment.NewLine, "AND [field].[DescId] BETWEEN @minDescId AND @maxDescId"
                , Environment.NewLine, "AND ("
                , Environment.NewLine, "    [field].[DescId] % 100 < @maxNbreField"
                , Environment.NewLine, "    OR [field].[DescId] IN(@ppInfos, @ppNotes, @pmInfos, @pmNotes, @adrNotes)"
                , Environment.NewLine, "    )"
                , Environment.NewLine, "AND [field].[Format] <> @fieldFormat_Title"
                , Environment.NewLine, "AND ("
                , Environment.NewLine, "        ("
                , Environment.NewLine, "        [field].[Popup] = @popupType_DATA"
                , Environment.NewLine, "        AND isnull([field].[Multiple], 0) = 0"
                , Environment.NewLine, "        )"
                , Environment.NewLine, "    OR [field].[Popup] <> @popupType_DATA"
                , Environment.NewLine, "    )"
                , Environment.NewLine, "ORDER BY[Label] ASC");


                RqParam rq = new RqParam(sSQL);
                rq.AddInputParameter("@ppInfos", System.Data.SqlDbType.Int, (int)TableType.PP + (int)AllField.MEMO_INFOS);
                rq.AddInputParameter("@ppNotes", System.Data.SqlDbType.Int, (int)TableType.PP + (int)AllField.MEMO_NOTES);
                rq.AddInputParameter("@pmInfos", System.Data.SqlDbType.Int, (int)TableType.PM + (int)AllField.MEMO_INFOS);
                rq.AddInputParameter("@pmNotes", System.Data.SqlDbType.Int, (int)TableType.PM + (int)AllField.MEMO_NOTES);
                rq.AddInputParameter("@adrNotes", System.Data.SqlDbType.Int, (int)TableType.ADR + (int)AllField.MEMO_NOTES);
                rq.AddInputParameter("@minDescId", System.Data.SqlDbType.Int, (int)TableType.PP);
                rq.AddInputParameter("@maxDescId", System.Data.SqlDbType.Int, (int)TableType.ADR + 99);
                rq.AddInputParameter("@maxNbreField", System.Data.SqlDbType.Int, eLibConst.MAX_NBRE_FIELD);
                rq.AddInputParameter("@fieldFormat_Title", System.Data.SqlDbType.Int, (int)FieldFormat.TYP_TITLE);
                rq.AddInputParameter("@popupType_DATA", System.Data.SqlDbType.Int, (int)PopupType.DATA);

                dtr = eDal.Execute(rq, out sError);
                if (sError.Length > 0) { throw new Exception(sError); }

                while(dtr.Read())
                {
                    int descid = dtr.GetEudoNumeric("DescId");
                    string label = dtr.GetString("Label");
                    if (!dicFields.ContainsKey(descid))
                        dicFields.Add(descid, label);
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

            return dicFields;
        }
    }
}
 