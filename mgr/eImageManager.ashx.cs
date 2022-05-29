using Com.Eudonet.Internal;
using EudoExtendedClasses;
using EudoQuery;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using System.Xml;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <className>eGetFieldManager</className>
    /// <summary>Permet d'effectuer des manipulations d'images côté serveur (dont ajout)</summary>
    /// <purpose>A appeler depuis un JavaScript par ex.</purpose>
    /// <authors>MAB</authors>
    /// <date>2015-02-12</date>
    public class eImageManager : eEudoManager
    {
        Boolean _bDragAndDrop = false;
        Boolean _bStoredInSession = false;
        Boolean _bParentIsPopup = false;
        Boolean _bIsUpdateOnBlur = false;
        String _imageURL = string.Empty;
        String _imageName = string.Empty;
        int _width = 0;
        int _height = 0;

        /// <summary>
        /// Type d'action disponible pour ce manager :
        /// </summary>
        public enum ImageManagerAction
        {
            /// <summary>
            /// Enregistrement d'une image
            /// </summary>
            UPLOAD,
            /// <summary>
            /// Suppression d'une image (notamment l'objet de session)
            /// </summary>
            DELETE
        }


        /// <summary>
        ///  Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
        /// </summary>
        public class ImageManagerContext
        {
            #region Propriétés

            private ImageManagerAction _action = ImageManagerAction.UPLOAD;
            private HttpPostedFile _imageFile = null;
            private string _postedImageURL = String.Empty;
            private Int32 _imageFieldDescId = 0;
            private Int32 _imageFieldFileId = 0;
            private eLibConst.IMAGE_TYPE _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;
            private Field _imageField = null;
            private eFieldRecord _imageFieldRecord = null;
            private bool _computeRealThumbnail = false;
            private int _imageWidth = 16;
            private int _imageHeight = 16;
            private String _imageAlt = String.Empty;

            #endregion

            #region Accesseurs

            /// <summary>
            /// Action à effectuer
            /// A passer en paramètre depuis l'appelant, sous forme de String
            /// UPLOAD : enregistrer une image selon son mode de stockage (fichier, base de données, URL)
            /// </summary>
            public ImageManagerAction Action
            {
                get { return _action; }
            }

            /// <summary>
            /// Fichier à uploader
            /// </summary>
            public HttpPostedFile ImageFile
            {
                get { return _imageFile; }
            }

            /// <summary>
            /// URL postée
            /// </summary>
            public string PostedImageURL
            {
                get { return _postedImageURL; }
            }

            /// <summary>
            /// DescID du champ à traiter
            /// </summary>
            public Int32 ImageFieldDescId
            {
                get { return _imageFieldDescId; }
            }
            /// <summary>
            /// ID de l'enregistrement pour lequel récupérer la valeur du champ
            /// </summary>
            public Int32 ImageFieldFileId
            {
                get { return _imageFieldFileId; }
            }

            /// <summary>
            /// Type d'image à générer/gérer (Avatar, recherche Google, champ Image, ajout d'image dans un champ Mémo...)
            /// </summary>
            public eLibConst.IMAGE_TYPE ImageType
            {
                get { return _imageType; }
            }

            /// <summary>
            /// Champ image
            /// </summary>
            public Field ImageField
            {
                get
                {
                    return _imageField;
                }
            }

            /// <summary>
            /// Objet de type eFieldRecord contenant les infos sur la fiche à mettre à jour
            /// </summary>
            public eFieldRecord ImageFieldRecord
            {
                get { return _imageFieldRecord; }
            }

            /// <summary>
            /// Indique si le manager doit renvoyer une image retaillée selon les propriétés Width et Height (notamment pour les modes Liste)
            /// </summary>
            public bool ComputeRealThumbnail
            {
                get { return _computeRealThumbnail; }
            }

            /// <summary>
            /// Largeur de l'image à générer si ComputeRealThumbnail est à true
            /// </summary>
            public Int32 ImageWidth
            {
                get { return _imageWidth; }
            }

            /// <summary>
            /// Hauteur de l'image à générer si ComputeRealThumbnail est à true
            /// </summary>
            public Int32 ImageHeight
            {
                get { return _imageHeight; }
            }

            /// <summary>Texte alternatif de l'image</summary>
            public String ImageAlt
            {
                get { return _imageAlt; }
            }

            #endregion

            /// <summary>
            /// Objet permettant de charger le contexte (paramètres d'exécution) pour l'exécution du processus de récupération de données
            /// </summary>
            public ImageManagerContext()
            {

            }

            /// <summary>
            /// Charge le contexte (paramètres d'exécution) à partir de l'objet contexte HTTP (valeurs dans Request.Form)
            /// </summary>
            /// <param name="pref">The preference.</param>
            /// <param name="context">Contexte HTTP contenant les paramètres requis</param>
            /// <param name="error">Message d'erreur si le traitement échoue</param>
            public void Load(ePref pref, HttpContext context, out String error)
            {
                error = String.Empty;
                NameValueCollection parameters = context.Request.Form;

                #region Action à effectuer
                try
                {
                    string strAction = parameters["action"].ToString();
                    switch (strAction)
                    {
                        case "UPLOAD":
                        default:
                            _action = ImageManagerAction.UPLOAD;
                            break;
                        case "DELETE":
                            _action = ImageManagerAction.DELETE;
                            break;
                    }
                }
                catch
                {
                    _action = ImageManagerAction.UPLOAD;
                }
                #endregion

                #region Paramètres obligatoires
                try
                {
                    string strGetFieldError = String.Empty;
                    bool bCanAccessImage = false;
                    object oImageData = null;

                    switch (_action)
                    {
                        case ImageManagerAction.UPLOAD:
                        default:
                            // Paramètres selon stockage : fichier uploadé ou URL
                            if (context.Request.Files["filMyFile"] != null)
                                _imageFile = context.Request.Files["filMyFile"];
                            if (parameters["imageURL"] != null)
                                _postedImageURL = parameters["imageURL"].ToString();
                            // Paramètres communs
                            _imageFieldFileId = Int32.Parse(parameters["fileId"].ToString());
                            _imageFieldDescId = Int32.Parse(parameters["fieldDescId"].ToString());
                            _imageType = (eLibConst.IMAGE_TYPE)Enum.Parse(typeof(eLibConst.IMAGE_TYPE), parameters["imageType"].ToString());
                            _computeRealThumbnail = parameters["computeRealThumbnail"].ToString().Equals("1");
                            _imageWidth = Int32.Parse(parameters["imageWidth"].ToString());
                            _imageHeight = Int32.Parse(parameters["imageHeight"].ToString());

                            // Instanciation d'un eFieldRecord en fonction des paramètres
                            strGetFieldError = String.Empty;
                            _imageFieldRecord = new eFieldRecord();
                            _imageField = new Field();
                            bCanAccessImage = false;
                            oImageData = eLibTools.GetDBImageData(pref, _imageFieldDescId, _imageFieldFileId, out _imageField, out _imageFieldRecord, out bCanAccessImage, out strGetFieldError);
                            if (strGetFieldError.Length > 0)
                                error = strGetFieldError;

                            break;
                        case ImageManagerAction.DELETE:
                            _imageFieldFileId = Int32.Parse(parameters["fileId"].ToString());
                            _imageFieldDescId = Int32.Parse(parameters["fieldDescId"].ToString());
                            _imageType = (eLibConst.IMAGE_TYPE)Enum.Parse(typeof(eLibConst.IMAGE_TYPE), parameters["imageType"].ToString());
                            _computeRealThumbnail = parameters["computeRealThumbnail"].ToString().Equals("1");
                            _imageWidth = Int32.Parse(parameters["imageWidth"].ToString());
                            _imageHeight = Int32.Parse(parameters["imageHeight"].ToString());

                            // Instanciation d'un eFieldRecord en fonction des paramètres
                            strGetFieldError = String.Empty;
                            _imageFieldRecord = new eFieldRecord();
                            _imageField = new Field();
                            bCanAccessImage = false;
                            oImageData = eLibTools.GetDBImageData(pref, _imageFieldDescId, _imageFieldFileId, out _imageField, out _imageFieldRecord, out bCanAccessImage, out strGetFieldError);
                            if (strGetFieldError.Length > 0)
                                error = strGetFieldError;

                            break;
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }
                #endregion
            }

            /// <summary>
            /// Charge le contexte (paramètres d'exécution) à partir de l'objet contexte HTTP (valeurs dans Request.Form)
            /// </summary>
            /// <param name="imageFile">The image file.</param>
            /// <param name="postedImageURL">The posted image URL.</param>
            /// <param name="imageType">Type of the image.</param>
            /// <param name="imageField">The image field.</param>
            /// <param name="imageFieldRecord">The image field record.</param>
            /// <param name="imageFieldFileId">The image field file identifier.</param>
            /// <param name="imageFieldDescId">The image field desc identifier.</param>
            /// <param name="computeRealThumbnail">if set to <c>true</c> [compute real thumbnail].</param>
            /// <param name="imageWidth">Width of the image.</param>
            /// <param name="imageHeight">Height of the image.</param>
            /// <param name="imageAlt">The image alt.</param>
            public void LoadForUpload(HttpPostedFile imageFile, string postedImageURL, eLibConst.IMAGE_TYPE imageType, Field imageField, eFieldRecord imageFieldRecord, int imageFieldFileId, int imageFieldDescId, bool computeRealThumbnail, int imageWidth, int imageHeight, String imageAlt)
            {
                _action = ImageManagerAction.UPLOAD;
                _imageFile = imageFile;
                _postedImageURL = postedImageURL;
                _imageFieldFileId = imageFieldFileId;
                _imageFieldDescId = imageFieldDescId;
                _imageType = imageType;
                _computeRealThumbnail = computeRealThumbnail;
                _imageWidth = imageWidth;
                _imageHeight = imageHeight;
                _imageField = imageField;
                _imageFieldRecord = imageFieldRecord;
                _imageAlt = imageAlt;
            }

            /// <summary>
            /// Charge le contexte (paramètres d'exécution) à partir de l'objet contexte HTTP (valeurs dans Request.Form)
            /// </summary>
            /// <param name="imageType">Type of the image.</param>
            /// <param name="imageField">The image field.</param>
            /// <param name="imageFieldRecord">The image field record.</param>
            /// <param name="imageFieldFileId">The image field file identifier.</param>
            /// <param name="imageFieldDescId">The image field desc identifier.</param>
            public void LoadForDelete(eLibConst.IMAGE_TYPE imageType, Field imageField, eFieldRecord imageFieldRecord, int imageFieldFileId, int imageFieldDescId)
            {
                _action = ImageManagerAction.DELETE;
                _imageFieldFileId = imageFieldFileId;
                _imageFieldDescId = imageFieldDescId;
                _imageType = imageType;
                _computeRealThumbnail = false; // non utilisé pour la suppression
                _imageWidth = 16; // non utilisé pour la suppression
                _imageHeight = 16; // non utilisé pour la suppression
                _imageField = imageField;
                _imageFieldRecord = imageFieldRecord;
            }
        }

        private ImageManagerContext _imageContext = null;

        /// <summary>
        /// Renvoie le nom de l'objet de session dans lequel on stocke l'image envoyée par l'utilisateur lorsque la
        /// fiche est en cours de création (FileId à 0)
        /// </summary>
        private string _tempImageSessionObjName
        {
            get { return String.Concat("TempImageFile_", _pref.UserId, "_", _imageContext.ImageFieldDescId); }
        }


        /// <summary>
        /// Chargement de la page
        /// </summary>
        protected override void ProcessManager()
        {
            Dictionary<string, string> returnValues = new Dictionary<string, string>();

            try
            {
                Process(_pref, _context, out returnValues);
            }
            catch (eEndResponseException) { }
            catch (System.Threading.ThreadAbortException) { }
            catch (Exception exp)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Exception Message : ", exp.Message, Environment.NewLine, "Exception StackTrace :", exp.StackTrace);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)), // Détail : pour améliorer...
                   eResApp.GetRes(_pref, 72),  //   titre
                   String.Concat(sDevMsg));
            }
            finally
            {
                DoResponse(returnValues);
            }
        }

        /// <summary>
        /// Récupère les paramètres d'exécution à partir d'un tableau de clés/valeurs et effectue le traitement
        /// Requiert de passer les préférences car cette méthode est destinée à être appelée hors contexte HTTP
        /// </summary>
        /// <param name="pref">Objet Préférences</param>
        /// <param name="context">Contexte HTTP</param>
        /// <param name="returnValues">The return values.</param>
        /// <returns></returns>
        public bool Process(ePref pref, HttpContext context, out Dictionary<string, string> returnValues)
        {
            returnValues = new Dictionary<string, string>();

            #region Récupération des paramètres

            String error = String.Empty;

            _pref = pref;
            _context = context;
            _imageContext = new ImageManagerContext();
            _imageContext.Load(pref, context, out error);


            _bDragAndDrop = _requestTools.GetRequestFormKeyS("fromDragAndDrop") == "1";
            _bParentIsPopup = _requestTools.GetRequestFormKeyS("parentIsPopup") == "1";
            _bIsUpdateOnBlur = _requestTools.GetRequestFormKeyS("updateOnBlur") == "1";


            if (error.Length > 0)
            {
                returnValues.Add("title", "eImageManager - Erreur lors du chargement du contexte");
                returnValues.Add("msg", error);
                returnValues.Add("detail", error);
                return false;
            }

            #endregion

            else
            {
                return ProcessAction(out returnValues);
            }
        }

        /// <summary>
        /// Récupère les informations demandées à partir des paramètres renseignés dans le contexte (lui-même initialisé par Process)
        /// </summary>
        /// <param name="returnValues">The return values.</param>
        /// <returns>
        /// true si la valeur a pu être récupérée, false sinon
        /// </returns>
        public Boolean ProcessAction(out Dictionary<string, string> returnValues)
        {
            returnValues = new Dictionary<string, string>();

            try
            {
                string strHTMLResponse = String.Empty;
                string strErrorTitle = String.Empty;
                string strErrorMessage = String.Empty;
                string strErrorDebugMessage = String.Empty;


                switch (_imageContext.Action)
                {
                    case ImageManagerAction.UPLOAD:
                    default:
                        string width = String.Empty;
                        string height = String.Empty;

                        if (!_bDragAndDrop)
                            UploadFromContext();
                        else
                            UploadFromDragAndDrop();

                        //Pour les widgets, on recupere les width et height reele de l'image
                        if (_width > 0 || _height > 0)
                        {
                            returnValues.Add("imageWidth", _width.ToString());
                            returnValues.Add("imageHeight", _height.ToString());
                        }

                        returnValues.Add("imageURL", _imageURL);
                        returnValues.Add("imageName", _imageName);
                        returnValues.Add("storedInSession", (_bStoredInSession ? "1" : "0"));
                        returnValues.Add("htmlResponse", strHTMLResponse);
                        break;

                    case ImageManagerAction.DELETE:
                        //Delete(out strHTMLResponse, out strErrorTitle, out strErrorMessage, out strErrorDebugMessage);
                        DeleteFromContext();
                        returnValues.Add("htmlResponse", strHTMLResponse);
                        break;
                }
                if (strErrorTitle.Length > 0 || strErrorMessage.Length > 0 || strErrorDebugMessage.Length > 0)
                {
                    ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, strErrorDebugMessage);
                    returnValues.Add("title", strErrorTitle);
                    returnValues.Add("msg", strErrorMessage);
                    returnValues.Add("detail", strErrorDebugMessage);
                }
            }
            catch (Exception e)
            {
                ErrorContainer = eErrorContainer.GetDevError(eLibConst.MSG_TYPE.CRITICAL, e.ToString());
                if (!returnValues.ContainsKey("title")) returnValues.Add("title", "Exception");
                if (!returnValues.ContainsKey("msg")) returnValues.Add("msg", e.Message);
                if (!returnValues.ContainsKey("detail")) returnValues.Add("detail", e.StackTrace);
            }
            finally
            {

            }

            return returnValues.ContainsKey("msg") == false;
        }

        private void DoResponse(Dictionary<string, string> returnValues)
        {
            XmlDocument xmlDocReturn = new XmlDocument();

            XmlNode xmlNodeEdnResult = xmlDocReturn.CreateElement("ednResult");
            xmlDocReturn.AppendChild(xmlNodeEdnResult);

            XmlNode xmlNodeSuccess = xmlDocReturn.CreateElement("success");
            xmlNodeEdnResult.AppendChild(xmlNodeSuccess);

            XmlNode xmlNode = null;

            #region Gestion d'erreur standard du Manager (ErrorContainer)

            if (ErrorContainer.IsSet)
            {
                xmlNode = xmlDocReturn.CreateElement("error");
                if (ErrorContainer.DebugMsg.Length > 0)
                    xmlNode.InnerText = ErrorContainer.DebugMsg.ToHtml();
                xmlNodeEdnResult.AppendChild(xmlNode);
            }

            #endregion

            #region Valeurs retournées, dont erreurs additionnelles spécifiques si définies

            xmlNode = xmlDocReturn.CreateElement("action");
            xmlNode.InnerText = _imageContext.Action.ToString();
            xmlNodeEdnResult.AppendChild(xmlNode);

            foreach (KeyValuePair<string, string> kvp in returnValues)
            {
                xmlNode = xmlDocReturn.CreateElement(kvp.Key);
                xmlNode.InnerText = kvp.Value;
                xmlNodeEdnResult.AppendChild(xmlNode);
            }

            #endregion

            xmlNodeSuccess.InnerText = (returnValues.ContainsKey("msg") == false ? "1" : "0");

            LaunchError();
            RenderResult(RequestContentType.XML, delegate () { return xmlDocReturn.OuterXml; });
        }



        /// <summary>
        /// Récupération du fichier à uploader depuis le contexte
        /// </summary>
        public void UploadFromContext()
        {
            if (_context != null)
            {
                byte[] data = null;

                eAbstractImage image = eAbstractImage.GetImage(_pref, _imageContext.ImageType, _imageContext.ImageFieldDescId / 100 * 100, _imageContext.ImageFieldFileId, _imageContext.ImageFieldDescId);

                if (_context.Session[_tempImageSessionObjName] != null && _context.Session[_tempImageSessionObjName] is byte[])
                {
                    String strFileName = string.Empty;
                    String strContentType = string.Empty;

                    int nFileLen = ((byte[])_context.Session[_tempImageSessionObjName]).Length;

                    if (_context.Session[String.Concat(_tempImageSessionObjName, "_FileName")] != null)
                        strFileName = _context.Session[String.Concat(_tempImageSessionObjName, "_FileName")].ToString();
                    // Au cas improbable où la variable de session ne serait pas définie, on donne un nom par défaut au fichier
                    else
                        strFileName = String.Concat(_tempImageSessionObjName, ".jpg");

                    if (_context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")] != null)
                    {
                        strContentType = _context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")].ToString();
                    }

                    image.SetFileInfos(strFileName, nFileLen, strContentType);
                }

                if (image.StorageType != ImageStorage.STORE_IN_URL)
                {

                    data = (byte[])_context.Session[_tempImageSessionObjName];

                    image.Save(data);

                    if (image is eFieldImage)
                        _imageURL = ((eFieldImage)image).GetImageURL();
                }
                else
                {
                    ((eImageFieldURL)image).StoreInURL(_imageContext.PostedImageURL);
                    _imageURL = _imageContext.PostedImageURL;
                }

                _imageName = image.FileName;

                SetEmptyContextFile();
            }
        }

        /// <summary>
        /// Uploads from drag and drop.
        /// </summary>
        public Boolean UploadFromDragAndDrop()
        {
            byte[] data = null;
            eAbstractImage image = null;

            if (_imageContext != null)
            {
                if (_imageContext.ImageFile != null)
                {
                    image = eAbstractImage.GetImage(_pref, _imageContext.ImageType, _imageContext.ImageFieldDescId / 100 * 100, _imageContext.ImageFieldFileId, _imageContext.ImageFieldDescId);
                    if (image.StorageType != ImageStorage.STORE_IN_URL)
                    {
                        data = image.GetPostedFileData(_imageContext.ImageFile);

                        bool saveInSession = false;
                        if (_imageContext.ImageFieldFileId <= 0 || (_bParentIsPopup && !_bIsUpdateOnBlur))
                            saveInSession = true;

                        if (saveInSession)
                        {
                            SetContextFile(data, image.FileName, image.ContentType);
                            _bStoredInSession = true;
                        }
                        else
                        {
                            if (!image.Save(data))
                            {
                                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 1760), image.UserError, "", image.DebugError);
                                LaunchError();
                                return false;
                            }
                        }

                        if (image is eFieldImage)
                            _imageURL = ((eFieldImage)image).GetImageURL(forceFromSession: saveInSession);
                        _imageName = image.FileName;
                        _width = image.Width;
                        _height = image.Height;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Suppression du fichier en session
        /// </summary>
        public void DeleteFromContext()
        {
            if (_context != null)
            {
                SetEmptyContextFile();
            }
        }

        /// <summary>
        /// Vide l'image stockée en session
        /// </summary>
        public void SetEmptyContextFile()
        {
            _context.Session[_tempImageSessionObjName] = null;
            _context.Session[String.Concat(_tempImageSessionObjName, "_FileName")] = String.Empty;
            _context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")] = String.Empty;

            _imageName = string.Empty;
        }

        /// <summary>
        /// Enregistre l'image en session
        /// </summary>
        /// <param name="obj">Fichier</param>
        /// <param name="filename">Nom du fichier</param>
        /// <param name="contentType">Type de fichier</param>
        public void SetContextFile(object obj, String filename, String contentType)
        {
            _context.Session[_tempImageSessionObjName] = obj;
            _context.Session[String.Concat(_tempImageSessionObjName, "_FileName")] = filename;
            _context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")] = contentType;

            _imageName = filename;
        }
    }
}