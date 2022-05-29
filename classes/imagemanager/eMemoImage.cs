using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eMemoImage : eAbstractImage
    {
        protected eMemoImage(ePref pref) : base(pref)
        {

        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.MEMO;
            _imageStorageType = EudoQuery.ImageStorage.STORE_IN_FILE;

            return base.Init();
        }

        public static eMemoImage GetMemoImage(ePref pref)
        {
            return new eMemoImage(pref);
        }

    }
}