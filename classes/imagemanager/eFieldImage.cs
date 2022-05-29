using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    public class eFieldImage : eAbstractImage
    {
        ImageStorage _storageMode = ImageStorage.STORE_IN_FILE;
        /// <summary>
        /// DescID de la table
        /// </summary>
        protected int _tab = 0;
        /// <summary>
        /// DescID du champ image
        /// </summary>
        protected int _descID = 0;
        /// <summary>
        /// ID de la fiche
        /// </summary>
        protected int _fileID = 0;

        protected eFieldImage(ePref pref, int tab, int descid, int fileID, ImageStorage storageType = ImageStorage.STORE_IN_FILE) : base(pref)
        {
            _tab = tab;
            _descID = descid;
            _fileID = fileID;
            _imageStorageType = storageType;
        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;
            _datasFolderType = eLibConst.FOLDER_TYPE.FILES;

            return base.Init();
        }

        public static eFieldImage GetFieldImage(ePref pref, int tab, int descid, int fileID)
        {
            return new eFieldImage(pref, tab, descid, fileID);
        }

        /// <summary>
        /// Enregistre les modifications associées au champ Image en base de données
        /// </summary>
        /// <returns>true si la sauvegarde en base a été effectuée (ou ignorée si FileID 0 / createInDatabase = false), false en cas de sauvegarde en erreur</returns>
        protected override bool SaveInDatabase()
        {
            // #90 392 - On ne crée pas d'enregistrement (et de fiche) en base si FileID = 0, sauf si demandé explicitement
            if (_fileID > 0 || _createInDatabase)
            {
                Engine.Engine eng = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = _fileID;

                eng.AddNewValue(_descID, _fileName, false, true);

                eng.EngineProcess(new StrategyCruSimple());

                if (!eng.Result.Success)
                {
                    _debugError = eng.Result.Error.DebugMsg;
                    return false;
                }
            }

            return true;
        }

        protected override bool CheckSize()
        {
            if (_fileSize > Math.Round(Int32.Parse(eLibTools.GetServerConfig("AvatarSize", eLibConst.AVATAR_SIZE.ToString())) * 1024.00 * 1024.00, 2, MidpointRounding.AwayFromZero))
            {
                _debugError = eResApp.GetRes(_pref, 1537).Replace("<CURRENT>", String.Concat(Math.Round(_fileSize / 1024.00 / 1024.00, 2, MidpointRounding.AwayFromZero), " M")).Replace("<MAX>", String.Concat(eLibConst.AVATAR_SIZE, " M"));
                _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                Width = 16;
                Height = 16;
                return false;
            }
            return true;
        }

        protected override bool CreateThumbnails()
        {
            if (_storageMode == ImageStorage.STORE_IN_FILE && _pref.ThumbNailEnabled)
            {
                eImageTools.CreateThumbnail(_fullPath, eLibConst.THUMBNAILWIDTH, eLibConst.THUMBNAILHEIGHT, false, out _debugError);
                if (String.IsNullOrEmpty(_debugError))
                    return true;
            }
            return false;
        }


        public Boolean StoreInURL(String url)
        {
            _imageURL = url;
            _fileName = url;

            Uri uriImageTest = null;
            if (_imageURL.Length > 0)
            {
                try
                {
                    uriImageTest = new Uri(_imageURL);
                }
                catch
                {
                    _debugError = eResApp.GetRes(_pref, 6275);
                    return false;
                }
            }


            // Affectation de l'URL à envoyer en base si l'URL saisie est conforme
            if (uriImageTest != null)
            {
                _imageURL = uriImageTest.ToString();

                Engine.Engine eng = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = _fileID;
                eng.AddNewValue(_descID, _imageURL);
                eng.EngineProcess(new StrategyCruSimple());
                if (!eng.Result.Success)
                {
                    _userError = eResApp.GetRes(_pref, 92);
                    _debugError = eng.Result.Error.DebugMsg;
                    return false;
                }
                return true;
            }
            else
            {
                // Pas de message d'erreur si aucun fichier n'est à envoyer : le manager peut être appelé via JS sans que celui-ci sache ce qui se trouve côté serveur/session
                _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                Width = 16;
                Height = 16;
                return false;
            }
        }


        /// <summary>
        /// Retourne l'URL de l'image
        /// </summary>
        /// <param name="thumbnail"></param>
        /// <param name="width">Largeur prise en compte si thumbnail = false</param>
        /// <param name="height">Hauteur prise en compte si thumbnail = false</param>
        /// <param name="forceFromSession">Force à chercher l'image en session (fid=-1), pour les templates en popup qui ont déjà un fileId</param>
        /// <returns></returns>
        public virtual String GetImageURL(Boolean thumbnail = false, int width = 0, int height = 0, bool forceFromSession = false)
        {
            String imageURL = string.Empty;
            if (thumbnail)
            {
                width = 16;
                height = 16;
            }

            string strImageSizeParams = String.Empty;
            if (width > 0)
                strImageSizeParams = String.Concat(strImageSizeParams, "&w=", width.ToString());
            if (height > 0)
                strImageSizeParams = String.Concat(strImageSizeParams, "&h=", height.ToString());

            imageURL = String.Concat("eImage.aspx?did=", _descID, "&fid=", (_fileID == 0) ? -1 : _fileID, "&it=", _imageType.ToString(), strImageSizeParams);

            if (forceFromSession)
                imageURL = String.Concat(imageURL, "&ffs=1");

            imageURL = String.Concat(imageURL, "&ts=1", DateTime.Now.ToString("yyyyMMddHHmmssfff"));

            _imageURL = imageURL;
            return imageURL;
        }
    }
}