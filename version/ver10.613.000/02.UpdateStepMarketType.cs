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

            //récup de tous les descid event step
            sql = @"select DISTINCT descid from DESCADV where Parameter = 42";
					 
			 

            RqParam rq = new RqParam(sql);

            var dt = eDal.Execute(rq);
            if (eDal.InnerException != null)
                throw eDal.InnerException;

            if (dt.HasRows)
            {
                while (dt.Read())
                {
                    int nDescId = dt.GetEudoNumeric("descid");
                     
					 
					eCatalog catalogType =
					 new eCatalog(
					eDal,
					prefuser,
					PopupType.DATA,
					prefuser.User,
					nDescId +  (int)EventStepField.DESCID_STEP_TYPE,
					false,
					searchedValues: new List<eCatalog.CatalogValue>() { new eCatalog.CatalogValue() { Data = "3" }  });


					eCatalog.CatalogValue cs = catalogType.Values.FirstOrDefault(cv => cv.Data == "3");

					 if(cs != null)
						continue;
					 
                    //création valeur de catalogue
                    StringBuilder sbError = new StringBuilder();
                   // eCatalog cat = new eCatalog(eDal, prefuser, PopupType.DATA, prefuser.User, nDescId +  (int)EventStepField.DESCID_STEP_TYPE);
 
					int id = InsertCatValue(catalogType, 2818, "3", ref sbError,  _pref: prefuser);  //Envoi SMS
 
                    if (sbError.Length > 0)
                    {
                        sError = String.Concat("Une erreur s'est produite lors de l'ajout du type Envoi SMS pour l'event ", nDescId , "  err : ", sbError.ToString());
                        throw new EudoException(sError);
                    }
					
					 
					
					
					// Régle sur Type = Envoi Emailing Ou Smsing
					var rule2 = AdvFilter.GetNewFilter(prefuser, TypeFilter.RULES, nDescId);

					rule2.FilterName = string.Format("{0} = {1}",
						eResApp.GetRes(prefuser.LangId, 105 ),
						eResApp.GetRes(prefuser.LangId,  2818));

					rule2.FilterParams = string.Format("&file_0={0}&field_0_0={1}&op_0_0=0&value_0_0={2}&question_0_0=0&fileonly=0&negation=0&raz=0&random=0",
						nDescId,
						nDescId + (int)EventStepField.DESCID_STEP_TYPE,
						id);

					rule2.UserId = prefuser.UserId;
					rule2.FilterLastModified = DateTime.Now;
					var ruleCreateResult2 = rule2.Save(eDal);
					
					//Modif de la condition : ajout de la regle en OR
					if( ruleCreateResult2.Code == AdvFilterCrudInfos.ReturnCode.OK)
					{
						  sql= string.Format("UPDATE [DESC] SET VIEWRULESID = VIEWRULESID + ' OR ${0}$' where DescId={1} and ViewRulesId like '$%$' and ViewRulesId NOT like '% OR %'",rule2.FilterId, nDescId + (int)EventStepField.DESCID_STEP_TITLE_SEND_DETAIL) ;
						
						 eDal.ExecuteNonQuery(new RqParam(sql), out sError);
                        if (sError.Length != 0)
						{
							throw new EudoException(sError);
						}
					
					}
					else
						throw new EudoException("Impossible de créer la regle sur le type de " + nDescId) ;

                }
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
