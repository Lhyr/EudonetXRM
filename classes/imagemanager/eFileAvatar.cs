using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eFileAvatar : eAvatar
    {
        public eFileAvatar(ePref pref, int tab, int fileID) : base(pref, tab, fileID)
        {

        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.AVATAR_FIELD;

            //if (_tab == TableType.PP.GetHashCode())
            //    _defaultAvatarPath = "../images/ui/avatar.png";
            //else if (_tab == TableType.PM.GetHashCode())
            //    _defaultAvatarPath = "../images/iVCard/unknown_pm.png";

            return base.Init();
        }

        protected override bool DeleteFromDatabase()
        {
            if (base.DeleteFromDatabase())
            {
                _imageURL = _defaultAvatarPath;
                return true;
            }
            return false;
        }

    }
}