using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Engine;
using Com.Eudonet.Engine.Notif;
using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Results;

namespace Com.Eudonet.Xrm.externalctrl
{
    /// <summary>
    /// Controller externerne pour les appels centralisés à l'orm
    /// </summary>   
    public class eNotificationController : ApiController
    {



        /// <summary>
        /// test
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            return GetResponse(new { success = true });
        }


        /// <summary>
        /// Création d'une Notification Customizable
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpActionResult Put(NotificationCallArgsCustom value)
        {

            NotificationWSResult result = new NotificationWSResult()
            {
                Success = false
            };


            try
            {

                string auth = GetHeader(this.Request, "x-auth");

                if (auth == null)
                {
                    result.Success = false;
                    result.ErrorMsg = "Token not found in header";
                }
                else
                {

                    // Icon
                    if (!string.IsNullOrEmpty(value.Icon) && !eFontIcons.GetFonts().ToDictionary(x => x.Key, x => x.Value).ContainsKey(value.Icon.ToLower()))
                    {
                        result.Success = false;
                        result.ErrorMsg = "Icon invalid";
                        return GetResponse(result);
                    }

                    //Taille texte
                    if (!string.IsNullOrEmpty(value.LongText) && value.LongText.Length > 1999)
                    {
                        result.Success = false;
                        result.ErrorMsg = "LongText must be under 2000 char.";
                        return GetResponse(result);
                    }

                    if (!string.IsNullOrEmpty(value.ShortText) && value.ShortText.Length > 1999)
                    {
                        result.Success = false;
                        result.ErrorMsg = "ShortText must be under 2000 char.";
                        return GetResponse(result);
                    }



                    //validation couleur
                    if (value.Expire < 0 || value.Expire > 365)
                    {
                        result.Success = false;
                        result.ErrorMsg = "Expiration between 1 and 365 days";
                        return GetResponse(result);
                    }


                    ePref pref = eExternal.GetExternalPrefFromWSToken(auth, Common.Enumerations.TokenRight.NOTIFICATIONS_STANDARD);


                    //validation couleur
                    if (string.IsNullOrEmpty(value.Color))
                        value.Color = pref.ThemeXRM.Color;

                    var regExp = new Regex(@"^#([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (!regExp.IsMatch(value.Color))
                    {
                        result.Success = false;
                        result.ErrorMsg = "Color invalid. Use hexa representation";
                        return GetResponse(result);
                    }


                    NotifEngineFactory
                            .CreateExpressNotifier(pref, broadcast: NotifConst.Broadcast.XRM)
                            .Notify(value, NotifConst.Type.VALIDATE, NotifConst.Broadcast.XRM);


                    result.Success = true;
                }
            }
            catch (EudoException ee)
            {
                result.Success = false;
                result.ErrorMsg = ee.UserMessage;
                result.ErrorDebugMsg = ee.FullDebugMessage;
            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrorMsg = e.Message;

            }

            return GetResponse(result);
        }


        /// <summary>
        /// Modification/creation
        /// </summary>
        /// <param name="value"></param>
        public IHttpActionResult Post(NotificationCallArgsCRU value)
        {
            NotificationWSResult res = new NotificationWSResult()
            {
                Success = false
            };

            return GetResponse(res);

            try
            {

                CheckArgs(value);

                string auth = GetHeader(this.Request, "x-auth");

                if (auth == null)
                {

                    res.Success = false;
                    res.ErrorMsg = "Token not found in header";
                    return GetResponse(res);
                }

                ePref pref = eExternal.GetExternalPrefFromWSToken(auth, Common.Enumerations.TokenRight.NOTIFICATIONS_STANDARD);


                String sDatapath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, pref.GetBaseName);

                EngineResult engResAll = new EngineResult() { Success = true };

                foreach (var op in value.Operations)
                {

                    EngineResult engRes = eLibTools.LaunchFormulaMulti(
                          pref,
                          pref.User,
                          sDatapath,
                          op.Table,
                          op.UpdateFields.ToList(),
                          op.FilesId,
                          new List<ExternalCommandType>() { ExternalCommandType.NOTIF },
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


        private ResponseMessageResult GetResponse(object obj)
        {
            var rep = base.ResponseMessage(new HttpResponseMessage() { Content = new StringContent(SerializerTools.JsonSerialize(obj)) }); ;

            //ajout header cors
            rep.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            rep.Response.Headers.Add("Access-Control-Allow-Methods", "*");
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




        private void CheckArgs(NotificationCallArgsCRU arg)
        {
            if (!eLibTools.IsLocalOrEudoMachine())
                throw new EudoException("Appel non local", "Appel non local");

            string auth = GetHeader(this.Request, "x-auth");

            if (auth == null)
                throw new EudoException("Token non fourni", "Token non fourni");



            if (arg.Operations == null || arg.Operations.Count == 0)
                throw new EudoException("Pas d'operations", "Pas d'operations");


            int nI = 0;
            foreach (var op in arg.Operations)
            {


                if (op == null)
                    throw new EudoException("Operation invalide pour batch line " + nI, "Operation invalide pour batch line " + nI);

                if (op.FilesId == null || op.FilesId.Count <= 0)
                    throw new EudoException("FileId invalide pour batch line " + nI, "FileId invalide pour batch line " + nI);

                if (op.Table <= 0)
                    throw new EudoException("Table invalide pour batch line " + nI, "Table invalide pour batch line " + nI);

                if (op.Operation < 0 || op.Operation > 1)
                    throw new EudoException("Type d'opération invalide (0 : create, 1 : Update)");

                if ((op.UpdateFields == null || op.UpdateFields.Count == 0))
                    throw new EudoException("Pas d'UpdateFields pour batch line " + nI, "Pas d'UpdateField pour batch line " + nI);

                if (op.UpdateFields.Any(fld => op.Table != fld - fld % 100))
                    throw new EudoException("Les champs UpdateFields doivent appartenir à la table de l'opération");

                nI++;
            }

        }




    }
}