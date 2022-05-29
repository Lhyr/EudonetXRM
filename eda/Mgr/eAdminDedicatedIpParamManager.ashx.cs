using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminDedicatedIpParamManager
    /// </summary>
    public class eAdminDedicatedIpParamManager : eAdminManager
    {
        /// <summary>
        /// Traitement de la maj d'un param IP Dédiée
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnDedicatedIpParam res = new JSONReturnDedicatedIpParam();

            try
            {
                //Modif Param depuis eUpdater
                var def = new { k = String.Empty, v = String.Empty };
                var jsonDedicatedIpParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);

                eExtension currentExtension = eExtension.GetExtensionByCode(_pref, "DEDICATEDIP");
                if (currentExtension == null)
                    throw new EudoAdminParameterException("Extension non installée");

                eAdminExtensionDedicatedIp currentAdminExtension = new eAdminExtensionDedicatedIp(_pref);
                currentAdminExtension.SetDefaultParam(currentExtension);

                if (String.Equals(jsonDedicatedIpParam.k, "DedicatedIp"))
                    currentAdminExtension.diSetting.DedicatedIp = jsonDedicatedIpParam.v;


                currentExtension.Param = JsonConvert.SerializeObject(currentAdminExtension.diSetting);
                res.Success = eExtension.UpdateExtension(currentExtension, _pref, _pref.User, eModelTools.GetRootPhysicalDatasPath(), _pref.ModeDebug);
                res.DedicatedIpValue = JsonConvert.SerializeObject(new { k = currentAdminExtension.diSetting.DedicatedIp });
            }
            catch (EudoException ee)
            {
                res.Success = false;
                res.ErrorTitle = ee.UserMessageTitle;
                res.ErrorMsg = ee.UserMessage;
                res.ErrorDetailMsg = ee.UserMessageDetails;
                res.ErrorCode = ee.ErrorCode;

                if (_pref.IsFromEudo || _pref.IsLocalIp)
                    res.ErrorDebugMsg = ee.FullDebugMessage;
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorTitle = eResApp.GetRes(_pref, 72);
                res.ErrorMsg = eResApp.GetRes(_pref, 6237) + eResApp.GetRes(_pref, 6236);
                res.ErrorCode = 999;

                if (_pref.IsFromEudo || _pref.IsLocalIp)
                    res.ErrorDebugMsg = e.Message + Environment.NewLine + e.StackTrace;
            }
            finally
            {

            }

            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }
    }

    /// <summary>
    /// JSON de  retour sur modification de paramètre
    /// Les valeurs courante de l'IP dédiée sont retournées pour MAJ de l'UI
    /// </summary>
    public class JSONReturnDedicatedIpParam : JSONReturnGeneric
    {
        /// <summary>
        ///  JSON "stringifier" de la configuration IP Dédiée
        /// </summary>
        public string DedicatedIpValue = "";
    }
}