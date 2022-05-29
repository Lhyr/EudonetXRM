using Com.Eudonet.Engine.Result;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Cryptography;
using Newtonsoft.Json;
using Com.Eudonet.Merge;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;
using Com.Eudonet.Engine;
using System.Threading.Tasks;
using System.Globalization;
using Com.Eudonet.Internal.Payment;
using static Com.Eudonet.Internal.Payment.IngenicoRestAPICall;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Description résumée de eUsrFrmManager
    /// </summary>
    public class eUsrFrmManager : eExternalManager, System.Web.SessionState.IRequiresSessionState
    {
        static string TEMPORARY_FOLDER_NAME = "tmp";
        static List<string> acceptedImageFormats = new List<string>() { "png", "gif", "jpeg", "jpg" };

        //frm.FormularLite _eFrmLite = null;
        eFormularFile _eFormFile = null;
        eFormularFileRenderer rend = null;
        LoadQueryStringForm _dataParam = null;

        /// <summary>
        /// Type de page externe
        /// </summary>
        protected override eExternal.ExternalPageType PgTyp
        {
            get { return eExternal.ExternalPageType.FORMULAR; }
        }

        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            eRequestTools requestTools = new eRequestTools(_context);
            string action = requestTools.GetRequestFormKeyS("action");

            if (action == "FILE_UPLOAD") //TODO Enum
            {
                RenderXmlResponse(GetUploadedFilesSrcUrl(requestTools));
            }
            else
            {
                if (ValidatePost())
                {
                    EngineResult eng = _eFormFile.SaveFormular(_context, _pref);
                    if (eng.Success)
                    {
                        // Déplace les images de dossier temporraire au dossier file
                        MoveImageFilesFromTmpFolder(_pref, _eFormFile.NewFields, requestTools);

                        //Sauvegarde la date de soumission
                        if (_eFormFile.SaveSubmissionDate())
                        {
                            // Init le document XML
                            XmlDocument xmlResult = new XmlDocument();

                            // Init le document XML
                            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                            xmlResult.AppendChild(mainNode);

                            XmlNode rootNode = xmlResult.CreateElement("root");
                            xmlResult.AppendChild(rootNode);

                            XmlNode successNode = xmlResult.CreateElement("success");
                            rootNode.AppendChild(successNode);

                            XmlNode modeNode = xmlResult.CreateElement("mode");
                            rootNode.AppendChild(modeNode);

                            //test sur le montant du paiement
                            if (!string.IsNullOrEmpty(_requestTools.GetRequestFormKeyS("isOnlinePaymentBtn")) && _requestTools.GetRequestFormKeyS("isOnlinePaymentBtn").Trim().Equals("1"))
                            {
                                successNode.InnerText = "1";
                                modeNode.InnerText = "DIALOG";


                                if (!validateAmountForOnlinePayment(_eFormFile.FormularId, _eFormFile.EvtFileId, _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0]))
                                {
                                    successNode.InnerText = "1";
                                    modeNode.InnerText = "DIALOG";

                                    XmlNode dialogType = xmlResult.CreateElement("dialogType");
                                    rootNode.AppendChild(dialogType);
                                    dialogType.InnerText = "AMOUNT";

                                    //on génére le lien pour rediriger l'utilisateur vers le formulaire à jour
                                    FormularBuildParam fbp = new FormularBuildParam();
                                    fbp.Uid = _pageQueryString.UID;
                                    fbp.AppExternalUrl = "";
                                    fbp.FormularId = _eFormFile.FormularId;
                                    fbp.ParentFileId = _eFormFile.EvtFileId;
                                    fbp.TplFileId = _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0];
                                    var url = string.Concat(ExternalUrlTools.GetLinkFormular(fbp));

                                    XmlNode redirectionUrl = xmlResult.CreateElement("redirectionUrl");
                                    rootNode.AppendChild(redirectionUrl);
                                    redirectionUrl.InnerText = string.Concat(eModelTools.GetBaseUrlXRM().TrimEnd('/'), url);
                                }
                                else
                                {

                                    int nFileId = _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0];


                                    bool bCheck = Task.Run(async () => await WLTransactionTools.VerifyTargetTransaction(_pref, _eFormFile.Tab, nFileId, "", EngineContext.APPLI)).Result;





                                    //recherche des transaction de cette fiche
                                    bool bAlreadyPaid = WLTransactionTools.TargetHasPaidTransaction(_pref, _eFormFile.Tab, nFileId);

                                    //si aucune en status payé, on redirige vers la page de payment
                                    if (!bAlreadyPaid)
                                    {
                                        string payTranFormularInfos = _requestTools.GetRequestFormKeyS("onlinePaymentParams");
                                        var jDef = new { edndPa = 0, edndTr = 0, edndPi = 0 };
                                        var jSer = JsonConvert.DeserializeAnonymousType(payTranFormularInfos, jDef);
                                        decimal amount = GetAmount(_eFormFile.FormularId, _eFormFile.EvtFileId, _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0]);
                                        string amountStr = amount.ToString();
                                        //on génére le lien pour rediriger l'utilisateur vers le formulaire à jour
                                        FormularBuildParam fbp = new FormularBuildParam();
                                        fbp.Uid = _pageQueryString.UID;
                                        fbp.AppExternalUrl = eLibTools.GetAppUrl(this._context.Request);
                                        fbp.FormularId = _eFormFile.FormularId;
                                        fbp.ParentFileId = _eFormFile.EvtFileId;
                                        fbp.TplFileId = _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0];
                                        fbp.TranDescId = jSer.edndTr;
                                        var url = string.Concat(ExternalUrlTools.GetLinkFormular(fbp));

                                        eWorldlinePaymentSetting worldlineSettings = eLibTools.GetSerializedWorldlineSettingsExtension(_pref);
                                        IngenicoRestAPICall apiCall = new IngenicoRestAPICall(_pref, worldlineSettings, url);

                                        //Appel à l'API Ingénico Ogone

                                        // Amount doit etre converti en centime
                                        amount *= 100;
                                        IngenicoCreateTransactionResult ingenicoResult = Task.Run(async () => await apiCall.Process(Decimal.ToInt64(amount))).Result;

                                        if (ingenicoResult != null)
                                        {
                                            if (string.IsNullOrEmpty(ingenicoResult.ErrorId))
                                            {
                                                string hostedCheckoutUrl = $"https://payment.{ingenicoResult.PartialRedirectUrl}";
                                                successNode.InnerText = "1";
                                                modeNode.InnerText = "REDIRECT";

                                                XmlNode urlNode = xmlResult.CreateElement("url");
                                                rootNode.AppendChild(urlNode);
                                                urlNode.InnerText = hostedCheckoutUrl;
                                            }
                                            else
                                            {
                                                //Gestion d'erreur
                                                this.ErrorContainer = eErrorContainer.GetDevUserError(
                                                   eLibConst.MSG_TYPE.CRITICAL,
                                                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                                   string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                                   eResApp.GetRes(_pref, 72),  //   titre
                                                   string.Concat("Error message : ", _eFormFile.ErrorMsg));
                                                LaunchError();
                                            }

                                            int fileid = _eFormFile.TplFileId > 0 ? _eFormFile.TplFileId : eng.NewRecord.FilesId[0];
                                            //add transaction and target table

                                            //
                                            var def = new { edndPa = 0, edndTr = 0, edndPi = 0 };
                                            var t = JsonConvert.DeserializeAnonymousType(_requestTools.GetRequestFormKeyS("onlinePaymentParams"), def);


                                            AddIngenicoResultToTransactionTable(ingenicoResult, amountStr, fileid, JsonConvert.SerializeObject(t));


                                        }
                                        else
                                        {
                                            //Gestion d'erreur
                                            this.ErrorContainer = eErrorContainer.GetDevUserError(
                                               eLibConst.MSG_TYPE.CRITICAL,
                                               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                               string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                               eResApp.GetRes(_pref, 72),  //   titre
                                               string.Concat("Aucune réponse de l'API Ingénico : ", _eFormFile.ErrorMsg));
                                            LaunchError();
                                        }
                                    }
                                    else
                                    {
                                        //transaction déjà payée

                                        this.ErrorContainer = eErrorContainer.GetDevUserError(
                                           eLibConst.MSG_TYPE.CRITICAL,
                                           eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                           string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                                           eResApp.GetRes(_pref, 72),  //   titre
                                           string.Concat("Transaction déjà payée"));

                                        LaunchError();
                                        return;
                                    }
                                }
                            }
                            else if (_eFormFile.SubmissionRedirectUrl.Length > 0)
                            {
                                successNode.InnerText = "1";
                                modeNode.InnerText = "REDIRECT";

                                XmlNode urlNode = xmlResult.CreateElement("url");
                                rootNode.AppendChild(urlNode);
                                urlNode.InnerText = _eFormFile.SubmissionRedirectUrl;
                            }
                            else
                            {
                                int targetFileId = _dataParam.ParamData.TplFileId;

                                if (targetFileId == 0 && eng.Success && eng.NewRecord?.FilesId?.Count == 1)
                                    targetFileId = eng.NewRecord.FilesId[0];

                                #region TODO - Vu existant, on fait comme ca mais c'est pourri de base Il faut refaire ! #59 351
                                rend = eRendererFactory.CreateFormularFileRenderer(
                                    _dataParam.CsData.UID,
                                    eLibTools.GetAppUrl(_context.Request),
                                    _pref,
                                   _dataParam.CsData.FormularId,
                                   _dataParam.ParamData.ParentFileId,
                                   targetFileId);


                                if (rend.ErrorMsg.Length > 0)
                                {
                                    //Gestion d'erreur
                                    this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, "MSG", "DETAILMSG", "TITLEMSG", string.Format("ProcessManager => {0}", rend.ErrorMsg));
                                    LaunchError();
                                }
                                #endregion

                                successNode.InnerText = "1";
                                modeNode.InnerText = "MSG";

                                XmlNode msgNode = xmlResult.CreateElement("msg");
                                rootNode.AppendChild(msgNode);
                                msgNode.InnerText = eLibTools.CleanXMLChar(rend.BodySubmissionMerge);

                                XmlNode cssNode = xmlResult.CreateElement("css");
                                rootNode.AppendChild(cssNode);
                                cssNode.InnerText = eLibTools.CleanXMLChar(_eFormFile.BodySubmissionCss);
                            }

                            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });

                        }
                        else
                        {
                            //Gestion d'erreur
                            this.ErrorContainer = eErrorContainer.GetDevUserError(
                               eLibConst.MSG_TYPE.CRITICAL,
                               eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                               string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                               eResApp.GetRes(_pref, 72),  //   titre
                               string.Concat("Sauvegarde la date de soumission impossible : ", _eFormFile.ErrorMsg));
                            LaunchError();
                        }
                    }
                    else
                    {
                        //Gestion d'erreur
                        this.ErrorContainer = eErrorContainer.GetUserError(eng.Error.TypeCriticity, eng.Error.Msg, eng.Error.Detail, eng.Error.Title);
                        LaunchError();
                    }
                }
            }

        }

        /// <summary>
        /// retour d'ingénico
        /// </summary>
        /// <param name="ingenicoResult">resultat d'ingénico</param>
        /// <param name="amountStr">montant</param>
        /// <param name="fileid">id de la  fiche target</param>
        /// <param name="sFormularInfos">Information sur le formulaire initialisant la transaction</param>
        /// 

        public void AddIngenicoResultToTransactionTable(IngenicoCreateTransactionResult ingenicoResult, string amountStr, int fileid, string sFormularInfos)
        {
            #region Create transaction and target table
            //Create engine transaction
            Engine.Engine engTransaction = eModelTools.GetEngine(_pref, (int)TableType.PAYMENTTRANSACTION, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            //Valeurs récupérées pour la table PAYMENTTRANSACTION
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANREFPRESTA.GetHashCode(), ingenicoResult.HostedCheckoutId);
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANAMOUNT.GetHashCode(), amountStr);
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANREFEUDO.GetHashCode(), ingenicoResult.MerchantReference);
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANTARGETDESCID.GetHashCode(), _eFormFile.Tab.ToString());
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANTARGETFILEID.GetHashCode(), fileid.ToString());
            if (string.IsNullOrEmpty(ingenicoResult.ErrorId))
            {
                engTransaction.AddNewValue(PaymentTransactionField.PAYTRANEUDOSTATUS.GetHashCode(), ((int)PayTranEudoStatusEnum.PAYMENT_IN_PROGRESS).ToString());
                engTransaction.AddNewValue(PaymentTransactionField.PAYTRANSTATUS.GetHashCode(), ((int)StatusHostedCheckout.PAYMENT_CREATED).ToString());
                engTransaction.AddNewValue(PaymentTransactionField.PAYTRANCATEGORY.GetHashCode(), ((int)PaymentSubStatus.CREATED).ToString());
            }

            else
            {
                engTransaction.AddNewValue(PaymentTransactionField.PAYTRANEUDOSTATUS.GetHashCode(), ((int)PayTranEudoStatusEnum.IN_ERROR).ToString());
                engTransaction.AddNewValue(PaymentTransactionField.PAYTRANSTATUS.GetHashCode(), ((int)StatusHostedCheckout.UNKNOWN).ToString());
            }

            engTransaction.AddNewValue((int)PaymentTransactionField.PAYTRANFORMULARINFOS, sFormularInfos);
            engTransaction.AddNewValue(PaymentTransactionField.PAYTRANDATEPAYMENT.GetHashCode(), DateTime.Now.ToString());

            engTransaction.EngineProcess(new StrategyCruTransaction());

            EngineResult engResult = engTransaction.Result;

            if (engResult == null)
                throw new EudoException(engResult.Error.Msg, engResult.Error.DebugMsg);

            if (!engResult.Success)
            {
                if (engResult.Error != null)
                {
                    EudoException ex = new EudoException(engResult.Error.Msg, engResult.Error.DebugMsg);
                    ex.UserMessageTitle = engResult.Error.Title;
                    ex.UserMessageDetails = engResult.Error.Detail;
                    throw ex;

                }
                else
                    throw new EudoException(engResult.Error.Msg, engResult.Error.DebugMsg);
            }
            #region add target table

            Engine.Engine engTargetTable = eModelTools.GetEngine(_pref, _eFormFile.Tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));

            engTargetTable.FileId = fileid;

            var def = new { edndPa = 0, edndTr = 0, edndPi = 0 };
            var t = JsonConvert.DeserializeAnonymousType(_requestTools.GetRequestFormKeyS("onlinePaymentParams"), def);
            if (t.edndTr != 0)
            {
                engTargetTable.AddNewValue(t.edndTr, ingenicoResult.MerchantReference);

            }

            engTargetTable.EngineProcess(new StrategyCruTransaction());

            EngineResult engResultTargetTable = engTargetTable.Result;

            if (engResultTargetTable == null)
                throw new EudoException(engResultTargetTable.Error.Msg, engResultTargetTable.Error.DebugMsg);

            if (!engResultTargetTable.Success)
            {
                if (engResultTargetTable.Error != null)
                {
                    EudoException ex = new EudoException(engResultTargetTable.Error.Msg, engResultTargetTable.Error.DebugMsg);
                    ex.UserMessageTitle = engResultTargetTable.Error.Title;
                    ex.UserMessageDetails = engResultTargetTable.Error.Detail;
                    throw ex;

                }
                else
                    throw new EudoException(engResultTargetTable.Error.Msg, engResultTargetTable.Error.DebugMsg);
            }
            #endregion add target table

            #endregion eng.AddNewValue 
        }

        /// <summary>
        /// get the amount of transaction
        /// </summary>
        /// <param name="formularId">id of formulaire</param>
        /// <param name="evtFileId"> id of event</param>
        /// <param name="tplFileId">id of template</param>
        /// <returns></returns>
        public decimal GetAmount(int formularId, int evtFileId, int tplFileId)
        {
            decimal value = 0;

            //on recharge le formulaire pour récupérer les données à jour
            var _FormFile = new eFormular(formularId, _pref, evtFileId, tplFileId);
            _FormFile.AnalyseAndSerializeFields();
            _FormFile.Init();
            if (_FormFile.Generate())
            {
                var def = new { edndPa = 0, edndTr = 0, edndPi = 0 };
                var t = JsonConvert.DeserializeAnonymousType(_requestTools.GetRequestFormKeyS("onlinePaymentParams"), def);
                if (t.edndPa != 0)
                {
                    var _fieldRecord = _FormFile.GetRecordFieldFromDesc(t.edndPa);
                    if (_fieldRecord != null)
                        value = eLibTools.GetDecimal(_fieldRecord.Value);
                }
            }

            return value;
        }


        /// <summary>
        /// Déplace les fichiers dans le dossier tmp dans le dossier files 
        /// </summary>
        /// <param name="_pref"></param>
        /// <param name="newFields"></param>
        /// <param name="requestTools"></param>
        private void MoveImageFilesFromTmpFolder(ePref _pref, Dictionary<int, string> newFields, eRequestTools requestTools)
        {
            string error;

            int? fileId = requestTools.GetRequestFormKeyI("fileId");
            int? tab = requestTools.GetRequestFormKeyI("tab");

            List<Field> fields = eDataTools.GetFieldsFromlistOfDescId(_pref, tab.HasValue ? tab.Value : 0, newFields.Keys.ToList());

            foreach (Field fld in fields)
            {
                if (fld.Format == FieldFormat.TYP_IMAGE && fld.ImgStorage == ImageStorage.STORE_IN_FILE)
                {
                    TryMoveImageFileFromTMP(fld, newFields[fld.Descid], tab.HasValue ? tab.Value : 0, fileId.HasValue ? fileId.Value : 0, out error);

                    if (error.Length > 0)
                        throw new Exception(error);
                }
            }
        }

        /// <summary>
        /// On catch les exceptions sur les fichier
        /// </summary>
        /// <param name="fld"></param>
        /// <param name="newName"></param>
        /// <param name="tab"></param>
        /// <param name="fileId"></param>
        private void TryMoveImageFileFromTMP(Field fld, string newName, int tab, int fileId, out string error)
        {
            error = string.Empty;

            string tmpPath = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref) + "\\" + TEMPORARY_FOLDER_NAME;
            string tmpName = HashMD5.GetHash(tab + "" + fld.Descid + "" + fileId) + "." + GetImageExtenstion(newName);

            try
            {
                MoveImageFileFromTMP(tmpPath, tmpName, newName);
            }
            catch (UnauthorizedAccessException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (ArgumentNullException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (PathTooLongException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (DirectoryNotFoundException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (ArgumentException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (HttpException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (IOException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (Exception ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
        }

        /// <summary>
        /// On déplace le fichier image de repertoire tmp au repertoire file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        private void MoveImageFileFromTMP(string tmpPath, string tmpName, string newName)
        {
            string sourceFile = tmpPath + "\\" + tmpName;
            string targetFile = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref) + "\\" + newName;

            if (File.Exists(sourceFile))
            {
                //On supprime l'ancien
                if (File.Exists(targetFile))
                    File.Delete(targetFile);

                //on déplace le nouveau avec changement de nom
                File.Move(sourceFile, targetFile);
            }
        }

        /// <summary>
        /// Renvoie l'url du fichier qui vient d'etre uploadé
        /// </summary>
        /// <param name="fileUrl"></param>
        private void RenderXmlResponse(List<string> fileSrcList)
        {
            //Sauvegarde la date de soumission

            // Init le document XML
            XmlDocument xmlResult = new XmlDocument();

            // Init le document XML
            XmlNode mainNode = xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
            xmlResult.AppendChild(mainNode);

            XmlNode rootNode = xmlResult.CreateElement("root");
            xmlResult.AppendChild(rootNode);

            XmlNode successNode = xmlResult.CreateElement("success");
            rootNode.AppendChild(successNode);

            XmlNode modeNode = xmlResult.CreateElement("mode");
            rootNode.AppendChild(modeNode);

            successNode.InnerText = "1";
            modeNode.InnerText = "FILESRC";

            if (fileSrcList.Count > 0)
            {
                XmlNode urlNode = xmlResult.CreateElement("url");
                rootNode.AppendChild(urlNode);
                urlNode.InnerText = fileSrcList[0];
            }

            RenderResult(RequestContentType.XML, delegate () { return xmlResult.OuterXml; });
        }

        /// <summary>
        /// Gére l'upload des fichier
        /// </summary>
        /// <param name="requestTools"></param>
        private List<string> GetUploadedFilesSrcUrl(eRequestTools requestTools)
        {

            List<string> listOfPath = new List<string>();
            string error;
            try
            {
                if (_context.Request.Files.Count > 0)
                {
                    HttpFileCollection files = _context.Request.Files;

                    int? fileId = requestTools.GetRequestFormKeyI("fileId");
                    int? tab = requestTools.GetRequestFormKeyI("tab");
                    int? descid = requestTools.GetRequestFormKeyI("descid");
                    string fullPathName;
                    HttpPostedFile file;
                    for (var i = 0; i < files.Count; i++)
                    {
                        file = files[i];

                        if (!IsValidImageExstention(file.FileName))
                            return listOfPath;

                        fullPathName = TrySaveTmpFile(file, fileId.HasValue ? fileId.Value : 0, tab.HasValue ? tab.Value : 0, descid.HasValue ? descid.Value : 0, out error);

                        //est-ce qu'on fait un feedback et on continue ou on s'arrete sec ? je ne sais pas ...allo les specs 
                        if (error.Length > 0)
                            throw new Exception(error);
                        //eFeedbackXrm.LaunchFeedbackXrm(eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, error), Pref);

                        listOfPath.Add(fullPathName);
                    }
                }
            }
            catch (Exception ex)
            {
                //Gestion d'erreur
                this.ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   string.Concat("<br>", eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  //  Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   string.Concat("Sauvegarde du fichier impossible : ", ex.Message, Environment.NewLine, ex.StackTrace));
                LaunchError();
            }

            return listOfPath;
        }

        /// <summary>
        /// Vérifie l'extension du fichier de type image
        /// </summary>
        /// <param name="file"></param>       
        private Boolean IsValidImageExstention(string fileName)
        {


            string fileExtension = GetImageExtenstion(fileName);

            if (string.IsNullOrEmpty(fileExtension) || !acceptedImageFormats.Contains(fileExtension))
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, "Image invalide", "Le format n'est pas de fichier n'est pas correct.", "Format image");
                LaunchError();

                return false;
            }

            return true;
        }

        /// <summary>
        /// Retourne l'extension de nom de fichier
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetImageExtenstion(string fileName)
        {
            string[] parts = fileName.Split('.');
            if (parts.Length < 2)
                return string.Empty;

            //le dernier bout
            return parts[parts.Length - 1];
        }


        /// <summary>   
        /// Sauvegarde un fichier temporaire en gérant les exceptions
        /// </summary>
        /// <param name="file">fichier a sauvegarder</param>
        /// <param name="fileId">id de la fiche</param>
        /// <param name="tab">table de de la fiche</param>
        /// <param name="error">erreur retourné lors de la tentative de sauvegarde</param>
        /// <returns></returns>
        private string TrySaveTmpFile(HttpPostedFile file, int fileId, int tab, int descid, out string error)
        {
            error = string.Empty;
            try
            {
                return saveTmpFile(file, fileId, tab, descid);
            }
            catch (UnauthorizedAccessException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (ArgumentNullException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (PathTooLongException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (DirectoryNotFoundException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (ArgumentException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (HttpException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }
            catch (IOException ex) { error = ex.Message + Environment.NewLine + ex.StackTrace; }

            return string.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
        }

        /// <summary>
        /// Sauvegarde les fichiers téléchargé dans un dossier tmp   /datas/base/files/tmp
        /// Nom du fichier : fileid_filename_fileid_tab_date
        /// Tous les fichier de plus de 2 jours seront supprimés
        /// </summary>
        /// <param name="file">fichier a sauvegarder</param>
        /// <param name="fileId">id de la fiche</param>
        /// <param name="tab">table de de la fiche</param>
        /// <returns></returns>
        private string saveTmpFile(HttpPostedFile file, int fileId, int tab, int descid)
        {
            string path = eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref) + "\\" + TEMPORARY_FOLDER_NAME;

            //On crée un dossier caché dans Files
            if (!Directory.Exists(path))
            {
                DirectoryInfo di = Directory.CreateDirectory(path);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }
            else
            {
                //On nétoie au bout de 2 jours au cas ou y a des résidus
                cleanDir(path, 2);
            }

            //On supprime si ca existe
            string fileName = HashMD5.GetHash(tab + "" + descid + "" + fileId) + "." + GetImageExtenstion(file.FileName);
            string fullName = path + "\\" + fileName;
            if (File.Exists(fullName))
                File.Delete(fullName);

            file.SaveAs(fullName);

            //On retourne le chemin virtuel
            return eLibTools.GetWebDatasPath(eLibConst.FOLDER_TYPE.FILES, _pref.GetBaseName) + "/" + TEMPORARY_FOLDER_NAME + "/" + fileName + "?date=" + DateTime.Now.ToString("yyMMddhhmmss");
        }

        /// <summary>
        /// On supprime les fichiers temp
        /// </summary>
        /// <param name="path"></param>
        private void cleanDir(string path, int dayAge)
        {
            //Pour la protection
            if (!path.EndsWith(TEMPORARY_FOLDER_NAME))
                return;

            string[] names = Directory.GetFiles(path);
            string name;
            for (int i = 0; i < names.Length; i++)
            {
                //Date de derniere ecriture
                name = names[i];
                if ((DateTime.Now - File.GetLastWriteTime(name)).Days >= dayAge)
                    File.Delete(name);
            }
        }

        /// <summary>
        /// Valide les informations nécessaire pour ouvrir une pseudo session externe et initialise le token
        ///   -> Token valide et cohérent
        ///   -> Information table/fileid cochérent
        ///   -> Création du FormularLite _eFrmLite
        /// </summary>
        /// <returns></returns>
        protected override bool ValidateExternalLoad()
        {
            //Chargement du token
            _dataParam = new LoadQueryStringForm(_pageQueryString.UID, _pageQueryString.Cs, _pageQueryString.P);

            if (_dataParam.InvalidQueryString())
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 8665), String.Concat(eResApp.GetRes(_pref, 6524), " (QueryString)"));
                LaunchError();
                return false;
            }

            //Token de bdd incohérent
            if (_pageQueryString.UID != _dataParam.CsData.UID)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 8665), eResApp.GetRes(_pref, 6241));
                LaunchError();
                return false;

            }

            //Cohérence Paramètres table/fileid
            int? nFileId = _requestTools.GetRequestFormKeyI("fileId");
            if (nFileId == null || nFileId != _dataParam.ParamData.TplFileId)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 8665), String.Concat(eResApp.GetRes(_pref, 6524), " (fileId)"));
                LaunchError();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Charge le formulaire posté et valide l'expiration et la soumission unique
        ///   -> eventuellement, validation avec le formulaire en session ?
        /// </summary>
        /// <returns></returns>
        private bool ValidatePost()
        {
            //message
            rend = eRendererFactory.CreateFormularFileRenderer(
               _dataParam.CsData.UID,
               eLibTools.GetAppUrl(_context.Request),
               _pref,
               _dataParam.CsData.FormularId,
               _dataParam.ParamData.ParentFileId,
               _dataParam.ParamData.TplFileId);


            if (rend.ErrorMsg.Length > 0)
            {
                //Gestion d'erreur
                this.ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.EXCLAMATION, "MSG", "DETAILMSG", "TITLEMSG", string.Format("ValidatePost => {0}", rend.ErrorMsg));
                LaunchError();
                return false;
            }

            //Chargement du eFormularFile
            _eFormFile = rend.FormFile;

            //Soumission Unique
            if (_eFormFile.AlreadySubmitted && _eFormFile.IsUniqueSubmission)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6768), "", eResApp.GetRes(_pref, 6768));
                LaunchError();
                return false;
            }

            //Soumission a date
            if (_eFormFile.IsExpired)
            {
                this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6767), "", eResApp.GetRes(_pref, 6767));
                LaunchError();
                return false;
            }

            //Captcha
            if (_eFormFile.TplFileId <= 0)
            {
                //TODO: Ajouter la partie captcha pour le formulaire avancé
                if (_eFormFile.FormularType == FORMULAR_TYPE.TYP_ADVANCED)
                {
                    if (string.IsNullOrEmpty(_requestTools.GetRequestFormKeyS("reCaptcha")) || !eTools.IsReCaptchValid(_requestTools.GetRequestFormKeyS("reCaptcha").Trim()))
                    {
                        this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 2658), "", eResApp.GetRes(_pref, 2658));
                        LaunchError();
                        return false;
                    }
                }
                else if (!_requestTools.GetRequestFormKeyS("Captcha").Trim().Equals(_context.Session["Captcha"].ToString()))
                {
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 6223), "", eResApp.GetRes(_pref, 6223));
                    LaunchError();
                    return false;
                }


            }
            else //check sur l'indicateur du paiement indicator si le formulaire n'est pas publique
                if (!string.IsNullOrEmpty(_requestTools.GetRequestFormKeyS("isOnlinePaymentBtn")) && _requestTools.GetRequestFormKeyS("isOnlinePaymentBtn").Trim().Equals("1"))
            {
                var def = new { edndPa = 0, edndTr = 0, edndPi = 0 };
                var t = JsonConvert.DeserializeAnonymousType(_requestTools.GetRequestFormKeyS("onlinePaymentParams"), def);
                if (t.edndPi != 0 && _eFormFile.CheckIfPaymentIsAlreadyDone(t.edndPi))
                {
                    this.ErrorContainer = eErrorContainer.GetUserError(eLibConst.MSG_TYPE.EXCLAMATION, eResApp.GetRes(_pref, 3085), System.Environment.NewLine, eResApp.GetRes(_pref, 3084));
                    LaunchError();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// valider le montant de payment 
        /// </summary>
        /// <returns></returns>
        private bool validateAmountForOnlinePayment(int formularId, int evtFileId, int tplFileId)
        {
            //get the amount of transaction
            decimal value = GetAmount(formularId, evtFileId, tplFileId);
            if (value <= 0)//si le montant est ngatif, on valide pas
                return false;
            else
                return true;
        }

        internal static void MoveImageFileFromTmpFolder(ePref pref, List<int> LstFld)
        {
            throw new NotImplementedException();
        }
    }
}