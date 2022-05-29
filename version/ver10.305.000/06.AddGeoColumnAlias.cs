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
    /// Si le champ geo xxx existe déjà, on le renome en xxx_GEO et on ajout un nouveau champ xxx physique de type text, hors 74
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
            String sql = @"  SELECT [DESCID], [FILE], [FIELD] FROM [DESC] WHERE [format] in ("+ (int)FieldFormat.TYP_GEOGRAPHY_V2 + ", "+ (int)FieldFormat.TYP_GEOGRAPHY_OLD + ")   ";

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
                fldName = dtr.GetString("FIELD");
                fldDescId = dtr.GetEudoNumeric("DESCID");

                // Si le champ xxx_GEO n'existe pas on renomme l'existant
                sb.Append(" if not exists(select * from sys.columns sc ").AppendLine()
                .Append("  inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = '").Append(fileName).Append("' collate french_ci_ai ").AppendLine()
                .Append("  where sc.name collate french_ci_ai = '").Append(fldName).Append("_GEO' collate french_ci_ai) ").AppendLine()
                .Append(" exec sp_rename '").Append(fileName).Append(".").Append(fldName).Append("', '").Append(fldName).Append("_GEO', 'COLUMN'").AppendLine()

                // Si le champ GEO n'existe plus on ajout un alias (pour les eudoquery anciens)
                .Append(" if not exists(select * from sys.columns sc ").AppendLine()
                .Append("  inner join sys.tables st on st.object_id = sc.object_id and st.name collate french_ci_ai = '").Append(fileName).Append("' collate french_ci_ai ").AppendLine()
                .Append("  where sc.name collate french_ci_ai = '").Append(fldName).Append("' collate french_ci_ai) ").AppendLine()
                .Append(" alter table [").Append(fileName).Append("] add [").Append(fldName).Append("] AS [").Append(fldName).Append("_GEO].STAsText()").AppendLine()

                .Append(" update [desc] set  [Format]=").Append((int)FieldFormat.TYP_GEOGRAPHY_V2).Append(" where descid = ").Append(fldDescId).Append(" ").AppendLine();
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