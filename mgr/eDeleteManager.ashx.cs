using System;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Result.Xrm;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eDeleteManager</className>
    /// <summary>Manager de liaison entre le moteur javascript Engine.js et le moteur d'enregistrement Engine.cs</summary>
    /// <purpose>Cette clase s'occupe des suppressions sur l'applicatif</purpose>
    /// <authors>HLA</authors>
    /// <date>2013-01-29</date>
    public class eDeleteManager : eEngineMgr
    {
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 tab = _allKeys.Contains("tab") ? eLibTools.GetNum(_context.Request.Form["tab"].ToString()) : 0;
            Int32 fileId = _allKeys.Contains("fileId") ? eLibTools.GetNum(_context.Request.Form["fileId"].ToString()) : 0;

            if (tab == 0 || fileId == 0)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "tab, fileid"), " (tab = ", tab, ", fileid = ", fileId, ")")
                );

                // Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            String deleteAdr = _allKeys.Contains("deleteAdr") && _context.Request.Form["deleteAdr"].ToString() == "1" ? "1" : "0";
            String deletePp = _allKeys.Contains("deletePp") && _context.Request.Form["deletePp"].ToString() == "1" ? "1" : "0";
            String openSerie = _allKeys.Contains("openSerie") && _context.Request.Form["openSerie"].ToString() == "1" ? "1" : "0";
            Boolean validDel = _allKeys.Contains("validDeletion") && _context.Request.Form["validDeletion"].ToString() == "1";

            //Cas particulier pour les tables Étapes, on doit d'abord aller supprimer la tache dans le plannificateur
            if (validDel)
            {
                eudoDAL dal = null;
                try
                {
                    dal = eLibTools.GetEudoDAL(_pref);
                    dal.OpenDatabase();
                    if(DescAdvDataSet.LoadAndGetAdvParam(dal, tab, DESCADV_PARAMETER.IS_EVENT_STEP, "0") == "1")
                    {
                        eEventStepXRM eventStep = new eEventStepXRM(_pref, tab, fileId);
                        string error;
                        if (!eventStep.LoadEventStepFile(out error, dal) || error.Length > 0)
                            throw new Exception(error);

                        if (eventStep.ScheduleJobId != 0)
                        {
                            eventStep.DeleteSchedule();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorContainer = eErrorContainer.GetDevUserError(
                        eLibConst.MSG_TYPE.CRITICAL,
                        eResApp.GetRes(_pref, 6237),
                        ex.Message,
                        eResApp.GetRes(_pref, 72),
                        ex.StackTrace
                        );

                    // Arrete le traitement et envoi l'erreur
                    LaunchError();
                }
                finally
                {
                    dal?.CloseDatabase();
                }
            }




            try
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = fileId;

                // Indique qu'il est également demandé de supprimer les PP en cascade
                eng.AddParam("deletePp", deletePp);
                // Indique qu'il est également demandé de supprimer les Adresses en cascade
                eng.AddParam("deleteAdr", deleteAdr);
                // Indique une suppression depuis une serie de planning
                eng.AddParam("openSerie", openSerie);
                // Retour de la confirmation de suppression
                eng.AddParam("validDeletion", validDel ? "1" : "0");
                // Informationsur la base
                eng.AddParam("uid", _pref.DatabaseUid);

                eng.EngineProcess(new StrategyDelXrm(), new ResultXrmDel());

                _engResult = eng.Result;

                

            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 6237),
                    ex.Message,
                    eResApp.GetRes(_pref, 72),
                    ex.StackTrace
                );

                // Arrete le traitement et envoi l'erreur
                LaunchError();
            }
            finally
            {
                DoResponse();
            }
        }
    }
}