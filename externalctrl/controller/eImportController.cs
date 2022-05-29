using Com.Eudonet.Common.Import;
using Com.Eudonet.Internal.Import;
using Com.Eudonet.Internal.wcfs.data.import;
using Com.Eudonet.Xrm.import;
using EudoQuery;
using System;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller pour declencher un import via code spécifique
    /// </summary>
    public class eImportController : ApiController
    {

        /// <summary>
        /// Get
        /// </summary>
        /// <param name="eImportByCodeParams"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Post(eImportByCodeParams eImportByCodeParams)
        {

            eImportByCodeResult results = eImportWizardStepFactory.Import(eImportByCodeParams);
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

            ImportCheckStatusResult res = new ImportCheckStatusResult()
            {
                ErrorWSCode = ImportWSErrorCode.UNSPECIFIED,
                Success = false

            };

            try
            {
                //retourne flux serialise
                var oImportInfos = GetImportStatus(id);

                res.Success = true;

                if (oImportInfos.Status == Internal.wcfs.data.common.eProcessStatus.SUCCESS
                    || oImportInfos.Status == Internal.wcfs.data.common.eProcessStatus.RUNNING
                    || oImportInfos.Status == Internal.wcfs.data.common.eProcessStatus.WAIT
                    )
                {
                    res.ErrorWSCode = ImportWSErrorCode.NONE;
                    res.Success = true;
                    res.IdImport = oImportInfos.ServerImportId;

                    res.StatusImport = (int)oImportInfos.Status;
                    res.DateStart = oImportInfos.DateCrea.ToString("yyyy/MM/dd HH:mm:ss");
                    res.DateEnd = oImportInfos.DateEnd.ToString("yyyy/MM/dd HH:mm:ss");
                    res.Percent = oImportInfos.CurrentImportProgress;

                    if (oImportInfos.SourceInfos != null)
                        res.TotalLines = oImportInfos.SourceInfos.LineCount;

                    if(!string.IsNullOrEmpty( oImportInfos.ReportLink))
                    {
                        res.ReportWebPath = oImportInfos.ReportLink;
                    }

                }
                else
                {

                    switch (oImportInfos.CodeResult)
                    {
                        case ImportResultCode.IdImportNotFound:
                            res.Success = false;
                            res.ErrorWSCode = ImportWSErrorCode.PARAMS_ID_IMPORT_NOTFOUND;
                            res.ErrorDescription = "Id import non trouvé";
                            break;
                        default:
                            res.Success = true;
                            res.ErrorWSCode = ImportWSErrorCode.EUDOPROCESS_ERROR;
                            res.IdImport = oImportInfos.ServerImportId;
                            res.StatusImport = (int)oImportInfos.Status;
                            res.DateStart = oImportInfos.DateStart.ToString("yyyy/MM/dd HH:mm:ss");
                            res.DateEnd = oImportInfos.DateEnd.ToString("yyyy/MM/dd HH:mm:ss");
                            res.Percent = oImportInfos.CurrentImportProgress;
                            if (oImportInfos.SourceInfos != null)
                                res.TotalLines = oImportInfos.SourceInfos.LineCount;
                            break;
                    }


                }
            }
            catch (ImportException ie)
            {
                switch (ie.Code)
                {
                    case ImportResultCode.ErrorOnCallingWCF:
                        res.ErrorWSCode = ImportWSErrorCode.EUDOPROCESS_WCF_UNAIVALBLE;
                        res.ErrorDescription = "wcf inaccessible";
                        res.InnerException = ie;
                        break;

                    default:
                        res.ErrorWSCode = ImportWSErrorCode.UNSPECIFIED;
                        res.ErrorDescription = "import exception " + ie.Code.ToString();
                        res.InnerException = ie;
                        break;
                }

            }
            catch (EudoException ee)
            {
                //erreur EudoException
                res.ErrorWSCode = ImportWSErrorCode.UNSPECIFIED;
                res.InnerException = ee;
                res.ErrorDescription = ee.UserMessage;

            }
            catch (Exception e)
            {
                //erreur inconnue
                res.ErrorWSCode = ImportWSErrorCode.UNSPECIFIED;
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
        private eImportInfos GetImportStatus(int id)
        {
            var hTok = Request.Headers.FirstOrDefault(h => h.Key == "x-auth");
            if (hTok.Value == null || hTok.Value?.Count() == 0)
                throw new EudoException("", "token non fourni");

            string sToken = hTok.Value.First();
            return eImportWizardStepFactory.CheckImportStatus(sToken, id);


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