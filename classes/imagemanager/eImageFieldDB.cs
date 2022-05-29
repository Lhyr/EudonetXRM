using Com.Eudonet.Engine;
using Com.Eudonet.Internal;
using System;
using System.IO;
using Com.Eudonet.Core.Model;
using Com.Eudonet.Common.Enumerations;
using Com.Eudonet.Common.CommonDTO;

namespace Com.Eudonet.Xrm
{
    public class eImageFieldDB : eImageField
    {
        protected eImageFieldDB(ePref pref, int tab, int descID, int fileID) : base(pref, tab, descID, fileID)
        {

        }

        public static eImageField GetImageFieldDB(ePref pref, int tab, int descID, int fileID)
        {
            eImageFieldDB image = new eImageFieldDB(pref, tab, descID, fileID);
            image.Init();
            return image;
        }

        protected override bool Init()
        {
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_DATABASE;

            return base.Init();
        }

        public override object Load()
        {
            return base.Load();
        }

        public override bool ImageExists(object value)
        {
            return eImageTools.DBImageExists(value);

        }


        /// <summary>
        /// Stockage de l'image en base
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override bool Save(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                _imageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                Width = 16;
                Height = 16;
                return false;
            }
            else
            {
                MemoryStream ms = new MemoryStream(data);
                byte[] imageBuffer = ms.ToArray();


                Engine.Engine eng = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
                eng.FileId = _fileID;

                // Ajout du flux Image comme valeur pour Engine
                eng.AddBinaryValue(_descID, imageBuffer);

                // Colonnes supplémentaires à mettre à jour (champ de type IMAGE)
                eng.AddParam(String.Concat(_descID, "_IMAGE_NAME"), Path.GetFileNameWithoutExtension(_fullPath));
                eng.AddParam(String.Concat(_descID, "_IMAGE_TYPE"), _contentType);

                // Envoi de la demande de mise à jour à Engine
                eng.EngineProcess(new StrategyCruSimple());
                if (!eng.Result.Success)
                {
                    _debugError = eng.Result.Error.DebugMsg;
                    return false;
                }

                return true;
            }
        }

        public override bool Delete()
        {
            Engine.Engine eng = eModelTools.GetEngine(_pref, _tab, eEngineCallContext.GetCallContext(EngineContext.APPLI));
            eng.FileId = _fileID;

            // Ajout du flux Image comme valeur pour Engine
            eng.AddBinaryValue(_descID, null, false);

            // Envoi de la demande de mise à jour à Engine
            eng.EngineProcess(new StrategyCruSimple());
            if (!eng.Result.Success)
            {
                _debugError = eng.Result.Error.DebugMsg;
                return false;
            }

            return true;
        }
    }
}