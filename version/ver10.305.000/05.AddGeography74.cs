﻿using Com.Eudonet.Internal;
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
    ///  #51630 Ajout de la colonne système géolocalisation descid=xx74
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(Object sender)
    {   
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;

        String error = String.Empty;
        DataTableReaderTuned dtr = null;

        try
        {
            String sql = @"   SELECT [DESCID], [FILE], [FIELD], (select lang_00 from res where resid = descid) FROM [DESC] 
                              INNER JOIN  sys.tables ST on ST.name collate french_ci_ai = [FILE] collate french_ci_ai
                              WHERE [DESCID] % 100 = 0 and descid > 0
                              AND  ( descid <= 100000 or DescId in ( 101000, 106000, 108000, 111000, 114000, 114200))";

            dtr = eDal.Execute(new RqParam(sql), out error);
            if (!String.IsNullOrEmpty(error))
                throw new Exception(error);

            if (dtr == null)
                return;

            StringBuilder sb = new StringBuilder();
            int fldDescId;
            string fldName, fileName;
            while (dtr.Read())
            {
                fileName = dtr.GetString("FILE");
                fldName = dtr.GetString("FIELD") + (int)AllField.GEOGRAPHY;
                fldDescId = dtr.GetEudoNumeric("DESCID") + (int)AllField.GEOGRAPHY;

                // Mise à jour desc
                sb.Append(" if not exists(select * from [desc] where descid = ").Append(fldDescId).Append(") ").AppendLine()
                .Append(" insert into [desc] ([DescId], [File], [Field], [Format], [Length]) select ").Append(fldDescId).Append(", '").Append(fileName).Append("', '").Append(fldName).Append("', ").Append((int)FieldFormat.TYP_GEOGRAPHY_V2).Append(", 100 ").AppendLine()
                .Append(" else ").AppendLine()
                .Append(" update [desc] set [Format]=").Append((int)FieldFormat.TYP_GEOGRAPHY_V2).Append(" where descid = ").Append(fldDescId).Append(" ").AppendLine()

                // mise à jour res
                .Append(" if not exists(select * from [res] where resid = ").Append(fldDescId).Append(") ").AppendLine()
                .Append(" insert into [res] (resid, lang_00, lang_01, lang_02, lang_03, lang_04) select ").Append(fldDescId).Append(", 'Géolocalisation', 'location', 'location', 'location', 'location' ").AppendLine()

                // si la colonne 74 existe mais que la colone 74_geo n'existe pas, alors on drop la column pour passer sur le nouveau système
                .Append(" if not exists(select * from sys.columns sc ").AppendLine()
                .Append("  inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = '").Append(fileName).Append("' collate french_ci_ai ").AppendLine()
                .Append("  where sc.name collate french_ci_ai = '").Append(fldName).Append("_GEO' collate french_ci_ai) ").AppendLine()
                .Append(" and exists(select * from sys.columns sc ").AppendLine()
                .Append("  inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = '").Append(fileName).Append("' collate french_ci_ai ").AppendLine()
                .Append("  where sc.name collate french_ci_ai = '").Append(fldName).Append("' collate french_ci_ai) ").AppendLine()
                .Append("  begin").AppendLine()
                .Append("  alter table [").Append(fileName).Append("] drop column [").Append(fldName).Append("] ").AppendLine()
                .Append("  end ").AppendLine()

                // création des colonnes physiquement
                .Append(" if not exists(select * from sys.columns sc ").AppendLine()
                .Append("  inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = '").Append(fileName).Append("' collate french_ci_ai ").AppendLine()
                .Append("  where sc.name collate french_ci_ai = '").Append(fldName).Append("' collate french_ci_ai) ").AppendLine()				
				.Append("  begin").AppendLine()             
				.Append("  alter table [").Append(fileName).Append("] add [").Append(fldName).Append("_GEO] geography null ").AppendLine()
                .Append("  alter table [").Append(fileName).Append("] add [").Append(fldName).Append("] AS [").Append(fldName).Append("_GEO].STAsText()").AppendLine()    
                .Append("  end ").AppendLine();
            }

            // exec du script final
            if (sb.Length > 0)
            {
                eDal.ExecuteNonQuery(new RqParam(sb.ToString()), out error);
                if (!String.IsNullOrEmpty(error))
                    throw new Exception(error);
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            if (dtr != null)
                dtr.Dispose();
        }
    }
}