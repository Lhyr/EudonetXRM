using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.api;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExtensionAPIManager
    /// </summary>
    public class eAdminExtensionAPIManager : eAdminManager
    {
        /// <summary>
        /// Gestion des la mise à jour des paramètres d'extension pour l'API
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnExtensionAPI res = new JSONReturnExtensionAPI() { Success = false };

            //Seul le super admin peut modifier les params de l'extension
            if (_pref.User.UserLevel < 100)
                throw new EudoException("Modification non autorisée", "Modification non autorisée");

            try
            {
                //Paramètres à modifier
                string strAction = _requestTools.GetRequestFormKeyS("action");

                // extension enregistrée
                eExtension eRegisteredExt = eExtension.GetExtensionByCode(_pref, "API");

                if (eRegisteredExt.Status != EXTENSION_STATUS.STATUS_READY)
                    throw new EudoException("Extension non enregistrée", eResApp.GetRes(_pref, 2676));

                var def = new { action = "", param = "", value = "" };
                var jsonAPIParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);

                if (jsonAPIParam.param == "rate")
                {
                    APIParams p = new APIParams() { BASE_NAME = _pref.GetBaseName };
                    int nVal = 60;
                    if (Int32.TryParse(jsonAPIParam.value, out nVal))
                    {
                        //Liste des valeurs autorisées
                        List<int> allowedVal = new List<int>() { 60, 90, 120, 240, 360 };
                        if (!allowedVal.Contains(nVal))
                            throw new EudoException("Valeur de paramètre non autorisée", eResApp.GetRes(_pref, 8018));

                        //Appel au ws de l'api pour maj la limite

                        //récupère l'URL de l'API

                        try
                        {
                          
                            string sURLAPI = eModelTools.GetBaseUrl() +  "/eudoapi/utils/RegenRate/" + _pref.DatabaseUid + "/" + nVal;

                            WebClient client = new WebClient();
                            string content = client.DownloadString(sURLAPI  );

                            p.SetCall(nVal);
                            eRegisteredExt.Param = JsonConvert.SerializeObject(p);
                            string rootPhysicalDatasPath = eModelTools.GetRootPhysicalDatasPath();
                            if (eExtension.UpdateExtension(eRegisteredExt, _pref, _pref.User, rootPhysicalDatasPath, false))
                                res.Success = true;
                            else
                                throw new EudoException("Enregistrement impossible", eResApp.GetRes(_pref, 2677));
                        }
                        catch(EudoAdminException)
                        {
                            throw;
                        }
                        catch
                        {
                            throw new EudoException("Appel a l'API impossible", "Appel a l'API impossible");
                        }
                    }
                    else
                        throw new EudoException("Valeur de paramètre invalide", eResApp.GetRes(_pref, 8018));
                }
                else
                    throw new EudoException("Paramètre invalide", eResApp.GetRes(_pref, 8018));
            }
            catch (EudoException ee)
            {
                //return error
                res.ErrorMsg = ee.UserMessage;
                res.ErrorDetailMsg = ee.UserMessageDetails;
                res.ErrorTitle = ee.UserMessageTitle;
            }
            catch
            {
                res.ErrorMsg = "Une erreur est survenue";
            }

            RenderResult(RequestContentType.SCRIPT, delegate () { return EudoQuery.SerializerTools.JsonSerialize(res); });
        }
    }


    /// <summary>
    /// Retour de modif de param API
    /// </summary>
    public class JSONReturnExtensionAPI : JSONReturnGeneric
    {
        /// <summary>
        /// valeur pour le rate en cas d'erreur
        /// </summary>
        public string ratefallbackvalue = "60";

    }
}