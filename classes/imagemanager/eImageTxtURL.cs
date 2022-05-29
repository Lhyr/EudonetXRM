using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eImageTxtURL : eAbstractImage
    {
        protected eImageTxtURL(ePref pref) : base(pref)
        {

        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.TXT_URL;
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_FILE;

            return base.Init();
        }

        public static eImageTxtURL GetImageTxtURL(ePref pref)
        {
            return new eImageTxtURL(pref);
        }

    }
}