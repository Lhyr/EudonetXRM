using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Com.Eudonet.Core.Model;
using System.Drawing;

namespace Com.Eudonet.Xrm
{
    public class eAbstractImage
    {
        protected ePref _pref;
        protected String _fileName = "";
        protected String _filePath = "";
        protected String _fullPath = "";
        protected string _imageURL = "";
        protected string _imageWebURL = string.Empty;
        protected int _fileSize = 0;
        protected String _contentType = "";
        protected byte[] _data = null;
        private int width = 0;
        private int height = 0;
        protected String _debugError = string.Empty;
        protected String _userError = string.Empty;

        protected eLibConst.IMAGE_TYPE _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;
        protected eLibConst.FOLDER_TYPE _datasFolderType = eLibConst.FOLDER_TYPE.FILES;
        protected ImageStorage _imageStorageType = ImageStorage.STORE_IN_FILE;
        protected String _thumbnailURL = "";

        /// <summary>
        /// Crée une fiche lors de la sauvegarde en base si FileID > 0 - Comportement par défaut XRM/E17 : oui, comportement par défaut saisie guidée : non - #90 392
        /// </summary>
        protected bool _createInDatabase = true;
        /// <summary>
        /// Indiwue s'il faut vérifier l'intégrité de l'image lors de l'ajout - Comportement par défaut XRM/E17 : non, comportement par défaut saisie guidée : oui - #90 392 et US #3704 (Tâche #5533)
        /// </summary>
        private bool _checkIntegrityOnUpload = false;

        /// <summary>
        /// Origine de la demande d'instanciation (type d'éditeur memo par exemple)
        /// </summary>
        private string _sFrom = "";


        #region Accesseurs        
        /// <summary>
        /// Nom du fichier
        /// </summary>
        public String FileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;
            }
        }
        /// <summary>
        /// URL de l'image
        /// </summary>
        public string ImageURL
        {
            get
            {
                return _imageURL;
            }

            set
            {
                _imageURL = value;
            }
        }

        /// <summary>
        /// URL de l'image utiliser dans ckEditor
        /// </summary>
        public string ImageWebURL
        {
            get
            {
                return _imageWebURL;
            }

            set
            {
                _imageWebURL = value;
            }
        }

        public ImageStorage StorageType
        {
            get
            {
                return _imageStorageType;
            }

            set
            {
                _imageStorageType = value;
            }
        }

        /// <summary>
        /// URL de la miniature
        /// </summary>
        public String ThumbnailURL
        {
            get
            {
                return _thumbnailURL;
            }

            set
            {
                _thumbnailURL = value;
            }
        }

        public string UserError
        {
            get
            {
                return _userError;
            }

            set
            {
                _userError = value;
            }
        }

        public string DebugError
        {
            get
            {
                return _debugError;
            }

            set
            {
                _debugError = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                this.width = value;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }

            set
            {
                this.height = value;
            }
        }

        public String ContentType
        {
            get
            {
                return _contentType;
            }

            set
            {
                this._contentType = value;
            }
        }

        /// <summary>
        /// Indique s'il faut créer une fiche lors de la sauvegarde en base si FileID > 0 - Comportement par défaut XRM/E17 - #90 392
        /// </summary>
        public bool CreateInDatabase
        {
            get
            {
                return _createInDatabase;
            }
            set
            {
                this._createInDatabase = value;
            }
        }

        /// <summary>
        /// Indiwue s'il faut vérifier l'intégrité de l'image lors de l'ajout - Comportement par défaut XRM/E17 : non, comportement par défaut saisie guidée : oui - #90 392 et US #3704 (Tâche #5533)
        /// </summary>
        public bool CheckIntegrityOnUpload
        {
            get
            {
                return _checkIntegrityOnUpload;
            }

            set
            {
                _checkIntegrityOnUpload = value;
            }
        }
        #endregion

        /// <summary>
        /// Retourne l'instance qui correspond au type d'image
        /// </summary>
        /// <param name="pref"></param>
        /// <param name="type"></param>
        /// <param name="tab"></param>
        /// <param name="fileid"></param>
        /// <param name="sFromType">Provenance de la demande</param>
        /// <returns></returns>
        public static eAbstractImage GetImage(ePref pref, eLibConst.IMAGE_TYPE type, int tab = 0, int fileid = 0, int fieldDescid = 0, string sFromType = "")
        {
            eAbstractImage image = null;
            switch (type)
            {
                case eLibConst.IMAGE_TYPE.LOGO: image = eAppLogo.GetAppLogo(pref); break;
                case eLibConst.IMAGE_TYPE.AVATAR_FIELD:
                case eLibConst.IMAGE_TYPE.AVATAR:
                case eLibConst.IMAGE_TYPE.USER_AVATAR_FIELD:
                    image = eAvatar.GetAvatar(pref, tab, fileid); break;
                case eLibConst.IMAGE_TYPE.IMAGE_FIELD:
                    image = eImageField.GetImageField(pref, tab, fieldDescid, fileid); break;
                case eLibConst.IMAGE_TYPE.IMAGE_WIDGET:
                    image = eWidgetImage.GetWidgetImage(pref, fileid); break;
                case eLibConst.IMAGE_TYPE.MEMO:
                    image = eMemoImage.GetMemoImage(pref); break;
                case eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL: // Backlog #315
                    image = eMemoImageDialogURL.GetMemoImageDialogURL(pref); break;
                case eLibConst.IMAGE_TYPE.TXT_URL:
                    image = eImageTxtURL.GetImageTxtURL(pref); break;
            }

            if (image != null)
            {
                if (image.Init())
                {
                    image._sFrom = sFromType;
                    return image;
                }
            }


            return image;
        }

        protected eAbstractImage(ePref pref)
        {
            _pref = pref;
        }

        /// <summary>
        /// Initialisation
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean Init()
        {
            _datasFolderType = eLibTools.GetFolderTypeFromImageType(_imageType);
            _filePath = String.Concat(eModelTools.GetPhysicalDatasPath(_datasFolderType, _pref), @"\");

            return true;
        }

        /// <summary>
        /// Enregistrement en base
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean SaveInDatabase()
        {
            return true;
        }

        /// <summary>
        /// Création du fichier
        /// </summary>
        /// <returns></returns>
        protected bool CreateFile()
        {
            return true;
        }

        /// <summary>
        /// Suppression 
        /// </summary>
        /// <returns></returns>
        public virtual bool Delete()
        {
            Load();
            if (DeleteFile())
            {
                _fileName = string.Empty;
                _fullPath = string.Empty;
                _imageURL = string.Empty;
                return DeleteFromDatabase();
            }

            return false;
        }

        /// <summary>
        /// Suppression du fichier
        /// </summary>
        /// <returns></returns>
        protected virtual bool DeleteFile()
        {
            try
            {
                _fullPath = String.Concat(_filePath, _fileName);
                if (File.Exists(_fullPath))
                {
                    File.Delete(_fullPath);
                }

                return true;
            }
            catch (Exception exc)
            {
                _debugError = String.Concat(exc.Message, Environment.NewLine, exc.StackTrace);
                _userError = eResApp.GetRes(_pref, 6243);
                return false;
            }
        }

        /// <summary>
        /// Suppression en base
        /// </summary>
        /// <returns></returns>
        protected virtual bool DeleteFromDatabase()
        {
            return true;
        }

        /// <summary>
        /// Retourne un byte[] à partir du fichier posté
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] GetPostedFileData(HttpPostedFile file)
        {
            _fileName = Path.GetFileName(file.FileName);
            _fileSize = file.ContentLength;
            _contentType = file.ContentType;

            if (file != null)
            {
                _data = new byte[_fileSize];
                file.InputStream.Read(_data, 0, _fileSize);
            }

            return _data;
        }

        /// <summary>
        /// Retourne un byte[] à partir du fichier posté
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] GetPostedFileData(HttpPostedFileBase file)
        {
            _fileName = Path.GetFileName(file.FileName);
            _fileSize = file.ContentLength;
            _contentType = file.ContentType;

            if (file != null)
            {
                _data = new byte[_fileSize];
                file.InputStream.Read(_data, 0, _fileSize);
            }

            return _data;
        }

        /// <summary>
        /// Stockage de l'image dans un fichier
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual Boolean Save(byte[] data)
        {
            if (_imageStorageType == ImageStorage.STORE_IN_FILE)
            {
                try
                {
                    if (data.Length > 0 && _fileSize > 0)
                    {
                        _data = data;

                        if (IsValid())
                        {
                            if (!CheckExtension())
                            {
                                return false;
                            }

                            if (!CheckSize())
                            {
                                return false;
                            }

                            RenameFile();

                            // Chemin complet du fichier
                            _fullPath = String.Concat(_filePath, _fileName);

                            WriteFile(data);

                            CreateThumbnails();


                            /*la demande ne concerne en fait que les images de diffusions
                            if ( _datasFolderType == eLibConst.FOLDER_TYPE.IMAGES && _sFrom.ToLower() == "formular")
                            {    
                                _imageURL = eLibTools.GetImgExternalLink(_fileName, _pref.GetBaseName, eLibConst.FOLDER_TYPE.IMAGES,_pref.AppExternalUrl);
                            }
                            else
                            */

                            if (_datasFolderType == eLibConst.FOLDER_TYPE.FILES && _sFrom.ToLower() == "sharringimage")
                            {
                                _imageURL = _fileName;
                            }
                            else
                            {
                                _imageURL = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(_datasFolderType, _pref.GetBaseName), "/", Path.GetFileName(_fullPath))
                                    .Replace("'", @"\'");
                                //BSE:#60 892
                                _imageWebURL = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(_datasFolderType, _pref.GetBaseName), "/", Uri.EscapeDataString(Path.GetFileName(_fullPath)))
                                    .Replace("'", @"\'");
                            }

                            SaveInDatabase();

                            return true;

                        }
                        else
                        {
                            return false;
                        }


                    }
                    else
                    {
                        _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                        Width = 16;
                        Height = 16;
                        return false;
                    }


                }
                catch (Exception e)
                {
                    _userError = eResApp.GetRes(_pref, 92);
                    _debugError = "StoreInFile => " + e.Message + " - " + e.StackTrace;
                }
            }


            return false;
        }

        /// <summary>
        /// Chargement de l'image existante
        /// </summary>
        /// <returns></returns>
        public virtual object Load()
        {
            return null;
        }

        /// <summary>
        /// Vérifie que tout est bon avant enregistrement
        /// TK #5533 - Charge notamment l'image en mémoire pour valider qu'il s'agit bien d'une image
        /// Vérification optionnelle - Désactivée sur XRM/E17, activée sur la saisie guidée - cf. #90 392
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean IsValid()
        {
            if (_checkIntegrityOnUpload) { 
            try
                {
                    using (var ms = new MemoryStream(_data))
                    {
                        using (Image newImage = Image.FromStream(ms))
                        { }
                    }
                }
                catch (OutOfMemoryException)
                {
                    // Le fichier n'est pas un format image, ou GDI+ ne sait pas le lire
                    return false;
                }
                catch (Exception)
                {
                    // Le fichier n'est pas un format image, ou GDI+ ne sait pas le lire
                    return false;
                }

                return true;
            }
            else
                return true;
        }

        /// <summary>
        /// Vérifie que le fichier ne dépasse pas les limites
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean CheckSize()
        {
            return true;
        }

        /// <summary>
        /// Vérifie l'extension du fichier
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean CheckExtension()
        {
            List<string> allowedExtensions = new List<string>(eLibConst.ALLOWED_IMAGE_EXTENSIONS);

            string extension = Path.GetExtension(_fileName.ToLower());

            if (!allowedExtensions.Contains(extension))
            {
                _userError = eResApp.GetRes(_pref, 1545);
                _debugError = eResApp.GetRes(_pref, 1545);
                _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                Width = 16;
                Height = 16;
                return false;
            }

            return true;
        }


        /// <summary>
        /// Renommage du nom de fichier s'il existe déjà
        /// </summary>
        public virtual string RenameFile()
        {
            if (File.Exists(String.Concat(_filePath, _fileName)))
            {
                string strFileTemp = String.Empty;
                string[] aFileTemp = _fileName.Split('.');
                string strFileToCopyExt = aFileTemp[aFileTemp.Length - 1];
                if (strFileToCopyExt != "")
                    strFileToCopyExt = string.Concat(".", strFileToCopyExt);

                string strFileToCopyTitle = _fileName.Substring(0, _fileName.LastIndexOf(strFileToCopyExt));
                int i = 2;
                do
                {
                    strFileTemp = string.Concat(strFileToCopyTitle, "(", i, ")", strFileToCopyExt);
                    i = i + 1;
                }
                while (File.Exists(String.Concat(_filePath, strFileTemp)));

                string strCopyMsg = eResApp.GetRes(_pref, 123).Replace("<NEW_FILE>", strFileTemp);
                strCopyMsg = strCopyMsg.Replace("<OLD_FILE>", _fileName);

                _fileName = strFileTemp;
            }

            return _fileName;
        }

        /// <summary>
        /// Ecriture du fichier à partir du byte[]
        /// </summary>
        /// <param name="data"></param>
        protected virtual void WriteFile(byte[] data)
        {
            eTools.WriteToFile(_fullPath, ref data);
        }

        /// <summary>
        /// Création éventuelle des miniatures
        /// </summary>
        /// <returns></returns>
        protected virtual Boolean CreateThumbnails()
        {
            return true;
        }

        public void SetFileInfos(String filename, int size, String contentType)
        {
            _fileName = filename;
            _fileSize = size;
            _contentType = contentType;
        }

        public virtual bool Copy()
        {
            return true;
        }

    }
}