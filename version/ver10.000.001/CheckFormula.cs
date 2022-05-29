using System;
using EudoQuery;
using Com.Eudonet.Xrm;
using Com.Eudonet.Core.Model;

public class VersionFile
{
    public static void Upgrade(Object sender)
    {
        eUpgrader upgraderSender = (eUpgrader)sender;
        //upgraderSender.AddReferenceAssembly(typeof(System.Data.DataTableReader));
        eudoDAL eDal = upgraderSender.EDal;

        String Error = String.Empty;

        //AJOUT du FROM et du WHERE dans les formules de type SELECT sans FROM et WHERE
        RqParam rqModif = new RqParam("update [DESC] set [Formula] = [Formula] collate french_ci_ai + ' from [' + [File]  collate french_ci_ai + '] where ['	 + (select field from [DESC] main where main.[File] collate french_ci_ai  = upd.[File] collate french_ci_ai and DescId like '%00') + 'ID] = $fileid$'  from [desc] upd where upd.[Formula] like '%select%' and upd.[Formula] not like '%from%' and upd.[Formula] not like '%where%'");
        eDal.ExecuteNonQuery(rqModif, out Error);
        if (!String.IsNullOrEmpty(Error))
            throw new Exception(Error);


        //AJOUT du FROM dans les formules de type SELECT avec WHERE mais sans FROM
        RqParam rqSelect = new RqParam("select [descid], [file], [Formula] from [desc] where formula like '%select%' and formula not like '%from%' and formula like '%where%'");
        System.Data.DataTableReader dtr = eDal.ExecuteQuery(rqSelect, out Error);
        if (!String.IsNullOrEmpty(Error))
            throw new Exception(Error);

        String descid = String.Empty;
        String tab = String.Empty;
        String formula = String.Empty;
        while (dtr.Read())
        {
            descid = dtr[0].ToString();
            tab = dtr[1].ToString();
            formula = dtr[2].ToString();

            String[] tmp = formula.Split(new String[] { "where" }, StringSplitOptions.None);
            if (tmp.Length != 2)
                continue;

            String newValue = String.Concat(tmp[0], " from [", tab, "] where ", tmp[1]);

            rqModif = new RqParam(String.Concat("update [DESC] set [Formula] = '", newValue.Replace("'", "''"), "' where descid = ", descid));
            eDal.ExecuteNonQuery(rqModif, out Error);
            if (!String.IsNullOrEmpty(Error))
                throw new Exception(Error);
        }
        dtr.Close();
    }
}