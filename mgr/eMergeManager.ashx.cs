using System;
using System.Text;
using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm.mgr
{
    /// <className>eMergeManager</className>
    /// <summary>Manager de liaison entre le moteur javascript Engine.js et le moteur d'enregistrement Engine.cs</summary>
    /// <purpose>Cette clase s'occupe des fusions sur l'applicatif</purpose>
    /// <authors>HLA</authors>
    /// <date>2014-09-04</date>
    public class eMergeManager : eEngineMgr
    {
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessManager()
        {
            Int32 tab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            Int32 masterFileId = _requestTools.GetRequestFormKeyI("masterFileId") ?? 0;
            Int32 doublonFileId = _requestTools.GetRequestFormKeyI("doublonFileId") ?? 0;

            if (tab == 0 || masterFileId == 0 || doublonFileId == 0)
            {
                String devMsg = new StringBuilder()
                    .Append(eResApp.GetRes(_pref, 2024)).Replace("<PARAM>", "tab, masterFileId, doublonFileId")
                    .Append(" (tab = ").Append(tab).Append(", masterFileId = ").Append(masterFileId).Append(", doublonFileId = ").Append(doublonFileId).Append(")")
                    .ToString();

                // 1795	- Une erreur est survenue lors de la fusion
                // 2024 - Paramètre(s) <PARAM> invalide(s)
                // 72 - Une erreur est survenue.
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 1795),
                    eResApp.GetRes(_pref, 2024).Replace(" <PARAM> ", " "),
                    eResApp.GetRes(_pref, 72),
                    devMsg
                );

                // Arrete le traitement et envoi l'erreur
                LaunchError();
            }

            try
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));

                String vals = String.Empty;
                if (_requestTools.GetRequestFormKey("fieldChange", out vals))
                    eng.AddParam("fieldChange", vals);
                if (_requestTools.GetRequestFormKey("fieldConcat", out vals))
                    eng.AddParam("fieldConcat", vals);

                eng.AddParam("keepAllAdr", (_requestTools.GetRequestFormKeyB("keepAllAdr") ?? false) ? "1" : "0");
                eng.AddParam("overwriteAdrInfos", (_requestTools.GetRequestFormKeyB("overwriteAdrInfos") ?? false) ? "1" : "0");
                eng.AddParam("validMerge", (_requestTools.GetRequestFormKeyB("validMerge") ?? false) ? "1" : "0");

                eng.EngineProcess(new StrategyMerge(masterFileId, doublonFileId));

                _engResult = eng.Result;
            }
            catch (Exception ex)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(
                    eLibConst.MSG_TYPE.CRITICAL,
                    eResApp.GetRes(_pref, 1795),
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