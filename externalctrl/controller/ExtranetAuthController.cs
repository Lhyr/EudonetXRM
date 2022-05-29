using Com.Eudonet.Common.Extranet;
using Com.Eudonet.Core.Model.api;
using Com.Eudonet.Core.Model.extranet;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller externerne pour les appels centralisés à l'orm
    /// </summary>   
    public class ExtranetAuthController : ApiController
    {

        /// <summary>
        /// Ws de vérification du token extranet
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{nTab:int=0}/{nFileId:int}")]
        public IHttpActionResult Get(int nTab, int nFileId)
        {

            //retourne flux serialise
            return GetResponse(new { toto = nTab, titi = nFileId });
        }


        /// <summary>
        /// Ws de vérification du token extranet
        /// </summary>
        /// <returns></returns>

        public IHttpActionResult Get()
        {


            ExtranetAuthResult res = new ExtranetAuthResult();

            try
            {

                /*
                if (!EudoIPHelpers.IsAllowedIP(null))
                    throw new EudoException("Appel non local", "Appel non local");
                */


                if (!eLibTools.IsLocalOrEudoMachine())
                    throw new EudoException("Appel non local", "Appel non local");

                var token = Request.GetQueryNameValuePairs().SingleOrDefault(nv => nv.Key == "t");
                if (string.IsNullOrEmpty(token.Key) || string.IsNullOrEmpty(token.Value))
                    throw new EudoException("", "Token non fourni");


                CnxInfosExtranet app = null;
                CnxToken ct = null;
                eExtranetParam eparam = null;
                DateTime dtExpire;


                ePrefSpecifExtranet w = eApiTools.GetPrefSpecifFromExtranetToken(token.Value, true, out app, out ct, out dtExpire, out eparam);


                res.UserInfos = new ExtranetUserTokenInfos()
                {
                    ExpirationDate = dtExpire.ToString("yyyy/MM/dd HH:mm:ss"),

                    ADRID = w.ExtranetUserInfos.ADRID,
                    PPID = w.ExtranetUserInfos.PPID,
                    PMID = w.ExtranetUserInfos.PMID,
                    Lang = w.ExtranetUserInfos.Lang,

                };
                res.BaseName = w.BaseName;
                res.ExtranetId = w.ExtranetId;
                res.LoginDescid = (int)eparam.GetKeyValue<long>("account", "login");
                res.Success = true;


            }
            catch (EudoException ee)
            {
                res.ErrorCode = ee.ErrorCode;
                res.ErrorMsg = ee.UserMessage;
                res.ErrorDebugMsg = ee.DebugMessage;
                res.Success = false;

            }
            catch (Exception e)
            {
                res.ErrorCode = -1;
                res.ErrorMsg = "erreur";
                res.ErrorDebugMsg = e.Message;
                res.Success = false;
            }

            //retourne flux serialise
            return GetResponse(res);
        }

        private ResponseMessageResult GetResponse(object obj)
        {
            var rep = base.ResponseMessage(new HttpResponseMessage() { Content = new StringContent(SerializerTools.JsonSerialize(obj)) }); ;

            //ajout header cors
            rep.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            rep.Response.Headers.Add("Access-Control-Allow-Headers", "x-auth,content-type");

            return rep;
        }


        /// <summary>
        /// options - retourne les headers
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Options()
        {
            return GetResponse(null);

        }

        private static string GetHeader(HttpRequestMessage request, string key)
        {
            IEnumerable<string> keys = null;
            if (!request.Headers.TryGetValues(key, out keys))
                return null;

            return keys.First();
        }









    }






}
