using Com.Eudonet.Internal;
using EudoQuery;
using System;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eUserAvatar : eAvatar
    {

        public eUserAvatar(ePref pref, int userID) : base(pref, TableType.USER.GetHashCode(), userID)
        {

        }



        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.AVATAR;
            _defaultAvatarPath = "themes/default/images/ui/avatar.png";

            if (_fileID <= 0)
                return false;




            return base.Init();
        }

        protected override bool SaveInDatabase()
        {
            if (base.SaveInDatabase())
            {
                return eUser.SetFieldValue(_pref, this._fileID, "HasAvatar", (String.IsNullOrEmpty(_fileName)) ? false : true, System.Data.SqlDbType.Bit);
            }
            return false;
        }


        public override string RenameFile()
        {
            _fileName = String.Concat(DateTime.Now.ToString("ddMMyyyyhhmmss"), "_", _fileName);
            return _fileName;
        }

        public override string GetImageURL(bool thumbnail = false, int width = 0, int height = 0, bool forceFromSession = false)
        {
            if (_fileID > 0 && !forceFromSession)
            {
                return _imageURL;
            }
            else
                return base.GetImageURL(thumbnail, width, height, forceFromSession);
        }




        //protected override bool DeleteFile()
        //{
        //    DirectoryInfo di = new DirectoryInfo(eModelTools.GetPhysicalDatasPath(_datasFolderType, _pref));

        //    foreach (FileInfo currentFile in di.GetFiles(string.Concat(_pref.User.UserId, ".*")))
        //    {
        //        if (currentFile.Exists)
        //        {
        //            currentFile.Delete();
        //        }
        //    }
        //    return base.DeleteFile();
        //}
    }
}