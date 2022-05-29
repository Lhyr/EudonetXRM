using Com.Eudonet.Common.Cryptography;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Core.Model.extranet;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminExtranetParamManager
    /// </summary>
    public class eAdminExtranetParamManager : eAdminManager
    {
        /// <summary>
        /// Traitement de la maj d'un param extranetz
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnExtranetParam res = new JSONReturnExtranetParam();

            try
            {

                //Modif Param depuis eUpdater
                var def = new { p = 0, i = 0, v = "", extid = 0, _pid = "" };
                var jsonExtranetParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);

                string suid = _context.Session["_uidupdater"]?.ToString() ?? "";

                //XSS
                if (suid.Length == 0 || jsonExtranetParam._pid != suid)
                    throw new EudoAdminInvalidRightException();

                //Extension Extranet configuré
                eExtension currentExtension = eExtension.GetExtensionByCode(_pref, "EXTRANET");
                if (currentExtension == null)
                    throw new eExtranetExceptionNotInstalled();


                eAdminExtensionExtranet currentAdminExtension = new eAdminExtensionExtranet(_pref);
                currentAdminExtension.SetDefaultParam(currentExtension);

                eExtranetParam currentExtranet = currentAdminExtension.lstExtranet.Find(a => a.Id == jsonExtranetParam.i);
                if (currentExtranet == null && jsonExtranetParam.p != 6)
                    throw new Exception("Extranet " + jsonExtranetParam.i + " non paramétré");
                else if (jsonExtranetParam.p == 6)
                {

                    int newId = 0;
                    if (currentAdminExtension.lstExtranet != null && currentAdminExtension.lstExtranet.Count > 0)
                        newId = currentAdminExtension.lstExtranet.OrderByDescending(ww => ww.Id).First().Id + 1;

                    currentExtranet = eExtranetParam.GetNewExtranetParam(_pref, newId);
                }

                switch (jsonExtranetParam.p)
                {
                    case 1:
                        //Nom Extranet
                        currentExtranet.Name = jsonExtranetParam.v;
                        break;
                    case 2:
                        //Actif/Inactif
                        currentExtranet.IsActive = (jsonExtranetParam.v == "1");
                        break;
                    case 3:
                        //TOML
                        // TODO : vérification détaillé de conformité & retour d'erreur détaillé
                        currentExtranet.TOML = jsonExtranetParam.v;
                        break;
                    case 4:
                        //Nombre d'utilisateurs concurrents
                        int n = 5;
                        int nMax = 200;
                        string sMax = eLibTools.GetServerConfig("extranetmaxuser", "200");

                        Int32.TryParse(sMax, out nMax);


                        Int32.TryParse(jsonExtranetParam.v, out n);
                        if (n > nMax || n < 0)
                        {
                            throw new EudoException("Extranet : Nombre d'utilisateurs concurrent hors limite", "Extranet : Nombre d'utilisateurs concurrent hors limite");
                        }

                        currentExtranet.MaxConccurentUser = Math.Min(n, nMax);
                        break;
                    case 5:
                        //Clé
                        currentExtranet.Token = eExtranetToken.GetExtranetToken(_pref, currentExtranet.Id);
                        break;
                    case 6:
                        //création nouveau extrant
                        if (currentAdminExtension.lstExtranet == null)
                            currentAdminExtension.lstExtranet = new System.Collections.Generic.List<eExtranetParam>();

                        if (currentAdminExtension.lstExtranet.Count >= 5)
                        {

                            throw new EudoException("", eResApp.GetRes(_pref, 2959).Replace("##NBMAX##","5"));
                        }

                        currentAdminExtension.lstExtranet.Add(currentExtranet);
                        break;
                    case 7:
                        // Suppression extranet
                        if (currentAdminExtension.lstExtranet == null)
                            currentAdminExtension.lstExtranet = new System.Collections.Generic.List<eExtranetParam>();

                        currentAdminExtension.lstExtranet.Remove(currentExtranet);


                        break;
                }

                //Vérification cohérence du token
                var def2 = new { Salt = "", DBUID = "", ExtranetID = 0, SubId = 0 };
                var t = JsonConvert.DeserializeAnonymousType(CryptoTripleDES.Decrypt(currentExtranet.Token, CryptographyConst.KEY_CRYPT_TOKEN), def2);

                currentExtension.Param = JsonConvert.SerializeObject(new { ext = currentAdminExtension.lstExtranet });
                res.Success = eExtension.UpdateExtension(currentExtension, _pref, _pref.User, eModelTools.GetRootPhysicalDatasPath(), _pref.ModeDebug);

                if (jsonExtranetParam.p != 6)
                {
                    res.ExtranetValue = JsonConvert.SerializeObject(new {
                        i = currentExtranet.Id,
                        a = currentExtranet.IsActive,
                        n = currentExtranet.Name,
                        t = currentExtranet.TOML,
                        k = currentExtranet.Token,
                        v = (t.SubId == 0 || t.DBUID != _pref.DatabaseUid || currentExtranet.Id != t.ExtranetID) ? "1" : "0"
                    }); ;

                }
                else
                {
                    if (jsonExtranetParam.extid > 0)
                    {
                        eAPIExtensionStoreAccess storeAccess = new eAPIExtensionStoreAccess(_pref);


                        var extension = storeAccess.GetExtensionFile(jsonExtranetParam.extid);

                        if (extension.Module == eUserOptionsModules.USROPT_MODULE.ADMIN_EXTENSIONS_EXTRANET)
                        {
                            eAdminStoreExtranetRenderer rdr = eAdminStoreExtranetRenderer.CreateAdminStoreExtranetRenderer(_pref, extension);



                            //dans le cas d'une création, on doit regénérer le bloc settings du rendu
                            res.ExtranetValue = JsonConvert.SerializeObject(new {
                                i = currentExtranet.Id,
                                content = GetResultHTML(rdr.ExtensionParametersContainer, true)
                            });
                        }
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
            finally
            {

            }


            RenderResult(RequestContentType.SCRIPT, delegate () { return SerializerTools.JsonSerialize(res); });
        }
    }


    /// <summary>
    /// JSON de  retour sur modification de paramètre
    /// Les valeurs courante de l'extranet sont retournées pour MAJ de l'UI
    /// </summary>
    public class JSONReturnExtranetParam : JSONReturnGeneric
    {
        /// <summary>
        ///  JSON "stringifier" de la configuration extranet
        /// </summary>
        public string ExtranetValue = "";
    }
}