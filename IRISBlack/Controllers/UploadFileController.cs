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

namespace Com.Eudonet.Xrm.IRISBlack.Controllers
{
    /// <summary>
    /// controller qui permet le téléversement des fichiers
    /// </summary>
    public class UploadFileController : BaseController
    {

        /// <summary>
        /// Permet de checker si un ou plusieurs fichiers ont déjà
        /// été uploadés.
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

                    if (files.Count == 0)
                        return InternalServerError(new EudoException(eResApp.GetRes(_pref, 72)));

                    Dictionary<string, string> dicRename = files.ToDictionary(key => key.FileName,
                        val => ePJTraitementsLite.EscapeSpecialCharactersInFilename(val.SaveAs.Length > 0 ? val.SaveAs : val.FileName));

                    CheckPJExistsFactory pjFact = CheckPJExistsFactory.initCheckPJExistsFactory(_pref,
                        eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.ANNEXES, _pref.GetBaseName));

                    IList<CheckFileModel> lstResFiles = dicRename
                        .Select(kvp => pjFact.checkIfFileExists(kvp.Key, kvp.Value))
                        .ToList();

                    return Ok(JsonConvert.SerializeObject(
                        new CheckUploadFilesModel
                        {
                            Success = pjFact.IsAllSuccess,
                            CheckFile = lstResFiles,
                        }));

                case eEnumMgrAction.Confirmation:
                    //ePJCheckerRenderer rdr = ePJCheckerRenderer.CreatePJCheckerRenderer(_pref, tab, fid, windowDescription, files);
                    //rdr.Generate();
                    break;
            }

            return Ok();
        }

        // POST api/<controller>
        /// <summary>
        /// Ici on ajoute les pièces jointes en provenance du client.
        /// </summary>
        /// <param name="uploadFileModel"></param>
        /// <returns></returns>
        public IHttpActionResult Post(UploadFileModel uploadFileModel)
        {

            if(!ModelState.IsValid)
                return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState));

            ePJToAdd _myPj = new ePJToAdd {
                SaveAs = (!_pref.IsFullUnicode && eLibTools.ContainsNonUtf8(uploadFileModel.SaveAs)) 
                    ? eLibTools.RemoveDiacritics(uploadFileModel.SaveAs) 
                    : HttpUtility.UrlDecode(uploadFileModel.SaveAs),
                FileId = uploadFileModel.FileId,
                Tab = uploadFileModel.Tab,
                ParentEvtFileId = uploadFileModel.ParentEvtFileId, //Cas d'ajout de pj depuis l'assiatnt d'emailing en signet ou modif
                ParentEvtTab = uploadFileModel.ParentEvtTab,
                PPID = uploadFileModel.PPID,
                PMID = uploadFileModel.PMID,
                ADRID = uploadFileModel.ADRID,
                MailForwarded = uploadFileModel.MailForwarded,
                TypePj = uploadFileModel.PJType,
                UploadLink = uploadFileModel.UploadLink,
            };

            bool _bFromTpl = uploadFileModel.FromTpl;
            List<string> liErrors = new List<string>();

            #region Recuperation des variables de la fiche
            PJUploadInfoModel[] files;

            try
            {
                files = uploadFileModel.UploadInfo
                    .Select(up => JsonConvert.DeserializeObject<PJUploadInfoModel>(up))
                    .ToArray();

            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            if(_myPj.TypePj != 0 || files == null)
                return Ok();

            int nbFileUp = 0;
            string sError = "";

            foreach (HttpPostedFileBase myFile in uploadFileModel.fileCollection)
            {
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
                _myPj.PjUploadInfo = files
                    .Select(fi => new PJUploadInfo(fi.FileName, fi.SaveAs, fi.Action)
                    {
                        PjId = fi.PjId,
                        ReplaceOptDisplayed = fi.ReplaceOptDisplayed,
                    })
                    .FirstOrDefault(f => f.FileName == HttpUtility.UrlDecode(Path.GetFileName(myFile.FileName)));

                _myPj.SaveAs = _myPj.PjUploadInfo?.SaveAs;

                try
                {
                    byte[] byFielStrm = new byte[myFile.InputStream.Length];
                    myFile.InputStream.Read(byFielStrm, 0, byFielStrm.Length);

                    //Sauvegarde du fichier
                    bool retourARddPj =  _myPj.Save(_pref, upFile.ConstructHttpPostedFile(byFielStrm, myFile.FileName, myFile.ContentType), string.Empty, out iPjId);

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
                    if (iPjId > 0)
                        nbFileUp++;
                }
            }

            if (nbFileUp < 1)
            {
                EudoException ex = new EudoException(sMessage: eLibTools.Join(Environment.NewLine, liErrors)
                    , sUserMessage: eResApp.GetRes(_pref, 6724));

                return InternalServerError(ex);
            }


            return Ok();

            #region oldcode
            //if (Request.Form["nFileID"] != null)
            //{
            //    _myPj.FileId = eLibTools.GetNum(Request.Form["nFileID"].ToString());
            //}

            //if (Request.Form["nTab"] != null)
            //{
            //    _myPj.Tab = eLibTools.GetNum(Request.Form["nTab"].ToString());
            //    //_nTab = _myPj.Tab;
            //}


            //if (_requestTools.AllKeys.Contains("fromtpl") && Request.Form["fromtpl"] != null)
            //{
            //    _bFromTpl = Request.Form["fromtpl"].ToString() == "1";
            //    if (_requestTools.AllKeys.Contains("viewtype") && Request.Form["viewtype"] != null)
            //    {
            //        _sViewType = Request.Form["viewtype"].ToString();
            //        viewtype.Value = _sViewType.Length > 0 ? _sViewType : "checkedonly";
            //    }
            //}
            //if (_requestTools.AllKeys.Contains("frommailtemplate") && Request.Form["frommailtemplate"] != null)
            //{
            //    _bFromMailTemplate = Request.Form["frommailtemplate"].ToString() == "1";
            //    if (_requestTools.AllKeys.Contains("viewtype") && Request.Form["viewtype"] != null)
            //    {
            //        _sViewType = Request.Form["viewtype"].ToString();
            //        viewtype.Value = _sViewType.Length > 0 ? _sViewType : "all";
            //    }
            //}
            #endregion

            #region oldcode
            //if (_requestTools.AllKeys.Contains("ppid") && Request.Form["ppid"] != null)
            //{
            //    _myPj.PPID = eLibTools.GetNum(Request.Form["ppid"].ToString());
            //}
            //if (_requestTools.AllKeys.Contains("pmid") && Request.Form["pmid"] != null)
            //{
            //    _myPj.PMID = eLibTools.GetNum(Request.Form["pmid"].ToString());
            //}
            //if (_requestTools.AllKeys.Contains("mailForward") && Request.Form["mailForward"] != null)
            //{
            //    _myPj.MailForwarded = Request.Form["mailForward"].Equals("1");
            //}
            //if (_requestTools.AllKeys.Contains("adrid") && Request.Form["adrid"] != null)
            //{
            //    _myPj.ADRID = eLibTools.GetNum(Request.Form["adrid"].ToString());
            //}

            //if (_requestTools.AllKeys.Contains("radioPJ") && Request.Form["radioPJ"] != null)
            //{
            //    _nPjType = eLibTools.GetNum(Request.Form["radioPJ"].ToString());
            //}

            //if (_requestTools.AllKeys.Contains("uploadvalue") && Request.Form["uploadvalue"] != null)
            //    _myPj.UploadLink = Request.Form["uploadvalue"].ToString();

            #endregion

            #endregion
        }

        // PUT api/<controller>/5
        public IHttpActionResult Put(int id, object value)
        {
            return InternalServerError(new NotImplementedException());
        }

        // DELETE api/<controller>/5
        public override IHttpActionResult Delete(int id)
        {
            return InternalServerError(new NotImplementedException());
        }
    }
}