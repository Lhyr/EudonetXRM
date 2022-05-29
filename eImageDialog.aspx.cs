using Com.Eudonet.Internal;
using EudoQuery;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Classe représentant la fenêtre d'ajout d'image
    /// </summary>
    public partial class eImageDialog : eEudoPage
    {
        /// <summary>Contenu de l'input File</summary>
        protected HtmlInputFile filMyFile;

        /// <summary>
        /// Nom de l'objet JavaScript représentant la fenêtre modale
        /// </summary>
        protected string _modalVarName = String.Empty;

        /// <summary>
        /// Indique, pour les cas où une icône "Il y a une image !" doit être renvoyée (mode Liste par ex.),
        /// si l'image à renvoyer doit être une miniature calculée dynamiquement à partir de l'image réelle
        /// Cela nécessitera un temps de traitement supplémentaire (à évaluer sur un gros mode liste)
        /// </summary>
        protected bool _computeRealThumbnail = false;

        /// <summary>
        /// Type d'image à traiter (Google Image, Avatar, E-mailing...)
        /// </summary>
        protected eLibConst.IMAGE_TYPE _imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;

        /// <summary>
        /// Table de l'image
        /// </summary>
        protected int _imageTab = 0;

        /// <summary>
        /// Pour le traitement des images de type Champ Image
        /// DescID du champ à traiter (pour instancier un eFieldRecord)
        /// </summary>
        protected int _imageFieldDescId = 0;

        /// <summary>
        /// Pour le traitement des images de type Champ Image
        /// FileID de la fiche à traiter (pour instancier un eFieldRecord)
        /// </summary>
        protected int _imageFieldFileId = 0;

        /// <summary>
        /// Image de type Annexe (PJ) - Id
        /// </summary>
        protected int _pjId = 0;

        /// <summary>
        /// Image de type Annexe (PJ) - Type d'annexe
        /// </summary>
        protected int _pjType = 0;


        /// <summary>
        /// Largeur désirée de l'image à renvoyer
        /// </summary>
        protected int _imageWidth = 0;

        /// <summary>
        /// Hauteur désirée de l'image à renvoyer
        /// </summary>
        protected int _imageHeight = 0;

        /// <summary>Texte alternatif de l'image</summary>
        protected String _imageAlt = String.Empty;

        /// <summary>
        /// Largeur désirée de la fenêtre
        /// </summary>
        protected int _width = 0;

        /// <summary>
        /// Hauteur désirée de la fenêtre
        /// </summary>
        protected int _height = 0;

        protected bool isB64 = false;
        protected string sVal64 = "";

        /// <summary>
        /// Indique si la fiche appelante est en mode popup
        /// </summary>
        private bool _parentIsPopup = false;

        /// <summary>
        /// Indique si la fiche appelante sauvegarde automatiquement les changements (normalement true, sauf pour les templates en popup avec autosave désactivé
        /// </summary>
        private bool _updateOnBlur = true;

        /// <summary>
        /// Indique si on sauvegarde l'image en session
        /// </summary>
        private bool _saveInSession = false;



        /// <summary>
        /// type d'editeur d'ou provient la demande d'image  (si depuis memo-editeur)
        /// </summary>
        string _sFrom = "";

        /// <summary>
        /// Code JavaScript à insérer dans le corps de la page
        /// Permet notamment de déclencher le chargement de l'image par JavaScript et de surveiller son chargement en vue de redimensionner la fenêtre
        /// une fois que l'image a été chargée
        /// </summary>
        protected string _bodyJavaScript = String.Empty;

        /// <summary>
        /// Renvoie le nom de l'objet de session dans lequel on stocke l'image envoyée par l'utilisateur lorsque la
        /// fiche est en cours de création (FileId à 0)
        /// </summary>
        private string _tempImageSessionObjName
        {
            get { return String.Concat("TempImageFile_", _pref.UserId, "_", _imageFieldDescId); }
        }

        /// <summary>
        /// Avatar PP/PM avant nouvelle mise en page
        /// </summary>
        private Boolean _oldAvatarField = false;

        /// <summary>
        /// Type d'image à traiter (Google Image, Avatar, E-mailing...)
        /// </summary>
        public eLibConst.IMAGE_TYPE ImageType
        {
            get { return _imageType; }
            set { _imageType = value; }
        }

        /// <summary>
        /// Appelée au chargement de la page !
        /// </summary>
        /// <param name="e"></param>
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Chargement des méthodes et nom des composants et des res
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdSend.Click += new System.EventHandler(this.cmdSend_Click);
            this.cmdDelete.Click += new System.EventHandler(this.cmdDelete_Click);
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return scriptHolder;
        }

        /// <summary>
        /// Initialisation de la page : affichage de la fenêtre d'ajout d'image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Ajout des css
            PageRegisters.AddCss("eModalDialog");
            PageRegisters.AddCss("eMain");
            PageRegisters.AddCss("eImage");
            PageRegisters.AddCss("eButtons");

            #endregion


            #region ajout des js
            PageRegisters.AddScript("eTools");
            PageRegisters.AddScript("eMain");
            PageRegisters.AddScript("eModalDialog");
            PageRegisters.AddScript("eImageManager");
            #endregion

            #region Initialisation des variables avec les informations passées dans l'objet Request

            _sFrom = _requestTools.GetRequestFormKeyS("CalledFrom") ?? "";
            CalledFrom.Value = _sFrom;

            if (Request.Form["ImageURL"] != null)
            {
                imageURL.Value = Request.Form["ImageURL"].ToString();
            }

            if (Request.Form["ImageType"] != null)
            {
                imageType.Value = Request.Form["ImageType"].ToString();

                if (imageType.Value == "OLD_AVATAR_FIELD")
                    _oldAvatarField = true;

                _imageType = eLibTools.GetImageTypeFromString(Request.Form["ImageType"].ToString());
            }

            if (Request.Form["DescId"] != null)
            {
                descId.Value = Request.Form["DescId"].ToString();

                int.TryParse(Request.Form["DescId"].ToString(), out _imageFieldDescId);

                _imageTab = _imageFieldDescId / 100 * 100;
            }
            if (Request.Form["FileId"] != null)
            {
                fileId.Value = Request.Form["FileId"].ToString();

                int.TryParse(Request.Form["FileId"].ToString(), out _imageFieldFileId);
            }
            if (Request.Form["PjId"] != null)
            {
                pjId.Value = Request.Form["PjId"].ToString();

                int.TryParse(Request.Form["PjId"].ToString(), out _pjId);
            }
            if (Request.Form["PjType"] != null)
            {
                pjType.Value = Request.Form["PjType"].ToString();

                int.TryParse(Request.Form["PjType"].ToString(), out _pjType);
            }

            // Largeur de la fenêtre
            if (Request.Form["width"] != null)
            {
                width.Value = Request.Form["width"].ToString();

                int.TryParse(Request.Form["width"].ToString(), out _width);
            }
            // Hauteur de la fenêtre
            if (Request.Form["height"] != null)
            {
                height.Value = Request.Form["height"].ToString();

                int.TryParse(Request.Form["height"].ToString(), out _height);
            }
            // Largeur de l'image
            if (Request.Form["ImageWidth"] != null)
            {
                imageWidth.Value = Request.Form["ImageWidth"].ToString();

                int.TryParse(Request.Form["ImageWidth"].ToString(), out _imageWidth);
            }
            // Hauteur de l'image
            if (Request.Form["ImageHeight"] != null)
            {
                imageHeight.Value = Request.Form["ImageHeight"].ToString();

                int.TryParse(Request.Form["ImageHeight"].ToString(), out _imageHeight);
            }

            if (Request.Form["modalVarName"] != null)
            {
                modalVarName.Value = Request.Form["modalVarName"].ToString();

                _modalVarName = Request.Form["modalVarName"].ToString();
            }


            isB64 = _requestTools.GetRequestFormKeyB("isb64") ?? false;
            if (isB64)
            {
                sVal64 = _requestTools.GetRequestFormKeyS("b64val").Replace("data:image/png;base64,", "");
            }

            _imageAlt = textImageAlt.Text;

            // Appelé depuis une popup
            if (Request.Form["parentIsPopup"] != null)
            {
                parentIsPopup.Value = Request.Form["parentIsPopup"].ToString();

                _parentIsPopup = Request.Form["parentIsPopup"].ToString() == "1" ? true : false;
            }

            // Met à jour automatiquement le champ
            if (Request.Form["updateOnBlur"] != null)
            {
                updateOnBlur.Value = Request.Form["updateOnBlur"].ToString();

                _updateOnBlur = Request.Form["updateOnBlur"].ToString() == "0" ? false : true;
            }

            #endregion

            try
            {
                #region Validation de l'existence d'une image à charger

                bool bImageExists = true; // indique s'il y a une image existante à charger via JavaScript

                eImageField image = null;

                if (_imageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD)
                {
                    image = eImageField.GetImageField(_pref, _imageTab, _imageFieldDescId, _imageFieldFileId);

                    #region Récupération de l'image existante si FileId > 0
                    if (_imageFieldFileId > 0)
                    {

                        if (image.StorageType == ImageStorage.STORE_IN_URL)
                        {
                            // Affectation au champ URL
                            if (!Page.IsPostBack)
                                imageURL.Value = image.DbValue.ToString();
                        }

                        try
                        {
                            bImageExists = image.ImageExists(image.DbValue);
                        }
                        catch
                        {
                            bImageExists = false;
                        }
                    }
                    #endregion
                    #region Si FileId < 0, on renvoie l'objet image stocké temporairement en session s'il existe
                    else
                    {

                        if (isB64)
                        {
                            imageURL.Value = sVal64;
                            imageURL.Attributes.Add("isb64", "1");

                        }
                        else
                        {
                            bImageExists = Session[_tempImageSessionObjName] != null;

                            // En mode stockage par URL, il faut remplir le champ de saisie avec la valeur actuelle s'il n'a pas déjà rempli par l'utilisateur (formulaire soumis)
                            if (!Page.IsPostBack && image.StorageType == ImageStorage.STORE_IN_URL && Session[_tempImageSessionObjName] is Uri)
                                imageURL.Value = ((Uri)Session[_tempImageSessionObjName]).ToString();
                        }
                    }
                    #endregion
                }

                #endregion

                #region Masquage des contrôles de la fenêtre en fonction du mode de stockage
                bool bUploadMode = true; // si false : ajout par URL
                switch (_imageType)
                {
                    case eLibConst.IMAGE_TYPE.AVATAR_FIELD:
                    case eLibConst.IMAGE_TYPE.AVATAR:
                    case eLibConst.IMAGE_TYPE.USER_AVATAR_FIELD:
                    case eLibConst.IMAGE_TYPE.LOGO:
                        bUploadMode = true;
                        break;
                    case eLibConst.IMAGE_TYPE.MEMO:
                    case eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL:
                        PanelImgAlt.Visible = true;
                        bUploadMode = true;
                        // lorsqu'elle est appelée depuis le champ Mémo, la fenêtre d'ajout d'image ne sert qu'à ajouter une nouvelle image, et non à modifier une image existante.
                        // Si on est amenés à utiliser cette fenêtre pour modifier une image du champ Mémo, il faudra alors faire les adaptations nécessaires pour charger l'image
                        // via eImage.aspx par exemple, puis laisser ce booléen à true pour déclencher son chargement (comme cela est fait pour IMAGE_FIELD ci-dessus)
                        bImageExists = false;
                        break;
                    case eLibConst.IMAGE_TYPE.URL:
                        bUploadMode = false;
                        break;
                    case eLibConst.IMAGE_TYPE.TXT_URL:
                        bUploadMode = true;
                        bImageExists = false;
                        break;
                    case eLibConst.IMAGE_TYPE.IMAGE_FIELD:
                    default:
                        if (image != null)
                        {
                            switch (image.StorageType)
                            {
                                case ImageStorage.STORE_IN_URL:
                                    bUploadMode = false;
                                    break;
                                case ImageStorage.STORE_IN_DATABASE:
                                case ImageStorage.STORE_IN_FILE:
                                default:
                                    bUploadMode = true;
                                    break;
                            }
                        }
                        else
                            bUploadMode = false;
                        break;
                }

                string strCallbackFunction = String.Empty;


                #region JS à exécuter
                bool bViewOnlyMode = _imageType == eLibConst.IMAGE_TYPE.URL;

                StringBuilder sb = new StringBuilder();

                if (bUploadMode)
                {
                    if (sVal64.Length == 0)
                        sb.AppendLine("document.getElementById('imageURL').style.display = 'none';");
                }
                else if (!bViewOnlyMode)
                {
                    sb.AppendLine("document.getElementById('filMyFile').style.display = 'none';");
                    sb.AppendLine("document.getElementById('imageURL').style.display = 'block';");
                    sb.AppendLine("document.getElementById('lblFile').innerHTML = '" + eResApp.GetRes(_pref, 712) + " (" + eResApp.GetRes(_pref, 6313).Replace("'", @"\'") + ")';");
                }

                // Si la fenêtre est utilisée pour visualiser une PJ, on masque tous les contrôles de mise à jour
                if (bViewOnlyMode)
                {

                    sb.AppendLine("document.getElementById('filMyFile').style.display = 'none';");
                    sb.AppendLine("document.getElementById('imageURL').style.display = 'none';");
                    sb.AppendLine("document.getElementById('lblFile').style.display = 'none';");
                    sb.AppendLine("document.getElementById('lblFormat').style.display = 'none';");
                    //sb.AppendLine("top.document.getElementById('btnSend').style.display = 'none';");
                    //sb.AppendLine("top.document.getElementById('btnDelete').style.display = 'none';");
                    //sb.AppendLine("top.document.getElementById('btnCancel').style.display = 'none';");
                    sb.AppendLine("top.setAttributeValue(document.getElementById('imgHolder'), 'viewonlymode', '1');");
                }
                #endregion

                #region Chargement de l'image via JavaScript, en passant le relais à eImage.aspx
                // IE 8 n'arrive pas à charger la fenêtre avec eTools à temps pour faire fonctionner les scripts de chargement d'image.
                // Il faut donc se servir de eTools instancié sur la page parente (eMain) pour ajouter le script sur la page actuelle,
                // et indiquer à addScript de déclencher les scripts une fois le fichier chargé...
                // On passe à loadImage une URL pointant vers eImage.aspx qui se chargera de rediriger vers la vraie image s'il n'y a pas de retraitement à faire
                // avec un timestamp pour court-circuiter le cache

                if (bImageExists)
                {
                    // Le FileId, s'il est à 0 (cas de l'ajout d'image depuis une nouvelle fiche) est passé à -1 afin d'indiquer à eImage.aspx d'aller chercher l'image temporaire stockée en variable de session, sauf pour l'affichage d'images à partir d'une URL (visualisation d'annexes)
                    int nImageFieldFileId = _imageFieldFileId;
                    if (nImageFieldFileId == 0 && bImageExists && _imageType != eLibConst.IMAGE_TYPE.URL)
                        nImageFieldFileId = -1;
                    {

                        strCallbackFunction = String.Concat("function () { loadImage('eImage.aspx?did=", _imageFieldDescId, "&fid=", nImageFieldFileId, "&pjid=", _pjId, "&pjt=", _pjType, "&it=", _imageType.ToString(), "&ts=", DateTime.Now.ToString("yyyyMMddHHmmssfff"), "');", "},");
                    }
                }
                // Si aucune image n'est à charger, on rend le conteneur invisible, afin de ne pas déclencher les JS de chargement d'images inutilement,
                // et on masque le bouton Supprimer
                else
                {
                    imgHolder.Visible = false;
                    imgPreview.Visible = false;
                    // #32 132 - On redimensionne la fenêtre à sa dimension d'origine, hors marge réservée à un éventuel affichage de l'image "Introuvable"
                    sb.AppendLine("if (top.modalImage && top.modalImage.initialWindowWidth && top.modalImage.initialWindowHeight) { top.modalImage.resizeTo(top.modalImage.initialWindowWidth, top.modalImage.initialWindowHeight); }");

                    // On masque le bouton Supprimer
                    cmdDelete.Visible = false;
                    //sb.AppendLine("top.document.getElementById('btnDelete').style.display = 'none';");
                    // Et le libellé du bouton de validation devient "Envoyer"
                    cmdSend.Text = eResApp.GetRes(_pref, 944);
                }
                #endregion
                #endregion


                #region Renvoi du JavaScript généré au navigateur
                sb.AppendLine("top.addScript('eTools', 'IMAGEDIALOG', " + strCallbackFunction + "window.document);");

                _bodyJavaScript = sb.ToString();
                #endregion
            }
            catch (eEndResponseException) { Response.End(); }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception exc)
            {
                ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), "", exc.Message, exc.StackTrace);
                LaunchErrorHTML(true, ErrorContainer);
            }


        }

        /// <summary>
        /// Action à la validation, permettant l'ajout de l'image sélectionnée
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdSend_Click(object sender, System.EventArgs e)
        {
            Boolean storedInSession = false;
            String imagePostedURL = imageURL.Value;

            if (_imageFieldFileId <= 0 || (_parentIsPopup && !_updateOnBlur))
                _saveInSession = true;

            try
            {
                String script = string.Empty;
                ClientScriptManager cs = Page.ClientScript;

                byte[] data = null;

                
                eAbstractImage image = eAbstractImage.GetImage(_pref, _imageType, _imageTab, _imageFieldFileId, _imageFieldDescId, _sFrom);
                image.Delete(); //ALISTER => Demande 67 887

                // Récupération de l'image uploadée...
                if ((image.StorageType != ImageStorage.STORE_IN_URL))
                    data = image.GetPostedFileData(filMyFile.PostedFile);

                if ((data != null && data.Length > 0) || image.StorageType == ImageStorage.STORE_IN_URL)
                {
                    if (_imageType == eLibConst.IMAGE_TYPE.MEMO || _imageType == eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL || _imageType == eLibConst.IMAGE_TYPE.TXT_URL)
                    {
                        //eMemoImage memoImg = (eMemoImage)image;
                        Boolean uploadOk = image.Save(data);
                        imagePostedURL = image.ImageURL;

                        if (!uploadOk)
                            LaunchImageDialogError(eResApp.GetRes(_pref, 6237), image.UserError, image.DebugError, "", true);
                    }
                    else if (_saveInSession && _imageType != eLibConst.IMAGE_TYPE.LOGO)
                    {
                        #region Cas création mode popup

                        // On stocke le fichier en session
                        if (image.StorageType != ImageStorage.STORE_IN_URL)
                        {
                            image.FileName = filMyFile.PostedFile.FileName;
                            // On appelle la méthode "RenameFile()" pour récupérer le nom du fichier final
                            //image.RenameFile();

                            //KHA le 20/02/2019 IE et Edge renvoient le chemin local de l'image, on ne veut que le nom du fichier
                            if (image.FileName.Contains(@"\"))
                            {
                                string[] sFileName = image.FileName.Split('\\');
                                image.FileName = sFileName[sFileName.Length - 1];
                            }


                            SetContextFile(data, image.FileName, filMyFile.PostedFile.ContentType);
                            if (image is eFieldImage)
                            {
                                if (_imageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD)
                                {
                                    _imageWidth = 45;
                                    _imageHeight = 45;
                                }
                                imagePostedURL = ((eFieldImage)image).GetImageURL(_computeRealThumbnail, _imageWidth, _imageHeight, true);
                            }

                        }
                        else
                        {
                            SetContextFile(new Uri(imagePostedURL), Path.GetFileName(imagePostedURL), String.Concat("image/", Path.GetExtension(imagePostedURL).Replace(".", String.Empty)).Replace("jpg", "jpeg"));
                        }

                        storedInSession = true;

                        #endregion

                    }
                    else
                    {
                        #region Cas de la mise à jour standard

                        Boolean uploadOk = false;

                        if (_imageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD)
                        {

                            eImageField fieldImage = (eImageField)image;


                            if (image.StorageType != ImageStorage.STORE_IN_URL)
                            {
                                uploadOk = fieldImage.Save(data);
                                if (image.StorageType == ImageStorage.STORE_IN_DATABASE)
                                {
                                    imagePostedURL = fieldImage.GetImageURL(_computeRealThumbnail, _width, _height);
                                }
                                else
                                {
                                    imagePostedURL = fieldImage.ImageURL;
                                }
                            }
                            else
                            {
                                uploadOk = fieldImage.StoreInURL(imageURL.Value);
                            }
                        }
                        else
                        {
                            uploadOk = image.Save(data);

                            imagePostedURL = image.ImageURL;

                        }


                        if (!uploadOk)
                            LaunchImageDialogError(eResApp.GetRes(_pref, 6237), image.UserError, image.DebugError, "", true);


                        #endregion
                    }
                }


                if (_imageType == eLibConst.IMAGE_TYPE.LOGO)
                {
                    script = "oImageManager.refreshLogo(top.modalImage, '" + imagePostedURL + "');";
                }
                else if (_imageType == eLibConst.IMAGE_TYPE.AVATAR)
                {
                    script = "oImageManager.refreshUserAvatar(top.modalImage, '" + imagePostedURL + "');";
                }
                else if (_imageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD && _oldAvatarField)
                {
                    script = "oImageManager.refreshAvatarField(top.modalImage, '" + imagePostedURL + "', " + (storedInSession ? "true" : "false") + ");";
                }
                else if (_imageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD || _imageType == eLibConst.IMAGE_TYPE.USER_AVATAR_FIELD || _imageType == eLibConst.IMAGE_TYPE.MEMO || _imageType == eLibConst.IMAGE_TYPE.MEMO_SETDIALOGURL || _imageType == eLibConst.IMAGE_TYPE.TXT_URL || _imageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD)
                {
                    //BSE:#59 743
                    if (_imageType != eLibConst.IMAGE_TYPE.MEMO)
                    {
                    // US #1904 - Tâche #2753 - On échappe les apostrophes uniquement si ça n'a pas déjà été fait par un autre traitement en amont
                    if (imagePostedURL.Contains("'") && !imagePostedURL.Contains(@"\'"))
                        imagePostedURL = imagePostedURL.Replace("'", @"\'");
                    }
                    else
                        imagePostedURL = string.IsNullOrEmpty(image.ImageWebURL) ? imagePostedURL : image.ImageWebURL;

                    script = string.Concat(
                                "top.onImageSubmit(",
                                        "true,",
                                        storedInSession ? "true" : "false", ", ",
                                        "'", _imageType, "', ",
                                        "'", imagePostedURL, "', ",
                                        ((image.StorageType == ImageStorage.STORE_IN_URL) || (image.StorageType == ImageStorage.STORE_IN_FILE) ? String.Concat("'", image.FileName.Replace("'", @"\'"), "'") : "null"), ", ",
                                        _imageWidth, ", ",
                                        _imageHeight, ", ",
                                        "'", _imageAlt.Replace("'", @"\'"), "'",
                                    ");"
                                    );
                }

                if (!String.IsNullOrEmpty(script))
                    cs.RegisterStartupScript(this.GetType(), "sendCallback", script, true);


            }
            catch (eEndResponseException) { Response.End(); }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception exc)
            {
                LaunchImageDialogError(eResApp.GetRes(_pref, 6237), exc.Message, exc.StackTrace, "");
            }


        }

        /// <summary>
        /// Action au clic sur le bouton de suppression, permettant de supprimer l'image existante
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdDelete_Click(object sender, System.EventArgs e)
        {
            try
            {
                ClientScriptManager cs = Page.ClientScript;
                String script = string.Empty;
                String imageURL = string.Empty;

                if (_imageFieldFileId > 0 || _imageType == eLibConst.IMAGE_TYPE.LOGO)
                {
                    eAbstractImage img = eAbstractImage.GetImage(_pref, _imageType, _imageTab, _imageFieldFileId, _imageFieldDescId);
                    if (!img.Delete())
                    {
                        ErrorContainer = eErrorContainer.GetDevUserError(eLibConst.MSG_TYPE.CRITICAL, eResApp.GetRes(_pref, 6237), img.UserError, "", img.DebugError);
                        LaunchErrorHTML(true, ErrorContainer);
                    }
                    else
                    {
                        imageURL = img.ImageURL;

                    }
                }
                else
                {
                    #region Suppression en session
                    SetEmptyContextFile();
                    #endregion

                }

                if (_imageType == eLibConst.IMAGE_TYPE.LOGO)
                {
                    script = "oImageManager.refreshLogo(top.modalImage, '" + imageURL + "');";
                }
                else if (_imageType == eLibConst.IMAGE_TYPE.AVATAR)
                {
                    script = "oImageManager.refreshUserAvatar(top.modalImage, '" + imageURL + "');";
                }
                else if (_imageType == eLibConst.IMAGE_TYPE.AVATAR_FIELD && _oldAvatarField)
                {
                    script = "oImageManager.refreshAvatarField(top.modalImage, '" + imageURL + "');";
                }
                else
                {
                    // US #1904 - Tâche #2753 - On échappe les apostrophes uniquement si ça n'a pas déjà été fait par un autre traitement en amont
                    if (imageURL.Contains("'") && !imageURL.Contains(@"\'"))
                        imageURL = imageURL.Replace("'", @"\'");

                    script = string.Concat(
                  "top.onImageSubmit(",
                          "true, ",
                          "false, ",
                          "'", _imageType, "', ",
                          "'", imageURL, "', ",
                          "null, ",
                          _imageWidth, ", ",
                          _imageHeight, ", ",
                          "'", _imageAlt.Replace("'", @"\'"), "', ",
                            "true, ",
                            eConst.FILE_LINE_HEIGHT.ToString(),
                      ");"
                      );
                }

                if (!String.IsNullOrEmpty(script))
                    cs.RegisterStartupScript(this.GetType(), "sendCallback", script, true);

            }
            catch (eEndResponseException) { Response.End(); }
            catch (System.Threading.ThreadAbortException)
            {
            }
            catch (Exception exc)
            {
                LaunchImageDialogError(eResApp.GetRes(_pref, 845), exc.Message, exc.StackTrace, String.Empty);
            }
        }

        /// <summary>
        /// Launches the image dialog error.
        /// </summary>
        /// <param name="reason">Message dév</param>
        /// <param name="message">Message client</param>
        /// <param name="stackTrace">Stacktrace</param>
        /// <param name="afterLaunch">JS à exécuter après l'alerte</param>
        /// <param name="isCleintMessage">True si message client</param>
        private void LaunchImageDialogError(string reason, string message, string stackTrace, string afterLaunch, bool isCleintMessage = false)
        {
            if (String.IsNullOrEmpty(afterLaunch))
                afterLaunch = "top.setWait(false);top.window['modalImage'].hide();";

            //Avec exception
            String sDevMsg = String.Concat("Erreur sur eImageDialog.aspx");
            if (!String.IsNullOrEmpty(reason))
                sDevMsg = String.Concat(sDevMsg, " : ", reason);
            sDevMsg = String.Concat(
                sDevMsg, Environment.NewLine,
                "Exception MSG ", message, Environment.NewLine,
                "Exception StackTrace ", stackTrace
            );

            ErrorContainer = eErrorContainer.GetDevUserError(
                eLibConst.MSG_TYPE.CRITICAL,
                eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                                             //String.Concat(eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  
                String.Concat(isCleintMessage ? message : eResApp.GetRes(_pref, 422), "<br>", eResApp.GetRes(_pref, 544)),  // Détail : pour améliorer...
                eResApp.GetRes(_pref, 72),  // Titre
                String.Concat(sDevMsg)
            );

            if (_bFromeUpdater)
                LaunchError();
            else
            {
                LaunchErrorHTML(true, null, afterLaunch);
            }
        }

        private void SetEmptyContextFile()
        {
            this.Context.Session[_tempImageSessionObjName] = null;
            this.Context.Session[String.Concat(_tempImageSessionObjName, "_FileName")] = String.Empty;
            this.Context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")] = String.Empty;
        }

        private void SetContextFile(object obj, String filename, String contentType)
        {
            this.Context.Session[_tempImageSessionObjName] = obj;
            this.Context.Session[String.Concat(_tempImageSessionObjName, "_FileName")] = filename;
            this.Context.Session[String.Concat(_tempImageSessionObjName, "_ContentType")] = contentType;
        }
    }
}