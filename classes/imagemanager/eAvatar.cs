using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.IO;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    public class eAvatar : eFieldImage
    {
        protected String _defaultAvatarPath = "";


        public static eAvatar GetAvatar(ePref pref, int tab, int fileid)
        {
            eAvatar avatar;
            if (tab == TableType.USER.GetHashCode())
                avatar = new eUserAvatar(pref, fileid);
            else
                avatar = new eFileAvatar(pref, tab, fileid);

            avatar.Init();
            return avatar;
        }

        protected eAvatar(ePref pref, int tab, int fileId) : base(pref, tab, tab + AllField.AVATAR.GetHashCode(), fileId)
        {

        }

        public override object Load()
        {
            eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, _tab, ViewQuery.CUSTOM);
            dtf.EudoqueryComplementaryOptions =
                delegate (EudoQuery.EudoQuery eqDelegate)
                {
                    eqDelegate.SetListCol = _descID.ToString();
                    eqDelegate.SetFileId = _fileID;

                };

            dtf.Generate();

            if (dtf.ErrorMsg.Length != 0 || dtf.InnerException != null)
            {
                if (dtf.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
                    throw new Exception(String.Concat(dtf.ErrorMsg, dtf.InnerException == null ? String.Empty : dtf.InnerException.Message));
            }


            if (dtf.ListRecords.Count == 1)
            {
                eFieldRecord f = dtf.GetFirstRow().GetFields.Find(fld => fld.FldInfo.Descid == _descID);
                if (f != null)
                {
                    _fileName = f.DisplayValue;
                    _imageURL = String.Concat(eLibTools.GetWebDatasPath(_datasFolderType, _pref.GetBaseName), @"/", _fileName);
                    _fullPath = String.Concat(_filePath, _fileName);
                    return _imageURL;
                }

            }

            // Sinon par défaut :
            _imageURL = _defaultAvatarPath;

            return _imageURL;
        }

        protected override void WriteFile(byte[] data)
        {
            return;
        }

        protected override bool DeleteFile()
        {

            try
            {
                if (File.Exists(_fullPath))
                    File.Delete(_fullPath);
                if (File.Exists(_fullPath.Replace(".jpg", String.Concat(eLibConst.MOBILE_SUFFIX, ".jpg"))))
                    File.Delete(_fullPath.Replace(".jpg", String.Concat(eLibConst.MOBILE_SUFFIX, ".jpg")));
                if (File.Exists(_fullPath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg"))))
                    File.Delete(_fullPath.Replace(".jpg", String.Concat(eLibConst.THUMB_SUFFIX, ".jpg")));


            }
            catch (Exception ex)
            {
                _userError = eResApp.GetRes(_pref, 845);
                _debugError = String.Concat(ex.Message, " - ", ex.StackTrace);
                return false;
            }

            return true;

        }


        protected override bool CreateThumbnails()
        {
            //Recherche de l'avatar existant.
            string sBaseName;
            if (_fileID == 0)
                sBaseName = String.Concat(_tab, "_", eLibTools.GetToken(5), "_", DateTime.Now.ToString("ddMMyyyyhhmmss"));
            else if (_tab == (int)TableType.USER)
                sBaseName = String.Concat(_tab, "_", _fileID);
            else
                sBaseName = String.Concat(_tab, "_", _fileID, "_", DateTime.Now.ToString("ddMMyyyyhhmmss"));


            eLibTools.PictureWebFullFileName webFullFileName = null;

            String originalFileName = String.Concat(sBaseName, ".jpg");
            _fileName = originalFileName;

            String mobileFileName = String.Concat(sBaseName, eLibConst.MOBILE_SUFFIX, ".jpg");
            String thumbnailFileName = String.Concat(sBaseName, eLibConst.THUMB_SUFFIX, ".jpg");

            _fullPath = String.Concat(_filePath, _fileName);
            String mobileFilePath = String.Concat(_filePath, mobileFileName);
            String thumbnailFilePath = String.Concat(_filePath, thumbnailFileName);


            try
            {
                MemoryStream msImg = new MemoryStream(_data);
                System.Drawing.Image image = System.Drawing.Image.FromStream(msImg);

                eLibTools.RotateImageFromEXIF(ref image);

                #region Suppression du fichier de destination s'il existe déjà
                DeleteFile();
                #endregion

                eLibTools.CreatePictureVersions(image, sBaseName, _filePath, _pref.AppExternalUrl, _pref.GetBaseName, out webFullFileName);

                _imageURL = webFullFileName.originalFileName;
                _thumbnailURL = webFullFileName.thumbnailFileName;

            }
            catch (Exception ex)
            {
                _userError = "Création des miniatures échouée"; // TODO RES
                _debugError = String.Concat(ex.Message, " - ", ex.StackTrace);
                return false;
            }


            return true;
        }

        protected override bool SaveInDatabase()
        {
            if (_fileID == 0)
                return true;

            Engine.Engine eng = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = _fileID;

            eng.AddNewValue(_tab + EudoQuery.AllField.AVATAR.GetHashCode(), _fileName, false, true);

            if (_tab == (int)TableType.USER)
                eng.EngineProcess(new StrategyCruUser());
            else
                eng.EngineProcess(new StrategyCruSimple());

            if (!eng.Result.Success)
            {
                throw new Exception(eng.Result.Error.DebugMsg);
            }

            return true;
        }

        protected override bool DeleteFromDatabase()
        {
            _fileName = string.Empty;
            return SaveInDatabase();
        }
    }
}