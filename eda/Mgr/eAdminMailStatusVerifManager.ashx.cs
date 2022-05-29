using Com.Eudonet.Common;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using Com.Eudonet.Internal.eda;
using EudoCommonInterface.mailchecker;
using EudoQuery;
using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Com.Eudonet.Xrm.eda.Mgr
{
    /// <summary>
    /// Description résumée de eAdminMailStatusVerifManager
    /// </summary>
    public class eAdminMailStatusVerifManager : eAdminManager
    {
        /// <summary>
        /// Traitement de la vérification des statuts des mails
        /// </summary>
        protected override void ProcessManager()
        {
            JSONReturnMailStatusVerif res = new JSONReturnMailStatusVerif();

            try
            {
                OperationType _OperationType = OperationType.Uncknown;
                bool _oldMail = false;

                //Modif Param depuis eUpdater
                var def = new { OperationType = String.Empty, Old = String.Empty };
                var jsonMailVerifParam = eAdminTools.DeserializeAnonymousTypeFromStream(_context.Request.InputStream, def);


                if (!String.IsNullOrEmpty(jsonMailVerifParam.Old))
                    _oldMail = jsonMailVerifParam.Old == "1";

                if (!String.IsNullOrEmpty(jsonMailVerifParam.OperationType))
                    _OperationType = (OperationType)int.Parse(jsonMailVerifParam.OperationType);

                if (_OperationType == OperationType.Counting)
                {
                    res.MailToCheckCount = Internal.mailchecker.eMailCheckerTools.CountMailToCheck(_pref, _oldMail);
                    res.MessageFromService = string.Format(eResApp.GetRes(_pref, 2982), res.MailToCheckCount);
                    res.Success = true;
                }
                else if (_OperationType == OperationType.LunchVerif)
                {
                    res.MailCheckerLaunchVerifResult = Internal.mailchecker.eMailCheckerTools.LaunchVerificationJob(_pref, new EudoCommonInterface.mailchecker.MailCheckParam
                    {
                        DateMax = _oldMail ? DateTime.Now.AddDays(-365) : DateTime.MaxValue
                    });

                    if (res.MailCheckerLaunchVerifResult.Success)
                    {
                        res.MessageFromService = string.Format(eResApp.GetRes(_pref, 2979), res.MailCheckerLaunchVerifResult.DateSchedule.ToString("dd/MM/yyyy HH:mm"));
                        res.Success = true;

                        //update extension table with id
                        var lst = eExtension.GetExtensionsByCode(_pref, "VERIFY");
                        if (lst.Count > 0)
                        {
                            eExtension ext = lst[0];
                            var defExt = new { idquery = res.MailCheckerLaunchVerifResult.MailVerifQueryId, DateSchedule = res.MailCheckerLaunchVerifResult.DateSchedule };
                            ext.Param = JsonConvert.SerializeObject(defExt);
                            eExtension.UpdateExtension(ext, _pref, _pref.User, eModelTools.GetRootPhysicalDatasPath(), _pref.ModeDebug);

                        }
                    }
                    else
                    {
                        res.ErrorCode = res.MailCheckerLaunchVerifResult.ErrorCode;
                        res.ErrorTitle = res.MailCheckerLaunchVerifResult.ErrorTitle;
                        res.ErrorMsg = res.MailCheckerLaunchVerifResult.ErrorDescription;
                        res.ErrorDetailMsg = res.MailCheckerLaunchVerifResult.ErrorDetail;

                        if (_pref.User.UserLevel >= 99)
                            res.ErrorDebugMsg = res.MailCheckerLaunchVerifResult.ErrorCode + "/" + res.MailCheckerLaunchVerifResult.ErrorSubCode + Environment.NewLine + res.MailCheckerLaunchVerifResult.ErrorDevInfos;
                        res.Success = false;
                    }
                }
                else if (_OperationType == OperationType.CheckStatus)
                {
                    //
                    var rMailCheckerStatus = Internal.mailchecker.eMailCheckerTools.GetMailVerifyQueryInfos(_pref);
                    if (!rMailCheckerStatus.Success)
                    {
                        res.ErrorCode = res.MailCheckerLaunchVerifResult.ErrorCode;
                        res.ErrorTitle = res.MailCheckerLaunchVerifResult.ErrorTitle;
                        res.ErrorMsg = res.MailCheckerLaunchVerifResult.ErrorDescription;
                        res.ErrorDetailMsg = res.MailCheckerLaunchVerifResult.ErrorDetail;
                        if (_pref.User.UserLevel >= 99)
                            res.MessageFromService = res.MailCheckerLaunchVerifResult.ErrorCode + "/" + res.MailCheckerLaunchVerifResult.ErrorSubCode + Environment.NewLine + res.MailCheckerLaunchVerifResult.ErrorDevInfos;
                        res.Success = false;
                    }
                    else
                        res.Success = true;
                }
            }
            catch (EudoException ee)
            {
                res.Success = false;
                res.ErrorTitle = ee.UserMessageTitle;
                res.ErrorMsg = ee.UserMessage;

                if (_pref.User.UserLevel >= 99)
                    res.ErrorDebugMsg = ee.Message;

                res.ErrorCode = ee.ErrorCode;
            }
            catch (Exception e)
            {
                res.Success = false;
                res.ErrorDebugMsg = e.Message;
                res.ErrorCode = (int)CheckMailsExceptionCode.OTHER;
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
    public class JSONReturnMailStatusVerif : JSONReturnGeneric
    {
        /// <summary>
        ///  Nombre des mails à vérifier
        /// </summary>
        public int MailToCheckCount { get; set; }

        /// <summary>
        /// Objet retour du service de vérification des mails
        /// </summary>
        [IgnoreDataMember]
        public eMailCheckerLaunchVerifResult MailCheckerLaunchVerifResult;




        /// <summary>
        /// Message de confirmation avant le lancement du job de vérif ou après lancement
        /// </summary>
        public string MessageFromService { get; set; }

    }

    enum OperationType
    {
        Counting = 1,
        LunchVerif = 2,

        Uncknown = 3,

        /// <summary>
        /// Vérification du status du job
        /// </summary>
        CheckStatus = 4


    }
}