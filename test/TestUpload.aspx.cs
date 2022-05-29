using System;
using System.IO;
using System.Web;
using System.Web.UI;
using Com.Eudonet.Internal;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{

    /// <summary>
    /// page de test d'upload
    /// </summary>
    public partial class TestUpload : eEudoPage //: System.Web.UI.Page
    {
        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// chargement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.ContentType.Contains("multipart/form-data"))
            {
                for (int j=0;j<Request.Files.Count;j++)
                {
                    HttpPostedFile myFile = Request.Files[j];
                    Int32 nFileLen = myFile.ContentLength;


                    myFile.InputStream.Position = 0;
                    System.IO.StreamReader str = new System.IO.StreamReader(myFile.InputStream);


                    byte[] myData = new byte[nFileLen];
                    myFile.InputStream.Read(myData, 0, nFileLen);
                    string _filename = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.AVATARS, _pref), @"\", DateTime.Now.ToString("ddMMyyyyhhmmss"), "_", Path.GetFileName(myFile.FileName));
                    //string _filename = String.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.AVATARS, _pref), @"\", DateTime.Now.ToString("ddMMyyyyhhmmss"), "_", "TEST");

                    eTools.WriteToFile(_filename, ref myData);
                    /*
                    FileInfo _fi = new System.IO.FileInfo(_filename);
                    
                    //Redimension de l'image pour la taille par défaut des avatar
                    try
                    {

                        System.Drawing.Image image = System.Drawing.Image.FromFile(_filename);

                        #region Obtention des nouvelles dimension de l'image en conservant le ratio de l'image :
                        int originalW = image.Width;
                        int originalH = image.Height;
                        int newW = (int)((originalW < originalH) ? (decimal)eConst.AVATAR_IMG_WIDTH * ((decimal)originalW / (decimal)originalH) : (decimal)eConst.AVATAR_IMG_WIDTH);
                        int newH = (int)((originalH < originalW) ? (decimal)eConst.AVATAR_IMG_HEIGHT * ((decimal)originalH / (decimal)originalW) : (decimal)eConst.AVATAR_IMG_HEIGHT);
                        #endregion

                        System.Drawing.Image thumbnailImage = image.GetThumbnailImage(newW, newH, null, IntPtr.Zero);
                        System.IO.MemoryStream imageStream = new System.IO.MemoryStream();

                        try
                        {

                            thumbnailImage.Save(string.Concat(eModelTools.GetPhysicalDatasPath(eLibConst.FOLDER_TYPE.AVATARS, _pref), "\\", _pref.User.UserId, "testTITI.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
                            //bAddPicture = true;
                        }
                        catch (Exception e2)
                        {
                            throw (e2);
                        }
                        finally
                        {
                            thumbnailImage.Dispose();
                            image.Dispose();
                            imageStream.Dispose();
                            //Suppression de l'ancien fichier (non redimensionné)
                            File.Delete(_filename);
                        }

                    }
                    catch (Exception e1)
                    {
                        //  this.lblError.Text = eResApp.GetRes(_pref.Lang, 6161) + " : " + e1.ToString();
                        //    return;


                        //Avec exception
                        String sDevMsg = String.Concat("Erreur sur eImageDialog.aspx");
                        sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception MSG ", e1.Message,
                            Environment.NewLine, "Exception StackTrace ", e1.StackTrace
                            );


                        ErrorContainer = eErrorContainer.GetDevUserError(
                            eLibConst.MSG_TYPE.CRITICAL,
                            eResApp.GetRes(_pref.Lang, 72),   // Message En-tête : Une erreur est survenue
                            String.Concat(eResApp.GetRes(_pref.Lang, 422), "<br>", eResApp.GetRes(_pref.Lang, 544)),  //  Détail : pour améliorer...
                            eResApp.GetRes(_pref.Lang, 72),  //   titre
                            String.Concat(sDevMsg)

                            );

                        if (_bFromeUpdater)
                            LaunchError();
                        else
                        {

                            LaunchErrorHTML(true, null, "top.window['modalImage'].hide();");
                        }

                    }
                    finally
                    {

                    }*/
                }
                /*
                XmlNode detailsNode = null;
                XmlDocument _xmlResult = new XmlDocument();

                detailsNode = _xmlResult.CreateElement("contents");

                XmlNode _maintNode = _xmlResult.CreateXmlDeclaration("1.0", "UTF-8", null);
                _xmlResult.AppendChild(_maintNode);

                XmlNode _resultNode = _xmlResult.CreateElement("result");
                _resultNode.InnerText = "SUCCESS";
                detailsNode.AppendChild(_resultNode);

                XmlNode _errDesc = _xmlResult.CreateElement("errordescription");
                _errDesc.InnerText = String.Empty;
                detailsNode.AppendChild(_errDesc);

                _xmlResult.AppendChild(detailsNode);


                RenderResult(RequestContentType.XML, delegate() { return _xmlResult.OuterXml; });
                 * */
            }



        }

    }
    /*
    
public class UploadController : ApiController
{
    public Task<HttpResponseMessage> PostFormData()
    {
        // Check if the request contains multipart/form-data.
        if (!Request.Content.IsMimeMultipartContent())
        {
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        string root = HttpContext.Current.Server.MapPath("~/App_Data");
        var provider = new MultipartFormDataStreamProvider(root);

        // Read the form data and return an async task.
        var task = Request.Content.ReadAsMultipartAsync(provider).
            ContinueWith<HttpResponseMessage>(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                // This illustrates how to get the file names.
                foreach (MultipartFileData file in provider.FileData)
                {
                    Trace.WriteLine(file.Headers.ContentDisposition.FileName);
                    Trace.WriteLine("Server file path: " + file.LocalFileName);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            });

        return task;
    }
}
    */

}