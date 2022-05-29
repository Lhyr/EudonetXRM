using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.TreatmentCampaign;
using Com.Eudonet.Merge.Eudonet;
using EudoProcessInterfaces;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Data;
using System.ServiceModel;

namespace Com.Eudonet.Xrm.mgr
{
    /// <summary>
    /// manager pour mettre en pause une campaign
    /// </summary>
    public class eOnBreakCampaignMgr : eEudoManager
    {
        /// <summary>Appelé à l'appel de la page</summary>
        protected override void ProcessManager()
        {
            JSONReturnGeneric res = new JSONReturnGeneric();
            int nCampaignId = (int)_requestTools.GetRequestFormKeyI("CampaignDescId");
            int nStatus = (int)_requestTools.GetRequestFormKeyI("Status");           
            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            string sBaseName = dal.dbname;
            string err = string.Empty;
            string sError = string.Empty;

            eOnBreakCampaignResponse result = new eOnBreakCampaignResponse();

            try
            {
                eWCFBasic<IEudoTreatmentCampaignWCF> wcfAccess = null;
                
                try
                {
                    if (nCampaignId != 0)
                    {
                        //désactivé les étapes d'une campaign

                        string wcfUrl = ConfigurationManager.AppSettings.Get("EudoPauseCampaign");
                        result = eWCFTools.WCFEudoProcessCaller<IEudoTreatmentCampaignWCF, eOnBreakCampaignResponse>(wcfUrl, obj => obj.PauseCampaignJob
                        (new eOnBreakCampaignCall() { CampaingId = nCampaignId, Status = nStatus, BaseName = sBaseName }));

                        if (!result.Succes)
                        {
                            res.Success = result.Succes;
                            res.ErrorMsg = result.ErrorMsg;
                        }
                        else
                        {
                            //mettre en pause ou réactiver une campaign

                            dal.OpenDatabase();
                            string sql = "UPDATE [CAMPAIGN] SET OnBreak = @Status where [CampaignId] = @CampaignId";

                            try
                            {
                                RqParam rq = new RqParam(sql);
                                rq.AddInputParameter("@CampaignId", SqlDbType.Int, nCampaignId);
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