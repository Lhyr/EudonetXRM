using System;
using EudoQuery;
using Com.Eudonet.Xrm;
using Com.Eudonet.Internal;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    public static void Upgrade(Object sender)
    {

        eUpgrader upgraderSender = (eUpgrader)sender;
        upgraderSender.AddReferenceAssembly(typeof(System.Web.HttpContext));

        eudoDAL _eDal = upgraderSender.EDal;
        ePrefSQL _pref = upgraderSender.Pref;

        bool bWasOpen = false;
        string error = string.Empty;
        System.Text.StringBuilder errorLog = new System.Text.StringBuilder();
        try
        {
            bWasOpen = _eDal.IsOpen;

            if (!bWasOpen)
                _eDal.OpenDatabase();

            string sSqlverif = "select count(1) from sys.columns A INNER JOIN INFORMATION_SCHEMA.COLUMNS B ON A.name = B.COLUMN_NAME and TABLE_NAME = 'CAMPAIGN' where TABLE_NAME = 'CAMPAIGN' AND COLUMN_NAME ='SubjectInClear'";

            string sSQLSel = "select CampaignId,[Subject] from CAMPAIGN where [Subject] IS NOT NULL AND  ISNULL([SubjectInClear],'') = '' ;";
            string sSQLUpdate = "UPDATE CAMPAIGN SET [SubjectInClear] = @subject  where [CampaignId] = @campaign ";

            RqParam rqVerif = new RqParam(sSqlverif);
            RqParam rq = new RqParam(sSQLSel);
            RqParam rqUpdate = new RqParam(sSQLUpdate);

            Int32 nbCol = _eDal.ExecuteScalar<Int32>(rqVerif, out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (nbCol == 1)
            {
                DataTableReaderTuned dtr = null;
                dtr = _eDal.Execute(rq, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);
                if (dtr == null)
                    return;



                while (dtr.Read())
                {
                    try
                    {
                        string subjectInClear = string.Empty;
                        int campaignId = dtr.GetEudoNumeric("CampaignId");
                        try
                        {
                            Com.Eudonet.Merge.eAnalyzerInfos subject = dtr.GetVarBinaryValue<Com.Eudonet.Merge.eAnalyzerInfos>("Subject");
                            subjectInClear = Com.Eudonet.Merge.eMergeTools.GetObjectMerge_Orig(subject);
                        }
                        catch (Exception e)
                        {
                            errorLog.AppendLine(e.Message);
                        }

                        if (campaignId > 0 && !string.IsNullOrEmpty(subjectInClear))
                        {
                            rqUpdate.AddInputParameter("@campaign", System.Data.SqlDbType.Int, campaignId);
                            rqUpdate.AddInputParameter("@subject", System.Data.SqlDbType.VarChar, subjectInClear);

                            _eDal.ExecuteNonQuery(rqUpdate, out error);
                            if (!String.IsNullOrEmpty(error))
                                throw new Exception(error);

                        }
                    }
                    catch (Exception ex)
                    {
                        errorLog.AppendLine(ex.Message);
                    }
                }
            }
        }
        finally
        {
            if (!bWasOpen)
                _eDal.CloseDatabase();
        }
    }


}