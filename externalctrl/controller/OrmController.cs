using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.ORM;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using EudoCommonInterface;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller externerne pour les appels centralisés à l'orm
    /// </summary>   
    public class OrmController : ApiController
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

                if (ui.UserDisabled)
                    throw new EudoException("Utilisateur désactivé", "Utilisateur désactivé");
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


        /// <summary>
        /// Modification/creation
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Post(OrmCallArgsCRU value)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();

            APPKEY myKey;
            try
            {
                myKey = CheckArgs(value);

                value.UserId = myKey.UserId;
                value.ApplicationName = myKey.AppName;
                value.BaseName = myKey.BaseName;
            }
            catch (EudoException e)
            {

                res.Success = false;
                res.ErrorMsg = e.UserMessage;
                res.ErrorDebugMsg = e.Message;
                res.ErrorCode = 5;
                return GetResponse(res);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Flux Invalide";
                res.ErrorDebugMsg = e.Message;
                res.ErrorCode = 5;
                return GetResponse(res);
            }

            try
            {
                res = CallOrmOnly(value);
                //  res = CallViaEngine(value);

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


        private APPKEY CheckArgs(OrmCallArgs arg)
        {
            if (!eLibTools.IsLocalOrEudoMachine())
                throw new EudoException("Appel non local", "Appel non local");

            string auth = GetHeader(this.Request, "x-auth");

            if (auth == null)
                throw new EudoException("Token non fourni", "Token non fourni");

            if (string.IsNullOrEmpty(arg.BaseName))
                throw new EudoException("BaseName non fourni", "BaseName non fourni");


            APPKEY myKey;
            try
            {
                myKey = APPKEY.AppKeyFromToken(auth, CryptographyConst.TokenType.ORM_WS);
                if (arg.BaseName.ToUpper() != myKey.BaseName.ToUpper())
                    throw new EudoException("Token non valide pour cette base", "Token non valide pour cette base");

                if (myKey.Rights == null || !myKey.Rights.Contains(TokenRight.ORM_WS))
                    throw new EudoAdminInvalidRightException();
            }
            catch
            {
                //erreur de token
                throw;
            }

            if (arg.Operations == null || arg.Operations.Count == 0)
                throw new EudoException("Pas d'operations", "Pas d'operations");


            int nI = 0;
            foreach (var op in arg.Operations)
            {



                if (op == null)
                    throw new EudoException("Operation invalide pour batch line " + nI, "Operation invalide pour batch line " + nI);



                if (op.FilesId == null)
                    op.FilesId = new List<int>();


                if (op.FileId > 0 && !op.FilesId.Contains(op.FileId))
                {

                    op.FilesId.Add(op.FileId);
                }

                if (op.FilesId.Count == 0)
                    throw new EudoException("FileId invalide pour batch line " + nI, "FileId invalide pour batch line " + nI);




                if (op.Table <= 0)
                    throw new EudoException("Table invalide pour batch line " + nI, "Table invalide pour batch line " + nI);


                //pour operation crea/update UpdateFields obligatoire
                if ((op.Operation == 1) && (op.UpdateFields == null || op.UpdateFields.Count == 0))
                    throw new EudoException("Pas d'UpdateFields pour batch line " + nI, "Pas d'UpdateField pour batch line " + nI);
            }
            return myKey;
        }



        /// <summary>
        /// Suppression
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Delete(OrmCallArgsDelete value)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();
            OrmCallArgsDelete mc = value;

            APPKEY myKey;
            try
            {
                myKey = CheckArgs(value);



                value.UserId = myKey.UserId;
                value.ApplicationName = myKey.AppName;
                value.BaseName = myKey.BaseName;
            }
            catch (EudoException e)
            {

                res.Success = false;
                res.ErrorMsg = e.UserMessage;
                res.ErrorDebugMsg = e.Message;
                res.ErrorCode = 5;
                return GetResponse(res);
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorMsg = "Flux Invalide";
                res.ErrorDebugMsg = e.Message;
                res.ErrorCode = 5;
                return GetResponse(res);
            }



            try
            {
                res = CallOrmOnly(mc);

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


        private OrmCallResults CallViaEngine(OrmCallArgs args)
        {

            OrmCallResults res = OrmCallResults.GetEmpty();
            if (args == null || args.Operations == null || args.Operations.Count == 0)
            {
                res.Success = false;
                res.ErrorMsg = "Aucune opérations à traiter";
                res.ErrorCode = 6;
                return res;
            }


            string auth = GetHeader(this.Request, "x-auth");

            if (auth == null)
            {

                res.Success = false;
                res.ErrorMsg = "Token not found in header";
                return res;
            }


            ePref pref = eExternal.GetExternalPrefFromWSToken(auth, Common.Enumerations.TokenRight.ORM_WS);


            String sDatapath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, pref.GetBaseName);

            EngineResult engResAll = new EngineResult() { Success = true };

            foreach (var op in args.Operations)
            {

                EngineResult engRes = eLibTools.LaunchFormulaMulti(
                      pref,
                      pref.User,
                      sDatapath,
                      op.Table,
                      op.UpdateFields.ToList(),
                      op.FilesId,
                      new List<ExternalCommandType>() { ExternalCommandType.NOTIF, ExternalCommandType.ORM, ExternalCommandType.WORKFLOW },
                     op.Operation == 1 ? Framework.ORM.Connector.Contract.DataClusterOperation.UPDATE : Framework.ORM.Connector.Contract.DataClusterOperation.CREATE,
                     EngineContext.ORM);


                if (engRes.Success)
                {
                    engResAll.AddExternalResult(engRes);
                }
                else
                {
                    engResAll.Success = false;
                    engResAll.SetError(engResAll.Error);
                }
            }


            res.Success = engResAll.Success;
            if (!engResAll.Success && engResAll.Error != null)
            {
                res.ErrorDebugMsg = engResAll.Error.DebugMsg;
                res.ErrorMsg = engResAll.Error.Msg;
                res.ErrorCode = (int)engResAll.Error.TypeCriticity;
            }

            return res;


        }

        private OrmCallResults CallOrmOnly(OrmCallArgs args)
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