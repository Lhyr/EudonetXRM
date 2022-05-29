using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe représentant un champ de type Image (hors Avatars 75)
    /// </summary>
    /// <seealso cref="Com.Eudonet.Xrm.eFieldImage" />
    public class eImageField : eFieldImage
    {
        protected Boolean _updateAllowed = false;
        public object DbValue { get; private set; }

        protected eImageField(ePref pref, int tab, int descID, int fileID) : base(pref, tab, descID, fileID)
        {

        }

        /// <summary>
        /// Retourne la bonne instance de eImageField suivant son type de stockage
        /// </summary>
        /// <param name="pref">The preference.</param>
        /// <param name="tab">The tab.</param>
        /// <param name="descID">The desc identifier.</param>
        /// <param name="fileID">The file identifier.</param>
        /// <returns></returns>
        public static eImageField GetImageField(ePref pref, int tab, int descID, int fileID)
        {
            eImageField image = new eImageField(pref, tab, descID, fileID);
            object value = image.Load();
            if (image.StorageType == ImageStorage.STORE_IN_DATABASE)
            {
                image = eImageFieldDB.GetImageFieldDB(pref, tab, descID, fileID);
            }
            else if (image.StorageType == ImageStorage.STORE_IN_URL)
            {
                image = eImageFieldURL.GetImageFieldURL(pref, tab, descID, fileID);
            }
            else
            {
                image = eImageFieldFile.GetImageFieldFile(pref, tab, descID, fileID);
            }
            image.DbValue = value;

            return image;
        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;
            DbValue = String.Empty;

            return base.Init();
        }

        protected void LoadImageStorage()
        {
            _imageStorageType = eImageTools.GetStorageType(_pref, _descID);
        }

        public override object Load()
        {
            //eDataFillerGeneric dtf = new eDataFillerGeneric(_pref, _tab, ViewQuery.CUSTOM);
            //dtf.EudoqueryComplementaryOptions =
            //    delegate (EudoQuery.EudoQuery eqDelegate)
            //    {
            //        eqDelegate.SetListCol = _descID.ToString();
            //        eqDelegate.SetFileId = _fileID;

            //    };

            //dtf.Generate();

            //if (dtf.ErrorMsg.Length != 0 || dtf.InnerException != null)
            //{
            //    if (dtf.ErrorType != QueryErrorType.ERROR_NUM_PREF_NOT_FOUND)
            //        throw new Exception(String.Concat(dtf.ErrorMsg, dtf.InnerException == null ? String.Empty : dtf.InnerException.Message));
            //}

            //if (dtf.ListRecords.Count > 0)
            //{
            //    eFieldRecord f = dtf.GetFirstRow().GetFields.Find(fld => fld.FldInfo.Descid == _descID);
            //    if (f != null)
            //    {
            //        _imageStorageType = (ImageStorage)(f.FldInfo.ImgStorage);
            //        _updateAllowed = f.RightIsUpdatable;

            //        DbValue = GetValue(f);
            //        return DbValue;

            //    }
            //}

            //return string.Empty¸;
            Field field;
            eFieldRecord fRecord;
            object objData = eLibTools.GetDBImageData(_pref, _descID, _fileID, out field, out fRecord, out _updateAllowed, out _debugError);
            _imageStorageType = field.ImgStorage;
            return objData;
        }

        protected virtual String GetValue(eFieldRecord r)
        {
            return r.Value;
        }

        /// <summary>
        /// L'image existe-t-elle ?
        /// </summary>
        /// <returns></returns>
        public virtual Boolean ImageExists(object value)
        {
            return false;
        }


    }
}