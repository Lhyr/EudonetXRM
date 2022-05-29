using System;
using System.Collections.Generic;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eChkTplDblMgr
    /// </summary>
    public class eChkTplDblMgr : eEudoManager
    {

        /// <summary>
        /// traitement
        /// </summary>
        protected override void ProcessManager()
        {
            int iTplTab = _requestTools.GetRequestFormKeyI("tab") ?? 0;
            int iPpId = _requestTools.GetRequestFormKeyI("ppid") ?? 0;
            int iEvtId = _requestTools.GetRequestFormKeyI("evtid") ?? 0;
            ChkTplDblResponse response = new ChkTplDblResponse();


            if (iTplTab == 0 || iPpId == 0 || iEvtId == 0)
                RenderResult(RequestContentType.TEXT, () => JsonConvert.SerializeObject(response));

            try
            {
                eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, iTplTab, EudoQuery.ViewQuery.CUSTOM);
                dtf.EudoqueryComplementaryOptions = delegate (EudoQuery.EudoQuery eq)
                {
                    eq.SetTopRecord = 1;
                    eq.SetListCol = "201";
                    WhereCustom wc = new WhereCustom(new List<WhereCustom>() {
                                    new WhereCustom("PPID", Operator.OP_EQUAL, iPpId.ToString(), InterOperator.OP_AND),
                                    new WhereCustom("EVTID", Operator.OP_EQUAL, iEvtId.ToString(), InterOperator.OP_AND)
                                }
                                , InterOperator.OP_AND);

                    eq.AddCustomFilter(wc);
                };

                dtf.Generate();

                if (!string.IsNullOrEmpty(dtf.ErrorMsg))
                {
                    throw dtf.InnerException ?? new Exception(dtf.ErrorMsg);
                }

                response.IsDbl = dtf.ListRecords != null && dtf.ListRecords.Count > 0;
                if (response.IsDbl)
                {
                    eRes res = new eRes(_pref, String.Format("200,{0},{1}", iTplTab, dtf.ViewMainTable.InterEVTDescid));
                    string sPPTabLabel = res.GetRes(200),
                        sEventTabLabel = res.GetRes(dtf.ViewMainTable.InterEVTDescid),
                        sTplTabLabel = res.GetRes(iTplTab);

                    response.Title = eResApp.GetRes(_pref, 80);
                    response.Msg = String.Format(eResApp.GetRes(_pref, 1455), sPPTabLabel, sEventTabLabel, sTplTabLabel);
                }

                RenderResult(RequestContentType.TEXT, () => JsonConvert.SerializeObject(response));


            }
            catch (eEndResponseException e)
            {
            }
            catch (Exception e)
            {
                eLogEvent.Log(String.Concat("eChkTplDblMgr.ashx, Une erreur est survenue : ", e.Message, Environment.NewLine, e.StackTrace), System.Diagnostics.EventLogEntryType.Error);
                RenderResult(RequestContentType.TEXT, () => JsonConvert.SerializeObject(response));
            }
            finally
            {

            }





        }

        /// <summary>
        /// objet de retour
        /// </summary>
        public class ChkTplDblResponse
        {
            /// <summary>indique si il existe un doublon </summary>
            public bool IsDbl = false;

            /// <summary>Titre du message utilisateur</summary>
            public string Title = "";

            /// <summary>Message à l'utilisateur </summary>
            public string Msg = "";

            /// <summary>Détail du message utilisateur</summary>
            public string MsgDetail = "";

            /// <summary>
            /// constructeur
            /// </summary>
            /// <param name="isDbl"></param>
            public ChkTplDblResponse(bool isDbl = false)
            {
                IsDbl = isDbl;
            }
        }

    }
}