using System;
using System.IO;
using System.Web;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Internal;
using EudoQuery;

namespace Com.Eudonet.Xrm
{
    public class eImageFieldFile : eImageField
    {
        protected eImageFieldFile(ePref pref, int tab, int descID, int fileID) : base(pref, tab, descID, fileID)
        {

        }

        public static eImageField GetImageFieldFile(ePref pref, int tab, int descID, int fileID)
        {
            eImageFieldFile image = new eImageFieldFile(pref, tab, descID, fileID);
            image.Init();
            return image;
        }

        protected override bool Init()
        {
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_FILE;

            return base.Init();
        }

        public override object Load()
        {
            return base.Load();
        }

        public override bool ImageExists(object value)
        {
            return !String.IsNullOrEmpty(value.ToString());

        }

        protected override bool DeleteFromDatabase()
        {
            _fileName = string.Empty;
            return SaveInDatabase();
        }

        public override bool Copy()
        {
            if (_imageStorageType == ImageStorage.STORE_IN_FILE)
            {
                try
                {
                    //on récupère l'image originale
                    this.FileName = this.DbValue.ToString();
                    _fullPath = String.Concat(_filePath, this.DbValue);
                    this._data = File.ReadAllBytes(this._fullPath);
                    if (this._data.Length > 0)
                    {
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

                            this.FileName = RenameFile();
                            _fullPath = String.Concat(_filePath, this.FileName);

                            // Chemin complet du fichier

                            WriteFile(this._data);

                            CreateThumbnails();


                            /*la demande ne concerne en fait que les images de diffusions
                            if ( _datasFolderType == eLibConst.FOLDER_TYPE.IMAGES && _sFrom.ToLower() == "formular")
                            {    
                                _imageURL = eLibTools.GetImgExternalLink(_fileName, _pref.GetBaseName, eLibConst.FOLDER_TYPE.IMAGES,_pref.AppExternalUrl);
                            }
                            else
                            */

                            if (_datasFolderType == eLibConst.FOLDER_TYPE.FILES)
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
    }
}