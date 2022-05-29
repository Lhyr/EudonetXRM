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

        bool bInternalOpen = false;
        string error = string.Empty;
        System.Text.StringBuilder errorLog = new System.Text.StringBuilder();
        DataTableReaderTuned dtr = null;

        try
        {
            if (!_eDal.IsOpen)
            {
                bInternalOpen = true;
                _eDal.OpenDatabase();
            }

            //  recherche des catalogues avancé affecté par le bug
            string sSqlverif = @"
 
                        SELECT
  
	                        sc.system_type_id, 
	                        D.DescId,
	                        D.[file] [FILE], 
	                        D.[field] [FIELD], 
	                        D.Format, 
	                        D.Length, 
	                        D.Popup, 
	                        D.PopupDescId,
	                        R.LANG_00

                        FROM [DESC]  D

	                        INNER JOIN sys.tables st ON st.name COLLATE french_ci_ai = D.[File] COLLATE french_ci_ai
	                        INNER JOIN sys.columns sc ON sc.name COLLATE french_ci_ai = D.[Field] COLLATE french_ci_ai and st.object_id = sc.object_id
	                        INNER JOIN [DESC] dparent ON D.PopupDescId - 1 = dparent.DescId
	                        INNER JOIN [DESC] pop ON pop.DescId=D.PopupDescId
	                        INNER JOIN [RES] R ON R.ResId = D.DescId
                        WHERE 
		                        D.PopupDescId like '%01'   
	                        AND D.PopupDescId <> D.DescId 
	                        AND isnull(dparent.type,0) <> 0
	                        AND sc.system_type_id = 108			
			 ";

            RqParam rqVerif = new RqParam(sSqlverif);


            dtr = _eDal.Execute(rqVerif, out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (dtr == null)
                return;

            string command = @"
			BEGIN TRY
                /*On tente de netoyer*/			
				ALTER TABLE {0} ALTER COLUMN {1} VARCHAR(100);				
                UPDATE [DESC] SET LENGTH = 100 WHERE [DESCID] = @descid
  		    END TRY
            BEGIN CATCH				
            END CATCH
			";
            string requieredDefaultName = string.Empty;
            string defaultConstraintName = string.Empty;
            while (dtr.Read())
            {

                RqParam rq = new RqParam(string.Format(command, dtr.GetString("File"), dtr.GetString("Field")));
                rq.AddInputParameter("@descid", System.Data.SqlDbType.Int, dtr.GetEudoNumeric("descid"));

                _eDal.ExecuteNonQuery(rq, out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);

            }
        }
        finally
        {
            if (dtr != null)
                dtr.Dispose();

            if (bInternalOpen && _eDal != null)
                _eDal.CloseDatabase();
        }
    }
}