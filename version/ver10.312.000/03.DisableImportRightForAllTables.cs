using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    /// <summary>
    ///  #59751 - Le droit d'import est désactivé par défaut sur toutes les tables de DESC
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(Object sender)
    {
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;

        String error = String.Empty;
        DataTableReaderTuned dtr = null;
        bool bInternalOpen = false;
		
        try
        {
		     if (!eDal.IsOpen)
			{
		     	bInternalOpen = true;
			   eDal.OpenDatabase();
            }
		
            /*********************************************
             * On sélection des tables définit dans desc sans exception
             *********************************************/
            String sql = @"          
                            SELECT  
	                            [d].[DescId],
	                            /* Présence de droit d'import en onglet dans trait */
	                             t18.TraitId TabRight,
	                            /* Présence de droit d'import en signet dans trait */
	                            t19.TraitId BkmRight                         
                            FROM [DESC] d                            
                            LEFT JOIN TRAIT t18 ON t18.TraitId = d.descid + 18
                            LEFT JOIN TRAIT t19 ON t19.TraitId = d.descid + 19
                            WHERE DescId % 100 = 0  AND DescId > 0
                            /*  Que des traits manquants */
                            AND (t18.TraitId IS NULL OR t19.TraitId IS NULL) ";

            dtr = eDal.Execute(new RqParam(sql), out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (dtr == null || !dtr.HasRows)
                return;

            // Si la ligne trait existe alors on rollback la transaction
             String insertSql = @"
                    BEGIN TRY
						BEGIN TRANSACTION; 
						/* Restriction totale */
						INSERT INTO [PERMISSION] (Mode, [Level], [User])	VALUES (1, 0, '');

						/* Ajout de la permission au droit d'import correspondant */
						INSERT INTO [TRAIT] (TraitId, UserLevel, TraitLevel, Sort, PermId) VALUES (@Tab + @TraitNumber, 0, 1, @TraitNumber, SCOPE_IDENTITY());
                        COMMIT;
  				    END TRY
                    BEGIN CATCH	
					    /* Si la ligne traite existe, alors on rollback */
						ROLLBACK;						
                    END CATCH				
				   ";

            RqParam rq;
            int descid;
            bool TabRightIsNotDefined, BkmRightIsNotDefined;	
            while (dtr.Read())
            {
                descid = dtr.GetEudoNumeric("DescId");
              
                // Permission pour la table en mode liste
			    TabRightIsNotDefined = dtr.IsDBNull("TabRight");
                if(TabRightIsNotDefined)
				{
				    rq = new RqParam(insertSql);
					rq.AddInputParameter("@Tab", System.Data.SqlDbType.Int, descid);
					rq.AddInputParameter("@TraitNumber", System.Data.SqlDbType.Int, (int)ProcessRights.PRC_RIGHT_IMPORT_TAB);
					
					eDal.ExecuteNonQuery(rq, out error);
					if (!String.IsNullOrEmpty(error))
						throw new Exception(string.Concat("DisableImportRightForAllTables.cs", ":", error));
				}
				 
				// Permission pour la la table depuis le signet
				BkmRightIsNotDefined = dtr.IsDBNull("BkmRight");
                if(BkmRightIsNotDefined)
				{
				    rq = new RqParam(insertSql); 
					rq.AddInputParameter("@Tab", System.Data.SqlDbType.Int, descid);
					rq.AddInputParameter("@TraitNumber", System.Data.SqlDbType.Int, (int)ProcessRights.PRC_RIGHT_IMPORT_BKM);
				   
				    eDal.ExecuteNonQuery(rq, out error);					
					if (!String.IsNullOrEmpty(error))
						throw new Exception(string.Concat("DisableImportRightForAllTables.cs",":", error));
				}
		   } 
        }     
        finally
        {
            if (dtr != null)
                dtr.Dispose();				 
        
            if (bInternalOpen && eDal != null)
                eDal.CloseDatabase();
        
        }
    }
}