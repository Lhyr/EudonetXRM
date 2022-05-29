using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Internal;
using EudoCommonInterface;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller externerne pour les appels centralisés à l'orm
    /// </summary>
    public class ormController : ApiController
    {



        private static Tuple<bool, CallOrm> GetCallOrm(OrmCallArgs mc)
        {
            CallOrm callOrm = null;
            ePrefSQL eprefLocal = null;
            eUserInfo ui = null;
            bool exec = false;

            //Creation call orm
            try
            {
                eprefLocal = ePrefTools.GetDefaultPrefSql(mc.BaseName);
                ui = eUserInfo.GetUserInfo(mc.UserId, eprefLocal);
            }
            catch
            {
                // Gestion erreur generation prefsql & userinfo
                throw;
            }


            callOrm = new CallOrm(eprefLocal, ui, (msg) => { });
            try
            {

                //Utilisation du cache statique
                OrmMappingInfo orminf = CallOrm.GetOrmMetaInfos(eprefLocal, ui, new OrmGetParams()
                {
                    CachePolicy = OrmMapCachePolicy.STATIC_CACHE,
                    ExceptionMode = OrmMappingExceptionMode.SAFE
                });

                if (orminf != null)
                    callOrm.GetterMappingInfos = () => { return orminf; };

                exec = callOrm.IfExecuteOrm();
            }
            catch
            {
                //erreur ormmapping
                throw;
            }


            return new Tuple<bool, CallOrm>(item1: exec, item2: callOrm);
        }
        /// <summary>
        /// test
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            OrmCallResults res = OrmCallResults.GetEmpty();

            //Fake argsd

            OrmCallArgsCRU mc = new OrmCallArgsCRU();

            mc.Operations.Add(
                new OperationORM()
                {
                    Table = 1100,
                    FileId = 80406,
                    Operation = 1,
                    UpdateFields = new HashSet<int>() { 1110 }
                });


            //récupération CallORM
            Tuple<bool, CallOrm> callOrmTPL = null;

            try
            {
                callOrmTPL = GetCallOrm(mc);
            }
            catch (Exception e)
            {
                //erreur Appel effectif
                res.Success = false;
                res.ErrorMsg = "Erreur lors de l'analyse du retour de connecteur";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 4;

                return GetResponse(res);
            }

            //Si ORM ok, lance l'appel
            if (callOrmTPL.Item1)
            {
                CallOrm callOrm = callOrmTPL.Item2;
                res = callOrm.CallOrmCRUD(mc);
            }

            //retourne flux serialise
            return GetResponse(res);
        }

        private ResponseMessageResult GetResponse(object obj)
        {
            return base.ResponseMessage(new HttpResponseMessage() { Content = new StringContent(SerializerTools.JsonSerialize(obj)) }); ;
        }



        /// <summary>
        /// Modification
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Post([FromBody] string value)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();
            OrmCallArgsCRU mc = null;

            try
            {
                mc = SerializerTools.JsonDeserialize<OrmCallArgsCRU>(value);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Impossible de désérialiser le flux";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 5;
                return GetResponse(res);
            }

            try
            {
                res = Call(mc);

                //retourne flux serialise
                return GetResponse(res);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Erreur non gérée";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 999;
                return GetResponse(res);
            }
        }



        /// <summary>
        /// Suppression
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Delete([FromBody] string value)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();
            OrmCallArgsDelete mc = null;

            try
            {
                mc = SerializerTools.JsonDeserialize<OrmCallArgsDelete>(value);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Impossible de désérialiser le flux";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 5;
                return GetResponse(res);
            }



            try
            {
                res = Call(mc);

                //retourne flux serialise
                return GetResponse(res);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Erreur non gérée";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 999;
                return GetResponse(res);
            }
        }


        private OrmCallResults Call(OrmCallArgs args)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();


            if (args == null || args.Operations == null || args.Operations.Count == 0)
            {
                res.Success = false;
                res.ErrorMsg = "Aucune opérations à traiter";
                res.ErrorCode = 6;
                return res;
            }

            //récupération CallORM
            Tuple<bool, CallOrm> callOrmTPL = null;

            try
            {
                callOrmTPL = GetCallOrm(args);
            }
            catch (Exception e)
            {
                //erreur Appel effectif
                res.Success = false;
                res.ErrorMsg = "Erreur lors de l'analyse du retour de connecteur";
                res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
                res.ErrorCode = 4;

                return res;
            }

            //Si ORM ok, lance l'appel
            if (callOrmTPL.Item1)
            {
                CallOrm callOrm = callOrmTPL.Item2;
                res = callOrm.CallOrmCRUD(args);
            }

            //retourne flux serialise
            return res;
        }

    }
}