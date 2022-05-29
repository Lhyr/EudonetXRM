using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.IO;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    public class eWidgetImage : eFieldImage
    {
        public static eWidgetImage GetWidgetImage(ePref pref, int widgetId)
        {
            eWidgetImage widget = new eWidgetImage(pref, widgetId);
            widget.Init();
            return widget;
        }

        protected eWidgetImage(ePref pref, int widgetId) : base(pref, TableType.XRMWIDGET.GetHashCode(), XrmWidgetField.ContentSource.GetHashCode(), widgetId)
        {

        }

        protected override bool Init()
        {
            _imageType = eLibConst.IMAGE_TYPE.IMAGE_WIDGET;
            _datasFolderType = eLibConst.FOLDER_TYPE.WIDGET;
            _filePath = String.Concat(eModelTools.GetPhysicalDatasPath(_datasFolderType, _pref), @"\");

            return true;
        }


        protected override bool DeleteFile()
        {

            try
            {
                DirectoryInfo di = new DirectoryInfo(eModelTools.GetPhysicalDatasPath(_datasFolderType, _pref));
                foreach (FileInfo currentFile in di.GetFiles(string.Concat(eLibConst.WIDGET_IMAGE_NAME_PREFIX, _fileID.ToString(), ".*")))
                {
                    if (currentFile.Exists)
                    {
                        currentFile.Delete();
                    }
                }
                _fileName = string.Concat(eLibConst.WIDGET_IMAGE_NAME_PREFIX, _fileID, _fileName.Substring(_fileName.LastIndexOf(".")));
            }
            catch (Exception ex)
            {
                _userError = eResApp.GetRes(_pref, 845);
                _debugError = String.Concat(ex.Message, " - ", ex.StackTrace);
                return false;
            }

            return true;

        }

        protected override bool CreateThumbnails()
        {
            return true;
        }

        public override string RenameFile()
        {
            DeleteFile();
            return _fileName;
        }

        public override string GetImageURL(bool thumbnail = false, int width = 0, int height = 0, bool forceFromSession = false)
        {
            return this._imageURL;
        }

        protected override void WriteFile(byte[] data)
        {
            base.WriteFile(data);

            // Récupération des dimensions de l'image
            System.Drawing.Image image = System.Drawing.Image.FromStream(new System.IO.MemoryStream(data));
            this.Width = image.Width;
            this.Height = image.Height;
            image.Dispose();
        }

        /// <summary>
        /// La sauvegarde de l'image est gérée ailleurs
        /// </summary>
        /// <returns></returns>
        protected override bool SaveInDatabase()
        {
            return true;
        }
    }
}