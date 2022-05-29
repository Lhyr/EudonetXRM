using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminUpdateFieldsCoordFromDisporder
    /// </summary>
    public class eAdminUpdateFieldsCoordFromDisporder : eAdminManager
    {
        /// <summary>
        /// page de test
        /// </summary>
        protected override void ProcessManager()
        {
            int iTab = _requestTools.GetRequestQSKeyI("d") ?? 0;
            StringBuilder sbError = new StringBuilder();
            string sError = "";


            List<int> liTabs = new List<int>();
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();
            try
            {

                if (iTab > 0)
                {
                    liTabs.Add(iTab);
                }
                else
                {
                    try
                    {
                        string sSQL = String.Format("SELECT Cast([DESCID] as int) AS DESCID FROM [DESC] WHERE [DESCID] % 100 = 0 AND [DESCID] BETWEEN 100 AND 100000 AND [DESCID] NOT IN (500, 600) " +
                            "AND Isnull(Type, 25) NOT IN ({0}, {1}, {2} , {3}, {4} )",
                            (int)EdnType.FILE_WEBTAB,
                            (int)EdnType.FILE_BKMWEB,
                            (int)EdnType.FILE_MAIL,
                            (int)EdnType.FILE_OBSOLETE, (int)EdnType.FILE_GRID);

                        RqParam rq = new RqParam(sSQL);
                        DataTableReaderTuned dtr = dal.Execute(rq, out sError);
                        if (sError.Length > 0)
                        {
                            throw new Exception(sError);
                        }

                        while (dtr.Read())
                        {
                            liTabs.Add(dtr.GetInt32(0));
                        }

                    }
                    catch (Exception e)
                    {
                        LaunchError(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat(e.Message, Environment.NewLine, e.StackTrace)));
                    }


                }
                try
                {

                    List<Tuple<int, EudoException>> lstErr = eFileLayout.UpdateTabsPositions(_pref, dal, liTabs);

                    /*
                    foreach (int iDescid in liTabs)
                    {
                        try
                        {
                            bool bFieldConflict = eSqlDesc.ResolveFieldsDisporderConflict(_pref, dal, iDescid, out sError);
                            if (sError.Length > 0)
                                sbError.AppendFormat("{0} eSqlDesc.ResolveFieldsDisporderConflict(): {1}", iDescid, sError).AppendLine();

                            if (bFieldConflict)
                            {
                                sbError.AppendFormat("{0} : Un conflit d'emplacement a été réparé automatiquement. Veuillez vérifier la mise en page.", iDescid).AppendLine();
                            }


                            eFileLayout.UpdateFieldsPositions(_pref, iDescid, out sError);
                            if (sError.Length > 0)
                                sbError.AppendFormat("{0} : {1}", iDescid, sError).AppendLine();
                        }
                        catch (Exception e)
                        {
                            sbError.AppendFormat("{0} : {1}", iDescid, e.Message).AppendLine().Append(e.StackTrace).AppendLine();
                        }

                    }

                    */

                    if (lstErr.Count > 0)
                    {
                       
                        foreach(var tpl in lstErr)
                        {
                            sbError.AppendLine(tpl.Item2.Message);
                        }

                        throw new EudoException(sbError.ToString());
                    }

                }
                catch (Exception e)
                {
                    LaunchError(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, String.Concat(e.Message, Environment.NewLine, e.StackTrace)));
                }
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                dal?.CloseDatabase();
            }
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void CheckAdminRight()
        {
            _pref.AdminMode = true;
            base.CheckAdminRight();
        }
    }
}