using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using Com.Eudonet.Internal;
using EudoQuery;
using Com.Eudonet.Core.Model;

namespace Com.Eudonet.Xrm
{
    /// <summary>
    /// Page permettant de renvoyer l'image contenue dans un champ Eudonet, avec possibilité de la redimensionner à la génération pour optimiser le rendu et la bande passante
    /// </summary>
    public partial class eImage : eEudoPage
    {
        /// <summary>
        /// Renvoie le nom de l'objet de session dans lequel on stocke l'image envoyée par l'utilisateur lorsque la
        /// fiche est en cours de création (FileId à 0)
        /// </summary>
        private string TempImageSessionObjName
        {
            get
            {
                if (_requestTools.AllKeysQS.Contains("did") && !String.IsNullOrEmpty(Request.QueryString["did"]))
                {
                    return String.Concat("TempImageFile_", _pref.UserId, "_", Request.QueryString["did"].ToString());
                }
                else
                {
                    // Ce cas ne devrait jamais se présenter. Si pas de DescId envoyé à la page : pas d'accès possible à l'image
                    return String.Concat("TempImageFile_", _pref.UserId, "_0");
                }
            }
        }

        /// <summary>
        /// Retourne le PlaceHolder de l'entête de la page pour placer les CSS et Script de celle-ci
        /// </summary>
        /// <returns>Retroune le PlaceHolder</returns>
        public override Control GetHeadPlaceHolder()
        {
            return null;
        }

        /// <summary>
        /// Page permettant de renvoyer l'image contenue dans un champ Eudonet, avec possibilité de la redimensionner à la génération pour optimiser le rendu et la bande passante
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            String strContentType = "image/jpeg";
            ImageFormat imageFormat = ImageFormat.Jpeg;

            #region Récupération et vérification des paramètres passés à la page

            string strImageURL = String.Empty;
            Int32 nFileId = 0;
            Int32 nDescId = 0;
            Int32 nPJId = 0;
            Int32 nPJType = 0;
            Int32 nTab = 0;
            Int32 nWidth = 0;
            Int32 nHeight = 0;
            bool bForceFromSession = false;
            // ASY : pour la duplication unitaire ,on a besoin du fileId de la fiche source pour récupérer son image
            Int32 nFileId0 = 0;
            eLibConst.FOLDER_TYPE targetFolderType = eLibConst.FOLDER_TYPE.FILES;
            eLibConst.IMAGE_TYPE imageType = eLibConst.IMAGE_TYPE.IMAGE_FIELD;

            bool bGenericImageOnly = false;
            // FileId de la fiche concernée
            if (_requestTools.AllKeysQS.Contains("fid") && !String.IsNullOrEmpty(Request.QueryString["fid"]))
                Int32.TryParse(Request.QueryString["fid"].ToString(), out nFileId);
            // ASY : pour la duplication unitaire, on a besoin du fileId de la fiche source pour récupérer son image
            if (_requestTools.AllKeysQS.Contains("fid0") && !String.IsNullOrEmpty(Request.QueryString["fid0"]))
                Int32.TryParse(Request.QueryString["fid0"].ToString(), out nFileId0);
            // DescId du champ à afficher
            if (_requestTools.AllKeysQS.Contains("did") && !String.IsNullOrEmpty(Request.QueryString["did"]))
                Int32.TryParse(Request.QueryString["did"].ToString(), out nDescId);
            // PJId de l'image (si on doit afficher une image issue d'Annexes)
            if (_requestTools.AllKeysQS.Contains("pjid") && !String.IsNullOrEmpty(Request.QueryString["pjid"]))
                Int32.TryParse(Request.QueryString["pjid"].ToString(), out nPJId);
            // Type d'annexe (si applicable)
            if (_requestTools.AllKeysQS.Contains("pjt") && !String.IsNullOrEmpty(Request.QueryString["pjt"]))
                Int32.TryParse(Request.QueryString["pjt"].ToString(), out nPJType);
            // Largeur de l'image
            if (_requestTools.AllKeysQS.Contains("w") && !String.IsNullOrEmpty(Request.QueryString["w"]))
                Int32.TryParse(Request.QueryString["w"].ToString(), out nWidth);
            // Hauteur de l'image
            if (_requestTools.AllKeysQS.Contains("h") && !String.IsNullOrEmpty(Request.QueryString["h"]))
                Int32.TryParse(Request.QueryString["h"].ToString(), out nHeight);
            // Forcer la récupération de l'image en session
            if (_requestTools.AllKeysQS.Contains("ffs") && !String.IsNullOrEmpty(Request.QueryString["ffs"]))
                bForceFromSession = Request.QueryString["ffs"].ToString() == "1";

            if (nWidth == 0)
                nWidth = nHeight;
            if (nHeight == 0)
                nHeight = nWidth;

            // Type d'image à traiter
            if (_requestTools.AllKeysQS.Contains("it") && !String.IsNullOrEmpty(Request.QueryString["it"]))
                imageType = eLibTools.GetImageTypeFromString(Request.QueryString["it"]);
            // Dossier de stockage de l'image en fonction de son type
            targetFolderType = eLibTools.GetFolderTypeFromImageType(imageType);
            // Indique si l'on doit renvoyer une image "générique" si une image est réellement présente en base
            // Evite de générer de multiples images dans certains contextes (ex : mode Liste), ce qui sollicite davantage le serveur
            // L'image réelle devra alors être affichée dans une fenêtre séparée gérée par le script appelant
            if (_requestTools.AllKeysQS.Contains("g") && !String.IsNullOrEmpty(Request.QueryString["g"]))
                bGenericImageOnly = Request.QueryString["g"].ToString().Equals("1");

            if (imageType == eLibConst.IMAGE_TYPE.IMAGE_FIELD)
            {
                // ASY : pour la duplication unitaire, on a besoin du fileId de la fiche source pour récupérer son image
                // Ce cas n'est pas celui de l'affichage d'une image ajoutée depuis une fiche en cours de création, où
                // eImageDialog.aspx passe le FileId à -1 pour faire la distinction avec ce cas
                if ((nFileId == 0 || nDescId == 0) && (nFileId0 == 0))
                {
                    strImageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
                    Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                    Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
                    Response.AppendHeader("Expires", "0"); // Proxies.
                    Response.Redirect(strImageURL);
                }
                nTab = eLibTools.GetNum((nDescId / 100).ToString()) * 100;
            }

            #endregion

            #region Génération de l'image

            string strError = String.Empty;
            bool bCanAccessImage = false;
            System.Drawing.Image bmpImageBitmap = null;
            eAbstractImage image;

            switch (imageType)
            {

                case eLibConst.IMAGE_TYPE.LOGO:
                    bCanAccessImage = true;
                    image = eAppLogo.GetAppLogo(_pref);
                    strImageURL = image.Load().ToString();
                    break;
                case eLibConst.IMAGE_TYPE.URL:
                    // Recherche du nom de la PJ à partir des ID, et vérifications de sécurité
                    // Avec renvoi du message d'erreur si nécessaire
                    ePJ myPJ = null;
                    string sFileName = ePJ.GetNamePJ(_pref, nPJId, nDescId, nFileId, out myPJ);
                    if (myPJ != null && myPJ.ErrorContainer != null)
                        LaunchError(myPJ.ErrorContainer);

                    eLibConst.FOLDER_TYPE folder = eLibConst.FOLDER_TYPE.ANNEXES;
                    if (nPJType == PjType.REPORTS.GetHashCode())
                        folder = eLibConst.FOLDER_TYPE.REPORTS;

                    bool bIsURLPJ = (nPJType == PjType.FTP.GetHashCode() || nPJType == PjType.MAIL.GetHashCode() || nPJType == PjType.WEB.GetHashCode());
                    string sFilePath = String.Empty;
                    StringBuilder sbPath = new StringBuilder();

                    if (bIsURLPJ)
                    {
                        if (nPJType == PjType.FTP.GetHashCode())
                        {
                            if (!sFileName.StartsWith("ftp://"))
                                sbPath.Append("ftp://");
                        }
                        if (nPJType == PjType.MAIL.GetHashCode())
                        {
                            if (!sFileName.StartsWith("mailto:"))
                                sbPath.Append("mailto:");
                        }
                        if (nPJType == PjType.WEB.GetHashCode())
                        {
                            if (!sFileName.StartsWith("http://") && !sFileName.StartsWith("https://") && !sFileName.StartsWith("ftp://") && !sFileName.StartsWith("mailto:"))
                                sbPath.Append("http://");
                        }
                        sbPath.Append(sFileName);
                        bCanAccessImage = true;
                    }
                    else
                    {
                        sbPath.Append(eLibTools.GetWebDatasPath(folder, _pref.GetBaseName));
                        if (sbPath.ToString().ToCharArray()[sbPath.Length - 1] != '/')
                            sbPath.Append("/");
                        // Chemin complet du fichier sur le système de fichiers
                        sFilePath = HttpContext.Current.Server.MapPath(String.Concat(sbPath.ToString(), sFileName));
                        bCanAccessImage = File.Exists(sFilePath);
                        //Url encodé (qui n'est utilisable que pour le chemin web)
                        sbPath.Append(Uri.EscapeDataString(sFileName));
                    }
                    strImageURL = sbPath.ToString();
                    break;

                case eLibConst.IMAGE_TYPE.IMAGE_FIELD:
                case eLibConst.IMAGE_TYPE.AVATAR_FIELD:
                case eLibConst.IMAGE_TYPE.AVATAR:
                case eLibConst.IMAGE_TYPE.USER_AVATAR_FIELD:
                default:



                    #region Génération du fichier si FileId > 0
                    if (nFileId > 0 && !bForceFromSession)
                    {
                        if (imageType != eLibConst.IMAGE_TYPE.IMAGE_FIELD)
                        {
                            bCanAccessImage = true;
                            image = eAvatar.GetAvatar(_pref,
                                (imageType == eLibConst.IMAGE_TYPE.AVATAR) ? TableType.USER.GetHashCode() : (nDescId / 100) * 100, nFileId);
                            strImageURL = image.Load().ToString();
                        }
                        else
                        {
                            Field fieldInfo = new Field();
                            eFieldRecord fieldRecord = new eFieldRecord();
                            // ASY : pour la duplication unitaire, on a besoin du fileId de la fiche source pour récupérer son image
                            object oImageData = null;
                            if (nFileId0 > 0)
                                oImageData = eLibTools.GetDBImageData(_pref, nDescId, nFileId0, out fieldInfo, out fieldRecord, out bCanAccessImage, out strError);
                            else
                                oImageData = eLibTools.GetDBImageData(_pref, nDescId, nFileId, out fieldInfo, out fieldRecord, out bCanAccessImage, out strError);

                            if (strError.Length == 0 && bCanAccessImage)
                            {
                                if (fieldInfo != null)
                                {
                                    switch (fieldInfo.ImgStorage)
                                    {
                                        case ImageStorage.STORE_IN_DATABASE:
                                            bmpImageBitmap = eImageTools.ConvertDBImage(_pref, oImageData, nWidth, nHeight, imageFormat, bGenericImageOnly, out strError);
                                            break;
                                        case ImageStorage.STORE_IN_FILE:
                                            if (nWidth == 0 && nHeight == 0)
                                                strImageURL = String.Concat(eLibTools.GetAppUrl(HttpContext.Current.Request), "/", eLibTools.GetWebDatasPath(targetFolderType, _pref.GetBaseName), "/", Path.GetFileName(oImageData.ToString()));
                                            else
                                                // Génération d'une version miniature de l'image
                                                bmpImageBitmap = eImageTools.GetThumbnail(String.Concat(eModelTools.GetPhysicalDatasPath(targetFolderType, _pref), @"\", Path.GetFileName(oImageData.ToString())), nWidth, nHeight);
                                            break;
                                        case ImageStorage.STORE_IN_URL:
                                            strImageURL = oImageData.ToString();
                                            break;
                                    }
                                }
                            }
                        }


                    }
                    #endregion

                    #region Si FileId < 0, on renvoie l'objet image stocké temporairement en session s'il existe
                    else
                    {
                        if (Session[TempImageSessionObjName] != null)
                        {
                            if (Session[TempImageSessionObjName] is byte[])
                            {
                                bmpImageBitmap = eImageTools.ConvertDBImage(_pref, Session[TempImageSessionObjName], nWidth, nHeight, imageFormat, bGenericImageOnly, out strError);
                            }
                            else if (Session[TempImageSessionObjName] is Uri)
                            {
                                strImageURL = ((Uri)Session[TempImageSessionObjName]).ToString();
                            }

                            bCanAccessImage = true;
                        }
                    }
                    #endregion

                    break;
            }

            if (strError.Length > 0)
            {
                String sDevMsg = String.Concat("Erreur sur la page : ", System.Web.HttpContext.Current.Request.Url.Segments[System.Web.HttpContext.Current.Request.Url.Segments.Length - 1], Environment.NewLine);
                sDevMsg = String.Concat(sDevMsg, Environment.NewLine, "Erreur Message : ", strError.ToString(), Environment.NewLine);

                ErrorContainer = eErrorContainer.GetDevUserError(
                   eLibConst.MSG_TYPE.CRITICAL,
                   eResApp.GetRes(_pref, 72),   // Message En-tête : Une erreur est survenue
                   String.Concat(eResApp.GetRes(_pref, 2024).Replace("<PARAM>", "")), // Paramètres invalides
                   eResApp.GetRes(_pref, 72),  //   Titre
                   String.Concat(sDevMsg, Environment.NewLine, "Erreur lors de la génération de l'image - ", strError));

                LaunchError();
            }
            else if (!bCanAccessImage)
            {
                strImageURL = String.Concat("themes/", _pref.ThemePaths.GetDefaultImageWebPath());
            }

            Response.Clear();
            Response.ClearContent();
            Response.ClearHeaders();

            if (bmpImageBitmap != null)
            {
                Response.ContentType = strContentType;
                bmpImageBitmap.Save(Response.OutputStream, imageFormat);
                bmpImageBitmap.Dispose();
            }
            else if (strImageURL.Length > 0)
            {
                // On redirige vers l'URL du vrai fichier si aucune transformation (redimensionnement) n'a été demandée
                // Toutefois, il faut indiquer au navigateur de ne pas tenir compte du cache (sinon, il afficherait l'ancienne image si cette dernière vient d'être remplacée
                // via eImageDialog.aspx)
                Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
                Response.AppendHeader("Expires", "0"); // Proxies.
                Response.Redirect(strImageURL);
            }
            #endregion
        }
    }
}