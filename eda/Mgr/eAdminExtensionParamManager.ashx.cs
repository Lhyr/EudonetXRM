using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;
using System;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExtranetParamManager
    /// </summary>
    public class eAdminExtensionParamManager : eAdminManager
    {
        /// <summary>
        /// Traitement de la maj d'un param extranetz
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnParam res = new JSONReturnParam();

            string paramsValue = _requestTools.GetRequestFormKeyS("params");
            int? idExtension = _requestTools.GetRequestFormKeyI("idExtension");

            try
            {
                //the id of extension not null or 0
                if(idExtension != 0)
                {
                    //update EXTENSIONPARAM
                    string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();
                    bool isUpdated = eExtension.UpdateExtensionParams(paramsValue,(int)idExtension,_pref, _pref.User,rootPhysicalDatasPath,_pref.ModeDebug);

                    if (isUpdated)
                        res.Success = true;
                    else
                    {
                        res.Success = false;
                        res.ErrorTitle = eResApp.GetRes(_pref, 72);

                        res.ErrorMsg = eResApp.GetRes(_pref, 6237) + eResApp.GetRes(_pref, 6236);
                        res.ErrorDetailMsg = "";
                    }                       
                }
            }
            catch (EudoException ee)
            {
                res.Success = false;
                res.ErrorTitle = ee.UserMessageTitle;
                res.ErrorMsg = ee.UserMessage;
                res.ErrorDetailMsg = ee.UserMessageDetails;


                res.ErrorCode = ee.ErrorCode;

                if (_pref.IsFromEudo || _pref.IsLocalIp)
                {
                    res.ErrorDebugMsg = ee.FullDebugMessage;
                }


            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorTitle = eResApp.GetRes(_pref, 72);

                res.ErrorMsg = eResApp.GetRes(_pref, 6237) + eResApp.GetRes(_pref, 6236);
                res.ErrorDetailMsg = "";


                res.ErrorCode = 999;

                if (_pref.IsFromEudo || _pref.IsLocalIp)
                    res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;

            }
          
            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }
    }


    /// <summary>
    /// JSON de  retour sur modification de paramètre
    /// Les valeurs courante de l'extranet sont retournées pour MAJ de l'UI
    /// </summary>
    public class JSONReturnParam : JSONReturnGeneric
    {
        /// <summary>
        ///  return success
        /// </summary>
        public Boolean Success = true;
    }
}