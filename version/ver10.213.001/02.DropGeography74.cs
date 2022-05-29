using Com.Eudonet.Internal;
using Com.Eudonet.Xrm;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
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
            String sql = @" select ' alter table [' + so.name + '] drop column [' + sc.name + ']; ' removeGeo, so.name, sc.name from sys.columns  sc
            inner join sys.objects so on so.object_id = sc.object_id  where sc.name like '%74' and sc.system_type_id = 240 ";



            RqParam rq = new RqParam(sql);
            dtr = eDal.Execute(rq, out error);
            if (!String.IsNullOrEmpty(error))
            {

                throw new Exception(error);
            }

            if (dtr == null)
            {

                return;
            }

            StringBuilder sb = new StringBuilder();
            int fldDescId;
            string fldName, fileName;


            while (dtr.Read())
            {

                sb.Append(dtr.GetString("removeGeo")).AppendLine();
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