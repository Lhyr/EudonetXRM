using Com.Eudonet.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eAppLogo : eAbstractImage
    {
        const String DEFAULT_LOGO_URL = "themes/default/images/emain_logo.png";

        protected eAppLogo(ePref pref) : base(pref)
        {

        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.LOGO;
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_FILE;

            return base.Init();
        }

        /// <summary>
        /// Chargement de l'image existante
        /// </summary>
        /// <returns></returns>
        public override object Load()
        {
            Dictionary<eLibConst.CONFIG_DEFAULT, String> defConfigs = _pref.GetConfigDefault(new List<eLibConst.CONFIG_DEFAULT> { eLibConst.CONFIG_DEFAULT.LOGONAME });
            if ((String.IsNullOrEmpty(defConfigs[eLibConst.CONFIG_DEFAULT.LOGONAME])))
            {
                _imageURL = DEFAULT_LOGO_URL;
            }
            else
            {
                _fileName = defConfigs[eLibConst.CONFIG_DEFAULT.LOGONAME];
                _imageURL = String.Concat(eLibTools.GetWebDatasPath(_datasFolderType, _pref.GetBaseName), @"/", _fileName);
                _fullPath = String.Concat(_filePath, _fileName);

            }
            _imageURL = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", _imageURL);
            return _imageURL;
        }


        public static eAppLogo GetAppLogo(ePref pref)
        {
            eAppLogo logo = new eAppLogo(pref);
            logo.Init();
            return logo;
        }

        /// <summary>
        /// Redimensionnement de l'image pour prendre le format 60x40
        /// </summary>
        /// <returns></returns>
        protected override bool CreateThumbnails()
        {
            System.Drawing.Image thumbnailImage = null;
            String thumbnailFilename = String.Empty;
            System.IO.MemoryStream imageStream = null;
            Boolean ok = false;
            try
            {
                _fileName = String.Concat("logo_", _fileName);
                thumbnailImage = eImageTools.GetThumbnail(_fullPath, eConst.LOGO_IMG_WIDTH, eConst.LOGO_IMG_HEIGHT);
                thumbnailFilename = String.Concat(_filePath, "\\", _fileName);


                imageStream = new System.IO.MemoryStream();
                thumbnailImage.Save(thumbnailFilename);
                ok = true;
            }
            catch (Exception exc)
            {
                _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                Width = 16;
                Height = 16;
                _userError = eResApp.GetRes(_pref, 6161);
                _debugError = String.Concat(exc.Message, " : ", exc.StackTrace);
            }
            finally
            {
                thumbnailImage.Dispose();
                imageStream.Dispose();
                // Suppression de l'ancien fichier (non redimensionné)
                File.Delete(_fullPath);
                // Mise à jour du nom de fichier pour l'URL à renvoyer
                _fullPath = thumbnailFilename;
            }

            return ok;
        }

        protected override bool SaveInDatabase()
        {
            return _pref.SetConfigDefaultScalar("LogoName", _fileName);
        }

        protected override bool DeleteFromDatabase()
        {
            _imageURL = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", DEFAULT_LOGO_URL);
            return _pref.SetConfigDefaultScalar("LogoName", string.Empty);
        }
    }
}