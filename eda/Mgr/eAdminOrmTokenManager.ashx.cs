using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using static Com.Eudonet.Common.Cryptography.CryptographyConst;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminOrmTokenManager
    /// </summary>
    public class eAdminOrmTokenManager : eAdminManager
    {

        /// <summary>
        /// Gestionnaire d'action token
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnGeneric res = new JSONReturnGeneric();

            try
            {
                //Seulement user >= 100
                if (_pref.User.UserLevel < 100)
                    throw new EudoAdminInvalidRightException();

                // 
                var def = new { a = 0, i = 0, n = "", id = 0, _pid = "", d = "", r = new List<int>(), t = (int)TokenType.ORM_WS };
                var jsonExtranetParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);

                switch (jsonExtranetParam.a)
                {

                    case 1:
                        //suppression
                        if (jsonExtranetParam.i == 0)
                            throw new ApiInvalidParameterException("id");

                        APPKEY.DeleteToken(_pref, _pref.User, jsonExtranetParam.i);

                        res.Success = true;
                        break;

                    case 2:
                        //Crea
                        DateTime d = DateTime.MinValue;

                        if (!string.IsNullOrEmpty(jsonExtranetParam.d))
                        {
                            try
                            {
                                d = DateTime.ParseExact(jsonExtranetParam.d, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                if (d < DateTime.Now)
                                    throw new EudoInvalidParameterException("date expiration", "superieur date du jour");
                            }
                            catch (EudoInvalidParameterException)
                            {
                                throw;
                            }
                            catch
                            {
                                throw new EudoInvalidParameterException("date expiration", "yyyy-MM-dd");
                            }
                        }
                        else
                            d = DateTime.MaxValue;


                        List<TokenRight> lstRights = new List<TokenRight>();
                        if (jsonExtranetParam.r != null && jsonExtranetParam.r.Count > 0)
                        {
                            foreach (int r in jsonExtranetParam.r)
                            {
                                var tr = eLibTools.GetEnumFromCode<TokenRight>(r);
                                if (tr != TokenRight.UNDEFINED && !lstRights.Contains(tr))
                                    lstRights.Add(tr);
                            }

                        }

                        



                        APPKEY tokenToCreate = APPKEY.GetEmptyKey();
                        tokenToCreate.AppName = jsonExtranetParam.n;
                        tokenToCreate.Type = eLibTools.GetEnumFromCode<TokenType>(jsonExtranetParam.t);
                        tokenToCreate.CreatedBy = _pref.UserId;
                        tokenToCreate.UserId = jsonExtranetParam.id;
                        tokenToCreate.ExpirationDate = d;
                        tokenToCreate.CreationDate = DateTime.Now;
                        tokenToCreate.Param = JsonConvert.SerializeObject(new { ApplicationName = jsonExtranetParam.n });
                        tokenToCreate.Rights = lstRights;
                        string sToken = APPKEY.CreateAppKeyToken(_pref, _pref.User, tokenToCreate);



                        /*
                        string sToken = APPKEY.CreateAppKeyToken(_pref, _pref.User, CryptographyConst.TokenType.ORM_WS, jsonExtranetParam.n,

                           JsonConvert.SerializeObject(
                               new { ApplicationName = jsonExtranetParam.n }
                               )
                            , jsonExtranetParam.id, d);
                        */


                        res.Success = true;
                        res.ErrorMsg = sToken;
                        break;

                    case 3:
                        // desactivation
                        if (jsonExtranetParam.i == 0)
                            throw new ApiInvalidParameterException("id");

                        APPKEY.DisableToken(_pref, _pref.User, jsonExtranetParam.i, jsonExtranetParam.d == "1");
                        res.Success = true;
                        break;
                    default:
                        res.Success = false;
                        res.ErrorCode = 1;
                        res.ErrorMsg = eResApp.GetRes(_pref, 8187); // Cette action n'est pas autorisée.
                        res.ErrorTitle = eResApp.GetRes(_pref, 8187);

                        break;

                }
            }
            catch (EudoException ee)
            {
                res.Success = false;
                res.ErrorCode = ee.ErrorCode;

                res.ErrorMsg = ee.UserMessage;
                res.ErrorTitle = ee.UserMessageTitle;
                res.ErrorDetailMsg = ee.UserMessageDetails;

                res.ErrorDebugMsg = ee.FullDebugMessage;
            }
            catch (Exception e)
            {

                res.Success = false;
                res.ErrorCode = 999;
                res.ErrorMsg = eResApp.GetRes(_pref, 72);
                res.ErrorDebugMsg = e.Message;

            }
            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }


    }
}