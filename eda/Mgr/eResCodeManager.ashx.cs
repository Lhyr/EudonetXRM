using Com.Eudonet.Internal;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Threading;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eResCodeManager
    /// </summary>
    public class eResCodeManager : eAdminManager
    {
        /// <summary>
        /// Action du manager
        /// </summary>
        public enum MgrAction
        {
            /// <summary>
            /// Indéfini
            /// </summary>
            Undefined = 0,
            /// <summary>
            /// Création d'un nouveau ResCode
            /// </summary>
            Create = 1,
            /// <summary>
            /// Mise à jour d'un ResCode
            /// </summary>
            Update = 2

        }



        /// <summary>
        /// Processes the manager.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        protected override void ProcessManager()
        {
            int action = _requestTools.GetRequestFormKeyI("action") ?? 0;
            MgrAction mgrAction = (MgrAction)action;

            if (mgrAction == MgrAction.Undefined)
            {
                ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "", eResApp.GetRes(_pref, 6524));
                LaunchError();
                return;
            }

            string value = _requestTools.GetRequestFormKeyS("value") ?? string.Empty;
            int resCode = _requestTools.GetRequestFormKeyI("code") ?? 0;

            eResCode rCode;

            eudoDAL dal = eLibTools.GetEudoDAL(_pref);
            dal.OpenDatabase();

            try
            {

                MgrResult result = new MgrResult();
                result.Success = false;

                switch (mgrAction)
                {
                    case MgrAction.Create:
                        #region Création
                        resCode = 0;
                        eResLocation resLoc = null;
                        string jsonResLoc = _requestTools.GetRequestFormKeyS("location") ?? string.Empty;

                        if (!String.IsNullOrEmpty(jsonResLoc))
                        {
                            resLoc = JsonConvert.DeserializeObject<eResLocation>(jsonResLoc);
                        }

                        rCode = new eResCode(_pref, dal, resCode, value, resLoc);
                        resCode = rCode.CreateNewResCode();
                        if (resCode > 0)
                        {
                            result.Success = true;
                            result.ResCode = resCode;
                            result.ResAlias = rCode.GetAlias();
                        }
                        else
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", rCode.UserError, "", rCode.DevError);
                            LaunchError();
                        }

                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(result); });
                        #endregion
                        break;
                    case MgrAction.Update:
                        #region Mise à jour
                        rCode = new eResCode(_pref, dal, resCode, value);
                        if (rCode.Update())
                        {
                            result.Success = true;
                            result.ResCode = resCode;
                            result.ResAlias = rCode.GetAlias();
                        }
                        else
                        {
                            ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", rCode.UserError, "", rCode.DevError);
                            LaunchError();
                        }

                        RenderResult(RequestContentType.TEXT, delegate () { return JsonConvert.SerializeObject(result); });
                        #endregion
                        break;
                }
            }
            catch (eEndResponseException) { _context.Response.End(); }
            catch (ThreadAbortException)
            {
            }
            catch (Exception e)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, "", eResApp.GetRes(_pref, 72), e.Message, e.StackTrace);
                LaunchError();
            }
            finally
            {
                dal.CloseDatabase();
            }



        }


        public class MgrResult
        {
            public bool Success { get; set; }
            public int ResCode { get; set; }
            public string ResAlias { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}