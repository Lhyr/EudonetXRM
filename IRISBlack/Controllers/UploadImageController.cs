using System.Web.Http;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Xrm.IRISBlack.Model;
using Com.Eudonet.Xrm.IRISBlack.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Com.Eudonet.Core.Model;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using Syncfusion.Compression.Zip;
using Com.Eudonet.Common.Enumerations;

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// controller qui permet le téléversement des images
    /// </summary>
    public class UploadImageController : BaseController
    {

        /// <summary>
        /// Permet de vérifier si une ou plusieurs images ont déjà été envoyés
        /// </summary>
        /// <param name="nAction"></param>
        /// <param name="sFiles"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Get(int nAction, string sFiles)
        {
            eEnumMgrAction action = (eEnumMgrAction)nAction;

            List<PJUploadInfo> files = null;
            try
            {
                files = JsonConvert.DeserializeObject<List<PJUploadInfo>>(sFiles);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            switch (action)
            {
                case eEnumMgrAction.Check:

                    #region Validation de l'existence d'une image à charger

                    bool bImageExists = true; // indique s'il y a une image existante à charger via JavaScript

                    eImageField image = null;

                    // TODO paramètres à cabler
                    eLibConst.IMAGE_TYPE _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;
                    int _imageTab = 200;
                    int _imageFieldDescId = 275;
                    int _imageFieldFileId = 1;

                    IList<CheckFileModel> lstResFiles = new List<CheckFileModel>();

                    if (_imageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD)
                    {
                        image = eImageField.GetImageField(_pref, _imageTab, _imageFieldDescId, _imageFieldFileId);

                        #region Récupération de l'image existante si FileId > 0
                        if (_imageFieldFileId > 0)
                        {

                            if (image.StorageType == ImageStorage.STORE_IN_URL)
                            {
                                // Renvoi de l'URL à l'appelant
                                CheckFileModel cfm = new CheckFileModel();
                                cfm.RealName = image.DbValue.ToString();
                                lstResFiles.Add(cfm);
                            }

                            try
                            {
                                bImageExists = image.ImageExists(image.DbValue);
                            }
                            catch
                            {
                                bImageExists = false;
                            }
                        }
                        #endregion
                        #region Si FileId < 0, on ne fait rien (en E17, on renvoyait l'image stockée en session)
                        else
                        {
                        }
                        #endregion
                    }

                    #endregion

                    return Ok(JsonConvert.SerializeObject(
                        new CheckUploadFilesModel
                        {
                            Success = bImageExists,
                            CheckFile = lstResFiles,
                        }));

                case eEnumMgrAction.Confirmation:

                    break;
            }

            return Ok();
        }

        // POST api/<controller>
        /// <summary>
        /// Ici on ajoute les pièces jointes en provenance du client.
        /// </summary>
        /// <param name="uploadImageModel"></param>
        /// <returns></returns>
        public IHttpActionResult Post(UploadImageModel uploadImageModel)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            List<IUploadImageReturnModel> uploadImageReturnModelList = new List<IUploadImageReturnModel>();
            /* TODO à décider fonctionnellement -
             * Lorsqu'on crée une fiche, doit-on sauvegarder l'image sur le serveur, ou la stocker en session/la renvoyer à l'appelant ?
             * E17 décidait en fonction du contexte. Sur IRIS, on se base sur ce que nous transmet le Front pour l'instant */
            /*
            if (uploadImageModel.FileId <= 0 || (uploadImageModel.ParentIsPopup && uploadImageModel.UpdateOnBlur))
                uploadImageModel.SaveInSession = true;
            */

            ePJToAdd _myPj = new ePJToAdd
            {
                SaveAs = (!_pref.IsFullUnicode && eLibTools.ContainsNonUtf8(uploadImageModel.SaveAs))
                    ? eLibTools.RemoveDiacritics(uploadImageModel.SaveAs)
                    : HttpUtility.UrlDecode(uploadImageModel.SaveAs),
                FileId = uploadImageModel.FileId,
                Tab = uploadImageModel.Tab,
                ParentEvtFileId = uploadImageModel.ParentEvtFileId, //Cas d'ajout de pj depuis l'assiatnt d'emailing en signet ou modif
                ParentEvtTab = uploadImageModel.ParentEvtTab,
                PPID = uploadImageModel.PPID,
                PMID = uploadImageModel.PMID,
                ADRID = uploadImageModel.ADRID,
                MailForwarded = uploadImageModel.MailForwarded,
                TypePj = uploadImageModel.PJType,
                UploadLink = uploadImageModel.UploadLink,
            };

            bool _bFromTpl = uploadImageModel.FromTpl;
            List<string> liErrors = new List<string>();

            PJUploadInfoModel[] uploadInfo;

            try
            {
                uploadInfo = uploadImageModel.UploadInfo
                    .Select(up => JsonConvert.DeserializeObject<PJUploadInfoModel>(up))
                    .ToArray();

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            if ((_myPj.TypePj != (int)PjType.FILE && _myPj.TypePj != (int)PjType.WEB && _myPj.TypePj != (int)PjType.FTP))
            {
                Exception ex = new Exception(String.Concat("Unsupported PJType: ", _myPj.TypePj));
                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                //return InternalServerError(ex);
            }
            else if (uploadInfo == null)
            {
                Exception ex = new Exception("No files or URLs to process");
                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                //return InternalServerError(ex);
            }

            string sError = "";

            // Chargement et suppression de l'image existante
            eAbstractImage image = eAbstractImage.GetImage(_pref, uploadImageModel.ImageType, uploadImageModel.Tab, uploadImageModel.FileId, uploadImageModel.DescId, uploadImageModel.FromTpl ? "" : "");
            if (image == null)
            {
                Exception ex = new Exception(String.Concat("Null image - Tab: ", uploadImageModel.Tab, ", FileId: ", uploadImageModel.FileId, ", DescId: ", uploadImageModel.DescId));
                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                //return InternalServerError(ex);
            }
            image.CreateInDatabase = false; // #90 392 - Si le FileID est à 0, on ne doit pas appeler SaveInDatabase() sur la saisie guidée (car la fiche ne doit pas être créée à l'ajout d'image mais à la fin du scénario)
            image.CheckIntegrityOnUpload = true; //# 90 392 - Même chose, sur la saisie guidée, on vérifie l'intégrité des images envoyées (TK #5533)
            image.Delete(); //ALISTER => Demande 67 887

            // Si on a des fichiers à traiter...
            if (uploadImageModel.fileCollection != null)
            {
                for (int i = 0; i < uploadImageModel.fileCollection.Length; i++)
                {
                    HttpPostedFileBase myFile = uploadImageModel.fileCollection[i];

                    UploadImageReturnModel uploadImageReturnModel = new UploadImageReturnModel();

                    int nFileLen = myFile.ContentLength;
                    int iPjId = 0;
                    UploadFileFactory upFile = UploadFileFactory.initUploadFileFactory(myFile);

                    if (!(upFile.CheckFileToUpload(out sError)))
                        continue;

                    iPjId = 0;

                    // #57 013 - Le tableau dicRename étant alimenté à partir de noms de fichiers stockés dans une variable ayant subi un UrlDecode(),
                    // il faut donc effectuer le même traitement sur le FileName de chaque fichier pour retrouver, dans le tableau des noms de fichier,
                    // les fichiers dont le nom a subi une transformation après passage via UrlDecode
                    // Cas des fichiers comportant, par ex., le signe + dans le nom
                    _myPj.PjUploadInfo = uploadInfo
                        .Select(fi => new PJUploadInfo(fi.FileName, fi.SaveAs, fi.Action)
                        {
                            PjId = fi.PjId,
                            ReplaceOptDisplayed = fi.ReplaceOptDisplayed,
                        })
                        .FirstOrDefault(f => f.FileName == HttpUtility.UrlDecode(Path.GetFileName(myFile.FileName)));

                    _myPj.SaveAs = _myPj.PjUploadInfo?.SaveAs;

                    try
                    {
                        #region Processus principal

                        /*
                        byte[] byFielStrm = new byte[myFile.InputStream.Length];
                        myFile.InputStream.Read(byFielStrm, 0, byFielStrm.Length);

                        //Sauvegarde du fichier
                        bool retourARddPj = _myPj.Save(_pref, upFile.ConstructHttpPostedFile(byFielStrm, myFile.FileName, myFile.ContentType), string.Empty, out iPjId);
                        */

                        byte[] data = null;

                        // Récupération de l'image uploadée...
                        if ((image.StorageType != ImageStorage.STORE_IN_URL))
                            data = image.GetPostedFileData(myFile);

                        if ((data != null && data.Length > 0) || image.StorageType == ImageStorage.STORE_IN_URL)
                        {
                            if (uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.MEMO || uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL || uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.TXT_URL)
                            {
                                //eMemoImage memoImg = (eMemoImage)image;
                                Boolean uploadOk = image.Save(data);
                                uploadImageReturnModel.ImageURL = image.ImageURL;

                                if (!uploadOk)
                                    throw new EudoException(eResApp.GetRes(_pref, 6237), image.UserError, new Exception(image.DebugError));
                            }
                            else if (uploadImageModel.SaveInSession && uploadImageModel.ImageType != eLibConst.IMAGE_TYPE.LOGO)
                            {
                                #region Cas création mode popup

                                // On stocke le fichier en session
                                if (image.StorageType != ImageStorage.STORE_IN_URL)
                                {
                                    image.FileName = myFile.FileName;
                                    // On appelle la méthode "RenameFile()" pour récupérer le nom du fichier final
                                    //image.RenameFile();

                                    //KHA le 20/02/2019 IE et Edge renvoient le chemin local de l'image, on ne veut que le nom du fichier
                                    if (image.FileName.Contains(@"\"))
                                    {
                                        string[] sFileName = image.FileName.Split('\\');
                                        image.FileName = sFileName[sFileName.Length - 1];
                                    }

                                    // Sauvegarde en session : on enregistre l'image analysée
                                    uploadImageReturnModel.SessionImageDataBytes = data;
                                    uploadImageReturnModel.SessionImageFileName = image.FileName;
                                    uploadImageReturnModel.SessionImageContentType = myFile.ContentType;

                                    if (image is eFieldImage)
                                    {
                                        if (uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD)
                                        {
                                            uploadImageModel.ImageWidth = 45;
                                            uploadImageModel.ImageHeight = 45;
                                        }
                                        uploadImageReturnModel.ImageURL = ((eFieldImage)image).GetImageURL(uploadImageModel.ComputeRealThumbnail, uploadImageModel.ImageWidth, uploadImageModel.ImageHeight, true);
                                    }

                                }
                                else
                                {
                                    // Sauvegarde en session de l'URL si image de type URL : on utilise le tableau UploadInfo passé par le Front qui contient,
                                    // en FileName, l'URL fournie
                                    uploadImageReturnModel.SessionImageUri = new Uri(uploadInfo[i].FileName);
                                    uploadImageReturnModel.SessionImageFileName = Path.GetFileName(uploadInfo[i].FileName);
                                    uploadImageReturnModel.SessionImageContentType = String.Concat("image/", Path.GetExtension(uploadInfo[i].FileName).Replace(".", String.Empty)).Replace("jpg", "jpeg");
                                }

                                uploadImageReturnModel.SessionImageSent = true;

                                #endregion

                            }
                            else
                            {
                                #region Cas de la mise à jour standard

                                Boolean uploadOk = false;

                                if (uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD)
                                {

                                    eImageField fieldImage = (eImageField)image;


                                    if (image.StorageType != ImageStorage.STORE_IN_URL)
                                    {
                                        uploadOk = fieldImage.Save(data);
                                        if (image.StorageType == ImageStorage.STORE_IN_DATABASE)
                                        {
                                            uploadImageReturnModel.ImageURL = fieldImage.GetImageURL(uploadImageModel.ComputeRealThumbnail, uploadImageModel.ImageWidth, uploadImageModel.ImageHeight);
                                        }
                                        else
                                        {
                                            uploadImageReturnModel.ImageURL = fieldImage.ImageURL;
                                        }
                                    }
                                    else
                                    {
                                        uploadOk = fieldImage.StoreInURL(uploadImageReturnModel.ImageURL);
                                    }
                                }
                                else
                                {
                                    uploadOk = image.Save(data);

                                    uploadImageReturnModel.ImageURL = image.ImageURL;

                                }


                                if (!uploadOk)
                                    throw new EudoException(eResApp.GetRes(_pref, 6237), image.UserError, new Exception(image.DebugError));


                                #endregion
                            }
                        }

                        if (
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD ||
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.USER_AVATAR_FIELD ||
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.MEMO ||
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL ||
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.TXT_URL ||
                            uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD)
                        {
                            //BSE:#59 743
                            if (uploadImageModel.ImageType != eLibConst.IMAGE_TYPE.MEMO)
                            {
                                // US #1904 - Tâche #2753 - On échappe les apostrophes uniquement si ça n'a pas déjà été fait par un autre traitement en amont
                                if (uploadImageReturnModel.ImageURL.Contains("'") && !uploadImageReturnModel.ImageURL.Contains(@"\'"))
                                    uploadImageReturnModel.ImageURL = uploadImageReturnModel.ImageURL.Replace("'", @"\'");
                            }
                            else
                                uploadImageReturnModel.ImageURL = string.IsNullOrEmpty(image.ImageWebURL) ? uploadImageReturnModel.ImageURL : image.ImageWebURL;
                        }
                        #endregion
                    }
                    catch (EudoException ex)
                    {
                        //return InternalServerError(ex);
                        return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                    }
                    catch (Exception ex)
                    {
                        return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                        //return InternalServerError(ex);
                    }
                    finally
                    {
                        if (iPjId > 0 || !String.IsNullOrEmpty(uploadImageReturnModel.ImageURL))
                            uploadImageReturnModelList.Add(uploadImageReturnModel);
                    }
                }
            }
            // Ou sinon, des URLs
            else
            {
                for (int i = 0; i < uploadInfo.Length; i++)
                {
                    PJUploadInfoModel uploadInfoModel = uploadInfo[i];

                    UploadImageReturnModel uploadImageReturnModel = new UploadImageReturnModel();

                    // IMAGE_TYPE = TXT_URL : téléchargement du fichier depuis l'URL - cf. eImageDialog.aspx.cs, bUploadMode = true
                    if (uploadImageModel.ImageType == eLibConst.IMAGE_TYPE.TXT_URL)
                    {
                        // TODO - Ajout d'un fichier à partir d'une URL - Téléchargement puis conversion en URL
                        return InternalServerError(new NotImplementedException());
                    }
                    else
                    {
                        uploadImageReturnModel.ImageURL = uploadInfoModel.FileName; // le Front envoie l'URL dans la propriété FileName (cf. EntryFieldsImage;js)
                        uploadImageReturnModel.SessionImageSent = false;
                        uploadImageReturnModelList.Add(uploadImageReturnModel);
                    }
                }
            }

            if (uploadImageReturnModelList.Count < 1)
            {
                EudoException ex = new EudoException(sMessage: eLibTools.Join(Environment.NewLine, liErrors)
                    , sUserMessage: eResApp.GetRes(_pref, 6724));

                return Ok(ExceptionFactory.InitExceptionFactory(eLibTools.IsLocalOrEudoMachine()).GetExceptionModel(ex));
                //return InternalServerError(ex);
            }


            return Ok(uploadImageReturnModelList);
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        // DELETE api/<controller>/5
        /// <summary>
        /// Méthode permettant de supprimer les images laissées orphelines sur le serveur, à la suite d'une saisie guidée non validée - NON IMPLEMENTEE
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }

        /// <summary>
        /// Méthode permettant de supprimer les images laissées orphelines sur le serveur, à la suite d'une saisie guidée non validée
        /// </summary>
        /// <param name="uploadImageDeleteModel">Paramètres à fournir pour la suppression</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public IHttpActionResult Delete(UploadImageDeleteModel uploadImageDeleteModel)
        {
            if (!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            List<IUploadImageReturnModel> uploadImageReturnModelList = new List<IUploadImageReturnModel>();

            foreach (UploadImageDeleteModel.ImageField imageField in uploadImageDeleteModel.ImageFields)
            {
                // Construction du chemin de l'image pour vérifications
                eLibConst.FOLDER_TYPE datasFolderType = eLibTools.GetFolderTypeFromImageType(imageField.ImageType);
                string filePath = String.Concat(eModelTools.GetPhysicalDatasPath(datasFolderType, _pref), @"\");
                string fullPath = Path.Combine(filePath, imageField.FileName);

                bool bFileExistedBeforeDeletion = File.Exists(fullPath);

                // Si l'image existe, on la supprime après contrôles
                UploadImageReturnModel uploadImageReturnModel = new UploadImageReturnModel();
                if (bFileExistedBeforeDeletion)
                {
                    // On utilise GetImage() pour vérifier que le contexte passé au contrôleur (FileId, DescId, TabId) soit cohérent, et empêcher des suppressions
                    // d'images non désirées en détournant l'usage du contrôleur. L'image à supprimer n'étant pas rattachée à un FileID (d'où la nécessité de la
                    // supprimer), on affectera ensuite le FileName visé à l'objet récupéré si le contexte est correct, pour que la méthode .DeleteFile() fonctionne
                    eAbstractImage imageContextInDb = eAbstractImage.GetImage(
                        _pref, imageField.ImageType, uploadImageDeleteModel.TabDescId, uploadImageDeleteModel.FileId, imageField.DescId);

                    // Si aucune info ne peut être rapatriée concernant le champ, on sort
                    if (imageContextInDb == null)
                        return Ok(uploadImageReturnModelList);

                    imageContextInDb.SetFileInfos(imageField.FileName, 0, String.Empty);
                    imageContextInDb.CreateInDatabase = false; // #90 392 - Si le FileID est à 0, on ne doit pas appeler SaveInDatabase() sur la saisie guidée (car la fiche ne doit pas être créée à la suppression des images temporaires)

                    // Puis on procède à la suppression
                    bool? bDeleted = imageContextInDb.Delete();

                    // Pour vérifier que l'image ait été supprimée, on ne se fie pas au retour de .Delete() qui renvoie le retour de .DeleteInDatabase(),
                    // qui renvoie forcément false si FileID = 0. A la place, on vérifie si le fichier est toujours présent sur le disque après opération
                    if (uploadImageDeleteModel.FileId == 0 && (bDeleted == false || bDeleted == null))
                        bDeleted = !File.Exists(Path.Combine(filePath, imageField.FileName));

                    // En réponse, on retourne la liste des fichiers dont la suppression s'est déroulée avec succès
                    if (bDeleted == true && bFileExistedBeforeDeletion)
                        uploadImageReturnModel.ImageURL = imageField.FileName;
                }
                uploadImageReturnModelList.Add(uploadImageReturnModel);

                // Rubriques de type images : suppression des fichiers _thumb et _mobile (miniatures)
                if ((DatasFolderType)datasFolderType == DatasFolderType.FILES)
                {
                    string extension = Path.GetExtension(fullPath);

                    foreach (string suffix in new string[] { eLibConst.THUMB_SUFFIX, eLibConst.MOBILE_SUFFIX })
                    {
                        string fileThumb = string.Concat(fullPath.Substring(0, fullPath.Length - extension.Length), suffix, extension);
                        try
                        {
                            if (File.Exists(fileThumb))
                            {
                                File.Delete(fileThumb);
                                // Pour chaque fichier miniature supprimé, on renvoie son nom en retour
                                if (!File.Exists(fileThumb))
                                {
                                    UploadImageReturnModel uploadImageThumbReturnModel = new UploadImageReturnModel();
                                    uploadImageThumbReturnModel.ImageURL = fileThumb;
                                    uploadImageReturnModelList.Add(uploadImageThumbReturnModel);
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            return Ok(uploadImageReturnModelList);
        }
    }
}