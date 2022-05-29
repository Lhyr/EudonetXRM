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
    /// Création les champs relations
    /// </summary>
    /// <param name="sender"></param>
    public static void Upgrade(object sender)
    {
        string sqlreq;
        eUpgrader upgraderSender = (eUpgrader)sender;
        eudoDAL eDal = upgraderSender.EDal;
        ePrefSQL prefsql = upgraderSender.Pref;
        List<TableType> tablesWorkflow = new List<TableType>()
        {
            TableType.SCENARIO,
            TableType.STEP,
            TableType.ACTIVITIES
        };


        bool bNeedLocalOpen = !eDal.IsOpen;

        string sError;
        try
        {

            eDal.OpenDatabase();

            //get usersql
            string sqluser = @"SELECT top 1 USERID FROM [USER] WHERE USERLEVEL >= 99 and IsNull(UserDisabled ,0) = 0 order by UserLevel desc";
            int UID = eDal.ExecuteScalar<int>(new RqParam(sqluser), out sError);
            if (sError.Length > 0)
                throw eDal.InnerException ?? new EudoException(sError);

            ePrefUser prefuser = ePrefUser.GetNew(prefsql, new eUserInfo(UID, eDal));



            //get from descadv les table "Étapes" activé
            sqlreq = @"SELECT [DESCID] FROM DESCADV where Parameter = " + (int)DESCADV_PARAMETER.EVENT_STEP_ENABLED + " and value = 1";
            RqParam rq = new RqParam(sqlreq);
            var dt = eDal.Execute(rq);
            if (eDal.InnerException != null)
                throw eDal.InnerException;

            if (dt.HasRows)
            {
                while (dt.Read())
                {

                    int nDescIdToLink = dt.GetEudoNumeric("DESCID");

                    //for every workflow table scenario, activities , step
                    foreach (TableType item in tablesWorkflow)
                    {
                        HashSet<int> listParentTab = new HashSet<int>();
                        DataTableReaderTuned dtr = null;
                        try
                        {
                            RqParam rqlink = eSqlDesc.GetRqLiaison((int)item, prefuser.User.UserLangId);
                            dtr = eDal.Execute(rqlink, out sError);
                            while (dtr.Read())
                                listParentTab.UnionWith(new HashSet<int>() { dtr.GetInt32("RelationFileDescId") });


                            //si liaison déjà existante, on continue
                            if (listParentTab.Contains(nDescIdToLink))
                                continue;
                        }
                        catch (Exception ex)
                        {
                            sError = String.Format("Une erreur s'est produite lors de la récuperation des infos de la table "+item+" : {0}", ex.Message);
                            return;
                        }
                        finally
                        {
                            if (dtr != null)
                                dtr.Dispose();
                        }

                        HashSet<int> hsFreeDescIds = new HashSet<int>();
                        hsFreeDescIds = eSqlDesc.GetFreeDescIds(eDal, (int)item, out sError);
                        if (sError.Length > 0)
                        {
                            sError = String.Format("Une erreur s'est produite lors de la récuperation des DescId libres de la table " + item + ": {0}", sError);
                            return;
                        }


                        if (hsFreeDescIds.Count > 0)
                        {
                            //add field
                            int fieldCreateDescId = hsFreeDescIds.First();
                            Dictionary<int, string> dicoLabels = eSqlRes.GetLabels(eDal, nDescIdToLink, out sError);
                            if (sError.Length > 0)
                            {
                                sError = String.Format("Une erreur s'est produite lors de la récuperation des RES de la table {0} : {1}", (int)item, sError);
                                return;
                            }


                            eAdminFieldInfos fieldCreate = new eAdminFieldInfos(prefuser, fieldCreateDescId);
                            fieldCreate.Format = FieldFormat.TYP_CHAR;
                            fieldCreate.Length = 0;
                            fieldCreate.PopupType = PopupType.ONLY;
                            fieldCreate.PopupDescId = nDescIdToLink + 1;
                            fieldCreate.Relation = true;
                            fieldCreate.Labels = dicoLabels;
                            fieldCreate.Rowspan = 3;
                            fieldCreate.Create(eDal, out sError);
                        }


                    }
                }
            }
        }
        catch
        {
            throw;
        }
        finally
        {
            if (bNeedLocalOpen)
                eDal.CloseDatabase();
        }
    }


}
