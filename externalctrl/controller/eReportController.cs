using Com.Eudonet.Common.Import;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.tools.WCF;
using Com.Eudonet.Internal.wcfs.data.import;
using Com.Eudonet.Internal.wcfs.data.report;
using Com.Eudonet.Xrm.import;
using EudoProcessInterfaces;
using EudoQuery;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller pour declencher un import via code spécifique
    /// </summary>
    public class eReportController : ApiController
    {

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="eReportParam"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Post(eReportByCodeParams eReportParam)
        {


            ReportLaunchResult results = LaunchReport(eReportParam);
            return GetResponse(results);
        }


        /// <summary>
        /// Récupère le status d'un import
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id:int=0}")]
        public IHttpActionResult GetStatus(int id)
        {
            ReportCheckStatusResult res = new ReportCheckStatusResult()
            {
                Status = ReportWSStatusCode.ERROR,
                ErrorWSCode = ReportWSErrorCode.UNSPECIFIED,
                Success = false
            };

            try
            {
                //retourne flux serialise
                res = GetReportStatus(id);

                return GetResponse(res);
            }
            catch (EudoException ee)
            {
                //erreur EudoException
                res.ErrorWSCode = ReportWSErrorCode.UNSPECIFIED;
                res.InnerException = ee;
                res.ErrorDescription = ee.UserMessage;

            }
            catch (Exception e)
            {
                //erreur inconnue
                res.ErrorWSCode = ReportWSErrorCode.UNSPECIFIED;
                res.InnerException = new EudoException(e.Message, "Erreur", e);
            }

            return GetResponse(res);
        }





        /// <summary>
        /// options - retourne les headers
        /// </summary>
        /// <returns></returns>       
        public IHttpActionResult Options()
        {
            return GetResponse(null);
        }


        /// <summary>
        /// Appel wcf pour avoir le status d un import
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private ReportCheckStatusResult GetReportStatus(int id)
        {

            var hTok = Request.Headers.FirstOrDefault(h => h.Key == "x-auth");

            if (hTok.Value == null || hTok.Value?.Count() == 0)
                throw new EudoException("", "token non fourni");

            string sToken = hTok.Value.First();

            if (string.IsNullOrEmpty(sToken))
                throw new EudoException("", "token non fourni");


            ReportCheckStatusResult res = new ReportCheckStatusResult();

            try
            {

                //Création de l'objet epref
                ePref pref = eExternal.GetExternalPrefFromWSToken(sToken, Common.Enumerations.TokenRight.EXPORT);


                var oReportInfos = eWCFTools.WCFEudoProcessCaller<IEudoReportWCF, eReportResponse>(
                    ConfigurationManager.AppSettings.Get("EudoReportURL"), obj => obj.GetReportResponse(pref.GetBaseName, pref.User.UserId, id));

                if (
                        oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.SUCCESS
                    || oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.RUNNING
                    || oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.WAIT
                    )
                {

                    if (oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.SUCCESS)
                        res.Status = ReportWSStatusCode.DONE;
                    else if (oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.RUNNING)
                        res.Status = ReportWSStatusCode.RUNNING;
                    else if (oReportInfos.Status == Internal.wcfs.data.common.eProcessStatus.WAIT)
                        res.Status = ReportWSStatusCode.WAIT;

                    res.ErrorWSCode = ReportWSErrorCode.NONE;
                    res.Success = true;

                    if (!string.IsNullOrEmpty(oReportInfos.WebPath))
                        res.ReportWebPath = oReportInfos.WebPath;


                }
            }
            catch (TokenInvalidException)
            {
                return new ReportCheckStatusResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_INVALID


                };
            }
            catch (EudoAdminInvalidRightException)
            {

                return new ReportCheckStatusResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_RIGHT


                };
            }
            catch (EudoException ee)
            {
                return new ReportCheckStatusResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_MISSING,
                    ErrorDescription = ee.FullDebugMessage

                };
            }
            catch (EndpointNotFoundException ExWS)
            {

                return new ReportCheckStatusResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.WCF_UNAVAILABLE,
                    ErrorDescription = ExWS.Message

                };
            }
            catch (Exception e)
            {
                throw;
            }

            return res;
        }


        private ReportLaunchResult LaunchReport(eReportByCodeParams param)
        {


            if (string.IsNullOrEmpty(param.Token))
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_MISSING
                };




            if (param.IdReport <= 0)
            {
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.PARAMS_INVALID
                };
            }

            ReportLaunchResult res = new ReportLaunchResult();

            try
            {

                //Création de l'objet epref
                ePref pref = eExternal.GetExternalPrefFromWSToken(param.Token, Common.Enumerations.TokenRight.EXPORT);


                ReportUrlPaths urlPaths = ReportUrlPaths.GetNew(pref, System.Web.HttpContext.Current);

                eReport rep = new eReport(pref, param.IdReport);
                rep.LoadFromDB();

                if(rep.InnerException != null)
                {

                    return new ReportLaunchResult()
                    {
                        Success = false,
                        Status = ReportWSStatusCode.ERROR,
                        ErrorWSCode = ReportWSErrorCode.REPORT_INVALID
                    };

                }

                if (rep.ViewPerm != null && !rep.ViewPerm.IsAllowed(pref.User))
                {

                    return new ReportLaunchResult()
                    {
                        Success = false,
                        Status = ReportWSStatusCode.ERROR,
                        ErrorWSCode = ReportWSErrorCode.REPORT_DENIED
                    };
                }



                eReportCall erc = new eReportCall();
                erc.ReportId = rep.Id;
                erc.UserId = pref.User.UserId;
                erc.Lang = pref.Lang;
                erc.PrefSQL = pref.GetNewPrefSql();
                erc.SecurityGroup = pref.GroupMode.GetHashCode();


                // On transforme l'enum dans EudoQuery vers l'enum dans EudoProcessInterfaces pour la communication avec le WCF
                erc.TypeReport = (ReportType)((int)rep.ReportType);
                erc.DynamicTitle = param.Title;


                erc.TabFrom = param.Tab > 0 ? param.Tab : rep.Tab;

                erc.FileId = param.FileId;

                
                erc.TabBkm = param.Bkm;   

                erc.SqlDateFormat = eLibTools.GetSqlServerDateFormat(pref.CultureInfo);

                erc.MarkedFileOnly = false;
                erc.FilterSelId = param.FilterId;


                erc.AppPath = string.Concat(urlPaths.UrlFullPath, "/");
                erc.StylePath = urlPaths.StylePath;
                erc.ImgPath = urlPaths.ImgPath;
                erc.DatasPath = urlPaths.DatasPath;

                string error = "";

                // Appel de la méthode pour déclencher le rapport et récupérer l'id du nouveau rapport
                var reportRunInfos = eWCFTools.WCFEudoProcessCaller<IEudoReportWCF, eReportRun>(
                      ConfigurationManager.AppSettings.Get("EudoReportURL"), obj => obj.RunReport(erc, out error));


                if (string.IsNullOrEmpty(error))
                {

                    if (reportRunInfos.AnReportInProgress)
                    {
                        return new ReportLaunchResult()
                        {
                            Success = false,
                            Status = ReportWSStatusCode.ERROR,
                            ErrorWSCode = ReportWSErrorCode.REPORT_ALREADY_RUNNING,
                            ReportId = reportRunInfos.ServerReportId
                        };

                    }
                    
                    if (reportRunInfos.ServerReportId > 0)
                    {
                        return new ReportLaunchResult()
                        {
                            Success = true,
                            Status = ReportWSStatusCode.WAIT,
                            ErrorWSCode = ReportWSErrorCode.NONE,
                            ReportId = reportRunInfos.ServerReportId
                        };
                    }                    
                    else  
                    {
                        return new ReportLaunchResult()
                        {
                            Success = false,
                            Status = ReportWSStatusCode.ERROR,
                            ErrorWSCode = ReportWSErrorCode.UNSPECIFIED,
                            
                        };

                    }
                }

            }
            catch (TokenInvalidException)
            {
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_INVALID
                };
            }
            catch (EudoAdminInvalidRightException)
            {
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.TOKEN_RIGHT
                };
            }
            catch (EudoException ee)
            {
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.UNSPECIFIED,
                    ErrorDescription = ee.FullDebugMessage
                };
            }
            catch (EndpointNotFoundException ExWS)
            {

                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.WCF_UNAVAILABLE,
                    ErrorDescription = ExWS.Message

                };
            }
            catch (Exception e)
            {
                return new ReportLaunchResult()
                {
                    Success = false,
                    Status = ReportWSStatusCode.ERROR,
                    ErrorWSCode = ReportWSErrorCode.UNSPECIFIED,
                    ErrorDescription = e.Message

                };
            }

            return res;



        }

        private ResponseMessageResult GetResponse(object obj)
        {
            var rep = base.ResponseMessage(new HttpResponseMessage() { Content = new StringContent(SerializerTools.JsonSerialize(obj)) }); ;

            //ajout header cors
            rep.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            rep.Response.Headers.Add("Access-Control-Allow-Headers", "x-auth,content-type");
            rep.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,OPTIONS");

            return rep;
        }
    }
}