using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.TreatmentCampaign;
using EudoProcessInterfaces;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.ServiceModel;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de EOnBreakEventStepDelayedMgr
    /// </summary>
    public class EOnBreakEventStepDelayedMgr : eEudoManager
    {

        protected override void ProcessManager()
        {
            JSONReturnGeneric res = new JSONReturnGeneric();
            int nEventId = (int)_requestTools.GetRequestFormKeyI("nEventId");
            int nStatus = (int)_requestTools.GetRequestFormKeyI("Status");
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            string sBaseName = dal.dbname;
            string err = string.Empty;
            string sError = string.Empty;
            DataTableReaderTuned dtr = null;
            List<int> campaignsId = null;

            eOnBreakCampaignsResponse result = new eOnBreakCampaignsResponse();

            try
            {
                eWCFBasic<IEudoTreatmentCampaignWCF> wcfAccess = null;

                try
                {
                    if (nEventId != 0)
                    {
                        dal.OpenDatabase();
                        string sqlselect = @"SELECT CampaignId FROM CAMPAIGN where ParentTabId = @ParentTabId and ParentFileId = 1 
                                            and status = " + CampaignStatus.MAIL_DELAYED;

                        try
                        {
                            RqParam rq = new RqParam(sqlselect);
                            rq.AddInputParameter("@ParentTabId", SqlDbType.Int, nEventId);
                            dtr = dal.Execute(rq, out sError);

                            if (sError.Length > 0)
                            {
                                res.Success = false;
                                res.ErrorDebugMsg = string.Concat(Environment.NewLine, "Erreur sur mettre en pause des étapes de marketing", sError);
                            }
                            else
                            {
                                while (dtr.Read())
                                {
                                    int valueCampaignId = dtr.GetInt32("CampaignId");
                                    campaignsId.Add(valueCampaignId);
                                }
                            }
                        }
                        finally
                        {
                            dal.CloseDatabase();
                        }

                        //désactivé les étapes marketings
                        string wcfUrl = ConfigurationManager.AppSettings.Get("EudoPauseCampaign");
                        result = eWCFTools.WCFEudoProcessCaller<IEudoTreatmentCampaignWCF, eOnBreakCampaignsResponse>(wcfUrl, obj => obj.PauseEtapeMarketingDelayed
                        (new eOnBreakCampaignsCall() { CampaingsId = campaignsId, Status = nStatus, BaseName = sBaseName }));

                        if (!result.Succes)
                        {
                            res.Success = result.Succes;
                            res.ErrorMsg = result.ErrorMsg;
                        }
                        else
                        {
                            //mettre en pause ou réactiver les étapes marketing

                            dal.OpenDatabase();
                            string sql = "UPDATE [CAMPAIGN] SET OnBreak = @Status where [CampaignId] in (";

                            int i = 0;
                            foreach (int campaignId in campaignsId)
                            {
                                if (i == 0)
                                    string.Concat(sql, campaignId);
                                else
                                    string.Concat(sql, " , ", campaignId);

                                ++i;
                            }

                            string.Concat(sql, " ) ");

                            try
                            {
                                RqParam rq = new RqParam(sql);
                                rq.AddInputParameter("@Status", SqlDbType.Int, nStatus);


                                dal.ExecuteNonQuery(rq, out sError);

                                if (sError.Length > 0)
                                {
                                    res.Success = false;
                                    res.ErrorDebugMsg = string.Concat(Environment.NewLine, "Erreur sur mettre en pause une campaign", sError);
                                }
                                else
                                {
                                    res.Success = true;
                                }


                            }
                            finally
                            {
                                dal.CloseDatabase();
                            }
                        }
                    }
                }
                catch (EndpointNotFoundException ExWS)
                {
                    err = string.Concat("Module de mettre en pause en campaigne injoignable : ", Environment.NewLine, ExWS.ToString());
                    res.Success = false;
                    res.ErrorDetailMsg = ExWS.ToString();

                }
                catch (Exception ex)
                {
                    err = string.Concat("Une erreur est survenue ", ex.ToString());
                    res.Success = false;
                    res.ErrorDetailMsg = ex.ToString();
                }
                finally
                {
                    wcfAccess?.Dispose();
                }

            }
            catch (Exception e)
            {
                err = string.Concat("Une erreur est survenue ", e.ToString());
                res.Success = false;
                res.ErrorDetailMsg = e.ToString();
            }

            RenderResult(RequestContentType.TEXT, delegate ()
            {
                return JsonConvert.SerializeObject(res);
            });
        }
    }
}