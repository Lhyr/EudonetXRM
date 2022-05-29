using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eImageFieldURL : eImageField
    {
        protected eImageFieldURL(ePref pref, int tab, int descID, int fileID) : base(pref, tab, descID, fileID)
        {

        }

        public static eImageField GetImageFieldURL(ePref pref, int tab, int descID, int fileID)
        {
            eImageFieldURL image = new eImageFieldURL(pref, tab, descID, fileID);
            image.Init();
            return image;
        }

        protected override bool Init()
        {
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_URL;

            return base.Init();
        }

        public override object Load()
        {
            return base.Load();
        }

        public override bool ImageExists(object value)
        {
            try
            {
                Uri uriImageTest = new Uri(value.ToString());
                return true;
            }

            catch
            {
                return false;
            }

        }

        public override bool Delete()
        {
            _fileName = string.Empty;
            return SaveInDatabase();
        }


    }
}