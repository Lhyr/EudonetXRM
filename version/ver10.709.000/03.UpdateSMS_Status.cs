using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using Com.Eudonet.Internal.SpecifTools;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Linq;

public class VersionFile
{
    /// <summary>
    /// Création du champ status
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(object sender)
    {
        string sql = string.Empty;
        string error = string.Empty;
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;
        ePrefSQL prefsql = upgraderSender.Pref;
        string sError;
        try
        {
            sql = @"SELECT top 1 USERID FROM [USER] WHERE USERLEVEL >= 99 and IsNull(UserDisabled ,0) = 0 order by UserLevel desc";
            int UID = eDal.ExecuteScalar<int>(new RqParam(sql), out sError);
            if (sError.Length > 0)
                throw eDal.InnerException ?? new EudoException(sError);


            ePrefUser prefuser = ePrefUser.GetNew(prefsql, new eUserInfo(UID, eDal));

            //récup de tous les templates sms  
            sql = @"SELECT [tab].[DescId] FROM [DESC] tab
					left join [DESC] fld on fld.[File] = [tab].[File] and lower(fld.Field) = 'tpl29'
				WHERE [tab].[TYPE] = 4";

            RqParam rq = new RqParam(sql);

            var dt = eDal.Execute(rq);
            if (eDal.InnerException != null)
                throw eDal.InnerException;

            if (dt.HasRows)
            {
                while (dt.Read())
                {
                    int nDescId = dt.GetEudoNumeric("descid");
  
                    eCatalog cat = new eCatalog(eDal, prefuser, PopupType.DATA, prefuser.User, nDescId + (int)MailField.DESCID_SMS_STATUS);

					StringBuilder sbError = new StringBuilder();
					var c = cat.Values.FirstOrDefault(cv => cv.Data.ToLower() == "error");
					if (c == null)
                    {
							InsertCatValue(cat, 7160, "Error", ref sbError, iResIdToolTIp: 6647, _pref: prefuser);  //Erreur d'envoi
					}
				 
		            if (sbError.Length > 0)
                    {
                        sError = String.Concat("Une erreur s'est produite lors de la mise a jour des catalogues SMS status : ", sbError.ToString());
                        throw new EudoException(sError);
                    }
                }
            }

            if (error.Length > 0)
            {
                throw new XRMUpgradeException(error, "Maj Catalogue status sms", "10.706.0000");
            }
        }
        catch
        {
            throw;
        }
        finally
        {

        }
    }


    protected static int InsertCatValue(eCatalog cat,
    Int32 iResId,
    string sData,
    ref StringBuilder sbError,
     ePrefUser _pref,
    Int32 iParentId = -1,
    int iResIdToolTIp = 0
    )
    {
        String sError = "";
        int catId = 0;

        Dictionary<String, String> diLabels = new Dictionary<String, string>();
        Dictionary<String, String> diLabelsTT = new Dictionary<String, string>();
        for (int i = 0; i < eLibConst.MAX_LANG; i++)
        {
            diLabels.Add(String.Concat("LANG_", i.ToString().PadLeft(2, '0')), eResApp.GetRes(i, iResId));
            diLabelsTT.Add(String.Concat("TIP_LANG_", i.ToString().PadLeft(2, '0')), iResIdToolTIp > 0 ? eResApp.GetRes(i, iResIdToolTIp) : "");
        }

        eCatalog.CatalogValue catVal = new eCatalog.CatalogValue(0, iParentId, eResApp.GetRes(_pref, iResId), sData);

        eCatalog.CatalogValue newCatValue = cat.Insert(catVal, out sError, diLabels, diLabelsTT);
        if (sError.Length > 0)
            sbError.AppendLine(sError);
        if (newCatValue != null)
            catId = newCatValue.Id;

        return catId;
    }

}
